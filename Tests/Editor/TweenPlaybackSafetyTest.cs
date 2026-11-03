using System;
using NUnit.Framework;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenPlaybackSafetyTest
    {
        [Test]
        public void KillAll_DoesNotKillWorkAddedAfterTheCallInAnotherLane()
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            ITweenHandle added = null;
            playable.Killed += () => added = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { }).SetUpdateMethod(UpdateMethod.FixedUpdate));
            service.Play(playable);
            service.KillAll();
            Assert.That(added, Is.Not.Null);
            Assert.That(added.IsActive, Is.True);
            service.KillAll();
            Assert.That(added.IsKilled, Is.True);
        }

        [Test]
        public void Play_WithInvalidLane_DoesNotActivateOrChangeThePlayable()
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Play(playable, (UpdateMethod)999));
            Assert.That(playable.IsActive, Is.False);
            Assert.That(playable.UpdateMethod, Is.EqualTo(UpdateMethod.Update));
        }

        [Test]
        public void ReentrantTick_IsRejected_AndTheLaneRecovers()
        {
            var service = new TweenService();
            bool nested = false;
            service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ =>
            {
                if (nested) return;
                nested = true;
                service.Tick(UpdateMethod.Update, 0.1f, 0.1f, true);
            }));
            Assert.Throws<InvalidOperationException>(() => service.Tick(UpdateMethod.Update, 0.1f, 0.1f, true));
            float value = 0f;
            service.Play(TweenCases.DOFloat(0f, 1f, 1f, next => value = next));
            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);
            Assert.That(value, Is.EqualTo(0.5f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Play_RejectsAnAlreadyActivePlayable(bool explicitLane)
        {
            var service = new TweenService();
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            var first = service.Play(playable);
            Assert.Throws<InvalidOperationException>(() =>
            {
                if (explicitLane) service.Play(playable, UpdateMethod.LateUpdate);
                else service.Play(playable);
            });
            Assert.That(first.IsActive, Is.True);
            Assert.That(playable.UpdateMethod, Is.EqualTo(UpdateMethod.Update));
        }

        [Test]
        public void ThrowingCallback_CancelsItsTask_PreservesTheError_AndAllowsTheNextTick()
        {
            var service = new TweenService();
            var error = new InvalidOperationException("execution failed");
            var failing = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => throw error));
            var task = failing.ToTask();
            float laterValue = 0f;
            service.Play(TweenCases.DOFloat(0f, 1f, 1f, value => laterValue = value));

            Assert.That(Assert.Throws<InvalidOperationException>(() =>
                service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true)), Is.SameAs(error));
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(laterValue, Is.Zero);
            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);
            Assert.That(laterValue, Is.EqualTo(0.5f));
        }

        [Test]
        public void KillAllDuringCallback_AlsoKillsRunsAddedEarlierInThatCallback()
        {
            var service = new TweenService();
            ITweenHandle pending = null;
            service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ =>
            {
                pending = service.Play(TweenCases.DOFloat(0f, 1f, 1f, value => { }));
                service.KillAll();
            }));

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);
            Assert.That(pending, Is.Not.Null);
            Assert.That(pending.IsKilled, Is.True);
        }

        [Test]
        public void KillAllDuringCallback_KillsBothRunsWithoutInvalidatingTheTick()
        {
            var service = new TweenService();
            var first = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => service.KillAll()));
            int laterInvocations = 0;
            var second = service.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => laterInvocations++));

            Assert.DoesNotThrow(() => service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true));
            Assert.That(first.IsKilled, Is.True);
            Assert.That(second.IsKilled, Is.True);
            Assert.That(laterInvocations, Is.Zero);
        }
    }
}
