using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectRequirements
    {
        public AvoidRequireTagGroup ApplicationRequirements;  // These tags are required to apply the effect
        public AvoidRequireTagGroup OngoingRequirements;  // These tags are required to keep the effect ongoing
        public AvoidRequireTagGroup RemovalRequirements;  // These tags are required to purge the effect

        public bool ValidateApplicationRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            return ApplicationRequirements.Validate(appliedTags);
        }

        public bool ValidateOngoingRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            return OngoingRequirements.Validate(appliedTags);
        }
        
        public bool ValidateRemovalRequirements(List<GameplayTagScriptableObject> appliedTags)
        {
            return RemovalRequirements.Validate(appliedTags);
        }
    }
}
