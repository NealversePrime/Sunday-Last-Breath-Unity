using System;
using System.Collections.Generic;
using UnityEngine;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using SundayLastBreath.UI;

namespace SundayLastBreath.Zones
{
    /// <summary>
    /// Section 4: Floor 4 Hallway Zone (Actor / Screenplay).
    /// Performs on the GameStageScreen Stage (Panggung).
    /// 
    /// Responsibilities:
    /// - Owns the contiguous 4th-floor hallway corridor (Sections 1 through 4: Upper, Mid, Lower, End).
    /// - Owns all room inspections and choice decks for 4F:
    ///   - Unit 407 (Exterior door back into apartment)
    ///   - Unit 406 (Professor Harmon - Keypad puzzle, study books, looting)
    ///   - Unit 405 (Marcus - Booby trap disarm, gunsmith bench, safe, logs)
    ///   - Unit 404 (Mrs. Gable & Buster - Barricaded door, soothing dialogue)
    ///   - Unit 403 (Sarah - Swedish Medical Center shift slip, deadbolted)
    ///   - Unit 402 (The Andersons - Silent/Loud entry, record player, .38 revolver, looting)
    ///   - Unit 401 (Party flat - Oak shelf lift puzzle, stereo system, Hyundai car keys, looting)
    ///   - Utility Closet (Carl's workshop - Magnetic lock bypass, log, supplies)
    ///   - Emergency Stairwell (Phases 1-4 descent, blood trail, barricade, 2F landing arrival)
    /// - Manages state persistence via ES3 keys for all 4F hallway and room flags.
    /// </summary>
    public class Floor4HallwayZone : MonoBehaviour
    {
        [Header("Stage Reference")]
        private GameStageScreen stage;

        [Header("Hallway Navigation")]
        public int hallwaySection = 1;
        public string currentInspectedRoom = null;

        [Header("Room 406 (Professor Harmon)")]
        public bool room406_opened = false;
        public int failed_406_attempts = 0;
        public bool room406_locked_out = false;
        public bool studied_406_mechanics = false;
        public bool studied_406_anatomy = false;
        public bool read_406_journal = false;
        public bool looted_406_kitchen = false;
        public bool looted_406 = false;

        [Header("Room 405 (Marcus)")]
        public bool room405_opened = false;
        public bool room405_trap_triggered = false;
        public bool read_marcus_log = false;
        public bool looted_405_bench = false;
        public bool looted_405_safe = false;

        [Header("Room 404 (Mrs. Gable)")]
        public bool spoke_to_mrs_gable = false;

        [Header("Room 402 (The Andersons)")]
        public bool room402_opened = false;
        public string room402_entry_method = "";
        public bool looted_402_weapon = false;
        public bool looted_402_kitchen = false;
        public bool looted_402_bathroom = false;
        public bool looted_402_ring = false;
        public bool took_402_photo = false;
        public bool read_402_note = false;

        [Header("Room 401 (College Students Party Flat)")]
        public bool room401_entered = false;
        public bool shelf_lifted = false;
        public bool turned_off_music = false;
        public bool looted_401_kitchen = false;
        public bool looted_401_bathroom = false;
        public bool looted_401_alcohol = false;
        public bool looted_401_pockets = false;

        [Header("Utility Closet (Carl's Workshop)")]
        public bool utility_opened = false;
        public bool read_utility_log = false;
        public bool looted_utility_bleach = false;
        public bool looted_utility_tape = false;
        public bool looted_utility_flashlight = false;

        /// <summary>
        /// Resets all 4th floor hallway and room states for a fresh game.
        /// </summary>
        public void ResetZoneState()
        {
            hallwaySection = 1;
            currentInspectedRoom = null;

            room406_opened = false;
            failed_406_attempts = 0;
            room406_locked_out = false;
            studied_406_mechanics = false;
            studied_406_anatomy = false;
            read_406_journal = false;
            looted_406_kitchen = false;
            looted_406 = false;

            room405_opened = false;
            room405_trap_triggered = false;
            read_marcus_log = false;
            looted_405_bench = false;
            looted_405_safe = false;

            spoke_to_mrs_gable = false;

            room402_opened = false;
            room402_entry_method = "";
            looted_402_weapon = false;
            looted_402_kitchen = false;
            looted_402_bathroom = false;
            looted_402_ring = false;
            took_402_photo = false;
            read_402_note = false;

            room401_entered = false;
            shelf_lifted = false;
            turned_off_music = false;
            looted_401_kitchen = false;
            looted_401_bathroom = false;
            looted_401_alcohol = false;
            looted_401_pockets = false;

            utility_opened = false;
            read_utility_log = false;
            looted_utility_bleach = false;
            looted_utility_tape = false;
            looted_utility_flashlight = false;
        }

        /// <summary>
        /// Enters the 4F Hallway Zone, synchronizes state, and renders the view.
        /// </summary>
        public void EnterZone(GameStageScreen stageController, int section = 1, string inspectRoom = null)
        {
            stage = stageController;
            if (stage == null) return;

            stage.IsInHallwayInternal = true;
            hallwaySection = Mathf.Clamp(section, 1, 4);
            currentInspectedRoom = inspectRoom;

            // Sync tactical floor map
            stage.TacticalMap?.LoadFloorById(stage.IsOnSecondFloor ? "ascott_2f" : "ascott_4f");

            // Prepare stage UI
            stage.SetApartmentButtonsVisible(false);
            stage.EndAwakeningIntro();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionFromApartmentToHallway();
            }

            // If a specific room was being inspected (e.g. from a save file), restore inspection
            if (!string.IsNullOrEmpty(currentInspectedRoom))
            {
                RestoreInspectedRoom(currentInspectedRoom);
            }
            else
            {
                UpdateHallwayView();
            }
        }

        private void RestoreInspectedRoom(string roomId)
        {
            switch (roomId)
            {
                case "407": InspectUnit407Exterior(); break;
                case "406": InspectUnit406(); break;
                case "405": InspectUnit405(); break;
                case "404": InspectUnit404(); break;
                case "403": InspectUnit403(); break;
                case "402": InspectUnit402(); break;
                case "401": InspectUnit401(); break;
                case "utility": InspectUtilityCloset(); break;
                case "stairs": stage.EmergencyStairwellZone.EnterZone(stage); break;
                default: UpdateHallwayView(); break;
            }
        }

        #region Hallway Corridor Navigation (Sections 1 - 4)

        public void UpdateHallwayView()
        {
            if (stage == null) return;

            currentInspectedRoom = null;
            stage.CurrentInspectedRoomInternal = null;
            stage.HallwaySectionInternal = hallwaySection;

            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            stage.PlayHubBGM("Audio/BGM/hallway_drone", 0.5f);

            int choiceIdx = 0;

            switch (hallwaySection)
            {
                case 1:
                    stage.SetLocation("hallway_1", "UPPER HALLWAY (406 / 407)");
                    stage.PlayStageAnimation("Hall406_407_Anim", "hall_406_407", 8f);

                    stage.ShowLocationAnchor(
                        "hallway_1",
                        "The door clicks shut behind you, locking you out of the only place you felt safe.\n\n<color=#93C5FD><b>UPPER HALLWAY.</b></color> One of the emergency bulbs overhead is buzzing and blinking on and off. The air feels still, so every siren outside comes through the hallway vents. Your unit (407) is to your right. Across the carpet is <b>Unit 406 (Professor Harmon)</b>.",
                        "<color=#93C5FD><b>UPPER HALLWAY.</b></color> Back at the north end of the 4th floor. The ceiling bulb still buzzes weakly over the carpet between your locked door (407) and Unit 406.",
                        $"<color=#93C5FD><b>UPPER HALLWAY.</b></color> Your front door (407) stays latched tight. Across the hall, Harmon's apartment (406) {(room406_opened ? "stands unlocked in the dark." : "still glows faintly at its blue keypad.")}",
                        "<color=#93C5FD><b>UPPER HALLWAY.</b></color> Dust hangs still under the blinking bulb while distant street sirens echo softly down through the hallway vents."
                    );

                    stage.SetChoice(choiceIdx++, "INVESTIGATE UNIT 406 <color=#9CA3AF>— Professor Harmon's door. Electronic keypad. Silence inside.</color>", () =>
                    {
                        if (room406_opened) stage.PlayClickSFX();
                        InspectUnit406();
                    });

                    stage.SetChoice(choiceIdx++, "EXAMINE UNIT 407 (YOUR DOOR) <color=#9CA3AF>— Your apartment door. Locked tight behind you.</color>", () => InspectUnit407Exterior());

                    stage.SetChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Walk south toward Units 404 & 405.</color>", () => AdvanceDownHall());
                    break;

                case 2:
                    stage.SetLocation("hallway_2", "MID-HALLWAY (404 / 405)");
                    stage.PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);

                    string midHallAudio = spoke_to_mrs_gable
                        ? "Behind Mrs. Gable's door (404), Buster lets out a soft, calmer whine. Across from her, the reinforced door of <b>Unit 405 (Marcus)</b> isn't moving."
                        : "Behind Mrs. Gable's door (404), Buster is barking nonstop and scratching at the bottom wood. Across from her, the reinforced door of <b>Unit 405 (Marcus)</b> sits slightly open.";

                    stage.ShowLocationAnchor(
                        "hallway_2",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> The hallway light here barely holds steady. {midHallAudio} There's a faint smell of gun oil and burnt powder in the air.",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> You step back into the middle stretch of the hall. {(spoke_to_mrs_gable ? "Mrs. Gable's apartment (404) stays quiet apart from Buster breathing behind the door." : "Buster is still barking and scratching behind Unit 404.")}",
                        $"<color=#93C5FD><b>MID-HALLWAY.</b></color> Under the uneven hallway light, {(room405_opened ? "Marcus's reinforced door (405) sits unlocked." : "Marcus's heavy steel door (405) remains cracked a fraction of an inch.")} The sharp scent of gun oil still hangs here.",
                        "<color=#93C5FD><b>MID-HALLWAY.</b></color> Nothing moved in this stretch of the corridor while you were gone—just buzzing wiring and faint sirens through the vents."
                    );

                    string unit405Label = room405_opened
                        ? "ENTER UNIT 405 <color=#9CA3AF>— Marcus's apartment. Door open, safe and bench inside.</color>"
                        : "INVESTIGATE UNIT 405 <color=#9CA3AF>— Marcus's place. Heavy steel door ajar. Scent of cordite.</color>";

                    stage.SetChoice(choiceIdx++, unit405Label, () =>
                    {
                        if (room405_opened) stage.PlayClickSFX();
                        InspectUnit405();
                    });

                    string unit404Label = spoke_to_mrs_gable
                        ? "CHECK UNIT 404 <color=#9CA3AF>— Mrs. Gable's door. Reassured; barricaded with Buster.</color>"
                        : "CHECK UNIT 404 <color=#9CA3AF>— Mrs. Gable's door. Barricaded inside; dog barking.</color>";

                    stage.SetChoice(choiceIdx++, unit404Label, () => InspectUnit404());

                    stage.SetChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Upper Hallway & Unit 407.</color>", () => RetreatUpHall());

                    stage.SetChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Lower Hallway.</color>", () => AdvanceDownHall());
                    break;

                case 3:
                    stage.SetLocation("hallway_3", "LOWER HALLWAY (402 / 403)");
                    stage.PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);

