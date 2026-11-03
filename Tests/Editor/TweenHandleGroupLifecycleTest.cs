using System;
using NUnit.Framework;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenHandleGroupLifecycleTest
    {
        [Test]
        public void MixedOutcomes_CancelTheGroupTaskAndItsRemainingRun()
        {
            var service = new TweenService();
            var group = service.PlayGroup(
                TweenCases.DOFloat(0f, 1f, 1f, _ => { }),
                TweenCases.DOFloat(0f, 1f, 1f, _ => { }),
                TweenCases.DOFloat(0f, 1f, 1f, _ => { }));
            var task = group.ToTask();
            group.Handles[0].Complete();
            group.Handles[1].Kill();
            Assert.That(group.IsKilled, Is.True);
            Assert.That(group.IsActive, Is.False);
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(group.Handles[2].IsKilled, Is.True);
        }

        [Test]
        public void PlayGroupFailure_RollsBackOnlyRunsStartedByThatCall()
        {
            var service = new TweenService();
            var existing = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            var created = TweenCases.DOFloat(0f, 1f, 1f, _ => { });
            var previous = service.Play(existing);
            Assert.Throws<InvalidOperationException>(() => service.PlayGroup(created, existing));
            Assert.That(previous.IsActive, Is.True);
            Assert.That(created.IsActive, Is.False);
        }
    }
}
