using System;
using IdleRacer.Game.Core.Economy;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Generator;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;
using IdleRacer.Game.Progression.Campaign;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Game.Core
{
    /// <summary>
    /// Pure-C# application service that composes the whole v0.1C incremental loop and holds the
    /// player's (serialisable-friendly) state: economy, campaign progress, equipped loadout,
    /// Item Creator level/XP, the pending generated item, and Auto Build state.
    /// <para>
    /// The presentation layer (MonoBehaviours) calls into this controller; it never contains race
    /// mathematics or currency mutation of its own. The authoritative <see cref="RaceSimulator"/>
    /// decides outcomes.
    /// </para>
    /// </summary>
    public sealed class GameController
    {
        private readonly GameConfig _config;
        private readonly EconomyService _economy;
        private readonly CampaignService _campaign;
        private readonly EquipmentLoadout _loadout;
        private readonly ItemCreator _itemCreator;
        private readonly ItemGenerator _generator;
        private readonly RaceSimulator _simulator;
        private readonly Random _random;

        private bool _autoBuildEnabled;

        /// <param name="config">Prototype/game configuration.</param>
        /// <param name="seed">Optional RNG seed for deterministic item generation (tests).</param>
        /// <param name="campaignState">
        /// Optional starting campaign state. Enables restoring progress later (save/load) and
        /// deterministic tests; defaults to a fresh campaign at stage 1.
        /// </param>
        public GameController(GameConfig config, int? seed = null, CampaignState campaignState = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _economy = new EconomyService();
            _campaign = new CampaignService(config.Campaign, _economy, campaignState);
            _loadout = new EquipmentLoadout();
            _itemCreator = new ItemCreator(config.ItemCreator);
            _generator = new ItemGenerator();
            _simulator = new RaceSimulator();
            _random = seed.HasValue ? new Random(seed.Value) : new Random();

            _economy.Grant(CurrencyType.Gold, config.InitialGold, TransactionReason.InitialGrant);
            _economy.Grant(CurrencyType.Wheels, config.InitialWheels, TransactionReason.InitialGrant);
        }

        // ---- Read-only accessors for the UI/tests ----
        public IEconomyService Economy => _economy;
        public long Gold => _economy.GetBalance(CurrencyType.Gold);
        public long Wheels => _economy.GetBalance(CurrencyType.Wheels);
        public StageDefinition CurrentStage => _campaign.CurrentStage;
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

        /// <summary>Item equipped in <paramref name="slot"/>, or null.</summary>
        public EquipmentItem GetEquipped(EquipmentSlotType slot) => _loadout.GetEquipped(slot);

        /// <summary>Final race stats for the player (base + equipped bonuses).</summary>
        public CarRaceStats GetPlayerRaceStats()
        {
            return PlayerStatsCalculator.CalculateRaceStats(
                _config.BasePlayerAcceleration, _config.BasePlayerTopSpeed, _loadout);
        }

        /// <summary>
        /// Prepares and simulates the current stage's race (authoritative), without applying any
        /// rewards or advancement yet. Call <see cref="ResolveRace"/> after playback.
        /// </summary>
        public RacePlan PrepareRace()
        {
            StageDefinition stage = _campaign.CurrentStage;
            CarRaceStats playerStats = GetPlayerRaceStats();
            var opponentStats = new CarRaceStats(stage.OpponentAcceleration, stage.OpponentTopSpeed);

            var request = new RaceSimulationRequest(playerStats, opponentStats, stage.TrackDistance, _config.RaceFixedTimeStep);
            RaceSimulationResult result = _simulator.Simulate(request);

            return new RacePlan(stage, playerStats, opponentStats, stage.TrackDistance, result);
        }

        /// <summary>
        /// Applies the outcome of a completed race to progression: grants rewards and advances on
        /// a player win; does nothing on a loss/draw (the same stage is retried).
        /// </summary>
        public RaceResolution ResolveRace(RacePlan plan)
        {
            return _campaign.ApplyRaceResult(plan.PlayerWon);
        }

        /// <summary>
        /// Attempts to build one item: fails if a pending item is unresolved or if there are not
        /// enough Wheels; otherwise spends exactly one Wheel, generates an item at the current
        /// creator level's odds, grants creator XP, and stores it as the pending item (never
        /// auto-equipped).
        /// </summary>
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
            return new ItemBuildOutcome(ItemBuildStatus.Success, item);
        }

        /// <summary>Equips the pending item (into its own slot only) and clears the pending slot.</summary>
        public void EquipPendingItem()
        {
            if (PendingItem == null)
            {
                return;
            }
            _loadout.Equip(PendingItem);
            PendingItem = null;
        }

        /// <summary>Discards the pending item (permanently removed in v0.1C).</summary>
        public void DiscardPendingItem()
        {
            PendingItem = null;
        }

        /// <summary>Turns Auto Build on/off (only effective once unlocked).</summary>
        public void SetAutoBuildEnabled(bool enabled)
        {
            _autoBuildEnabled = enabled;
        }

        /// <summary>
        /// True when an Auto Build step may run now: it is unlocked and enabled, no pending item is
        /// waiting for the player, and there is at least one Wheel to spend.
        /// </summary>
        public bool CanAutoBuildStep()
        {
            return IsAutoBuildEnabled && !HasPendingItem && Wheels >= _config.ItemGenerationWheelCost;
        }

        /// <summary>
        /// Runs one Auto Build step if allowed, using exactly the same generation path (and odds)
        /// as manual build. Returns the outcome; a non-success means the caller should stop/pause.
        /// </summary>
        public ItemBuildOutcome TryAutoBuildStep()
        {
            if (!CanAutoBuildStep())
            {
                return new ItemBuildOutcome(
                    HasPendingItem ? ItemBuildStatus.PendingItemUnresolved : ItemBuildStatus.NotEnoughWheels, null);
            }

            return TryBuildItem();
        }
    }
}