                    stage.ShowLocationAnchor(
                        "hallway_3",
                        "<color=#93C5FD><b>LOWER HALLWAY.</b></color> The ceiling lights here are completely dead. A sharp smell of bleach drifts out from under one of the doors. Soft classical piano is playing behind the chained door of <b>Unit 402 (The Andersons)</b>. Across the hall, <b>Unit 403 (Sarah)</b> is dark and quiet.",
                        "<color=#93C5FD><b>LOWER HALLWAY.</b></color> Back in the dark stretch between Units 402 and 403. Slow piano notes still drift softly under the Andersons' doorframe.",
                        $"<color=#93C5FD><b>LOWER HALLWAY.</b></color> A chemical bleach smell still bites the back of your throat. {(room402_opened ? "Unit 402's door sits unchained." : "Unit 402 remains held on its chain, while Sarah's door (403) stays dark.")}",
                        "<color=#93C5FD><b>LOWER HALLWAY.</b></color> Dead overhead fixtures leave this corner in near darkness. Only the piano record in 402 breaks the stillness."
                    );

                    string unit402Label = room402_opened
                        ? "ENTER UNIT 402 <color=#9CA3AF>— The Andersons' flat. Chain bypassed. Piano playing.</color>"
                        : "INVESTIGATE UNIT 402 <color=#9CA3AF>— The Andersons' door. Chained from the inside. Classical music.</color>";

                    stage.SetChoice(choiceIdx++, unit402Label, () =>
                    {
                        if (room402_opened) stage.PlayClickSFX();
                        InspectUnit402();
                    });

                    stage.SetChoice(choiceIdx++, "CHECK UNIT 403 <color=#9CA3AF>— Sarah's apartment. Deadbolted from the outside; dark inside.</color>", () => InspectUnit403());

                    stage.SetChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Mid-Hallway.</color>", () => RetreatUpHall());

                    stage.SetChoice(choiceIdx++, "[ADVANCE] ADVANCE DOWN THE HALL <color=#9CA3AF>— Proceed south toward Hallway End.</color>", () => AdvanceDownHall());
                    break;

                case 4:
                    stage.SetLocation("hallway_4", "HALLWAY END (401 / UTILITY)");
                    stage.PlayStageAnimation("HallEnd_Anim", "hall_end", 8f);

                    string hall4NoiseText = turned_off_music
                        ? "<b>Unit 401 (The College Students)</b> is finally quiet with the stereo shut off, leaving the end of the hall dead silent."
                        : "From <b>Unit 401 (The College Students)</b>, loud pop music thumps right through the wood, drowning out everything else.";

                    stage.ShowLocationAnchor(
                        "hallway_4",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> The hall ends at the heavy red fire doors to the stairwell. Dried bloody handprints smear the small glass window on the door. {hall4NoiseText} To your left is Carl's <b>Utility Closet</b>.",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> You step back up by the red stairwell doors. {(turned_off_music ? "With Unit 401's stereo shut off, the end of the floor is dead silent." : "Loud pop music is still thumping through Unit 401's open doorway.")}",
                        $"<color=#93C5FD><b>HALLWAY END.</b></color> Smeared handprints still mark the stairwell window glass. {(utility_opened ? "Carl's maintenance door stands unlatched on your left." : "To your left, the maintenance mag-lock hums steadily.")}",
                        "<color=#93C5FD><b>HALLWAY END.</b></color> Worn hall carpet meets bare concrete in front of the stairwell frame and service closet."
                    );

                    string unit401Label;
                    if (!room401_entered)
                        unit401Label = "INVESTIGATE UNIT 401 <color=#9CA3AF>— The Party flat. Door wide open. Music inside.</color>";
                    else if (turned_off_music)
                        unit401Label = "ENTER UNIT 401 <color=#9CA3AF>— The Party flat. Stereo silenced. Scavenged living room.</color>";
                    else
                        unit401Label = "ENTER UNIT 401 <color=#9CA3AF>— The Party flat. Pop music blaring inside.</color>";

                    stage.SetChoice(choiceIdx++, unit401Label, () =>
                    {
                        if (!room401_entered)
                        {
                            room401_entered = true;
                            stage.PlayDoorTransition(
                                "Videos/unit401_door_transition",
                                "[ UNIT 401 // THE PARTY RESIDENCE ]",
                                "You push open the cracked front door.\n<color=#94A3B8>Pop music thuds through the corridor as the hallway lights reflect on empty cups...</color>",
                                false,
                                () => InspectUnit401(),
                                false
                            );
                        }
                        else
                        {
                            stage.PlayClickSFX();
                            InspectUnit401();
                        }
                    });

                    string utilityLabel = utility_opened
                        ? "ENTER UTILITY CLOSET <color=#9CA3AF>— Maintenance workshop. Magnetic lock bypassed.</color>"
                        : "CHECK UTILITY CLOSET <color=#9CA3AF>— Maintenance room. Industrial steel door and magnetic lock.</color>";

                    stage.SetChoice(choiceIdx++, utilityLabel, () =>
                    {
                        if (utility_opened) stage.PlayClickSFX();
                        InspectUtilityCloset();
                    });

                    stage.SetChoice(choiceIdx++, "<color=#EF4444>ENTER STAIRWELL (EXIT)</color> <color=#FCA5A5>— Point of no return. Push through bloody fire doors.</color>", () =>
                    {
                        stage.PlayDoorTransition(
                            "Videos/stairs_door_transition",
                            "[ EMERGENCY STAIRWELL // EXIT ]",
                            "You push hard against the emergency crash bar.\n<color=#94A3B8>The heavy fire door swings open into the stairwell darkness...</color>",
                            false,
                            () => stage.EmergencyStairwellZone.EnterZone(stage, 1),
                            true
                        );
                    }, true, new Color(0.95f, 0.25f, 0.25f));

