using NUnit.Framework;
using IdleRacer.Racing.Visuals.Hud;

namespace IdleRacer.Racing.Tests.EditMode
{
    public sealed class HudTabControllerTests
    {
        [Test]
        public void DefaultTab_IsBuild()
        {
            var tabs = new HudTabController();
            Assert.That(tabs.ActiveTab, Is.EqualTo(HudTab.Build));
            Assert.That(tabs.IsActive(HudTab.Build), Is.True);
        }

        [Test]
        public void SelectingGarage_ActivatesGarageOnly()
        {
            var tabs = new HudTabController();
            tabs.Select(HudTab.Garage);
            Assert.That(tabs.ActiveTab, Is.EqualTo(HudTab.Garage));
            Assert.That(tabs.IsActive(HudTab.Garage), Is.True);
            Assert.That(tabs.IsActive(HudTab.Build), Is.False);
            Assert.That(tabs.IsActive(HudTab.Upgrades), Is.False);
            Assert.That(tabs.IsActive(HudTab.Race), Is.False);
            Assert.That(tabs.IsActive(HudTab.More), Is.False);
        }

        [Test]
        public void SelectingUpgrades_ActivatesUpgradesOnly()
        {
            var tabs = new HudTabController();
            tabs.Select(HudTab.Upgrades);
            Assert.That(tabs.ActiveTab, Is.EqualTo(HudTab.Upgrades));
            Assert.That(tabs.IsActive(HudTab.Upgrades), Is.True);
            Assert.That(tabs.IsActive(HudTab.Build), Is.False);
        }

        [Test]
        public void SelectingRace_ActivatesRaceOnly()
        {
            var tabs = new HudTabController();
            tabs.Select(HudTab.Race);
            Assert.That(tabs.ActiveTab, Is.EqualTo(HudTab.Race));
            Assert.That(tabs.IsActive(HudTab.Race), Is.True);
            Assert.That(tabs.IsActive(HudTab.Garage), Is.False);
        }

        [Test]
        public void SelectingMore_ActivatesMoreOnly()
        {
            var tabs = new HudTabController();
            tabs.Select(HudTab.More);
            Assert.That(tabs.ActiveTab, Is.EqualTo(HudTab.More));
            Assert.That(tabs.IsActive(HudTab.More), Is.True);
            Assert.That(tabs.IsActive(HudTab.Build), Is.False);
        }
    }
}
