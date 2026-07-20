using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Persistent Gold / Wheels / stage badge across all tabs.</summary>
    public sealed class ResourceBarView
    {
        private readonly TextMeshProUGUI _goldText;
        private readonly TextMeshProUGUI _wheelsText;
        private readonly TextMeshProUGUI _stageText;

        public ResourceBarView(RectTransform parent)
        {
            var bar = UiFactory.CreatePanel(parent, "ResourceBar", UiTheme.PillBackground);
            UiFactory.Stretch((RectTransform)bar.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(UiTheme.SpaceMd, -UiTheme.ResourceBarHeight - UiTheme.SpaceSm),
                new Vector2(-UiTheme.SpaceMd, -UiTheme.SpaceSm));

            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.spacing = UiTheme.SpaceMd;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;

            var barRt = (RectTransform)bar.transform;
            _goldText = CreatePill(barRt, "GoldPill", UiTheme.Gold);
            _stageText = CreatePill(barRt, "StagePill", UiTheme.PrimaryAccent);
            _wheelsText = CreatePill(barRt, "WheelsPill", UiTheme.Wheels);
        }

        public void Refresh(GameController game)
        {
            _goldText.text = "G  " + CompactNumberFormatter.Format(game.Gold);
            _goldText.color = UiTheme.Gold;
            _wheelsText.text = "W  " + CompactNumberFormatter.Format(game.Wheels);
            _wheelsText.color = UiTheme.Wheels;
            _stageText.text = game.CurrentStage.DisplayName;
            _stageText.color = UiTheme.TextPrimary;
        }

        private static TextMeshProUGUI CreatePill(RectTransform parent, string name, Color accent)
        {
            var pill = UiFactory.CreatePanel(parent, name, new Color(0.08f, 0.09f, 0.12f, 1f));
            var le = pill.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = 64f;

            var accentBar = UiFactory.CreatePanel(pill.transform, "Accent", accent);
            UiFactory.Stretch((RectTransform)accentBar.transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(6f, 0f));

            return UiFactory.AddTmp((RectTransform)pill.transform, "Value", UiTheme.FontSubtitle,
                TextAlignmentOptions.Center, UiTheme.TextPrimary, 56f, true, FontStyles.Bold);
        }
    }
}
