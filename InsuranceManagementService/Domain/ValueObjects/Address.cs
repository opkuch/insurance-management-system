using InsuranceManagementService.Domain.Common;

namespace InsuranceManagementService.Domain.ValueObjects;

/// <summary>
/// A postal address. Optional on a customer, but when supplied the essential
/// components must be present.
/// </summary>
public sealed class Address : ValueObject
{
    private Address()
    {
    }

    public Address(string line1, string? line2, string city, string? state, string postalCode, string country)
    {
        Line1 = Required(line1, "Address line 1");
        City = Required(city, "City");
        PostalCode = Required(postalCode, "Postal code");
        Country = Required(country, "Country");
        Line2 = Normalize(line2);
        State = Normalize(state);
    }

    public string Line1 { get; private set; } = string.Empty;

    public string? Line2 { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string? State { get; private set; }

    public string PostalCode { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    private static string Required(string value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{field} is required when an address is provided.")
            : value.Trim();

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
