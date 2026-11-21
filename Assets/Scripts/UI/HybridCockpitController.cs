using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

        [Header("Style D Hardware Theme (Revamped StageScreen)")]
        [SerializeField] private bool useStyleDHardwareTheme = false;
        [SerializeField] private Sprite styleDPanelChassisSprite;
        [SerializeField] private Sprite styleDCrtBezelSprite;
        [SerializeField] private Sprite styleDWellRecessedSprite;
        [SerializeField] private Sprite styleDSlotRecessedSprite;
        [SerializeField] private Sprite styleDStatCardSprite;
        [SerializeField] private Sprite styleDButtonMetalSprite;
        [SerializeField] private Sprite styleDMapPaperSprite;
        [SerializeField] private Sprite styleDPinnedNoteSprite;
        [SerializeField] private Sprite styleDRedPushpinSprite;
        public bool IsStyleDHardwareTheme => useStyleDHardwareTheme;

        private EcgWaveformGraphic _styleDEcg;
        private TextMeshProUGUI _styleDConditionText;
        private readonly Dictionary<string, TextMeshProUGUI> _styleDMortalValueTexts = new Dictionary<string, TextMeshProUGUI>(StringComparer.OrdinalIgnoreCase);
        private GameObject _styleDMortalGridObj;
        private GameObject _styleDCrtOverlayObj;

        private ReaderTab _activeTab = ReaderTab.Situation;
        private SurvivorSubTab _activeSurvivorTab = SurvivorSubTab.Mortal;
        private string _selectedItemId = "";
        private bool _selectedIsEquipped;
        private List<Sprite> _avatarFrames;
        private int _avatarFrameIndex;
        private float _avatarTimer;

        // Option A: Visual Hierarchy & Passive Panel Dimming State
        private CanvasGroup _cgInventory;
        private CanvasGroup _cgSurvivor;
        private CanvasGroup _cgSurvivorTabs;
        private CanvasGroup _cgMap;
        private bool _hoverInventory;
        private bool _hoverSurvivor;
        private bool _hoverMap;

        // Option B: Story Reader Font Scale & Story Log Scroll Lock
        private Button _fontDecBtn;
        private Button _fontIncBtn;
        private Button _fontResetBtn;
        private TextMeshProUGUI _fontScaleLabel;
        private Button _jumpToLatestLogBtn;
        private TextMeshProUGUI _jumpToLatestLogLabel;
        private int _lastRenderedStoryLogCount = -1;
        private float _lastRenderedFontScale = -1f;
        private bool _hasUnreadLogWhileScrolled;
        private bool _storyLogScrolledBack;

        // Palette constants matching blueprint_v4_hybrid_1_and_3.png
        private static readonly Color ColBgOuter = new Color(0.03f, 0.04f, 0.06f, 1f);
        private static readonly Color ColPanelBg = new Color(0.063f, 0.082f, 0.118f, 0.98f);
        private static readonly Color ColHeaderBg = new Color(0.094f, 0.125f, 0.173f, 1f);
        private static readonly Color ColBorderSlate = new Color(0.18f, 0.23f, 0.305f, 0.95f);
        private static readonly Color ColBorderActive = new Color(0.22f, 0.34f, 0.48f, 0.88f);
        private static readonly Color ColBorderPassive = new Color(0.12f, 0.16f, 0.22f, 0.38f);
        private static readonly Color ColBorderSubtle = new Color(0.13f, 0.17f, 0.23f, 0.25f);
        private static readonly Color ColHeaderTitlePassive = new Color(0.62f, 0.54f, 0.38f, 0.80f);
        private static readonly Color ColSlotBg = new Color(0.055f, 0.075f, 0.105f, 0.96f);
        private static readonly Color ColCardBg = new Color(0.075f, 0.10f, 0.145f, 0.96f);
        private static readonly Color ColAmber = new Color(0.96f, 0.62f, 0.043f, 1f);
        private static readonly Color ColCyan = new Color(0.22f, 0.74f, 0.97f, 1f);
        private static readonly Color ColTextMain = new Color(0.90f, 0.925f, 0.96f, 1f);
        private static readonly Color ColTextMuted = new Color(0.55f, 0.62f, 0.72f, 1f);

        // Style D Palette: 90s Gritty Gunmetal Hardware & Aged Paper
        private static readonly Color ColStyleDBgOuter = new Color(0.035f, 0.039f, 0.047f, 1f);
        private static readonly Color ColStyleDHeaderBg = new Color(0.055f, 0.062f, 0.072f, 0.55f);
        private static readonly Color ColStyleDHeaderLine = new Color(0.17f, 0.19f, 0.21f, 0.90f);
        private static readonly Color ColStyleDBoneText = new Color(0.894f, 0.871f, 0.788f, 1f);
        private static readonly Color ColStyleDMutedStone = new Color(0.604f, 0.584f, 0.541f, 1f);
        private static readonly Color ColStyleDTitlePassive = new Color(0.659f, 0.635f, 0.584f, 0.92f);
        private static readonly Color ColStyleDTitleActive = new Color(0.941f, 0.918f, 0.878f, 1f);
        private static readonly Color ColStyleDRust = new Color(0.68f, 0.29f, 0.19f, 1f);
        private static readonly Color ColStyleDRustBright = new Color(0.788f, 0.290f, 0.161f, 1f);
        private static readonly Color ColStyleDAmber = new Color(0.812f, 0.573f, 0.196f, 1f);
        private static readonly Color ColStyleDEquipBtn = new Color(0.34f, 0.44f, 0.37f, 1f);
        private static readonly Color ColStyleDDropBtn = new Color(0.68f, 0.27f, 0.24f, 1f);
        private static readonly Color ColStyleDTabInactive = new Color(0.24f, 0.26f, 0.29f, 1f);
        private static readonly Color ColStyleDMetalBtn = new Color(0.32f, 0.35f, 0.39f, 1f);
        private static readonly Color ColStyleDPaperInk = new Color(0.08f, 0.07f, 0.06f, 1f);
        private static readonly Color ColStyleDPaperHeader = new Color(0.478f, 0.141f, 0.102f, 1f);

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
            QuietInnerPanelBorders();
            EnsureReaderFontControlsAndScrollLockUI();
            HookInteractions();
            if (useStyleDHardwareTheme)
            {
                ApplyStyleDHardwareTheme();
            }
            OnConsoleModeChanged(GameStageScreen.ConsoleMode.Narrative);
            SelectReaderTab(_activeTab, force: true);
            SelectSurvivorTab(_activeSurvivorTab, force: true);
            RefreshAll();
            ApplyCockpitVisualHierarchy(immediate: true);
        }

        private void OnDestroy()
        {
            if (useStyleDHardwareTheme)
            {
                TacticalChoiceButton.StyleDEnabled = false;
                TacticalMapViewController.StyleDThemeEnabled = false;
            }
        }

        public void RefreshNarrativeTypography()
        {
            UpdateFontControlUI();
            RefreshReaderTabsContent();
            if (hubScreen != null && hubScreen.AwakeningContainer != null && hubScreen.AwakeningContainer.activeSelf)
            {
                AdjustAwakeningLayout(hubScreen.AwakeningContainer);
            }
        }

        private void EnsureReaderFontControlsAndScrollLockUI()
        {
            if (bottomCenterReaderPanel != null)
            {
                Transform tabBar = bottomCenterReaderPanel.Find("TabBar") ?? bottomCenterReaderPanel.Find("Body/TabBar");
                if (tabBar != null)
                {
                    HorizontalLayoutGroup hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.padding = new RectOffset(hlg.padding.left, 114, hlg.padding.top, hlg.padding.bottom);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(tabBar as RectTransform);
                    }

                    Transform groupTr = tabBar.Find("FontSizeControlGroup");
                    if (groupTr == null)
                    {
                        GameObject groupGo = new GameObject("FontSizeControlGroup", typeof(RectTransform), typeof(LayoutElement));
                        groupGo.transform.SetParent(tabBar, false);
                        groupTr = groupGo.transform;

                        LayoutElement le = groupGo.GetComponent<LayoutElement>();
                        le.ignoreLayout = true;

                        RectTransform gRt = groupGo.GetComponent<RectTransform>();
                        gRt.anchorMin = new Vector2(1f, 0.5f);
                        gRt.anchorMax = new Vector2(1f, 0.5f);
                        gRt.pivot = new Vector2(1f, 0.5f);
                        gRt.sizeDelta = new Vector2(102f, 26f);
                        gRt.anchoredPosition = new Vector2(-6f, 0f);

                        _fontDecBtn = CreateReaderFontControlPill(groupTr, "Btn_FontDec", "A-", 0f, 26f, ColTextMain);
                        _fontResetBtn = CreateReaderFontControlPill(groupTr, "Btn_FontReset", "100%", 28f, 46f, ColAmber);
                        _fontIncBtn = CreateReaderFontControlPill(groupTr, "Btn_FontInc", "A+", 76f, 26f, ColTextMain);
                    }
                    else
                    {
                        _fontDecBtn = groupTr.Find("Btn_FontDec")?.GetComponent<Button>();
                        _fontResetBtn = groupTr.Find("Btn_FontReset")?.GetComponent<Button>();
                        _fontIncBtn = groupTr.Find("Btn_FontInc")?.GetComponent<Button>();
                    }

                    if (_fontResetBtn != null)
                    {
                        _fontScaleLabel = _fontResetBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                    }

                    if (_fontDecBtn != null)
                    {
                        _fontDecBtn.onClick.RemoveAllListeners();
                        _fontDecBtn.onClick.AddListener(() =>
                        {
                            AudioManager.Instance?.PlayButtonClick();
                            SundayTypography.StepNarrativeFontScale(-1);
                        });
                    }
                    if (_fontResetBtn != null)
                    {
                        _fontResetBtn.onClick.RemoveAllListeners();
                        _fontResetBtn.onClick.AddListener(() =>
                        {
                            AudioManager.Instance?.PlayButtonClick();
                            SundayTypography.CycleNarrativeFontScale();
                        });
                    }
                    if (_fontIncBtn != null)
                    {
                        _fontIncBtn.onClick.RemoveAllListeners();
                        _fontIncBtn.onClick.AddListener(() =>
                        {
                            AudioManager.Instance?.PlayButtonClick();
                            SundayTypography.StepNarrativeFontScale(1);
                        });
                    }
                }
            }

            Transform storyLogViewTr = readerTabStoryLogView != null
                ? readerTabStoryLogView.transform
                : (bottomCenterReaderPanel != null ? bottomCenterReaderPanel.Find("ReaderBody/View_StoryLog") : null);
            if (storyLogViewTr != null)
            {
                Transform jumpTr = storyLogViewTr.Find("Btn_JumpToLatestLog");
                if (jumpTr == null)
                {
                    _jumpToLatestLogBtn = CreateActionPill(
                        storyLogViewTr,
                        "Btn_JumpToLatestLog",
                        "[DN] JUMP TO LATEST ENTRY",
                        new Color(0.08f, 0.11f, 0.17f, 0.96f),
                        ColAmber,
                        out _jumpToLatestLogLabel,
                        9.5f);
                    RectTransform jRt = _jumpToLatestLogBtn.GetComponent<RectTransform>();
                    jRt.anchorMin = new Vector2(1f, 0f);
                    jRt.anchorMax = new Vector2(1f, 0f);
                    jRt.pivot = new Vector2(1f, 0f);
                    jRt.anchoredPosition = new Vector2(-14f, 10f);
                    jRt.sizeDelta = new Vector2(210f, 26f);
                    if (_jumpToLatestLogLabel != null)
                    {
                        _jumpToLatestLogLabel.color = ColAmber;
                        SundayTypography.ApplyFont(_jumpToLatestLogLabel, FontRole.Mono);
                    }
                    _jumpToLatestLogBtn.gameObject.SetActive(false);
                }
                else
                {
                    _jumpToLatestLogBtn = jumpTr.GetComponent<Button>();
                    _jumpToLatestLogLabel = jumpTr.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                if (_jumpToLatestLogBtn != null)
                {
                    _jumpToLatestLogBtn.transform.SetAsLastSibling();
                    _jumpToLatestLogBtn.onClick.RemoveAllListeners();
                    _jumpToLatestLogBtn.onClick.AddListener(JumpToLatestStoryLog);
                }
            }

            if (inlineStoryLogScroll != null)
            {
                if (inlineStoryLogScroll.content != null)
                {
                    var vlg = inlineStoryLogScroll.content.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null && vlg.padding.bottom < 36)
                    {
                        vlg.padding = new RectOffset(vlg.padding.left, vlg.padding.right, vlg.padding.top, 36);
                    }
                }
                inlineStoryLogScroll.onValueChanged.RemoveListener(OnStoryLogScrollValueChanged);
                inlineStoryLogScroll.onValueChanged.AddListener(OnStoryLogScrollValueChanged);
            }

            UpdateFontControlUI();
            UpdateStoryLogScrollLockState();
        }

        private static Button CreateReaderFontControlPill(Transform parent, string name, string label, float leftX, float width, Color textColor)
        {
            Button btn = CreateActionPill(parent, name, label, ColSlotBg, ColBorderPassive, out TextMeshProUGUI lbl, 9.5f);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(leftX, 0f);
            rt.sizeDelta = new Vector2(width, 24f);
            if (lbl != null)
            {
                lbl.color = textColor;
                SundayTypography.ApplyFont(lbl, FontRole.Mono);
            }
            return btn;
        }

        private void UpdateFontControlUI()
        {
            if (_fontScaleLabel != null)
            {
                _fontScaleLabel.text = $"{SundayTypography.NarrativeFontScalePercent}%";
                if (useStyleDHardwareTheme)
                {
                    _fontScaleLabel.color = Mathf.Approximately(SundayTypography.NarrativeFontScale, 1.0f) ? ColStyleDAmber : ColStyleDBoneText;
                }
                else
                {
                    _fontScaleLabel.color = Mathf.Approximately(SundayTypography.NarrativeFontScale, 1.0f) ? ColAmber : ColCyan;
                }
            }
            if (_fontDecBtn != null)
            {
                _fontDecBtn.interactable = SundayTypography.NarrativeFontScale > 0.91f;
            }
            if (_fontIncBtn != null)
            {
                _fontIncBtn.interactable = SundayTypography.NarrativeFontScale < 1.29f;
            }
        }

        private void OnStoryLogScrollValueChanged(Vector2 _)
        {
            UpdateStoryLogScrollLockState();
        }

        private void UpdateStoryLogScrollLockState()
        {
            if (inlineStoryLogScroll == null)
            {
                _storyLogScrolledBack = false;
                _hasUnreadLogWhileScrolled = false;
                if (_jumpToLatestLogBtn != null) _jumpToLatestLogBtn.gameObject.SetActive(false);
                return;
            }

            RectTransform contentRt = inlineStoryLogScroll.content;
            RectTransform vpRt = inlineStoryLogScroll.viewport;
            bool scrollable = contentRt != null && vpRt != null && contentRt.rect.height > vpRt.rect.height + 8f;
            bool scrolledBack = scrollable && inlineStoryLogScroll.verticalNormalizedPosition > 0.05f;

            _storyLogScrolledBack = scrolledBack;
            if (!scrolledBack)
            {
                _hasUnreadLogWhileScrolled = false;
            }

            if (_jumpToLatestLogBtn != null)
            {
                _jumpToLatestLogBtn.gameObject.SetActive(scrolledBack);
                if (scrolledBack && _jumpToLatestLogLabel != null)
                {
                    _jumpToLatestLogLabel.text = SundayTypography.SanitizeText(
                        _hasUnreadLogWhileScrolled ? "[DN] NEW ENTRY // JUMP TO LATEST" : "[DN] JUMP TO LATEST ENTRY");
                }
            }
        }

        public void JumpToLatestStoryLog()
        {
            AudioManager.Instance?.PlayButtonClick();
            _hasUnreadLogWhileScrolled = false;
            _storyLogScrolledBack = false;
            if (inlineStoryLogScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                inlineStoryLogScroll.velocity = Vector2.zero;
                inlineStoryLogScroll.verticalNormalizedPosition = 0f;
            }
            UpdateStoryLogScrollLockState();
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

            bool overlayOpen = mode == GameStageScreen.ConsoleMode.PDA;
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
                hubScreen.DockContainerGear.SetActive(false);
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
            ApplyCockpitVisualHierarchy(immediate: true);
        }

        private void Awake()
        {
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();
            SundayTypography.StyleDThemeEnabled = useStyleDHardwareTheme;
            if (useStyleDHardwareTheme)
            {
                TacticalChoiceButton.StyleDEnabled = true;
                TacticalMapViewController.StyleDThemeEnabled = true;
            }
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
            ApplyCockpitVisualHierarchy(immediate: false);

            if (Application.isPlaying)
            {
                HandleHybridHotkeys();
            }
        }

        private static CanvasGroup EnsureCanvasGroup(Transform t)
        {
            if (t == null) return null;
            CanvasGroup cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            return cg;
        }

        private static void SetOutlineColor(Transform t, Color c)
        {
            if (t == null) return;
            Outline ol = t.GetComponent<Outline>();
            if (ol != null) ol.effectColor = c;
        }

        private static void ApplyAlpha(CanvasGroup cg, float target, bool immediate)
        {
            if (cg == null) return;
            cg.alpha = immediate ? target : Mathf.MoveTowards(cg.alpha, target, Time.unscaledDeltaTime * 6.5f);
        }

        private static void StylePassivePanelChrome(RectTransform panel, bool hovered, bool styleD = false)
        {
            if (panel == null) return;
            Color borderColor = hovered ? ColBorderActive : ColBorderPassive;
            Color titleColor = styleD
                ? (hovered ? ColStyleDTitleActive : ColStyleDTitlePassive)
                : (hovered ? ColAmber : ColHeaderTitlePassive);

            if (!styleD)
            {
                SetOutlineColor(panel, borderColor);
            }

            Transform bottomLine = panel.Find("Header/BottomLine");
            if (bottomLine != null)
            {
                Image blImg = bottomLine.GetComponent<Image>();
                if (blImg != null) blImg.color = styleD ? ColStyleDHeaderLine : borderColor;
            }

            Transform titleTr = panel.Find("Header/Title");
            if (titleTr != null)
            {
                TextMeshProUGUI titleTmp = titleTr.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null) titleTmp.color = titleColor;
            }
        }

        public void ApplyCockpitVisualHierarchy(bool immediate = false)
        {
            if (topLeftInventoryPanel != null && _cgInventory == null)
                _cgInventory = EnsureCanvasGroup(topLeftInventoryPanel);

            if (topRightSurvivorPanel != null)
            {
                if (_cgSurvivor == null) _cgSurvivor = EnsureCanvasGroup(topRightSurvivorPanel);
                if (_cgSurvivorTabs == null)
                    _cgSurvivorTabs = EnsureCanvasGroup(topRightSurvivorPanel.Find("Body/SurvivorTabsContainer"));
            }

            if (bottomLeftMapPanel != null && _cgMap == null)
                _cgMap = EnsureCanvasGroup(bottomLeftMapPanel);

            bool inCombat = hubScreen != null && hubScreen.IsCombatModalOpen;

            // Keep primary focal panels at 100% alpha and active border brightness (hide bottom StageScreen reader/deck cleanly behind combat panels)
            Transform topStage = transform.Find("TopScreen_VisualStage");
            if (topStage != null)
            {
                CanvasGroup tsCg = topStage.GetComponent<CanvasGroup>();
                if (tsCg != null) tsCg.alpha = 1f;
                if (!useStyleDHardwareTheme) SetOutlineColor(topStage, ColBorderActive);
            }
            if (bottomCenterReaderPanel != null)
            {
                CanvasGroup bcCg = EnsureCanvasGroup(bottomCenterReaderPanel);
                bcCg.alpha = inCombat ? 0f : 1f;
                bcCg.interactable = !inCombat;
                bcCg.blocksRaycasts = !inCombat;
                if (!useStyleDHardwareTheme) SetOutlineColor(bottomCenterReaderPanel, ColBorderActive);
            }
            if (bottomRightActionPanel != null)
            {
                CanvasGroup brCg = EnsureCanvasGroup(bottomRightActionPanel);
                brCg.alpha = inCombat ? 0f : 1f;
                brCg.interactable = !inCombat;
                brCg.blocksRaycasts = !inCombat;
                if (!useStyleDHardwareTheme) SetOutlineColor(bottomRightActionPanel, ColBorderActive);
            }

            float baseInv = useStyleDHardwareTheme ? 0.78f : 0.62f;
            float baseSurv = useStyleDHardwareTheme ? 0.80f : 0.65f;
            float baseSurvTabs = useStyleDHardwareTheme ? 0.85f : 0.72f;
            float baseMap = useStyleDHardwareTheme ? 0.78f : 0.60f;

            float invAlpha = inCombat ? (_hoverInventory ? 1.0f : 0.38f) : (_hoverInventory ? 1.0f : baseInv);
            float survAlpha = inCombat ? (_hoverSurvivor ? 1.0f : 0.52f) : (_hoverSurvivor ? 1.0f : baseSurv);
            float survTabsAlpha = inCombat ? (_hoverSurvivor ? 1.0f : 0.35f) : (_hoverSurvivor ? 1.0f : baseSurvTabs);
            float mapAlpha = inCombat ? (_hoverMap ? 0.95f : 0.28f) : (_hoverMap ? 1.0f : baseMap);

            ApplyAlpha(_cgInventory, invAlpha, immediate);
            ApplyAlpha(_cgSurvivor, survAlpha, immediate);
            ApplyAlpha(_cgSurvivorTabs, survTabsAlpha, immediate);
            ApplyAlpha(_cgMap, mapAlpha, immediate);

            StylePassivePanelChrome(topLeftInventoryPanel, _hoverInventory, useStyleDHardwareTheme);
            StylePassivePanelChrome(topRightSurvivorPanel, _hoverSurvivor, useStyleDHardwareTheme);
            StylePassivePanelChrome(bottomLeftMapPanel, _hoverMap, useStyleDHardwareTheme);
        }

        private void QuietInnerPanelBorders()
        {
            if (topLeftInventoryPanel != null)
            {
                Transform micro = topLeftInventoryPanel.Find("Body/MicroInspector");
                SetOutlineColor(micro, ColBorderSubtle);
                if (micro != null) SetOutlineColor(micro.Find("IconFrame"), ColBorderSubtle);
            }

            if (topRightSurvivorPanel != null)
            {
                Transform idCard = topRightSurvivorPanel.Find("Body/IdentityCard");
                SetOutlineColor(idCard, ColBorderSubtle);
                if (idCard != null)
                {
                    SetOutlineColor(idCard.Find("PortraitFrame"), new Color(0.18f, 0.52f, 0.68f, 0.45f));
                }

                Transform tabs = topRightSurvivorPanel.Find("Body/SurvivorTabsContainer");
                SetOutlineColor(tabs, ColBorderSubtle);
                if (tabs != null)
                {
                    foreach (var ol in tabs.GetComponentsInChildren<Outline>(true))
                    {
                        if (ol == null) continue;
                        string objName = ol.gameObject.name;
                        if (objName.StartsWith("StatRow_", StringComparison.Ordinal) || objName == "PartyGroupCard")
                        {
                            ol.effectColor = ColBorderSubtle;
                        }
                    }
                }
            }

            if (bottomLeftMapPanel != null)
            {
                SetOutlineColor(bottomLeftMapPanel.Find("Body/MiniMapCanvas"), ColBorderSubtle);
                SetOutlineColor(bottomLeftMapPanel.Find("Body/PinnedNoteCard"), new Color(0.38f, 0.28f, 0.12f, 0.40f));
            }

            SetOutlineColor(transform.Find("TopScreen_VisualStage"), ColBorderActive);
            SetOutlineColor(bottomCenterReaderPanel, ColBorderActive);
            SetOutlineColor(bottomRightActionPanel, ColBorderActive);
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
            QuietInnerPanelBorders();

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

            inspectorOpenGearBtn = CreateActionPill(inspGo.transform, "Btn_OpenFullGear", "DROP",
                new Color(0.22f, 0.11f, 0.11f, 1f), new Color(0.72f, 0.26f, 0.24f, 0.9f), out TextMeshProUGUI dropBtnLbl, 10f);
            if (dropBtnLbl != null) dropBtnLbl.color = new Color(0.96f, 0.78f, 0.76f, 1f);
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
                traitsCountHeaderText.text = useStyleDHardwareTheme
                    ? $"PERKS & DRAWBACKS ({traitCount})"
                    : $"// PERKS & DRAWBACKS ({traitCount})";
                if (useStyleDHardwareTheme) traitsCountHeaderText.color = ColStyleDMutedStone;
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

                if (useStyleDHardwareTheme && styleDStatCardSprite != null)
                {
                    Color cardTint = isDrawback
                        ? new Color(0.95f, 0.72f, 0.70f, 1f)
                        : new Color(0.76f, 0.92f, 0.80f, 1f);
                    ApplySlicedSprite(rBg, styleDStatCardSprite, 3.2f, cardTint, disableOutline: false);
                    rOl.enabled = false;
                }

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

            EnsureReaderFontControlsAndScrollLockUI();
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
                tsOl.effectColor = ColBorderActive;
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
                                    apTmp.alignment = useStyleDHardwareTheme ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
                                    apTmp.fontSize = useStyleDHardwareTheme ? 8.2f : 8.5f;
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
                float miniScale = useStyleDHardwareTheme ? 0.88f : MiniMapBaseScale;
                hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, miniScale);
            }

            // Enforce clean simplified legend on the blueprint mini-map
            Transform bpLegend = bottomLeftMapPanel != null ? bottomLeftMapPanel.Find("Body/MiniMapCanvas/BlueprintViewport/LegendBar/LegendText") : null;
            if (bpLegend != null)
            {
                var lTmp = bpLegend.GetComponent<TextMeshProUGUI>();
                if (lTmp != null)
                {
                    lTmp.text = useStyleDHardwareTheme
                        ? "<color=#D95829>■</color> CURRENT  <color=#8C8578>■</color> VISITED  <color=#2B2823>■</color> UNVISITED  <color=#C94A29>■</color> LOCKED  <color=#CF9232>■</color> BLOCKED"
                        : "<color=#00c7f2>■</color> CURRENT  <color=#94a3b8>■</color> VISITED  <color=#0a0f16>■</color> UNVISITED  <color=#ea2e38>■</color> LOCKED  <color=#f59e0b>■</color> BLOCKED";
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
                Transform tr = topLeftInventoryPanel.Find("Body/MicroInspector/Btn_OpenFullGear") ??
                               topLeftInventoryPanel.Find("Body/Inspector/Btn_OpenFullGear");
                if (tr != null) inspectorOpenGearBtn = tr.GetComponent<Button>();
            }

            if (inspectorOpenGearBtn != null)
            {
                Color dropBg = new Color(0.22f, 0.11f, 0.11f, 1f);
                Color dropHover = new Color(0.32f, 0.15f, 0.15f, 1f);
                Image dropImg = inspectorOpenGearBtn.GetComponent<Image>();
                if (dropImg != null) dropImg.color = dropBg;
                ConfigureButtonColors(inspectorOpenGearBtn, dropBg, dropHover);

                Outline dropOl = inspectorOpenGearBtn.GetComponent<Outline>();
                if (dropOl != null) dropOl.effectColor = new Color(0.72f, 0.26f, 0.24f, 0.9f);

                TextMeshProUGUI dropLbl = inspectorOpenGearBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (dropLbl != null)
                {
                    dropLbl.text = "DROP";
                    dropLbl.color = new Color(0.96f, 0.78f, 0.76f, 1f);
                }

                inspectorOpenGearBtn.onClick.RemoveAllListeners();
                inspectorOpenGearBtn.onClick.AddListener(OnInspectorDropClicked);
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

            BindPanelHoverTrigger(topLeftInventoryPanel, h => _hoverInventory = h);
            BindPanelHoverTrigger(topRightSurvivorPanel, h => _hoverSurvivor = h);
            BindPanelHoverTrigger(bottomLeftMapPanel, h => _hoverMap = h);
        }

        private static void BindPanelHoverTrigger(RectTransform panel, Action<bool> onHoverChanged)
        {
            if (panel == null || onHoverChanged == null) return;
            EventTrigger et = panel.GetComponent<EventTrigger>();
            if (et == null) et = panel.gameObject.AddComponent<EventTrigger>();
            et.triggers.Clear();

            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => onHoverChanged(true));
            et.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => onHoverChanged(false));
            et.triggers.Add(exitEntry);
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
            bool styleD = useStyleDHardwareTheme;
            if (styleD) EnsureStyleDSpritesLoaded();

            mapViewerRoot = new GameObject("Hybrid_MapViewer", typeof(RectTransform));
            mapViewerRoot.transform.SetParent(transform, false);
            StretchFull(mapViewerRoot.GetComponent<RectTransform>());

            Image dim = mapViewerRoot.AddComponent<Image>();
            dim.sprite = GetWhiteSprite();
            dim.color = styleD ? StyleDThemeUtility.ColBackdrop : new Color(0f, 0f, 0f, 0.86f);
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
            Outline pOl = panel.AddComponent<Outline>();
            pOl.effectColor = ColBorderSlate;
            pOl.effectDistance = new Vector2(1.5f, -1.5f);
            if (styleD && styleDPanelChassisSprite != null)
            {
                ApplySlicedSprite(pImg, styleDPanelChassisSprite, 3.2f, Color.white, disableOutline: true);
            }
            else
            {
                pImg.sprite = GetWhiteSprite();
                pImg.color = ColPanelBg;
            }

            TextMeshProUGUI title = CreateText(
                panel.transform,
                "Title",
                styleD ? "FLOOR BLUEPRINT - TACTICAL MAP VIEWER" : "// FLOOR BLUEPRINT // TACTICAL MAP VIEWER",
                styleD ? 13.5f : 16f,
                styleD ? ColStyleDBoneText : ColAmber,
                TextAlignmentOptions.MidlineLeft,
                FontStyles.Bold,
                FontRole.Display,
                wrap: false);
            RectTransform tRt = title.rectTransform;
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.pivot = new Vector2(0f, 1f);
            tRt.offsetMin = styleD ? new Vector2(22f, -52f) : new Vector2(16f, -44f);
            tRt.offsetMax = styleD ? new Vector2(-260f, -16f) : new Vector2(-260f, -6f);

            if (styleD)
            {
                GameObject hdrLine = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
                hdrLine.transform.SetParent(panel.transform, false);
                RectTransform hlRt = hdrLine.GetComponent<RectTransform>();
                hlRt.anchorMin = new Vector2(0f, 1f);
                hlRt.anchorMax = new Vector2(1f, 1f);
                hlRt.pivot = new Vector2(0.5f, 1f);
                hlRt.offsetMin = new Vector2(18f, -55f);
                hlRt.offsetMax = new Vector2(-18f, -54f);
                Image hlImg = hdrLine.GetComponent<Image>();
                hlImg.color = ColStyleDHeaderLine;
                hlImg.raycastTarget = false;
            }

            Button closeBtn = CreateActionPill(panel.transform, "BtnCloseMap", "[ X ] CLOSE MAP  [M / ESC]",
                new Color(0.30f, 0.07f, 0.07f, 1f), new Color(0.85f, 0.25f, 0.25f, 1f), out _, styleD ? 10.5f : 12f);
            RectTransform cRt = closeBtn.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(1f, 1f);
            cRt.anchoredPosition = styleD ? new Vector2(-18f, -16f) : new Vector2(-12f, -8f);
            cRt.sizeDelta = styleD ? new Vector2(220f, 32f) : new Vector2(230f, 32f);
            closeBtn.onClick.AddListener(CloseMapViewer);
            if (styleD)
            {
                ApplyButtonSprite(
                    closeBtn,
                    StyleDThemeUtility.WithRgbScale(ColStyleDRust, 0.75f),
                    ColStyleDRust,
                    2.8f,
                    ColStyleDBoneText);
            }

            GameObject host = new GameObject("MapHost", typeof(RectTransform), typeof(RectMask2D));
            host.transform.SetParent(panel.transform, false);
            mapViewerHost = host.GetComponent<RectTransform>();
            if (styleD)
            {
                StretchFull(mapViewerHost, 18f, 18f, 18f, 60f);
                if (styleDWellRecessedSprite != null)
                {
                    Image hostWell = host.AddComponent<Image>();
                    hostWell.raycastTarget = false;
                    ApplySlicedSprite(hostWell, styleDWellRecessedSprite, 3.2f, Color.white);
                }
            }
            else
            {
                StretchFull(mapViewerHost, 12f, 12f, 12f, 52f);
            }

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
                var titleTmp = mapViewerRoot.transform.Find("Panel/Title")?.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null && mapHeaderTitle != null && !string.IsNullOrEmpty(mapHeaderTitle.text))
                {
                    titleTmp.text = useStyleDHardwareTheme
                        ? StripLeadingSlashes(mapHeaderTitle.text)
                        : mapHeaderTitle.text;
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
                        apTmp.alignment = useStyleDHardwareTheme ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
                        apTmp.fontSize = useStyleDHardwareTheme ? 8.2f : 8.5f;
                    }
                }

                mapViewerBp.SetParent(mapViewerOrigParent, false);
                StretchFull((RectTransform)mapViewerBp);
                mapViewerBp.SetSiblingIndex(mapViewerOrigIndex);
            }
            mapViewerRoot.SetActive(false);
            if (hubScreen != null && hubScreen.TacticalMap != null)
            {
                float miniScale = useStyleDHardwareTheme ? 0.88f : MiniMapBaseScale;
                hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, miniScale);
                hubScreen.TacticalMap.RefreshAllNodes();
            }
            if (useStyleDHardwareTheme)
            {
                StyleStyleDMapAndNotePanel();
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
                if (useStyleDHardwareTheme)
                {
                    Color tabCol = active ? ColStyleDRust : ColStyleDTabInactive;
                    Color hovCol = active ? ColStyleDRust * 1.15f : ColStyleDMetalBtn * 1.25f;
                    if (readerTabBgs[i] != null)
                    {
                        readerTabBgs[i].color = Color.white;
                        if (readerTabBgs[i].canvasRenderer != null) readerTabBgs[i].canvasRenderer.SetColor(tabCol);
                    }
                    if (i < readerTabButtons.Count && readerTabButtons[i] != null)
                    {
                        ConfigureButtonColors(readerTabButtons[i], tabCol, hovCol);
                    }
                    if (i < readerTabLabels.Count && readerTabLabels[i] != null)
                        readerTabLabels[i].color = active ? Color.white : ColStyleDBoneText;
                }
                else
                {
                    if (readerTabBgs[i] != null)
                        readerTabBgs[i].color = active ? new Color(0.16f, 0.21f, 0.30f, 1f) : ColSlotBg;
                    if (i < readerTabLabels.Count && readerTabLabels[i] != null)
                        readerTabLabels[i].color = active ? ColAmber : ColTextMuted;
                }
            }

            RefreshReaderTabsContent();

            if (tab == ReaderTab.StoryLog && inlineStoryLogScroll != null && !_storyLogScrolledBack)
            {
                Canvas.ForceUpdateCanvases();
                inlineStoryLogScroll.verticalNormalizedPosition = 0f;
                UpdateStoryLogScrollLockState();
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
                if (useStyleDHardwareTheme)
                {
                    Color tabCol = active ? ColStyleDRust : ColStyleDTabInactive;
                    Color hovCol = active ? ColStyleDRust * 1.15f : ColStyleDMetalBtn * 1.25f;
                    if (survivorTabBgs[i] != null)
                    {
                        survivorTabBgs[i].color = Color.white;
                        if (survivorTabBgs[i].canvasRenderer != null) survivorTabBgs[i].canvasRenderer.SetColor(tabCol);
                    }
                    if (i < survivorTabButtons.Count && survivorTabButtons[i] != null)
                    {
                        ConfigureButtonColors(survivorTabButtons[i], tabCol, hovCol);
                    }
                    if (i < survivorTabLabels.Count && survivorTabLabels[i] != null)
                        survivorTabLabels[i].color = active ? Color.white : ColStyleDBoneText;
                }
                else
                {
                    if (survivorTabBgs[i] != null)
                        survivorTabBgs[i].color = active ? new Color(0.16f, 0.21f, 0.30f, 1f) : ColSlotBg;
                    if (i < survivorTabLabels.Count && survivorTabLabels[i] != null)
                        survivorTabLabels[i].color = active ? ColAmber : ColTextMuted;
                }
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
            if (useStyleDHardwareTheme)
            {
                topCenterBadgeText.text = "";
                return;
            }
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

            ComputeCategorySlotAllocations(
                p,
                new[] { wpnList.Count, appList.Count, conList.Count, utlList.Count },
                out int[] visiblePerCat,
                out int[] perkSlotsPerCat);

            BindCategorySlots("WPN", wpnList, visiblePerCat[0], perkSlotsPerCat[0]);
            BindCategorySlots("APPAREL", appList, visiblePerCat[1], perkSlotsPerCat[1]);
            BindCategorySlots("CONSUMABLE", conList, visiblePerCat[2], perkSlotsPerCat[2]);
            BindCategorySlots("UTILITY", utlList, visiblePerCat[3], perkSlotsPerCat[3]);

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

        private static void ComputeCategorySlotAllocations(
            CharacterData p,
            int[] entryCounts,
            out int[] visiblePerCat,
            out int[] perkSlotsPerCat)
        {
            // Order matches category indices: 0=WPN, 1=APPAREL, 2=CONSUMABLE, 3=UTILITY
            int[] maxPerCat = { 5, 5, 10, 10 };
            visiblePerCat = new[] { 3, 3, 3, 3 };
            perkSlotsPerCat = new[] { 0, 0, 0, 0 };

            int maxBagSlots = p != null ? Mathf.Max(6, p.bagLimit) : 12;
            int[] expandPriority = { 2, 3, 0, 1 };
            int[] shrinkPriority = { 3, 2, 1, 0 };

            int remainingToAdd = Mathf.Max(0, maxBagSlots - 12);
            while (remainingToAdd > 0)
            {
                bool added = false;
                for (int i = 0; i < expandPriority.Length && remainingToAdd > 0; i++)
                {
                    int catIdx = expandPriority[i];
                    if (visiblePerCat[catIdx] < maxPerCat[catIdx])
                    {
                        visiblePerCat[catIdx]++;
                        remainingToAdd--;
                        added = true;
                    }
                }
                if (!added) break;
            }

            int remainingToRemove = Mathf.Max(0, 12 - maxBagSlots);
            while (remainingToRemove > 0)
            {
                bool removed = false;
                for (int i = 0; i < shrinkPriority.Length && remainingToRemove > 0; i++)
                {
                    int catIdx = shrinkPriority[i];
                    if (visiblePerCat[catIdx] > 1)
                    {
                        visiblePerCat[catIdx]--;
                        remainingToRemove--;
                        removed = true;
                    }
                }
                if (!removed) break;
            }

            int perkBonusTotal = (p != null && p.HasTrait("HIKER")) ? 2 : Mathf.Max(0, maxBagSlots - 12);
            int perkRemaining = perkBonusTotal;
            while (perkRemaining > 0)
            {
                bool marked = false;
                for (int i = 0; i < expandPriority.Length && perkRemaining > 0; i++)
                {
                    int catIdx = expandPriority[i];
                    if (perkSlotsPerCat[catIdx] < visiblePerCat[catIdx])
                    {
                        perkSlotsPerCat[catIdx]++;
                        perkRemaining--;
                        marked = true;
                    }
                }
                if (!marked) break;
            }

            // Rebalance empty slots across categories when a category holds more items than its base slots
            // (e.g., equipped gear and key items take 0 bag slots yet still appear in category grids).
            for (int c = 0; c < 4; c++)
            {
                int targetCount = Mathf.Min(maxPerCat[c], entryCounts[c]);
                while (visiblePerCat[c] < targetCount)
                {
                    int donor = -1;
                    // Prefer borrowing non-perk surplus slots first
                    for (int d = 0; d < 4; d++)
                    {
                        if (d != c && visiblePerCat[d] - Mathf.Max(1, entryCounts[d]) > perkSlotsPerCat[d])
                        {
                            donor = d;
                            break;
                        }
                    }
                    if (donor < 0)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            if (d != c && visiblePerCat[d] > Mathf.Max(1, entryCounts[d]))
                            {
                                donor = d;
                                break;
                            }
                        }
                    }
                    visiblePerCat[c]++;
                    if (donor >= 0)
                    {
                        visiblePerCat[donor]--;
                        perkSlotsPerCat[donor] = Mathf.Min(perkSlotsPerCat[donor], visiblePerCat[donor]);
                    }
                }
            }
        }

        private static void AddEquippedEntry(string itemId, List<DisplaySlotEntry> list)
        {
            if (string.IsNullOrEmpty(itemId) || itemId.Equals("none", StringComparison.OrdinalIgnoreCase)) return;
            list.Add(new DisplaySlotEntry { itemId = itemId, quantity = 1, isEquipped = true });
        }

        private void BindCategorySlots(string categoryTag, List<DisplaySlotEntry> entries, int visibleSlots, int perkBonusSlots)
        {
            int entryIdx = 0;
            int catSlotIdx = 0;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                SlotUI slot = inventorySlots[i];
                if (slot.categoryTag != categoryTag) continue;

                if (catSlotIdx >= visibleSlots)
                {
                    if (slot.root != null && slot.root.activeSelf) slot.root.SetActive(false);
                    slot.boundItemId = "";
                    slot.isEquipped = false;
                    catSlotIdx++;
                    continue;
                }

                if (slot.root != null && !slot.root.activeSelf) slot.root.SetActive(true);
                bool isPerkSlot = perkBonusSlots > 0 && catSlotIdx >= (visibleSlots - perkBonusSlots);

                if (entryIdx < entries.Count)
                {
                    var e = entries[entryIdx++];
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
                    if (useStyleDHardwareTheme)
                    {
                        if (slot.outline != null)
                        {
                            slot.outline.enabled = isSelected;
                            if (isSelected)
                            {
                                slot.outline.effectColor = ColStyleDAmber;
                                slot.outline.effectDistance = new Vector2(1.6f, -1.6f);
                            }
                        }
                        if (slot.bgImage != null)
                        {
                            slot.bgImage.color = isSelected
                                ? new Color(1.0f, 0.90f, 0.72f, 1f)
                                : new Color(0.95f, 0.95f, 0.95f, 1f);
                        }
                    }
                    else
                    {
                        if (slot.outline != null)
                        {
                            if (isSelected)
                            {
                                slot.outline.effectColor = ColAmber;
                            }
                            else
                            {
                                Color catCol = ItemDatabase.GetCategoryBorderColor(def.category);
                                catCol.a *= 0.55f;
                                slot.outline.effectColor = catCol;
                            }
                        }
                        if (slot.bgImage != null)
                        {
                            slot.bgImage.color = isSelected ? new Color(0.16f, 0.14f, 0.08f, 1f) : new Color(0.08f, 0.105f, 0.15f, 1f);
                        }
                    }
                    ApplyPerkSlotHue(slot, isPerkSlot, isSelected);
                }
                else
                {
                    slot.boundItemId = "";
                    slot.isEquipped = false;
                    if (slot.iconImage != null) { slot.iconImage.sprite = null; slot.iconImage.enabled = false; }
                    if (slot.eqpBadge != null) slot.eqpBadge.text = "";
                    if (slot.countLabel != null) slot.countLabel.text = "";
                    if (useStyleDHardwareTheme)
                    {
                        if (slot.outline != null) slot.outline.enabled = false;
                        if (slot.bgImage != null) slot.bgImage.color = Color.white;
                    }
                    else
                    {
                        if (slot.outline != null) slot.outline.effectColor = ColBorderSubtle;
                        if (slot.bgImage != null) slot.bgImage.color = ColSlotBg;
                    }
                    ApplyPerkSlotHue(slot, isPerkSlot, isSelected: false);
                }

                catSlotIdx++;
            }
        }

        private void ApplyPerkSlotHue(SlotUI slot, bool isPerkSlot, bool isSelected)
        {
            if (slot == null || slot.root == null) return;

            Transform overlayTr = slot.root.transform.Find("PerkHueOverlay");
            if (!isPerkSlot)
            {
                if (overlayTr != null && overlayTr.gameObject.activeSelf)
                    overlayTr.gameObject.SetActive(false);
                return;
            }

            Image overlayImg;
            if (overlayTr == null)
            {
                GameObject overlayGo = new GameObject("PerkHueOverlay", typeof(RectTransform));
                overlayGo.transform.SetParent(slot.root.transform, false);
                overlayGo.transform.SetSiblingIndex(0);
                RectTransform oRt = overlayGo.GetComponent<RectTransform>();
                oRt.anchorMin = Vector2.zero;
                oRt.anchorMax = Vector2.one;
                oRt.offsetMin = new Vector2(4f, 4f);
                oRt.offsetMax = new Vector2(-4f, -4f);
                overlayImg = overlayGo.AddComponent<Image>();
                overlayImg.sprite = GetWhiteSprite();
                overlayImg.raycastTarget = false;
            }
            else
            {
                if (!overlayTr.gameObject.activeSelf)
                    overlayTr.gameObject.SetActive(true);
                overlayTr.SetSiblingIndex(0);
                overlayImg = overlayTr.GetComponent<Image>();
            }

            if (overlayImg != null)
            {
                overlayImg.color = isSelected
                    ? new Color(0.22f, 0.85f, 0.45f, 0.065f)
                    : new Color(0.14f, 0.72f, 0.36f, 0.035f);
            }

            if (slot.bgImage != null)
            {
                slot.bgImage.color = isSelected
                    ? new Color(0.88f, 1.0f, 0.84f, 1f)
                    : new Color(0.80f, 1.0f, 0.86f, 1f);
            }

            if (slot.outline != null && !isSelected)
            {
                slot.outline.enabled = true;
                slot.outline.effectColor = new Color(0.24f, 0.68f, 0.38f, 0.32f);
                slot.outline.effectDistance = new Vector2(1.0f, -1.0f);
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
                if (inspectorOpenGearBtn != null) inspectorOpenGearBtn.gameObject.SetActive(false);
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

            if (inspectorOpenGearBtn != null)
            {
                bool canDrop = !ItemDatabase.IsKeyItem(_selectedItemId);
                inspectorOpenGearBtn.gameObject.SetActive(canDrop);
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

        private void OnInspectorDropClicked()
        {
            if (hubScreen == null || hubScreen.CurrentPlayer == null || string.IsNullOrEmpty(_selectedItemId)) return;
            if (ItemDatabase.IsKeyItem(_selectedItemId)) return;

            CharacterData p = hubScreen.CurrentPlayer;
            var def = ItemDatabase.GetDefinition(_selectedItemId);
            string droppedItemId = "";

            if (_selectedIsEquipped && def != null && !string.IsNullOrEmpty(def.EquipSlot))
            {
                droppedItemId = p.DropEquipped(def.EquipSlot);
                if (!string.IsNullOrEmpty(droppedItemId))
                {
                    _selectedIsEquipped = false;
                }
            }
            else if (p.RemoveItem(_selectedItemId, 1))
            {
                droppedItemId = _selectedItemId;
            }

            if (string.IsNullOrEmpty(droppedItemId)) return;

            var stash = hubScreen.GetCurrentFloorStashPublic();
            if (stash != null)
            {
                var existing = stash.Find(f => string.Equals(f.itemId, droppedItemId, StringComparison.OrdinalIgnoreCase));
                if (existing != null) existing.quantity += 1;
                else stash.Add(new InventoryEntry(droppedItemId, 1));
            }

            AudioManager.Instance?.PlayButtonClick();
            string displayName = def != null && !string.IsNullOrEmpty(def.displayName) ? def.displayName : droppedItemId;
            if (!p.HasItem(droppedItemId))
            {
                _selectedItemId = "";
                _selectedIsEquipped = false;
            }

            hubScreen.SetSituationNarration($"<color=#F59E0B>[ITEM DROPPED]</color> Dropped {displayName} onto the floor (see FLOOR tab).");
            hubScreen.RefreshFromHybridCockpit();
        }

        private void RefreshSurvivorPanel()
        {
            CharacterData p = hubScreen != null ? hubScreen.CurrentPlayer : null;
            int dayCount = hubScreen != null && hubScreen.Clock != null ? hubScreen.Clock.DayCount : 0;

            if (p == null)
            {
                if (survivorLvlDayText != null) survivorLvlDayText.text = useStyleDHardwareTheme ? $"LVL 1 | DAY {dayCount}" : $"LVL 1 // DAY {dayCount}";
                return;
            }

            if (survivorLvlDayText != null) survivorLvlDayText.text = useStyleDHardwareTheme ? $"LVL {p.level} | DAY {dayCount}" : $"LVL {p.level} // DAY {dayCount}";
            if (survivorNameText != null) survivorNameText.text = p.FullName.ToUpperInvariant();
            if (survivorJobText != null) survivorJobText.text = useStyleDHardwareTheme ? $"{p.job.ToUpperInvariant()} | {p.gender.ToUpperInvariant()}" : $"{p.job.ToUpperInvariant()} // {p.gender.ToUpperInvariant()}";

            if (hungerValueText != null)
            {
                Color hCol = p.hunger <= 25 ? (useStyleDHardwareTheme ? ColStyleDAmber : ColAmber) : (useStyleDHardwareTheme ? ColStyleDBoneText : ColTextMain);
                hungerValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(hCol)}>HUNGER {p.hunger}%</color>";
            }

            if (thirstValueText != null)
            {
                Color tCol = p.thirst <= 25 ? (useStyleDHardwareTheme ? ColStyleDAmber : ColAmber) : (useStyleDHardwareTheme ? ColStyleDBoneText : ColTextMain);
                thirstValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(tCol)}>THIRST {p.thirst}%</color>";
            }

            if (xpFillImg != null) SetFillWidth(xpFillImg.rectTransform, Mathf.Clamp01((float)p.xp / Mathf.Max(1, p.xpRequired)));
            if (xpValueText != null) xpValueText.text = $"XP {p.xp} / {p.xpRequired}";

            // Vitals: 0 = HEALTH (Red), 1 = STAMINA (Amber), 2 = HUMANITY (Violet)
            SetVitalBar(0, p.hp, p.hpMax);
            SetVitalBar(1, p.stamina, p.staminaMax);
            SetVitalBar(2, p.humanity, 100);

            if (useStyleDHardwareTheme)
            {
                float hp01 = (float)p.hp / Mathf.Max(1, p.hpMax);
                if (_styleDEcg != null) _styleDEcg.SetHealthNormalized(hp01);

                SetStyleDMortalValue("MGT", p.Might);
                SetStyleDMortalValue("OBS", p.Observation);
                SetStyleDMortalValue("RFL", p.Reflex);
                SetStyleDMortalValue("TAC", p.Tactics);
                SetStyleDMortalValue("AUS", p.Auspice);
                SetStyleDMortalValue("LDR", p.Leadership);

                if (_styleDConditionText != null)
                {
                    if (hp01 < 0.30f)
                        _styleDConditionText.text = "CONDITION: <color=#E04242>CRITICAL</color>";
                    else if (hp01 < 0.60f || p.stamina < Mathf.RoundToInt(p.staminaMax * 0.35f) || p.hunger <= 25 || p.thirst <= 25)
                        _styleDConditionText.text = "CONDITION: <color=#E09C2B>CAUTION</color>";
                    else
                        _styleDConditionText.text = "CONDITION: <color=#3AD679>STABLE</color>";
                }
            }

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
                if (useStyleDHardwareTheme)
                {
                    if (slot.outline != null)
                    {
                        slot.outline.enabled = selected;
                        if (selected) slot.outline.effectColor = ColStyleDAmber;
                    }
                    if (slot.bgImage != null)
                    {
                        slot.bgImage.color = selected ? new Color(0.95f, 0.86f, 0.68f, 1f) : Color.white;
                    }
                }
                else
                {
                    if (slot.outline != null)
                    {
                        slot.outline.effectColor = selected ? ColAmber : (hasItem ? ColCyan : ColBorderSlate);
                    }
                    if (slot.bgImage != null)
                    {
                        slot.bgImage.color = selected ? new Color(0.14f, 0.17f, 0.24f, 1f) : ColSlotBg;
                    }
                }
            }

            if (partyHeaderText != null)
            {
                partyHeaderText.text = useStyleDHardwareTheme ? "SURVIVOR GROUP (1 / 4)" : "// SURVIVOR GROUP // PARTY (1 / 4)";
            }
            if (partyLeaderNameText != null)
            {
                partyLeaderNameText.text = useStyleDHardwareTheme
                    ? $"[1] LEADER - {p.FullName.ToUpperInvariant()}"
                    : $"[1] LEADER // {p.FullName.ToUpperInvariant()}";
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

            if (mapHeaderTitle != null) mapHeaderTitle.text = useStyleDHardwareTheme ? $"{floorTag} MAP" : $"// FLOOR BLUEPRINT // {floorTag}";

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
                    displayLoc = useStyleDHardwareTheme ? $"CORRIDOR | SEC {hubScreen.HallwaySection}" : $"CORRIDOR // SEC {hubScreen.HallwaySection}";

                mapSubHeaderPosText.text = useStyleDHardwareTheme
                    ? $"<color=#CF9232><b>POS:</b></color> <color=#E4DEC9>{displayLoc}</color>"
                    : $"<color=#00e5ff><b>POS:</b> {displayLoc}</color>";
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
                string badgeHex = useStyleDHardwareTheme ? "#CF9232" : "#F59E0B";
                readerTabLabels[0].text = "SITUATION";
                readerTabLabels[1].text = $"STORY LOG ({logCount})";
                readerTabLabels[2].text = $"NOTES ({notesCount})";
                readerTabLabels[3].text = floorCount > 0 && _activeTab != ReaderTab.Floor
                    ? $"FLOOR <color={badgeHex}><b>({floorCount})</b></color>"
                    : $"FLOOR ({floorCount})";
            }

            float scale = SundayTypography.NarrativeFontScale;
            UpdateFontControlUI();

            if (inlineStoryLogText != null && hubScreen.StoryLogs != null)
            {
                inlineStoryLogText.fontSize = Mathf.Round(13.5f * scale * 2f) * 0.5f;
                inlineStoryLogText.lineSpacing = 8f;

                bool countChanged = logCount != _lastRenderedStoryLogCount;
                bool scaleChanged = !Mathf.Approximately(scale, _lastRenderedFontScale);

                RectTransform slRt = inlineStoryLogText.transform.parent as RectTransform;
                RectTransform vpRt = inlineStoryLogScroll != null ? inlineStoryLogScroll.viewport : null;
                bool isScrollable = slRt != null && vpRt != null && slRt.rect.height > vpRt.rect.height + 5f;
                bool wasScrolledBack = isScrollable && inlineStoryLogScroll.verticalNormalizedPosition > 0.04f;
                float savedTopOffset = slRt != null ? slRt.anchoredPosition.y : 0f;

                string sepHex = useStyleDHardwareTheme ? "#3F3B34" : "#334155";
                inlineStoryLogText.text = string.Join($"\n\n<color={sepHex}>----------------------------------------------------</color>\n\n", hubScreen.StoryLogs);
                if (slRt != null)
                {
                    inlineStoryLogText.ForceMeshUpdate();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(slRt);
                    Canvas.ForceUpdateCanvases();

                    if (wasScrolledBack)
                    {
                        float maxScrollY = vpRt != null ? Mathf.Max(0f, slRt.rect.height - vpRt.rect.height) : savedTopOffset;
                        float clampedY = Mathf.Clamp(savedTopOffset, 0f, maxScrollY);
                        slRt.anchoredPosition = new Vector2(slRt.anchoredPosition.x, clampedY);
                        if (inlineStoryLogScroll != null) inlineStoryLogScroll.velocity = Vector2.zero;
                        if (countChanged && logCount > _lastRenderedStoryLogCount && _lastRenderedStoryLogCount >= 0)
                        {
                            _hasUnreadLogWhileScrolled = true;
                        }
                    }
                    else if ((countChanged || scaleChanged) && inlineStoryLogScroll != null)
                    {
                        inlineStoryLogScroll.verticalNormalizedPosition = 0f;
                    }
                }

                _lastRenderedStoryLogCount = logCount;
                _lastRenderedFontScale = scale;
                UpdateStoryLogScrollLockState();
            }

            if (inlineNotesText != null)
            {
                inlineNotesText.fontSize = Mathf.Round(13.5f * scale * 2f) * 0.5f;
                inlineNotesText.lineSpacing = 7f;
                string hHex = useStyleDHardwareTheme ? "#CF9232" : "#F59E0B";
                string tHex = useStyleDHardwareTheme ? "#E4DEC9" : "#38BDF8";
                string doneHex = useStyleDHardwareTheme ? "#3AD679" : "#22C55E";
                string mutHex = useStyleDHardwareTheme ? "#9A958A" : "#64748B";
                string pfx = useStyleDHardwareTheme ? "" : "// ";
                var lines = new List<string>();
                if (hubScreen.Objectives != null && hubScreen.Objectives.Objectives != null)
                {
                    lines.Add($"<b><color={hHex}>{pfx}ACTIVE OBJECTIVES</color></b>");
                    foreach (var obj in hubScreen.Objectives.Objectives)
                    {
                        if (obj.status == ObjectiveStatus.Locked) continue;
                        string mark = obj.status == ObjectiveStatus.Completed ? $"<color={doneHex}>[DONE]</color>" : $"<color={tHex}>[ACTIVE]</color>";
                        lines.Add($"{mark} {obj.description}");
                    }
                    lines.Add("");
                }

                lines.Add($"<b><color={hHex}>{pfx}RECORDED NOTES & INTEL</color></b>");
                if (hubScreen.Lore != null && hubScreen.Lore.Entries != null && hubScreen.Lore.Entries.Count > 0)
                {
                    foreach (var entry in hubScreen.Lore.Entries)
                    {
                        lines.Add($"<b><color={tHex}>[{entry.timestamp}] {entry.title}</color></b>\n{entry.content}");
                    }
                }
                else
                {
                    lines.Add($"<color={mutHex}>No field notes recorded yet. Check your phone, radio, and apartment logs.</color>");
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
                if (useStyleDHardwareTheme && styleDStatCardSprite != null)
                {
                    ApplySlicedSprite(rBg, styleDStatCardSprite, 3.2f, Color.white, disableOutline: false);
                    rOl.enabled = false;
                }

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
                    11.5f, useStyleDHardwareTheme ? ColStyleDBoneText : ColTextMain, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, FontRole.Body, wrap: false);
                lbl.rectTransform.anchorMin = Vector2.zero;
                lbl.rectTransform.anchorMax = Vector2.one;
                lbl.rectTransform.offsetMin = new Vector2(44f, 0f);
                lbl.rectTransform.offsetMax = new Vector2(-95f, 0f);

                string capId = entry.itemId;
                Button pickBtn = CreateActionPill(row.transform, "Btn_Take", "TAKE",
                    new Color(0.11f, 0.24f, 0.19f, 1f), new Color(0.16f, 0.72f, 0.45f, 1f), out _, 10f);
                if (useStyleDHardwareTheme && styleDButtonMetalSprite != null)
                {
                    ApplyButtonSprite(pickBtn, ColStyleDEquipBtn, ColStyleDEquipBtn * 1.25f, 2.4f, ColStyleDBoneText);
                }
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
            EnsureStyleDChoiceDeckSpacing();
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
                    if (useStyleDHardwareTheme && styleDStatCardSprite != null)
                    {
                        ApplySlicedSprite(img, styleDStatCardSprite, 2.4f, Color.white, disableOutline: true);
                        ConfigureButtonColors(narrativePromptBtn, Color.white, new Color(1.06f, 1.03f, 0.96f, 1f));
                    }
                    else
                    {
                        ConfigureButtonColors(narrativePromptBtn, img.color, new Color(0.08f, 0.14f, 0.22f, 1f));
                    }
                    narrativePromptBtn.targetGraphic = img;
                }

                if (useStyleDHardwareTheme)
                {
                    var hdrTmp = narrativePromptCard.transform.Find("PromptHeader")?.GetComponent<TextMeshProUGUI>();
                    if (hdrTmp != null)
                    {
                        hdrTmp.text = "<b>INCOMING EMERGENCY TRANSMISSION</b>";
                        hdrTmp.color = ColStyleDAmber;
                    }
                    var subTmp = narrativePromptCard.transform.Find("PromptSub")?.GetComponent<TextMeshProUGUI>();
                    if (subTmp != null)
                    {
                        subTmp.text = "Follow the live broadcast and telemetry readout in the narrative display.";
                        subTmp.color = ColStyleDMutedStone;
                    }
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
                    if (useStyleDHardwareTheme && styleDButtonMetalSprite != null)
                    {
                        ApplyButtonSprite(acBtn, ColStyleDMetalBtn, ColStyleDRust, 2.2f, ColStyleDBoneText);
                        var actTmp = actionCallout.Find("ActionText")?.GetComponent<TextMeshProUGUI>();
                        if (actTmp != null)
                        {
                            actTmp.text = "<color=#CF9232><b>[ SPACE / ENTER ]</b></color>  <color=#9A958A>OR</color>  <color=#E4DEC9><b>CLICK TO ADVANCE >></b></color>";
                        }
                    }
                    else if (acImg != null)
                    {
                        acBtn.targetGraphic = acImg;
                    }
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
            float scale = SundayTypography.NarrativeFontScale;

            // ContinuePrompt
            Transform prTr = awContainer.transform.Find("ContinuePrompt");
            if (prTr != null)
            {
                RectTransform prRt = prTr.GetComponent<RectTransform>();
                prRt.anchorMin = new Vector2(1f, 0f);
                prRt.anchorMax = new Vector2(1f, 0f);
                prRt.pivot = new Vector2(1f, 0f);
                prRt.anchoredPosition = new Vector2(-10f, 4f);
                prRt.sizeDelta = new Vector2(220f, 20f);
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
                        b1Tmp.fontSize = Mathf.Round(13f * scale * 2f) * 0.5f;
                        b1Tmp.lineSpacing = 5f;
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
                            etTmp.fontSize = Mathf.Round(12f * scale * 2f) * 0.5f;
                            etTmp.lineSpacing = 5f;
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
                    if (b2Tmp != null) b2Tmp.fontSize = Mathf.Round(13f * scale * 2f) * 0.5f;
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
                            stTmp.fontSize = Mathf.Round(12.5f * scale * 2f) * 0.5f;
                            stTmp.lineSpacing = 6f;
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
                        b2fTmp.fontSize = Mathf.Round(12.5f * scale * 2f) * 0.5f;
                        b2fTmp.lineSpacing = 5.5f;
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
                    b3tRt.anchoredPosition = new Vector2(8f, -26f);
                    b3tRt.sizeDelta = new Vector2(-16f, 94f);
                    var b3Tmp = b3tTr.GetComponent<TextMeshProUGUI>();
                    if (b3Tmp != null)
                    {
                        b3Tmp.fontSize = Mathf.Round(11.5f * scale * 2f) * 0.5f;
                        b3Tmp.lineSpacing = 2.5f;
                    }
                }

                Transform stashTr = b3Tr.Find("TableStashDisplay");
                if (stashTr != null)
                {
                    RectTransform sRt = stashTr.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0f, 0f);
                    sRt.anchorMax = new Vector2(1f, 0f);
                    sRt.pivot = new Vector2(0.5f, 0f);
                    sRt.anchoredPosition = new Vector2(0f, 28f);
                    sRt.sizeDelta = new Vector2(-12f, 122f);

                    Button stashBtn = stashTr.GetComponent<Button>();
                    if (stashBtn == null) stashBtn = stashTr.gameObject.AddComponent<Button>();
                    stashBtn.onClick.RemoveAllListeners();
                    stashBtn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlayButtonClick();
                        hubScreen?.TriggerAdvanceAwakening();
                    });
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

        #region Style D Hardware Theme
        public static Sprite SharedStyleDPanelChassis { get; private set; }
        public static Sprite SharedStyleDCrtBezel { get; private set; }
        public static Sprite SharedStyleDWellRecessed { get; private set; }
        public static Sprite SharedStyleDSlotRecessed { get; private set; }
        public static Sprite SharedStyleDButtonMetal { get; private set; }
        public static Sprite SharedStyleDStatCard { get; private set; }
        public static Texture2D GetSharedStyleDCrtScanlineTexture() => GetStyleDCrtScanlineTexture();

        private void EnsureStyleDSpritesLoaded()
        {
            if (styleDPanelChassisSprite == null) styleDPanelChassisSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_panel_chassis_9slice.png");
            if (styleDCrtBezelSprite == null) styleDCrtBezelSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_crt_bezel_frame.png");
            if (styleDWellRecessedSprite == null) styleDWellRecessedSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_well_recessed_9slice.png");
            if (styleDSlotRecessedSprite == null) styleDSlotRecessedSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_slot_recessed_9slice.png");
            if (styleDStatCardSprite == null) styleDStatCardSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_stat_card_9slice.png");
            if (styleDButtonMetalSprite == null) styleDButtonMetalSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_button_metal_9slice.png");
            if (styleDMapPaperSprite == null) styleDMapPaperSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_map_paper_sheet.png");
            if (styleDPinnedNoteSprite == null) styleDPinnedNoteSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_pinned_note_card.png");
            if (styleDRedPushpinSprite == null) styleDRedPushpinSprite = LoadSpriteFallback("Assets/Sprites/UI/HardwareTheme/ui_red_pushpin.png");

            SharedStyleDPanelChassis = styleDPanelChassisSprite;
            SharedStyleDCrtBezel = styleDCrtBezelSprite;
            SharedStyleDWellRecessed = styleDWellRecessedSprite;
            SharedStyleDSlotRecessed = styleDSlotRecessedSprite;
            SharedStyleDButtonMetal = styleDButtonMetalSprite;
            SharedStyleDStatCard = styleDStatCardSprite;
        }

        private static void SetOutlineEnabled(Transform tr, bool enabled)
        {
            if (tr == null) return;
            Outline ol = tr.GetComponent<Outline>();
            if (ol != null) ol.enabled = enabled;
        }

        private static void ApplySlicedSprite(Image img, Sprite sp, float ppuMult, Color tint, bool disableOutline = true)
        {
            if (img == null || sp == null) return;
            img.sprite = sp;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppuMult;
            img.color = tint;
            if (disableOutline) SetOutlineEnabled(img.transform, false);
        }

        private void ApplyButtonSprite(Button btn, Color normalTint, Color hoverTint, float ppuMult = 2.2f, Color? textCol = null, bool disableOutline = true)
        {
            if (btn == null) return;
            Image bg = btn.targetGraphic as Image ?? btn.GetComponent<Image>();
            if (bg != null && styleDButtonMetalSprite != null)
            {
                // Keep Image.color white so Selectable ColorTint applies normalTint once without squaring/darkening
                ApplySlicedSprite(bg, styleDButtonMetalSprite, ppuMult, Color.white, disableOutline);
                if (bg.canvasRenderer != null) bg.canvasRenderer.SetColor(normalTint);
            }
            ConfigureButtonColors(btn, normalTint, hoverTint);
            if (textCol.HasValue)
            {
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null) tmp.color = textCol.Value;
            }
        }

        private static string StripLeadingSlashes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.TrimStart('/', ' ');
        }

        private void StyleOuterPanelChassis(RectTransform panel, string cleanTitle, bool hasBody = true)
        {
            if (panel == null) return;
            // ppuMult = 3.2f yields a crisp ~14px riveted frame; inset Header and Body >= 18px (>= 16px top) for >= 4px dark clearance
            ApplySlicedSprite(panel.GetComponent<Image>(), styleDPanelChassisSprite, 3.2f, Color.white);

            Transform hdr = panel.Find("Header");
            if (hdr != null)
            {
                RectTransform hRt = hdr as RectTransform;
                hRt.anchorMin = new Vector2(0f, 1f);
                hRt.anchorMax = new Vector2(1f, 1f);
                hRt.pivot = new Vector2(0.5f, 1f);
                hRt.offsetMin = new Vector2(18f, -42f);
                hRt.offsetMax = new Vector2(-18f, -16f);

                Image hBg = hdr.GetComponent<Image>();
                if (hBg != null) hBg.color = ColStyleDHeaderBg;

                Transform bl = hdr.Find("BottomLine");
                if (bl != null && bl.GetComponent<Image>() is Image blImg)
                {
                    blImg.color = ColStyleDHeaderLine;
                }

                Transform titleTr = hdr.Find("Title");
                if (titleTr != null && titleTr.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI titleTmp)
                {
                    titleTmp.text = cleanTitle;
                    titleTmp.color = ColStyleDBoneText;
                    titleTmp.fontSize = 11f;
                    titleTmp.rectTransform.offsetMin = new Vector2(6f, 0f);
                }

                Transform rbBtnTr = hdr.Find("RightBadgeButton");
                if (rbBtnTr != null && rbBtnTr.GetComponent<Button>() is Button rbBtn)
                {
                    var rbLbl = rbBtnTr.GetComponentInChildren<TextMeshProUGUI>(true);
                    bool isWide = rbLbl != null && !string.IsNullOrEmpty(rbLbl.text) && rbLbl.text.Length > 8;
                    if (rbBtnTr is RectTransform rbRt)
                    {
                        rbRt.anchoredPosition = new Vector2(-4f, 0f);
                        rbRt.sizeDelta = new Vector2(isWide ? 112f : 86f, 20f);
                    }
                    if (rbLbl != null)
                    {
                        rbLbl.fontSize = 9.5f;
                        StretchFull(rbLbl.rectTransform, 6f, 0f, 6f, 0f);
                    }
                    ApplyButtonSprite(rbBtn, ColStyleDMetalBtn, ColStyleDMetalBtn * 1.25f, 3.2f, ColStyleDBoneText);
                }

                Transform rbTr = hdr.Find("RightBadge");
                if (rbTr != null && rbTr.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI rbTmp)
                {
                    rbTmp.color = ColStyleDMutedStone;
                    rbTmp.rectTransform.offsetMax = new Vector2(-6f, 0f);
                }
            }

            if (hasBody)
            {
                Transform bodyTr = panel.Find("Body");
                if (bodyTr is RectTransform bodyRt)
                {
                    bodyRt.anchorMin = Vector2.zero;
                    bodyRt.anchorMax = Vector2.one;
                    bodyRt.offsetMin = new Vector2(18f, 18f);
                    bodyRt.offsetMax = new Vector2(-18f, -46f);
                }
            }
        }

        public void ApplyStyleDHardwareTheme()
        {
            if (!useStyleDHardwareTheme) return;

            SundayTypography.StyleDThemeEnabled = true;
            EnsureStyleDSpritesLoaded();
            if (hubScreen == null) hubScreen = GetComponent<GameStageScreen>();

            TacticalChoiceButton.StyleDEnabled = true;
            TacticalChoiceButton.StyleDButtonSprite = styleDButtonMetalSprite;
            TacticalMapViewController.StyleDThemeEnabled = true;

            var rootImg = GetComponent<Image>();
            if (rootImg != null) rootImg.color = ColStyleDBgOuter;

            ApplyStyleDTypographyRemap();
            StyleStyleDCrtViewport();
            StyleStyleDInventoryPanel();
            StyleStyleDSurvivorPanel();
            StyleStyleDMapAndNotePanel();
            StyleStyleDReaderAndDeck();
            StyleStyleDAwakeningBoxes();
            UpdateFontControlUI();
        }

        private void ApplyStyleDTypographyRemap()
        {
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp == null) continue;
                string fName = tmp.font != null ? tmp.font.name : "";
                if (fName.StartsWith("VCR_OSD", StringComparison.OrdinalIgnoreCase) ||
                    fName.StartsWith("SpecialElite", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (fName.StartsWith("Oswald", StringComparison.OrdinalIgnoreCase) ||
                    fName.StartsWith("ChakraPetch", StringComparison.OrdinalIgnoreCase))
                {
                    SundayTypography.ApplyFont(tmp, FontRole.Display);
                }
                else if (fName.StartsWith("Merriweather", StringComparison.OrdinalIgnoreCase) ||
                         fName.StartsWith("Spectral", StringComparison.OrdinalIgnoreCase))
                {
                    SundayTypography.ApplyFont(tmp, FontRole.Narrative);
                }
                else
                {
                    SundayTypography.ApplyFont(tmp, FontRole.Mono);
                }
            }
        }

        private void StyleStyleDCrtViewport()
        {
            Transform topStage = transform.Find("TopScreen_VisualStage");
            if (topStage != null)
            {
                SetOutlineEnabled(topStage, false);
                var tsImg = topStage.GetComponent<Image>();
                if (tsImg != null) tsImg.color = ColStyleDBgOuter;

                Transform framedStage = topStage.Find("FramedStage");
                if (framedStage is RectTransform fsRt)
                {
                    fsRt.anchorMin = new Vector2(0.018f, 0.024f);
                    fsRt.anchorMax = new Vector2(0.982f, 0.976f);
                    fsRt.pivot = new Vector2(0.5f, 0.5f);
                    fsRt.offsetMin = Vector2.zero;
                    fsRt.offsetMax = Vector2.zero;
                    fsRt.anchoredPosition = Vector2.zero;
                }

                Transform bezelTr = topStage.Find("StyleD_CRTBezelOverlay");
                GameObject bezelGo;
                if (bezelTr != null)
                {
                    bezelGo = bezelTr.gameObject;
                }
                else
                {
                    bezelGo = new GameObject("StyleD_CRTBezelOverlay", typeof(RectTransform), typeof(Image));
                    bezelGo.transform.SetParent(topStage, false);
                }
                _styleDCrtOverlayObj = bezelGo;

                RectTransform bzRt = bezelGo.GetComponent<RectTransform>();
                bzRt.anchorMin = Vector2.zero;
                bzRt.anchorMax = Vector2.one;
                bzRt.pivot = new Vector2(0.5f, 0.5f);
                bzRt.offsetMin = Vector2.zero;
                bzRt.offsetMax = Vector2.zero;

                Image bzImg = bezelGo.GetComponent<Image>();
                bzImg.sprite = styleDCrtBezelSprite;
                bzImg.type = Image.Type.Simple;
                bzImg.preserveAspect = false;
                bzImg.color = Color.white;
                bzImg.raycastTarget = false;

                if (framedStage != null)
                {
                    bezelGo.transform.SetSiblingIndex(framedStage.GetSiblingIndex() + 1);
                }
            }

            Transform topHud = transform.Find("TopHUD");
            if (topHud != null)
            {
                if (topHud is RectTransform thRt)
                {
                    SetPixelBox(thRt, 486f, 68f, 1434f, 102f);
                }

                if (topStage != null && topHud.GetSiblingIndex() < topStage.GetSiblingIndex())
                {
                    topHud.SetSiblingIndex(topStage.GetSiblingIndex() + 1);
                }

                var thImg = topHud.GetComponent<Image>();
                if (thImg != null)
                {
                    thImg.sprite = null;
                    thImg.type = Image.Type.Simple;
                    thImg.color = Color.clear;
                    thImg.raycastTarget = false;
                }
                SetOutlineEnabled(topHud, false);

                Transform bbTr = topHud.Find("BottomBorder");
                if (bbTr != null) bbTr.gameObject.SetActive(false);

                var locTmp = topHud.Find("LocationBadge")?.GetComponent<TextMeshProUGUI>();
                if (locTmp != null)
                {
                    SundayTypography.ApplyFont(locTmp, FontRole.VcrOsd);
                    locTmp.fontStyle = FontStyles.Normal;
                    locTmp.color = new Color(0.90f, 0.88f, 0.82f, 0.82f);
                    locTmp.fontSize = 13.5f;
                    locTmp.characterSpacing = 1f;
                    locTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    locTmp.raycastTarget = false;
                    locTmp.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    locTmp.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                    locTmp.rectTransform.pivot = new Vector2(0f, 0.5f);
                    locTmp.rectTransform.anchoredPosition = new Vector2(4f, 2f);
                    locTmp.rectTransform.sizeDelta = new Vector2(520f, 28f);
                    if (!string.IsNullOrEmpty(locTmp.text))
                    {
                        locTmp.text = StripLeadingSlashes(locTmp.text).Replace(" // ", " - ");
                    }
                    ApplyStyleDCrtOsdLegibility(locTmp);
                    EnsureCrtOsdChromatic(locTmp);
                }

                Transform clkBlock = topHud.Find("ClockBlock");
                if (clkBlock is RectTransform cbRt)
                {
                    cbRt.anchorMin = new Vector2(1f, 0.5f);
                    cbRt.anchorMax = new Vector2(1f, 0.5f);
                    cbRt.pivot = new Vector2(1f, 0.5f);
                    cbRt.anchoredPosition = new Vector2(-4f, 0f);
                    cbRt.sizeDelta = new Vector2(132f, 34f);
                }
                var ctTmp = (clkBlock != null ? clkBlock.Find("ClockTime")?.GetComponent<TextMeshProUGUI>() : null)
                            ?? topHud.Find("ClockTime")?.GetComponent<TextMeshProUGUI>()
                            ?? hubScreen?.ClockTimeText;
                if (ctTmp != null)
                {
                    SundayTypography.ApplyFont(ctTmp, FontRole.VcrOsd);
                    ctTmp.fontStyle = FontStyles.Normal;
                    ctTmp.color = new Color(0.95f, 0.72f, 0.28f, 0.86f);
                    ctTmp.fontSize = 16.5f;
                    ctTmp.characterSpacing = 0.5f;
                    ctTmp.alignment = TextAlignmentOptions.Right;
                    ctTmp.raycastTarget = false;
                    ctTmp.rectTransform.anchorMin = new Vector2(0f, 0.38f);
                    ctTmp.rectTransform.anchorMax = Vector2.one;
                    ctTmp.rectTransform.offsetMin = Vector2.zero;
                    ctTmp.rectTransform.offsetMax = Vector2.zero;
                    ApplyStyleDCrtOsdLegibility(ctTmp);
                    EnsureCrtOsdChromatic(ctTmp);
                }

                var cdTmp = (clkBlock != null ? clkBlock.Find("ClockDay")?.GetComponent<TextMeshProUGUI>() : null)
                            ?? topHud.Find("ClockDay")?.GetComponent<TextMeshProUGUI>()
                            ?? hubScreen?.ClockDayText;
                if (cdTmp != null)
                {
                    SundayTypography.ApplyFont(cdTmp, FontRole.VcrOsd);
                    cdTmp.fontStyle = FontStyles.Normal;
                    cdTmp.color = new Color(0.84f, 0.82f, 0.76f, 0.78f);
                    cdTmp.fontSize = 10.5f;
                    cdTmp.characterSpacing = 1f;
                    cdTmp.alignment = TextAlignmentOptions.Right;
                    cdTmp.raycastTarget = false;
                    cdTmp.rectTransform.anchorMin = Vector2.zero;
                    cdTmp.rectTransform.anchorMax = new Vector2(1f, 0.40f);
                    cdTmp.rectTransform.offsetMin = Vector2.zero;
                    cdTmp.rectTransform.offsetMax = Vector2.zero;
                    ApplyStyleDCrtOsdLegibility(cdTmp);
                    EnsureCrtOsdChromatic(cdTmp);
                }

                var centerTmp = topCenterBadgeText ?? topHud.Find("HybridCenterBadge")?.GetComponent<TextMeshProUGUI>();
                if (centerTmp != null)
                {
                    SundayTypography.ApplyFont(centerTmp, FontRole.VcrOsd);
                    centerTmp.fontStyle = FontStyles.Normal;
                    centerTmp.color = new Color(0.84f, 0.82f, 0.76f, 0.76f);
                    centerTmp.fontSize = 12f;
                    centerTmp.characterSpacing = 1f;
                    centerTmp.raycastTarget = false;
                    ApplyStyleDCrtOsdLegibility(centerTmp);
                    EnsureCrtOsdChromatic(centerTmp);
                }

                EnsureStyleDCrtOsdScanlines(topHud);
            }
        }

        private static Texture2D _styleDCrtScanlineTex;

        private static Texture2D GetStyleDCrtScanlineTexture()
        {
            if (_styleDCrtScanlineTex != null) return _styleDCrtScanlineTex;

            var tex = new Texture2D(1, 4, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            tex.SetPixel(0, 1, new Color(0f, 0f, 0f, 0f));
            tex.SetPixel(0, 2, new Color(0f, 0f, 0f, 0.42f));
            tex.SetPixel(0, 3, new Color(0f, 0f, 0f, 0.16f));
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            _styleDCrtScanlineTex = tex;
            return _styleDCrtScanlineTex;
        }

        private static void EnsureCrtOsdChromatic(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            var fx = tmp.GetComponent<CrtOsdChromaticEffect>() ?? tmp.gameObject.AddComponent<CrtOsdChromaticEffect>();
            fx.SyncNow();
        }

        private static void EnsureStyleDCrtOsdScanlines(Transform topHud)
        {
            if (topHud == null) return;
            Transform scanTr = topHud.Find("StyleD_CrtOsdScanlines");
            GameObject scanGo = scanTr != null
                ? scanTr.gameObject
                : new GameObject("StyleD_CrtOsdScanlines", typeof(RectTransform), typeof(RawImage));
            if (scanTr == null) scanGo.transform.SetParent(topHud, false);

            StretchFull(scanGo.GetComponent<RectTransform>());
            var rawImg = scanGo.GetComponent<RawImage>();
            rawImg.texture = GetStyleDCrtScanlineTexture();
            rawImg.color = Color.white;
            rawImg.uvRect = new Rect(0f, 0f, 1f, 34f / 4f);
            rawImg.raycastTarget = false;
            scanGo.transform.SetAsLastSibling();
        }

        private static void ApplyStyleDCrtOsdLegibility(Graphic graphic)
        {
            if (graphic == null) return;

            var outline = graphic.GetComponent<Outline>();
            if (outline == null) outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.enabled = true;

            Shadow dropShadow = null;
            var shadows = graphic.GetComponents<Shadow>();
            for (int i = 0; i < shadows.Length; i++)
            {
                if (shadows[i] != null && shadows[i].GetType() == typeof(Shadow))
                {
                    dropShadow = shadows[i];
                    break;
                }
            }
            if (dropShadow == null) dropShadow = graphic.gameObject.AddComponent<Shadow>();
            dropShadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
            dropShadow.effectDistance = new Vector2(1.5f, -1.5f);
            dropShadow.enabled = true;
        }

        private void StyleStyleDInventoryPanel()
        {
            if (topLeftInventoryPanel == null) return;
            StyleOuterPanelChassis(topLeftInventoryPanel, "INVENTORY");

            Transform body = topLeftInventoryPanel.Find("Body");
            if (body != null)
            {
                (string tag, int rows)[] sections =
                {
                    ("WPN", 1),
                    ("APPAREL", 1),
                    ("CONSUMABLE", 2),
                    ("UTILITY", 2)
                };
                foreach (var secInfo in sections)
                {
                    Transform secTr = body.Find($"Section_{secInfo.tag}");
                    if (secTr == null) continue;

                    float secH = 16f + secInfo.rows * 48f + Mathf.Max(0, secInfo.rows - 1) * 4f;
                    var secLe = secTr.GetComponent<LayoutElement>();
                    if (secLe != null)
                    {
                        secLe.preferredHeight = secH;
                        secLe.minHeight = secH;
                    }

                    var lbl = secTr.Find("Label")?.GetComponent<TextMeshProUGUI>();
                    if (lbl != null)
                    {
                        lbl.color = ColStyleDMutedStone;
                        lbl.rectTransform.offsetMin = new Vector2(7f, -15f);
                    }

                    var glg = secTr.Find("Grid")?.GetComponent<GridLayoutGroup>();
                    if (glg != null)
                    {
                        glg.cellSize = new Vector2(62f, 48f);
                        glg.spacing = new Vector2(5f, 4f);
                        glg.padding = new RectOffset(7, 0, 0, 0);
                        glg.childAlignment = TextAnchor.UpperLeft;
                    }
                }
            }

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];
                if (slot == null) continue;
                if (slot.bgImage != null && styleDSlotRecessedSprite != null)
                {
                    ApplySlicedSprite(slot.bgImage, styleDSlotRecessedSprite, 2.2f, Color.white, disableOutline: false);
                }
                if (slot.outline != null) slot.outline.enabled = false;
                if (slot.countLabel != null) slot.countLabel.color = ColStyleDBoneText;
            }

            Transform micro = body != null ? body.Find("MicroInspector") : null;
            if (micro != null)
            {
                ApplySlicedSprite(micro.GetComponent<Image>(), styleDWellRecessedSprite, 3.2f, Color.white);
                Transform icFrame = micro.Find("IconFrame");
                if (icFrame is RectTransform ifRt)
                {
                    ifRt.anchoredPosition = new Vector2(12f, -12f);
                    ifRt.sizeDelta = new Vector2(54f, 54f);
                    ApplySlicedSprite(icFrame.GetComponent<Image>(), styleDSlotRecessedSprite, 2.2f, Color.white);
                }
                if (micro.Find("Title") is RectTransform tRt)
                {
                    tRt.offsetMin = new Vector2(74f, -29f);
                    tRt.offsetMax = new Vector2(-12f, -11f);
                }
                if (micro.Find("Stats") is RectTransform sRt)
                {
                    sRt.offsetMin = new Vector2(74f, -45f);
                    sRt.offsetMax = new Vector2(-12f, -29f);
                }
                if (micro.Find("Desc") is RectTransform dRt)
                {
                    dRt.offsetMin = new Vector2(74f, 38f);
                    dRt.offsetMax = new Vector2(-12f, -46f);
                }
                if (micro.Find("Btn_QuickAction") is RectTransform qaRt)
                {
                    qaRt.anchoredPosition = new Vector2(74f, 11f);
                    qaRt.sizeDelta = new Vector2(120f, 22f);
                }
                if (micro.Find("Btn_OpenFullGear") is RectTransform fgRt)
                {
                    fgRt.anchoredPosition = new Vector2(-12f, 11f);
                    fgRt.sizeDelta = new Vector2(104f, 22f);
                }
            }

            if (inspectorTitle != null) inspectorTitle.color = ColStyleDBoneText;
            if (inspectorStats != null) inspectorStats.color = ColStyleDAmber;
            if (inspectorDesc != null) inspectorDesc.color = ColStyleDMutedStone;

            if (inspectorUseEquipBtn != null)
            {
                ApplyButtonSprite(inspectorUseEquipBtn, ColStyleDEquipBtn, ColStyleDEquipBtn * 1.25f, 3.0f, ColStyleDBoneText);
            }
            if (inspectorOpenGearBtn != null)
            {
                ApplyButtonSprite(inspectorOpenGearBtn, ColStyleDDropBtn, ColStyleDDropBtn * 1.25f, 3.0f, new Color(0.96f, 0.80f, 0.78f, 1f));
            }
        }

        private static void RepositionAndStyleVitalRow(Transform idCard, string rowName, float topY, float height, Color barColor)
        {
            Transform row = idCard.Find(rowName);
            if (row == null) return;
            RectTransform rt = row as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(112f, topY - height);
            rt.offsetMax = new Vector2(-12f, topY);

            var nameTmp = row.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameTmp != null) nameTmp.color = ColStyleDBoneText;

            var valTmp = row.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (valTmp != null) valTmp.color = ColStyleDBoneText;

            var trImg = row.Find("Track")?.GetComponent<Image>();
            if (trImg != null) trImg.color = new Color(0.035f, 0.042f, 0.050f, 1f);

            var fillImg = row.Find("Track/Fill")?.GetComponent<Image>();
            if (fillImg != null) fillImg.color = barColor;
        }

        private static void StyleSurvivalChipMatte(Transform chipTr, float xMin, float xMax)
        {
            if (chipTr == null) return;
            if (chipTr is RectTransform cRt)
            {
                cRt.offsetMin = new Vector2(xMin, -146f);
                cRt.offsetMax = new Vector2(xMax, -125f);
            }
            var cImg = chipTr.GetComponent<Image>();
            if (cImg != null)
            {
                cImg.sprite = null;
                cImg.type = Image.Type.Simple;
                cImg.color = new Color(0.038f, 0.044f, 0.052f, 1f);
            }
            var cOl = chipTr.GetComponent<Outline>() ?? chipTr.gameObject.AddComponent<Outline>();
            cOl.enabled = true;
            cOl.effectColor = new Color(0.22f, 0.24f, 0.27f, 0.85f);
            cOl.effectDistance = new Vector2(1f, -1f);

            var valTmp = chipTr.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (valTmp != null)
            {
                valTmp.fontSize = 8.5f;
                valTmp.alignment = TextAlignmentOptions.MidlineLeft;
                valTmp.overflowMode = TextOverflowModes.Ellipsis;
                valTmp.rectTransform.offsetMin = new Vector2(20f, 1f);
                valTmp.rectTransform.offsetMax = new Vector2(-4f, -1f);
            }
        }

        private void StyleStyleDSurvivorPanel()
        {
            if (topRightSurvivorPanel == null) return;
            StyleOuterPanelChassis(topRightSurvivorPanel, "SURVIVOR");

            Transform body = topRightSurvivorPanel.Find("Body");
            if (body != null)
            {
                var bodyVlg = body.GetComponent<VerticalLayoutGroup>();
                if (bodyVlg != null) bodyVlg.spacing = 6f;
            }
            Transform idCard = body != null ? body.Find("IdentityCard") : null;
            if (idCard != null)
            {
                var idLe = idCard.GetComponent<LayoutElement>();
                if (idLe != null)
                {
                    idLe.preferredHeight = 176f;
                    idLe.minHeight = 176f;
                }

                ApplySlicedSprite(idCard.GetComponent<Image>(), styleDWellRecessedSprite, 3.2f, Color.white);
                Transform pfTr = idCard.Find("PortraitFrame");
                if (pfTr is RectTransform pfRt)
                {
                    pfRt.anchoredPosition = new Vector2(12f, 0f);
                    pfRt.sizeDelta = new Vector2(90f, 146f);
                    SetOutlineEnabled(pfTr, false);

                    var pfImg = pfTr.GetComponent<Image>();
                    if (pfImg != null)
                    {
                        pfImg.sprite = null;
                        pfImg.type = Image.Type.Simple;
                        pfImg.color = new Color(0.02f, 0.024f, 0.03f, 1f);
                        pfImg.raycastTarget = false;
                    }
                    if (pfTr.GetComponent<RectMask2D>() == null)
                    {
                        pfTr.gameObject.AddComponent<RectMask2D>();
                    }

                    Transform avTr = pfTr.Find("Avatar") ?? survivorPortraitImg?.transform;
                    if (avTr != null)
                    {
                        if (avTr is RectTransform avRt)
                        {
                            StretchFull(avRt, 5f, 5f, 5f, 5f);
                        }
                        var avImg = avTr.GetComponent<Image>() ?? survivorPortraitImg;
                        if (avImg != null)
                        {
                            avImg.preserveAspect = false;
                            avImg.raycastTarget = false;
                        }
                        avTr.SetSiblingIndex(0);
                    }

                    Transform borderTr = pfTr.Find("StyleD_PortraitBorderOverlay");
                    GameObject borderGo = borderTr != null
                        ? borderTr.gameObject
                        : new GameObject("StyleD_PortraitBorderOverlay", typeof(RectTransform), typeof(Image));
                    if (borderTr == null) borderGo.transform.SetParent(pfTr, false);
                    StretchFull(borderGo.GetComponent<RectTransform>());
                    Image borderImg = borderGo.GetComponent<Image>();
                    borderImg.sprite = styleDSlotRecessedSprite;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.fillCenter = false;
                    borderImg.pixelsPerUnitMultiplier = 2.6f;
                    borderImg.color = Color.white;
                    borderImg.raycastTarget = false;
                    borderGo.transform.SetAsLastSibling();
                }

                if (survivorNameText != null)
                {
                    survivorNameText.color = ColStyleDBoneText;
                    survivorNameText.fontSize = 12.5f;
                    survivorNameText.rectTransform.offsetMin = new Vector2(112f, -25f);
                    survivorNameText.rectTransform.offsetMax = new Vector2(-12f, -9f);
                }
                if (survivorJobText != null)
                {
                    survivorJobText.color = ColStyleDAmber;
                    survivorJobText.fontSize = 8.8f;
                    survivorJobText.rectTransform.offsetMin = new Vector2(112f, -38f);
                    survivorJobText.rectTransform.offsetMax = new Vector2(-12f, -25f);
                }

                RepositionAndStyleVitalRow(idCard, "Vital_HEALTH", -41f, 13f, new Color(0.72f, 0.20f, 0.20f, 1f));
                RepositionAndStyleVitalRow(idCard, "Vital_STAMINA", -56f, 13f, new Color(0.78f, 0.56f, 0.16f, 1f));
                RepositionAndStyleVitalRow(idCard, "Vital_HUMANITY", -71f, 13f, new Color(0.48f, 0.35f, 0.65f, 1f));

                // Live ECG Bio-Monitor Well
                Transform ecgWellTr = idCard.Find("StyleD_ECGWell");
                GameObject ecgWell = ecgWellTr != null ? ecgWellTr.gameObject : new GameObject("StyleD_ECGWell", typeof(RectTransform), typeof(Image));
                if (ecgWellTr == null) ecgWell.transform.SetParent(idCard, false);

                RectTransform ewRt = ecgWell.GetComponent<RectTransform>();
                ewRt.anchorMin = new Vector2(0f, 1f);
                ewRt.anchorMax = new Vector2(1f, 1f);
                ewRt.pivot = new Vector2(0f, 1f);
                ewRt.offsetMin = new Vector2(112f, -120f);
                ewRt.offsetMax = new Vector2(-12f, -88f);
                ApplySlicedSprite(ecgWell.GetComponent<Image>(), styleDWellRecessedSprite, 3.2f, new Color(0.90f, 0.98f, 0.94f, 1f));

                Transform ecgTr = ecgWell.transform.Find("EcgGraphic");
                GameObject ecgGo = ecgTr != null ? ecgTr.gameObject : new GameObject("EcgGraphic", typeof(RectTransform), typeof(CanvasRenderer), typeof(EcgWaveformGraphic));
                if (ecgTr == null) ecgGo.transform.SetParent(ecgWell.transform, false);
                StretchFull(ecgGo.GetComponent<RectTransform>(), 4f, 4f, 4f, 4f);
                _styleDEcg = ecgGo.GetComponent<EcgWaveformGraphic>();

                // Clean matte dark inset Hunger & Thirst chips (zero 9-slice vertical clipping)
                StyleSurvivalChipMatte(idCard.Find("Chip_Hunger"), 112f, 219f);
                StyleSurvivalChipMatte(idCard.Find("Chip_Thirst"), 225f, 332f);

                Transform xpTr = idCard.Find("XPBarTrack");
                if (xpTr is RectTransform xRt)
                {
                    xRt.offsetMin = new Vector2(112f, -165f);
                    xRt.offsetMax = new Vector2(-12f, -152f);
                    var xtImg = xpTr.GetComponent<Image>();
                    if (xtImg != null) xtImg.color = new Color(0.035f, 0.042f, 0.050f, 1f);
                }
                if (xpFillImg != null) xpFillImg.color = ColStyleDAmber;
                if (xpValueText != null) xpValueText.color = ColStyleDBoneText;
            }

            Transform tabsCard = body != null ? body.Find("SurvivorTabsContainer") : null;
            if (tabsCard != null)
            {
                // Move sunken well frame onto TabBody (matching Story Reader) so TabBar sits cleanly above the well
                var tcImg = tabsCard.GetComponent<Image>();
                if (tcImg != null)
                {
                    tcImg.sprite = null;
                    tcImg.color = Color.clear;
                }
                SetOutlineEnabled(tabsCard, false);

                Transform tabBar = tabsCard.Find("TabBar");
                if (tabBar != null)
                {
                    if (tabBar is RectTransform tbRt)
                    {
                        tbRt.offsetMin = new Vector2(0f, -26f);
                        tbRt.offsetMax = Vector2.zero;
                    }
                    var stBarImg = tabBar.GetComponent<Image>();
                    if (stBarImg != null) stBarImg.color = Color.clear;
                    var stHlg = tabBar.GetComponent<HorizontalLayoutGroup>();
                    if (stHlg != null)
                    {
                        stHlg.padding = new RectOffset(0, 0, 0, 0);
                        stHlg.spacing = 4f;
                    }
                }

                for (int i = 0; i < survivorTabButtons.Count; i++)
                {
                    ApplyButtonSprite(survivorTabButtons[i], ColStyleDTabInactive, ColStyleDRust, 3.2f);
                }

                Transform tabBody = tabsCard.Find("TabBody");
                if (tabBody != null)
                {
                    if (tabBody is RectTransform tbodyRt)
                    {
                        tbodyRt.offsetMin = Vector2.zero;
                        tbodyRt.offsetMax = new Vector2(0f, -30f);
                    }
                    Image wellImg = tabBody.GetComponent<Image>() ?? tabBody.gameObject.AddComponent<Image>();
                    wellImg.raycastTarget = false;
                    ApplySlicedSprite(wellImg, styleDWellRecessedSprite, 3.2f, Color.white);

                    foreach (string viewName in new[] { "View_Mortal", "View_Skills", "View_Traits", "View_TeamGear" })
                    {
                        Transform vTr = tabBody.Find(viewName);
                        if (vTr is RectTransform vRt) StretchFull(vRt, 10f, 10f, 10f, 10f);
                    }
                }
            }

            if (survivorTabMortalView != null)
            {
                Transform mHdr = survivorTabMortalView.transform.Find("SubHeader");
                if (mHdr != null) mHdr.gameObject.SetActive(false);
                Transform mList = survivorTabMortalView.transform.Find("MortalList");
                if (mList != null) mList.gameObject.SetActive(false);

                BuildStyleDMortalGridAndCondition(survivorTabMortalView.transform);
            }

            StyleSkillsTraitsAndGearViews();
        }

        private void BuildStyleDMortalGridAndCondition(Transform viewMortal)
        {
            if (viewMortal == null) return;

            Transform existingGrid = viewMortal.Find("StyleD_MortalGrid");
            if (existingGrid != null)
            {
                _styleDMortalGridObj = existingGrid.gameObject;
                for (int i = existingGrid.childCount - 1; i >= 0; i--)
                {
                    GameObject c = existingGrid.GetChild(i).gameObject;
                    c.transform.SetParent(null, false);
                    if (Application.isPlaying) Destroy(c);
                    else DestroyImmediate(c);
                }
            }
            else
            {
                _styleDMortalGridObj = new GameObject("StyleD_MortalGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                _styleDMortalGridObj.transform.SetParent(viewMortal, false);
            }

            RectTransform gridRt = _styleDMortalGridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.offsetMin = new Vector2(4f, 38f);
            gridRt.offsetMax = new Vector2(-4f, -4f);

            GridLayoutGroup glg = _styleDMortalGridObj.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(96f, 134f);
            glg.spacing = new Vector2(8f, 8f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment = TextAnchor.MiddleCenter;

            _styleDMortalValueTexts.Clear();
            (string code, string name)[] stats =
            {
                ("MGT", "MIGHT"),
                ("OBS", "OBSERVE"),
                ("RFL", "REFLEX"),
                ("TAC", "TACTICS"),
                ("AUS", "AUSPICE"),
                ("LDR", "LEADER")
            };

            foreach (var st in stats)
            {
                GameObject tile = new GameObject($"StatTile_{st.code}", typeof(RectTransform), typeof(Image));
                tile.transform.SetParent(_styleDMortalGridObj.transform, false);
                ApplySlicedSprite(tile.GetComponent<Image>(), styleDStatCardSprite, 3.2f, Color.white);

                TextMeshProUGUI codeTmp = CreateText(tile.transform, "Code", st.code, 12.5f, new Color(0.761f, 0.737f, 0.690f, 1f),
                    TextAlignmentOptions.Top, FontStyles.Bold, FontRole.Display, wrap: false);
                codeTmp.rectTransform.anchorMin = new Vector2(0f, 0.68f);
                codeTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
                codeTmp.rectTransform.offsetMin = new Vector2(4f, 0f);
                codeTmp.rectTransform.offsetMax = new Vector2(-4f, -11f);

                TextMeshProUGUI valTmp = CreateText(tile.transform, "Value", "0", 34f, new Color(0.937f, 0.914f, 0.863f, 1f),
                    TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Display, wrap: false);
                valTmp.rectTransform.anchorMin = new Vector2(0f, 0.22f);
                valTmp.rectTransform.anchorMax = new Vector2(1f, 0.74f);
                valTmp.rectTransform.offsetMin = Vector2.zero;
                valTmp.rectTransform.offsetMax = Vector2.zero;
                _styleDMortalValueTexts[st.code] = valTmp;

                TextMeshProUGUI subTmp = CreateText(tile.transform, "Sub", st.name, 8.5f, new Color(0.490f, 0.471f, 0.431f, 1f),
                    TextAlignmentOptions.Bottom, FontStyles.Bold, FontRole.Mono, wrap: false);
                subTmp.rectTransform.anchorMin = new Vector2(0f, 0f);
                subTmp.rectTransform.anchorMax = new Vector2(1f, 0.26f);
                subTmp.rectTransform.offsetMin = new Vector2(4f, 10f);
                subTmp.rectTransform.offsetMax = new Vector2(-4f, 0f);
            }

            Transform condTr = viewMortal.Find("StyleD_ConditionBar");
            GameObject condGo = condTr != null ? condTr.gameObject : new GameObject("StyleD_ConditionBar", typeof(RectTransform), typeof(Image));
            if (condTr == null) condGo.transform.SetParent(viewMortal, false);

            RectTransform cRt = condGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 0f);
            cRt.pivot = new Vector2(0.5f, 0f);
            cRt.offsetMin = new Vector2(4f, 4f);
            cRt.offsetMax = new Vector2(-4f, 32f);
            ApplySlicedSprite(condGo.GetComponent<Image>(), styleDWellRecessedSprite, 3.2f, Color.white);

            TextMeshProUGUI existingCondTmp = condGo.GetComponentInChildren<TextMeshProUGUI>(true);
            if (existingCondTmp != null)
            {
                _styleDConditionText = existingCondTmp;
                _styleDConditionText.text = "CONDITION: <color=#3AD679>STABLE</color>";
                _styleDConditionText.color = ColStyleDMutedStone;
            }
            else
            {
                _styleDConditionText = CreateText(condGo.transform, "Label", "CONDITION: <color=#3AD679>STABLE</color>", 10f,
                    ColStyleDMutedStone, TextAlignmentOptions.Center, FontStyles.Bold, FontRole.Mono, wrap: false);
                StretchFull(_styleDConditionText.rectTransform, 6f, 0f, 6f, 0f);
            }
        }

        private void SetStyleDMortalValue(string key, int val)
        {
            if (_styleDMortalValueTexts.TryGetValue(key, out TextMeshProUGUI tmp) && tmp != null)
            {
                tmp.text = val.ToString();
            }
        }

        private void StyleSkillsTraitsAndGearViews()
        {
            if (survivorTabSkillsView != null)
            {
                var skHdr = survivorTabSkillsView.transform.Find("SubHeader")?.GetComponent<TextMeshProUGUI>();
                if (skHdr != null)
                {
                    skHdr.text = "SURVIVAL SKILLS (0 - 5)";
                    skHdr.color = ColStyleDMutedStone;
                }

                Transform skList = survivorTabSkillsView.transform.Find("SkillsList");
                if (skList != null)
                {
                    for (int i = 0; i < skList.childCount; i++)
                    {
                        Transform row = skList.GetChild(i);
                        ApplySlicedSprite(row.GetComponent<Image>(), styleDStatCardSprite, 3.8f, Color.white);
                        var t = row.Find("Title")?.GetComponent<TextMeshProUGUI>();
                        if (t != null) t.color = ColStyleDBoneText;
                        var d = row.Find("Desc")?.GetComponent<TextMeshProUGUI>();
                        if (d != null) d.color = ColStyleDMutedStone;
                        var track = row.Find("Track")?.GetComponent<Image>();
                        if (track != null) track.color = new Color(0.035f, 0.042f, 0.050f, 1f);
                    }
                }
                for (int i = 0; i < skillRows.Count; i++)
                {
                    if (skillRows[i]?.fillImage != null) skillRows[i].fillImage.color = ColStyleDAmber;
                    if (skillRows[i]?.valueText != null) skillRows[i].valueText.color = ColStyleDAmber;
                }
            }

            if (traitsCountHeaderText != null)
            {
                traitsCountHeaderText.text = StripLeadingSlashes(traitsCountHeaderText.text);
                traitsCountHeaderText.color = ColStyleDMutedStone;
            }

            if (survivorTabTeamGearView != null)
            {
                var tgVlg = survivorTabTeamGearView.GetComponent<VerticalLayoutGroup>();
                if (tgVlg != null) tgVlg.spacing = 5f;

                var gh = survivorTabTeamGearView.transform.Find("GearHeader")?.GetComponent<TextMeshProUGUI>();
                if (gh != null)
                {
                    gh.text = "EQUIPPED LOADOUT";
                    gh.color = ColStyleDMutedStone;
                }

                foreach (string rowName in new[] { "EquipSlot_WPN", "ArmorRow_Top", "ArmorRow_Bottom" })
                {
                    Transform arTr = survivorTabTeamGearView.transform.Find(rowName);
                    if (arTr != null && arTr.GetComponent<LayoutElement>() is LayoutElement arLe)
                    {
                        arLe.preferredHeight = 48f;
                        arLe.minHeight = 48f;
                    }
                }

                for (int i = 0; i < equippedGearSlots.Count; i++)
                {
                    var eqSlot = equippedGearSlots[i];
                    if (eqSlot == null) continue;
                    if (eqSlot.bgImage != null && eqSlot.bgImage.GetComponent<LayoutElement>() is LayoutElement eqLe)
                    {
                        eqLe.preferredHeight = 48f;
                        eqLe.minHeight = 48f;
                    }
                    ApplySlicedSprite(eqSlot.bgImage, styleDStatCardSprite, 3.8f, Color.white);
                    if (eqSlot.button != null) ConfigureButtonColors(eqSlot.button, Color.white, new Color(1.12f, 1.08f, 1.0f, 1f));
                    Transform slotTr = eqSlot.bgImage != null ? eqSlot.bgImage.transform : null;
                    if (slotTr != null)
                    {
                        float iconSz = eqSlot.slotTag == "WPN" ? 32f : 28f;
                        Transform icF = slotTr.Find("IconFrame");
                        if (icF is RectTransform icRt)
                        {
                            icRt.anchoredPosition = new Vector2(8f, 0f);
                            icRt.sizeDelta = new Vector2(iconSz, iconSz);
                            ApplySlicedSprite(icF.GetComponent<Image>(), styleDSlotRecessedSprite, 2.2f, Color.white);
                        }
                        var tag = slotTr.Find("SlotTag")?.GetComponent<TextMeshProUGUI>();
                        if (tag != null)
                        {
                            tag.text = tag.text.Replace(" // ", ": ");
                            tag.color = ColStyleDAmber;
                            tag.rectTransform.anchorMin = new Vector2(0f, 0.50f);
                            tag.rectTransform.anchorMax = Vector2.one;
                            tag.rectTransform.offsetMin = new Vector2(tag.rectTransform.offsetMin.x, 0f);
                            tag.rectTransform.offsetMax = new Vector2(-8f, -7f);
                        }
                        if (eqSlot.itemNameText != null)
                        {
                            eqSlot.itemNameText.rectTransform.anchorMin = Vector2.zero;
                            eqSlot.itemNameText.rectTransform.anchorMax = new Vector2(1f, 0.52f);
                            eqSlot.itemNameText.rectTransform.offsetMin = new Vector2(eqSlot.itemNameText.rectTransform.offsetMin.x, 7f);
                            eqSlot.itemNameText.rectTransform.offsetMax = new Vector2(-8f, 0f);
                        }
                    }
                }

                if (partyHeaderText != null)
                {
                    partyHeaderText.text = "SURVIVOR GROUP (1 / 4)";
                    partyHeaderText.color = ColStyleDAmber;
                    if (partyHeaderText.GetComponent<LayoutElement>() is LayoutElement phLe)
                    {
                        phLe.preferredHeight = 16f;
                        phLe.minHeight = 16f;
                    }
                }

                Transform pCard = survivorTabTeamGearView.transform.Find("PartyGroupCard");
                if (pCard != null)
                {
                    ApplySlicedSprite(pCard.GetComponent<Image>(), styleDWellRecessedSprite, 3.4f, Color.white);
                    var pVlg = pCard.GetComponent<VerticalLayoutGroup>();
                    if (pVlg != null)
                    {
                        pVlg.padding = new RectOffset(6, 6, 6, 6);
                        pVlg.spacing = 4f;
                    }
                    for (int i = 0; i < pCard.childCount; i++)
                    {
                        Transform pRow = pCard.GetChild(i);
                        ApplySlicedSprite(pRow.GetComponent<Image>(), styleDStatCardSprite, 4.0f, i == 0 ? Color.white : new Color(0.88f, 0.88f, 0.88f, 0.90f));
                        var nTmp = pRow.Find("Name")?.GetComponent<TextMeshProUGUI>();
                        if (nTmp != null)
                        {
                            nTmp.text = nTmp.text.Replace(" // ", " - ");
                            nTmp.color = i == 0 ? ColStyleDBoneText : ColStyleDMutedStone;
                            nTmp.rectTransform.offsetMin = new Vector2(10f, 0f);
                        }
                        var mTmp = pRow.Find("Meta")?.GetComponent<TextMeshProUGUI>();
                        if (mTmp != null)
                        {
                            mTmp.rectTransform.offsetMax = new Vector2(-10f, 0f);
                        }
                    }
                }
            }
        }

        private void StyleStyleDMapAndNotePanel()
        {
            if (bottomLeftMapPanel == null) return;
            StyleOuterPanelChassis(bottomLeftMapPanel, "4F MAP");

            Transform body = bottomLeftMapPanel.Find("Body");
            if (body == null) return;

            Transform miniMap = body.Find("MiniMapCanvas");
            if (miniMap != null)
            {
                if (miniMap is RectTransform mmRt)
                {
                    mmRt.offsetMin = new Vector2(0f, 84f);
                    mmRt.offsetMax = Vector2.zero;
                }
                Image mmImg = miniMap.GetComponent<Image>();
                if (mmImg != null && styleDMapPaperSprite != null)
                {
                    mmImg.sprite = styleDMapPaperSprite;
                    mmImg.type = Image.Type.Simple;
                    mmImg.color = Color.white;
                }
                SetOutlineEnabled(miniMap, false);

                Transform bp = miniMap.Find("BlueprintViewport");
                if (bp != null)
                {
                    Image bpImg = bp.GetComponent<Image>();
                    if (bpImg != null) bpImg.color = Color.clear;

                    Transform gridOverlay = bp.Find("BlueprintGridOverlay");
                    if (gridOverlay != null) gridOverlay.gameObject.SetActive(false);

                    Transform fpCont = bp.Find("FloorplanContainer");
                    if (fpCont is RectTransform fcRt)
                    {
                        fcRt.offsetMin = new Vector2(14f, 22f);
                        fcRt.offsetMax = new Vector2(-14f, -24f);
                    }

                    Transform subHdr = bp.Find("BlueprintSubHeader");
                    if (subHdr != null)
                    {
                        Image shBg = subHdr.GetComponent<Image>() ?? subHdr.gameObject.AddComponent<Image>();
                        shBg.color = new Color(0.05f, 0.055f, 0.065f, 0.92f);
                        shBg.raycastTarget = false;
                        if (subHdr is RectTransform shRt)
                        {
                            shRt.anchorMin = new Vector2(0f, 1f);
                            shRt.anchorMax = new Vector2(1f, 1f);
                            shRt.offsetMin = new Vector2(46f, -24f);
                            shRt.offsetMax = new Vector2(-46f, -6f);
                        }
                        var apTmp = subHdr.Find("ActivePosBanner")?.GetComponent<TextMeshProUGUI>();
                        if (apTmp != null)
                        {
                            apTmp.alignment = TextAlignmentOptions.Center;
                            apTmp.fontSize = 8.2f;
                            apTmp.rectTransform.offsetMin = new Vector2(6f, 1f);
                            apTmp.rectTransform.offsetMax = new Vector2(-6f, -1f);
                        }
                    }

                    Transform legBar = bp.Find("LegendBar");
                    if (legBar != null)
                    {
                        Image lbBg = legBar.GetComponent<Image>() ?? legBar.gameObject.AddComponent<Image>();
                        lbBg.color = new Color(0.05f, 0.058f, 0.068f, 0.98f);
                        lbBg.raycastTarget = false;
                        if (legBar is RectTransform lbRt)
                        {
                            lbRt.anchorMin = Vector2.zero;
                            lbRt.anchorMax = new Vector2(1f, 0f);
                            lbRt.offsetMin = Vector2.zero;
                            lbRt.offsetMax = new Vector2(0f, 17f);
                        }
                        var lTmp = legBar.Find("LegendText")?.GetComponent<TextMeshProUGUI>();
                        if (lTmp != null)
                        {
                            lTmp.text = "<color=#D95829>■</color> CURRENT  <color=#8C8578>■</color> VISITED  <color=#2B2823>■</color> UNVISITED  <color=#C94A29>■</color> LOCKED  <color=#CF9232>■</color> BLOCKED";
                            lTmp.color = ColStyleDBoneText;
                        }
                    }
                }
            }

            Transform noteTr = body.Find("PinnedNoteCard");
            if (noteTr != null)
            {
                if (noteTr is RectTransform nRt)
                {
                    nRt.offsetMin = Vector2.zero;
                    nRt.offsetMax = new Vector2(0f, 78f);
                }
                Image nImg = noteTr.GetComponent<Image>();
                if (nImg != null && styleDPinnedNoteSprite != null)
                {
                    ApplySlicedSprite(nImg, styleDPinnedNoteSprite, 2.2f, Color.white);
                    if (nImg.canvasRenderer != null) nImg.canvasRenderer.SetColor(Color.white);
                }
                SetOutlineEnabled(noteTr, false);
                if (pinnedNoteBtn != null)
                {
                    ConfigureButtonColors(pinnedNoteBtn, Color.white, new Color(0.95f, 0.92f, 0.84f, 1f));
                }

                // Separate 1:1 Pushpin Image anchored onto top paper margin so it sits firmly on the card
                if (styleDRedPushpinSprite != null)
                {
                    Transform pinTr = noteTr.Find("StyleD_RedPushpin");
                    GameObject pinGo = pinTr != null ? pinTr.gameObject : new GameObject("StyleD_RedPushpin", typeof(RectTransform), typeof(Image));
                    if (pinTr == null) pinGo.transform.SetParent(noteTr, false);
                    RectTransform pinRt = pinGo.GetComponent<RectTransform>();
                    pinRt.anchorMin = new Vector2(0.5f, 1f);
                    pinRt.anchorMax = new Vector2(0.5f, 1f);
                    pinRt.pivot = new Vector2(0.5f, 0.5f);
                    pinRt.anchoredPosition = new Vector2(0f, -11f);
                    pinRt.sizeDelta = new Vector2(18f, 18f);
                    Image pinImg = pinGo.GetComponent<Image>();
                    pinImg.sprite = styleDRedPushpinSprite;
                    pinImg.type = Image.Type.Simple;
                    pinImg.preserveAspect = true;
                    pinImg.color = Color.white;
                    pinImg.raycastTarget = false;
                    pinGo.transform.SetAsLastSibling();
                }

                var tagTmp = noteTr.Find("Tag")?.GetComponent<TextMeshProUGUI>();
                if (tagTmp != null)
                {
                    SundayTypography.ApplyFont(tagTmp, FontRole.Typewriter);
                    tagTmp.text = "OBJECTIVE:";
                    tagTmp.color = ColStyleDPaperHeader;
                    tagTmp.fontStyle = FontStyles.Bold;
                    tagTmp.fontSize = 10.8f;
                    tagTmp.rectTransform.offsetMin = new Vector2(16f, -26f);
                    tagTmp.rectTransform.offsetMax = new Vector2(-16f, -10f);
                }

                var bodyTmp = noteTr.Find("Body")?.GetComponent<TextMeshProUGUI>() ?? pinnedNoteText;
                if (bodyTmp != null)
                {
                    SundayTypography.ApplyFont(bodyTmp, FontRole.Typewriter);
                    bodyTmp.color = ColStyleDPaperInk;
                    bodyTmp.fontStyle = FontStyles.Bold;
                    bodyTmp.fontSize = 11f;
                    bodyTmp.rectTransform.offsetMin = new Vector2(16f, 8f);
                    bodyTmp.rectTransform.offsetMax = new Vector2(-16f, -27f);
                }
            }

            if (hubScreen != null && hubScreen.TacticalMap != null)
            {
                hubScreen.TacticalMap.SetHorizontalMiniMapMode(true, 0.88f);
                hubScreen.TacticalMap.RefreshAllNodes();
            }
        }

        private void EnsureStyleDChoiceDeckSpacing()
        {
            if (!useStyleDHardwareTheme || hubScreen == null || hubScreen.HubMenuContainer == null) return;
            Transform root = hubScreen.HubMenuContainer.transform;
            var rootVlg = root.GetComponent<VerticalLayoutGroup>();
            if (rootVlg != null && (rootVlg.padding.left != 10 || rootVlg.spacing != 6f))
            {
                rootVlg.padding = new RectOffset(10, 10, 10, 10);
                rootVlg.spacing = 6f;
            }
            Transform contentTr = root.Find("ChoiceScrollView/Viewport/Content");
            if (contentTr != null)
            {
                var cVlg = contentTr.GetComponent<VerticalLayoutGroup>();
                if (cVlg != null && (cVlg.spacing != 6f || cVlg.padding.left != 4 || cVlg.padding.top != 4))
                {
                    cVlg.padding = new RectOffset(4, 6, 4, 4);
                    cVlg.spacing = 6f;
                    if (contentTr is RectTransform cRt) LayoutRebuilder.ForceRebuildLayoutImmediate(cRt);
                }
            }
        }

        private void StyleStyleDReaderAndDeck()
        {
            if (bottomCenterReaderPanel != null)
            {
                ApplySlicedSprite(bottomCenterReaderPanel.GetComponent<Image>(), styleDPanelChassisSprite, 3.2f, Color.white);

                Transform tabBar = bottomCenterReaderPanel.Find("TabBar");
                if (tabBar != null)
                {
                    if (tabBar is RectTransform tbRt)
                    {
                        tbRt.anchorMin = new Vector2(0f, 1f);
                        tbRt.anchorMax = new Vector2(1f, 1f);
                        tbRt.offsetMin = new Vector2(18f, -44f);
                        tbRt.offsetMax = new Vector2(-18f, -18f);
                    }
                    Image tbImg = tabBar.GetComponent<Image>();
                    if (tbImg != null) tbImg.color = new Color(0f, 0f, 0f, 0f);
                    var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.padding = new RectOffset(0, 108, 0, 0);
                        hlg.spacing = 5f;
                    }
                    Transform fgTr = tabBar.Find("FontSizeControlGroup");
                    if (fgTr is RectTransform fgRt)
                    {
                        fgRt.anchoredPosition = Vector2.zero;
                        fgRt.sizeDelta = new Vector2(102f, 26f);
                    }

                    for (int i = 0; i < readerTabButtons.Count; i++)
                    {
                        ApplyButtonSprite(readerTabButtons[i], ColStyleDTabInactive, ColStyleDRust, 3.2f);
                    }

                    ApplyButtonSprite(_fontDecBtn, ColStyleDMetalBtn, ColStyleDMetalBtn * 1.25f, 3.6f, ColStyleDBoneText);
                    ApplyButtonSprite(_fontResetBtn, ColStyleDMetalBtn, ColStyleDMetalBtn * 1.25f, 3.6f, ColStyleDAmber);
                    ApplyButtonSprite(_fontIncBtn, ColStyleDMetalBtn, ColStyleDMetalBtn * 1.25f, 3.6f, ColStyleDBoneText);
                }

                Transform readerBody = bottomCenterReaderPanel.Find("ReaderBody");
                if (readerBody != null)
                {
                    if (readerBody is RectTransform rbRt)
                    {
                        rbRt.anchorMin = Vector2.zero;
                        rbRt.anchorMax = Vector2.one;
                        rbRt.offsetMin = new Vector2(18f, 18f);
                        rbRt.offsetMax = new Vector2(-18f, -48f);
                    }

                    Image wellImg = readerBody.GetComponent<Image>() ?? readerBody.gameObject.AddComponent<Image>();
                    wellImg.raycastTarget = false;
                    ApplySlicedSprite(wellImg, styleDWellRecessedSprite, 3.2f, Color.white);

                    foreach (string viewName in new[] { "View_Situation", "View_StoryLog", "View_Notes", "View_Floor" })
                    {
                        Transform vTr = readerBody.Find(viewName);
                        if (vTr is RectTransform vRt) StretchFull(vRt, 10f, 10f, 10f, 10f);
                    }

                    if (inlineStoryLogScroll != null && inlineStoryLogScroll.content != null)
                    {
                        var slVlg = inlineStoryLogScroll.content.GetComponent<VerticalLayoutGroup>();
                        if (slVlg != null) slVlg.padding = new RectOffset(10, 14, 10, 44);
                    }
                    Transform notesContent = readerBody.Find("View_Notes/NotesScroll/Viewport/Content");
                    if (notesContent != null)
                    {
                        var nVlg = notesContent.GetComponent<VerticalLayoutGroup>();
                        if (nVlg != null) nVlg.padding = new RectOffset(10, 14, 10, 10);
                    }

                    if (situationScrollContent != null)
                    {
                        var sVlg = situationScrollContent.GetComponent<VerticalLayoutGroup>();
                        if (sVlg != null) sVlg.padding = new RectOffset(4, 8, 4, 4);

                        Transform sbTr = situationScrollContent.Find("SituationBox");
                        if (sbTr != null)
                        {
                            SetOutlineEnabled(sbTr, false);
                            var sbImg = sbTr.GetComponent<Image>();
                            if (sbImg != null) sbImg.color = Color.clear;
                            var goldBar = sbTr.Find("GoldBar")?.GetComponent<Image>();
                            if (goldBar != null) goldBar.color = ColStyleDRustBright;
                            var logBtn = sbTr.Find("TopRow/Btn_OpenStoryLog")?.GetComponent<Button>();
                            ApplyButtonSprite(logBtn, ColStyleDMetalBtn, ColStyleDRust, 3.0f, ColStyleDBoneText);
                            var logTmp = logBtn != null ? logBtn.GetComponentInChildren<TextMeshProUGUI>(true) : null;
                            if (logTmp != null)
                            {
                                logTmp.text = "<b>[ VIEW LOG ]</b>";
                                logTmp.color = ColStyleDBoneText;
                            }
                            var sitBody = sbTr.Find("SituationBody")?.GetComponent<TextMeshProUGUI>();
                            if (sitBody != null)
                            {
                                SundayTypography.ApplyFont(sitBody, FontRole.Narrative);
                                sitBody.color = ColStyleDBoneText;
                            }
                        }
                    }

                    if (inlineStoryLogText != null)
                    {
                        SundayTypography.ApplyFont(inlineStoryLogText, FontRole.Narrative);
                        inlineStoryLogText.color = ColStyleDBoneText;
                    }
                    if (inlineNotesText != null) inlineNotesText.color = ColStyleDBoneText;
                    if (inlineTakeAllFloorBtn != null)
                    {
                        ApplyButtonSprite(inlineTakeAllFloorBtn, ColStyleDEquipBtn, ColStyleDEquipBtn * 1.25f, 3.0f, ColStyleDBoneText);
                    }
                    if (_jumpToLatestLogBtn != null)
                    {
                        ApplyButtonSprite(_jumpToLatestLogBtn, ColStyleDMetalBtn, ColStyleDRust, 3.0f, ColStyleDAmber);
                    }
                }
            }

            if (bottomRightActionPanel != null)
            {
                StyleOuterPanelChassis(bottomRightActionPanel, "ACTIONS");

                if (choicesViewport != null)
                {
                    Image cvImg = choicesViewport.GetComponent<Image>() ?? choicesViewport.gameObject.AddComponent<Image>();
                    cvImg.raycastTarget = false;
                    ApplySlicedSprite(cvImg, styleDWellRecessedSprite, 3.2f, Color.white);

                    if (hubScreen != null && hubScreen.HubMenuContainer != null)
                    {
                        EnsureStyleDChoiceDeckSpacing();
                        foreach (var tcb in hubScreen.HubMenuContainer.GetComponentsInChildren<TacticalChoiceButton>(true))
                        {
                            tcb?.RefreshStyleDTheme();
                        }
                    }
                }
            }
        }

        private void StyleStyleDAwakeningBoxes()
        {
            if (hubScreen == null || hubScreen.AwakeningContainer == null) return;
            Transform aw = hubScreen.AwakeningContainer.transform;

            Transform easTr = aw.Find("Beat1Container/EASAlertBox");
            if (easTr != null)
            {
                ApplySlicedSprite(easTr.GetComponent<Image>(), styleDStatCardSprite, 2.4f, new Color(0.95f, 0.72f, 0.70f, 0.94f), disableOutline: false);
                var ol = easTr.GetComponent<Outline>();
                if (ol != null) ol.effectColor = ColStyleDRustBright;
            }

            Transform monoTr = aw.Find("Beat2Container/MonologueBox");
            if (monoTr != null)
            {
                ApplySlicedSprite(monoTr.GetComponent<Image>(), styleDStatCardSprite, 2.4f, Color.white, disableOutline: true);
                Transform pFrame = monoTr.Find("PortraitFrame") ?? monoTr.Find("SpeakerPortraitFrame");
                if (pFrame != null)
                {
                    ApplySlicedSprite(pFrame.GetComponent<Image>(), styleDSlotRecessedSprite, 2.2f, Color.white, disableOutline: true);
                }
                foreach (var ol in monoTr.GetComponentsInChildren<Outline>(true))
                {
                    if (ol != null) ol.enabled = false;
                }
            }

            Transform tableTr = aw.Find("Beat3Container/TableStashDisplay");
            if (tableTr != null)
            {
                ApplySlicedSprite(tableTr.GetComponent<Image>(), styleDWellRecessedSprite, 2.4f, Color.white, disableOutline: true);

                Transform gb = tableTr.Find("GoldBar");
                if (gb != null) gb.gameObject.SetActive(false);

                VerticalLayoutGroup sVlg = tableTr.GetComponent<VerticalLayoutGroup>();
                if (sVlg != null)
                {
                    sVlg.padding = new RectOffset(14, 14, 8, 10);
                    sVlg.spacing = 4f;
                }

                TextMeshProUGUI hdrTmp = tableTr.Find("Header")?.GetComponent<TextMeshProUGUI>();
                if (hdrTmp != null)
                {
                    hdrTmp.text = "TABLE STASH - CURRENT WORK GEAR & WEAPONS";
                    hdrTmp.fontSize = 11f;
                    hdrTmp.color = ColStyleDAmber;
                    hdrTmp.rectTransform.sizeDelta = new Vector2(0f, 18f);
                }

                Transform gridTr = tableTr.Find("Grid");
                if (gridTr != null)
                {
                    if (gridTr is RectTransform gRt) gRt.sizeDelta = new Vector2(0f, 82f);
                    var hlg = gridTr.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.spacing = 8f;
                        hlg.childAlignment = TextAnchor.MiddleLeft;
                    }
                }
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
