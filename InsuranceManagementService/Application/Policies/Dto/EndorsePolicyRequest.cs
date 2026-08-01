using System.ComponentModel.DataAnnotations;
using InsuranceManagementService.Application.Common;

namespace InsuranceManagementService.Application.Policies.Dto;

/// <summary>
/// A mid-term change to a policy. At least one of <see cref="Premium"/> or
/// <see cref="Coverages"/> must be supplied. When coverages are supplied they
/// fully replace the existing set.
/// </summary>
public sealed record EndorsePolicyRequest(
    MoneyDto? Premium,
    IReadOnlyList<CoverageDraftDto>? Coverages,
    string? Notes) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Premium is null && Coverages is null)
        {
            yield return new ValidationResult("An endorsement must change the premium and/or the coverages.");
        }

        if (Premium is not null && Premium.Amount <= 0)
        {
            yield return new ValidationResult("Premium must be greater than zero.", new[] { nameof(Premium) });
        }

        if (Coverages is not null && Coverages.Count == 0)
        {
            yield return new ValidationResult("An endorsement cannot remove all coverages.", new[] { nameof(Coverages) });
        }

        if (Notes is { Length: > 500 })
        {
            yield return new ValidationResult("Notes must be at most 500 characters.", new[] { nameof(Notes) });
        }
    }
}
