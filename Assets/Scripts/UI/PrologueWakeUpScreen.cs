using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SundayLastBreath.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Prologue Cold Open Scene (Prologue_WakeUp):
    /// Atmospheric Visual Novel intro prior to Character Creation.
    /// Follows the authentic Twine narrative arc: Saturday night talk show -> 
    /// TV broadcast cut to static -> frantic sirens -> distant cut-off scream ->
    /// stumble to balcony window -> "FOCUS ON YOUR REFLECTION..." -> Character Creation.
    /// Supports animated frame loops for both the living room and the TV static noise.
    /// Universal input compatibility using new Input System + IPointerClickHandler + Fullscreen click catcher.
    /// </summary>
    public class PrologueWakeUpScreen : ScreenBase, IPointerClickHandler
    {
        [Header("Stage Visuals")]
        [SerializeField] private Image roomBackdropImage;
        [SerializeField] private Image tvStaticImage;
        [SerializeField] private CanvasGroup tvFrameGroup;

        [Header("Animation Players")]
        [SerializeField] private FrameAnimationPlayer livingRoomAnimPlayer;
        [SerializeField] private FrameAnimationPlayer tvStaticAnimPlayer;

        [Header("Living Room Frame Collections")]
        [SerializeField] private System.Collections.Generic.List<Sprite> normalLivingRoomFrames = new System.Collections.Generic.List<Sprite>();
        [SerializeField] private System.Collections.Generic.List<Sprite> redLivingRoomFrames = new System.Collections.Generic.List<Sprite>();

        [Header("Narration Box")]
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI narrativeBodyText;
        [SerializeField] private GameObject continuePrompt;
        [SerializeField] private Button focusReflectionButton;
        [SerializeField] private Button fullScreenClickCatcher;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource staticSource;
        [SerializeField] private AudioSource sirensSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip staticClip;
        [SerializeField] private AudioClip sirensClip;
        [SerializeField] private AudioClip screamClip;

        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 35f;
        [Tooltip("Minimum time (in seconds) between clicks to prevent accidental double-clicks or faulty mouse switches from skipping text.")]
        [SerializeField] private float clickDebounceDelay = 0.35f;

        // Sequence State
        private int currentStep = 0;
        private bool isTyping = false;
        private bool isTransitioning = false;
        private string currentFullText = "";
        private Coroutine typeRoutine;
        private float lastClickTime = -999f;

        private static readonly string[] NarrativeSteps = new string[]
        {
            // Step 1: Peace before the collapse
            "It was supposed to be a quiet Saturday night. You remember the warmth of the couch, the low hum of the apartment heater, and drifting off to the dull murmur of a late-night talk show.",

            // Step 2: The broadcast dies (Static Jumpscare)
            "Then, the broadcast dies. A harsh, jagged tearing sound shreds through the tail end of your sleep—relentless, cold white noise.",

            // Step 3: Waking in disorientation
            "Your eyes snap open as you jolt upright. The living room is drowning in the chaotic strobe of an untuned television. Beside it, the modem flashes its LEDs in a frantic pulse, searching for a dead connection.",

            // Step 4: Sirens fade in
            "You hold your breath. Beyond the thin glass of the balcony door, a sound bleeds through from the city streets below—a rising, mournful chorus of distant emergency sirens echoing across the night.",

            // Step 5: The scream cut short
            "And buried deep beneath the mechanical wail of the sirens... a raw, terrifying scream that is abruptly cut short in the dark.",

            // Step 6: Reflection at the window
            "Barefoot and disoriented, you stumble toward the glass balcony door. As you reach out to pull the curtain, your own pale, blurred reflection stares back at you from the darkened window pane."
        };

        protected override void Awake()
        {
            base.Awake();

            InitializeAudioSources();

            if (fullScreenClickCatcher != null)
            {
                fullScreenClickCatcher.onClick.RemoveAllListeners();
                fullScreenClickCatcher.onClick.AddListener(OnDialogueAreaClicked);
            }

            if (focusReflectionButton != null)
            {
                focusReflectionButton.onClick.RemoveAllListeners();
                focusReflectionButton.onClick.AddListener(OnFocusReflectionClicked);
                focusReflectionButton.gameObject.SetActive(false);
            }
        }

        private void InitializeAudioSources()
        {
            if (staticSource == null)
            {
                GameObject obj = new GameObject("StaticAudioSource");
                obj.transform.SetParent(transform);
                staticSource = obj.AddComponent<AudioSource>();
                staticSource.loop = true;
                staticSource.playOnAwake = false;
                staticSource.spatialBlend = 0f;
            }

            if (sirensSource == null)
            {
                GameObject obj = new GameObject("SirensAudioSource");
                obj.transform.SetParent(transform);
                sirensSource = obj.AddComponent<AudioSource>();
                sirensSource.loop = true;
                sirensSource.playOnAwake = false;
                sirensSource.spatialBlend = 0f;
            }

            if (sfxSource == null)
            {
                GameObject obj = new GameObject("SFXAudioSource");
                obj.transform.SetParent(transform);
                sfxSource = obj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }
        }

        protected override void OnScreenOpened()
        {
            base.OnScreenOpened();
            StartPrologueSequence();
        }

        protected override void OnScreenClosed()
        {
            base.OnScreenClosed();
            StopAllAudio();
            if (typeRoutine != null) StopCoroutine(typeRoutine);

            if (livingRoomAnimPlayer != null) livingRoomAnimPlayer.Stop();
            if (tvStaticAnimPlayer != null) tvStaticAnimPlayer.Stop();
        }

        public void StartPrologueSequence()
        {
            currentStep = 0;
            isTyping = false;
            isTransitioning = false;
            lastClickTime = Time.unscaledTime;

            // Start living room animation with NORMAL peaceful Saturday night frames
            if (livingRoomAnimPlayer != null)
            {
                if (normalLivingRoomFrames != null && normalLivingRoomFrames.Count > 0)
                {
                    livingRoomAnimPlayer.SetFrames(normalLivingRoomFrames, 8f, true);
                }
                livingRoomAnimPlayer.Play();
            }

            if (roomBackdropImage != null)
            {
                roomBackdropImage.color = Color.white;
            }

            // TV static starts inactive
            if (tvStaticAnimPlayer != null)
            {
                tvStaticAnimPlayer.Stop();
            }

            if (tvStaticImage != null)
            {
                tvStaticImage.gameObject.SetActive(false);
                Color c = tvStaticImage.color;
                c.a = 0f;
                tvStaticImage.color = c;
            }

            if (focusReflectionButton != null) focusReflectionButton.gameObject.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(false);

            StopAllAudio();

            // Begin Step 1
            AdvanceStep();
        }

        private void Update()
        {
            if (!CanvasGroup.interactable || currentStep <= 0 || currentStep > NarrativeSteps.Length) return;

            // If action button is active, let player click the action button specifically,
            // or trigger it with Space / Enter
            if (focusReflectionButton != null && focusReflectionButton.gameObject.activeSelf)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || 
                                                 Keyboard.current.enterKey.wasPressedThisFrame || 
                                                 Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                {
                    OnFocusReflectionClicked();
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnFocusReflectionClicked();
                }
#endif
                return;
            }

#if ENABLE_INPUT_SYSTEM
            bool mouseClicked = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
            bool keyPressed = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || 
                                                          Keyboard.current.enterKey.wasPressedThisFrame || 
                                                          Keyboard.current.numpadEnterKey.wasPressedThisFrame);
            if (mouseClicked || keyPressed)
            {
                OnDialogueAreaClicked();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool mouseClicked = Input.GetMouseButtonDown(0);
            bool keyPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
            if (mouseClicked || keyPressed)
            {
                OnDialogueAreaClicked();
            }
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CanvasGroup.interactable)
            {
                OnDialogueAreaClicked();
            }
        }

        public void OnDialogueAreaClicked()
        {
            if (isTransitioning) return;

            if (focusReflectionButton != null && focusReflectionButton.gameObject.activeSelf)
            {
                // Action button is active, let player click the button specifically
                return;
            }

            // Anti-double-click debounce guard:
            // Prevents hardware switch bounce or accidental rapid double-clicks from skipping narrative text.
            if (Time.unscaledTime - lastClickTime < clickDebounceDelay)
            {
                return;
            }
            lastClickTime = Time.unscaledTime;

            if (isTyping)
            {
                // Fast-forward / complete typewriter text instantly
                CompleteTypingInstant();
            }
            else
            {
                // Advance to next narrative beat
                AdvanceStep();
            }
        }

        private void AdvanceStep()
        {
            if (currentStep >= NarrativeSteps.Length) return;

            currentStep++;
            currentFullText = NarrativeSteps[currentStep - 1];

            // Trigger narrative stage beats & audio cues
            ApplyBeatEffects(currentStep);

            // Start typewriter
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            typeRoutine = StartCoroutine(TypewriterRoutine(currentFullText));
        }

        private void ApplyBeatEffects(int step)
        {
            switch (step)
            {
                case 1:
                    // Quiet peaceful night
                    if (speakerLabel != null) speakerLabel.text = "<color=#94A3B8>[ SATURDAY NIGHT 23:45 ]</color>";
                    if (tvStaticImage != null) tvStaticImage.gameObject.SetActive(false);
                    if (livingRoomAnimPlayer != null && normalLivingRoomFrames != null && normalLivingRoomFrames.Count > 0)
                    {
                        livingRoomAnimPlayer.SetFrames(normalLivingRoomFrames, 8f, true);
                    }
                    if (roomBackdropImage != null) roomBackdropImage.color = Color.white;
                    break;

                case 2:
                    // Broadcast dies: JUMPSCARE TV STATIC!
                    if (speakerLabel != null) speakerLabel.text = "<color=#EF4444>[ SUDDEN SILENCE ]</color>";
                    if (staticClip != null && staticSource != null)
                    {
                        staticSource.clip = staticClip;
                        staticSource.volume = 0.55f;
                        staticSource.Play();
                    }

                    if (tvStaticImage != null)
                    {
                        tvStaticImage.gameObject.SetActive(true);
                        tvStaticImage.color = new Color(1f, 1f, 1f, 0.95f);
                    }

                    if (tvStaticAnimPlayer != null)
                    {
                        tvStaticAnimPlayer.Play();
                    }
                    break;

                case 3:
                    // Shock / living room strobe - switches to RED Emergency Alert System frames
                    if (speakerLabel != null) speakerLabel.text = "<color=#F59E0B>[ WAKING UP ]</color>";
                    if (livingRoomAnimPlayer != null && redLivingRoomFrames != null && redLivingRoomFrames.Count > 0)
                    {
                        livingRoomAnimPlayer.SetFrames(redLivingRoomFrames, 8f, true);
                    }
                    break;

                case 4:
                    // Distant emergency sirens fade in
                    if (speakerLabel != null) speakerLabel.text = "<color=#94A3B8>[ THE NIGHT AIR ]</color>";
                    if (sirensClip != null && sirensSource != null)
                    {
                        sirensSource.clip = sirensClip;
                        sirensSource.volume = 0f;
                        sirensSource.Play();
                        StartCoroutine(FadeAudioSource(sirensSource, 0f, 0.40f, 2.5f));
                    }
                    break;

                case 5:
                    // Terrifying scream cut short in the dark
                    if (speakerLabel != null) speakerLabel.text = "<color=#EF4444>[ ECHOES IN THE DARK ]</color>";
                    if (screamClip != null && sfxSource != null)
                    {
                        sfxSource.PlayOneShot(screamClip, 0.95f);
                    }
                    break;

                case 6:
                    // Stumble to balcony, window reflection
                    if (speakerLabel != null) speakerLabel.text = "<color=#38BDF8>[ WINDOW REFLECTION ]</color>";
                    break;
            }
        }

        private IEnumerator TypewriterRoutine(string targetText)
        {
            isTyping = true;
            if (continuePrompt != null) continuePrompt.SetActive(false);
            if (narrativeBodyText != null) narrativeBodyText.text = "";

            float charDelay = 1f / Mathf.Max(10f, charactersPerSecond);
            int length = targetText.Length;

            for (int i = 0; i < length; i++)
            {
                if (narrativeBodyText != null)
                {
                    narrativeBodyText.text = targetText.Substring(0, i + 1);
                }
                yield return new WaitForSecondsRealtime(charDelay);
            }

            OnTypingComplete();
        }

        private void CompleteTypingInstant()
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            if (narrativeBodyText != null) narrativeBodyText.text = currentFullText;
            OnTypingComplete();
        }

        private void OnTypingComplete()
        {
            isTyping = false;

            if (currentStep < NarrativeSteps.Length)
            {
                if (continuePrompt != null) continuePrompt.SetActive(true);
            }
            else
            {
                // Final step reached! Reveal "FOCUS ON YOUR REFLECTION..."
                if (continuePrompt != null) continuePrompt.SetActive(false);
                if (focusReflectionButton != null)
                {
                    focusReflectionButton.gameObject.SetActive(true);
                    CanvasGroup btnCg = focusReflectionButton.GetComponent<CanvasGroup>();
                    if (btnCg != null) StartCoroutine(FadeCanvasGroup(btnCg, 0f, 1f, 0.8f));
                }
            }
        }

        public void OnFocusReflectionClicked()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            if (focusReflectionButton != null)
            {
                focusReflectionButton.interactable = false;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

            // Stop animations
            if (livingRoomAnimPlayer != null) livingRoomAnimPlayer.Stop();
            if (tvStaticAnimPlayer != null) tvStaticAnimPlayer.Stop();

            // Fade out audio over 1.5 seconds
            if (staticSource != null && staticSource.isPlaying) StartCoroutine(FadeAudioSource(staticSource, staticSource.volume, 0f, 1.5f));
            if (sirensSource != null && sirensSource.isPlaying) StartCoroutine(FadeAudioSource(sirensSource, sirensSource.volume, 0f, 1.5f));

            // Smooth transition to Character Creation
            Hide(1.2f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TransitionFromPrologueToCharacterCreation();
                }
            });
        }

        private IEnumerator FadeAudioSource(AudioSource src, float from, float to, float duration)
        {
            if (src == null) yield break;
            float elapsed = 0f;
            src.volume = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            src.volume = to;
            if (to <= 0.001f) src.Stop();
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null) yield break;
            float elapsed = 0f;
            cg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        private void StopAllAudio()
        {
            if (staticSource != null) { staticSource.Stop(); staticSource.clip = null; }
            if (sirensSource != null) { sirensSource.Stop(); sirensSource.clip = null; }
            if (sfxSource != null) { sfxSource.Stop(); }
        }
    }
}

