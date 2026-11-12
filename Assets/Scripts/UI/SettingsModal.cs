using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    public class SettingsModal : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Display Settings")]
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Controls")]
        [SerializeField] private Button closeButton;

        private TextMeshProUGUI masterPctText;
        private TextMeshProUGUI bgmPctText;
        private TextMeshProUGUI sfxPctText;
        private Button displayModeButton;
        private TextMeshProUGUI displayModeBtnLabel;
        private Image displayModeBtnBg;
        private Outline displayModeBtnOutline;
        private bool isRuntimeBuilt;

        private void Awake()
        {
            EnsureSettingsContentBuilt();
            BindListeners();
        }

        private void OnEnable()
        {
            EnsureSettingsContentBuilt();
            RefreshUIValues();
        }

        private void Update()
        {
            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif
            if (escPressed)
            {
                Close();
            }
        }

        private void BindListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
                bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            }
        }

        private void RefreshUIValues()
        {
            float master = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : AudioListener.volume;
            float bgm = AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 1f;
            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1f;

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgm);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);

            UpdatePercentageLabels(master, bgm, sfx);

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            }

            RefreshDisplayModeButton(Screen.fullScreen);
        }

        private void UpdatePercentageLabels(float master, float bgm, float sfx)
        {
            if (masterPctText != null) masterPctText.text = $"{Mathf.RoundToInt(master * 100f)}%";
            if (bgmPctText != null) bgmPctText.text = $"{Mathf.RoundToInt(bgm * 100f)}%";
            if (sfxPctText != null) sfxPctText.text = $"{Mathf.RoundToInt(sfx * 100f)}%";
        }

        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);
            UpdatePercentageLabels(
                value,
                bgmSlider != null ? bgmSlider.value : 1f,
                sfxSlider != null ? sfxSlider.value : 1f);
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value);
            UpdatePercentageLabels(
                masterSlider != null ? masterSlider.value : 1f,
                value,
                sfxSlider != null ? sfxSlider.value : 1f);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
            UpdatePercentageLabels(
                masterSlider != null ? masterSlider.value : 1f,
                bgmSlider != null ? bgmSlider.value : 1f,
                value);
        }

        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.fullScreen = isFullscreen;
            RefreshDisplayModeButton(isFullscreen);
        }

        private void OnDisplayModeButtonClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            bool nextFullscreen = !Screen.fullScreen;
            OnFullscreenToggled(nextFullscreen);
        }

        private void RefreshDisplayModeButton(bool isFullscreen)
        {
            if (displayModeBtnLabel != null)
            {
                displayModeBtnLabel.text = isFullscreen
                    ? "<b>[ FULLSCREEN : ON ]</b>"
                    : "<b>[ WINDOWED MODE ]</b>";
                displayModeBtnLabel.color = isFullscreen
                    ? new Color(0.28f, 0.95f, 0.58f, 1f)
                    : new Color(0.95f, 0.72f, 0.22f, 1f);
            }
            if (displayModeBtnBg != null)
            {
                displayModeBtnBg.color = isFullscreen
                    ? new Color(0.02f, 0.08f, 0.05f, 0.96f)
                    : new Color(0.04f, 0.05f, 0.08f, 0.96f);
            }
            if (displayModeBtnOutline != null)
            {
                displayModeBtnOutline.effectColor = isFullscreen
                    ? new Color(0.22f, 0.85f, 0.50f, 0.70f)
                    : new Color(0.85f, 0.58f, 0.14f, 0.70f);
            }
        }

        private void OnResetDefaultsClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(1f);
                AudioManager.Instance.SetBGMVolume(1f);
                AudioManager.Instance.SetSFXVolume(1f);
                AudioManager.Instance.PlayButtonClick();
            }
            else
            {
                AudioListener.volume = 1f;
            }
            RefreshUIValues();
        }

        public void Close()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            gameObject.SetActive(false);
        }

        private void EnsureSettingsContentBuilt()
        {
            if (isRuntimeBuilt && masterSlider != null && bgmSlider != null && sfxSlider != null) return;
            isRuntimeBuilt = true;

            Image backdrop = GetComponent<Image>();
            if (backdrop != null)
            {
                backdrop.color = new Color(0.005f, 0.008f, 0.015f, 0.92f);
                backdrop.raycastTarget = true;
            }

            Transform panelTrans = transform.Find("Panel");
            GameObject panelObj;
            if (panelTrans == null)
            {
                panelObj = new GameObject("Panel", typeof(RectTransform), typeof(Image));
                panelObj.transform.SetParent(transform, false);
                panelTrans = panelObj.transform;
            }
            else
            {
                panelObj = panelTrans.gameObject;
            }

            RectTransform pRect = panelObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(680f, 500f);
            pRect.anchoredPosition = Vector2.zero;

            Image pBg = panelObj.GetComponent<Image>();
            if (pBg != null)
            {
                pBg.color = new Color(0.012f, 0.018f, 0.028f, 0.98f);
            }

            Outline pOutline = panelObj.GetComponent<Outline>();
            if (pOutline == null) pOutline = panelObj.AddComponent<Outline>();
            pOutline.effectColor = new Color(0.85f, 0.55f, 0.12f, 0.78f);
            pOutline.effectDistance = new Vector2(2f, -2f);

            // Restyle or create Header Bar
            Transform existingTitle = panelTrans.Find("Title");
            if (existingTitle != null)
            {
                existingTitle.gameObject.SetActive(false);
            }

            Transform headerTrans = panelTrans.Find("HeaderBar");
            if (headerTrans == null)
            {
                GameObject headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
                headerObj.transform.SetParent(panelTrans, false);
                RectTransform hRect = headerObj.GetComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0f, 1f);
                hRect.anchorMax = new Vector2(1f, 1f);
                hRect.pivot = new Vector2(0.5f, 1f);
                hRect.sizeDelta = new Vector2(0f, 52f);
                hRect.anchoredPosition = Vector2.zero;

                Image hBg = headerObj.GetComponent<Image>();
                hBg.color = new Color(0.03f, 0.045f, 0.07f, 0.98f);

                TextMeshProUGUI hTitle = CreateLabel(headerObj.transform, "HeaderTitle",
                    "SYSTEM SETTINGS // AUDIO & DISPLAY CONFIGURATION",
                    15.5f, FontStyles.Bold, new Color(0.95f, 0.65f, 0.15f, 1f), TextAlignmentOptions.MidlineLeft, FontRole.Display);
                RectTransform htRect = hTitle.rectTransform;
                htRect.anchorMin = Vector2.zero;
                htRect.anchorMax = Vector2.one;
                htRect.offsetMin = new Vector2(22f, 0f);
                htRect.offsetMax = new Vector2(-100f, 0f);

                Button escBtn = CreateActionButton(headerObj.transform, "EscCloseBtn", "<b>[ ESC ]</b>",
                    new Color(0.05f, 0.07f, 0.10f, 1f), new Color(0.82f, 0.86f, 0.92f, 1f), out _);
                RectTransform ebRect = escBtn.GetComponent<RectTransform>();
                ebRect.anchorMin = new Vector2(1f, 0.5f);
                ebRect.anchorMax = new Vector2(1f, 0.5f);
                ebRect.pivot = new Vector2(1f, 0.5f);
                ebRect.sizeDelta = new Vector2(72f, 30f);
                ebRect.anchoredPosition = new Vector2(-14f, 0f);
                escBtn.onClick.AddListener(Close);
            }

            // Build Audio & Display Rows if not already built
            if (masterSlider == null)
            {
                masterSlider = CreateVolumeSliderRow(panelTrans, "MasterVolumeRow",
                    "MASTER VOLUME", "Global output level for all game audio", -72f, out masterPctText);
            }
            if (bgmSlider == null)
            {
                bgmSlider = CreateVolumeSliderRow(panelTrans, "BgmVolumeRow",
                    "BGM & AMBIENCE", "Main menu theme, background music & atmospheric drones", -156f, out bgmPctText);
            }
            if (sfxSlider == null)
            {
                sfxSlider = CreateVolumeSliderRow(panelTrans, "SfxVolumeRow",
                    "SFX & BROADCASTS", "UI clicks, emergency broadcasts, radio & alarms", -240f, out sfxPctText);
            }

            // Display Mode Row (Fullscreen / Windowed Toggle)
            if (displayModeButton == null)
            {
                CreateDisplayModeRow(panelTrans, -324f);
            }

            // Footer Buttons: Reset Defaults + Save & Close
            Transform resetTrans = panelTrans.Find("ResetDefaultsButton");
            if (resetTrans == null)
            {
                Button resetBtn = CreateActionButton(panelTrans, "ResetDefaultsButton", "<b>[ RESET AUDIO DEFAULTS ]</b>",
                    new Color(0.03f, 0.045f, 0.07f, 0.98f), new Color(0.75f, 0.82f, 0.90f, 1f), out _);
                RectTransform rbRect = resetBtn.GetComponent<RectTransform>();
                rbRect.anchorMin = new Vector2(0.5f, 0f);
                rbRect.anchorMax = new Vector2(0.5f, 0f);
                rbRect.pivot = new Vector2(1f, 0f);
                rbRect.sizeDelta = new Vector2(296f, 44f);
                rbRect.anchoredPosition = new Vector2(-10f, 22f);
                resetBtn.onClick.AddListener(OnResetDefaultsClicked);
            }

            if (closeButton == null)
            {
                Transform cbTrans = panelTrans.Find("CloseButton");
                if (cbTrans != null) closeButton = cbTrans.GetComponent<Button>();
            }

            if (closeButton != null)
            {
                RectTransform cbRect = closeButton.GetComponent<RectTransform>();
                cbRect.anchorMin = new Vector2(0.5f, 0f);
                cbRect.anchorMax = new Vector2(0.5f, 0f);
                cbRect.pivot = new Vector2(0f, 0f);
                cbRect.sizeDelta = new Vector2(296f, 44f);
                cbRect.anchoredPosition = new Vector2(10f, 22f);

                closeButton.transition = Selectable.Transition.None;
                Image cbBg = closeButton.GetComponent<Image>();
                if (cbBg != null)
                {
                    cbBg.color = new Color(0.08f, 0.05f, 0.01f, 0.98f);
                    if (cbBg.canvasRenderer != null) cbBg.canvasRenderer.SetColor(Color.white);
                }

                Outline cbOl = closeButton.GetComponent<Outline>();
                if (cbOl == null) cbOl = closeButton.gameObject.AddComponent<Outline>();
                cbOl.effectColor = new Color(0.95f, 0.65f, 0.15f, 0.80f);
                cbOl.effectDistance = new Vector2(1.5f, -1.5f);

                TextMeshProUGUI cbTmp = closeButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (cbTmp != null)
                {
                    cbTmp.text = "<b>[ < APPLY & CLOSE ]</b>";
                    cbTmp.fontSize = 13.5f;
                    cbTmp.color = new Color(0.95f, 0.72f, 0.22f, 1f);
                    SundayTypography.ApplyFont(cbTmp, FontRole.Mono);
                }
            }
        }

        private Slider CreateVolumeSliderRow(Transform parent, string name, string title, string subtitle, float topOffset, out TextMeshProUGUI pctLabel)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            row.transform.SetParent(parent, false);
            RectTransform rr = row.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 1f);
            rr.anchorMax = new Vector2(0.5f, 1f);
            rr.pivot = new Vector2(0.5f, 1f);
            rr.anchoredPosition = new Vector2(0f, topOffset);
            rr.sizeDelta = new Vector2(612f, 72f);

            Image rowBg = row.GetComponent<Image>();
            rowBg.color = new Color(0.025f, 0.035f, 0.055f, 0.96f);

            Outline rowOl = row.GetComponent<Outline>();
            rowOl.effectColor = new Color(0.20f, 0.26f, 0.34f, 0.65f);
            rowOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI titleTmp = CreateLabel(row.transform, "Title",
                $"<b>{title}</b>   <size=11><color=#64748B>{subtitle}</color></size>",
                13f, FontStyles.Normal, new Color(0.94f, 0.96f, 0.98f, 1f), TextAlignmentOptions.MidlineLeft, FontRole.Body);
            RectTransform tr = titleTmp.rectTransform;
            tr.anchorMin = new Vector2(0f, 0.5f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(16f, 2f);
            tr.offsetMax = new Vector2(-80f, -4f);

            pctLabel = CreateLabel(row.transform, "PctValue", "100%",
                13.5f, FontStyles.Bold, new Color(0.95f, 0.72f, 0.22f, 1f), TextAlignmentOptions.MidlineRight, FontRole.Mono);
            RectTransform pr = pctLabel.rectTransform;
            pr.anchorMin = new Vector2(1f, 0.5f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.offsetMin = new Vector2(-80f, 2f);
            pr.offsetMax = new Vector2(-16f, -4f);

            // Interactive Slider
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
            sliderObj.transform.SetParent(row.transform, false);
            RectTransform sr = sliderObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(1f, 0.5f);
            sr.offsetMin = new Vector2(16f, 10f);
            sr.offsetMax = new Vector2(-16f, -4f);

            GameObject bgTrack = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgTrack.transform.SetParent(sliderObj.transform, false);
            RectTransform bgr = bgTrack.GetComponent<RectTransform>();
            bgr.anchorMin = new Vector2(0f, 0.25f);
            bgr.anchorMax = new Vector2(1f, 0.75f);
            bgr.offsetMin = Vector2.zero;
            bgr.offsetMax = Vector2.zero;
            Image bgImg = bgTrack.GetComponent<Image>();
            bgImg.color = new Color(0.005f, 0.008f, 0.014f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform far = fillArea.GetComponent<RectTransform>();
            far.anchorMin = new Vector2(0f, 0.25f);
            far.anchorMax = new Vector2(1f, 0.75f);
            far.offsetMin = Vector2.zero;
            far.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = Vector2.zero;
            Image fillImg = fill.GetComponent<Image>();
            fillImg.color = new Color(0.88f, 0.55f, 0.08f, 0.96f);

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform har = handleArea.GetComponent<RectTransform>();
            har.anchorMin = Vector2.zero;
            har.anchorMax = Vector2.one;
            har.offsetMin = new Vector2(6f, 0f);
            har.offsetMax = new Vector2(-6f, 0f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform hr = handle.GetComponent<RectTransform>();
            hr.sizeDelta = new Vector2(14f, 22f);
            Image handleImg = handle.GetComponent<Image>();
            handleImg.color = Color.white;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fr;
            slider.handleRect = hr;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private void CreateDisplayModeRow(Transform parent, float topOffset)
        {
            GameObject row = new GameObject("DisplayModeRow", typeof(RectTransform), typeof(Image), typeof(Outline));
            row.transform.SetParent(parent, false);
            RectTransform rr = row.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 1f);
            rr.anchorMax = new Vector2(0.5f, 1f);
            rr.pivot = new Vector2(0.5f, 1f);
            rr.anchoredPosition = new Vector2(0f, topOffset);
            rr.sizeDelta = new Vector2(612f, 64f);

            Image rowBg = row.GetComponent<Image>();
            rowBg.color = new Color(0.025f, 0.035f, 0.055f, 0.96f);

            Outline rowOl = row.GetComponent<Outline>();
            rowOl.effectColor = new Color(0.20f, 0.26f, 0.34f, 0.65f);
            rowOl.effectDistance = new Vector2(1f, -1f);

            TextMeshProUGUI titleTmp = CreateLabel(row.transform, "Title",
                "<b>DISPLAY MODE</b>   <size=11><color=#64748B>Switch between borderless fullscreen and windowed display</color></size>",
                13f, FontStyles.Normal, new Color(0.94f, 0.96f, 0.98f, 1f), TextAlignmentOptions.MidlineLeft, FontRole.Body);
            RectTransform tr = titleTmp.rectTransform;
            tr.anchorMin = new Vector2(0f, 0f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(16f, 0f);
            tr.offsetMax = new Vector2(-210f, 0f);

            displayModeButton = CreateActionButton(row.transform, "ToggleFullscreenBtn", "<b>[ FULLSCREEN : ON ]</b>",
                new Color(0.02f, 0.08f, 0.05f, 0.96f), new Color(0.28f, 0.95f, 0.58f, 1f), out displayModeBtnLabel);
            displayModeBtnBg = displayModeButton.GetComponent<Image>();
            displayModeBtnOutline = displayModeButton.GetComponent<Outline>();

            RectTransform br = displayModeButton.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(1f, 0.5f);
            br.anchorMax = new Vector2(1f, 0.5f);
            br.pivot = new Vector2(1f, 0.5f);
            br.sizeDelta = new Vector2(184f, 38f);
            br.anchoredPosition = new Vector2(-14f, 0f);

            displayModeButton.onClick.AddListener(OnDisplayModeButtonClicked);
        }

        private static Button CreateActionButton(Transform parent, string name, string text, Color bgCol, Color accentCol, out TextMeshProUGUI labelOut)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            Image bg = btnObj.GetComponent<Image>();
            bg.color = bgCol;

            Outline ol = btnObj.GetComponent<Outline>();
            ol.effectColor = new Color(accentCol.r, accentCol.g, accentCol.b, 0.70f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;

            labelOut = CreateLabel(btnObj.transform, "Label", text, 12.5f, FontStyles.Normal, accentCol, TextAlignmentOptions.Center, FontRole.Mono);
            RectTransform lr = labelOut.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            return btn;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions align, FontRole role)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            SundayTypography.ApplyFont(tmp, role);
            return tmp;
        }
    }
}

