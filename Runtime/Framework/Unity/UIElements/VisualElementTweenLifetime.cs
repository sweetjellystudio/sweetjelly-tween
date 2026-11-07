using System;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    internal sealed class VisualElementTweenLifetime
    {
        private readonly VisualElement target;
        private readonly TweenCase owner;
        private IPanel panel;
        private bool subscribed;

        internal VisualElementTweenLifetime(VisualElement target, TweenCase owner)
        {
            RequirePanel(target);
            this.target = target;
            this.owner = owner;
            owner.Completed += Release;
            owner.Killed += Release;
        }

        internal static void RequirePanel(VisualElement target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.panel == null) throw new InvalidOperationException("Attach the VisualElement before creating or playing its tween.");
        }

        internal static void RequireFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(name);
        }

        internal void Activate()
        {
            Release();
            if (target.panel == null)
            {
                owner.Kill();
                throw new InvalidOperationException("The VisualElement is detached.");
            }
            panel = target.panel;
            target.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            subscribed = true;
        }

        internal bool Validate() => subscribed && target.panel != null && ReferenceEquals(panel, target.panel);
        internal bool CanWrite()
        {
            if (Validate()) return true;
            owner.Kill();
            return false;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, target)) owner.Kill();
        }

        private void Release()
        {
            if (subscribed) target.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            subscribed = false;
            panel = null;
        }
    }
}
