using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Domain.ValueObjects;

/// <summary>
/// A human-readable, unique, immutable policy identifier in the form
/// {LOB}-{YYYY}-{sequence:000000}, e.g. AUTO-2026-000123.
/// </summary>
public sealed class PolicyNumber : ValueObject
{
    private PolicyNumber()
    {
    }

    private PolicyNumber(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    /// <summary>Builds a policy number from its structured parts.</summary>
    public static PolicyNumber Create(ProductType productType, int year, long sequence)
    {
        if (sequence <= 0)
        {
            throw new DomainException("Policy number sequence must be positive.");
        }

        var prefix = productType.ToString().ToUpperInvariant();
        return new PolicyNumber($"{prefix}-{year:0000}-{sequence:000000}");
    }

    /// <summary>Rehydrates a policy number from an existing string value.</summary>
    public static PolicyNumber FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Policy number cannot be empty.");
        }

        return new PolicyNumber(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
