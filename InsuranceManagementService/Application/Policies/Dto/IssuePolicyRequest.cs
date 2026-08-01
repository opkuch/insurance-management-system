using System.ComponentModel.DataAnnotations;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Policies.Dto;

public sealed record IssuePolicyRequest(
    ProductType ProductType,
    DateTime TermStartUtc,
    DateTime TermEndUtc,
    Common.MoneyDto Premium,
    BillingFrequency BillingFrequency,
    IReadOnlyList<CoverageDraftDto> Coverages) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(typeof(ProductType), ProductType))
        {
            yield return new ValidationResult("ProductType is not a recognised value.", new[] { nameof(ProductType) });
        }

        if (!Enum.IsDefined(typeof(BillingFrequency), BillingFrequency))
        {
            yield return new ValidationResult("BillingFrequency is not a recognised value.", new[] { nameof(BillingFrequency) });
        }

        if (TermEndUtc <= TermStartUtc)
        {
            yield return new ValidationResult("Term end date must be after the term start date.", new[] { nameof(TermEndUtc) });
        }

        if (Premium is null)
        {
            yield return new ValidationResult("Premium is required.", new[] { nameof(Premium) });
        }
        else if (Premium.Amount <= 0)
        {
            yield return new ValidationResult("Premium must be greater than zero.", new[] { nameof(Premium) });
        }

        if (Coverages is null || Coverages.Count == 0)
        {
            yield return new ValidationResult("A policy must have at least one coverage.", new[] { nameof(Coverages) });
        }
    }
}
