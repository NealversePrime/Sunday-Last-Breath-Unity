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
    /// Controls the Disclaimer Screen passage (Disclaimer.twee).
    /// Displays content warnings, shows pulsing 'CLICK ANYWHERE TO CONTINUE' prompt,
    /// and advances to Title Screen upon user interaction.
    /// Implements IPointerClickHandler for universal input compatibility.
    /// </summary>
    public class DisclaimerScreen : ScreenBase, IPointerClickHandler
    {
        [Header("Disclaimer Elements")]
        [SerializeField] private float inputDelay = 0.5f;

        private bool canClick = false;
        private bool hasProceeded = false;

        protected override void OnScreenOpened()
        {
            canClick = false;
            hasProceeded = false;
            StartCoroutine(EnableInputAfterDelay());
        }

        private IEnumerator EnableInputAfterDelay()
        {
            yield return new WaitForSecondsRealtime(inputDelay);
            canClick = true;
        }

        private void Update()
        {
            if (!canClick || hasProceeded) return;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                OnContinueClicked();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.anyKeyDown)
            {
                OnContinueClicked();
            }
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnContinueClicked();
        }

        public void OnContinueClicked()
        {
            if (!canClick || hasProceeded) return;
            hasProceeded = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            Hide(0.6f, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GoToTitle();
                }
            });
        }
    }
}

