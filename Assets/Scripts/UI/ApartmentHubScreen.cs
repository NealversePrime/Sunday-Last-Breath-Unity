using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using MoreMountains.Feedbacks;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Master Controller for the Apartment Hub (Unit 407 - Prologue).
    /// Combines the 60/40 Top Stage + Bottom VN Dock in the center with
    /// persistent Left & Right Survivor PDA panels (Status, Skills, Traits, Map, Lore,
    /// Active Loadout, Backpack Grid, and Floor Items) filling the widescreen side wings.
    /// </summary>
    public class ApartmentHubScreen : ScreenBase
    {
        [Header("Top HUD (Twine Style)")]
        [SerializeField] private Button pdaMenuButton;
        [SerializeField] private Image hpBarFill;
        [SerializeField] private TextMeshProUGUI hpValueText;
        [SerializeField] private Image staminaBarFill;
        [SerializeField] private TextMeshProUGUI staminaValueText;
        [SerializeField] private Image hungerBarFill;
        [SerializeField] private TextMeshProUGUI hungerValueText;
        [SerializeField] private Image thirstBarFill;
        [SerializeField] private TextMeshProUGUI thirstValueText;
        [SerializeField] private TextMeshProUGUI clockTimeText;
        [SerializeField] private TextMeshProUGUI clockDayText;
        [SerializeField] private Button floorItemsButton;
        [SerializeField] private TextMeshProUGUI floorItemsCountText;

        [Header("3DS Switchable Console (Seam Bezel)")]
        [SerializeField] private Button btnModeNarrative;
        [SerializeField] private Button btnModePDA;
        [SerializeField] private Button btnModeGear;
        [SerializeField] private TextMeshProUGUI btnModeGearLabel;
        [SerializeField] private GameObject btnModeGearBadge;
        [SerializeField] private Image modeIndicatorNarrative;
        [SerializeField] private Image modeIndicatorPDA;
        [SerializeField] private Image modeIndicatorGear;
        [SerializeField] private GameObject dockContainerNarrative;
        [SerializeField] private GameObject dockContainerPDA;
        [SerializeField] private GameObject dockContainerGear;

        [Header("Left Wing: Embedded Survivor PDA")]
        [SerializeField] private Button pdaTabStatusBtn;
        [SerializeField] private Button pdaTabSkillsBtn;
        [SerializeField] private Button pdaTabTraitsBtn;
        [SerializeField] private Button pdaTabMapBtn;
        [SerializeField] private Button pdaTabLoreBtn;
        [SerializeField] private GameObject pdaPanelStatus;
        [SerializeField] private GameObject pdaPanelSkills;
        [SerializeField] private GameObject pdaPanelTraits;
        [SerializeField] private GameObject pdaPanelMap;
        [SerializeField] private GameObject pdaPanelLore;

        [Header("Left PDA - Status Tab")]
        [SerializeField] private TextMeshProUGUI pdaCharNameText;
        private TextMeshProUGUI pdaNameJobText => pdaCharNameText;
        [SerializeField] private TextMeshProUGUI pdaCharSubText;
        [SerializeField] private TextMeshProUGUI pdaActiveLogsText;
        [SerializeField] private TextMeshProUGUI pdaLevelText;
        [SerializeField] private TextMeshProUGUI pdaXpText;
        [SerializeField] private Image pdaXpBarFill;
        [SerializeField] private Image pdaHumanityBarFill;
        [SerializeField] private TextMeshProUGUI pdaHumanityValueText;

        [Header("Tactical Map & Skills View Controllers")]
        [SerializeField] private TacticalMapViewController tacticalMapController;
        [SerializeField] private TacticalSkillsViewController tacticalSkillsController;
        [SerializeField] private TacticalGearViewController tacticalGearController;

        [Header("Left PDA - Skills, Traits & Lore Tabs")]
        [SerializeField] private TextMeshProUGUI pdaAttributesText;
        [SerializeField] private TextMeshProUGUI pdaSkillsText;
        [SerializeField] private Transform pdaTraitsListRoot;
        [SerializeField] private TextMeshProUGUI pdaLoreText;
        [SerializeField] private Slider pdaVolumeSlider;
        [SerializeField] private Button pdaSaveSettingsBtn;

        [Header("Right Wing: Active Loadout, Backpack & Floor Items")]
        [SerializeField] private Image equipHeadIcon;
        [SerializeField] private TextMeshProUGUI equipHeadLabel;
        [SerializeField] private Image equipBodyIcon;
        [SerializeField] private TextMeshProUGUI equipBodyLabel;
        [SerializeField] private Image equipWpnIcon;
        [SerializeField] private TextMeshProUGUI equipWpnLabel;
        [SerializeField] private Image equipLegsIcon;
        [SerializeField] private TextMeshProUGUI equipLegsLabel;
        [SerializeField] private Image equipFeetIcon;
        [SerializeField] private TextMeshProUGUI equipFeetLabel;
        [SerializeField] private TextMeshProUGUI pdaBackpackCountText;
        [SerializeField] private Transform pdaBackpackGrid;
        [SerializeField] private Transform pdaFloorItemsGrid;
        [SerializeField] private TextMeshProUGUI pdaFloorEmptyText;
        [SerializeField] private GameObject floorItemsCard;

        [Header("Stage Visuals (Top 60%)")]
        [SerializeField] private Image stageBackdropImage;
        [SerializeField] private FrameAnimationPlayer livingRoomAnimPlayer;
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI vitalsHpText;
        [SerializeField] private TextMeshProUGUI vitalsStaminaText;

        [Header("Bottom Dock Header")]
        [SerializeField] private TextMeshProUGUI dockHeaderLocation;
        [SerializeField] private TextMeshProUGUI dockHeaderChannel;

        [Header("Narration Box (Bottom 40% - Awakening Sequence)")]
        [SerializeField] private GameObject awakeningContainer;

        [Header("Beat 1: EAS Warning")]
        [SerializeField] private GameObject beat1Container;
        [SerializeField] private TextMeshProUGUI beat1IntroText;
        [SerializeField] private GameObject easAlertBox;
        [SerializeField] private TextMeshProUGUI easAlertText;

        [Header("Beat 2: Reaction Monologue")]
        [SerializeField] private GameObject beat2Container;
        [SerializeField] private TextMeshProUGUI beat2IntroText;
        [SerializeField] private GameObject monologueBox;
        [SerializeField] private Image monologueAvatar;
        [SerializeField] private TextMeshProUGUI monologueSpeakerText;
        [SerializeField] private TextMeshProUGUI monologueSpeechText;
        [SerializeField] private TextMeshProUGUI beat2FootnoteText;

        [Header("Beat 3: Table Stash")]
        [SerializeField] private GameObject beat3Container;
        [SerializeField] private TextMeshProUGUI beat3NarrativeText;
        [SerializeField] private GameObject tableStashDisplay;
        [SerializeField] private Transform tableStashGrid;
        [SerializeField] private GameObject tableEmptyNotice;

        [Header("Prompts & Click Catchers")]
        [SerializeField] private GameObject continuePrompt;
        [SerializeField] private Button fullScreenClickCatcher;

        [Header("Hub Menu (Bottom 40% - Interactive Menu)")]
        [SerializeField] private GameObject hubMenuContainer;
        [SerializeField] private Button packGearButton;
        [SerializeField] private GameObject packedSuccessBanner;
        [SerializeField] private TextMeshProUGUI packedSuccessText;

        [Header("Model A: Domestic Scavenge Actions")]
        [SerializeField] private Button searchKitchenButton;
        [SerializeField] private TextMeshProUGUI searchKitchenLabel;
        [SerializeField] private TextMeshProUGUI searchKitchenSub;

        [SerializeField] private Button searchBathroomButton;
        [SerializeField] private TextMeshProUGUI searchBathroomLabel;
        [SerializeField] private TextMeshProUGUI searchBathroomSub;

        [SerializeField] private Button searchClosetButton;
        [SerializeField] private TextMeshProUGUI searchClosetLabel;
        [SerializeField] private TextMeshProUGUI searchClosetSub;

        [SerializeField] private Button checkPhoneButton;
        [SerializeField] private Button listenRadioButton;
        [SerializeField] private TextMeshProUGUI listenRadioLabel;
        [SerializeField] private TextMeshProUGUI listenRadioSub;
        [SerializeField] private Button hackTerminalButton;
        [SerializeField] private TextMeshProUGUI hackTerminalLabel;
        [SerializeField] private TextMeshProUGUI hackTerminalSub;
        [SerializeField] private Button leaveApartmentButton;

        [Header("Story Log System (Option A)")]
        [SerializeField] private GameObject situationBox;
        [SerializeField] private TextMeshProUGUI situationBoxText;
        [SerializeField] private Button openStoryLogButton;
        [SerializeField] private GameObject storyLogModal;
        [SerializeField] private Button closeStoryLogButton;
        [SerializeField] private Button closeStoryLogBottomButton;
        [SerializeField] private TextMeshProUGUI storyLogContentText;

        [Header("Modal Overlays")]
        [SerializeField] private GameObject phoneModal;
        [SerializeField] private Button closePhoneButton;
        [SerializeField] private GameObject radioModal;
        [SerializeField] private Button closeRadioButton;
        [SerializeField] private GameObject hackingModal;
        [SerializeField] private TextMeshProUGUI hackingTerminalText;
        [SerializeField] private GameObject hackingFinishContainer;
        [SerializeField] private Button closeHackingButton;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource atmosphereSource;
        [SerializeField] private AudioSource ebsBroadcastSource;
        [SerializeField] private AudioSource staticSource;
        [SerializeField] private AudioSource radioSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip atmosphereClip;
        [SerializeField] private AudioClip ebsWarningClip;
        [SerializeField] private AudioClip staticClip;
        [SerializeField] private AudioClip policeRadioClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Input Debounce & Typewriter")]
        [SerializeField] private float clickDebounceDelay = 0.35f;
        [SerializeField] private float typewriterSpeed = 35f;

        // Runtime State
        private CharacterData currentPlayer;
        private GameClock gameClock;
        private ObjectiveSystem objectiveSystem;
        private LoreSystem loreSystem;

        private List<InventoryEntry> tableStash = new List<InventoryEntry>();
        private List<InventoryEntry> floorItems = new List<InventoryEntry>();
        private string equippedHead = "";
        private string equippedBody = "";
        private string equippedWpn = "";
        private string equippedLegs = "";
        private string equippedFeet = "";

        public List<InventoryEntry> TableStash
        {
            get => tableStash;
            set => tableStash = value ?? new List<InventoryEntry>();
        }

        public List<InventoryEntry> FloorItems
        {
            get => floorItems;
            set => floorItems = value ?? new List<InventoryEntry>();
        }

        public bool HasPacked
        {
            get => hasPacked;
            set => hasPacked = value;
        }

        public bool IntroSeen
        {
            get => introSeen;
            set => introSeen = value;
        }

        public int AwakeningStep
        {
            get => awakeningStep;
            set => awakeningStep = value;
        }

        public bool KitchenSearched
        {
            get => kitchenSearched;
            set => kitchenSearched = value;
        }

        public bool BathroomSearched
        {
            get => bathroomSearched;
            set => bathroomSearched = value;
        }

        public bool ClosetSearched
        {
            get => closetSearched;
            set => closetSearched = value;
        }

        private bool hasPacked = false;
        private bool kitchenSearched = false;
        private bool bathroomSearched = false;
        private bool closetSearched = false;
        private bool phoneVisited = false;
        private bool radioVisited = false;
        private bool hackingVisited = false;
        private bool introSeen = false;

        private int awakeningStep = 1;
        private float lastClickTime = -999f;
        private bool isTyping = false;
        private Coroutine typeRoutine;
        private Coroutine hackingRoutine;
        private TextMeshProUGUI activeTypewriterTarget;
        private string activeFullText = "";

        // Cached frames for player avatar animation
        private List<Sprite> cachedAvatarFrames;
        private Coroutine avatarAnimRoutine;

        public static ApartmentHubScreen Instance { get; private set; }

        [Header("FEEL Feedbacks")]
        [Tooltip("FEEL Camera Shake / Screen Rumble feedback for high tension moments, explosions, or gunfire.")]
        [SerializeField] private MMF_Player screenShakeFeedback;
        [Tooltip("FEEL Red Vignette Pulse feedback triggered when Humanity drops below 30%.")]
        [SerializeField] private MMF_Player lowHumanityPulseFeedback;

        public MMF_Player ScreenShakeFeedback => screenShakeFeedback;
        public MMF_Player LowHumanityPulseFeedback => lowHumanityPulseFeedback;
        public CharacterData CurrentPlayer => currentPlayer;
        public TacticalMapViewController TacticalMap => tacticalMapController;
        public string CurrentRoomId => currentPlayer?.currentLocationId ?? (tacticalMapController != null ? tacticalMapController.CurrentLocationId : "unit_407");

        public GameClock Clock => gameClock;
        public ObjectiveSystem Objectives => objectiveSystem;
        public LoreSystem Lore => loreSystem;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            InitializeAudioSources();
            InitializeSystems();
            EnsureHubMenuSanitized();
            HookUIButtons();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeAudioSources()
        {
            if (atmosphereSource == null) atmosphereSource = CreateChildAudioSource("AtmosphereSource", true);
            if (ebsBroadcastSource == null) ebsBroadcastSource = CreateChildAudioSource("EBSBroadcastSource", false);
            if (staticSource == null) staticSource = CreateChildAudioSource("StaticSource", true);
            if (radioSource == null) radioSource = CreateChildAudioSource("RadioSource", false);
            if (sfxSource == null) sfxSource = CreateChildAudioSource("SFXSource", false);

            if (staticClip != null && staticClip.name.Contains("radio_static_ebs"))
            {
                staticClip = null;
            }
        }

        private AudioSource CreateChildAudioSource(string name, bool loop)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            return src;
        }

        private void InitializeSystems()
        {
            gameClock = new GameClock(23, 45, 25, "OCT", 2025, "SATURDAY");
            gameClock.OnTimeAdvanced += (c) => UpdateClockUI();

            objectiveSystem = new ObjectiveSystem();
            objectiveSystem.OnObjectiveUpdated += (o) =>
            {
                UpdateObjectiveUI();
                RefreshPDAStatusTab();
            };

            loreSystem = new LoreSystem();
            loreSystem.OnLoreAdded += (l) => RefreshPDALoreTab();
        }

        private void HookUIButtons()
        {
            // 3DS Console Mode Switcher Buttons
            if (btnModeNarrative != null)
            {
                btnModeNarrative.onClick.RemoveAllListeners();
                btnModeNarrative.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.Narrative));
            }
            if (btnModePDA != null)
            {
                btnModePDA.onClick.RemoveAllListeners();
                btnModePDA.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.PDA));
            }
            if (btnModeGear != null)
            {
                btnModeGear.onClick.RemoveAllListeners();
                btnModeGear.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.Gear));
            }

            if (tacticalGearController != null)
            {
                tacticalGearController.OnVitalsUpdated += () =>
                {
                    UpdateVitalsUI();
                };
                tacticalGearController.OnInventoryUpdated += () =>
                {
                    UpdateFloorItemsNotification();
                    RefreshStoryLogUI();
                };
            }

            if (fullScreenClickCatcher != null)
            {
                fullScreenClickCatcher.onClick.RemoveAllListeners();
                fullScreenClickCatcher.onClick.AddListener(OnDialogueAreaClicked);
            }

            if (continuePrompt != null)
            {
                Button promptBtn = continuePrompt.GetComponent<Button>();
                if (promptBtn == null) promptBtn = continuePrompt.AddComponent<Button>();
                promptBtn.onClick.RemoveAllListeners();
                promptBtn.onClick.AddListener(OnDialogueAreaClicked);
            }

            if (packGearButton != null)
            {
                packGearButton.onClick.RemoveAllListeners();
                packGearButton.onClick.AddListener(OnPackGearClicked);
            }

            if (checkPhoneButton != null)
            {
                checkPhoneButton.onClick.RemoveAllListeners();
                checkPhoneButton.onClick.AddListener(OnCheckPhoneClicked);
            }

            if (listenRadioButton != null)
            {
                listenRadioButton.onClick.RemoveAllListeners();
                listenRadioButton.onClick.AddListener(OnListenRadioClicked);
            }

            if (hackTerminalButton != null)
            {
                hackTerminalButton.onClick.RemoveAllListeners();
                hackTerminalButton.onClick.AddListener(OnHackTerminalClicked);
            }

            if (leaveApartmentButton != null)
            {
                leaveApartmentButton.onClick.RemoveAllListeners();
                leaveApartmentButton.onClick.AddListener(OnLeaveApartmentClicked);
            }

            if (closePhoneButton != null)
            {
                closePhoneButton.onClick.RemoveAllListeners();
                closePhoneButton.onClick.AddListener(OnClosePhoneClicked);
            }

            if (closeRadioButton != null)
            {
                closeRadioButton.onClick.RemoveAllListeners();
                closeRadioButton.onClick.AddListener(OnCloseRadioClicked);
            }

            if (closeHackingButton != null)
            {
                closeHackingButton.onClick.RemoveAllListeners();
                closeHackingButton.onClick.AddListener(OnCloseHackingClicked);
            }

            if (openStoryLogButton != null)
            {
                openStoryLogButton.onClick.RemoveAllListeners();
                openStoryLogButton.onClick.AddListener(OnOpenStoryLogClicked);
            }

            if (closeStoryLogButton != null)
            {
                closeStoryLogButton.onClick.RemoveAllListeners();
                closeStoryLogButton.onClick.AddListener(OnCloseStoryLogClicked);
            }

            if (closeStoryLogBottomButton != null)
            {
                closeStoryLogBottomButton.onClick.RemoveAllListeners();
                closeStoryLogBottomButton.onClick.AddListener(OnCloseStoryLogClicked);
            }

            InitializeStoryLogs();

            // Left PDA Tab Buttons
            if (pdaTabStatusBtn != null)
            {
                pdaTabStatusBtn.onClick.RemoveAllListeners();
                pdaTabStatusBtn.onClick.AddListener(() => SwitchPDATab("STATUS"));
            }
            if (pdaTabSkillsBtn != null)
            {
                pdaTabSkillsBtn.onClick.RemoveAllListeners();
                pdaTabSkillsBtn.onClick.AddListener(() => SwitchPDATab("SKILLS"));
            }
            if (pdaTabTraitsBtn != null)
            {
                pdaTabTraitsBtn.onClick.RemoveAllListeners();
                pdaTabTraitsBtn.onClick.AddListener(() => SwitchPDATab("TRAITS"));
            }
            if (pdaTabMapBtn != null)
            {
                pdaTabMapBtn.onClick.RemoveAllListeners();
                pdaTabMapBtn.onClick.AddListener(() => SwitchPDATab("MAP"));
            }
            if (pdaTabLoreBtn != null)
            {
                pdaTabLoreBtn.onClick.RemoveAllListeners();
                pdaTabLoreBtn.onClick.AddListener(() => SwitchPDATab("LORE"));
            }

            if (pdaVolumeSlider != null)
            {
                // Hide redundant outer VOL slider since full audio mixer lives inside SAVE / LOAD / SETTINGS modal
                if (pdaVolumeSlider.transform.parent != null && pdaVolumeSlider.transform.parent.name == "VolRow")
                {
                    pdaVolumeSlider.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    pdaVolumeSlider.gameObject.SetActive(false);
                }
            }

            if (pdaSaveSettingsBtn != null)
            {
                pdaSaveSettingsBtn.onClick.RemoveAllListeners();
                pdaSaveSettingsBtn.onClick.AddListener(OnPDASaveClicked);

                // Clean tactical button frame at the bottom of the PDA left sidebar
                Outline btnOl = pdaSaveSettingsBtn.GetComponent<Outline>();
                if (btnOl == null) btnOl = pdaSaveSettingsBtn.gameObject.AddComponent<Outline>();
                btnOl.effectColor = new Color(0.28f, 0.34f, 0.44f, 0.85f);
                btnOl.effectDistance = new Vector2(1f, -1f);
            }



            if (searchKitchenButton != null)
            {
                searchKitchenButton.onClick.RemoveAllListeners();
                searchKitchenButton.onClick.AddListener(OnSearchKitchenClicked);
            }

            if (searchBathroomButton != null)
            {
                searchBathroomButton.onClick.RemoveAllListeners();
                searchBathroomButton.onClick.AddListener(OnSearchBathroomClicked);
            }

            if (searchClosetButton != null)
            {
                searchClosetButton.onClick.RemoveAllListeners();
                searchClosetButton.onClick.AddListener(OnSearchClosetClicked);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.onClick.RemoveAllListeners();
                floorItemsButton.onClick.AddListener(OnTakeAllFloorItemsClicked);
            }
        }

        private readonly List<TacticalChoiceButton> activeChoiceButtons = new List<TacticalChoiceButton>();

        private void EnsureHubMenuSanitized()
        {
            if (hubMenuContainer == null) return;

            // 1. Hide any orphaned floor take button inside the narrative choice menu
            foreach (var t in hubMenuContainer.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.gameObject.name == "Btn_TakeFloorItems")
                {
                    t.gameObject.SetActive(false);
                }
            }

            // 2. Ensure VerticalLayoutGroup properties on hubMenuContainer (Guarantees choice viewport fills entire console)
            var vlgRoot = hubMenuContainer.GetComponent<VerticalLayoutGroup>();
            if (vlgRoot != null)
            {
                vlgRoot.spacing = 4f;
                vlgRoot.childControlHeight = true;
                vlgRoot.childForceExpandHeight = false;
                vlgRoot.childControlWidth = true;
                vlgRoot.childForceExpandWidth = true;
            }

            // Ensure SituationBox layout element stays compact
            if (situationBox != null)
            {
                if (situationBox.transform.parent != hubMenuContainer.transform)
                {
                    situationBox.transform.SetParent(hubMenuContainer.transform, false);
                }
                LayoutElement sle = situationBox.GetComponent<LayoutElement>() ?? situationBox.AddComponent<LayoutElement>();
                sle.preferredHeight = 58f;
                sle.flexibleHeight = 0f;
                sle.flexibleWidth = 1f;
            }

            // Ensure PackedSuccessBanner stays compact
            if (packedSuccessBanner != null)
            {
                if (packedSuccessBanner.transform.parent != hubMenuContainer.transform)
                {
                    packedSuccessBanner.transform.SetParent(hubMenuContainer.transform, false);
                }
                LayoutElement ple = packedSuccessBanner.GetComponent<LayoutElement>() ?? packedSuccessBanner.AddComponent<LayoutElement>();
                ple.preferredHeight = 26f;
                ple.flexibleHeight = 0f;
                ple.flexibleWidth = 1f;
            }

            // 3. Find or Wrap in ScrollView for choices
            ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);
            Transform contentParent = null;

            if (sr == null)
            {
                GameObject scrollObj = new GameObject("ChoiceScrollView", typeof(RectTransform));
                scrollObj.transform.SetParent(hubMenuContainer.transform, false);
                RectTransform srt = scrollObj.GetComponent<RectTransform>();
                srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
                srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
                LayoutElement scLe = scrollObj.AddComponent<LayoutElement>();
                scLe.flexibleHeight = 1f;
                scLe.minHeight = 240f;
                scLe.flexibleWidth = 1f;

                sr = scrollObj.AddComponent<ScrollRect>();
                sr.horizontal = false;
                sr.vertical = true;
                sr.movementType = ScrollRect.MovementType.Clamped;
                sr.scrollSensitivity = 28f;

                GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
                viewport.transform.SetParent(scrollObj.transform, false);
                RectTransform vprt = viewport.GetComponent<RectTransform>();
                vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
                vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
                viewport.AddComponent<RectMask2D>();
                sr.viewport = vprt;

                GameObject content = new GameObject("Content", typeof(RectTransform));
                content.transform.SetParent(viewport.transform, false);
                RectTransform crt = content.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
                crt.pivot = new Vector2(0.5f, 1f);
                crt.anchoredPosition = Vector2.zero;
                crt.sizeDelta = Vector2.zero;

                VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 3f;
                vlg.padding = new RectOffset(2, 6, 2, 2);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                sr.content = crt;
                contentParent = content.transform;

                // Reparent only choice buttons to contentParent
                List<Transform> childrenToMove = new List<Transform>();
                for (int i = 0; i < hubMenuContainer.transform.childCount; i++)
                {
                    Transform child = hubMenuContainer.transform.GetChild(i);
                    if (child != scrollObj.transform && child != situationBox?.transform && child != packedSuccessBanner?.transform && child.gameObject.name != "ChoiceSubHeader")
                    {
                        childrenToMove.Add(child);
                    }
                }
                foreach (var c in childrenToMove)
                {
                    c.SetParent(contentParent, false);
                }
            }
            else
            {
                LayoutElement scLe = sr.GetComponent<LayoutElement>() ?? sr.gameObject.AddComponent<LayoutElement>();
                scLe.flexibleHeight = 1f;
                scLe.minHeight = 240f;
                scLe.flexibleWidth = 1f;

                contentParent = sr.content != null ? sr.content.transform : sr.transform;
                var cvlg = contentParent.GetComponent<VerticalLayoutGroup>();
                if (cvlg != null)
                {
                    cvlg.spacing = 3f;
                    cvlg.padding = new RectOffset(2, 6, 2, 2);
                    cvlg.childControlWidth = true;
                    cvlg.childControlHeight = true;
                    cvlg.childForceExpandWidth = true;
                    cvlg.childForceExpandHeight = false;
                }
            }

            // 4. Ensure header order: SituationBox -> PackedSuccessBanner -> ChoiceSubHeader -> ScrollView
            int sIdx = 0;
            if (situationBox != null) situationBox.transform.SetSiblingIndex(sIdx++);
            if (packedSuccessBanner != null) packedSuccessBanner.transform.SetSiblingIndex(sIdx++);
            Transform sh = hubMenuContainer.transform.Find("ChoiceSubHeader");
            if (sh != null)
            {
                sh.SetSiblingIndex(sIdx++);
                var shTmp = sh.GetComponentInChildren<TextMeshProUGUI>();
                if (shTmp != null)
                {
                    shTmp.text = "<b><color=#F5A623>// WHAT SHOULD I DO NEXT?</color></b>  <color=#64748B>[KEYS: 1–8 OR CLICK]</color>";
                }
            }
            if (sr != null) sr.transform.SetSiblingIndex(sIdx++);

            // 5. Dynamically create missing Scavenge Buttons if null
            Button templateBtn = packGearButton != null ? packGearButton : leaveApartmentButton;
            if (templateBtn != null)
            {
                if (searchKitchenButton == null)
                {
                    searchKitchenButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchKitchen",
                        "<b>Check the kitchen cabinets and fridge for bottled water and food.</b>  <color=#64748B><i>(~3 min)</i></color>",
                        "",
                        new Color(0.95f, 0.70f, 0.30f), OnSearchKitchenClicked, out searchKitchenLabel, out searchKitchenSub);
                }
                if (searchBathroomButton == null)
                {
                    searchBathroomButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchBathroom",
                        "<b>Check the bathroom medicine cabinet for painkillers and bandages.</b>  <color=#64748B><i>(~2 min)</i></color>",
                        "",
                        new Color(0.30f, 0.85f, 0.60f), OnSearchBathroomClicked, out searchBathroomLabel, out searchBathroomSub);
                }
                if (searchClosetButton == null)
                {
                    searchClosetButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchCloset",
                        "<b>Grab my heavy coat, rain jacket, and flashlight from the front closet.</b>  <color=#64748B><i>(~2 min)</i></color>",
                        "",
                        new Color(0.40f, 0.75f, 0.95f), OnSearchClosetClicked, out searchClosetLabel, out searchClosetSub);
                }
            }

            // 6. Ensure logical child order in contentParent so choices flow naturally
            if (packGearButton != null) packGearButton.transform.SetSiblingIndex(0);
            if (searchKitchenButton != null) searchKitchenButton.transform.SetSiblingIndex(1);
            if (searchBathroomButton != null) searchBathroomButton.transform.SetSiblingIndex(2);
            if (searchClosetButton != null) searchClosetButton.transform.SetSiblingIndex(3);
            if (checkPhoneButton != null) checkPhoneButton.transform.SetSiblingIndex(4);
            if (listenRadioButton != null) listenRadioButton.transform.SetSiblingIndex(5);
            if (hackTerminalButton != null) hackTerminalButton.transform.SetSiblingIndex(6);
            if (leaveApartmentButton != null) leaveApartmentButton.transform.SetSiblingIndex(7);

            // 7. Re-skin all buttons in contentParent to compact 32px Fallout/BG3 CRPG row format
            SanitizeChoiceRowVisuals(contentParent);

            // 8. Sanitize console switcher buttons and legacy baked badges to Day 0 civilian terminology
            SanitizeCivilianTerminology();
        }

        private void SanitizeCivilianTerminology()
        {
            // 1. Console Mode Switcher Buttons
            if (btnModeNarrative != null)
            {
                var tmp = btnModeNarrative.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "NARRATIVE & LOG [ESC]";
            }
            if (btnModePDA != null)
            {
                var tmp = btnModePDA.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "PERSONAL PHONE [TAB]";
            }
            if (btnModeGear != null)
            {
                int count = floorItems != null ? floorItems.Count : 0;
                bool hasLoot = count > 0;
                if (btnModeGearLabel != null)
                {
                    btnModeGearLabel.text = hasLoot 
                        ? $"BACKPACK & ITEMS [I] <color=#F59E0B><b>({count})</b></color>" 
                        : "BACKPACK & ITEMS [I]";
                }
                else
                {
                    var tmp = btnModeGear.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = "BACKPACK & ITEMS [I]";
                }
            }

            // 2. Sanitize any legacy baked badges & headers across child texts
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t == null || string.IsNullOrEmpty(t.text)) continue;

                if (t.text.Contains("SURVIVOR PDA"))
                    t.text = t.text.Replace("SURVIVOR PDA", "PERSONAL PHONE");
                if (t.text.Contains("TACTICAL GEAR"))
                    t.text = t.text.Replace("TACTICAL GEAR", "BACKPACK & ITEMS");
                if (t.text.Contains("SURVIVOR CHRONICLE"))
                    t.text = t.text.Replace("SURVIVOR CHRONICLE", "INCIDENT & JOURNAL LOG");
                if (t.text.Contains("INCIDENT & JOURNAL LOG // UNIT 407"))
                    t.text = t.text.Replace("INCIDENT & JOURNAL LOG // UNIT 407", "INCIDENT & JOURNAL LOG");
                if (t.text.Contains("CIVILIAN BACKGROUND"))
                    t.text = t.text.Replace("CIVILIAN BACKGROUND", "CITIZEN BACKGROUND");
                if (t.text.Contains("SURVIVOR MATRIX"))
                    t.text = t.text.Replace("SURVIVOR MATRIX", "ATTRIBUTE PROFILE");
                if (t.text.Contains("COMBAT & SURVIVAL DISCIPLINES // TACTICAL PROFICIENCY"))
                    t.text = t.text.Replace("COMBAT & SURVIVAL DISCIPLINES // TACTICAL PROFICIENCY", "SKILLS & PROFICIENCIES // PRACTICAL TRAINING");
                if (t.text.Contains("TACTICAL CONDITION:"))
                    t.text = t.text.Replace("TACTICAL CONDITION:", "CURRENT SITUATION:");
                if (t.text.Contains("ACTIVE LOADOUT"))
                    t.text = t.text.Replace("ACTIVE LOADOUT", "CLOTHING & EQUIPPED");
                if (t.text.Contains("ITEM DOSSIER"))
                    t.text = t.text.Replace("ITEM DOSSIER", "ITEM DETAILS");
                if (t.text.Contains("tactical intel & specs"))
                    t.text = t.text.Replace("tactical intel & specs", "details");
                if (t.text.Contains("tactical intel, ballistic specs, and survival properties"))
                    t.text = t.text.Replace("tactical intel, ballistic specs, and survival properties", "details and condition");
                if (t.text.Contains("TACTICAL SLOTS"))
                    t.text = t.text.Replace("TACTICAL SLOTS", "SLOTS");
            }
        }

        private void SetChoiceButtonContent(Button btn, string singleFullText)
        {
            if (btn == null) return;

            Transform tc = btn.transform.Find("TextContainer") ?? btn.transform.Find("LabelContainer");
            Transform ml = tc != null ? (tc.Find("MainLabel") ?? tc.Find("Label") ?? tc.Find("Title")) : btn.transform.Find("MainLabel");
            if (ml == null) ml = btn.transform.Find("Label") ?? btn.transform.Find("Title");

            TextMeshProUGUI tmp = ml != null ? ml.GetComponent<TextMeshProUGUI>() : btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = singleFullText;
                SundayTypography.ApplyFont(tmp, FontRole.Body);
            }

            Transform sl = tc != null ? (tc.Find("SubLabel") ?? tc.Find("SubText")) : btn.transform.Find("SubLabel");
            if (sl != null)
            {
                TextMeshProUGUI stmp = sl.GetComponent<TextMeshProUGUI>();
                if (stmp != null) stmp.text = "";
                sl.gameObject.SetActive(false);
            }
        }

        private void SanitizeChoiceRowVisuals(Transform contentParent)
        {
            if (contentParent == null) return;

            // Ensure parent VerticalLayoutGroup drives heights and uses compact spacing
            var cvlg = contentParent.GetComponent<VerticalLayoutGroup>();
            if (cvlg != null)
            {
                cvlg.spacing = 3f;
                cvlg.padding = new RectOffset(2, 6, 2, 2);
                cvlg.childControlWidth = true;
                cvlg.childControlHeight = true;
                cvlg.childForceExpandWidth = true;
                cvlg.childForceExpandHeight = false;
            }

            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                if (child == null) continue;

                // 1. Force 34px height for Direction A Fallout 4 FDI density
                RectTransform crt = child.GetComponent<RectTransform>();
                if (crt != null)
                {
                    crt.sizeDelta = new Vector2(crt.sizeDelta.x, 34f);
                }
                LayoutElement le = child.GetComponent<LayoutElement>();
                if (le == null && crt != null) le = child.gameObject.AddComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredHeight = 34f;
                    le.minHeight = 34f;
                    le.flexibleHeight = 0f;
                    le.flexibleWidth = 1f;
                }

                // 2. Ensure Left Accent Bar
                Transform bar = child.Find("AccentBar");
                Image barImg = null;
                if (bar != null)
                {
                    barImg = bar.GetComponent<Image>();
                    RectTransform brt = bar.GetComponent<RectTransform>();
                    if (brt != null)
                    {
                        brt.anchorMin = new Vector2(0f, 0f);
                        brt.anchorMax = new Vector2(0f, 1f);
                        brt.pivot = new Vector2(0f, 0.5f);
                        brt.sizeDelta = new Vector2(3f, 0f);
                        brt.anchoredPosition = Vector2.zero;
                    }
                    bar.gameObject.SetActive(false);
                }

                // 3. Ensure Gutter with HoverChevron (>>) and HotkeyLabel ([ 1 ])
                Transform gutter = child.Find("Gutter");
                TextMeshProUGUI chTmp = null;
                TextMeshProUGUI hkTmp = null;

                if (gutter == null)
                {
                    GameObject gutObj = new GameObject("Gutter", typeof(RectTransform));
                    gutObj.transform.SetParent(child, false);
                    gutObj.transform.SetSiblingIndex(0);
                    RectTransform gutRect = gutObj.GetComponent<RectTransform>();
                    gutRect.anchorMin = new Vector2(0f, 0f);
                    gutRect.anchorMax = new Vector2(0f, 1f);
                    gutRect.pivot = new Vector2(0f, 0.5f);
                    gutRect.sizeDelta = new Vector2(46f, 0f);
                    gutRect.anchoredPosition = Vector2.zero;
                    gutter = gutObj.transform;

                    // HoverChevron (>>)
                    GameObject chObj = new GameObject("HoverChevron", typeof(RectTransform));
                    chObj.transform.SetParent(gutter, false);
                    RectTransform chRect = chObj.GetComponent<RectTransform>();
                    chRect.anchorMin = new Vector2(0f, 0f);
                    chRect.anchorMax = new Vector2(0f, 1f);
                    chRect.pivot = new Vector2(0f, 0.5f);
                    chRect.anchoredPosition = new Vector2(-4f, 0f);
                    chRect.sizeDelta = new Vector2(16f, 0f);
                    chTmp = chObj.AddComponent<TextMeshProUGUI>();
                    chTmp.fontSize = 12f;
                    chTmp.fontStyle = FontStyles.Bold;
                    chTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    chTmp.color = new Color(1f, 0.65f, 0.15f);
                    chTmp.text = ">>";
                    chTmp.raycastTarget = false;
                    chObj.SetActive(false);

                    // HotkeyLabel ([ 1 ])
                    GameObject hkObj = new GameObject("HotkeyLabel", typeof(RectTransform));
                    hkObj.transform.SetParent(gutter, false);
                    RectTransform hkRect = hkObj.GetComponent<RectTransform>();
                    hkRect.anchorMin = new Vector2(0f, 0f);
                    hkRect.anchorMax = new Vector2(1f, 1f);
                    hkRect.offsetMin = new Vector2(16f, 0f);
                    hkRect.offsetMax = Vector2.zero;
                    hkTmp = hkObj.AddComponent<TextMeshProUGUI>();
                    hkTmp.fontSize = 11.5f;
                    hkTmp.fontStyle = FontStyles.Bold;
                    hkTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    hkTmp.color = new Color(0.85f, 0.65f, 0.30f, 0.95f);
                    hkTmp.text = $"[ {i + 1} ]";
                    hkTmp.raycastTarget = false;
                    SundayTypography.ApplyFont(hkTmp, FontRole.Mono);
                }
                else
                {
                    Transform ch = gutter.Find("HoverChevron");
                    if (ch != null) chTmp = ch.GetComponent<TextMeshProUGUI>();
                    Transform hk = gutter.Find("HotkeyLabel");
                    if (hk != null) hkTmp = hk.GetComponent<TextMeshProUGUI>();
                }

                // 4. Ensure TextContainer is formatted as a single full-width continuous row
                Transform textCont = child.Find("TextContainer") ?? child.Find("LabelContainer");
                TextMeshProUGUI mainLbl = null;

                if (textCont != null)
                {
                    RectTransform tcRect = textCont.GetComponent<RectTransform>();
                    tcRect.anchorMin = new Vector2(0f, 0f);
                    tcRect.anchorMax = new Vector2(1f, 1f);
                    tcRect.offsetMin = new Vector2(64f, 0f); // Clean 18px gap after 46px gutter
                    tcRect.offsetMax = new Vector2(-16f, 0f);

                    // Remove/disable HorizontalLayoutGroup if present so it doesn't split or clip text
                    HorizontalLayoutGroup hlg = textCont.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.enabled = false;
                        Destroy(hlg);
                    }

                    Transform ml = textCont.Find("MainLabel") ?? textCont.Find("Label") ?? textCont.Find("Title");
                    if (ml != null)
                    {
                        ContentSizeFitter csf = ml.GetComponent<ContentSizeFitter>();
                        if (csf != null)
                        {
                            csf.enabled = false;
                            Destroy(csf);
                        }

                        RectTransform mlRect = ml.GetComponent<RectTransform>();
                        if (mlRect != null)
                        {
                            mlRect.anchorMin = Vector2.zero;
                            mlRect.anchorMax = Vector2.one;
                            mlRect.offsetMin = Vector2.zero;
                            mlRect.offsetMax = Vector2.zero;
                            mlRect.pivot = new Vector2(0f, 0.5f);
                        }

                        mainLbl = ml.GetComponent<TextMeshProUGUI>();
                        if (mainLbl != null)
                        {
                            mainLbl.fontSize = 11.5f;
                            mainLbl.alignment = TextAlignmentOptions.MidlineLeft;
                            mainLbl.textWrappingMode = TextWrappingModes.NoWrap;
                            mainLbl.overflowMode = TextOverflowModes.Ellipsis;
                            SundayTypography.ApplyFont(mainLbl, FontRole.Body);
                        }
                    }

                    Transform sl = textCont.Find("SubLabel") ?? textCont.Find("SubText");
                    if (sl != null)
                    {
                        sl.gameObject.SetActive(false);
                    }
                }

                // 5. Initialize or re-link TacticalChoiceButton
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    TacticalChoiceButton tcb = child.GetComponent<TacticalChoiceButton>();
                    if (tcb == null) tcb = child.gameObject.AddComponent<TacticalChoiceButton>();

                    Image bg = child.GetComponent<Image>();
                    Outline ol = child.GetComponent<Outline>();
                    Transform shine = child.Find("ShineSweep");
                    RectTransform shineRect = shine != null ? shine.GetComponent<RectTransform>() : null;
                    Color accent = barImg != null ? barImg.color : new Color(0.95f, 0.75f, 0.35f);

                    tcb.Initialize(btn, textCont as RectTransform, bg, barImg, ol, shineRect, accent, chTmp, hkTmp, mainLbl, null);
                }
            }
        }

        private Button CreateScavengeButtonClone(Button template, Transform parent, string name, string title, string sub, Color accentColor, UnityEngine.Events.UnityAction action, out TextMeshProUGUI titleTmp, out TextMeshProUGUI subTmp)
        {
            GameObject go = Instantiate(template.gameObject, parent);
            go.name = name;
            go.SetActive(false);

            Button btn = go.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            if (action != null) btn.onClick.AddListener(action);

            titleTmp = null;
            subTmp = null;

            // Find AccentBar
            Transform bar = go.transform.Find("AccentBar");
            Image barImg = null;
            if (bar != null)
            {
                barImg = bar.GetComponent<Image>();
                if (barImg != null) barImg.color = accentColor;
            }

            // Find labels
            TextMeshProUGUI[] tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t.gameObject.name == "MainLabel" || t.gameObject.name == "Label" || t.gameObject.name == "Title" || t.fontSize > 11f)
                {
                    titleTmp = t;
                    t.text = title;
                }
                else if (t.gameObject.name == "SubLabel" || t.gameObject.name == "SubText" || t.fontSize <= 11f)
                {
                    subTmp = t;
                    t.text = "";
                    t.gameObject.SetActive(false);
                }
            }

            // Find gutter labels
            TextMeshProUGUI chTmp = null;
            TextMeshProUGUI hkTmp = null;
            Transform gut = go.transform.Find("Gutter");
            if (gut != null)
            {
                Transform ch = gut.Find("HoverChevron");
                if (ch != null) chTmp = ch.GetComponent<TextMeshProUGUI>();
                Transform hk = gut.Find("HotkeyLabel");
                if (hk != null) hkTmp = hk.GetComponent<TextMeshProUGUI>();
            }

            // Re-bind TacticalChoiceButton for hover animation & audio
            TacticalChoiceButton tcb = go.GetComponent<TacticalChoiceButton>();
            if (tcb != null)
            {
                RectTransform tcRect = go.transform.Find("TextContainer") as RectTransform;
                Image bg = go.GetComponent<Image>();
                Outline ol = go.GetComponent<Outline>();
                RectTransform shineRect = go.transform.Find("ShineSweep") as RectTransform;
                tcb.Initialize(btn, tcRect, bg, barImg, ol, shineRect, accentColor, chTmp, hkTmp, titleTmp, null);
            }

            return btn;
        }

        public void InitializeWithCharacter(CharacterData character)
        {
            EnsureHubMenuSanitized();
            currentPlayer = character;

            // In accordance with Twine design:
            // Items accumulated during creation are dropped on the tablestash prior to packing!
            tableStash.Clear();
            floorItems.Clear();
            equippedHead = "";
            equippedBody = "";
            equippedWpn = "";
            equippedLegs = "";
            equippedFeet = "";

            if (currentPlayer != null)
            {
                currentPlayer.equippedHead = "";
                currentPlayer.equippedBody = "";
                currentPlayer.equippedWpn = "";
                currentPlayer.equippedLegs = "";
                currentPlayer.equippedFeet = "";
            }

            if (currentPlayer != null && currentPlayer.inventory != null)
            {
                foreach (var item in currentPlayer.inventory)
                {
                    tableStash.Add(new InventoryEntry(item.itemId, item.quantity));
                }
                currentPlayer.inventory.Clear();
            }

            // Ensure Apartment Key is on the coffee table as part of Day 0 setup
            bool hasKey = tableStash.Any(entry => string.Equals(entry.itemId, "Apartment Key", StringComparison.OrdinalIgnoreCase) || string.Equals(entry.itemId, "Apartment 407 Key", StringComparison.OrdinalIgnoreCase));
            if (!hasKey)
            {
                tableStash.Insert(0, new InventoryEntry("Apartment Key", 1));
            }

            hasPacked = false;
            kitchenSearched = false;
            bathroomSearched = false;
            closetSearched = false;
            introSeen = false;
            awakeningStep = 1;

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"Sirens wail across the Seattle skyline in the blackout. In the dark of my apartment, the deadbolt on the front door is locked tight. My apartment keys, wallet, and everyday items are still sitting on the coffee table.\"";
            }

            if (floorItemsCard != null && tacticalGearController == null) floorItemsCard.SetActive(false);
            if (searchKitchenButton != null) searchKitchenButton.gameObject.SetActive(true);
            if (searchBathroomButton != null) searchBathroomButton.gameObject.SetActive(true);
            if (searchClosetButton != null) searchClosetButton.gameObject.SetActive(true);
            UpdateFloorItemsNotification();

            UpdateClockUI();
            UpdateVitalsUI();
            UpdateObjectiveUI();
            PopulateTableStashUI();
            RefreshAllPDAPanels();
            SwitchPDATab("STATUS", playSound: false);
            SwitchConsoleMode(ConsoleMode.Narrative, playSound: false);
            EvaluateHubButtonStates();

            // Automatic Location Synchronization across Top HUD & Tactical Map
            string startLoc = (currentPlayer != null && !string.IsNullOrEmpty(currentPlayer.currentLocationId))
                ? currentPlayer.currentLocationId
                : "unit_407";
            SetLocation(startLoc);

            // Load player avatar frames
            cachedAvatarFrames = CharacterCreationScreen.LoadPlayerFrames();
        }

        private Coroutine screenShakeRoutine;
        private Image lowHumanityVignetteOverlay;
        private Coroutine lowHumanityPulseRoutine;

        /// <summary>
        /// FEEL Feedback: Triggers Camera Shake / Screen Rumble during high tension events, breach, or gunshots.
        /// Guaranteed visual rumble even if MMF_Player component has not yet been dragged in Inspector.
        /// </summary>
        public void PlayScreenShake()
        {
            if (screenShakeFeedback != null)
            {
                screenShakeFeedback.PlayFeedbacks();
            }

            if (screenShakeRoutine != null) StopCoroutine(screenShakeRoutine);
            screenShakeRoutine = StartCoroutine(DoScreenShakeRumble(0.40f, 16f));

            Debug.Log("<color=orange><b>[FEEL] Camera Shake / Screen Rumble Triggered! (F6)</b></color>");
        }

        private IEnumerator DoScreenShakeRumble(float duration, float maxMagnitude)
        {
            RectTransform target = transform as RectTransform;
            if (target == null) yield break;

            Vector2 originalPos = target.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                float x = (UnityEngine.Random.value * 2f - 1f) * maxMagnitude * damper;
                float y = (UnityEngine.Random.value * 2f - 1f) * maxMagnitude * damper;

                target.anchoredPosition = originalPos + new Vector2(x, y);
                yield return null;
            }

            target.anchoredPosition = originalPos;
            screenShakeRoutine = null;
        }

        private Coroutine damageFlashRoutine;
        private Image damageVignetteOverlay;

        /// <summary>
        /// FEEL Feedback: Triggers an intense red impact flash & trauma jolt when the player is attacked, injured, or fails a dangerous skill check.
        /// </summary>
        public void PlayDamageFeedback(int damageAmount = 0)
        {
            EnsureDamageVignetteExists();
            if (damageFlashRoutine != null) StopCoroutine(damageFlashRoutine);
            damageFlashRoutine = StartCoroutine(DoDamageFlash());

            // Impact jolt
            if (screenShakeRoutine != null) StopCoroutine(screenShakeRoutine);
            screenShakeRoutine = StartCoroutine(DoScreenShakeRumble(0.28f, 14f));

            if (lowHumanityPulseFeedback != null)
            {
                lowHumanityPulseFeedback.PlayFeedbacks();
            }

            string dmgInfo = damageAmount > 0 ? $" (-{damageAmount} HP)" : "";
            Debug.Log($"<color=red><b>[FEEL] Player Damage Impact Feedback Triggered!{dmgInfo} (F7)</b></color>");
        }

        private void EnsureDamageVignetteExists()
        {
            if (damageVignetteOverlay != null) return;

            GameObject vigObj = new GameObject("DamageImpactVignette");
            vigObj.transform.SetParent(transform, false);
            RectTransform vr = vigObj.AddComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = Vector2.zero;
            vr.offsetMax = Vector2.zero;

            damageVignetteOverlay = vigObj.AddComponent<Image>();
            damageVignetteOverlay.color = new Color(0.95f, 0.05f, 0.05f, 0f);
            damageVignetteOverlay.raycastTarget = false;
            vigObj.transform.SetAsLastSibling();
            vigObj.SetActive(false);
        }

        private IEnumerator DoDamageFlash()
        {
            if (damageVignetteOverlay == null) yield break;

            damageVignetteOverlay.gameObject.SetActive(true);
            float duration = 0.50f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Sharp initial flash, smooth decay
                float alpha = Mathf.Lerp(0.55f, 0f, t);
                damageVignetteOverlay.color = new Color(0.95f, 0.05f, 0.05f, alpha);
                yield return null;
            }

            damageVignetteOverlay.gameObject.SetActive(false);
            damageFlashRoutine = null;
        }

        /// <summary>
        /// Public helper for skill checks and encounters to apply damage and trigger the red hit feedback.
        /// </summary>
        public void ApplyDamage(int amount, string source = null)
        {
            if (currentPlayer == null) return;
            currentPlayer.hp = Mathf.Max(0, currentPlayer.hp - amount);
            UpdateVitalsUI();
            PlayDamageFeedback(amount);

            if (!string.IsNullOrEmpty(source))
            {
                AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", "INJURY SUSTAINED",
                    $"Sustained {amount} trauma damage from {source}. Current Vital State: {currentPlayer.hp}/{currentPlayer.hpMax} HP.");
            }
        }

        [ContextMenu("FEEL: Test Screen Shake (F6)")]
        public void DebugTestScreenShake()
        {
            PlayScreenShake();
        }

        [ContextMenu("FEEL: Test Damage Hit Flash (F7)")]
        public void DebugTestDamageFeedback()
        {
            PlayDamageFeedback(15);
        }

        /// <summary>
        /// Restores and re-binds a loaded CharacterData state into the Apartment Hub UI.
        /// </summary>
        public void LoadCharacterState(CharacterData character, string roomId = null)
        {
            if (character == null) return;
            currentPlayer = character;

            // Re-sync equipped clothing / weapons from CharacterData
            equippedHead = character.equippedHead ?? "";
            equippedBody = character.equippedBody ?? "";
            equippedWpn = character.equippedWpn ?? "";
            equippedLegs = character.equippedLegs ?? "";
            equippedFeet = character.equippedFeet ?? "";

            // Safety recovery: If loading at Beat 1/Day 0 and both backpack and tableStash are empty (e.g. from an older save),
            // auto-restore starting items for the player's job AND all traits so they are never left empty-handed!
            if (!hasPacked && (tableStash == null || tableStash.Count == 0) && (currentPlayer.inventory == null || currentPlayer.inventory.Count == 0))
            {
                if (tableStash == null) tableStash = new List<InventoryEntry>();

                var job = System.Array.Find(CharacterCreationDefinitions.Jobs, j => j.title.Equals(currentPlayer.job, StringComparison.OrdinalIgnoreCase) || j.id.Equals(currentPlayer.job, StringComparison.OrdinalIgnoreCase));
                if (job != null && job.startingItems != null)
                {
                    foreach (var item in job.startingItems)
                    {
                        tableStash.Add(new InventoryEntry(item.item, item.qty));
                    }
                }

                // Also restore starting items given by character traits / hobbies (e.g. ARCHER -> Compound Bow + Arrows)
                if (currentPlayer.traits != null)
                {
                    foreach (var t in currentPlayer.traits)
                    {
                        if (string.Equals(t.name, "ARCHER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Compound Bow")) tableStash.Add(new InventoryEntry("Compound Bow", 1));
                            if (!tableStash.Any(e => e.itemId == "Arrows")) tableStash.Add(new InventoryEntry("Arrows", 5));
                        }
                        else if (string.Equals(t.name, "HIKER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Hiking Boots")) tableStash.Add(new InventoryEntry("Hiking Boots", 1));
                        }
                        else if (string.Equals(t.name, "RUNNER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Energy Bar")) tableStash.Add(new InventoryEntry("Energy Bar", 2));
                            if (!tableStash.Any(e => e.itemId == "Running Shoes")) tableStash.Add(new InventoryEntry("Running Shoes", 1));
                        }
                        else if (string.Equals(t.name, "HUNTER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Binoculars")) tableStash.Add(new InventoryEntry("Binoculars", 1));
                            if (!tableStash.Any(e => e.itemId == "Combat Knife")) tableStash.Add(new InventoryEntry("Combat Knife", 1));
                        }
                        else if (string.Equals(t.name, "BASEBALL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Baseball Bat")) tableStash.Add(new InventoryEntry("Baseball Bat", 1));
                        }
                        else if (string.Equals(t.name, "WATCHMAN", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Flashlight")) tableStash.Add(new InventoryEntry("Flashlight", 1));
                        }
                        else if (string.Equals(t.name, "SURVIVALIST", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!tableStash.Any(e => e.itemId == "Multi-Tool")) tableStash.Add(new InventoryEntry("Multi-Tool", 1));
                        }
                    }
                }

                if (!tableStash.Any(entry => string.Equals(entry.itemId, "Apartment Key", StringComparison.OrdinalIgnoreCase) || string.Equals(entry.itemId, "Apartment 407 Key", StringComparison.OrdinalIgnoreCase)))
                {
                    tableStash.Insert(0, new InventoryEntry("Apartment Key", 1));
                }
            }

            UpdateVitalsUI();
            UpdateClockUI();
            UpdateObjectiveUI();
            PopulateTableStashUI();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();

            // Restore awakening or hub sequence state
            if (hasPacked || introSeen)
            {
                introSeen = true;
                if (awakeningContainer != null) awakeningContainer.SetActive(false);
                if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
                if (continuePrompt != null) continuePrompt.SetActive(false);
                ShowHubMenu();
            }
            else
            {
                ShowAwakeningStep(Mathf.Clamp(awakeningStep, 1, 3));
            }

            string loc = !string.IsNullOrEmpty(roomId) ? roomId : (!string.IsNullOrEmpty(character.currentLocationId) ? character.currentLocationId : "unit_407");
            SetLocation(loc);
        }

        /// <summary>
        /// Updates the in-game clock time and refreshes the HUD clock display.
        /// </summary>
        public void SetGameClockTime(int day, int hour, int minute, int date = 25, string month = "OCT", int year = 2025, string dayName = null)
        {
            if (gameClock == null)
            {
                gameClock = new GameClock(hour, minute, date, month, year, dayName);
                gameClock.OnTimeAdvanced += (c) => UpdateClockUI();
            }

            gameClock.SetTime(day, hour, minute, date, month, year, dayName);
            UpdateClockUI();
        }

        protected override void OnScreenOpened()
        {
            base.OnScreenOpened();

            if (currentPlayer == null)
            {
                // Fallback default Police Officer character if tested directly in Editor
                CharacterData fallback = new CharacterData
                {
                    firstName = "James",
                    lastName = "Smith",
                    gender = "Male",
                    job = "Police Officer",
                    hp = 100,
                    hpMax = 100,
                    stamina = 80,
                    staminaMax = 80,
                    hunger = 100,
                    thirst = 100,
                    humanity = 35,
                    str = 6,
                    per = 8,
                    agi = 6,
                    intl = 5,
                    cha = 3,
                    lck = 4,
                    shooting = 3,
                    melee = 2,
                    tech = 2
                };
                fallback.AddTrait("HUNTER", "Spot ambushes early.");
                fallback.AddTrait("STEADY HANDS", "+15 Accuracy bonus.");
                fallback.AddTrait("PYRO", "Fire weapons deal +50% DMG.", null, true);
                fallback.AddTrait("MECHANIC", "Start cars without keys.");
                fallback.AddItem("Apartment Key", 1);
                fallback.AddItem("Glock 19", 1);
                fallback.AddItem("9mm Ammo", 24);
                fallback.AddItem("Multi-Tool", 1);
                fallback.AddItem("Binoculars", 1);
                fallback.AddItem("Lighter", 1);
                fallback.AddItem("Jerry Can", 1);
                InitializeWithCharacter(fallback);
            }

            // Play background atmosphere drone
            if (atmosphereSource != null && atmosphereClip != null)
            {
                atmosphereSource.clip = atmosphereClip;
                atmosphereSource.volume = 0.5f;
                atmosphereSource.Play();
            }

            if (!introSeen)
            {
                ShowAwakeningStep(1);
            }
            else
            {
                ShowHubMenu();
            }
        }

        protected override void OnScreenClosed()
        {
            base.OnScreenClosed();
            StopAllAudioSources();
        }

        private void StopAllAudioSources()
        {
            if (atmosphereSource != null) atmosphereSource.Stop();
            if (ebsBroadcastSource != null) ebsBroadcastSource.Stop();
            if (staticSource != null) staticSource.Stop();
            if (radioSource != null) radioSource.Stop();
            if (sfxSource != null) sfxSource.Stop();
        }

        public enum ConsoleMode { Narrative, PDA, Gear }
        private ConsoleMode currentConsoleMode = ConsoleMode.Narrative;

        public void SwitchConsoleMode(ConsoleMode mode, bool playSound = true)
        {
            currentConsoleMode = mode;
            if (dockContainerNarrative != null) dockContainerNarrative.SetActive(mode == ConsoleMode.Narrative);
            if (dockContainerPDA != null) dockContainerPDA.SetActive(mode == ConsoleMode.PDA);
            if (dockContainerGear != null) dockContainerGear.SetActive(mode == ConsoleMode.Gear);

            UpdateModeButtonVisuals();

            if (mode == ConsoleMode.PDA) RefreshAllPDAPanels();
            else if (mode == ConsoleMode.Gear) RefreshRightGearPanel();

            if (playSound) PlayClickSFX();
        }

        private void UpdateModeButtonVisuals()
        {
            if (modeIndicatorNarrative != null) modeIndicatorNarrative.gameObject.SetActive(currentConsoleMode == ConsoleMode.Narrative);
            if (modeIndicatorPDA != null) modeIndicatorPDA.gameObject.SetActive(currentConsoleMode == ConsoleMode.PDA);
            if (modeIndicatorGear != null) modeIndicatorGear.gameObject.SetActive(currentConsoleMode == ConsoleMode.Gear);
        }

        private void Update()
        {
            // 0a. Hotkey (Escape) to dismiss System Menu Modal if open
            if (systemMenuModal != null && systemMenuModal.activeSelf)
            {
                bool escSys = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null) escSys = Keyboard.current.escapeKey.wasPressedThisFrame;
#else
                escSys = Input.GetKeyDown(KeyCode.Escape);
#endif
                if (escSys)
                {
                    if (sysMainPanel != null && !sysMainPanel.activeSelf)
                    {
                        SwitchSystemMenuView(SystemMenuView.Main);
                    }
                    else
                    {
                        CloseSystemMenuModal();
                    }
                    return;
                }
            }

            // 0. Hotkey to dismiss Phone Modal if open (1, Escape, Space, Enter)
            if (phoneModal != null && phoneModal.activeSelf)
            {
                bool dismissPhone = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    dismissPhone = Keyboard.current.escapeKey.wasPressedThisFrame ||
                                   Keyboard.current.digit1Key.wasPressedThisFrame ||
                                   Keyboard.current.numpad1Key.wasPressedThisFrame ||
                                   Keyboard.current.spaceKey.wasPressedThisFrame ||
                                   Keyboard.current.enterKey.wasPressedThisFrame ||
                                   Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                }
