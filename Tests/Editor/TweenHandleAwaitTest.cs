using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenHandleAwaitTest
    {
        [Test]
        public void AwaitCreatedAtEndProgress_WaitsForValueApplicationBeforeCompleting()
        {
            var service = new TweenService();
            var error = new InvalidOperationException("Final value failed");
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => throw error);
            ITweenHandle handle = null;
            Task task = null;
            playable.AddOnTimeReached(1f, () => task = handle.ToTask());
            handle = service.Play(playable);
            Assert.Throws<InvalidOperationException>(() => service.Tick(UpdateMethod.Update, 1f, 1f, true));
            Assert.That(task, Is.Not.Null);
            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void PlayableKillObserverFailure_StillCancelsItsHandleTask()
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            var error = new InvalidOperationException("Playable observer failed");
            playable.Killed += () => throw error;
            var task = service.Play(playable).ToTask();
            Assert.That(Assert.Throws<InvalidOperationException>(() => service.KillAll()), Is.SameAs(error));
            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void CompletionObserverFailure_DoesNotLeaveOtherTerminalObserversWaiting()
        {
            var service = new TweenService();
            var handle = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { }));
            var error = new InvalidOperationException("Observer failed");
            handle.Completed += () => throw error;
            var task = handle.ToTask();
            Assert.That(Assert.Throws<InvalidOperationException>(() => service.Tick(UpdateMethod.Update, 1f, 1f, true)), Is.SameAs(error));
            Assert.That(task.IsCompleted, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ToTask_ReleasesBothSubscriptions_WhenItsRunTerminates(bool cancel)
        {
            var handle = new ObservedHandle();
            using var source = new CancellationTokenSource();
            var task = handle.ToTask(source.Token);
            Assert.That(handle.Subscriptions, Is.EqualTo(2));
            if (cancel) source.Cancel();
            else handle.Complete();
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsCanceled, Is.EqualTo(cancel));
            Assert.That(handle.Subscriptions, Is.Zero);
        }

        [Test]
        public void CompletedHandle_RemainsTerminal_WhenThePlayableIsReused()
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            var first = service.Play(playable);
            int completed = 0;
            first.Completed += () => completed++;
            service.Tick(UpdateMethod.Update, 1f, 1f, true);

            var second = service.Play(playable);
            Assert.That(first.IsActive, Is.False);
            Assert.That(first.IsCompleted, Is.True);
            second.Kill();
            Assert.That(first.IsKilled, Is.False);
            Assert.That(completed, Is.EqualTo(1));
        }

        [Test]
        public void ToTask_AfterKill_ReturnsAnAlreadyCanceledTask()
        {
            var handle = new TweenService().Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { }));
            handle.Kill();
            Assert.That(handle.ToTask().IsCanceled, Is.True);
        }

        private sealed class ObservedHandle : ITweenHandle
        {
            private Action completed;
            private Action killed;
            public int Subscriptions;
            public bool IsActive => !IsCompleted && !IsKilled;
            public bool IsCompleted { get; private set; }
            public bool IsKilled { get; private set; }
            public event Action Completed { add { completed += value; Subscriptions++; } remove { completed -= value; Subscriptions--; } }
            public event Action Killed { add { killed += value; Subscriptions++; } remove { killed -= value; Subscriptions--; } }
            public void Pause() { }
            public void Resume() { }
            public void Complete() { IsCompleted = true; completed?.Invoke(); }
            public void Kill() { IsKilled = true; killed?.Invoke(); }
        }
    }
}
