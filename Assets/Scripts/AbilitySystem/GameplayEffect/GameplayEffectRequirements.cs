using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Requirements")]
    public class GameplayEffectRequirements : ScriptableObject
    {
        public AvoidRequireTagGroup ApplicationRequirements;  // These tags are required to apply the effect
        public AvoidRequireTagGroup OngoingRequirements;  // These tags are required to keep the effect ongoing
        public AvoidRequireTagGroup RemovalRequirements;  // These tags are required to remove the effect

        public bool CheckApplicationRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            return !ApplicationRequirements.AvoidTags.Any(appliedTags.Contains) && ApplicationRequirements.RequireTags.All(appliedTags.Contains);
        }

        public bool CheckOngoingRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            if (OngoingRequirements.AvoidTags.Length == 0)
            {
                return OngoingRequirements.RequireTags.Length == 0 || OngoingRequirements.RequireTags.All(appliedTags.Contains);
            }

            return !OngoingRequirements.AvoidTags.Any(appliedTags.Contains) &&
                   (OngoingRequirements.RequireTags.Length == 0 || OngoingRequirements.RequireTags.All(appliedTags.Contains));
        }
        
        public bool CheckRemovalRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            return RemovalRequirements.AvoidTags.Any(appliedTags.Contains) 
                   || RemovalRequirements.RequireTags.Length != 0 && RemovalRequirements.RequireTags.All(appliedTags.Contains);
        }
    }
}
