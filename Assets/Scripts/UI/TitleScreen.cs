using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controls the Title Screen / Main Menu passage (Title.twee).
    /// Plays main menu BGM, loops title logo & background animation,
    /// and handles New Game, Load Game, Settings, and Quit options.
    /// </summary>
    public class TitleScreen : ScreenBase
    {
        [Header("Animation Players")]
        [SerializeField] private FrameAnimationPlayer backgroundAnimationPlayer;
        [SerializeField] private FrameAnimationPlayer titleLogoAnimationPlayer;

        [Header("Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Modals")]
        [SerializeField] private GameObject settingsModal;
        [SerializeField] private GameObject loadGameModal;

        // Multi-Slot Load Modal UI References
        private TextMeshProUGUI modalTitleText;
        private TextMeshProUGUI[] slotTitleTexts = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private TextMeshProUGUI[] slotDetailTexts = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private Button[] slotLoadButtons = new Button[SaveManager.TotalSlotCount];
        private TextMeshProUGUI[] slotLoadBtnLabels = new TextMeshProUGUI[SaveManager.TotalSlotCount];
        private Button[] slotDeleteButtons = new Button[SaveManager.TotalSlotCount];
        private Image[] slotCardBgs = new Image[SaveManager.TotalSlotCount];
        private Outline[] slotCardOutlines = new Outline[SaveManager.TotalSlotCount];

        protected override void Awake()
        {
            base.Awake();

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        public override void Show(float duration = -1, System.Action onComplete = null)
        {
            // Start Menu Music immediately when screen begins opening
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            base.Show(duration, onComplete);
        }

        protected override void OnScreenOpened()
        {
            // Ensure Menu Music is playing
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            if (backgroundAnimationPlayer != null)
            {
                backgroundAnimationPlayer.Play();
            }

            if (titleLogoAnimationPlayer != null)
            {
                titleLogoAnimationPlayer.Play();
            }
        }

        private void OnNewGameClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM(1f);
            }

            Hide(0.8f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartNewGame();
                }
            });
        }

        private void Update()
        {
            // Shortcut F9 to load game directly from title screen
            bool pressF9 = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) pressF9 = true;
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F9)) pressF9 = true;
#endif
            if (pressF9 && SaveManager.Instance != null && SaveManager.Instance.HasAnySave())
            {
                int recentSlot = SaveManager.Instance.GetMostRecentPopulatedSlotIndex();
                if (recentSlot >= 0)
                {
                    ExecuteResumeSavedSlot(recentSlot);
                }
            }
        }

        private void OnLoadGameClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            ShowLoadGameModal();
        }

        private void ShowLoadGameModal()
        {
            EnsureLoadGameModalBuilt();
            RefreshLoadSlotCards();

            if (loadGameModal != null)
            {
                loadGameModal.transform.SetAsLastSibling();
                loadGameModal.SetActive(true);
            }
        }

        private void RefreshLoadSlotCards()
        {
            bool anySave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();
            if (modalTitleText != null)
            {
                modalTitleText.text = anySave
                    ? "■ SELECT ARCHIVED SURVIVOR SLOT // CLASSIFIED DOSSIER"
                    : "■ SURVIVOR ARCHIVE // ALL SLOTS EMPTY";
            }

            for (int i = 0; i < SaveManager.TotalSlotCount; i++)
            {
                int slotIdx = i;
                var s = SaveManager.Instance != null ? SaveManager.Instance.GetSlotSummary(slotIdx) : default;
                string slotTitle = SaveManager.Instance != null ? SaveManager.Instance.GetSlotTitle(slotIdx) : $"SLOT {slotIdx}";

                if (s.exists)
                {
                    string savedTimeBadge = !string.IsNullOrEmpty(s.realSaveTime)
                        ? $"<color=#64748B>[{s.realSaveTime}]</color>"
                        : "";

                    if (slotTitleTexts[slotIdx] != null)
                    {
                        string titleHex = slotIdx == 0 ? "#38BDF8" : "#F59E0B";
                        slotTitleTexts[slotIdx].text =
                            $"<color={titleHex}><b>■ {slotTitle}</b></color>   <color=#F8FAFC><b>{s.characterName.ToUpper()}</b></color> <color=#94A3B8>({s.job})</color>   {savedTimeBadge}";
                    }

                    if (slotDetailTexts[slotIdx] != null)
                    {
                        slotDetailTexts[slotIdx].text =
                            $"<color=#FBBF24><b>{s.roomDisplayName}</b></color>   |   <color=#E2E8F0>{s.clockDisplay}</color>   |   " +
                            $"HP: <color=#EF4444><b>{s.hp}/{s.hpMax}</b></color>   STM: <color=#10B981><b>{s.stamina}/{s.staminaMax}</b></color>   HUM: <color=#A78BFA><b>{s.humanity}%</b></color>";
                    }

                    if (slotLoadButtons[slotIdx] != null)
                    {
                        slotLoadButtons[slotIdx].interactable = true;
                        Image btnBg = slotLoadButtons[slotIdx].GetComponent<Image>();
                        if (btnBg != null) btnBg.color = new Color(0.08f, 0.40f, 0.22f, 0.98f);
                        Outline btnOl = slotLoadButtons[slotIdx].GetComponent<Outline>();
                        if (btnOl != null) btnOl.effectColor = new Color(0.28f, 0.90f, 0.55f, 0.75f);
                    }

                    if (slotLoadBtnLabels[slotIdx] != null)
                    {
                        slotLoadBtnLabels[slotIdx].text = "<b>► LOAD SLOT</b>";
                        slotLoadBtnLabels[slotIdx].color = new Color(0.35f, 0.98f, 0.65f);
                    }

                    if (slotDeleteButtons[slotIdx] != null)
                    {
                        slotDeleteButtons[slotIdx].gameObject.SetActive(true);
                    }

                    if (slotCardBgs[slotIdx] != null)
                    {
                        slotCardBgs[slotIdx].color = new Color(0.06f, 0.09f, 0.14f, 0.96f);
                    }
                    if (slotCardOutlines[slotIdx] != null)
                    {
                        slotCardOutlines[slotIdx].effectColor = slotIdx == 0
                            ? new Color(0.22f, 0.74f, 0.97f, 0.65f)
                            : new Color(0.95f, 0.62f, 0.12f, 0.65f);
                    }
                }
                else
                {
                    if (slotTitleTexts[slotIdx] != null)
                    {
                        slotTitleTexts[slotIdx].text = $"<color=#64748B><b>□ {slotTitle}</b>   —   [ EMPTY ARCHIVE SLOT ]</color>";
                    }

                    if (slotDetailTexts[slotIdx] != null)
                    {
                        slotDetailTexts[slotIdx].text = slotIdx == 0
                            ? "<color=#475569><i>No QuickSave recorded yet. Press F5 during gameplay to QuickSave.</i></color>"
                            : "<color=#475569><i>No survivor record stored in this slot. Save via Personal Phone [TAB] in-game.</i></color>";
                    }

                    if (slotLoadButtons[slotIdx] != null)
                    {
                        slotLoadButtons[slotIdx].interactable = false;
                        Image btnBg = slotLoadButtons[slotIdx].GetComponent<Image>();
                        if (btnBg != null) btnBg.color = new Color(0.06f, 0.08f, 0.11f, 0.75f);
                        Outline btnOl = slotLoadButtons[slotIdx].GetComponent<Outline>();
                        if (btnOl != null) btnOl.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.35f);
                    }

                    if (slotLoadBtnLabels[slotIdx] != null)
                    {
                        slotLoadBtnLabels[slotIdx].text = "EMPTY";
                        slotLoadBtnLabels[slotIdx].color = new Color(0.35f, 0.40f, 0.46f);
                    }

                    if (slotDeleteButtons[slotIdx] != null)
                    {
                        slotDeleteButtons[slotIdx].gameObject.SetActive(false);
                    }

                    if (slotCardBgs[slotIdx] != null)
                    {
                        slotCardBgs[slotIdx].color = new Color(0.03f, 0.04f, 0.06f, 0.85f);
                    }
                    if (slotCardOutlines[slotIdx] != null)
                    {
                        slotCardOutlines[slotIdx].effectColor = new Color(0.16f, 0.20f, 0.26f, 0.45f);
                    }
                }
            }
        }

        private void ExecuteResumeSavedSlot(int slotIndex)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
                AudioManager.Instance.StopBGM(0.8f);
            }

            if (loadGameModal != null)
            {
                loadGameModal.SetActive(false);
            }

            bool loadSuccess = SaveManager.Instance != null && SaveManager.Instance.LoadFromSlot(slotIndex);

            if (loadSuccess)
            {
                Hide(0.6f, () =>
                {
                    if (GameManager.Instance != null)
                    {
                        var player = GameManager.Instance.CurrentPlayer;
                        string loc = player != null ? player.currentLocationId : "unit_407";
                        GameManager.Instance.ResumeFromSave(player, loc);
                    }
                });
            }
            else
            {
                Debug.LogWarning($"[TitleScreen] Failed to load save slot {slotIndex}.");
            }
        }

        private void OnDeleteSlotClicked(int slotIndex)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSlot(slotIndex);
            }

            RefreshLoadSlotCards();
        }

        private void EnsureLoadGameModalBuilt()
        {
            if (loadGameModal != null) return;

            // 1. Modal Root Object
            loadGameModal = new GameObject("LoadGameModal");
            loadGameModal.transform.SetParent(transform, false);
            RectTransform modalRect = loadGameModal.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            // Fullscreen Dark Backdrop (Blocks Raycasts)
            Image backdrop = loadGameModal.AddComponent<Image>();
            backdrop.color = new Color(0.015f, 0.02f, 0.03f, 0.92f);
            backdrop.raycastTarget = true;

            // 2. Central Dossier Panel (860 x 550)
            GameObject panel = new GameObject("DossierPanel");
            panel.transform.SetParent(loadGameModal.transform, false);
            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(860f, 550f);
            pRect.anchoredPosition = Vector2.zero;

            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0.04f, 0.06f, 0.09f, 0.98f);

            Outline pOutline = panel.AddComponent<Outline>();
            pOutline.effectColor = new Color(0.85f, 0.60f, 0.20f, 0.75f);
            pOutline.effectDistance = new Vector2(2f, -2f);

            // 3. Header Bar
            GameObject headerObj = new GameObject("HeaderBar");
            headerObj.transform.SetParent(panel.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0f, 52f);
            hRect.anchoredPosition = Vector2.zero;

            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.08f, 0.12f, 0.16f, 0.95f);

            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(headerObj.transform, false);
            RectTransform ttRect = titleTextObj.AddComponent<RectTransform>();
            ttRect.anchorMin = Vector2.zero;
            ttRect.anchorMax = Vector2.one;
            ttRect.offsetMin = new Vector2(20f, 0f);
            ttRect.offsetMax = new Vector2(-20f, 0f);

            modalTitleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            modalTitleText.text = "■ SELECT ARCHIVED SURVIVOR SLOT // CLASSIFIED DOSSIER";
            modalTitleText.fontSize = 16.5f;
            modalTitleText.fontStyle = FontStyles.Bold;
            modalTitleText.color = new Color(1f, 0.75f, 0.35f, 1f);
            modalTitleText.alignment = TextAlignmentOptions.MidlineLeft;
            SundayTypography.ApplyFont(modalTitleText, FontRole.Display);

            // 4. 4 Slot Cards Area
            for (int i = 0; i < SaveManager.TotalSlotCount; i++)
            {
                int slotIdx = i;
                GameObject cardObj = new GameObject($"SlotCard_{slotIdx}");
                cardObj.transform.SetParent(panel.transform, false);
                RectTransform cRect = cardObj.AddComponent<RectTransform>();
                cRect.anchorMin = new Vector2(0.5f, 1f);
                cRect.anchorMax = new Vector2(0.5f, 1f);
                cRect.pivot = new Vector2(0.5f, 1f);
                cRect.anchoredPosition = new Vector2(0f, -66f - (slotIdx * 98f));
                cRect.sizeDelta = new Vector2(816f, 88f);

                slotCardBgs[slotIdx] = cardObj.AddComponent<Image>();
                slotCardBgs[slotIdx].color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

                slotCardOutlines[slotIdx] = cardObj.AddComponent<Outline>();
                slotCardOutlines[slotIdx].effectColor = new Color(0.25f, 0.32f, 0.42f, 0.65f);
                slotCardOutlines[slotIdx].effectDistance = new Vector2(1.5f, -1.5f);

                // Dedicated Text Container with RectMask2D to strictly prevent text overflowing into buttons
                GameObject textContainer = new GameObject("TextContainer");
                textContainer.transform.SetParent(cardObj.transform, false);
                RectTransform tcRect = textContainer.AddComponent<RectTransform>();
                tcRect.anchorMin = Vector2.zero;
                tcRect.anchorMax = Vector2.one;
                tcRect.offsetMin = new Vector2(18f, 6f);
                tcRect.offsetMax = new Vector2(-205f, -6f); // Guaranteed safety margin from buttons
                textContainer.AddComponent<RectMask2D>();

                // Top Line: Slot Title + Survivor Name
                GameObject topObj = new GameObject("SlotTitle");
                topObj.transform.SetParent(textContainer.transform, false);
                RectTransform topRect = topObj.AddComponent<RectTransform>();
                topRect.anchorMin = new Vector2(0f, 0.5f);
                topRect.anchorMax = new Vector2(1f, 1f);
                topRect.offsetMin = new Vector2(0f, 2f);
                topRect.offsetMax = new Vector2(0f, -2f);

                slotTitleTexts[slotIdx] = topObj.AddComponent<TextMeshProUGUI>();
                slotTitleTexts[slotIdx].fontSize = 13.5f;
                slotTitleTexts[slotIdx].color = Color.white;
                slotTitleTexts[slotIdx].alignment = TextAlignmentOptions.MidlineLeft;
                slotTitleTexts[slotIdx].textWrappingMode = TextWrappingModes.NoWrap;
                slotTitleTexts[slotIdx].overflowMode = TextOverflowModes.Ellipsis;
                SundayTypography.ApplyFont(slotTitleTexts[slotIdx], FontRole.Body);

                // Bottom Line: Location, Clock, Vitals
                GameObject botObj = new GameObject("SlotDetails");
                botObj.transform.SetParent(textContainer.transform, false);
                RectTransform botRect = botObj.AddComponent<RectTransform>();
                botRect.anchorMin = new Vector2(0f, 0f);
                botRect.anchorMax = new Vector2(1f, 0.5f);
                botRect.offsetMin = new Vector2(0f, 2f);
                botRect.offsetMax = new Vector2(0f, -2f);

                slotDetailTexts[slotIdx] = botObj.AddComponent<TextMeshProUGUI>();
                slotDetailTexts[slotIdx].fontSize = 12f;
                slotDetailTexts[slotIdx].color = new Color(0.78f, 0.82f, 0.88f);
                slotDetailTexts[slotIdx].alignment = TextAlignmentOptions.MidlineLeft;
                slotDetailTexts[slotIdx].textWrappingMode = TextWrappingModes.NoWrap;
                slotDetailTexts[slotIdx].overflowMode = TextOverflowModes.Ellipsis;
                SundayTypography.ApplyFont(slotDetailTexts[slotIdx], FontRole.Body);

                // Load Slot Button (Right side)
                GameObject loadBtnObj = new GameObject("LoadButton");
                loadBtnObj.transform.SetParent(cardObj.transform, false);
                RectTransform lbRect = loadBtnObj.AddComponent<RectTransform>();
                lbRect.anchorMin = new Vector2(1f, 0.5f);
                lbRect.anchorMax = new Vector2(1f, 0.5f);
                lbRect.pivot = new Vector2(1f, 0.5f);
                lbRect.anchoredPosition = new Vector2(-56f, 0f);
                lbRect.sizeDelta = new Vector2(130f, 42f);

                Image lbBg = loadBtnObj.AddComponent<Image>();
                lbBg.color = new Color(0.08f, 0.40f, 0.22f, 0.98f);
                Outline lbOl = loadBtnObj.AddComponent<Outline>();
                lbOl.effectColor = new Color(0.28f, 0.90f, 0.55f, 0.75f);

                slotLoadButtons[slotIdx] = loadBtnObj.AddComponent<Button>();
                slotLoadButtons[slotIdx].targetGraphic = lbBg;
                slotLoadButtons[slotIdx].transition = Selectable.Transition.None;
                slotLoadButtons[slotIdx].onClick.AddListener(() => ExecuteResumeSavedSlot(slotIdx));

                GameObject lbTxtObj = new GameObject("Label");
                lbTxtObj.transform.SetParent(loadBtnObj.transform, false);
                RectTransform lbtRect = lbTxtObj.AddComponent<RectTransform>();
                lbtRect.anchorMin = Vector2.zero;
                lbtRect.anchorMax = Vector2.one;
                lbtRect.offsetMin = Vector2.zero;
                lbtRect.offsetMax = Vector2.zero;

                slotLoadBtnLabels[slotIdx] = lbTxtObj.AddComponent<TextMeshProUGUI>();
                slotLoadBtnLabels[slotIdx].text = "<b>► LOAD SLOT</b>";
                slotLoadBtnLabels[slotIdx].fontSize = 12.5f;
                slotLoadBtnLabels[slotIdx].alignment = TextAlignmentOptions.Center;
                slotLoadBtnLabels[slotIdx].color = new Color(0.35f, 0.98f, 0.65f);
                SundayTypography.ApplyFont(slotLoadBtnLabels[slotIdx], FontRole.Mono);

                // Delete Slot Button [DEL]
                GameObject delBtnObj = new GameObject("DeleteButton");
                delBtnObj.transform.SetParent(cardObj.transform, false);
                RectTransform dbRect = delBtnObj.AddComponent<RectTransform>();
                dbRect.anchorMin = new Vector2(1f, 0.5f);
                dbRect.anchorMax = new Vector2(1f, 0.5f);
                dbRect.pivot = new Vector2(1f, 0.5f);
                dbRect.anchoredPosition = new Vector2(-12f, 0f);
                dbRect.sizeDelta = new Vector2(36f, 42f);

                Image dbBg = delBtnObj.AddComponent<Image>();
                dbBg.color = new Color(0.26f, 0.07f, 0.08f, 0.95f);
                Outline dbOl = delBtnObj.AddComponent<Outline>();
                dbOl.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.65f);

                slotDeleteButtons[slotIdx] = delBtnObj.AddComponent<Button>();
                slotDeleteButtons[slotIdx].targetGraphic = dbBg;
                slotDeleteButtons[slotIdx].transition = Selectable.Transition.None;
                slotDeleteButtons[slotIdx].onClick.AddListener(() => OnDeleteSlotClicked(slotIdx));

                GameObject dbTxtObj = new GameObject("Label");
                dbTxtObj.transform.SetParent(delBtnObj.transform, false);
                RectTransform dbtRect = dbTxtObj.AddComponent<RectTransform>();
                dbtRect.anchorMin = Vector2.zero;
                dbtRect.anchorMax = Vector2.one;
                dbtRect.offsetMin = Vector2.zero;
                dbtRect.offsetMax = Vector2.zero;

                TextMeshProUGUI dbTmp = dbTxtObj.AddComponent<TextMeshProUGUI>();
                dbTmp.text = "<b>X</b>";
                dbTmp.fontSize = 12.5f;
                dbTmp.alignment = TextAlignmentOptions.Center;
                dbTmp.color = new Color(0.98f, 0.45f, 0.45f);
                SundayTypography.ApplyFont(dbTmp, FontRole.Mono);
            }

            // 5. Bottom Return Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(panel.transform, false);
            RectTransform cnRect = cancelObj.AddComponent<RectTransform>();
            cnRect.anchorMin = new Vector2(0.5f, 0f);
            cnRect.anchorMax = new Vector2(0.5f, 0f);
            cnRect.pivot = new Vector2(0.5f, 0f);
            cnRect.sizeDelta = new Vector2(260f, 42f);
            cnRect.anchoredPosition = new Vector2(0f, 16f);

            Image cnBg = cancelObj.AddComponent<Image>();
            cnBg.color = new Color(0.12f, 0.15f, 0.20f, 0.98f);
            Outline cnOl = cancelObj.AddComponent<Outline>();
            cnOl.effectColor = new Color(0.35f, 0.42f, 0.52f, 0.7f);

            Button cancelBtn = cancelObj.AddComponent<Button>();
            cancelBtn.targetGraphic = cnBg;
            cancelBtn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                if (loadGameModal != null) loadGameModal.SetActive(false);
            });

            GameObject cnTxtObj = new GameObject("Text");
            cnTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform cntRect = cnTxtObj.AddComponent<RectTransform>();
            cntRect.anchorMin = Vector2.zero;
            cntRect.anchorMax = Vector2.one;
            cntRect.offsetMin = Vector2.zero;
            cntRect.offsetMax = Vector2.zero;
            TextMeshProUGUI cnTmp = cnTxtObj.AddComponent<TextMeshProUGUI>();
            cnTmp.text = "<b>[ < RETURN TO MAIN MENU ]</b>";
            cnTmp.fontSize = 14f;
            cnTmp.alignment = TextAlignmentOptions.Center;
            cnTmp.color = new Color(0.85f, 0.88f, 0.92f, 1f);
            SundayTypography.ApplyFont(cnTmp, FontRole.Mono);

            loadGameModal.SetActive(false);
        }

        private void OnSettingsClicked()
        {
            if (settingsModal != null)
            {
                settingsModal.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[TitleScreen] Quitting game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

