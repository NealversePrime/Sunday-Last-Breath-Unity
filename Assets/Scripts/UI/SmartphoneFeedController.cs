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

        public bool IsNetworkDead => isNetworkDead;

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

            // Note: tick.mp3 is an 8.14-second ambient mechanical clock ticking track from Twine, NOT a UI notification beep.
            // Replace alertBeepClip if unassigned or if mistakenly mapped to tick.mp3 to prevent overlapping clock ticks.
            if (alertBeepClip == null || alertBeepClip.name.Contains("tick"))
            {
                alertBeepClip = screenWakeClip ?? Resources.Load<AudioClip>("Audio/SFX/terminal_beep");
#if UNITY_EDITOR
                if (alertBeepClip == null)
                {
                    alertBeepClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/terminal_beep.mp3");
                }
#endif
            }

            EnsureHarmonCardConfigured();
            SanitizeAllPhoneGlyphs();
            EnhanceFeedReadability();
        }

        private void EnsureHarmonCardConfigured()
        {
            if (feedContent == null) return;

            Transform harmonCardTr = feedContent.Find("PostCard_Harmon");
            if (harmonCardTr == null)
            {
                Transform template = feedContent.Find("PostCard_N");
                if (template == null) template = feedContent.Find("PostCard_C");
                if (template == null) return;

                GameObject harmonCard = Instantiate(template.gameObject, feedContent);
                harmonCard.name = "PostCard_Harmon";

                int nIndex = template.GetSiblingIndex();
                harmonCard.transform.SetSiblingIndex(nIndex + 1);
                harmonCardTr = harmonCard.transform;
            }

            // Remove/hide AlertBadge if present so Harmon's post never shows duplicate red text
            Transform alertBadge = harmonCardTr.Find("AlertBadge");
            if (alertBadge != null)
            {
                alertBadge.gameObject.SetActive(false);
            }

            // Remove/hide VideoAttachment if present
            Transform vid = harmonCardTr.Find("VideoAttachment");
            if (vid != null)
            {
                vid.gameObject.SetActive(false);
            }

            // Configure Texts
            TextMeshProUGUI[] tmps = harmonCardTr.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t.name == "AlertBadge")
                {
                    t.gameObject.SetActive(false);
                }
                else if (t.name == "Letter" || t.name == "AvatarText" || (t.transform.parent != null && t.transform.parent.name.Contains("Avatar")))
                {
                    t.text = "H";
                    t.fontSize = 13f;
                    t.fontStyle = FontStyles.Bold;
                }
                else if (t.name == "Name" || t.text.Contains("NewsWire") || t.text.Contains("CityScanner"))
                {
                    t.text = "Prof. Harmon";
                    t.color = new Color(0.96f, 0.72f, 0.25f); // Warm amber
                    t.fontSize = 13.5f;
                    t.fontStyle = FontStyles.Bold;
                }
                else if (t.name == "Handle" || t.text.Contains("@"))
                {
                    t.text = "@ProfHarmon_Soc \u2022 1m";
                    t.fontSize = 11.5f;
                    t.color = new Color(0.58f, 0.64f, 0.74f);
                }
                else if (t.name == "Stats" || t.name == "Metrics")
                {
                    t.text = "RE: 48   RT: 182   ♥ 314";
                    t.fontSize = 11f;
                    t.color = new Color(0.58f, 0.64f, 0.74f);
                }
                else if (t.name == "PostBody" || t.name == "Body")
                {
                    t.text = "To my neighbors at Ascott (especially 4th floor): It's not a riot. Systemic contagion collapse is underway. I've left for the rural counties. If you need supplies, medicine, or my research notes, my door code for Unit 406 is <color=#F59E0B><b>8445</b></color>. Take whatever you need and don't stay in the open.";
                    t.fontSize = 13.5f;
                    t.lineSpacing = 3.5f;
                    t.color = new Color(0.92f, 0.95f, 0.98f);
                }
            }

            Image[] imgs = harmonCardTr.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                if (img.name == "Avatar" || img.name == "AvatarCircle")
                {
                    img.color = new Color(0.65f, 0.40f, 0.15f);
                }
            }
        }

        /// <summary>
        /// Enlarges and balances font sizes and contrast across the phone feed so all emergency posts
        /// and critical intel (like Harmon's door code 8445) are comfortable and easily legible on 1080p and high-res screens.
        /// </summary>
        public void EnhanceFeedReadability()
        {
            if (feedContent == null) return;

            foreach (Transform card in feedContent)
            {
                if (card == null) continue;

                Image cardBg = card.GetComponent<Image>();
                if (cardBg != null)
                {
                    cardBg.color = new Color(0.07f, 0.09f, 0.12f, 0.95f);
                }

                Outline cardOl = card.GetComponent<Outline>();
                if (cardOl != null)
                {
                    cardOl.effectColor = new Color(0.24f, 0.30f, 0.40f, 0.60f);
                }

                VerticalLayoutGroup vlg = card.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.padding = new RectOffset(14, 14, 10, 10);
                    vlg.spacing = 6f;
                }

                TextMeshProUGUI[] tmps = card.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t == null) continue;
                    switch (t.name)
                    {
                        case "Name":
                            t.fontSize = 13.5f;
                            t.fontStyle = FontStyles.Bold;
                            break;
                        case "Handle":
                            t.fontSize = 11.5f;
                            t.color = new Color(0.58f, 0.64f, 0.74f);
                            break;
                        case "AlertBadge":
                            t.fontSize = 12f;
                            t.fontStyle = FontStyles.Bold;
                            t.color = new Color(0.97f, 0.45f, 0.45f);
                            break;
                        case "PostBody":
                            t.fontSize = 13.5f;
                            t.lineSpacing = 3.5f;
                            t.color = new Color(0.92f, 0.95f, 0.98f);
                            break;
                        case "Stats":
                            t.fontSize = 11f;
                            t.color = new Color(0.58f, 0.64f, 0.74f);
                            break;
                        case "VideoTag":
                            t.fontSize = 11.5f;
                            break;
                    }
                }
            }

            if (carrierStatusText != null) carrierStatusText.fontSize = 12f;
            if (clockText != null) clockText.fontSize = 12f;
            if (batteryText != null) batteryText.fontSize = 12f;
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
            EnsureHarmonCardConfigured();
            SanitizeAllPhoneGlyphs();
            EnhanceFeedReadability();
            isNetworkDead = false;

            if (SundayLastBreath.UI.ApartmentHubScreen.Instance != null && SundayLastBreath.UI.ApartmentHubScreen.Instance.CurrentPlayer != null)
            {
                SundayLastBreath.UI.ApartmentHubScreen.Instance.CurrentPlayer.SetFlag("checked_phone_harmon", true);
            }

            // 1. Reset Feed Scroll to Top
            if (feedScrollRect != null)
            {
                feedScrollRect.verticalNormalizedPosition = 1f;
                feedScrollRect.velocity = Vector2.zero;
            }

            // 2. Reset Status Readouts
            if (clockText != null && SundayLastBreath.UI.ApartmentHubScreen.Instance != null && SundayLastBreath.UI.ApartmentHubScreen.Instance.Clock != null)
            {
                clockText.text = SundayLastBreath.UI.ApartmentHubScreen.Instance.Clock.GetTimeString();
            }
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

        private void PlayAlertNotificationSound(float pitch = 1.0f, float volume = 0.4f)
        {
            if (audioSource == null) return;
            AudioClip clipToPlay = alertBeepClip;
            // Guarantee we never play an 8-second mechanical clock loop (tick.mp3) for a phone notification
            if (clipToPlay == null || clipToPlay.name.Contains("tick"))
            {
                clipToPlay = screenWakeClip ?? Resources.Load<AudioClip>("Audio/SFX/terminal_beep");
#if UNITY_EDITOR
                if (clipToPlay == null)
                {
                    clipToPlay = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/terminal_beep.mp3");
                }
#endif
            }

            if (clipToPlay != null)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clipToPlay, volume);
                audioSource.pitch = 1.0f;
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

            // Give player ample time (~4.5s) to read Post 1 (@CityScanner_WA - 5th St Hospital)
            yield return new WaitForSecondsRealtime(4.50f);
            if (isNetworkDead) yield break;

            // Stream in Post 2 (NewsWire Breaking - CDC Bulletin)
            int nIdx = cards.FindIndex(c => c != null && c.name == "PostCard_N");
            if (nIdx >= 0 && cards[nIdx] != null)
            {
                cards[nIdx].SetActive(true);
                PlayAlertNotificationSound(1.10f, 0.35f);
                yield return StartCoroutine(PopInCard(cards[nIdx].transform));
            }
            else if (cards.Count > 1 && cards[1] != null)
            {
                cards[1].SetActive(true);
                PlayAlertNotificationSound(1.10f, 0.35f);
                yield return StartCoroutine(PopInCard(cards[1].transform));
            }

            // Give player ample time (~4.5s) to read Post 2 (CDC Health Bulletin)
            yield return new WaitForSecondsRealtime(4.50f);
            if (isNetworkDead) yield break;

            // Stream in Post 3 (Professor Harmon - Emergency Post with Code 8445)
            int hIdx = cards.FindIndex(c => c != null && c.name == "PostCard_Harmon");
            if (hIdx >= 0 && cards[hIdx] != null)
            {
                cards[hIdx].SetActive(true);
                PlayAlertNotificationSound(1.30f, 0.45f);
                yield return StartCoroutine(PopInCard(cards[hIdx].transform));

                if (dossierOutroText != null)
                {
                    dossierOutroText.text = "A post from your next-door neighbor, <color=#F59E0B><b>Professor Harmon (Unit 406)</b></color>, flashes onto the timeline: he predicted the collapse and posted his door keypad code (<color=#F59E0B><b>8445</b></color>) before cell service dies!\n\n<color=#94A3B8><i>[ Incoming live video stream... ]</i></color>";
                }

                yield return new WaitForSecondsRealtime(4.50f);
                if (isNetworkDead) yield break;
            }

            // Stream in Post 4 (User_9921 - Blurry Video)
            int uIdx = cards.FindIndex(c => c != null && c.name == "PostCard_U");
            if (uIdx >= 0 && cards[uIdx] != null)
            {
                cards[uIdx].SetActive(true);
                PlayAlertNotificationSound(1.0f, 0.35f);
                yield return StartCoroutine(PopInCard(cards[uIdx].transform));
            }
            else if (cards.Count > 2 && cards[2] != null)
            {
                cards[2].SetActive(true);
                PlayAlertNotificationSound(1.0f, 0.35f);
                yield return StartCoroutine(PopInCard(cards[2].transform));
            }

            // Update left dossier with live observation while video buffers
            if (dossierOutroText != null)
            {
                dossierOutroText.text = "The comments are a storm of denial, prayers, and wild conspiracies. Professor Harmon's emergency alert and door code (<color=#F59E0B><b>8445</b></color>) stick firmly in your mind.\n\n<color=#94A3B8><i>[ Buffering attached video stream... ]</i></color>";
            }

            // Give player ample time (~5.5s) to read Post 3 and watch video buffer & live ticker
            yield return new WaitForSecondsRealtime(5.50f);
            if (isNetworkDead) yield break;

            // NETWORK DEATH SEQUENCE
            isNetworkDead = true;

            if (audioSource != null && staticBurstClip != null)
            {
                audioSource.PlayOneShot(staticBurstClip, 0.55f);
            }
            PlayAlertNotificationSound(0.85f, 0.45f);

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
                dossierOutroText.text = "The comments are a storm of denial, prayers, and wild conspiracies. Professor Harmon's emergency alert and door code (<color=#F59E0B><b>8445</b></color>) remain etched into your memory.\n\nBefore you can scroll further, the little loading icon freezes. The signal drops completely. <color=#EF4444>The internet is gone.</color>";
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
                r1 += Random.Range(6, 24);
                l1 += Random.Range(12, 38);
                if (metrics1 != null && metrics1.gameObject.activeInHierarchy)
                {
                    metrics1.text = $"RE: {r1}   RT: 1.2K   ♥ {l1}";
                }

                r2 += Random.Range(20, 75);
                l2 += Random.Range(25, 95);
                if (metrics2 != null && metrics2.gameObject.activeInHierarchy)
                {
                    metrics2.text = $"RE: {r2 / 1000f:F1}K   RT: 14.9K   ♥ {l2 / 1000f:F1}K";
                }

                r3 += Random.Range(35, 120);
                l3 += Random.Range(20, 60);
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

                yield return new WaitForSecondsRealtime(1.0f);
            }

            // Frozen state once network drops
            if (videoLabel != null)
            {
                videoLabel.text = "▶ [VIDEO STREAM FROZEN — ERR_503]\n<size=90%><color=#ef4444>0:14 / 0:30 • Packet stream terminated by host</color></size>";
            }
        }

        /// <summary>
        /// Fast-forwards the live feed to the severed network state if the player skips or clicks.
        /// </summary>
        public void FastForwardFeed()
        {
            if (isNetworkDead) return;

            if (liveStreamRoutine != null) StopCoroutine(liveStreamRoutine);

            isNetworkDead = true;

            // Immediately show all feed cards and reset their alpha
            if (feedContent != null)
            {
                for (int i = 0; i < feedContent.childCount; i++)
                {
                    var child = feedContent.GetChild(i);
                    if (child != null)
                    {
                        child.gameObject.SetActive(true);
                        CanvasGroup cg = child.GetComponent<CanvasGroup>();
                        if (cg != null) cg.alpha = 1f;
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(feedContent);
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

            if (dossierOutroText != null)
            {
                dossierOutroText.text = "The comments are a storm of denial, prayers, and wild conspiracies. Professor Harmon's emergency alert and door code (<color=#F59E0B><b>8445</b></color>) remain etched into your memory.\n\nBefore you can scroll further, the little loading icon freezes. The signal drops completely. <color=#EF4444>The internet is gone.</color>";
            }

            if (feedScrollRect != null)
            {
                feedScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
