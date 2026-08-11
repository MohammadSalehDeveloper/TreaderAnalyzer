using Core.Domain.Exceptions;

namespace Core.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        if (currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"Currency mismatch: {Currency} vs {other.Currency}.");
    }

    public bool Equals(Money? other)
    {
        if (other is null)
            return false;

        return Amount == other.Amount
               && Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount} {Currency}";
}
