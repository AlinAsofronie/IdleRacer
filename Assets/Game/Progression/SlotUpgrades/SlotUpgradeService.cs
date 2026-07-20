using System;
using IdleRacer.Game.Core.Economy;
using IdleRacer.Game.Equipment;

namespace IdleRacer.Game.Progression.SlotUpgrades
{
    /// <summary>Result of attempting a slot upgrade.</summary>
    public readonly struct SlotUpgradeOutcome
    {
        public bool Success { get; }
        public long GoldSpent { get; }
        public int NewLevel { get; }

        public SlotUpgradeOutcome(bool success, long goldSpent, int newLevel)
        {
            Success = success;
            GoldSpent = goldSpent;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// Pure-C# domain logic for Gold-funded permanent slot upgrades. UI requests an upgrade; this
    /// service determines the cost, spends Gold via <see cref="IEconomyService"/>, and raises the
    /// slot level by exactly one on success. On insufficient Gold nothing changes.
    /// </summary>
    public sealed class SlotUpgradeService
    {
        private readonly EquipmentSlotProgression _progression;
        private readonly SlotUpgradeConfig _config;
        private readonly IEconomyService _economy;

        public SlotUpgradeService(EquipmentSlotProgression progression, SlotUpgradeConfig config, IEconomyService economy)
        {
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        public int GetLevel(EquipmentSlotType slot) => _progression.GetLevel(slot);

        /// <summary>Gold cost to upgrade <paramref name="slot"/> to its next level.</summary>
        public long GetUpgradeCost(EquipmentSlotType slot) => _config.UpgradeCost(_progression.GetLevel(slot));

        /// <summary>True when the player can currently afford to upgrade <paramref name="slot"/>.</summary>
        public bool CanAfford(EquipmentSlotType slot) => _economy.GetBalance(CurrencyType.Gold) >= GetUpgradeCost(slot);

        /// <summary>
        /// Attempts to upgrade <paramref name="slot"/>: spends exactly the configured Gold cost and
        /// raises the level by one on success; changes nothing if Gold is insufficient.
        /// </summary>
        public SlotUpgradeOutcome TryUpgrade(EquipmentSlotType slot)
        {
            long cost = GetUpgradeCost(slot);
            if (!_economy.TrySpend(CurrencyType.Gold, cost, TransactionReason.EquipmentSlotUpgrade))
            {
                return new SlotUpgradeOutcome(false, 0L, _progression.GetLevel(slot));
            }

            _progression.IncrementLevel(slot);
            return new SlotUpgradeOutcome(true, cost, _progression.GetLevel(slot));
        }
    }
}
