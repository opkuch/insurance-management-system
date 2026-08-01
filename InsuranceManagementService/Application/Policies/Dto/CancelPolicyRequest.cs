using System.ComponentModel.DataAnnotations;

namespace InsuranceManagementService.Application.Policies.Dto;

/// <summary>
/// Cancels a policy before the end of its term. The effective date defaults to
/// now when not supplied.
/// </summary>
public sealed record CancelPolicyRequest(string Reason, DateTime? EffectiveDateUtc) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            yield return new ValidationResult("Reason is required.", new[] { nameof(Reason) });
        }
        else if (Reason.Length > 500)
        {
            yield return new ValidationResult("Reason must be at most 500 characters.", new[] { nameof(Reason) });
        }
    }
}
