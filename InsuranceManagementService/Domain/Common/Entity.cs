namespace InsuranceManagementService.Domain.Common;

/// <summary>
/// Base type for all domain entities. Provides identity and audit timestamps.
/// Timestamps are populated centrally by the persistence layer on save.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    internal void StampCreated(DateTime utcNow) => CreatedAtUtc = utcNow;

    internal void StampUpdated(DateTime utcNow) => UpdatedAtUtc = utcNow;
}
