using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AbstractAbilityProxyTaskScriptableObject), true)]
    public class ProxyTaskEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AbstractAbilityProxyTaskScriptableObject script = (AbstractAbilityProxyTaskScriptableObject)target;
            
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
