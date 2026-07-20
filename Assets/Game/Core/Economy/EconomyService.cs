using System;
using System.Collections.Generic;

namespace IdleRacer.Game.Core.Economy
{
    /// <summary>
    /// In-memory implementation of <see cref="IEconomyService"/>. Serialisable-friendly:
    /// balances can be read/restored via <see cref="GetBalance"/> / <see cref="Grant"/>.
    /// </summary>
    public sealed class EconomyService : IEconomyService
    {
        private readonly Dictionary<CurrencyType, long> _balances = new Dictionary<CurrencyType, long>();

        /// <inheritdoc />
        public event Action<CurrencyType, long> BalanceChanged;

        /// <inheritdoc />
        public long GetBalance(CurrencyType currency)
        {
            return _balances.TryGetValue(currency, out long value) ? value : 0L;
        }

        /// <inheritdoc />
        public void Grant(CurrencyType currency, long amount, TransactionReason reason)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Grant amount must be >= 0.");
            }

            if (amount == 0)
            {
                return;
            }

            _balances[currency] = GetBalance(currency) + amount;
            BalanceChanged?.Invoke(currency, _balances[currency]);
        }

        /// <inheritdoc />
        public bool TrySpend(CurrencyType currency, long amount, TransactionReason reason)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Spend amount must be >= 0.");
            }

            long current = GetBalance(currency);
            if (current < amount)
            {
                return false;
            }

            _balances[currency] = current - amount;
            BalanceChanged?.Invoke(currency, _balances[currency]);
            return true;
        }
    }
}
