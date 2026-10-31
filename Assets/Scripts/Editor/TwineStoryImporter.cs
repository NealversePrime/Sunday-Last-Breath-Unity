using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using SundayLastBreath.Data;

namespace SundayLastBreath.Editor
{
    /// <summary>
    /// Designer Tool: Imports Twine / Twee (.twee or .txt) story passages into modular
    /// StoryPassageData ScriptableObjects. Supports 1 single file for your entire story or
    /// batch importing an entire folder of .twee files.
    /// Parses narrative beats, dialogue lines, choices, RPG checks, backdrops, and room map tags automatically.
    /// </summary>
    public class TwineStoryImporter : EditorWindow
    {
        [MenuItem("Tools/Sunday's Last Breath/Story Tools/Import Passages from Twee File...", false, 103)]
        [MenuItem("Sunday's Last Breath/Import Passages from Twee File...", false, 103)]
        public static void OpenWindow()
        {
            var win = GetWindow<TwineStoryImporter>("Twine Story Importer");
            win.minSize = new Vector2(560, 600);
            win.Show();
        }

        [MenuItem("Tools/Sunday's Last Breath/Story Tools/Batch Import All Twine Passages...", false, 104)]
        [MenuItem("Sunday's Last Breath/Batch Import All Twine Passages...", false, 104)]
        public static void OpenBatchImportWindow()
        {
            var win = GetWindow<TwineStoryImporter>("Twine Story Importer");
            win.minSize = new Vector2(560, 600);
            win.tweeFolderPath = "d:/Documents/Twine/Twine Game/Sunday-Last-Breath/src/passages";
            win.Show();
        }

