using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleRacer.Game.Core;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Campaign status card with progress bar and locked placeholders.</summary>
    public sealed class RacePanelView
    {
        private readonly GameObject _root;
        private readonly TextMeshProUGUI _chapterText;
        private readonly TextMeshProUGUI _stageText;
        private readonly Image _progressFill;

        public RacePanelView(RectTransform host)
        {
            _root = new GameObject("RacePanel", typeof(RectTransform));
            _root.transform.SetParent(host, false);
            UiFactory.Stretch((RectTransform)_root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform content = UiFactory.CreateScrollContent((RectTransform)_root.transform);

            var campaign = UiFactory.CreatePanel(content, "CampaignCard", UiTheme.CardElevated);
            campaign.gameObject.AddComponent<LayoutElement>().minHeight = 280f;
            var v = campaign.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(22, 22, 20, 20);
            v.spacing = 12;
            v.childControlWidth = true;
            v.childForceExpandWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;

            var rt = (RectTransform)campaign.transform;
            UiFactory.AddTmp(rt, "Header", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.PrimaryAccent, 48f, true, FontStyles.Bold).text = "CAMPAIGN";
            _chapterText = UiFactory.AddTmp(rt, "Chapter", UiTheme.FontHero, TextAlignmentOptions.Left,
                UiTheme.TextPrimary, 56f, true, FontStyles.Bold);
            _stageText = UiFactory.AddTmp(rt, "Stage", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 40f, true);
            UiFactory.AddProgressBar(rt, "StageProgress", 32f, out _progressFill);
            UiFactory.AddTmp(rt, "AutoNote", UiTheme.FontCaption, TextAlignmentOptions.Left,
                UiTheme.TextMuted, 34f, true).text = "Races autoplay. Tabs never pause the race.";

            AddLockedCard(content, "Normal 2", "LOCKED");
            AddLockedCard(content, "Normal 3", "LOCKED");

            UiFactory.AddTmp(content, "FutureHeader", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 40f, true, FontStyles.Bold).text = "FUTURE MODES";
            AddLockedCard(content, "Drag Racing", "COMING SOON");
            AddLockedCard(content, "Circuit Racing", "COMING SOON");
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(GameController game)
        {
            int stageNumber = game.CurrentStageIndex + 1;
            const int total = 10;
            _chapterText.text = "NORMAL 1";
            _stageText.text = $"Current Stage   {stageNumber} / {total}";
            _progressFill.fillAmount = Mathf.Clamp01(stageNumber / (float)total);
        }

        private static void AddLockedCard(RectTransform parent, string title, string status)
        {
            var card = UiFactory.CreatePanel(parent, "Locked_" + title.Replace(" ", ""), UiTheme.CardBackground);
            card.gameObject.AddComponent<LayoutElement>().minHeight = 96f;
            var h = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(20, 20, 16, 16);
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childForceExpandWidth = true;
            h.childControlHeight = true;
            h.childForceExpandHeight = true;
            var rt = (RectTransform)card.transform;
            UiFactory.AddTmp(rt, "Title", UiTheme.FontSubtitle, TextAlignmentOptions.Left,
                UiTheme.TextMuted, 40f, true, FontStyles.Bold).text = title;
            UiFactory.AddTmp(rt, "Status", UiTheme.FontBody, TextAlignmentOptions.Right,
                UiTheme.TextMuted, 40f, true).text = status;
        }
    }
}
