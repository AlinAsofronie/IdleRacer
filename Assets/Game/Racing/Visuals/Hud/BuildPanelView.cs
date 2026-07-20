using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;
using IdleRacer.Game.Equipment.Items;
using IdleRacer.Game.Equipment.Rarities;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Item Creator–focused Build tab with prominent CTA and rarity rows.</summary>
    public sealed class BuildPanelView
    {
        private readonly GameObject _root;
        private readonly TextMeshProUGUI _levelText;
        private readonly TextMeshProUGUI _xpLabel;
        private readonly Image _xpFill;
        private readonly TextMeshProUGUI _wheelsCostText;
        private readonly TextMeshProUGUI _buildLabel;
        private readonly TextMeshProUGUI _autoLabel;
        private readonly Button _buildButton;
        private readonly Button _autoButton;
        private readonly Image _buildImage;
        private readonly GameObject _pendingPanel;
        private readonly Image _pendingAccent;
        private readonly Image _pendingBorder;
        private readonly TextMeshProUGUI _pendingRarity;
        private readonly TextMeshProUGUI _pendingSlot;
        private readonly TextMeshProUGUI _pendingAccel;
        private readonly TextMeshProUGUI _pendingTop;
        private readonly SimpleUiReveal _reveal;
        private readonly GameObject _oddsHeader;
        private readonly GameObject _oddsCard;
        private readonly TextMeshProUGUI[] _oddsLabels = new TextMeshProUGUI[6];
        private readonly TextMeshProUGUI[] _oddsValues = new TextMeshProUGUI[6];
        private bool _wasPending;

        public BuildPanelView(
            RectTransform host,
            Action onBuild,
            Action onToggleAuto,
            Action onEquip,
            Action onDiscard)
        {
            _root = new GameObject("BuildPanel", typeof(RectTransform));
            _root.transform.SetParent(host, false);
            UiFactory.Stretch((RectTransform)_root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform content = UiFactory.CreateScrollContent((RectTransform)_root.transform);

            var creator = UiFactory.CreatePanel(content, "ItemCreatorCard", UiTheme.CardElevated);
            var creatorLe = creator.gameObject.AddComponent<LayoutElement>();
            creatorLe.minHeight = 360f;
            var creatorV = creator.gameObject.AddComponent<VerticalLayoutGroup>();
            creatorV.padding = new RectOffset(20, 20, 16, 16);
            creatorV.spacing = UiTheme.SpaceSm;
            creatorV.childControlWidth = true;
            creatorV.childForceExpandWidth = true;
            creatorV.childControlHeight = true;
            creatorV.childForceExpandHeight = false;

            var creatorRt = (RectTransform)creator.transform;
            UiFactory.AddTmp(creatorRt, "Header", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.PrimaryAccent, 48f, true, FontStyles.Bold).text = "ITEM CREATOR";
            _levelText = UiFactory.AddTmp(creatorRt, "Level", UiTheme.FontHero, TextAlignmentOptions.Left,
                UiTheme.TextPrimary, 56f, true, FontStyles.Bold);
            _xpLabel = UiFactory.AddTmp(creatorRt, "XpLabel", UiTheme.FontBody, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 34f, true);
            UiFactory.AddProgressBar(creatorRt, "XpBar", 28f, out _xpFill);

            _wheelsCostText = UiFactory.AddTmp(creatorRt, "WheelCost", UiTheme.FontSubtitle, TextAlignmentOptions.Center,
                UiTheme.Wheels, 40f, true, FontStyles.Bold);

            _buildButton = UiFactory.AddButton(creatorRt, "BuildButton", "BUILD ITEM", UiTheme.FontCta,
                UiTheme.CtaBuild, out _buildLabel, 120f);
            _buildImage = _buildButton.GetComponent<Image>();
            _buildButton.onClick.AddListener(() => onBuild?.Invoke());

            _autoButton = UiFactory.AddButton(creatorRt, "AutoBuildButton", "AUTO BUILD", UiTheme.FontBody,
                UiTheme.NavInactive, out _autoLabel, UiTheme.TouchMinHeight);
            _autoButton.onClick.AddListener(() => onToggleAuto?.Invoke());

            _pendingPanel = new GameObject("PendingCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup));
            _pendingPanel.transform.SetParent(content, false);
            _pendingBorder = _pendingPanel.GetComponent<Image>();
            _pendingBorder.color = UiTheme.CardElevated;
            var pendingV = _pendingPanel.GetComponent<VerticalLayoutGroup>();
            pendingV.padding = new RectOffset(18, 18, 16, 16);
            pendingV.spacing = 10;
            pendingV.childControlWidth = true;
            pendingV.childForceExpandWidth = true;
            pendingV.childControlHeight = true;
            pendingV.childForceExpandHeight = false;
            _pendingPanel.GetComponent<LayoutElement>().minHeight = 440f;
            _reveal = _pendingPanel.AddComponent<SimpleUiReveal>();

            var pendingRt = (RectTransform)_pendingPanel.transform;
            _pendingAccent = UiFactory.CreatePanel(pendingRt, "Accent", UiTheme.PrimaryAccent).GetComponent<Image>();
            _pendingAccent.gameObject.AddComponent<LayoutElement>().minHeight = 10f;

            _pendingRarity = UiFactory.AddTmp(pendingRt, "Rarity", UiTheme.FontHero, TextAlignmentOptions.Center,
                UiTheme.TextPrimary, 52f, true, FontStyles.Bold);
            _pendingSlot = UiFactory.AddTmp(pendingRt, "Slot", UiTheme.FontSubtitle, TextAlignmentOptions.Center,
                UiTheme.TextSecondary, 36f, true);

            // Actions first so Equip/Discard stay visible without scrolling past the full card.
            RectTransform actions = UiFactory.AddRow(pendingRt, "PendingActions", 104f);
            Button equip = UiFactory.AddButton(actions, "EquipButton", "EQUIP", UiTheme.FontCta, UiTheme.CtaEquip, out _, 104f);
            equip.onClick.AddListener(() => onEquip?.Invoke());
            Button discard = UiFactory.AddButton(actions, "DiscardButton", "DISCARD", UiTheme.FontBody, UiTheme.CtaDiscard, out _, 104f);
            discard.onClick.AddListener(() => onDiscard?.Invoke());

            _pendingAccel = UiFactory.AddTmp(pendingRt, "AccelBlock", UiTheme.FontBody, TextAlignmentOptions.TopLeft,
                UiTheme.TextPrimary, 130f, true);
            _pendingTop = UiFactory.AddTmp(pendingRt, "TopBlock", UiTheme.FontBody, TextAlignmentOptions.TopLeft,
                UiTheme.TextPrimary, 130f, true);
            _pendingPanel.SetActive(false);

            _oddsHeader = UiFactory.AddTmp(content, "OddsHeader", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 40f, true, FontStyles.Bold).gameObject;
            _oddsHeader.GetComponent<TextMeshProUGUI>().text = "DROP CHANCES";

            var oddsCard = UiFactory.CreatePanel(content, "OddsCard", UiTheme.CardBackground);
            _oddsCard = oddsCard.gameObject;
            _oddsCard.AddComponent<LayoutElement>().minHeight = 300f;
            var oddsV = _oddsCard.AddComponent<VerticalLayoutGroup>();
            oddsV.padding = new RectOffset(14, 14, 10, 10);
            oddsV.spacing = 6;
            oddsV.childControlWidth = true;
            oddsV.childForceExpandWidth = true;
            oddsV.childControlHeight = true;
            oddsV.childForceExpandHeight = false;

            EquipmentRarity[] rarities =
            {
                EquipmentRarity.Common, EquipmentRarity.Uncommon, EquipmentRarity.Rare,
                EquipmentRarity.Epic, EquipmentRarity.Legendary, EquipmentRarity.Mythic
            };
            for (int i = 0; i < rarities.Length; i++)
            {
                RectTransform row = UiFactory.AddRow((RectTransform)_oddsCard.transform, "Odds_" + rarities[i], 44f);
                row.gameObject.AddComponent<Image>().color = UiTheme.OddsRowBackground;
                _oddsLabels[i] = UiFactory.AddTmp(row, "Name", UiTheme.FontBody, TextAlignmentOptions.Left,
                    UiTheme.Rarity(rarities[i]), 40f, true, FontStyles.Bold);
                _oddsLabels[i].text = rarities[i].ToString();
                _oddsValues[i] = UiFactory.AddTmp(row, "Pct", UiTheme.FontBody, TextAlignmentOptions.Right,
                    UiTheme.TextPrimary, 40f, true, FontStyles.Bold);
            }
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(GameController game)
        {
            _levelText.text = game.CreatorAtMaxLevel
                ? $"Level {game.CreatorLevel}  MAX"
                : $"Level {game.CreatorLevel}";
            if (game.CreatorAtMaxLevel)
            {
                _xpLabel.text = "XP  MAX";
                _xpFill.fillAmount = 1f;
            }
            else
            {
                _xpLabel.text = $"XP  {game.CreatorXp} / {game.CreatorXpToNextLevel}";
                _xpFill.fillAmount = game.CreatorXpToNextLevel <= 0
                    ? 0f
                    : Mathf.Clamp01((float)game.CreatorXp / game.CreatorXpToNextLevel);
            }

            _wheelsCostText.text = "Cost  1 Wheel   ·   You have  " + CompactNumberFormatter.Format(game.Wheels);

            bool canBuild = !game.HasPendingItem && game.Wheels >= 1;
            _buildButton.interactable = canBuild;
            _buildImage.color = canBuild ? UiTheme.CtaBuild : UiTheme.Disabled;
            _buildLabel.text = game.Wheels >= 1 ? "BUILD ITEM" : "NEED WHEELS";

            if (!game.IsAutoBuildUnlocked)
            {
                _autoButton.interactable = false;
                _autoLabel.text = "AUTO BUILD  ·  LOCKED";
                _autoButton.GetComponent<Image>().color = UiTheme.Disabled;
            }
            else
            {
                _autoButton.interactable = true;
                bool on = game.IsAutoBuildEnabled;
                _autoLabel.text = on ? "AUTO BUILD  ·  ON" : "AUTO BUILD  ·  OFF";
                _autoButton.GetComponent<Image>().color = on ? UiTheme.PrimaryAccent : UiTheme.NavInactive;
            }

            RefreshOdds(game.CurrentRarityTable);
            RefreshPending(game);

            // Keep the reveal above the fold: hide drop chances while a decision is pending.
            bool showOdds = !game.HasPendingItem;
            _oddsHeader.SetActive(showOdds);
            _oddsCard.SetActive(showOdds);
        }

        private void RefreshOdds(RarityTable table)
        {
            EquipmentRarity[] order =
            {
                EquipmentRarity.Common, EquipmentRarity.Uncommon, EquipmentRarity.Rare,
                EquipmentRarity.Epic, EquipmentRarity.Legendary, EquipmentRarity.Mythic
            };
            for (int i = 0; i < order.Length; i++)
            {
                double pct = 0;
                foreach (RarityWeight w in table.Weights)
                {
                    if (w.Rarity == order[i])
                    {
                        pct = w.ProbabilityPercent;
                        break;
                    }
                }
                _oddsValues[i].text = pct.ToString("0.##") + "%";
                _oddsLabels[i].color = UiTheme.Rarity(order[i]);
            }
        }

        private void RefreshPending(GameController game)
        {
            EquipmentItem pending = game.PendingItem;
            if (pending == null)
            {
                _pendingPanel.SetActive(false);
                _wasPending = false;
                return;
            }

            bool justShown = !_wasPending;
            _pendingPanel.SetActive(true);
            _wasPending = true;

            Color rarity = UiTheme.Rarity(pending.Rarity);
            _pendingAccent.color = rarity;
            _pendingBorder.color = Color.Lerp(UiTheme.CardElevated, rarity, 0.22f);
            _pendingRarity.color = rarity;
            _pendingRarity.text = pending.Rarity.ToString().ToUpperInvariant();
            _pendingSlot.text = UiFactory.SlotDisplayName(pending.Slot);

            EquipmentItem equipped = game.GetEquipped(pending.Slot);
            double curA = equipped?.AccelerationBonus ?? 0.0;
            double curT = equipped?.TopSpeedBonus ?? 0.0;
            double dA = pending.AccelerationBonus - curA;
            double dT = pending.TopSpeedBonus - curT;

            _pendingAccel.text =
                $"Acceleration\nNew  {UiFactory.Signed(pending.AccelerationBonus)}\nNow  {(equipped == null ? "EMPTY" : UiFactory.Signed(curA))}\nΔ    {UiFactory.Signed(dA)}";
            _pendingAccel.color = UiTheme.Delta(dA);
            _pendingAccel.lineSpacing = 8f;
            _pendingTop.text =
                $"Top Speed\nNew  {UiFactory.Signed(pending.TopSpeedBonus)}\nNow  {(equipped == null ? "EMPTY" : UiFactory.Signed(curT))}\nΔ    {UiFactory.Signed(dT)}";
            _pendingTop.color = UiTheme.Delta(dT);
            _pendingTop.lineSpacing = 8f;

            if (justShown)
            {
                _reveal.Play();
            }
        }
    }
}
