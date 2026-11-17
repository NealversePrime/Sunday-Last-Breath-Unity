using System;
using System.Collections.Generic;
using UnityEngine;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using SundayLastBreath.UI;

namespace SundayLastBreath.Zones
{
    /// <summary>
    /// Section 3: Unit 407 Apartment Zone (Actor).
    /// Performs on the GameScreen Stage (Panggung).
    /// 
    /// Responsibilities:
    /// - Owns Unit 407's local floor stash & scavenged supplies.
    /// - Owns room state flags (kitchen, bathroom, closet, phone, radio, terminal, keys).
    /// - Drives the narrative choices and dialogue beats for waking up inside Unit 407.
    /// - Hands off to AscottFloor4Zone when the player unlocks the front door.
    /// </summary>
    public class Unit407ApartmentZone : MonoBehaviour
    {
        [Header("Local Floor Stash (Stays in 407)")]
        [SerializeField] private List<InventoryEntry> localFloorItems = new List<InventoryEntry>();

        [Header("Room State Flags")]
        public bool hasPacked = false;
        public bool kitchenSearched = false;
        public bool bathroomSearched = false;
        public bool closetSearched = false;
        public bool phoneVisited = false;
        public bool radioVisited = false;
        public bool hackingVisited = false;

        private GameStageScreen stage;

        public List<InventoryEntry> LocalFloorItems => localFloorItems;

        /// <summary>
        /// Resets the apartment zone's state for a fresh new game run.
        /// </summary>
        public void ResetZoneState()
        {
            hasPacked = false;
            kitchenSearched = false;
            bathroomSearched = false;
            closetSearched = false;
            phoneVisited = false;
            radioVisited = false;
            hackingVisited = false;
            localFloorItems.Clear();
        }

        /// <summary>
        /// Enters the Unit 407 Zone and boots the Stage display.
        /// </summary>
        public void EnterZone(GameStageScreen stageController)
        {
            stage = stageController;
            if (stage == null) return;

            // Sync flags directly from stage (accurately reflects both fresh New Game false states and Save Load true states)
            hasPacked = stage.HasPacked;
            kitchenSearched = stage.KitchenSearched;
            bathroomSearched = stage.BathroomSearched;
            closetSearched = stage.ClosetSearched;
            phoneVisited = stage.PhoneVisited;
            radioVisited = stage.RadioVisited;
            hackingVisited = stage.HackingVisited;

            var player = stage.CurrentPlayer;
            // Only auto-restore starting equipment if the player already packed previously (e.g. from a loaded save)
            if (hasPacked && player != null)
            {
                EnsureStartingGearRecovered(player);
            }

            // 1. Configure the Stage
            stage.SetLocation("unit_407", "UNIT 407 // YOUR APARTMENT");
            stage.PlayStageAnimation("Unit407Inside_Anim", "unit407", 8f);

            // 2. Sync local floor stash with the Stage from loaded floor items
            localFloorItems.Clear();
            if (stage.FloorItems != null && stage.FloorItems.Count > 0)
            {
                foreach (var item in stage.FloorItems)
                {
                    if (item == null || string.IsNullOrEmpty(item.itemId)) continue;
                    localFloorItems.Add(new InventoryEntry(item.itemId, item.quantity));
                }
            }
            stage.SyncFloorStash("unit_407", localFloorItems);

            // 3. Render the initial apartment hub state
            RenderApartmentHub();
        }

