namespace SWeetJelly.Tween.Tests.EditMode
{
    using System;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;

    public sealed class TweenServiceTest
    {
        [Test]
        public void Play_WithFloatTween_UpdatesValueAndCompletes()
        {
            var service = new TweenService();
            var value = 0f;
            var completedCount = 0;

            var tween = TweenCases.DOFloat(
                0f,
                10f,
                1f,
                next => value = next);

            var handle = service.Play(tween);
            handle.Completed += () => completedCount++;

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(handle.IsCompleted, Is.False);

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(value, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(handle.IsCompleted, Is.True);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_WithDelay_DoesNotInvokeBeforeDelayEnds()
        {
            var service = new TweenService();
            var value = 0f;

            var tween = TweenCases.DOFloat(
                0f,
                10f,
                1f,
                next => value = next,
                delay: 0.5f);

            service.Play(tween);

            service.Tick(UpdateMethod.Update, 0.25f, 0.25f, true);
            Assert.That(value, Is.EqualTo(0f));

            service.Tick(UpdateMethod.Update, 0.25f, 0.25f, true);
            Assert.That(value, Is.EqualTo(0f));

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);
            Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void PauseAndResumeAll_StopsAndRestartsProgress()
        {
            var service = new TweenService();
            var value = 0f;

            service.Play(TweenCases.DOFloat(
                0f,
                10f,
                1f,
                next => value = next));

            service.PauseAll();
            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(value, Is.EqualTo(0f));

            service.ResumeAll();
            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void KillAll_KillsActiveTweenAndRaisesKilled()
        {
            var service = new TweenService();
            var killedCount = 0;

            var handle = service.Play(TweenCases.DOFloat(
                0f,
                10f,
                1f,
                _ => { }));

            handle.Killed += () => killedCount++;

            service.KillAll();

            Assert.That(handle.IsKilled, Is.True);
            Assert.That(killedCount, Is.EqualTo(1));
        }

        [Test]
        public void PlayGroup_WhenAllHandlesComplete_RaisesCompleted()
        {
            var service = new TweenService();
            var completedCount = 0;

            var group = service.PlayGroup(
                TweenCases.DOFloat(0f, 1f, 0.5f, _ => { }),
                TweenCases.DOFloat(0f, 1f, 1f, _ => { }));

            group.Completed += () => completedCount++;

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(group.IsCompleted, Is.False);
            Assert.That(completedCount, Is.EqualTo(0));

            service.Tick(UpdateMethod.Update, 0.5f, 0.5f, true);

            Assert.That(group.IsCompleted, Is.True);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_WithExplicitUpdateMethod_OnlyTicksOnThatLane()
        {
            var service = new TweenService();
            var value = 0f;

            service.Play(
                TweenCases.DOFloat(0f, 1f, 1f, next => value = next),
                UpdateMethod.LateUpdate);

            service.Tick(UpdateMethod.Update, 1f, 1f, true);
            Assert.That(value, Is.EqualTo(0f));

            service.Tick(UpdateMethod.LateUpdate, 1f, 1f, true);
            Assert.That(value, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Play_WithNullPlayable_ThrowsArgumentNullException()
        {
            var service = new TweenService();

            var exception = AssertThrows<ArgumentNullException>(() =>
                service.Play(null));

            Assert.That(exception.ParamName, Is.EqualTo("playable"));
        }

        private static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
            return null;
        }
    }
}
