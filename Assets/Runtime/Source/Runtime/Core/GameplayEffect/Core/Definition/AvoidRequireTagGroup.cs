using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AvoidRequireTagGroup
    {
        public Tag[] AvoidTags;
        public Tag[] RequireTags;

        private AvoidRequireTagGroup()
        {
            AvoidTags = Array.Empty<Tag>();
            RequireTags = Array.Empty<Tag>();
        }
        
        public AvoidRequireTagGroup(Tag[] avoidTags, Tag[] requireTags)
        {
            AvoidTags = avoidTags;
            RequireTags = requireTags;
        }

        public bool Validate(Tag[] appliedTags)
        {
            if (AvoidTags.Length == 0 && RequireTags.Length == 0) return true;
            return !AvoidTags.Any(appliedTags.Contains) && RequireTags.All(appliedTags.Contains);
        }

        public static AvoidRequireTagGroup GenerateEmpty()
        {
            return new AvoidRequireTagGroup();
        }
    }
}
