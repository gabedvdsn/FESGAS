using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AvoidRequireTagGroup
    {
        public GameplayTagScriptableObject[] AvoidTags;
        public GameplayTagScriptableObject[] RequireTags;

        public bool Validate(List<GameplayTagScriptableObject> appliedTags)
        {
            return !AvoidTags.Any(appliedTags.Contains) && RequireTags.All(appliedTags.Contains);
        }
    }
}
