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

        public ESignPolicy SignPolicy => GASHelper.SignPolicy(DeltaCurrentValue, DeltaBaseValue);

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
        public IAttributeDerivation BaseDerivation;
        
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public bool Workable;

        public ESignPolicy SignPolicy => GASHelper.SignPolicy(DeltaCurrentValue, DeltaBaseValue);
        
        public SourcedModifiedAttributeValue(IAttributeDerivation derivation, float deltaCurrentValue, float deltaBaseValue, bool workable = true)
        {
            Derivation = derivation;
            BaseDerivation = derivation;
            
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;

            Workable = workable;
        }

        public SourcedModifiedAttributeValue(IAttributeDerivation derivation, IAttributeDerivation baseDerivation, float deltaCurrentValue, float deltaBaseValue,
            bool workable = true)
        {
            Derivation = derivation;
            BaseDerivation = baseDerivation;
            
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;

            Workable = workable;
        }

        public SourcedModifiedAttributeValue Combine(SourcedModifiedAttributeValue other)
        {
            if (other.Derivation != Derivation)
            {
                return this;
            }
            
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Negate()
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Multiply(float magnitude)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Multiply(AttributeValue attributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
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
                BaseDerivation,
                DeltaCurrentValue * magnitude,
                DeltaBaseValue * magnitude
            );
        }
        
        public SourcedModifiedAttributeValue Add(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                DeltaCurrentValue * modifiedAttributeValue.DeltaCurrentValue,
                DeltaBaseValue * modifiedAttributeValue.DeltaBaseValue
            );
        }
        
        public SourcedModifiedAttributeValue Override(float currentMagnitude, float baseMagnitude)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                currentMagnitude,
                baseMagnitude
            );
        }
        
        public SourcedModifiedAttributeValue Override(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
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