                    stage.SetChoice(choiceIdx++, "[RETREAT] RETREAT UP THE HALL <color=#9CA3AF>— Back toward Lower Hallway.</color>", () => RetreatUpHall());
                    break;
            }

            stage.ReindexChoices();
        }

        public void AdvanceDownHall()
        {
            if (hallwaySection < 4)
            {
                hallwaySection++;
                stage.Clock?.AddTime(1);
                stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.65f);
                UpdateHallwayView();
            }
        }

        public void RetreatUpHall()
        {
            if (hallwaySection > 1)
            {
                hallwaySection--;
                stage.Clock?.AddTime(1);
                stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Footsteps/FootstepWood1.wav", 0.65f);
                UpdateHallwayView();
            }
        }

        #endregion

        #region Room Inspection: Unit 407 (Exterior)

        public void InspectUnit407Exterior()
        {
            currentInspectedRoom = "407";
            stage.CurrentInspectedRoomInternal = "407";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            stage.SetLocation("hallway_1", "YOUR APARTMENT (DOORWAY)", "unit_407");
            stage.UpdateLocationDisplay("UNIT 407", "YOUR APARTMENT (DOORWAY)");
            stage.PlayStageAnimation("Hall406_407_Anim", "hall_406_407", 8f);
            stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.9f);

            stage.ShowLocationAnchor(
                "ext_407",
                "You grab the brass knob of Unit 407 and twist. It doesn't budge.\n\n<color=#EF4444><b>*CLACK*</b></color>\n\nThe deadbolt snapped back into place when the door shut behind you, and your key only works that latch from the inside—trying to kick your own door in now would bring whatever's downstairs straight up here.\n\nNo going back inside. The only way is down the hall.",
                "You test the brass knob of Unit 407 again. Locked deadbolt-tight—your apartment key only turns that latch from the inside.",
                "Still shut solid. No getting back into 407 without tearing the frame apart."
            );

            stage.SetChoice(choiceIdx++, "[BACK] Return to Upper Hallway", () => EnterZone(stage, 1));
            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 406 (Professor Harmon)

        public void OpenUnit406Door()
        {
            room406_opened = true;
            if (stage.CurrentPlayer != null) stage.CurrentPlayer.SetFlag("room406_opened", true);
            stage.PlayClickSFX();

            stage.PlayDoorTransition(
                "Videos/unit406_door_transition",
                "[ UNIT 406 // PROFESSOR'S STUDY ]",
                "The electronic keypad beeps twice with a soft green indicator light.\n<color=#94A3B8>The heavy internal deadbolt retracts, and the door clicks open into darkness...</color>",
                false,
                () => InspectUnit406(),
                false
            );
        }

        public void InspectUnit406(string actionNarration = null)
        {
            currentInspectedRoom = "406";
            stage.CurrentInspectedRoomInternal = "406";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (!room406_opened)
            {
                stage.SetLocation("hallway_1", "THE KEYPAD", "unit_406");
                stage.UpdateLocationDisplay("UNIT 406", "THE KEYPAD");
                stage.PlayStageAnimation("Unit406Door_Anim", "unit406_door", 8f);
                stage.PlayClickSFX();

                string lockoutMsg = room406_locked_out
                    ? "\n\n<color=#EF4444><b>[LOCKED OUT] PERMANENTLY LOCKED OUT.</b> The keypad triggered its security failsafe. The software is sealed. You will have to bypass it physically.</color>"
                    : (failed_406_attempts > 0 ? $"\n\n<color=#F59E0B>[!] WRONG CODE. Attempts remaining: {3 - failed_406_attempts}.</color>" : "");

                string noteProse = "Taped above the keypad is a yellow index card in his neat, cramped handwriting: <i>\"If you're still on this floor, I posted my door code on the building feed before heading out. Grab what you need from the desk and get out before morning.\"</i>";

                stage.ShowLocationAnchor(
                    "ext_406",
                    $"Professor Harmon taught sociology at the university—mostly on how cities hold together when things fall apart. His door is locked with a commercial keypad, the blue digits glowing faintly in the dark hall. Four blank digits. Waiting.\n\n{noteProse}{lockoutMsg}",
                    $"You step back up to Harmon's door. Blue keypad numbers glow quietly in the dark hall.{lockoutMsg}",
                    $"Unit 406's yellow index card still hangs taped above the keypad. Four blank digits wait for a code.{lockoutMsg}"
                );

                bool hasPhoneCode = stage.PhoneVisited || (player != null && player.HasFlag("checked_phone_harmon"));

                if (!room406_locked_out)
                {
                    stage.SetChoice(choiceIdx++, "[CODE] Enter keypad code manually <color=#9CA3AF>— Four digits. 3 attempts limit.</color>", () =>
                    {
                        stage.Clock?.AddTime(1);
                        stage.OpenKeypadModal(
                            onUnlocked: () => OpenUnit406Door(),
                            onLockedOut: () =>
                            {
                                room406_locked_out = true;
                                if (player != null) player.SetFlag("room406_locked_out", true);
                                stage.PlayBeepSFX();
                                InspectUnit406();
                            },
                            onCancelled: () => InspectUnit406()
                        );
                    });
                }

                bool isBurglar = player != null && string.Equals(player.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmith = player != null && player.HasTrait("LOCKSMITH");
                bool hasTech2 = player != null && player.tech >= 2;
                bool hasMultiTool = player != null && player.HasItem("Multi-Tool");
                int perVal = player != null ? player.per : 5;
                int strVal = player != null ? player.str : 5;

                if (isBurglar)
                {
                    stage.SetChoice(choiceIdx++, "Override electronic latch [BURGLAR] <color=#F59E0B>(-7 Stamina, ~2 min)</color>", () =>
                    {
                        stage.ExertStamina(7);
                        stage.Clock?.AddTime(2);
                        OpenUnit406Door();
                    });
                }

                if (hasLocksmith)
                {
                    stage.SetChoice(choiceIdx++, "Exploit mechanical failsafe [LOCKSMITH] <color=#F59E0B>(-7 Stamina, ~3 min)</color>", () =>
                    {
                        stage.ExertStamina(7);
                        stage.Clock?.AddTime(3);
                        OpenUnit406Door();
                    });
                }

                // Phone Intel
                AddModularChoice(
                    ref choiceIdx,
                    hasPhoneCode,
                    "[PHONE INTEL]",
                    hasPhoneCode ? "Punch in Harmon's door code (8445)" : "Punch in Harmon's door code",
                    0, 1,
                    () =>
                    {
                        stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:15", "UNIT 406 // KEYPAD UNLOCKED", "Used Professor Harmon's emergency keypad code (8445) from his phone feed. Deadbolt retracted.");
                        OpenUnit406Door();
                    },
                    "Requires Harmon's Door Code from Phone Feed — You haven't checked his emergency feed"
                );

                // Tech 2 Knowledge
                AddModularChoice(
                    ref choiceIdx,
                    hasTech2,
                    "[TECH 2]",
                    "Bypass keypad circuit board",
                    5, 4,
                    () => OpenUnit406Door(),
                    $"Requires Tech 2 (You have {player?.tech ?? 0}) — You lack knowledge of circuit bypasses"
                );

                // Multi-Tool
                AddModularChoice(
                    ref choiceIdx,
                    hasMultiTool,
                    "[MULTI-TOOL]",
                    "Disassemble the housing cleanly",
                    5, 3,
                    () => OpenUnit406Door(),
                    "Requires Multi-Tool — You have no precision screwdrivers to remove security screws"
                );

                // Might 6 vs Fallback Pry
                if (strVal >= 6)
                {
                    stage.SetChoice(choiceIdx++, $"[MIGHT {strVal}/6] Smash keypad housing <color=#10B981>(-10 Stamina, 100% Guaranteed, ~2 min)</color>", () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(2);
                        OpenUnit406Door();
                    }, true);
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 10, 10, 35);
                    stage.SetChoice(choiceIdx++, $"[DESPERATE PRY] Wedge flat blade into lock frame [Might {strVal}/6 — {chance}% Chance] <color=#EF4444>(-15 Stamina, Injury Risk)</color>", () =>
                    {
                        stage.ExertStamina(15);
                        stage.Clock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            OpenUnit406Door();
                        }
                        else
                        {
                            stage.PlayBeepSFX();
                            stage.TakeDamage(10, "Blunt trauma prying steel deadbolt");
                            stage.SetSituationNarration("<color=#EF4444><b>BRUTE FORCE FAILED (-10 HP, -15 Stamina).</b></color> You battered the housing, but the internal deadbolt didn't budge. Your hands are bruised and bleeding.");
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Give up and return to hallway", () => EnterZone(stage, 1), true);
            }
            else
            {
                stage.SetLocation("unit_406", "PROFESSOR'S STUDY");
                stage.PlayStageAnimation("Unit406Inside_Anim", "unit406", 8f);
                stage.PlayHubBGM("Audio/SFX/tick", 0.25f, true);

                bool all406Done = studied_406_mechanics && studied_406_anatomy && read_406_journal && looted_406_kitchen && looted_406;
                string completionNotice = all406Done
                    ? "\n\n<color=#10B981><i>You've stripped the room of everything valuable — knowledge and supplies both. The rest is just dead weight.</i></color>"
                    : "";

                string roomAnchor = "Floor-to-ceiling shelves line all four walls, smelling of old paper and peppermint. On the table, Harmon's reading glasses rest on <i>The Fall of Rome</i> beside a yellow sticky note:\n<i>\"Every empire ends. We're just living in the intermission. Take what you need.\"</i>";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration($"{actionNarration}{completionNotice}");
                }
                else
                {
                    stage.ShowLocationAnchor(
                        "int_406",
                        $"The door swings open into darkness. The smell hits first: old paper, dust, and peppermint. Desk drawers hang open, and his heavy coat and hiking pack are gone.\n\n{roomAnchor}{completionNotice}",
                        $"You step back into Harmon's dark study. Tall bookshelves still smell faintly of dust and peppermint.{completionNotice}",
                        $"Back inside Unit 406. His reading glasses still sit on <i>The Fall of Rome</i> across the desk.{completionNotice}"
                    );
                }

                if (!studied_406_mechanics)
                {
                    stage.SetChoice(choiceIdx++, "[STUDY] Skim 'Advanced Mechanics' notes <color=#10B981>[TECH +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>
                    {
                        studied_406_mechanics = true;
                        stage.Clock?.AddTime(10);
                        if (player != null) player.tech += 1;
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit406("<color=#10B981>[TECH +1]</color> You pull a worn repair manual off the shelf and flip to the bookmarked pages. Harmon sketched clear wiring notes in the margins—starter relays, basic door latches, and how to jump them without frying the board. You memorize the basics, slide the book back into place, and step back into the room.");
                    });
                }

                if (!studied_406_anatomy)
                {
                    stage.SetChoice(choiceIdx++, "[STUDY] Skim 'Gray's Anatomy' notes <color=#10B981>[MEDICINE +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>
                    {
                        studied_406_anatomy = true;
                        stage.Clock?.AddTime(10);
                        if (player != null) player.medicine += 1;
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit406("<color=#10B981>[MEDICINE +1]</color> You pull a dog-eared anatomy textbook off the shelf. Under a bookmarked page, Harmon underlined one note twice in red pen: <color=#94A3B8>\"People coming into the ER weren't slowing down from broken ribs or blood loss. Only hard trauma to the upper neck or skull put them down.\"</color> You lock the note into memory, put the book back, and step back to the middle of the room.");
                    });
                }

                if (!read_406_journal)
                {
                    stage.SetChoice(choiceIdx++, "[NOTE] Read personal journal <color=#10B981>[PER +1]</color> <color=#9CA3AF>(~10 min)</color>", () =>
                    {
                        read_406_journal = true;
                        stage.Clock?.AddTime(10);
                        if (player != null) player.per += 1;
                        stage.Lore?.AddLore("Professor Harmon's Journal", "His handwriting gets messier near the back. By 22:30 he was jotting down police scanner chatter and rumors from his doctor friends: 'Half the downtown ERs stopped answering calls at nine. City hall keeps telling reporters it's a bad flu strain and bad shellfish, while patrol cars are shutting down the on-ramps. Nobody at the top is telling the same story.'", stage.Clock?.GetTimeString() ?? "00:15");
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit406("<color=#10B981>[PERCEPTION +1]</color> You walk over to the mahogany desk and open his leather notebook. The last entry, timed at 22:30, is scribbled fast across the lined paper:\n\n<color=#94A3B8>\"Two nurses I know downtown stopped answering at nine. Evening news called it a bad flu and contaminated water, while the police scanner is calling for roadblocks on I-5. Whatever started tonight, nobody's getting the same story twice. Don't wait until morning to get out.\"</color>\n\nYou close the cover and step back into the middle of the room.");
                    });
                }

                if (!looted_406_kitchen)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Search abandoned kitchen <color=#9CA3AF>(Canned Food x1, Energy Bar x1, Bottled Water x2)</color>", () =>
                    {
                        looted_406_kitchen = true;
                        stage.Clock?.AddTime(5);
                        stage.GiveOrDropItem("Canned Food", 1);
                        stage.GiveOrDropItem("Energy Bar", 1);
                        stage.GiveOrDropItem("Bottled Water", 2);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit406("<color=#F59E0B>Kitchen pantry raided.</color> You walk into the small kitchenette and check the cupboards. Harmon packed light for speed, leaving canned food, an energy bar, and two sealed bottles of water on the counter. You pack them into your bag and return to the center of the study.");
                    });
                }

                if (!looted_406)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Raid desk drawers <color=#9CA3AF>(Encyclopedia x1, Flashlight x1)</color>", () =>
                    {
                        looted_406 = true;
                        stage.Clock?.AddTime(5);
                        stage.GiveOrDropItem("Encyclopedia", 1);
                        stage.GiveOrDropItem("Flashlight", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit406("<color=#38BDF8>Desk drawers looted.</color> You crouch beside the oak desk and pull open the deep drawers beneath lecture drafts. Inside, you find a heavy hardbound Encyclopedia—solid enough to crush bone in a pinch—and a fully charged metal Flashlight. You slide the drawer shut and stand back in the middle of the room.");
                    });
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave study and return to hallway", () => EnterZone(stage, 1));
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 405 (Marcus)

        public void OpenUnit405Door(bool triggered = false)
        {
            room405_opened = true;
            if (stage.CurrentPlayer != null) stage.CurrentPlayer.SetFlag("room405_opened", true);
            stage.PlayClickSFX();

            string subtitle = triggered
                ? "The booby trap detonated in an ear-shattering blast.\n<color=#EF4444>Ears ringing, you push past the scorched doorframe into the gunsmith's quarters...</color>"
                : "The tripwire mechanism is safely neutralised.\n<color=#94A3B8>The reinforced steel door slowly swings open into the gunsmith's quarters...</color>";

            stage.PlayDoorTransition(
                "Videos/unit405_door_transition",
                "[ UNIT 405 // MARCUS'S APARTMENT ]",
                subtitle,
                false,
                () => InspectUnit405(),
                false
            );
        }

        public void InspectUnit405(string actionNarration = null)
        {
            currentInspectedRoom = "405";
            stage.CurrentInspectedRoomInternal = "405";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (!room405_opened)
            {
                stage.SetLocation("hallway_2", "THE BOOBY TRAP", "unit_405");
                stage.UpdateLocationDisplay("UNIT 405", "THE BOOBY TRAP");
                stage.PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);
                stage.PlayHubBGM("Audio/BGM/tension_drone", 0.4f);

                string trapStatus = room405_trap_triggered
                    ? "\n\n<color=#EF4444>[TRAP TRIGGERED] Your ears are ringing. The blank shell detonated, echoing down the corridor. The wire is burned away.</color>"
                    : "";

                stage.ShowLocationAnchor(
                    "ext_405",
                    $"This is Marcus's apartment. The quiet veteran down the hall spent his weekends cleaning hunting gear and reinforcing his doorframes. His heavy steel door isn't latched—it sits cracked open by a fraction of an inch, leaking the sharp smell of gun oil and burnt powder into the hall.\n\nYou nudge the door gently with one knuckle. It catches after half an inch with a small metallic click. Angling your flashlight through the gap shows a thin fishing line tied from the inside handle to a pull-pin canister and a shotgun blank clamped to a pipe. Shoving this open will set both off in your face and draw everything on this floor.{trapStatus}",
                    $"Marcus's reinforced steel door still sits open a fraction of an inch. Gun oil and burnt powder hang sharp in the hall.{trapStatus}",
                    $"Back at Unit 405's threshold. Through the narrow crack, the taut fishing line still waits behind the handle.{trapStatus}"
                );

                bool isBurglar = player != null && string.Equals(player.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmith = player != null && player.HasTrait("LOCKSMITH");
                bool hasTech2 = player != null && player.tech >= 2;
                bool hasMultiTool = player != null && player.HasItem("Multi-Tool");
                int perVal = player != null ? player.per : 5;

                // 1. Burglar or Locksmith
                AddModularChoice(
                    ref choiceIdx,
                    isBurglar || hasLocksmith,
                    "[BURGLAR/LOCKSMITH]",
                    "Jam the firing pin mechanism with a shim",
                    10, 2,
                    () => OpenUnit405Door(false),
                    "Requires Burglar or Locksmith — You lack the delicate touch to block an armed firing pin"
                );

                // 2. Tech 2
                AddModularChoice(
                    ref choiceIdx,
                    hasTech2,
                    "[TECH 2]",
                    "Release tension spring without tripping trigger",
                    10, 3,
                    () => OpenUnit405Door(false),
                    $"Requires Tech 2 (You have {player?.tech ?? 0}) — You don't understand the mechanical trigger assembly"
                );

                // 3. Multi-Tool
                AddModularChoice(
                    ref choiceIdx,
                    hasMultiTool,
                    "[MULTI-TOOL]",
                    "Cut and clamp the monofilament tripwire",
                    10, 2,
                    () => OpenUnit405Door(false),
                    "Requires Multi-Tool / Wire Cutters — Cannot cut taut monofilament safely without pliers"
                );

                // 4. Observation 6 vs Fallback Squeeze
                if (perVal >= 6)
                {
                    stage.SetChoice(choiceIdx++, $"[OBSERVATION {perVal}/6] Squeeze carefully through the gap <color=#10B981>(-15 Stamina, 100% Guaranteed, ~4 min)</color>", () =>
                    {
                        stage.ExertStamina(15);
                        stage.Clock?.AddTime(4);
                        OpenUnit405Door(false);
                    }, true);
                }
                else
                {
                    int chance = Mathf.Clamp(perVal * 8, 15, 45);
                    stage.SetChoice(choiceIdx++, $"[RISKY SQUEEZE] Squeeze past monofilament [Observation {perVal}/6 — {chance}% Chance] <color=#EF4444>(-20 Stamina, Lethal Detonation Risk)</color>", () =>
                    {
                        stage.ExertStamina(20);
                        stage.Clock?.AddTime(4);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            OpenUnit405Door(false);
                        }
                        else
                        {
                            room405_trap_triggered = true;
                            stage.PlaySFX("Audio/SFX/firedoor_slam", 0.9f);
                            stage.PlaySFX("Audio/SFX/elbow_smash_glass", 0.85f);
                            stage.PlaySFX("Audio/SFX/distant_explosion", 0.7f);
                            stage.TakeDamage(25, "12-gauge blank blast & flashbang detonation");
                            stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:20", "UNIT 405 // BOOBY TRAP DETONATED", "Detonated Marcus's door rig. The 12-gauge blank blast and flashbang detonated, causing 25 damage (-25 HP, -20 Stamina) and severe ringing in your ears.");
                            OpenUnit405Door(true);
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Step back and return to hallway", () => EnterZone(stage, 2), true);
            }
            else
            {
                stage.SetLocation("unit_405", "THE HUNTER'S SANCTUARY");
                stage.PlayStageAnimation("Unit405Inside_Anim", "unit405", 8f);

                bool all405Done = read_marcus_log && looted_405_bench && looted_405_safe;
                string completionNotice = all405Done
                    ? "\n\n<color=#EF4444>You've taken everything useful Marcus left behind. Near the corner of the open safe, a small wooden toy car sits on the carpet. His family is waiting at Oakhaven for someone who isn't coming back.</color>"
                    : "";

                string roomAnchor = "The apartment looks half-turned into a bunker. A heavy timber reloading bench sits against the far wall, dusted with loose powder and spent brass. In the corner, a big gun safe stands wide open with the rifle racks empty. On the wall beside a dead ham radio, a county map has backcountry trails circled toward Oakhaven Cabin.";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration($"{actionNarration}{completionNotice}");
                }
                else
                {
                    string entranceText = room405_trap_triggered
                        ? "You stumble through the scorched doorway, blinking through the ringing in your ears. The air inside tastes like burnt sulfur, oil, and smoke."
                        : "You slip past the disarmed door and ease the heavy steel shut behind you. The air inside smells strongly of machine oil, graphite, and gunpowder.";

                    stage.ShowLocationAnchor(
                        "int_405",
                        $"{entranceText}\n\n{roomAnchor}{completionNotice}",
                        $"You step back into Marcus's apartment. {(room405_trap_triggered ? "Smoke haze and spent brass still hang bitter in the dark." : "The open gun safe and reloading bench wait along the far wall.")}{completionNotice}",
                        $"Back inside Unit 405. Beside the dead ham radio, the county map with trails toward Oakhaven still hangs pinned to the wall.{completionNotice}"
                    );
                }

                if (!read_marcus_log)
                {
                    stage.SetChoice(choiceIdx++, "[NOTE] Read notepad by the radio <color=#F59E0B>[CLUE]</color> <color=#9CA3AF>(~5 min)</color>", () =>
                    {
                        read_marcus_log = true;
                        stage.Clock?.AddTime(5);
                        stage.Lore?.AddLore("Marcus's Final Log", "Marcus emptied his safe and rigged the door to stop looters. However, he was bitten on the stairwell before he could leave. Refusing to bring the infection to his family at Oakhaven Cabin, he chose to wander into the city to die alone.", stage.Clock?.GetTimeString() ?? "00:20");
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit405("<color=#F59E0B>[NOTE RECORDED]</color> You walk over to the radio desk. A grease-pencil note is pinned under the dead microphone, the handwriting shaking near the bottom:\n\n<color=#94A3B8>\"Diane... Emptied the safe and rigged the door. But on the stairs, one bit my forearm. Feels like battery acid in my veins. I won't bring this to Oakhaven. I won't bring it near you and the boys. Going outside to pull them away from the front doors. I love you.\"</color>\n\nYou set the mic down, look at the circled map coordinates for a quiet second, and step back to the middle of the room.");
                    });
                }

                if (!looted_405_bench)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Sweep reloading bench <color=#9CA3AF>(9mm Ammo x12, Scrap Metal x3 — ~5 min)</color>", () =>
                    {
                        looted_405_bench = true;
                        stage.Clock?.AddTime(5);
                        stage.GiveOrDropItem("9mm Ammo", 12);
                        stage.GiveOrDropItem("Scrap Metal", 3);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit405("<color=#F59E0B>Ammunition and salvage gathered.</color> You walk over to the heavy timber bench. Discarded sizing dies and empty brass casings clutter the surface, but a loaded plastic tray holds 12 rounds of live 9mm hollow-point ammunition alongside 3 heavy scrap metal brackets. You stow them securely into your pack, wipe the graphite dust from your hands, and walk back to the middle of the room.");
                    });
                }

                if (!looted_405_safe)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Check bottom of gun safe <color=#9CA3AF>(Hunting Knife x1 — ~3 min)</color>", () =>
                    {
                        looted_405_safe = true;
                        stage.Clock?.AddTime(3);
                        stage.GiveOrDropItem("Hunting Knife", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit405("<color=#F59E0B>Hunting knife recovered.</color> You walk to the open vault in the corner and crouch down. The rifle racks are empty, but beneath the torn felt lining at the bottom sits a high-carbon hunting knife in a rigid kydex sheath. You inspect the honed edge, slide the sheath onto your belt, and stand back up in the center of the room.");
                    });
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave apartment and return to hallway", () => EnterZone(stage, 2));
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 404 (Mrs. Gable & Buster)

        public void InspectUnit404(string actionNarration = null)
        {
            currentInspectedRoom = "404";
            stage.CurrentInspectedRoomInternal = "404";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            stage.SetLocation("hallway_2", "UNIT 404 // MRS. GABLE'S DOOR", "unit_404");
            stage.UpdateLocationDisplay("UNIT 404", "MRS. GABLE & BUSTER");
            stage.PlayStageAnimation("Hall404_405_Anim", "hall_404_405", 8f);
            stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.7f);

            string unit404Anchor = "The painted wood door of Unit 404. From inside comes the dull scrape of heavy furniture wedged against the floorboards. You can hear Buster pacing right behind the door, panting through the gap at the bottom.";

            if (!string.IsNullOrEmpty(actionNarration))
            {
                stage.SetSituationNarration(actionNarration);
            }
            else
            {
                if (spoke_to_mrs_gable)
                {
                    stage.ShowLocationAnchor(
                        "ext_404",
                        $"{unit404Anchor}\n\nThe door is still deadbolted tight. Buster lets out a soft puff of breath from the entryway rug, and a radio murmurs low inside. Mrs. Gable is staying put behind her oak dresser.",
                        "You pause outside Unit 404. Inside, Buster pants softly by the draft stopper while a radio murmurs low behind her barricaded dresser.",
                        "Unit 404's door stays wedged shut from the inside. Everything remains quiet behind Mrs. Gable's barricade."
                    );
                }
                else
                {
                    string entranceText = "Before you can even knock, sharp barking breaks out inside, followed by claws scrabbling against the wood.\n\nFrom the dark entryway, an old, shaking voice calls through the deadbolt:\n\n\"Who's out there? Stay back! Buster, heel!\"\n\nBehind the lock, the heavy groan of wood scraping across hardwood tells you she shoved a dresser against it. She's scared, and she's not opening that door for anyone.";

                    stage.ShowLocationAnchor(
                        "ext_404",
                        $"{unit404Anchor}\n\n{entranceText}",
                        "Buster starts barking again the moment your boots touch the mat outside 404. Mrs. Gable stays silent behind the deadbolt.",
                        "Wood scrapes faintly against floorboards inside 404 while the dog paces just behind the entryway."
                    );
                }
            }

            if (!spoke_to_mrs_gable)
            {
                stage.SetChoice(choiceIdx++, "[SOOTHE] Whisper through the door to Mrs. Gable <color=#10B981>[HUMANITY +5]</color> <color=#9CA3AF>(~2 min)</color>", () =>
                {
                    spoke_to_mrs_gable = true;
                    stage.Clock?.AddTime(2);
                    var player = stage.CurrentPlayer;
                    if (player != null) player.humanity = Mathf.Min(100, player.humanity + 5);
                    stage.PlayClickSFX();
                    stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/SwitchLight2.wav", 0.75f);
                    stage.UpdateVitalsUI();
                    stage.RefreshAllPDAPanels();
                    InspectUnit404("<color=#10B981>[HUMANITY +5]</color> You lean close to the doorframe, keeping your voice low and steady near the lock.\n\n\"Mrs. Gable, it's me from 407. Don't open the door. Keep that dresser wedged tight and stay away from the windows tonight.\"\n\nThe hallway goes quiet for a beat. The scratching stops, replaced by Buster's soft whimper under the draft stopper. Her shaky voice steadies behind the wood:\n\n\"Thank God it's you, dear. Be careful out there.\"\n\nYou step back into the hall and leave her door alone.");
                });
            }

            stage.SetChoice(choiceIdx++, "[BACK] Leave Mrs. Gable safe — return to hallway", () => EnterZone(stage, 2));
            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 403 (Sarah)

        public void InspectUnit403()
        {
            currentInspectedRoom = "403";
            stage.CurrentInspectedRoomInternal = "403";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            stage.SetLocation("hallway_3", "UNIT 403 // SARAH'S DOOR", "unit_403");
            stage.UpdateLocationDisplay("UNIT 403", "SARAH'S APARTMENT");
            stage.PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);
            stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.85f);

            stage.ShowLocationAnchor(
                "ext_403",
                "You try the brass handle of Unit 403. It doesn't budge—deadbolted from the outside with a clean key turn.\n\nBy the mat sits a small shoe rack with house slippers still lined up, the spot for her running shoes empty. A Swedish Medical Center call-in slip is tucked under the knocker, stamped for a 19:00 emergency shift.\n\nSarah left for the hospital hours before midnight. The local news clips on your phone showed police blocking off the streets around the downtown ERs once the waiting rooms overflowed. The peephole is pitch black, and there's no sound inside. Nobody's home.",
                "Unit 403 stays locked from the outside. Sarah's ER call-in slip still hangs tucked under the knocker.",
                "Still nothing behind Sarah's peephole—just house slippers lined neatly beside the empty spot for her shoes."
            );

            stage.SetChoice(choiceIdx++, "[BACK] Step back and return to hallway", () => EnterZone(stage, 3));
            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 402 (The Andersons)

        public void OpenUnit402Door(string method)
        {
            room402_entry_method = method;
            room402_opened = true;
            if (stage.CurrentPlayer != null) stage.CurrentPlayer.SetFlag("room402_opened", true);

            string subtitle;
            if (method == "loud")
            {
                stage.PlaySFX("Audio/SFX/elbow_smash_glass", 0.85f);
                stage.PlaySFX("Audio/SFX/firedoor_slam", 0.75f);
                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:25", "UNIT 402 // FORCED CHAIN LOCK", "Kicked the door bracket down with a loud crack. The chain tore out, echoing across the lower hallway.");
                subtitle = "The chain bracket rips from the jamb with a violent crack.\n<color=#EF4444>The door slams open, echoing down the dark corridor...</color>";
            }
            else
            {
                stage.PlayClickSFX();
                subtitle = "The steel security chain slides smoothly from its track.\n<color=#94A3B8>The door glides open softly into warm light and mournful piano music...</color>";
            }

            stage.PlayDoorTransition(
                "Videos/unit402_door_transition",
                "[ UNIT 402 // THE ANDERSONS ]",
                subtitle,
                false,
                () => InspectUnit402(),
                false
            );
        }

        public void InspectUnit402(string actionNarration = null)
        {
            currentInspectedRoom = "402";
            stage.CurrentInspectedRoomInternal = "402";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (!room402_opened)
            {
                stage.SetLocation("hallway_3", "THE CHAIN", "unit_402");
                stage.UpdateLocationDisplay("UNIT 402", "THE CHAIN");
                stage.PlayStageAnimation("Hall402_403_Anim", "hall_402_403", 8f);
                stage.PlayHubBGM("Audio/BGM/chopin", 0.45f);

                stage.ShowLocationAnchor(
                    "ext_402",
                    "The Andersons' place—the quiet family down the hall whose daughter Emma always practiced piano on Sunday mornings. The front door isn't locked. You push it open two inches before a heavy steel chain pulls tight against the frame.\n\nThrough the narrow crack, warm lamp light spills into the hall. Inside, a record player needle rides spinning black vinyl, sending a slow, sad piano piece drifting under the door. Mixing with the music is a sharp chemical smell—heavy household bleach, like someone scrubbed the whole floor tonight.",
                    "You step back up to Unit 402's cracked doorway. Soft piano notes still drift out over the sharp smell of bleach.",
                    "The heavy steel chain still holds Unit 402 two inches ajar against warm lamplight."
                );

                bool isBurglar = player != null && string.Equals(player.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasLocksmithOrGhost = player != null && (player.HasTrait("LOCKSMITH") || player.HasTrait("GHOST"));
                bool hasMultiTool = player != null && player.HasItem("Multi-Tool");
                bool hasTech1 = player != null && player.tech >= 1;
                bool hasKnife = player != null && (player.HasItem("Combat Knife") || player.HasItem("Kitchen Knife") || player.HasItem("Hunting Knife"));
                int agiVal = player != null ? player.agi : 5;
                int strVal = player != null ? player.str : 5;

                // 1. Burglar
                AddModularChoice(
                    ref choiceIdx,
                    isBurglar,
                    "[BURGLAR]",
                    "Slip the chain lock off its runner",
                    5, 1,
                    () => OpenUnit402Door("silent"),
                    $"Requires Burglar (Your Job: {player?.job ?? "Survivor"}) — You lack illicit lock knowledge"
                );

                // 2. Locksmith or Ghost
                AddModularChoice(
                    ref choiceIdx,
                    hasLocksmithOrGhost,
                    "[LOCKSMITH/GHOST]",
                    "Work the weak point in the track",
                    5, 2,
                    () => OpenUnit402Door("silent"),
                    "Requires Locksmith or Ghost trait — You lack specialized infiltration knowledge"
                );

                // 3. Tech 1 + Multi-Tool
                AddModularChoice(
                    ref choiceIdx,
                    hasTech1 && hasMultiTool,
                    "[TECH 1 + TOOL]",
                    "Unscrew the mounting bracket cleanly",
                    5, 3,
                    () => OpenUnit402Door("silent"),
                    !hasMultiTool 
                        ? "Requires Multi-Tool — You don't have tools to dismantle the bracket" 
                        : $"Requires Tech 1 (You have {player?.tech ?? 0}) — You don't know how to dismantle it safely"
                );

                // 4. Reflex/AGI 5 + Thin Blade
                AddModularChoice(
                    ref choiceIdx,
                    agiVal >= 5 && hasKnife,
                    "[REFLEX 5 + BLADE]",
                    "Guide the chain through the gap with a thin blade",
                    5, 4,
                    () => OpenUnit402Door("silent"),
                    !hasKnife 
                        ? "Requires Thin Blade — You don't have a knife equipped" 
                        : $"Requires Reflex 5 (You have {agiVal}) — Your hands aren't steady enough to work the gap"
                );

                // 5. Might 4 Clean Kick vs Fallback Gamble
                if (strVal >= 4)
                {
                    stage.SetChoice(choiceIdx++, $"[MIGHT {strVal}/4] Kick the door mount to snap the screws <color=#10B981>(-5 Stamina, 100% Guaranteed, ~1 min)</color>", () =>
                    {
                        stage.ExertStamina(5);
                        stage.Clock?.AddTime(1);
                        OpenUnit402Door("loud");
                    }, true);
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 10, 15, 35);
                    stage.SetChoice(choiceIdx++, $"[BRUTE FORCE] Throw weight against door [Might {strVal}/4 — {chance}% Chance] <color=#EF4444>(-15 Stamina, Injury Risk)</color>", () =>
                    {
                        stage.ExertStamina(15);
                        stage.Clock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            OpenUnit402Door("loud");
                        }
                        else
                        {
                            stage.PlayBeepSFX();
                            stage.TakeDamage(10, "Shoulder slam against steel chain");
                            stage.SetSituationNarration("<color=#EF4444><b>BRUTE FORCE FAILED (-10 HP, -15 Stamina).</b></color> You slammed your shoulder into the door, but the steel chain held firm. The heavy jolt bruised your shoulder and knocked the breath out of you.");
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave it and walk away", () => EnterZone(stage, 3), true);
            }
            else
            {
                stage.SetLocation("unit_402", "THE QUIET ROOM");
                stage.PlayStageAnimation("Unit402Inside_Anim", "unit402", 8f);
                stage.PlayHubBGM("Audio/BGM/chopin", 0.45f);

                string unit402Anchor = "A spotless apartment lit by warm table lamps. In the corner, the record player needle keeps spinning over black vinyl, filling the room with slow piano music. At the dining table, Mr. and Mrs. Anderson and their daughter Emma sit upright in wooden chairs, dressed in Sunday clothes, heads leaning together. Three wine glasses sit in front of them, fine white powder crusted around the rims.";

                bool all402Done = looted_402_weapon && looted_402_kitchen && looted_402_bathroom && took_402_photo && read_402_note;
                string completionNotice = all402Done
                    ? "\n\n<color=#10B981>There is nothing left to take. The Chopin nocturne continues spinning softly on the turntable. You ease the door shut behind you.</color>"
                    : "";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration($"{actionNarration}{completionNotice}");
                }
                else
                {
                    string entranceText = room402_entry_method == "loud"
                        ? "You step in past the splintered frame. The smell of bleach stings your nose and throat. There's no blood, no struggle, nothing broken. A .38 revolver lies on the wood floor beside Mr. Anderson's dress shoe."
                        : "You ease the door open and step into the quiet room. The smell of bleach stings your nose and throat. There's no blood, no struggle, nothing out of place. A .38 revolver lies on the wood floor beside Mr. Anderson's dress shoe.";

                    stage.ShowLocationAnchor(
                        "int_402",
                        $"{entranceText}\n\n{unit402Anchor}{completionNotice}",
                        $"You step back into the Andersons' quiet living room. The turntable still spins soft piano into the bleach-sharp air.{completionNotice}",
                        $"Back inside Unit 402. Warm lamps still glow over the tidy dining table and silent hall.{completionNotice}"
                    );
                }

                if (!looted_402_weapon)
                {
                    stage.SetChoice(choiceIdx++, "[TAKE] Pick up revolver from floor <color=#9CA3AF>(Revolver .38, .38 Ammo x5 — ~1 min)</color>", () =>
                    {
                        looted_402_weapon = true;
                        stage.Clock?.AddTime(1);
                        stage.GiveOrDropItem("Revolver .38", 1);
                        stage.GiveOrDropItem(".38 Ammo", 5);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#F59E0B>Sidearm acquired.</color> You walk over to the dining table and crouch beside Mr. Anderson's chair. His fingers hang limp inches above the weapon. You lift the blued-steel .38 Special revolver from the floor and check the cylinder. Five live cartridges remain in the chambers. You holster the gun at your hip, stand back up, and return to the center of the living room.");
                    });
                }

                if (!read_402_note)
                {
                    stage.SetChoice(choiceIdx++, "[NOTE] Read suicide note on table <color=#38BDF8>[+15 XP]</color> <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        read_402_note = true;
                        stage.Clock?.AddTime(1);
                        player?.GainXP(15);
                        stage.Lore?.AddLore("The Andersons' Note", "They killed themselves after hearing a rumor on shortwave radio claiming the police were locking people in and pumping gas through the vents before sunrise.", stage.Clock?.GetTimeString() ?? "00:25");
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#38BDF8>[+15 XP]</color> You step up to the dining table and pick up a folded sheet of stationery between the glasses. In neat handwriting, Mr. Anderson wrote:\n\n<color=#94A3B8>The shortwave station says the police lost control of downtown and they're shutting the vents and gassing the blocks before sunrise. We won't choke in the dark like animals. God forgive us.</color>\n\nSomebody got on an open band and started spreading panic—and they believed every word. You set the paper back down and step away from the table.");
                    });
                }

                if (!looted_402_kitchen)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Search kitchen pantry <color=#9CA3AF>(Canned Food x2, Bottled Water x1 — ~2 min)</color>", () =>
                    {
                        looted_402_kitchen = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Canned Food", 2);
                        stage.GiveOrDropItem("Bottled Water", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#F59E0B>Provisions gathered.</color> You walk into the tidy kitchen. The shelves are neatly organized with non-perishables they prepared before the panic set in. You take two cans of preserved stew and a sealed bottle of spring water, pack them into your bag, and walk back to the middle of the room.");
                    });
                }

                if (!looted_402_bathroom)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Check bathroom medicine cabinet <color=#9CA3AF>(Painkillers x2, Antiseptic x1 — ~1 min)</color>", () =>
                    {
                        looted_402_bathroom = true;
                        stage.Clock?.AddTime(1);
                        stage.GiveOrDropItem("Painkillers", 2);
                        stage.GiveOrDropItem("Antiseptic", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#F59E0B>Medical supplies recovered.</color> You walk into the tiled bathroom. In the wastebasket beneath the sink lie three emptied prescription sedative bottles. Inside the mirrored cabinet, you salvage two blister packs of painkillers and a bottle of antiseptic solution, slip them into your pouch, and return to the center of the living room.");
                    });
                }

                if (!took_402_photo)
                {
                    stage.SetChoice(choiceIdx++, "[TAKE] Take family photograph from mantel <color=#9CA3AF>(Family Photo x1 — ~1 min)</color>", () =>
                    {
                        took_402_photo = true;
                        stage.Clock?.AddTime(1);
                        stage.GiveOrDropItem("Family Photo", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#94A3B8>Family photograph taken.</color> You walk over to the fireplace mantel. A silver frame holds a photo of the three of them at a sunny lakeside cabin, laughing in windbreakers. You slide the picture from its frame and tuck it inside your jacket pocket, then step back to the center of the room.");
                    });
                }

                if (!looted_402_ring)
                {
                    stage.SetChoice(choiceIdx++, "[TAKE] Slip wedding ring from Mrs. Anderson's hand <color=#EF4444>[HUMANITY −10]</color> <color=#9CA3AF>(Gold Ring x1 — ~1 min)</color>", () =>
                    {
                        looted_402_ring = true;
                        stage.Clock?.AddTime(1);
                        if (player != null) player.humanity = Mathf.Max(0, player.humanity - 10);
                        stage.GiveOrDropItem("Gold Ring", 1);
                        stage.PlayClickSFX();
                        stage.UpdateVitalsUI();
                        stage.RefreshAllPDAPanels();
                        InspectUnit402("<color=#EF4444>[HUMANITY −10]</color> You walk over to Mrs. Anderson's chair. Her cold hand rests over her daughter's fingers. You carefully slide the gold wedding band off her ring finger—the metal feels like ice against your thumb. You drop it into your pocket with a dry throat and step away from the table.");
                    });
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave them in peace — return to hallway", () => EnterZone(stage, 3));
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Unit 401 (Party Flat)

        public void InspectUnit401(string actionNarration = null)
        {
            currentInspectedRoom = "401";
            stage.CurrentInspectedRoomInternal = "401";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (!shelf_lifted)
            {
                stage.SetLocation("unit_401", "UNIT 401 // THE FALLEN SHELF");
                stage.PlayStageAnimation("Unit401Inside_Anim", "unit401", 8f);
                if (!turned_off_music) stage.PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);

                string liftAnchor = "The door hangs wide open. Inside, loud pop music thumps out of a blown speaker. Crushed red plastic cups and spilled beer cover the hardwood floor.\n\nIn the middle of the living room, a heavy oak TV stand has pitched forward, pinning a young man face down against the boards. Deep gouge marks in the wood look like people shoved past it in a stampede for the front door. His right arm sticks out across the rug, fingers curled loosely around a set of Car Keys.";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration(actionNarration);
                }
                else
                {
                    stage.ShowLocationAnchor(
                        "ext_401",
                        liftAnchor,
                        $"You step back through Unit 401's open doorway. Crushed cups crunch underfoot around the tipped oak TV stand{(turned_off_music ? ", the room finally quiet." : " while pop music still rattles the floorboards.")}",
                        "Back in the wrecked party apartment. The young man's hand still rests on the rug beside the Car Keys."
                    );
                }

                bool isConstruction = player != null && string.Equals(player.job, "Construction Worker", StringComparison.OrdinalIgnoreCase);
                bool hasBreacherOrGrip = player != null && (player.HasTrait("BREACHER") || player.HasTrait("IRON GRIP"));
                bool hasTech2 = player != null && player.tech >= 2;
                string pryToolName = player != null && player.HasItem("Crowbar") ? "Crowbar" : (player != null && player.HasItem("Tire Iron") ? "Tire Iron" : (player != null && player.HasItem("Sledgehammer") ? "Sledgehammer" : null));
                bool hasPryTool = !string.IsNullOrEmpty(pryToolName);
                int strVal = player != null ? player.str : 5;

                // 1. Construction Worker
                AddModularChoice(
                    ref choiceIdx,
                    isConstruction,
                    "[CONSTRUCTION]",
                    "Leverage weak points with construction know-how",
                    10, 1,
                    () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(1);
                        shelf_lifted = true;
                        stage.GiveOrDropItem("Car Keys", 1);
                        stage.PlayClickSFX();
                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> Using construction know-how, you brace the center joist at its balance point and hoist the heavy entertainment unit off his chest, propping it against the wall. You gently loosen the Hyundai car keys from his cold grip and slide them into your pocket. The apartment is clear to search.");
                    },
                    $"Requires Construction Worker Job (Your Job: {player?.job ?? "Survivor"}) — You lack structural load knowledge"
                );

                // 2. Breacher / Iron Grip
                AddModularChoice(
                    ref choiceIdx,
                    hasBreacherOrGrip,
                    "[BREACHER/IRON GRIP]",
                    "Deadlift the lower frame clean",
                    10, 1,
                    () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(1);
                        shelf_lifted = true;
                        stage.GiveOrDropItem("Car Keys", 1);
                        stage.PlayClickSFX();
                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You lock your grip onto the lower frame and deadlift the oak cabinet with sheer explosive force, shoving it against the wall. You retrieve the Hyundai car keys from his fingers and stow them safely into your pocket. The apartment is clear to search.");
                    },
                    "Requires Breacher or Iron Grip Trait — You lack explosive lifting technique"
                );

                // 3. Tech 2 Knowledge
                AddModularChoice(
                    ref choiceIdx,
                    hasTech2,
                    "[TECH 2]",
                    "Dismantle bracket dowels and joint pins",
                    10, 4,
                    () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(4);
                        shelf_lifted = true;
                        stage.GiveOrDropItem("Car Keys", 1);
                        stage.PlayClickSFX();
                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You quickly loosen the connecting bracket dowels, relieving the crushing downward tension, and tilt the upper shelving safely away. You slide the Hyundai car keys from his grasp and pocket them. The apartment is clear to search.");
                    },
                    $"Requires Tech 2 (You have {player?.tech ?? 0}) — You lack knowledge of furniture joint mechanics"
                );

                // 4. Pry Tool
                AddModularChoice(
                    ref choiceIdx,
                    hasPryTool,
                    hasPryTool ? $"[TOOL: {pryToolName.ToUpper()}]" : "[TOOL: PRY BAR]",
                    hasPryTool ? $"Pry the oak frame upward using {pryToolName}" : "Pry the oak frame upward using prying tool",
                    10, 2,
                    () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(2);
                        shelf_lifted = true;
                        stage.GiveOrDropItem("Car Keys", 1);
                        stage.PlayClickSFX();
                        InspectUnit401($"<color=#F59E0B>Car keys secured.</color> You wedge the {pryToolName.ToLower()} under the oak lip, using a cinder block as a fulcrum to lever the heavy unit upward. You slide the Hyundai car keys free from his hand and pocket them. The apartment is clear to search.");
                    },
                    "Requires Crowbar, Tire Iron, or Sledgehammer — You have no leverage tool"
                );

                // 5. Might 6 vs Fallback Surge
                if (strVal >= 6)
                {
                    stage.SetChoice(choiceIdx++, $"[MIGHT {strVal}/6] Deadlift 300-lb shelf with raw muscle <color=#10B981>(-10 Stamina, 100% Guaranteed, ~1 min)</color>", () =>
                    {
                        stage.ExertStamina(10);
                        stage.Clock?.AddTime(1);
                        shelf_lifted = true;
                        stage.GiveOrDropItem("Car Keys", 1);
                        stage.PlayClickSFX();
                        InspectUnit401("<color=#F59E0B>Car keys secured.</color> You plant your boots, dig your fingers beneath the oak base, and strain with every muscle to heave the entertainment center off his torso. You take the Hyundai car keys from his hand and tuck them into your pocket. The apartment is clear to search.");
                    }, true);
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 8, 15, 45);
                    stage.SetChoice(choiceIdx++, $"[DESPERATE SURGE] Throw entire body weight into wood [Might {strVal}/6 — {chance}% Chance] <color=#EF4444>(-25 Stamina, Severe Injury Risk)</color>", () =>
                    {
                        stage.ExertStamina(25);
                        stage.Clock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            shelf_lifted = true;
                            stage.GiveOrDropItem("Car Keys", 1);
                            stage.PlayClickSFX();
                            InspectUnit401("<color=#F59E0B>Car keys secured.</color> With a heavy grunt, you get your shoulder under the wood and shove the oak unit back up against the wall. You pry the Hyundai keys out of his stiff fingers and drop them in your pocket. The rest of the place is clear to search.");
                        }
                        else
                        {
                            stage.PlayBeepSFX();
                            stage.TakeDamage(15, "Oak shelf collapse on back");
                            InspectUnit401("<color=#EF4444><b>LIFT FAILED (-15 HP, -25 Stamina).</b></color> The oak unit slipped out of your sweaty hands and crashed right back down. You wrenched your lower back and bruised your ribs.");
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave — keys not worth injury", () =>
                {
                    stage.StopHubBGM();
                    EnterZone(stage, 4);
                }, true);
            }
            else
            {
                stage.SetLocation("unit_401", "THE PARTY THAT ENDED");
                stage.PlayStageAnimation("Unit401Inside_Anim", "unit401", 8f);
                if (!turned_off_music) stage.PlayHubBGM("Audio/SFX/bestsongeversample", 0.6f);

                bool all401Done = shelf_lifted && looted_401_kitchen && looted_401_bathroom && looted_401_alcohol && looted_401_pockets && turned_off_music;
                string completionNotice = all401Done
                    ? "\n\n<color=#10B981>Nothing left to grab in here—just the dark, the crushed cups, and quiet where the stereo used to be. You head back toward the door.</color>"
                    : "";

                string musicState = turned_off_music
                    ? "The stereo is dead. With the bass gone, the room feels cramped and too quiet."
                    : "The stereo still vibrates on the table, blasting the same pop chorus over and over.";

                string unit401Anchor = $"The remains of a house party cut short. Crushed plastic cups and broken glass litter the sticky carpet, and the heavy oak TV stand sits propped against the wall beside the guy on the floor. {musicState}";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration($"{actionNarration}{completionNotice}");
                }
                else
                {
                    string entranceText = "The oak shelf is braced upright against the wall now, and the car keys are in your pocket. The room smells of stale beer, spilled soda, and sweat.";

                    stage.ShowLocationAnchor(
                        "int_401",
                        $"{entranceText}\n\n{unit401Anchor}{completionNotice}",
                        $"Back inside Unit 401. The oak TV stand leans propped against the wall{(turned_off_music ? " in dead silence." : " under thumping speakers.")}{completionNotice}",
                        $"You glance around the trashed party room again—crushed plastic cups, spilled beer, and open cupboards.{completionNotice}"
                    );
                }

                if (!looted_401_kitchen)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Raid kitchen cupboards <color=#9CA3AF>(Energy Bar x2, Bottled Water x1 — ~2 min)</color>", () =>
                    {
                        looted_401_kitchen = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Energy Bar", 2);
                        stage.GiveOrDropItem("Bottled Water", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit401("<color=#F59E0B>Kitchen scavenged.</color> You walk into the sticky kitchenette. Behind half-empty liquor bottles and party snacks, you locate two unopened energy bars and a sealed liter bottle of water shoved into the back of a cabinet. You slide them into your pack and return to the center of the living room.");
                    });
                }

                if (!looted_401_bathroom)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Check bathroom medicine cabinet <color=#9CA3AF>(Painkillers x2, Bandage x1 — ~2 min)</color>", () =>
                    {
                        looted_401_bathroom = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Painkillers", 2);
                        stage.GiveOrDropItem("Bandage", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit401("<color=#F59E0B>Medical supplies recovered.</color> You step into the bathroom and click open the mirrored cabinet. Amid hair products and party remedies, you find two blister cards of painkillers and a sterile elastic bandage roll. You stow them into your pouch and walk back to the middle of the room.");
                    });
                }

                if (!looted_401_alcohol)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Take liquor from kitchen island <color=#9CA3AF>(Bottle of Vodka x1, Whiskey x1 — ~2 min)</color>", () =>
                    {
                        looted_401_alcohol = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Bottle of Vodka", 1);
                        stage.GiveOrDropItem("Whiskey", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit401("<color=#F59E0B>High-proof alcohol secured.</color> You walk over to the kitchen island. Among the sticky red cups sit two unopened bottles: one of eighty-proof vodka and one of blended whiskey. Both work as disinfectants or improvised accelerants. You pack the heavy glass bottles securely and return to the center of the living room.");
                    });
                }

                if (!looted_401_pockets)
                {
                    stage.SetChoice(choiceIdx++, "[SEARCH] Search the fallen man's jacket <color=#9CA3AF>(Lighter x1 — ~1 min)</color>", () =>
                    {
                        looted_401_pockets = true;
                        stage.Clock?.AddTime(1);
                        stage.GiveOrDropItem("Lighter", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUnit401("<color=#F59E0B>Pocket item scavenged.</color> You crouch beside the young man and check his denim jacket pockets. You ignore a crumpled wad of paper cash—useless now—and retrieve a metal refillable lighter. You slide the lighter into your pocket, stand up, and return to the middle of the room.");
                    });
                }

                if (!turned_off_music)
                {
                    stage.SetChoice(choiceIdx++, "[MUTE] Shut off blasting stereo system <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        turned_off_music = true;
                        stage.Clock?.AddTime(1);
                        stage.StopHubBGM();
                        stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/SwitchMed1.wav", 0.8f);
                        stage.PlaySFX("Audio/SFX/static", 0.35f);
                        InspectUnit401("<color=#38BDF8>Stereo turned off.</color> You walk over to the speaker console and hit the power switch. The pop song cuts out with a dull thump, leaving the apartment dead quiet. Without the noise covering it up, the faint scratching in the hall and the sirens outside sound a lot closer. You step back toward the middle of the room.");
                    });
                }

                stage.SetChoice(choiceIdx++, "[BACK] Leave apartment and return to hallway", () =>
                {
                    stage.StopHubBGM();
                    EnterZone(stage, 4);
                });
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Room Inspection: Utility Closet (Carl's Maintenance Workshop)

        public void OpenUtilityDoor(string bypassAction = null)
        {
            utility_opened = true;
            if (stage.CurrentPlayer != null) stage.CurrentPlayer.SetFlag("utility_opened", true);
            stage.PlayClickSFX();

            stage.PlayDoorTransition(
                "Videos/utility_door_transition",
                "[ UTILITY CLOSET // MAINTENANCE ]",
                "The heavy magnetic cylinder lock disengages.\n<color=#94A3B8>The cold steel service door screeches outward, revealing utility racks and breaker boxes...</color>",
                false,
                () => InspectUtilityCloset(bypassAction),
                false
            );
        }

        public void InspectUtilityCloset(string actionNarration = null)
        {
            currentInspectedRoom = "utility";
            stage.CurrentInspectedRoomInternal = "utility";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (!utility_opened)
            {
                stage.SetLocation("hallway_4", "MAINTENANCE DOOR", "utility");
                stage.UpdateLocationDisplay("UTILITY", "MAINTENANCE DOOR");
                stage.PlayStageAnimation("HallEnd_Anim", "hall_end", 8f);
                stage.PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);

                string exteriorAnchor = "At the dead end of the hall, the worn carpet stops at bare concrete. In front of you is a heavy steel service door with faded yellow stencil paint: <b>MAINTENANCE — AUTHORIZED PERSONNEL ONLY</b>.\n\nA commercial magnetic lock holds the frame shut with a low electric hum. A strong smell of bleach and old rust—or dried blood—seeps under the rubber door sweep, while a dying fluorescent light flickers inside.";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration(actionNarration);
                }
                else
                {
                    stage.ShowLocationAnchor(
                        "ext_utility",
                        exteriorAnchor,
                        "You check Carl's heavy steel maintenance door again. The commercial mag-lock still hums against the frame.",
                        "Bleach and rust linger under the rubber door sweep while the dying fluorescent flickers inside."
                    );
                }

                bool isBurglar = player != null && string.Equals(player.job, "Burglar", StringComparison.OrdinalIgnoreCase);
                bool hasTech2 = player != null && player.tech >= 2;
                string pryTool = player != null && player.HasItem("Crowbar") ? "Crowbar" : (player != null && player.HasItem("Tire Iron") ? "Tire Iron" : (player != null && player.HasItem("Sledgehammer") ? "Sledgehammer" : null));
                bool hasPryTool = !string.IsNullOrEmpty(pryTool);
                int strVal = player != null ? player.str : 5;

                // 1. Burglar
                AddModularChoice(
                    ref choiceIdx,
                    isBurglar,
                    "[BURGLAR]",
                    "Pick manual override cylinder",
                    7, 2,
                    () =>
                    {
                        stage.ExertStamina(7);
                        stage.Clock?.AddTime(2);
                        OpenUtilityDoor("<color=#10B981>Magnetic override picked.</color> You locate the concealed mechanical cylinder recessed under the card reader housing. Working your tension wrench and pick through the industrial tumbler pins, the cylinder turns with a solid metallic clunk, cutting power to the holding magnet.");
                    },
                    $"Requires Burglar Job (Your Job: {player?.job ?? "Survivor"}) — You lack illicit lock knowledge"
                );

                // 2. Tech 2
                AddModularChoice(
                    ref choiceIdx,
                    hasTech2,
                    "[TECH 2]",
                    "Hotwire magnetic reader solenoid",
                    7, 3,
                    () =>
                    {
                        stage.ExertStamina(7);
                        stage.Clock?.AddTime(3);
                        OpenUtilityDoor("<color=#10B981>Circuit shorted.</color> You pop the reader faceplate loose, expose the twelve-volt wiring bundle, and bridge the holding solenoid circuit with copper wire. A bright spark jumps with a sharp snap, dropping the magnetic seal immediately.");
                    },
                    $"Requires Tech 2 (You have {player?.tech ?? 0}) — You lack knowledge of magnetic access systems"
                );

                // 3. Heavy Pry Tool
                AddModularChoice(
                    ref choiceIdx,
                    hasPryTool,
                    hasPryTool ? $"[TOOL: {pryTool.ToUpper()}]" : "[TOOL: HEAVY PRY]",
                    hasPryTool ? $"Pry steel door jamb with {pryTool}" : "Pry steel door jamb with heavy prying tool",
                    7, 2,
                    () =>
                    {
                        stage.ExertStamina(7);
                        stage.Clock?.AddTime(2);
                        OpenUtilityDoor($"<color=#10B981>Frame pried open.</color> You wedge the forged steel tip of the {pryTool.ToLower()} into the jamb right beside the magnetic housing. Leaning your full body weight into the leverage, the holding magnet gives way with a sharp metallic screech.");
                    },
                    "Requires Crowbar, Tire Iron, or Sledgehammer — You have no heavy pry tool"
                );

                // 4. Might 7 vs Fallback Ram
                if (strVal >= 7)
                {
                    stage.SetChoice(choiceIdx++, $"[MIGHT {strVal}/7] Violent shoulder tackle to overpower magnetic seal <color=#10B981>(-15 Stamina, 100% Guaranteed, ~1 min)</color>", () =>
                    {
                        stage.ExertStamina(15);
                        stage.Clock?.AddTime(1);
                        OpenUtilityDoor("<color=#10B981>Door forced open.</color> You brace your boots against the hallway floor and drive your shoulder squarely into the reinforced strike plate. The sudden violent blow overpowers the magnetic holding plate, throwing the steel door inward against the concrete wall.");
                    }, true);
                }
                else
                {
                    int chance = Mathf.Clamp(strVal * 3, 5, 20);
                    stage.SetChoice(choiceIdx++, $"[DESPERATE RAM] Ram industrial magnetic slab [Might {strVal}/7 — {chance}% Chance] <color=#EF4444>(-25 Stamina, Severe Injury Risk)</color>", () =>
                    {
                        stage.ExertStamina(25);
                        stage.Clock?.AddTime(3);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            OpenUtilityDoor("<color=#10B981>Door forced open.</color> Against all odds, your shoulder hits the weak spot in the old frame bracket, and the magnetic lock pops loose.");
                        }
                        else
                        {
                            stage.PlayBeepSFX();
                            stage.TakeDamage(20, "Violent collision against industrial steel door");
                            InspectUtilityCloset("<color=#EF4444><b>BRUTE FORCE FAILED (-20 HP, -25 Stamina).</b></color> You threw your whole weight into the heavy steel door, and the magnetic lock didn't budge an inch. The impact jarred your collarbone and left your shoulder throbbing. You aren't breaking a steel mag-lock door down with bare shoulders.");
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Step back into hallway", () =>
                {
                    stage.StopHubBGM();
                    EnterZone(stage, 4);
                }, true);
            }
            else
            {
                stage.SetLocation("utility", "UTILITY ROOM");
                stage.PlayStageAnimation("UtilityRoom_Anim", "utility_room", 8f);
                stage.PlayHubBGM("Audio/BGM/neon_flicker_buzz", 0.45f);

                bool allUtilityDone = read_utility_log && looted_utility_bleach && looted_utility_tape && looted_utility_flashlight;
                string completionNotice = allUtilityDone
                    ? "\n\n<color=#10B981>You've taken everything useful from the workshop. Only the bleach smell, the tipped mop bucket, and the buzzing overhead tube remain. Time to head for the stairwell.</color>"
                    : "";

                string utilityAnchor = "A cramped, windowless maintenance room smelling strongly of industrial bleach and damp concrete. A dying fluorescent bulb strobes overhead across steel supply racks. A yellow mop bucket lies tipped over in the center, and right above the back workbench, a ceiling service hatch hangs pushed aside—muddy boot scuffs lead up the shelving edge into the dark crawlspace.";

                if (!string.IsNullOrEmpty(actionNarration))
                {
                    stage.SetSituationNarration($"{actionNarration}{completionNotice}");
                }
                else
                {
                    string entranceText = "The heavy steel door sits propped behind you. Cool draft air drifts in from the hall, moving dust across the concrete floor.";

                    stage.ShowLocationAnchor(
                        "int_utility",
                        $"{entranceText}\n\n{utilityAnchor}{completionNotice}",
                        $"You step back into the cramped maintenance room. The bulb still strobes across steel racks and the open ceiling hatch.{completionNotice}",
                        $"Back inside Carl's utility closet. Tipped mop bucket, bare concrete, and a cool draft pulling down from the open hatch.{completionNotice}"
                    );
                }

                if (!read_utility_log)
                {
                    stage.SetChoice(choiceIdx++, "[NOTE] Read superintendent's logbook <color=#38BDF8>[+20 XP]</color> <color=#9CA3AF>(~2 min)</color>", () =>
                    {
                        read_utility_log = true;
                        stage.Clock?.AddTime(2);
                        player?.GainXP(20);
                        stage.Lore?.AddLore("Maintenance Log - 4th Floor", "Tenants in 402 complaining about a chemical smell again. Told them I'm not a maid. Feeling feverish today. My joints are burning. Taking some aspirin from the kit and locking myself in here to sleep it off until shift ends.", stage.Clock?.GetTimeString() ?? "00:30");
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUtilityCloset("<color=#38BDF8>Logbook inspected (+20 XP).</color> You step across the concrete to the metal desk and push aside curled building blueprints. Underneath lies Carl's spiral maintenance ledger. The last entry is scrawled at 00:15—he locked himself inside after his joints started burning with a high fever. The mag-lock stayed shut from the hall, though the open ceiling hatch above the counter shows where he must have scrambled up into the ducts. You close the ledger and step back.");
                    });
                }

                if (!looted_utility_bleach)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Search tipped cleaning cart <color=#9CA3AF>(Bleach x1, Work Gloves x1 — ~2 min)</color>", () =>
                    {
                        looted_utility_bleach = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Bleach", 1);
                        stage.GiveOrDropItem("Work Gloves", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUtilityCloset("<color=#F59E0B>Cleaning supplies scavenged.</color> You walk over to the overturned janitor cart and right the wire frame with your boot. From the lower tray, you retrieve an unopened gallon jug of concentrated industrial bleach and a pair of heavy work gloves. You stow them securely and return to the center of the workshop.");
                    });
                }

                if (!looted_utility_tape)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Scavenge metal shelves <color=#9CA3AF>(Duct Tape x1, Rope x1 — ~3 min)</color>", () =>
                    {
                        looted_utility_tape = true;
                        stage.Clock?.AddTime(3);
                        stage.GiveOrDropItem("Duct Tape", 1);
                        stage.GiveOrDropItem("Rope", 1);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUtilityCloset("<color=#F59E0B>Hardware salvaged.</color> You step past the spilled bucket to the perimeter shelving racks. Among rusted plumbing elbows and empty paint cans, you unearth a fresh roll of heavy-duty duct tape and a twelve-meter coil of braided nylon rope. You pack the supplies and walk back to the middle of the room.");
                    });
                }

                if (!looted_utility_flashlight)
                {
                    stage.SetChoice(choiceIdx++, "[LOOT] Check drawers of metal desk <color=#9CA3AF>(Flashlight x1, Batteries x2 — ~2 min)</color>", () =>
                    {
                        looted_utility_flashlight = true;
                        stage.Clock?.AddTime(2);
                        stage.GiveOrDropItem("Flashlight", 1);
                        stage.GiveOrDropItem("Batteries", 2);
                        stage.PlayClickSFX();
                        stage.RefreshAllPDAPanels();
                        InspectUtilityCloset("<color=#F59E0B>Lighting gear recovered.</color> You cross to the metal desk and pull open the sticking bottom drawer. Beneath discarded fuses and utility bills, you find a heavy aluminum LED flashlight and a sealed two-pack of fresh alkaline batteries. You test the switch briefly, slide them into your gear pocket, and step back to the center of the room.");
                    });
                }

                stage.SetChoice(choiceIdx++, "[BACK] Return to hallway", () =>
                {
                    stage.StopHubBGM();
                    EnterZone(stage, 4);
                });
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Modular CRPG Choice Helper

        private void AddModularChoice(
            ref int choiceIdx,
            bool conditionMet,
            string tag,
            string actionText,
            int staminaCost,
            int timeMinutes,
            System.Action onExecute,
            string failRequirementDesc
        )
        {
            if (stage == null) return;

            if (conditionMet)
            {
                string text = $"{tag} {actionText} <color=#10B981>(-{staminaCost} Stamina, 100% Guaranteed)</color>";
                stage.SetChoice(choiceIdx++, text, () =>
                {
                    stage.ExertStamina(staminaCost);
                    if (timeMinutes > 0) stage.Clock?.AddTime(timeMinutes);
                    onExecute?.Invoke();
                }, true);
            }
            else
            {
                string text = $"<s>{tag} {actionText}</s> <color=#64748B><i>[{failRequirementDesc}]</i></color>";
                stage.SetChoice(choiceIdx++, text, null, false);
            }
        }

        #endregion

        #region ES3 Save / Load Persistence

        public void SaveZoneState(string file)
        {
            if (string.IsNullOrEmpty(file)) return;

            ES3.Save<bool>("hub_is_in_hallway", stage != null && stage.IsInHallway, file);
            ES3.Save<int>("hub_hallway_section", hallwaySection, file);
            ES3.Save<string>("hub_inspected_room", currentInspectedRoom ?? "", file);

            // Room 406
            ES3.Save<bool>("hub_room406_opened", room406_opened, file);
            ES3.Save<int>("hub_failed_406_attempts", failed_406_attempts, file);
            ES3.Save<bool>("hub_room406_locked_out", room406_locked_out, file);
            ES3.Save<bool>("hub_studied_406_mechanics", studied_406_mechanics, file);
            ES3.Save<bool>("hub_studied_406_anatomy", studied_406_anatomy, file);
            ES3.Save<bool>("hub_read_406_journal", read_406_journal, file);
            ES3.Save<bool>("hub_looted_406_kitchen", looted_406_kitchen, file);
            ES3.Save<bool>("hub_looted_406", looted_406, file);

            // Room 405
            ES3.Save<bool>("hub_room405_opened", room405_opened, file);
            ES3.Save<bool>("hub_room405_trap_triggered", room405_trap_triggered, file);
            ES3.Save<bool>("hub_read_marcus_log", read_marcus_log, file);
            ES3.Save<bool>("hub_looted_405_bench", looted_405_bench, file);
            ES3.Save<bool>("hub_looted_405_safe", looted_405_safe, file);

            // Room 404
            ES3.Save<bool>("hub_spoke_to_mrs_gable", spoke_to_mrs_gable, file);

            // Room 402
            ES3.Save<bool>("hub_room402_opened", room402_opened, file);
            ES3.Save<string>("hub_room402_entry_method", room402_entry_method, file);
            ES3.Save<bool>("hub_looted_402_weapon", looted_402_weapon, file);
            ES3.Save<bool>("hub_looted_402_kitchen", looted_402_kitchen, file);
            ES3.Save<bool>("hub_looted_402_bathroom", looted_402_bathroom, file);
            ES3.Save<bool>("hub_looted_402_ring", looted_402_ring, file);
            ES3.Save<bool>("hub_took_402_photo", took_402_photo, file);
            ES3.Save<bool>("hub_read_402_note", read_402_note, file);

            // Room 401
            ES3.Save<bool>("hub_room401_entered", room401_entered, file);
            ES3.Save<bool>("hub_shelf_lifted", shelf_lifted, file);
            ES3.Save<bool>("hub_turned_off_music", turned_off_music, file);
            ES3.Save<bool>("hub_looted_401_kitchen", looted_401_kitchen, file);
            ES3.Save<bool>("hub_looted_401_bathroom", looted_401_bathroom, file);
            ES3.Save<bool>("hub_looted_401_alcohol", looted_401_alcohol, file);
            ES3.Save<bool>("hub_looted_401_pockets", looted_401_pockets, file);

            // Utility
            ES3.Save<bool>("hub_utility_opened", utility_opened, file);
            ES3.Save<bool>("hub_read_utility_log", read_utility_log, file);
            ES3.Save<bool>("hub_looted_utility_bleach", looted_utility_bleach, file);
            ES3.Save<bool>("hub_looted_utility_tape", looted_utility_tape, file);
            ES3.Save<bool>("hub_looted_utility_flashlight", looted_utility_flashlight, file);

        }

        public void LoadZoneState(string file)
        {
            if (string.IsNullOrEmpty(file) || !ES3.FileExists(file)) return;

            if (ES3.KeyExists("hub_hallway_section", file)) hallwaySection = ES3.Load<int>("hub_hallway_section", file);
            if (ES3.KeyExists("hub_inspected_room", file))
            {
                string r = ES3.Load<string>("hub_inspected_room", file);
                currentInspectedRoom = string.IsNullOrEmpty(r) ? null : r;
            }

            // Room 406
            if (ES3.KeyExists("hub_room406_opened", file)) room406_opened = ES3.Load<bool>("hub_room406_opened", file);
            if (ES3.KeyExists("hub_failed_406_attempts", file)) failed_406_attempts = ES3.Load<int>("hub_failed_406_attempts", file);
            if (ES3.KeyExists("hub_room406_locked_out", file)) room406_locked_out = ES3.Load<bool>("hub_room406_locked_out", file);
            if (ES3.KeyExists("hub_studied_406_mechanics", file)) studied_406_mechanics = ES3.Load<bool>("hub_studied_406_mechanics", file);
            if (ES3.KeyExists("hub_studied_406_anatomy", file)) studied_406_anatomy = ES3.Load<bool>("hub_studied_406_anatomy", file);
            if (ES3.KeyExists("hub_read_406_journal", file)) read_406_journal = ES3.Load<bool>("hub_read_406_journal", file);
            if (ES3.KeyExists("hub_looted_406_kitchen", file)) looted_406_kitchen = ES3.Load<bool>("hub_looted_406_kitchen", file);
            if (ES3.KeyExists("hub_looted_406", file)) looted_406 = ES3.Load<bool>("hub_looted_406", file);

            // Room 405
            if (ES3.KeyExists("hub_room405_opened", file)) room405_opened = ES3.Load<bool>("hub_room405_opened", file);
            if (ES3.KeyExists("hub_room405_trap_triggered", file)) room405_trap_triggered = ES3.Load<bool>("hub_room405_trap_triggered", file);
            if (ES3.KeyExists("hub_read_marcus_log", file)) read_marcus_log = ES3.Load<bool>("hub_read_marcus_log", file);
            if (ES3.KeyExists("hub_looted_405_bench", file)) looted_405_bench = ES3.Load<bool>("hub_looted_405_bench", file);
            if (ES3.KeyExists("hub_looted_405_safe", file)) looted_405_safe = ES3.Load<bool>("hub_looted_405_safe", file);

            // Room 404
            if (ES3.KeyExists("hub_spoke_to_mrs_gable", file)) spoke_to_mrs_gable = ES3.Load<bool>("hub_spoke_to_mrs_gable", file);

            // Room 402
            if (ES3.KeyExists("hub_room402_opened", file)) room402_opened = ES3.Load<bool>("hub_room402_opened", file);
            if (ES3.KeyExists("hub_room402_entry_method", file)) room402_entry_method = ES3.Load<string>("hub_room402_entry_method", file);
            if (ES3.KeyExists("hub_looted_402_weapon", file)) looted_402_weapon = ES3.Load<bool>("hub_looted_402_weapon", file);
            if (ES3.KeyExists("hub_looted_402_kitchen", file)) looted_402_kitchen = ES3.Load<bool>("hub_looted_402_kitchen", file);
            if (ES3.KeyExists("hub_looted_402_bathroom", file)) looted_402_bathroom = ES3.Load<bool>("hub_looted_402_bathroom", file);
            if (ES3.KeyExists("hub_looted_402_ring", file)) looted_402_ring = ES3.Load<bool>("hub_looted_402_ring", file);
            if (ES3.KeyExists("hub_took_402_photo", file)) took_402_photo = ES3.Load<bool>("hub_took_402_photo", file);
            if (ES3.KeyExists("hub_read_402_note", file)) read_402_note = ES3.Load<bool>("hub_read_402_note", file);

            // Room 401
            if (ES3.KeyExists("hub_room401_entered", file)) room401_entered = ES3.Load<bool>("hub_room401_entered", file);
            if (ES3.KeyExists("hub_shelf_lifted", file)) shelf_lifted = ES3.Load<bool>("hub_shelf_lifted", file);
            if (ES3.KeyExists("hub_turned_off_music", file)) turned_off_music = ES3.Load<bool>("hub_turned_off_music", file);
            if (ES3.KeyExists("hub_looted_401_kitchen", file)) looted_401_kitchen = ES3.Load<bool>("hub_looted_401_kitchen", file);
            if (ES3.KeyExists("hub_looted_401_bathroom", file)) looted_401_bathroom = ES3.Load<bool>("hub_looted_401_bathroom", file);
            if (ES3.KeyExists("hub_looted_401_alcohol", file)) looted_401_alcohol = ES3.Load<bool>("hub_looted_401_alcohol", file);
            if (ES3.KeyExists("hub_looted_401_pockets", file)) looted_401_pockets = ES3.Load<bool>("hub_looted_401_pockets", file);

            // Utility
            if (ES3.KeyExists("hub_utility_opened", file)) utility_opened = ES3.Load<bool>("hub_utility_opened", file);
            if (ES3.KeyExists("hub_read_utility_log", file)) read_utility_log = ES3.Load<bool>("hub_read_utility_log", file);
            if (ES3.KeyExists("hub_looted_utility_bleach", file)) looted_utility_bleach = ES3.Load<bool>("hub_looted_utility_bleach", file);
            if (ES3.KeyExists("hub_looted_utility_tape", file)) looted_utility_tape = ES3.Load<bool>("hub_looted_utility_tape", file);
            if (ES3.KeyExists("hub_looted_utility_flashlight", file)) looted_utility_flashlight = ES3.Load<bool>("hub_looted_utility_flashlight", file);

        }

        #endregion
    }
}
