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
        public int bonusPointsGranted = 2;
        [NonSerialized] public (string item, int qty)[] startingItems;
        public Action<CharacterData> onApply;

        public DrawbackData(
            string id, 
            string title, 
            DrawbackCategory category, 
            string desc, 
            string penalty, 
            int bonus = 2, 
            (string item, int qty)[] items = null,
            Action<CharacterData> apply = null)
        {
            this.id = id;
            this.title = title;
            this.category = category;
            this.description = desc;
            this.penaltySummary = penalty;
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
                2,
                new[] { ("Inhaler", 1) },
                c => { c.staminaMax -= 15; c.stamina = c.staminaMax; }
            ),
            new DrawbackData(
                "poor_vision",
                "POOR VISION",
                DrawbackCategory.Physical,
                "Severe uncorrected myopia. Without proper illumination, shapes blur into the dark.",
                "-1 Observation. Night exploration vision radius reduced by 30%.",
                2,
                new[] { ("Eyeglasses", 1) },
                c => { c.Observation = Mathf.Max(1, c.Observation - 1); }
            ),
            new DrawbackData(
                "hemophilia",
                "HEMOPHILIA",
                DrawbackCategory.Physical,
                "Impaired blood coagulation. Wounds remain open and dangerous far longer.",
                "Bleeding wounds deal 2x damage over time and require double bandaging.",
                2,
                new[] { ("Bandage", 3) },
                c => { /* Handled in medical/status system */ }
            ),
            new DrawbackData(
                "feeble",
                "FEEBLE",
                DrawbackCategory.Physical,
                "Underdeveloped muscular density. Forcing doors and swinging heavy tools is agonizing.",
                "-2 Might. Melee blunt damage reduced by 25%.",
                2,
                null,
                c => { c.Might = Mathf.Max(1, c.Might - 2); }
            ),
            new DrawbackData(
                "brittle_bones",
                "BRITTLE BONES",
                DrawbackCategory.Physical,
                "Low bone mineral density. Impacts and falls pose severe fracture risks.",
                "Blunt trauma and falling damage increased by 35%. Limbs fracture easily.",
                2,
                new[] { ("Painkillers", 1), ("Splint", 1) },
                c => { /* Handled in trauma system */ }
            ),
            new DrawbackData(
                "heavy_sleeper",
                "HEAVY SLEEPER",
                DrawbackCategory.Mental,
                "Once asleep, you do not easily rouse. Unmonitored camp rests become deadly gambles.",
                "Rest recovery takes 50% longer. Increased ambush probability during shelter rest.",
                2,
                new[] { ("Energy Drink", 2) },
                c => { /* Handled in shelter rest system */ }
            ),
            new DrawbackData(
                "nervous_tremor",
                "NERVOUS TREMOR",
                DrawbackCategory.Mental,
                "Adrenaline floods your system too intensely, causing involuntary muscle shakes under fire.",
                "QTE minigame indicator moves 25% faster in high-tension crisis encounters.",
                2,
                new[] { ("Beta Blockers", 1) },
                c => { /* Handled in QTE system */ }
            ),
            new DrawbackData(
                "paranoid_insomnia",
                "PARANOID INSOMNIA",
                DrawbackCategory.Mental,
                "You perceive betrayal in every whisper. You refuse to let your guard down near others.",
                "Cannot rest in the same room as companions. -1 Leadership in group dialogue.",
                2,
                new[] { ("Flashlight", 1) },
                c => { c.Leadership = Mathf.Max(1, c.Leadership - 1); }
            ),
            new DrawbackData(
                "nicotine_addict",
                "NICOTINE DEPENDENT",
                DrawbackCategory.Behavioral,
                "A persistent chemical addiction. In prolonged withdrawal, your focus unravels.",
                "-1 Tactics & -1 Reflex when craving. Relieved by smoking cigarettes.",
                2,
                new[] { ("Cigarettes", 1), ("Lighter", 1) },
                c => { /* Handled in status addiction system */ }
            ),
            new DrawbackData(
                "jinxed",
                "JINXED",
                DrawbackCategory.Behavioral,
                "Trouble seems to find you where others slip by unnoticed.",
                "-2 Auspice (Luck). Weapon jamming chance slightly elevated.",
                2,
                new[] { ("Lucky Coin", 1) },
                c => { c.Auspice = Mathf.Max(1, c.Auspice - 2); }
            )
        };
    }
}
