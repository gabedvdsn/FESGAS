using System.Collections.Generic;

namespace FESGameplayAbilitySystem.Gasify
{
    public abstract class GasifyDataNode
    {
        public string Id;

        public bool hasEditorTags => editorTags.Count > 0;
        public Dictionary<string, object> editorTags = new();
    }
    
    public class AttributeData : GasifyDataNode
    {
        public string Name;
        public string Description;
    }

    public class TagData : GasifyDataNode
    {
        public string Name;
        public string Parent;
    }

    public class AbilityData : GasifyDataNode
    {
        
    }

    public class ProxyTaskData : GasifyDataNode
    {
        
    }
    
    public class EffectData : GasifyDataNode
    {
        
    }

    public class GASData : GasifyDataNode
    {
        
    }

    public class AttributeSetData : GasifyDataNode
    {
        
    }

    public class ModifierData : GasifyDataNode
    {
        
    }

    public class AttributeEventData : GasifyDataNode
    {
        
    }
    
    public class ImpactWorkerData : GasifyDataNode
    {
        
    }

    public class EffectWorkerData : GasifyDataNode
    {
        
    }

    public class TagWorkerData : GasifyDataNode
    {
        
    }

    public class ProcessInstantiatorData : GasifyDataNode
    {
        
    }
}
