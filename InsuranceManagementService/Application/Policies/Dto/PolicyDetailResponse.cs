using InsuranceManagementService.Application.Common;

namespace InsuranceManagementService.Application.Policies.Dto;

public sealed record PolicyDetailResponse(
    Guid Id,
    string PolicyNumber,
    Guid CustomerId,
    string ProductType,
    string Status,
    DateTime TermStartUtc,
    DateTime TermEndUtc,
    MoneyDto Premium,
    string BillingFrequency,
    int Version,
    string? CancellationReason,
    DateTime? CancelledAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<CoverageResponse> Coverages,
    IReadOnlyList<PolicyTransactionResponse> Transactions);
