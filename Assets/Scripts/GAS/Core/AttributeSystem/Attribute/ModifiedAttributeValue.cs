﻿using FESGameplayAbilitySystem.Core;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public struct ModifiedAttributeValue
    {
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public ModifiedAttributeValue(float deltaCurrentValue, float deltaBaseValue)
        {
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public ModifiedAttributeValue Combine(ModifiedAttributeValue other)
        {
            return new ModifiedAttributeValue(
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue);
        }

        public ModifiedAttributeValue Negate()
        {
            return new ModifiedAttributeValue(
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }
        
        public ModifiedAttributeValue Multiply(AttributeValue attributeValue, bool oneMinus = true)
        {
            if (oneMinus)
            {
                return new ModifiedAttributeValue(
                    DeltaCurrentValue - DeltaCurrentValue * attributeValue.CurrentValue,
                    DeltaBaseValue - DeltaBaseValue * attributeValue.BaseValue
                );
            }
            
            return new ModifiedAttributeValue(
                DeltaCurrentValue * attributeValue.CurrentValue,
                DeltaBaseValue * attributeValue.BaseValue
            );
            
        }

        public ModifiedAttributeValue Multiply(ModifiedAttributeValue modifiedAttributeValue, bool oneMinus = true)
        {
            if (oneMinus)
            {
                return new ModifiedAttributeValue(
                    DeltaCurrentValue - DeltaCurrentValue * modifiedAttributeValue.DeltaCurrentValue,
                    DeltaBaseValue - DeltaBaseValue * modifiedAttributeValue.DeltaBaseValue
                );
            }
            
            return new ModifiedAttributeValue(
                DeltaCurrentValue * modifiedAttributeValue.DeltaCurrentValue,
                DeltaBaseValue * modifiedAttributeValue.DeltaBaseValue
            );
        }

        public SignPolicy SignPolicy => StaticSignPolicy.DeterminePolicy(DeltaCurrentValue, DeltaBaseValue);

        public SourcedModifiedAttributeValue ToSourced(IAttributeDerivation derivation)
        {
            return new SourcedModifiedAttributeValue(
                derivation,
                DeltaCurrentValue,
                DeltaBaseValue
            );
        }

        public AttributeValue ToAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            return $"[ MAV ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }

        public static ModifiedAttributeValue operator -(ModifiedAttributeValue mav1, ModifiedAttributeValue mav2)
        {
            return new ModifiedAttributeValue(mav1.DeltaCurrentValue - mav2.DeltaCurrentValue,
                mav1.DeltaBaseValue - mav2.DeltaBaseValue);
        }
    }

    public struct SourcedModifiedAttributeValue
    {
        public IAttributeDerivation Derivation;
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public SignPolicy SignPolicy => StaticSignPolicy.DeterminePolicy(DeltaCurrentValue, DeltaBaseValue);
        
        public SourcedModifiedAttributeValue(IAttributeDerivation derivation, float deltaCurrentValue, float deltaBaseValue)
        {
            Derivation = derivation;
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public SourcedModifiedAttributeValue Combine(SourcedModifiedAttributeValue other)
        {
            if (other.Derivation != Derivation)
            {
                return this;
            }
            
            return new SourcedModifiedAttributeValue(
                Derivation,
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Negate()
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Multiply(float magnitude)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Multiply(AttributeValue attributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                DeltaCurrentValue * attributeValue.CurrentValue,
                DeltaBaseValue * attributeValue.BaseValue
            );
        }

        public SourcedModifiedAttributeValue Multiply(ModifiedAttributeValue modifiedAttributeValue)
        {
            return ToModified().Multiply(modifiedAttributeValue).ToSourced(Derivation);
        }
        
        public SourcedModifiedAttributeValue Add(float magnitude)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Add(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                DeltaCurrentValue * modifiedAttributeValue.DeltaCurrentValue,
                DeltaBaseValue * modifiedAttributeValue.DeltaBaseValue
            );
        }
        
        public SourcedModifiedAttributeValue Override(float currentMagnitude, float baseMagnitude)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                currentMagnitude,
                baseMagnitude
            );
        }
        
        public SourcedModifiedAttributeValue Override(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                modifiedAttributeValue.DeltaCurrentValue,
                modifiedAttributeValue.DeltaBaseValue
            );
        }

        public ModifiedAttributeValue ToModified() => new(DeltaCurrentValue, DeltaBaseValue);

        public AttributeValue ToAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            if (Derivation is null) return $"[ SMAV-INSTANT ] {DeltaCurrentValue}/{DeltaBaseValue}";
            return $"[ SMAV-{Derivation.GetEffectDerivation().GetName()} ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }
    }
}
