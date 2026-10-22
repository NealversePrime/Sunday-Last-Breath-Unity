using System.Collections;
using UnityEngine;

namespace SundayLastBreath.Core
{
    /// <summary>
    /// Global Audio Manager handling Background Music (with crossfading) and Sound Effects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Default Clips")]
        [SerializeField] private AudioClip menuMusicClip;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip buttonHoverClip;

        [Header("Volume Controls (0 to 1)")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

        private Coroutine bgmFadeCoroutine;

        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            EnsureAudioListener();
        }

        private void EnsureAudioListener()
        {
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    mainCam.gameObject.AddComponent<AudioListener>();
                    Debug.Log("[AudioManager] Auto-attached AudioListener to Main Camera.");
                }
                else
                {
                    gameObject.AddComponent<AudioListener>();
                    Debug.Log("[AudioManager] Auto-attached AudioListener to AudioManager.");
                }
            }
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                GameObject bgmObj = new GameObject("BGM_Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f; // Pure 2D audio
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX_Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // Pure 2D audio
            }
        }

        public void PlayMenuMusic(float fadeDuration = 1.0f)
        {
            if (menuMusicClip != null)
            {
                PlayBGM(menuMusicClip, fadeDuration);
            }
            else
            {
                Debug.LogWarning("[AudioManager] menuMusicClip is not assigned!");
            }
        }

        public void PlayBGM(AudioClip clip, float fadeDuration = 1f)
        {
            if (clip == null) return;
            if (bgmSource.isPlaying && bgmSource.clip == clip) return;

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(clip, fadeDuration));
        }

        public void StopBGM(float fadeDuration = 1f)
        {
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeOutBGM(fadeDuration));
        }

        private IEnumerator FadeBGM(AudioClip newClip, float duration)
        {
            EnsureAudioListener();
            float targetVol = masterVolume * bgmVolume;

            if (bgmSource.isPlaying && duration > 0.1f)
            {
                float startVol = bgmSource.volume;
                float halfDuration = duration * 0.5f;
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                    yield return null;
                }
            }

            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();
            Debug.Log($"[AudioManager] Playing BGM: '{newClip.name}', Target Volume: {targetVol:F2}");

            if (duration > 0.1f)
            {
                float halfDuration = duration * 0.5f;
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(0f, targetVol, t / halfDuration);
                    yield return null;
                }
            }

            bgmSource.volume = targetVol;
        }

        private IEnumerator FadeOutBGM(float duration)
        {
            if (!bgmSource.isPlaying) yield break;

            float startVol = bgmSource.volume;
            if (duration > 0.1f)
            {
                for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = null;
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
        }

        public void PlayButtonClick()
        {
            if (buttonClickClip != null) PlaySFX(buttonClickClip, 0.4f);
        }

        public void PlayButtonHover()
        {
            // Muted per design preference (Option 1): hover uses visual highlight/slide only, reserving SFX strictly for clicks.
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.volume = masterVolume * bgmVolume;
            }
        }
    }
}

