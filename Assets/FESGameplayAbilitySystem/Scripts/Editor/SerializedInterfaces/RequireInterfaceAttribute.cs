using System;
using UnityEngine;

namespace FESGameplayAbilitySystem.Editor.SerializedInterfaces
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type IntefaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");
            IntefaceType = interfaceType;
        }
    }
}
