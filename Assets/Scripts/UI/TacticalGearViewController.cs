using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Tactical Gear & Inventory Workstation Controller for Sunday: Last Breath.
    /// Manages Active Loadout (5-slot paper doll), Backpack 12-Slot Encumbrance Grid,
    /// and the Interactive Tactical Item Dossier / Room Stash inspector.
    /// </summary>
    public class TacticalGearViewController : MonoBehaviour
    {
        public enum InventoryFilter
        {
            All,
            Weapons,
            Consumables,
            Apparel,
            Tools
        }

        public enum SelectedSource
        {
            None,
            Backpack,
            Equipped,
            Floor
        }

        [Header("Paper Doll Active Loadout Slots")]
        [SerializeField] private Button slotHeadBtn;
        [SerializeField] private Image slotHeadIcon;
        [SerializeField] private TextMeshProUGUI slotHeadLabel;
        [SerializeField] private Outline slotHeadOutline;

        [SerializeField] private Button slotBodyBtn;
        [SerializeField] private Image slotBodyIcon;
        [SerializeField] private TextMeshProUGUI slotBodyLabel;
        [SerializeField] private Outline slotBodyOutline;

        [SerializeField] private Button slotWpnBtn;
        [SerializeField] private Image slotWpnIcon;
        [SerializeField] private TextMeshProUGUI slotWpnLabel;
        [SerializeField] private Outline slotWpnOutline;

        [SerializeField] private Button slotLegsBtn;
        [SerializeField] private Image slotLegsIcon;
        [SerializeField] private TextMeshProUGUI slotLegsLabel;
        [SerializeField] private Outline slotLegsOutline;

        [SerializeField] private Button slotFeetBtn;
        [SerializeField] private Image slotFeetIcon;
        [SerializeField] private TextMeshProUGUI slotFeetLabel;
        [SerializeField] private Outline slotFeetOutline;

        [Header("Backpack Grid & Capacity")]
        [SerializeField] private Transform backpackGridRoot;
        [SerializeField] private TextMeshProUGUI backpackCountText;
        [SerializeField] private Image backpackCapacityFill;
        [SerializeField] private TextMeshProUGUI weightText;

        [Header("Category Filter Buttons")]
        [SerializeField] private Button btnFilterAll;
        [SerializeField] private Button btnFilterWeapons;
        [SerializeField] private Button btnFilterConsumables;
        [SerializeField] private Button btnFilterApparel;
        [SerializeField] private Button btnFilterTools;

        [Header("Right Column Dual Mode Tabs")]
        [SerializeField] private Button tabBtnDossier;
        [SerializeField] private Button tabBtnFloor;
        [SerializeField] private TextMeshProUGUI tabFloorCountText;
        [SerializeField] private GameObject dossierPanel;
        [SerializeField] private GameObject floorPanel;

        [Header("Item Dossier Inspector Elements")]
        [SerializeField] private GameObject dossierEmptyState;
        [SerializeField] private GameObject dossierDetailContent;
        [SerializeField] private TextMeshProUGUI dossierItemName;
        [SerializeField] private TextMeshProUGUI dossierCategoryBadge;
        [SerializeField] private Image dossierIcon;
        [SerializeField] private TextMeshProUGUI dossierSubInfo;
        [SerializeField] private TextMeshProUGUI dossierStatsText;
        [SerializeField] private TextMeshProUGUI dossierDescription;
        [SerializeField] private TextMeshProUGUI dossierToast;

        [Header("Dossier Action Buttons")]
        [SerializeField] private Button btnActionEquip;
        [SerializeField] private Button btnActionUnequip;
        [SerializeField] private Button btnActionUse;
        [SerializeField] private Button btnActionDrop;
        [SerializeField] private Button btnActionTake;

        [Header("Floor / Room Stash Elements")]
        [SerializeField] private Transform floorGridRoot;
        [SerializeField] private TextMeshProUGUI floorEmptyNotice;
        [SerializeField] private Button btnTakeAllFloor;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip equipSFX;
        [SerializeField] private AudioClip unequipSFX;
        [SerializeField] private AudioClip useSFX;
        [SerializeField] private AudioClip dropSFX;
        [SerializeField] private AudioClip lootSFX;

        // Runtime State
        private CharacterData _player;
        private List<InventoryEntry> _floorItems;
        private InventoryFilter _currentFilter = InventoryFilter.All;

        private SelectedSource _selectedSource = SelectedSource.None;
        private string _selectedItemId = "";
        private string _selectedSlot = "";
        private int _selectedBackpackIndex = -1;
        private int _selectedFloorIndex = -1;

        // Events for ApartmentHubScreen to synchronize Top HUD vitals & indicators
        public event Action OnVitalsUpdated;
        public event Action OnInventoryUpdated;

        private bool _listenersHooked = false;

        private void Awake()
        {
            EnsureListeners();
        }

        public void EnsureListeners()
        {
            if (_listenersHooked) return;
            _listenersHooked = true;
            SetupSlotListeners();
            SetupFilterListeners();
            SetupTabListeners();
            SetupActionListeners();
        }

        private void SetupSlotListeners()
        {
            if (slotHeadBtn != null) { slotHeadBtn.onClick.RemoveAllListeners(); slotHeadBtn.onClick.AddListener(() => OnEquipSlotClicked("HEAD")); }
            if (slotBodyBtn != null) { slotBodyBtn.onClick.RemoveAllListeners(); slotBodyBtn.onClick.AddListener(() => OnEquipSlotClicked("BODY")); }
            if (slotWpnBtn != null) { slotWpnBtn.onClick.RemoveAllListeners(); slotWpnBtn.onClick.AddListener(() => OnEquipSlotClicked("WPN")); }
            if (slotLegsBtn != null) { slotLegsBtn.onClick.RemoveAllListeners(); slotLegsBtn.onClick.AddListener(() => OnEquipSlotClicked("LEGS")); }
            if (slotFeetBtn != null) { slotFeetBtn.onClick.RemoveAllListeners(); slotFeetBtn.onClick.AddListener(() => OnEquipSlotClicked("FEET")); }
        }

        private void SetupFilterListeners()
        {
            if (btnFilterAll != null) { btnFilterAll.onClick.RemoveAllListeners(); btnFilterAll.onClick.AddListener(() => SetFilter(InventoryFilter.All)); }
            if (btnFilterWeapons != null) { btnFilterWeapons.onClick.RemoveAllListeners(); btnFilterWeapons.onClick.AddListener(() => SetFilter(InventoryFilter.Weapons)); }
            if (btnFilterConsumables != null) { btnFilterConsumables.onClick.RemoveAllListeners(); btnFilterConsumables.onClick.AddListener(() => SetFilter(InventoryFilter.Consumables)); }
            if (btnFilterApparel != null) { btnFilterApparel.onClick.RemoveAllListeners(); btnFilterApparel.onClick.AddListener(() => SetFilter(InventoryFilter.Apparel)); }
            if (btnFilterTools != null) { btnFilterTools.onClick.RemoveAllListeners(); btnFilterTools.onClick.AddListener(() => SetFilter(InventoryFilter.Tools)); }
        }

        private void SetupTabListeners()
        {
            if (tabBtnDossier != null) { tabBtnDossier.onClick.RemoveAllListeners(); tabBtnDossier.onClick.AddListener(() => SwitchRightTab(true)); }
            if (tabBtnFloor != null) { tabBtnFloor.onClick.RemoveAllListeners(); tabBtnFloor.onClick.AddListener(() => SwitchRightTab(false)); }
            if (btnTakeAllFloor != null) { btnTakeAllFloor.onClick.RemoveAllListeners(); btnTakeAllFloor.onClick.AddListener(TakeAllFloorItems); }
        }

        private void SetupActionListeners()
        {
            if (btnActionEquip != null) { btnActionEquip.onClick.RemoveAllListeners(); btnActionEquip.onClick.AddListener(ExecuteEquip); }
            if (btnActionUnequip != null) { btnActionUnequip.onClick.RemoveAllListeners(); btnActionUnequip.onClick.AddListener(ExecuteUnequip); }
            if (btnActionUse != null) { btnActionUse.onClick.RemoveAllListeners(); btnActionUse.onClick.AddListener(ExecuteUse); }
            if (btnActionDrop != null) { btnActionDrop.onClick.RemoveAllListeners(); btnActionDrop.onClick.AddListener(ExecuteDrop); }
            if (btnActionTake != null) { btnActionTake.onClick.RemoveAllListeners(); btnActionTake.onClick.AddListener(ExecuteTakeFloorItem); }
        }

        public void BindAudio(AudioSource src, AudioClip eqClip, AudioClip useClip, AudioClip drpClip, AudioClip ltClip)
        {
            audioSource = src;
            equipSFX = eqClip;
            unequipSFX = eqClip;
            useSFX = useClip;
            dropSFX = drpClip;
            lootSFX = ltClip;
        }

        public void Initialize(CharacterData player, List<InventoryEntry> roomFloorItems)
        {
            EnsureListeners();
            _player = player;
            _floorItems = roomFloorItems;

            // Ensure the right inspector card container is active and styled
            if (dossierPanel != null && dossierPanel.transform.parent != null)
            {
                GameObject parentCard = dossierPanel.transform.parent.gameObject;
                parentCard.SetActive(true);

                var cardOl = parentCard.GetComponent<Outline>();
                if (cardOl != null)
                {
                    cardOl.effectColor = new Color(0.95f, 0.48f, 0.12f, 0.55f); // Matching Tactical Orange outline
                }
            }

            // If room has floor loot and player hasn't inspected a specific item yet,
            // default to showing the Room Items tab so floor stash is immediately visible!
            if (_floorItems != null && _floorItems.Count > 0 && string.IsNullOrEmpty(_selectedItemId))
            {
                SwitchRightTab(false);
            }
            else
            {
                SwitchRightTab(true);
            }

            RefreshAll();
        }

        public void RefreshAll()
        {
            RefreshLoadoutSlots();
            RefreshBackpackGrid();
            RefreshFloorGrid();
            RefreshDossier();
            RefreshCapacityAndWeight();
            UpdateFilterButtonVisuals();
            UpdateTabButtonVisuals(dossierPanel != null && dossierPanel.activeSelf);
        }

        #region Active Loadout Paper Doll
        private void RefreshLoadoutSlots()
        {
            if (_player == null) return;

            UpdateSlotVisual(slotHeadIcon, slotHeadLabel, slotHeadOutline, _player.equippedHead, "HEAD");
            UpdateSlotVisual(slotBodyIcon, slotBodyLabel, slotBodyOutline, _player.equippedBody, "BODY");
            UpdateSlotVisual(slotWpnIcon, slotWpnLabel, slotWpnOutline, _player.equippedWpn, "WPN");
            UpdateSlotVisual(slotLegsIcon, slotLegsLabel, slotLegsOutline, _player.equippedLegs, "LEGS");
            UpdateSlotVisual(slotFeetIcon, slotFeetLabel, slotFeetOutline, _player.equippedFeet, "FEET");
        }

        private void UpdateSlotVisual(Image iconImg, TextMeshProUGUI label, Outline outline, string itemId, string defaultSlotName)
        {
            bool isEquipped = !string.IsNullOrEmpty(itemId) && !itemId.Equals("none", StringComparison.OrdinalIgnoreCase);

            if (iconImg != null)
            {
                if (isEquipped)
                {
                    Sprite s = ItemDatabase.GetItemIcon(itemId);
                    iconImg.gameObject.SetActive(true);
                    iconImg.sprite = s;
                    iconImg.color = Color.white;
                }
                else
                {
                    iconImg.gameObject.SetActive(false);
                }
            }

            if (label != null)
            {
                label.text = isEquipped ? "" : defaultSlotName;
            }

            if (outline != null)
            {
                bool isSelected = _selectedSource == SelectedSource.Equipped && _selectedSlot == defaultSlotName;
                if (isSelected)
                {
                    outline.effectColor = new Color(0.95f, 0.75f, 0.20f, 1f); // Pulsing gold selection
                }
                else if (isEquipped)
                {
                    outline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.85f); // Neon Cyan equipped
                }
                else
                {
                    outline.effectColor = new Color(0.20f, 0.24f, 0.30f, 0.65f); // Muted slate empty
                }
            }
        }

        private void OnEquipSlotClicked(string slot)
        {
            if (_player == null) return;

            string itemId = _player.GetEquippedInSlot(slot);
            if (string.IsNullOrEmpty(itemId) || itemId.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                ShowToast($"Slot [{slot}] is empty. Select an item from your backpack to equip.");
                return;
            }

            _selectedSource = SelectedSource.Equipped;
            _selectedItemId = itemId;
            _selectedSlot = slot;
            _selectedBackpackIndex = -1;
            _selectedFloorIndex = -1;

            SwitchRightTab(true);
            RefreshAll();
            PlaySFX(lootSFX);
        }
        #endregion

        #region Backpack Grid & Filtering
        public void SetFilter(InventoryFilter filter)
        {
            _currentFilter = filter;
            RefreshBackpackGrid();
            UpdateFilterButtonVisuals();
            PlaySFX(lootSFX);
        }

        private bool MatchesFilter(string itemId)
        {
            if (_currentFilter == InventoryFilter.All) return true;

            var def = ItemDatabase.GetDefinition(itemId);
            if (def == null) return true;

            switch (_currentFilter)
            {
                case InventoryFilter.Weapons:
                    return def.category == ItemCategory.RangedWeapon ||
                           def.category == ItemCategory.MeleeWeapon ||
                           def.category == ItemCategory.Ammunition;
                case InventoryFilter.Consumables:
                    return def.category == ItemCategory.MedicalConsumable ||
                           def.category == ItemCategory.FoodRation ||
                           def.category == ItemCategory.SkillBook;
                case InventoryFilter.Apparel:
                    return def.category == ItemCategory.WearableHead ||
                           def.category == ItemCategory.WearableBody ||
                           def.category == ItemCategory.WearableLegs ||
                           def.category == ItemCategory.WearableFeet;
                case InventoryFilter.Tools:
                    return def.category == ItemCategory.ToolUtility ||
                           def.category == ItemCategory.ValuableKey;
                default:
                    return true;
            }
        }

        private void RefreshBackpackGrid()
        {
            if (backpackGridRoot == null) return;

            for (int i = backpackGridRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(backpackGridRoot.GetChild(i).gameObject);
            }

            List<InventoryEntry> inv = _player != null && _player.inventory != null
                ? _player.inventory
                : new List<InventoryEntry>();

            int maxSlots = _player != null ? Mathf.Max(6, _player.bagLimit) : 12;
            int slotsRendered = 0;

            for (int i = 0; i < inv.Count; i++)
            {
                var entry = inv[i];
                bool matches = MatchesFilter(entry.itemId);
                int capturedIndex = i;
                var def = ItemDatabase.GetDefinition(entry.itemId);
                int size = def != null ? Mathf.Max(1, def.slotSize) : 1;

                if (size == 2)
                {
                    // Primary Cell (Slot 1 of 2)
                    CreateBackpackCell(backpackGridRoot, entry.itemId, entry.quantity, capturedIndex, matches, isTwoSlot: true);
                    slotsRendered++;

                    // Connected Companion Cell (Slot 2 of 2)
                    CreateBackpackCompanionCell(backpackGridRoot, entry.itemId, capturedIndex, matches);
                    slotsRendered++;
                }
                else
                {
                    CreateBackpackCell(backpackGridRoot, entry.itemId, entry.quantity, capturedIndex, matches, isTwoSlot: false);
                    slotsRendered++;
                }
            }

            // Fill remaining empty slots
            while (slotsRendered < maxSlots)
            {
                CreateEmptyCell(backpackGridRoot);
                slotsRendered++;
            }
        }

        private void CreateBackpackCell(Transform parent, string itemId, int quantity, int index, bool matchesFilter, bool isTwoSlot = false)
        {
            GameObject cell = new GameObject("Slot_" + itemId);
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(74f, 74f);

            bool isSelected = _selectedSource == SelectedSource.Backpack && _selectedBackpackIndex == index;

            Image bg = cell.AddComponent<Image>();
            bg.color = isSelected 
                ? new Color(0.16f, 0.22f, 0.32f, 0.98f) 
                : new Color(0.09f, 0.12f, 0.16f, 0.95f);

            var def = ItemDatabase.GetDefinition(itemId);
            Color catColor = def != null ? ItemDatabase.GetCategoryBorderColor(def.category) : new Color(0.28f, 0.32f, 0.38f);

            Outline ol = cell.AddComponent<Outline>();
            if (isSelected)
            {
                ol.effectColor = new Color(0.95f, 0.75f, 0.20f, 1f); // Bright Gold selection highlight ONLY for selected item
                ol.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                // Clean, crisp tactical steel border
                ol.effectColor = matchesFilter 
                    ? new Color(0.24f, 0.29f, 0.36f, 0.85f) 
                    : new Color(0.12f, 0.14f, 0.18f, 0.40f);
                ol.effectDistance = new Vector2(1f, -1f);
            }

            // Contrast Well Halo (Disc behind icon so dark items like bows/knives pop)
            GameObject wellObj = new GameObject("WellHalo");
            wellObj.transform.SetParent(cell.transform, false);
            Image wellImg = wellObj.AddComponent<Image>();
            wellImg.color = new Color(0.18f, 0.24f, 0.32f, 0.30f);
            wellImg.raycastTarget = false;
            RectTransform wrt = wellObj.GetComponent<RectTransform>();
            wrt.anchorMin = Vector2.zero; wrt.anchorMax = Vector2.one;
            wrt.offsetMin = new Vector2(6f, 14f); wrt.offsetMax = new Vector2(-6f, -4f);

            // Category Pip: Sleek 3x12 indicator on top-left of slot
            if (def != null && matchesFilter)
            {
                GameObject pipObj = new GameObject("CatPip");
                pipObj.transform.SetParent(cell.transform, false);
                Image pipImg = pipObj.AddComponent<Image>();
                pipImg.color = catColor;
                pipImg.raycastTarget = false;
                RectTransform prt = pipObj.GetComponent<RectTransform>();
                prt.anchorMin = new Vector2(0f, 1f); prt.anchorMax = new Vector2(0f, 1f);
                prt.pivot = new Vector2(0f, 1f);
                prt.anchoredPosition = new Vector2(4f, -4f);
                prt.sizeDelta = new Vector2(3f, 12f);
            }

            // 2-Slot Tactical Badge (Top Right)
            if (isTwoSlot && matchesFilter)
            {
                GameObject slotBadge = new GameObject("SlotBadge");
                slotBadge.transform.SetParent(cell.transform, false);
                RectTransform sbr = slotBadge.AddComponent<RectTransform>();
                sbr.anchorMin = new Vector2(1f, 1f); sbr.anchorMax = new Vector2(1f, 1f);
                sbr.pivot = new Vector2(1f, 1f);
                sbr.anchoredPosition = new Vector2(-3f, -3f);
                sbr.sizeDelta = new Vector2(34f, 12f);
                Image sbBg = slotBadge.AddComponent<Image>();
                sbBg.color = new Color(0.85f, 0.55f, 0.10f, 0.28f);
                sbBg.raycastTarget = false;

                GameObject sbtObj = new GameObject("Text");
                sbtObj.transform.SetParent(slotBadge.transform, false);
                TextMeshProUGUI sbTmp = sbtObj.AddComponent<TextMeshProUGUI>();
                sbTmp.fontSize = 7.5f;
                sbTmp.fontStyle = FontStyles.Bold;
                sbTmp.alignment = TextAlignmentOptions.Center;
                sbTmp.color = new Color(0.95f, 0.75f, 0.25f);
                sbTmp.text = "2-SLOT";
                sbTmp.raycastTarget = false;
                RectTransform sbtr = sbtObj.GetComponent<RectTransform>();
                sbtr.anchorMin = Vector2.zero; sbtr.anchorMax = Vector2.one;
                sbtr.offsetMin = Vector2.zero; sbtr.offsetMax = Vector2.zero;
            }

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cell.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(8f, 16f); ir.offsetMax = new Vector2(-8f, -4f);

            Sprite s = ItemDatabase.GetItemIcon(itemId);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = matchesFilter ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.45f);
            }

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = matchesFilter ? (isSelected ? Color.white : new Color(0.85f, 0.88f, 0.92f)) : new Color(0.4f, 0.4f, 0.4f);
            nameTmp.text = itemId.Length > 11 ? itemId.Substring(0, 10) + "." : itemId;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f); nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 15f);
            nr.anchoredPosition = new Vector2(0f, 2f);

            // Quantity Badge
            if (quantity > 1)
            {
                GameObject qBadge = new GameObject("QtyBadge");
                qBadge.transform.SetParent(cell.transform, false);
                RectTransform qr = qBadge.AddComponent<RectTransform>();
                qr.anchorMin = new Vector2(1f, 0f); qr.anchorMax = new Vector2(1f, 0f);
                qr.pivot = new Vector2(1f, 0f);
                qr.anchoredPosition = new Vector2(-2f, 14f);
                qr.sizeDelta = new Vector2(28f, 14f);
                Image qBg = qBadge.AddComponent<Image>();
                qBg.color = new Color(0.04f, 0.06f, 0.04f, 0.95f);
                qBg.raycastTarget = false;

                GameObject qtObj = new GameObject("Text");
                qtObj.transform.SetParent(qBadge.transform, false);
                TextMeshProUGUI qTmp = qtObj.AddComponent<TextMeshProUGUI>();
                qTmp.fontSize = 9.5f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.35f, 0.95f, 0.45f);
                qTmp.text = $"x{quantity}";
                qTmp.raycastTarget = false;
                RectTransform qtr = qtObj.GetComponent<RectTransform>();
                qtr.anchorMin = Vector2.zero; qtr.anchorMax = Vector2.one;
                qtr.offsetMin = Vector2.zero; qtr.offsetMax = Vector2.zero;
            }

            Button btn = cell.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capId = itemId;
            int capIndex = index;
            btn.onClick.AddListener(() =>
            {
                OnBackpackItemClicked(capId, capIndex);
            });
        }

        private void CreateBackpackCompanionCell(Transform parent, string itemId, int index, bool matchesFilter)
        {
            GameObject cell = new GameObject("Slot_" + itemId + "_Ext");
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(74f, 74f);

            bool isSelected = _selectedSource == SelectedSource.Backpack && _selectedBackpackIndex == index;

            Image bg = cell.AddComponent<Image>();
            bg.color = isSelected 
                ? new Color(0.14f, 0.19f, 0.28f, 0.95f) 
                : new Color(0.07f, 0.09f, 0.12f, 0.90f);

            Outline ol = cell.AddComponent<Outline>();
            if (isSelected)
            {
                ol.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.85f);
                ol.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                ol.effectColor = matchesFilter 
                    ? new Color(0.20f, 0.24f, 0.30f, 0.65f) 
                    : new Color(0.10f, 0.12f, 0.15f, 0.35f);
                ol.effectDistance = new Vector2(1f, -1f);
            }

            // Tactical bracket & link indicator
            GameObject extObj = new GameObject("ExtText");
            extObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI extTmp = extObj.AddComponent<TextMeshProUGUI>();
            extTmp.fontSize = 8.5f;
            extTmp.alignment = TextAlignmentOptions.Center;
            extTmp.color = isSelected ? new Color(0.95f, 0.75f, 0.25f, 0.9f) : new Color(0.55f, 0.60f, 0.68f, 0.75f);
            extTmp.text = $"[ >> BULKY ]\n<size=7.5><color=#64748B>PART 2/2</color></size>";
            extTmp.raycastTarget = false;
            RectTransform extr = extObj.GetComponent<RectTransform>();
            extr.anchorMin = Vector2.zero; extr.anchorMax = Vector2.one;
            extr.offsetMin = new Vector2(4f, 4f); extr.offsetMax = new Vector2(-4f, -4f);

            Button btn = cell.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capId = itemId;
            int capIndex = index;
            btn.onClick.AddListener(() =>
            {
                OnBackpackItemClicked(capId, capIndex);
            });
        }

        private void CreateEmptyCell(Transform parent)
        {
            GameObject cell = new GameObject("EmptySlot");
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(74f, 74f);

            Image bg = cell.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.75f);

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = new Color(0.14f, 0.16f, 0.20f, 0.6f);
            ol.effectDistance = new Vector2(1f, -1f);
        }

        private void OnBackpackItemClicked(string itemId, int index)
        {
            _selectedSource = SelectedSource.Backpack;
            _selectedItemId = itemId;
            _selectedBackpackIndex = index;
            _selectedSlot = "";
            _selectedFloorIndex = -1;

            SwitchRightTab(true);
            RefreshAll();
            PlaySFX(lootSFX);
        }
        #endregion

        #region Room / Table Floor Loot
        private void RefreshFloorGrid()
        {
            if (floorGridRoot == null) return;

            for (int i = floorGridRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(floorGridRoot.GetChild(i).gameObject);
            }

            int count = _floorItems != null ? _floorItems.Count : 0;
            bool hasItems = count > 0;

            if (floorEmptyNotice != null) floorEmptyNotice.gameObject.SetActive(!hasItems);
            if (btnTakeAllFloor != null) btnTakeAllFloor.gameObject.SetActive(hasItems);

            if (tabFloorCountText != null)
            {
                tabFloorCountText.text = hasItems ? $"ROOM ITEMS <color=#F59E0B>({count})</color>" : "ROOM ITEMS";
            }

            if (!hasItems) return;

            for (int i = 0; i < _floorItems.Count; i++)
            {
                var entry = _floorItems[i];
                int capIndex = i;
                CreateFloorCell(floorGridRoot, entry.itemId, entry.quantity, capIndex);
            }
        }

        private void CreateFloorCell(Transform parent, string itemId, int quantity, int index)
        {
            GameObject cell = new GameObject("FloorSlot_" + itemId);
            cell.transform.SetParent(parent, false);
            RectTransform rt = cell.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(72f, 72f);

            Image bg = cell.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.08f, 0.08f, 0.95f);

            bool isSelected = _selectedSource == SelectedSource.Floor && _selectedFloorIndex == index;

            Outline ol = cell.AddComponent<Outline>();
            ol.effectColor = isSelected
                ? new Color(0.95f, 0.75f, 0.20f, 1f)
                : new Color(0.91f, 0.30f, 0.24f, 0.85f); // Crimson border
            ol.effectDistance = isSelected ? new Vector2(2f, -2f) : new Vector2(1f, -1f);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cell.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform ir = iconObj.GetComponent<RectTransform>();
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(6f, 14f); ir.offsetMax = new Vector2(-6f, -4f);

            Sprite s = ItemDatabase.GetItemIcon(itemId);
            if (s != null)
            {
                iconImg.sprite = s;
                iconImg.color = Color.white;
            }

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cell.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 8.5f;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.95f, 0.85f, 0.85f);
            nameTmp.text = itemId.Length > 10 ? itemId.Substring(0, 9) + "." : itemId;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.raycastTarget = false;
            RectTransform nr = nameObj.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0f, 0f); nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(0.5f, 0f);
            nr.sizeDelta = new Vector2(0f, 14f);
            nr.anchoredPosition = new Vector2(0f, 1f);

            // Quantity
            if (quantity > 1)
            {
                GameObject qBadge = new GameObject("QtyBadge");
                qBadge.transform.SetParent(cell.transform, false);
                RectTransform qr = qBadge.AddComponent<RectTransform>();
                qr.anchorMin = new Vector2(1f, 0f); qr.anchorMax = new Vector2(1f, 0f);
                qr.pivot = new Vector2(1f, 0f);
                qr.anchoredPosition = new Vector2(-2f, 13f);
                qr.sizeDelta = new Vector2(26f, 14f);
                Image qBg = qBadge.AddComponent<Image>();
                qBg.color = new Color(0.04f, 0.06f, 0.04f, 0.95f);
                qBg.raycastTarget = false;

                GameObject qtObj = new GameObject("Text");
                qtObj.transform.SetParent(qBadge.transform, false);
                TextMeshProUGUI qTmp = qtObj.AddComponent<TextMeshProUGUI>();
                qTmp.fontSize = 9f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.35f, 0.95f, 0.45f);
                qTmp.text = $"x{quantity}";
                qTmp.raycastTarget = false;
                RectTransform qtr = qtObj.GetComponent<RectTransform>();
                qtr.anchorMin = Vector2.zero; qtr.anchorMax = Vector2.one;
                qtr.offsetMin = Vector2.zero; qtr.offsetMax = Vector2.zero;
            }

            Button btn = cell.AddComponent<Button>();
            btn.targetGraphic = bg;
            string capId = itemId;
            int capIndex = index;
            btn.onClick.AddListener(() =>
            {
                OnFloorItemClicked(capId, capIndex);
            });
        }

        private void OnFloorItemClicked(string itemId, int index)
        {
            _selectedSource = SelectedSource.Floor;
            _selectedItemId = itemId;
            _selectedFloorIndex = index;
            _selectedSlot = "";
            _selectedBackpackIndex = -1;

            SwitchRightTab(true);
            RefreshAll();
            PlaySFX(lootSFX);
        }

        public void TakeAllFloorItems()
        {
            if (_player == null || _floorItems == null || _floorItems.Count == 0) return;

            int taken = 0;
            for (int i = _floorItems.Count - 1; i >= 0; i--)
            {
                var item = _floorItems[i];
                if (_player.AddItem(item.itemId, item.quantity))
                {
                    _floorItems.RemoveAt(i);
                    taken++;
                }
                else
                {
                    ShowToast("<color=#EF4444>[!] Backpack capacity reached! Make room to take remaining items.</color>");
                    break;
                }
            }

            if (taken > 0)
            {
                PlaySFX(lootSFX);
                ShowToast($"<color=#10B981>[+] Transferred all items to backpack.</color>");
            }

            OnInventoryUpdated?.Invoke();
            RefreshAll();
        }
        #endregion

        #region Tactical Item Dossier Inspector
        public void SwitchRightTab(bool showDossier)
        {
            if (dossierPanel != null) dossierPanel.SetActive(showDossier);
            if (floorPanel != null) floorPanel.SetActive(!showDossier);
            UpdateTabButtonVisuals(showDossier);
        }

        private void UpdateFilterButtonVisuals()
        {
            UpdateFilterBtnVisual(btnFilterAll, _currentFilter == InventoryFilter.All);
            UpdateFilterBtnVisual(btnFilterWeapons, _currentFilter == InventoryFilter.Weapons);
            UpdateFilterBtnVisual(btnFilterConsumables, _currentFilter == InventoryFilter.Consumables);
            UpdateFilterBtnVisual(btnFilterApparel, _currentFilter == InventoryFilter.Apparel);
            UpdateFilterBtnVisual(btnFilterTools, _currentFilter == InventoryFilter.Tools);
        }

        private void UpdateFilterBtnVisual(Button btn, bool isActive)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            Outline ol = btn.GetComponent<Outline>();
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (bg != null) bg.color = isActive ? new Color(0.18f, 0.24f, 0.34f, 1f) : new Color(0.08f, 0.10f, 0.14f, 0.95f);
            if (ol != null) ol.effectColor = isActive ? new Color(0.22f, 0.74f, 0.97f, 1f) : new Color(0.20f, 0.24f, 0.30f, 0.65f);
            if (tmp != null) tmp.color = isActive ? Color.white : new Color(0.65f, 0.70f, 0.75f);
        }

        private void UpdateTabButtonVisuals(bool showingDossier)
        {
            bool hasFloorItems = _floorItems != null && _floorItems.Count > 0;
            UpdateTabBtnVisual(tabBtnDossier, showingDossier, false);
            UpdateTabBtnVisual(tabBtnFloor, !showingDossier, hasFloorItems && showingDossier);
        }

        private void UpdateTabBtnVisual(Button btn, bool isActive, bool hasAlert)
        {
            if (btn == null) return;
            Image bg = btn.GetComponent<Image>();
            Outline ol = btn.GetComponent<Outline>();
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (bg != null) bg.color = isActive 
                ? new Color(0.18f, 0.24f, 0.34f, 1f) 
                : (hasAlert ? new Color(0.16f, 0.12f, 0.08f, 0.95f) : new Color(0.08f, 0.10f, 0.14f, 0.95f));
            if (ol != null) ol.effectColor = isActive 
                ? new Color(0.95f, 0.75f, 0.20f, 1f) // Bright selection gold
                : (hasAlert ? new Color(0.96f, 0.62f, 0.04f, 0.90f) : new Color(0.20f, 0.24f, 0.30f, 0.65f)); // Amber alert for floor loot
            if (tmp != null) tmp.color = isActive 
                ? Color.white 
                : (hasAlert ? new Color(0.96f, 0.75f, 0.35f) : new Color(0.65f, 0.70f, 0.75f));
        }

        private void RefreshDossier()
        {
            if (dossierPanel == null) return;

            bool hasSelection = _selectedSource != SelectedSource.None && !string.IsNullOrEmpty(_selectedItemId);

            if (dossierEmptyState != null)
            {
                dossierEmptyState.SetActive(!hasSelection);
                if (!hasSelection)
                {
                    var emptyTmp = dossierEmptyState.GetComponentInChildren<TextMeshProUGUI>();
                    if (emptyTmp != null)
                    {
                        emptyTmp.text = "<color=#38BDF8><b>[ TACTICAL ITEM INSPECTOR ]</b></color>\n\n<color=#94A3B8>Select any weapon, gear, ammo, or consumable in your Active Loadout, Backpack, or Room Stash to inspect tactical intel, ballistic specs, and survival properties.</color>";
                    }
                }
            }
            if (dossierDetailContent != null) dossierDetailContent.SetActive(hasSelection);

            if (!hasSelection) return;

            var def = ItemDatabase.GetDefinition(_selectedItemId);
            if (def == null) return;

            if (dossierItemName != null) dossierItemName.text = def.displayName.ToUpperInvariant();
            if (dossierCategoryBadge != null) dossierCategoryBadge.text = ItemDatabase.GetCategoryTag(def.category);

            if (dossierIcon != null)
            {
                Sprite s = ItemDatabase.GetItemIcon(_selectedItemId);
                dossierIcon.sprite = s;
                dossierIcon.color = s != null ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            }

            // Sub Info (Stack & Caliber or Slot & Tactical Size)
            if (dossierSubInfo != null)
            {
                var subParts = new List<string>();
                if (def.IsEquippable) subParts.Add($"SLOT: <color=#38BDF8>{def.EquipSlot}</color>");
                if (!string.IsNullOrEmpty(def.ammoType)) subParts.Add($"CALIBER: <color=#F59E0B>{def.ammoType}</color>");
                int size = def.slotSize;
                subParts.Add($"SIZE: <color={(size == 2 ? "#F59E0B" : "#10B981")}>{size} {(size == 2 ? "SLOTS (BULKY)" : "SLOT")}</color>");
                subParts.Add($"WEIGHT: <color=#94A3B8>{def.weight:F1} KG</color>");
                dossierSubInfo.text = string.Join("  |  ", subParts);
            }

            // Stats Sheet
            if (dossierStatsText != null)
            {
                dossierStatsText.text = FormatStatsSheet(def);
            }

            // Description / Intel
            if (dossierDescription != null)
            {
                dossierDescription.text = $"\"{def.description}\"";
            }

            // Configure Action Buttons
            ConfigureActionButtons(def);
        }

        private string FormatStatsSheet(ItemDefinition def)
        {
            var lines = new List<string>();

            if (def.category == ItemCategory.RangedWeapon || def.category == ItemCategory.MeleeWeapon)
            {
                lines.Add($"<b>TACTICAL ACCURACY:</b> <color=#38BDF8>{def.accuracy}%</color>");
                lines.Add($"<b>MAX TARGET ENGAGEMENT:</b> <color=#F59E0B>{def.targets} Hostile(s)</color>");
                if (!string.IsNullOrEmpty(def.ammoType))
                {
                    lines.Add($"<b>MAGAZINE SPEC:</b> <color=#FBBF24>{def.ammoType}</color>");
                }
            }
            else if (def.IsConsumable)
            {
                if (def.heal > 0) lines.Add($"<b>VITAL RESTORATION:</b> <color=#22C55E>+{def.heal} HP</color>");
                if (def.stamina != 0) lines.Add($"<b>STAMINA SURGE:</b> <color=#38BDF8>{(def.stamina > 0 ? "+" : "")}{def.stamina}</color>");
                if (def.hunger > 0) lines.Add($"<b>NUTRITION INDEX:</b> <color=#34D399>+{def.hunger}% Hunger</color>");
                if (def.thirst > 0) lines.Add($"<b>HYDRATION LEVEL:</b> <color=#60A5FA>+{def.thirst}% Thirst</color>");
                if (def.humanity > 0) lines.Add($"<b>PSYCHOLOGICAL MORALE:</b> <color=#A78BFA>+{def.humanity} Humanity</color>");
                if (!string.IsNullOrEmpty(def.skill)) lines.Add($"<b>FIELD STUDY:</b> <color=#F59E0B>+1 {def.skill.ToUpperInvariant()} Skill</color>");
                if (def.xp > 0) lines.Add($"<b>SURVIVAL EXPERIENCE:</b> <color=#F59E0B>+{def.xp} XP</color>");
            }
            else if (def.category == ItemCategory.Ammunition)
            {
                lines.Add($"<b>CALIBER:</b> <color=#FBBF24>{def.displayName}</color>");
                lines.Add($"<b>MAX STACK:</b> <color=#94A3B8>{def.maxStack} Rounds</color>");
            }
            else
            {
                lines.Add($"<b>CLASSIFICATION:</b> <color=#94A3B8>{def.category}</color>");
                lines.Add($"<b>BARTER VALUE:</b> <color=#F59E0B>High Demand Post-Collapse</color>");
            }

            return string.Join("\n", lines);
        }

        private void ConfigureActionButtons(ItemDefinition def)
        {
            // Reset all actions
            if (btnActionEquip != null) btnActionEquip.gameObject.SetActive(false);
            if (btnActionUnequip != null) btnActionUnequip.gameObject.SetActive(false);
            if (btnActionUse != null) btnActionUse.gameObject.SetActive(false);
            if (btnActionDrop != null) btnActionDrop.gameObject.SetActive(false);
            if (btnActionTake != null) btnActionTake.gameObject.SetActive(false);

            switch (_selectedSource)
            {
                case SelectedSource.Backpack:
                    if (def.IsEquippable && btnActionEquip != null)
                    {
                        btnActionEquip.gameObject.SetActive(true);
                    }
                    if (def.IsConsumable && btnActionUse != null)
                    {
                        btnActionUse.gameObject.SetActive(true);
                    }
                    if (btnActionDrop != null)
                    {
                        btnActionDrop.gameObject.SetActive(true);
                    }
                    break;

                case SelectedSource.Equipped:
                    if (btnActionUnequip != null)
                    {
                        btnActionUnequip.gameObject.SetActive(true);
                    }
                    if (btnActionDrop != null)
                    {
                        btnActionDrop.gameObject.SetActive(true);
                    }
                    break;

                case SelectedSource.Floor:
                    if (btnActionTake != null)
                    {
                        btnActionTake.gameObject.SetActive(true);
                    }
                    break;
            }
        }
        #endregion

        #region Action Implementations
        public void ExecuteEquip()
        {
            if (_player == null || string.IsNullOrEmpty(_selectedItemId)) return;

            var def = ItemDatabase.GetDefinition(_selectedItemId);
            if (def == null || !def.IsEquippable) return;

            string targetSlot = def.EquipSlot;
            if (string.IsNullOrEmpty(targetSlot)) return;

            bool success = _player.EquipItem(targetSlot, _selectedItemId);
            if (success)
            {
                PlaySFX(equipSFX);
                ShowToast($"<color=#10B981>[+] Equipped {def.displayName} to Active Loadout [{targetSlot}].</color>");

                _selectedSource = SelectedSource.Equipped;
                _selectedSlot = targetSlot;
                _selectedBackpackIndex = -1;

                OnInventoryUpdated?.Invoke();
                RefreshAll();
            }
            else
            {
                ShowToast("<color=#EF4444>[!] Could not equip item.</color>");
            }
        }

        public void ExecuteUnequip()
        {
            if (_player == null || string.IsNullOrEmpty(_selectedSlot)) return;

            string equippedItem = _player.GetEquippedInSlot(_selectedSlot);
            bool success = _player.UnequipItem(_selectedSlot);
            if (success)
            {
                PlaySFX(unequipSFX);
                ShowToast($"<color=#38BDF8>[-] Unequipped {equippedItem} to backpack.</color>");

                _selectedSource = SelectedSource.None;
                _selectedItemId = "";
                _selectedSlot = "";

                OnInventoryUpdated?.Invoke();
                RefreshAll();
            }
            else
            {
                ShowToast("<color=#EF4444>[!] Backpack is full! Cannot unequip.</color>");
            }
        }

        public void ExecuteUse()
        {
            if (_player == null || string.IsNullOrEmpty(_selectedItemId)) return;

            bool success = _player.UseItem(_selectedItemId, out string feedback);
            if (success)
            {
                PlaySFX(useSFX);
                ShowToast($"<color=#22C55E>[+] {feedback}</color>");

                // Check if remaining in inventory
                var remaining = _player.inventory.Find(i => i.itemId.Equals(_selectedItemId, StringComparison.OrdinalIgnoreCase));
                if (remaining == null)
                {
                    _selectedSource = SelectedSource.None;
                    _selectedItemId = "";
                }

                OnVitalsUpdated?.Invoke();
                OnInventoryUpdated?.Invoke();
                RefreshAll();
            }
            else
            {
                ShowToast($"<color=#EF4444>[!] {feedback}</color>");
            }
        }

        public void ExecuteDrop()
        {
            if (_player == null || string.IsNullOrEmpty(_selectedItemId)) return;

            string droppedItem = "";

            if (_selectedSource == SelectedSource.Backpack)
            {
                if (_player.RemoveItem(_selectedItemId, 1))
                {
                    droppedItem = _selectedItemId;
                }
            }
            else if (_selectedSource == SelectedSource.Equipped)
            {
                droppedItem = _player.DropEquipped(_selectedSlot);
            }

            if (!string.IsNullOrEmpty(droppedItem))
            {
                // Add to room floor items
                if (_floorItems != null)
                {
                    var existing = _floorItems.Find(f => f.itemId.Equals(droppedItem, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) existing.quantity += 1;
                    else _floorItems.Add(new InventoryEntry(droppedItem, 1));
                }

                PlaySFX(dropSFX);
                ShowToast($"<color=#F59E0B>[-] Dropped {droppedItem} to room table/floor.</color>");

                _selectedSource = SelectedSource.None;
                _selectedItemId = "";
                _selectedSlot = "";

                OnInventoryUpdated?.Invoke();
                RefreshAll();
            }
        }

        public void ExecuteTakeFloorItem()
        {
            if (_player == null || _floorItems == null || string.IsNullOrEmpty(_selectedItemId)) return;

            var found = _floorItems.Find(f => f.itemId.Equals(_selectedItemId, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                if (_player.AddItem(found.itemId, 1))
                {
                    found.quantity -= 1;
                    if (found.quantity <= 0)
                    {
                        _floorItems.Remove(found);
                        _selectedSource = SelectedSource.None;
                        _selectedItemId = "";
                        _selectedFloorIndex = -1;
                    }

                    PlaySFX(lootSFX);
                    ShowToast($"<color=#10B981>[+] Picked up {found.itemId}.</color>");

                    OnInventoryUpdated?.Invoke();
                    RefreshAll();
                }
                else
                {
                    ShowToast("<color=#EF4444>[!] Backpack is full! Cannot take item.</color>");
                }
            }
        }
        #endregion

        #region Capacity & Weight Readouts
        private void RefreshCapacityAndWeight()
        {
            int currentSlotsUsed = _player != null ? _player.GetTotalSlotsUsed() : 0;
            int maxCapacity = _player != null ? Mathf.Max(6, _player.bagLimit) : 12;

            if (backpackCountText != null)
            {
                backpackCountText.text = $"[ {currentSlotsUsed} / {maxCapacity} SLOTS ]";
            }

            if (backpackCapacityFill != null)
            {
                float ratio = Mathf.Clamp01((float)currentSlotsUsed / maxCapacity);
                backpackCapacityFill.fillAmount = ratio;

                if (ratio >= 1.0f)
                {
                    backpackCapacityFill.color = new Color(0.93f, 0.27f, 0.27f); // Full Crimson
                }
                else if (ratio >= 0.75f)
                {
                    backpackCapacityFill.color = new Color(0.96f, 0.62f, 0.04f); // Amber Warning
                }
                else
                {
                    backpackCapacityFill.color = new Color(0.22f, 0.74f, 0.97f); // Cyan
                }
            }

            if (weightText != null && _player != null)
            {
                weightText.text = $"ENCUMBRANCE: {_player.GetTotalWeight():F1} KG  |  TACTICAL SLOTS: {currentSlotsUsed}/{maxCapacity}";
            }
        }

        private void ShowToast(string message)
        {
            if (dossierToast != null)
            {
                dossierToast.text = message;
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        #endregion
    }
}

