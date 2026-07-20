using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Game.Equipment;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Stacked Gold upgrade cards with large touch targets.</summary>
    public sealed class UpgradesPanelView
    {
        private static readonly EquipmentSlotType[] Slots =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        private readonly GameObject _root;
        private readonly TextMeshProUGUI[] _titles = new TextMeshProUGUI[8];
        private readonly TextMeshProUGUI[] _bodies = new TextMeshProUGUI[8];
        private readonly Button[] _buttons = new Button[8];
        private readonly TextMeshProUGUI[] _buttonLabels = new TextMeshProUGUI[8];
        private readonly Image[] _buttonImages = new Image[8];

        public UpgradesPanelView(RectTransform host, Action<EquipmentSlotType> onUpgrade)
        {
            _root = new GameObject("UpgradesPanel", typeof(RectTransform));
            _root.transform.SetParent(host, false);
            UiFactory.Stretch((RectTransform)_root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform content = UiFactory.CreateScrollContent((RectTransform)_root.transform);
            UiFactory.AddTmp(content, "Header", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.SecondaryAccent, 48f, true, FontStyles.Bold).text = "SLOT UPGRADES";
            UiFactory.AddTmp(content, "Hint", UiTheme.FontBody, TextAlignmentOptions.Left,
                UiTheme.TextMuted, 36f, true).text = "Permanent bonuses funded with Gold.";

            for (int i = 0; i < Slots.Length; i++)
            {
                EquipmentSlotType slot = Slots[i];
                var card = UiFactory.CreatePanel(content, "UpgradeCard_" + slot, UiTheme.CardBackground);
                card.gameObject.AddComponent<LayoutElement>().minHeight = 210f;
                var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(18, 18, 14, 14);
                v.spacing = 8;
                v.childControlWidth = true;
                v.childForceExpandWidth = true;
                v.childControlHeight = true;
                v.childForceExpandHeight = false;

                var cardRt = (RectTransform)card.transform;
                _titles[i] = UiFactory.AddTmp(cardRt, "Title", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                    UiTheme.TextPrimary, 40f, true, FontStyles.Bold);
                _bodies[i] = UiFactory.AddTmp(cardRt, "Body", UiTheme.FontBody, TextAlignmentOptions.Left,
                    UiTheme.TextSecondary, 70f, true);

                _buttons[i] = UiFactory.AddButton(cardRt, "UpgradeButton", "UPGRADE", UiTheme.FontBody,
                    UiTheme.UpgradeAfford, out _buttonLabels[i], UiTheme.TouchMinHeight);
                _buttonImages[i] = _buttons[i].GetComponent<Image>();
                EquipmentSlotType captured = slot;
                _buttons[i].onClick.AddListener(() => onUpgrade?.Invoke(captured));
            }
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(GameController game)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                EquipmentSlotType slot = Slots[i];
                int level = game.GetSlotLevel(slot);
                double accel = game.GetSlotAccelerationBonus(slot);
                double top = game.GetSlotTopSpeedBonus(slot);
                long cost = game.GetSlotUpgradeCost(slot);
                bool afford = game.CanAffordSlotUpgrade(slot);

                _titles[i].text = $"{UiFactory.SlotDisplayName(slot)}  ·  Level {level}";
                _bodies[i].text = $"Permanent  +{accel:0.##} Accel   ·   +{top:0.##} Top\nNext cost  G {CompactNumberFormatter.Format(cost)}";
                _buttonLabels[i].text = afford ? $"UPGRADE  ·  G {CompactNumberFormatter.Format(cost)}" : $"NEED G {CompactNumberFormatter.Format(cost)}";
                _buttons[i].interactable = afford;
                _buttonImages[i].color = afford ? UiTheme.UpgradeAfford : UiTheme.Disabled;
            }
        }
    }
}
