using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Hobby UI")]
        [SerializeField] private TextMeshProUGUI hobbyHeaderTitle;
        [SerializeField] private Transform hobbyCardsContainer;
        [SerializeField] private GameObject hobbyCardPrefab;

        [Header("Summary Dossier UI")]
        [SerializeField] private TextMeshProUGUI summaryNameText;
        [SerializeField] private TextMeshProUGUI summaryIdentityText;
        [SerializeField] private TextMeshProUGUI summaryVitalsText;
        [SerializeField] private TextMeshProUGUI summaryAttributesText;
        [SerializeField] private TextMeshProUGUI summarySkillsText;
        [SerializeField] private TextMeshProUGUI summaryTraitsText;
        [SerializeField] private TextMeshProUGUI summaryInventoryText;
        [SerializeField] private Button beginSurvivalButton;

        // Runtime creation data
        private CharacterData character = new CharacterData();
        private int hobbyStep = 1; // 1 = Primary, 2 = Secondary, 3 = Weakness
        private List<string> pickedHobbyIds = new List<string>();
        private int studentPointsRemaining = 5;

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
                Transform rows = studentTrainingPanel.transform.Find("AttributesRows");
                if (rows != null)
                {
                    WireTrainingPlus(rows, "Row_STRENGTH", "str");
                    WireTrainingPlus(rows, "Row_INTELLIGENCE", "int");
                    WireTrainingPlus(rows, "Row_AGILITY", "agi");
                    WireTrainingPlus(rows, "Row_CHARISMA", "cha");
                    WireTrainingPlus(rows, "Row_PERCEPTION", "per");
                }

                if (studentContinueButton != null)
                {
                    studentContinueButton.onClick.RemoveAllListeners();
                    studentContinueButton.onClick.AddListener(ConfirmStudentTraining);
                }
            }

            // 6. Student Major Buttons
            if (studentMajorPanel != null)
            {
                Transform grid = studentMajorPanel.transform.Find("MajorGrid");
                if (grid != null)
                {
                    var buttons = grid.GetComponentsInChildren<Button>(true);
                    foreach (var btn in buttons)
                    {
                        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            string txt = tmp.text.ToUpper();
                            btn.onClick.RemoveAllListeners();
                            if (txt.Contains("ROTC")) btn.onClick.AddListener(() => SelectStudentMajor("shooting"));
                            else if (txt.Contains("SPORTS")) btn.onClick.AddListener(() => SelectStudentMajor("melee"));
                            else if (txt.Contains("TRACK")) btn.onClick.AddListener(() => SelectStudentMajor("stealth"));
                            else if (txt.Contains("CS")) btn.onClick.AddListener(() => SelectStudentMajor("tech"));
                            else if (txt.Contains("BIOLOGY")) btn.onClick.AddListener(() => SelectStudentMajor("medicine"));
                        }
                    }
                }
            }

            // 7. Student Minor Buttons
            if (studentMinorPanel != null)
            {
                Transform grid = studentMinorPanel.transform.Find("MinorGrid");
                if (grid != null)
                {
                    var buttons = grid.GetComponentsInChildren<Button>(true);
                    foreach (var btn in buttons)
                    {
                        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            string txt = tmp.text.ToUpper();
                            btn.onClick.RemoveAllListeners();
                            if (txt.Contains("SHOOTING")) btn.onClick.AddListener(() => SelectStudentMinor("shooting"));
                            else if (txt.Contains("MELEE")) btn.onClick.AddListener(() => SelectStudentMinor("melee"));
                            else if (txt.Contains("STEALTH")) btn.onClick.AddListener(() => SelectStudentMinor("stealth"));
                            else if (txt.Contains("TECH")) btn.onClick.AddListener(() => SelectStudentMinor("tech"));
                            else if (txt.Contains("MEDICINE")) btn.onClick.AddListener(() => SelectStudentMinor("medicine"));
                        }
                    }
                }
            }

            // 8. Begin Survival Button
            if (beginSurvivalButton != null)
            {
                beginSurvivalButton.onClick.RemoveAllListeners();
                beginSurvivalButton.onClick.AddListener(OnBeginSurvivalClicked);
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

        private void WireTrainingPlus(Transform rows, string rowName, string stat)
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
            }
        }

        protected override void OnScreenOpened()
        {
            character = new CharacterData();
            hobbyStep = 1;
            pickedHobbyIds.Clear();
            studentPointsRemaining = 5;

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

            if (job.perks != null)
            {
                foreach (var perk in job.perks)
                {
                    character.AddTrait(perk.name, perk.desc);
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
        public void ShowStudentTrainingPanel()
        {
            HideAllPanels();
            if (studentTrainingPanel != null) studentTrainingPanel.SetActive(true);
            UpdateStudentTrainingUI();
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

            PlayClick();
            UpdateStudentTrainingUI();
        }

        private void UpdateStudentTrainingUI()
        {
            if (studentPointsText != null) studentPointsText.text = $"POINTS REMAINING: {studentPointsRemaining}";
            if (studentStrText != null) studentStrText.text = $"STRENGTH: {character.str}";
            if (studentIntText != null) studentIntText.text = $"INTELLIGENCE: {character.intl}";
            if (studentAgiText != null) studentAgiText.text = $"AGILITY: {character.agi}";
            if (studentChaText != null) studentChaText.text = $"CHARISMA: {character.cha}";
            if (studentPerText != null) studentPerText.text = $"PERCEPTION: {character.per}";

            bool finished = studentPointsRemaining == 0;
            if (studentContinueButton != null) studentContinueButton.gameObject.SetActive(finished);
            if (studentWarningText != null) studentWarningText.gameObject.SetActive(!finished);
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
            if (studentMajorPanel != null) studentMajorPanel.SetActive(true);
        }

        public void SelectStudentMajor(string skill)
        {
            switch (skill.ToLower())
            {
                case "shooting": character.shooting = 3; break;
                case "melee": character.melee = 3; break;
                case "stealth": character.stealth = 3; break;
                case "tech": character.tech = 3; break;
                case "medicine": character.medicine = 3; break;
            }
            PlayClick();
            ShowStudentMinorPanel();
        }

        public void ShowStudentMinorPanel()
        {
            HideAllPanels();
            if (studentMinorPanel != null) studentMinorPanel.SetActive(true);
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
                    hobbyHeaderTitle.text = "PRIMARY HOBBY <color=#888888>(+1-2 Stats / Items)</color>";
                else if (hobbyStep == 2)
                    hobbyHeaderTitle.text = "SECONDARY HOBBY <color=#888888>(+1 Stat / Items)</color>";
                else
                    hobbyHeaderTitle.text = "SURVIVOR WEAKNESS <color=#B32D2E>(Critical Drawback)</color>";
            }

            PopulateHobbyCards();
        }

        private void PopulateHobbyCards()
        {
            if (hobbyCardsContainer == null || hobbyCardPrefab == null) return;

            // Clear old cards
            foreach (Transform child in hobbyCardsContainer)
            {
                Destroy(child.gameObject);
            }

            HobbyData[] sourceList = (hobbyStep < 3)
                ? CharacterCreationDefinitions.PositiveHobbies
                : CharacterCreationDefinitions.DestructiveWeaknesses;

            foreach (var hobby in sourceList)
            {
                if (pickedHobbyIds.Contains(hobby.id)) continue;

                GameObject cardObj = Instantiate(hobbyCardPrefab, hobbyCardsContainer);
                HobbyCardView cardView = cardObj.GetComponent<HobbyCardView>();
                if (cardView != null)
                {
                    cardView.Setup(hobby, () => OnHobbySelected(hobby));
                }
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

            if (summaryNameText != null)
                summaryNameText.text = character.FullName.ToUpper();

            if (summaryIdentityText != null)
                summaryIdentityText.text = $"GENDER: {character.gender.ToUpper()}   |   BACKGROUND: {character.job.ToUpper()}";

            if (summaryVitalsText != null)
                summaryVitalsText.text = $"HP: {character.hp}/{character.hpMax}   |   STAMINA: {character.stamina}/{character.staminaMax}   |   HUMANITY: {character.humanity}/100";

            if (summaryAttributesText != null)
                summaryAttributesText.text = $"STR: {character.str}   INT: {character.intl}   AGI: {character.agi}   CHA: {character.cha}   PER: {character.per}";

            if (summarySkillsText != null)
                summarySkillsText.text = $"SHOOTING: {character.shooting}   MELEE: {character.melee}   STEALTH: {character.stealth}\nTECH: {character.tech}   MEDICINE: {character.medicine}   SCAVENGING: {character.scavenging}";

            if (summaryTraitsText != null)
            {
                string traitsList = "";
                foreach (var t in character.traits)
                {
                    string color = t.isWeakness ? "#B32D2E" : "#D35400";
                    traitsList += $"<color={color}><b>[{t.name}]</b></color> {t.description}\n";
                }
                summaryTraitsText.text = string.IsNullOrEmpty(traitsList) ? "None" : traitsList.TrimEnd();
            }

            if (summaryInventoryText != null)
            {
                string itemsList = "";
                foreach (var item in character.inventory)
                {
                    itemsList += $"• {item.itemId} (x{item.quantity})\n";
                }
                summaryInventoryText.text = string.IsNullOrEmpty(itemsList) ? "Empty" : itemsList.TrimEnd();
            }
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

