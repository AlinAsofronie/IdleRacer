using System;
using NUnit.Framework;
using IdleRacer.Game.Core;
using IdleRacer.Game.Core.Economy;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Generator;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;
using IdleRacer.Game.Progression.Campaign;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Tests.EditMode
{
    /// <summary>Edit Mode tests for the v0.1C incremental core loop domain logic.</summary>
    public sealed class GameLoopTests
    {
        private static GameConfig NewConfig() => GameConfig.CreatePrototype();

        private static CampaignService NewCampaign(out EconomyService economy, GameConfig config = null)
        {
            config ??= NewConfig();
            economy = new EconomyService();
            return new CampaignService(config.Campaign, economy);
        }

        // ---------------- CAMPAIGN ----------------

        [Test] // 1
        public void Campaign_WinningAdvancesStage()
        {
            CampaignService campaign = NewCampaign(out _);
            int before = campaign.CurrentStageIndex;

            campaign.ApplyRaceResult(true);

            Assert.AreEqual(before + 1, campaign.CurrentStageIndex);
        }

        [Test] // 2
        public void Campaign_LosingDoesNotAdvanceStage()
        {
            CampaignService campaign = NewCampaign(out _);
            int before = campaign.CurrentStageIndex;

            campaign.ApplyRaceResult(false);

            Assert.AreEqual(before, campaign.CurrentStageIndex);
        }

        [Test] // 3
        public void Campaign_WinningGrantsConfiguredRewards()
        {
            GameConfig config = NewConfig();
            CampaignService campaign = NewCampaign(out EconomyService economy, config);
            StageDefinition stage = campaign.CurrentStage;

            campaign.ApplyRaceResult(true);

            Assert.AreEqual(stage.GoldReward, economy.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(stage.WheelReward, economy.GetBalance(CurrencyType.Wheels));
        }

        [Test] // 4
        public void Campaign_LosingGrantsNoRewards()
        {
            CampaignService campaign = NewCampaign(out EconomyService economy);

            campaign.ApplyRaceResult(false);

            Assert.AreEqual(0L, economy.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(0L, economy.GetBalance(CurrencyType.Wheels));
        }

        [Test] // 5
        public void Campaign_CompletingNormal1_10_UnlocksAutoBuild()
        {
            GameConfig config = NewConfig();
            CampaignService campaign = NewCampaign(out _, config);

            Assert.IsFalse(campaign.IsAutoBuildUnlocked);

            // Win every stage, including the final one.
            for (int i = 0; i < config.Campaign.StageCount; i++)
            {
                campaign.ApplyRaceResult(true);
            }

            Assert.IsTrue(campaign.IsAutoBuildUnlocked);
        }

        // ---------------- ECONOMY ----------------

        [Test] // 6
        public void Economy_WheelsCanBeGranted()
        {
            var economy = new EconomyService();
            economy.Grant(CurrencyType.Wheels, 3, TransactionReason.RaceReward);
            Assert.AreEqual(3L, economy.GetBalance(CurrencyType.Wheels));
        }

        [Test] // 7
        public void Economy_SpendingOneWheelDecreasesByExactlyOne()
        {
            var economy = new EconomyService();
            economy.Grant(CurrencyType.Wheels, 5, TransactionReason.RaceReward);

            bool spent = economy.TrySpend(CurrencyType.Wheels, 1, TransactionReason.ItemGenerationCost);

            Assert.IsTrue(spent);
            Assert.AreEqual(4L, economy.GetBalance(CurrencyType.Wheels));
        }

        [Test] // 8
        public void Economy_SpendingFailsWhenInsufficient()
        {
            var economy = new EconomyService();
            economy.Grant(CurrencyType.Wheels, 0, TransactionReason.RaceReward);

            bool spent = economy.TrySpend(CurrencyType.Wheels, 1, TransactionReason.ItemGenerationCost);

            Assert.IsFalse(spent);
            Assert.AreEqual(0L, economy.GetBalance(CurrencyType.Wheels));
        }

        // ---------------- ITEM GENERATION ----------------

        [Test] // 9 & 10
        public void ItemGeneration_ProducesValidSlotAndRarity()
        {
            GameConfig config = NewConfig();
            var generator = new ItemGenerator();
            RarityTable table = config.ItemCreator.GetLevel(1).RarityTable;

            for (int seed = 0; seed < 25; seed++)
            {
                EquipmentItem item = generator.Generate(table, config.RarityStats, new Random(seed));
                Assert.IsTrue(Enum.IsDefined(typeof(EquipmentSlotType), item.Slot));
                Assert.IsTrue(Enum.IsDefined(typeof(EquipmentRarity), item.Rarity));
            }
        }

        [Test] // 11
        public void ItemGeneration_SameSeedProducesSameItem()
        {
            GameConfig config = NewConfig();
            var generator = new ItemGenerator();
            RarityTable table = config.ItemCreator.GetLevel(1).RarityTable;

            EquipmentItem a = generator.Generate(table, config.RarityStats, new Random(777));
            EquipmentItem b = generator.Generate(table, config.RarityStats, new Random(777));

            Assert.AreEqual(a.Slot, b.Slot);
            Assert.AreEqual(a.Rarity, b.Rarity);
            Assert.AreEqual(a.AccelerationBonus, b.AccelerationBonus);
            Assert.AreEqual(a.TopSpeedBonus, b.TopSpeedBonus);
            Assert.AreEqual(a.Id, b.Id);
        }

        [Test] // 12
        public void ItemGeneration_RarityProbabilitiesSumTo100_ForEveryLevel()
        {
            GameConfig config = NewConfig();
            for (int level = 1; level <= config.ItemCreator.MaxLevel; level++)
            {
                RarityTable table = config.ItemCreator.GetLevel(level).RarityTable;
                Assert.AreEqual(100.0, table.TotalPercent(), 1e-6, $"Level {level} odds must sum to 100.");
            }
        }

        [Test] // 13
        public void ItemGeneration_CostsExactlyOneWheel()
        {
            var game = new GameController(NewConfig(), seed: 1);
            long before = game.Wheels;

            ItemBuildOutcome outcome = game.TryBuildItem();

            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(before - 1, game.Wheels);
        }

        // ---------------- EQUIPMENT ----------------

        [Test] // 14
        public void Equipment_EquippingAffectsCalculatedStats()
        {
            var game = new GameController(NewConfig(), seed: 2);
            CarRaceStats before = game.GetPlayerRaceStats();

            game.TryBuildItem();
            EquipmentItem pending = game.PendingItem;
            game.EquipPendingItem();

            CarRaceStats after = game.GetPlayerRaceStats();
            Assert.AreEqual(before.Acceleration + pending.AccelerationBonus, after.Acceleration, 1e-9);
            Assert.AreEqual(before.TopSpeed + pending.TopSpeedBonus, after.TopSpeed, 1e-9);
        }

        [Test] // 15 & 16
        public void Equipment_ReplacingSlotOnlyAffectsThatSlot()
        {
            var loadout = new EquipmentLoadout();
            var engineA = new EquipmentItem("a", EquipmentSlotType.Engine, EquipmentRarity.Common, 1, 2);
            var turboB = new EquipmentItem("b", EquipmentSlotType.Turbo, EquipmentRarity.Rare, 3, 6);
            var engineC = new EquipmentItem("c", EquipmentSlotType.Engine, EquipmentRarity.Epic, 6, 12);

            loadout.Equip(engineA);
            loadout.Equip(turboB);
            loadout.Equip(engineC);

            Assert.AreSame(engineC, loadout.GetEquipped(EquipmentSlotType.Engine)); // replaced
            Assert.AreSame(turboB, loadout.GetEquipped(EquipmentSlotType.Turbo));   // unchanged
        }

        [Test] // 17
        public void Equipment_GeneratedItemIsNeverAutoEquipped()
        {
            var game = new GameController(NewConfig(), seed: 3);
            CarRaceStats before = game.GetPlayerRaceStats();

            game.TryBuildItem();
            EquipmentItem pending = game.PendingItem;

            Assert.IsNotNull(pending);
            Assert.IsNull(game.GetEquipped(pending.Slot)); // not equipped
            CarRaceStats after = game.GetPlayerRaceStats();
            Assert.AreEqual(before.Acceleration, after.Acceleration, 1e-9);
            Assert.AreEqual(before.TopSpeed, after.TopSpeed, 1e-9);
        }

        // ---------------- ITEM CREATOR ----------------

        [Test] // 18
        public void ItemCreator_GeneratingItemsGrantsXp()
        {
            var game = new GameController(NewConfig(), seed: 4);
            int before = game.CreatorXp;

            game.TryBuildItem();

            Assert.AreEqual(before + 1, game.CreatorXp);
        }

        [Test] // 19
        public void ItemCreator_LevelIncreasesAtConfiguredThreshold()
        {
            GameConfig config = NewConfig();
            var creator = new ItemCreator(config.ItemCreator);
            int threshold = config.ItemCreator.GetLevel(1).XpToNextLevel;

            for (int i = 0; i < threshold; i++)
            {
                creator.AddGenerationXp(1);
            }

            Assert.AreEqual(2, creator.Level);
            Assert.AreEqual(0, creator.Xp);
        }

        [Test] // 20
        public void ItemCreator_LevelChangesActiveRarityConfig()
        {
            GameConfig config = NewConfig();
            var creator = new ItemCreator(config.ItemCreator);
            RarityTable level1Table = creator.CurrentRarityTable;

            int threshold = config.ItemCreator.GetLevel(1).XpToNextLevel;
            for (int i = 0; i < threshold; i++)
            {
                creator.AddGenerationXp(1);
            }

            RarityTable level2Table = creator.CurrentRarityTable;
            Assert.AreNotSame(level1Table, level2Table);
            Assert.AreNotEqual(CommonPercent(level1Table), CommonPercent(level2Table));
        }

        private static double CommonPercent(RarityTable table)
        {
            foreach (RarityWeight w in table.Weights)
            {
                if (w.Rarity == EquipmentRarity.Common) return w.ProbabilityPercent;
            }
            return -1.0;
        }

        // ---------------- AUTO BUILD ----------------

        [Test] // 21
        public void AutoBuild_RemainsLockedBeforeNormal1_10()
        {
            var game = new GameController(NewConfig(), seed: 5);

            Assert.IsFalse(game.IsAutoBuildUnlocked);
            game.SetAutoBuildEnabled(true);
            Assert.IsFalse(game.CanAutoBuildStep());
            Assert.IsFalse(game.TryAutoBuildStep().Success);
        }

        [Test] // 22
        public void AutoBuild_UnlocksAfterNormal1_10()
        {
            GameConfig config = NewConfig();
            CampaignService campaign = NewCampaign(out _, config);

            for (int i = 0; i < config.Campaign.StageCount; i++)
            {
                campaign.ApplyRaceResult(true);
            }

            Assert.IsTrue(campaign.IsAutoBuildUnlocked);
        }

        [Test] // 23
        public void AutoBuild_UsesSameItemGeneratorAsManualBuild()
        {
            GameConfig config = NewConfig();
            int lastIndex = config.Campaign.LastStageIndex;

            var manual = new GameController(config, seed: 999,
                campaignState: new CampaignState(lastIndex, autoBuildUnlocked: true));
            var auto = new GameController(NewConfig(), seed: 999,
                campaignState: new CampaignState(lastIndex, autoBuildUnlocked: true));

            EquipmentItem manualItem = manual.TryBuildItem().Item;

            auto.SetAutoBuildEnabled(true);
            Assert.IsTrue(auto.CanAutoBuildStep());
            EquipmentItem autoItem = auto.TryAutoBuildStep().Item;

            Assert.AreEqual(manualItem.Slot, autoItem.Slot);
            Assert.AreEqual(manualItem.Rarity, autoItem.Rarity);
            Assert.AreEqual(manualItem.AccelerationBonus, autoItem.AccelerationBonus);
            Assert.AreEqual(manualItem.TopSpeedBonus, autoItem.TopSpeedBonus);
            Assert.AreEqual(manualItem.Id, autoItem.Id);
        }

        [Test] // 24
        public void AutoBuild_StopsWhenWheelsAreZero()
        {
            GameConfig config = NewConfig();
            int lastIndex = config.Campaign.LastStageIndex;
            var game = new GameController(config, seed: 6,
                campaignState: new CampaignState(lastIndex, autoBuildUnlocked: true));

            // Drain all Wheels through the economy service.
            game.Economy.TrySpend(CurrencyType.Wheels, game.Wheels, TransactionReason.ItemGenerationCost);
            Assert.AreEqual(0L, game.Wheels);

            game.SetAutoBuildEnabled(true);

            Assert.IsFalse(game.CanAutoBuildStep());
            Assert.AreEqual(ItemBuildStatus.NotEnoughWheels, game.TryAutoBuildStep().Status);
        }
    }
}