        /// <summary>
        /// Renders the current situation narration and choice menu on the Stage.
        /// </summary>
        public void RenderApartmentHub(string customNarration = null)
        {
            if (stage == null) return;

            // 1. Situation Narration
            if (!string.IsNullOrEmpty(customNarration))
            {
                stage.SetSituationNarration(customNarration, recordInLog: true);
            }
            else
            {
                string baseAnchor = GetApartmentAnchorText();
                stage.SetSituationNarration(baseAnchor, recordInLog: false);
            }

            // 2. Clear old choices on the Stage
            stage.ClearChoices();

            int choiceIdx = 0;

            // Choice 1: Pack Gear from Coffee Table (Disappears once packed)
            if (!hasPacked)
            {
                stage.SetChoice(choiceIdx++, "<b>Pick up my apartment keys, wallet, and work gear from the coffee table.</b> <color=#64748B><i>(~2 min)</i></color>", () =>
                {
                    OnPackGear();
                });
            }

            // Choice 2: Kitchenette
            if (!kitchenSearched)
            {
                stage.SetChoice(choiceIdx++, "<b>Check the kitchen cabinets and fridge for bottled water and food.</b> <color=#64748B><i>(~3 min)</i></color>", () =>
                {
                    OnSearchKitchen();
                });
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<s><color=#64748B>Check the kitchen cabinets and fridge.</color></s> <color=#475569><i>(Cleared)</i></color>", null, false);
            }

            // Choice 3: Bathroom
            if (!bathroomSearched)
            {
                stage.SetChoice(choiceIdx++, "<b>Check the bathroom medicine cabinet for painkillers and bandages.</b> <color=#64748B><i>(~2 min)</i></color>", () =>
                {
                    OnSearchBathroom();
                });
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<s><color=#64748B>Check the bathroom medicine cabinet.</color></s> <color=#475569><i>(Cleared)</i></color>", null, false);
            }

            // Choice 4: Front Closet
            if (!closetSearched)
            {
                stage.SetChoice(choiceIdx++, "<b>Grab my heavy coat, rain jacket, and flashlight from the front closet.</b> <color=#64748B><i>(~2 min)</i></color>", () =>
                {
                    OnSearchCloset();
                });
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<s><color=#64748B>Grab coat and flashlight from the front closet.</color></s> <color=#475569><i>(Cleared)</i></color>", null, false);
            }

            // Choice 5: Check Phone
            if (!phoneVisited)
            {
                stage.SetChoice(choiceIdx++, "<b>Check my phone to see if anyone knows what is happening outside.</b> <color=#64748B><i>(~1 min)</i></color>", () =>
                {
                    OnCheckPhone();
                });
            }

            // Choice 6: Emergency Radio
            if (!radioVisited)
            {
                var player = stage.CurrentPlayer;
                bool isPolice = player != null && string.Equals(player.job, "Police Officer", StringComparison.OrdinalIgnoreCase);
                bool hasRadioTrait = player != null && player.HasTrait("RADIO OPERATOR");
                bool hasTech = player != null && player.tech >= 1;
                bool hasTactics = player != null && player.Tactics >= 5;

                if (isPolice || hasRadioTrait || hasTech || hasTactics)
                {
                    string label = isPolice ? "<b>Turn on your department transceiver and listen to Seattle PD dispatch.</b> <color=#64748B><i>(~3 min) [Police]</i></color>" :
                                  hasRadioTrait ? "<b>Dial into the emergency dispatch band using your scanner.</b> <color=#64748B><i>(~4 min) [Radio Operator]</i></color>" :
                                  hasTech ? "<b>Manually calibrate the radio transceiver to sweep police frequencies.</b> <color=#64748B><i>(~5 min) [Tech]</i></color>" :
                                  "<b>Deduce the encrypted precinct frequencies and tune the scanner.</b> <color=#64748B><i>(~5 min) [Tactics 5+]</i></color>";

                    stage.SetChoice(choiceIdx++, label, () => OnListenRadio());
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "<s><color=#64748B>Tune the radio to emergency dispatch frequencies.</color></s> <color=#EF4444><i>[Requires Police, Tech, or Tactics 5+]</i></color>", null, false);
                }
            }

            // Choice 7: Terminal Hacking
            if (!hackingVisited)
            {
                var player = stage.CurrentPlayer;
                bool hasTechHacking = player != null && player.tech >= 3;
                bool hasTacticsHacking = player != null && player.Tactics >= 6;
                bool hasGamerTrait = player != null && player.HasTrait("GAMER");

                if (hasTechHacking || hasTacticsHacking || hasGamerTrait)
                {
                    string label = hasTechHacking ? "<b>Exploit building network vulnerabilities to inspect military traffic.</b> <color=#64748B><i>(~8 min) [Tech 3+]</i></color>" :
                                  hasTacticsHacking ? "<b>Analyze encrypted network packets to bypass defense protocols.</b> <color=#64748B><i>(~8 min) [Tactics 6+]</i></color>" :
                                  "<b>Run automated port scanners from your gaming rig.</b> <color=#64748B><i>(~8 min) [Gamer Trait]</i></color>";

                    stage.SetChoice(choiceIdx++, label, () => OnHackTerminal());
                }
            }

            // Choice 8: Exit to 4F Hallway
            if (!hasPacked)
            {
                stage.SetChoice(choiceIdx++, "<s><color=#64748B>Unlock the apartment door and step out.</color></s> <color=#EF4444><i>[LOCKED - Need keys from coffee table]</i></color>", () =>
                {
                    stage.PlaySFX("Audio/SFX/door_locked_rattle", 0.7f);
                    RenderApartmentHub("<color=#EF4444>[DOOR DEADBOLTED]</color> The heavy brass deadbolt is locked tight from the inside. You need to grab your apartment keys and essentials from the coffee table before unlocking the door.");
                }, true);
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<b><color=#F87171>Unlock the apartment door and step out into the hallway.</color></b> <color=#64748B><i>(No turning back)</i></color>", () =>
                {
                    OnLeaveApartment();
                });
            }

