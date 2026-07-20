using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Large fixed bottom navigation for portrait playtesting.</summary>
    public sealed class BottomNavigationView
    {
        private static readonly HudTab[] Tabs =
        {
            HudTab.Garage, HudTab.Build, HudTab.Upgrades, HudTab.Race, HudTab.More
        };

        private static readonly string[] Labels =
        {
            "Garage", "Build", "Upgrades", "Race", "More"
        };

        private readonly Image[] _images = new Image[5];
        private readonly TextMeshProUGUI[] _labels = new TextMeshProUGUI[5];
        private readonly Image[] _underlines = new Image[5];

        public BottomNavigationView(RectTransform parent, Action<HudTab> onSelect)
        {
            var nav = UiFactory.CreatePanel(parent, "BottomNavigation", UiTheme.NavBackground);
            UiFactory.Stretch((RectTransform)nav.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, new Vector2(0f, UiTheme.NavHeight));

            var hlg = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 12, 14);
            hlg.spacing = 8;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            var navRt = (RectTransform)nav.transform;
            for (int i = 0; i < Tabs.Length; i++)
            {
                HudTab tab = Tabs[i];
                var btnGo = new GameObject("Tab_" + tab, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                btnGo.transform.SetParent(navRt, false);
                _images[i] = btnGo.GetComponent<Image>();
                _images[i].color = UiTheme.NavInactive;
                btnGo.GetComponent<LayoutElement>().minHeight = UiTheme.TouchMinHeight;

                var vlg = btnGo.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(4, 4, 10, 8);
                vlg.spacing = 4;
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;

                _labels[i] = UiFactory.AddTmp((RectTransform)btnGo.transform, "Label", UiTheme.FontBody,
                    TextAlignmentOptions.Center, UiTheme.TextSecondary, 40f, true, FontStyles.Bold);

                var underline = UiFactory.CreatePanel(btnGo.transform, "Underline", UiTheme.PrimaryAccent);
                var ule = underline.gameObject.AddComponent<LayoutElement>();
                ule.minHeight = 6f;
                ule.preferredHeight = 6f;
                _underlines[i] = underline;

                int captured = i;
                btnGo.GetComponent<Button>().onClick.AddListener(() => onSelect?.Invoke(Tabs[captured]));
                _labels[i].text = Labels[i];
            }
        }

        public void SetActiveTab(HudTab active)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                bool on = Tabs[i] == active;
                _images[i].color = on ? new Color(0.16f, 0.28f, 0.40f, 1f) : UiTheme.NavInactive;
                _labels[i].color = on ? UiTheme.TextPrimary : UiTheme.TextMuted;
                _underlines[i].enabled = on;
                _underlines[i].color = on ? UiTheme.PrimaryAccent : Color.clear;
            }
        }
    }
}
