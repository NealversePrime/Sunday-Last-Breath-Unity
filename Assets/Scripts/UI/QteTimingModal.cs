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
    /// Interactive Quick-Time Event (QTE) Timing Bar Modal.
    /// Replaces desperate / last-resort brute force checks (e.g. deadlifting heavy shelf,
    /// disarming explosive tripwires, forcing electronic locks, shoulder-ramming doors,
    /// sprinting through pitch-black stairwells) when the player lacks specialized traits or high stats.
    ///
    /// An oscillating needle sweeps across a tension bar; the player must click or hit SPACE
    /// while the needle is inside the green Safe Zone!
    /// </summary>
    public class QteTimingModal : MonoBehaviour
    {
        [Header("State")]
        private bool isMoving = false;
        private bool isResolved = false;
        private float currentPositionPercent = 0f; // 0 to 100
        private float direction = 1f;
        private float sweepSpeed = 2.4f;

        private float safeMinPercent = 38f;
        private float safeMaxPercent = 62f;

        private Action onSuccessCallback;
        private Action onFailureCallback;
        private Action onCancelledCallback;

        [Header("UI References")]
        private GameObject modalContainer;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI promptText;
        private RectTransform barTrackRect;
        private RectTransform safeZoneRect;
        private RectTransform needleRect;
        private Image needleImage;
        private Image safeZoneImage;
        private Button actionButton;
        private TextMeshProUGUI actionBtnText;
        private AudioSource audioSource;

        private const float BAR_WIDTH = 480f;

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

        /// <summary>
        /// Launch the QTE Timing minigame with customizable parameters.
        /// </summary>
        public void StartQTE(
            string actionTitle,
            string promptDescription,
            float speed = 2.4f,
            float safeZoneWidthPercent = 24f,
            float safeZoneCenterPercent = 50f,
            Action onSuccess = null,
            Action onFailure = null,
            Action onCancelled = null)
        {
            EnsureAudioSource();
            onSuccessCallback = onSuccess;
            onFailureCallback = onFailure;
            onCancelledCallback = onCancelled;

            sweepSpeed = Mathf.Clamp(speed, 1.2f, 5.0f);
            safeZoneWidthPercent = Mathf.Clamp(safeZoneWidthPercent, 10f, 45f);

            safeMinPercent = Mathf.Clamp(safeZoneCenterPercent - (safeZoneWidthPercent * 0.5f), 5f, 95f - safeZoneWidthPercent);
            safeMaxPercent = safeMinPercent + safeZoneWidthPercent;

            currentPositionPercent = 0f;
            direction = 1f;
            isResolved = false;
            isMoving = true;

            BuildOrRefreshUI();
            gameObject.SetActive(true);

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(actionTitle) 
                    ? "<color=#EF4444>[ ! ] EMERGENCY ACTION</color> <color=#94A3B8>// MANUAL BYPASS</color>" 
                    : $"<color=#EF4444>[ ! ] {actionTitle.ToUpperInvariant()}</color>";

            if (promptText != null)
                promptText.text = string.IsNullOrEmpty(promptDescription)
                    ? "Keep the tension steady. Click or hit <b>[SPACE]</b> when the needle enters the <color=#10B981>GREEN safe zone</color>!"
                    : promptDescription;

            if (actionBtnText != null)
                actionBtnText.text = "<color=#FFFFFF><b>HOLD BREATH & STOP</b> [SPACE]</color>";

            if (actionButton != null)
            {
                actionButton.interactable = true;
                actionButton.GetComponent<Image>().color = new Color(0.18f, 0.05f, 0.06f, 0.95f);
            }

            // Position safe zone visually
            if (safeZoneRect != null)
            {
                float safeW = BAR_WIDTH * (safeZoneWidthPercent / 100f);
                float safeX = BAR_WIDTH * ((safeMinPercent + safeMaxPercent) * 0.5f / 100f) - (BAR_WIDTH * 0.5f);
                safeZoneRect.sizeDelta = new Vector2(safeW, 36f);
                safeZoneRect.anchoredPosition = new Vector2(safeX, 0f);
            }

            if (safeZoneImage != null)
            {
                safeZoneImage.color = new Color(0.06f, 0.72f, 0.40f, 0.42f);
            }

            if (needleImage != null)
            {
                needleImage.color = new Color(1f, 0.22f, 0.22f, 1f);
            }

            UpdateNeedleVisual();
            PlayBeepSound(900f, 0.06f);
        }

        public void CloseModal()
        {
            isMoving = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || isResolved) return;

            // Handle Key Input
            bool actionTriggered = false;
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                {
                    actionTriggered = true;
                }
                else if (kb.escapeKey.wasPressedThisFrame)
                {
                    OnCancelClicked();
                    return;
                }
            }
            else
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E))
                    {
                        actionTriggered = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        OnCancelClicked();
                        return;
                    }
                }
                catch { }
            }

            if (actionTriggered)
            {
                OnActionPressed();
                return;
            }

            // Needle Oscillation
            if (isMoving)
            {
                currentPositionPercent += sweepSpeed * direction * Time.unscaledDeltaTime * 65f;

                if (currentPositionPercent >= 98f)
                {
                    currentPositionPercent = 98f;
                    direction = -1f;
                    PlayBeepSound(450f, 0.02f);
                }
                else if (currentPositionPercent <= 2f)
                {
                    currentPositionPercent = 2f;
                    direction = 1f;
                    PlayBeepSound(450f, 0.02f);
                }

                UpdateNeedleVisual();
            }
        }

        private void UpdateNeedleVisual()
        {
            if (needleRect != null)
            {
                float posX = (BAR_WIDTH * (currentPositionPercent / 100f)) - (BAR_WIDTH * 0.5f);
                needleRect.anchoredPosition = new Vector2(posX, 0f);
            }
        }

        public void OnActionPressed()
        {
            if (isResolved || !isMoving) return;
            isMoving = false;
            isResolved = true;

            if (actionButton != null) actionButton.interactable = false;

            bool isSuccess = (currentPositionPercent >= safeMinPercent && currentPositionPercent <= safeMaxPercent);

            if (isSuccess)
            {
                StartCoroutine(SuccessRoutine());
            }
            else
            {
                StartCoroutine(FailureRoutine());
            }
        }

        private IEnumerator SuccessRoutine()
        {
            if (actionBtnText != null)
                actionBtnText.text = "<color=#10B981><b>✔ SUCCESS! TENSION HELD.</b></color>";

            if (safeZoneImage != null)
                safeZoneImage.color = new Color(0.1f, 1f, 0.5f, 0.85f);

            if (needleImage != null)
                needleImage.color = new Color(0.2f, 1f, 0.7f, 1f);

            PlaySuccessChime();

            yield return new WaitForSecondsRealtime(0.75f);

            CloseModal();
            onSuccessCallback?.Invoke();
        }

        private IEnumerator FailureRoutine()
        {
            if (actionBtnText != null)
                actionBtnText.text = "<color=#EF4444><b>❌ FAILED! TENSION LOST.</b></color>";

            if (needleImage != null)
                needleImage.color = new Color(1f, 0.1f, 0.1f, 1f);

            PlayErrorBuzzer();

            // Screen shake effect on chassis
            if (modalContainer != null)
            {
                Vector3 originalPos = modalContainer.transform.localPosition;
                float elapsed = 0f;
                while (elapsed < 0.35f)
                {
                    float offsetX = UnityEngine.Random.Range(-8f, 8f);
                    float offsetY = UnityEngine.Random.Range(-4f, 4f);
                    modalContainer.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                modalContainer.transform.localPosition = originalPos;
            }

            yield return new WaitForSecondsRealtime(0.7f);

            CloseModal();
            onFailureCallback?.Invoke();
        }

        public void OnCancelClicked()
        {
            if (isResolved) return;
            isMoving = false;
            CloseModal();
            onCancelledCallback?.Invoke();
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
            scrim.color = new Color(0.02f, 0.03f, 0.05f, 0.90f);

            // Container chassis
            modalContainer = new GameObject("QteChassis", typeof(RectTransform), typeof(Image), typeof(Outline));
            modalContainer.transform.SetParent(transform, false);
            RectTransform cRect = modalContainer.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(560f, 320f);
            cRect.anchoredPosition = Vector2.zero;

            Image cImg = modalContainer.GetComponent<Image>();
            cImg.color = new Color(0.07f, 0.08f, 0.10f, 0.98f);

            Outline cOut = modalContainer.GetComponent<Outline>();
            cOut.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.85f); // Emergency red outline
            cOut.effectDistance = new Vector2(2f, -2f);

            // Header Title
            GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerObj.transform.SetParent(modalContainer.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(-40f, 40f);
            hRect.anchoredPosition = new Vector2(0f, -20f);

            titleText = headerObj.GetComponent<TextMeshProUGUI>();
            titleText.fontSize = 16f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            // Prompt / Instructions Subtitle
            GameObject promptObj = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
            promptObj.transform.SetParent(modalContainer.transform, false);
            RectTransform pRect = promptObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0f, 1f);
            pRect.anchorMax = new Vector2(1f, 1f);
            pRect.pivot = new Vector2(0.5f, 1f);
            pRect.sizeDelta = new Vector2(-50f, 44f);
            pRect.anchoredPosition = new Vector2(0f, -65f);

            promptText = promptObj.GetComponent<TextMeshProUGUI>();
            promptText.fontSize = 13f;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.textWrappingMode = TextWrappingModes.Normal;
            promptText.color = new Color(0.8f, 0.85f, 0.9f, 0.95f);

            // Bar Track Container
            GameObject trackObj = new GameObject("BarTrack", typeof(RectTransform), typeof(Image), typeof(Outline));
            trackObj.transform.SetParent(modalContainer.transform, false);
            barTrackRect = trackObj.GetComponent<RectTransform>();
            barTrackRect.anchorMin = new Vector2(0.5f, 0.5f);
            barTrackRect.anchorMax = new Vector2(0.5f, 0.5f);
            barTrackRect.pivot = new Vector2(0.5f, 0.5f);
            barTrackRect.sizeDelta = new Vector2(BAR_WIDTH, 36f);
            barTrackRect.anchoredPosition = new Vector2(0f, 15f);

            Image trackImg = trackObj.GetComponent<Image>();
            trackImg.color = new Color(0.04f, 0.05f, 0.07f, 1f);

            Outline trackOut = trackObj.GetComponent<Outline>();
            trackOut.effectColor = new Color(0.25f, 0.32f, 0.42f, 0.7f);
            trackOut.effectDistance = new Vector2(1.5f, -1.5f);

            // Safe Zone Box
            GameObject safeObj = new GameObject("SafeZone", typeof(RectTransform), typeof(Image), typeof(Outline));
            safeObj.transform.SetParent(barTrackRect, false);
            safeZoneRect = safeObj.GetComponent<RectTransform>();
            safeZoneRect.anchorMin = new Vector2(0.5f, 0.5f);
            safeZoneRect.anchorMax = new Vector2(0.5f, 0.5f);
            safeZoneRect.pivot = new Vector2(0.5f, 0.5f);
            safeZoneRect.sizeDelta = new Vector2(120f, 36f);
            safeZoneRect.anchoredPosition = Vector2.zero;

            safeZoneImage = safeObj.GetComponent<Image>();
            safeZoneImage.color = new Color(0.06f, 0.72f, 0.40f, 0.42f);

            Outline safeOut = safeObj.GetComponent<Outline>();
            safeOut.effectColor = new Color(0.12f, 0.88f, 0.50f, 0.9f);
            safeOut.effectDistance = new Vector2(1.5f, -1.5f);

            // Center Notch in Safe Zone
            GameObject notchObj = new GameObject("SafeNotch", typeof(RectTransform), typeof(Image));
            notchObj.transform.SetParent(safeZoneRect, false);
            RectTransform nRt = notchObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0.5f, 0.5f);
            nRt.anchorMax = new Vector2(0.5f, 0.5f);
            nRt.pivot = new Vector2(0.5f, 0.5f);
            nRt.sizeDelta = new Vector2(2f, 24f);
            nRt.anchoredPosition = Vector2.zero;
            Image nImg = notchObj.GetComponent<Image>();
            nImg.color = new Color(0.12f, 0.88f, 0.50f, 0.7f);

            // Needle / Cursor Line
            GameObject needleObj = new GameObject("Needle", typeof(RectTransform), typeof(Image), typeof(Outline));
            needleObj.transform.SetParent(barTrackRect, false);
            needleRect = needleObj.GetComponent<RectTransform>();
            needleRect.anchorMin = new Vector2(0.5f, 0.5f);
            needleRect.anchorMax = new Vector2(0.5f, 0.5f);
            needleRect.pivot = new Vector2(0.5f, 0.5f);
            needleRect.sizeDelta = new Vector2(4f, 48f); // Extends slightly past bar top/bottom
            needleRect.anchoredPosition = new Vector2(-BAR_WIDTH * 0.5f, 0f);

            needleImage = needleObj.GetComponent<Image>();
            needleImage.color = new Color(1f, 0.22f, 0.22f, 1f);

            Outline needleOut = needleObj.GetComponent<Outline>();
            needleOut.effectColor = new Color(1f, 0.1f, 0.1f, 0.95f);
            needleOut.effectDistance = new Vector2(1f, -1f);

            // Primary Action Button: HOLD BREATH & STOP
            GameObject btnObj = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(modalContainer.transform, false);
            RectTransform bRect = btnObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.sizeDelta = new Vector2(BAR_WIDTH, 48f);
            bRect.anchoredPosition = new Vector2(0f, 65f);

            Image bImg = btnObj.GetComponent<Image>();
            bImg.color = new Color(0.18f, 0.05f, 0.06f, 0.95f);

            Outline bOut = btnObj.GetComponent<Outline>();
            bOut.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.85f);
            bOut.effectDistance = new Vector2(1.5f, -1.5f);

            actionButton = btnObj.GetComponent<Button>();
            actionButton.targetGraphic = bImg;
            actionButton.onClick.AddListener(OnActionPressed);

            GameObject bTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bTxtObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = bTxtObj.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;
            btRect.anchoredPosition = Vector2.zero;

            actionBtnText = bTxtObj.GetComponent<TextMeshProUGUI>();
            actionBtnText.fontSize = 15f;
            actionBtnText.fontStyle = FontStyles.Bold;
            actionBtnText.alignment = TextAlignmentOptions.Center;
            actionBtnText.text = "HOLD BREATH & STOP [SPACE]";

            // Cancel / Back away button
            GameObject cancelObj = new GameObject("CancelBtn", typeof(RectTransform), typeof(Button));
            cancelObj.transform.SetParent(modalContainer.transform, false);
            RectTransform cnRect = cancelObj.GetComponent<RectTransform>();
            cnRect.anchorMin = new Vector2(0.5f, 0f);
            cnRect.anchorMax = new Vector2(0.5f, 0f);
            cnRect.pivot = new Vector2(0.5f, 0f);
            cnRect.sizeDelta = new Vector2(260f, 30f);
            cnRect.anchoredPosition = new Vector2(0f, 20f);

            Button cnBtn = cancelObj.GetComponent<Button>();
            cnBtn.onClick.AddListener(OnCancelClicked);

            GameObject cnTxtObj = new GameObject("CancelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            cnTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform cntRect = cnTxtObj.GetComponent<RectTransform>();
            cntRect.anchorMin = Vector2.zero;
            cntRect.anchorMax = Vector2.one;
            cntRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI cnTmp = cnTxtObj.GetComponent<TextMeshProUGUI>();
            cnTmp.text = "<color=#64748B>[ ESC / BACK AWAY ]</color>";
            cnTmp.fontSize = 12f;
            cnTmp.alignment = TextAlignmentOptions.Center;
        }

        // Procedural Audio Synthesis
        private void PlayBeepSound(float frequency, float duration)
        {
            if (audioSource == null) return;
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01((samples - i) / (float)(sampleRate * 0.012f));
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.25f;
            }
            AudioClip clip = AudioClip.Create("QteBeep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip, 0.40f);
        }

        private void PlaySuccessChime()
        {
            StartCoroutine(SuccessChimeRoutine());
        }

        private IEnumerator SuccessChimeRoutine()
        {
            PlayBeepSound(1046.5f, 0.10f); // C6
            yield return new WaitForSecondsRealtime(0.08f);
            PlayBeepSound(1318.5f, 0.10f); // E6
            yield return new WaitForSecondsRealtime(0.08f);
            PlayBeepSound(1567.98f, 0.20f); // G6
        }

        private void PlayErrorBuzzer()
        {
            PlayBeepSound(165f, 0.35f); // Low buzz
        }
    }
}
