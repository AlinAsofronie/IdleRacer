using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Large locked feature cards for future systems.</summary>
    public sealed class MorePanelView
    {
        private readonly GameObject _root;

        private static readonly string[] Titles =
        {
            "Dungeons", "Skills", "Companions", "Trainers", "Prestige", "Arena", "Collection"
        };

        public MorePanelView(RectTransform host)
        {
            _root = new GameObject("MorePanel", typeof(RectTransform));
            _root.transform.SetParent(host, false);
            UiFactory.Stretch((RectTransform)_root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform content = UiFactory.CreateScrollContent((RectTransform)_root.transform);
            UiFactory.AddTmp(content, "Header", UiTheme.FontTitle, TextAlignmentOptions.Left,
                UiTheme.TextSecondary, 48f, true, FontStyles.Bold).text = "MORE";
            UiFactory.AddTmp(content, "Subtitle", UiTheme.FontBody, TextAlignmentOptions.Left,
                UiTheme.TextMuted, 36f, true).text = "Future systems — locked for now.";

            var gridGo = new GameObject("FeatureGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            gridGo.transform.SetParent(content, false);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(470f, 160f);
            grid.spacing = new Vector2(UiTheme.SpaceMd, UiTheme.SpaceMd);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            gridGo.GetComponent<LayoutElement>().minHeight = 600f;
            gridGo.GetComponent<LayoutElement>().preferredHeight = 600f;

            foreach (string title in Titles)
            {
                var card = UiFactory.CreatePanel(gridGo.transform, "Feature_" + title, UiTheme.CardBackground);
                var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(18, 18, 22, 18);
                v.spacing = 8;
                v.childAlignment = TextAnchor.MiddleCenter;
                v.childControlWidth = true;
                v.childForceExpandWidth = true;
                v.childControlHeight = true;
                v.childForceExpandHeight = false;
                var rt = (RectTransform)card.transform;
                UiFactory.AddTmp(rt, "Title", UiTheme.FontSubtitle, TextAlignmentOptions.Center,
                    UiTheme.TextMuted, 44f, true, FontStyles.Bold).text = title;
                UiFactory.AddTmp(rt, "Status", UiTheme.FontCaption, TextAlignmentOptions.Center,
                    UiTheme.Disabled, 36f, true).text = "COMING SOON";
            }
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh()
        {
        }
    }
}
