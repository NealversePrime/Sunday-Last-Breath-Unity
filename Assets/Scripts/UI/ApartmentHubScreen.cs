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

using UnityEngine.InputSystem;



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

        [SerializeField] private GameObject topHUDContainer;

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

        [SerializeField] private KeypadPuzzleModal keypadModal;

        [SerializeField] private QteTimingModal qteModal;

        [SerializeField] private LockpickingModal lockpickModal;

        [SerializeField] private TacticalCombatModal combatModal;

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

        [Tooltip("How long the key-unlocking text is displayed on screen before dissolving to the door video.")]

        [SerializeField] private float doorTransitionTextToVideoDelay = 1.5f;

        [Tooltip("Duration in seconds for the narrative text to smoothly dissolve before the video emerges.")]

        [SerializeField] private float doorTransitionTextFadeOutDuration = 0.45f;

        [Tooltip("Duration in seconds for the door video to smoothly fade in from darkness.")]

        [SerializeField] private float doorTransitionVideoFadeInDuration = 0.4f;

        [Tooltip("Pause duration after the video finishes before fading in to reveal the hallway.")]

        [SerializeField] private float doorTransitionPostVideoPause = 0.4f;

        [Tooltip("Duration of the smooth fade-in from black revealing the hallway.")]

        [SerializeField] private float doorTransitionFadeInDuration = 1.0f;



        [Header("Modular Passage Entry Point")]

        [Tooltip("If assigned, exiting the apartment will automatically begin this starting Story Passage! Leave empty to use legacy code.")]

        [SerializeField] private SundayLastBreath.Data.StoryPassageData hallwayStartingPassage;

        private SundayLastBreath.Data.StoryPassageData _unit407HubPassage;



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

        private readonly HashSet<string> initializedRoomFloorPassages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);



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

                // When examining from the hallway outside a closed door, the player's physical location remains the hallway

                if (currentInspectedRoom == "407") return $"hallway_{hallwaySection}";

                if (currentInspectedRoom == "406" && !room406_opened) return $"hallway_{hallwaySection}";

                if (currentInspectedRoom == "405" && !room405_opened) return $"hallway_{hallwaySection}";

                if (currentInspectedRoom == "404" || currentInspectedRoom == "403") return $"hallway_{hallwaySection}";

                if (currentInspectedRoom == "402" && !room402_opened) return $"hallway_{hallwaySection}";

                if (currentInspectedRoom == "utility" && !utility_opened) return $"hallway_{hallwaySection}";



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

            set

            {

                phoneVisited = value;

                if (value && currentPlayer != null) currentPlayer.SetFlag("checked_phone_harmon", true);

            }

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



        // Room 406 (Harmon)

        private bool room406_opened = false;

        private int failed_406_attempts = 0;

        private bool room406_locked_out = false;

        private bool studied_406_mechanics = false;

        private bool studied_406_anatomy = false;

        private bool read_406_journal = false;

        private bool looted_406_kitchen = false;

        private bool looted_406 = false;



        private bool room401_entered = false;



        // Room 405 (Marcus)

        private bool room405_opened = false;

        private bool room405_trap_triggered = false;

        private bool read_marcus_log = false;

        private bool looted_405_bench = false;

        private bool looted_405_safe = false;



        // Room 404 (Mrs. Gable)

        private bool spoke_to_mrs_gable = false;



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

        private bool barricade_inspected = false;

        private bool stair_3f_door_checked = false;

        private bool stair_listened_2f_door = false;

        private bool stair_noise_made = false;

        private bool stair_hum_deducted = false;

        private bool stair_entered_sfx_played = false;



        private readonly List<TacticalChoiceButton> hallwayChoiceButtons = new List<TacticalChoiceButton>();

        #endregion



        // Cached frames for player avatar animation

        private List<Sprite> cachedAvatarFrames;

        private Coroutine avatarAnimRoutine;



        private static ApartmentHubScreen _instance;
        public static ApartmentHubScreen Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<ApartmentHubScreen>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }



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
                var loreTabLabel = pdaTabLoreBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (loreTabLabel != null) loreTabLabel.text = "NOTES";
                pdaTabLoreBtn.onClick.RemoveAllListeners();
                pdaTabLoreBtn.onClick.AddListener(() => SwitchPDATab("LORE"));
            }
            if (pdaPanelLore != null)
            {
                Transform notesBadgeText = pdaPanelLore.transform.Find("SectionBadge/Text");
                if (notesBadgeText != null && notesBadgeText.TryGetComponent<TextMeshProUGUI>(out var notesBadgeTmp))
                {
                    notesBadgeTmp.text = "NOTES & CLUES";
                }
            }



            // Initialize default PDA tab state to STATUS

            SwitchPDATab("STATUS", playSound: false);



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

                vlgRoot.spacing = 12f;

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

                if (sle.preferredHeight <= 0f) sle.preferredHeight = 64f;

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

                ple.minHeight = 26f;

                ple.flexibleHeight = 0f;

                ple.flexibleWidth = 1f;

            }



            // Ensure ChoiceSubHeader layout element and text padding

            Transform subHeadTr = hubMenuContainer.transform.Find("ChoiceSubHeader");

            if (subHeadTr != null)

            {

                LayoutElement shLe = subHeadTr.GetComponent<LayoutElement>() ?? subHeadTr.gameObject.AddComponent<LayoutElement>();

                shLe.preferredHeight = 22f;

                shLe.minHeight = 22f;

                shLe.flexibleHeight = 0f;

                shLe.flexibleWidth = 1f;



                var shTmp = subHeadTr.GetComponentInChildren<TextMeshProUGUI>();

                if (shTmp != null)

                {

                    shTmp.rectTransform.offsetMin = new Vector2(6f, 2f);

                    shTmp.rectTransform.offsetMax = new Vector2(-6f, -2f);

                }

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

                scLe.minHeight = 80f;

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

                scLe.minHeight = 80f;

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

                LayoutElement shLe = sh.GetComponent<LayoutElement>() ?? sh.gameObject.AddComponent<LayoutElement>();

                shLe.preferredHeight = 22f;

                shLe.minHeight = 22f;

                shLe.flexibleHeight = 0f;

                shLe.flexibleWidth = 1f;



                var shTmp = sh.GetComponentInChildren<TextMeshProUGUI>();

                if (shTmp != null)

                {

                    shTmp.text = "<b><color=#F5A623>// WHAT SHOULD I DO NEXT?</color></b>  <color=#64748B>[KEYS: 1–8 OR CLICK]</color>";

                    shTmp.rectTransform.offsetMin = new Vector2(6f, 2f);

                    shTmp.rectTransform.offsetMax = new Vector2(-6f, -2f);

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

                    dRect.sizeDelta = new Vector2(470f, 800f);

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



                    // Dossier Divider (1px subtle horizontal rule)

                    Transform divTr = dossierTr.Find("DossierDivider");

                    if (divTr == null)

                    {

                        GameObject divObj = new GameObject("DossierDivider", typeof(RectTransform));

                        divObj.transform.SetParent(dossierTr, false);

                        divTr = divObj.transform;

                    }

                    divTr.SetSiblingIndex(2);

                    RectTransform divRt = divTr.GetComponent<RectTransform>();

                    if (divRt != null)

                    {

                        divRt.sizeDelta = new Vector2(0f, 1f);

                    }

                    LayoutElement divLe = divTr.GetComponent<LayoutElement>();

                    if (divLe == null) divLe = divTr.gameObject.AddComponent<LayoutElement>();

                    divLe.preferredHeight = 1f;

                    divLe.minHeight = 1f;

                    divLe.flexibleHeight = 0f;

                    Image divImg = divTr.GetComponent<Image>();

                    if (divImg == null) divImg = divTr.gameObject.AddComponent<Image>();

                    divImg.color = new Color(0.20f, 0.26f, 0.36f, 0.75f);



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



                    // 2. RIGHT WING: SLEEK OBLONG SMARTPHONE CHASSIS (410 x 800 px, 19.5:9 Flagship Ratio)

                    Transform chassis = centerCol.Find("PhoneChassis");

                    if (chassis != null)

                    {

                        RectTransform cr = chassis.GetComponent<RectTransform>();

                        cr.anchorMin = new Vector2(1f, 0.5f);

                        cr.anchorMax = new Vector2(1f, 0.5f);

                        cr.pivot = new Vector2(1f, 0.5f);

                        cr.anchoredPosition = new Vector2(0f, 0f);

                        cr.sizeDelta = new Vector2(410f, 800f);



                        LayoutElement cle = chassis.GetComponent<LayoutElement>();

                        if (cle != null)

                        {

                            cle.preferredWidth = 410f;

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

                            radioFreqHeaderText.text = "FREQ: 154.790 MHz                                    <color=#ef4444>[REC]</color>";

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



            // C. HACKING TERMINAL MODAL ENHANCEMENT (Desktop Workstation & Intranet Terminal)

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

                        pRect.sizeDelta = new Vector2(980f, 720f);

                    }



                    Image pBg = hackPanel.GetComponent<Image>();

                    if (pBg != null)

                    {

                        pBg.color = new Color(0.055f, 0.065f, 0.08f, 1f); // Dark matte workstation chassis

                    }



                    Outline pOl = hackPanel.GetComponent<Outline>() ?? hackPanel.gameObject.AddComponent<Outline>();

                    pOl.effectColor = new Color(0.24f, 0.32f, 0.42f, 0.95f);

                    pOl.effectDistance = new Vector2(2f, -2f);



                    VerticalLayoutGroup vlg = hackPanel.GetComponent<VerticalLayoutGroup>() ?? hackPanel.gameObject.AddComponent<VerticalLayoutGroup>();

                    vlg.padding = new RectOffset(16, 16, 12, 14);

                    vlg.spacing = 10f;

                    vlg.childControlWidth = true;

                    vlg.childControlHeight = true;

                    vlg.childForceExpandWidth = true;

                    vlg.childForceExpandHeight = false;



                    // 1. Top Bezel Strip with LEDs and Model Designation

                    Transform topBezelTr = hackPanel.Find("LaptopTopBezel");

                    if (topBezelTr == null)

                    {

                        GameObject topBezelObj = new GameObject("LaptopTopBezel", typeof(RectTransform));

                        topBezelObj.transform.SetParent(hackPanel, false);

                        topBezelObj.transform.SetSiblingIndex(0);



                        LayoutElement tbLe = topBezelObj.AddComponent<LayoutElement>();

                        tbLe.preferredHeight = 24f;

                        tbLe.minHeight = 24f;



                        // Left LEDs

                        GameObject ledObj = new GameObject("Leds", typeof(RectTransform));

                        ledObj.transform.SetParent(topBezelObj.transform, false);

                        RectTransform ledRt = ledObj.GetComponent<RectTransform>();

                        ledRt.anchorMin = new Vector2(0f, 0f);

                        ledRt.anchorMax = new Vector2(0.35f, 1f);

                        ledRt.offsetMin = Vector2.zero;

                        ledRt.offsetMax = Vector2.zero;

                        hackingLedText = ledObj.AddComponent<TextMeshProUGUI>();

                        hackingLedText.fontSize = 10.5f;

                        hackingLedText.alignment = TextAlignmentOptions.MidlineLeft;

                        hackingLedText.text = "<color=#10b981>[PWR]</color>   <color=#f59e0b>[HDD]</color>   <color=#38bdf8>[NET]</color>";

                        SundayTypography.ApplyFont(hackingLedText, FontRole.Mono);



                        // Center Status

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

                        camTmp.text = "LOCAL INTRANET // SUBNET 192.168.1.1";

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

                        modTmp.text = "DESKTOP WORKSTATION // UNIT 407";

                        SundayTypography.ApplyFont(modTmp, FontRole.Mono);

                    }

                    else

                    {

                        hackingLedText = topBezelTr.Find("Leds")?.GetComponent<TextMeshProUGUI>();

                        TextMeshProUGUI modTmp = topBezelTr.Find("ModelBadge")?.GetComponent<TextMeshProUGUI>();

                        if (modTmp != null) modTmp.text = "DESKTOP WORKSTATION // UNIT 407";

                        TextMeshProUGUI camTmp = topBezelTr.Find("WebCam")?.GetComponent<TextMeshProUGUI>();

                        if (camTmp != null) camTmp.text = "LOCAL INTRANET // SUBNET 192.168.1.1";

                    }



                    // 2. Laptop Screen Container (Inner Display)

                    Transform screenTr = hackPanel.Find("LaptopScreen");

                    if (screenTr == null)

                    {

                        GameObject screenObj = new GameObject("LaptopScreen", typeof(RectTransform));

                        screenObj.transform.SetParent(hackPanel, false);

                        screenObj.transform.SetSiblingIndex(1);

                        screenTr = screenObj.transform;



                        Image sBg = screenObj.AddComponent<Image>();

                        sBg.color = new Color(0.012f, 0.018f, 0.026f, 1f); // Deep matte CRT display



                        Outline sOl = screenObj.AddComponent<Outline>();

                        sOl.effectColor = new Color(0.12f, 0.16f, 0.24f, 1f);

                    }



                    // Ensure Screen LayoutElement & VerticalLayoutGroup are configured properly

                    LayoutElement sLe = screenTr.GetComponent<LayoutElement>() ?? screenTr.gameObject.AddComponent<LayoutElement>();

                    sLe.preferredHeight = 490f;

                    sLe.minHeight = 460f;

                    sLe.flexibleHeight = 1f;



                    VerticalLayoutGroup sVlg = screenTr.GetComponent<VerticalLayoutGroup>() ?? screenTr.gameObject.AddComponent<VerticalLayoutGroup>();

                    sVlg.padding = new RectOffset(16, 16, 10, 12);

                    sVlg.spacing = 8f;

                    sVlg.childControlWidth = true;

                    sVlg.childControlHeight = true;

                    sVlg.childForceExpandWidth = true;

                    sVlg.childForceExpandHeight = false;



                    // 3. Header Bar inside LaptopScreen

                    Transform hTr = screenTr.Find("Header") ?? hackPanel.Find("Header");

                    if (hTr != null)

                    {

                        hTr.SetParent(screenTr, false);

                        hTr.SetAsFirstSibling();



                        Image hBg = hTr.GetComponent<Image>() ?? hTr.gameObject.AddComponent<Image>();

                        hBg.color = new Color(0.06f, 0.09f, 0.16f, 0.95f);



                        LayoutElement hLe = hTr.GetComponent<LayoutElement>() ?? hTr.gameObject.AddComponent<LayoutElement>();

                        hLe.preferredHeight = 26f;

                        hLe.minHeight = 26f;



                        TextMeshProUGUI hTmp = hTr.GetComponentInChildren<TextMeshProUGUI>(true);

                        if (hTmp != null)

                        {

                            hTmp.fontSize = 11f;

                            hTmp.fontStyle = FontStyles.Bold;

                            hTmp.text = "<b><color=#38bdf8>[>]</color> terminal@unit407: ~/network/emergency_intercept.log</b>        <color=#64748b>[SUBNET: 192.168.1.1]</color>   <color=#10b981>[LINK: ACTIVE]</color>";

                            SundayTypography.ApplyFont(hTmp, FontRole.Mono);

                        }

                    }



                    // 4. Terminal Text inside LaptopScreen

                    Transform tTr = screenTr.Find("TerminalText") ?? hackPanel.Find("TerminalText");

                    if (tTr != null)

                    {

                        tTr.SetParent(screenTr, false);

                        tTr.SetSiblingIndex(1);



                        hackingTerminalText = tTr.GetComponent<TextMeshProUGUI>();

                        if (hackingTerminalText != null)

                        {

                            hackingTerminalText.fontSize = 12f;

                            hackingTerminalText.lineSpacing = 6f;

                            hackingTerminalText.color = new Color(0.20f, 0.90f, 0.45f); // Phosphor green

                            SundayTypography.ApplyFont(hackingTerminalText, FontRole.Mono);

                        }



                        LayoutElement tLe = tTr.GetComponent<LayoutElement>() ?? tTr.gameObject.AddComponent<LayoutElement>();

                        tLe.preferredHeight = 440f;

                        tLe.minHeight = 420f;

                        tLe.flexibleHeight = 1f;

                    }



                    // 5. CRITICAL FIX: Reparent FinishContainer to hackPanel BELOW LaptopScreen!

                    // This guarantees the narrative outro and exit button NEVER overlap the terminal log!

                    Transform fTr = screenTr.Find("FinishContainer") ?? hackPanel.Find("FinishContainer");

                    if (fTr != null)

                    {

                        fTr.SetParent(hackPanel, false);

                        fTr.SetAsLastSibling();



                        RectTransform fRt = fTr.GetComponent<RectTransform>();

                        if (fRt != null)

                        {

                            fRt.anchorMin = new Vector2(0f, 0f);

                            fRt.anchorMax = new Vector2(1f, 0f);

                            fRt.pivot = new Vector2(0.5f, 0f);

                            fRt.anchoredPosition = Vector2.zero;

                        }



                        LayoutElement fLe = fTr.GetComponent<LayoutElement>() ?? fTr.gameObject.AddComponent<LayoutElement>();

                        fLe.preferredHeight = 100f;

                        fLe.minHeight = 90f;

                        fLe.flexibleWidth = 1f;



                        VerticalLayoutGroup fVlg = fTr.GetComponent<VerticalLayoutGroup>() ?? fTr.gameObject.AddComponent<VerticalLayoutGroup>();

                        fVlg.padding = new RectOffset(0, 0, 4, 4);

                        fVlg.spacing = 8f;

                        fVlg.childControlWidth = true;

                        fVlg.childControlHeight = true;

                        fVlg.childForceExpandWidth = true;

                        fVlg.childForceExpandHeight = false;



                        hackingFinishContainer = fTr.gameObject;



                        Transform oTr = fTr.Find("Outro");

                        if (oTr != null)

                        {

                            TextMeshProUGUI foTmp = oTr.GetComponent<TextMeshProUGUI>();

                            if (foTmp != null)

                            {

                                foTmp.fontSize = 12.5f;

                                foTmp.lineSpacing = 4f;

                                foTmp.color = new Color(0.92f, 0.94f, 0.98f);

                                foTmp.text = "<color=#ef4444><b>'BRIDGES & FREEWAY ON-RAMPS CLOSING BY 0100.'</b></color> If the main roads lock down at 1 AM, waiting for morning means getting boxed in. You save the cached <b><color=#ffd700>Tier-1 Military Clearance Passkey</color></b> to your phone before the router goes dead.";

                                SundayTypography.ApplyFont(foTmp, FontRole.Body);

                            }



                            LayoutElement oLe = oTr.GetComponent<LayoutElement>() ?? oTr.gameObject.AddComponent<LayoutElement>();

                            oLe.preferredHeight = 40f;

                            oLe.minHeight = 36f;

                        }

                    }



                    // Hide redundant deck strip if present so it doesn't take up space

                    Transform deckTr = hackPanel.Find("LaptopDeckStrip");

                    if (deckTr != null)

                    {

                        deckTr.gameObject.SetActive(false);

                    }

                }



                if (closeHackingButton != null)

                {

                    LayoutElement le = closeHackingButton.GetComponent<LayoutElement>() ?? closeHackingButton.gameObject.AddComponent<LayoutElement>();

                    le.preferredHeight = 46f;

                    le.minHeight = 46f;

                    le.flexibleWidth = 1f;



                    Image bg = closeHackingButton.GetComponent<Image>();

                    if (bg != null) bg.color = new Color(0.04f, 0.14f, 0.08f, 0.98f);



                    Outline ol = closeHackingButton.GetComponent<Outline>() ?? closeHackingButton.gameObject.AddComponent<Outline>();

                    ol.effectColor = new Color(0.25f, 0.95f, 0.50f, 0.95f);



                    SetChoiceButtonContent(closeHackingButton,

                        "<b>[ 1 ] WIPE DRIVE & CLOSE LAPTOP</b>   <color=#34D399><b>[ESC / SPACE]</b></color>");

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

            TacticalChoiceButton tcb = btn.GetComponent<TacticalChoiceButton>();

            if (tcb != null)

            {

                tcb.SyncContentAndState(singleFullText);

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



        private string GetUnit407Anchor()

        {

            if (hasPacked)

            {

                return "Blue and red lights sweep across your living room curtains from the street below. Your apartment key is in your pocket, and the front door is ready to unlock.";

            }

            return "Blue and red lights sweep across your living room curtains from the street below. The front deadbolt is still locked. Your keys, wallet, and everyday gear are sitting on the coffee table.";

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



            // Reset Lore and Story Logs for new game session

            if (loreSystem != null)

            {

                loreSystem.Clear();

                RefreshPDALoreTab();

            }

            _storyLogs.Clear();

            _visitedLocationAnchors.Clear();
            _returnVariantIndices.Clear();

            lastAutoLoggedSituationText = "";

            InitializeStoryLogs();

            RefreshStoryLogUI();



            // Reset Hallway & Exploration World State for fresh playthrough

            isInHallway = false;

            hallwaySection = 1;

            currentInspectedRoom = null;

            ClearHallwayChoices();

            PlayStageAnimation("Unit407Inside_Anim", "unit407", 8f);

            room406_opened = false;

            failed_406_attempts = 0;

            room406_locked_out = false;

            if (currentPlayer != null)

            {

                currentPlayer.SetFlag("checked_phone_harmon", false);

                currentPlayer.SetFlag("room406_opened", false);

                currentPlayer.SetFlag("room406_locked_out", false);

            }

            studied_406_mechanics = false;

            studied_406_anatomy = false;

            read_406_journal = false;

            looted_406_kitchen = false;

            looted_406 = false;

            room405_opened = false;

            room405_trap_triggered = false;

            read_marcus_log = false;

            looted_405_bench = false;

            looted_405_safe = false;

            spoke_to_mrs_gable = false;

            room402_opened = false;

            room402_entry_method = "";

            looted_402_weapon = false;

            looted_402_kitchen = false;

            looted_402_bathroom = false;

            looted_402_ring = false;

            took_402_photo = false;

            read_402_note = false;

            room401_entered = false;

            shelf_lifted = false;

            turned_off_music = false;

            looted_401_kitchen = false;

            looted_401_bathroom = false;

            looted_401_alcohol = false;

            looted_401_pockets = false;

            utility_opened = false;

            read_utility_log = false;

            looted_utility_bleach = false;

            looted_utility_tape = false;

            looted_utility_flashlight = false;

            stair_phase = 1;

            descentMethod = "";

            barricade_pushed = false;

            barricade_inspected = false;

            stair_3f_door_checked = false;

            stair_listened_2f_door = false;

            stair_noise_made = false;

            stair_hum_deducted = false;

            stair_entered_sfx_played = false;



            SetSituationNarration(GetUnit407Anchor(), recordInLog: false);



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



            // Restore awakening or hub sequence state (Beat 1, Beat 2, Beat 3, Hub Choices, or Hallway Exploration)

            if (isInHallway)

            {

                introSeen = true;

                awakeningStep = 4;

                RestoreHallwayExploration();

            }

            else if (hasPacked || introSeen || awakeningStep > 3)

            {

                introSeen = true;

                awakeningStep = 4;

                if (awakeningContainer != null) awakeningContainer.SetActive(false);

                if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);

                if (continuePrompt != null) continuePrompt.SetActive(false);

                ShowHubMenu();

            }

            else

            {

                ShowAwakeningStep(Mathf.Clamp(awakeningStep, 1, 3));

            }



            if (!isInHallway)

            {

                string loc = !string.IsNullOrEmpty(roomId) ? roomId : (!string.IsNullOrEmpty(character.currentLocationId) ? character.currentLocationId : "unit_407");

                SetLocation(loc);

            }



            RefreshLocationBGM();

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



        public void StopAllAudioSources()

        {

            if (atmosphereSource != null) atmosphereSource.Stop();

            if (ebsBroadcastSource != null) ebsBroadcastSource.Stop();

            if (staticSource != null) staticSource.Stop();

            if (radioSource != null) radioSource.Stop();

            if (sfxSource != null) sfxSource.Stop();

        }



        public void RefreshLocationBGM()

        {

            StopAllAudioSources();



            if (!isInHallway)

            {

                // Unit 407 (Living Room)

                if (atmosphereClip != null)

                {

                    PlayHubBGM(atmosphereClip, 0.5f, true);

                }

                else

                {

                    PlayHubBGM("Audio/BGM/atmosphere", 0.5f, true);

                }

            }

            else

            {

                // Floor 4 Hallway or Inspected Units

                if (string.IsNullOrEmpty(currentInspectedRoom) || currentInspectedRoom == "407")

                {

                    PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "406")

                {

                    if (room406_opened)

                        PlayHubBGM("Audio/SFX/tick", 0.25f, true);

                    else

                        PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "405")

                {

                    if (room405_opened)

                        PlayHubBGM("Audio/BGM/tension_drone", 0.4f, true);

                    else

                        PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "404")

                {

                    PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "403")

                {

                    PlayHubBGM("Audio/BGM/chopin", 0.45f, true);

                }

                else if (currentInspectedRoom == "402")

                {

                    if (room402_opened)

                        PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f, true);

                    else

                        PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "401")

                {

                    if (!turned_off_music)

                        PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f, true);

                    else

                        StopHubBGM();

                }

                else if (currentInspectedRoom == "utility")

                {

                    if (utility_opened)

                        PlayHubBGM("Audio/SFX/generator_hum", 0.45f, true);

                    else

                        PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

                else if (currentInspectedRoom == "stairs")

                {

                    if (stair_phase < 4)
                        PlayHubBGM("Audio/SFX/generator_hum", 0.45f, true);
                    else
                        StopHubBGM();

                }

                else

                {

                    PlayHubBGM("Audio/BGM/hallway_drone", 0.5f, true);

                }

            }

        }



        public enum ConsoleMode { Narrative, PDA, Gear }

        private ConsoleMode currentConsoleMode = ConsoleMode.Narrative;

        private string currentPDATab = "STATUS";



        public void SwitchConsoleMode(ConsoleMode mode, bool playSound = true)

        {

            currentConsoleMode = mode;

            if (dockContainerNarrative != null) dockContainerNarrative.SetActive(mode == ConsoleMode.Narrative);

            if (dockContainerPDA != null) dockContainerPDA.SetActive(mode == ConsoleMode.PDA);

            if (dockContainerGear != null) dockContainerGear.SetActive(mode == ConsoleMode.Gear);



            UpdateModeButtonVisuals();



            if (mode == ConsoleMode.PDA)

            {

                SwitchPDATab(string.IsNullOrEmpty(currentPDATab) ? "STATUS" : currentPDATab, playSound: false);

                RefreshAllPDAPanels();

            }

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



            // 0a. Hotkeys for System Menu Modal if open (Save, Load, Settings, Exit, Resume)

            if (systemMenuModal != null && systemMenuModal.activeSelf)

            {

                HandleSystemMenuModalInput();

                return;

            }



            // 0b. Hotkey to dismiss Story Log Modal if open

            if (storyLogModal != null && storyLogModal.activeSelf)

            {

                bool dismissLog = false;

                if (Keyboard.current != null)

                {

                    dismissLog = Keyboard.current.escapeKey.wasPressedThisFrame ||

                                 Keyboard.current.spaceKey.wasPressedThisFrame ||

                                 Keyboard.current.enterKey.wasPressedThisFrame ||

                                 Keyboard.current.numpadEnterKey.wasPressedThisFrame ||

                                 Keyboard.current.lKey.wasPressedThisFrame;

                }

                else

                {

                    try

                    {

                        dismissLog = Input.GetKeyDown(KeyCode.Escape) ||

                                     Input.GetKeyDown(KeyCode.Space) ||

                                     Input.GetKeyDown(KeyCode.Return) ||

                                     Input.GetKeyDown(KeyCode.KeypadEnter) ||

                                     Input.GetKeyDown(KeyCode.L);

                    }

                    catch (System.InvalidOperationException) { }

                }

                if (dismissLog)

                {

                    OnCloseStoryLogClicked();

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

                if (Keyboard.current != null)

                {

                    dismissModal = Keyboard.current.escapeKey.wasPressedThisFrame ||

                                   Keyboard.current.digit1Key.wasPressedThisFrame ||

                                   Keyboard.current.numpad1Key.wasPressedThisFrame ||

                                   Keyboard.current.spaceKey.wasPressedThisFrame ||

                                   Keyboard.current.enterKey.wasPressedThisFrame ||

                                   Keyboard.current.numpadEnterKey.wasPressedThisFrame;

                }

                else

                {

                    try

                    {

                        dismissModal = Input.GetKeyDown(KeyCode.Escape) ||

                                       Input.GetKeyDown(KeyCode.Alpha1) ||

                                       Input.GetKeyDown(KeyCode.Keypad1) ||

                                       Input.GetKeyDown(KeyCode.Space) ||

                                       Input.GetKeyDown(KeyCode.Return) ||

                                       Input.GetKeyDown(KeyCode.KeypadEnter);

                    }

                    catch (System.InvalidOperationException) { }

                }

                if (dismissModal)

                {

                    bool isEscape = false;

                    if (Keyboard.current != null)

                    {

                        isEscape = Keyboard.current.escapeKey.wasPressedThisFrame;

                    }

                    else

                    {

                        try { isEscape = Input.GetKeyDown(KeyCode.Escape); } catch (System.InvalidOperationException) { }

                    }



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

                // When in stepped dialogue phase (modularDialogueBox is active), Space or Enter also advances the line!

                bool spaceOrEnter = false;

                if (Keyboard.current != null)

                {

                    spaceOrEnter = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;

                }

                else

                {

                    try

                    {

                        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))

                        {

                            spaceOrEnter = true;

                        }

                    }

                    catch (System.InvalidOperationException) { }

                }

                if (spaceOrEnter && modularDialogueBox != null && modularDialogueBox.activeSelf && activeChoiceButtons.Count > 0)

                {

                    activeChoiceButtons[0].TriggerClick();

                }

                else

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

            }



            // 2. Hotkeys for Console Mode Switching (Tab, I, Esc, F1-F3)

            if (Keyboard.current != null)

            {

                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)

                {

                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);

                }

                else if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.f3Key.wasPressedThisFrame)

                {

                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);

                }

                else if (Keyboard.current.lKey.wasPressedThisFrame && currentConsoleMode == ConsoleMode.Narrative)

                {

                    OnOpenStoryLogClicked();

                    return;

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

            else

            {

                try

                {

                    if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P))

                    {

                        SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);

                    }

                    else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.F3))

                    {

                        SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);

                    }

                    else if (Input.GetKeyDown(KeyCode.L) && currentConsoleMode == ConsoleMode.Narrative)

                    {

                        OnOpenStoryLogClicked();

                        return;

                    }

                    else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F1)) && currentConsoleMode != ConsoleMode.Narrative)

                    {

                        SwitchConsoleMode(ConsoleMode.Narrative);

                    }

                    else if (currentConsoleMode != ConsoleMode.Narrative && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))

                    {

                        SwitchConsoleMode(ConsoleMode.Narrative);

                    }

                }

                catch (System.InvalidOperationException) { }

            }



            // Debug shortcuts for FEEL Feedback testing (F6: Screen Shake, F7: Low Humanity Pulse)

            bool testShake = false;

            bool testLowHum = false;

            if (Keyboard.current != null)

            {

                testShake = Keyboard.current.f6Key.wasPressedThisFrame;

                testLowHum = Keyboard.current.f7Key.wasPressedThisFrame;

                if (Keyboard.current.f8Key.wasPressedThisFrame)

                {

                    TacticalCombatModal.TriggerCombat("zombie_commuter,zombie_guard");

                }

            }

            else

            {

                try

                {

                    if (Input.GetKeyDown(KeyCode.F6)) testShake = true;

                    if (Input.GetKeyDown(KeyCode.F7)) testLowHum = true;

                    if (Input.GetKeyDown(KeyCode.F8))

                    {

                        TacticalCombatModal.TriggerCombat("zombie_commuter,zombie_guard");

                    }

                }

                catch (System.InvalidOperationException) { }

            }

            if (testShake) PlayScreenShake();

            if (testLowHum) DebugTestDamageFeedback();



            // Debounced multi-input check (Space or Enter to advance awakening dialogue)

            if (!introSeen && awakeningContainer != null && awakeningContainer.activeSelf && currentConsoleMode == ConsoleMode.Narrative)

            {

                bool spacePressed = false;

                bool enterPressed = false;



                if (Keyboard.current != null)

                {

                    spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;

                    enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;

                }

                else

                {

                    try

                    {

                        spacePressed = Input.GetKeyDown(KeyCode.Space);

                        enterPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

                    }

                    catch (System.InvalidOperationException) { }

                }



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

            currentPDATab = tabName;

            if (playSound) PlayClickSFX();



            if (pdaPanelStatus != null) pdaPanelStatus.SetActive(tabName == "STATUS");

            if (pdaPanelSkills != null) pdaPanelSkills.SetActive(tabName == "SKILLS");

            if (pdaPanelTraits != null) pdaPanelTraits.SetActive(tabName == "TRAITS");

            if (pdaPanelMap != null) pdaPanelMap.SetActive(tabName == "MAP");

            if (pdaPanelLore != null) pdaPanelLore.SetActive(tabName == "LORE");



            if (tabName == "MAP" && tacticalMapController != null)

            {

                string targetLoc = GetCurrentLocationId();

                if (currentPlayer != null) currentPlayer.currentLocationId = targetLoc;



                if (!string.IsNullOrEmpty(currentInspectedRoom))

                {

                    string inspectId = currentInspectedRoom.StartsWith("unit_") ? currentInspectedRoom : (currentInspectedRoom == "utility" || currentInspectedRoom == "stairs" ? currentInspectedRoom : "unit_" + currentInspectedRoom);

                    tacticalMapController.SetThresholdLocation(targetLoc, inspectId);

                }

                else

                {

                    tacticalMapController.SetCurrentLocation(targetLoc);

                }

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



            Outline outline = btn.GetComponent<Outline>();

            if (outline != null)

            {

                outline.effectColor = isActive

                    ? new Color(0.95f, 0.48f, 0.12f, 0.75f)

                    : new Color(0.20f, 0.22f, 0.26f, 0.75f);

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



        public void RefreshPDAStatusTab()

        {

            if (currentPlayer != null)

            {

                if (pdaCharNameText != null) pdaCharNameText.text = currentPlayer.FullName;

                if (pdaCharSubText != null) pdaCharSubText.text = $"{currentPlayer.gender}  |  {currentPlayer.job}";



                int hum = Mathf.Clamp(currentPlayer.humanity, 0, 100);

                if (pdaHumanityBarFill != null) pdaHumanityBarFill.fillAmount = hum / 100f;

                if (pdaHumanityValueText != null) pdaHumanityValueText.text = $"{hum}/100";



                int req = Mathf.Max(1, currentPlayer.xpRequired);

                int curXp = Mathf.Clamp(currentPlayer.xp, 0, req);

                if (pdaLevelText != null) pdaLevelText.text = $"LVL {currentPlayer.level}";

                if (pdaXpText != null) pdaXpText.text = $"{curXp}/{req} XP";

                if (pdaXpBarFill != null) pdaXpBarFill.fillAmount = Mathf.Clamp01((float)curXp / req);

            }

            else

            {

                if (pdaLevelText != null) pdaLevelText.text = "LVL 1";

                if (pdaXpText != null) pdaXpText.text = "0/100 XP";

                if (pdaXpBarFill != null) pdaXpBarFill.fillAmount = 0f;

            }



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

            string filled = new string('|', clamped);

            string empty = new string('|', Mathf.Max(0, max - clamped));

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
                pdaLoreText.text = "<color=#6c7a89><i>No notes recorded yet.\n\nKey clues, door codes, journals, and broadcasts you discover will be saved here for quick reference.</i></color>";
                return;
            }



            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var entry in loreSystem.Entries)

            {

                sb.AppendLine($"<color=#f39c12><b>// {entry.title.ToUpperInvariant()}</b></color> <color=#6c7a89>[{entry.timestamp}]</color>");

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



            PlayStageAnimation("Unit407Inside_Anim", "unit407", 8f);



            if (beat1Container != null) beat1Container.SetActive(step == 1);

            if (beat2Container != null) beat2Container.SetActive(step == 2);

            if (beat3Container != null) beat3Container.SetActive(step == 3);



            if (!gameObject.activeInHierarchy)

            {

                if (step == 1)

                {

                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";

                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#EF4444>CHANNEL: EMERGENCY BROADCAST SYSTEM</color>";

                    if (beat1IntroText != null) beat1IntroText.text = "The TV snaps out of static into a loud, jarring Emergency Alert tone that fills the dark room.";

                }

                else if (step == 2)

                {

                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // INTERNAL MONOLOGUE ]</color>";

                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#60A5FA>CHANNEL: NEURAL APPRAISAL</color>";

                    if (monologueBox != null) monologueBox.SetActive(true);

                    if (monologueSpeakerText != null) monologueSpeakerText.text = currentPlayer != null ? $"YOU // {currentPlayer.FullName.ToUpper()}" : "YOU";

                    if (monologueSpeechText != null) monologueSpeechText.text = GetPlayerMonologueText();

                    if (beat2IntroText != null) beat2IntroText.text = "The automated voice cuts off, leaving nothing but TV static.";

                    if (beat2FootnoteText != null) beat2FootnoteText.text = "Through the wall, heavy footsteps run past in the hallway. Someone yells, then a fire door slams shut down the hall.";

                }

                else if (step == 3)

                {

                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";

                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#F59E0B>CHANNEL: EQUIPMENT INSPECTION</color>";

                    if (tableStashDisplay != null) { tableStashDisplay.SetActive(true); PopulateTableStashUI(); }

                    if (beat3NarrativeText != null) beat3NarrativeText.text = "It's almost midnight. If the power goes out completely, you won't be able to see a thing out there.\n\nYour keys, wallet, and gear are on the coffee table. Grab what you can carry—check the kitchen, bathroom, or closet too—before you unlock the door.";

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

                        "The TV snaps out of static into a loud, jarring Emergency Alert tone that fills the dark room."

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

                        beat2IntroText.text = "The automated voice cuts off, leaving nothing but TV static.";

                    }



                    StartTypewriter(

                        beat2FootnoteText,

                        "Through the wall, heavy footsteps run past in the hallway. Someone yells, then a fire door slams shut down the hall."

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

                        "It's almost midnight. If the power goes out completely, you won't be able to see a thing out there.\n\n" +

                        "Your keys, wallet, and gear are on the coffee table.\n\n" +

                        "Grab what you can carry—and check the kitchen, bathroom, or closet—before you unlock the front door."

                    );

                    break;

            }

        }



        private string GetPlayerMonologueText()

        {

            if (currentPlayer == null) return "Lethal force authorized. They don't lock down the whole city over a normal riot. If panic hits this building, a hollow apartment door won't stop anyone.";



            if (currentPlayer.HasTrait("PARANOID"))

            {

                return "Shelter in place? Wait inside a fourth-floor apartment with one door while shots are going off outside? No thanks. That's a trap.";

            }

            if (string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase))

            {

                return "Lethal force authorized on city streets. Dispatch doesn't drop rules of engagement like that for a normal crowd. If they cordon off this block, nobody's getting out.";

            }

            if (string.Equals(currentPlayer.job, "Doctor", StringComparison.OrdinalIgnoreCase) || currentPlayer.HasTrait("FIELD MEDIC"))

            {

                return "Fluid-borne and causing violent behavior... Whatever's going around downtown, sitting inside a building with shared vents isn't safe.";

            }



            return "Lethal force authorized. They don't lock down the whole city over a normal riot. If panic hits this building, a hollow apartment door won't stop anyone.";

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

        private void ShowHubMenu(bool updateSituation = true)

        {

            introSeen = true;

            awakeningStep = 4;



            if (awakeningContainer != null) awakeningContainer.SetActive(false);

            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);



            if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";

            if (dockHeaderChannel != null) dockHeaderChannel.text = hasPacked ? "<color=#10B981>DOOR READY TO UNLOCK</color>" : "<color=#EF4444>FRONT DOOR LOCKED (NEED KEYS)</color>";

            UpdateSituationBoxHeader("LIVING ROOM");
            if (updateSituation)
            {
                SetSituationNarration(GetUnit407Anchor(), recordInLog: false);
            }



            if (!isInHallway)

            {

                PlayStageAnimation("Unit407Inside_Anim", "unit407", 8f);

            }



            EvaluateHubButtonStates();

        }



        private bool IsChoiceHotkeyPressed(int number)

        {

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

            try

            {

                KeyCode alpha = (KeyCode)((int)KeyCode.Alpha0 + number);

                KeyCode pad = (KeyCode)((int)KeyCode.Keypad0 + number);

                return Input.GetKeyDown(alpha) || Input.GetKeyDown(pad);

            }

            catch (System.InvalidOperationException)

            {

                return false;

            }

        }



        private void ReindexActiveChoiceHotkeys()

        {

            activeChoiceButtons.Clear();

            if (hubMenuContainer == null) return;



            if (isInHallway)

            {

                int hallIndex = 1;

                for (int i = 0; i < hallwayChoiceButtons.Count; i++)

                {

                    var tcb = hallwayChoiceButtons[i];

                    if (tcb != null && tcb.gameObject.activeSelf)

                    {

                        if (tcb.ButtonComponent != null && tcb.ButtonComponent.interactable)

                        {

                            tcb.SetChoiceNumber(hallIndex);

                            activeChoiceButtons.Add(tcb);

                            hallIndex++;

                        }

                        else

                        {

                            tcb.SetDisabledChoiceTag();

                        }

                    }

                }

                return;

            }



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

                        if (tcb.ButtonComponent != null && tcb.ButtonComponent.interactable)

                        {

                            tcb.SetChoiceNumber(index);

                            activeChoiceButtons.Add(tcb);

                            index++;

                        }

                        else

                        {

                            tcb.SetDisabledChoiceTag();

                        }

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



                    int reqTech = 3;

                    int reqTactics = 6;

                    string reqTrait = "GAMER";

                    if (_unit407HubPassage == null)

                    {

                        _unit407HubPassage = Resources.Load<SundayLastBreath.Data.StoryPassageData>("StoryPassages/Act1/Passage_Unit407_Hub");

                    }

                    if (_unit407HubPassage != null && _unit407HubPassage.choices != null)

                    {

                        var hackChoice = _unit407HubPassage.choices.Find(c => c != null && (c.choiceLabel.IndexOf("Exploit", System.StringComparison.OrdinalIgnoreCase) >= 0 || c.choiceLabel.IndexOf("Hack", System.StringComparison.OrdinalIgnoreCase) >= 0));

                        if (hackChoice != null)

                        {

                            if (hackChoice.requiredSkillLevel > 0) reqTech = hackChoice.requiredSkillLevel;

                            if (hackChoice.requiredStatValue > 0) reqTactics = hackChoice.requiredStatValue;

                            if (!string.IsNullOrEmpty(hackChoice.requiredTrait)) reqTrait = hackChoice.requiredTrait;

                        }

                    }



                    bool hasTechHacking = currentPlayer != null && currentPlayer.tech >= reqTech;

                    bool hasIntlHacking = currentPlayer != null && currentPlayer.Tactics >= reqTactics;

                    bool hasTraitHacking = currentPlayer != null && !string.IsNullOrEmpty(reqTrait) && currentPlayer.HasTrait(reqTrait);



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

                        SetChoiceButtonContent(hackTerminalButton, $"<s><color=#64748B>Log onto the computer to inspect encrypted network traffic.</color></s>  <color=#EF4444><i>[Requires Tech {reqTech}+, Tactics {reqTactics}+, or {reqTrait} Trait]</i></color>");

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



        private string lastOverflowNotice = "";



        private bool GiveOrDropItem(string itemId, int quantity)

        {

            if (string.IsNullOrEmpty(itemId) || quantity <= 0 || currentPlayer == null) return false;

            if (!currentPlayer.AddItem(itemId, quantity))

            {

                AddFloorItem(itemId, quantity);

                UpdateFloorItemsNotification();

                lastOverflowNotice += $"\n<color=#F59E0B>[BACKPACK FULL] {itemId} (x{quantity}) placed in room container. Press [I] to manage items.</color>";

                return false;

            }

            return true;

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

            SetSituationNarration("<color=#10B981>[ESSENTIALS SECURED]</color> You approach the coffee table and scoop up your keys, wallet, and everyday gear. An unsent message sits frozen on your phone: <color=#94A3B8>\"Traffic stopped on Pike. Heading home soon.\"</color> You pocket the heavy brass apartment key, tighten your pack straps, and stand ready by the door.");



            if (packedSuccessBanner != null)

            {

                packedSuccessBanner.SetActive(true);

            }

            if (packedSuccessText != null)

            {

                packedSuccessText.text = $"<color=#10B981>[OK] Apartment keys and essentials packed ({gatheredCount} items).\n<color=#38BDF8>Front door deadbolt can now be unlocked. Scavenge the kitchen, bathroom, or closet before stepping out.</color></color>";

            }



            // Objectives

            objectiveSystem.CompleteObjective("pack_gear");

            objectiveSystem.ActivateObjective("leave_apt");



            ShowHubMenu(updateSituation: false);

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

            SetSituationNarration("<color=#F59E0B>[KITCHEN SEARCHED]</color> You step into the kitchen. The refrigerator is already warming up in the blackout. You pull two bottles of water, canned tuna, and a manual can opener from the cupboards, set them on the counter, and walk back into the living room.");



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

            SetSituationNarration("<color=#10B981>[MEDICINE CABINET CLEARED]</color> You flick on your light and step into the bathroom. You sweep ibuprofen, rolls of gauze, and rubbing alcohol from the vanity mirror cabinet, place them ready on the hall bench, and return to the living room.");



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

            SetSituationNarration("<color=#38BDF8>[FRONT CLOSET SEARCHED]</color> You cross over to the hallway coat closet. You pull down your heavy rain poncho from its hanger, grab a roll of duct tape and wooden matches off the top shelf, and step back into the dark room.");



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

                        UpdateSituationBoxTextAndHeight("<color=#EF4444>[!] Backpack capacity reached! Make room to pack remaining supplies.</color>");

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

                        UpdateSituationBoxTextAndHeight("<color=#EF4444>[!] Backpack capacity reached! Make room to carry this item.</color>");

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

            currentPlayer?.SetFlag("checked_phone_harmon", true);



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

                "Phone Feed (Midnight)",

                "People near 5th St Hospital posted photos of police in gas masks blocking ambulances. News alerts mention an aggressive flu-like illness behind tonight's assaults, while viral clips show someone taking multiple chest shots and keeping going before cell service cut out.",

                gameClock.GetTimeString()

            );



            loreSystem.AddLore(

                "Professor Harmon's Door Code",

                "Harmon from Unit 406 posted on the neighborhood feed before leaving the city. He left his door code (8445) so neighbors could grab supplies.",

                gameClock.GetTimeString()

            );



            if (currentPlayer != null)

            {

                currentPlayer.SetFlag("checked_phone_harmon", true);

            }



            SetSituationNarration("<color=#38BDF8>[PHONE INTEL]</color> You scroll through panicked local posts and a shaky video from downtown before the signal drops to zero. One post from your neighbor across the hall stands out: Harmon left the building and posted his Unit 406 door code (<color=#F59E0B>8445</color>).");

            EvaluateHubButtonStates();

        }



        #region System Menu Modal (Multi-Slot Save / Multi-Slot Load / Settings / Exit Confirmation)

        private enum SystemMenuView { Main, SaveSlots, LoadSlots, Settings, ExitConfirm }

        private SystemMenuView currentSysView = SystemMenuView.Main;



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

            currentSysView = view;

            isSlotPickerSaveMode = (view == SystemMenuView.SaveSlots);



            if (sysMainPanel != null) sysMainPanel.SetActive(view == SystemMenuView.Main);

            if (sysSlotsPanel != null) sysSlotsPanel.SetActive(view == SystemMenuView.SaveSlots || view == SystemMenuView.LoadSlots);

            if (sysSettingsPanel != null) sysSettingsPanel.SetActive(view == SystemMenuView.Settings);

            if (sysExitConfirmPanel != null) sysExitConfirmPanel.SetActive(view == SystemMenuView.ExitConfirm);



            switch (view)

            {

                case SystemMenuView.Main:

                    if (sysHeaderSubText != null)

                        sysHeaderSubText.text = "SYSTEM // ARCHIVE, AUDIO CONFIGURATION & SESSION CONTROL";

                    RefreshSystemMenuTelemetry();

                    break;

                case SystemMenuView.SaveSlots:

                    if (sysHeaderSubText != null)

                        sysHeaderSubText.text = "SAVE GAME // SELECT ARCHIVE SLOT TO RECORD PROGRESS";

                    RefreshSystemSlotCards();

                    break;

                case SystemMenuView.LoadSlots:

                    if (sysHeaderSubText != null)

                        sysHeaderSubText.text = "LOAD GAME // SELECT ARCHIVED SLOT TO RESTORE";

                    RefreshSystemSlotCards();

                    break;

                case SystemMenuView.Settings:

                    if (sysHeaderSubText != null)

                        sysHeaderSubText.text = "SETTINGS // AUDIO MIXER & VOLUME CALIBRATION";

                    SyncSettingsSlidersFromAudio();

                    break;

                case SystemMenuView.ExitConfirm:

                    if (sysHeaderSubText != null)

                        sysHeaderSubText.text = "WARNING // CONFIRM SESSION TERMINATION";

                    UpdateExitWarningDetails();

                    break;

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

                            $"<color={titleHex}><b>[{slotTitle}]</b></color>   <color=#F8FAFC><b>{s.characterName}</b></color> <color=#94A3B8>({s.job})</color>  {savedBadge}";

                    }



                    if (sysSlotDetails[slotIdx] != null)

                    {

                        sysSlotDetails[slotIdx].text =

                            $"<color=#FBBF24>{s.roomDisplayName}</color>  |  <color=#E2E8F0>{s.clockDisplay}</color>  |  " +

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

                        sysSlotActionLabels[slotIdx].text = isSlotPickerSaveMode ? "<b>OVERWRITE</b>" : "<b>> LOAD</b>";

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

                        sysSlotTitles[slotIdx].text = $"<color=#64748B><b>[{slotTitle}]</b>   -   [ EMPTY ARCHIVE SLOT ]</color>";

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



            if (currentSysView == SystemMenuView.LoadSlots || !isSlotPickerSaveMode)

            {

                if (!SaveManager.Instance.HasSaveInSlot(slotIndex))

                {

                    string slotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

                    RefreshSystemMenuTelemetry($"<color=#EF4444><b>{slotTitle} is empty — no survivor data to restore.</b></color>");

                    return;

                }

                string restoreSlotTitle = SaveManager.Instance.GetSlotTitle(slotIndex);

                CloseSystemMenuModal();

                if (LoadingScreen.Instance != null)

                {

                    LoadingScreen.Instance.LoadSlotWithScreen(slotIndex, () =>

                    {

                        AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", "ARCHIVE RESTORED",

                            $"Survivor vitals, backpack items, and room progress restored from {restoreSlotTitle}.");

                    });

                }

                else

                {

                    bool loaded = SaveManager.Instance.LoadFromSlot(slotIndex);

                    if (loaded)

                    {

                        RefreshLocationBGM();

                    }

                }

            }

            else

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

                        $"<color=#10B981><b>[SAVED] {slotTitle}!</b></color>  [{s.saveTimestamp}]  -  {s.characterName} | {s.roomDisplayName}");

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

                sysLoadGameBtn.interactable = true; // Always allow player to click LOAD GAME to inspect and restore slots!

            }



            if (sysLoadBtnLabel != null)

            {

                sysLoadBtnLabel.text = hasSave

                    ? "<b>[ 2 ]  LOAD GAME</b>   <size=11><color=#94A3B8>Choose from 5 archive slots to restore</color></size>"

                    : "<b>[ 2 ]  LOAD GAME</b>   <size=11><color=#64748B>[NO ARCHIVE SAVES DETECTED]</color></size>";

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

                    sysTelemetryStatusText.text = "<color=#EF4444><b>ARCHIVE STATUS:</b> All save slots are currently empty.</color>";

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

                "[!] UNSAVED PROGRESS WARNING — CONFIRM EXIT",

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



        private void HandleSystemMenuModalInput()

        {

            bool esc = false;

            bool spaceOrEnter = false;

            int numPressed = -1;



            if (Keyboard.current != null)

            {

                var kb = Keyboard.current;

                esc = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame;

                spaceOrEnter = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;



                if (kb.digit0Key.wasPressedThisFrame || kb.numpad0Key.wasPressedThisFrame) numPressed = 0;

                else if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) numPressed = 1;

                else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) numPressed = 2;

                else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) numPressed = 3;

                else if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) numPressed = 4;

            }

            else

            {

                try

                {

                    esc = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);

                    spaceOrEnter = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

                    if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) numPressed = 0;

                    else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) numPressed = 1;

                    else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) numPressed = 2;

                    else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) numPressed = 3;

                    else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) numPressed = 4;

                }

                catch (System.InvalidOperationException) { }

            }



            switch (currentSysView)

            {

                case SystemMenuView.Main:

                    if (numPressed == 1) OnSystemMenuSaveClicked();

                    else if (numPressed == 2) OnSystemMenuLoadClicked();

                    else if (numPressed == 3) SwitchSystemMenuView(SystemMenuView.Settings);

                    else if (numPressed == 4) SwitchSystemMenuView(SystemMenuView.ExitConfirm);

                    else if (esc || spaceOrEnter) CloseSystemMenuModal();

                    break;



                case SystemMenuView.SaveSlots:

                case SystemMenuView.LoadSlots:

                    if (numPressed >= 0 && numPressed < SaveManager.TotalSlotCount)

                    {

                        OnSystemSlotActionClicked(numPressed);

                    }

                    else if (esc)

                    {

                        SwitchSystemMenuView(SystemMenuView.Main);

                    }

                    break;



                case SystemMenuView.Settings:

                    if (esc) SwitchSystemMenuView(SystemMenuView.Main);

                    break;



                case SystemMenuView.ExitConfirm:

                    if (numPressed == 1) OnSystemMenuSaveAndExitClicked();

                    else if (numPressed == 2) OnSystemMenuExitWithoutSavingClicked();

                    else if (esc || numPressed == 3) SwitchSystemMenuView(SystemMenuView.Main);

                    break;

            }

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

                        ? "<color=#ef4444>[LIVE RX]</color>"

                        : "<color=#7f1d1d>[LIVE RX]</color>";

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



        private float GetRadioSyncTime(float startRealtime, float maxObservedTime)

        {

            if (radioSource != null && radioSource.isPlaying && radioSource.time > 0.001f)

            {

                return Mathf.Max(maxObservedTime, radioSource.time);

            }

            return Mathf.Max(maxObservedTime, Time.realtimeSinceStartup - startRealtime);

        }



        private IEnumerator WaitUntilRadioTime(float targetTime, float startRealtime, System.Action<float> onUpdateTime, System.Func<float> getMaxTime)

        {

            while (isRadioTransmitting)

            {

                float t = GetRadioSyncTime(startRealtime, getMaxTime());

                onUpdateTime(t);

                if (t >= targetTime) yield break;

                yield return null;

            }

        }



        private IEnumerator TypeRichTextSegmentSynced(

            System.Text.StringBuilder sb,

            string segment,

            float startSec,

            float endSec,

            float startRealtime,

            System.Action<float> onUpdateTime,

            System.Func<float> getMaxTime)

        {

            yield return WaitUntilRadioTime(startSec, startRealtime, onUpdateTime, getMaxTime);

            if (!isRadioTransmitting) yield break;



            string prefix = sb.ToString();

            var visibleEnds = new System.Collections.Generic.List<int>(segment.Length);

            for (int i = 0; i < segment.Length; i++)

            {

                if (segment[i] == '<')

                {

                    int closeIdx = segment.IndexOf('>', i);

                    if (closeIdx >= 0)

                    {

                        i = closeIdx;

                        continue;

                    }

                }

                visibleEnds.Add(i + 1);

            }



            float duration = Mathf.Max(0.05f, endSec - startSec);

            while (isRadioTransmitting)

            {

                float t = GetRadioSyncTime(startRealtime, getMaxTime());

                onUpdateTime(t);

                float progress = Mathf.Clamp01((t - startSec) / duration);

                int count = Mathf.CeilToInt(progress * visibleEnds.Count);

                if (radioTranscriptText != null)

                {

                    string typed = count <= 0 ? "" : segment.Substring(0, visibleEnds[count - 1]);

                    radioTranscriptText.text = prefix + typed;

                }

                if (progress >= 1f) break;

                yield return null;

            }



            sb.Append(segment);

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();

        }



        private IEnumerator RadioTransmissionRoutine()

        {

            isRadioTransmitting = true;

            float startRealtime = Time.realtimeSinceStartup;

            float maxObservedTime = 0f;



            if (radioOutroObj != null)

            {

                radioOutroObj.SetActive(false);

            }



            PlayBeepSFX();



            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("<color=#94A3B8>[STATIC HISS]</color>\n");

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();



            // Audio cues in police_radio.mp3 (36.13s):

            // 1.55s–9.05s: Unit 4-Charlie first burst

            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");

            yield return TypeRichTextSegmentSynced(

                sb,

                "<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing!\"</i>\n\n",

                1.55f, 9.05f, startRealtime, t => maxObservedTime = t, () => maxObservedTime

            );

            if (!isRadioTransmitting) yield break;



            // 9.85s: Heavy slam against cruiser + shout

            yield return WaitUntilRadioTime(9.85f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);

            if (!isRadioTransmitting) yield break;

            PlayScreenShake();

            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();



            // 10.05s–11.65s: "Get off the damn car!" -> 13.25s–18.10s: "Dispatch, standard protocol is useless!..."

            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");

            yield return TypeRichTextSegmentSynced(

                sb,

                "<i>\"Get off the damn car! ",

                10.05f, 11.65f, startRealtime, t => maxObservedTime = t, () => maxObservedTime

            );

            if (!isRadioTransmitting) yield break;



            yield return TypeRichTextSegmentSynced(

                sb,

                "Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n\n",

                13.25f, 18.10f, startRealtime, t => maxObservedTime = t, () => maxObservedTime

            );

            if (!isRadioTransmitting) yield break;



            // 18.95s: Radio Squelch bursts

            yield return WaitUntilRadioTime(18.95f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);

            if (!isRadioTransmitting) yield break;

            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");

            if (radioTranscriptText != null) radioTranscriptText.text = sb.ToString();



            // 22.15s–35.55s: Dispatch authorization

            sb.AppendLine("<b><color=#F59E0B>DISPATCH:</color></b>");

            yield return TypeRichTextSegmentSynced(

                sb,

                "<i>\"4-Charlie, unrestricted lethal force is authorized. All units, listen carefully. Standard rules of engagement are suspended. Body shots will not stop them. You must destroy the brain. <color=#EF4444>Aim for the head.</color> I repeat... <color=#EF4444>aim for the head.</color> Avoid all physical contact.\"</i>\n\n",

                22.15f, 35.55f, startRealtime, t => maxObservedTime = t, () => maxObservedTime

            );

            if (!isRadioTransmitting) yield break;



            // 35.75s: Signal Cuts Out

            yield return WaitUntilRadioTime(35.75f, startRealtime, t => maxObservedTime = t, () => maxObservedTime);

            if (!isRadioTransmitting) yield break;

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

            sb.AppendLine("<i>\"Dispatch, 4-Charlie! We've lost the 5th Street barricade! They are not stopping! I repeat, shooting them in the chest does nothing!\"</i>\n");

            sb.AppendLine("<color=#EF4444><b>[HEAVY STRUGGLE / THUD]</b></color>\n");

            sb.AppendLine("<b><color=#38BDF8>UNIT 4-CHARLIE:</color></b>");

            sb.AppendLine("<i>\"Get off the damn car! Dispatch, standard protocol is useless! Requesting authorization for unrestricted lethal force!\"</i>\n");

            sb.AppendLine("<color=#F59E0B><b>[RADIO SQUELCH]</b></color>\n");

            sb.AppendLine("<b><color=#F59E0B>DISPATCH:</color></b>");

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

                "Police Radio Intercept (Midnight)",

                "Police units at 5th Street lost their barricade and reported suspects ignoring chest shots. Dispatch suspended standard rules of engagement, ordered officers to aim for the head, and warned them to avoid all physical contact.",

                gameClock.GetTimeString()

            );



            if (currentPlayer != null)

            {

                currentPlayer.SetFlag("listened_radio_407", true);

            }



            SetSituationNarration("<color=#EF4444>[POLICE FREQUENCY LOGGED]</color> Over the scanner, an officer on 5th Street yells that chest shots aren't stopping whoever's rushing the barricade. Dispatch tells all units to aim for the head before the channel drops to dead air.");

            EvaluateHubButtonStates();

            if (CurrentPassage != null) RenderPassageChoices(CurrentPassage);

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

                    hackingLedText.text = $"<color=#10b981>[PWR]</color>   <color={hddColor}>[HDD]</color>   <color={netColor}>[NET]</color>";

                }

                tick++;

                yield return new WaitForSecondsRealtime(0.12f);

            }



            if (hackingLedText != null)

            {

                hackingLedText.text = "<color=#10b981>[PWR]</color>   <color=#78350f>[HDD]</color>   <color=#ef4444>[NET SEVERED]</color>";

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

                "<color=#64748b>>_ terminal@unit407: scanning local building gateway...</color>",

                "<color=#38bdf8>[*] SUBNET 192.168.1.1 — SNIFFING CITY EMERGENCY RELAY...</color>",

                "<color=#10b981>[+] LOCAL ROUTER CACHE DUMPED... [OK]</color>",

                "<color=#f59e0b>[*] READING LAST CACHED MUNICIPAL DISPATCH PACKETS...</color>",

                "",

                "<color=#ef4444><b>================== [ CACHED CITY EMERGENCY MEMO ] ==================</b></color>",

                "<color=#f1f5f9><b>INCIDENT CODE  :</b></color> <color=#38bdf8>AMBER-9 (UNCONTROLLED MASS VIOLENCE)</color>",

                "<color=#f1f5f9><b>AFFECTED ZONES :</b></color> <color=#f59e0b>5TH ST HOSPITAL // DOWNTOWN CORRIDOR // I-5 RAMPS</color>",

                "<color=#f1f5f9><b>MEDICAL NOTE   :</b></color> <color=#f59e0b>SUSPECTED ACUTE CONTAGION — BITING / EXTREME AGITATION</color>",

                "<color=#f1f5f9><b>TRAFFIC STATUS :</b></color> <color=#ef4444>BRIDGES & FREEWAY ON-RAMPS CLOSING BY 0100</color>",

                "<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>HOLD IN PLACE UNTIL FURTHER COUNTY ORDERS.</b></color>",

                "<color=#f1f5f9><b>COUNTY STAGING :</b></color> <color=#10b981>EASTBOUND ROUTES / TIMBERLINE CORRIDOR (UNCONFIRMED)</color>",

                "<color=#ef4444><b>====================================================================</b></color>",

                "",

                "<color=#10b981>[+] CACHED ROAD CLOSURE LIST SAVED (ROAD_STATUS.DAT).</color>",

                "<color=#ffd700><b>[+] FOUND UNREVOKED MUNICIPAL GATE PASSKEY... SAVED TO PHONE!</b></color>",

                "",

                "<color=#ef4444><b>[!] UPSTREAM GATEWAY TIMED OUT. NO CARRIER ON WAN PORT.</b></color>",

                "<color=#94a3b8>[X] CONNECTION DROPPED. FIBER LINE DEAD.</color>"

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

                hackingLedText.text = "<color=#10b981>[PWR]</color>   <color=#78350f>[HDD]</color>   <color=#ef4444>[NET SEVERED]</color>";

            }



            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("<color=#64748b>>_ terminal@unit407: scanning local building gateway...</color>");

            sb.AppendLine("<color=#38bdf8>[*] SUBNET 192.168.1.1 — SNIFFING CITY EMERGENCY RELAY...</color>");

            sb.AppendLine("<color=#10b981>[+] LOCAL ROUTER CACHE DUMPED... [OK]</color>");

            sb.AppendLine("<color=#f59e0b>[*] READING LAST CACHED MUNICIPAL DISPATCH PACKETS...</color>");

            sb.AppendLine("");

            sb.AppendLine("<color=#ef4444><b>================== [ CACHED CITY EMERGENCY MEMO ] ==================</b></color>");

            sb.AppendLine("<color=#f1f5f9><b>INCIDENT CODE  :</b></color> <color=#38bdf8>AMBER-9 (UNCONTROLLED MASS VIOLENCE)</color>");

            sb.AppendLine("<color=#f1f5f9><b>AFFECTED ZONES :</b></color> <color=#f59e0b>5TH ST HOSPITAL // DOWNTOWN CORRIDOR // I-5 RAMPS</color>");

            sb.AppendLine("<color=#f1f5f9><b>MEDICAL NOTE   :</b></color> <color=#f59e0b>SUSPECTED ACUTE CONTAGION — BITING / EXTREME AGITATION</color>");

            sb.AppendLine("<color=#f1f5f9><b>TRAFFIC STATUS :</b></color> <color=#ef4444>BRIDGES & FREEWAY ON-RAMPS CLOSING BY 0100</color>");

            sb.AppendLine("<color=#f1f5f9><b>DIRECTIVE      :</b></color> <color=#ef4444><b>HOLD IN PLACE UNTIL FURTHER COUNTY ORDERS.</b></color>");

            sb.AppendLine("<color=#f1f5f9><b>COUNTY STAGING :</b></color> <color=#10b981>EASTBOUND ROUTES / TIMBERLINE CORRIDOR (UNCONFIRMED)</color>");

            sb.AppendLine("<color=#ef4444><b>====================================================================</b></color>");

            sb.AppendLine("");

            sb.AppendLine("<color=#10b981>[+] CACHED ROAD CLOSURE LIST SAVED (ROAD_STATUS.DAT).</color>");

            sb.AppendLine("<color=#ffd700><b>[+] FOUND UNREVOKED MUNICIPAL GATE PASSKEY... SAVED TO PHONE!</b></color>");

            sb.AppendLine("");

            sb.AppendLine("<color=#ef4444><b>[!] UPSTREAM GATEWAY TIMED OUT. NO CARRIER ON WAN PORT.</b></color>");

            sb.AppendLine("<color=#94a3b8>[X] CONNECTION DROPPED. FIBER LINE DEAD.</color>");



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

                "Cached City Traffic (Midnight)",

                "Pulled cached municipal memos before the fiber line died. Downtown and 5th St Hospital are locked down under 'Code Amber-9', and freeway ramps and bridges are closing by 01:00. Saved a valid checkpoint passkey to my phone.",

                gameClock.GetTimeString()

            );



            if (currentPlayer != null)

            {

                currentPlayer.SetFlag("hacked_terminal_407", true);

            }



            SetSituationNarration("<color=#EF4444>[CACHED DISPATCH PULLED]</color> You pull a few cached city memos before the connection dies completely. Downtown and 5th Street are locked down, and bridges are closing by 01:00. You grab the attached municipal passkey and shut the laptop—if you're leaving this neighborhood, it has to be now.");

            EvaluateHubButtonStates();

            RefreshAllPDAPanels();

            if (CurrentPassage != null) RenderPassageChoices(CurrentPassage);

        }



        private void OnLeaveApartmentClicked()

        {

            bool hasKey = currentPlayer != null && (currentPlayer.HasItem("Apartment Key") || currentPlayer.HasItem("Apartment 407 Key"));

            bool tableEmpty = tableStash == null || tableStash.Count == 0;

            bool canExit = hasPacked && tableEmpty && hasKey;



            if (!canExit)

            {

                PlayBeepSFX();

                SetSituationNarration("<color=#EF4444>[DEADBOLT ENGAGED]</color> You try the front latch, but the deadbolt is locked tight from the inside. Your apartment keys and gear are still on the coffee table. Stepping into the hallway without them guarantees locking yourself out in the dark.");

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

        private TextMeshProUGUI doorTransitionSkipPromptTmp;

        private GameObject doorTransitionContentBox;

        private CanvasGroup doorTransitionContentCanvasGroup;

        private UnityEngine.Video.VideoPlayer doorVideoPlayer;

        private RawImage doorVideoRawImage;

        private RenderTexture doorVideoRenderTexture;



        private bool CheckDoorTransitionSkipInput()

        {

            if (Keyboard.current != null)

            {

                var kb = Keyboard.current;

                if (kb.anyKey.wasPressedThisFrame) return true;

            }

            if (Mouse.current != null)

            {

                var m = Mouse.current;

                if (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame) return true;

            }

            if (Gamepad.current != null)

            {

                var pad = Gamepad.current;

                if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame) return true;

            }

            try

            {

                if (Input.anyKeyDown || Input.GetMouseButtonDown(0)) return true;

            }

            catch (System.InvalidOperationException) { }

            return false;

        }



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

            doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f); // Hidden initially until smooth fade-in

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



            // Try loading VideoClip or file path (unit407_door_transition with fallback to legacy room403)

            var clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/unit407_door_transition");

            if (clip == null) clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/room403_door_transition");

            if (clip != null)

            {

                doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;

                doorVideoPlayer.clip = clip;

            }

            else

            {

                string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "unit407_door_transition.mp4").Replace("\\", "/");

                if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");

                if (System.IO.File.Exists(filePath))

                {

                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;

                    doorVideoPlayer.url = filePath;

                }

            }



            // Centered content box (Narrative Subtitle / Fallback) with its own CanvasGroup for smooth dissolves

            doorTransitionContentBox = new GameObject("ContentBox", typeof(RectTransform), typeof(CanvasGroup));

            doorTransitionContentBox.transform.SetParent(doorTransitionOverlay.transform, false);

            doorTransitionContentCanvasGroup = doorTransitionContentBox.GetComponent<CanvasGroup>();

            doorTransitionContentCanvasGroup.alpha = 1f;

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

            doorTransitionDescTmp.text = "You turn the brass key in the deadbolt. The lock mechanism clunks open with a heavy metallic click. You take a breath, push the door open, and step out into the 4th floor corridor.";



            // Subtle Skip Prompt in bottom-right corner

            GameObject skipObj = new GameObject("SkipPromptText", typeof(RectTransform), typeof(TextMeshProUGUI));

            skipObj.transform.SetParent(doorTransitionOverlay.transform, false);

            RectTransform skipRt = skipObj.GetComponent<RectTransform>();

            skipRt.anchorMin = new Vector2(0.5f, 0.02f);

            skipRt.anchorMax = new Vector2(0.98f, 0.08f);

            skipRt.offsetMin = Vector2.zero;

            skipRt.offsetMax = Vector2.zero;



            doorTransitionSkipPromptTmp = skipObj.GetComponent<TextMeshProUGUI>();

            doorTransitionSkipPromptTmp.fontSize = 12f;

            doorTransitionSkipPromptTmp.fontStyle = FontStyles.Bold;

            doorTransitionSkipPromptTmp.alignment = TextAlignmentOptions.BottomRight;

            doorTransitionSkipPromptTmp.color = new Color(0.40f, 0.48f, 0.58f, 0.75f);

            doorTransitionSkipPromptTmp.text = "<color=#64748B>[SPACE / CLICK TO SKIP]</color>";

            skipObj.SetActive(false);



            doorTransitionOverlay.SetActive(false);

        }



        private IEnumerator DoorExitTransitionRoutine()

        {

            isDoorTransitioning = true;

            EnsureDoorTransitionOverlay();

            doorTransitionOverlay.SetActive(true);

            doorTransitionOverlay.transform.SetAsLastSibling();

            doorTransitionCanvasGroup.alpha = 0f;



            doorTransitionContentBox?.SetActive(true);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);



            // Refresh video source in case of dynamic file addition

            if (doorVideoPlayer != null)

            {

                if (doorVideoPlayer.clip == null && string.IsNullOrEmpty(doorVideoPlayer.url))

                {

                    var c = Resources.Load<UnityEngine.Video.VideoClip>("Videos/unit407_door_transition");

                    if (c == null) c = Resources.Load<UnityEngine.Video.VideoClip>("Videos/room403_door_transition");

                    if (c != null)

                    {

                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;

                        doorVideoPlayer.clip = c;

                    }

                    else

                    {

                        string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "unit407_door_transition.mp4").Replace("\\", "/");

                        if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");

                        if (System.IO.File.Exists(filePath))

                        {

                            doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;

                            doorVideoPlayer.url = filePath;

                        }

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



            // Player reads the narrative text

            if (doorTransitionTextToVideoDelay > 0f)

            {

                yield return new WaitForSecondsRealtime(doorTransitionTextToVideoDelay);

            }



            // Smoothly dissolve / fade out the narrative text box into deep darkness

            float textFadeTime = Mathf.Max(0.05f, doorTransitionTextFadeOutDuration);

            if (doorTransitionContentCanvasGroup != null)

            {

                for (float t = 0; t < textFadeTime; t += Time.unscaledDeltaTime)

                {

                    doorTransitionContentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / textFadeTime);

                    yield return null;

                }

                doorTransitionContentCanvasGroup.alpha = 0f;

            }

            doorTransitionContentBox?.SetActive(false);



            // Brief cinematic breath in pitch black darkness

            yield return new WaitForSecondsRealtime(0.15f);



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



                    // Smoothly fade in the video from black (eliminates abrupt cuts!)

                    float vidFadeIn = Mathf.Max(0.05f, doorTransitionVideoFadeInDuration);

                    for (float t = 0; t < vidFadeIn; t += Time.unscaledDeltaTime)

                    {

                        float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                        yield return null;

                    }

                    if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;



                    // Show skip prompt

                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);



                    // Track video playback with instant skip support

                    float vidLength = (float)doorVideoPlayer.length;

                    if (vidLength <= 0.5f) vidLength = 4.5f;



                    float midTime = vidLength * 0.55f;

                    float elapsed = 0.2f + vidFadeIn;

                    bool playedFootstep = false;

                    bool skipped = false;



                    while (elapsed < vidLength - 0.35f)

                    {

                        if (CheckDoorTransitionSkipInput())

                        {

                            skipped = true;

                            break;

                        }



                        if (!playedFootstep && elapsed >= midTime)

                        {

                            playedFootstep = true;

                            if (doorTransitionPlayFootstepSFX)

                            {

                                if (doorTransitionFootstepSFX != null) PlaySFX(doorTransitionFootstepSFX, 0.8f);

                                else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);

                            }

                        }



                        elapsed += Time.unscaledDeltaTime;

                        yield return null;

                    }



                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);



                    if (skipped)

                    {

                        if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

                        if (doorTransitionPlaySlamSFX)

                        {

                            if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.75f);

                            else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.75f);

                        }

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

                        yield return new WaitForSecondsRealtime(0.05f);

                    }

                    else

                    {

                        if (doorTransitionPlaySlamSFX)

                        {

                            if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.65f);

                            else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);

                        }



                        // Smoothly fade video out to darkness as door slams shut

                        float vidFadeOut = 0.35f;

                        for (float t = 0; t < vidFadeOut; t += Time.unscaledDeltaTime)

                        {

                            float a = Mathf.Lerp(1f, 0f, t / vidFadeOut);

                            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                            yield return null;

                        }

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);



                        if (doorTransitionPostVideoPause > 0f)

                        {

                            yield return new WaitForSecondsRealtime(doorTransitionPostVideoPause);

                        }

                    }

                }

            }



            // Fallback narrative sequence if video could not play

            if (!playedVideo)

            {

                doorTransitionContentBox?.SetActive(true);

                if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

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

                doorTransitionDescTmp.text = "You take a breath of freezing, copper-scented air and step across the threshold.\nBehind you, the heavy door swings shut.\n\n<color=#EF4444>The spring-latch snaps into the strike plate.</color>\n<color=#94A3B8>The deadbolt locks from the outside. You are out in the corridor with no turning back.</color>";

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

            if (hallwayStartingPassage != null)

            {

                PlayStoryPassage(hallwayStartingPassage);

            }

            else

            {

                EnterHallwayMode(1);

            }



            // Step 4: Fade in from black to reveal the red 4th floor corridor

            fadeTime = Mathf.Max(0.05f, doorTransitionFadeInDuration);

            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)

            {

                doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

                yield return null;

            }

            doorTransitionCanvasGroup.alpha = 0f;



            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

            doorTransitionContentBox?.SetActive(true);

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



        public void PlayDoorTransition(string videoResourcePath, string roomTitle, string narrativeSubtitle, bool playKeySFX, System.Action onReachedDestination, bool playSlam = false)

        {

            if (isDoorTransitioning)

            {

                onReachedDestination?.Invoke();

                return;

            }

            StartCoroutine(GenericDoorTransitionRoutine(videoResourcePath, roomTitle, narrativeSubtitle, playKeySFX, onReachedDestination, playSlam));

        }



        private IEnumerator GenericDoorTransitionRoutine(string videoResourcePath, string roomTitle, string narrativeSubtitle, bool playKeySFX, System.Action onReachedDestination, bool playSlam = false)

        {

            isDoorTransitioning = true;

            EnsureDoorTransitionOverlay();

            doorTransitionOverlay.SetActive(true);

            doorTransitionOverlay.transform.SetAsLastSibling();

            doorTransitionCanvasGroup.alpha = 0f;



            doorTransitionContentBox?.SetActive(true);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);



            // Configure video if supplied

            bool hasVideo = false;

            if (!string.IsNullOrEmpty(videoResourcePath) && doorVideoPlayer != null)

            {

                string cleanRes = videoResourcePath;

                if (cleanRes.StartsWith("Videos/")) cleanRes = cleanRes.Substring(7);



                var clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/" + cleanRes);

                if (clip != null)

                {

                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;

                    doorVideoPlayer.clip = clip;

                    hasVideo = true;

                }

                else

                {

                    string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", cleanRes + ".mp4").Replace("\\", "/");

                    if (System.IO.File.Exists(filePath))

                    {

                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;

                        doorVideoPlayer.url = filePath;

                        hasVideo = true;

                    }

                }



                if (hasVideo) doorVideoPlayer.Prepare();

            }



            // Step 1: Smooth fade to black with audio

            doorTransitionTitleTmp.text = roomTitle;

            doorTransitionDescTmp.text = narrativeSubtitle;

            if (playKeySFX)

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



            float textFadeTime = Mathf.Max(0.05f, doorTransitionTextFadeOutDuration);

            if (doorTransitionContentCanvasGroup != null)

            {

                for (float t = 0; t < textFadeTime; t += Time.unscaledDeltaTime)

                {

                    doorTransitionContentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / textFadeTime);

                    yield return null;

                }

                doorTransitionContentCanvasGroup.alpha = 0f;

            }

            doorTransitionContentBox?.SetActive(false);



            yield return new WaitForSecondsRealtime(0.12f);



            // Step 2: Play Video if available

            bool playedVideo = false;

            if (hasVideo && doorVideoPlayer != null)

            {

                float prepTimer = 0f;

                while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)

                {

                    prepTimer += Time.unscaledDeltaTime;

                    yield return null;

                }



                if (doorVideoPlayer.isPrepared)

                {

                    playedVideo = true;

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



                    float vidFadeIn = Mathf.Max(0.05f, doorTransitionVideoFadeInDuration);

                    for (float t = 0; t < vidFadeIn; t += Time.unscaledDeltaTime)

                    {

                        float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                        yield return null;

                    }

                    if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;



                    // Show skip prompt

                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);



                    float vidLength = (float)doorVideoPlayer.length;

                    if (vidLength <= 0.5f) vidLength = 4.5f;



                    float midTime = vidLength * 0.55f;

                    float elapsed = 0.2f + vidFadeIn;

                    bool playedFootstep = false;

                    bool skipped = false;



                    while (elapsed < vidLength - 0.35f)

                    {

                        if (CheckDoorTransitionSkipInput())

                        {

                            skipped = true;

                            break;

                        }



                        if (!playedFootstep && elapsed >= midTime)

                        {

                            playedFootstep = true;

                            if (doorTransitionPlayFootstepSFX)

                            {

                                if (doorTransitionFootstepSFX != null) PlaySFX(doorTransitionFootstepSFX, 0.8f);

                                else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);

                            }

                        }



                        elapsed += Time.unscaledDeltaTime;

                        yield return null;

                    }



                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);



                    if (skipped)

                    {

                        if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

                        if (playSlam && doorTransitionPlaySlamSFX)

                        {

                            if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.75f);

                            else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.75f);

                        }

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

                        yield return new WaitForSecondsRealtime(0.05f);

                    }

                    else

                    {

                        if (playSlam && doorTransitionPlaySlamSFX)

                        {

                            if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.65f);

                            else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);

                        }



                        float vidFadeOut = 0.35f;

                        for (float t = 0; t < vidFadeOut; t += Time.unscaledDeltaTime)

                        {

                            float a = Mathf.Lerp(1f, 0f, t / vidFadeOut);

                            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                            yield return null;

                        }

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);



                        if (doorTransitionPostVideoPause > 0f)

                        {

                            yield return new WaitForSecondsRealtime(doorTransitionPostVideoPause);

                        }

                    }

                }

            }



            // Fallback audio if video wasn't played

            if (!playedVideo)

            {

                if (doorTransitionPlayKnobSFX)

                {

                    if (doorTransitionKnobSFX != null) PlaySFX(doorTransitionKnobSFX, 0.95f);

                    else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);

                }

                yield return new WaitForSecondsRealtime(0.6f);

            }



            // Step 3: Switch scene/room behind the black curtain!

            onReachedDestination?.Invoke();



            // Step 4: Fade in to reveal the room

            fadeTime = Mathf.Max(0.05f, doorTransitionFadeInDuration);

            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)

            {

                doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

                yield return null;

            }

            doorTransitionCanvasGroup.alpha = 0f;



            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

            doorTransitionContentBox?.SetActive(true);

            doorTransitionOverlay.SetActive(false);

            isDoorTransitioning = false;

        }



        #region Hallway & Exploration Implementation

        public void RestoreHallwayExploration()

        {

            isInHallway = true;

            if (awakeningContainer != null) awakeningContainer.SetActive(false);

            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);

            if (continuePrompt != null) continuePrompt.SetActive(false);



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



            SetApartmentButtonsVisible(false);

            currentConsoleMode = ConsoleMode.Narrative;



            if (currentInspectedRoom == "407")

            {

                InspectUnit407Exterior();

            }

            else if (currentInspectedRoom == "406")

            {

                InspectUnit406();

            }

            else if (currentInspectedRoom == "405")

            {

                InspectUnit405();

            }

            else if (currentInspectedRoom == "404")

            {

                InspectUnit404();

            }

            else if (currentInspectedRoom == "403")

            {

                InspectUnit403();

            }

            else if (currentInspectedRoom == "402")

            {

                InspectUnit402();

            }

            else if (currentInspectedRoom == "401")

            {

                InspectUnit401();

            }

            else if (currentInspectedRoom == "utility")

            {

                InspectUtilityCloset();

            }

            else if (currentInspectedRoom == "stairs")

            {

                InspectStairwell();

            }

            else

            {

                EnterHallwayMode(hallwaySection);

            }

        }



        public void SaveWorldState(string file)

        {

            if (string.IsNullOrEmpty(file)) return;

            ES3.Save<bool>("hub_is_in_hallway", isInHallway, file);

            ES3.Save<int>("hub_hallway_section", hallwaySection, file);

            ES3.Save<string>("hub_inspected_room", currentInspectedRoom ?? "", file);



            // Room 406

            ES3.Save<bool>("hub_room406_opened", room406_opened, file);

            ES3.Save<int>("hub_failed_406_attempts", failed_406_attempts, file);

            ES3.Save<bool>("hub_room406_locked_out", room406_locked_out, file);

            ES3.Save<bool>("hub_studied_406_mechanics", studied_406_mechanics, file);

            ES3.Save<bool>("hub_studied_406_anatomy", studied_406_anatomy, file);

            ES3.Save<bool>("hub_read_406_journal", read_406_journal, file);

            ES3.Save<bool>("hub_looted_406_kitchen", looted_406_kitchen, file);

            ES3.Save<bool>("hub_looted_406", looted_406, file);



            // Room 405

            ES3.Save<bool>("hub_room405_opened", room405_opened, file);

            ES3.Save<bool>("hub_room405_trap_triggered", room405_trap_triggered, file);

            ES3.Save<bool>("hub_read_marcus_log", read_marcus_log, file);

            ES3.Save<bool>("hub_looted_405_bench", looted_405_bench, file);

            ES3.Save<bool>("hub_looted_405_safe", looted_405_safe, file);



            // Room 404

            ES3.Save<bool>("hub_spoke_to_mrs_gable", spoke_to_mrs_gable, file);



            // Room 402

            ES3.Save<bool>("hub_room402_opened", room402_opened, file);

            ES3.Save<string>("hub_room402_entry_method", room402_entry_method, file);

            ES3.Save<bool>("hub_looted_402_weapon", looted_402_weapon, file);

            ES3.Save<bool>("hub_looted_402_kitchen", looted_402_kitchen, file);

            ES3.Save<bool>("hub_looted_402_bathroom", looted_402_bathroom, file);

            ES3.Save<bool>("hub_looted_402_ring", looted_402_ring, file);

            ES3.Save<bool>("hub_took_402_photo", took_402_photo, file);

            ES3.Save<bool>("hub_read_402_note", read_402_note, file);



            // Room 401

            ES3.Save<bool>("hub_room401_entered", room401_entered, file);

            ES3.Save<bool>("hub_shelf_lifted", shelf_lifted, file);

            ES3.Save<bool>("hub_turned_off_music", turned_off_music, file);

            ES3.Save<bool>("hub_looted_401_kitchen", looted_401_kitchen, file);

            ES3.Save<bool>("hub_looted_401_bathroom", looted_401_bathroom, file);

            ES3.Save<bool>("hub_looted_401_alcohol", looted_401_alcohol, file);

            ES3.Save<bool>("hub_looted_401_pockets", looted_401_pockets, file);



            // Utility

            ES3.Save<bool>("hub_utility_opened", utility_opened, file);

            ES3.Save<bool>("hub_read_utility_log", read_utility_log, file);

            ES3.Save<bool>("hub_looted_utility_bleach", looted_utility_bleach, file);

            ES3.Save<bool>("hub_looted_utility_tape", looted_utility_tape, file);

            ES3.Save<bool>("hub_looted_utility_flashlight", looted_utility_flashlight, file);



            // Stairs

            ES3.Save<int>("hub_stair_phase", stair_phase, file);

            ES3.Save<string>("hub_descent_method", descentMethod, file);

            ES3.Save<bool>("hub_barricade_pushed", barricade_pushed, file);

            ES3.Save<bool>("hub_barricade_inspected", barricade_inspected, file);

            ES3.Save<bool>("hub_stair_3f_door_checked", stair_3f_door_checked, file);

            ES3.Save<bool>("hub_stair_listened_2f_door", stair_listened_2f_door, file);

            ES3.Save<bool>("hub_stair_noise_made", stair_noise_made, file);

            ES3.Save<bool>("hub_stair_hum_deducted", stair_hum_deducted, file);



            // Apartment Hub Visit Flags

            ES3.Save<bool>("hub_phone_visited", phoneVisited, file);

            ES3.Save<bool>("hub_radio_visited", radioVisited, file);

            ES3.Save<bool>("hub_hacking_visited", hackingVisited, file);



            // Lore and Story Logs

            if (loreSystem != null)

            {

                ES3.Save<List<SundayLastBreath.Core.LoreEntry>>("hub_lore_entries", loreSystem.GetEntriesList(), file);

            }

            ES3.Save<List<string>>("hub_story_logs", new List<string>(_storyLogs), file);
            ES3.Save<List<string>>("hub_visited_anchors", new List<string>(_visitedLocationAnchors), file);

        }



        public void LoadWorldState(string file)

        {

            if (string.IsNullOrEmpty(file) || !ES3.FileExists(file)) return;

            if (ES3.KeyExists("hub_is_in_hallway", file)) isInHallway = ES3.Load<bool>("hub_is_in_hallway", file);

            if (ES3.KeyExists("hub_hallway_section", file)) hallwaySection = ES3.Load<int>("hub_hallway_section", file);

            if (ES3.KeyExists("hub_inspected_room", file))

            {

                string r = ES3.Load<string>("hub_inspected_room", file);

                currentInspectedRoom = string.IsNullOrEmpty(r) ? null : r;

            }



            if (ES3.KeyExists("hub_room406_opened", file)) room406_opened = ES3.Load<bool>("hub_room406_opened", file);

            if (ES3.KeyExists("hub_failed_406_attempts", file)) failed_406_attempts = ES3.Load<int>("hub_failed_406_attempts", file);

            if (ES3.KeyExists("hub_room406_locked_out", file)) room406_locked_out = ES3.Load<bool>("hub_room406_locked_out", file);

            if (ES3.KeyExists("hub_studied_406_mechanics", file)) studied_406_mechanics = ES3.Load<bool>("hub_studied_406_mechanics", file);

            if (ES3.KeyExists("hub_studied_406_anatomy", file)) studied_406_anatomy = ES3.Load<bool>("hub_studied_406_anatomy", file);

            if (ES3.KeyExists("hub_read_406_journal", file)) read_406_journal = ES3.Load<bool>("hub_read_406_journal", file);

            if (ES3.KeyExists("hub_looted_406_kitchen", file)) looted_406_kitchen = ES3.Load<bool>("hub_looted_406_kitchen", file);

            if (ES3.KeyExists("hub_looted_406", file)) looted_406 = ES3.Load<bool>("hub_looted_406", file);



            if (ES3.KeyExists("hub_room405_opened", file)) room405_opened = ES3.Load<bool>("hub_room405_opened", file);

            if (ES3.KeyExists("hub_room405_trap_triggered", file)) room405_trap_triggered = ES3.Load<bool>("hub_room405_trap_triggered", file);

            if (ES3.KeyExists("hub_read_marcus_log", file)) read_marcus_log = ES3.Load<bool>("hub_read_marcus_log", file);

            if (ES3.KeyExists("hub_looted_405_bench", file)) looted_405_bench = ES3.Load<bool>("hub_looted_405_bench", file);

            if (ES3.KeyExists("hub_looted_405_safe", file)) looted_405_safe = ES3.Load<bool>("hub_looted_405_safe", file);



            if (ES3.KeyExists("hub_spoke_to_mrs_gable", file)) spoke_to_mrs_gable = ES3.Load<bool>("hub_spoke_to_mrs_gable", file);



            if (ES3.KeyExists("hub_room402_opened", file)) room402_opened = ES3.Load<bool>("hub_room402_opened", file);

            if (ES3.KeyExists("hub_room402_entry_method", file)) room402_entry_method = ES3.Load<string>("hub_room402_entry_method", file);

            if (ES3.KeyExists("hub_looted_402_weapon", file)) looted_402_weapon = ES3.Load<bool>("hub_looted_402_weapon", file);

            if (ES3.KeyExists("hub_looted_402_kitchen", file)) looted_402_kitchen = ES3.Load<bool>("hub_looted_402_kitchen", file);

            if (ES3.KeyExists("hub_looted_402_bathroom", file)) looted_402_bathroom = ES3.Load<bool>("hub_looted_402_bathroom", file);

            if (ES3.KeyExists("hub_looted_402_ring", file)) looted_402_ring = ES3.Load<bool>("hub_looted_402_ring", file);

            if (ES3.KeyExists("hub_took_402_photo", file)) took_402_photo = ES3.Load<bool>("hub_took_402_photo", file);

            if (ES3.KeyExists("hub_read_402_note", file)) read_402_note = ES3.Load<bool>("hub_read_402_note", file);



            if (ES3.KeyExists("hub_room401_entered", file)) room401_entered = ES3.Load<bool>("hub_room401_entered", file);

            if (ES3.KeyExists("hub_shelf_lifted", file)) shelf_lifted = ES3.Load<bool>("hub_shelf_lifted", file);

            if (ES3.KeyExists("hub_turned_off_music", file)) turned_off_music = ES3.Load<bool>("hub_turned_off_music", file);

            if (ES3.KeyExists("hub_looted_401_kitchen", file)) looted_401_kitchen = ES3.Load<bool>("hub_looted_401_kitchen", file);

            if (ES3.KeyExists("hub_looted_401_bathroom", file)) looted_401_bathroom = ES3.Load<bool>("hub_looted_401_bathroom", file);

            if (ES3.KeyExists("hub_looted_401_alcohol", file)) looted_401_alcohol = ES3.Load<bool>("hub_looted_401_alcohol", file);

            if (ES3.KeyExists("hub_looted_401_pockets", file)) looted_401_pockets = ES3.Load<bool>("hub_looted_401_pockets", file);



            if (ES3.KeyExists("hub_utility_opened", file)) utility_opened = ES3.Load<bool>("hub_utility_opened", file);

            if (ES3.KeyExists("hub_read_utility_log", file)) read_utility_log = ES3.Load<bool>("hub_read_utility_log", file);

            if (ES3.KeyExists("hub_looted_utility_bleach", file)) looted_utility_bleach = ES3.Load<bool>("hub_looted_utility_bleach", file);

            if (ES3.KeyExists("hub_looted_utility_tape", file)) looted_utility_tape = ES3.Load<bool>("hub_looted_utility_tape", file);

            if (ES3.KeyExists("hub_looted_utility_flashlight", file)) looted_utility_flashlight = ES3.Load<bool>("hub_looted_utility_flashlight", file);



            if (ES3.KeyExists("hub_stair_phase", file)) stair_phase = ES3.Load<int>("hub_stair_phase", file);

            if (ES3.KeyExists("hub_descent_method", file)) descentMethod = ES3.Load<string>("hub_descent_method", file);

            if (ES3.KeyExists("hub_barricade_pushed", file)) barricade_pushed = ES3.Load<bool>("hub_barricade_pushed", file);

            if (ES3.KeyExists("hub_barricade_inspected", file)) barricade_inspected = ES3.Load<bool>("hub_barricade_inspected", file);

            if (ES3.KeyExists("hub_stair_3f_door_checked", file)) stair_3f_door_checked = ES3.Load<bool>("hub_stair_3f_door_checked", file);

            if (ES3.KeyExists("hub_stair_listened_2f_door", file)) stair_listened_2f_door = ES3.Load<bool>("hub_stair_listened_2f_door", file);

            if (ES3.KeyExists("hub_stair_noise_made", file)) stair_noise_made = ES3.Load<bool>("hub_stair_noise_made", file);

            if (ES3.KeyExists("hub_stair_hum_deducted", file)) stair_hum_deducted = ES3.Load<bool>("hub_stair_hum_deducted", file);



            phoneVisited = ES3.KeyExists("hub_phone_visited", file) ? ES3.Load<bool>("hub_phone_visited", file) : false;

            radioVisited = ES3.KeyExists("hub_radio_visited", file) ? ES3.Load<bool>("hub_radio_visited", file) : false;

            hackingVisited = ES3.KeyExists("hub_hacking_visited", file) ? ES3.Load<bool>("hub_hacking_visited", file) : false;



            if (currentPlayer != null)

            {

                currentPlayer.SetFlag("checked_phone_harmon", phoneVisited);

                currentPlayer.SetFlag("room406_opened", room406_opened);

                currentPlayer.SetFlag("room406_locked_out", room406_locked_out);

            }



            if (ES3.KeyExists("hub_lore_entries", file))

            {

                var loadedLore = ES3.Load<List<SundayLastBreath.Core.LoreEntry>>("hub_lore_entries", file);

                if (loreSystem != null)

                {

                    loreSystem.SetEntries(loadedLore);

                }

            }

            else

            {

                if (loreSystem != null)

                {

                    loreSystem.Clear();

                }

            }

            RefreshPDALoreTab();



            if (ES3.KeyExists("hub_story_logs", file))

            {

                _storyLogs.Clear();

                var loadedLogs = ES3.Load<List<string>>("hub_story_logs", file);

                if (loadedLogs != null) _storyLogs.AddRange(loadedLogs);

            }

            else

            {

                _storyLogs.Clear();

                InitializeStoryLogs();

            }

            RefreshStoryLogUI();

            SyncLastAutoLoggedSituationFromLogs();

            _visitedLocationAnchors.Clear();
            _returnVariantIndices.Clear();
            if (ES3.KeyExists("hub_visited_anchors", file))
            {
                var loadedAnchors = ES3.Load<List<string>>("hub_visited_anchors", file);
                if (loadedAnchors != null)
                {
                    foreach (string a in loadedAnchors)
                    {
                        if (!string.IsNullOrEmpty(a)) _visitedLocationAnchors.Add(a);
                    }
                }
            }

        }



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



            // Ensure console is in narrative mode

            currentConsoleMode = ConsoleMode.Narrative;

            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);



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

            Transform parent = leaveApartmentButton != null ? leaveApartmentButton.transform.parent : null;

            if (parent == null && hubMenuContainer != null)

            {

                ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);

                if (sr != null && sr.content != null) parent = sr.content;

            }

            if (parent != null)

            {

                for (int i = 0; i < parent.childCount; i++)

                {

                    var child = parent.GetChild(i);

                    if (child.name.StartsWith("HallwayChoice_"))

                    {

                        var tcb = child.GetComponent<TacticalChoiceButton>();

                        if (tcb != null) tcb.ResetVisuals();

                        child.gameObject.SetActive(false);

                    }

                }

            }



            for (int i = 0; i < hallwayChoiceButtons.Count; i++)

            {

                if (hallwayChoiceButtons[i] != null)

                {

                    hallwayChoiceButtons[i].ResetVisuals();

                    hallwayChoiceButtons[i].gameObject.SetActive(false);

                }

            }

        }



        private TacticalChoiceButton SetHallwayChoice(int index, string mainText, System.Action onClick, bool interactable = true, Color? accent = null)

        {

            TacticalChoiceButton btn = GetOrCreateHallwayChoiceButton(index);

            if (btn != null)

            {

                btn.SetChoice(mainText, onClick, interactable, accent);

                if (interactable)

                {

                    btn.SetChoiceNumber(index + 1);

                }

                else

                {

                    btn.SetDisabledChoiceTag();

                }

            }

            return btn;

        }



        /// <summary>

        /// Modular CRPG Choice Helper:

        /// - If condition is met: renders an enabled, 100% deterministic success choice.

        /// - If condition is NOT met: renders a dimmed, non-clickable Twine-style preview showing the requirement and current stat gap.

        /// </summary>

        private void AddModularChoice(

            ref int choiceIdx,

            bool conditionMet,

            string tag,

            string actionText,

            int staminaCost,

            int timeMinutes,

            System.Action onExecute,

            string failRequirementDesc

        )

        {

            if (conditionMet)

            {

                string text = $"{tag} {actionText} <color=#10B981>(-{staminaCost} Stamina, 100% Guaranteed)</color>";

                SetHallwayChoice(choiceIdx++, text, () =>

                {

                    ExertStamina(staminaCost);

                    if (timeMinutes > 0) gameClock?.AddTime(timeMinutes);

                    onExecute?.Invoke();

                }, true);

            }

            else

            {

                string text = $"<s>{tag} {actionText}</s> <color=#64748B><i>[{failRequirementDesc}]</i></color>";

                SetHallwayChoice(choiceIdx++, text, null, false);

            }

        }



        private TacticalChoiceButton GetOrCreateHallwayChoiceButton(int index)

        {

            Transform parent = leaveApartmentButton != null ? leaveApartmentButton.transform.parent : null;

            if (parent == null && hubMenuContainer != null)

            {

                ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);

                if (sr != null && sr.content != null) parent = sr.content;

            }



            string targetName = $"HallwayChoice_{index + 1}";

            if (parent != null)

            {

                Transform existing = parent.Find(targetName);

                if (existing != null)

                {

                    var existingTcb = existing.GetComponent<TacticalChoiceButton>();

                    if (existingTcb != null)

                    {

                        while (hallwayChoiceButtons.Count <= index) hallwayChoiceButtons.Add(null);

                        hallwayChoiceButtons[index] = existingTcb;

                        return existingTcb;

                    }

                }

            }



            if (index < hallwayChoiceButtons.Count && hallwayChoiceButtons[index] != null)

            {

                return hallwayChoiceButtons[index];

            }



            Button templateBtn = packGearButton != null ? packGearButton : leaveApartmentButton;

            if (templateBtn != null && parent != null)

            {

                GameObject clone = Instantiate(templateBtn.gameObject, parent);

                clone.name = targetName;

                TacticalChoiceButton tcb = clone.GetComponent<TacticalChoiceButton>();

                if (tcb != null)

                {

                    Image bg = clone.GetComponent<Image>();

                    if (bg != null) bg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);

                    tcb.SetChoiceNumber(index + 1);

                    tcb.ResetVisuals();

                    while (hallwayChoiceButtons.Count <= index) hallwayChoiceButtons.Add(null);

                    hallwayChoiceButtons[index] = tcb;

                    return tcb;

                }

            }

            return null;

        }



        private Dictionary<string, List<Sprite>> animFrameCache = new Dictionary<string, List<Sprite>>();



        /// <summary>

        /// Plays sequential frame animations (extracted from Twine GIFs) on the stage backdrop.

        /// If frames are missing or folder not found, falls back cleanly to the static PNG sprite.

        /// </summary>

        public void PlayStageAnimation(string animFolderName, string fallbackStaticSpriteName, float fps = 8f)

        {

            if (stageBackdropImage == null) return;

            stageBackdropImage.enabled = true;

            stageBackdropImage.color = Color.white;



            var anim = stageBackdropImage.GetComponent<SundayLastBreath.UI.FrameAnimationPlayer>();

            if (anim == null) anim = stageBackdropImage.gameObject.AddComponent<SundayLastBreath.UI.FrameAnimationPlayer>();



            List<Sprite> frames = null;

            if (!string.IsNullOrEmpty(animFolderName))

            {

                if (!animFrameCache.TryGetValue(animFolderName, out frames) || frames == null || frames.Count == 0)

                {

                    Sprite[] loaded = Resources.LoadAll<Sprite>("Act1/" + animFolderName);

                    if (loaded != null && loaded.Length > 0)

                    {

                        frames = new List<Sprite>(loaded);

                    }

#if UNITY_EDITOR

                    if (frames == null || frames.Count == 0)

                    {

                        string folderPath = "Assets/Resources/Act1/" + animFolderName;

                        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

                        if (guids != null && guids.Length > 0)

                        {

                            frames = new List<Sprite>();

                            foreach (var guid in guids)

                            {

                                string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                                Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);

                                if (s != null) frames.Add(s);

                            }

                        }

                    }

#endif

                    if (frames != null && frames.Count > 0)

                    {

                        frames.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

                        animFrameCache[animFolderName] = frames;

                    }

                }

            }



            if (frames != null && frames.Count > 1)

            {

                anim.enabled = true;

                anim.SetFrames(frames, fps, true);

                anim.Play();

            }

            else

            {

                if (anim != null)

                {

                    anim.Stop();

                    anim.enabled = false;

                }

                if (!string.IsNullOrEmpty(fallbackStaticSpriteName))

                {

                    stageBackdropImage.sprite = GetAct1Sprite(fallbackStaticSpriteName);

                }

            }

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

                    PlayStageAnimation("Hall406_407_Anim", "hall_406_407", 8f);



                    ShowLocationAnchor(
                        "hallway_1",
                        "The door clicks shut behind you, locking you out of the only place you felt safe.\n\n<color=#93C5FD><b>UPPER HALLWAY.</b></color> One of the emergency bulbs overhead is buzzing and blinking on and off. The air feels still, so every siren outside comes through the hallway vents. Your unit (407) is to your right. Across the carpet is <b>Unit 406 (Professor Harmon)</b>.",
                        "<color=#93C5FD><b>UPPER HALLWAY.</b></color> Back at the north end of the 4th floor. The ceiling bulb still buzzes weakly over the carpet between your locked door (407) and Unit 406.",
                        $"<color=#93C5FD><b>UPPER HALLWAY.</b></color> Your front door (407) stays latched tight. Across the hall, Harmon's apartment (406) {(room406_opened ? "stands unlocked in the dark." : "still glows faintly at its blue keypad.")}",
                        "<color=#93C5FD><b>UPPER HALLWAY.</b></color> Dust hangs still under the blinking bulb while distant street sirens echo softly down through the hallway vents."
                    );



                    SetHallwayChoice(choiceIdx++, "INVESTIGATE UNIT 406 <color=#9CA3AF>— Professor Harmon's door. Electronic keypad. Silence inside.</color>", () =>

                    {

                        if (room406_opened) PlayClickSFX();

                        InspectUnit406();

                    });

                    SetHallwayChoice(choiceIdx++, "EXAMINE UNIT 407 (YOUR DOOR) <color=#9CA3AF>— Your apartment door. Locked tight behind you.</color>", () => InspectUnit407Exterior());

                    SetHallwayChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Walk south toward Units 404 & 405.</color>", () => AdvanceDownHall());

                    break;



                case 2:

                    SetLocation("hallway_2", "CORRIDOR M // MID-HALLWAY (SEC 2)");

                    tacticalMapController?.DiscoverAdjacentRooms("hallway_2");

                    PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);



                    string midHallAudio = spoke_to_mrs_gable

                        ? "Behind Mrs. Gable's door (404), Buster lets out a soft, calmer whine. Across from her, the reinforced door of <b>Unit 405 (Marcus)</b> isn't moving."

                        : "Behind Mrs. Gable's door (404), Buster is barking nonstop and scratching at the bottom wood. Across from her, the reinforced door of <b>Unit 405 (Marcus)</b> sits slightly open.";



                    ShowLocationAnchor(
                        "hallway_2",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> The hallway light here barely holds steady. {midHallAudio} There's a faint smell of gun oil and burnt powder in the air.",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> You step back into the middle stretch of the hall. {(spoke_to_mrs_gable ? "Mrs. Gable's apartment (404) stays quiet apart from Buster breathing behind the door." : "Buster is still barking and scratching behind Unit 404.")}",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> Under the uneven hallway light, {(room405_opened ? "Marcus's reinforced door (405) sits unlocked." : "Marcus's heavy steel door (405) remains cracked a fraction of an inch.")} The sharp scent of gun oil still hangs here.",
                        "<color=#93C5FD><b>MID-HALLWAY.</b></color> Nothing moved in this stretch of the corridor while you were gone—just buzzing wiring and faint sirens through the vents."
                    );



                    string unit405Label = room405_opened

                        ? "ENTER UNIT 405 <color=#9CA3AF>— Marcus's apartment. Door open, safe and bench inside.</color>"

                        : "INVESTIGATE UNIT 405 <color=#9CA3AF>— Marcus's place. Heavy steel door ajar. Scent of cordite.</color>";



                    SetHallwayChoice(choiceIdx++, unit405Label, () =>

                    {

                        if (room405_opened) PlayClickSFX();

                        InspectUnit405();

                    });



                    string unit404Label = spoke_to_mrs_gable

                        ? "CHECK UNIT 404 <color=#9CA3AF>— Mrs. Gable's door. Reassured; barricaded with Buster.</color>"

                        : "CHECK UNIT 404 <color=#9CA3AF>— Mrs. Gable's door. Barricaded inside; dog barking.</color>";



                    SetHallwayChoice(choiceIdx++, unit404Label, () => InspectUnit404());

                    SetHallwayChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Upper Hallway & Unit 407.</color>", () => RetreatUpHall());

                    SetHallwayChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Lower Hallway.</color>", () => AdvanceDownHall());

                    break;



                case 3:

                    SetLocation("hallway_3", "CORRIDOR S // LOWER HALLWAY (SEC 3)");

                    tacticalMapController?.DiscoverAdjacentRooms("hallway_3");

                    PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);



                    string lowerHallAudioWarning = "";



                    ShowLocationAnchor(
                        "hallway_3",
                        $"<color=#93C5FD><b>LOWER HALLWAY.</b></color> The ceiling lights here are completely dead. A sharp smell of bleach drifts out from under one of the doors. Soft classical piano is playing behind the chained door of <b>Unit 402 (The Andersons)</b>. Across the hall, <b>Unit 403 (Sarah)</b> is dark and quiet.{lowerHallAudioWarning}",
                        $"<color=#93C5FD><b>LOWER HALLWAY.</b></color> Back in the dark stretch between Units 402 and 403. Slow piano notes still drift softly under the Andersons' doorframe.{lowerHallAudioWarning}",
                        $"<color=#93C5FD><b>LOWER HALLWAY.</b></color> A chemical bleach smell still bites the back of your throat. {(room402_opened ? "Unit 402's door sits unchained." : "Unit 402 remains held on its chain, while Sarah's door (403) stays dark.")}",
                        "<color=#93C5FD><b>LOWER HALLWAY.</b></color> Dead overhead fixtures leave this corner in near darkness. Only the piano record in 402 breaks the stillness."
                    );



                    string unit402Label = room402_opened

                        ? "ENTER UNIT 402 <color=#9CA3AF>— The Andersons' flat. Chain bypassed. Piano playing.</color>"

                        : "INVESTIGATE UNIT 402 <color=#9CA3AF>— The Andersons' door. Chained from the inside. Classical music.</color>";



                    SetHallwayChoice(choiceIdx++, unit402Label, () =>

                    {

                        if (room402_opened) PlayClickSFX();

                        InspectUnit402();

                    });

                    SetHallwayChoice(choiceIdx++, "CHECK UNIT 403 <color=#9CA3AF>— Sarah's apartment. Deadbolted from the outside; dark inside.</color>", () => InspectUnit403());

                    SetHallwayChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Mid-Hallway.</color>", () => RetreatUpHall());

                    SetHallwayChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Hallway End.</color>", () => AdvanceDownHall());

                    break;



                case 4:

                    SetLocation("hallway_4", "CORRIDOR E // HALLWAY END (SEC 4)");

                    tacticalMapController?.DiscoverAdjacentRooms("hallway_4");

                    PlayStageAnimation("HallEnd_Anim", "hall_end", 8f);



                    string hall4NoiseText = turned_off_music

                        ? "<b>Unit 401 (The College Students)</b> is finally quiet with the stereo shut off, leaving the end of the hall dead silent."

                        : "From <b>Unit 401 (The College Students)</b>, loud pop music thumps right through the wood, drowning out everything else.";



                    ShowLocationAnchor(
                        "hallway_4",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> The hall ends at the heavy red fire doors to the stairwell. Dried bloody handprints smear the small glass window on the door. {hall4NoiseText} To your left is Carl's <b>Utility Closet</b>.",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> You step back up by the red stairwell doors. {(turned_off_music ? "With Unit 401's stereo shut off, the end of the floor is dead silent." : "Loud pop music is still thumping through Unit 401's open doorway.")}",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> Smeared handprints still mark the stairwell window glass. {(utility_opened ? "Carl's maintenance door stands unlatched on your left." : "To your left, the maintenance mag-lock hums steadily.")}",
                        "<color=#93C5FD><b>HALLWAY END.</b></color> Worn hall carpet meets bare concrete in front of the stairwell frame and service closet."
                    );



                    string unit401Label;

                    if (!room401_entered)

                        unit401Label = "INVESTIGATE UNIT 401 <color=#9CA3AF>— The Party flat. Door wide open. Music inside.</color>";

                    else if (turned_off_music)

                        unit401Label = "ENTER UNIT 401 <color=#9CA3AF>— The Party flat. Stereo silenced. Scavenged living room.</color>";

                    else

                        unit401Label = "ENTER UNIT 401 <color=#9CA3AF>— The Party flat. Pop music blaring inside.</color>";



                    SetHallwayChoice(choiceIdx++, unit401Label, () =>

                    {

                        if (!room401_entered)

                        {

                            room401_entered = true;

                            PlayDoorTransition(

                                "Videos/unit401_door_transition",

                                "[ UNIT 401 // THE PARTY RESIDENCE ]",

                                "You push open the cracked front door.\n<color=#94A3B8>Pop music thuds through the corridor as the hallway lights reflect on empty cups...</color>",

                                false,

                                () => InspectUnit401(),

                                false

                            );

                        }

                        else

                        {

                            PlayClickSFX();

                            InspectUnit401();

                        }

                    });

                    string utilityLabel = utility_opened

                        ? "ENTER UTILITY CLOSET <color=#9CA3AF>— Maintenance workshop. Magnetic lock bypassed.</color>"

                        : "CHECK UTILITY CLOSET <color=#9CA3AF>— Maintenance room. Industrial steel door and magnetic lock.</color>";

                    SetHallwayChoice(choiceIdx++, utilityLabel, () =>

                    {

                        if (utility_opened) PlayClickSFX();

                        InspectUtilityCloset();

                    });

                    SetHallwayChoice(choiceIdx++, "<color=#EF4444>ENTER STAIRWELL (EXIT)</color> <color=#FCA5A5>— Point of no return. Push through bloody fire doors.</color>", () =>

                    {

                        PlayDoorTransition(

                            "Videos/stairs_door_transition",

                            "[ EMERGENCY STAIRWELL // EXIT ]",

                            "You push hard against the emergency crash bar.\n<color=#94A3B8>The heavy fire door swings open into the stairwell darkness...</color>",

                            false,

                            () => InspectStairwell(),

                            true

                        );

                    }, true, new Color(0.95f, 0.25f, 0.25f));

                    SetHallwayChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Lower Hallway.</color>", () => RetreatUpHall());

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

                PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.65f);

                UpdateHallwayView();

            }

        }



        private void RetreatUpHall()

        {

            if (hallwaySection > 1)

            {

                hallwaySection--;

                gameClock?.AddTime(1);

                PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.65f);

                UpdateHallwayView();

            }

        }



        public void InspectUnit407Exterior()

        {

            currentInspectedRoom = "407";

            ClearHallwayChoices();

            int choiceIdx = 0;



            SetLocation("hallway_1", "YOUR APARTMENT (DOORWAY)", "unit_407");

            UpdateLocationDisplay("UNIT 407", "YOUR APARTMENT (DOORWAY)");

            PlayStageAnimation("Hall406_407_Anim", "hall_406_407", 8f);

            PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.9f);



            ShowLocationAnchor(
                "ext_407",
                "You grab the brass knob of Unit 407 and twist. It doesn't budge.\n\n<color=#EF4444><b>*CLACK*</b></color>\n\nThe deadbolt snapped back into place when the door shut behind you, and your key only works that latch from the inside—trying to kick your own door in now would bring whatever's downstairs straight up here.\n\nNo going back inside. The only way is down the hall.",
                "You test the brass knob of Unit 407 again. Locked deadbolt-tight—your apartment key only turns that latch from the inside.",
                "Still shut solid. No getting back into 407 without tearing the frame apart."
            );



            SetHallwayChoice(choiceIdx++, "[BACK] Return to Upper Hallway", () => EnterHallwayMode(1));

            ReindexActiveChoiceHotkeys();

        }



        private void OpenUnit406Door()

        {

            room406_opened = true;

            PlayClickSFX();

            PlayDoorTransition(

                "Videos/unit406_door_transition",

                "[ UNIT 406 // PROFESSOR'S STUDY ]",

                "The electronic keypad beeps twice with a soft green indicator light.\n<color=#94A3B8>The heavy internal deadbolt retracts, and the door clicks open into darkness...</color>",

                false,

                () => InspectUnit406(),

                false

            );

        }



        public void InspectUnit406(string actionNarration = null)

        {

            currentInspectedRoom = "406";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (!room406_opened)

            {

                SetLocation("hallway_1", "THE KEYPAD", "unit_406");

                UpdateLocationDisplay("UNIT 406", "THE KEYPAD");

                PlayStageAnimation("Unit406Door_Anim", "unit406_door", 8f);

                PlayClickSFX();



                string lockoutMsg = room406_locked_out 

                    ? "\n\n<color=#EF4444><b>[LOCKED OUT] PERMANENTLY LOCKED OUT.</b> The keypad triggered its security failsafe. The software is sealed. You will have to bypass it physically.</color>"

                    : (failed_406_attempts > 0 ? $"\n\n<color=#F59E0B>[!] WRONG CODE. Attempts remaining: {3 - failed_406_attempts}.</color>" : "");



                string noteProse = "Taped above the keypad is a yellow index card in his neat, cramped handwriting: <i>\"If you're still on this floor, I posted my door code on the building feed before heading out. Grab what you need from the desk and get out before morning.\"</i>";



                ShowLocationAnchor(
                    "ext_406",
                    $"Professor Harmon taught sociology at the university—mostly on how cities hold together when things fall apart. His door is locked with a commercial keypad, the blue digits glowing faintly in the dark hall. Four blank digits. Waiting.\n\n{noteProse}{lockoutMsg}",
                    $"You step back up to Harmon's door. Blue keypad numbers glow quietly in the dark hall.{lockoutMsg}",
                    $"Unit 406's yellow index card still hangs taped above the keypad. Four blank digits wait for a code.{lockoutMsg}"
                );



                bool hasPhoneCode = phoneVisited || (currentPlayer != null && currentPlayer.HasFlag("checked_phone_harmon"));



                if (!room406_locked_out)
                {
                    SetHallwayChoice(choiceIdx++, "[CODE] Enter keypad code manually <color=#9CA3AF>— Four digits. 3 attempts limit.</color>", () =>
                    {
                        gameClock?.AddTime(1);
                        OpenKeypadPuzzleModal(null);
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

                        OpenUnit406Door();

                    });

                }



                if (hasLocksmith)

                {

                    SetHallwayChoice(choiceIdx++, "Exploit mechanical failsafe [LOCKSMITH] <color=#F59E0B>(-7 Stamina, ~3 min)</color>", () =>

                    {

                        ExertStamina(7);

                        gameClock?.AddTime(3);

                        OpenUnit406Door();

                    });

                }



                // 1. Phone Intel / Code

                AddModularChoice(

                    ref choiceIdx,

                    hasPhoneCode,

                    "[PHONE INTEL]",

                    hasPhoneCode ? "Punch in Harmon's door code (8445)" : "Punch in Harmon's door code",

                    0, 1,

                    () =>

                    {

                        AppendStoryLog(gameClock?.GetTimeString() ?? "00:15", "UNIT 406 // KEYPAD UNLOCKED", "Used Professor Harmon's emergency keypad code (8445) from his phone feed. Deadbolt retracted.");

                        OpenUnit406Door();

                    },

                    "Requires Harmon's Door Code from Phone Feed — You haven't checked his emergency feed"

                );



                // 2. Tech 2 Knowledge

                AddModularChoice(

                    ref choiceIdx,

                    hasTech2,

                    "[TECH 2]",

                    "Bypass keypad circuit board",

                    5, 4,

                    () => OpenUnit406Door(),

                    $"Requires Tech 2 (You have {currentPlayer?.tech ?? 0}) — You lack knowledge of circuit bypasses"

                );



                // 3. Multi-Tool

                AddModularChoice(

                    ref choiceIdx,

                    hasMultiTool,

                    "[MULTI-TOOL]",

                    "Disassemble the housing cleanly",

                    5, 3,

                    () => OpenUnit406Door(),

                    "Requires Multi-Tool — You have no precision screwdrivers to remove security screws"

                );



                // 5. Might 6 Guaranteed vs Fallback Gamble

                if (strVal >= 6)

                {

                    SetHallwayChoice(choiceIdx++, $"[MIGHT {strVal}/6] Smash keypad housing <color=#10B981>(-10 Stamina, 100% Guaranteed, ~2 min)</color>", () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(2);

                        OpenUnit406Door();

                    }, true);

                }

                else

                {

                    int chance = Mathf.Clamp(strVal * 10, 10, 35);

                    SetHallwayChoice(choiceIdx++, $"[DESPERATE PRY] Wedge flat blade into lock frame [Might {strVal}/6 — {chance}% Chance] <color=#EF4444>(-15 Stamina, Injury Risk)</color>", () =>

                    {

                        ExertStamina(15);

                        gameClock?.AddTime(3);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            OpenUnit406Door();

                        }

                        else

                        {

                            PlayBeepSFX();

                            TakeDamage(10, "Blunt trauma prying steel deadbolt");

                            SetSituationNarration("<color=#EF4444><b>BRUTE FORCE FAILED (-10 HP, -15 Stamina).</b></color> You battered the housing, but the internal deadbolt didn't budge. Your hands are bruised and bleeding.");

                        }

                    }, true);

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Give up and return to hallway", () => EnterHallwayMode(1), true);

            }

            else

            {

                SetLocation("unit_406", "PROFESSOR'S STUDY");

                PlayStageAnimation("Unit406Inside_Anim", "unit406", 8f);

                PlayHubBGM("Audio/SFX/tick", 0.25f, true);



                bool all406Done = studied_406_mechanics && studied_406_anatomy && read_406_journal && looted_406_kitchen && looted_406;

                string completionNotice = all406Done

                    ? "\n\n<color=#10B981><i>You've stripped the room of everything valuable — knowledge and supplies both. The rest is just dead weight.</i></color>"

                    : "";



                string roomAnchor = "Floor-to-ceiling shelves line all four walls, smelling of old paper and peppermint. On the table, Harmon's reading glasses rest on <i>The Fall of Rome</i> beside a yellow sticky note:\n<i>\"Every empire ends. We're just living in the intermission. Take what you need.\"</i>";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration($"{actionNarration}{completionNotice}");

                }

                else
                {
                    ShowLocationAnchor(
                        "int_406",
                        $"The door swings open into darkness. The smell hits first: old paper, dust, and peppermint. Desk drawers hang open, and his heavy coat and hiking pack are gone.\n\n{roomAnchor}{completionNotice}",
                        $"You step back into Harmon's dark study. Tall bookshelves still smell faintly of dust and peppermint.{completionNotice}",
                        $"Back inside Unit 406. His reading glasses still sit on <i>The Fall of Rome</i> across the desk.{completionNotice}"
                    );
                }



                if (!studied_406_mechanics)

                {

                    SetHallwayChoice(choiceIdx++, "[STUDY] Skim 'Advanced Mechanics' notes <color=#10B981>[TECH +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>

                    {

                        studied_406_mechanics = true;

                        gameClock?.AddTime(10);

                        if (currentPlayer != null) currentPlayer.tech += 1;

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit406("<color=#10B981>[TECH +1]</color> You pull a worn repair manual off the shelf and flip to the bookmarked pages. Harmon sketched clear wiring notes in the margins—starter relays, basic door latches, and how to jump them without frying the board. You memorize the basics, slide the book back into place, and step back into the room.");

                    });

                }



                if (!studied_406_anatomy)

                {

                    SetHallwayChoice(choiceIdx++, "[STUDY] Skim 'Gray's Anatomy' notes <color=#10B981>[MEDICINE +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>

                    {

                        studied_406_anatomy = true;

                        gameClock?.AddTime(10);

                        if (currentPlayer != null) currentPlayer.medicine += 1;

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit406("<color=#10B981>[MEDICINE +1]</color> You pull a dog-eared anatomy textbook off the shelf. Under a bookmarked page, Harmon underlined one note twice in red pen: <color=#94A3B8>\"People coming into the ER weren't slowing down from broken ribs or blood loss. Only hard trauma to the upper neck or skull put them down.\"</color> You lock the note into memory, put the book back, and step back to the middle of the room.");

                    });

                }



                if (!read_406_journal)

                {

                    SetHallwayChoice(choiceIdx++, "[NOTE] Read personal journal <color=#10B981>[PER +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>

                    {

                        read_406_journal = true;

                        gameClock?.AddTime(10);

                        if (currentPlayer != null) currentPlayer.per += 1;

                        loreSystem?.AddLore("Professor Harmon's Journal", "His handwriting gets messier near the back. By 22:30 he was jotting down police scanner chatter and rumors from his doctor friends: 'Half the downtown ERs stopped answering calls at nine. City hall keeps telling reporters it's a bad flu strain and bad shellfish, while patrol cars are shutting down the on-ramps. Nobody at the top is telling the same story.'", gameClock?.GetTimeString() ?? "00:15");

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit406("<color=#10B981>[PERCEPTION +1]</color> You walk over to the mahogany desk and open his leather notebook. The last entry, timed at 22:30, is scribbled fast across the lined paper:\n\n<color=#94A3B8>\"Two nurses I know downtown stopped answering at nine. Evening news called it a bad flu and contaminated water, while the police scanner is calling for roadblocks on I-5. Whatever started tonight, nobody's getting the same story twice. Don't wait until morning to get out.\"</color>\n\nYou close the cover and step back into the middle of the room.");

                    });

                }



                if (!looted_406_kitchen)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Search abandoned kitchen <color=#9CA3AF>(Canned Food x1, Energy Bar x1, Bottled Water x2)</color>", () =>

                    {

                        looted_406_kitchen = true;

                        gameClock?.AddTime(5);

                        GiveOrDropItem("Canned Food", 1);

                        GiveOrDropItem("Energy Bar", 1);

                        GiveOrDropItem("Bottled Water", 2);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit406("<color=#F59E0B>Kitchen pantry raided.</color> You walk into the small kitchenette and check the cupboards. Harmon packed light for speed, leaving canned food, an energy bar, and two sealed bottles of water on the counter. You pack them into your bag and return to the center of the study.");

                    });

                }



                if (!looted_406)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Raid desk drawers <color=#9CA3AF>(Encyclopedia x1, Flashlight x1)</color>", () =>

                    {

                        looted_406 = true;

                        gameClock?.AddTime(5);

                        GiveOrDropItem("Encyclopedia", 1);

                        GiveOrDropItem("Flashlight", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit406("<color=#38BDF8>Desk drawers looted.</color> You crouch beside the oak desk and pull open the deep drawers beneath lecture drafts. Inside, you find a heavy hardbound Encyclopedia—solid enough to crush bone in a pinch—and a fully charged metal Flashlight. You slide the drawer shut and stand back in the middle of the room.");

                    });

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave study and return to hallway", () => EnterHallwayMode(1));

            }



            ReindexActiveChoiceHotkeys();

        }



        private void OpenUnit405Door(bool triggered = false)

        {

            room405_opened = true;

            PlayClickSFX();



            string subtitle = triggered

                ? "The booby trap detonated in an ear-shattering blast.\n<color=#EF4444>Ears ringing, you push past the scorched doorframe into the gunsmith's quarters...</color>"

                : "The tripwire mechanism is safely neutralised.\n<color=#94A3B8>The reinforced steel door slowly swings open into the gunsmith's quarters...</color>";



            PlayDoorTransition(

                "Videos/unit405_door_transition",

                "[ UNIT 405 // MARCUS'S APARTMENT ]",

                subtitle,

                false,

                () => InspectUnit405(),

                false

            );

        }



        public void InspectUnit405(string actionNarration = null)

        {

            currentInspectedRoom = "405";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (!room405_opened)

            {

                SetLocation("hallway_2", "THE BOOBY TRAP", "unit_405");

                UpdateLocationDisplay("UNIT 405", "THE BOOBY TRAP");

                PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);

                PlayHubBGM("Audio/BGM/tension_drone", 0.4f);



                string trapStatus = room405_trap_triggered 

                    ? "\n\n<color=#EF4444>[TRAP TRIGGERED] Your ears are ringing. The blank shell detonated, echoing down the corridor. The wire is burned away.</color>" 

                    : "";



                ShowLocationAnchor(
                    "ext_405",
                    $"This is Marcus's apartment. The quiet veteran down the hall spent his weekends cleaning hunting gear and reinforcing his doorframes. His heavy steel door isn't latched—it sits cracked open by a fraction of an inch, leaking the sharp smell of gun oil and burnt powder into the hall.\n\nYou nudge the door gently with one knuckle. It catches after half an inch with a small metallic click. Angling your flashlight through the gap shows a thin fishing line tied from the inside handle to a pull-pin canister and a shotgun blank clamped to a pipe. Shoving this open will set both off in your face and draw everything on this floor.{trapStatus}",
                    $"Marcus's reinforced steel door still sits open a fraction of an inch. Gun oil and burnt powder hang sharp in the hall.{trapStatus}",
                    $"Back at Unit 405's threshold. Through the narrow crack, the taut fishing line still waits behind the handle.{trapStatus}"
                );



                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);

                bool hasLocksmith = currentPlayer != null && currentPlayer.HasTrait("LOCKSMITH");

                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;

                bool hasMultiTool = currentPlayer != null && currentPlayer.HasItem("Multi-Tool");

                int perVal = currentPlayer != null ? currentPlayer.per : 5;



                // 1. Burglar or Locksmith

                AddModularChoice(

                    ref choiceIdx,

                    isBurglar || hasLocksmith,

                    "[BURGLAR/LOCKSMITH]",

                    "Jam the firing pin mechanism with a shim",

                    10, 2,

                    () => OpenUnit405Door(false),

                    "Requires Burglar or Locksmith — You lack the delicate touch to block an armed firing pin"

                );



                // 2. Tech 2

                AddModularChoice(

                    ref choiceIdx,

                    hasTech2,

                    "[TECH 2]",

                    "Release tension spring without tripping trigger",

                    10, 3,

                    () => OpenUnit405Door(false),

                    $"Requires Tech 2 (You have {currentPlayer?.tech ?? 0}) — You don't understand the mechanical trigger assembly"

                );



                // 3. Multi-Tool

                AddModularChoice(

                    ref choiceIdx,

                    hasMultiTool,

                    "[MULTI-TOOL]",

                    "Cut and clamp the monofilament tripwire",

                    10, 2,

                    () => OpenUnit405Door(false),

                    "Requires Multi-Tool / Wire Cutters — Cannot cut taut monofilament safely without pliers"

                );



                // 4. Observation 6 vs Fallback Squeeze

                if (perVal >= 6)

                {

                    SetHallwayChoice(choiceIdx++, $"[OBSERVATION {perVal}/6] Squeeze carefully through the gap <color=#10B981>(-15 Stamina, 100% Guaranteed, ~4 min)</color>", () =>

                    {

                        ExertStamina(15);

                        gameClock?.AddTime(4);

                        OpenUnit405Door(false);

                    }, true);

                }

                else

                {

                    int chance = Mathf.Clamp(perVal * 8, 15, 45);

                    SetHallwayChoice(choiceIdx++, $"[RISKY SQUEEZE] Squeeze past monofilament [Observation {perVal}/6 — {chance}% Chance] <color=#EF4444>(-20 Stamina, Lethal Detonation Risk)</color>", () =>

                    {

                        ExertStamina(20);

                        gameClock?.AddTime(4);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            OpenUnit405Door(false);

                        }

                        else

                        {

                            room405_trap_triggered = true;

                            PlaySFX("Audio/SFX/firedoor_slam", 0.9f);

                            PlaySFX("Audio/SFX/elbow_smash_glass", 0.85f);

                            PlaySFX("Audio/SFX/distant_explosion", 0.7f);

                            TakeDamage(25, "12-gauge blank blast & flashbang detonation");

                            AppendStoryLog(gameClock?.GetTimeString() ?? "00:20", "UNIT 405 // BOOBY TRAP DETONATED", "Detonated Marcus's door rig. The 12-gauge blank blast and flashbang detonated, causing 25 damage (-25 HP, -20 Stamina) and severe ringing in your ears.");

                            OpenUnit405Door(true);

                        }

                    }, true);

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Step back and return to hallway", () => EnterHallwayMode(2), true);

            }

            else

            {

                SetLocation("unit_405", "THE HUNTER'S SANCTUARY");

                PlayStageAnimation("Unit405Inside_Anim", "unit405", 8f);



                bool all405Done = read_marcus_log && looted_405_bench && looted_405_safe;

                string completionNotice = all405Done

                    ? "\n\n<color=#EF4444>You've taken everything useful Marcus left behind. Near the corner of the open safe, a small wooden toy car sits on the carpet. His family is waiting at Oakhaven for someone who isn't coming back.</color>"

                    : "";



                string roomAnchor = "The apartment looks half-turned into a bunker. A heavy timber reloading bench sits against the far wall, dusted with loose powder and spent brass. In the corner, a big gun safe stands wide open with the rifle racks empty. On the wall beside a dead ham radio, a county map has backcountry trails circled toward Oakhaven Cabin.";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration($"{actionNarration}{completionNotice}");

                }

                else
                {
                    string entranceText = room405_trap_triggered
                        ? "You stumble through the scorched doorway, blinking through the ringing in your ears. The air inside tastes like burnt sulfur, oil, and smoke."
                        : "You slip past the disarmed door and ease the heavy steel shut behind you. The air inside smells strongly of machine oil, graphite, and gunpowder.";

                    ShowLocationAnchor(
                        "int_405",
                        $"{entranceText}\n\n{roomAnchor}{completionNotice}",
                        $"You step back into Marcus's apartment. {(room405_trap_triggered ? "Smoke haze and spent brass still hang bitter in the dark." : "The open gun safe and reloading bench wait along the far wall.")}{completionNotice}",
                        $"Back inside Unit 405. Beside the dead ham radio, the county map with trails toward Oakhaven still hangs pinned to the wall.{completionNotice}"
                    );
                }



                if (!read_marcus_log)

                {

                    SetHallwayChoice(choiceIdx++, "[NOTE] Read notepad by the radio <color=#F59E0B>[CLUE]</color> <color=#9CA3AF>(~5 min)</color>", () =>

                    {

                        read_marcus_log = true;

                        gameClock?.AddTime(5);

                        loreSystem?.AddLore("Marcus's Final Log", "Marcus emptied his safe and rigged the door to stop looters. However, he was bitten on the stairwell before he could leave. Refusing to bring the infection to his family at Oakhaven Cabin, he chose to wander into the city to die alone.", gameClock?.GetTimeString() ?? "00:20");

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit405("<color=#F59E0B>[NOTE RECORDED]</color> You walk over to the radio desk. A grease-pencil note is pinned under the dead microphone, the handwriting shaking near the bottom:\n\n<color=#94A3B8>\"Diane... Emptied the safe and rigged the door. But on the stairs, one bit my forearm. Feels like battery acid in my veins. I won't bring this to Oakhaven. I won't bring it near you and the boys. Going outside to pull them away from the front doors. I love you.\"</color>\n\nYou set the mic down, look at the circled map coordinates for a quiet second, and step back to the middle of the room.");

                    });

                }



                if (!looted_405_bench)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Sweep reloading bench <color=#9CA3AF>(9mm Ammo x12, Scrap Metal x3 — ~5 min)</color>", () =>

                    {

                        looted_405_bench = true;

                        gameClock?.AddTime(5);

                        GiveOrDropItem("9mm Ammo", 12);

                        GiveOrDropItem("Scrap Metal", 3);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit405("<color=#F59E0B>Ammunition and salvage gathered.</color> You walk over to the heavy timber bench. Discarded sizing dies and empty brass casings clutter the surface, but a loaded plastic tray holds 12 rounds of live 9mm hollow-point ammunition alongside 3 heavy scrap metal brackets. You stow them securely into your pack, wipe the graphite dust from your hands, and walk back to the middle of the room.");

                    });

                }



                if (!looted_405_safe)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Check bottom of gun safe <color=#9CA3AF>(Hunting Knife x1 — ~3 min)</color>", () =>

                    {

                        looted_405_safe = true;

                        gameClock?.AddTime(3);

                        GiveOrDropItem("Hunting Knife", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit405("<color=#F59E0B>Hunting knife recovered.</color> You walk to the open vault in the corner and crouch down. The rifle racks are empty, but beneath the torn felt lining at the bottom sits a high-carbon hunting knife in a rigid kydex sheath. You inspect the honed edge, slide the sheath onto your belt, and stand back up in the center of the room.");

                    });

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave apartment and return to hallway", () => EnterHallwayMode(2));

            }



            ReindexActiveChoiceHotkeys();

        }



        public void InspectUnit404(string actionNarration = null)

        {

            currentInspectedRoom = "404";

            ClearHallwayChoices();

            int choiceIdx = 0;



            SetLocation("hallway_2", "UNIT 404 // MRS. GABLE'S DOOR", "unit_404");

            UpdateLocationDisplay("UNIT 404", "MRS. GABLE & BUSTER");

            PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);

            PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.7f);



            string unit404Anchor = "The painted wood door of Unit 404. From inside comes the dull scrape of heavy furniture wedged against the floorboards. You can hear Buster pacing right behind the door, panting through the gap at the bottom.";



            if (!string.IsNullOrEmpty(actionNarration))

            {

                SetSituationNarration(actionNarration);

            }

            else
            {
                if (spoke_to_mrs_gable)
                {
                    ShowLocationAnchor(
                        "ext_404",
                        $"{unit404Anchor}\n\nThe door is still deadbolted tight. Buster lets out a soft puff of breath from the entryway rug, and a radio murmurs low inside. Mrs. Gable is staying put behind her oak dresser.",
                        "You pause outside Unit 404. Inside, Buster pants softly by the draft stopper while a radio murmurs low behind her barricaded dresser.",
                        "Unit 404's door stays wedged shut from the inside. Everything remains quiet behind Mrs. Gable's barricade."
                    );
                }
                else
                {
                    string entranceText = "Before you can even knock, sharp barking breaks out inside, followed by claws scrabbling against the wood.\n\nFrom the dark entryway, an old, shaking voice calls through the deadbolt:\n\n\"Who's out there? Stay back! Buster, heel!\"\n\nBehind the lock, the heavy groan of wood scraping across hardwood tells you she shoved a dresser against it. She's scared, and she's not opening that door for anyone.";

                    ShowLocationAnchor(
                        "ext_404",
                        $"{unit404Anchor}\n\n{entranceText}",
                        "Buster starts barking again the moment your boots touch the mat outside 404. Mrs. Gable stays silent behind the deadbolt.",
                        "Wood scrapes faintly against floorboards inside 404 while the dog paces just behind the entryway."
                    );
                }
            }



            if (!spoke_to_mrs_gable)

            {

                SetHallwayChoice(choiceIdx++, "[SOOTHE] Whisper through the door to Mrs. Gable <color=#10B981>[HUMANITY +5]</color> <color=#9CA3AF>(~2 min)</color>", () =>

                {

                    spoke_to_mrs_gable = true;

                    gameClock?.AddTime(2);

                    if (currentPlayer != null) currentPlayer.humanity = Mathf.Min(100, currentPlayer.humanity + 5);

                    PlayClickSFX();

                    PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/SwitchLight2.wav", 0.75f);

                    UpdateVitalsUI();

                    RefreshAllPDAPanels();

                    InspectUnit404("<color=#10B981>[HUMANITY +5]</color> You lean close to the doorframe, keeping your voice low and steady near the lock.\n\n\"Mrs. Gable, it's me from 407. Don't open the door. Keep that dresser wedged tight and stay away from the windows tonight.\"\n\nThe hallway goes quiet for a beat. The scratching stops, replaced by Buster's soft whimper under the draft stopper. Her shaky voice steadies behind the wood:\n\n\"Thank God it's you, dear. Be careful out there.\"\n\nYou step back into the hall and leave her door alone.");

                });

            }



            SetHallwayChoice(choiceIdx++, "[BACK] Leave Mrs. Gable safe — return to hallway", () => EnterHallwayMode(2));

            ReindexActiveChoiceHotkeys();

        }



        public void InspectUnit403()

        {

            currentInspectedRoom = "403";

            ClearHallwayChoices();

            int choiceIdx = 0;



            SetLocation("hallway_3", "UNIT 403 // SARAH'S DOOR", "unit_403");

            UpdateLocationDisplay("UNIT 403", "SARAH'S APARTMENT");

            PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);

            PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.85f);



            ShowLocationAnchor(
                "ext_403",
                "You try the brass handle of Unit 403. It doesn't budge—deadbolted from the outside with a clean key turn.\n\nBy the mat sits a small shoe rack with house slippers still lined up, the spot for her running shoes empty. A Swedish Medical Center call-in slip is tucked under the knocker, stamped for a 19:00 emergency shift.\n\nSarah left for the hospital hours before midnight. The local news clips on your phone showed police blocking off the streets around the downtown ERs once the waiting rooms overflowed. The peephole is pitch black, and there's no sound inside. Nobody's home.",
                "Unit 403 stays locked from the outside. Sarah's ER call-in slip still hangs tucked under the knocker.",
                "Still nothing behind Sarah's peephole—just house slippers lined neatly beside the empty spot for her shoes."
            );



            SetHallwayChoice(choiceIdx++, "[BACK] Step back and return to hallway", () => EnterHallwayMode(3));

            ReindexActiveChoiceHotkeys();

        }



        private void OpenUnit402Door(string method)

        {

            room402_entry_method = method;

            room402_opened = true;

            string subtitle;

            if (method == "loud")

            {

                PlaySFX("Audio/SFX/elbow_smash_glass", 0.85f);

                PlaySFX("Audio/SFX/firedoor_slam", 0.75f);

                AppendStoryLog(gameClock?.GetTimeString() ?? "00:25", "UNIT 402 // FORCED CHAIN LOCK", "Kicked the door bracket down with a loud crack. The chain tore out, echoing across the lower hallway.");

                subtitle = "The chain bracket rips from the jamb with a violent crack.\n<color=#EF4444>The door slams open, echoing down the dark corridor...</color>";

            }

            else

            {

                PlayClickSFX();

                subtitle = "The steel security chain slides smoothly from its track.\n<color=#94A3B8>The door glides open softly into warm light and mournful piano music...</color>";

            }



            PlayDoorTransition(

                "Videos/unit402_door_transition",

                "[ UNIT 402 // THE ANDERSONS ]",

                subtitle,

                false,

                () => InspectUnit402(),

                false

            );

        }



        public void InspectUnit402(string actionNarration = null)

        {

            currentInspectedRoom = "402";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (!room402_opened)

            {

                SetLocation("hallway_3", "THE CHAIN", "unit_402");

                UpdateLocationDisplay("UNIT 402", "THE CHAIN");

                PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);

                PlayHubBGM("Audio/BGM/chopin", 0.45f);



                ShowLocationAnchor(
                    "ext_402",
                    "The Andersons' place—the quiet family down the hall whose daughter Emma always practiced piano on Sunday mornings. The front door isn't locked. You push it open two inches before a heavy steel chain pulls tight against the frame.\n\nThrough the narrow crack, warm lamp light spills into the hall. Inside, a record player needle rides spinning black vinyl, sending a slow, sad piano piece drifting under the door. Mixing with the music is a sharp chemical smell—heavy household bleach, like someone scrubbed the whole floor tonight.",
                    "You step back up to Unit 402's cracked doorway. Soft piano notes still drift out over the sharp smell of bleach.",
                    "The heavy steel chain still holds Unit 402 two inches ajar against warm lamplight."
                );



                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);

                bool hasLocksmithOrGhost = currentPlayer != null && (currentPlayer.HasTrait("LOCKSMITH") || currentPlayer.HasTrait("GHOST"));

                bool hasMultiTool = currentPlayer != null && currentPlayer.HasItem("Multi-Tool");

                bool hasTech1 = currentPlayer != null && currentPlayer.tech >= 1;

                bool hasKnife = currentPlayer != null && (currentPlayer.HasItem("Combat Knife") || currentPlayer.HasItem("Kitchen Knife") || currentPlayer.HasItem("Hunting Knife"));

                int agiVal = currentPlayer != null ? currentPlayer.agi : 5;

                int strVal = currentPlayer != null ? currentPlayer.str : 5;



                // 1. Burglar

                AddModularChoice(

                    ref choiceIdx,

                    isBurglar,

                    "[BURGLAR]",

                    "Slip the chain lock off its runner",

                    5, 1,

                    () => OpenUnit402Door("silent"),

                    $"Requires Burglar (Your Job: {currentPlayer?.job ?? "Survivor"}) — You lack illicit lock knowledge"

                );



                // 2. Locksmith or Ghost

                AddModularChoice(

                    ref choiceIdx,

                    hasLocksmithOrGhost,

                    "[LOCKSMITH/GHOST]",

                    "Work the weak point in the track",

                    5, 2,

                    () => OpenUnit402Door("silent"),

                    "Requires Locksmith or Ghost trait — You lack specialized infiltration knowledge"

                );



                // 3. Tech 1 + Multi-Tool

                AddModularChoice(

                    ref choiceIdx,

                    hasTech1 && hasMultiTool,

                    "[TECH 1 + TOOL]",

                    "Unscrew the mounting bracket cleanly",

                    5, 3,

                    () => OpenUnit402Door("silent"),

                    !hasMultiTool 

                        ? "Requires Multi-Tool — You don't have tools to dismantle the bracket" 

                        : $"Requires Tech 1 (You have {currentPlayer?.tech ?? 0}) — You don't know how to dismantle it safely"

                );



                // 4. Reflex/AGI 5 + Thin Blade

                AddModularChoice(

                    ref choiceIdx,

                    agiVal >= 5 && hasKnife,

                    "[REFLEX 5 + BLADE]",

                    "Guide the chain through the gap with a thin blade",

                    5, 4,

                    () => OpenUnit402Door("silent"),

                    !hasKnife 

                        ? "Requires Thin Blade — You don't have a knife equipped" 

                        : $"Requires Reflex 5 (You have {agiVal}) — Your hands aren't steady enough to work the gap"

                );



                // 5. Might 4 Clean Kick vs Fallback Gamble

                if (strVal >= 4)

                {

                    SetHallwayChoice(choiceIdx++, $"[MIGHT {strVal}/4] Kick the door mount to snap the screws <color=#10B981>(-5 Stamina, 100% Guaranteed, ~1 min)</color>", () =>

                    {

                        ExertStamina(5);

                        gameClock?.AddTime(1);

                        OpenUnit402Door("loud");

                    }, true);

                }

                else

                {

                    int chance = Mathf.Clamp(strVal * 10, 15, 35);

                    SetHallwayChoice(choiceIdx++, $"[BRUTE FORCE] Throw weight against door [Might {strVal}/4 — {chance}% Chance] <color=#EF4444>(-15 Stamina, Injury Risk)</color>", () =>

                    {

                        ExertStamina(15);

                        gameClock?.AddTime(3);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            OpenUnit402Door("loud");

                        }

                        else

                        {

                            PlayBeepSFX();

                            TakeDamage(10, "Shoulder slam against steel chain");

                            SetSituationNarration("<color=#EF4444><b>BRUTE FORCE FAILED (-10 HP, -15 Stamina).</b></color> You slammed your shoulder into the door, but the steel chain held firm. The heavy jolt bruised your shoulder and knocked the breath out of you.");

                        }

                    }, true);

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave it and walk away", () => EnterHallwayMode(3), true);

            }

            else

            {

                SetLocation("unit_402", "THE QUIET ROOM");

                PlayStageAnimation("Unit402Inside_Anim", "unit402", 8f);

                PlayHubBGM("Audio/BGM/chopin", 0.45f);



                string unit402Anchor = "A spotless apartment lit by warm table lamps. In the corner, the record player needle keeps spinning over black vinyl, filling the room with slow piano music. At the dining table, Mr. and Mrs. Anderson and their daughter Emma sit upright in wooden chairs, dressed in Sunday clothes, heads leaning together. Three wine glasses sit in front of them, fine white powder crusted around the rims.";



                bool all402Done = looted_402_weapon && looted_402_kitchen && looted_402_bathroom && took_402_photo && read_402_note;

                string completionNotice = all402Done

                    ? "\n\n<color=#10B981>There is nothing left to take. The Chopin nocturne continues spinning softly on the turntable. You ease the door shut behind you.</color>"

                    : "";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration($"{actionNarration}{completionNotice}");

                }

                else
                {
                    string entranceText = room402_entry_method == "loud"
                        ? "You step in past the splintered frame. The smell of bleach stings your nose and throat. There's no blood, no struggle, nothing broken. A .38 revolver lies on the wood floor beside Mr. Anderson's dress shoe."
                        : "You ease the door open and step into the quiet room. The smell of bleach stings your nose and throat. There's no blood, no struggle, nothing out of place. A .38 revolver lies on the wood floor beside Mr. Anderson's dress shoe.";

                    ShowLocationAnchor(
                        "int_402",
                        $"{entranceText}\n\n{unit402Anchor}{completionNotice}",
                        $"You step back into the Andersons' quiet living room. The turntable still spins soft piano into the bleach-sharp air.{completionNotice}",
                        $"Back inside Unit 402. Warm lamps still glow over the tidy dining table and silent hall.{completionNotice}"
                    );
                }



                if (!looted_402_weapon)

                {

                    SetHallwayChoice(choiceIdx++, "[TAKE] Pick up revolver from floor <color=#9CA3AF>(Revolver .38, .38 Ammo x5 — ~1 min)</color>", () =>

                    {

                        looted_402_weapon = true;

                        gameClock?.AddTime(1);

                        GiveOrDropItem("Revolver .38", 1);

                        GiveOrDropItem(".38 Ammo", 5);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#F59E0B>Sidearm acquired.</color> You walk over to the dining table and crouch beside Mr. Anderson's chair. His fingers hang limp inches above the weapon. You lift the blued-steel .38 Special revolver from the floor and check the cylinder. Five live cartridges remain in the chambers. You holster the gun at your hip, stand back up, and return to the center of the living room.");

                    });

                }



                if (!read_402_note)

                {

                    SetHallwayChoice(choiceIdx++, "[NOTE] Read suicide note on table <color=#38BDF8>[+15 XP]</color> <color=#9CA3AF>(~1 min)</color>", () =>

                    {

                        read_402_note = true;

                        gameClock?.AddTime(1);

                        currentPlayer?.GainXP(15);

                        loreSystem?.AddLore("The Andersons' Note", "They killed themselves after hearing a rumor on shortwave radio claiming the police were locking people in and pumping gas through the vents before sunrise.", gameClock?.GetTimeString() ?? "00:25");

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#38BDF8>[+15 XP]</color> You step up to the dining table and pick up a folded sheet of stationery between the glasses. In neat handwriting, Mr. Anderson wrote:\n\n<color=#94A3B8>The shortwave station says the police lost control of downtown and they're shutting the vents and gassing the blocks before sunrise. We won't choke in the dark like animals. God forgive us.</color>\n\nSomebody got on an open band and started spreading panic—and they believed every word. You set the paper back down and step away from the table.");

                    });

                }



                if (!looted_402_kitchen)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Search kitchen pantry <color=#9CA3AF>(Canned Food x2, Bottled Water x1 — ~2 min)</color>", () =>

                    {

                        looted_402_kitchen = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Canned Food", 2);

                        GiveOrDropItem("Bottled Water", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#F59E0B>Provisions gathered.</color> You walk into the tidy kitchen. The shelves are neatly organized with non-perishables they prepared before the panic set in. You take two cans of preserved stew and a sealed bottle of spring water, pack them into your bag, and walk back to the middle of the room.");

                    });

                }



                if (!looted_402_bathroom)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Check bathroom medicine cabinet <color=#9CA3AF>(Painkillers x2, Antiseptic x1 — ~1 min)</color>", () =>

                    {

                        looted_402_bathroom = true;

                        gameClock?.AddTime(1);

                        GiveOrDropItem("Painkillers", 2);

                        GiveOrDropItem("Antiseptic", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#F59E0B>Medical supplies recovered.</color> You walk into the tiled bathroom. In the wastebasket beneath the sink lie three emptied prescription sedative bottles. Inside the mirrored cabinet, you salvage two blister packs of painkillers and a bottle of antiseptic solution, slip them into your pouch, and return to the center of the living room.");

                    });

                }



                if (!took_402_photo)

                {

                    SetHallwayChoice(choiceIdx++, "[TAKE] Take family photograph from mantel <color=#9CA3AF>(Family Photo x1 — ~1 min)</color>", () =>

                    {

                        took_402_photo = true;

                        gameClock?.AddTime(1);

                        GiveOrDropItem("Family Photo", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#94A3B8>Family photograph taken.</color> You walk over to the fireplace mantel. A silver frame holds a photo of the three of them at a sunny lakeside cabin, laughing in windbreakers. You slide the picture from its frame and tuck it inside your jacket pocket, then step back to the center of the room.");

                    });

                }



                if (!looted_402_ring)

                {

                    SetHallwayChoice(choiceIdx++, "[TAKE] Slip wedding ring from Mrs. Anderson's hand <color=#EF4444>[HUMANITY −10]</color> <color=#9CA3AF>(Gold Ring x1 — ~1 min)</color>", () =>

                    {

                        looted_402_ring = true;

                        gameClock?.AddTime(1);

                        if (currentPlayer != null) currentPlayer.humanity = Mathf.Max(0, currentPlayer.humanity - 10);

                        GiveOrDropItem("Gold Ring", 1);

                        PlayClickSFX();

                        UpdateVitalsUI();

                        RefreshAllPDAPanels();

                        InspectUnit402("<color=#EF4444>[HUMANITY −10]</color> You walk over to Mrs. Anderson's chair. Her cold hand rests over her daughter's fingers. You carefully slide the gold wedding band off her ring finger—the metal feels like ice against your thumb. You drop it into your pocket with a dry throat and step away from the table.");

                    });

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave them in peace — return to hallway", () => EnterHallwayMode(3));

            }



            ReindexActiveChoiceHotkeys();

        }



        public void InspectUnit401(string actionNarration = null)

        {

            currentInspectedRoom = "401";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (!shelf_lifted)

            {

                SetLocation("unit_401", "UNIT 401 // THE FALLEN SHELF");

                PlayStageAnimation("Unit401Inside_Anim", "unit401", 8f);

                if (!turned_off_music) PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);



                string liftAnchor = "The door hangs wide open. Inside, loud pop music thumps out of a blown speaker. Crushed red plastic cups and spilled beer cover the hardwood floor.\n\nIn the middle of the living room, a heavy oak TV stand has pitched forward, pinning a young man face down against the boards. Deep gouge marks in the wood look like people shoved past it in a stampede for the front door. His right arm sticks out across the rug, fingers curled loosely around a set of Car Keys.";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration(actionNarration);

                }

                else
                {
                    ShowLocationAnchor(
                        "ext_401",
                        liftAnchor,
                        $"You step back through Unit 401's open doorway. Crushed cups crunch underfoot around the tipped oak TV stand{(turned_off_music ? ", the room finally quiet." : " while pop music still rattles the floorboards.")}",
                        "Back in the wrecked party apartment. The young man's hand still rests on the rug beside the Car Keys."
                    );
                }



                bool isConstruction = currentPlayer != null && string.Equals(currentPlayer.job, "Construction Worker", StringComparison.OrdinalIgnoreCase);

                bool hasBreacherOrGrip = currentPlayer != null && (currentPlayer.HasTrait("BREACHER") || currentPlayer.HasTrait("IRON GRIP"));

                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;

                string pryToolName = currentPlayer != null && currentPlayer.HasItem("Crowbar") ? "Crowbar" : (currentPlayer != null && currentPlayer.HasItem("Tire Iron") ? "Tire Iron" : (currentPlayer != null && currentPlayer.HasItem("Sledgehammer") ? "Sledgehammer" : null));

                bool hasPryTool = !string.IsNullOrEmpty(pryToolName);

                int strVal = currentPlayer != null ? currentPlayer.str : 5;



                // 1. Construction Worker Job

                AddModularChoice(

                    ref choiceIdx,

                    isConstruction,

                    "[CONSTRUCTION]",

                    "Leverage weak points with construction know-how",

                    10, 1,

                    () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(1);

                        shelf_lifted = true;

                        GiveOrDropItem("Car Keys", 1);

                        PlayClickSFX();

                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> Using construction know-how, you brace the center joist at its balance point and hoist the heavy entertainment unit off his chest, propping it against the wall. You gently loosen the Hyundai car keys from his cold grip and slide them into your pocket. The apartment is clear to search.");

                    },

                    $"Requires Construction Worker Job (Your Job: {currentPlayer?.job ?? "Survivor"}) — You lack structural load knowledge"

                );



                // 2. Breacher / Iron Grip Trait

                AddModularChoice(

                    ref choiceIdx,

                    hasBreacherOrGrip,

                    "[BREACHER/IRON GRIP]",

                    "Deadlift the lower frame clean",

                    10, 1,

                    () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(1);

                        shelf_lifted = true;

                        GiveOrDropItem("Car Keys", 1);

                        PlayClickSFX();

                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You lock your grip onto the lower frame and deadlift the oak cabinet with sheer explosive force, shoving it against the wall. You retrieve the Hyundai car keys from his fingers and stow them safely into your pocket. The apartment is clear to search.");

                    },

                    "Requires Breacher or Iron Grip Trait — You lack explosive lifting technique"

                );



                // 3. Tech 2 Knowledge

                AddModularChoice(

                    ref choiceIdx,

                    hasTech2,

                    "[TECH 2]",

                    "Dismantle bracket dowels and joint pins",

                    10, 4,

                    () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(4);

                        shelf_lifted = true;

                        GiveOrDropItem("Car Keys", 1);

                        PlayClickSFX();

                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You quickly loosen the connecting bracket dowels, relieving the crushing downward tension, and tilt the upper shelving safely away. You slide the Hyundai car keys from his grasp and pocket them. The apartment is clear to search.");

                    },

                    $"Requires Tech 2 (You have {currentPlayer?.tech ?? 0}) — You lack knowledge of furniture joint mechanics"

                );



                // 4. Pry Tool

                AddModularChoice(

                    ref choiceIdx,

                    hasPryTool,

                    hasPryTool ? $"[TOOL: {pryToolName.ToUpper()}]" : "[TOOL: PRY BAR]",

                    hasPryTool ? $"Pry the oak frame upward using {pryToolName}" : "Pry the oak frame upward using prying tool",

                    10, 2,

                    () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(2);

                        shelf_lifted = true;

                        GiveOrDropItem("Car Keys", 1);

                        PlayClickSFX();

                        InspectUnit401($"<color=#F59E0B>Car keys secured.</color> You wedge the {pryToolName.ToLower()} under the oak lip, using a cinder block as a fulcrum to lever the heavy unit upward. You slide the Hyundai car keys free from his hand and pocket them. The apartment is clear to search.");

                    },

                    "Requires Crowbar, Tire Iron, or Sledgehammer — You have no leverage tool"

                );



                // 5. Might 6 Guaranteed vs Fallback Gamble

                if (strVal >= 6)

                {

                    SetHallwayChoice(choiceIdx++, $"[MIGHT {strVal}/6] Deadlift 300-lb shelf with raw muscle <color=#10B981>(-10 Stamina, 100% Guaranteed, ~1 min)</color>", () =>

                    {

                        ExertStamina(10);

                        gameClock?.AddTime(1);

                        shelf_lifted = true;

                        GiveOrDropItem("Car Keys", 1);

                        PlayClickSFX();

                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You plant your boots, dig your fingers beneath the oak base, and strain with every muscle to heave the entertainment center off his torso. You take the Hyundai car keys from his hand and tuck them into your pocket. The apartment is clear to search.");

                    }, true);

                }

                else

                {

                    int chance = Mathf.Clamp(strVal * 8, 15, 45);

                    SetHallwayChoice(choiceIdx++, $"[DESPERATE SURGE] Throw entire body weight into wood [Might {strVal}/6 — {chance}% Chance] <color=#EF4444>(-25 Stamina, Severe Injury Risk)</color>", () =>

                    {

                        ExertStamina(25);

                        gameClock?.AddTime(3);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            shelf_lifted = true;

                            GiveOrDropItem("Car Keys", 1);

                            PlayClickSFX();

                            InspectUnit401("<color=#F59E0B>Car keys secured.</color> With a heavy grunt, you get your shoulder under the wood and shove the oak unit back up against the wall. You pry the Hyundai keys out of his stiff fingers and drop them in your pocket. The rest of the place is clear to search.");

                        }

                        else

                        {

                            PlayBeepSFX();

                            TakeDamage(15, "Oak shelf collapse on back");

                            InspectUnit401("<color=#EF4444><b>LIFT FAILED (-15 HP, -25 Stamina).</b></color> The oak unit slipped out of your sweaty hands and crashed right back down. You wrenched your lower back and bruised your ribs.");

                        }

                    }, true);

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave — keys not worth injury", () =>

                {

                    StopHubBGM();

                    EnterHallwayMode(4);

                }, true);

            }

            else

            {

                SetLocation("unit_401", "THE PARTY THAT ENDED");

                PlayStageAnimation("Unit401Inside_Anim", "unit401", 8f);

                if (!turned_off_music) PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);



                bool all401Done = shelf_lifted && looted_401_kitchen && looted_401_bathroom && looted_401_alcohol && looted_401_pockets && turned_off_music;

                string completionNotice = all401Done

                    ? "\n\n<color=#10B981>Nothing left to grab in here—just the dark, the crushed cups, and quiet where the stereo used to be. You head back toward the door.</color>"

                    : "";



                string musicState = turned_off_music

                    ? "The stereo is dead. With the bass gone, the room feels cramped and too quiet."

                    : "The stereo still vibrates on the table, blasting the same pop chorus over and over.";



                string unit401Anchor = $"The remains of a house party cut short. Crushed plastic cups and broken glass litter the sticky carpet, and the heavy oak TV stand sits propped against the wall beside the guy on the floor. {musicState}";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration($"{actionNarration}{completionNotice}");

                }

                else
                {
                    string entranceText = "The oak shelf is braced upright against the wall now, and the car keys are in your pocket. The room smells of stale beer, spilled soda, and sweat.";

                    ShowLocationAnchor(
                        "int_401",
                        $"{entranceText}\n\n{unit401Anchor}{completionNotice}",
                        $"Back inside Unit 401. The oak TV stand leans propped against the wall{(turned_off_music ? " in dead silence." : " under thumping speakers.")}{completionNotice}",
                        $"You glance around the trashed party room again—crushed plastic cups, spilled beer, and open cupboards.{completionNotice}"
                    );
                }



                if (!looted_401_kitchen)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Raid kitchen cupboards <color=#9CA3AF>(Energy Bar x2, Bottled Water x1 — ~2 min)</color>", () =>

                    {

                        looted_401_kitchen = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Energy Bar", 2);

                        GiveOrDropItem("Bottled Water", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit401("<color=#F59E0B>Kitchen scavenged.</color> You walk into the sticky kitchenette. Behind half-empty liquor bottles and party snacks, you locate two unopened energy bars and a sealed liter bottle of water shoved into the back of a cabinet. You slide them into your pack and return to the center of the living room.");

                    });

                }



                if (!looted_401_bathroom)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Check bathroom medicine cabinet <color=#9CA3AF>(Painkillers x2, Bandage x1 — ~2 min)</color>", () =>

                    {

                        looted_401_bathroom = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Painkillers", 2);

                        GiveOrDropItem("Bandage", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit401("<color=#F59E0B>Medical supplies recovered.</color> You step into the bathroom and click open the mirrored cabinet. Amid hair products and party remedies, you find two blister cards of painkillers and a sterile elastic bandage roll. You stow them into your pouch and walk back to the middle of the room.");

                    });

                }



                if (!looted_401_alcohol)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Take liquor from kitchen island <color=#9CA3AF>(Bottle of Vodka x1, Whiskey x1 — ~2 min)</color>", () =>

                    {

                        looted_401_alcohol = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Bottle of Vodka", 1);

                        GiveOrDropItem("Whiskey", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit401("<color=#F59E0B>High-proof alcohol secured.</color> You walk over to the kitchen island. Among the sticky red cups sit two unopened bottles: one of eighty-proof vodka and one of blended whiskey. Both work as disinfectants or improvised accelerants. You pack the heavy glass bottles securely and return to the center of the living room.");

                    });

                }



                if (!looted_401_pockets)

                {

                    SetHallwayChoice(choiceIdx++, "[SEARCH] Search the fallen man's jacket <color=#9CA3AF>(Lighter x1 — ~1 min)</color>", () =>

                    {

                        looted_401_pockets = true;

                        gameClock?.AddTime(1);

                        GiveOrDropItem("Lighter", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUnit401("<color=#F59E0B>Pocket item scavenged.</color> You crouch beside the young man and check his denim jacket pockets. You ignore a crumpled wad of paper cash—useless now—and retrieve a metal refillable lighter. You slide the lighter into your pocket, stand up, and return to the middle of the room.");

                    });

                }



                if (!turned_off_music)

                {

                    SetHallwayChoice(choiceIdx++, "[MUTE] Shut off blasting stereo system <color=#9CA3AF>(~1 min)</color>", () =>

                    {

                        turned_off_music = true;

                        gameClock?.AddTime(1);

                        StopHubBGM();

                        PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/SwitchMed1.wav", 0.8f);

                        PlaySFX("Audio/SFX/static", 0.35f);

                        InspectUnit401("<color=#38BDF8>Stereo turned off.</color> You walk over to the speaker console and hit the power switch. The pop song cuts out with a dull thump, leaving the apartment dead quiet. Without the noise covering it up, the faint scratching in the hall and the sirens outside sound a lot closer. You step back toward the middle of the room.");

                    });

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Leave apartment and return to hallway", () =>

                {

                    StopHubBGM();

                    EnterHallwayMode(4);

                });

            }



            ReindexActiveChoiceHotkeys();

        }



        private void OpenUtilityDoor(string bypassAction = null)

        {

            utility_opened = true;

            PlayClickSFX();

            PlayDoorTransition(

                "Videos/utility_door_transition",

                "[ UTILITY CLOSET // MAINTENANCE ]",

                "The heavy magnetic cylinder lock disengages.\n<color=#94A3B8>The cold steel service door screeches outward, revealing utility racks and breaker boxes...</color>",

                false,

                () => InspectUtilityCloset(bypassAction),

                false

            );

        }



        public void InspectUtilityCloset(string actionNarration = null)

        {

            currentInspectedRoom = "utility";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (!utility_opened)

            {

                SetLocation("hallway_4", "MAINTENANCE DOOR", "utility");

                UpdateLocationDisplay("UTILITY", "MAINTENANCE DOOR");

                PlayStageAnimation("HallEnd_Anim", "hall_end", 8f);

                PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);



                string exteriorAnchor = "At the dead end of the hall, the worn carpet stops at bare concrete. In front of you is a heavy steel service door with faded yellow stencil paint: <b>MAINTENANCE — AUTHORIZED PERSONNEL ONLY</b>.\n\nA commercial magnetic lock holds the frame shut with a low electric hum. A strong smell of bleach and old rust—or dried blood—seeps under the rubber door sweep, while a dying fluorescent light flickers inside.";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration(actionNarration);

                }

                else
                {
                    ShowLocationAnchor(
                        "ext_utility",
                        exteriorAnchor,
                        "You check Carl's heavy steel maintenance door again. The commercial mag-lock still hums against the frame.",
                        "Bleach and rust linger under the rubber door sweep while the dying fluorescent flickers inside."
                    );
                }



                bool isBurglar = currentPlayer != null && string.Equals(currentPlayer.job, "Burglar", StringComparison.OrdinalIgnoreCase);

                bool hasTech2 = currentPlayer != null && currentPlayer.tech >= 2;

                string pryTool = currentPlayer != null && currentPlayer.HasItem("Crowbar") ? "Crowbar" : (currentPlayer != null && currentPlayer.HasItem("Tire Iron") ? "Tire Iron" : (currentPlayer != null && currentPlayer.HasItem("Sledgehammer") ? "Sledgehammer" : null));

                bool hasPryTool = !string.IsNullOrEmpty(pryTool);

                int strVal = currentPlayer != null ? currentPlayer.str : 5;



                // 1. Burglar Job

                AddModularChoice(

                    ref choiceIdx,

                    isBurglar,

                    "[BURGLAR]",

                    "Pick manual override cylinder",

                    7, 2,

                    () =>

                    {

                        ExertStamina(7);

                        gameClock?.AddTime(2);

                        OpenUtilityDoor("<color=#10B981>Magnetic override picked.</color> You locate the concealed mechanical cylinder recessed under the card reader housing. Working your tension wrench and pick through the industrial tumbler pins, the cylinder turns with a solid metallic clunk, cutting power to the holding magnet.");

                    },

                    $"Requires Burglar Job (Your Job: {currentPlayer?.job ?? "Survivor"}) — You lack illicit lock knowledge"

                );



                // 2. Tech 3 Knowledge

                AddModularChoice(

                    ref choiceIdx,

                    hasTech2,

                    "[TECH 2]",

                    "Hotwire magnetic reader solenoid",

                    7, 3,

                    () =>

                    {

                        ExertStamina(7);

                        gameClock?.AddTime(3);

                        OpenUtilityDoor("<color=#10B981>Circuit shorted.</color> You pop the reader faceplate loose, expose the twelve-volt wiring bundle, and bridge the holding solenoid circuit with copper wire. A bright spark jumps with a sharp snap, dropping the magnetic seal immediately.");

                    },

                    $"Requires Tech 2 (You have {currentPlayer?.tech ?? 0}) — You lack knowledge of magnetic access systems"

                );



                // 3. Heavy Pry Tool

                AddModularChoice(

                    ref choiceIdx,

                    hasPryTool,

                    hasPryTool ? $"[TOOL: {pryTool.ToUpper()}]" : "[TOOL: HEAVY PRY]",

                    hasPryTool ? $"Pry steel door jamb with {pryTool}" : "Pry steel door jamb with heavy prying tool",

                    7, 2,

                    () =>

                    {

                        ExertStamina(7);

                        gameClock?.AddTime(2);

                        OpenUtilityDoor($"<color=#10B981>Frame pried open.</color> You wedge the forged steel tip of the {pryTool.ToLower()} into the jamb right beside the magnetic housing. Leaning your full body weight into the leverage, the holding magnet gives way with a sharp metallic screech.");

                    },

                    "Requires Crowbar, Tire Iron, or Sledgehammer — You have no heavy pry tool"

                );



                // 4. Might 7 Guaranteed vs Fallback Gamble

                if (strVal >= 7)

                {

                    SetHallwayChoice(choiceIdx++, $"[MIGHT {strVal}/7] Violent shoulder tackle to overpower magnetic seal <color=#10B981>(-15 Stamina, 100% Guaranteed, ~1 min)</color>", () =>

                    {

                        ExertStamina(15);

                        gameClock?.AddTime(1);

                        OpenUtilityDoor("<color=#10B981>Door forced open.</color> You brace your boots against the hallway floor and drive your shoulder squarely into the reinforced strike plate. The sudden violent blow overpowers the magnetic holding plate, throwing the steel door inward against the concrete wall.");

                    }, true);

                }

                else

                {

                    int chance = Mathf.Clamp(strVal * 3, 5, 20);

                    SetHallwayChoice(choiceIdx++, $"[DESPERATE RAM] Ram industrial magnetic slab [Might {strVal}/7 — {chance}% Chance] <color=#EF4444>(-25 Stamina, Severe Injury Risk)</color>", () =>

                    {

                        ExertStamina(25);

                        gameClock?.AddTime(3);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            OpenUtilityDoor("<color=#10B981>Door forced open.</color> Against all odds, your shoulder hits the weak spot in the old frame bracket, and the magnetic lock pops loose.");

                        }

                        else

                        {

                            PlayBeepSFX();

                            TakeDamage(20, "Violent collision against industrial steel door");

                            InspectUtilityCloset("<color=#EF4444><b>BRUTE FORCE FAILED (-20 HP, -25 Stamina).</b></color> You threw your whole weight into the heavy steel door, and the magnetic lock didn't budge an inch. The impact jarred your collarbone and left your shoulder throbbing. You aren't breaking a steel mag-lock door down with bare shoulders.");

                        }

                    }, true);

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Step back into hallway", () =>

                {

                    StopHubBGM();

                    EnterHallwayMode(4);

                }, true);

            }

            else

            {

                SetLocation("utility", "UTILITY ROOM");

                PlayStageAnimation("UtilityRoom_Anim", "utility_room", 8f);

                PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);



                bool allUtilityDone = read_utility_log && looted_utility_bleach && looted_utility_tape && looted_utility_flashlight;

                string completionNotice = allUtilityDone

                    ? "\n\n<color=#10B981>You've taken everything useful from the workshop. Only the bleach smell, the tipped mop bucket, and the buzzing overhead tube remain. Time to head for the stairwell.</color>"

                    : "";



                string utilityAnchor = "A cramped, windowless maintenance room smelling strongly of industrial bleach and damp concrete. A dying fluorescent bulb strobes overhead across steel supply racks. A yellow mop bucket lies tipped over in the center, and right above the back workbench, a ceiling service hatch hangs pushed aside—muddy boot scuffs lead up the shelving edge into the dark crawlspace.";



                if (!string.IsNullOrEmpty(actionNarration))

                {

                    SetSituationNarration($"{actionNarration}{completionNotice}");

                }

                else
                {
                    string entranceText = "The heavy steel door sits propped behind you. Cool draft air drifts in from the hall, moving dust across the concrete floor.";

                    ShowLocationAnchor(
                        "int_utility",
                        $"{entranceText}\n\n{utilityAnchor}{completionNotice}",
                        $"You step back into the cramped maintenance room. The bulb still strobes across steel racks and the open ceiling hatch.{completionNotice}",
                        $"Back inside Carl's utility closet. Tipped mop bucket, bare concrete, and a cool draft pulling down from the open hatch.{completionNotice}"
                    );
                }



                if (!read_utility_log)

                {

                    SetHallwayChoice(choiceIdx++, "[NOTE] Read superintendent's logbook <color=#38BDF8>[+20 XP]</color> <color=#9CA3AF>(~2 min)</color>", () =>

                    {

                        read_utility_log = true;

                        gameClock?.AddTime(2);

                        currentPlayer?.GainXP(20);

                        loreSystem?.AddLore("Maintenance Log - 4th Floor", "Tenants in 402 complaining about a chemical smell again. Told them I'm not a maid. Feeling feverish today. My joints are burning. Taking some aspirin from the kit and locking myself in here to sleep it off until shift ends.", gameClock?.GetTimeString() ?? "00:30");

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUtilityCloset("<color=#38BDF8>Logbook inspected (+20 XP).</color> You step across the concrete to the metal desk and push aside curled building blueprints. Underneath lies Carl's spiral maintenance ledger. The last entry is scrawled at 00:15—he locked himself inside after his joints started burning with a high fever. The mag-lock stayed shut from the hall, though the open ceiling hatch above the counter shows where he must have scrambled up into the ducts. You close the ledger and step back.");

                    });

                }



                if (!looted_utility_bleach)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Search tipped cleaning cart <color=#9CA3AF>(Bleach x1, Work Gloves x1 — ~2 min)</color>", () =>

                    {

                        looted_utility_bleach = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Bleach", 1);

                        GiveOrDropItem("Work Gloves", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUtilityCloset("<color=#F59E0B>Cleaning supplies scavenged.</color> You walk over to the overturned janitor cart and right the wire frame with your boot. From the lower tray, you retrieve an unopened gallon jug of concentrated industrial bleach and a pair of heavy work gloves. You stow them securely and return to the center of the workshop.");

                    });

                }



                if (!looted_utility_tape)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Scavenge metal shelves <color=#9CA3AF>(Duct Tape x1, Rope x1 — ~3 min)</color>", () =>

                    {

                        looted_utility_tape = true;

                        gameClock?.AddTime(3);

                        GiveOrDropItem("Duct Tape", 1);

                        GiveOrDropItem("Rope", 1);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUtilityCloset("<color=#F59E0B>Hardware salvaged.</color> You step past the spilled bucket to the perimeter shelving racks. Among rusted plumbing elbows and empty paint cans, you unearth a fresh roll of heavy-duty duct tape and a twelve-meter coil of braided nylon rope. You pack the supplies and walk back to the middle of the room.");

                    });

                }



                if (!looted_utility_flashlight)

                {

                    SetHallwayChoice(choiceIdx++, "[LOOT] Check drawers of metal desk <color=#9CA3AF>(Flashlight x1, Batteries x2 — ~2 min)</color>", () =>

                    {

                        looted_utility_flashlight = true;

                        gameClock?.AddTime(2);

                        GiveOrDropItem("Flashlight", 1);

                        GiveOrDropItem("Batteries", 2);

                        PlayClickSFX();

                        RefreshAllPDAPanels();

                        InspectUtilityCloset("<color=#F59E0B>Lighting gear recovered.</color> You cross to the metal desk and pull open the sticking bottom drawer. Beneath discarded fuses and utility bills, you find a heavy aluminum LED flashlight and a sealed two-pack of fresh alkaline batteries. You test the switch briefly, slide them into your gear pocket, and step back to the center of the room.");

                    });

                }



                SetHallwayChoice(choiceIdx++, "[BACK] Return to hallway", () =>

                {

                    StopHubBGM();

                    EnterHallwayMode(4);

                });

            }



            ReindexActiveChoiceHotkeys();

        }



        public void InspectStairwell(string actionFeedback = null)

        {

            currentInspectedRoom = "stairs";

            ClearHallwayChoices();

            int choiceIdx = 0;



            if (stair_phase <= 1)

            {

                SetLocation("stairs", "4TH FLOOR // STAIRWELL DESCENT");

                tacticalMapController?.DiscoverAdjacentRooms("stairs");

                PlayStageAnimation("Stairwell4F_Anim", "stairwell4F", 8f);

                if (!stair_entered_sfx_played)
                {
                    stair_entered_sfx_played = true;
                    PlaySFX("Audio/SFX/firedoor_slam", 0.9f);
                }

                PlayHubBGM("Audio/SFX/generator_hum", 0.45f, true);



                objectiveSystem?.CompleteObjective("escape_building");

                objectiveSystem?.AddObjective("descend_stairs", "Descend through the stairwell to ground floor", ObjectiveStatus.Active);



                float carryWeight = currentPlayer != null ? currentPlayer.GetTotalWeight() : 0f;
                bool heavyPack = carryWeight >= 14f;
                string weightNote = heavyPack
                    ? $"\n\n<color=#F59E0B><b>[HEAVY PACK: {carryWeight:F1} KG]</b></color> Your loaded backpack pulls hard on your shoulders as you look down the steep concrete flight. Running in the dark with this much weight will throw off your balance."
                    : "";

                SetSituationNarration($"The heavy fire door swings shut behind you and the lock clicks into place. You try the handle once just to be sure. Locked. There's no going back into the 4th floor.\n\nDim red backup lights buzz along the wall, barely reaching the middle of the steps. Down at the bottom of the shaft, the building's backup generator hums through the concrete. You need to get down to the ground floor.{weightNote}");



                bool hasFlashlight = currentPlayer != null && currentPlayer.HasItem("Flashlight");

                bool hasLighter = currentPlayer != null && currentPlayer.HasItem("Lighter");

                int agiVal = currentPlayer != null ? currentPlayer.agi : 5;



                // 1. Flashlight

                AddModularChoice(

                    ref choiceIdx,

                    hasFlashlight,

                    "[GEAR: FLASHLIGHT]",

                    "Use your Flashlight to light the steps",

                    0, 4,

                    () =>

                    {

                        descentMethod = "flashlight";

                        gameClock?.AddTime(4);

                        stair_phase = 2;

                        PlaySFX("Audio/SFX/flashlight_click", 0.8f);

                        InspectStairwell();

                    },

                    "Requires Flashlight - You don't have a strong light source"

                );



                // 2. Lighter

                AddModularChoice(

                    ref choiceIdx,

                    hasLighter,

                    "[GEAR: LIGHTER]",

                    "Use your Lighter to see a few steps ahead",

                    0, 6,

                    () =>

                    {

                        descentMethod = "lighter";

                        gameClock?.AddTime(6);

                        stair_phase = 2;

                        PlaySFX("Audio/SFX/lighter_flick", 0.8f);

                        InspectStairwell();

                    },

                    "Requires Lighter - You have nothing to spark a flame"

                );



                // 3. Move Slowly and Carefully in dark (Always safe fallback)

                SetHallwayChoice(choiceIdx++, "Move slowly in the dark, one hand on the wall <color=#10B981>[SAFE]</color> <color=#9CA3AF>(~10 min)</color>", () =>

                {

                    descentMethod = "slow";

                    gameClock?.AddTime(10);

                    stair_phase = 2;

                    PlaySFX("Audio/SFX/boots_slow_concrete", 0.7f);

                    InspectStairwell();

                }, true);



                // 4. Agility Sprint vs Risky Sprint (scaled by backpack weight)
                int reqAgi = heavyPack ? 6 : 5;
                int sprintStam = heavyPack ? 10 : 5;
                string loadTag = heavyPack ? $" [HEAVY PACK {carryWeight:F1} KG]" : "";

                if (agiVal >= reqAgi)

                {

                    SetHallwayChoice(choiceIdx++, $"[AGILITY {agiVal}/{reqAgi}]{loadTag} Move quickly down the steps in the dark <color=#10B981>(-{sprintStam} Stamina, Guaranteed, ~1 min)</color>", () =>

                    {

                        descentMethod = "quick";

                        ExertStamina(sprintStam);

                        gameClock?.AddTime(1);

                        stair_phase = 2;

                        PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);

                        InspectStairwell();

                    }, true);

                }

                else

                {

                    int baseChance = agiVal * 18 - (heavyPack ? 15 : 0);
                    int chance = Mathf.Clamp(baseChance, 15, 75);

                    SetHallwayChoice(choiceIdx++, $"[RISKY SPRINT]{loadTag} Rush down the dark steps [Agility {agiVal}/{reqAgi} - {chance}% Chance] <color=#EF4444>(-{sprintStam} Stamina, Fall & Noise Risk, ~1 min)</color>", () =>

                    {

                        gameClock?.AddTime(1);

                        if (UnityEngine.Random.Range(1, 101) <= chance)

                        {

                            descentMethod = "quick";

                            ExertStamina(sprintStam);

                            PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);

                        }

                        else

                        {

                            descentMethod = "quick_fail";

                            stair_noise_made = true;

                            currentPlayer?.SetFlag("made_noise", true);

                            PlaySFX("Audio/SFX/stairs_tumble", 0.95f);

                            TakeDamage(15, "Slipped hard on the wet stairwell steps");

                            ExertStamina(20);

                        }

                        stair_phase = 2;

                        InspectStairwell();

                    }, true);

                }

            }

            else if (stair_phase == 2)

            {

                SetLocation("stairs", "3RD FLOOR LANDING // THE DESCENT");

                bool sawHorrors = room401_entered || room402_opened || room405_opened || room406_opened;

                if (descentMethod == "quick_fail")

                {

                    PlayStageAnimation("StairwellTumble_Anim", "stairwell_tumble", 8f);

                }

                else if (descentMethod == "flashlight" || descentMethod == "lighter")

                {

                    PlayStageAnimation("StairwellFlashlight_Anim", "stairwell_flashlight", 8f);

                    if (string.IsNullOrEmpty(actionFeedback))
                    {
                        PlaySFX("Audio/SFX/blood_drip", 0.6f);
                    }

                }

                else

                {

                    PlayStageAnimation("Stairwell4F_Anim", "stairwell4F", 8f);

                }



                bool seesBloodClearly = descentMethod == "quick_fail" || descentMethod == "flashlight" || descentMethod == "lighter";
                bool justLostHumanity = false;
                if (seesBloodClearly && !sawHorrors && !stair_hum_deducted)
                {
                    stair_hum_deducted = true;
                    justLostHumanity = true;
                    if (currentPlayer != null)
                    {
                        currentPlayer.humanity = Mathf.Max(0, currentPlayer.humanity - 5);
                    }
                    RefreshAllPDAPanels();
                }



                string descText = "";

                if (descentMethod == "quick")

                {

                    descText = "<color=#10B981><b>[AGILITY SUCCESS]</b></color> You take the steps two at a time, using the iron handrail to keep your balance.\n\nHalfway down to the 3rd-floor landing, your boot nearly comes down on a dark, wet patch beside a crushed red plastic cup. You catch the rail and hop wide onto the dry concrete below.\n\nUnder the dim red bulb, whatever you almost stepped in just looks like spilled beer or dirty water. You don't stop to check.";

                }

                else if (descentMethod == "slow")

                {

                    descText = "<color=#10B981><b>[SAFE DESCENT]</b></color> You keep one hand on the cold iron rail and slide your boots down one step at a time.\n\nNear the 3rd-floor landing, the red backup light glints off something wet pooled across the concrete next to a crushed plastic cup. In this dim light it just looks like spilled water. You press your back to the wall and step around the dry outer edge.";

                }

                else if (descentMethod == "quick_fail")

                {

                    string marcusNote = read_marcus_log
                        ? "\n\nYou think of the note on Marcus's radio table upstairs. This must be where he got bit on his way down."
                        : "";
                    string humNote = sawHorrors
                        ? "\n\nAfter what you already saw upstairs on the 4th floor, you just wipe your palms on your pants and keep moving."
                        : $"\n\n{(justLostHumanity || stair_hum_deducted ? "<color=#EF4444><b>[-5 HUMANITY]</b></color> " : "")}Your stomach turns. Whoever bled here lost a lot of it, and it happened not long ago.";

                    descText = $"<color=#EF4444><b>[FOOTING LOST // LOUD NOISE] (-15 HP, -20 STAMINA)</b></color>\n\nYou take the steps too fast in the dark. Halfway down, your heel hits something thick and slick on the concrete and goes straight out from under you. Your ribs slam the edge of the steps and your knee bangs the iron railing with a loud metallic <i>CLANG</i> that echoes all the way down the shaft.\n\nWhen you push yourself up on the 3rd-floor landing, your palms are sticky and warm. Even under the dim red bulb, you smell the sharp copper scent on your hands. It's blood - a wide puddle of it smeared across the concrete beside a torn piece of a jacket sleeve.{marcusNote}{humNote}";

                }

                else

                {

                    string lightIntro = descentMethod == "flashlight"
                        ? "<color=#10B981><b>[FLASHLIGHT BEAM]</b></color> You click on your flashlight, cutting a white beam down the concrete steps."
                        : "<color=#10B981><b>[LIGHTER FLAME]</b></color> You thumb your lighter. The small orange flame pushes back the dim red shadows a few steps at a time.";
                    string marcusNote = read_marcus_log
                        ? "\n\nYou remember the note by Marcus's radio upstairs. This is where he was bitten before he made it outside."
                        : "";
                    string humNote = sawHorrors
                        ? "\n\nAfter what you left behind on the 4th floor, you don't linger on it. You keep the light pointed ahead."
                        : $"\n\n{(justLostHumanity || stair_hum_deducted ? "<color=#EF4444><b>[-5 HUMANITY]</b></color> " : "")}Your throat tightens. Until now it was just sirens and voices on the radio, but this happened right outside your floor.";

                    descText = $"{lightIntro}\n\nHalfway to the 3rd-floor landing, the light catches a crushed red party cup and a wide, wet puddle spreading across the middle steps. Keeping the light steady, you step around the dry edge onto the landing.\n\nShining the light back over the concrete, you see it isn't water. It's thick, dark blood, still wet near the center. Red handprints are smeared across the wall at shoulder height next to a torn strip of a jacket sleeve - whoever got hurt here was leaning hard against the wall to stay on their feet.{marcusNote}{humNote}";

                }

                if (!string.IsNullOrEmpty(actionFeedback))
                {
                    SetSituationNarration(actionFeedback);
                }
                else
                {
                    SetSituationNarration(descText);
                }



                if (!stair_3f_door_checked)
                {
                    SetHallwayChoice(choiceIdx++, "[CHECK DOOR] Try the 3rd Floor fire door <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        stair_3f_door_checked = true;
                        gameClock?.AddTime(1);
                        PlaySFX("Audio/SFX/firedoor_click", 0.75f);
                        loreSystem?.AddLore("3rd Floor Fire Door Sealed", "Someone knotted a heavy orange extension cord tightly around the inside push-bar and tied it to a hallway pipe. The 3rd-floor corridor is dark and inaccessible from the stairs.", gameClock?.GetTimeString() ?? "00:35");
                        RefreshAllPDAPanels();
                        InspectStairwell("<color=#F59E0B><b>[3RD FLOOR SEALED]</b></color> You grab the handle of the 3rd-floor fire door and pull. It moves half an inch and stops dead. Looking through the narrow wire-glass window, you see someone knotted a heavy orange extension cord tightly around the inside push-bar and tied it to a hallway pipe. The 3rd-floor corridor beyond the glass is dark and quiet. There's no getting through here.");
                    }, true);
                }
                else
                {
                    SetHallwayChoice(choiceIdx++, "Try the 3rd Floor fire door <color=#EF4444>[TIED SHUT FROM INSIDE]</color>", null, false);
                }

                SetHallwayChoice(choiceIdx++, "Turn the corner and head down toward the lower floors", () =>

                {

                    stair_phase = 3;

                    PlayClickSFX();

                    InspectStairwell();

                }, true);

            }

            else if (stair_phase == 3)

            {

                SetLocation("stairs", "STAIRWELL // THE BARRICADE");

                PlayStageAnimation("StairwellBarricade_Anim", "stairwell_barricade", 8f);



                string baseNarration = "You head down past the 2nd-floor landing toward the ground floor lobby, but halfway between the 2nd and 1st floors, the steps are blocked.\n\n<color=#F59E0B><b>The stairs to the 1st Floor are jammed with furniture.</b></color>\n\nHeavy wooden desks, folding chairs, and a glass-front drink machine have been tipped over and wedged between the stairwell walls. Through a narrow gap beside the drink machine, you can see the 1st-floor lobby door just twenty feet below. The lower metal panel of the machine is dented inward and smeared with dark handprints from the bottom side.\n\nIt looks like if you could shift the drink machine a few inches, you might squeeze straight down to the 1st floor - otherwise you'll have to take the 2nd-floor door behind you.";

                if (!string.IsNullOrEmpty(actionFeedback))
                {
                    SetSituationNarration(actionFeedback);
                }
                else
                {
                    if (barricade_pushed)
                    {
                        baseNarration += "\n\n<color=#EF4444><b>[IMPASSABLE // LOUD NOISE MADE]</b></color> The drink machine's steel legs are wedged tight under the iron rail posts. You can't clear this way.";
                    }
                    SetSituationNarration(baseNarration);
                }



                if (!barricade_inspected && !barricade_pushed)
                {
                    SetHallwayChoice(choiceIdx++, "[INSPECT] Look closely at how the drink machine is wedged <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        barricade_inspected = true;
                        gameClock?.AddTime(1);
                        PlayClickSFX();
                        InspectStairwell("<color=#93C5FD><b>[BARRICADE INSPECTED]</b></color> You crouch by the gap and look at the frame. Whoever dumped this furniture from the upper floors wedged the drink machine's steel legs straight under the iron handrail posts. Shoving it by hand won't clear a path - it'll only scrape metal against concrete and make a loud racket down the shaft.");
                    }, true);
                }

                if (!barricade_pushed)

                {

                    SetHallwayChoice(choiceIdx++, "Try to shove the drink machine aside to squeeze through <color=#EF4444>[RISK: NOISE]</color> <color=#9CA3AF>(~1 min)</color>", () =>

                    {

                        barricade_pushed = true;

                        stair_noise_made = true;

                        currentPlayer?.SetFlag("made_noise", true);

                        gameClock?.AddTime(1);

                        PlaySFX("Audio/SFX/metal_shelf_crash", 0.85f);

                        InspectStairwell("<color=#EF4444><b>[FAILED // LOUD NOISE MADE]</b></color> You brace your boots on the step and throw your shoulder into the side of the drink machine.\n\n<i>SCREEEEECH.</i>\n\nThe metal frame grinds hard against the concrete wall, echoing up and down the stairwell shaft like a gunshot. The machine doesn't budge an inch. Behind you on the 2nd-floor landing, something heavy thumps against the inside of the corridor wall.");

                    }, true);

                }
                else
                {
                    SetHallwayChoice(choiceIdx++, "Try to shove the drink machine aside <color=#EF4444>[IMPASSABLE - WEDGED TIGHT]</color>", null, false);
                }



                if (!stair_listened_2f_door)
                {
                    SetHallwayChoice(choiceIdx++, "[LISTEN] Press your ear to the 2nd Floor fire door before opening it <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        stair_listened_2f_door = true;
                        gameClock?.AddTime(1);
                        if (stair_noise_made)
                        {
                            PlaySFX("Audio/SFX/zombie_rattle", 0.65f);
                            InspectStairwell("<color=#EF4444><b>[SOUND BEHIND DOOR // DANGER]</b></color> You press your ear against the cold steel of the 2nd-floor fire door. The noise you made in the stairwell carried inside. Right on the other side of the door, you hear wet, ragged breathing and shoes scraping against the floor. Whatever is in that hallway is already facing the door. <i>(Make sure a weapon is equipped in your Backpack before opening.)</i>");
                        }
                        else
                        {
                            PlaySFX("Audio/SFX/head_thump_wet", 0.55f);
                            InspectStairwell("<color=#F59E0B><b>[SOUND BEHIND DOOR]</b></color> You press your ear to the cold steel of the 2nd-floor door. For a second there's only the buzz of a failing hallway light - then a slow, wet, rhythmic <i>thump... thump...</i> about twenty feet down the hall, like someone tapping their forehead against the plaster wall. Whoever it is hasn't heard you yet. <i>(Check your Backpack if you need to equip a weapon first.)</i>");
                        }
                    }, true);
                }



                SetHallwayChoice(choiceIdx++, "Open the fire door to the 2nd Floor <color=#10B981>[DETOUR TO 2F]</color> <color=#9CA3AF>(~2 min)</color>", () =>

                {

                    StopHubBGM();

                    PlaySFX("Audio/SFX/firedoor_click", 0.8f);

                    gameClock?.AddTime(2);

                    currentPlayer?.SetFlag("made_noise", stair_noise_made);

                    objectiveSystem?.CompleteObjective("descend_stairs");

                    objectiveSystem?.AddObjective("detour_2f", "Cross the 2nd Floor to find the West Wing stairs", ObjectiveStatus.Active);

                    stair_phase = 4;

                    InspectStairwell();

                }, true);

            }
            else if (stair_phase >= 4)
            {
                SetLocation("stairs", "2ND FLOOR // EAST WING DOORWAY");
                StopHubBGM();

                if (stair_noise_made)
                {
                    PlayStageAnimation("F2Aggro_Anim", "F2_1_aggro", 8f);
                    SetSituationNarration("<color=#EF4444><b>[2ND FLOOR EAST WING // NOISE DETECTED]</b></color>\n\nThe heavy fire door clicks shut behind you. The loud noise from the stairwell carried straight down this hallway.\n\nTwenty feet ahead, under a single flickering tube light, a man in a torn suit stands in the middle of the corridor. He isn't facing the wall - he is staring straight at your door.");
                }
                else
                {
                    PlayStageAnimation("F2Thump_Anim", "F2_1_thump", 8f);
                    SetSituationNarration("<color=#10B981><b>[2ND FLOOR EAST WING // SILENT ENTRY]</b></color>\n\nThe fire door clicks shut behind you. The backup lights in this hallway are out, leaving only a single tube light flickering at the far end of the corridor.\n\nAbout twenty feet ahead, a man in a messy business suit stands in the middle of the hall with his forehead pressed against the wallpaper. Slow, wet <i>thumps</i> echo down the corridor as he taps his head against the plaster. He hasn't noticed you yet.");
                }

                SetHallwayChoice(choiceIdx++, "[2ND FLOOR] Step into the 2nd Floor Corridor (Next Section)", () =>
                {
                    PlayClickSFX();
                    InspectStairwell();
                }, true);
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

            path = "Assets/Sprites/Act1/" + spriteName + ".gif";

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



        private IEnumerator FadeAtmosphereAudio(float targetVol, float duration)

        {

            if (atmosphereSource == null) yield break;

            float startVol = atmosphereSource.volume;

            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)

            {

                if (atmosphereSource == null) yield break;

                atmosphereSource.volume = Mathf.Lerp(startVol, targetVol, t / duration);

                yield return null;

            }

            if (atmosphereSource != null) atmosphereSource.volume = targetVol;

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

        private int currentPassageBeatIndex = 0;

        private int currentPassageDialogueIndex = 0;

        private GameObject modularDialogueBox;

        private Image modularDialogueAvatar;

        private TextMeshProUGUI modularDialogueSpeakerText;

        private TextMeshProUGUI modularDialogueSpeechText;

        private RectTransform modularDialogueGoldBar;

        private RectTransform modularDialogueAvatarContainer;

        private RectTransform modularDialogueSpeechContainer;

        private Coroutine modularAvatarAnimRoutine;



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

            currentInspectedRoom = null;



            // Stop living room animated backdrop so it doesn't overwrite stageBackdropImage every frame!

            if (livingRoomAnimPlayer != null)

            {

                livingRoomAnimPlayer.Stop();

                livingRoomAnimPlayer.enabled = false;

            }



            SetApartmentButtonsVisible(false);



            currentConsoleMode = ConsoleMode.Narrative;

            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);



            if (GameManager.Instance != null)

            {

                GameManager.Instance.TransitionFromApartmentToHallway();

            }



            // 2. Visual Backdrop

            if (!passage.keepPreviousBackdrop)

            {

                bool hasValidAnimFrames = passage.animatedBackdropFrames != null && 

                                          passage.animatedBackdropFrames.Count > 0 && 

                                          passage.animatedBackdropFrames.Exists(f => f != null);



                if (hasValidAnimFrames)

                {

                    if (stageBackdropImage != null)

                    {

                        stageBackdropImage.enabled = true;

                        stageBackdropImage.color = Color.white;

                        var anim = stageBackdropImage.GetComponent<SundayLastBreath.UI.FrameAnimationPlayer>();

                        if (anim == null) anim = stageBackdropImage.gameObject.AddComponent<SundayLastBreath.UI.FrameAnimationPlayer>();

                        anim.enabled = true;

                        anim.SetFrames(passage.animatedBackdropFrames, passage.animationFPS > 0f ? passage.animationFPS : 8f, true);

                        anim.Play();

                    }

                }

                else if (passage.backdropSprite != null)

                {

                    if (stageBackdropImage != null)

                    {

                        var anim = stageBackdropImage.GetComponent<SundayLastBreath.UI.FrameAnimationPlayer>();

                        if (anim != null) { anim.Stop(); anim.enabled = false; }

                        stageBackdropImage.enabled = true;

                        stageBackdropImage.sprite = passage.backdropSprite;

                        stageBackdropImage.color = Color.white;

                    }

                }

                else

                {

                    if (stageBackdropImage != null)

                    {

                        var anim = stageBackdropImage.GetComponent<SundayLastBreath.UI.FrameAnimationPlayer>();

                        if (anim != null) { anim.Stop(); anim.enabled = false; }

                        // Never leave a null sprite with Color.white (prevents solid white screen!)

                        stageBackdropImage.sprite = null;

                        stageBackdropImage.color = new Color(0.04f, 0.045f, 0.06f, 1f);

                    }

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



                // Initialize loose room floor stash if this passage defines initial floor items and haven't populated yet

                if (passage.initialFloorItems != null && passage.initialFloorItems.Count > 0)

                {

                    string locKey = passage.associatedRoomId;

                    if (!initializedRoomFloorPassages.Contains(locKey))

                    {

                        initializedRoomFloorPassages.Add(locKey);

                        var stash = GetCurrentFloorStash();

                        foreach (var item in passage.initialFloorItems)

                        {

                            if (!string.IsNullOrEmpty(item.itemId) && item.quantity > 0)

                            {

                                var existing = stash.Find(e => e.itemId.Equals(item.itemId, StringComparison.OrdinalIgnoreCase));

                                if (existing != null) existing.quantity += item.quantity;

                                else stash.Add(new InventoryEntry(item.itemId, item.quantity));

                            }

                        }

                        tacticalGearController?.RefreshAll();

                        UpdateFloorItemsNotification();

                    }

                }

            }



            // 5.5. Dynamic Situation Box Header Synchronization

            UpdateSituationBoxHeader(passage.passageTitle);



            // 6. Narrative text display, Beats & Dialogue

            currentActiveDialogueLines = GetActiveDialogueLines(passage);

            if (currentActiveDialogueLines != null && currentActiveDialogueLines.Count > 0)

            {

                currentPassageDialogueIndex = 0;

                DisplayPassageDialogueLine(passage, 0);

            }

            else if (passage.narrativeBeats != null && passage.narrativeBeats.Count > 0)

            {

                if (modularDialogueBox != null) modularDialogueBox.SetActive(false);

                if (modularAvatarAnimRoutine != null) { StopCoroutine(modularAvatarAnimRoutine); modularAvatarAnimRoutine = null; }

                currentPassageBeatIndex = 0;

                DisplayPassageBeat(passage, 0);

            }

            else

            {

                if (modularDialogueBox != null) modularDialogueBox.SetActive(false);

                if (modularAvatarAnimRoutine != null) { StopCoroutine(modularAvatarAnimRoutine); modularAvatarAnimRoutine = null; }

                UpdateSituationBoxTextAndHeight(ParseMarkdown(passage.storyText));

                SetChoiceSubHeaderVisible(true);

                RenderPassageChoices(passage);

            }

        }



        public static string ParseMarkdown(string input)

        {

            if (string.IsNullOrEmpty(input)) return "";

            string result = System.Text.RegularExpressions.Regex.Replace(input, @"\*\*(.+?)\*\*", "<b>$1</b>");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"__(.+?)__", "<b>$1</b>");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"\*(.+?)\*", "<i>$1</i>");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"_(.+?)_", "<i>$1</i>");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"~~(.+?)~~", "<s>$1</s>");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"`(.+?)`", "<color=#93C5FD><b>$1</b></color>");

            return result;

        }



        public void UpdateSituationBoxHeader(string subtitle)

        {

            if (situationBox == null) return;

            Transform topRow = situationBox.transform.Find("TopRow");

            if (topRow != null)

            {

                Transform titleTr = topRow.Find("Title");

                if (titleTr != null)

                {

                    var tmp = titleTr.GetComponent<TextMeshProUGUI>();

                    if (tmp != null)

                    {

                        string sector = !string.IsNullOrEmpty(subtitle) ? subtitle.ToUpper() : "TACTICAL SITUATION";

                        string timeStr = gameClock != null ? $"{gameClock.GetTimeString()} DAY {gameClock.DayCount}" : "23:45 DAY 0";

                        tmp.text = $"<b><color=#F5A623>// CURRENT SITUATION</color></b>  <color=#78909C>// {sector} ({timeStr})</color>";

                    }

                }

            }

        }



        public void UpdateSituationBoxTextAndHeight(string formattedText)

        {

            if (situationBox == null) return;



            if (string.IsNullOrEmpty(formattedText))

            {

                situationBox.SetActive(false);

                return;

            }



            situationBox.SetActive(true);

            if (situationBoxText != null)

            {

                situationBoxText.text = formattedText;



                RectTransform sbtRect = situationBoxText.rectTransform;

                if (sbtRect != null)

                {

                    sbtRect.anchorMin = Vector2.zero;

                    sbtRect.anchorMax = Vector2.one;

                    sbtRect.offsetMin = new Vector2(18f, 16f);

                    sbtRect.offsetMax = new Vector2(-18f, -34f);

                }



                situationBoxText.ForceMeshUpdate();



                // Compute exact wrapped height for current box width with comfortable padding

                float boxWidth = 1140f;

                RectTransform sbRt = situationBox.GetComponent<RectTransform>();

                if (sbRt != null && sbRt.rect.width > 200f)

                {

                    boxWidth = sbRt.rect.width;

                }

                float availableTextWidth = Mathf.Max(200f, boxWidth - 36f);

                Vector2 prefValues = situationBoxText.GetPreferredValues(formattedText, availableTextWidth, 0f);

                float preferredTextH = prefValues.y;



                float topPadding = 34f;

                float bottomPadding = 16f;

                float extraBreathingRoom = 8f;

                float targetH = Mathf.Clamp(preferredTextH + topPadding + bottomPadding + extraBreathingRoom, 64f, 150f);



                LayoutElement le = situationBox.GetComponent<LayoutElement>();

                if (le == null) le = situationBox.AddComponent<LayoutElement>();

                le.preferredHeight = targetH;

                le.minHeight = targetH;

                le.flexibleHeight = 0f;



                // Add RectMask2D on situationBox if not present to cleanly clip if ever exceeded

                if (situationBox.GetComponent<RectMask2D>() == null)

                {

                    situationBox.AddComponent<RectMask2D>();

                }



                // Force layout rebuild so hubMenuContainer immediately reflows ChoiceSubHeader and choices below

                LayoutRebuilder.ForceRebuildLayoutImmediate(situationBox.GetComponent<RectTransform>());

                if (hubMenuContainer != null)

                {

                    LayoutRebuilder.ForceRebuildLayoutImmediate(hubMenuContainer.GetComponent<RectTransform>());

                    if (hubMenuContainer.transform.parent != null)

                    {

                        RectTransform parentRt = hubMenuContainer.transform.parent.GetComponent<RectTransform>();

                        if (parentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);

                    }

                }

            }

        }



        public void SetSituationNarration(string formattedText, bool recordInLog = true)

        {

            if (!string.IsNullOrEmpty(lastOverflowNotice))

            {

                formattedText += "\n" + lastOverflowNotice.TrimStart('\n');

                lastOverflowNotice = "";

            }

            if (recordInLog && !string.IsNullOrWhiteSpace(formattedText))
            {
                string trimmed = formattedText.Trim();
                if (!string.Equals(trimmed, lastAutoLoggedSituationText, StringComparison.Ordinal))
                {
                    lastAutoLoggedSituationText = trimmed;
                    string timestamp = gameClock != null ? gameClock.GetTimeString() : "00:00";
                    string header = (locationText != null && !string.IsNullOrEmpty(locationText.text))
                        ? locationText.text.Replace("\r", " ").Replace("\n", " ").TrimStart('/', ' ').Trim()
                        : "CURRENT SITUATION";
                    AppendStoryLog(timestamp, header, trimmed);
                }
            }

            UpdateSituationBoxTextAndHeight(formattedText);

        }

        private void ShowLocationAnchor(string anchorKey, string firstVisitText, params string[] returnVariants)
        {
            bool firstVisit = _visitedLocationAnchors.Add(anchorKey);
            if (firstVisit || returnVariants == null || returnVariants.Length == 0)
            {
                SetSituationNarration(firstVisitText, recordInLog: true);
                return;
            }

            int idx = _returnVariantIndices.TryGetValue(anchorKey, out int current) ? current : 0;
            string text = returnVariants[idx % returnVariants.Length];
            _returnVariantIndices[anchorKey] = idx + 1;
            SetSituationNarration(text, recordInLog: false);
        }



        public void SetChoiceSubHeaderVisible(bool visible, string customText = null)

        {

            if (hubMenuContainer == null) return;

            Transform sh = hubMenuContainer.transform.Find("ChoiceSubHeader");

            if (sh != null)

            {

                sh.gameObject.SetActive(visible);

                if (visible && !string.IsNullOrEmpty(customText))

                {

                    var tmp = sh.GetComponentInChildren<TextMeshProUGUI>();

                    if (tmp != null) tmp.text = customText;

                }

            }

        }



        private void EnsureModularDialogueBox()

        {

            if (modularDialogueBox != null) return;

            if (monologueBox == null || hubMenuContainer == null) return;



            modularDialogueBox = Instantiate(monologueBox, hubMenuContainer.transform, false);

            modularDialogueBox.name = "ModularDialogueBox";



            RectTransform rt = modularDialogueBox.GetComponent<RectTransform>();

            if (rt != null)

            {

                rt.anchorMin = new Vector2(0f, 1f);

                rt.anchorMax = new Vector2(1f, 1f);

                rt.pivot = new Vector2(0.5f, 1f);

                rt.anchoredPosition = Vector2.zero;

                rt.localPosition = Vector3.zero;

                rt.sizeDelta = new Vector2(0f, 96f);

            }



            Transform goldBarTr = modularDialogueBox.transform.Find("GoldBar");

            if (goldBarTr != null) modularDialogueGoldBar = goldBarTr.GetComponent<RectTransform>();



            Transform avatarContTr = modularDialogueBox.transform.Find("AvatarContainer");

            if (avatarContTr != null)

            {

                modularDialogueAvatarContainer = avatarContTr.GetComponent<RectTransform>();

                modularDialogueAvatar = avatarContTr.GetComponentInChildren<Image>();

            }



            Transform speechContTr = modularDialogueBox.transform.Find("SpeechContainer");

            if (speechContTr != null)

            {

                modularDialogueSpeechContainer = speechContTr.GetComponent<RectTransform>();

                var tmps = speechContTr.GetComponentsInChildren<TextMeshProUGUI>();

                if (tmps.Length > 0)

                {

                    modularDialogueSpeakerText = tmps[0];

                    modularDialogueSpeakerText.fontSize = 11.5f;

                }

                if (tmps.Length > 1)

                {

                    modularDialogueSpeechText = tmps[1];

                    modularDialogueSpeechText.fontSize = 13.5f;

                    modularDialogueSpeechText.lineSpacing = 3f;

                }

            }



            LayoutElement le = modularDialogueBox.GetComponent<LayoutElement>() ?? modularDialogueBox.AddComponent<LayoutElement>();

            le.preferredHeight = 96f;

            le.minHeight = 96f;

            le.flexibleHeight = 0f;

            le.flexibleWidth = 1f;



            int sIdx = situationBox != null ? situationBox.transform.GetSiblingIndex() + 1 : 0;

            modularDialogueBox.transform.SetSiblingIndex(sIdx);

            modularDialogueBox.SetActive(false);

        }



        private List<SundayLastBreath.Data.PassageDialogueLine> currentActiveDialogueLines = new List<SundayLastBreath.Data.PassageDialogueLine>();



        private List<SundayLastBreath.Data.PassageDialogueLine> GetActiveDialogueLines(SundayLastBreath.Data.StoryPassageData passage)

        {

            var list = new List<SundayLastBreath.Data.PassageDialogueLine>();

            if (passage == null || passage.dialogueLines == null) return list;



            foreach (var line in passage.dialogueLines)

            {

                if (line == null) continue;

                if (!string.IsNullOrEmpty(line.requiresStoryFlag))

                {

                    if (currentPlayer == null || !currentPlayer.HasFlag(line.requiresStoryFlag))

                        continue;

                }

                if (!string.IsNullOrEmpty(line.requiresNotFlag))

                {

                    if (currentPlayer != null && currentPlayer.HasFlag(line.requiresNotFlag))

                        continue;

                }

                list.Add(line);

            }

            return list;

        }



        private void DisplayPassageDialogueLine(SundayLastBreath.Data.StoryPassageData passage, int lineIdx)

        {

            if (passage == null) return;

            if (currentActiveDialogueLines == null || lineIdx >= currentActiveDialogueLines.Count)

            {

                EndPassageDialogueAndShowChoices(passage);

                return;

            }



            EnsureModularDialogueBox();

            if (modularDialogueBox == null)

            {

                EndPassageDialogueAndShowChoices(passage);

                return;

            }



            var line = currentActiveDialogueLines[lineIdx];



            // 1. Narration Above (Situation Box) - Only display introNarration for this specific line (or storyText on line 0)

            string currentNarration = "";

            if (!string.IsNullOrEmpty(line.introNarration))

            {

                currentNarration = ParseMarkdown(line.introNarration);

            }

            else if (lineIdx == 0 && !string.IsNullOrEmpty(passage.storyText))

            {

                currentNarration = ParseMarkdown(passage.storyText);

            }



            UpdateSituationBoxTextAndHeight(currentNarration);



            // 2. Activate Dialogue Box

            modularDialogueBox.SetActive(true);



            // 3. Layout Left vs Right

            bool isRight = line.side == SundayLastBreath.Data.DialogueSide.Right;

            if (modularDialogueGoldBar != null)

            {

                modularDialogueGoldBar.anchorMin = new Vector2(isRight ? 1f : 0f, 0f);

                modularDialogueGoldBar.anchorMax = new Vector2(isRight ? 1f : 0f, 1f);

                modularDialogueGoldBar.pivot = new Vector2(isRight ? 1f : 0f, 0.5f);

                modularDialogueGoldBar.anchoredPosition = Vector2.zero;

            }



            if (modularDialogueAvatarContainer != null)

            {

                modularDialogueAvatarContainer.anchorMin = new Vector2(isRight ? 1f : 0f, 0.5f);

                modularDialogueAvatarContainer.anchorMax = new Vector2(isRight ? 1f : 0f, 0.5f);

                modularDialogueAvatarContainer.pivot = new Vector2(isRight ? 1f : 0f, 0.5f);

                modularDialogueAvatarContainer.anchoredPosition = new Vector2(isRight ? -14f : 14f, 0f);

            }



            if (modularDialogueSpeechContainer != null)

            {

                modularDialogueSpeechContainer.anchorMin = new Vector2(0f, 0f);

                modularDialogueSpeechContainer.anchorMax = new Vector2(1f, 1f);

                modularDialogueSpeechContainer.pivot = new Vector2(isRight ? 1f : 0f, 0.5f);

                modularDialogueSpeechContainer.offsetMin = new Vector2(isRight ? 14f : 96f, 6f);

                modularDialogueSpeechContainer.offsetMax = new Vector2(isRight ? -96f : -14f, -6f);

            }



            // 4. Speaker Text

            if (modularDialogueSpeakerText != null)

            {

                modularDialogueSpeakerText.alignment = isRight ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;

                if (string.Equals(line.speakerName, "YOU", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(line.speakerName))

                {

                    modularDialogueSpeakerText.text = currentPlayer != null ? $"YOU // {currentPlayer.FullName.ToUpper()}" : "YOU";

                }

                else

                {

                    modularDialogueSpeakerText.text = line.speakerName.ToUpper();

                }

            }



            // 5. Speech Text & Dynamic Dialogue Box Height

            if (modularDialogueSpeechText != null)

            {

                modularDialogueSpeechText.alignment = isRight ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;

                string quote = ParseMarkdown(line.dialogueText);

                if (!quote.StartsWith("\"")) quote = $"\"{quote}\"";

                modularDialogueSpeechText.text = quote;

                modularDialogueSpeechText.ForceMeshUpdate();



                float speechH = modularDialogueSpeechText.preferredHeight;

                float boxH = Mathf.Clamp(speechH + 34f, 86f, 115f);

                LayoutElement le = modularDialogueBox.GetComponent<LayoutElement>();

                if (le != null)

                {

                    le.preferredHeight = boxH;

                    le.minHeight = boxH;

                }

            }



            // 6. Avatar Setup & Animation

            if (modularAvatarAnimRoutine != null)

            {

                StopCoroutine(modularAvatarAnimRoutine);

                modularAvatarAnimRoutine = null;

            }



            if (modularDialogueAvatar != null)

            {

                if (line.animatedAvatarFrames != null && line.animatedAvatarFrames.Count > 0)

                {

                    modularAvatarAnimRoutine = StartCoroutine(AnimateModularDialogueAvatar(line.animatedAvatarFrames, line.avatarFPS));

                }

                else if (line.avatarSprite != null)

                {

                    modularDialogueAvatar.sprite = line.avatarSprite;

                }

                else if (string.Equals(line.speakerName, "YOU", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(line.speakerName))

                {

                    if (cachedAvatarFrames == null || cachedAvatarFrames.Count == 0)

                    {

                        cachedAvatarFrames = CharacterCreationScreen.LoadPlayerFrames();

                    }

                    if (cachedAvatarFrames != null && cachedAvatarFrames.Count > 0)

                    {

                        modularAvatarAnimRoutine = StartCoroutine(AnimateModularDialogueAvatar(cachedAvatarFrames, 8f));

                    }

                    else if (monologueAvatar != null && monologueAvatar.sprite != null)

                    {

                        modularDialogueAvatar.sprite = monologueAvatar.sprite;

                    }

                }

            }



            // 7. Navigation Flow: Single Stepped Continue Button (Hide Command Subheader)

            SetChoiceSubHeaderVisible(false);

            ClearHallwayChoices();

            SetHallwayChoice(0, "▶ Continue... <color=#94A3B8>[Click / Space / 1]</color>", () =>

            {

                AdvancePassageDialogue(passage, lineIdx);

            });

        }



        private void AdvancePassageDialogue(SundayLastBreath.Data.StoryPassageData passage, int currentIdx)

        {

            if (passage == null) return;

            if (currentActiveDialogueLines != null && currentIdx + 1 < currentActiveDialogueLines.Count)

            {

                currentPassageDialogueIndex = currentIdx + 1;

                DisplayPassageDialogueLine(passage, currentPassageDialogueIndex);

            }

            else

            {

                EndPassageDialogueAndShowChoices(passage);

            }

        }



        private void EndPassageDialogueAndShowChoices(SundayLastBreath.Data.StoryPassageData passage)

        {

            if (modularDialogueBox != null) modularDialogueBox.SetActive(false);

            if (modularAvatarAnimRoutine != null) { StopCoroutine(modularAvatarAnimRoutine); modularAvatarAnimRoutine = null; }



            // Display outro narration if available on the last line, otherwise room story text

            string narrativeSummary = "";

            if (currentActiveDialogueLines != null && currentActiveDialogueLines.Count > 0)

            {

                var lastLine = currentActiveDialogueLines[currentActiveDialogueLines.Count - 1];

                if (!string.IsNullOrEmpty(lastLine.outroNarration))

                {

                    narrativeSummary = ParseMarkdown(lastLine.outroNarration);

                }

            }



            if (string.IsNullOrEmpty(narrativeSummary) && !string.IsNullOrEmpty(passage.storyText))

            {

                narrativeSummary = ParseMarkdown(passage.storyText);

            }



            UpdateSituationBoxTextAndHeight(narrativeSummary);

            SetChoiceSubHeaderVisible(true);

            RenderPassageChoices(passage);

        }



        private IEnumerator AnimateModularDialogueAvatar(List<Sprite> frames, float fps)

        {

            if (frames == null || frames.Count == 0 || modularDialogueAvatar == null) yield break;

            int idx = 0;

            float frameDelay = fps > 0f ? (1f / fps) : 0.125f;

            while (modularDialogueBox != null && modularDialogueBox.activeSelf)

            {

                if (modularDialogueAvatar != null)

                {

                    modularDialogueAvatar.sprite = frames[idx];

                    idx = (idx + 1) % frames.Count;

                }

                yield return new WaitForSecondsRealtime(frameDelay);

            }

        }



        private void DisplayPassageBeat(SundayLastBreath.Data.StoryPassageData passage, int beatIdx)

        {

            if (passage == null) return;

            if (passage.narrativeBeats == null || beatIdx >= passage.narrativeBeats.Count)

            {

                UpdateSituationBoxTextAndHeight(ParseMarkdown(passage.storyText));

                SetChoiceSubHeaderVisible(true);

                RenderPassageChoices(passage);

                return;

            }



            UpdateSituationBoxTextAndHeight(ParseMarkdown(passage.narrativeBeats[beatIdx]));

            SetChoiceSubHeaderVisible(false);

            ClearHallwayChoices();



            SetHallwayChoice(0, "▶ Continue... <color=#94A3B8>[Click / Space / 1]</color>", () =>

            {

                if (beatIdx + 1 < passage.narrativeBeats.Count)

                {

                    currentPassageBeatIndex = beatIdx + 1;

                    DisplayPassageBeat(passage, currentPassageBeatIndex);

                }

                else

                {

                    UpdateSituationBoxTextAndHeight(ParseMarkdown(passage.storyText));

                    SetChoiceSubHeaderVisible(true);

                    RenderPassageChoices(passage);

                }

            });

        }



        private bool EvaluateChoiceRequirements(

            SundayLastBreath.Data.PassageChoiceData choice,

            out bool meetsRequirements,

            out string requirementNote)

        {

            meetsRequirements = true;

            requirementNote = "";



            if (choice == null) return true;



            // 1. Mandatory Story Flag Checks

            if (!string.IsNullOrEmpty(choice.requiresStoryFlag))

            {

                bool hasFlag = currentPlayer != null && currentPlayer.HasFlag(choice.requiresStoryFlag);

                if (!hasFlag)

                {

                    if (choice.hideWhenUnavailable)

                    {

                        // Completely hide this choice if player hasn't discovered the prerequisite

                        return false;

                    }

                    meetsRequirements = false;

                    requirementNote = $"[Req: {choice.requiresStoryFlag}]";

                }

            }



            if (!string.IsNullOrEmpty(choice.requiresNotFlag))

            {

                bool hasProhibitedFlag = currentPlayer != null && currentPlayer.HasFlag(choice.requiresNotFlag);

                if (hasProhibitedFlag)

                {

                    if (choice.hideWhenUnavailable)

                    {

                        return false;

                    }

                    meetsRequirements = false;

                    requirementNote = "[Locked Out]";

                }

            }



            // 2. Individual RPG & Item Checks

            bool hasItemReq = !string.IsNullOrEmpty(choice.requiredItem);

            bool meetsItem = !hasItemReq || (currentPlayer != null && (currentPlayer.HasItem(choice.requiredItem) || currentPlayer.HasKeyItem(choice.requiredItem)));



            bool hasStatReq = !string.IsNullOrEmpty(choice.requiredStat) && choice.requiredStatValue > 0;

            bool meetsStat = !hasStatReq || (currentPlayer != null && GetPlayerAttributeValue(choice.requiredStat) >= choice.requiredStatValue);



            bool hasSkillReq = !string.IsNullOrEmpty(choice.requiredSkill) && choice.requiredSkillLevel > 0;

            bool meetsSkill = !hasSkillReq || (currentPlayer != null && GetPlayerSkillLevel(choice.requiredSkill) >= choice.requiredSkillLevel);



            bool hasTraitReq = !string.IsNullOrEmpty(choice.requiredTrait);

            bool meetsTrait = !hasTraitReq || (currentPlayer != null && (

                currentPlayer.HasTrait(choice.requiredTrait) ||

                string.Equals(currentPlayer.job, choice.requiredTrait, StringComparison.OrdinalIgnoreCase) ||

                (choice.requiredTrait.IndexOf("RADIO", StringComparison.OrdinalIgnoreCase) >= 0 && string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase))

            ));



            List<string> reqLabels = new List<string>();

            if (hasItemReq)

            {

                var def = SundayLastBreath.Data.ItemDatabase.GetDefinition(choice.requiredItem);

                reqLabels.Add(def != null ? def.displayName : choice.requiredItem);

            }

            if (hasStatReq)

            {

                reqLabels.Add($"{choice.requiredStat.ToUpperInvariant()} {choice.requiredStatValue}");

            }

            if (hasSkillReq)

            {

                reqLabels.Add($"{choice.requiredSkill.ToUpperInvariant()} {choice.requiredSkillLevel}");

            }

            if (hasTraitReq)

            {

                reqLabels.Add(choice.requiredTrait.ToUpperInvariant());

            }



            bool hasAnyDefinedRequirement = reqLabels.Count > 0;



            if (choice.requirementMode == SundayLastBreath.Data.ChoiceRequirementMode.AnyCanBeMet && hasAnyDefinedRequirement)

            {

                // Player needs ANY ONE of Item, Stat, Skill, or Trait

                bool anyMet = (hasItemReq && meetsItem) ||

                              (hasStatReq && meetsStat) ||

                              (hasSkillReq && meetsSkill) ||

                              (hasTraitReq && meetsTrait);



                if (!anyMet)

                {

                    if (choice.hideWhenUnavailable) return false;

                    meetsRequirements = false;

                    requirementNote = $"[Req: {string.Join(" or ", reqLabels)}]";

                }

            }

            else if (hasAnyDefinedRequirement)

            {

                // AllMustBeMet

                if (!meetsItem || !meetsStat || !meetsSkill || !meetsTrait)

                {

                    if (choice.hideWhenUnavailable) return false;

                    meetsRequirements = false;

                    requirementNote = $"[Req: {string.Join(", ", reqLabels)}]";

                }

            }



            return true; // Visible in UI

        }



        private int GetPlayerAttributeValue(string statName)

        {

            if (currentPlayer == null || string.IsNullOrEmpty(statName)) return 0;

            switch (statName.ToUpperInvariant())

            {

                case "STR":

                case "MIGHT":

                    return currentPlayer.str;

                case "PER":

                case "OBSERVATION":

                    return currentPlayer.per;

                case "AGI":

                case "REFLEX":

                    return currentPlayer.agi;

                case "INT":

                case "INTL":

                case "TACTICS":

                    return currentPlayer.intl;

                case "LCK":

                case "AUSPICE":

                    return currentPlayer.lck;

                case "CHA":

                case "LEADERSHIP":

                    return currentPlayer.cha;

                default:

                    return 0;

            }

        }



        private int GetPlayerSkillLevel(string skillName)

        {

            if (currentPlayer == null || string.IsNullOrEmpty(skillName)) return 0;

            switch (skillName.ToLowerInvariant())

            {

                case "tech": return currentPlayer.tech;

                case "shooting": return currentPlayer.shooting;

                case "melee": return currentPlayer.melee;

                case "stealth": return currentPlayer.stealth;

                case "medicine": return currentPlayer.medicine;

                case "scavenging": return currentPlayer.scavenging;

                case "survival": return currentPlayer.survival;

                case "resolve": return currentPlayer.resolve;

                default: return 0;

            }

        }



        private void RenderPassageChoices(SundayLastBreath.Data.StoryPassageData passage)

        {

            ClearHallwayChoices();



            if (passage == null || passage.choices == null) return;



            int visibleBtnIndex = 0;

            for (int i = 0; i < passage.choices.Count; i++)

            {

                var choice = passage.choices[i];



                // Evaluate visibility and requirements

                if (!EvaluateChoiceRequirements(choice, out bool meetsReq, out string reqNote))

                {

                    // Choice is hidden (e.g. phone code option before player has checked phone!)

                    continue;

                }



                string fullLabel = choice.choiceLabel;

                if (!string.IsNullOrEmpty(choice.choiceSubtitle))

                {

                    fullLabel += $" <color=#94A3B8>- {choice.choiceSubtitle}</color>";

                }



                if (!meetsReq && !string.IsNullOrEmpty(reqNote))

                {

                    fullLabel += $" <color=#EF4444>{reqNote}</color>";

                }



                Color? accentColor = choice.isBackButton ? new Color(0.6f, 0.65f, 0.7f) : (Color?)null;



                int btnIndex = visibleBtnIndex++;

                SetHallwayChoice(btnIndex, fullLabel, () =>

                {

                    ExecutePassageChoice(choice);

                }, meetsReq, accentColor);

            }

        }



        public void ExecutePassageChoice(SundayLastBreath.Data.PassageChoiceData choice)

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



            // Story Flags consequence

            if (!string.IsNullOrEmpty(choice.setsStoryFlagOnExecute) && currentPlayer != null)

            {

                currentPlayer.SetFlag(choice.setsStoryFlagOnExecute, true);

            }

            if (!string.IsNullOrEmpty(choice.clearsStoryFlagOnExecute) && currentPlayer != null)

            {

                currentPlayer.SetFlag(choice.clearsStoryFlagOnExecute, false);

            }



            // Special interactive minigames / triggers

            if (!string.IsNullOrEmpty(choice.triggerActionId))

            {

                if (string.Equals(choice.triggerActionId, "TRIGGER_QTE", StringComparison.OrdinalIgnoreCase) ||

                    string.Equals(choice.triggerActionId, "OPEN_QTE_MODAL", StringComparison.OrdinalIgnoreCase) ||

                    choice.triggerActionId.IndexOf("QTE", StringComparison.OrdinalIgnoreCase) >= 0)

                {

                    OpenQteModal(choice);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_LOCKPICK_MODAL", StringComparison.OrdinalIgnoreCase) ||

                         string.Equals(choice.triggerActionId, "LOCKPICK", StringComparison.OrdinalIgnoreCase) ||

                         choice.triggerActionId.IndexOf("LOCKPICK", StringComparison.OrdinalIgnoreCase) >= 0)

                {

                    OpenLockpickingModal(choice);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_COMBAT_MODAL", StringComparison.OrdinalIgnoreCase) ||

                         string.Equals(choice.triggerActionId, "COMBAT", StringComparison.OrdinalIgnoreCase) ||

                         choice.triggerActionId.IndexOf("COMBAT", StringComparison.OrdinalIgnoreCase) >= 0)

                {

                    OpenTacticalCombatModal(choice);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_KEYPAD_MODAL", StringComparison.OrdinalIgnoreCase))

                {

                    OpenKeypadPuzzleModal(choice);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_PHONE_MODAL", StringComparison.OrdinalIgnoreCase))

                {

                    OnCheckPhoneClicked();

                    if (CurrentPassage != null) RenderPassageChoices(CurrentPassage);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_RADIO_MODAL", StringComparison.OrdinalIgnoreCase))

                {

                    OnListenRadioClicked();

                    if (CurrentPassage != null) RenderPassageChoices(CurrentPassage);

                    return;

                }

                else if (string.Equals(choice.triggerActionId, "OPEN_HACKING_MODAL", StringComparison.OrdinalIgnoreCase))

                {

                    OnHackTerminalClicked();

                    if (CurrentPassage != null) RenderPassageChoices(CurrentPassage);

                    return;

                }

            }



            // Item loot / award (single item)

            if (!string.IsNullOrEmpty(choice.givesItem))

            {

                if (choice.dropLootDirectlyToFloor)

                {

                    AddFloorItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));

                    tacticalGearController?.RefreshAll();

                    UpdateFloorItemsNotification();

                }

                else

                {

                    GiveOrDropItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));

                }

            }



            // Item loot / award (multiple items list)

            if (choice.givesLootList != null && choice.givesLootList.Count > 0)

            {

                foreach (var loot in choice.givesLootList)

                {

                    if (!string.IsNullOrEmpty(loot.itemId) && loot.quantity > 0)

                    {

                        if (choice.dropLootDirectlyToFloor)

                        {

                            AddFloorItem(loot.itemId, loot.quantity);

                        }

                        else

                        {

                            GiveOrDropItem(loot.itemId, loot.quantity);

                        }

                    }

                }

                tacticalGearController?.RefreshAll();

                UpdateFloorItemsNotification();

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

                if (choice.transitionVideo != null || !string.IsNullOrEmpty(choice.transitionVideoPath))

                {

                    StartCoroutine(PlayPassageVideoTransition(choice, choice.targetPassage));

                }

                else

                {

                    PlayStoryPassage(choice.targetPassage);

                }

            }

            else if (choice.successPassage == null && choice.failurePassage == null && CurrentPassage != null)

            {

                // Remained on same passage (e.g. searched closet/cabinet, acquired items, toggled flags)

                RenderPassageChoices(CurrentPassage);

            }

        }



        private void OpenKeypadPuzzleModal(SundayLastBreath.Data.PassageChoiceData choice)

        {

            if (keypadModal == null)

            {

                keypadModal = GetComponentInChildren<KeypadPuzzleModal>(true);

                if (keypadModal == null)

                {

                    Canvas canvas = GetComponentInParent<Canvas>();

                    if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

                    GameObject kpObj = new GameObject("KeypadPuzzleModal", typeof(RectTransform));

                    if (canvas != null) kpObj.transform.SetParent(canvas.transform, false);

                    keypadModal = kpObj.AddComponent<KeypadPuzzleModal>();

                }

            }



            keypadModal.OpenKeypad(

                onUnlocked: () =>

                {

                    if (currentPlayer != null)

                    {

                        currentPlayer.SetFlag("room406_opened", true);

                    }

                    room406_opened = true;

                    PlayClickSFX();



                    if (choice != null && choice.targetPassage != null)

                    {

                        if (choice.transitionVideo != null || !string.IsNullOrEmpty(choice.transitionVideoPath))

                        {

                            StartCoroutine(PlayPassageVideoTransition(choice, choice.targetPassage));

                        }

                        else

                        {

                            PlayStoryPassage(choice.targetPassage);

                        }

                    }

                    else

                    {

                        OpenUnit406Door();

                    }

                },

                onLockedOut: () =>

                {

                    if (currentPlayer != null)

                    {

                        currentPlayer.SetFlag("room406_locked_out", true);

                    }

                    room406_locked_out = true;

                    PlayBeepSFX();

                    if (CurrentPassage != null)

                    {

                        RenderPassageChoices(CurrentPassage);

                    }

                    else

                    {

                        InspectUnit406();

                    }

                },

                onCancelled: () =>

                {

                    PlayClickSFX();

                },

                hasObservationClue: (currentPlayer != null && currentPlayer.per >= 6)

            );

        }



        private void OpenQteModal(SundayLastBreath.Data.PassageChoiceData choice)

        {

            if (qteModal == null)

            {

                qteModal = GetComponentInChildren<QteTimingModal>(true);

                if (qteModal == null)

                {

                    Canvas canvas = GetComponentInParent<Canvas>();

                    if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

                    GameObject qteObj = new GameObject("QteTimingModal", typeof(RectTransform));

                    if (canvas != null) qteObj.transform.SetParent(canvas.transform, false);

                    qteModal = qteObj.AddComponent<QteTimingModal>();

                }

            }



            // Calculate difficulty scaling dynamically if stat check is present

            float speed = 2.4f;

            float safeWidth = 24f;



            if (!string.IsNullOrEmpty(choice.requiredStat) && currentPlayer != null)

            {

                int playerStatVal = 3;

                string s = choice.requiredStat.ToLowerInvariant();

                if (s.Contains("might") || s == "str") playerStatVal = currentPlayer.Might;

                else if (s.Contains("obs") || s == "per") playerStatVal = currentPlayer.Observation;

                else if (s.Contains("ref") || s == "agi") playerStatVal = currentPlayer.Reflex;

                else if (s.Contains("tac") || s == "int") playerStatVal = currentPlayer.Tactics;

                else if (s.Contains("aus") || s == "lck") playerStatVal = currentPlayer.Auspice;

                else if (s.Contains("lead") || s == "cha") playerStatVal = currentPlayer.Leadership;



                int diff = Mathf.Max(0, choice.requiredStatValue - playerStatVal);

                speed = Mathf.Clamp(2.0f + (diff * 0.5f), 1.6f, 4.2f);

                safeWidth = Mathf.Clamp(28f - (diff * 4f), 12f, 36f);

            }



            string prompt = !string.IsNullOrEmpty(choice.choiceSubtitle) 

                ? choice.choiceSubtitle 

                : "Keep the tension steady. Click or hit <b>[SPACE]</b> when the needle enters the <color=#10B981>GREEN safe zone</color>!";



            qteModal.StartQTE(

                actionTitle: choice.choiceLabel,

                promptDescription: prompt,

                speed: speed,

                safeZoneWidthPercent: safeWidth,

                safeZoneCenterPercent: 50f,

                onSuccess: () =>

                {

                    PlayClickSFX();



                    // Award loot / item

                    if (!string.IsNullOrEmpty(choice.givesItem))

                    {

                        GiveOrDropItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));

                    }



                    // Consequence flags

                    if (!string.IsNullOrEmpty(choice.setsStoryFlagOnExecute) && currentPlayer != null)

                    {

                        currentPlayer.SetFlag(choice.setsStoryFlagOnExecute, true);

                    }



                    // Advance clock

                    gameClock.AddTime(Mathf.Max(1, choice.timeMinutesCost));



                    // Stamina cost

                    if (choice.staminaCost > 0 && currentPlayer != null)

                    {

                        currentPlayer.stamina = Mathf.Max(0, currentPlayer.stamina - choice.staminaCost);

                        UpdateVitalsUI();

                        tacticalGearController?.RefreshAll();

                    }



                    // Transition to success / target passage

                    if (choice.successPassage != null)

                    {

                        PlayStoryPassage(choice.successPassage);

                    }

                    else if (choice.targetPassage != null)

                    {

                        PlayStoryPassage(choice.targetPassage);

                    }

                    else if (CurrentPassage != null)

                    {

                        PlayStoryPassage(CurrentPassage);

                    }

                },

                onFailure: () =>

                {

                    // Penalize HP / Stamina

                    int hpDamage = choice.healthCost > 0 ? choice.healthCost : 15;

                    int staminaDmg = choice.staminaCost > 0 ? choice.staminaCost : 20;



                    if (currentPlayer != null)

                    {

                        currentPlayer.hp = Mathf.Max(0, currentPlayer.hp - hpDamage);

                        currentPlayer.stamina = Mathf.Max(0, currentPlayer.stamina - staminaDmg);

                        UpdateVitalsUI();

                        tacticalGearController?.RefreshAll();

                    }



                    // Advance clock

                    gameClock.AddTime(Mathf.Max(1, choice.timeMinutesCost));



                    // Set failure flag if in 401

                    if (CurrentPassage != null && CurrentPassage.passageId.Contains("401") && currentPlayer != null)

                    {

                        currentPlayer.SetFlag("room401_lift_fail", true);

                    }



                    // Transition to failure passage or re-render room

                    if (choice.failurePassage != null)

                    {

                        PlayStoryPassage(choice.failurePassage);

                    }

                    else if (CurrentPassage != null)

                    {

                        PlayStoryPassage(CurrentPassage);

                    }

                },

                onCancelled: () =>

                {

                    PlayClickSFX();

                }

            );

        }



        public void OpenLockpickingModal(SundayLastBreath.Data.PassageChoiceData choice)

        {

            if (lockpickModal == null)

            {

                lockpickModal = GetComponentInChildren<LockpickingModal>(true);

                if (lockpickModal == null)

                {

                    Canvas canvas = GetComponentInParent<Canvas>();

                    if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

                    GameObject lpObj = new GameObject("LockpickingModal", typeof(RectTransform));

                    if (canvas != null) lpObj.transform.SetParent(canvas.transform, false);

                    lockpickModal = lpObj.AddComponent<LockpickingModal>();

                }

            }



            bool hasLocksmith = currentPlayer != null && currentPlayer.HasTrait("LOCKSMITH");

            int difficulty = choice.skillDC > 0 ? choice.skillDC : 50;

            int numPins = difficulty >= 70 ? 5 : (difficulty >= 40 ? 4 : 3);

            int baseForce = Mathf.Clamp(100 - difficulty, 15, 85);



            lockpickModal.StartLockpick(

                numPins: numPins,

                difficulty: difficulty,

                forcePercent: baseForce,

                hasLocksmith: hasLocksmith,

                onUnlocked: () =>

                {

                    PlayClickSFX();



                    // Story flag

                    if (!string.IsNullOrEmpty(choice.setsStoryFlagOnExecute) && currentPlayer != null)

                    {

                        currentPlayer.SetFlag(choice.setsStoryFlagOnExecute, true);

                    }



                    // Loot / item

                    if (!string.IsNullOrEmpty(choice.givesItem))

                    {

                        GiveOrDropItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));

                    }



                    // Time cost

                    gameClock.AddTime(Mathf.Max(2, choice.timeMinutesCost));



                    // Transition to success / target passage

                    if (choice.successPassage != null)

                    {

                        PlayStoryPassage(choice.successPassage);

                    }

                    else if (choice.targetPassage != null)

                    {

                        PlayStoryPassage(choice.targetPassage);

                    }

                    else if (CurrentPassage != null)

                    {

                        PlayStoryPassage(CurrentPassage);

                    }

                },

                onFailed: () =>

                {

                    // Advance clock

                    gameClock.AddTime(5);



                    // Chance to consume / break lockpick

                    if (!hasLocksmith && currentPlayer != null && currentPlayer.inventory != null)

                    {

                        var pick = currentPlayer.inventory.Find(i => i.itemId.IndexOf("lockpick", StringComparison.OrdinalIgnoreCase) >= 0);

                        if (pick != null)

                        {

                            pick.quantity--;

                            if (pick.quantity <= 0) currentPlayer.inventory.Remove(pick);

                            tacticalGearController?.RefreshAll();

                        }

                    }



                    if (choice.failurePassage != null)

                    {

                        PlayStoryPassage(choice.failurePassage);

                    }

                    else if (CurrentPassage != null)

                    {

                        PlayStoryPassage(CurrentPassage);

                    }

                },

                onCancelled: () =>

                {

                    PlayClickSFX();

                }

            );

        }



        public void OpenTacticalCombatModal(SundayLastBreath.Data.PassageChoiceData choice)

        {

            if (combatModal == null)

            {

                combatModal = GetComponentInChildren<TacticalCombatModal>(true);

                if (combatModal == null)

                {

                    Canvas canvas = GetComponentInParent<Canvas>();

                    if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

                    GameObject cbObj = new GameObject("TacticalCombatModal", typeof(RectTransform));

                    if (canvas != null) cbObj.transform.SetParent(canvas.transform, false);

                    combatModal = cbObj.AddComponent<TacticalCombatModal>();

                }

            }



            var enemies = new List<TacticalCombatModal.CombatEnemyData>();

            string pid = CurrentPassage != null && !string.IsNullOrEmpty(CurrentPassage.passageId) ? CurrentPassage.passageId : "";



            // 1. Check if specific CombatEnemyProfile ScriptableObjects are assigned on the choice in Inspector

            if (choice != null && choice.combatEnemies != null && choice.combatEnemies.Count > 0)

            {

                foreach (var prof in choice.combatEnemies)

                {

                    if (prof != null) enemies.Add(TacticalCombatModal.FromProfile(prof));

                }

            }



            // 2. Check if comma-separated enemy IDs are specified on the choice

            if (enemies.Count == 0 && choice != null && !string.IsNullOrWhiteSpace(choice.combatEnemyIdsCsv))

            {

                var ids = choice.combatEnemyIdsCsv.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var id in ids)

                {

                    enemies.Add(TacticalCombatModal.CreateEnemyById(id.Trim()));

                }

            }



            // 3. Fallback to contextual passage enemies (loaded from CombatEnemyProfile assets if present!)

            if (enemies.Count == 0)

            {

                if (pid.IndexOf("7Eleven", StringComparison.OrdinalIgnoreCase) >= 0)

                {

                    enemies.Add(TacticalCombatModal.CreateEnemyById("zombie_motorist"));

                    enemies.Add(TacticalCombatModal.CreateEnemyById("zombie_passenger"));

                }

                else if (pid.IndexOf("Road", StringComparison.OrdinalIgnoreCase) >= 0)

                {

                    enemies.Add(TacticalCombatModal.CreateEnemyById("zombie_motorist"));

                }

                else

                {

                    enemies.Add(TacticalCombatModal.CreateEnemyById("zombie_commuter"));

                    enemies.Add(TacticalCombatModal.CreateEnemyById("zombie_guard"));

                }

            }



            combatModal.StartEncounter(

                playerData: currentPlayer,

                enemies: enemies,

                onVictory: () =>

                {

                    PlayClickSFX();

                    UpdateVitalsUI();

                    tacticalGearController?.RefreshAll();



                    if (choice != null)

                    {

                        if (!string.IsNullOrEmpty(choice.setsStoryFlagOnExecute) && currentPlayer != null)

                        {

                            currentPlayer.SetFlag(choice.setsStoryFlagOnExecute, true);

                        }

                        if (!string.IsNullOrEmpty(choice.givesItem))

                        {

                            GiveOrDropItem(choice.givesItem, Mathf.Max(1, choice.givesItemQty));

                        }

                        if (choice.successPassage != null)

                        {

                            PlayStoryPassage(choice.successPassage);

                            return;

                        }

                        if (choice.targetPassage != null)

                        {

                            PlayStoryPassage(choice.targetPassage);

                            return;

                        }

                    }



                    if (CurrentPassage != null) PlayStoryPassage(CurrentPassage);

                },

                onDefeat: () =>

                {

                    UpdateVitalsUI();

                    if (choice != null && choice.failurePassage != null)

                    {

                        PlayStoryPassage(choice.failurePassage);

                    }

                }

            );

        }



        private IEnumerator PlayPassageVideoTransition(SundayLastBreath.Data.PassageChoiceData choice, SundayLastBreath.Data.StoryPassageData targetPassage)

        {

            if (targetPassage == null || choice == null) yield break;



            EnsureDoorTransitionOverlay();

            if (doorTransitionOverlay == null || doorVideoPlayer == null)

            {

                PlayStoryPassage(targetPassage);

                yield break;

            }



            var clip = choice.transitionVideo;

            var path = choice.transitionVideoPath;



            // Set video source

            bool hasVideo = false;

            if (clip != null)

            {

                doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;

                doorVideoPlayer.clip = clip;

                hasVideo = true;

            }

            else if (!string.IsNullOrEmpty(path))

            {

                var loaded = Resources.Load<UnityEngine.Video.VideoClip>(path);

                if (loaded != null)

                {

                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;

                    doorVideoPlayer.clip = loaded;

                    hasVideo = true;

                }

                else

                {

                    string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", path).Replace("\\", "/");

                    if (!filePath.EndsWith(".mp4")) filePath += ".mp4";

                    if (System.IO.File.Exists(filePath))

                    {

                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;

                        doorVideoPlayer.url = filePath;

                        hasVideo = true;

                    }

                }

            }



            if (!hasVideo)

            {

                PlayStoryPassage(targetPassage);

                yield break;

            }



            // Optional BGM ducking during cinematic video transition

            if (choice.fadeOutBgmDuringTransition && atmosphereSource != null && atmosphereSource.isPlaying)

            {

                StartCoroutine(FadeAtmosphereAudio(0.05f, 0.4f));

            }



            isDoorTransitioning = true;

            doorTransitionOverlay.SetActive(true);

            doorTransitionContentBox?.SetActive(false);

            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 0f;



            doorVideoPlayer.Prepare();



            // Fade in black curtain (0.35s)

            float fadeInTime = 0.35f;

            for (float t = 0; t < fadeInTime; t += Time.unscaledDeltaTime)

            {

                if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);

                yield return null;

            }

            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 1f;



            // Wait for video prepare

            float prepTimer = 0f;

            while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)

            {

                prepTimer += Time.unscaledDeltaTime;

                yield return null;

            }



            if (doorVideoPlayer.isPrepared)

            {

                // Play custom start SFX or default knob

                if (choice.transitionStartSFX != null) PlaySFX(choice.transitionStartSFX, 0.95f);

                else if (doorTransitionKnobSFX != null) PlaySFX(doorTransitionKnobSFX, 0.95f);

                else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);



                doorVideoPlayer.Play();



                // Fade video in from black

                float vidFadeIn = choice.videoFadeInDuration > 0f ? choice.videoFadeInDuration : 0.35f;

                for (float t = 0; t < vidFadeIn; t += Time.unscaledDeltaTime)

                {

                    float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);

                    if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                    yield return null;

                }

                if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;



                // Show skip prompt

                if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);



                float vidLen = (float)doorVideoPlayer.length;

                if (vidLen <= 0.5f) vidLen = 3.5f;



                float midTime = vidLen * 0.55f;

                float elapsed = 0.2f + vidFadeIn;

                bool playedStep = false;

                bool skipped = false;



                while (elapsed < vidLen - 0.35f)

                {

                    if (CheckDoorTransitionSkipInput())

                    {

                        skipped = true;

                        break;

                    }



                    if (!playedStep && elapsed >= midTime)

                    {

                        playedStep = true;

                        if (choice.transitionStepSFX != null) PlaySFX(choice.transitionStepSFX, 0.85f);

                        else if (doorTransitionFootstepSFX != null) PlaySFX(doorTransitionFootstepSFX, 0.8f);

                        else PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);

                    }



                    elapsed += Time.unscaledDeltaTime;

                    yield return null;

                }



                if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);



                if (skipped)

                {

                    if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

                    if (choice.transitionEndSFX != null) PlaySFX(choice.transitionEndSFX, 0.9f);

                    else if (doorTransitionPlaySlamSFX)

                    {

                        if (doorTransitionSlamSFX != null) PlaySFX(doorTransitionSlamSFX, 0.75f);

                        else PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.75f);

                    }

                    if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

                    yield return new WaitForSecondsRealtime(0.05f);

                }

                else

                {

                    // Play end / slam SFX if provided

                    if (choice.transitionEndSFX != null) PlaySFX(choice.transitionEndSFX, 0.9f);



                    // Fade video out to black

                    float vidFadeOut = 0.35f;

                    for (float t = 0; t < vidFadeOut; t += Time.unscaledDeltaTime)

                    {

                        float a = Mathf.Lerp(1f, 0f, t / vidFadeOut);

                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);

                        yield return null;

                    }

                    if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);



                    // Brief post-video pause

                    float postPause = choice.postVideoPause > 0f ? choice.postVideoPause : 0.3f;

                    yield return new WaitForSecondsRealtime(postPause);

                }

            }



            // Transition passage behind dark curtain

            PlayStoryPassage(targetPassage);



            // Fade in from black to reveal new passage

            float fadeOutTime = 0.45f;

            for (float t = 0; t < fadeOutTime; t += Time.unscaledDeltaTime)

            {

                if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);

                yield return null;

            }

            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 0f;



            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();

            doorTransitionContentBox?.SetActive(true);

            doorTransitionOverlay.SetActive(false);

            isDoorTransitioning = false;

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

        public void SetLocation(string roomId, string customSectorSubtitle = null, string thresholdTargetRoomId = null)

        {

            if (string.IsNullOrEmpty(roomId)) roomId = "unit_407";



            string roomDisplay = roomId.ToUpper();

            string sectorSubtitle = customSectorSubtitle;



            if (tacticalMapController != null)

            {

                if (!string.IsNullOrEmpty(thresholdTargetRoomId))

                {

                    tacticalMapController.SetThresholdLocation(roomId, thresholdTargetRoomId);

                }

                else

                {

                    tacticalMapController.SetCurrentLocation(roomId);

                }



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

            UpdateSituationBoxHeader(sectorSubtitle);



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

            string formattedTop = $"// {displayTitle}";



            if (!string.IsNullOrEmpty(sectorSubtitle))

            {

                string sub = sectorSubtitle.ToUpper();

                if (sub.StartsWith(displayTitle + " // "))

                {

                    formattedTop = $"// {sub}";

                }

                else

                {

                    formattedTop += $" // {sub}";

                }

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



        public void UpdateVitalsUI()

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



        public void SetTopHUDVisible(bool visible)

        {

            if (topHUDContainer == null)

            {

                Transform found = transform.Find("TopHUD");

                if (found == null && hpBarFill != null)

                {

                    Transform curr = hpBarFill.transform;

                    while (curr != null && curr != transform)

                    {

                        if (curr.name.IndexOf("TopHUD", System.StringComparison.OrdinalIgnoreCase) >= 0)

                        {

                            found = curr;

                            break;

                        }

                        curr = curr.parent;

                    }

                    if (found == null && hpBarFill.transform.parent != null)

                    {

                        found = hpBarFill.transform.parent;

                        if (found.parent != null && found.parent != transform) found = found.parent;

                    }

                }

                if (found != null) topHUDContainer = found.gameObject;

            }



            if (topHUDContainer != null)

            {

                topHUDContainer.SetActive(visible);

            }

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

                floorItemsCountText.text = $"Floor Items ({tableStash.Count})";

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
        private readonly HashSet<string> _visitedLocationAnchors = new HashSet<string>();
        private readonly Dictionary<string, int> _returnVariantIndices = new Dictionary<string, int>();
        private string lastAutoLoggedSituationText = "";

        private void SyncLastAutoLoggedSituationFromLogs()
        {
            if (_storyLogs.Count == 0)
            {
                lastAutoLoggedSituationText = "";
                return;
            }

            string lastEntry = _storyLogs[_storyLogs.Count - 1];
            int nl = lastEntry.IndexOf('\n');
            lastAutoLoggedSituationText = nl >= 0 ? lastEntry.Substring(nl + 1).Trim() : lastEntry.Trim();
        }



        private void InitializeStoryLogs()

        {

            if (_storyLogs.Count > 0) return;



            _storyLogs.Add(

                "<b><color=#EF5350>[23:43] TV BROADCAST // EMERGENCY ALERT SYSTEM (CIVIL DANGER WARNING)</color></b>\n" +

                "\"This message is transmitted at the request of the King County Sheriff's Office. This is not a test.\n\n" +

                "A Civil Danger Warning is in effect for the Seattle metro area. Authorities are responding to rapidly spreading violent incidents and a suspected contagious illness. Avoid all physical contact.\n\n" +

                "Shelter in place immediately. Major interstates are closed. Lock your doors, turn off lights, and stay inside. The National Guard is mobilizing. Lethal force is authorized.\""

            );



            _storyLogs.Add(

                "<b><color=#F5BF42>[23:44] REACTION // INNER MONOLOGUE</color></b>\n" +

                "\"Lethal force authorized... They don't lock down the whole city over a normal riot. And this front door won't stop a crowd if panic hits the hallway.\"\n\n" +

                "<i>Through the wall, heavy footsteps run past in the corridor outside, followed by someone yelling and a fire door slamming shut.</i>"

            );



            _storyLogs.Add(

                "<b><color=#4FC3F7>[23:45] APARTMENT 407 // LIVING ROOM</color></b>\n" +

                "You're standing in your dark living room while sirens wail down the street.\n\n" +

                "Your keys, wallet, phone, and everyday gear are on the coffee table. Grab what you need from around the apartment before you unlock the deadbolt."

            );



            RefreshStoryLogUI();

            SyncLastAutoLoggedSituationFromLogs();

        }



        public void AppendStoryLog(string timestamp, string header, string content)

        {

            InitializeStoryLogs();

            string trimmedContent = (content ?? "").Trim();
            if (string.IsNullOrEmpty(trimmedContent)) return;

            if (_storyLogs.Count > 0)
            {
                string prev = _storyLogs[_storyLogs.Count - 1];
                int nl = prev.IndexOf('\n');
                string prevBody = nl >= 0 ? prev.Substring(nl + 1).Trim() : prev.Trim();
                if (string.Equals(prevBody, trimmedContent, StringComparison.Ordinal))
                {
                    return;
                }
            }

            string entry = $"<b><color=#66BB6A>[{timestamp}] {header}</color></b>\n{trimmedContent}";

            _storyLogs.Add(entry);

            lastAutoLoggedSituationText = trimmedContent;

            RefreshStoryLogUI();

        }



        private void EnsureStoryLogScrollSetup()
        {
            if (storyLogContentText == null) return;

            storyLogContentText.textWrappingMode = TextWrappingModes.Normal;
            storyLogContentText.overflowMode = TextOverflowModes.Overflow;

            Transform contentTransform = storyLogContentText.transform.parent;
            if (contentTransform != null)
            {
                var vlg = contentTransform.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = true;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                }

                var fitter = contentTransform.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (storyLogModal != null)
            {
                var scroll = storyLogModal.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null && scroll.verticalScrollbar == null)
                {
                    GameObject sbObj = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
                    sbObj.transform.SetParent(scroll.transform, false);

                    RectTransform sbRt = sbObj.GetComponent<RectTransform>();
                    sbRt.anchorMin = new Vector2(1f, 0f);
                    sbRt.anchorMax = new Vector2(1f, 1f);
                    sbRt.pivot = new Vector2(1f, 0.5f);
                    sbRt.sizeDelta = new Vector2(8f, 0f);
                    sbRt.anchoredPosition = Vector2.zero;

                    Image trackImg = sbObj.GetComponent<Image>();
                    trackImg.color = new Color(0.08f, 0.11f, 0.16f, 0.9f);

                    GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
                    slidingArea.transform.SetParent(sbObj.transform, false);
                    RectTransform saRt = slidingArea.GetComponent<RectTransform>();
                    saRt.anchorMin = Vector2.zero;
                    saRt.anchorMax = Vector2.one;
                    saRt.offsetMin = new Vector2(1f, 2f);
                    saRt.offsetMax = new Vector2(-1f, -2f);

                    GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                    handleObj.transform.SetParent(slidingArea.transform, false);
                    RectTransform hRt = handleObj.GetComponent<RectTransform>();
                    hRt.anchorMin = Vector2.zero;
                    hRt.anchorMax = Vector2.one;
                    hRt.sizeDelta = Vector2.zero;

                    Image handleImg = handleObj.GetComponent<Image>();
                    handleImg.color = new Color(0.31f, 0.76f, 0.97f, 0.65f);

                    Scrollbar sb = sbObj.GetComponent<Scrollbar>();
                    sb.direction = Scrollbar.Direction.BottomToTop;
                    sb.handleRect = hRt;
                    sb.targetGraphic = handleImg;

                    scroll.verticalScrollbar = sb;
                    scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                    scroll.verticalScrollbarSpacing = 6f;
                }
            }
        }

        private void RefreshStoryLogUI()
        {
            EnsureStoryLogScrollSetup();

            if (storyLogContentText != null)
            {
                storyLogContentText.text = string.Join("\n\n<color=#2C3E50>--------------------------------------------------------------------------</color>\n\n", _storyLogs);
                if (storyLogModal != null && storyLogModal.activeInHierarchy)
                {
                    storyLogContentText.ForceMeshUpdate();
                    var contentRt = storyLogContentText.transform.parent as RectTransform;
                    if (contentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
                }
            }
        }

        public void OnOpenStoryLogClicked()
        {
            PlayClickSFX();
            InitializeStoryLogs();
            RefreshStoryLogUI();

            if (storyLogModal != null)
            {
                storyLogModal.SetActive(true);
                StartCoroutine(ScrollStoryLogToBottomNextFrame());
            }
        }

        private IEnumerator ScrollStoryLogToBottomNextFrame()
        {
            EnsureStoryLogScrollSetup();
            yield return null;
            if (storyLogContentText != null)
            {
                storyLogContentText.ForceMeshUpdate();
                var contentRt = storyLogContentText.transform.parent as RectTransform;
                if (contentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
            }
            Canvas.ForceUpdateCanvases();
            if (storyLogModal != null)
            {
                var scroll = storyLogModal.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null)
                {
                    if (scroll.content != null) LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                    Canvas.ForceUpdateCanvases();
                    scroll.velocity = Vector2.zero;
                    scroll.verticalNormalizedPosition = 0f;
                }
            }
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



    /// <summary>

    /// Type alias for ApartmentHubScreen representing the Master Main Game Screen for Act 1.

    /// Manages the visual stage, HUD vitals, PDA console tabs, and all modular story passages.

    /// </summary>

    [AddComponentMenu("Sunday's Last Breath/Main Game Screen")]

    public class MainGameScreen : ApartmentHubScreen

    {

    }

}