        [MenuItem("Assets/Sunday's Last Breath/Import Story Passages from this Twee", true)]
        private static bool ValidateContextMenuItem()
        {
            if (Selection.activeObject == null) return false;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return path.EndsWith(".twee", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Sunday's Last Breath/Import Story Passages from this Twee", false, 21)]
        private static void OnContextMenuImport()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var win = GetWindow<TwineStoryImporter>("Twine Story Importer");
            win.tweeFilePath = path;
            win.Show();
        }

        [Header("Single Twee File")]
        public string tweeFilePath = "";

        [Header("Batch Import Folder")]
        public string tweeFolderPath = "d:/Documents/Twine/Twine Game/Sunday-Last-Breath/src/passages";

        [Header("Output Configuration")]
        public string targetFolder = "Assets/Resources/StoryPassages";
        public bool overwriteExisting = true;
        public bool autoLinkChoices = true;
        public bool autoAssignBackdrops = true;

        public void ImportFile(string path, bool silent = false)
        {
            tweeFilePath = path;
            ImportTweeFile(silent);
        }

        private Vector2 scrollPos;
        private string logOutput = "";

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(8);
            GUILayout.Label("TWINE / TWEE MODULAR STORY IMPORTER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Author your entire narrative in Twine or .twee files! " +
                "This importer automatically extracts `:: Passage_Name [tags]`, rich story beats, SugarCube choices, " +
                "left/right VN dialogue, RPG attribute checks, backdrops, and connects them directly to Tactical Map rooms.",
                MessageType.Info);
            EditorGUILayout.Space(6);

            // SECTION 1: Single File Import
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Single Twee File Import", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            tweeFilePath = EditorGUILayout.TextField("Twee File", tweeFilePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(75)))
            {
                string sel = EditorUtility.OpenFilePanel("Select Twee Story File", "d:/Documents/Twine/Twine Game/Sunday-Last-Breath/src/passages", "twee;*.txt");
                if (!string.IsNullOrEmpty(sel)) tweeFilePath = sel;
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("IMPORT THIS TWEE FILE", GUILayout.Height(30)))
            {
                ImportTweeFile();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // SECTION 2: Batch Folder Import
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("2. Batch Folder Import (All Passages)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            tweeFolderPath = EditorGUILayout.TextField("Passages Folder", tweeFolderPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(75)))
            {
                string sel = EditorUtility.OpenFolderPanel("Select Twine Passages Folder", tweeFolderPath, "");
                if (!string.IsNullOrEmpty(sel)) tweeFolderPath = sel;
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f);
            if (GUILayout.Button("BATCH IMPORT ALL TWEE PASSAGES IN FOLDER", GUILayout.Height(34)))
            {
                BatchImportFolder(tweeFolderPath);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // SECTION 3: Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("3. Options & Configuration", EditorStyles.boldLabel);
            targetFolder = EditorGUILayout.TextField("Output Folder", targetFolder);
            overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
            autoLinkChoices = EditorGUILayout.Toggle("Auto-Link Choices to Passages", autoLinkChoices);
            autoAssignBackdrops = EditorGUILayout.Toggle("Auto-Assign Room Backdrops", autoAssignBackdrops);

            if (GUILayout.Button("Re-Link All Story Choices Now"))
            {
                AutoLinkAllPassageReferences(targetFolder, false);
            }
            EditorGUILayout.EndVertical();

            if (!string.IsNullOrEmpty(logOutput))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Importer Output Log:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(logOutput, GUILayout.Height(150));
            }

            EditorGUILayout.EndScrollView();
        }

        private class RawPassage
        {
            public string title;
            public string tags;
            public string content;
        }

        private static Dictionary<string, string> spritePathCache = null;

        private static void BuildSpriteCache()
        {
            if (spritePathCache != null && spritePathCache.Count > 0) return;
            spritePathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                string fn = Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
                if (!spritePathCache.ContainsKey(fn))
                {
                    spritePathCache[fn] = p;
                }
            }
        }

        private static Sprite LoadSpriteCached(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public void ImportTweeFile(bool silent = false)
        {
            if (!File.Exists(tweeFilePath))
            {
                if (!silent) EditorUtility.DisplayDialog("Error", "File not found: " + tweeFilePath, "OK");
                Debug.LogError("File not found: " + tweeFilePath);
                return;
            }

            try
            {
                BuildSpriteCache();
                string text = File.ReadAllText(tweeFilePath);
                List<RawPassage> rawPassages = ParseTweePassages(text);

                if (rawPassages.Count == 0)
                {
                    if (!silent) EditorUtility.DisplayDialog("Warning", "No story passages found in " + tweeFilePath, "OK");
                    Debug.LogWarning("No story passages found matching `:: PassageName` format.");
                    return;
                }

                Directory.CreateDirectory(targetFolder);
                int createdCount = 0;
                int updatedCount = 0;

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var rp in rawPassages)
                    {
                        ProcessSinglePassage(rp, ref createdCount, ref updatedCount);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                AssetDatabase.SaveAssets();

                if (autoLinkChoices)
                {
                    AutoLinkAllPassageReferences(targetFolder, true);
                }

                AssetDatabase.Refresh();

                logOutput = $"SUCCESSFULLY IMPORTED TWEE FILE!\n" +
                            $"Source File: {tweeFilePath}\n" +
                            $"Total Passages Found: {rawPassages.Count}\n" +
                            $"Created Assets: {createdCount}\n" +
                            $"Updated Assets: {updatedCount}\n" +
                            $"Output Folder: {targetFolder}";

                Debug.Log($"[TwineStoryImporter] {logOutput}");
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Import Complete", logOutput, "OK");
                }
            }
            catch (Exception ex)
            {
                if (!silent) EditorUtility.DisplayDialog("Import Error", "Failed to import Twee: " + ex.Message, "OK");
                Debug.LogException(ex);
            }
        }

        public void BatchImportFolder(string folderPath, bool silent = false)
        {
            if (!Directory.Exists(folderPath))
            {
                if (!silent) EditorUtility.DisplayDialog("Error", "Directory not found: " + folderPath, "OK");
                Debug.LogError("Directory not found: " + folderPath);
                return;
            }

            try
            {
                BuildSpriteCache();
                string[] files = Directory.GetFiles(folderPath, "*.twee", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    if (!silent) EditorUtility.DisplayDialog("Warning", "No .twee files found in " + folderPath, "OK");
                    return;
                }

                Directory.CreateDirectory(targetFolder);
                int totalPassages = 0;
                int createdCount = 0;
                int updatedCount = 0;
                int processedFiles = 0;

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        // Skip widget macro libraries or system files
                        if (fileName.Equals("Widget.twee", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals("StoryData.twee", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals("StoryTitle.twee", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string text = File.ReadAllText(file);
                        var passages = ParseTweePassages(text);
                        if (passages.Count == 0) continue;

                        processedFiles++;
                        totalPassages += passages.Count;

                        foreach (var rp in passages)
                        {
                            ProcessSinglePassage(rp, ref createdCount, ref updatedCount);
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                AssetDatabase.SaveAssets();

                if (autoLinkChoices)
                {
                    AutoLinkAllPassageReferences(targetFolder, true);
                }

                AssetDatabase.Refresh();

                logOutput = $"BATCH IMPORT COMPLETE!\n" +
                            $"Folder: {folderPath}\n" +
                            $"Files Processed: {processedFiles} of {files.Length}\n" +
                            $"Total Passages Found: {totalPassages}\n" +
                            $"Created Assets: {createdCount}\n" +
                            $"Updated Assets: {updatedCount}\n" +
                            $"Output Folder: {targetFolder}";

                Debug.Log($"[TwineStoryImporter] {logOutput}");
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Batch Import Complete", logOutput, "OK");
                }
            }
            catch (Exception ex)
            {
                if (!silent) EditorUtility.DisplayDialog("Batch Import Error", "Failed batch import: " + ex.Message, "OK");
                Debug.LogException(ex);
            }
        }

        private bool ProcessSinglePassage(RawPassage rp, ref int createdCount, ref int updatedCount)
        {
            string safeTitle = Regex.Replace(rp.title.Trim(), @"[^\w\-]", "_");
            string assetPath = Path.Combine(targetFolder, "Passage_" + safeTitle + ".asset").Replace('\\', '/');

            StoryPassageData passage = AssetDatabase.LoadAssetAtPath<StoryPassageData>(assetPath);
            bool isNew = (passage == null);
            if (isNew)
            {
                passage = CreateInstance<StoryPassageData>();
                AssetDatabase.CreateAsset(passage, assetPath);
                createdCount++;
            }
            else
            {
                if (!overwriteExisting) return false;
                updatedCount++;
            }

            passage.passageId = rp.title.Trim();
            passage.passageTitle = FormatPassageTitle(rp.title);

            // Extract Room ID from tags, content, or title
            string roomId = ExtractRoomId(rp.title, rp.tags, rp.content);
            if (!string.IsNullOrEmpty(roomId))
            {
                passage.associatedRoomId = roomId;
            }

            // Parse Content (Dialogue, Narrative beats, Choices, RPG requirements)
            ParsePassageContent(passage, rp.content);

            // Auto-assign backdrop sprite if needed
            if (autoAssignBackdrops && passage.backdropSprite == null)
            {
                passage.backdropSprite = ResolveBackdropSprite(passage.passageId, roomId, rp.content);
            }

            EditorUtility.SetDirty(passage);
            return true;
        }

        private static List<RawPassage> ParseTweePassages(string text)
        {
            var list = new List<RawPassage>();
            // Matches `:: PassageName [tags] {"metadata"}`
            var passageRegex = new Regex(@"^::\s*([^\{\[\r\n]+)(?:\[([^\]]*)\])?", RegexOptions.Multiline);
            var matches = passageRegex.Matches(text);

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                string title = match.Groups[1].Value.Trim();
                string tags = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "";

                int startIdx = match.Index + match.Length;
                // Move past metadata line if present
                int lineEnd = text.IndexOf('\n', startIdx);
                if (lineEnd != -1)
                {
                    string restOfLine = text.Substring(startIdx, lineEnd - startIdx);
                    if (restOfLine.TrimStart().StartsWith("{"))
                    {
                        startIdx = lineEnd + 1;
                    }
                }

                int endIdx = (i + 1 < matches.Count) ? matches[i + 1].Index : text.Length;
                string content = text.Substring(startIdx, endIdx - startIdx).Trim();

                list.Add(new RawPassage
                {
                    title = title,
                    tags = tags,
                    content = content
                });
            }

            return list;
        }

        private static string ExtractRoomId(string title, string tags, string content)
        {
            // 1. Check tags e.g. [unit_407], [unit_408], [hallway_1]
            if (!string.IsNullOrEmpty(tags))
            {
                string[] tagParts = tags.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in tagParts)
                {
                    string tl = t.ToLowerInvariant();
                    if (tl.Contains("407") || tl.Contains("safehouse")) return "unit_407";
                    if (tl.Contains("408")) return "unit_408";
                    if (tl.Contains("406")) return "unit_406";
                    if (tl.Contains("405")) return "unit_405";
                    if (tl.Contains("404")) return "unit_404";
                    if (tl.Contains("403")) return "unit_403";
                    if (tl.Contains("402")) return "unit_402";
                    if (tl.Contains("401")) return "unit_401";
                    if (tl.Contains("stair")) return "stairs";
                    if (tl.Contains("util")) return "utility";
                    if (tl.Contains("lobby")) return "lobby";
                    if (tl.Contains("hall") || tl.Contains("corr")) return "hallway_1";
                    return tl;
                }
            }

            // 2. Check Twine variable <<set $currentLocation to "..." >>
            var locMatch = Regex.Match(content, @"<<set\s+\$currentLocation\s+to\s+""([^""]+)""");
            if (locMatch.Success)
            {
                string loc = locMatch.Groups[1].Value.ToLowerInvariant();
                if (loc == "407") return "unit_407";
                if (loc == "408") return "unit_408";
                if (loc == "406") return "unit_406";
                if (loc == "405") return "unit_405";
                if (loc == "404") return "unit_404";
                if (loc == "403") return "unit_403";
                if (loc == "402") return "unit_402";
                if (loc == "401") return "unit_401";
                if (loc.Contains("lobby")) return "lobby";
                return loc;
            }

            // 3. Infer from Title
            string tlTitle = title.ToLowerInvariant();
            if (tlTitle.Contains("407") || tlTitle.Contains("safehouse")) return "unit_407";
            if (tlTitle.Contains("408")) return "unit_408";
            if (tlTitle.Contains("406")) return "unit_406";
            if (tlTitle.Contains("405")) return "unit_405";
            if (tlTitle.Contains("404")) return "unit_404";
            if (tlTitle.Contains("403")) return "unit_403";
            if (tlTitle.Contains("402")) return "unit_402";
            if (tlTitle.Contains("401")) return "unit_401";
            if (tlTitle.Contains("stair")) return "stairs";
            if (tlTitle.Contains("utility")) return "utility";
            if (tlTitle.Contains("lobby")) return "lobby";
            if (tlTitle.Contains("hallway") || tlTitle.Contains("corridor")) return "hallway_1";

            return "";
        }

        private static void ParsePassageContent(StoryPassageData passage, string content)
        {
            passage.dialogueLines.Clear();
            passage.choices.Clear();
            passage.narrativeBeats.Clear();

            // 1. Extract Standard Choices: [[Choice Text->Target]] or [[Choice Text|Target]] or [[Target]]
            var choiceRegex = new Regex(@"\[\[(?:([^\|\]\->]+)(?:->|\|))?([^\]]+)\]\]");
            var choiceMatches = choiceRegex.Matches(content);

            foreach (Match m in choiceMatches)
            {
                string label = m.Groups[1].Success && !string.IsNullOrEmpty(m.Groups[1].Value) ? m.Groups[1].Value.Trim() : m.Groups[2].Value.Trim();
                string target = m.Groups[2].Value.Trim();

                passage.choices.Add(new PassageChoiceData
                {
                    choiceLabel = label,
                    targetPassageId = target,
                    choiceSubtitle = ""
                });
            }

            // 2. Extract SugarCube Link / Button Choices:
            // <<link "Choice Text">> ... <<goto "TargetPassage">> ... <</link>>
            // Optional Subtitle: <span class="btn-sub">...</span>
            var sugarCubeLinkRegex = new Regex(@"<<link\s+""([^""]+)""[^>]*>([\s\S]*?)<</link>>");
            var scMatches = sugarCubeLinkRegex.Matches(content);

            foreach (Match m in scMatches)
            {
                string label = m.Groups[1].Value.Trim();
                string body = m.Groups[2].Value;

                var gotoMatch = Regex.Match(body, @"<<goto\s+""([^""]+)""");
                if (!gotoMatch.Success)
                {
                    gotoMatch = Regex.Match(body, @"<<goto\s+'([^']+)'");
                }

                string target = gotoMatch.Success ? gotoMatch.Groups[1].Value.Trim() : "";

                var choice = new PassageChoiceData
                {
                    choiceLabel = label,
                    targetPassageId = target,
                    choiceSubtitle = ""
                };

                // Time cost: <<addTime N>>
                var timeMatch = Regex.Match(body, @"<<addTime\s+(\d+)>>");
                if (timeMatch.Success && int.TryParse(timeMatch.Groups[1].Value, out int tCost))
                {
                    choice.timeMinutesCost = tCost;
                }

                // Stamina cost: <<exert N>>
                var exertMatch = Regex.Match(body, @"<<exert\s+([^\s>]+)>>");
                if (exertMatch.Success && int.TryParse(exertMatch.Groups[1].Value, out int sCost))
                {
                    choice.staminaCost = sCost;
                }

                // Item loot: setup.addItem("ItemName", Qty)
                var itemMatch = Regex.Match(body, @"setup\.addItem\(\s*""([^""]+)""(?:\s*,\s*(\d+))?\s*\)");
                if (itemMatch.Success)
                {
                    choice.givesItem = itemMatch.Groups[1].Value;
                    if (itemMatch.Groups[2].Success && int.TryParse(itemMatch.Groups[2].Value, out int qty))
                    {
                        choice.givesItemQty = qty;
                    }
                }

                // Subtitle context right after link: <span class="btn-sub">(.*?)</span>
                int linkEnd = m.Index + m.Length;
                int nextSearchLen = Mathf.Min(400, content.Length - linkEnd);
                if (nextSearchLen > 0)
                {
                    string afterLink = content.Substring(linkEnd, nextSearchLen);
                    var subMatch = Regex.Match(afterLink, @"<span\s+class=""btn-sub""[^>]*>([\s\S]*?)</span>");
                    if (subMatch.Success && subMatch.Index < 80) // Must belong to this button
                    {
                        string subText = Regex.Replace(subMatch.Groups[1].Value, @"<[^>]+>", "");
                        subText = Regex.Replace(subText, @"<<[^>]+>>", "");
                        choice.choiceSubtitle = subText.Trim();
                    }
                }

                // Check for Desperate QTE Minigame trigger
                if (label.IndexOf("[MINIGAME]", StringComparison.OrdinalIgnoreCase) >= 0 || 
                    body.IndexOf("Minigame_QTE", StringComparison.OrdinalIgnoreCase) >= 0 || 
                    body.IndexOf("qte-wrapper", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    choice.triggerActionId = "TRIGGER_QTE";
                    if (string.IsNullOrEmpty(choice.targetPassageId))
                    {
                        choice.targetPassageId = passage.passageId;
                    }

                    // Check if passage has a qte-win-trigger
                    var winMatch = Regex.Match(content, @"<span\s+id=""qte-win-trigger""[^>]*>([\s\S]*?)</span>");
                    if (winMatch.Success)
                    {
                        string winBody = winMatch.Groups[1].Value;
                        var flagMatch = Regex.Match(winBody, @"<<set\s+\$([a-zA-Z0-9_]+)\s+to\s+true>>");
                        if (flagMatch.Success)
                        {
                            choice.setsStoryFlagOnExecute = flagMatch.Groups[1].Value;
                        }

                        var winItemMatch = Regex.Match(winBody, @"setup\.addItem\(\s*""([^""]+)""(?:\s*,\s*(\d+))?\s*\)");
                        if (winItemMatch.Success)
                        {
                            choice.givesItem = winItemMatch.Groups[1].Value;
                            if (winItemMatch.Groups[2].Success && int.TryParse(winItemMatch.Groups[2].Value, out int wQty))
                            {
                                choice.givesItemQty = wQty;
                            }
                        }
                    }

                    var loseMatch = Regex.Match(content, @"<span\s+id=""qte-lose-trigger""[^>]*>([\s\S]*?)</span>");
                    if (loseMatch.Success)
                    {
                        string loseBody = loseMatch.Groups[1].Value;
                        var hpMatch = Regex.Match(loseBody, @"<<set\s+\$hp\s+-=\s*(\d+)>>");
                        if (hpMatch.Success && int.TryParse(hpMatch.Groups[1].Value, out int hpDmg))
                        {
                            choice.healthCost = hpDmg;
                        }
                    }
                }

                // Check for Lockpick Minigame trigger
                if (label.IndexOf("pick the lock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    label.IndexOf("pick lock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    label.IndexOf("pick gate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    label.IndexOf("pick evidence", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    choice.triggerActionId = "OPEN_LOCKPICK_MODAL";
                }

                // Extract Preceding Requirements (Trait / Job / Skill / Stat)
                int checkWindow = Mathf.Min(350, m.Index);
                if (checkWindow > 0)
                {
                    string preceding = content.Substring(m.Index - checkWindow, checkWindow);

                    // Trait check
                    var traitMatch = Regex.Match(preceding, @"hasTrait\(\s*""([^""]+)""\s*\)");
                    if (traitMatch.Success)
                    {
                        choice.requiredTrait = traitMatch.Groups[1].Value.ToUpperInvariant();
                    }

                    // Skill check
                    var skillMatch = Regex.Match(preceding, @"\$skills\.([a-zA-Z0-9_]+)\s*>=\s*(\d+)");
                    if (skillMatch.Success)
                    {
                        choice.requiredSkill = skillMatch.Groups[1].Value.ToLowerInvariant();
                        if (int.TryParse(skillMatch.Groups[2].Value, out int skVal))
                            choice.requiredSkillLevel = skVal;
                    }

                    // Stat check
                    var statMatch = Regex.Match(preceding, @"\$(str|per|int|agi|lck|cha)\s*>=\s*(\d+)");
                    if (statMatch.Success)
                    {
                        string statCode = statMatch.Groups[1].Value.ToLowerInvariant();
                        if (statCode == "str") choice.requiredStat = "Might";
                        else if (statCode == "per") choice.requiredStat = "Observation";
                        else if (statCode == "int") choice.requiredStat = "Tactics";
                        else if (statCode == "agi") choice.requiredStat = "Reflex";
                        else if (statCode == "cha") choice.requiredStat = "Leadership";
                        else if (statCode == "lck") choice.requiredStat = "Auspice";

                        if (int.TryParse(statMatch.Groups[2].Value, out int stVal))
                            choice.requiredStatValue = stVal;
                    }
                }

                passage.choices.Add(choice);
            }

            // 3. Extract Twine Chat macros: <<chat "Speaker" "avatar_path" "left/right" "Dialogue text">>
            var chatRegex = new Regex(@"<<chat\s+""([^""]*)""\s+""([^""]*)""\s+""([^""]*)""\s+""([^""]*)""\s*>>");
            var chatMatches = chatRegex.Matches(content);

            foreach (Match cm in chatMatches)
            {
                string speaker = cm.Groups[1].Value.Trim();
                string avatarRel = cm.Groups[2].Value.Trim();
                string sideStr = cm.Groups[3].Value.Trim().ToLowerInvariant();
                string text = cm.Groups[4].Value.Trim();

                Sprite resolvedAvatar = ResolveAvatarSprite(speaker, avatarRel);

                passage.dialogueLines.Add(new PassageDialogueLine
                {
                    speakerName = speaker,
                    side = (sideStr == "right") ? DialogueSide.Right : DialogueSide.Left,
                    dialogueText = text,
                    avatarSprite = resolvedAvatar
                });
            }

            // 4. Clean Content for narrative text: strip macros << ... >> and HTML tags < ... >
            string cleaned = Regex.Replace(content, @"<<[^>]+>>", "");
            cleaned = Regex.Replace(cleaned, @"\[\[[^\]]+\]\]", "");
            cleaned = Regex.Replace(cleaned, @"<br\s*/?>", "\n");
            cleaned = Regex.Replace(cleaned, @"</?[^>]+>", "");
            cleaned = cleaned.Trim();

            // Split into paragraphs for narrative beats
            string[] paragraphs = cleaned.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paragraphs.Length > 0)
            {
                passage.storyText = paragraphs[0].Trim();
                for (int i = 1; i < paragraphs.Length; i++)
                {
                    string p = paragraphs[i].Trim();
                    if (!string.IsNullOrEmpty(p))
                    {
                        passage.narrativeBeats.Add(p);
                    }
                }
            }
        }

        private static Sprite ResolveBackdropSprite(string passageId, string roomId, string rawContent)
        {
            // 1. Check for <img src="..."> in content
            var imgMatch = Regex.Match(rawContent, @"<img\s+[^>]*src=""([^""]+)""");
            if (imgMatch.Success)
            {
                string src = imgMatch.Groups[1].Value;
                string baseName = Path.GetFileNameWithoutExtension(src);
                Sprite found = FindSpriteByName(baseName);
                if (found != null) return found;
            }

            // 2. Try by passage ID (e.g. Unit_401 -> unit401)
            string normPassage = passageId.Replace("_", "").ToLowerInvariant();
            Sprite pSprite = FindSpriteByName(normPassage);
            if (pSprite != null) return pSprite;

            // 3. Try by room ID (e.g. unit_408 -> unit408)
            if (!string.IsNullOrEmpty(roomId))
            {
                string normRoom = roomId.Replace("_", "").ToLowerInvariant();
                Sprite rSprite = FindSpriteByName(normRoom);
                if (rSprite != null) return rSprite;
            }

            return null;
        }

        private static Sprite FindSpriteByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (spritePathCache != null && spritePathCache.TryGetValue(name, out string p))
            {
                return LoadSpriteCached(p);
            }

            return null;
        }

        private static Sprite ResolveAvatarSprite(string speaker, string avatarPath)
        {
            string name = !string.IsNullOrEmpty(speaker) ? speaker.ToLowerInvariant() : "";
            if (!string.IsNullOrEmpty(avatarPath))
            {
                string fn = Path.GetFileNameWithoutExtension(avatarPath).ToLowerInvariant();
                if (!string.IsNullOrEmpty(fn)) name = fn;
            }

            if (name.Contains("player") || name == "you") name = "player";
            else if (name.Contains("vane")) name = "vane";
            else if (name.Contains("diane")) name = "diane";
            else if (name.Contains("thorne")) name = "drthorne";
            else if (name.Contains("evelyn")) name = "matriarch_evelyn";
            else if (name.Contains("vance")) name = "quartermaster_sgtvance";
            else if (name.Contains("chloe")) name = "radio_chloe";

            if (spritePathCache != null)
            {
                if (spritePathCache.TryGetValue(name, out string p)) return LoadSpriteCached(p);
                foreach (var kvp in spritePathCache)
                {
                    if (kvp.Key.Contains(name)) return LoadSpriteCached(kvp.Value);
                }
            }

            return null;
        }

        public static void AutoLinkAllPassageReferences(string targetFolder, bool silent = false)
        {
            if (!Directory.Exists(targetFolder)) return;

            string[] guids = AssetDatabase.FindAssets("t:StoryPassageData", new[] { targetFolder });
            var passageMap = new Dictionary<string, StoryPassageData>(StringComparer.OrdinalIgnoreCase);
            var loadedList = new List<StoryPassageData>();

            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                StoryPassageData data = AssetDatabase.LoadAssetAtPath<StoryPassageData>(p);
                if (data != null && !string.IsNullOrEmpty(data.passageId))
                {
                    passageMap[data.passageId] = data;
                    // Also register by safe filename
                    string safeFn = Path.GetFileNameWithoutExtension(p).Replace("Passage_", "");
                    passageMap[safeFn] = data;
                    loadedList.Add(data);
                }
            }

            int linkedChoices = 0;
            foreach (var data in loadedList)
            {
                bool dirty = false;
                foreach (var choice in data.choices)
                {
                    if (choice == null) continue;

                    if (choice.targetPassage == null && !string.IsNullOrEmpty(choice.targetPassageId))
                    {
                        if (passageMap.TryGetValue(choice.targetPassageId, out var targetAsset))
                        {
                            choice.targetPassage = targetAsset;
                            if (string.IsNullOrEmpty(choice.targetRoomId) && !string.IsNullOrEmpty(targetAsset.associatedRoomId))
                            {
                                choice.targetRoomId = targetAsset.associatedRoomId;
                            }
                            dirty = true;
                            linkedChoices++;
                        }
                    }
                    else if (choice.targetPassage != null && string.IsNullOrEmpty(choice.targetRoomId))
                    {
                        if (!string.IsNullOrEmpty(choice.targetPassage.associatedRoomId))
                        {
                            choice.targetRoomId = choice.targetPassage.associatedRoomId;
                            dirty = true;
                        }
                    }
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(data);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[TwineStoryImporter] Auto-link complete. Linked {linkedChoices} passage choice targets.");
            if (!silent)
            {
                EditorUtility.DisplayDialog("Auto-Link Complete", $"Successfully linked {linkedChoices} choice targets across all story passages!", "OK");
            }
        }

        private static string FormatPassageTitle(string id)
        {
            string s = id.Replace("_", " ").Trim();
            return s.ToUpperInvariant();
        }
    }
}
