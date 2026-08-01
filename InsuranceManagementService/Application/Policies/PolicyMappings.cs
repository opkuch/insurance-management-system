using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Policies;

public static class PolicyMappings
{
    public static PolicySummaryResponse ToSummary(this Policy policy, DateTime nowUtc) => new(
        policy.Id,
        policy.Number.Value,
        policy.ProductType.ToString(),
        policy.CurrentStatus(nowUtc).ToString(),
        new MoneyDto(policy.Premium.Amount, policy.Premium.Currency),
        policy.BillingFrequency.ToString(),
        policy.Term.Start,
        policy.Term.End,
        policy.Version);

    public static PolicyDetailResponse ToDetail(this Policy policy, DateTime nowUtc) => new(
        policy.Id,
        policy.Number.Value,
        policy.CustomerId,
        policy.ProductType.ToString(),
        policy.CurrentStatus(nowUtc).ToString(),
        policy.Term.Start,
        policy.Term.End,
        new MoneyDto(policy.Premium.Amount, policy.Premium.Currency),
        policy.BillingFrequency.ToString(),
        policy.Version,
        policy.CancellationReason,
        policy.CancelledAtUtc,
        policy.CreatedAtUtc,
        policy.UpdatedAtUtc,
        policy.Coverages.Select(c => c.ToResponse()).ToList(),
        policy.Transactions
            .OrderBy(t => t.CreatedAtUtc)
            .Select(t => t.ToResponse())
            .ToList());

    public static CoverageResponse ToResponse(this Coverage coverage) => new(
        coverage.Id,
        coverage.Type,
        new MoneyDto(coverage.Limit.Amount, coverage.Limit.Currency),
        new MoneyDto(coverage.Deductible.Amount, coverage.Deductible.Currency));

    public static PolicyTransactionResponse ToResponse(this PolicyTransaction transaction) => new(
        transaction.Id,
        transaction.Type.ToString(),
        transaction.EffectiveDateUtc,
        transaction.PreviousStatus?.ToString(),
        transaction.NewStatus.ToString(),
        transaction.Notes,
        transaction.CreatedBy,
        transaction.CreatedAtUtc);
}
