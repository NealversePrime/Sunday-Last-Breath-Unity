using System;
using System.Collections.Generic;
using UnityEngine;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using SundayLastBreath.UI;

namespace SundayLastBreath.Zones
{
    /// <summary>
    /// Section 6: Floor 2 Hallway Zone & First Zombie Encounter (Actor / Screenplay).
    /// Performs on the GameStageScreen Stage (Panggung).
    /// 
    /// Responsibilities:
    /// - Owns the 2nd-floor corridor detour (Sections 1 through 4: East Wing Doorway, Encounter Corridor, Residential Corridor, West Fire Exit).
    /// - Owns the First Zombie Encounter (Infected Commuter / David Keller):
    ///   - Stealth Approach vs Alerted Lunging Confrontation
    ///   - Full integration with TacticalCombatModal (Limb-targeting tactical combat system)
    ///   - Procedural & Frame animations: F2Thump_Anim, F2Aggro_Anim, F2Lunge_Anim, F2Dead_Anim, F2Exit_Anim
    /// - Owns 2F room inspections & scavenging:
    ///   - David Keller's body (Apartment 203 Key, Matches, Lore entry)
    ///   - Unit 201 (Barricaded survivors, pine scent, warning note)
    ///   - Unit 202 (Sheriff's eviction notice, empty flat)
    ///   - Unit 203 (David's studio apartment - Canned food, Medical supplies, Batteries)
    ///   - West Wing Fire Exit (Direct descent to 1F Lobby)
    /// - Manages state persistence via ES3 keys for all 2F flags.
    /// </summary>
    public class Floor2HallwayZone : MonoBehaviour
    {
        [Header("Stage Reference")]
        private GameStageScreen stage;

        [Header("2F Hallway Navigation")]
        public int hallwaySection = 1;
        public string currentInspectedRoom = null;

        [Header("First Zombie Encounter (David Keller)")]
        public bool zombie_alerted = false;
        public bool zombie_defeated = false;
        public bool zombie_looted = false;
        public bool stealth_ambush_ready = false;

        [Header("2F Room Inspections")]
        public bool room201_checked = false;
        public bool room202_checked = false;
        public bool room203_unlocked = false;
        public bool room203_looted = false;
        public bool exit_stairs_inspected = false;

        public bool IsZombieDefeated => zombie_defeated;

        /// <summary>
        /// Resets all 2nd floor hallway and encounter states for a fresh game.
        /// </summary>
        public void ResetZoneState()
        {
            hallwaySection = 1;
            currentInspectedRoom = null;
            zombie_alerted = false;
            zombie_defeated = false;
            zombie_looted = false;
            stealth_ambush_ready = false;
            room201_checked = false;
            room202_checked = false;
            room203_unlocked = false;
            room203_looted = false;
            exit_stairs_inspected = false;
        }

        /// <summary>
        /// Enters the 2nd Floor Hallway Zone, synchronizes stage state, and renders the section view.
        /// </summary>
        public void EnterZone(GameStageScreen stageController, int section = 1, bool alertState = false)
        {
            stage = stageController;
            if (stage == null) return;

            if (section > 0)
            {
                hallwaySection = section;
            }

            if (alertState)
            {
                zombie_alerted = true;
            }

            currentInspectedRoom = null;
            stage.IsInHallwayInternal = true;
            stage.CurrentInspectedRoomInternal = GetCurrentSectionRoomId();

            // Prepare stage UI
            stage.SetApartmentButtonsVisible(false);
            stage.EndAwakeningIntro();

            InspectHallway();
        }

        private string GetCurrentSectionRoomId()
        {
            switch (hallwaySection)
            {
                case 1: return "f2_hall_1";
                case 2: return "f2_hall_2";
                case 3: return "f2_hall_3";
                case 4: return "f2_hall_4";
                default: return "f2_hall_1";
            }
        }

        #region 2F Screenplay & Encounter Sections

