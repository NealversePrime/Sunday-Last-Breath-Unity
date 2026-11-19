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
    /// Performs on the GameStageScreen Stage.
    /// 
    /// Design:
    /// - 100% pure-hallway corridor gauntlet (Sections 1 through 4: East Wing Doorway, Encounter Corridor,
    ///   Residential Corridor, West Wing Fire Exit) with no enterable apartment rooms.
    /// - First Zombie Encounter (Infected Commuter / David Keller):
    ///   - The protagonist still believes the crisis is a riot/blackout and has no prior knowledge of zombies.
    ///   - Silent entry vs. stairwell-noise alerted entry leads into an unavoidable self-defense confrontation
    ///     that shatters denial and reveals the undead outbreak.
    ///   - Restores the pre-combat Lunge Reflex Check (STR 4 Shove / AGI 5 Dive, eased when prepared)
    ///     and the pinned-to-floor Tackle state (F2Tackle_Anim) before launching TacticalCombatModal.
    /// - Pure-hallway aftermath & scavenging:
    ///   - Searching David Keller's body & spilled commuter briefcase in the corridor (ID badge sticky-note clue).
    ///   - Testing sealed/barricaded apartment doors (Units 201–203) from the corridor only.
    ///   - West Exit corridor scavenging: Janitor's Maintenance Cart (PER 5 or ID Badge clue -> Inhaler, Battery, Bandage)
    ///     and Discarded Luggage (Scavenging 1 / Scavenger / Field Forager -> Energy Bar, Bottled Water).
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

        [Header("2F Pure-Hallway Encounter & Scavenging States")]
        public bool zombie_inspected = false;
        public bool prepared_for_lunge = false;
        public bool lunge_countered = false;
        public bool zombie_tackled = false;
        public bool lunge_resolved = false;
        public bool hallway_doors_checked = false;
        public bool janitor_cart_looted = false;
        public bool luggage_looted = false;

        [Header("2F Door Exterior Checks & Compatibility Flags")]
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

            zombie_inspected = false;
            prepared_for_lunge = false;
            lunge_countered = false;
            zombie_tackled = false;
            lunge_resolved = false;
            hallway_doors_checked = false;
            janitor_cart_looted = false;
            luggage_looted = false;

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
                hallwaySection = Mathf.Clamp(section, 1, 4);
            }

            if (alertState)
            {
                zombie_alerted = true;
            }

            currentInspectedRoom = null;
            stage.IsInHallwayInternal = true;
            stage.CurrentInspectedRoomInternal = GetCurrentSectionRoomId();

            stage.SetApartmentButtonsVisible(false);
            stage.EndAwakeningIntro();
            stage.PlaySFX("Audio/SFX/firedoor_click", 0.8f);
            stage.PlayHubBGM("Audio/SFX/fluorescent_buzz", 0.30f, true);

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

        #region 2F Screenplay & Corridor Sections

        public void InspectHallway(string actionFeedback = null)
        {
            if (stage == null) return;

            currentInspectedRoom = null;
            string roomId = GetCurrentSectionRoomId();
            stage.CurrentInspectedRoomInternal = roomId;

            stage.TacticalMap?.LoadFloorById("ascott_2f");
            stage.TacticalMap?.DiscoverRoom(roomId);
            stage.SyncTacticalMapRoomStates();

            stage.ClearChoices();
            int choiceIdx = 0;
            var player = stage.CurrentPlayer;

            switch (hallwaySection)
            {
                case 1:
                    RenderSection1EastWing(player, ref choiceIdx, actionFeedback);
                    break;
                case 2:
                    RenderSection2Encounter(player, ref choiceIdx, actionFeedback);
                    break;
                case 3:
                    RenderSection3CorridorAftermath(ref choiceIdx, actionFeedback);
                    break;
                case 4:
                    RenderSection4WestExit(player, ref choiceIdx, actionFeedback);
                    break;
            }

            stage.ReindexChoices();
        }

        private void RenderSection1EastWing(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("f2_hall_1", "2ND FLOOR // EAST WING DOORWAY");
            stage.TacticalMap?.SyncRoomState("f2_hall_1", isVisited: true, isChecked: true, isLocked: false, isBlocked: false, isAccessible: true);

            if (!zombie_defeated)
            {
                string job = GetNormalizedJob(player);
                string calloutText = GetJobCallout(job);
                string initialThought = GetJobInitialThought(job);
                string panicReaction = GetJobPanicReaction(job);

                if (zombie_alerted)
                {
                    stage.PlayStageAnimation("F2Aggro_Anim", "F2_1_aggro", 8f);
                    stage.PlaySFX("Audio/SFX/zombie_rattle", 0.65f);

                    string loudNarration = "<color=#EF4444><b>[2ND FLOOR EAST WING // NOISE ECHOED]</b></color>\n\n" +
                        "The heavy fire door clicks shut behind you. The loud metallic crash from the stairwell carried straight down this corridor.\n\n" +
                        "Twenty feet ahead, under a single buzzing fluorescent tube light, a man in a rumpled business suit whips around with a sickening wet pop of his vertebrae. " +
                        "Through cracked wire-frame spectacles, his eyes are milky white and ruptured with blood. Thick, dark fluid dribbles from his dislocated, half-torn jaw.\n\n" +
                        "A blue lanyard dangles from his collar: <b>David Keller – Junior Accountant</b>.\n\n" +
                        panicReaction + "\n\n" +
                        "<i>He isn't listening to reason—he lets out a wet, gurgling shriek and sprints straight down the narrow corridor!</i>";

                    stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : loudNarration);

                    stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[REACT TO CHARGE]</color> Meet his lunge in the corridor!</b>", () =>
                    {
                        stage.PlayClickSFX();
                        hallwaySection = 2;
                        InspectHallway();
                    }, true, new Color(0.95f, 0.25f, 0.25f));

                    stage.SetChoice(choiceIdx++, "[BRACE FOOTING] Dig your boots in and prepare for impact <color=#10B981>(-5 Stamina)</color>", () =>
                    {
                        stage.ExertStamina(5);
                        stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.7f);
                        prepared_for_lunge = true;
                        hallwaySection = 2;
                        InspectHallway("<color=#38BDF8><b>[FOOTING PLANTED]</b></color> You plant your boots firmly into the worn runner carpet and square your shoulders as the frenzied accountant charges down the corridor!");
                    }, true);
                }
                else
                {
                    stage.PlayStageAnimation("F2Thump_Anim", "F2_1_thump", 8f);
                    stage.PlaySFX("Audio/SFX/head_thump_wet", 0.55f);

                    string silentNarration = "<color=#10B981><b>[2ND FLOOR EAST WING // SILENT ENTRY]</b></color>\n\n" +
                        "The heavy fire door clicks softly shut behind you. The emergency backup lights in this wing are dead, leaving only a single fluorescent tube flickering twenty feet down the corridor.\n\n" +
                        "Ahead of you stands a man in a dark business suit, facing the wall. His forehead is pressed against the faded floral wallpaper, and a leather commuter briefcase lies spilled open at his heels.\n\n" +
                        "Slow, wet, rhythmic <i>thumps</i> echo down the hall as he taps his skull against the plaster:\n" +
                        "<i>Thump... thump... thump...</i>\n\n" +
                        "A blue lanyard hanging from his pocket reads: <b>David Keller – Junior Accountant</b>. It's probably just panic, shock, or bad drugs from the riots outside—he hasn't noticed you yet.\n\n" +
                        initialThought;

                    stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : silentNarration);

                    stage.SetChoice(choiceIdx++, $"<b><color=#38BDF8>[CALL OUT]</color> Speak to the stranger:</b> <color=#E2E8F0>{calloutText}</color>", () =>
                    {
                        stage.PlaySFX("Audio/SFX/zombie_rattle", 0.8f);
                        stage.PlaySFX("Audio/SFX/bone_crack", 0.9f);
                        zombie_alerted = true;
                        hallwaySection = 2;
                        InspectHallway("<color=#EF4444><b>[THE REVELATION]</b></color>\n\n" +
                            "At the sound of your voice, the rhythmic thumping abruptly stops. The corridor plunges into dead silence.\n\n" +
                            "Slowly, with a wet, cracking pop of neck cartilage, the man turns around. His lower jaw hangs torn and slack at an unnatural angle, black coagulated blood pooling on his collar. " +
                            "Behind cracked glasses, his eyes are milky cataracts swimming in broken red veins.\n\n" +
                            panicReaction + "\n\n" +
                            "He doesn't speak. With a choking, wet hiss, he violently lunges straight down the corridor at you!");
                    }, true);

                    if (!zombie_inspected)
                    {
                        stage.SetChoice(choiceIdx++, "[OBSERVE CLOSELY] Creep a few paces closer to inspect his condition <color=#9CA3AF>(~1 min)</color>", () =>
                        {
                            zombie_inspected = true;
                            prepared_for_lunge = true;
                            stage.Clock?.AddTime(1);
                            stage.PlaySFX("Audio/SFX/boots_slow_concrete", 0.5f);
                            InspectHallway("<color=#93C5FD><b>[CLOSE INSPECTION // ALERTED NERVES]</b></color>\n\n" +
                                "You take three slow, silent steps along the carpet runner, stopping six feet behind him.\n\n" +
                                "The smell hits you first—the sharp, unmistakable copper of arterial blood mixed with rancid, sour decay. " +
                                "His collar is torn open, revealing deep, jagged bite lacerations where entire chunks of flesh have been ripped from his neck.\n\n" +
                                "His chest is not rising or falling. A wet, suffocating rattle vibrates in his throat: <i>\"...hkkk... ghh... hrrr...\"</i>\n\n" +
                                "<i>Whatever is standing in front of you... it isn't breathing. Every muscle in your body locks tight, braced for him to turn.</i>");
                        }, true);
                    }

                    stage.SetChoice(choiceIdx++, "[TRY TO SNEAK PAST] Edge silently along the opposite wall toward the West Exit <color=#10B981>[ALERT ADVANTAGE]</color> <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        prepared_for_lunge = true;
                        zombie_alerted = true;
                        stage.Clock?.AddTime(1);
                        stage.PlaySFX("Audio/SFX/floorboard_groan", 0.7f);
                        stage.PlaySFX("Audio/SFX/bone_crack", 0.85f);
                        hallwaySection = 2;
                        InspectHallway("<color=#F59E0B><b>[CORRIDOR CREAK // LUNGE TRIGGERED]</b></color>\n\n" +
                            "You press your back flat against the opposite drywall and try to slip past this unstable stranger without starting a confrontation.\n\n" +
                            "Halfway past him, a loose floorboard groans sharply beneath your heel. The rhythmic thumping stops instantly. " +
                            "With a sickening crack of cartilage, David Keller whips around—milk-white eyes, half-torn jaw—and lunges straight at you!\n\n" +
                            "Because you were watching him carefully as you crept past, you catch his movement the split-second he pivots!");
                    }, true);
                }
            }
            else
            {
                stage.PlayStageAnimation("F2Thump_Anim", "F2_1_thump", 8f);
                stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback :
                    "<color=#10B981><b>[2ND FLOOR EAST WING // SECURED]</b></color>\n\n" +
                    "The East Wing fire door stands behind you, wedged ajar so it won't lock you in. " +
                    "The narrow corridor ahead smells sharply of copper and stale carpet dust.");

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
        }

        private void RenderSection2Encounter(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("f2_hall_2", "2ND FLOOR // THE FIRST ENCOUNTER");
            stage.TacticalMap?.SyncRoomState("f2_hall_2", isVisited: true, isChecked: zombie_defeated, isLocked: false, isBlocked: false, isAccessible: true);

            if (zombie_defeated)
            {
                hallwaySection = 3;
                InspectHallway(actionFeedback);
                return;
            }

            int strVal = player != null ? player.str : 3;
            int agiVal = player != null ? player.agi : 3;

            // State 2B: Pinned on the corridor floor (F2Tackle_Anim)
            if (zombie_tackled && !lunge_resolved)
            {
                stage.PlayStageAnimation("F2Tackle_Anim", "F2_3_tackle", 8f);
                stage.PlaySFX("Audio/SFX/zombie_snapping", 0.9f);

                string tackleNarration = "<color=#EF4444><b>[TACKLED TO THE FLOOR // -15 HP]</b></color>\n\n" +
                    "He hits you like a freight train! The sheer, unhinged force knocks the wind out of you as you crash backward onto the worn linoleum.\n\n" +
                    "Hot, putrid breath washes over your face. David Keller is thrashing wildly on top of you, snapping his ragged upper teeth inches from your throat as black blood drips down onto your collar. " +
                    "His dead weight is crushing the air out of your lungs—you have to throw him off right now!";

                stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : tackleNarration);

                bool canKickFree = strVal >= 3 || agiVal >= 3;
                if (canKickFree)
                {
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[STR {strVal}/3 OR AGI {agiVal}/3]</color> Wedge your knee against his chest and kick him off!</b> <color=#10B981>(-5 Stamina)</color>", () =>
                    {
                        stage.ExertStamina(5);
                        stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);
                        lunge_resolved = true;
                        lunge_countered = false;
                        stealth_ambush_ready = false;
                        InspectHallway("<color=#10B981><b>[KICKED FREE]</b></color> You wedge your knee against his chest and deliver a desperate, powerful kick. He goes sliding backward across the carpet runner, leaving a smear of dark fluid. You scramble back to your feet!");
                    }, true, new Color(0.2f, 0.9f, 0.4f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[STR 3 OR AGI 3] Wedge knee and kick him off</s> <color=#64748B><i>[Requires Might 3 or Reflex 3 — His dead weight pins you down]</i></color>", null, false);
                }

                stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[DESPERATELY STRUGGLE]</color> Heave him off in a surge of adrenaline!</b> <color=#EF4444>(-10 HP Scratched, -10 Stamina)</color>", () =>
                {
                    stage.TakeDamage(10, "Clawed by David Keller while pinned on the 2nd Floor");
                    stage.ExertStamina(10);
                    stage.PlaySFX("Audio/SFX/blood_drip", 0.75f);
                    lunge_resolved = true;
                    lunge_countered = false;
                    stealth_ambush_ready = false;
                    InspectHallway("<color=#EF4444><b>[DESPERATE ESCAPE (-10 HP, -10 STAMINA)]</b></color> You struggle, but he's too heavy! His teeth and jagged nails rake across your collarbone, tearing flesh! " +
                        "The blinding pain sends a jolt of pure adrenaline straight into your heart—with a primal shout, you violently heave him off your chest and scramble back to your feet!");
                }, true, new Color(0.95f, 0.25f, 0.25f));

                return;
            }

            stage.PlayStageAnimation("F2Lunge_Anim", "F2_2_lunge", 8f);

            // State 2A: Pre-combat Lunge Reflex Check
            if (!lunge_resolved)
            {
                stage.PlaySFX("Audio/SFX/zombie_lunge", 0.85f);

                string panicReaction = GetJobPanicReaction(GetNormalizedJob(player));
                string lungeNarration = "<color=#EF4444><b>[THE INFECTED COMMUTER // NARROW CORRIDOR LUNGE]</b></color>\n\n" +
                    "David Keller launches himself down the six-foot-wide corridor! His shredded tie whips through the fluorescent flicker as he lunges with blackened, splintered fingernails!\n\n" +
                    panicReaction + "\n\n" +
                    "There are no open apartment doors to duck into—only bare walls and his snapping jaw coming straight at your throat.";

                stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : lungeNarration);

                int reqStr = prepared_for_lunge ? 3 : 4;
                int reqAgi = prepared_for_lunge ? 4 : 5;
                string prepHint = prepared_for_lunge ? " (Alert Bonus)" : "";

                if (strVal >= reqStr)
                {
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[STR {strVal}/{reqStr}{prepHint}] Brace and Shove</color></b> Catch him by the shoulders and slam him into the wall! <color=#10B981>(-5 Stamina)</color>", () =>
                    {
                        stage.ExertStamina(5);
                        stage.PlaySFX("Audio/SFX/metal_shelf_crash", 0.6f);
                        lunge_resolved = true;
                        lunge_countered = true;
                        stealth_ambush_ready = true;
                        InspectHallway("<color=#10B981><b>[STRENGTH SUCCESS // WALL SLAM]</b></color>\n\n" +
                            "Pure fight-or-flight takes over. You plant your feet and catch him by the shoulders—his skin feels ice-cold and unnaturally greasy.\n\n" +
                            "You shove him violently sideways into the corridor drywall! He hits the plaster with a wet crunch, staggering off-balance as you ready yourself to fight for your life!");
                    }, true, new Color(0.2f, 0.9f, 0.4f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[STR {strVal}/{reqStr}{prepHint}] Brace and Shove</s> <color=#64748B><i>[Requires Might {reqStr}]</i></color>", null, false);
                }

                if (agiVal >= reqAgi)
                {
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[AGI {agiVal}/{reqAgi}{prepHint}] Dive Aside</color></b> Drop low and pivot out of his path! <color=#10B981>(-5 Stamina)</color>", () =>
                    {
                        stage.ExertStamina(5);
                        stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.75f);
                        lunge_resolved = true;
                        lunge_countered = true;
                        stealth_ambush_ready = true;
                        InspectHallway("<color=#10B981><b>[AGILITY SUCCESS // CLEAN DODGE]</b></color>\n\n" +
                            "Pure instinct takes over. You drop low and pivot sharply on your heel. His snapping teeth miss your face by an inch!\n\n" +
                            "His momentum carries him crashing knee-first into the opposite baseboard and plaster wall. You spring back up with a clear opening to defend yourself!");
                    }, true, new Color(0.2f, 0.9f, 0.4f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[AGI {agiVal}/{reqAgi}{prepHint}] Dive Aside</s> <color=#64748B><i>[Requires Reflex {reqAgi}]</i></color>", null, false);
                }

                stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[TAKE IMPACT]</color> Try to weather his charge!</b> <color=#EF4444>[RISK: TACKLED (-15 HP)]</color>", () =>
                {
                    zombie_tackled = true;
                    stage.TakeDamage(15, "Tackled hard onto the 2nd Floor corridor floor");
                    stage.PlaySFX("Audio/SFX/body_tackle", 0.9f);
                    InspectHallway();
                }, true, new Color(0.95f, 0.25f, 0.25f));

                return;
            }

            // State 2C: Mandatory Self-Defense Tactical Combat
            bool unarmed = player == null || string.IsNullOrEmpty(player.equippedWpn) ||
                           string.Equals(player.equippedWpn, "none", StringComparison.OrdinalIgnoreCase);
            string weaponReminder = unarmed
                ? "\n\n<color=#F59E0B><i>[!] You currently have no weapon equipped—open your Backpack [I] first if you want to equip one before fighting!</i></color>"
                : "";

            string readyDesc = lunge_countered
                ? "<color=#10B981><b>[CORRIDOR ADVANTAGE // ENEMY STAGGERED]</b></color>\n\n" +
                  "David Keller pushes himself off the dented drywall with a wet snarl, visibly battered from the impact. You cannot outrun him down a locked corridor—you have to put him down to survive."
                : "<color=#EF4444><b>[SELF-DEFENSE // NO ESCAPE]</b></color>\n\n" +
                  "David Keller scrambles back onto his feet, eyes locked onto your throat. There is no reasoning with him and nowhere to hide in this corridor. You must fight for your life.";

            stage.SetSituationNarration((!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : readyDesc) + weaponReminder);

            string engageTag = lunge_countered
                ? "<b><color=#EF4444>[ENGAGE — SELF-DEFENSE]</color> Ready weapon and defend yourself!</b> <color=#10B981>(Enemy Staggered: Torso & Leg Pre-Damaged)</color>"
                : "<b><color=#EF4444>[ENGAGE — SELF-DEFENSE]</color> Ready weapon and fight to survive!</b>";

            stage.SetChoice(choiceIdx++, engageTag, () =>
            {
                LaunchZombieCombat(isCountered: lunge_countered);
            }, true, new Color(0.95f, 0.25f, 0.25f));
        }

        private void RenderSection3CorridorAftermath(ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("f2_hall_3", "2ND FLOOR // RESIDENTIAL CORRIDOR");
            stage.TacticalMap?.SyncRoomState("f2_hall_3", isVisited: true, isChecked: zombie_looted || room203_looted, isLocked: false, isBlocked: false, isAccessible: true);

            stage.PlayStageAnimation("F2Dead_Anim", "F2_4_dead", 8f);

            string hallwayDesc = "<color=#10B981><b>[RESIDENTIAL CORRIDOR // AFTERMATH]</b></color>\n\n" +
                "David Keller lies motionless on the floral carpet runner, thick dark blood soaking into the fibers around his ruined head. " +
                "His spilled leather commuter briefcase rests near the skirting board across from the closed doors of Units 201, 202, and 203.\n\n" +
                "At the far western end of the corridor, a green emergency exit sign glows above the West Wing fire stairwell.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : hallwayDesc);

            if (!zombie_looted)
            {
                stage.SetChoice(choiceIdx++, "<color=#F59E0B>[SEARCH BODY & BRIEFCASE]</color> Check David's suit pockets and spilled briefcase in the hall <color=#9CA3AF>(~1 min)</color>", () =>
                {
                    zombie_looted = true;
                    room203_looted = true;
                    stage.Clock?.AddTime(1);
                    stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/Keys1.wav", 0.85f);

                    stage.GiveOrDropItem("Matches", 10);
                    stage.GiveOrDropItem("Canned Beans", 1);
                    stage.GiveOrDropItem("Bandage", 1);

                    stage.Lore?.AddLore("David Keller's ID Badge",
                        "Corporate ID badge for David Keller, resident of Unit 203. A worn yellow sticky note on the back reads: 'Remember to pick up spare inhaler and battery pack from maintenance cart by West Stairs.'",
                        stage.Clock?.GetTimeString() ?? "00:45");

                    stage.RefreshAllPDAPanels();

                    InspectHallway("<color=#10B981><b>[CORRIDOR GEAR RECOVERED]</b></color>\n\n" +
                        "Swallowing hard, you kneel beside the body and check his jacket pockets and the leather briefcase spilled open beside the wall.\n\n" +
                        "Inside his breast pocket is an Ascott Residency ID: <b>David Keller, Unit 203</b>. Stuck to the back is a crumpled yellow sticky note: " +
                        "<i>\"Remember to pick up spare inhaler and battery pack from maintenance cart by West Stairs.\"</i>\n\n" +
                        "From his coat and spilled commuter bag you salvage <b>Matches x10</b>, <b>Canned Beans x1</b>, and <b>Bandage x1</b>.");
                }, true);
            }
            else
            {
                stage.SetChoice(choiceIdx++, "Search body & spilled briefcase <color=#64748B>[ALREADY SEARCHED]</color>", null, false);
            }

            string doorsTag = hallway_doors_checked ? "<color=#9CA3AF>[ALL SEALED FROM INSIDE]</color>" : "";
            stage.SetChoice(choiceIdx++, $"[CHECK HALLWAY DOORS] {doorsTag} Test the apartment doors along the 2F corridor (Units 201–203)", () =>
            {
                stage.PlayClickSFX();
                InspectHallwayDoorsFromCorridor();
            }, true);

            stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[ADVANCE WEST]</color> Head toward the West Wing Fire Exit</b>", () =>
            {
                hallwaySection = 4;
                stage.PlayClickSFX();
                InspectHallway();
            }, true);

            stage.SetChoice(choiceIdx++, "[RETREAT EAST] Walk back toward the East Wing doorway", () =>
            {
                hallwaySection = 1;
                stage.PlayClickSFX();
                InspectHallway();
            }, true);
        }

        private void RenderSection4WestExit(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("f2_hall_4", "2ND FLOOR // WEST WING FIRE EXIT");
            stage.TacticalMap?.SyncRoomState("f2_hall_4", isVisited: true, isChecked: exit_stairs_inspected, isLocked: false, isBlocked: false, isAccessible: true);

            stage.PlayStageAnimation("F2Exit_Anim", "F2_5_exit", 8f);

            string exitDesc = "<color=#10B981><b>[WEST WING FIRE EXIT // THE WAY DOWN]</b></color>\n\n" +
                "You reach the western dead-end of the 2nd Floor. Cold air drafts beneath a heavy steel fire door equipped with an emergency crash bar. " +
                "Above the lintel, an illuminated green sign reads: <b><color=#10B981>WEST FIRE STAIRS — DIRECT GROUND FLOOR ACCESS</color></b>.\n\n" +
                "Beyond the reinforced wire-mesh glass, faint, muffled sounds echo up from the Ground Floor Lobby below—shouting, heavy thuds, and the rhythmic <i>pop-pop-pop</i> of distant gunfire.\n\n" +
                "<i>\"Gunfire... The police must be down there holding it together. If I can just get to them, maybe there's still a way out.\"</i>\n\n" +
                "Beside the stairwell frame sit an abandoned janitorial maintenance cart draped in soiled cleaning rags and a pile of discarded overnight luggage dropped by fleeing residents.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : exitDesc);

            if (!exit_stairs_inspected)
            {
                stage.SetChoice(choiceIdx++, "[INSPECT WINDOW] Peer through the wire-mesh glass down into the shaft <color=#9CA3AF>(~1 min)</color>", () =>
                {
                    exit_stairs_inspected = true;
                    stage.Clock?.AddTime(1);
                    stage.PlayClickSFX();
                    InspectHallway("<color=#93C5FD><b>[STAIRS CONFIRMED CLEAR]</b></color>\n\n" +
                        "You cup your hands against the cold wire-mesh glass. Unlike the East Wing stairwell, no furniture has been dumped here. " +
                        "A single clear flight of concrete steps leads straight down to the ground floor lobby, lit by flashing red-and-blue emergency strobes filtering through the street glass below.");
                }, true);
            }

            if (!janitor_cart_looted)
            {
                bool hasPer5 = player != null && player.per >= 5;
                bool canSearchCart = hasPer5 || zombie_looted;
                if (canSearchCart)
                {
                    string clueTag = hasPer5 ? "[PER 5]" : "[ID BADGE CLUE]";
                    stage.SetChoice(choiceIdx++, $"<color=#10B981>{clueTag} [SEARCH JANITOR'S CART]</color> Pull aside the rags on the maintenance cart <color=#9CA3AF>(~2 min)</color>", () =>
                    {
                        janitor_cart_looted = true;
                        stage.Clock?.AddTime(2);
                        stage.PlaySFX("Audio/SFX/flashlight_click", 0.8f);

                        stage.GiveOrDropItem("Inhaler", 1);
                        stage.GiveOrDropItem("Battery", 2);
                        stage.GiveOrDropItem("Bandage", 1);
                        stage.RefreshAllPDAPanels();

                        InspectHallway("<color=#10B981><b>[MAINTENANCE SUPPLIES RECOVERED]</b></color>\n\n" +
                            "Hidden beneath a bundle of stiff cleaning rags on the lower shelf of the janitor's cart, you uncover a sealed medical canister and spare power cells—the exact cache David Keller's sticky note mentioned.\n\n" +
                            "Recovered: <b>Inhaler x1</b>, <b>Battery x2</b>, and <b>Bandage x1</b>.");
                    }, true);
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "<s>[SEARCH JANITOR'S CART] Pull aside the rags on the maintenance cart</s> <color=#64748B><i>[REQUIRES PER 5 OR CLUE FROM TENANT'S ID]</i></color>", null, false);
                }
            }
            else
            {
                stage.SetChoice(choiceIdx++, "Search the janitor's maintenance cart <color=#64748B>[ALREADY SEARCHED]</color>", null, false);
            }

            if (!luggage_looted)
            {
                bool fastScavenge = player != null && (player.scavenging >= 1 || player.HasTrait("SCAVENGER") || player.HasTrait("FIELD FORAGER"));
                if (fastScavenge)
                {
                    stage.SetChoice(choiceIdx++, "<color=#10B981>[SCAVENGING] [SCOUR DISCARDED LUGGAGE]</color> Quickly check the bags worth opening by the exit <color=#9CA3AF>(~2 min)</color>", () =>
                    {
                        luggage_looted = true;
                        stage.Clock?.AddTime(2);
                        stage.PlayClickSFX();

                        stage.GiveOrDropItem("Energy Bar", 1);
                        stage.GiveOrDropItem("Bottled Water", 1);
                        stage.RefreshAllPDAPanels();

                        InspectHallway("<color=#10B981><b>[LUGGAGE SCAVENGED]</b></color>\n\n" +
                            "You skip the empty garment bags and unzip a half-spilled hiking duffel beside the fire door, pulling out <b>Energy Bar x1</b> and <b>Bottled Water x1</b>.");
                    }, true);
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "<color=#F59E0B>[SCOUR DISCARDED LUGGAGE]</color> Rummage through the pile of abandoned bags <color=#EF4444>(-5 Stamina, ~4 min)</color>", () =>
                    {
                        luggage_looted = true;
                        stage.ExertStamina(5);
                        stage.Clock?.AddTime(4);
                        stage.PlayClickSFX();

                        stage.GiveOrDropItem("Energy Bar", 1);
                        stage.GiveOrDropItem("Bottled Water", 1);
                        stage.RefreshAllPDAPanels();

                        InspectHallway("<color=#10B981><b>[LUGGAGE SCAVENGED]</b></color>\n\n" +
                            "It takes several tense minutes of digging through spilled clothes and broken zippers, but you salvage <b>Energy Bar x1</b> and <b>Bottled Water x1</b> from an abandoned duffel bag.");
                    }, true);
                }
            }
            else
            {
                stage.SetChoice(choiceIdx++, "Scour the discarded luggage <color=#64748B>[ALREADY SEARCHED]</color>", null, false);
            }

            stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[PUSH FIRE DOOR]</color> Push through the fire exit and descend to 1F Lobby</b>", () =>
            {
                stage.PlayClickSFX();
                stage.Objectives?.CompleteObjective("detour_2f");
                stage.Objectives?.CompleteObjective("reach_lobby");
                stage.Objectives?.AddObjective("escape_lobby", "Cross the ground floor lobby to reach the street outside", ObjectiveStatus.Active);

                stage.PlayDoorTransition(
                    "Videos/exit_stairs_door_transition",
                    "[ WEST FIRE STAIRS // GROUND FLOOR DESCENT ]",
                    "You push hard against the emergency crash bar.\n<color=#94A3B8>The heavy steel fire door swings smoothly open, revealing the clear flight down to the 1st Floor lobby...</color>",
                    false,
                    () => OnReachedGroundFloor(),
                    true
                );
            }, true, new Color(0.95f, 0.25f, 0.25f));

            stage.SetChoice(choiceIdx++, "[BACK] Return to 2F Residential Corridor", () =>
            {
                hallwaySection = 3;
                stage.PlayClickSFX();
                InspectHallway();
            }, true);
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

        private void LaunchZombieCombat(bool isCountered)
        {
            if (stage == null) return;

            stage.PlayClickSFX();

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

            if (isCountered)
            {
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

                    stage.PlaySFX("Audio/SFX/body_fall_wet", 0.75f);
                    stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:40", "2ND FLOOR // THE OUTBREAK REVEALED",
                        "Survived a close-quarters fight with David Keller in the 2nd Floor corridor. He wasn't sick or rioting—he was dead before he lunged.");

                    stage.Objectives?.AddObjective("reach_lobby", "Proceed to the West Wing fire exit and descend to the ground floor", ObjectiveStatus.Active);

                    InspectHallway("<color=#10B981><b>[THREAT ELIMINATED // REALITY SHATTERED]</b></color>\n\n" +
                        "With a final convulsive twitch, David Keller collapses face-first onto the carpet runner and lies completely still.\n\n" +
                        "You stand over the body, your chest heaving, adrenaline burning like battery acid in your veins. Your hands are shaking uncontrollably as dark, thick blood pools into the frayed carpet.\n\n" +
                        "<i>\"What just happened? Did I just... did I just kill someone?\"</i>\n\n" +
                        "You stare down at his ruined, half-torn jaw, the unseeing milky eyes, and the ragged wounds across his neck that had stopped bleeding long before he ever turned around. His skin was ice-cold. He wasn't even breathing.\n\n" +
                        "<i>\"No... that wasn't a person anymore. It couldn't be.\"</i>\n\n" +
                        "For the first time since waking up in Unit 407, the truth hits you like ice water: this isn't a riot, and it isn't mass hysteria. Something is turning the people of Seattle into mindless, flesh-eating corpses.");
                },
                onDefeat: () =>
                {
                    ResetToCorridorRetreat();
                    InspectHallway("<color=#EF4444><b>[TACTICAL DEFEAT // CORRIDOR OVERRUN]</b></color>\n\n" +
                        "David Keller's frenzied assault knocked you onto your back! With a desperate kick to his chest, you shove him away and scramble backward toward the East Wing fire door, trapped in the 2nd-floor gauntlet.\n\n" +
                        "<i>Tactical Survival Tip: The attacker does not feel pain. Target his LEGS to force a Crawler and destroy his mobility, or GUARD to cut his damage in half!</i>");
                },
                onFlee: () =>
                {
                    ResetToCorridorRetreat(extraStaminaCost: 15);
                    InspectHallway("<color=#F59E0B><b>[DRIVEN BACK]</b></color>\n\n" +
                        "You scramble backward toward the East Wing doorway, chest heaving—but the stairwell behind you is a dead end and the West Exit lies past him. You'll have to put him down to cross this corridor.");
                },
                isTutorial: true
            );
        }

        private void ResetToCorridorRetreat(int extraStaminaCost = 10)
        {
            hallwaySection = 1;
            zombie_alerted = true;
            lunge_resolved = false;
            zombie_tackled = false;
            lunge_countered = false;
            stealth_ambush_ready = false;
            stage?.ExertStamina(extraStaminaCost);
        }

        #endregion

        #region Corridor Exterior Door Checks (100% Pure Hallway — No Room Entry)

        private void InspectHallwayDoorsFromCorridor(string doorFeedback = null)
        {
            if (stage == null) return;

            hallway_doors_checked = true;
            currentInspectedRoom = null;
            stage.CurrentInspectedRoomInternal = "f2_hall_3";
            stage.SetLocation("f2_hall_3", "2ND FLOOR // SEALED APARTMENT DOORS");
            stage.ClearChoices();
            int choiceIdx = 0;

            string defaultNarration = "<color=#93C5FD><b>[SEALED APARTMENT DOORS // NO SANCTUARY ON 2F]</b></color>\n\n" +
                "You walk along the narrow residential corridor testing the brass handles of Units 201, 202, and 203. Every single apartment door on the 2nd Floor is shut tight from the hallway.\n\n" +
                "There are no open rooms to duck into here—only locked wood, warning notes, and dead silence.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(doorFeedback) ? doorFeedback : defaultNarration);

            string tag201 = room201_checked ? " <color=#9CA3AF>[BARRICADED]</color>" : "";
            stage.SetChoice(choiceIdx++, $"[TEST UNIT 201 DOOR]{tag201} Check the door smelling of pine cleaner", () =>
            {
                room201_checked = true;
                stage.PlaySFX("Audio/SFX/doorclosed1", 0.75f);
                InspectHallwayDoorsFromCorridor("<color=#F59E0B><b>[UNIT 201 // BARRICADED FROM WITHIN]</b></color>\n\n" +
                    "A thick scent of pine cleaner and melted beeswax seeps from under the draft stopper. Heavy 3-inch construction screws have been driven cleanly through the door frame into the studs.\n\n" +
                    "Taped over the peephole is a cardboard flap with jagged red marker:\n" +
                    "<color=#EF4444><i>\"WE HAVE 12-GAUGE BUCKSHOT AND THREE MONTHS OF WATER. DO NOT KNOCK. DO NOT POUND. WE WILL FIRE THROUGH THE PLASTER.\"</i></color>\n\n" +
                    "From deep inside, a battery radio crackles with static. Whoever lives here sealed themselves in early and isn't opening for anyone.");
            }, true);

            string tag202 = room202_checked ? " <color=#9CA3AF>[VACANT]</color>" : "";
            stage.SetChoice(choiceIdx++, $"[TEST UNIT 202 DOOR]{tag202} Check the flat with the pink notice", () =>
            {
                room202_checked = true;
                stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.75f);
                InspectHallwayDoorsFromCorridor("<color=#94A3B8><b>[UNIT 202 // EVICTED & PADLOCKED]</b></color>\n\n" +
                    "A bright pink legal eviction notice from the King County Sheriff's Office is taped squarely over the deadbolt, dated two weeks ago. The lock cylinder has been drilled out and replaced with a heavy brass padlocked hasp.\n\n" +
                    "Peeking through the empty peephole bore shows stripped floorboards and bare concrete counters. Nobody lives here.");
            }, true);

            stage.SetChoice(choiceIdx++, "[TEST UNIT 203 DOOR] Check David Keller's front door", () =>
            {
                stage.PlaySFX("Plugins/Feel/NiceVibrations/HapticSamples/Objects/DoorKnob1.wav", 0.8f);
                InspectHallwayDoorsFromCorridor("<color=#EF4444><b>[UNIT 203 // LOCKED OUT IN THE DARK]</b></color>\n\n" +
                    "Beside the doorbell hangs a dry-erase whiteboard: <i>\"Back from downtown at 6:30. Water is shut off again—boiler repair in basement.\"</i>\n\n" +
                    "When you test the brass knob, your fingers brush wet ridges along the deadbolt plate. Deep, frantic fingernail scratches—caked with dark blood and broken nail—are raked down the outside wood of the doorframe.\n\n" +
                    "The door is deadbolted shut from the inside, or jammed tight in its warped frame. David Keller never made it back inside his own apartment before the fever finished him in the hall.");
            }, true);

            stage.SetChoice(choiceIdx++, "[RETURN TO CORRIDOR] Step back into the 2F Residential Corridor", () =>
            {
                stage.PlayClickSFX();
                InspectHallway();
            }, true);

            stage.ReindexChoices();
        }

        #endregion

        #region Descent to Ground Floor Lobby

        private void OnReachedGroundFloor()
        {
            if (stage == null) return;

            if (stage.GroundFloorLobbyZone != null)
            {
                stage.GroundFloorLobbyZone.EnterZone(stage, "entry");
                return;
            }

            stage.IsInHallwayInternal = true;
            stage.CurrentInspectedRoomInternal = "nw_room";
            stage.TacticalMap?.LoadFloorById("ascott_lobby");
            stage.SetLocation("nw_room", "GROUND FLOOR // THE ASCOTT GRAND LOBBY");
            stage.PlayStageAnimation("LobbyMain_Anim", "Lobby_Main", 8f);
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

            ES3.Save<bool>("hub_f2_zombie_inspected", zombie_inspected, file);
            ES3.Save<bool>("hub_f2_prepared_lunge", prepared_for_lunge, file);
            ES3.Save<bool>("hub_f2_lunge_countered", lunge_countered, file);
            ES3.Save<bool>("hub_f2_zombie_tackled", zombie_tackled, file);
            ES3.Save<bool>("hub_f2_lunge_resolved", lunge_resolved, file);
            ES3.Save<bool>("hub_f2_doors_checked", hallway_doors_checked, file);
            ES3.Save<bool>("hub_f2_cart_looted", janitor_cart_looted, file);
            ES3.Save<bool>("hub_f2_luggage_looted", luggage_looted, file);

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

            if (ES3.KeyExists("hub_f2_zombie_inspected", file)) zombie_inspected = ES3.Load<bool>("hub_f2_zombie_inspected", file);
            if (ES3.KeyExists("hub_f2_prepared_lunge", file)) prepared_for_lunge = ES3.Load<bool>("hub_f2_prepared_lunge", file);
            if (ES3.KeyExists("hub_f2_lunge_countered", file)) lunge_countered = ES3.Load<bool>("hub_f2_lunge_countered", file);
            if (ES3.KeyExists("hub_f2_zombie_tackled", file)) zombie_tackled = ES3.Load<bool>("hub_f2_zombie_tackled", file);
            if (ES3.KeyExists("hub_f2_lunge_resolved", file)) lunge_resolved = ES3.Load<bool>("hub_f2_lunge_resolved", file);
            if (ES3.KeyExists("hub_f2_doors_checked", file)) hallway_doors_checked = ES3.Load<bool>("hub_f2_doors_checked", file);
            if (ES3.KeyExists("hub_f2_cart_looted", file)) janitor_cart_looted = ES3.Load<bool>("hub_f2_cart_looted", file);
            if (ES3.KeyExists("hub_f2_luggage_looted", file)) luggage_looted = ES3.Load<bool>("hub_f2_luggage_looted", file);

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
