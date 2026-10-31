using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using SundayLastBreath.Data;

namespace SundayLastBreath.Editor
{
    /// <summary>
    /// Designer Tool: Imports SVGs directly exported from Figma (or Illustrator/Inkscape),
    /// parses room geometry, labels, and corridor connection lines, and automatically generates
    /// modular TacticalFloorMapData ScriptableObjects for the PDA Tactical Blueprint.
    /// </summary>
    public class FigmaSvgMapImporter : EditorWindow
    {
        [MenuItem("Tools/Sunday's Last Breath/Map Tools/Import Floor Map from Figma SVG...", false, 102)]
        [MenuItem("Sunday's Last Breath/Import Floor Map from Figma SVG...", false, 102)]
        public static void OpenWindow()
        {
            var win = GetWindow<FigmaSvgMapImporter>("Figma SVG Map Importer");
            win.minSize = new Vector2(520, 560);
            win.Show();
        }

        [MenuItem("Assets/Sunday's Last Breath/Generate Floor Map Data from this SVG", true)]
        private static bool ValidateContextMenuItem()
        {
            if (Selection.activeObject == null) return false;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Sunday's Last Breath/Generate Floor Map Data from this SVG", false, 20)]
        private static void OnContextMenuImport()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var win = GetWindow<FigmaSvgMapImporter>("Figma SVG Map Importer");
            win.svgAssetPath = path;
            win.DeriveFloorSettingsFromPath(path);
            win.Show();
        }

        [Header("Source SVG")]
        [SerializeField] private string svgAssetPath = "Assets/Sprites/Maps/Map_Ascott_4thFloor.svg";

        [Header("Floor Configuration")]
        [SerializeField] private string floorId = "ascott_4f";
        [SerializeField] private string floorTitle = "ARCHITECTURAL BLUEPRINT // ASCOTT RESIDENCY — 4TH FLOOR WEST WING";
        [SerializeField] private string floorSubtitle = "SCHEMATIC SCALE 1:100 // CONTIGUOUS FLOORPLAN";
        [SerializeField] private string gridCoordinate = "GRID [W-01:R-01] // BIO-RADAR: ACTIVE";
        [SerializeField] private string targetFolder = "Assets/Resources/Maps";

        [Header("Blueprint Canvas Target Bounds")]
        [SerializeField] private Vector2 targetCanvasSize = new Vector2(560f, 260f);
        [SerializeField] private bool autoConnectCorridorLines = true;
        [SerializeField] private float lineSnapTolerance = 35f;

        private Vector2 scrollPos;
        private string logOutput = "";

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(8);
            GUILayout.Label("FIGMA SVG MAP IMPORTER // SUNDAY'S LAST BREATH", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Draw your floorplan in Figma with labeled rectangles/frames (e.g. 'unit_407', 'unit_408', 'hall_1') " +
                "and connecting lines for hallways. Export as SVG, drop it into Unity, and click 'Generate Modular Map Asset' below!",
                MessageType.Info);
            EditorGUILayout.Space(6);