#else
                dismissPhone = Input.GetKeyDown(KeyCode.Escape) ||
                               Input.GetKeyDown(KeyCode.Alpha1) ||
                               Input.GetKeyDown(KeyCode.Keypad1) ||
                               Input.GetKeyDown(KeyCode.Space) ||
                               Input.GetKeyDown(KeyCode.Return) ||
                               Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
                if (dismissPhone)
                {
                    OnClosePhoneClicked();
                    return;
                }
            }

            // 1. Hotkeys for Choice Selection (1-8) when in Narrative Choice Hub Mode
            if (currentConsoleMode == ConsoleMode.Narrative && hubMenuContainer != null && hubMenuContainer.activeInHierarchy)
            {
                for (int i = 0; i < Mathf.Min(activeChoiceButtons.Count, 8); i++)
                {
                    if (IsChoiceHotkeyPressed(i + 1))
                    {
                        activeChoiceButtons[i].TriggerClick();
                        break;
                    }
                }
            }

            // 2. Hotkeys for Console Mode Switching (Tab, I, Esc, F1-F3)
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.f2Key.wasPressedThisFrame)
                {
                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);
                }
                else if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.f3Key.wasPressedThisFrame)
                {
                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);
                }
                else if ((Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame) && currentConsoleMode != ConsoleMode.Narrative)
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
                else if (currentConsoleMode != ConsoleMode.Narrative && (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame))
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.F2))
            {
                SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);
            }
            else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.F3))
            {
                SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);
            }
            else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F1)) && currentConsoleMode != ConsoleMode.Narrative)
            {
                SwitchConsoleMode(ConsoleMode.Narrative);
            }
            else if (currentConsoleMode != ConsoleMode.Narrative && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))
            {
                SwitchConsoleMode(ConsoleMode.Narrative);
            }
