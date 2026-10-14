using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    [Serializable]
    public class JobData
    {
        public string id;
        public string title;
        public string description;
        public string quote;
        public Color accentColor;
        public int str, intl, agi, cha, per;
        public int hpBonus, staminaBonus, humanityBonus;
        public int shooting, melee, stealth, tech, medicine, scavenging;
        [NonSerialized] public (string name, string desc)[] perks;
        [NonSerialized] public (string item, int qty)[] startingItems;
    }

    [Serializable]
    public class HobbyData
    {
        public string id;
        public string title;
        public string description;
        public string statsPreview;
        public bool isWeakness;
        public Action<CharacterData, int> applyEffect; // (charData, step 1 or 2)
    }

    public static class CharacterCreationDefinitions
    {
        public static readonly JobData[] Jobs = new JobData[]
        {
            new JobData
            {
                id = "police",
                title = "POLICE OFFICER",
                description = "You served on the force. Trained to handle firearms under extreme stress.",
                quote = "\"Protect and Serve\" is just a memory. Now, it's \"Aim and Fire.\"",
                accentColor = new Color(0.36f, 0.55f, 0.71f), // #5D8DB5
                str = 5, intl = 4, agi = 6, cha = 3, per = 7,
                shooting = 3, melee = 2,
                perks = new[]
                {
                    ("STEADY HANDS", "+15 Accuracy bonus."),
                    ("AUTHORITY", "Command NPCs in crisis.")
                },
                startingItems = new[]
                {
                    ("Glock 19", 1),
                    ("9mm Ammo", 24)
                }
            },
            new JobData
            {
                id = "worker",
                title = "CONSTRUCTION WORKER",
                description = "You built this city, and you know how to break it. You are physically powerful.",
                quote = "Subtlety is not your strong suit. Force is.",
                accentColor = new Color(0.62f, 0.11f, 0.11f), // #9E1B1B
                str = 8, intl = 3, agi = 4, cha = 4, per = 3,
                hpBonus = 40,
                melee = 3, tech = 2,
                perks = new[]
                {
                    ("BREACHER", "Smash locked doors with force."),
                    ("IRON GRIP", "Knockback enemies on miss.")
                },
                startingItems = new[]
                {
                    ("Sledgehammer", 1),
                    ("Duct Tape", 2)
                }
            },
            new JobData
            {
                id = "thief",
                title = "BURGLAR",
                description = "You move silently and leave before anyone knows you were there.",
                quote = "Why fight when you can just take what you want?",
                accentColor = new Color(0.18f, 0.80f, 0.44f), // #2ECC71
                str = 3, intl = 5, agi = 8, cha = 2, per = 6,
                staminaBonus = 40,
                stealth = 3, scavenging = 2,
                perks = new[]
                {
                    ("LOCKSMITH", "Open locks silently."),
                    ("GHOST", "Harder for enemies to hit you.")
                },
                startingItems = new[]
                {
                    ("Combat Knife", 1),
                    ("Lockpicks", 1),
                    ("Flashlight", 1)
                }
            },
            new JobData
            {
                id = "doctor",
                title = "DOCTOR",
                description = "You know exactly how the human body works—and how to take it apart.",
                quote = "Knowledge is the only resource that doesn't run out.",
                accentColor = new Color(0.20f, 0.60f, 0.86f), // #3498DB
                str = 2, intl = 8, agi = 4, cha = 5, per = 5,
                humanityBonus = 20,
                medicine = 3, tech = 2,
                perks = new[]
                {
                    ("FIELD MEDIC", "Healing items restore 2x HP."),
                    ("ANATOMIST", "High critical hit chance.")
                },
                startingItems = new[]
                {
                    ("Scalpel", 1),
                    ("First Aid Kit", 1),
                    ("Antibiotics", 1)
                }
            },
            new JobData
            {
                id = "manager",
                title = "MANAGER",
                description = "A natural leader. You can talk your way out of a gunfight and barter for life.",
                quote = "Humans are the real threat. You know how to handle them.",
                accentColor = new Color(0.61f, 0.35f, 0.71f), // #9B59B6
                str = 3, intl = 5, agi = 3, cha = 9, per = 4,
                humanityBonus = 30,
                scavenging = 2, medicine = 1,
                perks = new[]
                {
                    ("SILVER TONGUE", "Better trade prices with NPCs."),
                    ("INSPIRATION", "Companions gain +10 Accuracy.")
                },
                startingItems = new[]
                {
                    ("Golf Club", 1),
                    ("Luxury Watch", 1)
                }
            },
            new JobData
            {
                id = "student",
                title = "STUDENT",
                description = "A blank slate with infinite potential. You adapt faster than anyone else.",
                quote = "You define your own survival.",
                accentColor = Color.white,
                str = 2, intl = 2, agi = 2, cha = 2, per = 2,
                perks = new[]
                {
                    ("FAST LEARNER", "Gain XP 2x faster."),
                    ("ADRENALINE", "Stats boost when HP below 30%.")
                },
                startingItems = new[]
                {
                    ("Baseball Bat", 1),
                    ("Textbook", 1)
                }
            }
        };

        public static readonly HobbyData[] PositiveHobbies = new HobbyData[]
        {
            new HobbyData
            {
                id = "runner",
                title = "RUNNER",
                description = "You run marathons. Outlast any chase.",
                statsPreview = "+STAMINA / +AGI",
                applyEffect = (c, step) =>
                {
                    c.staminaMax += (step == 1 ? 20 : 10);
                    c.stamina = c.staminaMax;
                    if (step == 1) c.agi += 1;
                    c.AddTrait("RUNNER", "Can Flee twice.");
                    c.AddItem("Energy Bar", 2);
                    c.AddItem("Running Shoes", 1);
                }
            },
            new HobbyData
            {
                id = "mechanic",
                title = "MECHANIC",
                description = "Fixing cars and tech comes naturally.",
                statsPreview = "+TECH / +STR",
                applyEffect = (c, step) =>
                {
                    if (step == 1) { c.str += 1; c.intl += 1; c.tech += 1; }
                    else { c.intl += 1; }
                    c.AddTrait("MECHANIC", "Start cars without keys.");
                    c.AddItem("Multi-Tool", 1);
                }
            },
            new HobbyData
            {
                id = "hunter",
                title = "HUNTER",
                description = "Track game in the wild. Spot danger first.",
                statsPreview = "+PERC / +SHOOT",
                applyEffect = (c, step) =>
                {
                    c.per += (step == 1 ? 2 : 1);
                    if (step == 1) c.shooting += 1;
                    c.AddTrait("HUNTER", "Spot ambushes early.");
                    c.AddItem("Binoculars", 1);
                    c.AddItem("Combat Knife", 1);
                }
            },
            new HobbyData
            {
                id = "boxer",
                title = "BOXER",
                description = "Know how to take a hit and throw a punch.",
                statsPreview = "+STR / +HP",
                applyEffect = (c, step) =>
                {
                    c.str += (step == 1 ? 2 : 1);
                    if (step == 1) { c.hpMax += 10; c.hp = c.hpMax; c.melee += 1; }
                    c.AddTrait("BOXER", "Unarmed attacks deal heavy damage.");
                }
            },
            new HobbyData
            {
                id = "gamer",
                title = "GAMER",
                description = "Simulations taught strategy and mental acuity.",
                statsPreview = "+INT / +TECH",
                applyEffect = (c, step) =>
                {
                    c.intl += (step == 1 ? 2 : 1);
                    if (step == 1) c.tech += 1;
                    c.AddTrait("GAMER", "See % Chance of success.");
                    c.AddItem("Fuse", 2);
                }
            },
            new HobbyData
            {
                id = "hiker",
                title = "HIKER",
                description = "Travel long distances carrying heavy loads.",
                statsPreview = "+BAG SPACE / +STAMINA",
                applyEffect = (c, step) =>
                {
                    c.staminaMax += 10;
                    c.stamina = c.staminaMax;
                    if (step == 1) { c.str += 1; c.scavenging += 1; }
                    c.bagLimit += 2;
                    c.AddTrait("HIKER", "+2 Inventory Slots.");
                    c.AddItem("Hiking Boots", 1);
                }
            },
            new HobbyData
            {
                id = "archer",
                title = "ARCHER",
                description = "Silent hunter. Retrieve ammo after a kill.",
                statsPreview = "+PERC / +BOW",
                applyEffect = (c, step) =>
                {
                    c.per += 1;
                    if (step == 1) { c.str += 1; c.shooting += 1; }
                    c.AddTrait("ARCHER", "Retrieve arrows after kill.");
                    c.AddItem("Compound Bow", 1);
                    c.AddItem("Arrows", 5);
                }
            },
            new HobbyData
            {
                id = "gardener",
                title = "GARDENER",
                description = "You know plants and natural remedies.",
                statsPreview = "+INT / +HEAL",
                applyEffect = (c, step) =>
                {
                    c.intl += 1;
                    if (step == 1) { c.per += 1; c.hpMax += 5; c.hp = c.hpMax; }
                    c.AddTrait("GARDENER", "Water and Food heal extra.");
                }
            },
            new HobbyData
            {
                id = "baseball",
                title = "BASEBALL",
                description = "Swing for the fences. High hand-eye coordination.",
                statsPreview = "+STR / +MELEE",
                applyEffect = (c, step) =>
                {
                    c.str += (step == 1 ? 2 : 1);
                    if (step == 1) c.melee += 1;
                    c.AddTrait("BASEBALL", "100% Knockdown on Crit.");
                    c.AddItem("Baseball Bat", 1);
                }
            },
            new HobbyData
            {
                id = "bookworm",
                title = "BOOKWORM",
                description = "Read about everything. Understand theory.",
                statsPreview = "+INT / -STAMINA",
                applyEffect = (c, step) =>
                {
                    c.intl += (step == 1 ? 2 : 1);
                    if (step == 1) { c.staminaMax -= 5; c.stamina = c.staminaMax; }
                    c.AddTrait("BOOKWORM", "Can attempt untrained Skill Checks.");
                    c.AddItem("Encyclopedia", 1);
                }
            },
            new HobbyData
            {
                id = "watchman",
                title = "WATCHMAN",
                description = "Used to night shifts. See clearly in the dark.",
                statsPreview = "+PERC / +STEALTH",
                applyEffect = (c, step) =>
                {
                    c.per += (step == 1 ? 2 : 1);
                    if (step == 1) c.stealth += 1;
                    c.AddTrait("WATCHMAN", "No accuracy penalty at night.");
                    c.AddItem("Flashlight", 1);
                }
            },
            new HobbyData
            {
                id = "debater",
                title = "DEBATER",
                description = "Able to argue your way out of anything.",
                statsPreview = "+CHA / +SPEECH",
                applyEffect = (c, step) =>
                {
                    c.cha += (step == 1 ? 2 : 1);
                    c.AddTrait("DEBATER", "Detect deceit.");
                }
            },
            new HobbyData
            {
                id = "chef",
                title = "CHEF",
                description = "Make the most of scarce food.",
                statsPreview = "+SCAVENGE / +PERC",
                applyEffect = (c, step) =>
                {
                    c.per += 1;
                    if (step == 1) c.scavenging += 1;
                    c.AddTrait("CHEF", "Create meals from raw items.");
                    c.AddItem("Combat Knife", 1);
                }
            },
            new HobbyData
            {
                id = "parkour",
                title = "PARKOUR",
                description = "Urban exploration. Climb vertical obstacles.",
                statsPreview = "+AGI / +STEALTH",
                applyEffect = (c, step) =>
                {
                    c.agi += (step == 1 ? 2 : 1);
                    if (step == 1) c.stealth += 1;
                    c.AddTrait("PARKOUR", "Access vertical paths.");
                }
            },
            new HobbyData
            {
                id = "volunteer",
                title = "VOLUNTEER",
                description = "You help others. High humanity.",
                statsPreview = "+HUMANITY / +MEDIC",
                applyEffect = (c, step) =>
                {
                    c.humanity += (step == 1 ? 10 : 5);
                    if (step == 1) c.medicine += 1;
                    c.AddTrait("VOLUNTEER", "Heal morale faster.");
                    c.AddItem("First Aid Kit", 1);
                }
            }
        };

        public static readonly HobbyData[] DestructiveWeaknesses = new HobbyData[]
        {
            new HobbyData
            {
                id = "smoker",
                title = "SMOKER",
                description = "Lungs of tar. Severe addiction penalty.",
                statsPreview = "-STAMINA / +PERC",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.staminaMax -= 20; c.stamina = c.staminaMax; c.per += 1;
                    c.AddTrait("SMOKER", "-20 Stamina. Needs Nicotine.", null, true);
                    c.AddItem("Cigarettes", 1); c.AddItem("Lighter", 1);
                }
            },
            new HobbyData
            {
                id = "alcoholic",
                title = "ALCOHOLIC",
                description = "Numb to pain, but shakes without a drink.",
                statsPreview = "-INT / +HP",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.intl -= 1; c.hpMax += 20; c.hp = c.hpMax;
                    c.AddTrait("ALCOHOLIC", "+20 HP. -1 Int. Reduced Pain.", null, true);
                    c.AddItem("Whiskey", 1);
                }
            },
            new HobbyData
            {
                id = "asthmatic",
                title = "ASTHMATIC",
                description = "Running is difficult. Rely on an inhaler.",
                statsPreview = "-STAMINA / +INT",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.staminaMax -= 30; c.stamina = c.staminaMax; c.intl += 1;
                    c.AddTrait("ASTHMATIC", "Cannot run without Inhaler.", null, true);
                    c.AddItem("Inhaler", 1);
                }
            },
            new HobbyData
            {
                id = "blind",
                title = "BLIND",
                description = "Terrible vision without corrective lenses.",
                statsPreview = "-PERC / +TECH",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.per -= 2; c.tech += 1;
                    c.AddTrait("BLIND", "Needs glasses to shoot.", null, true);
                    c.AddItem("Glasses", 1);
                }
            },
            new HobbyData
            {
                id = "badknee",
                title = "BAD KNEE",
                description = "Old injury prevents fleeing. Stand your ground.",
                statsPreview = "-AGI / +STR",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.agi -= 2; c.str += 1;
                    c.AddTrait("BAD KNEE", "Cannot Flee.", null, true);
                    c.AddItem("Cane", 1);
                }
            },
            new HobbyData
            {
                id = "glutton",
                title = "GLUTTON",
                description = "Hard to kill, but consumes extra food.",
                statsPreview = "-AGI / +HP",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.agi -= 1; c.hpMax += 20; c.hp = c.hpMax;
                    c.AddTrait("GLUTTON", "Must eat 2x Food.", null, true);
                    c.AddItem("Bag of Chips", 2);
                }
            },
            new HobbyData
            {
                id = "fragile",
                title = "FRAGILE",
                description = "Bruise easily. Learned medicine to survive.",
                statsPreview = "-HP / +MEDIC",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.hpMax -= 20; c.hp = c.hpMax; c.medicine += 1;
                    c.AddTrait("FRAGILE", "Bleeding deals double damage.", null, true);
                    c.AddItem("Bandage", 3);
                }
            },
            new HobbyData
            {
                id = "paranoid",
                title = "PARANOID",
                description = "Trust no one. You repel potential allies.",
                statsPreview = "-CHA / +SCAVENGE",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.cha -= 2; c.scavenging += 1;
                    c.AddTrait("PARANOID", "Cannot sleep near NPCs.", null, true);
                    c.AddItem("Tinfoil Hat", 1);
                }
            },
            new HobbyData
            {
                id = "insomniac",
                title = "INSOMNIAC",
                description = "You never sleep well. Used to the dark.",
                statsPreview = "-HP / +STEALTH",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.hpMax -= 10; c.hp = c.hpMax; c.stealth += 1;
                    c.AddTrait("INSOMNIAC", "-10 HP. Night Vision.", null, true);
                    c.AddItem("Sleeping Pills", 1);
                }
            },
            new HobbyData
            {
                id = "germophobe",
                title = "CLEAN",
                description = "Obsessed with hygiene. Blood causes stress.",
                statsPreview = "-HUMANITY / +HEAL",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.humanity -= 20; c.medicine += 1;
                    c.AddTrait("CLEAN", "Healing items give +20 HP.", null, true);
                    c.AddItem("Hand Sanitizer", 1);
                }
            },
            new HobbyData
            {
                id = "coward",
                title = "COWARD",
                description = "Freeze in combat, but run fast.",
                statsPreview = "-STR / +ESCAPE",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.str -= 2; c.agi += 2;
                    c.AddTrait("COWARD", "-Damage. High escape chance.", null, true);
                    c.AddItem("Pepper Spray", 1);
                }
            },
            new HobbyData
            {
                id = "pyro",
                title = "PYRO",
                description = "Obsessed with fire. Zero empathy.",
                statsPreview = "-HUMANITY / +TECH",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.humanity -= 15; c.tech += 1;
                    c.AddTrait("PYRO", "Fire weapons deal +50% DMG.", null, true);
                    c.AddItem("Lighter", 1); c.AddItem("Jerry Can", 1);
                }
            },
            new HobbyData
            {
                id = "lonewolf",
                title = "LONE WOLF",
                description = "Rude and abrasive. Stronger when alone.",
                statsPreview = "-CHA / +STEALTH",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.cha -= 2; c.stealth += 1;
                    c.AddTrait("LONE WOLF", "+10% Stats when alone.", null, true);
                    c.AddItem("Combat Knife", 1);
                }
            },
            new HobbyData
            {
                id = "luddite",
                title = "LUDDITE",
                description = "Hate technology. Excellent with nature.",
                statsPreview = "-TECH / +SCAVENGE",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.tech = 0; c.scavenging += 1;
                    c.AddTrait("LUDDITE", "Cannot use electronics.", null, true);
                    c.AddItem("Old Map", 1);
                }
            },
            new HobbyData
            {
                id = "pacifist",
                title = "PACIFIST",
                description = "Refuse to kill humans. Negotiator.",
                statsPreview = "-COMBAT / +HUM",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.shooting = 0; c.melee = 0; c.cha += 1; c.humanity += 20;
                    c.AddTrait("PACIFIST", "Unique peaceful choices.", null, true);
                }
            },
            new HobbyData
            {
                id = "clumsy",
                title = "CLUMSY",
                description = "Loud and accident-prone. Tough.",
                statsPreview = "-STEALTH / +STR",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.stealth = 0; c.str += 1;
                    c.AddTrait("CLUMSY", "Start encounters at high alert.", null, true);
                    c.AddItem("Hard Hat", 1);
                }
            },
            new HobbyData
            {
                id = "gambler",
                title = "GAMBLER",
                description = "Took risks with money. Relies on luck.",
                statsPreview = "-INT / +LUCK",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.intl -= 1;
                    c.AddTrait("GAMBLER", "10% Auto-Hit or Auto-Miss.", null, true);
                    c.AddItem("Deck of Cards", 1);
                }
            },
            new HobbyData
            {
                id = "sheltered",
                title = "SHELTERED",
                description = "Naive about the world. Optimistic.",
                statsPreview = "-SCAVENGE / +HUMANITY",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.scavenging = 0; c.humanity += 20;
                    c.AddTrait("SHELTERED", "Starts with +20 Humanity.", null, true);
                    c.AddItem("Luxury Watch", 1);
                }
            },
            new HobbyData
            {
                id = "shopaholic",
                title = "SHOPPER",
                description = "Carries useless things. Good at bartering.",
                statsPreview = "-BAG SLOTS / +CHA",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.bagLimit -= 2; c.cha += 1;
                    c.AddTrait("SHOPPER", "Sell luxury items for 200%.", null, true);
                    c.AddItem("Designer Bag", 1);
                }
            },
            new HobbyData
            {
                id = "arrogant",
                title = "ARROGANT",
                description = "You correct everyone. Unlikable.",
                statsPreview = "-CHA / +INT",
                isWeakness = true,
                applyEffect = (c, _) =>
                {
                    c.cha -= 2; c.intl += 1;
                    c.AddTrait("ARROGANT", "Companions dislike you.", null, true);
                    c.AddItem("Encyclopedia", 1);
                }
            }
        };
    }
}

