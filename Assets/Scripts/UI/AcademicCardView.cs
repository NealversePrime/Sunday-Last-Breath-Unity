using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SundayLastBreath.Core;

namespace SundayLastBreath.UI
{
    public class AcademicCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string skillId;

        private bool _isHovered;
        private Image _borderImg;
        private Image _topBarImg;
        private Image _btnBgImg;
        private Color _accentColor = new Color(0.90f, 0.49f, 0.13f, 1f);
        private Color _baseBorderColor = new Color(0.18f, 0.22f, 0.28f, 1f);

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            _isHovered = false;
            transform.localScale = Vector3.one;
            CacheReferences();
            ApplyVisuals(false);
        }

        private void CacheReferences()
        {
            if (_borderImg == null) _borderImg = GetComponent<Image>();
            Transform inner = transform.Find("InnerCard");
            if (inner != null)
            {
                if (_topBarImg == null)
                {
                    Transform tb = inner.Find("TopBorder");
                    if (tb != null) _topBarImg = tb.GetComponent<Image>();
                }
                if (_topBarImg != null) _accentColor = _topBarImg.color;

                if (_btnBgImg == null)
                {
                    Transform btn = inner.Find("SelectBtn");
                    if (btn != null) _btnBgImg = btn.GetComponent<Image>();
                }

                if (StyleDThemeUtility.IsStyleDActive)
                {
                    if (inner is RectTransform inRt)
                    {
                        inRt.offsetMin = new Vector2(12f, 16f);
                        inRt.offsetMax = new Vector2(-12f, -44f);
                    }
                    Image inImg = inner.GetComponent<Image>();
                    if (inImg != null) inImg.color = Color.clear;
                    if (_topBarImg != null) _topBarImg.gameObject.SetActive(false);

                    Transform bannerTr = inner.Find("Banner");
                    if (bannerTr is RectTransform bRt)
                    {
                        bRt.anchoredPosition = new Vector2(0f, 0f);
                        bRt.sizeDelta = new Vector2(0f, 220f);
                    }

                    Transform titleTr = inner.Find("Title");
                    if (titleTr is RectTransform tRt)
                    {
                        tRt.anchoredPosition = new Vector2(0f, -232f);
                        tRt.sizeDelta = new Vector2(-16f, 30f);
                    }

                    Transform statBadge = inner.Find("StatBadge");
                    if (statBadge != null)
                    {
                        if (statBadge is RectTransform sbRt)
                        {
                            sbRt.anchoredPosition = new Vector2(0f, -270f);
                            sbRt.sizeDelta = new Vector2(-32f, 26f);
                        }
                        StyleDThemeUtility.ApplySliced(statBadge.GetComponent<Image>(), StyleDThemeUtility.CcInputTerminal, 1.65f, Color.white, disableOutline: true);
                    }

                    Transform descTr = inner.Find("Description");
                    if (descTr is RectTransform dRt)
                    {
                        dRt.anchoredPosition = new Vector2(0f, -304f);
                        dRt.sizeDelta = new Vector2(-20f, 75f);
                    }

                    Transform btnTr = inner.Find("SelectBtn");
                    if (btnTr != null && btnTr.GetComponent<Button>() is Button b)
                    {
                        if (btnTr is RectTransform btnRt)
                        {
                            btnRt.anchoredPosition = new Vector2(0f, 14f);
                            btnRt.sizeDelta = new Vector2(240f, 44f);
                        }
                        StyleDThemeUtility.ApplyChipButton(b, new Color(0.24f, 0.27f, 0.31f, 1f), Color.Lerp(new Color(0.28f, 0.25f, 0.22f, 1f), _accentColor, 0.72f), 1.55f, StyleDThemeUtility.ColBone);
                    }
                }
            }
        }

        private void Update()
        {
            Vector3 targetScale = _isHovered ? new Vector3(1.025f, 1.025f, 1f) : Vector3.one;
            if ((transform.localScale - targetScale).sqrMagnitude > 0.00001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            ApplyVisuals(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            ApplyVisuals(false);
        }

        private void ApplyVisuals(bool hovered)
        {
            if (StyleDThemeUtility.IsStyleDActive)
            {
                if (_borderImg != null)
                    StyleDThemeUtility.ApplySliced(_borderImg, StyleDThemeUtility.CcDossierCard, 1.4f, hovered ? Color.white : new Color(0.86f, 0.86f, 0.88f, 1f), disableOutline: true);

                if (_btnBgImg != null && _btnBgImg.canvasRenderer != null)
                {
                    Color btnTint = hovered
                        ? Color.Lerp(new Color(0.28f, 0.25f, 0.22f, 1f), _accentColor, 0.72f)
                        : new Color(0.24f, 0.27f, 0.31f, 1f);
                    _btnBgImg.canvasRenderer.SetColor(btnTint);
                }
                return;
            }

            if (_borderImg != null)
                _borderImg.color = hovered ? _accentColor : _baseBorderColor;
            if (_btnBgImg != null)
                _btnBgImg.color = hovered
                    ? Color.Lerp(new Color(0.14f, 0.16f, 0.20f, 1f), _accentColor, 0.75f)
                    : new Color(0.09f, 0.11f, 0.14f, 0.98f);
        }
    }
}
