using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class HorizontalScrollWheelHelper : MonoBehaviour, IScrollHandler
    {
        private ScrollRect _scrollRect;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_scrollRect == null || !_scrollRect.horizontal || _scrollRect.vertical) return;

            // Scroll horizontally with vertical mouse wheel
            float delta = eventData.scrollDelta.y;
            if (Mathf.Abs(delta) > 0.01f)
            {
                float sensitivity = _scrollRect.scrollSensitivity > 0 ? _scrollRect.scrollSensitivity : 20f;
                float step = (delta * sensitivity) / 1000f;
                _scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition - step);
            }
        }
    }
}

