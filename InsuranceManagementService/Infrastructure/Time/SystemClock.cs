using InsuranceManagementService.Application.Abstractions;

namespace InsuranceManagementService.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
