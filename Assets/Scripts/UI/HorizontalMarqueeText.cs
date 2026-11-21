using TMPro;
using UnityEngine;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Gently pans overflowing single-line TextMeshProUGUI text left and right inside a masked parent viewport,
    /// pausing at both ends so truncated labels can be read without altering layout bounds.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HorizontalMarqueeText : MonoBehaviour
    {
        private enum ScrollPhase
        {
            StartPause,
            ScrollLeft,
            EndPause,
            ScrollRight
        }

        [SerializeField] private float scrollSpeed = 42f;
        [SerializeField] private float returnSpeed = 58f;
        [SerializeField] private float startPauseSeconds = 1.4f;
        [SerializeField] private float endPauseSeconds = 1.4f;
        [SerializeField] private float endPadding = 16f;

        private TextMeshProUGUI _tmp;
        private RectTransform _rt;
        private ScrollPhase _phase = ScrollPhase.StartPause;
        private float _timer;
        private float _offset;
        private float _overflowDistance;
        private string _lastText;
        private float _lastViewportWidth = -1f;
        private bool _needsMeasure = true;

        private void Awake()
        {
            CacheComponents();
            ResetScroll();
        }

        private void OnEnable()
        {
            CacheComponents();
            ResetScroll();
        }

        private void CacheComponents()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            if (_rt == null) _rt = GetComponent<RectTransform>();
            if (_tmp != null)
            {
                _tmp.textWrappingMode = TextWrappingModes.NoWrap;
                _tmp.overflowMode = TextOverflowModes.Overflow;
            }
        }

        /// <summary>
        /// Restarts the ping-pong scroll cycle from the left edge and queues a width re-measure.
        /// </summary>
        public void ResetScroll()
        {
            _phase = ScrollPhase.StartPause;
            _timer = 0f;
            _offset = 0f;
            _needsMeasure = true;
            ApplyOffset();
        }

        private void Update()
        {
            if (_tmp == null || _rt == null) return;

            RectTransform parentRt = transform.parent as RectTransform;
            float viewportWidth = parentRt != null ? parentRt.rect.width : _rt.rect.width;
            if (viewportWidth <= 1f) return;

            string currentText = _tmp.text ?? string.Empty;
            if (_needsMeasure || !string.Equals(currentText, _lastText, System.StringComparison.Ordinal) || !Mathf.Approximately(viewportWidth, _lastViewportWidth))
            {
                MeasureOverflow(currentText, viewportWidth);
            }

            if (_overflowDistance <= 0.5f)
            {
                if (_offset > 0f)
                {
                    _offset = 0f;
                    ApplyOffset();
                }
                return;
            }

            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            AdvanceMarquee(dt);
            ApplyOffset();
        }

        private void MeasureOverflow(string currentText, float viewportWidth)
        {
            _lastText = currentText;
            _lastViewportWidth = viewportWidth;
            _needsMeasure = false;

            _tmp.textWrappingMode = TextWrappingModes.NoWrap;
            _tmp.overflowMode = TextOverflowModes.Overflow;
            _tmp.ForceMeshUpdate();

            float preferred = _tmp.preferredWidth;
            _overflowDistance = preferred > viewportWidth + 1f
                ? Mathf.Max(0f, preferred - viewportWidth + endPadding)
                : 0f;

            _phase = ScrollPhase.StartPause;
            _timer = 0f;
            _offset = 0f;
            ApplyOffset();
        }

        private void AdvanceMarquee(float dt)
        {
            switch (_phase)
            {
                case ScrollPhase.StartPause:
                    _timer += dt;
                    if (_timer >= startPauseSeconds)
                    {
                        _timer = 0f;
                        _phase = ScrollPhase.ScrollLeft;
                    }
                    break;

                case ScrollPhase.ScrollLeft:
                    _offset += scrollSpeed * dt;
                    if (_offset >= _overflowDistance)
                    {
                        _offset = _overflowDistance;
                        _timer = 0f;
                        _phase = ScrollPhase.EndPause;
                    }
                    break;

                case ScrollPhase.EndPause:
                    _timer += dt;
                    if (_timer >= endPauseSeconds)
                    {
                        _timer = 0f;
                        _phase = ScrollPhase.ScrollRight;
                    }
                    break;

                case ScrollPhase.ScrollRight:
                    _offset -= returnSpeed * dt;
                    if (_offset <= 0f)
                    {
                        _offset = 0f;
                        _timer = 0f;
                        _phase = ScrollPhase.StartPause;
                    }
                    break;
            }
        }

        private void ApplyOffset()
        {
            if (_rt == null) return;
            _rt.anchoredPosition = new Vector2(-_offset, _rt.anchoredPosition.y);
        }
    }
}
