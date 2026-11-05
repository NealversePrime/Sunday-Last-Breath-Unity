using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Atmospheric retro terminal loading screen for Sunday's Last Breath.
    /// Handles in-game load transitions, F9 QuickLoad, and main menu resumes.
    /// Features:
    /// - Cleanly purges old scene/room audio (preventing BGM bleed from previous locations)
    /// - Retro CRT/terminal chassis with live survivor telemetry and progress bar
    /// - Authentic survival-horror lore directives / gameplay tips
    /// - Seamless unscaled realtime alpha fading
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private static LoadingScreen _instance;
        public static LoadingScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<LoadingScreen>(FindObjectsInactive.Include);
                    if (_instance == null)
                    {
                        Canvas canvas = FindAnyObjectByType<Canvas>();
                        if (canvas != null)
                        {
                            GameObject go = new GameObject("LoadingScreen", typeof(RectTransform), typeof(CanvasGroup));
                            go.transform.SetParent(canvas.transform, false);
                            _instance = go.AddComponent<LoadingScreen>();
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("State")]
        private bool isBusy = false;
        private CanvasGroup canvasGroup;
        private GameObject chassisObj;

        [Header("UI Element References")]
        private TextMeshProUGUI headerTitleText;
        private TextMeshProUGUI locationTargetText;
        private TextMeshProUGUI survivorInfoText;
        private RectTransform progressBarFillRect;
        private TextMeshProUGUI progressPercentText;
        private TextMeshProUGUI telemetryStatusText;
        private TextMeshProUGUI directiveBodyText;

        private const float PROGRESS_BAR_MAX_WIDTH = 640f;

        private readonly string[] survivalDirectives = new string[]
        {
            "The elevators lost power at 23:14. The central stairwell is the only route down.",
            "Light attracts the infected in the dark. Keep your flashlight switched off unless necessary.",
            "Sound travels through the air ducts. Avoid loud door breaches when an alternative exists.",
            "Professor Harmon posted his door code on the community feed before cell towers went offline.",
            "Marcus spent years rigging tactical defenses. Check for monofilament tripwires before entering cracked doors.",
            "Conserve your stamina. Running while exhausted will severely impair your reflexes in a crisis.",
            "Civil infrastructure collapses in hours, not days. Trust no promises of outside rescue.",
            "A deadbolt will keep you safe until supplies run dry. Eventually, everyone has to venture out.",
            "Mrs. Gable's terrier Buster is easily agitated by sudden noises in the corridor.",
            "Not all survivors you meet will be hostile—but in a crisis, desperation turns neighbors into threats."
        };

        private void Awake()
        {
            if (_instance == null) _instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void LoadSlotWithScreen(int slotIndex, Action onComplete = null)
        {
            if (SaveManager.Instance == null)
            {
                onComplete?.Invoke();
                return;
            }

            var summary = SaveManager.Instance.GetSlotSummary(slotIndex);
            string loc = !string.IsNullOrEmpty(summary.roomDisplayName) ? summary.roomDisplayName : "SECTOR UNKNOWN";
            string survivor = !string.IsNullOrEmpty(summary.characterName) ? summary.characterName : "SURVIVOR";
            string job = !string.IsNullOrEmpty(summary.job) ? summary.job : "CIVILIAN";
            string time = !string.IsNullOrEmpty(summary.clockDisplay) ? summary.clockDisplay : "DAY 0 // 23:45";

            ShowAndLoad(
                loadAction: () =>
                {
                    SaveManager.Instance.LoadFromSlot(slotIndex);
                },
                locationName: loc,
                survivorInfo: $"{survivor.ToUpper()} [{job.ToUpper()}] | {time}",
                onComplete: onComplete
            );
        }

        public void QuickLoadWithScreen(Action onComplete = null)
        {
            if (SaveManager.Instance == null)
            {
                onComplete?.Invoke();
                return;
            }

            var summary = SaveManager.Instance.GetSaveSummary();
            string loc = !string.IsNullOrEmpty(summary.roomDisplayName) ? summary.roomDisplayName : "SECTOR UNKNOWN";
            string survivor = !string.IsNullOrEmpty(summary.characterName) ? summary.characterName : "SURVIVOR";
            string job = !string.IsNullOrEmpty(summary.job) ? summary.job : "CIVILIAN";
            string time = !string.IsNullOrEmpty(summary.clockDisplay) ? summary.clockDisplay : "DAY 0 // 23:45";

            ShowAndLoad(
                loadAction: () =>
                {
                    SaveManager.Instance.QuickLoad();
                },
                locationName: loc,
                survivorInfo: $"{survivor.ToUpper()} [{job.ToUpper()}] | {time}",
                onComplete: onComplete
            );
        }

        public void ShowAndLoad(Action loadAction, string locationName = null, string survivorInfo = null, Action onComplete = null)
        {
            if (isBusy) return;

            BuildUI();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            StartCoroutine(LoadProcessCoroutine(loadAction, locationName, survivorInfo, onComplete));
        }

        private IEnumerator LoadProcessCoroutine(Action loadAction, string locationName, string survivorInfo, Action onComplete)
        {
            isBusy = true;

            // 1. Pick directive & update initial labels
            string directive = survivalDirectives[UnityEngine.Random.Range(0, survivalDirectives.Length)];
            if (directiveBodyText != null)
                directiveBodyText.text = $"<i>\"{directive}\"</i>";

            if (locationTargetText != null)
                locationTargetText.text = $"<color=#64748B>TARGET LOCATION:</color> <color=#38BDF8><b>{locationName ?? "SECTOR BUFFER"}</b></color>";

            if (survivorInfoText != null)
                survivorInfoText.text = $"<color=#64748B>SURVIVOR:</color> <color=#F1F5F9><b>{survivorInfo ?? "CIVILIAN"}</b></color>";

            SetProgress(0f, ">> Initializing sector archive stream...");

            // 2. Smooth Fade In (0 -> 1)
            float elapsed = 0f;
            float fadeInTime = 0.22f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            // 3. Purge previous scene audio & reset channels
            SetProgress(0.25f, ">> Purging audio cache & recalibrating acoustic channels...");
            if (ApartmentHubScreen.Instance != null)
            {
                ApartmentHubScreen.Instance.CloseSystemMenuModal();
                ApartmentHubScreen.Instance.StopAllAudioSources();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM(0.1f);
            }
            yield return new WaitForSecondsRealtime(0.15f);

            // 4. Execute actual load logic (ES3 restore)
            SetProgress(0.55f, ">> Restoring survivor vitals, inventory & world flags...");
            try
            {
                loadAction?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadingScreen] Error during load action: {ex.Message}\n{ex.StackTrace}");
            }
            yield return new WaitForSecondsRealtime(0.15f);

            // 5. Synchronize environmental acoustics for loaded location
            SetProgress(0.85f, ">> Synchronizing room acoustics & ambient feed...");
            if (ApartmentHubScreen.Instance != null)
            {
                ApartmentHubScreen.Instance.RefreshLocationBGM();
            }
            yield return new WaitForSecondsRealtime(0.12f);

            // 6. Complete
            SetProgress(1.0f, ">> Archive synchronized. Resuming survivor session.");
            yield return new WaitForSecondsRealtime(0.22f);

            // 7. Smooth Fade Out (1 -> 0)
            elapsed = 0f;
            float fadeOutTime = 0.28f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutTime));
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
            isBusy = false;
            onComplete?.Invoke();
        }

        private void SetProgress(float pct, string status)
        {
            pct = Mathf.Clamp01(pct);
            if (progressBarFillRect != null)
            {
                progressBarFillRect.sizeDelta = new Vector2(PROGRESS_BAR_MAX_WIDTH * pct, progressBarFillRect.sizeDelta.y);
            }
            if (progressPercentText != null)
            {
                progressPercentText.text = $"<color=#38BDF8><b>{(int)(pct * 100f)}%</b></color>";
            }
            if (telemetryStatusText != null)
            {
                telemetryStatusText.text = status;
            }
        }

        public void BuildUI()
        {
            if (chassisObj != null) return;

            // Fullscreen root
            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Scrim Image
            Image scrim = GetComponent<Image>();
            if (scrim == null) scrim = gameObject.AddComponent<Image>();
            scrim.color = new Color(0.03f, 0.04f, 0.06f, 1f);

            // Center Chassis
            chassisObj = new GameObject("LoadingChassis", typeof(RectTransform), typeof(Image), typeof(Outline));
            chassisObj.transform.SetParent(transform, false);
            RectTransform cRect = chassisObj.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(760f, 440f);
            cRect.anchoredPosition = Vector2.zero;

            Image cImg = chassisObj.GetComponent<Image>();
            cImg.color = new Color(0.07f, 0.09f, 0.12f, 0.98f);

            Outline cOut = chassisObj.GetComponent<Outline>();
            cOut.effectColor = new Color(0.20f, 0.28f, 0.38f, 0.9f);
            cOut.effectDistance = new Vector2(2f, -2f);

            // 1. Header Banner
            GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            headerObj.transform.SetParent(chassisObj.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(-40f, 50f);
            hRect.anchoredPosition = new Vector2(0f, -18f);

            headerTitleText = headerObj.GetComponent<TextMeshProUGUI>();
            headerTitleText.text = "<color=#38BDF8>■</color> <color=#94A3B8>// ASCOTT RESIDENCE ARCHIVE</color> <color=#F59E0B><b>[ SYSTEM RESTORATION ]</b></color>\n<size=70%><color=#64748B>ASCOTT RETRO-TERMINAL // SEC-LOAD v2.1</color></size>";
            headerTitleText.fontSize = 14f;
            headerTitleText.alignment = TextAlignmentOptions.Center;

            // 2. Divider Line
            GameObject div1 = new GameObject("Div1", typeof(RectTransform), typeof(Image));
            div1.transform.SetParent(chassisObj.transform, false);
            RectTransform d1Rect = div1.GetComponent<RectTransform>();
            d1Rect.anchorMin = new Vector2(0.5f, 1f);
            d1Rect.anchorMax = new Vector2(0.5f, 1f);
            d1Rect.sizeDelta = new Vector2(680f, 1.5f);
            d1Rect.anchoredPosition = new Vector2(0f, -74f);
            div1.GetComponent<Image>().color = new Color(0.18f, 0.24f, 0.32f, 0.8f);

            // 3. Location Target Text
            GameObject locObj = new GameObject("TargetLocationText", typeof(RectTransform), typeof(TextMeshProUGUI));
            locObj.transform.SetParent(chassisObj.transform, false);
            RectTransform lRect = locObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.sizeDelta = new Vector2(640f, 26f);
            lRect.anchoredPosition = new Vector2(0f, -92f);

            locationTargetText = locObj.GetComponent<TextMeshProUGUI>();
            locationTargetText.fontSize = 13f;
            locationTargetText.alignment = TextAlignmentOptions.Left;
            locationTargetText.text = "<color=#64748B>TARGET LOCATION:</color> <color=#38BDF8><b>UNIT 407 // LIVING ROOM</b></color>";

            // 4. Survivor Info Text
            GameObject survObj = new GameObject("SurvivorInfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
            survObj.transform.SetParent(chassisObj.transform, false);
            RectTransform sRect = survObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 1f);
            sRect.anchorMax = new Vector2(0.5f, 1f);
            sRect.sizeDelta = new Vector2(640f, 26f);
            sRect.anchoredPosition = new Vector2(0f, -122f);

            survivorInfoText = survObj.GetComponent<TextMeshProUGUI>();
            survivorInfoText.fontSize = 12f;
            survivorInfoText.alignment = TextAlignmentOptions.Left;
            survivorInfoText.text = "<color=#64748B>SURVIVOR:</color> <color=#F1F5F9><b>CIVILIAN</b></color>";

            // 5. Progress Bar Container
            GameObject barContainer = new GameObject("ProgressBarContainer", typeof(RectTransform), typeof(Image), typeof(Outline));
            barContainer.transform.SetParent(chassisObj.transform, false);
            RectTransform bRect = barContainer.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.sizeDelta = new Vector2(PROGRESS_BAR_MAX_WIDTH, 18f);
            bRect.anchoredPosition = new Vector2(0f, -165f);

            Image bImg = barContainer.GetComponent<Image>();
            bImg.color = new Color(0.04f, 0.06f, 0.08f, 1f);

            Outline bOut = barContainer.GetComponent<Outline>();
            bOut.effectColor = new Color(0.20f, 0.28f, 0.38f, 0.8f);
            bOut.effectDistance = new Vector2(1f, -1f);

            // 5b. Progress Bar Fill
            GameObject barFill = new GameObject("ProgressBarFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barContainer.transform, false);
            progressBarFillRect = barFill.GetComponent<RectTransform>();
            progressBarFillRect.anchorMin = new Vector2(0f, 0f);
            progressBarFillRect.anchorMax = new Vector2(0f, 1f);
            progressBarFillRect.pivot = new Vector2(0f, 0.5f);
            progressBarFillRect.sizeDelta = new Vector2(0f, 0f);
            progressBarFillRect.anchoredPosition = Vector2.zero;

            Image fImg = barFill.GetComponent<Image>();
            fImg.color = new Color(0.22f, 0.74f, 0.97f, 1f); // Bright cyber cyan

            // 5c. Percent Label
            GameObject pctObj = new GameObject("PercentText", typeof(RectTransform), typeof(TextMeshProUGUI));
            pctObj.transform.SetParent(chassisObj.transform, false);
            RectTransform pRect = pctObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 1f);
            pRect.anchorMax = new Vector2(0.5f, 1f);
            pRect.sizeDelta = new Vector2(100f, 22f);
            pRect.anchoredPosition = new Vector2(270f, -145f);

            progressPercentText = pctObj.GetComponent<TextMeshProUGUI>();
            progressPercentText.fontSize = 12f;
            progressPercentText.alignment = TextAlignmentOptions.Right;
            progressPercentText.text = "<color=#38BDF8><b>0%</b></color>";

            // 6. Telemetry Status Text
            GameObject telemObj = new GameObject("TelemetryStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            telemObj.transform.SetParent(chassisObj.transform, false);
            RectTransform tRect = telemObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(640f, 24f);
            tRect.anchoredPosition = new Vector2(0f, -196f);

            telemetryStatusText = telemObj.GetComponent<TextMeshProUGUI>();
            telemetryStatusText.fontSize = 11.5f;
            telemetryStatusText.color = new Color(0.58f, 0.64f, 0.72f, 1f);
            telemetryStatusText.alignment = TextAlignmentOptions.Left;
            telemetryStatusText.text = ">> Initializing sector archive stream...";

            // 7. Divider 2
            GameObject div2 = new GameObject("Div2", typeof(RectTransform), typeof(Image));
            div2.transform.SetParent(chassisObj.transform, false);
            RectTransform d2Rect = div2.GetComponent<RectTransform>();
            d2Rect.anchorMin = new Vector2(0.5f, 1f);
            d2Rect.anchorMax = new Vector2(0.5f, 1f);
            d2Rect.sizeDelta = new Vector2(680f, 1.5f);
            d2Rect.anchoredPosition = new Vector2(0f, -230f);
            div2.GetComponent<Image>().color = new Color(0.18f, 0.24f, 0.32f, 0.8f);

            // 8. Directive Card Box
            GameObject dirBox = new GameObject("DirectiveBox", typeof(RectTransform), typeof(Image), typeof(Outline));
            dirBox.transform.SetParent(chassisObj.transform, false);
            RectTransform dbRect = dirBox.GetComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0.5f, 0f);
            dbRect.anchorMax = new Vector2(0.5f, 0f);
            dbRect.pivot = new Vector2(0.5f, 0f);
            dbRect.sizeDelta = new Vector2(660f, 160f);
            dbRect.anchoredPosition = new Vector2(0f, 24f);

            Image dbImg = dirBox.GetComponent<Image>();
            dbImg.color = new Color(0.04f, 0.06f, 0.08f, 0.95f);

            Outline dbOut = dirBox.GetComponent<Outline>();
            dbOut.effectColor = new Color(0.18f, 0.24f, 0.32f, 0.75f);
            dbOut.effectDistance = new Vector2(1f, -1f);

            // 8a. Directive Header
            GameObject dirHeadObj = new GameObject("DirectiveHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
            dirHeadObj.transform.SetParent(dirBox.transform, false);
            RectTransform dhRect = dirHeadObj.GetComponent<RectTransform>();
            dhRect.anchorMin = new Vector2(0f, 1f);
            dhRect.anchorMax = new Vector2(1f, 1f);
            dhRect.pivot = new Vector2(0.5f, 1f);
            dhRect.sizeDelta = new Vector2(-30f, 26f);
            dhRect.anchoredPosition = new Vector2(0f, -12f);

            TextMeshProUGUI dhTmp = dirHeadObj.GetComponent<TextMeshProUGUI>();
            dhTmp.text = "<color=#F59E0B><b>[ ! ] SURVIVAL DIRECTIVE</b></color> <color=#475569>— ASCOTT EMERGENCY ARCHIVE</color>";
            dhTmp.fontSize = 11.5f;

            // 8b. Directive Body
            GameObject dirBodyObj = new GameObject("DirectiveBody", typeof(RectTransform), typeof(TextMeshProUGUI));
            dirBodyObj.transform.SetParent(dirBox.transform, false);
            RectTransform dirBodyRect = dirBodyObj.GetComponent<RectTransform>();
            dirBodyRect.anchorMin = new Vector2(0f, 0f);
            dirBodyRect.anchorMax = new Vector2(1f, 1f);
            dirBodyRect.offsetMin = new Vector2(16f, 12f);
            dirBodyRect.offsetMax = new Vector2(-16f, -38f);

            directiveBodyText = dirBodyObj.GetComponent<TextMeshProUGUI>();
            directiveBodyText.fontSize = 12.5f;
            directiveBodyText.color = new Color(0.75f, 0.80f, 0.88f, 1f);
            directiveBodyText.alignment = TextAlignmentOptions.TopLeft;
            directiveBodyText.textWrappingMode = TextWrappingModes.Normal;
            directiveBodyText.text = "<i>\"Light attracts the infected in the dark. Keep your flashlight switched off unless necessary.\"</i>";
        }
    }
}
