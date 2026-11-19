using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using SundayLastBreath.Core;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controller for the 1 + 3 Hybrid widescreen cockpit layout (1920x1080) in MainGame_HybridUI.unity.
    /// Arranges the existing MainGameScreen visual stage, situation narration, and tactical choices
    /// alongside live-synced Inventory, Survivor Status, Floor Blueprint Mini-Map, Story Reader tabs,
    /// and a 5-button hardware deck without modifying the original MainGame.unity layout.
    /// </summary>
    public class HybridCockpitController : MonoBehaviour
    {
        public enum ReaderTab
        {
            Situation = 0,
            StoryLog = 1,
            Notes = 2,
            Floor = 3
        }

        public enum SurvivorSubTab
        {
            Mortal = 0,
            Skills = 1,
            Traits = 2,
            TeamGear = 3
        }

        [Serializable]
        private class SlotUI
        {
            public string categoryTag;
            public GameObject root;
            public Image bgImage;
            public Outline outline;
            public Image iconImage;
            public TextMeshProUGUI eqpBadge;
            public TextMeshProUGUI countLabel;
            public Button button;
            [NonSerialized] public string boundItemId;
            [NonSerialized] public bool isEquipped;
        }

        [Serializable]
        private class VitalBarUI
        {
            public string label;
            public Image iconImage;
            public Image fillImage;
            public TextMeshProUGUI valueText;
        }

        [Serializable]
        private class StatBarRowUI
        {
            public string key;
            public Image fillImage;
            public TextMeshProUGUI valueText;
        }

        [Serializable]
        private class GearSlotUI
        {
            public string slotTag;
            public Button button;
            public Image bgImage;
            public Outline outline;
            public Image iconImage;
            public TextMeshProUGUI itemNameText;
            [NonSerialized] public string boundItemId;
        }

        [Header("Core Screen Reference")]
        [SerializeField] private GameStageScreen hubScreen;
        [SerializeField] private bool isBuilt;

        [Header("Cockpit Zones")]
        [SerializeField] private RectTransform topLeftInventoryPanel;
        [SerializeField] private RectTransform topRightSurvivorPanel;
        [SerializeField] private RectTransform bottomLeftMapPanel;
        [SerializeField] private RectTransform bottomCenterReaderPanel;
        [SerializeField] private RectTransform bottomRightActionPanel;
        [SerializeField] private RectTransform dockOverlayBackdrop;
        [SerializeField] private Button dockOverlayCloseBtn;

        [Header("Top-Left: Inventory & Micro-Inspector")]
        [SerializeField] private TextMeshProUGUI invHeaderBadgeText;
        [SerializeField] private List<SlotUI> inventorySlots = new List<SlotUI>();
        [SerializeField] private Image inspectorIcon;
        [SerializeField] private TextMeshProUGUI inspectorTitle;
        [SerializeField] private TextMeshProUGUI inspectorStats;
        [SerializeField] private TextMeshProUGUI inspectorDesc;
        [SerializeField] private Button inspectorUseEquipBtn;
        [SerializeField] private TextMeshProUGUI inspectorUseEquipLabel;
        [SerializeField] private Button inspectorOpenGearBtn;

        [Header("Top-Center: Stage Framing")]
        [SerializeField] private TextMeshProUGUI topCenterBadgeText;

        [Header("Top-Right: Survivor Status")]
        [SerializeField] private TextMeshProUGUI survivorLvlDayText;
        [SerializeField] private Image survivorPortraitImg;
        [SerializeField] private TextMeshProUGUI survivorNameText;
        [SerializeField] private TextMeshProUGUI survivorJobText;
        [SerializeField] private TextMeshProUGUI hungerValueText;
        [SerializeField] private TextMeshProUGUI thirstValueText;
        [SerializeField] private Image xpFillImg;
        [SerializeField] private TextMeshProUGUI xpValueText;
        [SerializeField] private List<VitalBarUI> vitalBars = new List<VitalBarUI>();

        // Top-Right 4-Tab Sub-Panel (M.O.R.T.A.L. | SKILLS | TRAITS | TEAM/GEAR)
        [SerializeField] private List<Button> survivorTabButtons = new List<Button>();
        [SerializeField] private List<Image> survivorTabBgs = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> survivorTabLabels = new List<TextMeshProUGUI>();
        [SerializeField] private GameObject survivorTabMortalView;
        [SerializeField] private GameObject survivorTabSkillsView;
        [SerializeField] private GameObject survivorTabTraitsView;
        [SerializeField] private GameObject survivorTabTeamGearView;

        [SerializeField] private List<StatBarRowUI> mortalRows = new List<StatBarRowUI>();
        [SerializeField] private List<StatBarRowUI> skillRows = new List<StatBarRowUI>();

        [SerializeField] private TextMeshProUGUI traitsCountHeaderText;
        [SerializeField] private RectTransform traitsScrollContent;
        [SerializeField] private TextMeshProUGUI traitsEmptyText;

        [SerializeField] private List<GearSlotUI> equippedGearSlots = new List<GearSlotUI>();
        [SerializeField] private TextMeshProUGUI partyHeaderText;
        [SerializeField] private TextMeshProUGUI partyLeaderNameText;
        [SerializeField] private TextMeshProUGUI partyLeaderMetaText;

        [Header("Bottom-Left: Floor Blueprint & Pinned Note")]
        [SerializeField] private TextMeshProUGUI mapHeaderTitle;
        [SerializeField] private TextMeshProUGUI mapSubHeaderPosText;
        [SerializeField] private Button expandMapBtn;
        [SerializeField] private TextMeshProUGUI pinnedNoteText;
        [SerializeField] private Button pinnedNoteBtn;

        [Header("Bottom-Center: 4-Tab Story Reader")]
        [SerializeField] private List<Button> readerTabButtons = new List<Button>();
        [SerializeField] private List<Image> readerTabBgs = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> readerTabLabels = new List<TextMeshProUGUI>();
        [SerializeField] private GameObject readerTabSituationView;
        [SerializeField] private RectTransform awakeningHost;
        [SerializeField] private RectTransform situationScrollContent;
        [SerializeField] private ScrollRect situationScrollRect;
        [SerializeField] private GameObject readerTabStoryLogView;
        [SerializeField] private TextMeshProUGUI inlineStoryLogText;
        [SerializeField] private ScrollRect inlineStoryLogScroll;
        [SerializeField] private GameObject readerTabNotesView;
        [SerializeField] private TextMeshProUGUI inlineNotesText;
        [SerializeField] private GameObject readerTabFloorView;
        [SerializeField] private Transform inlineFloorListRoot;
        [SerializeField] private TextMeshProUGUI inlineFloorEmptyText;
        [SerializeField] private Button inlineTakeAllFloorBtn;

        [Header("Bottom-Right: Action Deck & Utility Bar")]
        [SerializeField] private RectTransform choicesViewport;
        [SerializeField] private GameObject narrativePromptCard;
        [SerializeField] private Button narrativePromptBtn;
        [SerializeField] private Button utilPhoneBtn;
        [SerializeField] private TextMeshProUGUI utilPhoneBadge;
        [SerializeField] private Button utilNotesBtn;
        [SerializeField] private Button utilSkillsBtn;
        [SerializeField] private Button utilWaitBtn;
        [SerializeField] private Button utilSysBtn;

        private ReaderTab _activeTab = ReaderTab.Situation;
        private SurvivorSubTab _activeSurvivorTab = SurvivorSubTab.Mortal;
        private string _selectedItemId = "";
        private bool _selectedIsEquipped;
        private List<Sprite> _avatarFrames;
        private int _avatarFrameIndex;
        private float _avatarTimer;

        // Palette constants matching blueprint_v4_hybrid_1_and_3.png
        private static readonly Color ColBgOuter = new Color(0.03f, 0.04f, 0.06f, 1f);
        private static readonly Color ColPanelBg = new Color(0.063f, 0.082f, 0.118f, 0.98f);
        private static readonly Color ColHeaderBg = new Color(0.094f, 0.125f, 0.173f, 1f);
        private static readonly Color ColBorderSlate = new Color(0.18f, 0.23f, 0.305f, 0.95f);
        private static readonly Color ColSlotBg = new Color(0.055f, 0.075f, 0.105f, 0.96f);
        private static readonly Color ColCardBg = new Color(0.075f, 0.10f, 0.145f, 0.96f);
        private static readonly Color ColAmber = new Color(0.96f, 0.62f, 0.043f, 1f);
        private static readonly Color ColCyan = new Color(0.22f, 0.74f, 0.97f, 1f);
        private static readonly Color ColTextMain = new Color(0.90f, 0.925f, 0.96f, 1f);
        private static readonly Color ColTextMuted = new Color(0.55f, 0.62f, 0.72f, 1f);

        public static void SetPixelBox(RectTransform rt, float x1, float y1, float x2, float y2)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(x1 / 1920f, 1f - (y2 / 1080f));
            rt.anchorMax = new Vector2(x2 / 1920f, 1f - (y1 / 1080f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private static void StretchFull(RectTransform rt, float padLeft = 0f, float padBottom = 0f, float padRight = 0f, float padTop = 0f)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padLeft, padBottom);
            rt.offsetMax = new Vector2(-padRight, -padTop);
            rt.localScale = Vector3.one;
        }

        public void InitializeCockpit(GameStageScreen screen)
        {
            if (screen != null) hubScreen = screen;
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
            if (hubScreen == null) return;

            if (!isBuilt || topLeftInventoryPanel == null)
            {
                BuildCockpitHierarchy();
            }

            ReparentAndReframeExistingUI();
            HookInteractions();
            OnConsoleModeChanged(GameStageScreen.ConsoleMode.Narrative);
            SelectReaderTab(_activeTab, force: true);
            SelectSurvivorTab(_activeSurvivorTab, force: true);
            RefreshAll();
        }

        public void OnHubMenuSanitized()
        {
            if (hubScreen == null) return;
            ReparentSituationIntoReader();
            UpdateReaderAndActionDeckState();
        }

        public void OnConsoleModeChanged(GameStageScreen.ConsoleMode mode)
        {
            if (hubScreen == null) return;

            bool overlayOpen = mode == GameStageScreen.ConsoleMode.PDA || mode == GameStageScreen.ConsoleMode.Gear;
            Transform botConsole = transform.Find("BottomScreen_TouchConsole");
            if (botConsole != null)
            {
                var bg = botConsole.GetComponent<Image>();
                if (bg != null) bg.enabled = false;
                StretchFull(botConsole as RectTransform);
                botConsole.SetAsLastSibling();
            }

            if (hubScreen.DockContainerNarrative != null)
            {
                hubScreen.DockContainerNarrative.SetActive(false);
            }

            if (dockOverlayBackdrop != null)
            {
                dockOverlayBackdrop.gameObject.SetActive(overlayOpen);
            }

            if (hubScreen.DockContainerPDA != null)
            {
                bool activePda = mode == GameStageScreen.ConsoleMode.PDA;
                hubScreen.DockContainerPDA.SetActive(activePda);
                if (activePda) StyleOverlayDockPanel(hubScreen.DockContainerPDA);
            }

            if (hubScreen.DockContainerGear != null)
            {
                bool activeGear = mode == GameStageScreen.ConsoleMode.Gear;
                hubScreen.DockContainerGear.SetActive(activeGear);
                if (activeGear) StyleOverlayDockPanel(hubScreen.DockContainerGear);
            }

            EnsureModalZOrder();
        }

        public void RefreshAll()
        {
            if (hubScreen == null || !isBuilt) return;

            RefreshTopCenterHeader();
            RefreshInventoryGrid();
            RefreshSurvivorPanel();
            RefreshMapAndPinnedNote();
            RefreshReaderTabsContent();
            UpdateReaderAndActionDeckState();
        }

        private void Awake()
        {
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
        }

        private void OnEnable()
        {
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
            EnsureInteractionsHooked();
        }

        private void Start()
        {
            if (hubScreen != null)
            {
                InitializeCockpit(hubScreen);
            }
        }

        public void EnsureInteractionsHooked()
        {
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
            if (hubScreen == null) return;
            HookInteractions();
        }

        private void Update()
        {
            if (hubScreen == null || !isBuilt) return;

            UpdatePortraitAnimation();
            UpdateReaderAndActionDeckState();

            if (Application.isPlaying)
            {
                HandleHybridHotkeys();
            }
        }

        private void HandleHybridHotkeys()
        {
            if (IsAnyBlockingModalOpen()) return;

            bool pressedN = false;
            bool pressedM = false;
            bool pressedK = false;
            bool pressedT = false;
            bool pressed1 = false;
            bool pressed2 = false;
            bool pressedEsc = false;
            bool pressedContinue = false;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                pressedN = kb.nKey.wasPressedThisFrame;
                pressedM = kb.mKey.wasPressedThisFrame;
                pressedK = kb.kKey.wasPressedThisFrame;
                pressedT = kb.tKey.wasPressedThisFrame;
                pressed1 = kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                pressed2 = kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                pressedEsc = kb.escapeKey.wasPressedThisFrame;
                pressedContinue = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
            }
#else
            try
            {
                pressedN = Input.GetKeyDown(KeyCode.N);
                pressedM = Input.GetKeyDown(KeyCode.M);
                pressedK = Input.GetKeyDown(KeyCode.K);
                pressedT = Input.GetKeyDown(KeyCode.T);
                pressed1 = Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
                pressed2 = Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
                pressedEsc = Input.GetKeyDown(KeyCode.Escape);
                pressedContinue = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            }
            catch (InvalidOperationException) { }
#endif

            bool isAwakening = hubScreen.AwakeningContainer != null && hubScreen.AwakeningContainer.activeSelf;
            if (isAwakening && pressedContinue)
            {
                hubScreen.TriggerAdvanceAwakening();
                return;
            }

            bool sysMenuOpen = hubScreen.SystemMenuModal != null && hubScreen.SystemMenuModal.activeSelf;
            bool overlayOpen = (hubScreen.DockContainerPDA != null && hubScreen.DockContainerPDA.activeSelf) ||
                               (hubScreen.DockContainerGear != null && hubScreen.DockContainerGear.activeSelf);

            if (pressedEsc)
            {
                if (sysMenuOpen)
                {
                    hubScreen.CloseSystemMenuModal();
                }
                else if (MapViewerOpen)
                {
                    CloseMapViewer();
                }
                else if (overlayOpen)
                {
                    hubScreen.SwitchConsoleMode(GameStageScreen.ConsoleMode.Narrative);
                }
                else
                {
                    hubScreen.OpenSystemMenuModal();
                }
                return;
            }

            if (MapViewerOpen)
            {
                if (pressedM) CloseMapViewer();
                return;
            }

            if (sysMenuOpen || overlayOpen) return;

            if (pressedN)
            {
                SelectReaderTab(_activeTab == ReaderTab.Notes ? ReaderTab.Situation : ReaderTab.Notes);
            }
            else if (pressedM)
            {
                OpenPDATabOverlay("MAP");
            }
            else if (pressedK)
            {
                SelectSurvivorTab(SurvivorSubTab.Skills);
            }
            else if (pressedT)
            {
                SelectSurvivorTab(SurvivorSubTab.Traits);
            }
        }

        private bool IsAnyBlockingModalOpen()
        {
            if (hubScreen == null) return false;
            return hubScreen.IsCombatModalOpen ||
                   (hubScreen.PhoneModal != null && hubScreen.PhoneModal.activeSelf) ||
                   (hubScreen.StoryLogModal != null && hubScreen.StoryLogModal.activeSelf) ||
                   (hubScreen.RadioModal != null && hubScreen.RadioModal.activeSelf) ||
                   (hubScreen.HackingModal != null && hubScreen.HackingModal.activeSelf) ||
                   (hubScreen.SystemMenuModal != null && hubScreen.SystemMenuModal.activeSelf);
        }

        #region Hierarchy Construction (Serialized into MainGame_HybridUI.unity)
        private void BuildCockpitHierarchy()
        {
            ClearOldHybridNodes();

            var rootImg = GetComponent<Image>();
            if (rootImg != null) rootImg.color = ColBgOuter;

            BuildTopLeftInventoryPanel();
            BuildTopRightSurvivorPanel();
            BuildBottomLeftMapPanel();
            BuildBottomCenterReaderPanel();
            BuildBottomRightActionPanel();
            BuildDockOverlayBackdrop();

            isBuilt = true;
        }

        private void ClearOldHybridNodes()
        {
            string[] names =
            {
                "Hybrid_TopLeft_Inventory",
                "Hybrid_TopRight_Survivor",
                "Hybrid_BottomLeft_MapAndNotes",
                "Hybrid_BottomCenter_StoryReader",
                "Hybrid_BottomRight_ActionDeck"
            };

            foreach (string n in names)
            {
                Transform existing = transform.Find(n);
                if (existing != null)
                {
                    if (Application.isPlaying) Destroy(existing.gameObject);
                    else DestroyImmediate(existing.gameObject);
                }
            }

            Transform bot = transform.Find("BottomScreen_TouchConsole");
            if (bot != null)
            {
                Transform oldBackdrop = bot.Find("Hybrid_OverlayDimBackdrop");
                if (oldBackdrop != null)
                {
                    if (Application.isPlaying) Destroy(oldBackdrop.gameObject);
                    else DestroyImmediate(oldBackdrop.gameObject);
                }
            }

            inventorySlots.Clear();
            vitalBars.Clear();
            survivorTabButtons.Clear();
            survivorTabBgs.Clear();
            survivorTabLabels.Clear();
            mortalRows.Clear();
            skillRows.Clear();
            equippedGearSlots.Clear();
            readerTabButtons.Clear();
            readerTabBgs.Clear();
            readerTabLabels.Clear();
        }

        private RectTransform CreateCockpitPanelFrame(string objName, float x1, float y1, float x2, float y2,
            string headerTitle, out RectTransform bodyRect, out TextMeshProUGUI rightBadgeText,
            out Button rightActionBtn, string rightBadgeInitial = "", bool clickableRightBadge = false)
        {
            GameObject go = new GameObject(objName, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            SetPixelBox(rt, x1, y1, x2, y2);

            Image bg = go.AddComponent<Image>();
            bg.sprite = null;
            bg.color = ColPanelBg;
            bg.raycastTarget = true;

            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = ColBorderSlate;
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            // Header strip (32px)
            GameObject hdr = new GameObject("Header", typeof(RectTransform));
            hdr.transform.SetParent(go.transform, false);
            RectTransform hdrRt = hdr.GetComponent<RectTransform>();
            hdrRt.anchorMin = new Vector2(0f, 1f);
            hdrRt.anchorMax = new Vector2(1f, 1f);
            hdrRt.pivot = new Vector2(0.5f, 1f);
            hdrRt.sizeDelta = new Vector2(0f, 32f);
            hdrRt.anchoredPosition = Vector2.zero;

            Image hdrBg = hdr.AddComponent<Image>();
            hdrBg.sprite = null;
            hdrBg.color = ColHeaderBg;
            hdrBg.raycastTarget = false;

            GameObject hLine = new GameObject("BottomLine", typeof(RectTransform));
            hLine.transform.SetParent(hdr.transform, false);
            RectTransform hlRt = hLine.GetComponent<RectTransform>();
            hlRt.anchorMin = new Vector2(0f, 0f);
            hlRt.anchorMax = new Vector2(1f, 0f);
            hlRt.pivot = new Vector2(0.5f, 0f);
            hlRt.sizeDelta = new Vector2(0f, 1f);
            Image hlImg = hLine.AddComponent<Image>();
            hlImg.sprite = null;
            hlImg.color = ColBorderSlate;
            hlImg.raycastTarget = false;

            TextMeshProUGUI titleTmp = CreateText(hdr.transform, "Title", headerTitle, 11.5f, ColAmber,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Display, wrap: false);
            RectTransform tRt = titleTmp.rectTransform;
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(0.72f, 1f);
            tRt.offsetMin = new Vector2(12f, 0f);
            tRt.offsetMax = new Vector2(0f, 0f);

            rightActionBtn = null;
            if (clickableRightBadge)
            {
                GameObject btnGo = new GameObject("RightBadgeButton", typeof(RectTransform));
                btnGo.transform.SetParent(hdr.transform, false);
                RectTransform bRt = btnGo.GetComponent<RectTransform>();
                bRt.anchorMin = new Vector2(1f, 0.5f);
                bRt.anchorMax = new Vector2(1f, 0.5f);
                bRt.pivot = new Vector2(1f, 0.5f);
                bRt.anchoredPosition = new Vector2(-8f, 0f);
                bRt.sizeDelta = new Vector2(88f, 22f);

                Image bImg = btnGo.AddComponent<Image>();
                bImg.sprite = GetWhiteSprite();
                bImg.color = new Color(0.11f, 0.16f, 0.24f, 1f);

                Outline bOl = btnGo.AddComponent<Outline>();
                bOl.effectColor = ColCyan;
                bOl.effectDistance = new Vector2(1f, -1f);

                rightActionBtn = btnGo.AddComponent<Button>();
                rightActionBtn.targetGraphic = bImg;
                ConfigureButtonColors(rightActionBtn, bImg.color, new Color(0.17f, 0.25f, 0.36f, 1f));

                rightBadgeText = CreateText(btnGo.transform, "Label", rightBadgeInitial, 10.5f, ColCyan,
                    TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                StretchFull(rightBadgeText.rectTransform);
            }
            else
            {
                rightBadgeText = CreateText(hdr.transform, "RightBadge", rightBadgeInitial, 10.5f, ColTextMuted,
                    TextAlignmentOptions.MidlineRight, FontStyles.Bold, FontRole.Mono, wrap: false);
                RectTransform rbRt = rightBadgeText.rectTransform;
                rbRt.anchorMin = new Vector2(0.55f, 0f);
                rbRt.anchorMax = new Vector2(1f, 1f);
                rbRt.offsetMin = new Vector2(0f, 0f);
                rbRt.offsetMax = new Vector2(-12f, 0f);
            }

            GameObject bodyGo = new GameObject("Body", typeof(RectTransform));
            bodyGo.transform.SetParent(go.transform, false);
            bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(10f, 10f);
            bodyRect.offsetMax = new Vector2(-10f, -38f);

            return rt;
        }

        private void BuildTopLeftInventoryPanel()
        {
            topLeftInventoryPanel = CreateCockpitPanelFrame(
                "Hybrid_TopLeft_Inventory", 16f, 16f, 396f, 668f,
                "// INVENTORY // GEAR", out RectTransform body, out invHeaderBadgeText, out _, "0.0 / 18 KG");

            VerticalLayoutGroup vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            CreateInventorySection(body, "Weapons & Tools", "WPN", 1);
            CreateInventorySection(body, "Apparel & Protection", "APPAREL", 1);
            CreateInventorySection(body, "Medical & Rations", "CONSUMABLE", 2);
            CreateInventorySection(body, "Keys, Parts & Devices", "UTILITY", 2);

            // Micro-Inspector Strip at bottom (~92px)
            GameObject inspGo = new GameObject("MicroInspector", typeof(RectTransform));
            inspGo.transform.SetParent(body, false);
            LayoutElement ile = inspGo.AddComponent<LayoutElement>();
            ile.minHeight = 90f;
            ile.preferredHeight = 92f;
            ile.flexibleHeight = 1f;

            Image inspBg = inspGo.AddComponent<Image>();
            inspBg.sprite = GetWhiteSprite();
            inspBg.color = ColCardBg;
            Outline inspOl = inspGo.AddComponent<Outline>();
            inspOl.effectColor = new Color(0.24f, 0.32f, 0.42f, 0.9f);
            inspOl.effectDistance = new Vector2(1f, -1f);

            // Inspector Left Icon Box (top-left aligned with title/stats/desc)
            GameObject iIconBox = new GameObject("IconFrame", typeof(RectTransform));
            iIconBox.transform.SetParent(inspGo.transform, false);
            RectTransform iibRt = iIconBox.GetComponent<RectTransform>();
            iibRt.anchorMin = new Vector2(0f, 1f);
            iibRt.anchorMax = new Vector2(0f, 1f);
            iibRt.pivot = new Vector2(0f, 1f);
            iibRt.anchoredPosition = new Vector2(8f, -8f);
            iibRt.sizeDelta = new Vector2(54f, 54f);
            Image iibBg = iIconBox.AddComponent<Image>();
            iibBg.sprite = GetWhiteSprite();
            iibBg.color = ColSlotBg;
            Outline iibOl = iIconBox.AddComponent<Outline>();
            iibOl.effectColor = ColBorderSlate;

            GameObject iIconObj = new GameObject("Icon", typeof(RectTransform));
            iIconObj.transform.SetParent(iIconBox.transform, false);
            StretchFull(iIconObj.GetComponent<RectTransform>(), 5f, 5f, 5f, 5f);
            inspectorIcon = iIconObj.AddComponent<Image>();
            inspectorIcon.preserveAspect = true;
            inspectorIcon.raycastTarget = false;
            inspectorIcon.enabled = false;

            inspectorTitle = CreateText(inspGo.transform, "Title", "SELECT AN ITEM", 11.5f, ColTextMain,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Display, wrap: false);
            inspectorTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            inspectorTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            inspectorTitle.rectTransform.offsetMin = new Vector2(70f, -24f);
            inspectorTitle.rectTransform.offsetMax = new Vector2(-8f, -6f);

            inspectorStats = CreateText(inspGo.transform, "Stats", "Click any slot above to inspect, equip, or use.", 10f, ColAmber,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            inspectorStats.rectTransform.anchorMin = new Vector2(0f, 1f);
            inspectorStats.rectTransform.anchorMax = new Vector2(1f, 1f);
            inspectorStats.rectTransform.offsetMin = new Vector2(70f, -40f);
            inspectorStats.rectTransform.offsetMax = new Vector2(-8f, -24f);

            inspectorDesc = CreateText(inspGo.transform, "Desc", "Items sync directly with your backpack & gear.", 9.5f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Normal, FontRole.Body, wrap: true);
            inspectorDesc.overflowMode = TextOverflowModes.Ellipsis;
            inspectorDesc.rectTransform.anchorMin = new Vector2(0f, 0f);
            inspectorDesc.rectTransform.anchorMax = new Vector2(1f, 1f);
            inspectorDesc.rectTransform.offsetMin = new Vector2(70f, 30f);
            inspectorDesc.rectTransform.offsetMax = new Vector2(-8f, -41f);

            // Inspector Quick Action Buttons (bottom right)
            inspectorUseEquipBtn = CreateActionPill(inspGo.transform, "Btn_QuickAction", "USE / EQUIP",
                new Color(0.11f, 0.24f, 0.19f, 1f), new Color(0.16f, 0.72f, 0.45f, 1f), out inspectorUseEquipLabel, 10f);
            RectTransform ueRt = inspectorUseEquipBtn.GetComponent<RectTransform>();
            ueRt.anchorMin = new Vector2(0f, 0f);
            ueRt.anchorMax = new Vector2(0f, 0f);
            ueRt.pivot = new Vector2(0f, 0f);
            ueRt.anchoredPosition = new Vector2(70f, 5f);
            ueRt.sizeDelta = new Vector2(118f, 22f);

            inspectorOpenGearBtn = CreateActionPill(inspGo.transform, "Btn_OpenFullGear", "FULL GEAR [I]",
                new Color(0.10f, 0.14f, 0.21f, 1f), ColBorderSlate, out _, 10f);
            RectTransform ogRt = inspectorOpenGearBtn.GetComponent<RectTransform>();
            ogRt.anchorMin = new Vector2(1f, 0f);
            ogRt.anchorMax = new Vector2(1f, 0f);
            ogRt.pivot = new Vector2(1f, 0f);
            ogRt.anchoredPosition = new Vector2(-8f, 5f);
            ogRt.sizeDelta = new Vector2(110f, 22f);
        }

        private void CreateInventorySection(Transform parent, string title, string categoryTag, int rows)
        {
            GameObject sec = new GameObject($"Section_{categoryTag}", typeof(RectTransform));
            sec.transform.SetParent(parent, false);
            float height = 16f + rows * 52f + Mathf.Max(0, rows - 1) * 4f;
            LayoutElement le = sec.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            le.flexibleHeight = 0f;

            TextMeshProUGUI lbl = CreateText(sec.transform, "Label", title.ToUpperInvariant(), 9.5f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            lbl.rectTransform.anchorMin = new Vector2(0f, 1f);
            lbl.rectTransform.anchorMax = new Vector2(1f, 1f);
            lbl.rectTransform.offsetMin = new Vector2(2f, -15f);
            lbl.rectTransform.offsetMax = new Vector2(0f, 0f);

            GameObject grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(sec.transform, false);
            RectTransform gRt = grid.GetComponent<RectTransform>();
            gRt.anchorMin = Vector2.zero;
            gRt.anchorMax = Vector2.one;
            gRt.offsetMin = Vector2.zero;
            gRt.offsetMax = new Vector2(0f, -16f);

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(68f, 52f);
            glg.spacing = new Vector2(5f, 4f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.childAlignment = TextAnchor.UpperLeft;

            int count = rows * 5;
            for (int i = 0; i < count; i++)
            {
                GameObject sObj = new GameObject($"Slot_{categoryTag}_{i}", typeof(RectTransform));
                sObj.transform.SetParent(grid.transform, false);

                Image sBg = sObj.AddComponent<Image>();
                sBg.sprite = GetWhiteSprite();
                sBg.color = ColSlotBg;

                Outline sOl = sObj.AddComponent<Outline>();
                sOl.effectColor = ColBorderSlate;
                sOl.effectDistance = new Vector2(1f, -1f);

                Button btn = sObj.AddComponent<Button>();
                btn.targetGraphic = sBg;
                ConfigureButtonColors(btn, ColSlotBg, new Color(0.14f, 0.19f, 0.28f, 1f));

                GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
                iconGo.transform.SetParent(sObj.transform, false);
                StretchFull(iconGo.GetComponent<RectTransform>(), 7f, 6f, 7f, 6f);
                Image iconImg = iconGo.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                iconImg.enabled = false;

                TextMeshProUGUI eqp = CreateText(sObj.transform, "EqpBadge", "", 8.5f, new Color(0.22f, 0.88f, 0.45f, 1f),
                    TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
                eqp.rectTransform.anchorMin = new Vector2(0f, 1f);
                eqp.rectTransform.anchorMax = new Vector2(1f, 1f);
                eqp.rectTransform.offsetMin = new Vector2(4f, -13f);
                eqp.rectTransform.offsetMax = new Vector2(-4f, -2f);
                eqp.raycastTarget = false;

                TextMeshProUGUI cnt = CreateText(sObj.transform, "Count", "", 9.5f, ColTextMain,
                    TextAlignmentOptions.BottomRight, FontStyles.Bold, FontRole.Mono, wrap: false);
                cnt.rectTransform.anchorMin = new Vector2(0f, 0f);
                cnt.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                cnt.rectTransform.offsetMin = new Vector2(2f, 2f);
                cnt.rectTransform.offsetMax = new Vector2(-4f, 0f);
                cnt.raycastTarget = false;

                inventorySlots.Add(new SlotUI
                {
                    categoryTag = categoryTag,
                    root = sObj,
                    bgImage = sBg,
                    outline = sOl,
                    iconImage = iconImg,
                    eqpBadge = eqp,
                    countLabel = cnt,
                    button = btn
                });
            }
        }

        private void BuildTopRightSurvivorPanel()
        {
            topRightSurvivorPanel = CreateCockpitPanelFrame(
                "Hybrid_TopRight_Survivor", 1524f, 16f, 1904f, 668f,
                "// SURVIVOR // STATUS", out RectTransform body, out survivorLvlDayText, out _, "LVL 1 // DAY 0");

            VerticalLayoutGroup vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 1. Identity & Unified Status Card (168px height)
            GameObject idCard = new GameObject("IdentityCard", typeof(RectTransform));
            idCard.transform.SetParent(body, false);
            LayoutElement idLe = idCard.AddComponent<LayoutElement>();
            idLe.preferredHeight = 168f;
            idLe.minHeight = 168f;
            Image idBg = idCard.AddComponent<Image>();
            idBg.sprite = GetWhiteSprite();
            idBg.color = ColCardBg;
            Outline idOl = idCard.AddComponent<Outline>();
            idOl.effectColor = ColBorderSlate;

            // Portrait Frame (Left: 96x150)
            GameObject pBox = new GameObject("PortraitFrame", typeof(RectTransform));
            pBox.transform.SetParent(idCard.transform, false);
            RectTransform pRt = pBox.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0f, 0.5f);
            pRt.anchorMax = new Vector2(0f, 0.5f);
            pRt.pivot = new Vector2(0f, 0.5f);
            pRt.anchoredPosition = new Vector2(8f, 0f);
            pRt.sizeDelta = new Vector2(96f, 150f);
            Image pBg = pBox.AddComponent<Image>();
            pBg.sprite = GetWhiteSprite();
            pBg.color = new Color(0.04f, 0.055f, 0.08f, 1f);
            Outline pOl = pBox.AddComponent<Outline>();
            pOl.effectColor = ColCyan;
            pOl.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject pImgGo = new GameObject("Avatar", typeof(RectTransform));
            pImgGo.transform.SetParent(pBox.transform, false);
            StretchFull(pImgGo.GetComponent<RectTransform>(), 3f, 3f, 3f, 3f);
            survivorPortraitImg = pImgGo.AddComponent<Image>();
            survivorPortraitImg.preserveAspect = true;
            survivorPortraitImg.raycastTarget = false;
            Sprite defaultAvatar = LoadSpriteFallback("Assets/Sprites/Avatars/player.png");
            if (defaultAvatar != null) survivorPortraitImg.sprite = defaultAvatar;

            // Identity Header: Name & Job (Right stack: x = 112f to -8f)
            survivorNameText = CreateText(idCard.transform, "Name", "JAMES SMITH", 13.5f, ColTextMain,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Display, wrap: false);
            survivorNameText.rectTransform.anchorMin = new Vector2(0f, 1f);
            survivorNameText.rectTransform.anchorMax = new Vector2(1f, 1f);
            survivorNameText.rectTransform.offsetMin = new Vector2(112f, -22f);
            survivorNameText.rectTransform.offsetMax = new Vector2(-8f, -4f);

            survivorJobText = CreateText(idCard.transform, "Job", "SURVIVOR // FOCUSED", 9.5f, ColAmber,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            survivorJobText.rectTransform.anchorMin = new Vector2(0f, 1f);
            survivorJobText.rectTransform.anchorMax = new Vector2(1f, 1f);
            survivorJobText.rectTransform.offsetMin = new Vector2(112f, -38f);
            survivorJobText.rectTransform.offsetMax = new Vector2(-8f, -22f);

            // Three Core Survival Bars: Health (Red), Stamina (Amber), Humanity (Violet)
            vitalBars.Clear();
            CreateCompactVitalBar(idCard.transform, "HEALTH", new Color(0.86f, 0.21f, 0.27f, 1f), "hp_icon", -41f, 15f);
            CreateCompactVitalBar(idCard.transform, "STAMINA", new Color(0.92f, 0.70f, 0.03f, 1f), "stamina_icon", -58f, 15f);
            CreateCompactVitalBar(idCard.transform, "HUMANITY", new Color(0.60f, 0.25f, 0.95f, 1f), "humanity_icon", -75f, 15f);

            // Hunger & Thirst Side-by-Side Numeric Chips (Y: -94f to -116f, Height 22px)
            hungerValueText = CreateSurvivalChip(idCard.transform, "Chip_Hunger", "HUNGER", "hunger_icon",
                new Color(0.95f, 0.50f, 0.15f, 1f), 112f, 230f, -94f, 22f);
            thirstValueText = CreateSurvivalChip(idCard.transform, "Chip_Thirst", "THIRST", "thirst_icon",
                new Color(0.15f, 0.75f, 0.95f, 1f), 236f, 354f, -94f, 22f);

            // XP Bar at bottom of ID card (Y: -120f to -134f)
            GameObject xpTrack = new GameObject("XPBarTrack", typeof(RectTransform));
            xpTrack.transform.SetParent(idCard.transform, false);
            RectTransform xptRt = xpTrack.GetComponent<RectTransform>();
            xptRt.anchorMin = new Vector2(0f, 1f);
            xptRt.anchorMax = new Vector2(1f, 1f);
            xptRt.pivot = new Vector2(0f, 1f);
            xptRt.offsetMin = new Vector2(112f, -135f);
            xptRt.offsetMax = new Vector2(-8f, -121f);
            xpTrack.AddComponent<Image>().color = ColSlotBg;

            GameObject xpFill = new GameObject("Fill", typeof(RectTransform));
            xpFill.transform.SetParent(xpTrack.transform, false);
            StretchFull(xpFill.GetComponent<RectTransform>());
            xpFillImg = xpFill.AddComponent<Image>();
            xpFillImg.color = ColCyan;
            xpFillImg.raycastTarget = false;

            xpValueText = CreateText(xpTrack.transform, "Label", "XP 0 / 100", 8f, Color.white,
                TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
            StretchFull(xpValueText.rectTransform);

            // 2. 4-Tab Survivor Sub-Panel (M.O.R.T.A.L. | SKILLS | TRAITS | TEAM/GEAR)
            GameObject tabsCard = new GameObject("SurvivorTabsContainer", typeof(RectTransform));
            tabsCard.transform.SetParent(body, false);
            LayoutElement tcLe = tabsCard.AddComponent<LayoutElement>();
            tcLe.preferredHeight = 420f;
            tcLe.minHeight = 360f;
            tcLe.flexibleHeight = 1f;

            Image tcBg = tabsCard.AddComponent<Image>();
            tcBg.sprite = GetWhiteSprite();
            tcBg.color = ColCardBg;
            Outline tcOl = tabsCard.AddComponent<Outline>();
            tcOl.effectColor = ColBorderSlate;

            // Top 4-Tab Bar (30px height)
            GameObject stBar = new GameObject("TabBar", typeof(RectTransform));
            stBar.transform.SetParent(tabsCard.transform, false);
            RectTransform stbRt = stBar.GetComponent<RectTransform>();
            stbRt.anchorMin = new Vector2(0f, 1f);
            stbRt.anchorMax = new Vector2(1f, 1f);
            stbRt.pivot = new Vector2(0.5f, 1f);
            stbRt.sizeDelta = new Vector2(0f, 30f);

            Image stbBg = stBar.AddComponent<Image>();
            stbBg.sprite = GetWhiteSprite();
            stbBg.color = ColHeaderBg;

            HorizontalLayoutGroup stHlg = stBar.AddComponent<HorizontalLayoutGroup>();
            stHlg.padding = new RectOffset(4, 4, 3, 3);
            stHlg.spacing = 4f;
            stHlg.childControlWidth = true;
            stHlg.childControlHeight = true;
            stHlg.childForceExpandWidth = true;
            stHlg.childForceExpandHeight = true;

            string[] subTabTitles = { "M.O.R.T.A.L.", "SKILLS", "TRAITS (0)", "TEAM/GEAR" };
            for (int i = 0; i < subTabTitles.Length; i++)
            {
                GameObject tGo = new GameObject($"Tab_{i}", typeof(RectTransform));
                tGo.transform.SetParent(stBar.transform, false);
                Image tBg = tGo.AddComponent<Image>();
                tBg.sprite = GetWhiteSprite();
                tBg.color = ColSlotBg;

                Button tBtn = tGo.AddComponent<Button>();
                tBtn.targetGraphic = tBg;

                TextMeshProUGUI tLbl = CreateText(tGo.transform, "Label", subTabTitles[i], 9f, ColTextMuted,
                    TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                tLbl.overflowMode = TextOverflowModes.Ellipsis;
                StretchFull(tLbl.rectTransform, 2f, 0f, 2f, 0f);

                survivorTabButtons.Add(tBtn);
                survivorTabBgs.Add(tBg);
                survivorTabLabels.Add(tLbl);
            }

            // Tab Body Container
            GameObject stBody = new GameObject("TabBody", typeof(RectTransform));
            stBody.transform.SetParent(tabsCard.transform, false);
            RectTransform sbRt = stBody.GetComponent<RectTransform>();
            sbRt.anchorMin = Vector2.zero;
            sbRt.anchorMax = Vector2.one;
            sbRt.offsetMin = new Vector2(6f, 6f);
            sbRt.offsetMax = new Vector2(-6f, -34f);

            // Tab 0: M.O.R.T.A.L. Core Attributes (6 bars, scale 1-10)
            survivorTabMortalView = new GameObject("View_Mortal", typeof(RectTransform));
            survivorTabMortalView.transform.SetParent(stBody.transform, false);
            StretchFull(survivorTabMortalView.GetComponent<RectTransform>());

            TextMeshProUGUI mHdr = CreateText(survivorTabMortalView.transform, "SubHeader", "// M.O.R.T.A.L. ATTRIBUTES (1 - 10)", 9f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            mHdr.rectTransform.anchorMin = new Vector2(0f, 1f);
            mHdr.rectTransform.anchorMax = new Vector2(1f, 1f);
            mHdr.rectTransform.offsetMin = new Vector2(2f, -16f);
            mHdr.rectTransform.offsetMax = new Vector2(-2f, -1f);

            GameObject mortalList = new GameObject("MortalList", typeof(RectTransform));
            mortalList.transform.SetParent(survivorTabMortalView.transform, false);
            StretchFull(mortalList.GetComponent<RectTransform>(), 0f, 2f, 0f, 20f);
            VerticalLayoutGroup mVlg = mortalList.AddComponent<VerticalLayoutGroup>();
            mVlg.spacing = 6f;
            mVlg.childControlWidth = true;
            mVlg.childControlHeight = true;
            mVlg.childForceExpandWidth = true;
            mVlg.childForceExpandHeight = true;

            mortalRows.Add(CreateStatBarRow(mortalList.transform, "MGT", "MGT // MIGHT", "Carry capacity & melee impact", ColCyan, compact: false));
            mortalRows.Add(CreateStatBarRow(mortalList.transform, "OBS", "OBS // OBSERVATION", "Spot loot, traps & aim precision", ColCyan, compact: false));
            mortalRows.Add(CreateStatBarRow(mortalList.transform, "RFL", "RFL // REFLEX", "Initiative, dodge & move speed", ColCyan, compact: false));
            mortalRows.Add(CreateStatBarRow(mortalList.transform, "TAC", "TAC // TACTICS", "Logic, tech & battlefield awareness", ColCyan, compact: false));
            mortalRows.Add(CreateStatBarRow(mortalList.transform, "AUS", "AUS // AUSPICE", "Scavenge fortune & crit probability", ColCyan, compact: false));
            mortalRows.Add(CreateStatBarRow(mortalList.transform, "LDR", "LDR // LEADERSHIP", "Group cohesion, calm & persuasion", ColCyan, compact: false));

            // Tab 1: Survival Skills & Proficiencies (8 bars, scale 0-5)
            survivorTabSkillsView = new GameObject("View_Skills", typeof(RectTransform));
            survivorTabSkillsView.transform.SetParent(stBody.transform, false);
            StretchFull(survivorTabSkillsView.GetComponent<RectTransform>());

            TextMeshProUGUI skHdr = CreateText(survivorTabSkillsView.transform, "SubHeader", "// SURVIVAL SKILLS & PROFICIENCIES (0 - 5)", 9f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            skHdr.rectTransform.anchorMin = new Vector2(0f, 1f);
            skHdr.rectTransform.anchorMax = new Vector2(1f, 1f);
            skHdr.rectTransform.offsetMin = new Vector2(2f, -16f);
            skHdr.rectTransform.offsetMax = new Vector2(-2f, -1f);

            GameObject skillList = new GameObject("SkillsList", typeof(RectTransform));
            skillList.transform.SetParent(survivorTabSkillsView.transform, false);
            StretchFull(skillList.GetComponent<RectTransform>(), 0f, 2f, 0f, 20f);
            VerticalLayoutGroup skVlg = skillList.AddComponent<VerticalLayoutGroup>();
            skVlg.spacing = 4f;
            skVlg.childControlWidth = true;
            skVlg.childControlHeight = true;
            skVlg.childForceExpandWidth = true;
            skVlg.childForceExpandHeight = true;

            skillRows.Add(CreateStatBarRow(skillList.transform, "SHT", "SHOOTING", "Firearms & recoil", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "MEL", "MELEE", "Blunt & blade", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "STL", "STEALTH", "Quiet movement", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "TCH", "TECH", "Devices & locks", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "MED", "MEDICINE", "First aid & cure", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "SCV", "SCAVENGING", "Loot discovery", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "SRV", "SURVIVAL", "Rations & camp", ColAmber, compact: true));
            skillRows.Add(CreateStatBarRow(skillList.transform, "RSV", "RESOLVE", "Panic resistance", ColAmber, compact: true));

            // Tab 2: Traits & Perks (Scrollable compact green/red cards)
            survivorTabTraitsView = new GameObject("View_Traits", typeof(RectTransform));
            survivorTabTraitsView.transform.SetParent(stBody.transform, false);
            StretchFull(survivorTabTraitsView.GetComponent<RectTransform>());

            traitsCountHeaderText = CreateText(survivorTabTraitsView.transform, "SubHeader", "// PERKS & DRAWBACKS (0)", 9f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            traitsCountHeaderText.rectTransform.anchorMin = new Vector2(0f, 1f);
            traitsCountHeaderText.rectTransform.anchorMax = new Vector2(1f, 1f);
            traitsCountHeaderText.rectTransform.offsetMin = new Vector2(2f, -16f);
            traitsCountHeaderText.rectTransform.offsetMax = new Vector2(-2f, -1f);

            ScrollRect trScroll = CreateScrollArea(survivorTabTraitsView.transform, "TraitsScrollView", out traitsScrollContent);
            RectTransform trsRt = trScroll.GetComponent<RectTransform>();
            trsRt.offsetMin = Vector2.zero;
            trsRt.offsetMax = new Vector2(0f, -20f);

            VerticalLayoutGroup tvlg = traitsScrollContent.gameObject.AddComponent<VerticalLayoutGroup>();
            tvlg.spacing = 6f;
            tvlg.padding = new RectOffset(2, 6, 2, 2);
            tvlg.childControlWidth = true;
            tvlg.childControlHeight = true;
            tvlg.childForceExpandWidth = true;
            tvlg.childForceExpandHeight = false;

            traitsEmptyText = CreateText(survivorTabTraitsView.transform, "EmptyText", "No traits acquired.", 10f, ColTextMuted,
                TextAlignmentOptions.Center, FontStyles.Italic, FontRole.Body, wrap: true);
            StretchFull(traitsEmptyText.rectTransform, 10f, 10f, 10f, 22f);
            traitsEmptyText.gameObject.SetActive(false);

            // Tab 3: Team / Equipped Gear (Weapon box + 2x2 Armor grid + Party Group 1/4)
            survivorTabTeamGearView = new GameObject("View_TeamGear", typeof(RectTransform));
            survivorTabTeamGearView.transform.SetParent(stBody.transform, false);
            StretchFull(survivorTabTeamGearView.GetComponent<RectTransform>());

            VerticalLayoutGroup tgVlg = survivorTabTeamGearView.AddComponent<VerticalLayoutGroup>();
            tgVlg.spacing = 6f;
            tgVlg.padding = new RectOffset(0, 0, 1, 1);
            tgVlg.childControlWidth = true;
            tgVlg.childControlHeight = true;
            tgVlg.childForceExpandWidth = true;
            tgVlg.childForceExpandHeight = false;

            TextMeshProUGUI eqHdr = CreateText(survivorTabTeamGearView.transform, "GearHeader", "// EQUIPPED LOADOUT (CLICK TO INSPECT)", 9f, ColTextMuted,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            LayoutElement eqhLe = eqHdr.gameObject.AddComponent<LayoutElement>();
            eqhLe.preferredHeight = 16f;
            eqhLe.minHeight = 16f;

            equippedGearSlots.Add(CreateEquippedGearSlot(survivorTabTeamGearView.transform, "WPN", "MAIN WEAPON", 52f));

            GameObject armorRow1 = new GameObject("ArmorRow_Top", typeof(RectTransform));
            armorRow1.transform.SetParent(survivorTabTeamGearView.transform, false);
            LayoutElement ar1Le = armorRow1.AddComponent<LayoutElement>();
            ar1Le.preferredHeight = 50f;
            ar1Le.minHeight = 50f;
            HorizontalLayoutGroup ar1Hlg = armorRow1.AddComponent<HorizontalLayoutGroup>();
            ar1Hlg.spacing = 6f;
            ar1Hlg.childControlWidth = true;
            ar1Hlg.childControlHeight = true;
            ar1Hlg.childForceExpandWidth = true;
            ar1Hlg.childForceExpandHeight = true;
            equippedGearSlots.Add(CreateEquippedGearSlot(armorRow1.transform, "HEAD", "HEADGEAR", 50f));
            equippedGearSlots.Add(CreateEquippedGearSlot(armorRow1.transform, "BODY", "TORSO ARMOR", 50f));

            GameObject armorRow2 = new GameObject("ArmorRow_Bottom", typeof(RectTransform));
            armorRow2.transform.SetParent(survivorTabTeamGearView.transform, false);
            LayoutElement ar2Le = armorRow2.AddComponent<LayoutElement>();
            ar2Le.preferredHeight = 50f;
            ar2Le.minHeight = 50f;
            HorizontalLayoutGroup ar2Hlg = armorRow2.AddComponent<HorizontalLayoutGroup>();
            ar2Hlg.spacing = 6f;
            ar2Hlg.childControlWidth = true;
            ar2Hlg.childControlHeight = true;
            ar2Hlg.childForceExpandWidth = true;
            ar2Hlg.childForceExpandHeight = true;
            equippedGearSlots.Add(CreateEquippedGearSlot(armorRow2.transform, "LEGS", "LEGWEAR", 50f));
            equippedGearSlots.Add(CreateEquippedGearSlot(armorRow2.transform, "FEET", "FOOTWEAR", 50f));

            partyHeaderText = CreateText(survivorTabTeamGearView.transform, "PartyHeader", "// SURVIVOR GROUP // PARTY (1 / 4)", 9f, ColAmber,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            LayoutElement phLe = partyHeaderText.gameObject.AddComponent<LayoutElement>();
            phLe.preferredHeight = 18f;
            phLe.minHeight = 18f;

            GameObject partyCard = new GameObject("PartyGroupCard", typeof(RectTransform));
            partyCard.transform.SetParent(survivorTabTeamGearView.transform, false);
            LayoutElement pcLe = partyCard.AddComponent<LayoutElement>();
            pcLe.preferredHeight = 160f;
            pcLe.minHeight = 128f;
            pcLe.flexibleHeight = 1f;
            Image pcBg = partyCard.AddComponent<Image>();
            pcBg.sprite = GetWhiteSprite();
            pcBg.color = ColSlotBg;
            Outline pcOl = partyCard.AddComponent<Outline>();
            pcOl.effectColor = ColBorderSlate;

            VerticalLayoutGroup pVlg = partyCard.AddComponent<VerticalLayoutGroup>();
            pVlg.spacing = 4f;
            pVlg.padding = new RectOffset(5, 5, 5, 5);
            pVlg.childControlWidth = true;
            pVlg.childControlHeight = true;
            pVlg.childForceExpandWidth = true;
            pVlg.childForceExpandHeight = true;

            CreatePartyMemberRow(partyCard.transform, 1, active: true, out partyLeaderNameText, out partyLeaderMetaText);
            for (int slotIdx = 2; slotIdx <= 4; slotIdx++)
            {
                CreatePartyMemberRow(partyCard.transform, slotIdx, active: false, out _, out _);
            }
        }

        private void CreateCompactVitalBar(Transform parent, string label, Color barColor, string iconName, float topY, float height)
        {
            GameObject row = new GameObject($"Vital_{label}", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(112f, topY - height);
            rt.offsetMax = new Vector2(-8f, topY);

            Sprite iconSprite = FindExistingVitalIcon(iconName);
            if (iconSprite != null)
            {
                GameObject ic = new GameObject("Icon", typeof(RectTransform));
                ic.transform.SetParent(row.transform, false);
                RectTransform icRt = ic.GetComponent<RectTransform>();
                icRt.anchorMin = new Vector2(0f, 0.5f);
                icRt.anchorMax = new Vector2(0f, 0.5f);
                icRt.pivot = new Vector2(0f, 0.5f);
                icRt.anchoredPosition = Vector2.zero;
                icRt.sizeDelta = new Vector2(height - 1f, height - 1f);
                Image icImg = ic.AddComponent<Image>();
                icImg.sprite = iconSprite;
                icImg.preserveAspect = true;
                icImg.raycastTarget = false;
            }

            TextMeshProUGUI nameTmp = CreateText(row.transform, "Name", label, 8.5f, ColTextMain,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            nameTmp.rectTransform.anchorMin = new Vector2(0f, 0f);
            nameTmp.rectTransform.anchorMax = new Vector2(0f, 1f);
            nameTmp.rectTransform.offsetMin = new Vector2(18f, 0f);
            nameTmp.rectTransform.offsetMax = new Vector2(78f, 0f);

            GameObject track = new GameObject("Track", typeof(RectTransform));
            track.transform.SetParent(row.transform, false);
            RectTransform trRt = track.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0f, 0.15f);
            trRt.anchorMax = new Vector2(1f, 0.85f);
            trRt.offsetMin = new Vector2(80f, 0f);
            trRt.offsetMax = new Vector2(-54f, 0f);
            Image trImg = track.AddComponent<Image>();
            trImg.color = ColSlotBg;
            trImg.raycastTarget = false;

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(track.transform, false);
            StretchFull(fill.GetComponent<RectTransform>());
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = barColor;
            fillImg.raycastTarget = false;

            TextMeshProUGUI valTmp = CreateText(row.transform, "Value", "100 / 100", 8.5f, ColTextMain,
                TextAlignmentOptions.MidlineRight, FontStyles.Bold, FontRole.Mono, wrap: false);
            valTmp.rectTransform.anchorMin = new Vector2(1f, 0f);
            valTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            valTmp.rectTransform.offsetMin = new Vector2(-52f, 0f);
            valTmp.rectTransform.offsetMax = Vector2.zero;

            vitalBars.Add(new VitalBarUI
            {
                label = label,
                fillImage = fillImg,
                valueText = valTmp
            });
        }

        private TextMeshProUGUI CreateSurvivalChip(Transform parent, string name, string label, string iconName, Color textColor, float xMin, float xMax, float topY, float height)
        {
            GameObject chip = new GameObject(name, typeof(RectTransform));
            chip.transform.SetParent(parent, false);
            RectTransform rt = chip.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(xMin, topY - height);
            rt.offsetMax = new Vector2(xMax, topY);

            Image bg = chip.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = ColSlotBg;
            Outline ol = chip.AddComponent<Outline>();
            ol.effectColor = ColBorderSlate;

            Sprite iconSprite = FindExistingVitalIcon(iconName);
            if (iconSprite != null)
            {
                GameObject ic = new GameObject("Icon", typeof(RectTransform));
                ic.transform.SetParent(chip.transform, false);
                RectTransform icRt = ic.GetComponent<RectTransform>();
                icRt.anchorMin = new Vector2(0f, 0.5f);
                icRt.anchorMax = new Vector2(0f, 0.5f);
                icRt.pivot = new Vector2(0f, 0.5f);
                icRt.anchoredPosition = new Vector2(4f, 0f);
                icRt.sizeDelta = new Vector2(14f, 14f);
                Image icImg = ic.AddComponent<Image>();
                icImg.sprite = iconSprite;
                icImg.preserveAspect = true;
                icImg.raycastTarget = false;
            }

            TextMeshProUGUI tmp = CreateText(chip.transform, "Value", $"{label} 100%", 8.5f, textColor,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            tmp.rectTransform.anchorMin = Vector2.zero;
            tmp.rectTransform.anchorMax = Vector2.one;
            tmp.rectTransform.offsetMin = new Vector2(20f, 0f);
            tmp.rectTransform.offsetMax = new Vector2(-2f, 0f);
            return tmp;
        }

        private StatBarRowUI CreateStatBarRow(Transform parent, string key, string title, string subtitle, Color barColor, bool compact)
        {
            GameObject row = new GameObject($"StatRow_{key}", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            Image bg = row.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = ColSlotBg;
            Outline ol = row.AddComponent<Outline>();
            ol.effectColor = ColBorderSlate;

            float titleYMin = compact ? 0.44f : 0.48f;
            float titleSize = compact ? 10.5f : 11f;
            TextMeshProUGUI tTmp = CreateText(row.transform, "Title", title, titleSize, ColTextMain,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            tTmp.rectTransform.anchorMin = new Vector2(0f, titleYMin);
            tTmp.rectTransform.anchorMax = new Vector2(0.56f, 1f);
            tTmp.rectTransform.offsetMin = new Vector2(8f, 0f);
            tTmp.rectTransform.offsetMax = new Vector2(0f, -2f);

            TextMeshProUGUI subTmp = CreateText(row.transform, "Desc", subtitle, compact ? 8f : 8.5f, ColTextMuted,
                TextAlignmentOptions.MidlineLeft, FontStyles.Normal, FontRole.Body, wrap: false);
            subTmp.overflowMode = TextOverflowModes.Ellipsis;
            subTmp.rectTransform.anchorMin = Vector2.zero;
            subTmp.rectTransform.anchorMax = new Vector2(0.56f, titleYMin + 0.04f);
            subTmp.rectTransform.offsetMin = new Vector2(8f, 2f);
            subTmp.rectTransform.offsetMax = Vector2.zero;

            // Progress bar track (right-middle column: 0.56 to 0.85)
            GameObject track = new GameObject("Track", typeof(RectTransform));
            track.transform.SetParent(row.transform, false);
            RectTransform trRt = track.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0.56f, 0.5f);
            trRt.anchorMax = new Vector2(0.85f, 0.5f);
            trRt.pivot = new Vector2(0.5f, 0.5f);
            trRt.sizeDelta = new Vector2(0f, compact ? 8f : 10f);
            track.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 1f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(track.transform, false);
            StretchFull(fill.GetComponent<RectTransform>());
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = barColor;
            fillImg.raycastTarget = false;

            TextMeshProUGUI valTmp = CreateText(row.transform, "Val", "0", compact ? 10.5f : 13f, barColor,
                TextAlignmentOptions.MidlineRight, FontStyles.Bold, FontRole.Mono, wrap: false);
            valTmp.rectTransform.anchorMin = new Vector2(0.85f, 0f);
            valTmp.rectTransform.anchorMax = Vector2.one;
            valTmp.rectTransform.offsetMin = Vector2.zero;
            valTmp.rectTransform.offsetMax = new Vector2(-8f, 0f);

            return new StatBarRowUI
            {
                key = key,
                fillImage = fillImg,
                valueText = valTmp
            };
        }

        private GearSlotUI CreateEquippedGearSlot(Transform parent, string slotTag, string labelTitle, float height)
        {
            GameObject slotGo = new GameObject($"EquipSlot_{slotTag}", typeof(RectTransform));
            slotGo.transform.SetParent(parent, false);
            LayoutElement le = slotGo.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            Image bg = slotGo.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = ColSlotBg;
            Outline ol = slotGo.AddComponent<Outline>();
            ol.effectColor = ColBorderSlate;

            Button btn = slotGo.AddComponent<Button>();
            btn.targetGraphic = bg;
            ConfigureButtonColors(btn, ColSlotBg, new Color(0.14f, 0.20f, 0.30f, 1f));

            // Left Icon Box
            float iconSize = slotTag == "WPN" ? 36f : 32f;
            GameObject icFrame = new GameObject("IconFrame", typeof(RectTransform));
            icFrame.transform.SetParent(slotGo.transform, false);
            RectTransform ifRt = icFrame.GetComponent<RectTransform>();
            ifRt.anchorMin = new Vector2(0f, 0.5f);
            ifRt.anchorMax = new Vector2(0f, 0.5f);
            ifRt.pivot = new Vector2(0f, 0.5f);
            ifRt.anchoredPosition = new Vector2(7f, 0f);
            ifRt.sizeDelta = new Vector2(iconSize, iconSize);
            icFrame.AddComponent<Image>().color = new Color(0.03f, 0.045f, 0.07f, 1f);

            GameObject icGo = new GameObject("Icon", typeof(RectTransform));
            icGo.transform.SetParent(icFrame.transform, false);
            StretchFull(icGo.GetComponent<RectTransform>(), 3f, 3f, 3f, 3f);
            Image icImg = icGo.AddComponent<Image>();
            icImg.preserveAspect = true;
            icImg.raycastTarget = false;
            icImg.enabled = false;

            float textLeft = iconSize + 14f;
            TextMeshProUGUI tagTmp = CreateText(slotGo.transform, "SlotTag", $"{slotTag} // {labelTitle}", 8f, ColAmber,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            tagTmp.overflowMode = TextOverflowModes.Ellipsis;
            tagTmp.rectTransform.anchorMin = new Vector2(0f, 0.52f);
            tagTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            tagTmp.rectTransform.offsetMin = new Vector2(textLeft, 0f);
            tagTmp.rectTransform.offsetMax = new Vector2(-6f, -4f);

            TextMeshProUGUI nameTmp = CreateText(slotGo.transform, "ItemName", "NONE", 10f, ColTextMuted,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Body, wrap: false);
            nameTmp.overflowMode = TextOverflowModes.Ellipsis;
            nameTmp.rectTransform.anchorMin = Vector2.zero;
            nameTmp.rectTransform.anchorMax = new Vector2(1f, 0.54f);
            nameTmp.rectTransform.offsetMin = new Vector2(textLeft, 4f);
            nameTmp.rectTransform.offsetMax = new Vector2(-6f, 0f);

            return new GearSlotUI
            {
                slotTag = slotTag,
                button = btn,
                bgImage = bg,
                outline = ol,
                iconImage = icImg,
                itemNameText = nameTmp
            };
        }

        private void CreatePartyMemberRow(Transform parent, int slotIndex, bool active,
            out TextMeshProUGUI nameText, out TextMeshProUGUI metaText)
        {
            GameObject row = new GameObject($"PartySlot_{slotIndex}", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            Image bg = row.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = active ? new Color(0.08f, 0.125f, 0.19f, 1f) : new Color(0.045f, 0.06f, 0.085f, 0.85f);
            Outline ol = row.AddComponent<Outline>();
            ol.effectColor = active ? ColCyan : ColBorderSlate;

            nameText = CreateText(row.transform, "Name",
                active ? $"[{slotIndex}] LEADER // PLAYER" : $"[{slotIndex}] COMPANION SLOT // EMPTY",
                9.5f, active ? ColTextMain : ColTextMuted,
                TextAlignmentOptions.MidlineLeft, active ? FontStyles.Bold : FontStyles.Normal, FontRole.Mono, wrap: false);
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            nameText.rectTransform.anchorMin = Vector2.zero;
            nameText.rectTransform.anchorMax = new Vector2(0.68f, 1f);
            nameText.rectTransform.offsetMin = new Vector2(8f, 0f);
            nameText.rectTransform.offsetMax = Vector2.zero;

            metaText = CreateText(row.transform, "Meta",
                active ? "ACTIVE" : "STANDBY",
                8.5f, active ? new Color(0.28f, 0.88f, 0.45f, 1f) : ColTextMuted,
                TextAlignmentOptions.MidlineRight, FontStyles.Bold, FontRole.Mono, wrap: false);
            metaText.rectTransform.anchorMin = new Vector2(0.68f, 0f);
            metaText.rectTransform.anchorMax = Vector2.one;
            metaText.rectTransform.offsetMin = Vector2.zero;
            metaText.rectTransform.offsetMax = new Vector2(-8f, 0f);
        }

        private void PopulateTraitsList(CharacterData p)
        {
            if (traitsScrollContent == null) return;

            int traitCount = p != null && p.traits != null ? p.traits.Count : 0;
            if (traitsCountHeaderText != null)
            {
                traitsCountHeaderText.text = $"// PERKS & DRAWBACKS ({traitCount})";
            }
            if (survivorTabLabels.Count > (int)SurvivorSubTab.Traits && survivorTabLabels[(int)SurvivorSubTab.Traits] != null)
            {
                survivorTabLabels[(int)SurvivorSubTab.Traits].text = $"TRAITS ({traitCount})";
            }

            for (int i = traitsScrollContent.childCount - 1; i >= 0; i--)
            {
                Transform childTr = traitsScrollContent.GetChild(i);
                GameObject childGo = childTr.gameObject;
                childTr.SetParent(null, false);
                if (Application.isPlaying) Destroy(childGo);
                else DestroyImmediate(childGo);
            }

            if (traitCount == 0)
            {
                if (traitsEmptyText != null) traitsEmptyText.gameObject.SetActive(true);
                return;
            }

            if (traitsEmptyText != null) traitsEmptyText.gameObject.SetActive(false);

            for (int i = 0; i < p.traits.Count; i++)
            {
                var tr = p.traits[i];
                if (tr == null) continue;

                bool isDrawback = tr.isWeakness;
                GameObject row = new GameObject($"Trait_{i}_{tr.name}", typeof(RectTransform));
                row.transform.SetParent(traitsScrollContent, false);
                LayoutElement rLe = row.AddComponent<LayoutElement>();
                rLe.preferredHeight = 56f;
                rLe.minHeight = 56f;
                rLe.flexibleHeight = 0f;

                Image rBg = row.AddComponent<Image>();
                rBg.sprite = GetWhiteSprite();
                rBg.color = isDrawback
                    ? new Color(0.14f, 0.04f, 0.05f, 0.96f)
                    : new Color(0.04f, 0.11f, 0.05f, 0.96f);
                Outline rOl = row.AddComponent<Outline>();
                rOl.effectColor = isDrawback
                    ? new Color(0.58f, 0.16f, 0.18f, 0.90f)
                    : new Color(0.16f, 0.48f, 0.22f, 0.90f);

                // Trait Icon on Left (40x40)
                GameObject icGo = new GameObject("Icon", typeof(RectTransform));
                icGo.transform.SetParent(row.transform, false);
                RectTransform icRt = icGo.GetComponent<RectTransform>();
                icRt.anchorMin = new Vector2(0f, 0.5f);
                icRt.anchorMax = new Vector2(0f, 0.5f);
                icRt.pivot = new Vector2(0f, 0.5f);
                icRt.anchoredPosition = new Vector2(8f, 0f);
                icRt.sizeDelta = new Vector2(40f, 40f);
                Image icImg = icGo.AddComponent<Image>();
                Sprite trIcon = tr.icon != null ? tr.icon : ItemDatabase.GetTraitIcon(tr.name);
                icImg.sprite = trIcon;
                icImg.preserveAspect = true;
                icImg.raycastTarget = false;
                icImg.enabled = trIcon != null;

                // Name (Top)
                Color nameCol = isDrawback
                    ? new Color(0.95f, 0.35f, 0.35f, 1f)
                    : new Color(0.28f, 0.88f, 0.45f, 1f);
                TextMeshProUGUI nTmp = CreateText(row.transform, "Name", tr.name.ToUpperInvariant(), 10.5f, nameCol,
                    TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Body, wrap: false);
                nTmp.overflowMode = TextOverflowModes.Ellipsis;
                nTmp.rectTransform.anchorMin = new Vector2(0f, 0.54f);
                nTmp.rectTransform.anchorMax = new Vector2(0.70f, 1f);
                nTmp.rectTransform.offsetMin = new Vector2(56f, 0f);
                nTmp.rectTransform.offsetMax = new Vector2(0f, -5f);

                // Type Badge (Top-Right)
                string badgeStr = isDrawback ? "<color=#ef4444>DRAWBACK</color>" : "<color=#22c55e>PERK</color>";
                TextMeshProUGUI bTmp = CreateText(row.transform, "Badge", badgeStr, 8.5f, Color.white,
                    TextAlignmentOptions.TopRight, FontStyles.Bold, FontRole.Mono, wrap: false);
                bTmp.rectTransform.anchorMin = new Vector2(0.70f, 0.54f);
                bTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
                bTmp.rectTransform.offsetMin = Vector2.zero;
                bTmp.rectTransform.offsetMax = new Vector2(-8f, -5f);

                // Description (Bottom)
                string desc = !string.IsNullOrEmpty(tr.description) ? tr.description : "-";
                TextMeshProUGUI dTmp = CreateText(row.transform, "Desc", desc, 9f, new Color(0.84f, 0.88f, 0.90f, 1f),
                    TextAlignmentOptions.TopLeft, FontStyles.Normal, FontRole.Body, wrap: true);
                dTmp.overflowMode = TextOverflowModes.Ellipsis;
                dTmp.rectTransform.anchorMin = Vector2.zero;
                dTmp.rectTransform.anchorMax = new Vector2(1f, 0.56f);
                dTmp.rectTransform.offsetMin = new Vector2(56f, 4f);
                dTmp.rectTransform.offsetMax = new Vector2(-8f, 0f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(traitsScrollContent);
        }

        private void BuildBottomLeftMapPanel()
        {
            bottomLeftMapPanel = CreateCockpitPanelFrame(
                "Hybrid_BottomLeft_MapAndNotes", 16f, 682f, 396f, 1064f,
                "// FLOOR BLUEPRINT // 4F", out RectTransform body, out _, out expandMapBtn,
                "[M] MAP", clickableRightBadge: true);

            mapHeaderTitle = bottomLeftMapPanel.Find("Header/Title").GetComponent<TextMeshProUGUI>();

            // MiniMapCanvas (Top ~294px) — Hosts the docked architectural BlueprintViewport
            GameObject mapBox = new GameObject("MiniMapCanvas", typeof(RectTransform));
            mapBox.transform.SetParent(body, false);
            RectTransform mbRt = mapBox.GetComponent<RectTransform>();
            mbRt.anchorMin = new Vector2(0f, 0f);
            mbRt.anchorMax = new Vector2(1f, 1f);
            mbRt.offsetMin = new Vector2(0f, 88f);
            mbRt.offsetMax = Vector2.zero;

            Image mbBg = mapBox.AddComponent<Image>();
            mbBg.sprite = GetWhiteSprite();
            mbBg.color = new Color(0.025f, 0.04f, 0.07f, 0.98f);
            Outline mbOl = mapBox.AddComponent<Outline>();
            mbOl.effectColor = ColBorderSlate;

            // Pinned Note Card (Bottom 82px)
            GameObject noteGo = new GameObject("PinnedNoteCard", typeof(RectTransform));
            noteGo.transform.SetParent(body, false);
            RectTransform nRt = noteGo.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0f, 0f);
            nRt.anchorMax = new Vector2(1f, 0f);
            nRt.offsetMin = Vector2.zero;
            nRt.offsetMax = new Vector2(0f, 82f);

            Image nBg = noteGo.AddComponent<Image>();
            nBg.sprite = GetWhiteSprite();
            nBg.color = ColCardBg;
            Outline nOl = noteGo.AddComponent<Outline>();
            nOl.effectColor = new Color(0.45f, 0.32f, 0.10f, 0.9f);

            pinnedNoteBtn = noteGo.AddComponent<Button>();
            pinnedNoteBtn.targetGraphic = nBg;
            ConfigureButtonColors(pinnedNoteBtn, ColCardBg, new Color(0.14f, 0.17f, 0.23f, 1f));

            TextMeshProUGUI nTag = CreateText(noteGo.transform, "Tag", "PINNED NOTE / OBJECTIVE:", 9.5f, ColAmber,
                TextAlignmentOptions.TopLeft, FontStyles.Bold, FontRole.Mono, wrap: false);
            nTag.rectTransform.anchorMin = new Vector2(0f, 1f);
            nTag.rectTransform.anchorMax = new Vector2(1f, 1f);
            nTag.rectTransform.offsetMin = new Vector2(10f, -20f);
            nTag.rectTransform.offsetMax = new Vector2(-10f, -5f);

            pinnedNoteText = CreateText(noteGo.transform, "Body",
                "Pick up your apartment keys and belongings from the coffee table before unlocking Unit 407.",
                10f, ColTextMain, TextAlignmentOptions.TopLeft, FontStyles.Normal, FontRole.Body, wrap: true);
            pinnedNoteText.overflowMode = TextOverflowModes.Ellipsis;
            pinnedNoteText.rectTransform.anchorMin = Vector2.zero;
            pinnedNoteText.rectTransform.anchorMax = Vector2.one;
            pinnedNoteText.rectTransform.offsetMin = new Vector2(10f, 6f);
            pinnedNoteText.rectTransform.offsetMax = new Vector2(-10f, -22f);
        }

        private void BuildBottomCenterReaderPanel()
        {
            GameObject go = new GameObject("Hybrid_BottomCenter_StoryReader", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            bottomCenterReaderPanel = go.GetComponent<RectTransform>();
            SetPixelBox(bottomCenterReaderPanel, 412f, 682f, 1116f, 1064f);

            Image bg = go.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = ColPanelBg;
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = ColBorderSlate;
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            // Header Tab Strip (34px)
            GameObject tabBar = new GameObject("TabBar", typeof(RectTransform));
            tabBar.transform.SetParent(go.transform, false);
            RectTransform tbRt = tabBar.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 1f);
            tbRt.anchorMax = new Vector2(1f, 1f);
            tbRt.pivot = new Vector2(0.5f, 1f);
            tbRt.sizeDelta = new Vector2(0f, 34f);

            Image tbBg = tabBar.AddComponent<Image>();
            tbBg.sprite = GetWhiteSprite();
            tbBg.color = ColHeaderBg;

            HorizontalLayoutGroup hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(6, 6, 4, 4);
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            string[] tabTitles = { "CURRENT SITUATION", "STORY LOG", "NOTES", "FLOOR" };
            for (int i = 0; i < tabTitles.Length; i++)
            {
                GameObject tGo = new GameObject($"Tab_{i}_{tabTitles[i]}", typeof(RectTransform));
                tGo.transform.SetParent(tabBar.transform, false);
                Image tBg = tGo.AddComponent<Image>();
                tBg.sprite = GetWhiteSprite();
                tBg.color = ColSlotBg;

                Button tBtn = tGo.AddComponent<Button>();
                tBtn.targetGraphic = tBg;

                TextMeshProUGUI tLbl = CreateText(tGo.transform, "Label", tabTitles[i], 10.5f, ColTextMain,
                    TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                StretchFull(tLbl.rectTransform, 4f, 0f, 4f, 0f);

                readerTabButtons.Add(tBtn);
                readerTabBgs.Add(tBg);
                readerTabLabels.Add(tLbl);
            }

            // Reader Viewport Body
            GameObject readerBody = new GameObject("ReaderBody", typeof(RectTransform));
            readerBody.transform.SetParent(go.transform, false);
            RectTransform rbRt = readerBody.GetComponent<RectTransform>();
            rbRt.anchorMin = Vector2.zero;
            rbRt.anchorMax = Vector2.one;
            rbRt.offsetMin = new Vector2(10f, 10f);
            rbRt.offsetMax = new Vector2(-10f, -38f);

            // Tab 0: Current Situation View (contains AwakeningHost + SituationScrollView)
            readerTabSituationView = new GameObject("View_Situation", typeof(RectTransform));
            readerTabSituationView.transform.SetParent(readerBody.transform, false);
            StretchFull(readerTabSituationView.GetComponent<RectTransform>());

            GameObject awHost = new GameObject("AwakeningHost", typeof(RectTransform));
            awHost.transform.SetParent(readerTabSituationView.transform, false);
            awakeningHost = awHost.GetComponent<RectTransform>();
            StretchFull(awakeningHost);

            situationScrollRect = CreateScrollArea(readerTabSituationView.transform, "SituationScrollView", out situationScrollContent);
            VerticalLayoutGroup svlg = situationScrollContent.gameObject.AddComponent<VerticalLayoutGroup>();
            svlg.spacing = 8f;
            svlg.padding = new RectOffset(2, 6, 2, 4);
            svlg.childControlWidth = true;
            svlg.childControlHeight = true;
            svlg.childForceExpandWidth = true;
            svlg.childForceExpandHeight = false;

            // Tab 1: Inline Story Log View
            readerTabStoryLogView = new GameObject("View_StoryLog", typeof(RectTransform));
            readerTabStoryLogView.transform.SetParent(readerBody.transform, false);
            StretchFull(readerTabStoryLogView.GetComponent<RectTransform>());
            inlineStoryLogScroll = CreateScrollArea(readerTabStoryLogView.transform, "StoryLogScroll", out RectTransform slContent);
            VerticalLayoutGroup slVlg = slContent.gameObject.AddComponent<VerticalLayoutGroup>();
            slVlg.padding = new RectOffset(8, 12, 8, 8);
            slVlg.childControlWidth = true;
            slVlg.childControlHeight = true;
            slVlg.childForceExpandWidth = true;
            slVlg.childForceExpandHeight = false;

            inlineStoryLogText = CreateText(slContent, "StoryLogText", "", 12.5f, ColTextMain,
                TextAlignmentOptions.TopLeft, FontStyles.Normal, FontRole.Body, wrap: true);
            inlineStoryLogText.lineSpacing = 4f;

            // Tab 2: Inline Notes & Objectives View
            readerTabNotesView = new GameObject("View_Notes", typeof(RectTransform));
            readerTabNotesView.transform.SetParent(readerBody.transform, false);
            StretchFull(readerTabNotesView.GetComponent<RectTransform>());
            CreateScrollArea(readerTabNotesView.transform, "NotesScroll", out RectTransform notesContent);
            VerticalLayoutGroup nVlg = notesContent.gameObject.AddComponent<VerticalLayoutGroup>();
            nVlg.padding = new RectOffset(8, 12, 8, 8);
            nVlg.childControlWidth = true;
            nVlg.childControlHeight = true;
            nVlg.childForceExpandWidth = true;
            nVlg.childForceExpandHeight = false;

            inlineNotesText = CreateText(notesContent, "NotesText", "", 12.5f, ColTextMain,
                TextAlignmentOptions.TopLeft, FontStyles.Normal, FontRole.Body, wrap: true);
            inlineNotesText.lineSpacing = 4f;

            // Tab 3: Inline Floor Stash View
            readerTabFloorView = new GameObject("View_Floor", typeof(RectTransform));
            readerTabFloorView.transform.SetParent(readerBody.transform, false);
            StretchFull(readerTabFloorView.GetComponent<RectTransform>());

            inlineTakeAllFloorBtn = CreateActionPill(readerTabFloorView.transform, "Btn_TakeAllFloor", "TAKE ALL FLOOR ITEMS",
                new Color(0.11f, 0.24f, 0.19f, 1f), new Color(0.16f, 0.72f, 0.45f, 1f), out _, 11f);
            RectTransform tafRt = inlineTakeAllFloorBtn.GetComponent<RectTransform>();
            tafRt.anchorMin = new Vector2(0f, 1f);
            tafRt.anchorMax = new Vector2(1f, 1f);
            tafRt.offsetMin = new Vector2(0f, -32f);
            tafRt.offsetMax = Vector2.zero;

            ScrollRect flScroll = CreateScrollArea(readerTabFloorView.transform, "FloorScroll", out RectTransform flContent);
            RectTransform flsRt = flScroll.GetComponent<RectTransform>();
            flsRt.offsetMax = new Vector2(0f, -38f);
            VerticalLayoutGroup flVlg = flContent.gameObject.AddComponent<VerticalLayoutGroup>();
            flVlg.spacing = 6f;
            flVlg.padding = new RectOffset(4, 8, 4, 4);
            flVlg.childControlWidth = true;
            flVlg.childControlHeight = true;
            flVlg.childForceExpandWidth = true;
            flVlg.childForceExpandHeight = false;
            inlineFloorListRoot = flContent;

            inlineFloorEmptyText = CreateText(readerTabFloorView.transform, "EmptyText",
                "Nothing loose on the floor in this area right now.", 12f, ColTextMuted,
                TextAlignmentOptions.Center, FontStyles.Italic, FontRole.Body, wrap: true);
            StretchFull(inlineFloorEmptyText.rectTransform, 16f, 16f, 16f, 44f);
        }

        private void BuildBottomRightActionPanel()
        {
            bottomRightActionPanel = CreateCockpitPanelFrame(
                "Hybrid_BottomRight_ActionDeck", 1132f, 682f, 1904f, 1064f,
                "// WHAT SHOULD I DO NEXT?", out RectTransform body, out _, out utilSysBtn, "[ESC] SYSTEM", clickableRightBadge: true);

            // Full-Height Choices Viewport (Expanded to full 338px with ample breathing room)
            GameObject cVp = new GameObject("ChoicesViewport", typeof(RectTransform));
            cVp.transform.SetParent(body, false);
            choicesViewport = cVp.GetComponent<RectTransform>();
            choicesViewport.anchorMin = Vector2.zero;
            choicesViewport.anchorMax = Vector2.one;
            choicesViewport.offsetMin = Vector2.zero;
            choicesViewport.offsetMax = Vector2.zero;

            // Utility bar removed to give full 338px vertical height to choices
            utilPhoneBtn = null;
            utilPhoneBadge = null;
            utilNotesBtn = null;
            utilSkillsBtn = null;
            utilWaitBtn = null;
        }

        private Button CreateUtilityButton(Transform parent, string name, string label, Color borderCol, out TextMeshProUGUI badgeText)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image bg = go.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = ColHeaderBg;
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = borderCol;
            ol.effectDistance = new Vector2(1.2f, -1.2f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ConfigureButtonColors(btn, ColHeaderBg, new Color(0.16f, 0.21f, 0.30f, 1f));

            TextMeshProUGUI tmp = CreateText(go.transform, "Label", label, 11f, ColTextMain,
                TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Display, wrap: false);
            StretchFull(tmp.rectTransform, 4f, 0f, 4f, 0f);

            badgeText = CreateText(go.transform, "Badge", "", 9f, ColAmber,
                TextAlignmentOptions.TopRight, FontStyles.Bold, FontRole.Mono, wrap: false);
            badgeText.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
            badgeText.rectTransform.anchorMax = new Vector2(1f, 1f);
            badgeText.rectTransform.offsetMin = Vector2.zero;
            badgeText.rectTransform.offsetMax = new Vector2(-4f, -2f);

            return btn;
        }

        private void BuildDockOverlayBackdrop()
        {
            Transform botConsole = transform.Find("BottomScreen_TouchConsole");
            if (botConsole == null) return;

            GameObject bd = new GameObject("Hybrid_OverlayDimBackdrop", typeof(RectTransform));
            bd.transform.SetParent(botConsole, false);
            bd.transform.SetAsFirstSibling();
            dockOverlayBackdrop = bd.GetComponent<RectTransform>();
            StretchFull(dockOverlayBackdrop);

            Image bdImg = bd.AddComponent<Image>();
            bdImg.sprite = GetWhiteSprite();
            bdImg.color = new Color(0f, 0f, 0f, 0.72f);

            dockOverlayCloseBtn = bd.AddComponent<Button>();
            dockOverlayCloseBtn.targetGraphic = bdImg;
            bd.SetActive(false);

            // Add clean [ X ] CLOSE [ESC] buttons on Dock_SurvivorPDA & Dock_TacticalGear
            EnsureOverlayCloseButton(hubScreen != null ? hubScreen.DockContainerPDA : null);
            EnsureOverlayCloseButton(hubScreen != null ? hubScreen.DockContainerGear : null);
        }

        private void EnsureOverlayCloseButton(GameObject dockObj)
        {
            if (dockObj == null) return;
            Transform existing = dockObj.transform.Find("Hybrid_BtnCloseOverlay");
            if (existing == null)
            {
                Button btn = CreateActionPill(dockObj.transform, "Hybrid_BtnCloseOverlay", "[ X ] CLOSE OVERLAY [ESC]",
                    new Color(0.24f, 0.08f, 0.09f, 0.98f), new Color(0.92f, 0.28f, 0.28f, 1f), out _, 11f);
                RectTransform bRt = btn.GetComponent<RectTransform>();
                bRt.anchorMin = new Vector2(1f, 1f);
                bRt.anchorMax = new Vector2(1f, 1f);
                bRt.pivot = new Vector2(1f, 1f);
                bRt.anchoredPosition = new Vector2(-12f, -10f);
                bRt.sizeDelta = new Vector2(190f, 30f);
            }
        }
        #endregion

        #region Existing Scene Reparenting & Reframing
        private void ReparentAndReframeExistingUI()
        {
            if (hubScreen == null) return;

            // 1. Frame TopScreen_VisualStage into Top-Center (412, 16, 1508, 668)
            Transform topStage = transform.Find("TopScreen_VisualStage");
            if (topStage != null)
            {
                RectTransform tsRt = topStage as RectTransform;
                SetPixelBox(tsRt, 412f, 16f, 1508f, 668f);

                Image tsBg = topStage.GetComponent<Image>() ?? topStage.gameObject.AddComponent<Image>();
                tsBg.sprite = GetWhiteSprite();
                tsBg.color = ColPanelBg;
                Outline tsOl = topStage.GetComponent<Outline>() ?? topStage.gameObject.AddComponent<Outline>();
                tsOl.effectColor = ColBorderSlate;
                tsOl.effectDistance = new Vector2(1.5f, -1.5f);

                Transform framedStage = topStage.Find("FramedStage");
                if (framedStage != null)
                {
                    RectTransform fsRt = framedStage as RectTransform;
                    fsRt.anchorMin = new Vector2(0.005f, 0.01f);
                    fsRt.anchorMax = new Vector2(0.995f, 0.935f);
                    fsRt.pivot = new Vector2(0.5f, 0.5f);
                    fsRt.offsetMin = Vector2.zero;
                    fsRt.offsetMax = Vector2.zero;
                    fsRt.anchoredPosition = Vector2.zero;

                    var arf = framedStage.GetComponent<AspectRatioFitter>();
                    if (arf != null) arf.enabled = false;
                }
            }

            // 2. Frame TopHUD as the top header bar over TopScreen_VisualStage (412, 16, 1508, 52)
            Transform topHud = transform.Find("TopHUD");
            if (topHud != null)
            {
                RectTransform thRt = topHud as RectTransform;
                SetPixelBox(thRt, 412f, 16f, 1508f, 52f);

                Image thBg = topHud.GetComponent<Image>();
                if (thBg != null) thBg.color = ColHeaderBg;

                Transform vitalsRow = topHud.Find("VitalsRow");
                if (vitalsRow != null) vitalsRow.gameObject.SetActive(false);

                // Ensure center hybrid badge inside TopHUD
                Transform badgeTr = topHud.Find("HybridCenterBadge");
                if (badgeTr == null)
                {
                    topCenterBadgeText = CreateText(topHud, "HybridCenterBadge", "", 10.5f, ColCyan,
                        TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                    RectTransform bcRt = topCenterBadgeText.rectTransform;
                    bcRt.anchorMin = new Vector2(0.5f, 0.5f);
                    bcRt.anchorMax = new Vector2(0.5f, 0.5f);
                    bcRt.pivot = new Vector2(0.5f, 0.5f);
                    bcRt.sizeDelta = new Vector2(220f, 24f);
                }
                else
                {
                    topCenterBadgeText = badgeTr.GetComponent<TextMeshProUGUI>();
                }
            }

            // 3. Hide CenterSeam_ConsoleSwitcher
            Transform seam = transform.Find("CenterSeam_ConsoleSwitcher");
            if (seam != null) seam.gameObject.SetActive(false);

            // 4. Move AwakeningContainer into Bottom-Center Reader
            if (hubScreen.AwakeningContainer != null && awakeningHost != null)
            {
                if (hubScreen.AwakeningContainer.transform.parent != awakeningHost)
                {
                    hubScreen.AwakeningContainer.transform.SetParent(awakeningHost, false);
                }
                StretchFull(hubScreen.AwakeningContainer.GetComponent<RectTransform>(), 4f, 4f, 4f, 4f);
                AdjustAwakeningLayout(hubScreen.AwakeningContainer);
            }

            // 5. Move SituationBox / PackedSuccessBanner into SituationScrollView
            ReparentSituationIntoReader();

            // 6. Move HubMenuContainer into Bottom-Right ChoicesViewport
            if (hubScreen.HubMenuContainer != null && choicesViewport != null)
            {
                hubScreen.HubMenuContainer.transform.SetParent(choicesViewport, false);
                StretchFull(hubScreen.HubMenuContainer.GetComponent<RectTransform>());
                Transform sh = hubScreen.HubMenuContainer.transform.Find("ChoiceSubHeader");
                if (sh != null) sh.gameObject.SetActive(false);
            }

            // 6b. Dock authentic BlueprintViewport from Panel_Map into MiniMapCanvas
            Transform miniMap = bottomLeftMapPanel != null ? bottomLeftMapPanel.Find("Body/MiniMapCanvas") : null;
            Transform panelMap = transform.Find("ModalOverlays/PDA_Modal/TacticalPDA_PanelRoot/Panel_Map");
            if (panelMap == null && hubScreen != null && hubScreen.TacticalMap != null)
            {
                panelMap = hubScreen.TacticalMap.transform;
            }
            if (miniMap != null && panelMap != null)
            {
                Transform bpViewport = panelMap.Find("BlueprintViewport") ?? miniMap.Find("BlueprintViewport");
                if (bpViewport != null)
                {
                    if (bpViewport.parent != miniMap)
                    {
                        bpViewport.SetParent(miniMap, false);
                        RectTransform bpRt = bpViewport.GetComponent<RectTransform>();
                        bpRt.anchorMin = Vector2.zero;
                        bpRt.anchorMax = Vector2.one;
                        bpRt.offsetMin = Vector2.zero;
                        bpRt.offsetMax = Vector2.zero;
                    }

                    Transform zw = bpViewport.Find("BlueprintZoomWidget");
                    if (zw != null) zw.gameObject.SetActive(false);

                    if (!MapViewerOpen)
                    {
                        Transform subHdr = bpViewport.Find("BlueprintSubHeader");
                        if (subHdr != null)
                        {
                            Transform sl = subHdr.Find("ScaleLabel");
                            if (sl != null)
                            {
                                sl.gameObject.SetActive(false);
                            }
                            Transform ap = subHdr.Find("ActivePosBanner");
                            if (ap != null)
                            {
                                RectTransform apRt = ap.GetComponent<RectTransform>();
                                apRt.anchorMin = Vector2.zero;
                                apRt.anchorMax = Vector2.one;
                                apRt.offsetMin = new Vector2(4f, 0f);
                                apRt.offsetMax = new Vector2(-4f, 0f);
                                var apTmp = ap.GetComponent<TextMeshProUGUI>();
                                if (apTmp != null)
                                {
                                    apTmp.alignment = TextAlignmentOptions.MidlineLeft;
                                    apTmp.fontSize = 8.5f;
                                    apTmp.textWrappingMode = TextWrappingModes.NoWrap;
                                    apTmp.overflowMode = TextOverflowModes.Ellipsis;
                                }
                            }
                        }
                    }

                    Transform fpCanvas = bpViewport.Find("FloorplanContainer/FloorplanCanvas");
                    if (fpCanvas != null)
                    {
                        RectTransform fcRt = fpCanvas.GetComponent<RectTransform>();
                        fcRt.anchoredPosition = Vector2.zero;
                    }
                }
            }

            // Always enforce horizontal mini-map rotation + base scale (even when BlueprintViewport was already reparented)
            if (hubScreen.TacticalMap != null)
            {
                hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, 1.08f);
            }

            // Enforce clean simplified legend on the blueprint mini-map
            Transform bpLegend = bottomLeftMapPanel != null ? bottomLeftMapPanel.Find("Body/MiniMapCanvas/BlueprintViewport/LegendBar/LegendText") : null;
            if (bpLegend != null)
            {
                var lTmp = bpLegend.GetComponent<TextMeshProUGUI>();
                if (lTmp != null)
                {
                    lTmp.text = "<color=#00c7f2>■</color> CURRENT  <color=#94a3b8>■</color> VISITED  <color=#0a0f16>■</color> UNVISITED  <color=#ea2e38>■</color> LOCKED  <color=#f59e0b>■</color> BLOCKED";
                    lTmp.fontSize = 6.8f;
                    lTmp.textWrappingMode = TextWrappingModes.NoWrap;
                }
            }

            // 7. Reorder sibling indices so TopHUD stays on top of TopScreen_VisualStage,
            // and BottomScreen_TouchConsole + ModalOverlays sit above the 6 cockpit panels
            EnsureModalZOrder();
        }

        private void ReparentSituationIntoReader()
        {
            if (hubScreen == null || situationScrollContent == null) return;

            if (hubScreen.SituationBox != null && hubScreen.SituationBox.transform.parent != situationScrollContent)
            {
                hubScreen.SituationBox.transform.SetParent(situationScrollContent, false);
            }

            if (hubScreen.PackedSuccessBanner != null && hubScreen.PackedSuccessBanner.transform.parent != situationScrollContent)
            {
                hubScreen.PackedSuccessBanner.transform.SetParent(situationScrollContent, false);
            }
        }

        private void StyleOverlayDockPanel(GameObject dockObj)
        {
            if (dockObj == null) return;
            RectTransform rt = dockObj.GetComponent<RectTransform>();
            SetPixelBox(rt, 180f, 80f, 1740f, 990f);

            Image bg = dockObj.GetComponent<Image>() ?? dockObj.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = new Color(0.055f, 0.072f, 0.105f, 0.985f);

            Outline ol = dockObj.GetComponent<Outline>() ?? dockObj.AddComponent<Outline>();
            ol.effectColor = ColAmber;
            ol.effectDistance = new Vector2(2f, -2f);

            dockObj.transform.SetAsLastSibling();
            Transform closeTr = dockObj.transform.Find("Hybrid_BtnCloseOverlay");
            if (closeTr != null) closeTr.SetAsLastSibling();
        }

        private void EnsureModalZOrder()
        {
            Transform topStage = transform.Find("TopScreen_VisualStage");
            Transform topHud = transform.Find("TopHUD");
            if (topStage != null && topHud != null)
            {
                int stageIdx = topStage.GetSiblingIndex();
                if (topHud.GetSiblingIndex() < stageIdx)
                {
                    topHud.SetSiblingIndex(stageIdx + 1);
                }
            }

            Transform botConsole = transform.Find("BottomScreen_TouchConsole");
            if (botConsole != null) botConsole.SetAsLastSibling();

            Transform modalOverlays = transform.Find("ModalOverlays");
            if (modalOverlays != null) modalOverlays.SetAsLastSibling();

            if (hubScreen != null && hubScreen.SystemMenuModal != null)
            {
                hubScreen.SystemMenuModal.transform.SetAsLastSibling();
            }
        }
        #endregion

        #region Interactions & Hotkey Wiring
        public void HookInteractions()
        {
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
            if (hubScreen == null) return;

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                int idx = i;
                if (inventorySlots[i].button != null)
                {
                    inventorySlots[i].button.onClick.RemoveAllListeners();
                    inventorySlots[i].button.onClick.AddListener(() => OnInventorySlotClicked(idx));
                }
            }

            if (inspectorUseEquipBtn == null && topLeftInventoryPanel != null)
            {
                Transform tr = topLeftInventoryPanel.Find("Body/Inspector/Btn_QuickAction");
                if (tr != null) inspectorUseEquipBtn = tr.GetComponent<Button>();
            }

            if (inspectorUseEquipBtn != null)
            {
                inspectorUseEquipBtn.onClick.RemoveAllListeners();
                inspectorUseEquipBtn.onClick.AddListener(OnInspectorUseOrEquipClicked);
            }

            if (inspectorOpenGearBtn == null && topLeftInventoryPanel != null)
            {
                Transform tr = topLeftInventoryPanel.Find("Body/Inspector/Btn_OpenFullGear");
                if (tr != null) inspectorOpenGearBtn = tr.GetComponent<Button>();
            }

            if (inspectorOpenGearBtn != null)
            {
                inspectorOpenGearBtn.onClick.RemoveAllListeners();
                inspectorOpenGearBtn.onClick.AddListener(() => hubScreen.SwitchConsoleMode(GameStageScreen.ConsoleMode.Gear));
            }

            if (survivorTabButtons.Count == 0 && topRightSurvivorPanel != null)
            {
                Transform stBar = topRightSurvivorPanel.Find("Body/TabsCard/SubTabBar");
                if (stBar != null)
                {
                    survivorTabButtons.Clear();
                    survivorTabBgs.Clear();
                    survivorTabLabels.Clear();
                    for (int i = 0; i < 4; i++)
                    {
                        Transform tChild = i < stBar.childCount ? stBar.GetChild(i) : null;
                        if (tChild != null)
                        {
                            survivorTabButtons.Add(tChild.GetComponent<Button>());
                            survivorTabBgs.Add(tChild.GetComponent<Image>());
                            survivorTabLabels.Add(tChild.GetComponentInChildren<TextMeshProUGUI>());
                        }
                    }
                }
            }

            for (int i = 0; i < survivorTabButtons.Count; i++)
            {
                SurvivorSubTab sTab = (SurvivorSubTab)i;
                if (survivorTabButtons[i] != null)
                {
                    survivorTabButtons[i].onClick.RemoveAllListeners();
                    survivorTabButtons[i].onClick.AddListener(() => SelectSurvivorTab(sTab));
                }
            }

            for (int i = 0; i < equippedGearSlots.Count; i++)
            {
                GearSlotUI capSlot = equippedGearSlots[i];
                if (capSlot != null && capSlot.button != null)
                {
                    capSlot.button.onClick.RemoveAllListeners();
                    capSlot.button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(capSlot.boundItemId))
                        {
                            SelectItemForInspector(capSlot.boundItemId, isEquipped: true);
                            RefreshInventoryGrid();
                            RefreshTeamGearPanel(hubScreen != null ? hubScreen.CurrentPlayer : null);
                        }
                    });
                }
            }

            if (expandMapBtn == null && bottomLeftMapPanel != null)
            {
                Transform tr = bottomLeftMapPanel.Find("Header/RightBadgeButton");
                if (tr != null) expandMapBtn = tr.GetComponent<Button>();
            }

            if (expandMapBtn != null)
            {
                expandMapBtn.onClick.RemoveAllListeners();
                expandMapBtn.onClick.AddListener(() => OpenPDATabOverlay("MAP"));
            }

            if (pinnedNoteBtn == null && bottomLeftMapPanel != null)
            {
                Transform tr = bottomLeftMapPanel.Find("Body/PinnedNoteButton");
                if (tr != null) pinnedNoteBtn = tr.GetComponent<Button>();
            }

            if (pinnedNoteBtn != null)
            {
                pinnedNoteBtn.onClick.RemoveAllListeners();
                pinnedNoteBtn.onClick.AddListener(() => SelectReaderTab(ReaderTab.Notes));
            }

            if (readerTabButtons.Count == 0 && bottomCenterReaderPanel != null)
            {
                Transform tabBar = bottomCenterReaderPanel.Find("Body/TabBar");
                if (tabBar != null)
                {
                    readerTabButtons.Clear();
                    readerTabBgs.Clear();
                    readerTabLabels.Clear();
                    for (int i = 0; i < 4; i++)
                    {
                        Transform tChild = i < tabBar.childCount ? tabBar.GetChild(i) : null;
                        if (tChild != null)
                        {
                            readerTabButtons.Add(tChild.GetComponent<Button>());
                            readerTabBgs.Add(tChild.GetComponent<Image>());
                            readerTabLabels.Add(tChild.GetComponentInChildren<TextMeshProUGUI>());
                        }
                    }
                }
            }

            for (int i = 0; i < readerTabButtons.Count; i++)
            {
                ReaderTab tab = (ReaderTab)i;
                if (readerTabButtons[i] != null)
                {
                    readerTabButtons[i].onClick.RemoveAllListeners();
                    readerTabButtons[i].onClick.AddListener(() => SelectReaderTab(tab));
                }
            }

            if (inlineTakeAllFloorBtn != null)
            {
                inlineTakeAllFloorBtn.onClick.RemoveAllListeners();
                inlineTakeAllFloorBtn.onClick.AddListener(() =>
                {
                    hubScreen.TriggerTakeAllFloorItems();
                    RefreshAll();
                });
            }

            if (utilPhoneBtn != null)
            {
                utilPhoneBtn.onClick.RemoveAllListeners();
                utilPhoneBtn.onClick.AddListener(() => hubScreen.TriggerOpenPhone());
            }

            if (utilNotesBtn != null)
            {
                utilNotesBtn.onClick.RemoveAllListeners();
                utilNotesBtn.onClick.AddListener(() => SelectReaderTab(ReaderTab.Notes));
            }

            if (utilSkillsBtn != null)
            {
                utilSkillsBtn.onClick.RemoveAllListeners();
                utilSkillsBtn.onClick.AddListener(() => SelectSurvivorTab(SurvivorSubTab.Skills));
            }

            if (utilWaitBtn != null)
            {
                utilWaitBtn.onClick.RemoveAllListeners();
                utilWaitBtn.onClick.AddListener(() => hubScreen.TriggerWaitMinutes(15));
            }

            if (utilSysBtn == null && bottomRightActionPanel != null)
            {
                Transform tr = bottomRightActionPanel.Find("Header/RightBadgeButton");
                if (tr != null) utilSysBtn = tr.GetComponent<Button>();
            }

            if (utilSysBtn != null)
            {
                utilSysBtn.onClick.RemoveAllListeners();
                utilSysBtn.onClick.AddListener(() =>
                {
                    if (hubScreen.SystemMenuModal != null && hubScreen.SystemMenuModal.activeSelf)
                        hubScreen.CloseSystemMenuModal();
                    else
                        hubScreen.OpenSystemMenuModal();
                });
            }

            if (dockOverlayCloseBtn != null)
            {
                dockOverlayCloseBtn.onClick.RemoveAllListeners();
                dockOverlayCloseBtn.onClick.AddListener(() => hubScreen.SwitchConsoleMode(GameStageScreen.ConsoleMode.Narrative));
            }

            HookCloseButtonOnDock(hubScreen.DockContainerPDA);
            HookCloseButtonOnDock(hubScreen.DockContainerGear);
        }

        private void HookCloseButtonOnDock(GameObject dockObj)
        {
            if (dockObj == null) return;
            Transform tr = dockObj.transform.Find("Hybrid_BtnCloseOverlay");
            Button btn = tr != null ? tr.GetComponent<Button>() : null;
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => hubScreen.SwitchConsoleMode(GameStageScreen.ConsoleMode.Narrative));
            }
        }

        private void OpenPDATabOverlay(string tabName)
        {
            if (hubScreen == null) return;
            if (tabName == "MAP")
            {
                if (MapViewerOpen) CloseMapViewer(); else OpenMapViewer();
                return;
            }
            hubScreen.SwitchConsoleMode(GameStageScreen.ConsoleMode.PDA);
            hubScreen.TriggerSwitchPDATab(tabName);
        }

        #region Enlarged Tactical Map Viewer
        private GameObject mapViewerRoot;
        private RectTransform mapViewerHost;
        private Transform mapViewerOrigParent;
        private Transform mapViewerBp;
        private int mapViewerOrigIndex;
        private const float MiniMapBaseScale = 1.08f;

        private bool MapViewerOpen => mapViewerRoot != null && mapViewerRoot.activeSelf;

        private void BuildMapViewer()
        {
            mapViewerRoot = new GameObject("Hybrid_MapViewer", typeof(RectTransform));
            mapViewerRoot.transform.SetParent(transform, false);
            StretchFull(mapViewerRoot.GetComponent<RectTransform>());

            Image dim = mapViewerRoot.AddComponent<Image>();
            dim.sprite = GetWhiteSprite();
            dim.color = new Color(0f, 0f, 0f, 0.86f);
            Button dimBtn = mapViewerRoot.AddComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(CloseMapViewer);

            GameObject panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(mapViewerRoot.transform, false);
            RectTransform pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.06f, 0.07f);
            pRt.anchorMax = new Vector2(0.94f, 0.93f);
            pRt.offsetMin = Vector2.zero;
            pRt.offsetMax = Vector2.zero;
            Image pImg = panel.AddComponent<Image>();
            pImg.sprite = GetWhiteSprite();
            pImg.color = ColPanelBg;
            Outline pOl = panel.AddComponent<Outline>();
            pOl.effectColor = ColBorderSlate;
            pOl.effectDistance = new Vector2(1.5f, -1.5f);

            TextMeshProUGUI title = CreateText(panel.transform, "Title", "// FLOOR BLUEPRINT // TACTICAL MAP VIEWER", 16f,
                ColAmber, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Display, wrap: false);
            RectTransform tRt = title.rectTransform;
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.pivot = new Vector2(0f, 1f);
            tRt.offsetMin = new Vector2(16f, -44f);
            tRt.offsetMax = new Vector2(-260f, -6f);

            Button closeBtn = CreateActionPill(panel.transform, "BtnCloseMap", "[ X ] CLOSE MAP  [M / ESC]",
                new Color(0.30f, 0.07f, 0.07f, 1f), new Color(0.85f, 0.25f, 0.25f, 1f), out _, 12f);
            RectTransform cRt = closeBtn.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(1f, 1f);
            cRt.anchoredPosition = new Vector2(-12f, -8f);
            cRt.sizeDelta = new Vector2(230f, 32f);
            closeBtn.onClick.AddListener(CloseMapViewer);

            GameObject host = new GameObject("MapHost", typeof(RectTransform), typeof(RectMask2D));
            host.transform.SetParent(panel.transform, false);
            mapViewerHost = host.GetComponent<RectTransform>();
            StretchFull(mapViewerHost, 12f, 12f, 12f, 52f);

            mapViewerRoot.SetActive(false);
        }

        private void OpenMapViewer()
        {
            if (hubScreen == null || hubScreen.TacticalMap == null) return;
            Transform miniMap = bottomLeftMapPanel != null ? bottomLeftMapPanel.Find("Body/MiniMapCanvas") : null;
            Transform bp = miniMap != null ? miniMap.Find("BlueprintViewport") : null;
            if (bp == null) return;

            if (mapViewerRoot == null) BuildMapViewer();

            if (mapViewerRoot != null)
            {
                var titleTmp = mapViewerRoot.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null && mapHeaderTitle != null && !string.IsNullOrEmpty(mapHeaderTitle.text))
                {
                    titleTmp.text = mapHeaderTitle.text;
                }
            }

            Canvas.ForceUpdateCanvases();
            Vector2 miniSize = ((RectTransform)miniMap).rect.size;

            mapViewerOrigParent = bp.parent;
            mapViewerOrigIndex = bp.GetSiblingIndex();
            mapViewerBp = bp;

            mapViewerRoot.SetActive(true);
            mapViewerRoot.transform.SetAsLastSibling();
            bp.SetParent(mapViewerHost, false);
            StretchFull((RectTransform)bp);
            Canvas.ForceUpdateCanvases();

            Transform slOpen = bp.Find("BlueprintSubHeader/ScaleLabel");
            if (slOpen != null)
            {
                slOpen.gameObject.SetActive(true);
                RectTransform slRt = slOpen.GetComponent<RectTransform>();
                slRt.anchorMin = Vector2.zero;
                slRt.anchorMax = new Vector2(0.40f, 1f);
                slRt.offsetMin = new Vector2(8f, 0f);
                slRt.offsetMax = Vector2.zero;
            }
            Transform apOpen = bp.Find("BlueprintSubHeader/ActivePosBanner");
            if (apOpen != null)
            {
                RectTransform apRt = apOpen.GetComponent<RectTransform>();
                apRt.anchorMin = new Vector2(0.42f, 0f);
                apRt.anchorMax = Vector2.one;
                apRt.offsetMin = Vector2.zero;
                apRt.offsetMax = new Vector2(-8f, 0f);
                var apTmp = apOpen.GetComponent<TextMeshProUGUI>();
                if (apTmp != null)
                {
                    apTmp.alignment = TextAlignmentOptions.MidlineRight;
                    apTmp.fontSize = 10.5f;
                    apTmp.textWrappingMode = TextWrappingModes.NoWrap;
                    apTmp.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            Vector2 host = mapViewerHost.rect.size;
            float ratio = Mathf.Min(host.x / Mathf.Max(1f, miniSize.x), host.y / Mathf.Max(1f, miniSize.y));
            hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, MiniMapBaseScale * ratio * 0.94f);
            hubScreen.TacticalMap.RefreshAllNodes();
        }

        private void CloseMapViewer()
        {
            if (!MapViewerOpen) return;
            if (mapViewerBp != null && mapViewerOrigParent != null)
            {
                Transform slClose = mapViewerBp.Find("BlueprintSubHeader/ScaleLabel");
                if (slClose != null) slClose.gameObject.SetActive(false);
                Transform apClose = mapViewerBp.Find("BlueprintSubHeader/ActivePosBanner");
                if (apClose != null)
                {
                    RectTransform apRt = apClose.GetComponent<RectTransform>();
                    apRt.anchorMin = Vector2.zero;
                    apRt.anchorMax = Vector2.one;
                    apRt.offsetMin = new Vector2(4f, 0f);
                    apRt.offsetMax = new Vector2(-4f, 0f);
                    var apTmp = apClose.GetComponent<TextMeshProUGUI>();
                    if (apTmp != null)
                    {
                        apTmp.alignment = TextAlignmentOptions.MidlineLeft;
                        apTmp.fontSize = 8.5f;
                    }
                }

                mapViewerBp.SetParent(mapViewerOrigParent, false);
                StretchFull((RectTransform)mapViewerBp);
                mapViewerBp.SetSiblingIndex(mapViewerOrigIndex);
            }
            mapViewerRoot.SetActive(false);
            if (hubScreen != null && hubScreen.TacticalMap != null)
            {
                hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, MiniMapBaseScale);
                hubScreen.TacticalMap.RefreshAllNodes();
            }
        }
        #endregion

        public void SelectReaderTab(ReaderTab tab, bool force = false)
        {
            if (!force && _activeTab == tab) return;
            _activeTab = tab;

            if (readerTabSituationView != null) readerTabSituationView.SetActive(tab == ReaderTab.Situation);
            if (readerTabStoryLogView != null) readerTabStoryLogView.SetActive(tab == ReaderTab.StoryLog);
            if (readerTabNotesView != null) readerTabNotesView.SetActive(tab == ReaderTab.Notes);
            if (readerTabFloorView != null) readerTabFloorView.SetActive(tab == ReaderTab.Floor);

            for (int i = 0; i < readerTabBgs.Count; i++)
            {
                bool active = i == (int)tab;
                if (readerTabBgs[i] != null)
                    readerTabBgs[i].color = active ? new Color(0.16f, 0.21f, 0.30f, 1f) : ColSlotBg;
                if (i < readerTabLabels.Count && readerTabLabels[i] != null)
                    readerTabLabels[i].color = active ? ColAmber : ColTextMuted;
            }

            RefreshReaderTabsContent();

            if (tab == ReaderTab.StoryLog && inlineStoryLogScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                inlineStoryLogScroll.verticalNormalizedPosition = 0f;
            }
        }

        public void SelectSurvivorTab(SurvivorSubTab tab, bool force = false)
        {
            if (!force && _activeSurvivorTab == tab) return;
            _activeSurvivorTab = tab;

            if (survivorTabMortalView != null) survivorTabMortalView.SetActive(tab == SurvivorSubTab.Mortal);
            if (survivorTabSkillsView != null) survivorTabSkillsView.SetActive(tab == SurvivorSubTab.Skills);
            if (survivorTabTraitsView != null) survivorTabTraitsView.SetActive(tab == SurvivorSubTab.Traits);
            if (survivorTabTeamGearView != null) survivorTabTeamGearView.SetActive(tab == SurvivorSubTab.TeamGear);

            for (int i = 0; i < survivorTabBgs.Count; i++)
            {
                bool active = i == (int)tab;
                if (survivorTabBgs[i] != null)
                    survivorTabBgs[i].color = active ? new Color(0.16f, 0.21f, 0.30f, 1f) : ColSlotBg;
                if (i < survivorTabLabels.Count && survivorTabLabels[i] != null)
                    survivorTabLabels[i].color = active ? ColAmber : ColTextMuted;
            }

            if (tab == SurvivorSubTab.Traits && traitsScrollContent != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(traitsScrollContent);
            }
        }
        #endregion

        #region Live Data Sync (Inventory, Vitals, Map, Reader)
        private void RefreshTopCenterHeader()
        {
            if (topCenterBadgeText == null || hubScreen == null) return;
            string room = hubScreen.CurrentInspectedRoom;
            if (!string.IsNullOrEmpty(room))
            {
                topCenterBadgeText.text = $"VIEW // {room.ToUpperInvariant()}";
            }
            else if (hubScreen.IsInHallway)
            {
                topCenterBadgeText.text = $"4F CORRIDOR // SEC {hubScreen.HallwaySection}";
            }
            else
            {
                topCenterBadgeText.text = "";
            }
        }

        private struct DisplaySlotEntry
        {
            public string itemId;
            public int quantity;
            public bool isEquipped;
        }

        private void RefreshInventoryGrid()
        {
            CharacterData p = hubScreen != null ? hubScreen.CurrentPlayer : null;
            if (invHeaderBadgeText != null)
            {
                if (p != null)
                    invHeaderBadgeText.text = $"{p.GetTotalWeight():F1} KG  |  {p.GetTotalSlotsUsed()}/{p.bagLimit} SLOTS";
                else
                    invHeaderBadgeText.text = "0.0 KG  |  0/12 SLOTS";
            }

            var wpnList = new List<DisplaySlotEntry>();
            var appList = new List<DisplaySlotEntry>();
            var conList = new List<DisplaySlotEntry>();
            var utlList = new List<DisplaySlotEntry>();

            if (p != null)
            {
                // Equipped gear appears first in Weapons / Apparel with [EQP] badge
                AddEquippedEntry(p.equippedWpn, wpnList);
                AddEquippedEntry(p.equippedHead, appList);
                AddEquippedEntry(p.equippedBody, appList);
                AddEquippedEntry(p.equippedLegs, appList);
                AddEquippedEntry(p.equippedFeet, appList);

                if (p.inventory != null)
                {
                    foreach (var entry in p.inventory)
                    {
                        if (entry == null || string.IsNullOrEmpty(entry.itemId)) continue;
                        var d = new DisplaySlotEntry { itemId = entry.itemId, quantity = Mathf.Max(1, entry.quantity), isEquipped = false };
                        ItemCategory cat = ItemDatabase.GetDefinition(entry.itemId).category;
                        switch (cat)
                        {
                            case ItemCategory.RangedWeapon:
                            case ItemCategory.MeleeWeapon:
                            case ItemCategory.Ammunition:
                                wpnList.Add(d);
                                break;
                            case ItemCategory.WearableHead:
                            case ItemCategory.WearableBody:
                            case ItemCategory.WearableLegs:
                            case ItemCategory.WearableFeet:
                                appList.Add(d);
                                break;
                            case ItemCategory.MedicalConsumable:
                            case ItemCategory.FoodRation:
                            case ItemCategory.SkillBook:
                                conList.Add(d);
                                break;
                            default:
                                utlList.Add(d);
                                break;
                        }
                    }
                }

                if (p.keyItems != null)
                {
                    foreach (var k in p.keyItems)
                    {
                        if (k != null && !k.isUsed)
                        {
                            utlList.Add(new DisplaySlotEntry { itemId = k.itemId, quantity = 1, isEquipped = false });
                        }
                    }
                }
            }

            BindCategorySlots("WPN", wpnList);
            BindCategorySlots("APPAREL", appList);
            BindCategorySlots("CONSUMABLE", conList);
            BindCategorySlots("UTILITY", utlList);

            // Validate that the currently selected item actually exists in the character's gear or backpack
            if (!string.IsNullOrEmpty(_selectedItemId))
            {
                bool itemStillExists = wpnList.Any(x => string.Equals(x.itemId, _selectedItemId, StringComparison.OrdinalIgnoreCase)) ||
                                       appList.Any(x => string.Equals(x.itemId, _selectedItemId, StringComparison.OrdinalIgnoreCase)) ||
                                       conList.Any(x => string.Equals(x.itemId, _selectedItemId, StringComparison.OrdinalIgnoreCase)) ||
                                       utlList.Any(x => string.Equals(x.itemId, _selectedItemId, StringComparison.OrdinalIgnoreCase));
                if (!itemStillExists)
                {
                    _selectedItemId = "";
                    _selectedIsEquipped = false;
                }
            }

            if (string.IsNullOrEmpty(_selectedItemId))
            {
                if (wpnList.Count > 0) SelectItemForInspector(wpnList[0].itemId, wpnList[0].isEquipped);
                else if (conList.Count > 0) SelectItemForInspector(conList[0].itemId, false);
                else if (utlList.Count > 0) SelectItemForInspector(utlList[0].itemId, false);
                else if (appList.Count > 0) SelectItemForInspector(appList[0].itemId, appList[0].isEquipped);
                else
                {
                    _selectedItemId = "";
                    _selectedIsEquipped = false;
                    RefreshInspector();
                }
            }
            else
            {
                RefreshInspector();
            }
        }

        private static void AddEquippedEntry(string itemId, List<DisplaySlotEntry> list)
        {
            if (string.IsNullOrEmpty(itemId) || itemId.Equals("none", StringComparison.OrdinalIgnoreCase)) return;
            list.Add(new DisplaySlotEntry { itemId = itemId, quantity = 1, isEquipped = true });
        }

        private void BindCategorySlots(string categoryTag, List<DisplaySlotEntry> entries)
        {
            int idx = 0;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                SlotUI slot = inventorySlots[i];
                if (slot.categoryTag != categoryTag) continue;

                if (idx < entries.Count)
                {
                    var e = entries[idx++];
                    slot.boundItemId = e.itemId;
                    slot.isEquipped = e.isEquipped;

                    Sprite icon = ItemDatabase.GetItemIcon(e.itemId);
                    if (slot.iconImage != null)
                    {
                        slot.iconImage.sprite = icon;
                        slot.iconImage.enabled = icon != null;
                        slot.iconImage.color = Color.white;
                    }

                    if (slot.eqpBadge != null) slot.eqpBadge.text = e.isEquipped ? "EQP" : "";
                    if (slot.countLabel != null)
                    {
                        if (e.quantity > 1) slot.countLabel.text = $"x{e.quantity}";
                        else if (icon == null) slot.countLabel.text = ShortLabel(e.itemId);
                        else slot.countLabel.text = "";
                    }

                    bool isSelected = !string.IsNullOrEmpty(_selectedItemId) &&
                                      string.Equals(_selectedItemId, e.itemId, StringComparison.OrdinalIgnoreCase) &&
                                      _selectedIsEquipped == e.isEquipped;
                    var def = ItemDatabase.GetDefinition(e.itemId);
                    if (slot.outline != null)
                    {
                        slot.outline.effectColor = isSelected ? ColAmber : ItemDatabase.GetCategoryBorderColor(def.category);
                    }
                    if (slot.bgImage != null)
                    {
                        slot.bgImage.color = isSelected ? new Color(0.16f, 0.14f, 0.08f, 1f) : new Color(0.08f, 0.105f, 0.15f, 1f);
                    }
                }
                else
                {
                    slot.boundItemId = "";
                    slot.isEquipped = false;
                    if (slot.iconImage != null) { slot.iconImage.sprite = null; slot.iconImage.enabled = false; }
                    if (slot.eqpBadge != null) slot.eqpBadge.text = "";
                    if (slot.countLabel != null) slot.countLabel.text = "";
                    if (slot.outline != null) slot.outline.effectColor = ColBorderSlate;
                    if (slot.bgImage != null) slot.bgImage.color = ColSlotBg;
                }
            }
        }

        private void OnInventorySlotClicked(int index)
        {
            if (index < 0 || index >= inventorySlots.Count) return;
            SlotUI slot = inventorySlots[index];
            if (string.IsNullOrEmpty(slot.boundItemId))
            {
                ClearSelection();
                RefreshInventoryGrid();
                RefreshTeamGearPanel(hubScreen != null ? hubScreen.CurrentPlayer : null);
                return;
            }

            SelectItemForInspector(slot.boundItemId, slot.isEquipped);
            RefreshInventoryGrid();
            RefreshTeamGearPanel(hubScreen != null ? hubScreen.CurrentPlayer : null);
        }

        public void ClearSelection()
        {
            _selectedItemId = "";
            _selectedIsEquipped = false;
            RefreshInspector();
        }

        private void SelectItemForInspector(string itemId, bool isEquipped)
        {
            _selectedItemId = itemId;
            _selectedIsEquipped = isEquipped;
            RefreshInspector();
        }

        private void RefreshInspector()
        {
            if (string.IsNullOrEmpty(_selectedItemId))
            {
                if (inspectorIcon != null) inspectorIcon.enabled = false;
                if (inspectorTitle != null) inspectorTitle.text = "BACKPACK INSPECTOR";
                if (inspectorStats != null) inspectorStats.text = "Select an inventory slot above";
                if (inspectorDesc != null) inspectorDesc.text = "Weapons, clothing, meds, and key items sync automatically.";
                if (inspectorUseEquipBtn != null) inspectorUseEquipBtn.gameObject.SetActive(false);
                return;
            }

            var def = ItemDatabase.GetDefinition(_selectedItemId);
            if (def == null)
            {
                _selectedItemId = "";
                _selectedIsEquipped = false;
                RefreshInspector();
                return;
            }

            Sprite sp = ItemDatabase.GetItemIcon(_selectedItemId);
            if (inspectorIcon != null)
            {
                inspectorIcon.sprite = sp;
                inspectorIcon.enabled = sp != null;
            }

            if (inspectorTitle != null)
                inspectorTitle.text = $"{def.displayName.ToUpperInvariant()}" + (_selectedIsEquipped ? "  <color=#22C55E>[EQUIPPED]</color>" : "");

            if (inspectorStats != null)
            {
                var statParts = new List<string> { $"{def.weight:F1} KG" };
                if (def.accuracy > 0) statParts.Add($"ACC {def.accuracy}%");
                if (def.heal > 0) statParts.Add($"+{def.heal} HP");
                if (def.stamina != 0) statParts.Add($"{(def.stamina > 0 ? "+" : "")}{def.stamina} STM");
                if (def.hunger > 0) statParts.Add($"+{def.hunger} HNG");
                if (def.thirst > 0) statParts.Add($"+{def.thirst} THR");
                if (def.humanity > 0) statParts.Add($"+{def.humanity} HUM");
                inspectorStats.text = string.Join("  |  ", statParts);
            }

            if (inspectorDesc != null) inspectorDesc.text = def.description;

            if (inspectorUseEquipBtn != null && inspectorUseEquipLabel != null)
            {
                if (_selectedIsEquipped && !string.IsNullOrEmpty(def.EquipSlot))
                {
                    inspectorUseEquipBtn.gameObject.SetActive(true);
                    inspectorUseEquipLabel.text = "UNEQUIP";
                }
                else if (def.IsEquippable)
                {
                    inspectorUseEquipBtn.gameObject.SetActive(true);
                    inspectorUseEquipLabel.text = "EQUIP";
                }
                else if (def.IsConsumable)
                {
                    inspectorUseEquipBtn.gameObject.SetActive(true);
                    inspectorUseEquipLabel.text = "USE ITEM";
                }
                else
                {
                    inspectorUseEquipBtn.gameObject.SetActive(false);
                }
            }
        }

        private void OnInspectorUseOrEquipClicked()
        {
            if (hubScreen == null || hubScreen.CurrentPlayer == null || string.IsNullOrEmpty(_selectedItemId)) return;
            CharacterData p = hubScreen.CurrentPlayer;
            var def = ItemDatabase.GetDefinition(_selectedItemId);

            if (_selectedIsEquipped && !string.IsNullOrEmpty(def.EquipSlot))
            {
                if (p.UnequipItem(def.EquipSlot))
                {
                    _selectedIsEquipped = false;
                }
            }
            else if (def.IsEquippable && !string.IsNullOrEmpty(def.EquipSlot))
            {
                if (p.EquipItem(def.EquipSlot, _selectedItemId))
                {
                    _selectedIsEquipped = true;
                }
            }
            else if (def.IsConsumable)
            {
                if (p.UseItem(_selectedItemId, out string feedback))
                {
                    if (!p.HasItem(_selectedItemId)) _selectedItemId = "";
                    hubScreen.SetSituationNarration($"<color=#22C55E>[SUPPLY USED]</color> {feedback}");
                }
            }

            hubScreen.RefreshFromHybridCockpit();
        }

        private void RefreshSurvivorPanel()
        {
            CharacterData p = hubScreen != null ? hubScreen.CurrentPlayer : null;
            int dayCount = hubScreen != null && hubScreen.Clock != null ? hubScreen.Clock.DayCount : 0;

            if (p == null)
            {
                if (survivorLvlDayText != null) survivorLvlDayText.text = $"LVL 1 // DAY {dayCount}";
                return;
            }

            if (survivorLvlDayText != null) survivorLvlDayText.text = $"LVL {p.level} // DAY {dayCount}";
            if (survivorNameText != null) survivorNameText.text = p.FullName.ToUpperInvariant();
            if (survivorJobText != null) survivorJobText.text = $"{p.job.ToUpperInvariant()} // {p.gender.ToUpperInvariant()}";

            if (hungerValueText != null)
            {
                Color hCol = p.hunger <= 25 ? ColAmber : ColTextMain;
                hungerValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(hCol)}>HUNGER {p.hunger}%</color>";
            }

            if (thirstValueText != null)
            {
                Color tCol = p.thirst <= 25 ? ColAmber : ColTextMain;
                thirstValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(tCol)}>THIRST {p.thirst}%</color>";
            }

            if (xpFillImg != null) SetFillWidth(xpFillImg.rectTransform, Mathf.Clamp01((float)p.xp / Mathf.Max(1, p.xpRequired)));
            if (xpValueText != null) xpValueText.text = $"XP {p.xp} / {p.xpRequired}";

            // Vitals: 0 = HEALTH (Red), 1 = STAMINA (Amber), 2 = HUMANITY (Violet)
            SetVitalBar(0, p.hp, p.hpMax);
            SetVitalBar(1, p.stamina, p.staminaMax);
            SetVitalBar(2, p.humanity, 100);

            // Core M.O.R.T.A.L. Attributes (6 bars, scale 1-10)
            SetStatBarRowValue("MGT", p.Might, 10, showFraction: false, mortalRows);
            SetStatBarRowValue("OBS", p.Observation, 10, showFraction: false, mortalRows);
            SetStatBarRowValue("RFL", p.Reflex, 10, showFraction: false, mortalRows);
            SetStatBarRowValue("TAC", p.Tactics, 10, showFraction: false, mortalRows);
            SetStatBarRowValue("AUS", p.Auspice, 10, showFraction: false, mortalRows);
            SetStatBarRowValue("LDR", p.Leadership, 10, showFraction: false, mortalRows);

            // Survival Skills (8 bars, scale 0-5)
            SetStatBarRowValue("SHT", p.shooting, 5, showFraction: true, skillRows);
            SetStatBarRowValue("MEL", p.melee, 5, showFraction: true, skillRows);
            SetStatBarRowValue("STL", p.stealth, 5, showFraction: true, skillRows);
            SetStatBarRowValue("TCH", p.tech, 5, showFraction: true, skillRows);
            SetStatBarRowValue("MED", p.medicine, 5, showFraction: true, skillRows);
            SetStatBarRowValue("SCV", p.scavenging, 5, showFraction: true, skillRows);
            SetStatBarRowValue("SRV", p.survival, 5, showFraction: true, skillRows);
            SetStatBarRowValue("RSV", p.resolve, 5, showFraction: true, skillRows);

            // Dynamic Traits List
            PopulateTraitsList(p);

            // Team & Equipped Gear Panel
            RefreshTeamGearPanel(p);
        }

        private static void SetStatBarRowValue(string key, int val, int maxVal, bool showFraction, List<StatBarRowUI> rows)
        {
            if (rows == null) return;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null || !string.Equals(row.key, key, StringComparison.OrdinalIgnoreCase)) continue;
                if (row.fillImage != null)
                {
                    SetFillWidth(row.fillImage.rectTransform, Mathf.Clamp01((float)val / Mathf.Max(1, maxVal)));
                }
                if (row.valueText != null)
                {
                    row.valueText.text = showFraction ? $"{val} / {maxVal}" : val.ToString();
                }
                return;
            }
        }

        private void RefreshTeamGearPanel(CharacterData p)
        {
            if (p == null) return;

            for (int i = 0; i < equippedGearSlots.Count; i++)
            {
                GearSlotUI slot = equippedGearSlots[i];
                if (slot == null) continue;

                string eqId = p.GetEquippedInSlot(slot.slotTag);
                bool hasItem = !string.IsNullOrEmpty(eqId) && !eqId.Equals("none", StringComparison.OrdinalIgnoreCase);
                slot.boundItemId = hasItem ? eqId : "";

                if (slot.iconImage != null)
                {
                    Sprite sp = hasItem ? ItemDatabase.GetItemIcon(eqId) : null;
                    slot.iconImage.sprite = sp;
                    slot.iconImage.enabled = sp != null;
                }

                if (slot.itemNameText != null)
                {
                    if (hasItem)
                    {
                        var def = ItemDatabase.GetDefinition(eqId);
                        slot.itemNameText.text = def != null && !string.IsNullOrEmpty(def.displayName)
                            ? def.displayName.ToUpperInvariant()
                            : eqId.ToUpperInvariant();
                        slot.itemNameText.color = ColTextMain;
                    }
                    else
                    {
                        slot.itemNameText.text = slot.slotTag == "WPN" ? "UNARMED" : "NONE";
                        slot.itemNameText.color = ColTextMuted;
                    }
                }

                bool selected = hasItem && _selectedIsEquipped && string.Equals(_selectedItemId, eqId, StringComparison.OrdinalIgnoreCase);
                if (slot.outline != null)
                {
                    slot.outline.effectColor = selected ? ColAmber : (hasItem ? ColCyan : ColBorderSlate);
                }
                if (slot.bgImage != null)
                {
                    slot.bgImage.color = selected ? new Color(0.14f, 0.17f, 0.24f, 1f) : ColSlotBg;
                }
            }

            if (partyHeaderText != null)
            {
                partyHeaderText.text = "// SURVIVOR GROUP // PARTY (1 / 4)";
            }
            if (partyLeaderNameText != null)
            {
                partyLeaderNameText.text = $"[1] LEADER // {p.FullName.ToUpperInvariant()}";
            }
            if (partyLeaderMetaText != null)
            {
                partyLeaderMetaText.text = $"HP {p.hp}/{p.hpMax} | LVL {p.level}";
            }
        }

        private void SetVitalBar(int idx, int cur, int max)
        {
            if (idx < 0 || idx >= vitalBars.Count) return;
            var bar = vitalBars[idx];
            if (bar.fillImage != null) SetFillWidth(bar.fillImage.rectTransform, Mathf.Clamp01((float)cur / Mathf.Max(1, max)));
            if (bar.valueText != null) bar.valueText.text = $"{cur} / {max}";
        }

        private static void SetFillWidth(RectTransform fillRt, float pct)
        {
            if (fillRt == null) return;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(pct), 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
        }

        private void RefreshMapAndPinnedNote()
        {
            if (hubScreen == null) return;

            string locId = hubScreen.GetCurrentLocationId() ?? "unit_407";
            string floorTag = "4F";
            if (hubScreen.TacticalMap != null && hubScreen.TacticalMap.CurrentFloorData != null)
            {
                string fid = hubScreen.TacticalMap.CurrentFloorData.floorId;
                if (fid == "stairwell_4f") floorTag = "STAIRWELL (4F)";
                else if (fid == "stairwell_2f") floorTag = "STAIRWELL (2F)";
                else if (fid == "ascott_lobby") floorTag = "1F LOBBY";
                else if (fid == "ascott_2f" || hubScreen.IsOnSecondFloor) floorTag = "2F";
                else floorTag = "4F";
            }
            else if (hubScreen.GroundFloorLobbyZone != null && hubScreen.GroundFloorLobbyZone.isInLobby)
            {
                floorTag = "1F LOBBY";
            }
            else if (hubScreen.IsOnSecondFloor)
            {
                floorTag = "2F";
            }

            if (mapHeaderTitle != null) mapHeaderTitle.text = $"// FLOOR BLUEPRINT // {floorTag}";

            if (mapSubHeaderPosText != null)
            {
                string displayLoc = locId.ToUpperInvariant().Replace('_', ' ');
                if (locId == "f4_entrance") displayLoc = "F4 ENTRANCE";
                else if (locId == "f3_landing") displayLoc = "F3 LANDING";
                else if (locId == "f2_entrance") displayLoc = "F2 ENTRANCE";
                else if (locId == "f1_landing") displayLoc = "F1 LANDING / BARRICADE";
                else if (locId == "nw_room") displayLoc = "WEST STAIRS EXIT";
                else if (locId == "n_room") displayLoc = "FALLEN SOLDIER";
                else if (locId == "ne_room") displayLoc = "RECEPTION DESK";
                else if (locId == "e_room") displayLoc = "ELEVATOR BANK";
                else if (locId == "c_room") displayLoc = "LOBBY ROTUNDA";
                else if (locId == "lobby_exit") displayLoc = "FRONT BARRICADE";
                else if (hubScreen.IsInHallway && string.IsNullOrEmpty(hubScreen.CurrentInspectedRoom))
                    displayLoc = $"CORRIDOR // SEC {hubScreen.HallwaySection}";

                mapSubHeaderPosText.text = $"<color=#00e5ff><b>POS:</b> {displayLoc}</color>";
            }

            if (hubScreen.TacticalMap != null)
            {
                hubScreen.TacticalMap.SetCurrentLocation(locId);
                hubScreen.TacticalMap.RefreshAllNodes();
            }

            if (pinnedNoteText != null)
            {
                string pinned = "";
                if (hubScreen.Lore != null && hubScreen.Lore.Entries != null && hubScreen.Lore.Entries.Count > 0)
                {
                    var latest = hubScreen.Lore.Entries[hubScreen.Lore.Entries.Count - 1];
                    pinned = $"<b>{latest.title}:</b> {latest.content}";
                }
                else if (hubScreen.Objectives != null)
                {
                    pinned = hubScreen.Objectives.GetActiveObjectiveText();
                }

                if (string.IsNullOrEmpty(pinned))
                    pinned = "Search the coffee table & apartment for your keys before unlocking Unit 407.";

                pinnedNoteText.text = pinned;
            }
        }

        private void RefreshReaderTabsContent()
        {
            if (hubScreen == null) return;

            int logCount = hubScreen.StoryLogs != null ? hubScreen.StoryLogs.Count : 0;
            int notesCount = hubScreen.Lore != null && hubScreen.Lore.Entries != null ? hubScreen.Lore.Entries.Count : 0;
            var stash = hubScreen.GetCurrentFloorStashPublic();
            int floorCount = stash != null ? stash.Count : 0;

            if (readerTabLabels.Count >= 4)
            {
                readerTabLabels[0].text = "SITUATION";
                readerTabLabels[1].text = $"STORY LOG ({logCount})";
                readerTabLabels[2].text = $"NOTES ({notesCount})";
                readerTabLabels[3].text = floorCount > 0 && _activeTab != ReaderTab.Floor
                    ? $"FLOOR <color=#F59E0B><b>({floorCount})</b></color>"
                    : $"FLOOR ({floorCount})";
            }

            if (inlineStoryLogText != null && hubScreen.StoryLogs != null)
            {
                inlineStoryLogText.text = string.Join("\n\n<color=#334155>----------------------------------------------------</color>\n\n", hubScreen.StoryLogs);
                if (inlineStoryLogText.transform.parent is RectTransform slRt)
                {
                    inlineStoryLogText.ForceMeshUpdate();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(slRt);
                }
            }

            if (inlineNotesText != null)
            {
                var lines = new List<string>();
                if (hubScreen.Objectives != null && hubScreen.Objectives.Objectives != null)
                {
                    lines.Add("<b><color=#F59E0B>// ACTIVE OBJECTIVES</color></b>");
                    foreach (var obj in hubScreen.Objectives.Objectives)
                    {
                        if (obj.status == ObjectiveStatus.Locked) continue;
                        string mark = obj.status == ObjectiveStatus.Completed ? "<color=#22C55E>[DONE]</color>" : "<color=#38BDF8>[ACTIVE]</color>";
                        lines.Add($"{mark} {obj.description}");
                    }
                    lines.Add("");
                }

                lines.Add("<b><color=#F59E0B>// RECORDED NOTES & INTEL</color></b>");
                if (hubScreen.Lore != null && hubScreen.Lore.Entries != null && hubScreen.Lore.Entries.Count > 0)
                {
                    foreach (var entry in hubScreen.Lore.Entries)
                    {
                        lines.Add($"<b><color=#38BDF8>[{entry.timestamp}] {entry.title}</color></b>\n{entry.content}");
                    }
                }
                else
                {
                    lines.Add("<color=#64748B>No field notes recorded yet. Check your phone, radio, and apartment logs.</color>");
                }
                inlineNotesText.text = string.Join("\n\n", lines);
                if (inlineNotesText.transform.parent is RectTransform ntRt)
                {
                    inlineNotesText.ForceMeshUpdate();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ntRt);
                }
            }

            RefreshInlineFloorList(stash);

            if (utilPhoneBadge != null)
            {
                utilPhoneBadge.text = hubScreen.PhoneVisited ? "" : "NEW";
            }
        }

        private void RefreshInlineFloorList(List<InventoryEntry> stash)
        {
            if (inlineFloorListRoot == null) return;

            for (int i = inlineFloorListRoot.childCount - 1; i >= 0; i--)
            {
                Transform childTr = inlineFloorListRoot.GetChild(i);
                GameObject childGo = childTr.gameObject;
                childTr.SetParent(null, false);
                if (Application.isPlaying) Destroy(childGo);
                else DestroyImmediate(childGo);
            }

            bool hasLoot = stash != null && stash.Count > 0;
            if (inlineFloorEmptyText != null) inlineFloorEmptyText.gameObject.SetActive(!hasLoot);
            if (inlineTakeAllFloorBtn != null) inlineTakeAllFloorBtn.gameObject.SetActive(hasLoot);
            if (!hasLoot) return;

            foreach (var entry in stash)
            {
                if (entry == null) continue;
                GameObject row = new GameObject($"Floor_{entry.itemId}", typeof(RectTransform));
                row.transform.SetParent(inlineFloorListRoot, false);
                LayoutElement rLe = row.AddComponent<LayoutElement>();
                rLe.preferredHeight = 40f;
                rLe.minHeight = 40f;

                Image rBg = row.AddComponent<Image>();
                rBg.sprite = GetWhiteSprite();
                rBg.color = ColCardBg;
                Outline rOl = row.AddComponent<Outline>();
                rOl.effectColor = ColBorderSlate;

                GameObject icGo = new GameObject("Icon", typeof(RectTransform));
                icGo.transform.SetParent(row.transform, false);
                RectTransform icRt = icGo.GetComponent<RectTransform>();
                icRt.anchorMin = new Vector2(0f, 0.5f);
                icRt.anchorMax = new Vector2(0f, 0.5f);
                icRt.anchoredPosition = new Vector2(20f, 0f);
                icRt.sizeDelta = new Vector2(26f, 26f);
                Image icImg = icGo.AddComponent<Image>();
                icImg.sprite = ItemDatabase.GetItemIcon(entry.itemId);
                icImg.preserveAspect = true;
                icImg.enabled = icImg.sprite != null;

                TextMeshProUGUI lbl = CreateText(row.transform, "Name",
                    entry.quantity > 1 ? $"{entry.itemId} (x{entry.quantity})" : entry.itemId,
                    11.5f, ColTextMain, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Body, wrap: false);
                lbl.rectTransform.anchorMin = Vector2.zero;
                lbl.rectTransform.anchorMax = Vector2.one;
                lbl.rectTransform.offsetMin = new Vector2(44f, 0f);
                lbl.rectTransform.offsetMax = new Vector2(-95f, 0f);

                string capId = entry.itemId;
                Button pickBtn = CreateActionPill(row.transform, "Btn_Take", "TAKE",
                    new Color(0.11f, 0.24f, 0.19f, 1f), new Color(0.16f, 0.72f, 0.45f, 1f), out _, 10f);
                RectTransform pRt = pickBtn.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(1f, 0.5f);
                pRt.anchorMax = new Vector2(1f, 0.5f);
                pRt.pivot = new Vector2(1f, 0.5f);
                pRt.anchoredPosition = new Vector2(-8f, 0f);
                pRt.sizeDelta = new Vector2(76f, 24f);
                pickBtn.onClick.AddListener(() =>
                {
                    if (hubScreen != null && hubScreen.TakeSingleFloorOrTableItem(capId))
                    {
                        RefreshAll();
                    }
                });
            }

            Canvas.ForceUpdateCanvases();
            if (inlineFloorListRoot is RectTransform flRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(flRt);
            }
        }

        private void UpdateReaderAndActionDeckState()
        {
            if (hubScreen == null) return;

            bool isAwakening = hubScreen.AwakeningContainer != null && hubScreen.AwakeningContainer.activeSelf;
            if (awakeningHost != null) awakeningHost.gameObject.SetActive(isAwakening);
            if (situationScrollRect != null) situationScrollRect.gameObject.SetActive(!isAwakening);
            if (isAwakening) AdjustAwakeningLayout(hubScreen.AwakeningContainer);

            UpdateNarrativeActionPrompt(isAwakening);
        }

        private void UpdateNarrativeActionPrompt(bool isAwakening)
        {
            if (choicesViewport == null) return;

            if (narrativePromptCard == null)
            {
                Transform existing = choicesViewport.Find("NarrativeContinuePromptCard");
                if (existing != null)
                {
                    narrativePromptCard = existing.gameObject;
                }
                else
                {
                    narrativePromptCard = new GameObject("NarrativeContinuePromptCard", typeof(RectTransform));
                    narrativePromptCard.transform.SetParent(choicesViewport, false);
                    RectTransform rt = narrativePromptCard.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.04f, 0.22f);
                    rt.anchorMax = new Vector2(0.96f, 0.78f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    Image bg = narrativePromptCard.AddComponent<Image>();
                    bg.sprite = GetWhiteSprite();
                    bg.color = new Color(0.045f, 0.075f, 0.12f, 0.96f);

                    Outline ol = narrativePromptCard.AddComponent<Outline>();
                    ol.effectColor = new Color(0.0f, 0.85f, 1.0f, 0.85f);
                    ol.effectDistance = new Vector2(1.5f, -1.5f);

                    // Header
                    var hdr = CreateText(narrativePromptCard.transform, "PromptHeader",
                        "<color=#00e5ff><b>▶ INCOMING EMERGENCY TRANSMISSION</b></color>",
                        12.5f, ColCyan, TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Display, wrap: false);
                    RectTransform hRt = hdr.rectTransform;
                    hRt.anchorMin = new Vector2(0f, 0.60f);
                    hRt.anchorMax = new Vector2(1f, 0.95f);
                    hRt.offsetMin = Vector2.zero;
                    hRt.offsetMax = Vector2.zero;

                    // Subtitle instruction
                    var sub = CreateText(narrativePromptCard.transform, "PromptSub",
                        "<color=#94a3b8>Follow the live broadcast and telemetry readout in the narrative display.</color>",
                        10.5f, ColTextMuted, TextAlignmentOptions.Center, FontStyles.Normal, FontRole.Body, wrap: true);
                    RectTransform sRt = sub.rectTransform;
                    sRt.anchorMin = new Vector2(0.05f, 0.35f);
                    sRt.anchorMax = new Vector2(0.95f, 0.60f);
                    sRt.offsetMin = Vector2.zero;
                    sRt.offsetMax = Vector2.zero;

                    // Action Callout Button
                    GameObject actionBox = new GameObject("ActionCallout", typeof(RectTransform));
                    actionBox.transform.SetParent(narrativePromptCard.transform, false);
                    RectTransform abRt = actionBox.GetComponent<RectTransform>();
                    abRt.anchorMin = new Vector2(0.06f, 0.08f);
                    abRt.anchorMax = new Vector2(0.94f, 0.34f);
                    abRt.offsetMin = Vector2.zero;
                    abRt.offsetMax = Vector2.zero;

                    Image abBg = actionBox.AddComponent<Image>();
                    abBg.sprite = GetWhiteSprite();
                    abBg.color = new Color(0.08f, 0.14f, 0.22f, 0.95f);
                    abBg.raycastTarget = false;

                    Outline abOl = actionBox.AddComponent<Outline>();
                    abOl.effectColor = new Color(0.95f, 0.62f, 0.05f, 0.85f);
                    abOl.effectDistance = new Vector2(1f, -1f);

                    var actTmp = CreateText(actionBox.transform, "ActionText",
                        "<color=#f59e0b><b>[ SPACE / ENTER ]</b></color>  <color=#cbd5e1>OR</color>  <color=#00e5ff><b>CLICK HERE TO ADVANCE >></b></color>",
                        11.5f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                    StretchFull(actTmp.rectTransform);
                    actTmp.raycastTarget = false;
                }
            }

            if (narrativePromptCard != null)
            {
                if (narrativePromptBtn == null)
                {
                    narrativePromptBtn = narrativePromptCard.GetComponent<Button>();
                    if (narrativePromptBtn == null)
                    {
                        narrativePromptBtn = narrativePromptCard.AddComponent<Button>();
                    }
                }

                var img = narrativePromptCard.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    narrativePromptBtn.targetGraphic = img;
                    ConfigureButtonColors(narrativePromptBtn, img.color, new Color(0.08f, 0.14f, 0.22f, 1f));
                }

                narrativePromptBtn.onClick.RemoveAllListeners();
                narrativePromptBtn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    hubScreen.TriggerAdvanceAwakening();
                });

                Transform actionCallout = narrativePromptCard.transform.Find("ActionCallout");
                if (actionCallout != null)
                {
                    var acImg = actionCallout.GetComponent<Image>();
                    if (acImg != null) acImg.raycastTarget = true;
                    Button acBtn = actionCallout.GetComponent<Button>();
                    if (acBtn == null) acBtn = actionCallout.gameObject.AddComponent<Button>();
                    if (acImg != null) acBtn.targetGraphic = acImg;
                    acBtn.onClick.RemoveAllListeners();
                    acBtn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlayButtonClick();
                        hubScreen.TriggerAdvanceAwakening();
                    });
                }
            }

            narrativePromptCard.SetActive(isAwakening);
        }

        private void AdjustAwakeningLayout(GameObject awContainer)
        {
            if (awContainer == null) return;

            // ContinuePrompt
            Transform prTr = awContainer.transform.Find("ContinuePrompt");
            if (prTr != null)
            {
                RectTransform prRt = prTr.GetComponent<RectTransform>();
                prRt.anchorMin = new Vector2(1f, 0f);
                prRt.anchorMax = new Vector2(1f, 0f);
                prRt.pivot = new Vector2(1f, 0f);
                prRt.anchoredPosition = new Vector2(-10f, 6f);
                prRt.sizeDelta = new Vector2(220f, 22f);
                var prTmp = prTr.GetComponent<TextMeshProUGUI>();
                if (prTmp != null) prTmp.fontSize = 11.5f;
            }

            // Beat 1
            Transform b1Tr = awContainer.transform.Find("Beat1Container");
            if (b1Tr != null)
            {
                Transform b1TextTr = b1Tr.Find("Beat1IntroText");
                if (b1TextTr != null)
                {
                    RectTransform b1tRt = b1TextTr.GetComponent<RectTransform>();
                    b1tRt.anchorMin = new Vector2(0f, 1f);
                    b1tRt.anchorMax = new Vector2(1f, 1f);
                    b1tRt.pivot = new Vector2(0f, 1f);
                    b1tRt.anchoredPosition = new Vector2(8f, -4f);
                    b1tRt.sizeDelta = new Vector2(-16f, 44f);
                    var b1Tmp = b1TextTr.GetComponent<TextMeshProUGUI>();
                    if (b1Tmp != null)
                    {
                        b1Tmp.fontSize = 12.5f;
                        b1Tmp.lineSpacing = 1.5f;
                    }
                }

                Transform easTr = b1Tr.Find("EASAlertBox");
                if (easTr != null)
                {
                    RectTransform easRt = easTr.GetComponent<RectTransform>();
                    easRt.anchorMin = new Vector2(0f, 0f);
                    easRt.anchorMax = new Vector2(1f, 1f);
                    easRt.pivot = new Vector2(0.5f, 0.5f);
                    easRt.offsetMin = new Vector2(6f, 6f);
                    easRt.offsetMax = new Vector2(-6f, -50f);

                    Transform ehTr = easTr.Find("EASHeader");
                    if (ehTr != null)
                    {
                        RectTransform ehRt = ehTr.GetComponent<RectTransform>();
                        ehRt.anchorMin = new Vector2(0f, 1f);
                        ehRt.anchorMax = new Vector2(1f, 1f);
                        ehRt.pivot = new Vector2(0.5f, 1f);
                        ehRt.anchoredPosition = new Vector2(0f, -4f);
                        ehRt.sizeDelta = new Vector2(0f, 36f);
                        var ehTmp = ehTr.GetComponent<TextMeshProUGUI>();
                        if (ehTmp != null)
                        {
                            ehTmp.fontSize = 13.5f;
                            ehTmp.lineSpacing = 0f;
                        }
                    }

                    Transform divTr = easTr.Find("HeaderDiv");
                    if (divTr != null)
                    {
                        RectTransform dRt = divTr.GetComponent<RectTransform>();
                        dRt.anchorMin = new Vector2(0.04f, 1f);
                        dRt.anchorMax = new Vector2(0.96f, 1f);
                        dRt.pivot = new Vector2(0.5f, 1f);
                        dRt.anchoredPosition = new Vector2(0f, -42f);
                        dRt.sizeDelta = new Vector2(0f, 1.5f);
                    }

                    Transform etTr = easTr.Find("EASText");
                    if (etTr != null)
                    {
                        RectTransform etRt = etTr.GetComponent<RectTransform>();
                        etRt.anchorMin = new Vector2(0f, 0f);
                        etRt.anchorMax = new Vector2(1f, 1f);
                        etRt.offsetMin = new Vector2(16f, 16f);
                        etRt.offsetMax = new Vector2(-16f, -46f);
                        var etTmp = etTr.GetComponent<TextMeshProUGUI>();
                        if (etTmp != null)
                        {
                            etTmp.fontSize = 11.5f;
                            etTmp.lineSpacing = 1.5f;
                        }
                    }

                    Button easBtn = easTr.GetComponent<Button>();
                    if (easBtn == null) easBtn = easTr.gameObject.AddComponent<Button>();
                    easBtn.onClick.RemoveAllListeners();
                    easBtn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlayButtonClick();
                        hubScreen?.TriggerAdvanceAwakening();
                    });
                }
            }

            // Beat 2
            Transform b2Tr = awContainer.transform.Find("Beat2Container");
            if (b2Tr != null)
            {
                Transform b2tTr = b2Tr.Find("Beat2IntroText");
                if (b2tTr != null)
                {
                    RectTransform b2tRt = b2tTr.GetComponent<RectTransform>();
                    b2tRt.anchorMin = new Vector2(0f, 1f);
                    b2tRt.anchorMax = new Vector2(1f, 1f);
                    b2tRt.pivot = new Vector2(0f, 1f);
                    b2tRt.anchoredPosition = new Vector2(8f, -4f);
                    b2tRt.sizeDelta = new Vector2(-16f, 26f);
                    var b2Tmp = b2tTr.GetComponent<TextMeshProUGUI>();
                    if (b2Tmp != null) b2Tmp.fontSize = 12.5f;
                }

                Transform monoTr = b2Tr.Find("MonologueBox");
                if (monoTr != null)
                {
                    RectTransform monoRt = monoTr.GetComponent<RectTransform>();
                    monoRt.anchorMin = new Vector2(0f, 1f);
                    monoRt.anchorMax = new Vector2(1f, 1f);
                    monoRt.pivot = new Vector2(0.5f, 1f);
                    monoRt.anchoredPosition = new Vector2(0f, -32f);
                    monoRt.sizeDelta = new Vector2(-12f, 136f);

                    Transform spkTr = monoTr.Find("SpeechContainer/Speaker");
                    if (spkTr != null)
                    {
                        var sTmp = spkTr.GetComponent<TextMeshProUGUI>();
                        if (sTmp != null) sTmp.fontSize = 12f;
                    }
                    Transform speechTr = monoTr.Find("SpeechContainer/Speech");
                    if (speechTr != null)
                    {
                        var stTmp = speechTr.GetComponent<TextMeshProUGUI>();
                        if (stTmp != null)
                        {
                            stTmp.fontSize = 12f;
                            stTmp.lineSpacing = 2.5f;
                        }
                    }

                    Button monoBtn = monoTr.GetComponent<Button>();
                    if (monoBtn == null) monoBtn = monoTr.gameObject.AddComponent<Button>();
                    monoBtn.onClick.RemoveAllListeners();
                    monoBtn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlayButtonClick();
                        hubScreen?.TriggerAdvanceAwakening();
                    });
                }

                Transform b2fTr = b2Tr.Find("Beat2FootnoteText");
                if (b2fTr != null)
                {
                    RectTransform b2fRt = b2fTr.GetComponent<RectTransform>();
                    b2fRt.anchorMin = new Vector2(0f, 1f);
                    b2fRt.anchorMax = new Vector2(1f, 1f);
                    b2fRt.pivot = new Vector2(0f, 1f);
                    b2fRt.anchoredPosition = new Vector2(8f, -174f);
                    b2fRt.sizeDelta = new Vector2(-16f, 75f);
                    var b2fTmp = b2fTr.GetComponent<TextMeshProUGUI>();
                    if (b2fTmp != null)
                    {
                        b2fTmp.fontSize = 12f;
                        b2fTmp.lineSpacing = 2.5f;
                    }
                }
            }

            // Beat 3
            Transform b3Tr = awContainer.transform.Find("Beat3Container");
            if (b3Tr != null)
            {
                Transform badgeTr = b3Tr.Find("RoomBadge");
                if (badgeTr != null)
                {
                    RectTransform bRt = badgeTr.GetComponent<RectTransform>();
                    bRt.anchorMin = new Vector2(0f, 1f);
                    bRt.anchorMax = new Vector2(0f, 1f);
                    bRt.pivot = new Vector2(0f, 1f);
                    bRt.anchoredPosition = new Vector2(8f, -4f);
                    bRt.sizeDelta = new Vector2(110f, 22f);
                }

                Transform b3tTr = b3Tr.Find("Beat3NarrativeText");
                if (b3tTr != null)
                {
                    RectTransform b3tRt = b3tTr.GetComponent<RectTransform>();
                    b3tRt.anchorMin = new Vector2(0f, 1f);
                    b3tRt.anchorMax = new Vector2(1f, 1f);
                    b3tRt.pivot = new Vector2(0f, 1f);
                    b3tRt.anchoredPosition = new Vector2(8f, -28f);
                    b3tRt.sizeDelta = new Vector2(-16f, 126f);
                    var b3Tmp = b3tTr.GetComponent<TextMeshProUGUI>();
                    if (b3Tmp != null)
                    {
                        b3Tmp.fontSize = 12f;
                        b3Tmp.lineSpacing = 2f;
                    }
                }

                Transform stashTr = b3Tr.Find("TableStashDisplay");
                if (stashTr != null)
                {
                    RectTransform sRt = stashTr.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0f, 0f);
                    sRt.anchorMax = new Vector2(1f, 0f);
                    sRt.pivot = new Vector2(0.5f, 0f);
                    sRt.anchoredPosition = new Vector2(0f, 6f);
                    sRt.sizeDelta = new Vector2(-12f, 155f);
                }
            }
        }

        private void UpdatePortraitAnimation()
        {
            if (!Application.isPlaying || survivorPortraitImg == null) return;

            if (_avatarFrames == null)
            {
                _avatarFrames = CharacterCreationScreen.LoadPlayerFrames();
            }

            if (_avatarFrames == null || _avatarFrames.Count == 0) return;

            _avatarTimer += Time.unscaledDeltaTime;
            if (_avatarTimer >= 0.14f)
            {
                _avatarTimer = 0f;
                _avatarFrameIndex = (_avatarFrameIndex + 1) % _avatarFrames.Count;
                survivorPortraitImg.sprite = _avatarFrames[_avatarFrameIndex];
            }
        }
        #endregion

        #region UI Factory Helpers
        private static Sprite GetWhiteSprite()
        {
            return null;
        }

        private Sprite FindExistingVitalIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            string mapChild = iconName switch
            {
                "hp_icon" => "Vital_HP",
                "stamina_icon" => "Vital_Stamina",
                "hunger_icon" => "Vital_Hunger",
                "thirst_icon" => "Vital_Thirst",
                _ => null
            };

            if (!string.IsNullOrEmpty(mapChild))
            {
                Transform row = transform.Find($"TopHUD/VitalsRow/{mapChild}");
                if (row != null)
                {
                    foreach (var img in row.GetComponentsInChildren<Image>(true))
                    {
                        if (img != null && img.sprite != null && img.sprite.name.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return img.sprite;
                        }
                    }
                }
            }

            return LoadSpriteFallback($"Assets/Sprites/UI/{iconName}.png");
        }

        private static Sprite LoadSpriteFallback(string assetPath)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s != null) return s;
