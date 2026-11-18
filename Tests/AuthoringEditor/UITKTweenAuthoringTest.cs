using System;
using System.Collections;
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
        [UnitySetUp] public IEnumerator SetUp()
        {
            ownsFixture = false;
            Assert.That(AssetDatabase.IsValidFolder(Folder), Is.False, "Preserve a pre-existing test folder; remove only this task's fixture.");
            AssetDatabase.CreateFolder("Assets", "__UITKTweenAuthoringTests");
            ownsFixture = true;
            uxmlBefore = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\"><ui:Style src=\"Document.uss\"/><ui:VisualElement name=\"root\"><ui:VisualElement name=\"title\"/><ui:VisualElement name=\"art\"/></ui:VisualElement></ui:UXML>";
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
