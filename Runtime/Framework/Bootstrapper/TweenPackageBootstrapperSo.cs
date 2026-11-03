namespace SWeetJelly.Tween.Framework.Bootstrapper
{
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Bootstrapper.Contract;
    using UnityEngine;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Framework.Unity;

    [CreateAssetMenu(
        fileName = "Tween Package Bootstrapper", 
        menuName = "SWeetJelly/Bootstrapper/Packages/Tween")]
    public class TweenPackageBootstrapperSo : PackageBootstrapperSo
    {
        private TweenService service;
        private IServiceRegistry registry;
        private TweenBehaviour driver;
        private bool ownsDriver;

        public override string PackageName => "Tween";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException(nameof(context));

            if (service != null || context.Services.TryResolve<ITweenService>(out _))
                throw new System.InvalidOperationException("A tween service is already active.");

            var tweenBehaviour = context.HolderObject.GetComponent<TweenBehaviour>();
            if (tweenBehaviour != null && tweenBehaviour.IsBound)
                throw new System.InvalidOperationException("The tween driver is already bound.");
            bool createdDriver = false;
            if (tweenBehaviour == null)
            {
                tweenBehaviour = context.HolderObject.AddComponent<TweenBehaviour>();
                createdDriver = true;
            }
            var created = new TweenService();
            try
            {
                context.Services.Register<ITweenService>(created);
                tweenBehaviour.Bind(created);
            }
            catch
            {
                try { created.Dispose(); } catch { }
                try
                {
                    if (context.Services.TryResolve<ITweenService>(out var registered) && ReferenceEquals(registered, created))
                        context.Services.Unregister<ITweenService>();
                }
                catch { }
                try
                {
                    tweenBehaviour.Unbind(created);
                    if (createdDriver && !tweenBehaviour.IsBound)
                    {
                        if (Application.isPlaying) Object.Destroy(tweenBehaviour);
                        else Object.DestroyImmediate(tweenBehaviour);
                    }
                }
                catch { }
                throw;
            }
            service = created;
            registry = context.Services;
            driver = tweenBehaviour;
            ownsDriver = createdDriver;
        }

        public override void Shutdown(PackageBootstrapContext context)
        {
            if (service == null) return;
            var owned = service;
            service = null;
            var ownedRegistry = registry;
            var ownedDriver = driver;
            bool destroyDriver = ownsDriver;
            registry = null;
            driver = null;
            ownsDriver = false;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo failure = null;
            try { owned.Dispose(); }
            catch (System.Exception error) { failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            try
            {
                if (ownedRegistry != null && ownedRegistry.TryResolve<ITweenService>(out var registered) &&
                    ReferenceEquals(registered, owned)) ownedRegistry.Unregister<ITweenService>();
            }
            catch (System.Exception error) { if (failure == null) failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            try
            {
                if (ownedDriver != null && ownedDriver.Unbind(owned) && destroyDriver)
                {
                    if (Application.isPlaying) Object.Destroy(ownedDriver);
                    else Object.DestroyImmediate(ownedDriver);
                }
            }
            catch (System.Exception error) { if (failure == null) failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            failure?.Throw();
        }
    }
}
