using InsuranceManagementService.Domain.Common;

namespace InsuranceManagementService.Domain.ValueObjects;

/// <summary>
/// A monetary amount together with its ISO 4217 currency code. Immutable and
/// non-negative; arithmetic is only allowed between amounts of the same currency.
/// </summary>
public sealed class Money : ValueObject
{
    private Money()
    {
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("A monetary amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        currency = currency.Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter ISO code (e.g. USD).");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException("Cannot combine monetary amounts with different currencies.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
