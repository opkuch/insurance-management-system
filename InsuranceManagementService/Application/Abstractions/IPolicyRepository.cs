using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Abstractions;

public interface IPolicyRepository
{
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);

    /// <summary>Loads a policy with its coverages and transaction history.</summary>
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Policy>> ListAsync(
        ProductType? productType,
        PolicyStatus? status,
        Guid? customerId,
        DateTime nowUtc,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Policy>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns tracked policies whose time-driven state is stale (a pending policy
    /// whose term has started, or an in-force policy whose term has ended).
    /// </summary>
    Task<IReadOnlyList<Policy>> ListPendingLifecycleAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}
