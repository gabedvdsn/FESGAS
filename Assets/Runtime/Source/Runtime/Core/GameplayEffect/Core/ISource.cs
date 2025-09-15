using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface ISource : ITarget, IGameplayProcessHandler
    {
        public Tag[] GetContextTags();
        public TagCache GetTagCache();
        public Tag GetAssetTag();
        public int GetLevel();
        public int GetMaxLevel();
        public void SetLevel(int level);
        public string GetName();
        public GameplayEffectDuration GetLongestDurationFor(Tag lookForTag);
        public GameplayEffectDuration GetLongestDurationFor(Tag[] lookForTags);
    }
    
    public interface  ITarget
    {
        public void CommunicateTargetedIntent(IDisjointableEntity entity);
        public void OnDisjoint(DisjointTarget disjointTarget);
        
        public Tag GetAffiliation();
        public Tag[] GetAppliedTags();
        public bool ApplyGameplayEffect(GameplayEffectSpec spec);
        public GameplayEffectSpec GenerateEffectSpec(IEffectOrigin origin, GameplayEffect GameplayEffect);
        public bool FindAttributeSystem(out AttributeSystemComponent attrSystem);
        public bool FindAbilitySystem(out AbilitySystemComponent abilSystem);
        public SystemComponentData AsData()
        {
            return new SystemComponentData(this);
        }
        public GASComponent AsGAS() => this is GASComponent gas ? gas : null;
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
