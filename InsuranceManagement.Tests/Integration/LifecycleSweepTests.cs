using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceManagement.Tests.Integration;

public class LifecycleSweepTests
{
    private static CreateCustomerRequest NewCustomer(string nationalId) =>
        new("Acme Corp", nationalId, "ops@acme.com", null, null);

    [Fact]
    public async Task Sweep_activates_issued_policies_whose_term_has_started()
    {
        await using var factory = new ApiFactory();
        var clock = factory.UseControllableClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("SW-1"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        // Term starts in the future relative to the controllable clock → Issued.
        var request = new IssuePolicyRequest(
            ProductType.Auto,
            new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new MoneyDto(1200m, "USD"),
            BillingFrequency.Monthly,
            new[]
            {
                new CoverageDraftDto("Collision", new MoneyDto(50000m, "USD"), new MoneyDto(500m, "USD")),
            });

        var issued = await (await client.PostAsJsonAsync($"/api/customers/{customer!.Id}/policies", request, TestJson.Options))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        issued!.Status.Should().Be("Issued");

        clock.UtcNow = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        using (var scope = factory.Services.CreateScope())
        {
            var policies = scope.ServiceProvider.GetRequiredService<IPolicyService>();
            var changed = await policies.RunLifecycleSweepAsync();
            changed.Should().Be(1);
        }

        var refreshed = await (await client.GetAsync($"/api/policies/{issued.Id}"))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        refreshed!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Sweep_expires_active_policies_whose_term_has_ended()
    {
        await using var factory = new ApiFactory();
        var clock = factory.UseControllableClock(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("SW-2"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        // Term already started at issue time → Active; end is still in the future.
        var request = new IssuePolicyRequest(
            ProductType.Health,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new MoneyDto(800m, "USD"),
            BillingFrequency.Annual,
            new[]
            {
                new CoverageDraftDto("Medical", new MoneyDto(100000m, "USD"), new MoneyDto(1000m, "USD")),
            });

        var issued = await (await client.PostAsJsonAsync($"/api/customers/{customer!.Id}/policies", request, TestJson.Options))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        issued!.Status.Should().Be("Active");

        clock.UtcNow = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        using (var scope = factory.Services.CreateScope())
        {
            var policies = scope.ServiceProvider.GetRequiredService<IPolicyService>();
            var changed = await policies.RunLifecycleSweepAsync();
            changed.Should().Be(1);
        }

        var refreshed = await (await client.GetAsync($"/api/policies/{issued.Id}"))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        refreshed!.Status.Should().Be("Expired");
        refreshed.Transactions.Select(t => t.Type).Should().Contain("Expired");
    }

    [Fact]
    public async Task Health_endpoint_returns_healthy()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