            // 1. File Selection
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Source SVG Vector Asset", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            svgAssetPath = EditorGUILayout.TextField("SVG Path", svgAssetPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(75)))
            {
                string selected = EditorUtility.OpenFilePanel("Select Figma SVG", "Assets/Sprites/Maps", "svg");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                    {
                        svgAssetPath = "Assets" + selected.Substring(Application.dataPath.Length).Replace('\\', '/');
                    }
                    else
                    {
                        svgAssetPath = selected;
                    }
                    DeriveFloorSettingsFromPath(svgAssetPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            UnityEngine.Object svgObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(svgAssetPath);
            EditorGUILayout.ObjectField("SVG Asset Preview", svgObj, typeof(UnityEngine.Object), false);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // 2. Blueprint Telemetry Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("2. Floor Telemetry & Headers", EditorStyles.boldLabel);
            floorId = EditorGUILayout.TextField("Floor ID", floorId);
            floorTitle = EditorGUILayout.TextField("Floor Title", floorTitle);
            floorSubtitle = EditorGUILayout.TextField("Floor Subtitle", floorSubtitle);
            gridCoordinate = EditorGUILayout.TextField("Grid Coordinate", gridCoordinate);
            targetFolder = EditorGUILayout.TextField("Target Output Folder", targetFolder);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // 3. Transformation Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("3. Blueprint Coordinates & Auto-Wiring", EditorStyles.boldLabel);
            targetCanvasSize = EditorGUILayout.Vector2Field("Blueprint Canvas Size", targetCanvasSize);
            autoConnectCorridorLines = EditorGUILayout.Toggle("Auto-Connect Corridor Lines", autoConnectCorridorLines);
            if (autoConnectCorridorLines)
            {
                lineSnapTolerance = EditorGUILayout.Slider("Line Snap Tolerance (px)", lineSnapTolerance, 10f, 80f);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(12);

            // Generate Button
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("GENERATE MODULAR MAP ASSET", GUILayout.Height(38)))
            {
                GenerateMapAsset();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);

            // Batch Convert All Existing SVGs
            if (GUILayout.Button("Batch Convert All SVGs in Assets/Sprites/Maps/", GUILayout.Height(26)))
            {
                BatchConvertAllMaps();
            }

            if (!string.IsNullOrEmpty(logOutput))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Import Log:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(logOutput, GUILayout.Height(120));
            }

            EditorGUILayout.EndScrollView();
        }

        public void DeriveFloorSettingsFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string fn = Path.GetFileNameWithoutExtension(path);
            string cleanId = fn.ToLowerInvariant().Replace("map_", "").Replace("_map", "");

