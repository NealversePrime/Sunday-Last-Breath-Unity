using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SundayLastBreath.UI
{
    public enum FontRole
    {
        Display,    // Oswald-Bold / Style D: ChakraPetch-Bold
        Mono,       // JetBrainsMono / Style D: IBMPlexMono-Regular
        Narrative,  // Merriweather / Style D: Spectral-Regular
        Body,       // Inter / Style D: IBMPlexMono-Regular
        VcrOsd,     // VCR_OSD_MONO_1.001: 90s CRT/VHS On-Screen Display (Location, Time, Day)
        Typewriter  // SpecialElite-Regular: Pinned Physical Paper Note
    }

    /// <summary>
    /// Centralized 4-Font Typography System for Sunday: Last Breath (Paket 1: Gritty Literary Survival).
    /// Dynamically loads TTF fonts from Assets/Resources/Fonts and constructs fallback-safe TMP_FontAssets.
    /// </summary>
    public static class SundayTypography
    {
        private const string NarrativeScalePrefKey = "SLB_NarrativeFontScale";
        private static readonly float[] NarrativeScaleSteps = { 0.90f, 1.00f, 1.15f, 1.30f };
        private static float _narrativeFontScale = -1f;

        public static event System.Action OnNarrativeScaleChanged;

        public static float NarrativeFontScale
        {
            get
            {
                if (_narrativeFontScale < 0f)
                {
                    float saved = PlayerPrefs.GetFloat(NarrativeScalePrefKey, 1.0f);
                    _narrativeFontScale = SnapToNearestScale(saved);
                }
                return _narrativeFontScale;
            }
            set
            {
                float snapped = SnapToNearestScale(value);
                if (!Mathf.Approximately(_narrativeFontScale, snapped))
                {
                    _narrativeFontScale = snapped;
                    PlayerPrefs.SetFloat(NarrativeScalePrefKey, _narrativeFontScale);
                    PlayerPrefs.Save();
                    OnNarrativeScaleChanged?.Invoke();
                }
            }
        }

        public static int NarrativeFontScalePercent => Mathf.RoundToInt(NarrativeFontScale * 100f);

        private static int FindNearestScaleIndex(float scale)
        {
            float clamped = Mathf.Clamp(scale, NarrativeScaleSteps[0], NarrativeScaleSteps[NarrativeScaleSteps.Length - 1]);
            int bestIdx = 1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < NarrativeScaleSteps.Length; i++)
            {
                float d = Mathf.Abs(clamped - NarrativeScaleSteps[i]);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        private static float SnapToNearestScale(float scale)
        {
            return NarrativeScaleSteps[FindNearestScaleIndex(scale)];
        }

        public static void StepNarrativeFontScale(int delta)
        {
            int curIdx = FindNearestScaleIndex(NarrativeFontScale);
            int nextIdx = Mathf.Clamp(curIdx + delta, 0, NarrativeScaleSteps.Length - 1);
            NarrativeFontScale = NarrativeScaleSteps[nextIdx];
        }

        public static void CycleNarrativeFontScale()
        {
            int curIdx = FindNearestScaleIndex(NarrativeFontScale);
            int nextIdx = (curIdx + 1) % NarrativeScaleSteps.Length;
            NarrativeFontScale = NarrativeScaleSteps[nextIdx];
        }

        public static bool StyleDThemeEnabled = false;

        private static TMP_FontAsset _displayFont;
        private static TMP_FontAsset _monoFont;
        private static TMP_FontAsset _narrativeFont;
        private static TMP_FontAsset _bodyFont;
        private static TMP_FontAsset _vcrOsdFont;
        private static TMP_FontAsset _styleDDisplayFont;
        private static TMP_FontAsset _styleDMonoFont;
        private static TMP_FontAsset _styleDNarrativeFont;
        private static TMP_FontAsset _typewriterFont;

        public static TMP_FontAsset DisplayFont => StyleDThemeEnabled
            ? GetOrCreateFont(ref _styleDDisplayFont, "Fonts/ChakraPetch-Bold", includeMonoFallback: true)
            : GetOrCreateFont(ref _displayFont, "Fonts/Oswald-Bold", includeMonoFallback: true);

        public static TMP_FontAsset MonoFont => StyleDThemeEnabled
            ? GetOrCreateFont(ref _styleDMonoFont, "Fonts/IBMPlexMono-Regular", includeMonoFallback: false)
            : GetOrCreateFont(ref _monoFont, "Fonts/JetBrainsMono-Regular", includeMonoFallback: false);

        public static TMP_FontAsset NarrativeFont => StyleDThemeEnabled
            ? GetOrCreateFont(ref _styleDNarrativeFont, "Fonts/Spectral-Regular", includeMonoFallback: true)
            : GetOrCreateFont(ref _narrativeFont, "Fonts/Merriweather-Regular", includeMonoFallback: true);

        public static TMP_FontAsset BodyFont => StyleDThemeEnabled
            ? MonoFont
            : GetOrCreateFont(ref _bodyFont, "Fonts/Inter-Regular", includeMonoFallback: true);

        public static TMP_FontAsset VcrOsdFont
        {
            get
            {
                bool wasNull = _vcrOsdFont == null || _vcrOsdFont.material == null;
                TMP_FontAsset font = GetOrCreateFont(ref _vcrOsdFont, "Fonts/VCR_OSD_MONO_1.001", includeMonoFallback: true);
                if (font != null && font.material != null && (wasNull || !font.material.IsKeywordEnabled("UNDERLAY_ON")))
                {
                    Material mat = font.material;
                    mat.EnableKeyword("UNDERLAY_ON");
                    if (mat.HasProperty("_UnderlayColor")) mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.78f));
                    if (mat.HasProperty("_UnderlayOffsetX")) mat.SetFloat("_UnderlayOffsetX", 0.55f);
                    if (mat.HasProperty("_UnderlayOffsetY")) mat.SetFloat("_UnderlayOffsetY", -0.55f);
                    if (mat.HasProperty("_UnderlayDilate")) mat.SetFloat("_UnderlayDilate", 0.25f);
                    if (mat.HasProperty("_UnderlaySoftness")) mat.SetFloat("_UnderlaySoftness", 0.45f);
                    if (mat.HasProperty("_OutlineSoftness")) mat.SetFloat("_OutlineSoftness", 0.14f);
                }
                return font;
            }
        }
        public static TMP_FontAsset TypewriterFont => GetOrCreateFont(ref _typewriterFont, "Fonts/SpecialElite-Regular", includeMonoFallback: true);

        private static TMP_FontAsset GetOrCreateFont(ref TMP_FontAsset cached, string resourcePath, bool includeMonoFallback)
        {
            if (cached != null && cached.material != null)
            {
                return cached;
            }

            Font sourceFont = Resources.Load<Font>(resourcePath);
#if UNITY_EDITOR
            if (sourceFont == null)
            {
                sourceFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>($"Assets/Resources/{resourcePath}.ttf");
            }
#endif
            if (sourceFont == null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            cached = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (cached == null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            cached.name = sourceFont.name + " (Dynamic SDF)";

            if (cached.fallbackFontAssetTable == null)
            {
                cached.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }

            // Ensure geometric symbols (■, □, ●, ○, ✓, ↺, ↻, arrows) never show missing glyph boxes
            if (includeMonoFallback)
            {
                TMP_FontAsset mono = MonoFont;
                if (mono != null && mono != cached && !cached.fallbackFontAssetTable.Contains(mono))
                {
                    cached.fallbackFontAssetTable.Add(mono);
                }
            }

            TMP_FontAsset defaultTmp = TMP_Settings.defaultFontAsset;
            if (defaultTmp != null && defaultTmp != cached && !cached.fallbackFontAssetTable.Contains(defaultTmp))
            {
                cached.fallbackFontAssetTable.Add(defaultTmp);
            }

            return cached;
        }

        public static TMP_FontAsset GetFont(FontRole role)
        {
            switch (role)
            {
                case FontRole.Display: return DisplayFont;
                case FontRole.Mono: return MonoFont;
                case FontRole.Narrative: return NarrativeFont;
                case FontRole.VcrOsd: return VcrOsdFont;
                case FontRole.Typewriter: return TypewriterFont;
                case FontRole.Body:
                default: return BodyFont;
            }
        }

        public static string SanitizeText(string t)
        {
            if (string.IsNullOrEmpty(t)) return t;
            return t.Replace("\u2611", "[v]")
                    .Replace("\U0001F4AC", "RE:")
                    .Replace("\u21BA", "RT:")
                    .Replace("\u26A0", "[!]")
                    .Replace("■", "//")
                    .Replace("□", "[ ]")
                    .Replace("▲", "[UP]")
                    .Replace("▼", "[DN]")
                    .Replace("►", ">")
                    .Replace("✓", "[OK]")
                    .Replace("●", "*")
                    .Replace("○", "-");
        }

        public static void ApplyFont(TextMeshProUGUI tmp, FontRole role)
        {
            if (tmp == null) return;

            if (!string.IsNullOrEmpty(tmp.text))
            {
                tmp.text = SanitizeText(tmp.text);
            }

            TMP_FontAsset fontAsset = GetFont(role);
            if (fontAsset != null)
            {
                tmp.font = fontAsset;
            }
        }

        /// <summary>
        /// Automatically applies the 3-Font + UI Hierarchy across all TextMeshProUGUI components under a hierarchy root
        /// based on semantic naming, text styling, and UI context.
        /// </summary>
        public static void ApplyHierarchy(Transform root)
        {
            if (root == null) return;

            TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTexts)
            {
                if (tmp == null) continue;
                FontRole role = InferRole(tmp);
                ApplyFont(tmp, role);
            }
        }

        private static FontRole InferRole(TextMeshProUGUI tmp)
        {
            string objName = tmp.gameObject.name.ToLowerInvariant();
            string parentName = tmp.transform.parent != null ? tmp.transform.parent.name.ToLowerInvariant() : "";
            string sample = tmp.text ?? "";

            // 1. Explicit Monospace / Tactical Terminal elements
            if (objName.Contains("mono") || objName.Contains("stat") || objName.Contains("code") ||
                objName.Contains("badge") || objName.Contains("mrz") || objName.Contains("barcode") ||
                objName.Contains("version") || objName.Contains("pip") || objName.Contains("value") ||
                objName.Contains("vital") || objName.Contains("sublabel") || objName.Contains("doc") ||
                parentName.Contains("stat") || parentName.Contains("badge") || parentName.Contains("category") ||
                sample.Contains("<mspace") || sample.Contains("<pos=") || sample.Contains("■") ||
                sample.Contains("DOC-") || sample.Contains("IDUSA<<"))
            {
                return FontRole.Mono;
            }

            // 2. Narrative / Literary Quotes & Story Prose
            if (objName.Contains("quote") || objName.Contains("subtitle") || objName.Contains("reflection") ||
                objName.Contains("lore") || objName.Contains("narrative") ||
                (tmp.fontStyle & FontStyles.Italic) != 0 ||
                (sample.StartsWith("\"") && sample.EndsWith("\"")))
            {
                return FontRole.Narrative;
            }

            // 3. Display / Headers / Titles / Primary Buttons
            if (objName.Contains("title") || objName.Contains("header") || objName.Contains("banner") ||
                objName.Contains("stamp") || objName.Contains("name") ||
                parentName.Contains("header") || parentName.Contains("btn_") || parentName.Contains("selectbtn") ||
                parentName.Contains("confirmbtn") ||
                tmp.fontSize >= 19f || tmp.characterSpacing >= 4f)
            {
                return FontRole.Display;
            }

            // 4. Clean Modern UI Body (Descriptions, Input Fields, Notices)
            return FontRole.Body;
        }
    }
}

