using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractCachedAttributeMagnitudeModifier : AbstractMagnitudeModifierScriptableObject
    {
        public abstract void Regulate(IAttribute attribute, AttributeModificationRule rules);
    }

    public class AttributeModificationRule
    {
        private Dictionary<IAttribute, List<IAttribute>> matrix = new();

        public void RegisterRelation(IAttribute contact, IAttribute related)
        {
            matrix.SafeAdd(contact, related);
        }

        /// <summary>
        /// When attr is changed, we want re-init values of related attributes via modifier(s)
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="related"></param>
        /// <returns></returns>
        public bool RelatedAttributes(IAttribute attr, out List<IAttribute> related)
        {
            return matrix.TryGetValue(attr, out related);
        }
    }
}
