using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// Input used to describe a coverage when issuing or endorsing a policy, before
/// it is materialized into a <see cref="Coverage"/> owned by the aggregate.
/// </summary>
public sealed record CoverageDraft(string Type, Money Limit, Money Deductible);
