namespace FESGameplayAbilitySystem
{
    public struct SourcedModifiedAttributeValue
    {
        public IAttributeImpactDerivation Derivation;
        public IAttributeImpactDerivation BaseDerivation;
        
        public float CurrentValue;
        public float BaseValue;

        public bool Workable;

        public ESignPolicy SignPolicy => GASHelper.SignPolicy(CurrentValue, BaseValue);
        
        public SourcedModifiedAttributeValue(IAttributeImpactDerivation derivation, float currentValue, float baseValue, bool workable = true)
        {
            Derivation = derivation;
            BaseDerivation = derivation;
            
            CurrentValue = currentValue;
            BaseValue = baseValue;

            Workable = workable;
        }

        public SourcedModifiedAttributeValue(IAttributeImpactDerivation derivation, IAttributeImpactDerivation baseDerivation, float currentValue, float baseValue,
            bool workable = true)
        {
            Derivation = derivation;
            BaseDerivation = baseDerivation;
            
            CurrentValue = currentValue;
            BaseValue = baseValue;

            Workable = workable;
        }

        #region Helpers
        
        public SourcedModifiedAttributeValue Combine(SourcedModifiedAttributeValue other)
        {
            if (other.Derivation != Derivation)
            {
                return this;
            }
            
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                CurrentValue + other.CurrentValue, 
                BaseValue + other.BaseValue
            );
        }

        public SourcedModifiedAttributeValue Negate()
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                -CurrentValue,
                -BaseValue
            );
        }
        
        public ModifiedAttributeValue ToModified() => new(CurrentValue, BaseValue);

        public AttributeValue ToAttributeValue() => new(CurrentValue, BaseValue);
        
        #endregion
        
        #region Operations
        
        public SourcedModifiedAttributeValue Multiply(AttributeValue operand)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                CurrentValue * operand.CurrentValue,
                BaseValue * operand.BaseValue
            );
        }
        
        public SourcedModifiedAttributeValue Add(AttributeValue operand)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                CurrentValue + operand.CurrentValue,
                BaseValue + operand.BaseValue
            );
        }
        
        public SourcedModifiedAttributeValue Override(AttributeValue operand)
        {
            return new SourcedModifiedAttributeValue(
                Derivation,
                BaseDerivation,
                operand.CurrentValue,
                operand.BaseValue
            );
        }

        #endregion
        
        public override string ToString()
        {
            if (Derivation is null) return $"[ SMAV-INSTANT ] {CurrentValue}/{BaseValue}";
            return $"[ SMAV-{Derivation.GetEffectDerivation().GetName()} ] {CurrentValue}/{BaseValue}";
        }
    }
}
