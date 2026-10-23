using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SundayLastBreath.Data
{
    public enum ItemCategory
    {
        RangedWeapon,
        MeleeWeapon,
        Ammunition,
        WearableHead,
        WearableBody,
        WearableLegs,
        WearableFeet,
        MedicalConsumable,
        FoodRation,
        SkillBook,
        ToolUtility,
        ValuableKey
    }

    [Serializable]
    public class ItemDefinition
    {
        public string id;
        public string displayName;
        public ItemCategory category;
        public string description;
        public bool stackable;
        public int maxStack = 1;

        // Combat / Weapon stats
        public int accuracy = 0;
        public int targets = 1;
        public string ammoType = "";

        // Consumable & Survival stats
        public int heal = 0;
        public int stamina = 0;
        public int hunger = 0;
        public int thirst = 0;
        public int humanity = 0;
        public string skill = "";
        public int xp = 0;

        // Weight / Encumbrance
        public float weight = 0.5f;

        public bool IsEquippable =>
            category == ItemCategory.RangedWeapon ||
            category == ItemCategory.MeleeWeapon ||
            category == ItemCategory.WearableHead ||
            category == ItemCategory.WearableBody ||
            category == ItemCategory.WearableLegs ||
            category == ItemCategory.WearableFeet;

        public bool IsConsumable =>
            category == ItemCategory.MedicalConsumable ||
            category == ItemCategory.FoodRation ||
            category == ItemCategory.SkillBook;

        public string EquipSlot
        {
            get
            {
                switch (category)
                {
                    case ItemCategory.RangedWeapon:
                    case ItemCategory.MeleeWeapon:
                        return "WPN";
                    case ItemCategory.WearableHead:
                        return "HEAD";
                    case ItemCategory.WearableBody:
                        return "BODY";
                    case ItemCategory.WearableLegs:
                        return "LEGS";
                    case ItemCategory.WearableFeet:
                        return "FEET";
                    default:
                        return null;
                }
            }
        }
    }

    [Serializable]
    public class ItemIconMapping
    {
        public string itemId;
        public Sprite icon;

        public ItemIconMapping(string id, Sprite icon)
        {
            this.itemId = id;
            this.icon = icon;
        }
    }

    /// <summary>
    /// Master Item & Equipment Database for Sunday: Last Breath.
    /// Provides canonical item metadata, stat scaling, category classifications,
    /// and sprite lookup mirroring the Twine/SugarCube engine specifications.
    /// </summary>
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> _traitIconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static bool _editorInitialized = false;

        private static readonly Dictionary<string, ItemDefinition> _itemDefinitions = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);

        static ItemDatabase()
        {
            InitializeDefinitions();
        }

        private static void InitializeDefinitions()
        {
            if (_itemDefinitions.Count > 0) return;

            // --- RANGED WEAPONS ---
            RegisterDef(new ItemDefinition { id = "Glock 19", displayName = "Glock 19", category = ItemCategory.RangedWeapon, accuracy = 60, targets = 1, ammoType = "9mm Ammo", weight = 1.2f, description = "Reliable 9mm police service pistol." });
            RegisterDef(new ItemDefinition { id = "Makarov", displayName = "Makarov", category = ItemCategory.RangedWeapon, accuracy = 55, targets = 1, ammoType = "9mm Ammo", weight = 1.1f, description = "Rugged Soviet steel. Compact and indestructible." });
            RegisterDef(new ItemDefinition { id = "Taurus", displayName = "Taurus", category = ItemCategory.RangedWeapon, accuracy = 50, targets = 1, ammoType = "9mm Ammo", weight = 1.0f, description = "Snub-nose revolver for close encounters." });
            RegisterDef(new ItemDefinition { id = "Revolver .38", displayName = "Revolver .38", category = ItemCategory.RangedWeapon, accuracy = 55, targets = 1, ammoType = ".38 Ammo", weight = 1.3f, description = "Civilian defense six-shooter." });
            RegisterDef(new ItemDefinition { id = "Five-Seven", displayName = "Five-Seven", category = ItemCategory.RangedWeapon, accuracy = 75, targets = 1, ammoType = "9mm Ammo", weight = 1.2f, description = "High-velocity tactical pistol with armor piercing capability." });
            RegisterDef(new ItemDefinition { id = "Magnum", displayName = "Magnum", category = ItemCategory.RangedWeapon, accuracy = 65, targets = 1, ammoType = ".500 Magnum", weight = 2.2f, description = "Hand cannon. High recoil, catastrophic kinetic punch." });
            RegisterDef(new ItemDefinition { id = "MP5", displayName = "MP5", category = ItemCategory.RangedWeapon, accuracy = 75, targets = 3, ammoType = "9mm Ammo", weight = 2.8f, description = "9mm submachine gun. Fast cyclic rate with controllable spread." });
            RegisterDef(new ItemDefinition { id = "M4A1", displayName = "M4A1", category = ItemCategory.RangedWeapon, accuracy = 80, targets = 2, ammoType = "5.56mm Ammo", weight = 3.4f, description = "Military selective-fire carbine. High lethality at range." });
            RegisterDef(new ItemDefinition { id = "Barret M82", displayName = "Barret M82", category = ItemCategory.RangedWeapon, accuracy = 95, targets = 1, ammoType = ".50 Cal", weight = 8.5f, description = "Anti-materiel heavy rifle. Devastating kinetic power." });
            RegisterDef(new ItemDefinition { id = "Spas-12", displayName = "Spas-12", category = ItemCategory.RangedWeapon, accuracy = 60, targets = 3, ammoType = "Shotgun Shells", weight = 4.2f, description = "Dual-mode combat shotgun. High close-quarters spread." });
            RegisterDef(new ItemDefinition { id = "Remington 870", displayName = "Remington 870", category = ItemCategory.RangedWeapon, accuracy = 70, targets = 2, ammoType = "Shotgun Shells", weight = 3.6f, description = "Pump-action shotgun with tight choke spread." });
            RegisterDef(new ItemDefinition { id = "Compound Bow", displayName = "Compound Bow", category = ItemCategory.RangedWeapon, accuracy = 85, targets = 1, ammoType = "Arrows", weight = 1.8f, description = "Silent hunter's weapon. Arrows can be retrieved." });

            // --- MELEE WEAPONS ---
            RegisterDef(new ItemDefinition { id = "Crowbar", displayName = "Crowbar", category = ItemCategory.MeleeWeapon, accuracy = 90, targets = 1, weight = 2.5f, description = "Forged steel prying tool. Breaches reinforced crates and skull bone." });
            RegisterDef(new ItemDefinition { id = "Combat Knife", displayName = "Combat Knife", category = ItemCategory.MeleeWeapon, accuracy = 80, targets = 1, weight = 0.6f, description = "Military carbon blade. Completely silent in close quarters." });
            RegisterDef(new ItemDefinition { id = "Hunting Knife", displayName = "Hunting Knife", category = ItemCategory.MeleeWeapon, accuracy = 85, targets = 1, weight = 0.7f, description = "Serrated field blade designed for deep gouging lacerations." });
            RegisterDef(new ItemDefinition { id = "Kitchen Knife", displayName = "Kitchen Knife", category = ItemCategory.MeleeWeapon, accuracy = 70, targets = 1, weight = 0.4f, description = "Chef's knife. Razor sharp stainless steel, but brittle under heavy impact." });
            RegisterDef(new ItemDefinition { id = "Sledgehammer", displayName = "Sledgehammer", category = ItemCategory.MeleeWeapon, accuracy = 50, targets = 2, weight = 6.0f, description = "Heavy industrial demolition hammer. Breaches barricaded doors. Slow swing." });
            RegisterDef(new ItemDefinition { id = "Scalpel", displayName = "Scalpel", category = ItemCategory.MeleeWeapon, accuracy = 90, targets = 1, weight = 0.2f, description = "Surgical steel precision blade. Pinpoint critical strikes." });
            RegisterDef(new ItemDefinition { id = "Golf Club", displayName = "Golf Club", category = ItemCategory.MeleeWeapon, accuracy = 60, targets = 1, weight = 1.2f, description = "Titanium driver. Medium reach with good kinetic transfer." });
            RegisterDef(new ItemDefinition { id = "Baseball Bat", displayName = "Baseball Bat", category = ItemCategory.MeleeWeapon, accuracy = 70, targets = 1, weight = 1.4f, description = "Hardwood slugger. Solid knockback against charging hostiles." });
            RegisterDef(new ItemDefinition { id = "Cane", displayName = "Cane", category = ItemCategory.MeleeWeapon, accuracy = 50, targets = 1, weight = 0.8f, description = "Sturdy oak walking stick. Improvised striking weapon." });
            RegisterDef(new ItemDefinition { id = "Tire Iron", displayName = "Tire Iron", category = ItemCategory.MeleeWeapon, accuracy = 60, targets = 1, weight = 2.0f, description = "Rusted roadside automotive crossbar club." });

            // --- AMMUNITION ---
            RegisterDef(new ItemDefinition { id = "9mm Ammo", displayName = "9mm Ammo", category = ItemCategory.Ammunition, stackable = true, maxStack = 60, weight = 0.02f, description = "Standard 9x19mm Parabellum hollow-point handgun rounds." });
            RegisterDef(new ItemDefinition { id = "5.56mm Ammo", displayName = "5.56mm Ammo", category = ItemCategory.Ammunition, stackable = true, maxStack = 60, weight = 0.025f, description = "High-velocity 5.56x45mm NATO rifle cartridges." });
            RegisterDef(new ItemDefinition { id = ".38 Ammo", displayName = ".38 Ammo", category = ItemCategory.Ammunition, stackable = true, maxStack = 20, weight = 0.03f, description = ".38 Special lead round nose revolver ammunition." });
            RegisterDef(new ItemDefinition { id = ".500 Magnum", displayName = ".500 Magnum", category = ItemCategory.Ammunition, stackable = true, maxStack = 12, weight = 0.06f, description = "Devastating heavy magnum ammunition for high caliber wheelguns." });
            RegisterDef(new ItemDefinition { id = ".50 Cal", displayName = ".50 Cal", category = ItemCategory.Ammunition, stackable = true, maxStack = 10, weight = 0.12f, description = "Heavy anti-materiel rounds designed for light vehicle penetration." });
            RegisterDef(new ItemDefinition { id = "Shotgun Shells", displayName = "Shotgun Shells", category = ItemCategory.Ammunition, stackable = true, maxStack = 20, weight = 0.05f, description = "12-gauge 00 buckshot shells containing nine hardened lead pellets." });
            RegisterDef(new ItemDefinition { id = "Arrows", displayName = "Arrows", category = ItemCategory.Ammunition, stackable = true, maxStack = 20, weight = 0.04f, description = "Carbon-composite broadhead arrows for archery hunting." });

            // --- APPAREL & WEARABLES ---
            RegisterDef(new ItemDefinition { id = "Tactical Vest", displayName = "Tactical Vest", category = ItemCategory.WearableBody, weight = 3.5f, description = "Ballistic Kevlar body armor. Reduces torso projectile & bite damage." });
            RegisterDef(new ItemDefinition { id = "Leather Jacket", displayName = "Leather Jacket", category = ItemCategory.WearableBody, weight = 2.0f, description = "Heavy cowhide jacket. Provides minor laceration and bite protection." });
            RegisterDef(new ItemDefinition { id = "T-Shirt", displayName = "T-Shirt", category = ItemCategory.WearableBody, weight = 0.3f, description = "Worn civilian cotton shirt. Provides zero defensive protection." });
            RegisterDef(new ItemDefinition { id = "Jeans", displayName = "Jeans", category = ItemCategory.WearableLegs, weight = 1.0f, description = "Durable denim pants with reinforced stitching." });
            RegisterDef(new ItemDefinition { id = "Hiking Boots", displayName = "Hiking Boots", category = ItemCategory.WearableFeet, weight = 1.5f, description = "Vibram-soled rugged boots. Increases long-distance stamina efficiency." });
            RegisterDef(new ItemDefinition { id = "Running Shoes", displayName = "Running Shoes", category = ItemCategory.WearableFeet, weight = 0.8f, description = "Lightweight athletic footwear. Dampens sprinting noise on hard tile." });
            RegisterDef(new ItemDefinition { id = "Sneakers", displayName = "Sneakers", category = ItemCategory.WearableFeet, weight = 0.9f, description = "Comfortable everyday rubber sneakers." });
            RegisterDef(new ItemDefinition { id = "Hard Hat", displayName = "Hard Hat", category = ItemCategory.WearableHead, weight = 0.9f, description = "Industrial high-density polyethylene helmet. Absorbs blunt trauma." });
            RegisterDef(new ItemDefinition { id = "Tinfoil Hat", displayName = "Tinfoil Hat", category = ItemCategory.WearableHead, weight = 0.1f, description = "A makeshift cone of folded aluminum. Supposedly deflects mind-control signals." });
            RegisterDef(new ItemDefinition { id = "Glasses", displayName = "Glasses", category = ItemCategory.WearableHead, weight = 0.1f, description = "Prescription eyeglasses. Sharpens visual observation." });

            // --- MEDICAL CONSUMABLES ---
            RegisterDef(new ItemDefinition { id = "First Aid Kit", displayName = "First Aid Kit", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 5, heal = 100, weight = 1.2f, description = "Comprehensive emergency surgical trauma pack. Restores Health to full." });
            RegisterDef(new ItemDefinition { id = "Bandage", displayName = "Bandage", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 10, heal = 15, weight = 0.2f, description = "Sterilized cotton gauze. Cleans open wounds and halts hemorrhaging (+15 HP)." });
            RegisterDef(new ItemDefinition { id = "Painkillers", displayName = "Painkillers", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 10, heal = 10, stamina = 15, weight = 0.1f, description = "Prescription analgesics. Blunts acute trauma shock (+10 HP, +15 Stamina)." });
            RegisterDef(new ItemDefinition { id = "Antibiotics", displayName = "Antibiotics", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 10, heal = 10, weight = 0.1f, description = "Broad-spectrum antibacterial pills. Fights systemic sepsis and infection." });
            RegisterDef(new ItemDefinition { id = "Inhaler", displayName = "Inhaler", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 5, stamina = 100, weight = 0.15f, description = "Albuterol sulfate aerosol bronchodilator. Restores depleted breath and stamina instantly." });
            RegisterDef(new ItemDefinition { id = "Sleeping Pills", displayName = "Sleeping Pills", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 10, weight = 0.1f, description = "Heavy sedative tablets. Induces immediate restorative sleep." });

            // --- FOOD & RATIONS ---
            RegisterDef(new ItemDefinition { id = "Bottled Water", displayName = "Bottled Water", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, heal = 5, stamina = 10, thirst = 30, weight = 0.5f, description = "500ml purified mineral water. Essential for hydration (+30 Thirst, +10 Stamina)." });
            RegisterDef(new ItemDefinition { id = "Canned Food", displayName = "Canned Food", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, heal = 20, stamina = 20, hunger = 40, weight = 0.6f, description = "Long-life preserved stew. Hearty caloric sustenance (+40 Hunger, +20 HP, +20 Stamina)." });
            RegisterDef(new ItemDefinition { id = "Beef Jerky", displayName = "Beef Jerky", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, heal = 10, stamina = 30, hunger = 30, weight = 0.25f, description = "Smoked cured beef strips. High protein field ration (+30 Hunger, +30 Stamina)." });
            RegisterDef(new ItemDefinition { id = "Stale Coffee", displayName = "Stale Coffee", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, stamina = 10, thirst = 10, weight = 0.3f, description = "Cold, bitter leftover roast. Mild caffeine kick (+10 Stamina, +10 Thirst)." });
            RegisterDef(new ItemDefinition { id = "Energy Bar", displayName = "Energy Bar", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, heal = 5, stamina = 50, hunger = 20, weight = 0.15f, description = "High-glucose endurance oat bar. Quick burst of energy (+50 Stamina, +20 Hunger)." });
            RegisterDef(new ItemDefinition { id = "Bag of Chips", displayName = "Bag of Chips", category = ItemCategory.FoodRation, stackable = true, maxStack = 10, heal = 5, stamina = -10, hunger = 15, weight = 0.2f, description = "Greasy salted potato chips. Quick calories, but leaves you sluggish." });
            RegisterDef(new ItemDefinition { id = "Whiskey", displayName = "Whiskey", category = ItemCategory.FoodRation, stackable = true, maxStack = 5, heal = 10, humanity = 10, thirst = 5, weight = 1.0f, description = "Distilled bourbon. Numbs existential dread and restores Morale (+10 Humanity)." });
            RegisterDef(new ItemDefinition { id = "Bottle of Vodka", displayName = "Bottle of Vodka", category = ItemCategory.FoodRation, stackable = true, maxStack = 5, humanity = 10, thirst = 5, weight = 1.0f, description = "Clear 80-proof alcohol. Disinfectant or courage in a bottle." });
            RegisterDef(new ItemDefinition { id = "Family Photo", displayName = "Family Photo", category = ItemCategory.ValuableKey, stackable = false, humanity = 15, weight = 0.05f, description = "A creased, faded photograph of loved ones. Grounding anchor to humanity (+15 Humanity)." });

            // --- SKILL BOOKS ---
            RegisterDef(new ItemDefinition { id = "Manual: Mechanics", displayName = "Manual: Mechanics", category = ItemCategory.SkillBook, skill = "tech", weight = 0.8f, description = "Technical schematics on wiring and circuitry. Grants +1 Tech Skill when studied." });
            RegisterDef(new ItemDefinition { id = "Manual: Anatomy", displayName = "Manual: Anatomy", category = ItemCategory.SkillBook, skill = "medicine", weight = 0.8f, description = "Surgical reference on human physiology. Grants +1 Medicine Skill when studied." });
            RegisterDef(new ItemDefinition { id = "Textbook", displayName = "Textbook", category = ItemCategory.SkillBook, xp = 50, weight = 1.0f, description = "Heavy university textbook. Grants +50 XP when studied." });

            // --- TOOLS & UTILITY ---
            RegisterDef(new ItemDefinition { id = "Flashlight", displayName = "Flashlight", category = ItemCategory.ToolUtility, weight = 0.6f, description = "Durable aluminum LED torch. Illuminates blacked-out hallways and reveals hidden salvage." });
            RegisterDef(new ItemDefinition { id = "Fuse", displayName = "Fuse", category = ItemCategory.ToolUtility, stackable = true, maxStack = 10, weight = 0.1f, description = "Industrial ceramic power fuse required to restore apartment wing breaker panels." });
            RegisterDef(new ItemDefinition { id = "Work Gloves", displayName = "Work Gloves", category = ItemCategory.ToolUtility, weight = 0.3f, description = "Reinforced leather work gloves. Protects fingers from jagged glass and hot metal." });
            RegisterDef(new ItemDefinition { id = "Bleach", displayName = "Bleach", category = ItemCategory.ToolUtility, weight = 1.5f, description = "Concentrated sodium hypochlorite. Neutralizes chemical odors and biological contaminants." });
            RegisterDef(new ItemDefinition { id = "Duct Tape", displayName = "Duct Tape", category = ItemCategory.ToolUtility, weight = 0.3f, description = "Heavy-duty waterproof adhesive tape. Essential for field repairs and makeshift gear." });
            RegisterDef(new ItemDefinition { id = "Lockpicks", displayName = "Lockpicks", category = ItemCategory.ToolUtility, weight = 0.1f, description = "Spring-steel tension wrench and pick rakes for quietly bypassing civilian tumbler deadbolts." });
            RegisterDef(new ItemDefinition { id = "Multi-Tool", displayName = "Multi-Tool", category = ItemCategory.ToolUtility, weight = 0.4f, description = "Folding pliers, wire cutters, serrated knife, and screwdrivers all in one rugged chassis." });
            RegisterDef(new ItemDefinition { id = "Binoculars", displayName = "Binoculars", category = ItemCategory.ToolUtility, weight = 0.9f, description = "Compact 10x42 tactical roof prism binoculars for long-distance scouting." });
            RegisterDef(new ItemDefinition { id = "Compass", displayName = "Compass", category = ItemCategory.ToolUtility, weight = 0.15f, description = "Fluid-damped magnetic compass for land navigation when GPS grids collapse." });
            RegisterDef(new ItemDefinition { id = "Lighter", displayName = "Lighter", category = ItemCategory.ToolUtility, weight = 0.1f, description = "Refillable butane flip lighter. Reliable flame source." });
            RegisterDef(new ItemDefinition { id = "Jerry Can", displayName = "Jerry Can", category = ItemCategory.ToolUtility, weight = 2.5f, description = "Steel 20-liter canister for gasoline fuel storage." });
            RegisterDef(new ItemDefinition { id = "Car Keys", displayName = "Car Keys", category = ItemCategory.ValuableKey, weight = 0.1f, description = "Key fob for an old sedan parked down in the Ascott basement lot." });
            RegisterDef(new ItemDefinition { id = "Bluetooth Speaker", displayName = "Bluetooth Speaker", category = ItemCategory.ToolUtility, weight = 0.5f, description = "Portable speaker. Can be paired and activated remotely to distract hostiles." });
            RegisterDef(new ItemDefinition { id = "Ceramic Mug", displayName = "Ceramic Mug", category = ItemCategory.ToolUtility, weight = 0.4f, description = "Heavy stoneware coffee cup. Can be thrown to lure stalkers toward the noise." });
            RegisterDef(new ItemDefinition { id = "Hand Sanitizer", displayName = "Hand Sanitizer", category = ItemCategory.ToolUtility, weight = 0.3f, description = "Ethyl alcohol gel antiseptic for personal hygiene and fire acceleration." });
            RegisterDef(new ItemDefinition { id = "Pepper Spray", displayName = "Pepper Spray", category = ItemCategory.ToolUtility, weight = 0.2f, description = "Pressurized OC spray. Stuns and temporarily blinds human aggressors." });
            RegisterDef(new ItemDefinition { id = "Designer Bag", displayName = "Designer Bag", category = ItemCategory.ValuableKey, weight = 1.0f, description = "Luxury leather handbag from the 4th floor penthouse. High barter trade value." });
            RegisterDef(new ItemDefinition { id = "Gold Ring", displayName = "Gold Ring", category = ItemCategory.ValuableKey, weight = 0.05f, description = "18-karat gold wedding band. Universal post-collapse currency." });
            RegisterDef(new ItemDefinition { id = "Luxury Watch", displayName = "Luxury Watch", category = ItemCategory.ValuableKey, weight = 0.2f, description = "Automatic Swiss timepiece. Prized barter trade asset." });
            RegisterDef(new ItemDefinition { id = "Encyclopedia", displayName = "Encyclopedia", category = ItemCategory.ToolUtility, weight = 2.2f, description = "Thick hardbound volume on historical mechanics and science." });
            RegisterDef(new ItemDefinition { id = "Cigarettes", displayName = "Cigarettes", category = ItemCategory.ToolUtility, stackable = true, maxStack = 20, humanity = 5, weight = 0.1f, description = "Pack of filter cigarettes. Steady nerves and universal trade commodity (+5 Humanity)." });
            RegisterDef(new ItemDefinition { id = "Deck of Cards", displayName = "Deck of Cards", category = ItemCategory.ToolUtility, weight = 0.15f, description = "Full 52-card poker deck. Solitaire passes the grueling hours." });
            RegisterDef(new ItemDefinition { id = "Old Map", displayName = "Old Map", category = ItemCategory.ToolUtility, weight = 0.1f, description = "A faded street layout of the Ascott district and local city highways." });
            RegisterDef(new ItemDefinition { id = "Guitar", displayName = "Guitar", category = ItemCategory.ToolUtility, weight = 2.5f, description = "Acoustic six-string wooden guitar. Playing chords restores survivor morale." });
            RegisterDef(new ItemDefinition { id = "Batteries", displayName = "Batteries", category = ItemCategory.ToolUtility, stackable = true, maxStack = 12, weight = 0.1f, description = "Standard 1.5V alkaline AA battery cells for radios and electronic tools." });
            RegisterDef(new ItemDefinition { id = "Rope", displayName = "Rope", category = ItemCategory.ToolUtility, weight = 1.5f, description = "Twelve meters of high-tensile braided nylon climbing cord." });
            RegisterDef(new ItemDefinition { id = "Tier-1 Military Clearance Passkey", displayName = "Tier-1 Clearance Passkey", category = ItemCategory.ValuableKey, weight = 0.1f, description = "Magnetic security card issued by the National Guard quarantine division." });

            // --- EXPANDED SURVIVAL & DOMESTIC ITEMS ---
            RegisterDef(new ItemDefinition { id = "Can Opener", displayName = "Can Opener", category = ItemCategory.ToolUtility, weight = 0.2f, description = "Manual stainless rotary can opener. Opens canned provisions cleanly without dulling combat knives." });
            RegisterDef(new ItemDefinition { id = "Hydro Flask", displayName = "Hydro Flask", category = ItemCategory.FoodRation, stackable = false, heal = 5, stamina = 15, thirst = 50, weight = 0.6f, description = "Insulated 32oz vacuum flask. Durable, boil-safe, and keeps water cold or hot for 24 hours." });
            RegisterDef(new ItemDefinition { id = "Suture Kit", displayName = "Suture Kit", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 5, heal = 40, weight = 0.25f, description = "Curved surgical needle and medical nylon thread. Stitches deep lacerations and prevents lethal arterial bleeding." });
            RegisterDef(new ItemDefinition { id = "Rain Poncho", displayName = "Rain Poncho", category = ItemCategory.WearableBody, weight = 0.3f, description = "Heavyweight hooded vinyl poncho. Shields you and your backpack from freezing Seattle rain." });
            RegisterDef(new ItemDefinition { id = "Matches", displayName = "Matches", category = ItemCategory.ToolUtility, stackable = true, maxStack = 50, weight = 0.05f, description = "Waterproof strike-anywhere sulfur matches. Reliable ignition for campfires and dark corridors." });
            RegisterDef(new ItemDefinition { id = "Trash Bags", displayName = "Trash Bags", category = ItemCategory.ToolUtility, stackable = true, maxStack = 10, weight = 0.2f, description = "Heavy-duty 55-gallon contractor trash bags. Waterproofing layer, makeshift poncho, or emergency corpse cover." });
            RegisterDef(new ItemDefinition { id = "Metal Canteen", displayName = "Metal Canteen", category = ItemCategory.FoodRation, stackable = false, heal = 5, stamina = 10, thirst = 40, weight = 0.5f, description = "Anodized aluminum military field canteen. Boil-safe over open flames to purify tainted water." });
            RegisterDef(new ItemDefinition { id = "Rubbing Alcohol", displayName = "Rubbing Alcohol", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 5, heal = 10, weight = 0.4f, description = "70% Isopropyl antiseptic solution. Sterilizes infected puncture wounds and doubles as an incendiary accelerant." });
            RegisterDef(new ItemDefinition { id = "Walkie-Talkie", displayName = "Walkie-Talkie", category = ItemCategory.ToolUtility, weight = 0.4f, description = "Handheld VHF transceiver. Intercepts local emergency broadcasts, distress chirps, and tactical chatter." });
            RegisterDef(new ItemDefinition { id = "Battery", displayName = "Battery", category = ItemCategory.ToolUtility, stackable = true, maxStack = 12, weight = 0.1f, description = "Standard 1.5V alkaline AA battery cell. Powers tactical radios, flashlights, and electronic tools." });
            RegisterDef(new ItemDefinition { id = "Bandage", displayName = "Bandage", category = ItemCategory.MedicalConsumable, stackable = true, maxStack = 10, heal = 25, weight = 0.1f, description = "Sterile elastic compression dressing. Halts active hemorrhaging and wraps fractured limbs." });
            RegisterDef(new ItemDefinition { id = "Canned Tuna", displayName = "Canned Tuna", category = ItemCategory.FoodRation, stackable = true, maxStack = 8, heal = 15, hunger = 35, weight = 0.3f, description = "Preserved tuna in oil. High protein, long shelf-life survival ration." });
            RegisterDef(new ItemDefinition { id = "Canned Beans", displayName = "Canned Beans", category = ItemCategory.FoodRation, stackable = true, maxStack = 8, heal = 15, hunger = 40, weight = 0.4f, description = "Hearty baked beans in tomato sauce. Substantial nourishment that requires a can opener." });
        }

        private static void RegisterDef(ItemDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) return;
            _itemDefinitions[def.id] = def;
        }

        public static ItemDefinition GetDefinition(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            InitializeDefinitions();

            if (_itemDefinitions.TryGetValue(itemId, out var def)) return def;

            // Search by normalized name
            string norm = itemId.Trim().ToLowerInvariant();
            foreach (var kvp in _itemDefinitions)
            {
                if (kvp.Key.Trim().ToLowerInvariant() == norm) return kvp.Value;
            }

            // Fallback definition for unknown items
            return new ItemDefinition
            {
                id = itemId,
                displayName = itemId,
                category = ItemCategory.ToolUtility,
                description = "Standard survivor gear.",
                weight = 0.5f
            };
        }

        public static string GetCategoryTag(ItemCategory cat)
        {
            switch (cat)
            {
                case ItemCategory.RangedWeapon:
                    return "<color=#38bdf8>[RANGED WEAPON]</color>";
                case ItemCategory.MeleeWeapon:
                    return "<color=#f59e0b>[MELEE WEAPON]</color>";
                case ItemCategory.Ammunition:
                    return "<color=#fbbf24>[AMMUNITION]</color>";
                case ItemCategory.WearableHead:
                    return "<color=#a78bfa>[HEAD APPAREL]</color>";
                case ItemCategory.WearableBody:
                    return "<color=#a78bfa>[BODY APPAREL]</color>";
                case ItemCategory.WearableLegs:
                    return "<color=#a78bfa>[LEGS APPAREL]</color>";
                case ItemCategory.WearableFeet:
                    return "<color=#a78bfa>[FEET APPAREL]</color>";
                case ItemCategory.MedicalConsumable:
                    return "<color=#22c55e>[MEDICAL AID]</color>";
                case ItemCategory.FoodRation:
                    return "<color=#34d399>[SURVIVAL RATION]</color>";
                case ItemCategory.SkillBook:
                    return "<color=#60a5fa>[FIELD STUDY MANUAL]</color>";
                case ItemCategory.ValuableKey:
                    return "<color=#eab308>[KEY & BARTER ITEM]</color>";
                case ItemCategory.ToolUtility:
                default:
                    return "<color=#94a3b8>[UTILITY GEAR]</color>";
            }
        }

        public static Color GetCategoryBorderColor(ItemCategory cat)
        {
            switch (cat)
            {
                case ItemCategory.RangedWeapon:
                    return new Color(0.22f, 0.74f, 0.97f, 0.95f); // Cyan
                case ItemCategory.MeleeWeapon:
                    return new Color(0.96f, 0.62f, 0.04f, 0.95f); // Amber
                case ItemCategory.Ammunition:
                    return new Color(0.98f, 0.75f, 0.14f, 0.95f); // Yellow
                case ItemCategory.MedicalConsumable:
                    return new Color(0.13f, 0.77f, 0.37f, 0.95f); // Green
                case ItemCategory.FoodRation:
                    return new Color(0.20f, 0.83f, 0.60f, 0.95f); // Emerald
                case ItemCategory.WearableHead:
                case ItemCategory.WearableBody:
                case ItemCategory.WearableLegs:
                case ItemCategory.WearableFeet:
                    return new Color(0.65f, 0.55f, 0.98f, 0.95f); // Purple
                case ItemCategory.SkillBook:
                    return new Color(0.38f, 0.65f, 0.98f, 0.95f); // Blue
                case ItemCategory.ValuableKey:
                    return new Color(0.92f, 0.70f, 0.03f, 0.95f); // Gold
                default:
                    return new Color(0.40f, 0.45f, 0.52f, 0.85f); // Slate
            }
        }

        private static readonly Dictionary<string, string> _itemAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Glock 19", "glock-19" },
            { "9mm Ammo", "9mm" },
            { "5.56mm Ammo", "5.56" },
            { ".38 Ammo", ".38" },
            { ".500 Magnum", ".500" },
            { ".50 Cal", ".50" },
            { "Shotgun Shells", "shotgunshell" },
            { "Compound Bow", "bow" },
            { "Arrows", "arrows" },
            { "Sledgehammer", "sledgehammer" },
            { "Duct Tape", "duct_tape" },
            { "Combat Knife", "knife" },
            { "Kitchen Knife", "kitchen_knife" },
            { "Hunting Knife", "hunting_knife" },
            { "Lockpicks", "lockpicks" },
            { "Flashlight", "flashlight" },
            { "Scalpel", "scalpel" },
            { "First Aid Kit", "p3k" },
            { "Antibiotics", "antibiotics" },
            { "Bandage", "bandage" },
            { "Painkillers", "painkiller" },
            { "Sleeping Pills", "sleeping_pills" },
            { "Inhaler", "inhaler" },
            { "Bottled Water", "waterbottle" },
            { "Canned Food", "canfood" },
            { "Beef Jerky", "jerky" },
            { "Stale Coffee", "coffee" },
            { "Energy Bar", "energy_bar" },
            { "Bag of Chips", "chips" },
            { "Whiskey", "whiskey" },
            { "Bottle of Vodka", "vodka" },
            { "Family Photo", "photo" },
            { "Golf Club", "golf_club" },
            { "Baseball Bat", "baseball_bat" },
            { "Cane", "cane" },
            { "Tire Iron", "tire_iron" },
            { "Crowbar", "crowbar" },
            { "Tactical Vest", "vest" },
            { "Leather Jacket", "jacket" },
            { "T-Shirt", "tshirt" },
            { "Jeans", "jeans" },
            { "Hiking Boots", "hiking_boots" },
            { "Running Shoes", "running_shoes" },
            { "Sneakers", "sneakers" },
            { "Hard Hat", "hardhat" },
            { "Tinfoil Hat", "tinfoil_hat" },
            { "Glasses", "glasses" },
            { "Work Gloves", "gloves" },
            { "Bleach", "bleach" },
            { "Multi-Tool", "multitool" },
            { "Binoculars", "binoculars" },
            { "Compass", "compass" },
            { "Lighter", "lighter" },
            { "Jerry Can", "jerrycan" },
            { "Car Keys", "car_keys" },
            { "Bluetooth Speaker", "speaker" },
            { "Ceramic Mug", "mug" },
            { "Hand Sanitizer", "sanitizer" },
            { "Pepper Spray", "pepper_spray" },
            { "Designer Bag", "handbag" },
            { "Gold Ring", "gold_ring" },
            { "Luxury Watch", "watch" },
            { "Encyclopedia", "encyclopedia" },
            { "Textbook", "encyclopedia" },
            { "Manual: Mechanics", "encyclopedia" },
            { "Manual: Anatomy", "encyclopedia" },
            { "Cigarettes", "cigarettes" },
            { "Deck of Cards", "cards" },
            { "Old Map", "map" },
            { "Batteries", "batteries" },
            { "Battery", "batteries" },
            { "Bandages", "bandage" },
            { "Rope", "rope" },
            { "Fuse", "fuse" },
            { "Tier-1 Military Clearance Passkey", "fuse" },
            { "Can Opener", "multitool" },
            { "Hydro Flask", "waterbottle" },
            { "Suture Kit", "scalpel" },
            { "Rain Poncho", "jacket" },
            { "Matches", "lighter" },
            { "Trash Bags", "handbag" },
            { "Metal Canteen", "waterbottle" },
            { "Rubbing Alcohol", "sanitizer" },
            { "Walkie-Talkie", "speaker" },
            { "Canned Tuna", "canfood" },
            { "Canned Beans", "canfood" }
        };

        private static readonly Dictionary<string, string> _traitAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CLEAN", "trait_germophobe" },
            { "SHOPPER", "trait_shopaholic" },
            { "BAD KNEE", "trait_badknee" },
            { "LONE WOLF", "trait_lonewolf" },
            { "RADIO OPERATOR", "trait_radio" },
            { "GUNSLINGER", "perk_gunslinger" },
            { "SHOTGUNNER", "perk_shotgunner" },
            { "BLUNT BRUISER", "perk_blunt_bruiser" },
            { "BLADESMITH", "perk_bladesmith" },
            { "IRON STOMACH", "perk_iron_stomach" },
            { "FIELD FORAGER", "perk_field_forager" },
            { "SECOND WIND", "perk_second_wind" },
            { "IRON WILL", "perk_iron_will" },
            { "CRISIS NEGOTIATOR", "perk_crisis_negotiator" },
            { "COMMANDING PRESENCE", "perk_commanding_presence" },
            { "FIELD MEDIC", "perk_iron_stomach" },
            { "FAST LEARNER", "perk_second_wind" },
            { "ADRENALINE", "perk_second_wind" }
        };

        private static void EnsureLoaded()
        {
            if (_editorInitialized && _iconCache.Count > 0) return;
            _editorInitialized = true;

#if UNITY_EDITOR
            LoadFolderEditor("Assets/Sprites/Items", _iconCache);
            LoadFolderEditor("Assets/Sprites/Traits", _traitIconCache);
            LoadFolderEditor("Assets/Sprites/Perks", _traitIconCache);
#endif
        }

