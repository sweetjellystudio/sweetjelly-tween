using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SWeetJelly.Tween.Editor;
using SWeetJelly.Tween.Framework.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Tests.Authoring
{
    public sealed class UITKTweenPreviewViewportTest
    {
        private const string Folder = "Assets/__UITKTweenPreviewTests";
        private const string Prefix = "SWeetJelly.Tween.UITKPreview.";
        private readonly Dictionary<string, object> preferences = new();
        private UITKTweenAsset asset;
        private UITKTweenEditor window;
        private bool ownsFixture;
        private string assetBefore, documentBefore;
        private VisualElement View => window.rootVisualElement.Q("preview-canvas");
        private VisualElement Surface => View.Q("preview-surface");
        private VisualElement Root => View.Q("documentRoot");
        private IntegerField Width => window.rootVisualElement.Q<IntegerField>("preview-width");
        private IntegerField Height => window.rootVisualElement.Q<IntegerField>("preview-height");
        private FloatField Zoom => window.rootVisualElement.Q<FloatField>("preview-zoom");
        private Slider Time => window.rootVisualElement.Q<Slider>("preview-time");

        [UnitySetUp] public IEnumerator SetUp()
        {
            ownsFixture = false;
            foreach (string key in new[] { "Width", "Height", "Zoom", "Fit" })
            {
                string full = Prefix + key;
                preferences[key] = !EditorPrefs.HasKey(full) ? null : key == "Zoom" ? (object)EditorPrefs.GetFloat(full) :
                    key == "Fit" ? (object)EditorPrefs.GetBool(full) : EditorPrefs.GetInt(full);
                EditorPrefs.DeleteKey(full);
            }
            Assert.That(AssetDatabase.IsValidFolder(Folder), Is.False, "Preserve a pre-existing fixture folder.");
            AssetDatabase.CreateFolder("Assets", "__UITKTweenPreviewTests"); ownsFixture = true;
            documentBefore = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\"><ui:Style src=\"Layout.uss\"/><ui:VisualElement name=\"documentRoot\"><ui:VisualElement name=\"target\"/></ui:VisualElement></ui:UXML>";
            File.WriteAllText(Folder + "/Layout.uxml", documentBefore);
            File.WriteAllText(Folder + "/Layout.uss", "#documentRoot { flex-grow: 1; } #target { position: absolute; left: 25%; top: 30%; width: 10%; height: 20%; opacity: 1; }");
            AssetDatabase.Refresh();
            asset = ScriptableObject.CreateInstance<UITKTweenAsset>();
            asset.AuthoringDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Folder + "/Layout.uxml");
            asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Group, Children = new()
            {
                new UITKTweenNode { TargetName = "target", FromCurrent = false, From = Vector2.zero, To = Vector2.one, Duration = 1 },
                new UITKTweenNode { Property = UITKTweenProperty.Scale, FromCurrent = false, From = Vector2.one, To = Vector2.one * 2, Duration = 1 }
            } };
            AssetDatabase.CreateAsset(asset, Folder + "/Tween.asset"); AssetDatabase.SaveAssets();
            assetBefore = File.ReadAllText(Folder + "/Tween.asset");
            window = UITKTweenEditor.Open(asset); window.position = new Rect(30, 30, 1200, 850); window.Focus();
            yield return null; yield return null;
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            if (window != null) window.Close();
            foreach (var item in preferences)
            {
                string full = Prefix + item.Key;
                if (item.Value == null) EditorPrefs.DeleteKey(full);
                else if (item.Value is float number) EditorPrefs.SetFloat(full, number);
                else if (item.Value is bool flag) EditorPrefs.SetBool(full, flag);
                else EditorPrefs.SetInt(full, (int)item.Value);
            }
            preferences.Clear();
            if (ownsFixture) AssetDatabase.DeleteAsset(Folder);
            ownsFixture = false; yield return null;
        }

        private void Click(string name)
        {
            var button = window.rootVisualElement.Q<Button>(name);
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = button.worldBound.center })) button.SendEvent(down);
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = button.worldBound.center })) button.SendEvent(up);
        }
        private static void Key(VisualElement target, char character, KeyCode code = KeyCode.None, EventModifiers modifiers = EventModifiers.None)
        {
            using var evt = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, character = character, keyCode = code, modifiers = modifiers });
            target.SendEvent(evt);
        }
        private void AssertUnchangedAssets()
        {
            Assert.That(EditorUtility.IsDirty(asset), Is.False);
            Assert.That(File.ReadAllText(Folder + "/Tween.asset"), Is.EqualTo(assetBefore));
            Assert.That(File.ReadAllText(Folder + "/Layout.uxml"), Is.EqualTo(documentBefore));
        }

        [UnityTest] public IEnumerator FitAndResizeKeepLogicalCanvasAndPercentageLayout()
        {
            Assert.That(Width.value, Is.EqualTo(1920)); Assert.That(Height.value, Is.EqualTo(1080));
            var root = Root; var target = root.Q("target"); var local = target.layout;
            Assert.That(root.layout.width, Is.EqualTo(1920).Within(.01)); Assert.That(root.layout.height, Is.EqualTo(1080).Within(.01));
            Assert.That(local.x, Is.EqualTo(480).Within(.01)); Assert.That(local.y, Is.EqualTo(324).Within(.01));
            float before = Surface.worldBound.width / 1920;
            window.position = new Rect(30, 30, 1500, 950); yield return null; yield return null;
            Assert.That(Root, Is.SameAs(root)); Assert.That(target.layout, Is.EqualTo(local));
            float scale = Surface.worldBound.width / 1920;
            Assert.That(scale, Is.GreaterThan(before));
            Assert.That(Surface.worldBound.height / 1080, Is.EqualTo(scale).Within(.0001));
            Assert.That(Surface.worldBound.center.x, Is.EqualTo(View.worldBound.center.x).Within(.1));
            Assert.That(Surface.worldBound.center.y, Is.EqualTo(View.worldBound.center.y).Within(.1));
            AssertUnchangedAssets();
        }

        [UnityTest] public IEnumerator ZoomAndResizeRetainSampledRootScaleAndExecution()
        {
            var root = Root; Time.value = .5f; yield return null;
            Assert.That(root.style.scale.value.value.x, Is.EqualTo(1.5f).Within(.001));
            Zoom.value = 75; yield return null;
            Assert.That(Surface.worldBound.width / 1920, Is.EqualTo(.75f).Within(.001));
            window.position = new Rect(30, 30, 1450, 920); yield return null; yield return null;
            Assert.That(Root, Is.SameAs(root)); Assert.That(Time.value, Is.EqualTo(.5f));
            Assert.That(root.Q("target").style.opacity.value, Is.EqualTo(.5f).Within(.001));
            Assert.That(root.style.scale.value.value.x, Is.EqualTo(1.5f).Within(.001));
            Assert.That(Zoom.value, Is.EqualTo(75).Within(.001));
            Click("preview-fit"); yield return null;
            Assert.That(root.style.scale.value.value.x, Is.EqualTo(1.5f).Within(.001));
            Time.value = .75f; yield return null;
            Assert.That(root.Q("target").style.opacity.value, Is.EqualTo(.75f).Within(.001));
            Width.value = 1280; yield return null; yield return null;
            Assert.That(Time.value, Is.Zero); Assert.That(root.style.scale.keyword, Is.EqualTo(StyleKeyword.Null));
            Assert.That(root.layout.width, Is.EqualTo(1280).Within(.01));
            Time.value = .25f; yield return null;
            Assert.That(root.Q("target").style.opacity.value, Is.EqualTo(.25f).Within(.001));
            AssertUnchangedAssets();
        }

        [UnityTest] public IEnumerator DelayedDimensionsAndZoomCommitThroughTypingAndBlur()
        {
            Width.Focus(); yield return null;
            var input = window.rootVisualElement.focusController.focusedElement as VisualElement;
            Key(input, 'a', KeyCode.A, EventModifiers.Control); Key(input, '8'); yield return null;
            Assert.That(Width.value, Is.EqualTo(1920)); Assert.That(Root.layout.width, Is.EqualTo(1920).Within(.01));
            Key(input, '0'); yield return null; Key(input, '0'); yield return null;
            Key(input, '\n', KeyCode.Return); yield return null; yield return null;
            Assert.That(Width.value, Is.EqualTo(800)); Assert.That(Root.layout.width, Is.EqualTo(800).Within(.01));
            Zoom.Focus(); yield return null;
            input = window.rootVisualElement.focusController.focusedElement as VisualElement;
            float before = Zoom.value;
            Key(input, 'a', KeyCode.A, EventModifiers.Control); Key(input, '5'); Key(input, '0'); yield return null;
            Key(input, '.'); yield return null;
            Assert.That(Zoom.text, Does.Contain("50.")); Assert.That(Zoom.value, Is.EqualTo(before));
            window.position = new Rect(30, 30, 1300, 900); yield return null; yield return null;
            Assert.That(Zoom.text, Is.EqualTo("50.")); Assert.That(Zoom.value, Is.EqualTo(before));
            Key(input, '2'); yield return null;
            window.SendEvent(Event.KeyboardEvent("backspace")); yield return null;
            Assert.That(Zoom.text, Is.EqualTo("50."));
            Key(input, '5'); yield return null; Height.Focus(); yield return null;
            Assert.That(Zoom.value, Is.EqualTo(50.5f).Within(.001));
            Assert.That(Surface.worldBound.width / 800, Is.EqualTo(.505f).Within(.001));
            Width.value = 0; Height.value = -1; Zoom.value = float.NaN; yield return null;
            Assert.That(Width.value, Is.EqualTo(800)); Assert.That(Height.value, Is.EqualTo(1080));
            Assert.That(Zoom.value, Is.EqualTo(50.5f).Within(.001));
            AssertUnchangedAssets();
        }

        [UnityTest] public IEnumerator SettingsSurviveReopenAndAssetSwitchWithoutSavingAssets()
        {
            Width.value = 900; Height.value = 1600; Zoom.value = 50; yield return null; yield return null;
            window.Close(); window = UITKTweenEditor.Open(asset); yield return null; yield return null;
            Assert.That(Width.value, Is.EqualTo(900)); Assert.That(Height.value, Is.EqualTo(1600)); Assert.That(Zoom.value, Is.EqualTo(50));
            var other = ScriptableObject.CreateInstance<UITKTweenAsset>(); other.AuthoringDocument = asset.AuthoringDocument;
            AssetDatabase.CreateAsset(other, Folder + "/Other.asset"); AssetDatabase.SaveAssets();
            try
            {
                UITKTweenEditor.Open(other); yield return null; yield return null;
                Assert.That(Width.value, Is.EqualTo(900)); Assert.That(Height.value, Is.EqualTo(1600));
                Assert.That(EditorUtility.IsDirty(other), Is.False);
                Click("preview-actual-size"); yield return null;
                Assert.That(Surface.worldBound.width, Is.EqualTo(900).Within(.01));
            }
            finally { UITKTweenEditor.Open(asset); AssetDatabase.DeleteAsset(Folder + "/Other.asset"); }
            AssertUnchangedAssets();
        }

        [UnityTest] public IEnumerator FitCanShowLargeCanvasWhileManualZoomKeepsItsBounds()
        {
            Width.value = 16384; Height.value = 16384; Click("preview-fit"); yield return null; yield return null;
            Assert.That(Zoom.value, Is.LessThan(10)); Assert.That(Zoom.value, Is.GreaterThan(0));
            var position = View.worldBound.center;
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 2, mousePosition = position })) View.SendEvent(down);
            yield return null;
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 2, mousePosition = position })) View.SendEvent(up);
            yield return null;
            Assert.That(Zoom.value, Is.EqualTo(10).Within(.001));
            Width.value = 16385; yield return null; Assert.That(Width.value, Is.EqualTo(16384));
            Zoom.value = 500; yield return null; Assert.That(Zoom.value, Is.EqualTo(400));
            AssertUnchangedAssets();
        }

        [UnityTest] public IEnumerator WheelAnchorsCanvasPointAndMiddleDragPansWithoutChangingLayout()
        {
            Zoom.value = 50; yield return null;
            Vector2 position = View.worldBound.center + new Vector2(30, 20);
            Vector2 logical = Surface.WorldToLocal(position);
            using (var wheel = WheelEvent.GetPooled(new Event { type = EventType.ScrollWheel, mousePosition = position, delta = new Vector2(0, -2) })) View.SendEvent(wheel);
            yield return null;
            Assert.That(Vector2.Distance(Surface.WorldToLocal(position), logical), Is.LessThan(.05f));
            var before = Surface.worldBound.position; var layout = Root.layout;
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 2, mousePosition = position })) View.SendEvent(down);
            yield return null;
            using (var move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseDrag, button = 2, mousePosition = position + new Vector2(40, 25) })) View.SendEvent(move);
            yield return null;
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 2, mousePosition = position + new Vector2(40, 25) })) View.SendEvent(up);
            yield return null;
            Assert.That(Surface.worldBound.x - before.x, Is.EqualTo(40).Within(.1));
            Assert.That(Surface.worldBound.y - before.y, Is.EqualTo(25).Within(.1));
            Assert.That(Root.layout, Is.EqualTo(layout)); AssertUnchangedAssets();
        }
    }
}
