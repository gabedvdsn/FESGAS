using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_Channel_", menuName = "FESGAS/Ability/Task/Channel")]
    public class ChannelProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public float ChannelDuration;
        
        public override void Prepare(ProxyDataPacket data)
        {
            SliderManager.Instance.ToggleSlider(true);
        }

        public override void Clean(ProxyDataPacket data)
        {
            SliderManager.Instance.ToggleSlider(false);
        }

        private void OnValidate()
        {
            ReadOnlyDescription = "Holds a channeling state for the assigned duration, and updates the Slider value while doing so";
        }
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            float elapsedDuration = 0f;
            while (elapsedDuration < ChannelDuration)
            {
                elapsedDuration += Time.deltaTime;
                SliderManager.Instance.SetValue(elapsedDuration / ChannelDuration);

                await UniTask.NextFrame(token);
            }
        }
    }
}
