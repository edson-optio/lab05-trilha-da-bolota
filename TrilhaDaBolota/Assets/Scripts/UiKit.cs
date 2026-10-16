using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TrilhaDaBolota
{
    // Funções pequenas para montar a interface (Canvas, textos, imagens e botões) pelo código.
    public static class UiKit
    {
        static Font font;
        public static Font Font => font ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static RectTransform Canvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = .5f;
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            return (RectTransform)canvasObject.transform;
        }

        public static RectTransform Box(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Text Label(Transform parent, string text, int size, Vector2 anchor, Vector2 position, Vector2 boxSize,
            TextAnchor alignment = TextAnchor.MiddleCenter, Color? color = null)
        {
            var rect = Box("Texto", parent, anchor, position, boxSize);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = color ?? GameRules.Cream;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = GameRules.Ink;
            outline.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        public static Image Picture(Transform parent, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var image = Box("Imagem", parent, anchor, position, size).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static Image Panel(Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var image = Box("Painel", parent, anchor, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Button Button(Transform parent, string text, Vector2 anchor, Vector2 position, UnityAction onClick)
        {
            var rect = Box("Botão " + text, parent, anchor, position, new Vector2(340f, 64f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color32(122, 74, 36, 255);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color32(255, 214, 150, 255);
            colors.selectedColor = new Color32(255, 214, 150, 255);
            colors.pressedColor = new Color32(200, 160, 110, 255);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            var border = rect.gameObject.AddComponent<Outline>();
            border.effectColor = GameRules.Ink;
            border.effectDistance = new Vector2(4f, -4f);
            Label(rect, text, 30, new Vector2(.5f, .5f), Vector2.zero, rect.sizeDelta);
            return button;
        }
    }
}
