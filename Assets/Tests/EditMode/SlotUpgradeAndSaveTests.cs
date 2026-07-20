using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using IdleRacer.Game.Core;
using IdleRacer.Game.Core.Economy;
using IdleRacer.Game.Core.SaveSystem;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;
using IdleRacer.Game.Progression.Campaign;
using IdleRacer.Game.Infrastructure;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Tests.EditMode
{
    /// <summary>Edit Mode tests for v0.1D slot upgrades and the local save system.</summary>
    public sealed class SlotUpgradeAndSaveTests
    {
        private static GameConfig NewConfig() => GameConfig.CreatePrototype();

        private readonly List<string> _tempFiles = new List<string>();

        private LocalJsonSaveRepository NewTempRepo(out string path)
        {
            path = Path.Combine(Path.GetTempPath(), "idleracer_test_" + Guid.NewGuid().ToString("N") + ".json");
            _tempFiles.Add(path);
            return new LocalJsonSaveRepository(path);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (string f in _tempFiles)
            {
                try
                {
                    if (File.Exists(f)) File.Delete(f);
                    if (File.Exists(f + ".tmp")) File.Delete(f + ".tmp");
                }
                catch { /* ignore cleanup errors */ }
            }
            _tempFiles.Clear();
        }

        // ---------------- SLOT UPGRADES ----------------

        [Test] // 1
        public void AllEightSlots_StartAtLevelOne()
        {
            var game = new GameController(NewConfig(), seed: 1);
            foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                Assert.AreEqual(1, game.GetSlotLevel(slot), $"{slot} should start at level 1.");
            }
        }

        [Test] // 2 & 9
        public void UpgradingASlot_IncreasesOnlyThatSlotByOne()
        {
            var game = new GameController(NewConfig(), seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 1000, TransactionReason.RaceReward);

            game.TryUpgradeSlot(EquipmentSlotType.Engine);

            Assert.AreEqual(2, game.GetSlotLevel(EquipmentSlotType.Engine));
            Assert.AreEqual(1, game.GetSlotLevel(EquipmentSlotType.Turbo));
            Assert.AreEqual(1, game.GetSlotLevel(EquipmentSlotType.Gearbox));
        }

        [Test] // 3
        public void Upgrade_SpendsExactlyConfiguredGold()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            long cost = game.GetSlotUpgradeCost(EquipmentSlotType.Engine);
            game.Economy.Grant(CurrencyType.Gold, cost, TransactionReason.RaceReward);
            long before = game.Gold;

            var outcome = game.TryUpgradeSlot(EquipmentSlotType.Engine);

            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(cost, outcome.GoldSpent);
            Assert.AreEqual(before - cost, game.Gold);
        }

        [Test] // 4 & 5
        public void InsufficientGold_DoesNotUpgradeOrSpend()
        {
            var game = new GameController(NewConfig(), seed: 1);
            long cost = game.GetSlotUpgradeCost(EquipmentSlotType.Engine);
            game.Economy.Grant(CurrencyType.Gold, cost - 1, TransactionReason.RaceReward); // one short
            long before = game.Gold;

            var outcome = game.TryUpgradeSlot(EquipmentSlotType.Engine);

            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(1, game.GetSlotLevel(EquipmentSlotType.Engine)); // unchanged
            Assert.AreEqual(before, game.Gold);                              // nothing spent
        }

        [Test] // 6
        public void SlotLevel_UnchangedWhenReplacingEquippedItem()
        {
            var game = new GameController(NewConfig(), seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 1000, TransactionReason.RaceReward);
            game.TryUpgradeSlot(EquipmentSlotType.Engine); // Engine -> level 2

            // Equip (and replace) an Engine item directly on the loadout.
            game.Loadout.Equip(new EquipmentItem("x", EquipmentSlotType.Engine, EquipmentRarity.Common, 1, 2));
            game.Loadout.Equip(new EquipmentItem("y", EquipmentSlotType.Engine, EquipmentRarity.Rare, 3, 6));

            Assert.AreEqual(2, game.GetSlotLevel(EquipmentSlotType.Engine));
        }

        [Test] // 7
        public void SlotBonuses_AffectFinalPlayerStats()
        {
            var game = new GameController(NewConfig(), seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 1000, TransactionReason.RaceReward);
            CarRaceStats before = game.GetPlayerRaceStats();

            game.TryUpgradeSlot(EquipmentSlotType.Engine);
            CarRaceStats after = game.GetPlayerRaceStats();

            Assert.Greater(after.Acceleration, before.Acceleration);
            Assert.Greater(after.TopSpeed, before.TopSpeed);
        }

        [Test] // 8
        public void ItemAndSlotBonuses_CombineCorrectly()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 1000, TransactionReason.RaceReward);

            game.TryUpgradeSlot(EquipmentSlotType.Engine); // slot bonus for level 2
            var item = new EquipmentItem("e", EquipmentSlotType.Engine, EquipmentRarity.Rare, 3.0, 6.0);
            game.Loadout.Equip(item);

            double expectedAccel = config.BasePlayerAcceleration
                + config.SlotUpgrades.AccelerationBonusAtLevel(2)
                + item.AccelerationBonus;
            double expectedTop = config.BasePlayerTopSpeed
                + config.SlotUpgrades.TopSpeedBonusAtLevel(2)
                + item.TopSpeedBonus;

            CarRaceStats stats = game.GetPlayerRaceStats();
            Assert.AreEqual(expectedAccel, stats.Acceleration, 1e-9);
            Assert.AreEqual(expectedTop, stats.TopSpeed, 1e-9);
        }

        [Test] // 10
        public void UpgradeCost_FollowsConfiguredCurve()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 100000, TransactionReason.RaceReward);

            long costL1 = game.GetSlotUpgradeCost(EquipmentSlotType.Engine);
            Assert.AreEqual(config.SlotUpgrades.UpgradeCost(1), costL1);

            game.TryUpgradeSlot(EquipmentSlotType.Engine); // now level 2
            long costL2 = game.GetSlotUpgradeCost(EquipmentSlotType.Engine);
            Assert.AreEqual(config.SlotUpgrades.UpgradeCost(2), costL2);
            Assert.Greater(costL2, costL1);
        }

        // ---------------- SAVE SYSTEM ----------------

        [Test] // 11
        public void FreshState_SerialisesAndDeserialises()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            SaveLoadResult result = repo.Load();

            Assert.AreEqual(SaveLoadStatus.Loaded, result.Status);
            var restored = new GameController(config, loadedData: result.Data);
            Assert.AreEqual(game.Gold, restored.Gold);
            Assert.AreEqual(game.Wheels, restored.Wheels);
            Assert.AreEqual(game.CurrentStageIndex, restored.CurrentStageIndex);
            Assert.AreEqual(1, restored.GetSlotLevel(EquipmentSlotType.Engine));
        }

        [Test] // 12 & 13
        public void GoldAndWheels_Persist()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 777, TransactionReason.RaceReward);
            game.Economy.Grant(CurrencyType.Wheels, 13, TransactionReason.RaceReward);
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.AreEqual(game.Gold, restored.Gold);
            Assert.AreEqual(game.Wheels, restored.Wheels);
        }

        [Test] // 14
        public void CampaignStage_Persists()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1, campaignState: new CampaignState(4, false));
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.AreEqual(4, restored.CurrentStageIndex);
        }

        [Test] // 15
        public void EquippedItems_Persist()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.TryBuildItem();
            EquipmentItem pending = game.PendingItem;
            game.EquipPendingItem();
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            EquipmentItem restoredItem = restored.GetEquipped(pending.Slot);
            Assert.IsNotNull(restoredItem);
            Assert.AreEqual(pending.Id, restoredItem.Id);
            Assert.AreEqual(pending.Rarity, restoredItem.Rarity);
            Assert.AreEqual(pending.AccelerationBonus, restoredItem.AccelerationBonus, 1e-9);
            Assert.AreEqual(pending.TopSpeedBonus, restoredItem.TopSpeedBonus, 1e-9);
        }

        [Test] // 16
        public void SlotLevels_Persist()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.Economy.Grant(CurrencyType.Gold, 100000, TransactionReason.RaceReward);
            game.TryUpgradeSlot(EquipmentSlotType.Engine);
            game.TryUpgradeSlot(EquipmentSlotType.Engine); // Engine -> 3
            game.TryUpgradeSlot(EquipmentSlotType.Tyres);   // Tyres -> 2
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.AreEqual(3, restored.GetSlotLevel(EquipmentSlotType.Engine));
            Assert.AreEqual(2, restored.GetSlotLevel(EquipmentSlotType.Tyres));
            Assert.AreEqual(1, restored.GetSlotLevel(EquipmentSlotType.Turbo));
        }

        [Test] // 17
        public void ItemCreatorLevelAndXp_Persist()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.Economy.Grant(CurrencyType.Wheels, 100, TransactionReason.RaceReward);
            // Generate several items (resolving pending each time) to accrue XP / level.
            for (int i = 0; i < 7; i++)
            {
                game.TryBuildItem();
                game.DiscardPendingItem();
            }
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.AreEqual(game.CreatorLevel, restored.CreatorLevel);
            Assert.AreEqual(game.CreatorXp, restored.CreatorXp);
        }

        [Test] // 18
        public void AutoBuildUnlock_Persists()
        {
            GameConfig config = NewConfig();
            int last = config.Campaign.LastStageIndex;
            var game = new GameController(config, seed: 1, campaignState: new CampaignState(last, true));
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.IsTrue(restored.IsAutoBuildUnlocked);
        }

        [Test] // 19
        public void PendingItem_Persists()
        {
            GameConfig config = NewConfig();
            var game = new GameController(config, seed: 1);
            game.TryBuildItem();
            EquipmentItem pending = game.PendingItem;
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            var restored = new GameController(config, loadedData: repo.Load().Data);

            Assert.IsTrue(restored.HasPendingItem);
            Assert.AreEqual(pending.Id, restored.PendingItem.Id);
            Assert.AreEqual(pending.Slot, restored.PendingItem.Slot);
            Assert.AreEqual(pending.Rarity, restored.PendingItem.Rarity);
        }

        [Test] // 20
        public void SaveVersion_IsWritten()
        {
            var game = new GameController(NewConfig(), seed: 1);
            LocalJsonSaveRepository repo = NewTempRepo(out _);

            repo.Save(game.CreateSaveData());
            SaveLoadResult result = repo.Load();

            Assert.AreEqual(SaveLoadStatus.Loaded, result.Status);
            Assert.AreEqual(SaveConstants.CurrentVersion, result.Data.saveVersion);
        }

        [Test] // 21
        public void MissingSave_ReturnsFreshState()
        {
            LocalJsonSaveRepository repo = NewTempRepo(out _); // path does not exist yet
            SaveLoadResult result = repo.Load();
            Assert.AreEqual(SaveLoadStatus.NoSave, result.Status);
        }

        [Test] // 22
        public void CorruptedSave_FailsSafely()
        {
            LocalJsonSaveRepository repo = NewTempRepo(out string path);
            File.WriteAllText(path, "{ this is not valid json ]]");

            SaveLoadResult result = repo.Load();

            Assert.AreEqual(SaveLoadStatus.Corrupted, result.Status);
            Assert.IsNull(result.Data);
        }

        [Test] // 23
        public void UnsupportedFutureVersion_FailsSafely()
        {
            LocalJsonSaveRepository repo = NewTempRepo(out string path);
            File.WriteAllText(path, "{\"saveVersion\":999,\"gold\":10}");

            SaveLoadResult result = repo.Load();

            Assert.AreEqual(SaveLoadStatus.UnsupportedVersion, result.Status);
        }

        [Test] // 24
        public void OfflineDuration_CalculatedFromLastSavedUtc()
        {
            long start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            long now = start + TimeSpan.FromHours(3).Ticks;

            TimeSpan duration = OfflineProgress.CalculateOfflineDuration(start, now);

            Assert.AreEqual(TimeSpan.FromHours(3), duration);
        }

        [Test] // 25
        public void NegativeOfflineDuration_ClampsToZero()
        {
            long start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            long now = start - TimeSpan.FromHours(5).Ticks; // clock moved backwards

            TimeSpan duration = OfflineProgress.CalculateOfflineDuration(start, now);

            Assert.AreEqual(TimeSpan.Zero, duration);
        }
    }
}
