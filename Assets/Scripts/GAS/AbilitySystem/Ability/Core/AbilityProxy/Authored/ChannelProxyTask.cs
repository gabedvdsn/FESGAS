using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "New Channel Proxy Task", menuName = "FESGAS/Ability/Task/Channel")]
    public class ChannelProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public float ChannelDuration;
        
        public override UniTask Prepare(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            SliderManager.Instance.ToggleSlider(true);
            return base.Prepare(spec, target, token);
        }

        public override async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            float elapsedDuration = 0f;
            while (elapsedDuration < ChannelDuration)
            {
                elapsedDuration += Time.deltaTime;
                SliderManager.Instance.SetValue(elapsedDuration / ChannelDuration);

                await UniTask.NextFrame(token);
            }
        }
        
        public override async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            float elapsedDuration = 0f;
            while (elapsedDuration < ChannelDuration)
            {
                elapsedDuration += Time.deltaTime;
                SliderManager.Instance.SetValue(elapsedDuration / ChannelDuration);

                await UniTask.NextFrame(token);
            }
        }

        public override UniTask Clean(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            SliderManager.Instance.ToggleSlider(false);
            return base.Clean(spec, target, token);
        }

        private void OnValidate()
        {
            ReadOnlyDescription = "Holds a channeling state for the assigned duration, and updates the Slider value while doing so";
        }
    }
}
