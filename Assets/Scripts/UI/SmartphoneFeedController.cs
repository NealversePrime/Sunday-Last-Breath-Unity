using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controls the immersive Day 0 Smartphone interface in Unit 407.
    /// Handles live feed streaming, engagement counter ticking, blurry video playback counter,
    /// carrier signal blinking, dying network alert sequence, and glyph sanitization.
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

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip screenWakeClip;
        [SerializeField] private AudioClip staticBurstClip;
        [SerializeField] private AudioClip alertBeepClip;

        private Coroutine carrierRoutine;
        private Coroutine liveStreamRoutine;
        private Coroutine liveTickerRoutine;
        private bool isNetworkDead = false;

        private TextMeshProUGUI liveBadgeText;
        private TextMeshProUGUI dossierOutroText;
        private RectTransform phoneChassisRect;

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

            SanitizeAllPhoneGlyphs();
        }

        /// <summary>
        /// Replaces unsupported Unicode emojis/symbols (💬, ↺, ☑, □, ▲) before TMP generates meshes,
        /// preventing all missing-glyph console warnings on JetBrainsMono and Oswald fonts.
        /// </summary>
        public void SanitizeAllPhoneGlyphs()
        {
            TextMeshProUGUI[] allTmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTmps)
            {
                if (tmp == null || string.IsNullOrEmpty(tmp.text)) continue;

                string t = tmp.text;
                bool changed = false;

                if (t.Contains("\u2611") || t.Contains("☑"))
                {
                    t = t.Replace("\u2611", "[VERIFIED]").Replace("☑", "[VERIFIED]");
                    changed = true;
                }
                if (t.Contains("\U0001F4AC") || t.Contains("💬"))
                {
                    t = t.Replace("\U0001F4AC", "RE:").Replace("💬", "RE:");
                    changed = true;
                }
                if (t.Contains("\u21BA") || t.Contains("↺"))
                {
                    t = t.Replace("\u21BA", "RT:").Replace("↺", "RT:");
                    changed = true;
                }
                if (t.Contains("\u25A1") || t.Contains("□"))
                {
                    t = t.Replace("\u25A1", ".").Replace("□", ".");
                    changed = true;
                }
                if (t.Contains("\u25B2") || t.Contains("▲") || t.Contains("\u26A0") || t.Contains("⚠"))
                {
                    t = t.Replace("\u25B2", "[!]").Replace("▲", "[!]").Replace("\u26A0", "[!]").Replace("⚠", "[!]");
                    changed = true;
                }

                if (changed)
                {
                    tmp.text = t;
                }

                if (tmp.gameObject.name == "LiveBadge")
                {
                    liveBadgeText = tmp;
                }
                else if (tmp.gameObject.name == "OutroProse")
                {
                    dossierOutroText = tmp;
                }
            }

            Transform centerCol = transform.Find("PhoneCenterColumn");
            if (centerCol != null)
            {
                Transform chassis = centerCol.Find("PhoneChassis");
                if (chassis != null) phoneChassisRect = chassis as RectTransform;
            }
        }

        private void OnEnable()
        {
            SanitizeAllPhoneGlyphs();
            isNetworkDead = false;

            // 1. Reset Feed Scroll to Top
            if (feedScrollRect != null)
            {
                feedScrollRect.verticalNormalizedPosition = 1f;
                feedScrollRect.velocity = Vector2.zero;
            }

            // 2. Reset Status Readouts
            if (carrierStatusText != null)
            {
                carrierStatusText.text = "<color=#F59E0B><b>[!] WEAK 4G (1 BAR)</b></color>";
            }
            if (batteryText != null)
            {
                batteryText.text = "<color=#EF4444><b>12%</b></color> <color=#EF4444>[■.]</color>";
            }
            if (liveBadgeText != null)
            {
                liveBadgeText.text = "<color=#10B981>● SYNCING</color>";
            }

            // 3. Play subtle screen wake audio
            if (audioSource != null && screenWakeClip != null)
            {
                audioSource.PlayOneShot(screenWakeClip, 0.45f);
            }

            // 4. Start carrier status blinking loop
            if (carrierRoutine != null) StopCoroutine(carrierRoutine);
            carrierRoutine = StartCoroutine(CarrierBlinkLoop());

            // 5. Start dynamic live timeline stream & network death sequence
            if (liveStreamRoutine != null) StopCoroutine(liveStreamRoutine);
            liveStreamRoutine = StartCoroutine(LiveTimelineStreamRoutine());

            // 6. Start live engagement counter & video timestamp ticker
            if (liveTickerRoutine != null) StopCoroutine(liveTickerRoutine);
            liveTickerRoutine = StartCoroutine(LiveEngagementTickerRoutine());
        }

        private void OnDisable()
        {
            if (carrierRoutine != null) StopCoroutine(carrierRoutine);
            if (liveStreamRoutine != null) StopCoroutine(liveStreamRoutine);
            if (liveTickerRoutine != null) StopCoroutine(liveTickerRoutine);
        }

        private IEnumerator CarrierBlinkLoop()
        {
            while (!isNetworkDead)
            {
                if (carrierStatusText != null)
                    carrierStatusText.text = "<color=#F59E0B><b>[!] WEAK 4G</b></color>";
                if (liveBadgeText != null)
                    liveBadgeText.text = "<color=#10B981>● LIVE FEED</color>";
                yield return new WaitForSecondsRealtime(0.9f);

                if (isNetworkDead) break;

                if (carrierStatusText != null)
                    carrierStatusText.text = "<color=#94A3B8><b>REFRESHING...</b></color>";
                if (liveBadgeText != null)
                    liveBadgeText.text = "<color=#38BDF8>● SYNCING...</color>";
                yield return new WaitForSecondsRealtime(0.9f);
            }

            if (carrierStatusText != null)
            {
                carrierStatusText.text = "<color=#EF4444><b>[X] NO SIGNAL</b></color>";
            }
            if (liveBadgeText != null)
            {
                liveBadgeText.text = "<color=#EF4444>● OFFLINE</color>";
            }
        }

        private IEnumerator LiveTimelineStreamRoutine()
        {
            // Collect feed cards inside feedContent
            List<GameObject> cards = new List<GameObject>();
            if (feedContent != null)
            {
                for (int i = 0; i < feedContent.childCount; i++)
                {
                    cards.Add(feedContent.GetChild(i).gameObject);
                }
            }

            // Initially show Post 1 (@CityScanner_WA), hide subsequent cards so they stream in live!
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == null) continue;
                cards[i].SetActive(i == 0);
            }

            if (dossierOutroText != null)
            {
                dossierOutroText.text = "<color=#38BDF8><i>[ Refreshing live timeline... Incoming emergency posts... ]</i></color>";
            }

            if (feedContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(feedContent);

            // Stream in Post 2 (NewsWire Breaking - CDC Bulletin)
            yield return new WaitForSecondsRealtime(0.75f);
            if (cards.Count > 1 && cards[1] != null)
            {
                cards[1].SetActive(true);
                if (audioSource != null && alertBeepClip != null)
                    audioSource.PlayOneShot(alertBeepClip, 0.35f);
                yield return StartCoroutine(PopInCard(cards[1].transform));
            }

            // Stream in Post 3 (User_9921 - Blurry Video)
            yield return new WaitForSecondsRealtime(0.95f);
            if (cards.Count > 2 && cards[2] != null)
            {
                cards[2].SetActive(true);
                if (audioSource != null && screenWakeClip != null)
                    audioSource.PlayOneShot(screenWakeClip, 0.35f);
                yield return StartCoroutine(PopInCard(cards[2].transform));
            }

            // Update left dossier with live observation while video buffers
            if (dossierOutroText != null)
            {
                dossierOutroText.text = "The comments are a storm of denial, prayers, and wild conspiracies. One post catches your eye: a traffic map showing military checkpoints. <color=#F59E0B><i>They are barricading the bridges out of the city.</i></color>\n\n<color=#94A3B8><i>[ Buffering attached video stream... ]</i></color>";
            }

            // Let player watch the live counters & video buffer for 1.6s before the cell tower dies
            yield return new WaitForSecondsRealtime(1.60f);

            // NETWORK DEATH SEQUENCE
            isNetworkDead = true;

            if (audioSource != null && staticBurstClip != null)
            {
                audioSource.PlayOneShot(staticBurstClip, 0.55f);
            }
            if (audioSource != null && alertBeepClip != null)
            {
                audioSource.PlayOneShot(alertBeepClip, 0.50f);
            }

            if (carrierStatusText != null)
            {
                carrierStatusText.text = "<color=#EF4444><b>[X] NO SIGNAL</b></color>";
            }
            if (batteryText != null)
            {
                batteryText.text = "<color=#EF4444><b>11%</b></color> <color=#EF4444>[!.]</color>";
            }
            if (liveBadgeText != null)
            {
                liveBadgeText.text = "<color=#EF4444>● OFFLINE</color>";
            }

            // Reveal Disconnect Card (ERROR 503)
            if (cards.Count > 3 && cards[3] != null)
            {
                cards[3].SetActive(true);
                if (feedContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(feedContent);
            }

            // Complete Dossier Outro Prose
            if (dossierOutroText != null)
            {
                dossierOutroText.text = "The comments are a storm of denial, prayers, and wild conspiracies. One post catches your eye: a traffic map showing military checkpoints. <color=#F59E0B><i>They are barricading the bridges out of the city.</i></color>\n\nBefore you can scroll further, the little loading icon freezes. The signal drops completely. <color=#EF4444>The internet is gone.</color>";
            }

            // Subtle chassis glitch jolt
            if (phoneChassisRect != null)
            {
                Vector2 orig = phoneChassisRect.anchoredPosition;
                for (int i = 0; i < 6; i++)
                {
                    float offset = (i % 2 == 0 ? 5f : -5f) * (1f - i / 6f);
                    phoneChassisRect.anchoredPosition = orig + new Vector2(offset, 0f);
                    yield return new WaitForSecondsRealtime(0.03f);
                }
                phoneChassisRect.anchoredPosition = orig;
            }

            // Smoothly scroll down slightly so the ERROR 503 banner is clearly seen
            if (feedScrollRect != null)
            {
                float startPos = feedScrollRect.verticalNormalizedPosition;
                for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
                {
                    feedScrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, 0f, t / 0.35f);
                    yield return null;
                }
                feedScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private IEnumerator PopInCard(Transform cardTr)
        {
            if (feedContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(feedContent);
            CanvasGroup cg = cardTr.GetComponent<CanvasGroup>();
            if (cg == null) cg = cardTr.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            float duration = 0.22f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                cg.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        private IEnumerator LiveEngagementTickerRoutine()
        {
            TextMeshProUGUI metrics1 = null;
            TextMeshProUGUI metrics2 = null;
            TextMeshProUGUI metrics3 = null;
            TextMeshProUGUI videoLabel = null;

            if (feedContent != null)
            {
                TextMeshProUGUI[] tmps = feedContent.GetComponentsInChildren<TextMeshProUGUI>(true);
                int metricIdx = 0;
                foreach (var t in tmps)
                {
                    if (t.gameObject.name == "Stats" || t.gameObject.name == "Metrics")
                    {
                        if (metricIdx == 0) metrics1 = t;
                        else if (metricIdx == 1) metrics2 = t;
                        else if (metricIdx == 2) metrics3 = t;
                        metricIdx++;
                    }
                    else if (t.gameObject.name == "VideoTag" || t.gameObject.name == "VideoLabel")
                    {
                        videoLabel = t;
                    }
                }
            }

            int r1 = 412, l1 = 890;
            int r2 = 3410, l2 = 2140;
            int r3 = 8920, l3 = 1120;
            int vidSec = 11;

            while (!isNetworkDead)
            {
                r1 += Random.Range(3, 14);
                l1 += Random.Range(7, 25);
                if (metrics1 != null && metrics1.gameObject.activeInHierarchy)
                {
                    metrics1.text = $"RE: {r1}   RT: 1.2K   ♥ {l1}";
                }

                r2 += Random.Range(12, 45);
                l2 += Random.Range(15, 60);
                if (metrics2 != null && metrics2.gameObject.activeInHierarchy)
                {
                    metrics2.text = $"RE: {r2 / 1000f:F1}K   RT: 14.9K   ♥ {l2 / 1000f:F1}K";
                }

                r3 += Random.Range(20, 80);
                l3 += Random.Range(10, 40);
                if (metrics3 != null && metrics3.gameObject.activeInHierarchy)
                {
                    metrics3.text = $"RE: {r3 / 1000f:F1}K   RT: 21.6K   ♥ {l3 / 1000f:F1}K";
                }

                if (videoLabel != null && videoLabel.gameObject.activeInHierarchy)
                {
                    vidSec = Mathf.Min(14, vidSec + 1);
                    string state = vidSec < 14 ? "PLAYING" : "BUFFERING... 99%";
                    videoLabel.text = $"▶ [ATTACHED VIDEO: BLURRY — {state}]\n<size=90%><color=#94a3b8>0:{vidSec:D2} / 0:30 • 5th & Pine St • Audio: Gunfire</color></size>";
                }

                yield return new WaitForSecondsRealtime(0.45f);
            }

            // Frozen state once network drops
            if (videoLabel != null)
            {
                videoLabel.text = "▶ [VIDEO STREAM FROZEN — ERR_503]\n<size=90%><color=#ef4444>0:14 / 0:30 • Packet stream terminated by host</color></size>";
            }
        }
    }
}
