using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// The central insurance contract. A versioned aggregate root that owns its
/// coverages and an append-only transaction log, and enforces the lifecycle
/// state machine (Issued -> Active -> Cancelled/Expired).
/// </summary>
public class Policy : Entity
{
    private readonly List<Coverage> _coverages = new();
    private readonly List<PolicyTransaction> _transactions = new();

    private Policy()
    {
    }

    private Policy(
        PolicyNumber number,
        Guid customerId,
        ProductType productType,
        DateRange term,
        Money premium,
        BillingFrequency billingFrequency)
    {
        Number = number;
        CustomerId = customerId;
        ProductType = productType;
        Term = term;
        Premium = premium;
        BillingFrequency = billingFrequency;
        Version = 1;
        Status = PolicyStatus.Issued;
    }

    public PolicyNumber Number { get; private set; } = null!;

    public Guid CustomerId { get; private set; }

    public ProductType ProductType { get; private set; }

    public PolicyStatus Status { get; private set; }

    public DateRange Term { get; private set; } = null!;

    public Money Premium { get; private set; } = null!;

    public BillingFrequency BillingFrequency { get; private set; }

    /// <summary>Incremented on every endorsement so history is traceable.</summary>
    public int Version { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<Coverage> Coverages => _coverages.AsReadOnly();

    public IReadOnlyCollection<PolicyTransaction> Transactions => _transactions.AsReadOnly();

    /// <summary>
    /// Derives the effective status from cancellation and term dates.
    /// Stored <see cref="Status"/> is a materialization cache; this is the read/filter source of truth.
    /// </summary>
    public PolicyStatus CurrentStatus(DateTime nowUtc)
    {
        if (Status == PolicyStatus.Cancelled)
        {
            return PolicyStatus.Cancelled;
        }

        if (Term.HasEnded(nowUtc))
        {
            return PolicyStatus.Expired;
        }

        if (Term.HasStarted(nowUtc))
        {
            return PolicyStatus.Active;
        }

        return PolicyStatus.Issued;
    }

    /// <summary>
    /// Issues a new policy to a customer. The policy is created Active if its term
    /// has already started, otherwise Issued (pending the effective date).
    /// </summary>
    public static Policy Issue(
        PolicyNumber number,
        Guid customerId,
        ProductType productType,
        DateRange term,
        Money premium,
        BillingFrequency billingFrequency,
        IEnumerable<CoverageDraft> coverages,
        DateTime nowUtc,
        string issuedBy = "system")
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(term);
        ArgumentNullException.ThrowIfNull(premium);

        if (customerId == Guid.Empty)
        {
            throw new DomainException("A policy must be linked to a customer.");
        }

        if (premium.Amount <= 0)
        {
            throw new DomainException("Premium must be greater than zero.");
        }

        var drafts = coverages?.ToList() ?? new List<CoverageDraft>();
        if (drafts.Count == 0)
        {
            throw new DomainException("A policy must have at least one coverage.");
        }

        EnsureConsistentCurrency(premium.Currency, drafts);

        var policy = new Policy(number, customerId, productType, term, premium, billingFrequency);
        foreach (var draft in drafts)
        {
            policy._coverages.Add(new Coverage(draft.Type, draft.Limit, draft.Deductible));
        }

        policy.Status = term.HasStarted(nowUtc) ? PolicyStatus.Active : PolicyStatus.Issued;
        policy.AddTransaction(
            PolicyTransactionType.Issued,
            term.Start,
            previousStatus: null,
            policy.Status,
            $"Policy issued with {drafts.Count} coverage(s).",
            issuedBy);

        return policy;
    }

