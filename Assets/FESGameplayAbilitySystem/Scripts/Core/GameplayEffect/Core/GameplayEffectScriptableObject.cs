using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Effect/Gameplay Effect", fileName = "GE_")]
    public class GameplayEffectScriptableObject : ScriptableObject
    {
        [Header("Gameplay Effect")]
        
        public GameplayTagScriptableObject Identifier;
        public GameplayTagScriptableObject[] GrantedTags;

        [Header("Specifications")] 
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public GameplayEffectDurationSpecification DurationSpecification;
        
        [Header("Effect Workers")]
        
        public List<AbstractEffectWorkerScriptableObject> Workers;
        
        [Header("Requirements")]
        
        public GameplayEffectRequirements SourceRequirements;
        public GameplayEffectRequirements TargetRequirements;

        [Header("Interactions")] 
        
        public GameplayTagScriptableObject[] RemoveEffectsWithTag;

        public GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponentBase target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, derivation, target);
            ApplyImpactSpecification(spec);

            return spec;
        }
        public void ApplyImpactSpecification(GameplayEffectSpec spec)
        {
            ImpactSpecification.ApplyImpactSpecifications(spec);
        }

        private void OnValidate()
        {
            if (DurationSpecification.UseDefaultTickRate && DurationSpecification.Duration > 0f)
            {
                DurationSpecification.TickOnApplication = false;
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * (1 / GASRateNormals.DEFAULT_TICK_PERIOD));
            }
        }

        public override string ToString()
        {
            return $"GE-{Identifier.Name}";
        }

    }

    public enum GameplayEffectApplicationPolicy
    {
        Append,  // Create another instance of the effect independent of the existing one(s)
        Refresh,  // Refresh the duration of the effect
        Extend,  // Extend the duration of the effect
        Stack,  // Inject a duration-independent stack of the effect into the existing one 
        StackRefresh,  // Stack and refresh the duration of each stack
        StackExtend  // Stacks and extend the duration of each stack
    }

    public interface IEffectDerivation
    {
        public GASComponentBase GetOwner();
        public List<GameplayTagScriptableObject> GetContextTags();
        public int GetLevel();
        public void SetLevel(int level);
        public float GetRelativeLevel();
        public string GetName();

        public static SourceEffectDerivation GenerateSourceDerivation(GASComponentBase source)
        {
            return new SourceEffectDerivation(source);
        }
    }

    public class SourceEffectDerivation : IEffectDerivation
    {
        private GASComponentBase Owner;

        public SourceEffectDerivation(GASComponentBase owner)
        {
            Owner = owner;
        }

        public GASComponentBase GetOwner()
        {
            return Owner;
        }
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return new List<GameplayTagScriptableObject>() { Owner.Identity.NameTag };
        }
        public int GetLevel()
        {
            return Owner.Identity.Level;
        }
        public void SetLevel(int level)
        {
            Owner.Identity.Level = level;
        }
        public float GetRelativeLevel()
        {
            return (Owner.Identity.Level - 1) / (float)(Owner.Identity.MaxLevel - 1);
        }
        public string GetName()
        {
            return Owner.Identity.DistinctName;
        }
    }

}
