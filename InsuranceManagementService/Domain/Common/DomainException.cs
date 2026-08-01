namespace InsuranceManagementService.Domain.Common;

/// <summary>
/// Raised when a domain invariant or business rule is violated.
/// Surfaced by the API as a 409/422 rather than a 500.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
