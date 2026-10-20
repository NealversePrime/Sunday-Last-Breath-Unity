using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SundayLastBreath.Core;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    public class HobbyCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsPreviewText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image cardBorder;
        [SerializeField] private Image topAccentBar;
        [SerializeField] private Image statsBg;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image categoryBadgeBg;
        [SerializeField] private TextMeshProUGUI categoryBadgeText;

        private Action _onSelect;
        private bool _isLocked;
        private bool _isHovered;
        private Color _baseBorderColor;
        private Color _hoverBorderColor;
        private Color _baseBtnBgColor;
        private Color _hoverBtnBgColor;
        private Color _accentColor;
        private Image _btnBgImage;

        public void Setup(HobbyData data, Action onSelect, bool isEquippedPrimary = false)
        {
            _onSelect = onSelect;
            _isLocked = isEquippedPrimary;
            _isHovered = false;
            transform.localScale = Vector3.one;

            EnsureDynamicElements();

            if (titleText != null)
            {
                titleText.text = data.title;
                SundayTypography.ApplyFont(titleText, FontRole.Display);
            }
            if (descriptionText != null)
            {
                descriptionText.text = data.description;
                SundayTypography.ApplyFont(descriptionText, FontRole.Body);
            }
            if (statsPreviewText != null)
            {
                statsPreviewText.text = data.statsPreview;
                SundayTypography.ApplyFont(statsPreviewText, FontRole.Mono);
            }
            if (buttonText != null)
            {
                SundayTypography.ApplyFont(buttonText, FontRole.Display);
            }
            if (categoryBadgeText != null)
            {
                SundayTypography.ApplyFont(categoryBadgeText, FontRole.Mono);
            }

            if (selectButton != null)
            {
                _btnBgImage = selectButton.GetComponent<Image>();
            }

            string categoryLabel = GetCategoryInfo(data, out _accentColor);

            if (isEquippedPrimary)
            {
                Color equippedColor = new Color(0.18f, 0.80f, 0.44f, 1f); // Emerald #2ECC71
                _accentColor = equippedColor;
                _baseBorderColor = new Color(0.15f, 0.55f, 0.32f, 0.95f);
                _hoverBorderColor = equippedColor;
                _baseBtnBgColor = new Color(0.08f, 0.28f, 0.16f, 0.98f);
                _hoverBtnBgColor = _baseBtnBgColor;

                if (cardBorder != null) cardBorder.color = _baseBorderColor;
                if (topAccentBar != null) topAccentBar.color = equippedColor;
                if (titleText != null) titleText.color = equippedColor;
                if (statsPreviewText != null) statsPreviewText.color = equippedColor;
                if (statsBg != null) statsBg.color = new Color(0.05f, 0.18f, 0.10f, 0.75f);

                if (categoryBadgeBg != null) categoryBadgeBg.color = new Color(0.08f, 0.32f, 0.18f, 0.95f);
                if (categoryBadgeText != null)
                {
                    categoryBadgeText.text = "PRIMARY";
                    categoryBadgeText.color = equippedColor;
                }

                if (_btnBgImage != null) _btnBgImage.color = _baseBtnBgColor;
                if (buttonText != null)
                {
                    buttonText.text = "[ EQUIPPED: PRIMARY ]";
                    buttonText.color = equippedColor;
                }
                if (selectButton != null) selectButton.interactable = false;
            }
            else if (data.isWeakness)
            {
                _baseBorderColor = new Color(0.38f, 0.12f, 0.12f, 1f);
                _hoverBorderColor = new Color(0.95f, 0.25f, 0.25f, 1f);
                _baseBtnBgColor = new Color(0.16f, 0.05f, 0.05f, 0.98f);
                _hoverBtnBgColor = new Color(0.75f, 0.16f, 0.16f, 1f);

                if (cardBorder != null) cardBorder.color = _baseBorderColor;
                if (topAccentBar != null) topAccentBar.color = _accentColor;
                if (titleText != null) titleText.color = new Color(0.98f, 0.36f, 0.36f);
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.98f, 0.42f, 0.42f);
                if (statsBg != null) statsBg.color = new Color(0.24f, 0.05f, 0.05f, 0.72f);

                if (categoryBadgeBg != null) categoryBadgeBg.color = new Color(0.28f, 0.06f, 0.06f, 0.92f);
                if (categoryBadgeText != null)
                {
                    categoryBadgeText.text = categoryLabel;
                    categoryBadgeText.color = new Color(1f, 0.48f, 0.48f);
                }

                if (_btnBgImage != null) _btnBgImage.color = _baseBtnBgColor;
                if (buttonText != null)
                {
                    buttonText.text = "ACCEPT FLAW";
                    buttonText.color = new Color(0.95f, 0.82f, 0.82f);
                }
                if (selectButton != null) selectButton.interactable = true;
            }
            else
            {
                _baseBorderColor = new Color(0.20f, 0.22f, 0.26f, 1f);
                _hoverBorderColor = _accentColor;
                _baseBtnBgColor = new Color(0.08f, 0.10f, 0.14f, 0.98f);
                _hoverBtnBgColor = Color.Lerp(new Color(0.12f, 0.14f, 0.18f, 1f), _accentColor, 0.78f);

                if (cardBorder != null) cardBorder.color = _baseBorderColor;
                if (topAccentBar != null) topAccentBar.color = _accentColor;
                if (titleText != null) titleText.color = Color.white;
                if (statsPreviewText != null) statsPreviewText.color = new Color(0.42f, 0.78f, 1.0f);
                if (statsBg != null) statsBg.color = new Color(0.04f, 0.09f, 0.16f, 0.75f);

                if (categoryBadgeBg != null)
                {
                    categoryBadgeBg.color = new Color(_accentColor.r * 0.22f, _accentColor.g * 0.22f, _accentColor.b * 0.22f, 0.92f);
                }
                if (categoryBadgeText != null)
                {
                    categoryBadgeText.text = categoryLabel;
                    categoryBadgeText.color = Color.Lerp(_accentColor, Color.white, 0.25f);
                }

                if (_btnBgImage != null) _btnBgImage.color = _baseBtnBgColor;
                if (buttonText != null)
                {
                    buttonText.text = "SELECT TRAIT";
                    buttonText.color = new Color(0.88f, 0.92f, 0.96f);
                }
                if (selectButton != null) selectButton.interactable = true;
            }

            // Load Sprite from Assets/Sprites/Traits/trait_{id}.png
            string spritePath = $"Assets/Sprites/Traits/trait_{data.id}.png";
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (s != null && iconImage != null)
            {
                iconImage.sprite = s;
                iconImage.color = isEquippedPrimary
                    ? new Color(0.65f, 1f, 0.80f, 0.95f)
                    : (data.isWeakness ? new Color(1f, 0.86f, 0.86f, 0.94f) : new Color(0.94f, 0.94f, 0.94f, 0.95f));
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
#endif

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                if (!isEquippedPrimary)
                {
                    selectButton.onClick.AddListener(TriggerSelect);
                }
            }
        }

        private void Update()
        {
            Vector3 targetScale = (_isHovered && !_isLocked) ? new Vector3(1.028f, 1.028f, 1f) : Vector3.one;
            if ((transform.localScale - targetScale).sqrMagnitude > 0.00001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isLocked) return;
            _isHovered = true;

            if (cardBorder != null) cardBorder.color = _hoverBorderColor;
            if (_btnBgImage != null) _btnBgImage.color = _hoverBtnBgColor;
            if (buttonText != null) buttonText.color = Color.white;
            if (iconImage != null) iconImage.color = Color.white;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isLocked) return;
            _isHovered = false;

            if (cardBorder != null) cardBorder.color = _baseBorderColor;
            if (_btnBgImage != null) _btnBgImage.color = _baseBtnBgColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isLocked) return;
            // Avoid double firing if clicking directly on the child Button
            if (selectButton != null && eventData.pointerPress == selectButton.gameObject) return;
            TriggerSelect();
        }

        private void TriggerSelect()
        {
            if (_isLocked) return;
            _onSelect?.Invoke();
        }

        private void EnsureDynamicElements()
        {
            // Ensure larger icon size (112x112) inside banner for punchier visual presence
            if (iconImage != null)
            {
                RectTransform icRect = iconImage.GetComponent<RectTransform>();
                if (icRect != null)
                {
                    icRect.sizeDelta = new Vector2(112f, 112f);
                    icRect.anchoredPosition = new Vector2(0f, -2f);
                }
            }

            // Upgrade Title readability & positioning
            if (titleText != null)
            {
                titleText.fontSize = 19f;
                RectTransform tRect = titleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(-20f, 28f);
                    tRect.anchoredPosition = new Vector2(0f, -124f);
                }
            }

            // Upgrade Description readability & positioning (13px instead of 11.5px)
            if (descriptionText != null)
            {
                descriptionText.fontSize = 13f;
                descriptionText.lineSpacing = 2.5f;
                descriptionText.color = new Color(0.84f, 0.88f, 0.92f);
                RectTransform dRect = descriptionText.GetComponent<RectTransform>();
                if (dRect != null)
                {
                    dRect.sizeDelta = new Vector2(-24f, 44f);
                    dRect.anchoredPosition = new Vector2(0f, -154f);
                }
            }

            // Upgrade Stats Preview readability & positioning (12.5px instead of 11px)
            if (statsPreviewText != null)
            {
                statsPreviewText.fontSize = 12.5f;
            }
            if (statsBg != null)
            {
                RectTransform stRect = statsBg.GetComponent<RectTransform>();
                if (stRect != null)
                {
                    stRect.sizeDelta = new Vector2(-24f, 28f);
                    stRect.anchoredPosition = new Vector2(0f, -206f);
                }
            }

            // Upgrade Select Button to stretch width cleanly across both 4-column and 5-column cards
            if (selectButton != null)
            {
                RectTransform bBtnRect = selectButton.GetComponent<RectTransform>();
                if (bBtnRect != null)
                {
                    bBtnRect.anchorMin = new Vector2(0f, 0f);
                    bBtnRect.anchorMax = new Vector2(1f, 0f);
                    bBtnRect.pivot = new Vector2(0.5f, 0f);
                    bBtnRect.sizeDelta = new Vector2(-24f, 38f);
                    bBtnRect.anchoredPosition = new Vector2(0f, 12f);
                }
            }
            if (buttonText != null)
            {
                buttonText.fontSize = 13.5f;
                buttonText.characterSpacing = 2f;
            }

            // Ensure Category Badge exists in top-right of Banner with larger 11px text
            if (categoryBadgeBg == null && iconImage != null && iconImage.transform.parent != null)
            {
                Transform banner = iconImage.transform.parent;
                Transform existing = banner.Find("CategoryBadge");
                if (existing != null)
                {
                    categoryBadgeBg = existing.GetComponent<Image>();
                    categoryBadgeText = existing.GetComponentInChildren<TextMeshProUGUI>();
                }
                else
                {
                    GameObject badgeObj = new GameObject("CategoryBadge");
                    badgeObj.transform.SetParent(banner, false);
                    RectTransform bRect = badgeObj.AddComponent<RectTransform>();
                    bRect.anchorMin = new Vector2(1f, 1f);
                    bRect.anchorMax = new Vector2(1f, 1f);
                    bRect.pivot = new Vector2(1f, 1f);
                    bRect.sizeDelta = new Vector2(104f, 22f);
                    bRect.anchoredPosition = new Vector2(-6f, -6f);

                    categoryBadgeBg = badgeObj.AddComponent<Image>();
                    categoryBadgeBg.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);
                    categoryBadgeBg.raycastTarget = false;

                    Outline outline = badgeObj.AddComponent<Outline>();
                    outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
                    outline.effectDistance = new Vector2(1f, -1f);

                    GameObject txtObj = new GameObject("BadgeText");
                    txtObj.transform.SetParent(badgeObj.transform, false);
                    RectTransform tRect = txtObj.AddComponent<RectTransform>();
                    tRect.anchorMin = Vector2.zero;
                    tRect.anchorMax = Vector2.one;
                    tRect.offsetMin = new Vector2(4f, 0f);
                    tRect.offsetMax = new Vector2(-4f, 0f);

                    categoryBadgeText = txtObj.AddComponent<TextMeshProUGUI>();
                    categoryBadgeText.fontSize = 11f;
                    categoryBadgeText.fontStyle = FontStyles.Bold;
                    categoryBadgeText.alignment = TextAlignmentOptions.Center;
                    categoryBadgeText.raycastTarget = false;
                }
            }

            if (categoryBadgeBg != null)
            {
                RectTransform cbRect = categoryBadgeBg.GetComponent<RectTransform>();
                if (cbRect != null) cbRect.sizeDelta = new Vector2(104f, 22f);
            }
            if (categoryBadgeText != null)
            {
                categoryBadgeText.fontSize = 11f;
            }
        }

        /// <summary>
        /// Deterministic sort order so traits with the same Category Tag are always grouped side-by-side.
        /// Tab 0 (Action/Field): COMBAT (4) -> SURVIVAL (2) -> STEALTH (2)
        /// Tab 1 (Mind/Support): TECH & MIND (3) -> MEDICAL (2) -> SURVIVAL (1) -> DIPLOMACY (1)
        /// </summary>
        public static int GetCategorySortOrder(HobbyData data)
        {
            if (data == null) return 999;
            switch ((data.id ?? "").ToLowerInvariant())
            {
                // Tab 0: Row 1 = 4 COMBAT
                case "hunter": return 10;
                case "boxer": return 11;
                case "archer": return 12;
                case "baseball": return 13;
                // Tab 0: Row 2 = 2 SURVIVAL + 2 STEALTH
                case "runner": return 20;
                case "hiker": return 21;
                case "watchman": return 30;
                case "parkour": return 31;

                // Tab 1: Row 1 = 4 TECH & MIND
                case "mechanic": return 40;
                case "radio": return 41;
                case "gamer": return 42;
                case "bookworm": return 43;
                // Tab 1: Row 2 = MEDICAL + SURVIVAL + DIPLOMACY
                case "volunteer": return 50;
                case "gardener": return 51;
                case "chef": return 60;
                case "debater": return 70;

                default: return 100;
            }
        }

        public static string GetCategoryInfo(HobbyData data, out Color accent)
        {
            if (data.isWeakness)
            {
                switch (data.id.ToLowerInvariant())
                {
                    case "smoker":
                    case "alcoholic":
                    case "asthmatic":
                    case "blind":
                    case "badknee":
                    case "glutton":
                    case "fragile":
                    case "insomniac":
                    case "clumsy":
                    case "germophobe":
                        accent = new Color(0.90f, 0.22f, 0.21f, 1f); // Crimson
                        return "PHYSICAL";
                    default:
                        accent = new Color(0.85f, 0.18f, 0.38f, 1f); // Deep Rose/Crimson
                        return "PSYCHOLOGY";
                }
            }

            switch (data.id.ToLowerInvariant())
            {
                case "hunter":
                case "boxer":
                case "archer":
                case "baseball":
                    accent = new Color(0.90f, 0.49f, 0.13f, 1f); // Tactical Amber #E67E22
                    return "COMBAT";

                case "mechanic":
                case "radio":
                case "gamer":
                case "bookworm":
                    accent = new Color(0.16f, 0.71f, 0.96f, 1f); // Technical Cyan #29B6F6
                    return "TECH & MIND";

                case "runner":
                case "hiker":
                case "chef":
                    accent = new Color(0.18f, 0.80f, 0.44f, 1f); // Survival Emerald #2ECC71
                    return "SURVIVAL";

                case "gardener":
                case "volunteer":
                    accent = new Color(0.15f, 0.65f, 0.60f, 1f); // Medical Teal #26A69A
                    return "MEDICAL";

                case "watchman":
                case "parkour":
                    accent = new Color(0.67f, 0.28f, 0.74f, 1f); // Stealth Violet #AB47BC
                    return "STEALTH";

                case "debater":
                    accent = new Color(1.00f, 0.76f, 0.03f, 1f); // Diplomacy Gold #FFC107
                    return "DIPLOMACY";

                default:
                    accent = new Color(0.83f, 0.33f, 0f, 1f);
                    return "TRAIT";
            }
        }
    }
}