        public void InspectHallway(string actionFeedback = null)
        {
            if (stage == null) return;

            currentInspectedRoom = null;
            string roomId = GetCurrentSectionRoomId();
            stage.CurrentInspectedRoomInternal = roomId;

            // Load and synchronize tactical map for 2F
            stage.TacticalMap?.LoadFloorById("ascott_2f");
            stage.TacticalMap?.DiscoverRoom(roomId);
            stage.SyncTacticalMapRoomStates();

            stage.ClearChoices();
            int choiceIdx = 0;
            var player = stage.CurrentPlayer;
            string job = GetNormalizedJob(player);
            string calloutText = GetJobCallout(job);
            string initialThought = GetJobInitialThought(job);
            string panicReaction = GetJobPanicReaction(job);

            switch (hallwaySection)
            {
                // =========================================================================================
                // SECTION 1: EAST WING DOORWAY (Arrival from Fire Stairwell)
                // =========================================================================================
                case 1:
                    stage.SetLocation("f2_hall_1", "2ND FLOOR // EAST WING DOORWAY");
                    stage.TacticalMap?.SyncRoomState("f2_hall_1", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);

                    if (!zombie_defeated)
                    {
                        if (zombie_alerted)
                        {
                            stage.PlayStageAnimation("F2Aggro_Anim", "F2_1_aggro", 8f);
                            stage.PlaySFX("Audio/SFX/zombie_rattle", 0.65f);

                            string narration = "<color=#EF4444><b>[2ND FLOOR EAST WING // NOISE ECHOED]</b></color>\n\n" +
                                "The heavy fire door clicks shut behind you. The loud metallic crash from the stairwell carried straight down this corridor.\n\n" +
                                "Twenty feet ahead, under a single buzzing fluorescent tube light, a man in a rumpled business suit whips around with a sickening wet pop of his vertebrae. " +
                                "Through cracked wire-frame spectacles, his eyes are milky white and ruptured with blood. Thick, dark fluid dribbles from his dislocated jaw.\n\n" +
                                "A blue lanyard dangles from his collar: <b>David Keller – Junior Accountant</b>.\n\n" +
                                panicReaction + "\n\n" +
                                "<i>He is snarling—jaw snapping empty air as black bile drips onto the carpet. He lunges straight down the narrow corridor!</i>";

                            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

                            stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[DEFEND YOURSELF]</color> Raise weapon and fight off the attacker!</b>", () =>
                            {
                                hallwaySection = 2;
                                InspectHallway();
                            }, true, new Color(0.95f, 0.25f, 0.25f));

                            stage.SetChoice(choiceIdx++, "[BRACE] Plant your footing and raise your guard <color=#10B981>(-5 Stamina)</color>", () =>
                            {
                                stage.ExertStamina(5);
                                stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.8f);
                                hallwaySection = 2;
                                InspectHallway("<color=#38BDF8><b>[GUARD RAISED]</b></color> You dig your boots into the runner carpet and brace your arms. The frenzied accountant lunges into the dim light!");
                            }, true);
                        }
                        else
                        {
                            stage.PlayStageAnimation("F2Thump_Anim", "F2_1_thump", 8f);
                            stage.PlaySFX("Audio/SFX/head_thump_wet", 0.55f);

                            string narration = "<color=#10B981><b>[2ND FLOOR EAST WING // SILENT ENTRY]</b></color>\n\n" +
                                "The fire door clicks softly shut behind you. The backup generator lights in this wing are completely dead, leaving only a single fluorescent tube flickering twenty feet down the corridor.\n\n" +
                                "Ahead of you stands a man in a dark business suit, facing the wall. His forehead is pressed against the faded floral wallpaper.\n\n" +
                                "Slow, wet, rhythmic <i>thuds</i> echo down the hall as he rhythmically taps his skull against the plaster:\n" +
                                "<i>Thump... thump... thump...</i>\n\n" +
                                "A blue lanyard hanging from his pocket reads: <b>David Keller – Junior Accountant</b>. A brass key ring glints at his belt loop. He hasn't noticed you yet.\n\n" +
                                initialThought;

                            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

                            stage.SetChoice(choiceIdx++, $"<b><color=#38BDF8>[CALL OUT]</color> Speak to the stranger:</b> <color=#E2E8F0>{calloutText}</color>", () =>
                            {
                                stage.PlayClickSFX();
                                zombie_alerted = true;
                                hallwaySection = 2;
                                InspectHallway("<color=#EF4444><b>[THE REVELATION]</b></color>\n\n" +
                                    "At the sound of your voice, the rhythmic thumping abruptly stops. The corridor plunges into dead silence.\n\n" +
                                    "Slowly, with a wet, cracking pop of cartilage, the man turns his head. His jaw hangs open at an unnatural angle, black coagulated blood pooling on his tongue. " +
                                    "Behind cracked glasses, his eyes are milky cataracts swimming in broken red veins.\n\n" +
                                    panicReaction + "\n\n" +
                                    "He doesn't speak. With a choking, wet hiss, he violently lunges toward you!");
                            }, true);

                            stage.SetChoice(choiceIdx++, "[APPROACH CAUTIOUSLY] Creep closer to inspect his condition <color=#9CA3AF>(~1 min)</color>", () =>
                            {
                                stage.Clock?.AddTime(1);
                                stage.PlaySFX("Audio/SFX/boots_slow_concrete", 0.5f);
                                InspectHallway("<color=#93C5FD><b>[CLOSE INSPECTION]</b></color>\n\n" +
                                    "You take three slow, silent steps along the carpet runner. You are now six feet behind him.\n\n" +
                                    "The smell hits you first—the sharp, unmistakable copper of arterial blood mixed with a rancid, sour decay. " +
                                    "His collar is torn open, revealing deep, jagged lacerations where entire chunks of flesh have been torn from his neck.\n\n" +
                                    "His chest is not rising or falling. A wet, suffocating rattle vibrates in his throat: <i>\"...hkkk... ghh... hrrr...\"</i>\n\n" +
                                    "<i>Whatever is standing in front of you... it isn't breathing.</i>");
                            }, true);

                            stage.SetChoice(choiceIdx++, "[STEALTH CRAWL] Drop low beneath the wainscoting and slip behind him <color=#10B981>[AMBUSH ADVANTAGE]</color>", () =>
                            {
                                stealth_ambush_ready = true;
                                stage.Clock?.AddTime(1);
                                stage.PlaySFX("Audio/SFX/boots_slow_concrete", 0.6f);
                                hallwaySection = 2;
                                InspectHallway("<color=#10B981><b>[STEALTH SUCCESS]</b></color> You drop onto your knees and creep silently under the shadows of the door jambs. You slip directly behind the unresponsive tenant. His back is completely exposed to an opening strike!");
                            }, true);
                        }
                    }
                    else
                    {
                        stage.PlayStageAnimation("F2Thump_Anim", "F2_1_thump", 8f);
                        stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback :
                            "<color=#10B981><b>[2ND FLOOR EAST WING // SECURED]</b></color>\n\n" +
                            "The East Wing fire door stands behind you, wedged safely ajar with a folded cardboard coaster so it won't lock you out. " +
                            "The corridor ahead is silent. You can smell the sharp copper of fresh blood mixed with stale dust.");

