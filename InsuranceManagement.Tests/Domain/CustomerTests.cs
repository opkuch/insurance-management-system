using FluentAssertions;
using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Domain;

public class CustomerTests
{
    private static Customer NewCustomer() =>
        Customer.Create("Jane Doe", "ID-1", "jane@example.com", null, null);

    [Fact]
    public void Create_starts_active_and_trims_input()
    {
        var customer = Customer.Create("  Jane Doe  ", " ID-1 ", "jane@example.com", "  ", null);

        customer.Status.Should().Be(CustomerStatus.Active);
        customer.FullName.Should().Be("Jane Doe");
        customer.NationalId.Should().Be("ID-1");
        customer.PhoneNumber.Should().BeNull();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("")]
    public void Create_rejects_invalid_email(string email)
    {
        var act = () => Customer.Create("Jane", "ID-1", email, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivating_twice_is_rejected()
    {
        var customer = NewCustomer();
        customer.Deactivate();

        var act = () => customer.Deactivate();

        act.Should().Throw<DomainException>();
    }
}
