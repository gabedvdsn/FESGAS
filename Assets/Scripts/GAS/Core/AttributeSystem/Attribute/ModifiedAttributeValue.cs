using FESGameplayAbilitySystem.Core;
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

        public SignPolicy SignPolicy => StaticSignPolicy.DeterminePolicy(DeltaCurrentValue, DeltaBaseValue);

        public SourcedModifiedAttributeValue ToSourced(GameplayEffectShelfContainer container)
        {
            return new SourcedModifiedAttributeValue(
                container,
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
        public GameplayEffectShelfContainer Container;
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public SourcedModifiedAttributeValue(GameplayEffectShelfContainer container, float deltaCurrentValue, float deltaBaseValue)
        {
            Container = container;
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public SourcedModifiedAttributeValue Combine(SourcedModifiedAttributeValue other)
        {
            if (other.Container != Container)
            {
                return this;
            }
            
            return new SourcedModifiedAttributeValue(
                Container,
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Negate()
        {
            return new SourcedModifiedAttributeValue(
                Container,
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Multiply(float magnitude)
        {
            return new SourcedModifiedAttributeValue(
                Container,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Add(float magnitude)
        {
            return new SourcedModifiedAttributeValue(
                Container,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Override(float currentMagnitude, float baseMagnitude)
        {
            return new SourcedModifiedAttributeValue(
                Container,
                currentMagnitude,
                baseMagnitude
            );
        }

        public ModifiedAttributeValue ToModifiedAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public AttributeValue ToAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            if (Container is null) return $"[ SMAV-NULL ] {DeltaCurrentValue}/{DeltaBaseValue}";
            return $"[ SMAV-{Container.Spec.Ability.Base.Definition.Name} ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }
    }
}
