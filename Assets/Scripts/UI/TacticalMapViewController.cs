using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Architectural Blueprint & Bio-Radar Scanner for the Survivor PDA (4th Floor Ascott Residency).
    /// Combines Resident Evil 2 Remake clearance color-coding, Signalis/Sevastolink CAD blueprint grids,
    /// contiguous room-and-corridor schematic geometry, doorway threshold indicators, procedural pixel/vector
    /// room icons (zero missing font glyph warnings), and interactive WASD / click sector navigation.
    /// </summary>
    public class TacticalMapViewController : MonoBehaviour
    {
        public enum RoomIconType
        {
            Home,
            Corridor,
            Locked,
            Hazard,
            DoorAjar,
            Apartment,
            Utility,
            Stairs,
            Collapsed
        }

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
            public RoomIconType iconType;
            public int row;
            public int col;
            public List<string> connectedIds = new List<string>();

            public NodeModel(string id, string roomCode, string shortSub, string title, string statusTag,
                             string threatLevel, string scavengeStatus, string acousticScan, string reconIntel,
                             bool isAccessible, bool isDiscovered, int row, int col, RoomIconType iconType,
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
        private static readonly Dictionary<RoomIconType, Sprite> cachedIcons = new Dictionary<RoomIconType, Sprite>();

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

        public static Sprite GetRoomIconSprite(RoomIconType type)
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

        private static string[] GetPixelPattern(RoomIconType type)
        {
            switch (type)
            {
                case RoomIconType.Home:
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
                case RoomIconType.Locked:
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
                case RoomIconType.Hazard:
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
                case RoomIconType.DoorAjar:
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
                case RoomIconType.Utility:
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
                case RoomIconType.Stairs:
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
                case RoomIconType.Collapsed:
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
                case RoomIconType.Corridor:
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

            if (up) TryNavigateDirection("W");
            else if (left) TryNavigateDirection("A");
            else if (down) TryNavigateDirection("S");
            else if (right) TryNavigateDirection("D");
        }

        // =========================================================================
        // 4TH FLOOR ARCHITECTURAL DATA MODEL (Twine Canonical Rooms)
        // =========================================================================

        private void InitializeDataModel()
        {
            if (nodeRegistry.Count > 0) return;

            // ROW 0: North Wing (Unit 407 <-> Hallway 1 <-> Unit 406/408)
            var u407 = new NodeModel(
                "unit_407", "UNIT 407", "SAFE ROOM", "YOUR RESIDENCE (SAFEHOUSE)", "SECURED",
                "MINIMAL", "LOOTED / SECURE", "TV STATIC HUM",
                "Your residential apartment. The television flickers with civil defense white noise. Heavy reinforced deadbolt held firm during the initial outbreak chaos. Direct exit to North Spine Corridor.",
                isAccessible: true, isDiscovered: true, row: 0, col: 0, iconType: RoomIconType.Home);

            var h1 = new NodeModel(
                "hallway_1", "CORR-N1", "NORTH SPINE", "NORTH HALLWAY JUNCTION", "CLEAR",
                "LOW", "UNSEARCHED", "FAINT DRAFT",
                "North corridor segment outside Units 407 and 406. Commercial carpet muffled under emergency amber lighting. Scattered luggage suggests residents evacuated in a panic.",
                isAccessible: true, isDiscovered: true, row: 0, col: 1, iconType: RoomIconType.Corridor, isHallway: true);

            var u406 = new NodeModel(
                "unit_406", "UNIT 406", "PROFESSOR", "DR. ALISTAIR'S RESIDENCE", "DEADBOLT LOCKED",
                "UNKNOWN", "HIGH INTEL / LORE", "DEAD SILENCE",
                "Belongs to Dr. Alistair, retired university historian. Known to keep rare archival notes and emergency radio gear. Heavy brass keypad/deadbolt is engaged from within.",
                isAccessible: false, isDiscovered: true, row: 0, col: 2, iconType: RoomIconType.Locked, isLocked: true);

            // ROW 1: Mid-North Wing (Unit 405 <-> Hallway 2 <-> Unit 404)
            var u405 = new NodeModel(
                "unit_405", "UNIT 405", "HUNTER'S APT", "MARCUS'S APARTMENT", "REINFORCED LOCK",
                "UNKNOWN", "WEAPONS / AMMO", "METALLIC CLICK",
                "Residence of Marcus, an experienced outdoorsman and big-game hunter. Faint scent of gun oil and cordite seeps beneath the steel-reinforced door frame. High-priority tactical cache.",
                isAccessible: false, isDiscovered: true, row: 1, col: 0, iconType: RoomIconType.Locked, isLocked: true);

            var h2 = new NodeModel(
                "hallway_2", "CORR-N2", "UPPER-MID", "CENTRAL-NORTH CORRIDOR", "SHADOWED",
                "MEDIUM", "SCATTERED DEBRIS", "WALL SCRATCHING",
                "Upper-middle hallway corridor. Shattered wall sconces and overturned side tables partially narrow the path. Intermittent rasping echoes from behind the East wing drywall.",
                isAccessible: true, isDiscovered: true, row: 1, col: 1, iconType: RoomIconType.Corridor, isHallway: true);

            var u404 = new NodeModel(
                "unit_404", "UNIT 404", "BARRICADED", "QUARANTINED NEIGHBOR", "BARRICADED",
                "HIGH", "INACCESSIBLE", "HEAVY THUMPING",
                "Doorway violently nailed shut from the inside with timber planks and heavy furniture. Deep, frantic gouge marks score the outer mahogany veneer. Do not breach.",
                isAccessible: false, isDiscovered: true, row: 1, col: 2, iconType: RoomIconType.Hazard, isBlocked: true);

            // ROW 2: Mid-South Wing (Unit 403 <-> Hallway 3 <-> Unit 402)
            var u403 = new NodeModel(
                "unit_403", "UNIT 403", "BIOHAZARD", "CONTAMINATED RESIDENCE", "BIOHAZARD",
                "DANGER", "UNKNOWN", "FLIES / SILENCE",
                "A dark coagulated smear of blood trails down the brass handle onto the threshold. The door frame is warped and jammed shut. Entering without bio-protective gear is lethal.",
                isAccessible: false, isDiscovered: true, row: 2, col: 0, iconType: RoomIconType.Hazard, isBlocked: true);

            var h3 = new NodeModel(
                "hallway_3", "CORR-S1", "LOWER-MID", "CENTRAL-SOUTH CORRIDOR", "FLICKERING",
                "MEDIUM", "UNSEARCHED", "DISTANT MUSIC",
                "Fluorescent ceiling tube strobes erratically, casting harsh staccato shadows. Dark footprints on the runner rug lead southward toward the fire exit stairwell.",
                isAccessible: true, isDiscovered: true, row: 2, col: 1, iconType: RoomIconType.Corridor, isHallway: true);

            var u402 = new NodeModel(
                "unit_402", "UNIT 402", "DOOR AJAR", "ANDERSONS' RESIDENCE", "DOOR AJAR",
                "LOW", "MEDICAL / RATIONS", "RUNNING WATER",
                "The safety chain is snapped and the door hangs open two inches. Sound of an overflowing sink inside. High probability of salvageable first-aid supplies and canned provisions.",
                isAccessible: true, isDiscovered: true, row: 2, col: 2, iconType: RoomIconType.DoorAjar);

            // ROW 3: South Wing (Unit 401 <-> Hallway 4 <-> Utility Closet)
            var u401 = new NodeModel(
                "unit_401", "UNIT 401", "CORNER APT", "STUDENT RESIDENCE 401", "UNLOCKED",
                "LOW", "KEYS / VALUABLES", "WIND THROUGH GLASS",
                "South-west corner apartment. Unopened mail and flyers are piled on the welcome mat. Latch appears unlocked following the post-party evacuation.",
                isAccessible: true, isDiscovered: true, row: 3, col: 0, iconType: RoomIconType.Apartment);

            var h4 = new NodeModel(
                "hallway_4", "CORR-S2", "SOUTH SPINE", "SOUTH SERVICE JUNCTION", "ELEVATED",
                "MEDIUM", "UNSEARCHED", "OZONE & SMOKE",
                "Southern corridor intersection bordering the electrical utility room and stairwell foyer. The air is thick with the acrid smell of blown transformers and damp concrete.",
                isAccessible: true, isDiscovered: true, row: 3, col: 1, iconType: RoomIconType.Corridor, isHallway: true);

            var util = new NodeModel(
                "utility", "UTILITY", "MAINTENANCE", "ELECTRICAL & SERVICE CLOSET", "SERVICE ACCESS",
                "LOW", "TOOLS / FUSES", "TRANSFORMER HUM",
                "Building maintenance alcove with a heavy steel louver door. Houses the 4th floor master breaker panel, backup fuses, and janitorial hardware lockers.",
                isAccessible: true, isDiscovered: true, row: 3, col: 2, iconType: RoomIconType.Utility);

            // ROW 4: Exit Foyer (Stairwell <-> Hallway End <-> Collapsed East Wing)
            var stairs = new NodeModel(
                "stairs", "STAIRWELL", "TO FLOOR 2", "WEST EMERGENCY STAIRWELL", "ESCAPE ROUTE",
                "MEDIUM", "PRIMARY EXIT", "ECHOING GROANS",
                "Reinforced concrete fire stairwell descending to the 2nd Floor and Ground Lobby. Emergency push-bar door is operational. Descending past this point commits to leaving the 4th floor.",
                isAccessible: true, isDiscovered: true, row: 4, col: 0, iconType: RoomIconType.Stairs, isStairs: true);

            var hEnd = new NodeModel(
                "hallway_end", "FOYER-4S", "EXIT FOYER", "STAIRWELL ACCESS FOYER", "CLEAR",
                "MEDIUM", "EXTINGUISHER", "DRAFT FROM BELOW",
                "Southern terminus of the 4th floor spine corridor. A discharged fire extinguisher lies rolling on the tiles. Direct access West to the Fire Stairwell.",
                isAccessible: true, isDiscovered: true, row: 4, col: 1, iconType: RoomIconType.Corridor, isHallway: true);

            var blocked = new NodeModel(
                "empty_bot", "EAST WING", "COLLAPSED", "COLLAPSED EAST ALCOVE", "STRUCTURAL FAIL",
                "IMPASSABLE", "NONE", "SETTLING CONCRETE",
                "Load-bearing ceiling beams and concrete slabs have caved in, completely sealing the south-east service shaft. Impassable without heavy excavation equipment.",
                isAccessible: false, isDiscovered: false, row: 4, col: 2, iconType: RoomIconType.Collapsed, isBlocked: true);

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
            // If clicking an already-selected adjacent accessible room, step into it; otherwise select it for recon
            if (string.Equals(selectedNodeId, nodeId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentLocationId, nodeId, StringComparison.OrdinalIgnoreCase))
            {
                if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr) &&
                    curr.connectedIds.Contains(nodeId) &&
                    nodeRegistry.TryGetValue(nodeId, out NodeModel target) &&
                    target.isAccessible)
                {
                    SetCurrentLocation(nodeId);
                    return;
                }
            }

            SelectNode(nodeId);
        }

        private void OnTransitButtonClicked()
        {
            AudioManager.Instance?.PlayButtonClick();

            if (!nodeRegistry.TryGetValue(selectedNodeId, out NodeModel target)) return;

            if (string.Equals(selectedNodeId, currentLocationId, StringComparison.OrdinalIgnoreCase))
            {
                // If already at current location and not in 407, allow quick reset to 407 safehouse
                if (!string.Equals(currentLocationId, "unit_407", StringComparison.OrdinalIgnoreCase))
                {
                    SetCurrentLocation("unit_407");
                }
                return;
            }

            if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr) && curr.connectedIds.Contains(target.id))
            {
                if (target.isAccessible)
                {
                    SetCurrentLocation(target.id);
                }
            }
            else if (target.isAccessible)
            {
                // Direct tactical simulation jump along cleared corridor spine
                SetCurrentLocation(target.id);
            }
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
            bool isAdjacent = false;
            string dirLetter = "";
            if (!isCurrent && nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr))
            {
                if (curr.connectedIds.Contains(model.id))
                {
                    isAdjacent = true;
                    dirLetter = GetDirectionLetter(curr, model);
                }
            }

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
                    $"<color=#00e5ff><b>PLAYER POS:</b> {playerNode.roomCode} ({playerNode.shortSub})</color>   <color=#475569>|</color>   <color=#f39c12><b>SELECTED:</b> {model.roomCode}</color>";
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
                    "ZERO" or "MINIMAL" => "#2ecc71",
                    "LOW" => "#38bdf8",
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

            // Update Action Prompt & Transit Button
            if (quickNavPromptText != null)
            {
                if (isCurrent)
                {
                    quickNavPromptText.text = "<color=#00e5ff><b>[RADAR LOCK]</b></color> Occupying this sector. Use <b>[W/A/S/D]</b> or click adjacent sectors to move.";
                }
                else if (isAdjacent)
                {
                    if (model.isAccessible)
                        quickNavPromptText.text = $"<color=#2ecc71><b>[DIRECT DOORWAY]</b></color> Press <b>[{dirLetter}]</b>, double-click tile, or click button to enter.";
                    else if (model.isLocked)
                        quickNavPromptText.text = "<color=#f39c12><b>[LOCKED THRESHOLD]</b></color> Heavy deadbolt engaged. Requires key or lockpick.";
                    else
                        quickNavPromptText.text = "<color=#e74c3c><b>[HAZARD BARRICADE]</b></color> Structural blockade or lethal biohazard.";
                }
                else
                {
                    quickNavPromptText.text = $"<color=#94a3b8><b>[REMOTE RECON]</b></color> {model.title} — navigate via Central Spine Corridor.";
                }
            }

            if (transitActionButton != null && transitActionBg != null && transitActionBtnText != null)
            {
                if (isCurrent)
                {
                    if (string.Equals(currentLocationId, "unit_407", StringComparison.OrdinalIgnoreCase))
                    {
                        transitActionButton.interactable = false;
                        transitActionBg.color = new Color(0.06f, 0.14f, 0.18f, 0.95f);
                        transitActionBtnText.text = "<color=#00e5ff><b>[ CURRENTLY OCCUPYING SAFEHOUSE 407 ]</b></color>";
                    }
                    else
                    {
                        transitActionButton.interactable = true;
                        transitActionBg.color = new Color(0.08f, 0.18f, 0.24f, 0.95f);
                        transitActionBtnText.text = "<color=#00e5ff><b>[ << RETURN TO UNIT 407 SAFEHOUSE ]</b></color>";
                    }
                }
                else if (model.isAccessible)
                {
                    transitActionButton.interactable = true;
                    transitActionBg.color = isAdjacent
                        ? new Color(0.10f, 0.24f, 0.16f, 0.98f)
                        : new Color(0.12f, 0.18f, 0.26f, 0.98f);
                    string keyTag = isAdjacent && !string.IsNullOrEmpty(dirLetter) ? $" [{dirLetter}]" : "";
                    transitActionBtnText.text = $"<color=#2ecc71><b>[ >> TRANSIT TO {model.roomCode}{keyTag} ]</b></color>";
                }
                else
                {
                    transitActionButton.interactable = false;
                    transitActionBg.color = new Color(0.18f, 0.08f, 0.08f, 0.95f);
                    string reason = model.isLocked ? "DOOR LOCKED — KEY / LOCKPICK REQUIRED" : "SECTOR IMPASSABLE — BARRICADED";
                    transitActionBtnText.text = $"<color=#e74c3c><b>[ X {reason} ]</b></color>";
                }
            }
        }

        private void TryNavigateDirection(string directionKey)
        {
            if (!nodeRegistry.TryGetValue(currentLocationId, out NodeModel current)) return;

            foreach (string neighborId in current.connectedIds)
            {
                if (!nodeRegistry.TryGetValue(neighborId, out NodeModel neighbor)) continue;
                string dir = GetDirectionLetter(current, neighbor);
                if (string.Equals(dir, directionKey, StringComparison.OrdinalIgnoreCase))
                {
                    AudioManager.Instance?.PlayButtonClick();
                    if (neighbor.isAccessible)
                    {
                        SetCurrentLocation(neighbor.id);
                    }
                    else
                    {
                        SelectNode(neighbor.id);
                    }
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

            bool isAdjacent = false;
            string dirLetter = "";
            if (!isCurrent && nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr))
            {
                if (curr.connectedIds.Contains(holder.model.id))
                {
                    isAdjacent = true;
                    dirLetter = GetDirectionLetter(curr, holder.model);
                }
            }

            // Player Radar Beacon
            if (holder.beaconObj != null)
            {
                holder.beaconObj.SetActive(isCurrent);
            }

            // Directional Hotkey Pill [W], [A], [S], [D]
            if (holder.navBadgeObj != null)
            {
                holder.navBadgeObj.SetActive(isAdjacent && !string.IsNullOrEmpty(dirLetter));
                if (holder.navBadgeTmp != null && !string.IsNullOrEmpty(dirLetter))
                {
                    holder.navBadgeTmp.text = $"<b>{dirLetter}</b>";
                }
            }

            // Procedural Pattern Overlay (Diagonal Hazard Stripes or Stair Treads)
            if (holder.patternOverlay != null)
            {
                if (holder.model.isBlocked || holder.model.isLocked)
                {
                    holder.patternOverlay.gameObject.SetActive(true);
                    holder.patternOverlay.sprite = GetHatchedSprite();
                    holder.patternOverlay.type = Image.Type.Tiled;
                    holder.patternOverlay.color = holder.model.isLocked
                        ? new Color(0.95f, 0.65f, 0.15f, 0.30f)
                        : new Color(0.90f, 0.25f, 0.25f, 0.38f);
                }
                else if (holder.model.isStairs)
                {
                    holder.patternOverlay.gameObject.SetActive(true);
                    holder.patternOverlay.sprite = GetStairTreadSprite();
                    holder.patternOverlay.type = Image.Type.Tiled;
                    holder.patternOverlay.color = Color.white;
                }
                else
                {
                    holder.patternOverlay.gameObject.SetActive(false);
                }
            }

            // Doorway Threshold Tick Color
            if (holder.doorTickImage != null)
            {
                if (isCurrent || (isAdjacent && holder.model.isAccessible))
                    holder.doorTickImage.color = new Color(0.0f, 0.90f, 1.0f, 0.95f); // Glowing Cyan threshold
                else if (holder.model.isAccessible)
                    holder.doorTickImage.color = new Color(0.18f, 0.78f, 0.48f, 0.85f); // Accessible Emerald threshold
                else if (holder.model.isLocked)
                    holder.doorTickImage.color = new Color(0.95f, 0.60f, 0.12f, 0.90f); // Locked Amber threshold
                else
                    holder.doorTickImage.color = new Color(0.88f, 0.24f, 0.24f, 0.90f); // Barricaded Crimson threshold
            }

            // Room Footprint Fill, Wall Stroke & Typography
            if (isCurrent)
            {
                // Player Location: Glowing Phosphor Cyan Blueprint Module
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.04f, 0.22f, 0.30f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.0f, 0.92f, 1.0f, 1.0f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.0f, 0.96f, 1.0f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = Color.white;
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "LOCATED";
                    holder.subLabelTmp.color = new Color(0.0f, 0.92f, 1.0f);
                }
            }
            else if (isSelected)
            {
                // Active Scanner Target: Tactical Amber Selection Reticle
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.16f, 0.14f, 0.08f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.98f, 0.68f, 0.12f, 1.0f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.98f, 0.72f, 0.18f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(1.0f, 0.85f, 0.40f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = Color.white;
                }
            }
            else if (isAdjacent && holder.model.isAccessible)
            {
                // Directly Reachable Sector: Highlighted Emerald-Cyan Blueprint
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.06f, 0.16f, 0.18f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.20f, 0.85f, 0.65f, 0.90f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.25f, 0.92f, 0.68f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.85f, 0.98f, 0.92f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.55f, 0.82f, 0.72f);
                }
            }
            else if (holder.model.isStairs)
            {
                // Fire Stairwell Exit: Tactical Amber-Slate
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.11f, 0.09f, 0.06f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.88f, 0.52f, 0.14f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.95f, 0.60f, 0.18f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.95f, 0.70f, 0.30f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.75f, 0.60f, 0.42f);
                }
            }
            else if (holder.model.isHallway)
            {
                // Spine Corridor Segment: Recessed Blueprint Slate
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.20f, 0.32f, 0.44f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.42f, 0.62f, 0.78f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.68f, 0.82f, 0.94f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.45f, 0.58f, 0.70f);
                }
            }
            else if (holder.model.isLocked)
            {
                // Locked Unit: Amber Security Tint
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.10f, 0.08f, 0.05f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.65f, 0.44f, 0.14f, 0.80f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.92f, 0.65f, 0.20f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.92f, 0.76f, 0.45f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "LOCKED";
                    holder.subLabelTmp.color = new Color(0.75f, 0.55f, 0.25f);
                }
            }
            else if (holder.model.isBlocked)
            {
                // Barricaded / Biohazard / Collapsed: Crimson Alert Tint (RE2 Danger Style)
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.14f, 0.05f, 0.06f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.72f, 0.20f, 0.22f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.92f, 0.30f, 0.30f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.95f, 0.48f, 0.48f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.75f, 0.36f, 0.36f);
                }
            }
            else
            {
                // Accessible / Cleared Residential Unit: Blueprint Steel-Emerald
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.06f, 0.11f, 0.16f, 0.96f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.22f, 0.44f, 0.56f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconImage != null) holder.iconImage.color = new Color(0.35f, 0.82f, 0.65f);
                if (holder.roomCodeTmp != null) holder.roomCodeTmp.color = new Color(0.82f, 0.92f, 0.98f);
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.shortSub;
                    holder.subLabelTmp.color = new Color(0.52f, 0.68f, 0.78f);
                }
            }
        }
    }
}
