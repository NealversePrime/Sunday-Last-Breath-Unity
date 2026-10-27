using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    [Serializable]
    public class TraitEntry
    {
        public string name;
        public string description;
        [System.NonSerialized] public Sprite icon;
        public bool isWeakness;

        public TraitEntry() { }

        public TraitEntry(string name, string desc, Sprite icon = null, bool isWeakness = false)
        {
            this.name = name;
            this.description = desc;
            this.icon = icon;
            this.isWeakness = isWeakness;
        }
    }

    [Serializable]
    public class InventoryEntry
    {
        public string itemId;
        public int quantity;

        public InventoryEntry() { }

        public InventoryEntry(string id, int qty)
        {
            this.itemId = id;
            this.quantity = qty;
        }
    }

    /// <summary>
    /// Resident Evil style Key Item & Treasure/Valuable model.
    /// Key items (Keys, Passkeys, Badges, Keycards, Artifacts) do NOT consume backpack grid slots.
    /// Tracks active state, when and where the item was used, and if its purpose has been fulfilled.
    /// </summary>
    [Serializable]
    public class KeyItemEntry
    {
        public string itemId;
        public string displayName;
        public string description;
        public bool isUsed;
        public string usedLocation;
        public string acquiredTime;

        public KeyItemEntry() { }

        public KeyItemEntry(string id, string name = null, string desc = null, bool used = false, string location = null, string time = null)
        {
            this.itemId = id;
            this.displayName = string.IsNullOrEmpty(name) ? id : name;
            this.description = desc;
            this.isUsed = used;
            this.usedLocation = location;
            this.acquiredTime = time;
        }
    }

    /// <summary>
    /// Master data model for the survivor player character.
    /// Standardized to the M.O.R.T.A.L. attribute system:
    /// [M] Might (str)
    /// [O] Observation (per)
    /// [R] Reflex (agi)
    /// [T] Tactics (intl)
    /// [A] Auspice (lck)
    /// [L] Leadership (cha)
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        [Header("Identity")]
        public string firstName = "James";
        public string lastName = "Smith";
        public string gender = "Male";
        public string job = "Survivor";

        public string FullName => $"{firstName} {lastName}".Trim();

        [Header("Location State")]
        public string currentLocationId = "unit_407";
        public string currentLocationName = "UNIT 407 // THE ASCOTT RESIDENCY [4TH FLOOR]";

        [Header("Vitals (0 - 100)")]
        public int hp = 100;
        public int hpMax = 100;
        public int stamina = 100;
        public int staminaMax = 100;
        public int baseStaminaMax = 100;
        public int hunger = 100;
        public int thirst = 100;
        public int humanity = 50;

        [Header("Progression (XP & Level)")]
        public int level = 1;
        public int xp = 0;
        public int xpRequired = 100;

        [Header("M.O.R.T.A.L. Attributes (1 - 10)")]
        public int str = 3;   // Might
        public int per = 3;   // Observation
        public int agi = 3;   // Reflex
        public int intl = 3;  // Tactics
        public int lck = 3;   // Auspice (Luck)
        public int cha = 3;   // Leadership

        // M.O.R.T.A.L. Property Mappings
        public int Might       { get => str;  set => str = value; }
        public int Observation { get => per;  set => per = value; }
        public int Reflex      { get => agi;  set => agi = value; }
        public int Tactics     { get => intl; set => intl = value; }
        public int Auspice     { get => lck;  set => lck = value; }
        public int Leadership  { get => cha;  set => cha = value; }

        [Header("Skills (0 - 5)")]
        public int shooting = 0;
        public int melee = 0;
        public int stealth = 0;
        public int tech = 0;
        public int medicine = 0;
        public int scavenging = 0;
        public int survival = 0;
        public int resolve = 0;

        [Header("Active Loadout (Paper Doll Equipment)")]
        public string equippedHead = "";
        public string equippedBody = "";
        public string equippedWpn = "";
        public string equippedLegs = "";
        public string equippedFeet = "";

        [Header("Backpack Inventory & Encumbrance")]
        public int bagLimit = 12;
        public List<TraitEntry> traits = new List<TraitEntry>();
        public List<string> selectedDrawbacks = new List<string>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();

        [Header("Key Items & Important Valuables (RE4 Style — 0 Bag Slots)")]
        public List<KeyItemEntry> keyItems = new List<KeyItemEntry>();

        /// <summary>
        /// Calculates the total number of tactical backpack slots used (accounting for 1-slot and 2-slot bulky items).
        /// Key items and quest items take 0 slots.
        /// </summary>
        public int GetTotalSlotsUsed()
        {
            if (inventory == null) return 0;
            int total = 0;
            foreach (var item in inventory)
            {
                total += ItemDatabase.GetItemSlotSize(item.itemId);
            }
            return total;
        }

        /// <summary>
        /// Returns the remaining free tactical backpack slots.
        /// </summary>
        public int GetFreeSlots()
        {
            return Mathf.Max(0, bagLimit - GetTotalSlotsUsed());
        }

        public void AddTrait(string name, string desc, Sprite icon = null, bool isWeakness = false)
        {
            if (traits.Exists(t => t.name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return;

            traits.Add(new TraitEntry(name, desc, icon, isWeakness));
        }

        public bool HasTrait(string name)
        {
            return traits.Exists(t => t.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds a key item to the survivor's dedicated Key Items pocket (0 bag slots consumed).
        /// </summary>
        public void AddKeyItem(string id, string name = null, string desc = null, string time = null)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (keyItems == null) keyItems = new List<KeyItemEntry>();

            var existing = keyItems.Find(k => string.Equals(k.itemId, id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (!string.IsNullOrEmpty(name)) existing.displayName = name;
                if (!string.IsNullOrEmpty(desc)) existing.description = desc;
                return;
            }

            var def = ItemDatabase.GetDefinition(id);
            string disp = !string.IsNullOrEmpty(name) ? name : (def != null ? def.displayName : id);
            string d = !string.IsNullOrEmpty(desc) ? desc : (def != null ? def.description : "");
            keyItems.Add(new KeyItemEntry(id, disp, d, false, null, time));
        }

        public bool HasKeyItem(string id, bool includeUsed = false)
        {
            if (string.IsNullOrEmpty(id) || keyItems == null) return false;
            var entry = keyItems.Find(k => string.Equals(k.itemId, id, StringComparison.OrdinalIgnoreCase) ||
                                           string.Equals(k.displayName, id, StringComparison.OrdinalIgnoreCase));
            if (entry == null) return false;
            return includeUsed || !entry.isUsed;
        }

        public KeyItemEntry GetKeyItem(string id)
        {
            if (string.IsNullOrEmpty(id) || keyItems == null) return null;
            return keyItems.Find(k => string.Equals(k.itemId, id, StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(k.displayName, id, StringComparison.OrdinalIgnoreCase));
        }

        public void MarkKeyItemUsed(string id, string usedLocationDescription = "Ascott District")
        {
            if (string.IsNullOrEmpty(id)) return;
            if (keyItems == null) keyItems = new List<KeyItemEntry>();

            var entry = GetKeyItem(id);
            if (entry != null)
            {
                entry.isUsed = true;
                entry.usedLocation = usedLocationDescription;
            }
            else
            {
                // Register it as used
                var def = ItemDatabase.GetDefinition(id);
                string disp = def != null ? def.displayName : id;
                string d = def != null ? def.description : "";
                keyItems.Add(new KeyItemEntry(id, disp, d, true, usedLocationDescription, null));
            }

            // Also clean up from regular inventory if it happened to be there
            RemoveItem(id, 1);
        }

        public bool HasItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            if (inventory != null && inventory.Exists(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Also check Key Items pocket
            return HasKeyItem(itemId, includeUsed: false);
        }

        public string GetEquippedInSlot(string slot)
        {
            if (string.IsNullOrEmpty(slot)) return "";
            switch (slot.ToUpperInvariant())
            {
                case "HEAD": return equippedHead;
                case "BODY": return equippedBody;
                case "WPN":
                case "WEAPON": return equippedWpn;
                case "LEGS": return equippedLegs;
                case "FEET":
                case "BOOTS": return equippedFeet;
                default: return "";
            }
        }

        public void SetEquippedInSlot(string slot, string itemId)
        {
            if (string.IsNullOrEmpty(slot)) return;
            switch (slot.ToUpperInvariant())
            {
                case "HEAD": equippedHead = itemId ?? ""; break;
                case "BODY": equippedBody = itemId ?? ""; break;
                case "WPN":
                case "WEAPON": equippedWpn = itemId ?? ""; break;
                case "LEGS": equippedLegs = itemId ?? ""; break;
                case "FEET":
                case "BOOTS": equippedFeet = itemId ?? ""; break;
            }
        }

        /// <summary>
        /// Adds an item to the player's backpack. Stacks if stackable.
        /// Returns true if added, false if inventory is full.
        /// </summary>
        public bool AddItem(string itemId, int qty = 1)
        {
            if (string.IsNullOrEmpty(itemId) || qty <= 0) return false;

            // RE4-Style Key Items / Valuables: Automatically route to Key Items pocket (0 slots consumed)
            if (ItemDatabase.IsKeyItem(itemId))
            {
                var keyDef = ItemDatabase.GetDefinition(itemId);
                AddKeyItem(itemId, keyDef != null ? keyDef.displayName : itemId, keyDef != null ? keyDef.description : "");
                return true;
            }

            var def = ItemDatabase.GetDefinition(itemId);
            bool isStackable = def != null && def.stackable;

            if (isStackable)
            {
                var found = inventory.Find(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    found.quantity += qty;
                    return true;
                }
            }

            int requiredSlots = ItemDatabase.GetItemSlotSize(itemId);
            if (GetTotalSlotsUsed() + requiredSlots > bagLimit)
            {
                return false; // Backpack tactical capacity full
            }

            inventory.Add(new InventoryEntry(itemId, qty));
            return true;
        }

        /// <summary>
        /// Removes or decrements an item from the player's backpack.
        /// </summary>
        public bool RemoveItem(string itemId, int qty = 1)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            var found = inventory.Find(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                // If it's a key item, mark it used instead of failing
                if (HasKeyItem(itemId, includeUsed: false))
                {
                    MarkKeyItemUsed(itemId);
                    return true;
                }
                return false;
            }

            found.quantity -= qty;
            if (found.quantity <= 0)
            {
                inventory.Remove(found);
            }
            return true;
        }

        /// <summary>
        /// Equips an item from the backpack to the specified slot.
        /// Returns previously equipped item to inventory if slot was occupied.
        /// </summary>
        public bool EquipItem(string slot, string itemId)
        {
            if (string.IsNullOrEmpty(slot) || string.IsNullOrEmpty(itemId)) return false;

            var invItem = inventory.Find(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (invItem == null) return false;

            string currentlyEquipped = GetEquippedInSlot(slot);
            bool hasCurrentlyEquipped = !string.IsNullOrEmpty(currentlyEquipped) && !currentlyEquipped.Equals("none", StringComparison.OrdinalIgnoreCase);

            if (hasCurrentlyEquipped)
            {
                int unequippedCost = ItemDatabase.GetItemSlotSize(currentlyEquipped);
                int equippingCost = ItemDatabase.GetItemSlotSize(itemId);
                int netSlotDiff = unequippedCost - equippingCost;
                if (GetTotalSlotsUsed() + netSlotDiff > bagLimit)
                {
                    return false; // Not enough room to swap items in backpack
                }
            }

            // Decrement or remove from backpack
            RemoveItem(itemId, 1);

            // Put current gear back into backpack if any
            if (hasCurrentlyEquipped)
            {
                AddItem(currentlyEquipped, 1);
            }

            SetEquippedInSlot(slot, itemId);
            return true;
        }

        /// <summary>
        /// Unequips an item from the paper doll slot back into the backpack.
        /// Returns false if backpack has no free slot.
        /// </summary>
        public bool UnequipItem(string slot)
        {
            string currentlyEquipped = GetEquippedInSlot(slot);
            if (string.IsNullOrEmpty(currentlyEquipped) || currentlyEquipped.Equals("none", StringComparison.OrdinalIgnoreCase))
                return false;

            int requiredSlots = ItemDatabase.GetItemSlotSize(currentlyEquipped);
            if (GetTotalSlotsUsed() + requiredSlots > bagLimit)
            {
                return false; // Cannot unequip when backpack has insufficient free slots
            }

            AddItem(currentlyEquipped, 1);
            SetEquippedInSlot(slot, "");
            return true;
        }

        /// <summary>
        /// Drops an equipped item directly from the slot.
        /// </summary>
        public string DropEquipped(string slot)
        {
            string currentlyEquipped = GetEquippedInSlot(slot);
            if (string.IsNullOrEmpty(currentlyEquipped) || currentlyEquipped.Equals("none", StringComparison.OrdinalIgnoreCase))
                return "";

            SetEquippedInSlot(slot, "");
            return currentlyEquipped;
        }

        /// <summary>
        /// Consumes an item, applying health, stamina, hunger, thirst, humanity, and skill buffs.
        /// Implements trait modifiers (FIELD MEDIC, STERILE, HERBALIST, HUNGRY).
        /// </summary>
        public bool UseItem(string itemId, out string feedbackMsg)
        {
            feedbackMsg = "";
            var def = ItemDatabase.GetDefinition(itemId);
            if (def == null) return false;

            var invItem = inventory.Find(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (invItem == null)
            {
                feedbackMsg = "Item not found in backpack.";
                return false;
            }

            int healAmount = def.heal;
            int staminaAmount = def.stamina;
            int humAmount = def.humanity;
            int hungerAmount = def.hunger;
            int thirstAmount = def.thirst;

            // Trait Modifiers
            if (HasTrait("FIELD MEDIC") && healAmount > 0) healAmount *= 2;
            if (HasTrait("STERILE") && healAmount > 0) healAmount += 20;
            if (HasTrait("HERBALIST"))
            {
                bool isNourishment = itemId.Equals("Bottled Water", StringComparison.OrdinalIgnoreCase) ||
                                     itemId.IndexOf("Food", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     itemId.IndexOf("Jerky", StringComparison.OrdinalIgnoreCase) >= 0;
                if (isNourishment)
                {
                    healAmount += 10;
                    staminaAmount += 10;
                }
            }

            bool used = false;
            var parts = new List<string>();

            if (healAmount > 0)
            {
                hp = Mathf.Clamp(hp + healAmount, 0, hpMax);
                parts.Add($"+{healAmount} HP");
                used = true;
            }

            if (staminaAmount != 0)
            {
                stamina = Mathf.Clamp(stamina + staminaAmount, 0, staminaMax);
                parts.Add(staminaAmount > 0 ? $"+{staminaAmount} Stamina" : $"{staminaAmount} Stamina");
                used = true;
            }

            if (humAmount > 0)
            {
                humanity = Mathf.Clamp(humanity + humAmount, 0, 100);
                parts.Add($"+{humAmount} Humanity");
                used = true;
            }

            if (hungerAmount > 0)
            {
                hunger = Mathf.Clamp(hunger + hungerAmount, 0, 100);
                parts.Add($"+{hungerAmount}% Hunger");
                used = true;
            }

            if (thirstAmount > 0)
            {
                thirst = Mathf.Clamp(thirst + thirstAmount, 0, 100);
                parts.Add($"+{thirstAmount}% Thirst");
                used = true;
            }

            // Skill Manuals
            if (!string.IsNullOrEmpty(def.skill))
            {
                switch (def.skill.ToLowerInvariant())
                {
                    case "tech": tech = Mathf.Clamp(tech + 1, 0, 5); break;
                    case "medicine": medicine = Mathf.Clamp(medicine + 1, 0, 5); break;
                    case "shooting": shooting = Mathf.Clamp(shooting + 1, 0, 5); break;
                    case "melee": melee = Mathf.Clamp(melee + 1, 0, 5); break;
                    case "stealth": stealth = Mathf.Clamp(stealth + 1, 0, 5); break;
                    case "scavenging": scavenging = Mathf.Clamp(scavenging + 1, 0, 5); break;
                    case "survival": survival = Mathf.Clamp(survival + 1, 0, 5); break;
                    case "resolve": resolve = Mathf.Clamp(resolve + 1, 0, 5); break;
                }
                parts.Add($"+1 {def.skill.ToUpperInvariant()} Skill");
                used = true;
            }

            // XP
            if (def.xp > 0)
            {
                GainXP(def.xp);
                parts.Add($"+{def.xp} XP");
                used = true;
            }

            // Drawback: HUNGRY demands extra ration
            if (HasTrait("HUNGRY") && hungerAmount > 0 && invItem.quantity > 1)
            {
                invItem.quantity -= 1;
                parts.Add("[Hungry: ate extra]");
            }

            if (used)
            {
                RemoveItem(itemId, 1);
                feedbackMsg = $"Consumed {itemId}: {string.Join(", ", parts)}";
                return true;
            }

            feedbackMsg = "Item cannot be consumed right now.";
            return false;
        }

        public void GainXP(int amount)
        {
            if (amount <= 0) return;
            xp += amount;
            while (xp >= xpRequired)
            {
                xp -= xpRequired;
                level++;
                xpRequired = Mathf.RoundToInt(xpRequired * 1.5f);
            }
        }

        public float GetTotalWeight()
        {
            float total = 0f;
            foreach (var item in inventory)
            {
                var def = ItemDatabase.GetDefinition(item.itemId);
                float w = def != null ? def.weight : 0.5f;
                total += w * item.quantity;
            }

            string[] equipped = { equippedHead, equippedBody, equippedWpn, equippedLegs, equippedFeet };
            foreach (var eq in equipped)
            {
                if (!string.IsNullOrEmpty(eq))
                {
                    var def = ItemDatabase.GetDefinition(eq);
                    total += def != null ? def.weight : 1.0f;
                }
            }

            return total;
        }
    }
}
