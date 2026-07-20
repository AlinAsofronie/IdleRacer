using UnityEngine;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>
    /// Central presentation-only visual tokens for the playtest UI (v0.2B).
    /// Domain logic must never depend on these values.
    /// </summary>
    public static class UiTheme
    {
        // ---- Surfaces ----
        public static readonly Color CanvasClear = new Color(0.05f, 0.06f, 0.08f, 1f);
        public static readonly Color ShellBackground = new Color(0.08f, 0.09f, 0.12f, 1f);
        public static readonly Color PanelBackground = new Color(0.11f, 0.12f, 0.16f, 1f);
        public static readonly Color CardBackground = new Color(0.14f, 0.15f, 0.20f, 1f);
        public static readonly Color CardElevated = new Color(0.16f, 0.18f, 0.24f, 1f);
        public static readonly Color PillBackground = new Color(0.10f, 0.11f, 0.15f, 0.94f);
        public static readonly Color NavBackground = new Color(0.06f, 0.07f, 0.10f, 1f);
        public static readonly Color NavInactive = new Color(0.12f, 0.13f, 0.17f, 1f);
        public static readonly Color OddsRowBackground = new Color(0.12f, 0.13f, 0.17f, 1f);

        // ---- Accents ----
        public static readonly Color PrimaryAccent = new Color(0.25f, 0.72f, 0.95f, 1f);
        public static readonly Color SecondaryAccent = new Color(0.95f, 0.72f, 0.28f, 1f);
        public static readonly Color Success = new Color(0.28f, 0.82f, 0.48f, 1f);
        public static readonly Color Negative = new Color(0.95f, 0.38f, 0.40f, 1f);
        public static readonly Color Neutral = new Color(0.78f, 0.80f, 0.86f, 1f);
        public static readonly Color Disabled = new Color(0.28f, 0.29f, 0.34f, 1f);
        public static readonly Color TextPrimary = new Color(0.96f, 0.97f, 1f, 1f);
        public static readonly Color TextSecondary = new Color(0.72f, 0.74f, 0.80f, 1f);
        public static readonly Color TextMuted = new Color(0.55f, 0.57f, 0.64f, 1f);
        public static readonly Color Gold = new Color(1f, 0.84f, 0.32f, 1f);
        public static readonly Color Wheels = new Color(0.55f, 0.82f, 1f, 1f);
        public static readonly Color CtaBuild = new Color(0.12f, 0.72f, 0.48f, 1f);
        public static readonly Color CtaEquip = new Color(0.14f, 0.70f, 0.46f, 1f);
        public static readonly Color CtaDiscard = new Color(0.38f, 0.20f, 0.22f, 1f);
        public static readonly Color UpgradeAfford = new Color(0.42f, 0.34f, 0.14f, 1f);
        public static readonly Color ProgressTrack = new Color(0.22f, 0.24f, 0.30f, 1f);
        public static readonly Color ProgressFill = new Color(0.28f, 0.70f, 0.95f, 1f);

        // ---- Spacing / sizing (reference 1080×1920 canvas units) ----
        public const float SpaceXs = 6f;
        public const float SpaceSm = 10f;
        public const float SpaceMd = 16f;
        public const float SpaceLg = 24f;
        public const float CornerRadiusHint = 16f;
        public const float TouchMinHeight = 96f;
        public const float NavHeight = 168f;
        public const float ResourceBarHeight = 88f;
        public const float ShellTopFraction = 0.60f; // bottom 60% progression → race ~40%

        // ---- Typography ----
        public const float FontCaption = 22f;
        public const float FontBody = 28f;
        public const float FontSubtitle = 32f;
        public const float FontTitle = 40f;
        public const float FontHero = 48f;
        public const float FontRaceStatus = 56f;
        public const float FontCta = 36f;

        public static Color Rarity(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => new Color(0.78f, 0.78f, 0.82f, 1f),
                EquipmentRarity.Uncommon => new Color(0.40f, 0.88f, 0.52f, 1f),
                EquipmentRarity.Rare => new Color(0.40f, 0.62f, 1f, 1f),
                EquipmentRarity.Epic => new Color(0.78f, 0.45f, 0.98f, 1f),
                EquipmentRarity.Legendary => new Color(1f, 0.74f, 0.24f, 1f),
                EquipmentRarity.Mythic => new Color(1f, 0.38f, 0.48f, 1f),
                _ => TextPrimary
            };
        }

        public static Color Delta(double value)
        {
            if (value > 0.0001) return Success;
            if (value < -0.0001) return Negative;
            return Neutral;
        }
    }
}
