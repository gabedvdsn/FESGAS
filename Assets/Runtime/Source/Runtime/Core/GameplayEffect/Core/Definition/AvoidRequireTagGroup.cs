using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AvoidRequireTagGroup
    {
        public List<GameplayTagScriptableObject> AvoidTags;
        public List<GameplayTagScriptableObject> RequireTags;

        private AvoidRequireTagGroup()
        {
            AvoidTags = new List<GameplayTagScriptableObject>();
            RequireTags = new List<GameplayTagScriptableObject>();
        }
        
        public AvoidRequireTagGroup(List<GameplayTagScriptableObject> avoidTags, List<GameplayTagScriptableObject> requireTags)
        {
            AvoidTags = avoidTags;
            RequireTags = requireTags;
        }

        public bool Validate(List<GameplayTagScriptableObject> appliedTags)
        {
            if (AvoidTags.Count == 0 && RequireTags.Count == 0) return true;
            return !AvoidTags.Any(appliedTags.Contains) && RequireTags.All(appliedTags.Contains);
        }

        public static AvoidRequireTagGroup GenerateEmpty()
        {
            return new AvoidRequireTagGroup();
        }
    }
}
