namespace InsuranceManagementService.Application.Policies.Dto;

public sealed record PolicyTransactionResponse(
    Guid Id,
    string Type,
    DateTime EffectiveDateUtc,
    string? PreviousStatus,
    string NewStatus,
    string Notes,
    string CreatedBy,
    DateTime CreatedAtUtc);
