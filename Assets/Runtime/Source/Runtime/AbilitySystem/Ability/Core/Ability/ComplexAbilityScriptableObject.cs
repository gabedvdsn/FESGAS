using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Ability/Complex Ability", fileName = "CPX_ABIL_")]
    public class ComplexAbilityScriptableObject : ScriptableObject, IAbilityData
    {

        public AbilityDefinition GetDefinition()
        {
            throw new System.NotImplementedException();
        }
        public AbilityTags GetTags()
        {
            throw new System.NotImplementedException();
        }
        public AbilityProxySpecification GetProxy()
        {
            throw new System.NotImplementedException();
        }
        public int GetStartingLevel()
        {
            throw new System.NotImplementedException();
        }
        public int GetMaxLevel()
        {
            throw new System.NotImplementedException();
        }
        public bool GetIgnoreWhenLevelZero()
        {
            throw new System.NotImplementedException();
        }
        public GameplayEffectScriptableObject GetCost()
        {
            throw new System.NotImplementedException();
        }
        public GameplayEffectScriptableObject GetCooldown()
        {
            throw new System.NotImplementedException();
        }
    }
}
