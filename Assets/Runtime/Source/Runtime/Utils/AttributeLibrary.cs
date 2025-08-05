using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeLibrary : MonoBehaviour
    {
        private static AttributeLibrary Instance;

        [Header("Attribute Library")]
        
        [Tooltip("Compiles all Attributes within the Attribute Sets")]
        public List<AttributeSetScriptableObject> AttributeSets;
        [Tooltip("Compiles all Attributes, if they are not already present in an Attribute Set")]
        public List<AttributeScriptableObject> Attributes;

        private Dictionary<string, AttributeScriptableObject> Library;

        /// <summary>
        /// REFACTOR THIS TO REFLECT YOUR ATTRIBUTE NAMING CONVENTION
        ///
        /// With respect to source code:
        /// Attributes should be named using logical spaces (as opposed to pascal or camel)
        /// This method replaces capitalizes and replaces spaces with underscores.
        /// </summary>
        /// <param name="attr">The stored name of the attribute</param>
        /// <returns></returns>
        private string RefactorByNamingConvention(string attr)
        {
            string _name = attr.ToUpper();
            _name = _name.Replace(' ', '_');
            return _name;
        }
        
        #region Internal
        
        private void Awake()
        {
            if (Instance is null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            
            Compile();
        }

        private void Compile()
        {
            Library = new Dictionary<string, AttributeScriptableObject>();
            
            var uniqueSet = new HashSet<AttributeScriptableObject>();
            
            foreach (var unique in AttributeSets.SelectMany(set => set.GetUnique()))
            {
                uniqueSet.Add(unique);
            }
            foreach (var attr in Attributes) uniqueSet.Add(attr);
            
            foreach (var attr in uniqueSet) Library[RefactorByNamingConvention(attr.Name)] = attr;
        }  

        public static bool Contains(AttributeScriptableObject attribute) => Contains(attribute.Name);
        public static bool Contains(string attrName) => Instance.Library.ContainsKey(attrName.ToUpper());
        
        public static bool Add(AttributeScriptableObject attribute)
        {
            var _name = attribute.Name.ToUpper();
            if (Instance.Library.ContainsKey(_name)) return false;

            Instance.Library[_name] = attribute;
            return true;
        }

        public static bool TryGetByName(string attrName, out AttributeScriptableObject attribute)
        {
            string _name = attrName.ToUpper();
            if (!Contains(_name))
            {
                attribute = default;
                return false;
            }

            attribute = Instance.Library[_name];
            return true;
        }
        
        #endregion
        
    }
}
