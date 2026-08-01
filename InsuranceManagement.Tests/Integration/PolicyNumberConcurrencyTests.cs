using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagement.Tests.Integration;

public class PolicyNumberConcurrencyTests
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
    public async Task Concurrent_issues_produce_unique_policy_numbers()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var customer = await (await client.PostAsJsonAsync("/api/customers", NewCustomer("PN-1"), TestJson.Options))
            .Content.ReadFromJsonAsync<CustomerResponse>(TestJson.Options);

        const int count = 8;
        var tasks = Enumerable.Range(0, count)
            .Select(_ => client.PostAsJsonAsync(
                $"/api/customers/{customer!.Id}/policies",
                NewAutoPolicy(),
                TestJson.Options))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Created);

        var policies = new List<PolicyDetailResponse>();
        foreach (var response in responses)
        {
            var policy = await response.Content.ReadFromJsonAsync<PolicyDetailResponse>(TestJson.Options);
            policies.Add(policy!);
        }

        policies.Select(p => p.PolicyNumber).Should().OnlyHaveUniqueItems();
        policies.Should().HaveCount(count);
    }
}
