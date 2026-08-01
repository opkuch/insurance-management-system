using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// A specific protection provided by a policy, with a limit and a deductible.
/// Child entity of the Policy aggregate.
/// </summary>
public class Coverage : Entity
{
    private Coverage()
    {
    }

    internal Coverage(string type, Money limit, Money deductible)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException("Coverage type is required.");
        }

        ArgumentNullException.ThrowIfNull(limit);
        ArgumentNullException.ThrowIfNull(deductible);

        if (deductible.Currency != limit.Currency)
        {
            throw new DomainException("Coverage limit and deductible must use the same currency.");
        }

        if (deductible.Amount > limit.Amount)
        {
            throw new DomainException("Coverage deductible cannot exceed its limit.");
        }

        Type = type.Trim();
        Limit = limit;
        Deductible = deductible;
    }

    public Guid PolicyId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public Money Limit { get; private set; } = null!;

    public Money Deductible { get; private set; } = null!;
}