#endif
            return null;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Color color,
            TextAlignmentOptions alignment, FontStyles style, FontRole role, bool wrap)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            SundayTypography.ApplyFont(tmp, role);
            return tmp;
        }

        private static Button CreateActionPill(Transform parent, string name, string label, Color bgColor, Color borderCol,
            out TextMeshProUGUI labelTmp, float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image bg = go.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = bgColor;
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = borderCol;
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ConfigureButtonColors(btn, bgColor, bgColor * 1.35f);

            labelTmp = CreateText(go.transform, "Label", label, fontSize, Color.white,
                TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Display, wrap: false);
            StretchFull(labelTmp.rectTransform, 4f, 0f, 4f, 0f);
            return btn;
        }

        private static ScrollRect CreateScrollArea(Transform parent, string name, out RectTransform content)
        {
            GameObject scGo = new GameObject(name, typeof(RectTransform));
            scGo.transform.SetParent(parent, false);
            StretchFull(scGo.GetComponent<RectTransform>());

            ScrollRect sr = scGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 28f;

            GameObject vp = new GameObject("Viewport", typeof(RectTransform));
            vp.transform.SetParent(scGo.transform, false);
            RectTransform vpRt = vp.GetComponent<RectTransform>();
            StretchFull(vpRt);
            vp.AddComponent<RectMask2D>();
            Image vpImg = vp.AddComponent<Image>();
            vpImg.color = new Color(0f, 0f, 0f, 0.005f);
            sr.viewport = vpRt;

            GameObject cnt = new GameObject("Content", typeof(RectTransform));
            cnt.transform.SetParent(vp.transform, false);
            content = cnt.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            ContentSizeFitter csf = cnt.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = content;
            return sr;
        }

        private static void ConfigureButtonColors(Button btn, Color normal, Color hover)
        {
            if (btn == null) return;
            ColorBlock cb = btn.colors;
            cb.normalColor = normal;
            cb.highlightedColor = hover;
            cb.pressedColor = normal * 0.8f;
            cb.selectedColor = normal;
            cb.disabledColor = normal * 0.45f;
            btn.colors = cb;
        }

        private static string ShortLabel(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            return id.Length <= 6 ? id.ToUpperInvariant() : id.Substring(0, 6).ToUpperInvariant();
        }
        #endregion
    }
}
