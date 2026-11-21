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

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDRootBackdrop();
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
                if (!clockTickSource.isPlaying || clockTickSource.clip != clockTickClip)
                {
                    clockTickSource.clip = clockTickClip;
                    clockTickSource.volume = 0.25f;
                    clockTickSource.Play();
                }
            }

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDRootBackdrop();
            }

            EnsureIdentityConsoleBuilt();
            EnsureJobBackButton();
            EnsureHobbyTopBar();
            InitializeButtons();
            SundayTypography.ApplyHierarchy(transform);
            ShowGenderPanel();
        }

        protected override void OnScreenClosed()
        {
            base.OnScreenClosed();
            if (clockTickSource != null && clockTickSource.isPlaying)
            {
                clockTickSource.Stop();
            }
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
            if (string.IsNullOrEmpty(cleanLast)) cleanLast = "CITIZEN";
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
                        if (icon == null)
                        {
                            icon = ItemDatabase.GetTraitIcon(perk.name);
                        }
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
            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDIdentityConsoleTheme();
            }
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
                character.gender = "Other";
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
                if (StyleDThemeUtility.IsStyleDActive) ApplyStyleDIdentityConsoleTheme();
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

            CreateModuleHeader(mirrorBox.transform, "■ MIRROR REFLECTION & CITIZEN PROFILE");

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

            CreateInlineCustomInput(modGender.transform, "CustomGenderRow", "CUSTOM IDENTITY:", "Or type custom identity (e.g. Non-Binary)...",
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
            if (StyleDThemeUtility.IsStyleDActive) ApplyStyleDIdentityConsoleTheme();
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
            bool isStyleD = StyleDThemeUtility.IsStyleDActive;

            if (topBannerCaseIdText != null)
            {
                topBannerCaseIdText.text = isStyleD
                    ? $"CASE FILE: <color=#E0A43A><b>{docNo}</b></color>   |   <color=#D95829><b>[STATUS: CIVIL DANGER LEVEL 4]</b></color>"
                    : $"CASE FILE: <color=#FFE082><b>{docNo}</b></color>   |   <color=#EF5350><b>[ STATUS: CIVIL DANGER WARNING // LEVEL 4 ]</b></color>";
            }

            if (idCardLastNameText != null) idCardLastNameText.text = ln.ToUpperInvariant();
            if (idCardFirstNameText != null) idCardFirstNameText.text = fn.ToUpperInvariant();

            Color profileTint = isStyleD
                ? StyleDThemeUtility.ColAmber
                : (isMale
                    ? new Color(0.45f, 0.82f, 0.98f, 0.95f)
                    : (isFemale ? new Color(0.98f, 0.58f, 0.75f, 0.95f) : new Color(0.75f, 0.60f, 1.0f, 0.95f)));

            if (idCardSilhouetteImg != null)
            {
                idCardSilhouetteImg.color = isStyleD ? new Color(0.68f, 1.0f, 0.84f, 0.96f) : Color.white;
            }
            if (idCardEmblemText != null)
            {
                string code = isMale ? "MALE" : (isFemale ? "FEMALE" : "NON-BINARY");
                idCardEmblemText.text = $"<b>[{code}]</b>";
                idCardEmblemText.color = profileTint;
            }

            if (idCardMetaText != null)
            {
                if (isStyleD)
                {
                    idCardMetaText.text =
                        $"<color=#9A9488>SEX / ID:</color> <color=#E0A43A><b>{g.ToUpperInvariant()}</b></color><pos=52%><color=#9A9488>BLOOD:</color> <color=#D95829><b>{blood}</b></color>\n" +
                        $"<color=#9A9488>DOB:</color>      <color=#E4DFD1>{dob}</color><pos=52%><color=#9A9488>ZONE:</color>  <color=#6EE7B7>SECTOR-04</color>\n" +
                        $"<color=#9A9488>CASE ID:</color>  <color=#E0A43A>{docNo}</color>";
                }
                else
                {
                    idCardMetaText.text =
                        $"<color=#78909C>SEX / ID:</color> <color=#FFE082><b>{g.ToUpperInvariant()}</b></color><pos=52%><color=#78909C>BLOOD:</color> <color=#EF5350><b>{blood}</b></color>\n" +
                        $"<color=#78909C>DOB:</color>      <color=#FFFFFF>{dob}</color><pos=52%><color=#78909C>ZONE:</color>  <color=#80CBC4>SECTOR-04</color>\n" +
                        $"<color=#78909C>CASE ID:</color>  <color=#4FC3F7>{docNo}</color>";
                }
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
                idCardActiveRecordText.text = isStyleD
                    ? $"<color=#9A9488><b>CITIZEN:</b></color> <color=#E4DFD1><b>{fn.ToUpperInvariant()} {ln.ToUpperInvariant()}</b></color>  <color=#6E6A62>|</color>  <color=#9A9488><b>CASE:</b></color> <color=#E0A43A>{docNo}</color>"
                    : $"<b>CITIZEN:</b> <color=#FFFFFF>{fn.ToUpperInvariant()} {ln.ToUpperInvariant()}</color>  <color=#78909C>|</color>  <b>CASE:</b> <color=#4FC3F7>{docNo}</color>";
            }
        }

        private void StyleGenderButton(Button btn, bool isSelected, string baseTitle)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            Outline ol = btn.GetComponent<Outline>();
            TextMeshProUGUI titleTmp = btn.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI subTmp = btn.transform.Find("Sub")?.GetComponent<TextMeshProUGUI>();
            Image dotImg = btn.transform.Find("LeftStrip")?.GetComponent<Image>();

            if (StyleDThemeUtility.IsStyleDActive)
            {
                Color norm = isSelected ? new Color(0.72f, 0.34f, 0.14f, 1f) : new Color(0.23f, 0.26f, 0.30f, 1f);
                Color hov = isSelected ? new Color(0.84f, 0.42f, 0.18f, 1f) : new Color(0.32f, 0.36f, 0.42f, 1f);
                StyleDThemeUtility.ApplyChipButton(btn, norm, hov, 1.5f);
                if (dotImg != null)
                    dotImg.color = isSelected ? new Color(0.96f, 0.72f, 0.26f, 1f) : new Color(0.14f, 0.16f, 0.18f, 0.75f);
                if (titleTmp != null)
                {
                    titleTmp.text = isSelected ? $"● {baseTitle}" : baseTitle;
                    titleTmp.color = isSelected ? new Color(1f, 0.95f, 0.84f, 1f) : StyleDThemeUtility.ColBone;
                }
                if (subTmp != null)
                {
                    subTmp.color = isSelected ? new Color(0.92f, 0.89f, 0.82f, 1f) : StyleDThemeUtility.ColMuted;
                }
                return;
            }

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
            bool isStyleD = StyleDThemeUtility.IsStyleDActive;
            for (int i = 0; i < container.childCount; i++)
            {
                Transform chip = container.GetChild(i);
                string chipVal = chip.name.Replace("Chip_", "");
                bool active = string.Equals(chipVal, activeValue, System.StringComparison.OrdinalIgnoreCase);

                Image bg = chip.GetComponent<Image>();
                Outline ol = chip.GetComponent<Outline>();
                TextMeshProUGUI txt = chip.GetComponentInChildren<TextMeshProUGUI>();
                Button btn = chip.GetComponent<Button>();

                if (isStyleD)
                {
                    if (btn != null)
                    {
                        Color norm = active ? new Color(0.78f, 0.37f, 0.15f, 1f) : new Color(0.23f, 0.26f, 0.30f, 1f);
                        Color hov = active ? new Color(0.88f, 0.44f, 0.19f, 1f) : new Color(0.32f, 0.36f, 0.42f, 1f);
                        StyleDThemeUtility.ApplyChipButton(btn, norm, hov, 1.65f);
                    }
                    if (txt != null)
                    {
                        SundayTypography.ApplyFont(txt, FontRole.Mono);
                        txt.text = active ? $"<b>● {chipVal.ToUpperInvariant()}</b>" : chipVal.ToUpperInvariant();
                        txt.color = active ? new Color(1f, 0.96f, 0.85f, 1f) : new Color(0.84f, 0.82f, 0.77f, 1f);
                    }
                    continue;
                }

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

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDJobPanelTheme();
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
                EnsureStudentPanelMortalOrder();
                if (StyleDThemeUtility.IsStyleDActive)
                {
                    ApplyStyleDStudentTrainingTheme();
                }
            }
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            RebuildCharacterFromSelections();
            InspectStudentStat("str", lockSelection: false);
            UpdateStudentTrainingUI();
        }

        private void EnsureStudentPanelMortalOrder()
        {
            if (studentTrainingPanel == null) return;
            var allTransforms = studentTrainingPanel.GetComponentsInChildren<Transform>(true);
            Transform rows = null;
            foreach (var t in allTransforms)
            {
                if (t.name == "AllocTitle")
                {
                    TextMeshProUGUI titleTmp = t.GetComponent<TextMeshProUGUI>();
                    if (titleTmp != null)
                    {
                        titleTmp.text = "M.O.R.T.A.L. ATTRIBUTES ALLOCATION";
                        SundayTypography.ApplyFont(titleTmp, FontRole.Display);
                    }
                }
                else if (t.name == "AllocSubtitle")
                {
                    TextMeshProUGUI subTmp = t.GetComponent<TextMeshProUGUI>();
                    if (subTmp != null)
                    {
                        subTmp.text = "\"Distribute 5 AP points across your M.O.R.T.A.L. profile.\"";
                    }
                }
                else if (t.name == "AttributesRows")
                {
                    rows = t;
                }
            }

            if (rows != null)
            {
                // Order rows as M.O.R.T.A.L.: STRENGTH (MIGHT), PERCEPTION (OBSERVATION), AGILITY (REFLEX), INTELLIGENCE (TACTICS), CHARISMA (LEADERSHIP)
                string[] mortalRowOrder = { "Row_STRENGTH", "Row_PERCEPTION", "Row_AGILITY", "Row_INTELLIGENCE", "Row_CHARISMA" };
                for (int i = 0; i < mortalRowOrder.Length; i++)
                {
                    Transform r = rows.Find(mortalRowOrder[i]);
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
                    inspectorStatTitle.text = "[M] MIGHT (PHYSICAL POWER)";
                    inspectorStatTitle.color = new Color(0.93f, 0.33f, 0.31f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Muscles conditioned from athletics, manual labor, and gym drills.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Determines raw physical power, encumbrance limit, and brute force survival actions in combat and exploration.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#EF5350><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Melee Combat:</b></color> +15% damage with blunt and edged weapons\n" +
                        "• <color=#FFFFFF><b>Carrying Capacity:</b></color> +8 kg maximum inventory encumbrance\n" +
                        "• <color=#FFFFFF><b>Breaching:</b></color> Force open jammed doors and barricades without tools";
                    break;

                case "int":
                    inspectorStatTitle.text = "[T] TACTICS (ANALYTICAL MIND)";
                    inspectorStatTitle.color = new Color(0.26f, 0.65f, 0.96f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Late-night academic research, problem solving, and technical curiosity.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs scientific knowledge, electronic engineering, and the efficiency of medical supplies under pressure.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#42A5F5><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Electronics & Tech:</b></color> +20% success when bypassing security and hotwiring\n" +
                        "• <color=#FFFFFF><b>Field Medicine:</b></color> Bandages, antibiotics & medkits restore +25% extra HP\n" +
                        "• <color=#FFFFFF><b>Crafting Efficiency:</b></color> Unlock advanced survival blueprints with less scrap";
                    break;

                case "agi":
                    inspectorStatTitle.text = "[R] REFLEX (MOBILITY & EVASION)";
                    inspectorStatTitle.color = new Color(0.40f, 0.73f, 0.42f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Quick reflexes, track sprinting, and agile body coordination.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Determines movement swiftness, noise generation while sneaking, and reactive evasion when ambushed.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#66BB6A><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Sprint Stamina:</b></color> -10% stamina drain while sprinting away from hazards\n" +
                        "• <color=#FFFFFF><b>Stealth Evasion:</b></color> Sound footprint reduced by 15% across all surfaces\n" +
                        "• <color=#FFFFFF><b>Dodge Chance:</b></color> +12% chance to evade enemy lunges and grab attacks";
                    break;

                case "cha":
                    inspectorStatTitle.text = "[L] LEADERSHIP (FORCE OF PERSONALITY)";
                    inspectorStatTitle.color = new Color(0.67f, 0.28f, 0.74f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Campus diplomacy, debate leadership, and unwavering social poise.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs interpersonal influence, trade negotiations with survivors, and psychological resilience.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#AB47BC><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
                        "• <color=#FFFFFF><b>Dialogue Checks:</b></color> Significant persuasion and de-escalation advantages\n" +
                        "• <color=#FFFFFF><b>Barter Economy:</b></color> 10% discount when trading goods with neutral survivors\n" +
                        "• <color=#FFFFFF><b>Morale & Composure:</b></color> High resistance to psychological panic checks";
                    break;

                case "per":
                    inspectorStatTitle.text = "[O] OBSERVATION (SITUATIONAL ALERTNESS)";
                    inspectorStatTitle.color = new Color(1.00f, 0.65f, 0.15f);
                    if (inspectorStatQuote != null) inspectorStatQuote.text = "\"Keen senses—noticing subtle sounds, shadows, and concealed hazards.\"";
                    if (inspectorStatDesc != null) inspectorStatDesc.text = "Governs sensory alertness, firearm accuracy at range, and the ability to scavenge hidden supplies.";
                    if (inspectorStatImpact != null) inspectorStatImpact.text = 
                        "<color=#FFA726><b>SURVIVAL IMPACT ANALYSIS:</b></color>\n" +
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
                if (StyleDThemeUtility.IsStyleDActive)
                {
                    ApplyStyleDAcademicHeaderTheme(studentMajorPanel);
                }
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
                    perkIcon = "field_medic";
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
                if (icon == null)
                    icon = ItemDatabase.GetTraitIcon(perkName);
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

                if (StyleDThemeUtility.IsStyleDActive)
                {
                    ApplyStyleDAcademicHeaderTheme(studentMinorPanel);
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
                    hobbyHeaderTitle.text = "CHARACTER VULNERABILITY <size=75%><color=#EF5350>[CRITICAL DRAWBACK]</color></size>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"Nobody is without flaws. What is your critical drawback?\"";
                }
            }

            UpdateHobbyTopBarStatus();

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDHobbyPanelTheme();
            }

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
                RebuildCharacterFromSelections();
                ShowHobbySelectionPanel();
            }
            else if (hobbyStep == 2)
            {
                // Going back from Secondary Hobby to Primary Hobby:
                // Clear all picked hobbies so player can cleanly re-pick Primary Hobby!
                pickedHobbyIds.Clear();
                hobbyStep = 1;
                activeWeaknessTab = 0;
                RebuildCharacterFromSelections();
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

            if (StyleDThemeUtility.IsStyleDActive)
            {
                Color inactiveBg = new Color(0.22f, 0.25f, 0.29f, 1f);
                Color inactiveHov = new Color(0.32f, 0.36f, 0.42f, 1f);
                Color activeBg = hobbyStep < 3
                    ? new Color(0.68f, 0.31f, 0.14f, 1f)
                    : new Color(0.64f, 0.20f, 0.16f, 1f);
                Color activeHov = hobbyStep < 3
                    ? new Color(0.80f, 0.38f, 0.18f, 1f)
                    : new Color(0.78f, 0.25f, 0.20f, 1f);

                StyleDThemeUtility.ApplyChipButton(physicalTabButton, isTab0 ? activeBg : inactiveBg, isTab0 ? activeHov : inactiveHov, 1.5f);
                StyleDThemeUtility.ApplyChipButton(mentalTabButton, !isTab0 ? activeBg : inactiveBg, !isTab0 ? activeHov : inactiveHov, 1.5f);

                if (hobbyStep < 3)
                {
                    if (physicalTabText != null)
                        physicalTabText.text = isTab0
                            ? "<b><color=#FFF5DF>● COMBAT, FIELD & STEALTH (8)</color></b>"
                            : "<b><color=#9A9488>○ COMBAT, FIELD & STEALTH (8)</color></b>";
                    if (mentalTabText != null)
                        mentalTabText.text = !isTab0
                            ? "<b><color=#FFF5DF>● MIND, TECH & SUPPORT (8)</color></b>"
                            : "<b><color=#9A9488>○ MIND, TECH & SUPPORT (8)</color></b>";
                }
                else
                {
                    if (physicalTabText != null)
                        physicalTabText.text = isTab0
                            ? "<b><color=#FFF5DF>● PHYSICAL FLAWS (10)</color></b>"
                            : "<b><color=#9A9488>○ PHYSICAL FLAWS (10)</color></b>";
                    if (mentalTabText != null)
                        mentalTabText.text = !isTab0
                            ? "<b><color=#FFF5DF>● MENTAL & SOCIAL (10)</color></b>"
                            : "<b><color=#9A9488>○ MENTAL & SOCIAL (10)</color></b>";
                }
                return;
            }

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
                if (child == null) continue;
                child.gameObject.SetActive(false);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
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
                    HobbyCardView.EquippedHobbyStatus status = HobbyCardView.EquippedHobbyStatus.None;
                    if (pickedHobbyIds.Count > 0 && string.Equals(pickedHobbyIds[0], hobby.id, System.StringComparison.OrdinalIgnoreCase))
                    {
                        status = HobbyCardView.EquippedHobbyStatus.Primary;
                    }
                    else if (pickedHobbyIds.Count > 1 && string.Equals(pickedHobbyIds[1], hobby.id, System.StringComparison.OrdinalIgnoreCase))
                    {
                        status = HobbyCardView.EquippedHobbyStatus.Secondary;
                    }

                    GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                    cardObj.SetActive(true);
                    spawnedCount++;
                    HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                    if (cardView != null)
                    {
                        HobbyData captured = hobby;
                        cardView.Setup(captured, () => OnHobbySelected(captured), status);
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

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDSummaryDossierTheme();
            }

            Canvas.ForceUpdateCanvases();
            Transform colsTr = summaryDossierPanel.transform.Find("FederalDossierFrame/ColumnsContainer");
            if (colsTr is RectTransform colsRebuildRt)
            {
                for (int i = 0; i < colsRebuildRt.childCount; i++)
                {
                    if (colsRebuildRt.GetChild(i) is RectTransform colChildRt)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(colChildRt);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(colsRebuildRt);
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
                            abRect.sizeDelta = new Vector2(0f, 156f);
                        }
                        LayoutElement abLe = advBox.GetComponent<LayoutElement>() ?? advBox.gameObject.AddComponent<LayoutElement>();
                        abLe.minHeight = 156f;
                        abLe.preferredHeight = 156f;

                        TextMeshProUGUI abTmp = advBox.GetComponentInChildren<TextMeshProUGUI>();
                        if (abTmp != null)
                        {
                            abTmp.fontSize = 11f;
                            abTmp.lineSpacing = 1.8f;
                            abTmp.rectTransform.offsetMin = new Vector2(14f, 12f);
                            abTmp.rectTransform.offsetMax = new Vector2(-14f, -12f);
                        }
                    }
                }

                Transform col3 = cols.Find("Col3_Manifest");
                if (col3 != null)
                {
                    if (summaryTraitsText != null)
                    {
                        summaryTraitsText.fontSize = 11f;
                        summaryTraitsText.lineSpacing = 1.5f;
                        summaryTraitsText.textWrappingMode = TextWrappingModes.Normal;
                        summaryTraitsText.overflowMode = TextOverflowModes.Overflow;
                    }

                    if (summaryInventoryText != null)
                    {
                        summaryInventoryText.fontSize = 11.5f;
                        summaryInventoryText.lineSpacing = 1.5f;
                    }

                    Transform edictBox = col3.Find("EdictBox");
                    if (edictBox != null)
                    {
                        RectTransform ebRect = edictBox.GetComponent<RectTransform>();
                        if (ebRect != null)
                        {
                            ebRect.sizeDelta = new Vector2(0f, 144f);
                        }
                        LayoutElement ebLe = edictBox.GetComponent<LayoutElement>() ?? edictBox.gameObject.AddComponent<LayoutElement>();
                        ebLe.minHeight = 144f;
                        ebLe.preferredHeight = 144f;

                        TextMeshProUGUI ebTmp = edictBox.GetComponentInChildren<TextMeshProUGUI>();
                        if (ebTmp != null)
                        {
                            ebTmp.fontSize = 10.5f;
                            ebTmp.lineSpacing = 1.5f;
                            ebTmp.rectTransform.offsetMin = new Vector2(14f, 12f);
                            ebTmp.rectTransform.offsetMax = new Vector2(-14f, -12f);
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

            if (StyleDThemeUtility.IsStyleDActive)
            {
                ApplyStyleDSummaryDossierTheme();
            }

            Canvas.ForceUpdateCanvases();
            Transform colsTr = summaryDossierPanel.transform.Find("FederalDossierFrame/ColumnsContainer");
            if (colsTr is RectTransform colsRebuildRt)
            {
                for (int i = 0; i < colsRebuildRt.childCount; i++)
                {
                    if (colsRebuildRt.GetChild(i) is RectTransform colChildRt)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(colChildRt);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(colsRebuildRt);
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

        #region Style D Hardware Theme
        private void ApplyStyleDRootBackdrop()
        {
            Image rootImg = GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.color = new Color(0.03f, 0.035f, 0.042f, 1f);
            }
        }

        private static string StripSlashesAndPrefix(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return raw.Replace("■", "").Replace("//", "-").Trim();
        }

        private void ApplyStyleDIdentityConsoleTheme()
        {
            if (genderPanel == null) return;
            ApplyStyleDRootBackdrop();

            Transform outerHdr = genderPanel.transform.Find("Header");
            if (outerHdr != null) outerHdr.gameObject.SetActive(false);

            Transform frame = genderPanel.transform.Find("IdentityConsoleFrame");
            if (frame == null) return;

            RectTransform fRect = frame as RectTransform;
            if (fRect != null)
            {
                fRect.sizeDelta = new Vector2(1520f, 830f);
                fRect.anchoredPosition = Vector2.zero;
            }
            StyleDThemeUtility.ApplySliced(frame.GetComponent<Image>(), StyleDThemeUtility.CcConsoleChassis, 2.4f, Color.white, disableOutline: true);

            // Top Banner inset inside chassis rail
            Transform topBanner = frame.Find("TopBanner");
            if (topBanner is RectTransform tbRt)
            {
                tbRt.anchorMin = new Vector2(0f, 1f);
                tbRt.anchorMax = new Vector2(1f, 1f);
                tbRt.pivot = new Vector2(0.5f, 1f);
                tbRt.offsetMin = new Vector2(24f, -62f);
                tbRt.offsetMax = new Vector2(-24f, -18f);

                Image tbImg = topBanner.GetComponent<Image>();
                if (tbImg != null) tbImg.color = Color.clear;

                Transform gDiv = topBanner.Find("GoldDivider");
                if (gDiv != null && gDiv.GetComponent<Image>() is Image gImg)
                    gImg.color = new Color(0.55f, 0.38f, 0.18f, 0.72f);

                TextMeshProUGUI bLeft = topBanner.Find("BannerLeft")?.GetComponent<TextMeshProUGUI>();
                if (bLeft != null)
                {
                    bLeft.text = "<b>FEMA BIO-REGISTRY - STEP 01: CITIZEN INTAKE</b>";
                    bLeft.fontSize = 18f;
                    bLeft.color = StyleDThemeUtility.ColBone;
                    SundayTypography.ApplyFont(bLeft, FontRole.Display);
                    bLeft.rectTransform.anchorMax = new Vector2(0.56f, 1f);
                    bLeft.rectTransform.offsetMin = new Vector2(14f, 0f);
                }

                TextMeshProUGUI bRight = topBanner.Find("BannerRight")?.GetComponent<TextMeshProUGUI>();
                if (bRight != null)
                {
                    bRight.fontSize = 12f;
                    bRight.color = StyleDThemeUtility.ColMuted;
                    SundayTypography.ApplyFont(bRight, FontRole.Mono);
                    bRight.rectTransform.anchorMin = new Vector2(0.56f, 0f);
                    bRight.rectTransform.offsetMax = new Vector2(-14f, 0f);
                }
            }

            // Remove StepTabsBar if present so Step 1 matches the rest of the wizard
            Transform existingTabs = frame.Find("StyleD_StepTabsBar");
            if (existingTabs != null) existingTabs.gameObject.SetActive(false);

            // Body layout - full height between top banner and footer
            Transform body = frame.Find("ConsoleBody");
            if (body is RectTransform bodyRt)
            {
                bodyRt.anchorMin = Vector2.zero;
                bodyRt.anchorMax = Vector2.one;
                bodyRt.offsetMin = new Vector2(26f, 76f);
                bodyRt.offsetMax = new Vector2(-26f, -70f);
            }

            Transform leftCol = frame.Find("ConsoleBody/LeftCol_CitizenCard");
            if (leftCol is RectTransform lcRt) lcRt.sizeDelta = new Vector2(530f, 0f);

            Transform rightCol = frame.Find("ConsoleBody/RightCol_Controls");
            if (rightCol is RectTransform rcRt) rcRt.sizeDelta = new Vector2(916f, 0f);

            // Left Column: Citizen ID Pass CRT
            Transform idCard = leftCol?.Find("USCitizenIDCard");
            if (idCard is RectTransform idRt)
            {
                idRt.sizeDelta = new Vector2(0f, 360f);
                idRt.anchoredPosition = new Vector2(0f, 0f);
                StyleDThemeUtility.ApplySliced(idCard.GetComponent<Image>(), StyleDThemeUtility.CcIdCrtHousing, 1.35f, Color.white, disableOutline: true);

                Transform idHdr = idCard.Find("IDHeader");
                if (idHdr is RectTransform idhRt)
                {
                    idhRt.sizeDelta = new Vector2(0f, 26f);
                    idhRt.anchoredPosition = new Vector2(0f, -17f);
                    Image hBg = idHdr.GetComponent<Image>();
                    if (hBg != null) hBg.color = Color.clear;

                    Transform gTop = idHdr.Find("GoldTopLine");
                    if (gTop != null) gTop.gameObject.SetActive(false);

                    TextMeshProUGUI hl = idHdr.Find("IDHeaderLeft")?.GetComponent<TextMeshProUGUI>();
                    if (hl != null)
                    {
                        hl.text = "<b>FEMA - CDC JOINT BIO-REGISTRY // CITIZEN ID PASS</b>";
                        hl.fontSize = 11f;
                        hl.color = StyleDThemeUtility.ColBone;
                        hl.rectTransform.offsetMin = new Vector2(24f, 0f);
                    }
                    TextMeshProUGUI hr = idHdr.Find("IDHeaderRight")?.GetComponent<TextMeshProUGUI>();
                    if (hr != null)
                    {
                        hr.fontSize = 10.5f;
                        hr.color = StyleDThemeUtility.ColAmber;
                        hr.rectTransform.offsetMax = new Vector2(-24f, 0f);
                    }
                }

                Transform pb = idCard.Find("PortraitBox");
                if (pb is RectTransform pbRt)
                {
                    pbRt.anchoredPosition = new Vector2(24f, -46f);
                    pbRt.sizeDelta = new Vector2(132f, 174f);
                    StyleDThemeUtility.ApplySliced(pb.GetComponent<Image>(), StyleDThemeUtility.CcBiometricGridWell, 1.45f, Color.white, disableOutline: true);

                    TextMeshProUGUI rtTop = pb.Find("ReticleTop")?.GetComponent<TextMeshProUGUI>();
                    if (rtTop != null)
                    {
                        rtTop.text = "[BIO-SCAN]";
                        rtTop.color = StyleDThemeUtility.ColSage;
                        rtTop.rectTransform.offsetMin = new Vector2(4f, -22f);
                        rtTop.rectTransform.offsetMax = new Vector2(-4f, -6f);
                    }

                    Transform sil = pb.Find("SilhouetteGraphic");
                    if (sil is RectTransform silRt)
                    {
                        silRt.sizeDelta = new Vector2(102f, 102f);
                        silRt.anchoredPosition = new Vector2(0f, 8f);
                    }

                    TextMeshProUGUI emb = pb.Find("EmblemSymbol")?.GetComponent<TextMeshProUGUI>();
                    if (emb != null)
                    {
                        emb.rectTransform.offsetMin = new Vector2(4f, 22f);
                        emb.rectTransform.offsetMax = new Vector2(-4f, 38f);
                    }

                    Transform pill = pb.Find("BioScanPill");
                    if (pill is RectTransform pillRt)
                    {
                        pillRt.anchoredPosition = new Vector2(0f, 6f);
                        pillRt.sizeDelta = new Vector2(-38f, 14f);
                        Image pillBg = pill.GetComponent<Image>();
                        if (pillBg != null) pillBg.color = Color.clear;
                        TextMeshProUGUI scanTxt = pill.Find("ScanText")?.GetComponent<TextMeshProUGUI>();
                        if (scanTxt != null)
                        {
                            scanTxt.text = "<b>[OK] TIER-1 CLEAR</b>";
                            scanTxt.fontSize = 8f;
                            scanTxt.overflowMode = TextOverflowModes.Overflow;
                            scanTxt.color = StyleDThemeUtility.ColSage;
                        }
                    }
                }

                Transform lf = idCard.Find("LiveFields");
                if (lf is RectTransform lfRt)
                {
                    lfRt.offsetMin = new Vector2(174f, -220f);
                    lfRt.offsetMax = new Vector2(-24f, -46f);

                    TextMeshProUGUI lblS = lf.Find("LblSurname")?.GetComponent<TextMeshProUGUI>();
                    if (lblS != null)
                    {
                        lblS.text = "01 - FAMILY SURNAME";
                        lblS.color = StyleDThemeUtility.ColMuted;
                        SundayTypography.ApplyFont(lblS, FontRole.Mono);
                    }

                    TextMeshProUGUI valS = lf.Find("ValSurname")?.GetComponent<TextMeshProUGUI>();
                    if (valS != null)
                    {
                        valS.color = StyleDThemeUtility.ColBone;
                        valS.fontSize = 21f;
                        valS.overflowMode = TextOverflowModes.Overflow;
                    }

                    TextMeshProUGUI lblG = lf.Find("LblGiven")?.GetComponent<TextMeshProUGUI>();
                    if (lblG != null)
                    {
                        lblG.text = "02 - GIVEN FIRST NAME";
                        lblG.color = StyleDThemeUtility.ColMuted;
                        SundayTypography.ApplyFont(lblG, FontRole.Mono);
                    }

                    TextMeshProUGUI valG = lf.Find("ValGiven")?.GetComponent<TextMeshProUGUI>();
                    if (valG != null)
                    {
                        valG.color = StyleDThemeUtility.ColAmber;
                        valG.fontSize = 19f;
                        valG.overflowMode = TextOverflowModes.Overflow;
                    }

                    Transform div = lf.Find("FieldDivider");
                    if (div != null && div.GetComponent<Image>() is Image divImg)
                        divImg.color = new Color(0.22f, 0.36f, 0.30f, 0.65f);
                }

                Transform mrz = idCard.Find("MRZStrip");
                if (mrz is RectTransform mrzRt)
                {
                    mrzRt.anchorMin = Vector2.zero;
                    mrzRt.anchorMax = new Vector2(1f, 0f);
                    mrzRt.pivot = new Vector2(0.5f, 0f);
                    mrzRt.sizeDelta = new Vector2(-48f, 76f);
                    mrzRt.anchoredPosition = new Vector2(0f, 22f);
                    StyleDThemeUtility.ApplySliced(mrz.GetComponent<Image>(), StyleDThemeUtility.CcMrzPaperStrip, 1.45f, Color.white, disableOutline: true);

                    Transform mrzLine = mrz.Find("TopLine");
                    if (mrzLine != null) mrzLine.gameObject.SetActive(false);

                    TextMeshProUGUI bars = mrz.Find("BarcodeBars")?.GetComponent<TextMeshProUGUI>();
                    if (bars != null)
                    {
                        bars.color = new Color(0.09f, 0.10f, 0.12f, 0.92f);
                        bars.fontSize = 10.5f;
                        bars.textWrappingMode = TextWrappingModes.NoWrap;
                        bars.overflowMode = TextOverflowModes.Truncate;
                        bars.rectTransform.offsetMin = new Vector2(22f, -28f);
                        bars.rectTransform.offsetMax = new Vector2(-22f, -7f);
                    }
                    TextMeshProUGUI mrzCode = mrz.Find("ValMrzCode")?.GetComponent<TextMeshProUGUI>();
                    if (mrzCode != null)
                    {
                        mrzCode.color = new Color(0.11f, 0.13f, 0.15f, 0.96f);
                        mrzCode.fontSize = 10.5f;
                        mrzCode.rectTransform.offsetMin = new Vector2(22f, 8f);
                        mrzCode.rectTransform.offsetMax = new Vector2(-22f, -30f);
                    }
                }
            }

            // Left Column: Mirror Profile Tray
            Transform mirror = leftCol?.Find("MirrorProfileBox");
            if (mirror is RectTransform mbRt)
            {
                mbRt.sizeDelta = new Vector2(0f, 304f);
                mbRt.anchoredPosition = new Vector2(0f, 0f);
                StyleDThemeUtility.ApplySliced(mirror.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.55f, Color.white, disableOutline: true);
                StyleDModuleTrayHeader(mirror, "MIRROR REFLECTION & CITIZEN PROFILE");

                Transform barTr = mirror.Find("StyleD_MirrorAccentBar");
                GameObject barObj = barTr != null ? barTr.gameObject : new GameObject("StyleD_MirrorAccentBar", typeof(RectTransform), typeof(Image));
                if (barTr == null) barObj.transform.SetParent(mirror, false);
                RectTransform brt = barObj.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = new Vector2(0f, 1f);
                brt.offsetMin = new Vector2(20f, 114f);
                brt.offsetMax = new Vector2(24f, -38f);
                Image bImg = barObj.GetComponent<Image>();
                bImg.color = StyleDThemeUtility.ColRust;
                bImg.raycastTarget = false;

                TextMeshProUGUI qTmp = mirror.Find("ValMirrorQuote")?.GetComponent<TextMeshProUGUI>();
                if (qTmp != null)
                {
                    qTmp.color = new Color(0.87f, 0.85f, 0.79f, 1f);
                    qTmp.fontSize = 13.5f;
                    qTmp.rectTransform.offsetMin = new Vector2(34f, 112f);
                    qTmp.rectTransform.offsetMax = new Vector2(-20f, -38f);
                    SundayTypography.ApplyFont(qTmp, FontRole.Narrative);
                }

                Transform pill = mirror.Find("ActiveRecordPill");
                if (pill is RectTransform pillRt)
                {
                    pillRt.anchoredPosition = new Vector2(0f, 68f);
                    pillRt.sizeDelta = new Vector2(-36f, 34f);
                    StyleDThemeUtility.ApplySliced(pill.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.6f, Color.white, disableOutline: true);
                }

                Button rNameBtn = mirror.Find("Btn_RandomName")?.GetComponent<Button>();
                if (rNameBtn != null)
                {
                    if (rNameBtn.transform is RectTransform rnRt)
                    {
                        rnRt.anchoredPosition = new Vector2(18f, 16f);
                        rnRt.sizeDelta = new Vector2(240f, 42f);
                    }
                    StyleDThemeUtility.ApplyChipButton(rNameBtn, new Color(0.28f, 0.31f, 0.36f, 1f), new Color(0.38f, 0.42f, 0.48f, 1f), 1.55f, StyleDThemeUtility.ColBone);
                }

                Button rAllBtn = mirror.Find("Btn_RandomAll")?.GetComponent<Button>();
                if (rAllBtn != null)
                {
                    if (rAllBtn.transform is RectTransform raRt)
                    {
                        raRt.anchoredPosition = new Vector2(-18f, 16f);
                        raRt.sizeDelta = new Vector2(240f, 42f);
                    }
                    StyleDThemeUtility.ApplyChipButton(rAllBtn, new Color(0.56f, 0.26f, 0.14f, 1f), new Color(0.72f, 0.34f, 0.18f, 1f), 1.55f, new Color(0.96f, 0.94f, 0.89f, 1f));
                }
            }

            // Right Column: 3 Module Trays
            Transform modGender = rightCol?.Find("Module01_Gender");
            if (modGender is RectTransform mgRt)
            {
                mgRt.anchoredPosition = new Vector2(0f, 0f);
                mgRt.sizeDelta = new Vector2(0f, 156f);
                StyleDThemeUtility.ApplySliced(modGender.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.55f, Color.white, disableOutline: true);
                StyleDModuleTrayHeader(modGender, "01 - BIOLOGICAL SEX & GENDER IDENTITY");
                PositionStyleDGenderCard(modGender.Find("Btn_Gender_Male"), 20f);
                PositionStyleDGenderCard(modGender.Find("Btn_Gender_Female"), 317f);
                PositionStyleDGenderCard(modGender.Find("Btn_Gender_NonBinary"), 614f);
                StyleDInlineCustomRow(modGender.Find("CustomGenderRow"), -106f);
            }

            Transform modFirst = rightCol?.Find("Module02_FirstName");
            if (modFirst is RectTransform mfRt)
            {
                mfRt.anchoredPosition = new Vector2(0f, -168f);
                mfRt.sizeDelta = new Vector2(0f, 240f);
                StyleDThemeUtility.ApplySliced(modFirst.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.55f, Color.white, disableOutline: true);
                StyleDModuleTrayHeader(modFirst, "02 - GIVEN NAME (FIRST NAME)");
                StyleDChipGrid(modFirst.Find("FirstNameChipsGrid"));
                StyleDInlineCustomRow(modFirst.Find("CustomFirstNameRow"), -190f);
            }

            Transform modLast = rightCol?.Find("Module03_LastName");
            if (modLast is RectTransform mlRt)
            {
                mlRt.anchoredPosition = new Vector2(0f, -420f);
                mlRt.sizeDelta = new Vector2(0f, 240f);
                StyleDThemeUtility.ApplySliced(modLast.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.55f, Color.white, disableOutline: true);
                StyleDModuleTrayHeader(modLast, "03 - FAMILY SURNAME (LAST NAME)");
                StyleDChipGrid(modLast.Find("LastNameChipsGrid"));
                StyleDInlineCustomRow(modLast.Find("CustomLastNameRow"), -188f);
            }

            // Bottom Footer Bar
            Transform footer = frame.Find("IdentityFooterBar");
            if (footer is RectTransform footRt)
            {
                footRt.anchorMin = new Vector2(0f, 0f);
                footRt.anchorMax = new Vector2(1f, 0f);
                footRt.pivot = new Vector2(0.5f, 0f);
                footRt.offsetMin = new Vector2(26f, 16f);
                footRt.offsetMax = new Vector2(-26f, 70f);
                Image footImg = footer.GetComponent<Image>();
                if (footImg != null) footImg.color = Color.clear;

                Transform fLine = footer.Find("TopDivider");
                if (fLine != null && fLine.GetComponent<Image>() is Image flImg)
                    flImg.color = new Color(0.55f, 0.38f, 0.18f, 0.55f);

                TextMeshProUGUI notice = footer.Find("FooterNotice")?.GetComponent<TextMeshProUGUI>();
                if (notice != null)
                {
                    notice.text = "<color=#E0A43A><b>[!] OFFICIAL NOTICE:</b></color> <color=#9A9488>IDENTITY RECORD ABOVE WILL BE SEALED INTO YOUR FINAL FEDERAL BIO-EMERGENCY DOSSIER.</color>";
                    notice.rectTransform.offsetMin = new Vector2(12f, 0f);
                }

                Button procBtn = footer.Find("Btn_ConfirmIdentityProceed")?.GetComponent<Button>();
                if (procBtn != null)
                {
                    if (procBtn.transform is RectTransform prRt)
                    {
                        prRt.anchoredPosition = new Vector2(-12f, 0f);
                        prRt.sizeDelta = new Vector2(400f, 44f);
                    }
                    StyleDThemeUtility.ApplyChipButton(procBtn, new Color(0.68f, 0.24f, 0.15f, 1f), new Color(0.84f, 0.31f, 0.19f, 1f), 1.45f, new Color(0.96f, 0.94f, 0.89f, 1f));
                }
            }
        }

        private void EnsureStyleDStepTabsBar(Transform frame)
        {
            Transform barTr = frame.Find("StyleD_StepTabsBar");
            GameObject barObj;
            if (barTr == null)
            {
                barObj = new GameObject("StyleD_StepTabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                barObj.transform.SetParent(frame, false);
                RectTransform rt = barObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.offsetMin = new Vector2(26f, -104f);
                rt.offsetMax = new Vector2(-26f, -68f);

                HorizontalLayoutGroup hlg = barObj.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 12f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = true;

                string[] labels = { "[01 IDENTITY]", "[02 VOCATION]", "[03 TRAITS & HOBBIES]", "[04 BARGAIN & M.O.R.T.A.L.]" };
                for (int i = 0; i < labels.Length; i++)
                {
                    bool active = i == 0;
                    GameObject tab = new GameObject("StepTab_" + i, typeof(RectTransform), typeof(Image));
                    tab.transform.SetParent(barObj.transform, false);
                    Image img = tab.GetComponent<Image>();
                    img.raycastTarget = false;
                    Color tint = active ? new Color(0.68f, 0.31f, 0.14f, 1f) : new Color(0.20f, 0.22f, 0.26f, 0.92f);
                    StyleDThemeUtility.ApplySliced(img, StyleDThemeUtility.CcChipButton, 1.5f, tint, disableOutline: true);

                    TextMeshProUGUI txt = CreateUIText(tab.transform, "Label", labels[i], 13f, TextAlignmentOptions.Center,
                        active ? new Color(0.96f, 0.94f, 0.89f, 1f) : StyleDThemeUtility.ColMuted,
                        FontRole.Display, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    txt.fontStyle = FontStyles.Bold;
                }
            }
            else
            {
                barTr.gameObject.SetActive(true);
            }
        }

        private static void StyleDModuleTrayHeader(Transform moduleTr, string cleanTitle)
        {
            if (moduleTr == null) return;
            Transform hdr = moduleTr.Find("ModuleHeader");
            if (hdr is RectTransform hRt)
            {
                hRt.sizeDelta = new Vector2(0f, 26f);
                hRt.anchoredPosition = Vector2.zero;
                Image hBg = hdr.GetComponent<Image>();
                if (hBg != null) hBg.color = Color.clear;

                TextMeshProUGUI tTmp = hdr.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (tTmp != null)
                {
                    tTmp.text = $"<b>{cleanTitle}</b>";
                    tTmp.color = StyleDThemeUtility.ColBone;
                    tTmp.fontSize = 11.5f;
                    SundayTypography.ApplyFont(tTmp, FontRole.Display);
                    tTmp.rectTransform.offsetMin = new Vector2(18f, 0f);
                }
            }
        }

        private static void PositionStyleDGenderCard(Transform card, float posX)
        {
            if (card is RectTransform rt)
            {
                rt.anchoredPosition = new Vector2(posX, -36f);
                rt.sizeDelta = new Vector2(282f, 54f);
            }
            if (card != null && card.Find("LeftStrip") is RectTransform stripRt)
            {
                stripRt.anchorMin = new Vector2(0f, 0.18f);
                stripRt.anchorMax = new Vector2(0f, 0.82f);
                stripRt.pivot = new Vector2(0.5f, 0.5f);
                stripRt.anchoredPosition = new Vector2(11.5f, 0f);
                stripRt.sizeDelta = new Vector2(3.5f, 0f);
            }
            if (card != null && card.Find("Title") is RectTransform tRt) tRt.offsetMin = new Vector2(18f, 0f);
            if (card != null && card.Find("Sub") is RectTransform sRt) sRt.offsetMin = new Vector2(18f, 4f);
        }

        private static void StyleDChipGrid(Transform gridTr)
        {
            if (gridTr is RectTransform gRt)
            {
                gRt.anchoredPosition = new Vector2(0f, -36f);
                gRt.sizeDelta = new Vector2(-40f, 134f);
                GridLayoutGroup glg = gridTr.GetComponent<GridLayoutGroup>();
                if (glg != null)
                {
                    glg.cellSize = new Vector2(208f, 38f);
                    glg.spacing = new Vector2(14f, 8f);
                }
            }
        }

        private static void StyleDInlineCustomRow(Transform row, float posY)
        {
            if (row == null) return;
            if (row is RectTransform rRt)
            {
                rRt.anchoredPosition = new Vector2(0f, posY);
                rRt.sizeDelta = new Vector2(-40f, 34f);
            }

            TextMeshProUGUI lbl = row.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (lbl != null) lbl.color = new Color(0.66f, 0.63f, 0.58f, 1f);

            Transform inp = row.Find("InputField");
            if (inp != null)
            {
                StyleDThemeUtility.ApplySliced(inp.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                TextMeshProUGUI val = inp.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();
                if (val != null) val.color = StyleDThemeUtility.ColBone;
                TextMeshProUGUI ph = inp.Find("Text Area/Placeholder")?.GetComponent<TextMeshProUGUI>();
                if (ph != null) ph.color = new Color(0.45f, 0.43f, 0.40f, 0.85f);
            }

            Button actBtn = row.Find("ActionBtn")?.GetComponent<Button>();
            if (actBtn != null)
            {
                StyleDThemeUtility.ApplyChipButton(actBtn, new Color(0.28f, 0.31f, 0.36f, 1f), new Color(0.38f, 0.42f, 0.48f, 1f), 1.6f, StyleDThemeUtility.ColBone);
            }
        }

        private void ApplyStyleDScreenHeaderAndBadge(GameObject panel, string badgeName, string backBtnName)
        {
            if (panel == null) return;
            ApplyStyleDRootBackdrop();

            Transform header = panel.transform.Find("Header");
            if (header != null)
            {
                TextMeshProUGUI tTmp = header.Find("Title")?.GetComponent<TextMeshProUGUI>();
                if (tTmp != null)
                {
                    tTmp.color = StyleDThemeUtility.ColBone;
                    SundayTypography.ApplyFont(tTmp, FontRole.Display);
                }
                TextMeshProUGUI sTmp = header.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
                if (sTmp != null)
                {
                    sTmp.color = StyleDThemeUtility.ColMuted;
                    SundayTypography.ApplyFont(sTmp, FontRole.Narrative);
                }
            }

            if (!string.IsNullOrEmpty(backBtnName))
            {
                Button backBtn = panel.transform.Find(backBtnName)?.GetComponent<Button>();
                if (backBtn != null)
                {
                    StyleDThemeUtility.ApplyChipButton(backBtn, new Color(0.24f, 0.27f, 0.31f, 1f), new Color(0.34f, 0.38f, 0.44f, 1f), 1.55f, StyleDThemeUtility.ColBone);
                }
            }

            if (!string.IsNullOrEmpty(badgeName))
            {
                Transform badge = panel.transform.Find(badgeName);
                if (badge != null)
                {
                    StyleDThemeUtility.ApplySliced(badge.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                }
            }
        }

        private void ApplyStyleDJobPanelTheme()
        {
            if (jobSelectionPanel == null) return;
            ApplyStyleDScreenHeaderAndBadge(jobSelectionPanel, "JobStepBadge", "Btn_BackToIdentity");

            TextMeshProUGUI tTmp = jobSelectionPanel.transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
            if (tTmp != null)
                tTmp.text = "PRE-OUTBREAK VOCATION <size=75%><color=#E0A43A>[SERVICE RECORD]</color></size>";

            Transform legend = jobSelectionPanel.transform.Find("SPAICLegendBar");
            if (legend != null)
            {
                StyleDThemeUtility.ApplySliced(legend.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                TextMeshProUGUI lTmp = legend.Find("LegendText")?.GetComponent<TextMeshProUGUI>();
                if (lTmp != null && !string.IsNullOrEmpty(lTmp.text))
                    lTmp.text = lTmp.text.Replace("// ", "").Replace("//", "");
            }

            TextMeshProUGUI badgeTmp = jobSelectionPanel.transform.Find("JobStepBadge/StatusText")?.GetComponent<TextMeshProUGUI>();
            if (badgeTmp != null && !string.IsNullOrEmpty(badgeTmp.text))
                badgeTmp.text = badgeTmp.text.Replace("//", "-");
        }

        private void ApplyStyleDAcademicHeaderTheme(GameObject panel)
        {
            ApplyStyleDScreenHeaderAndBadge(panel, null, null);
            if (panel != null)
            {
                Transform container = panel.transform.Find("AcademicContainer");
                if (container is RectTransform cRt)
                {
                    cRt.anchoredPosition = new Vector2(0f, -55f);
                }
            }
        }

        private void ApplyStyleDHobbyPanelTheme()
        {
            ApplyStyleDScreenHeaderAndBadge(hobbySelectionPanel, "HobbyLoadoutBadge", "Btn_HobbyPreviousStep");
            if (hobbySelectionPanel != null && hobbySelectionPanel.transform.Find("HobbyLoadoutBadge") is RectTransform hRt)
            {
                hRt.sizeDelta = new Vector2(460f, 48f);
                hRt.anchoredPosition = new Vector2(-170f, -44f);
            }
            UpdateWeaknessTabVisuals();
        }

        private void ApplyStyleDStudentTrainingTheme()
        {
            if (studentTrainingPanel == null) return;
            ApplyStyleDScreenHeaderAndBadge(studentTrainingPanel, null, null);

            Transform dossier = studentTrainingPanel.transform.Find("DossierBox");
            if (dossier == null) return;
            StyleDThemeUtility.ApplySliced(dossier.GetComponent<Image>(), StyleDThemeUtility.CcConsoleChassis, 2.4f, Color.white, disableOutline: true);

            if (dossier is RectTransform dRt)
            {
                dRt.sizeDelta = new Vector2(1380f, 660f);
                dRt.anchoredPosition = new Vector2(0f, -40f);
            }

            Transform inner = dossier.Find("Inner");
            if (inner == null) return;
            if (inner is RectTransform inRt)
            {
                inRt.offsetMin = new Vector2(34f, 34f);
                inRt.offsetMax = new Vector2(-34f, -34f);
            }
            Image inImg = inner.GetComponent<Image>();
            if (inImg != null) inImg.color = Color.clear;

            Transform topStripe = inner.Find("TopStripe");
            if (topStripe != null) topStripe.gameObject.SetActive(false);

            Transform leftCol = inner.Find("LeftColumn");
            if (leftCol is RectTransform lcRt)
            {
                lcRt.anchorMin = new Vector2(0f, 0f);
                lcRt.anchorMax = new Vector2(0f, 1f);
                lcRt.pivot = new Vector2(0f, 0.5f);
                lcRt.anchoredPosition = new Vector2(6f, 0f);
                lcRt.sizeDelta = new Vector2(640f, 0f);
            }

            Transform rightCol = inner.Find("RightColumn");
            if (rightCol is RectTransform rcRt)
            {
                rcRt.anchorMin = new Vector2(1f, 0f);
                rcRt.anchorMax = new Vector2(1f, 1f);
                rcRt.pivot = new Vector2(1f, 0.5f);
                rcRt.anchoredPosition = new Vector2(-6f, 0f);
                rcRt.sizeDelta = new Vector2(640f, 0f);
            }

            Transform alloc = inner.Find("LeftColumn/AllocatorPanel");
            if (alloc != null)
            {
                if (alloc is RectTransform alRt)
                {
                    alRt.offsetMin = Vector2.zero;
                    alRt.offsetMax = Vector2.zero;
                }
                StyleDThemeUtility.ApplySliced(alloc.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.35f, Color.white, disableOutline: true);

                Transform allocTitle = alloc.Find("AllocTitle");
                if (allocTitle is RectTransform atRt)
                {
                    atRt.anchoredPosition = new Vector2(0f, -4f);
                    atRt.sizeDelta = new Vector2(-36f, 24f);
                }
                if (allocTitle?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI atTmp)
                    atTmp.fontSize = 15f;

                Transform allocSub = alloc.Find("AllocSubtitle");
                if (allocSub is RectTransform asRt)
                {
                    asRt.anchoredPosition = new Vector2(0f, -38f);
                    asRt.sizeDelta = new Vector2(-36f, 20f);
                }

                Transform div = alloc.Find("LeftDivider");
                if (div != null) div.gameObject.SetActive(false);

                Transform pb = alloc.Find("PointsBadge");
                if (pb != null)
                {
                    if (pb is RectTransform pbRt)
                    {
                        pbRt.anchoredPosition = new Vector2(0f, -68f);
                        pbRt.sizeDelta = new Vector2(576f, 36f);
                    }
                    StyleDThemeUtility.ApplySliced(pb.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                }

                Transform rows = alloc.Find("AttributesRows");
                if (rows != null)
                {
                    if (rows is RectTransform rowsRt)
                    {
                        rowsRt.anchoredPosition = new Vector2(0f, -116f);
                        rowsRt.sizeDelta = new Vector2(576f, 270f);
                    }

                    for (int i = 0; i < rows.childCount; i++)
                    {
                        Transform row = rows.GetChild(i);
                        if (row is RectTransform rRt)
                        {
                            rRt.sizeDelta = new Vector2(576f, 48f);
                        }
                        StyleDThemeUtility.ApplySliced(row.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.5f, Color.white, disableOutline: true);

                        Transform pip = row.Find("StatPip");
                        if (pip is RectTransform pipRt)
                        {
                            pipRt.anchorMin = new Vector2(0f, 0.18f);
                            pipRt.anchorMax = new Vector2(0f, 0.82f);
                            pipRt.pivot = new Vector2(0.5f, 0.5f);
                            pipRt.anchoredPosition = new Vector2(10f, 0f);
                            pipRt.sizeDelta = new Vector2(3.5f, 0f);
                        }

                        Transform lblTr = row.Find("NameLabel");
                        if (lblTr != null)
                        {
                            if (lblTr is RectTransform nlRt)
                            {
                                nlRt.anchorMin = new Vector2(0f, 0f);
                                nlRt.anchorMax = new Vector2(0f, 1f);
                                nlRt.pivot = new Vector2(0f, 0.5f);
                                nlRt.anchoredPosition = new Vector2(20f, 0f);
                                nlRt.sizeDelta = new Vector2(178f, 0f);
                            }
                            TextMeshProUGUI nlTmp = lblTr.GetComponent<TextMeshProUGUI>();
                            if (nlTmp != null)
                            {
                                nlTmp.fontSize = 13.5f;
                                if (row.name == "Row_STRENGTH") nlTmp.text = "<b><color=#EF5350>[M] MIGHT</color></b> <size=85%><color=#90A4AE>(STR)</color></size>";
                                else if (row.name == "Row_PERCEPTION") nlTmp.text = "<b><color=#FFA726>[O] OBSERVATION</color></b> <size=85%><color=#90A4AE>(PER)</color></size>";
                                else if (row.name == "Row_AGILITY") nlTmp.text = "<b><color=#66BB6A>[R] REFLEX</color></b> <size=85%><color=#90A4AE>(AGI)</color></size>";
                                else if (row.name == "Row_INTELLIGENCE") nlTmp.text = "<b><color=#42A5F5>[T] TACTICS</color></b> <size=85%><color=#90A4AE>(INT)</color></size>";
                                else if (row.name == "Row_CHARISMA") nlTmp.text = "<b><color=#AB47BC>[L] LEADERSHIP</color></b> <size=85%><color=#90A4AE>(CHA)</color></size>";
                            }
                        }

                        Button minus = row.Find("Btn_Minus")?.GetComponent<Button>();
                        if (minus != null)
                        {
                            if (minus.transform is RectTransform mRt)
                            {
                                mRt.anchorMin = new Vector2(0f, 0.5f);
                                mRt.anchorMax = new Vector2(0f, 0.5f);
                                mRt.pivot = new Vector2(0f, 0.5f);
                                mRt.anchoredPosition = new Vector2(204f, 0f);
                                mRt.sizeDelta = new Vector2(34f, 34f);
                            }
                            StyleDThemeUtility.ApplyChipButton(minus, new Color(0.28f, 0.31f, 0.36f, 1f), new Color(0.40f, 0.44f, 0.50f, 1f), 1.8f, StyleDThemeUtility.ColBone);
                        }

                        Transform valTr = row.Find("ValueDisplay");
                        if (valTr is RectTransform vdRt)
                        {
                            vdRt.anchorMin = new Vector2(0f, 0f);
                            vdRt.anchorMax = new Vector2(0f, 1f);
                            vdRt.pivot = new Vector2(0f, 0.5f);
                            vdRt.anchoredPosition = new Vector2(244f, 0f);
                            vdRt.sizeDelta = new Vector2(278f, 0f);
                            TextMeshProUGUI vTmp = valTr.GetComponent<TextMeshProUGUI>();
                            if (vTmp != null)
                            {
                                vTmp.alignment = TextAlignmentOptions.Center;
                                vTmp.fontSize = 15f;
                            }
                        }

                        Button plus = row.Find("Btn_Plus")?.GetComponent<Button>();
                        if (plus != null)
                        {
                            if (plus.transform is RectTransform pRt)
                            {
                                pRt.anchorMin = new Vector2(1f, 0.5f);
                                pRt.anchorMax = new Vector2(1f, 0.5f);
                                pRt.pivot = new Vector2(1f, 0.5f);
                                pRt.anchoredPosition = new Vector2(-14f, 0f);
                                pRt.sizeDelta = new Vector2(34f, 34f);
                            }
                            StyleDThemeUtility.ApplyChipButton(plus, new Color(0.28f, 0.31f, 0.36f, 1f), new Color(0.40f, 0.44f, 0.50f, 1f), 1.8f, StyleDThemeUtility.ColBone);
                        }
                    }
                }

                if (studentWarningText != null && studentWarningText.transform is RectTransform wtRt)
                {
                    wtRt.anchoredPosition = new Vector2(0f, 26f);
                    wtRt.sizeDelta = new Vector2(560f, 34f);
                    studentWarningText.fontSize = 12f;
                }

                if (studentContinueButton != null)
                {
                    if (studentContinueButton.transform is RectTransform cbRt)
                    {
                        cbRt.anchoredPosition = new Vector2(0f, 26f);
                        cbRt.sizeDelta = new Vector2(400f, 44f);
                    }
                    StyleDThemeUtility.ApplyChipButton(studentContinueButton, new Color(0.68f, 0.31f, 0.14f, 1f), new Color(0.84f, 0.40f, 0.18f, 1f), 1.45f, new Color(0.96f, 0.94f, 0.89f, 1f));
                }
            }

            Transform insp = inner.Find("RightColumn/InspectorPanel");
            if (insp != null)
            {
                if (insp is RectTransform inspRt)
                {
                    inspRt.offsetMin = Vector2.zero;
                    inspRt.offsetMax = Vector2.zero;
                }
                StyleDThemeUtility.ApplySliced(insp.GetComponent<Image>(), StyleDThemeUtility.CcModuleTray, 1.35f, Color.white, disableOutline: true);

                Transform div = insp.Find("Divider");
                if (div != null) div.gameObject.SetActive(false);

                Transform title = insp.Find("InspTitle");
                if (title is RectTransform tRt)
                {
                    tRt.anchoredPosition = new Vector2(0f, -4f);
                    tRt.sizeDelta = new Vector2(-36f, 24f);
                }
                if (title?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI inspTitleTmp)
                    inspTitleTmp.fontSize = 15f;

                Transform quote = insp.Find("InspQuote");
                if (quote is RectTransform qRt)
                {
                    qRt.anchoredPosition = new Vector2(0f, -38f);
                    qRt.sizeDelta = new Vector2(-36f, 20f);
                }
                Transform vBanner = insp.Find("VignetteBanner");
                if (vBanner is RectTransform vbRt)
                {
                    vbRt.anchoredPosition = new Vector2(0f, -68f);
                    vbRt.sizeDelta = new Vector2(-48f, 138f);
                }
                Transform desc = insp.Find("InspDesc");
                if (desc is RectTransform dsRt)
                {
                    dsRt.anchoredPosition = new Vector2(0f, -216f);
                    dsRt.sizeDelta = new Vector2(-48f, 48f);
                }

                Transform impact = insp.Find("ImpactCard");
                if (impact != null)
                {
                    if (impact is RectTransform imRt)
                    {
                        imRt.anchoredPosition = new Vector2(0f, -274f);
                        imRt.sizeDelta = new Vector2(-48f, 240f);
                    }
                    StyleDThemeUtility.ApplySliced(impact.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                    Transform tLine = impact.Find("TopLine");
                    if (tLine != null) tLine.gameObject.SetActive(false);

                    Transform inspImpact = impact.Find("InspImpact");
                    if (inspImpact is RectTransform iiRt)
                    {
                        iiRt.offsetMin = new Vector2(18f, 14f);
                        iiRt.offsetMax = new Vector2(-18f, -14f);
                    }
                }
            }
        }

        private void ApplyStyleDSummaryDossierTheme()
        {
            if (summaryDossierPanel == null) return;
            ApplyStyleDRootBackdrop();

            Transform frame = summaryDossierPanel.transform.Find("FederalDossierFrame");
            if (frame == null) return;
            if (frame is RectTransform frameRt)
            {
                frameRt.sizeDelta = new Vector2(1480f, 800f);
                frameRt.anchoredPosition = Vector2.zero;
            }
            StyleDThemeUtility.ApplySliced(frame.GetComponent<Image>(), StyleDThemeUtility.CcConsoleChassis, 2.4f, Color.white, disableOutline: true);

            Transform hdrBar = frame.Find("HeaderBar");
            if (hdrBar is RectTransform hbRt)
            {
                hbRt.offsetMin = new Vector2(34f, -100f);
                hbRt.offsetMax = new Vector2(-34f, -26f);
                Image hbImg = hdrBar.GetComponent<Image>();
                if (hbImg != null) hbImg.color = Color.clear;

                Transform gDiv = hdrBar.Find("GoldDivider");
                if (gDiv != null && gDiv.GetComponent<Image>() is Image gImg)
                    gImg.color = new Color(0.55f, 0.38f, 0.18f, 0.72f);

                TextMeshProUGUI titleTmp = hdrBar.Find("TitleBlock/Title")?.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null) titleTmp.color = StyleDThemeUtility.ColBone;
                TextMeshProUGUI subTmp = hdrBar.Find("TitleBlock/Subtitle")?.GetComponent<TextMeshProUGUI>();
                if (subTmp != null) subTmp.color = StyleDThemeUtility.ColAmber;
                TextMeshProUGUI caseTmp = hdrBar.Find("MetaBlock/CaseId")?.GetComponent<TextMeshProUGUI>();
                if (caseTmp != null) caseTmp.color = StyleDThemeUtility.ColMuted;
            }

            Transform cols = frame.Find("ColumnsContainer");
            if (cols != null)
            {
                if (cols is RectTransform colsRt)
                {
                    colsRt.offsetMin = new Vector2(34f, 92f);
                    colsRt.offsetMax = new Vector2(-34f, -110f);
                }

                string[] colNames = { "Col1_Identity", "Col2_Aptitudes", "Col3_Manifest" };
                foreach (string cName in colNames)
                {
                    Transform col = cols.Find(cName);
                    if (col == null) continue;
                    StyleDThemeUtility.ApplySliced(col.GetComponent<Image>(), StyleDThemeUtility.CcDossierCard, 1.35f, Color.white, disableOutline: true);

                    VerticalLayoutGroup colVlg = col.GetComponent<VerticalLayoutGroup>();
                    if (colVlg != null)
                    {
                        colVlg.padding = new RectOffset(18, 18, 5, 16);
                        colVlg.spacing = 6f;
                    }

                    Transform secHdr = col.Find("SectionHeader");
                    if (secHdr != null)
                    {
                        if (secHdr is RectTransform shRt)
                            shRt.sizeDelta = new Vector2(shRt.sizeDelta.x, 22f);
                        LayoutElement shLe = secHdr.GetComponent<LayoutElement>() ?? secHdr.gameObject.AddComponent<LayoutElement>();
                        shLe.minHeight = 22f;
                        shLe.preferredHeight = 22f;
                        Image shBg = secHdr.GetComponent<Image>();
                        if (shBg != null) shBg.color = Color.clear;
                        TextMeshProUGUI shTxt = secHdr.Find("Text")?.GetComponent<TextMeshProUGUI>();
                        if (shTxt != null)
                        {
                            shTxt.text = StripSlashesAndPrefix(shTxt.text);
                            shTxt.fontSize = 12f;
                            shTxt.color = StyleDThemeUtility.ColBone;
                            SundayTypography.ApplyFont(shTxt, FontRole.Display);
                        }
                    }

                    Transform adv = col.Find("AdvisoryBox");
                    if (adv != null)
                    {
                        StyleDThemeUtility.ApplySliced(adv.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                        if (adv is RectTransform abRt)
                            abRt.sizeDelta = new Vector2(0f, 156f);
                        LayoutElement abLe = adv.GetComponent<LayoutElement>() ?? adv.gameObject.AddComponent<LayoutElement>();
                        abLe.minHeight = 156f;
                        abLe.preferredHeight = 156f;
                        TextMeshProUGUI abTmp = adv.GetComponentInChildren<TextMeshProUGUI>();
                        if (abTmp != null)
                        {
                            abTmp.fontSize = 11f;
                            abTmp.lineSpacing = 1.8f;
                            abTmp.rectTransform.offsetMin = new Vector2(14f, 12f);
                            abTmp.rectTransform.offsetMax = new Vector2(-14f, -12f);
                        }
                    }

                    Transform edict = col.Find("EdictBox");
                    if (edict != null)
                    {
                        StyleDThemeUtility.ApplySliced(edict.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.55f, Color.white, disableOutline: true);
                        if (edict is RectTransform ebRt)
                            ebRt.sizeDelta = new Vector2(0f, 144f);
                        LayoutElement ebLe = edict.GetComponent<LayoutElement>() ?? edict.gameObject.AddComponent<LayoutElement>();
                        ebLe.minHeight = 144f;
                        ebLe.preferredHeight = 144f;
                        TextMeshProUGUI ebTmp = edict.GetComponentInChildren<TextMeshProUGUI>();
                        if (ebTmp != null)
                        {
                            ebTmp.fontSize = 10.5f;
                            ebTmp.lineSpacing = 1.5f;
                            ebTmp.rectTransform.offsetMin = new Vector2(14f, 12f);
                            ebTmp.rectTransform.offsetMax = new Vector2(-14f, -12f);
                        }
                    }
                }
            }

            Transform footer = frame.Find("FooterBar");
            if (footer is RectTransform ftRt)
            {
                ftRt.offsetMin = new Vector2(34f, 26f);
                ftRt.offsetMax = new Vector2(-34f, 82f);
                Image ftImg = footer.GetComponent<Image>();
                if (ftImg != null) ftImg.color = Color.clear;

                Transform tDiv = footer.Find("TopDivider");
                if (tDiv != null && tDiv.GetComponent<Image>() is Image tdImg)
                    tdImg.color = new Color(0.55f, 0.38f, 0.18f, 0.55f);

                Button startOver = footer.Find("Btn_StartOver")?.GetComponent<Button>() ?? restartCreationButton;
                if (startOver != null)
                    StyleDThemeUtility.ApplyChipButton(startOver, new Color(0.24f, 0.27f, 0.31f, 1f), new Color(0.56f, 0.22f, 0.18f, 1f), 1.5f, StyleDThemeUtility.ColBone);

                Button confirm = footer.Find("Btn_CONFIRM&COMMENCESURVIVAL>>")?.GetComponent<Button>() ?? beginSurvivalButton;
                if (confirm != null)
                    StyleDThemeUtility.ApplyChipButton(confirm, new Color(0.68f, 0.24f, 0.15f, 1f), new Color(0.84f, 0.31f, 0.19f, 1f), 1.45f, new Color(0.96f, 0.94f, 0.89f, 1f));
            }
        }
        #endregion
    }
}
