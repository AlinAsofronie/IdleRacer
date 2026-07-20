using System.Collections.Generic;
using IdleRacer.Game.Equipment.Generator;
using IdleRacer.Game.Equipment.Rarities;
using IdleRacer.Game.Progression.Campaign;

namespace IdleRacer.Game.Core
{
    /// <summary>
    /// Central, data-driven configuration for the v0.1C incremental loop. All values here are
    /// PROTOTYPE values, not final balance. Extend/replace with authored data assets later.
    /// Units: Acceleration m/s^2, TopSpeed m/s, TrackDistance m, TimeStep s.
    /// </summary>
    public sealed class GameConfig
    {
        public double BasePlayerAcceleration { get; }
        public double BasePlayerTopSpeed { get; }
        public long InitialGold { get; }
        public long InitialWheels { get; }
        public long ItemGenerationWheelCost { get; }
        public int XpPerGeneratedItem { get; }
        public double RaceFixedTimeStep { get; }

        public CampaignDefinition Campaign { get; }
        public ItemCreatorConfig ItemCreator { get; }
        public RarityStatTable RarityStats { get; }

        public GameConfig(
            double basePlayerAcceleration,
            double basePlayerTopSpeed,
            long initialGold,
            long initialWheels,
            long itemGenerationWheelCost,
            int xpPerGeneratedItem,
            double raceFixedTimeStep,
            CampaignDefinition campaign,
            ItemCreatorConfig itemCreator,
            RarityStatTable rarityStats)
        {
            BasePlayerAcceleration = basePlayerAcceleration;
            BasePlayerTopSpeed = basePlayerTopSpeed;
            InitialGold = initialGold;
            InitialWheels = initialWheels;
            ItemGenerationWheelCost = itemGenerationWheelCost;
            XpPerGeneratedItem = xpPerGeneratedItem;
            RaceFixedTimeStep = raceFixedTimeStep;
            Campaign = campaign;
            ItemCreator = itemCreator;
            RarityStats = rarityStats;
        }

        /// <summary>Builds the default prototype configuration for Milestone 0.1C.</summary>
        public static GameConfig CreatePrototype()
        {
            return new GameConfig(
                basePlayerAcceleration: 8.0,
                basePlayerTopSpeed: 40.0,
                initialGold: 0L,
                initialWheels: 5L,
                itemGenerationWheelCost: 1L,
                xpPerGeneratedItem: 1,
                raceFixedTimeStep: 0.02,
                campaign: CreateNormalChapter1(),
                itemCreator: CreateItemCreatorConfig(),
                rarityStats: CreateRarityStatTable());
        }

        // Normal 1-1 .. 1-10. Opponent stats scale up so the early stages are winnable with base
        // stats, then require equipment (creating the lose -> build -> equip -> win loop).
        private static CampaignDefinition CreateNormalChapter1()
        {
            var stages = new List<StageDefinition>
            {
                new StageDefinition("Normal-1-1", "NORMAL 1-1", 6.0, 35.0, 300.0, 50L, 2L),
                new StageDefinition("Normal-1-2", "NORMAL 1-2", 7.0, 38.0, 320.0, 60L, 2L),
                new StageDefinition("Normal-1-3", "NORMAL 1-3", 8.0, 41.0, 340.0, 70L, 2L),
                new StageDefinition("Normal-1-4", "NORMAL 1-4", 9.0, 44.0, 360.0, 80L, 3L),
                new StageDefinition("Normal-1-5", "NORMAL 1-5", 10.0, 47.0, 380.0, 90L, 3L),
                new StageDefinition("Normal-1-6", "NORMAL 1-6", 11.0, 50.0, 400.0, 100L, 3L),
                new StageDefinition("Normal-1-7", "NORMAL 1-7", 12.0, 53.0, 420.0, 120L, 4L),
                new StageDefinition("Normal-1-8", "NORMAL 1-8", 13.0, 56.0, 440.0, 140L, 4L),
                new StageDefinition("Normal-1-9", "NORMAL 1-9", 14.0, 59.0, 460.0, 160L, 4L),
                new StageDefinition("Normal-1-10", "NORMAL 1-10", 16.0, 63.0, 500.0, 250L, 6L),
            };
            return new CampaignDefinition(stages);
        }

        // Three prototype creator levels; higher levels shift odds toward better rarities.
        private static ItemCreatorConfig CreateItemCreatorConfig()
        {
            var level1 = new RarityTable(new[]
            {
                new RarityWeight(EquipmentRarity.Common, 60.0),
                new RarityWeight(EquipmentRarity.Uncommon, 25.0),
                new RarityWeight(EquipmentRarity.Rare, 10.0),
                new RarityWeight(EquipmentRarity.Epic, 4.0),
                new RarityWeight(EquipmentRarity.Legendary, 0.9),
                new RarityWeight(EquipmentRarity.Mythic, 0.1),
            });
            var level2 = new RarityTable(new[]
            {
                new RarityWeight(EquipmentRarity.Common, 45.0),
                new RarityWeight(EquipmentRarity.Uncommon, 30.0),
                new RarityWeight(EquipmentRarity.Rare, 15.0),
                new RarityWeight(EquipmentRarity.Epic, 7.0),
                new RarityWeight(EquipmentRarity.Legendary, 2.5),
                new RarityWeight(EquipmentRarity.Mythic, 0.5),
            });
            var level3 = new RarityTable(new[]
            {
                new RarityWeight(EquipmentRarity.Common, 30.0),
                new RarityWeight(EquipmentRarity.Uncommon, 30.0),
                new RarityWeight(EquipmentRarity.Rare, 22.0),
                new RarityWeight(EquipmentRarity.Epic, 12.0),
                new RarityWeight(EquipmentRarity.Legendary, 5.0),
                new RarityWeight(EquipmentRarity.Mythic, 1.0),
            });

            return new ItemCreatorConfig(new[]
            {
                new ItemCreatorLevelDefinition(1, level1, xpToNextLevel: 5),
                new ItemCreatorLevelDefinition(2, level2, xpToNextLevel: 5),
                new ItemCreatorLevelDefinition(3, level3, xpToNextLevel: 0), // max level
            });
        }

        private static RarityStatTable CreateRarityStatTable()
        {
            return new RarityStatTable(new Dictionary<EquipmentRarity, RarityStats>
            {
                { EquipmentRarity.Common, new RarityStats(1.0, 2.0) },
                { EquipmentRarity.Uncommon, new RarityStats(2.0, 4.0) },
                { EquipmentRarity.Rare, new RarityStats(3.5, 7.0) },
                { EquipmentRarity.Epic, new RarityStats(6.0, 12.0) },
                { EquipmentRarity.Legendary, new RarityStats(10.0, 20.0) },
                { EquipmentRarity.Mythic, new RarityStats(16.0, 32.0) },
            });
        }
    }
}
