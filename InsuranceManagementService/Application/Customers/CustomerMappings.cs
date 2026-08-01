using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies;
using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Application.Customers;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer customer, DateTime nowUtc) => new(
        customer.Id,
        customer.FullName,
        customer.NationalId,
        customer.Email,
        customer.PhoneNumber,
        customer.Address.ToDto(),
        customer.Status.ToString(),
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc,
        customer.Policies.Select(p => p.ToSummary(nowUtc)).ToList());

    public static CustomerListItemResponse ToListItem(this Customer customer) => new(
        customer.Id,
        customer.FullName,
        customer.NationalId,
        customer.Email,
        customer.PhoneNumber,
        customer.Status.ToString(),
        customer.CreatedAtUtc);

    public static AddressDto? ToDto(this Address? address) => address is null
        ? null
        : new AddressDto(address.Line1, address.Line2, address.City, address.State, address.PostalCode, address.Country);

    public static Address? ToDomain(this AddressDto? dto) => dto is null
        ? null
        : new Address(dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Country);
}