#if UNITY_EDITOR
        private static void LoadFolderEditor(string folder, Dictionary<string, Sprite> target)
        {
            if (!Directory.Exists(folder)) return;
            string[] files = Directory.GetFiles(folder, "*.png");
            foreach (string f in files)
            {
                string relPath = f.Replace("\\", "/");
                UnityEditor.TextureImporter ti = UnityEditor.AssetImporter.GetAtPath(relPath) as UnityEditor.TextureImporter;
                if (ti != null && (ti.textureType != UnityEditor.TextureImporterType.Sprite || !ti.alphaIsTransparency))
                {
                    ti.textureType = UnityEditor.TextureImporterType.Sprite;
                    ti.alphaIsTransparency = true;
                    ti.SaveAndReimport();
                }
                Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(relPath);
                if (s != null)
                {
                    string key = Path.GetFileNameWithoutExtension(f);
                    target[key] = s;
                }
            }
        }
#endif

        public static void RegisterIcon(string itemId, Sprite icon)
        {
            if (string.IsNullOrEmpty(itemId) || icon == null) return;
            _iconCache[itemId] = icon;
        }

        public static void RegisterIcons(IEnumerable<ItemIconMapping> mappings)
        {
            if (mappings == null) return;
            foreach (var m in mappings)
            {
                if (m != null && !string.IsNullOrEmpty(m.itemId) && m.icon != null)
                {
                    _iconCache[m.itemId] = m.icon;
                }
            }
        }

        public static Sprite GetItemIcon(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            EnsureLoaded();

            if (_iconCache.TryGetValue(itemId, out var icon)) return icon;

            if (_itemAliases.TryGetValue(itemId.Trim(), out string aliasedKey) &&
                _iconCache.TryGetValue(aliasedKey, out var aliasedIcon))
            {
                return aliasedIcon;
            }

            string normalized = itemId.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            foreach (var kvp in _iconCache)
            {
                string keyNorm = kvp.Key.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
                if (keyNorm == normalized || keyNorm.Contains(normalized) || normalized.Contains(keyNorm))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        public static Sprite GetTraitIcon(string traitName)
        {
            if (string.IsNullOrEmpty(traitName)) return null;
            EnsureLoaded();

            if (_traitIconCache.TryGetValue(traitName, out var icon)) return icon;

            if (_traitAliases.TryGetValue(traitName.Trim(), out string aliasedKey) &&
                _traitIconCache.TryGetValue(aliasedKey, out var aliasedIcon))
            {
                return aliasedIcon;
            }

            string norm = traitName.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            if (_traitIconCache.TryGetValue("trait_" + norm, out var tIcon)) return tIcon;
            if (_traitIconCache.TryGetValue("perk_" + norm, out var pIcon)) return pIcon;

            foreach (var kvp in _traitIconCache)
            {
                string keyNorm = kvp.Key.Trim().ToLowerInvariant();
                if (keyNorm.Contains(norm) || norm.Contains(keyNorm.Replace("trait_", "").Replace("perk_", "")))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns "HEAD", "BODY", "WPN", "LEGS", "FEET", or null if the item is not an equippable slot item.
        /// </summary>
        public static string GetEquipSlot(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            var def = GetDefinition(itemId);
            if (def != null && !string.IsNullOrEmpty(def.EquipSlot))
            {
                return def.EquipSlot;
            }

            string n = itemId.Trim().ToLowerInvariant();
            if (n.Contains("hat") || n.Contains("glasses") || n.Contains("helmet")) return "HEAD";
            if (n.Contains("shirt") || n.Contains("jacket") || n.Contains("vest") || n.Contains("coat") || n.Contains("poncho")) return "BODY";
            if (n.Contains("glock") || n.Contains("knife") || n.Contains("sledgehammer") || n.Contains("scalpel") ||
                n.Contains("club") || n.Contains("bat") || n.Contains("bow") || n.Contains("cane") ||
                n.Contains("revolver") || n.Contains("rifle") || n.Contains("shotgun") || n.Contains("mp5") || n.Contains("makarov")) return "WPN";
            if (n.Contains("jeans") || n.Contains("pants") || n.Contains("trousers")) return "LEGS";
            if (n.Contains("shoes") || n.Contains("boots") || n.Contains("sneakers")) return "FEET";

            return null;
        }
    }
}
