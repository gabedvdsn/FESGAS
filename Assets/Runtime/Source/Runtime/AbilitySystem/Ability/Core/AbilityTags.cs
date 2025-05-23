﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public struct AbilityTags
    {
        [Header("Base")] 
        
        public GameplayTagScriptableObject AssetTag;
        public List<GameplayTagScriptableObject> ContextTags;
        
        [Header("Tags")]
        
        [Tooltip("Tags that are granted as long as this ability is learned")]
        public GameplayTagScriptableObject[] PassivelyGrantedTags;
        [Tooltip("Tags that are granted while this ability is active")]
        public GameplayTagScriptableObject[] ActiveGrantedTags;

        [Header("Requirements")]
        
        [Tooltip("Source requirements to use this ability")]
        public AvoidRequireTagGroup SourceRequirements;
        [Tooltip("Target requirements to use this ability (n/a for non-targeted abilities, e.g. ground cast)")]
        public AvoidRequireTagGroup TargetRequirements;

        public bool ValidateSourceRequirements(GASComponentBase source)
        {
            return SourceRequirements.Validate(source.TagCache.GetAppliedTags());
        }

        public bool ValidateTargetRequirements(GASComponentBase target)
        {
            return TargetRequirements.Validate(target.TagCache.GetAppliedTags());
        }
    }
}
