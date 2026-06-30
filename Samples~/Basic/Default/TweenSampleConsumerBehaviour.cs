namespace SWeetJelly.Samples.Tween
{
    using System;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;

    public sealed class TweenSampleConsumerBehaviour : MonoBehaviour
    {
        [Header("GUI Layout")]
        [SerializeField] private float guiX = 16f;
        [SerializeField] private float guiY = 16f;
        [SerializeField] private float labelWidth = 640f;
        [SerializeField] private float labelHeight = 24f;
        [SerializeField] private float labelRowSpacing = 28f;
        [SerializeField] private float buttonWidth = 180f;
        [SerializeField] private float smallButtonWidth = 86f;
        [SerializeField] private float buttonHeight = 28f;
        [SerializeField] private float buttonRowSpacing = 34f;
        [SerializeField] private float buttonPairGap = 8f;
        [SerializeField] private float sectionSpacing = 42f;

        [Header("Tween Settings")]
        [SerializeField] private EaseType currentEasing = EaseType.CubicInOut;
        [SerializeField] private float moveDuration = 0.7f;
        [SerializeField] private float scaleDuration = 0.5f;
        [SerializeField] private float rotateShakeDuration = 0.8f;
        [SerializeField] private float groupDuration = 0.6f;
        [SerializeField] private float delayedColorDelay = 0.35f;
        [SerializeField] private float delayedColorDuration = 0.5f;
        [SerializeField] private float cameraFieldOfViewDuration = 0.55f;

        [Header("Transform Targets")]
        [SerializeField] private Vector3 initialPosition = new Vector3(0f, 0f, 5f);
        [SerializeField] private Vector3 initialRotationEuler = Vector3.zero;
        [SerializeField] private Vector3 movePositivePosition = new Vector3(2.5f, 0f, 5f);
        [SerializeField] private Vector3 moveNegativePosition = new Vector3(-2.5f, 0f, 5f);
        [SerializeField] private Vector3 initialScale = Vector3.one;
        [SerializeField] private Vector3 scaledTarget = new Vector3(1.8f, 1.8f, 1.8f);
        [SerializeField] private Vector3 rotationTarget = new Vector3(0f, 180f, 0f);
        [SerializeField] private float shakeMagnitude = 0.18f;

        [Header("Group Targets")]
        [SerializeField] private Vector3 groupMovePosition = new Vector3(0f, 1.25f, 5f);
        [SerializeField] private Vector3 groupReturnPosition = new Vector3(0f, 0f, 5f);
        [SerializeField] private float groupScale = 1.35f;
        [SerializeField] private float groupReturnScale = 1f;
        [SerializeField] private Color groupColorA = new Color(0.2f, 0.85f, 0.95f);
        [SerializeField] private Color groupColorB = new Color(1f, 0.55f, 0.25f);

        [Header("Camera Targets")]
        [SerializeField] private float defaultFieldOfView = 60f;
        [SerializeField] private float wideFieldOfViewTarget = 82f;
        [SerializeField] private Color cameraBackgroundColor = new Color(0.19f, 0.3f, 0.47f, 1f);

        [Header("Runtime Target")]
        [SerializeField] private string targetObjectName = "Tween Sample Target";
        [SerializeField] private Color initialColor = new Color(1f, 0.55f, 0.25f);

        private ITweenService tweenService;
        private ITweenHandle activeHandle;
        private ITweenHandle delayedColorHandle;
        private Transform target;
        private Renderer targetRenderer;
        private Camera targetCamera;
        private string status = "Waiting for Tween service...";
        private bool wideFieldOfView;
        private bool moved;
        private bool scaled;
        private bool rotated;
        private bool grouped;

        private void OnEnable()
        {
            BootstrapperBehaviour.Bootstrapped += OnBootstrapped;
            EnsureTarget();
            ResolveIfReady();
        }

        private void OnDisable()
        {
            BootstrapperBehaviour.Bootstrapped -= OnBootstrapped;
            KillActive();
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

            status = "Tween service is ready.";
        }

        private void OnGUI()
        {
            var x = guiX;
            var y = guiY;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), status);
            y += labelRowSpacing;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), FormatHandleState());
            y += labelRowSpacing + buttonPairGap;

            GUI.Label(new Rect(x, y, labelWidth, labelHeight), $"Easing: {currentEasing}");
            y += labelRowSpacing;

            if (GUI.Button(new Rect(x, y, smallButtonWidth, buttonHeight), "Prev Ease"))
                SelectPreviousEasing();

            if (GUI.Button(new Rect(x + smallButtonWidth + buttonPairGap, y, smallButtonWidth, buttonHeight), "Next Ease"))
                SelectNextEasing();

            y += sectionSpacing;

            GUI.enabled = tweenService != null;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Move"))
                PlayMove();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Scale"))
                PlayScale();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Rotate + Shake"))
                PlayRotateAndShake();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Play Group"))
                PlayGroup();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Delayed Color"))
                PlayDelayedColor();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Camera FOV"))
                PlayCameraFieldOfView();

            y += sectionSpacing;

            GUI.enabled = activeHandle != null;

            if (GUI.Button(new Rect(x, y, smallButtonWidth, buttonHeight), "Pause"))
                activeHandle.Pause();

            if (GUI.Button(new Rect(x + smallButtonWidth + buttonPairGap, y, smallButtonWidth, buttonHeight), "Resume"))
                activeHandle.Resume();

            y += buttonRowSpacing;

            if (GUI.Button(new Rect(x, y, smallButtonWidth, buttonHeight), "Complete"))
                activeHandle.Complete();

            if (GUI.Button(new Rect(x + smallButtonWidth + buttonPairGap, y, smallButtonWidth, buttonHeight), "Kill"))
                activeHandle.Kill();

            y += sectionSpacing;

            GUI.enabled = true;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset Target"))
                ResetTarget();
        }

        private void PlayMove()
        {
            KillActive();

            moved = !moved;
            var destination = moved ? movePositivePosition : moveNegativePosition;

            activeHandle = tweenService.Play(
                target.DOMove(destination, moveDuration)
                    .SetEasing(currentEasing)
                    .AddOnCompleted(() => status = $"Move completed: {destination}"));

            status = "Move tween started.";
        }

        private void PlayScale()
        {
            KillActive();

            scaled = !scaled;
            var destination = scaled ? scaledTarget : initialScale;

            activeHandle = tweenService.Play(
                target.DOScale(destination, scaleDuration)
                    .SetEasing(currentEasing)
                    .AddOnCompleted(() => status = $"Scale completed: {destination}"));

            status = "Scale tween started.";
        }

        private void PlayRotateAndShake()
        {
            KillActive();

            rotated = !rotated;
            var destination = rotated ? rotationTarget : initialRotationEuler;

            activeHandle = tweenService.PlayGroup(
                target.DORotate(destination, rotateShakeDuration).SetEasing(currentEasing),
                target.DOShake(shakeMagnitude, rotateShakeDuration).SetEasing(currentEasing));

            activeHandle.Completed += () => status = $"Rotate + shake group completed: {destination}";
            status = "Rotate + shake group started.";
        }

        private void PlayGroup()
        {
            KillActive();

            grouped = !grouped;
            var position = grouped ? groupMovePosition : groupReturnPosition;
            var scale = grouped ? groupScale : groupReturnScale;
            var color = grouped ? groupColorA : groupColorB;

            activeHandle = tweenService.PlayGroup(
                target.DOMove(position, groupDuration).SetEasing(currentEasing),
                target.DOScale(scale, groupDuration).SetEasing(currentEasing),
                TweenCases.DOColor(targetRenderer.material.color, color, groupDuration, next => targetRenderer.material.color = next)
                    .SetEasing(currentEasing));

            activeHandle.Completed += () => status = $"Move + scale + color group completed: {position}, {scale:0.00}";
            status = "Grouped tweens started.";
        }

        private void PlayDelayedColor()
        {
            KillActive();

            var startColor = targetRenderer.material.color;
            var endColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            activeHandle = tweenService.Play(
                TweenCases.DelayedCall(
                    delayedColorDelay,
                    () =>
                    {
                        delayedColorHandle = tweenService.Play(
                            TweenCases.DOColor(startColor, endColor, delayedColorDuration, next => targetRenderer.material.color = next)
                                .SetEasing(currentEasing)
                                .AddOnCompleted(() => status = "Delayed color tween completed."));

                        activeHandle = delayedColorHandle;
                        status = "Delayed color tween started.";
                    }));

            activeHandle.Completed += () => status = "Delayed call fired.";
            status = "Delayed color call queued.";
        }

        private void PlayCameraFieldOfView()
        {
            KillActive();
            EnsureCamera();

            wideFieldOfView = !wideFieldOfView;
            var targetFieldOfView = wideFieldOfView ? wideFieldOfViewTarget : defaultFieldOfView;

            activeHandle = tweenService.Play(
                targetCamera.DOFieldOfView(targetFieldOfView, cameraFieldOfViewDuration)
                    .SetEasing(currentEasing)
                    .AddOnCompleted(() => status = $"Camera FOV completed: {targetFieldOfView:0}"));

            status = "Camera FOV tween started.";
        }

        private void SelectPreviousEasing()
        {
            var values = (EaseType[])Enum.GetValues(typeof(EaseType));
            var index = Array.IndexOf(values, currentEasing);
            currentEasing = values[(index - 1 + values.Length) % values.Length];
            status = $"Selected easing: {currentEasing}";
        }

        private void SelectNextEasing()
        {
            var values = (EaseType[])Enum.GetValues(typeof(EaseType));
            var index = Array.IndexOf(values, currentEasing);
            currentEasing = values[(index + 1) % values.Length];
            status = $"Selected easing: {currentEasing}";
        }

        private string FormatHandleState()
        {
            if (activeHandle == null)
                return "Active Handle: none";

            return $"Active: {activeHandle.IsActive}   Completed: {activeHandle.IsCompleted}   Killed: {activeHandle.IsKilled}";
        }

        private void KillActive()
        {
            if (delayedColorHandle != null && !delayedColorHandle.IsCompleted && !delayedColorHandle.IsKilled)
                delayedColorHandle.Kill();

            delayedColorHandle = null;

            activeHandle?.Kill();
            activeHandle = null;
        }

        private void ResetTarget()
        {
            KillActive();
            target.position = initialPosition;
            target.eulerAngles = initialRotationEuler;
            target.localScale = initialScale;
            targetRenderer.material.color = initialColor;
            EnsureCamera();
            targetCamera.fieldOfView = defaultFieldOfView;
            wideFieldOfView = false;
            moved = false;
            scaled = false;
            rotated = false;
            grouped = false;
            status = tweenService == null ? "Waiting for Tween service..." : "Target reset.";
        }

        private void EnsureTarget()
        {
            if (target != null)
                return;

            var targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetObject.name = targetObjectName;
            target = targetObject.transform;
            targetRenderer = targetObject.GetComponent<Renderer>();
            ResetTarget();
        }

        private void EnsureCamera()
        {
            if (targetCamera != null)
                return;

            targetCamera = Camera.main;

            if (targetCamera == null)
                targetCamera = FindObjectOfType<Camera>();

            if (targetCamera == null)
            {
                var cameraObject = new GameObject("Tween Sample Camera");
                targetCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            targetCamera.transform.position = Vector3.zero;
            targetCamera.transform.rotation = Quaternion.identity;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = cameraBackgroundColor;
        }
    }
}
