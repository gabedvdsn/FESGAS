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
                    EditorGUILayout.LabelField($"Queued: {relay.QueuedState}");
                    EditorGUILayout.LabelField($"Update Time: {relay.Lifetime:F2} seconds");
                    EditorGUILayout.LabelField($"Timing: {relay.Process.StepTiming}");
                    EditorGUILayout.LabelField($"Lifecycle: {relay.Process.Lifecycle}");
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Wait"))
                    {
                        ProcessControl.Instance.Wait(relay.CacheIndex);
                    }
                    if (GUILayout.Button("Pause"))
                    {
                        ProcessControl.Instance.Pause(relay.CacheIndex);
                    }
                    if (GUILayout.Button("Run"))
                    {
                        ProcessControl.Instance.Run(relay.CacheIndex);
                    }
                    if (GUILayout.Button("Terminate"))
                    {
                        ProcessControl.Instance.Terminate(relay.CacheIndex);
                    }
                    EditorGUILayout.EndHorizontal();
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
