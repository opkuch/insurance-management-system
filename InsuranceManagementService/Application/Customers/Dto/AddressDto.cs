using System.ComponentModel.DataAnnotations;

namespace InsuranceManagementService.Application.Customers.Dto;

public sealed record AddressDto(
    string Line1,
    string? Line2,
    string City,
    string? State,
    string PostalCode,
    string Country) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Line1))
        {
            yield return new ValidationResult("Line1 is required.", new[] { nameof(Line1) });
        }
        else if (Line1.Length > 200)
        {
            yield return new ValidationResult("Line1 must be at most 200 characters.", new[] { nameof(Line1) });
        }

        if (Line2 is { Length: > 200 })
        {
            yield return new ValidationResult("Line2 must be at most 200 characters.", new[] { nameof(Line2) });
        }

        if (string.IsNullOrWhiteSpace(City))
        {
            yield return new ValidationResult("City is required.", new[] { nameof(City) });
        }
        else if (City.Length > 120)
        {
            yield return new ValidationResult("City must be at most 120 characters.", new[] { nameof(City) });
        }

        if (State is { Length: > 120 })
        {
            yield return new ValidationResult("State must be at most 120 characters.", new[] { nameof(State) });
        }

        if (string.IsNullOrWhiteSpace(PostalCode))
        {
            yield return new ValidationResult("PostalCode is required.", new[] { nameof(PostalCode) });
        }
        else if (PostalCode.Length > 32)
        {
            yield return new ValidationResult("PostalCode must be at most 32 characters.", new[] { nameof(PostalCode) });
        }

        if (string.IsNullOrWhiteSpace(Country))
        {
            yield return new ValidationResult("Country is required.", new[] { nameof(Country) });
        }
        else if (Country.Length > 120)
        {
            yield return new ValidationResult("Country must be at most 120 characters.", new[] { nameof(Country) });
        }
    }
}
