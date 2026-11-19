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
using UnityEngine.Networking;

namespace SundayLastBreath.UI

{
/// <summary>
    /// Master Game Stage Controller ("Panggung").
    /// Hosts the unified gameplay stage across all zones (Unit 407, 4F Hallways, Rooms 401-408, Stairwells, and beyond).
    /// Modular Zone actors (e.g. Unit407ApartmentZone, AscottFloor4Zone) perform on this stage.
    /// Combines the visual stage animations, story reader narrative, choice deck, atmosphere audio,
    /// and survivor status/inventory cockpit.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, null, null, "ApartmentHubScreen")]
    public class GameStageScreen : ScreenBase

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
        public TacticalCombatModal CombatModal
        {
            get
            {
                if (combatModal == null)
                    combatModal = GetComponentInChildren<TacticalCombatModal>(true);
                if (combatModal == null)
                {
                    var cbObj = new GameObject("TacticalCombatModal", typeof(RectTransform));
                    cbObj.transform.SetParent(transform, false);
                    combatModal = cbObj.AddComponent<TacticalCombatModal>();
                }
                return combatModal;
            }
        }
        public bool IsCombatModalOpen => combatModal != null && combatModal.gameObject.activeInHierarchy;

[Header("Modular Story Zones (Actors)")]
        [SerializeField] private SundayLastBreath.Zones.Unit407ApartmentZone unit407Zone;
        public SundayLastBreath.Zones.Unit407ApartmentZone Unit407Zone
        {
            get
            {
                if (unit407Zone == null)
                    unit407Zone = GetComponentInChildren<SundayLastBreath.Zones.Unit407ApartmentZone>(true);
                if (unit407Zone == null)
                    unit407Zone = gameObject.AddComponent<SundayLastBreath.Zones.Unit407ApartmentZone>();
                return unit407Zone;
            }
        }

[SerializeField] private SundayLastBreath.Zones.Floor4HallwayZone floor4Zone;
        public SundayLastBreath.Zones.Floor4HallwayZone Floor4Zone
        {
            get
            {
                if (floor4Zone == null)
                    floor4Zone = GetComponentInChildren<SundayLastBreath.Zones.Floor4HallwayZone>(true);
                if (floor4Zone == null)
                    floor4Zone = gameObject.AddComponent<SundayLastBreath.Zones.Floor4HallwayZone>();
                return floor4Zone;
            }
        }

[SerializeField] private SundayLastBreath.Zones.EmergencyStairwellZone emergencyStairwellZone;
        public SundayLastBreath.Zones.EmergencyStairwellZone EmergencyStairwellZone
        {
            get
            {
                if (emergencyStairwellZone == null)
                    emergencyStairwellZone = GetComponentInChildren<SundayLastBreath.Zones.EmergencyStairwellZone>(true);
                if (emergencyStairwellZone == null)
                    emergencyStairwellZone = gameObject.AddComponent<SundayLastBreath.Zones.EmergencyStairwellZone>();
                return emergencyStairwellZone;
            }
        }

        [SerializeField] private SundayLastBreath.Zones.Floor2HallwayZone floor2Zone;
        public SundayLastBreath.Zones.Floor2HallwayZone Floor2Zone
        {
            get
            {
                if (floor2Zone == null)
                    floor2Zone = GetComponentInChildren<SundayLastBreath.Zones.Floor2HallwayZone>(true);
                if (floor2Zone == null)
                    floor2Zone = gameObject.AddComponent<SundayLastBreath.Zones.Floor2HallwayZone>();
                return floor2Zone;
            }
        }

        [SerializeField] private SundayLastBreath.Zones.GroundFloorLobbyZone groundFloorLobbyZone;
        public SundayLastBreath.Zones.GroundFloorLobbyZone GroundFloorLobbyZone
        {
            get
            {
                if (groundFloorLobbyZone == null)
                    groundFloorLobbyZone = GetComponentInChildren<SundayLastBreath.Zones.GroundFloorLobbyZone>(true);
                if (groundFloorLobbyZone == null)
                    groundFloorLobbyZone = gameObject.AddComponent<SundayLastBreath.Zones.GroundFloorLobbyZone>();
                return groundFloorLobbyZone;
            }
        }

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
if (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby)
{
    return !string.IsNullOrEmpty(currentInspectedRoom) ? currentInspectedRoom : "nw_room";
}

if (IsOnSecondFloor)
{
    if (!string.IsNullOrEmpty(currentInspectedRoom) && currentInspectedRoom.StartsWith("f2_hall_", StringComparison.OrdinalIgnoreCase))
    {
        return currentInspectedRoom;
    }
    int sec = floor2Zone != null ? Mathf.Clamp(floor2Zone.hallwaySection, 1, 4) : 1;
    return $"f2_hall_{sec}";
}

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

if (currentInspectedRoom == "stairs")
                {
                    int sp = emergencyStairwellZone != null ? emergencyStairwellZone.stair_phase : stair_phase;
                    if (sp <= 1) return "f4_entrance";
                    if (sp == 2) return "f3_landing";
                    if (sp == 3) return "f1_landing";
                    return "f2_hall_1";
                }

if (currentInspectedRoom == "utility") return currentInspectedRoom;

if (currentInspectedRoom.EndsWith("_room", StringComparison.OrdinalIgnoreCase)
    || currentInspectedRoom == "lobby_exit"
    || currentInspectedRoom.StartsWith("f2_hall_", StringComparison.OrdinalIgnoreCase))
{
    return currentInspectedRoom;
}

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
public bool isHackingTerminal => HackingTerminal != null && HackingTerminal.IsHacking;
private TextMeshProUGUI hackingLedText;
private Coroutine radioRoutine;
private Coroutine radioMeterRoutine;
public bool isRadioTransmitting => RadioScanner != null && RadioScanner.IsTransmitting;
private TextMeshProUGUI radioFreqHeaderText;
private TextMeshProUGUI radioSignalMeterText;
private TextMeshProUGUI radioTranscriptText;
private GameObject radioOutroObj;

private TextMeshProUGUI activeTypewriterTarget;

private string activeFullText = "";

// =================================================================================================
        // SECTION 1: STAGE STATE, RUNTIME SYSTEMS & INSPECTOR REFERENCES
        // Description: Serialized UI references, singleton access, tactical map controller,
        //              clock, objective system, and HUD anchor references.
        // =================================================================================================
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

private readonly List<TacticalChoiceButton> hallwayChoiceButtons = new List<TacticalChoiceButton>();

#endregion

// Cached frames for player avatar animation

private List<Sprite> cachedAvatarFrames;

private Coroutine avatarAnimRoutine;

private static GameStageScreen _instance;
        public static GameStageScreen Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<GameStageScreen>(FindObjectsInactive.Include);
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

public GameObject DockContainerNarrative => dockContainerNarrative;
        public GameObject DockContainerPDA => dockContainerPDA;
        public GameObject DockContainerGear => dockContainerGear;
        public GameObject AwakeningContainer => awakeningContainer;
        public GameObject HubMenuContainer => hubMenuContainer;
        public GameObject SituationBox => situationBox;
        public TextMeshProUGUI SituationBoxText => situationBoxText;
        public GameObject PackedSuccessBanner => packedSuccessBanner;
        public GameObject PdaPanelStatus => pdaPanelStatus;
        public GameObject PdaPanelSkills => pdaPanelSkills;
        public GameObject PdaPanelTraits => pdaPanelTraits;
        public GameObject PdaPanelMap => pdaPanelMap;
        public GameObject PdaPanelLore => pdaPanelLore;
        public TextMeshProUGUI LocationText => locationText;
        public TextMeshProUGUI ClockTimeText => clockTimeText;
        public TextMeshProUGUI ClockDayText => clockDayText;
        public GameObject PhoneModal => phoneModal;
        public GameObject StoryLogModal => storyLogModal;
        public GameObject RadioModal => radioModal;
        public GameObject HackingModal => hackingModal;
        public AudioSource AtmosphereSource => atmosphereSource;
        public AudioSource RadioSource => radioSource;
        public AudioClip PoliceRadioClip => policeRadioClip;
        public SundayLastBreath.Data.StoryPassageData HallwayStartingPassage => hallwayStartingPassage;
        public Coroutine FadeAtmosphereAudioRoutine(float targetVol, float duration) => StartCoroutine(FadeAtmosphereAudio(targetVol, duration));

