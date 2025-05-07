using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class GASComponentAdderEditor
    {
        [MenuItem("GAS/Add Components", false, 0)]
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
            return Selection.activeGameObject != null;
        }
        
        [MenuItem("GAS/Add Components (Manual)", false, 0)]
        private static void AddManualGASComponents()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning($"Cannot add GAS components: No GameObject selected.");
                return;
            }

            GameObject go = Selection.activeGameObject;

            if (go.GetComponent<GASComponentManual>() is not null)
            {
                Debug.LogWarning($"GameObject ({go.name}) already contains GAS components.");
                return;
            }

            Undo.AddComponent<GASComponentManual>(go);
        }
        
        [MenuItem("GAS/Add Components (Manual)", true)]
        private static bool ValidateAddManualGAS()
        {
            return Selection.activeGameObject != null;
        }
    }
}
