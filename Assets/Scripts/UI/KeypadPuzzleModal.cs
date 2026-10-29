using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Interactive retro electronic lock keypad puzzle modal (inspired by classic survival horror and Resident Evil/Fallout).
    /// Features:
    /// - 4-digit code entry with tactile feedback
    /// - Physical wear / smudged enamel visual clue on keys 1, 4, and 8 (matching the reference photo)
    /// - 3 attempts limit before security lockout failsafe engages
    /// - Full keyboard input (0-9, Backspace, Enter, Esc) and on-screen button controls
    /// </summary>
    public class KeypadPuzzleModal : MonoBehaviour
    {
        [Header("State")]
        private string enteredCode = "";
        private const int MAX_DIGITS = 4;
        private int remainingAttempts = 3;
        private bool isLockedOut = false;
        private bool isUnlocked = false;
        private bool isProcessing = false;

        private Action onUnlockedCallback;
        private Action onLockedOutCallback;
        private Action onCancelledCallback;

        [Header("Valid Passcodes")]
        private readonly string[] validCodes = new string[] { "8465", "8445", "4181", "1481" };

        [Header("UI References")]
        private GameObject modalContainer;
        private TextMeshProUGUI lcdDisplayText;
        private TextMeshProUGUI lcdStatusText;
        private TextMeshProUGUI attemptsText;
        private TextMeshProUGUI observationClueText;
        private List<Button> keypadButtons = new List<Button>();
        private AudioSource audioSource;

        private void Awake()
        {
            EnsureAudioSource();
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }
            }
        }

        public void OpenKeypad(Action onUnlocked, Action onLockedOut = null, Action onCancelled = null)
        {
            EnsureAudioSource();
            onUnlockedCallback = onUnlocked;
            onLockedOutCallback = onLockedOut;
            onCancelledCallback = onCancelled;

            enteredCode = "";
            remainingAttempts = 3;
            isLockedOut = false;
            isUnlocked = false;
            isProcessing = false;

            BuildOrRefreshUI();
            gameObject.SetActive(true);
            UpdateDisplay();
            PlayBeepSound(1200f, 0.05f);
        }

        public void CloseKeypad()
        {
            gameObject.SetActive(false);
            onCancelledCallback?.Invoke();
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || isProcessing || isLockedOut || isUnlocked) return;

            // 1. New Input System (UnityEngine.InputSystem)
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.digit0Key.wasPressedThisFrame || kb.numpad0Key.wasPressedThisFrame) { OnDigitClicked("0"); return; }
                if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) { OnDigitClicked("1"); return; }
                if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) { OnDigitClicked("2"); return; }
                if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) { OnDigitClicked("3"); return; }
                if (kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame) { OnDigitClicked("4"); return; }
                if (kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame) { OnDigitClicked("5"); return; }
                if (kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame) { OnDigitClicked("6"); return; }
                if (kb.digit7Key.wasPressedThisFrame || kb.numpad7Key.wasPressedThisFrame) { OnDigitClicked("7"); return; }
                if (kb.digit8Key.wasPressedThisFrame || kb.numpad8Key.wasPressedThisFrame) { OnDigitClicked("8"); return; }
                if (kb.digit9Key.wasPressedThisFrame || kb.numpad9Key.wasPressedThisFrame) { OnDigitClicked("9"); return; }

                if (kb.backspaceKey.wasPressedThisFrame || kb.deleteKey.wasPressedThisFrame) { OnClearClicked(); return; }
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) { OnSubmitClicked(); return; }
                if (kb.escapeKey.wasPressedThisFrame) { CloseKeypad(); return; }
                return;
            }

            // 2. Fallback to Legacy Input System if active (wrapped safely in try-catch)
            try
            {
                for (int i = 0; i <= 9; i++)
                {
                    KeyCode alphaKey = KeyCode.Alpha0 + i;
                    KeyCode numKey = KeyCode.Keypad0 + i;
                    if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(numKey))
                    {
                        OnDigitClicked(i.ToString());
                        return;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    OnClearClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnSubmitClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseKeypad();
                }
            }
            catch (System.InvalidOperationException)
            {
                // Ignored: Input System package is active in Project Settings
            }
        }

        public void OnDigitClicked(string digit)
        {
            if (isProcessing || isLockedOut || isUnlocked) return;
            if (enteredCode.Length >= MAX_DIGITS)
            {
                PlayBeepSound(400f, 0.08f);
                return;
            }

            enteredCode += digit;
            PlayBeepSound(900f + (digit[0] - '0') * 50f, 0.06f);
            UpdateDisplay();

            // Auto-submit if 4 digits entered
            if (enteredCode.Length == MAX_DIGITS)
            {
                StartCoroutine(AutoSubmitDelay());
            }
        }

        private IEnumerator AutoSubmitDelay()
        {
            yield return new WaitForSecondsRealtime(0.18f);
            if (enteredCode.Length == MAX_DIGITS && !isProcessing)
            {
                OnSubmitClicked();
            }
        }

        public void OnClearClicked()
        {
            if (isProcessing || isLockedOut || isUnlocked) return;
            if (enteredCode.Length > 0)
            {
                enteredCode = enteredCode.Substring(0, enteredCode.Length - 1);
                PlayBeepSound(650f, 0.05f);
                UpdateDisplay();
            }
        }

        public void OnSubmitClicked()
        {
            if (isProcessing || isLockedOut || isUnlocked) return;
            if (string.IsNullOrEmpty(enteredCode)) return;

            StartCoroutine(EvaluateCodeCoroutine());
        }

        private IEnumerator EvaluateCodeCoroutine()
        {
            isProcessing = true;
            if (lcdStatusText != null)
                lcdStatusText.text = "<color=#FBBF24>VERIFYING HASH...</color>";

            yield return new WaitForSecondsRealtime(0.35f);

            bool isMatch = false;
            foreach (var code in validCodes)
            {
                if (string.Equals(enteredCode, code, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                    break;
                }
            }

            if (isMatch)
            {
                // SUCCESS
                isUnlocked = true;
                if (lcdStatusText != null)
                    lcdStatusText.text = "<color=#10B981>✔ ACCESS GRANTED</color>";
                if (lcdDisplayText != null)
                    lcdDisplayText.text = $"<color=#10B981>[ {enteredCode} ]</color>";

                PlaySuccessChime();
                yield return new WaitForSecondsRealtime(0.75f);

                gameObject.SetActive(false);
                onUnlockedCallback?.Invoke();
            }
            else
            {
                // FAILED ATTEMPT
                remainingAttempts--;
                PlayErrorBuzzer();

                if (lcdStatusText != null)
                    lcdStatusText.text = "<color=#EF4444>⛔ INVALID CODE</color>";
                if (lcdDisplayText != null)
                    lcdDisplayText.text = "<color=#EF4444>[ ERR ]</color>";

                yield return new WaitForSecondsRealtime(0.6f);

                if (remainingAttempts <= 0)
                {
                    // PERMANENT LOCKOUT
                    isLockedOut = true;
                    if (lcdStatusText != null)
                        lcdStatusText.text = "<color=#EF4444><b>SECURITY LOCKOUT ENGAGED</b></color>";
                    if (lcdDisplayText != null)
                        lcdDisplayText.text = "<color=#EF4444>[ HALT ]</color>";
                    if (attemptsText != null)
                        attemptsText.text = "<color=#EF4444>LOCKOUT: 0/3 ATTEMPTS - CIRCUIT TRIPPED</color>";

                    PlayLockoutAlarm();
                    yield return new WaitForSecondsRealtime(1.2f);

                    gameObject.SetActive(false);
                    onLockedOutCallback?.Invoke();
                }
                else
                {
                    enteredCode = "";
                    isProcessing = false;
                    UpdateDisplay();
                }
            }
        }

        private void UpdateDisplay()
        {
            if (lcdStatusText != null)
            {
                lcdStatusText.text = "ENTER 4-DIGIT AUTHORIZATION CODE";
            }

            if (lcdDisplayText != null)
            {
                string display = "";
                for (int i = 0; i < MAX_DIGITS; i++)
                {
                    if (i < enteredCode.Length)
                    {
                        display += $"<color=#38BDF8><b>{enteredCode[i]}</b></color> ";
                    }
                    else
                    {
                        display += "<color=#334155>_</color> ";
                    }
                }
                lcdDisplayText.text = $"[  {display.Trim()}  ]";
            }

            if (attemptsText != null)
            {
                string pips = "";
                for (int i = 0; i < 3; i++)
                {
                    if (i < remainingAttempts)
                        pips += "<color=#10B981>●</color> ";
                    else
                        pips += "<color=#EF4444>○</color> ";
                }
                attemptsText.text = $"SECURITY FAILSAFE: {pips} ({remainingAttempts} ATTEMPTS REMAIN)";
            }
        }

        private void BuildOrRefreshUI()
        {
            if (modalContainer != null) return;

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            // Scrim
            Image scrim = GetComponent<Image>();
            if (scrim == null) scrim = gameObject.AddComponent<Image>();
            scrim.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);

            // Container chassis
            modalContainer = new GameObject("KeypadChassis", typeof(RectTransform), typeof(Image), typeof(Outline));
            modalContainer.transform.SetParent(transform, false);
            RectTransform cRect = modalContainer.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(520f, 680f);
            cRect.anchoredPosition = Vector2.zero;

            Image cImg = modalContainer.GetComponent<Image>();
            cImg.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);

            Outline cOut = modalContainer.GetComponent<Outline>();
            cOut.effectColor = new Color(0.3f, 0.35f, 0.45f, 0.8f);
            cOut.effectDistance = new Vector2(2f, -2f);

            // Header banner
            GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerObj.transform.SetParent(modalContainer.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(-40f, 50f);
            hRect.anchoredPosition = new Vector2(0f, -20f);

            TextMeshProUGUI hTmp = headerObj.GetComponent<TextMeshProUGUI>();
            hTmp.text = "<color=#94A3B8>// SEC-LOCK v4.2 //</color> <color=#38BDF8><b>UNIT 408 DEADBOLT</b></color>";
            hTmp.fontSize = 15f;
            hTmp.alignment = TextAlignmentOptions.Center;

            // LCD Box
            GameObject lcdBox = new GameObject("LCDScreen", typeof(RectTransform), typeof(Image), typeof(Outline));
            lcdBox.transform.SetParent(modalContainer.transform, false);
            RectTransform lcdRect = lcdBox.GetComponent<RectTransform>();
            lcdRect.anchorMin = new Vector2(0.5f, 1f);
            lcdRect.anchorMax = new Vector2(0.5f, 1f);
            lcdRect.pivot = new Vector2(0.5f, 1f);
            lcdRect.sizeDelta = new Vector2(440f, 110f);
            lcdRect.anchoredPosition = new Vector2(0f, -75f);

            Image lcdImg = lcdBox.GetComponent<Image>();
            lcdImg.color = new Color(0.03f, 0.06f, 0.05f, 1f); // Dark greenish LCD glass
            Outline lcdOut = lcdBox.GetComponent<Outline>();
            lcdOut.effectColor = new Color(0.08f, 0.28f, 0.18f, 0.9f);
            lcdOut.effectDistance = new Vector2(1.5f, -1.5f);

            // LCD Status Text
            GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObj.transform.SetParent(lcdBox.transform, false);
            RectTransform sRect = statusObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.sizeDelta = new Vector2(-20f, 26f);
            sRect.anchoredPosition = new Vector2(0f, -8f);
            lcdStatusText = statusObj.GetComponent<TextMeshProUGUI>();
            lcdStatusText.fontSize = 11.5f;
            lcdStatusText.alignment = TextAlignmentOptions.Center;
            lcdStatusText.color = new Color(0.35f, 0.65f, 0.5f, 1f);

            // LCD Digits Text
            GameObject digitsObj = new GameObject("DigitsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            digitsObj.transform.SetParent(lcdBox.transform, false);
            RectTransform dRect = digitsObj.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0f, 0f);
            dRect.anchorMax = new Vector2(1f, 0.65f);
            dRect.sizeDelta = Vector2.zero;
            dRect.anchoredPosition = Vector2.zero;
            lcdDisplayText = digitsObj.GetComponent<TextMeshProUGUI>();
            lcdDisplayText.fontSize = 32f;
            lcdDisplayText.fontStyle = FontStyles.Bold;
            lcdDisplayText.alignment = TextAlignmentOptions.Center;
            lcdDisplayText.color = new Color(0.2f, 0.9f, 0.7f, 1f);

            // Attempts indicator
            GameObject attObj = new GameObject("AttemptsIndicator", typeof(RectTransform), typeof(TextMeshProUGUI));
            attObj.transform.SetParent(modalContainer.transform, false);
            RectTransform aRect = attObj.GetComponent<RectTransform>();
            aRect.anchorMin = new Vector2(0.5f, 1f);
            aRect.anchorMax = new Vector2(0.5f, 1f);
            aRect.sizeDelta = new Vector2(440f, 25f);
            aRect.anchoredPosition = new Vector2(0f, -195f);
            attemptsText = attObj.GetComponent<TextMeshProUGUI>();
            attemptsText.fontSize = 11f;
            attemptsText.alignment = TextAlignmentOptions.Center;

            // Keypad Grid Container (3 cols x 4 rows)
            GameObject gridObj = new GameObject("KeyGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(modalContainer.transform, false);
            RectTransform gRect = gridObj.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0.5f, 0.5f);
            gRect.anchorMax = new Vector2(0.5f, 0.5f);
            gRect.pivot = new Vector2(0.5f, 0.5f);
            gRect.sizeDelta = new Vector2(360f, 260f);
            gRect.anchoredPosition = new Vector2(0f, -25f);

            GridLayoutGroup glg = gridObj.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(105f, 55f);
            glg.spacing = new Vector2(18f, 12f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment = TextAnchor.MiddleCenter;

            // Buttons: 1..9, Backspace, 0, Submit
            string[] labels = new string[] {
                "1", "2", "3",
                "4", "5", "6",
                "7", "8", "9",
                "CLR", "0", "ENT"
            };

            // Buttons 4, 5, 6, 8 (and 1) are worn as shown in the reference photo!
            for (int i = 0; i < labels.Length; i++)
            {
                string lbl = labels[i];
                bool isWornKey = (lbl == "4" || lbl == "5" || lbl == "6" || lbl == "8" || lbl == "1");
                CreateKeypadButton(gridObj.transform, lbl, isWornKey);
            }

            // Clue note box under keypad
            GameObject clueObj = new GameObject("ClueBox", typeof(RectTransform), typeof(TextMeshProUGUI));
            clueObj.transform.SetParent(modalContainer.transform, false);
            RectTransform clRect = clueObj.GetComponent<RectTransform>();
            clRect.anchorMin = new Vector2(0.5f, 0f);
            clRect.anchorMax = new Vector2(0.5f, 0f);
            clRect.pivot = new Vector2(0.5f, 0f);
            clRect.sizeDelta = new Vector2(440f, 50f);
            clRect.anchoredPosition = new Vector2(0f, 70f);

            observationClueText = clueObj.GetComponent<TextMeshProUGUI>();
            observationClueText.text = "<color=#FBBF24><b>[OBSERVATION]</b></color> <color=#94A3B8>Buttons <b>4</b>, <b>5</b>, <b>6</b>, and <b>8</b> show noticeable enamel wear and dark skin-oil smudges from frequent entry.</color>";
            observationClueText.fontSize = 10.5f;
            observationClueText.alignment = TextAlignmentOptions.Center;

            // Bottom Exit Button
            GameObject cancelObj = new GameObject("CancelButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            cancelObj.transform.SetParent(modalContainer.transform, false);
            RectTransform cBtnRect = cancelObj.GetComponent<RectTransform>();
            cBtnRect.anchorMin = new Vector2(0.5f, 0f);
            cBtnRect.anchorMax = new Vector2(0.5f, 0f);
            cBtnRect.pivot = new Vector2(0.5f, 0f);
            cBtnRect.sizeDelta = new Vector2(240f, 38f);
            cBtnRect.anchoredPosition = new Vector2(0f, 20f);

            Image cBtnImg = cancelObj.GetComponent<Image>();
            cBtnImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            Outline cBtnOut = cancelObj.GetComponent<Outline>();
            cBtnOut.effectColor = new Color(0.3f, 0.35f, 0.45f, 0.7f);

            Button cBtn = cancelObj.GetComponent<Button>();
            cBtn.onClick.AddListener(CloseKeypad);

            GameObject cTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            cTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform ctRect = cTxtObj.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI ctTmp = cTxtObj.GetComponent<TextMeshProUGUI>();
            ctTmp.text = "[ ESC ] STEP AWAY";
            ctTmp.fontSize = 12f;
            ctTmp.fontStyle = FontStyles.Bold;
            ctTmp.alignment = TextAlignmentOptions.Center;
            ctTmp.color = new Color(0.7f, 0.75f, 0.85f, 1f);
        }

        private void CreateKeypadButton(Transform parent, string label, bool isWornKey)
        {
            GameObject btnObj = new GameObject($"Key_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.GetComponent<Image>();
            Outline ol = btnObj.GetComponent<Outline>();

            // Distinct visual wear styling for buttons 1, 4, 8
            if (isWornKey)
            {
                img.color = new Color(0.18f, 0.16f, 0.12f, 1f); // Weathered brass/enamel tint
                ol.effectColor = new Color(0.75f, 0.55f, 0.25f, 0.85f);
            }
            else
            {
                img.color = new Color(0.12f, 0.14f, 0.17f, 1f);
                ol.effectColor = new Color(0.25f, 0.3f, 0.38f, 0.7f);
            }
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.GetComponent<Button>();
            keypadButtons.Add(btn);

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform tr = txtObj.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (isWornKey)
            {
                tmp.text = $"<color=#FDE047><b>{label}</b></color> <size=60%><color=#CA8A04>•</color></size>";
            }
            else if (label == "ENT")
            {
                tmp.text = "<color=#10B981><b>ENT</b></color>";
            }
            else if (label == "CLR")
            {
                tmp.text = "<color=#EF4444><b>CLR</b></color>";
            }
            else
            {
                tmp.text = $"<color=#E2E8F0><b>{label}</b></color>";
            }

            tmp.fontSize = (label == "ENT" || label == "CLR") ? 13f : 18f;
            tmp.alignment = TextAlignmentOptions.Center;

            // Wire click
            if (label == "ENT")
            {
                btn.onClick.AddListener(OnSubmitClicked);
            }
            else if (label == "CLR")
            {
                btn.onClick.AddListener(OnClearClicked);
            }
            else
            {
                string digit = label;
                btn.onClick.AddListener(() => OnDigitClicked(digit));
            }
        }

        private void PlayBeepSound(float frequency, float duration)
        {
            if (audioSource == null) return;
            // Generate procedural chip tone
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01((samples - i) / (float)(sampleRate * 0.015f));
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.25f;
            }
            AudioClip clip = AudioClip.Create("KeyBeep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip, 0.45f);
        }

        private void PlaySuccessChime()
        {
            StartCoroutine(SuccessChimeSequence());
        }

        private IEnumerator SuccessChimeSequence()
        {
            PlayBeepSound(1046.5f, 0.12f); // C6
            yield return new WaitForSecondsRealtime(0.1f);
            PlayBeepSound(1318.5f, 0.12f); // E6
            yield return new WaitForSecondsRealtime(0.1f);
            PlayBeepSound(1567.98f, 0.25f); // G6
        }

        private void PlayErrorBuzzer()
        {
            PlayBeepSound(196f, 0.35f); // Low G3 buzz
        }

        private void PlayLockoutAlarm()
        {
            StartCoroutine(LockoutSequence());
        }

        private IEnumerator LockoutSequence()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayBeepSound(880f, 0.08f);
                yield return new WaitForSecondsRealtime(0.12f);
            }
        }
    }
}

