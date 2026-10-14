using TMPro;
using UnityEngine;

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
}

