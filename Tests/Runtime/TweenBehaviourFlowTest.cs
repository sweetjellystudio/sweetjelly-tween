namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;

    public sealed class TweenBehaviourFlowTest
    {
        [UnityTest]
        public IEnumerator TweenBehaviour_WhenBound_TicksTweenServiceOnUpdate()
        {
            var holder = new GameObject("Tween Behaviour Test Holder");
            var behaviour = holder.AddComponent<TweenBehaviour>();
            var service = new TweenService();
            var value = 0f;

            try
            {
                behaviour.Bind(service);

                service.Play(TweenCases.DOFloat(
                    0f,
                    1f,
                    0.01f,
                    next => value = next));

                var timeoutAt = Time.realtimeSinceStartup + 1f;

                while (value < 1f && Time.realtimeSinceStartup < timeoutAt)
                    yield return null;

                Assert.That(value, Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.Destroy(holder);
            }
        }
    }
}
