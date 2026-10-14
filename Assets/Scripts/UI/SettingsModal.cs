using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    public class SettingsModal : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Display Settings")]
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Controls")]
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (masterSlider != null)
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (bgmSlider != null)
                bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }

        private void OnEnable()
        {
            if (AudioManager.Instance != null)
            {
                if (masterSlider != null) masterSlider.value = AudioManager.Instance.MasterVolume;
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BgmVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SfxVolume;
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
        }

        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public void Close()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            gameObject.SetActive(false);
        }
    }
}

