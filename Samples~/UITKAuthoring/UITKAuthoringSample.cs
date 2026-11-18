using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.UITKAuthoring.Sample
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UITKAuthoringSample : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset featureDocument;
        [SerializeField] private UITKTweenAsset showing;
        [SerializeField] private UITKTweenAsset hiding;
        [SerializeField] private UITKTweenAsset focusing;
        private TweenService service;
        private FeatureExample a, b;
        private Label status;
        public FeatureExample A => a;
        public FeatureExample B => b;

        private void OnEnable() => StartCoroutine(CreateCopies());
        private IEnumerator CreateCopies()
        {
            yield return null;
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.AddToClassList("sample-panel");
            root.style.unityFontDefinition = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            status = root.Q<Label>("status");
            if (featureDocument == null || showing == null || hiding == null || focusing == null)
            { if (status != null) status.text = "Assign the feature UXML and Show / Hide / Focus assets."; yield break; }
            var copies = root.Q("copies"); copies.Clear();
            var left = AddCopy(copies, "A"); var right = AddCopy(copies, "B");
            service = new TweenService();
            yield return null; // Resolve the real tree's USS/layout before capturing.
            a = new FeatureExample(showing, hiding, focusing, left, service);
            b = new FeatureExample(showing, hiding, focusing, right, service);
            AddControls(left.parent, "A", a); AddControls(right.parent, "B", b);
            Debug.Log("UITK authoring sample: two independent roots prepared.");
        }

        private VisualElement AddCopy(VisualElement copies, string label)
        {
            var column = new VisualElement { name = "copy" + label }; column.AddToClassList("copy"); copies.Add(column);
            var template = featureDocument.Instantiate(); template.AddToClassList("copy-template"); column.Add(template);
            return template.childCount == 1 ? template[0] : template;
        }
        private void AddControls(VisualElement template, string label, FeatureExample feature)
        {
            var controls = new VisualElement(); controls.AddToClassList("copy-controls"); template.parent.Add(controls);
            controls.Add(new Button(() => Observe(feature.ShowAsync(), label + " Show")) { text = label + " Show" });
            controls.Add(new Button(() => Observe(feature.HideAsync(), label + " Hide")) { text = "Hide" });
            controls.Add(new Button(() => Observe(feature.FocusAsync(), label + " Focus")) { text = "Focus" });
            controls.Add(new Button(() => feature.ApplyShown()) { text = "End" });
            controls.Add(new Button(feature.Restore) { text = "Restore" });
            var slider = new Slider("Show", 0, 1);
            slider.RegisterValueChangedCallback(evt => feature.SampleShow(evt.newValue)); template.parent.Add(slider);
        }
        private async void Observe(Task task, string label)
        {
            try { await task; if (status != null) status.text = label + " completed"; }
            catch (OperationCanceledException) { if (status != null) status.text = label + " canceled"; }
            catch (Exception error) { Debug.LogException(error); }
        }
        private void Update() => service?.Tick(UpdateMethod.Update, Time.deltaTime, Time.unscaledDeltaTime, true);
        private void FixedUpdate() => service?.Tick(UpdateMethod.FixedUpdate, Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime, true);
        private void LateUpdate() => service?.Tick(UpdateMethod.LateUpdate, Time.deltaTime, Time.unscaledDeltaTime, true);
        private void OnDisable()
        {
            StopAllCoroutines(); a?.Dispose(); b?.Dispose(); a = b = null; service?.Dispose(); service = null;
        }

        /// <summary>A sample consumer, not a Tween package contract or required game superclass.</summary>
        public sealed class FeatureExample : IDisposable
        {
            private readonly UITKTweenPlayer show, hide, focus;
            private readonly UITKTweenTimeline showTimeline;
            private UITKTweenControl control;
            public FeatureExample(UITKTweenAsset showing, UITKTweenAsset hiding, UITKTweenAsset focusing, VisualElement root, ITweenService service)
            {
                show = showing.Bind(root, service, unscaledTime: true);
                hide = hiding.Bind(root, service, unscaledTime: true);
                focus = focusing.Bind(root, service, unscaledTime: true);
                showTimeline = show.PrepareTimeline(); hide.PrepareTimeline(); focus.PrepareTimeline();
            }
            public Task ShowAsync(CancellationToken ct = default) { Cancel(); return show.PlayAsync(ct); }
            public Task HideAsync(CancellationToken ct = default) { Cancel(); return hide.PlayAsync(ct); }
            public Task FocusAsync(CancellationToken ct = default) { Cancel(); return focus.PlayAsync(ct); }
            public void SampleShow(float progress)
            {
                if (control == null || !control.IsActive) { Cancel(); control = showTimeline.TakeControl(); }
                control.SampleProgress(progress);
            }
            public void ApplyShown() { Cancel(); show.ApplyEnd(); }
            public void Restore() { Cancel(); show.Restore(); hide.Restore(); focus.Restore(); }
            private void Cancel() { control?.Dispose(); control = null; show.Kill(); hide.Kill(); focus.Kill(); }
            public void Dispose() { Cancel(); show.Dispose(); hide.Dispose(); focus.Dispose(); }
        }
    }
}
