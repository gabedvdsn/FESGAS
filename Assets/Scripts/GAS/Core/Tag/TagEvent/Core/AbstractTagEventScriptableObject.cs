using UnityEngine;

namespace FESGameplayAbilitySystem.TagEvent.Core
{
    public abstract class AbstractTagEventScriptableObject : ScriptableObject
    {
        public GameplayTagScriptableObject Tag;
        
        protected abstract void OnTagApplied(GASComponent component);
        protected abstract void WhileTagApplied(GASComponent component);
        protected abstract void OnTagRemoved(GASComponent component);

        private abstract class AbstractTagEventContainer
        {
            private AbstractTagEventScriptableObject Base;
            
            public void OnTagApplied(GASComponent component) => Base.OnTagApplied(component);
            public void WhileTagApplied(GASComponent component) => Base.WhileTagApplied(component);
            public void OnTagRemoved(GASComponent component) => Base.OnTagRemoved(component);
        }
    }
    
    
}
