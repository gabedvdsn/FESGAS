using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem.Gasify
{
    /// <summary>
    /// Editor tags:
    /// - no_edit: bool => Cannot be edited
    /// - built: bool => Data object is built into SO
    /// - requires_rebuild: bool => Data object need has changes and needs to be rebuilt
    /// - missing_refs: bool => Referenced materials are missing
    /// - search_open_in_home: bool
    /// - search_open_in_creator: bool
    /// - search_open_in_developer: bool
    /// </summary>
    
    [Serializable]
    public class FrameworkProject
    {
        public bool Loaded;
        
        public string Version;
        public string MetaName;
        public string MetaAuthor;

        public List<AttributeData> Attributes = new();
        public List<TagData> Tags = new();
        public List<AbilityData> Abilities = new();
        public List<ProxyTaskData> ProxyTasks = new();
        public List<EffectData> Effects = new();
        public List<GASData> GAS = new();
        public List<AttributeSetData> AttributeSets = new();
        public List<ModifierData> Modifiers = new();
        public List<AttributeEventData> AttributeEvents = new();
        public List<ImpactWorkerData> ImpactWorkers = new();
        public List<EffectWorkerData> EffectWorkers = new();
        public List<TagWorkerData> TagWorkers = new();
        public List<ProcessInstantiatorData> ProcessInstantiators = new();
    }
    
    [Serializable]
    public abstract class GasifyDataNode
    {
        public string Id;

        public bool hasEditorTags => editorTags.Count > 0;
        public Dictionary<Tag, object> editorTags = new();
    }
    
    [Serializable]
    public abstract class DescriptiveDataNode : GasifyDataNode
    {
        public string Name;
        public string Description;
    }
    
    [Serializable]
    public class AttributeData : DescriptiveDataNode
    {
        
    }
    
    [Serializable]
    public class TagData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class AbilityData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class ProxyTaskData : DescriptiveDataNode
    {
        
    }
    
    [Serializable]
    public class EffectData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class GASData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class AttributeSetData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class ModifierData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class AttributeEventData : DescriptiveDataNode
    {
        
    }
    
    [Serializable]
    public class ImpactWorkerData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class EffectWorkerData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class TagWorkerData : DescriptiveDataNode
    {
        
    }

    [Serializable]
    public class ProcessInstantiatorData : DescriptiveDataNode
    {
        
    }
}
