namespace SWeetJelly.Samples.Tween
{
    using System;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;

    public sealed class TweenPlayerSampleInjectorBehaviour : MonoBehaviour
    {
        [Header("GUI Layout")]
        [SerializeField] private float guiX = 16f;
        [SerializeField] private float guiY = 16f;
        [SerializeField] private float labelWidth = 680f;
        [SerializeField] private float labelHeight = 24f;
        [SerializeField] private float labelSpacing = 36f;
        [SerializeField] private float buttonWidth = 190f;
        [SerializeField] private float buttonHeight = 28f;
        [SerializeField] private float buttonSpacing = 34f;

        private ITweenService tweenService;
        private string status = "Waiting for Tween service...";

        private void OnEnable()
        {
            BootstrapperBehaviour.Bootstrapped += OnBootstrapped;
            ResolveIfReady();
        }

        private void OnDisable()
        {
            BootstrapperBehaviour.Bootstrapped -= OnBootstrapped;
            tweenService = null;
        }

        private void OnBootstrapped(object sender, EventArgs args)
        {
            ResolveIfReady();
        }

        private void ResolveIfReady()
        {
            if (tweenService != null || !BootstrapperBehaviour.IsBootstrapped || BootstrapperBehaviour.Services == null)
                return;

            if (!BootstrapperBehaviour.Services.TryResolve<ITweenService>(out tweenService))
            {
                status = "ITweenService is not registered.";
                return;
            }

            InjectPlayers();
        }

        private void OnGUI()
        {
            var x = guiX;
            var y = guiY;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), status);
            y += labelSpacing;

            GUI.enabled = tweenService != null;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Replay Tween Players"))
                ReplayPlayers();

            y += buttonSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Inject Again"))
                InjectPlayers();

            GUI.enabled = true;
        }

        private void InjectPlayers()
        {
            var count = 0;

            foreach (var player in FindObjectsByType<TweenPlayerBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                player.Initialise(tweenService);
                count++;
            }

            status = $"Injected ITweenService into {count} TweenPlayer component(s).";
        }

        private void ReplayPlayers()
        {
            foreach (var behaviour in FindObjectsByType<TweenPlayerBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour.GetComponentsInParent<TweenPlayerBehaviour>().Length > 1) continue;
                behaviour.Play();
            }

            status = "TweenPlayer playback started through its public Play API.";
        }

    }
}
