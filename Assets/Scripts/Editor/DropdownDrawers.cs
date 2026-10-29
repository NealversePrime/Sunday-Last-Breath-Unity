using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SundayLastBreath.Data;

namespace SundayLastBreath.Editor
{
    /// <summary>
    /// Base PropertyDrawer that renders a string serialized property as a user-friendly dropdown popup in the Inspector,
    /// with seamless fallback to custom text input whenever needed.
    /// </summary>
    public abstract class BaseDropdownDrawer : PropertyDrawer
    {
        protected abstract List<(string value, string display)> GetOptions();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var rawOptions = GetOptions();
            List<string> values = new List<string> { "" };
            List<string> displays = new List<string> { "[ None ]" };

            string currentVal = property.stringValue ?? "";

            foreach (var opt in rawOptions)
            {
                values.Add(opt.value);
                displays.Add(opt.display);
            }

            // Find current value in options (case-insensitive)
            int selectedIndex = -1;
            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], currentVal, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = i;
                    break;
                }
            }

            bool isCustom = (selectedIndex == -1 && !string.IsNullOrEmpty(currentVal));

            values.Add("__CUSTOM__");
            displays.Add(isCustom ? $"[ Custom: \"{currentVal}\" ]" : "[ Custom / Manual Text... ]");

            if (isCustom)
            {
                selectedIndex = values.Count - 1;
            }
            else if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            Rect contentPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (isCustom)
            {
                float popupWidth = Mathf.Min(160f, contentPosition.width * 0.45f);
                Rect popupRect = new Rect(contentPosition.x, contentPosition.y, popupWidth, contentPosition.height);
                Rect textRect = new Rect(contentPosition.x + popupWidth + 4f, contentPosition.y, contentPosition.width - popupWidth - 4f, contentPosition.height);

                int newIndex = EditorGUI.Popup(popupRect, selectedIndex, displays.ToArray());
                if (newIndex != selectedIndex && newIndex < values.Count - 1)
                {
                    property.stringValue = values[newIndex];
                }

                string newText = EditorGUI.TextField(textRect, currentVal);
                if (newText != currentVal)
                {
                    property.stringValue = newText;
                }
            }
            else
            {
                int newIndex = EditorGUI.Popup(contentPosition, selectedIndex, displays.ToArray());
                if (newIndex != selectedIndex)
                {
                    if (newIndex == values.Count - 1)
                    {
                        // Switch to custom text entry
                        property.stringValue = string.IsNullOrEmpty(currentVal) ? "custom" : currentVal;
                    }
                    else
                    {
                        property.stringValue = values[newIndex];
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(StatDropdownAttribute))]
    public class StatDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            ("Might", "Might (STR - Strength, Melee, Forcing Doors)"),
            ("Observation", "Observation (PER - Perception, Searching, Clues)"),
            ("Reflex", "Reflex (AGI - Agility, Evasion, Speed)"),
            ("Tactics", "Tactics (INT - Intelligence, Strategy, Hacking)"),
            ("Auspice", "Auspice (LCK - Luck, Criticals, Fortune)"),
            ("Leadership", "Leadership (CHA - Charisma, Morale, Negotiation)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }

    [CustomPropertyDrawer(typeof(SkillDropdownAttribute))]
    public class SkillDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            ("tech", "tech (Electronics, Circuits, Keypads, Terminal Hacking)"),
            ("shooting", "shooting (Firearms, Handguns, Shotguns, Precision)"),
            ("melee", "melee (Blades, Blunt Weapons, Brawling, Axes)"),
            ("stealth", "stealth (Sneaking, Noise Discipline, Shadowing)"),
            ("medicine", "medicine (First Aid, Bandaging, Pharmacology)"),
            ("scavenging", "scavenging (Looting Efficiency, Spotting Gear)"),
            ("survival", "survival (Wilderness, Tracking, Endurance)"),
            ("resolve", "resolve (Willpower, Sanity, Dread Resistance)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }

    [CustomPropertyDrawer(typeof(TraitDropdownAttribute))]
    public class TraitDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            // Vocations / Backgrounds
            ("POLICE OFFICER", "★ POLICE OFFICER (Vocation - Dispatch & Firearms)"),
            ("DOCTOR", "★ DOCTOR (Vocation - Advanced Surgery & Medicine)"),
            ("ENGINEER", "★ ENGINEER (Vocation - Circuitry & Structural Bypass)"),
            ("STUDENT", "★ STUDENT (Vocation - Fast Learner & Adaptability)"),
            ("SOLDIER", "★ SOLDIER (Vocation - Tactical Training & Marksmanship)"),
            ("OFFICE WORKER", "★ OFFICE WORKER (Vocation - Urban Resilience)"),

            // Key Story Traits & Hobbies
            ("RADIO OPERATOR", "RADIO OPERATOR (Shortwave Scanner & Police Dispatch)"),
            ("GAMER", "GAMER (Tech Aptitude & Percentage Deduction)"),
            ("LOCKSMITH", "LOCKSMITH (Bypass Locks, Pins & Deadbolts)"),
            ("BURGLAR", "BURGLAR (Silent Entry, Lock Bypass & Stealth)"),
            ("BRAWN", "BRAWN (Heavy Lifting, Forcing Doors, Raw Muscle)"),
            ("HEAVY HITTER", "HEAVY HITTER (Massive Blunt Strike Damage)"),
            ("FIELD MEDIC", "FIELD MEDIC (Double Healing Item Efficiency)"),
            ("STERILE", "STERILE (Clean Wound Care & Sepsis Immunity)"),
            ("HERBALIST", "HERBALIST (Natural Remedies & Foraged Rations)"),
            ("RUNNER", "RUNNER (High Dash Speed & Sprint Stamina)"),
            ("MECHANIC", "MECHANIC (Automotive & Mechanical Systems)"),
            ("HUNTER", "HUNTER (Early Ambush Spotting & Skinning)"),
            ("BOXER", "BOXER (Heavy Unarmed Knockout Damage)"),
            ("HIKER", "HIKER (+2 Permanent Inventory Bag Slots)"),
            ("ARCHER", "ARCHER (Bow Proficiency & Arrow Recovery)"),
            ("SURVIVOR", "SURVIVOR (Clutch Resilience in Mortal Danger)"),
            ("NIMBLE", "NIMBLE (Evasive Reflexes & Slip-Through)"),
            ("EAGLE EYE", "EAGLE EYE (Keen Sight & Distant Target Clues)"),
            ("PACK RAT", "PACK RAT (Optimized Carry Slots & Stacking)"),

            // Drawbacks / Negative Traits
            ("LUDDITE", "LUDDITE (Drawback - Inability to use Electronics)"),
            ("PACIFIST", "PACIFIST (Drawback - Avoid Lethal Violence)"),
            ("CLUMSY", "CLUMSY (Drawback - Noise Spikes & Tripping)"),
            ("GAMBLER", "GAMBLER (Drawback - High Risk RNG Checks)"),
            ("SHELTERED", "SHELTERED (Drawback - Lower Survival Grit)"),
            ("HUNGRY", "HUNGRY (Drawback - Higher Ration Consumption)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }

    [CustomPropertyDrawer(typeof(ItemDropdownAttribute))]
    public class ItemDropdownDrawer : BaseDropdownDrawer
    {
        private static List<(string value, string display)> _cachedItemOptions;

        protected override List<(string value, string display)> GetOptions()
        {
            if (_cachedItemOptions != null && _cachedItemOptions.Count > 0)
                return _cachedItemOptions;

            _cachedItemOptions = new List<(string value, string display)>();

            try
            {
                var defs = ItemDatabase.GetAllDefinitions();
                foreach (var kvp in defs)
                {
                    var def = kvp.Value;
                    string catTag = def.category.ToString();
                    _cachedItemOptions.Add((def.id, $"{def.displayName} [{catTag}] (id: '{def.id}')"));
                }
            }
            catch (Exception)
            {
                // Fallback items if ItemDatabase is uninitialized
                string[] fallbackIds = new string[] {
                    "Apartment 407 Key", "Bandage", "Battery", "Bleach", "Can Opener", "Canned Beans", "Canned Tuna",
                    "Crowbar", "Duct Tape", "Flashlight", "Hydro Flask", "Lockpicks", "Matches", "Multi-Tool",
                    "Rain Poncho", "Rope", "Rubbing Alcohol", "Suture Kit", "Tier-1 Military Clearance Passkey",
                    "Trash Bags", "Walkie-Talkie", "Whiskey", "Work Gloves"
                };
                foreach (var id in fallbackIds)
                {
                    _cachedItemOptions.Add((id, id));
                }
            }

            _cachedItemOptions.Sort((a, b) => string.Compare(a.display, b.display, StringComparison.OrdinalIgnoreCase));
            return _cachedItemOptions;
        }
    }

    [CustomPropertyDrawer(typeof(StoryFlagDropdownAttribute))]
    public class StoryFlagDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            // Act 1 Safehouse & Starting Checks
            ("checked_phone_harmon", "checked_phone_harmon (Read Harmon's Passcode SMS)"),
            ("searched_bathroom_407", "searched_bathroom_407 (Medicine Vanity Scavenged)"),
            ("searched_closet_407", "searched_closet_407 (Front Closet Scavenged)"),
            ("listened_radio_407", "listened_radio_407 (Police Radio Intercepted)"),
            ("hacked_terminal_407", "hacked_terminal_407 (Government Terminal Infiltrated)"),

            // Unit 408 (Harmon's Lab)
            ("room408_opened", "room408_opened (Unit 408 Electronic Deadbolt Unlocked)"),
            ("room408_locked_out", "room408_locked_out (Unit 408 Keypad Circuit Tripped)"),
            ("room408_searched", "room408_searched (Harmon's Lab Scavenged)"),

            // Unit 405
            ("room405_opened", "room405_opened (Unit 405 Door Unlocked)"),
            ("room405_searched", "room405_searched (Unit 405 Scavenged)"),

            // Unit 402
            ("room402_opened", "room402_opened (Unit 402 Door Unlocked)"),
            ("room402_searched", "room402_searched (Unit 402 Scavenged)"),

            // Unit 401
            ("room401_opened", "room401_opened (Unit 401 Door Unlocked)"),
            ("room401_searched", "room401_searched (Unit 401 Scavenged)"),

            // Utility Room & Stairwell
            ("utility_door_opened", "utility_door_opened (Utility Room Door Unlocked)"),
            ("utility_searched", "utility_searched (Utility Breaker Box Scavenged)"),
            ("stairwell_4f_unlocked", "stairwell_4f_unlocked (Fire Stairwell Door Unlocked)"),

            // Story Milestones & Discovery
            ("knows_basement_safe", "knows_basement_safe (Learned Basement Route)"),
            ("power_generator_active", "power_generator_active (Emergency Power Restored)"),
            ("met_harmon", "met_harmon (Encountered Professor Harmon)"),
            ("met_vane", "met_vane (Encountered Officer Vane)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }

    [CustomPropertyDrawer(typeof(TriggerActionDropdownAttribute))]
    public class TriggerActionDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            ("OPEN_KEYPAD_MODAL", "OPEN_KEYPAD_MODAL (Interactive Electronic Keypad Minigame)"),
            ("OPEN_PHONE_MODAL", "OPEN_PHONE_MODAL (Personal Smartphone PDA / Feed Modal)"),
            ("OPEN_RADIO_MODAL", "OPEN_RADIO_MODAL (Police Radio Dispatch Scanner Modal)"),
            ("OPEN_HACKING_MODAL", "OPEN_HACKING_MODAL (Toughbook Military CRT Hacking Modal)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }

    [CustomPropertyDrawer(typeof(RoomIdDropdownAttribute))]
    public class RoomIdDropdownDrawer : BaseDropdownDrawer
    {
        private static readonly List<(string value, string display)> _options = new List<(string, string)>
        {
            ("unit_407", "unit_407 (Safehouse Apartment)"),
            ("hallway_1", "hallway_1 (4F North Corridor / Units 407 & 408)"),
            ("hallway_2", "hallway_2 (4F Mid Corridor / Units 404 & 405)"),
            ("hallway_3", "hallway_3 (4F South Corridor / Units 401 & 402)"),
            ("hallway_4", "hallway_4 (4F Utility Alcove)"),
            ("unit_408", "unit_408 (Professor Harmon's Residence & Lab)"),
            ("unit_405", "unit_405 (Abandoned Apartment)"),
            ("unit_402", "unit_402 (Apartment)"),
            ("unit_401", "unit_401 (Apartment)"),
            ("utility_room", "utility_room (Electrical & Fuse Box Room)"),
            ("stairwell_4f", "stairwell_4f (4th Floor Fire Exit Stairwell)")
        };

        protected override List<(string value, string display)> GetOptions() => _options;
    }
}

