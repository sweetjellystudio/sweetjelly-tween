using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>A reusable captured trajectory. Preparing it never writes target values or starts children.</summary>
    public sealed class TweenPlayerTimeline
    {
        internal readonly TimelineNode Root;
        internal readonly TimelineBuild Build;
        internal readonly TweenPlayerBehaviour Owner;
        internal TweenPlayerBehaviour.PlaybackRun RecordedRun;
        internal TweenPlayerControl ActiveControl;
        public float Duration { get { RefreshLive(); return Root.Duration; } }
        public float Position
        {
            get
            {
                if (ActiveControl != null) return ActiveControl.Position;
                RefreshLive();
                return Owner != null && ReferenceEquals(Owner.TimelineRun, RecordedRun) && RecordedRun != null ? Root.Position() : 0f;
            }
        }

        private void RefreshLive()
        {
            if (Owner != null && RecordedRun != null && ReferenceEquals(Owner.TimelineRun, RecordedRun)) Root.Refresh();
        }

        internal TweenPlayerTimeline(TweenPlayerBehaviour owner, bool? forward)
        {
            Owner = owner;
            Build = new TimelineBuild { RootOwner = owner };
            Root = Build.Add(owner, forward);
        }

        /// <summary>Transfer a live command, or acquire an idle prepared trajectory, without applying a pose.</summary>
        public TweenPlayerControl TakeControl()
        {
            ValidateTargets();
            TweenPlayerControl.CheckAvailable(this);
            foreach (var parent in Build.Parents)
                if (parent.Key.TimelineRun != null && !parent.Value.OwnsTimelineChild(parent.Key))
                    throw new InvalidOperationException("A child is owned by an unrelated playback command.");
            float position = 0f;
            if (Owner.TimelineRun != null)
            {
                if (!ReferenceEquals(Owner.TimelineRun, RecordedRun))
                    throw new InvalidOperationException("This snapshot does not describe the current playback.");
                RefreshLive();
                position = Root.Position();
                var expected = Evaluate(position);
                foreach (var item in Build.Properties)
                    if (!item.Value.Matches(expected[item.Key]))
                        throw new NotSupportedException("The live trajectory was changed after capture; prepare a new idle timeline.");
            }
            else
                foreach (var member in Build.Members)
                    if (member.TimelineRun != null || member.ManualControl != null)
                        throw new InvalidOperationException("A timeline member is owned by another execution.");

            // Kill only the observed command. Notifications may start replacement playback.
            var previous = Owner.TimelineRun;
            if (previous != null) previous.Kill();
            foreach (var member in Build.Members)
                if (member == null || !member.isActiveAndEnabled || member.TimelineRun != null || member.ManualControl != null)
                    throw new InvalidOperationException("Playback changed during transfer.");
            TweenPlayerControl.CheckAvailable(this);
            return new TweenPlayerControl(this, position);
        }

        /// <summary>Restore the prepare-time values when every member is idle. Capture is retained.</summary>
        public void Restore()
        {
            ValidateTargets();
            TweenPlayerControl.CheckAvailable(this);
            foreach (var member in Build.Members)
                if (member.TimelineRun != null || member.ManualControl != null)
                    throw new InvalidOperationException("Dispose or finish the current execution before restoring.");
            foreach (var property in Build.Properties.Values) property.Write(property.Origin);
        }

        internal void ValidateTargets()
        {
            foreach (var member in Build.Members)
                if (member == null || !member.isActiveAndEnabled)
                    throw new InvalidOperationException("A timeline player was disabled or destroyed.");
            foreach (var property in Build.Properties.Values)
                if (!property.Valid()) throw new InvalidOperationException("A timeline target was released or replaced.");
        }

        private Dictionary<TimelineKey, object> Evaluate(float time)
        {
            var values = new Dictionary<TimelineKey, object>();
            foreach (var property in Build.Properties) values.Add(property.Key, property.Value.Origin);
            Root.Evaluate(TweenTimelineTiming.ClampTime(time, Duration), values);
            return values;
        }

        internal void Apply(float time)
        {
            ValidateTargets();
            var values = Evaluate(time);
            foreach (var property in Build.Properties) property.Value.Write(values[property.Key]);
        }
    }

    internal readonly struct TimelineKey : IEquatable<TimelineKey>
    {
        private readonly int target;
        private readonly string property;
        internal TimelineKey(Component target, string property) { this.target = target.GetInstanceID(); this.property = property; }
        public bool Equals(TimelineKey other) => target == other.target && property == other.property;
        public override bool Equals(object other) => other is TimelineKey key && Equals(key);
        public override int GetHashCode() => unchecked(target * 397 ^ property.GetHashCode());
    }

    internal sealed class TimelineProperty
    {
        internal object Origin;
        internal Action<object> Write;
        internal Func<object, bool> Matches;
        internal Func<bool> Valid;
    }

    internal sealed class TimelineNode
    {
        internal TweenPlayerBehaviour Player;
        internal float Duration;
        internal Action<float, Dictionary<TimelineKey, object>> Evaluate;
        internal Func<float> Position;
        internal Action Refresh = () => { };
        internal readonly HashSet<TimelineKey> Keys = new();
    }

    internal sealed class TimelineBuild
    {
        internal TweenPlayerBehaviour RootOwner;
        internal readonly HashSet<TweenPlayerBehaviour> Members = new();
        internal readonly Dictionary<TweenPlayerBehaviour, TweenPlayerBehaviour> Parents = new();
        internal readonly Dictionary<TimelineKey, TimelineProperty> Properties = new();
        internal readonly Dictionary<TimelineKey, object> Values = new();
        internal ITweenService Service;
        internal UpdateMethod Lane;
        internal bool Unscaled;
        private bool hasClock;

        internal TimelineNode Add(TweenPlayerBehaviour player, bool? forward)
        {
            if (player == null || !player.isActiveAndEnabled)
                throw new NotSupportedException("All timeline members must be active and enabled.");
            if (!Members.Add(player))
                throw new NotSupportedException("A timeline contains a cycle or reuses a player instance.");
            return player.BuildTimeline(this, forward);
        }

        internal void Clock(ITweenService service, UpdateMethod lane, bool unscaled)
        {
            if (service == null) throw new NotSupportedException("Inject a tween service before preparing a timeline.");
            if (hasClock && (!ReferenceEquals(Service, service) || Lane != lane || Unscaled != unscaled))
                throw new NotSupportedException("Timeline members must share a service, update lane and clock.");
            hasClock = true; Service = service; Lane = lane; Unscaled = unscaled;
        }

        internal TimelineNode Composite(TweenPlayerBehaviour owner, TweenPlayerBehaviour[] players,
            bool? forward, bool sequence, bool reverse, Func<IReadOnlyList<TimelineNode>, float> position)
        {
            var nodes = new List<TimelineNode>();
            if (players != null)
                for (int i = 0; i < players.Length; i++)
                {
                    var player = players[reverse ? players.Length - 1 - i : i];
                    if (player == null) continue;
                    nodes.Add(Add(player, forward));
                    Parents.Add(player, owner);
                }
            var result = new TimelineNode { Player = owner };
            foreach (var node in nodes)
            {
                if (!sequence && result.Keys.Overlaps(node.Keys))
                    throw new NotSupportedException("Parallel children write the same target property.");
                result.Keys.UnionWith(node.Keys);
                result.Duration = TweenTimelineTiming.Combine(result.Duration, node.Duration, sequence);
            }
            if (float.IsInfinity(result.Duration)) throw new NotSupportedException("Timeline duration exceeds its finite range.");
            result.Position = () => TweenTimelineTiming.ClampTime(position(nodes), result.Duration);
            result.Refresh = () =>
            {
                result.Duration = 0f;
                foreach (var node in nodes)
                {
                    node.Refresh();
                    result.Duration = TweenTimelineTiming.Combine(result.Duration, node.Duration, sequence);
                }
            };
            result.Evaluate = (time, values) =>
            {
                float offset = 0f;
                foreach (var node in nodes)
                {
                    if (sequence && time < offset) break;
                    node.Evaluate(TweenTimelineTiming.ChildTime(time, offset, node.Duration), values);
                    if (sequence) offset = TweenTimelineTiming.Combine(offset, node.Duration, true);
                }
            };
            return result;
        }
    }
}
