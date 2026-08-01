using InsuranceManagementService.Application.Policies.Dto;

namespace InsuranceManagementService.Application.Customers.Dto;

public sealed record CustomerResponse(
    Guid Id,
    string FullName,
    string NationalId,
    string Email,
    string? PhoneNumber,
    AddressDto? Address,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<PolicySummaryResponse> Policies);
