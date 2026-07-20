using System;
using System.Collections.Generic;
using IdleRacer.Game.Core.Economy;
using IdleRacer.Game.Core.SaveSystem;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Generator;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;
using IdleRacer.Game.Progression.Campaign;
using IdleRacer.Game.Progression.SlotUpgrades;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Game.Core
{
    /// <summary>
    /// Pure-C# application service composing the incremental loop (v0.1D) and holding the player's
    /// serialisable-friendly state: economy, campaign progress, equipped loadout, permanent slot
    /// levels, Item Creator level/XP, the pending generated item, and Auto Build state.
    /// <para>
    /// The presentation layer calls into this controller; it never contains race mathematics,
    /// currency mutation, or file-system access. The authoritative <see cref="RaceSimulator"/>
    /// decides outcomes. <see cref="StateChanged"/> is raised after any persistent-state change so a
    /// save coordinator can persist without this class depending on the file system.
    /// </para>
    /// </summary>
    public sealed class GameController
    {
        private readonly GameConfig _config;
        private readonly EconomyService _economy;
        private readonly CampaignService _campaign;
        private readonly EquipmentLoadout _loadout;
        private readonly EquipmentSlotProgression _slotProgression;
        private readonly SlotUpgradeService _slotUpgrades;
        private readonly ItemCreator _itemCreator;
        private readonly ItemGenerator _generator;
        private readonly RaceSimulator _simulator;
        private readonly Random _random;

        private bool _autoBuildEnabled;

        /// <summary>Raised after any change to persistent player state (for save coordination).</summary>
        public event Action StateChanged;

        /// <param name="config">Prototype/game configuration (single source of defaults).</param>
        /// <param name="seed">Optional RNG seed for deterministic item generation (tests).</param>
        /// <param name="campaignState">Optional starting campaign state (tests); ignored if <paramref name="loadedData"/> is set.</param>
        /// <param name="loadedData">Optional loaded save to restore from; null creates a fresh player.</param>
        public GameController(GameConfig config, int? seed = null, CampaignState campaignState = null, GameSaveDataV1 loadedData = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _economy = new EconomyService();
            _loadout = new EquipmentLoadout();
            _slotProgression = new EquipmentSlotProgression(config.StartingSlotLevel);
            _slotUpgrades = new SlotUpgradeService(_slotProgression, config.SlotUpgrades, _economy);
            _generator = new ItemGenerator();
            _simulator = new RaceSimulator();
            _random = seed.HasValue ? new Random(seed.Value) : new Random();

            if (loadedData != null)
            {
                _campaign = new CampaignService(config.Campaign, _economy,
                    new CampaignState(ClampStageIndex(loadedData.campaignStageIndex), loadedData.autoBuildUnlocked));
                _itemCreator = new ItemCreator(config.ItemCreator, loadedData.itemCreatorLevel, loadedData.itemCreatorXp);
                ApplyLoadedData(loadedData);
            }
            else
            {
                _campaign = new CampaignService(config.Campaign, _economy, campaignState);
                _itemCreator = new ItemCreator(config.ItemCreator);
                _economy.Grant(CurrencyType.Gold, config.InitialGold, TransactionReason.InitialGrant);
                _economy.Grant(CurrencyType.Wheels, config.InitialWheels, TransactionReason.InitialGrant);
            }
        }

        // ---- Read-only accessors for the UI/tests ----
        public IEconomyService Economy => _economy;
        public long Gold => _economy.GetBalance(CurrencyType.Gold);
        public long Wheels => _economy.GetBalance(CurrencyType.Wheels);
        public StageDefinition CurrentStage => _campaign.CurrentStage;
        public int CurrentStageIndex => _campaign.CurrentStageIndex;
        public int CreatorLevel => _itemCreator.Level;
        public int CreatorXp => _itemCreator.Xp;
        public int CreatorXpToNextLevel => _itemCreator.XpToNextLevel;
        public bool CreatorAtMaxLevel => _itemCreator.IsMaxLevel;
        public RarityTable CurrentRarityTable => _itemCreator.CurrentRarityTable;
        public EquipmentLoadout Loadout => _loadout;
        public EquipmentItem PendingItem { get; private set; }
        public bool HasPendingItem => PendingItem != null;
        public bool IsAutoBuildUnlocked => _campaign.IsAutoBuildUnlocked;
        public bool IsAutoBuildEnabled => _autoBuildEnabled && IsAutoBuildUnlocked;

        public EquipmentItem GetEquipped(EquipmentSlotType slot) => _loadout.GetEquipped(slot);

        // ---- Slot progression (v0.1D) ----
        public int GetSlotLevel(EquipmentSlotType slot) => _slotProgression.GetLevel(slot);
        public long GetSlotUpgradeCost(EquipmentSlotType slot) => _slotUpgrades.GetUpgradeCost(slot);
        public bool CanAffordSlotUpgrade(EquipmentSlotType slot) => _slotUpgrades.CanAfford(slot);
        public double GetSlotAccelerationBonus(EquipmentSlotType slot) => _config.SlotUpgrades.AccelerationBonusAtLevel(_slotProgression.GetLevel(slot));
        public double GetSlotTopSpeedBonus(EquipmentSlotType slot) => _config.SlotUpgrades.TopSpeedBonusAtLevel(_slotProgression.GetLevel(slot));

        /// <summary>Attempts to upgrade a slot with Gold; raises <see cref="StateChanged"/> on success.</summary>
        public SlotUpgradeOutcome TryUpgradeSlot(EquipmentSlotType slot)
        {
            SlotUpgradeOutcome outcome = _slotUpgrades.TryUpgrade(slot);
            if (outcome.Success)
            {
                RaiseStateChanged();
            }
            return outcome;
        }

        /// <summary>Final race stats: base + slot-level bonuses + equipped-item bonuses (single path).</summary>
        public CarRaceStats GetPlayerRaceStats()
        {
            return PlayerStatsCalculator.CalculateRaceStats(
                _config.BasePlayerAcceleration, _config.BasePlayerTopSpeed,
                _loadout, _slotProgression, _config.SlotUpgrades);
        }

        public RacePlan PrepareRace()
        {
            StageDefinition stage = _campaign.CurrentStage;
            CarRaceStats playerStats = GetPlayerRaceStats();
            var opponentStats = new CarRaceStats(stage.OpponentAcceleration, stage.OpponentTopSpeed);

            var request = new RaceSimulationRequest(playerStats, opponentStats, stage.TrackDistance, _config.RaceFixedTimeStep);
            RaceSimulationResult result = _simulator.Simulate(request);

            return new RacePlan(stage, playerStats, opponentStats, stage.TrackDistance, result);
        }

        /// <summary>Applies race outcome; grants rewards/advances on a win (raises StateChanged then).</summary>
        public RaceResolution ResolveRace(RacePlan plan)
        {
            RaceResolution resolution = _campaign.ApplyRaceResult(plan.PlayerWon);
            if (resolution.PlayerWon)
            {
                RaiseStateChanged();
            }
            return resolution;
        }

        public ItemBuildOutcome TryBuildItem()
        {
            if (HasPendingItem)
            {
                return new ItemBuildOutcome(ItemBuildStatus.PendingItemUnresolved, null);
            }

            if (!_economy.TrySpend(CurrencyType.Wheels, _config.ItemGenerationWheelCost, TransactionReason.ItemGenerationCost))
            {
                return new ItemBuildOutcome(ItemBuildStatus.NotEnoughWheels, null);
            }

            EquipmentItem item = _generator.Generate(_itemCreator.CurrentRarityTable, _config.RarityStats, _random);
            _itemCreator.AddGenerationXp(_config.XpPerGeneratedItem);
            PendingItem = item;
            RaiseStateChanged();
            return new ItemBuildOutcome(ItemBuildStatus.Success, item);
        }

        public void EquipPendingItem()
        {
            if (PendingItem == null)
            {
                return;
            }
            _loadout.Equip(PendingItem);
            PendingItem = null;
            RaiseStateChanged();
        }

        public void DiscardPendingItem()
        {
            if (PendingItem == null)
            {
                return;
            }
            PendingItem = null;
            RaiseStateChanged();
        }

        /// <summary>Turns Auto Build on/off (session-only; not persisted). Only effective once unlocked.</summary>
        public void SetAutoBuildEnabled(bool enabled)
        {
            _autoBuildEnabled = enabled;
        }

        public bool CanAutoBuildStep()
        {
            return IsAutoBuildEnabled && !HasPendingItem && Wheels >= _config.ItemGenerationWheelCost;
        }

        public ItemBuildOutcome TryAutoBuildStep()
        {
            if (!CanAutoBuildStep())
            {
                return new ItemBuildOutcome(
                    HasPendingItem ? ItemBuildStatus.PendingItemUnresolved : ItemBuildStatus.NotEnoughWheels, null);
            }

            return TryBuildItem();
        }

        // ---- Save / load mapping (DTOs only; no file-system access here) ----

        /// <summary>Snapshots the current persistent state into a versioned save DTO.</summary>
        public GameSaveDataV1 CreateSaveData()
        {
            var data = new GameSaveDataV1
            {
                saveVersion = SaveConstants.CurrentVersion,
                lastSavedUtcTicks = DateTime.UtcNow.Ticks,
                gold = Gold,
                wheels = Wheels,
                campaignStageIndex = _campaign.CurrentStageIndex,
                autoBuildUnlocked = _campaign.IsAutoBuildUnlocked,
                itemCreatorLevel = _itemCreator.Level,
                itemCreatorXp = _itemCreator.Xp,
                slotLevels = BuildSlotLevelsArray(),
                equippedItems = new List<EquipmentItemDto>(),
                hasPendingItem = HasPendingItem,
                pendingItem = HasPendingItem ? ToDto(PendingItem) : null
            };

            foreach (EquipmentItem item in _loadout.GetAllEquipped())
            {
                data.equippedItems.Add(ToDto(item));
            }

            return data;
        }

        private void ApplyLoadedData(GameSaveDataV1 data)
        {
            // Economy: grant saved balances into a fresh economy (starts at 0).
            _economy.Grant(CurrencyType.Gold, Math.Max(0, data.gold), TransactionReason.InitialGrant);
            _economy.Grant(CurrencyType.Wheels, Math.Max(0, data.wheels), TransactionReason.InitialGrant);

            // Slot levels.
            if (data.slotLevels != null)
            {
                foreach (EquipmentSlotType slot in EquipmentSlotProgression.Slots)
                {
                    int index = (int)slot;
                    if (index >= 0 && index < data.slotLevels.Length)
                    {
                        _slotProgression.SetLevel(slot, data.slotLevels[index]);
                    }
                }
            }

            // Equipped items.
            if (data.equippedItems != null)
            {
                foreach (EquipmentItemDto dto in data.equippedItems)
                {
                    EquipmentItem item = FromDto(dto);
                    if (item != null)
                    {
                        _loadout.Equip(item);
                    }
                }
            }

            // Pending item (persisted to prevent free rerolls by restarting the app).
            PendingItem = data.hasPendingItem ? FromDto(data.pendingItem) : null;
        }

        private int[] BuildSlotLevelsArray()
        {
            var slots = EquipmentSlotProgression.Slots;
            var array = new int[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                array[(int)slots[i]] = _slotProgression.GetLevel(slots[i]);
            }
            return array;
        }

        private int ClampStageIndex(int index)
        {
            if (index < 0) return 0;
            if (index > _config.Campaign.LastStageIndex) return _config.Campaign.LastStageIndex;
            return index;
        }

        private void RaiseStateChanged() => StateChanged?.Invoke();

        private static EquipmentItemDto ToDto(EquipmentItem item)
        {
            return new EquipmentItemDto
            {
                id = item.Id,
                slot = (int)item.Slot,
                rarity = (int)item.Rarity,
                accelerationBonus = item.AccelerationBonus,
                topSpeedBonus = item.TopSpeedBonus
            };
        }

        private static EquipmentItem FromDto(EquipmentItemDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.id))
            {
                return null;
            }
            return new EquipmentItem(
                dto.id,
                (EquipmentSlotType)dto.slot,
                (EquipmentRarity)dto.rarity,
                dto.accelerationBonus,
                dto.topSpeedBonus);
        }
    }
}
