using System;
using System.Collections.Generic;
using UnityEngine;
using SundayLastBreath.Data;
using SundayLastBreath.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.Core
{
    /// <summary>
    /// Master Save & Load Controller utilizing Easy Save 3 (ES3).
    /// Responsible for persisting:
    /// 1. CharacterData (Identity, Job, M.O.R.T.A.L. Attributes, Perks/Traits, Vitals: HP, Stamina, Humanity, Inventory).
    /// 2. Player Location (Current Room ID in the Tactical Floor Map / Apartment Hub).
    /// 3. In-Game World Clock (Day, Hour, Minute, Date, Month, Year).
    /// 
    /// Includes developer shortcuts:
    /// - F5: QuickSave
    /// - F9: QuickLoad
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;

        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<SaveManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Easy Save 3 Settings")]
        [SerializeField] private string defaultSaveFileName = "QuickSave.es3";
        [SerializeField] private bool enableDebugShortcuts = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (instance == null)
            {
                _ = Instance;
            }
        }

        public string SaveFileName
        {
            get => defaultSaveFileName;
            set => defaultSaveFileName = value;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!enableDebugShortcuts) return;

            bool triggerSave = false;
            bool triggerLoad = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.f5Key.wasPressedThisFrame) triggerSave = true;
                if (Keyboard.current.f9Key.wasPressedThisFrame) triggerLoad = true;
            }
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F5)) triggerSave = true;
            if (Input.GetKeyDown(KeyCode.F9)) triggerLoad = true;
