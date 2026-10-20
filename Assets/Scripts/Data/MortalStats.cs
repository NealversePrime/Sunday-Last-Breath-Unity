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
            new DrawbackData(
                "asthmatic",
                "ASTHMATIC",
                DrawbackCategory.Physical,
                "Chronic bronchial constriction under stress. High exertion rapidly drains your reserves.",
                "-15 Max Stamina. Sprinting drains stamina 25% faster.",
                icon: "trait_asthmatic",
                bonus: 2,
                items: new[] { ("Inhaler", 1) },
                apply: c => { c.staminaMax -= 15; c.stamina = c.staminaMax; }
            ),
            new DrawbackData(
                "poor_vision",
                "POOR VISION",
                DrawbackCategory.Physical,
                "Severe uncorrected myopia. Without proper illumination, shapes blur into the dark.",
                "-1 Observation. Night exploration vision radius reduced by 30%.",
                icon: "trait_blind",
                bonus: 2,
                items: new[] { ("Eyeglasses", 1) },
                apply: c => { c.Observation = Mathf.Max(1, c.Observation - 1); }
            ),
            new DrawbackData(
                "hemophilia",
                "HEMOPHILIA",
                DrawbackCategory.Physical,
                "Impaired blood coagulation. Wounds remain open and dangerous far longer.",
                "Bleeding wounds deal 2x damage over time and require double bandaging.",
                icon: "trait_fragile",
                bonus: 2,
                items: new[] { ("Bandage", 3) },
                apply: c => { /* Handled in medical/status system */ }
            ),
            new DrawbackData(
                "feeble",
                "FEEBLE",
                DrawbackCategory.Physical,
                "Underdeveloped muscular density. Forcing doors and swinging heavy tools is agonizing.",
                "-2 Might. Melee blunt damage reduced by 25%.",
                icon: "trait_clumsy",
                bonus: 2,
                items: null,
                apply: c => { c.Might = Mathf.Max(1, c.Might - 2); }
            ),
            new DrawbackData(
                "brittle_bones",
                "BRITTLE BONES",
                DrawbackCategory.Physical,
                "Low bone mineral density. Impacts and falls pose severe fracture risks.",
                "Blunt trauma and falling damage increased by 35%. Limbs fracture easily.",
                icon: "trait_badknee",
                bonus: 2,
                items: new[] { ("Painkillers", 1), ("Splint", 1) },
                apply: c => { /* Handled in trauma system */ }
            ),
            new DrawbackData(
                "heavy_sleeper",
                "HEAVY SLEEPER",
                DrawbackCategory.Mental,
                "Once asleep, you do not easily rouse. Unmonitored camp rests become deadly gambles.",
                "Rest recovery takes 50% longer. Increased ambush probability during shelter rest.",
                icon: "trait_insomniac",
                bonus: 2,
                items: new[] { ("Energy Drink", 2) },
                apply: c => { /* Handled in shelter rest system */ }
            ),
            new DrawbackData(
                "nervous_tremor",
                "NERVOUS TREMOR",
                DrawbackCategory.Mental,
                "Adrenaline floods your system too intensely, causing involuntary muscle shakes under fire.",
                "QTE minigame indicator moves 25% faster in high-tension crisis encounters.",
                icon: "trait_coward",
                bonus: 2,
                items: new[] { ("Beta Blockers", 1) },
                apply: c => { /* Handled in QTE system */ }
            ),
            new DrawbackData(
                "paranoid_insomnia",
                "PARANOID INSOMNIA",
                DrawbackCategory.Mental,
                "You perceive betrayal in every whisper. You refuse to let your guard down near others.",
                "Cannot rest in the same room as companions. -1 Leadership in group dialogue.",
                icon: "trait_paranoid",
                bonus: 2,
                items: new[] { ("Flashlight", 1) },
                apply: c => { c.Leadership = Mathf.Max(1, c.Leadership - 1); }
            ),
            new DrawbackData(
                "nicotine_addict",
                "NICOTINE DEPENDENT",
                DrawbackCategory.Behavioral,
                "A persistent chemical addiction. In prolonged withdrawal, your focus unravels.",
                "-1 Tactics & -1 Reflex when craving. Relieved by smoking cigarettes.",
                icon: "trait_smoker",
                bonus: 2,
                items: new[] { ("Cigarettes", 1), ("Lighter", 1) },
                apply: c => { /* Handled in status addiction system */ }
            ),
            new DrawbackData(
                "jinxed",
                "JINXED",
                DrawbackCategory.Behavioral,
                "Trouble seems to find you where others slip by unnoticed.",
                "-2 Auspice (Luck). Weapon jamming chance slightly elevated.",
                icon: "trait_gambler",
                bonus: 2,
                items: new[] { ("Lucky Coin", 1) },
                apply: c => { c.Auspice = Mathf.Max(1, c.Auspice - 2); }
            )
        };
    }
}
