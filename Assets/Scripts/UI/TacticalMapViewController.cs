using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Interactive Tactical Connected Floorplan Map View Controller for the Survivor PDA.
    /// Recreates the iconic Trials in Tainted Space (TiTS) mini-map & sector scanner aesthetic,
    /// faithfully integrated with the Twine 4th Floor Ascott Residency floorplan:
    /// - Uniform 5-row x 3-column square grid tiles (44x44 px) with rounded corners
    /// - Clean corridor connection links linking adjacent rooms horizontally and vertically
    /// - Solid glowing purple tile fill for current player location
    /// - Directional quick-nav indicators [W], [A], [S], [D] on reachable adjacent rooms
    /// - Diagonal hatched / hazard-striped overlay on blocked and locked rooms
    /// - Dedicated status icons (Lock, Stairs Descent, Lightning Bolt, Danger)
    /// - Dual-banner framing: Sector/Floor header at top, dynamic Location footer at bottom
    /// - Comprehensive Tactical Recon Dossier on the right panel
    /// - Keyboard (W, A, S, D) and click-based interactive navigation
    /// </summary>
    public class TacticalMapViewController : MonoBehaviour
    {
        [System.Serializable]
        public class NodeModel
        {
            public string id;
            public string roomCode;
            public string title;
            public string statusTag;
            public string threatLevel;
            public string scavengeStatus;
            [TextArea(2, 4)] public string reconIntel;
            public bool isAccessible;
            public bool isDiscovered;
            public bool isStairs;
            public bool isHallway;
            public bool isBlocked;
            public bool isLocked;
            public string iconSymbol;
            public int row;
            public int col;
            public List<string> connectedIds = new List<string>();

            public NodeModel(string id, string roomCode, string title, string statusTag,
                             string threatLevel, string scavengeStatus, string reconIntel,
                             bool isAccessible, bool isDiscovered, int row, int col,
                             bool isStairs = false, bool isHallway = false,
                             bool isBlocked = false, bool isLocked = false,
                             string iconSymbol = "")
            {
                this.id = id;
                this.roomCode = roomCode;
                this.title = title;
                this.statusTag = statusTag;
                this.threatLevel = threatLevel;
                this.scavengeStatus = scavengeStatus;
                this.reconIntel = reconIntel;
                this.isAccessible = isAccessible;
                this.isDiscovered = isDiscovered;
                this.row = row;
                this.col = col;
                this.isStairs = isStairs;
                this.isHallway = isHallway;
                this.isBlocked = isBlocked;
                this.isLocked = isLocked;
                this.iconSymbol = iconSymbol;
            }
        }

        private class NodeUIHolder
        {
            public NodeModel model;
            public GameObject rootObj;
            public Image bgImage;
            public Image hatchedOverlay;
            public Outline outline;
            public TextMeshProUGUI iconTextTmp;
            public TextMeshProUGUI subLabelTmp;
            public GameObject navBadgeObj;
            public TextMeshProUGUI navBadgeTmp;
            public GameObject beaconObj;
            public Button button;
        }

        [Header("Runtime State")]
        [SerializeField] private string currentLocationId = "unit_407";
        [SerializeField] private string selectedNodeId = "unit_407";

        [Header("Minimap Header & Footer Banners")]
        [SerializeField] private TextMeshProUGUI minimapHeaderSectorTmp;
        [SerializeField] private TextMeshProUGUI minimapHeaderFloorTmp;
        [SerializeField] private TextMeshProUGUI minimapFooterLocationTmp;
        [SerializeField] private TextMeshProUGUI minimapSubFooterTmp;

        [Header("Recon Intel Panel UI References")]
        [SerializeField] private TextMeshProUGUI intelSectorTitleText;
        [SerializeField] private TextMeshProUGUI intelStatusBadgeText;
        [SerializeField] private TextMeshProUGUI intelThreatText;
        [SerializeField] private TextMeshProUGUI intelScavengeText;
        [SerializeField] private TextMeshProUGUI intelDescriptionText;
        [SerializeField] private TextMeshProUGUI quickNavPromptText;

        private readonly Dictionary<string, NodeModel> nodeRegistry = new Dictionary<string, NodeModel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NodeUIHolder> uiHolders = new Dictionary<string, NodeUIHolder>(StringComparer.OrdinalIgnoreCase);

        private static Sprite cachedHatchedSprite;

        public static Sprite GetHatchedSprite()
        {
            if (cachedHatchedSprite != null) return cachedHatchedSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color cDark = new Color(0.10f, 0.12f, 0.16f, 0.85f);
            Color cStripe = new Color(0.28f, 0.35f, 0.45f, 0.90f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isStripe = ((x + y) % 8) < 4;
                    tex.SetPixel(x, y, isStripe ? cStripe : cDark);
                }
            }
            tex.Apply();

            cachedHatchedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedHatchedSprite;
        }

        private void Awake()
        {
            InitializeDataModel();
        }

        private void Start()
        {
            RefreshAllNodes();
            SelectNode(currentLocationId);
        }

        private void OnEnable()
        {
            RefreshAllNodes();
            SelectNode(selectedNodeId);
        }

        private void Update()
        {
            // Keyboard quick navigation in Trials in Tainted Space style (W, A, S, D / Arrow keys)
            if (!gameObject.activeInHierarchy) return;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                TryNavigateDirection("W");
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TryNavigateDirection("A");
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                TryNavigateDirection("S");
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                TryNavigateDirection("D");
            }
        }

        private void InitializeDataModel()
        {
            if (nodeRegistry.Count > 0) return;

            // =========================================================================
            // ROW 0: NORTH CORRIDOR (Unit 407 <-> Hallway 1 <-> Unit 406)
            // =========================================================================
            var u407 = new NodeModel(
                "unit_407", "407", "YOUR RESIDENCE", "SECURED", "MINIMAL", "LOOTED",
                "Your residential unit. Living room television flickers with emergency static. The heavy deadbolt held during the initial breach. Hallway corridor exit door is directly ahead.",
                isAccessible: true, isDiscovered: true, row: 0, col: 0, iconSymbol: "⌂");

            var h1 = new NodeModel(
                "hallway_1", "H1", "NORTH CORRIDOR", "CLEAR", "LOW", "UNCHECKED",
                "North corridor outside Units 407 and 406. Carpeted hallway, emergency exit lights faintly glowing red. Signs of hurried evacuation and scattered paperwork.",
                isAccessible: true, isDiscovered: true, row: 0, col: 1, isHallway: true, iconSymbol: "║");

            var u406 = new NodeModel(
                "unit_406", "406", "PROFESSOR", "LOCKED", "UNKNOWN", "HIGH INTEL",
                "Belongs to Dr. Alistair, retired history professor. Walls lined with academic texts and historical artifacts. Heavy brass deadbolt is engaged from inside.",
                isAccessible: false, isDiscovered: true, row: 0, col: 2, isLocked: true, iconSymbol: "🔒");

            // =========================================================================
            // ROW 1: MID-NORTH (Unit 405 <-> Hallway 2 <-> Unit 404)
            // =========================================================================
            var u405 = new NodeModel(
                "unit_405", "405", "HUNTER'S UNIT", "LOCKED", "UNKNOWN", "WEAPONS / GEAR",
                "Apartment of Marcus, an avid outdoorsman and hunter. Strong smell of gun oil and cured leather. High priority recon target for survival supplies.",
                isAccessible: false, isDiscovered: true, row: 1, col: 0, isLocked: true, iconSymbol: "🔒");

            var h2 = new NodeModel(
                "hallway_2", "H2", "CENTRAL HALL", "SHADOWS", "MEDIUM", "DEBRIS",
                "Central corridor section. Broken picture frames on the floor. Faint scratching heard from behind nearby apartment walls. Southern corridor continues.",
                isAccessible: true, isDiscovered: true, row: 1, col: 1, isHallway: true, iconSymbol: "║");

            var u404 = new NodeModel(
                "unit_404", "404", "NEIGHBOR", "BARRICADED", "HIGH", "INACCESSIBLE",
                "Door reinforced from within with nailed two-by-fours and heavy furniture. Deep violent claw marks score the outer wooden frame. Completely impenetrable.",
                isAccessible: false, isDiscovered: true, row: 1, col: 2, isBlocked: true, iconSymbol: "⛔");

            // =========================================================================
            // ROW 2: MID-SOUTH (Unit 403 <-> Hallway 3 <-> Unit 402)
            // =========================================================================
            var u403 = new NodeModel(
                "unit_403", "403", "RESIDENCE", "BLOOD SMEAR", "DANGER", "UNKNOWN",
                "A dark smear of dried blood streaks down the brass handle. Dead silence inside. Entry without tactical preparation is strongly discouraged.",
                isAccessible: false, isDiscovered: true, row: 2, col: 0, isBlocked: true, iconSymbol: "⛔");

            var h3 = new NodeModel(
                "hallway_3", "H3", "SOUTH-MID", "DIM LIGHT", "MEDIUM", "UNCHECKED",
                "Fluorescent ceiling bulb strobes erratically. Dark stains on the commercial carpet lead southward toward the fire stairwell.",
                isAccessible: true, isDiscovered: true, row: 2, col: 1, isHallway: true, iconSymbol: "║");

            var u402 = new NodeModel(
                "unit_402", "402", "APARTMENT 402", "DOOR AJAR", "LOW", "FOOD / MEDS",
                "Apartment door cracked open two inches. Sounds of dripping water inside. High potential for salvageable medical supplies and canned goods.",
                isAccessible: true, isDiscovered: true, row: 2, col: 2, iconSymbol: "🚪");

            // =========================================================================
            // ROW 3: SOUTH WING (Unit 401 <-> Hallway 4 <-> Utility)
            // =========================================================================
            var u401 = new NodeModel(
                "unit_401", "401", "APARTMENT 401", "UNCHECKED", "LOW", "SHELTER",
                "Corner residential unit. Mail stacked on welcome mat. Appears undisturbed since before the citywide emergency curfew was enacted.",
                isAccessible: true, isDiscovered: true, row: 3, col: 0, iconSymbol: "⌂");

            var h4 = new NodeModel(
                "hallway_4", "H4", "SOUTH CORRIDOR", "HAZARD", "ELEVATED", "UNCHECKED",
                "Corridor junction right before the emergency stairwell and electrical utility room. Air smells faintly of ozone, smoke, and damp plaster.",
                isAccessible: true, isDiscovered: true, row: 3, col: 1, isHallway: true, iconSymbol: "║");

            var util = new NodeModel(
                "utility", "UTIL", "MAINTENANCE", "BREAKER BOX", "LOW", "TOOLS / WIRE",
                "Building maintenance and electrical room. Heavy industrial steel door with latch. Contains master fuse panels and janitorial tool cabinets.",
                isAccessible: true, isDiscovered: true, row: 3, col: 2, iconSymbol: "⚡");

            // =========================================================================
            // ROW 4: STAIRWELL & EXIT (Stairs <-> Hall End <-> Sector East Blocked)
            // =========================================================================
            var stairs = new NodeModel(
                "stairs", "STAIRS", "FIRE STAIRWELL", "ESCAPE ROUTE", "ELEVATED", "TO 2ND FLOOR",
                "Emergency concrete stairwell descending to Floor 2 and the Lobby. Heavy reinforced fire door. Descending past here enters contested building sectors.",
                isAccessible: true, isDiscovered: true, row: 4, col: 0, isStairs: true, iconSymbol: "▼");

            var hEnd = new NodeModel(
                "hallway_end", "EXIT", "STAIR FOYER", "EXIT FOYER", "ELEVATED", "CLEAR",
                "Staircase access foyer. Discharged fire extinguisher on the floor. Muffled groans echo up from lower stair landings.",
                isAccessible: true, isDiscovered: true, row: 4, col: 1, isHallway: true, iconSymbol: "║");

            var blocked = new NodeModel(
                "empty_bot", "EAST", "EAST WING", "COLLAPSED", "IMPASSABLE", "NONE",
                "Ceiling drywall and structural concrete collapsed, completely sealing off the eastern wing. Impassable without demolition tools.",
                isAccessible: false, isDiscovered: false, row: 4, col: 2, isBlocked: true, iconSymbol: "✖");

            // Connect Topology
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

        public void BindUIHolder(string nodeId, GameObject rootObj, Image bgImage, Image hatchedOverlay,
                                 Outline outline, TextMeshProUGUI iconTextTmp, TextMeshProUGUI subLabelTmp,
                                 GameObject navBadgeObj, TextMeshProUGUI navBadgeTmp,
                                 GameObject beaconObj, Button btn)
        {
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;

            var holder = new NodeUIHolder
            {
                model = model,
                rootObj = rootObj,
                bgImage = bgImage,
                hatchedOverlay = hatchedOverlay,
                outline = outline,
                iconTextTmp = iconTextTmp,
                subLabelTmp = subLabelTmp,
                navBadgeObj = navBadgeObj,
                navBadgeTmp = navBadgeTmp,
                beaconObj = beaconObj,
                button = btn
            };

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

        public void BindIntelReferences(TextMeshProUGUI title, TextMeshProUGUI status,
                                        TextMeshProUGUI threat, TextMeshProUGUI scavenge,
                                        TextMeshProUGUI desc, TextMeshProUGUI quickNav)
        {
            intelSectorTitleText = title;
            intelStatusBadgeText = status;
            intelThreatText = threat;
            intelScavengeText = scavenge;
            intelDescriptionText = desc;
            quickNavPromptText = quickNav;
        }

        public void BindMinimapBanners(TextMeshProUGUI sectorTmp, TextMeshProUGUI floorTmp,
                                      TextMeshProUGUI locationTmp, TextMeshProUGUI subFooterTmp)
        {
            minimapHeaderSectorTmp = sectorTmp;
            minimapHeaderFloorTmp = floorTmp;
            minimapFooterLocationTmp = locationTmp;
            minimapSubFooterTmp = subFooterTmp;
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
            SelectNode(nodeId);

            // If player clicked on an adjacent accessible room, move into it!
            if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr))
            {
                if (curr.connectedIds.Contains(nodeId))
                {
                    if (nodeRegistry.TryGetValue(nodeId, out NodeModel target) && target.isAccessible)
                    {
                        SetCurrentLocation(nodeId);
                    }
                }
            }
        }

        public void SelectNode(string nodeId)
        {
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;
            selectedNodeId = nodeId;

            // Update all visual node outlines and nav badges
            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }

            // Update Minimap Footer Banner
            if (minimapFooterLocationTmp != null)
            {
                string marker = model.id == currentLocationId ? "CURRENT LOCATION: " : "INSPECTING: ";
                minimapFooterLocationTmp.text = $"<b>{marker}{model.title.ToUpper()}</b>";
            }

            // Update Recon Intel Panel
            if (intelSectorTitleText != null)
            {
                string icon = model.isStairs ? "▼ " : (model.isHallway ? "║ " : (model.isLocked ? "🔒 " : "■ "));
                intelSectorTitleText.text = $"{icon}<b>UNIT {model.roomCode} // {model.title}</b>";
            }

            if (intelStatusBadgeText != null)
            {
                string colorHex = model.id == currentLocationId ? "#00e5ff" : (model.isAccessible ? "#2ecc71" : (model.isLocked ? "#f39c12" : "#e74c3c"));
                string tag = model.id == currentLocationId ? "CURRENT LOCATION" : model.statusTag;
                intelStatusBadgeText.text = $"<color={colorHex}><b>[ {tag} ]</b></color>";
            }

            if (intelThreatText != null)
            {
                string threatCol = model.threatLevel switch
                {
                    "ZERO" or "MINIMAL" => "#2ecc71",
                    "LOW" => "#4fc3f7",
                    "MEDIUM" => "#f39c12",
                    "HIGH" or "DANGER" => "#e74c3c",
                    _ => "#95a5a6"
                };
                intelThreatText.text = $"THREAT LEVEL: <color={threatCol}><b>{model.threatLevel}</b></color>";
            }

            if (intelScavengeText != null)
            {
                intelScavengeText.text = $"RECON TARGET: <color=#f39c12><b>{model.scavengeStatus}</b></color>";
            }

            if (intelDescriptionText != null)
            {
                intelDescriptionText.text = model.reconIntel;
            }

            if (quickNavPromptText != null)
            {
                if (model.id == currentLocationId)
                {
                    quickNavPromptText.text = "<color=#00e5ff>▶</color> <color=#ffffff><b>YOU ARE HERE.</b> Use [W/A/S/D] or click adjacent highlighted rooms to navigate.</color>";
                }
                else if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr) && curr.connectedIds.Contains(model.id))
                {
                    string dir = GetDirectionLetter(curr, model);
                    string keyPrompt = string.IsNullOrEmpty(dir) ? "" : $"Press <b>[{dir}]</b> or ";
                    if (model.isAccessible)
                        quickNavPromptText.text = $"<color=#2ecc71>▶</color> <color=#ffffff><b>ADJACENT SECTOR:</b> {keyPrompt}click to step into {model.title}.</color>";
                    else if (model.isLocked)
                        quickNavPromptText.text = $"<color=#f39c12>▶</color> <color=#ffffff><b>DOOR LOCKED:</b> Heavy deadbolt engaged. Requires Lockpicking or Key.</color>";
                    else
                        quickNavPromptText.text = $"<color=#e74c3c>▶</color> <color=#ffffff><b>SECTOR BLOCKED:</b> Completely impassable without demolition tools.</color>";
                }
                else
                {
                    quickNavPromptText.text = $"<color=#f39c12>▶</color> <color=#ffffff><b>RECON TARGET:</b> {model.title} ({model.statusTag}). Requires moving through hallway.</color>";
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
            if (toNode.row < fromNode.row) return "W"; // North
            if (toNode.row > fromNode.row) return "S"; // South
            if (toNode.col < fromNode.col) return "A"; // West
            if (toNode.col > fromNode.col) return "D"; // East
            return "";
        }

        public void RefreshAllNodes()
        {
            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }

            if (nodeRegistry.TryGetValue(currentLocationId, out NodeModel curr))
            {
                if (minimapFooterLocationTmp != null)
                {
                    minimapFooterLocationTmp.text = $"<b>CURRENT LOCATION: {curr.title.ToUpper()}</b>";
                }
            }
        }

        private void ApplyVisualState(NodeUIHolder holder)
        {
            if (holder == null || holder.model == null) return;

            bool isCurrent = string.Equals(holder.model.id, currentLocationId, StringComparison.OrdinalIgnoreCase);
            bool isSelected = string.Equals(holder.model.id, selectedNodeId, StringComparison.OrdinalIgnoreCase);

            // Check if adjacent to current position
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

            // Beacon Halo
            if (holder.beaconObj != null)
            {
                holder.beaconObj.SetActive(isCurrent);
            }

            // Direction Navigation Badge [W], [A], [S], [D]
            if (holder.navBadgeObj != null)
            {
                holder.navBadgeObj.SetActive(isAdjacent && !string.IsNullOrEmpty(dirLetter));
                if (holder.navBadgeTmp != null && !string.IsNullOrEmpty(dirLetter))
                {
                    holder.navBadgeTmp.text = $"<b>{dirLetter}</b>";
                }
            }

            // Hatched Overlay for Blocked or Locked
            bool showHatched = holder.model.isBlocked || holder.model.isLocked;
            if (holder.hatchedOverlay != null)
            {
                holder.hatchedOverlay.gameObject.SetActive(showHatched);
                if (showHatched && holder.hatchedOverlay.sprite == null)
                {
                    holder.hatchedOverlay.sprite = GetHatchedSprite();
                    holder.hatchedOverlay.type = Image.Type.Tiled;
                }
            }

            // Colors based on Trials in Tainted Space aesthetic
            if (isCurrent)
            {
                // Active Player Room: Solid Vibrant Purple fill (#8e24aa / #9c27b0)
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.55f, 0.14f, 0.67f, 1.0f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.92f, 0.48f, 0.98f, 0.95f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = $"<b>{holder.model.roomCode}</b>";
                    holder.iconTextTmp.color = Color.white;
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "YOU";
                    holder.subLabelTmp.color = new Color(0.95f, 0.85f, 1.0f);
                }
            }
            else if (isSelected)
            {
                // Selected Room for Recon Intel: Gold Accent Outline
                if (holder.bgImage != null)
                {
                    holder.bgImage.color = holder.model.isHallway ?
                        new Color(0.08f, 0.11f, 0.16f, 0.95f) :
                        new Color(0.12f, 0.16f, 0.22f, 0.98f);
                }
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.98f, 0.70f, 0.15f, 0.95f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = GetIconText(holder.model);
                    holder.iconTextTmp.color = new Color(0.98f, 0.75f, 0.25f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.roomCode;
                    holder.subLabelTmp.color = Color.white;
                }
            }
            else if (isAdjacent)
            {
                // Reachable Adjacent Room: Highlighted Purple Outline (#a855f7)
                if (holder.bgImage != null)
                {
                    holder.bgImage.color = holder.model.isHallway ?
                        new Color(0.09f, 0.12f, 0.17f, 0.95f) :
                        new Color(0.13f, 0.18f, 0.25f, 0.98f);
                }
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.68f, 0.35f, 0.95f, 0.90f);
                    holder.outline.effectDistance = new Vector2(1.5f, -1.5f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = GetIconText(holder.model);
                    holder.iconTextTmp.color = new Color(0.85f, 0.70f, 0.98f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.roomCode;
                    holder.subLabelTmp.color = new Color(0.80f, 0.85f, 0.92f);
                }
            }
            else if (holder.model.isStairs)
            {
                // Stairs Room: Amber / Escape Theme
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.09f, 0.10f, 0.13f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.90f, 0.50f, 0.12f, 0.80f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = "▼";
                    holder.iconTextTmp.color = new Color(0.95f, 0.55f, 0.15f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "STAIRS";
                    holder.subLabelTmp.color = new Color(0.75f, 0.75f, 0.80f);
                }
            }
            else if (holder.model.isHallway)
            {
                // Central Corridor Hallway: Steel Blue-Grey Tile
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.24f, 0.32f, 0.42f, 0.80f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = holder.model.roomCode;
                    holder.iconTextTmp.color = new Color(0.60f, 0.72f, 0.85f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = "CORR";
                    holder.subLabelTmp.color = new Color(0.45f, 0.55f, 0.65f);
                }
            }
            else if (holder.model.isLocked)
            {
                // Locked Room: Dark Slate with Padlock icon
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.08f, 0.08f, 0.10f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.55f, 0.40f, 0.20f, 0.70f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = "🔒";
                    holder.iconTextTmp.color = new Color(0.95f, 0.70f, 0.20f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.roomCode;
                    holder.subLabelTmp.color = new Color(0.65f, 0.65f, 0.70f);
                }
            }
            else if (holder.model.isBlocked)
            {
                // Blocked / Impassable: Hatched Dark Slate with Hazard Red Outline
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.07f, 0.07f, 0.09f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.55f, 0.20f, 0.20f, 0.70f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = "⛔";
                    holder.iconTextTmp.color = new Color(0.85f, 0.35f, 0.35f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.roomCode;
                    holder.subLabelTmp.color = new Color(0.55f, 0.50f, 0.50f);
                }
            }
            else
            {
                // Standard Discovered Room: Dark Navy Slate (#1e293b)
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.10f, 0.14f, 0.20f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.24f, 0.30f, 0.40f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.iconTextTmp != null)
                {
                    holder.iconTextTmp.text = GetIconText(holder.model);
                    holder.iconTextTmp.color = new Color(0.75f, 0.86f, 0.95f);
                }
                if (holder.subLabelTmp != null)
                {
                    holder.subLabelTmp.text = holder.model.roomCode;
                    holder.subLabelTmp.color = new Color(0.55f, 0.65f, 0.75f);
                }
            }
        }

        private static string GetIconText(NodeModel model)
        {
            if (model.isStairs) return "▼";
            if (model.isLocked) return "🔒";
            if (model.isBlocked) return "⛔";
            if (model.id.Equals("utility", StringComparison.OrdinalIgnoreCase)) return "⚡";
            return model.roomCode;
        }
    }
}
