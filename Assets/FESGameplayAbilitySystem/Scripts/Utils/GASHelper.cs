﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace FESGameplayAbilitySystem
{
    public static class GASHelper
    {
        #region GAS Utils
        
        public static ESignPolicy SignPolicy(params float[] magnitudes)
        {
            float sum = magnitudes.Sum();
            return sum switch
            {
                > 0 => ESignPolicy.Positive,
                < 0 => ESignPolicy.Negative,
                0 when magnitudes.Any(mag => mag != 0) => ESignPolicy.ZeroBiased,
                _ => ESignPolicy.ZeroNeutral
            };
        }
        
        public static int SignInt(ESignPolicy signPolicy)
        {
            return signPolicy switch
            {

                ESignPolicy.Negative => -1,
                ESignPolicy.Positive => 1,
                ESignPolicy.ZeroBiased => 0,
                ESignPolicy.ZeroNeutral => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static float SignFloat(ESignPolicy signPolicy)
        {
            return signPolicy switch
            {

                ESignPolicy.Negative => -1f,
                ESignPolicy.Positive => 1f,
                ESignPolicy.ZeroBiased => 0f,
                ESignPolicy.ZeroNeutral => 0f,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }

        public static int AlignedSignInt(params ESignPolicy[] signPolicies)
        {
            return signPolicies.Aggregate(1, (current, signPolicy) => current * SignInt(signPolicy));
        }
        
        public static float AlignedSignFloat(params ESignPolicy[] signPolicies)
        {
            return signPolicies.Aggregate(1f, (current, signPolicy) => current * SignFloat(signPolicy));
        }

        public static int AlignToSign(int value, ESignPolicy signPolicy)
        {
            int _value = Mathf.Abs(value);
            return signPolicy switch
            {

                ESignPolicy.Negative => -_value,
                ESignPolicy.Positive => _value,
                ESignPolicy.ZeroBiased => _value,
                ESignPolicy.ZeroNeutral => _value,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static float AlignToSign(float value, ESignPolicy signPolicy)
        {
            float _value = Mathf.Abs(value);
            return signPolicy switch
            {

                ESignPolicy.Negative => -_value,
                ESignPolicy.Positive => _value,
                ESignPolicy.ZeroBiased => _value,
                ESignPolicy.ZeroNeutral => _value,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static AttributeValue AlignToSign(AttributeValue attributeValue, ESignPolicy signPolicy)
        {
            float _curr = Mathf.Abs(attributeValue.CurrentValue);
            float _base= Mathf.Abs(attributeValue.BaseValue);

            switch (signPolicy)
            {
                case ESignPolicy.Negative:
                    return new AttributeValue(-_curr, -_base);
                case ESignPolicy.Positive:
                case ESignPolicy.ZeroBiased:
                case ESignPolicy.ZeroNeutral:
                    return new AttributeValue(_curr, _base);
                default:
                    throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null);
            }
        }

        public static EImpactType FromImpactTypeAny(EImpactTypeAny impactType)
        {
            return impactType switch
            {

                EImpactTypeAny.NotApplicable => EImpactType.NotApplicable,
                EImpactTypeAny.Physical => EImpactType.Physical,
                EImpactTypeAny.Magical => EImpactType.Magical,
                EImpactTypeAny.Pure => EImpactType.Pure,
                EImpactTypeAny.Any => EImpactType.NotApplicable,
                _ => throw new ArgumentOutOfRangeException(nameof(impactType), impactType, null)
            };
        }

        public static bool ValidateImpactTypes(EImpactType impactType, EImpactTypeAny impactTypeAny)
        {
            return impactTypeAny switch
            {
                EImpactTypeAny.NotApplicable => impactType == EImpactType.NotApplicable,
                EImpactTypeAny.Physical => impactType == EImpactType.Physical,
                EImpactTypeAny.Magical => impactType == EImpactType.Magical,
                EImpactTypeAny.Pure => impactType == EImpactType.Pure,
                EImpactTypeAny.Any => true,
                _ => throw new ArgumentOutOfRangeException(nameof(impactTypeAny), impactTypeAny, null)
            };
        }

        public static bool ValidateImpactTargets(EEffectImpactTargetExpanded impactTarget, AttributeValue attributeValue, bool exclusive)
        {
            return impactTarget switch
            {
                EEffectImpactTargetExpanded.Current => attributeValue.CurrentValue != 0,
                EEffectImpactTargetExpanded.Base => attributeValue.BaseValue != 0,
                EEffectImpactTargetExpanded.CurrentAndBase => attributeValue.CurrentValue != 0 && attributeValue.BaseValue != 0,
                EEffectImpactTargetExpanded.CurrentOrBase => ValidateImpactTargets(EEffectImpactTargetExpanded.Current, attributeValue, exclusive) || ValidateImpactTargets(EEffectImpactTargetExpanded.Base, attributeValue, exclusive),
                _ => throw new ArgumentOutOfRangeException(nameof(impactTarget), impactTarget, null)
            };
        }

        public static bool ValidateSignPolicy(ESignPolicy signPolicy, EEffectImpactTargetExpanded impactTarget, AttributeValue attributeValue)
        {
            return impactTarget switch
            {

                EEffectImpactTargetExpanded.Current => SignPolicy(attributeValue.CurrentValue) == signPolicy,
                EEffectImpactTargetExpanded.Base => SignPolicy(attributeValue.BaseValue) == signPolicy,
                EEffectImpactTargetExpanded.CurrentAndBase => SignPolicy(attributeValue.CurrentValue) == signPolicy && SignPolicy(attributeValue.BaseValue) == signPolicy,
                EEffectImpactTargetExpanded.CurrentOrBase => SignPolicy(attributeValue.CurrentValue) == signPolicy || SignPolicy(attributeValue.BaseValue) == signPolicy,
                _ => throw new ArgumentOutOfRangeException(nameof(impactTarget), impactTarget, null)
            };
        }
        
        #endregion
        
        #region Utils
        
        public static T RandomChoice<T>(this List<T> list) => list is null ? default : list[Mathf.FloorToInt(Random.value * list.Count)];

        public static void SafeAdd<K, V>(this Dictionary<K, List<V>> dict, K key, V value)
        {
            if (dict.ContainsKey(key)) dict[key].Add(value);
            else dict[key] = new List<V>() { value };
        }

        public static void SafeAddRange<K, V>(this Dictionary<K, List<V>> dict, K key, List<V> values)
        {
            if (dict.ContainsKey(key)) dict[key].AddRange(values);
            else dict[key] = values;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Mathf.FloorToInt(Random.value * (n + 1));
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static List<T> WeightedSample<T>(this List<T> list, int N, List<float> weights)
        {
            float weightSum = weights.Sum();
            List<float> normalizedWeights = weights.Select(w => w / weightSum).ToList();
            List<T> selected = new List<T>();

            for (int _ = 0; _ < N; _++)
            {
                float randomValue = Random.value;
                float cumulative = 0f;

                for (int i = 0; i < list.Count; i++)
                {
                    cumulative += normalizedWeights[i];
                    if (randomValue <= cumulative)
                    {
                        selected.Add(list[i]);
                        break;
                    }
                }
            }

            return selected;
        }

        public static bool ContainsAll<T>(this List<T> list, List<T> match)
        {
            return match.All(list.Contains);
        }
        
        #endregion
    }
}
