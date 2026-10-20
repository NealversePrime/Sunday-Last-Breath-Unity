using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SundayLastBreath.Core;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Master UI controller managing the entire multi-step Character Creation flow:
    /// Unified Federal Bio-Emergency Citizen Intake Console -> Job Selection -> (Student Training/Major/Minor) -> Primary Hobby -> Secondary Hobby -> Weakness -> Final Dossier
    /// </summary>
    public partial class CharacterCreationScreen : ScreenBase
    {
        [Header("Audio")]
        [SerializeField] private AudioClip creationMusicClip;
        [SerializeField] private AudioClip clockTickClip;
        [SerializeField] private AudioSource clockTickSource;

        [Header("Panels")]
        [SerializeField] private GameObject genderPanel;
        [SerializeField] private GameObject firstNamePanel;
        [SerializeField] private GameObject lastNamePanel;
        [SerializeField] private GameObject jobSelectionPanel;
        [SerializeField] private GameObject studentTrainingPanel;
        [SerializeField] private GameObject studentMajorPanel;
        [SerializeField] private GameObject studentMinorPanel;
        [SerializeField] private GameObject hobbySelectionPanel;
        [SerializeField] private GameObject summaryDossierPanel;

        [Header("Gender Inputs")]
        [SerializeField] private TMP_InputField customGenderInput;

        [Header("First Name Inputs")]
        [SerializeField] private Transform maleNamesContainer;
        [SerializeField] private Transform femaleNamesContainer;
        [SerializeField] private Transform nonBinaryNamesContainer;
        [SerializeField] private TMP_InputField customFirstNameInput;

        [Header("Last Name Inputs")]
        [SerializeField] private TMP_InputField customLastNameInput;

        [Header("Student Training")]
        [SerializeField] private TextMeshProUGUI studentPointsText;
        [SerializeField] private TextMeshProUGUI studentStrText;
        [SerializeField] private TextMeshProUGUI studentIntText;
        [SerializeField] private TextMeshProUGUI studentAgiText;
        [SerializeField] private TextMeshProUGUI studentChaText;
        [SerializeField] private TextMeshProUGUI studentPerText;
        [SerializeField] private Button studentContinueButton;
        [SerializeField] private TextMeshProUGUI studentWarningText;

        [Header("Student Training Inspector")]
        [SerializeField] private TextMeshProUGUI inspectorStatTitle;
        [SerializeField] private TextMeshProUGUI inspectorStatQuote;
        [SerializeField] private TextMeshProUGUI inspectorStatDesc;
        [SerializeField] private TextMeshProUGUI inspectorStatImpact;

        [Header("Hobby UI")]
        [SerializeField] private TextMeshProUGUI hobbyHeaderTitle;
        [SerializeField] private TextMeshProUGUI hobbyHeaderSubtitle;
        [SerializeField] private Transform hobbyCardsContainer;
        [SerializeField] private GameObject hobbyCardPrefab;

        [Header("Thematic Category Tabs (Hobbies & Weaknesses)")]
        [SerializeField] private GameObject weaknessTabBar;
        [SerializeField] private Button physicalTabButton;
        [SerializeField] private Button mentalTabButton;
        [SerializeField] private Image physicalTabBg;
        [SerializeField] private Image mentalTabBg;
        [SerializeField] private TextMeshProUGUI physicalTabText;
        [SerializeField] private TextMeshProUGUI mentalTabText;
        [SerializeField] private Outline physicalTabOutline;
        [SerializeField] private Outline mentalTabOutline;

        private int activeWeaknessTab = 0; // 0 = Tab 1 (Action/Combat or Physical Flaws), 1 = Tab 2 (Mind/Tech/Support or Mental Flaws)

        private static readonly HashSet<string> ActionFieldHobbyIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "runner", "hunter", "boxer", "hiker", "archer", "baseball", "watchman", "parkour"
        };

        private static readonly HashSet<string> PhysicalWeaknessIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "smoker", "alcoholic", "asthmatic", "blind", "badknee", 
            "glutton", "fragile", "insomniac", "clumsy", "germophobe"
        };

        [Header("Summary Dossier UI")]
        [SerializeField] private TextMeshProUGUI summaryNameText;
        [SerializeField] private TextMeshProUGUI summaryIdentityText;
        [SerializeField] private TextMeshProUGUI summaryVitalsText;
        [SerializeField] private TextMeshProUGUI summaryAttributesText;
        [SerializeField] private TextMeshProUGUI summarySkillsText;
        [SerializeField] private TextMeshProUGUI summaryTraitsText;
        [SerializeField] private TextMeshProUGUI summaryInventoryText;
        [SerializeField] private Button beginSurvivalButton;
        [SerializeField] private Button restartCreationButton;

        // Preset Name Pools for the Unified Citizen Identity Console
        private static readonly string[] MaleFirstNames = {
            "James", "Michael", "Robert", "William", "David", "Neal",
            "Marcus", "Ethan", "Cole", "Lucas", "Nathan", "Jack"
        };

        private static readonly string[] FemaleFirstNames = {
            "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Nadia",
            "Claire", "Elena", "Sarah", "Maya", "Rachel", "Hannah"
        };

        private static readonly string[] NonBinaryFirstNames = {
            "Alex", "Jordan", "Taylor", "Quinn", "Casey", "Skyler",
            "Morgan", "Riley", "Avery", "Rowan", "Cameron", "Ellis"
        };

        private static readonly string[] PresetLastNames = {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Sage",
            "Miller", "Vance", "Mercer", "Holloway", "Sterling", "Callahan"
        };

        // Unified Identity Console runtime UI references
        private TextMeshProUGUI topBannerCaseIdText;
        private TextMeshProUGUI idCardLastNameText;
        private TextMeshProUGUI idCardFirstNameText;
        private TextMeshProUGUI idCardMetaText;
        private TextMeshProUGUI idCardMrzText;
        private TextMeshProUGUI idCardEmblemText;
        private Image idCardSilhouetteImg;
        private TextMeshProUGUI idCardQuoteText;
        private TextMeshProUGUI idCardActiveRecordText;
        private Button genderMaleBtn;
        private Button genderFemaleBtn;
        private Button genderNonBinaryBtn;
        private Transform firstNameChipsRoot;
        private Transform lastNameChipsRoot;
        private bool suppressInputCallback = false;

        private static Sprite _cachedSilhouetteSprite;

        // Hobby Top Bar runtime UI references
        private TextMeshProUGUI hobbyLoadoutStatusText;

        // Runtime creation state (tracked cleanly so Back navigation never duplicates stats/items)
        private CharacterData character = new CharacterData();
        private string selectedJobId = "police";
        private int studentAllocStr = 0;
        private int studentAllocInt = 0;
        private int studentAllocAgi = 0;
        private int studentAllocCha = 0;
        private int studentAllocPer = 0;
        private string chosenMajorSkill = "";
        private string chosenMinorSkill = "";
        private int hobbyStep = 1; // 1 = Primary, 2 = Secondary, 3 = Weakness
        private List<string> pickedHobbyIds = new List<string>();
        private int studentPointsRemaining = 5;
        private bool dossierLayoutEnforced = false;

        public CharacterData CreatedCharacter => character;

        protected override void Awake()
        {
            base.Awake();

            if (clockTickSource == null)
            {
                clockTickSource = gameObject.AddComponent<AudioSource>();
                clockTickSource.loop = true;
                clockTickSource.playOnAwake = false;
                clockTickSource.spatialBlend = 0f;
            }

            EnsureIdentityConsoleBuilt();
            EnsureJobBackButton();
            EnsureHobbyTopBar();
            InitializeButtons();
            SundayTypography.ApplyHierarchy(transform);
            HideAllPanels();
        }

        private void InitializeButtons()
        {
            // 1. Job Selection Cards (Interactive JobCardView with hover + click + upgraded badges)
            if (jobSelectionPanel != null)
            {
                Transform content = jobSelectionPanel.transform.Find("JobScrollView/Viewport/Content");
                if (content != null)
                {
                    foreach (var job in CharacterCreationDefinitions.Jobs)
                    {
                        Transform card = content.Find("JobCard_" + job.id);
                        if (card != null)
                        {
                            string jId = job.id;
                            JobData capturedJob = job;
                            JobCardView cardView = card.GetComponent<JobCardView>();
                            if (cardView == null) cardView = card.gameObject.AddComponent<JobCardView>();
                            cardView.Setup(capturedJob, () => SelectJob(jId));
                        }
                    }
                }
            }

            // 2. Student Training Buttons
            if (studentTrainingPanel != null)
            {
                Transform rows = null;
                var allChildren = studentTrainingPanel.GetComponentsInChildren<Transform>(true);
                foreach (var t in allChildren)
                {
                    if (t.name == "AttributesRows") { rows = t; break; }
                }

                if (rows != null)
                {
                    WireTrainingRow(rows, "Row_STRENGTH", "str");
                    WireTrainingRow(rows, "Row_INTELLIGENCE", "int");
                    WireTrainingRow(rows, "Row_AGILITY", "agi");
                    WireTrainingRow(rows, "Row_CHARISMA", "cha");
                    WireTrainingRow(rows, "Row_PERCEPTION", "per");
                }

                if (studentContinueButton != null)
                {
                    studentContinueButton.onClick.RemoveAllListeners();
                    studentContinueButton.onClick.AddListener(ConfirmStudentTraining);
                }
            }

            // 3. Student Major & Minor Buttons
            WireAcademicCards(studentMajorPanel, isMajor: true);
            WireAcademicCards(studentMinorPanel, isMajor: false);

            // 4. Summary Dossier Buttons
            if (beginSurvivalButton != null)
            {
                beginSurvivalButton.onClick.RemoveAllListeners();
                beginSurvivalButton.onClick.AddListener(OnBeginSurvivalClicked);
            }

            if (restartCreationButton != null)
            {
                restartCreationButton.onClick.RemoveAllListeners();
                restartCreationButton.onClick.AddListener(RestartCharacterCreation);
            }

            // 5. Thematic Tab Buttons (used across Primary Hobby, Secondary Hobby, and Weakness!)
            if (physicalTabButton != null)
            {
                physicalTabButton.onClick.RemoveAllListeners();
                physicalTabButton.onClick.AddListener(() => SwitchWeaknessTab(0));
            }
            if (mentalTabButton != null)
            {
                mentalTabButton.onClick.RemoveAllListeners();
                mentalTabButton.onClick.AddListener(() => SwitchWeaknessTab(1));
            }
        }

        private void WireTrainingRow(Transform rows, string rowName, string stat)
        {
            Transform row = rows.Find(rowName);
            if (row != null)
            {
                Button plusBtn = row.Find("Btn_Plus")?.GetComponent<Button>();
                if (plusBtn != null)
                {
                    plusBtn.onClick.RemoveAllListeners();
                    plusBtn.onClick.AddListener(() => AddStudentPoint(stat));
                }

                Button minusBtn = row.Find("Btn_Minus")?.GetComponent<Button>();
                if (minusBtn != null)
                {
                    minusBtn.onClick.RemoveAllListeners();
                    minusBtn.onClick.AddListener(() => RemoveStudentPoint(stat));
                }

                Button rowBtn = row.GetComponent<Button>();
                if (rowBtn != null)
                {
                    rowBtn.onClick.RemoveAllListeners();
                    rowBtn.onClick.AddListener(() =>
                    {
                        InspectStudentStat(stat, lockSelection: true);
                        PlayClick();
                    });
                }

                EventTrigger rowTrigger = row.gameObject.GetComponent<EventTrigger>();
                if (rowTrigger == null) rowTrigger = row.gameObject.AddComponent<EventTrigger>();
                rowTrigger.triggers.Clear();

                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => { OnRowPointerEnter(stat); });
                rowTrigger.triggers.Add(enterEntry);

                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => { OnRowPointerExit(stat); });
                rowTrigger.triggers.Add(exitEntry);

                if (plusBtn != null)
                {
                    EventTrigger plusTrigger = plusBtn.gameObject.GetComponent<EventTrigger>();
                    if (plusTrigger == null) plusTrigger = plusBtn.gameObject.AddComponent<EventTrigger>();
                    plusTrigger.triggers.Clear();
                    plusTrigger.triggers.Add(enterEntry);
                }

                if (minusBtn != null)
                {
                    EventTrigger minusTrigger = minusBtn.gameObject.GetComponent<EventTrigger>();
                    if (minusTrigger == null) minusTrigger = minusBtn.gameObject.AddComponent<EventTrigger>();
                    minusTrigger.triggers.Clear();
                    minusTrigger.triggers.Add(enterEntry);
                }
            }
        }

        private void Update()
        {
            if (studentTrainingPanel != null && studentTrainingPanel.activeInHierarchy && inspectLockTimer > 0f)
            {
                inspectLockTimer -= Time.deltaTime;
                if (inspectLockTimer <= 0f)
                {
                    inspectLockTimer = 0f;
                    if (!string.IsNullOrEmpty(currentlyHoveredStat) &&
                        !string.Equals(currentInspectedStat, currentlyHoveredStat, System.StringComparison.OrdinalIgnoreCase))
                    {
                        InspectStudentStat(currentlyHoveredStat, lockSelection: false);
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayButtonHover();
                        }
                    }
                }
            }

            if (summaryDossierPanel != null && summaryDossierPanel.activeInHierarchy && !dossierLayoutEnforced)
            {
                EnforceDossierLayout();
                dossierLayoutEnforced = true;
            }
        }

        protected override void OnScreenOpened()
        {
            ResetCreationState();

            if (AudioManager.Instance != null && creationMusicClip != null)
            {
                AudioManager.Instance.PlayBGM(creationMusicClip, 1.5f);
            }

            if (clockTickClip != null && clockTickSource != null)
            {
                clockTickSource.clip = clockTickClip;
                clockTickSource.volume = 0.25f;
                clockTickSource.Play();
            }

            EnsureIdentityConsoleBuilt();
            EnsureJobBackButton();
            EnsureHobbyTopBar();
            InitializeButtons();
            SundayTypography.ApplyHierarchy(transform);
            ShowGenderPanel();
        }

        private void ResetCreationState()
        {
            character = new CharacterData();
            character.gender = "Male";
            character.firstName = "Neal";
            character.lastName = "Sage";
            selectedJobId = "police";
            studentAllocStr = 0;
            studentAllocInt = 0;
            studentAllocAgi = 0;
            studentAllocCha = 0;
            studentAllocPer = 0;
            chosenMajorSkill = "";
            chosenMinorSkill = "";
            hobbyStep = 1;
            pickedHobbyIds.Clear();
            studentPointsRemaining = 5;
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            activeWeaknessTab = 0;
            dossierLayoutEnforced = false;
            ResetBargainState();
        }

        /// <summary>
        /// Computes deterministic biometric & federal case file metadata from the citizen's identity
        /// so Step 1 (Citizen Intake Console) and Step 4 (Final Summary Dossier) match 100%.
        /// </summary>
        private void GetCitizenBiometrics(out string docNo, out string dob, out string blood, out string sector, out string clearance, out string mrzLine1, out string mrzLine2)
        {
            string g = string.IsNullOrWhiteSpace(character.gender) ? "Male" : character.gender.Trim();
            string fn = string.IsNullOrWhiteSpace(character.firstName) ? "Neal" : character.firstName.Trim();
            string ln = string.IsNullOrWhiteSpace(character.lastName) ? "Sage" : character.lastName.Trim();

            bool isMale = g.Equals("Male", System.StringComparison.OrdinalIgnoreCase);
            bool isFemale = g.Equals("Female", System.StringComparison.OrdinalIgnoreCase);

            int hash = Mathf.Abs((fn.ToUpperInvariant() + "|" + ln.ToUpperInvariant() + "|" + g.ToUpperInvariant()).GetHashCode());
            int caseDigits = (hash % 9000) + 1000;
            docNo = $"#EAS-2026-US{caseDigits}";

            string[] bloodTypes = { "O-POS", "A-POS", "B-POS", "AB-POS", "O-NEG", "A-NEG" };
            blood = bloodTypes[hash % bloodTypes.Length];

            int birthMonth = (hash % 12) + 1;
            int birthDay = (hash % 28) + 1;
            dob = $"1999-{birthMonth:00}-{birthDay:00}";

            sector = "SECTOR-04 // METRO GATE";
            clearance = "TIER-1 CONDITIONAL";

            string cleanLast = System.Text.RegularExpressions.Regex.Replace(ln.ToUpperInvariant(), "[^A-Z]", "");
            string cleanFirst = System.Text.RegularExpressions.Regex.Replace(fn.ToUpperInvariant(), "[^A-Z]", "");
            if (string.IsNullOrEmpty(cleanLast)) cleanLast = "SURVIVOR";
            if (string.IsNullOrEmpty(cleanFirst)) cleanFirst = "UNKNOWN";

            mrzLine1 = $"IDUSA<<{cleanLast}<<{cleanFirst}".PadRight(40, '<').Substring(0, 40);
            string sexCode = isMale ? "M" : (isFemale ? "F" : "X");
            mrzLine2 = $"202610194USA99{birthMonth:00}{birthDay:00}7{sexCode}3010192<<<<EAS{caseDigits}".PadRight(40, '<').Substring(0, 40);
        }

        private void RebuildCharacterFromSelections()
        {
            string savedGender = string.IsNullOrWhiteSpace(character.gender) ? "Male" : character.gender;
            string savedFirst = string.IsNullOrWhiteSpace(character.firstName) ? "Neal" : character.firstName;
            string savedLast = string.IsNullOrWhiteSpace(character.lastName) ? "Sage" : character.lastName;

            character = new CharacterData
            {
                gender = savedGender,
                firstName = savedFirst,
                lastName = savedLast
            };

            JobData job = System.Array.Find(CharacterCreationDefinitions.Jobs, j => j.id.Equals(selectedJobId, System.StringComparison.OrdinalIgnoreCase));
            if (job != null)
            {
                character.job = job.title;
                character.str = job.str;
                character.intl = job.intl;
                character.agi = job.agi;
                character.cha = job.cha;
                character.per = job.per;
                character.lck = job.lck;

                character.hpMax += job.hpBonus;
                character.hp = character.hpMax;

                character.staminaMax += job.staminaBonus;
                character.baseStaminaMax += job.staminaBonus;
                character.stamina = character.staminaMax;

                character.humanity += job.humanityBonus;

                character.shooting = job.shooting;
                character.melee = job.melee;
                character.stealth = job.stealth;
                character.tech = job.tech;
                character.medicine = job.medicine;
                character.scavenging = job.scavenging;
                character.survival = job.survival;
                character.resolve = job.resolve;

                if (job.perks != null)
                {
                    foreach (var perk in job.perks)
                    {
                        Sprite icon = null;
#if UNITY_EDITOR
                        string iconKey = perk.name.ToLower().Replace(" ", "_");
                        icon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Perks/{iconKey}.png");
#endif
                        character.AddTrait(perk.name, perk.desc, icon);
                    }
                }

                if (job.startingItems != null)
                {
                    foreach (var item in job.startingItems)
                    {
                        character.AddItem(item.item, item.qty);
                    }
                }
            }

            if (selectedJobId.Equals("student", System.StringComparison.OrdinalIgnoreCase))
            {
                character.str += studentAllocStr;
                character.intl += studentAllocInt;
                character.agi += studentAllocAgi;
                character.cha += studentAllocCha;
                character.per += studentAllocPer;

                if (!string.IsNullOrEmpty(chosenMajorSkill))
                {
                    ApplyMajorToCharacter(chosenMajorSkill);
                }
                if (!string.IsNullOrEmpty(chosenMinorSkill))
                {
                    ApplyMinorToCharacter(chosenMinorSkill);
                }
            }

            for (int i = 0; i < pickedHobbyIds.Count; i++)
            {
                string hId = pickedHobbyIds[i];
                int stepNum = i + 1;
                HobbyData hobby = System.Array.Find(CharacterCreationDefinitions.PositiveHobbies, h => h.id.Equals(hId, System.StringComparison.OrdinalIgnoreCase));
                if (hobby == null)
                {
                    hobby = System.Array.Find(CharacterCreationDefinitions.DestructiveWeaknesses, w => w.id.Equals(hId, System.StringComparison.OrdinalIgnoreCase));
                }
                hobby?.applyEffect?.Invoke(character, stepNum);
            }

            // Apply Step 4: Survival Bargain Allocations & Drawbacks
            ApplyBargainToCharacter();
        }

        private void HideAllPanels()
        {
            if (genderPanel != null) genderPanel.SetActive(false);
            if (firstNamePanel != null) firstNamePanel.SetActive(false);
            if (lastNamePanel != null) lastNamePanel.SetActive(false);
            if (jobSelectionPanel != null) jobSelectionPanel.SetActive(false);
            if (studentTrainingPanel != null) studentTrainingPanel.SetActive(false);
            if (studentMajorPanel != null) studentMajorPanel.SetActive(false);
            if (studentMinorPanel != null) studentMinorPanel.SetActive(false);
            if (hobbySelectionPanel != null) hobbySelectionPanel.SetActive(false);
            if (bargainPanel != null) bargainPanel.SetActive(false);
            if (summaryDossierPanel != null) summaryDossierPanel.SetActive(false);
            dossierLayoutEnforced = false;
        }

        #region Step 1: Unified Federal Bio-Emergency Citizen Intake Console
        public void ShowGenderPanel()
        {
            HideAllPanels();
            EnsureIdentityConsoleBuilt();
            if (genderPanel != null) genderPanel.SetActive(true);
            RefreshIdentityConsoleUI(rebuildFirstNameChips: true);
        }

        public void SelectGender(string gender)
        {
            bool genderChanged = !string.Equals(character.gender, gender, System.StringComparison.OrdinalIgnoreCase);
            character.gender = gender;

            if (genderChanged && IsPresetFirstName(character.firstName))
            {
                string[] pool = GetFirstNamePoolForGender(gender);
                if (pool.Length > 0) character.firstName = pool[0];
            }

            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: true);
        }

        public void ConfirmCustomGender()
        {
            if (customGenderInput != null && !string.IsNullOrWhiteSpace(customGenderInput.text))
            {
                character.gender = customGenderInput.text.Trim();
            }
            else
            {
                character.gender = "Survivor";
            }
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: true);
        }

        public void SelectFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) return;
            character.firstName = firstName.Trim();
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
        }

        public void ConfirmCustomFirstName()
        {
            if (customFirstNameInput != null && !string.IsNullOrWhiteSpace(customFirstNameInput.text))
            {
                character.firstName = customFirstNameInput.text.Trim();
                PlayClick();
                RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
            }
        }

        public void SelectLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName)) return;
            character.lastName = lastName.Trim();
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
        }

        public void ConfirmCustomLastName()
        {
            if (customLastNameInput != null && !string.IsNullOrWhiteSpace(customLastNameInput.text))
            {
                character.lastName = customLastNameInput.text.Trim();
                PlayClick();
                RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
            }
        }

        public void ShowFirstNamePanel() => ShowGenderPanel();
        public void ShowLastNamePanel() => ShowJobSelectionPanel();

        public void RollRandomFirstName()
        {
            string[] pool = GetFirstNamePoolForGender(character.gender);
            if (pool.Length > 0)
            {
                string pick = pool[Random.Range(0, pool.Length)];
                if (pool.Length > 1 && pick == character.firstName)
                {
                    pick = pool[(System.Array.IndexOf(pool, pick) + 1) % pool.Length];
                }
                character.firstName = pick;
            }
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
        }

        public void RollRandomLastName()
        {
            if (PresetLastNames.Length > 0)
            {
                string pick = PresetLastNames[Random.Range(0, PresetLastNames.Length)];
                if (PresetLastNames.Length > 1 && pick == character.lastName)
                {
                    pick = PresetLastNames[(System.Array.IndexOf(PresetLastNames, pick) + 1) % PresetLastNames.Length];
                }
                character.lastName = pick;
            }
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
        }

        public void RandomizeNameOnly()
        {
            string[] pool = GetFirstNamePoolForGender(character.gender);
            if (pool.Length > 0) character.firstName = pool[Random.Range(0, pool.Length)];
            if (PresetLastNames.Length > 0) character.lastName = PresetLastNames[Random.Range(0, PresetLastNames.Length)];
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: false);
        }

        public void RandomizeFullIdentity()
        {
            string[] genders = { "Male", "Female", "Non-Binary" };
            character.gender = genders[Random.Range(0, genders.Length)];
            string[] pool = GetFirstNamePoolForGender(character.gender);
            if (pool.Length > 0) character.firstName = pool[Random.Range(0, pool.Length)];
            if (PresetLastNames.Length > 0) character.lastName = PresetLastNames[Random.Range(0, PresetLastNames.Length)];
            PlayClick();
            RefreshIdentityConsoleUI(rebuildFirstNameChips: true);
        }

        public void ConfirmIdentityAndProceed()
        {
            if (string.IsNullOrWhiteSpace(character.gender)) character.gender = "Male";
            if (string.IsNullOrWhiteSpace(character.firstName)) character.firstName = "Neal";
            if (string.IsNullOrWhiteSpace(character.lastName)) character.lastName = "Sage";
            PlayClick();
            ShowJobSelectionPanel();
        }

        private string[] GetFirstNamePoolForGender(string gender)
        {
            if (string.Equals(gender, "Male", System.StringComparison.OrdinalIgnoreCase)) return MaleFirstNames;
            if (string.Equals(gender, "Female", System.StringComparison.OrdinalIgnoreCase)) return FemaleFirstNames;
            return NonBinaryFirstNames;
        }

        private bool IsPresetFirstName(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            foreach (var n in MaleFirstNames) if (string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var n in FemaleFirstNames) if (string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var n in NonBinaryFirstNames) if (string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public void EnsureIdentityConsoleBuilt()
        {
            if (genderPanel == null) return;

            Transform existingFrame = genderPanel.transform.Find("IdentityConsoleFrame");
            if (existingFrame != null && existingFrame.Find("V3_AvatarMarker") != null)
            {
                BindIdentityConsoleReferences(existingFrame);
                return;
            }

            // Clear legacy or V1 objects
            for (int i = genderPanel.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = genderPanel.transform.GetChild(i);
                if (child.name != "Header")
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            // Update Top Screen Header Title & Subtitle
            Transform header = genderPanel.transform.Find("Header");
            if (header != null)
            {
                RectTransform hRect = header.GetComponent<RectTransform>();
                if (hRect != null) hRect.anchoredPosition = new Vector2(0, -28);

                TextMeshProUGUI tTmp = header.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (tTmp != null)
                {
                    tTmp.text = "CITIZEN IDENTITY & FEMA BIO-REGISTRY";
                    tTmp.fontSize = 28f;
                    SundayTypography.ApplyFont(tTmp, FontRole.Display);
                }
                TextMeshProUGUI sTmp = header.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
                if (sTmp != null)
                {
                    sTmp.text = "\"Before the sirens wailed across the county, who was the person staring back in the darkened glass?\"";
                    sTmp.fontSize = 15.5f;
                    SundayTypography.ApplyFont(sTmp, FontRole.Narrative);
                }
            }

            // Build Master Frame (1420 x 750) matching the exact Federal Dossier theme
            GameObject frameObj = new GameObject("IdentityConsoleFrame");
            frameObj.transform.SetParent(genderPanel.transform, false);
            RectTransform fRect = frameObj.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0.5f);
            fRect.anchorMax = new Vector2(0.5f, 0.5f);
            fRect.pivot = new Vector2(0.5f, 0.5f);
            fRect.sizeDelta = new Vector2(1420f, 750f);
            fRect.anchoredPosition = new Vector2(0f, -36f);

            GameObject v3Marker = new GameObject("V3_AvatarMarker");
            v3Marker.transform.SetParent(frameObj.transform, false);

            Image fBg = frameObj.AddComponent<Image>();
            fBg.color = new Color(0.05f, 0.07f, 0.10f, 0.98f);
            Outline fOut = frameObj.AddComponent<Outline>();
            fOut.effectColor = new Color(0.18f, 0.28f, 0.40f, 0.85f);
            fOut.effectDistance = new Vector2(2f, -2f);
            frameObj.AddComponent<RectMask2D>();

            // Top Federal Header Banner (52px, matching Final Dossier HeaderBar!)
            GameObject topStrip = new GameObject("TopBanner");
            topStrip.transform.SetParent(frameObj.transform, false);
            RectTransform tsRect = topStrip.AddComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0f, 1f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.pivot = new Vector2(0.5f, 1f);
            tsRect.sizeDelta = new Vector2(0f, 48f);
            tsRect.anchoredPosition = Vector2.zero;
            topStrip.AddComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 1f);

            GameObject goldBottomLine = new GameObject("GoldDivider");
            goldBottomLine.transform.SetParent(topStrip.transform, false);
            RectTransform gblRect = goldBottomLine.AddComponent<RectTransform>();
            gblRect.anchorMin = new Vector2(0f, 0f);
            gblRect.anchorMax = new Vector2(1f, 0f);
            gblRect.pivot = new Vector2(0.5f, 0f);
            gblRect.sizeDelta = new Vector2(0f, 2f);
            gblRect.anchoredPosition = Vector2.zero;
            goldBottomLine.AddComponent<Image>().color = new Color(0.85f, 0.68f, 0.25f, 1f);

            CreateUIText(topStrip.transform, "BannerLeft",
                "<b><color=#F5BF42>■ FEDERAL BIO-EMERGENCY AGENCY</color></b>   <color=#78909C>|</color>   <color=#8CC2E8>CDC · FEMA JOINT OUTBREAK RESPONSE TASKFORCE // STEP 01: CITIZEN INTAKE</color>",
                12f, TextAlignmentOptions.MidlineLeft, Color.white, FontRole.Mono,
                new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(18f, 0f), Vector2.zero);

            CreateUIText(topStrip.transform, "BannerRight",
                "CASE FILE: #EAS-2026-US4349   |   <color=#EF5350><b>[ STATUS: CIVIL DANGER WARNING // LEVEL 4 ]</b></color>",
                11.5f, TextAlignmentOptions.MidlineRight, new Color(0.75f, 0.84f, 0.92f), FontRole.Mono,
                new Vector2(0.62f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-18f, 0f));

            // Console Body Area (1380 x 624)
            GameObject bodyObj = new GameObject("ConsoleBody");
            bodyObj.transform.SetParent(frameObj.transform, false);
            RectTransform bRect = bodyObj.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(20f, 70f);
            bRect.offsetMax = new Vector2(-20f, -54f);

            // =========================================================================
            // LEFT COLUMN: Live Interactive Federal Bio-Emergency Citizen ID + Mirror Box
            // =========================================================================
            GameObject leftCol = new GameObject("LeftCol_CitizenCard");
            leftCol.transform.SetParent(bodyObj.transform, false);
            RectTransform lcRect = leftCol.AddComponent<RectTransform>();
            lcRect.anchorMin = new Vector2(0f, 0f);
            lcRect.anchorMax = new Vector2(0f, 1f);
            lcRect.pivot = new Vector2(0f, 1f);
            lcRect.sizeDelta = new Vector2(500f, 0f);
            lcRect.anchoredPosition = Vector2.zero;

            // 1. Federal Citizen Bio-ID Card (500 x 324)
            GameObject idCard = new GameObject("USCitizenIDCard");
            idCard.transform.SetParent(leftCol.transform, false);
            RectTransform idRect = idCard.AddComponent<RectTransform>();
            idRect.anchorMin = new Vector2(0f, 1f);
            idRect.anchorMax = new Vector2(1f, 1f);
            idRect.pivot = new Vector2(0.5f, 1f);
            idRect.sizeDelta = new Vector2(0f, 324f);
            idRect.anchoredPosition = new Vector2(0f, -4f);
            idCard.AddComponent<Image>().color = new Color(0.05f, 0.09f, 0.15f, 0.98f);
            Outline idOut = idCard.AddComponent<Outline>();
            idOut.effectColor = new Color(0.18f, 0.34f, 0.52f, 0.95f);
            idOut.effectDistance = new Vector2(1f, -1f);
            idCard.AddComponent<RectMask2D>();

            // ID Card Header Bar (34px)
            GameObject idHeader = new GameObject("IDHeader");
            idHeader.transform.SetParent(idCard.transform, false);
            RectTransform idhRect = idHeader.AddComponent<RectTransform>();
            idhRect.anchorMin = new Vector2(0f, 1f);
            idhRect.anchorMax = new Vector2(1f, 1f);
            idhRect.pivot = new Vector2(0.5f, 1f);
            idhRect.sizeDelta = new Vector2(0f, 34f);
            idhRect.anchoredPosition = Vector2.zero;
            idHeader.AddComponent<Image>().color = new Color(0.08f, 0.16f, 0.28f, 1f);

            GameObject idGoldBar = new GameObject("GoldTopLine");
            idGoldBar.transform.SetParent(idHeader.transform, false);
            RectTransform igbRect = idGoldBar.AddComponent<RectTransform>();
            igbRect.anchorMin = new Vector2(0f, 1f);
            igbRect.anchorMax = new Vector2(1f, 1f);
            igbRect.pivot = new Vector2(0.5f, 1f);
            igbRect.sizeDelta = new Vector2(0f, 3f);
            igbRect.anchoredPosition = Vector2.zero;
            idGoldBar.AddComponent<Image>().color = new Color(0.96f, 0.75f, 0.26f, 1f);

            CreateUIText(idHeader.transform, "IDHeaderLeft", "<b>■ FEMA · CDC JOINT BIO-REGISTRY // CITIZEN ID PASS</b>",
                12f, TextAlignmentOptions.MidlineLeft, new Color(0.55f, 0.85f, 0.98f), FontRole.Display,
                new Vector2(0f, 0f), new Vector2(0.72f, 1f), new Vector2(14f, 0f), Vector2.zero);

            CreateUIText(idHeader.transform, "IDHeaderRight", "<b>DAY ZERO 23:45</b>",
                10.5f, TextAlignmentOptions.MidlineRight, new Color(1f, 0.88f, 0.50f), FontRole.Mono,
                new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-14f, 0f));

            // Portrait Box (126 x 162 at top-left of card body)
            GameObject portraitBox = new GameObject("PortraitBox");
            portraitBox.transform.SetParent(idCard.transform, false);
            RectTransform pbRect = portraitBox.AddComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0f, 1f);
            pbRect.anchorMax = new Vector2(0f, 1f);
            pbRect.pivot = new Vector2(0f, 1f);
            pbRect.sizeDelta = new Vector2(126f, 162f);
            pbRect.anchoredPosition = new Vector2(18f, -46f);
            portraitBox.AddComponent<Image>().color = new Color(0.03f, 0.05f, 0.09f, 1f);
            Outline pbOut = portraitBox.AddComponent<Outline>();
            pbOut.effectColor = new Color(0.20f, 0.45f, 0.68f, 0.85f);
            pbOut.effectDistance = new Vector2(1f, -1f);
            portraitBox.AddComponent<RectMask2D>();

            CreateUIText(portraitBox.transform, "ReticleTop", "[ BIO-SCAN ]",
                9.5f, TextAlignmentOptions.Center, new Color(0.35f, 0.75f, 0.95f, 0.85f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(0f, -4f));

            // Survivor Glitch Avatar Graphic inside PortraitBox
            GameObject silObj = new GameObject("SilhouetteGraphic");
            silObj.transform.SetParent(portraitBox.transform, false);
            RectTransform silRect = silObj.AddComponent<RectTransform>();
            silRect.anchorMin = new Vector2(0.5f, 0.5f);
            silRect.anchorMax = new Vector2(0.5f, 0.5f);
            silRect.pivot = new Vector2(0.5f, 0.5f);
            silRect.sizeDelta = new Vector2(104f, 96f);
            silRect.anchoredPosition = new Vector2(0f, 10f); // Placed with clear margin above [MALE] badge
            Image silImg = silObj.AddComponent<Image>();
            silImg.sprite = GetOrCreateSilhouetteSprite();
            silImg.preserveAspect = true;
            silImg.color = Color.white;
            silImg.raycastTarget = false;

            // Animate glitch avatar if frame sequence is available
            List<Sprite> playerFrames = LoadPlayerFrames();
            if (playerFrames != null && playerFrames.Count > 0)
            {
                FrameAnimationPlayer animPlayer = silObj.AddComponent<FrameAnimationPlayer>();
                animPlayer.SetFrames(playerFrames, 12f, true);
                animPlayer.Play();
            }

            // Subject / Gender badge placed cleanly below avatar photo
            CreateUIText(portraitBox.transform, "EmblemSymbol", "<b>[MALE]</b>",
                9.5f, TextAlignmentOptions.Center, new Color(1f, 0.88f, 0.50f, 0.95f), FontRole.Mono,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 26f), new Vector2(0f, 42f));

            // Verification status pill at bottom
            GameObject scanPill = new GameObject("BioScanPill");
            scanPill.transform.SetParent(portraitBox.transform, false);
            RectTransform spRect = scanPill.AddComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0f, 0f);
            spRect.anchorMax = new Vector2(1f, 0f);
            spRect.pivot = new Vector2(0.5f, 0f);
            spRect.sizeDelta = new Vector2(-12f, 18f);
            spRect.anchoredPosition = new Vector2(0f, 5f);
            scanPill.AddComponent<Image>().color = new Color(0.06f, 0.24f, 0.14f, 0.95f);

            CreateUIText(scanPill.transform, "ScanText", "<b>✓ TIER-1 CLEAR</b>",
                8.5f, TextAlignmentOptions.Center, new Color(0.30f, 0.92f, 0.55f), FontRole.Mono,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Live Identity Data Fields (Right of Portrait)
            GameObject fieldsRoot = new GameObject("LiveFields");
            fieldsRoot.transform.SetParent(idCard.transform, false);
            RectTransform frRect = fieldsRoot.AddComponent<RectTransform>();
            frRect.anchorMin = new Vector2(0f, 1f);
            frRect.anchorMax = new Vector2(1f, 1f);
            frRect.pivot = new Vector2(0f, 1f);
            frRect.offsetMin = new Vector2(160f, -214f);
            frRect.offsetMax = new Vector2(-18f, -44f);

            CreateUIText(fieldsRoot.transform, "LblSurname", "01 // FAMILY SURNAME",
                9.5f, TextAlignmentOptions.BottomLeft, new Color(0.45f, 0.60f, 0.72f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -14f), new Vector2(0f, 0f));

            CreateUIText(fieldsRoot.transform, "ValSurname", "SAGE",
                23f, TextAlignmentOptions.MidlineLeft, Color.white, FontRole.Display,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -42f), new Vector2(0f, -15f));

            CreateUIText(fieldsRoot.transform, "LblGiven", "02 // GIVEN FIRST NAME",
                9.5f, TextAlignmentOptions.BottomLeft, new Color(0.45f, 0.60f, 0.72f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -58f), new Vector2(0f, -44f));

            CreateUIText(fieldsRoot.transform, "ValGiven", "NEAL",
                20f, TextAlignmentOptions.MidlineLeft, new Color(1f, 0.85f, 0.45f), FontRole.Display,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -84f), new Vector2(0f, -59f));

            GameObject idDiv = new GameObject("FieldDivider");
            idDiv.transform.SetParent(fieldsRoot.transform, false);
            RectTransform iddRect = idDiv.AddComponent<RectTransform>();
            iddRect.anchorMin = new Vector2(0f, 1f);
            iddRect.anchorMax = new Vector2(1f, 1f);
            iddRect.pivot = new Vector2(0.5f, 1f);
            iddRect.sizeDelta = new Vector2(0f, 1f);
            iddRect.anchoredPosition = new Vector2(0f, -90f);
            idDiv.AddComponent<Image>().color = new Color(0.16f, 0.28f, 0.42f, 0.85f);

            CreateUIText(fieldsRoot.transform, "ValMetaGrid", "",
                10.5f, TextAlignmentOptions.TopLeft, new Color(0.82f, 0.88f, 0.94f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -168f), new Vector2(0f, -96f));

            // Bottom Machine-Readable Barcode Zone (MRZStrip, height 88px)
            GameObject mrzStrip = new GameObject("MRZStrip");
            mrzStrip.transform.SetParent(idCard.transform, false);
            RectTransform mrzRect = mrzStrip.AddComponent<RectTransform>();
            mrzRect.anchorMin = new Vector2(0f, 0f);
            mrzRect.anchorMax = new Vector2(1f, 0f);
            mrzRect.pivot = new Vector2(0.5f, 0f);
            mrzRect.sizeDelta = new Vector2(0f, 88f);
            mrzRect.anchoredPosition = Vector2.zero;
            mrzStrip.AddComponent<Image>().color = new Color(0.03f, 0.05f, 0.08f, 1f);

            GameObject mrzLine = new GameObject("TopLine");
            mrzLine.transform.SetParent(mrzStrip.transform, false);
            RectTransform mlRect = mrzLine.AddComponent<RectTransform>();
            mlRect.anchorMin = new Vector2(0f, 1f);
            mlRect.anchorMax = new Vector2(1f, 1f);
            mlRect.pivot = new Vector2(0.5f, 1f);
            mlRect.sizeDelta = new Vector2(0f, 1f);
            mlRect.anchoredPosition = Vector2.zero;
            mrzLine.AddComponent<Image>().color = new Color(0.16f, 0.28f, 0.42f, 0.9f);

            CreateUIText(mrzStrip.transform, "BarcodeBars",
                "||| || |||| | ||| |||| || | ||| |||| ||| || | |||| ||| | |||| || | ||| ||||",
                11.5f, TextAlignmentOptions.Center, new Color(0.35f, 0.65f, 0.88f, 0.55f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -26f), new Vector2(-16f, -5f));

            CreateUIText(mrzStrip.transform, "ValMrzCode", "",
                10.5f, TextAlignmentOptions.Center, new Color(0.38f, 0.78f, 0.98f, 0.92f), FontRole.Mono,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 6f), new Vector2(-16f, -28f));

            // 2. Psychological Mirror Reflection & Randomizer Box (500 x 284 at bottom of Left Column)
            GameObject mirrorBox = new GameObject("MirrorProfileBox");
            mirrorBox.transform.SetParent(leftCol.transform, false);
            RectTransform mbRect = mirrorBox.AddComponent<RectTransform>();
            mbRect.anchorMin = new Vector2(0f, 0f);
            mbRect.anchorMax = new Vector2(1f, 0f);
            mbRect.pivot = new Vector2(0.5f, 0f);
            mbRect.sizeDelta = new Vector2(0f, 284f);
            mbRect.anchoredPosition = new Vector2(0f, 4f);
            mirrorBox.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.12f, 0.96f);
            Outline mbOut = mirrorBox.AddComponent<Outline>();
            mbOut.effectColor = new Color(0.15f, 0.24f, 0.35f, 0.85f);
            mbOut.effectDistance = new Vector2(1f, -1f);
            mirrorBox.AddComponent<RectMask2D>();

            CreateModuleHeader(mirrorBox.transform, "■ MIRROR REFLECTION & SURVIVOR PROFILE");

            // Properly wrapped inside MirrorProfileBox (anchor 0..1, offsetMin (18, 114), offsetMax (-18, -36))
            TextMeshProUGUI quoteTmp = CreateUIText(mirrorBox.transform, "ValMirrorQuote", "",
                13f, TextAlignmentOptions.TopLeft, new Color(0.82f, 0.86f, 0.90f), FontRole.Narrative,
                Vector2.zero, Vector2.one, new Vector2(18f, 112f), new Vector2(-18f, -38f));
            quoteTmp.lineSpacing = 4f;
            quoteTmp.textWrappingMode = TextWrappingModes.Normal;
            quoteTmp.overflowMode = TextOverflowModes.Ellipsis;

            GameObject activePill = new GameObject("ActiveRecordPill");
            activePill.transform.SetParent(mirrorBox.transform, false);
            RectTransform apRect = activePill.AddComponent<RectTransform>();
            apRect.anchorMin = new Vector2(0f, 0f);
            apRect.anchorMax = new Vector2(1f, 0f);
            apRect.pivot = new Vector2(0.5f, 0f);
            apRect.sizeDelta = new Vector2(-32f, 34f);
            apRect.anchoredPosition = new Vector2(0f, 66f);
            activePill.AddComponent<Image>().color = new Color(0.03f, 0.06f, 0.10f, 0.95f);
            Outline apOut = activePill.AddComponent<Outline>();
            apOut.effectColor = new Color(0.18f, 0.36f, 0.52f, 0.7f);
            apOut.effectDistance = new Vector2(1f, -1f);

            CreateUIText(activePill.transform, "ValActiveRecord", "ACTIVE CITIZEN: NEAL SAGE  [MALE]",
                11f, TextAlignmentOptions.Center, new Color(0.50f, 0.84f, 0.98f), FontRole.Mono,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Randomize Buttons Row at bottom of MirrorProfileBox (using clean [RNG] tag instead of missing glyph)
            CreateTacticalActionButton(mirrorBox.transform, "Btn_RandomName", "[RNG] RANDOM NAME",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 14f), new Vector2(226f, 42f),
                new Color(0.08f, 0.20f, 0.30f, 0.98f), new Color(0.20f, 0.60f, 0.86f, 0.85f), RandomizeNameOnly);

            CreateTacticalActionButton(mirrorBox.transform, "Btn_RandomAll", "[RNG] RANDOM ALL",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(226f, 42f),
                new Color(0.26f, 0.14f, 0.05f, 0.98f), new Color(0.90f, 0.49f, 0.13f, 0.85f), RandomizeFullIdentity, pivotX: 1f);

            // =========================================================================
            // RIGHT COLUMN: Interactive Selection Modules (Gender, First Name, Surname)
            // =========================================================================
            GameObject rightCol = new GameObject("RightCol_Controls");
            rightCol.transform.SetParent(bodyObj.transform, false);
            RectTransform rcRect = rightCol.AddComponent<RectTransform>();
            rcRect.anchorMin = new Vector2(1f, 0f);
            rcRect.anchorMax = new Vector2(1f, 1f);
            rcRect.pivot = new Vector2(1f, 1f);
            rcRect.sizeDelta = new Vector2(860f, 0f);
            rcRect.anchoredPosition = Vector2.zero;

            // MODULE 01: GENDER IDENTITY (860 x 146 at Y = -4)
            GameObject modGender = CreateControlModuleBox(rightCol.transform, "Module01_Gender", -4f, 146f,
                "■ 01 // BIOLOGICAL SEX & GENDER IDENTITY");

            CreateGenderArchetypeButton(modGender.transform, "Btn_Gender_Male", "[M]  MALE", "Standard Male Profile", 16f, () => SelectGender("Male"));
            CreateGenderArchetypeButton(modGender.transform, "Btn_Gender_Female", "[F]  FEMALE", "Standard Female Profile", 296f, () => SelectGender("Female"));
            CreateGenderArchetypeButton(modGender.transform, "Btn_Gender_NonBinary", "[NB] NON-BINARY", "Non-Binary / Fluid Profile", 576f, () => SelectGender("Non-Binary"));

            CreateInlineCustomInput(modGender.transform, "CustomGenderRow", "CUSTOM IDENTITY:", "Or type custom identity (e.g. Survivor)...",
                -102f, "APPLY", ConfirmCustomGender, out customGenderInput, (val) =>
                {
                    if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                    {
                        character.gender = val.Trim();
                        RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                    }
                });

            // MODULE 02: GIVEN FIRST NAME (860 x 226 at Y = -162)
            GameObject modFirst = CreateControlModuleBox(rightCol.transform, "Module02_FirstName", -162f, 226f,
                "■ 02 // GIVEN NAME (FIRST NAME) — CLICK PRESET CHIP OR TYPE CUSTOM");

            GameObject fnGridObj = new GameObject("FirstNameChipsGrid");
            fnGridObj.transform.SetParent(modFirst.transform, false);
            RectTransform fngRect = fnGridObj.AddComponent<RectTransform>();
            fngRect.anchorMin = new Vector2(0f, 1f);
            fngRect.anchorMax = new Vector2(1f, 1f);
            fngRect.pivot = new Vector2(0.5f, 1f);
            fngRect.sizeDelta = new Vector2(-32f, 132f);
            fngRect.anchoredPosition = new Vector2(0f, -38f);
            GridLayoutGroup fnGlg = fnGridObj.AddComponent<GridLayoutGroup>();
            fnGlg.cellSize = new Vector2(196f, 36f);
            fnGlg.spacing = new Vector2(14f, 8f);
            fnGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            fnGlg.constraintCount = 4;
            fnGlg.childAlignment = TextAnchor.UpperCenter;

            CreateInlineCustomInput(modFirst.transform, "CustomFirstNameRow", "TYPE FIRST NAME:", "Type custom given name...",
                -180f, "[RNG] ROLL FIRST", RollRandomFirstName, out customFirstNameInput, (val) =>
                {
                    if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                    {
                        character.firstName = val.Trim();
                        RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                    }
                });

            // MODULE 03: FAMILY SURNAME (860 x 226 at Y = -400)
            GameObject modLast = CreateControlModuleBox(rightCol.transform, "Module03_LastName", -400f, 226f,
                "■ 03 // FAMILY SURNAME (LAST NAME) — CLICK PRESET CHIP OR TYPE CUSTOM");

            GameObject lnGridObj = new GameObject("LastNameChipsGrid");
            lnGridObj.transform.SetParent(modLast.transform, false);
            RectTransform lngRect = lnGridObj.AddComponent<RectTransform>();
            lngRect.anchorMin = new Vector2(0f, 1f);
            lngRect.anchorMax = new Vector2(1f, 1f);
            lngRect.pivot = new Vector2(0.5f, 1f);
            lngRect.sizeDelta = new Vector2(-32f, 132f);
            lngRect.anchoredPosition = new Vector2(0f, -38f);
            GridLayoutGroup lnGlg = lnGridObj.AddComponent<GridLayoutGroup>();
            lnGlg.cellSize = new Vector2(196f, 36f);
            lnGlg.spacing = new Vector2(14f, 8f);
            lnGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            lnGlg.constraintCount = 4;
            lnGlg.childAlignment = TextAnchor.UpperCenter;

            foreach (string surname in PresetLastNames)
            {
                string captured = surname;
                CreateNameChipButton(lnGridObj.transform, captured, () => SelectLastName(captured));
            }

            CreateInlineCustomInput(modLast.transform, "CustomLastNameRow", "TYPE SURNAME:", "Type custom family surname...",
                -180f, "[RNG] ROLL SURNAME", RollRandomLastName, out customLastNameInput, (val) =>
                {
                    if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                    {
                        character.lastName = val.Trim();
                        RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                    }
                });

            // =========================================================================
            // BOTTOM FOOTER ACTION BAR (62px)
            // =========================================================================
            GameObject footer = new GameObject("IdentityFooterBar");
            footer.transform.SetParent(frameObj.transform, false);
            RectTransform footRect = footer.AddComponent<RectTransform>();
            footRect.anchorMin = new Vector2(0f, 0f);
            footRect.anchorMax = new Vector2(1f, 0f);
            footRect.pivot = new Vector2(0.5f, 0f);
            footRect.sizeDelta = new Vector2(0f, 62f);
            footRect.anchoredPosition = Vector2.zero;
            footer.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 1f);

            GameObject footLine = new GameObject("TopDivider");
            footLine.transform.SetParent(footer.transform, false);
            RectTransform flRect = footLine.AddComponent<RectTransform>();
            flRect.anchorMin = new Vector2(0f, 1f);
            flRect.anchorMax = new Vector2(1f, 1f);
            flRect.pivot = new Vector2(0.5f, 1f);
            flRect.sizeDelta = new Vector2(0f, 1.5f);
            flRect.anchoredPosition = Vector2.zero;
            footLine.AddComponent<Image>().color = new Color(0.18f, 0.28f, 0.40f, 0.85f);

            CreateUIText(footer.transform, "FooterNotice",
                "<color=#EF5350><b>[!] OFFICIAL NOTICE:</b></color> <color=#90A4AE>IDENTITY RECORD ABOVE WILL BE SEALED INTO YOUR FINAL FEDERAL BIO-EMERGENCY DOSSIER.</color>",
                11f, TextAlignmentOptions.MidlineLeft, Color.white, FontRole.Mono,
                new Vector2(0f, 0f), new Vector2(0.66f, 1f), new Vector2(22f, 0f), Vector2.zero);

            CreateTacticalActionButton(footer.transform, "Btn_ConfirmIdentityProceed", "CONFIRM IDENTITY & SELECT VOCATION >>",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(390f, 44f),
                new Color(0.78f, 0.16f, 0.16f, 1f), new Color(1f, 0.45f, 0.35f, 0.9f), ConfirmIdentityAndProceed, pivotX: 1f, pivotY: 0.5f, fontSize: 15f);

            BindIdentityConsoleReferences(frameObj.transform);
            SundayTypography.ApplyHierarchy(genderPanel.transform);
        }

        private void BindIdentityConsoleReferences(Transform frame)
        {
            if (frame == null) return;

            topBannerCaseIdText = frame.Find("TopBanner/BannerRight")?.GetComponent<TextMeshProUGUI>();
            idCardLastNameText = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/LiveFields/ValSurname")?.GetComponent<TextMeshProUGUI>();
            idCardFirstNameText = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/LiveFields/ValGiven")?.GetComponent<TextMeshProUGUI>();
            idCardMetaText = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/LiveFields/ValMetaGrid")?.GetComponent<TextMeshProUGUI>();
            idCardMrzText = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/MRZStrip/ValMrzCode")?.GetComponent<TextMeshProUGUI>();
            idCardEmblemText = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/PortraitBox/EmblemSymbol")?.GetComponent<TextMeshProUGUI>();
            idCardSilhouetteImg = frame.Find("ConsoleBody/LeftCol_CitizenCard/USCitizenIDCard/PortraitBox/SilhouetteGraphic")?.GetComponent<Image>();
            idCardQuoteText = frame.Find("ConsoleBody/LeftCol_CitizenCard/MirrorProfileBox/ValMirrorQuote")?.GetComponent<TextMeshProUGUI>();
            idCardActiveRecordText = frame.Find("ConsoleBody/LeftCol_CitizenCard/MirrorProfileBox/ActiveRecordPill/ValActiveRecord")?.GetComponent<TextMeshProUGUI>();

            genderMaleBtn = frame.Find("ConsoleBody/RightCol_Controls/Module01_Gender/Btn_Gender_Male")?.GetComponent<Button>();
            genderFemaleBtn = frame.Find("ConsoleBody/RightCol_Controls/Module01_Gender/Btn_Gender_Female")?.GetComponent<Button>();
            genderNonBinaryBtn = frame.Find("ConsoleBody/RightCol_Controls/Module01_Gender/Btn_Gender_NonBinary")?.GetComponent<Button>();

            if (genderMaleBtn != null)
            {
                genderMaleBtn.onClick.RemoveAllListeners();
                genderMaleBtn.onClick.AddListener(() => SelectGender("Male"));
            }
            if (genderFemaleBtn != null)
            {
                genderFemaleBtn.onClick.RemoveAllListeners();
                genderFemaleBtn.onClick.AddListener(() => SelectGender("Female"));
            }
            if (genderNonBinaryBtn != null)
            {
                genderNonBinaryBtn.onClick.RemoveAllListeners();
                genderNonBinaryBtn.onClick.AddListener(() => SelectGender("Non-Binary"));
            }

            firstNameChipsRoot = frame.Find("ConsoleBody/RightCol_Controls/Module02_FirstName/FirstNameChipsGrid");
            lastNameChipsRoot = frame.Find("ConsoleBody/RightCol_Controls/Module03_LastName/LastNameChipsGrid");

            if (lastNameChipsRoot != null)
            {
                for (int i = 0; i < lastNameChipsRoot.childCount; i++)
                {
                    Transform chip = lastNameChipsRoot.GetChild(i);
                    Button b = chip.GetComponent<Button>();
                    string chipName = chip.name.Replace("Chip_", "");
                    if (b != null)
                    {
                        b.onClick.RemoveAllListeners();
                        b.onClick.AddListener(() => SelectLastName(chipName));
                    }
                }
            }

            Button randNameBtn = frame.Find("ConsoleBody/LeftCol_CitizenCard/MirrorProfileBox/Btn_RandomName")?.GetComponent<Button>();
            if (randNameBtn != null)
            {
                randNameBtn.onClick.RemoveAllListeners();
                randNameBtn.onClick.AddListener(RandomizeNameOnly);
            }

            Button randAllBtn = frame.Find("ConsoleBody/LeftCol_CitizenCard/MirrorProfileBox/Btn_RandomAll")?.GetComponent<Button>();
            if (randAllBtn != null)
            {
                randAllBtn.onClick.RemoveAllListeners();
                randAllBtn.onClick.AddListener(RandomizeFullIdentity);
            }

            Button proceedBtn = frame.Find("IdentityFooterBar/Btn_ConfirmIdentityProceed")?.GetComponent<Button>();
            if (proceedBtn != null)
            {
                proceedBtn.onClick.RemoveAllListeners();
                proceedBtn.onClick.AddListener(ConfirmIdentityAndProceed);
            }

            BindInlineRow(frame.Find("ConsoleBody/RightCol_Controls/Module01_Gender/CustomGenderRow"), ConfirmCustomGender, ref customGenderInput, (val) =>
            {
                if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                {
                    character.gender = val.Trim();
                    RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                }
            });

            BindInlineRow(frame.Find("ConsoleBody/RightCol_Controls/Module02_FirstName/CustomFirstNameRow"), RollRandomFirstName, ref customFirstNameInput, (val) =>
            {
                if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                {
                    character.firstName = val.Trim();
                    RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                }
            });

            BindInlineRow(frame.Find("ConsoleBody/RightCol_Controls/Module03_LastName/CustomLastNameRow"), RollRandomLastName, ref customLastNameInput, (val) =>
            {
                if (!suppressInputCallback && !string.IsNullOrWhiteSpace(val))
                {
                    character.lastName = val.Trim();
                    RefreshIdentityConsoleUI(rebuildFirstNameChips: false, skipInputSync: true);
                }
            });
        }

        private void BindInlineRow(Transform row, UnityEngine.Events.UnityAction onBtnClick, ref TMP_InputField fieldRef, UnityEngine.Events.UnityAction<string> onChanged)
        {
            if (row == null) return;
            TMP_InputField inp = row.Find("InputField")?.GetComponent<TMP_InputField>();
            if (inp != null)
            {
                fieldRef = inp;
                inp.onValueChanged.RemoveAllListeners();
                inp.onValueChanged.AddListener(onChanged);
            }
            Button actBtn = row.Find("ActionBtn")?.GetComponent<Button>();
            if (actBtn != null)
            {
                actBtn.onClick.RemoveAllListeners();
                actBtn.onClick.AddListener(onBtnClick);
            }
        }

        private void RefreshIdentityConsoleUI(bool rebuildFirstNameChips, bool skipInputSync = false)
        {
            string g = string.IsNullOrWhiteSpace(character.gender) ? "Male" : character.gender.Trim();
            string fn = string.IsNullOrWhiteSpace(character.firstName) ? "Neal" : character.firstName.Trim();
            string ln = string.IsNullOrWhiteSpace(character.lastName) ? "Sage" : character.lastName.Trim();

            bool isMale = g.Equals("Male", System.StringComparison.OrdinalIgnoreCase);
            bool isFemale = g.Equals("Female", System.StringComparison.OrdinalIgnoreCase);
            bool isNb = g.Equals("Non-Binary", System.StringComparison.OrdinalIgnoreCase);

            // 1. Highlight active Gender Card (using clean ASCII tags)
            StyleGenderButton(genderMaleBtn, isMale, "[M]  MALE");
            StyleGenderButton(genderFemaleBtn, isFemale, "[F]  FEMALE");
            StyleGenderButton(genderNonBinaryBtn, isNb, "[NB] NON-BINARY");

            // 2. Rebuild First Name Chips if gender category changed
            if (rebuildFirstNameChips && firstNameChipsRoot != null)
            {
                for (int i = firstNameChipsRoot.childCount - 1; i >= 0; i--)
                {
                    GameObject c = firstNameChipsRoot.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(c);
                    else DestroyImmediate(c);
                }

                string[] pool = GetFirstNamePoolForGender(g);
                foreach (string preset in pool)
                {
                    string captured = preset;
                    CreateNameChipButton(firstNameChipsRoot, captured, () => SelectFirstName(captured));
                }
            }

            // 3. Highlight active First Name & Last Name Chips
            HighlightChipContainer(firstNameChipsRoot, fn);
            HighlightChipContainer(lastNameChipsRoot, ln);

            // 4. Sync InputFields without re-triggering callbacks
            if (!skipInputSync)
            {
                suppressInputCallback = true;
                if (customGenderInput != null)
                    customGenderInput.text = (!isMale && !isFemale && !isNb) ? g : "";
                if (customFirstNameInput != null)
                    customFirstNameInput.text = fn;
                if (customLastNameInput != null)
                    customLastNameInput.text = ln;
                suppressInputCallback = false;
            }

            // 5. Compute unified biometric data (shared 100% with Final Summary Dossier)
            GetCitizenBiometrics(out string docNo, out string dob, out string blood, out string sector, out string clearance, out string mrzLine1, out string mrzLine2);

            if (topBannerCaseIdText != null)
            {
                topBannerCaseIdText.text = $"CASE FILE: <color=#FFE082><b>{docNo}</b></color>   |   <color=#EF5350><b>[ STATUS: CIVIL DANGER WARNING // LEVEL 4 ]</b></color>";
            }

            if (idCardLastNameText != null) idCardLastNameText.text = ln.ToUpperInvariant();
            if (idCardFirstNameText != null) idCardFirstNameText.text = fn.ToUpperInvariant();

            Color profileTint = isMale
                ? new Color(0.45f, 0.82f, 0.98f, 0.95f)
                : (isFemale ? new Color(0.98f, 0.58f, 0.75f, 0.95f) : new Color(0.75f, 0.60f, 1.0f, 0.95f));

            if (idCardSilhouetteImg != null)
            {
                idCardSilhouetteImg.color = Color.white;
            }
            if (idCardEmblemText != null)
            {
                string code = isMale ? "MALE" : (isFemale ? "FEMALE" : "NON-BINARY");
                idCardEmblemText.text = $"<b>[{code}]</b>";
                idCardEmblemText.color = profileTint;
            }

            if (idCardMetaText != null)
            {
                idCardMetaText.text =
                    $"<color=#78909C>SEX / ID:</color> <color=#FFE082><b>{g.ToUpperInvariant()}</b></color><pos=52%><color=#78909C>BLOOD:</color> <color=#EF5350><b>{blood}</b></color>\n" +
                    $"<color=#78909C>DOB:</color>      <color=#FFFFFF>{dob}</color><pos=52%><color=#78909C>ZONE:</color>  <color=#80CBC4>SECTOR-04</color>\n" +
                    $"<color=#78909C>CASE ID:</color>  <color=#4FC3F7>{docNo}</color>";
            }

            if (idCardMrzText != null)
            {
                idCardMrzText.text = $"{mrzLine1}\n{mrzLine2}";
            }

            if (idCardQuoteText != null)
            {
                if (isMale)
                {
                    idCardQuoteText.text = $"\"A weathered reflection stares back from the darkened apartment glass. {fn} {ln}—broad shoulders tense under the low hum of emergency broadcasts. When Sunday night falls, hesitation is fatal.\"";
                }
                else if (isFemale)
                {
                    idCardQuoteText.text = $"\"Sharp, watchful eyes catch the dim streetlight through the blinds. {fn} {ln}—steady hands and a restless pulse as sirens echo across the district. Nobody is coming to save you tonight.\"";
                }
                else
                {
                    idCardQuoteText.text = $"\"An adaptable reflection caught between the old world and the silence to come. {fn} {ln}—old labels mean nothing to the infection outside, only nerve, instinct, and the will to endure.\"";
                }
            }

            if (idCardActiveRecordText != null)
            {
                idCardActiveRecordText.text = $"<b>CITIZEN:</b> <color=#FFFFFF>{fn.ToUpperInvariant()} {ln.ToUpperInvariant()}</color>  <color=#78909C>|</color>  <b>CASE:</b> <color=#4FC3F7>{docNo}</color>";
            }
        }

        private void StyleGenderButton(Button btn, bool isSelected, string baseTitle)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            Outline ol = btn.GetComponent<Outline>();
            TextMeshProUGUI titleTmp = btn.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            Image dotImg = btn.transform.Find("LeftStrip")?.GetComponent<Image>();

            if (bg != null)
                bg.color = isSelected ? new Color(0.24f, 0.13f, 0.04f, 0.98f) : new Color(0.06f, 0.09f, 0.14f, 0.95f);
            if (ol != null)
                ol.effectColor = isSelected ? new Color(0.95f, 0.55f, 0.15f, 1f) : new Color(0.18f, 0.28f, 0.40f, 0.75f);
            if (dotImg != null)
                dotImg.color = isSelected ? new Color(1f, 0.65f, 0.15f, 1f) : new Color(0.22f, 0.32f, 0.44f, 0.8f);
            if (titleTmp != null)
            {
                titleTmp.text = isSelected ? $"● {baseTitle}" : baseTitle;
                titleTmp.color = isSelected ? new Color(1f, 0.85f, 0.45f) : Color.white;
            }
        }

        private void HighlightChipContainer(Transform container, string activeValue)
        {
            if (container == null) return;
            for (int i = 0; i < container.childCount; i++)
            {
                Transform chip = container.GetChild(i);
                string chipVal = chip.name.Replace("Chip_", "");
                bool active = string.Equals(chipVal, activeValue, System.StringComparison.OrdinalIgnoreCase);

                Image bg = chip.GetComponent<Image>();
                Outline ol = chip.GetComponent<Outline>();
                TextMeshProUGUI txt = chip.GetComponentInChildren<TextMeshProUGUI>();

                if (bg != null)
                    bg.color = active ? new Color(0.24f, 0.13f, 0.04f, 0.98f) : new Color(0.06f, 0.09f, 0.14f, 0.92f);
                if (ol != null)
                    ol.effectColor = active ? new Color(0.95f, 0.55f, 0.15f, 0.95f) : new Color(0.16f, 0.24f, 0.34f, 0.65f);
                if (txt != null)
                {
                    txt.text = active ? $"<b>● {chipVal.ToUpperInvariant()}</b>" : chipVal.ToUpperInvariant();
                    txt.color = active ? new Color(1f, 0.85f, 0.45f) : new Color(0.82f, 0.86f, 0.90f);
                }
            }
        }
        #endregion

        #region Step 2: Job Selection
        public void ShowJobSelectionPanel()
        {
            HideAllPanels();
            EnsureJobBackButton();
            if (jobSelectionPanel != null) jobSelectionPanel.SetActive(true);
        }

        private void EnsureJobBackButton()
        {
            if (jobSelectionPanel == null) return;

            // Upgrade Header Title & Subtitle (constrained width so it never overlaps left button or right badge)
            Transform header = jobSelectionPanel.transform.Find("Header");
            if (header != null)
            {
                RectTransform hRect = header.GetComponent<RectTransform>();
                if (hRect != null)
                {
                    hRect.anchorMin = new Vector2(0.5f, 1f);
                    hRect.anchorMax = new Vector2(0.5f, 1f);
                    hRect.pivot = new Vector2(0.5f, 1f);
                    hRect.sizeDelta = new Vector2(780f, 82f);
                    hRect.anchoredPosition = new Vector2(0f, -24f);
                }

                TextMeshProUGUI titleTmp = header.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null)
                {
                    titleTmp.text = "PRE-OUTBREAK VOCATION <size=75%><color=#90A4AE>[SERVICE RECORD]</color></size>";
                    SundayTypography.ApplyFont(titleTmp, FontRole.Display);
                }
                TextMeshProUGUI subTmp = header.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
                if (subTmp != null)
                {
                    subTmp.text = "\"What was your life before the screaming started?\"";
                    SundayTypography.ApplyFont(subTmp, FontRole.Narrative);
                    RectTransform sRect = subTmp.GetComponent<RectTransform>();
                    if (sRect != null) sRect.anchoredPosition = new Vector2(0f, -40f);
                }
            }

            // Ensure M.O.R.T.A.L. Biometric Legend Pill Bar right below the subtitle so players immediately know the 6 attributes!
            Transform legendTr = jobSelectionPanel.transform.Find("SPAICLegendBar");
            if (legendTr == null)
            {
                GameObject legendObj = new GameObject("SPAICLegendBar");
                legendObj.transform.SetParent(jobSelectionPanel.transform, false);
                RectTransform lRect = legendObj.AddComponent<RectTransform>();
                lRect.anchorMin = new Vector2(0.5f, 1f);
                lRect.anchorMax = new Vector2(0.5f, 1f);
                lRect.pivot = new Vector2(0.5f, 1f);
                lRect.sizeDelta = new Vector2(1040f, 30f);
                lRect.anchoredPosition = new Vector2(0f, -104f);

                Image lBg = legendObj.AddComponent<Image>();
                lBg.color = new Color(0.05f, 0.08f, 0.13f, 0.94f);
                lBg.raycastTarget = false;

                Outline lOl = legendObj.AddComponent<Outline>();
                lOl.effectColor = new Color(0.20f, 0.34f, 0.50f, 0.75f);
                lOl.effectDistance = new Vector2(1f, -1f);

                CreateUIText(legendObj.transform, "LegendText",
                    "<b><color=#F5BF42>■ M.O.R.T.A.L. RATING:</color></b>    " +
                    "<color=#EF5350><b>[M]</b></color> Might   ·   " +
                    "<color=#FFA726><b>[O]</b></color> Observation   ·   " +
                    "<color=#66BB6A><b>[R]</b></color> Reflex   ·   " +
                    "<color=#42A5F5><b>[T]</b></color> Tactics   ·   " +
                    "<color=#FFD54F><b>[A]</b></color> Auspice   ·   " +
                    "<color=#AB47BC><b>[L]</b></color> Leadership",
                    11.5f, TextAlignmentOptions.Center, new Color(0.84f, 0.90f, 0.96f), FontRole.Mono,
                    Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            }

            // Ensure Top-Left Back Button
            Transform existing = jobSelectionPanel.transform.Find("Btn_BackToIdentity");
            if (existing != null)
            {
                TextMeshProUGUI t = existing.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "<b><< EDIT CITIZEN ID</b>";
                Button b = existing.GetComponent<Button>();
                if (b != null)
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => { PlayClick(); ShowGenderPanel(); });
                }
            }
            else
            {
                CreateTacticalActionButton(jobSelectionPanel.transform, "Btn_BackToIdentity", "<< EDIT CITIZEN ID",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -48f), new Vector2(210f, 40f),
                    new Color(0.08f, 0.12f, 0.18f, 0.95f), new Color(0.25f, 0.42f, 0.62f, 0.85f),
                    () => { PlayClick(); ShowGenderPanel(); }, pivotX: 0f, pivotY: 1f, fontSize: 13f);
            }

            // Ensure Top-Right Federal Step & Citizen Status Badge (matching Step 1 & Step 3)
            GetCitizenBiometrics(out string docNo, out _, out _, out _, out _, out _, out _);
            Transform badgeTr = jobSelectionPanel.transform.Find("JobStepBadge");
            TextMeshProUGUI badgeText;
            if (badgeTr == null)
            {
                GameObject box = new GameObject("JobStepBadge");
                box.transform.SetParent(jobSelectionPanel.transform, false);
                RectTransform bRect = box.AddComponent<RectTransform>();
                bRect.anchorMin = new Vector2(1f, 1f);
                bRect.anchorMax = new Vector2(1f, 1f);
                bRect.pivot = new Vector2(1f, 1f);
                bRect.sizeDelta = new Vector2(390f, 48f);
                bRect.anchoredPosition = new Vector2(-190f, -44f);
                box.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
                Outline ol = box.AddComponent<Outline>();
                ol.effectColor = new Color(0.18f, 0.32f, 0.48f, 0.85f);
                ol.effectDistance = new Vector2(1f, -1f);

                badgeText = CreateUIText(box.transform, "StatusText", "",
                    11f, TextAlignmentOptions.MidlineRight, new Color(0.80f, 0.88f, 0.95f), FontRole.Mono,
                    Vector2.zero, Vector2.one, new Vector2(12f, 2f), new Vector2(-12f, -2f));
            }
            else
            {
                badgeText = badgeTr.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            }

            if (badgeText != null)
            {
                string citizenName = character.FullName.ToUpperInvariant();
                badgeText.text =
                    $"<color=#FFA726><b>STEP 02 // VOCATION</b></color>  |  CITIZEN: <color=#FFFFFF><b>{citizenName}</b></color>\n" +
                    $"<color=#90A4AE>CASE FILE: <color=#4FC3F7>{docNo}</color> // GENDER: {character.gender.ToUpperInvariant()}</color>";
            }

            // Ensure all 6 JobCards have JobCardView attached and configured
            Transform content = jobSelectionPanel.transform.Find("JobScrollView/Viewport/Content");
            if (content != null)
            {
                foreach (var job in CharacterCreationDefinitions.Jobs)
                {
                    Transform card = content.Find("JobCard_" + job.id);
                    if (card != null)
                    {
                        string jId = job.id;
                        JobData capturedJob = job;
                        JobCardView cardView = card.GetComponent<JobCardView>();
                        if (cardView == null) cardView = card.gameObject.AddComponent<JobCardView>();
                        cardView.Setup(capturedJob, () => SelectJob(jId));
                    }
                }
            }

            JobSelectionScreenAdapter jobAdapter = jobSelectionPanel.GetComponentInChildren<JobSelectionScreenAdapter>(true);
            if (jobAdapter != null)
            {
                jobAdapter.UpdateLayout();
            }
        }

        public void SelectJob(string jobId)
        {
            JobData job = System.Array.Find(CharacterCreationDefinitions.Jobs, j => j.id.Equals(jobId, System.StringComparison.OrdinalIgnoreCase));
            if (job == null) return;

            selectedJobId = job.id;
            pickedHobbyIds.Clear();
            hobbyStep = 1;
            activeWeaknessTab = 0;

            RebuildCharacterFromSelections();
            PlayClick();

            if (jobId.Equals("student", System.StringComparison.OrdinalIgnoreCase))
            {
                ShowStudentTrainingPanel();
            }
            else
            {
                ShowHobbySelectionPanel();
            }
        }
        #endregion

        #region Step 2b: Student Exclusive Training & Majors
        private string currentInspectedStat = "str";
        private string currentlyHoveredStat = null;
        private float inspectLockTimer = 0f;
        private const float CLICK_LOCK_DURATION = 3.5f;

        public void ShowStudentTrainingPanel()
        {
            HideAllPanels();
            if (studentTrainingPanel != null)
            {
                studentTrainingPanel.SetActive(true);
                EnsureStudentPanelSPAICOrder();
            }
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            RebuildCharacterFromSelections();
            InspectStudentStat("str", lockSelection: false);
            UpdateStudentTrainingUI();
        }

        private void EnsureStudentPanelSPAICOrder()
        {
            if (studentTrainingPanel == null) return;
            var allTransforms = studentTrainingPanel.GetComponentsInChildren<Transform>(true);
            Transform rows = null;
            foreach (var t in allTransforms)
            {
                if (t.name == "AllocTitle")
                {
                    TextMeshProUGUI titleTmp = t.GetComponent<TextMeshProUGUI>();
                    if (titleTmp != null) titleTmp.text = "S.P.A.I.C. APTITUDE ALLOCATION";
                }
                else if (t.name == "AttributesRows")
                {
                    rows = t;
                }
            }

            if (rows != null)
            {
                // Order rows as S.P.A.I.C.: STRENGTH, PERCEPTION, AGILITY, INTELLIGENCE, CHARISMA
                string[] spaicRowOrder = { "Row_STRENGTH", "Row_PERCEPTION", "Row_AGILITY", "Row_INTELLIGENCE", "Row_CHARISMA" };
                for (int i = 0; i < spaicRowOrder.Length; i++)
                {
                    Transform r = rows.Find(spaicRowOrder[i]);
                    if (r != null) r.SetSiblingIndex(i);
                }
            }
        }

        public void OnRowPointerEnter(string stat)
        {
            currentlyHoveredStat = stat;
            if (inspectLockTimer <= 0f)
            {
                if (!string.Equals(currentInspectedStat, stat, System.StringComparison.OrdinalIgnoreCase))
                {
                    InspectStudentStat(stat, lockSelection: false);
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayButtonHover();
                    }
                }
            }
        }

        public void OnRowPointerExit(string stat)
        {
            if (string.Equals(currentlyHoveredStat, stat, System.StringComparison.OrdinalIgnoreCase))
            {
                currentlyHoveredStat = null;
            }
        }

        public void HoverStudentStat(string stat)
        {
            OnRowPointerEnter(stat);
        }

        public void InspectStudentStat(string stat, bool lockSelection = false)
        {
            currentInspectedStat = stat;
            if (lockSelection)
            {
                inspectLockTimer = CLICK_LOCK_DURATION;
            }
            UpdateStudentInspectorUI();
        }

        public void AddStudentPoint(string stat)
        {
            if (studentPointsRemaining <= 0) return;

            switch (stat.ToLower())
            {
                case "str": if (2 + studentAllocStr < 10) { studentAllocStr++; studentPointsRemaining--; } break;
                case "int": if (2 + studentAllocInt < 10) { studentAllocInt++; studentPointsRemaining--; } break;
                case "agi": if (2 + studentAllocAgi < 10) { studentAllocAgi++; studentPointsRemaining--; } break;
                case "cha": if (2 + studentAllocCha < 10) { studentAllocCha++; studentPointsRemaining--; } break;
                case "per": if (2 + studentAllocPer < 10) { studentAllocPer++; studentPointsRemaining--; } break;
            }

            RebuildCharacterFromSelections();
            InspectStudentStat(stat, lockSelection: true);
            PlayClick();
            UpdateStudentTrainingUI();
        }

        public void RemoveStudentPoint(string stat)
        {
            if (studentPointsRemaining >= 5) return;

            switch (stat.ToLower())
            {
                case "str": if (studentAllocStr > 0) { studentAllocStr--; studentPointsRemaining++; } break;
                case "int": if (studentAllocInt > 0) { studentAllocInt--; studentPointsRemaining++; } break;
                case "agi": if (studentAllocAgi > 0) { studentAllocAgi--; studentPointsRemaining++; } break;
                case "cha": if (studentAllocCha > 0) { studentAllocCha--; studentPointsRemaining++; } break;
                case "per": if (studentAllocPer > 0) { studentAllocPer--; studentPointsRemaining++; } break;
            }

            RebuildCharacterFromSelections();
            InspectStudentStat(stat, lockSelection: true);
            PlayClick();
            UpdateStudentTrainingUI();
        }

        private string FormatPips(int value)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 1; i <= 10; i++)
            {
                if (i <= value)
                    sb.Append("<color=#E67E22>■ </color>");
                else
                    sb.Append("<color=#2A2A2A>■ </color>");
            }
            return sb.ToString().TrimEnd();
        }

        private void UpdateStudentTrainingUI()
        {
            if (studentPointsText != null)
            {
                studentPointsText.text = studentPointsRemaining > 0
                    ? $"<color=#E67E22><b>[ {studentPointsRemaining} AP AVAILABLE FOR ALLOCATION ]</b></color>"
                    : "<color=#2ECC71><b>[ ALL 5 AP POINTS ALLOCATED ]</b></color>";
            }

            if (studentStrText != null) studentStrText.text = $"{FormatPips(character.str)}   <color=#FFFFFF><b>{character.str}</b></color>";
            if (studentIntText != null) studentIntText.text = $"{FormatPips(character.intl)}   <color=#FFFFFF><b>{character.intl}</b></color>";
            if (studentAgiText != null) studentAgiText.text = $"{FormatPips(character.agi)}   <color=#FFFFFF><b>{character.agi}</b></color>";
            if (studentChaText != null) studentChaText.text = $"{FormatPips(character.cha)}   <color=#FFFFFF><b>{character.cha}</b></color>";
            if (studentPerText != null) studentPerText.text = $"{FormatPips(character.per)}   <color=#FFFFFF><b>{character.per}</b></color>";

            bool finished = studentPointsRemaining == 0;
            if (studentContinueButton != null) studentContinueButton.gameObject.SetActive(finished);
            if (studentWarningText != null)
            {
                studentWarningText.gameObject.SetActive(!finished);
                studentWarningText.text = $"[!] ALLOCATE ALL 5 POINTS TO FINALIZE RECORD ({studentPointsRemaining} REMAINING)";
            }

            UpdateStudentInspectorUI();
        }

        private void UpdateStudentInspectorUI()
        {
            if (inspectorStatTitle == null) return;

            switch (currentInspectedStat.ToLower())
            {
                case "str":
                    inspectorStatTitle.text = "STRENGTH (PHYSICAL POWER)";
                    inspectorStatTitle.color = new Color(0.91f, 0.30f, 0.24f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Muscles conditioned from athletics, manual labor, and gym drills.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Determines raw physical power, encumbrance limit, and brute force survival actions in combat and exploration.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#E74C3C><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Melee Combat:</b></color> +15% damage with blunt and edged weapons\n" +
                        "• <color=#FFFFFF><b>Carrying Capacity:</b></color> +8 kg maximum inventory encumbrance\n" +
                        "• <color=#FFFFFF><b>Breaching:</b></color> Force open jammed doors and barricades without tools";
                    break;

                case "int":
                    inspectorStatTitle.text = "INTELLIGENCE (ANALYTICAL MIND)";
                    inspectorStatTitle.color = new Color(0.20f, 0.60f, 0.86f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Late-night academic research, problem solving, and technical curiosity.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs scientific knowledge, electronic engineering, and the efficiency of medical supplies under pressure.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#3498DB><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Electronics & Tech:</b></color> +20% success when bypassing security and hotwiring\n" +
                        "• <color=#FFFFFF><b>Field Medicine:</b></color> Bandages, antibiotics & medkits restore +25% extra HP\n" +
                        "• <color=#FFFFFF><b>Crafting Efficiency:</b></color> Unlock advanced survival blueprints with less scrap";
                    break;

                case "agi":
                    inspectorStatTitle.text = "AGILITY (REFLEXES & MOBILITY)";
                    inspectorStatTitle.color = new Color(0.18f, 0.80f, 0.44f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Quick reflexes, track sprinting, and agile body coordination.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Determines movement swiftness, noise generation while sneaking, and reactive evasion when ambushed.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#2ECC71><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Sprint Stamina:</b></color> -10% stamina drain while sprinting away from hazards\n" +
                        "• <color=#FFFFFF><b>Stealth Evasion:</b></color> Sound footprint reduced by 15% across all surfaces\n" +
                        "• <color=#FFFFFF><b>Dodge Chance:</b></color> +12% chance to evade enemy lunges and grab attacks";
                    break;

                case "cha":
                    inspectorStatTitle.text = "CHARISMA (FORCE OF PERSONALITY)";
                    inspectorStatTitle.color = new Color(0.61f, 0.35f, 0.71f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Campus diplomacy, debate leadership, and unwavering social poise.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs interpersonal influence, trade negotiations with survivors, and psychological resilience.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#9B59B6><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Dialogue Checks:</b></color> Significant persuasion and de-escalation advantages\n" +
                        "• <color=#FFFFFF><b>Barter Economy:</b></color> 10% discount when trading goods with neutral survivors\n" +
                        "• <color=#FFFFFF><b>Morale & Composure:</b></color> High resistance to psychological panic checks";
                    break;

                case "per":
                    inspectorStatTitle.text = "PERCEPTION (SITUATIONAL ALERTNESS)";
                    inspectorStatTitle.color = new Color(0.95f, 0.77f, 0.06f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Keen senses—noticing subtle sounds, shadows, and concealed hazards.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs sensory alertness, firearm accuracy at range, and the ability to scavenge hidden supplies.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#F1C40F><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Firearms & Aiming:</b></color> +10% ranged critical hit chance and recoil control\n" +
                        "• <color=#FFFFFF><b>Loot Detection:</b></color> Spot hidden supplies and secret caches from +3m distance\n" +
                        "• <color=#FFFFFF><b>Ambush Warning:</b></color> Early audio-visual cues before walking into dangerous traps";
                    break;
            }
        }

        public void ConfirmStudentTraining()
        {
            if (studentPointsRemaining > 0) return;
            PlayClick();
            ShowStudentMajorPanel();
        }

        public void ShowStudentMajorPanel()
        {
            HideAllPanels();
            if (studentMajorPanel != null)
            {
                studentMajorPanel.SetActive(true);
                WireAcademicCards(studentMajorPanel, isMajor: true);
            }
        }

        private void WireAcademicCards(GameObject panel, bool isMajor)
        {
            if (panel == null) return;
            AcademicCardView[] cards = panel.GetComponentsInChildren<AcademicCardView>(true);
            foreach (var card in cards)
            {
                string sId = card.skillId;
                Button[] buttons = card.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    btn.onClick.RemoveAllListeners();
                    if (isMajor)
                    {
                        btn.onClick.AddListener(() => SelectStudentMajor(sId));
                    }
                    else
                    {
                        btn.onClick.AddListener(() => SelectStudentMinor(sId));
                    }
                }
            }
        }

        public void SelectStudentMajor(string skill)
        {
            chosenMajorSkill = skill.ToLower();
            RebuildCharacterFromSelections();
            PlayClick();
            ShowStudentMinorPanel();
        }

        private void ApplyMajorToCharacter(string majorSkill)
        {
            string perkName = null;
            string perkDesc = null;
            string perkIcon = null;

            switch (majorSkill.ToLower())
            {
                case "shooting":
                    character.shooting = 3;
                    perkName = "GUNSLINGER";
                    perkDesc = "+25% reload speed & +15% pistol accuracy.";
                    perkIcon = "gunslinger";
                    break;
                case "melee":
                    character.melee = 3;
                    perkName = "BLADESMITH";
                    perkDesc = "Attacks cause Bleed. Critical sneak attacks.";
                    perkIcon = "bladesmith";
                    break;
                case "stealth":
                    character.stealth = 3;
                    break;
                case "tech":
                    character.tech = 3;
                    break;
                case "medicine":
                    character.medicine = 3;
                    perkName = "FIELD MEDIC";
                    perkDesc = "Healing items restore 2x HP.";
                    break;
                case "survival":
                    character.survival = 3;
                    perkName = "FIELD FORAGER";
                    perkDesc = "Harvest +50% extra food, water & campcraft scrap.";
                    perkIcon = "field_forager";
                    break;
                case "resolve":
                    character.resolve = 3;
                    perkName = "IRON WILL";
                    perkDesc = "Immunity to panic in high-stress crisis encounters.";
                    perkIcon = "iron_will";
                    break;
            }

            if (!string.IsNullOrEmpty(perkName))
            {
                Sprite icon = null;
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(perkIcon))
                    icon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Perks/{perkIcon}.png");
#endif
                character.AddTrait(perkName, perkDesc, icon);
            }
        }

        public void ShowStudentMinorPanel()
        {
            HideAllPanels();
            if (studentMinorPanel != null)
            {
                studentMinorPanel.SetActive(true);
                WireAcademicCards(studentMinorPanel, isMajor: false);

                AcademicCardView[] minorCards = studentMinorPanel.GetComponentsInChildren<AcademicCardView>(true);
                foreach (var card in minorCards)
                {
                    bool isMajor = !string.IsNullOrEmpty(chosenMajorSkill) && card.skillId.Equals(chosenMajorSkill, System.StringComparison.OrdinalIgnoreCase);
                    card.gameObject.SetActive(!isMajor);
                }
            }
        }

        public void SelectStudentMinor(string skill)
        {
            chosenMinorSkill = skill.ToLower();
            pickedHobbyIds.Clear();
            hobbyStep = 1;
            activeWeaknessTab = 0;
            RebuildCharacterFromSelections();
            PlayClick();
            ShowHobbySelectionPanel();
        }

        private void ApplyMinorToCharacter(string minorSkill)
        {
            switch (minorSkill.ToLower())
            {
                case "shooting": if (character.shooting < 3) character.shooting = 2; break;
                case "melee": if (character.melee < 3) character.melee = 2; break;
                case "stealth": if (character.stealth < 3) character.stealth = 2; break;
                case "tech": if (character.tech < 3) character.tech = 2; break;
                case "medicine": if (character.medicine < 3) character.medicine = 2; break;
                case "survival": if (character.survival < 3) character.survival = 2; break;
                case "resolve": if (character.resolve < 3) character.resolve = 2; break;
            }
        }
        #endregion

        #region Step 3: Hobbies & Weakness Selection (Unified 2-Tab Categorized Layout)
        public void ShowHobbySelectionPanel()
        {
            HideAllPanels();
            EnsureHobbyTopBar();
            if (hobbySelectionPanel != null) hobbySelectionPanel.SetActive(true);

            RebuildCharacterFromSelections();

            if (hobbyHeaderTitle != null)
            {
                SundayTypography.ApplyFont(hobbyHeaderTitle, FontRole.Display);
                if (hobbyHeaderSubtitle != null)
                    SundayTypography.ApplyFont(hobbyHeaderSubtitle, FontRole.Narrative);

                if (hobbyStep == 1)
                {
                    hobbyHeaderTitle.text = "PRIMARY HOBBY <size=75%><color=#90A4AE>[+1-2 STATS & GEAR]</color></size>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"How did you spend your weekends before everything fell apart?\"";
                }
                else if (hobbyStep == 2)
                {
                    hobbyHeaderTitle.text = "SECONDARY HOBBY <size=75%><color=#90A4AE>[+1 STAT & TRAIT]</color></size>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"A secondary pursuit that kept your instincts sharp.\"";
                }
                else
                {
                    hobbyHeaderTitle.text = "SURVIVOR WEAKNESS <size=75%><color=#EF5350>[CRITICAL DRAWBACK]</color></size>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"Nobody survives untouched. What was your fatal flaw?\"";
                }
            }

            UpdateHobbyTopBarStatus();

            // Enable the 2-Tab Thematic Category Bar on ALL 3 steps (Primary Hobby, Secondary Hobby, and Weakness!)
            if (weaknessTabBar != null)
            {
                weaknessTabBar.SetActive(true);
            }
            UpdateWeaknessTabVisuals();

            ScrollRect sr = hobbyCardsContainer != null ? hobbyCardsContainer.GetComponentInParent<ScrollRect>() : null;
            if (sr != null)
            {
                sr.verticalNormalizedPosition = 1f;
            }

            PopulateHobbyCards();
        }

        private void EnsureHobbyTopBar()
        {
            if (hobbySelectionPanel == null) return;

            Transform backBtn = hobbySelectionPanel.transform.Find("Btn_HobbyPreviousStep");
            if (backBtn == null)
            {
                CreateTacticalActionButton(hobbySelectionPanel.transform, "Btn_HobbyPreviousStep", "<< PREVIOUS STEP",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -48f), new Vector2(205f, 40f),
                    new Color(0.08f, 0.12f, 0.18f, 0.95f), new Color(0.25f, 0.42f, 0.62f, 0.85f),
                    OnHobbyBackClicked, pivotX: 0f, pivotY: 1f, fontSize: 13f);
            }
            else
            {
                TextMeshProUGUI t = backBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "<b><< PREVIOUS STEP</b>";
                Button b = backBtn.GetComponent<Button>();
                if (b != null)
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(OnHobbyBackClicked);
                }
            }

            Transform badgeObj = hobbySelectionPanel.transform.Find("HobbyLoadoutBadge");
            if (badgeObj == null)
            {
                GameObject box = new GameObject("HobbyLoadoutBadge");
                box.transform.SetParent(hobbySelectionPanel.transform, false);
                RectTransform bRect = box.AddComponent<RectTransform>();
                bRect.anchorMin = new Vector2(1f, 1f);
                bRect.anchorMax = new Vector2(1f, 1f);
                bRect.pivot = new Vector2(1f, 1f);
                bRect.sizeDelta = new Vector2(390f, 48f);
                bRect.anchoredPosition = new Vector2(-190f, -44f);
                box.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
                Outline ol = box.AddComponent<Outline>();
                ol.effectColor = new Color(0.18f, 0.32f, 0.48f, 0.85f);
                ol.effectDistance = new Vector2(1f, -1f);

                hobbyLoadoutStatusText = CreateUIText(box.transform, "StatusText", "",
                    10.5f, TextAlignmentOptions.MidlineRight, new Color(0.80f, 0.88f, 0.95f), FontRole.Mono,
                    Vector2.zero, Vector2.one, new Vector2(12f, 2f), new Vector2(-12f, -2f));
            }
            else
            {
                hobbyLoadoutStatusText = badgeObj.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            }
        }

        private void UpdateHobbyTopBarStatus()
        {
            if (hobbyLoadoutStatusText == null) return;

            string primaryTitle = pickedHobbyIds.Count >= 1 ? GetHobbyTitleById(pickedHobbyIds[0]) : "NONE";
            string secondaryTitle = pickedHobbyIds.Count >= 2 ? GetHobbyTitleById(pickedHobbyIds[1]) : "NONE";

            string mortalLine = $"<color=#90A4AE>M.O.R.T.A.L.:</color> <color=#EF5350>M:{character.str}</color> <color=#FFA726>O:{character.per}</color> <color=#66BB6A>R:{character.agi}</color> <color=#42A5F5>T:{character.intl}</color> <color=#FFD54F>A:{character.lck}</color> <color=#AB47BC>L:{character.cha}</color>";

            if (hobbyStep == 1)
            {
                hobbyLoadoutStatusText.text =
                    $"<color=#FFA726><b>STEP 01/02: PRIMARY HOBBY</b></color>  |  VOCATION: <color=#FFFFFF><b>{character.job.ToUpperInvariant()}</b></color>\n" +
                    mortalLine;
            }
            else if (hobbyStep == 2)
            {
                hobbyLoadoutStatusText.text =
                    $"<color=#2ECC71><b>■ PRIMARY: {primaryTitle}</b></color>  |  <color=#FFA726><b>STEP 02/02: SECONDARY HOBBY</b></color>\n" +
                    mortalLine;
            }
            else
            {
                hobbyLoadoutStatusText.text =
                    $"<color=#2ECC71><b>■ {primaryTitle} + {secondaryTitle}</b></color>  |  <color=#EF5350><b>HOBBIES COMPLETED</b></color>\n" +
                    mortalLine;
            }
        }

        private string GetHobbyTitleById(string id)
        {
            var h = System.Array.Find(CharacterCreationDefinitions.PositiveHobbies, x => x.id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            return h != null ? h.title : id.ToUpperInvariant();
        }

        public void OnHobbyBackClicked()
        {
            PlayClick();
            if (hobbyStep == 3)
            {
                if (pickedHobbyIds.Count >= 2) pickedHobbyIds.RemoveAt(pickedHobbyIds.Count - 1);
                hobbyStep = 2;
                activeWeaknessTab = 0;
                ShowHobbySelectionPanel();
            }
            else if (hobbyStep == 2)
            {
                if (pickedHobbyIds.Count >= 1) pickedHobbyIds.RemoveAt(pickedHobbyIds.Count - 1);
                hobbyStep = 1;
                activeWeaknessTab = 0;
                ShowHobbySelectionPanel();
            }
            else
            {
                pickedHobbyIds.Clear();
                if (selectedJobId.Equals("student", System.StringComparison.OrdinalIgnoreCase))
                {
                    ShowStudentMinorPanel();
                }
                else
                {
                    ShowJobSelectionPanel();
                }
            }
        }

        public void SwitchWeaknessTab(int tabIndex)
        {
            if (activeWeaknessTab == tabIndex) return;
            activeWeaknessTab = tabIndex;
            PlayClick();
            UpdateWeaknessTabVisuals();
            PopulateHobbyCards();
        }

        private void UpdateWeaknessTabVisuals()
        {
            bool isTab0 = activeWeaknessTab == 0;

            if (hobbyStep < 3)
            {
                // Positive Hobbies: Tab 0 = COMBAT, FIELD & STEALTH (8), Tab 1 = MIND, TECH & SUPPORT (8)
                Color activeTab0Bg = new Color(0.72f, 0.32f, 0.06f, 0.95f);
                Color activeTab1Bg = new Color(0.10f, 0.36f, 0.62f, 0.95f);
                Color inactiveBg = new Color(0.08f, 0.09f, 0.12f, 0.88f);

                if (physicalTabBg != null) physicalTabBg.color = isTab0 ? activeTab0Bg : inactiveBg;
                if (mentalTabBg != null) mentalTabBg.color = !isTab0 ? activeTab1Bg : inactiveBg;

                if (physicalTabText != null)
                {
                    physicalTabText.text = isTab0
                        ? "<b><color=#FFFFFF>● COMBAT, FIELD & STEALTH (8)</color></b>"
                        : "<b><color=#8899AA>○ COMBAT, FIELD & STEALTH (8)</color></b>";
                }
                if (mentalTabText != null)
                {
                    mentalTabText.text = !isTab0
                        ? "<b><color=#FFFFFF>● MIND, TECH & SUPPORT (8)</color></b>"
                        : "<b><color=#8899AA>○ MIND, TECH & SUPPORT (8)</color></b>";
                }

                if (physicalTabOutline != null)
                    physicalTabOutline.effectColor = isTab0 ? new Color(0.95f, 0.55f, 0.15f, 0.9f) : new Color(0.20f, 0.24f, 0.30f, 0.5f);
                if (mentalTabOutline != null)
                    mentalTabOutline.effectColor = !isTab0 ? new Color(0.29f, 0.68f, 0.98f, 0.9f) : new Color(0.20f, 0.24f, 0.30f, 0.5f);
            }
            else
            {
                // Weaknesses: Tab 0 = PHYSICAL FLAWS (10), Tab 1 = MENTAL & SOCIAL (10)
                if (physicalTabBg != null)
                    physicalTabBg.color = isTab0 ? new Color(0.65f, 0.15f, 0.12f, 0.95f) : new Color(0.08f, 0.08f, 0.08f, 0.85f);
                if (mentalTabBg != null)
                    mentalTabBg.color = !isTab0 ? new Color(0.65f, 0.15f, 0.12f, 0.95f) : new Color(0.08f, 0.08f, 0.08f, 0.85f);

                if (physicalTabText != null)
                {
                    physicalTabText.text = isTab0 ? "<b><color=#FFFFFF>● PHYSICAL FLAWS (10)</color></b>" : "<b><color=#777777>○ PHYSICAL FLAWS (10)</color></b>";
                }
                if (mentalTabText != null)
                {
                    mentalTabText.text = !isTab0 ? "<b><color=#FFFFFF>● MENTAL & SOCIAL (10)</color></b>" : "<b><color=#777777>○ MENTAL & SOCIAL (10)</color></b>";
                }

                if (physicalTabOutline != null)
                    physicalTabOutline.effectColor = isTab0 ? new Color(0.91f, 0.30f, 0.24f, 0.85f) : new Color(0.20f, 0.20f, 0.20f, 0.5f);
                if (mentalTabOutline != null)
                    mentalTabOutline.effectColor = !isTab0 ? new Color(0.91f, 0.30f, 0.24f, 0.85f) : new Color(0.20f, 0.20f, 0.20f, 0.5f);
            }
        }

        private void PopulateHobbyCards()
        {
            if (hobbyCardsContainer == null || hobbyCardPrefab == null) return;

            for (int i = hobbyCardsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = hobbyCardsContainer.GetChild(i);
                child.gameObject.SetActive(false);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            int spawnedCount = 0;

            if (hobbyStep < 3)
            {
                // Collect and sort by Category Tag so identical tags (COMBAT, SURVIVAL, STEALTH, etc.) are always grouped together!
                List<HobbyData> sortedHobbies = new List<HobbyData>();
                foreach (var hobby in CharacterCreationDefinitions.PositiveHobbies)
                {
                    bool isActionField = ActionFieldHobbyIds.Contains(hobby.id);
                    if (activeWeaknessTab == 0 && !isActionField) continue;
                    if (activeWeaknessTab == 1 && isActionField) continue;
                    sortedHobbies.Add(hobby);
                }

                sortedHobbies.Sort((a, b) => HobbyCardView.GetCategorySortOrder(a).CompareTo(HobbyCardView.GetCategorySortOrder(b)));

                foreach (var hobby in sortedHobbies)
                {
                    bool isAlreadyEquipped = pickedHobbyIds.Contains(hobby.id);

                    GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                    cardObj.SetActive(true);
                    spawnedCount++;
                    HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                    if (cardView != null)
                    {
                        HobbyData captured = hobby;
                        cardView.Setup(captured, () => OnHobbySelected(captured), isEquippedPrimary: isAlreadyEquipped);
                    }
                }
            }
            else
            {
                // Weakness step: filtered by active tab (10 cards each in a 5x2 grid)
                foreach (var hobby in CharacterCreationDefinitions.DestructiveWeaknesses)
                {
                    if (pickedHobbyIds.Contains(hobby.id)) continue;

                    bool isPhys = PhysicalWeaknessIds.Contains(hobby.id);
                    if (activeWeaknessTab == 0 && !isPhys) continue;
                    if (activeWeaknessTab == 1 && isPhys) continue;

                    GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                    cardObj.SetActive(true);
                    spawnedCount++;
                    HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                    if (cardView != null)
                    {
                        HobbyData captured = hobby;
                        cardView.Setup(captured, () => OnHobbySelected(captured), isEquippedPrimary: false);
                    }
                }
            }

            HobbySelectionScreenAdapter adapter = hobbyCardsContainer.GetComponentInParent<HobbySelectionScreenAdapter>();
            if (adapter != null)
            {
                adapter.UpdateLayout(spawnedCount);
            }
        }

        public void OnHobbySelected(HobbyData hobby)
        {
            if (pickedHobbyIds.Contains(hobby.id)) return;

            pickedHobbyIds.Add(hobby.id);
            RebuildCharacterFromSelections();
            PlayClick();

            if (hobbyStep == 1)
            {
                hobbyStep = 2;
                ShowHobbySelectionPanel();
            }
            else if (hobbyStep == 2)
            {
                // Completed Primary & Secondary Hobby -> Transition to Step 4: The Survival Bargain (Drawbacks & M.O.R.T.A.L. Points)
                ShowBargainPanel();
            }
            else
            {
                ShowBargainPanel();
            }
        }
        #endregion

        #region Step 4: Final Summary Dossier (100% Synced with Step 1 Citizen Intake Record)
        public void ShowSummaryDossierPanel()
        {
            HideAllPanels();
            RebuildCharacterFromSelections();
            if (summaryDossierPanel != null) summaryDossierPanel.SetActive(true);
            EnforceDossierLayout();
            SundayTypography.ApplyHierarchy(summaryDossierPanel.transform);
            dossierLayoutEnforced = true;

            GetCitizenBiometrics(out string docNo, out string dob, out string blood, out string sector, out string clearance, out string mrzLine1, out string mrzLine2);

            // Sync top-right HeaderBar CaseId with Step 1's exact docNo
            Transform caseIdTr = summaryDossierPanel.transform.Find("FederalDossierFrame/HeaderBar/MetaBlock/CaseId");
            if (caseIdTr != null)
            {
                TextMeshProUGUI caseTmp = caseIdTr.GetComponent<TextMeshProUGUI>();
                if (caseTmp != null)
                {
                    caseTmp.fontSize = 12.5f;
                    caseTmp.text = $"CASE FILE: <color=#FFE082><b>{docNo}</b></color>   |   DATE: DAY ZERO 23:45";
                }
            }

            if (summaryNameText != null)
            {
                summaryNameText.fontSize = 24f;
                summaryNameText.text = character.FullName.ToUpper();
            }

            if (summaryIdentityText != null)
            {
                summaryIdentityText.fontSize = 13.5f;
                summaryIdentityText.lineSpacing = 3f;
                summaryIdentityText.text =
                    $"GENDER: <color=#FFE082><b>{character.gender.ToUpper()}</b></color>   |   VOCATION: <color=#FFE082><b>{character.job.ToUpper()}</b></color>\n" +
                    $"<size=95%><color=#90A4AE>DOB:</color> <color=#FFFFFF>{dob}</color>   |   <color=#90A4AE>BLOOD:</color> <color=#EF5350><b>{blood}</b></color>   |   <color=#90A4AE>ID:</color> <color=#4FC3F7>{docNo}</color></size>";
            }

            if (summaryVitalsText != null)
            {
                summaryVitalsText.fontSize = 13.5f;
                summaryVitalsText.text = $"<color=#EF5350><b>HP:</b> {character.hp}/{character.hpMax}</color>   |   <color=#FFA726><b>STAMINA:</b> {character.stamina}/{character.staminaMax}</color>   |   <color=#29B6F6><b>HUMANITY:</b> {character.humanity}/100</color>";
            }

            // Sync Col1_Identity screening protocol & barcode with Step 1's exact citizen record
            Transform col1 = summaryDossierPanel.transform.Find("FederalDossierFrame/ColumnsContainer/Col1_Identity");
            if (col1 != null)
            {
                TextMeshProUGUI[] col1Texts = col1.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in col1Texts)
                {
                    if (t != null && t.text != null && t.text.Contains("CHECKPOINT:"))
                    {
                        t.fontSize = 12f;
                        t.lineSpacing = 3f;
                        t.text =
                            $"• CHECKPOINT: <color=#FFE082>SECTOR-04 // METRO DECONTAM GATE</color>\n" +
                            $"• BIOMETRIC RECORD: <color=#FFFFFF>DOB {dob} // BLOOD {blood}</color>\n" +
                            $"• VIRAL ANTIGEN SWAB: <color=#66BB6A>NEGATIVE (NON-REACTIVE)</color>\n" +
                            $"• RETINAL OCULAR SCAN: <color=#66BB6A>NORMAL (NO CAPILLARY RUPTURE)</color>\n" +
                            $"• EPIDERMAL CHECK: <color=#66BB6A>NO INFECTED LACERATIONS / BITES</color>\n" +
                            $"• STATUS: <color=#29B6F6><b>{clearance} CLEARANCE</b></color>\n\n" +
                            $"<color=#4FC3F7><size=94%>{mrzLine1.Substring(0, Mathf.Min(28, mrzLine1.Length))}</size></color>  <color=#FFE082>{docNo}</color>";
                        break;
                    }
                }
            }

            // Sync Col2 Aptitudes header and biological index to M.O.R.T.A.L.
            Transform col2 = summaryDossierPanel.transform.Find("FederalDossierFrame/ColumnsContainer/Col2_Aptitudes");
            if (col2 != null)
            {
                TextMeshProUGUI[] col2Texts = col2.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in col2Texts)
                {
                    if (t != null && t.text != null)
                    {
                        if (t.text.Contains("S.P.A.I.C. ATTRIBUTES"))
                            t.text = "02 // M.O.R.T.A.L. ATTRIBUTES & PROFICIENCIES";
                        else if (t.text.Contains("S.P.A.I.C. BIOLOGICAL"))
                            t.text = "M.O.R.T.A.L. BIOLOGICAL APTITUDE INDEX:";
                    }
                }
            }

            if (summaryAttributesText != null)
            {
                summaryAttributesText.fontSize = 13f;
                summaryAttributesText.lineSpacing = 2.5f;
                summaryAttributesText.text = 
                    $"<color=#EF5350><b>[M] MIGHT</b></color><pos=42%><color=#90A4AE>(STR)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.str:00}</b></color></mspace><pos=64%>{FormatStatPips(character.str, 10)}\n" +
                    $"<color=#FFA726><b>[O] OBSERVATION</b></color><pos=42%><color=#90A4AE>(PER)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.per:00}</b></color></mspace><pos=64%>{FormatStatPips(character.per, 10)}\n" +
                    $"<color=#66BB6A><b>[R] REFLEX</b></color><pos=42%><color=#90A4AE>(AGI)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.agi:00}</b></color></mspace><pos=64%>{FormatStatPips(character.agi, 10)}\n" +
                    $"<color=#42A5F5><b>[T] TACTICS</b></color><pos=42%><color=#90A4AE>(INT)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.intl:00}</b></color></mspace><pos=64%>{FormatStatPips(character.intl, 10)}\n" +
                    $"<color=#FFD54F><b>[A] AUSPICE</b></color><pos=42%><color=#90A4AE>(LCK)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.lck:00}</b></color></mspace><pos=64%>{FormatStatPips(character.lck, 10)}\n" +
                    $"<color=#AB47BC><b>[L] LEADERSHIP</b></color><pos=42%><color=#90A4AE>(CHA)</color><pos=54%><mspace=0.6em><color=#FFFFFF><b>{character.cha:00}</b></color></mspace><pos=64%>{FormatStatPips(character.cha, 10)}";
            }

            if (summarySkillsText != null)
            {
                summarySkillsText.fontSize = 12.5f;
                summarySkillsText.lineSpacing = 3f;
                summarySkillsText.text = 
                    $"<color=#90A4AE>SHOOTING:</color> <pos=26%><mspace=0.6em><color=#FFE082><b>{character.shooting:00}</b></color></mspace><pos=52%><color=#90A4AE>MELEE:</color> <pos=78%><mspace=0.6em><color=#FFE082><b>{character.melee:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>STEALTH:</color> <pos=26%><mspace=0.6em><color=#FFE082><b>{character.stealth:00}</b></color></mspace><pos=52%><color=#90A4AE>TECH:</color> <pos=78%><mspace=0.6em><color=#FFE082><b>{character.tech:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>MEDICINE:</color> <pos=26%><mspace=0.6em><color=#FFE082><b>{character.medicine:00}</b></color></mspace><pos=52%><color=#90A4AE>SCAVENGING:</color> <pos=78%><mspace=0.6em><color=#FFE082><b>{character.scavenging:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>SURVIVAL:</color> <pos=26%><mspace=0.6em><color=#FFE082><b>{character.survival:00}</b></color></mspace><pos=52%><color=#90A4AE>RESOLVE:</color> <pos=78%><mspace=0.6em><color=#FFE082><b>{character.resolve:00}</b></color></mspace>";
            }

            if (summaryTraitsText != null)
            {
                summaryTraitsText.fontSize = 11.5f;
                summaryTraitsText.lineSpacing = 2.5f;
                summaryTraitsText.textWrappingMode = TextWrappingModes.Normal;
                summaryTraitsText.overflowMode = TextOverflowModes.Overflow;
                string traitsList = "";
                foreach (var t in character.traits)
                {
                    string color = t.isWeakness ? "#EF5350" : "#66BB6A";
                    string icon = t.isWeakness ? "[-]" : "[+]";
                    traitsList += $"<color={color}><b>{icon} {t.name.ToUpper()}:</b></color> <color=#CFD8DC>{t.description}</color>\n";
                }
                summaryTraitsText.text = string.IsNullOrEmpty(traitsList) ? "None" : traitsList.TrimEnd();
            }

            if (summaryInventoryText != null)
            {
                summaryInventoryText.fontSize = 12.5f;
                summaryInventoryText.lineSpacing = 3f;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int idx = 0;
                foreach (var item in character.inventory)
                {
                    if (idx % 2 == 0)
                    {
                        if (idx > 0) sb.Append("\n");
                        sb.Append($"<color=#80CBC4>•</color> <color=#FFFFFF><b>{item.itemId}</b></color> <color=#FFE082>(x{item.quantity})</color>");
                    }
                    else
                    {
                        sb.Append($"<pos=50%><color=#80CBC4>•</color> <color=#FFFFFF><b>{item.itemId}</b></color> <color=#FFE082>(x{item.quantity})</color>");
                    }
                    idx++;
                }
                summaryInventoryText.text = sb.Length == 0 ? "Empty" : sb.ToString().TrimEnd();
            }
        }

        private static string FormatStatPips(int val, int max = 10)
        {
            int filled = Mathf.Clamp(val, 0, max);
            int empty = Mathf.Max(0, max - filled);
            return $"<color=#FFA726>{new string('■', filled)}</color><color=#243242>{new string('■', empty)}</color>";
        }

        private void EnforceDossierLayout()
        {
            if (summaryDossierPanel == null) return;

            Transform frame = summaryDossierPanel.transform.Find("FederalDossierFrame");
            if (frame == null) return;

            RectTransform frameRect = frame.GetComponent<RectTransform>();
            if (frameRect != null)
            {
                frameRect.sizeDelta = new Vector2(1420, 760);
                frameRect.anchoredPosition = Vector2.zero;
            }
            if (frame.GetComponent<RectMask2D>() == null)
            {
                frame.gameObject.AddComponent<RectMask2D>();
            }

            Transform cols = frame.Find("ColumnsContainer");
            if (cols != null)
            {
                if (cols.GetComponent<RectMask2D>() == null)
                {
                    cols.gameObject.AddComponent<RectMask2D>();
                }

                RectTransform cRect = cols.GetComponent<RectTransform>();
                if (cRect != null)
                {
                    cRect.anchorMin = new Vector2(0, 0);
                    cRect.anchorMax = new Vector2(1, 1);
                    cRect.offsetMin = new Vector2(18, 76);
                    cRect.offsetMax = new Vector2(-18, -96);
                }

                HorizontalLayoutGroup hlg = cols.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.childControlHeight = true;
                    hlg.childForceExpandHeight = true;
                    hlg.spacing = 16;
                }

                for (int i = 0; i < cols.childCount; i++)
                {
                    Transform card = cols.GetChild(i);
                    if (card.GetComponent<RectMask2D>() == null)
                    {
                        card.gameObject.AddComponent<RectMask2D>();
                    }
                    LayoutElement le = card.GetComponent<LayoutElement>();
                    if (le != null)
                    {
                        le.preferredHeight = 580;
                        le.flexibleHeight = 0;
                    }
                    RectTransform crt = card.GetComponent<RectTransform>();
                    if (crt != null)
                    {
                        crt.sizeDelta = new Vector2(crt.sizeDelta.x, 580);
                    }

                    VerticalLayoutGroup vlg = card.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.padding = new RectOffset(16, 16, 12, 12);
                        vlg.spacing = 6;
                        vlg.childControlWidth = true;
                        vlg.childForceExpandWidth = true;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandHeight = false;
                    }
                }

                Transform col1 = cols.Find("Col1_Identity");
                if (col1 != null)
                {
                    Transform stamp = col1.Find("StampHolder");
                    if (stamp != null)
                    {
                        LayoutElement stLe = stamp.GetComponent<LayoutElement>();
                        if (stLe != null) stLe.ignoreLayout = true;

                        RectTransform sRect = stamp.GetComponent<RectTransform>();
                        if (sRect != null)
                        {
                            sRect.anchorMin = new Vector2(0.5f, 0f);
                            sRect.anchorMax = new Vector2(0.5f, 0f);
                            sRect.pivot = new Vector2(0.5f, 0f);
                            sRect.sizeDelta = new Vector2(175, 175);
                            sRect.anchoredPosition = new Vector2(25, 18);
                        }
                    }
                }

                Transform col2 = cols.Find("Col2_Aptitudes");
                if (col2 != null)
                {
                    TextMeshProUGUI[] col2Texts = col2.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in col2Texts)
                    {
                        if (t != null && t.text != null)
                        {
                            if (t.text.Contains("S.P.A.I.C. ATTRIBUTES"))
                                t.text = "02 // M.O.R.T.A.L. ATTRIBUTES & PROFICIENCIES";
                            else if (t.text.Contains("S.P.A.I.C. BIOLOGICAL"))
                                t.text = "M.O.R.T.A.L. BIOLOGICAL APTITUDE INDEX:";
                        }
                    }

                    Transform advBox = col2.Find("AdvisoryBox");
                    if (advBox != null)
                    {
                        RectTransform abRect = advBox.GetComponent<RectTransform>();
                        if (abRect != null)
                        {
                            abRect.sizeDelta = new Vector2(0, 138);
                        }
                        TextMeshProUGUI abTmp = advBox.GetComponentInChildren<TextMeshProUGUI>();
                        if (abTmp != null)
                        {
                            abTmp.fontSize = 11.5f;
                            abTmp.lineSpacing = 2.5f;
                        }
                    }
                }

                Transform col3 = cols.Find("Col3_Manifest");
                if (col3 != null)
                {
                    if (summaryTraitsText != null)
                    {
                        summaryTraitsText.textWrappingMode = TextWrappingModes.Normal;
                        summaryTraitsText.overflowMode = TextOverflowModes.Overflow;
                    }

                    Transform edictBox = col3.Find("EdictBox");
                    if (edictBox != null)
                    {
                        RectTransform ebRect = edictBox.GetComponent<RectTransform>();
                        if (ebRect != null)
                        {
                            ebRect.sizeDelta = new Vector2(0, 128);
                        }
                        TextMeshProUGUI ebTmp = edictBox.GetComponentInChildren<TextMeshProUGUI>();
                        if (ebTmp != null)
                        {
                            ebTmp.fontSize = 11.5f;
                            ebTmp.lineSpacing = 2.5f;
                        }
                    }
                }

                if (cRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(cRect);
                }
            }

            Transform footer = frame.Find("FooterBar");
            if (footer != null)
            {
                RectTransform footRect = footer.GetComponent<RectTransform>();
                if (footRect != null)
                {
                    footRect.anchorMin = new Vector2(0, 0);
                    footRect.anchorMax = new Vector2(1, 0);
                    footRect.pivot = new Vector2(0.5f, 0);
                    footRect.sizeDelta = new Vector2(0, 72);
                    footRect.anchoredPosition = Vector2.zero;
                }

                Transform notice = footer.Find("WarningNotice");
                if (notice != null)
                {
                    RectTransform nr = notice.GetComponent<RectTransform>();
                    if (nr != null)
                    {
                        nr.anchorMin = new Vector2(0, 0.5f);
                        nr.anchorMax = new Vector2(0, 0.5f);
                        nr.pivot = new Vector2(0, 0.5f);
                        nr.sizeDelta = new Vector2(680, 50);
                        nr.anchoredPosition = new Vector2(24, 0);
                    }
                    TextMeshProUGUI nTmp = notice.GetComponent<TextMeshProUGUI>();
                    if (nTmp != null)
                    {
                        nTmp.fontSize = 11.5f;
                        nTmp.textWrappingMode = TextWrappingModes.Normal;
                    }
                }

                Transform begin = footer.Find("Btn_CONFIRM&COMMENCESURVIVAL>>");
                if (begin != null)
                {
                    RectTransform br = begin.GetComponent<RectTransform>();
                    if (br != null)
                    {
                        br.anchorMin = new Vector2(1, 0.5f);
                        br.anchorMax = new Vector2(1, 0.5f);
                        br.pivot = new Vector2(1, 0.5f);
                        br.sizeDelta = new Vector2(350, 46);
                        br.anchoredPosition = new Vector2(-24, 0);
                    }
                }

                Transform resetObj = footer.Find("Btn_StartOver");
                if (resetObj == null)
                {
                    GameObject rBtnObj = new GameObject("Btn_StartOver");
                    rBtnObj.transform.SetParent(footer, false);
                    RectTransform rRect = rBtnObj.AddComponent<RectTransform>();
                    rRect.anchorMin = new Vector2(1, 0.5f);
                    rRect.anchorMax = new Vector2(1, 0.5f);
                    rRect.pivot = new Vector2(1, 0.5f);
                    rRect.sizeDelta = new Vector2(210, 46);
                    rRect.anchoredPosition = new Vector2(-386, 0);

                    Image rBg = rBtnObj.AddComponent<Image>();
                    rBg.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);

                    Outline rOutline = rBtnObj.AddComponent<Outline>();
                    rOutline.effectColor = new Color(0.35f, 0.45f, 0.58f, 0.75f);
                    rOutline.effectDistance = new Vector2(1, -1);

                    Button rBtn = rBtnObj.AddComponent<Button>();
                    rBtn.targetGraphic = rBg;
                    ColorBlock cb = rBtn.colors;
                    cb.normalColor = new Color(0.12f, 0.16f, 0.22f, 0.95f);
                    cb.highlightedColor = new Color(0.65f, 0.20f, 0.18f, 1f);
                    cb.pressedColor = new Color(0.45f, 0.12f, 0.10f, 1f);
                    rBtn.colors = cb;
                    rBtn.onClick.AddListener(RestartCharacterCreation);

                    TextMeshProUGUI tmp = CreateUIText(rBtnObj.transform, "Text", "<b><color=#CFD8DC><< START OVER</color></b>",
                        13.5f, TextAlignmentOptions.Center, Color.white, FontRole.Display,
                        Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                    restartCreationButton = rBtn;
                }
                else
                {
                    RectTransform rRect = resetObj.GetComponent<RectTransform>();
                    if (rRect != null)
                    {
                        rRect.anchorMin = new Vector2(1, 0.5f);
                        rRect.anchorMax = new Vector2(1, 0.5f);
                        rRect.pivot = new Vector2(1, 0.5f);
                        rRect.sizeDelta = new Vector2(210, 46);
                        rRect.anchoredPosition = new Vector2(-386, 0);
                    }
                    TextMeshProUGUI rTxt = resetObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (rTxt != null) rTxt.text = "<b><color=#CFD8DC><< START OVER</color></b>";
                    Button rBtn = resetObj.GetComponent<Button>();
                    if (rBtn != null)
                    {
                        rBtn.onClick.RemoveAllListeners();
                        rBtn.onClick.AddListener(RestartCharacterCreation);
                        restartCreationButton = rBtn;
                    }
                }
            }
        }

        public void RestartCharacterCreation()
        {
            PlayClick();
            ResetCreationState();
            ShowGenderPanel();
        }

        private void OnBeginSurvivalClicked()
        {
            PlayClick();

            if (clockTickSource != null) clockTickSource.Stop();
            if (AudioManager.Instance != null) AudioManager.Instance.StopBGM(1.5f);

            Hide(0.8f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteCharacterCreation(character);
                }
            });
        }
        #endregion

        #region UI Construction Helpers
        private static List<Sprite> _cachedPlayerFrames;

        public static List<Sprite> LoadPlayerFrames()
        {
            if (_cachedPlayerFrames != null && _cachedPlayerFrames.Count > 0) return _cachedPlayerFrames;

            _cachedPlayerFrames = new List<Sprite>();
            string framesFolder = "Assets/Sprites/Avatars/PlayerFrames";
            if (System.IO.Directory.Exists(framesFolder))
            {
                string[] files = System.IO.Directory.GetFiles(framesFolder, "player_*.png");
                System.Array.Sort(files);
                foreach (var file in files)
                {
                    Sprite s = LoadSpriteSafe(file);
                    if (s != null) _cachedPlayerFrames.Add(s);
                }
            }
            return _cachedPlayerFrames;
        }

        public static Sprite LoadSpriteSafe(string path)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
