using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// A policyholder (the person the agent does business with). Aggregate root that
/// owns the relationship to its policies.
/// </summary>
public class Customer : Entity
{
    private readonly List<Policy> _policies = new();

    private Customer()
    {
    }

    public string FullName { get; private set; } = string.Empty;

    /// <summary>Government identity document number; unique across customers.</summary>
    public string NationalId { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public Address? Address { get; private set; }

    public CustomerStatus Status { get; private set; }

    public IReadOnlyCollection<Policy> Policies => _policies.AsReadOnly();

    public bool IsActive => Status == CustomerStatus.Active;

    public static Customer Create(
        string fullName,
        string nationalId,
        string email,
        string? phoneNumber,
        Address? address)
    {
        return new Customer
        {
            FullName = Required(fullName, "Full name"),
            NationalId = Required(nationalId, "National ID"),
            Email = ValidateEmail(email),
            PhoneNumber = NormalizePhone(phoneNumber),
            Address = address,
            Status = CustomerStatus.Active,
        };
    }

    public void UpdateContactDetails(string email, string? phoneNumber, Address? address)
    {
        Email = ValidateEmail(email);
        PhoneNumber = NormalizePhone(phoneNumber);
        Address = address;
    }

    public void Rename(string fullName) => FullName = Required(fullName, "Full name");

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
        {
            throw new DomainException("Customer is already inactive.");
        }

        Status = CustomerStatus.Inactive;
    }

    public void Activate()
    {
        if (Status == CustomerStatus.Active)
        {
            throw new DomainException("Customer is already active.");
        }

        Status = CustomerStatus.Active;
    }

    private static string Required(string value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{field} is required.")
            : value.Trim();

    private static string? NormalizePhone(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        email = email.Trim();
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1 || !email[(atIndex + 1)..].Contains('.'))
        {
            throw new DomainException("Email is not in a valid format.");
        }

        return email;
    }
}

/// <summary>
/// Whether a customer can have new policies issued to them.
/// </summary>
public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
}
