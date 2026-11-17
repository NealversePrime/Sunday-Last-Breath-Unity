using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Resident Evil Style Classic Door & Cinematic Transition Overlay.
    /// Plays full-screen MP4 videos, renders CRT vignette effects, audio crossfades,
    /// and title cards between zone boundaries and room doors.
    /// </summary>
    public class StageTransitionOverlay : MonoBehaviour
    {
        [Header("Stage Reference")]
        [SerializeField] private GameStageScreen stage;

        [Header("Door Exit Transition Settings & Audio")]
        [SerializeField] private bool doorTransitionFadeAmbience = true;
        [SerializeField] private bool doorTransitionPlayKeySFX = true;
        [SerializeField] private AudioClip doorTransitionKeySFX;
        [SerializeField] private bool doorTransitionPlayKnobSFX = true;
        [SerializeField] private AudioClip doorTransitionKnobSFX;
        [SerializeField] private bool doorTransitionPlayFootstepSFX = true;
        [SerializeField] private AudioClip doorTransitionFootstepSFX;
        [SerializeField] private bool doorTransitionPlaySlamSFX = true;
        [SerializeField] private AudioClip doorTransitionSlamSFX;

        [Header("Door Exit Transition Timing (Seconds)")]
        [SerializeField] private float doorTransitionFadeOutDuration = 0.6f;
        [SerializeField] private float doorTransitionTextToVideoDelay = 1.5f;
        [SerializeField] private float doorTransitionTextFadeOutDuration = 0.45f;
        [SerializeField] private float doorTransitionVideoFadeInDuration = 0.4f;
        [SerializeField] private float doorTransitionPostVideoPause = 0.4f;
        [SerializeField] private float doorTransitionFadeInDuration = 1.0f;

        public bool IsTransitioning => isDoorTransitioning;
        public GameObject OverlayObject => doorTransitionOverlay;

        private bool isDoorTransitioning = false;
        private GameObject doorTransitionOverlay;
        private CanvasGroup doorTransitionCanvasGroup;
        private TextMeshProUGUI doorTransitionTitleTmp;
        private TextMeshProUGUI doorTransitionDescTmp;
        private TextMeshProUGUI doorTransitionSkipPromptTmp;
        private GameObject doorTransitionContentBox;
        private CanvasGroup doorTransitionContentCanvasGroup;
        private UnityEngine.Video.VideoPlayer doorVideoPlayer;
        private RawImage doorVideoRawImage;
        private RenderTexture doorVideoRenderTexture;

        public void Initialize(GameStageScreen stageController)
        {
            stage = stageController;
            EnsureDoorTransitionOverlay();
        }

        private void OnDestroy()
        {
            if (doorVideoRenderTexture != null)
            {
                doorVideoRenderTexture.Release();
                Destroy(doorVideoRenderTexture);
                doorVideoRenderTexture = null;
            }
        }

        public bool CheckDoorTransitionSkipInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.anyKey.wasPressedThisFrame) return true;
            }
            if (Mouse.current != null)
            {
                var m = Mouse.current;
                if (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame) return true;
            }
            if (Gamepad.current != null)
            {
                var pad = Gamepad.current;
                if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame) return true;
            }
