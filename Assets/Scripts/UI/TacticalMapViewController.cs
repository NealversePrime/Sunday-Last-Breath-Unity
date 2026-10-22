using System;
using System.Collections.Generic;
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
    /// Architectural Blueprint & Bio-Radar Scanner for the Survivor PDA.
    /// Provides an informational floorplan inspection viewer with high-contrast typography,
    /// dark obsidian blueprint styling, color-coded doorway thresholds, and recon telemetry.
    /// Does NOT move the player (pure reference tool; player movement is driven by narrative gameplay).
    /// </summary>
    public class TacticalMapViewController : MonoBehaviour
    {
        // Backward-compatible alias for existing callers
        public static TacticalRoomIconType RoomIconType => TacticalRoomIconType.Home;

        [System.Serializable]
        public class NodeModel
        {
            public string id;
            public string roomCode;
            public string shortSub;
            public string title;
            public string statusTag;
            public string threatLevel;
            public string scavengeStatus;
            public string acousticScan;
            [TextArea(2, 4)] public string reconIntel;
            public bool isAccessible;
            public bool isDiscovered;
            public bool isStairs;
            public bool isHallway;
            public bool isBlocked;
            public bool isLocked;
            public TacticalRoomIconType iconType;
            public int row;
            public int col;
            public List<string> connectedIds = new List<string>();

            public NodeModel(string id, string roomCode, string shortSub, string title, string statusTag,
                             string threatLevel, string scavengeStatus, string acousticScan, string reconIntel,
                             bool isAccessible, bool isDiscovered, int row, int col, TacticalRoomIconType iconType,
                             bool isStairs = false, bool isHallway = false,
                             bool isBlocked = false, bool isLocked = false)
            {
                this.id = id;
                this.roomCode = roomCode;
                this.shortSub = shortSub;
                this.title = title;
                this.statusTag = statusTag;
                this.threatLevel = threatLevel;
                this.scavengeStatus = scavengeStatus;
                this.acousticScan = acousticScan;
                this.reconIntel = reconIntel;
                this.isAccessible = isAccessible;
                this.isDiscovered = isDiscovered;
                this.row = row;
                this.col = col;
                this.iconType = iconType;
                this.isStairs = isStairs;
                this.isHallway = isHallway;
                this.isBlocked = isBlocked;
                this.isLocked = isLocked;
            }
        }

        [System.Serializable]
        public class NodeUIHolder
        {
            public string nodeId;
            [System.NonSerialized] public NodeModel model;
            public GameObject rootObj;
            public Image bgImage;
            public Image patternOverlay;
            public Outline outline;
            public Image iconImage;
            public TextMeshProUGUI roomCodeTmp;
            public TextMeshProUGUI subLabelTmp;
            public GameObject navBadgeObj;
            public TextMeshProUGUI navBadgeTmp;
            public GameObject beaconObj;
            public Image doorTickImage;
            public Button button;
        }

        [Header("Runtime State")]
        [SerializeField] private string currentLocationId = "unit_407";
        [SerializeField] private string selectedNodeId = "unit_407";

        [Header("Modular Floor Data (Optional ScriptableObject)")]
        [SerializeField] private TacticalFloorMapData currentFloorData;

        [Header("Serialized Blueprint Nodes & Grid")]
        [SerializeField] private Image blueprintGridOverlayImage;
        [SerializeField] private List<NodeUIHolder> serializedHolders = new List<NodeUIHolder>();

        [Header("Blueprint Header & Telemetry Banners")]
        [SerializeField] private TextMeshProUGUI blueprintCoordTmp;
        [SerializeField] private TextMeshProUGUI blueprintFooterLocationTmp;

        [Header("Recon Intel Dossier References")]
        [SerializeField] private Image intelHeaderIcon;
        [SerializeField] private TextMeshProUGUI intelSectorTitleText;
        [SerializeField] private TextMeshProUGUI intelStatusBadgeText;
        [SerializeField] private TextMeshProUGUI intelThreatText;
        [SerializeField] private TextMeshProUGUI intelScavengeText;
        [SerializeField] private TextMeshProUGUI intelAcousticText;
        [SerializeField] private TextMeshProUGUI intelDescriptionText;
        [SerializeField] private TextMeshProUGUI quickNavPromptText;
        [SerializeField] private Button transitActionButton;
        [SerializeField] private Image transitActionBg;
        [SerializeField] private TextMeshProUGUI transitActionBtnText;

        private readonly Dictionary<string, NodeModel> nodeRegistry = new Dictionary<string, NodeModel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NodeUIHolder> uiHolders = new Dictionary<string, NodeUIHolder>(StringComparer.OrdinalIgnoreCase);

        private static Sprite cachedBlueprintGridSprite;
        private static Sprite cachedHatchedSprite;
        private static Sprite cachedStairTreadSprite;
        private static readonly Dictionary<TacticalRoomIconType, Sprite> cachedIcons = new Dictionary<TacticalRoomIconType, Sprite>();

        // =========================================================================
        // PROCEDURAL TEXTURE & ICON GENERATORS (Zero external assets or font emojis)
        // =========================================================================

        public static Sprite GetBlueprintGridSprite()
        {
            if (cachedBlueprintGridSprite != null) return cachedBlueprintGridSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color bg = new Color(0.025f, 0.038f, 0.058f, 1f);
            Color minorLine = new Color(0.08f, 0.16f, 0.24f, 1f);
            Color majorLine = new Color(0.12f, 0.24f, 0.36f, 1f);
            Color crossTick = new Color(0.18f, 0.42f, 0.58f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = bg;
                    if (x % 16 == 0 || y % 16 == 0) c = minorLine;
                    if (x == 0 || y == 0) c = majorLine;
                    if ((x == 0 && (y <= 2 || y >= size - 2)) || (y == 0 && (x <= 2 || x >= size - 2)))
                        c = crossTick;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            cachedBlueprintGridSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedBlueprintGridSprite;
        }

        public static Sprite GetHatchedSprite()
        {
            if (cachedHatchedSprite != null) return cachedHatchedSprite;

            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color cClear = new Color(0f, 0f, 0f, 0f);
            Color cStripe = new Color(1f, 1f, 1f, 0.22f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isStripe = ((x + y) % 8) < 3;
                    tex.SetPixel(x, y, isStripe ? cStripe : cClear);
                }
            }
            tex.Apply();
            cachedHatchedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedHatchedSprite;
        }

        public static Sprite GetStairTreadSprite()
        {
            if (cachedStairTreadSprite != null) return cachedStairTreadSprite;

            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color cClear = new Color(0f, 0f, 0f, 0f);
            Color cStep = new Color(0.85f, 0.65f, 0.25f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isStep = (x % 4 == 0) && (y > 2 && y < size - 2);
                    tex.SetPixel(x, y, isStep ? cStep : cClear);
                }
            }
            tex.Apply();
            cachedStairTreadSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedStairTreadSprite;
        }

        public static Sprite GetRoomIconSprite(TacticalRoomIconType type)
        {
            if (cachedIcons.TryGetValue(type, out Sprite existing) && existing != null)
                return existing;

            int s = 16;
            Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            string[] pattern = GetPixelPattern(type);
            for (int y = 0; y < s; y++)
            {
                string rowStr = pattern[s - 1 - y];
                for (int x = 0; x < s; x++)
                {
                    char ch = x < rowStr.Length ? rowStr[x] : '.';
                    tex.SetPixel(x, y, ch == '#' ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
            cachedIcons[type] = sp;
            return sp;
        }

        private static string[] GetPixelPattern(TacticalRoomIconType type)
        {
            switch (type)
            {
                case TacticalRoomIconType.Home:
                    return new[]
                    {
                        ".......##.......",
                        "......####......",
                        ".....######.....",
                        "....########....",
                        "...##########...",
                        "..############..",
                        ".##############.",
                        "...##########...",
                        "...###....###...",
                        "...###....###...",
                        "...###....###...",
                        "...##########...",
                        "...##########...",
                        "................",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Locked:
                    return new[]
                    {
                        ".....######.....",
                        "....########....",
                        "...###....###...",
                        "...##......##...",
                        "...##......##...",
                        "...##......##...",
                        "..############..",
                        "..############..",
                        "..#####..#####..",
                        "..#####..#####..",
                        "..#####..#####..",
                        "..############..",
                        "..############..",
                        "................",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Hazard:
                    return new[]
                    {
                        ".......##.......",
                        "......####......",
                        "......####......",
                        ".....######.....",
                        ".....##..##.....",
                        "....###..###....",
                        "....###..###....",
                        "...####..####...",
                        "...####..####...",
                        "..#####..#####..",
                        "..############..",
                        ".#####....#####.",
                        ".#####....#####.",
                        ".##############.",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.DoorAjar:
                    return new[]
                    {
                        "...##########...",
                        "...##########...",
                        "...###....###...",
                        "...###..#####...",
                        "...###.######...",
                        "...##########...",
                        "...#####..###...",
                        "...##########...",
                        "...###.######...",
                        "...###..#####...",
                        "...###....###...",
                        "...##########...",
                        ".##############.",
                        "................",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Utility:
                    return new[]
                    {
                        "........#####...",
                        ".......#####....",
                        "......#####.....",
                        ".....#####......",
                        "....#########...",
                        "...##########...",
                        "......#####.....",
                        ".....#####......",
                        "....#####.......",
                        "...#####........",
                        "..####..........",
                        ".###............",
                        "................",
                        "................",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Stairs:
                    return new[]
                    {
                        "............####",
                        "............####",
                        ".........#######",
                        ".........#######",
                        "......##########",
                        "......##########",
                        "...#############",
                        "...#############",
                        "################",
                        "################",
                        "..##............",
                        ".####...........",
                        "######..........",
                        "..##............",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Collapsed:
                    return new[]
                    {
                        "..###......###..",
                        ".#####....#####.",
                        "..#####..#####..",
                        "...##########...",
                        "....########....",
                        ".....######.....",
                        "....########....",
                        "...##########...",
                        "..#####..#####..",
                        ".#####....#####.",
                        "..###......###..",
                        "................",
                        ".##############.",
                        "................",
                        "................",
                        "................"
                    };
                case TacticalRoomIconType.Corridor:
                    return new[]
                    {
                        "..##........##..",
                        "..##...##...##..",
                        "..##...##...##..",
                        "..##........##..",
                        "..##...##...##..",
                        "..##...##...##..",
                        "..##........##..",
                        "..##...##...##..",
                        "..##...##...##..",
                        "..##........##..",
                        "..##...##...##..",
                        "..##...##...##..",
                        "..##........##..",
                        "................",
                        "................",
                        "................"
                    };
                default:
                    return new[]
                    {
                        "..############..",
                        "..############..",
                        "..##........##..",
                        "..##..####..##..",
                        "..##..####..##..",
                        "..##........##..",
                        "..##..####..##..",
                        "..##..####..##..",
                        "..##........##..",
                        "..############..",
                        "..############..",
                        "................",
                        "................",
                        "................",
                        "................",
                        "................"
                    };
            }
        }

        // =========================================================================
        // LIFECYCLE & INPUT HANDLING
        // =========================================================================

        private void Awake()
        {
            EnsureRuntimeBindings();
        }

        private void Start()
        {
            EnsureRuntimeBindings();
            RefreshAllNodes();
            SelectNode(currentLocationId);
        }

        private void OnEnable()
        {
            EnsureRuntimeBindings();
            RefreshAllNodes();
            SelectNode(string.IsNullOrEmpty(selectedNodeId) ? currentLocationId : selectedNodeId);
        }

        private void EnsureRuntimeBindings()
        {
            if (nodeRegistry.Count == 0)
            {
                InitializeDataModel();
            }

            if (blueprintGridOverlayImage != null && blueprintGridOverlayImage.sprite == null)
            {
                blueprintGridOverlayImage.sprite = GetBlueprintGridSprite();
                blueprintGridOverlayImage.type = Image.Type.Tiled;
            }

            if (serializedHolders != null && serializedHolders.Count > 0 && uiHolders.Count == 0)
            {
                foreach (var holder in serializedHolders)
                {
                    if (holder == null || string.IsNullOrEmpty(holder.nodeId)) continue;
                    if (!nodeRegistry.TryGetValue(holder.nodeId, out NodeModel model)) continue;

                    holder.model = model;
                    if (holder.iconImage != null)
                    {
                        holder.iconImage.sprite = GetRoomIconSprite(model.iconType);
                    }

                    string capturedId = holder.nodeId;
                    if (holder.button != null)
                    {
                        holder.button.onClick.RemoveAllListeners();
                        holder.button.onClick.AddListener(() =>
                        {
                            AudioManager.Instance?.PlayButtonClick();
                            OnNodeClicked(capturedId);
                        });
                    }

                    uiHolders[holder.nodeId] = holder;
                }
            }

            if (transitActionButton != null)
            {
                transitActionButton.onClick.RemoveAllListeners();
                transitActionButton.onClick.AddListener(OnTransitButtonClicked);
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            bool up = false, down = false, left = false, right = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                up = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
                down = Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;
                left = Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame;
                right = Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame;
            }
#else
            up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            down = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            left = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
#endif

            // Navigates inspection cursor in dossier without moving the player
            if (up) TrySelectDirection("W");
            else if (left) TrySelectDirection("A");
            else if (down) TrySelectDirection("S");
            else if (right) TrySelectDirection("D");
        }

        // =========================================================================
        // ARCHITECTURAL DATA MODEL (Modular ScriptableObject with 4F Defaults)
        // =========================================================================

        public void LoadFloor(TacticalFloorMapData floorData)
        {
            currentFloorData = floorData;
            nodeRegistry.Clear();
            InitializeDataModel();
            RefreshAllNodes();
            SelectNode(currentLocationId);
        }

        private void InitializeDataModel()
        {
            if (nodeRegistry.Count > 0) return;

            if (currentFloorData != null && currentFloorData.rooms != null && currentFloorData.rooms.Count > 0)
            {
                foreach (var r in currentFloorData.rooms)
                {
                    var node = new NodeModel(
                        r.roomId, r.roomCode, r.shortSub, r.fullTitle, r.statusTag,
                        r.threatLevel, r.scavengeStatus, r.acousticScan, r.reconIntel,
                        r.isAccessible, true, (int)r.position.y, (int)r.position.x, r.iconType,
                        r.isStairs, r.isHallway, r.isBlocked, r.isLocked);
                    node.connectedIds.AddRange(r.connectedRoomIds);
                    nodeRegistry[r.roomId] = node;
                }
                return;
            }

            // Built-in Default Floor: Ascott Residency 4th Floor
            // ROW 0: North Wing (Unit 407 <-> Hallway 1 <-> Unit 406/408)
            var u407 = new NodeModel(
                "unit_407", "UNIT 407", "SAFE ROOM", "YOUR RESIDENCE (SAFEHOUSE)", "SECURED",
                "NONE", "LOOTED / SECURE", "TV STATIC HUM",
                "Your residential apartment. The television flickers with civil defense emergency alert white noise. Heavy reinforced deadbolt held firm during the initial outbreak chaos. Direct exit to North Spine Corridor.",
                isAccessible: true, isDiscovered: true, row: 0, col: 0, iconType: TacticalRoomIconType.Home);

            var h1 = new NodeModel(
                "hallway_1", "CORR-N1", "NORTH SPINE", "NORTH HALLWAY JUNCTION", "CLEAR",
                "LOW", "UNSEARCHED", "FAINT DRAFT",
                "North corridor segment outside Units 407 and 406. Commercial carpet muffled under emergency amber lighting. Scattered luggage suggests residents evacuated in a panic.",
                isAccessible: true, isDiscovered: true, row: 0, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true);

            var u406 = new NodeModel(
                "unit_406", "UNIT 406", "PROFESSOR", "DR. ALISTAIR'S RESIDENCE", "DEADBOLT LOCKED",
                "UNKNOWN", "HIGH INTEL / LORE", "DEAD SILENCE",
                "Belongs to Dr. Alistair, retired university historian. Known to keep rare archival notes and emergency radio gear. Heavy brass keypad/deadbolt is engaged from within.",
                isAccessible: false, isDiscovered: true, row: 0, col: 2, iconType: TacticalRoomIconType.Locked, isLocked: true);

            // ROW 1: Mid-North Wing (Unit 405 <-> Hallway 2 <-> Unit 404)
            var u405 = new NodeModel(
                "unit_405", "UNIT 405", "HUNTER'S APT", "MARCUS'S APARTMENT", "REINFORCED LOCK",
                "UNKNOWN", "WEAPONS / AMMO", "METALLIC CLICK",
                "Residence of Marcus, an experienced outdoorsman and big-game hunter. Faint scent of gun oil and cordite seeps beneath the steel-reinforced door frame. High-priority tactical cache.",
                isAccessible: false, isDiscovered: true, row: 1, col: 0, iconType: TacticalRoomIconType.Locked, isLocked: true);

            var h2 = new NodeModel(
                "hallway_2", "CORR-N2", "UPPER-MID", "CENTRAL-NORTH CORRIDOR", "SHADOWED",
                "MEDIUM", "SCATTERED DEBRIS", "WALL SCRATCHING",
                "Upper-middle hallway corridor. Shattered wall sconces and overturned side tables partially narrow the path. Intermittent rasping echoes from behind the East wing drywall.",
                isAccessible: true, isDiscovered: true, row: 1, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true);

            var u404 = new NodeModel(
                "unit_404", "UNIT 404", "BARRICADED", "QUARANTINED NEIGHBOR", "BARRICADED",
                "HIGH", "INACCESSIBLE", "HEAVY THUMPING",
                "Doorway violently nailed shut from the inside with timber planks and heavy furniture. Deep, frantic gouge marks score the outer mahogany veneer. Do not breach.",
                isAccessible: false, isDiscovered: true, row: 1, col: 2, iconType: TacticalRoomIconType.Hazard, isBlocked: true);

            // ROW 2: Mid-South Wing (Unit 403 <-> Hallway 3 <-> Unit 402)
            var u403 = new NodeModel(
                "unit_403", "UNIT 403", "BIOHAZARD", "CONTAMINATED RESIDENCE", "BIOHAZARD",
                "DANGER", "UNKNOWN", "FLIES / SILENCE",
                "A dark coagulated smear of blood trails down the brass handle onto the threshold. The door frame is warped and jammed shut. Entering without bio-protective gear is lethal.",
                isAccessible: false, isDiscovered: true, row: 2, col: 0, iconType: TacticalRoomIconType.Hazard, isBlocked: true);

            var h3 = new NodeModel(
                "hallway_3", "CORR-S1", "LOWER-MID", "CENTRAL-SOUTH CORRIDOR", "FLICKERING",
                "MEDIUM", "UNSEARCHED", "DISTANT MUSIC",
                "Fluorescent ceiling tube strobes erratically, casting harsh staccato shadows. Dark footprints on the runner rug lead southward toward the fire exit stairwell.",
                isAccessible: true, isDiscovered: true, row: 2, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true);

            var u402 = new NodeModel(
                "unit_402", "UNIT 402", "DOOR AJAR", "ANDERSONS' RESIDENCE", "DOOR AJAR",
                "LOW", "MEDICAL / RATIONS", "RUNNING WATER",
                "The safety chain is snapped and the door hangs open two inches. Sound of an overflowing sink inside. High probability of salvageable first-aid supplies and canned provisions.",
                isAccessible: true, isDiscovered: true, row: 2, col: 2, iconType: TacticalRoomIconType.DoorAjar);

            // ROW 3: South Wing (Unit 401 <-> Hallway 4 <-> Utility Closet)
            var u401 = new NodeModel(
                "unit_401", "UNIT 401", "CORNER APT", "STUDENT RESIDENCE 401", "UNLOCKED",
                "LOW", "KEYS / VALUABLES", "WIND THROUGH GLASS",
                "South-west corner apartment. Unopened mail and flyers are piled on the welcome mat. Latch appears unlocked following the post-party evacuation.",
                isAccessible: true, isDiscovered: true, row: 3, col: 0, iconType: TacticalRoomIconType.Apartment);

            var h4 = new NodeModel(
                "hallway_4", "CORR-S2", "SOUTH SPINE", "SOUTH SERVICE JUNCTION", "ELEVATED",
                "MEDIUM", "UNSEARCHED", "OZONE & SMOKE",
                "Southern corridor intersection bordering the electrical utility room and stairwell foyer. The air is thick with the acrid smell of blown transformers and damp concrete.",
                isAccessible: true, isDiscovered: true, row: 3, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true);

            var util = new NodeModel(
                "utility", "UTILITY", "MAINTENANCE", "ELECTRICAL & SERVICE CLOSET", "SERVICE ACCESS",
                "LOW", "TOOLS / FUSES", "TRANSFORMER HUM",
                "Building maintenance alcove with a heavy steel louver door. Houses the 4th floor master breaker panel, backup fuses, and janitorial hardware lockers.",
                isAccessible: true, isDiscovered: true, row: 3, col: 2, iconType: TacticalRoomIconType.Utility);

            // ROW 4: Exit Foyer (Stairwell <-> Hallway End <-> Collapsed East Wing)
            var stairs = new NodeModel(
                "stairs", "STAIRWELL", "TO FLOOR 2", "WEST EMERGENCY STAIRWELL", "ESCAPE ROUTE",
                "MEDIUM", "PRIMARY EXIT", "ECHOING GROANS",
                "Reinforced concrete fire stairwell descending to the 2nd Floor and Ground Lobby. Emergency push-bar door is operational. Descending past this point commits to leaving the 4th floor.",
                isAccessible: true, isDiscovered: true, row: 4, col: 0, iconType: TacticalRoomIconType.Stairs, isStairs: true);

            var hEnd = new NodeModel(
                "hallway_end", "FOYER-4S", "EXIT FOYER", "STAIRWELL ACCESS FOYER", "CLEAR",
                "MEDIUM", "EXTINGUISHER", "DRAFT FROM BELOW",
                "Southern terminus of the 4th floor spine corridor. A discharged fire extinguisher lies rolling on the tiles. Direct access West to the Fire Stairwell.",
                isAccessible: true, isDiscovered: true, row: 4, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true);

            var blocked = new NodeModel(
                "empty_bot", "EAST WING", "COLLAPSED", "COLLAPSED EAST ALCOVE", "STRUCTURAL FAIL",
                "IMPASSABLE", "NONE", "SETTLING CONCRETE",
                "Load-bearing ceiling beams and concrete slabs have caved in, completely sealing the south-east service shaft. Impassable without heavy excavation equipment.",
                isAccessible: false, isDiscovered: false, row: 4, col: 2, iconType: TacticalRoomIconType.Collapsed, isBlocked: true);

            // Connect Orthogonal Floorplan Graph
            ConnectBi(u407, h1);
            ConnectBi(h1, u406);
            ConnectBi(h1, h2);

            ConnectBi(u405, h2);
            ConnectBi(h2, u404);
            ConnectBi(h2, h3);

            ConnectBi(u403, h3);
            ConnectBi(h3, u402);
            ConnectBi(h3, h4);

            ConnectBi(u401, h4);
            ConnectBi(h4, util);
            ConnectBi(h4, hEnd);

            ConnectBi(stairs, hEnd);
            ConnectBi(hEnd, blocked);

            var list = new[] { u407, h1, u406, u405, h2, u404, u403, h3, u402, u401, h4, util, stairs, hEnd, blocked };
            foreach (var n in list) nodeRegistry[n.id] = n;
        }

        private static void ConnectBi(NodeModel a, NodeModel b)
        {
            if (!a.connectedIds.Contains(b.id)) a.connectedIds.Add(b.id);
            if (!b.connectedIds.Contains(a.id)) b.connectedIds.Add(a.id);
        }

        // =========================================================================
        // UI BINDING & INTERACTION
        // =========================================================================

        public void BindUIHolder(string nodeId, GameObject rootObj, Image bgImage, Image patternOverlay,
                                 Outline outline, Image iconImage, TextMeshProUGUI roomCodeTmp,
                                 TextMeshProUGUI subLabelTmp, GameObject navBadgeObj, TextMeshProUGUI navBadgeTmp,
                                 GameObject beaconObj, Image doorTickImage, Button btn)
        {
            if (nodeRegistry.Count == 0)
            {
                InitializeDataModel();
            }

            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;

            var holder = new NodeUIHolder
            {
                nodeId = nodeId,
                model = model,
                rootObj = rootObj,
                bgImage = bgImage,
                patternOverlay = patternOverlay,
                outline = outline,
                iconImage = iconImage,
                roomCodeTmp = roomCodeTmp,
                subLabelTmp = subLabelTmp,
                navBadgeObj = navBadgeObj,
                navBadgeTmp = navBadgeTmp,
                beaconObj = beaconObj,
                doorTickImage = doorTickImage,
                button = btn
            };

            if (iconImage != null)
            {
                iconImage.sprite = GetRoomIconSprite(model.iconType);
            }

            serializedHolders.RemoveAll(h => h != null && string.Equals(h.nodeId, nodeId, StringComparison.OrdinalIgnoreCase));
            serializedHolders.Add(holder);
            uiHolders[nodeId] = holder;

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    OnNodeClicked(nodeId);
                });
            }

            ApplyVisualState(holder);
        }

        public void BindBlueprintBanners(TextMeshProUGUI coordTmp, TextMeshProUGUI footerLocationTmp, Image gridOverlayImg = null)
        {
            blueprintCoordTmp = coordTmp;
            blueprintFooterLocationTmp = footerLocationTmp;
            if (gridOverlayImg != null)
            {
                blueprintGridOverlayImage = gridOverlayImg;
                blueprintGridOverlayImage.sprite = GetBlueprintGridSprite();
                blueprintGridOverlayImage.type = Image.Type.Tiled;
            }
        }

        public void BindIntelReferences(Image headerIcon, TextMeshProUGUI title, TextMeshProUGUI status,
                                        TextMeshProUGUI threat, TextMeshProUGUI scavenge, TextMeshProUGUI acoustic,
                                        TextMeshProUGUI desc, TextMeshProUGUI quickNav,
                                        Button transitBtn, Image transitBg, TextMeshProUGUI transitTxt)
        {
            intelHeaderIcon = headerIcon;
            intelSectorTitleText = title;
            intelStatusBadgeText = status;
            intelThreatText = threat;
            intelScavengeText = scavenge;
            intelAcousticText = acoustic;
            intelDescriptionText = desc;
            quickNavPromptText = quickNav;
            transitActionButton = transitBtn;
            transitActionBg = transitBg;
            transitActionBtnText = transitTxt;

            if (transitActionButton != null)
            {
                transitActionButton.onClick.RemoveAllListeners();
                transitActionButton.onClick.AddListener(OnTransitButtonClicked);
            }

            RefreshAllNodes();
            SelectNode(currentLocationId);
        }

        public void SetCurrentLocation(string locationId)
        {
            if (!nodeRegistry.ContainsKey(locationId)) return;
            currentLocationId = locationId;
            RefreshAllNodes();
            SelectNode(locationId);
        }

        private void OnNodeClicked(string nodeId)
        {
            // Pure recon inspection — clicking any sector selects it in the right dossier without moving player
            SelectNode(nodeId);
        }

        private void OnTransitButtonClicked()
        {
            // Informational only — player movement is controlled by narrative actions, not map teleportation
            AudioManager.Instance?.PlayButtonClick();
        }

        public void SelectNode(string nodeId)
        {
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;
            selectedNodeId = nodeId;

            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }

            bool isCurrent = string.Equals(model.id, currentLocationId, StringComparison.OrdinalIgnoreCase);

            // Update Blueprint Coordinate & Footer Readout
            if (blueprintCoordTmp != null)
            {
                string gridX = model.col == 0 ? "W-01" : (model.col == 1 ? "C-00" : "E-02");
                string gridY = $"R-0{model.row + 1}";
                blueprintCoordTmp.text = $"GRID [{gridX}:{gridY}] // BIO-RADAR: ACTIVE";
            }

            if (blueprintFooterLocationTmp != null && nodeRegistry.TryGetValue(currentLocationId, out NodeModel playerNode))
            {
                blueprintFooterLocationTmp.text =
                    $"<color=#00e5ff><b>PLAYER POS:</b> {playerNode.roomCode} ({playerNode.shortSub})</color>   <color=#475569>|</color>   <color=#f39c12><b>INSPECTING:</b> {model.roomCode}</color>";
            }

            // Update Right Dossier Header & Icon
            if (intelHeaderIcon != null)
            {
                intelHeaderIcon.sprite = GetRoomIconSprite(model.iconType);
                intelHeaderIcon.color = isCurrent
                    ? new Color(0.0f, 0.90f, 1.0f)
                    : (model.isAccessible ? new Color(0.25f, 0.88f, 0.55f) : (model.isLocked ? new Color(0.95f, 0.65f, 0.15f) : new Color(0.92f, 0.30f, 0.30f)));
            }

            if (intelSectorTitleText != null)
            {
                intelSectorTitleText.text = $"<b>{model.roomCode} // {model.title}</b>";
            }

            if (intelStatusBadgeText != null)
            {
                string colorHex = isCurrent ? "#00e5ff" : (model.isAccessible ? "#2ecc71" : (model.isLocked ? "#f39c12" : "#e74c3c"));
                string tag = isCurrent ? "CURRENT SECTOR" : model.statusTag;
                intelStatusBadgeText.text = $"<color={colorHex}><b>[ {tag} ]</b></color>";
            }

            if (intelThreatText != null)
            {
                string threatCol = model.threatLevel switch
                {
                    "NONE" or "ZERO" => "#2ecc71",
                    "LOW" or "MINIMAL" => "#38bdf8",
                    "MEDIUM" => "#f39c12",
                    "HIGH" or "DANGER" or "IMPASSABLE" => "#e74c3c",
                    _ => "#94a3b8"
                };
                intelThreatText.text = $"<color=#64748b>THREAT:</color> <color={threatCol}><b>{model.threatLevel}</b></color>";
            }

            if (intelScavengeText != null)
            {
                intelScavengeText.text = $"<color=#64748b>SALVAGE:</color> <color=#f39c12><b>{model.scavengeStatus}</b></color>";
            }

            if (intelAcousticText != null)
            {
                intelAcousticText.text = $"<color=#64748b>AUDIO:</color> <color=#38bdf8><b>{model.acousticScan}</b></color>";
            }

            if (intelDescriptionText != null)
            {
                intelDescriptionText.text = model.reconIntel;
            }

            // Informational Prompt & Clearance Status Bar (Zero Teleportation)
            if (quickNavPromptText != null)
            {
                quickNavPromptText.text = "<color=#94a3b8>Click any sector on the blueprint or use arrow keys to inspect recon telemetry & architectural notes.</color>";
            }

            if (transitActionButton != null && transitActionBg != null && transitActionBtnText != null)
            {
                transitActionButton.interactable = false;

                if (isCurrent)
                {
                    transitActionBg.color = new Color(0.04f, 0.14f, 0.18f, 0.95f);
                    transitActionBtnText.text = "<color=#00e5ff><b>[ CURRENT LOCATION // SAFEHOUSE 407 ]</b></color>";
                }
                else if (model.isAccessible)
                {
                    transitActionBg.color = new Color(0.04f, 0.16f, 0.10f, 0.95f);
                    transitActionBtnText.text = "<color=#2ecc71><b>[ DOORWAY ACCESS: OPEN CORRIDOR ACCESS ]</b></color>";
                }
                else if (model.isLocked)
                {
                    transitActionBg.color = new Color(0.16f, 0.10f, 0.04f, 0.95f);
                    transitActionBtnText.text = "<color=#f39c12><b>[ SECURITY STATUS: REINFORCED DEADBOLT ENGAGED ]</b></color>";
                }
                else if (model.isBlocked)
                {
                    transitActionBg.color = new Color(0.18f, 0.05f, 0.06f, 0.95f);
                    transitActionBtnText.text = "<color=#e74c3c><b>[ ACCESS BLOCKED: STRUCTURAL HAZARD / BARRICADE ]</b></color>";
                }
                else
                {
                    transitActionBg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
                    transitActionBtnText.text = "<color=#94a3b8><b>[ REMOTE SECTOR // ARCHITECTURAL RECON ]</b></color>";
                }
            }
        }

        private void TrySelectDirection(string directionKey)
        {
            string baseId = string.IsNullOrEmpty(selectedNodeId) ? currentLocationId : selectedNodeId;
            if (!nodeRegistry.TryGetValue(baseId, out NodeModel current)) return;

            foreach (string neighborId in current.connectedIds)
            {
                if (!nodeRegistry.TryGetValue(neighborId, out NodeModel neighbor)) continue;
                string dir = GetDirectionLetter(current, neighbor);
                if (string.Equals(dir, directionKey, StringComparison.OrdinalIgnoreCase))
                {
                    AudioManager.Instance?.PlayButtonClick();
                    SelectNode(neighbor.id);
                    return;
                }
            }
        }

        private static string GetDirectionLetter(NodeModel fromNode, NodeModel toNode)
        {
            if (toNode.row < fromNode.row) return "W";
            if (toNode.row > fromNode.row) return "S";
            if (toNode.col < fromNode.col) return "A";
            if (toNode.col > fromNode.col) return "D";
            return "";
        }

        public void RefreshAllNodes()
        {
            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }
        }

        // =========================================================================
        // BLUEPRINT CLEARANCE & VISUAL STYLING (RE2 + Signalis + Sevastolink)
        // =========================================================================

        private void ApplyVisualState(NodeUIHolder holder)
        {
            if (holder == null || holder.model == null) return;

            bool isCurrent = string.Equals(holder.model.id, currentLocationId, StringComparison.OrdinalIgnoreCase);
            bool isSelected = string.Equals(holder.model.id, selectedNodeId, StringComparison.OrdinalIgnoreCase);

            // Player Radar Beacon (Only illuminates the sector where the player actually is)
            if (holder.beaconObj != null)
            {
                holder.beaconObj.SetActive(isCurrent);
            }

            // Directional & Status Chip Badge (Top-Right of Sector)
            if (holder.navBadgeObj != null)
            {
                if (holder.navBadgeTmp != null)
                {
                    if (isCurrent)
                    {
                        holder.navBadgeObj.SetActive(true);
                        holder.navBadgeTmp.text = "[HERE]";
                        holder.navBadgeTmp.color = new Color(0.0f, 0.95f, 1.0f);
                    }
                    else if (holder.model.isLocked)
                    {
                        holder.navBadgeObj.SetActive(true);
                        holder.navBadgeTmp.text = "[LOCK]";
                        holder.navBadgeTmp.color = new Color(0.95f, 0.65f, 0.15f);
                    }
                    else if (holder.model.isBlocked)
                    {
                        holder.navBadgeObj.SetActive(true);
                        holder.navBadgeTmp.text = "[HAZ]";
                        holder.navBadgeTmp.color = new Color(0.92f, 0.30f, 0.30f);
                    }
                    else if (holder.model.isStairs)
                    {
                        holder.navBadgeObj.SetActive(true);
                        holder.navBadgeTmp.text = "[EXIT]";
                        holder.navBadgeTmp.color = new Color(0.95f, 0.60f, 0.18f);
                    }
                    else
                    {
                        holder.navBadgeObj.SetActive(false);
                    }
                }
            }

            // Procedural Pattern Overlay (Subtle 0.10f - 0.12f alpha so it adds tactical CAD texture without obscuring text)
            if (holder.patternOverlay != null)
            {
                if (holder.model.isBlocked || holder.model.isLocked)
                {
                    holder.patternOverlay.gameObject.SetActive(true);
                    holder.patternOverlay.sprite = GetHatchedSprite();
                    holder.patternOverlay.type = Image.Type.Tiled;
                    holder.patternOverlay.color = holder.model.isLocked
                        ? new Color(0.95f, 0.65f, 0.15f, 0.10f)
                        : new Color(0.90f, 0.25f, 0.25f, 0.12f);
                }
                else if (holder.model.isStairs)
                {
                    holder.patternOverlay.gameObject.SetActive(true);
                    holder.patternOverlay.sprite = GetStairTreadSprite();
                    holder.patternOverlay.type = Image.Type.Tiled;
                    holder.patternOverlay.color = new Color(0.95f, 0.65f, 0.25f, 0.12f);
                }
                else
                {
                    holder.patternOverlay.gameObject.SetActive(false);
                }
            }

            // Doorway Threshold Tick Color (Clear, vibrant threshold marker connecting room to spine corridor)
            if (holder.doorTickImage != null)
            {
                if (isCurrent)
                    holder.doorTickImage.color = new Color(0.0f, 0.90f, 1.0f, 0.95f); // Glowing Cyan threshold
                else if (holder.model.isAccessible)
                    holder.doorTickImage.color = new Color(0.18f, 0.78f, 0.48f, 0.85f); // Accessible Emerald threshold
                else if (holder.model.isLocked)
                    holder.doorTickImage.color = new Color(0.95f, 0.60f, 0.12f, 0.90f); // Locked Amber threshold
                else
                    holder.doorTickImage.color = new Color(0.88f, 0.24f, 0.24f, 0.90f); // Barricaded Crimson threshold
            }

            // High-Contrast Dark Obsidian Core & Crisp White Typography (Zero blinding solid neon fills)
            if (isCurrent)
            {
                // Player Location: Deep Obsidian Slate with Subtle Cyan Core Glow & Glowing Outline
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.035f, 0.08f, 0.12f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.0f, 0.92f, 1.0f, 1.0f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.0f, 0.96f, 1.0f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "CURRENT POS";
                    holder.subLabelTmp.color = new Color(0.0f, 0.92f, 1.0f);
                }
            }
            else if (isSelected)
            {
                // Active Scanner Target: Deep Obsidian with Tactical Amber Selection Outline
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.06f, 0.07f, 0.09f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.98f, 0.72f, 0.18f, 1.0f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.98f, 0.72f, 0.18f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.98f, 0.78f, 0.35f);
                }
            }
            else if (holder.model.isStairs)
            {
                // Fire Stairwell Exit: Deep Obsidian with Tactical Amber-Slate Wall
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.04f, 0.05f, 0.07f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.85f, 0.55f, 0.18f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.95f, 0.60f, 0.18f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.85f, 0.65f, 0.35f);
                }
            }
            else if (holder.model.isHallway)
            {
                // Spine Corridor Segment: Recessed Blueprint Slate
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.035f, 0.055f, 0.08f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.20f, 0.32f, 0.44f, 0.80f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.42f, 0.62f, 0.78f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.55f, 0.68f, 0.80f);
                }
            }
            else if (holder.model.isLocked)
            {
                // Locked Unit: Deep Obsidian with Amber Security Stroke
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.045f, 0.05f, 0.065f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.85f, 0.55f, 0.15f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.95f, 0.65f, 0.18f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "LOCKED";
                    holder.subLabelTmp.color = new Color(0.90f, 0.70f, 0.30f);
                }
            }
            else if (holder.model.isBlocked)
            {
                // Barricaded / Biohazard / Collapsed: Deep Obsidian with Crimson Alert Stroke
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.05f, 0.04f, 0.045f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.92f, 0.30f, 0.30f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.90f, 0.45f, 0.45f);
                }
            }
            else
            {
                // Accessible / Open Apartment: Deep Obsidian with Steel-Mint Stroke
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.035f, 0.055f, 0.085f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.20f, 0.60f, 0.48f, 0.80f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.25f, 0.85f, 0.60f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.55f, 0.78f, 0.70f);
                }
            }
        }
    }
}
