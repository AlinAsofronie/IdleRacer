using UnityEngine;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Compatibility wrapper over <see cref="UiTheme"/> rarity colours.</summary>
    public static class RarityUiColors
    {
        public static Color For(EquipmentRarity rarity) => UiTheme.Rarity(rarity);
        public static Color PositiveDelta => UiTheme.Success;
        public static Color NegativeDelta => UiTheme.Negative;
        public static Color NeutralDelta => UiTheme.Neutral;
    }
}