#endif
            try
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0)) return true;
            }
            catch (System.InvalidOperationException) { }
            return false;
        }

        public void EnsureDoorTransitionOverlay()
        {
            if (doorTransitionOverlay != null)
            {
                if (stage != null && doorTransitionOverlay.transform.parent != stage.transform)
                {
                    doorTransitionOverlay.transform.SetParent(stage.transform, false);
                }
                return;
            }

            doorTransitionOverlay = new GameObject("DoorTransitionOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            doorTransitionOverlay.transform.SetParent(stage != null ? stage.transform : transform, false);
            doorTransitionOverlay.transform.SetAsLastSibling();

            RectTransform rt = doorTransitionOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = doorTransitionOverlay.GetComponent<Image>();
            img.color = new Color(0.015f, 0.02f, 0.035f, 1f); // Deep pitch black
            img.raycastTarget = true;

            doorTransitionCanvasGroup = doorTransitionOverlay.GetComponent<CanvasGroup>();
            doorTransitionCanvasGroup.alpha = 0f;

            // Resident Evil Classic Door Video Player (RawImage + RenderTexture)
            GameObject vidObj = new GameObject("DoorVideoDisplay", typeof(RectTransform), typeof(RawImage));
            vidObj.transform.SetParent(doorTransitionOverlay.transform, false);

            RectTransform vidRt = vidObj.GetComponent<RectTransform>();
            vidRt.anchorMin = Vector2.zero;
            vidRt.anchorMax = Vector2.one;
            vidRt.offsetMin = Vector2.zero;
            vidRt.offsetMax = Vector2.zero;

            doorVideoRawImage = vidObj.GetComponent<RawImage>();
            doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f); // Hidden initially until smooth fade-in
            doorVideoRawImage.raycastTarget = false;

            if (doorVideoRenderTexture == null)
            {
                doorVideoRenderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                doorVideoRenderTexture.Create();
            }

            doorVideoRawImage.texture = doorVideoRenderTexture;

            doorVideoPlayer = doorTransitionOverlay.AddComponent<UnityEngine.Video.VideoPlayer>();
            doorVideoPlayer.playOnAwake = false;
            doorVideoPlayer.isLooping = false;
            doorVideoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            doorVideoPlayer.targetTexture = doorVideoRenderTexture;
            doorVideoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.Direct;

            // Try loading VideoClip or file path (unit407_door_transition with fallback to legacy room403)
            var clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/unit407_door_transition");
            if (clip == null) clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/room403_door_transition");

            if (clip != null)
            {
                doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                doorVideoPlayer.clip = clip;
            }
            else
            {
                string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "unit407_door_transition.mp4").Replace("\\", "/");
                if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");
                if (System.IO.File.Exists(filePath))
                {
                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                    doorVideoPlayer.url = filePath;
                }
            }

            // Centered content box (Narrative Subtitle / Fallback) with its own CanvasGroup for smooth dissolves
            doorTransitionContentBox = new GameObject("ContentBox", typeof(RectTransform), typeof(CanvasGroup));
            doorTransitionContentBox.transform.SetParent(doorTransitionOverlay.transform, false);

            doorTransitionContentCanvasGroup = doorTransitionContentBox.GetComponent<CanvasGroup>();
            doorTransitionContentCanvasGroup.alpha = 1f;

            RectTransform boxRt = doorTransitionContentBox.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.12f, 0.08f);
            boxRt.anchorMax = new Vector2(0.88f, 0.28f);
            boxRt.offsetMin = Vector2.zero;
            boxRt.offsetMax = Vector2.zero;

            // Title / Header
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(doorTransitionContentBox.transform, false);

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.55f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            doorTransitionTitleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            doorTransitionTitleTmp.fontSize = 17f;
            doorTransitionTitleTmp.fontStyle = FontStyles.Bold;
            doorTransitionTitleTmp.alignment = TextAlignmentOptions.Center;
            doorTransitionTitleTmp.color = new Color(0.0f, 0.92f, 1.0f); // Bright Cyan
            doorTransitionTitleTmp.text = "[ UNIT 407 // FRONT DEADBOLT ]";

            // Narrative description
            GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(doorTransitionContentBox.transform, false);

            RectTransform descRt = descObj.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 0.55f);
            descRt.offsetMin = Vector2.zero;
            descRt.offsetMax = Vector2.zero;

            doorTransitionDescTmp = descObj.GetComponent<TextMeshProUGUI>();
            doorTransitionDescTmp.fontSize = 14f;
            doorTransitionDescTmp.lineSpacing = 14f;
            doorTransitionDescTmp.alignment = TextAlignmentOptions.Center;
            doorTransitionDescTmp.color = new Color(0.88f, 0.90f, 0.94f);
            doorTransitionDescTmp.text = "You turn the brass key in the deadbolt. The lock mechanism clunks open with a heavy metallic click. You take a breath, push the door open, and step out into the 4th floor corridor.";

            // Subtle Skip Prompt in bottom-right corner
            GameObject skipObj = new GameObject("SkipPromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
            skipObj.transform.SetParent(doorTransitionOverlay.transform, false);

            RectTransform skipRt = skipObj.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.5f, 0.02f);
            skipRt.anchorMax = new Vector2(0.98f, 0.08f);
            skipRt.offsetMin = Vector2.zero;
            skipRt.offsetMax = Vector2.zero;

            doorTransitionSkipPromptTmp = skipObj.GetComponent<TextMeshProUGUI>();
            doorTransitionSkipPromptTmp.fontSize = 12f;
            doorTransitionSkipPromptTmp.fontStyle = FontStyles.Bold;
            doorTransitionSkipPromptTmp.alignment = TextAlignmentOptions.BottomRight;
            doorTransitionSkipPromptTmp.color = new Color(0.40f, 0.48f, 0.58f, 0.75f);
            doorTransitionSkipPromptTmp.text = "<color=#64748B>[SPACE / CLICK TO SKIP]</color>";
            skipObj.SetActive(false);

            doorTransitionOverlay.SetActive(false);
        }

        public void PlayApartmentExitTransition()
        {
            if (isDoorTransitioning) return;
            StartCoroutine(DoorExitTransitionRoutine());
        }

        private IEnumerator DoorExitTransitionRoutine()
        {
            isDoorTransitioning = true;
            EnsureDoorTransitionOverlay();
            if (stage != null && doorTransitionOverlay.transform.parent != stage.transform)
            {
                doorTransitionOverlay.transform.SetParent(stage.transform, false);
            }
            doorTransitionOverlay.transform.SetAsLastSibling();
            doorTransitionOverlay.SetActive(true);
            doorTransitionCanvasGroup.alpha = 0f;
            doorTransitionContentBox?.SetActive(true);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

            // Refresh video source in case of dynamic file addition
            if (doorVideoPlayer != null)
            {
                if (doorVideoPlayer.clip == null && string.IsNullOrEmpty(doorVideoPlayer.url))
                {
                    var c = Resources.Load<UnityEngine.Video.VideoClip>("Videos/unit407_door_transition");
                    if (c == null) c = Resources.Load<UnityEngine.Video.VideoClip>("Videos/room403_door_transition");
                    if (c != null)
                    {
                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        doorVideoPlayer.clip = c;
                    }
                    else
                    {
                        string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "unit407_door_transition.mp4").Replace("\\", "/");
                        if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", "room403_door_transition.mp4").Replace("\\", "/");
                        if (System.IO.File.Exists(filePath))
                        {
                            doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                            doorVideoPlayer.url = filePath;
                        }
                    }
                }
                doorVideoPlayer.Prepare();
            }

            // Step 1: Smooth fade to black with key insertion SFX
            doorTransitionTitleTmp.text = "[ UNIT 407 // UNLOCKING FRONT DEADBOLT ]";
            doorTransitionDescTmp.text = "You slide the heavy brass key into the cylinder.\n<color=#94A3B8>The mechanical tumblers align with a sharp metallic click.</color>";

            if (doorTransitionPlayKeySFX && stage != null)
            {
                if (doorTransitionKeySFX != null) stage.PlaySFX(doorTransitionKeySFX, 0.9f);
                else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.9f);
            }

            float fadeTime = Mathf.Max(0.05f, doorTransitionFadeOutDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 1f;

            // Player reads the narrative text
            if (doorTransitionTextToVideoDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(doorTransitionTextToVideoDelay);
            }

            // Smoothly dissolve / fade out the narrative text box into deep darkness
            float textFadeTime = Mathf.Max(0.05f, doorTransitionTextFadeOutDuration);
            if (doorTransitionContentCanvasGroup != null)
            {
                for (float t = 0; t < textFadeTime; t += Time.unscaledDeltaTime)
                {
                    doorTransitionContentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / textFadeTime);
                    yield return null;
                }
                doorTransitionContentCanvasGroup.alpha = 0f;
            }
            doorTransitionContentBox?.SetActive(false);

            // Brief cinematic breath in pitch black darkness
            yield return new WaitForSecondsRealtime(0.15f);

            // Step 2: Play the Resident Evil Door Opening Video!
            bool playedVideo = false;
            if (doorVideoPlayer != null && (doorVideoPlayer.clip != null || !string.IsNullOrEmpty(doorVideoPlayer.url)))
            {
                // Wait for video prepare if needed (with 1.5s max timeout)
                float prepTimer = 0f;
                while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)
                {
                    prepTimer += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (doorVideoPlayer.isPrepared)
                {
                    playedVideo = true;
                    if (doorTransitionPlayKnobSFX && stage != null)
                    {
                        if (doorTransitionKnobSFX != null) stage.PlaySFX(doorTransitionKnobSFX, 0.95f);
                        else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                    }

                    if (doorTransitionFadeAmbience && stage != null && stage.AtmosphereSource != null && stage.AtmosphereSource.isPlaying)
                    {
                        StartCoroutine(FadeOutAudio(stage.AtmosphereSource, 0.8f));
                    }

                    doorVideoPlayer.Play();

                    // Smoothly fade in the video from black
                    float vidFadeIn = Mathf.Max(0.05f, doorTransitionVideoFadeInDuration);
                    for (float t = 0; t < vidFadeIn; t += Time.unscaledDeltaTime)
                    {
                        float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);
                        yield return null;
                    }
                    if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;

                    // Show skip prompt
                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);

                    // Track video playback with instant skip support
                    float vidLength = (float)doorVideoPlayer.length;
                    if (vidLength <= 0.5f) vidLength = 4.5f;
                    float midTime = vidLength * 0.55f;
                    float elapsed = 0.2f + vidFadeIn;
                    bool playedFootstep = false;
                    bool skipped = false;

                    while (elapsed < vidLength - 0.35f)
                    {
                        if (CheckDoorTransitionSkipInput())
                        {
                            skipped = true;
                            break;
                        }

                        if (!playedFootstep && elapsed >= midTime)
                        {
                            playedFootstep = true;
                            if (doorTransitionPlayFootstepSFX && stage != null)
                            {
                                if (doorTransitionFootstepSFX != null) stage.PlaySFX(doorTransitionFootstepSFX, 0.8f);
                                else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);
                            }
                        }

                        elapsed += Time.unscaledDeltaTime;
                        yield return null;
                    }

                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);

                    if (skipped)
                    {
                        if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
                        if (doorTransitionPlaySlamSFX && stage != null)
                        {
                            if (doorTransitionSlamSFX != null) stage.PlaySFX(doorTransitionSlamSFX, 0.75f);
                            else stage.PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.75f);
                        }
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
                        yield return new WaitForSecondsRealtime(0.05f);
                    }
                    else
                    {
                        if (doorTransitionPlaySlamSFX && stage != null)
                        {
                            if (doorTransitionSlamSFX != null) stage.PlaySFX(doorTransitionSlamSFX, 0.65f);
                            else stage.PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);
                        }

                        // Smoothly fade video out to darkness as door slams shut
                        float vidFadeOut = 0.35f;
                        for (float t = 0; t < vidFadeOut; t += Time.unscaledDeltaTime)
                        {
                            float a = Mathf.Lerp(1f, 0f, t / vidFadeOut);
                            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);
                            yield return null;
                        }
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

                        if (doorTransitionPostVideoPause > 0f)
                        {
                            yield return new WaitForSecondsRealtime(doorTransitionPostVideoPause);
                        }
                    }
                }
            }

            // Fallback narrative sequence if video could not play
            if (!playedVideo)
            {
                doorTransitionContentBox?.SetActive(true);
                if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;

                doorTransitionTitleTmp.text = "[ UNIT 407 // DISENGAGING DEADBOLT ]";
                doorTransitionDescTmp.text = "<i>*CLACK-CHUNK*</i>\n\nThe heavy steel deadbolt disengages with a hollow metallic snap.\nYou grip the cold brass handle and ease the door open.";

                if (doorTransitionPlayKnobSFX && stage != null)
                {
                    if (doorTransitionKnobSFX != null) stage.PlaySFX(doorTransitionKnobSFX, 0.95f);
                    else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                }

                if (doorTransitionFadeAmbience && stage != null && stage.AtmosphereSource != null && stage.AtmosphereSource.isPlaying)
                {
                    StartCoroutine(FadeOutAudio(stage.AtmosphereSource, 0.8f));
                }

                yield return new WaitForSecondsRealtime(2.0f);

                doorTransitionTitleTmp.text = "[ 4TH FLOOR CORRIDOR // THE THRESHOLD ]";
                doorTransitionDescTmp.text = "You take a breath of freezing, copper-scented air and step across the threshold.\nBehind you, the heavy door swings shut.\n\n<color=#EF4444>The spring-latch snaps into the strike plate.</color>\n<color=#94A3B8>The deadbolt locks from the outside. You are out in the corridor with no turning back.</color>";

                if (doorTransitionPlayFootstepSFX && stage != null)
                {
                    if (doorTransitionFootstepSFX != null) stage.PlaySFX(doorTransitionFootstepSFX, 0.8f);
                    else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);
                }

                yield return new WaitForSecondsRealtime(1.0f);

                if (doorTransitionPlaySlamSFX && stage != null)
                {
                    if (doorTransitionSlamSFX != null) stage.PlaySFX(doorTransitionSlamSFX, 0.65f);
                    else stage.PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);
                }

                yield return new WaitForSecondsRealtime(2.0f);
            }

            // Step 3: Switch to Hallway Mode behind the black screen
            if (stage != null)
            {
                if (stage.HallwayStartingPassage != null)
                {
                    stage.PlayStoryPassage(stage.HallwayStartingPassage);
                }
                else
                {
                    stage.EnterHallwayMode(1);
                }
            }

            // Step 4: Fade in from black to reveal the red 4th floor corridor
            fadeTime = Mathf.Max(0.05f, doorTransitionFadeInDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 0f;

            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;
            doorTransitionContentBox?.SetActive(true);
            doorTransitionOverlay.SetActive(false);
            isDoorTransitioning = false;
        }

        public IEnumerator FadeOutAudio(AudioSource src, float duration)
        {
            if (src == null) yield break;
            float startVol = src.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (src == null) yield break;
                src.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            if (src != null) src.Stop();
        }

        public void PlayDoorTransition(string videoResourcePath, string roomTitle, string narrativeSubtitle, bool playKeySFX, System.Action onReachedDestination, bool playSlam = false)
        {
            if (isDoorTransitioning)
            {
                onReachedDestination?.Invoke();
                return;
            }

            StartCoroutine(GenericDoorTransitionRoutine(videoResourcePath, roomTitle, narrativeSubtitle, playKeySFX, onReachedDestination, playSlam));
        }

        private IEnumerator GenericDoorTransitionRoutine(string videoResourcePath, string roomTitle, string narrativeSubtitle, bool playKeySFX, System.Action onReachedDestination, bool playSlam = false)
        {
            isDoorTransitioning = true;
            EnsureDoorTransitionOverlay();
            if (stage != null && doorTransitionOverlay.transform.parent != stage.transform)
            {
                doorTransitionOverlay.transform.SetParent(stage.transform, false);
            }
            doorTransitionOverlay.transform.SetAsLastSibling();
            doorTransitionOverlay.SetActive(true);
            doorTransitionCanvasGroup.alpha = 0f;
            doorTransitionContentBox?.SetActive(true);

            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

            // Configure video if supplied
            bool hasVideo = false;
            if (!string.IsNullOrEmpty(videoResourcePath) && doorVideoPlayer != null)
            {
                string cleanRes = videoResourcePath;
                if (cleanRes.StartsWith("Videos/")) cleanRes = cleanRes.Substring(7);
                var clip = Resources.Load<UnityEngine.Video.VideoClip>("Videos/" + cleanRes);

                if (clip != null)
                {
                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    doorVideoPlayer.clip = clip;
                    hasVideo = true;
                }
                else
                {
                    string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", "Videos", cleanRes + ".mp4").Replace("\\", "/");
                    if (System.IO.File.Exists(filePath))
                    {
                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                        doorVideoPlayer.url = filePath;
                        hasVideo = true;
                    }
                }

                if (hasVideo) doorVideoPlayer.Prepare();
            }

            // Step 1: Smooth fade to black with audio
            doorTransitionTitleTmp.text = roomTitle;
            doorTransitionDescTmp.text = narrativeSubtitle;

            if (playKeySFX && stage != null)
            {
                if (doorTransitionKeySFX != null) stage.PlaySFX(doorTransitionKeySFX, 0.9f);
                else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.9f);
            }

            float fadeTime = Mathf.Max(0.05f, doorTransitionFadeOutDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 1f;

            if (doorTransitionTextToVideoDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(doorTransitionTextToVideoDelay);
            }

            float textFadeTime = Mathf.Max(0.05f, doorTransitionTextFadeOutDuration);
            if (doorTransitionContentCanvasGroup != null)
            {
                for (float t = 0; t < textFadeTime; t += Time.unscaledDeltaTime)
                {
                    doorTransitionContentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / textFadeTime);
                    yield return null;
                }
                doorTransitionContentCanvasGroup.alpha = 0f;
            }
            doorTransitionContentBox?.SetActive(false);

            yield return new WaitForSecondsRealtime(0.12f);

            // Step 2: Play Video if available
            bool playedVideo = false;
            if (hasVideo && doorVideoPlayer != null)
            {
                float prepTimer = 0f;
                while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)
                {
                    prepTimer += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (doorVideoPlayer.isPrepared)
                {
                    playedVideo = true;
                    if (doorTransitionPlayKnobSFX && stage != null)
                    {
                        if (doorTransitionKnobSFX != null) stage.PlaySFX(doorTransitionKnobSFX, 0.95f);
                        else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                    }

                    if (doorTransitionFadeAmbience && stage != null && stage.AtmosphereSource != null && stage.AtmosphereSource.isPlaying)
                    {
                        StartCoroutine(FadeOutAudio(stage.AtmosphereSource, 0.8f));
                    }

                    doorVideoPlayer.Play();

                    float vidFadeIn = Mathf.Max(0.05f, doorTransitionVideoFadeInDuration);
                    for (float t = 0; t < vidFadeIn; t += Time.unscaledDeltaTime)
                    {
                        float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);
                        yield return null;
                    }
                    if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;

                    // Show skip prompt
                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);

                    float vidLength = (float)doorVideoPlayer.length;
                    if (vidLength <= 0.5f) vidLength = 4.5f;
                    float midTime = vidLength * 0.55f;
                    float elapsed = 0.2f + vidFadeIn;
                    bool playedFootstep = false;
                    bool skipped = false;

                    while (elapsed < vidLength - 0.35f)
                    {
                        if (CheckDoorTransitionSkipInput())
                        {
                            skipped = true;
                            break;
                        }

                        if (!playedFootstep && elapsed >= midTime)
                        {
                            playedFootstep = true;
                            if (doorTransitionPlayFootstepSFX && stage != null)
                            {
                                if (doorTransitionFootstepSFX != null) stage.PlaySFX(doorTransitionFootstepSFX, 0.8f);
                                else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.8f);
                            }
                        }

                        elapsed += Time.unscaledDeltaTime;
                        yield return null;
                    }

                    if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);

                    if (skipped)
                    {
                        if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
                        if (playSlam && doorTransitionPlaySlamSFX && stage != null)
                        {
                            if (doorTransitionSlamSFX != null) stage.PlaySFX(doorTransitionSlamSFX, 0.75f);
                            else stage.PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.75f);
                        }
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
                        yield return new WaitForSecondsRealtime(0.05f);
                    }
                    else
                    {
                        if (playSlam && doorTransitionPlaySlamSFX && stage != null)
                        {
                            if (doorTransitionSlamSFX != null) stage.PlaySFX(doorTransitionSlamSFX, 0.65f);
                            else stage.PlaySFX("Audio/SFX/firedoor_slam.mp3", 0.65f);
                        }

                        float vidFadeOut = 0.35f;
                        for (float t = 0; t < vidFadeOut; t += Time.unscaledDeltaTime)
                        {
                            float a = Mathf.Lerp(1f, 0f, t / vidFadeOut);
                            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);
                            yield return null;
                        }
                        if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);

                        if (doorTransitionPostVideoPause > 0f)
                        {
                            yield return new WaitForSecondsRealtime(doorTransitionPostVideoPause);
                        }
                    }
                }
            }

            // Fallback audio if video wasn't played
            if (!playedVideo)
            {
                if (doorTransitionPlayKnobSFX && stage != null)
                {
                    if (doorTransitionKnobSFX != null) stage.PlaySFX(doorTransitionKnobSFX, 0.95f);
                    else stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);
                }
                yield return new WaitForSecondsRealtime(0.6f);
            }

            // Step 3: Switch scene/room behind the black curtain!
            onReachedDestination?.Invoke();

            // Step 4: Fade in to reveal the room
            fadeTime = Mathf.Max(0.05f, doorTransitionFadeInDuration);
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }
            doorTransitionCanvasGroup.alpha = 0f;

            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
            if (doorTransitionContentCanvasGroup != null) doorTransitionContentCanvasGroup.alpha = 1f;
            doorTransitionContentBox?.SetActive(true);
            doorTransitionOverlay.SetActive(false);
            isDoorTransitioning = false;
        }

        public IEnumerator PlayPassageVideoTransition(SundayLastBreath.Data.PassageChoiceData choice, SundayLastBreath.Data.StoryPassageData targetPassage)
        {
            if (targetPassage == null || choice == null) yield break;

            EnsureDoorTransitionOverlay();
            if (doorTransitionOverlay == null || doorVideoPlayer == null)
            {
                stage?.PlayStoryPassage(targetPassage);
                yield break;
            }

            var clip = choice.transitionVideo;
            var path = choice.transitionVideoPath;

            bool hasVideo = false;
            if (clip != null)
            {
                doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                doorVideoPlayer.clip = clip;
                hasVideo = true;
            }
            else if (!string.IsNullOrEmpty(path))
            {
                var loaded = Resources.Load<UnityEngine.Video.VideoClip>(path);
                if (loaded != null)
                {
                    doorVideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    doorVideoPlayer.clip = loaded;
                    hasVideo = true;
                }
                else
                {
                    string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", path).Replace("\\", "/");
                    if (!filePath.EndsWith(".mp4")) filePath += ".mp4";
                    if (System.IO.File.Exists(filePath))
                    {
                        doorVideoPlayer.source = UnityEngine.Video.VideoSource.Url;
                        doorVideoPlayer.url = filePath;
                        hasVideo = true;
                    }
                }
            }

            if (!hasVideo)
            {
                stage?.PlayStoryPassage(targetPassage);
                yield break;
            }

            if (choice.fadeOutBgmDuringTransition && stage != null && stage.AtmosphereSource != null && stage.AtmosphereSource.isPlaying)
            {
                stage.FadeAtmosphereAudioRoutine(0.05f, 0.4f);
            }

            isDoorTransitioning = true;
            if (stage != null && doorTransitionOverlay.transform.parent != stage.transform)
            {
                doorTransitionOverlay.transform.SetParent(stage.transform, false);
            }
            doorTransitionOverlay.transform.SetAsLastSibling();
            doorTransitionOverlay.SetActive(true);
            doorTransitionContentBox?.SetActive(false);
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 0f;

            doorVideoPlayer.Prepare();

            float fadeInTime = 0.35f;
            for (float t = 0f; t < fadeInTime; t += Time.unscaledDeltaTime)
            {
                if (doorTransitionCanvasGroup != null)
                    doorTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
                yield return null;
            }
            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 1f;

            float prepTimer = 0f;
            while (!doorVideoPlayer.isPrepared && prepTimer < 1.5f)
            {
                prepTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (doorVideoPlayer.isPrepared)
            {
                if (choice.transitionStartSFX != null && stage != null) stage.PlaySFX(choice.transitionStartSFX, 0.95f);
                else if (doorTransitionKnobSFX != null && stage != null) stage.PlaySFX(doorTransitionKnobSFX, 0.95f);
                else if (stage != null) stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.95f);

                doorVideoPlayer.Play();

                float vidFadeIn = choice.videoFadeInDuration > 0f ? choice.videoFadeInDuration : 0.35f;
                for (float t = 0f; t < vidFadeIn; t += Time.unscaledDeltaTime)
                {
                    float a = Mathf.Lerp(0f, 1f, t / vidFadeIn);
                    if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, a);
                    yield return null;
                }
                if (doorVideoRawImage != null) doorVideoRawImage.color = Color.white;

                if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(true);

                float vidLength = (float)doorVideoPlayer.length;
                if (vidLength <= 0.5f) vidLength = 3.5f;
                float elapsed = 0.25f;
                bool skipped = false;

                while (elapsed < vidLength - 0.2f)
                {
                    if (CheckDoorTransitionSkipInput())
                    {
                        skipped = true;
                        break;
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (doorTransitionSkipPromptTmp != null) doorTransitionSkipPromptTmp.gameObject.SetActive(false);

                if (skipped)
                {
                    if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
                }
                else
                {
                    for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
                    {
                        if (doorVideoRawImage != null)
                            doorVideoRawImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t / 0.25f));
                        yield return null;
                    }
                }
            }

            stage?.PlayStoryPassage(targetPassage);

            float fadeOutTime = 0.35f;
            for (float t = 0f; t < fadeOutTime; t += Time.unscaledDeltaTime)
            {
                if (doorTransitionCanvasGroup != null)
                    doorTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
                yield return null;
            }

            if (doorTransitionCanvasGroup != null) doorTransitionCanvasGroup.alpha = 0f;
            if (doorVideoPlayer != null && doorVideoPlayer.isPlaying) doorVideoPlayer.Stop();
            if (doorVideoRawImage != null) doorVideoRawImage.color = new Color(1f, 1f, 1f, 0f);
            doorTransitionOverlay.SetActive(false);
            isDoorTransitioning = false;
        }
    }
}
