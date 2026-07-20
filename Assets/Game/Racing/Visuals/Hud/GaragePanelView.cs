using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Racing.Simulation;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>2×4 equipment card grid plus player stats card.</summary>
    public sealed class GaragePanelView
    {
        private static readonly EquipmentSlotType[] Slots =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        private readonly GameObject _root;
        private readonly TextMeshProUGUI _accelText;
        private readonly TextMeshProUGUI _topText;
        private readonly TextMeshProUGUI[] _slotTitles = new TextMeshProUGUI[8];
        private readonly TextMeshProUGUI[] _slotBodies = new TextMeshProUGUI[8];
        private readonly Image[] _slotAccents = new Image[8];

        public GaragePanelView(RectTransform host)
        {
            _root = new GameObject("GaragePanel", typeof(RectTransform));
            _root.transform.SetParent(host, false);
            UiFactory.Stretch((RectTransform)_root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform content = UiFactory.CreateScrollContent((RectTransform)_root.transform);

            var statsCard = UiFactory.CreatePanel(content, "PlayerStatsCard", UiTheme.CardElevated);
            var statsLe = statsCard.gameObject.AddComponent<LayoutElement>();
            statsLe.minHeight = 150f;
            statsLe.preferredHeight = 150f;
            var statsV = statsCard.gameObject.AddComponent<VerticalLayoutGroup>();
            statsV.padding = new RectOffset(20, 20, 16, 16);
            statsV.spacing = 8;
            statsV.childControlWidth = true;
            statsV.childForceExpandWidth = true;
            statsV.childControlHeight = true;
            statsV.childForceExpandHeight = false;

            var statsRt = (RectTransform)statsCard.transform;
            UiFactory.AddTmp(statsRt, "StatsHeader", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 36f, true, FontStyles.Bold).text = "PLAYER STATS";
            _accelText = UiFactory.AddTmp(statsRt, "Accel", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.TextPrimary, 44f, true, FontStyles.Bold);
            _topText = UiFactory.AddTmp(statsRt, "Top", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.TextPrimary, 44f, true, FontStyles.Bold);

            var gridGo = new GameObject("EquipmentGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            gridGo.transform.SetParent(content, false);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480f, 210f);
            grid.spacing = new Vector2(UiTheme.SpaceMd, UiTheme.SpaceMd);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            gridGo.GetComponent<LayoutElement>().minHeight = 900f;
            gridGo.GetComponent<LayoutElement>().preferredHeight = 900f;

            // Flexible cell sizing via ContentSizeFitter-like approach: use aspect with flexible width
            // Grid cell fixed width works with reference 1080; use flexible via LayoutElement on parent width.
            // Update cell size at runtime based on parent - for now use slightly smaller cells.
            grid.cellSize = new Vector2(470f, 200f);

            for (int i = 0; i < Slots.Length; i++)
            {
                var card = UiFactory.CreatePanel(gridGo.transform, "SlotCard_" + Slots[i], UiTheme.CardBackground);
                card.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

                _slotAccents[i] = UiFactory.CreatePanel(card.transform, "Accent", UiTheme.TextMuted);
                UiFactory.Stretch((RectTransform)_slotAccents[i].transform,
                    new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(8f, 0f));

                var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(20, 14, 14, 12);
                v.spacing = 4;
                v.childControlWidth = true;
                v.childForceExpandWidth = true;
                v.childControlHeight = true;
                v.childForceExpandHeight = false;

                var cardRt = (RectTransform)card.transform;
                _slotTitles[i] = UiFactory.AddTmp(cardRt, "Title", UiTheme.FontBody, TextAlignmentOptions.Left,
                    UiTheme.TextPrimary, 36f, true, FontStyles.Bold);
                _slotBodies[i] = UiFactory.AddTmp(cardRt, "Body", UiTheme.FontCaption, TextAlignmentOptions.TopLeft,
                    UiTheme.TextSecondary, 120f, true);
            }
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(GameController game)
        {
            CarRaceStats stats = game.GetPlayerRaceStats();
            _accelText.text = $"Accel  {stats.Acceleration:0.00}";
            _topText.text = $"Top Speed  {stats.TopSpeed:0.00}";

            for (int i = 0; i < Slots.Length; i++)
            {
                EquipmentSlotType slot = Slots[i];
                EquipmentItem item = game.GetEquipped(slot);
                int level = game.GetSlotLevel(slot);
                _slotTitles[i].text = $"{UiFactory.SlotDisplayName(slot)}  ·  Lv {level}";
                if (item == null)
                {
                    _slotBodies[i].text = "EMPTY\nAccel  —\nTop  —";
                    _slotBodies[i].color = UiTheme.TextMuted;
                    _slotAccents[i].color = UiTheme.TextMuted;
                }
                else
                {
                    _slotBodies[i].text =
                        $"{item.Rarity}\nAccel  {UiFactory.Signed(item.AccelerationBonus)}\nTop  {UiFactory.Signed(item.TopSpeedBonus)}";
                    Color rarity = UiTheme.Rarity(item.Rarity);
                    _slotBodies[i].color = rarity;
                    _slotAccents[i].color = rarity;
                }
            }
        }
    }
}
