using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeResponder : ScriptableObject
    {
        public abstract void WhenAttributeChangeResponse(GASComponent system, SourcedModifiedAttributeCache modifiedAttributeCache);
        public abstract void PostAttributeChangeResponse(GASComponent system, SourcedModifiedAttributeCache modifiedAttributeCache);
    }
}
