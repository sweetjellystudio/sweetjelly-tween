using System;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed partial class TweenPlayerSequenceBehaviour
    {
        internal override bool OwnsTimelineChild(TweenPlayerBehaviour child) =>
            ReferenceEquals(currentPlayer, child) && ReferenceEquals(currentChildRun, child.TimelineRun);
        internal override TimelineNode BuildTimeline(TimelineBuild build, bool? forward) =>
            build.Composite(this, players, forward, true, forward == false && reverseWhenPlayingBackward, nodes =>
            {
                float elapsed = 0f;
                foreach (var node in nodes)
                {
                    if (ReferenceEquals(node.Player, currentPlayer)) return elapsed + node.Position();
                    elapsed += node.Duration;
                }
                return elapsed;
            });

        internal override void NotifyTimelineTerminal(bool success)
        {
            if (success) NotifyAllTerminals(NotifyCompleted, () => completed?.Invoke());
            else NotifyAllTerminals(NotifyKilled, () => killed?.Invoke());
        }
    }

    public sealed partial class TweenPlayerGroupBehaviour
    {
        internal override bool OwnsTimelineChild(TweenPlayerBehaviour player)
        {
            foreach (var child in children)
                if (ReferenceEquals(child.Player, player) && ReferenceEquals(child.Operation, player.TimelineRun)) return true;
            return false;
        }
        internal override TimelineNode BuildTimeline(TimelineBuild build, bool? forward) =>
            build.Composite(this, players, forward, false, false, nodes =>
            {
                float elapsed = 0f;
                foreach (var node in nodes) elapsed = Math.Max(elapsed, node.Position());
                return elapsed;
            });

        internal override void NotifyTimelineTerminal(bool success)
        {
            if (success) NotifyAllTerminals(NotifyCompleted, () => completed?.Invoke());
            else NotifyAllTerminals(NotifyKilled, () => killed?.Invoke());
        }
    }
}
