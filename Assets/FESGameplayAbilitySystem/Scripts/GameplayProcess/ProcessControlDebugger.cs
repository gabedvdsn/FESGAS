#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class ProcessControlDebugger : EditorWindow
    {
        bool showProcessControls = true;
        
        [MenuItem("FESGameplayAbilitySystem/Process Debugger")]
        public static void ShowWindow()
        {
            GetWindow<ProcessControlDebugger>("Process Debugger");
        }

        private void OnGUI()
        {
            if (ProcessControl.Instance is null)
            {
                EditorGUILayout.LabelField("ProcessControl not found.");
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"State: {ProcessControl.Instance.State}  |  {ProcessControl.Instance.Active}/{ProcessControl.Instance.Created}");

            showProcessControls = EditorGUILayout.Toggle("Show Process Controls", showProcessControls);
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.Ready);
            if (GUILayout.Button("Ready"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.Ready);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.Closed);
            if (GUILayout.Button("Closed"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.Closed);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.Waiting);
            if (GUILayout.Button("Waiting"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.Waiting);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.ClosedWaiting);
            if (GUILayout.Button("ClosedWaiting"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.ClosedWaiting);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.Terminated);
            if (GUILayout.Button("Terminated"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.Terminated);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State == EProcessControlState.TerminatedImmediately);
            if (GUILayout.Button("TerminatedImmediately"))
            {
                ProcessControl.Instance.SetControlState(EProcessControlState.TerminatedImmediately);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();

            if (showProcessControls) ShowFull();
            else ShowSimple();
        }

        private void ShowSimple()
        {
            try
            {
                foreach (var kvp in ProcessControl.Instance.FetchActiveProcesses())
                {
                    var relay = kvp.Value.GetRelay();
                    if (relay.Process is null) continue;

                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField($"{relay.Process.ProcessName}");
                    EditorGUILayout.LabelField($"ID: {relay.CacheIndex} | {relay.Process.Lifecycle} | {relay.Process.StepTiming}");
                    
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField($"State: {relay.State}");
                    EditorGUILayout.LabelField($"Queued: {relay.QueuedState}");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField($"Runtime: {relay.Runtime:F2} seconds");
                    EditorGUILayout.LabelField($"Lifetime: {relay.UnscaledLifetime:F2} seconds");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                }
            }
            catch (InvalidOperationException) { }
        }

        private void ShowFull()
        {
            try
            {
                foreach (var kvp in ProcessControl.Instance.FetchActiveProcesses())
                {
                    var relay = kvp.Value.GetRelay();
                    if (relay.Process is null) continue;

                    EditorGUILayout.BeginVertical("box");
                    
                    EditorGUILayout.LabelField($"{relay.Process.ProcessName}");
                    EditorGUILayout.LabelField($"ID: {relay.CacheIndex} | {relay.Process.Lifecycle} | {relay.Process.StepTiming}");
                    
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField($"State: {relay.State}");
                    EditorGUILayout.LabelField($"Queued: {relay.QueuedState}");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField($"Runtime: {relay.Runtime:F2} seconds");
                    EditorGUILayout.LabelField($"Lifetime: {relay.UnscaledLifetime:F2} seconds");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUI.BeginDisabledGroup(relay.State == EProcessState.Waiting || relay.QueuedState == EProcessState.Waiting);
                    if (GUILayout.Button("Wait"))
                    {
                        ProcessControl.Instance.Wait(relay.CacheIndex);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(ProcessControl.Instance.State != EProcessControlState.Ready && ProcessControl.Instance.State != EProcessControlState.Closed || relay.State is EProcessState.Running or EProcessState.Terminated || relay.Process.Lifecycle == EProcessLifecycle.SelfTerminating);
                    if (GUILayout.Button("Run"))
                    {
                        ProcessControl.Instance.Run(relay.CacheIndex);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(relay.State == EProcessState.Terminated || relay.QueuedState == EProcessState.Terminated);
                    if (GUILayout.Button("Terminate"))
                    {
                        ProcessControl.Instance.Terminate(relay.CacheIndex);
                    }
                    EditorGUI.EndDisabledGroup();
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
