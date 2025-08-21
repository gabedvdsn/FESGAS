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
    public class AttributeSetScriptableObject : ScriptableObject, IAttributeSet
    {
        [Header("Attribute Set")]
        
        public List<AttributeSetElement> Attributes;
        
        [Space]
        
        public List<AttributeSetScriptableObject> SubSets;
        public EValueCollisionPolicy CollisionResolutionPolicy;

        public List<AttributeSetElement> GetAttributes()
        {
            return Attributes;
        }
        public List<IAttributeSet> GetSubSets()
        {
            return SubSets.Cast<IAttributeSet>().ToList();
        }
        public EValueCollisionPolicy GetCollisionResolutionPolicy()
        {
            return CollisionResolutionPolicy;
        }
        public void Initialize(AttributeSystemComponent system)
        {
            AttributeSetMeta meta = new AttributeSetMeta(this);
            meta.InitializeAttributeSystem(system, this);
        }

        public HashSet<IAttribute> GetUnique()
        {
            var attributes = new HashSet<IAttribute>();
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

    public enum EValueCollisionPolicy
    {
        UseMaximum,
        UseMinimum,
        UseAverage
    }
    
    [Serializable]
    public struct AttributeSetElement
    {
        [Header("Attribute Declaration")]
        
        public AttributeScriptableObject Attribute;
        public float Magnitude;
        public AbstractMagnitudeModifierScriptableObject MagnitudeModifier;
        
        public ELimitedEffectImpactTarget Target;
        public AttributeOverflowData Overflow;
        
        [Header("Multiple Set Collision")]
        
        public EAttributeElementCollisionPolicy CollisionPolicy;

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

        public AttributeSetMeta(IAttributeSet attributeSet)
        {
            matrix = new Dictionary<AttributeScriptableObject, Dictionary<EAttributeElementCollisionPolicy, List<DefaultAttributeValue>>>();
            HandleAttributeSet(attributeSet);
        }

        private void HandleAttributeSet(IAttributeSet attributeSet)
        {
            foreach (AttributeSetElement element in attributeSet.GetAttributes())
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
            
            foreach (IAttributeSet subSet in attributeSet.GetSubSets()) HandleAttributeSet(subSet);
        }

        public void InitializeAttributeSystem(AttributeSystemComponent system, IAttributeSet attributeSet)
        {
            foreach (AttributeScriptableObject attribute in matrix.Keys)
            {
                if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseThis, out var defaults))
                {
                    InitializeAggregatePolicy(system, attribute, defaults, attributeSet.GetCollisionResolutionPolicy());
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.Combine, out defaults))
                {
                    DefaultAttributeValue defaultValue = new DefaultAttributeValue();
                    foreach (DefaultAttributeValue metaMav in defaults) defaultValue = defaultValue.Combine(metaMav);

                    system.ProvideAttribute(attribute, defaultValue);
                }
                else if (matrix[attribute].TryGetValue(EAttributeElementCollisionPolicy.UseExisting, out defaults))
                {
                    InitializeAggregatePolicy(system, attribute, defaults, attributeSet.GetCollisionResolutionPolicy());
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

    public interface IAttributeSet
    {
        public List<AttributeSetElement> GetAttributes();
        public List<IAttributeSet> GetSubSets();
        public EValueCollisionPolicy GetCollisionResolutionPolicy();
        public void Initialize(AttributeSystemComponent system);
        public HashSet<IAttribute> GetUnique();

        public static IAttributeSet GenerateEmpty()
        {
            return new CustomAttributeSet();
        }
    }

    public class CustomAttributeSet : IAttributeSet
    {
        public List<AttributeSetElement> Attributes = new();
        public List<IAttributeSet> SubSets = new();
        public EValueCollisionPolicy CollisionResolutionPolicy = EValueCollisionPolicy.UseMaximum;

        public List<AttributeSetElement> GetAttributes()
        {
            return Attributes;
        }
        public List<IAttributeSet> GetSubSets()
        {
            return SubSets;
        }
        public EValueCollisionPolicy GetCollisionResolutionPolicy()
        {
            return CollisionResolutionPolicy;
        }
        public void Initialize(AttributeSystemComponent system)
        {
            AttributeSetMeta meta = new AttributeSetMeta(this);
            meta.InitializeAttributeSystem(system, this);
        }
        public HashSet<IAttribute> GetUnique()
        {
            var attributes = new HashSet<IAttribute>();
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
}
