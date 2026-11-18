using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Editor
{
    internal sealed class UITKTweenPreviewViewport : VisualElement
    {
        private const string PreferencePrefix = "SWeetJelly.Tween.UITKPreview.";
        private const int MaximumDimension = 16384;
        private const float Margin = 16f;
        private readonly VisualElement surface;
        private readonly IntegerField widthField, heightField;
        private readonly FloatField zoomField;
        private readonly ToolbarButton fitButton;
        private readonly Action beforeResize;
        private int canvasWidth, canvasHeight;
        private float manualZoom, scale = 1f;
        private bool fit;
        private Vector2 pan;

        internal Toolbar Controls { get; }
        internal VisualElement Document { get; }
        internal bool IsLayoutReady => Mathf.Approximately(surface.layout.width, canvasWidth) &&
            Mathf.Approximately(surface.layout.height, canvasHeight);

        internal UITKTweenPreviewViewport(Action beforeResize)
        {
            this.beforeResize = beforeResize;
            name = "preview-canvas";
            AddToClassList("canvas");
            tooltip = "Wheel: zoom at pointer. Middle button drag: pan. Fit: center the whole canvas.";
            canvasWidth = Mathf.Clamp(EditorPrefs.GetInt(PreferencePrefix + "Width", 1920), 1, MaximumDimension);
            canvasHeight = Mathf.Clamp(EditorPrefs.GetInt(PreferencePrefix + "Height", 1080), 1, MaximumDimension);
            manualZoom = EditorPrefs.GetFloat(PreferencePrefix + "Zoom", 1f);
            manualZoom = float.IsFinite(manualZoom) ? Mathf.Clamp(manualZoom, .1f, 4f) : 1f;
            fit = EditorPrefs.GetBool(PreferencePrefix + "Fit", true);

            Controls = new Toolbar { name = "preview-view-controls" };
            widthField = new IntegerField("Canvas Width") { name = "preview-width", value = canvasWidth, isDelayed = true };
            heightField = new IntegerField("Height") { name = "preview-height", value = canvasHeight, isDelayed = true };
            widthField.tooltip = heightField.tooltip = "Preview only, 1–16384 logical UI pixels. Enter or leave the field to apply.";
            widthField.RegisterValueChangedCallback(evt => Resize(evt.newValue, canvasHeight));
            heightField.RegisterValueChangedCallback(evt => Resize(canvasWidth, evt.newValue));
            Controls.Add(widthField); Controls.Add(heightField);
            fitButton = new ToolbarButton(Fit) { name = "preview-fit", text = "Fit" };
            Controls.Add(fitButton);
            Controls.Add(new ToolbarButton(() => SetZoom(1f, contentRect.center)) { name = "preview-actual-size", text = "100%" });
            zoomField = new FloatField("Zoom %") { name = "preview-zoom", isDelayed = true };
            zoomField.tooltip = "Manual zoom: 10–400%. Fit may use a smaller or larger scale to show the entire canvas.";
            zoomField.RegisterValueChangedCallback(evt => SetZoom(evt.newValue / 100f, contentRect.center));
            Controls.Add(zoomField);
            Controls.Add(new Label("Preview only") { name = "preview-only-label" });

            surface = new VisualElement { name = "preview-surface", usageHints = UsageHints.DynamicTransform };
            surface.AddToClassList("preview-surface");
            Document = new VisualElement { name = "preview-document" };
            Document.AddToClassList("preview-document");
            surface.Add(Document); Add(surface);
            surface.style.width = canvasWidth; surface.style.height = canvasHeight;
            RegisterCallback<GeometryChangedEvent>(_ => UpdateView(true));
            RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            this.AddManipulator(new PanManipulator(this));
            UpdateView();
        }

        private void Resize(int width, int height)
        {
            if (width < 1 || height < 1 || width > MaximumDimension || height > MaximumDimension)
            {
                widthField.SetValueWithoutNotify(canvasWidth); heightField.SetValueWithoutNotify(canvasHeight);
                return;
            }
            if (width == canvasWidth && height == canvasHeight) return;
            beforeResize();
            canvasWidth = width; canvasHeight = height; pan = Vector2.zero;
            surface.style.width = width; surface.style.height = height;
            Save(); UpdateView();
        }

        private void Fit()
        {
            fit = true; pan = Vector2.zero;
            Save(); UpdateView();
        }

        private void SetZoom(float zoom, Vector2 anchor)
        {
            if (!float.IsFinite(zoom)) { zoomField.SetValueWithoutNotify(scale * 100f); return; }
            zoom = Mathf.Clamp(zoom, .1f, 4f);
            Vector2 center = contentRect.center;
            pan = (anchor - center) - (anchor - center - pan) * (zoom / scale);
            fit = false; manualZoom = zoom;
            Save(); UpdateView();
        }

        private void OnWheel(WheelEvent evt)
        {
            SetZoom(scale * Mathf.Exp(-evt.delta.y * .05f), this.WorldToLocal(evt.mousePosition));
            evt.StopPropagation();
        }

        private void UpdateView(bool preserveZoomInput = false)
        {
            var bounds = contentRect;
            if (bounds.width > 0f && bounds.height > 0f && float.IsFinite(bounds.width) && float.IsFinite(bounds.height))
            {
                scale = fit ? Mathf.Min(Mathf.Max(1f, bounds.width - Margin * 2f) / canvasWidth,
                    Mathf.Max(1f, bounds.height - Margin * 2f) / canvasHeight) : manualZoom;
                surface.style.scale = new Scale(new Vector3(scale, scale, 1f));
                Vector2 offset = bounds.center - new Vector2(canvasWidth, canvasHeight) * (scale * .5f) + pan;
                surface.style.translate = new Translate(offset.x, offset.y);
            }
            var focused = focusController?.focusedElement as VisualElement;
            if (!preserveZoomInput || focused == null || (!ReferenceEquals(focused, zoomField) && !zoomField.Contains(focused)))
                zoomField.SetValueWithoutNotify(scale * 100f);
            fitButton.EnableInClassList("active-fit", fit);
        }

        private void Save()
        {
            EditorPrefs.SetInt(PreferencePrefix + "Width", canvasWidth);
            EditorPrefs.SetInt(PreferencePrefix + "Height", canvasHeight);
            EditorPrefs.SetFloat(PreferencePrefix + "Zoom", manualZoom);
            EditorPrefs.SetBool(PreferencePrefix + "Fit", fit);
        }

        private sealed class PanManipulator : PointerManipulator
        {
            private readonly UITKTweenPreviewViewport view;
            private int pointer = -1;
            private Vector2 previous;
            internal PanManipulator(UITKTweenPreviewViewport view) => this.view = view;
            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(Down, TrickleDown.TrickleDown);
                target.RegisterCallback<PointerMoveEvent>(Move);
                target.RegisterCallback<PointerUpEvent>(Up);
                target.RegisterCallback<PointerCaptureOutEvent>(Lost);
            }
            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(Down, TrickleDown.TrickleDown);
                target.UnregisterCallback<PointerMoveEvent>(Move);
                target.UnregisterCallback<PointerUpEvent>(Up);
                target.UnregisterCallback<PointerCaptureOutEvent>(Lost);
                if (pointer >= 0 && target.HasPointerCapture(pointer)) target.ReleasePointer(pointer);
                pointer = -1;
            }
            private void Down(PointerDownEvent evt)
            {
                if (evt.button != 2 || pointer >= 0) return;
                pointer = evt.pointerId; previous = evt.position;
                view.SetZoom(view.scale, view.WorldToLocal(evt.position));
                target.CapturePointer(pointer);
                evt.StopPropagation();
            }
            private void Move(PointerMoveEvent evt)
            {
                if (evt.pointerId != pointer || !target.HasPointerCapture(pointer)) return;
                Vector2 position = evt.position;
                view.pan += position - previous; previous = position;
                view.UpdateView(); evt.StopPropagation();
            }
            private void Up(PointerUpEvent evt)
            {
                if (evt.pointerId != pointer || evt.button != 2) return;
                target.ReleasePointer(pointer); pointer = -1; evt.StopPropagation();
            }
            private void Lost(PointerCaptureOutEvent evt) { if (evt.pointerId == pointer) pointer = -1; }
        }
    }
}
