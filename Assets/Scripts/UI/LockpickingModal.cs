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
    /// Barebones geometric (squares, lines, outlines) Pin Tumbler Lockpicking Minigame.
    /// Inspired by classic survival horror and immersive sims (Fallout, Oblivion, This War of Mine).
    /// 
    /// Mechanics:
    /// - 3 to 5 vertical tumbler pins in a cutaway chamber.
    /// - [ W ] PUSH PIN: Hold W (or click button) to raise the active pin against its spring.
    /// - THE SWEET SPOT: Listen and watch as the pin crosses the shear line (*CLICK* audio/visual tell).
    /// - [ D ] HOLD/SET PIN: Press D when inside the sweet spot to lock the pin into the shear line.
    /// - [ FORCE LOCK ]: Risk a percentage-based bypass roll (higher with LOCKSMITH trait).
    /// - [ E / ESC ]: Exit/back away without breaking anything.
    /// </summary>
    public class LockpickingModal : MonoBehaviour
    {
        [Header("Lock Configuration")]
        private int totalPins = 4;
        private int currentPinIndex = 0;
        private int lockDifficulty = 45; // e.g. 30 (Easy), 50 (Med), 75 (Hard)
        private int forceLockSuccessRate = 70; // 0 to 100%

        private bool hasLocksmithTrait = false;
        private bool isProcessing = false;
        private bool isUnlocked = false;

        // Pin Physics State
        private class PinData
        {
            public float heightPercent = 0f; // 0 to 100%
            public float sweetSpotPercent = 50f; // 30 to 80%
            public float sweetSpotTolerance = 8f; // +- 8%
            public bool isSet = false;
            public RectTransform pinRect;
            public Image pinImage;
            public RectTransform springRect;
        }

        private List<PinData> pins = new List<PinData>();
        private bool isPushingPin = false;
        private float liftSpeed = 95f;   // % height per second
        private float fallSpeed = 140f;  // % height per second
        private bool sweetSpotHitPlayed = false;

        // Callbacks
        private Action onUnlockedCallback;
        private Action onFailedCallback;
        private Action onCancelledCallback;

        // UI References
        private GameObject modalContainer;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI pinCountText;
        private TextMeshProUGUI forceLockBtnText;
        private RectTransform chamberBoxRect;
        private RectTransform shearLineRect;
        private RectTransform pinPointerRect;
        private RectTransform pickToolRect;
        private Button forceLockBtn;
        private AudioSource audioSource;

        private const float CHAMBER_WIDTH = 260f;
        private const float CHAMBER_HEIGHT = 160f;

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
        /// Launch the Lockpicking Minigame.
        /// </summary>
        public void StartLockpick(
            int numPins = 4,
            int difficulty = 50,
            int forcePercent = 65,
            bool hasLocksmith = false,
            Action onUnlocked = null,
            Action onFailed = null,
            Action onCancelled = null)
        {
            EnsureAudioSource();
            onUnlockedCallback = onUnlocked;
            onFailedCallback = onFailed;
            onCancelledCallback = onCancelled;

            totalPins = Mathf.Clamp(numPins, 3, 5);
            lockDifficulty = difficulty;
            hasLocksmithTrait = hasLocksmith;
            forceLockSuccessRate = Mathf.Clamp(forcePercent + (hasLocksmith ? 25 : 0), 10, 95);

            currentPinIndex = 0;
            isUnlocked = false;
            isProcessing = false;
            isPushingPin = false;

            // Generate Pin Data
            pins.Clear();
            float tolerance = hasLocksmith ? 11f : 7.5f;

            for (int i = 0; i < totalPins; i++)
            {
                pins.Add(new PinData
                {
                    heightPercent = 0f,
                    sweetSpotPercent = UnityEngine.Random.Range(35f, 75f),
                    sweetSpotTolerance = tolerance,
                    isSet = false
                });
            }

            BuildOrRefreshUI();
            gameObject.SetActive(true);
            UpdatePinChamberVisuals();
            UpdateUIInfo();

            PlayBeepSound(1100f, 0.05f);
        }

        public void CloseModal()
        {
            isPushingPin = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || isProcessing || isUnlocked) return;

            // 1. Keyboard Controls
            bool wHeld = false;
            bool dPressed = false;
            bool fPressed = false;
            bool cancelPressed = false;

            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                wHeld = kb.wKey.isPressed || kb.upArrowKey.isPressed;
                dPressed = kb.dKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
                fPressed = kb.fKey.wasPressedThisFrame;
                cancelPressed = kb.escapeKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame;
            }
            else
            {
                try
                {
                    wHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                    dPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
                    fPressed = Input.GetKeyDown(KeyCode.F);
                    cancelPressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E);
                }
                catch { }
            }

            if (cancelPressed)
            {
                OnExitClicked();
                return;
            }

            if (fPressed)
            {
                OnForceLockClicked();
                return;
            }

            isPushingPin = wHeld;

            if (dPressed)
            {
                OnSetPinClicked();
                return;
            }

            // 2. Physics / Height Update of Active Pin
            if (currentPinIndex < pins.Count)
            {
                var curPin = pins[currentPinIndex];
                if (!curPin.isSet)
                {
                    float prevHeight = curPin.heightPercent;

                    if (isPushingPin)
                    {
                        curPin.heightPercent = Mathf.Clamp(curPin.heightPercent + (liftSpeed * Time.unscaledDeltaTime), 0f, 100f);
                    }
                    else
                    {
                        curPin.heightPercent = Mathf.Clamp(curPin.heightPercent - (fallSpeed * Time.unscaledDeltaTime), 0f, 100f);
                    }

                    // Check Sweet Spot Crossing
                    bool inSweetSpot = Mathf.Abs(curPin.heightPercent - curPin.sweetSpotPercent) <= curPin.sweetSpotTolerance;

                    if (inSweetSpot && !sweetSpotHitPlayed)
                    {
                        sweetSpotHitPlayed = true;
                        PlaySweetSpotClick();
                    }
                    else if (!inSweetSpot && sweetSpotHitPlayed)
                    {
                        sweetSpotHitPlayed = false;
                    }

                    UpdatePinVisual(currentPinIndex);
                }
            }
        }

        public void OnSetPinClicked()
        {
            if (isProcessing || isUnlocked || currentPinIndex >= pins.Count) return;

            var pin = pins[currentPinIndex];
            bool inSweetSpot = Mathf.Abs(pin.heightPercent - pin.sweetSpotPercent) <= pin.sweetSpotTolerance;

            if (inSweetSpot)
            {
                // Successful pin lock
                pin.isSet = true;
                pin.heightPercent = pin.sweetSpotPercent; // Lock cleanly at shear height
                PlayPinLockClack();

                currentPinIndex++;
                sweetSpotHitPlayed = false;
                UpdatePinVisual(currentPinIndex - 1);
                UpdateUIInfo();

                if (currentPinIndex >= pins.Count)
                {
                    // ALL PINS SET -> UNLOCKED!
                    StartCoroutine(UnlockSequence());
                }
                else
                {
                    UpdatePinChamberVisuals();
                }
            }
            else
            {
                // Slip / Failure
                pin.heightPercent = 0f;
                PlaySnapBackThud();

                if (statusText != null)
                {
                    statusText.text = "<color=#EF4444>PIN SLIPPED! TENSION LOST.</color>";
                }

                // Shake chamber slightly
                StartCoroutine(ShakeChamber());
                UpdatePinVisual(currentPinIndex);
            }
        }

        public void OnForceLockClicked()
        {
            if (isProcessing || isUnlocked) return;
            StartCoroutine(ForceLockSequence());
        }

        private IEnumerator ForceLockSequence()
        {
            isProcessing = true;
            if (statusText != null)
                statusText.text = "<color=#F59E0B>APPLYING BRUTE TENSION...</color>";

            PlayBeepSound(400f, 0.15f);
            yield return new WaitForSecondsRealtime(0.4f);

            int roll = UnityEngine.Random.Range(1, 101);
            if (roll <= forceLockSuccessRate)
            {
                // Success!
                for (int i = 0; i < pins.Count; i++)
                {
                    pins[i].isSet = true;
                    pins[i].heightPercent = pins[i].sweetSpotPercent;
                    UpdatePinVisual(i);
                }
                currentPinIndex = pins.Count;
                StartCoroutine(UnlockSequence());
            }
            else
            {
                // Failed & jammed
                PlayErrorBuzzer();
                if (statusText != null)
                    statusText.text = "<color=#EF4444>FAILED! LOCK MECHANISM JAMMED.</color>";

                StartCoroutine(ShakeChamber());
                yield return new WaitForSecondsRealtime(0.9f);

                CloseModal();
                onFailedCallback?.Invoke();
            }
        }

        private IEnumerator UnlockSequence()
        {
            isProcessing = true;
            isUnlocked = true;

            if (statusText != null)
                statusText.text = "<color=#10B981>[SUCCESS] ALL PINS SET // CYLINDER TURNING...</color>";

            PlayUnlockFanfare();

            // Rotate cylinder / pick tool animation
            if (pickToolRect != null)
            {
                float elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    pickToolRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -35f, elapsed / 0.4f));
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            yield return new WaitForSecondsRealtime(0.6f);

            CloseModal();
            onUnlockedCallback?.Invoke();
        }

        public void OnExitClicked()
        {
            if (isProcessing) return;
            CloseModal();
            onCancelledCallback?.Invoke();
        }

        private IEnumerator ShakeChamber()
        {
            if (chamberBoxRect == null) yield break;
            Vector2 orig = chamberBoxRect.anchoredPosition;
            float t = 0f;
            while (t < 0.25f)
            {
                chamberBoxRect.anchoredPosition = orig + new Vector2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-3f, 3f));
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            chamberBoxRect.anchoredPosition = orig;
        }

        // ==========================================
        // BAREBONES UI BUILDING (SQUARES & LINES)
        // ==========================================
        private void BuildOrRefreshUI()
        {
            if (modalContainer != null) return;

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // Scrim
            Image scrim = GetComponent<Image>();
            if (scrim == null) scrim = gameObject.AddComponent<Image>();
            scrim.color = new Color(0.02f, 0.03f, 0.05f, 0.92f);

            // Modal Chassis Box (Square container)
            modalContainer = new GameObject("LockpickChassis", typeof(RectTransform), typeof(Image), typeof(Outline));
            modalContainer.transform.SetParent(transform, false);
            RectTransform cRect = modalContainer.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(640f, 440f);
            cRect.anchoredPosition = Vector2.zero;

            Image cImg = modalContainer.GetComponent<Image>();
            cImg.color = new Color(0.08f, 0.10f, 0.14f, 0.98f);

            Outline cOut = modalContainer.GetComponent<Outline>();
            cOut.effectColor = new Color(0.3f, 0.4f, 0.55f, 0.9f);
            cOut.effectDistance = new Vector2(2f, -2f);

            // Header Title Line
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(modalContainer.transform, false);
            RectTransform tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(-40f, 40f);
            tRt.anchoredPosition = new Vector2(0f, -16f);

            titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.fontSize = 16f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            // ------------------------------------------
            // LEFT SIDE: Lock Cylinder & Pick Tool
            // ------------------------------------------
            GameObject cylinderObj = new GameObject("LockCylinder", typeof(RectTransform), typeof(Image), typeof(Outline));
            cylinderObj.transform.SetParent(modalContainer.transform, false);
            RectTransform cylRect = cylinderObj.GetComponent<RectTransform>();
            cylRect.anchorMin = new Vector2(0f, 0.5f);
            cylRect.anchorMax = new Vector2(0f, 0.5f);
            cylRect.pivot = new Vector2(0.5f, 0.5f);
            cylRect.sizeDelta = new Vector2(180f, 180f);
            cylRect.anchoredPosition = new Vector2(135f, 35f);

            Image cylImg = cylinderObj.GetComponent<Image>();
            cylImg.color = new Color(0.12f, 0.16f, 0.22f, 1f);

            Outline cylOut = cylinderObj.GetComponent<Outline>();
            cylOut.effectColor = new Color(0.45f, 0.55f, 0.70f, 0.9f);
            cylOut.effectDistance = new Vector2(2f, -2f);

            // Inner Plug (Plug Square)
            GameObject plugObj = new GameObject("InnerPlug", typeof(RectTransform), typeof(Image), typeof(Outline));
            plugObj.transform.SetParent(cylinderObj.transform, false);
            RectTransform plugRect = plugObj.GetComponent<RectTransform>();
            plugRect.anchorMin = new Vector2(0.5f, 0.5f);
            plugRect.anchorMax = new Vector2(0.5f, 0.5f);
            plugRect.sizeDelta = new Vector2(110f, 110f);
            plugRect.anchoredPosition = Vector2.zero;

            Image plugImg = plugObj.GetComponent<Image>();
            plugImg.color = new Color(0.18f, 0.22f, 0.30f, 1f);

            Outline plugOut = plugObj.GetComponent<Outline>();
            plugOut.effectColor = new Color(0.35f, 0.45f, 0.60f, 0.8f);

            // Keyway Vertical Line
            GameObject keywayObj = new GameObject("KeywaySlot", typeof(RectTransform), typeof(Image));
            keywayObj.transform.SetParent(plugObj.transform, false);
            RectTransform kwRect = keywayObj.GetComponent<RectTransform>();
            kwRect.anchorMin = new Vector2(0.5f, 0.5f);
            kwRect.anchorMax = new Vector2(0.5f, 0.5f);
            kwRect.sizeDelta = new Vector2(8f, 55f);
            kwRect.anchoredPosition = Vector2.zero;
            Image kwImg = keywayObj.GetComponent<Image>();
            kwImg.color = new Color(0.04f, 0.05f, 0.07f, 1f);

            // Pick Needle Tool (Line)
            GameObject pickObj = new GameObject("PickTool", typeof(RectTransform), typeof(Image));
            pickObj.transform.SetParent(cylinderObj.transform, false);
            pickToolRect = pickObj.GetComponent<RectTransform>();
            pickToolRect.anchorMin = new Vector2(0.5f, 0.5f);
            pickToolRect.anchorMax = new Vector2(0.5f, 0.5f);
            pickToolRect.pivot = new Vector2(0.5f, 0.1f);
            pickToolRect.sizeDelta = new Vector2(6f, 100f);
            pickToolRect.anchoredPosition = new Vector2(0f, -45f);
            Image pickImg = pickObj.GetComponent<Image>();
            pickImg.color = new Color(0.75f, 0.82f, 0.90f, 1f);

            // ------------------------------------------
            // RIGHT SIDE: Cutaway Tumbler Pin Chamber
            // ------------------------------------------
            GameObject chamberObj = new GameObject("TumblerChamber", typeof(RectTransform), typeof(Image), typeof(Outline));
            chamberObj.transform.SetParent(modalContainer.transform, false);
            chamberBoxRect = chamberObj.GetComponent<RectTransform>();
            chamberBoxRect.anchorMin = new Vector2(1f, 0.5f);
            chamberBoxRect.anchorMax = new Vector2(1f, 0.5f);
            chamberBoxRect.pivot = new Vector2(1f, 0.5f);
            chamberBoxRect.sizeDelta = new Vector2(CHAMBER_WIDTH, CHAMBER_HEIGHT);
            chamberBoxRect.anchoredPosition = new Vector2(-40f, 45f);

            Image chImg = chamberObj.GetComponent<Image>();
            chImg.color = new Color(0.03f, 0.04f, 0.06f, 1f);

            Outline chOut = chamberObj.GetComponent<Outline>();
            chOut.effectColor = new Color(0.20f, 0.65f, 0.90f, 0.85f); // Cyan technical border
            chOut.effectDistance = new Vector2(2f, -2f);

            // Chamber Header Label
            GameObject chLblObj = new GameObject("ChamberHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
            chLblObj.transform.SetParent(chamberBoxRect, false);
            RectTransform chlRect = chLblObj.GetComponent<RectTransform>();
            chlRect.anchorMin = new Vector2(0f, 1f);
            chlRect.anchorMax = new Vector2(1f, 1f);
            chlRect.pivot = new Vector2(0.5f, 1f);
            chlRect.sizeDelta = new Vector2(0f, 22f);
            chlRect.anchoredPosition = new Vector2(0f, 22f);
            TextMeshProUGUI chlTmp = chLblObj.GetComponent<TextMeshProUGUI>();
            chlTmp.text = "<color=#38BDF8>TUMBLER CUTAWAY // SHEAR LINE</color>";
            chlTmp.fontSize = 11f;
            chlTmp.fontStyle = FontStyles.Bold;
            chlTmp.alignment = TextAlignmentOptions.Center;

            // Horizontal Shear Line (Dashed / Line across center)
            GameObject shearObj = new GameObject("ShearLine", typeof(RectTransform), typeof(Image));
            shearObj.transform.SetParent(chamberBoxRect, false);
            shearLineRect = shearObj.GetComponent<RectTransform>();
            shearLineRect.anchorMin = new Vector2(0f, 0.5f);
            shearLineRect.anchorMax = new Vector2(1f, 0.5f);
            shearLineRect.pivot = new Vector2(0.5f, 0.5f);
            shearLineRect.sizeDelta = new Vector2(0f, 2f);
            shearLineRect.anchoredPosition = Vector2.zero;
            Image slImg = shearObj.GetComponent<Image>();
            slImg.color = new Color(0.25f, 0.85f, 0.50f, 0.5f); // Soft green shear line

            // Current Pin Pointer (Arrow / marker line at bottom)
            GameObject ptrObj = new GameObject("PinPointer", typeof(RectTransform), typeof(Image));
            ptrObj.transform.SetParent(chamberBoxRect, false);
            pinPointerRect = ptrObj.GetComponent<RectTransform>();
            pinPointerRect.anchorMin = new Vector2(0f, 0f);
            pinPointerRect.anchorMax = new Vector2(0f, 0f);
            pinPointerRect.pivot = new Vector2(0.5f, 0f);
            pinPointerRect.sizeDelta = new Vector2(14f, 6f);
            pinPointerRect.anchoredPosition = new Vector2(30f, 4f);
            Image ptrImg = ptrObj.GetComponent<Image>();
            ptrImg.color = new Color(0.22f, 0.74f, 0.97f, 1f); // Cyan marker

            // ------------------------------------------
            // BOTTOM DOCKET & ACTION BUTTONS
            // ------------------------------------------
            // Status Text Line
            GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObj.transform.SetParent(modalContainer.transform, false);
            RectTransform sRt = statusObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0f);
            sRt.anchorMax = new Vector2(0.5f, 0f);
            sRt.pivot = new Vector2(0.5f, 0f);
            sRt.sizeDelta = new Vector2(560f, 26f);
            sRt.anchoredPosition = new Vector2(0f, 105f);

            statusText = statusObj.GetComponent<TextMeshProUGUI>();
            statusText.fontSize = 13f;
            statusText.fontStyle = FontStyles.Bold;
            statusText.alignment = TextAlignmentOptions.Center;

            // Pin Count / Progress
            GameObject pcObj = new GameObject("PinCountText", typeof(RectTransform), typeof(TextMeshProUGUI));
            pcObj.transform.SetParent(modalContainer.transform, false);
            RectTransform pcRt = pcObj.GetComponent<RectTransform>();
            pcRt.anchorMin = new Vector2(0.5f, 0f);
            pcRt.anchorMax = new Vector2(0.5f, 0f);
            pcRt.pivot = new Vector2(0.5f, 0f);
            pcRt.sizeDelta = new Vector2(560f, 22f);
            pcRt.anchoredPosition = new Vector2(0f, 130f);

            pinCountText = pcObj.GetComponent<TextMeshProUGUI>();
            pinCountText.fontSize = 12f;
            pinCountText.alignment = TextAlignmentOptions.Center;

            // Controls Bar Container (Buttons Grid)
            GameObject barObj = new GameObject("ControlsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            barObj.transform.SetParent(modalContainer.transform, false);
            RectTransform bRt = barObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0f);
            bRt.anchorMax = new Vector2(0.5f, 0f);
            bRt.pivot = new Vector2(0.5f, 0f);
            bRt.sizeDelta = new Vector2(600f, 44f);
            bRt.anchoredPosition = new Vector2(0f, 50f);

            HorizontalLayoutGroup hlg = barObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Button 1: [W] PUSH PIN
            CreateActionButton(barObj.transform, "[ W ] PUSH PIN", new Color(0.15f, 0.20f, 0.28f, 0.95f), () =>
            {
                // Click toggle/tap support
                if (currentPinIndex < pins.Count)
                {
                    pins[currentPinIndex].heightPercent = Mathf.Clamp(pins[currentPinIndex].heightPercent + 15f, 0f, 100f);
                    UpdatePinVisual(currentPinIndex);
                }
            });

            // Button 2: [ D ] SET PIN
            CreateActionButton(barObj.transform, "[ D ] HOLD / SET PIN", new Color(0.12f, 0.35f, 0.22f, 0.95f), OnSetPinClicked);

            // Button 3: FORCE LOCK %
            GameObject forceBtnObj = CreateActionButton(barObj.transform, "FORCE LOCK [ 70% ]", new Color(0.35f, 0.22f, 0.08f, 0.95f), OnForceLockClicked);
            forceLockBtn = forceBtnObj.GetComponent<Button>();
            forceLockBtnText = forceBtnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Button 4: [ E ] EXIT
            CreateActionButton(barObj.transform, "[ E ] EXIT", new Color(0.20f, 0.08f, 0.08f, 0.95f), OnExitClicked);

            // Instructions Hint at very bottom
            GameObject hintObj = new GameObject("InstructionsHint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintObj.transform.SetParent(modalContainer.transform, false);
            RectTransform hiRt = hintObj.GetComponent<RectTransform>();
            hiRt.anchorMin = new Vector2(0.5f, 0f);
            hiRt.anchorMax = new Vector2(0.5f, 0f);
            hiRt.pivot = new Vector2(0.5f, 0f);
            hiRt.sizeDelta = new Vector2(560f, 24f);
            hiRt.anchoredPosition = new Vector2(0f, 16f);

            TextMeshProUGUI hiTmp = hintObj.GetComponent<TextMeshProUGUI>();
            hiTmp.text = "<color=#64748B>Hold [W] to push pin up. Listen for the <b>*CLICK*</b> at the sweet spot, then hit [D] to set!</color>";
            hiTmp.fontSize = 11f;
            hiTmp.alignment = TextAlignmentOptions.Center;
        }

        private GameObject CreateActionButton(Transform parent, string label, Color bgCol, Action onClick)
        {
            GameObject btnObj = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.GetComponent<Image>();
            img.color = bgCol;

            Outline outl = btnObj.GetComponent<Outline>();
            outl.effectColor = new Color(0.4f, 0.5f, 0.6f, 0.8f);
            outl.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 11f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            return btnObj;
        }

        private void UpdatePinChamberVisuals()
        {
            if (chamberBoxRect == null) return;

            // Clear old pin objects
            for (int i = chamberBoxRect.childCount - 1; i >= 0; i--)
            {
                var child = chamberBoxRect.GetChild(i);
                if (child.name.StartsWith("PinColumn_"))
                {
                    Destroy(child.gameObject);
                }
            }

            float spacing = CHAMBER_WIDTH / (totalPins + 1);

            for (int i = 0; i < pins.Count; i++)
            {
                float posX = (i + 1) * spacing;

                GameObject colObj = new GameObject("PinColumn_" + i, typeof(RectTransform));
                colObj.transform.SetParent(chamberBoxRect, false);
                RectTransform colRect = colObj.GetComponent<RectTransform>();
                colRect.anchorMin = new Vector2(0f, 0.5f);
                colRect.anchorMax = new Vector2(0f, 0.5f);
                colRect.pivot = new Vector2(0.5f, 0.5f);
                colRect.sizeDelta = new Vector2(24f, CHAMBER_HEIGHT - 10f);
                colRect.anchoredPosition = new Vector2(posX, 0f);

                // Upper Spring Line (Dotted or line)
                GameObject sprObj = new GameObject("SpringLine", typeof(RectTransform), typeof(Image));
                sprObj.transform.SetParent(colObj.transform, false);
                RectTransform sprRt = sprObj.GetComponent<RectTransform>();
                sprRt.anchorMin = new Vector2(0.5f, 1f);
                sprRt.anchorMax = new Vector2(0.5f, 1f);
                sprRt.pivot = new Vector2(0.5f, 1f);
                sprRt.sizeDelta = new Vector2(4f, 40f);
                sprRt.anchoredPosition = Vector2.zero;
                Image sprImg = sprObj.GetComponent<Image>();
                sprImg.color = new Color(0.35f, 0.45f, 0.55f, 0.7f);

                // Pin Rect (Square bar)
                GameObject pBarObj = new GameObject("PinBar", typeof(RectTransform), typeof(Image), typeof(Outline));
                pBarObj.transform.SetParent(colObj.transform, false);
                RectTransform pRt = pBarObj.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0f);
                pRt.anchorMax = new Vector2(0.5f, 0f);
                pRt.pivot = new Vector2(0.5f, 0f);
                pRt.sizeDelta = new Vector2(16f, 50f);
                pRt.anchoredPosition = Vector2.zero;

                Image pImg = pBarObj.GetComponent<Image>();
                pImg.color = new Color(0.85f, 0.70f, 0.25f, 1f); // Brass/gold pin

                Outline pOut = pBarObj.GetComponent<Outline>();
                pOut.effectColor = new Color(0.1f, 0.15f, 0.2f, 0.9f);
                pOut.effectDistance = new Vector2(1f, -1f);

                pins[i].pinRect = pRt;
                pins[i].pinImage = pImg;
                pins[i].springRect = sprRt;

                UpdatePinVisual(i);
            }

            // Update bottom pointer arrow position
            if (pinPointerRect != null && currentPinIndex < pins.Count)
            {
                float ptrX = (currentPinIndex + 1) * spacing;
                pinPointerRect.anchoredPosition = new Vector2(ptrX, 4f);
                pinPointerRect.SetAsLastSibling();
            }
        }

        private void UpdatePinVisual(int index)
        {
            if (index < 0 || index >= pins.Count) return;
            var pin = pins[index];
            if (pin.pinRect == null) return;

            float maxTravel = CHAMBER_HEIGHT - 65f;
            float posY = maxTravel * (pin.heightPercent / 100f);

            pin.pinRect.anchoredPosition = new Vector2(0f, posY);

            // Compress spring
            if (pin.springRect != null)
            {
                float springLen = Mathf.Max(8f, (CHAMBER_HEIGHT - 55f) - posY);
                pin.springRect.sizeDelta = new Vector2(4f, springLen);
            }

            // Color changes
            if (pin.pinImage != null)
            {
                if (pin.isSet)
                {
                    pin.pinImage.color = new Color(0.10f, 0.85f, 0.45f, 1f); // Emerald locked
                }
                else if (index == currentPinIndex)
                {
                    bool inSweetSpot = Mathf.Abs(pin.heightPercent - pin.sweetSpotPercent) <= pin.sweetSpotTolerance;
                    if (inSweetSpot)
                    {
                        pin.pinImage.color = new Color(1f, 0.90f, 0.35f, 1f); // Golden highlight in sweet spot
                    }
                    else
                    {
                        pin.pinImage.color = new Color(0.85f, 0.68f, 0.22f, 1f); // Normal brass
                    }
                }
                else
                {
                    pin.pinImage.color = new Color(0.45f, 0.50f, 0.58f, 0.8f); // Inactive pin
                }
            }
        }

        private void UpdateUIInfo()
        {
            if (titleText != null)
            {
                string traitNotice = hasLocksmithTrait ? " <color=#10B981>[LOCKSMITH: +SWEET SPOT]</color>" : "";
                titleText.text = $"<color=#38BDF8>MECHANICAL LOCKPICKING</color> <color=#94A3B8>[ DIFFICULTY: {lockDifficulty} ]</color>{traitNotice}";
            }

            if (pinCountText != null)
            {
                string pips = "";
                for (int i = 0; i < totalPins; i++)
                {
                    if (i < currentPinIndex)
                        pips += "<color=#10B981>[*]</color> ";
                    else if (i == currentPinIndex)
                        pips += "<color=#38BDF8>[>]</color> ";
                    else
                        pips += "<color=#64748B>[ ]</color> ";
                }
                pinCountText.text = $"PINS: {pips} ({currentPinIndex} / {totalPins} SET)";
            }

            if (statusText != null && !isUnlocked && !isProcessing)
            {
                statusText.text = $"<color=#CBD5E1>PIN #{currentPinIndex + 1} OF {totalPins}: HOLD [W] TO LIFT PIN</color>";
            }

            if (pinPointerRect != null && chamberBoxRect != null && totalPins > 0)
            {
                float spacing = CHAMBER_WIDTH / (totalPins + 1);
                float ptrX = (currentPinIndex + 1) * spacing;
                pinPointerRect.anchoredPosition = new Vector2(ptrX, 4f);
                pinPointerRect.gameObject.SetActive(currentPinIndex < totalPins);
            }

            if (forceLockBtnText != null)
            {
                forceLockBtnText.text = $"FORCE LOCK [{forceLockSuccessRate}%]";
            }
        }

        // ==========================================
        // PROCEDURAL AUDIO SYNTHESIS
        // ==========================================
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
            AudioClip clip = AudioClip.Create("LpBeep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip, 0.40f);
        }

        private void PlaySweetSpotClick()
        {
            // Crisp metallic click
            PlayBeepSound(1800f, 0.035f);
        }

        private void PlayPinLockClack()
        {
            StartCoroutine(PinLockRoutine());
        }

        private IEnumerator PinLockRoutine()
        {
            PlayBeepSound(1200f, 0.04f);
            yield return new WaitForSecondsRealtime(0.04f);
            PlayBeepSound(850f, 0.08f);
        }

        private void PlaySnapBackThud()
        {
            PlayBeepSound(220f, 0.12f);
        }

        private void PlayUnlockFanfare()
        {
            StartCoroutine(UnlockFanfareRoutine());
        }

        private IEnumerator UnlockFanfareRoutine()
        {
            PlayBeepSound(523.25f, 0.08f); // C5
            yield return new WaitForSecondsRealtime(0.07f);
            PlayBeepSound(659.25f, 0.08f); // E5
            yield return new WaitForSecondsRealtime(0.07f);
            PlayBeepSound(783.99f, 0.08f); // G5
            yield return new WaitForSecondsRealtime(0.07f);
            PlayBeepSound(1046.50f, 0.22f); // C6
        }

        private void PlayErrorBuzzer()
        {
            PlayBeepSound(140f, 0.35f);
        }
    }
}
