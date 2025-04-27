#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class ProcessControlDebugger : EditorWindow
    {
        [MenuItem("FESGameplayAbilitySystem/Process Debugger")]
        public static void ShowWindow()
        {
            GetWindow<ProcessControlDebugger>("Process Debugger");
        }

        private void OnGUI()
        {
            if (ProcessControl.Instance == null)
            {
                EditorGUILayout.LabelField("ProcessControl not found.");
                return;
            }

            EditorGUILayout.LabelField($"State: {ProcessControl.Instance.State}");
            EditorGUILayout.Space();

            try
            {
                foreach (var kvp in ProcessControl.Instance.FetchActiveProcesses())
                {
                    var relay = kvp.Value.GetRelay();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Process ID: {relay.CacheIndex}");
                    EditorGUILayout.LabelField($"State: {relay.State}");
                    EditorGUILayout.LabelField($"Update Time: {relay.Lifetime:F2} seconds");
                    EditorGUILayout.LabelField($"Timing: {relay.Process.StepTiming}");
                    EditorGUILayout.LabelField($"Lifecycle: {relay.Process.Lifecycle}");
                    if (GUILayout.Button("Force Terminate"))
                    {
                        ProcessControl.Instance.TerminateImmediate(relay.CacheIndex);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            catch (InvalidOperationException) { }
        }
        
        private void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}
#endif
