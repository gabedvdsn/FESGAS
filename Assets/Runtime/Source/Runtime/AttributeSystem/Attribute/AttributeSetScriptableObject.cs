using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
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

        public HashSet<AttributeScriptableObject> GetUnique()
        {
            var attributes = new HashSet<AttributeScriptableObject>();
            foreach (var attr in Attributes)
            {
                attributes.Add(attr.Attribute);
            }

            foreach (var subSet in SubSets)
            {
                foreach (var unique in subSet.GetUnique())
                {
                    attributes.Add(unique);
                }
            }

            return attributes;
        }
    }
    
    public enum ELimitedEffectImpactTarget
    {
        CurrentAndBase,
        Base
    }

    /*public enum EEffectImpactTarget
    {
        Current,
        Base,
        CurrentAndBase
    }*/

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
        public AttributeOverflowData Overflow;

        public DefaultAttributeValue ToDefaultAttribute()
        {
            return Target switch
            {
                ELimitedEffectImpactTarget.CurrentAndBase => new DefaultAttributeValue(new ModifiedAttributeValue(Magnitude, Magnitude), Overflow),
                ELimitedEffectImpactTarget.Base => new DefaultAttributeValue(new ModifiedAttributeValue(0, Magnitude), Overflow),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public struct DefaultAttributeValue
    {
        public ModifiedAttributeValue DefaultValue;
        public AttributeOverflowData Overflow;

        public DefaultAttributeValue(ModifiedAttributeValue defaultValue, AttributeOverflowData overflow)
        {
            DefaultValue = defaultValue;
            Overflow = overflow;
        }

        public DefaultAttributeValue Combine(DefaultAttributeValue other)
        {
            return new DefaultAttributeValue(DefaultValue.Combine(other.DefaultValue), Overflow);
        }

        public AttributeValue ToAttributeValue() => DefaultValue.ToAttributeValue();
    }
    
    public enum EAttributeElementCollisionPolicy
    {
        UseThis,
        UseExisting,
        Combine
    }

    public class AttributeSetMeta
    {
        private Dictionary<AttributeScriptableObject, Dictionary<EAttributeElementCollisionPolicy, List<DefaultAttributeValue>>> matrix; 

        public AttributeSetMeta(AttributeSetScriptableObject attributeSet)
        {
            matrix = new Dictionary<AttributeScriptableObject, Dictionary<EAttributeElementCollisionPolicy, List<DefaultAttributeValue>>>();
            HandleAttributeSet(attributeSet);
        }

        private void HandleAttributeSet(AttributeSetScriptableObject attributeSet)
        {
            foreach (AttributeSetElement element in attributeSet.Attributes)
            {
                if (!matrix.TryGetValue(element.Attribute, out var table))
                {
                    table = matrix[element.Attribute] = new Dictionary<EAttributeElementCollisionPolicy, List<DefaultAttributeValue>>();
                }

                if (!table.ContainsKey(element.CollisionPolicy))
                {
                    matrix[element.Attribute][element.CollisionPolicy] = new List<DefaultAttributeValue> { element.ToDefaultAttribute() };
                }
                else matrix[element.Attribute][element.CollisionPolicy].Add(element.ToDefaultAttribute());
            }
            
            foreach (AttributeSetScriptableObject subSet in attributeSet.SubSets) HandleAttributeSet(subSet);
        }

        public void InitializeAttributeSystem(AttributeSystemComponent system, AttributeSetScriptableObject attributeSet)
        {
            foreach (AttributeScriptableObject attribute in matrix.Keys)
            {
                if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseThis, out var defaults))
                {
                    InitializeAggregatePolicy(system, attribute, defaults, attributeSet.AttributeSetCollisionResolution);
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.Combine, out defaults))
                {
                    DefaultAttributeValue defaultValue = new DefaultAttributeValue();
                    foreach (DefaultAttributeValue metaMav in defaults) defaultValue = defaultValue.Combine(metaMav);

                    system.ProvideAttribute(attribute, defaultValue);
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseExisting, out defaults))
                {
                    InitializeAggregatePolicy(system, attribute, defaults, attributeSet.AttributeSetCollisionResolution);
                }
            }
        }

        private void InitializeAggregatePolicy(AttributeSystemComponent system, AttributeScriptableObject attribute, List<DefaultAttributeValue> defaults, EValueCollisionPolicy resolution)
        {
            switch (resolution)
            {
                case EValueCollisionPolicy.UseAverage:
                {
                    float _current = defaults.Average(mav => mav.DefaultValue.DeltaCurrentValue);
                    float _base = defaults.Average(mav => mav.DefaultValue.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new DefaultAttributeValue(new ModifiedAttributeValue(_current, _base), defaults[0].Overflow));
                    break;
                }
                case EValueCollisionPolicy.UseMaximum:
                {
                    float _current = defaults.Max(mav => mav.DefaultValue.DeltaCurrentValue);
                    float _base = defaults.Max(mav => mav.DefaultValue.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new DefaultAttributeValue(new ModifiedAttributeValue(_current, _base), defaults[0].Overflow));
                    break;
                }
                case EValueCollisionPolicy.UseMinimum:
                {
                    float _current = defaults.Min(mav => mav.DefaultValue.DeltaCurrentValue);
                    float _base = defaults.Min(mav => mav.DefaultValue.DeltaBaseValue);

                    system.ProvideAttribute(attribute, new DefaultAttributeValue(new ModifiedAttributeValue(_current, _base), defaults[0].Overflow));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null);
            }
        }
    }
}