#endif

            if (triggerSave)
            {
                QuickSave();
            }
            else if (triggerLoad)
            {
                QuickLoad();
            }
        }

        /// <summary>
        /// Performs a QuickSave of the active game state to the default ES3 save file.
        /// </summary>
        [ContextMenu("QuickSave (F5)")]
        public bool QuickSave()
        {
            return SaveGame(defaultSaveFileName);
        }

        /// <summary>
        /// Performs a QuickLoad of the saved game state from the default ES3 save file.
        /// </summary>
        [ContextMenu("QuickLoad (F9)")]
        public bool QuickLoad()
        {
            return LoadGame(defaultSaveFileName);
        }

        /// <summary>
        /// Saves CharacterData, Current Room ID, and GameClock into Easy Save 3.
        /// </summary>
        public bool SaveGame(string fileName = null)
        {
            string file = string.IsNullOrEmpty(fileName) ? defaultSaveFileName : fileName;

            try
            {
                // 1. Resolve Character Data
                CharacterData character = null;
                if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
                {
                    character = GameManager.Instance.CurrentPlayer;
                }
                else if (ApartmentHubScreen.Instance != null && ApartmentHubScreen.Instance.CurrentPlayer != null)
                {
                    character = ApartmentHubScreen.Instance.CurrentPlayer;
                }

                // Fallback creation for direct test runs
                if (character == null)
                {
                    character = new CharacterData
                    {
                        firstName = "James",
                        lastName = "Miller",
                        job = "Survivor",
                        hp = 100,
                        hpMax = 100,
                        stamina = 100,
                        staminaMax = 100,
                        humanity = 45,
                        str = 4,
                        per = 5,
                        agi = 4,
                        intl = 5,
                        lck = 3,
                        cha = 4,
                        currentLocationId = "unit_407"
                    };
                    character.AddTrait("Cautious Scout", "High perception and alert senses.", null, false);
                }

                // 2. Resolve Current Room ID
                string roomId = "unit_407";
                if (ApartmentHubScreen.Instance != null && !string.IsNullOrEmpty(ApartmentHubScreen.Instance.CurrentRoomId))
                {
                    roomId = ApartmentHubScreen.Instance.CurrentRoomId;
                }
                else if (!string.IsNullOrEmpty(character.currentLocationId))
                {
                    roomId = character.currentLocationId;
                }
                character.currentLocationId = roomId;

                // 3. Resolve Game Clock
                GameClock clock = ApartmentHubScreen.Instance != null ? ApartmentHubScreen.Instance.Clock : null;
                int day = clock != null ? clock.DayCount : 0;
                int hour = clock != null ? clock.Hour : 23;
                int minute = clock != null ? clock.Minute : 45;
                int date = clock != null ? clock.Date : 25;
                string month = clock != null ? clock.Month : "OCT";
                int year = clock != null ? clock.Year : 2025;
                string dayName = clock != null ? clock.DayName : "SATURDAY";

                // 4. Easy Save 3 Persistence
                ES3.Save<CharacterData>("character_data", character, file);
                ES3.Save<string>("current_room_id", roomId, file);
                ES3.Save<int>("clock_day", day, file);
                ES3.Save<int>("clock_hour", hour, file);
                ES3.Save<int>("clock_minute", minute, file);
                ES3.Save<int>("clock_date", date, file);
                ES3.Save<string>("clock_month", month, file);
                ES3.Save<int>("clock_year", year, file);
                ES3.Save<string>("clock_day_name", dayName, file);

                // 5. Rich Console Telemetry Output
                string traitsList = (character.traits != null && character.traits.Count > 0)
                    ? string.Join(", ", character.traits.ConvertAll(t => t.name))
                    : "None";

                Debug.Log($"<color=#10B981><b>[Easy Save 3] QuickSave Successful! (F5)</b></color>\n" +
                          $"<color=#94A3B8>├─ File:</color> <b>{file}</b>\n" +
                          $"<color=#94A3B8>├─ Survivor:</color> <b>{character.FullName}</b> | Job: <color=#F59E0B>{character.job}</color>\n" +
                          $"<color=#94A3B8>├─ Vitals:</color> HP: <color=#EF4444>{character.hp}/{character.hpMax}</color> | Stamina: <color=#10B981>{character.stamina}/{character.staminaMax}</color> | Humanity: <color=#8B5CF6>{character.humanity}/100</color>\n" +
                          $"<color=#94A3B8>├─ M.O.R.T.A.L.:</color> M:{character.Might} O:{character.Observation} R:{character.Reflex} T:{character.Tactics} A:{character.Auspice} L:{character.Leadership}\n" +
                          $"<color=#94A3B8>├─ Traits/Perks ({character.traits?.Count ?? 0}):</color> <i>{traitsList}</i>\n" +
                          $"<color=#94A3B8>├─ Location:</color> <color=#38BDF8><b>{roomId.ToUpper()}</b></color>\n" +
                          $"<color=#94A3B8>└─ World Clock:</color> <b>DAY {day} ({dayName}) - {hour:D2}:{minute:D2}</b> ({month} {date}, {year})");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red><b>[Easy Save 3] SaveGame Failed!</b></color>\nException: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Loads CharacterData, Current Room ID, and GameClock from Easy Save 3 and updates active runtime systems.
        /// </summary>
        public bool LoadGame(string fileName = null)
        {
            string file = string.IsNullOrEmpty(fileName) ? defaultSaveFileName : fileName;

            if (!ES3.FileExists(file))
            {
                Debug.LogWarning($"<color=yellow>[Easy Save 3] QuickLoad Aborted: Save file '{file}' does not exist on disk.</color>");
                return false;
            }

            try
            {
                // 1. Load Character Data
                CharacterData loadedChar = null;
                if (ES3.KeyExists("character_data", file))
                {
                    loadedChar = ES3.Load<CharacterData>("character_data", file);
                }

                // Restore trait sprite references via ItemDatabase
                if (loadedChar != null && loadedChar.traits != null)
                {
                    foreach (var trait in loadedChar.traits)
                    {
                        if (trait.icon == null)
                        {
                            trait.icon = ItemDatabase.GetTraitIcon(trait.name);
                        }
                    }
                }

                // 2. Load Location
                string loadedRoomId = "unit_407";
                if (ES3.KeyExists("current_room_id", file))
                {
                    loadedRoomId = ES3.Load<string>("current_room_id", file);
                }

                // 3. Load World Clock
                int loadedDay = ES3.KeyExists("clock_day", file) ? ES3.Load<int>("clock_day", file) : 0;
                int loadedHour = ES3.KeyExists("clock_hour", file) ? ES3.Load<int>("clock_hour", file) : 23;
                int loadedMinute = ES3.KeyExists("clock_minute", file) ? ES3.Load<int>("clock_minute", file) : 45;
                int loadedDate = ES3.KeyExists("clock_date", file) ? ES3.Load<int>("clock_date", file) : 25;
                string loadedMonth = ES3.KeyExists("clock_month", file) ? ES3.Load<string>("clock_month", file) : "OCT";
                int loadedYear = ES3.KeyExists("clock_year", file) ? ES3.Load<int>("clock_year", file) : 2025;
                string loadedDayName = ES3.KeyExists("clock_day_name", file) ? ES3.Load<string>("clock_day_name", file) : "SATURDAY";

                // 4. Apply to Game State
                if (loadedChar != null)
                {
                    loadedChar.currentLocationId = loadedRoomId;

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SetCurrentPlayer(loadedChar);
                        GameManager.Instance.SetPlayerLocation(loadedRoomId);
                    }

                    if (ApartmentHubScreen.Instance != null)
                    {
                        ApartmentHubScreen.Instance.LoadCharacterState(loadedChar, loadedRoomId);
                    }
                }

                if (ApartmentHubScreen.Instance != null)
                {
                    ApartmentHubScreen.Instance.SetGameClockTime(loadedDay, loadedHour, loadedMinute, loadedDate, loadedMonth, loadedYear, loadedDayName);
                    ApartmentHubScreen.Instance.SetLocation(loadedRoomId);
                }

                // 5. Rich Console Telemetry Output
                string traitsList = (loadedChar != null && loadedChar.traits != null && loadedChar.traits.Count > 0)
                    ? string.Join(", ", loadedChar.traits.ConvertAll(t => t.name))
                    : "None";

                string charName = loadedChar != null ? loadedChar.FullName : "Unknown Survivor";
                string charJob = loadedChar != null ? loadedChar.job : "Civilian";
                int hp = loadedChar != null ? loadedChar.hp : 0;
                int hpMax = loadedChar != null ? loadedChar.hpMax : 0;
                int stm = loadedChar != null ? loadedChar.stamina : 0;
                int stmMax = loadedChar != null ? loadedChar.staminaMax : 0;
                int humanity = loadedChar != null ? loadedChar.humanity : 0;

                Debug.Log($"<color=#38BDF8><b>[Easy Save 3] QuickLoad Successful! (F9)</b></color>\n" +
                          $"<color=#94A3B8>├─ File:</color> <b>{file}</b>\n" +
                          $"<color=#94A3B8>├─ Survivor:</color> <b>{charName}</b> | Job: <color=#F59E0B>{charJob}</color>\n" +
                          $"<color=#94A3B8>├─ Vitals:</color> HP: <color=#EF4444>{hp}/{hpMax}</color> | Stamina: <color=#10B981>{stm}/{stmMax}</color> | Humanity: <color=#8B5CF6>{humanity}/100</color>\n" +
                          $"<color=#94A3B8>├─ M.O.R.T.A.L.:</color> M:{loadedChar?.Might ?? 0} O:{loadedChar?.Observation ?? 0} R:{loadedChar?.Reflex ?? 0} T:{loadedChar?.Tactics ?? 0} A:{loadedChar?.Auspice ?? 0} L:{loadedChar?.Leadership ?? 0}\n" +
                          $"<color=#94A3B8>├─ Traits/Perks ({loadedChar?.traits?.Count ?? 0}):</color> <i>{traitsList}</i>\n" +
                          $"<color=#94A3B8>├─ Location:</color> <color=#38BDF8><b>{loadedRoomId.ToUpper()}</b></color>\n" +
                          $"<color=#94A3B8>└─ World Clock:</color> <b>DAY {loadedDay} ({loadedDayName}) - {loadedHour:D2}:{loadedMinute:D2}</b> ({loadedMonth} {loadedDate}, {loadedYear})");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red><b>[Easy Save 3] LoadGame Failed!</b></color>\nException: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Checks whether a save file exists on disk.
        /// </summary>
        public bool HasSave(string fileName = null)
        {
            string file = string.IsNullOrEmpty(fileName) ? defaultSaveFileName : fileName;
            return ES3.FileExists(file);
        }

        /// <summary>
        /// Deletes the designated save file from disk.
        /// </summary>
        public void DeleteSave(string fileName = null)
        {
            string file = string.IsNullOrEmpty(fileName) ? defaultSaveFileName : fileName;
            if (ES3.FileExists(file))
            {
                ES3.DeleteFile(file);
                Debug.Log($"<color=yellow>[Easy Save 3] Deleted save file '{file}' from disk.</color>");
            }
        }

        [Serializable]
        public struct SaveSummary
        {
            public bool exists;
            public string survivorName;
            public string job;
            public int hp;
            public int hpMax;
            public int stamina;
            public int staminaMax;
            public int humanity;
            public string roomId;
            public int day;
            public int hour;
            public int minute;
            public string dateString;
            public int traitCount;
            public int itemCount;

            // UI helper alias properties
            public string characterName => !string.IsNullOrEmpty(survivorName) ? survivorName : "Unknown Survivor";
            public string saveTimestamp => !string.IsNullOrEmpty(dateString) ? dateString : "Recent";
            public string roomDisplayName => string.IsNullOrEmpty(roomId) ? "Unit 407" : roomId.Replace("_", " ").ToUpperInvariant();
            public string clockDisplay => $"DAY {day} // {hour:D2}:{minute:D2}";
        }

        /// <summary>
        /// Reads high-level meta telemetry of the save file without loading full game state into memory.
        /// </summary>
        public SaveSummary GetSaveSummary(string fileName = null)
        {
            string file = string.IsNullOrEmpty(fileName) ? defaultSaveFileName : fileName;
            SaveSummary summary = new SaveSummary();

            if (!ES3.FileExists(file))
            {
                summary.exists = false;
                return summary;
            }

            summary.exists = true;

            try
            {
                if (ES3.KeyExists("character_data", file))
                {
                    CharacterData cd = ES3.Load<CharacterData>("character_data", file);
                    if (cd != null)
                    {
                        summary.survivorName = cd.FullName;
                        summary.job = cd.job;
                        summary.hp = cd.hp;
                        summary.hpMax = cd.hpMax;
                        summary.stamina = cd.stamina;
                        summary.staminaMax = cd.staminaMax;
                        summary.humanity = cd.humanity;
                        summary.traitCount = cd.traits?.Count ?? 0;
                        summary.itemCount = cd.inventory?.Count ?? 0;
                    }
                }

                summary.roomId = ES3.KeyExists("current_room_id", file) ? ES3.Load<string>("current_room_id", file) : "unit_407";
                summary.day = ES3.KeyExists("clock_day", file) ? ES3.Load<int>("clock_day", file) : 0;
                summary.hour = ES3.KeyExists("clock_hour", file) ? ES3.Load<int>("clock_hour", file) : 23;
                summary.minute = ES3.KeyExists("clock_minute", file) ? ES3.Load<int>("clock_minute", file) : 45;
                string dayName = ES3.KeyExists("clock_day_name", file) ? ES3.Load<string>("clock_day_name", file) : "SATURDAY";
                summary.dateString = $"DAY {summary.day} ({dayName}) // {summary.hour:D2}:{summary.minute:D2}";
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Failed to read save summary: {ex.Message}");
            }

            return summary;
        }
    }
}