        [SerializeField] private RadioScannerModal radioScannerModal;
        public RadioScannerModal RadioScanner
        {
            get
            {
                if (radioScannerModal == null)
                {
                    if (radioModal != null)
                        radioScannerModal = radioModal.GetComponent<RadioScannerModal>() ?? radioModal.AddComponent<RadioScannerModal>();
                    else
                        radioScannerModal = GetComponentInChildren<RadioScannerModal>(true) ?? gameObject.AddComponent<RadioScannerModal>();
                    radioScannerModal.Initialize(this);
                }
                return radioScannerModal;
            }
        }

        [SerializeField] private HackingTerminalModal hackingTerminalModal;
        public HackingTerminalModal HackingTerminal
        {
            get
            {
                if (hackingTerminalModal == null)
                {
                    if (hackingModal != null)
                        hackingTerminalModal = hackingModal.GetComponent<HackingTerminalModal>() ?? hackingModal.AddComponent<HackingTerminalModal>();
                    else
                        hackingTerminalModal = GetComponentInChildren<HackingTerminalModal>(true) ?? gameObject.AddComponent<HackingTerminalModal>();
                    hackingTerminalModal.Initialize(this);
                }
                return hackingTerminalModal;
            }
        }

        [SerializeField] private StageTransitionOverlay stageTransitionOverlay;
        public StageTransitionOverlay TransitionOverlay
        {
            get
            {
                if (stageTransitionOverlay == null)
                {
                    stageTransitionOverlay = GetComponentInChildren<StageTransitionOverlay>(true);
                    if (stageTransitionOverlay == null)
                    {
                        var holder = new GameObject("StageTransitionOverlay", typeof(RectTransform));
                        holder.transform.SetParent(transform, false);
                        stageTransitionOverlay = holder.AddComponent<StageTransitionOverlay>();
                    }
                    stageTransitionOverlay.Initialize(this);
                }
                return stageTransitionOverlay;
            }
        }
        public bool IsOnSecondFloor => !(groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby)
            && ((floor2Zone != null && floor2Zone.hallwaySection > 0 && currentInspectedRoom != null && (currentInspectedRoom.StartsWith("f2_") || currentInspectedRoom.StartsWith("unit_2") || currentInspectedRoom.StartsWith("20")))
            || (emergencyStairwellZone != null && emergencyStairwellZone.IsOnSecondFloor)
            || stair_phase >= 4);

private HybridCockpitController _hybridCockpit;
        public bool IsHybridCockpitActive => _hybridCockpit != null && _hybridCockpit.enabled;
        public IReadOnlyList<string> StoryLogs => _storyLogs;
        public List<InventoryEntry> GetCurrentFloorStashPublic()
        {
            return GetCurrentFloorStash();
        }
        public void TriggerAdvanceAwakening() => OnDialogueAreaClicked();
        public void TriggerOpenPhone() => OnCheckPhoneClicked();
        public void TriggerSwitchPDATab(string tabName) => SwitchPDATab(tabName, playSound: true);
        public void TriggerTakeAllFloorItems() => OnTakeAllFloorItemsClicked();
        public bool TakeSingleFloorOrTableItem(string itemId)
        {
            if (currentPlayer == null || string.IsNullOrEmpty(itemId)) return false;

if (!isInHallway && !hasPacked && tableStash != null)
            {
                int tIdx = tableStash.FindIndex(i => string.Equals(i.itemId, itemId, StringComparison.OrdinalIgnoreCase));
                if (tIdx >= 0)
                {
                    var entry = tableStash[tIdx];
                    bool picked;
                    if (string.Equals(entry.itemId, "Apartment Key", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(entry.itemId, "Apartment 407 Key", StringComparison.OrdinalIgnoreCase))
                    {
                        currentPlayer.AddKeyItem("Apartment Key", "Apartment 407 Key", "Brass tumbler key labeled 'Room 407'. Required once to unlock the front door deadbolt.", gameClock != null ? gameClock.GetTimeString() : "23:45");
                        picked = true;
                    }
                    else
                    {
                        picked = currentPlayer.AddItem(entry.itemId, entry.quantity);
                    }

if (picked)
                    {
                        tableStash.RemoveAt(tIdx);
                        AppendStoryLog(gameClock != null ? gameClock.GetTimeString() : "23:45", "LOOT // TABLE ITEM SECURED",
                            $"You packed {entry.itemId} (x{entry.quantity}) from the coffee table.");
                        if (tableStash.Count == 0)
                        {
                            hasPacked = true;
                            objectiveSystem?.CompleteObjective("pack_gear");
                            objectiveSystem?.ActivateObjective("leave_apt");
                        }
                        PopulateTableStashUI();
                        RefreshFromHybridCockpit();
                        return true;
                    }
                    else
                    {
                        if (situationBoxText != null)
                        {
                            UpdateSituationBoxTextAndHeight("<color=#EF4444>[!] Backpack capacity reached! Make room to carry this item.</color>");
                        }
                        return false;
                    }
                }
            }

OnTakeSingleFloorItemClicked(itemId);
            _hybridCockpit?.RefreshAll();
            return true;
        }
        public void TriggerWaitMinutes(int mins)
        {
            if (Clock != null) Clock.AddTime(mins);
            UpdateClockUI();
            UpdateVitalsUI();
            RefreshAllPDAPanels();
            _hybridCockpit?.RefreshAll();
        }
        public void RefreshFromHybridCockpit()
        {
            UpdateVitalsUI();
            UpdateClockUI();
            RefreshAllPDAPanels();
            RefreshRightGearPanel();
            UpdateFloorItemsNotification();
            EvaluateHubButtonStates();
            _hybridCockpit?.RefreshAll();
        }

protected override void Awake()

{
base.Awake();

Instance = this;

InitializeAudioSources();

InitializeSystems();

_hybridCockpit = GetComponent<HybridCockpitController>();
            if (_hybridCockpit != null)
            {
                _hybridCockpit.InitializeCockpit(this);
            }

if (unit407Zone == null)
            {
                unit407Zone = GetComponentInChildren<SundayLastBreath.Zones.Unit407ApartmentZone>(true);
                if (unit407Zone == null)
                {
                    unit407Zone = gameObject.AddComponent<SundayLastBreath.Zones.Unit407ApartmentZone>();
                }
            }

if (floor4Zone == null)
            {
                floor4Zone = GetComponentInChildren<SundayLastBreath.Zones.Floor4HallwayZone>(true);
                if (floor4Zone == null)
                {
                    floor4Zone = gameObject.AddComponent<SundayLastBreath.Zones.Floor4HallwayZone>();
                }
            }

if (emergencyStairwellZone == null)
            {
                emergencyStairwellZone = GetComponentInChildren<SundayLastBreath.Zones.EmergencyStairwellZone>(true);
                if (emergencyStairwellZone == null)
                {
                    emergencyStairwellZone = gameObject.AddComponent<SundayLastBreath.Zones.EmergencyStairwellZone>();
                }
            }

            if (floor2Zone == null)
            {
                floor2Zone = GetComponentInChildren<SundayLastBreath.Zones.Floor2HallwayZone>(true);
                if (floor2Zone == null)
                {
                    floor2Zone = gameObject.AddComponent<SundayLastBreath.Zones.Floor2HallwayZone>();
                }
            }

            if (groundFloorLobbyZone == null)
            {
                groundFloorLobbyZone = GetComponentInChildren<SundayLastBreath.Zones.GroundFloorLobbyZone>(true);
                if (groundFloorLobbyZone == null)
                {
                    groundFloorLobbyZone = gameObject.AddComponent<SundayLastBreath.Zones.GroundFloorLobbyZone>();
                }
            }

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

_hybridCockpit?.RefreshAll();
};
}

if (loreSystem == null)

{
loreSystem = new LoreSystem();

loreSystem.OnLoreAdded += (l) =>
                {
                    RefreshPDALoreTab();
                    _hybridCockpit?.RefreshAll();
                };
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

_hybridCockpit?.RefreshAll();
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

if (_hybridCockpit == null) _hybridCockpit = GetComponent<HybridCockpitController>();
            bool hybridActive = IsHybridCockpitActive;

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
vlgRoot.spacing = hybridActive ? 4f : 12f;

vlgRoot.childControlHeight = true;

vlgRoot.childForceExpandHeight = false;

vlgRoot.childControlWidth = true;

vlgRoot.childForceExpandWidth = true;
}

// Ensure SituationBox layout element stays compact (or full height in Hybrid Story Reader)

if (situationBox != null)

{
if (!hybridActive && situationBox.transform.parent != hubMenuContainer.transform)

{
situationBox.transform.SetParent(hubMenuContainer.transform, false);
}

LayoutElement sle = situationBox.GetComponent<LayoutElement>() ?? situationBox.AddComponent<LayoutElement>();

if (hybridActive)
                {
                    sle.minHeight = 250f;
                    if (sle.preferredHeight < 250f) sle.preferredHeight = 270f;
                }
                else
                {
                    if (sle.preferredHeight <= 0f) sle.preferredHeight = 64f;
                }

sle.flexibleHeight = 0f;

sle.flexibleWidth = 1f;
}

// Ensure PackedSuccessBanner stays compact

if (packedSuccessBanner != null)

{
Transform bannerTargetParent = (hybridActive && situationBox != null && situationBox.transform.parent != null)
                    ? situationBox.transform.parent
                    : hubMenuContainer.transform;

if (packedSuccessBanner.transform.parent != bannerTargetParent)

{
packedSuccessBanner.transform.SetParent(bannerTargetParent, false);
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

vlg.spacing = hybridActive ? 2f : 3f;

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
cvlg.spacing = hybridActive ? 2f : 3f;

cvlg.padding = new RectOffset(2, 6, 2, 2);

cvlg.childControlWidth = true;

cvlg.childControlHeight = true;

cvlg.childForceExpandWidth = true;

cvlg.childForceExpandHeight = false;
}
}

// 4. Ensure header order: SituationBox -> PackedSuccessBanner -> ChoiceSubHeader -> ScrollView

int sIdx = 0;

if (situationBox != null && situationBox.transform.parent == hubMenuContainer.transform) situationBox.transform.SetSiblingIndex(sIdx++);

if (packedSuccessBanner != null && packedSuccessBanner.transform.parent == hubMenuContainer.transform) packedSuccessBanner.transform.SetSiblingIndex(sIdx++);

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

if (hybridActive)
                {
                    sh.gameObject.SetActive(false);
                }
}

