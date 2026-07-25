namespace SWeetJelly.Tween.Framework.Bootstrapper
{
    using SWeetJelly.Bootstrapper.Framework.Unity;
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

        public override string PackageName => "Tween";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException(nameof(context));

            var tweenBehaviour = context.HolderObject.GetComponent<TweenBehaviour>();

            if (tweenBehaviour == null)
            {
                tweenBehaviour = context.HolderObject.AddComponent<TweenBehaviour>();
            }

            service = new TweenService();

            tweenBehaviour.Bind(service);

            context.Services.Register<ITweenService>(service);
        }

        public override void Shutdown(PackageBootstrapContext context)
        {
            context.Services.Unregister<ITweenService>();

            service = null;
        }
    }
}
