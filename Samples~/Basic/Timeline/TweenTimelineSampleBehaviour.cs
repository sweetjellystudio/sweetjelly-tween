using System.Threading.Tasks;
using UnityEngine;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

namespace SWeetJelly.Samples.Tween
{
    /// <summary>Assign an existing value/Sequence/Group player; invoke the methods from consumer input.</summary>
    public sealed class TweenTimelineSampleBehaviour : MonoBehaviour
    {
        [SerializeField] private TweenPlayerBehaviour player;
        private TweenService service;
        private TweenPlayerTimeline timeline;
        private TweenPlayerControl control;

        private void Awake()
        {
            service = new TweenService();
            if (player != null) player.Initialise(service);
        }
        private void Update()
        {
            service.Tick(UpdateMethod.Update, Time.deltaTime, Time.unscaledDeltaTime, true);
        }
        private void LateUpdate() => service.Tick(UpdateMethod.LateUpdate, Time.deltaTime, Time.unscaledDeltaTime, true);
        private void FixedUpdate() => service.Tick(UpdateMethod.FixedUpdate, Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime, true);

        [ContextMenu("Start ordinary automatic playback")]
        public void StartAutomatic() { control?.Dispose(); timeline = null; player.PlayForward(); }

        [ContextMenu("Take control without changing pose")]
        public void TakeControl()
        {
            if (control != null && control.IsActive) { control.Sample(control.Position); return; }
            timeline = player.PrepareTimeline();
            control = timeline.TakeControl();
        }

        public void SampleProgress(float progress)
        {
            if (control == null || !control.IsActive) TakeControl();
            control.SampleProgress(progress);
        }

        public Task SettleAsync(bool towardEnd) => control.ResumeAsync(towardEnd);

        [ContextMenu("Restore prepared values")]
        public void Restore() { control?.Dispose(); timeline?.Restore(); }

        private void OnDisable() { control?.Dispose(); if (player != null) player.Kill(); }
        private void OnDestroy() { control?.Dispose(); service?.Dispose(); }
    }
}
