using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Customers;

public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<CustomerListItemResponse>> ListAsync(
        string? search,
        CustomerStatus? status,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CustomerResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
