using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem.Gasify
{
    public static class EditorTagService
    {
        public static Tag EDITOR_TAGS => Tag.Generate("E_EDITOR_TAGS");
        
        public static Tag EDITABLE => Tag.Generate("E_NO_EDIT");
        public static Tag IS_BUILT => Tag.Generate("E_IS_BUILT");
        public static Tag REQUIRES_REBUILD => Tag.Generate("E_REQUIRES_REBUILD");
        public static Tag MISSING_REFS => Tag.Generate("E_MISSING_REFS");
        public static Tag SEARCH_OPEN_IN_CREATOR => Tag.Generate("E_SEARCH_OPEN_IN_CREATOR");
        public static Tag SEARCH_OPEN_IN_DEVELOPER => Tag.Generate("E_SEARCH_OPEN_IN_DEVELOPER");
        public static Tag SAVED_NOT_BUILT => Tag.Generate("E_SAVED_NOT_BUILT");
        public static Tag UNSAVED_UNBUILT => Tag.Generate("E_UNSAVED_UNBUILT");

        public static GasifyEditorWindow.GasifyPage GetPageAfterSearchQuery(GasifyEditorWindow.GasifyPage activePage, GasifyDataNode node)
    {   
            switch (activePage)
            {
                case GasifyEditorWindow.GasifyPage.Home:
                    return GasifyEditorWindow.GasifyPage.Home;
                case GasifyEditorWindow.GasifyPage.Creator:
                    return node.editorTags.ContainsKey(SEARCH_OPEN_IN_CREATOR) ? GasifyEditorWindow.GasifyPage.Creator : GasifyEditorWindow.GasifyPage.Home;
                case GasifyEditorWindow.GasifyPage.Developer:
                    return node.editorTags.ContainsKey(SEARCH_OPEN_IN_DEVELOPER) ? GasifyEditorWindow.GasifyPage.Developer : GasifyEditorWindow.GasifyPage.Home;
                case GasifyEditorWindow.GasifyPage.Landing:
                default:
                    return GasifyEditorWindow.GasifyPage.Home;
            }
        }

        public static bool HasEditorTag(this GasifyDataNode node, Tag tag)
        {
            return node.editorTags.ContainsKey(tag);
        }

        public static void SetEditorTag(this GasifyDataNode node, Tag tag, object data)
        {
            node.editorTags[tag] = data;
        }
        
        #region Creator

        public static void NewlyCreatedTags(GasifyEditorWindow.CreatorItem item)
        {
            item.Data[EDITOR_TAGS] = new Dictionary<Tag, object>();
            if (item.Data[EDITOR_TAGS] is not Dictionary<Tag, object> editor) return;
            
            editor[UNSAVED_UNBUILT] = true;
            editor[SAVED_NOT_BUILT] = false;
            editor[IS_BUILT] = false;
            editor[REQUIRES_REBUILD] = false;
            editor[MISSING_REFS] = false;
            editor[EDITABLE] = true;
                
            editor[SEARCH_OPEN_IN_CREATOR] = true;
            editor[SEARCH_OPEN_IN_DEVELOPER] = false;
        }
        
        public static void Save(GasifyEditorWindow.CreatorItem item)
        {
            Debug.Log(item.Data);
            if (!item.Data.ContainsKey(EDITOR_TAGS)) return;
            if (item.Data[EDITOR_TAGS] is not Dictionary<Tag, object> editor) return;
            
            editor[UNSAVED_UNBUILT] = false;
            editor[SAVED_NOT_BUILT] = true;
            editor[IS_BUILT] = false;
            editor[REQUIRES_REBUILD] = false;
            editor[MISSING_REFS] = false;
            editor[EDITABLE] = true;
                
            editor[SEARCH_OPEN_IN_CREATOR] = true;
            editor[SEARCH_OPEN_IN_DEVELOPER] = false;
        }
        
        #endregion
    }
}