if (sr != null) sr.transform.SetSiblingIndex(sIdx++);

_hybridCockpit?.OnHubMenuSanitized();

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

if (Unit407Zone != null)
            {
                SetApartmentButtonsVisible(false);
            }
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

bool hybridActive = IsHybridCockpitActive;
            float rowH = hybridActive ? 31f : 34f;
            float rowSpacing = hybridActive ? 2f : 3f;

// Ensure parent VerticalLayoutGroup drives heights and uses compact spacing

var cvlg = contentParent.GetComponent<VerticalLayoutGroup>();

if (cvlg != null)

{
cvlg.spacing = rowSpacing;

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

// 1. Force compact height (31px in 1+3 Hybrid deck so 8 choices fit without clipping; 34px legacy)

RectTransform crt = child.GetComponent<RectTransform>();

if (crt != null)

{
crt.sizeDelta = new Vector2(crt.sizeDelta.x, rowH);
}

LayoutElement le = child.GetComponent<LayoutElement>();

if (le == null && crt != null) le = child.gameObject.AddComponent<LayoutElement>();

if (le != null)

{
le.preferredHeight = rowH;

le.minHeight = rowH;

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

if (Unit407Zone != null)
            {
                Unit407Zone.ResetZoneState();
            }

if (Floor4Zone != null)
            {
                Floor4Zone.ResetZoneState();
            }

if (EmergencyStairwellZone != null)
            {
                EmergencyStairwellZone.ResetZoneState();
            }

            if (Floor2Zone != null)
            {
                Floor2Zone.ResetZoneState();
            }

            if (GroundFloorLobbyZone != null)
            {
                GroundFloorLobbyZone.ResetZoneState();
            }

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
            awakeningStep = 1;
            introSeen = false;

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

SetSituationNarration(GetUnit407Anchor(), recordInLog: true);

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

tacticalMapController?.ResetAllRuntimeMapStates();
            SetLocation(startLoc);

// Load player avatar frames
            cachedAvatarFrames = CharacterCreationScreen.LoadPlayerFrames();

// Always begin at Awakening Beat 1 (Civil Danger Warning / EAS Alert)
            ShowAwakeningStep(1);

if (_hybridCockpit == null) _hybridCockpit = GetComponent<HybridCockpitController>();
            if (_hybridCockpit != null)
            {
                _hybridCockpit.ClearSelection();
                _hybridCockpit.EnsureInteractionsHooked();
                _hybridCockpit.RefreshAll();
            }
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

// Case A: If loaded with hasPacked == true but backpack is empty, auto-restore starting gear directly into inventory!
            if (hasPacked && (currentPlayer.inventory == null || currentPlayer.inventory.Count == 0))
            {
                if (currentPlayer.inventory == null) currentPlayer.inventory = new List<InventoryEntry>();

if (!currentPlayer.HasKeyItem("Apartment Key", true) && !currentPlayer.HasItem("Apartment Key") &&
                    !currentPlayer.HasKeyItem("Apartment 407 Key", true) && !currentPlayer.HasItem("Apartment 407 Key"))
                {
                    currentPlayer.AddKeyItem("Apartment Key", "Apartment 407 Key", "Brass tumbler key labeled 'Room 407'. Required once to unlock the front door deadbolt.", gameClock != null ? gameClock.GetTimeString() : "23:45");
                }

var jobDef = System.Array.Find(CharacterCreationDefinitions.Jobs, j => j.title.Equals(currentPlayer.job, StringComparison.OrdinalIgnoreCase) || j.id.Equals(currentPlayer.job, StringComparison.OrdinalIgnoreCase));
                if (jobDef != null && jobDef.startingItems != null)
                {
                    foreach (var item in jobDef.startingItems)
                    {
                        if (!currentPlayer.HasItem(item.item))
                        {
                            currentPlayer.AddItem(item.item, item.qty);
                        }
                    }
                }

if (currentPlayer.traits != null)
                {
                    foreach (var t in currentPlayer.traits)
                    {
                        if (string.Equals(t.name, "ARCHER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Compound Bow")) currentPlayer.AddItem("Compound Bow", 1);
                            if (!currentPlayer.HasItem("Arrows")) currentPlayer.AddItem("Arrows", 5);
                        }
                        else if (string.Equals(t.name, "HIKER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Hiking Boots")) currentPlayer.AddItem("Hiking Boots", 1);
                        }
                        else if (string.Equals(t.name, "RUNNER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Energy Bar")) currentPlayer.AddItem("Energy Bar", 2);
                            if (!currentPlayer.HasItem("Running Shoes")) currentPlayer.AddItem("Running Shoes", 1);
                        }
                        else if (string.Equals(t.name, "HUNTER", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Binoculars")) currentPlayer.AddItem("Binoculars", 1);
                            if (!currentPlayer.HasItem("Combat Knife")) currentPlayer.AddItem("Combat Knife", 1);
                        }
                        else if (string.Equals(t.name, "BASEBALL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Baseball Bat")) currentPlayer.AddItem("Baseball Bat", 1);
                        }
                        else if (string.Equals(t.name, "WATCHMAN", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Flashlight")) currentPlayer.AddItem("Flashlight", 1);
                        }
                        else if (string.Equals(t.name, "MECHANIC", StringComparison.OrdinalIgnoreCase) || string.Equals(t.name, "SURVIVALIST", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!currentPlayer.HasItem("Multi-Tool")) currentPlayer.AddItem("Multi-Tool", 1);
                        }
                    }
                }

string[] defaults = { "Flashlight", "Bandage", "Multi-Tool" };
                foreach (var d in defaults)
                {
                    if (!currentPlayer.HasItem(d)) currentPlayer.AddItem(d, 1);
                }
            }

