using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeLibrary : MonoBehaviour
    {
        public static AttributeLibrary Instance;

        [Header("Attribute Library")]
        
        [Tooltip("Compiles all Attributes within the Attribute Sets")]
        public List<AttributeSetScriptableObject> AttributeSets;
        [Tooltip("Compiles all Attributes, if they are not already present in an Attribute Set")]
        public List<AttributeScriptableObject> Attributes;

        private Dictionary<string, AttributeScriptableObject> Library;
        
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
            
            foreach (var set in AttributeSets)
            {
                foreach (var unique in set.GetUnique()) uniqueSet.Add(unique);
            }
            foreach (var attr in Attributes) uniqueSet.Add(attr);
            
            foreach (var attr in uniqueSet) Library[attr.Name.ToUpper()] = attr;
        }   

        public bool Contains(AttributeScriptableObject attribute) => Contains(attribute.Name);
        public bool Contains(string attrName) => Library.ContainsKey(attrName.ToUpper());
        
        public bool Add(AttributeScriptableObject attribute)
        {
            var _name = attribute.Name.ToUpper();
            if (Library.ContainsKey(_name)) return false;

            Library[_name] = attribute;
            return true;
        }

        public bool TryGetByName(string attrName, out AttributeScriptableObject attribute)
        {
            string _name = attrName.ToUpper();
            if (!Contains(_name))
            {
                attribute = default;
                return false;
            }

            attribute = Library[_name];
            return true;
        }
        
    }
}
