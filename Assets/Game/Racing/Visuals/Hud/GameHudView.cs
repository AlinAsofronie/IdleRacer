using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Game.Equipment;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>
    /// Playtest-ready mobile shell (v0.2B): resource bar, race overlay, tabbed panels, bottom nav.
    /// </summary>
    public sealed class GameHudView
    {
        private readonly HudTabController _tabs = new HudTabController();
        private readonly ResourceBarView _resourceBar;
        private readonly BottomNavigationView _navigation;
        private readonly GaragePanelView _garage;
        private readonly BuildPanelView _build;
        private readonly UpgradesPanelView _upgrades;
        private readonly RacePanelView _race;
        private readonly MorePanelView _more;

        private readonly TextMeshProUGUI _statusText;
        private readonly TextMeshProUGUI _playerTimeText;
        private readonly TextMeshProUGUI _opponentTimeText;

        public HudTab ActiveTab => _tabs.ActiveTab;
        public TextMeshProUGUI StatusText => _statusText;
        public TextMeshProUGUI PlayerTimeText => _playerTimeText;
        public TextMeshProUGUI OpponentTimeText => _opponentTimeText;

        public GameHudView(
            Transform canvasParent,
            Action onBuild,
            Action onToggleAuto,
            Action onEquip,
            Action onDiscard,
            Action<EquipmentSlotType> onUpgradeSlot)
        {
            var canvasGo = new GameObject("GameHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(canvasParent, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeGo = new GameObject("SafeAreaRoot", typeof(RectTransform));
            safeGo.transform.SetParent(canvasGo.transform, false);
            var safeRt = (RectTransform)safeGo.transform;
            UiFactory.ApplySafeArea(safeRt);

            _resourceBar = new ResourceBarView(safeRt);

            float shellTop = UiTheme.ShellTopFraction;
            // Status sits near the bottom of the race band so it does not invent empty mid-band space.
            _statusText = CreateOverlayText(safeRt, "StatusText",
                new Vector2(0.08f, shellTop + 0.02f), new Vector2(0.92f, shellTop + 0.12f),
                UiTheme.FontRaceStatus, TextAlignmentOptions.Center, UiTheme.TextPrimary);
            _playerTimeText = CreateOverlayText(safeRt, "PlayerTimeText",
                new Vector2(0.06f, shellTop + 0.12f), new Vector2(0.50f, shellTop + 0.18f),
                UiTheme.FontCaption, TextAlignmentOptions.Left, new Color(0.55f, 0.78f, 1f));
            _opponentTimeText = CreateOverlayText(safeRt, "OpponentTimeText",
                new Vector2(0.50f, shellTop + 0.12f), new Vector2(0.94f, shellTop + 0.18f),
                UiTheme.FontCaption, TextAlignmentOptions.Right, new Color(1f, 0.55f, 0.48f));

            var shell = UiFactory.CreatePanel(safeRt, "ProgressionShell", UiTheme.ShellBackground);
            var shellRt = (RectTransform)shell.transform;
            UiFactory.Stretch(shellRt, new Vector2(0f, 0f), new Vector2(1f, shellTop), Vector2.zero, Vector2.zero);

            var panelHostGo = new GameObject("ActivePanelHost", typeof(RectTransform));
            panelHostGo.transform.SetParent(shellRt, false);
            var panelHost = (RectTransform)panelHostGo.transform;
            UiFactory.Stretch(panelHost, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0f, UiTheme.NavHeight), Vector2.zero);

            _garage = new GaragePanelView(panelHost);
            _build = new BuildPanelView(panelHost, onBuild, onToggleAuto, onEquip, onDiscard);
            _upgrades = new UpgradesPanelView(panelHost, onUpgradeSlot);
            _race = new RacePanelView(panelHost);
            _more = new MorePanelView(panelHost);

            _navigation = new BottomNavigationView(shellRt, SelectTab);
            ApplyTabVisibility();
        }

        public void SelectTab(HudTab tab)
        {
            _tabs.Select(tab);
            ApplyTabVisibility();
        }

        public void Refresh(GameController game)
        {
            _resourceBar.Refresh(game);
            _garage.Refresh(game);
            _build.Refresh(game);
            _upgrades.Refresh(game);
            _race.Refresh(game);
            _more.Refresh();
        }

        private void ApplyTabVisibility()
        {
            HudTab active = _tabs.ActiveTab;
            _garage.SetVisible(active == HudTab.Garage);
            _build.SetVisible(active == HudTab.Build);
            _upgrades.SetVisible(active == HudTab.Upgrades);
            _race.SetVisible(active == HudTab.Race);
            _more.SetVisible(active == HudTab.More);
            _navigation.SetActiveTab(active);
        }

        private static TextMeshProUGUI CreateOverlayText(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            UiFactory.Stretch((RectTransform)go.transform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = UiFactory.Font;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
