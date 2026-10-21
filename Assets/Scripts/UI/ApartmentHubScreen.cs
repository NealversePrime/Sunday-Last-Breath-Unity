using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Master Controller for the Apartment Hub (Unit 404 - Prologue).
    /// Combines the 60/40 Top Stage + Bottom VN Dock in the center with
    /// persistent Left & Right Survivor PDA panels (Status, Skills, Traits, Map, Lore,
    /// Active Loadout, Backpack Grid, and Floor Items) filling the widescreen side wings.
    /// </summary>
    public class ApartmentHubScreen : ScreenBase
    {
        [Header("Top HUD (Twine Style)")]
        [SerializeField] private Button pdaMenuButton;
        [SerializeField] private Image hpBarFill;
        [SerializeField] private TextMeshProUGUI hpValueText;
        [SerializeField] private Image staminaBarFill;
        [SerializeField] private TextMeshProUGUI staminaValueText;
        [SerializeField] private Image hungerBarFill;
        [SerializeField] private TextMeshProUGUI hungerValueText;
        [SerializeField] private Image thirstBarFill;
        [SerializeField] private TextMeshProUGUI thirstValueText;
        [SerializeField] private TextMeshProUGUI clockTimeText;
        [SerializeField] private TextMeshProUGUI clockDayText;
        [SerializeField] private Button floorItemsButton;
        [SerializeField] private TextMeshProUGUI floorItemsCountText;

        [Header("3DS Switchable Console (Seam Bezel)")]
        [SerializeField] private Button btnModeNarrative;
        [SerializeField] private Button btnModePDA;
        [SerializeField] private Button btnModeGear;
        [SerializeField] private Image modeIndicatorNarrative;
        [SerializeField] private Image modeIndicatorPDA;
        [SerializeField] private Image modeIndicatorGear;
        [SerializeField] private GameObject dockContainerNarrative;
        [SerializeField] private GameObject dockContainerPDA;
        [SerializeField] private GameObject dockContainerGear;

        [Header("Left Wing: Embedded Survivor PDA")]
        [SerializeField] private Button pdaTabStatusBtn;
        [SerializeField] private Button pdaTabSkillsBtn;
        [SerializeField] private Button pdaTabTraitsBtn;
        [SerializeField] private Button pdaTabMapBtn;
        [SerializeField] private Button pdaTabLoreBtn;
        [SerializeField] private GameObject pdaPanelStatus;
        [SerializeField] private GameObject pdaPanelSkills;
        [SerializeField] private GameObject pdaPanelTraits;
        [SerializeField] private GameObject pdaPanelMap;
        [SerializeField] private GameObject pdaPanelLore;

        [Header("Left PDA - Status Tab")]
        [SerializeField] private TextMeshProUGUI pdaCharNameText;
        [SerializeField] private TextMeshProUGUI pdaCharSubText;
        [SerializeField] private TextMeshProUGUI pdaActiveLogsText;
        [SerializeField] private TextMeshProUGUI pdaLevelText;
        [SerializeField] private TextMeshProUGUI pdaXpText;
        [SerializeField] private Image pdaXpBarFill;
        [SerializeField] private Image pdaHumanityBarFill;
        [SerializeField] private TextMeshProUGUI pdaHumanityValueText;

        [Header("Left PDA - Skills, Traits & Lore Tabs")]
        [SerializeField] private TextMeshProUGUI pdaAttributesText;
        [SerializeField] private TextMeshProUGUI pdaSkillsText;
        [SerializeField] private Transform pdaTraitsListRoot;
        [SerializeField] private TextMeshProUGUI pdaLoreText;
        [SerializeField] private Slider pdaVolumeSlider;
        [SerializeField] private Button pdaSaveSettingsBtn;

        [Header("Right Wing: Active Loadout, Backpack & Floor Items")]
        [SerializeField] private Image equipHeadIcon;
        [SerializeField] private TextMeshProUGUI equipHeadLabel;
        [SerializeField] private Image equipBodyIcon;
        [SerializeField] private TextMeshProUGUI equipBodyLabel;
        [SerializeField] private Image equipWpnIcon;
        [SerializeField] private TextMeshProUGUI equipWpnLabel;
        [SerializeField] private Image equipLegsIcon;
        [SerializeField] private TextMeshProUGUI equipLegsLabel;
        [SerializeField] private Image equipFeetIcon;
        [SerializeField] private TextMeshProUGUI equipFeetLabel;
        [SerializeField] private TextMeshProUGUI pdaBackpackCountText;
        [SerializeField] private Transform pdaBackpackGrid;
        [SerializeField] private Transform pdaFloorItemsGrid;
        [SerializeField] private TextMeshProUGUI pdaFloorEmptyText;

        [Header("Stage Visuals (Top 60%)")]
        [SerializeField] private Image stageBackdropImage;
        [SerializeField] private FrameAnimationPlayer livingRoomAnimPlayer;
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI vitalsHpText;
        [SerializeField] private TextMeshProUGUI vitalsStaminaText;

        [Header("Bottom Dock Header")]
        [SerializeField] private TextMeshProUGUI dockHeaderLocation;
        [SerializeField] private TextMeshProUGUI dockHeaderChannel;

        [Header("Narration Box (Bottom 40% - Awakening Sequence)")]
        [SerializeField] private GameObject awakeningContainer;

        [Header("Beat 1: EAS Warning")]
        [SerializeField] private GameObject beat1Container;
        [SerializeField] private TextMeshProUGUI beat1IntroText;
        [SerializeField] private GameObject easAlertBox;
        [SerializeField] private TextMeshProUGUI easAlertText;

        [Header("Beat 2: Reaction Monologue")]
        [SerializeField] private GameObject beat2Container;
        [SerializeField] private TextMeshProUGUI beat2IntroText;
        [SerializeField] private GameObject monologueBox;
        [SerializeField] private Image monologueAvatar;
        [SerializeField] private TextMeshProUGUI monologueSpeakerText;
        [SerializeField] private TextMeshProUGUI monologueSpeechText;
        [SerializeField] private TextMeshProUGUI beat2FootnoteText;

        [Header("Beat 3: Table Stash")]
        [SerializeField] private GameObject beat3Container;
        [SerializeField] private TextMeshProUGUI beat3NarrativeText;
        [SerializeField] private GameObject tableStashDisplay;
        [SerializeField] private Transform tableStashGrid;
        [SerializeField] private GameObject tableEmptyNotice;

        [Header("Prompts & Click Catchers")]
        [SerializeField] private GameObject continuePrompt;
        [SerializeField] private Button fullScreenClickCatcher;

        [Header("Hub Menu (Bottom 40% - Interactive Menu)")]
        [SerializeField] private GameObject hubMenuContainer;
        [SerializeField] private Button packGearButton;
        [SerializeField] private GameObject packedSuccessBanner;
        [SerializeField] private Button checkPhoneButton;
        [SerializeField] private Button listenRadioButton;
        [SerializeField] private TextMeshProUGUI listenRadioLabel;
        [SerializeField] private TextMeshProUGUI listenRadioSub;
        [SerializeField] private Button hackTerminalButton;
        [SerializeField] private TextMeshProUGUI hackTerminalLabel;
        [SerializeField] private TextMeshProUGUI hackTerminalSub;
        [SerializeField] private Button leaveApartmentButton;

        [Header("Modal Overlays")]
        [SerializeField] private GameObject phoneModal;
        [SerializeField] private Button closePhoneButton;
        [SerializeField] private GameObject radioModal;
        [SerializeField] private Button closeRadioButton;
        [SerializeField] private GameObject hackingModal;
        [SerializeField] private TextMeshProUGUI hackingTerminalText;
        [SerializeField] private GameObject hackingFinishContainer;
        [SerializeField] private Button closeHackingButton;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource atmosphereSource;
        [SerializeField] private AudioSource ebsBroadcastSource;
        [SerializeField] private AudioSource staticSource;
        [SerializeField] private AudioSource radioSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip atmosphereClip;
        [SerializeField] private AudioClip ebsWarningClip;
        [SerializeField] private AudioClip staticClip;
        [SerializeField] private AudioClip policeRadioClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Input Debounce & Typewriter")]
        [SerializeField] private float clickDebounceDelay = 0.35f;
        [SerializeField] private float typewriterSpeed = 35f;

        // Runtime State
        private CharacterData currentPlayer;
        private GameClock gameClock;
        private ObjectiveSystem objectiveSystem;
        private LoreSystem loreSystem;

        private List<InventoryEntry> tableStash = new List<InventoryEntry>();
        private string equippedHead = "";
        private string equippedBody = "";
        private string equippedWpn = "";
        private string equippedLegs = "";
        private string equippedFeet = "";

        private bool hasPacked = false;
        private bool phoneVisited = false;
        private bool radioVisited = false;
        private bool hackingVisited = false;
        private bool introSeen = false;

        private int awakeningStep = 1;
        private float lastClickTime = -999f;
        private bool isTyping = false;
        private Coroutine typeRoutine;
        private Coroutine hackingRoutine;
        private TextMeshProUGUI activeTypewriterTarget;
        private string activeFullText = "";

        // Cached frames for player avatar animation
        private List<Sprite> cachedAvatarFrames;
        private Coroutine avatarAnimRoutine;

        public GameClock Clock => gameClock;
        public ObjectiveSystem Objectives => objectiveSystem;
        public LoreSystem Lore => loreSystem;

        protected override void Awake()
        {
            base.Awake();

            InitializeAudioSources();
            InitializeSystems();
            HookUIButtons();
        }

        private void InitializeAudioSources()
        {
            if (atmosphereSource == null) atmosphereSource = CreateChildAudioSource("AtmosphereSource", true);
            if (ebsBroadcastSource == null) ebsBroadcastSource = CreateChildAudioSource("EBSBroadcastSource", false);
            if (staticSource == null) staticSource = CreateChildAudioSource("StaticSource", true);
            if (radioSource == null) radioSource = CreateChildAudioSource("RadioSource", false);
            if (sfxSource == null) sfxSource = CreateChildAudioSource("SFXSource", false);

            if (staticClip != null && staticClip.name.Contains("radio_static_ebs"))
            {
                staticClip = null;
            }
        }

        private AudioSource CreateChildAudioSource(string name, bool loop)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            return src;
        }

        private void InitializeSystems()
        {
            gameClock = new GameClock(23, 45, 25, "OCT", 2025, "SATURDAY");
            gameClock.OnTimeAdvanced += (c) => UpdateClockUI();

            objectiveSystem = new ObjectiveSystem();
            objectiveSystem.OnObjectiveUpdated += (o) =>
            {
                UpdateObjectiveUI();
                RefreshPDAStatusTab();
            };

            loreSystem = new LoreSystem();
            loreSystem.OnLoreAdded += (l) => RefreshPDALoreTab();
        }

        private void HookUIButtons()
        {
            // 3DS Console Mode Switcher Buttons
            if (btnModeNarrative != null)
            {
                btnModeNarrative.onClick.RemoveAllListeners();
                btnModeNarrative.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.Narrative));
            }
            if (btnModePDA != null)
            {
                btnModePDA.onClick.RemoveAllListeners();
                btnModePDA.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.PDA));
            }
            if (btnModeGear != null)
            {
                btnModeGear.onClick.RemoveAllListeners();
                btnModeGear.onClick.AddListener(() => SwitchConsoleMode(ConsoleMode.Gear));
            }

            if (fullScreenClickCatcher != null)
            {
                fullScreenClickCatcher.onClick.RemoveAllListeners();
                fullScreenClickCatcher.onClick.AddListener(OnDialogueAreaClicked);
            }

            if (continuePrompt != null)
            {
                Button promptBtn = continuePrompt.GetComponent<Button>();
                if (promptBtn == null) promptBtn = continuePrompt.AddComponent<Button>();
                promptBtn.onClick.RemoveAllListeners();
                promptBtn.onClick.AddListener(OnDialogueAreaClicked);
            }

            if (packGearButton != null)
            {
                packGearButton.onClick.RemoveAllListeners();
                packGearButton.onClick.AddListener(OnPackGearClicked);
            }

            if (checkPhoneButton != null)
            {
                checkPhoneButton.onClick.RemoveAllListeners();
                checkPhoneButton.onClick.AddListener(OnCheckPhoneClicked);
            }

            if (listenRadioButton != null)
            {
                listenRadioButton.onClick.RemoveAllListeners();
                listenRadioButton.onClick.AddListener(OnListenRadioClicked);
            }

            if (hackTerminalButton != null)
            {
                hackTerminalButton.onClick.RemoveAllListeners();
                hackTerminalButton.onClick.AddListener(OnHackTerminalClicked);
            }

            if (leaveApartmentButton != null)
            {
                leaveApartmentButton.onClick.RemoveAllListeners();
                leaveApartmentButton.onClick.AddListener(OnLeaveApartmentClicked);
            }

            if (closePhoneButton != null)
            {
                closePhoneButton.onClick.RemoveAllListeners();
                closePhoneButton.onClick.AddListener(OnClosePhoneClicked);
            }

            if (closeRadioButton != null)
            {
                closeRadioButton.onClick.RemoveAllListeners();
                closeRadioButton.onClick.AddListener(OnCloseRadioClicked);
            }

            if (closeHackingButton != null)
            {
                closeHackingButton.onClick.RemoveAllListeners();
                closeHackingButton.onClick.AddListener(OnCloseHackingClicked);
            }

            // Left PDA Tab Buttons
            if (pdaTabStatusBtn != null)
            {
                pdaTabStatusBtn.onClick.RemoveAllListeners();
                pdaTabStatusBtn.onClick.AddListener(() => SwitchPDATab("STATUS"));
            }
            if (pdaTabSkillsBtn != null)
            {
                pdaTabSkillsBtn.onClick.RemoveAllListeners();
                pdaTabSkillsBtn.onClick.AddListener(() => SwitchPDATab("SKILLS"));
            }
            if (pdaTabTraitsBtn != null)
            {
                pdaTabTraitsBtn.onClick.RemoveAllListeners();
                pdaTabTraitsBtn.onClick.AddListener(() => SwitchPDATab("TRAITS"));
            }
            if (pdaTabMapBtn != null)
            {
                pdaTabMapBtn.onClick.RemoveAllListeners();
                pdaTabMapBtn.onClick.AddListener(() => SwitchPDATab("MAP"));
            }
            if (pdaTabLoreBtn != null)
            {
                pdaTabLoreBtn.onClick.RemoveAllListeners();
                pdaTabLoreBtn.onClick.AddListener(() => SwitchPDATab("LORE"));
            }

            if (pdaVolumeSlider != null)
            {
                pdaVolumeSlider.onValueChanged.RemoveAllListeners();
                pdaVolumeSlider.onValueChanged.AddListener((val) =>
                {
                    AudioListener.volume = Mathf.Clamp01(val);
                });
            }

            if (pdaSaveSettingsBtn != null)
            {
                pdaSaveSettingsBtn.onClick.RemoveAllListeners();
                pdaSaveSettingsBtn.onClick.AddListener(PlayClickSFX);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.onClick.RemoveAllListeners();
                floorItemsButton.onClick.AddListener(() =>
                {
                    if (!hasPacked && introSeen)
                    {
                        OnPackGearClicked();
                    }
                    else
                    {
                        PlayClickSFX();
                    }
                });
            }
        }

        public void InitializeWithCharacter(CharacterData character)
        {
            currentPlayer = character;

            // In accordance with Twine design:
            // Items accumulated during creation are dropped on the tablestash prior to packing!
            tableStash.Clear();
            equippedHead = "";
            equippedBody = "";
            equippedWpn = "";
            equippedLegs = "";
            equippedFeet = "";

            if (currentPlayer != null && currentPlayer.inventory != null)
            {
                foreach (var item in currentPlayer.inventory)
                {
                    tableStash.Add(new InventoryEntry(item.itemId, item.quantity));
                }
                currentPlayer.inventory.Clear();
            }

            hasPacked = false;
            introSeen = false;
            awakeningStep = 1;

            UpdateClockUI();
            UpdateVitalsUI();
            UpdateObjectiveUI();
            PopulateTableStashUI();
            RefreshAllPDAPanels();
            SwitchPDATab("STATUS", playSound: false);
            SwitchConsoleMode(ConsoleMode.Narrative, playSound: false);
            EvaluateHubButtonStates();

            // Load player avatar frames
            cachedAvatarFrames = CharacterCreationScreen.LoadPlayerFrames();
        }

        protected override void OnScreenOpened()
        {
            base.OnScreenOpened();

            if (currentPlayer == null)
            {
                // Fallback default Police Officer character if tested directly in Editor
                CharacterData fallback = new CharacterData
                {
                    firstName = "James",
                    lastName = "Smith",
                    gender = "Male",
                    job = "Police Officer",
                    hp = 100,
                    hpMax = 100,
                    stamina = 80,
                    staminaMax = 80,
                    hunger = 100,
                    thirst = 100,
                    humanity = 35,
                    str = 6,
                    per = 8,
                    agi = 6,
                    intl = 5,
                    cha = 3,
                    lck = 4,
                    shooting = 3,
                    melee = 2,
                    tech = 2
                };
                fallback.AddTrait("HUNTER", "Spot ambushes early.");
                fallback.AddTrait("STEADY HANDS", "+15 Accuracy bonus.");
                fallback.AddTrait("PYRO", "Fire weapons deal +50% DMG.", null, true);
                fallback.AddTrait("MECHANIC", "Start cars without keys.");
                fallback.AddItem("Glock 19", 1);
                fallback.AddItem("9mm Ammo", 24);
                fallback.AddItem("Multi-Tool", 1);
                fallback.AddItem("Binoculars", 1);
                fallback.AddItem("Lighter", 1);
                fallback.AddItem("Jerry Can", 1);
                InitializeWithCharacter(fallback);
            }

            // Play background atmosphere drone
            if (atmosphereSource != null && atmosphereClip != null)
            {
                atmosphereSource.clip = atmosphereClip;
                atmosphereSource.volume = 0.5f;
                atmosphereSource.Play();
            }

            if (!introSeen)
            {
                ShowAwakeningStep(1);
            }
            else
            {
                ShowHubMenu();
            }
        }

        protected override void OnScreenClosed()
        {
            base.OnScreenClosed();
            StopAllAudioSources();
        }

        private void StopAllAudioSources()
        {
            if (atmosphereSource != null) atmosphereSource.Stop();
            if (ebsBroadcastSource != null) ebsBroadcastSource.Stop();
            if (staticSource != null) staticSource.Stop();
            if (radioSource != null) radioSource.Stop();
            if (sfxSource != null) sfxSource.Stop();
        }

        public enum ConsoleMode { Narrative, PDA, Gear }
        private ConsoleMode currentConsoleMode = ConsoleMode.Narrative;

        public void SwitchConsoleMode(ConsoleMode mode, bool playSound = true)
        {
            currentConsoleMode = mode;
            if (dockContainerNarrative != null) dockContainerNarrative.SetActive(mode == ConsoleMode.Narrative);
            if (dockContainerPDA != null) dockContainerPDA.SetActive(mode == ConsoleMode.PDA);
            if (dockContainerGear != null) dockContainerGear.SetActive(mode == ConsoleMode.Gear);

            UpdateModeButtonVisuals();

            if (mode == ConsoleMode.PDA) RefreshAllPDAPanels();
            else if (mode == ConsoleMode.Gear) RefreshRightGearPanel();

            if (playSound) PlayClickSFX();
        }

        private void UpdateModeButtonVisuals()
        {
            if (modeIndicatorNarrative != null) modeIndicatorNarrative.gameObject.SetActive(currentConsoleMode == ConsoleMode.Narrative);
            if (modeIndicatorPDA != null) modeIndicatorPDA.gameObject.SetActive(currentConsoleMode == ConsoleMode.PDA);
            if (modeIndicatorGear != null) modeIndicatorGear.gameObject.SetActive(currentConsoleMode == ConsoleMode.Gear);
        }

        private void Update()
        {
            // Hotkeys for 3DS Switchable Console Mode
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                {
                    SwitchConsoleMode(ConsoleMode.PDA);
                }
                else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
                {
                    SwitchConsoleMode(ConsoleMode.Gear);
                }
                else if (Keyboard.current.escapeKey.wasPressedThisFrame && currentConsoleMode != ConsoleMode.Narrative)
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SwitchConsoleMode(ConsoleMode.Narrative);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P))
            {
                SwitchConsoleMode(ConsoleMode.PDA);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.I))
            {
                SwitchConsoleMode(ConsoleMode.Gear);
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && currentConsoleMode != ConsoleMode.Narrative)
            {
                SwitchConsoleMode(ConsoleMode.Narrative);
            }
