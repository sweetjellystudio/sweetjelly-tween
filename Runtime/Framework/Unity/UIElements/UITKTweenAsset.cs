using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    public enum UITKTweenNodeKind { Property, Sequence, Group }
    public enum UITKTweenProperty { Opacity, Translate, Scale }
    public enum UITKTweenLoop { Repeat, Yoyo, Increment }

    /// <summary>Authored data only. An empty target name selects the supplied root.</summary>
    [Serializable]
    public sealed class UITKTweenNode
    {
        public string Label = "Tween";
        public UITKTweenNodeKind Kind;
        public string TargetName = "";
        public UITKTweenProperty Property;
        public bool FromCurrent = true;
        public Vector2 From;
        public Vector2 To = Vector2.one;
        public float Duration = .3f;
        public float Delay;
        [Min(1)] public int Repetitions = 1;
        public UITKTweenLoop Loop;
        public EaseType Easing;
        public bool UseCurve;
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeReference] public List<UITKTweenNode> Children = new();
    }

    [CreateAssetMenu(menuName = "SWeetJelly/Tween/UI Toolkit Tween", fileName = "UITKTween")]
    public sealed class UITKTweenAsset : ScriptableObject
    {
        public VisualTreeAsset AuthoringDocument;
        public UpdateMethod UpdateMethod = UpdateMethod.Update;
        public bool UnscaledTime;
        [SerializeReference] public UITKTweenNode Root = new() { Kind = UITKTweenNodeKind.Sequence, Label = "Sequence" };

        /// <summary>Bind after creating the consumer's tree. No values are captured or written here.</summary>
        public UITKTweenPlayer Bind(VisualElement root, ITweenService service,
            UpdateMethod? updateMethod = null, bool? unscaledTime = null) =>
            new(this, root, service, updateMethod ?? UpdateMethod, unscaledTime ?? UnscaledTime);
    }
}
