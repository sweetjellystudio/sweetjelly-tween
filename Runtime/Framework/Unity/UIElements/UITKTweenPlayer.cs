using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>A consumer-owned binding. Dispose it when the consumer ends; restore styles explicitly if needed.</summary>
    public sealed class UITKTweenPlayer : IDisposable
    {
        private readonly UITKTweenAsset asset;
        internal readonly ITweenService Service;
        internal readonly UpdateMethod Lane;
        internal readonly bool Unscaled;
        internal VisualElement Root { get; private set; }
        private UITKTweenTimeline prepared;
        internal UITKTweenControl Active;
        private int generation;
        private bool disposed, bindingLost;
        private readonly HashSet<VisualElement> targets = new();
        public bool IsPlaying => Active != null;
        public float Position => Active?.Position ?? 0f;

        internal UITKTweenPlayer(UITKTweenAsset asset, VisualElement root, ITweenService service, UpdateMethod lane, bool unscaled)
        {
            this.asset = asset != null ? asset : throw new ArgumentNullException(nameof(asset));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            if (!Enum.IsDefined(typeof(UpdateMethod), lane)) throw new ArgumentOutOfRangeException(nameof(lane));
            Lane = lane; Unscaled = unscaled;
            Rebind(root);
        }

        public void Rebind(VisualElement root)
        {
            if (disposed) throw new ObjectDisposedException(nameof(UITKTweenPlayer));
            if (root == null) throw new ArgumentNullException(nameof(root));
            Kill(); Unsubscribe();
            generation++; prepared = null; bindingLost = false; Root = root;
            Subscribe(root);
        }

        /// <summary>Idle preparation captures again; during playback returns that execution's existing trajectory.</summary>
        public UITKTweenTimeline PrepareTimeline()
        {
            RequireReady();
            if (Active != null) return prepared;
            var candidate = new UITKTweenTimeline(this, asset.Root, generation + 1);
            Unsubscribe(); generation++; prepared = candidate;
            Subscribe(Root);
            foreach (var property in candidate.Properties.Values) Subscribe(property.Element);
            candidate.Validate();
            return candidate;
        }

        public Task PlayAsync(CancellationToken ct = default) => PlayAsync(true, ct);
        public Task PlayAsync(bool forward, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested) return Task.FromCanceled(ct);
            var timeline = GetPrepared();
            UITKTweenControl.CheckAvailable(timeline, Active);
            Kill();
            var control = new UITKTweenControl(this, timeline, forward ? 0f : timeline.Duration);
            try { control.Sample(control.Position); return control.ResumeAsync(forward, ct); }
            catch { control.Dispose(); throw; }
        }

        public void Kill() => Active?.Dispose();
        public void ApplyStart() => ApplyEndpoint(false);
        public void ApplyEnd() => ApplyEndpoint(true);
        public void Restore() => Restore(GetPrepared());

        private void ApplyEndpoint(bool end)
        {
            var timeline = GetPrepared();
            UITKTweenControl.CheckAvailable(timeline, Active);
            Kill(); timeline.Apply(end ? timeline.Duration : 0f);
        }

        internal void Restore(UITKTweenTimeline timeline)
        {
            timeline.Validate();
            if (!IsCurrent(timeline)) throw new InvalidOperationException("The timeline was superseded by a new preparation.");
            UITKTweenControl.CheckAvailable(timeline, Active);
            Kill(); timeline.RestoreValues();
        }

        internal UITKTweenControl TakeControl(UITKTweenTimeline timeline)
        {
            timeline.Validate();
            if (!IsCurrent(timeline)) throw new InvalidOperationException("The timeline was superseded by a new preparation.");
            UITKTweenControl.CheckAvailable(timeline, Active);
            float position = Active?.Position ?? 0f;
            Kill();
            return new UITKTweenControl(this, timeline, position);
        }

        internal bool IsCurrent(UITKTweenTimeline timeline) => ReferenceEquals(prepared, timeline);
        private UITKTweenTimeline GetPrepared()
        {
            RequireReady();
            if (prepared == null) PrepareTimeline();
            prepared.Validate();
            return prepared;
        }

        private void RequireReady()
        {
            if (disposed) throw new ObjectDisposedException(nameof(UITKTweenPlayer));
            if (bindingLost) throw new InvalidOperationException("The binding detached. Explicitly Rebind and prepare again.");
            if (Root.panel == null || float.IsNaN(Root.layout.width) || float.IsNaN(Root.layout.height) ||
                float.IsInfinity(Root.layout.width) || float.IsInfinity(Root.layout.height))
                throw new InvalidOperationException("Attach the root and allow style/layout resolution before preparing.");
        }

        internal void ValidateBinding(int expectedGeneration)
        {
            RequireReady();
            if (generation != expectedGeneration) throw new InvalidOperationException("This prepared timeline's binding or capture is no longer current.");
        }

        private void Subscribe(VisualElement element)
        {
            if (targets.Add(element)) element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }
        private void Unsubscribe()
        {
            foreach (var element in targets) element.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            targets.Clear();
        }
        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (evt.target is not VisualElement target || !targets.Contains(target)) return;
            bindingLost = true; generation++; prepared = null;
            try { Kill(); } finally { Unsubscribe(); }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true; generation++; prepared = null;
            try { Kill(); } finally { Unsubscribe(); }
        }
    }
}
