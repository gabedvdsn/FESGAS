using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlasticGui.WorkspaceWindow.Home;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;

/*
 * Attributions:
 * - Home Icon -> <a href="https://www.flaticon.com/free-icons/home-button" title="home button icons">Home button icons created by Freepik - Flaticon</a>
 * - Fireball -> <a href="https://www.flaticon.com/free-icons/fireball" title="fireball icons">Fireball icons created by Vectorslab - Flaticon</a>
 * 
 */

namespace FESGameplayAbilitySystem.Gasify
{
    #if UNITY_EDITOR
    public class GasifyEditorWindow : EditorWindow
    {
        
        #region Wrappers
        
        class SearchItem
        {
            public string Display;
            public string Id;
            public DataType Kind;
            public bool IsPlaceholder;
            public int KindOrder => (int)Kind;
            public override string ToString() => Display;

            public SearchItem(string display, string id, DataType kind, bool isPlaceholder = false)
            {
                Display = display;
                Id = id;
                Kind = kind;
                IsPlaceholder = isPlaceholder;
            }
        }

        // Menu entry model for multi-column dropdowns
        class MenuEntry
        {
            public Texture2D Icon;
            public string Text;
            public Action OnClick;
            public MenuEntry(Texture2D icon, string text, Action onClick) { Icon = icon; Text = text; OnClick = onClick; }
        }
        
        #endregion
        
        public enum GasifyPage { Landing, Home, Creator, Developer }

        public enum DataType
        {
            Ability = 2, 
            Effect = 3, 
            Entity = 1, 
            Attribute = 4, 
            Tag = 5, 
            ProxyTask = 7,
            AttributeSet = 8, 
            Modifier = 6, 
            AttributeWorker = 9, 
            ImpactWorker = 19, 
            EffectWorker = 11, 
            TagWorker = 12, 
            ProcessInstantiator = 13,
            None = 14
        }

        static string DataTypeText(DataType kind)
        {
            var header = kind switch
            {
                DataType.Ability   => "Ability",
                DataType.Effect    => "Effect",
                DataType.Entity    => "System",
                DataType.Attribute => "Attribute",
                DataType.Tag       => "Tag",
                DataType.ProxyTask       => "Proxy Task",
                DataType.AttributeSet       => "Attribute Set",
                DataType.Modifier       => "Modifier",
                DataType.AttributeWorker       => "Attribute Worker",
                DataType.ImpactWorker       => "Impact Worker",
                DataType.EffectWorker       => "Effect Worker",
                DataType.TagWorker       => "Tag Worker",
                DataType.ProcessInstantiator       => "Process Instantiator",
                _                  => kind.ToString()
            };
            return header;
        }
        
        static string DataTypeTextPlural(DataType kind)
        {
            var header = kind switch
            {
                DataType.Ability   => "Abilities",
                DataType.Effect    => "Effects",
                DataType.Entity    => "Systems",
                DataType.Attribute => "Attributes",
                DataType.Tag       => "Tags",
                DataType.ProxyTask       => "Proxy Tasks",
                DataType.AttributeSet       => "Attribute Sets",
                DataType.Modifier       => "Modifiers",
                DataType.AttributeWorker       => "Attribute Workers",
                DataType.ImpactWorker       => "Impact Workers",
                DataType.EffectWorker       => "Effect Workers",
                DataType.TagWorker       => "Tag Workers",
                DataType.ProcessInstantiator       => "Process Instantiators",
                _                  => kind.ToString()
            };
            return header;
        }
        
        private static GasifyPage activePage = GasifyPage.Landing;
        
        private const string RootTitle = "FESGAS";
        
        private const string LandingPageTitle = RootTitle;
        private const string HomePageTitle = RootTitle + " — Home";
        private const string CreatePageTitle = RootTitle + " — Create";
        private const string DevelopPageTitle = RootTitle + " — Develop";

        private Color PrimaryButtonColor => new Color(.36f, .32f, .7f);
        private Color PrimaryButtonColorHover => new Color(.46f, .42f, .8f);
        private Color SecondaryButtonColor => new Color(.46f, .44f, .55f);
        private Color SecondaryButtonColorHover => new Color(.56f, .54f, .65f);
        private Color TertiaryButtonColor => new Color(.77f, .75f, .85f);
        private Color TertiaryButtonColorHover => new Color(.87f, .85f, .95f);
        
        private Color BackgroundColorDark => new Color(0.17f, 0.17f, 0.2f, 1f);
        private Color BackgroundColorDeepDark => new Color(0.12f, 0.12f, 0.14f, 1f);
        private Color BackgroundColorLight => new Color(0.27f, 0.27f, 0.3f, 1f);
        private Color BackgroundColorDeepLight => new Color(0.42f, 0.42f, 0.45f, 1f);
        private Color BackgroundColorDropdown => new Color(0.22f, 0.22f, 0.26f, 1f);
        private Color BackgroundColorSub => new Color(0.4f, 0.4f, 0.5f, 1f);
        private Color EdgeColorDark => new Color(0, 0, 0, .5f);

        private Color TextColorLight => new Color(0.85f, 0.85f, 0.9f, 1f);
        private Color TextColorSub => new Color(0.75f, 0.75f, 0.75f, 0.95f);
        
        // Sizing
        private const float DropdownColumnWidthSmall = 160f;
        private const float DropdownColumnWidth = 200f;
        private const float DropdownColumnWidthLarge = 260f;
        private const float ListviewMinHeight = 220f;
        private const float ListviewItemMinHeight = 22f;

        #region Internal
        
        // Project / data management
        private FrameworkProject Project = new FrameworkProject();
        private GasifyProjectIndex Index = new GasifyProjectIndex();
        private GasifyNavigationService Navigation;

        private Dictionary<DataType, List<(string, string)>> projectData;

        private IList GetProjectItems(DataType data)
        {
            return data switch
            {
                DataType.Ability => Project.Abilities,
                DataType.Effect => Project.Effects,
                DataType.Entity => Project.Entities,
                DataType.Attribute => Project.Attributes,
                DataType.Tag => Project.Tags,
                DataType.ProxyTask => Project.ProxyTasks,
                DataType.AttributeSet => Project.AttributeSets,
                DataType.Modifier => Project.Modifiers,
                DataType.AttributeWorker => Project.AttributeEvents,
                DataType.ImpactWorker => Project.ImpactWorkers,
                DataType.EffectWorker => Project.EffectWorkers,
                DataType.TagWorker => Project.TagWorkers,
                DataType.ProcessInstantiator => Project.ProcessInstantiators,
                DataType.None => new List<byte>(),
                _ => throw new ArgumentOutOfRangeException(nameof(data), data, null)
            };
        }
        
        #region Page Elements
        
        private VisualElement content;
        private VisualElement overlay;
        
        #region Navigation Bar
        
        // Navigation bar
        private VisualElement navBar;
        
        // Home
        private VisualElement nav_homeButton;
        
        // Create dropdown
        private VisualElement nav_createButton;
        private bool _nav_overCreateButton, _nav_overCreateDD;
        private bool nav_overCreateButton, nav_overCreateDD;
        private VisualElement nav_createDropdown;
        private int nav_createHoverCount;

        enum ProxyRowType { Header, RecentItem, Separator, TaskItem, Placeholder }
        private TextField nav_proxySearchField;
        private ListView nav_proxyListView;
        private List<ProxyRow> nav_recentProxyTasks = new();
        private const int nav_maxRecentProxy = 6;

        class ProxyRow
        {
            public ProxyRowType Type;
            public string Text;
            public DataType DataType;
            public string Id;
            public bool IsPlaceholder;
            private ProxyRow(ProxyRowType type, string text, DataType dataType, string id, bool isPlaceholder)
            {
                Type = type;
                Text = text;
                DataType = dataType;
                Id = id;
                IsPlaceholder = isPlaceholder;
            }

            public static ProxyRow TaskItem(ProxyTaskData data) => new(ProxyRowType.TaskItem, data.Name, DataType.ProxyTask, data.Id, false);
            public static ProxyRow Header(string text) => new(ProxyRowType.Header, text, DataType.None, "-1", true);
            public static ProxyRow Separator() => new(ProxyRowType.Separator, "", DataType.None, "-1", true);
            public static ProxyRow Placeholder(string text) => new(ProxyRowType.Placeholder, text, DataType.None, "-1", true);
        }
        
        // Develop dropdown
        private VisualElement nav_developButton;
        private bool nav_overDevelopButton, nav_overDevelopDD;
        private VisualElement nav_developDropdown;
        private int nav_developHoverCount;
        
        // Search
        private VisualElement nav_searchDropdown;
        private TextField nav_searchField;
        private ListView nav_searchList;
        private List<SearchItem> nav_searchItems = new();
        private bool _nav_overSearchField, _nav_overSearchDD;

        private bool nav_overSearchField
        {
            get => _nav_overSearchField;
            set
            {
                Debug.Log($"search over: old {_nav_overSearchField} new {value}");
                _nav_overSearchField = value;
            }
        }
        private bool nav_overSearchDD
        {
            get => _nav_overSearchDD;
            set
            {
                Debug.Log($"search dd over: old {_nav_overSearchDD} new {value}");
                _nav_overSearchDD = value;
            }
        }
        
        // Options
        private VisualElement nav_optionsButton;
        private bool nav_overOptionsButton, nav_overOptionsDD;
        private VisualElement nav_optionsDropdown;
        
        #endregion
        
        // Pages
        private VisualElement landingPage;
        
        #region Home Page
        
        private VisualElement homePage;
        
        // Project view
        private VisualElement home_projectView;
        private TextField home_projectSearchField;
        private Button home_projectFilterButton;
        private ListView home_projectList;
        
        enum PVRowType { Header, Item, Placeholder }

        class PVRow
        {
            public PVRowType Type;
            public DataType DataType;
            public string Id;
            public string Name;
            public bool Collapsed;

            public PVRow()
            {
            }

