using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AbstractImpactWorkerScriptableObject), true)]
    public class ImpactWorkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AbstractImpactWorkerScriptableObject script = (AbstractImpactWorkerScriptableObject)target;
            
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
