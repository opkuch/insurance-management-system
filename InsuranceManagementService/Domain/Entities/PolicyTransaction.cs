using InsuranceManagementService.Domain.Common;

namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// An immutable audit record of a single lifecycle event on a policy. Together
/// these form the policy's full history.
/// </summary>
public class PolicyTransaction : Entity
{
    private PolicyTransaction()
    {
    }

    internal PolicyTransaction(
        Guid policyId,
        PolicyTransactionType type,
        DateTime effectiveDateUtc,
        PolicyStatus? previousStatus,
        PolicyStatus newStatus,
        string notes,
        string createdBy)
    {
        PolicyId = policyId;
        Type = type;
        EffectiveDateUtc = DateTime.SpecifyKind(effectiveDateUtc, DateTimeKind.Utc);
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy.Trim();
    }

    public Guid PolicyId { get; private set; }

    public PolicyTransactionType Type { get; private set; }

    /// <summary>When the event takes effect (may differ from when it was recorded).</summary>
    public DateTime EffectiveDateUtc { get; private set; }

    public PolicyStatus? PreviousStatus { get; private set; }

    public PolicyStatus NewStatus { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = "system";
}
