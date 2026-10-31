using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    public enum TacticalRoomIconType
    {
        Apartment,
        Home,
        Locked,
        Hazard,
        DoorAjar,
        Utility,
        Stairs,
        Corridor,
        Collapsed
    }

    [System.Serializable]
    public class TacticalRoomNodeData
    {
        [Header("Identity & Labels")]
        public string roomId = "unit_407";
        public string roomCode = "UNIT 407";
        public string shortSub = "SAFEHOUSE";
        public string fullTitle = "YOUR RESIDENCE (SAFEHOUSE)";

        [Header("Canvas Placement")]
        public Vector2 position = new Vector2(-160f, 100f);
        public Vector2 size = new Vector2(168f, 44f);

        [Header("Room Category & Icon")]
        public TacticalRoomIconType iconType = TacticalRoomIconType.Home;
        public bool isHallway = false;
        public bool isStairs = false;
        public bool isLocked = false;
        public bool isBlocked = false;
        public bool isAccessible = true;

        [Header("Tactical Recon Intel")]
        public bool isDiscovered = false;
        public string undiscoveredSub = "RESIDENTIAL";
        public string undiscoveredTitle = "RESIDENTIAL UNIT";
        public string statusTag = "CURRENT SECTOR";
        public string threatLevel = "NONE";
        public string scavengeStatus = "LOOTED / SECURED";
        public string acousticScan = "60Hz TV STATIC HUM";
        [TextArea(3, 8)]
        public string reconIntel = "Your 4th-floor studio apartment. The heavy steel deadbolt held through the initial wave of sirens. Living room television flickers with civil defense emergency alert white noise.";

        [Header("Connected Room IDs")]
        public List<string> connectedRoomIds = new List<string>();
    }

    [System.Serializable]
    public class TacticalDoorwayThresholdData
    {
        public string doorId = "door_unit_407";
        public string connectedRoomId = "unit_407";
        public Vector2 position = new Vector2(-54f, 100f);
        public Vector2 size = new Vector2(44f, 4f);
    }

    /// <summary>
    /// Modular ScriptableObject defining floorplan layouts for the PDA Architectural Blueprint & Bio-Radar.
    /// Designers can create and edit new floorplans directly in the Unity Inspector without writing C# code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFloorMapData", menuName = "Sunday's Last Breath/Floor Map Data")]
    public class TacticalFloorMapData : ScriptableObject
    {
        [Header("Floor Identity")]
        public string floorId = "ascott_4f";
        public string floorTitle = "ARCHITECTURAL BLUEPRINT // ASCOTT RESIDENCY — 4TH FLOOR WEST WING";
        public string floorSubtitle = "SCHEMATIC SCALE 1:100 // CONTIGUOUS FLOORPLAN";
        public string gridCoordinate = "GRID [W-01:R-01] // BIO-RADAR: ACTIVE";

        [Header("Vector Graphics Blueprint Backdrop")]
        [Tooltip("Optional Vector Sprite (imported from SVG via VectorGraphics package) representing the architectural floorplan")]
        public Sprite vectorMapSprite;

        [Header("Room Nodes")]
        public List<TacticalRoomNodeData> rooms = new List<TacticalRoomNodeData>();

        [Header("Doorway Threshold Ticks")]
        public List<TacticalDoorwayThresholdData> doorways = new List<TacticalDoorwayThresholdData>();
    }
}

