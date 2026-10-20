using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Step 4 of Character Creation: The Survival Bargain.
    /// Allows the survivor to voluntarily take 0 to 2 Drawbacks,
    /// each granting +2 free M.O.R.T.A.L. attribute points to distribute freely.
    /// </summary>
    public partial class CharacterCreationScreen : ScreenBase
    {
        [Header("Step 4: Survival Bargain")]
        [SerializeField] private GameObject bargainPanel;

        private List<string> selectedDrawbackIds = new List<string>();
        private int mortalAllocStr = 0;
        private int mortalAllocPer = 0;
        private int mortalAllocAgi = 0;
        private int mortalAllocInt = 0;
        private int mortalAllocLck = 0;
        private int mortalAllocCha = 0;

        // Cached UI References for Bargain Panel
        private TextMeshProUGUI _bargainBannerText;
        private Transform _drawbackCardsContainer;
        private Transform _mortalRowsContainer;
        private Button _bargainProceedButton;
        private TextMeshProUGUI _bargainProceedButtonText;

        public void ResetBargainState()
        {
            selectedDrawbackIds.Clear();
            mortalAllocStr = 0;
            mortalAllocPer = 0;
            mortalAllocAgi = 0;
            mortalAllocInt = 0;
            mortalAllocLck = 0;
            mortalAllocCha = 0;
        }

        public void ApplyBargainToCharacter()
        {
            if (character == null) return;

            // 1. Apply allocated M.O.R.T.A.L. points
            character.str += mortalAllocStr;
            character.per += mortalAllocPer;
            character.agi += mortalAllocAgi;
            character.intl += mortalAllocInt;
            character.lck += mortalAllocLck;
            character.cha += mortalAllocCha;

            // 2. Apply selected Drawbacks & their starting items / penalties
            character.selectedDrawbacks.Clear();
            foreach (var dId in selectedDrawbackIds)
            {
                character.selectedDrawbacks.Add(dId);
                var drawback = Array.Find(MortalStats.AllDrawbacks, d => d.id.Equals(dId, StringComparison.OrdinalIgnoreCase));
                if (drawback != null)
                {
                    drawback.onApply?.Invoke(character);
                    Sprite drawbackIcon = LoadSpriteSafe($"Assets/Sprites/Traits/{drawback.iconName}.png");
                    character.AddTrait(drawback.title, drawback.penaltySummary, drawbackIcon, isWeakness: true);
                    if (drawback.startingItems != null)
                    {
                        foreach (var it in drawback.startingItems)
                        {
                            character.AddItem(it.item, it.qty);
                        }
                    }
                }
            }
        }

        public void ShowBargainPanel()
        {
            HideAllPanels();
            EnsureBargainPanelBuilt();
            if (bargainPanel != null) bargainPanel.SetActive(true);
            RefreshBargainUI();
        }

        private int GetTotalBargainPointsEarned()
        {
            return selectedDrawbackIds.Count * 2;
        }

        private int GetTotalBargainPointsSpent()
        {
            return mortalAllocStr + mortalAllocPer + mortalAllocAgi + mortalAllocInt + mortalAllocLck + mortalAllocCha;
        }

        private int GetUnallocatedBargainPoints()
        {
            return GetTotalBargainPointsEarned() - GetTotalBargainPointsSpent();
        }

        private void ToggleDrawback(string drawbackId)
        {
            PlayClick();

            if (selectedDrawbackIds.Contains(drawbackId))
            {
                selectedDrawbackIds.Remove(drawbackId);
                ClampExcessAllocations();
            }
            else
            {
                if (selectedDrawbackIds.Count >= 2)
                {
                    // Max 2 drawbacks
                    return;
                }
                selectedDrawbackIds.Add(drawbackId);
            }

            RebuildCharacterFromSelections();
            RefreshBargainUI();
        }

        private void ClampExcessAllocations()
        {
            int earned = GetTotalBargainPointsEarned();
            int spent = GetTotalBargainPointsSpent();

            while (spent > earned)
            {
                if (mortalAllocCha > 0) { mortalAllocCha--; spent--; continue; }
                if (mortalAllocLck > 0) { mortalAllocLck--; spent--; continue; }
                if (mortalAllocInt > 0) { mortalAllocInt--; spent--; continue; }
                if (mortalAllocAgi > 0) { mortalAllocAgi--; spent--; continue; }
                if (mortalAllocPer > 0) { mortalAllocPer--; spent--; continue; }
                if (mortalAllocStr > 0) { mortalAllocStr--; spent--; continue; }
                break;
            }
        }

        private void ModifyMortalStat(MortalStatType stat, int delta)
        {
            int unallocated = GetUnallocatedBargainPoints();

            if (delta > 0 && unallocated <= 0) return;

            switch (stat)
            {
                case MortalStatType.Might:
                    if (delta < 0 && mortalAllocStr <= 0) return;
                    if (delta > 0 && (character.str + 1) > 10) return;
                    mortalAllocStr += delta;
                    break;
                case MortalStatType.Observation:
                    if (delta < 0 && mortalAllocPer <= 0) return;
                    if (delta > 0 && (character.per + 1) > 10) return;
                    mortalAllocPer += delta;
                    break;
                case MortalStatType.Reflex:
                    if (delta < 0 && mortalAllocAgi <= 0) return;
                    if (delta > 0 && (character.agi + 1) > 10) return;
                    mortalAllocAgi += delta;
                    break;
                case MortalStatType.Tactics:
                    if (delta < 0 && mortalAllocInt <= 0) return;
                    if (delta > 0 && (character.intl + 1) > 10) return;
                    mortalAllocInt += delta;
                    break;
                case MortalStatType.Auspice:
                    if (delta < 0 && mortalAllocLck <= 0) return;
                    if (delta > 0 && (character.lck + 1) > 10) return;
                    mortalAllocLck += delta;
                    break;
                case MortalStatType.Leadership:
                    if (delta < 0 && mortalAllocCha <= 0) return;
                    if (delta > 0 && (character.cha + 1) > 10) return;
                    mortalAllocCha += delta;
                    break;
            }

            PlayClick();
            RebuildCharacterFromSelections();
            RefreshBargainUI();
        }

        public void OnBackFromBargainClicked()
        {
            PlayClick();
            hobbyStep = 2;
            ShowHobbySelectionPanel();
        }

        public void OnProceedFromBargainClicked()
        {
            if (GetUnallocatedBargainPoints() > 0) return;

            PlayClick();
            ShowSummaryDossierPanel();
        }

        public void EnsureBargainPanelBuilt()
        {
            if (bargainPanel == null)
            {
                Transform parent = hobbySelectionPanel != null ? hobbySelectionPanel.transform.parent : transform;
                Transform found = parent.Find("BargainPanel");
                if (found != null)
                {
                    bargainPanel = found.gameObject;
                }
            }

            if (bargainPanel != null && bargainPanel.transform.Find("BargainBuiltMarker_V2") != null)
            {
                BindBargainReferences();
                // Panel was built in the editor and saved into the scene. Runtime (non-persistent)
                // onClick listeners are NOT serialized, so they must be re-attached at runtime.
                if (Application.isPlaying) WireBargainListeners();
                return;
            }

            if (bargainPanel != null)
            {
                for (int i = bargainPanel.transform.childCount - 1; i >= 0; i--)
                {
                    Transform c = bargainPanel.transform.GetChild(i);
                    if (Application.isPlaying) Destroy(c.gameObject);
                    else DestroyImmediate(c.gameObject);
                }
            }

            if (bargainPanel == null)
            {
                Transform parent = hobbySelectionPanel != null ? hobbySelectionPanel.transform.parent : transform;
                bargainPanel = new GameObject("BargainPanel");
                bargainPanel.transform.SetParent(parent, false);

                RectTransform rect = bargainPanel.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            // Build Marker
            GameObject marker = new GameObject("BargainBuiltMarker_V2");
            marker.transform.SetParent(bargainPanel.transform, false);

            // Dark Atmosphere Background
            Image bg = bargainPanel.GetComponent<Image>() ?? bargainPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.09f, 0.98f);

            // 1. Header Area
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(bargainPanel.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.5f, 1f);
            hRect.anchorMax = new Vector2(0.5f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(1120f, 95f);
            hRect.anchoredPosition = new Vector2(0f, -18f);

            CreateUIText(headerObj.transform, "Badge",
                "<b><color=#EF5350>[ STEP 04 // PROTOCOL: SURVIVAL BARGAIN ]</color></b>",
                11.5f, TextAlignmentOptions.Center, new Color(0.93f, 0.33f, 0.31f), FontRole.Mono,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

            CreateUIText(headerObj.transform, "Title",
                "THE SURVIVAL BARGAIN",
                30f, TextAlignmentOptions.Center, new Color(0.95f, 0.85f, 0.40f), FontRole.Display,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -56f), new Vector2(0f, -16f));

            CreateUIText(headerObj.transform, "Subtitle",
                "VOLUNTARILY ACCEPT PHYSICAL OR PSYCHOLOGICAL FLAWS TO ACQUIRE EXTRA M.O.R.T.A.L. ATTRIBUTE POINTS (MAX 2).",
                12f, TextAlignmentOptions.Center, new Color(0.70f, 0.78f, 0.86f), FontRole.Body,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 22f));

            // 2. Status Banner (Pill Bar)
            GameObject bannerObj = new GameObject("BargainBanner");
            bannerObj.transform.SetParent(bargainPanel.transform, false);
            RectTransform bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.sizeDelta = new Vector2(1120f, 34f);
            bRect.anchoredPosition = new Vector2(0f, -118f);

            Image bBg = bannerObj.AddComponent<Image>();
            bBg.color = new Color(0.06f, 0.10f, 0.15f, 0.95f);
            Outline bOl = bannerObj.AddComponent<Outline>();
            bOl.effectColor = new Color(0.24f, 0.36f, 0.48f, 0.8f);
            bOl.effectDistance = new Vector2(1f, -1f);

            _bargainBannerText = CreateUIText(bannerObj.transform, "BannerText",
                "DRAWBACKS ACCEPTED: <b>[ 0 / 2 ]</b>    |    UNALLOCATED ATTRIBUTE POINTS: <b>[ +0 ]</b>",
                12.5f, TextAlignmentOptions.Center, new Color(0.90f, 0.94f, 0.98f), FontRole.Mono,
                Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            // 3. Main Split Container (Left = Drawbacks, Right = M.O.R.T.A.L. Allocator)
            GameObject splitObj = new GameObject("SplitContainer");
            splitObj.transform.SetParent(bargainPanel.transform, false);
            RectTransform sRect = splitObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.pivot = new Vector2(0.5f, 0.5f);
            sRect.sizeDelta = new Vector2(1120f, 490f);
            sRect.anchoredPosition = new Vector2(0f, -22f);

            BuildDrawbacksColumn(splitObj.transform);
            BuildMortalAllocatorColumn(splitObj.transform);

            // 4. Bottom Footer Navigation
            BuildBargainFooter(bargainPanel.transform);

            SundayTypography.ApplyHierarchy(bargainPanel.transform);
            BindBargainReferences();
        }

        private void BuildDrawbacksColumn(Transform parent)
        {
            GameObject colObj = new GameObject("Col_Drawbacks");
            colObj.transform.SetParent(parent, false);
            RectTransform colRect = colObj.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(0f, 0f);
            colRect.anchorMax = new Vector2(0.51f, 1f);
            colRect.offsetMin = Vector2.zero;
            colRect.offsetMax = new Vector2(-8f, 0f);

            // Column Header
            CreateUIText(colObj.transform, "ColHeader",
                "<b><color=#EF5350>■ AVAILABLE SURVIVAL DRAWBACKS</color></b>  <color=#90A4AE>(MAX 2)</color>",
                13.5f, TextAlignmentOptions.Left, Color.white, FontRole.Display,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(4f, -26f), new Vector2(-4f, 0f));

            // Scroll Area
            GameObject scrollObj = new GameObject("DrawbackScroll");
            scrollObj.transform.SetParent(colObj.transform, false);
            RectTransform scrRect = scrollObj.AddComponent<RectTransform>();
            scrRect.anchorMin = Vector2.zero;
            scrRect.anchorMax = Vector2.one;
            scrRect.offsetMin = Vector2.zero;
            scrRect.offsetMax = new Vector2(0f, -32f);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 28f;

            Image scrImg = scrollObj.AddComponent<Image>();
            scrImg.color = Color.clear;
            scrImg.raycastTarget = true;

            GameObject vpObj = new GameObject("Viewport");
            vpObj.transform.SetParent(scrollObj.transform, false);
            RectTransform vpRect = vpObj.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            Image vpImg = vpObj.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            vpObj.AddComponent<RectMask2D>();
            sr.viewport = vpRect;

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(vpObj.transform, false);
            RectTransform cRect = contentObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.sizeDelta = new Vector2(0f, 850f);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(4, 8, 4, 16);

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = cRect;
            _drawbackCardsContainer = contentObj.transform;

            // Generate Drawback Cards
            foreach (var db in MortalStats.AllDrawbacks)
            {
                CreateDrawbackCardItem(_drawbackCardsContainer, db);
            }
        }

        private void CreateDrawbackCardItem(Transform parent, DrawbackData db)
        {
            GameObject card = new GameObject("Drawback_" + db.id);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.minHeight = 88f;
            le.preferredHeight = 88f;

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.10f, 0.14f, 0.94f);

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = new Color(0.20f, 0.28f, 0.36f, 0.7f);
            ol.effectDistance = new Vector2(1f, -1f);

            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capturedId = db.id;
            btn.onClick.AddListener(() => ToggleDrawback(capturedId));

            // Content Left Strip
            GameObject strip = new GameObject("LeftStrip");
            strip.transform.SetParent(card.transform, false);
            RectTransform sRect = strip.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(0f, 1f);
            sRect.sizeDelta = new Vector2(4f, 0f);
            sRect.anchoredPosition = new Vector2(2f, 0f);
            Image sImg = strip.AddComponent<Image>();
            sImg.color = new Color(0.85f, 0.25f, 0.25f, 0.85f);

            // Trait / Drawback Graphic Icon Box (56x56)
            GameObject iconBox = new GameObject("IconBox");
            iconBox.transform.SetParent(card.transform, false);
            RectTransform ibRect = iconBox.AddComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0f, 0.5f);
            ibRect.anchorMax = new Vector2(0f, 0.5f);
            ibRect.pivot = new Vector2(0f, 0.5f);
            ibRect.sizeDelta = new Vector2(56f, 56f);
            ibRect.anchoredPosition = new Vector2(14f, 0f);

            Image ibBg = iconBox.AddComponent<Image>();
            ibBg.color = new Color(0.04f, 0.06f, 0.09f, 0.95f);

            Outline ibOl = iconBox.AddComponent<Outline>();
            ibOl.effectColor = new Color(0.70f, 0.20f, 0.20f, 0.5f);
            ibOl.effectDistance = new Vector2(1f, -1f);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconBox.transform, false);
            RectTransform iRect = iconObj.AddComponent<RectTransform>();
            iRect.anchorMin = Vector2.zero;
            iRect.anchorMax = Vector2.one;
            iRect.offsetMin = new Vector2(4f, 4f);
            iRect.offsetMax = new Vector2(-4f, -4f);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            Sprite traitSprite = LoadSpriteSafe($"Assets/Sprites/Traits/{db.iconName}.png");
            if (traitSprite != null)
            {
                iconImg.sprite = traitSprite;
                iconImg.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
            }
            else
            {
                iconImg.gameObject.SetActive(false);
            }

            // Title + Bonus Badge (Shifted right of IconBox: x = 80px)
            string catTag = $"[{db.category.ToString().ToUpper()}]";
            CreateUIText(card.transform, "TitleLine",
                $"<b><color=#FFFFFF>{db.title}</color></b>   <color=#4FC3F7><size=11>{catTag}</size></color>   <color=#66BB6A><b>[+2 PTS]</b></color>",
                13.5f, TextAlignmentOptions.Left, Color.white, FontRole.Display,
                new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(80f, 0f), new Vector2(-116f, -6f));

            // Penalty Description (Shifted right of IconBox: x = 80px)
            TextMeshProUGUI penTmp = CreateUIText(card.transform, "PenaltyText",
                $"<color=#EF5350><b>FLAW:</b></color> <color=#CFD8DC>{db.penaltySummary}</color>",
                11f, TextAlignmentOptions.Left, new Color(0.85f, 0.90f, 0.95f), FontRole.Body,
                new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(80f, 6f), new Vector2(-116f, 0f));
            if (penTmp != null)
            {
                penTmp.textWrappingMode = TextWrappingModes.Normal;
            }

            // Selection Status Pill Button
            GameObject pillObj = new GameObject("SelectBadge");
            pillObj.transform.SetParent(card.transform, false);
            RectTransform pRect = pillObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(1f, 0.5f);
            pRect.anchorMax = new Vector2(1f, 0.5f);
            pRect.pivot = new Vector2(1f, 0.5f);
            pRect.sizeDelta = new Vector2(98f, 32f);
            pRect.anchoredPosition = new Vector2(-12f, 0f);

            Image pBg = pillObj.AddComponent<Image>();
            pBg.color = new Color(0.12f, 0.16f, 0.22f, 0.9f);

            CreateUIText(pillObj.transform, "Label",
                "SELECT",
                11f, TextAlignmentOptions.Center, Color.white, FontRole.Mono,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void BuildMortalAllocatorColumn(Transform parent)
        {
            GameObject colObj = new GameObject("Col_Allocator");
            colObj.transform.SetParent(parent, false);
            RectTransform colRect = colObj.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(0.53f, 0f);
            colRect.anchorMax = new Vector2(1f, 1f);
            colRect.offsetMin = new Vector2(8f, 0f);
            colRect.offsetMax = Vector2.zero;

            // Column Header
            CreateUIText(colObj.transform, "ColHeader",
                "<b><color=#F5BF42>■ M.O.R.T.A.L. ATTRIBUTES ALLOCATION</color></b>",
                13.5f, TextAlignmentOptions.Left, Color.white, FontRole.Display,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(4f, -26f), new Vector2(-4f, 0f));

            // Rows Container
            GameObject rowsObj = new GameObject("RowsContainer");
            rowsObj.transform.SetParent(colObj.transform, false);
            RectTransform rRect = rowsObj.AddComponent<RectTransform>();
            rRect.anchorMin = Vector2.zero;
            rRect.anchorMax = Vector2.one;
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = new Vector2(0f, -32f);

            VerticalLayoutGroup vlg = rowsObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.padding = new RectOffset(4, 4, 4, 4);

            _mortalRowsContainer = rowsObj.transform;

            foreach (var descriptor in MortalStats.StatDescriptors)
            {
                BuildMortalRowItem(_mortalRowsContainer, descriptor);
            }
        }

        private void BuildMortalRowItem(Transform parent, (MortalStatType type, char letter, string name, string code, string summary) desc)
        {
            GameObject row = new GameObject("Row_" + desc.type.ToString());
            row.transform.SetParent(parent, false);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.09f, 0.13f, 0.95f);

            Outline ol = row.AddComponent<Outline>();
            ol.effectColor = new Color(0.18f, 0.26f, 0.34f, 0.6f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Left Letter Accent
            Color accentColor = GetMortalStatColor(desc.type);
            GameObject letBox = new GameObject("LetterBox");
            letBox.transform.SetParent(row.transform, false);
            RectTransform letRect = letBox.AddComponent<RectTransform>();
            letRect.anchorMin = new Vector2(0f, 0f);
            letRect.anchorMax = new Vector2(0f, 1f);
            letRect.sizeDelta = new Vector2(36f, 0f);
            letRect.anchoredPosition = new Vector2(18f, 0f);

            Image letBg = letBox.AddComponent<Image>();
            letBg.color = new Color(accentColor.r * 0.25f, accentColor.g * 0.25f, accentColor.b * 0.25f, 0.8f);

            CreateUIText(letBox.transform, "LetterText",
                $"<b>{desc.letter}</b>",
                18f, TextAlignmentOptions.Center, accentColor, FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Name + Summary
            CreateUIText(row.transform, "NameLine",
                $"<color=#{ColorUtility.ToHtmlStringRGB(accentColor)}><b>{desc.name}</b></color> <color=#90A4AE>({desc.code})</color>",
                13.5f, TextAlignmentOptions.Left, Color.white, FontRole.Display,
                new Vector2(0f, 0.5f), new Vector2(0.6f, 1f), new Vector2(44f, 0f), new Vector2(0f, -4f));

            CreateUIText(row.transform, "SummaryLine",
                $"<color=#90A4AE>{desc.summary}</color>",
                10.5f, TextAlignmentOptions.Left, new Color(0.70f, 0.78f, 0.86f), FontRole.Body,
                new Vector2(0f, 0f), new Vector2(0.65f, 0.5f), new Vector2(44f, 4f), new Vector2(0f, 0f));

            // Controls on Right: [ - ] [ Value ] [ + ]
            GameObject ctrlObj = new GameObject("Controls");
            ctrlObj.transform.SetParent(row.transform, false);
            RectTransform cRect = ctrlObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 0.5f);
            cRect.anchorMax = new Vector2(1f, 0.5f);
            cRect.pivot = new Vector2(1f, 0.5f);
            cRect.sizeDelta = new Vector2(160f, 36f);
            cRect.anchoredPosition = new Vector2(-10f, 0f);

            // Minus Button
            GameObject minusBtnObj = new GameObject("Btn_Minus");
            minusBtnObj.transform.SetParent(ctrlObj.transform, false);
            RectTransform mRect = minusBtnObj.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0f, 0.5f);
            mRect.anchorMax = new Vector2(0f, 0.5f);
            mRect.pivot = new Vector2(0f, 0.5f);
            mRect.sizeDelta = new Vector2(34f, 32f);
            mRect.anchoredPosition = new Vector2(0f, 0f);

            Image mBg = minusBtnObj.AddComponent<Image>();
            mBg.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
            Button mBtn = minusBtnObj.AddComponent<Button>();
            mBtn.targetGraphic = mBg;
            MortalStatType capturedType = desc.type;
            mBtn.onClick.AddListener(() => ModifyMortalStat(capturedType, -1));

            CreateUIText(minusBtnObj.transform, "Text", "<b>-</b>", 16f, TextAlignmentOptions.Center, Color.white, FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Value Display
            GameObject valObj = new GameObject("ValText");
            valObj.transform.SetParent(ctrlObj.transform, false);
            RectTransform vRect = valObj.AddComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.5f, 0.5f);
            vRect.anchorMax = new Vector2(0.5f, 0.5f);
            vRect.pivot = new Vector2(0.5f, 0.5f);
            vRect.sizeDelta = new Vector2(80f, 32f);

            CreateUIText(valObj.transform, "Text", "<mspace=0.6em><b>03</b></mspace>", 15f, TextAlignmentOptions.Center, Color.white, FontRole.Mono,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Plus Button
            GameObject plusBtnObj = new GameObject("Btn_Plus");
            plusBtnObj.transform.SetParent(ctrlObj.transform, false);
            RectTransform pRect = plusBtnObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(1f, 0.5f);
            pRect.anchorMax = new Vector2(1f, 0.5f);
            pRect.pivot = new Vector2(1f, 0.5f);
            pRect.sizeDelta = new Vector2(34f, 32f);
            pRect.anchoredPosition = new Vector2(0f, 0f);

            Image pBg = plusBtnObj.AddComponent<Image>();
            pBg.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
            Button pBtn = plusBtnObj.AddComponent<Button>();
            pBtn.targetGraphic = pBg;
            pBtn.onClick.AddListener(() => ModifyMortalStat(capturedType, 1));

            CreateUIText(plusBtnObj.transform, "Text", "<b>+</b>", 16f, TextAlignmentOptions.Center, Color.white, FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void BuildBargainFooter(Transform parent)
        {
            GameObject footerObj = new GameObject("Footer");
            footerObj.transform.SetParent(parent, false);
            RectTransform fRect = footerObj.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0f);
            fRect.anchorMax = new Vector2(0.5f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.sizeDelta = new Vector2(1120f, 52f);
            fRect.anchoredPosition = new Vector2(0f, 16f);

            // Back Button
            GameObject backObj = new GameObject("Btn_BackToHobbies");
            backObj.transform.SetParent(footerObj.transform, false);
            RectTransform bRect = backObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 0.5f);
            bRect.anchorMax = new Vector2(0f, 0.5f);
            bRect.pivot = new Vector2(0f, 0.5f);
            bRect.sizeDelta = new Vector2(210f, 44f);
            bRect.anchoredPosition = new Vector2(0f, 0f);

            Image bBg = backObj.AddComponent<Image>();
            bBg.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);
            Button bBtn = backObj.AddComponent<Button>();
            bBtn.targetGraphic = bBg;
            bBtn.onClick.AddListener(OnBackFromBargainClicked);

            CreateUIText(backObj.transform, "Label", "<b><< EDIT HOBBIES</b>", 12.5f, TextAlignmentOptions.Center, new Color(0.80f, 0.88f, 0.95f), FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Proceed Button
            GameObject fwdObj = new GameObject("Btn_ProceedToDossier");
            fwdObj.transform.SetParent(footerObj.transform, false);
            RectTransform fwdRect = fwdObj.AddComponent<RectTransform>();
            fwdRect.anchorMin = new Vector2(1f, 0.5f);
            fwdRect.anchorMax = new Vector2(1f, 0.5f);
            fwdRect.pivot = new Vector2(1f, 0.5f);
            fwdRect.sizeDelta = new Vector2(250f, 44f);
            fwdRect.anchoredPosition = new Vector2(0f, 0f);

            Image fwdBg = fwdObj.AddComponent<Image>();
            fwdBg.color = new Color(0.85f, 0.55f, 0.15f, 0.95f);
            _bargainProceedButton = fwdObj.AddComponent<Button>();
            _bargainProceedButton.targetGraphic = fwdBg;
            _bargainProceedButton.onClick.AddListener(OnProceedFromBargainClicked);

            _bargainProceedButtonText = CreateUIText(fwdObj.transform, "Label", "<b>PROCEED TO DOSSIER >></b>", 13f, TextAlignmentOptions.Center, Color.black, FontRole.Display,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        /// <summary>
        /// Re-attaches all runtime onClick listeners of the Bargain panel (drawback cards, M.O.R.T.A.L. +/- buttons,
        /// Back and Proceed buttons). Required because listeners added via AddListener in the editor-time build
        /// are not saved into the scene file.
        /// </summary>
        private void WireBargainListeners()
        {
            if (bargainPanel == null) return;

            // Drawback cards
            Transform cardsRoot = bargainPanel.transform.Find("SplitContainer/Col_Drawbacks/DrawbackScroll/Viewport/Content");
            if (cardsRoot != null)
            {
                for (int i = 0; i < cardsRoot.childCount; i++)
                {
                    Transform card = cardsRoot.GetChild(i);
                    if (!card.name.StartsWith("Drawback_")) continue;
                    Button btn = card.GetComponent<Button>();
                    if (btn == null) continue;
                    string capturedId = card.name.Substring("Drawback_".Length);
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ToggleDrawback(capturedId));
                }
            }

            // M.O.R.T.A.L. +/- rows
            Transform rowsRoot = bargainPanel.transform.Find("SplitContainer/Col_Allocator/RowsContainer");
            if (rowsRoot != null)
            {
                for (int i = 0; i < rowsRoot.childCount; i++)
                {
                    Transform row = rowsRoot.GetChild(i);
                    if (!row.name.StartsWith("Row_")) continue;
                    if (!Enum.TryParse(row.name.Substring("Row_".Length), out MortalStatType statType)) continue;

                    Button minus = row.Find("Controls/Btn_Minus")?.GetComponent<Button>();
                    Button plus = row.Find("Controls/Btn_Plus")?.GetComponent<Button>();
                    if (minus != null)
                    {
                        minus.onClick.RemoveAllListeners();
                        minus.onClick.AddListener(() => ModifyMortalStat(statType, -1));
                    }
                    if (plus != null)
                    {
                        plus.onClick.RemoveAllListeners();
                        plus.onClick.AddListener(() => ModifyMortalStat(statType, 1));
                    }
                }
            }

            // Footer navigation
            Button back = bargainPanel.transform.Find("Footer/Btn_BackToHobbies")?.GetComponent<Button>();
            if (back != null)
            {
                back.onClick.RemoveAllListeners();
                back.onClick.AddListener(OnBackFromBargainClicked);
            }

            Button proceed = bargainPanel.transform.Find("Footer/Btn_ProceedToDossier")?.GetComponent<Button>();
            if (proceed != null)
            {
                proceed.onClick.RemoveAllListeners();
                proceed.onClick.AddListener(OnProceedFromBargainClicked);
            }
        }

        private void BindBargainReferences()
        {
            if (bargainPanel == null) return;

            Transform banner = bargainPanel.transform.Find("BargainBanner/BannerText");
            if (banner != null) _bargainBannerText = banner.GetComponent<TextMeshProUGUI>();

            _drawbackCardsContainer = bargainPanel.transform.Find("SplitContainer/Col_Drawbacks/DrawbackScroll/Viewport/Content");
            _mortalRowsContainer = bargainPanel.transform.Find("SplitContainer/Col_Allocator/RowsContainer");

            Transform fwd = bargainPanel.transform.Find("Footer/Btn_ProceedToDossier");
            if (fwd != null)
            {
                _bargainProceedButton = fwd.GetComponent<Button>();
                _bargainProceedButtonText = fwd.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void RefreshBargainUI()
        {
            if (bargainPanel == null) return;
            BindBargainReferences();

            int drawbacksCount = selectedDrawbackIds.Count;
            int totalBonus = GetTotalBargainPointsEarned();
            int unallocated = GetUnallocatedBargainPoints();

            // 1. Update Banner Text
            if (_bargainBannerText != null)
            {
                string drawbackColor = drawbacksCount > 0 ? "#EF5350" : "#90A4AE";
                string pointsColor = unallocated > 0 ? "#F5BF42" : "#66BB6A";
                _bargainBannerText.text =
                    $"SURVIVAL DRAWBACKS: <color={drawbackColor}><b>[ {drawbacksCount} / 2 SELECTED ]</b></color>    |    " +
                    $"TOTAL BONUS EARNED: <color=#66BB6A><b>+{totalBonus} PTS</b></color>    |    " +
                    $"UNALLOCATED: <color={pointsColor}><b>[ {unallocated} PTS ]</b></color>";
            }

            // 2. Update Drawback Card Items Visuals
            if (_drawbackCardsContainer != null)
            {
                for (int i = 0; i < _drawbackCardsContainer.childCount; i++)
                {
                    Transform card = _drawbackCardsContainer.GetChild(i);
                    string id = card.name.Replace("Drawback_", "");
                    bool isSelected = selectedDrawbackIds.Contains(id);

                    Image bg = card.GetComponent<Image>();
                    Outline ol = card.GetComponent<Outline>();
                    Transform badge = card.Find("SelectBadge");
                    Image badgeBg = badge?.GetComponent<Image>();
                    TextMeshProUGUI badgeLabel = badge?.GetComponentInChildren<TextMeshProUGUI>();
                    Transform iconBox = card.Find("IconBox");
                    Outline ibOl = iconBox?.GetComponent<Outline>();

                    if (isSelected)
                    {
                        if (bg != null) bg.color = new Color(0.14f, 0.08f, 0.08f, 0.98f);
                        if (ol != null) { ol.effectColor = new Color(0.92f, 0.28f, 0.28f, 0.95f); ol.effectDistance = new Vector2(2f, -2f); }
                        if (ibOl != null) ibOl.effectColor = new Color(0.95f, 0.35f, 0.35f, 0.95f);
                        if (badgeBg != null) badgeBg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
                        if (badgeLabel != null) badgeLabel.text = "ACCEPTED";
                    }
                    else
                    {
                        bool canSelect = drawbacksCount < 2;
                        if (bg != null) bg.color = canSelect ? new Color(0.07f, 0.10f, 0.14f, 0.94f) : new Color(0.05f, 0.07f, 0.09f, 0.6f);
                        if (ol != null) { ol.effectColor = new Color(0.20f, 0.28f, 0.36f, 0.6f); ol.effectDistance = new Vector2(1f, -1f); }
                        if (ibOl != null) ibOl.effectColor = canSelect ? new Color(0.70f, 0.20f, 0.20f, 0.5f) : new Color(0.35f, 0.15f, 0.15f, 0.3f);
                        if (badgeBg != null) badgeBg.color = canSelect ? new Color(0.12f, 0.16f, 0.22f, 0.9f) : new Color(0.08f, 0.10f, 0.13f, 0.4f);
                        if (badgeLabel != null) badgeLabel.text = canSelect ? "SELECT" : "LOCKED";
                    }
                }
            }

            // 3. Update M.O.R.T.A.L. Row Controls
            if (_mortalRowsContainer != null)
            {
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Might"),       character.str,  mortalAllocStr, unallocated);
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Observation"), character.per,  mortalAllocPer, unallocated);
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Reflex"),      character.agi,  mortalAllocAgi, unallocated);
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Tactics"),     character.intl, mortalAllocInt, unallocated);
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Auspice"),     character.lck,  mortalAllocLck, unallocated);
                UpdateMortalRowUI(_mortalRowsContainer.Find("Row_Leadership"),  character.cha,  mortalAllocCha, unallocated);
            }

            // 4. Update Proceed Button
            if (_bargainProceedButton != null)
            {
                bool canProceed = unallocated == 0;
                _bargainProceedButton.interactable = canProceed;
                Image btnBg = _bargainProceedButton.GetComponent<Image>();

                if (canProceed)
                {
                    if (btnBg != null) btnBg.color = new Color(0.85f, 0.55f, 0.15f, 1f);
                    if (_bargainProceedButtonText != null) _bargainProceedButtonText.text = "<b>PROCEED TO DOSSIER >></b>";
                }
                else
                {
                    if (btnBg != null) btnBg.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
                    if (_bargainProceedButtonText != null) _bargainProceedButtonText.text = $"<b>ALLOCATE {unallocated} PTS TO CONTINUE</b>";
                }
            }
        }

        private void UpdateMortalRowUI(Transform row, int totalVal, int allocVal, int unallocated)
        {
            if (row == null) return;

            Transform ctrl = row.Find("Controls");
            if (ctrl == null) return;

            TextMeshProUGUI valTmp = ctrl.Find("ValText")?.GetComponentInChildren<TextMeshProUGUI>();
            Button minusBtn = ctrl.Find("Btn_Minus")?.GetComponent<Button>();
            Button plusBtn = ctrl.Find("Btn_Plus")?.GetComponent<Button>();

            if (valTmp != null)
            {
                string bonusTag = allocVal > 0 ? $" <color=#66BB6A><size=11>(+{allocVal})</size></color>" : "";
                valTmp.text = $"<mspace=0.6em><b>{totalVal:00}</b></mspace>{bonusTag}";
            }

            if (minusBtn != null)
            {
                minusBtn.interactable = allocVal > 0;
            }

            if (plusBtn != null)
            {
                plusBtn.interactable = unallocated > 0 && totalVal < 10;
            }
        }

        private static Color GetMortalStatColor(MortalStatType type)
        {
            switch (type)
            {
                case MortalStatType.Might:       return new Color(0.93f, 0.33f, 0.31f); // #EF5350 Red
                case MortalStatType.Observation: return new Color(1.00f, 0.65f, 0.15f); // #FFA726 Orange
                case MortalStatType.Reflex:      return new Color(0.40f, 0.73f, 0.42f); // #66BB6A Green
                case MortalStatType.Tactics:     return new Color(0.26f, 0.65f, 0.96f); // #42A5F5 Blue
                case MortalStatType.Auspice:     return new Color(1.00f, 0.84f, 0.31f); // #FFD54F Gold
                case MortalStatType.Leadership:  return new Color(0.67f, 0.28f, 0.74f); // #AB47BC Purple
                default:                         return Color.white;
            }
        }
    }
}
