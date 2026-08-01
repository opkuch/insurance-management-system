using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace InsuranceManagementService.Application.Policies;

public sealed class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policies;
    private readonly ICustomerRepository _customers;
    private readonly IPolicyNumberGenerator _policyNumbers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(
        IPolicyRepository policies,
        ICustomerRepository customers,
        IPolicyNumberGenerator policyNumbers,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<PolicyService> logger)
    {
        _policies = policies;
        _customers = customers;
        _policyNumbers = policyNumbers;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PolicyDetailResponse> IssueAsync(Guid customerId, IssuePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(customerId, includePolicies: false, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

        if (!customer.IsActive)
        {
            throw new ConflictException("A policy cannot be issued to an inactive customer.");
        }

        var now = _clock.UtcNow;
        var term = new DateRange(request.TermStartUtc, request.TermEndUtc);
        var premium = new Money(request.Premium.Amount, request.Premium.Currency);
        var drafts = request.Coverages
            .Select(c => new CoverageDraft(
                c.Type,
                new Money(c.Limit.Amount, c.Limit.Currency),
                new Money(c.Deductible.Amount, c.Deductible.Currency)))
            .ToList();

        Policy? policy = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var number = await _policyNumbers.NextAsync(request.ProductType, now.Year, ct);

            policy = Policy.Issue(
                number,
                customerId,
                request.ProductType,
                term,
                premium,
                request.BillingFrequency,
                drafts,
                now);

            await _policies.AddAsync(policy, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            "Issued policy {PolicyNumber} ({PolicyId}) for customer {CustomerId}",
            policy!.Number.Value,
            policy.Id,
            customerId);

        return policy.ToDetail(now);
    }

    public async Task<PolicyDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _policies.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Policy", id);

        var now = _clock.UtcNow;

        // Materialize stored status + Expired audit; API status still comes from CurrentStatus.
        if (policy.RefreshLifecycle(now))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return policy.ToDetail(now);
    }

    public async Task<PolicyDetailResponse> EndorseAsync(Guid id, EndorsePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await _policies.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Policy", id);

        var now = _clock.UtcNow;
        policy.RefreshLifecycle(now);

        var premium = request.Premium is null
            ? null
            : new Money(request.Premium.Amount, request.Premium.Currency);

        var drafts = request.Coverages?
            .Select(c => new CoverageDraft(
                c.Type,
                new Money(c.Limit.Amount, c.Limit.Currency),
                new Money(c.Deductible.Amount, c.Deductible.Currency)))
            .ToList();

        policy.Endorse(premium, drafts, request.Notes, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Endorsed policy {PolicyNumber} ({PolicyId}) to version {Version} for customer {CustomerId}",
            policy.Number.Value,
            policy.Id,
            policy.Version,
            policy.CustomerId);

        return policy.ToDetail(now);
    }

    public async Task<PolicyDetailResponse> CancelAsync(Guid id, CancelPolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await _policies.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Policy", id);

        var now = _clock.UtcNow;
        policy.RefreshLifecycle(now);

        policy.Cancel(request.EffectiveDateUtc ?? now, request.Reason, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled policy {PolicyNumber} ({PolicyId}) for customer {CustomerId}",
            policy.Number.Value,
            policy.Id,
            policy.CustomerId);

        return policy.ToDetail(now);
    }

    public async Task<int> RunLifecycleSweepAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var policies = await _policies.ListPendingLifecycleAsync(now, cancellationToken);

        var changed = 0;
        foreach (var policy in policies)
        {
            if (policy.RefreshLifecycle(now))
            {
                changed++;
            }
        }

        if (changed > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return changed;
    }

    public async Task<PagedResult<PolicySummaryResponse>> ListAsync(
        ProductType? productType,
        PolicyStatus? status,
        Guid? customerId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var result = await _policies.ListAsync(productType, status, customerId, now, page, cancellationToken);
        var items = result.Items.Select(p => p.ToSummary(now)).ToList();
        return new PagedResult<PolicySummaryResponse>(items, result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<IReadOnlyList<PolicySummaryResponse>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _ = await _customers.GetByIdAsync(customerId, includePolicies: false, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

        var now = _clock.UtcNow;
        var policies = await _policies.ListByCustomerAsync(customerId, cancellationToken);
        return policies.Select(p => p.ToSummary(now)).ToList();
    }
}
