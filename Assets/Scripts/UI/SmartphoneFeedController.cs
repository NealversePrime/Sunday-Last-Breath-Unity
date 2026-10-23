using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controls the immersive Day 0 Smartphone interface in Unit 407.
    /// Handles carrier signal blinking, scroll positioning, dying network alert sequence,
    /// and tactile haptic/audio feedback.
    /// </summary>
    public class SmartphoneFeedController : MonoBehaviour
    {
        [Header("Scroll System")]
        [SerializeField] private ScrollRect feedScrollRect;
        [SerializeField] private RectTransform feedContent;

        [Header("Status Bar Readouts")]
        [SerializeField] private TextMeshProUGUI carrierStatusText;
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private TextMeshProUGUI batteryText;

        [Header("Network Alert Toast")]
        [SerializeField] private RectTransform disconnectToast;
        [SerializeField] private GameObject connectionLostCard;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip screenWakeClip;
        [SerializeField] private AudioClip staticBurstClip;
        [SerializeField] private AudioClip alertBeepClip;

        private Coroutine carrierRoutine;
        private Coroutine networkDeathRoutine;
        private bool isNetworkDead = false;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D UI SFX
                }
            }
        }

        private void OnEnable()
        {
            isNetworkDead = false;

            // 1. Reset Feed Scroll to Top
            if (feedScrollRect != null)
            {
                feedScrollRect.verticalNormalizedPosition = 1f;
                feedScrollRect.velocity = Vector2.zero;
            }

            // 2. Hide disconnect toast above viewport
            if (disconnectToast != null)
            {
                disconnectToast.anchoredPosition = new Vector2(0f, 40f);
            }

            if (connectionLostCard != null)
            {
                connectionLostCard.SetActive(true);
            }

            // 3. Play subtle screen wake audio
            if (audioSource != null && screenWakeClip != null)
            {
                audioSource.PlayOneShot(screenWakeClip, 0.45f);
            }

            // 4. Start carrier status blinking loop
            if (carrierRoutine != null) StopCoroutine(carrierRoutine);
            carrierRoutine = StartCoroutine(CarrierBlinkLoop());

            // 5. Start dynamic network death sequence
            if (networkDeathRoutine != null) StopCoroutine(networkDeathRoutine);
            networkDeathRoutine = StartCoroutine(NetworkDeathSequence());
        }

        private void OnDisable()
        {
            if (carrierRoutine != null) StopCoroutine(carrierRoutine);
            if (networkDeathRoutine != null) StopCoroutine(networkDeathRoutine);
        }

        private IEnumerator CarrierBlinkLoop()
        {
            while (!isNetworkDead)
            {
                if (carrierStatusText != null)
                    carrierStatusText.text = "<color=#EF4444><b>[!] NO SIGNAL</b></color>";
                yield return new WaitForSecondsRealtime(1.8f);

                if (isNetworkDead) break;

                if (carrierStatusText != null)
                    carrierStatusText.text = "<color=#94A3B8><b>SEARCHING...</b></color>";
                yield return new WaitForSecondsRealtime(1.8f);
            }

            if (carrierStatusText != null)
            {
                carrierStatusText.text = "<color=#EF4444><b>[X] OFFLINE</b></color>";
            }
        }

        private IEnumerator NetworkDeathSequence()
        {
            // Give player 4.5 seconds of normal reading time before the grid drops
            yield return new WaitForSecondsRealtime(4.5f);

            isNetworkDead = true;

            // 1. Audio alert beep
            if (audioSource != null && alertBeepClip != null)
            {
                audioSource.PlayOneShot(alertBeepClip, 0.5f);
            }

            // 2. Slide down crimson disconnect banner
            if (disconnectToast != null)
            {
                float duration = 0.35f;
                float elapsed = 0f;
                Vector2 startPos = new Vector2(0f, 40f);
                Vector2 endPos = new Vector2(0f, -4f);

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                    disconnectToast.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }
                disconnectToast.anchoredPosition = endPos;
            }

            yield return new WaitForSecondsRealtime(1.2f);

            // 3. Audio static pop & battery drop from 12% to 11%
            if (audioSource != null && staticBurstClip != null)
            {
                audioSource.PlayOneShot(staticBurstClip, 0.4f);
            }

            if (batteryText != null)
            {
                batteryText.text = "<color=#EF4444><b>11%</b></color> <color=#EF4444>[■□]</color>";
            }
        }
    }
}
