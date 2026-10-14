using System;
using System.Collections;
using UnityEngine;

namespace SundayLastBreath.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenBase : MonoBehaviour
    {
        [Header("Screen Settings")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float defaultFadeDuration = 0.5f;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null)
                {
                    canvasGroup = GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }
                return canvasGroup;
            }
        }

        protected virtual void Awake()
        {
            _ = CanvasGroup;
        }

        public virtual void Show(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0f) duration = defaultFadeDuration;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f, 1f, duration, () =>
            {
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
                OnScreenOpened();
                onComplete?.Invoke();
            }));
        }

        public virtual void Hide(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0f) duration = defaultFadeDuration;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(CanvasGroup.alpha, 0f, duration, () =>
            {
                gameObject.SetActive(false);
                OnScreenClosed();
                onComplete?.Invoke();
            }));
        }

        public virtual void ShowInstant()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            OnScreenOpened();
        }

        public virtual void HideInstant()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnScreenClosed();
        }

        protected virtual void OnScreenOpened() { }
        protected virtual void OnScreenClosed() { }

        protected IEnumerator FadeRoutine(float from, float to, float duration, Action onFinish)
        {
            if (duration <= 0f)
            {
                CanvasGroup.alpha = to;
                onFinish?.Invoke();
                yield break;
            }

            CanvasGroup.alpha = from;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                CanvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            CanvasGroup.alpha = to;
            onFinish?.Invoke();
        }
    }
}