                        stage.SetChoice(choiceIdx++, "[ADVANCE] Walk west toward the Residential Corridor", () =>
                        {
                            hallwaySection = 3;
                            stage.PlayClickSFX();
                            InspectHallway();
                        }, true);

                        stage.SetChoice(choiceIdx++, "[EXAMINE STAIRWELL DOOR] Check the fire door leading back to the stairs", () =>
                        {
                            stage.PlayClickSFX();
                            InspectHallway("<color=#94A3B8>The heavy fire door connects back to the stairwell landing. The stairs to the 1st Floor remain completely jammed by the drink machine and dumped furniture. Your only way down is the West Wing fire exit at the far end of this floor.</color>");
                        }, true);
                    }
                    break;

                // =========================================================================================
                // SECTION 2: MID-EAST CORRIDOR (The First Zombie Encounter)
                // =========================================================================================
                case 2:
                    stage.SetLocation("f2_hall_2", "2ND FLOOR // THE FIRST ENCOUNTER");
                    stage.TacticalMap?.SyncRoomState("f2_hall_2", isVisited: true, isChecked: true, isLocked: false, isBlocked: !zombie_defeated, isAccessible: true);

                    if (!zombie_defeated)
                    {
                        stage.PlayStageAnimation("F2Lunge_Anim", "F2_2_lunge", 8f);
                        stage.PlaySFX("Audio/SFX/zombiewalking1", 0.9f);

                        string encounterDesc = stealth_ambush_ready
                            ? "<color=#10B981><b>[TACTICAL AMBUSH OPPORTUNITY]</b></color>\n\n" +
                              "You are crouching three feet behind David Keller. His knuckles are bloody and gray from clawing the drywall, completely unaware of you.\n\n" +
                              "You have a clean opening to strike first before he turns and lunges!"
                            : "<color=#EF4444><b>[THE INFECTED COMMUTER // LUNGING THREAT]</b></color>\n\n" +
                              "David Keller staggers forward into the light! His shredded tie whips as he lunges with blackened, splintered fingernails!\n\n" +
                              panicReaction + "\n\n" +
                              "There is no way to talk to him. He does not feel fear, reason, or pain. You must neutralize this threat to survive the 2nd Floor.";

                        stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : encounterDesc);

                        if (stealth_ambush_ready)
                        {
                            stage.SetChoice(choiceIdx++, "<b><color=#10B981>[SURPRISE ATTACK]</color> Strike from behind! (Pre-Damages Torso & Legs)</b>", () =>
                            {
                                LaunchZombieCombat(isAmbush: true);
                            }, true, new Color(0.2f, 0.9f, 0.4f));
                        }