// Case B: Safety recovery: If loading at Beat 1/Day 0 and both backpack and tableStash are empty (e.g. from an older save),
            // auto-restore starting items for the player's job AND all traits on the table so they are never left empty-handed!
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

if (isInHallway || (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby))

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

if (_hybridCockpit == null) _hybridCockpit = GetComponent<HybridCockpitController>();
            if (_hybridCockpit != null)
            {
                _hybridCockpit.ClearSelection();
                _hybridCockpit.EnsureInteractionsHooked();
                _hybridCockpit.RefreshAll();
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

if (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby)
{
PlayHubBGM("Audio/BGM/lobby_slaughter", 0.6f, true);
return;
}

if (IsOnSecondFloor)
{
PlayHubBGM("Audio/SFX/fluorescent_buzz", 0.3f, true);
return;
}

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

_hybridCockpit?.OnConsoleModeChanged(mode);
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

if (SystemMenu != null && SystemMenu.gameObject.activeSelf)
{
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

else if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
{
    if (SystemMenu != null && SystemMenu.gameObject.activeSelf)
    {
        CloseSystemMenuModal();
    }
    else if (currentConsoleMode != ConsoleMode.Narrative)
    {
        SwitchConsoleMode(ConsoleMode.Narrative);
    }
    else
    {
        OpenSystemMenuModal();
    }
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

else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F1))
                    {
                        if (SystemMenu != null && SystemMenu.gameObject.activeSelf)
                        {
                            CloseSystemMenuModal();
                        }
                        else if (currentConsoleMode != ConsoleMode.Narrative)
                        {
                            SwitchConsoleMode(ConsoleMode.Narrative);
                        }
                        else
                        {
                            OpenSystemMenuModal();
                        }
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

// =================================================================================================
        // SECTION 2: SURVIVOR PDA & EMBEDDED COCKPIT TELEMETRY
        // Description: Real-time UI updating for survivor status: M.O.R.T.A.L. attribute bars,
        //              skills, perks/traits, humanity heartbeat pulse, and inventory slots.
        // =================================================================================================
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

bool isLobbyOr2F = (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby) || IsOnSecondFloor;

if (!isLobbyOr2F && !string.IsNullOrEmpty(currentInspectedRoom))

{
string inspectId = currentInspectedRoom.StartsWith("unit_") ? currentInspectedRoom : (currentInspectedRoom == "utility" || currentInspectedRoom == "stairs" ? currentInspectedRoom : "unit_" + currentInspectedRoom);

tacticalMapController.SetThresholdLocation(targetLoc, inspectId);
}

else

{
tacticalMapController.SetCurrentLocation(targetLoc);
SyncTacticalMapRoomStates();
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

public void RefreshAllPDAPanels()
        {
            RefreshPDAStatusTab();
            RefreshPDASkillsTab();
            RefreshPDATraitsTab();
            RefreshPDALoreTab();
            RefreshRightGearPanel();
            _hybridCockpit?.RefreshAll();
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

_hybridCockpit?.RefreshAll();
}

private void RefreshRightGearPanel()

{
UpdateFloorItemsNotification();

var activeFloor = GetCurrentFloorStash();

if (tacticalGearController != null)

{
tacticalGearController.Initialize(currentPlayer, activeFloor);

_hybridCockpit?.RefreshAll();

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

_hybridCockpit?.RefreshAll();
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

// =================================================================================================
        // SECTION 3: AWAKENING PROLOGUE SEQUENCE (BEATS 1 - 3)
        // Description: The opening TV broadcast sequence (Civil Danger Warning, Seattle evacuation alert),
        //              audio siren hum, typewriter effect, and Neal Sage intro monologue.
        // =================================================================================================
        #region Awakening Sequence (Beats 1 - 3)

private void ShowAwakeningStep(int step)

{
awakeningStep = Mathf.Clamp(step, 1, 3);

introSeen = false;

EnsureAwakeningBeatLogged(awakeningStep);

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

_hybridCockpit?.RefreshAll();
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

// =================================================================================================
        // SECTION 4: APARTMENT HUB MODE & CHOICE HOTKEY ENGINE
        // Description: Choice Deck hotkey listeners [1]-[9], pagination indexers, and local floor stash
        //              helpers that allow the player to interact with the active screenplay.
        // =================================================================================================
        #region Hub Menu Mode

private void ShowHubMenu(bool updateSituation = true)
        {
            introSeen = true;
            awakeningStep = 4;

if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);

if (Unit407Zone != null)
            {
                Unit407Zone.EnterZone(this);
                return;
            }

if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";

if (dockHeaderChannel != null) dockHeaderChannel.text = hasPacked ? "<color=#10B981>DOOR READY TO UNLOCK</color>" : "<color=#EF4444>FRONT DOOR LOCKED (NEED KEYS)</color>";

UpdateSituationBoxHeader("LIVING ROOM");
            if (updateSituation)
            {
                SetSituationNarration(GetUnit407Anchor(), recordInLog: true);
            }

if (!isInHallway)

{
PlayStageAnimation("Unit407Inside_Anim", "unit407", 8f);
}

EvaluateHubButtonStates();

_hybridCockpit?.RefreshAll();
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

if (isInHallway || Unit407Zone != null)

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
if (isInHallway || (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby)) return;

if (Unit407Zone != null)
            {
                SetApartmentButtonsVisible(false);
                ReindexActiveChoiceHotkeys();
                return;
            }

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

public bool GiveOrDropItem(string itemId, int quantity)

{
if (string.IsNullOrEmpty(itemId) || quantity <= 0 || currentPlayer == null) return false;

if (!currentPlayer.AddItem(itemId, quantity))

{
AddFloorItem(itemId, quantity);

UpdateFloorItemsNotification();

lastOverflowNotice += $"\n<color=#F59E0B>[BACKPACK FULL] {itemId} (x{quantity}) placed in room container. Press [I] to manage items.</color>";

return false;
}

_hybridCockpit?.RefreshAll();

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

if (currentPlayer == null) return;

if (!isInHallway && !hasPacked && tableStash != null && tableStash.Count > 0)
            {
                OnPackGearClicked();
            }

var currentStash = GetCurrentFloorStash();

if (currentStash.Count == 0)
            {
                _hybridCockpit?.RefreshAll();
                return;
            }

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

int idx = currentStash.FindIndex(i => string.Equals(i.itemId, itemId, StringComparison.OrdinalIgnoreCase));

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

// =================================================================================================
        // =================================================================================================
        // SECTION 5: SYSTEM MENU MODAL FORWARDERS (ES3 SAVE / LOAD / AUDIO / EXIT)
        // Description: Delegated to the dedicated SystemMenuModal.cs component.
        // =================================================================================================
        #region System Menu Modal Forwarders

        [SerializeField] private SystemMenuModal systemMenuModalComponent;
        public SystemMenuModal SystemMenu
        {
            get
            {
                if (systemMenuModalComponent == null)
                    systemMenuModalComponent = GetComponentInChildren<SystemMenuModal>(true);
                if (systemMenuModalComponent == null)
                {
                    var go = new GameObject("SystemMenuModal", typeof(RectTransform));
                    go.transform.SetParent(transform, false);
                    systemMenuModalComponent = go.AddComponent<SystemMenuModal>();
                    systemMenuModalComponent.Initialize(this);
                    go.SetActive(false);
                }
                return systemMenuModalComponent;
            }
        }

        public GameObject SystemMenuModal => SystemMenu != null ? SystemMenu.gameObject : null;

        private void OnPDASaveClicked()
        {
            PlayClickSFX();
            OpenSystemMenuModal();
        }

        public void OpenSystemMenuModal() => SystemMenu?.Open();
        public void CloseSystemMenuModal() => SystemMenu?.Close();

        #endregion

        #region Modular Modals: Radio, Hacking, Door Transitions

        // =================================================================================================
        // SECTION 5B: RADIO SCANNER MODAL (STANDALONE COMPONENT DELEGATE)
        // Handled via RadioScannerModal.cs
        // =================================================================================================
        public void OnListenRadioClicked() => RadioScanner.Open();
        public void OnCloseRadioClicked() => RadioScanner.Close();
        public void FastForwardRadio() => RadioScanner.FastForwardRadio();

        // =================================================================================================
        // SECTION 5C: HACKING TERMINAL MODAL (STANDALONE COMPONENT DELEGATE)
        // Handled via HackingTerminalModal.cs
        // =================================================================================================
        public void OnHackTerminalClicked() => HackingTerminal.Open();
        public void OnCloseHackingClicked() => HackingTerminal.Close();
        public void FastForwardHacking() => HackingTerminal.FastForwardHacking();

        // =================================================================================================
        // SECTION 5D: APARTMENT EXIT & STAGE DOOR TRANSITIONS (STANDALONE OVERLAY DELEGATE)
        // Handled via StageTransitionOverlay.cs
        // =================================================================================================
        public void OnLeaveApartmentClicked()
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
            if (currentPlayer != null)
            {
                currentPlayer.MarkKeyItemUsed("Apartment Key", "Unit 407 Front Deadbolt (The Ascott Residency [4th Floor])");
                currentPlayer.MarkKeyItemUsed("Apartment 407 Key", "Unit 407 Front Deadbolt (The Ascott Residency [4th Floor])");
                currentPlayer.RemoveItem("Apartment Key", 1);
                currentPlayer.RemoveItem("Apartment 407 Key", 1);
            }

            AppendStoryLog(gameClock.GetTimeString(), "HALLWAY // LEFT UNIT 407",
                "I turned the brass key in the deadbolt, unlocked the door, and stepped out into the 4th floor hallway. The door clicked shut behind me, locked tight.");

            objectiveSystem.CompleteObjective("leave_apt");
            objectiveSystem.AddObjective("escape_building", "Find a way down to the ground floor", ObjectiveStatus.Active);

            TransitionOverlay.PlayApartmentExitTransition();
        }

        public bool isDoorTransitioning => TransitionOverlay != null && TransitionOverlay.IsTransitioning;
        public GameObject doorTransitionOverlay => TransitionOverlay != null ? TransitionOverlay.OverlayObject : null;

        public void PlayDoorTransition(string videoResourcePath, string roomTitle, string narrativeSubtitle, bool playKeySFX, System.Action onReachedDestination, bool playSlam = false)
        {
            TransitionOverlay.PlayDoorTransition(videoResourcePath, roomTitle, narrativeSubtitle, playKeySFX, onReachedDestination, playSlam);
        }

        #endregion

// =================================================================================================
        // SECTION 6: GLOBAL WORLD STATE COORDINATOR (SAVE / LOAD / RESET DISPATCHER)
        // Description: Dispatches SaveZoneState(), LoadZoneState(), and ResetZoneState() across
        //              all active screenplay components (Unit 407, Floor 4, Stairwell) via Easy Save 3.
        // =================================================================================================
        #region Hallway & Exploration Implementation

public void RestoreHallwayExploration()
        {
            isInHallway = true;

if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
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

if (GroundFloorLobbyZone != null && GroundFloorLobbyZone.isInLobby)
            {
                GroundFloorLobbyZone.EnterZone(this);
                return;
            }

if (IsOnSecondFloor && Floor2Zone != null)
            {
                Floor2Zone.EnterZone(this, Floor2Zone.hallwaySection, Floor2Zone.zombie_alerted);
                return;
            }

if (Floor4Zone != null)
            {
                Floor4Zone.EnterZone(this, hallwaySection, currentInspectedRoom);
                return;
            }

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
if (EmergencyStairwellZone != null)
                {
                    EmergencyStairwellZone.EnterZone(this, EmergencyStairwellZone.stair_phase);
                }
                else
                {
                    InspectStairwell();
                }
}

else

{
EnterHallwayMode(hallwaySection);
}
}

public void SaveWorldState(string file)
        {
            if (string.IsNullOrEmpty(file)) return;

if (Floor4Zone != null)
            {
                Floor4Zone.SaveZoneState(file);
            }

if (EmergencyStairwellZone != null)
            {
                EmergencyStairwellZone.SaveZoneState(file);
            }

            if (Floor2Zone != null)
            {
                Floor2Zone.SaveZoneState(file);
            }

            if (GroundFloorLobbyZone != null)
            {
                GroundFloorLobbyZone.SaveZoneState(file);
            }

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

if (Floor4Zone != null)
            {
                Floor4Zone.LoadZoneState(file);
            }

if (EmergencyStairwellZone != null)
            {
                EmergencyStairwellZone.LoadZoneState(file);
            }

            if (Floor2Zone != null)
            {
                Floor2Zone.LoadZoneState(file);
            }

            if (GroundFloorLobbyZone != null)
            {
                GroundFloorLobbyZone.LoadZoneState(file);
            }

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

SyncTacticalMapRoomStates();
}

public void SyncTacticalMapRoomStates()
        {
            if (tacticalMapController == null) return;

tacticalMapController.SyncRoomState("unit_407", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);

bool is406Open = (floor4Zone != null && floor4Zone.room406_opened) || room406_opened;
            string curInsp = floor4Zone != null && !string.IsNullOrEmpty(floor4Zone.currentInspectedRoom) ? floor4Zone.currentInspectedRoom : currentInspectedRoom;
            bool c406 = is406Open || _visitedLocationAnchors.Contains("ext_406") || _visitedLocationAnchors.Contains("int_406") || curInsp == "406";
            tacticalMapController.SyncRoomState("unit_406", isVisited: is406Open, isChecked: c406, isLocked: !is406Open, isBlocked: false, isAccessible: true);

bool is405Open = (floor4Zone != null && floor4Zone.room405_opened) || room405_opened;
            bool c405 = is405Open || _visitedLocationAnchors.Contains("ext_405") || _visitedLocationAnchors.Contains("int_405") || curInsp == "405";
            tacticalMapController.SyncRoomState("unit_405", isVisited: is405Open, isChecked: c405, isLocked: !is405Open, isBlocked: false, isAccessible: true);

bool spokeGable = (floor4Zone != null && floor4Zone.spoke_to_mrs_gable) || spoke_to_mrs_gable;
            bool c404 = spokeGable || _visitedLocationAnchors.Contains("ext_404") || curInsp == "404";
            tacticalMapController.SyncRoomState("unit_404", isVisited: false, isChecked: c404, isLocked: false, isBlocked: true, isAccessible: false);

bool c403 = _visitedLocationAnchors.Contains("ext_403") || curInsp == "403";
            tacticalMapController.SyncRoomState("unit_403", isVisited: false, isChecked: c403, isLocked: false, isBlocked: true, isAccessible: false);

bool is402Open = (floor4Zone != null && floor4Zone.room402_opened) || room402_opened;
            bool c402 = is402Open || _visitedLocationAnchors.Contains("ext_402") || _visitedLocationAnchors.Contains("int_402") || curInsp == "402";
            tacticalMapController.SyncRoomState("unit_402", isVisited: is402Open, isChecked: c402, isLocked: !is402Open, isBlocked: false, isAccessible: true);

bool is401Entered = (floor4Zone != null && floor4Zone.room401_entered) || room401_entered;
            bool c401 = is401Entered || _visitedLocationAnchors.Contains("ext_401") || _visitedLocationAnchors.Contains("int_401") || curInsp == "401";
            tacticalMapController.SyncRoomState("unit_401", isVisited: c401, isChecked: c401, isLocked: false, isBlocked: false, isAccessible: true);

bool isUtilOpen = (floor4Zone != null && floor4Zone.utility_opened) || utility_opened;
            bool cUtil = isUtilOpen || _visitedLocationAnchors.Contains("ext_utility") || _visitedLocationAnchors.Contains("int_utility") || curInsp == "utility";
            tacticalMapController.SyncRoomState("utility", isVisited: isUtilOpen, isChecked: cUtil, isLocked: !isUtilOpen, isBlocked: false, isAccessible: true);

            if (floor2Zone != null)
            {
                tacticalMapController.SyncRoomState("f2_hall_1", isVisited: floor2Zone.hallwaySection >= 1, isChecked: floor2Zone.hallwaySection >= 1, isLocked: false, isBlocked: false, isAccessible: true);
                tacticalMapController.SyncRoomState("f2_hall_2", isVisited: floor2Zone.hallwaySection >= 2, isChecked: floor2Zone.zombie_defeated, isLocked: false, isBlocked: false, isAccessible: true);
                tacticalMapController.SyncRoomState("f2_hall_3", isVisited: floor2Zone.hallwaySection >= 3, isChecked: floor2Zone.room203_looted, isLocked: false, isBlocked: false, isAccessible: true);
                tacticalMapController.SyncRoomState("f2_hall_4", isVisited: floor2Zone.hallwaySection >= 4, isChecked: floor2Zone.exit_stairs_inspected, isLocked: false, isBlocked: false, isAccessible: true);
            }

            if (groundFloorLobbyZone != null && groundFloorLobbyZone.isInLobby)
            {
                tacticalMapController.SyncRoomState("nw_room", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);
                tacticalMapController.SyncRoomState("n_room", isVisited: groundFloorLobbyZone.pack_resolved, isChecked: groundFloorLobbyZone.pack_resolved, isLocked: false, isBlocked: false, isAccessible: true);
                tacticalMapController.SyncRoomState("ne_room", isVisited: groundFloorLobbyZone.maya_resolved, isChecked: groundFloorLobbyZone.maya_resolved, isLocked: false, isBlocked: groundFloorLobbyZone.maya_status == "dead", isAccessible: groundFloorLobbyZone.maya_status != "dead");
                tacticalMapController.SyncRoomState("e_room", isVisited: groundFloorLobbyZone.vane_resolved, isChecked: groundFloorLobbyZone.vane_resolved, isLocked: false, isBlocked: groundFloorLobbyZone.vane_status == "dead", isAccessible: groundFloorLobbyZone.vane_status != "dead");
                tacticalMapController.SyncRoomState("c_room", isVisited: groundFloorLobbyZone.triageActionsTaken >= 1, isChecked: groundFloorLobbyZone.triageActionsTaken >= 2, isLocked: false, isBlocked: false, isAccessible: true);
                bool exitUnlocked = groundFloorLobbyZone.lobbyPhase == "barricade_unlocked" || groundFloorLobbyZone.lobbyPhase == "act2_exit";
                tacticalMapController.SyncRoomState("lobby_exit", isVisited: groundFloorLobbyZone.lobbyPhase.StartsWith("barricade") || exitUnlocked, isChecked: exitUnlocked, isLocked: !exitUnlocked, isBlocked: false, isAccessible: true);
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

if (Floor4Zone != null)
            {
                Floor4Zone.EnterZone(this, section);
                return;
            }

tacticalMapController?.LoadFloorById(IsOnSecondFloor ? "ascott_2f" : "ascott_4f");

if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionFromApartmentToHallway();
            }

UpdateHallwayView();
        }

public void SetApartmentButtonsVisible(bool visible)

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

public void ClearHallwayChoices()

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

public TacticalChoiceButton SetHallwayChoice(int index, string mainText, System.Action onClick, bool interactable = true, Color? accent = null)

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

// =================================================================================================
        // SECTION 7: PUBLIC STAGE & ZONE ENGINE API
        // Description: The primary API called by Screenplay Actors to control the Stage:
        //              PlayStageAnimation(), SetChoice(), ClearChoices(), SyncFloorStash(), and ReindexChoices().
        // =================================================================================================
        #region Public Stage & Zone API

public void EndAwakeningIntro()
        {
            introSeen = true;
            awakeningStep = 4;
            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);
        }

public void ClearChoices()
        {
            EndAwakeningIntro();
            SetApartmentButtonsVisible(false);
            ClearHallwayChoices();
            activeChoiceButtons.Clear();
        }

public TacticalChoiceButton SetChoice(int index, string mainText, System.Action onClick, bool interactable = true, Color? accent = null)
        {
            return SetHallwayChoice(index, mainText, onClick, interactable, accent);
        }

public void ReindexChoices()
        {
            ReindexActiveChoiceHotkeys();
        }

public void TriggerChoice(int index)
        {
            if (index >= 0 && index < activeChoiceButtons.Count && activeChoiceButtons[index] != null)
            {
                activeChoiceButtons[index].TriggerClick();
            }
        }

public void SyncFloorStash(string roomId, List<InventoryEntry> stash)
        {
            if (string.IsNullOrEmpty(roomId)) roomId = "unit_407";
            roomFloorStashes[roomId] = stash;
            floorItems = stash;
            UpdateFloorItemsNotification();
        }

public bool IsInHallwayInternal
        {
            get => isInHallway;
            set => isInHallway = value;
        }

public int HallwaySectionInternal
        {
            get => hallwaySection;
            set => hallwaySection = value;
        }

public string CurrentInspectedRoomInternal
        {
            get => currentInspectedRoom;
            set => currentInspectedRoom = value;
        }

public void OpenKeypadModal(System.Action onUnlocked, System.Action onLockedOut = null, System.Action onCancelled = null)
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
                    onUnlocked?.Invoke();
                },
                onLockedOut: () =>
                {
                    onLockedOut?.Invoke();
                },
                onCancelled: () =>
                {
                    PlayClickSFX();
                    onCancelled?.Invoke();
                },
                hasObservationClue: (currentPlayer != null && currentPlayer.per >= 6)
            );
        }

#endregion

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

string folderPath = "Assets/Resources/Act1/" + animFolderName;

#if UNITY_EDITOR

if (frames == null || frames.Count == 0)

{
if (UnityEditor.AssetDatabase.IsValidFolder(folderPath))

{
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
}

#endif

if ((frames == null || frames.Count == 0) && System.IO.Directory.Exists(folderPath))

{
string[] files = System.IO.Directory.GetFiles(folderPath, "*.png");

if (files != null && files.Length > 0)

{
System.Array.Sort(files, System.StringComparer.Ordinal);

frames = new List<Sprite>(files.Length);

foreach (string file in files)

{
Sprite s = LoadSpriteFromDiskFile(file);

if (s != null) frames.Add(s);
}
}
}

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

// =================================================================================================
        // SECTION 8: SCREENPLAY FORWARDERS (FLOOR 4 & STAIRWELL)
        // Description: Public API bridge that forwards exterior door and room actions directly to
        //              the active Screenplay Actor (Floor4HallwayZone and EmergencyStairwellZone).
        // =================================================================================================
        #region Hallway & Exploration Screenplay Forwarders (Floor 4 & Stairwell)

public void UpdateHallwayView() => Floor4Zone?.UpdateHallwayView();
        public void AdvanceDownHall() => Floor4Zone?.AdvanceDownHall();
        public void RetreatUpHall() => Floor4Zone?.RetreatUpHall();
        public void InspectUnit407Exterior() => Floor4Zone?.InspectUnit407Exterior();
        public void OpenUnit406Door() => Floor4Zone?.OpenUnit406Door();
        public void InspectUnit406(string actionNarration = null) => Floor4Zone?.InspectUnit406(actionNarration);
        public void OpenUnit405Door(bool triggered = false) => Floor4Zone?.OpenUnit405Door(triggered);
        public void InspectUnit405(string actionNarration = null) => Floor4Zone?.InspectUnit405(actionNarration);
        public void InspectUnit404(string actionNarration = null) => Floor4Zone?.InspectUnit404(actionNarration);
        public void InspectUnit403() => Floor4Zone?.InspectUnit403();
        public void OpenUnit402Door(string method) => Floor4Zone?.OpenUnit402Door(method);
        public void InspectUnit402(string actionNarration = null) => Floor4Zone?.InspectUnit402(actionNarration);
        public void InspectUnit401(string actionNarration = null) => Floor4Zone?.InspectUnit401(actionNarration);
        public void OpenUtilityDoor(string bypassAction = null) => Floor4Zone?.OpenUtilityDoor(bypassAction);
        public void InspectUtilityCloset(string actionNarration = null) => Floor4Zone?.InspectUtilityCloset(actionNarration);
        public void InspectStairwell(string actionFeedback = null) => EmergencyStairwellZone?.InspectStairwell(actionFeedback);
        public void EnterFloor2Hallway(int section = 1, bool alertState = false) => Floor2Zone?.EnterZone(this, section, alertState);
        public void InspectFloor2Hallway(string actionFeedback = null) => Floor2Zone?.InspectHallway(actionFeedback);

#endregion

public void ExertStamina(int amount)

{
if (currentPlayer != null)

{
currentPlayer.stamina = Mathf.Max(0, currentPlayer.stamina - amount);

UpdateVitalsUI();

RefreshAllPDAPanels();
}
}

public void TakeDamage(int amount, string source)

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

sp = LoadSpriteFromDiskFile("Assets/Resources/Act1/" + spriteName + ".png");

if (sp != null) return sp;

return null;
}

/// <summary>
        /// Loads a PNG sprite directly from disk as a fallback when AssetDatabase or Resources have not imported the file yet.
        /// </summary>
        private static Sprite LoadSpriteFromDiskFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;

#if UNITY_EDITOR
            string assetPath = filePath.Replace("\\", "/");
            Sprite assetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (assetSprite != null) return assetSprite;
#endif

if (!System.IO.File.Exists(filePath)) return null;

try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                if (bytes == null || bytes.Length == 0) return null;

Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(tex);
                    return null;
                }

tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s.name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                return s;
            }
            catch (Exception)
            {
                return null;
            }
        }

private readonly Dictionary<string, AudioClip> _runtimeAudioCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
private Coroutine _bgmDiskLoadCoroutine;
private string _pendingBgmPath;

private AudioClip ResolveAudioClipImmediate(string relPath)
{
    if (string.IsNullOrEmpty(relPath)) return null;
    if (_runtimeAudioCache.TryGetValue(relPath, out var cached) && cached != null) return cached;

    AudioClip clip = Resources.Load<AudioClip>(relPath);

    if (clip == null)
    {
#if UNITY_EDITOR
        string cleanPath = relPath.Replace("\\", "/");
        if (!cleanPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) cleanPath = "Assets/" + cleanPath;
        if (!cleanPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
            && !cleanPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
            && !cleanPath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            cleanPath += ".mp3";
        }
        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(cleanPath);
#endif
    }

    if (clip != null) _runtimeAudioCache[relPath] = clip;
    return clip;
}

private string ResolveDiskAudioPath(string relPath)
{
    string normalized = relPath.Replace("\\", "/").TrimStart('/');
    if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
    {
        normalized = normalized.Substring("Assets/".Length);
    }

    string[] exts = normalized.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                 || normalized.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                 || normalized.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
        ? new[] { "" }
        : new[] { ".mp3", ".wav", ".ogg" };

    foreach (string ext in exts)
    {
        string direct = System.IO.Path.Combine(Application.dataPath, normalized + ext);
        if (System.IO.File.Exists(direct)) return direct;

        string res = System.IO.Path.Combine(Application.dataPath, "Resources/" + normalized + ext);
        if (System.IO.File.Exists(res)) return res;
    }

    return null;
}

