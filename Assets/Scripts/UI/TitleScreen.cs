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

        // Dynamic modal references
        private TextMeshProUGUI modalTitleText;
        private TextMeshProUGUI modalBodyText;
        private Button modalResumeButton;
        private Button modalCancelButton;

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
            if (pressF9 && SaveManager.Instance != null && SaveManager.Instance.HasSave())
            {
                ExecuteResumeSavedGame();
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

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave();

            if (hasSave)
            {
                var summary = SaveManager.Instance.GetSaveSummary();

                if (modalTitleText != null)
                {
                    modalTitleText.text = "■ RECOVER SURVIVOR RECORD // ARCHIVED DOSSIER";
                }

                if (modalBodyText != null)
                {
                    string survivorName = !string.IsNullOrEmpty(summary.survivorName) ? summary.survivorName.ToUpper() : "UNKNOWN SURVIVOR";
                    string vocation = !string.IsNullOrEmpty(summary.job) ? summary.job.ToUpper() : "CIVILIAN";
                    string location = !string.IsNullOrEmpty(summary.roomId) ? summary.roomId.ToUpper() : "UNIT 407";

                    modalBodyText.text =
                        $"<color=#94A3B8>FILE:</color> <color=#38BDF8>QuickSave.es3</color>\n\n" +
                        $"<color=#94A3B8>SURVIVOR:</color> <color=#FFA726><b>{survivorName}</b></color>\n" +
                        $"<color=#94A3B8>BACKGROUND:</color> <b>{vocation}</b>\n\n" +
                        $"<color=#94A3B8>VITALS:</color> HP: <color=#EF4444><b>{summary.hp}/{summary.hpMax}</b></color>  |  " +
                        $"STAMINA: <color=#10B981><b>{summary.stamina}/{summary.staminaMax}</b></color>  |  " +
                        $"HUMANITY: <color=#8B5CF6><b>{summary.humanity}/100</b></color>\n\n" +
                        $"<color=#94A3B8>LAST RECORDED POSITION:</color> <color=#FBBF24><b>{location}</b></color>\n" +
                        $"<color=#94A3B8>TIMESTAMP:</color> <b>{summary.dateString}</b>\n\n" +
                        $"<color=#64748B>Telemetry confirms biological signals are intact. Ready to synchronize neural link.</color>";
                }

                if (modalResumeButton != null)
                {
                    modalResumeButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (modalTitleText != null)
                {
                    modalTitleText.text = "■ SURVIVOR ARCHIVE // RECORD EMPTY";
                }

                if (modalBodyText != null)
                {
                    modalBodyText.text =
                        $"<color=#EF4444><b>[!] NO ARCHIVED SURVIVOR DOSSIER FOUND</b></color>\n\n" +
                        $"No saved intake records exist in the current storage terminal.\n\n" +
                        $"Please return to the main menu and select <b>NEW GAME</b> to initiate your first citizen intake and begin surviving Day Zero.\n\n" +
                        $"<color=#64748B><i>(During gameplay, press <b>F5</b> at any time to QuickSave your progress.)</i></color>";
                }

                if (modalResumeButton != null)
                {
                    modalResumeButton.gameObject.SetActive(false);
                }
            }

            if (loadGameModal != null)
            {
                loadGameModal.SetActive(true);
            }
        }

        private void ExecuteResumeSavedGame()
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

            bool loadSuccess = SaveManager.Instance != null && SaveManager.Instance.QuickLoad();

            if (loadSuccess)
            {
                Hide(0.6f, () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.GoToApartmentHub();
                    }
                });
            }
            else
            {
                Debug.LogWarning("[TitleScreen] Failed to load save file.");
            }
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

            // 2. Central Dossier Panel
            GameObject panel = new GameObject("DossierPanel");
            panel.transform.SetParent(loadGameModal.transform, false);
            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(720, 520);
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
            hRect.sizeDelta = new Vector2(0, 52);
            hRect.anchoredPosition = Vector2.zero;

            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.08f, 0.12f, 0.16f, 0.95f);

            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(headerObj.transform, false);
            RectTransform ttRect = titleTextObj.AddComponent<RectTransform>();
            ttRect.anchorMin = Vector2.zero;
            ttRect.anchorMax = Vector2.one;
            ttRect.offsetMin = new Vector2(20, 0);
            ttRect.offsetMax = new Vector2(-20, 0);

            modalTitleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            modalTitleText.text = "■ RECOVER SURVIVOR RECORD // CLASSIFIED ARCHIVE";
            modalTitleText.fontSize = 18;
            modalTitleText.fontStyle = FontStyles.Bold;
            modalTitleText.color = new Color(1f, 0.75f, 0.35f, 1f);
            modalTitleText.alignment = TextAlignmentOptions.MidlineLeft;
            SundayTypography.ApplyFont(modalTitleText, FontRole.Display);

            // 4. Dossier Content Area
            GameObject contentObj = new GameObject("ContentArea");
            contentObj.transform.SetParent(panel.transform, false);
            RectTransform cRect = contentObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 0f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.offsetMin = new Vector2(25, 75);
            cRect.offsetMax = new Vector2(-25, -60);

            Image cBg = contentObj.AddComponent<Image>();
            cBg.color = new Color(0.02f, 0.03f, 0.05f, 0.85f);

            Outline cOutline = contentObj.AddComponent<Outline>();
            cOutline.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.5f);
            cOutline.effectDistance = new Vector2(1f, -1f);

            GameObject bodyTextObj = new GameObject("BodyText");
            bodyTextObj.transform.SetParent(contentObj.transform, false);
            RectTransform btRect = bodyTextObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = new Vector2(20, 15);
            btRect.offsetMax = new Vector2(-20, -15);

            modalBodyText = bodyTextObj.AddComponent<TextMeshProUGUI>();
            modalBodyText.fontSize = 15;
            modalBodyText.lineSpacing = 8;
            modalBodyText.color = new Color(0.85f, 0.88f, 0.92f, 1f);
            modalBodyText.alignment = TextAlignmentOptions.TopLeft;
            SundayTypography.ApplyFont(modalBodyText, FontRole.Body);

            // 5. Button Row
            GameObject btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(panel.transform, false);
            RectTransform brRect = btnRow.AddComponent<RectTransform>();
            brRect.anchorMin = new Vector2(0f, 0f);
            brRect.anchorMax = new Vector2(1f, 0f);
            brRect.sizeDelta = new Vector2(0, 60);
            brRect.anchoredPosition = new Vector2(0, 12);

            // Cancel Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(btnRow.transform, false);
            RectTransform cnRect = cancelObj.AddComponent<RectTransform>();
            cnRect.anchorMin = new Vector2(0.5f, 0.5f);
            cnRect.anchorMax = new Vector2(0.5f, 0.5f);
            cnRect.sizeDelta = new Vector2(180, 42);
            cnRect.anchoredPosition = new Vector2(-120, 0);

            Image cnBg = cancelObj.AddComponent<Image>();
            cnBg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);
            modalCancelButton = cancelObj.AddComponent<Button>();
            modalCancelButton.targetGraphic = cnBg;
            modalCancelButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                if (loadGameModal != null) loadGameModal.SetActive(false);
            });

            GameObject cnTxtObj = new GameObject("Text");
            cnTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform cntRect = cnTxtObj.AddComponent<RectTransform>();
            cntRect.anchorMin = Vector2.zero;
            cntRect.anchorMax = Vector2.one;
            TextMeshProUGUI cnTmp = cnTxtObj.AddComponent<TextMeshProUGUI>();
            cnTmp.text = "[ RETURN ]";
            cnTmp.fontSize = 15;
            cnTmp.fontStyle = FontStyles.Bold;
            cnTmp.alignment = TextAlignmentOptions.Center;
            cnTmp.color = new Color(0.75f, 0.80f, 0.85f, 1f);
            SundayTypography.ApplyFont(cnTmp, FontRole.Mono);

            // Resume Button
            GameObject resumeObj = new GameObject("ResumeButton");
            resumeObj.transform.SetParent(btnRow.transform, false);
            RectTransform rmRect = resumeObj.AddComponent<RectTransform>();
            rmRect.anchorMin = new Vector2(0.5f, 0.5f);
            rmRect.anchorMax = new Vector2(0.5f, 0.5f);
            rmRect.sizeDelta = new Vector2(260, 42);
            rmRect.anchoredPosition = new Vector2(110, 0);

            Image rmBg = resumeObj.AddComponent<Image>();
            rmBg.color = new Color(0.10f, 0.45f, 0.25f, 0.95f); // Terminal green
            modalResumeButton = resumeObj.AddComponent<Button>();
            modalResumeButton.targetGraphic = rmBg;
            modalResumeButton.onClick.AddListener(ExecuteResumeSavedGame);

            GameObject rmTxtObj = new GameObject("Text");
            rmTxtObj.transform.SetParent(resumeObj.transform, false);
            RectTransform rmtRect = rmTxtObj.AddComponent<RectTransform>();
            rmtRect.anchorMin = Vector2.zero;
            rmtRect.anchorMax = Vector2.one;
            TextMeshProUGUI rmTmp = rmTxtObj.AddComponent<TextMeshProUGUI>();
            rmTmp.text = "► RESUME SURVIVOR";
            rmTmp.fontSize = 15;
            rmTmp.fontStyle = FontStyles.Bold;
            rmTmp.alignment = TextAlignmentOptions.Center;
            rmTmp.color = Color.white;
            SundayTypography.ApplyFont(rmTmp, FontRole.Display);

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

