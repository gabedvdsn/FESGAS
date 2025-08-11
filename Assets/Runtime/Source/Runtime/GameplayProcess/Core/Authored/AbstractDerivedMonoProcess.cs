namespace FESGameplayAbilitySystem
{
    public class AbstractDerivedMonoProcess : LazyMonoProcess
    {
        protected IEffectDerivation Derivation;
        protected TargetGASData Source;

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            if (!regData.TryGet(ITag.Get(TagChannels.PAYLOAD_DERIVATION), EProxyDataValueTarget.Primary, out Derivation))
            {
                Derivation = GameRoot.Instance;
            }

            Source = Derivation.GetOwner().AsData();
        }
    }
}
