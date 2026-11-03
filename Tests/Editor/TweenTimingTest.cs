using System;
using NUnit.Framework;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenTimingTest
    {
        [Test]
        public void ZeroScaledDelta_SkipsScaledFrameCallbacks_WhileUnscaledContinues()
        {
            var service = new TweenService();
            int scaled = 0;
            int unscaled = 0;
            service.Play(TweenCases.NextFrame(() => scaled++));
            service.Play(TweenCases.NextFrame(() => unscaled++, unscaledTime: true));
            service.Tick(UpdateMethod.Update, 0f, 0.1f, true);
            Assert.That(scaled, Is.Zero);
            Assert.That(unscaled, Is.EqualTo(1));
            service.Tick(UpdateMethod.Update, 0.1f, 0.1f, true);
            Assert.That(scaled, Is.EqualTo(1));
        }

        [Test]
        public void NegativeDelta_DoesNotRewindProgressOrDelay()
        {
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { }).SetDelay(1f);
            playable.Activate(UpdateMethod.Update);
            playable.UpdateDelay(0.5f);
            playable.UpdateDelay(-0.25f);
            Assert.That(playable.CurrentDelay, Is.EqualTo(0.5f));
            playable.UpdateState(0.5f);
            playable.UpdateState(-0.25f);
            Assert.That(playable.State, Is.EqualTo(0.5f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteTime_IsRejectedBeforeChangingTheRun(float value)
        {
            var playable = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            Assert.Throws<ArgumentOutOfRangeException>(() => playable.SetDuration(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => playable.SetDelay(value));
            var service = new TweenService();
            var handle = service.Play(playable);
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Tick(UpdateMethod.Update, value, 1f, true));
            Assert.That(playable.State, Is.Zero);
            Assert.That(handle.IsActive, Is.True);
        }
    }
}
