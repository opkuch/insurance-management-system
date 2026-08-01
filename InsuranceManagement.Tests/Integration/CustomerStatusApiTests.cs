using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Integration;

public class CustomerStatusApiTests
{
    private static CreateCustomerRequest NewCustomer(string nationalId) =>
        new("Acme Corp", nationalId, "ops@acme.com", null, null);

    private static IssuePolicyRequest NewAutoPolicy() => new(
        ProductType.Auto,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new MoneyDto(1200m, "USD"),
        BillingFrequency.Monthly,
        new[]
        {
            new CoverageDraftDto("Collision", new MoneyDto(50000m, "USD"), new MoneyDto(500m, "USD")),
        });

    [Fact]
    public async Task Issuing_to_an_inactive_customer_returns_409()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("ST-1"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        var deactivate = await client.PostAsync($"/api/customers/{customer!.Id}/deactivate", null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.OK);

        var issue = await client.PostAsJsonAsync($"/api/customers/{customer.Id}/policies", NewAutoPolicy(), TestJson.Options);

        issue.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await issue.Content.ReadFromJsonAsync<Dictionary<string, object>>(TestJson.Options);
        problem.Should().ContainKey("title");
    }

    [Fact]
    public async Task Deactivate_then_activate_restores_issueability()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("ST-2"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        (await client.PostAsync($"/api/customers/{customer!.Id}/deactivate", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var activateResponse = await client.PostAsync($"/api/customers/{customer.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var activated = await activateResponse.Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);
        activated!.Status.Should().Be("Active");

        var issue = await client.PostAsJsonAsync($"/api/customers/{customer.Id}/policies", NewAutoPolicy(), TestJson.Options);
        issue.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Double_deactivate_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("ST-3"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        (await client.PostAsync($"/api/customers/{customer!.Id}/deactivate", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsync($"/api/customers/{customer.Id}/deactivate", null);
        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
