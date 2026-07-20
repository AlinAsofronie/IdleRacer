using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Game.Equipment;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Racing.Visuals
{
    /// <summary>
    /// Builds and updates the bottom progression UI (v0.1D): currencies, Item Creator, pending
    /// item, and the eight equipment slots with permanent slot levels + Gold upgrade buttons.
    /// Presentation only: it reads <see cref="GameController"/> and forwards button presses via
    /// callbacks; it holds no game logic and never mutates currency or levels directly.
    /// </summary>
    public sealed class ProgressionUiView
    {
        private static readonly EquipmentSlotType[] Slots =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        private readonly Font _font;
        private readonly Action<EquipmentSlotType> _onUpgradeSlot;

        private Text _stageText;
        private Text _goldText;
        private Text _wheelsText;
        private Text _creatorText;
        private Text _oddsText;
        private Text _buildButtonLabel;
        private Text _autoBuildButtonLabel;
        private Button _buildButton;
        private Button _autoBuildButton;

        private GameObject _pendingPanel;
        private Text _pendingText;

        private readonly Text[] _slotInfoTexts = new Text[8];
        private readonly Button[] _slotUpgradeButtons = new Button[8];
        private readonly Text[] _slotUpgradeLabels = new Text[8];

        public ProgressionUiView(
            RectTransform panel,
            Action onBuild,
            Action onToggleAuto,
            Action onEquip,
            Action onDiscard,
            Action<EquipmentSlotType> onUpgradeSlot)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _onUpgradeSlot = onUpgradeSlot;
            BuildContent(panel, onBuild, onToggleAuto, onEquip, onDiscard);
        }

        private void BuildContent(RectTransform panel, Action onBuild, Action onToggleAuto, Action onEquip, Action onDiscard)
        {
            RectTransform content = CreateScrollView(panel);

            _stageText = AddText(content, "StageText", 40, TextAnchor.MiddleCenter, Color.white, 60);

            RectTransform currencyRow = AddRow(content, "CurrencyRow", 44);
            _goldText = AddText(currencyRow, "GoldText", 34, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.3f), 44, true);
            _wheelsText = AddText(currencyRow, "WheelsText", 34, TextAnchor.MiddleRight, new Color(0.6f, 0.85f, 1f), 44, true);

            AddText(content, "CreatorHeader", 30, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.85f), 38).text = "ITEM CREATOR";
            _creatorText = AddText(content, "CreatorText", 30, TextAnchor.MiddleLeft, Color.white, 40);
            _oddsText = AddText(content, "OddsText", 26, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.9f), 190);

            RectTransform buttonRow = AddRow(content, "ButtonRow", 90);
            _buildButton = AddButton(buttonRow, "BuildButton", "BUILD ITEM", 34, new Color(0.20f, 0.45f, 0.30f), out _buildButtonLabel);
            _buildButton.onClick.AddListener(() => onBuild?.Invoke());
            _autoBuildButton = AddButton(buttonRow, "AutoBuildButton", "AUTO BUILD", 30, new Color(0.30f, 0.30f, 0.42f), out _autoBuildButtonLabel);
            _autoBuildButton.onClick.AddListener(() => onToggleAuto?.Invoke());

            BuildPendingPanel(content, onEquip, onDiscard);

            AddText(content, "EquipmentHeader", 30, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.85f), 38).text = "EQUIPMENT";
            for (int i = 0; i < Slots.Length; i++)
            {
                BuildSlotRow(content, i, Slots[i]);
            }
        }

        private void BuildSlotRow(RectTransform content, int index, EquipmentSlotType slot)
        {
            RectTransform row = AddRow(content, "SlotRow_" + slot, 120);
            _slotInfoTexts[index] = AddText(row, "SlotInfo_" + slot, 25, TextAnchor.MiddleLeft, Color.white, 120, true);

            Button upgrade = AddButton(row, "SlotUpgrade_" + slot, "UPGRADE", 26, new Color(0.36f, 0.30f, 0.12f), out _slotUpgradeLabels[index]);
            upgrade.GetComponent<LayoutElement>().preferredWidth = 320;
            upgrade.GetComponent<LayoutElement>().flexibleWidth = 0f;
            EquipmentSlotType captured = slot;
            upgrade.onClick.AddListener(() => _onUpgradeSlot?.Invoke(captured));
            _slotUpgradeButtons[index] = upgrade;
        }

        private void BuildPendingPanel(RectTransform content, Action onEquip, Action onDiscard)
        {
            _pendingPanel = new GameObject("PendingPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            _pendingPanel.transform.SetParent(content, false);
            _pendingPanel.GetComponent<Image>().color = new Color(0.18f, 0.16f, 0.10f, 1f);
            var layout = _pendingPanel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 6;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            _pendingPanel.GetComponent<LayoutElement>().minHeight = 170;

            var panelRt = (RectTransform)_pendingPanel.transform;
            _pendingText = AddText(panelRt, "PendingText", 27, TextAnchor.UpperLeft, Color.white, 110);

            RectTransform pendingButtons = AddRow(panelRt, "PendingButtons", 70);
            Button equip = AddButton(pendingButtons, "EquipButton", "EQUIP", 32, new Color(0.20f, 0.45f, 0.30f), out _);
            equip.onClick.AddListener(() => onEquip?.Invoke());
            Button discard = AddButton(pendingButtons, "DiscardButton", "DISCARD", 32, new Color(0.5f, 0.22f, 0.22f), out _);
            discard.onClick.AddListener(() => onDiscard?.Invoke());

            _pendingPanel.SetActive(false);
        }

        /// <summary>Updates every field from the current game state.</summary>
        public void Refresh(GameController game)
        {
            _stageText.text = game.CurrentStage.DisplayName;
            _goldText.text = "Gold: " + game.Gold;
            _wheelsText.text = "Wheels: " + game.Wheels;

            _creatorText.text = game.CreatorAtMaxLevel
                ? $"Level {game.CreatorLevel} (MAX)"
                : $"Level {game.CreatorLevel}   XP {game.CreatorXp}/{game.CreatorXpToNextLevel}";
            _oddsText.text = BuildOddsText(game.CurrentRarityTable);

            _buildButton.interactable = !game.HasPendingItem && game.Wheels >= 1;
            _buildButtonLabel.text = game.Wheels >= 1 ? "BUILD ITEM (1 Wheel)" : "BUILD ITEM (No Wheels)";

            if (!game.IsAutoBuildUnlocked)
            {
                _autoBuildButton.interactable = false;
                _autoBuildButtonLabel.text = "AUTO BUILD (LOCKED)";
            }
            else
            {
                _autoBuildButton.interactable = true;
                _autoBuildButtonLabel.text = game.IsAutoBuildEnabled ? "AUTO BUILD: ON" : "AUTO BUILD: OFF";
            }

            RefreshPending(game);
            RefreshSlots(game);
        }

        private void RefreshPending(GameController game)
        {
            EquipmentItem pending = game.PendingItem;
            if (pending == null)
            {
                _pendingPanel.SetActive(false);
                return;
            }

            _pendingPanel.SetActive(true);
            EquipmentItem equipped = game.GetEquipped(pending.Slot);

            var sb = new StringBuilder();
            sb.AppendLine("NEW ITEM");
            sb.AppendLine($"{pending.Rarity}  {SlotName(pending.Slot)}");
            sb.AppendLine($"+{pending.AccelerationBonus:0.##} Accel   +{pending.TopSpeedBonus:0.##} Top");
            if (equipped == null)
            {
                sb.Append("Equipped: EMPTY");
            }
            else
            {
                double dAccel = pending.AccelerationBonus - equipped.AccelerationBonus;
                double dTop = pending.TopSpeedBonus - equipped.TopSpeedBonus;
                sb.Append($"Equipped: {equipped.Rarity} (+{equipped.AccelerationBonus:0.##}/+{equipped.TopSpeedBonus:0.##})  Δ {Signed(dAccel)}/{Signed(dTop)}");
            }
            _pendingText.text = sb.ToString();
        }

        private void RefreshSlots(GameController game)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                EquipmentSlotType slot = Slots[i];
                EquipmentItem item = game.GetEquipped(slot);
                int level = game.GetSlotLevel(slot);
                double slotAccel = game.GetSlotAccelerationBonus(slot);
                double slotTop = game.GetSlotTopSpeedBonus(slot);

                var sb = new StringBuilder();
                sb.AppendLine($"{SlotName(slot).ToUpperInvariant()} — LV. {level}");
                sb.AppendLine(item == null
                    ? "Item: EMPTY"
                    : $"Item: {item.Rarity}  +{item.AccelerationBonus:0.##}/+{item.TopSpeedBonus:0.##}");
                sb.Append($"Slot: +{slotAccel:0.##} Accel / +{slotTop:0.##} Top");
                _slotInfoTexts[i].text = sb.ToString();

                long cost = game.GetSlotUpgradeCost(slot);
                bool afford = game.CanAffordSlotUpgrade(slot);
                _slotUpgradeLabels[i].text = $"UPGRADE\n{cost} G";
                _slotUpgradeButtons[i].interactable = afford;
            }
        }

        private static string BuildOddsText(RarityTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Rarity odds:");
            foreach (RarityWeight w in table.Weights)
            {
                sb.AppendLine($"  {w.Rarity}: {w.ProbabilityPercent:0.##}%");
            }
            return sb.ToString().TrimEnd();
        }

        private static string Signed(double value) => (value >= 0 ? "+" : "") + value.ToString("0.##");

        private static string SlotName(EquipmentSlotType slot)
        {
            return slot == EquipmentSlotType.FuelSystem ? "Fuel System" :
                   slot == EquipmentSlotType.Ecu ? "ECU" : slot.ToString();
        }

        // ---- UI construction helpers (legacy uGUI, prototype) ----

        private RectTransform CreateScrollView(RectTransform panel)
        {
            var scrollGo = new GameObject("ProgressionScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(panel, false);
            Stretch((RectTransform)scrollGo.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 16f), new Vector2(-16f, -16f));
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            Stretch((RectTransform)viewportGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.offsetMin = new Vector2(0f, 0f);
            contentRt.offsetMax = new Vector2(0f, 0f);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = (RectTransform)viewportGo.transform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;

            return contentRt;
        }

        private Text AddText(RectTransform parent, string name, int fontSize, TextAnchor anchor, Color color, float minHeight, bool flexibleWidth = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;
            if (flexibleWidth) le.flexibleWidth = 1f;
            return text;
        }

        private RectTransform AddRow(RectTransform parent, string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;
            go.GetComponent<LayoutElement>().minHeight = height;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return (RectTransform)go.transform;
        }

        private Button AddButton(RectTransform parent, string name, string label, int fontSize, Color color, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            go.GetComponent<LayoutElement>().minHeight = 80;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            Stretch((RectTransform)textGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            labelText = textGo.GetComponent<Text>();
            labelText.font = _font;
            labelText.fontSize = fontSize;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;

            return go.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
