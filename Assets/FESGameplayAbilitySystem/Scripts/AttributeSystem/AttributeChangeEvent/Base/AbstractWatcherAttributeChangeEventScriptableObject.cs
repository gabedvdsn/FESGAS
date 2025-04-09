using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractWatcherAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        protected override void InternalValidate()
        {
            // Watchers always run post attribute change
            Timing = EChangeEventTiming.PostChange;
        }
    }

    public class AttributeWatcher
    {
        
    }

    public class AttributeWatcherCache
    {
        
    }
}
