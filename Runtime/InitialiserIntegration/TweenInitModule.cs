namespace SWeetJelly.Tween.InitialiserIntegration
{
    using SWeetJelly.Initialiser.Contract;
    using SWeetJelly.Initialiser.Framework;
    using UnityEngine;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Framework;

    [RegisterModule("Tween")]
    [CreateAssetMenu(
        fileName = "Tween Init Module", 
        menuName = "SWeetJelly/Initialiser/Modules/Tween")]
    public class TweenInitModule: InitModuleSo
    {
        private TweenService service;

        public override string ModuleName => "Tween";

        public override void Install(InitModuleInstallContext context)
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

        public override void Uninstall(InitModuleInstallContext context)
        {
            context.Services.Unregister<ITweenService>();

            service = null;
        }

    }
}