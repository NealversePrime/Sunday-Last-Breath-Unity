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
    /// Recreates the authentic Twine 4th Floor grid system:
    /// 5 rows of Left Units <==> Central Hallway Corridor <==> Right Units,
    /// complete with vertical hallway connectors, glowing current location beacons,
    /// and a comprehensive Recon Intel dossier panel.
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

            public NodeModel(string id, string roomCode, string title, string statusTag,
                             string threatLevel, string scavengeStatus, string reconIntel,
                             bool isAccessible, bool isDiscovered, bool isStairs = false,
                             bool isHallway = false, bool isBlocked = false)
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
                this.isStairs = isStairs;
                this.isHallway = isHallway;
                this.isBlocked = isBlocked;
            }
        }

        private class NodeUIHolder
        {
            public NodeModel model;
            public GameObject rootObj;
            public Image bgImage;
            public Outline outline;
            public TextMeshProUGUI roomCodeText;
            public TextMeshProUGUI titleText;
            public GameObject beaconObj;
            public Button button;
        }

        [Header("Runtime State")]
        [SerializeField] private string currentLocationId = "unit_407";
        [SerializeField] private string selectedNodeId = "unit_407";

        [Header("Recon Intel Panel UI References")]
        [SerializeField] private TextMeshProUGUI intelSectorTitleText;
        [SerializeField] private TextMeshProUGUI intelStatusBadgeText;
        [SerializeField] private TextMeshProUGUI intelThreatText;
        [SerializeField] private TextMeshProUGUI intelScavengeText;
        [SerializeField] private TextMeshProUGUI intelDescriptionText;

        [Header("Grid Containers")]
        [SerializeField] private Transform gridRoot;

        private readonly Dictionary<string, NodeModel> nodeRegistry = new Dictionary<string, NodeModel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, NodeUIHolder> uiHolders = new Dictionary<string, NodeUIHolder>(StringComparer.OrdinalIgnoreCase);
        private readonly List<MapRowData> rows = new List<MapRowData>();

        private struct MapRowData
        {
            public NodeModel left;
            public NodeModel hall;
            public NodeModel right;
        }

        private void Awake()
        {
            InitializeDataModel();
            if (gridRoot == null)
            {
                gridRoot = transform.Find("MapCard/Content/LeftCol_Grid/GridScroll/Viewport/Content");
                if (gridRoot == null) gridRoot = transform.Find("MapCard/Content/GridArea");
            }
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

        private void InitializeDataModel()
        {
            if (nodeRegistry.Count > 0) return;

            // Row 0: North Wing (Unit 407, Hallway 1, Unit 406)
            var u407 = new NodeModel(
                "unit_407", "UNIT 407", "YOUR RESIDENCE", "SECURED", "MINIMAL", "LOOTED",
                "Your residential unit. Living room television flickers with white noise. The heavy deadbolt held during the initial breach. Hallway exit door is directly ahead.",
                isAccessible: true, isDiscovered: true);

            var h1 = new NodeModel(
                "hallway_1", "HALL 1", "NORTH CORRIDOR", "CLEAR", "LOW", "UNCHECKED",
                "North corridor outside Unit 407 and 406. Carpeted hallway, emergency exit lights faintly glowing red. Signs of hurried evacuation.",
                isAccessible: true, isDiscovered: true, isHallway: true);

            var u406 = new NodeModel(
                "unit_406", "UNIT 406", "PROFESSOR", "LOCKED", "UNKNOWN", "HIGH INTEL",
                "Belongs to Dr. Alistair, retired history professor. Walls lined with academic texts and historical artifacts. Heavy brass deadbolt is engaged from inside.",
                isAccessible: true, isDiscovered: true);

            // Row 1: Mid-North (Unit 405, Hallway 2, Unit 404)
            var u405 = new NodeModel(
                "unit_405", "UNIT 405", "HUNTER'S UNIT", "LOCKED", "UNKNOWN", "WEAPONS / GEAR",
                "Apartment of Marcus, an avid outdoorsman and hunter. Strong smell of gun oil and cured leather. High priority recon target for survival supplies.",
                isAccessible: true, isDiscovered: true);

            var h2 = new NodeModel(
                "hallway_2", "HALL 2", "CENTRAL HALL", "SHADOWS", "MEDIUM", "DEBRIS",
                "Central corridor section. Broken picture frames on the floor. Faint scratching heard from behind nearby apartment walls.",
                isAccessible: true, isDiscovered: true, isHallway: true);

            var u404 = new NodeModel(
                "unit_404", "UNIT 404", "NEIGHBOR", "BARRICADED", "HIGH", "INACCESSIBLE",
                "Door reinforced from within with nailed two-by-fours and heavy furniture. Deep violent claw marks score the outer wooden frame.",
                isAccessible: false, isDiscovered: true);

            // Row 2: Mid-South (Unit 403, Hallway 3, Unit 402)
            var u403 = new NodeModel(
                "unit_403", "UNIT 403", "RESIDENCE", "BLOOD SMEAR", "DANGER", "UNKNOWN",
                "A dark smear of dried blood streaks down the brass handle. Dead silence inside. Entry without tactical preparation is strongly discouraged.",
                isAccessible: false, isDiscovered: true);

            var h3 = new NodeModel(
                "hallway_3", "HALL 3", "SOUTH-MID", "DIM LIGHT", "MEDIUM", "UNCHECKED",
                "Fluorescent ceiling bulb strobes erratically. Dark stains on the commercial carpet lead southward toward the fire stairwell.",
                isAccessible: true, isDiscovered: true, isHallway: true);

            var u402 = new NodeModel(
                "unit_402", "UNIT 402", "APARTMENT 402", "DOOR AJAR", "UNGUARDED", "FOOD / MEDS",
                "Apartment door cracked open two inches. Sounds of dripping water inside. High potential for salvageable medical supplies and canned goods.",
                isAccessible: true, isDiscovered: true);

            // Row 3: South Wing (Unit 401, Hallway 4, Utility)
            var u401 = new NodeModel(
                "unit_401", "UNIT 401", "APARTMENT 401", "UNCHECKED", "LOW", "SHELTER",
                "Corner residential unit. Mail stacked on welcome mat. Appears undisturbed since before the citywide emergency curfew was enacted.",
                isAccessible: true, isDiscovered: true);

            var h4 = new NodeModel(
                "hallway_4", "HALL 4", "SOUTH CORRIDOR", "HAZARD", "ELEVATED", "UNCHECKED",
                "Corridor junction right before the emergency stairwell and electrical utility room. Air smells faintly of ozone, smoke, and damp plaster.",
                isAccessible: true, isDiscovered: true, isHallway: true);

            var util = new NodeModel(
                "utility", "UTILITY", "MAINTENANCE", "BREAKER BOX", "LOW", "TOOLS / WIRE",
                "Building maintenance and electrical room. Heavy industrial steel door with latch. Contains master fuse panels and janitorial tool cabinets.",
                isAccessible: true, isDiscovered: true);

            // Row 4: Stairwell & Exit (Stairs, Hallway End, Blocked Sector)
            var stairs = new NodeModel(
                "stairs", "STAIRS", "TO 2ND FL", "FIRE EXIT", "ESCAPE ROUTE", "TACTICAL EXIT",
                "Emergency concrete stairwell descending to Floor 2 and the Lobby. Heavy reinforced fire door. Descending past here enters contested building sectors.",
                isAccessible: true, isDiscovered: true, isStairs: true);

            var hEnd = new NodeModel(
                "hallway_end", "HALL END", "STAIR FOYER", "EXIT FOYER", "ELEVATED", "CLEAR",
                "Staircase access foyer. Discharged fire extinguisher on the wall. Muffled groans echo up from lower stair landings.",
                isAccessible: true, isDiscovered: true, isHallway: true);

            var blocked = new NodeModel(
                "empty_bot", "SECTOR EAST", "BLOCKED", "COLLAPSED", "IMPASSABLE", "NONE",
                "Ceiling drywall and structural concrete collapsed, completely sealing off the eastern wing. Impassable without demolition tools.",
                isAccessible: false, isDiscovered: false, isBlocked: true);

            // Register nodes
            var list = new[] { u407, h1, u406, u405, h2, u404, u403, h3, u402, u401, h4, util, stairs, hEnd, blocked };
            foreach (var n in list) nodeRegistry[n.id] = n;

            rows.Add(new MapRowData { left = u407, hall = h1, right = u406 });
            rows.Add(new MapRowData { left = u405, hall = h2, right = u404 });
            rows.Add(new MapRowData { left = u403, hall = h3, right = u402 });
            rows.Add(new MapRowData { left = u401, hall = h4, right = util });
            rows.Add(new MapRowData { left = stairs, hall = hEnd, right = blocked });
        }

        public void BindUIHolder(string nodeId, GameObject rootObj, Image bgImage, Outline outline,
                                 TextMeshProUGUI roomCodeText, TextMeshProUGUI titleText,
                                 GameObject beaconObj, Button btn)
        {
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;

            var holder = new NodeUIHolder
            {
                model = model,
                rootObj = rootObj,
                bgImage = bgImage,
                outline = outline,
                roomCodeText = roomCodeText,
                titleText = titleText,
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
                    SelectNode(nodeId);
                });
            }

            ApplyVisualState(holder);
        }

        public void BindIntelReferences(TextMeshProUGUI title, TextMeshProUGUI status,
                                        TextMeshProUGUI threat, TextMeshProUGUI scavenge,
                                        TextMeshProUGUI desc)
        {
            intelSectorTitleText = title;
            intelStatusBadgeText = status;
            intelThreatText = threat;
            intelScavengeText = scavenge;
            intelDescriptionText = desc;
        }

        public void SetCurrentLocation(string locationId)
        {
            currentLocationId = locationId;
            RefreshAllNodes();
            SelectNode(locationId);
        }

        public void SelectNode(string nodeId)
        {
            if (!nodeRegistry.TryGetValue(nodeId, out NodeModel model)) return;
            selectedNodeId = nodeId;

            // Highlight selected node border
            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }

            // Update Intel Panel
            if (intelSectorTitleText != null)
            {
                string icon = model.isStairs ? "▼ " : (model.isHallway ? "║ " : "■ ");
                intelSectorTitleText.text = $"{icon}<b>{model.roomCode} // {model.title}</b>";
            }

            if (intelStatusBadgeText != null)
            {
                string colorHex = model.id == currentLocationId ? "#00e5ff" : (model.isAccessible ? "#2ecc71" : "#e74c3c");
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
        }

        public void RefreshAllNodes()
        {
            foreach (var kvp in uiHolders)
            {
                ApplyVisualState(kvp.Value);
            }
        }

        private void ApplyVisualState(NodeUIHolder holder)
        {
            if (holder == null || holder.model == null) return;

            bool isCurrent = string.Equals(holder.model.id, currentLocationId, StringComparison.OrdinalIgnoreCase);
            bool isSelected = string.Equals(holder.model.id, selectedNodeId, StringComparison.OrdinalIgnoreCase);

            if (holder.beaconObj != null)
            {
                holder.beaconObj.SetActive(isCurrent);
            }

            if (isCurrent)
            {
                // Active Player Location: Glowing Cyan
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.04f, 0.12f, 0.18f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.0f, 0.90f, 1.0f, 0.95f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.0f, 0.95f, 1.0f);
                if (holder.titleText != null) holder.titleText.color = new Color(0.75f, 0.95f, 1.0f);
            }
            else if (isSelected)
            {
                // Selected Target for Recon: Gold Outline
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.10f, 0.13f, 0.17f, 0.98f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.95f, 0.65f, 0.15f, 0.95f);
                    holder.outline.effectDistance = new Vector2(2f, -2f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.95f, 0.75f, 0.20f);
                if (holder.titleText != null) holder.titleText.color = Color.white;
            }
            else if (holder.model.isStairs)
            {
                // Stairs Exit: Amber Theme
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.08f, 0.08f, 0.10f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.90f, 0.50f, 0.12f, 0.75f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.95f, 0.55f, 0.15f);
                if (holder.titleText != null) holder.titleText.color = new Color(0.70f, 0.70f, 0.75f);
            }
            else if (holder.model.isHallway)
            {
                // Central Hallway: Steel Blue-Grey
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.06f, 0.07f, 0.09f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.22f, 0.28f, 0.35f, 0.75f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.55f, 0.68f, 0.80f);
                if (holder.titleText != null) holder.titleText.color = new Color(0.40f, 0.50f, 0.60f);
            }
            else if (!holder.model.isAccessible)
            {
                // Inaccessible / Barricaded: Dark Slate / Dim Hazard Red
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.05f, 0.04f, 0.04f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.50f, 0.15f, 0.15f, 0.65f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.85f, 0.35f, 0.35f);
                if (holder.titleText != null) holder.titleText.color = new Color(0.55f, 0.40f, 0.40f);
            }
            else
            {
                // Standard Discovered Room: Dark Navy Slate
                if (holder.bgImage != null) holder.bgImage.color = new Color(0.05f, 0.06f, 0.08f, 0.95f);
                if (holder.outline != null)
                {
                    holder.outline.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
                    holder.outline.effectDistance = new Vector2(1f, -1f);
                }
                if (holder.roomCodeText != null) holder.roomCodeText.color = new Color(0.70f, 0.82f, 0.92f);
                if (holder.titleText != null) holder.titleText.color = new Color(0.55f, 0.60f, 0.68f);
            }
        }
    }
}

