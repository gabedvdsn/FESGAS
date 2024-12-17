using System.Collections;
using System.Collections.Generic;
using FESGameplayAbilitySystem;
using UnityEngine;

public interface IAbilityActor
{
    public void GatherAbilitySystemComponent();
    public AbilitySystemComponent GetAbilitySystemComponent();
}