            // 3. Re-index hotkeys
            stage.ReindexChoices();
        }

        private string GetApartmentAnchorText()
        {
            if (hasPacked)
            {
                return "<color=#38BDF8><b>[APARTMENT 407 // READY TO LEAVE]</b></color>\n\n" +
                       "Your backpack is zipped and your apartment keys are in your pocket. Outside the window, the city is bathed in an eerie crimson gloom as rain lashes against the glass. Distant sirens wailed and abruptly fell silent ten minutes ago.\n\n" +
                       "You can scavenge the kitchen, bathroom, or closet for remaining supplies before unlocking the deadbolt and stepping out into the 4th-floor hallway.";
            }

            return "<color=#F59E0B><b>[APARTMENT 407 // LIVING ROOM]</b></color>\n\n" +
                   "The emergency broadcast cut out five minutes ago, leaving only a hiss of white static humming from the TV speakers. The power grid has completely collapsed. Outside your window, the Seattle skyline is dark, punctuated by billowing columns of smoke and flashing red lights.\n\n" +
                   "You need to get down to street level before whatever is happening outside reaches this building. Your keys and work gear are lying on the coffee table.";
        }

        public void OnPackGear()
        {
            stage.PlaySFX("Audio/SFX/click_tactical", 0.8f);
            hasPacked = true;
            if (stage != null) stage.HasPacked = true;
            stage.Clock?.AddTime(2);

            var player = stage.CurrentPlayer;
            if (player != null)
            {
                // Ensure key item
                if (!player.HasKeyItem("Apartment Key", true) && !player.HasItem("Apartment Key"))
                {
                    player.AddKeyItem("Apartment Key", "Apartment 407 Key", "Brass tumbler key labeled 'Room 407'. Required once to unlock the front door deadbolt.", stage.Clock != null ? stage.Clock.GetTimeString() : "23:52");
                }

                // Transfer all items from coffee table stash (which holds job starting items like Police Glock 19 & 9mm Ammo, and hobby items)
                if (stage.TableStash != null && stage.TableStash.Count > 0)
                {
                    for (int i = 0; i < stage.TableStash.Count; i++)
                    {
                        var entry = stage.TableStash[i];
                        if (entry == null || string.IsNullOrEmpty(entry.itemId)) continue;
                        if (string.Equals(entry.itemId, "Apartment Key", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(entry.itemId, "Apartment 407 Key", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!player.AddItem(entry.itemId, entry.quantity))
                        {
                            AddLocalFloorItem(entry.itemId, entry.quantity);
                        }
                    }
                    stage.TableStash.Clear();
                }

                // Guarantee all vocation gear, trait starting items, and EDC essentials are restored
                EnsureStartingGearRecovered(player);

                stage.Objectives?.CompleteObjective("pack_gear");
                stage.Objectives?.ActivateObjective("leave_apt");
            }

            stage.SyncFloorStash("unit_407", localFloorItems);
            stage.RefreshFromHybridCockpit();

            RenderApartmentHub("<color=#10B981><b>[ESSENTIALS SECURED]</b></color>\n\n" +
                              "You scoop up your keys, wallet, service weapon, and everyday carry from the coffee table. You pocket the heavy brass apartment key and tighten your backpack straps.\n\n" +
                              "The front door deadbolt can now be unlocked. Scavenge the kitchen, bathroom, or closet if you need extra supplies before leaving.");
        }

        public void OnSearchKitchen()
        {
            stage.PlaySFX("Audio/SFX/cabinet_open", 0.75f);
            kitchenSearched = true;
            if (stage != null) stage.KitchenSearched = true;
            stage.Clock?.AddTime(3);

            AddLocalFloorItem("Bottled Water", 2);
            AddLocalFloorItem("Canned Tuna", 1);
            AddLocalFloorItem("Can Opener", 1);
            AddLocalFloorItem("Stale Coffee", 1);

            stage.SyncFloorStash("unit_407", localFloorItems);

            RenderApartmentHub("<color=#F59E0B><b>[KITCHEN SEARCHED]</b></color>\n\n" +
                              "You step into the kitchenette. The refrigerator is already warming up in the blackout. You pull two bottles of water, canned tuna, and a manual can opener from the cupboards.\n\n" +
                              "<color=#38BDF8><i>(Supplies placed on the kitchen counter — check the Floor Items tab on your left to take them.)</i></color>");
        }

        public void OnSearchBathroom()
        {
            stage.PlaySFX("Audio/SFX/cabinet_open", 0.75f);
            bathroomSearched = true;
            if (stage != null) stage.BathroomSearched = true;
            stage.Clock?.AddTime(2);

            AddLocalFloorItem("Painkillers", 1);
            AddLocalFloorItem("Bandage", 2);
            AddLocalFloorItem("Rubbing Alcohol", 1);

            stage.SyncFloorStash("unit_407", localFloorItems);

            RenderApartmentHub("<color=#10B981><b>[MEDICINE CABINET CLEARED]</b></color>\n\n" +
                              "You flick on your light and check the bathroom vanity mirror. You sweep ibuprofen, sterile gauze rolls, and rubbing alcohol from the shelf.\n\n" +
                              "<color=#38BDF8><i>(Medical items placed on the counter — check Floor Items to take them.)</i></color>");
        }

        public void OnSearchCloset()
        {
            stage.PlaySFX("Audio/SFX/cabinet_open", 0.75f);
            closetSearched = true;
            if (stage != null) stage.ClosetSearched = true;
            stage.Clock?.AddTime(2);

            AddLocalFloorItem("Rain Poncho", 1);
            AddLocalFloorItem("Duct Tape", 1);
            AddLocalFloorItem("Matches", 1);

            stage.SyncFloorStash("unit_407", localFloorItems);

            RenderApartmentHub("<color=#38BDF8><b>[FRONT CLOSET SEARCHED]</b></color>\n\n" +
                              "You reach into the hallway coat closet. You pull down a heavy rain poncho, grab a fresh roll of duct tape, and a box of wooden matches.\n\n" +
                              "<color=#38BDF8><i>(Gear placed ready on the bench — check Floor Items to take them.)</i></color>");
        }

        private void OnCheckPhone()
        {
            phoneVisited = true;
            if (stage != null) stage.PhoneVisited = true;
            stage.Clock?.AddTime(1);
            stage.TriggerOpenPhone();
        }

        private void OnListenRadio()
        {
            stage.PlaySFX("Audio/SFX/radio_static_burst", 0.8f);
            radioVisited = true;
            if (stage != null) stage.RadioVisited = true;
            stage.Clock?.AddTime(3);

            RenderApartmentHub("<color=#38BDF8><b>[EMERGENCY SCANNER // ARMED PATROL LOG]</b></color>\n\n" +
                              "You adjust the tuning dial on your receiver. Through frantic bursts of static, a dispatcher's strained voice cuts through:\n\n" +
                              "<i>\"...All available units fall back from 4th and Pike. Repeating, perimeter at 4th is compromised. Suspects are unresponsive to TASER and small arms fire. Shoot on sight. Do not allow physical contact...\"</i>\n\n" +
                              "A sudden screech of interference cuts the feed dead. It's not a riot.");
        }

        private void OnHackTerminal()
        {
            stage.PlaySFX("Audio/SFX/computer_keystrokes", 0.8f);
            hackingVisited = true;
            if (stage != null) stage.HackingVisited = true;
            stage.Clock?.AddTime(8);

            RenderApartmentHub("<color=#F59E0B><b>[NETWORK TRAFFIC INTERCEPT // DOD PROXY]</b></color>\n\n" +
                              "Your terminal sweeps local subnets. An unencrypted military relay echoes on port 8080:\n\n" +
                              "<color=#64748B>> CONNECTION ESTABLISHED // SEATTLE TAC-NET\n" +
                              "> WARNING: PROTOCOL ODIN IN EFFECT\n" +
                              "> CONTAINMENT STATUS: CRITICAL FAILURE\n" +
                              "> AIRBORNE EXTRACTION AT HARBORVIEW SUSPENDED</color>\n\n" +
                              "No one is coming to save this neighborhood. You have to get out on your own.");
        }

        private void OnLeaveApartment()
        {
            if (stage != null)
            {
                stage.OnLeaveApartmentClicked();
            }
        }

        /// <summary>
        /// Universal starting gear recovery: Guarantees that the survivor's vocation gear,
        /// trait/hobby equipment, and everyday carry essentials exist in inventory (or floor overflow)
        /// when packed, and ensures Apartment Key is pocketed.
        /// </summary>
        public void EnsureStartingGearRecovered(CharacterData player)
        {
            if (player == null) return;

            // 1. Apartment 407 Key
            if (!player.HasKeyItem("Apartment Key", true) && !player.HasItem("Apartment Key") &&
                !player.HasKeyItem("Apartment 407 Key", true) && !player.HasItem("Apartment 407 Key"))
            {
                player.AddKeyItem("Apartment Key", "Apartment 407 Key", "Brass tumbler key labeled 'Room 407'. Required once to unlock the front door deadbolt.", stage != null && stage.Clock != null ? stage.Clock.GetTimeString() : "23:52");
            }

            // 2. Vocation / Job starting items
            if (!string.IsNullOrEmpty(player.job))
            {
                var job = System.Array.Find(CharacterCreationDefinitions.Jobs, j => 
                    string.Equals(j.title, player.job, StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(j.id, player.job, StringComparison.OrdinalIgnoreCase));

                if (job != null && job.startingItems != null)
                {
                    foreach (var item in job.startingItems)
                    {
                        if (!player.HasItem(item.item))
                        {
                            if (!player.AddItem(item.item, item.qty))
                            {
                                AddLocalFloorItem(item.item, item.qty);
                            }
                        }
                    }
                }
            }

            // 3. Trait / Hobby starting items
            if (player.traits != null)
            {
                foreach (var t in player.traits)
                {
                    if (string.Equals(t.name, "ARCHER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Compound Bow")) { if (!player.AddItem("Compound Bow", 1)) AddLocalFloorItem("Compound Bow", 1); }
                        if (!player.HasItem("Arrows")) { if (!player.AddItem("Arrows", 5)) AddLocalFloorItem("Arrows", 5); }
                    }
                    else if (string.Equals(t.name, "HIKER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Hiking Boots")) { if (!player.AddItem("Hiking Boots", 1)) AddLocalFloorItem("Hiking Boots", 1); }
                    }
                    else if (string.Equals(t.name, "RUNNER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Energy Bar")) { if (!player.AddItem("Energy Bar", 2)) AddLocalFloorItem("Energy Bar", 2); }
                        if (!player.HasItem("Running Shoes")) { if (!player.AddItem("Running Shoes", 1)) AddLocalFloorItem("Running Shoes", 1); }
                    }
                    else if (string.Equals(t.name, "HUNTER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Binoculars")) { if (!player.AddItem("Binoculars", 1)) AddLocalFloorItem("Binoculars", 1); }
                        if (!player.HasItem("Combat Knife")) { if (!player.AddItem("Combat Knife", 1)) AddLocalFloorItem("Combat Knife", 1); }
                    }
                    else if (string.Equals(t.name, "BASEBALL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Baseball Bat")) { if (!player.AddItem("Baseball Bat", 1)) AddLocalFloorItem("Baseball Bat", 1); }
                    }
                    else if (string.Equals(t.name, "WATCHMAN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Flashlight")) { if (!player.AddItem("Flashlight", 1)) AddLocalFloorItem("Flashlight", 1); }
                    }
                    else if (string.Equals(t.name, "MECHANIC", StringComparison.OrdinalIgnoreCase) || string.Equals(t.name, "SURVIVALIST", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Multi-Tool")) { if (!player.AddItem("Multi-Tool", 1)) AddLocalFloorItem("Multi-Tool", 1); }
                    }
                    else if (string.Equals(t.name, "PYRO", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Lighter")) { if (!player.AddItem("Lighter", 1)) AddLocalFloorItem("Lighter", 1); }
                        if (!player.HasItem("Jerry Can")) { if (!player.AddItem("Jerry Can", 1)) AddLocalFloorItem("Jerry Can", 1); }
                    }
                    else if (string.Equals(t.name, "GAMER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Fuse")) { if (!player.AddItem("Fuse", 2)) AddLocalFloorItem("Fuse", 2); }
                    }
                    else if (string.Equals(t.name, "CHEF", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("Combat Knife")) { if (!player.AddItem("Combat Knife", 1)) AddLocalFloorItem("Combat Knife", 1); }
                    }
                    else if (string.Equals(t.name, "VOLUNTEER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!player.HasItem("First Aid Kit")) { if (!player.AddItem("First Aid Kit", 1)) AddLocalFloorItem("First Aid Kit", 1); }
                    }
                }
            }

            // 4. Default EDC essentials
            string[] defaults = { "Flashlight", "Bandage", "Multi-Tool" };
            foreach (var d in defaults)
            {
                if (!player.HasItem(d))
                {
                    if (!player.AddItem(d, 1))
                    {
                        AddLocalFloorItem(d, 1);
                    }
                }
            }
        }

        private void AddLocalFloorItem(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return;
            var match = localFloorItems.Find(i => string.Equals(i.itemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                match.quantity += quantity;
            }
            else
            {
                localFloorItems.Add(new InventoryEntry(itemId, quantity));
            }
        }
    }
}
