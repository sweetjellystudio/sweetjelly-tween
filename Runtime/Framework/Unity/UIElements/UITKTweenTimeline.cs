using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;
using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>One immutable captured trajectory. All target access requires Unity's main thread.</summary>
    public sealed class UITKTweenTimeline
    {
        internal readonly UITKTweenPlayer Owner;
        internal readonly Dictionary<UITKWriterKey, UITKPropertySnapshot> Properties = new();
        private readonly UITKPreparedNode graph;
        private readonly HashSet<UITKTweenNode> visited = new();
        private readonly int generation;
        public float Duration => graph.Duration;
        public float Position => Owner.IsCurrent(this) ? Owner.Position : 0f;

        internal UITKTweenTimeline(UITKTweenPlayer owner, UITKTweenNode root, int generation)
        {
            Owner = owner;
            this.generation = generation;
            var values = new Dictionary<UITKWriterKey, Vector2>();
            graph = Build(root, values);
        }

        public UITKTweenControl TakeControl() => Owner.TakeControl(this);

        /// <summary>Cancel this owner's execution and restore original inline declarations. Capture is retained.</summary>
        public void Restore() => Owner.Restore(this);

        internal void Validate()
        {
            Owner.ValidateBinding(generation);
            foreach (var property in Properties.Values)
                if (!Owner.Root.Contains(property.Element) && !ReferenceEquals(Owner.Root, property.Element))
                    throw new InvalidOperationException("A prepared target left its bound root. Bind and prepare again.");
        }

        internal void Apply(float time)
        {
            Validate();
            var values = new Dictionary<UITKWriterKey, Vector2>();
            foreach (var property in Properties) values.Add(property.Key, property.Value.Origin);
            graph.Evaluate(TweenTimelineTiming.ClampTime(time, Duration), values);
            // Evaluate every value before making any target writes.
            foreach (var value in values) UITKPropertySnapshot.RequireFinite(value.Value);
            foreach (var value in values) Properties[value.Key].Write(value.Value);
        }

        internal void RestoreValues()
        {
            Validate();
            foreach (var property in Properties.Values) property.Restore();
        }

        private UITKPreparedNode Build(UITKTweenNode source, Dictionary<UITKWriterKey, Vector2> values)
        {
            if (source == null) throw new InvalidOperationException("The graph contains a missing node.");
            if (!visited.Add(source)) throw new NotSupportedException("Cycles and shared node instances are not supported.");
            if (!Enum.IsDefined(typeof(UITKTweenNodeKind), source.Kind)) throw new NotSupportedException("Unknown node kind.");
            var node = new UITKPreparedNode();
            if (source.Kind != UITKTweenNodeKind.Property)
            {
                if (source.Children == null) throw new InvalidOperationException("A composite has no child list.");
                bool sequence = source.Kind == UITKTweenNodeKind.Sequence;
                var children = new List<UITKPreparedNode>();
                foreach (var child in source.Children)
                {
                    var input = sequence ? values : new Dictionary<UITKWriterKey, Vector2>(values);
                    var prepared = Build(child, input);
                    if (!sequence && node.Keys.Overlaps(prepared.Keys))
                        throw new NotSupportedException("Parallel nodes write the same target/property.");
                    if (!sequence)
                        foreach (var childKey in prepared.Keys) values[childKey] = input[childKey];
                    children.Add(prepared);
                    node.Keys.UnionWith(prepared.Keys);
                    node.Duration = TweenTimelineTiming.Combine(node.Duration, prepared.Duration, sequence);
                }
                node.Evaluate = (time, output) =>
                {
                    float offset = 0f;
                    foreach (var child in children)
                    {
                        if (sequence && time < offset) break;
                        child.Evaluate(TweenTimelineTiming.ChildTime(time, offset, child.Duration), output);
                        if (sequence) offset += child.Duration;
                    }
                };
                return node;
            }

            if (!Enum.IsDefined(typeof(UITKTweenProperty), source.Property) ||
                !Enum.IsDefined(typeof(UITKTweenLoop), source.Loop) || !Enum.IsDefined(typeof(EaseType), source.Easing))
                throw new NotSupportedException("Unknown property, loop or easing.");
            if (source.Children != null && source.Children.Count != 0)
                throw new InvalidOperationException("A property node cannot contain children.");
            var element = Resolve(source.TargetName);
            var key = new UITKWriterKey(element, source.Property);
            if (!Properties.TryGetValue(key, out var snapshot))
            {
                snapshot = new UITKPropertySnapshot(element, source.Property);
                Properties.Add(key, snapshot);
            }
            if (!values.ContainsKey(key)) values.Add(key, snapshot.Origin);
            Vector2 from = source.FromCurrent ? values[key] : source.From;
            Vector2 to = source.To;
            UITKPropertySnapshot.RequireFinite(from); UITKPropertySnapshot.RequireFinite(to);
            float duration = TweenTiming.Duration(source.Duration), delay = TweenTiming.Delay(source.Delay);
            int count = source.Repetitions;
            node.Duration = TweenTimelineTiming.Length(duration, delay, count);
            var loop = source.Loop;
            IEasingFunction easing;
            if (source.UseCurve)
            {
                if (source.Curve == null || source.Curve.length == 0) throw new InvalidOperationException("The custom curve is empty.");
                foreach (var point in source.Curve.keys)
                {
                    UITKPropertySnapshot.RequireFinite(new Vector2(point.time, point.value));
                    if (float.IsNaN(point.inTangent) || float.IsNaN(point.outTangent))
                        throw new ArgumentException("Curve tangents must not be NaN.");
                }
                easing = new AnimationCurveEasingFunction(new AnimationCurve(source.Curve.keys)
                    { preWrapMode = source.Curve.preWrapMode, postWrapMode = source.Curve.postWrapMode });
            }
            else easing = EasingFunctions.Get(source.Easing);
            Vector2 Pose(float time)
            {
                TweenTimelineTiming.Locate(time, duration, delay, count, out int cycle, out float progress);
                Vector2 a = from, b = to;
                if (loop == UITKTweenLoop.Yoyo && (cycle & 1) != 0) { a = to; b = from; }
                else if (loop == UITKTweenLoop.Increment) { a = from + (to - from) * cycle; b = from + (to - from) * (cycle + 1f); }
                Vector2 result = time < delay ? from : progress >= 1f ? b : Vector2.LerpUnclamped(a, b, easing.Evaluate(progress));
                UITKPropertySnapshot.RequireFinite(result);
                if (key.Property == UITKTweenProperty.Opacity) result.x = Mathf.Clamp01(result.x);
                return result;
            }
            node.Keys.Add(key);
            node.Evaluate = (time, output) => output[key] = Pose(time);
            values[key] = Pose(node.Duration);
            return node;
        }

        private VisualElement Resolve(string name)
        {
            if (string.IsNullOrEmpty(name)) return Owner.Root;
            VisualElement found = null;
            int count = 0;
            void Visit(VisualElement element)
            {
                if (element.name == name) { count++; found = element; }
                foreach (var child in element.Children()) Visit(child);
            }
            Visit(Owner.Root);
            if (count != 1) throw new InvalidOperationException($"Target '{name}' must occur once inside the supplied root; found {count}. Check missing, renamed or duplicate names.");
            return found;
        }
    }

    internal readonly struct UITKWriterKey : IEquatable<UITKWriterKey>
    {
        internal readonly VisualElement Element;
        internal readonly UITKTweenProperty Property;
        internal UITKWriterKey(VisualElement element, UITKTweenProperty property) { Element = element; Property = property; }
        public bool Equals(UITKWriterKey other) => ReferenceEquals(Element, other.Element) && Property == other.Property;
        public override bool Equals(object other) => other is UITKWriterKey key && Equals(key);
        public override int GetHashCode() => unchecked(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Element) * 397 ^ (int)Property);
    }

    internal sealed class UITKPreparedNode
    {
        internal float Duration;
        internal readonly HashSet<UITKWriterKey> Keys = new();
        internal Action<float, Dictionary<UITKWriterKey, Vector2>> Evaluate;
    }

    internal sealed class UITKPropertySnapshot
    {
        internal readonly VisualElement Element;
        internal readonly Vector2 Origin;
        private readonly UITKTweenProperty property;
        private readonly StyleFloat opacity;
        private readonly StyleTranslate translate;
        private readonly StyleScale scale;
        private readonly float z;

        internal UITKPropertySnapshot(VisualElement element, UITKTweenProperty property)
        {
            Element = element; this.property = property;
            opacity = element.style.opacity; translate = element.style.translate; scale = element.style.scale;
            if (property == UITKTweenProperty.Opacity)
                Origin = new Vector2(opacity.keyword == StyleKeyword.Undefined ? opacity.value : element.resolvedStyle.opacity, 0);
            else if (property == UITKTweenProperty.Scale)
            {
                var value = (scale.keyword == StyleKeyword.Undefined ? scale.value : element.resolvedStyle.scale).value;
                Origin = new Vector2(value.x, value.y); z = value.z;
            }
            else
            {
                var value = translate.keyword == StyleKeyword.Undefined ? translate.value : element.resolvedStyle.translate;
                if (value.x.unit != LengthUnit.Pixel || value.y.unit != LengthUnit.Pixel)
                    throw new NotSupportedException("Translate capture requires pixel values; percentage translation is not supported.");
                Origin = new Vector2(value.x.value, value.y.value); z = value.z;
            }
            RequireFinite(Origin);
        }

        internal static void RequireFinite(Vector2 value)
        {
            if (float.IsNaN(value.x) || float.IsInfinity(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.y))
                throw new ArgumentException("Tween values must be finite.");
        }
        internal void Write(Vector2 value)
        {
            if (property == UITKTweenProperty.Opacity) Element.style.opacity = Mathf.Clamp01(value.x);
            else if (property == UITKTweenProperty.Scale) Element.style.scale = new Scale(new Vector3(value.x, value.y, z));
            else Element.style.translate = new Translate(value.x, value.y, z);
        }
        internal void Restore()
        {
            if (property == UITKTweenProperty.Opacity) Element.style.opacity = opacity;
            else if (property == UITKTweenProperty.Scale) Element.style.scale = scale;
            else Element.style.translate = translate;
        }
    }
}
