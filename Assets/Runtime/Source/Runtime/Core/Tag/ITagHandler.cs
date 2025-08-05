using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ITagHandler
    {
        public List<ITag> GetAppliedTags();
        public int GetWeight(ITag _tag);
    }
}
