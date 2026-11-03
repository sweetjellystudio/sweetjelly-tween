namespace SWeetJelly.Samples.Tween
{
    using System;
    using TMPro;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class TweenUISampleConsumerBehaviour : MonoBehaviour
    {
        [Header("GUI Layout")]
        [SerializeField] private float guiX = 16f;
        [SerializeField] private float guiY = 16f;
        [SerializeField] private float labelWidth = 620f;
        [SerializeField] private float labelHeight = 24f;
        [SerializeField] private float labelRowSpacing = 28f;
        [SerializeField] private float buttonWidth = 180f;
        [SerializeField] private float smallButtonWidth = 86f;
        [SerializeField] private float buttonHeight = 28f;
        [SerializeField] private float buttonRowSpacing = 34f;
        [SerializeField] private float buttonPairGap = 8f;
        [SerializeField] private float sectionSpacing = 42f;

        [Header("Tween Settings")]
        [SerializeField] private EaseType canvasFadeEasing = EaseType.SineInOut;
        [SerializeField] private EaseType graphicColorEasing = EaseType.CubicInOut;
        [SerializeField] private EaseType graphicFadeEasing = EaseType.SineInOut;
        [SerializeField] private EaseType imageFillEasing = EaseType.CubicInOut;
        [SerializeField] private EaseType fontSizeEasing = EaseType.BackInOut;
        [SerializeField] private EaseType rectTransformEasing = EaseType.CubicInOut;
        [SerializeField] private EaseType layoutHeightEasing = EaseType.BackInOut;
        [SerializeField] private float canvasFadeDuration = 0.45f;
        [SerializeField] private float graphicColorDuration = 0.45f;
        [SerializeField] private float graphicFadeDuration = 0.45f;
        [SerializeField] private float imageFillDuration = 0.5f;
        [SerializeField] private float fontSizeDuration = 0.45f;
        [SerializeField] private float rectMoveDuration = 0.45f;
        [SerializeField] private float rectSizeDuration = 0.45f;
        [SerializeField] private float rectShakeDuration = 0.45f;
        [SerializeField] private float rectShakeMagnitude = 12f;
        [SerializeField] private float layoutHeightDuration = 0.45f;

        [Header("Panel")]
        [SerializeField] private string panelObjectName = "Tween UI Sample Panel";
        [SerializeField] private Vector2 panelAnchoredPosition = new Vector2(120f, 20f);
        [SerializeField] private Vector2 panelMovedPosition = new Vector2(220f, 20f);
        [SerializeField] private Vector2 panelSize = new Vector2(360f, 220f);
        [SerializeField] private Vector2 panelExpandedSize = new Vector2(420f, 260f);
        [SerializeField] private Color panelBackgroundColor = new Color(0.07f, 0.1f, 0.14f, 0.9f);
        [SerializeField] private float panelVisibleAlpha = 1f;
        [SerializeField] private float panelDimmedAlpha = 0.35f;

        [Header("Title")]
        [SerializeField] private string titleObjectName = "TMP Label";
        [SerializeField] private string titleText = "Tween UI Sample";
        [SerializeField] private Vector2 titlePosition = new Vector2(0f, 68f);
        [SerializeField] private Vector2 titleSize = new Vector2(320f, 54f);
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private float titleNormalFontSize = 28f;
        [SerializeField] private float titleLargeFontSize = 42f;

        [Header("Color Swatch")]
        [SerializeField] private string swatchObjectName = "Color Swatch";
        [SerializeField] private Vector2 swatchPosition = new Vector2(-80f, -18f);
        [SerializeField] private Vector2 swatchSize = new Vector2(96f, 56f);
        [SerializeField] private Color swatchInitialColor = new Color(1f, 0.55f, 0.25f, 1f);
        [SerializeField] private float swatchVisibleAlpha = 1f;
        [SerializeField] private float swatchDimmedAlpha = 0.25f;

        [Header("Fill Gauge")]
        [SerializeField] private string fillGaugeObjectName = "Fill Gauge";
        [SerializeField] private string fillGaugeBackgroundName = "Fill Gauge Background";
        [SerializeField] private string fillGaugeLabelName = "Fill Gauge Label";
        [SerializeField] private Vector2 fillGaugePosition = new Vector2(70f, -18f);
        [SerializeField] private Vector2 fillGaugeSize = new Vector2(140f, 28f);
        [SerializeField] private Color fillGaugeBackgroundColor = new Color(0.03f, 0.04f, 0.05f, 1f);
        [SerializeField] private Color fillGaugeColor = new Color(0.2f, 0.85f, 0.95f, 1f);
        [SerializeField] private Color fillGaugeLabelColor = Color.white;
        [SerializeField] private float fillGaugeLabelFontSize = 16f;
        [SerializeField] private float fillGaugeLowAmount = 0.15f;
        [SerializeField] private float fillGaugeHighAmount = 1f;

        [Header("Layout Height")]
        [SerializeField] private string layoutContainerName = "Layout Height Container";
        [SerializeField] private string layoutBlockName = "Layout Height Block";
        [SerializeField] private Vector2 layoutContainerPosition = new Vector2(0f, -78f);
        [SerializeField] private Vector2 layoutContainerSize = new Vector2(300f, 70f);
        [SerializeField] private float layoutLowHeight = 22f;
        [SerializeField] private float layoutHighHeight = 58f;
        [SerializeField] private Color layoutContainerColor = new Color(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color layoutBlockColor = new Color(0.65f, 0.35f, 1f, 0.85f);

        [Header("Fallback Canvas")]
        [SerializeField] private string fallbackCanvasName = "Tween UI Canvas";

        private ITweenService tweenService;
        private ITweenHandle activeHandle;
        private CanvasGroup panelGroup;
        private RectTransform panelRect;
        private Image panelBackground;
        private Image swatch;
        private Image fillGaugeBackground;
        private Image fillGauge;
        private TMP_Text fillGaugeLabel;
        private TMP_Text title;
        private LayoutElement layoutBlockElement;
        private Sprite runtimeSprite;
        private Texture2D runtimeTexture;
        private GameObject ownedPanel;
        private Canvas ownedCanvas;
        private string status = "Waiting for Tween service...";
        private bool faded;
        private bool swatchFaded;
        private bool filled;
        private bool enlarged;
        private bool panelMoved;
        private bool panelExpanded;
        private bool layoutExpanded;

        private void OnEnable()
        {
            BootstrapperBehaviour.Bootstrapped += OnBootstrapped;
            EnsureVisuals();
            if (ownedPanel != null) ownedPanel.SetActive(true);
            ResolveIfReady();
        }

        private void OnDisable()
        {
            BootstrapperBehaviour.Bootstrapped -= OnBootstrapped;
            KillActive();
            tweenService = null;
            if (ownedPanel != null) ownedPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (ownedPanel != null) Destroy(ownedPanel);
            if (ownedCanvas != null) Destroy(ownedCanvas.gameObject);
            if (runtimeSprite != null)
                Destroy(runtimeSprite);

            if (runtimeTexture != null)
                Destroy(runtimeTexture);
        }

        private void OnBootstrapped(object sender, EventArgs args)
        {
            ResolveIfReady();
        }

        private void ResolveIfReady()
        {
            if (tweenService != null || !BootstrapperBehaviour.IsBootstrapped || BootstrapperBehaviour.Services == null)
                return;

            if (!BootstrapperBehaviour.Services.TryResolve<ITweenService>(out tweenService))
            {
                status = "ITweenService is not registered.";
                return;
            }

            status = "Tween service is ready.";
        }

        private void OnGUI()
        {
            var x = guiX;
            var y = guiY;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), status);
            y += labelRowSpacing;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), FormatHandleState());
            y += labelRowSpacing + buttonPairGap;

            GUI.enabled = tweenService != null;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "CanvasGroup Fade"))
                PlayCanvasFade();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Graphic Color"))
                PlayGraphicColor();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Graphic Fade"))
                PlayGraphicFade();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Image Fill"))
                PlayImageFill();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "TMP Font Size"))
                PlayFontSize();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Rect Move"))
                PlayRectMove();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Rect Size"))
                PlayRectSize();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Rect Shake"))
                PlayRectShake();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Layout Height"))
                PlayLayoutHeight();

            y += sectionSpacing;

            GUI.enabled = activeHandle != null;

            if (GUI.Button(new Rect(x, y, smallButtonWidth, buttonHeight), "Pause"))
                activeHandle.Pause();

            if (GUI.Button(new Rect(x + smallButtonWidth + buttonPairGap, y, smallButtonWidth, buttonHeight), "Resume"))
                activeHandle.Resume();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, smallButtonWidth, buttonHeight), "Complete"))
                activeHandle.Complete();

            if (GUI.Button(new Rect(x + smallButtonWidth + buttonPairGap, y, smallButtonWidth, buttonHeight), "Kill"))
                activeHandle.Kill();

            y += sectionSpacing;

            GUI.enabled = true;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset UI"))
                ResetVisuals();
        }

        private void PlayCanvasFade()
        {
            KillActive();

            faded = !faded;
            var alpha = faded ? panelDimmedAlpha : panelVisibleAlpha;

            activeHandle = tweenService.Play(
                panelGroup.DOFade(alpha, canvasFadeDuration)
                    .SetEasing(canvasFadeEasing)
                    .AddOnCompleted(() => status = $"CanvasGroup alpha: {alpha:0.00}"));

            status = "CanvasGroup fade started.";
        }

        private void PlayGraphicColor()
        {
            KillActive();

            var color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);

            activeHandle = tweenService.Play(
                swatch.DOColor(color, graphicColorDuration)
                    .SetEasing(graphicColorEasing)
                    .AddOnCompleted(() => status = "Graphic color tween completed."));

            status = "Graphic color tween started.";
        }

        private void PlayGraphicFade()
        {
            KillActive();

            swatchFaded = !swatchFaded;
            var alpha = swatchFaded ? swatchDimmedAlpha : swatchVisibleAlpha;

            activeHandle = tweenService.Play(
                swatch.DOFade(alpha, graphicFadeDuration)
                    .SetEasing(graphicFadeEasing)
                    .AddOnCompleted(() => status = $"Graphic alpha: {alpha:0.00}"));

            status = "Graphic fade tween started.";
        }

        private void PlayImageFill()
        {
            KillActive();

            filled = !filled;
            var amount = filled ? fillGaugeHighAmount : fillGaugeLowAmount;

            activeHandle = tweenService.PlayGroup(
                fillGauge.DOFillAmount(amount, imageFillDuration)
                    .SetEasing(imageFillEasing),
                TweenCases.DOFloat(fillGauge.fillAmount, amount, imageFillDuration, SetFillGaugeLabel)
                    .SetEasing(imageFillEasing));

            activeHandle.Completed += () => status = $"Image fillAmount: {amount:0.00}";

            status = "Image fill tween started.";
        }

        private void PlayFontSize()
        {
            KillActive();

            enlarged = !enlarged;
            var fontSize = enlarged ? titleLargeFontSize : titleNormalFontSize;

            activeHandle = tweenService.Play(
                title.DOFontSize(fontSize, fontSizeDuration)
                    .SetEasing(fontSizeEasing)
                    .AddOnCompleted(() => status = $"TMP font size: {fontSize:0}"));

            status = "TMP font size tween started.";
        }

        private void PlayRectMove()
        {
            KillActive();

            panelMoved = !panelMoved;
            var position = panelMoved ? panelMovedPosition : panelAnchoredPosition;

            activeHandle = tweenService.Play(
                panelRect.DOAnchoredPosition(position, rectMoveDuration)
                    .SetEasing(rectTransformEasing)
                    .AddOnCompleted(() => status = $"Rect anchoredPosition: {position}"));

            status = "RectTransform position tween started.";
        }

        private void PlayRectSize()
        {
            KillActive();

            panelExpanded = !panelExpanded;
            var size = panelExpanded ? panelExpandedSize : panelSize;

            activeHandle = tweenService.Play(
                panelRect.DOSize(size, rectSizeDuration)
                    .SetEasing(rectTransformEasing)
                    .AddOnCompleted(() => status = $"Rect sizeDelta: {size}"));

            status = "RectTransform size tween started.";
        }

        private void PlayRectShake()
        {
            KillActive();

            activeHandle = tweenService.Play(
                panelRect.DOAnchoredPositionShake(rectShakeMagnitude, rectShakeDuration)
                    .SetEasing(rectTransformEasing)
                    .AddOnCompleted(() => status = "RectTransform shake completed."));

            status = "RectTransform shake tween started.";
        }

        private void PlayLayoutHeight()
        {
            KillActive();

            layoutExpanded = !layoutExpanded;
            var height = layoutExpanded ? layoutHighHeight : layoutLowHeight;

            activeHandle = tweenService.Play(
                layoutBlockElement.DOPreferredHeight(height, layoutHeightDuration)
                    .SetEasing(layoutHeightEasing)
                    .AddOnCompleted(() => status = $"Layout preferredHeight: {height:0}"));

            status = "LayoutElement preferred height tween started.";
        }

        private string FormatHandleState()
        {
            if (activeHandle == null)
                return "Active Handle: none";

            return $"Active: {activeHandle.IsActive}   Completed: {activeHandle.IsCompleted}   Killed: {activeHandle.IsKilled}";
        }

        private void KillActive()
        {
            activeHandle?.Kill();
            activeHandle = null;
        }

        private void ResetVisuals()
        {
            KillActive();

            panelGroup.alpha = panelVisibleAlpha;
            panelRect.anchoredPosition = panelAnchoredPosition;
            panelRect.sizeDelta = panelSize;
            panelBackground.color = panelBackgroundColor;
            swatch.color = swatchInitialColor;
            fillGauge.fillAmount = fillGaugeLowAmount;
            title.fontSize = titleNormalFontSize;
            layoutBlockElement.preferredHeight = layoutLowHeight;

            faded = false;
            swatchFaded = false;
            filled = false;
            enlarged = false;
            panelMoved = false;
            panelExpanded = false;
            layoutExpanded = false;

            SetFillGaugeLabel(fillGaugeLowAmount);
            status = tweenService == null ? "Waiting for Tween service..." : "UI sample reset.";
        }

        private void EnsureVisuals()
        {
            if (panelGroup != null)
                return;

            var canvas = BootstrapperBehaviour.Canvas != null
                ? BootstrapperBehaviour.Canvas
                : CreateCanvas();

            var panel = new GameObject(panelObjectName);
            ownedPanel = panel;
            panel.transform.SetParent(canvas.transform, false);
            panelGroup = panel.AddComponent<CanvasGroup>();

            panelRect = panel.AddComponent<RectTransform>();
            ApplyCenteredRect(panelRect, panelAnchoredPosition, panelSize);

            panelBackground = panel.AddComponent<Image>();
            panelBackground.sprite = GetRuntimeSprite();
            panelBackground.color = panelBackgroundColor;

            title = CreateText(panel.transform, titleObjectName, titleText, titlePosition, titleSize, titleNormalFontSize, titleColor);
            swatch = CreateImage(panel.transform, swatchObjectName, swatchPosition, swatchSize);
            swatch.sprite = GetRuntimeSprite();
            swatch.color = swatchInitialColor;

            fillGaugeBackground = CreateImage(panel.transform, fillGaugeBackgroundName, fillGaugePosition, fillGaugeSize);
            fillGaugeBackground.sprite = GetRuntimeSprite();
            fillGaugeBackground.color = fillGaugeBackgroundColor;

            fillGauge = CreateImage(panel.transform, fillGaugeObjectName, fillGaugePosition, fillGaugeSize);
            fillGauge.sprite = GetRuntimeSprite();
            fillGauge.type = Image.Type.Filled;
            fillGauge.fillMethod = Image.FillMethod.Horizontal;
            fillGauge.fillAmount = fillGaugeLowAmount;
            fillGauge.color = fillGaugeColor;

            fillGaugeLabel = CreateText(
                panel.transform,
                fillGaugeLabelName,
                string.Empty,
                fillGaugePosition,
                fillGaugeSize,
                fillGaugeLabelFontSize,
                fillGaugeLabelColor);

            CreateLayoutHeightBlock(panel.transform);
            ResetVisuals();
        }

        private void CreateLayoutHeightBlock(Transform parent)
        {
            var container = new GameObject(layoutContainerName);
            container.transform.SetParent(parent, false);

            var containerRect = container.AddComponent<RectTransform>();
            ApplyCenteredRect(containerRect, layoutContainerPosition, layoutContainerSize);

            var containerImage = container.AddComponent<Image>();
            containerImage.sprite = GetRuntimeSprite();
            containerImage.color = layoutContainerColor;

            var layoutGroup = container.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(6, 6, 6, 6);

            var block = new GameObject(layoutBlockName);
            block.transform.SetParent(container.transform, false);

            var blockImage = block.AddComponent<Image>();
            blockImage.sprite = GetRuntimeSprite();
            blockImage.color = layoutBlockColor;

            layoutBlockElement = block.AddComponent<LayoutElement>();
            layoutBlockElement.preferredHeight = layoutLowHeight;
            layoutBlockElement.flexibleWidth = 1f;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string objectName,
            string text,
            Vector2 position,
            Vector2 size,
            float fontSize,
            Color color)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            ApplyCenteredRect(rect, position, size);

            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            return label;
        }

        private static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);

            var rect = imageObject.AddComponent<RectTransform>();
            ApplyCenteredRect(rect, position, size);

            return imageObject.AddComponent<Image>();
        }

        private static void ApplyCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private Canvas CreateCanvas()
        {
            var canvasObject = new GameObject(fallbackCanvasName);
            var canvas = canvasObject.AddComponent<Canvas>();
            ownedCanvas = canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void SetFillGaugeLabel(float amount)
        {
            if (fillGaugeLabel == null)
                return;

            fillGaugeLabel.text = $"{Mathf.Clamp01(amount):P0}";
        }

        private Sprite GetRuntimeSprite()
        {
            if (runtimeSprite != null)
                return runtimeSprite;

            runtimeTexture = new Texture2D(1, 1);
            runtimeTexture.SetPixel(0, 0, Color.white);
            runtimeTexture.Apply();

            runtimeSprite = Sprite.Create(
                runtimeTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f));

            return runtimeSprite;
        }
    }
}
