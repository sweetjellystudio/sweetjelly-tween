using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Editor
{
    internal sealed class UITKTweenEditorLayout : VisualElement
    {
        private const string PreferencePrefix = "SWeetJelly.Tween.UITKEditor.Layout.";
        private readonly TwoPaneSplitView leftSplit, rightSplit, bottomSplit;

        internal UITKTweenEditorLayout(VisualElement left, VisualElement preview, VisualElement right,
            VisualElement bottom, VisualElement playback)
        {
            name = "editor-layout";
            AddToClassList("editor-layout");
            leftSplit = Split("elements-split", 0, Read("Left", 220), TwoPaneSplitViewOrientation.Horizontal, left, preview);
            rightSplit = Split("inspector-split", 1, Read("Right", 310), TwoPaneSplitViewOrientation.Horizontal, leftSplit, right);
            var upper = new VisualElement(); upper.AddToClassList("upper-workspace");
            upper.Add(rightSplit); upper.Add(playback);
            bottomSplit = Split("hierarchy-split", 1, Read("Bottom", 240), TwoPaneSplitViewOrientation.Vertical, upper, bottom);
            Add(bottomSplit);
            Remember(left, "Left", true);
            Remember(right, "Right", true);
            Remember(bottom, "Bottom", false);
        }

        internal void Reset()
        {
            leftSplit.fixedPaneInitialDimension = 220;
            rightSplit.fixedPaneInitialDimension = 310;
            bottomSplit.fixedPaneInitialDimension = 240;
        }

        private static float Read(string key, float fallback)
        {
            float value = EditorPrefs.GetFloat(PreferencePrefix + key, fallback);
            return float.IsFinite(value) && value > 0 ? value : fallback;
        }

        private static TwoPaneSplitView Split(string name, int fixedIndex, float size,
            TwoPaneSplitViewOrientation orientation, VisualElement first, VisualElement second)
        {
            var split = new TwoPaneSplitView(fixedIndex, size, orientation) { name = name };
            split.AddToClassList("editor-split"); split.Add(first); split.Add(second);
            return split;
        }

        private static void Remember(VisualElement pane, string key, bool horizontal)
        {
            pane.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                float size = horizontal ? evt.newRect.width : evt.newRect.height;
                if (float.IsFinite(size) && size > 0) EditorPrefs.SetFloat(PreferencePrefix + key, size);
            });
        }
    }
}
