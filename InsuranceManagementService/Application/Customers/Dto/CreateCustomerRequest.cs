using System.ComponentModel.DataAnnotations;

namespace InsuranceManagementService.Application.Customers.Dto;

public sealed record CreateCustomerRequest(
    string FullName,
    string NationalId,
    string Email,
    string? PhoneNumber,
    AddressDto? Address) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            yield return new ValidationResult("FullName is required.", new[] { nameof(FullName) });
        }
        else if (FullName.Length > 200)
        {
            yield return new ValidationResult("FullName must be at most 200 characters.", new[] { nameof(FullName) });
        }

        if (string.IsNullOrWhiteSpace(NationalId))
        {
            yield return new ValidationResult("NationalId is required.", new[] { nameof(NationalId) });
        }
        else if (NationalId.Length > 64)
        {
            yield return new ValidationResult("NationalId must be at most 64 characters.", new[] { nameof(NationalId) });
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult("Email is required.", new[] { nameof(Email) });
        }
        else if (!new EmailAddressAttribute().IsValid(Email))
        {
            yield return new ValidationResult("Email must be a valid email address.", new[] { nameof(Email) });
        }
        else if (Email.Length > 256)
        {
            yield return new ValidationResult("Email must be at most 256 characters.", new[] { nameof(Email) });
        }

        if (PhoneNumber is { Length: > 32 })
        {
            yield return new ValidationResult("PhoneNumber must be at most 32 characters.", new[] { nameof(PhoneNumber) });
        }
    }
}
