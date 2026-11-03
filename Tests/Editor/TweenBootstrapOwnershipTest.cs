using System;
using NUnit.Framework;
using SWeetJelly.Bootstrapper.App;
using SWeetJelly.Bootstrapper.Framework.Unity;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Bootstrapper;
using SWeetJelly.Tween.Framework.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SWeetJelly.Tween.Tests.EditMode
{
    public sealed class TweenBootstrapOwnershipTest
    {
        [Test]
        public void Shutdown_WhenAKillObserverThrows_StillClosesAllOwnedWork()
        {
            var holder = new GameObject("Throwing shutdown");
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                module.Bootstrap(context);
                var owned = registry.Resolve<ITweenService>();
                var first = owned.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { }));
                var error = new InvalidOperationException("Kill observer failed");
                first.Killed += () => throw error;
                var task = owned.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { })).ToTask();
                Assert.That(Assert.Throws<InvalidOperationException>(() => module.Shutdown(context)), Is.SameAs(error));
                Assert.That(task.IsCanceled, Is.True);
                Assert.That(registry.TryResolve<ITweenService>(out _), Is.False);
                Assert.That(holder.GetComponent<TweenBehaviour>(), Is.Null);
                Assert.Throws<ObjectDisposedException>(() => owned.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { })));
            }
            finally { Object.DestroyImmediate(module); Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Bootstrap_RejectsAnAlreadyBoundBorrowedDriver()
        {
            var holder = new GameObject("Borrowed driver");
            var driver = holder.AddComponent<TweenBehaviour>();
            var foreign = new TweenService();
            driver.Bind(foreign);
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();
            var registry = new ServiceRegistry();
            try
            {
                Assert.Throws<InvalidOperationException>(() => module.Bootstrap(
                    new PackageBootstrapContext(holder, null, null, registry)));
                module.Shutdown(null);
                Assert.That(driver.Unbind(foreign), Is.True);
                Assert.That(registry.TryResolve<ITweenService>(out _), Is.False);
            }
            finally { Object.DestroyImmediate(module); Object.DestroyImmediate(holder); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RegistrationFailure_RollsBackTheCreatedDriver_AndAllowsRetry(bool registerBeforeFailure)
        {
            var holder = new GameObject("Tween rollback");
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();
            var registry = new FailingRegistry { RegisterBeforeFailure = registerBeforeFailure };
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                Assert.Throws<InvalidOperationException>(() => module.Bootstrap(context));
                Assert.That(registry.TryResolve<ITweenService>(out _), Is.False);
                Assert.That(holder.GetComponent<TweenBehaviour>(), Is.Null);
                registry.Fail = false;
                Assert.DoesNotThrow(() => module.Bootstrap(context));
                module.Shutdown(null);
            }
            finally { Object.DestroyImmediate(module); Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Shutdown_CancelsOwnedRuns_AndPreservesAReplacementRegistration()
        {
            var holder = new GameObject("Tween shutdown");
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                module.Bootstrap(context);
                var owned = registry.Resolve<ITweenService>();
                var task = owned.Play(TweenCases.DOFloat(0f, 1f, 1f, _ => { })).ToTask();
                registry.Unregister<ITweenService>();
                var replacement = new TweenService();
                registry.Register<ITweenService>(replacement);

                module.Shutdown(context);
                Assert.That(task.IsCanceled, Is.True);
                Assert.That(registry.Resolve<ITweenService>(), Is.SameAs(replacement));
                Assert.That(holder.GetComponent<TweenBehaviour>(), Is.Null);
                Assert.DoesNotThrow(() => module.Shutdown(null));
            }
            finally { Object.DestroyImmediate(module); Object.DestroyImmediate(holder); }
        }

        [Test]
        public void ExistingRegistration_IsPreservedByRejectedBootstrapAndShutdown()
        {
            var holder = new GameObject("Tween ownership");
            var module = ScriptableObject.CreateInstance<TweenPackageBootstrapperSo>();
            var registry = new ServiceRegistry();
            var foreign = new TweenService();
            registry.Register<ITweenService>(foreign);
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                Assert.Throws<InvalidOperationException>(() => module.Bootstrap(context));
                module.Shutdown(context);
                Assert.That(registry.Resolve<ITweenService>(), Is.SameAs(foreign));
            }
            finally { Object.DestroyImmediate(module); Object.DestroyImmediate(holder); }
        }

        private sealed class FailingRegistry : SWeetJelly.Bootstrapper.Contract.IServiceRegistry
        {
            private readonly ServiceRegistry inner = new ServiceRegistry();
            public bool Fail = true;
            public bool RegisterBeforeFailure;
            public void Register<T>(T instance) where T : class
            {
                if (!Fail || RegisterBeforeFailure) inner.Register(instance);
                if (Fail) throw new InvalidOperationException("Registry failure");
            }
            public bool TryResolve<T>(out T instance) where T : class => inner.TryResolve(out instance);
            public T Resolve<T>() where T : class => inner.Resolve<T>();
            public void Unregister<T>() where T : class => inner.Unregister<T>();
        }
    }
}
