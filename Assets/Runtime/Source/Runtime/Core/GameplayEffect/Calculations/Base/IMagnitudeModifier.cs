using System;

namespace FESGameplayAbilitySystem
{
    public interface IMagnitudeModifier
    {
        public void Initialize(GameplayEffectSpec spec);
        public float Evaluate(GameplayEffectSpec spec);

        public static CustomMagnitudeModifier Generate()
        {
            return new CustomMagnitudeModifier(InitFunc, EvalFunc);

            void InitFunc(GameplayEffectSpec spec)
            {
            }

            float EvalFunc(GameplayEffectSpec spec) => spec.RelativeLevel;
        }

        public static CustomMagnitudeModifier Generate(Action<GameplayEffectSpec> initAction, Func<GameplayEffectSpec, float> evalFunc)
        {
            return new CustomMagnitudeModifier(initAction, evalFunc);
        }
        
    }

    public class CustomMagnitudeModifier : IMagnitudeModifier
    {
        private Action<GameplayEffectSpec> InitializationAction;
        private Func<GameplayEffectSpec, float> EvaluationFunc;

        public CustomMagnitudeModifier(Action<GameplayEffectSpec> initializationAction, Func<GameplayEffectSpec, float> evaluationFunc)
        {
            InitializationAction = initializationAction;
            EvaluationFunc = evaluationFunc;
        }

        public void Initialize(GameplayEffectSpec spec)
        {
            InitializationAction(spec);
        }
        public float Evaluate(GameplayEffectSpec spec)
        {
            return EvaluationFunc(spec);
        }
    }
}
