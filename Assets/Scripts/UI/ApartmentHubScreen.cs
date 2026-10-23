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
    /// Master Controller for the Apartment Hub (Unit 407 - Prologue).
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
        [SerializeField] private TextMeshProUGUI btnModeGearLabel;
        [SerializeField] private GameObject btnModeGearBadge;
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

        [Header("Tactical Map & Skills View Controllers")]
        [SerializeField] private TacticalMapViewController tacticalMapController;
        [SerializeField] private TacticalSkillsViewController tacticalSkillsController;
        [SerializeField] private TacticalGearViewController tacticalGearController;

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
        [SerializeField] private GameObject floorItemsCard;

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
        [SerializeField] private TextMeshProUGUI packedSuccessText;

        [Header("Model A: Domestic Scavenge Actions")]
        [SerializeField] private Button searchKitchenButton;
        [SerializeField] private TextMeshProUGUI searchKitchenLabel;
        [SerializeField] private TextMeshProUGUI searchKitchenSub;

        [SerializeField] private Button searchBathroomButton;
        [SerializeField] private TextMeshProUGUI searchBathroomLabel;
        [SerializeField] private TextMeshProUGUI searchBathroomSub;

        [SerializeField] private Button searchClosetButton;
        [SerializeField] private TextMeshProUGUI searchClosetLabel;
        [SerializeField] private TextMeshProUGUI searchClosetSub;

        [SerializeField] private Button checkPhoneButton;
        [SerializeField] private Button listenRadioButton;
        [SerializeField] private TextMeshProUGUI listenRadioLabel;
        [SerializeField] private TextMeshProUGUI listenRadioSub;
        [SerializeField] private Button hackTerminalButton;
        [SerializeField] private TextMeshProUGUI hackTerminalLabel;
        [SerializeField] private TextMeshProUGUI hackTerminalSub;
        [SerializeField] private Button leaveApartmentButton;

        [Header("Story Log System (Option A)")]
        [SerializeField] private GameObject situationBox;
        [SerializeField] private TextMeshProUGUI situationBoxText;
        [SerializeField] private Button openStoryLogButton;
        [SerializeField] private GameObject storyLogModal;
        [SerializeField] private Button closeStoryLogButton;
        [SerializeField] private Button closeStoryLogBottomButton;
        [SerializeField] private TextMeshProUGUI storyLogContentText;

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
        private List<InventoryEntry> floorItems = new List<InventoryEntry>();
        private string equippedHead = "";
        private string equippedBody = "";
        private string equippedWpn = "";
        private string equippedLegs = "";
        private string equippedFeet = "";

        private bool hasPacked = false;
        private bool kitchenSearched = false;
        private bool bathroomSearched = false;
        private bool closetSearched = false;
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
            EnsureHubMenuSanitized();
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

            if (tacticalGearController != null)
            {
                tacticalGearController.OnVitalsUpdated += () =>
                {
                    UpdateVitalsUI();
                };
                tacticalGearController.OnInventoryUpdated += () =>
                {
                    UpdateFloorItemsNotification();
                    RefreshStoryLogUI();
                };
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

            if (openStoryLogButton != null)
            {
                openStoryLogButton.onClick.RemoveAllListeners();
                openStoryLogButton.onClick.AddListener(OnOpenStoryLogClicked);
            }

            if (closeStoryLogButton != null)
            {
                closeStoryLogButton.onClick.RemoveAllListeners();
                closeStoryLogButton.onClick.AddListener(OnCloseStoryLogClicked);
            }

            if (closeStoryLogBottomButton != null)
            {
                closeStoryLogBottomButton.onClick.RemoveAllListeners();
                closeStoryLogBottomButton.onClick.AddListener(OnCloseStoryLogClicked);
            }

            InitializeStoryLogs();

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



            if (searchKitchenButton != null)
            {
                searchKitchenButton.onClick.RemoveAllListeners();
                searchKitchenButton.onClick.AddListener(OnSearchKitchenClicked);
            }

            if (searchBathroomButton != null)
            {
                searchBathroomButton.onClick.RemoveAllListeners();
                searchBathroomButton.onClick.AddListener(OnSearchBathroomClicked);
            }

            if (searchClosetButton != null)
            {
                searchClosetButton.onClick.RemoveAllListeners();
                searchClosetButton.onClick.AddListener(OnSearchClosetClicked);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.onClick.RemoveAllListeners();
                floorItemsButton.onClick.AddListener(OnTakeAllFloorItemsClicked);
            }
        }

        private readonly List<TacticalChoiceButton> activeChoiceButtons = new List<TacticalChoiceButton>();

        private void EnsureHubMenuSanitized()
        {
            if (hubMenuContainer == null) return;

            // 1. Hide any orphaned floor take button inside the narrative choice menu
            foreach (var t in hubMenuContainer.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.gameObject.name == "Btn_TakeFloorItems")
                {
                    t.gameObject.SetActive(false);
                }
            }

            // 2. Ensure SituationBox and PackedSuccessBanner stay directly under hubMenuContainer (outside scrollview)
            if (situationBox != null && situationBox.transform.parent != hubMenuContainer.transform)
            {
                situationBox.transform.SetParent(hubMenuContainer.transform, false);
            }
            if (packedSuccessBanner != null && packedSuccessBanner.transform.parent != hubMenuContainer.transform)
            {
                packedSuccessBanner.transform.SetParent(hubMenuContainer.transform, false);
            }

            // 3. Find or Wrap in ScrollView for choices
            ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);
            Transform contentParent = null;

            if (sr == null)
            {
                var oldVlg = hubMenuContainer.GetComponent<VerticalLayoutGroup>();
                if (oldVlg != null)
                {
                    oldVlg.spacing = 4f;
                }

                GameObject scrollObj = new GameObject("ChoiceScrollView", typeof(RectTransform));
                scrollObj.transform.SetParent(hubMenuContainer.transform, false);
                RectTransform srt = scrollObj.GetComponent<RectTransform>();
                srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
                srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
                LayoutElement scLe = scrollObj.AddComponent<LayoutElement>();
                scLe.flexibleHeight = 1f;
                scLe.flexibleWidth = 1f;

                sr = scrollObj.AddComponent<ScrollRect>();
                sr.horizontal = false;
                sr.vertical = true;
                sr.movementType = ScrollRect.MovementType.Clamped;
                sr.scrollSensitivity = 28f;

                GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
                viewport.transform.SetParent(scrollObj.transform, false);
                RectTransform vprt = viewport.GetComponent<RectTransform>();
                vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
                vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
                viewport.AddComponent<RectMask2D>();
                sr.viewport = vprt;

                GameObject content = new GameObject("Content", typeof(RectTransform));
                content.transform.SetParent(viewport.transform, false);
                RectTransform crt = content.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
                crt.pivot = new Vector2(0.5f, 1f);
                crt.anchoredPosition = Vector2.zero;
                crt.sizeDelta = Vector2.zero;

                VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 4f;
                vlg.padding = new RectOffset(2, 6, 2, 2);
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                sr.content = crt;
                contentParent = content.transform;

                // Reparent only choice buttons to contentParent
                List<Transform> childrenToMove = new List<Transform>();
                for (int i = 0; i < hubMenuContainer.transform.childCount; i++)
                {
                    Transform child = hubMenuContainer.transform.GetChild(i);
                    if (child != scrollObj.transform && child != situationBox?.transform && child != packedSuccessBanner?.transform)
                    {
                        childrenToMove.Add(child);
                    }
                }
                foreach (var c in childrenToMove)
                {
                    c.SetParent(contentParent, false);
                }
            }
            else
            {
                contentParent = sr.content != null ? sr.content.transform : sr.transform;
                var cvlg = contentParent.GetComponent<VerticalLayoutGroup>();
                if (cvlg != null)
                {
                    cvlg.spacing = 4f;
                    cvlg.padding = new RectOffset(2, 6, 2, 2);
                }
            }

            // 4. Ensure situationBox and packedSuccessBanner are correctly ordered before the scrollview
            if (situationBox != null) situationBox.transform.SetSiblingIndex(0);
            if (packedSuccessBanner != null) packedSuccessBanner.transform.SetSiblingIndex(1);
            if (sr != null) sr.transform.SetSiblingIndex(2);

            // 5. Dynamically create missing Scavenge Buttons if null
            Button templateBtn = packGearButton != null ? packGearButton : leaveApartmentButton;
            if (templateBtn != null)
            {
                if (searchKitchenButton == null)
                {
                    searchKitchenButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchKitchen",
                        "SEARCH KITCHENETTE FOR PROVISIONS",
                        "3 Minutes | Scrounge cupboards and fridge for preserved food and clean water.",
                        new Color(0.95f, 0.70f, 0.30f), OnSearchKitchenClicked, out searchKitchenLabel, out searchKitchenSub);
                }
                if (searchBathroomButton == null)
                {
                    searchBathroomButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchBathroom",
                        "RAID BATHROOM MEDICINE CABINET",
                        "2 Minutes | Grab painkillers, sterile bandages, and first-aid supplies.",
                        new Color(0.30f, 0.85f, 0.60f), OnSearchBathroomClicked, out searchBathroomLabel, out searchBathroomSub);
                }
                if (searchClosetButton == null)
                {
                    searchClosetButton = CreateScavengeButtonClone(templateBtn, contentParent, "Btn_SearchCloset",
                        "PULL WEATHER GEAR FROM CLOSET",
                        "2 Minutes | Retrieve weather protection, duct tape, and utility gear.",
                        new Color(0.40f, 0.75f, 0.95f), OnSearchClosetClicked, out searchClosetLabel, out searchClosetSub);
                }
            }

            // 6. Ensure logical child order in contentParent so choices flow naturally
            if (packGearButton != null) packGearButton.transform.SetSiblingIndex(0);
            if (searchKitchenButton != null) searchKitchenButton.transform.SetSiblingIndex(1);
            if (searchBathroomButton != null) searchBathroomButton.transform.SetSiblingIndex(2);
            if (searchClosetButton != null) searchClosetButton.transform.SetSiblingIndex(3);
            if (checkPhoneButton != null) checkPhoneButton.transform.SetSiblingIndex(4);
            if (listenRadioButton != null) listenRadioButton.transform.SetSiblingIndex(5);
            if (hackTerminalButton != null) hackTerminalButton.transform.SetSiblingIndex(6);
            if (leaveApartmentButton != null) leaveApartmentButton.transform.SetSiblingIndex(7);

            // 7. Re-skin all buttons in contentParent to compact 32px Fallout/BG3 CRPG row format
            SanitizeChoiceRowVisuals(contentParent);
        }

        private void SanitizeChoiceRowVisuals(Transform contentParent)
        {
            if (contentParent == null) return;

            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                if (child == null) continue;

                // 1. Force 32px height for high density
                LayoutElement le = child.GetComponent<LayoutElement>();
                if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 32f;
                le.flexibleWidth = 1f;

                // 2. Ensure Left Accent Bar
                Transform bar = child.Find("AccentBar");
                Image barImg = null;
                if (bar != null)
                {
                    barImg = bar.GetComponent<Image>();
                    RectTransform brt = bar.GetComponent<RectTransform>();
                    if (brt != null)
                    {
                        brt.anchorMin = new Vector2(0f, 0f);
                        brt.anchorMax = new Vector2(0f, 1f);
                        brt.pivot = new Vector2(0f, 0.5f);
                        brt.sizeDelta = new Vector2(3f, 0f);
                        brt.anchoredPosition = Vector2.zero;
                    }
                    bar.gameObject.SetActive(false);
                }

                // 3. Ensure Gutter with HoverChevron (>>) and HotkeyLabel ([ 1 ])
                Transform gutter = child.Find("Gutter");
                TextMeshProUGUI chTmp = null;
                TextMeshProUGUI hkTmp = null;

                if (gutter == null)
                {
                    GameObject gutObj = new GameObject("Gutter", typeof(RectTransform));
                    gutObj.transform.SetParent(child, false);
                    gutObj.transform.SetSiblingIndex(0);
                    RectTransform gutRect = gutObj.GetComponent<RectTransform>();
                    gutRect.anchorMin = new Vector2(0f, 0f);
                    gutRect.anchorMax = new Vector2(0f, 1f);
                    gutRect.pivot = new Vector2(0f, 0.5f);
                    gutRect.sizeDelta = new Vector2(46f, 0f);
                    gutRect.anchoredPosition = Vector2.zero;
                    gutter = gutObj.transform;

                    // HoverChevron (>>)
                    GameObject chObj = new GameObject("HoverChevron", typeof(RectTransform));
                    chObj.transform.SetParent(gutter, false);
                    RectTransform chRect = chObj.GetComponent<RectTransform>();
                    chRect.anchorMin = new Vector2(0f, 0f);
                    chRect.anchorMax = new Vector2(0f, 1f);
                    chRect.pivot = new Vector2(0f, 0.5f);
                    chRect.anchoredPosition = new Vector2(-4f, 0f);
                    chRect.sizeDelta = new Vector2(16f, 0f);
                    chTmp = chObj.AddComponent<TextMeshProUGUI>();
                    chTmp.fontSize = 12f;
                    chTmp.fontStyle = FontStyles.Bold;
                    chTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    chTmp.color = new Color(1f, 0.65f, 0.15f);
                    chTmp.text = ">>";
                    chTmp.raycastTarget = false;
                    chObj.SetActive(false);

                    // HotkeyLabel ([ 1 ])
                    GameObject hkObj = new GameObject("HotkeyLabel", typeof(RectTransform));
                    hkObj.transform.SetParent(gutter, false);
                    RectTransform hkRect = hkObj.GetComponent<RectTransform>();
                    hkRect.anchorMin = new Vector2(0f, 0f);
                    hkRect.anchorMax = new Vector2(1f, 1f);
                    hkRect.offsetMin = new Vector2(16f, 0f);
                    hkRect.offsetMax = Vector2.zero;
                    hkTmp = hkObj.AddComponent<TextMeshProUGUI>();
                    hkTmp.fontSize = 11.5f;
                    hkTmp.fontStyle = FontStyles.Bold;
                    hkTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    hkTmp.color = new Color(0.85f, 0.65f, 0.30f, 0.95f);
                    hkTmp.text = $"[ {i + 1} ]";
                    hkTmp.raycastTarget = false;
                    SundayTypography.ApplyFont(hkTmp, FontRole.Mono);
                }
                else
                {
                    Transform ch = gutter.Find("HoverChevron");
                    if (ch != null) chTmp = ch.GetComponent<TextMeshProUGUI>();
                    Transform hk = gutter.Find("HotkeyLabel");
                    if (hk != null) hkTmp = hk.GetComponent<TextMeshProUGUI>();
                }

                // 4. Ensure TextContainer is formatted as a single-line horizontal flex row
                Transform textCont = child.Find("TextContainer") ?? child.Find("LabelContainer");
                TextMeshProUGUI mainLbl = null;
                TextMeshProUGUI subLbl = null;

                if (textCont != null)
                {
                    RectTransform tcRect = textCont.GetComponent<RectTransform>();
                    tcRect.anchorMin = new Vector2(0f, 0f);
                    tcRect.anchorMax = new Vector2(1f, 1f);
                    tcRect.offsetMin = new Vector2(50f, 0f);
                    tcRect.offsetMax = new Vector2(-10f, 0f);

                    HorizontalLayoutGroup hlg = textCont.GetComponent<HorizontalLayoutGroup>();
                    if (hlg == null) hlg = textCont.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.spacing = 10f;
                    hlg.childAlignment = TextAnchor.MiddleLeft;
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = true;

                    Transform ml = textCont.Find("MainLabel") ?? textCont.Find("Label") ?? textCont.Find("Title");
                    if (ml != null)
                    {
                        mainLbl = ml.GetComponent<TextMeshProUGUI>();
                        if (mainLbl != null)
                        {
                            mainLbl.fontSize = 11.5f;
                            mainLbl.fontStyle = FontStyles.Bold;
                            mainLbl.alignment = TextAlignmentOptions.MidlineLeft;
                            SundayTypography.ApplyFont(mainLbl, FontRole.Body);

                            ContentSizeFitter csf = ml.GetComponent<ContentSizeFitter>();
                            if (csf == null) csf = ml.gameObject.AddComponent<ContentSizeFitter>();
                            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                        }
                    }

                    Transform sl = textCont.Find("SubLabel") ?? textCont.Find("SubText");
                    if (sl != null)
                    {
                        subLbl = sl.GetComponent<TextMeshProUGUI>();
                        if (subLbl != null)
                        {
                            subLbl.fontSize = 10f;
                            subLbl.alignment = TextAlignmentOptions.MidlineLeft;
                            subLbl.color = new Color(0.64f, 0.70f, 0.78f);
                            subLbl.textWrappingMode = TextWrappingModes.NoWrap;
                            subLbl.overflowMode = TextOverflowModes.Ellipsis;
                            SundayTypography.ApplyFont(subLbl, FontRole.Body);

                            LayoutElement slLe = sl.GetComponent<LayoutElement>();
                            if (slLe == null) slLe = sl.gameObject.AddComponent<LayoutElement>();
                            slLe.flexibleWidth = 1f;
                        }
                    }
                }

                // 5. Initialize or re-link TacticalChoiceButton
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    TacticalChoiceButton tcb = child.GetComponent<TacticalChoiceButton>();
                    if (tcb == null) tcb = child.gameObject.AddComponent<TacticalChoiceButton>();

                    Image bg = child.GetComponent<Image>();
                    Outline ol = child.GetComponent<Outline>();
                    Transform shine = child.Find("ShineSweep");
                    RectTransform shineRect = shine != null ? shine.GetComponent<RectTransform>() : null;
                    Color accent = barImg != null ? barImg.color : new Color(0.95f, 0.75f, 0.35f);

                    tcb.Initialize(btn, textCont as RectTransform, bg, barImg, ol, shineRect, accent, chTmp, hkTmp, mainLbl, subLbl);
                }
            }
        }

        private Button CreateScavengeButtonClone(Button template, Transform parent, string name, string title, string sub, Color accentColor, UnityEngine.Events.UnityAction action, out TextMeshProUGUI titleTmp, out TextMeshProUGUI subTmp)
        {
            GameObject go = Instantiate(template.gameObject, parent);
            go.name = name;
            go.SetActive(false);

            Button btn = go.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            if (action != null) btn.onClick.AddListener(action);

            titleTmp = null;
            subTmp = null;

            // Find AccentBar
            Transform bar = go.transform.Find("AccentBar");
            Image barImg = null;
            if (bar != null)
            {
                barImg = bar.GetComponent<Image>();
                if (barImg != null) barImg.color = accentColor;
            }

            // Find labels
            TextMeshProUGUI[] tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t.gameObject.name == "MainLabel" || t.gameObject.name == "Label" || t.gameObject.name == "Title" || t.fontSize > 11f)
                {
                    titleTmp = t;
                    t.text = title;
                }
                else if (t.gameObject.name == "SubLabel" || t.gameObject.name == "SubText" || t.fontSize <= 11f)
                {
                    subTmp = t;
                    t.text = string.IsNullOrEmpty(sub) ? "" : $"— {sub}";
                }
            }

            // Find gutter labels
            TextMeshProUGUI chTmp = null;
            TextMeshProUGUI hkTmp = null;
            Transform gut = go.transform.Find("Gutter");
            if (gut != null)
            {
                Transform ch = gut.Find("HoverChevron");
                if (ch != null) chTmp = ch.GetComponent<TextMeshProUGUI>();
                Transform hk = gut.Find("HotkeyLabel");
                if (hk != null) hkTmp = hk.GetComponent<TextMeshProUGUI>();
            }

            // Re-bind TacticalChoiceButton for hover animation & audio
            TacticalChoiceButton tcb = go.GetComponent<TacticalChoiceButton>();
            if (tcb != null)
            {
                RectTransform tcRect = go.transform.Find("TextContainer") as RectTransform;
                Image bg = go.GetComponent<Image>();
                Outline ol = go.GetComponent<Outline>();
                RectTransform shineRect = go.transform.Find("ShineSweep") as RectTransform;
                tcb.Initialize(btn, tcRect, bg, barImg, ol, shineRect, accentColor, chTmp, hkTmp, titleTmp, subTmp);
            }

            return btn;
        }

        public void InitializeWithCharacter(CharacterData character)
        {
            EnsureHubMenuSanitized();
            currentPlayer = character;

            // In accordance with Twine design:
            // Items accumulated during creation are dropped on the tablestash prior to packing!
            tableStash.Clear();
            floorItems.Clear();
            equippedHead = "";
            equippedBody = "";
            equippedWpn = "";
            equippedLegs = "";
            equippedFeet = "";

            if (currentPlayer != null)
            {
                currentPlayer.equippedHead = "";
                currentPlayer.equippedBody = "";
                currentPlayer.equippedWpn = "";
                currentPlayer.equippedLegs = "";
                currentPlayer.equippedFeet = "";
            }

            if (currentPlayer != null && currentPlayer.inventory != null)
            {
                foreach (var item in currentPlayer.inventory)
                {
                    tableStash.Add(new InventoryEntry(item.itemId, item.quantity));
                }
                currentPlayer.inventory.Clear();
            }

            hasPacked = false;
            kitchenSearched = false;
            bathroomSearched = false;
            closetSearched = false;
            introSeen = false;
            awakeningStep = 1;

            if (floorItemsCard != null && tacticalGearController == null) floorItemsCard.SetActive(false);
            if (searchKitchenButton != null) searchKitchenButton.gameObject.SetActive(false);
            if (searchBathroomButton != null) searchBathroomButton.gameObject.SetActive(false);
            if (searchClosetButton != null) searchClosetButton.gameObject.SetActive(false);
            UpdateFloorItemsNotification();

            UpdateClockUI();
            UpdateVitalsUI();
            UpdateObjectiveUI();
            PopulateTableStashUI();
            RefreshAllPDAPanels();
            SwitchPDATab("STATUS", playSound: false);
            SwitchConsoleMode(ConsoleMode.Narrative, playSound: false);
            EvaluateHubButtonStates();

            // Automatic Location Synchronization across Top HUD & Tactical Map
            string startLoc = (currentPlayer != null && !string.IsNullOrEmpty(currentPlayer.currentLocationId))
                ? currentPlayer.currentLocationId
                : "unit_407";
            SetLocation(startLoc);

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
            // 1. Hotkeys for Choice Selection (1-8) when in Narrative Choice Hub Mode
            if (currentConsoleMode == ConsoleMode.Narrative && hubMenuContainer != null && hubMenuContainer.activeInHierarchy)
            {
                for (int i = 0; i < Mathf.Min(activeChoiceButtons.Count, 8); i++)
                {
                    if (IsChoiceHotkeyPressed(i + 1))
                    {
                        activeChoiceButtons[i].TriggerClick();
                        break;
                    }
                }
            }

            // 2. Hotkeys for Console Mode Switching (Tab, I, Esc, F1-F3)
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.f2Key.wasPressedThisFrame)
                {
                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);
                }
                else if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.f3Key.wasPressedThisFrame)
                {
                    SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);
                }
                else if ((Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame) && currentConsoleMode != ConsoleMode.Narrative)
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
                else if (currentConsoleMode != ConsoleMode.Narrative && (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame))
                {
                    SwitchConsoleMode(ConsoleMode.Narrative);
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.F2))
            {
                SwitchConsoleMode(currentConsoleMode == ConsoleMode.PDA ? ConsoleMode.Narrative : ConsoleMode.PDA);
            }
            else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.F3))
            {
                SwitchConsoleMode(currentConsoleMode == ConsoleMode.Gear ? ConsoleMode.Narrative : ConsoleMode.Gear);
            }
            else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F1)) && currentConsoleMode != ConsoleMode.Narrative)
            {
                SwitchConsoleMode(ConsoleMode.Narrative);
            }
            else if (currentConsoleMode != ConsoleMode.Narrative && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))
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

            if (tabName == "MAP" && tacticalMapController != null)
            {
                tacticalMapController.RefreshAllNodes();
            }
            if (tabName == "SKILLS" && tacticalSkillsController != null && currentPlayer != null)
            {
                tacticalSkillsController.Refresh(currentPlayer);
            }

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

            if (tacticalSkillsController != null)
            {
                tacticalSkillsController.Refresh(currentPlayer);
            }

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
            // Deprecated: Player manually inspects and equips weapons/gear via Tactical Gear Workstation
        }

        private void UpdateFloorItemsNotification()
        {
            int count = floorItems != null ? floorItems.Count : 0;
            bool hasLoot = count > 0;

            if (btnModeGearLabel != null)
            {
                btnModeGearLabel.text = hasLoot 
                    ? $"[3] TACTICAL GEAR [I] <color=#F59E0B><b>({count})</b></color>" 
                    : "[3] TACTICAL GEAR [I]";
            }

            if (btnModeGearBadge != null)
            {
                btnModeGearBadge.SetActive(hasLoot);
            }

            if (btnModeGear != null)
            {
                var rect = btnModeGear.GetComponent<RectTransform>();
                if (rect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.gameObject.SetActive(hasLoot);
            }

            if (floorItemsCountText != null)
            {
                floorItemsCountText.text = hasLoot ? $"TAKE ALL ({count})" : "TAKE ALL";
            }
        }

        private void RefreshRightGearPanel()
        {
            UpdateFloorItemsNotification();

            if (tacticalGearController != null)
            {
                tacticalGearController.Initialize(currentPlayer, floorItems);
                return;
            }

            // Sync local equip strings with currentPlayer
            if (currentPlayer != null)
            {
                if (!string.IsNullOrEmpty(currentPlayer.equippedHead)) equippedHead = currentPlayer.equippedHead;
                if (!string.IsNullOrEmpty(currentPlayer.equippedBody)) equippedBody = currentPlayer.equippedBody;
                if (!string.IsNullOrEmpty(currentPlayer.equippedWpn)) equippedWpn = currentPlayer.equippedWpn;
                if (!string.IsNullOrEmpty(currentPlayer.equippedLegs)) equippedLegs = currentPlayer.equippedLegs;
                if (!string.IsNullOrEmpty(currentPlayer.equippedFeet)) equippedFeet = currentPlayer.equippedFeet;
            }

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

                int maxSlots = currentPlayer != null ? Mathf.Max(6, currentPlayer.bagLimit) : 12;
                int currentUsed = currentPlayer != null ? currentPlayer.GetTotalSlotsUsed() : inv.Count;
                if (pdaBackpackCountText != null)
                {
                    pdaBackpackCountText.text = $"{currentUsed}/{maxSlots}";
                }

                int slotsRendered = 0;
                for (int i = 0; i < inv.Count; i++)
                {
                    int size = ItemDatabase.GetItemSlotSize(inv[i].itemId);
                    if (size == 2)
                    {
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, inv[i].quantity, true, isExtension: false);
                        slotsRendered++;
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, 0, true, isExtension: true);
                        slotsRendered++;
                    }
                    else
                    {
                        CreateInventorySlotCell(pdaBackpackGrid, inv[i].itemId, inv[i].quantity, true, isExtension: false);
                        slotsRendered++;
                    }
                }

                while (slotsRendered < maxSlots)
                {
                    CreateInventorySlotCell(pdaBackpackGrid, null, 0, false, isExtension: false);
                    slotsRendered++;
                }
            }

            // 3. Update Floor / Room Items in Right Wing
            bool hasFloorLoot = floorItems != null && floorItems.Count > 0;
            if (floorItemsCard != null)
            {
                floorItemsCard.SetActive(hasFloorLoot);
            }

            if (floorItemsButton != null)
            {
                floorItemsButton.gameObject.SetActive(hasFloorLoot);
            }

            UpdateFloorItemsNotification();

            if (pdaFloorItemsGrid != null)
            {
                for (int i = pdaFloorItemsGrid.childCount - 1; i >= 0; i--)
                {
                    Destroy(pdaFloorItemsGrid.GetChild(i).gameObject);
                }

                if (pdaFloorEmptyText != null)
                {
                    pdaFloorEmptyText.gameObject.SetActive(!hasFloorLoot);
                }

                if (hasFloorLoot)
                {
                    foreach (var item in floorItems)
                    {
                        CreateFloorItemCell(pdaFloorItemsGrid, item.itemId, item.quantity);
                    }
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

        private void CreateInventorySlotCell(Transform parent, string itemId, int quantity, bool allowEquipClick, bool isExtension = false)
        {
            GameObject cell = new GameObject(string.IsNullOrEmpty(itemId) ? "EmptySlot" : (isExtension ? "Slot_" + itemId + "_Ext" : "Slot_" + itemId));
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(66f, 66f);

            Image bg = cell.AddComponent<Image>();
            bg.color = isExtension ? new Color(0.06f, 0.08f, 0.11f, 0.90f) : new Color(0.09f, 0.11f, 0.15f, 0.95f);

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = string.IsNullOrEmpty(itemId)
                ? new Color(0.18f, 0.20f, 0.24f, 0.75f)
                : (isExtension ? new Color(0.20f, 0.24f, 0.30f, 0.65f) : new Color(0.28f, 0.32f, 0.38f, 0.95f));
            ol.effectDistance = new Vector2(1f, -1f);

            if (string.IsNullOrEmpty(itemId)) return;

            if (isExtension)
            {
                // Visual extension indicator
                GameObject extObj = new GameObject("ExtText");
                extObj.transform.SetParent(cell.transform, false);
                TextMeshProUGUI extTmp = extObj.AddComponent<TextMeshProUGUI>();
                extTmp.fontSize = 7.5f;
                extTmp.alignment = TextAlignmentOptions.Center;
                extTmp.color = new Color(0.55f, 0.60f, 0.68f, 0.75f);
                extTmp.text = "[ >> BULKY ]\n<size=6.5><color=#64748B>PART 2/2</color></size>";
                extTmp.raycastTarget = false;
                RectTransform extr = extObj.GetComponent<RectTransform>();
                extr.anchorMin = Vector2.zero; extr.anchorMax = Vector2.one;
                extr.offsetMin = new Vector2(2f, 2f); extr.offsetMax = new Vector2(-2f, -2f);
                return;
            }

            // Contrast Well Halo
            GameObject wellObj = new GameObject("WellHalo");
            wellObj.transform.SetParent(cell.transform, false);
            Image wellImg = wellObj.AddComponent<Image>();
            wellImg.color = new Color(0.18f, 0.24f, 0.32f, 0.25f);
            wellImg.raycastTarget = false;
            RectTransform wrt = wellObj.GetComponent<RectTransform>();
            wrt.anchorMin = Vector2.zero; wrt.anchorMax = Vector2.one;
            wrt.offsetMin = new Vector2(4f, 12f); wrt.offsetMax = new Vector2(-4f, -4f);

            // 2-Slot badge if slotSize == 2
            int itemSlotSize = ItemDatabase.GetItemSlotSize(itemId);
            if (itemSlotSize == 2)
            {
                GameObject slotBadge = new GameObject("SlotBadge");
                slotBadge.transform.SetParent(cell.transform, false);
                RectTransform sbr = slotBadge.AddComponent<RectTransform>();
                sbr.anchorMin = new Vector2(1f, 1f); sbr.anchorMax = new Vector2(1f, 1f);
                sbr.pivot = new Vector2(1f, 1f);
                sbr.anchoredPosition = new Vector2(-2f, -2f);
                sbr.sizeDelta = new Vector2(30f, 11f);
                Image sbBg = slotBadge.AddComponent<Image>();
                sbBg.color = new Color(0.85f, 0.55f, 0.10f, 0.25f);
                sbBg.raycastTarget = false;

                GameObject sbtObj = new GameObject("Text");
                sbtObj.transform.SetParent(slotBadge.transform, false);
                TextMeshProUGUI sbTmp = sbtObj.AddComponent<TextMeshProUGUI>();
                sbTmp.fontSize = 7f;
                sbTmp.fontStyle = FontStyles.Bold;
                sbTmp.alignment = TextAlignmentOptions.Center;
                sbTmp.color = new Color(0.95f, 0.75f, 0.25f);
                sbTmp.text = "2-SLOT";
                sbTmp.raycastTarget = false;
                RectTransform sbtr = sbtObj.GetComponent<RectTransform>();
                sbtr.anchorMin = Vector2.zero; sbtr.anchorMax = Vector2.one;
                sbtr.offsetMin = Vector2.zero; sbtr.offsetMax = Vector2.zero;
            }

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

        private void CreateFloorItemCell(Transform parent, string itemId, int quantity)
        {
            GameObject cell = new GameObject(string.IsNullOrEmpty(itemId) ? "EmptySlot" : "FloorSlot_" + itemId);
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(66f, 66f);

            Image bg = cell.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.08f, 0.08f, 0.95f);

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = new Color(0.91f, 0.30f, 0.24f, 0.85f);
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

            // Short Item Name
            GameObject nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.95f, 0.85f, 0.85f);
            nameTmp.text = itemId.Length > 10 ? itemId.Substring(0, 9) + "." : itemId;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f);
            nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 14f);
            nr.anchoredPosition = new Vector2(0f, 1f);

            // Quantity badge
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
                qtr.anchorMin = Vector2.zero; qtr.anchorMax = Vector2.one; qtr.offsetMin = Vector2.zero; qtr.offsetMax = Vector2.zero;
            }

            // Click button to take this specific item
            Button btn = cell.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capturedId = itemId;
            btn.onClick.AddListener(() =>
            {
                OnTakeSingleFloorItemClicked(capturedId);
            });
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
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (beat1Container != null) beat1Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
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
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (beat2Container != null) beat2Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // INTERNAL MONOLOGUE ]</color>";
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
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    if (staticSource != null) staticSource.Stop();
                    if (beat3Container != null) beat3Container.SetActive(true);
                    if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // LIVING ROOM ]</color>";
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

            if (dockHeaderLocation != null) dockHeaderLocation.text = "<color=#FFA726>[ UNIT 407 // ACTIONS ]</color>";
            if (dockHeaderChannel != null) dockHeaderChannel.text = "<color=#10B981>STATUS: READY FOR TRANSIT</color>";

            EvaluateHubButtonStates();
        }

        private bool IsChoiceHotkeyPressed(int number)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                switch (number)
                {
                    case 1: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                    case 2: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                    case 3: return kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame;
                    case 4: return kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame;
                    case 5: return kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame;
                    case 6: return kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame;
                    case 7: return kb.digit7Key.wasPressedThisFrame || kb.numpad7Key.wasPressedThisFrame;
                    case 8: return kb.digit8Key.wasPressedThisFrame || kb.numpad8Key.wasPressedThisFrame;
                    case 9: return kb.digit9Key.wasPressedThisFrame || kb.numpad9Key.wasPressedThisFrame;
                }
            }
