using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ISource : ITarget, IGameplayProcessHandler
    {
        public List<GameplayTagScriptableObject> GetContextTags();
        public GameplayTagScriptableObject GetAssetTag();
        public int GetLevel();
        public int GetMaxLevel();
        public void SetLevel(int level);
        public string GetName();
        public GameplayTagScriptableObject GetAffiliation();
        public List<ITag> GetAppliedTags();
    }
    
    public interface ITarget
    {
        public bool ApplyGameplayEffect(GameplayEffectSpec spec);
        public bool ApplyGameplayEffect(IEffectDerivation derivation, IEffectBase GameplayEffect);
        public bool FindAttributeSystem(out AttributeSystemComponent attrSystem);
        public bool FindAbilitySystem(out AbilitySystemComponent abilSystem);
    }
}