#endif

            // Debounced multi-input check (Space or Enter to advance awakening dialogue)
            if (!introSeen && awakeningContainer != null && awakeningContainer.activeSelf && currentConsoleMode == ConsoleMode.Narrative)
            {
                bool spacePressed = false;
                bool enterPressed = false;

#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
                    enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                }
#else
                spacePressed = Input.GetKeyDown(KeyCode.Space);
                enterPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif

                if (spacePressed || enterPressed)
                {
                    OnDialogueAreaClicked();
                }
            }
        }

        // Note: Global OnPointerClick removed to prevent background/tab clicks from advancing dialogue.

        private void OnDialogueAreaClicked()
        {
            if (introSeen) return;

            // Debounce double-clicks
            if (Time.unscaledTime - lastClickTime < clickDebounceDelay)
            {
                return;
            }
            lastClickTime = Time.unscaledTime;

            if (isTyping)
            {
                // Instantly complete typewriter
                CompleteTypewriterInstant();
                return;
            }

            PlayClickSFX();

            // Advance to next awakening beat
            awakeningStep++;
            if (awakeningStep > 3)
            {
                introSeen = true;
                ShowHubMenu();
            }
            else
            {
                ShowAwakeningStep(awakeningStep);
            }
        }

        #region Left & Right Embedded Survivor PDA Logic
        private void SwitchPDATab(string tabName, bool playSound = true)
        {
            if (playSound) PlayClickSFX();

            if (pdaPanelStatus != null) pdaPanelStatus.SetActive(tabName == "STATUS");
            if (pdaPanelSkills != null) pdaPanelSkills.SetActive(tabName == "SKILLS");
            if (pdaPanelTraits != null) pdaPanelTraits.SetActive(tabName == "TRAITS");
            if (pdaPanelMap != null) pdaPanelMap.SetActive(tabName == "MAP");
            if (pdaPanelLore != null) pdaPanelLore.SetActive(tabName == "LORE");

            UpdateTabButtonVisual(pdaTabStatusBtn, tabName == "STATUS");
            UpdateTabButtonVisual(pdaTabSkillsBtn, tabName == "SKILLS");
            UpdateTabButtonVisual(pdaTabTraitsBtn, tabName == "TRAITS");
            UpdateTabButtonVisual(pdaTabMapBtn, tabName == "MAP");
            UpdateTabButtonVisual(pdaTabLoreBtn, tabName == "LORE");
        }

        private void UpdateTabButtonVisual(Button btn, bool isActive)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isActive
                    ? new Color(0.18f, 0.20f, 0.23f, 1f)
                    : new Color(0.08f, 0.09f, 0.11f, 0.95f);
            }

            Transform accent = btn.transform.Find("BottomAccent");
            if (accent != null) accent.gameObject.SetActive(isActive);

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.color = isActive
                    ? new Color(0.95f, 0.48f, 0.12f, 1f) // Twine PDA orange #f39c12 / #e67e22
                    : new Color(0.55f, 0.58f, 0.62f, 1f);
            }
        }

        private void RefreshAllPDAPanels()
        {
            RefreshPDAStatusTab();
            RefreshPDASkillsTab();
            RefreshPDATraitsTab();
            RefreshPDALoreTab();
            RefreshRightGearPanel();
        }

        private void RefreshPDAStatusTab()
        {
            if (currentPlayer != null)
            {
                if (pdaCharNameText != null) pdaCharNameText.text = currentPlayer.FullName;
                if (pdaCharSubText != null) pdaCharSubText.text = $"{currentPlayer.gender}  |  {currentPlayer.job}";

                int hum = Mathf.Clamp(currentPlayer.humanity, 0, 100);
                if (pdaHumanityBarFill != null) pdaHumanityBarFill.fillAmount = hum / 100f;
                if (pdaHumanityValueText != null) pdaHumanityValueText.text = $"{hum}/100";
            }

            if (pdaLevelText != null) pdaLevelText.text = "LVL 1";
            if (pdaXpText != null) pdaXpText.text = "0/100 XP";
            if (pdaXpBarFill != null) pdaXpBarFill.fillAmount = 0f;

            if (pdaActiveLogsText != null && objectiveSystem != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var obj in objectiveSystem.Objectives)
                {
                    if (obj.status == ObjectiveStatus.Completed)
                    {
                        sb.AppendLine($"<color=#546e7a>[v] <s>{obj.description}</s></color>");
                    }
                    else if (obj.status == ObjectiveStatus.Active)
                    {
                        sb.AppendLine($"<color=#e67e22>[ ]</color> <color=#e8eaed>{obj.description}</color>");
                    }
                    else
                    {
                        sb.AppendLine($"<color=#e67e22>[ ]</color> <color=#90a4ae>{obj.description}</color>");
                    }
                }
                pdaActiveLogsText.text = sb.ToString().TrimEnd();
            }
        }

        private void RefreshPDASkillsTab()
        {
            if (currentPlayer == null) return;

            if (pdaAttributesText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(FormatStatBarRow("STR", currentPlayer.str, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("PER", currentPlayer.per, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("AGI", currentPlayer.agi, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("INT", currentPlayer.intl, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("CHA", currentPlayer.cha, 10, "#2ecc71"));
                sb.AppendLine(FormatStatBarRow("LCK", currentPlayer.lck, 10, "#2ecc71"));
                pdaAttributesText.text = sb.ToString().TrimEnd();
            }

            if (pdaSkillsText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(FormatStatBarRow("SHOOTING", currentPlayer.shooting, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("MELEE", currentPlayer.melee, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("STEALTH", currentPlayer.stealth, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("TECH", currentPlayer.tech, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("MEDICINE", currentPlayer.medicine, 5, "#f39c12"));
                sb.AppendLine(FormatStatBarRow("SCAVENGE", currentPlayer.scavenging, 5, "#f39c12"));
                pdaSkillsText.text = sb.ToString().TrimEnd();
            }
        }

        private string FormatStatBarRow(string label, int val, int max, string hexColor)
        {
            int clamped = Mathf.Clamp(val, 0, max);
            string filled = new string('■', clamped);
            string empty = new string('■', Mathf.Max(0, max - clamped));
            return $"<b>{label.PadRight(9)}</b> <color={hexColor}>{filled}</color><color=#22262c>{empty}</color>  <color=#ffffff>[ {val} ]</color>";
        }

        private void RefreshPDATraitsTab()
        {
            if (pdaTraitsListRoot == null) return;

            for (int i = pdaTraitsListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(pdaTraitsListRoot.GetChild(i).gameObject);
            }

            if (currentPlayer == null || currentPlayer.traits == null || currentPlayer.traits.Count == 0)
            {
                GameObject emptyObj = new GameObject("EmptyTraits");
                emptyObj.transform.SetParent(pdaTraitsListRoot, false);
                TextMeshProUGUI tmp = emptyObj.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 11.5f;
                tmp.color = new Color(0.55f, 0.58f, 0.62f);
                tmp.text = "No specialized traits or perks recorded.";
                RectTransform rt = emptyObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0f, 36f);
                return;
            }

            foreach (var trait in currentPlayer.traits)
            {
                CreateTraitCardUI(pdaTraitsListRoot, trait);
            }
        }

        private void CreateTraitCardUI(Transform parent, TraitEntry trait)
        {
            GameObject card = new GameObject("Trait_" + trait.name);
            card.transform.SetParent(parent, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 64f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.flexibleWidth = 1f;

            Image bg = card.AddComponent<Image>();
            bg.color = trait.isWeakness
                ? new Color(0.12f, 0.04f, 0.04f, 0.95f)
                : new Color(0.04f, 0.10f, 0.05f, 0.95f);

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = trait.isWeakness
                ? new Color(0.55f, 0.16f, 0.16f, 0.85f)
                : new Color(0.16f, 0.45f, 0.22f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Icon (Left 44x44)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            RectTransform ir = iconObj.AddComponent<RectTransform>();
            ir.anchorMin = new Vector2(0f, 0.5f);
            ir.anchorMax = new Vector2(0f, 0.5f);
            ir.pivot = new Vector2(0f, 0.5f);
            ir.anchoredPosition = new Vector2(10f, 0f);
            ir.sizeDelta = new Vector2(44f, 44f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            Sprite s = trait.icon != null ? trait.icon : ItemDatabase.GetTraitIcon(trait.name);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.2f, 0.25f, 0.2f, 0.6f);
            }

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 12.5f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.color = trait.isWeakness
                ? new Color(0.95f, 0.35f, 0.35f)
                : new Color(0.28f, 0.88f, 0.45f);
            titleTmp.text = trait.name.ToUpperInvariant();
            titleTmp.textWrappingMode = TextWrappingModes.NoWrap;
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 0.5f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(64f, 0f);
            tr.offsetMax = new Vector2(-10f, -4f);

            // Description
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 10.5f;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            descTmp.color = new Color(0.78f, 0.80f, 0.84f);
            descTmp.text = trait.description;
            descTmp.textWrappingMode = TextWrappingModes.Normal;
            RectTransform dr = descObj.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0f, 0f);
            dr.anchorMax = new Vector2(1f, 0.5f);
            dr.offsetMin = new Vector2(64f, 4f);
            dr.offsetMax = new Vector2(-10f, 0f);
        }

        private void RefreshPDALoreTab()
        {
            if (pdaLoreText == null) return;

            if (loreSystem == null || loreSystem.Entries.Count == 0)
            {
                pdaLoreText.text = "<color=#6c7a89><i>No intelligence logs recorded yet.\n\nUse your Phone, Emergency Radio, or Laptop in the apartment to gather outbreak intel.</i></color>";
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var entry in loreSystem.Entries)
            {
                sb.AppendLine($"<color=#f39c12><b>■ {entry.title.ToUpperInvariant()}</b></color> <color=#6c7a89>[{entry.timestamp}]</color>");
                sb.AppendLine($"<color=#d0d6dc>{entry.content}</color>\n");
            }
            pdaLoreText.text = sb.ToString().TrimEnd();
        }

        private void AutoEquipFromInventory()
        {
            if (currentPlayer == null || currentPlayer.inventory == null) return;

            foreach (var entry in currentPlayer.inventory)
            {
                string slot = ItemDatabase.GetEquipSlot(entry.itemId);
                if (slot == "HEAD" && string.IsNullOrEmpty(equippedHead)) equippedHead = entry.itemId;
                else if (slot == "BODY" && string.IsNullOrEmpty(equippedBody)) equippedBody = entry.itemId;
                else if (slot == "WPN" && string.IsNullOrEmpty(equippedWpn)) equippedWpn = entry.itemId;
                else if (slot == "LEGS" && string.IsNullOrEmpty(equippedLegs)) equippedLegs = entry.itemId;
                else if (slot == "FEET" && string.IsNullOrEmpty(equippedFeet)) equippedFeet = entry.itemId;
            }
        }

        private void RefreshRightGearPanel()
        {
            // 1. Update Active Loadout Slots on Body Wireframe
            UpdateEquipSlotVisual(equipHeadIcon, equipHeadLabel, equippedHead, "HEAD");
            UpdateEquipSlotVisual(equipBodyIcon, equipBodyLabel, equippedBody, "BODY");
            UpdateEquipSlotVisual(equipWpnIcon, equipWpnLabel, equippedWpn, "WPN");
            UpdateEquipSlotVisual(equipLegsIcon, equipLegsLabel, equippedLegs, "LEGS");
            UpdateEquipSlotVisual(equipFeetIcon, equipFeetLabel, equippedFeet, "FEET");

            // 2. Update Backpack 12-Slot Grid (Exact Twine replica)
            if (pdaBackpackGrid != null)
            {
                for (int i = pdaBackpackGrid.childCount - 1; i >= 0; i--)
                {
                    Destroy(pdaBackpackGrid.GetChild(i).gameObject);
                }

                List<InventoryEntry> inv = currentPlayer != null && currentPlayer.inventory != null
                    ? currentPlayer.inventory
                    : new List<InventoryEntry>();

                int maxSlots = currentPlayer != null ? Mathf.Max(10, currentPlayer.bagLimit) : 12;
                if (pdaBackpackCountText != null)
                {
                    pdaBackpackCountText.text = $"{inv.Count}/{maxSlots}";
                }

                int totalCells = Mathf.Max(12, maxSlots);
                for (int i = 0; i < totalCells; i++)
                {
                    if (i < inv.Count)
                    {
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, inv[i].quantity, true);
                    }
                    else
                    {
                        CreateInventorySlotCell(pdaBackpackGrid, null, 0, false);
                    }
                }
            }

            // 3. Update Floor / Room Items (Table Stash) in Right Wing
            if (pdaFloorItemsGrid != null)
            {
                for (int i = pdaFloorItemsGrid.childCount - 1; i >= 0; i--)
                {
                    Destroy(pdaFloorItemsGrid.GetChild(i).gameObject);
                }

                if (pdaFloorEmptyText != null)
                {
                    pdaFloorEmptyText.gameObject.SetActive(tableStash.Count == 0);
                }

                foreach (var item in tableStash)
                {
                    CreateInventorySlotCell(pdaFloorItemsGrid, item.itemId, item.quantity, false);
                }
            }
        }

        private void UpdateEquipSlotVisual(Image iconImg, TextMeshProUGUI slotLabel, string equippedItemId, string defaultSlotName)
        {
            if (iconImg == null || slotLabel == null) return;

            if (!string.IsNullOrEmpty(equippedItemId))
            {
                Sprite s = ItemDatabase.GetItemIcon(equippedItemId);
                if (s != null)
                {
                    iconImg.gameObject.SetActive(true);
                    iconImg.sprite = s;
                    iconImg.color = Color.white;
                    slotLabel.text = "";
                    return;
                }
            }

            iconImg.gameObject.SetActive(false);
            slotLabel.text = defaultSlotName;
        }

        private void CreateInventorySlotCell(Transform parent, string itemId, int quantity, bool allowEquipClick)
        {
            GameObject cell = new GameObject(string.IsNullOrEmpty(itemId) ? "EmptySlot" : "Slot_" + itemId);
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(66f, 66f);

            Image bg = cell.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.11f, 0.95f);

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = string.IsNullOrEmpty(itemId)
                ? new Color(0.18f, 0.20f, 0.24f, 0.75f)
                : new Color(0.28f, 0.32f, 0.38f, 0.95f);
            ol.effectDistance = new Vector2(1f, -1f);

            if (string.IsNullOrEmpty(itemId)) return;

            // Item Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cell.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(6f, 14f);
            ir.offsetMax = new Vector2(-6f, -4f);

            Sprite s = ItemDatabase.GetItemIcon(itemId);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.35f, 0.4f, 0.45f, 0.5f);
            }

            // Short Item Name at bottom
            GameObject nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.78f, 0.82f, 0.86f);
            nameTmp.text = itemId.Length > 10 ? itemId.Substring(0, 9) + "." : itemId;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f);
            nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 14f);
            nr.anchoredPosition = new Vector2(0f, 1f);

            // Quantity badge (Twine style: bottom-right dark box with green x24 text)
            if (quantity > 1)
            {
                GameObject qBadge = new GameObject("QtyBadge");
                qBadge.transform.SetParent(cell.transform, false);
                RectTransform qr = qBadge.AddComponent<RectTransform>();
                qr.anchorMin = new Vector2(1f, 0f);
                qr.anchorMax = new Vector2(1f, 0f);
                qr.pivot = new Vector2(1f, 0f);
                qr.anchoredPosition = new Vector2(-2f, 13f);
                qr.sizeDelta = new Vector2(26f, 14f);
                Image qBg = qBadge.AddComponent<Image>();
                qBg.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
                qBg.raycastTarget = false;

                GameObject qtObj = new GameObject("Text");
                qtObj.transform.SetParent(qBadge.transform, false);
                TextMeshProUGUI qTmp = qtObj.AddComponent<TextMeshProUGUI>();
                qTmp.fontSize = 9f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.35f, 0.95f, 0.45f);
                qTmp.text = $"x{quantity}";
                qTmp.raycastTarget = false;
                RectTransform qtr = qtObj.GetComponent<RectTransform>();
                qtr.anchorMin = Vector2.zero;
                qtr.anchorMax = Vector2.one;
                qtr.offsetMin = Vector2.zero;
                qtr.offsetMax = Vector2.zero;
            }

            if (allowEquipClick)
            {
                Button btn = cell.AddComponent<Button>();
                btn.targetGraphic = bg;
                string capturedItem = itemId;
                btn.onClick.AddListener(() =>
                {
                    string slot = ItemDatabase.GetEquipSlot(capturedItem);
                    if (!string.IsNullOrEmpty(slot))
                    {
                        PlayClickSFX();
                        if (slot == "HEAD") equippedHead = capturedItem;
                        else if (slot == "BODY") equippedBody = capturedItem;
                        else if (slot == "WPN") equippedWpn = capturedItem;
                        else if (slot == "LEGS") equippedLegs = capturedItem;
                        else if (slot == "FEET") equippedFeet = capturedItem;
                        RefreshRightGearPanel();
                    }
                });
            }
        }
        #endregion

        #region Awakening Sequence (Beats 1 - 3)
        private void ShowAwakeningStep(int step)
        {
            if (awakeningContainer != null) awakeningContainer.SetActive(true);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(true);

            if (beat1Container != null) beat1Container.SetActive(false);
            if (beat2Container != null) beat2Container.SetActive(false);
            if (beat3Container != null) beat3Container.SetActive(false);

            switch (step)
            {
                case 1:
                    // Beat 1: Civil Danger Warning (EAS Broadcast)
                    if (beat1Container != null) beat1Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 404 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#EF4444>CHANNEL: EMERGENCY BROADCAST SYSTEM</color>";

                    // Play emergency warning TTS / robotic broadcast
                    if (ebsBroadcastSource != null && ebsWarningClip != null)
                    {
                        ebsBroadcastSource.clip = ebsWarningClip;
                        ebsBroadcastSource.volume = 0.85f;
                        ebsBroadcastSource.Play();
                    }

                    StartTypewriter(
                        beat1IntroText,
                        "You turn away from the glass. The silence in your apartment breaks—not with a scream, but with an agonizing, electronic tone pulsing from the television."
                    );
                    break;

                case 2:
                    // Beat 2: The Reaction
                    if (beat2Container != null) beat2Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 404 // INTERNAL MONOLOGUE ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#60A5FA>CHANNEL: NEURAL APPRAISAL</color>";

                    // Stop EAS broadcast, start low TV static
                    if (ebsBroadcastSource != null) ebsBroadcastSource.Stop();
                    if (staticSource != null)
                    {
                        if (staticClip != null && !staticClip.name.Contains("radio_static_ebs"))
                        {
                            staticSource.clip = staticClip;
                            staticSource.volume = 0.2f;
                            staticSource.Play();
                        }
                        else
                        {
                            staticSource.Stop();
                        }
                    }

                    if (monologueBox != null)
                    {
                        monologueBox.SetActive(true);
                        StartAvatarAnimation();
                        if (monologueSpeakerText != null)
                        {
                            monologueSpeakerText.text = currentPlayer != null ? $"YOU // {currentPlayer.FullName.ToUpper()}" : "YOU";
                        }
                        if (monologueSpeechText != null)
                        {
                            monologueSpeechText.text = GetPlayerMonologueText();
                        }
                    }

                    if (beat2IntroText != null)
                    {
                        beat2IntroText.text = "The robotic voice cuts out. Cold static returns to the TV.";
                    }

                    StartTypewriter(
                        beat2FootnoteText,
                        "Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door."
                    );
                    break;

                case 3:
                    // Beat 3: Table Stash
                    if (staticSource != null) staticSource.Stop();
                    if (beat3Container != null) beat3Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 404 // LIVING ROOM ]</color>";
                    if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#F59E0B>CHANNEL: EQUIPMENT INSPECTION</color>";

                    if (tableStashDisplay != null)
                    {
                        tableStashDisplay.SetActive(true);
                        PopulateTableStashUI();
                    }

                    StartTypewriter(
                        beat3NarrativeText,
                        "You are standing in the middle of your living room in your pajamas. You feel naked. Vulnerable.\n\n" +
                        "You glance around the dim room, illuminated only by the harsh red glow of the TV. Your everyday carry sits on the cramped desk. But your heavier equipment rests by the front door, exactly where you dropped it after yesterday's shift.\n\n" +
                        "It's not a tactical survival kit. But right now, the tools of your trade are the only thing standing between you and whatever violent outbreak is tearing the city apart."
                    );
                    break;
            }
        }

        private string GetPlayerMonologueText()
        {
            if (currentPlayer == null) return "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"";

            if (currentPlayer.HasTrait("PARANOID"))
            {
                return "\"'Shelter in place.' Right. Wait in a fourth-floor box with a single exit while the military shoots anything that moves. That's not a shelter. That's a coffin.\"";
            }
            if (string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase))
            {
                return "\"'Lethal force authorized...' The Guard doesn't deploy for a standard riot. If they establish a quarantine cordon around this block, I'll be trapped here.\"";
            }
            if (string.Equals(currentPlayer.job, "Doctor", StringComparison.OrdinalIgnoreCase) || currentPlayer.HasTrait("FIELD MEDIC"))
            {
                return "\"'Biological hazard. Extreme violence.' In an apartment complex with shared ventilation and paper-thin walls... staying here is suicide.\"";
            }

            return "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"";
        }

        private void StartTypewriter(TextMeshProUGUI target, string fullText)
        {
            activeTypewriterTarget = target;
            activeFullText = fullText;

            if (typeRoutine != null) StopCoroutine(typeRoutine);
            typeRoutine = StartCoroutine(TypewriterRoutine(target, fullText));
        }

        private IEnumerator TypewriterRoutine(TextMeshProUGUI target, string text)
        {
            isTyping = true;
            if (target != null) target.text = "";

            float delay = 1f / Mathf.Max(1f, typewriterSpeed);
            for (int i = 0; i <= text.Length; i++)
            {
                if (target != null)
                {
                    target.text = text.Substring(0, i);
                }
                yield return new WaitForSecondsRealtime(delay);
            }

            isTyping = false;
        }

        private void CompleteTypewriterInstant()
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            if (activeTypewriterTarget != null) activeTypewriterTarget.text = activeFullText;
            isTyping = false;
        }

        private void StartAvatarAnimation()
        {
            if (avatarAnimRoutine != null) StopCoroutine(avatarAnimRoutine);
            if (cachedAvatarFrames != null && cachedAvatarFrames.Count > 0 && monologueAvatar != null)
            {
                avatarAnimRoutine = StartCoroutine(AvatarAnimRoutine());
            }
        }

        private IEnumerator AvatarAnimRoutine()
        {
            int idx = 0;
            float frameDelay = 1f / 8f;
            while (monologueBox != null && monologueBox.activeSelf)
            {
                if (cachedAvatarFrames.Count > 0 && monologueAvatar != null)
                {
                    monologueAvatar.sprite = cachedAvatarFrames[idx];
                    idx = (idx + 1) % cachedAvatarFrames.Count;
                }
                yield return new WaitForSecondsRealtime(frameDelay);
            }
        }
        #endregion

        #region Hub Menu Mode
        private void ShowHubMenu()
        {
            if (awakeningContainer != null) awakeningContainer.SetActive(false);
            if (hubMenuContainer != null) hubMenuContainer.SetActive(true);
            if (continuePrompt != null) continuePrompt.SetActive(false);

            if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 404 // ACTIONS ]</color>";
            if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#10B981>STATUS: READY FOR TRANSIT</color>";

            EvaluateHubButtonStates();
        }

        private void EvaluateHubButtonStates()
        {
            // 1. Pack Gear Button
            if (packGearButton != null && packedSuccessBanner != null)
            {
                packGearButton.gameObject.SetActive(!hasPacked);
                packedSuccessBanner.SetActive(hasPacked);
            }

            // 2. Check News Feed / Phone
            if (checkPhoneButton != null)
            {
                checkPhoneButton.gameObject.SetActive(!phoneVisited);
            }

            // 3. Listen to Emergency Band
            if (listenRadioButton != null)
            {
                if (radioVisited)
                {
                    listenRadioButton.gameObject.SetActive(false);
                }
                else
                {
                    listenRadioButton.gameObject.SetActive(true);
                    bool isPolice = currentPlayer != null && string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
                    bool hasTech1 = currentPlayer != null && currentPlayer.tech >= 1 && currentPlayer.Tactics >= 4;

                    if (isPolice)
                    {
                        listenRadioButton.interactable = true;
                        if (listenRadioLabel != null) listenRadioLabel.text = "LISTEN TO EMERGENCY BAND [POLICE]";
                        if (listenRadioSub != null) listenRadioSub.text = "<color=#f39c12>[JOB: POLICE]</color> 3 Minutes | Monitor encrypted dispatch channels. Find out where lines are breaking.";
                    }
                    else if (hasTech1)
                    {
                        listenRadioButton.interactable = true;
                        if (listenRadioLabel != null) listenRadioLabel.text = "HACK LOCAL POLICE CHATTER [TECH 1]";
                        if (listenRadioSub != null) listenRadioSub.text = "<color=#f39c12>[TECH 1 / INT 4]</color> 5 Minutes | Intercept dispatch frequency and bypass local scramblers.";
                    }
                    else
                    {
                        listenRadioButton.interactable = false;
                        if (listenRadioLabel != null) listenRadioLabel.text = "<s>ACCESS EMERGENCY BAND</s>";
                        if (listenRadioSub != null) listenRadioSub.text = "<color=#777777>[REQUIRES POLICE OR TECH 1 & INT 4] You lack the hardware or expertise to intercept encrypted radio traffic.</color>";
                    }
                }
            }

            // 4. Bypass Gov Firewall
            if (hackTerminalButton != null)
            {
                if (hackingVisited)
                {
                    hackTerminalButton.gameObject.SetActive(false);
                }
                else
                {
                    hackTerminalButton.gameObject.SetActive(true);
                    bool hasTech3 = currentPlayer != null && currentPlayer.tech >= 3 && currentPlayer.Tactics >= 6;

                    if (hasTech3)
                    {
                        hackTerminalButton.interactable = true;
                        if (hackTerminalLabel != null) hackTerminalLabel.text = "BYPASS GOV FIREWALL [TECH 3]";
                        if (hackTerminalSub != null) hackTerminalSub.text = "<color=#f39c12>[TECH 3 / INT 6]</color> 8 Minutes | Breach military-grade encryption to extract classified directives.";
                    }
                    else
                    {
                        hackTerminalButton.interactable = false;
                        if (hackTerminalLabel != null) hackTerminalLabel.text = "<s>BYPASS GOV FIREWALL</s>";
                        if (hackTerminalSub != null) hackTerminalSub.text = "<color=#777777>[REQUIRES TECH 3 & INT 6] You lack the high-level expertise to breach military node encryption.</color>";
                    }
                }
            }

            // 5. Leave Apartment (unlocked only after packing!)
            if (leaveApartmentButton != null)
            {
                leaveApartmentButton.gameObject.SetActive(hasPacked);
            }
        }

        private void OnPackGearClicked()
        {
            PlayClickSFX();

            // Advance time +4 minutes
            gameClock.AddTime(4);

            // Move table stash into player inventory
            if (currentPlayer != null)
            {
                foreach (var item in tableStash)
                {
                    currentPlayer.AddItem(item.itemId, item.quantity);
                }
                tableStash.Clear();

                // Add apartment kitchen loot (as in Twine Prologue_Menu.twee)
                currentPlayer.AddItem("Stale Coffee", 1);
                currentPlayer.AddItem("Kitchen Knife", 1);
                currentPlayer.AddItem("T-Shirt", 1);
            }

            hasPacked = true;
            AutoEquipFromInventory();

            // Objectives
            objectiveSystem.CompleteObjective("pack_gear");
            objectiveSystem.ActivateObjective("leave_apt");

            EvaluateHubButtonStates();
            PopulateTableStashUI();
            RefreshAllPDAPanels();
        }

        private void OnCheckPhoneClicked()
        {
            PlayClickSFX();

            // Advance time +1 minute
            gameClock.AddTime(1);
            phoneVisited = true;

            if (phoneModal != null)
            {
                phoneModal.SetActive(true);
            }
        }

        private void OnClosePhoneClicked()
        {
            PlayClickSFX();

            if (phoneModal != null)
            {
                phoneModal.SetActive(false);
            }

            // Record Lore Entry
            loreSystem.AddLore(
                "X-Feed Timeline (Day 0)",
                "CDC reported an unprecedented neuro-aggressive pathogen causing extreme psychosis. Violence escalating. Videos show infected individuals ignoring fatal chest wounds. The military barricaded the bridges before the cellular network collapsed.",
                gameClock.GetTimeString()
            );

            EvaluateHubButtonStates();
        }

        private void OnListenRadioClicked()
        {
            PlayClickSFX();

            bool isPolice = currentPlayer != null && string.Equals(currentPlayer.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
            gameClock.AddTime(isPolice ? 3 : 5);
            radioVisited = true;

            if (radioModal != null)
            {
                radioModal.SetActive(true);
            }

            // Play police radio squelch and audio
            if (radioSource != null && policeRadioClip != null)
            {
                radioSource.clip = policeRadioClip;
                radioSource.volume = 0.9f;
                radioSource.Play();
            }
        }

        private void OnCloseRadioClicked()
        {
            PlayClickSFX();

            if (radioSource != null) radioSource.Stop();
            if (radioModal != null) radioModal.SetActive(false);

            // Record Lore Entry
            loreSystem.AddLore(
                "Radio Intercept (Day 0)",
                "Police dispatch authorized unrestricted lethal force. Standard body shots are ineffective against the infected. Neutralization requires destroying the brain. All physical contact must be avoided.",
                gameClock.GetTimeString()
            );

            EvaluateHubButtonStates();
        }

        private void OnHackTerminalClicked()
        {
            PlayClickSFX();

            gameClock.AddTime(8);
            hackingVisited = true;

            if (hackingModal != null)
            {
                hackingModal.SetActive(true);
            }

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(false);
            }

            // Start matrix green phosphor scramble sequence
            if (hackingRoutine != null) StopCoroutine(hackingRoutine);
            hackingRoutine = StartCoroutine(HackingTerminalRoutine());
        }

        private IEnumerator HackingTerminalRoutine()
        {
            string[] hackingTextLines = new string[]
            {
                "> BYPASSING 256-BIT AES ENCRYPTION...",
                "> ACCESSING SECURE MILITARY NODE...",
                "> DOWNLOADING CLASSIFIED DIRECTIVE: OPR_WILDFIRE",
                "------------------------------------------------",
                "<color=#ff5555>THREAT LEVEL : CATEGORY 5 (EXTINCTION)</color>",
                "PATHOGEN     : NEURO-AGGRESSIVE MUTAGEN (UNKNOWN ORIGIN)",
                "<color=#ff5555>STATUS       : QUARANTINE FAILED. INFECTION RATE 87%.</color>",
                "<color=#ff3333><b>DIRECTIVE    : INITIATE SECTOR PURGE (FIREBOMBING) AT 0600 HOURS.</b></color>",
                "------------------------------------------------",
                "> DOWNLOADING ATTACHMENT: EVAC_PROTOCOLS.DAT",
                "> EXTRACTION POINT 'ECHO' COORDINATES SECURED.",
                "<color=#ffd700>> GENERATING TIER-1 MILITARY CLEARANCE PASSKEY...</color>",
                "------------------------------------------------",
                "<color=#ff5555>[!] WARNING: TRACE DETECTED. SEVERING CONNECTION.</color>"
            };

            if (hackingTerminalText != null) hackingTerminalText.text = "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hackingTextLines.Length; i++)
            {
                sb.AppendLine(hackingTextLines[i]);
                if (hackingTerminalText != null)
                {
                    hackingTerminalText.text = sb.ToString();
                }
                yield return new WaitForSecondsRealtime(0.25f);
            }

            yield return new WaitForSecondsRealtime(0.4f);

            if (hackingFinishContainer != null)
            {
                hackingFinishContainer.SetActive(true);
            }
        }

        private void OnCloseHackingClicked()
        {
            PlayClickSFX();

            if (hackingModal != null)
            {
                hackingModal.SetActive(false);
            }

            // Award Tier-1 Military Clearance Passkey
            if (currentPlayer != null)
            {
                currentPlayer.AddItem("Tier-1 Military Clearance Passkey", 1);
            }

            loreSystem.AddLore(
                "DoD Intercept: Operation WILDFIRE",
                "The outbreak is uncontained. The military considers the city a lost cause and has ordered a 'Sector Purge' (firebombing) at 0600 hours. I managed to download a Tier-1 Military Clearance Passkey before severing the connection.",
                gameClock.GetTimeString()
            );

            EvaluateHubButtonStates();
            RefreshAllPDAPanels();
        }

        private void OnLeaveApartmentClicked()
        {
            PlayClickSFX();

            // Complete leave apartment objective, add escape building
            objectiveSystem.CompleteObjective("leave_apt");
            objectiveSystem.AddObjective("escape_building", "Find a way down to the ground floor", ObjectiveStatus.Active);

            // Fade atmosphere audio
            if (atmosphereSource != null)
            {
                StartCoroutine(FadeOutAudio(atmosphereSource, 1.2f));
            }

            Hide(0.8f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TransitionFromApartmentToHallway();
                }
            });
        }

        private IEnumerator FadeOutAudio(AudioSource src, float duration)
        {
            float startVol = src.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                src.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            src.Stop();
        }
        #endregion

        #region UI Display Helpers
        private void UpdateClockUI()
        {
            if (gameClock == null) return;

            if (clockTimeText != null) clockTimeText.text = gameClock.GetTimeString();
            if (clockDayText != null) clockDayText.text = $"DAY {gameClock.DayCount}";
            if (clockText != null && clockText != clockTimeText)
            {
                clockText.text = $"<color=#FFA726>{gameClock.DayName} NIGHT</color>  |  {gameClock.GetTimeString()}";
            }
        }

        private void UpdateVitalsUI()
        {
            if (currentPlayer == null) return;

            // Twine Style 4 Vital Bars
            if (hpValueText != null) hpValueText.text = currentPlayer.hp.ToString();
            if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.hp / Mathf.Max(1, currentPlayer.hpMax));

            if (staminaValueText != null) staminaValueText.text = currentPlayer.stamina.ToString();
            if (staminaBarFill != null) staminaBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.stamina / Mathf.Max(1, currentPlayer.staminaMax));

            if (hungerValueText != null) hungerValueText.text = currentPlayer.hunger.ToString();
            if (hungerBarFill != null) hungerBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.hunger / 100f);

            if (thirstValueText != null) thirstValueText.text = currentPlayer.thirst.ToString();
            if (thirstBarFill != null) thirstBarFill.fillAmount = Mathf.Clamp01((float)currentPlayer.thirst / 100f);

            // Legacy HUD fields
            if (vitalsHpText != null) vitalsHpText.text = $"<color=#FF5555><b>HP</b></color> {currentPlayer.hp}/{currentPlayer.hpMax}";
            if (vitalsStaminaText != null) vitalsStaminaText.text = $"<color=#55FF88><b>STM</b></color> {currentPlayer.stamina}/{currentPlayer.staminaMax}";
        }

        private void UpdateObjectiveUI()
        {
            if (objectiveText != null && objectiveSystem != null)
            {
                objectiveText.text = $"<b>[!] OBJECTIVE:</b> {objectiveSystem.GetActiveObjectiveText()}";
            }
        }

        private void PopulateTableStashUI()
        {
            if (floorItemsCountText != null)
            {
                floorItemsCountText.text = $"■ Floor Items ({tableStash.Count})";
            }

            RefreshRightGearPanel();

            if (tableStashGrid == null) return;

            // Clear old children
            for (int i = tableStashGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(tableStashGrid.GetChild(i).gameObject);
            }

            if (tableStash.Count == 0)
            {
                if (tableEmptyNotice != null) tableEmptyNotice.SetActive(true);
                return;
            }

            if (tableEmptyNotice != null) tableEmptyNotice.SetActive(false);

            foreach (var item in tableStash)
            {
                CreateItemCard(tableStashGrid, item.itemId, item.quantity);
            }
        }

        private void CreateItemCard(Transform parent, string itemId, int quantity)
        {
            GameObject card = new GameObject("ItemCard_" + itemId);
            card.transform.SetParent(parent, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(86f, 88f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 86f;
            le.preferredHeight = 88f;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);
            cardBg.raycastTarget = false;

            Outline cardOl = card.AddComponent<Outline>();
            cardOl.effectColor = new Color(0.25f, 0.35f, 0.45f, 0.6f);
            cardOl.effectDistance = new Vector2(1f, -1f);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.5f, 1f);
            ir.anchorMax = new Vector2(0.5f, 1f);
            ir.pivot = new Vector2(0.5f, 1f);
            ir.anchoredPosition = new Vector2(0f, -8f);
            ir.sizeDelta = new Vector2(42f, 42f);

            Sprite iconSprite = ItemDatabase.GetItemIcon(itemId);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.fontSize = 9.5f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.85f, 0.88f, 0.92f);
            label.text = quantity > 1 ? $"{itemId} (x{quantity})" : itemId;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 0f);
            lr.pivot = new Vector2(0.5f, 0f);
            lr.sizeDelta = new Vector2(-8f, 30f);
            lr.anchoredPosition = new Vector2(0f, 4f);
        }

        private void PlayClickSFX()
        {
            if (sfxSource != null && clickClip != null)
            {
                sfxSource.PlayOneShot(clickClip, 0.7f);
            }
        }
        #endregion
    }
}
