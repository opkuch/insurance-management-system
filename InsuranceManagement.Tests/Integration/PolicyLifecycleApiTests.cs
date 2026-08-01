using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Integration;

public class PolicyLifecycleApiTests
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
    public async Task Issue_then_endorse_then_cancel_walks_the_full_lifecycle()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customerResponse = await client.PostAsJsonAsync("/api/customers", NewCustomer("LC-1"), TestJson.Options);
        customerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        var issueResponse = await client.PostAsJsonAsync($"/api/customers/{customer!.Id}/policies", NewAutoPolicy(), TestJson.Options);
        issueResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var issued = await issueResponse.Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        issued!.PolicyNumber.Should().Be("AUTO-2026-000001");
        issued.Version.Should().Be(1);

        var endorse = new EndorsePolicyRequest(new MoneyDto(1500m, "USD"), null, "Premium adjustment");
        var endorseResponse = await client.PatchAsJsonAsync($"/api/policies/{issued.Id}", endorse, TestJson.Options);
        endorseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var endorsed = await endorseResponse.Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        endorsed!.Version.Should().Be(2);
        endorsed.Premium.Amount.Should().Be(1500m);

        var cancelResponse = await client.PostAsJsonAsync($"/api/policies/{issued.Id}/cancel", new CancelPolicyRequest("Customer request", null), TestJson.Options);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        cancelled!.Status.Should().Be("Cancelled");

        cancelled.Transactions.Select(t => t.Type)
            .Should().ContainInOrder("Issued", "Endorsed", "Cancelled");
    }

    [Fact]
    public async Task Cancelling_an_already_cancelled_policy_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("LC-2"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);
        var issued = await (await client.PostAsJsonAsync($"/api/customers/{customer!.Id}/policies", NewAutoPolicy(), TestJson.Options))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);

        await client.PostAsJsonAsync($"/api/policies/{issued!.Id}/cancel", new CancelPolicyRequest("first", null), TestJson.Options);
        var second = await client.PostAsJsonAsync($"/api/policies/{issued.Id}/cancel", new CancelPolicyRequest("second", null), TestJson.Options);

        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Issuing_to_an_unknown_customer_returns_404()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/customers/{Guid.NewGuid()}/policies", NewAutoPolicy(), TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
