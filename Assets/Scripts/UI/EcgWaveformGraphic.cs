using UnityEngine;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Procedural animated ECG bio-monitor graphic drawn directly in UI mesh geometry (zero texture allocations).
    /// Reacts to player HP% by shifting sweep frequency and phosphor trace hue.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class EcgWaveformGraphic : MaskableGraphic
    {
        private const int SegmentCount = 72;
        private static readonly Color ColGrid = new Color(0.067f, 0.165f, 0.114f, 0.42f);
        private static readonly Color ColEmerald = new Color(0.227f, 0.839f, 0.475f, 1f); // #3AD679
        private static readonly Color ColAmber = new Color(0.878f, 0.612f, 0.169f, 1f);   // #E09C2B
        private static readonly Color ColCrimson = new Color(0.878f, 0.259f, 0.259f, 1f); // #E04242

        private float _sweepPhase = 0.72f;
        private float _beatsPerSecond = 1.05f;
        private Color _traceColor = ColEmerald;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        public void SetHealthNormalized(float hp01)
        {
            hp01 = Mathf.Clamp01(hp01);
            if (hp01 >= 0.60f)
            {
                _traceColor = ColEmerald;
                _beatsPerSecond = 1.05f;
            }
            else if (hp01 >= 0.30f)
            {
                _traceColor = ColAmber;
                _beatsPerSecond = 1.45f;
            }
            else
            {
                _traceColor = ColCrimson;
                _beatsPerSecond = 1.90f;
            }
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            _sweepPhase += Time.unscaledDeltaTime * (_beatsPerSecond * 0.55f);
            if (_sweepPhase >= 1f) _sweepPhase -= 1f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            if (r.width <= 4f || r.height <= 4f) return;

            DrawOscilloscopeGrid(vh, r);
            DrawWaveformTrace(vh, r);
        }

        private static void DrawOscilloscopeGrid(VertexHelper vh, Rect r)
        {
            const int cols = 6;
            const int rows = 3;
            const float thickness = 1f;

            for (int c = 1; c < cols; c++)
            {
                float x = r.xMin + r.width * (c / (float)cols);
                AddLineSegment(vh, new Vector2(x, r.yMin), new Vector2(x, r.yMax), thickness, ColGrid);
            }

            for (int row = 1; row < rows; row++)
            {
                float y = r.yMin + r.height * (row / (float)rows);
                AddLineSegment(vh, new Vector2(r.xMin, y), new Vector2(r.xMax, y), thickness, ColGrid);
            }
        }

        private void DrawWaveformTrace(VertexHelper vh, Rect r)
        {
            float baseY = r.yMin + r.height * 0.44f;
            float amp = r.height * 0.42f;
            float headU = Mathf.Repeat(_sweepPhase, 1f);
            float dx = r.width / SegmentCount;
            Vector2 prev = SamplePoint(0f, r, baseY, amp);

            for (int i = 1; i <= SegmentCount; i++)
            {
                float u = i / (float)SegmentCount;
                Vector2 curr = SamplePoint(u, r, baseY, amp);

                // Sweeping phosphor decay behind scanhead
                float distBehind = headU - u;
                if (distBehind < 0f) distBehind += 1f;
                float fade = distBehind < 0.04f ? 1f : Mathf.Clamp01(1f - distBehind * 1.15f);
                fade = Mathf.Lerp(0.16f, 1f, fade * fade);

                // Small dark blanking gap right ahead of scanhead
                if (distBehind > 0.94f) fade = 0.08f;

                Color segCol = _traceColor;
                segCol.a *= fade;
                AddLineSegment(vh, prev, curr, 1.75f, segCol);

                // Subtle phosphor bloom halo near scanhead
                if (fade > 0.55f)
                {
                    Color halo = _traceColor;
                    halo.a *= (fade - 0.50f) * 0.35f;
                    AddLineSegment(vh, prev, curr, 4.2f, halo);
                }

                prev = curr;
            }

            // Leading bright phosphor dot at scanhead
            Vector2 headPt = SamplePoint(headU, r, baseY, amp);
            headPt.x = Mathf.Clamp(headPt.x, r.xMin + 2f, r.xMax - 2f);
            Color glowCol = _traceColor;
            glowCol.a *= 0.40f;
            AddQuadDot(vh, headPt, 4.2f, glowCol);
            AddQuadDot(vh, headPt, 2.1f, Color.Lerp(_traceColor, Color.white, 0.72f));
        }

        private static Vector2 SamplePoint(float u, Rect r, float baseY, float amp)
        {
            float x = r.xMin + u * r.width;
            // Two heartbeats visible across horizontal oscilloscope width
            float beatPhase = Mathf.Repeat(u * 2.0f + 0.12f, 1f);
            float yOffset = EvaluateEcgPulse(beatPhase) * amp;
            return new Vector2(x, Mathf.Clamp(baseY + yOffset, r.yMin + 2f, r.yMax - 2f));
        }

        private static float EvaluateEcgPulse(float t)
        {
            // Classic P-Q-R-S-T clinical waveform normalized in [-0.45 .. +1.0]
            if (t >= 0.14f && t < 0.24f)
            {
                float p = (t - 0.14f) / 0.10f;
                return Mathf.Sin(p * Mathf.PI) * 0.16f; // P wave
            }
            if (t >= 0.29f && t < 0.33f)
            {
                float p = (t - 0.29f) / 0.04f;
                return -Mathf.Sin(p * Mathf.PI) * 0.18f; // Q dip
            }
            if (t >= 0.33f && t < 0.39f)
            {
                float p = (t - 0.33f) / 0.06f;
                return Mathf.Sin(p * Mathf.PI) * 1.0f; // R spike
            }
            if (t >= 0.39f && t < 0.44f)
            {
                float p = (t - 0.39f) / 0.05f;
                return -Mathf.Sin(p * Mathf.PI) * 0.42f; // S trough
            }
            if (t >= 0.54f && t < 0.70f)
            {
                float p = (t - 0.54f) / 0.16f;
                return Mathf.Sin(p * Mathf.PI) * 0.26f; // T wave
            }
            return 0f;
        }

        private static void AddLineSegment(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color col)
        {
            Vector2 dir = b - a;
            float len = dir.magnitude;
            if (len < 0.001f) return;

            Vector2 perp = new Vector2(-dir.y, dir.x) * ((thickness * 0.5f) / len);
            int idx = vh.currentVertCount;
            Color32 c32 = col;

            vh.AddVert(a - perp, c32, Vector2.zero);
            vh.AddVert(a + perp, c32, Vector2.zero);
            vh.AddVert(b + perp, c32, Vector2.zero);
            vh.AddVert(b - perp, c32, Vector2.zero);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }

        private static void AddQuadDot(VertexHelper vh, Vector2 center, float halfSize, Color col)
        {
            int idx = vh.currentVertCount;
            Color32 c32 = col;

            vh.AddVert(center + new Vector2(-halfSize, -halfSize), c32, Vector2.zero);
            vh.AddVert(center + new Vector2(-halfSize, halfSize), c32, Vector2.zero);
            vh.AddVert(center + new Vector2(halfSize, halfSize), c32, Vector2.zero);
            vh.AddVert(center + new Vector2(halfSize, -halfSize), c32, Vector2.zero);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
    }
}