#endif

            // Debug shortcuts for FEEL Feedback testing (F6: Screen Shake, F7: Low Humanity Pulse)
            bool testShake = false;
            bool testLowHum = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                testShake = Keyboard.current.f6Key.wasPressedThisFrame;
                testLowHum = Keyboard.current.f7Key.wasPressedThisFrame;
            }
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F6)) testShake = true;
            if (Input.GetKeyDown(KeyCode.F7)) testLowHum = true;
#endif
            if (testShake) PlayScreenShake();
            if (testLowHum) DebugTestDamageFeedback();

            // Debounced multi-input check (Space or Enter to advance awakening dialogue)
            if (!introSeen && awakeningContainer != null && awakeningContainer.activeSelf && currentConsoleMode == ConsoleMode.Narrative)
            {
                bool spacePressed = false;
                bool enterPressed = false;

#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
                    enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                }
#else
                spacePressed = Input.GetKeyDown(KeyCode.Space);
                enterPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif

                if (spacePressed || enterPressed)
                {
                    OnDialogueAreaClicked();
                }
            }
        }

        // Note: Global OnPointerClick removed to prevent background/tab clicks from advancing dialogue.

        private void OnDialogueAreaClicked()
        {
            if (introSeen) return;

            // Debounce double-clicks
            if (Time.unscaledTime - lastClickTime < clickDebounceDelay)
            {
                return;
            }
            lastClickTime = Time.unscaledTime;

            if (isTyping)
            {
                // Instantly complete typewriter
                CompleteTypewriterInstant();
                return;
            }

            PlayClickSFX();

            // Advance to next awakening beat
            awakeningStep++;
            if (awakeningStep > 3)
            {
                introSeen = true;
                ShowHubMenu();
            }
            else
            {
                ShowAwakeningStep(awakeningStep);
            }
        }

        #region Left & Right Embedded Survivor PDA Logic
        private void SwitchPDATab(string tabName, bool playSound = true)
        {
            if (playSound) PlayClickSFX();

            if (pdaPanelStatus != null) pdaPanelStatus.SetActive(tabName == "STATUS");
            if (pdaPanelSkills != null) pdaPanelSkills.SetActive(tabName == "SKILLS");
            if (pdaPanelTraits != null) pdaPanelTraits.SetActive(tabName == "TRAITS");
            if (pdaPanelMap != null) pdaPanelMap.SetActive(tabName == "MAP");
            if (pdaPanelLore != null) pdaPanelLore.SetActive(tabName == "LORE");

            if (tabName == "MAP" && tacticalMapController != null)
            {
                tacticalMapController.RefreshAllNodes();
            }
            if (tabName == "SKILLS" && tacticalSkillsController != null && currentPlayer != null)
            {
                tacticalSkillsController.Refresh(currentPlayer);
            }

            UpdateTabButtonVisual(pdaTabStatusBtn, tabName == "STATUS");
            UpdateTabButtonVisual(pdaTabSkillsBtn, tabName == "SKILLS");
            UpdateTabButtonVisual(pdaTabTraitsBtn, tabName == "TRAITS");
            UpdateTabButtonVisual(pdaTabMapBtn, tabName == "MAP");
            UpdateTabButtonVisual(pdaTabLoreBtn, tabName == "LORE");
        }

        private void UpdateTabButtonVisual(Button btn, bool isActive)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isActive
                    ? new Color(0.18f, 0.20f, 0.23f, 1f)
                    : new Color(0.08f, 0.09f, 0.11f, 0.95f);
            }

            Transform accent = btn.transform.Find("BottomAccent");
            if (accent != null) accent.gameObject.SetActive(isActive);

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.color = isActive
                    ? new Color(0.95f, 0.48f, 0.12f, 1f) // Twine PDA orange #f39c12 / #e67e22
                    : new Color(0.55f, 0.58f, 0.62f, 1f);
            }
        }

        private void RefreshAllPDAPanels()
        {
            RefreshPDAStatusTab();
            RefreshPDASkillsTab();
            RefreshPDATraitsTab();
            RefreshPDALoreTab();
            RefreshRightGearPanel();
        }

        private void RefreshPDAStatusTab()
        {
            if (currentPlayer != null)
            {
                if (pdaCharNameText != null) pdaCharNameText.text = currentPlayer.FullName;
                if (pdaCharSubText != null) pdaCharSubText.text = $"{currentPlayer.gender}  |  {currentPlayer.job}";

                int hum = Mathf.Clamp(currentPlayer.humanity, 0, 100);
                if (pdaHumanityBarFill != null) pdaHumanityBarFill.fillAmount = hum / 100f;
                if (pdaHumanityValueText != null) pdaHumanityValueText.text = $"{hum}/100";
            }

            if (pdaLevelText != null) pdaLevelText.text = "LVL 1";
            if (pdaXpText != null) pdaXpText.text = "0/100 XP";
            if (pdaXpBarFill != null) pdaXpBarFill.fillAmount = 0f;

            if (pdaActiveLogsText != null && objectiveSystem != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var obj in objectiveSystem.Objectives)
                {
                    if (obj.status == ObjectiveStatus.Completed)
                    {
                        sb.AppendLine($"<color=#546e7a>[v] <s>{obj.description}</s></color>");
                    }
                    else if (obj.status == ObjectiveStatus.Active)
                    {
                        sb.AppendLine($"<color=#e67e22>[ ]</color> <color=#e8eaed>{obj.description}</color>");
                    }
                    else
                    {
                        sb.AppendLine($"<color=#e67e22>[ ]</color> <color=#90a4ae>{obj.description}</color>");
                    }
                }
                pdaActiveLogsText.text = sb.ToString().TrimEnd();
            }
        }

        private void RefreshPDASkillsTab()
        {
            if (currentPlayer == null) return;

            if (tacticalSkillsController != null)
            {
                tacticalSkillsController.Refresh(currentPlayer);
            }

            if (pdaAttributesText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(FormatStatBarRow("STR", currentPlayer.str, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("PER", currentPlayer.per, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("AGI", currentPlayer.agi, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("INT", currentPlayer.intl, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("CHA", currentPlayer.cha, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("LCK", currentPlayer.lck, 10, "#2ecc71"));
                pdaAttributesText.text = sb.ToString().TrimEnd();
            }

            if (pdaSkillsText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(FormatStatBarRow("SHOOTING", currentPlayer.shooting, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("MELEE", currentPlayer.melee, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("STEALTH", currentPlayer.stealth, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("TECH", currentPlayer.tech, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("MEDICINE", currentPlayer.medicine, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("SCAVENGE", currentPlayer.scavenging, 5, "#f39c12"));
                pdaSkillsText.text = sb.ToString().TrimEnd();
            }
        }

        private string FormatStatBarRow(string label, int val, int max, string hexColor)
        {
            int clamped = Mathf.Clamp(val, 0, max);
            string filled = new string('■', clamped);
            string empty = new string('■', Mathf.Max(0, max - clamped));
            return $"<b>{label.PadRight(9)}</b> <color={hexColor}>{filled}</color><color=#22262c>{empty}</color>  <color=#ffffff>[ {val} ]</color>";
        }

        private void RefreshPDATraitsTab()
        {
            if (pdaTraitsListRoot == null) return;

            for (int i = pdaTraitsListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(pdaTraitsListRoot.GetChild(i).gameObject);
            }

            if (currentPlayer == null || currentPlayer.traits == null || currentPlayer.traits.Count == 0)
            {
                GameObject emptyObj = new GameObject("EmptyTraits");
                emptyObj.transform.SetParent(pdaTraitsListRoot, false);
                TextMeshProUGUI tmp = emptyObj.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 11.5f;
                tmp.color = new Color(0.55f, 0.58f, 0.62f);
                tmp.text = "No specialized traits or perks recorded.";
                RectTransform rt = emptyObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0f, 36f);
                return;
            }

            foreach (var trait in currentPlayer.traits)
            {
                CreateTraitCardUI(pdaTraitsListRoot, trait);
            }
        }

        private void CreateTraitCardUI(Transform parent, TraitEntry trait)
        {
            GameObject card = new GameObject("Trait_" + trait.name);
            card.transform.SetParent(parent, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 64f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.flexibleWidth = 1f;

            Image bg = card.AddComponent<Image>();
            bg.color = trait.isWeakness
                ? new Color(0.12f, 0.04f, 0.04f, 0.95f)
                : new Color(0.04f, 0.10f, 0.05f, 0.95f);

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = trait.isWeakness
                ? new Color(0.55f, 0.16f, 0.16f, 0.85f)
                : new Color(0.16f, 0.45f, 0.22f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Icon (Left 44x44)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            RectTransform ir = iconObj.AddComponent<RectTransform>();
            ir.anchorMin = new Vector2(0f, 0.5f);
            ir.anchorMax = new Vector2(0f, 0.5f);
            ir.pivot = new Vector2(0f, 0.5f);
            ir.anchoredPosition = new Vector2(10f, 0f);
            ir.sizeDelta = new Vector2(44f, 44f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            Sprite s = trait.icon != null ? trait.icon : ItemDatabase.GetTraitIcon(trait.name);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.2f, 0.25f, 0.2f, 0.6f);
            }

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 12.5f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.color = trait.isWeakness
                ? new Color(0.95f, 0.35f, 0.35f)
                : new Color(0.28f, 0.88f, 0.45f);
            titleTmp.text = trait.name.ToUpperInvariant();
            titleTmp.textWrappingMode = TextWrappingModes.NoWrap;
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 0.5f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(64f, 0f);
            tr.offsetMax = new Vector2(-10f, -4f);

            // Description
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 10.5f;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            descTmp.color = new Color(0.78f, 0.80f, 0.84f);
            descTmp.text = trait.description;
            descTmp.textWrappingMode = TextWrappingModes.Normal;
            RectTransform dr = descObj.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0f, 0f);
            dr.anchorMax = new Vector2(1f, 0.5f);
            dr.offsetMin = new Vector2(64f, 4f);
            dr.offsetMax = new Vector2(-10f, 0f);
        }

        private void RefreshPDALoreTab()
        {
            if (pdaLoreText == null) return;

            if (loreSystem == null || loreSystem.Entries.Count == 0)
            {
                pdaLoreText.text = "<color=#6c7a89><i>No intelligence logs recorded yet.\n\nUse your Phone, Emergency Radio, or Laptop in the apartment to gather outbreak intel.</i></color>";
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var entry in loreSystem.Entries)
            {
                sb.AppendLine($"<color=#f39c12><b>■ {entry.title.ToUpperInvariant()}</b></color> <color=#6c7a89>[{entry.timestamp}]</color>");
                sb.AppendLine($"<color=#d0d6dc>{entry.content}</color>\n");
            }
            pdaLoreText.text = sb.ToString().TrimEnd();
        }

        private void AutoEquipFromInventory()
        {
            // Deprecated: Player manually inspects and equips weapons/gear via Tactical Gear Workstation
        }

        private void UpdateFloorItemsNotification()
        {
            int count = floorItems != null ? floorItems.Count : 0;
            bool hasLoot = count > 0;

            if (btnModeGearLabel != null)
            {
                btnModeGearLabel.text = hasLoot 
                    ? $"BACKPACK & ITEMS [I] <color=#F59E0B><b>({count})</b></color>" 
                    : "BACKPACK & ITEMS [I]";
            }

            if (btnModeGearBadge != null)
            {
                btnModeGearBadge.SetActive(hasLoot);
            }

            if (btnModeGear != null)
            {
                var rect = btnModeGear.GetComponent<RectTransform>();
                if (rect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.gameObject.SetActive(hasLoot);
            }

            if (floorItemsCountText != null)
            {
                floorItemsCountText.text = hasLoot ? $"TAKE ALL ({count})" : "TAKE ALL";
            }
        }

        private void RefreshRightGearPanel()
        {
            UpdateFloorItemsNotification();

            if (tacticalGearController != null)
            {
                tacticalGearController.Initialize(currentPlayer, floorItems);
                return;
            }

            // Sync local equip strings with currentPlayer
            if (currentPlayer != null)
            {
                if (!string.IsNullOrEmpty(currentPlayer.equippedHead)) equippedHead = currentPlayer.equippedHead;
                if (!string.IsNullOrEmpty(currentPlayer.equippedBody)) equippedBody = currentPlayer.equippedBody;
                if (!string.IsNullOrEmpty(currentPlayer.equippedWpn)) equippedWpn = currentPlayer.equippedWpn;
                if (!string.IsNullOrEmpty(currentPlayer.equippedLegs)) equippedLegs = currentPlayer.equippedLegs;
                if (!string.IsNullOrEmpty(currentPlayer.equippedFeet)) equippedFeet = currentPlayer.equippedFeet;
            }

            // 1. Update Active Loadout Slots on Body Wireframe
            UpdateEquipSlotVisual(equipHeadIcon, equipHeadLabel, equippedHead, "HEAD");
            UpdateEquipSlotVisual(equipBodyIcon, equipBodyLabel, equippedBody, "BODY");
            UpdateEquipSlotVisual(equipWpnIcon, equipWpnLabel, equippedWpn, "WPN");
            UpdateEquipSlotVisual(equipLegsIcon, equipLegsLabel, equippedLegs, "LEGS");
            UpdateEquipSlotVisual(equipFeetIcon, equipFeetLabel, equippedFeet, "FEET");

            // 2. Update Backpack 12-Slot Grid (Exact Twine replica)
            if (pdaBackpackGrid != null)
            {
                for (int i = pdaBackpackGrid.childCount - 1; i >= 0; i--)
                {
                    Destroy(pdaBackpackGrid.GetChild(i).gameObject);
                }

                List<InventoryEntry> inv = currentPlayer != null && currentPlayer.inventory != null
                    ? currentPlayer.inventory
                    : new List<InventoryEntry>();

                int maxSlots = currentPlayer != null ? Mathf.Max(6, currentPlayer.bagLimit) : 12;
                int currentUsed = currentPlayer != null ? currentPlayer.GetTotalSlotsUsed() : inv.Count;
                if (pdaBackpackCountText != null)
                {
                    pdaBackpackCountText.text = $"{currentUsed}/{maxSlots}";
                }

                int slotsRendered = 0;
                for (int i = 0; i < inv.Count; i++)
                {
                    int size = ItemDatabase.GetItemSlotSize(inv[i].itemId);
                    if (size == 2)
                    {
                        if (slotsRendered % 5 == 4 && slotsRendered < maxSlots)
                        {
                            CreateInventorySlotCell(pdaBackpackGrid, null, 0, false, isExtension: false, isTwoSlot: false);
                            slotsRendered++;
                        }
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, inv[i].quantity, true, isExtension: false, isTwoSlot: true);
                        slotsRendered++;
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, 0, true, isExtension: true, isTwoSlot: false);
                        slotsRendered++;
                    }
                    else
                    {
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, inv[i].quantity, true, isExtension: false, isTwoSlot: false);
                        slotsRendered++;
                    }
                }

                while (slotsRendered < maxSlots)
                {
                    CreateInventorySlotCell(pdaBackpackGrid, null, 0, false, isExtension: false, isTwoSlot: false);
                    slotsRendered++;
                }
            }

            // 3. Update Floor / Room Items in Right Wing
            bool hasFloorLoot = floorItems != null && floorItems.Count > 0;
            if (floorItemsCard != null)
            {
                floorItemsCard.SetActive(hasFloorLoot);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.gameObject.SetActive(hasFloorLoot);
            }

            UpdateFloorItemsNotification();

            if (pdaFloorItemsGrid != null)
            {
                for (int i = pdaFloorItemsGrid.childCount - 1; i >= 0; i--)
                {
                    Destroy(pdaFloorItemsGrid.GetChild(i).gameObject);
                }

                if (pdaFloorEmptyText != null)
                {
                    pdaFloorEmptyText.gameObject.SetActive(!hasFloorLoot);
                }

                if (hasFloorLoot)
                {
                    foreach (var item in floorItems)
                    {
                        CreateFloorItemCell(pdaFloorItemsGrid, item.itemId, item.quantity);
                    }
                }
            }
        }

        private void UpdateEquipSlotVisual(Image iconImg, TextMeshProUGUI slotLabel, string equippedItemId, string defaultSlotName)
        {
            if (iconImg == null || slotLabel == null) return;

            if (!string.IsNullOrEmpty(equippedItemId))
            {
                Sprite s = ItemDatabase.GetItemIcon(equippedItemId);
                if (s != null)
                {
                    iconImg.gameObject.SetActive(true);
                    iconImg.sprite = s;
                    iconImg.color = Color.white;
                    slotLabel.text = "";
                    return;
                }
            }

            iconImg.gameObject.SetActive(false);
            slotLabel.text = defaultSlotName;
        }

        private void CreateInventorySlotCell(Transform parent, string itemId, int quantity, bool allowEquipClick, bool isExtension = false, bool isTwoSlot = false)
        {
            GameObject cell = new GameObject(string.IsNullOrEmpty(itemId) ? "EmptySlot" : (isExtension ? "Slot_" + itemId + "_Ext" : "Slot_" + itemId));
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(74f, 74f);

            if (isExtension)
            {
                // Companion cell is an invisible layout placeholder in GridLayoutGroup
                Image extBg = cell.AddComponent<Image>();
                extBg.color = new Color(0f, 0f, 0f, 0f);
                extBg.raycastTarget = false;
                return;
            }

            GameObject visualRoot;
            if (isTwoSlot)
            {
                visualRoot = new GameObject("SpanRoot");
                visualRoot.transform.SetParent(cell.transform, false);
                RectTransform vrt = visualRoot.AddComponent<RectTransform>();
                vrt.anchorMin = new Vector2(0f, 0f);
                vrt.anchorMax = new Vector2(0f, 1f);
                vrt.pivot = new Vector2(0f, 0.5f);
                vrt.anchoredPosition = Vector2.zero;
                vrt.sizeDelta = new Vector2(158f, 0f); // 74 * 2 + 10 spacing = 158px
            }
            else
            {
                visualRoot = cell;
            }

            Image bg = visualRoot.AddComponent<Image>();
            bg.color = new Color(0.09f, 0.11f, 0.15f, 0.95f);

            Outline ol = visualRoot.AddComponent<Outline>();
            ol.effectColor = string.IsNullOrEmpty(itemId)
                ? new Color(0.18f, 0.20f, 0.24f, 0.75f)
                : new Color(0.28f, 0.32f, 0.38f, 0.95f);
            ol.effectDistance = new Vector2(1f, -1f);

            if (string.IsNullOrEmpty(itemId)) return;

            // Contrast Well Halo
            GameObject wellObj = new GameObject("WellHalo");
            wellObj.transform.SetParent(visualRoot.transform, false);
            Image wellImg = wellObj.AddComponent<Image>();
            wellImg.color = new Color(0.18f, 0.24f, 0.32f, 0.25f);
            wellImg.raycastTarget = false;
            RectTransform wrt = wellObj.GetComponent<RectTransform>();
            wrt.anchorMin = Vector2.zero; wrt.anchorMax = Vector2.one;
            wrt.offsetMin = new Vector2(4f, 12f); wrt.offsetMax = new Vector2(-4f, -4f);

            // 2-Slot badge if isTwoSlot
            if (isTwoSlot)
            {
                GameObject slotBadge = new GameObject("SlotBadge");
                slotBadge.transform.SetParent(visualRoot.transform, false);
                RectTransform sbr = slotBadge.AddComponent<RectTransform>();
                sbr.anchorMin = new Vector2(1f, 1f); sbr.anchorMax = new Vector2(1f, 1f);
                sbr.pivot = new Vector2(1f, 1f);
                sbr.anchoredPosition = new Vector2(-4f, -4f);
                sbr.sizeDelta = new Vector2(36f, 13f);
                Image sbBg = slotBadge.AddComponent<Image>();
                sbBg.color = new Color(0.85f, 0.55f, 0.10f, 0.32f);
                sbBg.raycastTarget = false;

                GameObject sbtObj = new GameObject("Text");
                sbtObj.transform.SetParent(slotBadge.transform, false);
                TextMeshProUGUI sbTmp = sbtObj.AddComponent<TextMeshProUGUI>();
                sbTmp.fontSize = 7f;
                sbTmp.fontStyle = FontStyles.Bold;
                sbTmp.alignment = TextAlignmentOptions.Center;
                sbTmp.color = new Color(0.95f, 0.75f, 0.25f);
                sbTmp.text = "2-SLOT";
                sbTmp.raycastTarget = false;
                RectTransform sbtr = sbtObj.GetComponent<RectTransform>();
                sbtr.anchorMin = Vector2.zero; sbtr.anchorMax = Vector2.one;
                sbtr.offsetMin = Vector2.zero; sbtr.offsetMax = Vector2.zero;
            }

            // Item Icon - Centered horizontally across the full visual span
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(visualRoot.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(isTwoSlot ? 10f : 6f, 14f);
            ir.offsetMax = new Vector2(isTwoSlot ? -10f : -6f, -4f);

            Sprite s = ItemDatabase.GetItemIcon(itemId);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.35f, 0.4f, 0.45f, 0.5f);
            }

            // Short Item Name at bottom
            GameObject nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(visualRoot.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.78f, 0.82f, 0.86f);
            nameTmp.text = isTwoSlot ? itemId : (itemId.Length > 10 ? itemId.Substring(0, 9) + "." : itemId);
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f);
            nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 14f);
            nr.anchoredPosition = new Vector2(0f, 1f);

            // Quantity badge (Twine style: bottom-right dark box with green x24 text)
            if (quantity > 1)
            {
                GameObject qBadge = new GameObject("QtyBadge");
                qBadge.transform.SetParent(visualRoot.transform, false);
                RectTransform qr = qBadge.AddComponent<RectTransform>();
                qr.anchorMin = new Vector2(1f, 0f);
                qr.anchorMax = new Vector2(1f, 0f);
                qr.pivot = new Vector2(1f, 0f);
                qr.anchoredPosition = new Vector2(-2f, 13f);
                qr.sizeDelta = new Vector2(26f, 14f);
                Image qBg = qBadge.AddComponent<Image>();
                qBg.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
                qBg.raycastTarget = false;

                GameObject qtObj = new GameObject("Text");
                qtObj.transform.SetParent(qBadge.transform, false);
                TextMeshProUGUI qTmp = qtObj.AddComponent<TextMeshProUGUI>();
                qTmp.fontSize = 9f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.35f, 0.95f, 0.45f);
                qTmp.text = $"x{quantity}";
                qTmp.raycastTarget = false;
                RectTransform qtr = qtObj.GetComponent<RectTransform>();
                qtr.anchorMin = Vector2.zero;
                qtr.anchorMax = Vector2.one;
                qtr.offsetMin = Vector2.zero;
                qtr.offsetMax = Vector2.zero;
            }

            if (allowEquipClick)
            {
                Button btn = cell.AddComponent<Button>();
                btn.targetGraphic = bg;
                string capturedItem = itemId;
                btn.onClick.AddListener(() =>
                {
                    string slot = ItemDatabase.GetEquipSlot(capturedItem);
                    if (!string.IsNullOrEmpty(slot))
                    {
                        PlayClickSFX();
                        if (slot == "HEAD") equippedHead = capturedItem;
                        else if (slot == "BODY") equippedBody = capturedItem;
                        else if (slot == "WPN") equippedWpn = capturedItem;
                        else if (slot == "LEGS") equippedLegs = capturedItem;
                        else if (slot == "FEET") equippedFeet = capturedItem;
                        RefreshRightGearPanel();
                    }
                });
            }
        }

        private void CreateFloorItemCell(Transform parent, string itemId, int quantity)
        {
            GameObject cell = new GameObject(string.IsNullOrEmpty(itemId) ? "EmptySlot" : "FloorSlot_" + itemId);
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(66f, 66f);

            Image bg = cell.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.08f, 0.08f, 0.95f);

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = new Color(0.91f, 0.30f, 0.24f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            if (string.IsNullOrEmpty(itemId)) return;

            // Item Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cell.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(6f, 14f);
            ir.offsetMax = new Vector2(-6f, -4f);

            Sprite s = ItemDatabase.GetItemIcon(itemId);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.35f, 0.4f, 0.45f, 0.5f);
            }

            // Short Item Name
            GameObject nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.95f, 0.85f, 0.85f);
            nameTmp.text = itemId.Length > 10 ? itemId.Substring(0, 9) + "." : itemId;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f);
            nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 14f);
            nr.anchoredPosition = new Vector2(0f, 1f);

            // Quantity badge
            if (quantity > 1)
            {
                GameObject qBadge = new GameObject("QtyBadge");
                qBadge.transform.SetParent(cell.transform, false);
                RectTransform qr = qBadge.AddComponent<RectTransform>();
                qr.anchorMin = new Vector2(1f, 0f);
                qr.anchorMax = new Vector2(1f, 0f);
                qr.pivot = new Vector2(1f, 0f);
                qr.anchoredPosition = new Vector2(-2f, 13f);
                qr.sizeDelta = new Vector2(26f, 14f);
                Image qBg = qBadge.AddComponent<Image>();
                qBg.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
                qBg.raycastTarget = false;

                GameObject qtObj = new GameObject("Text");
                qtObj.transform.SetParent(qBadge.transform, false);
                TextMeshProUGUI qTmp = qtObj.AddComponent<TextMeshProUGUI>();
                qTmp.fontSize = 9f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.35f, 0.95f, 0.45f);
                qTmp.text = $"x{quantity}";
                qTmp.raycastTarget = false;
                RectTransform qtr = qtObj.GetComponent<RectTransform>();
                qtr.anchorMin = Vector2.zero; qtr.anchorMax = Vector2.one; qtr.offsetMin = Vector2.zero; qtr.offsetMax = Vector2.zero;
            }

            // Click button to take this specific item
            Button btn = cell.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capturedId = itemId;
            btn.onClick.AddListener(() =>
            {
                OnTakeSingleFloorItemClicked(capturedId);
            });
        }
        #endregion

        #region Awakening Sequence (Beats 1 - 3)
        private void ShowAwakeningStep(int step)
        {
            if (awakeningContainer != null) awakeningContainer.SetActive(true);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(true);

            if (beat1Container != null) beat1Container.SetActive(false);
            if (beat2Container != null) beat2Container.SetActive(false);
            if (beat3Container != null) beat3Container.SetActive(false);

            switch (step)
            {
                case 1:
                    // Beat 1: Civil Danger Warning (EAS Broadcast)
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (beat1Container != null) beat1Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#EF4444>CHANNEL: EMERGENCY BROADCAST SYSTEM</color>";

                    // Play emergency warning TTS / robotic broadcast
                    if (ebsBroadcastSource != null && ebsWarningClip != null)
                    {
                        ebsBroadcastSource.clip = ebsWarningClip;
                        ebsBroadcastSource.volume = 0.85f;
                        ebsBroadcastSource.Play();
                    }

                    StartTypewriter(
                        beat1IntroText,
                        "You turn away from the glass. The silence in your apartment breaks—not with a scream, but with an agonizing, electronic tone pulsing from the television."
                    );
                    break;

                case 2:
                    // Beat 2: The Reaction (EAS cuts abruptly into TV static hiss)
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (beat2Container != null) beat2Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // INTERNAL MONOLOGUE ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#60A5FA>CHANNEL: NEURAL APPRAISAL</color>";

                    // Stop EAS broadcast, start low TV static
                    if (ebsBroadcastSource != null) ebsBroadcastSource.Stop();
                    if (staticSource != null)
                    {
                        if (staticClip != null && !staticClip.name.Contains("radio_static_ebs"))
                        {
                            staticSource.clip = staticClip;
                            staticSource.volume = 0.2f;
                            staticSource.Play();
                        }
                        else
                        {
                            staticSource.Stop();
                        }
                    }

                    if (monologueBox != null)
                    {
                        monologueBox.SetActive(true);
                        StartAvatarAnimation();
                        if (monologueSpeakerText != null)
                        {
                            monologueSpeakerText.text = currentPlayer != null ? $"YOU // {currentPlayer.FullName.ToUpper()}" : "YOU";
                        }
                        if (monologueSpeechText != null)
                        {
                            monologueSpeechText.text = GetPlayerMonologueText();
                        }
                    }

                    if (beat2IntroText != null)
                    {
                        beat2IntroText.text = "The robotic voice cuts out. Cold static returns to the TV.";
                    }

                    StartTypewriter(
                        beat2FootnoteText,
                        "Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door."
                    );
                    break;

                case 3:
                    // Beat 3: Table Stash
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (staticSource != null) staticSource.Stop();
                    if (beat3Container != null) beat3Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#F59E0B>CHANNEL: EQUIPMENT INSPECTION</color>";

                    if (tableStashDisplay != null)
                    {
                        tableStashDisplay.SetActive(true);
                        PopulateTableStashUI();
                    }

                    StartTypewriter(
                        beat3NarrativeText,
                        "You stand in the middle of your living room in the pitch-black darkness. Outside, sirens wail across the Seattle skyline.\n\n" +
                        "Your apartment keys, wallet, and belongings are sitting on the coffee table. The front door deadbolt is locked—you cannot step out into the hallway without your keys, or you'll be locked out in the dark.\n\n" +
                        "Grab your apartment keys and belongings off the coffee table, and check what else you can gather around the apartment before unlocking the door."
                    );
                    break;
            }
        }

        private string GetPlayerMonologueText()
        {
            if (currentPlayer == null) return "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"";

            if (currentPlayer.HasTrait("PARANOID"))
            {
                return "\"'Shelter in place.' Right. Wait in a fourth-floor box with a single exit while the military shoots anything that moves. That's not a shelter. That's a coffin.\"";
            }
            if (string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase))
            {
                return "\"'Lethal force authorized...' The Guard doesn't deploy for a standard riot. If they establish a quarantine cordon around this block, I'll be trapped here.\"";
            }
            if (string.Equals(currentPlayer.job, "Doctor", StringComparison.OrdinalIgnoreCase) || currentPlayer.HasTrait("FIELD MEDIC"))
            {
                return "\"'Biological hazard. Extreme violence.' In an apartment complex with shared ventilation and paper-thin walls... staying here is suicide.\"";
            }

            return "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"";
        }

        private void StartTypewriter(TextMeshProUGUI target, string fullText)
        {
            activeTypewriterTarget = target;
            activeFullText = fullText;

            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }

            if (!gameObject.activeInHierarchy)
            {
                if (target != null) target.text = fullText;
                isTyping = false;
                return;
            }

            typeRoutine = StartCoroutine(TypewriterRoutine(target, fullText));
        }

        private IEnumerator TypewriterRoutine(TextMeshProUGUI target, string text)
        {
            isTyping = true;
            if (target != null) target.text = "";

            float delay = 1f / Mathf.Max(1f, typewriterSpeed);
            for (int i = 0; i <= text.Length; i++)
            {
                if (target != null)
                {
                    target.text = text.Substring(0, i);
                }
                yield return new WaitForSecondsRealtime(delay);
            }

            isTyping = false;
        }

        private void CompleteTypewriterInstant()
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            if (activeTypewriterTarget != null) activeTypewriterTarget.text = activeFullText;
            isTyping = false;
        }

        private void StartAvatarAnimation()
        {
            if (avatarAnimRoutine != null)
            {
                StopCoroutine(avatarAnimRoutine);
                avatarAnimRoutine = null;
            }

            if (!gameObject.activeInHierarchy) return;

            if (cachedAvatarFrames != null && cachedAvatarFrames.Count > 0 && monologueAvatar != null)
            {
                avatarAnimRoutine = StartCoroutine(AvatarAnimRoutine());
            }
        }

        private IEnumerator AvatarAnimRoutine()
        {
            int idx = 0;
            float frameDelay = 1f / 8f;
            while (monologueBox != null && monologueBox.activeSelf)
            {
                if (cachedAvatarFrames.Count > 0 && monologueAvatar != null)
                {
                    monologueAvatar.sprite = cachedAvatarFrames[idx];
                    idx = (idx + 1) % cachedAvatarFrames.Count;
                }
                yield return new WaitForSecondsRealtime(frameDelay);
            }
        }
        #endregion

        #region Hub Menu Mode
        private void ShowHubMenu()
        {
            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);

            if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
            if (dockHeaderChannel != null) dockHeaderChannel.text = hasPacked ? "<color=#10B981>DOOR READY TO UNLOCK</color>" : "<color=#EF4444>FRONT DOOR LOCKED (NEED KEYS)</color>";

            EvaluateHubButtonStates();
        }

        private bool IsChoiceHotkeyPressed(int number)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                switch (number)
                {
                    case 1: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                    case 2: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                    case 3: return kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame;
                    case 4: return kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame;
                    case 5: return kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame;
                    case 6: return kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame;
                    case 7: return kb.digit7Key.wasPressedThisFrame || kb.numpad7Key.wasPressedThisFrame;
                    case 8: return kb.digit8Key.wasPressedThisFrame || kb.numpad8Key.wasPressedThisFrame;
                    case 9: return kb.digit9Key.wasPressedThisFrame || kb.numpad9Key.wasPressedThisFrame;
                }
            }
