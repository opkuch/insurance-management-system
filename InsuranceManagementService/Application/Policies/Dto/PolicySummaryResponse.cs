using InsuranceManagementService.Application.Common;

namespace InsuranceManagementService.Application.Policies.Dto;

/// <summary>Lightweight view of a policy for listings and customer summaries.</summary>
public sealed record PolicySummaryResponse(
    Guid Id,
    string PolicyNumber,
    string ProductType,
    string Status,
    MoneyDto Premium,
    string BillingFrequency,
    DateTime TermStartUtc,
    DateTime TermEndUtc,
    int Version);
