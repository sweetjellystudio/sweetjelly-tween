using System;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>Exclusive control of one timeline execution. All calls and cancellation use Unity's main thread.</summary>
    public sealed class TweenPlayerControl : IDisposable
    {
        private static readonly System.Collections.Generic.Dictionary<TimelineKey, TweenPlayerControl> writers = new();
        private static readonly System.Collections.Generic.Dictionary<TimelineKey, System.Collections.Generic.HashSet<TweenPlayerBehaviour>> automaticWriters = new();
        private readonly TweenPlayerTimeline timeline;
        private readonly TweenPlayerBehaviour.PlaybackRun run;
        private ITweenHandle clock;
        private bool active = true;
        public float Duration => timeline.Duration;
        public float Position { get; private set; }
        public float Progress => Duration == 0f ? 1f : Position / Duration;
        public bool IsActive => active;
        public bool IsAutomatic => clock != null && clock.IsActive;
        public Task Completion { get; }

        internal TweenPlayerControl(TweenPlayerTimeline timeline, float position)
        {
            this.timeline = timeline;
            Position = position;
            run = timeline.Owner.BeginTimelineControl();
            foreach (var player in timeline.Build.Members) player.ManualControl = this;
            foreach (var key in timeline.Build.Properties.Keys) writers.Add(key, this);
            timeline.ActiveControl = this;
            Completion = run.Wait(default);
        }

        public void Sample(float time)
        {
            EnsureActive();
            time = TweenTimelineTiming.ClampTime(time, Duration);
            StopClock();
            try { timeline.Apply(time); Position = time; }
            catch { try { Finish(false); } catch { } throw; }
        }

        public void SampleProgress(float progress)
        {
            if (float.IsNaN(progress) || float.IsInfinity(progress)) throw new ArgumentOutOfRangeException(nameof(progress));
            Sample(Math.Max(0f, Math.Min(1f, progress)) * Duration);
        }

        /// <summary>Continue along the captured path to its end or beginning, without replaying child events.</summary>
        public Task ResumeAsync(bool towardEnd = true, CancellationToken ct = default)
        {
            EnsureActive();
            if (ct.IsCancellationRequested) { Dispose(); return Task.FromCanceled(ct); }
            StopClock();
            float destination = towardEnd ? Duration : 0f;
            try
            {
                timeline.ValidateTargets();
                if (destination == Position) { timeline.Apply(destination); Finish(true); }
                else
                {
                    if (timeline.Build.Service == null) throw new InvalidOperationException("No tween service is available.");
                    var tween = new TimelineClock(this, Position, destination);
                    tween.SetDuration(Math.Abs(destination - Position)).SetUpdateMethod(timeline.Build.Lane)
                        .SetUnscaledMode(timeline.Build.Unscaled);
                    var handle = timeline.Build.Service.Play(tween);
                    clock = handle;
                    handle.Completed += () => { if (ReferenceEquals(clock, handle)) { clock = null; Finish(true); } };
                    handle.Killed += () => { if (ReferenceEquals(clock, handle)) { clock = null; Finish(false); } };
                }
                return run.Wait(ct);
            }
            catch { try { Finish(false); } catch { } throw; }
        }

        public void Dispose() { if (active) Finish(false); }

        internal void ValidateLifetime()
        {
            if (!active) return;
            try { timeline.ValidateTargets(); }
            catch (InvalidOperationException) { Finish(false); }
        }
        internal bool IsOwner(TweenPlayerBehaviour player) => ReferenceEquals(timeline.Owner, player);

        internal static void CheckAvailable(TweenPlayerTimeline timeline)
        {
            foreach (var key in timeline.Build.Properties.Keys)
            {
                if (writers.ContainsKey(key)) throw new InvalidOperationException("Another control owns this target property.");
                if (automaticWriters.TryGetValue(key, out var players))
                    foreach (var player in players)
                        if (player != null && !timeline.Build.Members.Contains(player))
                            throw new InvalidOperationException("Another automatic player owns this target property.");
            }
        }

        internal static void RegisterAutomatic(TimelineKey key, TweenPlayerBehaviour player)
        {
            if (!automaticWriters.TryGetValue(key, out var players))
                automaticWriters.Add(key, players = new System.Collections.Generic.HashSet<TweenPlayerBehaviour>());
            players.Add(player);
        }
        internal static void UnregisterAutomatic(TimelineKey key, TweenPlayerBehaviour player)
        {
            if (!automaticWriters.TryGetValue(key, out var players)) return;
            players.Remove(player);
            if (players.Count == 0) automaticWriters.Remove(key);
        }

        internal static bool StopWriter(TimelineKey key)
        {
            if (!writers.TryGetValue(key, out var writer)) return false;
            writer.Dispose();
            return true;
        }

        private void EnsureActive()
        {
            if (!active || !ReferenceEquals(timeline.Owner.ManualControl, this))
                throw new InvalidOperationException("This timeline execution has ended.");
        }

        private void StopClock()
        {
            var previous = clock; clock = null; previous?.Kill();
        }

        private void Finish(bool completed)
        {
            if (!active) return;
            active = false;
            if (ReferenceEquals(timeline.ActiveControl, this)) timeline.ActiveControl = null;
            StopClock();
            foreach (var key in timeline.Build.Properties.Keys)
                if (writers.TryGetValue(key, out var writer) && ReferenceEquals(writer, this)) writers.Remove(key);
            foreach (var player in timeline.Build.Members)
                if (player != null && ReferenceEquals(player.ManualControl, this)) player.ManualControl = null;
            timeline.Owner.EndTimelineControl(run, completed);
        }

        private sealed class TimelineClock : TweenCase
        {
            private readonly TweenPlayerControl control;
            private readonly float from, to;
            internal TimelineClock(TweenPlayerControl control, float from, float to)
            { this.control = control; this.from = from; this.to = to; }
            public override bool Validate()
            {
                if (!control.active) return false;
                try { control.timeline.ValidateTargets(); return true; }
                catch (InvalidOperationException) { return false; }
            }
            public override void Invoke(float deltaTime)
            {
                if (!control.active) return;
                float time = from + (to - from) * State;
                control.timeline.Apply(time);
                control.Position = time;
            }
            public override void DefaultComplete()
            {
                if (!control.active) return;
                control.timeline.Apply(to);
                control.Position = to;
            }
        }
    }
}
