using System;
using System.Collections.Generic;
using System.Linq;
using FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponent : MonoBehaviour
    {
        [Header("Attributes")]
        
        public AttributeSetScriptableObject AttributeSet;
        
        [Header("Attribute Change Events")]
        
        [SerializeField] private List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;

        private AttributeChangeEventHandler ChangeEventHandler;
        
        private Dictionary<AttributeScriptableObject, CachedAttributeValue> AttributeCache;
        private SourcedModifiedAttributeCache ModifiedAttributeCache;
        private bool modifiedCacheDirty;

        private Dictionary<AttributeScriptableObject, AttributeValue> HoldAttributeCache;

        // Order of events should be considered (e.g. clamping before damage numbers)
        // Pre: changing modified attribute values to reflect weakness, amplification, etc...
        // Post: clamping max values, damage numbers, etc...

        private GASComponent System;

        public void Initialize(GASComponent system)
        {
            System = system;
            
            InitializeCaches();
            InitializePriorityChangeEvents();
            InitializeAttributeSets();
        }
        
        private void LateUpdate()
        {
            UpdateAttributes();
        }

        private void InitializeCaches()
        {
            AttributeCache = new Dictionary<AttributeScriptableObject, CachedAttributeValue>();
            ModifiedAttributeCache = new SourcedModifiedAttributeCache();
            HoldAttributeCache = new Dictionary<AttributeScriptableObject, AttributeValue>();
        }

        private void InitializeAttributeSets()
        {
            AttributeSet.Initialize(this);
            
            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.GetDefined())
            {
                ModifyAttribute(attribute, new SourcedModifiedAttributeValue(IAttributeDerivation.GenerateSourceDerivation(System, attribute), 0f, 0f, false));
            }
            
            UpdateAttributes();
        }

        private void InitializePriorityChangeEvents()
        {
            ChangeEventHandler = new AttributeChangeEventHandler();
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents) ChangeEventHandler.AddEvent(changeEvent);
        }

        public void ProvideAttribute(AttributeScriptableObject attribute, DefaultAttributeValue defaultValue)
        {
            if (AttributeCache.ContainsKey(attribute)) return;
            AttributeCache[attribute] = new CachedAttributeValue(defaultValue.Overflow);
            
            AttributeCache[attribute].Add(IAttributeDerivation.GenerateSourceDerivation(System, attribute), defaultValue.ToAttributeValue());
            ModifiedAttributeCache.SubscribeModifiableAttribute(attribute);
        }

        public bool DefinesAttribute(AttributeScriptableObject attribute) => AttributeCache.ContainsKey(attribute);

        private void UpdateAttributes(bool allowWorkers = true)
        {
            ApplyAttributeModifications(allowWorkers);
        }

        public void RemoveAttributeDerivation(IAttributeDerivation derivation)
        {
            if (!AttributeCache.ContainsKey(derivation.GetAttribute())) return;
            AttributeCache[derivation.GetAttribute()].Remove(derivation);
        }

        public void RemoveAttributeDerivations(List<IAttributeDerivation> derivations)
        {
            foreach (IAttributeDerivation derivation in derivations)
            {
                if (!AttributeCache.ContainsKey(derivation.GetAttribute())) continue;
                AttributeCache[derivation.GetAttribute()].Remove(derivation);
            }
        }
        
        private void ApplyAttributeModifications(bool allowWorkers = true)
        {
            if (!modifiedCacheDirty) return;
            
            ChangeEventHandler.RunPreChangeEvents(System, ref AttributeCache, ModifiedAttributeCache);

            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.GetModified())
            {
                HoldAttributeCache[attribute] = AttributeCache[attribute].Value;
                if (!ModifiedAttributeCache.TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    AttributeCache[attribute].Add(sourcedModifier.Derivation, sourcedModifier.ToModified());
                }
            }
            
            ChangeEventHandler.RunPostChangeEvents(System, ref AttributeCache, ModifiedAttributeCache);

            List<GASComponent> communicateComps = new List<GASComponent>();
            // Communicate the impact of the modification back to the source
            foreach (AttributeScriptableObject attribute in HoldAttributeCache.Keys)
            {
                if (!ModifiedAttributeCache.TryGetCachedValue(attribute, out var sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    if (sourcedModifier.BaseDerivation.GetSource().AbilitySystem.CommunicateAbilityImpact(
                            AbilityImpactData.Generate(
                                System, attribute, sourcedModifier, AttributeCache[attribute].Value - HoldAttributeCache[attribute]
                            )
                        ))
                    {
                        // Only add if the comp has applicable workers
                        communicateComps.Add(sourcedModifier.Derivation.GetSource());
                    }
                }
                
                AttributeCache[attribute].Clean();
            }
            
            modifiedCacheDirty = false;
            HoldAttributeCache.Clear();
            ModifiedAttributeCache.Clear();

            if (!allowWorkers) return;
            foreach (GASComponent comp in communicateComps) comp.AbilitySystem.ActivateAbilityImpactWorkers();
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;
            
            modifiedCacheDirty = true;
            ModifiedAttributeCache.Add(attribute, sourcedModifiedValue);
        }

        public void ModifyAttributeImmediate(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue, bool allowWorkers = true)
        {
            ModifyAttribute(attribute, sourcedModifiedValue);
            UpdateAttributes(allowWorkers);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out CachedAttributeValue attributeValue)
        {
            return AttributeCache.TryGetValue(attribute, out attributeValue);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out AttributeValue attributeValue)
        {
            if (AttributeCache.TryGetValue(attribute, out var cachedValue))
            {
                attributeValue = cachedValue.Value;
                return true;
            }

            attributeValue = default;
            return false;
        }

        private class AttributeChangeEventHandler
        {
            private List<AttributeChangeEventPriorityPacket> Packets = new();

            public void AddEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
            {
                if (Packets.Count == 0)
                {
                    Packets.Add(
                        new AttributeChangeEventPriorityPacket(changeEvent.Priorities[0])
                    );
                    Packets[0].AddEvent(changeEvent, 1);
                    return;
                }

                bool inserted = false;
                for (int i = 0; i < Packets.Count; i++)
                {
                    if (changeEvent.Priorities[0] > Packets[i].Priority) continue;
                    
                    if (changeEvent.Priorities[0] == Packets[i].Priority) Packets[i].AddEvent(changeEvent, 1);
                    else if (changeEvent.Priorities[0] < Packets[i].Priority)
                    {
                        Packets.Insert(i, new AttributeChangeEventPriorityPacket(changeEvent.Priorities[0]));
                        Packets[i].AddEvent(changeEvent, 1);
                    }
                    
                    inserted = true;
                    break;
                }

                if (inserted) return;

                Packets.Add(
                    new AttributeChangeEventPriorityPacket(changeEvent.Priorities[0])
                );
                Packets[^1].AddEvent(changeEvent, 1);
            }

            public bool RemoveEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
            {
                return Packets.Any(packet => packet.TryRemoveEvent(changeEvent));
            }

            public void RunPreChangeEvents(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
            {
                foreach (AttributeChangeEventPriorityPacket packet in Packets) packet.RunPreChangeEvents(system, ref attributeCache, modifiedAttributeCache);
            }

            public void RunPostChangeEvents(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
            {
                foreach (AttributeChangeEventPriorityPacket packet in Packets) packet.RunPostChangeEvents(system, ref attributeCache, modifiedAttributeCache);
            }
        }
        
        private struct AttributeChangeEventPriorityPacket
        {
            public int Priority;
            private List<AbstractAttributeChangeEventScriptableObject> Events;
            private List<AttributeChangeEventPriorityPacket> SubPackets;

            public AttributeChangeEventPriorityPacket(int priority)
            {
                Priority = priority;
                
                Events = new List<AbstractAttributeChangeEventScriptableObject>();
                SubPackets = new List<AttributeChangeEventPriorityPacket>();
            }

            public void AddEvent(AbstractAttributeChangeEventScriptableObject changeEvent, int depth)
            {
                if (depth >= changeEvent.Priorities.Count) Events.Add(changeEvent);
                else
                {
                    bool inserted = false;
                    for (int i = 0; i < SubPackets.Count; i++)
                    {
                        if (changeEvent.Priorities[depth] > SubPackets[i].Priority) continue;
                    
                        if (changeEvent.Priorities[depth] == SubPackets[i].Priority) SubPackets[i].AddEvent(changeEvent, depth + 1);
                        else if (changeEvent.Priorities[depth] < SubPackets[i].Priority)
                        {
                            SubPackets.Insert(i, new AttributeChangeEventPriorityPacket(changeEvent.Priorities[depth]));
                            SubPackets[i].AddEvent(changeEvent, depth + 1);
                        }
                    
                        inserted = true;
                        break;
                    }

                    if (inserted) return;
                    
                    SubPackets.Add(new AttributeChangeEventPriorityPacket());
                    SubPackets[^1].AddEvent(changeEvent, depth + 1);
                }
            }

            public bool TryRemoveEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
            {
                if (Events.Remove(changeEvent)) return true;
                
                for (int i = 0; i < SubPackets.Count; i++)
                {
                    if (SubPackets[i].TryRemoveEvent(changeEvent))
                    {
                        if (SubPackets[i].Events.Count == 0) SubPackets.RemoveAt(i);
                        return true;
                    }
                }

                return false;

            }
            
            public void RunPreChangeEvents(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
            {
                foreach (AbstractAttributeChangeEventScriptableObject changeEvent in Events) changeEvent.PreAttributeChange(system, ref attributeCache, modifiedAttributeCache);
                foreach (AttributeChangeEventPriorityPacket packet in SubPackets) packet.RunPreChangeEvents(system, ref attributeCache, modifiedAttributeCache);
            }

            public void RunPostChangeEvents(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
            {
                foreach (AbstractAttributeChangeEventScriptableObject changeEvent in Events) changeEvent.PostAttributeChange(system, ref attributeCache, modifiedAttributeCache);
                foreach (AttributeChangeEventPriorityPacket packet in SubPackets) packet.RunPostChangeEvents(system, ref attributeCache, modifiedAttributeCache);
            }
        }
    }
}