#endif
            if (!System.IO.File.Exists(path)) return null;
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Bilinear;
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[CharacterCreationScreen] Could not load sprite at {path}: {ex.Message}");
            }
            return null;
        }

        private static Sprite GetOrCreateSilhouetteSprite()
        {
            if (_cachedSilhouetteSprite != null) return _cachedSilhouetteSprite;

            // 1. Try loading primary avatar from Assets/Sprites/Avatars/player.png
            Sprite loaded = LoadSpriteSafe("Assets/Sprites/Avatars/player.png");
            if (loaded != null)
            {
                _cachedSilhouetteSprite = loaded;
                return _cachedSilhouetteSprite;
            }

            // 2. Procedural fallback if asset not present
            int w = 64;
            int h = 72;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x - w * 0.5f) / (w * 0.5f);
                    // Head circle centered at y = 48, radius = 12
                    float headDx = (x - 32f) / 11.5f;
                    float headDy = (y - 49f) / 13.0f;
                    float headDist = headDx * headDx + headDy * headDy;

                    // Neck centered at y = 33..38
                    bool inNeck = (y >= 31 && y <= 38) && Mathf.Abs(x - 32f) <= 5.5f;

                    // Shoulders / torso (y = 6..33)
                    float shoulderHalfWidth = 23f * Mathf.Clamp01((34f - y) / 10f);
                    bool inTorso = (y >= 6 && y <= 33) && Mathf.Abs(x - 32f) <= shoulderHalfWidth;

                    bool inside = (headDist <= 1.0f) || inNeck || inTorso;
                    if (inside)
                    {
                        float scanline = (y % 3 == 0) ? 0.72f : 0.95f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, scanline));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            _cachedSilhouetteSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return _cachedSilhouetteSprite;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string name, string content, float size,
            TextAlignmentOptions align, Color color, FontRole role,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            // IMPORTANT: Add TextMeshProUGUI BEFORE setting RectTransform offsets so TMP's Reset() does not overwrite sizeDelta!
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();

            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            if (role == FontRole.Narrative) tmp.fontStyle = FontStyles.Italic;
            SundayTypography.ApplyFont(tmp, role);
            return tmp;
        }

        private static GameObject CreateControlModuleBox(Transform parent, string name, float topY, float height, string headerTitle)
        {
            GameObject box = new GameObject(name);
            box.transform.SetParent(parent, false);
            RectTransform rt = box.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, topY);

            box.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.12f, 0.96f);
            Outline ol = box.AddComponent<Outline>();
            ol.effectColor = new Color(0.15f, 0.24f, 0.35f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            CreateModuleHeader(box.transform, headerTitle);
            return box;
        }

        private static void CreateModuleHeader(Transform parent, string title)
        {
            GameObject hObj = new GameObject("ModuleHeader");
            hObj.transform.SetParent(parent, false);
            RectTransform hRect = hObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0f, 28f);
            hRect.anchoredPosition = Vector2.zero;
            hObj.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.22f, 0.98f);

            CreateUIText(hObj.transform, "Title", $"<b>{title}</b>",
                11.5f, TextAlignmentOptions.MidlineLeft, new Color(0.45f, 0.76f, 0.98f), FontRole.Mono,
                Vector2.zero, Vector2.one, new Vector2(14f, 0f), new Vector2(-14f, 0f));
        }

        private static Button CreateGenderArchetypeButton(Transform parent, string name, string title, string sub, float posX, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(268f, 54f);
            rt.anchoredPosition = new Vector2(posX, -38f);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.18f, 0.28f, 0.40f, 0.75f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            GameObject strip = new GameObject("LeftStrip");
            strip.transform.SetParent(btnObj.transform, false);
            RectTransform sRect = strip.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(0f, 1f);
            sRect.pivot = new Vector2(0f, 0.5f);
            sRect.sizeDelta = new Vector2(4f, 0f);
            sRect.anchoredPosition = Vector2.zero;
            strip.AddComponent<Image>().color = new Color(0.90f, 0.49f, 0.13f, 1f);

            CreateUIText(btnObj.transform, "Title", title,
                16f, TextAlignmentOptions.BottomLeft, Color.white, FontRole.Display,
                new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(16f, 0f), new Vector2(-10f, -4f));

            CreateUIText(btnObj.transform, "Sub", sub,
                10.5f, TextAlignmentOptions.TopLeft, new Color(0.58f, 0.68f, 0.78f), FontRole.Body,
                new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Vector2(16f, 4f), new Vector2(-10f, 0f));

            return btn;
        }

        private static Button CreateNameChipButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject chipObj = new GameObject("Chip_" + label);
            chipObj.transform.SetParent(parent, false);
            RectTransform rt = chipObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(196f, 36f);

            Image bg = chipObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);
            Outline ol = chipObj.AddComponent<Outline>();
            ol.effectColor = new Color(0.16f, 0.24f, 0.34f, 0.65f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = chipObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            CreateUIText(chipObj.transform, "Text", label.ToUpperInvariant(),
                13f, TextAlignmentOptions.Center, new Color(0.85f, 0.88f, 0.92f), FontRole.Mono,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return btn;
        }

        private static void CreateInlineCustomInput(Transform parent, string name, string label, string placeholder,
            float posY, string btnLabel, UnityEngine.Events.UnityAction onBtnClick,
            out TMP_InputField inputField, UnityEngine.Events.UnityAction<string> onValueChanged)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            RectTransform rRect = row.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0f, 1f);
            rRect.anchorMax = new Vector2(1f, 1f);
            rRect.pivot = new Vector2(0.5f, 1f);
            rRect.sizeDelta = new Vector2(-32f, 34f);
            rRect.anchoredPosition = new Vector2(0f, posY);

            CreateUIText(row.transform, "Label", $"<b>{label}</b>",
                11f, TextAlignmentOptions.MidlineLeft, new Color(0.58f, 0.70f, 0.82f), FontRole.Mono,
                new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(165f, 0f));

            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            RectTransform iRect = inputObj.AddComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0f, 0f);
            iRect.anchorMax = new Vector2(1f, 1f);
            iRect.offsetMin = new Vector2(170f, 0f);
            iRect.offsetMax = new Vector2(-176f, 0f);

            Image iBg = inputObj.AddComponent<Image>();
            iBg.color = new Color(0.03f, 0.05f, 0.08f, 1f);
            Outline iOl = inputObj.AddComponent<Outline>();
            iOl.effectColor = new Color(0.20f, 0.34f, 0.50f, 0.85f);
            iOl.effectDistance = new Vector2(1f, -1f);

            inputField = inputObj.AddComponent<TMP_InputField>();

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(12f, 2f);
            taRect.offsetMax = new Vector2(-12f, -2f);
            textArea.AddComponent<RectMask2D>();

            TextMeshProUGUI phText = CreateUIText(textArea.transform, "Placeholder", placeholder,
                13f, TextAlignmentOptions.MidlineLeft, new Color(0.42f, 0.50f, 0.58f, 0.75f), FontRole.Body,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            phText.fontStyle = FontStyles.Italic;

            TextMeshProUGUI valText = CreateUIText(textArea.transform, "Text", "",
                14f, TextAlignmentOptions.MidlineLeft, Color.white, FontRole.Body,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            inputField.textViewport = taRect;
            inputField.textComponent = valText;
            inputField.placeholder = phText;
            if (onValueChanged != null) inputField.onValueChanged.AddListener(onValueChanged);

            CreateTacticalActionButton(row.transform, "ActionBtn", btnLabel,
                new Vector2(1f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(164f, 0f),
                new Color(0.10f, 0.20f, 0.32f, 0.98f), new Color(0.25f, 0.52f, 0.78f, 0.85f),
                onBtnClick, pivotX: 1f, pivotY: 0.5f, fontSize: 12f);
        }

        private static Button CreateTacticalActionButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
            Color bgColor, Color outlineColor, UnityEngine.Events.UnityAction onClick,
            float pivotX = 0f, float pivotY = 0f, float fontSize = 13f)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(pivotX, pivotY);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;
            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = outlineColor;
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = Color.Lerp(bgColor, outlineColor, 0.45f);
            cb.pressedColor = bgColor * 0.8f;
            cb.selectedColor = bgColor;
            btn.colors = cb;

            if (onClick != null) btn.onClick.AddListener(onClick);

            CreateUIText(btnObj.transform, "Text", $"<b>{label}</b>",
                fontSize, TextAlignmentOptions.Center, Color.white, FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return btn;
        }
        #endregion

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
