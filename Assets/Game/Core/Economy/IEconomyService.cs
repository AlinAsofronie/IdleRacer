using System;

namespace IdleRacer.Game.Core.Economy
{
    /// <summary>
    /// Central abstraction for all currency changes. UI must never mutate balances directly;
    /// it calls this service, which records a <see cref="TransactionReason"/> for every change.
    /// </summary>
    public interface IEconomyService
    {
        /// <summary>Raised after any balance changes, with the affected currency and its new balance.</summary>
        event Action<CurrencyType, long> BalanceChanged;

        /// <summary>Returns the current balance of <paramref name="currency"/> (never negative).</summary>
        long GetBalance(CurrencyType currency);

        /// <summary>Adds <paramref name="amount"/> of <paramref name="currency"/> for the given reason.</summary>
        void Grant(CurrencyType currency, long amount, TransactionReason reason);

        /// <summary>
        /// Attempts to spend <paramref name="amount"/> of <paramref name="currency"/>. Returns
        /// <c>true</c> and deducts the amount if the balance is sufficient; otherwise returns
        /// <c>false</c> and changes nothing.
        /// </summary>
        bool TrySpend(CurrencyType currency, long amount, TransactionReason reason);
    }
}
