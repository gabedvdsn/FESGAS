using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CustomEditor(typeof(GASComponentManual))]
    public class GASComponentManualEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Delete GAS Components"))
            {
                GASComponentManual gasObj = (GASComponentManual)target;
                GameObject obj = gasObj.gameObject;
                DestroyImmediate(obj.GetComponent<GASComponentManual>());
                DestroyImmediate(obj.GetComponent<AbilitySystemComponent>());
                DestroyImmediate(obj.GetComponent<AttributeSystemComponent>());
            }
        }
    }
}