            if (cleanId.Contains("4th") || cleanId.Contains("4f"))
            {
                floorId = "ascott_4f";
                floorTitle = "ARCHITECTURAL BLUEPRINT // ASCOTT RESIDENCY — 4TH FLOOR WEST WING";
                floorSubtitle = "SCHEMATIC SCALE 1:100 // CONTIGUOUS FLOORPLAN";
                gridCoordinate = "GRID [W-01:R-01] // BIO-RADAR: ACTIVE";
            }
            else if (cleanId.Contains("2f") || cleanId.Contains("floor2"))
            {
                floorId = "ascott_2f";
                floorTitle = "ARCHITECTURAL BLUEPRINT // ASCOTT RESIDENCY — 2ND FLOOR CORRIDOR";
                floorSubtitle = "SCHEMATIC SCALE 1:100 // RESIDENTIAL ACCESS";
                gridCoordinate = "GRID [W-02:R-01] // BIO-RADAR: STANDBY";
            }
            else if (cleanId.Contains("lobby"))
            {
                floorId = "ascott_lobby";
                floorTitle = "ARCHITECTURAL BLUEPRINT // ASCOTT GRAND RESIDENCY — MAIN LOBBY & FOYER";
                floorSubtitle = "SCHEMATIC SCALE 1:100 // GROUND FLOOR ENTRY";
                gridCoordinate = "GRID [G-01:L-01] // BIO-RADAR: ELEVATED THREAT";
            }
            else if (cleanId.Contains("1st") || cleanId.Contains("1f"))
            {
                floorId = "highschool_1f";
                floorTitle = "ARCHITECTURAL BLUEPRINT // TIMBERLINE HIGH SCHOOL — 1ST FLOOR ACADEMIC WING";
                floorSubtitle = "CLASSROOMS & LABORATORIES // ELEVATED FLOOR";
                gridCoordinate = "GRID [HS-02:1F] // BIO-RADAR: SCANNING";
            }
            else if (cleanId.Contains("ground"))
            {
                floorId = "highschool_ground";
                floorTitle = "ARCHITECTURAL BLUEPRINT // TIMBERLINE HIGH SCHOOL — GROUND FLOOR";
                floorSubtitle = "COMMUNITY EVACUATION FACILITY // CIVIC BLUEPRINT";
                gridCoordinate = "GRID [HS-01:GF] // BIO-RADAR: SCANNING";
            }
            else
            {
                floorId = cleanId;
                string titleWords = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cleanId.Replace("_", " "));
                floorTitle = "ARCHITECTURAL BLUEPRINT // " + titleWords.ToUpperInvariant();
                floorSubtitle = "SCHEMATIC SCALE 1:100 // TACTICAL BLUEPRINT";
                gridCoordinate = "GRID [SEC-01] // BIO-RADAR: ACTIVE";
            }
        }

        private class ParsedSvgRect
        {
            public string rawId;
            public string cleanId;
            public float x, y, width, height;
            public float cx, cy;
        }

        private class ParsedSvgLine
        {
            public float x1, y1, x2, y2;
        }

        public void GenerateMapAsset(bool silent = false)
        {
            if (!File.Exists(svgAssetPath))
            {
                if (!silent) EditorUtility.DisplayDialog("Error", "SVG file not found at path: " + svgAssetPath, "OK");
                Debug.LogError("SVG file not found at path: " + svgAssetPath);
                return;
            }

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(svgAssetPath);

                XmlNode svgNode = xml.DocumentElement;
                if (svgNode == null || !string.Equals(svgNode.Name, "svg", StringComparison.OrdinalIgnoreCase))
                {
                    if (!silent) EditorUtility.DisplayDialog("Error", "Invalid SVG file: root element is not <svg>.", "OK");
                    Debug.LogError("Invalid SVG file: root element is not <svg>.");
                    return;
                }

                // 1. Determine SVG Coordinate Dimensions (viewBox or width/height)
                float vbWidth = 500f, vbHeight = 500f;
                string vbAttr = svgNode.Attributes?["viewBox"]?.Value;
                if (!string.IsNullOrEmpty(vbAttr))
                {
                    string[] tokens = Regex.Split(vbAttr.Trim(), @"[\s,]+");
                    if (tokens.Length >= 4 &&
                        float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) &&
                        float.TryParse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float h))
                    {
                        vbWidth = w;
                        vbHeight = h;
                    }
                }
                else
                {
                    string wAttr = svgNode.Attributes?["width"]?.Value;
                    string hAttr = svgNode.Attributes?["height"]?.Value;
                    if (!string.IsNullOrEmpty(wAttr)) float.TryParse(wAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out vbWidth);
                    if (!string.IsNullOrEmpty(hAttr)) float.TryParse(hAttr, NumberStyles.Float, CultureInfo.InvariantCulture, out vbHeight);
                }

                // 2. Parse all <rect> elements
                List<ParsedSvgRect> rects = new List<ParsedSvgRect>();
                XmlNodeList rectNodes = xml.GetElementsByTagName("rect");
                foreach (XmlNode rNode in rectNodes)
                {
                    string id = rNode.Attributes?["id"]?.Value;
                    if (string.IsNullOrEmpty(id)) continue;

                    // Filter out internal patterns or background images
                    string idLower = id.ToLowerInvariant();
                    if (idLower.StartsWith("image") || idLower.StartsWith("pattern") || idLower.StartsWith("toilet_2") || idLower.StartsWith("toilet_4"))
                        continue;

                    float rx = 0f, ry = 0f, rw = 40f, rh = 40f;
                    string xStr = rNode.Attributes?["x"]?.Value;
                    string yStr = rNode.Attributes?["y"]?.Value;
                    string wStr = rNode.Attributes?["width"]?.Value;
                    string hStr = rNode.Attributes?["height"]?.Value;

                    if (!string.IsNullOrEmpty(xStr)) float.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out rx);
                    if (!string.IsNullOrEmpty(yStr)) float.TryParse(yStr, NumberStyles.Float, CultureInfo.InvariantCulture, out ry);
                    if (!string.IsNullOrEmpty(wStr)) float.TryParse(wStr, NumberStyles.Float, CultureInfo.InvariantCulture, out rw);
                    if (!string.IsNullOrEmpty(hStr)) float.TryParse(hStr, NumberStyles.Float, CultureInfo.InvariantCulture, out rh);

                    string clean = CleanRoomId(id);
                    // Avoid duplicates
                    if (rects.Exists(r => string.Equals(r.cleanId, clean, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    rects.Add(new ParsedSvgRect
                    {
                        rawId = id,
                        cleanId = clean,
                        x = rx,
                        y = ry,
                        width = rw,
                        height = rh,
                        cx = rx + rw * 0.5f,
                        cy = ry + rh * 0.5f
                    });
                }

                // Also inspect <path> elements that represent rooms (e.g. L-shaped rooms like radio_room)
                XmlNodeList pathNodes = xml.GetElementsByTagName("path");
                foreach (XmlNode pNode in pathNodes)
                {
                    string id = pNode.Attributes?["id"]?.Value;
                    if (string.IsNullOrEmpty(id)) continue;
                    string idLower = id.ToLowerInvariant();
                    if (idLower.StartsWith("txt_") || idLower.StartsWith("line_") || idLower.StartsWith("alert_") || idLower.Contains("text"))
                        continue;

                    string clean = CleanRoomId(id);
                    if (rects.Exists(r => string.Equals(r.cleanId, clean, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    // Approximate bounding box from path if possible, or skip if already captured
                    rects.Add(new ParsedSvgRect
                    {
                        rawId = id,
                        cleanId = clean,
                        x = 50f, y = 200f, width = 60f, height = 50f,
                        cx = 80f, cy = 225f
                    });
                }

                // 3. Parse <line> elements for corridor connectivity
                List<ParsedSvgLine> lines = new List<ParsedSvgLine>();
                if (autoConnectCorridorLines)
                {
                    XmlNodeList lineNodes = xml.GetElementsByTagName("line");
                    foreach (XmlNode lNode in lineNodes)
                    {
                        string x1Str = lNode.Attributes?["x1"]?.Value;
                        string y1Str = lNode.Attributes?["y1"]?.Value;
                        string x2Str = lNode.Attributes?["x2"]?.Value;
                        string y2Str = lNode.Attributes?["y2"]?.Value;

                        if (float.TryParse(x1Str, NumberStyles.Float, CultureInfo.InvariantCulture, out float x1) &&
                            float.TryParse(y1Str, NumberStyles.Float, CultureInfo.InvariantCulture, out float y1) &&
                            float.TryParse(x2Str, NumberStyles.Float, CultureInfo.InvariantCulture, out float x2) &&
                            float.TryParse(y2Str, NumberStyles.Float, CultureInfo.InvariantCulture, out float y2))
                        {
                            lines.Add(new ParsedSvgLine { x1 = x1, y1 = y1, x2 = x2, y2 = y2 });
                        }
                    }
                }

                // 4. Create or Load ScriptableObject Asset
                Directory.CreateDirectory(targetFolder);
                string assetName = GetAssetNameForFloorId(floorId);
                string assetPath = Path.Combine(targetFolder, assetName).Replace('\\', '/');

                TacticalFloorMapData mapAsset = AssetDatabase.LoadAssetAtPath<TacticalFloorMapData>(assetPath);
                if (mapAsset == null)
                {
                    mapAsset = CreateInstance<TacticalFloorMapData>();
                    AssetDatabase.CreateAsset(mapAsset, assetPath);
                }

                mapAsset.floorId = floorId;
                mapAsset.floorTitle = floorTitle;
                mapAsset.floorSubtitle = floorSubtitle;
                mapAsset.gridCoordinate = gridCoordinate;

                // Bind Vector Graphics Sprite
                Sprite svgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(svgAssetPath);
                if (svgSprite != null)
                {
                    mapAsset.vectorMapSprite = svgSprite;
                }

                mapAsset.rooms.Clear();
                mapAsset.doorways.Clear();

                // Compute aspect ratio scaling
                float scale = Mathf.Min(targetCanvasSize.x / vbWidth, targetCanvasSize.y / vbHeight);

                // 5. Build Room Nodes
                Dictionary<string, TacticalRoomNodeData> createdRooms = new Dictionary<string, TacticalRoomNodeData>(StringComparer.OrdinalIgnoreCase);

                foreach (var r in rects)
                {
                    // Convert SVG Top-Left coordinates to Unity UI Centered coordinates (Y up)
                    float normX = (r.cx - vbWidth * 0.5f);
                    float normY = (vbHeight * 0.5f - r.cy); // invert Y

                    Vector2 canvasPos = new Vector2(
                        Mathf.Round(normX * scale),
                        Mathf.Round(normY * scale)
                    );
                    Vector2 canvasSize = new Vector2(
                        Mathf.Max(24f, Mathf.Round(r.width * scale)),
                        Mathf.Max(24f, Mathf.Round(r.height * scale))
                    );

                    TacticalRoomIconType iconType = InferIconType(r.cleanId);
                    bool isHall = iconType == TacticalRoomIconType.Corridor;
                    bool isStairs = iconType == TacticalRoomIconType.Stairs;
                    bool isBlocked = iconType == TacticalRoomIconType.Hazard || iconType == TacticalRoomIconType.Collapsed;
                    bool isLocked = iconType == TacticalRoomIconType.Locked;
                    bool isSafehouse = iconType == TacticalRoomIconType.Home;

                    var node = new TacticalRoomNodeData
                    {
                        roomId = r.cleanId,
                        roomCode = FormatRoomCode(r.cleanId),
                        shortSub = FormatShortSub(r.cleanId, iconType),
                        fullTitle = FormatFullTitle(r.cleanId, iconType),
                        position = canvasPos,
                        size = canvasSize,
                        iconType = iconType,
                        isHallway = isHall,
                        isStairs = isStairs,
                        isLocked = isLocked,
                        isBlocked = isBlocked,
                        isAccessible = !isBlocked && !isLocked,
                        isDiscovered = isSafehouse,
                        statusTag = isSafehouse ? "CURRENT SECTOR" : (isBlocked ? "BLOCKED" : (isLocked ? "LOCKED" : "CLEAR")),
                        threatLevel = isSafehouse ? "NONE" : (isBlocked ? "HIGH" : (isHall ? "MEDIUM" : "LOW")),
                        scavengeStatus = isSafehouse ? "LOOTED / SECURED" : "UNSEARCHED",
                        acousticScan = isSafehouse ? "TV STATIC HUM" : (isHall ? "AIR DRAFT & SCRATCHING" : "DEAD SILENCE"),
                        reconIntel = GenerateDefaultIntel(r.cleanId, iconType),
                        undiscoveredSub = isHall ? "CORRIDOR" : "SECTOR RECON",
                        undiscoveredTitle = FormatFullTitle(r.cleanId, iconType)
                    };

                    createdRooms[r.cleanId] = node;
                    mapAsset.rooms.Add(node);
                }

                // 6. Connect Corridors from Lines
                int connectionCount = 0;
                if (autoConnectCorridorLines)
                {
                    foreach (var line in lines)
                    {
                        ParsedSvgRect r1 = FindNearestRect(line.x1, line.y1, rects, lineSnapTolerance);
                        ParsedSvgRect r2 = FindNearestRect(line.x2, line.y2, rects, lineSnapTolerance);

                        if (r1 != null && r2 != null && !string.Equals(r1.cleanId, r2.cleanId, StringComparison.OrdinalIgnoreCase))
                        {
                            if (createdRooms.TryGetValue(r1.cleanId, out var node1) &&
                                createdRooms.TryGetValue(r2.cleanId, out var node2))
                            {
                                if (!node1.connectedRoomIds.Contains(node2.roomId)) node1.connectedRoomIds.Add(node2.roomId);
                                if (!node2.connectedRoomIds.Contains(node1.roomId)) node2.connectedRoomIds.Add(node1.roomId);
                                connectionCount++;

                                // Add doorway threshold
                                Vector2 midPos = (node1.position + node2.position) * 0.5f;
                                mapAsset.doorways.Add(new TacticalDoorwayThresholdData
                                {
                                    doorId = "door_" + node1.roomId + "_" + node2.roomId,
                                    connectedRoomId = node2.roomId,
                                    position = midPos,
                                    size = new Vector2(24f, 4f)
                                });
                            }
                        }
                    }
                }

                EditorUtility.SetDirty(mapAsset);
                if (!silent)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(mapAsset);
                    Selection.activeObject = mapAsset;
                }

                logOutput = $"SUCCESSFULLY GENERATED MAP ASSET!\n" +
                            $"Asset Path: {assetPath}\n" +
                            $"Rooms Extracted: {mapAsset.rooms.Count}\n" +
                            $"Corridor Connections: {connectionCount}\n" +
                            $"Doorways Generated: {mapAsset.doorways.Count}\n" +
                            $"Vector Sprite Linked: {(svgSprite != null ? svgSprite.name : "None (Check VectorGraphics import)")}";
            }
            catch (Exception ex)
            {
                if (!silent) EditorUtility.DisplayDialog("Import Error", "Failed to parse SVG: " + ex.Message, "OK");
                Debug.LogException(ex);
            }
        }

        private static ParsedSvgRect FindNearestRect(float px, float py, List<ParsedSvgRect> rects, float maxDistance)
        {
            ParsedSvgRect best = null;
            float bestDist = maxDistance;

            foreach (var r in rects)
            {
                float dx = Mathf.Max(r.x - px, 0f, px - (r.x + r.width));
                float dy = Mathf.Max(r.y - py, 0f, py - (r.y + r.height));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = r;
                }
            }

            return best;
        }

        private static string CleanRoomId(string raw)
        {
            string s = raw.Trim();
            if (s.StartsWith("rect_", StringComparison.OrdinalIgnoreCase)) s = s.Substring(5);
            s = s.ToLowerInvariant();
            s = Regex.Replace(s, @"[\s\-]+", "_");

            // Canonical mapping for standard Sunday Last Breath rooms
            if (s == "407" || s == "safehouse") return "unit_407";
            if (s == "408") return "unit_408";
            if (s == "406") return "unit_406";
            if (s == "405") return "unit_405";
            if (s == "404") return "unit_404";
            if (s == "403") return "unit_403";
            if (s == "402") return "unit_402";
            if (s == "401") return "unit_401";
            if (s == "stairwell" || s == "stairs") return "stairs";
            if (s == "utility_bay" || s == "utility_closet") return "utility";

            return s;
        }

        private static TacticalRoomIconType InferIconType(string id)
        {
            string s = id.ToLowerInvariant();
            if (s.Contains("407") || s.Contains("safehouse") || s.Contains("home")) return TacticalRoomIconType.Home;
            if (s.Contains("stair") || s.Contains("fire_exit")) return TacticalRoomIconType.Stairs;
            if (s.Contains("hall") || s.Contains("corr") || s.Contains("foyer")) return TacticalRoomIconType.Corridor;
            if (s.Contains("blocked") || s.Contains("hazard")) return TacticalRoomIconType.Hazard;
            if (s.Contains("collapse")) return TacticalRoomIconType.Collapsed;
            if (s.Contains("lock")) return TacticalRoomIconType.Locked;
            if (s.Contains("util") || s.Contains("mep") || s.Contains("server")) return TacticalRoomIconType.Utility;
            return TacticalRoomIconType.Apartment;
        }

        private static string FormatRoomCode(string id)
        {
            string s = id.Replace("_", " ").Trim();
            if (s.StartsWith("unit ", StringComparison.OrdinalIgnoreCase))
                return s.ToUpperInvariant();
            if (s.StartsWith("room ", StringComparison.OrdinalIgnoreCase))
                return s.ToUpperInvariant();
            if (s.StartsWith("hall ", StringComparison.OrdinalIgnoreCase))
                return s.Replace("hall ", "CORR-").ToUpperInvariant();
            if (s == "stairs") return "STAIRWELL W";
            if (s == "utility") return "UTILITY BAY";

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s).ToUpperInvariant();
        }

        private static string FormatShortSub(string id, TacticalRoomIconType type)
        {
            if (type == TacticalRoomIconType.Home) return "SAFEHOUSE";
            if (type == TacticalRoomIconType.Stairs) return "FIRE EXIT";
            if (type == TacticalRoomIconType.Corridor) return "CORRIDOR";
            if (type == TacticalRoomIconType.Utility) return "SERVICE ROOM";
            if (type == TacticalRoomIconType.Hazard) return "BIOHAZARD";
            if (type == TacticalRoomIconType.Collapsed) return "COLLAPSED";
            if (type == TacticalRoomIconType.Locked) return "DEADBOLT LOCKED";

            string s = id.ToLowerInvariant();
            if (s.Contains("406")) return "PROFESSOR";
            if (s.Contains("405")) return "HUNTER'S APT";
            if (s.Contains("408")) return "KEYPAD SCANNER";
            if (s.Contains("office")) return "OFFICE";
            if (s.Contains("canteen") || s.Contains("kitchen")) return "CANTEEN";
            if (s.Contains("library")) return "LIBRARY";
            if (s.Contains("infirmary")) return "INFIRMARY";
            if (s.Contains("lab")) return "LABORATORY";

            return "RESIDENTIAL";
        }

        private static string FormatFullTitle(string id, TacticalRoomIconType type)
        {
            string code = FormatRoomCode(id);
            string sub = FormatShortSub(id, type);
            return $"{code} // {sub}";
        }

        private static string GenerateDefaultIntel(string id, TacticalRoomIconType type)
        {
            if (type == TacticalRoomIconType.Home)
                return "Your residential apartment. Reinforced deadbolt held through the initial outbreak chaos. Emergency broadcasts flicker over television.";
            if (type == TacticalRoomIconType.Stairs)
                return "Emergency fire exit stairwell. Heavy reinforced fire doors provide vertical access between facility floors.";
            if (type == TacticalRoomIconType.Corridor)
                return "Continuous corridor access spine. Low emergency amber illumination. Caution advised when clearing blind hallway corners.";
            if (type == TacticalRoomIconType.Utility)
                return "Facility service closet and breaker substation. Houses fuse boxes, spare cables, and building maintenance gear.";
            if (type == TacticalRoomIconType.Hazard)
                return "Critical biohazard quarantine or structural cave-in. Passage blocked by severe debris or viral contamination.";

            return $"Architectural sector {FormatRoomCode(id)}. Initial tactical scans show potential supply caches and survivor barricades.";
        }

        private static string GetAssetNameForFloorId(string id)
        {
            string fid = id.ToLowerInvariant();
            if (fid == "ascott_4f" || fid.Contains("4th")) return "FloorMap_Ascott_4F.asset";
            if (fid == "ascott_2f" || fid.Contains("2f")) return "FloorMap_Ascott_2F.asset";
            if (fid == "ascott_lobby" || fid.Contains("lobby")) return "FloorMap_Ascott_Lobby.asset";
            if (fid == "highschool_1f" || fid.Contains("1st") || fid.Contains("1f")) return "FloorMap_HighSchool_1F.asset";
            if (fid == "highschool_ground" || fid.Contains("ground")) return "FloorMap_HighSchool_Ground.asset";

            return "FloorMap_" + ToPascalCase(id) + ".asset";
        }

        private static string ToPascalCase(string s)
        {
            string[] words = s.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string res = "";
            foreach (var w in words)
            {
                if (w.Length > 0)
                    res += char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant();
            }
            return res;
        }

        public void BatchConvertAllMaps(bool silent = false)
        {
            string folder = "Assets/Sprites/Maps";
            if (!Directory.Exists(folder))
            {
                if (!silent) EditorUtility.DisplayDialog("Error", "Folder not found: " + folder, "OK");
                Debug.LogError("Folder not found: " + folder);
                return;
            }

            string[] svgs = Directory.GetFiles(folder, "*.svg");
            int count = 0;
            foreach (var svg in svgs)
            {
                svgAssetPath = svg.Replace('\\', '/');
                DeriveFloorSettingsFromPath(svgAssetPath);
                GenerateMapAsset(silent);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FigmaSvgMapImporter] Batch conversion complete: {count} SVGs converted into TacticalFloorMapData assets.");
            if (!silent)
            {
                EditorUtility.DisplayDialog("Batch Conversion Complete", $"Successfully converted {count} SVG map files into TacticalFloorMapData assets!", "OK");
            }
        }
    }
}
