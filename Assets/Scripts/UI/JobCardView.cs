using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SundayLastBreath.Core;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Interactive tactical dossier card for Pre-Outbreak Vocations (Jobs).
    /// Provides full-card hover scaling, border illumination, banner brightening,
    /// role badge, core attribute strip, and click handling.
    /// </summary>
    public class JobCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public string jobId;

        private Action _onSelect;
        private bool _isHovered;
        private Color _accentColor = new Color(0.83f, 0.33f, 0f, 1f);
        private Color _baseBorderColor = new Color(0.18f, 0.24f, 0.32f, 1f);
        private Color _hoverBorderColor = Color.white;
        private Color _baseInnerColor = new Color(0.035f, 0.045f, 0.068f, 0.98f);
        private Color _hoverInnerColor = new Color(0.055f, 0.075f, 0.110f, 0.99f);
        private Color _baseBtnColor = new Color(0.08f, 0.11f, 0.16f, 0.98f);
        private Color _hoverBtnColor = new Color(0.75f, 0.34f, 0.06f, 1f);

        private Image _cardBorder;
        private Image _innerBg;
        private Image _topBar;
        private Image _jobImage;
        private Image _btnBg;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _btnText;
        private Outline _cardOutline;

        public void Setup(JobData job, Action onSelect)
        {
            if (job == null) return;
            jobId = job.id;
            _onSelect = onSelect;
            _isHovered = false;
            transform.localScale = Vector3.one;

            // Give Student a warm tactical silver-cyan instead of stark white so hover/badges look cohesive
            _accentColor = job.id.Equals("student", StringComparison.OrdinalIgnoreCase)
                ? new Color(0.82f, 0.88f, 0.96f, 1f)
                : job.accentColor;

            // Boost dark crimson on Construction Worker for readability on dark slate backgrounds
            if (job.id.Equals("worker", StringComparison.OrdinalIgnoreCase))
            {
                _accentColor = new Color(0.92f, 0.32f, 0.26f, 1f);
            }

            _baseBorderColor = Color.Lerp(new Color(0.16f, 0.22f, 0.30f, 1f), _accentColor, 0.28f);
            _hoverBorderColor = Color.Lerp(_accentColor, Color.white, 0.20f);
            _baseBtnColor = new Color(0.08f, 0.11f, 0.16f, 0.98f);
            _hoverBtnColor = Color.Lerp(new Color(0.12f, 0.16f, 0.22f, 1f), _accentColor, 0.72f);

            BindAndUpgradeVisuals(job);
            ApplyHoverVisuals(false);
        }

        private void BindAndUpgradeVisuals(JobData job)
        {
            _cardBorder = GetComponent<Image>();
            if (_cardBorder != null)
            {
                _cardBorder.raycastTarget = true;
            }

            _cardOutline = GetComponent<Outline>();
            if (_cardOutline == null)
            {
                _cardOutline = gameObject.AddComponent<Outline>();
                _cardOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            Transform inner = transform.Find("InnerCard");
            if (inner == null) return;

            _innerBg = inner.GetComponent<Image>();
            if (_innerBg != null)
            {
                _innerBg.color = _baseInnerColor;
            }

            Transform topBarTr = inner.Find("TopBorder");
            if (topBarTr != null)
            {
                _topBar = topBarTr.GetComponent<Image>();
                if (_topBar != null) _topBar.color = _accentColor;
                RectTransform tbRect = topBarTr.GetComponent<RectTransform>();
                if (tbRect != null) tbRect.sizeDelta = new Vector2(0f, 4f);
            }

            // Banner & Role Archetype Badge
            Transform banner = inner.Find("Banner");
            if (banner != null)
            {
                RectTransform bRect = banner.GetComponent<RectTransform>();
                if (bRect != null)
                {
                    bRect.sizeDelta = new Vector2(0f, 162f);
                    bRect.anchoredPosition = new Vector2(0f, -4f);
                }

                Transform jobImgTr = banner.Find("JobImage");
                if (jobImgTr != null)
                {
                    _jobImage = jobImgTr.GetComponent<Image>();
                }

                EnsureRoleBadge(banner, job);
            }

            // Title + Inline Stat Summary
            Transform titleTr = inner.Find("Title");
            if (titleTr != null)
            {
                RectTransform tRect = titleTr.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(-36f, 30f);
                    tRect.anchoredPosition = new Vector2(0f, -172f);
                }

                _titleText = titleTr.GetComponent<TextMeshProUGUI>();
                if (_titleText != null)
                {
                    string displayTitle = job.id.Equals("worker", StringComparison.OrdinalIgnoreCase)
                        ? "CONSTRUCTION"
                        : job.title;
                    _titleText.text = displayTitle;
                    _titleText.fontSize = 22f;
                    _titleText.color = _accentColor;
                    SundayTypography.ApplyFont(_titleText, FontRole.Display);
                }
            }

            // Ensure Core Attribute Pill on the right of the Title row (using 3-letter S.P.A.I.C. tags!)
            EnsureAttributeStrip(inner, job);

            // Description & Quote (Bumped to 13px for crisp readability)
            Transform descTr = inner.Find("Description");
            if (descTr != null)
            {
                RectTransform dRect = descTr.GetComponent<RectTransform>();
                if (dRect != null)
                {
                    dRect.sizeDelta = new Vector2(-36f, 56f);
                    dRect.anchoredPosition = new Vector2(0f, -206f);
                }

                TextMeshProUGUI dTmp = descTr.GetComponent<TextMeshProUGUI>();
                if (dTmp != null)
                {
                    string cleanQuote = (job.quote ?? "").Trim('\"');
                    dTmp.text = $"<color=#DCE4EC>{job.description}</color>\n<color=#90A4AE><i>\"{cleanQuote}\"</i></color>";
                    dTmp.fontSize = 13f;
                    dTmp.lineSpacing = 3f;
                    dTmp.overflowMode = TextOverflowModes.Truncate;
                    SundayTypography.ApplyFont(dTmp, FontRole.Body);
                }
            }

            // Stats / Perks Box (Upgraded federal slate styling + 12.5px readable Mono font)
            Transform statsTr = inner.Find("StatsBox");
            if (statsTr != null)
            {
                RectTransform stRect = statsTr.GetComponent<RectTransform>();
                if (stRect != null)
                {
                    stRect.sizeDelta = new Vector2(-36f, 62f);
                    stRect.anchoredPosition = new Vector2(0f, -268f);
                }

                Image stBg = statsTr.GetComponent<Image>();
                if (stBg != null)
                {
                    stBg.color = new Color(0.05f, 0.08f, 0.13f, 0.92f);
                }

                Outline stOl = statsTr.GetComponent<Outline>();
                if (stOl == null) stOl = statsTr.gameObject.AddComponent<Outline>();
                stOl.effectColor = new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.25f);
                stOl.effectDistance = new Vector2(1f, -1f);

                Transform stripeTr = statsTr.Find("LeftStripe");
                if (stripeTr != null)
                {
                    Image sImg = stripeTr.GetComponent<Image>();
                    if (sImg != null) sImg.color = _accentColor;
                    RectTransform sRect = stripeTr.GetComponent<RectTransform>();
                    if (sRect != null) sRect.sizeDelta = new Vector2(4f, 0f);
                }

                Transform sTextTr = statsTr.Find("Text");
                if (sTextTr != null)
                {
                    TextMeshProUGUI stTmp = sTextTr.GetComponent<TextMeshProUGUI>();
                    if (stTmp != null)
                    {
                        stTmp.fontSize = 12.5f;
                        stTmp.lineSpacing = 4f;
                        stTmp.color = new Color(0.85f, 0.90f, 0.95f);
                        stTmp.text = FormatPerksAndLoadout(job);
                        SundayTypography.ApplyFont(stTmp, FontRole.Mono);
                    }
                }
            }

            // Select Button
            Transform btnTr = inner.Find("SelectBtn");
            if (btnTr != null)
            {
                RectTransform btnRect = btnTr.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.sizeDelta = new Vector2(-36f, 44f);
                    btnRect.anchoredPosition = new Vector2(0f, 14f);
                }

                _btnBg = btnTr.GetComponent<Image>();
                Outline btnOl = btnTr.GetComponent<Outline>();
                if (btnOl == null) btnOl = btnTr.gameObject.AddComponent<Outline>();
                btnOl.effectColor = new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.45f);
                btnOl.effectDistance = new Vector2(1f, -1f);

                Button bBtn = btnTr.GetComponent<Button>();
                if (bBtn != null)
                {
                    bBtn.onClick.RemoveAllListeners();
                    bBtn.onClick.AddListener(TriggerSelect);
                }

                _btnText = btnTr.GetComponentInChildren<TextMeshProUGUI>();
                if (_btnText != null)
                {
                    _btnText.fontSize = 14.5f;
                    _btnText.characterSpacing = 2.5f;
                    SundayTypography.ApplyFont(_btnText, FontRole.Display);
                }
            }
        }

        private void EnsureRoleBadge(Transform banner, JobData job)
        {
            Transform badgeTr = banner.Find("RoleBadge");
            GameObject badgeObj;
            if (badgeTr == null)
            {
                badgeObj = new GameObject("RoleBadge");
                badgeObj.transform.SetParent(banner, false);
                RectTransform bRect = badgeObj.AddComponent<RectTransform>();
                bRect.anchorMin = new Vector2(1f, 1f);
                bRect.anchorMax = new Vector2(1f, 1f);
                bRect.pivot = new Vector2(1f, 1f);
                bRect.sizeDelta = new Vector2(148f, 24f);
                bRect.anchoredPosition = new Vector2(-8f, -8f);

                Image bg = badgeObj.AddComponent<Image>();
                bg.raycastTarget = false;

                Outline ol = badgeObj.AddComponent<Outline>();
                ol.effectDistance = new Vector2(1f, -1f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(badgeObj.transform, false);
                RectTransform tRect = txtObj.AddComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.offsetMin = new Vector2(6f, 0f);
                tRect.offsetMax = new Vector2(-6f, 0f);

                TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 11f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
            else
            {
                badgeObj = badgeTr.gameObject;
            }

            Image badgeBg = badgeObj.GetComponent<Image>();
            if (badgeBg != null)
            {
                badgeBg.color = new Color(0.04f, 0.06f, 0.10f, 0.92f);
            }
            Outline badgeOl = badgeObj.GetComponent<Outline>();
            if (badgeOl != null)
            {
                badgeOl.effectColor = new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.70f);
            }

            TextMeshProUGUI badgeTmp = badgeObj.GetComponentInChildren<TextMeshProUGUI>();
            if (badgeTmp != null)
            {
                badgeTmp.text = GetRoleCategoryTag(job.id);
                badgeTmp.color = Color.Lerp(_accentColor, Color.white, 0.25f);
                SundayTypography.ApplyFont(badgeTmp, FontRole.Mono);
            }
        }

        private void EnsureAttributeStrip(Transform inner, JobData job)
        {
            Transform stripTr = inner.Find("AttrStrip");
            GameObject stripObj;
            if (stripTr == null)
            {
                stripObj = new GameObject("AttrStrip");
                stripObj.transform.SetParent(inner, false);
                RectTransform sRect = stripObj.AddComponent<RectTransform>();
                sRect.anchorMin = new Vector2(1f, 1f);
                sRect.anchorMax = new Vector2(1f, 1f);
                sRect.pivot = new Vector2(1f, 1f);
                sRect.sizeDelta = new Vector2(246f, 24f);
                sRect.anchoredPosition = new Vector2(-18f, -175f);

                Image bg = stripObj.AddComponent<Image>();
                bg.color = new Color(0.05f, 0.08f, 0.13f, 0.90f);
                bg.raycastTarget = false;

                Outline ol = stripObj.AddComponent<Outline>();
                ol.effectColor = new Color(0.20f, 0.28f, 0.40f, 0.6f);
                ol.effectDistance = new Vector2(1f, -1f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(stripObj.transform, false);
                RectTransform tRect = txtObj.AddComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.offsetMin = new Vector2(6f, 0f);
                tRect.offsetMax = new Vector2(-6f, 0f);

                TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 10f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
            else
            {
                stripObj = stripTr.gameObject;
                RectTransform sRect = stripObj.GetComponent<RectTransform>();
                if (sRect != null)
                {
                    sRect.sizeDelta = new Vector2(246f, 24f);
                    sRect.anchoredPosition = new Vector2(-18f, -175f);
                }
            }

            TextMeshProUGUI attrTmp = stripObj.GetComponentInChildren<TextMeshProUGUI>();
            if (attrTmp != null)
            {
                attrTmp.fontSize = 10f;
                if (job.id.Equals("student", StringComparison.OrdinalIgnoreCase))
                {
                    attrTmp.text = "<color=#FFA726>M.O.R.T.A.L. BASE 2 | +5 AP</color>";
                }
                else
                {
                    // M.O.R.T.A.L. Order: Might, Observation, Reflex, Tactics, Auspice, Leadership
                    attrTmp.text = $"<color=#EF5350>M:{job.str}</color> <color=#FFA726>O:{job.per}</color> <color=#66BB6A>R:{job.agi}</color> <color=#42A5F5>T:{job.intl}</color> <color=#FFD54F>A:{job.lck}</color> <color=#AB47BC>L:{job.cha}</color>";
                }
                SundayTypography.ApplyFont(attrTmp, FontRole.Mono);
            }
        }

        private static string GetRoleCategoryTag(string id)
        {
            switch ((id ?? "").ToLowerInvariant())
            {
                case "police": return "LAW ENFORCEMENT";
                case "worker": return "HEAVY LABOR";
                case "thief": return "INFILTRATION";
                case "doctor": return "MEDICAL CORPS";
                case "manager": return "EXECUTIVE LEAD";
                case "student": return "CUSTOM APTITUDE";
                default: return "VOCATION";
            }
        }

        private static string FormatPerksAndLoadout(JobData job)
        {
            string perks = "Special";
            if (job.perks != null && job.perks.Length >= 2)
                perks = $"{ToTitleCase(job.perks[0].name)} & {ToTitleCase(job.perks[1].name)}";
            else if (job.perks != null && job.perks.Length == 1)
                perks = ToTitleCase(job.perks[0].name);

            string gear = "Standard Gear";
            switch ((job.id ?? "").ToLowerInvariant())
            {
                case "police": gear = "Glock 19, 9mm Ammo x24"; break;
                case "worker": gear = "Sledgehammer, Duct Tape x2"; break;
                case "thief": gear = "Combat Knife, Lockpicks, Light"; break;
                case "doctor": gear = "First Aid Kit, Antibiotics, Scalpel"; break;
                case "manager": gear = "Luxury Watch, Golf Club"; break;
                case "student": gear = "Baseball Bat, Textbook"; break;
            }

            return $"<color=#FFA726><b>■ PERKS:</b></color> <color=#FFFFFF>{perks}</color>\n" +
                   $"<color=#4FC3F7><b>■ GEAR:</b></color>  <color=#CFD8DC>{gear}</color>";
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var words = input.ToLowerInvariant().Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
            }
            return string.Join(" ", words);
        }

        private void Update()
        {
            Vector3 targetScale = _isHovered ? new Vector3(1.024f, 1.024f, 1f) : Vector3.one;
            if ((transform.localScale - targetScale).sqrMagnitude > 0.00001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            ApplyHoverVisuals(true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            ApplyHoverVisuals(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_btnBg != null && eventData.pointerPress == _btnBg.gameObject) return;
            TriggerSelect();
        }

        private void TriggerSelect()
        {
            _onSelect?.Invoke();
        }

        private void ApplyHoverVisuals(bool hovered)
        {
            if (_cardBorder != null)
                _cardBorder.color = hovered ? _hoverBorderColor : _baseBorderColor;

            if (_cardOutline != null)
                _cardOutline.effectColor = hovered
                    ? new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.80f)
                    : new Color(0.10f, 0.15f, 0.22f, 0.50f);

            if (_innerBg != null)
                _innerBg.color = hovered ? _hoverInnerColor : _baseInnerColor;

            if (_jobImage != null && _jobImage.sprite != null)
                _jobImage.color = hovered ? Color.white : new Color(0.86f, 0.88f, 0.92f, 1f);

            if (_btnBg != null)
                _btnBg.color = hovered ? _hoverBtnColor : _baseBtnColor;

            if (_btnText != null)
                _btnText.color = hovered ? Color.white : new Color(0.90f, 0.94f, 0.98f);
        }
    }
}

