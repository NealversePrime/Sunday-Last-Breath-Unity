using TMPro;
using UnityEngine;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Adds a lightweight horizontal RGB chromatic aberration fringe to CRT/VHS on-screen display
    /// TextMeshProUGUI elements so OSD text blends physically into low-res scanline monitors.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CrtOsdChromaticEffect : MonoBehaviour
    {
        private static readonly Color CyanFringeColor = new Color(0.15f, 0.88f, 1.0f, 0.24f);
        private static readonly Color MagentaFringeColor = new Color(1.0f, 0.22f, 0.35f, 0.22f);
        private const float HorizontalOffsetPx = 1.6f;

        private TextMeshProUGUI _source;
        private TextMeshProUGUI _cyanGhost;
        private TextMeshProUGUI _magentaGhost;

        private string _lastText;
        private float _lastFontSize = -1f;
        private TMP_FontAsset _lastFont;
        private FontStyles _lastFontStyle;

        private void OnEnable()
        {
            SyncNow();
        }

        private void LateUpdate()
        {
            if (_source == null) _source = GetComponent<TextMeshProUGUI>();
            if (_source == null) return;

            if (!string.Equals(_source.text, _lastText, System.StringComparison.Ordinal) ||
                !Mathf.Approximately(_source.fontSize, _lastFontSize) ||
                _source.font != _lastFont ||
                _source.fontStyle != _lastFontStyle)
            {
                SyncNow();
            }
        }

        public void SyncNow()
        {
            if (_source == null) _source = GetComponent<TextMeshProUGUI>();
            if (_source == null) return;

            _cyanGhost = EnsureGhost("_CrtCyanFringe", _cyanGhost, -HorizontalOffsetPx, CyanFringeColor);
            _magentaGhost = EnsureGhost("_CrtMagentaFringe", _magentaGhost, HorizontalOffsetPx, MagentaFringeColor);

            CopyGhostProperties(_cyanGhost, CyanFringeColor);
            CopyGhostProperties(_magentaGhost, MagentaFringeColor);

            _lastText = _source.text;
            _lastFontSize = _source.fontSize;
            _lastFont = _source.font;
            _lastFontStyle = _source.fontStyle;
        }

        private TextMeshProUGUI EnsureGhost(string childName, TextMeshProUGUI cached, float offsetX, Color fringeColor)
        {
            if (cached == null)
            {
                Transform existing = transform.Find(childName);
                GameObject go = existing != null
                    ? existing.gameObject
                    : new GameObject(childName, typeof(RectTransform), typeof(TextMeshProUGUI));
                if (existing == null) go.transform.SetParent(transform, false);
                cached = go.GetComponent<TextMeshProUGUI>();
            }

            RectTransform rt = cached.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(offsetX, 0f);
            rt.offsetMax = new Vector2(offsetX, 0f);
            rt.localScale = Vector3.one;

            cached.raycastTarget = false;
            cached.color = fringeColor;
            return cached;
        }

        private void CopyGhostProperties(TextMeshProUGUI ghost, Color fringeColor)
        {
            if (ghost == null || _source == null) return;

            if (_source.font != null && ghost.font != _source.font)
            {
                ghost.font = _source.font;
            }
            ghost.fontSize = _source.fontSize;
            ghost.enableAutoSizing = _source.enableAutoSizing;
            ghost.fontSizeMin = _source.fontSizeMin;
            ghost.fontSizeMax = _source.fontSizeMax;
            ghost.fontStyle = _source.fontStyle;
            ghost.characterSpacing = _source.characterSpacing;
            ghost.alignment = _source.alignment;
            ghost.textWrappingMode = _source.textWrappingMode;
            ghost.overflowMode = _source.overflowMode;
            ghost.margin = _source.margin;
            ghost.color = fringeColor;
            ghost.raycastTarget = false;
            ghost.text = _source.text;
        }
    }
}
