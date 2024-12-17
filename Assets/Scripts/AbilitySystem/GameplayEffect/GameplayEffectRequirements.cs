using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectRequirements
    {

        public AvoidRequireTagGroup ApplicationRequirements;  // These tags are required to apply the effect
        public AvoidRequireTagGroup OngoingRequirements;  // These tags are required to keep the effect ongoing
        public AvoidRequireTagGroup PurgeRequirements;  // These tags are required to purge the effect
    }
}
