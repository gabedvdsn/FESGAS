using System.Collections;
using System.Collections.Generic;
using FESGameplayAbilitySystem;
using UnityEngine;

public interface IGameplayAbilitySystemActor
{
    public void GatherAbilitySystemComponent();
    public GASComponent GetAbilitySystemComponent();
}
