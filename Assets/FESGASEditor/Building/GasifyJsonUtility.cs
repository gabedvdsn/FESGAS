using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace FESGameplayAbilitySystem.Gasify
{
    public static class GasifyJsonUtility
    {
        public const string CurrentSchema = "1.0.0";
        
        #region Parameters

        private const string attributes = "attributes";
        private const string tags = "tags";
        private const string abilities = "abilities";
        private const string proxyTasks = "proxyTasks";
        private const string effects = "effects";
        private const string gas = "gas";
        private const string attributeSets = "attributeSets";
        private const string modifiers = "modifiers";
        private const string attributeEvents = "attributeEvents";
        private const string impactWorkers = "impactWorkers";
        private const string effectWorkers = "effectWorkers";
        private const string tagWorkers = "tagWorkers";
        private const string processInstantiators = "processInstantiators";
        
        #endregion
        
        #region Saving
        
        public static void Save(FrameworkProject framework, string path)
        {
            var root = BuildDictionary(framework);
            var json = JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        static Dictionary<string, object> BuildDictionary(FrameworkProject p)
        {
            var root = new Dictionary<string, object>()
            {
                ["schemaVersion"] = CurrentSchema,
                ["meta"] = new Dictionary<string, object>()
                {
                    ["name"] = p.MetaName,
                    ["author"] = p.MetaAuthor,
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                },
                [attributes] = BuildAttributes(p),
                [tags] = BuildTags(p),
                [abilities] = BuildAbilities(p),
                [proxyTasks] = BuildProxyTasks(p),
                [effects] = BuildEffects(p),
                [gas] = BuildGAS(p),
                [attributeSets] = BuildAttributeSets(p),
                [modifiers] = BuildModifiers(p),
                [attributeEvents] = BuildAttributeEvents(p),
                [impactWorkers] = BuildImpactWorkers(p),
                [effectWorkers] = BuildEffectWorkers(p),
                [tagWorkers] = BuildTagWorkers(p),
                [processInstantiators] = BuildProcessInstantiators(p),
            };

            return root;
        }
        
        #region Builders (Saving)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="target"></param>
        static void BuildEditorTagsInPlace(GasifyDataNode node, Dictionary<Tag, object> target)
        {
            if (!node.hasEditorTags) return;

            object[] values = new object[node.editorTags.Count];
            int i = 0;
            foreach (Tag flag in node.editorTags.Keys)
            {
                values[i++] = new Dictionary<Tag, object>
                {
                    [flag] = node.editorTags[flag]
                };
            }

            target[EditorTagService.EDITOR_TAG] = values;
        }
        
        static List<Dictionary<string, object>> BuildAttributes(FrameworkProject p)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var o in p.Attributes)
            {
                var data = new Dictionary<string, object>
                {
                    ["id"] = o.Id,
                    ["name"] = o.Name,
                    ["description"] = o.Description
                };
                // BuildEditorTagsInPlace(o, data);
            }
            return list;
        }
        
        static List<Dictionary<string, object>> BuildTags(FrameworkProject p)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var o in p.Tags)
            {
                var data = new Dictionary<string, object>
                {
                    ["id"] = o.Id,
                    ["name"] = o.Name
                };
                // BuildEditorTagsInPlace(o, data);
            }
            return list;
        }
        
        static List<Dictionary<string, object>> BuildAbilities(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildProxyTasks(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildEffects(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildGAS(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildAttributeSets(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildModifiers(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildAttributeEvents(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildImpactWorkers(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildEffectWorkers(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildTagWorkers(FrameworkProject p)
        {
            return null;
        }
        
        static List<Dictionary<string, object>> BuildProcessInstantiators(FrameworkProject p)
        {
            return null;
        }
        
        #endregion
     
        #endregion

        #region Loading
        
        public static FrameworkProject Load(string path)
        {
            var json = File.ReadAllText(path);
            var token = JToken.Parse(json);
            ValidateSchema(token);
            token = RunMigrationsIfNeeded(token);
            return ParseFramework(token);
        }
        
        #region Validation
        
        static void ValidateSchema(JToken root)
        {
            if (root.Type != JTokenType.Object)
                throw new Exception("Root must be an object.");
            var ver = root.Value<string>("schemaVersion");
            if (string.IsNullOrEmpty(ver))
                throw new Exception("schemaVersion missing.");
        }

        static JToken RunMigrationsIfNeeded(JToken root)
        {
            return root;
            
            /*
            var ver = root.Value<string>("schemaVersion");
// Example: migrate 1.0.0 -> 1.1.0 renaming "description" -> "desc"
            if (ver == "1.0.0")
            {
                var abilities = root["abilities"] as JArray;
                if (abilities != null)
                {
                    foreach (var a in abilities)
                    {
                        var desc = a["description"];
                        if (desc != null && a["desc"] == null)
                        {
                            a["desc"] = desc;
                            a["description"] = null;
                        }
                    }
                }
                ((JObject)root)["schemaVersion"] = CurrentSchema;
            }
            return root;*/
        }
        
        #endregion

        static FrameworkProject ParseFramework(JToken root)
        {
            var p = new FrameworkProject();

            ParseAttributes(p, root);
            ParseTags(p, root);
            ParseAbilities(p, root);
            ParseProxyTasks(p, root);
            ParseEffects(p, root);
            ParseEntities(p, root);
            ParseAttributeSets(p, root);
            ParseModifiers(p, root);
            ParseAttributeEvents(p, root);
            ParseImpactWorkers(p, root);
            ParseEffectWorkers(p, root);
            ParseTagWorkers(p, root);
            ParseProcessInstantiators(p, root);

            return p;
        }
        
        #region Parsers

        static void ParseAttributes(FrameworkProject p, JToken root)
        {
            if (root[attributes] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseTags(FrameworkProject p, JToken root)
        {
            if (root[tags] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new TagData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name"),
                    // Parent = t.Value<string>("parent")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseAbilities(FrameworkProject p, JToken root)
        {
            if (root[abilities] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseProxyTasks(FrameworkProject p, JToken root)
        {
            if (root[proxyTasks] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseEffects(FrameworkProject p, JToken root)
        {
            if (root[effects] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseEntities(FrameworkProject p, JToken root)
        {
            if (root[gas] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseAttributeSets(FrameworkProject p, JToken root)
        {
            if (root[attributeSets] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseModifiers(FrameworkProject p, JToken root)
        {
            if (root[modifiers] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseAttributeEvents(FrameworkProject p, JToken root)
        {
            if (root[attributeEvents] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseImpactWorkers(FrameworkProject p, JToken root)
        {
            if (root[impactWorkers] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseEffectWorkers(FrameworkProject p, JToken root)
        {
            if (root[effectWorkers] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseTagWorkers(FrameworkProject p, JToken root)
        {
            if (root[tagWorkers] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }
        
        static void ParseProcessInstantiators(FrameworkProject p, JToken root)
        {
            if (root[processInstantiators] is not JArray arr) return;
            
            foreach (var t in arr)
            {
                var data = new AttributeData()
                {
                    Id = t.Value<string>("id"),
                    Name = t.Value<string>("name")
                };
                ParseEditorTags(data, t);
            }
        }

        static void ParseEditorTags(GasifyDataNode node, JToken local)
        {
            /*if (local[editorTags] is not JArray eTags) return;

            foreach (var eTagToken in eTags)
            {
                string eTag = eTagToken.Value<string>();
                if (eTag is null) continue;

                switch (eTag)
                {
                    case et_noEdit:
                        node.editorTags[et_noEdit] = eTags.Value<bool>();
                        break;
                    case et_description:
                        node.editorTags[et_description] = eTags.Value<string>();
                        break;
                }
            }*/
        }
        
        #endregion
        
        #endregion
        
    }
}
