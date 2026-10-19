using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SundayLastBreath.UI
{
    public enum FontRole
    {
        Display,    // Oswald-Bold: Headers, Titles, Alert Banners, Primary Buttons
        Mono,       // JetBrainsMono: Tactical Stats, Dossier Codes, Badges, Terminal Readouts
        Narrative,  // Merriweather: Atmospheric Quotes, Story Prose, Literary Monologues
        Body        // Inter: Clean UI Descriptions, Input Fields, General Labels
    }

    /// <summary>
    /// Centralized 4-Font Typography System for Sunday: Last Breath (Paket 1: Gritty Literary Survival).
    /// Dynamically loads TTF fonts from Assets/Resources/Fonts and constructs fallback-safe TMP_FontAssets.
    /// </summary>
    public static class SundayTypography
    {
        private static TMP_FontAsset _displayFont;
        private static TMP_FontAsset _monoFont;
        private static TMP_FontAsset _narrativeFont;
        private static TMP_FontAsset _bodyFont;

        public static TMP_FontAsset DisplayFont => GetOrCreateFont(ref _displayFont, "Fonts/Oswald-Bold", includeMonoFallback: true);
        public static TMP_FontAsset MonoFont => GetOrCreateFont(ref _monoFont, "Fonts/JetBrainsMono-Regular", includeMonoFallback: false);
        public static TMP_FontAsset NarrativeFont => GetOrCreateFont(ref _narrativeFont, "Fonts/Merriweather-Regular", includeMonoFallback: true);
        public static TMP_FontAsset BodyFont => GetOrCreateFont(ref _bodyFont, "Fonts/Inter-Regular", includeMonoFallback: true);

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
                case FontRole.Body:
                default: return BodyFont;
            }
        }

        public static void ApplyFont(TextMeshProUGUI tmp, FontRole role)
        {
            if (tmp == null) return;
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

