using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Abstractions;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, bool includePolicies = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);

    Task<PagedResult<Customer>> ListAsync(
        string? search,
        CustomerStatus? status,
        PageRequest page,
        CancellationToken cancellationToken = default);
}
