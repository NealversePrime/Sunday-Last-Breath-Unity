using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Recreates the custom Twine CSS hover effects for main menu buttons:
    /// - Smooth horizontal slide (+15px)
    /// - Gold highlight color (#D35400)
    /// - Left border accent indicator
    /// - Retro UI audio feedback
    /// </summary>
    public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image leftAccentBar;
        [SerializeField] private Image backgroundGradient;

        [Header("Styling")]
        [SerializeField] private Color normalTextColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        [SerializeField] private Color hoverTextColor = new Color(0.83f, 0.33f, 0f, 1f); // #D35400 Gold/Orange accent
        [SerializeField] private float slideDistance = 15f;
        [SerializeField] private float transitionDuration = 0.2f;

        private Vector2 originalPosition;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

            originalPosition = rectTransform.anchoredPosition;

            ResetVisuals();
        }

        private void OnEnable()
        {
            ResetVisuals();
        }

        private void ResetVisuals()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPosition;
            }

            if (buttonText != null)
            {
                buttonText.color = normalTextColor;
            }

            if (leftAccentBar != null)
            {
                leftAccentBar.gameObject.SetActive(false);
            }

            if (backgroundGradient != null)
            {
                backgroundGradient.gameObject.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }

            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(AnimateHover(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(AnimateHover(false));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        private IEnumerator AnimateHover(bool isHovered)
        {
            Vector2 targetPos = originalPosition + (isHovered ? new Vector2(slideDistance, 0f) : Vector2.zero);
            Vector2 startPos = rectTransform.anchoredPosition;
            Color targetColor = isHovered ? hoverTextColor : normalTextColor;
            Color startColor = buttonText != null ? buttonText.color : normalTextColor;

            if (leftAccentBar != null) leftAccentBar.gameObject.SetActive(isHovered);
            if (backgroundGradient != null) backgroundGradient.gameObject.SetActive(isHovered);

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionDuration;

                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                if (buttonText != null)
                {
                    buttonText.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            rectTransform.anchoredPosition = targetPos;
            if (buttonText != null) buttonText.color = targetColor;
        }
    }
}

