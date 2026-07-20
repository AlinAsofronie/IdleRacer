using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleRacer.Game.Equipment;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Shared TMP + uGUI construction helpers for the playtest shell.</summary>
    public static class UiFactory
    {
        private static TMP_FontAsset _font;

        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = TMP_Settings.defaultFontAsset;
                }
                return _font;
            }
        }

        public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static void ApplySafeArea(RectTransform rect)
        {
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1f, Screen.width);
            float h = Mathf.Max(1f, Screen.height);
            rect.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static Image CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static RectTransform CreateScrollContent(RectTransform panel, float bottomPadding = 8f)
        {
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(panel, false);
            Stretch((RectTransform)scrollGo.transform, Vector2.zero, Vector2.one, new Vector2(UiTheme.SpaceMd, bottomPadding), new Vector2(-UiTheme.SpaceMd, -UiTheme.SpaceSm));
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
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 8, 16);
            vlg.spacing = UiTheme.SpaceMd;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = (RectTransform)viewportGo.transform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return contentRt;
        }

        public static TextMeshProUGUI AddTmp(
            RectTransform parent,
            string name,
            float fontSize,
            TextAlignmentOptions align,
            Color color,
            float minHeight,
            bool flexibleWidth = false,
            FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;
            if (flexibleWidth) le.flexibleWidth = 1f;
            return tmp;
        }

        public static RectTransform AddRow(RectTransform parent, string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = UiTheme.SpaceSm;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return (RectTransform)go.transform;
        }

        public static Button AddButton(
            RectTransform parent,
            string name,
            string label,
            float fontSize,
            Color color,
            out TextMeshProUGUI labelText,
            float height = UiTheme.TouchMinHeight)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            Stretch((RectTransform)textGo.transform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            labelText = textGo.GetComponent<TextMeshProUGUI>();
            labelText.font = Font;
            labelText.fontSize = fontSize;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = UiTheme.TextPrimary;
            labelText.text = label;
            labelText.fontStyle = FontStyles.Bold;
            labelText.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        public static Image AddProgressBar(RectTransform parent, string name, float height, out Image fill)
        {
            var trackGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            trackGo.transform.SetParent(parent, false);
            var track = trackGo.GetComponent<Image>();
            track.color = UiTheme.ProgressTrack;
            var le = trackGo.GetComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(trackGo.transform, false);
            fill = fillGo.GetComponent<Image>();
            fill.color = UiTheme.ProgressFill;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            Stretch((RectTransform)fillGo.transform, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));
            return track;
        }

        public static string SlotDisplayName(EquipmentSlotType slot)
        {
            return slot == EquipmentSlotType.FuelSystem ? "Fuel System" :
                   slot == EquipmentSlotType.Ecu ? "ECU" : slot.ToString();
        }

        public static string Signed(double value) => (value >= 0 ? "+" : string.Empty) + value.ToString("0.00");
    }
}
