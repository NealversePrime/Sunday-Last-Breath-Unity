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
            if (_borderImg != null)
                _borderImg.color = hovered ? _accentColor : _baseBorderColor;
            if (_btnBgImg != null)
                _btnBgImg.color = hovered
                    ? Color.Lerp(new Color(0.14f, 0.16f, 0.20f, 1f), _accentColor, 0.75f)
                    : new Color(0.09f, 0.11f, 0.14f, 0.98f);
        }
    }
}
