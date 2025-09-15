namespace FESGameplayAbilitySystem.Gasify
{
    public static class EditorTagService
    {
        public static Tag EDITOR_TAG => Tag.Generate("E_EDITOR_TAG");
        
        public static Tag NO_EDIT => Tag.Generate("E_NO_EDIT");
        public static Tag IS_BUILT => Tag.Generate("E_IS_BUILT");
        public static Tag REQUIRES_REBUILD => Tag.Generate("E_REQUIRES_REBUILD");
        public static Tag MISSING_REFS => Tag.Generate("E_MISSING_REFS");
        public static Tag SEARCH_OPEN_IN_CREATOR => Tag.Generate("E_SEARCH_OPEN_IN_CREATOR");
        public static Tag SEARCH_OPEN_IN_DEVELOPER => Tag.Generate("E_SEARCH_OPEN_IN_DEVELOPER");

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
    }
}
