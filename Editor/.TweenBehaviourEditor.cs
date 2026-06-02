using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework;

namespace SWeetJelly.Tween.Editor
{
    [CustomEditor(typeof(TweenBehaviour))]
    public sealed class TweenBehaviourEditor : UnityEditor.Editor
    {
        private readonly string[] tweenTabs = { "Update", "Fixed", "Late" };
        private readonly UpdateMethod[] updateMethods =
        {
            UpdateMethod.Update,
            UpdateMethod.FixedUpdate,
            UpdateMethod.LateUpdate
        };

        private readonly List<TweenCase> activeTweens = new();

        private TweenBehaviour tweenBehaviour;
        private MonoScript script;

        private int tabId;
        private bool showActiveTweens;

        private void OnEnable()
        {
            tweenBehaviour = (TweenBehaviour)target;
            script = MonoScript.FromMonoBehaviour(tweenBehaviour);
        }

        public override void OnInspectorGUI()
        {
            DrawScriptField();
            DrawDefaultProperties();

            EditorGUILayout.Space(8f);

            DrawRuntimeTweenDebugger();
        }

        private void DrawScriptField()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Script",
                    script,
                    typeof(MonoScript),
                    false);
            }
        }

        private void DrawDefaultProperties()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeTweenDebugger()
        {
            showActiveTweens = EditorGUILayout.Foldout(
                showActiveTweens,
                "Show Active Tweens",
                true);

            if (!showActiveTweens)
                return;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Active tweens are only available in Play Mode.",
                    MessageType.Info);
                return;
            }

            tabId = GUILayout.Toolbar(tabId, tweenTabs);

            UpdateMethod updateMethod = updateMethods[tabId];

            tweenBehaviour.CopyActiveTweens(updateMethod, activeTweens);

            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    "Active",
                    tweenBehaviour.GetActiveTweenCount(updateMethod).ToString());

                EditorGUILayout.LabelField(
                    "Max Used",
                    tweenBehaviour.GetMaxUsedTweenCount(updateMethod).ToString());
            }

            EditorGUILayout.Space(4f);

            if (activeTweens.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"No active {updateMethod} tweens.",
                    MessageType.None);
                return;
            }

            for (int i = 0; i < activeTweens.Count; i++)
            {
                DrawTweenRow(i, activeTweens[i]);
            }

            Repaint();
        }

        private void DrawTweenRow(int index, TweenCase tweenCase)
        {
            if (tweenCase == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        $"{index}: {tweenCase.GetType().Name}",
                        EditorStyles.boldLabel);

                    if (GUILayout.Button("Complete", GUILayout.Width(80f)))
                    {
                        tweenCase.CompleteImmediately();
                        return;
                    }

                    if (GUILayout.Button("Kill", GUILayout.Width(50f)))
                    {
                        tweenCase.Kill();
                        return;
                    }
                }

                EditorGUILayout.LabelField("State", tweenCase.State.ToString("0.000"));
                EditorGUILayout.LabelField("Duration", tweenCase.Duration.ToString("0.000"));
                EditorGUILayout.LabelField("Delay", $"{tweenCase.CurrentDelay:0.000} / {tweenCase.Delay:0.000}");
                EditorGUILayout.LabelField("Paused", tweenCase.IsPaused.ToString());
                EditorGUILayout.LabelField("Completed", tweenCase.IsCompleted.ToString());
                EditorGUILayout.LabelField("Killing", tweenCase.IsKilling.ToString());
            }
        }
    }
}