using UnityEngine;
using UnityEngine.Events;

namespace WarHoller.UI
{
    /// <summary>
    /// Runtime uGUI helpers — no prefabs required.
    /// Factory method names intentionally avoid UnityEngine.UI type names (Button/Slider/Image/Text)
    /// so they cannot shadow those types (CS0119). UI types are fully qualified.
    /// </summary>
    public static class UiFactory
    {
        public static Font DefaultFont
        {
            get
            {
                var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return f;
            }
        }

        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvas;
        }

        public static GameObject EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return null;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Active Input Handling is Both (legacy + Input System). Prefer legacy Standalone
            // so menu clicks work with Input.* without an Input Actions asset.
            // If Active Input Handling is switched to Input System only, swap this for
            // UnityEngine.InputSystem.UI.InputSystemUIInputModule.
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            return es;
        }

        public static UnityEngine.UI.Text Label(Transform parent, string text, int size, TextAnchor anchor, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var t = go.AddComponent<UnityEngine.UI.Text>();
            t.font = DefaultFont;
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static UnityEngine.UI.Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 sizeDelta, UnityAction onClick, Color? bg = null)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = bg ?? new Color(0.25f, 0.18f, 0.1f, 0.92f);

            var btn = go.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var text = Label(go.transform, label, 28, TextAnchor.MiddleCenter, new Color(0.95f, 0.9f, 0.75f), Vector2.zero, sizeDelta);
            text.raycastTarget = false;
            return btn;
        }

        public static UnityEngine.UI.Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, float value, UnityAction<float> onChanged)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var bg = go.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillRt = fillArea.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0.25f);
            fillRt.anchorMax = new Vector2(1f, 0.75f);
            fillRt.offsetMin = new Vector2(5f, 0f);
            fillRt.offsetMax = new Vector2(-5f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<UnityEngine.UI.Image>();
            fillImg.color = new Color(0.75f, 0.55f, 0.2f, 1f);
            var fillImgRt = fill.GetComponent<RectTransform>();
            fillImgRt.anchorMin = Vector2.zero;
            fillImgRt.anchorMax = Vector2.one;
            fillImgRt.offsetMin = Vector2.zero;
            fillImgRt.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRt = handleArea.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = new Vector2(10f, 0f);
            handleAreaRt.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleImg = handle.AddComponent<UnityEngine.UI.Image>();
            handleImg.color = new Color(0.95f, 0.85f, 0.55f);
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20f, 0f);

            var slider = go.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fillImgRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        public static UnityEngine.UI.Image Panel(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, Color color, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            if (anchorMin.HasValue) rt.anchorMin = anchorMin.Value;
            if (anchorMax.HasValue) rt.anchorMax = anchorMax.Value;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            return img;
        }
    }
}
