using FluentAssertions;
using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagement.Tests.Models;

public class ValueObjectTests
{
    [Fact]
    public void Money_rejects_negative_amounts()
    {
        var act = () => new Money(-1m, "USD");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Money_normalizes_currency_and_rounds_amount()
    {
        var money = new Money(10.005m, "usd");

        money.Currency.Should().Be("USD");
        money.Amount.Should().Be(10.00m);
    }

    [Fact]
    public void Money_addition_requires_matching_currency()
    {
        var usd = new Money(10m, "USD");
        var eur = new Money(5m, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Money_is_equal_by_value()
    {
        new Money(10m, "USD").Should().Be(new Money(10m, "USD"));
    }

    [Fact]
    public void DateRange_requires_end_after_start()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var act = () => new DateRange(start, start);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("2026-06-01", true)]
    [InlineData("2027-06-01", false)]
    public void DateRange_includes_only_instants_within_the_period(string instant, bool expected)
    {
        var range = new DateRange(
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        range.Includes(DateTime.Parse(instant).ToUniversalTime()).Should().Be(expected);
    }

    [Fact]
    public void PolicyNumber_is_formatted_from_its_parts()
    {
        var number = PolicyNumber.Create(ProductType.Auto, 2026, 123);

        number.Value.Should().Be("AUTO-2026-000123");
    }

    [Fact]
    public void PolicyNumber_rejects_non_positive_sequence()
    {
        var act = () => PolicyNumber.Create(ProductType.Life, 2026, 0);

        act.Should().Throw<DomainException>();
    }
}
