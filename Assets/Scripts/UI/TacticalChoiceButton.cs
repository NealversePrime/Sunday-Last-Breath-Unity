using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Implements Fallout / Baldur's Gate 3 style tactical CRPG choice row behavior:
    /// - High-density 30-34px row height
    /// - Glowing hover chevron ">>" with smooth rightward slide
    /// - Diegetic bracketed hotkey "[ 1 ]", "[ 2 ]"
    /// - Translucent phosphor amber/cyan hover wash & 2px left neon accent bar
    /// - Tactile scale dip on click
    /// - Audio triggers for hover and click
    /// - Programmatic execution via keyboard hotkeys (1-9)
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
        [SerializeField] private TextMeshProUGUI hoverChevronTmp;
        [SerializeField] private TextMeshProUGUI hotkeyTmp;
        [SerializeField] private TextMeshProUGUI mainLabelTmp;
        [SerializeField] private TextMeshProUGUI subLabelTmp;

        [Header("Colors & Styling")]
        [SerializeField] private Color baseBgColor = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        [SerializeField] private Color hoverBgColor = new Color(0.95f, 0.65f, 0.15f, 0.14f);
        [SerializeField] private Color baseOutlineColor = new Color(0.18f, 0.22f, 0.28f, 0.40f);
        [SerializeField] private Color hoverOutlineColor = new Color(0.95f, 0.70f, 0.25f, 0.85f);
        [SerializeField] private Color baseAccentColor = new Color(0.95f, 0.75f, 0.35f);
        [SerializeField] private Color hoverAccentColor = new Color(1f, 0.85f, 0.40f);
        [SerializeField] private Color normalHotkeyColor = new Color(0.85f, 0.65f, 0.30f, 0.95f);
        [SerializeField] private Color hoverHotkeyColor = Color.white;

        [Header("Animation Settings")]
        [SerializeField] private float slideDistance = 6f;
        [SerializeField] private float transitionSpeed = 16f;

        private float defaultChevronOffset = 2f;
        private float targetChevronOffset = 2f;
        private float currentChevronOffset = 2f;
        private bool isHovered = false;
        private Coroutine shineCoroutine;

        public Button ButtonComponent => button;

        public void Initialize(Button btn, RectTransform txtContainer, Image bg, Image bar, Outline ol, RectTransform shine, Color accent,
            TextMeshProUGUI chevron = null, TextMeshProUGUI hotkey = null, TextMeshProUGUI mainLbl = null, TextMeshProUGUI subLbl = null)
        {
            button = btn;
            textContainer = txtContainer;
            background = bg;
            accentBar = bar;
            outline = ol;
            shineSweep = shine;
            hoverChevronTmp = chevron;
            hotkeyTmp = hotkey;
            mainLabelTmp = mainLbl;
            subLabelTmp = subLbl;
            baseAccentColor = accent;
            hoverAccentColor = Color.Lerp(accent, Color.white, 0.45f);

            hoverOutlineColor = new Color(
                Mathf.Clamp01(accent.r * 1.35f),
                Mathf.Clamp01(accent.g * 1.35f),
                Mathf.Clamp01(accent.b * 1.35f),
                0.95f
            );

            if (background != null) baseBgColor = background.color;
            if (outline != null) baseOutlineColor = outline.effectColor;
            if (accentBar != null) baseAccentColor = accentBar.color;
        }

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (background == null) background = GetComponent<Image>();
            if (outline == null) outline = GetComponent<Outline>();

            // Auto-locate components if not linked
            if (hoverChevronTmp == null)
            {
                Transform ch = transform.Find("HoverChevron") ?? transform.Find("Gutter/HoverChevron");
                if (ch != null) hoverChevronTmp = ch.GetComponent<TextMeshProUGUI>();
            }
            if (hotkeyTmp == null)
            {
                Transform hk = transform.Find("HotkeyLabel") ?? transform.Find("Gutter/HotkeyLabel");
                if (hk != null) hotkeyTmp = hk.GetComponent<TextMeshProUGUI>();
            }
            if (mainLabelTmp == null)
            {
                Transform ml = transform.Find("MainLabel") ?? transform.Find("TextContainer/MainLabel") ?? transform.Find("LabelContainer/MainLabel");
                if (ml != null) mainLabelTmp = ml.GetComponent<TextMeshProUGUI>();
            }
            if (subLabelTmp == null)
            {
                Transform sl = transform.Find("SubLabel") ?? transform.Find("TextContainer/SubLabel") ?? transform.Find("LabelContainer/SubLabel");
                if (sl != null) subLabelTmp = sl.GetComponent<TextMeshProUGUI>();
            }
            if (accentBar == null)
            {
                Transform ab = transform.Find("AccentBar");
                if (ab != null) accentBar = ab.GetComponent<Image>();
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

        public void SetChoiceNumber(int number)
        {
            if (hotkeyTmp != null)
            {
                hotkeyTmp.text = $"[ {number} ]";
                SundayTypography.ApplyFont(hotkeyTmp, FontRole.Mono);
            }
        }

        public void TriggerClick()
        {
            if (button == null || !button.interactable || !gameObject.activeInHierarchy) return;
            AudioManager.Instance?.PlayButtonClick();
            button.onClick.Invoke();
        }

        public void ResetVisuals()
        {
            isHovered = false;
            currentChevronOffset = defaultChevronOffset;
            targetChevronOffset = defaultChevronOffset;

            if (hoverChevronTmp != null)
            {
                hoverChevronTmp.gameObject.SetActive(false);
                RectTransform rt = hoverChevronTmp.rectTransform;
                rt.anchoredPosition = new Vector2(defaultChevronOffset, rt.anchoredPosition.y);
            }

            if (hotkeyTmp != null)
            {
                hotkeyTmp.color = normalHotkeyColor;
            }

            if (background != null) background.color = baseBgColor;
            if (outline != null)
            {
                outline.effectColor = baseOutlineColor;
                outline.effectDistance = new Vector2(1f, -1f);
            }
            if (accentBar != null)
            {
                accentBar.color = baseAccentColor;
                accentBar.gameObject.SetActive(false);
            }
            if (shineSweep != null) shineSweep.gameObject.SetActive(false);
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            // Smooth Chevron Slide
            if (hoverChevronTmp != null && hoverChevronTmp.gameObject.activeSelf)
            {
                if (Mathf.Abs(currentChevronOffset - targetChevronOffset) > 0.05f)
                {
                    currentChevronOffset = Mathf.Lerp(currentChevronOffset, targetChevronOffset, Time.unscaledDeltaTime * transitionSpeed);
                    RectTransform rt = hoverChevronTmp.rectTransform;
                    rt.anchoredPosition = new Vector2(currentChevronOffset, rt.anchoredPosition.y);
                }
            }

            // Smooth background & accent color transitions
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
            targetChevronOffset = defaultChevronOffset + slideDistance;

            if (hoverChevronTmp != null)
            {
                hoverChevronTmp.gameObject.SetActive(true);
            }
            if (hotkeyTmp != null)
            {
                hotkeyTmp.color = hoverHotkeyColor;
            }
            if (accentBar != null)
            {
                accentBar.gameObject.SetActive(true);
            }
            if (outline != null)
            {
                outline.effectDistance = new Vector2(2f, -1f);
            }

            // High-frequency CRT phosphor tick SFX
            AudioManager.Instance?.PlayButtonHover();

            // Trigger sweeping sheen reflection
            if (shineSweep != null)
            {
                if (shineCoroutine != null) StopCoroutine(shineCoroutine);
                shineCoroutine = StartCoroutine(AnimateShineSweep());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            targetChevronOffset = defaultChevronOffset;

            if (hoverChevronTmp != null)
            {
                hoverChevronTmp.gameObject.SetActive(false);
            }
            if (hotkeyTmp != null)
            {
                hotkeyTmp.color = normalHotkeyColor;
            }
            if (accentBar != null)
            {
                accentBar.gameObject.SetActive(false);
            }
            if (outline != null)
            {
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            transform.localScale = new Vector3(0.99f, 0.99f, 1f);
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
            float duration = 0.30f;
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

