using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Implements high-polish tactical choice button behavior matching the original Twine UX:
    /// - 10px smooth rightward hover slide
    /// - Neon border glow & background brightening
    /// - Sweeping sheen reflection across the card
    /// - Tactile scale dip on click
    /// - Audio triggers for hover and click
    /// </summary>
    public class TacticalChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private RectTransform textContainer;
        [SerializeField] private Image background;
        [SerializeField] private Image accentBar;
        [SerializeField] private Outline outline;
        [SerializeField] private RectTransform shineSweep;

        [Header("Colors & Styling")]
        [SerializeField] private Color baseBgColor = new Color(0.08f, 0.09f, 0.11f, 0.98f);
        [SerializeField] private Color hoverBgColor = new Color(0.14f, 0.17f, 0.22f, 1f);
        [SerializeField] private Color baseOutlineColor = new Color(0.24f, 0.18f, 0.18f, 0.85f);
        [SerializeField] private Color hoverOutlineColor = new Color(0.35f, 0.75f, 0.95f, 0.95f);
        [SerializeField] private Color baseAccentColor = new Color(0.95f, 0.75f, 0.35f);
        [SerializeField] private Color hoverAccentColor = new Color(0.40f, 0.85f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float slideDistance = 10f;
        [SerializeField] private float transitionSpeed = 14f;

        private float defaultLeftOffset = 18f;
        private float targetLeftOffset = 18f;
        private float currentLeftOffset = 18f;
        private bool isHovered = false;
        private Coroutine shineCoroutine;

        public void Initialize(Button btn, RectTransform txtContainer, Image bg, Image bar, Outline ol, RectTransform shine, Color accent)
        {
            button = btn;
            textContainer = txtContainer;
            background = bg;
            accentBar = bar;
            outline = ol;
            shineSweep = shine;
            baseAccentColor = accent;
            hoverAccentColor = Color.Lerp(accent, Color.white, 0.4f);

            // Compute vibrant hover border glow matching the button's accent tone
            hoverOutlineColor = new Color(
                Mathf.Clamp01(accent.r * 1.35f),
                Mathf.Clamp01(accent.g * 1.35f),
                Mathf.Clamp01(accent.b * 1.35f),
                0.95f
            );

            if (background != null) baseBgColor = background.color;
            if (outline != null) baseOutlineColor = outline.effectColor;
            if (accentBar != null) baseAccentColor = accentBar.color;

            if (textContainer != null)
            {
                defaultLeftOffset = textContainer.offsetMin.x;
                currentLeftOffset = defaultLeftOffset;
                targetLeftOffset = defaultLeftOffset;
            }
        }

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            if (outline == null) outline = GetComponent<Outline>();

            if (textContainer != null)
            {
                defaultLeftOffset = textContainer.offsetMin.x;
                currentLeftOffset = defaultLeftOffset;
                targetLeftOffset = defaultLeftOffset;
            }
        }

        private void OnEnable()
        {
            ResetVisuals();
        }

        private void OnDisable()
        {
            ResetVisuals();
        }

        public void ResetVisuals()
        {
            isHovered = false;
            if (textContainer != null)
            {
                currentLeftOffset = defaultLeftOffset;
                targetLeftOffset = defaultLeftOffset;
                Vector2 min = textContainer.offsetMin;
                min.x = defaultLeftOffset;
                textContainer.offsetMin = min;
            }

            if (background != null) background.color = baseBgColor;
            if (outline != null)
            {
                outline.effectColor = baseOutlineColor;
                outline.effectDistance = new Vector2(1f, -1f);
            }
            if (accentBar != null) accentBar.color = baseAccentColor;
            if (shineSweep != null) shineSweep.gameObject.SetActive(false);
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (textContainer == null) return;

            // Smooth transition for 10px slide
            if (Mathf.Abs(currentLeftOffset - targetLeftOffset) > 0.05f)
            {
                currentLeftOffset = Mathf.Lerp(currentLeftOffset, targetLeftOffset, Time.unscaledDeltaTime * transitionSpeed);
                Vector2 min = textContainer.offsetMin;
                min.x = currentLeftOffset;
                textContainer.offsetMin = min;
            }

            // Smooth color transitions
            Color targetBg = isHovered ? hoverBgColor : baseBgColor;
            Color targetOl = isHovered ? hoverOutlineColor : baseOutlineColor;
            Color targetBar = isHovered ? hoverAccentColor : baseAccentColor;

            if (background != null && background.color != targetBg)
            {
                background.color = Color.Lerp(background.color, targetBg, Time.unscaledDeltaTime * transitionSpeed);
            }
            if (outline != null && outline.effectColor != targetOl)
            {
                outline.effectColor = Color.Lerp(outline.effectColor, targetOl, Time.unscaledDeltaTime * transitionSpeed);
            }
            if (accentBar != null && accentBar.color != targetBar)
            {
                accentBar.color = Color.Lerp(accentBar.color, targetBar, Time.unscaledDeltaTime * transitionSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            isHovered = true;
            targetLeftOffset = defaultLeftOffset + slideDistance;

            if (outline != null)
            {
                outline.effectDistance = new Vector2(2f, -2f);
            }

            // Subtle tactical audio tick
            AudioManager.Instance?.PlayButtonHover();

            // Trigger sweeping sheen shine animation across button face
            if (shineSweep != null)
            {
                if (shineCoroutine != null) StopCoroutine(shineCoroutine);
                shineCoroutine = StartCoroutine(AnimateShineSweep());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            targetLeftOffset = defaultLeftOffset;

            if (outline != null)
            {
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            transform.localScale = new Vector3(0.98f, 0.98f, 1f);
            AudioManager.Instance?.PlayButtonClick();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }

        private IEnumerator AnimateShineSweep()
        {
            if (shineSweep == null) yield break;

            shineSweep.gameObject.SetActive(true);
            float startX = -120f;
            float endX = 650f;
            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float x = Mathf.Lerp(startX, endX, t);
                shineSweep.anchoredPosition = new Vector2(x, 0f);
                yield return null;
            }

            shineSweep.gameObject.SetActive(false);
        }
    }
}

