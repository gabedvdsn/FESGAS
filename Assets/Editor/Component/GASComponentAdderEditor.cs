using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class GASComponentAdderEditor
    {
        [MenuItem("Tools/FESGAS/Add GAS Components", false, 0)]
        private static void AddComprehensiveGASComponents()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning($"Cannot add GAS components: No GameObject selected.");
                return;
            }

            GameObject go = Selection.activeGameObject;

            if (go.GetComponent<GASComponent>() is not null)
            {
                Debug.LogWarning($"GameObject ({go.name}) already contains GAS components.");
                return;
            }

            Undo.AddComponent<GASComponent>(go);
        }
        
        // Disable menu item if no GameObject is selected
        [MenuItem("GAS/Add Components", true)]
        private static bool ValidateAddComprehensiveGAS()
        {
            return Selection.activeGameObject != null && !Selection.activeGameObject.GetComponent<GASComponent>();
        }
    }
}
