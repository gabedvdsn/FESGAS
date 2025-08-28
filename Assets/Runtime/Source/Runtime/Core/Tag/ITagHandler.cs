using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ITagHandler
    {
        public Tag[] GetAppliedTags();
        public int GetWeight(Tag _tag);
    }
}
