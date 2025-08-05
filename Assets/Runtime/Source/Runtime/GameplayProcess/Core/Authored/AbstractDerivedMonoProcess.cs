namespace FESGameplayAbilitySystem
{
    public class AbstractDerivedMonoProcess : LazyMonoProcess
    {
        protected IEffectDerivation Derivation;
        protected TargetGASData Source;

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            if (!regData.TryGetPayload(GameRoot.DerivationTag, AbilitySystemComponent.AbilityDerivationPolicy, EProxyDataValueTarget.Primary, out Derivation))
            {
                Derivation = GameRoot.Instance;
            }

            Source = Derivation.GetOwner().AsData();
        }
    }
}
