using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewAttributeSet", menuName = "FESGAS/Attribute/Set")]
    public class AttributeSetScriptableObject : ScriptableObject
    {
        [Header("Attribute Set")]
        
        public List<AttributeSetElement> Attributes;
        
        [Space]
        
        public List<AttributeSetScriptableObject> SubSets;
        public EValueCollisionPolicy AttributeSetCollisionResolution;

        public void Initialize(AttributeSystemComponent system)
        {
            AttributeSetMeta meta = new AttributeSetMeta(this);
            meta.InitializeAttributeSystem(system, this);
        }
    }
    
    public enum ELimitedEffectImpactTarget
    {
        CurrentAndBase,
        Base
    }

    public enum EValueCollisionPolicy
    {
        UseMaximum,
        UseMinimum,
        UseAverage
    }
    
    [Serializable]
    public struct AttributeSetElement
    {
        public AttributeScriptableObject Attribute;
        public ELimitedEffectImpactTarget Target;
        public EAttributeElementCollisionPolicy CollisionPolicy;
        public float Magnitude;

        public ModifiedAttributeValue ToModifiedAttribute()
        {
            return Target switch
            {
                ELimitedEffectImpactTarget.CurrentAndBase => new ModifiedAttributeValue(Magnitude, Magnitude),
                ELimitedEffectImpactTarget.Base => new ModifiedAttributeValue(0, Magnitude),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    public enum EAttributeElementCollisionPolicy
    {
        UseThis,
        UseExisting,
        Combine
    }

    public class AttributeSetMeta
    {
        private Dictionary<AttributeScriptableObject, Dictionary<EAttributeElementCollisionPolicy, List<ModifiedAttributeValue>>> matrix; 

        public AttributeSetMeta(AttributeSetScriptableObject attributeSet)
        {
            matrix = new Dictionary<AttributeScriptableObject, Dictionary<EAttributeElementCollisionPolicy, List<ModifiedAttributeValue>>>();
            HandleAttributeSet(attributeSet);
        }

        private void HandleAttributeSet(AttributeSetScriptableObject attributeSet)
        {
            foreach (AttributeSetElement element in attributeSet.Attributes)
            {
                if (!matrix.TryGetValue(element.Attribute, out var table))
                {
                    table = matrix[element.Attribute] = new Dictionary<EAttributeElementCollisionPolicy, List<ModifiedAttributeValue>>();
                }

                if (!table.ContainsKey(element.CollisionPolicy))
                {
                    matrix[element.Attribute][element.CollisionPolicy] = new List<ModifiedAttributeValue> { element.ToModifiedAttribute() };
                }
                else matrix[element.Attribute][element.CollisionPolicy].Add(element.ToModifiedAttribute());
            }
            
            foreach (AttributeSetScriptableObject subSet in attributeSet.SubSets) HandleAttributeSet(subSet);
        }

        public void InitializeAttributeSystem(AttributeSystemComponent system, AttributeSetScriptableObject attributeSet)
        {
            foreach (AttributeScriptableObject attribute in matrix.Keys)
            {
                if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseThis, out var mavs))
                {
                    InitializeAggregatePolicy(system, attribute, mavs, attributeSet.AttributeSetCollisionResolution);
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.Combine, out mavs))
                {
                    ModifiedAttributeValue mav = new ModifiedAttributeValue();
                    foreach (ModifiedAttributeValue metaMav in mavs) mav = mav.Combine(metaMav);

                    system.ProvideAttribute(attribute, mav);
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseExisting, out mavs))
                {
                    InitializeAggregatePolicy(system, attribute, mavs, attributeSet.AttributeSetCollisionResolution);
                }
            }
        }

        private void InitializeAggregatePolicy(AttributeSystemComponent system, AttributeScriptableObject attribute, List<ModifiedAttributeValue> mavs, EValueCollisionPolicy resolution)
        {
            switch (resolution)
            {
                case EValueCollisionPolicy.UseAverage:
                {
                    float _current = mavs.Average(mav => mav.DeltaCurrentValue);
                    float _base = mavs.Average(mav => mav.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new ModifiedAttributeValue(_current, _base));
                    break;
                }
                case EValueCollisionPolicy.UseMaximum:
                {
                    float _current = mavs.Max(mav => mav.DeltaCurrentValue);
                    float _base = mavs.Max(mav => mav.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new ModifiedAttributeValue(_current, _base));
                    break;
                }
                case EValueCollisionPolicy.UseMinimum:
                {
                    float _current = mavs.Min(mav => mav.DeltaCurrentValue);
                    float _base = mavs.Min(mav => mav.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new ModifiedAttributeValue(_current, _base));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null);
            }
        }
    }
}