            public PVRow(PVRowType type, DataType dataType, string id, string name, bool collapsed = false)
            {
                Type = type;
                DataType = dataType;
                Id = id;
                Name = name;
                Collapsed = collapsed;
            }
        }

        private readonly Dictionary<DataType, bool> home_pvSectionCollapsed = new();
        private readonly HashSet<DataType> home_pvTypeFilter = new();
        private bool home_pvFilterInUse = false;
        private bool home_pvOnlyPopulated = true;
        private List<PVRow> home_pvRows = new();
        
        // Usage map
        private VisualElement home_usageMap;
        
        #endregion
        
        #region Creator Page
        
        private VisualElement creatorPage;
        
        private VisualElement creator_mode;
        private VisualElement creator_stacktrace;
        private VisualElement creator_actions;
        private VisualElement creator_usage;
        private VisualElement creator_editor;

        private const float Creator_LeftColWidth = 260f;
        private const float Creator_BarHeight    = 28f;

        private CreatorItem creator_active;

        public class CreatorItem
        {
            public string Name;
            public string Id;
            public DataType Kind;
            public Dictionary<Tag, object> Data;

            public CreatorItem(string name, string id, DataType kind)
            {
                Name = name;
                Id = id;
                Kind = kind;
                Data = new Dictionary<Tag, object>();
            }

            public static CreatorItem Fresh(DataType kind)
            {
                var item = new CreatorItem($"Unnamed {DataTypeText(kind)}", "-1", kind);
                EditorTagService.NewlyCreatedTags(item);
                return item;
            }
        }
        
        #endregion
        
        private VisualElement developerPage;
        
        #endregion
        
        #region Icons

        private Texture2D icon_HOME;
        private Texture2D icon_RECENT;
        private Texture2D icon_ABILITY;
        private Texture2D icon_EFFECT;
        private Texture2D icon_SYSTEM;
        private Texture2D icon_ATTRIBUTE;
        private Texture2D icon_TAG;
        private Texture2D icon_ATTRIBUTE_SET;
        private Texture2D icon_MODIFIER;
        private Texture2D icon_IMPACT;
        private Texture2D icon_PROCESS;
        
        #endregion

        private bool OverlayActive => overlay.style.display == DisplayStyle.Flex;
        
        #endregion

        private Toolbar toolbar;
        private ToolbarMenu filterMenu;
        
        
        [MenuItem("Tools/Gasify")]
        private static void ShowWindow()
        {
            var window = GetWindow<GasifyEditorWindow>(LandingPageTitle);
            window.minSize = new Vector2(640, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            activePage = Project.Loaded ? GasifyPage.Home : GasifyPage.Landing;
            
            Reset();

            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1;
            
            rootVisualElement.style.paddingLeft = 0;
            rootVisualElement.style.paddingRight = 0;
            rootVisualElement.style.paddingTop = 0;
            rootVisualElement.style.paddingBottom = 0;

            LoadIcons();
            
            navBar = BuildNavigationBar();
            content = BuildContent();
            overlay = BuildOverlay();
            
            // === Build dropdown panels (Create / Develop / Search) ===
            nav_createDropdown  = BuildCreateDropdown_Nav();
            nav_developDropdown = BuildDevelopDropdown_Nav();
            nav_searchDropdown  = BuildSearchDropdown_Nav();

            // dropdowns live at the same level as the bar so they can overlap content
            overlay.Add(nav_createDropdown);
            overlay.Add(nav_developDropdown);
            overlay.Add(nav_searchDropdown);
            
            landingPage = BuildLandingPage();
            homePage = BuildHomePage();
            creatorPage = BuildCreatorPage();
            developerPage = BuildDeveloperPage();
            
            rootVisualElement.Add(navBar);
            
            content.Add(landingPage);
            content.Add(homePage);
            content.Add(creatorPage);
            content.Add(developerPage);

            rootVisualElement.Add(content);
            rootVisualElement.Add(overlay);
            
            rootVisualElement.RegisterCallback<MouseDownEvent>(_ => CloseAllDropdowns());
            
            /*navBar.RegisterCallback<MouseUpEvent>(_ => CloseAllDropdowns());
            content.RegisterCallback<MouseUpEvent>(_ => CloseAllDropdowns());
            overlay.RegisterCallback<MouseUpEvent>(_ => CloseAllDropdowns());*/
            
            SetPage(activePage);
        }
        
        private void LoadIcons()
        {
            icon_HOME = LoadFESGASIcon("home.png");
            icon_RECENT = LoadFESGASIcon("recent.png");
        }

        private void Reset()
        {
            nav_searchItems.Clear();
            nav_recentProxyTasks.Clear();
            
            LoadTestData();

            LoadProjectData();

            Debug.Log($"RESET FESGASIFY EDITOR");
        }

        private void LoadProjectData()
        {
            projectData = Project.GetCompleteDescriptions();
            // TODO order keys by alphabetical order
            // projectData = projectData.OrderByDescending(t => t.Key.ToString()).ToDictionary();
        }

        void LoadTestData()
        {
            Project = new FrameworkProject();
            
            Project.Attributes.Add(new AttributeData() { Id = "Health", Name = "Health" });
            Project.Attributes.Add(new AttributeData() { Id = "Mana", Name = "Mana" });
            Project.Attributes.Add(new AttributeData() { Id = "Speed", Name = "Speed" });
            
            Project.Tags.Add(new TagData() { Id = "CanMove", Name = "CanMove" });
            Project.Tags.Add(new TagData() { Id = "CanAttack", Name = "CanAttack" });
            
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "CreateProcess", Name = "CreateProcess" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "ApplyEffect", Name = "ApplyEffect" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "DestroyProcess", Name = "DestroyProcess" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "PurgeEffects", Name = "PurgeEffects" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "A", Name = "A" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "B", Name = "B" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "C", Name = "C" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "D", Name = "D" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "E", Name = "E" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "F", Name = "F" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "G", Name = "G" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "H", Name = "H" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "I", Name = "I" });
            Project.ProxyTasks.Add(new ProxyTaskData() { Id = "J", Name = "J" });
        }
        
        #region Helpers
        
        void SetElement(VisualElement ve, bool flag)
        {
            ve.style.display = flag ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        private void SetPage(GasifyPage page)
        {
            if (page == GasifyPage.Landing) Reset();
            
            activePage = page;

            // Show nav only on Home/Creator/Developer
            bool showNav = page is GasifyPage.Home or GasifyPage.Creator or GasifyPage.Developer;
            if (navBar != null)
                navBar.style.display = showNav ? DisplayStyle.Flex : DisplayStyle.None;

            landingPage.style.display   = page == GasifyPage.Landing   ? DisplayStyle.Flex : DisplayStyle.None;
            homePage.style.display      = page == GasifyPage.Home      ? DisplayStyle.Flex : DisplayStyle.None;
            creatorPage.style.display   = page == GasifyPage.Creator   ? DisplayStyle.Flex : DisplayStyle.None;
            developerPage.style.display = page == GasifyPage.Developer ? DisplayStyle.Flex : DisplayStyle.None;
        }

        Texture2D LoadFESGASIcon(string file, string path = "Assets/FESGASEditor/Icons/")
        {
            #if UNITY_EDITOR
            var t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(path + file);
            return t2d;
#else
            return null;
#endif
        }
        
        #region Buttons
        
        Button PrimaryButton(string text, System.Action onClick, int height = 28, int radius = 8)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            b.style.height = height;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = PrimaryButtonColor; // purple-ish
            b.style.color = TextColorLight;
            b.style.borderTopLeftRadius = radius;
            b.style.borderTopRightRadius = radius;
            b.style.borderBottomLeftRadius = radius;
            b.style.borderBottomRightRadius = radius;
            b.RegisterCallback<PointerEnterEvent>(_ => b.style.backgroundColor = PrimaryButtonColorHover);
            b.RegisterCallback<PointerLeaveEvent>(_ => b.style.backgroundColor = PrimaryButtonColor);
            return b;
        }
        
