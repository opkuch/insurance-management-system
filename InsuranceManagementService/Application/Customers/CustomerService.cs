using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Application.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CustomerService(ICustomerRepository customers, IUnitOfWork unitOfWork, IClock clock)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var nationalId = request.NationalId.Trim();
        if (await _customers.ExistsByNationalIdAsync(nationalId, cancellationToken))
        {
            throw new ConflictException($"A customer with national ID '{nationalId}' already exists.");
        }

        var customer = Customer.Create(
            request.FullName,
            nationalId,
            request.Email,
            request.PhoneNumber,
            request.Address.ToDomain());

        await _customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToResponse(_clock.UtcNow);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(id, includePolicies: true, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        customer.Rename(request.FullName);
        customer.UpdateContactDetails(request.Email, request.PhoneNumber, request.Address.ToDomain());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToResponse(_clock.UtcNow);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(id, includePolicies: true, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        return customer.ToResponse(_clock.UtcNow);
    }

    public async Task<PagedResult<CustomerListItemResponse>> ListAsync(
        string? search,
        CustomerStatus? status,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var result = await _customers.ListAsync(search, status, page, cancellationToken);
        var items = result.Items.Select(c => c.ToListItem()).ToList();
        return new PagedResult<CustomerListItemResponse>(items, result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<CustomerResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(id, includePolicies: true, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        customer.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToResponse(_clock.UtcNow);
    }

    public async Task<CustomerResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(id, includePolicies: true, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        customer.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToResponse(_clock.UtcNow);
    }
}
