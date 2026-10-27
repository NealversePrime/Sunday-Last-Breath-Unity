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

        [Header("Door Exit Transition Settings & Audio")]
        [SerializeField] private bool doorTransitionFadeAmbience = true;
        [SerializeField] private bool doorTransitionPlayKeySFX = true;
        [SerializeField] private AudioClip doorTransitionKeySFX;
        [SerializeField] private bool doorTransitionPlayKnobSFX = true;
        [SerializeField] private AudioClip doorTransitionKnobSFX;
        [SerializeField] private bool doorTransitionPlayFootstepSFX = true;
        [SerializeField] private AudioClip doorTransitionFootstepSFX;
        [SerializeField] private bool doorTransitionPlaySlamSFX = true;
        [SerializeField] private AudioClip doorTransitionSlamSFX;

        [Header("Door Exit Transition Timing (Seconds)")]
        [Tooltip("Duration of the smooth fade-to-black when clicking Leave Apartment.")]
        [SerializeField] private float doorTransitionFadeOutDuration = 0.6f;
        [Tooltip("How long the key-unlocking text is displayed on screen before the door video starts playing.")]
        [SerializeField] private float doorTransitionTextToVideoDelay = 1.5f;
        [Tooltip("Pause duration after the video finishes before fading in to reveal the hallway.")]
        [SerializeField] private float doorTransitionPostVideoPause = 0.4f;
        [Tooltip("Duration of the smooth fade-in from black revealing the hallway.")]
        [SerializeField] private float doorTransitionFadeInDuration = 1.0f;

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
        private readonly Dictionary<string, List<InventoryEntry>> roomFloorStashes = new Dictionary<string, List<InventoryEntry>>(StringComparer.OrdinalIgnoreCase);

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
            get => roomFloorStashes.TryGetValue("unit_407", out var list) ? list : floorItems;
            set
            {
                floorItems = value ?? new List<InventoryEntry>();
                roomFloorStashes["unit_407"] = floorItems;
            }
        }

        public Dictionary<string, List<InventoryEntry>> RoomFloorStashes
        {
            get => roomFloorStashes;
            set
            {
                if (value != null)
                {
                    roomFloorStashes.Clear();
                    foreach (var kvp in value) roomFloorStashes[kvp.Key] = kvp.Value;
                    if (roomFloorStashes.TryGetValue("unit_407", out var u407))
                    {
                        floorItems = u407;
                    }
                }
            }
        }

        public string GetCurrentLocationId()
        {
            if (!isInHallway) return "unit_407";
            if (!string.IsNullOrEmpty(currentInspectedRoom))
            {
                if (currentInspectedRoom.StartsWith("unit_")) return currentInspectedRoom;
                if (currentInspectedRoom == "utility" || currentInspectedRoom == "stairs") return currentInspectedRoom;
                return "unit_" + currentInspectedRoom;
            }
            return $"hallway_{hallwaySection}";
        }

        public List<InventoryEntry> GetCurrentFloorStash()
        {
            string loc = GetCurrentLocationId();
            if (!roomFloorStashes.TryGetValue(loc, out var stash))
            {
                stash = new List<InventoryEntry>();
                roomFloorStashes[loc] = stash;
            }
            return stash;
        }

        public bool HasPacked
        {
            get => hasPacked;
            set => hasPacked = value;
        }

        public bool IntroSeen
        {
            get => introSeen || hasPacked || awakeningStep > 3 || (hubMenuContainer != null && hubMenuContainer.activeSelf);
            set => introSeen = value;
        }

        public int AwakeningStep
        {
            get => IntroSeen ? 4 : Mathf.Clamp(awakeningStep, 1, 3);
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

        public bool PhoneVisited
        {
            get => phoneVisited;
            set => phoneVisited = value;
        }

        public bool RadioVisited
        {
            get => radioVisited;
            set => radioVisited = value;
        }

        public bool HackingVisited
        {
            get => hackingVisited;
            set => hackingVisited = value;
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
        private Coroutine hackingLedRoutine;
        private bool isHackingTerminal = false;
        private TextMeshProUGUI hackingLedText;
        private Coroutine radioRoutine;
        private Coroutine radioMeterRoutine;
        private bool isRadioTransmitting = false;
        private TextMeshProUGUI radioFreqHeaderText;
        private TextMeshProUGUI radioSignalMeterText;
        private TextMeshProUGUI radioTranscriptText;
        private GameObject radioOutroObj;
        private TextMeshProUGUI activeTypewriterTarget;
        private string activeFullText = "";

        #region Hallway & Exploration State
        [Header("Hallway & Room Exploration State")]
        private bool isInHallway = false;
        private int hallwaySection = 1; // 1: Upper, 2: Mid, 3: Lower, 4: End
        private string currentInspectedRoom = null;

        // Room 408 (Harmon)
        private bool room408_opened = false;
        private int failed_408_attempts = 0;
        private bool room408_locked_out = false;
        private bool studied_408_mechanics = false;
        private bool studied_408_anatomy = false;
        private bool read_408_journal = false;
        private bool looted_408_kitchen = false;
        private bool looted_408 = false;

        // Room 405 (Marcus)
        private bool room405_opened = false;
        private bool room405_trap_triggered = false;
        private bool read_marcus_log = false;
        private bool looted_405_bench = false;
        private bool looted_405_safe = false;

        // Room 402 (The Andersons)
        private bool room402_opened = false;
        private string room402_entry_method = "silent";
        private bool looted_402_weapon = false;
        private bool looted_402_kitchen = false;
        private bool looted_402_bathroom = false;
        private bool looted_402_ring = false;
        private bool took_402_photo = false;
        private bool read_402_note = false;

        // Room 401 (Party Flat)
        private bool shelf_lifted = false;
        private bool turned_off_music = false;
        private bool looted_401_kitchen = false;
        private bool looted_401_bathroom = false;
        private bool looted_401_alcohol = false;
        private bool looted_401_pockets = false;

        // Utility Closet
        private bool utility_opened = false;
        private bool read_utility_log = false;
        private bool looted_utility_bleach = false;
        private bool looted_utility_tape = false;
        private bool looted_utility_flashlight = false;

        // Stairwell
        private int stair_phase = 1;
        private string descentMethod = "slow";
        private bool barricade_pushed = false;

        private readonly List<TacticalChoiceButton> hallwayChoiceButtons = new List<TacticalChoiceButton>();
        #endregion

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
        public bool IsInHallway => isInHallway;
        public int HallwaySection => hallwaySection;
        public string CurrentInspectedRoom => currentInspectedRoom;

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

            if (doorVideoRenderTexture != null)
            {
                doorVideoRenderTexture.Release();
                Destroy(doorVideoRenderTexture);
                doorVideoRenderTexture = null;
            }
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

        public void EnsureSystemsInitialized()
        {
            if (gameClock == null)
            {
                gameClock = new GameClock(23, 45, 25, "OCT", 2025, "SATURDAY");
                gameClock.OnTimeAdvanced += (c) => UpdateClockUI();
            }

            if (objectiveSystem == null)
            {
                objectiveSystem = new ObjectiveSystem();
                objectiveSystem.OnObjectiveUpdated += (o) =>
                {
                    UpdateObjectiveUI();
                    RefreshPDAStatusTab();
                };
            }

            if (loreSystem == null)
            {
                loreSystem = new LoreSystem();
                loreSystem.OnLoreAdded += (l) => RefreshPDALoreTab();
            }
        }

        private void InitializeSystems()
        {
            EnsureSystemsInitialized();
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

            // 9. Ensure Phone, Radio, and Hacking modals have full-width, prominent exit controls
            EnsureInspectionModalsEnhanced();
        }

        private void EnsureInspectionModalsEnhanced()
        {
            // A. PHONE MODAL ENHANCEMENT (Split Presentation: Oblong 19.5:9 Smartphone on Right, Narrative Dossier on Left)
            if (phoneModal != null)
            {
                Image backdrop = phoneModal.GetComponent<Image>();
                if (backdrop != null)
                {
                    // Solid dark opaque scrim - zero background text bleed-through
                    backdrop.color = new Color(0.015f, 0.02f, 0.035f, 0.985f);
                }

                Transform centerCol = phoneModal.transform.Find("PhoneCenterColumn");
                if (centerCol != null)
                {
                    RectTransform colRect = centerCol.GetComponent<RectTransform>();
                    if (colRect != null)
                    {
                        colRect.anchorMin = new Vector2(0.5f, 0.5f);
                        colRect.anchorMax = new Vector2(0.5f, 0.5f);
                        colRect.pivot = new Vector2(0.5f, 0.5f);
                        colRect.sizeDelta = new Vector2(920f, 820f);
                        colRect.anchoredPosition = Vector2.zero;
                    }

                    // Remove legacy single-column vertical layout if present
                    VerticalLayoutGroup legacyVlg = centerCol.GetComponent<VerticalLayoutGroup>();
                    if (legacyVlg != null) Destroy(legacyVlg);

                    // 1. LEFT WING: NARRATIVE DOSSIER PANEL (Holds Scene Context, Consequence Prose & Single Action Button)
                    Transform dossierTr = centerCol.Find("NarrativeDossier");
                    if (dossierTr == null)
                    {
                        GameObject dObj = new GameObject("NarrativeDossier", typeof(RectTransform));
                        dObj.transform.SetParent(centerCol, false);
                        dossierTr = dObj.transform;
                    }

                    RectTransform dRect = dossierTr.GetComponent<RectTransform>();
                    dRect.anchorMin = new Vector2(0f, 0.5f);
                    dRect.anchorMax = new Vector2(0f, 0.5f);
                    dRect.pivot = new Vector2(0f, 0.5f);
                    dRect.sizeDelta = new Vector2(480f, 800f);
                    dRect.anchoredPosition = new Vector2(0f, 0f);

                    Image dBg = dossierTr.GetComponent<Image>();
                    if (dBg == null) dBg = dossierTr.gameObject.AddComponent<Image>();
                    dBg.color = new Color(0.035f, 0.048f, 0.068f, 0.98f);

                    Outline dOl = dossierTr.GetComponent<Outline>();
                    if (dOl == null) dOl = dossierTr.gameObject.AddComponent<Outline>();
                    dOl.effectColor = new Color(0.22f, 0.28f, 0.38f, 0.85f);
                    dOl.effectDistance = new Vector2(1f, -1f);

                    VerticalLayoutGroup dVlg = dossierTr.GetComponent<VerticalLayoutGroup>();
                    if (dVlg == null) dVlg = dossierTr.gameObject.AddComponent<VerticalLayoutGroup>();
                    dVlg.padding = new RectOffset(26, 26, 24, 24);
                    dVlg.spacing = 14f;
                    dVlg.childAlignment = TextAnchor.UpperLeft;
                    dVlg.childControlWidth = true;
                    dVlg.childForceExpandWidth = true;
                    dVlg.childControlHeight = false;
                    dVlg.childForceExpandHeight = false;

                    // Header Label in Dossier
                    Transform headerTr = dossierTr.Find("DossierHeader");
                    if (headerTr == null)
                    {
                        GameObject hObj = new GameObject("DossierHeader", typeof(RectTransform));
                        hObj.transform.SetParent(dossierTr, false);
                        hObj.transform.SetSiblingIndex(0);
                        TextMeshProUGUI hTmp = hObj.AddComponent<TextMeshProUGUI>();
                        hTmp.text = "// RECON INTEL // PERSONAL SMARTPHONE (UNIT 407)";
                        hTmp.fontSize = 11.5f;
                        hTmp.fontStyle = FontStyles.Bold;
                        hTmp.color = new Color(0.96f, 0.65f, 0.18f, 0.95f); // Amber
                        SundayTypography.ApplyFont(hTmp, FontRole.Mono);
                    }

                    // Reparent IntroProse into NarrativeDossier
                    Transform introObj = centerCol.Find("IntroProse");
                    if (introObj == null) introObj = dossierTr.Find("IntroProse");
                    if (introObj != null)
                    {
                        introObj.SetParent(dossierTr, false);
                        introObj.SetSiblingIndex(1);
                        LayoutElement ile = introObj.GetComponent<LayoutElement>();
                        if (ile != null)
                        {
                            ile.preferredHeight = 72f;
                            ile.minHeight = 72f;
                            ile.flexibleHeight = 0f;
                        }
                        TextMeshProUGUI iTmp = introObj.GetComponent<TextMeshProUGUI>();
                        if (iTmp != null)
                        {
                            iTmp.alignment = TextAlignmentOptions.TopLeft;
                            iTmp.fontSize = 13f;
                            iTmp.color = new Color(0.85f, 0.90f, 0.96f, 0.98f);
                            iTmp.lineSpacing = 4f;
                        }
                    }

                    // Dossier Divider
                    Transform divTr = dossierTr.Find("DossierDivider");
                    if (divTr == null)
                    {
                        GameObject divObj = new GameObject("DossierDivider", typeof(RectTransform));
                        divObj.transform.SetParent(dossierTr, false);
                        divObj.transform.SetSiblingIndex(2);
                        RectTransform divRt = divObj.GetComponent<RectTransform>();
                        divRt.sizeDelta = new Vector2(0f, 1f);
                        LayoutElement divLe = divObj.AddComponent<LayoutElement>();
                        divLe.preferredHeight = 1f;
                        divLe.minHeight = 1f;
                        divLe.flexibleHeight = 0f;
                        Image divImg = divObj.AddComponent<Image>();
                        divImg.color = new Color(0.20f, 0.26f, 0.36f, 0.75f);
                    }

                    // Reparent OutroProse into NarrativeDossier
                    Transform outroObj = centerCol.Find("OutroProse");
                    if (outroObj == null) outroObj = dossierTr.Find("OutroProse");
                    if (outroObj != null)
                    {
                        outroObj.SetParent(dossierTr, false);
                        outroObj.SetSiblingIndex(3);
                        LayoutElement ole = outroObj.GetComponent<LayoutElement>();
                        if (ole != null)
                        {
                            ole.preferredHeight = 135f;
                            ole.minHeight = 135f;
                            ole.flexibleHeight = 0f;
                        }
                        TextMeshProUGUI oTmp = outroObj.GetComponent<TextMeshProUGUI>();
                        if (oTmp != null)
                        {
                            oTmp.alignment = TextAlignmentOptions.TopLeft;
                            oTmp.fontSize = 12.5f;
                            oTmp.color = new Color(0.78f, 0.83f, 0.90f, 0.95f);
                            oTmp.lineSpacing = 4f;
                            oTmp.text = "The comments are a storm of denial, prayers, and wild conspiracies. One post catches your eye: a traffic map showing military checkpoints. <color=#F59E0B><i>They are barricading the bridges out of the city.</i></color>\n\nBefore you can scroll further, the little loading icon freezes. The signal drops completely. The internet is gone.";
                        }
                    }

                    // Flexible Spacer to push button neatly to bottom
                    Transform spacerTr = dossierTr.Find("DossierSpacer");
                    if (spacerTr == null)
                    {
                        GameObject spObj = new GameObject("DossierSpacer", typeof(RectTransform));
                        spObj.transform.SetParent(dossierTr, false);
                        spObj.transform.SetSiblingIndex(4);
                        LayoutElement spLe = spObj.AddComponent<LayoutElement>();
                        spLe.flexibleHeight = 1f;
                    }

                    // Reparent ButtonHolder into NarrativeDossier
                    Transform btnHolder = centerCol.Find("ButtonHolder");
                    if (btnHolder == null) btnHolder = dossierTr.Find("ButtonHolder");
                    if (btnHolder != null)
                    {
                        btnHolder.SetParent(dossierTr, false);
                        btnHolder.SetSiblingIndex(5);
                        LayoutElement bhLe = btnHolder.GetComponent<LayoutElement>();
                        if (bhLe != null)
                        {
                            bhLe.preferredHeight = 46f;
                            bhLe.minHeight = 46f;
                            bhLe.flexibleHeight = 0f;
                        }

                        VerticalLayoutGroup bhVlg = btnHolder.GetComponent<VerticalLayoutGroup>();
                        if (bhVlg == null) bhVlg = btnHolder.gameObject.AddComponent<VerticalLayoutGroup>();
                        bhVlg.padding = new RectOffset(0, 0, 0, 0);
                        bhVlg.childControlWidth = true;
                        bhVlg.childControlHeight = true;
                        bhVlg.childForceExpandWidth = true;
                        bhVlg.childForceExpandHeight = true;
                    }

                    // 2. RIGHT WING: SLEEK OBLONG SMARTPHONE CHASSIS (380 x 800 px, 19.5:9 Flagship Ratio)
                    Transform chassis = centerCol.Find("PhoneChassis");
                    if (chassis != null)
                    {
                        RectTransform cr = chassis.GetComponent<RectTransform>();
                        cr.anchorMin = new Vector2(1f, 0.5f);
                        cr.anchorMax = new Vector2(1f, 0.5f);
                        cr.pivot = new Vector2(1f, 0.5f);
                        cr.anchoredPosition = new Vector2(0f, 0f);
                        cr.sizeDelta = new Vector2(380f, 800f);

                        LayoutElement cle = chassis.GetComponent<LayoutElement>();
                        if (cle != null)
                        {
                            cle.preferredWidth = 380f;
                            cle.preferredHeight = 800f;
                            cle.minHeight = 800f;
                        }

                        // Remove redundant duplicate top-right exit button if present
                        Transform topClose = chassis.Find("Btn_TopRightClosePhone");
                        if (topClose != null) Destroy(topClose.gameObject);

                        // Adjust OLED display margins
                        Transform oled = chassis.Find("OledDisplay");
                        if (oled != null)
                        {
                            RectTransform oRt = oled.GetComponent<RectTransform>();
                            oRt.offsetMin = new Vector2(8f, 16f);
                            oRt.offsetMax = new Vector2(-8f, -18f);
                        }

                        // Bottom Home Indicator Pill
                        Transform homePill = chassis.Find("HomePill");
                        if (homePill != null)
                        {
                            RectTransform hpRect = homePill.GetComponent<RectTransform>();
                            if (hpRect != null)
                            {
                                hpRect.sizeDelta = new Vector2(100f, 4f);
                                hpRect.anchoredPosition = new Vector2(0f, 6f);
                            }
                            Image hpImg = homePill.GetComponent<Image>();
                            if (hpImg != null)
                            {
                                hpImg.color = new Color(0.45f, 0.50f, 0.60f, 0.65f);
                            }
                        }
                    }
                }

                // 3. SINGLE UNIFIED ACTION BUTTON (Bottom of Narrative Dossier)
                if (closePhoneButton != null)
                {
                    RectTransform cbr = closePhoneButton.GetComponent<RectTransform>();
                    if (cbr != null)
                    {
                        cbr.anchorMin = Vector2.zero;
                        cbr.anchorMax = Vector2.one;
                        cbr.offsetMin = Vector2.zero;
                        cbr.offsetMax = Vector2.zero;
                        cbr.sizeDelta = new Vector2(0f, 46f);
                    }

                    LayoutElement cle = closePhoneButton.GetComponent<LayoutElement>();
                    if (cle == null) cle = closePhoneButton.gameObject.AddComponent<LayoutElement>();
                    cle.preferredHeight = 46f;
                    cle.minHeight = 46f;
                    cle.flexibleWidth = 1f;

                    Image bg = closePhoneButton.GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.color = new Color(0.14f, 0.09f, 0.03f, 0.98f);
                    }

                    Outline ol = closePhoneButton.GetComponent<Outline>();
                    if (ol == null) ol = closePhoneButton.gameObject.AddComponent<Outline>();
                    ol.effectColor = new Color(0.96f, 0.62f, 0.12f, 0.95f);
                    ol.effectDistance = new Vector2(1.5f, -1.5f);

                    Transform barObj = closePhoneButton.transform.Find("AccentBar");
                    if (barObj != null)
                    {
                        barObj.gameObject.SetActive(true);
                        Image barImg = barObj.GetComponent<Image>();
                        if (barImg != null) barImg.color = new Color(0.98f, 0.65f, 0.15f, 1f);
                    }

                    SetChoiceButtonContent(closePhoneButton,
                        "<b>LOWER THE PHONE & RETURN</b>   <color=#F59E0B><b>[ESC / SPACE]</b></color>");
                }

                // Pre-sanitize all phone TMP texts while phoneModal is still inactive so zero missing-glyph warnings occur on Enable
                SmartphoneFeedController feedCtrl = phoneModal.GetComponent<SmartphoneFeedController>();
                if (feedCtrl != null)
                {
                    feedCtrl.SanitizeAllPhoneGlyphs();
                }
            }

            // B. RADIO MODAL ENHANCEMENT (Live Tactical VHF Receiver with Signal Spectrum Meter & Dynamic Transcript)
            if (radioModal != null)
            {
                Image rBackdrop = radioModal.GetComponent<Image>();
                if (rBackdrop != null)
                {
                    rBackdrop.color = new Color(0.015f, 0.02f, 0.035f, 0.985f);
                }

                Transform radioPanel = radioModal.transform.Find("RadioPanel");
                if (radioPanel != null)
                {
                    RectTransform rpRect = radioPanel.GetComponent<RectTransform>();
                    if (rpRect != null)
                    {
                        rpRect.sizeDelta = new Vector2(680f, 620f);
                    }

                    Image rpBg = radioPanel.GetComponent<Image>();
                    if (rpBg != null)
                    {
                        rpBg.color = new Color(0.035f, 0.048f, 0.068f, 0.98f);
                    }

                    Outline rpOl = radioPanel.GetComponent<Outline>();
                    if (rpOl == null) rpOl = radioPanel.gameObject.AddComponent<Outline>();
                    rpOl.effectColor = new Color(0.95f, 0.65f, 0.18f, 0.65f);
                    rpOl.effectDistance = new Vector2(1.5f, -1.5f);

                    VerticalLayoutGroup vlg = radioPanel.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.padding = new RectOffset(24, 24, 20, 20);
                        vlg.spacing = 10f;
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = true;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                    }

                    // 1. Intro Prose text matching canonical Twine narrative
                    Transform introTr = radioPanel.Find("Intro");
                    if (introTr == null)
                    {
                        GameObject introObj = new GameObject("Intro", typeof(RectTransform));
                        introObj.transform.SetParent(radioPanel, false);
                        introObj.transform.SetSiblingIndex(0);

                        TextMeshProUGUI iTmp = introObj.AddComponent<TextMeshProUGUI>();
                        iTmp.fontSize = 11.5f;
                        iTmp.color = new Color(0.85f, 0.88f, 0.92f);
                        iTmp.lineSpacing = 5f;
                        iTmp.text = "You pull your emergency-band scanner from your gear bag. Your hands are shaking slightly as you dial into the encrypted precinct frequency. The speaker hisses with white noise before a voice breaks through.";
                        SundayTypography.ApplyFont(iTmp, FontRole.Body);

                        LayoutElement iLe = introObj.AddComponent<LayoutElement>();
                        iLe.preferredHeight = 44f;
                        iLe.minHeight = 38f;
                    }

                    // 2. Freq Header
                    Transform fhTr = radioPanel.Find("FreqHeader");
                    if (fhTr != null)
                    {
                        radioFreqHeaderText = fhTr.GetComponent<TextMeshProUGUI>();
                        LayoutElement fhLe = fhTr.GetComponent<LayoutElement>() ?? fhTr.gameObject.AddComponent<LayoutElement>();
                        fhLe.preferredHeight = 22f;
                        fhLe.minHeight = 22f;
                        if (radioFreqHeaderText != null)
                        {
                            radioFreqHeaderText.text = "FREQ: 154.790 MHz                                    <color=#ef4444>● REC</color>";
                            SundayTypography.ApplyFont(radioFreqHeaderText, FontRole.Mono);
                        }
                    }

                    // 3. Add Live RF Signal & Waveform Visualizer Bar right under FreqHeader
                    Transform sigTr = radioPanel.Find("SignalMeterBar");
                    if (sigTr == null)
                    {
                        GameObject sigObj = new GameObject("SignalMeterBar", typeof(RectTransform));
                        sigObj.transform.SetParent(radioPanel, false);
                        sigObj.transform.SetSiblingIndex(2);

                        Image sigBg = sigObj.AddComponent<Image>();
                        sigBg.color = new Color(0.02f, 0.03f, 0.045f, 0.95f);

                        Outline sigOl = sigObj.AddComponent<Outline>();
                        sigOl.effectColor = new Color(0.20f, 0.30f, 0.42f, 0.75f);

                        LayoutElement sigLe = sigObj.AddComponent<LayoutElement>();
                        sigLe.preferredHeight = 26f;
                        sigLe.minHeight = 26f;

                        GameObject sigTxtObj = new GameObject("MeterText", typeof(RectTransform));
                        sigTxtObj.transform.SetParent(sigObj.transform, false);
                        RectTransform stRt = sigTxtObj.GetComponent<RectTransform>();
                        stRt.anchorMin = Vector2.zero;
                        stRt.anchorMax = Vector2.one;
                        stRt.offsetMin = new Vector2(10f, 0f);
                        stRt.offsetMax = new Vector2(-10f, 0f);

                        radioSignalMeterText = sigTxtObj.AddComponent<TextMeshProUGUI>();
                        radioSignalMeterText.fontSize = 10.5f;
                        radioSignalMeterText.alignment = TextAlignmentOptions.MidlineLeft;
                        radioSignalMeterText.color = new Color(0.38f, 0.85f, 0.65f);
                        SundayTypography.ApplyFont(radioSignalMeterText, FontRole.Mono);
                    }
                    else
                    {
                        radioSignalMeterText = sigTr.GetComponentInChildren<TextMeshProUGUI>(true);
                    }

                    Transform transTr = radioPanel.Find("Transcript");
                    if (transTr != null)
                    {
                        radioTranscriptText = transTr.GetComponent<TextMeshProUGUI>();
                        LayoutElement tLe = transTr.GetComponent<LayoutElement>() ?? transTr.gameObject.AddComponent<LayoutElement>();
                        tLe.preferredHeight = 310f;
                        tLe.minHeight = 280f;
                        tLe.flexibleHeight = 1f;
                        if (radioTranscriptText != null)
                        {
                            radioTranscriptText.fontSize = 11.5f;
                            radioTranscriptText.lineSpacing = 6f;
                            SundayTypography.ApplyFont(radioTranscriptText, FontRole.Mono);
                        }
                    }

                    Transform outroTr = radioPanel.Find("Outro");
                    if (outroTr != null)
                    {
                        radioOutroObj = outroTr.gameObject;
                        LayoutElement oLe = outroTr.GetComponent<LayoutElement>() ?? outroTr.gameObject.AddComponent<LayoutElement>();
                        oLe.preferredHeight = 46f;
                        oLe.minHeight = 42f;
                        TextMeshProUGUI oTmp = outroTr.GetComponent<TextMeshProUGUI>();
                        if (oTmp != null)
                        {
                            oTmp.fontSize = 11.5f;
                            oTmp.lineSpacing = 5f;
                            oTmp.color = new Color(0.85f, 0.85f, 0.88f);
                            oTmp.text = "<color=#ef4444><b>'Standard rules of engagement are suspended.'</b></color> Total lethal force. No warnings, no arrests, no hesitation. They only issue that for active warzones. You stare at the radio. Whatever is out there, the law is officially dead.";
                            SundayTypography.ApplyFont(oTmp, FontRole.Body);
                        }
                    }
                }

                if (closeRadioButton != null)
                {
                    LayoutElement le = closeRadioButton.GetComponent<LayoutElement>() ?? closeRadioButton.gameObject.AddComponent<LayoutElement>();
                    le.preferredHeight = 44f;
                    le.minHeight = 44f;
                    le.flexibleWidth = 1f;

                    Image bg = closeRadioButton.GetComponent<Image>();
                    if (bg != null) bg.color = new Color(0.14f, 0.09f, 0.03f, 0.98f);

                    Outline ol = closeRadioButton.GetComponent<Outline>() ?? closeRadioButton.gameObject.AddComponent<Outline>();
                    ol.effectColor = new Color(0.96f, 0.62f, 0.12f, 0.95f);

                    SetChoiceButtonContent(closeRadioButton,
                        "<b>TURN OFF THE RADIO & RETURN TO LIVING ROOM</b>   <color=#F59E0B><b>[ESC / SPACE]</b></color>");
                }
            }

            // C. HACKING TERMINAL MODAL ENHANCEMENT (Panasonic Toughbook Rugged Tactical Laptop & CRT Workstation)
            if (hackingModal != null)
            {
                Image hBackdrop = hackingModal.GetComponent<Image>();
                if (hBackdrop != null)
                {
                    hBackdrop.color = new Color(0.012f, 0.016f, 0.024f, 0.985f);
                }

                Transform hackPanel = hackingModal.transform.Find("HackingPanel");
                if (hackPanel != null)
                {
                    RectTransform pRect = hackPanel.GetComponent<RectTransform>();
                    if (pRect != null)
                    {
                        pRect.sizeDelta = new Vector2(960f, 690f);
                    }

                    Image pBg = hackPanel.GetComponent<Image>();
                    if (pBg != null)
                    {
                        pBg.color = new Color(0.065f, 0.075f, 0.09f, 1f); // Industrial dark matte polycarbonate chassis
                    }

                    Outline pOl = hackPanel.GetComponent<Outline>() ?? hackPanel.gameObject.AddComponent<Outline>();
                    pOl.effectColor = new Color(0.28f, 0.35f, 0.44f, 0.95f);
                    pOl.effectDistance = new Vector2(2f, -2f);

                    VerticalLayoutGroup vlg = hackPanel.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.padding = new RectOffset(16, 16, 10, 12);
                        vlg.spacing = 6f;
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = true;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                    }

                    // 1. Top Bezel Strip with LEDs, WebCam, and Model Designation
                    Transform topBezelTr = hackPanel.Find("LaptopTopBezel");
                    if (topBezelTr == null)
                    {
                        GameObject topBezelObj = new GameObject("LaptopTopBezel", typeof(RectTransform));
                        topBezelObj.transform.SetParent(hackPanel, false);
                        topBezelObj.transform.SetSiblingIndex(0);

                        LayoutElement tbLe = topBezelObj.AddComponent<LayoutElement>();
                        tbLe.preferredHeight = 22f;
                        tbLe.minHeight = 22f;

                        // Left LEDs
                        GameObject ledObj = new GameObject("Leds", typeof(RectTransform));
                        ledObj.transform.SetParent(topBezelObj.transform, false);
                        RectTransform ledRt = ledObj.GetComponent<RectTransform>();
                        ledRt.anchorMin = new Vector2(0f, 0f);
                        ledRt.anchorMax = new Vector2(0.35f, 1f);
                        ledRt.offsetMin = Vector2.zero;
                        ledRt.offsetMax = Vector2.zero;
                        hackingLedText = ledObj.AddComponent<TextMeshProUGUI>();
                        hackingLedText.fontSize = 10f;
                        hackingLedText.alignment = TextAlignmentOptions.MidlineLeft;
                        hackingLedText.text = "<color=#10b981>● PWR</color>   <color=#f59e0b>● HDD</color>   <color=#38bdf8>● NET</color>";
                        SundayTypography.ApplyFont(hackingLedText, FontRole.Mono);

                        // Center WebCam lens
                        GameObject camObj = new GameObject("WebCam", typeof(RectTransform));
                        camObj.transform.SetParent(topBezelObj.transform, false);
                        RectTransform camRt = camObj.GetComponent<RectTransform>();
                        camRt.anchorMin = new Vector2(0.35f, 0f);
                        camRt.anchorMax = new Vector2(0.65f, 1f);
                        camRt.offsetMin = Vector2.zero;
                        camRt.offsetMax = Vector2.zero;
                        TextMeshProUGUI camTmp = camObj.AddComponent<TextMeshProUGUI>();
                        camTmp.fontSize = 9.5f;
                        camTmp.alignment = TextAlignmentOptions.Center;
                        camTmp.color = new Color(0.40f, 0.48f, 0.58f);
                        camTmp.text = "[ ● ] HD WEBCAM // IR SENSOR";
                        SundayTypography.ApplyFont(camTmp, FontRole.Mono);

                        // Right Model Badge
                        GameObject modObj = new GameObject("ModelBadge", typeof(RectTransform));
                        modObj.transform.SetParent(topBezelObj.transform, false);
                        RectTransform modRt = modObj.GetComponent<RectTransform>();
                        modRt.anchorMin = new Vector2(0.65f, 0f);
                        modRt.anchorMax = new Vector2(1f, 1f);
                        modRt.offsetMin = Vector2.zero;
                        modRt.offsetMax = Vector2.zero;
                        TextMeshProUGUI modTmp = modObj.AddComponent<TextMeshProUGUI>();
                        modTmp.fontSize = 9.5f;
                        modTmp.alignment = TextAlignmentOptions.MidlineRight;
                        modTmp.color = new Color(0.40f, 0.48f, 0.58f);
                        modTmp.text = "PANASONIC TOUGHBOOK CF-33";
                        SundayTypography.ApplyFont(modTmp, FontRole.Mono);
                    }
                    else
                    {
                        hackingLedText = topBezelTr.Find("Leds")?.GetComponent<TextMeshProUGUI>();
                    }

                    // 2. Laptop Screen Container (Inner Display)
                    Transform screenTr = hackPanel.Find("LaptopScreen");
                    if (screenTr == null)
                    {
                        GameObject screenObj = new GameObject("LaptopScreen", typeof(RectTransform));
                        screenObj.transform.SetParent(hackPanel, false);
                        screenObj.transform.SetSiblingIndex(1);

                        Image sBg = screenObj.AddComponent<Image>();
                        sBg.color = new Color(0.015f, 0.022f, 0.032f, 1f); // Deep matte CRT display

                        Outline sOl = screenObj.AddComponent<Outline>();
                        sOl.effectColor = new Color(0.12f, 0.16f, 0.24f, 1f);

                        VerticalLayoutGroup sVlg = screenObj.AddComponent<VerticalLayoutGroup>();
                        sVlg.padding = new RectOffset(14, 14, 8, 10);
                        sVlg.spacing = 6f;
                        sVlg.childControlWidth = true;
                        sVlg.childControlHeight = true;
                        sVlg.childForceExpandWidth = true;
                        sVlg.childForceExpandHeight = false;

                        LayoutElement sLe = screenObj.AddComponent<LayoutElement>();
                        sLe.preferredHeight = 600f;
                        sLe.flexibleHeight = 1f;

                        // Reparent existing Header, TerminalText, and FinishContainer into the new screen container
                        Transform origHeader = hackPanel.Find("Header");
                        if (origHeader != null) origHeader.SetParent(screenObj.transform, false);

                        Transform origText = hackPanel.Find("TerminalText");
                        if (origText != null) origText.SetParent(screenObj.transform, false);

                        Transform origFinish = hackPanel.Find("FinishContainer");
                        if (origFinish != null) origFinish.SetParent(screenObj.transform, false);
                    }

                    // 3. Header Bar inside LaptopScreen
                    Transform hTr = hackPanel.Find("LaptopScreen/Header") ?? hackPanel.Find("Header");
                    if (hTr != null)
                    {
                        TextMeshProUGUI directTmp = hTr.GetComponent<TextMeshProUGUI>();
                        if (directTmp != null && hTr.GetComponent<Image>() == null)
                        {
                            // Legacy structure: Header itself was TextMeshProUGUI.
                            // Convert Header into a container with child Text so Image and Text can coexist without Graphic component conflict.
                            GameObject headerContainer = new GameObject("HeaderBar", typeof(RectTransform));
                            headerContainer.transform.SetParent(hTr.parent, false);
                            headerContainer.transform.SetSiblingIndex(hTr.GetSiblingIndex());

                            Image newBg = headerContainer.AddComponent<Image>();
                            newBg.color = new Color(0.05f, 0.08f, 0.14f, 0.95f);

                            LayoutElement newLe = headerContainer.AddComponent<LayoutElement>();
                            newLe.preferredHeight = 24f;
                            newLe.minHeight = 24f;

                            hTr.SetParent(headerContainer.transform, false);
                            hTr.name = "TitleText";
                            RectTransform directRt = hTr.GetComponent<RectTransform>();
                            if (directRt != null)
                            {
                                directRt.anchorMin = Vector2.zero;
                                directRt.anchorMax = Vector2.one;
                                directRt.offsetMin = new Vector2(8f, 0f);
                                directRt.offsetMax = new Vector2(-8f, 0f);
                            }

                            headerContainer.name = "Header";
                            hTr = headerContainer.transform;
                        }
                        else
                        {
                            Image hBg = hTr.GetComponent<Image>();
                            if (hBg == null && directTmp == null)
                            {
                                hBg = hTr.gameObject.AddComponent<Image>();
                            }
                            if (hBg != null)
                            {
                                hBg.color = new Color(0.05f, 0.08f, 0.14f, 0.95f);
                            }

                            LayoutElement hLe = hTr.GetComponent<LayoutElement>() ?? hTr.gameObject.AddComponent<LayoutElement>();
                            hLe.preferredHeight = 24f;
                            hLe.minHeight = 24f;
                        }

                        TextMeshProUGUI hTmp = hTr.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (hTmp != null)
                        {
                            hTmp.fontSize = 10.5f;
                            hTmp.fontStyle = FontStyles.Bold;
                            hTmp.text = "<b><color=#38bdf8>[>]</color> terminal@unit407: ~/recon/defense_grid</b>               <color=#64748b>[ETH0: 1 Gbps]</color>   <color=#10b981>[VPN: ACTIVE]</color>   <color=#94a3b8>[ — ]  [ □ ]  [ ✕ ]</color>";
                            SundayTypography.ApplyFont(hTmp, FontRole.Mono);
                        }
                    }

                    // 4. Terminal Text inside LaptopScreen
                    Transform tTr = hackPanel.Find("LaptopScreen/TerminalText") ?? hackPanel.Find("TerminalText");
                    if (tTr != null)
                    {
                        hackingTerminalText = tTr.GetComponent<TextMeshProUGUI>();
                        if (hackingTerminalText != null)
                        {
                            hackingTerminalText.fontSize = 11.5f;
                            hackingTerminalText.lineSpacing = 5f;
                            hackingTerminalText.color = new Color(0.20f, 0.90f, 0.45f); // Phosphor green
                            SundayTypography.ApplyFont(hackingTerminalText, FontRole.Mono);
                        }

                        LayoutElement tLe = tTr.GetComponent<LayoutElement>() ?? tTr.gameObject.AddComponent<LayoutElement>();
                        tLe.preferredHeight = 360f;
                        tLe.minHeight = 320f;
                        tLe.flexibleHeight = 1f;
                    }

                    // 5. Finish Container inside LaptopScreen
                    Transform fTr = hackPanel.Find("LaptopScreen/FinishContainer") ?? hackPanel.Find("FinishContainer");
                    if (fTr != null)
                    {
                        hackingFinishContainer = fTr.gameObject;
                        VerticalLayoutGroup fVlg = fTr.GetComponent<VerticalLayoutGroup>();
                        if (fVlg != null)
                        {
                            fVlg.spacing = 6f;
                            fVlg.childControlWidth = true;
                            fVlg.childForceExpandWidth = true;
                        }

                        Transform oTr = fTr.Find("Outro");
                        if (oTr != null)
                        {
                            TextMeshProUGUI foTmp = oTr.GetComponent<TextMeshProUGUI>();
                            if (foTmp != null)
                            {
                                foTmp.fontSize = 11.5f;
                                foTmp.lineSpacing = 5f;
                                foTmp.color = new Color(0.85f, 0.88f, 0.92f);
                                foTmp.text = "<color=#ef4444><b>'SECTOR PURGE AT 0600 HOURS.'</b></color> The military is firebombing the city at dawn. You secured a <b><color=#ffd700>Tier-1 Military Clearance Passkey</color></b> to your device.";
                                SundayTypography.ApplyFont(foTmp, FontRole.Body);
                            }

                            LayoutElement oLe = oTr.GetComponent<LayoutElement>() ?? oTr.gameObject.AddComponent<LayoutElement>();
                            oLe.preferredHeight = 38f;
                            oLe.minHeight = 34f;
                        }
                    }

                    // 6. Bottom Bezel / Keyboard Deck Strip
                    Transform deckTr = hackPanel.Find("LaptopDeckStrip");
                    if (deckTr == null)
                    {
                        GameObject deckObj = new GameObject("LaptopDeckStrip", typeof(RectTransform));
                        deckObj.transform.SetParent(hackPanel, false);

                        LayoutElement dLe = deckObj.AddComponent<LayoutElement>();
                        dLe.preferredHeight = 18f;
                        dLe.minHeight = 18f;

                        TextMeshProUGUI dTmp = deckObj.AddComponent<TextMeshProUGUI>();
                        dTmp.fontSize = 9.5f;
                        dTmp.alignment = TextAlignmentOptions.Center;
                        dTmp.color = new Color(0.35f, 0.42f, 0.50f);
                        dTmp.text = "— — —  [ EMBEDDED TPM 2.0 CRYPTOGRAPHIC MODULE ACTIVE // MIL-STD-810G ]  — — —";
                        SundayTypography.ApplyFont(dTmp, FontRole.Mono);
                    }
                }

                if (closeHackingButton != null)
                {
                    LayoutElement le = closeHackingButton.GetComponent<LayoutElement>() ?? closeHackingButton.gameObject.AddComponent<LayoutElement>();
                    le.preferredHeight = 44f;
                    le.minHeight = 44f;
                    le.flexibleWidth = 1f;

                    Image bg = closeHackingButton.GetComponent<Image>();
                    if (bg != null) bg.color = new Color(0.03f, 0.14f, 0.07f, 0.98f);

                    Outline ol = closeHackingButton.GetComponent<Outline>() ?? closeHackingButton.gameObject.AddComponent<Outline>();
                    ol.effectColor = new Color(0.25f, 0.95f, 0.50f, 0.95f);

                    SetChoiceButtonContent(closeHackingButton,
                        "<b>SHUT DOWN LAPTOP & RETURN TO LIVING ROOM</b>   <color=#34D399><b>[ESC / SPACE]</b></color>");
                }
            }
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
            roomFloorStashes.Clear();
            roomFloorStashes["unit_407"] = floorItems;
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
            phoneVisited = false;
            radioVisited = false;
            hackingVisited = false;
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

            ShowAwakeningStep(1);
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
            if (!gameObject.activeInHierarchy) return;

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
            if (!gameObject.activeInHierarchy) return;

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

            // Restore awakening or hub sequence state (Beat 1, Beat 2, Beat 3, or Hub Choices)
            if (hasPacked || introSeen || awakeningStep > 3)
            {
                introSeen = true;
                awakeningStep = 4;
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
                if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
                {
                    currentPlayer = GameManager.Instance.CurrentPlayer;
                }
                else
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
            }

            // Play background atmosphere drone
            if (atmosphereSource != null && atmosphereClip != null && !atmosphereSource.isPlaying)
            {
                atmosphereSource.clip = atmosphereClip;
                atmosphereSource.volume = 0.5f;
                atmosphereSource.Play();
            }

            // Preserve the exact Beat (1, 2, or 3) or Hub Menu state set by InitializeWithCharacter / LoadCharacterState
            if (hasPacked || introSeen || awakeningStep > 3)
            {
                ShowHubMenu();
            }
            else if (awakeningContainer != null && !awakeningContainer.activeSelf && (hubMenuContainer == null || !hubMenuContainer.activeSelf))
            {
                ShowAwakeningStep(Mathf.Clamp(awakeningStep, 1, 3));
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
            if (isDoorTransitioning || (doorTransitionOverlay != null && doorTransitionOverlay.activeSelf)) return;

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

            // 0. Hotkey to dismiss Inspection Modals (Phone / Radio / Hacking) if open (1, Escape, Space, Enter)
            bool anyInspectionModalOpen = (phoneModal != null && phoneModal.activeSelf) ||
                                          (radioModal != null && radioModal.activeSelf) ||
                                          (hackingModal != null && hackingModal.activeSelf && hackingFinishContainer != null && hackingFinishContainer.activeSelf);
            if (anyInspectionModalOpen)
            {
                bool dismissModal = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    dismissModal = Keyboard.current.escapeKey.wasPressedThisFrame ||
                                   Keyboard.current.digit1Key.wasPressedThisFrame ||
                                   Keyboard.current.numpad1Key.wasPressedThisFrame ||
                                   Keyboard.current.spaceKey.wasPressedThisFrame ||
                                   Keyboard.current.enterKey.wasPressedThisFrame ||
                                   Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                }
#else
                dismissModal = Input.GetKeyDown(KeyCode.Escape) ||
                               Input.GetKeyDown(KeyCode.Alpha1) ||
                               Input.GetKeyDown(KeyCode.Keypad1) ||
                               Input.GetKeyDown(KeyCode.Space) ||
                               Input.GetKeyDown(KeyCode.Return) ||
                               Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
                if (dismissModal)
                {
                    bool isEscape = false;
#if ENABLE_INPUT_SYSTEM
                    if (Keyboard.current != null) isEscape = Keyboard.current.escapeKey.wasPressedThisFrame;
#else
                    isEscape = Input.GetKeyDown(KeyCode.Escape);
#endif

                    if (phoneModal != null && phoneModal.activeSelf)
                    {
                        var feedCtrl = phoneModal.GetComponentInChildren<SmartphoneFeedController>(true);
                        if (feedCtrl != null && !feedCtrl.IsNetworkDead && !isEscape)
                        {
                            feedCtrl.FastForwardFeed();
                        }
                        else
                        {
                            OnClosePhoneClicked();
                        }
                    }
                    else if (radioModal != null && radioModal.activeSelf)
                    {
                        if (isRadioTransmitting && !isEscape)
                        {
                            FastForwardRadio();
                        }
                        else
                        {
                            OnCloseRadioClicked();
                        }
                    }
                    else if (hackingModal != null && hackingModal.activeSelf)
                    {
                        if (isHackingTerminal && !isEscape)
                        {
                            FastForwardHacking();
                        }
                        else
                        {
                            OnCloseHackingClicked();
                        }
                    }
                    return;
                }
            }

            // 1. Hotkeys for Choice Selection (1-9) when in Narrative Choice Hub Mode
            if (currentConsoleMode == ConsoleMode.Narrative && hubMenuContainer != null && hubMenuContainer.activeInHierarchy)
            {
                for (int i = 0; i < Mathf.Min(activeChoiceButtons.Count, 9); i++)
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
            if (radioModal != null && radioModal.activeSelf && isRadioTransmitting)
            {
                FastForwardRadio();
                return;
            }

            if (phoneModal != null && phoneModal.activeSelf)
            {
                var feedCtrl = phoneModal.GetComponentInChildren<SmartphoneFeedController>(true);
                if (feedCtrl != null && !feedCtrl.IsNetworkDead)
                {
                    feedCtrl.FastForwardFeed();
                    return;
                }
            }

            if (hackingModal != null && hackingModal.activeSelf && isHackingTerminal)
            {
                FastForwardHacking();
                return;
            }

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
                string targetLoc = !string.IsNullOrEmpty(currentPlayer?.currentLocationId)
                    ? currentPlayer.currentLocationId
                    : GetCurrentLocationId();
                tacticalMapController.SetCurrentLocation(targetLoc);
                tacticalMapController.SelectNode(targetLoc);
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
            var activeFloor = GetCurrentFloorStash();
            int count = activeFloor != null ? activeFloor.Count : 0;
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

            var activeFloor = GetCurrentFloorStash();
            if (tacticalGearController != null)
            {
                tacticalGearController.Initialize(currentPlayer, activeFloor);
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
            bool hasFloorLoot = activeFloor != null && activeFloor.Count > 0;
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
                    foreach (var item in activeFloor)
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
            awakeningStep = Mathf.Clamp(step, 1, 3);
            introSeen = false;

            if (awakeningContainer != null) awakeningContainer.SetActive(true);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(true);

            if (beat1Container != null) beat1Container.SetActive(step == 1);
            if (beat2Container != null) beat2Container.SetActive(step == 2);
            if (beat3Container != null) beat3Container.SetActive(step == 3);

            if (!gameObject.activeInHierarchy)
            {
                if (step == 1)
                {
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#EF4444>CHANNEL: EMERGENCY BROADCAST SYSTEM</color>";
                    if (beat1IntroText != null) beat1IntroText.text = "You turn away from the glass. The silence in your apartment breaks—not with a scream, but with an agonizing, electronic tone pulsing from the television.";
                }
                else if (step == 2)
                {
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // INTERNAL MONOLOGUE ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#60A5FA>CHANNEL: NEURAL APPRAISAL</color>";
                    if (monologueBox != null) monologueBox.SetActive(true);
                    if (monologueSpeakerText != null) monologueSpeakerText.text = currentPlayer != null ? $"YOU // {currentPlayer.FullName.ToUpper()}" : "YOU";
                    if (monologueSpeechText != null) monologueSpeechText.text = GetPlayerMonologueText();
                    if (beat2IntroText != null) beat2IntroText.text = "The robotic voice cuts out. Cold static returns to the TV.";
                    if (beat2FootnoteText != null) beat2FootnoteText.text = "Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door.";
                }
                else if (step == 3)
                {
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#F59E0B>CHANNEL: EQUIPMENT INSPECTION</color>";
                    if (tableStashDisplay != null) { tableStashDisplay.SetActive(true); PopulateTableStashUI(); }
                    if (beat3NarrativeText != null) beat3NarrativeText.text = "You stand in the middle of your living room in the pitch-black darkness. Outside, sirens wail across the Seattle skyline.\n\nYour apartment keys, wallet, and belongings are sitting on the coffee table. The front door deadbolt is locked—you cannot step out into the hallway without your keys, or you'll be locked out in the dark.\n\nGrab your apartment keys and belongings off the coffee table, and check what else you can gather around the apartment before unlocking the door.";
                }
                return;
            }

            switch (awakeningStep)
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
            introSeen = true;
            awakeningStep = 4;

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
            if (isInHallway) return;

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
                    bool hasTech = currentPlayer != null && currentPlayer.tech >= 1;
                    bool hasIntl = currentPlayer != null && currentPlayer.Tactics >= 5;
                    bool hasRadioTrait = currentPlayer != null && currentPlayer.HasTrait("RADIO OPERATOR");

                    if (isPolice)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Turn on your department transceiver and listen to Seattle PD dispatch.</b>  <color=#64748B><i>(~3 min) [Police Vocation]</i></color>");
                    }
                    else if (hasRadioTrait)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Dial into the emergency dispatch band using your scanner.</b>  <color=#64748B><i>(~4 min) [Radio Operator]</i></color>");
                    }
                    else if (hasTech)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Manually calibrate the radio transceiver to sweep police frequencies.</b>  <color=#64748B><i>(~5 min) [Tech Knowledge]</i></color>");
                    }
                    else if (hasIntl)
                    {
                        listenRadioButton.interactable = true;
                        SetChoiceButtonContent(listenRadioButton, "<b>Deduce the encrypted precinct frequencies and tune the scanner.</b>  <color=#64748B><i>(~5 min) [Tactics / Intelligence]</i></color>");
                    }
                    else
                    {
                        listenRadioButton.interactable = false;
                        SetChoiceButtonContent(listenRadioButton, "<s><color=#64748B>Tune the radio to emergency dispatch frequencies.</color></s>  <color=#EF4444><i>[Requires Police, Tech, or Intelligence (Tactics 5+)]</i></color>");
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

                    bool hasTechHacking = currentPlayer != null && currentPlayer.tech >= 2;
                    bool hasIntlHacking = currentPlayer != null && (currentPlayer.Tactics >= 7 || (currentPlayer.Tactics >= 6 && currentPlayer.tech >= 1));
                    bool hasTraitHacking = currentPlayer != null && (currentPlayer.HasTrait("GAMER") || currentPlayer.HasTrait("RADIO OPERATOR")) && currentPlayer.Tactics >= 5;

                    if (hasTechHacking)
                    {
                        hackTerminalButton.interactable = true;
                        SetChoiceButtonContent(hackTerminalButton, "<b>Exploit building network vulnerabilities to inspect military traffic.</b>  <color=#64748B><i>(~8 min) [Tech Knowledge]</i></color>");
                    }
                    else if (hasIntlHacking)
                    {
                        hackTerminalButton.interactable = true;
                        SetChoiceButtonContent(hackTerminalButton, "<b>Analyze encrypted network packets to bypass defense protocols.</b>  <color=#64748B><i>(~8 min) [Tactics / Intelligence]</i></color>");
                    }
                    else if (hasTraitHacking)
                    {
                        hackTerminalButton.interactable = true;
                        SetChoiceButtonContent(hackTerminalButton, "<b>Use simulation & codebreaking algorithms to penetrate the node.</b>  <color=#64748B><i>(~8 min) [Simulation / Trait]</i></color>");
                    }
                    else
                    {
                        hackTerminalButton.interactable = false;
                        SetChoiceButtonContent(hackTerminalButton, "<s><color=#64748B>Log onto the computer to inspect encrypted network traffic.</color></s>  <color=#EF4444><i>[Requires Tech 2+ or Intelligence (Tactics 6+)]</i></color>");
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
            var currentStash = GetCurrentFloorStash();
            var existing = currentStash.Find(f => string.Equals(f.itemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                currentStash.Add(new InventoryEntry(itemId, quantity));
            }
        }

        private void GiveOrDropItem(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0 || currentPlayer == null) return;
            if (!currentPlayer.AddItem(itemId, quantity))
            {
                AddFloorItem(itemId, quantity);
                UpdateFloorItemsNotification();
                if (situationBoxText != null)
                {
                    situationBoxText.text += $"\n<color=#F59E0B>[!] Backpack full: {itemId} (x{quantity}) placed in room container.</color>";
                }
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
            var currentStash = GetCurrentFloorStash();
            if (currentPlayer == null || currentStash.Count == 0) return;

            int taken = 0;
            for (int i = currentStash.Count - 1; i >= 0; i--)
            {
                var item = currentStash[i];
                if (currentPlayer.AddItem(item.itemId, item.quantity))
                {
                    currentStash.RemoveAt(i);
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

            var currentStash = GetCurrentFloorStash();
            int idx = currentStash.FindIndex(i => i.itemId == itemId);
            if (idx >= 0)
            {
                var item = currentStash[idx];
                if (currentPlayer.AddItem(item.itemId, item.quantity))
                {
                    currentStash.RemoveAt(idx);

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

            EnsureInspectionModalsEnhanced();

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

            EnsureInspectionModalsEnhanced();

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

            if (radioRoutine != null) StopCoroutine(radioRoutine);
            radioRoutine = StartCoroutine(RadioTransmissionRoutine());

            if (radioMeterRoutine != null) StopCoroutine(radioMeterRoutine);
            radioMeterRoutine = StartCoroutine(RadioWaveformMeterRoutine());
        }

        private IEnumerator RadioWaveformMeterRoutine()
        {
            string[] wavePatterns = new string[]
            {
                "|| ||||||| |||| ||||||||| ||| ||||||",
                "||||| ||| |||||||| || ||||||| ||||||",
                "||| ||||||||| |||| ||||| |||||||| ||",
                "||||||| || |||||| ||||||||| |||| |||",
                "|||| |||||||| ||||||| || ||||| |||||"
            };

            int tick = 0;
            while (isRadioTransmitting && radioModal != null && radioModal.activeSelf)
            {
                if (radioFreqHeaderText != null)
                {
                    string recDot = (tick % 2 == 0)
                        ? "<color=#ef4444>● LIVE RX</color>"
                        : "<color=#7f1d1d>● LIVE RX</color>";
                    radioFreqHeaderText.text = $"FREQ: 154.790 MHz // SEATTLE PD DISPATCH [CH-1]          {recDot}";
                }

                if (radioSignalMeterText != null)
                {
                    int dbm = -54 - UnityEngine.Random.Range(0, 12);
                    string wave = wavePatterns[tick % wavePatterns.Length];
                    radioSignalMeterText.text = $"<color=#34d399><b>RF SIGNAL:</b></color> <color=#10b981>[ {wave} ]</color>  <color=#94a3b8>{dbm} dBm // VHF ENCRYPTED</color>";
                }

                tick++;
                yield return new WaitForSecondsRealtime(0.18f);
            }

            if (radioFreqHeaderText != null)
            {
                radioFreqHeaderText.text = "FREQ: 154.790 MHz // SEATTLE PD DISPATCH [CH-1]          <color=#ef4444>[X] CARRIER LOST</color>";
            }

            if (radioSignalMeterText != null)
            {
                radioSignalMeterText.text = "<color=#ef4444><b>RF SIGNAL:</b> [ ------------------------------------ ]  NO CARRIER // DEAD AIR</color>";
            }
        }

        private IEnumerator RadioTransmissionRoutine()
        {
            isRadioTransmitting = true;

            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(false);
            }

            if (radioTranscriptText != null)
            {
                radioTranscriptText.text = "<color=#94A3B8>[STATIC HISS]</color>";
            }

            PlayBeepSFX();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#94A3B8>[STATIC HISS]</color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 0.0s to 1.6s: Static hiss / frequency lock
            yield return new WaitForSecondsRealtime(1.6f);
            if (!isRadioTransmitting) yield break;

            // 1.6s to 11.4s: UNIT 4-CHARLIE First Transmission (Barricade lost)
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            string line1 = "<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing! They just keep coming!\"</i>\n";
            string prefix1 = sb.ToString();

            float line1Duration = 9.8f;
            float charDelay1 = line1Duration / line1.Length;
            for (int i = 1; i <= line1.Length; i += 2)
            {
                if (!isRadioTransmitting) yield break;
                int len = Mathf.Min(i, line1.Length);
                if (radioTranscriptText != null) radioTranscriptText.text = prefix1 + line1.Substring(0, len);
                yield return new WaitForSecondsRealtime(charDelay1 * 2);
            }
            sb.AppendLine(line1);
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 11.5s to 13.3s: Heavy Struggle & Screen Shake
            if (!isRadioTransmitting) yield break;
            yield return new WaitForSecondsRealtime(0.2f);
            PlayScreenShake();
            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            yield return new WaitForSecondsRealtime(1.7f);
            if (!isRadioTransmitting) yield break;

            // 13.4s to 21.4s: UNIT 4-CHARLIE Second Transmission (Get off the car, unrestricted lethal force)
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            string line2 = "<i>\"God damn it, get off the damn car! Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n";
            string prefix2 = sb.ToString();

            float line2Duration = 7.8f;
            float charDelay2 = line2Duration / line2.Length;
            for (int i = 1; i <= line2.Length; i += 2)
            {
                if (!isRadioTransmitting) yield break;
                int len = Mathf.Min(i, line2.Length);
                if (radioTranscriptText != null) radioTranscriptText.text = prefix2 + line2.Substring(0, len);
                yield return new WaitForSecondsRealtime(charDelay2 * 2);
            }
            sb.AppendLine(line2);
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 21.5s to 22.1s: Radio Squelch
            yield return new WaitForSecondsRealtime(0.7f);
            if (!isRadioTransmitting) yield break;
            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            yield return new WaitForSecondsRealtime(0.6f);
            if (!isRadioTransmitting) yield break;

            // 22.2s to 35.4s: DISPATCH (CALM)
            sb.AppendLine("<b><color=#F59E0B>DISPATCH (CALM):</color></b>");
            string line3 = "<i>\"4-Charlie, unrestricted lethal force is authorized. All units, listen carefully. Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. <color=#EF4444>Aim for the head.</color> I repeat... <color=#EF4444>aim for the head.</color> Avoid all physical contact.\"</i>\n";
            string prefix3 = sb.ToString();

            float line3Duration = 13.0f;
            float charDelay3 = line3Duration / line3.Length;
            for (int i = 1; i <= line3.Length; i += 2)
            {
                if (!isRadioTransmitting) yield break;
                int len = Mathf.Min(i, line3.Length);
                if (radioTranscriptText != null) radioTranscriptText.text = prefix3 + line3.Substring(0, len);
                yield return new WaitForSecondsRealtime(charDelay3 * 2);
            }
            sb.AppendLine(line3);
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            // 35.5s: Signal Cuts Out
            yield return new WaitForSecondsRealtime(0.5f);
            isRadioTransmitting = false;
            PlayBeepSFX();
            sb.AppendLine("<color=#EF4444><b>[SIGNAL CUTS OUT]</b></color>");
            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(true);
            }
        }

        /// <summary>
        /// Fast-forwards the radio transmission to full transcript if the player presses Space or clicks.
        /// </summary>
        public void FastForwardRadio()
        {
            if (!isRadioTransmitting) return;

            isRadioTransmitting = false;
            if (radioRoutine != null) StopCoroutine(radioRoutine);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#94A3B8>[STATIC HISS]</color>\n");
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            sb.AppendLine("<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing! They just keep coming!\"</i>\n");
            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");
            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");
            sb.AppendLine("<i>\"God damn it, get off the damn car! Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n");
            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");
            sb.AppendLine("<b><color=#F59E0B>DISPATCH (CALM):</color></b>");
            sb.AppendLine("<i>\"4-Charlie, unrestricted lethal force is authorized. All units, listen carefully. Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. <color=#EF4444>Aim for the head.</color> I repeat... <color=#EF4444>aim for the head.</color> Avoid all physical contact.\"</i>\n");
            sb.AppendLine("<color=#EF4444><b>[SIGNAL CUTS OUT]</b></color>");

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

            if (radioOutroObj != null)
            {
                radioOutroObj.SetActive(true);
            }
        }

        private void OnCloseRadioClicked()
        {
            PlayClickSFX();

            isRadioTransmitting = false;
            if (radioRoutine != null) StopCoroutine(radioRoutine);
            if (radioMeterRoutine != null) StopCoroutine(radioMeterRoutine);
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

            EnsureInspectionModalsEnhanced();

            if (hackingModal != null)
            {
                hackingModal.SetActive(true);
            }

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(false);
            }

            // Start laptop terminal decryption & LED activity routines
            isHackingTerminal = true;
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            hackingRoutine = StartCoroutine(HackingTerminalRoutine());

            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);
            hackingLedRoutine = StartCoroutine(HackingLedRoutine());
        }

        private IEnumerator HackingLedRoutine()
        {
            int tick = 0;
            while (isHackingTerminal && hackingModal != null && hackingModal.activeSelf)
            {
                if (hackingLedText != null)
                {
                    string hddColor = (tick % 2 == 0) ? "#f59e0b" : "#78350f";
                    string netColor = (tick % 3 == 0) ? "#38bdf8" : "#0284c7";
                    hackingLedText.text = $"<color=#10b981>● PWR</color>   <color={hddColor}>● HDD</color>   <color={netColor}>● NET</color>";
                }
                tick++;
                yield return new WaitForSecondsRealtime(0.12f);
            }

            if (hackingLedText != null)
            {
                hackingLedText.text = "<color=#10b981>● PWR</color>   <color=#78350f>● HDD</color>   <color=#ef4444>● NET [SEVERED]</color>";
            }
        }

        private IEnumerator HackingTerminalRoutine()
        {
            isHackingTerminal = true;

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(false);
            }

            string[] hackingTextLines = new string[]
            {
                "<color=#64748b>root@unit407:~# ./extract_defense_grid.sh --target=DISPATCH_SECURE_NODE_04</color>",
                "<color=#38bdf8>[*] ESTABLISHING ENCRYPTED TUNNEL VIA SEATTLE FIBER GRID...</color>",
                "<color=#10b981>[+] HANDSHAKE ACCEPTED: 154.790 MHz MILNET GATEWAY (IP: 10.14.88.2)</color>",
                "<color=#f59e0b>[*] BYPASSING 256-BIT AES DEFENSE PROTOCOL... [100% OK]</color>",
                "",
                "<color=#ef4444><b>================== [ TOP SECRET // CLASSIFIED DOD DIRECTIVE ] ==================</b></color>",
                "<color=#f1f5f9><b>OPERATION CODE :</b></color> <color=#38bdf8>WILDFIRE (EXTREME BIOLOGICAL CONTAINMENT)</color>",
                "<color=#f1f5f9><b>THREAT LEVEL   :</b></color> <color=#ef4444><b>CATEGORY 5 (EXTINCTION EVENT)</b></color>",
                "<color=#f1f5f9><b>PATHOGEN       :</b></color> <color=#f59e0b>NEURO-AGGRESSIVE MUTAGEN (PRIMARY VECTOR: HARBORVIEW LAB)</color>",
                "<color=#f1f5f9><b>STATUS         :</b></color> <color=#ef4444>QUARANTINE FAILED // INFECTION RATE: 87.4%</color>",
                "<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>INITIATE SECTOR 4 PURGE (THERMAL FIREBOMBING) AT 0600 HOURS.</b></color>",
                "<color=#f1f5f9><b>EXTRACTION LZ  :</b></color> <color=#10b981>POINT 'ECHO' // HARBORVIEW HELIPAD (47.604° N, 122.324° W)</color>",
                "<color=#ef4444><b>================================================================================</b></color>",
                "",
                "<color=#10b981>[+] DOWNLOADING ATTACHMENT: EVAC_PROTOCOLS.DAT (128 KB)... DONE.</color>",
                "<color=#ffd700><b>[+] GENERATING TIER-1 MILITARY CLEARANCE PASSKEY... SYNCED TO PHONE!</b></color>",
                "",
                "<color=#ef4444><b>[!] INTRUSION DETECTED BY CENTRAL DEFENSE NODE. KILL SWITCH ENGAGED!</b></color>",
                "<color=#94a3b8>[X] CONNECTION TERMINATED BY REMOTE HOST. LOCAL IP BURNED.</color>"
            };

            if (hackingTerminalText != null) hackingTerminalText.text = "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hackingTextLines.Length; i++)
            {
                if (!isHackingTerminal) yield break;
                sb.AppendLine(hackingTextLines[i]);
                if (hackingTerminalText != null)
                {
                    hackingTerminalText.text = sb.ToString();
                }
                PlayBeepSFX();
                yield return new WaitForSecondsRealtime(0.18f);
            }

            isHackingTerminal = false;
            PlayBeepSFX();
            yield return new WaitForSecondsRealtime(0.35f);

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(true);
            }
        }

        /// <summary>
        /// Fast-forwards the hacking terminal to the completed classified directive readout.
        /// </summary>
        public void FastForwardHacking()
        {
            if (!isHackingTerminal) return;

            isHackingTerminal = false;
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);

            if (hackingLedText != null)
            {
                hackingLedText.text = "<color=#10b981>● PWR</color>   <color=#78350f>● HDD</color>   <color=#ef4444>● NET [SEVERED]</color>";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#64748b>root@unit407:~# ./extract_defense_grid.sh --target=DISPATCH_SECURE_NODE_04</color>");
            sb.AppendLine("<color=#38bdf8>[*] ESTABLISHING ENCRYPTED TUNNEL VIA SEATTLE FIBER GRID...</color>");
            sb.AppendLine("<color=#10b981>[+] HANDSHAKE ACCEPTED: 154.790 MHz MILNET GATEWAY (IP: 10.14.88.2)</color>");
            sb.AppendLine("<color=#f59e0b>[*] BYPASSING 256-BIT AES DEFENSE PROTOCOL... [100% OK]</color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#ef4444><b>================== [ TOP SECRET // CLASSIFIED DOD DIRECTIVE ] ==================</b></color>");
            sb.AppendLine("<color=#f1f5f9><b>OPERATION CODE :</b></color> <color=#38bdf8>WILDFIRE (EXTREME BIOLOGICAL CONTAINMENT)</color>");
            sb.AppendLine("<color=#f1f5f9><b>THREAT LEVEL   :</b></color> <color=#ef4444><b>CATEGORY 5 (EXTINCTION EVENT)</b></color>");
            sb.AppendLine("<color=#f1f5f9><b>PATHOGEN       :</b></color> <color=#f59e0b>NEURO-AGGRESSIVE MUTAGEN (PRIMARY VECTOR: HARBORVIEW LAB)</color>");
            sb.AppendLine("<color=#f1f5f9><b>STATUS         :</b></color> <color=#ef4444>QUARANTINE FAILED // INFECTION RATE: 87.4%</color>");
            sb.AppendLine("<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>INITIATE SECTOR 4 PURGE (THERMAL FIREBOMBING) AT 0600 HOURS.</b></color>");
            sb.AppendLine("<color=#f1f5f9><b>EXTRACTION LZ  :</b></color> <color=#10b981>POINT 'ECHO' // HARBORVIEW HELIPAD (47.604° N, 122.324° W)</color>");
            sb.AppendLine("<color=#ef4444><b>================================================================================</b></color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#10b981>[+] DOWNLOADING ATTACHMENT: EVAC_PROTOCOLS.DAT (128 KB)... DONE.</color>");
            sb.AppendLine("<color=#ffd700><b>[+] GENERATING TIER-1 MILITARY CLEARANCE PASSKEY... SYNCED TO PHONE!</b></color>");
            sb.AppendLine("");
            sb.AppendLine("<color=#ef4444><b>[!] INTRUSION DETECTED BY CENTRAL DEFENSE NODE. KILL SWITCH ENGAGED!</b></color>");
            sb.AppendLine("<color=#94a3b8>[X] CONNECTION TERMINATED BY REMOTE HOST. LOCAL IP BURNED.</color>");

            if (hackingTerminalText != null) hackingTerminalText.text = sb.ToString();

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(true);
            }
        }

        private void OnCloseHackingClicked()
        {
            PlayClickSFX();

            isHackingTerminal = false;
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            if (hackingLedRoutine != null) StopCoroutine(hackingLedRoutine);

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

            // Atmospheric transition with sound effects, black screen fade, and narrative suspense
            StartCoroutine(DoorExitTransitionRoutine());
        }

        private bool isDoorTransitioning = false;
        private GameObject doorTransitionOverlay;
        private CanvasGroup doorTransitionCanvasGroup;
        private TextMeshProUGUI doorTransitionTitleTmp;
        private TextMeshProUGUI doorTransitionDescTmp;
        private GameObject doorTransitionContentBox;
        private UnityEngine.Video.VideoPlayer doorVideoPlayer;
        private RawImage doorVideoRawImage;
        private RenderTexture doorVideoRenderTexture;

        private void EnsureDoorTransitionOverlay()
        {
            if (doorTransitionOverlay != null) return;

            doorTransitionOverlay = new GameObject("DoorTransitionOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            doorTransitionOverlay.transform.SetParent(transform, false);
            doorTransitionOverlay.transform.SetAsLastSibling();

            RectTransform rt = doorTransitionOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = doorTransitionOverlay.GetComponent<Image>();
            img.color = new Color(0.015f, 0.02f, 0.035f, 1f); // Deep pitch black
            img.raycastTarget = true;

            doorTransitionCanvasGroup = doorTransitionOverlay.GetComponent<CanvasGroup>();
            doorTransitionCanvasGroup.alpha = 0f;

            // Resident Evil Classic Door Video Player (RawImage + RenderTexture)
            GameObject vidObj = new GameObject("DoorVideoDisplay", typeof(RectTransform), typeof(RawImage));
            vidObj.transform.SetParent(doorTransitionOverlay.transform, false);
            RectTransform vidRt = vidObj.GetComponent<RectTransform>();
            vidRt.anchorMin = Vector2.zero;
            vidRt.anchorMax = Vector2.one;
            vidRt.offsetMin = Vector2.zero;
            vidRt.offsetMax = Vector2.zero;

            doorVideoRawImage = vidObj.GetComponent<RawImage>();
            doorVideoRawImage.color = Color.black;
            doorVideoRawImage.raycastTarget = false;

            if (doorVideoRenderTexture == null)
            {
                doorVideoRenderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                doorVideoRenderTexture.Create();
            }
            doorVideoRawImage.texture = doorVideoRenderTexture;

            doorVideoPlayer = doorTransitionOverlay.AddComponent<UnityEngine.Video.VideoPlayer>();
            doorVideoPlayer.playOnAwake = false;
            doorVideoPlayer.isLooping = false;
            doorVideoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            doorVideoPlayer.targetTexture = doorVideoRenderTexture;
            doorVideoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;

            // Try loading VideoClip or file path
            var clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/room403_door_transition");
            if (clip != null)
            {
                doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                doorVideoPlayer.clip = clip;
            }
            else
            {
                string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");
                if (System.IO.File.Exists(filePath))
                {
                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                    doorVideoPlayer.url = filePath;
                }
            }

            // Centered content box (Narrative Subtitle / Fallback)
            doorTransitionContentBox = new GameObject("ContentBox", typeof(RectTransform));
            doorTransitionContentBox.transform.SetParent(doorTransitionOverlay.transform, false);
            RectTransform boxRt = doorTransitionContentBox.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.12f, 0.08f);
            boxRt.anchorMax = new Vector2(0.88f, 0.28f);
            boxRt.offsetMin = Vector2.zero;
            boxRt.offsetMax = Vector2.zero;

            // Title / Header
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(doorTransitionContentBox.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.55f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            doorTransitionTitleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            doorTransitionTitleTmp.fontSize = 17f;
            doorTransitionTitleTmp.fontStyle = FontStyles.Bold;
            doorTransitionTitleTmp.alignment = TextAlignmentOptions.Center;
            doorTransitionTitleTmp.color = new Color(0.0f, 0.92f, 1.0f); // Bright Cyan
            doorTransitionTitleTmp.text = "[ UNIT 407 // FRONT DEADBOLT ]";

            // Narrative description
            GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(doorTransitionContentBox.transform, false);
            RectTransform descRt = descObj.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 0.55f);
            descRt.offsetMin = Vector2.zero;
            descRt.offsetMax = Vector2.zero;

            doorTransitionDescTmp = descObj.GetComponent<TextMeshProUGUI>();
            doorTransitionDescTmp.fontSize = 14f;
            doorTransitionDescTmp.lineSpacing = 14f;
            doorTransitionDescTmp.alignment = TextAlignmentOptions.Center;
            doorTransitionDescTmp.color = new Color(0.88f, 0.90f, 0.94f);
            doorTransitionDescTmp.text = "You insert the key into the heavy deadbolt...";

            doorTransitionOverlay.SetActive(false);
        }

        private IEnumerator DoorExitTransitionRoutine()
        {
            isDoorTransitioning = true;
            EnsureDoorTransitionOverlay();
            doorTransitionOverlay.SetActive(true);
            doorTransitionOverlay.transform.SetAsLastSibling();
            doorTransitionCanvasGroup.alpha = 0f;

            // Refresh video source in case of dynamic file addition
            if (doorVideoPlayer != null)
            {
                if (doorVideoPlayer.clip == null && string.IsNullOrEmpty(doorVideoPlayer.url))
                {
                    string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");
                    if (System.IO.File.Exists(filePath))
                    {
                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                        doorVideoPlayer.url = filePath;
                    }
                }
                doorVideoPlayer.Prepare();
            }

            // Step 1: Smooth fade to black with key insertion SFX
            doorTransitionTitleTmp.text = "[ UNIT 407 // UNLOCKING FRONT DEADBOLT ]";
            doorTransitionDescTmp.text = "You slide the heavy brass key into the cylinder.\n<color=#94A3B8>The mechanical tumblers align with a sharp metallic click.</color>";
            if (doorTransitionPlayKeySFX)
            {
                if (doorTransitionKeySFX != null) PlaySFX(doorTransitionKeySFX, 0.9f);
                else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.9f);
            }

            float fadeTime = Mathf.Max(0.05f, doorTransitionFadeOutDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 1f;

            if (doorTransitionTextToVideoDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(doorTransitionTextToVideoDelay);
            }

            // Step 2: Play the Resident Evil Door Opening Video!
            bool playedVideo = false;
            if (doorVideoPlayer != null && (doorVideoPlayer.clip != null || !string.IsNullOrEmpty(doorVideoPlayer.url)))
            {
                // Wait for video prepare if needed (with 1.5s max timeout)
                float prepTimer = 0f;
                while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)
                {
                    prepTimer += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (doorVideoPlayer.isPrepared)
                {
                    playedVideo = true;
                    doorVideoRawImage.color = Color.white;
                    doorTransitionContentBox?.SetActive(false); // Clean cinematic presentation

                    if (doorTransitionPlayKnobSFX)
                    {
                        if (doorTransitionKnobSFX != null) PlaySFX(doorTransitionKnobSFX, 0.95f);
                        else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                    }
                    if (doorTransitionFadeAmbience && atmosphereSource != null && atmosphereSource.isPlaying)
                    {
                        StartCoroutine(FadeOutAudio(atmosphereSource, 0.8f));
                    }

                    doorVideoPlayer.Play();

                    // Track video playback
                    yield return new WaitForSecondsRealtime(0.3f);
                    float vidLength = (float)doorVideoPlayer.length;
                    if (vidLength <= 0.5f) vidLength = 4.5f;

                    // Play footsteps and door shut as the door swings open into darkness
                    float midTime = vidLength * 0.55f;
                    yield return new WaitForSecondsRealtime(midTime);
                    if (doorTransitionPlayFootstepSFX)
                    {
                        if (doorTransitionFootstepSFX != null) PlaySFX(doorTransitionFootstepSFX, 0.8f);
                        else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);
                    }

                    float remain = Mathf.Max(0.2f, vidLength - midTime - 0.4f);
                    yield return new WaitForSecondsRealtime(remain);
                    if (doorTransitionPlaySlamSFX)
                    {
                        if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.65f);
                        else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);
                    }

                    if (doorTransitionPostVideoPause > 0f)
                    {
                        yield return new WaitForSecondsRealtime(doorTransitionPostVideoPause);
                    }
                }
            }

            // Fallback narrative sequence if video could not play
            if (!playedVideo)
            {
                doorTransitionContentBox?.SetActive(true);
                doorTransitionTitleTmp.text = "[ UNIT 407 // DISENGAGING DEADBOLT ]";
                doorTransitionDescTmp.text = "<i>*CLACK-CHUNK*</i>\n\nThe heavy steel deadbolt disengages with a hollow metallic snap.\nYou grip the cold brass handle and ease the door open.";
                if (doorTransitionPlayKnobSFX)
                {
                    if (doorTransitionKnobSFX != null) PlaySFX(doorTransitionKnobSFX, 0.95f);
                    else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                }
                if (doorTransitionFadeAmbience && atmosphereSource != null && atmosphereSource.isPlaying)
                {
                    StartCoroutine(FadeOutAudio(atmosphereSource, 0.8f));
                }
                yield return new WaitForSecondsRealtime(2.0f);

                doorTransitionTitleTmp.text = "[ 4TH FLOOR CORRIDOR // THE THRESHOLD ]";
                doorTransitionDescTmp.text = "You take a breath of freezing, copper-scented air and step across the threshold.\nBehind you, the heavy door swings shut.\n\n<color=#EF4444><b>*CLICK*</b></color>\n<color=#94A3B8>The lock engages automatically. You are locked out into the hallway.</color>";
                if (doorTransitionPlayFootstepSFX)
                {
                    if (doorTransitionFootstepSFX != null) PlaySFX(doorTransitionFootstepSFX, 0.8f);
                    else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);
                }
                yield return new WaitForSecondsRealtime(1.0f);
                if (doorTransitionPlaySlamSFX)
                {
                    if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.65f);
                    else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);
                }
                yield return new WaitForSecondsRealtime(2.0f);
            }

            // Step 3: Switch to Hallway Mode behind the black screen
            EnterHallwayMode(1);

            // Step 4: Fade in from black to reveal the red 4th floor corridor
            fadeTime = Mathf.Max(0.05f, doorTransitionFadeInDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 0f;

            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
            if (doorVideoRawImage != null) doorVideoRawImage.color = Color.black;
            doorTransitionOverlay.SetActive(false);
            isDoorTransitioning = false;
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

        #region Hallway & Exploration Implementation
        public void EnterHallwayMode(int section = 1)
        {
            isInHallway = true;
            hallwaySection = Mathf.Clamp(section, 1, 4);
            currentInspectedRoom = null;

            // Stop living room animated backdrop
            if (livingRoomAnimPlayer != null)
            {
                livingRoomAnimPlayer.Stop();
                livingRoomAnimPlayer.enabled = false;
            }

            if (stageBackdropImage != null)
            {
                stageBackdropImage.enabled = true;
                stageBackdropImage.color = Color.white;
            }

            // Hide apartment buttons
            SetApartmentButtonsVisible(false);

            // Ensure situation box preferred height accommodates rich corridor narration
            if (situationBox != null)
            {
                LayoutElement sle = situationBox.GetComponent<LayoutElement>();
                if (sle != null) sle.preferredHeight = 110f;
            }

            // Ensure console is in narrative mode
            currentConsoleMode = ConsoleMode.Narrative;
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionFromApartmentToHallway();
            }

            UpdateHallwayView();
        }

        private void SetApartmentButtonsVisible(bool visible)
        {
            if (packGearButton != null) packGearButton.gameObject.SetActive(visible);
            if (searchKitchenButton != null) searchKitchenButton.gameObject.SetActive(visible);
            if (searchBathroomButton != null) searchBathroomButton.gameObject.SetActive(visible);
            if (searchClosetButton != null) searchClosetButton.gameObject.SetActive(visible);
            if (checkPhoneButton != null) checkPhoneButton.gameObject.SetActive(visible);
            if (listenRadioButton != null) listenRadioButton.gameObject.SetActive(visible);
            if (hackTerminalButton != null) hackTerminalButton.gameObject.SetActive(visible);
            if (leaveApartmentButton != null) leaveApartmentButton.gameObject.SetActive(visible);
            if (packedSuccessBanner != null) packedSuccessBanner.SetActive(visible && hasPacked);
        }

        private void ClearHallwayChoices()
        {
            for (int i = 0; i < hallwayChoiceButtons.Count; i++)
            {
                if (hallwayChoiceButtons[i] != null)
                {
                    hallwayChoiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private TacticalChoiceButton SetHallwayChoice(int index, string mainText, System.Action onClick, bool interactable = true, Color? accent = null)
        {
            TacticalChoiceButton btn = GetOrCreateHallwayChoiceButton(index);
            if (btn != null)
            {
                btn.SetChoiceNumber(index + 1);
                btn.SetChoice(mainText, onClick, interactable, accent);
            }
            return btn;
        }

        private TacticalChoiceButton GetOrCreateHallwayChoiceButton(int index)
        {
            if (index < hallwayChoiceButtons.Count && hallwayChoiceButtons[index] != null)
            {
                return hallwayChoiceButtons[index];
            }

            Transform parent = leaveApartmentButton != null ? leaveApartmentButton.transform.parent : null;
            if (parent == null && hubMenuContainer != null)
            {
                ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);
                if (sr != null && sr.content != null) parent = sr.content;
            }

            if (leaveApartmentButton != null && parent != null)
            {
                GameObject clone = Instantiate(leaveApartmentButton.gameObject, parent);
                clone.name = $"HallwayChoice_{index + 1}";
                TacticalChoiceButton tcb = clone.GetComponent<TacticalChoiceButton>();
                if (tcb != null)
                {
                    tcb.SetChoiceNumber(index + 1);
                    hallwayChoiceButtons.Add(tcb);
                    return tcb;
                }
            }
            return null;
        }

        public void UpdateHallwayView()
        {
            currentInspectedRoom = null;
            ClearHallwayChoices();
            PlayHubBGM("Audio/BGM/hallway_drone", 0.5f);

            int choiceIdx = 0;

            switch (hallwaySection)
            {
                case 1:
                    SetLocation("hallway_1", "CORRIDOR N // UPPER HALLWAY (SEC 1)");
                    tacticalMapController?.DiscoverAdjacentRooms("hallway_1");
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_406_407");

                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "The door to your apartment clicks shut behind you, sealing away the only safe place you have left.\n\n<color=#93C5FD><b>UPPER HALLWAY.</b></color> The emergency lights above are failing, casting long, twitching shadows across the cheap carpet. It's too quiet here, making every distant scream outside echo through the vents. To your right is your unit (407). To your left is <b>Unit 408</b>.";
                    }

                    SetHallwayChoice(choiceIdx++, "INVESTIGATE UNIT 408 <color=#9CA3AF>— The Professor's door. Electronic lock. Total silence inside.</color>", () => InspectUnit408());
                    SetHallwayChoice(choiceIdx++, "▼ ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Mid-Hallway.</color>", () => AdvanceDownHall());
                    break;

                case 2:
                    SetLocation("hallway_2", "CORRIDOR M // MID-HALLWAY (SEC 2)");
                    tacticalMapController?.DiscoverAdjacentRooms("hallway_2");
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_404_405");

                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#93C5FD><b>MID-HALLWAY.</b></color> Dust motes dance in the flickering light. You can hear a dog barking relentlessly from behind door 404, its claws scratching desperately against the wood. Across the hall, the reinforced door of <b>Unit 405</b> stands silent. The air here smells faintly of gunpowder.";
                    }

                    SetHallwayChoice(choiceIdx++, "INVESTIGATE UNIT 405 <color=#9CA3AF>— The Hunter's door. Heavy steel frame. Faint cordite.</color>", () => InspectUnit405());
                    SetHallwayChoice(choiceIdx++, "▲ RETREAT UP THE HALL <color=#9CA3AF>— Back toward Upper Hallway & Unit 407.</color>", () => RetreatUpHall());
                    SetHallwayChoice(choiceIdx++, "▼ ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Lower Hallway.</color>", () => AdvanceDownHall());
                    break;

                case 3:
                    SetLocation("hallway_3", "CORRIDOR S // LOWER HALLWAY (SEC 3)");
                    tacticalMapController?.DiscoverAdjacentRooms("hallway_3");
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_402_403");

                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#93C5FD><b>LOWER HALLWAY.</b></color> The lights are completely dead in this section. The metallic, coppery tang of blood mixes with the smell of ozone. Jarringly, soft classical music plays from behind the chained door of <b>Unit 402</b>. Across from it is <b>Unit 403</b>.";
                    }

                    SetHallwayChoice(choiceIdx++, "INVESTIGATE UNIT 402 <color=#9CA3AF>— The Andersons' door. Chained from the inside. Classical music.</color>", () => InspectUnit402());
                    SetHallwayChoice(choiceIdx++, "▲ RETREAT UP THE HALL <color=#9CA3AF>— Back toward Mid-Hallway.</color>", () => RetreatUpHall());
                    SetHallwayChoice(choiceIdx++, "▼ ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Hallway End.</color>", () => AdvanceDownHall());
                    break;

                case 4:
                    SetLocation("hallway_4", "CORRIDOR E // HALLWAY END (SEC 4)");
                    tacticalMapController?.DiscoverAdjacentRooms("hallway_4");
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_end");

                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#93C5FD><b>HALLWAY END.</b></color> The corridor terminates at the heavy fire doors leading to the stairwell. Bloody handprints smear the glass viewing pane. From <b>Unit 401</b>, loud, obnoxious pop music thumps through the wood, completely masking the sounds of the nightmare happening outside.";
                    }

                    SetHallwayChoice(choiceIdx++, "INVESTIGATE UNIT 401 <color=#9CA3AF>— The Party door. Slightly ajar. Music is loud.</color>", () => InspectUnit401());
                    SetHallwayChoice(choiceIdx++, "CHECK UTILITY CLOSET <color=#9CA3AF>— Maintenance room. The handle feels jammed.</color>", () => InspectUtilityCloset());
                    SetHallwayChoice(choiceIdx++, "<color=#EF4444>ENTER STAIRWELL (EXIT)</color> <color=#FCA5A5>— Point of no return. Push through bloody fire doors.</color>", () => InspectStairwell(), true, new Color(0.95f, 0.25f, 0.25f));
                    SetHallwayChoice(choiceIdx++, "▲ RETREAT UP THE HALL <color=#9CA3AF>— Back toward Lower Hallway.</color>", () => RetreatUpHall());
                    break;
            }

            ReindexActiveChoiceHotkeys();
        }

        private void AdvanceDownHall()
        {
            if (hallwaySection < 4)
            {
                hallwaySection++;
                gameClock?.AddTime(1);
                PlayClickSFX();
                UpdateHallwayView();
            }
        }

        private void RetreatUpHall()
        {
            if (hallwaySection > 1)
            {
                hallwaySection--;
                gameClock?.AddTime(1);
                PlayClickSFX();
                UpdateHallwayView();
            }
        }

        public void InspectUnit408()
        {
            currentInspectedRoom = "408";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (!room408_opened)
            {
                SetLocation("unit_408", "UNIT 408 // THE KEYPAD");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_406_407");
                PlaySFX("Audio/SFX/tick", 0.4f);

                string lockoutMsg = room408_locked_out 
                    ? "\n\n<color=#EF4444><b>⛔ PERMANENTLY LOCKED OUT.</b> The keypad triggered its security failsafe. The software is sealed. You will have to bypass it physically.</color>"
                    : (failed_408_attempts > 0 ? $"\n\n<color=#F59E0B>⚠️ WRONG CODE. Attempts remaining: {3 - failed_408_attempts}.</color>" : "");

                if (situationBoxText != null)
                {
                    situationBoxText.text = $"Professor Harmon taught history and behavioral sociology at the university. He talked about the psychology of collapsing civilizations as inevitable and fascinating.\n\nThe door is sealed with a heavy electronic lock. A small keypad glows faintly in the dark hallway, its LEDs casting a cold blue light. Four blank digits. Waiting.\n\nTaped above the scanner is a note: <i>\"If I'm not answering, I'm either dead or I finally took my own advice and ran. Either way, you're not getting in.\"</i>{lockoutMsg}";
                }

                if (!room408_locked_out)
                {
                    SetHallwayChoice(choiceIdx++, "📱 Enter the code from your phone (8445) <color=#9CA3AF>— You saw his text earlier.</color>", () =>
                    {
                        room408_opened = true;
                        gameClock?.AddTime(1);
                        PlayClickSFX();
                        InspectUnit408();
                    });

                    SetHallwayChoice(choiceIdx++, "🔢 Enter keypad code manually [8445] <color=#9CA3AF>— Four digits. 3 attempts limit.</color>", () =>
                    {
                        room408_opened = true;
                        gameClock?.AddTime(1);
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmith = currentPlayer != null && currentPlayer.HasTrait("LOCKSMITH");
                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;
                bool hasMultiTool = currentPlayer != null && currentPlayer.HasItem("Multi-Tool");
                int perVal = currentPlayer != null ? currentPlayer.per : 5;
                int strVal = currentPlayer != null ? currentPlayer.str : 5;

                if (isBurglar)
                {
                    SetHallwayChoice(choiceIdx++, "Override electronic latch [BURGLAR] <color=#F59E0B>(-7 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(2);
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                if (hasLocksmith)
                {
                    SetHallwayChoice(choiceIdx++, "Exploit mechanical failsafe [LOCKSMITH] <color=#F59E0B>(-7 Stamina, ~3 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(3);
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                if (hasTech2)
                {
                    SetHallwayChoice(choiceIdx++, "Bypass keypad circuit [TECH 2] <color=#F59E0B>(-7 Stamina, ~4 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(4);
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                if (hasMultiTool)
                {
                    SetHallwayChoice(choiceIdx++, "Disassemble the housing [MULTI-TOOL] <color=#F59E0B>(-7 Stamina, ~3 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(3);
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                if (perVal >= 6 && !room408_locked_out)
                {
                    SetHallwayChoice(choiceIdx++, "Read finger smudges [PER 6] <color=#10B981>(0 Stamina, instant)</color>", () =>
                    {
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }

                if (strVal >= 6)
                {
                    SetHallwayChoice(choiceIdx++, $"Smash keypad housing [STR {strVal}/6] <color=#F59E0B>(-15 Stamina, ~3 min)</color>", () =>
                    {
                        ExertStamina(15);
                        gameClock?.AddTime(3);
                        room408_opened = true;
                        PlayClickSFX();
                        InspectUnit408();
                    });
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 15, 25, 75);
                    SetHallwayChoice(choiceIdx++, $"Force keypad housing [STR {strVal}/6 — {chance}% Chance] <color=#EF4444>(-15 Stamina)</color>", () =>
                    {
                        ExertStamina(15);
                        gameClock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            room408_opened = true;
                            PlayClickSFX();
                            InspectUnit408();
                        }
                        else
                        {
                            PlayBeepSFX();
                            TakeDamage(5, "Blunt trauma to hands");
                            if (situationBoxText != null)
                            {
                                situationBoxText.text = "<color=#EF4444><b>BRUTE FORCE FAILED.</b></color> You battered the housing, but the internal deadbolt didn't budge. Your hands are bruised and bleeding.";
                            }
                        }
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Give up and return to the hallway", () => EnterHallwayMode(1));
            }
            else
            {
                SetLocation("unit_408", "UNIT 408 // PROFESSOR'S STUDY");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("unit408");

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The heavy door swings open into darkness. The smell reaches you first: old paper, dust, and peppermint. Drawers are pulled open. His heavy coat and hiking backpack are gone.\n\nFloor-to-ceiling shelves line all four walls. His reading glasses sit folded on an open book — <i>The Fall of Rome</i>, marked with a yellow sticky note:\n\n<i>\"Every empire ends. We're just living in the intermission. Take what you need.\"</i>";
                }

                if (!studied_408_mechanics)
                {
                    SetHallwayChoice(choiceIdx++, "📖 Skim 'Advanced Mechanics' notes <color=#10B981>[TECH +1]</color> <color=#9CA3AF>(~20 min)</color>", () =>
                    {
                        studied_408_mechanics = true;
                        gameClock?.AddTime(20);
                        if (currentPlayer != null) currentPlayer.tech += 1;
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit408();
                    });
                }

                if (!studied_408_anatomy)
                {
                    SetHallwayChoice(choiceIdx++, "📖 Skim 'Gray's Anatomy' notes <color=#10B981>[MEDICINE +1]</color> <color=#9CA3AF>(~20 min)</color>", () =>
                    {
                        studied_408_anatomy = true;
                        gameClock?.AddTime(20);
                        if (currentPlayer != null) currentPlayer.medicine += 1;
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit408();
                    });
                }

                if (!read_408_journal)
                {
                    SetHallwayChoice(choiceIdx++, "📓 Read his personal journal <color=#10B981>[PER +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>
                    {
                        read_408_journal = true;
                        gameClock?.AddTime(10);
                        if (currentPlayer != null) currentPlayer.per += 1;
                        loreSystem?.AddLore("Professor Harmon's Journal", "His handwriting degrades into a frantic scrawl toward the end. He documented unmarked FEMA transports operating at night and the CDC's total blackout protocols: 'The state lies because panic moves faster than infection. But cognitive dissonance is the real plague.'", gameClock?.GetTimeString() ?? "00:15");
                        AppendStoryLog(gameClock?.GetTimeString() ?? "00:15", "UNIT 408 // HARMON'S JOURNAL", "Read Professor Harmon's field notes on the collapse. Gained +1 Perception.");
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit408();
                    });
                }

                if (!looted_408_kitchen)
                {
                    SetHallwayChoice(choiceIdx++, "🍵 Search the abandoned kitchen <color=#9CA3AF>(Canned Food x1, Energy Bar x1, Bottled Water x2)</color>", () =>
                    {
                        looted_408_kitchen = true;
                        gameClock?.AddTime(5);
                        GiveOrDropItem("Canned Food", 1);
                        GiveOrDropItem("Energy Bar", 1);
                        GiveOrDropItem("Bottled Water", 2);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit408();
                    });
                }

                if (!looted_408)
                {
                    SetHallwayChoice(choiceIdx++, "🔍 Raid the desk drawers <color=#9CA3AF>(Encyclopedia x1, Flashlight x1)</color>", () =>
                    {
                        looted_408 = true;
                        gameClock?.AddTime(5);
                        GiveOrDropItem("Encyclopedia", 1);
                        GiveOrDropItem("Flashlight", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit408();
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave the study and return to the hallway", () => EnterHallwayMode(1));
            }

            ReindexActiveChoiceHotkeys();
        }

        public void InspectUnit405()
        {
            currentInspectedRoom = "405";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (!room405_opened)
            {
                SetLocation("unit_405", "UNIT 405 // THE BOOBY TRAP");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_404_405");
                PlayHubBGM("Audio/BGM/tension_drone", 0.4f);

                string trapStatus = room405_trap_triggered 
                    ? "\n\n<color=#EF4444><b>❌ TRAP TRIGGERED.</b> Your ears are ringing. The blank shell detonated, echoing down the floor. The wire is burned away.</color>" 
                    : "";

                if (situationBoxText != null)
                {
                    situationBoxText.text = $"This is Marcus's place. An ex-military survivalist who reinforced his walls. The door is unlocked, sitting ajar by a fraction of an inch. The sharp smell of cordite and gun oil leaks into the hallway.\n\nYou push the door gently. It stops with a faint metallic <i>*clink*</i>.\n\nA monofilament fishing line is tied from the doorknob to the pin of a flashbang grenade and a makeshift 12-gauge blank shell. Opening this normally will blow your eardrums out and alert every infected on this floor.{trapStatus}";
                }

                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmith = currentPlayer != null && currentPlayer.HasTrait("LOCKSMITH");
                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;
                bool hasMultiTool = currentPlayer != null && currentPlayer.HasItem("Multi-Tool");
                int perVal = currentPlayer != null ? currentPlayer.per : 5;

                if (isBurglar || hasLocksmith)
                {
                    SetHallwayChoice(choiceIdx++, "Block the firing pin [BURGLAR/LOCKSMITH] <color=#F59E0B>(-10 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(2);
                        room405_opened = true;
                        PlayClickSFX();
                        InspectUnit405();
                    });
                }

                if (hasTech2)
                {
                    SetHallwayChoice(choiceIdx++, "Release the tension spring [TECH 2] <color=#F59E0B>(-10 Stamina, ~3 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(3);
                        room405_opened = true;
                        PlayClickSFX();
                        InspectUnit405();
                    });
                }

                if (hasMultiTool)
                {
                    SetHallwayChoice(choiceIdx++, "Cut and hold the wire [MULTI-TOOL] <color=#F59E0B>(-10 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(2);
                        room405_opened = true;
                        PlayClickSFX();
                        InspectUnit405();
                    });
                }

                if (perVal >= 6)
                {
                    SetHallwayChoice(choiceIdx++, $"Squeeze through the gap [PER {perVal}/6] <color=#F59E0B>(-20 Stamina, ~4 min)</color>", () =>
                    {
                        ExertStamina(20);
                        gameClock?.AddTime(4);
                        room405_opened = true;
                        PlayClickSFX();
                        InspectUnit405();
                    });
                }
                else
                {
                    int chance = Mathf.Clamp(perVal * 15, 20, 75);
                    SetHallwayChoice(choiceIdx++, $"Squeeze through the gap [PER {perVal}/6 — {chance}% Chance] <color=#EF4444>(-20 Stamina)</color>", () =>
                    {
                        ExertStamina(20);
                        gameClock?.AddTime(4);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            room405_opened = true;
                            PlayClickSFX();
                            InspectUnit405();
                        }
                        else
                        {
                            room405_trap_triggered = true;
                            room405_opened = true;
                            PlaySFX("Audio/SFX/firedoor_slam", 0.8f);
                            TakeDamage(15, "12-gauge blank blast & flashbang detonation");
                            InspectUnit405();
                        }
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Step back and return to the hallway", () => EnterHallwayMode(2));
            }
            else
            {
                SetLocation("unit_405", "UNIT 405 // THE HUNTER'S SANCTUARY");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("unit405");

                if (situationBoxText != null)
                {
                    situationBoxText.text = "You step into Marcus's apartment. It looks like a staging ground. A heavy wooden reloading bench is dusted with gunpowder and brass shavings. In the corner, a massive steel gun safe sits wide open — completely stripped of rifles.\n\nOn the wall is a topographical map tracing a route avoiding highways to <i>'Oakhaven Cabin'</i>. Underneath is a HAM radio with a notepad on the mic. Near the safe, a small wooden toy car lies forgotten on the rug.";
                }

                if (!read_marcus_log)
                {
                    SetHallwayChoice(choiceIdx++, "📝 Read the notepad by the radio <color=#EF4444>[LORE]</color> <color=#9CA3AF>(~5 min)</color>", () =>
                    {
                        read_marcus_log = true;
                        gameClock?.AddTime(5);
                        loreSystem?.AddLore("Marcus's Final Log", "Diane... I emptied the safe. I rigged the door for looters. But on the stairs... one bit my arm. It burns. I won't bring this to Oakhaven. I won't bring this to you and the boys. I'm going to end it outside. I love you.", gameClock?.GetTimeString() ?? "00:20");
                        AppendStoryLog(gameClock?.GetTimeString() ?? "00:20", "UNIT 405 // MARCUS'S LOG", "Read Marcus's tragic goodbye letter. He was bitten on the stairwell and chose to die alone in the streets to protect his family at Oakhaven Cabin.");
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit405();
                    });
                }

                if (!looted_405_bench)
                {
                    SetHallwayChoice(choiceIdx++, "🔍 Sweep the reloading bench <color=#9CA3AF>(Pistol Ammo x12, Scrap Metal x3)</color>", () =>
                    {
                        looted_405_bench = true;
                        gameClock?.AddTime(5);
                        GiveOrDropItem("Pistol Ammo", 12);
                        GiveOrDropItem("Scrap Metal", 3);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit405();
                    });
                }

                if (!looted_405_safe)
                {
                    SetHallwayChoice(choiceIdx++, "🔪 Check the bottom of the gun safe <color=#9CA3AF>(Hunting Knife x1)</color>", () =>
                    {
                        looted_405_safe = true;
                        gameClock?.AddTime(3);
                        GiveOrDropItem("Hunting Knife", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit405();
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave the apartment and return to the hallway", () => EnterHallwayMode(2));
            }

            ReindexActiveChoiceHotkeys();
        }

        public void InspectUnit402()
        {
            currentInspectedRoom = "402";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (!room402_opened)
            {
                SetLocation("unit_402", "UNIT 402 // THE CHAIN");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_402_403");
                PlayHubBGM("Audio/BGM/chopin", 0.45f);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The Andersons. A middle-aged couple and their teenage daughter. The door is unlocked. You push it open two inches before it catches on a heavy steel chain lock.\n\nThrough the gap, the lights are on. A turntable spins in the corner playing Chopin — slow, mournful piano. And there's a sharp chemical smell: clinical bleach, like a hospital scrubbed too clean.";
                }

                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmithOrGhost = currentPlayer != null && (currentPlayer.HasTrait("LOCKSMITH") || currentPlayer.HasTrait("GHOST"));
                bool hasMultiTool = currentPlayer != null && currentPlayer.HasItem("Multi-Tool");
                bool hasTech1 = currentPlayer != null && currentPlayer.tech >= 1;
                bool hasKnife = currentPlayer != null && (currentPlayer.HasItem("Combat Knife") || currentPlayer.HasItem("Kitchen Knife") || currentPlayer.HasItem("Hunting Knife"));
                int agiVal = currentPlayer != null ? currentPlayer.agi : 5;
                int strVal = currentPlayer != null ? currentPlayer.str : 5;

                if (isBurglar)
                {
                    SetHallwayChoice(choiceIdx++, "Slip the chain lock [BURGLAR] <color=#F59E0B>(-5 Stamina, ~1 min, SILENT)</color>", () =>
                    {
                        ExertStamina(5);
                        gameClock?.AddTime(1);
                        room402_entry_method = "silent";
                        room402_opened = true;
                        PlayClickSFX();
                        InspectUnit402();
                    });
                }

                if (hasLocksmithOrGhost)
                {
                    SetHallwayChoice(choiceIdx++, "Work the gap [LOCKSMITH/GHOST] <color=#F59E0B>(-5 Stamina, ~2 min, SILENT)</color>", () =>
                    {
                        ExertStamina(5);
                        gameClock?.AddTime(2);
                        room402_entry_method = "silent";
                        room402_opened = true;
                        PlayClickSFX();
                        InspectUnit402();
                    });
                }

                if (hasTech1 && hasMultiTool)
                {
                    SetHallwayChoice(choiceIdx++, "Unscrew the mount [TECH 1 + TOOL] <color=#F59E0B>(-5 Stamina, ~9 min, SILENT)</color>", () =>
                    {
                        ExertStamina(5);
                        gameClock?.AddTime(9);
                        room402_entry_method = "silent";
                        room402_opened = true;
                        PlayClickSFX();
                        InspectUnit402();
                    });
                }

                if (agiVal >= 5 && hasKnife)
                {
                    SetHallwayChoice(choiceIdx++, "Slide the chain [AGI 5 + KNIFE] <color=#F59E0B>(-5 Stamina, ~4 min, SILENT)</color>", () =>
                    {
                        ExertStamina(5);
                        gameClock?.AddTime(4);
                        room402_entry_method = "silent";
                        room402_opened = true;
                        PlayClickSFX();
                        InspectUnit402();
                    });
                }

                if (strVal >= 4)
                {
                    SetHallwayChoice(choiceIdx++, $"Kick the door mount [STR {strVal}/4] <color=#EF4444>(-5 Stamina, ~1 min, LOUD ENTRY)</color>", () =>
                    {
                        ExertStamina(5);
                        gameClock?.AddTime(1);
                        room402_entry_method = "loud";
                        room402_opened = true;
                        PlayClickSFX();
                        InspectUnit402();
                    });
                }
                else
                {
                    SetHallwayChoice(choiceIdx++, "Throw your weight against door [70% Chance] <color=#EF4444>(-10 Stamina, LOUD)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= 70)
                        {
                            room402_entry_method = "loud";
                            room402_opened = true;
                            PlayClickSFX();
                            InspectUnit402();
                        }
                        else
                        {
                            PlayBeepSFX();
                            TakeDamage(5, "Shoulder slam against steel chain");
                            if (situationBoxText != null)
                            {
                                situationBoxText.text = "<color=#EF4444><b>BRUTE FORCE FAILED.</b></color> You slammed your shoulder into the door, but the steel chain held firm. You made a massive racket and bruised yourself for nothing.";
                            }
                        }
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave it and walk away", () => EnterHallwayMode(3));
            }
            else
            {
                SetLocation("unit_402", "UNIT 402 // THE QUIET ROOM");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("unit402");
                PlayHubBGM("Audio/BGM/chopin", 0.45f);

                string loudNotice = room402_entry_method == "loud" 
                    ? "<color=#EF4444><i>The chain bracket tore from the wall with a loud CRACK that echoed through the building. Somewhere below, something stirred.</i></color>\n\n" 
                    : "";

                if (situationBoxText != null)
                {
                    situationBoxText.text = $"{loudNotice}You step inside. The piano swells — Chopin, <i>Nocturne in E-flat major</i>. The apartment is immaculate. No struggle. No blood. Every surface is dusted.\n\nThe Anderson family is sitting at the dining table: Mr. and Mrs. Anderson, and their teenage daughter Emma. Three empty wine glasses sit before them with white powder residue.\n\nOn the center of the table is a note: they believed a broadcast claiming the military was gassing the building. They chose peace over suffocating in the dark. Misinformation killed them.\n\nA .38 revolver lies on the floor near Mr. Anderson's foot.";
                }

                if (!looted_402_weapon)
                {
                    SetHallwayChoice(choiceIdx++, "🔫 Take the revolver from the floor <color=#9CA3AF>(Revolver .38, .38 Ammo x5)</color>", () =>
                    {
                        looted_402_weapon = true;
                        gameClock?.AddTime(1);
                        GiveOrDropItem("Revolver .38", 1);
                        GiveOrDropItem(".38 Ammo", 5);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                if (!looted_402_kitchen)
                {
                    SetHallwayChoice(choiceIdx++, "🍽️ Search the kitchen pantry <color=#9CA3AF>(Canned Food x2, Bottled Water x1)</color>", () =>
                    {
                        looted_402_kitchen = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Canned Food", 2);
                        GiveOrDropItem("Bottled Water", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                if (!looted_402_bathroom)
                {
                    SetHallwayChoice(choiceIdx++, "💊 Check the medicine cabinet <color=#9CA3AF>(Painkillers x2, Antiseptic x1)</color>", () =>
                    {
                        looted_402_bathroom = true;
                        gameClock?.AddTime(1);
                        GiveOrDropItem("Painkillers", 2);
                        GiveOrDropItem("Antiseptic", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                if (!looted_402_ring)
                {
                    SetHallwayChoice(choiceIdx++, "💍 Take Mrs. Anderson's wedding ring <color=#EF4444>[HUMANITY −10]</color> <color=#9CA3AF>(Gold Ring x1)</color>", () =>
                    {
                        looted_402_ring = true;
                        gameClock?.AddTime(1);
                        if (currentPlayer != null) currentPlayer.humanity = Mathf.Max(0, currentPlayer.humanity - 10);
                        GiveOrDropItem("Gold Ring", 1);
                        PlayClickSFX();
                        UpdateVitalsUI();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                if (!took_402_photo)
                {
                    SetHallwayChoice(choiceIdx++, "📷 Take the family photo from the mantle <color=#9CA3AF>(Family Photo x1)</color>", () =>
                    {
                        took_402_photo = true;
                        gameClock?.AddTime(1);
                        GiveOrDropItem("Family Photo", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                if (!read_402_note)
                {
                    SetHallwayChoice(choiceIdx++, "🔍 Keep the suicide note <color=#10B981>[PER +1]</color> <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        read_402_note = true;
                        gameClock?.AddTime(1);
                        if (currentPlayer != null) currentPlayer.per += 1;
                        loreSystem?.AddLore("The Andersons' Note", "They killed themselves after hearing a fake radio broadcast claiming the military was gassing the building. Misinformation travels faster than the virus.", gameClock?.GetTimeString() ?? "00:25");
                        AppendStoryLog(gameClock?.GetTimeString() ?? "00:25", "UNIT 402 // ANDERSON SUICIDE NOTE", "Logged the Anderson suicide note. A grim reminder: fear kills just as fast as the infected.");
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit402();
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave them in peace — return to the hallway", () => EnterHallwayMode(3));
            }

            ReindexActiveChoiceHotkeys();
        }

        public void InspectUnit401()
        {
            currentInspectedRoom = "401";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (!shelf_lifted)
            {
                SetLocation("unit_401", "UNIT 401 // THE FALLEN SHELF");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_end");
                if (!turned_off_music) PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The door is wide open. Loud pop music blares from within: <i>'...and we danced all night to the best song ever...'</i>\n\nRed solo cups litter the floor. In the living room, a massive 300-pound oak entertainment center has toppled forward, crushing a young man flat to the floor. His outstretched fingers are loosely curled around a set of <b>Car Keys</b>.\n\nA stampede of panicked partiers crushed him as they scrambled for the door. The scratched CD keeps looping endlessly.";
                }

                bool isConstruction = currentPlayer != null && string.Equals(currentPlayer.job, "Construction Worker", StringComparison.OrdinalIgnoreCase);
                bool hasBreacherOrGrip = currentPlayer != null && (currentPlayer.HasTrait("BREACHER") || currentPlayer.HasTrait("IRON GRIP"));
                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;
                bool hasPryTool = currentPlayer != null && (currentPlayer.HasItem("Crowbar") || currentPlayer.HasItem("Tire Iron"));
                int strVal = currentPlayer != null ? currentPlayer.str : 5;

                if (isConstruction)
                {
                    SetHallwayChoice(choiceIdx++, "Leverage weak points [CONSTRUCTION] <color=#F59E0B>(-10 Stamina, ~1 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(1);
                        shelf_lifted = true;
                        currentPlayer?.AddItem("Car Keys", 1);
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }

                if (hasBreacherOrGrip)
                {
                    SetHallwayChoice(choiceIdx++, "Lift it clean [TRAIT] <color=#F59E0B>(-10 Stamina, ~1 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(1);
                        shelf_lifted = true;
                        currentPlayer?.AddItem("Car Keys", 1);
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }

                if (hasTech2)
                {
                    SetHallwayChoice(choiceIdx++, "Dismantle the joints [TECH 2] <color=#F59E0B>(-10 Stamina, ~4 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(4);
                        shelf_lifted = true;
                        currentPlayer?.AddItem("Car Keys", 1);
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }

                if (hasPryTool)
                {
                    SetHallwayChoice(choiceIdx++, "Pry it up with prying tool <color=#F59E0B>(-10 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(2);
                        shelf_lifted = true;
                        currentPlayer?.AddItem("Car Keys", 1);
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }

                if (strVal >= 6)
                {
                    SetHallwayChoice(choiceIdx++, $"Deadlift with raw strength [STR {strVal}/6] <color=#F59E0B>(-10 Stamina, ~1 min)</color>", () =>
                    {
                        ExertStamina(10);
                        gameClock?.AddTime(1);
                        shelf_lifted = true;
                        currentPlayer?.AddItem("Car Keys", 1);
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 15, 20, 75);
                    SetHallwayChoice(choiceIdx++, $"Throw body weight into it [STR {strVal}/6 — {chance}% Chance] <color=#EF4444>(-20 Stamina)</color>", () =>
                    {
                        ExertStamina(20);
                        gameClock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            shelf_lifted = true;
                            currentPlayer?.AddItem("Car Keys", 1);
                            PlayClickSFX();
                            InspectUnit401();
                        }
                        else
                        {
                            PlayBeepSFX();
                            TakeDamage(15, "Oak shelf collapse on back");
                            if (situationBoxText != null)
                            {
                                situationBoxText.text = "<color=#EF4444><b>LIFT FAILED.</b></color> The oak shelf slipped from your grip and slammed back down. You tore a muscle in your back.";
                            }
                        }
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave — the keys aren't worth the injury", () =>
                {
                    StopHubBGM();
                    EnterHallwayMode(4);
                });
            }
            else
            {
                SetLocation("unit_401", "UNIT 401 // THE PARTY THAT ENDED");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("unit401");
                if (!turned_off_music) PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The heavy oak shelf is off him. The <b>Car Keys</b> are safely in your pocket. The music loops endlessly from the player.\n\n<i>'...danced all night to the best song ever...'</i>\n\nThe apartment smells like beer, grief, and stale panic. You are the only living thing left here.";
                }

                if (!looted_401_kitchen)
                {
                    SetHallwayChoice(choiceIdx++, "🍕 Raid the kitchen <color=#9CA3AF>(Energy Bar x2, Bottled Water x1)</color>", () =>
                    {
                        looted_401_kitchen = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Energy Bar", 2);
                        GiveOrDropItem("Bottled Water", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit401();
                    });
                }

                if (!looted_401_bathroom)
                {
                    SetHallwayChoice(choiceIdx++, "🪥 Check medicine cabinet <color=#9CA3AF>(Painkillers x2, Bandage x1)</color>", () =>
                    {
                        looted_401_bathroom = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Painkillers", 2);
                        GiveOrDropItem("Bandage", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit401();
                    });
                }

                if (!looted_401_alcohol)
                {
                    SetHallwayChoice(choiceIdx++, "🍺 Take alcohol from counter <color=#9CA3AF>(Bottle of Vodka x1, Whiskey x1)</color>", () =>
                    {
                        looted_401_alcohol = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Bottle of Vodka", 1);
                        GiveOrDropItem("Whiskey", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit401();
                    });
                }

                if (!looted_401_pockets)
                {
                    SetHallwayChoice(choiceIdx++, "🔍 Go through his jacket pockets <color=#9CA3AF>(Lighter x1)</color>", () =>
                    {
                        looted_401_pockets = true;
                        gameClock?.AddTime(1);
                        GiveOrDropItem("Lighter", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUnit401();
                    });
                }

                if (!turned_off_music)
                {
                    SetHallwayChoice(choiceIdx++, "🔇 Turn off the blaring stereo music", () =>
                    {
                        turned_off_music = true;
                        gameClock?.AddTime(1);
                        StopHubBGM();
                        PlayClickSFX();
                        InspectUnit401();
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Leave the apartment and return to hallway", () =>
                {
                    StopHubBGM();
                    EnterHallwayMode(4);
                });
            }

            ReindexActiveChoiceHotkeys();
        }

        public void InspectUtilityCloset()
        {
            currentInspectedRoom = "utility";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (!utility_opened)
            {
                SetLocation("utility", "4TH FLOOR // UTILITY ROOM");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("hall_end");
                PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "At the end of the corridor, the carpet transitions to bare concrete. Ahead is a heavy, industrial steel door: <b>MAINTENANCE - AUTHORIZED PERSONNEL ONLY</b>.\n\nThe reinforced door has a heavy-duty magnetic swipe lock. Bleach and raw copper seep from underneath. Inside, a failing fluorescent light buzzes erratically.";
                }

                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;
                bool hasPryTool = currentPlayer != null && (currentPlayer.HasItem("Crowbar") || currentPlayer.HasItem("Tire Iron"));
                int strVal = currentPlayer != null ? currentPlayer.str : 5;

                if (isBurglar)
                {
                    SetHallwayChoice(choiceIdx++, "Pick manual override cylinder [BURGLAR] <color=#F59E0B>(-7 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(2);
                        utility_opened = true;
                        PlayClickSFX();
                        InspectUtilityCloset();
                    });
                }

                if (hasTech2)
                {
                    SetHallwayChoice(choiceIdx++, "Hotwire magnetic reader [TECH 2] <color=#F59E0B>(-7 Stamina, ~3 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(3);
                        utility_opened = true;
                        PlayClickSFX();
                        InspectUtilityCloset();
                    });
                }

                if (hasPryTool)
                {
                    SetHallwayChoice(choiceIdx++, "Pry steel door with tool <color=#F59E0B>(-7 Stamina, ~2 min)</color>", () =>
                    {
                        ExertStamina(7);
                        gameClock?.AddTime(2);
                        utility_opened = true;
                        PlayClickSFX();
                        InspectUtilityCloset();
                    });
                }

                if (strVal >= 7)
                {
                    SetHallwayChoice(choiceIdx++, $"Shoulder ram the steel door [STR {strVal}/7] <color=#F59E0B>(-15 Stamina, ~1 min)</color>", () =>
                    {
                        ExertStamina(15);
                        gameClock?.AddTime(1);
                        utility_opened = true;
                        PlayClickSFX();
                        InspectUtilityCloset();
                    });
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 12, 15, 65);
                    SetHallwayChoice(choiceIdx++, $"Shoulder ram steel door [STR {strVal}/7 — {chance}% Chance] <color=#EF4444>(-15 Stamina)</color>", () =>
                    {
                        ExertStamina(15);
                        gameClock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            utility_opened = true;
                            PlayClickSFX();
                            InspectUtilityCloset();
                        }
                        else
                        {
                            PlayBeepSFX();
                            TakeDamage(10, "Blunt impact against industrial steel door");
                            if (situationBoxText != null)
                            {
                                situationBoxText.text = "<color=#EF4444><b>BRUTE FORCE FAILED.</b></color> You threw your weight against the heavy steel, but the magnetic lock didn't flinch. Your shoulder is badly bruised.";
                            }
                        }
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Step back into the hallway", () =>
                {
                    StopHubBGM();
                    EnterHallwayMode(4);
                });
            }
            else
            {
                SetLocation("utility", "4TH FLOOR // UTILITY ROOM");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("utility_room");
                PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The heavy door gives way. The air is thick and stinging with industrial bleach. A single failing fluorescent tube flickers on bare concrete.\n\nHeavy metal racks line the room, packed with maintenance supplies. In the center, a yellow mop bucket is tipped over, a dark smear leading toward the back corner. A metal desk sits buried under blueprints and a logbook.";
                }

                if (!read_utility_log)
                {
                    SetHallwayChoice(choiceIdx++, "📓 Read the maintenance logbook <color=#10B981>[PER +1]</color> <color=#9CA3AF>(~2 min)</color>", () =>
                    {
                        read_utility_log = true;
                        gameClock?.AddTime(2);
                        if (currentPlayer != null) currentPlayer.per += 1;
                        loreSystem?.AddLore("Maintenance Log - 4th Floor", "Tenants in 402 complaining about a chemical smell again. Feeling feverish today. My joints are burning. Taking some aspirin and locking myself in here to sleep it off until shift ends.", gameClock?.GetTimeString() ?? "00:30");
                        AppendStoryLog(gameClock?.GetTimeString() ?? "00:30", "UTILITY ROOM // JANITOR'S LOG", "Read the janitor's final log. He locked himself in here after coming down with the burning fever.");
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUtilityCloset();
                    });
                }

                if (!looted_utility_bleach)
                {
                    SetHallwayChoice(choiceIdx++, "🧽 Search the tipped cleaning cart <color=#9CA3AF>(Bleach x1, Work Gloves x1)</color>", () =>
                    {
                        looted_utility_bleach = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Bleach", 1);
                        GiveOrDropItem("Work Gloves", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUtilityCloset();
                    });
                }

                if (!looted_utility_tape)
                {
                    SetHallwayChoice(choiceIdx++, "🧰 Scavenge metal shelves <color=#9CA3AF>(Duct Tape x1, Rope x1)</color>", () =>
                    {
                        looted_utility_tape = true;
                        gameClock?.AddTime(3);
                        GiveOrDropItem("Duct Tape", 1);
                        GiveOrDropItem("Rope", 1);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUtilityCloset();
                    });
                }

                if (!looted_utility_flashlight)
                {
                    SetHallwayChoice(choiceIdx++, "🔦 Check drawers of the metal desk <color=#9CA3AF>(Flashlight x1, Batteries x2)</color>", () =>
                    {
                        looted_utility_flashlight = true;
                        gameClock?.AddTime(2);
                        GiveOrDropItem("Flashlight", 1);
                        GiveOrDropItem("Batteries", 2);
                        PlayClickSFX();
                        RefreshAllPDAPanels();
                        InspectUtilityCloset();
                    });
                }

                SetHallwayChoice(choiceIdx++, "🚪 Return to the hallway", () =>
                {
                    StopHubBGM();
                    EnterHallwayMode(4);
                });
            }

            ReindexActiveChoiceHotkeys();
        }

        public void InspectStairwell()
        {
            currentInspectedRoom = "stairs";
            ClearHallwayChoices();
            int choiceIdx = 0;

            if (stair_phase == 1)
            {
                SetLocation("stairs", "4TH FLOOR // STAIRWELL DESCENT");
                tacticalMapController?.DiscoverAdjacentRooms("stairs");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("stairwell4F");
                PlaySFX("Audio/SFX/firedoor_slam", 0.9f);
                PlayHubBGM("Audio/SFX/generator_hum", 0.45f);

                objectiveSystem?.CompleteObjective("escape_building");
                objectiveSystem?.AddObjective("descend_stairs", "Descend through the stairwell to ground floor", ObjectiveStatus.Active);

                if (situationBoxText != null)
                {
                    situationBoxText.text = "The heavy fire door slams shut behind you, sealing off the 4th floor with a final, echoing <b>*BOOM*</b>.\n\nThe stairwell is bathed in the pulsing, dim red glow of emergency backup lights. It's hard to see the bottom of the steps. The silence is broken only by the distant, vibrating hum of a struggling generator.\n\nYou need to get down to the ground floor.";
                }

                bool hasFlashlight = currentPlayer != null && currentPlayer.HasItem("Flashlight");
                bool hasLighter = currentPlayer != null && currentPlayer.HasItem("Lighter");
                int agiVal = currentPlayer != null ? currentPlayer.agi : 5;

                if (hasFlashlight)
                {
                    SetHallwayChoice(choiceIdx++, "Use your Flashlight <color=#F59E0B>[GEAR]</color> <color=#9CA3AF>(Fast & safe, ~4 min)</color>", () =>
                    {
                        descentMethod = "flashlight";
                        gameClock?.AddTime(4);
                        stair_phase = 2;
                        PlayClickSFX();
                        InspectStairwell();
                    });
                }

                if (hasLighter)
                {
                    SetHallwayChoice(choiceIdx++, "Use your Lighter <color=#F59E0B>[GEAR]</color> <color=#9CA3AF>(Flickering flame, ~6 min)</color>", () =>
                    {
                        descentMethod = "lighter";
                        gameClock?.AddTime(6);
                        stair_phase = 2;
                        PlayClickSFX();
                        InspectStairwell();
                    });
                }

                SetHallwayChoice(choiceIdx++, "Move Slowly and Carefully in dark <color=#10B981>[SAFE]</color> <color=#9CA3AF>(Feel walls, ~10 min)</color>", () =>
                {
                    descentMethod = "slow";
                    gameClock?.AddTime(10);
                    stair_phase = 2;
                    PlayClickSFX();
                    InspectStairwell();
                });

                if (agiVal >= 5)
                {
                    SetHallwayChoice(choiceIdx++, $"Move Quickly down the steps [AGI {agiVal}/5] <color=#F59E0B>(Sprint down, ~1 min)</color>", () =>
                    {
                        descentMethod = "quick";
                        gameClock?.AddTime(1);
                        stair_phase = 2;
                        PlayClickSFX();
                        InspectStairwell();
                    });
                }
                else
                {
                    int chance = Mathf.Clamp(agiVal * 18, 20, 75);
                    SetHallwayChoice(choiceIdx++, $"Sprint down the steps [AGI {agiVal}/5 — {chance}% Chance] <color=#EF4444>(High risk stumble)</color>", () =>
                    {
                        gameClock?.AddTime(1);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            descentMethod = "quick";
                        }
                        else
                        {
                            descentMethod = "quick_fail";
                            TakeDamage(15, "Catastrophic stumble down concrete stairs");
                            ExertStamina(20);
                        }
                        stair_phase = 2;
                        PlayClickSFX();
                        InspectStairwell();
                    });
                }
            }
            else if (stair_phase == 2)
            {
                SetLocation("stairs", "STAIRWELL // THE DESCENT");
                if (descentMethod == "quick_fail")
                {
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("stairwell_tumble");
                }
                else if (descentMethod == "flashlight" || descentMethod == "lighter")
                {
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("stairwell_flashlight");
                }
                else
                {
                    if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("stairwell4F");
                }

                string descText = "";
                if (descentMethod == "quick")
                {
                    descText = "<color=#10B981><b>[AGILITY SUCCESS]</b></color> You take the stairs two at a time. In the flickering strobe light, your boot hovers over a slick dark patch on the concrete. Your reflexes kick in — you grab the iron handrail, vaulting over the mess and landing safely on the landing below.";
                }
                else if (descentMethod == "slow")
                {
                    descText = "<color=#10B981><b>[SAFE DESCENT]</b></color> You grip the cold handrail, taking it one agonizing step at a time. As the light flares, something wet and dark glistens on the steps. You press your back against the wall and carefully step wide, avoiding the puddle entirely.";
                }
                else if (descentMethod == "quick_fail")
                {
                    descText = "<color=#EF4444><b>[AGILITY FAILED - LOUD NOISE]</b></color> In the dark, your heel slides out on a thick slick puddle. You violently tumble forward into concrete and slam against the iron railing with a loud echoing <i>CLANG</i>.\n\nYour palms are coated in thick, sticky, arterial blood. Someone was dragged down these stairs recently.";
                }
                else
                {
                    descText = "<color=#10B981><b>[LIGHT SOURCE]</b></color> Your light sweeps down the stairs and catches a thick dark reflection. A pool of arterial blood coats the middle of the flight. Keeping the beam steady, you safely navigate around the mess.";
                }

                if (situationBoxText != null)
                {
                    situationBoxText.text = descText;
                }

                SetHallwayChoice(choiceIdx++, "Turn the corner to the next landing", () =>
                {
                    stair_phase = 3;
                    PlayClickSFX();
                    InspectStairwell();
                });
            }
            else if (stair_phase == 3)
            {
                SetLocation("stairs", "STAIRWELL // THE BARRICADE");
                if (stageBackdropImage != null) stageBackdropImage.sprite = GetAct1Sprite("stairwell_barricade");

                if (situationBoxText != null)
                {
                    situationBoxText.text = "You continue your descent, hoping to reach the ground floor. But halfway between the 2nd and 1st floors, the stairs simply end.\n\n<color=#F59E0B><b>The way down is completely blocked.</b></color>\n\nHeavy oak desks, folding chairs, and a massive glass-front vending machine are wedged together into an impassable wall of junk. The dented, buckling steel shows whatever was trying to come up was monstrously strong.";
                }

                if (!barricade_pushed)
                {
                    SetHallwayChoice(choiceIdx++, "Try to push the barricade out of the way <color=#EF4444>[RISK: NOISE]</color>", () =>
                    {
                        barricade_pushed = true;
                        gameClock?.AddTime(1);
                        PlaySFX("Audio/SFX/firedoor_slam", 0.8f);
                        if (situationBoxText != null)
                        {
                            situationBoxText.text = "<color=#EF4444><b>[FAILED - IMPASSABLE]</b></color> You throw your shoulder against the vending machine. The metal shrieks violently against concrete, echoing like a gunshot. The barricade doesn't budge an inch. But that shriek definitely woke something up below.";
                        }
                        InspectStairwell();
                    });
                }

                SetHallwayChoice(choiceIdx++, "Open the fire door to the 2nd Floor <color=#10B981>[DETOUR]</color>", () =>
                {
                    PlayClickSFX();
                    objectiveSystem?.CompleteObjective("descend_stairs");
                    objectiveSystem?.AddObjective("detour_2f", "Cross the 2nd Floor to find the East Wing stairs", ObjectiveStatus.Active);
                    AppendStoryLog(gameClock?.GetTimeString() ?? "00:40", "STAIRWELL // 2ND FLOOR DETOUR", "The 1st floor descent was completely barricaded with heavy office furniture. Forced to take the fire door into the 2nd Floor corridor.");
                    if (situationBoxText != null)
                    {
                        situationBoxText.text = "<color=#10B981><b>FIRE DOOR UNLATCHED.</b></color> You step through the heavy emergency door onto the 2nd Floor corridor. The air is cold and smells of wet plaster. Ahead, the corridor stretches into darkness.";
                    }
                    ClearHallwayChoices();
                    SetHallwayChoice(0, "Proceed into 2nd Floor Corridor...", () =>
                    {
                        EnterHallwayMode(1);
                    });
                    ReindexActiveChoiceHotkeys();
                });
            }

            ReindexActiveChoiceHotkeys();
        }

        private void ExertStamina(int amount)
        {
            if (currentPlayer != null)
            {
                currentPlayer.stamina = Mathf.Max(0, currentPlayer.stamina - amount);
                UpdateVitalsUI();
                RefreshAllPDAPanels();
            }
        }

        private void TakeDamage(int amount, string source)
        {
            if (currentPlayer != null)
            {
                currentPlayer.hp = Mathf.Max(0, currentPlayer.hp - amount);
                UpdateVitalsUI();
                RefreshAllPDAPanels();
                AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "00:00", "INJURY SUSTAINED",
                    $"Sustained {amount} trauma damage from {source}. Current Vital State: {currentPlayer.hp}/{currentPlayer.hpMax} HP.");
            }
        }

        private Sprite GetAct1Sprite(string spriteName)
        {
            Sprite sp = Resources.Load<Sprite>("Act1/" + spriteName);
            if (sp != null) return sp;

#if UNITY_EDITOR
            string path = "Assets/Resources/Act1/" + spriteName + ".png";
            sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null) return sp;
            path = "Assets/Sprites/Act1/" + spriteName + ".png";
            sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null) return sp;
#endif
            return null;
        }

        private void PlayHubBGM(string relPath, float volume = 0.5f, bool loop = true)
        {
            if (string.IsNullOrEmpty(relPath)) return;
            AudioClip clip = Resources.Load<AudioClip>(relPath);
            if (clip == null)
            {
#if UNITY_EDITOR
                string cleanPath = relPath.Replace("\\", "/");
                if (!cleanPath.StartsWith("Assets/")) cleanPath = "Assets/" + cleanPath;
                if (!cleanPath.EndsWith(".mp3") && !cleanPath.EndsWith(".wav") && !cleanPath.EndsWith(".ogg")) cleanPath += ".mp3";
                clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(cleanPath);
#endif
            }
            if (clip != null && atmosphereSource != null)
            {
                if (atmosphereSource.clip == clip && atmosphereSource.isPlaying) return;
                atmosphereSource.Stop();
                atmosphereSource.clip = clip;
                atmosphereSource.loop = loop;
                atmosphereSource.volume = volume;
                atmosphereSource.Play();
            }
        }

        public void PlayHubBGM(AudioClip clip, float volume = 0.5f, bool loop = true)
        {
            if (clip == null || atmosphereSource == null) return;
            if (atmosphereSource.clip == clip && atmosphereSource.isPlaying) return;
            atmosphereSource.Stop();
            atmosphereSource.clip = clip;
            atmosphereSource.loop = loop;
            atmosphereSource.volume = volume;
            atmosphereSource.Play();
        }

        private void StopHubBGM()
        {
            if (atmosphereSource != null && atmosphereSource.isPlaying)
            {
                atmosphereSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 0.8f)
        {
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip, volume);
            }
        }

        private void PlaySFX(string relPath, float volume = 0.8f)
        {
            if (string.IsNullOrEmpty(relPath)) return;
            AudioClip clip = Resources.Load<AudioClip>(relPath);
            if (clip == null)
            {
#if UNITY_EDITOR
                string cleanPath = relPath.Replace("\\", "/");
                if (!cleanPath.StartsWith("Assets/")) cleanPath = "Assets/" + cleanPath;
                if (!cleanPath.EndsWith(".mp3") && !cleanPath.EndsWith(".wav") && !cleanPath.EndsWith(".ogg")) cleanPath += ".mp3";
                clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(cleanPath);
#endif
            }
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip, volume);
            }
        }

        #region Modular Story Passage Runner (Twine / ScriptableObject Engine)
        public SundayLastBreath.Data.StoryPassageData CurrentPassage { get; private set; }

        /// <summary>
        /// Plays an interactive story scene / passage directly from a ScriptableObject.
        /// Automatically handles background graphics, music/ambience, entrance SFX,
        /// HUD/Map synchronization, and interactive choice buttons with skill checks & rewards.
        /// </summary>
        public void PlayStoryPassage(SundayLastBreath.Data.StoryPassageData passage)
        {
            if (passage == null) return;
            CurrentPassage = passage;

            // 1. Ensure Hub is in narrative corridor mode
            isInHallway = true;
            SetApartmentButtonsVisible(false);
            currentConsoleMode = ConsoleMode.Narrative;
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);

            // 2. Visual Backdrop
            if (!passage.keepPreviousBackdrop && passage.backdropSprite != null)
            {
                if (stageBackdropImage != null)
                {
                    stageBackdropImage.enabled = true;
                    stageBackdropImage.sprite = passage.backdropSprite;
                    stageBackdropImage.color = Color.white;
                }
            }

            // 3. Audio & Music Management
            switch (passage.musicAction)
            {
                case SundayLastBreath.Data.PassageMusicAction.StopMusic:
                    StopHubBGM();
                    break;
                case SundayLastBreath.Data.PassageMusicAction.PlayNew:
                    if (passage.backgroundMusicOrAmbience != null)
                    {
                        PlayHubBGM(passage.backgroundMusicOrAmbience, passage.musicVolume);
                    }
                    break;
                case SundayLastBreath.Data.PassageMusicAction.KeepPlaying:
                default:
                    // Keep playing whatever ambience is currently playing
                    break;
            }

            // 4. Entrance Sound Effect
            if (passage.entranceSFX != null)
            {
                PlaySFX(passage.entranceSFX, passage.sfxVolume);
            }

            // 5. Location & Map synchronization
            if (!string.IsNullOrEmpty(passage.associatedRoomId))
            {
                SetLocation(passage.associatedRoomId, passage.passageTitle);
                tacticalMapController?.DiscoverAdjacentRooms(passage.associatedRoomId);
            }

            // 6. Narrative text display
            if (situationBoxText != null)
            {
                situationBoxText.text = passage.storyText;
            }

            // 7. Choices & Branching (Twine Links)
            ClearHallwayChoices();

            if (passage.choices != null)
            {
                for (int i = 0; i < passage.choices.Count; i++)
                {
                    var choice = passage.choices[i];
                    int btnIndex = i;

                    string fullLabel = choice.choiceLabel;
                    if (!string.IsNullOrEmpty(choice.choiceSubtitle))
                    {
                        fullLabel += $" <color=#94A3B8>({choice.choiceSubtitle})</color>";
                    }

                    // Check requirement
                    bool meetsReq = true;
                    if (!string.IsNullOrEmpty(choice.requiredItem))
                    {
                        meetsReq = currentPlayer != null && currentPlayer.HasItem(choice.requiredItem);
                        if (!meetsReq)
                        {
                            var def = SundayLastBreath.Data.ItemDatabase.GetDefinition(choice.requiredItem);
                            string itemTitle = def != null ? def.displayName : choice.requiredItem;
                            fullLabel += $" <color=#EF4444>[Req: {itemTitle}]</color>";
                        }
                    }

                    Color? accentColor = choice.isBackButton ? new Color(0.6f, 0.65f, 0.7f) : (Color?)null;

                    SetHallwayChoice(btnIndex, fullLabel, () =>
                    {
                        ExecutePassageChoice(choice);
                    }, meetsReq, accentColor);
                }
            }
        }

        private void ExecutePassageChoice(SundayLastBreath.Data.PassageChoiceData choice)
        {
            if (choice == null) return;

            // Audio feedback
            if (choice.choiceSFX != null)
            {
                PlaySFX(choice.choiceSFX);
            }
            else
            {
                PlayClickSFX();
            }

            // Time & resource costs
            if (choice.timeMinutesCost > 0)
            {
                gameClock?.AddTime(choice.timeMinutesCost);
            }
            if (choice.staminaCost > 0)
            {
                ExertStamina(choice.staminaCost);
            }
            if (choice.healthCost > 0)
            {
                TakeDamage(choice.healthCost, "Hazard");
            }

            // Item consumption
            if (!string.IsNullOrEmpty(choice.consumesItem) && currentPlayer != null)
            {
                currentPlayer.RemoveItem(choice.consumesItem, 1);
                tacticalGearController?.RefreshAll();
            }

            // Item loot / award
            if (!string.IsNullOrEmpty(choice.givesItem))
            {
                GiveOrDropItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));
            }

            // Room / location transition
            if (!string.IsNullOrEmpty(choice.targetRoomId))
            {
                SetLocation(choice.targetRoomId);
            }

            // Skill check branching
            if (choice.skillCheckType != SundayLastBreath.Data.PassageSkillCheckType.None)
            {
                int playerBonus = GetSkillBonus(choice.skillCheckType);
                int d20Roll = UnityEngine.Random.Range(1, 21);
                bool success = (d20Roll + playerBonus) >= choice.skillDC;

                if (success && choice.successPassage != null)
                {
                    PlayStoryPassage(choice.successPassage);
                    return;
                }
                else if (!success && choice.failurePassage != null)
                {
                    PlayStoryPassage(choice.failurePassage);
                    return;
                }
            }

            // Direct target passage
            if (choice.targetPassage != null)
            {
                PlayStoryPassage(choice.targetPassage);
            }
        }

        private int GetSkillBonus(SundayLastBreath.Data.PassageSkillCheckType skillType)
        {
            if (currentPlayer == null) return 0;
            switch (skillType)
            {
                case SundayLastBreath.Data.PassageSkillCheckType.Tactics:
                    return currentPlayer.Tactics;
                case SundayLastBreath.Data.PassageSkillCheckType.Job:
                    return currentPlayer.per;
                case SundayLastBreath.Data.PassageSkillCheckType.Tech:
                    return currentPlayer.tech;
                case SundayLastBreath.Data.PassageSkillCheckType.Physical:
                    return currentPlayer.str;
                default:
                    return 0;
            }
        }
        #endregion
        #endregion
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

            // Sync room floor items & badges dynamically per location
            UpdateFloorItemsNotification();
            if (currentConsoleMode == ConsoleMode.Gear)
            {
                RefreshRightGearPanel();
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
