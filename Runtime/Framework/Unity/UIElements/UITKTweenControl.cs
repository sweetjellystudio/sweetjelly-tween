using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>One execution of a captured trajectory. Sampling never settles Completion.</summary>
    public sealed class UITKTweenControl : IDisposable
    {
        private static readonly Dictionary<UITKWriterKey, UITKTweenControl> writers = new();
        private readonly UITKTweenPlayer owner;
        private readonly UITKTweenTimeline timeline;
        private readonly TaskCompletionSource<bool> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<CancellationTokenRegistration> registrations = new();
        private ITweenHandle clock;
        private Action completed, killed;
        public float Position { get; private set; }
        public float Duration => timeline.Duration;
        public float Progress => Duration == 0f ? 1f : Position / Duration;
        public bool IsActive { get; private set; } = true;
        public bool IsAutomatic => clock != null;
        public Task Completion => completion.Task;

        internal UITKTweenControl(UITKTweenPlayer owner, UITKTweenTimeline timeline, float position)
        {
            this.owner = owner; this.timeline = timeline; Position = position;
            CheckAvailable(timeline, null);
            foreach (var key in timeline.Properties.Keys) writers.Add(key, this);
            owner.Active = this;
        }

        internal static void CheckAvailable(UITKTweenTimeline timeline, UITKTweenControl allowed)
        {
            timeline.Validate();
            foreach (var key in timeline.Properties.Keys)
                if (writers.TryGetValue(key, out var other) && !ReferenceEquals(other, allowed))
                    throw new InvalidOperationException("Another UITK Player owns this target/property. Cancel it explicitly before replacing it.");
        }

        public void Sample(float time)
        {
            EnsureActive();
            time = TweenTimelineTiming.ClampTime(time, Duration);
            StopClock();
            try { timeline.Apply(time); Position = time; }
            catch { Dispose(); throw; }
        }

        public void SampleProgress(float progress)
        {
            if (float.IsNaN(progress) || float.IsInfinity(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
            Sample(Math.Max(0f, Math.Min(1f, progress)) * Duration);
        }

        public void Pause() { EnsureActive(); StopClock(); }

        public Task ResumeAsync(bool towardEnd = true, CancellationToken ct = default)
        {
            EnsureActive();
            if (ct.IsCancellationRequested) { Dispose(); return Task.FromCanceled(ct); }
            StopClock();
            try
            {
                float destination = towardEnd ? Duration : 0f;
                if (destination == Position) { timeline.Apply(destination); Finish(true); }
                else
                {
                    var tween = new Clock(this, Position, destination);
                    tween.SetDuration(Math.Abs(destination - Position)).SetUpdateMethod(owner.Lane).SetUnscaledMode(owner.Unscaled);
                    var handle = owner.Service.Play(tween);
                    clock = handle;
                    completed = () => { if (ReferenceEquals(clock, handle)) Finish(true); };
                    killed = () => { if (ReferenceEquals(clock, handle)) Finish(false); };
                    handle.Completed += completed; handle.Killed += killed;
                }
                if (IsActive && ct.CanBeCanceled)
                {
                    var registration = ct.Register(() => Dispose());
                    if (IsActive) registrations.Add(registration); else registration.Dispose();
                }
                return Completion;
            }
            catch { Dispose(); throw; }
        }

        public void Dispose() => Finish(false);
        private void EnsureActive()
        {
            if (!IsActive || !ReferenceEquals(owner.Active, this)) throw new InvalidOperationException("The control execution has ended.");
            timeline.Validate();
        }

        private void StopClock()
        {
            var previous = clock; clock = null;
            if (previous == null) return;
            previous.Completed -= completed; previous.Killed -= killed;
            completed = killed = null;
            previous.Kill();
        }

        private void Finish(bool success)
        {
            if (!IsActive) return;
            IsActive = false;
            if (ReferenceEquals(owner.Active, this)) owner.Active = null;
            foreach (var key in timeline.Properties.Keys)
                if (writers.TryGetValue(key, out var current) && ReferenceEquals(current, this)) writers.Remove(key);
            try { StopClock(); }
            finally
            {
                foreach (var registration in registrations) registration.Dispose();
                registrations.Clear();
                if (success) completion.TrySetResult(true); else completion.TrySetCanceled();
            }
        }

        private sealed class Clock : TweenCase
        {
            private readonly UITKTweenControl control;
            private readonly float from, to;
            internal Clock(UITKTweenControl control, float from, float to) { this.control = control; this.from = from; this.to = to; }
            public override bool Validate()
            {
                if (!control.IsActive) return false;
                try { control.timeline.Validate(); return true; }
                catch (InvalidOperationException) { control.Dispose(); return false; }
            }
            public override void Invoke(float deltaTime)
            {
                if (!control.IsActive) return;
                float time = from + (to - from) * State;
                control.timeline.Apply(time); control.Position = time;
            }
            public override void DefaultComplete()
            {
                if (!control.IsActive) return;
                control.timeline.Apply(to); control.Position = to;
            }
        }
    }
}
