using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CustomEditor(typeof(GASComponent))]
    public class GASComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Delete GAS Components"))
            {
                GASComponent gasObj = (GASComponent)target;
                GameObject obj = gasObj.gameObject;
                DestroyImmediate(obj.GetComponent<GASComponent>());
                DestroyImmediate(obj.GetComponent<AbilitySystemComponent>());
                DestroyImmediate(obj.GetComponent<AttributeSystemComponent>());
            }
        }
    }
}
