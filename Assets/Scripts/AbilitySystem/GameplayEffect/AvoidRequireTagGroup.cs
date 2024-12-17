using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AvoidRequireTagGroup
    {
        public List<GameplayTagScriptableObject> AvoidTags;
        public List<GameplayTagScriptableObject> RequireTags;
    }
}
