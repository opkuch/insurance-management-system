using InsuranceManagementService.Application.Common;

namespace InsuranceManagementService.Application.Policies.Dto;

public sealed record CoverageResponse(Guid Id, string Type, MoneyDto Limit, MoneyDto Deductible);
