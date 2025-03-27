using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AbstractApplicationWorkerScriptableObject), true)]
    public class ApplicationWorkerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AbstractApplicationWorkerScriptableObject script = (AbstractApplicationWorkerScriptableObject)target;
            
            EditorGUILayout.LabelField($"Task Description");
            
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };
            
            GUI.enabled = false;
            EditorGUILayout.TextArea(script.ReadOnlyDescription, wrapStyle);
            GUI.enabled = true;
            
            EditorGUILayout.Space(10);

            DrawDefaultInspector();
        }
    }
}
