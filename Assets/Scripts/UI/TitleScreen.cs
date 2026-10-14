using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controls the Title Screen / Main Menu passage (Title.twee).
    /// Plays main menu BGM, loops title logo & background animation,
    /// and handles New Game, Load Game, Settings, and Quit options.
    /// </summary>
    public class TitleScreen : ScreenBase
    {
        [Header("Animation Players")]
        [SerializeField] private FrameAnimationPlayer backgroundAnimationPlayer;
        [SerializeField] private FrameAnimationPlayer titleLogoAnimationPlayer;

        [Header("Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Modals")]
        [SerializeField] private GameObject settingsModal;

        protected override void Awake()
        {
            base.Awake();

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        public override void Show(float duration = -1, System.Action onComplete = null)
        {
            // Start Menu Music immediately when screen begins opening
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            base.Show(duration, onComplete);
        }

        protected override void OnScreenOpened()
        {
            // Ensure Menu Music is playing
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            if (backgroundAnimationPlayer != null)
            {
                backgroundAnimationPlayer.Play();
            }

            if (titleLogoAnimationPlayer != null)
            {
                titleLogoAnimationPlayer.Play();
            }
        }

        private void OnNewGameClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM(1f);
            }

            Hide(0.8f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartNewGame();
                }
            });
        }

        private void OnLoadGameClicked()
        {
            Debug.Log("[TitleScreen] Load Game clicked.");
            // Open Save/Load system
        }

        private void OnSettingsClicked()
        {
            if (settingsModal != null)
            {
                settingsModal.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[TitleScreen] Quitting game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

