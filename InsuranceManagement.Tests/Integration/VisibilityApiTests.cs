using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Integration;

public class VisibilityApiTests
{
    private static IssuePolicyRequest Policy(ProductType type) => new(
        type,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new MoneyDto(1000m, "USD"),
        BillingFrequency.Annual,
        new[] { new CoverageDraftDto("Base", new MoneyDto(10000m, "USD"), new MoneyDto(0m, "USD")) });

    [Fact]
    public async Task Policies_can_be_filtered_by_type_and_customer()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers",
                new CreateCustomerRequest("Filter Co", "F-1", "f@co.com", null, null), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        await client.PostAsJsonAsync($"/api/customers/{customer!.Id}/policies", Policy(ProductType.Auto), TestJson.Options);
        await client.PostAsJsonAsync($"/api/customers/{customer.Id}/policies", Policy(ProductType.Life), TestJson.Options);

        var autoOnly = await client.GetFromJsonAsync<PagedResult<PolicySummaryResponse>>(
            "/api/policies?productType=Auto", TestJson.Options);
        autoOnly!.TotalCount.Should().Be(1);
        autoOnly.Items.Should().OnlyContain(p => p.ProductType == "Auto");

        var byCustomer = await client.GetFromJsonAsync<PagedResult<PolicySummaryResponse>>(
            $"/api/policies?customerId={customer.Id}", TestJson.Options);
        byCustomer!.TotalCount.Should().Be(2);

        var book = await client.GetFromJsonAsync<List<PolicySummaryResponse>>(
            $"/api/customers/{customer.Id}/policies", TestJson.Options);
        book!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Duplicate_national_id_returns_409()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var request = new CreateCustomerRequest("Dup", "DUP-1", "d@e.com", null, null);
        await client.PostAsJsonAsync("/api/customers", request, TestJson.Options);
        var second = await client.PostAsJsonAsync("/api/customers", request, TestJson.Options);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invalid_customer_request_returns_400()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var invalid = new CreateCustomerRequest("", "", "not-an-email", null, null);
        var response = await client.PostAsJsonAsync("/api/customers", invalid, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
