namespace InsuranceManagementService.Application.Common;

/// <summary>
/// Raised when a request conflicts with the current state of a resource, such as
/// violating a uniqueness constraint. Mapped to HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