        Button SecondaryButton(string text, System.Action onClick, int height = 24, int radius = 8)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            b.style.height = height;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = SecondaryButtonColor;
            b.style.color = TextColorLight;
            b.style.borderTopLeftRadius = radius;
            b.style.borderTopRightRadius = radius;
            b.style.borderBottomLeftRadius = radius;
            b.style.borderBottomRightRadius = radius;
            b.RegisterCallback<PointerEnterEvent>(_ => b.style.backgroundColor = SecondaryButtonColorHover);
            b.RegisterCallback<PointerLeaveEvent>(_ => b.style.backgroundColor = SecondaryButtonColor);
            return b;
        }
        
        Button TertiaryButton(string text, System.Action onClick, int height = 24, int radius = 8)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            b.style.height = height;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = TertiaryButtonColor;
            b.style.color = TextColorLight;
            b.style.borderTopLeftRadius = radius;
            b.style.borderTopRightRadius = radius;
            b.style.borderBottomLeftRadius = radius;
            b.style.borderBottomRightRadius = radius;
            b.RegisterCallback<PointerEnterEvent>(_ => b.style.backgroundColor = TertiaryButtonColorHover);
            b.RegisterCallback<PointerLeaveEvent>(_ => b.style.backgroundColor = TertiaryButtonColor);
            return b;
        }
        
        Button ActionButtonFill(string text, System.Action onClick)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            // Fill the parent's height (row cross-axis)
            b.style.alignSelf = Align.Stretch;
            b.style.flexGrow = 1;                 // share space with siblings
            b.style.height = StyleKeyword.Auto;   // no fixed height

            // Visuals to match your theme
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.backgroundColor = BackgroundColorLight;
            b.style.color = TextColorLight;
            b.style.borderTopLeftRadius = 6; b.style.borderTopRightRadius = 6;
            b.style.borderBottomLeftRadius = 6; b.style.borderBottomRightRadius = 6;

            b.RegisterCallback<PointerEnterEvent>(_ => b.style.backgroundColor = BackgroundColorLight * .8f);
            b.RegisterCallback<PointerLeaveEvent>(_ => b.style.backgroundColor = BackgroundColorLight);

            return b;
        }
        
        #endregion
        
        #region Events

        void TrackHover(VisualElement ve, Action<bool> onHover)
        {
            ve.RegisterCallback<PointerEnterEvent>(_ => onHover?.Invoke(true));
            ve.RegisterCallback<PointerLeaveEvent>(_ => onHover?.Invoke(false));
        }
        
        private void Delay(float seconds, Action action)
        {
            double target = EditorApplication.timeSinceStartup + seconds;
            EditorApplication.update += Tick;
            return;

            void Tick()
            {
                if (EditorApplication.timeSinceStartup >= target)
                {
                    EditorApplication.update -= Tick;
                    action?.Invoke();
                }
            }
        }
        
        void AttachHoverGroup(VisualElement button, VisualElement dropdown)
        {
            if (button == null || dropdown == null) return;

            int hoverCount = 0; // local to this group

            void Inc(PointerEnterEvent _) { hoverCount++; Debug.Log($"{button.name} inc {hoverCount}"); Debug.Log($"{dropdown.name} inc {hoverCount}"); }
            void Dec(PointerLeaveEvent _) { hoverCount--; Debug.Log($"{button.name} dec {hoverCount}"); Debug.Log($"{dropdown.name} dec {hoverCount}");  DebouncedMaybeHide(dropdown, () => hoverCount <= 0); }

            button.RegisterCallback<PointerEnterEvent>(Inc);
            button.RegisterCallback<PointerLeaveEvent>(Dec);

            dropdown.RegisterCallback<PointerEnterEvent>(Inc);
            dropdown.RegisterCallback<PointerLeaveEvent>(Dec);
        }

        // ~120ms debounce so pointer can cross tiny gaps
        void DebouncedMaybeHide(VisualElement dropdown, Func<bool> shouldHide)
        {
            Delay(0.12f, () =>
            {
                if (dropdown == null) return;
                if (shouldHide()) dropdown.style.display = DisplayStyle.None;
            });
        }
        
        #endregion
        
        #region Dropdown

        void CloseAllDropdowns()
        {
            CloseAllDropdowns_Navigation();

            SetElement(overlay, false);
        }
        
        void CloseAllDropdowns_Navigation()
        {
            nav_createDropdown.style.display = DisplayStyle.None;
            nav_developDropdown.style.display = DisplayStyle.None;
            nav_searchDropdown.style.display = DisplayStyle.None;
        }

        VisualElement NewDropdownBase(float minWidth)
        {
            var ve = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    display  = DisplayStyle.None,
                    backgroundColor = BackgroundColorDropdown,
                    borderTopLeftRadius = 6, borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6, borderBottomRightRadius = 6,
                    borderBottomWidth = 1, borderTopWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderBottomColor = EdgeColorDark, borderTopColor = EdgeColorDark,
                    borderLeftColor   = EdgeColorDark, borderRightColor = EdgeColorDark,
                    paddingTop = 4, paddingBottom = 6, paddingLeft = 6, paddingRight = 6,
                    minWidth = minWidth
                }
            };
            return ve;
        }
        
        VisualElement NewColumn(float flexGrow = 1)
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column, 
                    flexGrow = flexGrow, 
                    marginRight = 8
                }
            };
        }
        
        VisualElement NewColumn(float width, float minWidth, float maxWidth, float flexGrow = 1)
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column, 
                    flexGrow = flexGrow, 
                    marginRight = 8,
                    width = width,
                    minWidth = minWidth,
                    maxWidth = maxWidth
                }
            };
        }

        Label BuildHeader(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white,
                    marginTop = 4, marginBottom = 4,
                    paddingLeft = 4
                }
            };
        }

        VisualElement BuildSeparator(float thickness = 2f)
        {
            return new VisualElement
            {
                style = {
                    height = thickness,
                    backgroundColor = new Color(0f,0f,0f,0.35f),
                    marginTop = 4, marginBottom = 4
                }
            };
        }
        
        VisualElement BuildIconTextItem(MenuEntry entry)
        {
            var item = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = 26,
                    paddingLeft = 6, paddingRight = 6,
                    marginBottom = 2
                }
            };

            var icon = new Image
            {
                image = entry.Icon,
                scaleMode = ScaleMode.ScaleToFit,
                style = { width = 16, height = 16, marginRight = 6 }
            };
            item.Add(icon);

            var label = new Label(entry.Text)
            {
                style = {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    color = new Color(0.95f,0.95f,1f,1f),
                    flexGrow = 1
                }
            };
            item.Add(label);

            // Hover highlight
            TrackHover(item, over => { item.style.backgroundColor = over ? new Color(0.20f,0.20f,0.24f,1f) : new StyleColor(); });

            // Click
            item.RegisterCallback<MouseUpEvent>(_ =>
            {
                entry.OnClick?.Invoke();
                // Close menus after click
                nav_createDropdown.style.display  = DisplayStyle.None;
                nav_developDropdown.style.display = DisplayStyle.None;
            });

            return item;
        }
        
        #endregion
        
        #endregion
        
        #region Navigation Bar
        
        #region Helpers
        
        // A consistently styled nav button matching your palette
        Button MakeNavButton(string text, Action onClick, Color color, Color hoverColor, string tooltip = null)
        {
            var btn = new Button(() => onClick?.Invoke()) { text = text };
            btn.tooltip = string.IsNullOrEmpty(tooltip) ? string.Empty : tooltip;
            btn.style.height = 26;
            btn.style.marginRight = 6;
            btn.style.paddingLeft = 10; btn.style.paddingRight = 10;
            btn.style.backgroundColor = color;
            btn.style.color = TextColorLight;
            btn.style.borderTopLeftRadius = 8; btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8; btn.style.borderBottomRightRadius = 8;
            btn.RegisterCallback<PointerEnterEvent>(_ => btn.style.backgroundColor = hoverColor);
            btn.RegisterCallback<PointerLeaveEvent>(_ => btn.style.backgroundColor = color);
            return btn;
        }

        Button MakeNavButton(Texture2D icon, Action onClick, Color color, Color hoverColor, string tooltip = null)
        {
            var btn = new Button(() => onClick?.Invoke())
            {
                style =
                {
                    height = 26,
                    marginRight = 6,
                    paddingLeft = 8, paddingRight = 8, // remove padding so centering is exact
                    backgroundColor = color,
                    color = TextColorLight,
                    borderTopLeftRadius = 8, borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8, borderBottomRightRadius = 8,

                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center
                },
                tooltip = string.IsNullOrEmpty(tooltip) ? string.Empty : tooltip
            };

            btn.RegisterCallback<PointerEnterEvent>(_ => btn.style.backgroundColor = hoverColor);
            btn.RegisterCallback<PointerLeaveEvent>(_ => btn.style.backgroundColor = color);

            var _icon = new Image
            {
                image = icon,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 16,
                    height = 16,
                    marginRight = 0, // remove margin so it’s dead-center
                    marginLeft = 0
                }
            };

            btn.Add(_icon);
            return btn;
        }

        VisualElement MakeNavButton(Texture2D icon, string text, Action onClick, Color color, Color hoverColor, string tooltip = null)
        {
            var btn = new Button(() => onClick?.Invoke())
            {
                tooltip = string.IsNullOrEmpty(tooltip) ? (text ?? string.Empty) : tooltip
            };

            // Base styling (reuse your palette)
            btn.style.height = 26;
            btn.style.marginRight = 6;
            btn.style.backgroundColor = color;
            btn.style.color = TextColorLight;
            btn.style.borderTopLeftRadius = 8;  btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8; btn.style.borderBottomRightRadius = 8;

            // Make the button act like a flex container
            btn.style.flexDirection = FlexDirection.Row;
            btn.style.alignItems = Align.Center;

            // Center if icon-only; otherwise left-align content
            bool iconOnly = (icon != null) && string.IsNullOrEmpty(text);
            btn.style.justifyContent = iconOnly ? Justify.Center : Justify.FlexStart;

            // Padding: tighter for icon-only, roomier for text/buttons
            btn.style.paddingLeft  = iconOnly ? 6 : 10;
            btn.style.paddingRight = iconOnly ? 6 : 10;

            // Hover feedback
            btn.RegisterCallback<PointerEnterEvent>(_ => btn.style.backgroundColor = hoverColor);
            btn.RegisterCallback<PointerLeaveEvent>(_ => btn.style.backgroundColor = color);

            // ---- Content ----
            if (icon != null)
            {
                var img = new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit
                };
                img.style.width = 16;
                img.style.height = 16;
                img.style.marginRight = string.IsNullOrEmpty(text) ? 0 : 6; // gap only when text exists
                btn.Add(img);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var lbl = new Label(text)
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        color = TextColorLight
                    }
                };
                btn.Add(lbl);
            }

            return btn;
        }

        // Positions a dropdown flush under a target (no gap) and shows it
        void OpenDropdownFrom(VisualElement button, VisualElement dd)
        {
            if (dd == null) return;

            CloseAllDropdowns();
            SetElement(overlay, true);
            
            /*// Hide others
            if (dd != nav_createDropdown)  nav_createDropdown.style.display  = DisplayStyle.None;
            if (dd != nav_developDropdown) nav_developDropdown.style.display = DisplayStyle.None;
            if (dd != nav_searchDropdown)  nav_searchDropdown.style.display  = DisplayStyle.None;*/

            if (dd.style.display == DisplayStyle.Flex)
            {
                dd.style.display = DisplayStyle.None;
                SetElement(overlay, false);
                return;
            }

            var targetWB   = button.worldBound;
            var wrapOrigin = overlay.worldBound.position;
            dd.style.left = targetWB.xMin - wrapOrigin.x;
            dd.style.top  = targetWB.yMax - wrapOrigin.y; // flush (prevents tiny dead zone)
            dd.style.display = DisplayStyle.Flex;
        }
        
        #endregion
        
        #region Building
        
        VisualElement BuildNavigationBar()
        {
            // === Wrapper that holds the bar and absolute-positioned dropdowns ===
            var _navWrap = new VisualElement
            {
                name = "navWrap",
                style =
                {
                    position = Position.Relative,
                    flexDirection = FlexDirection.Column,
                    display = DisplayStyle.None,   // shown only on Home/Creator/Developer
                    // paddingTop = 6, paddingBottom = 6, paddingLeft = 8, paddingRight = 8
                }
            };

            // === Custom styled bar (card-like, matches landing/dropdowns) ===
            var bar = new VisualElement
            {
                name = "navBarCard",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    backgroundColor = BackgroundColorDeepDark,
                    // square edges so it looks like a single rectangle across the top
                    borderTopLeftRadius = 0, borderTopRightRadius = 0,
                    borderBottomLeftRadius = 0, borderBottomRightRadius = 0,

                    borderTopWidth = 0, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = EdgeColorDark,
                    borderBottomColor = EdgeColorDark,
                    borderLeftColor = EdgeColorDark,
                    borderRightColor = EdgeColorDark,

                    // remove any outer margins and padding that create a gap
                    marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 0,
                    paddingLeft = 6, paddingRight = 6, paddingTop = 6, paddingBottom = 6,

                    // force it to span the full window width
                    width = Length.Percent(100)
                }
            };
            _navWrap.Add(bar);

            // ---------- Left group (Home / Create / Develop) ----------
            var left = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            bar.Add(left);

            // var icon = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
            // var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FESGASEditor/Icons/home.png");
            
            nav_homeButton = MakeNavButton(icon_HOME, OnHomeClicked_Nav, PrimaryButtonColor, PrimaryButtonColorHover, "Return to home");
            nav_createButton = MakeNavButton("Create", OnCreateClicked_Nav, SecondaryButtonColor, SecondaryButtonColorHover);
            nav_developButton = MakeNavButton("Develop", OnDevelopClicked_Nav, SecondaryButtonColor, SecondaryButtonColorHover);

            left.Add(nav_homeButton);
            left.Add(nav_createButton);
            left.Add(nav_developButton);

            // ---------- Spacer ----------
            var spacer = new VisualElement { style = { flexGrow = 1 } };
            bar.Add(spacer);

            // ---------- Search ----------
            // A small container so we can pad/round the TextField like our cards
            var searchWrap = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    backgroundColor = BackgroundColorDark,
                    borderTopLeftRadius = 8, borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8, borderBottomRightRadius = 8,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = EdgeColorDark,
                    borderBottomColor = EdgeColorDark,
                    borderLeftColor = EdgeColorDark,
                    borderRightColor = EdgeColorDark,
                    paddingLeft = 6, paddingRight = 6,
                    marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 0
                }
            };
            bar.Add(searchWrap);

            nav_searchField = new TextField { name = "FES-SearchField" };
            nav_searchField.style.width = 320;
            nav_searchField.style.marginTop = 2;
            nav_searchField.style.marginBottom = 2;
            nav_searchField.label = "";
            nav_searchField.style.color = BackgroundColorSub;
            searchWrap.Add(nav_searchField);

            // Open results on focus/click
            nav_searchField.RegisterCallback<FocusInEvent>(_ =>
            {
                OpenSearchDropdown_Nav();
                if (string.IsNullOrEmpty(nav_searchField.value)) ShowNoSearchResults_Nav();
            });
            // Value change → (re)query
            nav_searchField.RegisterValueChangedCallback(OnSearchChanged_Nav);

            // ---------- Options menu (custom-styled button + Unity menu) ----------
            var optionsBtn = BuildOptionsMenu_Nav();
            bar.Add(optionsBtn);

            // ---------- Leave-to-close wiring (no open-on-hover) ----------
            AttachHoverGroup(nav_createButton,  nav_createDropdown);
            AttachHoverGroup(nav_developButton, nav_developDropdown);
            AttachHoverGroup(searchWrap,         nav_searchDropdown); // field container + dropdown

            return _navWrap;

            // ---------- Local builders for the three dropdowns with matching style ----------
        }

        VisualElement BuildCreateDropdown_Nav()
        {
            float width = 3 * DropdownColumnWidth;
            var dd = NewDropdownBase(width); // card style consistent with landing
            /*dd.style.width = width;
            dd.style.minWidth = width;
            dd.style.maxWidth = width;*/
            
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            dd.Add(row);

            var left  = NewColumn(DropdownColumnWidth, DropdownColumnWidthSmall, DropdownColumnWidthLarge, 0f);
            var mid  = NewColumn(DropdownColumnWidth, DropdownColumnWidthSmall, DropdownColumnWidthLarge, 0f);
            var right = NewColumn(DropdownColumnWidth, DropdownColumnWidth, DropdownColumnWidth * 2);
            
            row.Add(left); row.Add(mid); row.Add(right);
            
            // LEFT: Data + Keys + Special
            left.Add(BuildHeader("Data"));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("ability.png"), "Ability",  () => LoadIntoCreator(DataType.Ability))));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("effect.png"), "Effect",   () => LoadIntoCreator(DataType.Effect))));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("person.png"), "System",   () => LoadIntoCreator(DataType.Entity))));
            left.Add(BuildSeparator());
            left.Add(BuildHeader("Keys"));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("attribute.png"), "Attribute",() => LoadIntoCreator(DataType.Attribute))));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("tag.png"), "Tag",      () => LoadIntoCreator(DataType.Tag))));
            left.Add(BuildSeparator());
            left.Add(BuildHeader("Special"));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("attribute_set.png"), "Attribute Set", () => LoadIntoCreator(DataType.AttributeSet))));
            left.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("modifier.png"), "Modifier",      () => LoadIntoCreator(DataType.Modifier))));

            // MIDDLE: Workers + Process
            mid.Add(BuildHeader("Workers"));
            mid.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("attribute.png"), "Attribute", () => LoadIntoCreator(DataType.AttributeWorker))));
            mid.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("impact.png"), "Impact",    () => LoadIntoCreator(DataType.ImpactWorker))));
            mid.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("effect.png"), "Effect",    () => LoadIntoCreator(DataType.EffectWorker))));
            mid.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("tag.png"), "Tag",       () => LoadIntoCreator(DataType.TagWorker))));
            mid.Add(BuildSeparator());
            mid.Add(BuildHeader("Process Management"));
            mid.Add(BuildIconTextItem(new MenuEntry(LoadFESGASIcon("process.png"), "Instantiator", () => LoadIntoCreator(DataType.ProcessInstantiator))));

            // RIGHT: Proxy Task (search + recent)
            right.Add(BuildHeader("Proxy Task"));

            nav_proxySearchField = new TextField { tooltip = "Search Proxy Tasks" };
            nav_proxySearchField.style.marginTop = 4;
            nav_proxySearchField.style.width = Length.Percent(100);
            nav_proxySearchField.style.flexGrow = 0;
            // nav_proxySearchField.RegisterCallback<PointerUpEvent>(e => e.StopImmediatePropagation());
            nav_proxySearchField.RegisterValueChangedCallback(evt => RefreshProxyTaskList(evt.newValue));
            right.Add(nav_proxySearchField);

            nav_proxyListView = new ListView
            {
                selectionType = SelectionType.Single,
                // Use fixed-height rows so each item actually takes vertical space
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                //fixedItemHeight = -1,

                style =
                {
                    height = ListviewMinHeight,      // you already have this constant
                    marginTop = 4,
                    width = Length.Percent(100),
                    // Keep list content clipped so it doesn’t draw over the column header
                    overflow = Overflow.Hidden
                }
            };
            // nav_proxyListView.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
            nav_proxyListView.makeItem = () =>
            {
                // row container
                var r = new VisualElement
                {
                    name = "ProxyRow",
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        height = ListviewItemMinHeight,                    // critical: gives the row vertical space
                        paddingLeft = 6, paddingRight = 6
                    }
                };

                // optional icon (hidden for normal items; shown for Recent)
                var _icon = new Image
                {
                    name = "Icon",
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 14, height = 14, marginRight = 6, display = DisplayStyle.None }
                };
                r.Add(_icon);

                // main label
                var label = new Label
                {
                    name = "Text",
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        whiteSpace = WhiteSpace.NoWrap,
                        overflow = Overflow.Hidden,
                        textOverflow = TextOverflow.Ellipsis
                    }
                };
                r.Add(label);

                // small clear button (only visible for the “Recent” header row)
                var clearBtn = new Button { name = "ClearRecent", text = "×" };
                clearBtn.style.width = 18; clearBtn.style.height = 18;
                clearBtn.style.marginLeft = 6;
                clearBtn.style.display = DisplayStyle.None; // hidden by default
                clearBtn.clicked += OnClearRecentClicked;
                r.Add(clearBtn);
                
                // dedicated separator element (hidden by default)
                var sep = new VisualElement
                {
                    name = "Sep",
                    style = { height = 1, flexGrow = 1, backgroundColor = new Color(0,0,0,0.35f), display = DisplayStyle.None }
                };
                r.Add(sep);

                return r;

                return r;
            };
            
            nav_proxyListView.bindItem = (r, idx) =>
            {
                var rows = nav_proxyListView.itemsSource as List<ProxyRow>;
                if (rows == null || idx < 0 || idx >= rows.Count) return;
                var data = rows[idx];

                // Ensure expected children exist (in case Unity recycled one from older code)
                var icon    = r.Q<Image>("Icon") ?? new Image { name = "Icon" };
                var label   = r.Q<Label>("Text") ?? new Label   { name = "Text" };
                var clearBt = r.Q<Button>("ClearRecent") ?? new Button { name = "ClearRecent", text = "×" };
                var sep     = r.Q<VisualElement>("Sep") ?? new VisualElement { name = "Sep" };

                if (icon.parent == null)    r.Insert(0, icon);
                if (label.parent == null)   r.Add(label);
                if (clearBt.parent == null) r.Add(clearBt);
                if (sep.parent == null)     r.Add(sep);
                
                label.style.display  = DisplayStyle.Flex;   // default for non-separator rows
                icon.style.display   = DisplayStyle.None;
                clearBt.style.display= DisplayStyle.None;
                sep.style.display    = DisplayStyle.None;

                // reset visuals every bind
                r.style.height = ListviewItemMinHeight;
                r.style.paddingLeft = 6; r.style.paddingRight = 6;
                r.pickingMode = PickingMode.Position;
                r.focusable = false;
                r.SetEnabled(true);

                icon.style.display = DisplayStyle.None;
                clearBt.style.display = DisplayStyle.None;
                sep.style.display = DisplayStyle.None;

                label.style.color = Color.white;
                label.style.unityFontStyleAndWeight = FontStyle.Normal;

                // avoid stacking multiple handlers
                clearBt.clicked -= OnClearRecentClicked;
                r.UnregisterCallback<PointerDownEvent>(StopSelect);

                switch (data.Type)
                {
                    case ProxyRowType.Header:
                        label.text = "Recent";
                        label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        clearBt.style.display = DisplayStyle.Flex;
                        clearBt.clicked += OnClearRecentClicked;

                        // non-interactive
                        r.RegisterCallback<PointerDownEvent>(StopSelect);
                        break;

                    case ProxyRowType.RecentItem:
                        label.text = data.Text;
                        if (icon_RECENT != null)
                        {
                            icon.image = icon_RECENT;
                            icon.style.display = DisplayStyle.Flex;
                            icon.style.unityBackgroundImageTintColor = Color.white;
                        }
                        break;

                    case ProxyRowType.Separator:
                        // make the row a 1px line, full width
                        r.style.height = 2;
                        r.style.paddingLeft = 0;
                        r.style.paddingRight = 0;

                        // HIDE other children so they don't consume width
                        label.style.display = DisplayStyle.None;
                        icon.style.display  = DisplayStyle.None;
                        clearBt.style.display = DisplayStyle.None;

                        // show the line
                        sep.style.display = DisplayStyle.Flex;
                        sep.style.height = 2;
                        sep.style.flexGrow = 1;
                        // ensure no margins clip it
                        sep.style.marginLeft = 0;
                        sep.style.marginRight = 0;

                        // non-interactive
                        r.RegisterCallback<PointerDownEvent>(e => e.StopImmediatePropagation());
                        break;

                    case ProxyRowType.TaskItem:
                        label.text = data.Text;
                        break;

                    case ProxyRowType.Placeholder:
                        label.text = data.Text;
                        label.style.unityFontStyleAndWeight = FontStyle.Italic;
                        label.style.color = new Color(0.7f,0.7f,0.8f,1f);
                        r.SetEnabled(false);
                        r.RegisterCallback<PointerDownEvent>(StopSelect);
                        break;
                }

                void StopSelect(PointerDownEvent e) => e.StopImmediatePropagation();
            };

            
            nav_proxyListView.selectionChanged += objs =>
            {
                var r = objs.FirstOrDefault() as ProxyRow;
                if (r == null)
                {
                    nav_proxyListView.ClearSelection();
                    return;
                }

                if (r.Type is ProxyRowType.Header or ProxyRowType.Separator or ProxyRowType.Placeholder)
                {
                    nav_proxyListView.ClearSelection();
                    return;
                }

                // valid selection (RecentItem or TaskItem)
                var sel = r;
                if (string.IsNullOrEmpty(sel.Text)) { nav_proxyListView.ClearSelection(); return; }

                // MRU update
                nav_recentProxyTasks.RemoveAll(t => t.Id == sel.Id);
                nav_recentProxyTasks.Insert(0, sel);
                if (nav_recentProxyTasks.Count > nav_maxRecentProxy) nav_recentProxyTasks.RemoveAt(nav_recentProxyTasks.Count - 1);
                
                CloseAllDropdowns();
                RefreshProxyTaskList("");
                
                Debug.Log("Create Proxy Task: " + sel.Text);
            };

            right.Add(nav_proxyListView);

            // First populate (recent if empty)
            RefreshProxyTaskList("");

            return dd;
            
            void OnClearRecentClicked()
            {
                nav_recentProxyTasks.Clear();
                RefreshProxyTaskList(nav_proxySearchField?.value ?? "");
            }

            void RefreshProxyTaskList(string query)
            {
                // Normalize / prepare source
                var q = (query ?? "").Trim();
                var qLower = q.ToLowerInvariant();

                // Base task catalog in strict alphabetical order
                // (Replace 'allProxyTasks' with your actual source if it’s named differently)
                var allAlpha = Project.ProxyTasks
                    .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // If there is a query, filter the alpha list
                if (!string.IsNullOrEmpty(qLower)) allAlpha = allAlpha.Where(t => t != null && t.Name.ToLowerInvariant().Contains(qLower)).ToList();

                // Build rows
                var rows = new List<ProxyRow>();

                // RECENTS (only if there are any, and only when query is empty)
                // If you want recents to still show while typing (filtered too), remove the 'qLower.Length == 0' condition.
                if (nav_recentProxyTasks.Count > 0)
                {
                    var keepRecent = new List<ProxyRow>();
                    foreach (var r in nav_recentProxyTasks)
                        keepRecent.Add(r);

                    if (keepRecent.Count > 0)
                    {
                        rows.Add(ProxyRow.Header("Recently Used"));
                        rows.AddRange(keepRecent);
                        rows.Add(ProxyRow.Separator());
                    }
                }

                // TASKS
                if (allAlpha.Count > 0)
                {
                    foreach (var t in allAlpha)
                        rows.Add(ProxyRow.TaskItem(t));
                }
                else
                {
                    // No matches → placeholder (non-interactive)
                    rows.Add(ProxyRow.Placeholder("No items found"));
                }

                nav_proxyListView.itemsSource = rows;
                nav_proxyListView.Rebuild();
                nav_proxyListView.ClearSelection(); // ensure nothing is highlighted
            }
        }

        VisualElement BuildDevelopDropdown_Nav()
        {
            var dd = NewDropdownBase(minWidth: 3 * DropdownColumnWidth);
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            dd.Add(row);

            var a = NewColumn(); var b = NewColumn(); var c = NewColumn();
            row.Add(a); row.Add(b); row.Add(c);

            var icon = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D;

            a.Add(BuildHeader("Build"));
            a.Add(BuildIconTextItem(new MenuEntry(icon, "Content Build", () => Debug.Log("Content Build"))));
            a.Add(BuildIconTextItem(new MenuEntry(icon, "Rebuild All",   () => Debug.Log("Rebuild All"))));
            a.Add(BuildIconTextItem(new MenuEntry(icon, "Clean Build",   () => Debug.Log("Clean Build"))));

            b.Add(BuildHeader("Validate"));
            b.Add(BuildIconTextItem(new MenuEntry(icon, "Validate Project", () => Debug.Log("Validate Project"))));
            b.Add(BuildIconTextItem(new MenuEntry(icon, "Lint Data",        () => Debug.Log("Lint Data"))));
            b.Add(BuildIconTextItem(new MenuEntry(icon, "Find Duplicates",  () => Debug.Log("Find Duplicates"))));

            c.Add(BuildHeader("Utilities"));
            c.Add(BuildIconTextItem(new MenuEntry(icon, "Run Tests",     () => Debug.Log("Run Tests"))));
            c.Add(BuildIconTextItem(new MenuEntry(icon, "Open Logs",     () => Debug.Log("Open Logs"))));
            c.Add(BuildIconTextItem(new MenuEntry(icon, "Export Report", () => Debug.Log("Export Report"))));

            return dd;
        }

        VisualElement BuildSearchDropdown_Nav()
        {
            var dd = NewDropdownBase(minWidth: DropdownColumnWidthLarge);

            nav_searchList = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                style = { width = DropdownColumnWidthLarge, height = ListviewMinHeight }
            };
            nav_searchList.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
            nav_searchList.bindItem = (e, i) =>
            {
                if (i < 0 || i >= nav_searchItems.Count) return;
                var item = nav_searchItems[i];
                ((Label)e).text = item.Display;
                e.SetEnabled(!item.IsPlaceholder);
            };
            nav_searchList.selectionChanged += OnSearchItemChosen_Nav;
            
            var scroll = nav_searchList.Q<ScrollView>("unity-vertical-scrollbar");
            if (scroll != null)
            {
                scroll.style.backgroundColor = BackgroundColorDark;
            }

            dd.Add(nav_searchList);
            return dd;
        }

        VisualElement BuildOptionsMenu_Nav()
        {
            var optionsBtn = MakeNavButton("Options", null, SecondaryButtonColor, SecondaryButtonColorHover);
            
            // Build a Unity menu and open it on click
            var optionsMenu = new GenericMenu();
            optionsMenu.AddItem(new GUIContent("Open JSON…"), false, () => Debug.Log("Open JSON"));
            optionsMenu.AddItem(new GUIContent("Save JSON"), false, () => Debug.Log("Save JSON"));
            optionsMenu.AddSeparator("");
            optionsMenu.AddItem(new GUIContent("Settings"), false, () => Debug.Log("Open Settings"));
            optionsMenu.AddItem(new GUIContent("Documentation"), false, () => Application.OpenURL("https://example.com/docs"));
            optionsMenu.AddItem(new GUIContent("About"), false, () => EditorUtility.DisplayDialog("About FESGAS", "FESGAS & Gasify Editor Suite\n\nCreate precise, meaningful gameplay behaviour at the intersection of logic and complexity.\n\nA thorough Gameplay Ability System for Unity.\n\n© Far Emerald Studio", "OK"));

            optionsBtn.RegisterCallback<MouseUpEvent>(evt =>
            {
                var r = new Rect(evt.mousePosition, Vector2.zero);
                optionsMenu.DropDown(r);
            });

            return optionsBtn;
        }

        VisualElement BuildDropdownHost()
        {
            // Full-screen transparent backdrop (clicking this closes the dropdown)
            var dropdownBackdrop = new VisualElement
            {
                name = "DropdownBackdrop",
                pickingMode = PickingMode.Position,
                style = {
                    position = Position.Absolute,
                    left = 0, right = 0, top = 0, bottom = 0,
                    backgroundColor = new StyleColor(Color.clear), // fully transparent
                    display = DisplayStyle.None           // hidden until a menu is open
                }
            };
            overlay.Add(dropdownBackdrop);

// Host that will be positioned under the clicked button
            var dropdownHost = new VisualElement
            {
                name = "DropdownHost",
                pickingMode = PickingMode.Position,
                style = {
                    position = Position.Absolute,
                    width = StyleKeyword.Auto,
                    height = StyleKeyword.Auto,
                    overflow = Overflow.Visible,
                    display = DisplayStyle.None
                }
            };
            dropdownBackdrop.Add(dropdownHost);

// Clicking anywhere on the backdrop (not the host) closes menus
            dropdownBackdrop.RegisterCallback<MouseDownEvent>(_ => CloseAllDropdowns());

// Prevent clicks INSIDE the popup from bubbling to the backdrop
            dropdownHost.RegisterCallback<MouseDownEvent>(e => e.StopImmediatePropagation());
            
            return dropdownBackdrop;
        }

        void LoadIntoCreator(DataType kind)
        {
            SetCreatorPage(CreatorItem.Fresh(kind));
        }
        
        #endregion
        
        #region Functionality
        
        #region Buttons
        
        void OnHomeClicked_Nav()
        {
            SetPage(activePage == GasifyPage.Home ? GasifyPage.Landing : GasifyPage.Home);
        }

        void OnCreateClicked_Nav()
        {
            
            OpenDropdownFrom(nav_createButton, nav_createDropdown);
        }

        void OnDevelopClicked_Nav()
        {
            OpenDropdownFrom(nav_developButton, nav_developDropdown);
        }
        
        #endregion

        #region Search
        
        void OpenSearchDropdown_Nav()
        {
            OpenDropdownFrom(nav_searchField, nav_searchDropdown);
            SetElement(overlay, true);
            
            // Size the list
            nav_searchList.style.width = nav_searchField.worldBound.width;
            nav_searchList.style.height = ListviewMinHeight;

            // Populate with current query
            RefreshSearchResults_Nav(nav_searchField.value);
        }

        void ShowNoSearchResults_Nav()
        {
            
        }

        void CloseSearchDropdown_Nav()
        {
            nav_searchDropdown.style.display = DisplayStyle.None;
            nav_searchList.ClearSelection();
            
            CloseAllDropdowns();
        }

        void OnSearchChanged_Nav(ChangeEvent<string> evt)
        {
            if (nav_searchDropdown.style.display == DisplayStyle.None) OpenSearchDropdown_Nav();
            RefreshSearchResults_Nav(evt.newValue ?? "");
        }

        void RefreshSearchResults_Nav(string query)
        {
            var q = (query ?? "").Trim().ToLowerInvariant();

            // Collect across your domains; weight/display as you like
            var items = new List<SearchItem>();

            foreach (var a in Project.Abilities)
                if (Matches(a.Name, a.Id, q))
                    items.Add(new SearchItem($"{a.Name} • Ability", a.Id, DataType.Ability));

            foreach (var at in Project.Attributes)
                if (Matches(at.Name, at.Id, q))
                    items.Add(new SearchItem($"{at.Name} • Attribute", at.Id, DataType.Attribute));
            
            foreach (var t in Project.Tags)
                if (Matches(t.Name, t.Id, q))
                    items.Add(new SearchItem($"{t.Name} • Tag", t.Id, DataType.Tag));

            foreach (var at in Project.ProxyTasks)
                if (Matches(at.Name, at.Id, q))
                    items.Add(new SearchItem($"{at.Name} • Proxy Task", at.Id, DataType.ProcessInstantiator));

            // Sort (Abilities first, then Tags, then Attributes, then by name)
            nav_searchItems = items
                .OrderBy(it => it.KindOrder)
                .ThenBy(it => it.Display, StringComparer.OrdinalIgnoreCase)
                .ToList();
            
            if (nav_searchItems.Count == 0) nav_searchItems.Add(new SearchItem("No search results", "-1", DataType.None, true));

            nav_searchList.itemsSource = nav_searchItems;
            nav_searchList.Rebuild();
        }

        static bool Matches(string name, string id, string q)
        {
            if (string.IsNullOrEmpty(q)) return true;
            return (!string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains(q)) ||
                   (!string.IsNullOrEmpty(id)   && id.ToLowerInvariant().Contains(q));
        }

        void OnSearchItemChosen_Nav(IEnumerable<object> selection)
        {
            var item = selection?.FirstOrDefault() as SearchItem;
            if (item == null) return;

            if (item.IsPlaceholder) return;
            
            // Navigate depending on item.Kind (here we just go to Home and pretend to open it)
            SetPage(GasifyPage.Home);
            // TODO: tell your Home/Creator page to focus the selected object by ID (item.Id)
            CloseSearchDropdown_Nav();
        }
        
        #endregion
        
        #endregion
        
        #endregion
        
        #region Landing Page

        VisualElement BuildLandingPage()
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

            // Center wrapper
            var center = new VisualElement
            {
                style = { flexGrow = 1, justifyContent = Justify.Center, alignItems = Align.Center, paddingLeft = 10, paddingRight = 10 }
            };
            root.Add(center);

            // Card
            var card = new VisualElement
            {
                style =
                {
                    width = 420, maxWidth = 520,
                    paddingTop = 16, paddingBottom = 16, paddingLeft = 16, paddingRight = 16,
                    backgroundColor = BackgroundColorDark,
                    borderTopLeftRadius = 12, borderTopRightRadius = 12,
                    borderBottomLeftRadius = 12, borderBottomRightRadius = 12,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            center.Add(card);

            var _title = new Label("Framework Editor Suite")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 18, marginBottom = 8, color = Color.white }
            };
            card.Add(_title);

            var subtitle = new Label("Create, load and manage your GAS framework data")
            {
                style = { fontSize = 12, marginBottom = 10, color = TextColorLight }
            };
            card.Add(subtitle);

            var col = new VisualElement { style = { flexDirection = FlexDirection.Column } };
            card.Add(col);

            col.Add(PrimaryButton("Test", OnTestClicked_Landing));
            col.Add(PrimaryButton("Create", OnCreateClicked_Landing));
            col.Add(SecondaryButton("Load", OnLoadClicked_Landing));
            col.Add(SecondaryButton("Documentation", OnDocumentationClicked_Landing));
            col.Add(SecondaryButton("Settings", OnSettingsClicked_Landing));

            // Disclaimer pinned bottom
            var disclaimer = new Label("Disclaimer: This tool is pre-release. Data formats may change. Back up your project before use. Thank you for using FESGAS.")
            {
                style = {
                    color = TextColorSub, fontSize = 11,
                    marginTop = 8, marginBottom = 8, marginLeft = 8, marginRight = 8,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            root.Add(disclaimer);
            return root;
        }

        void OnTestClicked_Landing()
        {
            SetPage(GasifyPage.Home);
        }
        
        void OnCreateClicked_Landing()
        {
            // TODO: open your "New Framework" wizard
            EditorUtility.DisplayDialog("Create", "Open New Framework flow here.", "OK");
        }

        void OnLoadClicked_Landing()
        {
            // TODO: call your loader / welcome flow
            string path = EditorUtility.OpenFilePanel("Load Framework JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                // Hook into your ManualJsonStore/JsonStore then swap windows/state
                Debug.Log($"Selected JSON: {path}");
            }
        }

        void OnDocumentationClicked_Landing()
        {
            // TODO: point to your docs URL or local file
            Application.OpenURL("https://example.com/your-framework-docs");
        }

        void OnSettingsClicked_Landing()
        {
            // TODO: open your settings UI or a ScriptableObject inspector
            EditorUtility.DisplayDialog("Settings", "Open Settings UI here.", "OK");
        }
        
        #endregion
        
        #region Content

        VisualElement BuildContent()
        {
            var v = new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 0,
                    paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0,
                    display = DisplayStyle.Flex
                }
            };
            return v;
        }

        VisualElement BuildOverlay()
        {
            var v = new VisualElement()
            {
                pickingMode = PickingMode.Position,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    position = Position.Absolute,
                    left = 0, right = 0, top = 0, bottom = 0,
                    display = DisplayStyle.None
                }
            };
            return v;
        }

        
        #endregion
        
        #region Home Page
        
        VisualElement BuildHomePage()
        {
            var root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0
                }
            };
            
                // LEFT: Project View
            home_projectView = BuildProjectView();
            // Give it a card-like look
            home_projectView.style.flexBasis = 0;
            home_projectView.style.flexGrow  = 1f;
            home_projectView.style.minWidth = 300;
            home_projectView.style.maxWidth = 300;
            home_projectView.style.backgroundColor = BackgroundColorDark;
            home_projectView.style.borderTopLeftRadius = 0;
            home_projectView.style.borderTopRightRadius = 0;
            home_projectView.style.borderBottomLeftRadius = 0;
            home_projectView.style.borderBottomRightRadius = 0;
            home_projectView.style.paddingLeft = 0; home_projectView.style.paddingRight = 0; home_projectView.style.paddingTop = 0; home_projectView.style.paddingBottom = 0;

            
            home_usageMap = BuildUsageMap();
            
            root.Add(home_projectView);
            root.Add(home_usageMap);
            
            return root;

            VisualElement BuildProjectView()
            {
                var wrap = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
                wrap.style.backgroundColor = BackgroundColorDark;
                
                // --- Top bar: Search + Filter
                var top = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 0,
                        paddingTop = 8,
                        paddingRight = 8,
                        backgroundColor = BackgroundColorDark
                    }
                };
                wrap.Add(top);

                home_projectSearchField = new TextField { name = "PV-Search", tooltip = "Search by name or id…" };
                home_projectSearchField.style.flexGrow = 1;
                home_projectSearchField.style.height = 22;
                home_projectSearchField.style.paddingLeft = 8;
                home_projectSearchField.style.paddingTop = 0;
                home_projectSearchField.style.paddingRight = 8;
                // Style inner input background/text
                var input = home_projectSearchField.Q("unity-text-input");
                if (input != null)
                {
                    input.style.backgroundColor = BackgroundColorLight;
                    input.style.color = TextColorLight;
                }
                home_projectSearchField.RegisterValueChangedCallback(_ => RefreshProjectView());
                top.Add(home_projectSearchField);

                //home_projectFilterButton = new Button(ShowProjectFilterMenu) { text = "Filter" };
                home_projectFilterButton = TertiaryButton("", ShowProjectFilterMenu, 22);

                var _icon = LoadFESGASIcon("filter.png");
                home_projectFilterButton = MakeNavButton(_icon, ShowProjectFilterMenu, TertiaryButtonColor, TertiaryButtonColorHover);
                
                home_projectFilterButton.style.height = 22;
                top.Add(home_projectFilterButton);

                // --- List
                home_projectList = new ListView
                {
                    selectionType = SelectionType.Single,
                    virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                    fixedItemHeight = 22,
                    style =
                    {
                        flexGrow = 1,
                        backgroundColor = BackgroundColorDark,
                        paddingLeft = 8,
                        paddingTop = 8
                    }
                };

                // Row visual template
                home_projectList.makeItem = () =>
                {
                    var row = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            paddingLeft = 6, paddingRight = 6,
                            height = 22
                        }
                    };

                    // Chevron / icon (for headers)
                    var chevron = new Label { name = "Chevron", text = "", style = { width = 14, unityTextAlign = TextAnchor.MiddleCenter, marginRight = 4, color = TextColorLight } };
                    row.Add(chevron);

                    // Name
                    var _name = new Label { name = "Name", text = "", style = { flexGrow = 1, color = TextColorLight, unityTextAlign = TextAnchor.MiddleLeft, whiteSpace = WhiteSpace.NoWrap, overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis } };
                    row.Add(_name);

                    // Hover-only arrow (for items)
                    var goBtn = new Button { name = "Go", text = "→" };
                    goBtn.style.height = 18; goBtn.style.width = 22; goBtn.style.display = DisplayStyle.None;
                    row.Add(goBtn);

                    // Hover behavior to show/hide the go arrow on items
                    row.RegisterCallback<PointerEnterEvent>(_ =>
                    {
                        var data = row.userData as PVRow;
                        if (data != null && data.Type == PVRowType.Item)
                            goBtn.style.display = DisplayStyle.Flex;
                    });
                    row.RegisterCallback<PointerLeaveEvent>(_ => goBtn.style.display = DisplayStyle.None);

                    return row;
                };

                // Bind per row
                home_projectList.bindItem = (row, index) =>
                {
                    if (index < 0 || index >= home_pvRows.Count) return;
                    var data = home_pvRows[index];
                    row.userData = data;

                    var chevron = row.Q<Label>("Chevron");
                    var _name    = row.Q<Label>("Name");
                    var goBtn   = row.Q<Button>("Go");
                    goBtn.tooltip = "Edit";

                    // Reset
                    goBtn.style.display = DisplayStyle.None;
                    row.style.unityBackgroundImageTintColor = new StyleColor();

                    if (data.Type == PVRowType.Header)
                    {
                        // Header row
                        chevron.text = data.Collapsed ? "▲" : "▼";
                        chevron.style.display = DisplayStyle.Flex;
                        _name.text = DataTypeTextPlural(data.DataType) + $" ({projectData[data.DataType].Count})";
                        _name.style.unityFontStyleAndWeight = FontStyle.Bold;

                        // Click anywhere in header row to toggle
                        row.RegisterCallback<MouseDownEvent>(OnHeaderClicked, TrickleDown.NoTrickleDown);
                        row.style.backgroundColor = BackgroundColorLight;
                        goBtn.style.display = DisplayStyle.None;
                    }
                    else if (data.Type == PVRowType.Placeholder)
                    {
                        chevron.text = "";
                        _name.text = "No items to display";
                        _name.style.unityFontStyleAndWeight = FontStyle.Bold;
                        row.style.backgroundColor = BackgroundColorLight;
                        goBtn.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        // Item row
                        chevron.text = ""; // no chevron for items
                        chevron.style.display = DisplayStyle.None;
                        _name.text = data.Name;
                        _name.style.unityFontStyleAndWeight = FontStyle.Normal;

                        row.style.paddingLeft = 24;

                        // Primary click loads into Usage (stub)
                        row.RegisterCallback<MouseDownEvent>(OnItemClicked, TrickleDown.NoTrickleDown);

                        // Arrow button → go to Create page & load item (stub)
                        goBtn.clicked -= OnGoClicked; // avoid stacking
                        goBtn.clicked += OnGoClicked;
                        void OnGoClicked()
                        {
                            Debug.Log($"[ProjectView] Go → Create page for {data.DataType}: {data.Name} ({data.Id})");
                            SetCreatorPage(data.Id, data.DataType);
                        }
                    }

                    return;

                    // Local handlers capture current row via row.userData
                    void OnHeaderClicked(MouseDownEvent e)
                    {
                        e.StopImmediatePropagation();
                        ToggleSection(data.DataType);
                    }

                    void OnItemClicked(MouseDownEvent e)
                    {
                        if (data.Type != PVRowType.Item) return;
                        Debug.Log($"[ProjectView] Load into Usage: {data.DataType} • {data.Name} ({data.Id})");
                        // TODO load into usage map
                    }
                };

                // Avoid “selection highlight” sticking on headers
                home_projectList.selectionChanged += objs =>
                {
                    var row = objs?.FirstOrDefault() as PVRow;
                    if (row == null || row.Type == PVRowType.Header)
                    {
                        home_projectList.ClearSelection();
                        return;
                    }
                    // You could react to item selection here if desired.
                };

                wrap.Add(home_projectList);

                // Init collapsed flags (default: expanded)
                foreach (DataType dt in Enum.GetValues(typeof(DataType)))
                    home_pvSectionCollapsed[dt] = false;

                // First population
                RefreshProjectView();
                return wrap;
                
                void ShowProjectFilterMenu()
                {
                    var m = new GenericMenu();

                    // DataType filters (toggle)
                    void ToggleKind(DataType k)
                    {
                        Debug.Log($"toggling {k}");
                        if (home_pvTypeFilter.Contains(k)) home_pvTypeFilter.Remove(k);
                        else home_pvTypeFilter.Add(k);
                        RefreshProjectView();
                    }

                    // Build entries for the core types you’re using. Add more as you add types.
                    AddKind(DataType.Ability, "Ability");
                    AddKind(DataType.Effect, "Effect");
                    AddKind(DataType.Entity, "System");
                    AddKind(DataType.Attribute, "Attribute");
                    AddKind(DataType.Tag, "Tag");
                    AddKind(DataType.AttributeSet, "Attribute Set");
                    AddKind(DataType.Modifier, "Modifier");
                    AddKind(DataType.AttributeWorker, "Attribute Worker");
                    AddKind(DataType.ImpactWorker, "Impact Worker");
                    AddKind(DataType.EffectWorker, "Effect Worker");
                    AddKind(DataType.TagWorker, "Tag Worker");
                    AddKind(DataType.ProcessInstantiator, "Process Management");

                    m.AddSeparator("");
                    
                    // In-Use only (stubbed—flag stored, not applied yet)
                    m.AddItem(new GUIContent("▲ Reset Data Type Filter"), false, () =>
                    {
                        home_pvTypeFilter.Clear();
                        RefreshProjectView();
                    });
                    
                    m.AddSeparator("");

                    // In-Use only (stubbed—flag stored, not applied yet)
                    m.AddItem(new GUIContent("In Use Only (stub)"), home_pvFilterInUse, () =>
                    {
                        home_pvFilterInUse = !home_pvFilterInUse;
                        RefreshProjectView();
                    });

                    m.AddSeparator("");

                    // In-Use only (stubbed—flag stored, not applied yet)
                    m.AddItem(new GUIContent("Only Populated Data Types"), home_pvOnlyPopulated, () =>
                    {
                        home_pvOnlyPopulated = !home_pvOnlyPopulated;
                        RefreshProjectView();
                    });
                    
                    m.AddSeparator("");
                    
                    // In-Use only (stubbed—flag stored, not applied yet)
                    m.AddItem(new GUIContent("▲ Reset All"), false, () =>
                    {
                        home_pvTypeFilter.Clear();
                        home_pvFilterInUse = false;
                        home_pvOnlyPopulated = true;
                        RefreshProjectView();
                    });
                    
                    
                    
                    m.ShowAsContext();
                    return;

                    void AddKind(DataType kind, string label)
                    {
                        bool on = home_pvTypeFilter.Contains(kind);
                        m.AddItem(new GUIContent($"{label}"), on, () => ToggleKind(kind));
                    }
                }

                void ToggleSection(DataType kind)
                {
                    home_pvSectionCollapsed.TryAdd(kind, false);
                    home_pvSectionCollapsed[kind] = !home_pvSectionCollapsed[kind];
                    RefreshProjectView();
                }

                void RefreshProjectView()
                {
                    // Build a flat list of PVRow from current project, applying search + type filter + collapse states.
                    var q = (home_projectSearchField?.value ?? string.Empty).Trim().ToLowerInvariant();
                    bool HasQuery = q.Length > 0;
                    
                    // Flatten to rows with headers (respect filters & collapse)
                    var rows = new List<PVRow>();
                    IEnumerable<DataType> kindsOrder = new[]
                    {
                        DataType.Ability, DataType.Effect, DataType.Entity, DataType.Attribute, DataType.Tag, DataType.ProxyTask, DataType.AttributeSet,
                        DataType.Modifier,
                        DataType.AttributeWorker,
                        DataType.ImpactWorker,
                        DataType.EffectWorker,
                        DataType.TagWorker,
                        DataType.ProcessInstantiator,
                    };

                    // Apply alpha sort & search
                    foreach (var k in projectData.Keys.ToList())
                    {
                        var list = projectData[k]
                            .OrderBy(p => p.Item2, StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        if (HasQuery)
                            list = list.Where(p =>
                                (!string.IsNullOrEmpty(p.Item2) && p.Item2.ToLowerInvariant().Contains(q)) ||
                                (!string.IsNullOrEmpty(p.Item1)   && p.Item1.ToLowerInvariant().Contains(q))
                            ).ToList();

                        projectData[k] = list;
                    }

                    foreach (var kind in kindsOrder)
                    {
                        if (!projectData.ContainsKey(kind))
                        {
                            rows.Add(new PVRow { Type = PVRowType.Header, DataType = kind, Name = DataTypeTextPlural(kind) + $" ({projectData[kind].Count})", Collapsed = true });
                            continue;
                        }
                        if (home_pvTypeFilter.Count > 0 && !home_pvTypeFilter.Contains(kind)) continue;

                        var items = projectData[kind];
                        
                        if (home_pvOnlyPopulated && items.Count == 0) continue;
                        // Always render header, even if empty (optional; you can skip if items.Count==0 && !HasQuery)
                        
                        bool collapsed = home_pvSectionCollapsed.TryGetValue(kind, out var c) && c;

                        rows.Add(new PVRow { Type = PVRowType.Header, DataType = kind, Name = DataTypeTextPlural(kind) + $" ({projectData[kind].Count})", Collapsed = collapsed });

                        if (!collapsed)
                        {
                            foreach (var (id, _name) in items)
                                rows.Add(new PVRow { Type = PVRowType.Item, DataType = kind, Id = id, Name = _name });
                        }
                    }

                    // If everything filtered away, show a friendly placeholder header
                    if (rows.Count == 0)
                        rows.Add(new PVRow { Type = PVRowType.Placeholder, DataType = DataType.None, Name = "No items to display", Collapsed = false });

                    home_pvRows = rows;
                    home_projectList.itemsSource = home_pvRows;
                    home_projectList.Rebuild();
                }
            }

            VisualElement BuildUsageMap()
            {
                // RIGHT: Usage Map (placeholder for now)
                var rightUsage = new VisualElement
                {
                    style =
                    {
                        flexBasis = 0,
                        flexGrow = 1f,
                        backgroundColor = BackgroundColorDeepLight,
                        justifyContent = Justify.Center, alignItems = Align.Center
                    }
                };
                rightUsage.Add(new Label("Usage Map (coming soon)") { style = { color = TextColorLight, unityFontStyleAndWeight = FontStyle.Italic } });
                return rightUsage;
            }
        }
        
        #endregion
        
        #region Creator Page
        
        VisualElement BuildCreatorPage()
        {
            // Root column
            var root = new VisualElement
            {
                name = "creatorRoot",
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column
                }
            };

            // ===== Top row: [ MODE | STACK ] =====
            var topRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            BuildModeInPlace();
            void BuildModeInPlace()
            {
                // MODE cell (fixed width)
                creator_mode = Card("MODE");
                creator_mode.style.width = Creator_LeftColWidth;
                creator_mode.style.height = Creator_BarHeight;
                creator_mode.style.justifyContent = Justify.Center;
                creator_mode.style.alignItems = Align.Center;
                
                creator_mode.Add(new Label("CREATE")
                {
                    style = { color = TextColorLight, unityFontStyleAndWeight = FontStyle.Bold },
                    name = "LABEL"
                });
            }

            BuildStackTraceInPlace();
            void BuildStackTraceInPlace()
            {
                // STACK bar (fills remainder)
                creator_stacktrace = Card("STACKTRACE");
                creator_stacktrace.style.flexGrow = 1;
                creator_stacktrace.style.height = Creator_BarHeight;
                creator_stacktrace.style.alignItems = Align.FlexStart;
                creator_stacktrace.Add(new Label("Stack")
                {
                    style = { color = TextColorLight, unityFontStyleAndWeight = FontStyle.Bold }
                });
            }

            topRow.Add(creator_mode);
            topRow.Add(creator_stacktrace);
            root.Add(topRow);

            // ===== Second row: two columns =====
            var secondRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, flexGrow = 1 }
            };

            // LEFT column: [ ACTIONS bar | USAGE pane ]
            var leftCol = new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, width = Creator_LeftColWidth, flexShrink = 0 }
            };

            BuildActionsInPlace();
            void BuildActionsInPlace()
            {
                // ===== Actions Bar (Reset/Discard stacked, Save/Create wide) =====
                creator_actions = Card("ACTIONS");
                creator_actions.style.flexDirection = FlexDirection.Row;
                creator_actions.style.alignItems = Align.Stretch;     // children may stretch vertically

// Make the bar tall enough for two rows and leave breathing room
                creator_actions.style.minHeight    = Creator_BarHeight * 2 + 12;
                creator_actions.style.paddingLeft  = 8;
                creator_actions.style.paddingRight = 8;
                creator_actions.style.paddingTop   = 6;
                creator_actions.style.paddingBottom= 6;

// Left column (stacked Reset/Discard)
                var _leftCol = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                        width = 96,           // tweak to taste
                        flexShrink = 0,
                        alignSelf = Align.Stretch,
                        height = StyleKeyword.Auto
                    }
                };
                var resetBtn   = ActionButtonFill("Reset",   () => Debug.Log("Reset clicked"));
                var discardBtn = ActionButtonFill("Discard", () => Debug.Log("Discard clicked"));
                _leftCol.Add(resetBtn);
                _leftCol.Add(discardBtn);

