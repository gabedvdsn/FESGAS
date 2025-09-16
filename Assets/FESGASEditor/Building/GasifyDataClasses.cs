using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<AbilityData> Abilities = new();
        public List<AttributeData> Attributes = new();
        public List<TagData> Tags = new();
        public List<ProxyTaskData> ProxyTasks = new();
        public List<EffectData> Effects = new();
        public List<EntityData> Entities = new();
        public List<AttributeSetData> AttributeSets = new();
        public List<ModifierData> Modifiers = new();
        public List<AttributeEventData> AttributeEvents = new();
        public List<ImpactWorkerData> ImpactWorkers = new();
        public List<EffectWorkerData> EffectWorkers = new();
        public List<TagWorkerData> TagWorkers = new();
        public List<ProcessInstantiatorData> ProcessInstantiators = new();

        public Dictionary<GasifyEditorWindow.DataType, List<(string, string)>> GetCompleteDescriptions()
        {
            var items = new Dictionary<GasifyEditorWindow.DataType, List<(string, string)>>();
            
            AddItems(GasifyEditorWindow.DataType.Ability, Abilities);
            AddItems(GasifyEditorWindow.DataType.Effect, Effects);
            AddItems(GasifyEditorWindow.DataType.Entity, Entities);
            AddItems(GasifyEditorWindow.DataType.Attribute, Attributes);
            AddItems(GasifyEditorWindow.DataType.Tag, Tags);
            AddItems(GasifyEditorWindow.DataType.ProxyTask, ProxyTasks);
            AddItems(GasifyEditorWindow.DataType.AttributeSet, AttributeSets);
            AddItems(GasifyEditorWindow.DataType.Modifier, Modifiers);
            AddItems(GasifyEditorWindow.DataType.AttributeWorker, AttributeEvents);
            AddItems(GasifyEditorWindow.DataType.ImpactWorker, ImpactWorkers);
            AddItems(GasifyEditorWindow.DataType.EffectWorker, EffectWorkers);
            AddItems(GasifyEditorWindow.DataType.TagWorker, TagWorkers);
            AddItems(GasifyEditorWindow.DataType.ProcessInstantiator, ProcessInstantiators);

            return items;

            void AddItems<T>(GasifyEditorWindow.DataType type, List<T> nodes) where T : DescriptiveDataNode
            {
                items[type] = new List<(string, string)>();
                foreach (var node in nodes) items[type].Add((node.Id, node.Name));
            }
        }

        public DescriptiveDataNode Get(string id, GasifyEditorWindow.DataType kind)
        {
            switch (kind)
            {
                case GasifyEditorWindow.DataType.Ability:
                    var AbilityData = Abilities.Where(d => d.Id == id).ToArray();
                    if (AbilityData.Length != 0) return AbilityData[0];
                    break;
                case GasifyEditorWindow.DataType.Effect:
                    var EffectData = Effects.Where(d => d.Id == id).ToArray();
                    if (EffectData.Length != 0) return EffectData[0];
                    break;
                case GasifyEditorWindow.DataType.Entity:
                    var EntityData = Entities.Where(d => d.Id == id).ToArray();
                    if (EntityData.Length != 0) return EntityData[0];
                    break;
                case GasifyEditorWindow.DataType.Attribute:
                    var AttributeData = Attributes.Where(d => d.Id == id).ToArray();
                    if (AttributeData.Length != 0) return AttributeData[0];
                    break;
                case GasifyEditorWindow.DataType.Tag:
                    var TagData = Tags.Where(d => d.Id == id).ToArray();
                    if (TagData.Length != 0) return TagData[0];
                    break;
                case GasifyEditorWindow.DataType.ProxyTask:
                    var ProxyTaskData = ProxyTasks.Where(d => d.Id == id).ToArray();
                    if (ProxyTaskData.Length != 0) return ProxyTaskData[0];
                    break;
                case GasifyEditorWindow.DataType.AttributeSet:
                    var AttributeSetData = AttributeSets.Where(d => d.Id == id).ToArray();
                    if (AttributeSetData.Length != 0) return AttributeSetData[0];
                    break;
                case GasifyEditorWindow.DataType.Modifier:
                    var ModifierData = Modifiers.Where(d => d.Id == id).ToArray();
                    if (ModifierData.Length != 0) return ModifierData[0];
                    break;
                case GasifyEditorWindow.DataType.AttributeWorker:
                    var AttributeWorkerData = AttributeEvents.Where(d => d.Id == id).ToArray();
                    if (AttributeWorkerData.Length != 0) return AttributeWorkerData[0];
                    break;
                case GasifyEditorWindow.DataType.ImpactWorker:
                    var ImpactWorkerData = ImpactWorkers.Where(d => d.Id == id).ToArray();
                    if (ImpactWorkerData.Length != 0) return ImpactWorkerData[0];
                    break;
                case GasifyEditorWindow.DataType.EffectWorker:
                    var EffectWorkerData = EffectWorkers.Where(d => d.Id == id).ToArray();
                    if (EffectWorkerData.Length != 0) return EffectWorkerData[0];
                    break;
                case GasifyEditorWindow.DataType.TagWorker:
                    var TagWorkerData = TagWorkers.Where(d => d.Id == id).ToArray();
                    if (TagWorkerData.Length != 0) return TagWorkerData[0];
                    break;
                case GasifyEditorWindow.DataType.ProcessInstantiator:
                    var ProcessInstantiatorData = ProcessInstantiators.Where(d => d.Id == id).ToArray();
                    if (ProcessInstantiatorData.Length != 0) return ProcessInstantiatorData[0];
                    break;
                case GasifyEditorWindow.DataType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            return null;
        }

        public void Add(GasifyEditorWindow.CreatorItem item)
        {
            var node = From(item);
            
            switch (item.Kind)
            {

                case GasifyEditorWindow.DataType.Ability:
                    for (int i = 0; i < Abilities.Count; i++)
                    {
                        if (Abilities[i].Id == item.Id)
                        {
                            Abilities[i] = node as AbilityData;
                            return;
                        }
                        Abilities.Add(node as AbilityData);
                    }
                    break;
                case GasifyEditorWindow.DataType.Effect:
                    Effects.Add(node as EffectData);
                    for (int i = 0; i < Effects.Count; i++)
                    {
                        if (Effects[i].Id == item.Id)
                        {
                            Effects[i] = node as EffectData;
                            return;
                        }
                        Effects.Add(node as EffectData);
                    }
                    break;
                case GasifyEditorWindow.DataType.Entity:
                    Entities.Add(node as EntityData);
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        if (Entities[i].Id == item.Id)
                        {
                            Entities[i] = node as EntityData;
                            return;
                        }
                        Entities.Add(node as EntityData);
                    }
                    break;
                case GasifyEditorWindow.DataType.Attribute:
                    Attributes.Add(node as AttributeData);
                    for (int i = 0; i < Attributes.Count; i++)
                    {
                        if (Attributes[i].Id == item.Id)
                        {
                            Attributes[i] = node as AttributeData;
                            return;
                        }
                        Attributes.Add(node as AttributeData);
                    }
                    break;
                case GasifyEditorWindow.DataType.Tag:
                    Tags.Add(node as TagData);
                    for (int i = 0; i < Tags.Count; i++)
                    {
                        if (Tags[i].Id == item.Id)
                        {
                            Tags[i] = node as TagData;
                            return;
                        }
                        Tags.Add(node as TagData);
                    }
                    break;
                case GasifyEditorWindow.DataType.ProxyTask:
                    ProxyTasks.Add(node as ProxyTaskData);
                    for (int i = 0; i < ProxyTasks.Count; i++)
                    {
                        if (ProxyTasks[i].Id == item.Id)
                        {
                            ProxyTasks[i] = node as ProxyTaskData;
                            return;
                        }
                        ProxyTasks.Add(node as ProxyTaskData);
                    }
                    break;
                case GasifyEditorWindow.DataType.AttributeSet:
                    AttributeSets.Add(node as AttributeSetData);
                    for (int i = 0; i < AttributeSets.Count; i++)
                    {
                        if (AttributeSets[i].Id == item.Id)
                        {
                            AttributeSets[i] = node as AttributeSetData;
                            return;
                        }
                        AttributeSets.Add(node as AttributeSetData);
                    }
                    break;
                case GasifyEditorWindow.DataType.Modifier:
                    Modifiers.Add(node as ModifierData);
                    for (int i = 0; i < Modifiers.Count; i++)
                    {
                        if (Modifiers[i].Id == item.Id)
                        {
                            Modifiers[i] = node as ModifierData;
                            return;
                        }
                        Modifiers.Add(node as ModifierData);
                    }
                    break;
                case GasifyEditorWindow.DataType.AttributeWorker:
                    AttributeEvents.Add(node as AttributeEventData);
                    for (int i = 0; i < AttributeEvents.Count; i++)
                    {
                        if (AttributeEvents[i].Id == item.Id)
                        {
                            AttributeEvents[i] = node as AttributeEventData;
                            return;
                        }
                        AttributeEvents.Add(node as AttributeEventData);
                    }
                    break;
                case GasifyEditorWindow.DataType.ImpactWorker:
                    ImpactWorkers.Add(node as ImpactWorkerData);
                    for (int i = 0; i < ImpactWorkers.Count; i++)
                    {
                        if (ImpactWorkers[i].Id == item.Id)
                        {
                            ImpactWorkers[i] = node as ImpactWorkerData;
                            return;
                        }
                        ImpactWorkers.Add(node as ImpactWorkerData);
                    }
                    break;
                case GasifyEditorWindow.DataType.EffectWorker:
                    EffectWorkers.Add(node as EffectWorkerData);
                    for (int i = 0; i < EffectWorkers.Count; i++)
                    {
                        if (EffectWorkers[i].Id == item.Id)
                        {
                            EffectWorkers[i] = node as EffectWorkerData;
                            return;
                        }
                        EffectWorkers.Add(node as EffectWorkerData);
                    }
                    break;
                case GasifyEditorWindow.DataType.TagWorker:
                    TagWorkers.Add(node as TagWorkerData);
                    for (int i = 0; i < TagWorkers.Count; i++)
                    {
                        if (TagWorkers[i].Id == item.Id)
                        {
                            TagWorkers[i] = node as TagWorkerData;
                            return;
                        }
                        TagWorkers.Add(node as TagWorkerData);
                    }
                    break;
                case GasifyEditorWindow.DataType.ProcessInstantiator:
                    ProcessInstantiators.Add(node as ProcessInstantiatorData);
                    for (int i = 0; i < ProcessInstantiators.Count; i++)
                    {
                        if (ProcessInstantiators[i].Id == item.Id)
                        {
                            ProcessInstantiators[i] = node as ProcessInstantiatorData;
                            return;
                        }
                        ProcessInstantiators.Add(node as ProcessInstantiatorData);
                    }
                    break;
                case GasifyEditorWindow.DataType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void ReplaceIfItemExists(List<GasifyDataNode> source)
            {
                
            }
        }

        private GasifyDataNode From(GasifyEditorWindow.CreatorItem item)
        {
            var data = item.Kind switch
            {
                GasifyEditorWindow.DataType.Ability => FromAbility(),
                GasifyEditorWindow.DataType.Effect => FromEffect(),
                GasifyEditorWindow.DataType.Entity => FromEntity(),
                GasifyEditorWindow.DataType.Attribute => FromAttribute(),
                GasifyEditorWindow.DataType.Tag => FromTag(),
                GasifyEditorWindow.DataType.ProxyTask => FromProxyTask(),
                GasifyEditorWindow.DataType.AttributeSet => FromAttributeSet(),
                GasifyEditorWindow.DataType.Modifier => FromModifier(),
                GasifyEditorWindow.DataType.AttributeWorker => FromAttributeWorker(),
                GasifyEditorWindow.DataType.ImpactWorker => FromImpactWorker(),
                GasifyEditorWindow.DataType.EffectWorker => FromEffectWorker(),
                GasifyEditorWindow.DataType.TagWorker => FromTagWorker(),
                GasifyEditorWindow.DataType.ProcessInstantiator => FromProcessInstantiator(),
                GasifyEditorWindow.DataType.None => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (data is null) return null;
            
            data.editorTags = item.Data[EditorTagService.EDITOR_TAGS] as Dictionary<Tag, object>;

            return data;
            
            GasifyDataNode FromAbility()
            {
                var node = new AbilityData()
                {
                    Id = item.Id,
                    Name = item.Name
                };
                return node;
            }
            
            GasifyDataNode FromEffect()
            {
                var node = new EffectData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromEntity()
            {
                var node = new EntityData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromAttribute()
            {
                var node = new AttributeData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromTag()
            {
                var node = new TagData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromProxyTask()
            {
                var node = new ProxyTaskData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromAttributeSet()
            {
                var node = new AttributeSetData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromModifier()
            {
                var node = new ModifierData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromAttributeWorker()
            {
                var node = new AttributeEventData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromImpactWorker()
            {
                var node = new ImpactWorkerData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromEffectWorker()
            {
                var node = new EffectWorkerData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromTagWorker()
            {
                var node = new TagWorkerData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
            
            GasifyDataNode FromProcessInstantiator()
            {
                var node = new ProcessInstantiatorData()
                {
                    Id = item.Id,
                    Name = item.Name,
                };
                return node;
            }
        }
        
    }
    
    [Serializable]
    public abstract class GasifyDataNode
    {
        public string Id;

        public bool hasEditorTags => editorTags.Count > 0;
        public Dictionary<Tag, object> editorTags = new();

        public abstract Dictionary<Tag, object> ToData();
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

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }
    
    [Serializable]
    public class TagData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class AbilityData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class ProxyTaskData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }
    
    [Serializable]
    public class EffectData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class EntityData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class AttributeSetData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class ModifierData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class AttributeEventData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }
    
    [Serializable]
    public class ImpactWorkerData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class EffectWorkerData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class TagWorkerData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }

    [Serializable]
    public class ProcessInstantiatorData : DescriptiveDataNode
    {

        public override Dictionary<Tag, object> ToData()
        {
            return null;
        }
    }
}
