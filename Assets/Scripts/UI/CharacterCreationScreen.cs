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
    /// Gender -> First Name -> Last Name -> Job Selection -> (Student Training/Major/Minor) -> Primary Hobby -> Secondary Hobby -> Weakness -> Final Dossier
    /// </summary>
    public class CharacterCreationScreen : ScreenBase
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

        [Header("Weakness Thematic Tabs")]
        [SerializeField] private GameObject weaknessTabBar;
        [SerializeField] private Button physicalTabButton;
        [SerializeField] private Button mentalTabButton;
        [SerializeField] private Image physicalTabBg;
        [SerializeField] private Image mentalTabBg;
        [SerializeField] private TextMeshProUGUI physicalTabText;
        [SerializeField] private TextMeshProUGUI mentalTabText;
        [SerializeField] private Outline physicalTabOutline;
        [SerializeField] private Outline mentalTabOutline;

        private int activeWeaknessTab = 0; // 0 = Physical, 1 = Mental

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

        // Runtime creation data
        private CharacterData character = new CharacterData();
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

            InitializeButtons();
            HideAllPanels();
        }

        private void InitializeButtons()
        {
            // 1. Gender Panel Buttons
            if (genderPanel != null)
            {
                var buttons = genderPanel.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    string bName = btn.gameObject.name.ToUpper();
                    if (bName.Contains("MALE") && !bName.Contains("FEMALE"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectGender("Male"));
                    }
                    else if (bName.Contains("FEMALE"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectGender("Female"));
                    }
                    else if (bName.Contains("NON-BINARY") || bName.Contains("NONBINARY"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectGender("Non-Binary"));
                    }
                }

                Transform customRow = genderPanel.transform.Find("CustomGenderRow");
                if (customRow != null)
                {
                    Button confirmBtn = customRow.Find("ConfirmBtn")?.GetComponent<Button>();
                    if (confirmBtn != null)
                    {
                        confirmBtn.onClick.RemoveAllListeners();
                        confirmBtn.onClick.AddListener(ConfirmCustomGender);
                    }
                }
                if (customGenderInput != null)
                {
                    customGenderInput.onSubmit.RemoveAllListeners();
                    customGenderInput.onSubmit.AddListener(_ => ConfirmCustomGender());
                }
            }

            // 2. First Name Buttons
            WireNameButtons(maleNamesContainer, SelectFirstName);
            WireNameButtons(femaleNamesContainer, SelectFirstName);
            WireNameButtons(nonBinaryNamesContainer, SelectFirstName);

            if (firstNamePanel != null)
            {
                Transform customRow = firstNamePanel.transform.Find("CustomFirstNameRow");
                if (customRow != null)
                {
                    Button confirmBtn = customRow.Find("ConfirmBtn")?.GetComponent<Button>();
                    if (confirmBtn != null)
                    {
                        confirmBtn.onClick.RemoveAllListeners();
                        confirmBtn.onClick.AddListener(ConfirmCustomFirstName);
                    }
                }
                if (customFirstNameInput != null)
                {
                    customFirstNameInput.onSubmit.RemoveAllListeners();
                    customFirstNameInput.onSubmit.AddListener(_ => ConfirmCustomFirstName());
                }
            }

            // 3. Last Name Buttons
            if (lastNamePanel != null)
            {
                Transform grid = lastNamePanel.transform.Find("LastNameGrid");
                if (grid != null)
                {
                    WireNameButtons(grid, SelectLastName);
                }

                Transform customRow = lastNamePanel.transform.Find("CustomLastNameRow");
                if (customRow != null)
                {
                    Button confirmBtn = customRow.Find("ConfirmBtn")?.GetComponent<Button>();
                    if (confirmBtn != null)
                    {
                        confirmBtn.onClick.RemoveAllListeners();
                        confirmBtn.onClick.AddListener(ConfirmCustomLastName);
                    }
                }
                if (customLastNameInput != null)
                {
                    customLastNameInput.onSubmit.RemoveAllListeners();
                    customLastNameInput.onSubmit.AddListener(_ => ConfirmCustomLastName());
                }
            }

            // 4. Job Selection Buttons
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
                            Button btn = card.GetComponentInChildren<Button>();
                            if (btn != null)
                            {
                                string jId = job.id;
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => SelectJob(jId));
                            }
                        }
                    }
                }
            }

            // 5. Student Training Buttons
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

            // 6. Student Major Buttons
            WireAcademicCards(studentMajorPanel, isMajor: true);

            // 7. Student Minor Buttons
            WireAcademicCards(studentMinorPanel, isMajor: false);

            // 8. Begin Survival Button
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

            // 9. Weakness Thematic Tab Buttons
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

        private void WireNameButtons(Transform container, System.Action<string> onSelect)
        {
            if (container == null) return;
            var buttons = container.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    string nameText = tmp.text.Trim();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => onSelect(nameText));
                }
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

                // Add PointerEnter & PointerExit triggers for instant hover preview and lock mechanics
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
                    // Lock window expired: seamlessly switch to currently hovered stat if mouse is over one
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
            character = new CharacterData();
            hobbyStep = 1;
            pickedHobbyIds.Clear();
            studentPointsRemaining = 5;
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            activeWeaknessTab = 0;

            // Start creation music & clock tick
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

            // Step 1: Start at Gender Selection
            InitializeButtons();
            ShowGenderPanel();
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
            if (summaryDossierPanel != null) summaryDossierPanel.SetActive(false);
            dossierLayoutEnforced = false;
        }

        #region Step 1: Gender Selection
        public void ShowGenderPanel()
        {
            HideAllPanels();
            if (genderPanel != null) genderPanel.SetActive(true);
        }

        public void SelectGender(string gender)
        {
            Debug.Log($"[CharacterCreationScreen] Gender selected: {gender}");
            character.gender = gender;
            PlayClick();
            ShowFirstNamePanel();
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
            ShowFirstNamePanel();
        }
        #endregion

        #region Step 2: First Name Selection
        public void ShowFirstNamePanel()
        {
            HideAllPanels();
            if (firstNamePanel != null) firstNamePanel.SetActive(true);

            // Filter presets based on gender
            bool isMale = character.gender.Equals("Male", System.StringComparison.OrdinalIgnoreCase);
            bool isFemale = character.gender.Equals("Female", System.StringComparison.OrdinalIgnoreCase);

            if (maleNamesContainer != null) maleNamesContainer.gameObject.SetActive(isMale);
            if (femaleNamesContainer != null) femaleNamesContainer.gameObject.SetActive(isFemale);
            if (nonBinaryNamesContainer != null) nonBinaryNamesContainer.gameObject.SetActive(!isMale && !isFemale);
        }

        public void SelectFirstName(string firstName)
        {
            character.firstName = firstName;
            PlayClick();
            ShowLastNamePanel();
        }

        public void ConfirmCustomFirstName()
        {
            if (customFirstNameInput != null && !string.IsNullOrWhiteSpace(customFirstNameInput.text))
            {
                character.firstName = customFirstNameInput.text.Trim();
                PlayClick();
                ShowLastNamePanel();
            }
        }
        #endregion

        #region Step 3: Last Name Selection
        public void ShowLastNamePanel()
        {
            HideAllPanels();
            if (lastNamePanel != null) lastNamePanel.SetActive(true);
        }

        public void SelectLastName(string lastName)
        {
            character.lastName = lastName;
            PlayClick();
            ShowJobSelectionPanel();
        }

        public void ConfirmCustomLastName()
        {
            if (customLastNameInput != null && !string.IsNullOrWhiteSpace(customLastNameInput.text))
            {
                character.lastName = customLastNameInput.text.Trim();
                PlayClick();
                ShowJobSelectionPanel();
            }
        }
        #endregion

        #region Step 4: Job Selection
        public void ShowJobSelectionPanel()
        {
            HideAllPanels();
            if (jobSelectionPanel != null) jobSelectionPanel.SetActive(true);
        }

        public void SelectJob(string jobId)
        {
            JobData job = System.Array.Find(CharacterCreationDefinitions.Jobs, j => j.id.Equals(jobId, System.StringComparison.OrdinalIgnoreCase));
            if (job == null) return;

            character.job = job.title;
            character.str = job.str;
            character.intl = job.intl;
            character.agi = job.agi;
            character.cha = job.cha;
            character.per = job.per;

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

            PlayClick();

            if (jobId.Equals("student", System.StringComparison.OrdinalIgnoreCase))
            {
                ShowStudentTrainingPanel();
            }
            else
            {
                hobbyStep = 1;
                ShowHobbySelectionPanel();
            }
        }
        #endregion

        #region Step 4b: Student Exclusive Training & Majors
        private string currentInspectedStat = "str";
        private string currentlyHoveredStat = null;
        private float inspectLockTimer = 0f;
        private const float CLICK_LOCK_DURATION = 3.5f;

        public void ShowStudentTrainingPanel()
        {
            HideAllPanels();
            if (studentTrainingPanel != null) studentTrainingPanel.SetActive(true);
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            InspectStudentStat("str", lockSelection: false);
            UpdateStudentTrainingUI();
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
                case "str": if (character.str < 10) { character.str++; studentPointsRemaining--; } break;
                case "int": if (character.intl < 10) { character.intl++; studentPointsRemaining--; } break;
                case "agi": if (character.agi < 10) { character.agi++; studentPointsRemaining--; } break;
                case "cha": if (character.cha < 10) { character.cha++; studentPointsRemaining--; } break;
                case "per": if (character.per < 10) { character.per++; studentPointsRemaining--; } break;
            }

            InspectStudentStat(stat, lockSelection: true);
            PlayClick();
            UpdateStudentTrainingUI();
        }

        public void RemoveStudentPoint(string stat)
        {
            if (studentPointsRemaining >= 5) return;

            switch (stat.ToLower())
            {
                case "str": if (character.str > 2) { character.str--; studentPointsRemaining++; } break;
                case "int": if (character.intl > 2) { character.intl--; studentPointsRemaining++; } break;
                case "agi": if (character.agi > 2) { character.agi--; studentPointsRemaining++; } break;
                case "cha": if (character.cha > 2) { character.cha--; studentPointsRemaining++; } break;
                case "per": if (character.per > 2) { character.per--; studentPointsRemaining++; } break;
            }

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

        private string chosenMajorSkill = "";

        public void SelectStudentMajor(string skill)
        {
            chosenMajorSkill = skill.ToLower();
            string perkName = null;
            string perkDesc = null;
            string perkIcon = null;

            switch (chosenMajorSkill)
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

            PlayClick();
            ShowStudentMinorPanel();
        }

        public void ShowStudentMinorPanel()
        {
            HideAllPanels();
            if (studentMinorPanel != null)
            {
                studentMinorPanel.SetActive(true);
                WireAcademicCards(studentMinorPanel, isMajor: false);

                // Filter out the skill already chosen as Major (matches Twine behavior)
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
            switch (skill.ToLower())
            {
                case "shooting": if (character.shooting < 3) character.shooting = 2; break;
                case "melee": if (character.melee < 3) character.melee = 2; break;
                case "stealth": if (character.stealth < 3) character.stealth = 2; break;
                case "tech": if (character.tech < 3) character.tech = 2; break;
                case "medicine": if (character.medicine < 3) character.medicine = 2; break;
                case "survival": if (character.survival < 3) character.survival = 2; break;
                case "resolve": if (character.resolve < 3) character.resolve = 2; break;
            }
            PlayClick();
            hobbyStep = 1;
            ShowHobbySelectionPanel();
        }
        #endregion

        #region Step 5: Hobbies & Weakness Selection
        public void ShowHobbySelectionPanel()
        {
            HideAllPanels();
            if (hobbySelectionPanel != null) hobbySelectionPanel.SetActive(true);

            if (hobbyHeaderTitle != null)
            {
                if (hobbyStep == 1)
                {
                    hobbyHeaderTitle.text = "PRIMARY HOBBY <color=#888888>(+1-2 Stats / Items)</color>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"How did you spend your weekends before everything fell apart?\"";
                }
                else if (hobbyStep == 2)
                {
                    hobbyHeaderTitle.text = "SECONDARY HOBBY <color=#888888>(+1 Stat / Items)</color>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"A secondary pursuit that kept your instincts sharp.\"";
                }
                else
                {
                    hobbyHeaderTitle.text = "SURVIVOR WEAKNESS <color=#E74C3C>(Critical Drawback)</color>";
                    if (hobbyHeaderSubtitle != null)
                        hobbyHeaderSubtitle.text = "\"Nobody survives untouched. What was your fatal flaw?\"";
                }
            }

            if (weaknessTabBar != null)
            {
                weaknessTabBar.SetActive(hobbyStep == 3);
            }

            if (hobbyStep == 3)
            {
                UpdateWeaknessTabVisuals();
            }

            // Reset scroll position to top
            ScrollRect sr = hobbyCardsContainer != null ? hobbyCardsContainer.GetComponentInParent<ScrollRect>() : null;
            if (sr != null)
            {
                sr.verticalNormalizedPosition = 1f;
            }

            PopulateHobbyCards();
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
            bool isPhysical = activeWeaknessTab == 0;

            if (physicalTabBg != null)
                physicalTabBg.color = isPhysical ? new Color(0.65f, 0.15f, 0.12f, 0.95f) : new Color(0.08f, 0.08f, 0.08f, 0.85f);
            if (mentalTabBg != null)
                mentalTabBg.color = !isPhysical ? new Color(0.65f, 0.15f, 0.12f, 0.95f) : new Color(0.08f, 0.08f, 0.08f, 0.85f);

            if (physicalTabText != null)
            {
                physicalTabText.text = isPhysical ? "<b><color=#FFFFFF>● PHYSICAL FLAWS (10)</color></b>" : "<b><color=#777777>○ PHYSICAL FLAWS (10)</color></b>";
            }
            if (mentalTabText != null)
            {
                mentalTabText.text = !isPhysical ? "<b><color=#FFFFFF>● MENTAL & SOCIAL (10)</color></b>" : "<b><color=#777777>○ MENTAL & SOCIAL (10)</color></b>";
            }

            if (physicalTabOutline != null)
                physicalTabOutline.effectColor = isPhysical ? new Color(0.91f, 0.30f, 0.24f, 0.85f) : new Color(0.20f, 0.20f, 0.20f, 0.5f);
            if (mentalTabOutline != null)
                mentalTabOutline.effectColor = !isPhysical ? new Color(0.91f, 0.30f, 0.24f, 0.85f) : new Color(0.20f, 0.20f, 0.20f, 0.5f);
        }

        private void PopulateHobbyCards()
        {
            if (hobbyCardsContainer == null || hobbyCardPrefab == null) return;

            // Clear old cards
            foreach (Transform child in hobbyCardsContainer)
            {
                Destroy(child.gameObject);
            }

            if (hobbyStep < 3)
            {
                foreach (var hobby in CharacterCreationDefinitions.PositiveHobbies)
                {
                    if (pickedHobbyIds.Contains(hobby.id)) continue;

                    GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                    cardObj.SetActive(true);
                    HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                    if (cardView != null)
                    {
                        cardView.Setup(hobby, () => OnHobbySelected(hobby));
                    }
                }
            }
            else
            {
                // Weakness step: filtered by active tab (10 cards each in a 5x2 grid, 0 scrolling!)
                foreach (var hobby in CharacterCreationDefinitions.DestructiveWeaknesses)
                {
                    if (pickedHobbyIds.Contains(hobby.id)) continue;

                    bool isPhys = PhysicalWeaknessIds.Contains(hobby.id);
                    if (activeWeaknessTab == 0 && !isPhys) continue;
                    if (activeWeaknessTab == 1 && isPhys) continue;

                    GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                    cardObj.SetActive(true);
                    HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                    if (cardView != null)
                    {
                        cardView.Setup(hobby, () => OnHobbySelected(hobby));
                    }
                }
            }

            // Notify responsive adapter to fit or enable scroll
            HobbySelectionScreenAdapter adapter = hobbyCardsContainer.GetComponentInParent<HobbySelectionScreenAdapter>();
            if (adapter != null)
            {
                adapter.UpdateLayout();
            }
        }

        public void OnHobbySelected(HobbyData hobby)
        {
            pickedHobbyIds.Add(hobby.id);
            hobby.applyEffect?.Invoke(character, hobbyStep);
            PlayClick();

            if (hobbyStep == 1)
            {
                hobbyStep = 2;
                ShowHobbySelectionPanel();
            }
            else if (hobbyStep == 2)
            {
                hobbyStep = 3;
                ShowHobbySelectionPanel();
            }
            else
            {
                ShowSummaryDossierPanel();
            }
        }
        #endregion

        #region Step 6: Final Summary Dossier
        public void ShowSummaryDossierPanel()
        {
            HideAllPanels();
            if (summaryDossierPanel != null) summaryDossierPanel.SetActive(true);
            EnforceDossierLayout();
            dossierLayoutEnforced = true;

            if (summaryNameText != null)
                summaryNameText.text = character.FullName.ToUpper();

            if (summaryIdentityText != null)
                summaryIdentityText.text = $"GENDER: <color=#FFE082><b>{character.gender.ToUpper()}</b></color>   |   VOCATION: <color=#FFE082><b>{character.job.ToUpper()}</b></color>";

            if (summaryVitalsText != null)
                summaryVitalsText.text = $"<color=#EF5350><b>HP:</b> {character.hp}/{character.hpMax}</color>   |   <color=#FFA726><b>STAMINA:</b> {character.stamina}/{character.staminaMax}</color>   |   <color=#29B6F6><b>HUMANITY:</b> {character.humanity}/100</color>";

            if (summaryAttributesText != null)
            {
                summaryAttributesText.text = 
                    $"<color=#90A4AE>STR:</color> <mspace=0.6em><color=#FFFFFF><b>{character.str:00}</b></color></mspace>  <pos=26%><color=#FFA726>{FormatStatPips(character.str, 10)}</color>\n" +
                    $"<color=#90A4AE>INT:</color> <mspace=0.6em><color=#FFFFFF><b>{character.intl:00}</b></color></mspace>  <pos=26%><color=#FFA726>{FormatStatPips(character.intl, 10)}</color>\n" +
                    $"<color=#90A4AE>AGI:</color> <mspace=0.6em><color=#FFFFFF><b>{character.agi:00}</b></color></mspace>  <pos=26%><color=#FFA726>{FormatStatPips(character.agi, 10)}</color>\n" +
                    $"<color=#90A4AE>CHA:</color> <mspace=0.6em><color=#FFFFFF><b>{character.cha:00}</b></color></mspace>  <pos=26%><color=#FFA726>{FormatStatPips(character.cha, 10)}</color>\n" +
                    $"<color=#90A4AE>PER:</color> <mspace=0.6em><color=#FFFFFF><b>{character.per:00}</b></color></mspace>  <pos=26%><color=#FFA726>{FormatStatPips(character.per, 10)}</color>";
            }

            if (summarySkillsText != null)
            {
                summarySkillsText.text = 
                    $"<color=#90A4AE>SHOOTING:</color> <pos=22%><mspace=0.6em><color=#FFE082><b>{character.shooting:00}</b></color></mspace><pos=50%><color=#90A4AE>MELEE:</color> <pos=74%><mspace=0.6em><color=#FFE082><b>{character.melee:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>STEALTH:</color> <pos=22%><mspace=0.6em><color=#FFE082><b>{character.stealth:00}</b></color></mspace><pos=50%><color=#90A4AE>TECH:</color> <pos=74%><mspace=0.6em><color=#FFE082><b>{character.tech:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>MEDICINE:</color> <pos=22%><mspace=0.6em><color=#FFE082><b>{character.medicine:00}</b></color></mspace><pos=50%><color=#90A4AE>SCAVENGING:</color> <pos=74%><mspace=0.6em><color=#FFE082><b>{character.scavenging:00}</b></color></mspace>\n" +
                    $"<color=#90A4AE>SURVIVAL:</color> <pos=22%><mspace=0.6em><color=#FFE082><b>{character.survival:00}</b></color></mspace><pos=50%><color=#90A4AE>RESOLVE:</color> <pos=74%><mspace=0.6em><color=#FFE082><b>{character.resolve:00}</b></color></mspace>";
            }

            if (summaryTraitsText != null)
            {
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
            return $"{new string('■', filled)}{new string('□', empty)}";
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
                        vlg.spacing = 5;
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
                            sRect.sizeDelta = new Vector2(185, 185);
                            sRect.anchoredPosition = new Vector2(25, 25);
                        }
                    }
                }

                Transform col2 = cols.Find("Col2_Aptitudes");
                if (col2 != null)
                {
                    Transform advBox = col2.Find("AdvisoryBox");
                    if (advBox != null)
                    {
                        RectTransform abRect = advBox.GetComponent<RectTransform>();
                        if (abRect != null)
                        {
                            abRect.sizeDelta = new Vector2(0, 115);
                        }
                    }
                }

                Transform col3 = cols.Find("Col3_Manifest");
                if (col3 != null)
                {
                    Transform edictBox = col3.Find("EdictBox");
                    if (edictBox != null)
                    {
                        RectTransform ebRect = edictBox.GetComponent<RectTransform>();
                        if (ebRect != null)
                        {
                            ebRect.sizeDelta = new Vector2(0, 105);
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
                        nr.sizeDelta = new Vector2(740, 50);
                        nr.anchoredPosition = new Vector2(24, 0);
                    }
                }

                // Confirm button right
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

                // Reset / Start Over button
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

                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(rBtnObj.transform, false);
                    RectTransform tr = textObj.AddComponent<RectTransform>();
                    tr.anchorMin = Vector2.zero;
                    tr.anchorMax = Vector2.one;
                    tr.offsetMin = Vector2.zero;
                    tr.offsetMax = Vector2.zero;

                    TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.text = "<b><color=#CFD8DC>↺ START OVER</color></b>";
                    tmp.fontSize = 13.5f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.raycastTarget = false;

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
            character = new CharacterData();
            hobbyStep = 1;
            pickedHobbyIds.Clear();
            studentPointsRemaining = 5;
            inspectLockTimer = 0f;
            currentlyHoveredStat = null;
            activeWeaknessTab = 0;
            dossierLayoutEnforced = false;

            if (customFirstNameInput != null) customFirstNameInput.text = "";
            if (customLastNameInput != null) customLastNameInput.text = "";
            if (customGenderInput != null) customGenderInput.text = "";

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

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}