#endif
            KeyCode alpha = (KeyCode)((int)KeyCode.Alpha0 + number);
            KeyCode pad = (KeyCode)((int)KeyCode.Keypad0 + number);
            return Input.GetKeyDown(alpha) || Input.GetKeyDown(pad);
        }

        private void ReindexActiveChoiceHotkeys()
        {
            activeChoiceButtons.Clear();
            if (hubMenuContainer == null) return;

            ScrollRect sr = hubMenuContainer.GetComponentInChildren<ScrollRect>(true);
            Transform parent = sr != null && sr.content != null ? sr.content.transform : hubMenuContainer.transform;

            int index = 1;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    TacticalChoiceButton tcb = child.GetComponent<TacticalChoiceButton>();
                    if (tcb != null)
                    {
                        tcb.SetChoiceNumber(index);
                        activeChoiceButtons.Add(tcb);
                        index++;
                    }
                }
            }
        }

        private void EvaluateHubButtonStates()
        {
            // 1. Pack Gear Button
            if (packGearButton != null)
            {
                packGearButton.gameObject.SetActive(!hasPacked);
            }
            if (packedSuccessBanner != null)
            {
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
                        if (listenRadioLabel != null) listenRadioLabel.text = "<color=#F59E0B>[POLICE]</color> LISTEN TO EMERGENCY BAND";
                        if (listenRadioSub != null) listenRadioSub.text = "— 3 Minutes | Monitor encrypted dispatch channels";
                    }
                    else if (hasTech1)
                    {
                        listenRadioButton.interactable = true;
                        if (listenRadioLabel != null) listenRadioLabel.text = "<color=#38BDF8>[TECH 1]</color> HACK LOCAL POLICE CHATTER";
                        if (listenRadioSub != null) listenRadioSub.text = "— 5 Minutes | Intercept dispatch frequency and bypass scramblers";
                    }
                    else
                    {
                        listenRadioButton.interactable = false;
                        if (listenRadioLabel != null) listenRadioLabel.text = "<s>ACCESS EMERGENCY BAND</s>";
                        if (listenRadioSub != null) listenRadioSub.text = "<color=#EF4444>[LOCKED: REQ POLICE OR TECH 1 & TACTICS 4]</color>";
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
                        if (hackTerminalLabel != null) hackTerminalLabel.text = "<color=#10B981>[TECH 3]</color> BYPASS GOV FIREWALL";
                        if (hackTerminalSub != null) hackTerminalSub.text = "— 8 Minutes | Breach military encryption to extract directives";
                    }
                    else
                    {
                        hackTerminalButton.interactable = false;
                        if (hackTerminalLabel != null) hackTerminalLabel.text = "<s>BYPASS GOV FIREWALL</s>";
                        if (hackTerminalSub != null) hackTerminalSub.text = "<color=#EF4444>[LOCKED: REQ TECH 3 & TACTICS 6]</color>";
                    }
                }
            }

            // 5. Model A: Domestic Scavenge Actions (Kitchenette, Bathroom, Closet)
            if (searchKitchenButton != null)
            {
                searchKitchenButton.gameObject.SetActive(hasPacked);
                if (hasPacked)
                {
                    searchKitchenButton.interactable = !kitchenSearched;
                    if (searchKitchenLabel != null)
                    {
                        searchKitchenLabel.text = kitchenSearched
                            ? "<s>KITCHENETTE (CLEARED)</s>"
                            : "<color=#F59E0B>[3 MIN]</color> SEARCH KITCHENETTE FOR PROVISIONS";
                    }
                    if (searchKitchenSub != null)
                    {
                        searchKitchenSub.text = kitchenSearched
                            ? "— <color=#64748B>Cupboards and fridge stripped bare</color>"
                            : "— Scrounge cupboards and fridge for preserved food and clean water";
                    }
                }
            }

            if (searchBathroomButton != null)
            {
                searchBathroomButton.gameObject.SetActive(hasPacked);
                if (hasPacked)
                {
                    searchBathroomButton.interactable = !bathroomSearched;
                    if (searchBathroomLabel != null)
                    {
                        searchBathroomLabel.text = bathroomSearched
                            ? "<s>MEDICINE CABINET (CLEARED)</s>"
                            : "<color=#10B981>[2 MIN]</color> RAID BATHROOM MEDICINE CABINET";
                    }
                    if (searchBathroomSub != null)
                    {
                        searchBathroomSub.text = bathroomSearched
                            ? "— <color=#64748B>Medicine cabinet stripped bare</color>"
                            : "— Grab painkillers, sterile bandages, and first-aid supplies";
                    }
                }
            }

            if (searchClosetButton != null)
            {
                searchClosetButton.gameObject.SetActive(hasPacked);
                if (hasPacked)
                {
                    searchClosetButton.interactable = !closetSearched;
                    if (searchClosetLabel != null)
                    {
                        searchClosetLabel.text = closetSearched
                            ? "<s>HALLWAY CLOSET (CLEARED)</s>"
                            : "<color=#38BDF8>[2 MIN]</color> PULL WEATHER GEAR FROM CLOSET";
                    }
                    if (searchClosetSub != null)
                    {
                        searchClosetSub.text = closetSearched
                            ? "— <color=#64748B>Closet shelves and coat rack are empty</color>"
                            : "— Retrieve weather protection, duct tape, and utility gear";
                    }
                }
            }

            // 6. Leave Apartment (unlocked only after packing!)
            if (leaveApartmentButton != null)
            {
                leaveApartmentButton.gameObject.SetActive(hasPacked);
                TextMeshProUGUI leaveLbl = leaveApartmentButton.GetComponentInChildren<TextMeshProUGUI>();
                if (leaveLbl != null && leaveLbl.gameObject.name == "MainLabel")
                {
                    leaveLbl.text = "<color=#EF4444>[EXIT]</color> STEP INTO 4TH FLOOR CORRIDOR";
                }
            }

            UpdateFloorItemsNotification();

            // 7. Dynamic Numbering & Hotkey Indexing (Contiguous [ 1 ], [ 2 ], [ 3 ]...)
            ReindexActiveChoiceHotkeys();
        }

        private void OnPackGearClicked()
        {
            PlayClickSFX();

            introSeen = true;
            awakeningStep = 4;

            // Advance time +2 minutes
            gameClock.AddTime(2);

            // Move table stash into player inventory
            int gatheredCount = 0;
            if (currentPlayer != null)
            {
                if (tableStash.Count == 0)
                {
                    currentPlayer.AddItem("Glock 19", 1);
                    currentPlayer.AddItem("9mm Ammo", 24);
                    currentPlayer.AddItem("Multi-Tool", 1);
                    currentPlayer.AddItem("Flashlight", 1);
                    currentPlayer.AddItem("Bandages", 2);
                    gatheredCount = 5;
                }
                else
                {
                    foreach (var item in tableStash)
                    {
                        currentPlayer.AddItem(item.itemId, item.quantity);
                        gatheredCount += item.quantity;
                    }
                    tableStash.Clear();
                }
            }

            hasPacked = true;
            floorItems.Clear();

            AppendStoryLog(gameClock.GetTimeString(), "ACTION // WORK EQUIPMENT SECURED",
                $"You geared up. You strapped on your primary equipment and weapons. Sirens wail outside across the Ascott district corridor.");
            if (situationBoxText != null)
            {
                situationBoxText.text = $"\"<color=#10B981>✓ PRIMARY GEAR SECURED.</color> Holster strapped, boots laced, and {gatheredCount} items secured into tactical vest. Outside, sirens wail across the Ascott district. Scavenge the remaining rooms before stepping into the corridor.\"";
            }

            if (packedSuccessBanner != null)
            {
                packedSuccessBanner.SetActive(true);
            }
            if (packedSuccessText != null)
            {
                packedSuccessText.text = $"<color=#10B981>✓ Primary gear secured ({gatheredCount} items packed).\n<color=#38BDF8>[!] You can scavenge your apartment rooms below before stepping into the corridor.</color></color>";
            }

            // Objectives
            objectiveSystem.CompleteObjective("pack_gear");
            objectiveSystem.ActivateObjective("leave_apt");

            ShowHubMenu();
            PopulateTableStashUI();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchKitchenClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || kitchenSearched) return;

            kitchenSearched = true;
            gameClock.AddTime(3); // +3 minutes

            currentPlayer.AddItem("Bottled Water", 2);
            currentPlayer.AddItem("Canned Tuna", 1);
            currentPlayer.AddItem("Can Opener", 1);
            currentPlayer.AddItem("Stale Coffee", 1);

            AppendStoryLog(gameClock.GetTimeString(), "SCAVENGE // KITCHENETTE RAIDED",
                "You raided the kitchen cupboards and fridge. You secured 2x Bottled Water, Canned Tuna, a manual Can Opener, and Stale Coffee before the tap sputtered dead brown sludge.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#F59E0B>✓ KITCHENETTE RAIDED.</color> Bottled Water (x2), Canned Tuna (x1), Can Opener (x1), and Stale Coffee (x1) secured into your pack. The tap sputters brown sediment.\"";
            }

            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchBathroomClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || bathroomSearched) return;

            bathroomSearched = true;
            gameClock.AddTime(2); // +2 minutes

            currentPlayer.AddItem("Painkillers", 1);
            currentPlayer.AddItem("Bandage", 2);
            currentPlayer.AddItem("Rubbing Alcohol", 1);

            AppendStoryLog(gameClock.GetTimeString(), "SCAVENGE // MEDICINE CABINET",
                "Behind the bathroom mirror, you grabbed emergency medical supplies: Painkillers, 2x Bandages, and Rubbing Alcohol. The mirror reflects your pale face.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#10B981>✓ MEDICINE CABINET CLEARED.</color> Smashed mirror cabinet. Painkillers (x1), Sterile Bandages (x2), and Rubbing Alcohol (x1) secured into first-aid pouch.\"";
            }

            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnSearchClosetClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || closetSearched) return;

            closetSearched = true;
            gameClock.AddTime(2); // +2 minutes

            currentPlayer.AddItem("Rain Poncho", 1);
            currentPlayer.AddItem("Duct Tape", 1);
            currentPlayer.AddItem("Matches", 1);

            AppendStoryLog(gameClock.GetTimeString(), "SCAVENGE // HALLWAY CLOSET",
                "You rummaged through the dark hallway closet. You pulled out a heavy Rain Poncho, industrial Duct Tape, and a box of waterproof Matches.");

            if (situationBoxText != null)
            {
                situationBoxText.text = "\"<color=#38BDF8>✓ WEATHER GEAR RETRIEVED.</color> Heavy Rain Poncho, industrial Duct Tape, and waterproof Matches secured from the top shelf.\"";
            }

            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnTakeAllFloorItemsClicked()
        {
            PlayClickSFX();
            if (currentPlayer == null || floorItems.Count == 0) return;

            foreach (var item in floorItems)
            {
                currentPlayer.AddItem(item.itemId, item.quantity);
            }
            floorItems.Clear();

            AppendStoryLog(gameClock.GetTimeString(), "LOOT // ROOM ITEMS SECURED",
                "You scooped up all remaining supplies from the room into your backpack.");

            UpdateFloorItemsNotification();
            RefreshRightGearPanel();
            RefreshAllPDAPanels();
            EvaluateHubButtonStates();
        }

        private void OnTakeSingleFloorItemClicked(string itemId)
        {
            PlayClickSFX();
            if (currentPlayer == null) return;

            int idx = floorItems.FindIndex(i => i.itemId == itemId);
            if (idx >= 0)
            {
                var item = floorItems[idx];
                currentPlayer.AddItem(item.itemId, item.quantity);
                floorItems.RemoveAt(idx);

                AppendStoryLog(gameClock.GetTimeString(), "LOOT // ITEM SECURED",
                    $"You picked up {item.itemId} (x{item.quantity}) from the room.");

                UpdateFloorItemsNotification();
                RefreshRightGearPanel();
                RefreshAllPDAPanels();
                EvaluateHubButtonStates();
            }
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

            AppendStoryLog(gameClock.GetTimeString(), "INTEL // SOCIAL MEDIA INTERCEPT",
                "CityScanner reports police in riot gear and gas masks sealing 5th St Hospital. A blurry civilian video shows an infected person continuing to sprint despite three center-mass gunshot wounds before the feed cuts. Cellular network is dead.");

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

            AppendStoryLog(gameClock.GetTimeString(), "INTEL // EMERGENCY POLICE DISPATCH",
                "Emergency dispatch confirmed unrestricted lethal cranial force authorization. Body shots ineffective. Neutralization requires cranial destruction. Harborview perimeter compromised.");

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

            AppendStoryLog(gameClock.GetTimeString(), "RECON // MILITARY DIRECTIVE EXTRACTION",
                "Extracted CDC & Homeland Defense containment directive. Saved Tier-1 Emergency Access Clearance Passkey directly to your Survivor PDA storage.");

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

        #region Dynamic Location Management
        /// <summary>
        /// Automatically synchronizes the player's location across:
        /// 1. Top HUD location badge (locationText / LocationBadge)
        /// 2. Bottom Dock location header (dockHeaderLocation)
        /// 3. CharacterData persistent location
        /// 4. Tactical Map controller bio-radar beacon & node selection
        /// </summary>
        public void SetLocation(string roomId, string customSectorSubtitle = null)
        {
            if (string.IsNullOrEmpty(roomId)) roomId = "unit_407";

            string roomDisplay = roomId.ToUpper();
            string sectorSubtitle = customSectorSubtitle;

            if (tacticalMapController != null)
            {
                tacticalMapController.SetCurrentLocation(roomId);
                var node = tacticalMapController.GetNode(roomId);
                if (node != null)
                {
                    roomDisplay = !string.IsNullOrEmpty(node.roomCode) ? node.roomCode : node.id.ToUpper();
                    if (string.IsNullOrEmpty(sectorSubtitle))
                    {
                        sectorSubtitle = !string.IsNullOrEmpty(node.shortSub) ? node.shortSub : node.title;
                    }
                }
            }

            UpdateLocationDisplay(roomDisplay, sectorSubtitle);

            if (currentPlayer != null)
            {
                currentPlayer.currentLocationId = roomId;
                if (locationText != null) currentPlayer.currentLocationName = locationText.text;
            }
        }

        public void UpdateLocationDisplay(string roomTitle, string sectorSubtitle = null)
        {
            string displayTitle = !string.IsNullOrEmpty(roomTitle) ? roomTitle.ToUpper() : "UNIT 407";
            string formattedTop = $"■ {displayTitle}";

            if (!string.IsNullOrEmpty(sectorSubtitle))
            {
                formattedTop += $" // {sectorSubtitle.ToUpper()}";
            }
            else if (!displayTitle.Contains("ASCOTT"))
            {
                formattedTop += " // THE ASCOTT RESIDENCY [4TH FLOOR]";
            }

            if (locationText != null)
            {
                locationText.text = formattedTop;
            }

            if (dockHeaderLocation != null)
            {
                dockHeaderLocation.text = $"<color=#FFA726>[ {displayTitle} ]</color>";
            }
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
            rt.sizeDelta = new Vector2(98f, 96f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 98f;
            le.preferredHeight = 96f;

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
            ir.anchoredPosition = new Vector2(0f, -6f);
            ir.sizeDelta = new Vector2(46f, 46f);

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
            label.fontSize = 11.5f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.88f, 0.91f, 0.95f);
            label.text = quantity > 1 ? $"{itemId} (x{quantity})" : itemId;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 0f);
            lr.pivot = new Vector2(0.5f, 0f);
            lr.sizeDelta = new Vector2(-6f, 36f);
            lr.anchoredPosition = new Vector2(0f, 4f);
        }

        #region Story Log System (Option A)
        private readonly List<string> _storyLogs = new List<string>();

        private void InitializeStoryLogs()
        {
            if (_storyLogs.Count > 0) return;

            _storyLogs.Add(
                "<b><color=#EF5350>[23:43] TV BROADCAST // EMERGENCY ALERT SYSTEM (CIVIL DANGER WARNING)</color></b>\n" +
                "\"The following message is transmitted at the request of the King County Sheriff's Office. This is not a test.\n\n" +
                "A Civil Danger Warning is in effect for the Seattle metropolitan area. Authorities reported a rapidly spreading biological hazard. Affected individuals exhibit extreme, unprovoked violence. The pathogen is highly contagious through fluid contact.\n\n" +
                "Shelter in place immediately. Major interstates are closed. Lock all doors, turn off lights, and remain completely silent. The National Guard has been deployed. Lethal force is authorized.\""
            );

            _storyLogs.Add(
                "<b><color=#F5BF42>[23:44] REACTION // INNER MONOLOGUE</color></b>\n" +
                "\"Lethal force authorized... They don't lock down the whole city for mass hysteria. And this front door isn't heavy enough to keep a panicked mob out.\"\n\n" +
                "<i>Through the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens. And closer... the muffled sound of heavy, frantic footsteps pounding down the hallway outside your door.</i>"
            );

            _storyLogs.Add(
                "<b><color=#4FC3F7>[23:45] APARTMENT HUB // UNIT 407 LIVING ROOM</color></b>\n" +
                "You are standing in the middle of your living room in your pajamas. You feel naked. Vulnerable.\n\n" +
                "You glance around the dim room, illuminated only by the harsh red glow of the TV. Your everyday carry sits on the cramped desk. But your heavier equipment rests by the front door, exactly where you dropped it after yesterday's shift.\n\n" +
                "It's not a tactical survival kit. But right now, the tools of your trade are the only thing standing between you and whatever violent outbreak is tearing the city apart."
            );

            RefreshStoryLogUI();
        }

        public void AppendStoryLog(string timestamp, string header, string content)
        {
            InitializeStoryLogs();
            string entry = $"<b><color=#66BB6A>[{timestamp}] {header}</color></b>\n{content}";
            _storyLogs.Add(entry);
            RefreshStoryLogUI();
        }

        private void RefreshStoryLogUI()
        {
            if (storyLogContentText != null)
            {
                storyLogContentText.text = string.Join("\n\n<color=#2C3E50>──────────────────────────────────────────────────────────────────────────</color>\n\n", _storyLogs);
            }
        }

        public void OnOpenStoryLogClicked()
        {
            PlayClickSFX();
            InitializeStoryLogs();
            if (storyLogModal != null) storyLogModal.SetActive(true);
        }

        public void OnCloseStoryLogClicked()
        {
            PlayClickSFX();
            if (storyLogModal != null) storyLogModal.SetActive(false);
        }
        #endregion

        private void PlayClickSFX()
        {
            if (sfxSource != null && clickClip != null)
            {
                sfxSource.PlayOneShot(clickClip, 0.4f);
            }
        }
        #endregion
    }
}
