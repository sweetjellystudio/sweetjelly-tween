using System;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract partial class TweenPlayerBehaviour
    {
        internal TweenPlayerControl ManualControl;
        private TweenPlayerTimeline recordedTimeline;
        internal PlaybackRun TimelineRun => CurrentPlayback;
        internal bool HasManualControl => ManualControl != null && ManualControl.IsActive;

        private void Update()
        {
            if (ManualControl != null && ManualControl.IsOwner(this)) ManualControl.ValidateLifetime();
        }

        /// <summary>Checks the entire graph without starting playback, stopping work or writing values.</summary>
        public bool CanPrepareTimeline(out string reason, bool? forward = null)
        {
            try { PrepareTimeline(forward); reason = null; return true; }
            catch (NotSupportedException error) { reason = error.Message; return false; }
            catch (InvalidOperationException error) { reason = error.Message; return false; }
            catch (ArgumentException error) { reason = error.Message; return false; }
        }

        /// <summary>Capture a reusable trajectory, or return the snapshot of the current supported command.</summary>
        public TweenPlayerTimeline PrepareTimeline(bool? forward = null)
        {
            if (HasManualControl) throw new InvalidOperationException("Use the prepared timeline that owns this control.");
            if (CurrentPlayback != null)
            {
                if (recordedTimeline == null || !ReferenceEquals(recordedTimeline.RecordedRun, CurrentPlayback))
                    throw new NotSupportedException("The active command does not have a supported timeline snapshot.");
                if (forward.HasValue) throw new InvalidOperationException("An active command already has a captured direction; omit forward.");
                recordedTimeline.ValidateTargets();
                return recordedTimeline;
            }
            return new TweenPlayerTimeline(this, forward);
        }

        internal virtual TimelineNode BuildTimeline(TimelineBuild build, bool? forward) =>
            throw new NotSupportedException($"{GetType().Name} does not support timeline sampling.");
        internal virtual bool OwnsTimelineChild(TweenPlayerBehaviour child) => false;

        internal void RecordTimeline(bool? forward)
        {
            recordedTimeline = null;
            // Recording is optional for legacy playback; unsupported graphs keep their original behavior.
            try
            {
                var snapshot = new TweenPlayerTimeline(this, forward);
                snapshot.RecordedRun = CurrentPlayback;
                recordedTimeline = snapshot;
            }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }
        }

        internal PlaybackRun BeginTimelineControl() => BeginPlayback();
        internal void EndTimelineControl(PlaybackRun run, bool completed)
        {
            NotifyAllTerminals(
                () => { if (completed) CompletePlayback(run); else CancelPlayback(run); },
                () => NotifyTimelineTerminal(completed));
        }
        internal virtual void NotifyTimelineTerminal(bool completed)
        { if (completed) NotifyCompleted(); else NotifyKilled(); }

        internal bool StopManualControl()
        {
            var control = ManualControl;
            if (control == null) return false;
            control.Dispose();
            return true;
        }
    }

    public abstract partial class TweenPlayerBehaviour<C, T>
    {
        private TweenCase timelineCase;
        private int timelineCommandVersion;
        private T timelineInitialStart, timelineInitialEnd;
        private readonly System.Collections.Generic.List<float> timelineCycleSpans = new();
        private TimelineKey? automaticTimelineKey;

        protected override void OnDestroy()
        {
            UnregisterTimelineWriter();
            base.OnDestroy();
        }

        private void RegisterTimelineWriter()
        {
            string property = GetType() == typeof(CanvasGroupFadeTweenPlayer) ? "alpha" :
                GetType() == typeof(RectTransformAnchoredPositionTweenPlayer) ? "anchoredPosition" :
                GetType() == typeof(TransformScaleTweenPlayer) ? "localScale" : null;
            if (property == null || TargetComponent == null) return;
            automaticTimelineKey = new TimelineKey(TargetComponent, property);
            TweenPlayerControl.RegisterAutomatic(automaticTimelineKey.Value, this);
        }
        private void UnregisterTimelineWriter()
        {
            if (automaticTimelineKey.HasValue) TweenPlayerControl.UnregisterAutomatic(automaticTimelineKey.Value, this);
            automaticTimelineKey = null;
        }

        private bool StopTimelineWriter()
        {
            if (StopManualControl()) return true;
            string property = GetType() == typeof(CanvasGroupFadeTweenPlayer) ? "alpha" :
                GetType() == typeof(RectTransformAnchoredPositionTweenPlayer) ? "anchoredPosition" :
                GetType() == typeof(TransformScaleTweenPlayer) ? "localScale" : null;
            return property != null && TargetComponent != null && TweenPlayerControl.StopWriter(new TimelineKey(TargetComponent, property));
        }

        internal override TimelineNode BuildTimeline(TimelineBuild build, bool? forward)
        {
            Type type = GetType();
            string property;
            if (type == typeof(CanvasGroupFadeTweenPlayer)) property = "alpha";
            else if (type == typeof(RectTransformAnchoredPositionTweenPlayer)) property = "anchoredPosition";
            else if (type == typeof(TransformScaleTweenPlayer)) property = "localScale";
            else throw new NotSupportedException($"{type.Name} does not support timeline sampling.");
            var target = TargetComponent;
            if (target == null) throw new NotSupportedException("The player target is missing.");
            build.Clock(tweenService, updateMethod, unscaledTime);
            if (loopAmount <= 0) throw new NotSupportedException("Infinite loops cannot be sampled on a finite timeline.");
            if (!Enum.IsDefined(typeof(LoopType), loopType)) throw new NotSupportedException("Unknown loop mode.");
            var key = new TimelineKey(target, property);
            if (!build.Properties.ContainsKey(key))
            {
                T origin = CurrentValue;
                build.Properties.Add(key, new TimelineProperty
                {
                    Origin = origin,
                    Write = value => CurrentValue = (T)value,
                    // Normalized state and elapsed time accumulate with different float rounding.
                    Matches = value => Distance(CurrentValue, (T)value) <= .0001f + .00001f * Distance(CurrentValue, default),
                    Valid = () => target != null && TargetComponent == target
                });
                build.Values.Add(key, origin);
            }
            T input = (T)build.Values[key];
            bool direction = forward ?? playOnEnableDirection == PlaybackDirection.Forward;
            bool noOrigin = !direction && endpointMode == EndpointMode.CaptureOnForward && !hasCapturedOrigin;
            T from = endpointMode == EndpointMode.ConfiguredEndpoints ? (direction ? backwardValue : forwardValue) : input;
            T to = direction ? forwardValue : (endpointMode == EndpointMode.ConfiguredEndpoints ? backwardValue : capturedOrigin);
            if (noOrigin) from = to = input;
            float seconds = TweenTiming.Duration(duration), wait = TweenTiming.Delay(delay);
            int count = loopAmount;
            int initialVersion = timelineCommandVersion;
            bool isRoot = ReferenceEquals(build.RootOwner, this);
            var spans = Array.Empty<float>();
            LoopType loops = loopType;
            IEasingFunction ease;
            if (hasCustomCurve && customCurve != null)
            {
                var curve = new AnimationCurve(customCurve.keys) { preWrapMode = customCurve.preWrapMode, postWrapMode = customCurve.postWrapMode };
                ease = new AnimationCurveEasingFunction(curve);
            }
            else ease = EasingFunctions.Get(easing);
            T Pose(float time)
            {
                if (noOrigin) return input;
                TweenTimelineTiming.Locate(time, seconds, wait, spans, count, out int cycle, out float progress);
                T a = from, b = to;
                if (loops == LoopType.Yoyo && (cycle & 1) != 0) { a = to; b = from; }
                else if (loops == LoopType.Increment) { a = Mix(from, to, cycle); b = Mix(from, to, cycle + 1f); }
                // During delay ordinary players hold the start, even for curves whose value at zero is nonzero.
                T result = time < wait ? from : progress >= 1f ? b : Mix(a, b, ease.Evaluate(progress));
                if (type == typeof(CanvasGroupFadeTweenPlayer)) result = (T)(object)Mathf.Clamp01((float)(object)result);
                return result;
            }
            var node = new TimelineNode { Player = this, Duration = noOrigin ? 0f : TweenTimelineTiming.Length(seconds, wait, count) };
            node.Keys.Add(key);
            node.Evaluate = (time, values) => values[key] = Pose(time);
            node.Position = () =>
            {
                if (timelineCase == null || CurrentPlayback == null) return node.Duration;
                float elapsed = timelineCase.ObservedElapsed;
                foreach (float span in timelineCycleSpans) elapsed += span;
                return TweenTimelineTiming.ClampTime(elapsed, node.Duration);
            };
            node.Refresh = () =>
            {
                if (noOrigin || timelineCase == null || (!isRoot && timelineCommandVersion <= initialVersion)) return;
                from = timelineInitialStart; to = timelineInitialEnd;
                int observed = Math.Min(count, timelineCycleSpans.Count + 1);
                spans = new float[observed];
                for (int i = 0; i < observed; i++)
                {
                    float span = seconds + (i == 0 ? wait : 0f);
                    if (i < timelineCycleSpans.Count) span = Math.Max(span, timelineCycleSpans[i]);
                    else if (i == timelineCycleSpans.Count) span = Math.Max(span, timelineCase.ObservedElapsed);
                    spans[i] = span;
                }
                node.Duration = TweenTimelineTiming.Length(seconds, wait, count, spans);
            };
            build.Values[key] = Pose(node.Duration);
            return node;
        }

        private static T Mix(T a, T b, float progress)
        {
            if (typeof(T) == typeof(float)) return (T)(object)Mathf.LerpUnclamped((float)(object)a, (float)(object)b, progress);
            if (typeof(T) == typeof(Vector2)) return (T)(object)Vector2.LerpUnclamped((Vector2)(object)a, (Vector2)(object)b, progress);
            if (typeof(T) == typeof(Vector3)) return (T)(object)Vector3.LerpUnclamped((Vector3)(object)a, (Vector3)(object)b, progress);
            throw new NotSupportedException("Unsupported sampled value type.");
        }
        private static float Distance(T a, T b)
        {
            if (typeof(T) == typeof(float)) return Mathf.Abs((float)(object)a - (float)(object)b);
            if (typeof(T) == typeof(Vector2)) return Vector2.Distance((Vector2)(object)a, (Vector2)(object)b);
            return Vector3.Distance((Vector3)(object)a, (Vector3)(object)b);
        }

        internal override void NotifyTimelineTerminal(bool success)
        {
            if (success) NotifyAllTerminals(NotifyCompleted, () => completed?.Invoke());
            else NotifyAllTerminals(NotifyKilled, () => killed?.Invoke());
        }
    }
}