#endif
            KeyCode alpha = (KeyCode)((int)KeyCode.Alpha0 + number);
            KeyCode pad = (KeyCode)((int)KeyCode.Keypad0 + number);
            return Input.GetKeyDown(alpha) || Input.GetKeyDown(pad);
        }

        private void ReindexActiveChoiceHotkeys()
        {
            activeChoiceButtons.Clear();
            if (hubMenuContainer == null) return;

            ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);
            Transform parent = sr != null && sr.content != null ? sr.content.transform : hubMenuContainer.transform;

            int index = 1;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    TacticalChoiceButton tcb = child.GetComponent<TacticalChoiceButton>();
                    if (tcb != null)
                    {
                        tcb.SetChoiceNumber(index);
                        activeChoiceButtons.Add(tcb);
                        index++;
                    }
                }
            }
        }

        private void EvaluateHubButtonStates()
        {
            // 1. Pack Gear Button (Coffee Table)
            if (packGearButton != null)
            {
                packGearButton.gameObject.SetActive(!hasPacked);
                SetChoiceButtonContent(packGearButton, "<b>Pick up my apartment keys, wallet, and work gear from the coffee table.</b>  <color=#64748B><i>(~2 min)</i></color>");
            }
            if (packedSuccessBanner != null)
            {
                packedSuccessBanner.SetActive(hasPacked);
            }

            // 2. Kitchenette
            if (searchKitchenButton != null)
            {
                searchKitchenButton.gameObject.SetActive(true);
                searchKitchenButton.interactable = !kitchenSearched;
                SetChoiceButtonContent(searchKitchenButton, kitchenSearched
                    ? "<s><color=#64748B>Check the kitchen cabinets and fridge.</color></s>  <color=#475569><i>(Cabinets and fridge are cleared)</i></color>"
                    : "<b>Check the kitchen cabinets and fridge for bottled water and food.</b>  <color=#64748B><i>(~3 min)</i></color>");
            }

            // 3. Bathroom
            if (searchBathroomButton != null)
            {
                searchBathroomButton.gameObject.SetActive(true);
                searchBathroomButton.interactable = !bathroomSearched;
                SetChoiceButtonContent(searchBathroomButton, bathroomSearched
                    ? "<s><color=#64748B>Check the bathroom medicine cabinet.</color></s>  <color=#475569><i>(Medicine cabinet is cleared)</i></color>"
                    : "<b>Check the bathroom medicine cabinet for painkillers and bandages.</b>  <color=#64748B><i>(~2 min)</i></color>");
            }

            // 4. Closet
            if (searchClosetButton != null)
            {
                searchClosetButton.gameObject.SetActive(true);
                searchClosetButton.interactable = !closetSearched;
                SetChoiceButtonContent(searchClosetButton, closetSearched
                    ? "<s><color=#64748B>Grab my coat and flashlight from the front closet.</color></s>  <color=#475569><i>(Closet is cleared)</i></color>"
                    : "<b>Grab my heavy coat, rain jacket, and flashlight from the front closet.</b>  <color=#64748B><i>(~2 min)</i></color>");
            }

            // 5. Check Phone
            if (checkPhoneButton != null)
            {
                checkPhoneButton.gameObject.SetActive(!phoneVisited);
                SetChoiceButtonContent(checkPhoneButton, "<b>Check my phone to see if anyone knows what is happening outside.</b>  <color=#64748B><i>(~1 min)</i></color>");
            }

            // 6. Listen to Emergency Band / Radio
            if (listenRadioButton != null)
            {
                if (radioVisited)
                {
                    listenRadioButton.gameObject.SetActive(false);
                }
                else
                {
                    listenRadioButton.gameObject.SetActive(true);
                    bool isPolice = currentPlayer != null && string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
                    bool hasTech1 = currentPlayer != null && currentPlayer.tech >= 1 && currentPlayer.Tactics >= 4;

                    if (isPolice)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Turn on the radio and tune in to the emergency police dispatch band.</b>  <color=#64748B><i>(~3 min)</i></color>");
                    }
                    else if (hasTech1)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Tune the portable radio to scan for emergency frequencies.</b>  <color=#64748B><i>(~5 min)</i></color>");
                    }
                    else
                    {
                        listenRadioButton.interactable = false;
                        SetChoiceButtonContent(listenRadioButton, "<s><color=#64748B>Tune the radio to emergency dispatch frequencies.</color></s>  <color=#EF4444><i>[Requires Police or Radio knowledge]</i></color>");
                    }
                }
            }

            // 7. Bypass Gov Firewall / Computer Terminal
            if (hackTerminalButton != null)
            {
                if (hackingVisited)
                {
                    hackTerminalButton.gameObject.SetActive(false);
                }
                else
                {
                    hackTerminalButton.gameObject.SetActive(true);
                    bool hasTech3 = currentPlayer != null && currentPlayer.tech >= 3 && currentPlayer.Tactics >= 6;

                    if (hasTech3)
                    {
                        hackTerminalButton.interactable = true;
                        SetChoiceButtonContent(hackTerminalButton, "<b>Log onto the computer to see if the building network is still responding.</b>  <color=#64748B><i>(~8 min)</i></color>");
                    }
                    else
                    {
                        hackTerminalButton.interactable = false;
                        SetChoiceButtonContent(hackTerminalButton, "<s><color=#64748B>Log onto the computer to inspect encrypted network traffic.</color></s>  <color=#EF4444><i>[Requires Tech knowledge]</i></color>");
                    }
                }
            }

            // 8. Leave Apartment / Unlock Front Door
            if (leaveApartmentButton != null)
            {
                leaveApartmentButton.gameObject.SetActive(true);
                bool hasKey = currentPlayer != null && (currentPlayer.HasItem("Apartment Key") || currentPlayer.HasItem("Apartment 407 Key"));
                bool tableEmpty = tableStash == null || tableStash.Count == 0;
                bool canExit = hasPacked && tableEmpty && hasKey;

                leaveApartmentButton.interactable = true;
                if (!canExit)
                {
                    SetChoiceButtonContent(leaveApartmentButton, "<s><color=#94A3B8>Unlock the apartment door and step out into the hallway.</color></s>  <color=#EF4444><i>[Locked — My keys are still on the coffee table]</i></color>");
                }
                else
                {
                    SetChoiceButtonContent(leaveApartmentButton, "<b><color=#F87171>Unlock the apartment door and step out into the hallway.</color></b>  <color=#64748B><i>(No turning back)</i></color>");
                }
            }

            UpdateFloorItemsNotification();

            // 7. Dynamic Numbering & Hotkey Indexing (Contiguous [ 1 ], [ 2 ], [ 3 ]...)
            ReindexActiveChoiceHotkeys();
        }

        private void AddFloorItem(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return;
            var existing = floorItems.Find(f => string.Equals(f.itemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                floorItems.Add(new InventoryEntry(itemId, quantity));
            }
        }

        private void OnPackGearClicked()
        {
            PlayClickSFX();

            introSeen = true;
            awakeningStep = 4;

            // Advance time +2 minutes
            gameClock.AddTime(2);

            // Move table stash into player inventory, overflow to floorItems if full
            int gatheredCount = 0;
            if (currentPlayer != null)
            {
                if (!currentPlayer.HasKeyItem("Apartment Key", true) && !currentPlayer.HasKeyItem("Apartment 407 Key", true) && !currentPlayer.HasItem("Apartment Key") && !currentPlayer.HasItem("Apartment 407 Key"))
                {
                    currentPlayer.AddKeyItem("Apartment Key", "Apartment 407 Key", "Brass tumbler key labeled 'Room 407'. Required once to unlock the front door deadbolt.", gameClock.GetTimeString());
                    gatheredCount++;
                }

                if (tableStash.Count == 0)
                {
                    string[] defaults = new[] { "Glock 19", "9mm Ammo", "Multi-Tool", "Flashlight", "Bandages" };
                    int[] defaultQtys = new[] { 1, 24, 1, 1, 2 };
                    for (int d = 0; d < defaults.Length; d++)
                    {
                        if (!currentPlayer.AddItem(defaults[d], defaultQtys[d]))
                        {
                            AddFloorItem(defaults[d], defaultQtys[d]);
                        }
                        gatheredCount++;
                    }
                }
                else
                {
                    foreach (var item in tableStash)
                    {
                        if (string.Equals(item.itemId, "Apartment Key", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(item.itemId, "Apartment 407 Key", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!currentPlayer.AddItem(item.itemId, item.quantity))
                        {
                            // If backpack is full, place remaining item on the room table/floor stash
                            AddFloorItem(item.itemId, item.quantity);
                        }
                        gatheredCount += item.quantity;
                    }
                    tableStash.Clear();
                }
            }

            hasPacked = true;
            // Note: Keep room floorItems intact so player doesn't lose anything!

            AppendStoryLog(gameClock.GetTimeString(), "APARTMENT 407 // GATHERED ESSENTIALS",
                $"I picked up my apartment keys, wallet, phone, and belongings off the coffee table. Outside, sirens echo through the pitch-black streets.");
            if (situationBoxText != null)
            {
                situationBoxText.text = $"\"<color=#10B981>✓ Pocketed my apartment keys and gathered my things off the table ({gatheredCount} items).</color> The apartment door is now ready to be unlocked. I can check the rest of the apartment first if there's anything else I need.\"";
            }

            if (packedSuccessBanner != null)
            {
                packedSuccessBanner.SetActive(true);
            }
            if (packedSuccessText != null)
            {
                packedSuccessText.text = $"<color=#10B981>✓ Gathered my apartment keys, wallet, and gear ({gatheredCount} items).\n<color=#38BDF8>Front door can now be unlocked. Check the kitchen, bathroom, or closet before stepping out.</color></color>";
            }

            // Objectives
            objectiveSystem.CompleteObjective("pack_gear");
            objectiveSystem.ActivateObjective("leave_apt");

            ShowHubMenu();
            PopulateTableStashUI();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchKitchenClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || kitchenSearched) return;

            kitchenSearched = true;
            gameClock.AddTime(3); // +3 minutes

            // Scavenged items go into floorItems (Room stash) so the player can choose what to bring
            AddFloorItem("Bottled Water", 2);
            AddFloorItem("Canned Tuna", 1);
            AddFloorItem("Can Opener", 1);
            AddFloorItem("Stale Coffee", 1);

            AppendStoryLog(gameClock.GetTimeString(), "KITCHEN // SUPPLIES FOUND",
                "I opened the cabinets and fridge in the dark. Placed two bottles of water, canned tuna, a can opener, and stale coffee on the counter. When I turned the tap, only brown silt sputtered out.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#F59E0B>✓ Kitchen checked.</color> Found bottled water (x2), canned tuna, a can opener, and coffee. Placed on counter. Open <color=#F59E0B>BACKPACK & ITEMS [I]</color> to choose what to bring.\"";
            }

            UpdateFloorItemsNotification();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchBathroomClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || bathroomSearched) return;

            bathroomSearched = true;
            gameClock.AddTime(2); // +2 minutes

            // Scavenged items go into floorItems (Room stash) so the player can choose what to bring
            AddFloorItem("Painkillers", 1);
            AddFloorItem("Bandage", 2);
            AddFloorItem("Rubbing Alcohol", 1);

            AppendStoryLog(gameClock.GetTimeString(), "BATHROOM // MEDICINE CABINET",
                "I felt my way into the dark bathroom and opened the medicine cabinet. Placed painkillers, bandages, and rubbing alcohol on the vanity counter.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#10B981>✓ Medicine cabinet checked.</color> Found painkillers, bandages (x2), and rubbing alcohol. Placed on vanity. Open <color=#10B981>BACKPACK & ITEMS [I]</color> to choose what to bring.\"";
            }

            UpdateFloorItemsNotification();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchClosetClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || closetSearched) return;

            closetSearched = true;
            gameClock.AddTime(2); // +2 minutes

            // Scavenged items go into floorItems (Room stash) so the player can choose what to bring
            AddFloorItem("Rain Poncho", 1);
            AddFloorItem("Duct Tape", 1);
            AddFloorItem("Matches", 1);

            AppendStoryLog(gameClock.GetTimeString(), "FRONT CLOSET // COAT & SUPPLIES",
                "I reached onto the top shelf of the coat closet and set down my heavy rain poncho, duct tape, and a box of matches on the hall bench.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#38BDF8>✓ Front closet checked.</color> Found my heavy rain poncho, duct tape, and matches. Placed on hall bench. Open <color=#38BDF8>BACKPACK & ITEMS [I]</color> to choose what to bring.\"";
            }

            UpdateFloorItemsNotification();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnTakeAllFloorItemsClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || floorItems.Count == 0) return;

            int taken = 0;
            for (int i = floorItems.Count - 1; i >= 0; i--)
            {
                var item = floorItems[i];
                if (currentPlayer.AddItem(item.itemId, item.quantity))
                {
                    floorItems.RemoveAt(i);
                    taken++;
                }
                else
                {
                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#EF4444>[!] Backpack capacity reached! Make room to pack remaining supplies.</color>";
                    }
                    break;
                }
            }

            if (taken > 0)
            {
                AppendStoryLog(gameClock.GetTimeString(), "LOOT // ROOM ITEMS SECURED",
                    $"Packed {taken} item(s) from the room into backpack.");
            }

            UpdateFloorItemsNotification();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnTakeSingleFloorItemClicked(string itemId)
        {
            PlayClickSFX();
            if (currentPlayer == null) return;

            int idx = floorItems.FindIndex(i => i.itemId == itemId);
            if (idx >= 0)
            {
                var item = floorItems[idx];
                if (currentPlayer.AddItem(item.itemId, item.quantity))
                {
                    floorItems.RemoveAt(idx);

                    AppendStoryLog(gameClock.GetTimeString(), "LOOT // ITEM SECURED",
                        $"You packed {item.itemId} (x{item.quantity}) into your backpack.");
                }
                else
                {
                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#EF4444>[!] Backpack capacity reached! Make room to carry this item.</color>";
                    }
                }

                UpdateFloorItemsNotification();
                RefreshRightGearPanel();
                RefreshAllPDAPanels();
                EvaluateHubButtonStates();
            }
        }

        private void OnCheckPhoneClicked()
        {
            PlayClickSFX();

            // Advance time +1 minute
            gameClock.AddTime(1);
            phoneVisited = true;

            if (phoneModal != null)
            {
                phoneModal.SetActive(true);
            }
        }

        private void OnClosePhoneClicked()
        {
            PlayClickSFX();

            if (phoneModal != null)
            {
                phoneModal.SetActive(false);
            }

            // Record Lore Entry
            loreSystem.AddLore(
                "X-Feed Timeline (Day 0)",
                "CDC reported an unprecedented neuro-aggressive pathogen causing extreme psychosis. Violence escalating. Videos show infected individuals ignoring fatal chest wounds. The military barricaded the bridges before the cellular network collapsed.",
                gameClock.GetTimeString()
            );

            AppendStoryLog(gameClock.GetTimeString(), "NEWS // SOCIAL MEDIA TIMELINE",
                "City news feeds report police in riot gear and gas masks sealing 5th St Hospital. A cell phone video shows an infected person continuing to sprint despite multiple gunshot wounds before the network went dark. Cellular service is now completely dead.");

            EvaluateHubButtonStates();
        }

        #region System Menu Modal (Multi-Slot Save / Multi-Slot Load / Settings / Exit Confirmation)
        private enum SystemMenuView { Main, SaveSlots, LoadSlots, Settings, ExitConfirm }

        private GameObject systemMenuModal;
        private GameObject sysMainPanel;
        private GameObject sysSlotsPanel;
        private GameObject sysSettingsPanel;
        private GameObject sysExitConfirmPanel;

        private bool isSlotPickerSaveMode = true;
        private TextMeshProUGUI[] sysSlotTitles = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private TextMeshProUGUI[] sysSlotDetails = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private Button[] sysSlotActionBtns = new Button[SaveManager.TotalSlotCount];
        private TextMeshProUGUI[] sysSlotActionLabels = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private Button[] sysSlotDeleteBtns = new Button[SaveManager.TotalSlotCount];
        private Image[] sysSlotCardBgs = new Image[SaveManager.TotalSlotCount];
        private Outline[] sysSlotCardOutlines = new Outline[SaveManager.TotalSlotCount];

        private TextMeshProUGUI sysHeaderSubText;
        private TextMeshProUGUI sysTelemetryStatusText;
        private TextMeshProUGUI sysSaveBtnLabel;
        private Button sysLoadGameBtn;
        private TextMeshProUGUI sysLoadBtnLabel;
        private TextMeshProUGUI sysExitWarningDetailText;

        private Slider sysMasterVolSlider;
        private Slider sysBgmVolSlider;
        private Slider sysSfxVolSlider;
        private TextMeshProUGUI sysMasterVolValueText;
        private TextMeshProUGUI sysBgmVolValueText;
        private TextMeshProUGUI sysSfxVolValueText;

        private Coroutine sysSaveFeedbackRoutine;

        private void OnPDASaveClicked()
        {
            PlayClickSFX();
            OpenSystemMenuModal();
        }

        public void OpenSystemMenuModal()
        {
            EnsureSystemMenuModalBuilt();
            RefreshSystemMenuTelemetry();
            SwitchSystemMenuView(SystemMenuView.Main);
            systemMenuModal.transform.SetAsLastSibling();
            systemMenuModal.SetActive(true);
        }

        public void CloseSystemMenuModal()
        {
            PlayClickSFX();
            if (systemMenuModal != null)
            {
                systemMenuModal.SetActive(false);
            }
        }

        private void SwitchSystemMenuView(SystemMenuView view)
        {
            PlayClickSFX();
            if (sysMainPanel != null) sysMainPanel.SetActive(view == SystemMenuView.Main);
            if (sysSlotsPanel != null) sysSlotsPanel.SetActive(view == SystemMenuView.SaveSlots || view == SystemMenuView.LoadSlots);
            if (sysSettingsPanel != null) sysSettingsPanel.SetActive(view == SystemMenuView.Settings);
            if (sysExitConfirmPanel != null) sysExitConfirmPanel.SetActive(view == SystemMenuView.ExitConfirm);

            if (sysHeaderSubText != null)
            {
                switch (view)
                {
                    case SystemMenuView.Main:
                        sysHeaderSubText.text = "SYSTEM // ARCHIVE, AUDIO CONFIGURATION & SESSION CONTROL";
                        RefreshSystemMenuTelemetry();
                        break;
                    case SystemMenuView.SaveSlots:
                        isSlotPickerSaveMode = true;
                        sysHeaderSubText.text = "SAVE GAME // SELECT ARCHIVE SLOT TO RECORD PROGRESS";
                        RefreshSystemSlotCards();
                        break;
                    case SystemMenuView.LoadSlots:
                        isSlotPickerSaveMode = false;
                        sysHeaderSubText.text = "LOAD GAME // SELECT ARCHIVED SLOT TO RESTORE";
                        RefreshSystemSlotCards();
                        break;
                    case SystemMenuView.Settings:
                        sysHeaderSubText.text = "SETTINGS // AUDIO MIXER & VOLUME CALIBRATION";
                        SyncSettingsSlidersFromAudio();
                        break;
                    case SystemMenuView.ExitConfirm:
                        sysHeaderSubText.text = "WARNING // CONFIRM SESSION TERMINATION";
                        UpdateExitWarningDetails();
                        break;
                }
            }
        }

        private void RefreshSystemSlotCards()
        {
            for (int i = 0; i < SaveManager.TotalSlotCount; i++)
            {
                int slotIdx = i;
                var s = SaveManager.Instance != null ? SaveManager.Instance.GetSlotSummary(slotIdx) : default;
                string slotTitle = SaveManager.Instance != null ? SaveManager.Instance.GetSlotTitle(slotIdx) : $"SLOT {slotIdx}";

                if (s.exists)
                {
                    string savedBadge = !string.IsNullOrEmpty(s.realSaveTime)
                        ? $"<color=#64748B>[{s.realSaveTime}]</color>"
                        : "";

                    if (sysSlotTitles[slotIdx] != null)
                    {
                        string titleHex = slotIdx == 0 ? "#38BDF8" : "#F59E0B";
                        sysSlotTitles[slotIdx].text =
                            $"<color={titleHex}><b>■ {slotTitle}</b></color>   <color=#F8FAFC><b>{s.characterName}</b></color> <color=#94A3B8>({s.job})</color>  {savedBadge}";
                    }

                    if (sysSlotDetails[slotIdx] != null)
                    {
                        sysSlotDetails[slotIdx].text =
                            $"<color=#FBBF24>{s.roomDisplayName}</color>  •  <color=#E2E8F0>{s.clockDisplay}</color>  •  " +
                            $"HP <color=#EF4444>{s.hp}/{s.hpMax}</color>  STM <color=#10B981>{s.stamina}/{s.staminaMax}</color>  HUM <color=#A78BFA>{s.humanity}%</color>";
                    }

                    if (sysSlotActionBtns[slotIdx] != null)
                    {
                        sysSlotActionBtns[slotIdx].interactable = true;
                        Image btnBg = sysSlotActionBtns[slotIdx].GetComponent<Image>();
                        if (btnBg != null)
                        {
                            btnBg.color = isSlotPickerSaveMode
                                ? new Color(0.24f, 0.16f, 0.04f, 0.98f)
                                : new Color(0.07f, 0.25f, 0.36f, 0.98f);
                        }
                    }

                    if (sysSlotActionLabels[slotIdx] != null)
                    {
                        sysSlotActionLabels[slotIdx].text = isSlotPickerSaveMode ? "<b>OVERWRITE</b>" : "<b>► LOAD</b>";
                        sysSlotActionLabels[slotIdx].color = isSlotPickerSaveMode
                            ? new Color(0.98f, 0.78f, 0.28f)
                            : new Color(0.38f, 0.85f, 0.98f);
                    }

                    if (sysSlotDeleteBtns[slotIdx] != null)
                    {
                        sysSlotDeleteBtns[slotIdx].gameObject.SetActive(true);
                    }

                    if (sysSlotCardBgs[slotIdx] != null)
                    {
                        sysSlotCardBgs[slotIdx].color = new Color(0.07f, 0.10f, 0.15f, 0.96f);
                    }
                    if (sysSlotCardOutlines[slotIdx] != null)
                    {
                        sysSlotCardOutlines[slotIdx].effectColor = slotIdx == 0
                            ? new Color(0.22f, 0.74f, 0.97f, 0.65f)
                            : new Color(0.95f, 0.62f, 0.12f, 0.65f);
                    }
                }
                else
                {
                    if (sysSlotTitles[slotIdx] != null)
                    {
                        sysSlotTitles[slotIdx].text = $"<color=#64748B><b>□ {slotTitle}</b>   —   [ EMPTY ARCHIVE SLOT ]</color>";
                    }

                    if (sysSlotDetails[slotIdx] != null)
                    {
                        sysSlotDetails[slotIdx].text = isSlotPickerSaveMode
                            ? "<color=#64748B><i>Available slot — click SAVE HERE to record current progress.</i></color>"
                            : "<color=#475569><i>No survivor data recorded in this slot.</i></color>";
                    }

                    if (sysSlotActionBtns[slotIdx] != null)
                    {
                        sysSlotActionBtns[slotIdx].interactable = isSlotPickerSaveMode;
                        Image btnBg = sysSlotActionBtns[slotIdx].GetComponent<Image>();
                        if (btnBg != null)
                        {
                            btnBg.color = isSlotPickerSaveMode
                                ? new Color(0.06f, 0.24f, 0.14f, 0.98f)
                                : new Color(0.06f, 0.08f, 0.11f, 0.75f);
                        }
                    }

                    if (sysSlotActionLabels[slotIdx] != null)
                    {
                        sysSlotActionLabels[slotIdx].text = isSlotPickerSaveMode ? "<b>SAVE HERE</b>" : "EMPTY";
                        sysSlotActionLabels[slotIdx].color = isSlotPickerSaveMode
                            ? new Color(0.28f, 0.95f, 0.58f)
                            : new Color(0.35f, 0.40f, 0.46f);
                    }

                    if (sysSlotDeleteBtns[slotIdx] != null)
                    {
                        sysSlotDeleteBtns[slotIdx].gameObject.SetActive(false);
                    }

                    if (sysSlotCardBgs[slotIdx] != null)
                    {
                        sysSlotCardBgs[slotIdx].color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
                    }
                    if (sysSlotCardOutlines[slotIdx] != null)
                    {
                        sysSlotCardOutlines[slotIdx].effectColor = new Color(0.18f, 0.22f, 0.28f, 0.5f);
                    }
                }
            }
        }

        private void OnSystemSlotActionClicked(int slotIndex)
        {
            PlayClickSFX();
            if (SaveManager.Instance == null) return;

            if (isSlotPickerSaveMode)
            {
                bool saved = SaveManager.Instance.SaveToSlot(slotIndex);
                if (saved)
                {
                    string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);
                    AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", $"MANUAL ARCHIVE // {slotTitle}",
                        $"Survivor vitals, backpack items, and room progress saved to {slotTitle}.");

                    var s = SaveManager.Instance.GetSlotSummary(slotIndex);
                    RefreshSystemSlotCards();
                    RefreshSystemMenuTelemetry(
                        $"<color=#10B981><b>✓ SAVED TO {slotTitle}!</b></color>  [{s.saveTimestamp}]  —  {s.characterName} | {s.roomDisplayName}");
                }
            }
            else
            {
                if (!SaveManager.Instance.HasSaveInSlot(slotIndex)) return;
                bool loaded = SaveManager.Instance.LoadFromSlot(slotIndex);
                if (loaded)
                {
                    string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);
                    RefreshSystemMenuTelemetry($"<color=#38BDF8><b>✓ LOADED {slotTitle} SUCCESSFULLY</b></color>");
                    CloseSystemMenuModal();
                }
            }
        }

        private void OnSystemSlotDeleteClicked(int slotIndex)
        {
            PlayClickSFX();
            if (SaveManager.Instance == null) return;

            string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);
            SaveManager.Instance.DeleteSlot(slotIndex);
            RefreshSystemSlotCards();
            RefreshSystemMenuTelemetry($"<color=#F59E0B>Deleted archive data in {slotTitle}.</color>");
        }

        private void RefreshSystemMenuTelemetry(string customBanner = null)
        {
            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();

            if (sysLoadGameBtn != null)
            {
                sysLoadGameBtn.interactable = hasSave;
            }

            if (sysLoadBtnLabel != null)
            {
                sysLoadBtnLabel.text = hasSave
                    ? "<b>[ 2 ]  LOAD GAME</b>   <size=11><color=#94A3B8>Choose from 4 archive slots to restore</color></size>"
                    : "<b>[ 2 ]  LOAD GAME</b>   <size=11><color=#64748B>[ALL ARCHIVE SLOTS EMPTY]</color></size>";
            }

            if (sysTelemetryStatusText != null)
            {
                if (!string.IsNullOrEmpty(customBanner))
                {
                    sysTelemetryStatusText.text = customBanner;
                }
                else if (hasSave && SaveManager.Instance != null)
                {
                    var s = SaveManager.Instance.GetSaveSummary();
                    sysTelemetryStatusText.text =
                        $"<color=#38BDF8><b>LATEST ({s.slotTitle}):</b></color> {s.saveTimestamp}  |  <color=#F8FAFC>{s.characterName}</color>  |  <color=#F59E0B>{s.roomDisplayName}</color>";
                }
                else
                {
                    sysTelemetryStatusText.text = "<color=#EF4444><b>ARCHIVE STATUS:</b> All 4 save slots are currently empty.</color>";
                }
            }
        }

        private void UpdateExitWarningDetails()
        {
            if (sysExitWarningDetailText == null) return;

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();
            if (hasSave && SaveManager.Instance != null)
            {
                var s = SaveManager.Instance.GetSaveSummary();
                sysExitWarningDetailText.text =
                    "Are you sure you want to exit the game?\n" +
                    "Any unsaved progress since your last save will be lost.\n\n" +
                    $"<color=#38BDF8><b>Latest Save ({s.slotTitle}):</b></color> {s.saveTimestamp}\n" +
                    $"<color=#94A3B8>Citizen: {s.characterName}  |  Location: {s.roomDisplayName} ({s.clockDisplay})</color>";
            }
            else
            {
                sysExitWarningDetailText.text =
                    "Are you sure you want to exit the game?\n" +
                    "<color=#EF4444><b>You have not saved your game in any slot yet!</b></color> If you exit without saving, all current progress will be lost.";
            }
        }

        private void OnSystemMenuSaveClicked()
        {
            SwitchSystemMenuView(SystemMenuView.SaveSlots);
        }

        private void OnSystemMenuLoadClicked()
        {
            if (SaveManager.Instance == null || !SaveManager.Instance.HasAnySave()) return;
            SwitchSystemMenuView(SystemMenuView.LoadSlots);
        }

        private void OnSystemMenuSaveAndExitClicked()
        {
            PlayClickSFX();
            if (SaveManager.Instance != null)
            {
                int targetSlot = SaveManager.Instance.GetMostRecentPopulatedSlotIndex();
                if (targetSlot <= 0) targetSlot = 1; // Default to Slot 01 if no save exists yet or if quicksave was recent
                SaveManager.Instance.SaveToSlot(targetSlot);
                SaveManager.Instance.QuickSave(); // Synchronize QuickSave so both buttons on Title Screen restore this exact moment!
            }
            ExecuteExitToMainMenu();
        }

        private void OnSystemMenuExitWithoutSavingClicked()
        {
            PlayClickSFX();
            ExecuteExitToMainMenu();
        }

        private void ExecuteExitToMainMenu()
        {
            if (systemMenuModal != null) systemMenuModal.SetActive(false);
            StopAllAudioSources();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToTitle();
            }
            else
            {
                Application.Quit();
            }
        }

        private void SyncSettingsSlidersFromAudio()
        {
            float master = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : AudioListener.volume;
            float bgm = AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 0.75f;
            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f;

            if (sysMasterVolSlider != null) sysMasterVolSlider.SetValueWithoutNotify(master);
            if (sysBgmVolSlider != null) sysBgmVolSlider.SetValueWithoutNotify(bgm);
            if (sysSfxVolSlider != null) sysSfxVolSlider.SetValueWithoutNotify(sfx);

            UpdateVolumeLabels(master, bgm, sfx);
        }

        private void OnMasterVolumeChanged(float val)
        {
            AudioListener.volume = Mathf.Clamp01(val);
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);
            ApplyLocalAudioVolumes();
            UpdateVolumeLabels(
                val,
                sysBgmVolSlider != null ? sysBgmVolSlider.value : 0.75f,
                sysSfxVolSlider != null ? sysSfxVolSlider.value : 0.85f);
        }

        private void OnBgmVolumeChanged(float val)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(val);
            ApplyLocalAudioVolumes();
            UpdateVolumeLabels(
                sysMasterVolSlider != null ? sysMasterVolSlider.value : 1f,
                val,
                sysSfxVolSlider != null ? sysSfxVolSlider.value : 0.85f);
        }

        private void OnSfxVolumeChanged(float val)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
            ApplyLocalAudioVolumes();
            UpdateVolumeLabels(
                sysMasterVolSlider != null ? sysMasterVolSlider.value : 1f,
                sysBgmVolSlider != null ? sysBgmVolSlider.value : 0.75f,
                val);
        }

        private void ApplyLocalAudioVolumes()
        {
            float master = sysMasterVolSlider != null ? sysMasterVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 1f);
            float bgm = sysBgmVolSlider != null ? sysBgmVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 0.75f);
            float sfx = sysSfxVolSlider != null ? sysSfxVolSlider.value : (AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f);

            if (atmosphereSource != null) atmosphereSource.volume = 0.5f * bgm * master;
            if (ebsBroadcastSource != null) ebsBroadcastSource.volume = 0.65f * sfx * master;
            if (staticSource != null) staticSource.volume = 0.25f * sfx * master;
            if (radioSource != null) radioSource.volume = 0.75f * sfx * master;
            if (sfxSource != null) sfxSource.volume = 0.85f * sfx * master;
        }

        private void UpdateVolumeLabels(float master, float bgm, float sfx)
        {
            if (sysMasterVolValueText != null) sysMasterVolValueText.text = $"{Mathf.RoundToInt(master * 100f)}%";
            if (sysBgmVolValueText != null) sysBgmVolValueText.text = $"{Mathf.RoundToInt(bgm * 100f)}%";
            if (sysSfxVolValueText != null) sysSfxVolValueText.text = $"{Mathf.RoundToInt(sfx * 100f)}%";
        }

        private void EnsureSystemMenuModalBuilt()
        {
            if (systemMenuModal != null) return;

            TMP_FontAsset sharedFont = pdaCharNameText != null ? pdaCharNameText.font : (dockHeaderLocation != null ? dockHeaderLocation.font : null);

            // 1. Fullscreen Backdrop Dimmer
            systemMenuModal = new GameObject("SystemMenuModal");
            systemMenuModal.transform.SetParent(transform, false);
            RectTransform overlayRect = systemMenuModal.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImg = systemMenuModal.AddComponent<Image>();
            overlayImg.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);
            overlayImg.raycastTarget = true;

            // 2. Center Window Card (760 x 530)
            GameObject card = new GameObject("SystemWindowCard");
            card.transform.SetParent(systemMenuModal.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 530f);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.05f, 0.07f, 0.10f, 0.99f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.95f, 0.62f, 0.05f, 0.85f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Top Header Bar
            GameObject headerBar = new GameObject("HeaderBar");
            headerBar.transform.SetParent(card.transform, false);
            RectTransform hbRect = headerBar.AddComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0f, 1f);
            hbRect.anchorMax = new Vector2(1f, 1f);
            hbRect.pivot = new Vector2(0.5f, 1f);
            hbRect.anchoredPosition = Vector2.zero;
            hbRect.sizeDelta = new Vector2(0f, 52f);
            Image hbImg = headerBar.AddComponent<Image>();
            hbImg.color = new Color(0.09f, 0.12f, 0.17f, 1f);

            sysHeaderSubText = CreateSysTMP(headerBar.transform, "HeaderTitle",
                "SYSTEM // ARCHIVE, AUDIO CONFIGURATION & SESSION CONTROL",
                13.5f, FontStyles.Bold, new Color(0.96f, 0.65f, 0.14f), TextAlignmentOptions.MidlineLeft, sharedFont);
            RectTransform hTitleRect = sysHeaderSubText.rectTransform;
            hTitleRect.anchorMin = Vector2.zero;
            hTitleRect.anchorMax = Vector2.one;
            hTitleRect.offsetMin = new Vector2(20f, 0f);
            hTitleRect.offsetMax = new Vector2(-100f, 0f);

            // Close [X] Button in Header
            Button closeTopBtn = CreateSysActionButton(headerBar.transform, "CloseTopBtn", "[ ESC ]",
                new Color(0.16f, 0.19f, 0.24f, 1f), new Color(0.85f, 0.88f, 0.92f), sharedFont, out _);
            RectTransform ctbRect = closeTopBtn.GetComponent<RectTransform>();
            ctbRect.anchorMin = new Vector2(1f, 0.5f);
            ctbRect.anchorMax = new Vector2(1f, 0.5f);
            ctbRect.pivot = new Vector2(1f, 0.5f);
            ctbRect.anchoredPosition = new Vector2(-14f, 0f);
            ctbRect.sizeDelta = new Vector2(72f, 30f);
            closeTopBtn.onClick.AddListener(CloseSystemMenuModal);

            // Telemetry Strip under Header
            GameObject teleStrip = new GameObject("TelemetryStrip");
            teleStrip.transform.SetParent(card.transform, false);
            RectTransform tsRect = teleStrip.AddComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0f, 1f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.pivot = new Vector2(0.5f, 1f);
            tsRect.anchoredPosition = new Vector2(0f, -52f);
            tsRect.sizeDelta = new Vector2(0f, 34f);
            Image tsImg = teleStrip.AddComponent<Image>();
            tsImg.color = new Color(0.03f, 0.05f, 0.08f, 1f);

            sysTelemetryStatusText = CreateSysTMP(teleStrip.transform, "TelemetryText",
                "ARCHIVE STATUS: READY", 11.5f, FontStyles.Normal, new Color(0.82f, 0.86f, 0.92f), TextAlignmentOptions.MidlineLeft, sharedFont);
            RectTransform ttRect = sysTelemetryStatusText.rectTransform;
            ttRect.anchorMin = Vector2.zero;
            ttRect.anchorMax = Vector2.one;
            ttRect.offsetMin = new Vector2(20f, 0f);
            ttRect.offsetMax = new Vector2(-20f, 0f);

            // =========================================================================
            // VIEW 1: MAIN 4-BUTTON SYSTEM MENU (SAVE, LOAD, SETTINGS, EXIT GAME)
            // =========================================================================
            sysMainPanel = CreateSubPanelRoot(card.transform, "MainPanel");

            // 1. SAVE GAME
            Button saveBtn = CreateSysActionButton(sysMainPanel.transform, "BtnSaveGame",
                "<b>[ 1 ]  SAVE GAME</b>   <size=11><color=#94A3B8>Choose from 4 archive slots to save progress</color></size>",
                new Color(0.06f, 0.22f, 0.14f, 0.98f), new Color(0.25f, 0.95f, 0.58f), sharedFont, out sysSaveBtnLabel);
            PositionMenuRow(saveBtn.GetComponent<RectTransform>(), 0);
            saveBtn.onClick.AddListener(OnSystemMenuSaveClicked);

            // 2. LOAD GAME
            sysLoadGameBtn = CreateSysActionButton(sysMainPanel.transform, "BtnLoadGame",
                "<b>[ 2 ]  LOAD GAME</b>   <size=11><color=#94A3B8>Choose from 4 archive slots to restore</color></size>",
                new Color(0.07f, 0.17f, 0.26f, 0.98f), new Color(0.35f, 0.78f, 0.98f), sharedFont, out sysLoadBtnLabel);
            PositionMenuRow(sysLoadGameBtn.GetComponent<RectTransform>(), 1);
            sysLoadGameBtn.onClick.AddListener(OnSystemMenuLoadClicked);

            // 3. SETTINGS (BGM, SFX, Master Volume)
            Button settingsBtn = CreateSysActionButton(sysMainPanel.transform, "BtnSettings",
                "<b>[ 3 ]  SETTINGS</b>   <size=11><color=#94A3B8>Configure Master, BGM & SFX audio levels</color></size>",
                new Color(0.14f, 0.15f, 0.20f, 0.98f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out _);
            PositionMenuRow(settingsBtn.GetComponent<RectTransform>(), 2);
            settingsBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Settings));

            // 4. EXIT GAME (Opens Unsaved Progress Confirmation Window)
            Button exitBtn = CreateSysActionButton(sysMainPanel.transform, "BtnExitGame",
                "<b>[ 4 ]  EXIT GAME</b>   <size=11><color=#FCA5A5>Return to Main Menu / Quit session</color></size>",
                new Color(0.25f, 0.07f, 0.08f, 0.98f), new Color(0.98f, 0.42f, 0.42f), sharedFont, out _);
            PositionMenuRow(exitBtn.GetComponent<RectTransform>(), 3);
            exitBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.ExitConfirm));

            // Bottom Resume Button
            Button resumeBtn = CreateSysActionButton(sysMainPanel.transform, "BtnResume",
                "<b>[ RESUME GAME ]</b>",
                new Color(0.10f, 0.12f, 0.16f, 1f), new Color(0.82f, 0.86f, 0.92f), sharedFont, out _);
            RectTransform resRect = resumeBtn.GetComponent<RectTransform>();
            resRect.anchorMin = new Vector2(0.5f, 0f);
            resRect.anchorMax = new Vector2(0.5f, 0f);
            resRect.pivot = new Vector2(0.5f, 0f);
            resRect.anchoredPosition = new Vector2(0f, 14f);
            resRect.sizeDelta = new Vector2(710f, 40f);
            resumeBtn.onClick.AddListener(CloseSystemMenuModal);

            // =========================================================================
            // VIEW 1B: 4-SLOT SAVE / LOAD PICKER SUB-WINDOW
            // =========================================================================
            sysSlotsPanel = CreateSubPanelRoot(card.transform, "SlotsPickerPanel");

            for (int i = 0; i < SaveManager.TotalSlotCount; i++)
            {
                int slotIdx = i;
                GameObject sCard = new GameObject($"SysSlotCard_{slotIdx}");
                sCard.transform.SetParent(sysSlotsPanel.transform, false);
                RectTransform scRect = sCard.AddComponent<RectTransform>();
                scRect.anchorMin = new Vector2(0.5f, 1f);
                scRect.anchorMax = new Vector2(0.5f, 1f);
                scRect.pivot = new Vector2(0.5f, 1f);
                scRect.anchoredPosition = new Vector2(0f, -8f - (slotIdx * 74f));
                scRect.sizeDelta = new Vector2(710f, 68f);

                sysSlotCardBgs[slotIdx] = sCard.AddComponent<Image>();
                sysSlotCardBgs[slotIdx].color = new Color(0.07f, 0.10f, 0.15f, 0.96f);

                sysSlotCardOutlines[slotIdx] = sCard.AddComponent<Outline>();
                sysSlotCardOutlines[slotIdx].effectColor = new Color(0.25f, 0.32f, 0.42f, 0.65f);

                // Dedicated Text Container with RectMask2D to prevent overlaying into buttons
                GameObject textContainer = new GameObject("TextContainer");
                textContainer.transform.SetParent(sCard.transform, false);
                RectTransform tcRect = textContainer.AddComponent<RectTransform>();
                tcRect.anchorMin = Vector2.zero;
                tcRect.anchorMax = Vector2.one;
                tcRect.offsetMin = new Vector2(16f, 4f);
                tcRect.offsetMax = new Vector2(-188f, -4f);
                textContainer.AddComponent<RectMask2D>();

                sysSlotTitles[slotIdx] = CreateSysTMP(textContainer.transform, "SlotTitle", "SLOT", 13f, FontStyles.Normal, Color.white, TextAlignmentOptions.MidlineLeft, sharedFont);
                sysSlotTitles[slotIdx].overflowMode = TextOverflowModes.Ellipsis;
                RectTransform stRect = sysSlotTitles[slotIdx].rectTransform;
                stRect.anchorMin = new Vector2(0f, 0.5f);
                stRect.anchorMax = new Vector2(1f, 1f);
                stRect.offsetMin = new Vector2(0f, 2f);
                stRect.offsetMax = new Vector2(0f, -2f);

                sysSlotDetails[slotIdx] = CreateSysTMP(textContainer.transform, "SlotDetails", "Details", 11.5f, FontStyles.Normal, new Color(0.78f, 0.82f, 0.88f), TextAlignmentOptions.MidlineLeft, sharedFont);
                sysSlotDetails[slotIdx].overflowMode = TextOverflowModes.Ellipsis;
                RectTransform sdRect = sysSlotDetails[slotIdx].rectTransform;
                sdRect.anchorMin = new Vector2(0f, 0f);
                sdRect.anchorMax = new Vector2(1f, 0.5f);
                sdRect.offsetMin = new Vector2(0f, 2f);
                sdRect.offsetMax = new Vector2(0f, -2f);

                // Action Button (SAVE HERE / OVERWRITE / LOAD)
                Button actBtn = CreateSysActionButton(sCard.transform, "BtnSlotAction", "<b>SAVE HERE</b>",
                    new Color(0.06f, 0.24f, 0.14f, 0.98f), new Color(0.28f, 0.95f, 0.58f), sharedFont, out sysSlotActionLabels[slotIdx]);
                actBtn.transition = Selectable.Transition.None;
                sysSlotActionLabels[slotIdx].alignment = TextAlignmentOptions.Center;
                sysSlotActionLabels[slotIdx].fontSize = 12f;
                sysSlotActionLabels[slotIdx].rectTransform.offsetMin = Vector2.zero;
                sysSlotActionLabels[slotIdx].rectTransform.offsetMax = Vector2.zero;

                RectTransform abRect = actBtn.GetComponent<RectTransform>();
                abRect.anchorMin = new Vector2(1f, 0.5f);
                abRect.anchorMax = new Vector2(1f, 0.5f);
                abRect.pivot = new Vector2(1f, 0.5f);
                abRect.anchoredPosition = new Vector2(-48f, 0f);
                abRect.sizeDelta = new Vector2(120f, 38f);
                actBtn.onClick.AddListener(() => OnSystemSlotActionClicked(slotIdx));
                sysSlotActionBtns[slotIdx] = actBtn;

                // Delete Slot Button [X]
                Button delBtn = CreateSysActionButton(sCard.transform, "BtnSlotDelete", "<b>X</b>",
                    new Color(0.26f, 0.07f, 0.08f, 0.95f), new Color(0.98f, 0.45f, 0.45f), sharedFont, out TextMeshProUGUI delLbl);
                delBtn.transition = Selectable.Transition.None;
                delLbl.alignment = TextAlignmentOptions.Center;
                delLbl.fontSize = 12f;
                delLbl.rectTransform.offsetMin = Vector2.zero;
                delLbl.rectTransform.offsetMax = Vector2.zero;

                RectTransform dbRect = delBtn.GetComponent<RectTransform>();
                dbRect.anchorMin = new Vector2(1f, 0.5f);
                dbRect.anchorMax = new Vector2(1f, 0.5f);
                dbRect.pivot = new Vector2(1f, 0.5f);
                dbRect.anchoredPosition = new Vector2(-10f, 0f);
                dbRect.sizeDelta = new Vector2(32f, 38f);
                delBtn.onClick.AddListener(() => OnSystemSlotDeleteClicked(slotIdx));
                sysSlotDeleteBtns[slotIdx] = delBtn;
            }

            Button slotsBackBtn = CreateSysActionButton(sysSlotsPanel.transform, "BtnSlotsBack",
                "<b>[ < BACK TO SYSTEM MENU ]</b>",
                new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out _);
            RectTransform slbRect = slotsBackBtn.GetComponent<RectTransform>();
            slbRect.anchorMin = new Vector2(0.5f, 0f);
            slbRect.anchorMax = new Vector2(0.5f, 0f);
            slbRect.pivot = new Vector2(0.5f, 0f);
            slbRect.anchoredPosition = new Vector2(0f, 14f);
            slbRect.sizeDelta = new Vector2(710f, 38f);
            slotsBackBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));
            slbRect.anchorMin = new Vector2(0.5f, 0f);
            slbRect.anchorMax = new Vector2(0.5f, 0f);
            slbRect.pivot = new Vector2(0.5f, 0f);
            slbRect.anchoredPosition = new Vector2(0f, 14f);
            slbRect.sizeDelta = new Vector2(592f, 38f);
            slotsBackBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));

            // =========================================================================
            // VIEW 2: SETTINGS SUB-WINDOW (Master, BGM, SFX Sliders)
            // =========================================================================
            sysSettingsPanel = CreateSubPanelRoot(card.transform, "SettingsPanel");

            sysMasterVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "MasterVolumeRow",
                "MASTER VOLUME", "Global output level for all game audio", -18f, sharedFont, out sysMasterVolValueText);
            sysMasterVolSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            sysBgmVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "BgmVolumeRow",
                "BGM & AMBIENCE", "Background music & apartment atmospheric drone", -96f, sharedFont, out sysBgmVolValueText);
            sysBgmVolSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

            sysSfxVolSlider = CreateVolumeSliderRow(sysSettingsPanel.transform, "SfxVolumeRow",
                "SFX & BROADCASTS", "UI clicks, emergency broadcasts, radio & alarms", -174f, sharedFont, out sysSfxVolValueText);
            sysSfxVolSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            Button settingsBackBtn = CreateSysActionButton(sysSettingsPanel.transform, "BtnSettingsBack",
                "<b>[ < BACK TO SYSTEM MENU ]</b>",
                new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.95f, 0.75f, 0.28f), sharedFont, out _);
            RectTransform sbRect = settingsBackBtn.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0.5f, 0f);
            sbRect.anchorMax = new Vector2(0.5f, 0f);
            sbRect.pivot = new Vector2(0.5f, 0f);
            sbRect.anchoredPosition = new Vector2(0f, 16f);
            sbRect.sizeDelta = new Vector2(512f, 42f);
            settingsBackBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));

            // =========================================================================
            // VIEW 3: EXIT GAME CONFIRMATION WINDOW (Unsaved Progress Check)
            // =========================================================================
            sysExitConfirmPanel = CreateSubPanelRoot(card.transform, "ExitConfirmPanel");

            GameObject warnBox = new GameObject("WarningBox");
            warnBox.transform.SetParent(sysExitConfirmPanel.transform, false);
            RectTransform wbRect = warnBox.AddComponent<RectTransform>();
            wbRect.anchorMin = new Vector2(0f, 1f);
            wbRect.anchorMax = new Vector2(1f, 1f);
            wbRect.pivot = new Vector2(0.5f, 1f);
            wbRect.anchoredPosition = new Vector2(0f, -14f);
            wbRect.sizeDelta = new Vector2(-48f, 132f);
            Image wbBg = warnBox.AddComponent<Image>();
            wbBg.color = new Color(0.16f, 0.05f, 0.06f, 0.95f);
            Outline wbOl = warnBox.AddComponent<Outline>();
            wbOl.effectColor = new Color(0.85f, 0.22f, 0.22f, 0.75f);

            TextMeshProUGUI warnHeader = CreateSysTMP(warnBox.transform, "WarnHeader",
                "⚠  UNSAVED PROGRESS WARNING — CONFIRM EXIT",
                13.5f, FontStyles.Bold, new Color(0.98f, 0.42f, 0.42f), TextAlignmentOptions.TopLeft, sharedFont);
            RectTransform whRect = warnHeader.rectTransform;
            whRect.anchorMin = Vector2.zero;
            whRect.anchorMax = Vector2.one;
            whRect.offsetMin = new Vector2(16f, 10f);
            whRect.offsetMax = new Vector2(-16f, -12f);

            sysExitWarningDetailText = CreateSysTMP(warnBox.transform, "WarnDetails",
                "Are you sure you want to exit? Any unsaved progress will be lost.",
                12f, FontStyles.Normal, new Color(0.90f, 0.92f, 0.95f), TextAlignmentOptions.TopLeft, sharedFont);
            sysExitWarningDetailText.textWrappingMode = TextWrappingModes.Normal;
            RectTransform wdRect = sysExitWarningDetailText.rectTransform;
            wdRect.anchorMin = Vector2.zero;
            wdRect.anchorMax = Vector2.one;
            wdRect.offsetMin = new Vector2(16f, 10f);
            wdRect.offsetMax = new Vector2(-16f, -38f);

            // Option A: SAVE & EXIT TO MAIN MENU
            Button saveAndExitBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnSaveAndExit",
                "<b>[ SAVE & EXIT TO MAIN MENU ]</b>   <size=11><color=#A7F3D0>Save progress first, then exit</color></size>",
                new Color(0.06f, 0.24f, 0.15f, 0.98f), new Color(0.28f, 0.95f, 0.60f), sharedFont, out _);
            RectTransform saeRect = saveAndExitBtn.GetComponent<RectTransform>();
            saeRect.anchorMin = new Vector2(0.5f, 1f);
            saeRect.anchorMax = new Vector2(0.5f, 1f);
            saeRect.pivot = new Vector2(0.5f, 1f);
            saeRect.anchoredPosition = new Vector2(0f, -160f);
            saeRect.sizeDelta = new Vector2(512f, 48f);
            saveAndExitBtn.onClick.AddListener(OnSystemMenuSaveAndExitClicked);

            // Option B: EXIT WITHOUT SAVING
            Button exitNoSaveBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnExitWithoutSaving",
                "<b>[ EXIT WITHOUT SAVING ]</b>   <size=11><color=#FECACA>Discard unsaved changes & return to Title</color></size>",
                new Color(0.28f, 0.07f, 0.08f, 0.98f), new Color(0.98f, 0.45f, 0.45f), sharedFont, out _);
            RectTransform ensRect = exitNoSaveBtn.GetComponent<RectTransform>();
            ensRect.anchorMin = new Vector2(0.5f, 1f);
            ensRect.anchorMax = new Vector2(0.5f, 1f);
            ensRect.pivot = new Vector2(0.5f, 1f);
            ensRect.anchoredPosition = new Vector2(0f, -218f);
            ensRect.sizeDelta = new Vector2(512f, 48f);
            exitNoSaveBtn.onClick.AddListener(OnSystemMenuExitWithoutSavingClicked);

            // Option C: CANCEL (Return to System Menu)
            Button cancelExitBtn = CreateSysActionButton(sysExitConfirmPanel.transform, "BtnCancelExit",
                "<b>[ CANCEL — RETURN TO GAME ]</b>",
                new Color(0.12f, 0.15f, 0.20f, 1f), new Color(0.85f, 0.88f, 0.92f), sharedFont, out _);
            RectTransform ceRect = cancelExitBtn.GetComponent<RectTransform>();
            ceRect.anchorMin = new Vector2(0.5f, 0f);
            ceRect.anchorMax = new Vector2(0.5f, 0f);
            ceRect.pivot = new Vector2(0.5f, 0f);
            ceRect.anchoredPosition = new Vector2(0f, 16f);
            ceRect.sizeDelta = new Vector2(512f, 42f);
            cancelExitBtn.onClick.AddListener(() => SwitchSystemMenuView(SystemMenuView.Main));
        }

        private GameObject CreateSubPanelRoot(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0f, 0f);
            rt.offsetMax = new Vector2(0f, -86f);
            return panel;
        }

        private void PositionMenuRow(RectTransform rt, int index)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -18f - (index * 72f));
            rt.sizeDelta = new Vector2(710f, 60f);
        }

        private Button CreateSysActionButton(Transform parent, string name, string labelText, Color bgColor, Color borderAndTextColor, TMP_FontAsset font, out TextMeshProUGUI labelOut)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            btnObj.AddComponent<RectTransform>();

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(borderAndTextColor.r, borderAndTextColor.g, borderAndTextColor.b, 0.65f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            labelOut = CreateSysTMP(btnObj.transform, "Label", labelText, 13.5f, FontStyles.Normal, borderAndTextColor, TextAlignmentOptions.MidlineLeft, font);
            labelOut.raycastTarget = false;
            RectTransform lr = labelOut.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(18f, 0f);
            lr.offsetMax = new Vector2(-18f, 0f);

            return btn;
        }

        private Slider CreateVolumeSliderRow(Transform parent, string name, string title, string subtitle, float topOffset, TMP_FontAsset font, out TextMeshProUGUI pctLabel)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            RectTransform rr = row.AddComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 1f);
            rr.anchorMax = new Vector2(0.5f, 1f);
            rr.pivot = new Vector2(0.5f, 1f);
            rr.anchoredPosition = new Vector2(0f, topOffset);
            rr.sizeDelta = new Vector2(512f, 68f);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);

            Outline rowOl = row.AddComponent<Outline>();
            rowOl.effectColor = new Color(0.22f, 0.28f, 0.36f, 0.7f);

            TextMeshProUGUI titleTmp = CreateSysTMP(row.transform, "Title", $"<b>{title}</b>  <size=10.5><color=#64748B>{subtitle}</color></size>",
                12.5f, FontStyles.Normal, new Color(0.92f, 0.95f, 0.98f), TextAlignmentOptions.MidlineLeft, font);
            RectTransform tr = titleTmp.rectTransform;
            tr.anchorMin = new Vector2(0f, 0.5f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(14f, 2f);
            tr.offsetMax = new Vector2(-70f, -4f);

            pctLabel = CreateSysTMP(row.transform, "PctValue", "100%", 13f, FontStyles.Bold, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.MidlineRight, font);
            RectTransform pr = pctLabel.rectTransform;
            pr.anchorMin = new Vector2(1f, 0.5f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.pivot = new Vector2(1f, 1f);
            pr.offsetMin = new Vector2(-70f, 2f);
            pr.offsetMax = new Vector2(-14f, -4f);

            // Interactive Slider Control
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(row.transform, false);
            RectTransform sr = sliderObj.AddComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(1f, 0.5f);
            sr.offsetMin = new Vector2(14f, 10f);
            sr.offsetMax = new Vector2(-14f, -4f);

            GameObject bgTrack = new GameObject("Background");
            bgTrack.transform.SetParent(sliderObj.transform, false);
            RectTransform bgr = bgTrack.AddComponent<RectTransform>();
            bgr.anchorMin = new Vector2(0f, 0.25f);
            bgr.anchorMax = new Vector2(1f, 0.75f);
            bgr.offsetMin = Vector2.zero;
            bgr.offsetMax = Vector2.zero;
            Image bgImg = bgTrack.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.04f, 0.06f, 1f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform far = fillArea.AddComponent<RectTransform>();
            far.anchorMin = new Vector2(0f, 0.25f);
            far.anchorMax = new Vector2(1f, 0.75f);
            far.offsetMin = Vector2.zero;
            far.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fr = fill.AddComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.95f, 0.62f, 0.08f, 0.95f);

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform har = handleArea.AddComponent<RectTransform>();
            har.anchorMin = Vector2.zero;
            har.anchorMax = Vector2.one;
            har.offsetMin = new Vector2(6f, 0f);
            har.offsetMax = new Vector2(-6f, 0f);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform hr = handle.AddComponent<RectTransform>();
            hr.sizeDelta = new Vector2(14f, 20f);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.98f, 0.98f, 1f, 1f);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fr;
            slider.handleRect = hr;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private TextMeshProUGUI CreateSysTMP(Transform parent, string name, string text, float size, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
        #endregion

        private void OnListenRadioClicked()
        {
            PlayClickSFX();

            bool isPolice = currentPlayer != null && string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
            gameClock.AddTime(isPolice ? 3 : 5);
            radioVisited = true;

            if (radioModal != null)
            {
                radioModal.SetActive(true);
            }

            // Play police radio squelch and audio
            if (radioSource != null && policeRadioClip != null)
            {
                radioSource.clip = policeRadioClip;
                radioSource.volume = 0.9f;
                radioSource.Play();
            }

            PlayScreenShake();
        }

        private void OnCloseRadioClicked()
        {
            PlayClickSFX();

            if (radioSource != null) radioSource.Stop();
            if (radioModal != null) radioModal.SetActive(false);

            // Record Lore Entry
            loreSystem.AddLore(
                "Radio Intercept (Day 0)",
                "Police dispatch authorized unrestricted lethal force. Standard body shots are ineffective against the infected. Neutralization requires destroying the brain. All physical contact must be avoided.",
                gameClock.GetTimeString()
            );

            AppendStoryLog(gameClock.GetTimeString(), "RADIO // EMERGENCY POLICE DISPATCH",
                "Emergency dispatch authorized unrestricted lethal force. Standard body shots are ineffective; neutralization requires destroying the brain. The Harborview medical perimeter has collapsed.");

            EvaluateHubButtonStates();
        }

        private void OnHackTerminalClicked()
        {
            PlayClickSFX();

            gameClock.AddTime(8);
            hackingVisited = true;

            if (hackingModal != null)
            {
                hackingModal.SetActive(true);
            }

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(false);
            }

            // Start matrix green phosphor scramble sequence
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            hackingRoutine = StartCoroutine(HackingTerminalRoutine());
        }

        private IEnumerator HackingTerminalRoutine()
        {
            string[] hackingTextLines = new string[]
            {
                "> BYPASSING 256-BIT AES ENCRYPTION...",
                "> ACCESSING SECURE MILITARY NODE...",
                "> DOWNLOADING CLASSIFIED DIRECTIVE: OPR_WILDFIRE",
                "------------------------------------------------",
                "<color=#ff5555>THREAT LEVEL : CATEGORY 5 (EXTINCTION)</color>",
                "PATHOGEN     : NEURO-AGGRESSIVE MUTAGEN (UNKNOWN ORIGIN)",
                "<color=#ff5555>STATUS       : QUARANTINE FAILED. INFECTION RATE 87%.</color>",
                "<color=#ff3333><b>DIRECTIVE    : INITIATE SECTOR PURGE (FIREBOMBING) AT 0600 HOURS.</b></color>",
                "------------------------------------------------",
                "> DOWNLOADING ATTACHMENT: EVAC_PROTOCOLS.DAT",
                "> EXTRACTION POINT 'ECHO' COORDINATES SECURED.",
                "<color=#ffd700>> GENERATING TIER-1 MILITARY CLEARANCE PASSKEY...</color>",
                "------------------------------------------------",
                "<color=#ff5555>[!] WARNING: TRACE DETECTED. SEVERING CONNECTION.</color>"
            };

            if (hackingTerminalText != null) hackingTerminalText.text = "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hackingTextLines.Length; i++)
            {
                sb.AppendLine(hackingTextLines[i]);
                if (hackingTerminalText != null)
                {
                    hackingTerminalText.text = sb.ToString();
                }
                yield return new WaitForSecondsRealtime(0.25f);
            }

            yield return new WaitForSecondsRealtime(0.4f);

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(true);
            }
        }

        private void OnCloseHackingClicked()
        {
            PlayClickSFX();

            if (hackingModal != null)
            {
                hackingModal.SetActive(false);
            }

            // Award Tier-1 Military Clearance Passkey
            if (currentPlayer != null)
            {
                currentPlayer.AddItem("Tier-1 Military Clearance Passkey", 1);
            }

            loreSystem.AddLore(
                "DoD Intercept: Operation WILDFIRE",
                "The outbreak is uncontained. The military considers the city a lost cause and has ordered a 'Sector Purge' (firebombing) at 0600 hours. I managed to download a Tier-1 Military Clearance Passkey before severing the connection.",
                gameClock.GetTimeString()
            );

            AppendStoryLog(gameClock.GetTimeString(), "TERMINAL // CLASSIFIED PASSKEY",
                "Extracted emergency containment directives. Saved Tier-1 Emergency Access Clearance Passkey directly to my phone storage.");

            EvaluateHubButtonStates();
            RefreshAllPDAPanels();
        }

        private void OnLeaveApartmentClicked()
        {
            bool hasKey = currentPlayer != null && (currentPlayer.HasItem("Apartment Key") || currentPlayer.HasItem("Apartment 407 Key"));
            bool tableEmpty = tableStash == null || tableStash.Count == 0;
            bool canExit = hasPacked && tableEmpty && hasKey;

            if (!canExit)
            {
                PlayBeepSFX();
                if (situationBoxText != null)
                {
                    situationBoxText.text = "\"<color=#EF4444><b>The deadbolt is engaged and locked tight.</b></color> My apartment keys and belongings are still sitting on the coffee table. I can't leave without my keys—I would lock myself out in a dark hallway during a citywide blackout.\"";
                }
                return;
            }

            PlayClickSFX();

            // One-time use: consume the Apartment Key when unlocking the deadbolt and stepping out
            if (currentPlayer != null)
            {
                currentPlayer.MarkKeyItemUsed("Apartment Key", "Unit 407 Front Deadbolt (The Ascott Residency [4th Floor])");
                currentPlayer.MarkKeyItemUsed("Apartment 407 Key", "Unit 407 Front Deadbolt (The Ascott Residency [4th Floor])");
                currentPlayer.RemoveItem("Apartment Key", 1);
                currentPlayer.RemoveItem("Apartment 407 Key", 1);
            }

            AppendStoryLog(gameClock.GetTimeString(), "HALLWAY // LEFT UNIT 407",
                "I turned the brass key in the deadbolt, unlocked the door, and stepped out into the 4th floor hallway. The door clicked shut behind me, locked tight.");

            // Complete leave apartment objective, add escape building
            objectiveSystem.CompleteObjective("leave_apt");
            objectiveSystem.AddObjective("escape_building", "Find a way down to the ground floor", ObjectiveStatus.Active);

            // Fade atmosphere audio
            if (atmosphereSource != null)
            {
                StartCoroutine(FadeOutAudio(atmosphereSource, 1.2f));
            }

            Hide(0.8f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TransitionFromApartmentToHallway();
                }
            });
        }

        private IEnumerator FadeOutAudio(AudioSource src, float duration)
        {
            float startVol = src.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                src.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            src.Stop();
        }
        #endregion

        #region Dynamic Location Management
        /// <summary>
        /// Automatically synchronizes the player's location across:
        /// 1. Top HUD location badge (locationText / LocationBadge)
        /// 2. Bottom Dock location header (dockHeaderLocation)
        /// 3. CharacterData persistent location
        /// 4. Tactical Map controller bio-radar beacon & node selection
        /// </summary>
        public void SetLocation(string roomId, string customSectorSubtitle = null)
        {
            if (string.IsNullOrEmpty(roomId)) roomId = "unit_407";

            string roomDisplay = roomId.ToUpper();
            string sectorSubtitle = customSectorSubtitle;

            if (tacticalMapController != null)
            {
                tacticalMapController.SetCurrentLocation(roomId);
                var node = tacticalMapController.GetNode(roomId);
                if (node != null)
                {
                    roomDisplay = !string.IsNullOrEmpty(node.roomCode) ? node.roomCode : node.id.ToUpper();
                    if (string.IsNullOrEmpty(sectorSubtitle))
                    {
                        sectorSubtitle = !string.IsNullOrEmpty(node.shortSub) ? node.shortSub : node.title;
                    }
                }
            }

            UpdateLocationDisplay(roomDisplay, sectorSubtitle);

            if (currentPlayer != null)
            {
                currentPlayer.currentLocationId = roomId;
                if (locationText != null) currentPlayer.currentLocationName = locationText.text;
            }
        }

        public void UpdateLocationDisplay(string roomTitle, string sectorSubtitle = null)
        {
            string displayTitle = !string.IsNullOrEmpty(roomTitle) ? roomTitle.ToUpper() : "UNIT 407";
            string formattedTop = $"■ {displayTitle}";

            if (!string.IsNullOrEmpty(sectorSubtitle))
            {
                formattedTop += $" // {sectorSubtitle.ToUpper()}";
            }
            else if (!displayTitle.Contains("ASCOTT"))
            {
                formattedTop += " // THE ASCOTT RESIDENCY [4TH FLOOR]";
            }

            if (locationText != null)
            {
                locationText.text = formattedTop;
            }

            if (dockHeaderLocation != null)
            {
                dockHeaderLocation.text = $"<color=#FFA726>[ {displayTitle} ]</color>";
            }
        }
        #endregion

        #region UI Display Helpers
        private void UpdateClockUI()
        {
            if (gameClock == null) return;

            if (clockTimeText != null) clockTimeText.text = gameClock.GetTimeString();
            if (clockDayText != null) clockDayText.text = $"DAY {gameClock.DayCount}";
            if (clockText != null && clockText != clockTimeText)
            {
                clockText.text = $"<color=#FFA726>{gameClock.DayName} NIGHT</color>  |  {gameClock.GetTimeString()}";
            }
        }

        private void UpdateVitalsUI()
        {
            if (currentPlayer == null) return;

            // Twine Style 4 Vital Bars
            if (hpValueText != null) hpValueText.text = currentPlayer.hp.ToString();
            if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.hp / Mathf.Max(1, currentPlayer.hpMax));

            if (staminaValueText != null) staminaValueText.text = currentPlayer.stamina.ToString();
            if (staminaBarFill != null) staminaBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.stamina / Mathf.Max(1, currentPlayer.staminaMax));

            if (hungerValueText != null) hungerValueText.text = currentPlayer.hunger.ToString();
            if (hungerBarFill != null) hungerBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.hunger / 100f);

            if (thirstValueText != null) thirstValueText.text = currentPlayer.thirst.ToString();
            if (thirstBarFill != null) thirstBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.thirst / 100f);

            // Legacy HUD fields
            if (vitalsHpText != null) vitalsHpText.text = $"<color=#FF5555><b>HP</b></color> {currentPlayer.hp}/{currentPlayer.hpMax}";
            if (vitalsStaminaText != null) vitalsStaminaText.text = $"<color=#55FF88><b>STM</b></color> {currentPlayer.stamina}/{currentPlayer.staminaMax}";
        }

        private void UpdateObjectiveUI()
        {
            if (objectiveText != null && objectiveSystem != null)
            {
                objectiveText.text = $"<b>[!] OBJECTIVE:</b> {objectiveSystem.GetActiveObjectiveText()}";
            }
        }

        private void PopulateTableStashUI()
        {
            if (floorItemsCountText != null)
            {
                floorItemsCountText.text = $"■ Floor Items ({tableStash.Count})";
            }

            RefreshRightGearPanel();

            if (tableStashGrid == null) return;

            // Clear old children
            for (int i = tableStashGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(tableStashGrid.GetChild(i).gameObject);
            }

            if (tableStash.Count == 0)
            {
                if (tableEmptyNotice != null) tableEmptyNotice.SetActive(true);
                return;
            }

            if (tableEmptyNotice != null) tableEmptyNotice.SetActive(false);

            foreach (var item in tableStash)
            {
                CreateItemCard(tableStashGrid, item.itemId, item.quantity);
            }
        }

        private void CreateItemCard(Transform parent, string itemId, int quantity)
        {
            GameObject card = new GameObject("ItemCard_" + itemId);
            card.transform.SetParent(parent, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(98f, 96f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 98f;
            le.preferredHeight = 96f;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);
            cardBg.raycastTarget = false;

            Outline cardOl = card.AddComponent<Outline>();
            cardOl.effectColor = new Color(0.25f, 0.35f, 0.45f, 0.6f);
            cardOl.effectDistance = new Vector2(1f, -1f);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.5f, 1f);
            ir.anchorMax = new Vector2(0.5f, 1f);
            ir.pivot = new Vector2(0.5f, 1f);
            ir.anchoredPosition = new Vector2(0f, -6f);
            ir.sizeDelta = new Vector2(46f, 46f);

            Sprite iconSprite = ItemDatabase.GetItemIcon(itemId);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.fontSize = 11.5f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.88f, 0.91f, 0.95f);
            label.text = quantity > 1 ? $"{itemId} (x{quantity})" : itemId;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 0f);
            lr.pivot = new Vector2(0.5f, 0f);
            lr.sizeDelta = new Vector2(-6f, 36f);
            lr.anchoredPosition = new Vector2(0f, 4f);
        }

        #region Story Log System (Option A)
        private readonly List<string> _storyLogs = new List<string>();

        private void InitializeStoryLogs()
        {
            if (_storyLogs.Count > 0) return;

            _storyLogs.Add(
                "<b><color=#EF5350>[23:43] TV BROADCAST // EMERGENCY ALERT SYSTEM (CIVIL DANGER WARNING)</color></b>\n" +
                "\"The following message is transmitted at the request of the King County Sheriff's Office. This is not a test.\n\n" +
                "A Civil Danger Warning is in effect for the Seattle metropolitan area. Authorities reported a rapidly spreading biological hazard. Affected individuals exhibit extreme, unprovoked violence. The pathogen is highly contagious through fluid contact.\n\n" +
                "Shelter in place immediately. Major interstates are closed. Lock all doors, turn off lights, and remain completely silent. The National Guard has been deployed. Lethal force is authorized.\""
            );

            _storyLogs.Add(
                "<b><color=#F5BF42>[23:44] REACTION // INNER MONOLOGUE</color></b>\n" +
                "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"\n\n" +
                "<i>Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door.</i>"
            );

            _storyLogs.Add(
                "<b><color=#4FC3F7>[23:45] APARTMENT 407 // LIVING ROOM</color></b>\n" +
                "You are standing in the middle of your living room in the pitch-black darkness. Sirens wail across the Seattle skyline.\n\n" +
                "Your apartment keys, wallet, phone, and belongings are sitting on the coffee table. The front door deadbolt is locked—you cannot leave without your keys. Before heading into the hallway, you need your keys and whatever you can grab from around the apartment."
            );

            RefreshStoryLogUI();
        }

        public void AppendStoryLog(string timestamp, string header, string content)
        {
            InitializeStoryLogs();
            string entry = $"<b><color=#66BB6A>[{timestamp}] {header}</color></b>\n{content}";
            _storyLogs.Add(entry);
            RefreshStoryLogUI();
        }

        private void RefreshStoryLogUI()
        {
            if (storyLogContentText != null)
            {
                storyLogContentText.text = string.Join("\n\n<color=#2C3E50>──────────────────────────────────────────────────────────────────────────</color>\n\n", _storyLogs);
            }
        }

        public void OnOpenStoryLogClicked()
        {
            PlayClickSFX();
            InitializeStoryLogs();
            if (storyLogModal != null) storyLogModal.SetActive(true);
        }

        public void OnCloseStoryLogClicked()
        {
            PlayClickSFX();
            if (storyLogModal != null) storyLogModal.SetActive(false);
        }
        #endregion

        private void PlayClickSFX()
        {
            if (sfxSource != null && clickClip != null)
            {
                sfxSource.PlayOneShot(clickClip, 0.4f);
            }
        }

        private void PlayBeepSFX()
        {
            AudioManager.Instance?.PlayButtonHover();
        }
        #endregion
    }
}
