using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Policies;

public interface IPolicyService
{
    Task<PolicyDetailResponse> IssueAsync(Guid customerId, IssuePolicyRequest request, CancellationToken cancellationToken = default);

    Task<PolicyDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<PolicySummaryResponse>> ListAsync(
        ProductType? productType,
        PolicyStatus? status,
        Guid? customerId,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicySummaryResponse>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<PolicyDetailResponse> EndorseAsync(Guid id, EndorsePolicyRequest request, CancellationToken cancellationToken = default);

    Task<PolicyDetailResponse> CancelAsync(Guid id, CancelPolicyRequest request, CancellationToken cancellationToken = default);

    /// <summary>Advances time-driven policy states (activation, expiry). Returns the number changed.</summary>
    Task<int> RunLifecycleSweepAsync(CancellationToken cancellationToken = default);
}
