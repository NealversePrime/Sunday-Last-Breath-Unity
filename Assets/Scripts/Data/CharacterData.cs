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
        public Sprite icon;
        public bool isWeakness;

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

        public InventoryEntry(string id, int qty)
        {
            this.itemId = id;
            this.quantity = qty;
        }
    }

    /// <summary>
    /// Master data model for the survivor player character.
    /// Matches StoryInit.twee variables 1:1.
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

        [Header("Vitals (0 - 100)")]
        public int hp = 100;
        public int hpMax = 100;
        public int stamina = 100;
        public int staminaMax = 100;
        public int baseStaminaMax = 100;
        public int hunger = 100;
        public int thirst = 100;
        public int humanity = 50;

        [Header("Attributes (1 - 10)")]
        public int str = 3;
        public int intl = 3;
        public int agi = 3;
        public int cha = 3;
        public int per = 3;

        [Header("Skills (0 - 5)")]
        public int shooting = 0;
        public int melee = 0;
        public int stealth = 0;
        public int tech = 0;
        public int medicine = 0;
        public int scavenging = 0;
        public int survival = 0;
        public int resolve = 0;

        [Header("Perks & Inventory")]
        public int bagLimit = 12;
        public List<TraitEntry> traits = new List<TraitEntry>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();

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

        public void AddItem(string itemId, int qty = 1)
        {
            var found = inventory.Find(i => i.itemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                found.quantity += qty;
            }
            else
            {
                inventory.Add(new InventoryEntry(itemId, qty));
            }
        }
    }
}

