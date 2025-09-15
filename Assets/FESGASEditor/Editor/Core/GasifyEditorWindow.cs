using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

            public SearchItem(string display, string id, DataType kind, bool isPlaceholder = true)
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
            System = 1, 
            Attribute = 4, 
            Tag = 5, 
            ProxyTask = 7,
            AttributeSet = 8, 
            Modifier = 6, 
            AttributeWorker = 9, 
            ImpactWorker = 19, 
            EffectWorker = 11, 
            TagWorker = 12, 
            ProcessInstantiator = 13
        }

        private static GasifyPage activePage = GasifyPage.Landing;
        
        private const string RootTitle = "FESGAS";
        
        private const string LandingPageTitle = RootTitle;
        private const string HomePageTitle = RootTitle + " — Home";
        private const string CreatePageTitle = RootTitle + " — Create";
        private const string DevelopPageTitle = RootTitle + " — Develop";

        private Color BackgroundColorDark => new Color(0.17f, 0.17f, 0.2f, 1f);
        private Color BackgroundColorLight => new Color(0.27f, 0.27f, 0.3f, 1f);
        private Color BackgroundColorSub => new Color(0.4f, 0.4f, 0.5f, 1f);
        private Color EdgeColorDark => new Color(0, 0, 0, .5f);

        private Color TextColorLight => new Color(0.85f, 0.85f, 0.9f, 1f);
        private Color TextColorSub => new Color(0.75f, 0.75f, 0.75f, 0.95f);

        #region Internal
        
        // Project / data management
        private FrameworkProject Project = new FrameworkProject();
        private GasifyProjectIndex Index = new GasifyProjectIndex();
        private GasifyNavigationService Navigation;
        
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

        private TextField nav_proxySearchField;
        private ListView nav_proxyListView;
        private List<string> nav_recentProxyTasks = new();
        private const int nav_maxRecentProxy = 6;
        
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
        private VisualElement homePage;
        private VisualElement creatorPage;
        private VisualElement developerPage;
        
        #endregion
        
        #endregion

        private Toolbar toolbar;
        private ToolbarMenu filterMenu;
        
        #region Editor
        
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

            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1;
            
            rootVisualElement.style.paddingLeft = 0;
            rootVisualElement.style.paddingRight = 0;
            rootVisualElement.style.paddingTop = 0;
            rootVisualElement.style.paddingBottom = 0;
            
            navBar = BuildNavigationBar();
            content = BuildContent();
            overlay = BuildOverlay();
            
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
            
            content.RegisterCallback<MouseDownEvent>(_ => CloseAllDropdowns());
            
            SetPage(activePage);
        }

        private void SetPage(GasifyPage page)
        {
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
        
        #endregion
        
        #region Helpers
        
        #region Buttons
        
        private const float _primaryButtonColor_r = .36f;
        private const float _primaryButtonColor_g = .32f;
        private const float _primaryButtonColor_b = .7f;
        Button PrimaryButton(string text, System.Action onClick, int height = 28, int radius = 8)
        {
            return PrimaryButton(text, onClick, new Color(_primaryButtonColor_r, _primaryButtonColor_g, _primaryButtonColor_b), height, radius);
        }
        
        Button PrimaryButton(string text, System.Action onClick, Color color, int height = 28, int radius = 8)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            b.style.height = height;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = color; // purple-ish
            b.style.color = Color.white;
            b.style.borderTopLeftRadius = radius;
            b.style.borderTopRightRadius = radius;
            b.style.borderBottomLeftRadius = radius;
            b.style.borderBottomRightRadius = radius;
            return b;
        }

        private const float _secondaryButtonColor_r = .95f;
        private const float _secondaryButtonColor_g = .95f;
        private const float _secondaryButtonColor_b = 1f;
        Button SecondaryButton(string text, System.Action onClick, int height = 24, int radius = 8)
        {
            return SecondaryButton(text, onClick, new Color(_secondaryButtonColor_r, _secondaryButtonColor_g, _secondaryButtonColor_b), height, radius);
        }
        
        Button SecondaryButton(string text, System.Action onClick, Color color, int height = 24, int radius = 8)
        {
            var b = new Button(() => onClick?.Invoke()) { text = text };
            b.style.height = height;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = new Color(0.22f, 0.22f, 0.26f, 1f);
            b.style.color = color;
            b.style.borderTopLeftRadius = radius;
            b.style.borderTopRightRadius = radius;
            b.style.borderBottomLeftRadius = radius;
            b.style.borderBottomRightRadius = radius;
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

        /*void ToggleButtonDropdown_Navigation(VisualElement button, VisualElement dd)
        {
            if (dd != nav_createDropdown) nav_createDropdown.style.display = DisplayStyle.None;
            if (dd != nav_developDropdown) nav_developDropdown.style.display = DisplayStyle.None;
            if (dd != nav_searchDropdown) nav_searchDropdown.style.display = DisplayStyle.None;
            // if (dd != nav_optionsDropdown) nav_optionsDropdown.style.display = DisplayStyle.None;

            if (dd.style.display == DisplayStyle.Flex)
            {
                dd.style.display = DisplayStyle.None;
                return;
            }

            PositionDropdownUnderElement(button, dd);
            dd.style.display = DisplayStyle.Flex;
        }*/

        void PositionDropdownUnderElement(VisualElement target, VisualElement dd)
        {
            var targetWB = target.worldBound;
            var wrapOrigin = navBar.worldBound.position;
            dd.style.left = targetWB.xMin - wrapOrigin.x;
            dd.style.top = targetWB.yMax - wrapOrigin.y;
        }

        void MaybeHideDropdown(VisualElement dd, Func<bool> predicate)
        {
            if (dd.style.display == DisplayStyle.None) return;
            Delay(.15f, () =>
            {
                if (predicate())
                {
                    dd.style.display = DisplayStyle.None;
                    Debug.Log($"Hide {dd.name}");
                }
            });
        }

        void CloseAllDropdowns()
        {
            CloseAllDropdowns_Navigation();
        }
        
        void CloseAllDropdowns_Navigation()
        {
            nav_createDropdown.style.display = DisplayStyle.None;
            nav_developDropdown.style.display = DisplayStyle.None;
            nav_searchDropdown.style.display = DisplayStyle.None;
            // nav_optionsDropdown.style.display = DisplayStyle.None;
        }

        VisualElement NewDropdownBase(float minWidth)
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    display  = DisplayStyle.None,
                    backgroundColor = new Color(0.14f,0.14f,0.16f,1f),
                    borderTopLeftRadius = 6, borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6, borderBottomRightRadius = 6,
                    borderBottomWidth = 1, borderTopWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderBottomColor = new Color(0,0,0,0.5f), borderTopColor = new Color(0,0,0,0.5f),
                    borderLeftColor   = new Color(0,0,0,0.5f), borderRightColor = new Color(0,0,0,0.5f),
                    paddingTop = 4, paddingBottom = 6, paddingLeft = 6, paddingRight = 6,
                    minWidth = minWidth
                }
            };
        }
        
        VisualElement NewColumn()
        {
            return new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, flexGrow = 1, marginRight = 8 }
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

        VisualElement BuildSeparator(float thickness = 1f)
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
        VisualElement MakeNavButton(string text, Action onClick)
        {
            var btn = new Button(() => onClick?.Invoke()) { text = text };
            btn.style.height = 26;
            btn.style.marginRight = 6;
            btn.style.paddingLeft = 10; btn.style.paddingRight = 10;
            btn.style.backgroundColor = new Color(0.22f, 0.22f, 0.26f, 1f);
            btn.style.color = new Color(0.95f, 0.95f, 1f, 1f);
            btn.style.borderTopLeftRadius = 8; btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8; btn.style.borderBottomRightRadius = 8;
            btn.RegisterCallback<PointerEnterEvent>(_ => btn.style.backgroundColor = new Color(0.20f,0.20f,0.24f,1f));
            btn.RegisterCallback<PointerLeaveEvent>(_ => btn.style.backgroundColor = new Color(0.22f,0.22f,0.26f,1f));
            return btn;
        }

        VisualElement MakeNavButton(string text, string icon, Action onClick)
        {
            var btn = MakeNavButton(text, onClick);
            var img = EditorGUIUtility.IconContent(icon).image as Texture2D;
            var _icon = new Image
            {
                image = img,
                scaleMode = ScaleMode.ScaleToFit,
                style = { width = 16, height = 16, marginRight = 6 }
            };
            btn.Add(_icon);
            return btn;
        }

        // Positions a dropdown flush under a target (no gap) and shows it
        void ToggleButtonDropdown_Navigation(VisualElement button, VisualElement dd)
        {
            if (dd == null) return;

            // Hide others
            if (dd != nav_createDropdown)  nav_createDropdown.style.display  = DisplayStyle.None;
            if (dd != nav_developDropdown) nav_developDropdown.style.display = DisplayStyle.None;
            if (dd != nav_searchDropdown)  nav_searchDropdown.style.display  = DisplayStyle.None;

            if (dd.style.display == DisplayStyle.Flex)
            {
                dd.style.display = DisplayStyle.None;
                return;
            }

            SetElement(overlay, true);

            var targetWB   = button.worldBound;
            var wrapOrigin = overlay.worldBound.position;
            dd.style.left = targetWB.xMin - wrapOrigin.x;
            dd.style.top  = targetWB.yMax - wrapOrigin.y; // flush (prevents tiny dead zone)
            dd.style.display = DisplayStyle.Flex;
        }

        void SetElement(VisualElement ve, bool flag)
        {
            ve.style.display = flag ? DisplayStyle.Flex : DisplayStyle.None;
        }

        
        #endregion
        
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
                    backgroundColor = BackgroundColorLight,
                    // square edges so it looks like a single rectangle across the top
                    borderTopLeftRadius = 0, borderTopRightRadius = 0,
                    borderBottomLeftRadius = 0, borderBottomRightRadius = 0,

                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
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

            nav_homeButton = MakeNavButton("", "Prefab Icon", OnHomeClicked_Nav);
            nav_createButton = MakeNavButton("Create", OnCreateClicked_Nav);
            nav_developButton = MakeNavButton("Develop", OnDevelopClicked_Nav);

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
            var optionsBtn = MakeNavButton("Options", null);
            bar.Add(optionsBtn);

            // Build a Unity menu and open it on click
            var optionsMenu = new GenericMenu();
            optionsMenu.AddItem(new GUIContent("Open JSON…"), false, () => Debug.Log("Open JSON"));
            optionsMenu.AddItem(new GUIContent("Save JSON"), false, () => Debug.Log("Save JSON"));
            optionsMenu.AddSeparator("");
            optionsMenu.AddItem(new GUIContent("Settings"), false, () => Debug.Log("Open Settings"));
            optionsMenu.AddItem(new GUIContent("Documentation"), false, () => Application.OpenURL("https://example.com/docs"));
            optionsMenu.AddItem(new GUIContent("About"), false, () => EditorUtility.DisplayDialog("About FES", "Framework Editor Suite\n© You", "OK"));

            optionsBtn.RegisterCallback<MouseUpEvent>(evt =>
            {
                var r = new Rect(evt.mousePosition, Vector2.zero);
                optionsMenu.DropDown(r);
            });

            // === Build dropdown panels (Create / Develop / Search) ===
            nav_createDropdown  = BuildCreateDropdown_NewStyle();
            nav_developDropdown = BuildDevelopDropdown_NewStyle();
            nav_searchDropdown  = BuildSearchDropdown_NewStyle();

            // dropdowns live at the same level as the bar so they can overlap content
            _navWrap.Add(nav_createDropdown);
            _navWrap.Add(nav_developDropdown);
            _navWrap.Add(nav_searchDropdown);

            // Ensure dropdowns actually receive pointer events (prevents flicker)
            nav_createDropdown.pickingMode  = PickingMode.Position;
            nav_developDropdown.pickingMode = PickingMode.Position;
            nav_searchDropdown.pickingMode  = PickingMode.Position;

            // Root hookup
            rootVisualElement.Add(_navWrap);

            // ---------- Leave-to-close wiring (no open-on-hover) ----------
            AttachHoverGroup(nav_createButton,  nav_createDropdown);
            AttachHoverGroup(nav_developButton, nav_developDropdown);
            AttachHoverGroup(searchWrap,         nav_searchDropdown); // field container + dropdown

            // Also close any open dropdown when clicking outside
            rootVisualElement.RegisterCallback<MouseDownEvent>(_ => CloseAllDropdowns());

            return _navWrap;

            // ---------- Local builders for the three dropdowns with matching style ----------
            VisualElement BuildCreateDropdown_NewStyle()
            {
                var dd = NewDropdownBase(minWidth: 3 * 240); // card style consistent with landing
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                dd.Add(row);

                var left  = NewColumn();
                var mid   = NewColumn();
                var right = NewColumn();
                row.Add(left); row.Add(mid); row.Add(right);

                var icon = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D;

                // LEFT: Data + Keys + Special
                left.Add(BuildHeader("Data"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Ability",  () => Debug.Log("Create Ability"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Effect",   () => Debug.Log("Create Effect"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "System",   () => Debug.Log("Create System"))));
                left.Add(BuildSeparator());
                left.Add(BuildHeader("Keys"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute",() => Debug.Log("Create Attribute"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Tag",      () => Debug.Log("Create Tag"))));
                left.Add(BuildSeparator());
                left.Add(BuildHeader("Special"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute Set", () => Debug.Log("Create Attribute Set"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Modifier",      () => Debug.Log("Create Modifier"))));

                // MIDDLE: Workers + Process
                mid.Add(BuildHeader("Workers"));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute", () => Debug.Log("Create Worker: Attribute"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Impact",    () => Debug.Log("Create Worker: Impact"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Effect",    () => Debug.Log("Create Worker: Effect"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Tag",       () => Debug.Log("Create Worker: Tag"))));
                mid.Add(BuildSeparator());
                mid.Add(BuildHeader("Process"));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Instantiator", () => Debug.Log("Create Process: Instantiator"))));

                // RIGHT: Proxy Task (search + recent)
                right.Add(BuildHeader("Proxy Task"));

                nav_proxySearchField = new TextField { tooltip = "Search Proxy Tasks" };
                nav_proxySearchField.style.marginTop = 4;
                nav_proxySearchField.RegisterValueChangedCallback(evt => RefreshProxyTaskList(evt.newValue));
                right.Add(nav_proxySearchField);

                nav_proxyListView = new ListView
                {
                    selectionType = SelectionType.Single,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    style = { height = 240, marginTop = 4 }
                };
                nav_proxyListView.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
                nav_proxyListView.bindItem = (ve, idx) =>
                {
                    var data = (List<string>)nav_proxyListView.itemsSource;
                    if (idx >= 0 && idx < data.Count) ((Label)ve).text = data[idx];
                };
                nav_proxyListView.selectionChanged += items =>
                {
                    var sel = items.FirstOrDefault() as string;
                    if (string.IsNullOrEmpty(sel) || sel.StartsWith("—")) return;

                    // recent list update
                    nav_recentProxyTasks.Remove(sel);
                    nav_recentProxyTasks.Insert(0, sel);
                    if (nav_recentProxyTasks.Count > nav_maxRecentProxy) nav_recentProxyTasks.RemoveAt(nav_recentProxyTasks.Count - 1);

                    Debug.Log("Create Proxy Task: " + sel);
                    dd.style.display = DisplayStyle.None; // close after choose
                };
                right.Add(nav_proxyListView);

                // First populate (recent if empty)
                RefreshProxyTaskList("");

                return dd;

                void RefreshProxyTaskList(string query)
                {
                    List<string> src;
                    var q = (query ?? "").Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(q))
                    {
                        src = nav_recentProxyTasks.Count > 0
                            ? new List<string>(nav_recentProxyTasks)
                            : new List<string> { "— No recent proxy tasks —" };
                    }
                    else
                    {
                        // Using your Project.ProxyTasks (already in your file)
                        src = Project.ProxyTasks
                            .Where(t => t.Name != null && t.Name.ToLowerInvariant().Contains(q))
                            .Select(t => t.Name)
                            .ToList();
                        if (src.Count == 0) src.Add("— No matches —");
                    }
                    nav_proxyListView.itemsSource = src;
                    nav_proxyListView.Rebuild();
                }
            }

            VisualElement BuildDevelopDropdown_NewStyle()
            {
                var dd = NewDropdownBase(minWidth: 3 * 240);
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

            VisualElement BuildSearchDropdown_NewStyle()
            {
                var dd = NewDropdownBase(minWidth: 280);

                nav_searchList = new ListView
                {
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    selectionType = SelectionType.Single,
                    showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                    style = { width = 280, height = 220 }
                };
                nav_searchList.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
                nav_searchList.bindItem = (e, i) =>
                {
                    if (i < 0 || i >= nav_searchItems.Count) return;
                    var item = nav_searchItems[i];
                    ((Label)e).text = item.Display;
                    e.SetEnabled(!item.IsPlaceholder);
                };
                nav_searchList.selectionChanged += OnSearchItemChosen;

                dd.Add(nav_searchList);
                return dd;
            }
        }

        
        #region OLD NAV BAR
        /*void BuildNavigationBar()
        {
            navWrap = new VisualElement()
            {
                style =
                {
                    position = Position.Relative,
                    flexDirection = FlexDirection.Column,
                    display = DisplayStyle.None
                }
            };

            navBar = new Toolbar();

            nav_homeButton = new ToolbarButton(OnHomeClicked_Nav)
            {
                text = "Home"
            };
            
            nav_createButton = new ToolbarButton(OnCreateClicked_Nav)
            {
                text = "Create"
            };
            
            nav_developButton = new ToolbarButton(OnDevelopClicked_Nav)
            {
                text = "Develop"
            };
            
            navBar.Add(nav_homeButton);
            navBar.Add(nav_createButton);
            navBar.Add(nav_developButton);
            
            navBar.Add(new ToolbarSpacer() { style = { flexGrow = 1 }});
            
            // Search field
            nav_searchField = new TextField() { name = "FES-SearchField" };
            nav_searchField.style.width = 340;
            nav_searchField.style.marginTop = 2; // align nicely in toolbar
            nav_searchField.style.marginBottom = 2;
            nav_searchField.label = ""; // no label, placeholder instead
            // nav_searchField.tooltip = "Search (IDs, names, tags)";
            nav_searchField.RegisterValueChangedCallback(OnSearchChanged_Nav);
            nav_searchField.RegisterCallback<FocusInEvent>(_ =>
            {
                Debug.Log($"Focus in");
                OpenSearchDropdown_Nav();
                if (string.IsNullOrEmpty(nav_searchField.value)) ShowNoSearchResults_Nav();
            });
            // nav_overSearchField
            nav_searchField.RegisterCallback<FocusOutEvent>(_ => Delay(.05f, () => { MaybeHideDropdown(nav_searchDropdown, () => !nav_overSearchField && !nav_overSearchDD); Debug.Log($"Focus out"); }));
            // nav_searchField.RegisterCallback<FocusOutEvent>(_ => Delay(.05f, () => MaybeHideDropdown(nav_searchDropdown, () => !nav_overSearchField && !nav_overSearchDD)));
            navBar.Add(nav_searchField);
            
            // Options
            var optionsMenu = new ToolbarMenu { text = "Options" };
            optionsMenu.menu.AppendAction("Open JSON…", _ => Debug.Log("Open JSON"));
            optionsMenu.menu.AppendAction("Save JSON", _ => Debug.Log("Save JSON"));
            optionsMenu.menu.AppendSeparator("");
            optionsMenu.menu.AppendAction("Settings", _ => Debug.Log("Open Settings"));
            optionsMenu.menu.AppendAction("Documentation", _ => Application.OpenURL("https://example.com/docs"));
            optionsMenu.menu.AppendAction("About", _ => EditorUtility.DisplayDialog("About FES", "Framework Editor Suite\n© You", "OK"));
            navBar.Add(optionsMenu);

            navWrap.Add(navBar);

            nav_createDropdown = BuildCreateDropdown();
            nav_developDropdown = BuildDevelopDropdown();
            nav_searchDropdown = BuildSearchDropdown();
            
            navWrap.Add(nav_createDropdown);
            navWrap.Add(nav_developDropdown);
            navWrap.Add(nav_searchDropdown);
            
            /*nav_createDropdown.pickingMode  = PickingMode.Position;
            nav_developDropdown.pickingMode = PickingMode.Position;
            nav_searchDropdown.pickingMode  = PickingMode.Position;#1#

            rootVisualElement.Add(navWrap);

            /*AttachHoverGroup(nav_createButton, nav_createDropdown);
            AttachHoverGroup(nav_developButton, nav_developDropdown);
            AttachHoverGroup(nav_searchField, nav_searchDropdown);#1#
            
            // Hover tracking used ONLY to detect leave → close (no open on hover)
            TrackHover(nav_createButton,       over => { nav_overCreateButton   = over; MaybeHideDropdown(nav_createDropdown,   () => !nav_overCreateButton   && !nav_overCreateDD); });
            TrackHover(nav_createDropdown,  over => { nav_overCreateDD    = over; MaybeHideDropdown(nav_createDropdown,   () => !nav_overCreateButton   && !nav_overCreateDD); });
            TrackHover(nav_developButton,      over => { nav_overDevelopButton  = over; MaybeHideDropdown(nav_developDropdown,  () => !nav_overDevelopButton  && !nav_overDevelopDD); });
            TrackHover(nav_developDropdown, over => { nav_overDevelopDD   = over; MaybeHideDropdown(nav_developDropdown,  () => !nav_overDevelopButton  && !nav_overDevelopDD); });
            TrackHover(nav_searchField,     over => { nav_overSearchField = over; MaybeHideDropdown(nav_searchDropdown,   () => !nav_overSearchField && !nav_overSearchDD); });
            TrackHover(nav_searchDropdown,  over => { nav_overSearchDD    = over; MaybeHideDropdown(nav_searchDropdown,   () => !nav_overSearchField && !nav_overSearchDD); });
            
            return;
            
            VisualElement BuildCreateDropdown()
            {
                var dd = NewDropdownBase(minWidth: 3 * 240); // a bit wider for 3 columns

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                dd.Add(row);

                var left  = NewColumn();
                var mid   = NewColumn();
                var right = NewColumn();

                row.Add(left); row.Add(mid); row.Add(right);

                // Placeholder icon (replace with your icons)
                var icon = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D;

                // ----- LEFT: Data + Keys -----
                left.Add(BuildHeader("Data"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Ability",  () => Debug.Log("Create Ability"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Effect",   () => Debug.Log("Create Effect"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "System",   () => Debug.Log("Create System"))));

                left.Add(BuildSeparator());
                left.Add(BuildHeader("Keys"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute",() => Debug.Log("Create Attribute"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Tag",      () => Debug.Log("Create Tag"))));

                left.Add(BuildSeparator());
                left.Add(BuildHeader("Special"));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute Set", () => Debug.Log("Create Attribute Set"))));
                left.Add(BuildIconTextItem(new MenuEntry(icon, "Modifier",      () => Debug.Log("Create Modifier"))));

                // ----- MIDDLE: Workers + Process -----
                mid.Add(BuildHeader("Workers"));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Attribute", () => Debug.Log("Create Worker: Attribute"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Impact",    () => Debug.Log("Create Worker: Impact"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Effect",    () => Debug.Log("Create Worker: Effect"))));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Tag",       () => Debug.Log("Create Worker: Tag"))));

                mid.Add(BuildSeparator());
                mid.Add(BuildHeader("Process"));
                mid.Add(BuildIconTextItem(new MenuEntry(icon, "Instantiator", () => Debug.Log("Create Process: Instantiator"))));

                // ----- RIGHT: Proxy Task (search+recent) -----
                right.Add(BuildHeader("Proxy Task"));

                nav_proxySearchField = new TextField { tooltip = "Search Proxy Tasks" };
                nav_proxySearchField.style.marginTop = 4;
                nav_proxySearchField.RegisterValueChangedCallback(evt => RefreshProxyTaskList(evt.newValue));
                right.Add(nav_proxySearchField);

                nav_proxyListView = new ListView
                {
                    selectionType = SelectionType.Single,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    style = { height = 240, marginTop = 4 }
                };
                nav_proxyListView.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
                nav_proxyListView.bindItem = (ve, idx) =>
                {
                    var data = (List<string>) nav_proxyListView.itemsSource;
                    if (idx >= 0 && idx < data.Count) ((Label)ve).text = data[idx];
                };
                nav_proxyListView.selectionChanged += items =>
                {
                    var sel = items.FirstOrDefault() as string;
                    if (string.IsNullOrEmpty(sel)) return;

                    // Add to recent list (most-recent-first, unique, capped)
                    nav_recentProxyTasks.Remove(sel);
                    nav_recentProxyTasks.Insert(0, sel);
                    if (nav_recentProxyTasks.Count > nav_maxRecentProxy) nav_recentProxyTasks.RemoveAt(nav_recentProxyTasks.Count - 1);

                    Debug.Log("Create Proxy Task: " + sel);
                    dd.style.display = DisplayStyle.None; // close after choose
                };
                right.Add(nav_proxyListView);

                // Initial population (show recent when empty)
                RefreshProxyTaskList("");

                return dd;

                void RefreshProxyTaskList(string query)
                {
                    List<string> source;
                    var q = (query ?? "").Trim().ToLowerInvariant();

                    if (string.IsNullOrEmpty(q))
                    {
                        // Empty → show recent; if none, show a hint
                        source = nav_recentProxyTasks.Count > 0 ? new List<string>(nav_recentProxyTasks)
                            : new List<string> { "— No recent proxy tasks —" };
                    }
                    else
                    {
                        source = Project.ProxyTasks.Where(t => t.Name.ToLowerInvariant().Contains(q)).Select(t => t.Name).ToList();
                        if (source.Count == 0) source.Add("— No matches —");
                    }

                    nav_proxyListView.itemsSource = source;
                    nav_proxyListView.Rebuild();
                }
            }

            VisualElement BuildDevelopDropdown()
            {
                var dd = NewDropdownBase(minWidth: 3 * 240);

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                dd.Add(row);

                var colA = NewColumn();
                var colB = NewColumn();
                var colC = NewColumn();

                row.Add(colA); row.Add(colB); row.Add(colC);

                var icon = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D;

                // Column A: Build
                colA.Add(BuildHeader("Build"));
                colA.Add(BuildIconTextItem(new MenuEntry(icon, "Content Build", () => Debug.Log("Content Build"))));
                colA.Add(BuildIconTextItem(new MenuEntry(icon, "Rebuild All",   () => Debug.Log("Rebuild All"))));
                colA.Add(BuildIconTextItem(new MenuEntry(icon, "Clean Build",   () => Debug.Log("Clean Build"))));

                // Column B: Validate
                colB.Add(BuildHeader("Validate"));
                colB.Add(BuildIconTextItem(new MenuEntry(icon, "Validate Project", () => Debug.Log("Validate Project"))));
                colB.Add(BuildIconTextItem(new MenuEntry(icon, "Lint Data",        () => Debug.Log("Lint Data"))));
                colB.Add(BuildIconTextItem(new MenuEntry(icon, "Find Duplicates",  () => Debug.Log("Find Duplicates"))));

                // Column C: Utilities
                colC.Add(BuildHeader("Utilities"));
                colC.Add(BuildIconTextItem(new MenuEntry(icon, "Run Tests",      () => Debug.Log("Run Tests"))));
                colC.Add(BuildIconTextItem(new MenuEntry(icon, "Open Logs",      () => Debug.Log("Open Logs"))));
                colC.Add(BuildIconTextItem(new MenuEntry(icon, "Export Report",  () => Debug.Log("Export Report"))));

                return dd;
            }
            
            VisualElement BuildSearchDropdown()
            {
                var dd = NewDropdownBase(minWidth: 280);

                nav_searchList = new ListView
                {
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                    selectionType = SelectionType.Single,
                    showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                    style = { width = 280, height = 220 }
                };
                nav_searchList.makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 8, paddingRight = 8 } };
                nav_searchList.bindItem = (e, i) =>
                {
                    if (i < 0 || i >= nav_searchItems.Count) return;
                    var item = nav_searchItems[i];
                    ((Label)e).text = item.Display;
                    e.SetEnabled(!item.IsPlaceholder);
                };
                nav_searchList.selectionChanged += OnSearchItemChosen;

                dd.Add(nav_searchList);
                return dd;
            }
            
            void BuildOptionsDropdown()
            {
                nav_optionsButton = new ToolbarButton(OnDevelopClicked_Nav)
                {
                    text = "Develop"
                };
            }
        }*/

        

        #endregion
        
        void OnHomeClicked_Nav()
        {
            SetPage(activePage == GasifyPage.Home ? GasifyPage.Landing : GasifyPage.Home);
        }

        void OnCreateClicked_Nav()
        {
            ToggleButtonDropdown_Navigation(nav_createButton, nav_createDropdown);
        }

        void OnDevelopClicked_Nav()
        {
            ToggleButtonDropdown_Navigation(nav_developButton, nav_developDropdown);
        }

        void OnSearchClicked_Nav()
        {
            
        }

        void OnOptionsClicked_Nav()
        {
            
        }

        #region Search
        
        void OpenSearchDropdown_Nav()
        {
            // Position the dropdown right under the search field
            var local = nav_searchField.worldBound;              // world rect of field
            var parentSpace = navBar.worldBound.position;    // top-left of nav wrap
            var x = local.xMin - parentSpace.x;
            var y = local.yMax - parentSpace.y;

            nav_searchDropdown.style.left = x;
            nav_searchDropdown.style.top = y;
            nav_searchDropdown.style.width = local.width;
            nav_searchDropdown.style.display = DisplayStyle.Flex;

            // Size the list
            nav_searchList.style.width = local.width;
            nav_searchList.style.height = 220;

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

            nav_searchList.itemsSource = nav_searchItems;
            nav_searchList.Rebuild();
        }

        static bool Matches(string name, string id, string q)
        {
            if (string.IsNullOrEmpty(q)) return true;
            return (!string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains(q)) ||
                   (!string.IsNullOrEmpty(id)   && id.ToLowerInvariant().Contains(q));
        }

        void OnSearchItemChosen(IEnumerable<object> selection)
        {
            var item = selection?.FirstOrDefault() as SearchItem;
            if (item == null) return;

            // Navigate depending on item.Kind (here we just go to Home and pretend to open it)
            SetPage(GasifyPage.Home);
            // TODO: tell your Home/Creator page to focus the selected object by ID (item.Id)
            CloseSearchDropdown_Nav();
        }
        
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
                    paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0
                }
            };
            return v;
        }

        VisualElement BuildOverlay()
        {
            var v = new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                    // position = Position.Absolute,
                    left = 0, right = 0, top = 0, bottom = 0,
                    marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 0,
                    paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0,
                    display = DisplayStyle.None
                }
            };
            return v;
        }

        
        #endregion
        
        #region Home Page
        
        VisualElement BuildHomePage()
        {
            var v = new VisualElement { style = { flexGrow = 1 } };
            v.Add(new Label("Home Page Placeholder — Navigation / Project / Usage Tree") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            return v;
        }
        
        #endregion
        
        #region Creator Page
        
        VisualElement BuildCreatorPage()
        {
            var v = new VisualElement { style = { flexGrow = 1 } };
            v.Add(new Label("Creator page placeholder — Actions / Usage / Editor") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            return v;
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
