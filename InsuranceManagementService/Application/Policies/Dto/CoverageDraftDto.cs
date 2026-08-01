using System.ComponentModel.DataAnnotations;
using InsuranceManagementService.Application.Common;

namespace InsuranceManagementService.Application.Policies.Dto;

public sealed record CoverageDraftDto(string Type, MoneyDto Limit, MoneyDto Deductible) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Type))
        {
            yield return new ValidationResult("Type is required.", new[] { nameof(Type) });
        }
        else if (Type.Length > 120)
        {
            yield return new ValidationResult("Type must be at most 120 characters.", new[] { nameof(Type) });
        }

        if (Limit is null)
        {
            yield return new ValidationResult("Limit is required.", new[] { nameof(Limit) });
        }

        if (Deductible is null)
        {
            yield return new ValidationResult("Deductible is required.", new[] { nameof(Deductible) });
        }

        if (Limit is not null && Deductible is not null && Deductible.Amount > Limit.Amount)
        {
            yield return new ValidationResult(
                "Coverage deductible cannot exceed its limit.",
                new[] { nameof(Deductible) });
        }
    }
}
