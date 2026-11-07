using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Samples.Tween
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class TweenUITKSampleBehaviour : MonoBehaviour
    {
        [SerializeField] private TweenPlayerBehaviour optionalPlayer;
        private TweenService service;
        private TweenBehaviour driver;
        private GameObject driverObject;
        private VisualElement sampleRoot;
        private VisualElement box;
        private Label status;
        private CancellationTokenSource cancellation;

        private void OnEnable()
        {
            service = new TweenService();
            driverObject = new GameObject("UITK sample tween driver");
            driverObject.transform.SetParent(transform, false);
            driver = driverObject.AddComponent<TweenBehaviour>();
            driver.Bind(service);
            optionalPlayer?.Initialise(service);
            StartCoroutine(Build());
        }

        private IEnumerator Build()
        {
            yield return null;
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null || root.panel == null)
            {
                Debug.LogError("Assign PanelSettings to the sample UIDocument.", this);
                yield break;
            }
            sampleRoot = new VisualElement { name = "tweenSample" };
            sampleRoot.style.paddingLeft = 24; sampleRoot.style.paddingTop = 24;
            sampleRoot.style.paddingRight = 24; sampleRoot.style.paddingBottom = 24;
            sampleRoot.style.backgroundColor = new Color(.94f, .94f, .94f);
            root.Add(sampleRoot);
            sampleRoot.Add(new Label("Tween opacity and scale"));
            box = new VisualElement { name = "tweenBox" };
            box.style.width = 160; box.style.height = 80;
            box.style.marginTop = 20; box.style.marginBottom = 20;
            box.style.backgroundColor = new Color(.2f, .75f, 1);
            sampleRoot.Add(box);
            sampleRoot.Add(new Button(Replay) { text = "Replay" });
            sampleRoot.Add(new Button(() => cancellation?.Cancel()) { text = "Cancel" });
            status = new Label("Ready"); sampleRoot.Add(status);
        }

        private async void Replay()
        {
            cancellation?.Cancel();
            var token = new CancellationTokenSource();
            cancellation = token;
            try
            {
                box.style.opacity = 1;
                box.style.scale = new Scale(Vector3.one);
                status.text = "Playing";
                var fade = service.Play(box.DOFade(.2f, 1, unscaledTime: true));
                var scale = service.Play(box.DOScale(new Vector3(.4f, 1, 1), 1, unscaledTime: true));
                Task targets = Task.WhenAll(fade.ToTask(token.Token), scale.ToTask(token.Token));
                if (optionalPlayer != null) await Task.WhenAll(targets, optionalPlayer.PlayAsync(token.Token));
                else await targets;
                if (ReferenceEquals(cancellation, token)) status.text = "Completed";
            }
            catch (OperationCanceledException)
            {
                if (ReferenceEquals(cancellation, token) && status != null) status.text = "Canceled";
            }
            catch (Exception error) { Debug.LogException(error, this); }
            finally
            {
                if (ReferenceEquals(cancellation, token)) cancellation = null;
                token.Dispose();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            cancellation?.Cancel(); cancellation = null;
            driver?.Unbind(service); service?.Dispose();
            if (driverObject != null) Destroy(driverObject);
            sampleRoot?.RemoveFromHierarchy(); sampleRoot = null; box = null; status = null;
            driver = null; driverObject = null; service = null;
        }
    }
}
