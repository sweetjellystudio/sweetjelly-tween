namespace SWeetJelly.Tween.Tests.EditMode
{
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using SWeetJelly.Tween.Framework.Bootstrapper;
    using UnityEngine;

    public sealed class TweenPackageIntegrationTest
    {
        [Test]
        public void TweenPackageBootstrapperSo_RegistersTweenServiceAndAddsTweenBehaviour()
        {
            var holder = new GameObject("Tween Test Holder");
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();

            try
            {
                var registry = new ServiceRegistry();
                var context = new PackageBootstrapContext(holder, null, null, registry);

                module.Bootstrap(context);

                Assert.That(holder.GetComponent<TweenBehaviour>(), Is.Not.Null);
                Assert.That(registry.TryResolve<ITweenService>(out var service), Is.True);
                Assert.That(service, Is.Not.Null);

                module.Shutdown(context);

                Assert.That(registry.TryResolve<ITweenService>(out _), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(module);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }
    }
}
