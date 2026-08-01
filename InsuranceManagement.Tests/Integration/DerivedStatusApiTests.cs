using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Integration;

public class DerivedStatusApiTests
{
    private static CreateCustomerRequest NewCustomer(string nationalId) =>
        new("Acme Corp", nationalId, "ops@acme.com", null, null);

    [Fact]
    public async Task List_filters_use_derived_status_without_get_or_sweep()
    {
        await using var factory = new ApiFactory();
        var clock = factory.UseControllableClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("DS-1"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

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

        // Advance into the term without get-by-id or sweep — stored Status remains Issued.
        clock.UtcNow = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var active = await (await client.GetAsync("/api/policies?status=Active"))
            .Content.ReadFromJsonAsync<PagedResult<PolicySummaryResponse>>(TestJson.Options);
        active!.Items.Should().Contain(p => p.Id == issued.Id);
        active.Items.Single(p => p.Id == issued.Id).Status.Should().Be("Active");

        var stillIssued = await (await client.GetAsync("/api/policies?status=Issued"))
            .Content.ReadFromJsonAsync<PagedResult<PolicySummaryResponse>>(TestJson.Options);
        stillIssued!.Items.Should().NotContain(p => p.Id == issued.Id);

        // Past term end — list shows Expired without materialization; detail then audits expiry.
        clock.UtcNow = new DateTime(2027, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var expired = await (await client.GetAsync("/api/policies?status=Expired"))
            .Content.ReadFromJsonAsync<PagedResult<PolicySummaryResponse>>(TestJson.Options);
        expired!.Items.Should().Contain(p => p.Id == issued.Id);
        expired.Items.Single(p => p.Id == issued.Id).Status.Should().Be("Expired");

        var detail = await (await client.GetAsync($"/api/policies/{issued.Id}"))
            .Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
        detail!.Status.Should().Be("Expired");
        detail.Transactions.Select(t => t.Type).Should().Contain("Expired");
    }
}
