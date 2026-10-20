using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    public enum MortalStatType
    {
        Might,       // M - str (Strength / Physical power)
        Observation, // O - per (Perception / Sensory acuity)
        Reflex,      // R - agi (Agility / Speed & dodging)
        Tactics,     // T - intl (Intellect / Technical & Medical)
        Auspice,     // A - lck (Luck / Fortune & Crits)
        Leadership   // L - cha (Charisma / Influence & Morale)
    }

    public enum DrawbackCategory
    {
        Physical,
        Mental,
        Behavioral
    }

    [Serializable]
    public class DrawbackData
    {
        public string id;
        public string title;
        public DrawbackCategory category;
        public string description;
        public string penaltySummary;
        public string iconName;
        public int bonusPointsGranted = 2;
        [NonSerialized] public (string item, int qty)[] startingItems;
        public Action<CharacterData> onApply;

        public DrawbackData(
            string id, 
            string title, 
            DrawbackCategory category, 
            string desc, 
            string penalty, 
            string icon = null,
            int bonus = 2, 
            (string item, int qty)[] items = null,
            Action<CharacterData> apply = null)
        {
            this.id = id;
            this.title = title;
            this.category = category;
            this.description = desc;
            this.penaltySummary = penalty;
            this.iconName = !string.IsNullOrEmpty(icon) ? icon : ("trait_" + id);
            this.bonusPointsGranted = bonus;
            this.startingItems = items;
            this.onApply = apply;
        }
    }

    /// <summary>
    /// Master definitions and database for the M.O.R.T.A.L. survival rating system
    /// and the optional Survival Bargain (Drawbacks -> Attribute Points).
    /// </summary>
    public static class MortalStats
    {
        public const string Acronym = "M.O.R.T.A.L.";
        public const string SystemSubtitle = "FEDERAL BIO-EMERGENCY SURVIVOR APPRAISAL INDEX";

        public static readonly (MortalStatType type, char letter, string name, string code, string summary)[] StatDescriptors =
        {
            (MortalStatType.Might,       'M', "MIGHT",       "STR", "Physical force, heavy melee impact, and gear carrying capacity."),
            (MortalStatType.Observation, 'O', "OBSERVATION", "PER", "Sensory acuity, spotting concealed threats, traps, and firearm precision."),
            (MortalStatType.Reflex,      'R', "REFLEX",      "AGI", "Reaction speed, evasion, combat turn initiative, and QTE needle stability."),
            (MortalStatType.Tactics,     'T', "TACTICS",     "INT", "Analytical problem solving, medicine application, and terminal hacking."),
            (MortalStatType.Auspice,     'A', "AUSPICE",     "LCK", "Fortune & critical hit rate, extra scavenging yields, and lucky saves."),
            (MortalStatType.Leadership,  'L', "LEADERSHIP",  "CHA", "Crisis persuasion, companion morale stabilization, and barter negotiation.")
        };

        public static readonly DrawbackData[] AllDrawbacks = new DrawbackData[]
        {
            // === PHYSICAL FLAWS (1-10) ===
            new DrawbackData(
                "smoker",
                "SMOKER",
                DrawbackCategory.Physical,
                "Lungs of tar and chronic chemical reliance. Under crisis, cravings severely disrupt focus.",
                "-20 Max Stamina. Suffers aim tremors and slower reactions when craving nicotine.",
                icon: "trait_smoker",
                bonus: 2,
                items: new[] { ("Cigarettes", 1), ("Lighter", 1) },
                apply: c => { c.staminaMax -= 20; c.stamina = c.staminaMax; }
            ),
            new DrawbackData(
                "alcoholic",
                "ALCOHOLIC",
                DrawbackCategory.Physical,
                "Dulled reflexes and tremors without ethanol. Shakes impair fine motor coordination.",
                "-1 Tactics (INT). Suffers shooting inaccuracy and weapon handling penalties during withdrawal.",
                icon: "trait_alcoholic",
                bonus: 2,
                items: new[] { ("Whiskey", 1) },
                apply: c => { c.Tactics = Mathf.Max(1, c.Tactics - 1); }
            ),
            new DrawbackData(
                "asthmatic",
                "ASTHMATIC",
                DrawbackCategory.Physical,
                "Chronic bronchial constriction under stress. High physical exertion rapidly drains your reserves.",
                "-20 Max Stamina. Sprinting drains stamina 50% faster without an Inhaler.",
                icon: "trait_asthmatic",
                bonus: 2,
                items: new[] { ("Inhaler", 1) },
                apply: c => { c.staminaMax -= 20; c.stamina = c.staminaMax; }
            ),
            new DrawbackData(
                "blind",
                "POOR VISION",
                DrawbackCategory.Physical,
                "Severe uncorrected myopia. Without proper illumination or corrective lenses, shapes blur.",
                "-2 Observation (PER). Night exploration and ranged accuracy severely penalized.",
                icon: "trait_blind",
                bonus: 2,
                items: new[] { ("Glasses", 1) },
                apply: c => { c.Observation = Mathf.Max(1, c.Observation - 2); }
            ),
            new DrawbackData(
                "badknee",
                "BAD KNEE",
                DrawbackCategory.Physical,
                "Chronic joint degradation from prior injury. Sudden sprints and heavy carrying induce sharp pain.",
                "-1 Reflex (AGI). Cannot perform rapid flee maneuvers during tactical combat.",
                icon: "trait_badknee",
                bonus: 2,
                items: new[] { ("Cane", 1) },
                apply: c => { c.Reflex = Mathf.Max(1, c.Reflex - 1); }
            ),
            new DrawbackData(
                "glutton",
                "GLUTTON",
                DrawbackCategory.Physical,
                "High metabolic appetite. Starvation debuffs set in twice as fast during survival marches.",
                "-1 Reflex (AGI). Consumes double food rations; suffers stat penalties when hungry.",
                icon: "trait_glutton",
                bonus: 2,
                items: new[] { ("Bag of Chips", 2) },
                apply: c => { c.Reflex = Mathf.Max(1, c.Reflex - 1); }
            ),
            new DrawbackData(
                "fragile",
                "FRAGILE",
                DrawbackCategory.Physical,
                "Impaired tissue and bone density. Wounds tear easily and trauma risks severe fractures.",
                "-20 Max HP. Bleeding wounds deal double damage over time and require multiple bandages.",
                icon: "trait_fragile",
                bonus: 2,
                items: new[] { ("Bandage", 3) },
                apply: c => { c.hpMax -= 20; c.hp = c.hpMax; }
            ),
            new DrawbackData(
                "insomniac",
                "INSOMNIAC",
                DrawbackCategory.Physical,
                "Chronic sleep deprivation. You remain hyper-vigilant, but your body never fully repairs.",
                "-10 Max HP. Shelter rest yields 50% less stamina recovery.",
                icon: "trait_insomniac",
                bonus: 2,
                items: new[] { ("Sleeping Pills", 1) },
                apply: c => { c.hpMax -= 10; c.hp = c.hpMax; }
            ),
            new DrawbackData(
                "clumsy",
                "CLUMSY",
                DrawbackCategory.Physical,
                "Heavy, uncoordinated footing. Sneaking through debris often triggers acoustic alerts.",
                "-1 Reflex (AGI). Stealth movement produces 2x louder acoustic noise.",
                icon: "trait_clumsy",
                bonus: 2,
                items: new[] { ("Hard Hat", 1) },
                apply: c => { c.Reflex = Mathf.Max(1, c.Reflex - 1); }
            ),
            new DrawbackData(
                "germophobe",
                "GERMOPHOBE",
                DrawbackCategory.Physical,
                "Obsessive aversion to bodily filth and viral bio-rot. Handling infected gore induces nausea.",
                "-15 Humanity. Scavenging infected corpses causes morale collapse and nausea debuffs.",
                icon: "trait_germophobe",
                bonus: 2,
                items: new[] { ("Hand Sanitizer", 1) },
                apply: c => { c.humanity = Mathf.Max(0, c.humanity - 15); }
            ),

            // === MENTAL & BEHAVIORAL FLAWS (11-20) ===
            new DrawbackData(
                "coward",
                "COWARD",
                DrawbackCategory.Mental,
                "Freezes under lethal pressure. Adrenaline floods your system, causing involuntary muscle shakes.",
                "-2 Might (STR). QTE crisis indicators move 25% faster during high-tension encounters.",
                icon: "trait_coward",
                bonus: 2,
                items: new[] { ("Pepper Spray", 1) },
                apply: c => { c.Might = Mathf.Max(1, c.Might - 2); }
            ),
            new DrawbackData(
                "pyro",
                "PYROMANIAC",
                DrawbackCategory.Behavioral,
                "Morbid obsession with open fire. Recklessly risks burning vital resources and shelter.",
                "-15 Humanity. Refuses to enter unlit corridors without flares or active incendiaries.",
                icon: "trait_pyro",
                bonus: 2,
                items: new[] { ("Lighter", 1), ("Jerry Can", 1) },
                apply: c => { c.humanity = Mathf.Max(0, c.humanity - 15); }
            ),
            new DrawbackData(
                "lonewolf",
                "LONE WOLF",
                DrawbackCategory.Behavioral,
                "Abrasive, standoffish loner. Disdains team coordination and group shelter.",
                "-2 Leadership (CHA). Camp rest morale drops when traveling in groups.",
                icon: "trait_lonewolf",
                bonus: 2,
                items: new[] { ("Combat Knife", 1) },
                apply: c => { c.Leadership = Mathf.Max(1, c.Leadership - 2); }
            ),
            new DrawbackData(
                "luddite",
                "LUDDITE",
                DrawbackCategory.Mental,
                "Deep distrust of electronic computers, sensors, and automated terminal locks.",
                "-2 Tactics (INT). Cannot operate electronic terminals, keypads, or GPS trackers.",
                icon: "trait_luddite",
                bonus: 2,
                items: new[] { ("Old Map", 1) },
                apply: c => { c.Tactics = Mathf.Max(1, c.Tactics - 2); }
            ),
            new DrawbackData(
                "pacifist",
                "PACIFIST",
                DrawbackCategory.Mental,
                "Moral revulsion toward lethal violence against humans. Sparing lives is an obsession.",
                "Lethal combat choices incur massive -25 Humanity loss and severe guilt penalties.",
                icon: "trait_pacifist",
                bonus: 2,
                items: null,
                apply: c => { /* Handled in morality system */ }
            ),
            new DrawbackData(
                "gambler",
                "GAMBLER",
                DrawbackCategory.Behavioral,
                "Impulsive risk-taker who wagers safety on blind chance and superstitious impulse.",
                "-2 Auspice (LCK). 10% chance of critical hit, but 10% chance of weapon jamming.",
                icon: "trait_gambler",
                bonus: 2,
                items: new[] { ("Deck of Cards", 1) },
                apply: c => { c.Auspice = Mathf.Max(1, c.Auspice - 2); }
            ),
            new DrawbackData(
                "sheltered",
                "SHELTERED",
                DrawbackCategory.Behavioral,
                "Naive and unaccustomed to urban peril. Easily misled by predatory survivors.",
                "-1 Observation (PER). Blind to booby traps and hostile deceit in dialogue.",
                icon: "trait_sheltered",
                bonus: 2,
                items: new[] { ("Luxury Watch", 1) },
                apply: c => { c.Observation = Mathf.Max(1, c.Observation - 1); }
            ),
            new DrawbackData(
                "shopaholic",
                "SHOPAHOLIC",
                DrawbackCategory.Behavioral,
                "Hoards useless consumer luxury items. Refuses to discard trinkets in survival emergencies.",
                "-2 Inventory Backpack Slots. Cannot drop prized luxury collectibles.",
                icon: "trait_shopaholic",
                bonus: 2,
                items: new[] { ("Designer Bag", 1) },
                apply: c => { c.bagLimit = Mathf.Max(4, c.bagLimit - 2); }
            ),
            new DrawbackData(
                "arrogant",
                "ARROGANT",
                DrawbackCategory.Behavioral,
                "Condescending know-it-all. Disregards companion advice and alienates potential allies.",
                "-2 Leadership (CHA). Companion trust increases 50% slower; dialogue irritates allies.",
                icon: "trait_arrogant",
                bonus: 2,
                items: new[] { ("Encyclopedia", 1) },
                apply: c => { c.Leadership = Mathf.Max(1, c.Leadership - 2); }
            ),
            new DrawbackData(
                "paranoid",
                "PARANOID",
                DrawbackCategory.Mental,
                "Pathological suspicion. Sees conspiracy and betrayal in every shadow.",
                "-1 Leadership (CHA). Refuses to sleep in the same room as companions.",
                icon: "trait_paranoid",
                bonus: 2,
                items: new[] { ("Flashlight", 1) },
                apply: c => { c.Leadership = Mathf.Max(1, c.Leadership - 1); }
            )
        };
    }
}
