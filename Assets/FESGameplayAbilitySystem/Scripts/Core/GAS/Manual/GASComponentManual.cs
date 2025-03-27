using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponentManual))]
    [RequireComponent(typeof(AbilitySystemComponentManual))]
    public class GASComponentManual : GASComponentBase
    {
        [Header("Tag Workers")]
        
        public List<AbstractTagWorkerScriptableObject> Workers;
        
        protected override void PrepareSystem()
        {
            TagCache = new TagCache(this, Workers);
        }
    }
}