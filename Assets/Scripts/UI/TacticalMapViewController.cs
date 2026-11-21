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
    /// Architectural Blueprint & Bio-Radar Scanner for the Survivor PDA.
    /// Provides an informational floorplan inspection viewer with high-contrast typography,
    /// dark obsidian blueprint styling, color-coded doorway thresholds, and recon telemetry.
    /// Does NOT move the player (pure reference tool; player movement is driven by narrative gameplay).
    /// </summary>
    public class TacticalMapViewController : MonoBehaviour
    {
        public static bool StyleDThemeEnabled { get; set; }

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
            public bool isVisited;
            public bool isChecked;
            public bool isStairs;
            public bool isHallway;
            public bool isBlocked;
            public bool isLocked;
            public TacticalRoomIconType iconType;
            public int row;
            public int col;
            public string undiscoveredSub;
            public string undiscoveredTitle;
            public List<string> connectedIds = new List<string>();

            public NodeModel(string id, string roomCode, string shortSub, string title, string statusTag,
                             string threatLevel, string scavengeStatus, string acousticScan, string reconIntel,
                             bool isAccessible, bool isDiscovered, int row, int col, TacticalRoomIconType iconType,
                             bool isStairs = false, bool isHallway = false,
                             bool isBlocked = false, bool isLocked = false,
                             string undiscoveredSub = null, string undiscoveredTitle = null)
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
                this.isVisited = false;
                this.isChecked = false;
                this.row = row;
                this.col = col;
                this.iconType = iconType;
                this.isStairs = isStairs;
                this.isHallway = isHallway;
                this.isBlocked = isBlocked;
                this.isLocked = isLocked;
                this.undiscoveredSub = !string.IsNullOrEmpty(undiscoveredSub) ? undiscoveredSub : (isHallway ? shortSub : (isStairs ? "FIRE EXIT" : (id.Contains("util") ? "SERVICE ROOM" : "RESIDENTIAL")));
                this.undiscoveredTitle = !string.IsNullOrEmpty(undiscoveredTitle) ? undiscoveredTitle : (isHallway ? "CORRIDOR JUNCTION" : (isStairs ? "WEST FIRE STAIRWELL" : (id.Contains("util") ? "UTILITY SERVICE BAY" : "RESIDENTIAL UNIT")));
            }
        }

        private class RuntimeRoomState
        {
            public bool isVisited;
            public bool isChecked;
            public bool isDiscovered;
            public bool isLocked;
            public bool isBlocked;
            public bool isAccessible;
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

        [System.Serializable]
        public class DoorwayUIHolder
        {
            public string doorId;
            public string roomA;
            public string roomB;
            public Vector2 position;
            public Vector2 size;
            public GameObject rootObj;
            public Image lineImg;
            public Image glowImg;
            public bool isGlowing;
        }

        [Header("Runtime State")]
        [SerializeField] private string currentLocationId = "unit_407";
        [SerializeField] private string currentThresholdRoomId = null;
        [SerializeField] private string selectedNodeId = "unit_407";
        private readonly List<DoorwayUIHolder> doorwayHolders = new List<DoorwayUIHolder>();

        [Header("Modular Floor Data (Optional ScriptableObject)")]
        [SerializeField] private TacticalFloorMapData currentFloorData;
        public TacticalFloorMapData CurrentFloorData => currentFloorData;

        [Header("Vector Graphics Floorplan (SVG)")]
        [SerializeField] private Image vectorMapBackdropImage;

        [Header("Tactical Blueprint Zoom & Pan")]
        [SerializeField] private RectTransform floorplanCanvasRect;
        [SerializeField] private RectTransform blueprintViewportRect;
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button zoomResetButton;
        [SerializeField] private TextMeshProUGUI zoomLevelTmp;
        [SerializeField] private bool isHorizontalMiniMap = false;
        [SerializeField] private float miniMapBaseScale = 1.08f;

        private float currentZoom = 1.0f;
        private const float MinZoom = 0.8f;
        private const float MaxZoom = 3.0f;
        private const float ZoomStep = 0.25f;

        private Vector2 canvasBasePosition = new Vector2(0f, 2f);
        private Vector2 panOffset = Vector2.zero;
        private bool isDraggingPan = false;
        private Vector2 lastMouseScreenPos;
        private Vector2 leftMouseDownPos;
        private bool leftDragThresholdPassed = false;

        [Header("Serialized Blueprint Nodes & Grid")]
        [SerializeField] private Image blueprintGridOverlayImage;
        [SerializeField] private List<NodeUIHolder> serializedHolders = new List<NodeUIHolder>();

        [Header("Blueprint Header & Telemetry Banners")]
        [SerializeField] private TextMeshProUGUI blueprintHeaderTitleTmp;
        [SerializeField] private TextMeshProUGUI blueprintHeaderSubtitleTmp;
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
        private readonly Dictionary<string, RuntimeRoomState> persistentRoomStates = new Dictionary<string, RuntimeRoomState>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NodeUIHolder> uiHolders = new Dictionary<string, NodeUIHolder>(StringComparer.OrdinalIgnoreCase);
        private readonly List<GameObject> dynamicRoomHitboxes = new List<GameObject>();
        private TacticalFloorMapData _lastSyncedFloorData;

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

        private static Sprite cachedHazardStripeSprite;
        private static Sprite cachedRedStripeSprite;
        private static Sprite cachedExitSignSprite;
        private static Sprite cachedStairsStripeSprite;

        // Emergency exit running person sign from the authentic Twine map SVG
        public static Sprite GetExitSignSprite()
        {
            if (cachedExitSignSprite != null) return cachedExitSignSprite;
#if UNITY_EDITOR
            cachedExitSignSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/Map_Exit_Icon.png");
#endif
            if (cachedExitSignSprite == null)
            {
                cachedExitSignSprite = Resources.Load<Sprite>("Maps/Map_Exit_Icon");
            }
            return cachedExitSignSprite;
        }

        // Same visual language as the SVG floorplan: amber hazard stripes for blocked/inaccessible rooms
        public static Sprite GetHazardStripeSprite()
        {
            if (cachedHazardStripeSprite != null) return cachedHazardStripeSprite;
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            Color amber = new Color(0.96f, 0.62f, 0.04f, 1f);
            Color dark = new Color(0.09f, 0.09f, 0.11f, 1f);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, (((x + y) / 5) % 2 == 0) ? amber : dark);
            tex.Apply();
            cachedHazardStripeSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return cachedHazardStripeSprite;
        }

        // Red diagonal stripes for accessible but locked rooms
        public static Sprite GetRedStripeSprite()
        {
            if (cachedRedStripeSprite != null) return cachedRedStripeSprite;
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            Color red = new Color(0.92f, 0.18f, 0.22f, 1f);
            Color dark = new Color(0.09f, 0.09f, 0.11f, 1f);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, (((x + y) / 5) % 2 == 0) ? red : dark);
            tex.Apply();
            cachedRedStripeSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return cachedRedStripeSprite;
        }

        // Green stairwell lines (SVG pattern_stairs)
        public static Sprite GetStairsStripeSprite()
        {
            if (cachedStairsStripeSprite != null) return cachedStairsStripeSprite;
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            Color green = new Color(0.29f, 0.87f, 0.50f, 1f);
            Color dark = new Color(0.03f, 0.04f, 0.06f, 1f);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, (y % 5 < 2) ? green : dark);
            tex.Apply();
            cachedStairsStripeSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return cachedStairsStripeSprite;
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
            SelectNode(string.IsNullOrEmpty(currentLocationId) ? "unit_407" : currentLocationId);
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
                    if (!nodeRegistry.TryGetValue(holder.nodeId, out NodeModel model))
                    {
                        string norm = NormalizeLocationId(holder.nodeId);
                        if (!nodeRegistry.TryGetValue(norm, out model)) continue;
                    }

                    holder.model = model;
                    if (holder.iconImage != null)
                    {
                        holder.iconImage.gameObject.SetActive(false);
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

            if (zoomInButton != null)
            {
                zoomInButton.onClick.RemoveAllListeners();
                zoomInButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    ZoomIn();
                });
            }
            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.RemoveAllListeners();
                zoomOutButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    ZoomOut();
                });
            }
            if (zoomResetButton != null)
            {
                zoomResetButton.onClick.RemoveAllListeners();
                zoomResetButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    ResetZoom();
                });
            }

            if (floorplanCanvasRect != null && canvasBasePosition == Vector2.zero)
            {
                canvasBasePosition = floorplanCanvasRect.anchoredPosition;
            }

            if (vectorMapBackdropImage == null && floorplanCanvasRect != null)
            {
                Transform vmb = floorplanCanvasRect.Find("VectorMapBackdrop");
                if (vmb != null) vectorMapBackdropImage = vmb.GetComponent<Image>();
            }

            if (currentFloorData == null)
            {
                currentFloorData = Resources.Load<TacticalFloorMapData>("Maps/FloorMap_Ascott_4F");
            }

            if (currentFloorData != null)
            {
                if (blueprintHeaderTitleTmp != null && !string.IsNullOrEmpty(currentFloorData.floorTitle))
                    blueprintHeaderTitleTmp.text = currentFloorData.floorTitle;
                if (blueprintHeaderSubtitleTmp != null && !string.IsNullOrEmpty(currentFloorData.floorSubtitle))
                    blueprintHeaderSubtitleTmp.text = currentFloorData.floorSubtitle;
                if (blueprintCoordTmp != null && !string.IsNullOrEmpty(currentFloorData.gridCoordinate))
                    blueprintCoordTmp.text = currentFloorData.gridCoordinate;
            }

            if (_lastSyncedFloorData != currentFloorData || uiHolders.Count == 0)
            {
                SyncRoomHitboxes();
                UpdateVectorMapDisplay();
            }
        }

        public void BindVectorMapBackdrop(Image backdropImg)
        {
            vectorMapBackdropImage = backdropImg;
            UpdateVectorMapDisplay();
        }

        public void UpdateVectorMapDisplay()
        {
            if (vectorMapBackdropImage == null) return;

            Sprite mapSprite = currentFloorData != null ? currentFloorData.vectorMapSprite : null;
            if (mapSprite != null)
            {
                vectorMapBackdropImage.sprite = mapSprite;
                vectorMapBackdropImage.preserveAspect = true;
                vectorMapBackdropImage.gameObject.SetActive(true);

                Transform parent = vectorMapBackdropImage.transform.parent;
                if (parent != null)
                {
                    Transform sp = parent.Find("StructuralPerimeter");
                    if (sp != null) sp.gameObject.SetActive(false);

                    Transform cs = parent.Find("CentralCorridorShaft");
                    if (cs != null) cs.gameObject.SetActive(false);
                }

                bool isAscott4F = currentFloorData == null || currentFloorData.floorId == "ascott_4f";

                foreach (var holder in serializedHolders)
                {
                    if (holder == null || holder.rootObj == null) continue;

                    if (!isAscott4F)
                    {
                        holder.rootObj.SetActive(false);
                        continue;
                    }

                    // Check if this holder's node is actually used in current floor
                    bool isRoomInFloor = false;
                    if (currentFloorData != null && currentFloorData.rooms != null)
                    {
                        for (int rIdx = 0; rIdx < currentFloorData.rooms.Count; rIdx++)
                        {
                            var rm = currentFloorData.rooms[rIdx];
                            if (rm == null) continue;
                            if (string.Equals(rm.roomId, holder.nodeId, StringComparison.OrdinalIgnoreCase)
                                || (rm.roomId == "blocked_stairs" && holder.nodeId == "empty_bot")
                                || (rm.roomId == "unit_406" && holder.nodeId == "unit_406")
                                || (rm.roomId == "unit_404" && holder.nodeId == "unit_404")
                                || (rm.roomId == "unit_402" && holder.nodeId == "unit_402"))
                            {
                                isRoomInFloor = true;
                                break;
                            }
                        }
                    }

                    if (currentFloorData != null && !isRoomInFloor)
                    {
                        holder.rootObj.SetActive(false);
                        continue;
                    }

                    holder.rootObj.SetActive(true);

                    if (holder.bgImage != null)
                    {
                        holder.bgImage.color = new Color(0f, 0f, 0f, 0f);
                    }
                    if (holder.roomCodeTmp != null) holder.roomCodeTmp.gameObject.SetActive(false);
                    if (holder.subLabelTmp != null) holder.subLabelTmp.gameObject.SetActive(false);
                    if (holder.iconImage != null) holder.iconImage.gameObject.SetActive(false);
                    if (holder.navBadgeObj != null) holder.navBadgeObj.SetActive(false);
                    if (holder.patternOverlay != null) holder.patternOverlay.gameObject.SetActive(false);
                    if (holder.beaconObj != null) holder.beaconObj.SetActive(false);
                    if (holder.doorTickImage != null)
                    {
                        Transform dtParent = holder.doorTickImage.transform.parent;
                        if (dtParent != null) dtParent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // If no vector sprite assigned on floor data, keep backdrop image hidden
                vectorMapBackdropImage.gameObject.SetActive(false);

                bool isAscott4F = currentFloorData == null || string.Equals(currentFloorData.floorId, "ascott_4f", StringComparison.OrdinalIgnoreCase);
                if (!isAscott4F)
                {
                    Transform parent = vectorMapBackdropImage.transform.parent;
                    if (parent != null)
                    {
                        Transform sp = parent.Find("StructuralPerimeter");
                        if (sp != null) sp.gameObject.SetActive(false);

                        Transform cs = parent.Find("CentralCorridorShaft");
                        if (cs != null) cs.gameObject.SetActive(false);
                    }

                    foreach (var holder in serializedHolders)
                    {
                        if (holder != null && holder.rootObj != null)
                        {
                            holder.rootObj.SetActive(false);
                        }
                    }
                }
            }

            UpdateHorizontalNodeLabels();
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            // Pulse active glowing doorway connecting lines
            if (doorwayHolders != null && doorwayHolders.Count > 0)
            {
                bool showStaticDoors = currentFloorData != null && currentFloorData.vectorMapSprite == null;
                float pulse = Mathf.PingPong(Time.unscaledTime * 3.5f, 0.40f) + 0.60f;
                for (int i = 0; i < doorwayHolders.Count; i++)
                {
                    var d = doorwayHolders[i];
                    if (d == null) continue;

                    if (d.isGlowing)
                    {
                        if (d.rootObj != null && !d.rootObj.activeSelf) d.rootObj.SetActive(true);
                        if (d.lineImg != null) d.lineImg.color = StyleDThemeEnabled ? new Color(0.95f, 0.56f, 0.22f, 1f) : new Color(0.0f, 0.95f, 1.0f, 1.0f);
                        if (d.glowImg != null)
                        {
                            if (!d.glowImg.gameObject.activeSelf) d.glowImg.gameObject.SetActive(true);
                            d.glowImg.color = StyleDThemeEnabled
                                ? new Color(0.90f, 0.44f, 0.16f, pulse * 0.45f)
                                : new Color(0.0f, 0.90f, 1.0f, pulse * 0.45f);
                        }
                    }
                    else if (showStaticDoors)
                    {
                        if (d.rootObj != null && !d.rootObj.activeSelf) d.rootObj.SetActive(true);
                        if (d.lineImg != null) d.lineImg.color = StyleDThemeEnabled ? new Color(0.45f, 0.40f, 0.34f, 0.90f) : new Color(0.29f, 0.40f, 0.52f, 0.90f);
                        if (d.glowImg != null && d.glowImg.gameObject.activeSelf) d.glowImg.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Clean Blueprint: Hide line overlay completely when idle (zero redundant dark rectangles)
                        if (d.rootObj != null && d.rootObj.activeSelf) d.rootObj.SetActive(false);
                    }
                }
            }

            // Pulse red '!' crisis alerts on 1F Lobby map (matching Twine .map-alert-icon)
            if (IsLobbyMap() && uiHolders != null)
            {
                float alertAlpha = Mathf.PingPong(Time.unscaledTime * 3.0f, 0.55f) + 0.45f;
                foreach (var kvp in uiHolders)
                {
                    var h = kvp.Value;
                    if (h == null || h.roomCodeTmp == null || !h.roomCodeTmp.gameObject.activeSelf) continue;
                    if (string.Equals(h.roomCodeTmp.text, "!", StringComparison.Ordinal))
                    {
                        Color c = h.roomCodeTmp.color;
                        bool isCur = string.Equals(h.nodeId, currentLocationId, StringComparison.OrdinalIgnoreCase);
                        c.a = isCur ? 1.0f : alertAlpha;
                        h.roomCodeTmp.color = c;
                    }
                }
            }

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

            HandlePanZoomInput();
        }

        public void BindPanZoomControls(RectTransform canvasRect, RectTransform viewportRect,
                                        Button inBtn, Button outBtn, Button resetBtn, TextMeshProUGUI zoomTmp)
        {
            floorplanCanvasRect = canvasRect;
            blueprintViewportRect = viewportRect;
            zoomInButton = inBtn;
            zoomOutButton = outBtn;
            zoomResetButton = resetBtn;
            zoomLevelTmp = zoomTmp;

            if (floorplanCanvasRect != null)
            {
                canvasBasePosition = floorplanCanvasRect.anchoredPosition;
            }

            if (zoomInButton != null)
            {
                zoomInButton.onClick.RemoveAllListeners();
                zoomInButton.onClick.AddListener(ZoomIn);
            }
            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.RemoveAllListeners();
                zoomOutButton.onClick.AddListener(ZoomOut);
            }
            if (zoomResetButton != null)
            {
                zoomResetButton.onClick.RemoveAllListeners();
                zoomResetButton.onClick.AddListener(ResetZoom);
            }

            ApplyZoomAndPan();
            UpdateZoomReadout();
        }

        public void ZoomIn()
        {
            SetZoom(currentZoom + ZoomStep);
        }

        public void ZoomOut()
        {
            SetZoom(currentZoom - ZoomStep);
        }

        public void ResetZoom()
        {
            panOffset = Vector2.zero;
            SetZoom(1.0f);
        }

        public void OnDragPan(Vector2 screenDelta)
        {
            if (floorplanCanvasRect == null || currentZoom <= 1.001f) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            float scaleFactor = canvas != null ? canvas.scaleFactor : 1.0f;
            if (scaleFactor <= 0.001f) scaleFactor = 1.0f;

            panOffset += screenDelta / scaleFactor;
            ApplyZoomAndPan();
        }

        public void SetZoom(float targetZoom)
        {
            currentZoom = Mathf.Clamp(targetZoom, MinZoom, MaxZoom);
            if (currentZoom <= 1.001f)
            {
                panOffset = Vector2.zero;
            }
            ApplyZoomAndPan();
            UpdateZoomReadout();
        }

        public void SetHorizontalMiniMapMode(bool horizontal, float baseScale = 1.08f)
        {
            isHorizontalMiniMap = horizontal;
            miniMapBaseScale = baseScale;
            EnsureRuntimeBindings();
            if (isHorizontalMiniMap)
            {
                canvasBasePosition = Vector2.zero;
                panOffset = Vector2.zero;
            }
            ApplyZoomAndPan();
            UpdateHorizontalNodeLabels();
        }

        private bool IsLobbyMap()
        {
            return currentFloorData != null
                && string.Equals(currentFloorData.floorId, "ascott_lobby", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsStairwellMap()
        {
            return currentFloorData != null
                && (string.Equals(currentFloorData.floorId, "stairwell_4f", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentFloorData.floorId, "stairwell_2f", StringComparison.OrdinalIgnoreCase));
        }

        private bool ShouldRotateFloorplanForMiniMap()
        {
            if (!isHorizontalMiniMap) return false;
            return currentFloorData == null
                || string.Equals(currentFloorData.floorId, "ascott_4f", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyZoomAndPan()
        {
            if (floorplanCanvasRect == null) return;

            bool rotate90 = ShouldRotateFloorplanForMiniMap();
            float floorFit = 1.0f;
            if (isHorizontalMiniMap)
            {
                if (IsLobbyMap()) floorFit = 0.68f;
                else if (IsStairwellMap()) floorFit = 0.66f;
            }
            float effectiveScale = (isHorizontalMiniMap ? miniMapBaseScale * floorFit : 1.0f) * currentZoom;
            floorplanCanvasRect.localScale = Vector3.one * effectiveScale;
            floorplanCanvasRect.localRotation = rotate90 ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;

            if (currentZoom > 1.001f)
            {
                float halfWidth = floorplanCanvasRect.rect.width * 0.5f;
                float halfHeight = floorplanCanvasRect.rect.height * 0.5f;
                float maxPanX = halfWidth * (currentZoom - 1f) + 40f;
                float maxPanY = halfHeight * (currentZoom - 1f) + 40f;

                panOffset.x = Mathf.Clamp(panOffset.x, -maxPanX, maxPanX);
                panOffset.y = Mathf.Clamp(panOffset.y, -maxPanY, maxPanY);
            }
            else
            {
                panOffset = Vector2.zero;
            }

            Vector2 basePos = isHorizontalMiniMap ? Vector2.zero : canvasBasePosition;
            floorplanCanvasRect.anchoredPosition = basePos + panOffset;
        }

        private void UpdateZoomReadout()
        {
            if (zoomLevelTmp != null)
            {
                zoomLevelTmp.text = $"{Mathf.RoundToInt(currentZoom * 100f)}%";
            }
        }

        private void HandlePanZoomInput()
        {
            if (floorplanCanvasRect == null) return;

            Vector2 mousePos = Vector2.zero;
            float scrollDelta = 0f;
            bool rightHeld = false;
            bool middleHeld = false;
            bool leftHeld = false;
            bool rightDown = false;
            bool middleDown = false;
            bool leftDown = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
                scrollDelta = Mouse.current.scroll.ReadValue().y;
                rightHeld = Mouse.current.rightButton.isPressed;
                middleHeld = Mouse.current.middleButton.isPressed;
                leftHeld = Mouse.current.leftButton.isPressed;
                rightDown = Mouse.current.rightButton.wasPressedThisFrame;
                middleDown = Mouse.current.middleButton.wasPressedThisFrame;
                leftDown = Mouse.current.leftButton.wasPressedThisFrame;
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                    ZoomIn();
                if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                    ZoomOut();
                if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame)
                    ResetZoom();
            }
#else
            mousePos = Input.mousePosition;
            scrollDelta = Input.mouseScrollDelta.y;
            rightHeld = Input.GetMouseButton(1);
            middleHeld = Input.GetMouseButton(2);
            leftHeld = Input.GetMouseButton(0);
            rightDown = Input.GetMouseButtonDown(1);
            middleDown = Input.GetMouseButtonDown(2);
            leftDown = Input.GetMouseButtonDown(0);

            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) ZoomIn();
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) ZoomOut();
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.R)) ResetZoom();
#endif

            // Check if mouse is hovering over blueprint area
            bool isOverViewport = false;
            if (blueprintViewportRect != null)
            {
                isOverViewport = RectTransformUtility.RectangleContainsScreenPoint(blueprintViewportRect, mousePos);
            }

            // Scroll zoom
            if (isOverViewport && Mathf.Abs(scrollDelta) > 0.05f)
            {
                if (scrollDelta > 0f) ZoomIn();
                else ZoomOut();
            }

            // Drag to pan: Right-click, Middle-click, or Left-drag (when zoomed)
            bool startDrag = isOverViewport && (rightDown || middleDown);
            bool isDraggingActive = rightHeld || middleHeld;

            // Handle left click drag when zoomed in
            if (isOverViewport && leftDown && currentZoom > 1.0f)
            {
                leftMouseDownPos = mousePos;
                leftDragThresholdPassed = false;
            }
            if (leftHeld && currentZoom > 1.0f && !leftDragThresholdPassed)
            {
                if (Vector2.Distance(mousePos, leftMouseDownPos) > 6f)
                {
                    leftDragThresholdPassed = true;
                    isDraggingPan = true;
                    lastMouseScreenPos = mousePos;
                }
            }

            if (startDrag)
            {
                isDraggingPan = true;
                lastMouseScreenPos = mousePos;
            }
            else if (!isDraggingActive && (!leftHeld || !leftDragThresholdPassed))
            {
                isDraggingPan = false;
                leftDragThresholdPassed = false;
            }

            if (isDraggingPan && currentZoom > 1.0f)
            {
                Vector2 delta = mousePos - lastMouseScreenPos;
                panOffset += delta;
                ApplyZoomAndPan();
                lastMouseScreenPos = mousePos;
            }
        }

        // =========================================================================
        // ARCHITECTURAL DATA MODEL (Modular ScriptableObject with 4F Defaults)
        // =========================================================================

        public void LoadFloorById(string floorId)
        {
            if (string.IsNullOrEmpty(floorId)) return;
            string cleanId = floorId.ToLowerInvariant().Trim().Replace(" ", "_").Replace("-", "_");

            if (currentFloorData != null && string.Equals(currentFloorData.floorId, cleanId, StringComparison.OrdinalIgnoreCase))
            {
                return; // Already loaded
            }

            string resourcePath = "Maps/FloorMap_Ascott_4F";
            if (cleanId == "ascott_4f" || cleanId == "4f" || cleanId == "floor4" || cleanId == "4th_floor")
                resourcePath = "Maps/FloorMap_Ascott_4F";
            else if (cleanId == "stairwell_4f" || cleanId == "stairs_4f" || cleanId == "stairwell")
                resourcePath = "Maps/FloorMap_Stairwell_4F";
            else if (cleanId == "stairwell_2f" || cleanId == "stairs_2f")
                resourcePath = "Maps/FloorMap_Stairwell_2F";
            else if (cleanId == "ascott_2f" || cleanId == "2f" || cleanId == "2f_hallway" || cleanId == "floor2")
                resourcePath = "Maps/FloorMap_Ascott_2F";
            else if (cleanId == "ascott_lobby" || cleanId == "lobby")
                resourcePath = "Maps/FloorMap_Ascott_Lobby";
            else if (cleanId.Contains("stairwell_4"))
                resourcePath = "Maps/FloorMap_Stairwell_4F";
            else if (cleanId.Contains("stairwell_2"))
                resourcePath = "Maps/FloorMap_Stairwell_2F";
            else
                resourcePath = "Maps/FloorMap_" + cleanId;

            TacticalFloorMapData data = Resources.Load<TacticalFloorMapData>(resourcePath);
            if (data != null)
            {
                LoadFloor(data);
            }
            else
            {
                Debug.LogWarning($"[TacticalMap] Could not find FloorMap asset at Resources/{resourcePath}");
            }
        }

        private void SaveNodeToPersistentState(NodeModel m)
        {
            if (m == null || string.IsNullOrEmpty(m.id)) return;
            string key = NormalizeLocationId(m.id);
            if (!persistentRoomStates.TryGetValue(key, out var st))
            {
                st = new RuntimeRoomState();
                persistentRoomStates[key] = st;
            }
            st.isVisited = m.isVisited;
            st.isChecked = m.isChecked;
            st.isDiscovered = m.isDiscovered;
            st.isLocked = m.isLocked;
            st.isBlocked = m.isBlocked;
            st.isAccessible = m.isAccessible;
        }

        private void ApplyPersistentStateIfPresent(NodeModel node)
        {
            if (node == null || string.IsNullOrEmpty(node.id)) return;
            string key = NormalizeLocationId(node.id);
            if (persistentRoomStates.TryGetValue(key, out var saved))
            {
                node.isVisited = saved.isVisited;
                node.isChecked = saved.isChecked;
                node.isDiscovered = saved.isDiscovered;
                node.isLocked = saved.isLocked;
                node.isBlocked = saved.isBlocked;
                node.isAccessible = saved.isAccessible;
            }
        }

        public void ResetAllRuntimeMapStates()
        {
            persistentRoomStates.Clear();
            nodeRegistry.Clear();
            InitializeDataModel();
            RefreshAllNodes();
        }

        public void SyncRoomState(string roomId, bool isVisited, bool isChecked, bool isLocked, bool isBlocked, bool isAccessible)
        {
            if (string.IsNullOrEmpty(roomId)) return;
            EnsureRuntimeBindings();
            string normId = NormalizeLocationId(roomId);

            if (!persistentRoomStates.TryGetValue(normId, out var st))
            {
                st = new RuntimeRoomState();
                persistentRoomStates[normId] = st;
            }
            bool alreadyDiscovered = st.isDiscovered
                || ((nodeRegistry.TryGetValue(normId, out NodeModel preModel) || nodeRegistry.TryGetValue(roomId, out preModel)) && preModel.isDiscovered);
            st.isVisited = isVisited;
            st.isChecked = isChecked || isVisited;
            st.isDiscovered = alreadyDiscovered || isChecked || isVisited;
            st.isLocked = isLocked;
            st.isBlocked = isBlocked;
            st.isAccessible = isAccessible;

            if (nodeRegistry.TryGetValue(normId, out NodeModel model) || nodeRegistry.TryGetValue(roomId, out model))
            {
                model.isVisited = st.isVisited;
                model.isChecked = st.isChecked;
                model.isDiscovered = st.isDiscovered;
                model.isLocked = st.isLocked;
                model.isBlocked = st.isBlocked;
                model.isAccessible = st.isAccessible;

                if (TryGetUIHolder(normId, out NodeUIHolder holder) || TryGetUIHolder(roomId, out holder))
                {
                    ApplyVisualState(holder);
                    UpdateHorizontalNodeLabels();
                }
            }
        }

        public void LoadFloor(TacticalFloorMapData floorData)
        {
            if (floorData == null) return;

            foreach (var kvp in nodeRegistry)
            {
                SaveNodeToPersistentState(kvp.Value);
            }

            currentFloorData = floorData;

            if (blueprintHeaderTitleTmp != null && !string.IsNullOrEmpty(floorData.floorTitle))
            {
                blueprintHeaderTitleTmp.text = floorData.floorTitle;
            }
            if (blueprintHeaderSubtitleTmp != null && !string.IsNullOrEmpty(floorData.floorSubtitle))
            {
                blueprintHeaderSubtitleTmp.text = floorData.floorSubtitle;
            }
            if (blueprintCoordTmp != null && !string.IsNullOrEmpty(floorData.gridCoordinate))
            {
                blueprintCoordTmp.text = floorData.gridCoordinate;
            }

            nodeRegistry.Clear();
            InitializeDataModel();
            SyncRoomHitboxes();
            UpdateVectorMapDisplay();
            RefreshAllNodes();
            SelectNode(currentLocationId);
            ApplyZoomAndPan();
        }

        public void SyncRoomHitboxes()
        {
            // Clean up previously spawned dynamic hitboxes
            if (dynamicRoomHitboxes != null && dynamicRoomHitboxes.Count > 0)
            {
                foreach (var go in dynamicRoomHitboxes)
                {
                    if (go != null)
                    {
                        if (Application.isPlaying)
                        {
                            go.transform.SetParent(null, false);
                            Destroy(go);
                        }
                        else DestroyImmediate(go);
                    }
                }
                dynamicRoomHitboxes.Clear();
            }

            if (floorplanCanvasRect != null)
            {
                for (int i = floorplanCanvasRect.childCount - 1; i >= 0; i--)
                {
                    Transform child = floorplanCanvasRect.GetChild(i);
                    if (child != null && child.name.StartsWith("DynamicHitbox_", StringComparison.Ordinal))
                    {
                        if (Application.isPlaying)
                        {
                            child.SetParent(null, false);
                            Destroy(child.gameObject);
                        }
                        else DestroyImmediate(child.gameObject);
                    }
                }
            }

            // Remove any stale/destroyed entries from uiHolders
            var staleKeys = new List<string>();
            foreach (var kvp in uiHolders)
            {
                if (kvp.Value == null || kvp.Value.rootObj == null)
                {
                    staleKeys.Add(kvp.Key);
                }
            }
            foreach (var k in staleKeys)
            {
                uiHolders.Remove(k);
            }

            if (currentFloorData == null || currentFloorData.rooms == null || currentFloorData.rooms.Count == 0)
            {
                _lastSyncedFloorData = currentFloorData;
                return;
            }

            _lastSyncedFloorData = currentFloorData;

            bool isAscott4F = string.Equals(currentFloorData.floorId, "ascott_4f", StringComparison.OrdinalIgnoreCase);

            if (floorplanCanvasRect == null) return;

            foreach (var r in currentFloorData.rooms)
            {
                if (r == null || string.IsNullOrEmpty(r.roomId)) continue;

                // On Ascott 4F, if serializedHolders already binds this room, wire it, snap transform to vector map coords, and continue
                string lookupId = r.roomId;
                if (!uiHolders.ContainsKey(lookupId))
                {
                    if (lookupId == "blocked_stairs" && uiHolders.ContainsKey("empty_bot")) lookupId = "empty_bot";
                }

                if (isAscott4F && uiHolders.TryGetValue(lookupId, out var existingHolder) && existingHolder != null && existingHolder.rootObj != null)
                {
                    if (nodeRegistry.TryGetValue(r.roomId, out NodeModel m))
                    {
                        existingHolder.model = m;
                    }
                    RectTransform srt = existingHolder.rootObj.GetComponent<RectTransform>();
                    if (srt != null)
                    {
                        srt.anchoredPosition = r.position;
                        srt.sizeDelta = (r.size.x > 5f && r.size.y > 5f) ? r.size : new Vector2(35.1f, 35.1f);
                    }
                    continue;
                }

                // Dynamically instantiate clickable node hitbox
                GameObject nodeObj = new GameObject("DynamicHitbox_" + r.roomId, typeof(RectTransform));
                nodeObj.transform.SetParent(floorplanCanvasRect, false);

                RectTransform rt = nodeObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = r.position;
                rt.sizeDelta = (r.size.x > 5f && r.size.y > 5f) ? r.size : new Vector2(80f, 80f);

                Image bg = nodeObj.AddComponent<Image>();
                bg.color = new Color(0.02f, 0.035f, 0.05f, 1f);
                bg.raycastTarget = true;

                Outline ol = nodeObj.AddComponent<Outline>();
                ol.effectColor = new Color(0.20f, 0.34f, 0.48f, 0.85f);
                ol.effectDistance = new Vector2(1.5f, -1.5f);
                ol.enabled = false;

                Button btn = nodeObj.AddComponent<Button>();
                btn.targetGraphic = bg;
                string capturedId = r.roomId;
                btn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    OnNodeClicked(capturedId);
                });

                // Child Room Code / Label Text
                GameObject textObj = new GameObject("RoomLabel", typeof(RectTransform));
                textObj.transform.SetParent(nodeObj.transform, false);
                RectTransform trt = textObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = Vector2.zero;
                trt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                string shortLabel = GetMiniMapRoomLabel(r.roomId, null);
                tmp.text = !string.IsNullOrEmpty(shortLabel) ? shortLabel : (!string.IsNullOrEmpty(r.roomCode) ? r.roomCode : r.roomId.ToUpper());
                tmp.fontSize = 12f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.raycastTarget = false;
                SundayTypography.ApplyFont(tmp, FontRole.Mono);

                // Child Pattern Overlay for Hazard / Blocked
                GameObject patObj = new GameObject("PatternOverlay", typeof(RectTransform));
                patObj.transform.SetParent(nodeObj.transform, false);
                RectTransform prt = patObj.GetComponent<RectTransform>();
                prt.anchorMin = Vector2.zero;
                prt.anchorMax = Vector2.one;
                prt.offsetMin = Vector2.zero;
                prt.offsetMax = Vector2.zero;
                Image patImg = patObj.AddComponent<Image>();
                patImg.raycastTarget = false;
                patObj.SetActive(false);

                nodeRegistry.TryGetValue(r.roomId, out NodeModel model);

                var dynamicHolder = new NodeUIHolder
                {
                    nodeId = r.roomId,
                    model = model,
                    rootObj = nodeObj,
                    bgImage = bg,
                    outline = ol,
                    roomCodeTmp = tmp,
                    patternOverlay = patImg,
                    button = btn
                };

                uiHolders[r.roomId] = dynamicHolder;
                dynamicRoomHitboxes.Add(nodeObj);
                ApplyVisualState(dynamicHolder);
            }

            InitializeDoorwayLines();
            UpdateDoorwayVisuals();
        }

        public void InitializeDoorwayLines()
        {
            if (doorwayHolders != null && doorwayHolders.Count > 0)
            {
                foreach (var d in doorwayHolders)
                {
                    if (d != null && d.rootObj != null)
                    {
                        if (Application.isPlaying)
                        {
                            d.rootObj.transform.SetParent(null, false);
                            Destroy(d.rootObj);
                        }
                        else DestroyImmediate(d.rootObj);
                    }
                }
                doorwayHolders.Clear();
            }

            if (floorplanCanvasRect != null)
            {
                for (int i = floorplanCanvasRect.childCount - 1; i >= 0; i--)
                {
                    Transform child = floorplanCanvasRect.GetChild(i);
                    if (child != null && child.name.StartsWith("DoorwayLine_", StringComparison.Ordinal))
                    {
                        if (Application.isPlaying)
                        {
                            child.SetParent(null, false);
                            Destroy(child.gameObject);
                        }
                        else DestroyImmediate(child.gameObject);
                    }
                }
            }

            if (currentFloorData == null || currentFloorData.doorways == null || floorplanCanvasRect == null)
            {
                return;
            }

            foreach (var dt in currentFloorData.doorways)
            {
                if (dt == null || string.IsNullOrEmpty(dt.doorId)) continue;

                GameObject doorObj = new GameObject("DoorwayLine_" + dt.doorId, typeof(RectTransform));
                doorObj.transform.SetParent(floorplanCanvasRect, false);

                if (vectorMapBackdropImage != null)
                {
                    doorObj.transform.SetSiblingIndex(vectorMapBackdropImage.transform.GetSiblingIndex() + 1);
                }

                RectTransform rt = doorObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = dt.position;

                bool isHorizontal = dt.size.x >= dt.size.y;
                float thickness = IsLobbyMap() ? 3.0f : 2.0f;
                float glowThickness = IsLobbyMap() ? 6.0f : 4.5f;
                Vector2 lineSize = isHorizontal ? new Vector2(dt.size.x, thickness) : new Vector2(thickness, dt.size.y);
                Vector2 glowSize = isHorizontal ? new Vector2(dt.size.x, glowThickness) : new Vector2(glowThickness, dt.size.y);
                rt.sizeDelta = lineSize;

                Image lineImg = doorObj.AddComponent<Image>();
                lineImg.color = new Color(0.0f, 0.95f, 1.0f, 1.0f);
                lineImg.raycastTarget = false;

                // Pulsing Halo Glow child
                GameObject glowObj = new GameObject("Glow", typeof(RectTransform));
                glowObj.transform.SetParent(doorObj.transform, false);
                RectTransform grt = glowObj.GetComponent<RectTransform>();
                grt.anchorMin = new Vector2(0.5f, 0.5f);
                grt.anchorMax = new Vector2(0.5f, 0.5f);
                grt.pivot = new Vector2(0.5f, 0.5f);
                grt.anchoredPosition = Vector2.zero;
                grt.sizeDelta = glowSize;

                Image glowImg = glowObj.AddComponent<Image>();
                glowImg.color = new Color(0.0f, 0.90f, 1.0f, 0f);
                glowImg.raycastTarget = false;
                glowObj.SetActive(false);

                // Start inactive: zero redundant rectangles drawn over blueprint lines when idle
                doorObj.SetActive(false);

                string rA = "", rB = "";
                string cleanDoor = dt.doorId.StartsWith("door_") ? dt.doorId.Substring(5) : dt.doorId;
                if (currentFloorData.rooms != null)
                {
                    // Exact prefix + suffix token match (prevents "n_room" falsely matching inside "nw_room")
                    foreach (var rm in currentFloorData.rooms)
                    {
                        if (rm == null || string.IsNullOrEmpty(rm.roomId)) continue;
                        string prefix = rm.roomId + "_";
                        if (cleanDoor.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            string remainder = cleanDoor.Substring(prefix.Length);
                            if (currentFloorData.rooms.Exists(other => other != null && string.Equals(other.roomId, remainder, StringComparison.OrdinalIgnoreCase)))
                            {
                                rA = rm.roomId;
                                rB = remainder;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(rA))
                    {
                        foreach (var rm in currentFloorData.rooms.OrderByDescending(x => x != null && x.roomId != null ? x.roomId.Length : 0))
                        {
                            if (rm == null || string.IsNullOrEmpty(rm.roomId)) continue;
                            if (cleanDoor.IndexOf(rm.roomId, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (string.IsNullOrEmpty(rA)) rA = rm.roomId;
                                else if (string.IsNullOrEmpty(rB) && !string.Equals(rm.roomId, rA, StringComparison.OrdinalIgnoreCase)) rB = rm.roomId;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(rB) && !string.IsNullOrEmpty(dt.connectedRoomId))
                {
                    rB = dt.connectedRoomId;
                }

                var holder = new DoorwayUIHolder
                {
                    doorId = dt.doorId,
                    roomA = rA,
                    roomB = rB,
                    position = dt.position,
                    size = dt.size,
                    rootObj = doorObj,
                    lineImg = lineImg,
                    glowImg = glowImg,
                    isGlowing = false
                };
                doorwayHolders.Add(holder);
            }
        }

        public void UpdateDoorwayVisuals()
        {
            if (doorwayHolders == null || doorwayHolders.Count == 0) return;

            bool showStaticDoors = currentFloorData != null && currentFloorData.vectorMapSprite == null;

            foreach (var d in doorwayHolders)
            {
                if (d == null) continue;
                bool shouldGlow = false;

                if (!string.IsNullOrEmpty(currentThresholdRoomId))
                {
                    // Player is standing at the threshold of a specific room from a hallway
                    bool connectsCurrentHall = string.Equals(d.roomA, currentLocationId, StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(d.roomB, currentLocationId, StringComparison.OrdinalIgnoreCase);
                    bool connectsTargetRoom = string.Equals(d.roomA, currentThresholdRoomId, StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(d.roomB, currentThresholdRoomId, StringComparison.OrdinalIgnoreCase) ||
                                              d.doorId.IndexOf(currentThresholdRoomId, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (connectsCurrentHall && connectsTargetRoom)
                    {
                        shouldGlow = true;
                    }
                }
                else if (IsLobbyMap())
                {
                    if (string.Equals(d.roomA, currentLocationId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(d.roomB, currentLocationId, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldGlow = true;
                    }
                }
                else if (currentLocationId.StartsWith("hallway", StringComparison.OrdinalIgnoreCase)
                      || currentLocationId.StartsWith("f2_hall", StringComparison.OrdinalIgnoreCase))
                {
                    // Player is navigating the corridor without inspecting a specific exterior door
                    bool connectsCurrent = string.Equals(d.roomA, currentLocationId, StringComparison.OrdinalIgnoreCase) ||
                                           string.Equals(d.roomB, currentLocationId, StringComparison.OrdinalIgnoreCase);
                    if (connectsCurrent)
                    {
                        // Corridor spine connections glow to illuminate player's corridor path (room doorways stay unlit)
                        bool isVerticalSpine = d.size.y > d.size.x;
                        bool isHallA = !string.IsNullOrEmpty(d.roomA) && (d.roomA.StartsWith("hallway", StringComparison.OrdinalIgnoreCase) || d.roomA.StartsWith("f2_hall", StringComparison.OrdinalIgnoreCase));
                        bool isHallB = !string.IsNullOrEmpty(d.roomB) && (d.roomB.StartsWith("hallway", StringComparison.OrdinalIgnoreCase) || d.roomB.StartsWith("f2_hall", StringComparison.OrdinalIgnoreCase));
                        bool connectsTwoHallways = isHallA && isHallB;
                        if (isVerticalSpine || connectsTwoHallways)
                        {
                            shouldGlow = true;
                        }
                    }
                }

                d.isGlowing = shouldGlow;
                if (d.rootObj != null)
                {
                    d.rootObj.SetActive(shouldGlow || showStaticDoors);
                }
                if (d.lineImg != null)
                {
                    d.lineImg.color = shouldGlow ? new Color(0.0f, 0.95f, 1.0f, 1.0f) : new Color(0.29f, 0.40f, 0.52f, 0.90f);
                }
                if (d.glowImg != null)
                {
                    d.glowImg.gameObject.SetActive(shouldGlow);
                }
            }
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
                        r.isAccessible, r.isDiscovered, (int)r.position.y, (int)r.position.x, r.iconType,
                        r.isStairs, r.isHallway, r.isBlocked, r.isLocked,
                        r.undiscoveredSub, r.undiscoveredTitle);
                    node.isVisited = r.isVisited || string.Equals(r.roomId, "unit_407", StringComparison.OrdinalIgnoreCase);
                    node.isChecked = node.isVisited;
                    node.connectedIds.AddRange(r.connectedRoomIds);
                    ApplyPersistentStateIfPresent(node);
                    nodeRegistry[r.roomId] = node;
                }
                return;
            }

            // Built-in Default Floor: Ascott Residency 4th Floor
            // ROW 0: North Wing (Unit 406 <-> Hallway 1 <-> Unit 407)
            var u407 = new NodeModel(
                "unit_407", "UNIT 407", "MY PLACE", "YOUR APARTMENT (LOCKED OUT)", "DEADBOLTED",
                "SAFE (SEALED)", "KEYS LOCKED INSIDE", "MUTED TV STATIC",
                "Your residential apartment. The heavy deadbolt clicked shut automatically when you stepped out into the hallway. In your panic to pack, your keys were left sitting on the kitchen counter. There's no getting back inside without making enough noise to wake the dead.",
                isAccessible: true, isDiscovered: true, row: 0, col: 2, iconType: TacticalRoomIconType.Home,
                undiscoveredSub: "MY PLACE", undiscoveredTitle: "YOUR APARTMENT (LOCKED OUT)");
            u407.isVisited = true;
            u407.isChecked = true;

            var h1 = new NodeModel(
                "hallway_1", "CORR-N1", "UPPER HALL", "NORTH HALLWAY (406 / 407)", "DIM LIGHTING",
                "LOW", "SCATTERED SHOES", "FAINT DRAFT",
                "The north corridor segment outside your door (407) and Professor Harmon's (406). Emergency backup bulbs cast jittery amber shadows over the worn carpet. Scattered slippers and dropped mail show how fast people ran.",
                isAccessible: true, isDiscovered: false, row: 0, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true,
                undiscoveredSub: "UPPER HALL", undiscoveredTitle: "NORTH HALLWAY CORRIDOR");

            var u406 = new NodeModel(
                "unit_406", "UNIT 406", "PROF. HARMON", "PROFESSOR HARMON'S PLACE", "KEYPAD LOCKED",
                "QUIET / NO SOUND", "SURVIVAL NOTES & FOOD", "FAINT KEYPAD HUM",
                "Professor Harmon's apartment. An eccentric sociology professor who spent years writing about societal collapse. He posted his keypad code (8445) before the cell towers died, telling neighbors to take whatever supplies they need.",
                isAccessible: true, isDiscovered: false, row: 0, col: 0, iconType: TacticalRoomIconType.Locked, isLocked: true,
                undiscoveredSub: "PROF. HARMON", undiscoveredTitle: "PROFESSOR HARMON'S PLACE");

            // ROW 1: Mid-North Wing (Unit 404 <-> Hallway 2 <-> Unit 405)
            var u405 = new NodeModel(
                "unit_405", "UNIT 405", "MARCUS", "MARCUS'S APARTMENT", "DOOR AJAR (TRIPPED?)",
                "TRIPWIRE HAZARD", "GUNPOWDER & SAFE", "METALLIC CLINK",
                "Marcus's place. The quiet veteran down the hall who always talked about home defense and bugging out. The door is cracked open an inch and smells sharply of gun oil and cordite—knowing Marcus, he didn't leave without booby-trapping the entrance.",
                isAccessible: true, isDiscovered: false, row: 1, col: 2, iconType: TacticalRoomIconType.Locked, isLocked: true,
                undiscoveredSub: "MARCUS", undiscoveredTitle: "MARCUS'S RESIDENCE");

            var h2 = new NodeModel(
                "hallway_2", "CORR-N2", "MID-HALL", "MID-HALLWAY (404 / 405)", "FAINT CORDITE",
                "MEDIUM", "OVERTURNED TABLES", "DOG BARKING & SCRATCHING",
                "The middle hallway corridor between Mrs. Gable's barricaded unit (404) and Marcus's apartment (405). The carpet is kicked up and smells faintly of spent powder. Buster's frantic whines echo through the walls.",
                isAccessible: true, isDiscovered: false, row: 1, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true,
                undiscoveredSub: "MID-HALL", undiscoveredTitle: "MID-HALLWAY CORRIDOR");

            var u404 = new NodeModel(
                "unit_404", "UNIT 404", "MRS. GABLE", "MRS. GABLE & BUSTER", "FURNITURE WEDGED",
                "PANICKED RESIDENT", "NONE / KEEP OUT", "DOG SCRATCHING & WHINING",
                "Old Mrs. Gable's place. Sweet old lady who always gave out hard candies in the elevator. She panics at every loud siren—she must have shoved her heavy oak dresser against the door when the screaming started. Her golden retriever Buster is whining and scratching frantically at the wood.",
                isAccessible: false, isDiscovered: false, row: 1, col: 0, iconType: TacticalRoomIconType.Hazard, isBlocked: true,
                undiscoveredSub: "MRS. GABLE", undiscoveredTitle: "MRS. GABLE'S RESIDENCE");

            // ROW 2: Mid-South Wing (Unit 402 <-> Hallway 3 <-> Unit 403)
            var u403 = new NodeModel(
                "unit_403", "UNIT 403", "SARAH (AWAY)", "SARAH'S APARTMENT", "LOCKED / VACANT",
                "NO SIGN OF LIFE", "NONE / VACANT", "DEAD SILENCE",
                "Sarah's apartment. She's an ER trauma nurse down at Swedish Medical. Her running sneakers aren't on the welcome mat and the apartment is pitch black. She was on the night shift when the hospitals were overwhelmed—she never made it back home.",
                isAccessible: false, isDiscovered: false, row: 2, col: 2, iconType: TacticalRoomIconType.Hazard, isBlocked: true,
                undiscoveredSub: "SARAH", undiscoveredTitle: "SARAH'S RESIDENCE");

            var h3 = new NodeModel(
                "hallway_3", "CORR-S1", "LOWER HALL", "LOWER HALLWAY (402 / 403)", "PITCH BLACK",
                "MEDIUM", "UNSEARCHED", "DISTANT CHOPIN PIANO",
                "The ceiling fixtures have completely burned out in this section. The hallway is pitch black except for a thin sliver of warm light beneath the Andersons' chained door, accompanied by haunting piano music.",
                isAccessible: true, isDiscovered: false, row: 2, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true,
                undiscoveredSub: "LOWER HALL", undiscoveredTitle: "LOWER HALLWAY CORRIDOR");

            var u402 = new NodeModel(
                "unit_402", "UNIT 402", "THE ANDERSONS", "THE ANDERSONS' PLACE", "CHAIN LOCKED",
                "CHILLING SILENCE", "MEDICINE & SUPP.", "CHOPIN ON VINYL",
                "The Andersons' place. The polite family from down the hall whose daughter Emma plays classical piano. The door is cracked two inches on a brass security chain. The smell of harsh bleach seeps out, while Chopin's Nocturne plays softly on repeat from their living room.",
                isAccessible: true, isDiscovered: false, row: 2, col: 0, iconType: TacticalRoomIconType.DoorAjar, isLocked: true,
                undiscoveredSub: "THE ANDERSONS", undiscoveredTitle: "THE ANDERSONS' RESIDENCE");

            // ROW 3: South Wing (Utility Closet <-> Hallway 4 <-> Unit 401)
            var u401 = new NodeModel(
                "unit_401", "UNIT 401", "PARTY FLAT", "THE PARTY FLAT (401)", "DOOR WIDE OPEN",
                "BLARING NOISE", "CAR KEYS & RATIONS", "BLARING POP MUSIC",
                "The corner apartment rented to college students. They were throwing a massive weekend party when the sirens started. The front door was left hanging wide open during the stampede. Obnoxious pop music is still thumping from their sound system, drowning out the street.",
                isAccessible: true, isDiscovered: false, row: 3, col: 2, iconType: TacticalRoomIconType.Apartment,
                undiscoveredSub: "STUDENT APT", undiscoveredTitle: "STUDENT RESIDENCE 401");

            var h4 = new NodeModel(
                "hallway_4", "CORR-S2", "SOUTH SPINE", "SOUTH SERVICE JUNCTION", "VIBRATING WALLS",
                "MEDIUM", "DISCARDED CUPS", "OZONE & THUMPING BASS",
                "Southern corridor intersection bordering the electrical utility room and stairwell foyer. The carpet gives way to scuffed concrete. The vibrating bass from Kyle's stereo shakes the ceiling tiles.",
                isAccessible: true, isDiscovered: false, row: 3, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true,
                undiscoveredSub: "SOUTH SPINE", undiscoveredTitle: "SOUTH SERVICE JUNCTION");

            var util = new NodeModel(
                "utility", "UTILITY", "MAINTENANCE", "CARL'S UTILITY CLOSET", "LOCKED DOOR",
                "TRANSFORMER HUM", "TOOLS & FUSES", "FLUORESCENT BUZZ",
                "Carl the building superintendent's maintenance closet. Houses the floor breaker panels, backup fuses, and janitorial supplies behind a heavy magnetic access door.",
                isAccessible: true, isDiscovered: false, row: 3, col: 0, iconType: TacticalRoomIconType.Utility, isLocked: true,
                undiscoveredSub: "MAINTENANCE", undiscoveredTitle: "UTILITY SERVICE BAY");

            // ROW 4: Exit Foyer (Collapsed East Wing <-> Hallway End <-> Stairwell)
            var stairs = new NodeModel(
                "stairs", "STAIRWELL", "FIRE EXIT", "EMERGENCY FIRE EXIT", "ESCAPE ROUTE",
                "BLOOD ON GLASS", "DESCENT TO LOBBY", "DRAFT FROM BELOW",
                "The north-west fire stairwell descending toward the lobby and street. Emergency push-bar is operational, but bloody smears on the glass warn of chaos on the lower floors. Once you go down, there is no coming back up.",
                isAccessible: true, isDiscovered: false, row: 4, col: 2, iconType: TacticalRoomIconType.Stairs, isStairs: true,
                undiscoveredSub: "FIRE EXIT", undiscoveredTitle: "WEST FIRE STAIRWELL");

            var hEnd = new NodeModel(
                "hallway_end", "FOYER-4S", "STAIR FOYER", "STAIRWELL ACCESS FOYER", "FIRE DOORS",
                "BLOODY PRINTS", "EXTINGUISHER", "DRAFT FROM BELOW",
                "Southern terminus of the 4th floor corridor. A discharged fire extinguisher lies rolling on the tiles. Bloody streaks smear the emergency fire doors leading into the stairwell.",
                isAccessible: true, isDiscovered: false, row: 4, col: 1, iconType: TacticalRoomIconType.Corridor, isHallway: true,
                undiscoveredSub: "STAIR FOYER", undiscoveredTitle: "STAIRWELL ACCESS FOYER");

            var blocked = new NodeModel(
                "empty_bot", "EAST WING", "BLOCKED HALL", "COLLAPSED EAST ALCOVE", "CAVED IN",
                "IMPASSABLE", "NONE", "SETTLING CONCRETE",
                "Plaster and support beams caved in when the blast wave hit the downtown substation. The corridor leading to the east residential wing is completely sealed by rubble.",
                isAccessible: false, isDiscovered: false, row: 4, col: 0, iconType: TacticalRoomIconType.Collapsed, isBlocked: true,
                undiscoveredSub: "EAST WING", undiscoveredTitle: "EAST RESIDENTIAL WING");

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
            foreach (var n in list)
            {
                ApplyPersistentStateIfPresent(n);
                nodeRegistry[n.id] = n;
            }
            // Seamless alias support for narrative/lore variants
            nodeRegistry["blocked_stairs"] = blocked;
            nodeRegistry["empty_bot"] = blocked;
            nodeRegistry["utility_closet"] = util;
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

            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model))
            {
                string norm = NormalizeLocationId(nodeId);
                if (!nodeRegistry.TryGetValue(norm, out model)) return;
            }

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
                iconImage.gameObject.SetActive(false);
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

        public void BindBlueprintBanners(TextMeshProUGUI headerTitleTmp, TextMeshProUGUI subtitleTmp, TextMeshProUGUI coordTmp, TextMeshProUGUI footerLocationTmp, Image gridOverlayImg = null)
        {
            blueprintHeaderTitleTmp = headerTitleTmp;
            blueprintHeaderSubtitleTmp = subtitleTmp;
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

        public string CurrentLocationId => currentLocationId;
        public string CurrentThresholdRoomId => currentThresholdRoomId;

        private string NormalizeLocationId(string locId)
        {
            if (string.IsNullOrEmpty(locId)) return "unit_407";
            string lower = locId.ToLowerInvariant().Trim();

            if (currentFloorData != null && string.Equals(currentFloorData.floorId, "stairwell_4f", StringComparison.OrdinalIgnoreCase))
            {
                if (lower == "stairs" || lower == "stairwell" || lower == "f4_entrance" || lower == "f4 entrance" || lower == "f4")
                    return "f4_entrance";
                if (lower == "f3_landing" || lower == "f3 landing" || lower == "f3")
                    return "f3_landing";
            }
            else if (currentFloorData != null && string.Equals(currentFloorData.floorId, "stairwell_2f", StringComparison.OrdinalIgnoreCase))
            {
                if (lower == "stairs" || lower == "stairwell" || lower == "f2_entrance" || lower == "f2 entrance" || lower == "f2")
                    return "f2_entrance";
                if (lower == "f1_landing" || lower == "f1 landing" || lower == "barricade" || lower == "f1")
                    return "f1_landing";
            }

            if (string.Equals(lower, "utility_closet", StringComparison.OrdinalIgnoreCase)) return "utility";
            if (string.Equals(lower, "empty_bot", StringComparison.OrdinalIgnoreCase)) return "blocked_stairs";
            if (string.Equals(lower, "hallway_end", StringComparison.OrdinalIgnoreCase)) return "hallway_4";
            if (nodeRegistry != null && nodeRegistry.ContainsKey(locId)) return locId;
            return locId;
        }

        private bool TryGetUIHolder(string roomId, out NodeUIHolder holder)
        {
            if (string.IsNullOrEmpty(roomId)) { holder = null; return false; }
            if (uiHolders.TryGetValue(roomId, out holder) && holder != null && holder.rootObj != null) return true;
            string norm = NormalizeLocationId(roomId);
            if (!string.Equals(norm, roomId, StringComparison.OrdinalIgnoreCase) && uiHolders.TryGetValue(norm, out holder) && holder != null && holder.rootObj != null)
            {
                return true;
            }
            if (string.Equals(roomId, "blocked_stairs", StringComparison.OrdinalIgnoreCase) && uiHolders.TryGetValue("empty_bot", out holder) && holder != null && holder.rootObj != null) return true;
            if (string.Equals(roomId, "empty_bot", StringComparison.OrdinalIgnoreCase) && uiHolders.TryGetValue("blocked_stairs", out holder) && holder != null && holder.rootObj != null) return true;
            holder = null;
            return false;
        }

        public NodeModel GetNode(string roomId)
        {
            EnsureRuntimeBindings();
            if (string.IsNullOrEmpty(roomId)) return null;
            roomId = NormalizeLocationId(roomId);
            nodeRegistry.TryGetValue(roomId, out NodeModel model);
            return model;
        }

        public event Action<string, NodeModel> OnPlayerLocationChanged;

        public void SetCurrentLocation(string locationId)
        {
            EnsureRuntimeBindings();
            locationId = NormalizeLocationId(locationId);

            if (!nodeRegistry.ContainsKey(locationId)) return;
            currentLocationId = locationId;
            currentThresholdRoomId = null;
            selectedNodeId = locationId;

            var locNode = nodeRegistry[locationId];
            locNode.isVisited = true;
            locNode.isChecked = true;
            locNode.isDiscovered = true;
            locNode.isLocked = false;
            locNode.isAccessible = true;
            SaveNodeToPersistentState(locNode);

            RefreshAllNodes();
            SelectNode(locationId);
            UpdateDoorwayVisuals();
            OnPlayerLocationChanged?.Invoke(locationId, locNode);
        }

        public void SetThresholdLocation(string hallwayLocationId, string inspectingRoomId)
        {
            EnsureRuntimeBindings();
            hallwayLocationId = NormalizeLocationId(hallwayLocationId);
            inspectingRoomId = NormalizeLocationId(inspectingRoomId);

            if (!nodeRegistry.ContainsKey(hallwayLocationId)) return;
            currentLocationId = hallwayLocationId;
            currentThresholdRoomId = inspectingRoomId;
            selectedNodeId = inspectingRoomId;

            var hallNode = nodeRegistry[hallwayLocationId];
            hallNode.isVisited = true;
            hallNode.isChecked = true;
            hallNode.isDiscovered = true;
            SaveNodeToPersistentState(hallNode);

            if (!string.IsNullOrEmpty(inspectingRoomId) && nodeRegistry.TryGetValue(inspectingRoomId, out var targetNode))
            {
                targetNode.isChecked = true;
                targetNode.isDiscovered = true;
                SaveNodeToPersistentState(targetNode);
            }
            RefreshAllNodes();
            SelectNode(inspectingRoomId);
            UpdateDoorwayVisuals();
            OnPlayerLocationChanged?.Invoke(hallwayLocationId, hallNode);
        }

        public void DiscoverRoom(string roomId)
        {
            EnsureRuntimeBindings();
            roomId = NormalizeLocationId(roomId);
            if (nodeRegistry.TryGetValue(roomId, out NodeModel model))
            {
                model.isDiscovered = true;
                if (TryGetUIHolder(roomId, out NodeUIHolder holder))
                {
                    ApplyVisualState(holder);
                }
                if (string.Equals(selectedNodeId, roomId, StringComparison.OrdinalIgnoreCase))
                {
                    SelectNode(selectedNodeId);
                }
            }
        }

        public void DiscoverRooms(params string[] roomIds)
        {
            if (roomIds == null) return;
            foreach (var id in roomIds) DiscoverRoom(id);
        }

        public void DiscoverAdjacentRooms(string roomId)
        {
            EnsureRuntimeBindings();
            roomId = NormalizeLocationId(roomId);
            if (nodeRegistry.TryGetValue(roomId, out NodeModel model))
            {
                foreach (var connId in model.connectedIds)
                {
                    DiscoverRoom(connId);
                }
            }
        }

        public void SetRoomDiscovered(string roomId, bool discovered)
        {
            EnsureRuntimeBindings();
            roomId = NormalizeLocationId(roomId);
            if (nodeRegistry.TryGetValue(roomId, out NodeModel model))
            {
                model.isDiscovered = discovered;
                if (TryGetUIHolder(roomId, out NodeUIHolder holder))
                {
                    ApplyVisualState(holder);
                }
                if (string.Equals(selectedNodeId, roomId, StringComparison.OrdinalIgnoreCase))
                {
                    SelectNode(selectedNodeId);
                }
            }
        }

        public bool IsRoomDiscovered(string roomId)
        {
            EnsureRuntimeBindings();
            roomId = NormalizeLocationId(roomId);
            return nodeRegistry.TryGetValue(roomId, out NodeModel model) && model.isDiscovered;
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
            EnsureRuntimeBindings();
            nodeId = NormalizeLocationId(nodeId);
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model))
            {
                string norm = NormalizeLocationId(nodeId);
                if (!nodeRegistry.TryGetValue(norm, out model)) return;
            }
            selectedNodeId = nodeId;

            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }
            UpdateHorizontalNodeLabels();

            bool isPhysicallyInside = string.Equals(model.id, currentLocationId, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(currentThresholdRoomId);
            bool isCurrent = isPhysicallyInside;
            bool isAtThreshold = !string.IsNullOrEmpty(currentThresholdRoomId) && string.Equals(model.id, currentThresholdRoomId, StringComparison.OrdinalIgnoreCase);
            bool isDiscovered = model.isDiscovered || isPhysicallyInside || isAtThreshold;

            // Update Blueprint Coordinate & Footer Readout
            if (blueprintCoordTmp != null)
            {
                string gridX = model.col == 0 ? "W-01" : (model.col == 1 ? "C-00" : "E-02");
                string gridY = $"R-0{model.row + 1}";
                blueprintCoordTmp.text = $"GRID [{gridX}:{gridY}] // BIO-RADAR: ACTIVE";
            }

            if (blueprintFooterLocationTmp != null)
            {
                string posHex = StyleDThemeEnabled ? "#CF9232" : "#00e5ff";
                string valHex = StyleDThemeEnabled ? "#E4DEC9" : "#00e5ff";
                string pipeHex = StyleDThemeEnabled ? "#7A746A" : "#475569";

                if (!string.IsNullOrEmpty(currentThresholdRoomId) && nodeRegistry.TryGetValue(currentThresholdRoomId, out NodeModel thresholdNode))
                {
                    string hallName = nodeRegistry.TryGetValue(currentLocationId, out NodeModel hNode) ? hNode.roomCode : currentLocationId.ToUpper();
                    blueprintFooterLocationTmp.text = isHorizontalMiniMap
                        ? $"<color={posHex}><b>POS:</b></color> <color={valHex}>{hallName} ({thresholdNode.roomCode})</color>"
                        : $"<color={posHex}><b>PLAYER POS:</b></color> <color={valHex}>{hallName} (THRESHOLD // {thresholdNode.roomCode})</color>   <color={pipeHex}>|</color>   <color=#f39c12><b>INSPECTING:</b> {model.roomCode}</color>";
                }
                else if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel playerNode))
                {
                    if (isHorizontalMiniMap)
                    {
                        blueprintFooterLocationTmp.text = isPhysicallyInside
                            ? $"<color={posHex}><b>POS:</b></color> <color={valHex}>{playerNode.roomCode} ({playerNode.shortSub})</color>"
                            : $"<color={posHex}><b>POS:</b></color> <color={valHex}>{playerNode.roomCode}</color> <color={pipeHex}>|</color> <color=#f39c12><b>VIEW:</b> {model.roomCode}</color>";
                    }
                    else
                    {
                        blueprintFooterLocationTmp.text =
                            $"<color={posHex}><b>PLAYER POS:</b></color> <color={valHex}>{playerNode.roomCode} ({playerNode.shortSub})</color>   <color={pipeHex}>|</color>   <color=#f39c12><b>INSPECTING:</b> {model.roomCode}</color>";
                    }
                }
            }

            // Update Right Dossier Header & Icon
            if (intelHeaderIcon != null)
            {
                if (isPhysicallyInside)
                {
                    intelHeaderIcon.sprite = GetRoomIconSprite(model.iconType);
                    intelHeaderIcon.color = new Color(0.0f, 0.90f, 1.0f);
                }
                else if (isAtThreshold)
                {
                    intelHeaderIcon.sprite = GetRoomIconSprite(model.iconType);
                    intelHeaderIcon.color = new Color(0.98f, 0.72f, 0.18f);
                }
                else if (isDiscovered)
                {
                    intelHeaderIcon.sprite = GetRoomIconSprite(model.iconType);
                    intelHeaderIcon.color = model.isAccessible ? new Color(0.25f, 0.88f, 0.55f) : (model.isLocked ? new Color(0.95f, 0.65f, 0.15f) : new Color(0.92f, 0.30f, 0.30f));
                }
                else
                {
                    TacticalRoomIconType baseIcon = model.isHallway ? TacticalRoomIconType.Corridor : (model.isStairs ? TacticalRoomIconType.Stairs : (model.id.Contains("util") ? TacticalRoomIconType.Utility : TacticalRoomIconType.Apartment));
                    intelHeaderIcon.sprite = GetRoomIconSprite(baseIcon);
                    intelHeaderIcon.color = new Color(0.35f, 0.48f, 0.62f);
                }
            }

            if (intelSectorTitleText != null)
            {
                if (isPhysicallyInside || isAtThreshold || isDiscovered)
                {
                    intelSectorTitleText.text = $"<b>{model.roomCode} // {model.title}</b>";
                }
                else
                {
                    intelSectorTitleText.text = $"<b>{model.roomCode} // {model.undiscoveredTitle}</b>";
                }
            }

            if (intelStatusBadgeText != null)
            {
                if (isPhysicallyInside)
                {
                    intelStatusBadgeText.text = "<color=#00e5ff><b>[ CURRENT LOCATION // OCCUPIED ]</b></color>";
                }
                else if (isAtThreshold)
                {
                    string colorHex = model.isLocked ? "#f39c12" : (model.isBlocked ? "#e74c3c" : "#38bdf8");
                    string tag = model.isLocked ? "DOORWAY // LOCKED EXTERIOR" : (model.isBlocked ? "DOORWAY // BARRED ENTRY" : "DOORWAY // THRESHOLD ACCESS");
                    intelStatusBadgeText.text = $"<color={colorHex}><b>[ {tag} ]</b></color>";
                }
                else if (isDiscovered)
                {
                    string colorHex = model.isAccessible ? "#2ecc71" : (model.isLocked ? "#f39c12" : "#e74c3c");
                    intelStatusBadgeText.text = $"<color={colorHex}><b>[ {model.statusTag} ]</b></color>";
                }
                else
                {
                    intelStatusBadgeText.text = "<color=#64748b><b>[ UNCHARTED // NO RECON TELEMETRY ]</b></color>";
                }
            }

            if (intelThreatText != null)
            {
                if (isPhysicallyInside)
                {
                    intelThreatText.text = "<color=#94a3b8>THREAT:</color> <color=#2ecc71><b>NONE (INTERIOR SECURED)</b></color>";
                }
                else if (isAtThreshold)
                {
                    intelThreatText.text = $"<color=#94a3b8>THREAT:</color> <color=#f39c12><b>ACTIVE (STANDING AT EXTERIOR)</b></color>";
                }
                else if (isDiscovered)
                {
                    string threatCol = model.threatLevel switch
                    {
                        "NONE" or "ZERO" => "#2ecc71",
                        "LOW" or "MINIMAL" => "#38bdf8",
                        "MEDIUM" => "#f39c12",
                        "HIGH" or "DANGER" or "IMPASSABLE" => "#e74c3c",
                        _ => "#cbd5e1"
                    };
                    intelThreatText.text = $"<color=#94a3b8>THREAT:</color> <color={threatCol}><b>{model.threatLevel}</b></color>";
                }
                else
                {
                    intelThreatText.text = "<color=#94a3b8>THREAT:</color> <color=#cbd5e1><b>UNKNOWN (NO SENSOR DATA)</b></color>";
                }
            }

            if (intelScavengeText != null)
            {
                if (isCurrent)
                {
                    intelScavengeText.text = $"<color=#94a3b8>SUPPLIES:</color> <color=#2ecc71><b>{model.scavengeStatus}</b></color>";
                }
                else if (isDiscovered)
                {
                    intelScavengeText.text = $"<color=#94a3b8>SUPPLIES:</color> <color=#f39c12><b>{model.scavengeStatus}</b></color>";
                }
                else
                {
                    intelScavengeText.text = "<color=#94a3b8>SUPPLIES:</color> <color=#cbd5e1><b>UNKNOWN (UNSEARCHED)</b></color>";
                }
            }

            if (intelAcousticText != null)
            {
                if (isCurrent || isDiscovered)
                {
                    intelAcousticText.text = $"<color=#94a3b8>AUDIO:</color> <color=#38bdf8><b>{model.acousticScan}</b></color>";
                }
                else
                {
                    intelAcousticText.text = "<color=#94a3b8>AUDIO:</color> <color=#cbd5e1><b>OUT OF SENSOR RANGE</b></color>";
                }
            }

            if (intelDescriptionText != null)
            {
                if (isCurrent || isDiscovered)
                {
                    intelDescriptionText.text = model.reconIntel;
                }
                else
                {
                    intelDescriptionText.text = "Standard apartment listed on the floor directory. You haven't checked on this neighbor yet. Approach the door to see if anyone is inside.";
                }
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
                    transitActionBtnText.text = $"<color=#00e5ff><b>[ CURRENT LOCATION // {model.title.ToUpper()} ]</b></color>";
                }
                else if (!isDiscovered)
                {
                    transitActionBg.color = new Color(0.04f, 0.06f, 0.08f, 0.95f);
                    transitActionBtnText.text = "<color=#64748b><b>[ RECON UNAVAILABLE // SECTOR UNINSPECTED ]</b></color>";
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
            UpdateHorizontalNodeLabels();
        }

        private static string GetMiniMapRoomLabel(string roomId, NodeModel model = null)
        {
            if (string.IsNullOrEmpty(roomId)) return null;
            string lower = roomId.ToLowerInvariant().Trim();
            if (lower == "f4_entrance" || lower == "f4 entrance" || lower == "f4") return "4F";
            if (lower == "f3_landing" || lower == "f3 landing" || lower == "f3") return "3F";
            if (lower == "f2_entrance" || lower == "f2 entrance" || lower == "f2") return "2F";
            if (lower == "f1_landing" || lower == "f1 landing" || lower == "barricade" || lower == "f1") return "1F";
            if (lower == "stairs" || lower == "stairwell") return null;
            if (lower.Contains("blocked") || lower == "empty_bot") return null;

            switch (lower)
            {
                case "unit_407": return "407";
                case "unit_406": return "406";
                case "unit_405": return "405";
                case "unit_404": return "404";
                case "unit_403": return "403";
                case "unit_402": return "402";
                case "unit_401": return "401";
                case "utility":
                case "utility_closet": return "UTIL";
                case "nw_room":
                case "w_room":
                case "c_room":
                case "sw_room":
                case "s_room":
                case "se_room":
                    return "";
                case "n_room":
                case "ne_room":
                case "e_room":
                    return (model == null || !model.isChecked) ? "!" : "";
                case "lobby_exit":
                    return (model == null || model.isLocked) ? "!" : "";
                default:
                    if (lower.StartsWith("unit_")) return lower.Substring(5).ToUpperInvariant();
                    return null;
            }
        }

        private void UpdateHorizontalNodeLabels()
        {
            bool rotate90 = ShouldRotateFloorplanForMiniMap();
            foreach (var kvp in uiHolders)
            {
                var holder = kvp.Value;
                if (holder == null || holder.rootObj == null) continue;

                Transform existing = holder.rootObj.transform.Find("HorizontalLabelPlate");
                string roomId = holder.model != null ? holder.model.id : holder.nodeId;
                string shortLabel = GetMiniMapRoomLabel(roomId, holder.model);
                bool isPlayerInsideNode = string.Equals(roomId, currentLocationId, StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(currentThresholdRoomId);
                if (IsLobbyMap() && isPlayerInsideNode && string.IsNullOrEmpty(shortLabel))
                {
                    shortLabel = "YOU";
                }
                bool isExitStairsIcon = (holder.model != null && holder.model.isStairs && !holder.model.isBlocked && !holder.model.id.Contains("blocked") && string.IsNullOrEmpty(shortLabel))
                    || string.Equals(roomId, "stairs", StringComparison.OrdinalIgnoreCase);

                if (holder.iconImage != null) holder.iconImage.gameObject.SetActive(false);
                if (holder.roomCodeTmp != null)
                {
                    bool showDefaultCode = !rotate90 && !isExitStairsIcon && (holder.model == null || holder.model.isHallway || !string.IsNullOrEmpty(shortLabel));
                    holder.roomCodeTmp.gameObject.SetActive(showDefaultCode);
                }
                if (holder.subLabelTmp != null) holder.subLabelTmp.gameObject.SetActive(!rotate90);

                string labelText = rotate90 ? shortLabel : null;

                if (string.IsNullOrEmpty(labelText) && !isExitStairsIcon)
                {
                    if (existing != null) existing.gameObject.SetActive(false);
                    continue;
                }

                GameObject plateObj;
                if (existing != null)
                {
                    plateObj = existing.gameObject;
                }
                else
                {
                    plateObj = new GameObject("HorizontalLabelPlate", typeof(RectTransform));
                    plateObj.transform.SetParent(holder.rootObj.transform, false);
                }

                plateObj.SetActive(true);
                plateObj.transform.SetAsLastSibling();
                RectTransform pRt = plateObj.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.pivot = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = Vector2.zero;
                RectTransform nodeRt = holder.rootObj.GetComponent<RectTransform>();
                float plateSize = (nodeRt != null && Mathf.Min(nodeRt.sizeDelta.x, nodeRt.sizeDelta.y) > 38f)
                    ? Mathf.Min(nodeRt.sizeDelta.x, nodeRt.sizeDelta.y) - 6f
                    : 31f;
                pRt.sizeDelta = new Vector2(plateSize, plateSize);
                pRt.localScale = Vector3.one;
                pRt.localRotation = rotate90 ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.identity;

                bool isPlayerInside = string.Equals(roomId, currentLocationId, StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(currentThresholdRoomId);
                bool isRoomVisited = (holder.model != null && holder.model.isVisited) || isPlayerInside;
                bool hasStripe = !isPlayerInside && !isRoomVisited && holder.patternOverlay != null && holder.patternOverlay.gameObject.activeSelf;

                Image plateBg = plateObj.GetComponent<Image>() ?? plateObj.AddComponent<Image>();
                plateBg.color = Color.clear;
                plateBg.raycastTarget = false;

                Transform imgTr = plateObj.transform.Find("ExitIcon");
                Image exitImg;
                if (imgTr != null)
                {
                    exitImg = imgTr.GetComponent<Image>();
                }
                else
                {
                    GameObject iGo = new GameObject("ExitIcon", typeof(RectTransform));
                    iGo.transform.SetParent(plateObj.transform, false);
                    exitImg = iGo.AddComponent<Image>();
                    exitImg.preserveAspect = true;
                    exitImg.raycastTarget = false;
                    RectTransform iRt = exitImg.rectTransform;
                    iRt.anchorMin = new Vector2(0.08f, 0.08f);
                    iRt.anchorMax = new Vector2(0.92f, 0.92f);
                    iRt.offsetMin = Vector2.zero;
                    iRt.offsetMax = Vector2.zero;
                }

                Transform lblTr = plateObj.transform.Find("Label");
                TextMeshProUGUI lblTmp;
                if (lblTr != null)
                {
                    lblTmp = lblTr.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    GameObject lGo = new GameObject("Label", typeof(RectTransform));
                    lGo.transform.SetParent(plateObj.transform, false);
                    lblTmp = lGo.AddComponent<TextMeshProUGUI>();
                    RectTransform lRt = lblTmp.rectTransform;
                    lRt.anchorMin = Vector2.zero;
                    lRt.anchorMax = Vector2.one;
                    lRt.offsetMin = Vector2.zero;
                    lRt.offsetMax = Vector2.zero;
                    SundayTypography.ApplyFont(lblTmp, FontRole.Mono);
                }

                if (isExitStairsIcon)
                {
                    if (lblTmp != null) lblTmp.gameObject.SetActive(false);
                    if (exitImg != null)
                    {
                        exitImg.gameObject.SetActive(true);
                        exitImg.sprite = GetExitSignSprite();
                        exitImg.color = Color.white;
                    }
                }
                else
                {
                    if (exitImg != null) exitImg.gameObject.SetActive(false);
                    if (lblTmp != null)
                    {
                        lblTmp.gameObject.SetActive(true);
                        lblTmp.text = labelText;
                        lblTmp.fontSize = (roomId.Contains("util") || labelText.Length > 4) ? 7.5f : 10.0f;
                        lblTmp.fontStyle = FontStyles.Bold;
                        lblTmp.alignment = TextAlignmentOptions.Center;
                        lblTmp.textWrappingMode = TextWrappingModes.NoWrap;
                        if (StyleDThemeEnabled)
                        {
                            lblTmp.color = isPlayerInside
                                ? new Color(0.99f, 0.96f, 0.91f, 1f)
                                : (isRoomVisited ? new Color(0.96f, 0.93f, 0.87f, 1f) : (hasStripe ? new Color(0.95f, 0.97f, 1.0f, 1f) : new Color(0.76f, 0.72f, 0.66f, 1f)));
                            Outline lblOutline = lblTmp.GetComponent<Outline>() ?? lblTmp.gameObject.AddComponent<Outline>();
                            lblOutline.enabled = !isPlayerInside;
                            lblOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                            lblOutline.effectDistance = new Vector2(1f, -1f);
                        }
                        else
                        {
                            lblTmp.color = isPlayerInside
                                ? new Color(0.01f, 0.05f, 0.09f, 1f)
                                : (isRoomVisited ? new Color(0.03f, 0.05f, 0.08f, 1f) : (hasStripe ? new Color(0.95f, 0.97f, 1.0f, 1f) : new Color(0.62f, 0.70f, 0.78f, 1f)));
                            Outline lblOutline = lblTmp.GetComponent<Outline>() ?? lblTmp.gameObject.AddComponent<Outline>();
                            lblOutline.enabled = !isPlayerInside && !isRoomVisited;
                            lblOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                            lblOutline.effectDistance = new Vector2(1f, -1f);
                        }
                        lblTmp.raycastTarget = false;
                    }
                }
            }
        }

        // =========================================================================
        // BLUEPRINT CLEARANCE & VISUAL STYLING (RE2 + Signalis + Sevastolink)
        // =========================================================================

        private void ApplyVisualState(NodeUIHolder holder)
        {
            if (holder == null || holder.model == null) return;

            bool isPlayerPhysicallyInside = string.Equals(holder.model.id, currentLocationId, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrEmpty(currentThresholdRoomId);
            bool isCurrent = isPlayerPhysicallyInside;
            bool isSelected = string.Equals(holder.model.id, selectedNodeId, StringComparison.OrdinalIgnoreCase);
            bool isThresholdInspected = !string.IsNullOrEmpty(currentThresholdRoomId)
                && string.Equals(holder.model.id, currentThresholdRoomId, StringComparison.OrdinalIgnoreCase);
            bool isDiscovered = holder.model.isDiscovered || isPlayerPhysicallyInside || isThresholdInspected;

            // Player Radar Halo (Legacy sci-fi cyan outline + PulsingBadge; hidden in Style D so warm burnt-vermilion tile renders cleanly)
            if (holder.beaconObj != null)
            {
                holder.beaconObj.SetActive(isCurrent && !StyleDThemeEnabled);
            }

            // Hallways: Clean architectural connectors (like Twine)
            if (holder.model.isHallway)
            {
                if (holder.roomCodeTmp != null)
                {
                    holder.roomCodeTmp.text = isCurrent ? "▲ YOU" : "";
                    holder.roomCodeTmp.color = StyleDThemeEnabled
                        ? new Color(0.98f, 0.82f, 0.56f, 1f)
                        : new Color(0.0f, 0.95f, 1.0f, 1f);
                    holder.roomCodeTmp.fontSize = isHorizontalMiniMap ? 9f : 11f;
                    holder.roomCodeTmp.alignment = TextAlignmentOptions.Center;
                    holder.roomCodeTmp.gameObject.SetActive(true);
                }
                if (holder.subLabelTmp != null) holder.subLabelTmp.text = "";
                if (holder.iconImage != null) holder.iconImage.gameObject.SetActive(false);
                if (holder.navBadgeObj != null) holder.navBadgeObj.SetActive(false);
                if (holder.patternOverlay != null) holder.patternOverlay.gameObject.SetActive(false);

                bool isHallwayVisited = holder.model.isVisited || isPlayerPhysicallyInside;
                if (isCurrent)
                {
                    if (holder.bgImage != null) holder.bgImage.color = StyleDThemeEnabled ? new Color(0.55f, 0.22f, 0.12f, 1f) : new Color(0.04f, 0.22f, 0.32f, 1f);
                    if (holder.outline != null)
                    {
                        holder.outline.enabled = true;
                        holder.outline.effectColor = StyleDThemeEnabled ? new Color(0.90f, 0.45f, 0.25f, 1f) : new Color(0.0f, 0.92f, 1.0f, 1.0f);
                        holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                }
                else if (isSelected)
                {
                    if (holder.bgImage != null) holder.bgImage.color = isHallwayVisited
                        ? (StyleDThemeEnabled ? new Color(0.43f, 0.40f, 0.35f, 1f) : new Color(0.24f, 0.28f, 0.36f, 1f))
                        : (StyleDThemeEnabled ? new Color(0.16f, 0.15f, 0.13f, 1f) : new Color(0.04f, 0.05f, 0.07f, 1f));
                    if (holder.outline != null)
                    {
                        holder.outline.enabled = true;
                        holder.outline.effectColor = new Color(0.98f, 0.72f, 0.18f, 1.0f);
                        holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                }
                else if (isHallwayVisited)
                {
                    if (holder.bgImage != null) holder.bgImage.color = StyleDThemeEnabled ? new Color(0.43f, 0.40f, 0.35f, 1f) : new Color(0.22f, 0.26f, 0.33f, 1f);
                    if (holder.outline != null)
                    {
                        holder.outline.enabled = true;
                        holder.outline.effectColor = StyleDThemeEnabled ? new Color(0.58f, 0.54f, 0.48f, 0.85f) : new Color(0.40f, 0.48f, 0.58f, 0.85f);
                        holder.outline.effectDistance = new Vector2(1f, -1f);
                    }
                }
                else
                {
                    if (holder.bgImage != null) holder.bgImage.color = StyleDThemeEnabled ? new Color(0.16f, 0.15f, 0.13f, 1f) : new Color(0.025f, 0.035f, 0.05f, 1f);
                    if (holder.outline != null)
                    {
                        holder.outline.enabled = true;
                        holder.outline.effectColor = StyleDThemeEnabled ? new Color(0.28f, 0.26f, 0.23f, 0.75f) : new Color(0.12f, 0.18f, 0.26f, 0.60f);
                        holder.outline.effectDistance = new Vector2(1f, -1f);
                    }
                }
                return;
            }

            // Room Nodes: Solid flat fill colors adhering to the legend
            // (cyan = player pos, grey = visited, black = unvisited, red stripes = checked locked, yellow stripes = checked blocked/inaccessible)
            var m = holder.model;
            bool isLobby = IsLobbyMap();
            bool isRoomVisited = m.isVisited || isPlayerPhysicallyInside;
            bool isRoomChecked = m.isChecked || isThresholdInspected || isRoomVisited;
            string shortLabel = GetMiniMapRoomLabel(holder.nodeId, holder.model);
            bool isLobbyAlert = isLobby && string.Equals(shortLabel, "!", StringComparison.Ordinal);
            bool isStairsExit = (m.isStairs && !m.isBlocked && !m.id.Contains("blocked") && string.IsNullOrEmpty(shortLabel));

            bool showYellowStripe = !isLobbyAlert && !isCurrent && !isRoomVisited && isRoomChecked && (!m.isAccessible || m.isBlocked);
            bool showRedStripe = !isLobby && !isCurrent && !isRoomVisited && isRoomChecked && (m.isAccessible && !m.isBlocked && m.isLocked);

            Color fill = StyleDThemeEnabled
                ? (isCurrent ? new Color(0.85f, 0.34f, 0.16f, 1f)
                  : (isStairsExit ? new Color(0.169f, 0.157f, 0.137f, 1f)
                  : (isLobbyAlert ? new Color(0.169f, 0.157f, 0.137f, 1f)
                  : (isRoomVisited ? new Color(0.431f, 0.408f, 0.361f, 1f)
                  : new Color(0.169f, 0.157f, 0.137f, 1f)))))
                : (isCurrent ? new Color(0.0f, 0.82f, 0.98f, 1f)
                  : (isStairsExit ? new Color(0.01f, 0.015f, 0.025f, 1f)
                  : (isLobbyAlert ? new Color(0.01f, 0.015f, 0.025f, 1f)
                  : (isRoomVisited ? new Color(0.58f, 0.64f, 0.72f, 1f)
                  : new Color(0.01f, 0.015f, 0.025f, 1f)))));
            if (holder.bgImage != null) holder.bgImage.color = fill;

            if (holder.outline != null)
            {
                bool showStaticOutline = currentFloorData != null && currentFloorData.vectorMapSprite == null;
                holder.outline.enabled = isCurrent || isSelected || showStaticOutline || isLobbyAlert;
                if (isCurrent)
                {
                    holder.outline.effectColor = StyleDThemeEnabled ? new Color(0.94f, 0.77f, 0.61f, 1f) : new Color(0.75f, 0.98f, 1.0f, 1f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                else if (isSelected)
                {
                    holder.outline.effectColor = new Color(0.98f, 0.72f, 0.18f, 1f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                else if (isLobbyAlert)
                {
                    holder.outline.effectColor = new Color(0.92f, 0.24f, 0.22f, 0.90f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                else if (showStaticOutline)
                {
                    holder.outline.effectColor = StyleDThemeEnabled
                        ? new Color(0.32f, 0.29f, 0.25f, 0.85f)
                        : (isLobby ? new Color(0.28f, 0.40f, 0.54f, 0.90f) : new Color(0.18f, 0.28f, 0.38f, 0.85f));
                    holder.outline.effectDistance = new Vector2(1.3f, -1.3f);
                }
            }

            if (holder.iconImage != null) holder.iconImage.gameObject.SetActive(false);
            if (holder.navBadgeObj != null) holder.navBadgeObj.SetActive(false);
            if (holder.subLabelTmp != null) holder.subLabelTmp.gameObject.SetActive(false);
            if (holder.doorTickImage != null) holder.doorTickImage.color = new Color(0f, 0f, 0f, 0f);

            if (holder.patternOverlay != null)
            {
                if (showYellowStripe)
                {
                    holder.patternOverlay.sprite = GetHazardStripeSprite();
                    holder.patternOverlay.color = new Color(1f, 1f, 1f, 0.85f);
                    holder.patternOverlay.gameObject.SetActive(true);
                }
                else if (showRedStripe)
                {
                    holder.patternOverlay.sprite = GetRedStripeSprite();
                    holder.patternOverlay.color = new Color(1f, 1f, 1f, 0.85f);
                    holder.patternOverlay.gameObject.SetActive(true);
                }
                else
                {
                    holder.patternOverlay.gameObject.SetActive(false);
                }
            }

            // Label Text (e.g. 407, 4F, 3F, 2F, 1F, YOU, or red '!' crisis markers on 1F Lobby)
            if (isLobby && isCurrent && string.IsNullOrEmpty(shortLabel))
            {
                shortLabel = "YOU";
            }
            if (holder.roomCodeTmp != null)
            {
                if (isStairsExit)
                {
                    holder.roomCodeTmp.gameObject.SetActive(false);
                }
                else if (!string.IsNullOrEmpty(shortLabel))
                {
                    holder.roomCodeTmp.gameObject.SetActive(true);
                    holder.roomCodeTmp.text = shortLabel;
                    if (isLobbyAlert)
                    {
                        holder.roomCodeTmp.fontSize = 24f;
                        holder.roomCodeTmp.color = isCurrent
                            ? new Color(0.05f, 0.08f, 0.12f, 1f)
                            : new Color(1.0f, 0.20f, 0.15f, 1.0f);
                    }
                    else
                    {
                        holder.roomCodeTmp.fontSize = (shortLabel.Length > 4) ? 9f : 12f;
                        if (StyleDThemeEnabled)
                        {
                            holder.roomCodeTmp.color = isCurrent
                                ? new Color(0.99f, 0.96f, 0.91f, 1f)
                                : (isRoomVisited ? new Color(0.96f, 0.93f, 0.87f, 1f) : ((showYellowStripe || showRedStripe) ? new Color(0.95f, 0.97f, 1.0f, 1f) : new Color(0.76f, 0.72f, 0.66f, 1f)));
                        }
                        else
                        {
                            holder.roomCodeTmp.color = isCurrent
                                ? new Color(0.01f, 0.05f, 0.09f, 1f) // Dark navy contrast on bright cyan fill
                                : (isRoomVisited ? new Color(0.03f, 0.05f, 0.08f, 1f) : ((showYellowStripe || showRedStripe) ? new Color(0.95f, 0.97f, 1.0f, 1f) : new Color(0.62f, 0.70f, 0.78f, 1f)));
                        }
                    }
                    Outline codeOutline = holder.roomCodeTmp.GetComponent<Outline>() ?? holder.roomCodeTmp.gameObject.AddComponent<Outline>();
                    codeOutline.enabled = StyleDThemeEnabled
                        ? (!isCurrent && !isLobbyAlert)
                        : (!isCurrent && !isRoomVisited && !isLobbyAlert);
                    codeOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                    codeOutline.effectDistance = new Vector2(1f, -1f);
                    if (holder.patternOverlay != null && holder.roomCodeTmp.transform.parent == holder.patternOverlay.transform.parent)
                    {
                        holder.roomCodeTmp.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    holder.roomCodeTmp.gameObject.SetActive(false);
                }
            }
        }
    }
}