    /// <summary>
    /// Applies a mid-term change: optionally replaces coverages and/or the premium.
    /// Produces a new version and an audit entry. Not allowed once terminated.
    /// </summary>
    public void Endorse(
        Money? newPremium,
        IEnumerable<CoverageDraft>? newCoverages,
        string? notes,
        DateTime nowUtc,
        string endorsedBy = "system")
    {
        EnsureModifiable("endorsed", nowUtc);

        if (newPremium is null && newCoverages is null)
        {
            throw new DomainException("An endorsement must change the premium and/or the coverages.");
        }

        var targetCurrency = newPremium?.Currency ?? Premium.Currency;

        if (newCoverages is not null)
        {
            var drafts = newCoverages.ToList();
            if (drafts.Count == 0)
            {
                throw new DomainException("An endorsement cannot remove all coverages.");
            }

            EnsureConsistentCurrency(targetCurrency, drafts);

            _coverages.Clear();
            foreach (var draft in drafts)
            {
                _coverages.Add(new Coverage(draft.Type, draft.Limit, draft.Deductible));
            }
        }

        if (newPremium is not null)
        {
            if (newPremium.Amount <= 0)
            {
                throw new DomainException("Premium must be greater than zero.");
            }

            if (_coverages.Any(c => c.Limit.Currency != newPremium.Currency))
            {
                throw new DomainException("All monetary amounts on a policy must use the same currency.");
            }

            Premium = newPremium;
        }

        var current = CurrentStatus(nowUtc);
        Version += 1;
        AddTransaction(
            PolicyTransactionType.Endorsed,
            nowUtc,
            previousStatus: current,
            current,
            notes ?? "Policy endorsed.",
            endorsedBy);
    }

    /// <summary>Terminates coverage before the natural end of the term.</summary>
    public void Cancel(DateTime effectiveDateUtc, string reason, DateTime nowUtc, string cancelledBy = "system")
    {
        var current = CurrentStatus(nowUtc);

        if (current == PolicyStatus.Cancelled)
        {
            throw new DomainException("Policy is already cancelled.");
        }

        if (current == PolicyStatus.Expired)
        {
            throw new DomainException("An expired policy cannot be cancelled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A cancellation reason is required.");
        }

        Status = PolicyStatus.Cancelled;
        CancelledAtUtc = nowUtc;
        CancellationReason = reason.Trim();

        AddTransaction(
            PolicyTransactionType.Cancelled,
            effectiveDateUtc,
            current,
            Status,
            reason.Trim(),
            cancelledBy);
    }

    /// <summary>
    /// Materializes stored <see cref="Status"/> to match <see cref="CurrentStatus"/>.
    /// Activation is a passive transition and is not audited; expiry appends an audit row.
    /// </summary>
    public bool RefreshLifecycle(DateTime nowUtc, string by = "system")
    {
        var derived = CurrentStatus(nowUtc);
        if (Status == derived)
        {
            return false;
        }

        var previous = Status;
        Status = derived;

        if (derived == PolicyStatus.Expired)
        {
            AddTransaction(
                PolicyTransactionType.Expired,
                Term.End,
                previous,
                Status,
                "Policy term ended.",
                by);
        }

        return true;
    }

    private void EnsureModifiable(string action, DateTime nowUtc)
    {
        var current = CurrentStatus(nowUtc);

        if (current == PolicyStatus.Cancelled)
        {
            throw new DomainException($"A cancelled policy cannot be {action}.");
        }

        if (current == PolicyStatus.Expired)
        {
            throw new DomainException($"An expired policy cannot be {action}.");
        }
    }

    private void AddTransaction(
        PolicyTransactionType type,
        DateTime effectiveDateUtc,
        PolicyStatus? previousStatus,
        PolicyStatus newStatus,
        string notes,
        string by)
    {
        _transactions.Add(new PolicyTransaction(Id, type, effectiveDateUtc, previousStatus, newStatus, notes, by));
    }

    private static void EnsureConsistentCurrency(string currency, IEnumerable<CoverageDraft> drafts)
    {
        foreach (var draft in drafts)
        {
            if (draft.Limit.Currency != currency || draft.Deductible.Currency != currency)
            {
                throw new DomainException("All monetary amounts on a policy must use the same currency.");
            }
        }
    }
}
