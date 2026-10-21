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
    /// Follows authentic Twine design from Prologue.twee, Prologue_Menu.twee,
    /// Prologue_Phone.twee, Prologue_Radio.twee, and Prologue_Hacking.twee.
    /// 
    /// Adheres to the Nintendo 3DS 60/40 screen layout:
    /// - Top Stage (60%): Centered framed Living Room Viewport (1152x610), tactical telemetry wings,
    ///   vitals HUD, real-time clock, and dynamic objective tracker pill.
    /// - Bottom Dock (40%): Narrative awakening sequence (Beats 1-3) with dedicated containers
    ///   followed by the interactive apartment hub menu and modal sub-views (Phone, Radio, Hacking).
    /// Includes mouse double-click debounce protection and multi-input advancement (Space, Enter, Click).
    /// </summary>
    public class ApartmentHubScreen : ScreenBase, IPointerClickHandler
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
            objectiveSystem.OnObjectiveUpdated += (o) => UpdateObjectiveUI();

            loreSystem = new LoreSystem();
        }

        private void HookUIButtons()
        {
            if (fullScreenClickCatcher != null)
            {
                fullScreenClickCatcher.onClick.RemoveAllListeners();
                fullScreenClickCatcher.onClick.AddListener(OnDialogueAreaClicked);
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

            if (pdaMenuButton != null)
            {
                pdaMenuButton.onClick.RemoveAllListeners();
                pdaMenuButton.onClick.AddListener(() =>
                {
                    PlayClickSFX();
                });
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.onClick.RemoveAllListeners();
                floorItemsButton.onClick.AddListener(() =>
                {
                    PlayClickSFX();
                });
            }
        }

        public void InitializeWithCharacter(CharacterData character)
        {
            currentPlayer = character;

            // In accordance with Twine design:
            // Items accumulated during creation are dropped on the tablestash prior to packing!
            tableStash.Clear();
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
            EvaluateHubButtonStates();

            // Load player avatar frames
            cachedAvatarFrames = CharacterCreationScreen.LoadPlayerFrames();
        }

        protected override void OnScreenOpened()
        {
            base.OnScreenOpened();

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

        private void Update()
        {
            // Debounced multi-input check (Space or Enter to advance awakening dialogue)
            if (!introSeen && awakeningContainer != null && awakeningContainer.activeSelf)
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

        public void OnPointerClick(PointerEventData eventData)
        {
            OnDialogueAreaClicked();
        }

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
                    if (staticSource != null && staticClip != null)
                    {
                        staticSource.clip = staticClip;
                        staticSource.volume = 0.2f;
                        staticSource.Play();
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

            // Objectives
            objectiveSystem.CompleteObjective("pack_gear");
            objectiveSystem.ActivateObjective("leave_apt");

            EvaluateHubButtonStates();
            PopulateTableStashUI();
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
            if (clockText != null) clockText.text = $"<color=#FFA726>{gameClock.DayName} NIGHT</color>  |  {gameClock.GetTimeString()}";
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

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 92f;
            le.preferredHeight = 96f;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);
            cardBg.raycastTarget = false;

            Outline cardOl = card.AddComponent<Outline>();
            cardOl.effectColor = new Color(0.25f, 0.35f, 0.45f, 0.6f);
            cardOl.effectDistance = new Vector2(1f, -1f);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            LayoutElement iconLe = iconObj.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 44f;
            iconLe.preferredHeight = 44f;
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

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
            label.fontSize = 10.5f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.85f, 0.88f, 0.92f);
            label.text = quantity > 1 ? $"{itemId} (x{quantity})" : itemId;
            label.raycastTarget = false;
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
