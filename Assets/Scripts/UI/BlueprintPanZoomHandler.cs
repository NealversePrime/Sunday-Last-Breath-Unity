using UnityEngine;
using UnityEngine.EventSystems;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Captures scroll wheel zoom and mouse drag pan events over the tactical blueprint viewport.
    /// Operates via standard UGUI EventSystem for rock-solid reliability across all resolutions,
    /// Game View zoom scales (e.g. 0.83x), DPIs, and display modes.
    /// </summary>
    public class BlueprintPanZoomHandler : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TacticalMapViewController controller;

        public void Initialize(TacticalMapViewController mapController)
        {
            controller = mapController;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (controller == null)
            {
                controller = GetComponentInParent<TacticalMapViewController>();
            }
            if (controller == null) return;

            if (eventData.scrollDelta.y > 0.01f)
            {
                controller.ZoomIn();
            }
            else if (eventData.scrollDelta.y < -0.01f)
            {
                controller.ZoomOut();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controller == null)
            {
                controller = GetComponentInParent<TacticalMapViewController>();
            }
            if (controller == null) return;

            controller.OnDragPan(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
