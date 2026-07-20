using NUnit.Framework;
using IdleRacer.Racing.Visuals.Hud;

namespace IdleRacer.Racing.Tests.EditMode
{
    public sealed class CompactNumberFormatterTests
    {
        [TestCase(0L, "0")]
        [TestCase(950L, "950")]
        [TestCase(1200L, "1.2K")]
        [TestCase(15400L, "15.4K")]
        [TestCase(2300000L, "2.3M")]
        [TestCase(1100000000L, "1.1B")]
        [TestCase(-1500L, "-1.5K")]
        public void Format_MatchesExpectedCompactForm(long value, string expected)
        {
            Assert.That(CompactNumberFormatter.Format(value), Is.EqualTo(expected));
        }
    }
}
