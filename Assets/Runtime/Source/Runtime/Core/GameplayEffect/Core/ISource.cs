using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface ISource : ITarget, IGameplayProcessHandler
    {
        public List<ITag> GetContextTags();
        public TagCache GetTagCache();
        public ITag GetAssetTag();
        public int GetLevel();
        public int GetMaxLevel();
        public void SetLevel(int level);
        public string GetName();
        public GameplayEffectDuration GetLongestDurationFor(GameplayTagScriptableObject[] lookForTags);
    }
    
    public interface  ITarget
    {
        public void CommunicateTargetedIntent(IDisjointableEntity entity);
        public void OnDisjoint(DisjointTarget disjointTarget);
        
        public GameplayTagScriptableObject GetAffiliation();
        public List<ITag> GetAppliedTags();
        public bool ApplyGameplayEffect(GameplayEffectSpec spec);
        public GameplayEffectSpec GenerateEffectSpec(IEffectDerivation derivation, IEffectBase GameplayEffect);
        public bool FindAttributeSystem(out AttributeSystemComponent attrSystem);
        public bool FindAbilitySystem(out AbilitySystemComponent abilSystem);
        public SystemComponentData AsData()
        {
            return new SystemComponentData(this);
        }
        public GASComponentBase AsGAS() => this is GASComponentBase gas ? gas : null;
        public AbstractTransformPacket AsTransform();
    }

    public struct SystemComponentData
    {
        public readonly AbilitySystemComponent AbilitySystem;
        public readonly AttributeSystemComponent AttributeSystem;

        public SystemComponentData(ITarget source)
        {
            source.FindAbilitySystem(out AbilitySystem);
            source.FindAttributeSystem(out AttributeSystem);
        }
    }
}
