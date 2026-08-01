namespace InsuranceManagementService.Application.Customers.Dto;

/// <summary>Row shape returned when listing customers.</summary>
public sealed record CustomerListItemResponse(
    Guid Id,
    string FullName,
    string NationalId,
    string Email,
    string? PhoneNumber,
    string Status,
    DateTime CreatedAtUtc);
