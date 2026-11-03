using NUnit.Framework;
using SWeetJelly.Tween.Framework.Unity;
using UnityEditor;
using UnityEngine;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenPlayerCompositionTest
    {
        [Test]
        public void Sequence_NextChildStartupFailure_CancelsItsOwnedRun()
        {
            var root = new GameObject("Next child failure");
            var firstObject = new GameObject("First child");
            var secondObject = new GameObject("Failing next child");
            try
            {
                var sequence = root.AddComponent<TweenPlayerSequenceBehaviour>();
                var first = firstObject.AddComponent<CompositionTestPlayer>();
                var second = secondObject.AddComponent<CompositionTestPlayer>(); second.ThrowOnPlay = true;
                SetPlayers(sequence, first, second);
                int killed = 0; sequence.Killed += () => killed++;
                sequence.Play();
                Assert.Throws<System.InvalidOperationException>(() => first.CompleteNow());
                Assert.That(sequence.IsPlaying, Is.False);
                Assert.That(second.IsPlaying, Is.False);
                Assert.That(killed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(secondObject); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Composition_ChildStartupFailure_CancelsItsOwnedRun(bool sequential)
        {
            var root = new GameObject("Failed composition");
            var childObject = new GameObject("Failing child");
            try
            {
                TweenPlayerBehaviour composition = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                var child = childObject.AddComponent<CompositionTestPlayer>();
                child.ThrowOnPlay = true;
                SetPlayers(composition, child);
                int killed = 0;
                composition.Killed += () => killed++;
                Assert.Throws<System.InvalidOperationException>(() => composition.Play());
                Assert.That(composition.IsPlaying, Is.False);
                Assert.That(child.IsPlaying, Is.False);
                Assert.That(killed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(childObject); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Composition_SynchronousChildren_CompleteOnce(bool sequential)
        {
            var root = new GameObject("Synchronous composition");
            var childObject = new GameObject("Synchronous child");
            try
            {
                TweenPlayerBehaviour composition = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                var child = childObject.AddComponent<CompositionTestPlayer>();
                child.CompleteSynchronously = true;
                SetPlayers(composition, child);
                int completed = 0;
                composition.Completed += () => completed++;
                composition.Play();
                Assert.That(composition.IsPlaying, Is.False);
                Assert.That(completed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(childObject); }
        }

        [Test]
        public void ChildCompletionObserverFailure_DoesNotLeaveTheGroupWaiting()
        {
            var root = new GameObject("Terminal observer group");
            var childObject = new GameObject("Child player");
            try
            {
                var group = root.AddComponent<TweenPlayerGroupBehaviour>();
                var child = childObject.AddComponent<CompositionTestPlayer>();
                child.Completed += () => throw new System.InvalidOperationException("Observer failed");
                SetPlayers(group, child);
                int completed = 0;
                group.Completed += () => completed++;
                group.Play();
                Assert.Throws<System.InvalidOperationException>(() => child.CompleteNow());
                Assert.That(group.IsPlaying, Is.False);
                Assert.That(completed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(childObject); }
        }

        [Test]
        public void PropertyPlayer_KilledCallback_CanStartAReplacementPlayback()
        {
            var holder = new GameObject("Player replay", typeof(CanvasGroup));
            holder.SetActive(false);
            try
            {
                var service = new SWeetJelly.Tween.App.TweenService();
                var player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
                player.Initialise(service);
                holder.SetActive(true);
                bool replayed = false;
                player.Killed += () => { if (!replayed) { replayed = true; player.PlayForward(); } };
                player.PlayForward();
                player.Kill();
                Assert.That(player.IsPlaying, Is.True);
                service.Tick(SWeetJelly.Tween.Contract.UpdateMethod.Update, 1f, 1f, true);
                Assert.That(player.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(holder); }
        }

        [Test]
        public void PropertyPlayer_KillFromPlayed_EmitsKilledOnce()
        {
            var holder = new GameObject("Player immediate cancellation", typeof(CanvasGroup));
            holder.SetActive(false);
            try
            {
                var player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
                player.Initialise(new SWeetJelly.Tween.App.TweenService());
                holder.SetActive(true);
                int killed = 0;
                player.Played += player.Kill;
                player.Killed += () => killed++;
                player.PlayForward();
                Assert.That(killed, Is.EqualTo(1));
                Assert.That(player.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Sequence_StaleChildCompletion_CannotAdvanceAReplacementRun()
        {
            var root = new GameObject("Replayed player sequence");
            var firstObject = new GameObject("First player");
            var secondObject = new GameObject("Second player");
            try
            {
                var sequence = root.AddComponent<TweenPlayerSequenceBehaviour>();
                var first = firstObject.AddComponent<CompositionTestPlayer>();
                var second = secondObject.AddComponent<CompositionTestPlayer>();
                SetPlayers(sequence, first, second);
                bool replayed = false;
                first.Completed += () => { if (!replayed) { replayed = true; sequence.Play(); } };
                sequence.Play();
                first.CompleteNow();
                Assert.That(first.IsPlaying, Is.True);
                Assert.That(second.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(secondObject); }
        }

        [Test]
        public void Group_StaleChildCompletion_CannotCompleteAReplacementRun()
        {
            var root = new GameObject("Replayed player group");
            var firstObject = new GameObject("First player");
            var secondObject = new GameObject("Second player");
            try
            {
                var group = root.AddComponent<TweenPlayerGroupBehaviour>();
                var first = firstObject.AddComponent<CompositionTestPlayer>();
                var second = secondObject.AddComponent<CompositionTestPlayer>();
                SetPlayers(group, first, second);
                int completed = 0;
                bool replayed = false;
                first.Completed += () => { if (!replayed) { replayed = true; group.Play(); } };
                group.Completed += () => completed++;
                group.Play();
                first.CompleteNow();
                second.CompleteNow();
                Assert.That(completed, Is.Zero);
                Assert.That(group.IsPlaying, Is.True);
                first.CompleteNow();
                Assert.That(completed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(secondObject); }
        }

        [Test]
        public void Group_ChildKill_StopsTheOtherChildItStarted()
        {
            var root = new GameObject("Canceled player group");
            var firstObject = new GameObject("First player");
            var secondObject = new GameObject("Second player");
            try
            {
                var group = root.AddComponent<TweenPlayerGroupBehaviour>();
                var first = firstObject.AddComponent<CompositionTestPlayer>();
                var second = secondObject.AddComponent<CompositionTestPlayer>();
                SetPlayers(group, first, second);
                group.Play();
                first.Kill();
                Assert.That(second.IsPlaying, Is.False);
                Assert.That(group.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(secondObject); }
        }

        [Test]
        public void Group_MixedCompletionAndKill_TerminatesAsKilledOnce()
        {
            var root = new GameObject("Player group");
            var firstObject = new GameObject("First player");
            var secondObject = new GameObject("Second player");
            try
            {
                var group = root.AddComponent<TweenPlayerGroupBehaviour>();
                var first = firstObject.AddComponent<CompositionTestPlayer>();
                var second = secondObject.AddComponent<CompositionTestPlayer>();
                SetPlayers(group, first, second);
                int completed = 0, killed = 0;
                group.Completed += () => completed++;
                group.Killed += () => killed++;
                group.Play();
                first.CompleteNow();
                second.Kill();
                Assert.That(group.IsPlaying, Is.False);
                Assert.That(killed, Is.EqualTo(1));
                Assert.That(completed, Is.Zero);
                group.Kill();
                Assert.That(killed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(secondObject); }
        }

        private static void SetPlayers(Object composition, params TweenPlayerBehaviour[] players)
        {
            var settings = new SerializedObject(composition);
            var items = settings.FindProperty("players");
            items.arraySize = players.Length;
            for (int i = 0; i < players.Length; i++) items.GetArrayElementAtIndex(i).objectReferenceValue = players[i];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public sealed class CompositionTestPlayer : TweenPlayerBehaviour
    {
        private bool playing;
        public bool CompleteSynchronously;
        public bool ThrowOnPlay;
        public override bool IsPlaying => playing;
        public override void Play() { playing = true; NotifyPlayed(); if (ThrowOnPlay) throw new System.InvalidOperationException("Child startup failed"); if (CompleteSynchronously) CompleteNow(); }
        public override void Play(bool forward) => Play();
        public override void PlayForward() => Play();
        public override void PlayBackward() => Play();
        public override void Snap(bool forward) { Play(); CompleteNow(); }
        public override void SnapForward() => Snap(true);
        public override void SnapBackward() => Snap(false);
        public override void Kill() { if (!playing) return; playing = false; NotifyKilled(); }
        public override void SetPlayOnEnable(bool enabled) { }
        public void CompleteNow() { if (!playing) return; playing = false; NotifyCompleted(); }
    }
}
