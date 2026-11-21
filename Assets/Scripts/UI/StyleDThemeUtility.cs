using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Shared Style D 90s tactical-hardware visual helpers and palette constants for modals
    /// (Load Game, Settings, Expanded Map Viewer, and System Menu), gated on Style D activation.
    /// </summary>
    public static class StyleDThemeUtility
    {
        public static readonly Color ColBackdrop = new Color(0.01f, 0.012f, 0.016f, 0.88f);
        public static readonly Color ColBone = new Color(0.89f, 0.87f, 0.82f, 1f);
        public static readonly Color ColMuted = new Color(0.60f, 0.58f, 0.54f, 1f);
        public static readonly Color ColAmber = new Color(0.88f, 0.64f, 0.22f, 1f);
        public static readonly Color ColRust = new Color(0.76f, 0.24f, 0.18f, 1f);
        public static readonly Color ColSage = new Color(0.32f, 0.64f, 0.44f, 1f);
        public static readonly Color ColMetalBtn = new Color(0.24f, 0.26f, 0.29f, 1f);
        public static readonly Color ColMetalHover = new Color(0.34f, 0.36f, 0.40f, 1f);
        public static readonly Color ColMetalDisabled = new Color(0.14f, 0.15f, 0.17f, 0.72f);
        public static readonly Color ColHairline = new Color(0.24f, 0.25f, 0.28f, 0.55f);

        private static Sprite _panelChassis;
        private static Sprite _crtBezel;
        private static Sprite _wellRecessed;
        private static Sprite _buttonMetal;
        private static Sprite _statCard;
        private static Sprite _ccConsoleChassis;
        private static Sprite _ccIdCrtHousing;
        private static Sprite _ccBiometricGridWell;
        private static Sprite _ccMrzPaperStrip;
        private static Sprite _ccModuleTray;
        private static Sprite _ccChipButton;
        private static Sprite _ccInputTerminal;
        private static Sprite _ccDossierCard;
        private static Texture2D _crtScanlineTex;

        public static bool IsStyleDActive
        {
            get
            {
                if (SundayTypography.StyleDThemeEnabled) return true;

                string sceneName = SceneManager.GetActiveScene().name;
                bool active = !string.IsNullOrEmpty(sceneName)
                    && sceneName.IndexOf("StyleD", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!active)
                {
                    var cockpit = UnityEngine.Object.FindAnyObjectByType<HybridCockpitController>(FindObjectsInactive.Include);
                    active = cockpit != null && cockpit.IsStyleDHardwareTheme;
                }

                if (active)
                {
                    SundayTypography.StyleDThemeEnabled = true;
                }

                return active;
            }
        }

        public static Sprite PanelChassis =>
            HybridCockpitController.SharedStyleDPanelChassis
            ?? LoadSprite(ref _panelChassis, "Assets/Sprites/UI/HardwareTheme/ui_panel_chassis_9slice.png", "UI/HardwareTheme/ui_panel_chassis_9slice");

        public static Sprite WellRecessed =>
            HybridCockpitController.SharedStyleDWellRecessed
            ?? LoadSprite(ref _wellRecessed, "Assets/Sprites/UI/HardwareTheme/ui_well_recessed_9slice.png", "UI/HardwareTheme/ui_well_recessed_9slice");

        public static Sprite ButtonMetal =>
            HybridCockpitController.SharedStyleDButtonMetal
            ?? LoadSprite(ref _buttonMetal, "Assets/Sprites/UI/HardwareTheme/ui_button_metal_9slice.png", "UI/HardwareTheme/ui_button_metal_9slice");

        public static Sprite StatCard =>
            HybridCockpitController.SharedStyleDStatCard
            ?? LoadSprite(ref _statCard, "Assets/Sprites/UI/HardwareTheme/ui_stat_card_9slice.png", "UI/HardwareTheme/ui_stat_card_9slice");

        public static Sprite CrtBezel =>
            HybridCockpitController.SharedStyleDCrtBezel
            ?? LoadSprite(ref _crtBezel, "Assets/Sprites/UI/HardwareTheme/ui_crt_bezel_frame.png", "UI/HardwareTheme/ui_crt_bezel_frame");

        // Dedicated Style D Character Creation sprites (Assets/Sprites/UI/CharacterCreationTheme/)
        public static Sprite CcConsoleChassis =>
            LoadSprite(ref _ccConsoleChassis, "Assets/Sprites/UI/CharacterCreationTheme/cc_console_chassis_9slice.png", "UI/CharacterCreationTheme/cc_console_chassis_9slice", new Vector4(64f, 64f, 64f, 64f));

        public static Sprite CcIdCrtHousing =>
            LoadSprite(ref _ccIdCrtHousing, "Assets/Sprites/UI/CharacterCreationTheme/cc_id_crt_housing_9slice.png", "UI/CharacterCreationTheme/cc_id_crt_housing_9slice", new Vector4(44f, 44f, 44f, 44f));

        public static Sprite CcBiometricGridWell =>
            LoadSprite(ref _ccBiometricGridWell, "Assets/Sprites/UI/CharacterCreationTheme/cc_biometric_grid_well.png", "UI/CharacterCreationTheme/cc_biometric_grid_well", new Vector4(24f, 24f, 24f, 24f));

        public static Sprite CcMrzPaperStrip =>
            LoadSprite(ref _ccMrzPaperStrip, "Assets/Sprites/UI/CharacterCreationTheme/cc_mrz_paper_strip_9slice.png", "UI/CharacterCreationTheme/cc_mrz_paper_strip_9slice", new Vector4(24f, 20f, 24f, 20f));

        public static Sprite CcModuleTray =>
            LoadSprite(ref _ccModuleTray, "Assets/Sprites/UI/CharacterCreationTheme/cc_module_tray_9slice.png", "UI/CharacterCreationTheme/cc_module_tray_9slice", new Vector4(32f, 32f, 32f, 42f));

        public static Sprite CcChipButton =>
            LoadSprite(ref _ccChipButton, "Assets/Sprites/UI/CharacterCreationTheme/cc_chip_button_9slice.png", "UI/CharacterCreationTheme/cc_chip_button_9slice", new Vector4(22f, 20f, 22f, 20f));

        public static Sprite CcInputTerminal =>
            LoadSprite(ref _ccInputTerminal, "Assets/Sprites/UI/CharacterCreationTheme/cc_input_terminal_9slice.png", "UI/CharacterCreationTheme/cc_input_terminal_9slice", new Vector4(20f, 18f, 20f, 18f));

        public static Sprite CcDossierCard =>
            LoadSprite(ref _ccDossierCard, "Assets/Sprites/UI/CharacterCreationTheme/cc_dossier_card_9slice.png", "UI/CharacterCreationTheme/cc_dossier_card_9slice", new Vector4(32f, 32f, 32f, 44f));

        public static Texture2D GetCrtScanlineTexture()
        {
            Texture2D shared = HybridCockpitController.GetSharedStyleDCrtScanlineTexture();
            if (shared != null) return shared;
            if (_crtScanlineTex != null) return _crtScanlineTex;

            var tex = new Texture2D(1, 4, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            tex.SetPixel(0, 1, new Color(0f, 0f, 0f, 0f));
            tex.SetPixel(0, 2, new Color(0f, 0f, 0f, 0.42f));
            tex.SetPixel(0, 3, new Color(0f, 0f, 0f, 0.16f));
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            _crtScanlineTex = tex;
            return _crtScanlineTex;
        }

        public static void AttachCrtOsdChromatic(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            var fx = tmp.GetComponent<CrtOsdChromaticEffect>() ?? tmp.gameObject.AddComponent<CrtOsdChromaticEffect>();
            fx.SyncNow();
        }

        private static Sprite LoadSprite(ref Sprite cache, string assetPath, string resourcePath, Vector4? fallbackBorder = null)
        {
            if (cache != null) return cache;
#if UNITY_EDITOR
            cache = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (cache != null) return cache;
#endif
            cache = Resources.Load<Sprite>(resourcePath);
            if (cache != null) return cache;

            if (System.IO.File.Exists(assetPath))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(assetPath);
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        tex.hideFlags = HideFlags.HideAndDontSave;
                        Vector4 border = fallbackBorder ?? Vector4.zero;
                        cache = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, border);
                        cache.hideFlags = HideFlags.HideAndDontSave;
                        return cache;
                    }
                }
                catch { }
            }
            return cache;
        }

        public static void ApplySliced(Image img, Sprite sp, float ppuMult, Color tint, bool disableOutline = true)
        {
            if (img == null) return;
            if (sp != null)
            {
                img.sprite = sp;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = ppuMult;
            }
            img.color = tint;
            if (disableOutline)
            {
                Outline ol = img.GetComponent<Outline>();
                if (ol != null) ol.enabled = false;
            }
        }

        public static void ApplyMetalButton(Button btn, Color normalTint, Color hoverTint, float ppuMult = 2.8f, Color? textCol = null, bool disableOutline = true)
        {
            ApplyButtonSpriteImpl(btn, ButtonMetal, normalTint, hoverTint, ppuMult, textCol, disableOutline);
        }

        public static void ApplyChipButton(Button btn, Color normalTint, Color hoverTint, float ppuMult = 1.6f, Color? textCol = null, bool disableOutline = true)
        {
            ApplyButtonSpriteImpl(btn, CcChipButton, normalTint, hoverTint, ppuMult, textCol, disableOutline);
        }

        private static void ApplyButtonSpriteImpl(Button btn, Sprite sp, Color normalTint, Color hoverTint, float ppuMult, Color? textCol, bool disableOutline)
        {
            if (btn == null) return;
            Image bg = btn.targetGraphic as Image ?? btn.GetComponent<Image>();
            if (bg != null)
            {
                if (sp != null)
                {
                    ApplySliced(bg, sp, ppuMult, Color.white, disableOutline);
                    if (bg.canvasRenderer != null) bg.canvasRenderer.SetColor(normalTint);
                }
                else
                {
                    bg.color = normalTint;
                    if (disableOutline)
                    {
                        Outline ol = bg.GetComponent<Outline>();
                        if (ol != null) ol.enabled = false;
                    }
                }
            }

            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.normalColor = normalTint;
            cb.highlightedColor = hoverTint;
            cb.pressedColor = new Color(normalTint.r * 0.72f, normalTint.g * 0.72f, normalTint.b * 0.72f, normalTint.a);
            cb.selectedColor = normalTint;
            cb.disabledColor = ColMetalDisabled;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;

            if (textCol.HasValue)
            {
                TextMeshProUGUI lbl = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (lbl != null) lbl.color = textCol.Value;
            }
        }

        public static Color WithRgbScale(Color c, float scale, float alpha = 1f)
        {
            return new Color(Mathf.Clamp01(c.r * scale), Mathf.Clamp01(c.g * scale), Mathf.Clamp01(c.b * scale), alpha);
        }

        public static Image AddLeftAccentBar(Transform parent, Color col, float width = 4f)
        {
            if (parent == null) return null;
            Transform existing = parent.Find("StyleD_AccentBar");
            GameObject barObj = existing != null
                ? existing.gameObject
                : new GameObject("StyleD_AccentBar", typeof(RectTransform), typeof(Image));
            if (existing == null) barObj.transform.SetParent(parent, false);

            RectTransform rt = barObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(0f, 2f);
            rt.offsetMax = new Vector2(width, -2f);

            Image img = barObj.GetComponent<Image>();
            img.color = col;
            img.raycastTarget = false;
            return img;
        }

        public static void AddHeaderDivider(Transform header)
        {
            if (header == null || header.Find("StyleD_HeaderDivider") != null) return;
            GameObject line = new GameObject("StyleD_HeaderDivider", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(header, false);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;

            Image img = line.GetComponent<Image>();
            img.color = ColHairline;
            img.raycastTarget = false;
        }
    }
}
