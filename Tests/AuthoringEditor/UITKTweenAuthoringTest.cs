using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Editor;
using SWeetJelly.Tween.Framework.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SWeetJelly.Tween.Tests.Authoring
{
    public sealed class UITKTweenAuthoringTest
    {
        private const string Folder = "Assets/__UITKTweenAuthoringTests";
        private UITKTweenAsset asset;
        private UITKTweenEditor window;
        private string uxmlBefore, ussBefore;
        private bool ownsFixture;
        private readonly Dictionary<string, object> preferences = new();
        [UnitySetUp] public IEnumerator SetUp()
        {
            ownsFixture = false;
            foreach (string suffix in new[] { "Left", "Right", "Bottom" })
            {
                string key = "SWeetJelly.Tween.UITKEditor.Layout." + suffix;
                preferences[key] = EditorPrefs.HasKey(key) ? (object)EditorPrefs.GetFloat(key) : null;
                EditorPrefs.DeleteKey(key);
            }
            foreach (string suffix in new[] { "Width", "Height", "Zoom", "Fit" })
            {
                string key = "SWeetJelly.Tween.UITKPreview." + suffix;
                preferences[key] = !EditorPrefs.HasKey(key) ? null : suffix == "Zoom" ? (object)EditorPrefs.GetFloat(key) :
                    suffix == "Fit" ? (object)EditorPrefs.GetBool(key) : EditorPrefs.GetInt(key);
            }
            Assert.That(AssetDatabase.IsValidFolder(Folder), Is.False, "Preserve a pre-existing test folder; remove only this task's fixture.");
            AssetDatabase.CreateFolder("Assets", "__UITKTweenAuthoringTests");
            ownsFixture = true;
            uxmlBefore = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\"><ui:Style src=\"Document.uss\"/><ui:VisualElement name=\"root\"><ui:VisualElement name=\"title\"/><ui:VisualElement name=\"art\"/><ui:VisualElement name=\"target-with-a-very-long-designer-authored-identifier\"/></ui:VisualElement></ui:UXML>";
            ussBefore = "#root { width: 400px; height: 300px; } #title, #art { width: 80px; height: 60px; opacity: 0.7; background-color: #ce7d99; }";
            File.WriteAllText(Folder + "/Document.uxml", uxmlBefore); File.WriteAllText(Folder + "/Document.uss", ussBefore);
            AssetDatabase.Refresh();
            asset = ScriptableObject.CreateInstance<UITKTweenAsset>();
            asset.AuthoringDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Folder + "/Document.uxml");
            asset.Root.Children.Add(new UITKTweenNode { Label = "Title opacity", TargetName = "title", Duration = .6f, FromCurrent = false, From = Vector2.zero, To = Vector2.one });
            asset.Root.Children.Add(new UITKTweenNode { Label = "Art scale", TargetName = "art", Property = UITKTweenProperty.Scale, From = Vector2.one, To = Vector2.one * 2 });
            AssetDatabase.CreateAsset(asset, Folder + "/Tween.asset"); AssetDatabase.SaveAssets();
            window = UITKTweenEditor.Open(asset); window.position = new Rect(40, 40, 1200, 850); window.Focus();
            yield return null; yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (window != null) window.Close();
            foreach (var item in preferences)
            {
                if (item.Value == null) EditorPrefs.DeleteKey(item.Key);
                else if (item.Value is float number) EditorPrefs.SetFloat(item.Key, number);
                else if (item.Value is bool flag) EditorPrefs.SetBool(item.Key, flag);
                else EditorPrefs.SetInt(item.Key, (int)item.Value);
            }
            preferences.Clear();
            if (ownsFixture) AssetDatabase.DeleteAsset(Folder);
            ownsFixture = false; yield return null;
        }

        private static void Click(Button button)
        {
            Assert.That(button, Is.Not.Null);
            var position = button.worldBound.center;
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = position })) button.SendEvent(down);
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = position })) button.SendEvent(up);
        }
        private void SelectTitle() => Click(window.rootVisualElement.Q<ScrollView>("tween-nodes").Query<Button>().ToList().First(button => button.text.Contains("Title opacity")));
        private Button NodeButton(UITKTweenNode node) => window.rootVisualElement.Q<ScrollView>("tween-nodes").Query<Button>(className: "node-select").ToList().Single(button => ReferenceEquals(button.userData, node));
        private IEnumerator Select(UITKTweenNode node)
        {
            window.rootVisualElement.Q<ScrollView>("tween-nodes").ScrollTo(NodeButton(node));
            yield return null;
            Click(NodeButton(node)); yield return null;
            Assert.That(window.rootVisualElement.Q<TextField>("node-label").value, Is.EqualTo(node.Label), "The requested hierarchy row must receive the pointer input.");
        }
        private void Rename(string text) => window.rootVisualElement.Q<TextField>("node-label").value = text;
        private IEnumerator Drag(string splitName, Vector2 delta)
        {
            var split = window.rootVisualElement.Q<TwoPaneSplitView>(splitName);
            var handle = split.Query<VisualElement>(className: "unity-two-pane-split-view__dragline-anchor").ToList().Last();
            var start = handle.worldBound.center;
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = start })) handle.SendEvent(down);
            yield return null;
            using (var move = PointerMoveEvent.GetPooled(new Event { type = EventType.MouseDrag, button = 0, mousePosition = start + delta })) handle.SendEvent(move);
            yield return null;
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = start + delta })) handle.SendEvent(up);
            yield return null; yield return null;
        }
        private static void Key(VisualElement target, char character, KeyCode code = KeyCode.None, EventModifiers modifiers = EventModifiers.None)
        {
            using var evt = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, character = character, keyCode = code, modifiers = modifiers }); target.SendEvent(evt);
        }

        [UnityTest] public IEnumerator NumericTypingPreservesIntermediateDecimal_CommitsOnEnter_AndPersistsAcrossReopen()
        {
            SelectTitle(); yield return null;
            var field = window.rootVisualElement.Q<FloatField>("duration"); Assert.That(field.isDelayed, Is.True);
            field.Focus(); yield return null;
            var input = window.rootVisualElement.focusController.focusedElement as VisualElement;
            Assert.That(input, Is.Not.Null);
            Key(input, 'a', KeyCode.A, EventModifiers.Control); yield return null;
            Key(input, '0'); yield return null;
            Assert.That(asset.Root.Children[0].Duration, Is.EqualTo(.6f));
            Key(input, '.'); yield return null; Assert.That(field.text, Does.Contain("0."));
            Key(input, '3'); yield return null; Key(input, '5'); yield return null;
            Assert.That(asset.Root.Children[0].Duration, Is.EqualTo(.6f));
            Key(input, '\n', KeyCode.Return); yield return null;
            Assert.That(asset.Root.Children[0].Duration, Is.EqualTo(.35f).Within(.00001));
            Click(window.rootVisualElement.Q<Button>("save-tween")); yield return null;
            window.Close(); window = null; asset = null;
            var reopened = AssetDatabase.LoadAssetAtPath<UITKTweenAsset>(Folder + "/Tween.asset");
            window = UITKTweenEditor.Open(reopened); yield return null;
            Assert.That(reopened.Root.Children[0].Duration, Is.EqualTo(.35f).Within(.00001));
            Assert.That(File.ReadAllText(Folder + "/Document.uxml"), Is.EqualTo(uxmlBefore));
            Assert.That(File.ReadAllText(Folder + "/Document.uss"), Is.EqualTo(ussBefore));
        }

        [UnityTest] public IEnumerator NumericDeletionStaysEditable_AndFocusLossCommits()
        {
            SelectTitle(); yield return null;
            var field = window.rootVisualElement.Q<FloatField>("duration"); field.Focus(); yield return null;
            var input = window.rootVisualElement.focusController.focusedElement as VisualElement;
            Key(input, 'a', KeyCode.A, EventModifiers.Control); Key(input, '0'); yield return null;
            Key(input, '.'); yield return null; Key(input, '4'); yield return null;
            window.SendEvent(Event.KeyboardEvent("backspace")); yield return null;
            Assert.That(field.text, Is.EqualTo("0.")); Assert.That(asset.Root.Children[0].Duration, Is.EqualTo(.6f));
            Key(input, '7'); yield return null;
            window.rootVisualElement.Q<TextField>("node-label").Focus(); yield return null;
            Assert.That(asset.Root.Children[0].Duration, Is.EqualTo(.7f).Within(.00001));
        }

        [UnityTest] public IEnumerator ActualElementSelectionReorderAndCurveEditRoundTrip()
        {
            SelectTitle(); yield return null;
            var art = window.rootVisualElement.Q<ScrollView>("element-tree").Query<Button>().ToList().First(button => button.text.Contains("#art"));
            Click(art); yield return null; Assert.That(asset.Root.Children[0].TargetName, Is.EqualTo("art"));
            Click(window.rootVisualElement.Q<Button>("move-down")); yield return null;
            Assert.That(asset.Root.Children[1].Label, Is.EqualTo("Title opacity"));
            var curve = window.rootVisualElement.Q<CurveField>("curve");
            curve.value = AnimationCurve.EaseInOut(0, 0, 1, 1);
            var toggle = window.rootVisualElement.Q<Toggle>("use-curve"); toggle.value = true;
            Click(window.rootVisualElement.Q<Button>("save-tween")); yield return null;
            Assert.That(asset.Root.Children[1].UseCurve, Is.True); Assert.That(asset.Root.Children[1].Curve.length, Is.EqualTo(2));
            Assert.That(asset.Root.Children[1].Curve.Evaluate(.25f), Is.EqualTo(.15625f).Within(.0001));
            Assert.That(File.ReadAllText(Folder + "/Document.uxml"), Is.EqualTo(uxmlBefore));
        }

        [UnityTest] public IEnumerator PreviewScrubPauseResetAndCloseRestoreTheRealClone()
        {
            Click(window.rootVisualElement.Q<Button>("preview-play")); yield return null; yield return null;
            Click(window.rootVisualElement.Q<Button>("preview-pause")); yield return null;
            var target = window.rootVisualElement.Q("preview-canvas").Q("title");
            Assert.That(target.style.opacity.keyword, Is.EqualTo(StyleKeyword.Undefined));
            var slider = window.rootVisualElement.Q<Slider>("preview-time"); slider.value = .3f; yield return null;
            Assert.That(target.style.opacity.value, Is.EqualTo(.5f).Within(.001));
            Click(window.rootVisualElement.Q<Button>("preview-reset")); yield return null;
            Assert.That(target.style.opacity.keyword, Is.EqualTo(StyleKeyword.Null));
            Assert.That(target.resolvedStyle.opacity, Is.EqualTo(.7f).Within(.001));
            slider.value = .2f; yield return null;
            window.Close(); window = null; yield return null;
            Assert.That(target.style.opacity.keyword, Is.EqualTo(StyleKeyword.Null));
            Assert.That(File.ReadAllText(Folder + "/Document.uss"), Is.EqualTo(ussBefore));
        }

        [UnityTest] public IEnumerator PanelDraggingPreservesCanvasAndPose_AndRemembersOnlyEditorLayout()
        {
            Rename("A very long root tween label that must remain reachable through horizontal scrolling in the hierarchy");
            Click(window.rootVisualElement.Q<Button>("save-tween")); yield return null;
            string assetBefore = File.ReadAllText(Folder + "/Tween.asset");
            var root = window.rootVisualElement;
            root.Q<IntegerField>("preview-width").value = 1170; root.Q<IntegerField>("preview-height").value = 2532;
            yield return null; yield return null;
            Click(root.Q<Button>("preview-play")); yield return null; yield return null;
            Click(root.Q<Button>("preview-pause")); root.Q<Slider>("preview-time").value = .3f;
            root.Q<FloatField>("preview-zoom").value = 125; yield return null;
            var target = root.Q("preview-canvas").Q("title");
            float opacity = target.style.opacity.value;
            yield return Drag("elements-split", new Vector2(60, 0));
            yield return Drag("inspector-split", new Vector2(-40, 0));
            yield return Drag("hierarchy-split", new Vector2(0, -50));
            Assert.That(root.Q("elements-pane").layout.width, Is.EqualTo(280).Within(2));
            Assert.That(root.Q("inspector-pane").layout.width, Is.EqualTo(350).Within(2));
            Assert.That(root.Q("hierarchy-pane").layout.height, Is.EqualTo(290).Within(2));
            Assert.That(root.Q("preview-canvas").Q("title"), Is.SameAs(target));
            Assert.That(root.Q<IntegerField>("preview-width").value, Is.EqualTo(1170));
            Assert.That(root.Q<IntegerField>("preview-height").value, Is.EqualTo(2532));
            Assert.That(root.Q<FloatField>("preview-zoom").value, Is.EqualTo(125).Within(.001));
            Assert.That(root.Q<Slider>("preview-time").value, Is.EqualTo(.3f).Within(.001));
            Assert.That(target.style.opacity.value, Is.EqualTo(opacity).Within(.001));
            Assert.That(root.Q<ScrollView>("element-tree").horizontalScroller.highValue, Is.GreaterThan(0));
            window.position = new Rect(40, 40, 1000, 700); yield return null; yield return null;
            Assert.That(root.Q<ScrollView>("tween-nodes").horizontalScroller.highValue, Is.GreaterThan(0));
            Assert.That(root.Q<FloatField>("preview-zoom").value, Is.EqualTo(125).Within(.001));
            Click(root.Q<Button>("preview-fit")); yield return null;
            float fit = root.Q<FloatField>("preview-zoom").value;
            yield return Drag("hierarchy-split", new Vector2(0, 30));
            Assert.That(root.Q<FloatField>("preview-zoom").value, Is.Not.EqualTo(fit));
            Assert.That(root.Q<Slider>("preview-time").value, Is.EqualTo(.3f).Within(.001));
            float left = root.Q("elements-pane").layout.width, right = root.Q("inspector-pane").layout.width, bottom = root.Q("hierarchy-pane").layout.height;
            window.Close(); window = UITKTweenEditor.Open(asset); window.position = new Rect(40, 40, 1000, 700);
            yield return null; yield return null;
            root = window.rootVisualElement;
            Assert.That(root.Q("elements-pane").layout.width, Is.EqualTo(left).Within(2));
            Assert.That(root.Q("inspector-pane").layout.width, Is.EqualTo(right).Within(2));
            Assert.That(root.Q("hierarchy-pane").layout.height, Is.EqualTo(bottom).Within(2));
            Click(root.Q<Button>("reset-layout")); yield return null; yield return null;
            Assert.That(root.Q("elements-pane").layout.width, Is.EqualTo(220).Within(2));
            Assert.That(root.Q("inspector-pane").layout.width, Is.EqualTo(310).Within(2));
            Assert.That(root.Q("hierarchy-pane").layout.height, Is.EqualTo(240).Within(2));
            Assert.That(EditorUtility.IsDirty(asset), Is.False);
            Assert.That(File.ReadAllText(Folder + "/Tween.asset"), Is.EqualTo(assetBefore));
            Assert.That(File.ReadAllText(Folder + "/Document.uxml"), Is.EqualTo(uxmlBefore));
            Assert.That(File.ReadAllText(Folder + "/Document.uss"), Is.EqualTo(ussBefore));
        }

        [UnityTest] public IEnumerator GroupChildrenCanBeAuthoredSelectedCollapsedReorderedAndSaved()
        {
            Click(window.rootVisualElement.Q<Button>("add-group")); yield return null; Rename("Parallel");
            var group = asset.Root.Children.Last();
            Click(window.rootVisualElement.Q<Button>("add-child-property")); yield return null; Rename("First child");
            var first = group.Children[0];
            yield return Select(group);
            Click(window.rootVisualElement.Q<Button>("add-child-property")); yield return null; Rename("Second child");
            var second = group.Children[1];
            Click(window.rootVisualElement.Q<Button>("move-up")); yield return null;
            Assert.That(group.Children, Is.EqualTo(new[] { second, first }));
            yield return Select(group);
            var childButton = window.rootVisualElement.Q<ScrollView>("node-inspector").Query<Button>(className: "child-select").ToList().Single(b => ReferenceEquals(b.userData, first));
            Click(childButton); yield return null;
            Assert.That(window.rootVisualElement.Q<TextField>("node-label").value, Is.EqualTo("First child"));
            yield return Select(group);
            Click(window.rootVisualElement.Q<Button>("add-child-sequence")); yield return null; Rename("Nested");
            Click(window.rootVisualElement.Q<Button>("add-child-property")); yield return null; Rename("Nested leaf");
            var nested = group.Children[2];
            Assert.That(nested.Kind, Is.EqualTo(UITKTweenNodeKind.Sequence)); Assert.That(nested.Children.Single().Label, Is.EqualTo("Nested leaf"));
            yield return Select(group);
            var disclosure = window.rootVisualElement.Q<ScrollView>("tween-nodes").Query<Button>("toggle-children").ToList().Single(b => ReferenceEquals(b.userData, group));
            Click(disclosure); yield return null;
            Assert.That(NodeButton(first).parent.parent.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Click(disclosure); yield return null; Assert.That(NodeButton(first).parent.parent.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            yield return Select(nested.Children[0]);
            Undo.IncrementCurrentGroup(); Click(window.rootVisualElement.Q<Button>("remove-node")); yield return null;
            Assert.That(nested.Children.Count, Is.Zero);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo(); yield return null;
            group = asset.Root.Children.Single(n => n.Label == "Parallel");
            Assert.That(group.Children[2].Children.Single().Label, Is.EqualTo("Nested leaf"));
            Undo.PerformRedo(); yield return null;
            group = asset.Root.Children.Single(n => n.Label == "Parallel"); Assert.That(group.Children[2].Children, Is.Empty);
            Click(window.rootVisualElement.Q<Button>("save-tween")); yield return null;
            window.Close(); window = null; Resources.UnloadAsset(asset);
            asset = AssetDatabase.LoadAssetAtPath<UITKTweenAsset>(Folder + "/Tween.asset"); window = UITKTweenEditor.Open(asset); yield return null;
            group = asset.Root.Children.Single(n => n.Label == "Parallel");
            Assert.That(group.Children.Select(n => n.Label), Is.EqualTo(new[] { "Second child", "First child", "Nested" }));
            Assert.That(group.Children[2].Kind, Is.EqualTo(UITKTweenNodeKind.Sequence));
        }

        [UnityTest] public IEnumerator MoveToReparentsTheWholeSubtree_RejectsCycles_AndSupportsUndoRedo()
        {
            Click(window.rootVisualElement.Q<Button>("add-group")); yield return null; Rename("Outer");
            var outer = asset.Root.Children.Last();
            Click(window.rootVisualElement.Q<Button>("add-child-sequence")); yield return null; Rename("Nested");
            var nested = outer.Children.Single();
            Click(window.rootVisualElement.Q<Button>("add-child-property")); yield return null; Rename("Nested leaf");
            yield return Select(outer);
            var field = window.rootVisualElement.Q<DropdownField>("move-to-parent");
            Assert.That(field.enabledSelf, Is.False); Assert.That(field.choices.Count, Is.EqualTo(1), "Self, descendants and the current parent cannot be destinations.");
            yield return Select(nested);
            field = window.rootVisualElement.Q<DropdownField>("move-to-parent");
            field.index = field.choices.FindIndex(path => path.StartsWith(asset.Root.Label + " / Sequence"));
            Assert.That(field.index, Is.GreaterThan(0));
            yield return null;
            var moveButton = window.rootVisualElement.Q<Button>("move-to-apply");
            Assert.That(moveButton.enabledInHierarchy, Is.True);
            Assert.That(moveButton.worldBound.xMax, Is.LessThanOrEqualTo(window.rootVisualElement.Q<ScrollView>("node-inspector").contentViewport.worldBound.xMax + 1), "Move must remain visible beside a long parent path.");
            Undo.IncrementCurrentGroup(); Click(window.rootVisualElement.Q<Button>("move-to-apply")); yield return null;
            Assert.That(outer.Children, Is.Empty); Assert.That(asset.Root.Children.Last(), Is.SameAs(nested));
            Assert.That(nested.Children.Single().Label, Is.EqualTo("Nested leaf"));
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo(); yield return null;
            Assert.That(asset.Root.Children.Single(n => n.Label == "Outer").Children.Single().Label, Is.EqualTo("Nested"));
            Undo.PerformRedo(); yield return null;
            Assert.That(asset.Root.Children.Last().Label, Is.EqualTo("Nested"));
            Click(window.rootVisualElement.Q<Button>("save-tween")); yield return null;
            window.Close(); window = null; Resources.UnloadAsset(asset);
            asset = AssetDatabase.LoadAssetAtPath<UITKTweenAsset>(Folder + "/Tween.asset"); window = UITKTweenEditor.Open(asset); yield return null;
            Assert.That(asset.Root.Children.Single(n => n.Label == "Outer").Children, Is.Empty);
            Assert.That(asset.Root.Children.Last().Children.Single().Label, Is.EqualTo("Nested leaf"));
        }

        [Test] public void UnityAssetSerializationRetainsNestedNodesCurveAndClock()
        {
            var leaf = asset.Root.Children[0]; leaf.UseCurve = true; leaf.Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            leaf.Delay = .125f; leaf.Repetitions = 4; leaf.Loop = UITKTweenLoop.Increment;
            asset.UpdateMethod = UpdateMethod.LateUpdate; asset.UnscaledTime = true;
            asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Group, Label = "Nested", Children = new() { asset.Root } };
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
            Resources.UnloadAsset(asset); asset = AssetDatabase.LoadAssetAtPath<UITKTweenAsset>(Folder + "/Tween.asset");
            var restored = asset.Root.Children[0].Children[0];
            Assert.That(asset.AuthoringDocument, Is.Not.Null); Assert.That(restored.Delay, Is.EqualTo(.125f));
            Assert.That(restored.Repetitions, Is.EqualTo(4)); Assert.That(restored.Loop, Is.EqualTo(UITKTweenLoop.Increment));
            Assert.That(restored.Curve.Evaluate(.25f), Is.EqualTo(.15625f).Within(.0001));
            Assert.That(asset.UpdateMethod, Is.EqualTo(UpdateMethod.LateUpdate)); Assert.That(asset.UnscaledTime, Is.True);
        }
    }
}
