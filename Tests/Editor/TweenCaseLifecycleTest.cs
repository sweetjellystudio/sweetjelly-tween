using NUnit.Framework;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenCaseLifecycleTest
    {
        [Test]
        public void ForcedCompletion_KilledFromTimeReached_DoesNotApplyTheEndpoint()
        {
            var service = new TweenService();
            int writes = 0;
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => writes++);
            playable.AddOnTimeReached(1f, () => playable.Kill());
            var handle = service.Play(playable);
            var task = handle.ToTask();
            handle.Complete();
            Assert.That(writes, Is.Zero);
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(playable.IsCompleted, Is.False);
        }

        [Test]
        public void ForcedCompletion_ValueFailure_CancelsItsAwaitAndPreservesTheError()
        {
            var service = new TweenService();
            var error = new System.InvalidOperationException("Endpoint failed");
            var handle = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => throw error));
            var task = handle.ToTask();
            Assert.That(Assert.Throws<System.InvalidOperationException>(() => handle.Complete()), Is.SameAs(error));
            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void ReplayAfterKill_AdvancesOnlyOncePerTick()
        {
            var service = new TweenService();
            float value = 0f;
            var playable = TweenCases.DOFloat(0f, 1f, 1f, next => value = next);
            service.Play(playable).Kill();
            service.Play(playable);
            service.Tick(UpdateMethod.Update, 0.25f, 0.25f, true);
            Assert.That(value, Is.EqualTo(0.25f));
        }

        [Test]
        public void CompletionObserverFailure_DoesNotKeepApplyingACompletedValue()
        {
            var service = new TweenService();
            int writes = 0;
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => writes++);
            var handle = service.Play(playable);
            handle.Completed += () => throw new System.InvalidOperationException("terminal observer");
            Assert.Throws<System.InvalidOperationException>(() => service.Tick(UpdateMethod.Update, 1f, 1f, true));
            int atCompletion = writes;
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(writes, Is.EqualTo(atCompletion));
            Assert.That(playable.IsActive, Is.False);
        }

        [Test]
        public void FrameWait_HonorsItsOffsetAgainAfterReplay()
        {
            var service = new TweenService();
            int calls = 0;
            var playable = new NextFrameTweenCase(() => calls++, 3);
            for (int run = 0; run < 2; run++)
            {
                var handle = service.Play(playable);
                for (int tick = 0; tick < 2; tick++)
                    service.Tick(UpdateMethod.Update, 1f, 1f, true);
                Assert.That(handle.IsActive, Is.True);
                Assert.That(calls, Is.EqualTo(run));
                service.Tick(UpdateMethod.Update, 1f, 1f, true);
                Assert.That(handle.IsCompleted, Is.True);
                Assert.That(calls, Is.EqualTo(run + 1));
            }
        }

        [Test]
        public void DirectConditionCase_WaitsForExplicitCompletion()
        {
            var service = new TweenService();
            bool ready = false;
            var handle = service.Play(new ConditionTweenCase(item => { if (ready) item.Complete(); }));
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(handle.IsCompleted, Is.False);
            ready = true;
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(handle.IsCompleted, Is.True);
        }

        [Test]
        public void KillFromTimeReached_PreventsFurtherValueWritesInThatTick()
        {
            var service = new TweenService();
            int writes = 0;
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => writes++);
            playable.AddOnTimeReached(0.5f, () => playable.Kill());
            var handle = service.Play(playable);
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(writes, Is.Zero);
            Assert.That(handle.IsKilled, Is.True);
            Assert.That(playable.IsCompleted, Is.False);
        }

        [Test]
        public void DirectWaitCase_DoesNotCompleteUntilItsConditionIsMet()
        {
            var service = new TweenService();
            bool ready = false;
            var handle = service.Play(new WaitUntilTweenCase(() => ready));
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(handle.IsCompleted, Is.False);
            ready = true;
            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(handle.IsCompleted, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OneRun_PublishesOnlyOneTerminalOutcome(bool killFirst)
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            int completed = 0, killed = 0;
            playable.Completed += () => completed++;
            playable.Killed += () => killed++;
            service.Play(playable);
            if (killFirst)
            {
                playable.Kill();
                playable.CompleteImmediately();
                Assert.That(playable.IsCompleted, Is.False);
            }
            else
            {
                service.Tick(UpdateMethod.Update, 1f, 1f, true);
                playable.Kill();
                Assert.That(playable.IsKilled, Is.False);
            }
            Assert.That(completed, Is.EqualTo(killFirst ? 0 : 1));
            Assert.That(killed, Is.EqualTo(killFirst ? 1 : 0));
        }
    }
}
