using FluentAssertions;
using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagement.Tests.Domain;

public class PolicyTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    private static CoverageDraft SampleCoverage(string currency = "USD") =>
        new("Liability", new Money(10000m, currency), new Money(500m, currency));

    private static Policy IssueSample(
        DateRange? term = null,
        Money? premium = null,
        IEnumerable<CoverageDraft>? coverages = null,
        DateTime? now = null)
    {
        return Policy.Issue(
            PolicyNumber.Create(ProductType.Auto, 2026, 1),
            Guid.NewGuid(),
            ProductType.Auto,
            term ?? new DateRange(Now, Now.AddYears(1)),
            premium ?? new Money(1000m, "USD"),
            BillingFrequency.Monthly,
            coverages ?? new[] { SampleCoverage() },
            now ?? Now);
    }

    [Fact]
    public void Issue_activates_when_term_has_started_and_logs_a_transaction()
    {
        var policy = IssueSample();

        policy.Status.Should().Be(PolicyStatus.Active);
        policy.Version.Should().Be(1);
        policy.Coverages.Should().HaveCount(1);
        policy.Transactions.Should().ContainSingle(t => t.Type == PolicyTransactionType.Issued);
    }

    [Fact]
    public void Issue_is_pending_when_term_starts_in_the_future()
    {
        var policy = IssueSample(term: new DateRange(Now.AddDays(10), Now.AddYears(1)));

        policy.Status.Should().Be(PolicyStatus.Issued);
    }

    [Fact]
    public void Issue_requires_at_least_one_coverage()
    {
        var act = () => IssueSample(coverages: Array.Empty<CoverageDraft>());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Issue_requires_a_positive_premium()
    {
        var act = () => IssueSample(premium: new Money(0m, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Issue_rejects_mixed_currencies()
    {
        var act = () => IssueSample(
            premium: new Money(1000m, "USD"),
            coverages: new[] { SampleCoverage("EUR") });

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Endorse_increments_version_updates_premium_and_logs_transaction()
    {
        var policy = IssueSample();

        policy.Endorse(new Money(1500m, "USD"), null, "rate change", Now.AddDays(1));

        policy.Version.Should().Be(2);
        policy.Premium.Amount.Should().Be(1500m);
        policy.Transactions.Should().Contain(t => t.Type == PolicyTransactionType.Endorsed);
    }

    [Fact]
    public void Endorse_requires_at_least_one_change()
    {
        var policy = IssueSample();

        var act = () => policy.Endorse(null, null, null, Now.AddDays(1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_terminates_the_policy_and_records_reason()
    {
        var policy = IssueSample();

        policy.Cancel(Now.AddDays(5), "customer request", Now.AddDays(5));

        policy.Status.Should().Be(PolicyStatus.Cancelled);
        policy.CancellationReason.Should().Be("customer request");
        policy.Transactions.Should().Contain(t => t.Type == PolicyTransactionType.Cancelled);
    }

    [Fact]
    public void Cancelled_policy_cannot_be_endorsed_or_cancelled_again()
    {
        var policy = IssueSample();
        policy.Cancel(Now, "first", Now);

        policy.Invoking(p => p.Endorse(new Money(200m, "USD"), null, null, Now))
            .Should().Throw<DomainException>();
        policy.Invoking(p => p.Cancel(Now, "second", Now))
            .Should().Throw<DomainException>();
    }

    [Fact]
    public void CurrentStatus_advances_with_the_clock_without_RefreshLifecycle()
    {
        var policy = IssueSample(
            term: new DateRange(Now.AddDays(10), Now.AddDays(40)),
            now: Now);

        policy.Status.Should().Be(PolicyStatus.Issued);
        policy.CurrentStatus(Now).Should().Be(PolicyStatus.Issued);
        policy.CurrentStatus(Now.AddDays(15)).Should().Be(PolicyStatus.Active);
        policy.CurrentStatus(Now.AddDays(41)).Should().Be(PolicyStatus.Expired);
        policy.Status.Should().Be(PolicyStatus.Issued);
    }

    [Fact]
    public void Endorse_and_Cancel_are_blocked_when_derived_status_is_Expired()
    {
        var policy = IssueSample(term: new DateRange(Now, Now.AddDays(30)));
        var afterTerm = Now.AddDays(31);

        policy.Status.Should().Be(PolicyStatus.Active);
        policy.CurrentStatus(afterTerm).Should().Be(PolicyStatus.Expired);

        policy.Invoking(p => p.Endorse(new Money(200m, "USD"), null, null, afterTerm))
            .Should().Throw<DomainException>();
        policy.Invoking(p => p.Cancel(afterTerm, "too late", afterTerm))
            .Should().Throw<DomainException>();
    }

    [Fact]
    public void RefreshLifecycle_expires_a_policy_whose_term_has_ended()
    {
        var policy = IssueSample(term: new DateRange(Now, Now.AddDays(30)));

        var changed = policy.RefreshLifecycle(Now.AddDays(31));

        changed.Should().BeTrue();
        policy.Status.Should().Be(PolicyStatus.Expired);
        policy.Transactions.Should().Contain(t => t.Type == PolicyTransactionType.Expired);
    }

    [Fact]
    public void RefreshLifecycle_activates_a_pending_policy_without_an_audit_entry()
    {
        var policy = IssueSample(
            term: new DateRange(Now.AddDays(10), Now.AddYears(1)),
            now: Now);
        var transactionsBefore = policy.Transactions.Count;

        var changed = policy.RefreshLifecycle(Now.AddDays(11));

        changed.Should().BeTrue();
        policy.Status.Should().Be(PolicyStatus.Active);
        policy.Transactions.Should().HaveCount(transactionsBefore);
    }
}
