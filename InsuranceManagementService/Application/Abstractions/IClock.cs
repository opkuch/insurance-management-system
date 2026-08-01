namespace InsuranceManagementService.Application.Abstractions;

/// <summary>
/// Abstraction over the system clock so time-dependent logic (lifecycle,
/// timestamps) is testable.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
