using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface IGameplayProcessHandler
    {
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler);
    }
}
