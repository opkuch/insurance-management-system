using System.ComponentModel.DataAnnotations;

namespace InsuranceManagementService.Application.Common;

/// <summary>Transport representation of a monetary amount.</summary>
public sealed record MoneyDto(decimal Amount, string Currency) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount < 0)
        {
            yield return new ValidationResult("Amount must be zero or greater.", new[] { nameof(Amount) });
        }

        if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3)
        {
            yield return new ValidationResult(
                "Currency must be a 3-letter ISO code (e.g. USD).",
                new[] { nameof(Currency) });
        }
    }
}