                        stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[ENGAGE]</color> Enter Tactical Combat (Target Anatomy & Defend)</b>", () =>
                        {
                            LaunchZombieCombat(isAmbush: false);
                        }, true, new Color(0.95f, 0.25f, 0.25f));

                        stage.SetChoice(choiceIdx++, "[FALL BACK] Backpedal toward the East Wing doorway <color=#EF4444>(-10 Stamina)</color>", () =>
                        {
                            stage.ExertStamina(10);
                            hallwaySection = 1;
                            zombie_alerted = true;
                            stealth_ambush_ready = false;
                            stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);
                            InspectHallway("<color=#F59E0B>You backpedal quickly into the doorway shadows, putting distance between you and the snarling tenant.</color>");
                        }, true);
                    }
                    else
                    {
                        // Defeated -> auto-advance to Section 3
                        hallwaySection = 3;
                        InspectHallway();
                    }
                    break;

                // =========================================================================================
                // SECTION 3: RESIDENTIAL CORRIDOR (Aftermath, Looting, & Apartment Doors)
                // =========================================================================================
                case 3:
                    stage.SetLocation("f2_hall_3", "2ND FLOOR // RESIDENTIAL CORRIDOR");
                    stage.TacticalMap?.SyncRoomState("f2_hall_3", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);

                    stage.PlayStageAnimation("F2Dead_Anim", "F2_4_dead", 8f);

                    string hallwayDesc = "<color=#10B981><b>[RESIDENTIAL CORRIDOR // AFTERMATH]</b></color>\n\n" +
                        "The fallen tenant lies motionless on the floral carpet runner, black fluid pooling around his head. " +
                        "The buzzing tube light casts sharp shadows across the doors of Units 201, 202, and 203.\n\n" +
                        "At the far end of the hallway, a green emergency exit sign glows above the West Wing fire stairwell.";

                    stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : hallwayDesc);

                    // 1. Search David Keller's Body
                    if (!zombie_looted)
                    {
                        stage.SetChoice(choiceIdx++, "<color=#F59E0B>[SEARCH BODY]</color> Search the fallen tenant's suit pockets <color=#9CA3AF>(~1 min)</color>", () =>
                        {
                            zombie_looted = true;
                            stage.Clock?.AddTime(1);
                            stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.85f);

                            player?.AddItem("Apartment 203 Key", 1);
                            player?.AddItem("Matches", 10);

                            stage.Lore?.AddLore("David Keller's ID Badge",
                                "Corporate ID badge for David Keller, resident of Unit 203. A worn sticky note on the back reads: 'Remember to pick up spare inhaler and battery pack from maintenance.'",
                                stage.Clock?.GetTimeString() ?? "00:45");

                            stage.RefreshAllPDAPanels();

                            InspectHallway("<color=#10B981><b>[ITEMS RECOVERED]</b></color> You kneel beside the fallen body and carefully check his jacket.\n\n" +
                                "From his breast pocket, you retrieve an Ascott Residency tenant ID: <b>David Keller, Unit 203</b>.\n" +
                                "Attached to his belt is a brass key labeled <b><color=#F59E0B>[APARTMENT 203 KEY]</color></b> alongside a box of waterproof matches.\n\n" +
                                "You slide the key into your pocket and step back to your feet.");
                        }, true);
                    }
                    else
                    {
                        stage.SetChoice(choiceIdx++, "Search David Keller's body <color=#64748B>[ALREADY SEARCHED]</color>", null, false);
                    }

                    // 2. Unit 201 (Barricaded)
                    string u201Tag = room201_checked ? "<color=#9CA3AF>[BARRICADED SURVIVORS]</color>" : "";
                    stage.SetChoice(choiceIdx++, $"CHECK UNIT 201 {u201Tag} <color=#9CA3AF>— Heavy pine fragrance, barricaded from within.</color>", () =>
                    {
                        InspectUnit201();
                    }, true);

                    // 3. Unit 202 (Evicted / Abandoned)
                    string u202Tag = room202_checked ? "<color=#9CA3AF>[ABANDONED / EVICTED]</color>" : "";
                    stage.SetChoice(choiceIdx++, $"CHECK UNIT 202 {u202Tag} <color=#9CA3AF>— Pink sheriff notice on door, pitch dark inside.</color>", () =>
                    {
                        InspectUnit202();
                    }, true);

                    // 4. Unit 203 (David Keller's Apartment)
                    string u203Tag = room203_looted ? "<color=#9CA3AF>[SEARCHED]</color>" : (room203_unlocked ? "<color=#10B981>[UNLOCKED]</color>" : "<color=#F59E0B>[LOCKED - UNIT 203]</color>");
                    stage.SetChoice(choiceIdx++, $"CHECK UNIT 203 (DAVID'S FLAT) {u203Tag} <color=#9CA3AF>— The deceased tenant's apartment.</color>", () =>
                    {
                        InspectUnit203();
                    }, true);

                    // 5. Navigation: Advance to West Fire Exit
                    stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[ADVANCE WEST]</color> Head toward the West Wing Fire Exit</b>", () =>
                    {
                        hallwaySection = 4;
                        stage.PlayClickSFX();
                        InspectHallway();
                    }, true);

                    // 6. Navigation: Retreat to East Wing
                    stage.SetChoice(choiceIdx++, "[RETREAT EAST] Walk back toward the East Wing doorway", () =>
                    {
                        hallwaySection = 1;
                        stage.PlayClickSFX();
                        InspectHallway();
                    }, true);
                    break;

                // =========================================================================================
                // SECTION 4: WEST WING FIRE EXIT (Direct Descent to 1F Lobby)
                // =========================================================================================
                case 4:
                    stage.SetLocation("f2_hall_4", "2ND FLOOR // WEST WING FIRE EXIT");
                    stage.TacticalMap?.SyncRoomState("f2_hall_4", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);

                    stage.PlayStageAnimation("F2Exit_Anim", "F2_5_exit", 8f);

                    string exitDesc = "<color=#10B981><b>[WEST WING FIRE EXIT // THE WAY DOWN]</b></color>\n\n" +
                        "You stand at the western dead-end of the 2nd Floor. Before you is a heavy industrial steel door equipped with an emergency crash bar.\n\n" +
                        "Above the lintel, an illuminated green sign reads: <b><color=#10B981>WEST FIRE STAIRS — DIRECT GROUND FLOOR ACCESS</color></b>.\n" +
                        "Through the reinforced wire-mesh glass, the concrete stairs descending to the 1st Floor are completely clear and unobstructed.";

                    stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : exitDesc);

                    // 1. Inspect window
                    if (!exit_stairs_inspected)
                    {
                        stage.SetChoice(choiceIdx++, "[INSPECT] Peer through the wire-mesh window down into the shaft <color=#9CA3AF>(~1 min)</color>", () =>
                        {
                            exit_stairs_inspected = true;
                            stage.Clock?.AddTime(1);
                            stage.PlayClickSFX();
                            InspectHallway("<color=#93C5FD><b>[STAIRS CONFIRMED CLEAR]</b></color> You cup your hands against the cold wire-mesh glass. Unlike the East Wing stairwell, no furniture has been dumped here. A single flight of steps leads straight down to the ground floor glass doors of the main lobby. Faint streetlamp reflections shine on the lobby tile below.");
                        }, true);
                    }

                    // 2. Open Fire Door and Descend to 1F
                    stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[PUSH FIRE DOOR]</color> Push through the fire exit and descend to 1F Lobby</b>", () =>
                    {
                        stage.PlayClickSFX();
                        stage.Objectives?.CompleteObjective("detour_2f");
                        stage.Objectives?.AddObjective("escape_lobby", "Cross the ground floor lobby to reach the street outside", ObjectiveStatus.Active);

                        stage.PlayDoorTransition(
                            "Videos/exit_stairs_door_transition",
                            "[ WEST FIRE STAIRS // GROUND FLOOR DESCENT ]",
                            "You push hard against the emergency crash bar.\n<color=#94A3B8>The heavy steel fire door swings smoothly open, revealing the clear flight down to the 1st Floor lobby...</color>",
                            false,
                            () =>
                            {
                                // Transition to Ground Floor / Lobby Screenplay
                                OnReachedGroundFloor();
                            },
                            true
                        );
                    }, true, new Color(0.95f, 0.25f, 0.25f));

                    // 3. Retreat back into 2F residential hall
                    stage.SetChoice(choiceIdx++, "[BACK] Return to 2F Residential Corridor", () =>
                    {
                        hallwaySection = 3;
                        stage.PlayClickSFX();
                        InspectHallway();
                    }, true);
                    break;
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Job Dialogue & Voice Helpers

        private string GetNormalizedJob(CharacterData p)
        {
            if (p == null || string.IsNullOrEmpty(p.job)) return "survivor";
            string j = p.job.ToLowerInvariant();
            if (j.Contains("police") || j.Contains("cop") || j.Contains("officer")) return "police";
            if (j.Contains("doc") || j.Contains("medic") || j.Contains("nurse") || j.Contains("surgeon")) return "doctor";
            if (j.Contains("work") || j.Contains("construct") || j.Contains("labor") || j.Contains("mechanic")) return "worker";
            if (j.Contains("thief") || j.Contains("burglar") || j.Contains("stealth")) return "thief";
            if (j.Contains("manag") || j.Contains("exec") || j.Contains("boss")) return "manager";
            if (j.Contains("stud") || j.Contains("college") || j.Contains("teen")) return "student";
            return "survivor";
        }

        private string GetJobCallout(string job)
        {
            switch (job)
            {
                case "police":
                    return "\"Hey. Seattle PD. Don't move. Turn around slowly and show me your hands.\"";
                case "doctor":
                    return "\"Excuse me? Can you hear my voice? I'm a doctor—are you injured?\"";
                case "worker":
                    return "\"Whoa, buddy, easy there. You look like you took a bad fall. You alright?\"";
                case "thief":
                    return "\"(low, tense whisper) Hey... keep it down, man. I don't want any trouble, just stay right there.\"";
                case "manager":
                    return "\"Excuse me! Mister! What are you doing out here? Do you live on this floor?\"";
                case "student":
                    return "\"U-um... hello? Excuse me? Are you... okay? Do you need help...?\"";
                default:
                    return "\"Hey... mister? Can you hear me? What happened to you?\"";
            }
        }

        private string GetJobInitialThought(string job)
        {
            switch (job)
            {
                case "police":
                    return "<color=#93C5FD><i>Your hand instinctively drops to your waist. Standard procedure runs through your mind: possible narcotics intoxication, severe head trauma, or an assault suspect hiding out in the corridor. You need compliance before you get within arms' reach.</i></color>";
                case "doctor":
                    return "<color=#93C5FD><i>Your clinical training kicks in instantly: rhythmic skull-tapping, lack of speech, rigid posture. It looks like acute neurological trauma—an intracranial hematoma, severe concussion, or late-stage encephalitis. He needs immediate medical triage.</i></color>";
                case "worker":
                    return "<color=#93C5FD><i>He's swaying like a guy who drank half a fifth of cheap whiskey and tumbled down the fire stairs. If he keeps cracking his forehead into the drywall like that, he's going to cave his skull in.</i></color>";
                case "thief":
                    return "<color=#93C5FD><i>The hairs on your arms stand up. In your line of work, anyone standing motionless in a pitch-black corridor means danger—an ambush, a trap, or a strung-out junkie who will scream and blow your cover.</i></color>";
                case "manager":
                    return "<color=#93C5FD><i>You feel a sharp spike of irritation mixed with anxiety. A resident wandering into the hallway in the dead of night, bleeding on the carpet and banging on the wall? This is completely unacceptable.</i></color>";
                case "student":
                    return "<color=#93C5FD><i>Your chest tightens and your throat feels bone dry. This doesn't look right at all. Is this a sick prank? Did someone overdose? You just want to wake up from whatever nightmare this morning has become.</i></color>";
                default:
                    return "<color=#93C5FD><i>The sight chills you. You woke up to an empty building and dead power lines, and now a total stranger is pressing his skull into the wallpaper twenty feet ahead.</i></color>";
            }
        }

        private string GetJobPanicReaction(string job)
        {
            switch (job)
            {
                case "police":
                    return "<color=#EF4444><b>\"I said BACK OFF! Back the fuck up! WHAT THE FUCK IS WRONG WITH YOU?!\"</b></color>";
                case "doctor":
                    return "<color=#EF4444><b>\"Stay back! Your skin is... wait, you're not even breathing—GET AWAY FROM ME!\"</b></color>";
                case "worker":
                    return "<color=#EF4444><b>\"Hey! Back the fuck off! What the hell is eating you?!\"</b></color>";
                case "thief":
                    return "<color=#EF4444><b>\"Shit! What the fuck—you're trying to bite my throat off?!\"</b></color>";
                case "manager":
                    return "<color=#EF4444><b>\"Get away from me! Stop! WHAT IS WRONG WITH YOU PEOPLE?!\"</b></color>";
                case "student":
                    return "<color=#EF4444><b>\"Oh god, oh god, he's not stopping—HE'S TRYING TO BITE ME!\"</b></color>";
                default:
                    return "<color=#EF4444><b>\"Stay back! GET AWAY FROM ME! What is wrong with you?!\"</b></color>";
            }
        }

        #endregion

        #region First Zombie Tactical Combat Integration

        private void LaunchZombieCombat(bool isAmbush)
        {
            if (stage == null) return;

            stage.PlayClickSFX();

            // Create preset enemy for David Keller
            var zombie = TacticalCombatModal.CreatePresetEnemy(
                id: "zombie_commuter",
                name: "Infected Commuter (David Keller)",
                weapon: "Splintered Nails & Snapping Jaw",
                dmg: 14,
                acc: 65,
                ev: 10,
                desc: "A freshly turned resident in a torn suit. Lunges wildly with blackened nails and disjointed jaws.",
                xpReward: 35
            );

            if (isAmbush)
            {
                // Ambush bonus: Torso and Leg pre-damaged from sneak attack!
                if (zombie.limbs.ContainsKey(TacticalCombatModal.LimbType.Torso))
                {
                    zombie.limbs[TacticalCombatModal.LimbType.Torso].hp = Mathf.Max(10, zombie.limbs[TacticalCombatModal.LimbType.Torso].hp - 25);
                }
                if (zombie.limbs.ContainsKey(TacticalCombatModal.LimbType.LeftLeg))
                {
                    zombie.limbs[TacticalCombatModal.LimbType.LeftLeg].hp = Mathf.Max(5, zombie.limbs[TacticalCombatModal.LimbType.LeftLeg].hp - 15);
                }
            }

            var enemyList = new List<TacticalCombatModal.CombatEnemyData> { zombie };

            stage.CombatModal.StartEncounter(
                playerData: stage.CurrentPlayer,
                enemies: enemyList,
                onVictory: () =>
                {
                    zombie_defeated = true;
                    hallwaySection = 3;

                    stage.PlaySFX("Audio/SFX/blood_drip", 0.75f);
                    stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:40", "2ND FLOOR // FIRST INFECTED SLAIN",
                        "Neutralized David Keller in close-quarters tactical combat. The reality of the infection has set in.");

                    stage.Objectives?.CompleteObjective("detour_2f");
                    stage.Objectives?.AddObjective("reach_lobby", "Proceed to the West Wing fire exit and descend to the ground floor", ObjectiveStatus.Active);

                    InspectHallway("<color=#10B981><b>[THREAT ELIMINATED]</b></color>\n\nWith a final convulsive twitch, David Keller collapses face-first into the carpet runner and lies completely still.\n\n" +
                        "Your breath comes in ragged, burning gasps. The corridor falls into heavy silence, broken only by the buzz of the failing tube light.\n\n" +
                        "You stand over his body with your pulse hammering in your ears. <i>He wasn't drunk. He wasn't having a seizure. He was dead before he ever lunged at you.</i>");
                },
                onDefeat: () =>
                {
                    stage.SetSituationNarration("<color=#EF4444><b>[TACTICAL DEFEAT // CORRIDOR OVERRUN]</b></color>\n\n" +
                        "David Keller's frenzied assault knocked you onto your back! With a desperate kick to his chest, you shove him away and scramble backward through the fire door.\n\n" +
                        "<i>Tactical Survival Tip: The attacker does not feel pain. Target his LEGS to force a Crawler and destroy his mobility, or GUARD to cut his damage in half!</i>");
                    hallwaySection = 1;
                    zombie_alerted = true;
                    stealth_ambush_ready = false;
                    stage.ExertStamina(10);
                    InspectHallway();
                },
                onFlee: () =>
                {
                    hallwaySection = 1;
                    zombie_alerted = true;
                    stealth_ambush_ready = false;
                    stage.ExertStamina(15);
                    InspectHallway("<color=#F59E0B>You scramble backwards onto your feet and flee to the East Wing doorway, your chest heaving.</color>");
                },
                isTutorial: true
            );
        }

        #endregion

        #region Room Inspections: Units 201, 202, 203

        public void InspectUnit201()
        {
            if (stage == null) return;
            currentInspectedRoom = "201";
            stage.CurrentInspectedRoomInternal = "201";
            stage.ClearChoices();
            int choiceIdx = 0;

            room201_checked = true;
            stage.PlayClickSFX();

            stage.SetLocation("f2_hall_3", "UNIT 201 // BARRICADED FLAT");
            stage.UpdateLocationDisplay("UNIT 201", "BARRICADED RESIDENCE");

            string narration = "You approach the door of Unit 201. A thick scent of pine cleaner and melted beeswax seeps from under the draft stopper.\n\n" +
                "Heavy 3-inch construction screws have been driven through the door frame, locking it securely to the studs. " +
                "Taped over the peephole is a cardboard flap with handwritten red marker:\n\n" +
                "<color=#EF4444><i>\"WE HAVE 12-GAUGE BUCKSHOT AND THREE MONTHS OF WATER. DO NOT KNOCK. DO NOT POUND. WE WILL FIRE THROUGH THE PLASTER.\"</i></color>\n\n" +
                "You listen closely. From deep inside the flat, you can hear a battery-powered radio playing low emergency static. Whoever lives here prepared early and isn't opening for anyone.";

            stage.SetSituationNarration(narration);

            stage.SetChoice(choiceIdx++, "[LEAVE ALONE] Step back and leave the barricaded residents in peace", () =>
            {
                InspectHallway();
            }, true);

            stage.ReindexChoices();
        }

        public void InspectUnit202()
        {
            if (stage == null) return;
            currentInspectedRoom = "202";
            stage.CurrentInspectedRoomInternal = "202";
            stage.ClearChoices();
            int choiceIdx = 0;

            room202_checked = true;
            stage.PlayClickSFX();

            stage.SetLocation("f2_hall_3", "UNIT 202 // VACANT FLAT");
            stage.UpdateLocationDisplay("UNIT 202", "ABANDONED UNIT");

            string narration = "You examine the door of Unit 202. A bright pink legal eviction notice from the King County Sheriff's Office is taped squarely over the deadbolt, dated two weeks ago.\n\n" +
                "The lock cylinder has been drilled out and replaced with a heavy brass padlocked hasp. Looking through the peephole hole, the apartment is completely stripped—bare floorboards, exposed ceiling wires, and clean concrete counters.\n\n" +
                "Nobody is inside, and there is nothing of value left to scavenge.";

            stage.SetSituationNarration(narration);

            stage.SetChoice(choiceIdx++, "[BACK] Return to the corridor", () =>
            {
                InspectHallway();
            }, true);

            stage.ReindexChoices();
        }

        public void InspectUnit203()
        {
            if (stage == null) return;
            currentInspectedRoom = "203";
            stage.CurrentInspectedRoomInternal = "203";
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;
            bool hasKey = player != null && player.HasItem("Apartment 203 Key");

            stage.SetLocation("f2_hall_3", "UNIT 203 // DAVID KELLER'S FLAT");
            stage.UpdateLocationDisplay("UNIT 203", "DAVID KELLER'S APARTMENT");

            if (!room203_unlocked)
            {
                string lockedNarration = "You stand in front of Unit 203, David Keller's apartment. The door is secured with a clean single-cylinder deadbolt.\n\n" +
                    "A dry-erase whiteboard hung beside the doorbell reads: <i>\"Back from downtown at 6:30. Water is shut off again—boiler repair in basement.\"</i>";

                stage.SetSituationNarration(lockedNarration);

                if (hasKey)
                {
                    stage.SetChoice(choiceIdx++, "<b><color=#F59E0B>[USE KEY: 203 KEY]</color> Unlock David Keller's front deadbolt</b>", () =>
                    {
                        room203_unlocked = true;
                        stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.9f);
                        stage.Clock?.AddTime(1);
                        InspectUnit203();
                    }, true, new Color(0.95f, 0.75f, 0.25f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "Try the door handle <color=#EF4444>[LOCKED — REQUIRES APARTMENT 203 KEY]</color>", () =>
                    {
                        stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.8f);
                        stage.SetSituationNarration("You rattle the brass handle, but the steel deadbolt holds firm. The key must still be on David Keller's body out in the corridor.");
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[BACK] Return to the corridor", () =>
                {
                    InspectHallway();
                }, true);
            }
            else
            {
                // Interior of Unit 203
                string insideNarration = "You step into Unit 203 and gently close the door behind you. It's a modest studio apartment with a galley kitchenette and a neat work desk.\n\n" +
                    "An empty briefcase sits open on the bed. An unplugged laptop sits on the desk beside an untouched glass of tap water.\n\n" +
                    (room203_looted
                        ? "You have already gathered all usable survival rations and medical supplies from this apartment."
                        : "The kitchen pantry and bathroom medicine cabinet look undisturbed. You can scavenge for emergency rations.");

                stage.SetSituationNarration(insideNarration);

                if (!room203_looted)
                {
                    stage.SetChoice(choiceIdx++, "<color=#10B981>[SCAVENGE APARTMENT]</color> Search the kitchenette pantry & medicine cabinet <color=#9CA3AF>(~2 min)</color>", () =>
                    {
                        room203_looted = true;
                        stage.Clock?.AddTime(2);
                        stage.PlaySFX("Audio/SFX/flashlight_click", 0.85f);

                        player?.AddItem("Canned Beans", 1);
                        player?.AddItem("Bandage", 2);
                        player?.AddItem("Battery", 2);

                        stage.RefreshAllPDAPanels();

                        InspectUnit203();
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "[LEAVE FLAT] Step back out into the 2nd Floor hallway", () =>
                {
                    InspectHallway();
                }, true);
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Descent to Ground Floor Lobby

        private void OnReachedGroundFloor()
        {
            if (stage == null) return;

            stage.IsInHallwayInternal = false;
            stage.CurrentInspectedRoomInternal = "lobby";

            // Update Tactical Map to Lobby
            stage.TacticalMap?.LoadFloorById("ascott_lobby");
            stage.SetLocation("lobby_entrance", "ASCOTT RESIDENCY // GROUND FLOOR LOBBY");

            stage.PlayStageAnimation("Lobby_Main", "Lobby_Main", 8f);
            stage.PlayHubBGM("Audio/SFX/generator_hum", 0.35f, true);

            stage.SetSituationNarration("<color=#10B981><b>[GROUND FLOOR LOBBY // ARRIVAL]</b></color>\n\n" +
                "You emerge from the West Wing fire stairwell into the expansive ground floor lobby of the Ascott Residency.\n\n" +
                "Rain lashes against the two-story plate glass windows at the front entrance. Through the glass, flashing red and blue emergency strobes illuminate Seattle's rain-slicked pavement. " +
                "Abandoned cars line the street with doors hanging open.\n\n" +
                "Across the lobby floor, past overturned reception counters and smashed luggage carts, the revolving glass doors lead out into the city.");

            stage.ClearChoices();
            int choiceIdx = 0;

            stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[RECON]</color> Survey the lobby reception desk and main entrance</b>", () =>
            {
                stage.PlayClickSFX();
                stage.SetSituationNarration("You crouch behind a marble pillar and scan the shadows. The front lobby is eerily quiet, but shadows shift near the glass revolving doors. You need to prepare before making a run for the outside.");
            }, true);

            stage.ReindexChoices();
        }

        #endregion

        #region ES3 Save / Load Persistence

        public void SaveZoneState(string file)
        {
            if (string.IsNullOrEmpty(file)) return;

            ES3.Save<int>("hub_f2_section", hallwaySection, file);
            ES3.Save<bool>("hub_f2_zombie_alerted", zombie_alerted, file);
            ES3.Save<bool>("hub_f2_zombie_defeated", zombie_defeated, file);
            ES3.Save<bool>("hub_f2_zombie_looted", zombie_looted, file);
            ES3.Save<bool>("hub_f2_stealth_ready", stealth_ambush_ready, file);
            ES3.Save<bool>("hub_f2_room201_checked", room201_checked, file);
            ES3.Save<bool>("hub_f2_room202_checked", room202_checked, file);
            ES3.Save<bool>("hub_f2_room203_unlocked", room203_unlocked, file);
            ES3.Save<bool>("hub_f2_room203_looted", room203_looted, file);
            ES3.Save<bool>("hub_f2_exit_inspected", exit_stairs_inspected, file);
            ES3.Save<string>("hub_f2_current_room", currentInspectedRoom, file);
        }

        public void LoadZoneState(string file)
        {
            if (string.IsNullOrEmpty(file) || !ES3.FileExists(file)) return;

            if (ES3.KeyExists("hub_f2_section", file)) hallwaySection = ES3.Load<int>("hub_f2_section", file);
            if (ES3.KeyExists("hub_f2_zombie_alerted", file)) zombie_alerted = ES3.Load<bool>("hub_f2_zombie_alerted", file);
            if (ES3.KeyExists("hub_f2_zombie_defeated", file)) zombie_defeated = ES3.Load<bool>("hub_f2_zombie_defeated", file);
            if (ES3.KeyExists("hub_f2_zombie_looted", file)) zombie_looted = ES3.Load<bool>("hub_f2_zombie_looted", file);
            if (ES3.KeyExists("hub_f2_stealth_ready", file)) stealth_ambush_ready = ES3.Load<bool>("hub_f2_stealth_ready", file);
            if (ES3.KeyExists("hub_f2_room201_checked", file)) room201_checked = ES3.Load<bool>("hub_f2_room201_checked", file);
            if (ES3.KeyExists("hub_f2_room202_checked", file)) room202_checked = ES3.Load<bool>("hub_f2_room202_checked", file);
            if (ES3.KeyExists("hub_f2_room203_unlocked", file)) room203_unlocked = ES3.Load<bool>("hub_f2_room203_unlocked", file);
            if (ES3.KeyExists("hub_f2_room203_looted", file)) room203_looted = ES3.Load<bool>("hub_f2_room203_looted", file);
            if (ES3.KeyExists("hub_f2_exit_inspected", file)) exit_stairs_inspected = ES3.Load<bool>("hub_f2_exit_inspected", file);
            if (ES3.KeyExists("hub_f2_current_room", file)) currentInspectedRoom = ES3.Load<string>("hub_f2_current_room", file);
        }

        #endregion
    }
}
