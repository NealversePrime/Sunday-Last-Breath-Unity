using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Data;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// In-game Developer Window & Live State Editor port of the Twine Debug Panel (F2).
    /// Runs on its own persistent root Canvas (SortingOrder 32700) so it works EVERYWHERE:
    /// in the Main Menu / Title Screen, during Character Creation, in the Apartment Hub,
    /// in Hallways, and during Tactical Combat!
    /// </summary>
    public class DeveloperWindowModal : MonoBehaviour
    {
        public static DeveloperWindowModal Instance { get; private set; }

        public enum DevTab
        {
            Presets,
            VitalsAndStats,
            SkillsAndLevel,
            TraitsAndPerks,
            LoadoutAndItems,
            TimeAndClock,
            StoryFlags,
            FastTravel,
            CombatTester
        }

        private DevTab currentTab = DevTab.Presets;
        private CharacterData currentPlayer;
        private CanvasGroup modalCanvasGroup;

        // UI references
        private GameObject chassisContainer;
        private RectTransform chassisRect;
        private Transform navSidebarTransform;
        private Transform contentAreaTransform;
        private readonly Dictionary<DevTab, Button> tabButtons = new Dictionary<DevTab, Button>();
        private readonly Dictionary<DevTab, GameObject> tabPanels = new Dictionary<DevTab, GameObject>();

        // Reusable Colors (Matches Twine Debug Panel theme: #ff6b35 amber, #0b0e14 dark slate)
        private static readonly Color AmberAccent = new Color(1.0f, 0.42f, 0.21f, 1f);
        private static readonly Color BgDark = new Color(0.04f, 0.05f, 0.08f, 0.98f);
        private static readonly Color ButtonNormalBg = new Color(0.12f, 0.14f, 0.18f, 1f);
        private static readonly Color ButtonActiveBg = new Color(0.25f, 0.15f, 0.10f, 1f);
        private static readonly Color GreenSuccess = new Color(0.20f, 0.78f, 0.35f, 1f);
        private static readonly Color RedDanger = new Color(0.90f, 0.28f, 0.28f, 1f);

        private float lastToggleTime = -1f;

        public bool IsOpen => chassisContainer != null && chassisContainer.activeSelf;

        // ====================================================================
        // PUBLIC STATIC API
        // ====================================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            EnsureInstance();
        }

        public static void Toggle()
        {
            var modal = EnsureInstance();
            if (modal == null) return;

            if (modal.IsOpen)
            {
                modal.CloseInternal();
            }
            else
            {
                modal.OpenInternal();
            }
        }

        public static void Open()
        {
            var modal = EnsureInstance();
            if (modal != null) modal.OpenInternal();
        }

        public static void Close()
        {
            var modal = EnsureInstance();
            if (modal != null) modal.CloseInternal();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Sunday's Last Breath/Toggle Developer Window (F2) _F2", priority = 50)]
        private static void DevMenuToggle()
        {
            if (Application.isPlaying)
            {
                Toggle();
            }
            else
            {
                Debug.LogWarning("[DeveloperWindowModal] Enter Play Mode first to open the Developer Window (F2)!");
            }
        }
#endif

        public static DeveloperWindowModal EnsureInstance()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<DeveloperWindowModal>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            // Create dedicated independent Root with its own ScreenSpaceOverlay Canvas!
            GameObject rootGo = new GameObject("[DeveloperWindow_GlobalCanvas]", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(DeveloperWindowModal));
            DontDestroyOnLoad(rootGo);

            Canvas canvas = rootGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32700; // Always render on top of every title screen, UI, or dialog

            CanvasScaler scaler = rootGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Instance = rootGo.GetComponent<DeveloperWindowModal>();
            return Instance;
        }

        // ====================================================================
        // LIFECYCLE & INPUT
        // ====================================================================
        private void Awake()
        {
            Instance = this;
            BuildUI();
            if (chassisContainer != null) chassisContainer.SetActive(false);
        }

        private void Update()
        {
            bool f2Pressed = false;
            bool escPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                f2Pressed = UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame;
                escPressed = UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
            }
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F2)) f2Pressed = true;
            if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif

            if (f2Pressed)
            {
                if (Time.unscaledTime - lastToggleTime > 0.25f)
                {
                    lastToggleTime = Time.unscaledTime;
                    Toggle();
                }
            }
            else if (escPressed && IsOpen)
            {
                lastToggleTime = Time.unscaledTime;
                CloseInternal();
            }
        }

        private void OpenInternal()
        {
            lastToggleTime = Time.unscaledTime;

            currentPlayer = GameManager.Instance != null ? GameManager.Instance.CurrentPlayer : null;
            if (currentPlayer == null && ApartmentHubScreen.Instance != null)
            {
                currentPlayer = ApartmentHubScreen.Instance.CurrentPlayer;
            }
            if (currentPlayer == null)
            {
                currentPlayer = new CharacterData { firstName = "Neal", lastName = "Sage", job = "SURVIVOR", hp = 100, hpMax = 100, stamina = 100, staminaMax = 100 };
                if (GameManager.Instance != null) GameManager.Instance.SetCurrentPlayer(currentPlayer);
            }

            if (chassisContainer != null) chassisContainer.SetActive(true);

            if (modalCanvasGroup == null)
            {
                modalCanvasGroup = chassisContainer.GetComponent<CanvasGroup>();
                if (modalCanvasGroup == null) modalCanvasGroup = chassisContainer.AddComponent<CanvasGroup>();
            }
            modalCanvasGroup.alpha = 1f;
            modalCanvasGroup.interactable = true;
            modalCanvasGroup.blocksRaycasts = true;

            SelectTab(currentTab);
        }

        private void CloseInternal()
        {
            lastToggleTime = Time.unscaledTime;
            if (chassisContainer != null) chassisContainer.SetActive(false);

            ApartmentHubScreen.Instance?.UpdateVitalsUI();
            ApartmentHubScreen.Instance?.RefreshPDAStatusTab();
        }

        // ====================================================================
        // PROCEDURAL UI GENERATION
        // ====================================================================
        private void BuildUI()
        {
            // Chassis Container covers full screen with transparent scrim
            chassisContainer = new GameObject("ChassisContainer", typeof(RectTransform), typeof(Image));
            chassisContainer.transform.SetParent(transform, false);
            RectTransform rootRt = chassisContainer.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image scrim = chassisContainer.GetComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.85f);

            // Centered Main Window Chassis (1160 x 720)
            GameObject chassis = new GameObject("DevChassis", typeof(RectTransform), typeof(Image), typeof(Outline));
            chassis.transform.SetParent(chassisContainer.transform, false);
            chassisRect = chassis.GetComponent<RectTransform>();
            chassisRect.anchorMin = new Vector2(0.5f, 0.5f);
            chassisRect.anchorMax = new Vector2(0.5f, 0.5f);
            chassisRect.pivot = new Vector2(0.5f, 0.5f);
            chassisRect.sizeDelta = new Vector2(1160f, 720f);
            chassisRect.anchoredPosition = Vector2.zero;

            chassis.GetComponent<Image>().color = BgDark;
            Outline outline = chassis.GetComponent<Outline>();
            outline.effectColor = AmberAccent;
            outline.effectDistance = new Vector2(2f, -2f);

            // ----------------------------------------------------------------
            // 1. TOP TOOLBAR (Height 52px)
            // ----------------------------------------------------------------
            GameObject toolbar = new GameObject("Toolbar", typeof(RectTransform), typeof(Image));
            toolbar.transform.SetParent(chassis.transform, false);
            RectTransform tbRt = toolbar.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 1f);
            tbRt.anchorMax = new Vector2(1f, 1f);
            tbRt.pivot = new Vector2(0.5f, 1f);
            tbRt.sizeDelta = new Vector2(0f, 52f);
            tbRt.anchoredPosition = Vector2.zero;
            toolbar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 1f);

            // Title & Dev Badge (ASCII only, no emoji warnings)
            TextMeshProUGUI titleTxt = CreateText(toolbar.transform, "Title", "[DEV] <b>DEVELOPER WINDOW</b>  <color=#FF6B35>[STATE EDITOR // DEV BUILD]</color>", 15f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform titRt = titleTxt.GetComponent<RectTransform>();
            titRt.anchorMin = new Vector2(0f, 0f);
            titRt.anchorMax = new Vector2(0.48f, 1f);
            titRt.offsetMin = new Vector2(16f, 0f);
            titRt.offsetMax = Vector2.zero;

            // Quick Toolbar Actions: MAX ALL, MIN ALL, HEAL, CLOSE
            Transform actRoot = CreateContainer(toolbar.transform, "Actions", new Vector2(0.48f, 0f), new Vector2(1f, 1f));
            CreateButton(actRoot, "MaxAllBtn", "MAX ALL", new Vector2(-360f, 0f), new Vector2(96f, 32f), () => ApplyPresetMaxAll(), GreenSuccess);
            CreateButton(actRoot, "MinAllBtn", "MIN ALL", new Vector2(-254f, 0f), new Vector2(96f, 32f), () => ApplyPresetMinAll(), RedDanger);
            CreateButton(actRoot, "HealAllBtn", "HEAL 100%", new Vector2(-148f, 0f), new Vector2(96f, 32f), () => HealPlayerFull(), AmberAccent);
            CreateButton(actRoot, "CloseBtn", "X RESUME [F2]", new Vector2(-36f, 0f), new Vector2(120f, 32f), () => CloseInternal(), new Color(0.35f, 0.38f, 0.45f, 1f));

            // ----------------------------------------------------------------
            // 2. SPLIT VIEW: LEFT SIDEBAR (Width 220px) & RIGHT CONTENT (Width 940px)
            // ----------------------------------------------------------------
            GameObject splitView = new GameObject("SplitView", typeof(RectTransform));
            splitView.transform.SetParent(chassis.transform, false);
            RectTransform svRt = splitView.GetComponent<RectTransform>();
            svRt.anchorMin = Vector2.zero;
            svRt.anchorMax = Vector2.one;
            svRt.offsetMin = new Vector2(0f, 0f);
            svRt.offsetMax = new Vector2(0f, -52f);

            // Sidebar
            GameObject sidebar = new GameObject("NavSidebar", typeof(RectTransform), typeof(Image));
            sidebar.transform.SetParent(splitView.transform, false);
            RectTransform sbRt = sidebar.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0f, 0f);
            sbRt.anchorMax = new Vector2(0f, 1f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.sizeDelta = new Vector2(220f, 0f);
            sbRt.anchoredPosition = Vector2.zero;
            sidebar.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f, 1f);
            navSidebarTransform = sidebar.transform;

            // Content Area
            GameObject contentArea = new GameObject("ContentArea", typeof(RectTransform), typeof(Image));
            contentArea.transform.SetParent(splitView.transform, false);
            RectTransform caRt = contentArea.GetComponent<RectTransform>();
            caRt.anchorMin = new Vector2(0f, 0f);
            caRt.anchorMax = new Vector2(1f, 1f);
            caRt.offsetMin = new Vector2(222f, 0f);
            caRt.offsetMax = Vector2.zero;
            contentArea.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 1f);
            contentAreaTransform = contentArea.transform;

            // Build Sidebar Tab Buttons
            BuildSidebarTabs();

            // Build Panes
            BuildPresetsPane();
            BuildVitalsPane();
            BuildSkillsPane();
            BuildTraitsPane();
            BuildLoadoutPane();
            BuildTimePane();
            BuildFlagsPane();
            BuildFastTravelPane();
            BuildCombatTesterPane();
        }

        private void BuildSidebarTabs()
        {
            var tabDefs = new[]
            {
                (DevTab.Presets,         "[+] PRESETS"),
                (DevTab.VitalsAndStats,   "[+] VITALS & STATS"),
                (DevTab.SkillsAndLevel,   "[+] SKILLS & LEVEL"),
                (DevTab.TraitsAndPerks,   "[+] TRAITS & PERKS"),
                (DevTab.LoadoutAndItems,  "[+] ITEMS & LOADOUT"),
                (DevTab.TimeAndClock,     "[+] TIME & CLOCK"),
                (DevTab.StoryFlags,       "[+] STORY FLAGS"),
                (DevTab.FastTravel,       "[+] FAST TRAVEL"),
                (DevTab.CombatTester,     "[+] COMBAT TESTER")
            };

            float yOffset = -12f;
            foreach (var (tab, label) in tabDefs)
            {
                var tabEnum = tab;
                GameObject btnObj = new GameObject($"TabBtn_{tab}", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(navSidebarTransform, false);
                RectTransform bRt = btnObj.GetComponent<RectTransform>();
                bRt.anchorMin = new Vector2(0f, 1f);
                bRt.anchorMax = new Vector2(1f, 1f);
                bRt.pivot = new Vector2(0.5f, 1f);
                bRt.sizeDelta = new Vector2(-16f, 44f);
                bRt.anchoredPosition = new Vector2(0f, yOffset);

                Image img = btnObj.GetComponent<Image>();
                img.color = ButtonNormalBg;

                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectTab(tabEnum));

                TextMeshProUGUI txt = CreateText(btnObj.transform, "Lbl", label, 12.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
                RectTransform tRt = txt.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = new Vector2(14f, 0f);
                tRt.offsetMax = new Vector2(-10f, 0f);
                txt.color = Color.white;

                tabButtons[tab] = btn;
                yOffset -= 50f;
            }
        }

        private void SelectTab(DevTab tab)
        {
            currentTab = tab;
            foreach (var kvp in tabButtons)
            {
                bool active = kvp.Key == tab;
                Image img = kvp.Value.GetComponent<Image>();
                if (img != null) img.color = active ? ButtonActiveBg : ButtonNormalBg;
                var txt = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = active ? AmberAccent : Color.white;
            }

            foreach (var kvp in tabPanels)
            {
                kvp.Value.SetActive(kvp.Key == tab);
            }

            RefreshActivePanel();
        }

        private GameObject CreateTabPanelRoot(DevTab tab, string title, string description)
        {
            GameObject panel = new GameObject($"Pane_{tab}", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            panel.transform.SetParent(contentAreaTransform, false);
            RectTransform pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.offsetMin = new Vector2(12f, 12f);
            pRt.offsetMax = new Vector2(-12f, -12f);

            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
            ScrollRect sr = panel.GetComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 28f;

            // Content container
            GameObject content = new GameObject("ScrollContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(panel.transform, false);
            RectTransform cRt = content.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(16, 16, 16, 16);

            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRt;
            sr.viewport = pRt;

            // Header Banner inside Content
            GameObject headerBox = new GameObject("HeaderBox", typeof(RectTransform), typeof(Image));
            headerBox.transform.SetParent(content.transform, false);
            headerBox.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = headerBox.AddComponent<LayoutElement>();
            le.minHeight = 52f;
            le.preferredHeight = 52f;

            TextMeshProUGUI hTxt = CreateText(headerBox.transform, "Title", $"<b>{title}</b>\n<color=#94A3B8><size=11>{description}</size></color>", 13.5f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform htRt = hTxt.GetComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero;
            htRt.anchorMax = Vector2.one;
            htRt.offsetMin = new Vector2(14f, 4f);
            htRt.offsetMax = new Vector2(-14f, -4f);

            tabPanels[tab] = panel;
            panel.SetActive(false);
            return content;
        }

        // ====================================================================
        // TAB 1: PRESETS
        // ====================================================================
        private void BuildPresetsPane()
        {
            var content = CreateTabPanelRoot(DevTab.Presets, "[PRESETS] QUICK CHARACTER BUILDS", "Select an archetype preset to immediately reconfigure vitals, core attributes, and skill points.");

            var presets = new[]
            {
                ("MAX ALL STATS", "Sets All Vitals to 100, Core Attributes to 10, All Skills to 5.", (Action)(() => ApplyPresetMaxAll())),
                ("COMBAT SPECIALIST", "STR 8, AGI 6, PER 5, CHA 3, INT 4. Melee 3, Shooting 2. High combat resilience.", (Action)(() => ApplyCombatPreset())),
                ("SILVER TONGUE DIPLOMAT", "STR 3, AGI 4, PER 6, CHA 8, INT 7. Scavenging 3, Tech 2, Medicine 1.", (Action)(() => ApplyDiplomatPreset())),
                ("AGILITY SCOUT", "STR 5, AGI 9, PER 7, CHA 5, INT 6. Stealth 3, Scavenging 4, Tech 2.", (Action)(() => ApplyAgilityPreset())),
                ("POLICE OFFICER", "STR 7, AGI 6, PER 6, CHA 6, INT 5. Shooting 2, Melee 1, Scavenging 1. Job = Police Officer.", (Action)(() => ApplyPolicePreset())),
                ("DOCTOR / SURGEON", "STR 4, AGI 5, PER 7, CHA 7, INT 9. Medicine 3, Stealth 1, Scavenging 1, Tech 2. Job = Doctor.", (Action)(() => ApplyDoctorPreset())),
                ("MIN ALL (CHALLENGE / WEAK)", "All Attributes 1, Skills 0, Vitals at critical 20 HP / 20 STM.", (Action)(() => ApplyPresetMinAll()))
            };

            foreach (var (title, desc, action) in presets)
            {
                CreatePresetCard(content.transform, title, desc, action);
            }
        }

        private void CreatePresetCard(Transform parent, string title, string desc, Action onApply)
        {
            GameObject card = new GameObject("PresetCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            card.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 1f);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.preferredHeight = 60f;

            TextMeshProUGUI txt = CreateText(card.transform, "Info", $"<b>{title}</b>\n<color=#94A3B8><size=11>{desc}</size></color>", 13f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.78f, 1f);
            tRt.offsetMin = new Vector2(14f, 4f);
            tRt.offsetMax = new Vector2(-10f, -4f);

            Button btn = CreateButton(card.transform, "ApplyBtn", "APPLY", new Vector2(-14f, 0f), new Vector2(96f, 36f), onApply, AmberAccent);
            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-60f, 0f);
        }

        // ====================================================================
        // TAB 2: VITALS & ATTRIBUTES
        // ====================================================================
        private readonly List<Action> vitalsRefreshCallbacks = new List<Action>();

        private void BuildVitalsPane()
        {
            var content = CreateTabPanelRoot(DevTab.VitalsAndStats, "[VITALS] SURVIVAL VITALS & ATTRIBUTES", "Modify current survival vitals and core attributes (1 - 10 scale).");

            CreateSectionHeader(content.transform, "SURVIVAL VITALS");

            // HP Row
            CreateAdjusterRow(content.transform, "BODY (HP)", () => currentPlayer != null ? $"{currentPlayer.hp} / {currentPlayer.hpMax}" : "100/100",
                () => { if (currentPlayer != null) { currentPlayer.hp = Mathf.Clamp(currentPlayer.hp - 10, 1, currentPlayer.hpMax); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.hp = Mathf.Clamp(currentPlayer.hp + 10, 1, currentPlayer.hpMax); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.hp = currentPlayer.hpMax; SyncVitals(); } }, "FILL 100%");

            // Stamina Row
            CreateAdjusterRow(content.transform, "STAMINA", () => currentPlayer != null ? $"{currentPlayer.stamina} / {currentPlayer.staminaMax}" : "100/100",
                () => { if (currentPlayer != null) { currentPlayer.stamina = Mathf.Clamp(currentPlayer.stamina - 10, 0, currentPlayer.staminaMax); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.stamina = Mathf.Clamp(currentPlayer.stamina + 10, 0, currentPlayer.staminaMax); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.stamina = currentPlayer.staminaMax; SyncVitals(); } }, "FILL 100%");

            // Hunger Row
            CreateAdjusterRow(content.transform, "HUNGER (ENERGY)", () => currentPlayer != null ? $"{currentPlayer.hunger} / 100" : "100/100",
                () => { if (currentPlayer != null) { currentPlayer.hunger = Mathf.Clamp(currentPlayer.hunger - 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.hunger = Mathf.Clamp(currentPlayer.hunger + 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.hunger = 100; SyncVitals(); } }, "FILL 100%");

            // Thirst Row
            CreateAdjusterRow(content.transform, "THIRST (HYDRATION)", () => currentPlayer != null ? $"{currentPlayer.thirst} / 100" : "100/100",
                () => { if (currentPlayer != null) { currentPlayer.thirst = Mathf.Clamp(currentPlayer.thirst - 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.thirst = Mathf.Clamp(currentPlayer.thirst + 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.thirst = 100; SyncVitals(); } }, "FILL 100%");

            // Humanity Row
            CreateAdjusterRow(content.transform, "HUMANITY (MORALE)", () => currentPlayer != null ? $"{currentPlayer.humanity} / 100" : "50/100",
                () => { if (currentPlayer != null) { currentPlayer.humanity = Mathf.Clamp(currentPlayer.humanity - 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.humanity = Mathf.Clamp(currentPlayer.humanity + 10, 0, 100); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.humanity = 100; SyncVitals(); } }, "SET 100%");

            CreateSectionHeader(content.transform, "M.O.R.T.A.L. CORE ATTRIBUTES (1 - 10)");

            CreateAdjusterRow(content.transform, "MIGHT (STR)", () => currentPlayer != null ? currentPlayer.str.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.str = Mathf.Clamp(currentPlayer.str - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.str = Mathf.Clamp(currentPlayer.str + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.str = 10; SyncVitals(); } }, "MAX 10");

            CreateAdjusterRow(content.transform, "OBSERVATION (PER)", () => currentPlayer != null ? currentPlayer.per.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.per = Mathf.Clamp(currentPlayer.per - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.per = Mathf.Clamp(currentPlayer.per + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.per = 10; SyncVitals(); } }, "MAX 10");

            CreateAdjusterRow(content.transform, "REFLEX (AGI)", () => currentPlayer != null ? currentPlayer.agi.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.agi = Mathf.Clamp(currentPlayer.agi - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.agi = Mathf.Clamp(currentPlayer.agi + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.agi = 10; SyncVitals(); } }, "MAX 10");

            CreateAdjusterRow(content.transform, "TACTICS (INT)", () => currentPlayer != null ? currentPlayer.intl.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.intl = Mathf.Clamp(currentPlayer.intl - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.intl = Mathf.Clamp(currentPlayer.intl + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.intl = 10; SyncVitals(); } }, "MAX 10");

            CreateAdjusterRow(content.transform, "LEADERSHIP (CHA)", () => currentPlayer != null ? currentPlayer.cha.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.cha = Mathf.Clamp(currentPlayer.cha - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.cha = Mathf.Clamp(currentPlayer.cha + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.cha = 10; SyncVitals(); } }, "MAX 10");

            CreateAdjusterRow(content.transform, "AUSPICE (LCK)", () => currentPlayer != null ? currentPlayer.lck.ToString() : "3",
                () => { if (currentPlayer != null) { currentPlayer.lck = Mathf.Clamp(currentPlayer.lck - 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.lck = Mathf.Clamp(currentPlayer.lck + 1, 1, 10); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.lck = 10; SyncVitals(); } }, "MAX 10");
        }

        private void CreateAdjusterRow(Transform parent, string label, Func<string> getValue, Action onMinus, Action onPlus, Action onQuickSet = null, string quickSetLabel = null)
        {
            GameObject row = new GameObject("AdjusterRow", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;

            TextMeshProUGUI lbl = CreateText(row.transform, "Lbl", label, 13f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0.42f, 1f);
            lRt.offsetMin = new Vector2(14f, 0f);
            lRt.offsetMax = Vector2.zero;

            TextMeshProUGUI valTxt = CreateText(row.transform, "Val", getValue(), 14f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform vRt = valTxt.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.44f, 0f);
            vRt.anchorMax = new Vector2(0.68f, 1f);
            vRt.offsetMin = Vector2.zero;
            vRt.offsetMax = Vector2.zero;
            valTxt.color = AmberAccent;

            vitalsRefreshCallbacks.Add(() =>
            {
                if (valTxt != null) valTxt.text = getValue();
            });

            CreateButton(row.transform, "MinusBtn", "-", new Vector2(0.68f, 0.5f), new Vector2(38f, 32f), () => { onMinus(); valTxt.text = getValue(); }, ButtonNormalBg);
            CreateButton(row.transform, "PlusBtn", "+", new Vector2(0.74f, 0.5f), new Vector2(38f, 32f), () => { onPlus(); valTxt.text = getValue(); }, ButtonNormalBg);

            if (onQuickSet != null && !string.IsNullOrEmpty(quickSetLabel))
            {
                CreateButton(row.transform, "QuickBtn", quickSetLabel, new Vector2(0.88f, 0.5f), new Vector2(88f, 32f), () => { onQuickSet(); valTxt.text = getValue(); }, AmberAccent);
            }
        }

        private void CreateSectionHeader(Transform parent, string title)
        {
            GameObject sec = new GameObject("SecHeader", typeof(RectTransform));
            sec.transform.SetParent(parent, false);
            var le = sec.AddComponent<LayoutElement>();
            le.minHeight = 30f;
            le.preferredHeight = 30f;

            TextMeshProUGUI txt = CreateText(sec.transform, "Title", $"<b><color=#FF6B35>> {title}</color></b>", 12.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(4f, 0f);
            tRt.offsetMax = Vector2.zero;
        }

        // ====================================================================
        // TAB 3: SKILLS & LEVEL
        // ====================================================================
        private void BuildSkillsPane()
        {
            var content = CreateTabPanelRoot(DevTab.SkillsAndLevel, "[SKILLS] SKILLS & LEVEL PROGRESSION", "Train or tweak survival skills (0 - 5 scale) and character experience.");

            CreateSectionHeader(content.transform, "SURVIVOR LEVEL & EXPERIENCE");

            CreateAdjusterRow(content.transform, "CHARACTER LEVEL", () => currentPlayer != null ? $"LVL {currentPlayer.level}" : "LVL 1",
                () => { if (currentPlayer != null) { currentPlayer.level = Mathf.Max(1, currentPlayer.level - 1); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.level++; SyncVitals(); } });

            CreateAdjusterRow(content.transform, "EXPERIENCE (XP)", () => currentPlayer != null ? $"{currentPlayer.xp} / {currentPlayer.xpRequired} XP" : "0/100 XP",
                () => { if (currentPlayer != null) { currentPlayer.xp = Mathf.Max(0, currentPlayer.xp - 50); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.GainXP(50); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.GainXP(currentPlayer.xpRequired - currentPlayer.xp); SyncVitals(); } }, "LEVEL UP");

            CreateSectionHeader(content.transform, "TACTICAL SKILLS (0 - 5)");

            CreateAdjusterRow(content.transform, "SHOOTING (FIREARMS)", () => currentPlayer != null ? currentPlayer.shooting.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.shooting = Mathf.Clamp(currentPlayer.shooting - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.shooting = Mathf.Clamp(currentPlayer.shooting + 1, 0, 5); SyncVitals(); } });

            CreateAdjusterRow(content.transform, "MELEE (CLOSE COMBAT)", () => currentPlayer != null ? currentPlayer.melee.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.melee = Mathf.Clamp(currentPlayer.melee - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.melee = Mathf.Clamp(currentPlayer.melee + 1, 0, 5); SyncVitals(); } });

            CreateAdjusterRow(content.transform, "STEALTH (INFILTRATION)", () => currentPlayer != null ? currentPlayer.stealth.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.stealth = Mathf.Clamp(currentPlayer.stealth - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.stealth = Mathf.Clamp(currentPlayer.stealth + 1, 0, 5); SyncVitals(); } });

            CreateAdjusterRow(content.transform, "MEDICINE (FIRST AID)", () => currentPlayer != null ? currentPlayer.medicine.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.medicine = Mathf.Clamp(currentPlayer.medicine - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.medicine = Mathf.Clamp(currentPlayer.medicine + 1, 0, 5); SyncVitals(); } });

            CreateAdjusterRow(content.transform, "SCAVENGING (LOOTING)", () => currentPlayer != null ? currentPlayer.scavenging.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.scavenging = Mathf.Clamp(currentPlayer.scavenging - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.scavenging = Mathf.Clamp(currentPlayer.scavenging + 1, 0, 5); SyncVitals(); } });

            CreateAdjusterRow(content.transform, "TECH (ELECTRONICS / LOCKS)", () => currentPlayer != null ? currentPlayer.tech.ToString() : "0",
                () => { if (currentPlayer != null) { currentPlayer.tech = Mathf.Clamp(currentPlayer.tech - 1, 0, 5); SyncVitals(); } },
                () => { if (currentPlayer != null) { currentPlayer.tech = Mathf.Clamp(currentPlayer.tech + 1, 0, 5); SyncVitals(); } });

            // Action row: All skills to 5 or reset
            GameObject btnRow = new GameObject("SkillQuickRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnRow.transform.SetParent(content.transform, false);
            var le = btnRow.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;
            HorizontalLayoutGroup hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateLayoutButton(btnRow.transform, "ALL SKILLS = 5", () =>
            {
                if (currentPlayer == null) return;
                currentPlayer.shooting = 5; currentPlayer.melee = 5; currentPlayer.stealth = 5;
                currentPlayer.medicine = 5; currentPlayer.scavenging = 5; currentPlayer.tech = 5;
                SyncVitals();
            }, GreenSuccess);

            CreateLayoutButton(btnRow.transform, "RESET ALL SKILLS (0)", () =>
            {
                if (currentPlayer == null) return;
                currentPlayer.shooting = 0; currentPlayer.melee = 0; currentPlayer.stealth = 0;
                currentPlayer.medicine = 0; currentPlayer.scavenging = 0; currentPlayer.tech = 0;
                SyncVitals();
            }, RedDanger);
        }

        // ====================================================================
        // TAB 4: TRAITS & PERKS
        // ====================================================================
        private readonly Dictionary<string, Image> traitButtonImages = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private void BuildTraitsPane()
        {
            var content = CreateTabPanelRoot(DevTab.TraitsAndPerks, "[PERKS] TRAITS, PERKS & DRAWBACKS", "Toggle character perks, tactical talents, and survival drawbacks directly.");

            var allTraits = new[]
            {
                ("STEADY HANDS", "+15 Accuracy bonus with ranged weapons."),
                ("AUTHORITY", "Command NPCs in crises and resolve disputes."),
                ("BREACHER", "Smash locked doors with sheer brute physical force."),
                ("IRON GRIP", "Knockback attacking enemies even on near-misses."),
                ("LOCKSMITH", "Pick mechanical tumblers and locks without noise."),
                ("GHOST", "Silent footsteps. Hostiles struggle to detect you."),
                ("FIELD MEDIC", "Healing bandages and medicine restore 2x HP."),
                ("ANATOMIST", "Pinpoint fatal weak spots. Heavy critical strike bonus."),
                ("SILVER TONGUE", "Exceptional charisma. Better bartering prices."),
                ("FAST LEARNER", "Gain double experience from all activities."),
                ("ADRENALINE", "Surge in stats when health drops below 30%."),
                ("RUNNER", "High stamina sprint. Can flee combat twice per fight."),
                ("MECHANIC", "Hotwire vehicles and repair machinery effortlessly."),
                ("HUNTER", "Spot predatory ambushes and hidden tracks early."),
                ("BOXER", "Heavy bare-knuckle unarmed strikes deal massive punch damage."),
                ("ARCHER", "Silent lethal compound bow hunter. Arrows are retrievable."),
                ("SMOKER", "Drawback: -20 Stamina. Requires nicotine to maintain focus."),
                ("ALCOHOLIC", "Drawback: +20 HP, -1 Intelligence. Reduced agony and pain."),
                ("ASTHMATIC", "Drawback: Heavy breathing. Cannot sprint without inhaler."),
                ("BLIND", "Drawback: Severely impaired aim without corrective glasses."),
                ("BAD KNEE", "Drawback: Damaged joint. Incapable of fleeing encounters."),
                ("FRAGILE", "Drawback: Bleeding and lacerations cause 2x damage over time.")
            };

            CreateLayoutButton(content.transform, "X CLEAR ALL EQUIPPED TRAITS", () =>
            {
                if (currentPlayer == null) return;
                currentPlayer.traits.Clear();
                RefreshTraitsUI();
                SyncVitals();
            }, RedDanger);

            foreach (var (name, desc) in allTraits)
            {
                CreateTraitToggleCard(content.transform, name, desc);
            }
        }

        private void CreateTraitToggleCard(Transform parent, string traitName, string desc)
        {
            GameObject card = new GameObject($"Trait_{traitName}", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            Image cardBg = card.GetComponent<Image>();
            cardBg.color = new Color(0.09f, 0.11f, 0.15f, 1f);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 50f;
            le.preferredHeight = 50f;

            TextMeshProUGUI txt = CreateText(card.transform, "Info", $"<b>{traitName}</b>\n<color=#94A3B8><size=11>{desc}</size></color>", 12.5f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.80f, 1f);
            tRt.offsetMin = new Vector2(14f, 2f);
            tRt.offsetMax = new Vector2(-10f, -2f);

            Button btn = CreateButton(card.transform, "ToggleBtn", "OFF", new Vector2(1f, 0.5f), new Vector2(80f, 32f), () =>
            {
                if (currentPlayer == null) return;
                bool has = currentPlayer.HasTrait(traitName);
                if (has)
                {
                    currentPlayer.traits.RemoveAll(t => string.Equals(t.name, traitName, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    currentPlayer.AddTrait(traitName, desc);
                }
                RefreshTraitsUI();
                SyncVitals();
            }, ButtonNormalBg);

            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-54f, 0f);

            traitButtonImages[traitName] = btn.GetComponent<Image>();
        }

        private void RefreshTraitsUI()
        {
            if (currentPlayer == null) return;
            foreach (var kvp in traitButtonImages)
            {
                bool has = currentPlayer.HasTrait(kvp.Key);
                if (kvp.Value != null)
                {
                    kvp.Value.color = has ? GreenSuccess : ButtonNormalBg;
                    var txt = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        txt.text = has ? "ACTIVE" : "OFF";
                        txt.color = has ? Color.black : Color.white;
                    }
                }
            }
        }

        // ====================================================================
        // TAB 5: ITEMS & LOADOUT
        // ====================================================================
        private void BuildLoadoutPane()
        {
            var content = CreateTabPanelRoot(DevTab.LoadoutAndItems, "[ITEMS] INVENTORY ITEM SPAWNER", "Spawn weapons, ammunition, medicine, and key survival tools directly into your backpack.");

            // Backpack Status
            GameObject statBox = new GameObject("BagStatus", typeof(RectTransform), typeof(Image));
            statBox.transform.SetParent(content.transform, false);
            statBox.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = statBox.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;

            TextMeshProUGUI bTxt = CreateText(statBox.transform, "Txt", "BACKPACK SLOTS: 0 / 12 ITEMS", 13f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform btRt = bTxt.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = new Vector2(14f, 0f);
            btRt.offsetMax = Vector2.zero;

            vitalsRefreshCallbacks.Add(() =>
            {
                if (bTxt != null && currentPlayer != null)
                {
                    int total = currentPlayer.inventory != null ? currentPlayer.inventory.Count : 0;
                    bTxt.text = $"BACKPACK SLOTS: <color=#FF6B35><b>{total} / {currentPlayer.bagLimit} ITEMS</b></color>";
                }
            });

            // Action Row: Quick Packs
            GameObject packRow = new GameObject("PackRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            packRow.transform.SetParent(content.transform, false);
            var leRow = packRow.AddComponent<LayoutElement>();
            leRow.minHeight = 44f;
            leRow.preferredHeight = 44f;
            HorizontalLayoutGroup hlg = packRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateLayoutButton(packRow.transform, "GIVE FULL SURVIVOR KIT", () => GiveFullKit(), AmberAccent);
            CreateLayoutButton(packRow.transform, "GIVE ALL WEAPONS & AMMO", () => GiveAllWeapons(), GreenSuccess);
            CreateLayoutButton(packRow.transform, "CLEAR BACKPACK", () => ClearBackpack(), RedDanger);

            CreateSectionHeader(content.transform, "WEAPONS & FIREARMS");
            CreateItemSpawnRow(content.transform, "Glock 19", 1, "9mm service pistol.");
            CreateItemSpawnRow(content.transform, "Remington 870", 1, "Pump-action shotgun.");
            CreateItemSpawnRow(content.transform, "Compound Bow", 1, "Silent hunting bow.");
            CreateItemSpawnRow(content.transform, "Crowbar", 1, "Forged prying tool.");
            CreateItemSpawnRow(content.transform, "Combat Knife", 1, "Tactical military blade.");

            CreateSectionHeader(content.transform, "AMMUNITION");
            CreateItemSpawnRow(content.transform, "9mm Ammo", 30, "Handgun rounds.");
            CreateItemSpawnRow(content.transform, "Shotgun Shells", 12, "12-gauge 00 buckshot.");
            CreateItemSpawnRow(content.transform, "Arrows", 10, "Carbon composite broadheads.");

            CreateSectionHeader(content.transform, "MEDICAL & RECOVERY");
            CreateItemSpawnRow(content.transform, "First Aid Kit", 1, "Full surgical trauma kit.");
            CreateItemSpawnRow(content.transform, "Sterile Bandage", 3, "Stops hemorrhaging.");
            CreateItemSpawnRow(content.transform, "Painkillers", 2, "Relieves acute pain.");
            CreateItemSpawnRow(content.transform, "Antibiotics", 2, "Fights infection.");

            CreateSectionHeader(content.transform, "SURVIVAL SUPPLIES & TOOLS");
            CreateItemSpawnRow(content.transform, "Canned Beef", 3, "High-calorie preserved ration.");
            CreateItemSpawnRow(content.transform, "Clean Water Bottle", 3, "Purified drinking water.");
            CreateItemSpawnRow(content.transform, "Flashlight", 1, "Durable tactical flashlight.");
            CreateItemSpawnRow(content.transform, "Lockpick", 5, "Spring steel picking tool.");
            CreateItemSpawnRow(content.transform, "Batteries", 4, "Alkaline batteries.");
        }

        private void CreateItemSpawnRow(Transform parent, string itemId, int defaultQty, string description)
        {
            GameObject row = new GameObject($"Spawn_{itemId}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;

            TextMeshProUGUI txt = CreateText(row.transform, "Lbl", $"<b>{itemId}</b> <color=#94A3B8>(x{defaultQty})</color>  <size=11><color=#64748B>{description}</size></color>", 12.5f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.75f, 1f);
            tRt.offsetMin = new Vector2(14f, 0f);
            tRt.offsetMax = Vector2.zero;

            Button btn = CreateButton(row.transform, "AddBtn", "+ SPAWN", new Vector2(1f, 0.5f), new Vector2(96f, 32f), () =>
            {
                if (currentPlayer == null) return;
                currentPlayer.AddItem(itemId, defaultQty);
                SyncVitals();
            }, ButtonNormalBg);

            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-60f, 0f);
        }

        private void GiveFullKit()
        {
            if (currentPlayer == null) return;
            currentPlayer.AddItem("Glock 19", 1);
            currentPlayer.AddItem("9mm Ammo", 30);
            currentPlayer.AddItem("Combat Knife", 1);
            currentPlayer.AddItem("First Aid Kit", 1);
            currentPlayer.AddItem("Sterile Bandage", 2);
            currentPlayer.AddItem("Canned Beef", 2);
            currentPlayer.AddItem("Clean Water Bottle", 2);
            currentPlayer.AddItem("Flashlight", 1);
            currentPlayer.AddItem("Lockpick", 3);
            SyncVitals();
        }

        private void GiveAllWeapons()
        {
            if (currentPlayer == null) return;
            currentPlayer.AddItem("Glock 19", 1);
            currentPlayer.AddItem("Remington 870", 1);
            currentPlayer.AddItem("Compound Bow", 1);
            currentPlayer.AddItem("Crowbar", 1);
            currentPlayer.AddItem("Combat Knife", 1);
            currentPlayer.AddItem("9mm Ammo", 60);
            currentPlayer.AddItem("Shotgun Shells", 20);
            currentPlayer.AddItem("Arrows", 20);
            SyncVitals();
        }

        private void ClearBackpack()
        {
            if (currentPlayer == null) return;
            currentPlayer.inventory.Clear();
            SyncVitals();
        }

        // ====================================================================
        // TAB 6: TIME & CLOCK
        // ====================================================================
        private void BuildTimePane()
        {
            var content = CreateTabPanelRoot(DevTab.TimeAndClock, "[TIME] IN-GAME CLOCK & CALENDAR", "Advance hours, set day/night cycles, or fast forward the survival timeline.");

            GameObject clockBox = new GameObject("ClockBox", typeof(RectTransform), typeof(Image));
            clockBox.transform.SetParent(content.transform, false);
            clockBox.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = clockBox.AddComponent<LayoutElement>();
            le.minHeight = 56f;
            le.preferredHeight = 56f;

            TextMeshProUGUI cTxt = CreateText(clockBox.transform, "TimeDisplay", "CURRENT TIME: 23:45 (DAY 0)", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform ctRt = cTxt.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.offsetMin = Vector2.zero;
            ctRt.offsetMax = Vector2.zero;
            cTxt.color = AmberAccent;

            vitalsRefreshCallbacks.Add(() =>
            {
                if (cTxt != null && ApartmentHubScreen.Instance != null && ApartmentHubScreen.Instance.Clock != null)
                {
                    var clk = ApartmentHubScreen.Instance.Clock;
                    cTxt.text = $"CURRENT TIME: <b>{clk.Hour:D2}:{clk.Minute:D2}</b> // {clk.DayName} (DAY {clk.DayCount})";
                }
            });

            CreateSectionHeader(content.transform, "TIME ADVANCE / REVERSE");

            CreateAdjusterRow(content.transform, "HOUR (+ / -)", () => ApartmentHubScreen.Instance?.Clock != null ? $"{ApartmentHubScreen.Instance.Clock.Hour:D2}:00" : "23:00",
                () => { SetTimeRelative(-60); },
                () => { SetTimeRelative(60); },
                () => { SetTimeRelative(180); }, "+3 HOURS");

            CreateAdjusterRow(content.transform, "MINUTE (+ / -)", () => ApartmentHubScreen.Instance?.Clock != null ? $"{ApartmentHubScreen.Instance.Clock.Minute:D2} MIN" : "45 MIN",
                () => { SetTimeRelative(-10); },
                () => { SetTimeRelative(10); },
                () => { SetTimeRelative(30); }, "+30 MIN");

            CreateSectionHeader(content.transform, "QUICK TIME PRESETS");

            var timePresets = new[]
            {
                ("SET NIGHT // 23:00 (START HOUR)", (Action)(() => SetClockExact(23, 0))),
                ("SET MIDNIGHT // 00:00", (Action)(() => SetClockExact(0, 0))),
                ("SET DAWN // 06:00", (Action)(() => SetClockExact(6, 0))),
                ("SET NOON // 12:00", (Action)(() => SetClockExact(12, 0))),
                ("ADVANCE TO NEXT DAY (+24 HRS)", (Action)(() => SetTimeRelative(1440)))
            };

            foreach (var (label, action) in timePresets)
            {
                CreateLayoutButton(content.transform, label, action, ButtonNormalBg);
            }
        }

        private void SetTimeRelative(int minutes)
        {
            if (ApartmentHubScreen.Instance != null && ApartmentHubScreen.Instance.Clock != null)
            {
                var clk = ApartmentHubScreen.Instance.Clock;
                int totalMinutes = (clk.DayCount * 24 * 60) + (clk.Hour * 60) + clk.Minute + minutes;
                if (totalMinutes < 0) totalMinutes = 0;
                int newDay = totalMinutes / (24 * 60);
                int rem = totalMinutes % (24 * 60);
                int newHour = rem / 60;
                int newMin = rem % 60;
                clk.SetTime(newDay, newHour, newMin, clk.Date, clk.Month, clk.Year, clk.DayName);
                SyncVitals();
            }
        }

        private void SetClockExact(int hour, int minute)
        {
            if (ApartmentHubScreen.Instance != null && ApartmentHubScreen.Instance.Clock != null)
            {
                var clk = ApartmentHubScreen.Instance.Clock;
                clk.SetTime(clk.DayCount, hour, minute, clk.Date, clk.Month, clk.Year, clk.DayName);
                SyncVitals();
            }
        }

        // ====================================================================
        // TAB 7: STORY FLAGS
        // ====================================================================
        private readonly Dictionary<string, Image> flagButtonImages = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private void BuildFlagsPane()
        {
            var content = CreateTabPanelRoot(DevTab.StoryFlags, "[FLAGS] STORY & QUEST SWITCHES", "Inspect or toggle key exploration switches and locked room statuses.");

            GameObject flagRow = new GameObject("FlagRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            flagRow.transform.SetParent(content.transform, false);
            var leRow = flagRow.AddComponent<LayoutElement>();
            leRow.minHeight = 44f;
            leRow.preferredHeight = 44f;
            HorizontalLayoutGroup hlg = flagRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateLayoutButton(flagRow.transform, "UNLOCK ALL APARTMENT ROOMS", () => UnlockAllRooms(), GreenSuccess);
            CreateLayoutButton(flagRow.transform, "RESET ALL STORY FLAGS", () => ResetAllFlags(), RedDanger);

            var keyFlags = new[]
            {
                ("room405_opened", "Unit 405 (Marcus's Apartment) door unlocked/breached."),
                ("looted_405_shotgun", "Marcus's Remington shotgun looted from closet."),
                ("read_marcus_phone", "Marcus's personal smartphone inspected."),
                ("room406_opened", "Unit 406 door unlocked/inspected."),
                ("looted_406", "Unit 406 supplies and medical goods cleared."),
                ("utility_opened", "Floor 4 Utility maintenance closet unlocked."),
                ("looted_utility_flashlight", "Heavy industrial flashlight looted from utility."),
                ("looted_utility_batteries", "Utility spare batteries collected."),
                ("f4_hallway_cleared", "Floor 4 infected encounter resolved / avoided."),
                ("stairwell_unlocked", "Stairwell fire exit door unlocked for lower floor access.")
            };

            foreach (var (flag, desc) in keyFlags)
            {
                CreateFlagToggleCard(content.transform, flag, desc);
            }
        }

        private void CreateFlagToggleCard(Transform parent, string flagName, string desc)
        {
            GameObject card = new GameObject($"Flag_{flagName}", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            card.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.15f, 1f);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 50f;
            le.preferredHeight = 50f;

            TextMeshProUGUI txt = CreateText(card.transform, "Info", $"<b>{flagName}</b>\n<color=#94A3B8><size=11>{desc}</size></color>", 12.5f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.80f, 1f);
            tRt.offsetMin = new Vector2(14f, 2f);
            tRt.offsetMax = new Vector2(-10f, -2f);

            Button btn = CreateButton(card.transform, "ToggleBtn", "OFF", new Vector2(1f, 0.5f), new Vector2(80f, 32f), () =>
            {
                if (currentPlayer == null) return;
                bool has = currentPlayer.HasFlag(flagName);
                currentPlayer.SetFlag(flagName, !has);
                RefreshFlagsUI();
            }, ButtonNormalBg);

            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-54f, 0f);

            flagButtonImages[flagName] = btn.GetComponent<Image>();
        }

        private void RefreshFlagsUI()
        {
            if (currentPlayer == null) return;
            foreach (var kvp in flagButtonImages)
            {
                bool has = currentPlayer.HasFlag(kvp.Key);
                if (kvp.Value != null)
                {
                    kvp.Value.color = has ? GreenSuccess : ButtonNormalBg;
                    var txt = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        txt.text = has ? "TRUE" : "FALSE";
                        txt.color = has ? Color.black : Color.white;
                    }
                }
            }
        }

        private void UnlockAllRooms()
        {
            if (currentPlayer == null) return;
            currentPlayer.SetFlag("room405_opened", true);
            currentPlayer.SetFlag("room406_opened", true);
            currentPlayer.SetFlag("utility_opened", true);
            currentPlayer.SetFlag("stairwell_unlocked", true);
            RefreshFlagsUI();
        }

        private void ResetAllFlags()
        {
            if (currentPlayer == null) return;
            currentPlayer.storyFlags.Clear();
            RefreshFlagsUI();
        }

        // ====================================================================
        // TAB 8: FAST TRAVEL / WARP
        // ====================================================================
        private void BuildFastTravelPane()
        {
            var content = CreateTabPanelRoot(DevTab.FastTravel, "[WARP] FAST TRAVEL & PASSAGE TELEPORT", "Instantly jump to any room, hallway corridor, or external survival passage.");

            CreateSectionHeader(content.transform, "UNIT 407 (SAFEHOUSE)");
            CreateWarpRow(content.transform, "Unit 407 Living Room (Safehouse Hub)", () => WarpToHubMode());
            CreateWarpRow(content.transform, "Passage: Unit 407 Hub", () => WarpToPassage("Act1/Passage_Unit407_Hub"));

            CreateSectionHeader(content.transform, "FLOOR 4 RESIDENCY & UNITS");
            CreateWarpRow(content.transform, "Passage: Hallway Section 1", () => WarpToPassage("Act1/Passage_Hallway_1"));
            CreateWarpRow(content.transform, "Passage: Hallway Section 2", () => WarpToPassage("Act1/Passage_Hallway_2"));
            CreateWarpRow(content.transform, "Passage: Hallway Section 3", () => WarpToPassage("Act1/Passage_Hallway_3"));
            CreateWarpRow(content.transform, "Passage: Hallway Section 4", () => WarpToPassage("Act1/Passage_Hallway_4"));
            CreateWarpRow(content.transform, "Passage: Unit 401 Inside", () => WarpToPassage("Act1/Passage_Unit401_Inside"));
            CreateWarpRow(content.transform, "Passage: Unit 402 Inside", () => WarpToPassage("Act1/Passage_Unit402_Inside"));
            CreateWarpRow(content.transform, "Passage: Unit 405 Inside (Marcus)", () => WarpToPassage("Act1/Passage_Unit405_Inside"));
            CreateWarpRow(content.transform, "Passage: Unit 408 Inside", () => WarpToPassage("Act1/Passage_Unit408_Inside"));
            CreateWarpRow(content.transform, "Passage: Utility Maintenance Inside", () => WarpToPassage("Act1/Passage_Utility_Inside"));

            CreateSectionHeader(content.transform, "LOWER FLOORS & EXTERIOR");
            CreateWarpRow(content.transform, "Passage: Stairwell 4F", () => WarpToPassage("Act1/Passage_Stairwell_4F"));
            CreateWarpRow(content.transform, "Passage: Lobby Entrance", () => WarpToPassage("Passage_Lobby_Entrance"));
            CreateWarpRow(content.transform, "Passage: Act 2 The Street", () => WarpToPassage("Passage_Act2_TheStreet"));
            CreateWarpRow(content.transform, "Passage: Act 2 Gas Station", () => WarpToPassage("Passage_Act2_GasStation"));
            CreateWarpRow(content.transform, "Passage: Act 2 Highway", () => WarpToPassage("Passage_Act2_Highway"));
            CreateWarpRow(content.transform, "Passage: Act 2 Marcus Cabin", () => WarpToPassage("Passage_Act2_5_MarcusCabin"));
            CreateWarpRow(content.transform, "Passage: Act 3 School Ground Floor", () => WarpToPassage("Passage_Act3_SchoolGFloor"));
            CreateWarpRow(content.transform, "Passage: Act 3 School 1st Floor", () => WarpToPassage("Passage_Act3_School1Floor"));
            CreateWarpRow(content.transform, "Passage: Act 3 7-Eleven Arrival", () => WarpToPassage("Passage_Act3_7Eleven_Arrival"));
        }

        private void CreateWarpRow(Transform parent, string destinationName, Action onWarp)
        {
            GameObject row = new GameObject($"Warp_{destinationName}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 1f);
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;

            TextMeshProUGUI txt = CreateText(row.transform, "Lbl", $"<b>{destinationName}</b>", 13f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.80f, 1f);
            tRt.offsetMin = new Vector2(14f, 0f);
            tRt.offsetMax = Vector2.zero;

            Button btn = CreateButton(row.transform, "WarpBtn", "WARP", new Vector2(1f, 0.5f), new Vector2(80f, 32f), () =>
            {
                CloseInternal();
                EnsureGameRunning();
                onWarp();
            }, AmberAccent);

            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-54f, 0f);
        }

        private void EnsureGameRunning()
        {
            var title = FindAnyObjectByType<TitleScreen>(FindObjectsInactive.Include);
            if (title != null && title.gameObject.activeSelf)
            {
                title.gameObject.SetActive(false);
            }
            if (ApartmentHubScreen.Instance != null && !ApartmentHubScreen.Instance.gameObject.activeSelf)
            {
                ApartmentHubScreen.Instance.gameObject.SetActive(true);
            }
        }

        private void WarpToHubMode()
        {
            if (ApartmentHubScreen.Instance != null)
            {
                ApartmentHubScreen.Instance.SwitchConsoleMode(ApartmentHubScreen.ConsoleMode.Narrative);
                ApartmentHubScreen.Instance.UpdateVitalsUI();
            }
        }

        private void WarpToPassage(string passagePath)
        {
            var passage = Resources.Load<StoryPassageData>($"StoryPassages/{passagePath}");
            if (passage == null)
            {
                string baseName = System.IO.Path.GetFileName(passagePath);
                passage = Resources.Load<StoryPassageData>($"StoryPassages/{baseName}");
            }

            if (passage != null && ApartmentHubScreen.Instance != null)
            {
                ApartmentHubScreen.Instance.PlayStoryPassage(passage);
            }
            else
            {
                Debug.LogWarning($"[DeveloperWindow] Could not find StoryPassageData at: StoryPassages/{passagePath}");
            }
        }

        // ====================================================================
        // TAB 9: COMBAT TESTER
        // ====================================================================
        private void BuildCombatTesterPane()
        {
            var content = CreateTabPanelRoot(DevTab.CombatTester, "[COMBAT] 1-CLICK TACTICAL COMBAT TESTER", "Trigger instant tactical limb encounters against zombies, security guards, or custom hordes.");

            CreateCombatCard(content.transform, "INFECTED COMMUTER (SOLO)", "Test limb dismemberment against a single erratic commuter zombie.", () =>
            {
                CloseInternal();
                EnsureGameRunning();
                TacticalCombatModal.TriggerCombat("zombie_commuter");
            });

            CreateCombatCard(content.transform, "INFECTED SECURITY GUARD (SOLO)", "Armored torso with baton swings and heavy grasp.", () =>
            {
                CloseInternal();
                EnsureGameRunning();
                TacticalCombatModal.TriggerCombat("zombie_guard");
            });

            CreateCombatCard(content.transform, "2x ZOMBIE ENCOUNTER (Commuter + Guard)", "Standard multi-target encounter with target switching ([Q] / [E]).", () =>
            {
                CloseInternal();
                EnsureGameRunning();
                TacticalCombatModal.TriggerCombat("zombie_commuter,zombie_guard");
            });

            CreateCombatCard(content.transform, "3x ZOMBIE HORDE (Commuter + Guard + Motorist)", "High-intensity survivor test against a 3-enemy pack.", () =>
            {
                CloseInternal();
                EnsureGameRunning();
                TacticalCombatModal.TriggerCombat("zombie_commuter,zombie_guard,zombie_motorist");
            });
        }

        private void CreateCombatCard(Transform parent, string title, string desc, Action onFight)
        {
            GameObject card = new GameObject("CombatCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            card.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.09f, 1f);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.preferredHeight = 60f;

            TextMeshProUGUI txt = CreateText(card.transform, "Info", $"<b><color=#F87171>{title}</color></b>\n<color=#94A3B8><size=11>{desc}</size></color>", 13f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.78f, 1f);
            tRt.offsetMin = new Vector2(14f, 4f);
            tRt.offsetMax = new Vector2(-10f, -4f);

            Button btn = CreateButton(card.transform, "FightBtn", "FIGHT!", new Vector2(1f, 0.5f), new Vector2(96f, 36f), onFight, RedDanger);
            RectTransform bRt = btn.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 0.5f);
            bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-60f, 0f);
        }

        // ====================================================================
        // PRESET ACTIONS
        // ====================================================================
        private void ApplyPresetMaxAll()
        {
            if (currentPlayer == null) return;
            currentPlayer.hp = currentPlayer.hpMax = 100;
            currentPlayer.stamina = currentPlayer.staminaMax = 100;
            currentPlayer.hunger = 100;
            currentPlayer.thirst = 100;
            currentPlayer.humanity = 100;

            currentPlayer.str = 10; currentPlayer.agi = 10; currentPlayer.per = 10;
            currentPlayer.cha = 10; currentPlayer.intl = 10; currentPlayer.lck = 10;

            currentPlayer.shooting = 5; currentPlayer.melee = 5; currentPlayer.stealth = 5;
            currentPlayer.medicine = 5; currentPlayer.scavenging = 5; currentPlayer.tech = 5;
            SyncVitals();
        }

        private void ApplyPresetMinAll()
        {
            if (currentPlayer == null) return;
            currentPlayer.hp = 20; currentPlayer.stamina = 20;
            currentPlayer.hunger = 20; currentPlayer.thirst = 20;
            currentPlayer.humanity = 10;

            currentPlayer.str = 1; currentPlayer.agi = 1; currentPlayer.per = 1;
            currentPlayer.cha = 1; currentPlayer.intl = 1; currentPlayer.lck = 1;

            currentPlayer.shooting = 0; currentPlayer.melee = 0; currentPlayer.stealth = 0;
            currentPlayer.medicine = 0; currentPlayer.scavenging = 0; currentPlayer.tech = 0;
            SyncVitals();
        }

        private void HealPlayerFull()
        {
            if (currentPlayer == null) return;
            currentPlayer.hp = currentPlayer.hpMax;
            currentPlayer.stamina = currentPlayer.staminaMax;
            currentPlayer.hunger = 100;
            currentPlayer.thirst = 100;
            SyncVitals();
        }

        private void ApplyCombatPreset()
        {
            if (currentPlayer == null) return;
            HealPlayerFull();
            currentPlayer.str = 8; currentPlayer.agi = 6; currentPlayer.per = 5;
            currentPlayer.cha = 3; currentPlayer.intl = 4;
            currentPlayer.melee = 3; currentPlayer.shooting = 2; currentPlayer.scavenging = 2;
            SyncVitals();
        }

        private void ApplyDiplomatPreset()
        {
            if (currentPlayer == null) return;
            HealPlayerFull();
            currentPlayer.str = 3; currentPlayer.agi = 4; currentPlayer.per = 6;
            currentPlayer.cha = 8; currentPlayer.intl = 7;
            currentPlayer.scavenging = 3; currentPlayer.tech = 2; currentPlayer.medicine = 1;
            SyncVitals();
        }

        private void ApplyAgilityPreset()
        {
            if (currentPlayer == null) return;
            HealPlayerFull();
            currentPlayer.str = 5; currentPlayer.agi = 9; currentPlayer.per = 7;
            currentPlayer.cha = 5; currentPlayer.intl = 6;
            currentPlayer.stealth = 3; currentPlayer.scavenging = 4; currentPlayer.tech = 2;
            SyncVitals();
        }

        private void ApplyPolicePreset()
        {
            if (currentPlayer == null) return;
            HealPlayerFull();
            currentPlayer.job = "Police Officer";
            currentPlayer.str = 7; currentPlayer.agi = 6; currentPlayer.per = 6;
            currentPlayer.cha = 6; currentPlayer.intl = 5;
            currentPlayer.shooting = 2; currentPlayer.melee = 1; currentPlayer.scavenging = 1; currentPlayer.tech = 1;
            SyncVitals();
        }

        private void ApplyDoctorPreset()
        {
            if (currentPlayer == null) return;
            HealPlayerFull();
            currentPlayer.job = "Doctor";
            currentPlayer.str = 4; currentPlayer.agi = 5; currentPlayer.per = 7;
            currentPlayer.cha = 7; currentPlayer.intl = 9;
            currentPlayer.medicine = 3; currentPlayer.stealth = 1; currentPlayer.scavenging = 1; currentPlayer.tech = 2;
            SyncVitals();
        }

        private void SyncVitals()
        {
            ApartmentHubScreen.Instance?.UpdateVitalsUI();
            ApartmentHubScreen.Instance?.RefreshPDAStatusTab();
            RefreshActivePanel();
        }

        private void RefreshActivePanel()
        {
            foreach (var cb in vitalsRefreshCallbacks) cb?.Invoke();
            RefreshTraitsUI();
            RefreshFlagsUI();
        }

        // ====================================================================
        // UI FACTORY HELPERS
        // ====================================================================
        private Transform CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return obj.transform;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick, Color bgColor)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            Image img = btnObj.GetComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            TextMeshProUGUI txt = CreateText(btnObj.transform, "Lbl", label, 12.5f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateLayoutButton(Transform parent, string label, Action onClick, Color bgColor)
        {
            GameObject btnObj = new GameObject("LayoutBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnObj.transform.SetParent(parent, false);

            var le = btnObj.GetComponent<LayoutElement>();
            le.minHeight = 40f;
            le.preferredHeight = 40f;

            Image img = btnObj.GetComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            TextMeshProUGUI txt = CreateText(btnObj.transform, "Lbl", label, 13f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            return btn;
        }
    }
}