private IEnumerator LoadAudioClipFromDiskRoutine(string relPath, Action<AudioClip> onLoaded)
{
    string diskPath = ResolveDiskAudioPath(relPath);
    if (string.IsNullOrEmpty(diskPath)) yield break;

    AudioType audioType = AudioType.MPEG;
    if (diskPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) audioType = AudioType.WAV;
    else if (diskPath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) audioType = AudioType.OGGVORBIS;

    string uri = new Uri(diskPath).AbsoluteUri;
    using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
    {
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            AudioClip loaded = DownloadHandlerAudioClip.GetContent(req);
            if (loaded != null)
            {
                loaded.name = System.IO.Path.GetFileNameWithoutExtension(diskPath);
                _runtimeAudioCache[relPath] = loaded;
                onLoaded?.Invoke(loaded);
            }
        }
    }
}

public void PlayHubBGM(string relPath, float volume = 0.5f, bool loop = true)

{
if (string.IsNullOrEmpty(relPath)) return;

_pendingBgmPath = relPath;
if (_bgmDiskLoadCoroutine != null)
{
    StopCoroutine(_bgmDiskLoadCoroutine);
    _bgmDiskLoadCoroutine = null;
}

AudioClip clip = ResolveAudioClipImmediate(relPath);
if (clip != null)
{
    PlayHubBGM(clip, volume, loop);
    return;
}

string requestedPath = relPath;
_bgmDiskLoadCoroutine = StartCoroutine(LoadAudioClipFromDiskRoutine(requestedPath, loaded =>
{
    _bgmDiskLoadCoroutine = null;
    if (string.Equals(_pendingBgmPath, requestedPath, StringComparison.OrdinalIgnoreCase))
    {
        PlayHubBGM(loaded, volume, loop);
    }
}));
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

public void StopHubBGM()

{
_pendingBgmPath = null;
if (_bgmDiskLoadCoroutine != null)
{
    StopCoroutine(_bgmDiskLoadCoroutine);
    _bgmDiskLoadCoroutine = null;
}
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

public void PlaySFX(string relPath, float volume = 0.8f)

{
if (string.IsNullOrEmpty(relPath)) return;

AudioClip clip = ResolveAudioClipImmediate(relPath);
if (clip != null)
{
    AudioManager.Instance?.PlaySFX(clip, volume);
    return;
}

StartCoroutine(LoadAudioClipFromDiskRoutine(relPath, loaded =>
{
    AudioManager.Instance?.PlaySFX(loaded, volume);
}));
}

// =================================================================================================
        // SECTION 9: MODULAR STORY PASSAGE RUNNER & PUZZLE MINIGAMES
        // Description: ScriptableObject story passage renderer, keypad puzzle modal, lockpicking modal,
        //              quick-time events (QTE), and tactical combat integration.
        // =================================================================================================
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

if (IsHybridCockpitActive)
                {
                    situationBoxText.fontSize = 14f;
                    situationBoxText.lineSpacing = 4f;
                }

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

float targetH = IsHybridCockpitActive
                    ? Mathf.Max(250f, preferredTextH + topPadding + bottomPadding + 14f)
                    : Mathf.Clamp(preferredTextH + topPadding + bottomPadding + extraBreathingRoom, 64f, 150f);

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

if (situationBox.transform.parent != null)
                {
                    RectTransform sitParentRt = situationBox.transform.parent.GetComponent<RectTransform>();
                    if (sitParentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(sitParentRt);
                }

if (hubMenuContainer != null)

{
LayoutRebuilder.ForceRebuildLayoutImmediate(hubMenuContainer.GetComponent<RectTransform>());

if (hubMenuContainer.transform.parent != null)

{
RectTransform parentRt = hubMenuContainer.transform.parent.GetComponent<RectTransform>();

if (parentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
}
}

_hybridCockpit?.RefreshAll();
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

public void ShowLocationAnchor(string anchorKey, string firstVisitText, params string[] returnVariants)
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

Transform targetDialogueParent = (IsHybridCockpitActive && situationBox != null && situationBox.transform.parent != null)
                ? situationBox.transform.parent
                : hubMenuContainer.transform;

modularDialogueBox = Instantiate(monologueBox, targetDialogueParent, false);

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
            yield return TransitionOverlay.PlayPassageVideoTransition(choice, targetPassage);
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

// =================================================================================================
        // SECTION 10: DYNAMIC LOCATION & TACTICAL MAP COORDINATOR
        // Description: Dynamically coordinates tactical SVG/texture floor maps (4F, 2F, Stairwells),
        //              room visibility states, and location header typography.
        // =================================================================================================
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

_hybridCockpit?.RefreshAll();
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

// =================================================================================================
        // SECTION 11: UI DISPLAY HELPERS & TOOLTIP FEEDBACK
        // Description: Floating text toasts, tooltip formatting, and CRT post-processing helpers.
        // =================================================================================================
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

_hybridCockpit?.RefreshAll();
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

_hybridCockpit?.RefreshAll();
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

// =================================================================================================
        // SECTION 12: CHRONOLOGICAL STORY & ACTION LOG
        // Description: Persistent log of narrative beats, player actions, skill check results, and
        //              dialogue history viewable via the Story Log modal.
        // =================================================================================================
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

public void EnsureAwakeningBeatLogged(int step)
        {
            if (step >= 1)
            {
                string beat1Log = "<b><color=#EF5350>[23:43] TV BROADCAST // EMERGENCY ALERT SYSTEM (CIVIL DANGER WARNING)</color></b>\n" +
                    "\"This message is transmitted at the request of the King County Sheriff's Office. This is not a test.\n\n" +
                    "A Civil Danger Warning is in effect for the Seattle metro area. Authorities are responding to rapidly spreading violent incidents and a suspected contagious illness. Avoid all physical contact.\n\n" +
                    "Shelter in place immediately. Major interstates are closed. Lock your doors, turn off lights, and stay inside. The National Guard is mobilizing. Lethal force is authorized.\"";

if (!_storyLogs.Contains(beat1Log))
                {
                    _storyLogs.Add(beat1Log);
                    RefreshStoryLogUI();
                    SyncLastAutoLoggedSituationFromLogs();
                }
            }

if (step >= 2)
            {
                string beat2Log = "<b><color=#F5BF42>[23:44] REACTION // INNER MONOLOGUE</color></b>\n" +
                    "\"Lethal force authorized... They don't lock down the whole city over a normal riot. And this front door won't stop a crowd if panic hits the hallway.\"\n\n" +
                    "<i>Through the wall, heavy footsteps run past in the corridor outside, followed by someone yelling and a fire door slamming shut.</i>";

if (!_storyLogs.Contains(beat2Log))
                {
                    _storyLogs.Add(beat2Log);
                    RefreshStoryLogUI();
                    SyncLastAutoLoggedSituationFromLogs();
                }
            }

if (step >= 3)
            {
                string beat3Log = "<b><color=#4FC3F7>[23:45] APARTMENT 407 // LIVING ROOM</color></b>\n" +
                    "Blue and red lights sweep across your living room curtains from the street below. The front deadbolt is still locked. Your keys, wallet, and everyday gear are sitting on the coffee table.";

if (!_storyLogs.Contains(beat3Log))
                {
                    _storyLogs.Add(beat3Log);
                    RefreshStoryLogUI();
                    SyncLastAutoLoggedSituationFromLogs();
                }
            }
        }

private void InitializeStoryLogs()
        {
            if (_storyLogs.Count > 0) return;

if (!introSeen && awakeningStep >= 1 && awakeningStep <= 3)
            {
                EnsureAwakeningBeatLogged(awakeningStep);
                return;
            }

EnsureAwakeningBeatLogged(3);
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

_hybridCockpit?.RefreshAll();
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

public void PlayClickSFX()

{
if (sfxSource != null && clickClip != null)

{
sfxSource.PlayOneShot(clickClip, 0.4f);
}
}

public void PlayBeepSFX()

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
    public class MainGameScreen : GameStageScreen
    {
    }

/// <summary>
    /// Master Stage & Console Controller for all gameplay zones and narrative acts.
    /// Manages the visual stage, HUD vitals, backpack inventory, and displays actor zones.
    /// </summary>
    [AddComponentMenu("Sunday's Last Breath/Game Screen (Stage)")]
    public class GameScreen : GameStageScreen
    {
    }
}

