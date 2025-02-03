using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public struct AttributeValue
    {
        public float CurrentValue;
        public float BaseValue;

        public AttributeValue(float currentValue, float baseValue)
        {
            CurrentValue = currentValue;
            BaseValue = baseValue;
        }

        public AttributeValue ApplyModified(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new AttributeValue(
                CurrentValue + modifiedAttributeValue.DeltaCurrentValue,
                BaseValue + modifiedAttributeValue.DeltaBaseValue
            );
        }
        
        public static AttributeValue operator +(AttributeValue a, AttributeValue b)
        {
            return new AttributeValue(a.CurrentValue + b.CurrentValue, a.BaseValue + b.BaseValue);
        }
        
        public static AttributeValue operator -(AttributeValue a, AttributeValue b)
        {
            return new AttributeValue(a.CurrentValue - b.CurrentValue, a.BaseValue - b.BaseValue);
        }
        
        public static AttributeValue operator /(AttributeValue a, AttributeValue b)
        {
            return new AttributeValue(a.CurrentValue / b.CurrentValue, a.BaseValue / b.BaseValue);
        }
        
        public static AttributeValue operator /(AttributeValue a, float b)
        {
            return new AttributeValue(a.CurrentValue / b, a.BaseValue / b);
        }
        
        public static AttributeValue operator *(AttributeValue a, float b)
        {
            return new AttributeValue(a.CurrentValue * b, a.BaseValue * b);
        }
        
        public static AttributeValue operator *(AttributeValue a, AttributeValue b)
        {
            return new AttributeValue(a.CurrentValue * b.CurrentValue, a.BaseValue * b.BaseValue);
        }
        
        public override string ToString()
        {
            return $"{CurrentValue}/{BaseValue}";
        }
    }

    public class CachedAttributeValue
    {
        public Dictionary<IAttributeDerivation, AttributeValue> DerivedValues = new();
        public AttributeValue Value;

        public void Add(IAttributeDerivation derivation, AttributeValue attributeValue)
        {
            if (DerivedValues.ContainsKey(derivation)) DerivedValues[derivation] += attributeValue;
            else DerivedValues[derivation] = attributeValue;

            Value += attributeValue;
        }

        public void Add(IAttributeDerivation derivation, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (DerivedValues.ContainsKey(derivation)) DerivedValues[derivation] = DerivedValues[derivation].ApplyModified(modifiedAttributeValue);
            else DerivedValues[derivation] = modifiedAttributeValue.ToAttributeValue();

            Value += modifiedAttributeValue.ToAttributeValue();
        }

        public void Remove(IAttributeDerivation derivation)
        {
            if (!DerivedValues.ContainsKey(derivation)) return;
            
            Value -= DerivedValues[derivation];
            DerivedValues.Remove(derivation);
        }

        public void Set(IAttributeDerivation derivation, AttributeValue attributeValue)
        {
            if (!DerivedValues.ContainsKey(derivation)) return;
            AttributeValue difference = attributeValue - DerivedValues[derivation];
            DerivedValues[derivation] = attributeValue;
            Value += difference;
        }

        public void Clamp(EAttributeModificationMethod method, AttributeValue floor, AttributeValue ceil)
        {
            if (floor.CurrentValue <= Value.CurrentValue && Value.CurrentValue <= ceil.CurrentValue && floor.BaseValue <= Value.BaseValue &&
                Value.BaseValue <= ceil.BaseValue) return;

            float currDelta = 0f;
            if (Value.CurrentValue < floor.CurrentValue) currDelta = floor.CurrentValue - Value.CurrentValue;
            else if (Value.CurrentValue > ceil.CurrentValue) currDelta = ceil.CurrentValue - Value.CurrentValue;

            float baseDelta = 0f;
            if (Value.BaseValue < floor.BaseValue) baseDelta = floor.BaseValue - Value.BaseValue;
            else if (Value.BaseValue > ceil.BaseValue) baseDelta = ceil.BaseValue - Value.BaseValue;

            Debug.Log($"Clamping with floor {floor} and ceil {ceil} with {currDelta}/{baseDelta} for {Value.CurrentValue}/{Value.BaseValue}");
            
            switch (method)
            {
                case EAttributeModificationMethod.FromLast:
                    foreach (IAttributeDerivation derivation in DerivedValues.Keys.Reverse())
                    {
                        Debug.Log($"{derivation.GetEffectDerivation().GetName()} => {DerivedValues[derivation]}");
                        if (currDelta == 0f && baseDelta == 0f) return;
                        AttributeValue delta = Limit(derivation, ref currDelta, ref baseDelta);
                        //DerivedValues[derivation] = delta;
                        Set(derivation, delta);
                    }
                    break;
                case EAttributeModificationMethod.FromFirst:
                    foreach (IAttributeDerivation derivation in DerivedValues.Keys)
                    {
                        if (currDelta == 0f && baseDelta == 0f) return;
                        AttributeValue delta = Limit(derivation, ref currDelta, ref baseDelta);
                        //DerivedValues[derivation] = delta;
                        Set(derivation, delta);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            foreach (IAttributeDerivation derivation in DerivedValues.Keys)
            {
                Debug.Log($"{derivation.GetEffectDerivation().GetName()} => {DerivedValues[derivation]}");
            }
        }

        private AttributeValue Limit(IAttributeDerivation derivation, ref float currDelta, ref float baseDelta)
        {
            AttributeValue delta = default;
            if (currDelta != 0f)
            {
                if (currDelta < 0f)  // We want to decrease the curr value (higher than ceil)
                {
                    delta.CurrentValue = Mathf.Max(currDelta, DerivedValues[derivation].CurrentValue);
                    currDelta -= delta.CurrentValue;
                }
                else  // We want to increase the curr value (lower than floor)
                {
                    delta.CurrentValue = Mathf.Min(currDelta, DerivedValues[derivation].CurrentValue);
                    currDelta += delta.CurrentValue;
                }
            }
                        
            if (baseDelta != 0f)
            {
                if (baseDelta < 0f)  // We want to decrease the base value (higher than ceil)
                {
                    delta.BaseValue = Mathf.Max(baseDelta, DerivedValues[derivation].BaseValue);
                    baseDelta -= delta.BaseValue;
                }
                else  // We want to increase the base value (lower than floor)
                {
                    delta.BaseValue = Mathf.Min(baseDelta, DerivedValues[derivation].BaseValue);
                    baseDelta += delta.BaseValue;
                }
            }

            Debug.Log($"Limited delta: {delta}");
            return delta;
        }
    }

}
