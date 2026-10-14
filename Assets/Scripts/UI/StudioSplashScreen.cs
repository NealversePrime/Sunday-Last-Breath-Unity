using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using SundayLastBreath.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Controls the Studio Splash Screen passage (StudioSplash.twee).
    /// Plays studio logo animation, displays 'PRESENTS', and auto-advances to Disclaimer after 4.2 seconds.
    /// Allows clicking or pressing Space to skip.
    /// Implements IPointerClickHandler for universal input compatibility.
    /// </summary>
    public class StudioSplashScreen : ScreenBase, IPointerClickHandler
    {
        [Header("Studio Elements")]
        [SerializeField] private FrameAnimationPlayer logoAnimationPlayer;
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private float displayDuration = 4.2f;

        private Coroutine timerCoroutine;
        private bool hasSkipped = false;

        protected override void OnScreenOpened()
        {
            hasSkipped = false;

            if (logoAnimationPlayer != null)
            {
                logoAnimationPlayer.Play();
            }

            timerCoroutine = StartCoroutine(SplashSequenceRoutine());
        }

        private IEnumerator SplashSequenceRoutine()
        {
            float elapsed = 0f;
            Vector3 startScale = new Vector3(0.97f, 0.97f, 1f);
            Vector3 endScale = new Vector3(1.02f, 1.02f, 1f);

            while (elapsed < displayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / displayDuration;

                if (logoTransform != null)
                {
                    logoTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
                }

                // Check keyboard skip
                if (CheckKeyboardInput())
                {
                    Skip();
                    yield break;
                }

                yield return null;
            }

            ProceedToNext();
        }

        private bool CheckKeyboardInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                return true;
#endif
            return false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Skip();
        }

        public void Skip()
        {
            if (hasSkipped) return;
            hasSkipped = true;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }

            ProceedToNext();
        }

        private void ProceedToNext()
        {
            Hide(0.6f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GoToDisclaimer();
                }
            });
        }
    }
}

