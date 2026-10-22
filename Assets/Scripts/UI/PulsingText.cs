using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Smoothly pulses TextMeshPro alpha to recreate retro prompt blinking.
    /// Matches the CSS keyframes pulse animation from Twine.
    /// </summary>
    public class PulsingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float minAlpha = 0.25f;
        [SerializeField] private float maxAlpha = 1.0f;

        private void Awake()
        {
            if (targetText == null)
            {
                targetText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Update()
        {
            if (targetText == null) return;

            float sin = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, sin);

            Color color = targetText.color;
            color.a = alpha;
            targetText.color = color;
        }
    }

    /// <summary>
    /// Smoothly pulses a UI badge's scale and brightness to draw the player's attention.
    /// Used for alert indicators like new floor loot or urgent notifications.
    /// </summary>
    public class PulsingBadge : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private Image targetImage;
        [SerializeField] private float pulseSpeed = 3.2f;
        [SerializeField] private float minScale = 0.98f;
        [SerializeField] private float maxScale = 1.15f;

        private Vector3 baseScale;

        private void Awake()
        {
            if (targetRect == null) targetRect = GetComponent<RectTransform>();
            if (targetImage == null) targetImage = GetComponent<Image>();
            baseScale = targetRect != null ? targetRect.localScale : Vector3.one;
        }

        private void OnEnable()
        {
            if (targetRect != null) targetRect.localScale = baseScale;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(minScale, maxScale, t);

            if (targetRect != null)
            {
                targetRect.localScale = baseScale * scale;
            }

            if (targetImage != null)
            {
                Color c = targetImage.color;
                c.a = Mathf.Lerp(0.85f, 1.0f, t);
                targetImage.color = c;
            }
        }
    }
}