// Right row (Save/Create fill the bar height)
                var rightRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        alignItems = Align.Stretch    // make buttons fill height
                    }
                };
                var saveBtn   = ActionButtonFill("Save",   () => Debug.Log("Save clicked"));
                var createBtn = ActionButtonFill("Create", () => Debug.Log("Create clicked"));
                rightRow.Add(saveBtn);
                rightRow.Add(createBtn);

// Add both groups to actions bar
                creator_actions.Add(_leftCol);
                creator_actions.Add(rightRow);
            }

            BuildUsageInPlace();
            void BuildUsageInPlace()
            {
                creator_usage = Card("USAGE");
                creator_usage.style.flexGrow = 1;
                creator_usage.style.justifyContent = Justify.Center;
                creator_usage.style.alignItems = Align.Center;
                creator_usage.Add(new Label("Usage")
                {
                    style = { color = TextColorLight, unityFontStyleAndWeight = FontStyle.Normal }
                });
            }

            leftCol.Add(creator_actions);
            leftCol.Add(creator_usage);

            // RIGHT column: EDITOR pane
            var rightCol = new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, flexGrow = 1 }
            };

            BuildEditorInPlace();
            void BuildEditorInPlace()
            {
                creator_editor = Card("EDITOR");
                creator_editor.style.flexGrow = 1;
                creator_editor.style.justifyContent = Justify.Center;
                creator_editor.style.alignItems = Align.Center;
                creator_editor.style.backgroundColor = BackgroundColorDeepLight;
                creator_editor.Add(new Label("Editor")
                {
                    style = { color = TextColorLight, unityFontStyleAndWeight = FontStyle.Normal }
                });
            }
            
            rightCol.Add(creator_editor);

            secondRow.Add(leftCol);
            secondRow.Add(rightCol);
            root.Add(secondRow);

            return root;

            // Local helper for consistent “card” look
            VisualElement Card(string _name)
            {
                return new VisualElement
                {
                    name = _name,
                    style =
                    {
                        backgroundColor = BackgroundColorDark,
                        borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                        borderTopColor = EdgeColorDark, borderBottomColor = EdgeColorDark,
                        borderLeftColor = EdgeColorDark, borderRightColor = EdgeColorDark,
                        paddingLeft = 6, paddingRight = 6, paddingTop = 4, paddingBottom = 4
                    }
                };
            }
        }
        
        void StyleActionButton(Button b)
        {
            b.style.flexGrow = 1;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.backgroundColor = BackgroundColorLight;
            b.style.color = TextColorLight;
            b.style.height = Creator_BarHeight - 6;
            b.style.borderTopLeftRadius = 4; b.style.borderTopRightRadius = 4;
            b.style.borderBottomLeftRadius = 4; b.style.borderBottomRightRadius = 4;
            b.RegisterCallback<PointerEnterEvent>(_ => b.style.backgroundColor = BackgroundColorLight * .8f);
            b.RegisterCallback<PointerLeaveEvent>(_ => b.style.backgroundColor = BackgroundColorLight);
        }


        void RefreshCreatorPage()
        {
            if (creator_active is null || creator_active.Id == "-1")
            {
                SetUnfocusedCreator();
                return;
            }
            
            // Mode
            var mode_label = creator_mode.Q<Label>("LABEL");
            mode_label.text = $"{DataTypeText(creator_active.Kind)} › {creator_active.Name}";
            
        }

        /// <summary>
        /// For when there is no creator_active set.
        /// </summary>
        void SetUnfocusedCreator()
        {
            var mode_label = creator_mode.Q<Label>("CREATE");
            mode_label.text = "CREATOR › None";
        }

        void SetCreatorPage(string id, DataType kind)
        {
            var node = Project.Get(id, kind);
            var item = new CreatorItem(node.Name, node.Id, kind);
            item.Data = node.ToData();
            
            // TODO remove later
            item.Data = new Dictionary<Tag, object>();

            SetCreatorPage(item);
        }
        
        void SetCreatorPage(CreatorItem item)
        {
            SaveCreatorItem();

            creator_active = item;
            
            SetPage(GasifyPage.Creator);

            RefreshCreatorPage();
        }

        void SaveCreatorItem()
        {
            if (creator_active is null) return;

            EditorTagService.Save(creator_active);
        }
        
        #endregion
        
        #region Developer Page
        
        VisualElement BuildDeveloperPage()
        {
            var v = new VisualElement { style = { flexGrow = 1 } };
            v.Add(new Label("Developer page placeholder — Actions / Usage / Editor") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            return v;
        }
        
        #endregion
        
        #region Stub Page

        VisualElement BuildStubPage()
        {
            var v = new VisualElement { style = { flexGrow = 1 } };
            v.Add(new Label("This page has no content.") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            return v;
        }
        
        #endregion
    }
    #endif
}
