using System;
using UnityEngine;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using SundayLastBreath.UI;

namespace SundayLastBreath.Zones
{
    /// <summary>
    /// Section 5: Emergency Stairwell Zone (Actor / Screenplay).
    /// Performs on the GameStageScreen Stage (Panggung).
    /// 
    /// Responsibilities:
    /// - Owns the multi-floor vertical descent shaft between floors (4F -> 3F -> 2F -> 1F).
    /// - Owns all stairwell phases:
    ///   - Phase 1: 4th Floor entrance & descent method (Flashlight, Lighter, Slow, Agility dark sprint).
    ///   - Phase 2: 3rd Floor landing, blood pool investigation, sealed extension-cord door, humanity check.
    ///   - Phase 3: 2nd Floor landing barricade, wedged drink machine, noise risk, listening at 2F door.
    ///   - Phase 4: Arrival at 2nd Floor East Wing (detour entry).
    /// - Owns stairwell tactical maps ("stairwell_4f", "stairwell_2f").
    /// - Manages state persistence via ES3 keys for all stairwell flags.
    /// </summary>
    public class EmergencyStairwellZone : MonoBehaviour
    {
        [Header("Stage Reference")]
        private GameStageScreen stage;

        [Header("Stairwell Descent State")]
        public int stair_phase = 1;
        public string descentMethod = "";
        public bool barricade_pushed = false;
        public bool barricade_inspected = false;
        public bool stair_3f_door_checked = false;
        public bool stair_listened_2f_door = false;
        public bool stair_noise_made = false;
        public bool stair_hum_deducted = false;
        public bool stair_entered_sfx_played = false;

        public bool IsOnSecondFloor => stair_phase >= 4;

        /// <summary>
        /// Resets all stairwell descent states for a fresh game.
        /// </summary>
        public void ResetZoneState()
        {
            stair_phase = 1;
            descentMethod = "";
            barricade_pushed = false;
            barricade_inspected = false;
            stair_3f_door_checked = false;
            stair_listened_2f_door = false;
            stair_noise_made = false;
            stair_hum_deducted = false;
            stair_entered_sfx_played = false;
        }

        /// <summary>
        /// Enters the Emergency Stairwell Zone, synchronizes stage state, and renders the descent view.
        /// </summary>
        public void EnterZone(GameStageScreen stageController, int startPhase = 1)
        {
            stage = stageController;
            if (stage == null) return;

            if (startPhase > 0)
            {
                stair_phase = startPhase;
            }

            stage.IsInHallwayInternal = true;
            stage.CurrentInspectedRoomInternal = "stairs";

            // Prepare stage UI
            stage.SetApartmentButtonsVisible(false);
            stage.EndAwakeningIntro();

            InspectStairwell();
        }

        #region Stairwell Descent Screenplay (Phases 1 - 4)

        public void InspectStairwell(string actionFeedback = null)
        {
            if (stage == null) return;

            stage.CurrentInspectedRoomInternal = "stairs";
            stage.SyncTacticalMapRoomStates();
            stage.ClearChoices();
            int choiceIdx = 0;

            var player = stage.CurrentPlayer;

            if (stair_phase <= 1)
            {
                stage.TacticalMap?.LoadFloorById("stairwell_4f");
                stage.SetLocation("f4_entrance", "4TH FLOOR // STAIRWELL DESCENT");
                stage.PlayStageAnimation("Stairwell4F_Anim", "stairwell4F", 8f);

                if (!stair_entered_sfx_played)
                {
                    stair_entered_sfx_played = true;
                    stage.PlaySFX("Audio/SFX/firedoor_slam", 0.9f);
                }

                stage.PlayHubBGM("Audio/SFX/generator_hum", 0.45f, true);

                stage.Objectives?.CompleteObjective("escape_building");
                stage.Objectives?.AddObjective("descend_stairs", "Descend through the stairwell to ground floor", ObjectiveStatus.Active);

                float carryWeight = player != null ? player.GetTotalWeight() : 0f;
                bool heavyPack = carryWeight >= 14f;
                string weightNote = heavyPack
                    ? $"\n\n<color=#F59E0B><b>[HEAVY PACK: {carryWeight:F1} KG]</b></color> Your loaded backpack pulls hard on your shoulders as you look down the steep concrete flight. Running in the dark with this much weight will throw off your balance."
                    : "";

                stage.SetSituationNarration($"The heavy fire door swings shut behind you and the lock clicks into place. You try the handle once just to be sure. Locked. There's no going back into the 4th floor.\n\nDim red backup lights buzz along the wall, barely reaching the middle of the steps. Down at the bottom of the shaft, the building's backup generator hums through the concrete. You need to get down to the ground floor.{weightNote}");

                bool hasFlashlight = player != null && player.HasItem("Flashlight");
                bool hasLighter = player != null && player.HasItem("Lighter");
                int agiVal = player != null ? player.agi : 5;

                // 1. Flashlight
                AddModularChoice(
                    ref choiceIdx,
                    hasFlashlight,
                    "[GEAR: FLASHLIGHT]",
                    "Use your Flashlight to light the steps",
                    0, 4,
                    () =>
                    {
                        descentMethod = "flashlight";
                        stage.Clock?.AddTime(4);
                        stair_phase = 2;
                        stage.PlaySFX("Audio/SFX/flashlight_click", 0.8f);
                        InspectStairwell();
                    },
                    "Requires Flashlight - You don't have a strong light source"
                );

                // 2. Lighter
                AddModularChoice(
                    ref choiceIdx,
                    hasLighter,
                    "[GEAR: LIGHTER]",
                    "Use your Lighter to see a few steps ahead",
                    0, 6,
                    () =>
                    {
                        descentMethod = "lighter";
                        stage.Clock?.AddTime(6);
                        stair_phase = 2;
                        stage.PlaySFX("Audio/SFX/lighter_flick", 0.8f);
                        InspectStairwell();
                    },
                    "Requires Lighter - You have nothing to spark a flame"
                );

                // 3. Move Slowly and Carefully in dark (Always safe fallback)
                stage.SetChoice(choiceIdx++, "Move slowly in the dark, one hand on the wall <color=#10B981>[SAFE]</color> <color=#9CA3AF>(~10 min)</color>", () =>
                {
                    descentMethod = "slow";
                    stage.Clock?.AddTime(10);
                    stair_phase = 2;
                    stage.PlaySFX("Audio/SFX/boots_slow_concrete", 0.7f);
                    InspectStairwell();
                }, true);

                // 4. Agility Sprint vs Risky Sprint
                int reqAgi = heavyPack ? 6 : 5;
                int sprintStam = heavyPack ? 10 : 5;
                string loadTag = heavyPack ? $" [HEAVY PACK {carryWeight:F1} KG]" : "";

                if (agiVal >= reqAgi)
                {
                    stage.SetChoice(choiceIdx++, $"[AGILITY {agiVal}/{reqAgi}]{loadTag} Move quickly down the steps in the dark <color=#10B981>(-{sprintStam} Stamina, Guaranteed, ~1 min)</color>", () =>
                    {
                        descentMethod = "quick";
                        stage.ExertStamina(sprintStam);
                        stage.Clock?.AddTime(1);
                        stair_phase = 2;
                        stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);
                        InspectStairwell();
                    }, true);
                }
                else
                {
                    int baseChance = agiVal * 18 - (heavyPack ? 15 : 0);
                    int chance = Mathf.Clamp(baseChance, 15, 75);

                    stage.SetChoice(choiceIdx++, $"[RISKY SPRINT]{loadTag} Rush down the dark steps [Agility {agiVal}/{reqAgi} - {chance}% Chance] <color=#EF4444>(-{sprintStam} Stamina, Fall & Noise Risk, ~1 min)</color>", () =>
                    {
                        stage.Clock?.AddTime(1);
                        if (UnityEngine.Random.Range(1, 101) <= chance)
                        {
                            descentMethod = "quick";
                            stage.ExertStamina(sprintStam);
                            stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.8f);
                        }
                        else
                        {
                            descentMethod = "quick_fail";
                            stair_noise_made = true;
                            player?.SetFlag("made_noise", true);
                            stage.PlaySFX("Audio/SFX/stairs_tumble", 0.95f);
                            stage.TakeDamage(15, "Slipped hard on the wet stairwell steps");
                            stage.ExertStamina(20);
                        }
                        stair_phase = 2;
                        InspectStairwell();
                    }, true);
                }
            }
            else if (stair_phase == 2)
            {
                stage.TacticalMap?.LoadFloorById("stairwell_4f");
                stage.SetLocation("f3_landing", "3RD FLOOR LANDING // THE DESCENT");
                stage.TacticalMap?.DiscoverRoom("f3_landing");

                bool sawHorrors = (stage.Floor4Zone != null && (stage.Floor4Zone.room401_entered || stage.Floor4Zone.room402_opened || stage.Floor4Zone.room405_opened || stage.Floor4Zone.room406_opened));

                if (descentMethod == "quick_fail")
                {
                    stage.PlayStageAnimation("StairwellTumble_Anim", "stairwell_tumble", 8f);
                }
                else if (descentMethod == "flashlight" || descentMethod == "lighter")
                {
                    stage.PlayStageAnimation("StairwellFlashlight_Anim", "stairwell_flashlight", 8f);
                    if (string.IsNullOrEmpty(actionFeedback))
                    {
                        stage.PlaySFX("Audio/SFX/blood_drip", 0.6f);
                    }
                }
                else
                {
                    stage.PlayStageAnimation("Stairwell4F_Anim", "stairwell4F", 8f);
                }

                bool seesBloodClearly = descentMethod == "quick_fail" || descentMethod == "flashlight" || descentMethod == "lighter";
                bool justLostHumanity = false;
                if (seesBloodClearly && !sawHorrors && !stair_hum_deducted)
                {
                    stair_hum_deducted = true;
                    justLostHumanity = true;
                    if (player != null)
                    {
                        player.humanity = Mathf.Max(0, player.humanity - 5);
                    }
                    stage.RefreshAllPDAPanels();
                }

                string descText = "";
                if (descentMethod == "quick")
                {
                    descText = "<color=#10B981><b>[AGILITY SUCCESS]</b></color> You take the steps two at a time, using the iron handrail to keep your balance.\n\nHalfway down to the 3rd-floor landing, your boot nearly comes down on a dark, wet patch beside a crushed red plastic cup. You catch the rail and hop wide onto the dry concrete below.\n\nUnder the dim red bulb, whatever you almost stepped in just looks like spilled beer or dirty water. You don't stop to check.";
                }
                else if (descentMethod == "slow")
                {
                    descText = "<color=#10B981><b>[SAFE DESCENT]</b></color> You keep one hand on the cold iron rail and slide your boots down one step at a time.\n\nNear the 3rd-floor landing, the red backup light glints off something wet pooled across the concrete next to a crushed plastic cup. In this dim light it just looks like spilled water. You press your back to the wall and step around the dry outer edge.";
                }
                else if (descentMethod == "quick_fail")
                {
                    bool readMarcus = stage.Floor4Zone != null && stage.Floor4Zone.read_marcus_log;
                    string marcusNote = readMarcus
                        ? "\n\nYou think of the note on Marcus's radio table upstairs. This must be where he got bit on his way down."
                        : "";
                    string humNote = sawHorrors
                        ? "\n\nAfter what you already saw upstairs on the 4th floor, you just wipe your palms on your pants and keep moving."
                        : $"\n\n{(justLostHumanity || stair_hum_deducted ? "<color=#EF4444><b>[-5 HUMANITY]</b></color> " : "")}Your stomach turns. Whoever bled here lost a lot of it, and it happened not long ago.";

                    descText = $"<color=#EF4444><b>[FOOTING LOST // LOUD NOISE] (-15 HP, -20 STAMINA)</b></color>\n\nYou take the steps too fast in the dark. Halfway down, your heel hits something thick and slick on the concrete and goes straight out from under you. Your ribs slam the edge of the steps and your knee bangs the iron railing with a loud metallic <i>CLANG</i> that echoes all the way down the shaft.\n\nWhen you push yourself up on the 3rd-floor landing, your palms are sticky and warm. Even under the dim red bulb, you smell the sharp copper scent on your hands. It's blood - a wide puddle of it smeared across the concrete beside a torn piece of a jacket sleeve.{marcusNote}{humNote}";
                }
                else
                {
                    string lightIntro = descentMethod == "flashlight"
                        ? "<color=#10B981><b>[FLASHLIGHT BEAM]</b></color> You click on your flashlight, cutting a white beam down the concrete steps."
                        : "<color=#10B981><b>[LIGHTER FLAME]</b></color> You thumb your lighter. The small orange flame pushes back the dim red shadows a few steps at a time.";
                    bool readMarcus = stage.Floor4Zone != null && stage.Floor4Zone.read_marcus_log;
                    string marcusNote = readMarcus
                        ? "\n\nYou remember the note by Marcus's radio upstairs. This is where he was bitten before he made it outside."
                        : "";
                    string humNote = sawHorrors
                        ? "\n\nAfter what you left behind on the 4th floor, you don't linger on it. You keep the light pointed ahead."
                        : $"\n\n{(justLostHumanity || stair_hum_deducted ? "<color=#EF4444><b>[-5 HUMANITY]</b></color> " : "")}Your throat tightens. Until now it was just sirens and voices on the radio, but this happened right outside your floor.";

                    descText = $"{lightIntro}\n\nHalfway to the 3rd-floor landing, the light catches a crushed red party cup and a wide, wet puddle spreading across the middle steps. Keeping the light steady, you step around the dry edge onto the landing.\n\nShining the light back over the concrete, you see it isn't water. It's thick, dark blood, still wet near the center. Red handprints are smeared across the wall at shoulder height next to a torn strip of a jacket sleeve - whoever got hurt here was leaning hard against the wall to stay on their feet.{marcusNote}{humNote}";
                }

                if (!string.IsNullOrEmpty(actionFeedback))
                {
                    stage.SetSituationNarration(actionFeedback);
                }
                else
                {
                    stage.SetSituationNarration(descText);
                }

                if (!stair_3f_door_checked)
                {
                    stage.SetChoice(choiceIdx++, "[CHECK DOOR] Try the 3rd Floor fire door <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        stair_3f_door_checked = true;
                        stage.Clock?.AddTime(1);
                        stage.PlaySFX("Audio/SFX/firedoor_click", 0.75f);
                        stage.Lore?.AddLore("3rd Floor Fire Door Sealed", "Someone knotted a heavy orange extension cord tightly around the inside push-bar and tied it to a hallway pipe. The 3rd-floor corridor is dark and inaccessible from the stairs.", stage.Clock?.GetTimeString() ?? "00:35");
                        stage.RefreshAllPDAPanels();
                        InspectStairwell("<color=#F59E0B><b>[3RD FLOOR SEALED]</b></color> You grab the handle of the 3rd-floor fire door and pull. It moves half an inch and stops dead. Looking through the narrow wire-glass window, you see someone knotted a heavy orange extension cord tightly around the inside push-bar and tied it to a hallway pipe. The 3rd-floor corridor beyond the glass is dark and quiet. There's no getting through here.");
                    }, true);
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "Try the 3rd Floor fire door <color=#EF4444>[TIED SHUT FROM INSIDE]</color>", null, false);
                }

                stage.SetChoice(choiceIdx++, "Turn the corner and head down toward the lower floors", () =>
                {
                    stair_phase = 3;
                    stage.PlayClickSFX();
                    InspectStairwell();
                }, true);
            }
            else if (stair_phase == 3)
            {
                stage.TacticalMap?.LoadFloorById("stairwell_2f");
                stage.SetLocation("f2_entrance", "STAIRWELL // THE BARRICADE");
                stage.TacticalMap?.SyncRoomState("f1_landing", isVisited: false, isChecked: barricade_inspected || barricade_pushed, isLocked: false, isBlocked: true, isAccessible: false);
                stage.PlayStageAnimation("StairwellBarricade_Anim", "stairwell_barricade", 8f);

                string baseNarration = "You head down past the 2nd-floor landing toward the ground floor lobby, but halfway between the 2nd and 1st floors, the steps are blocked.\n\n<color=#F59E0B><b>The stairs to the 1st Floor are jammed with furniture.</b></color>\n\nHeavy wooden desks, folding chairs, and a glass-front drink machine have been tipped over and wedged between the stairwell walls. Through a narrow gap beside the drink machine, you can see the 1st-floor lobby door just twenty feet below. The lower metal panel of the machine is dented inward and smeared with dark handprints from the bottom side.\n\nIt looks like if you could shift the drink machine a few inches, you might squeeze straight down to the 1st floor - otherwise you'll have to take the 2nd-floor door behind you.";

                if (!string.IsNullOrEmpty(actionFeedback))
                {
                    stage.SetSituationNarration(actionFeedback);
                }
                else
                {
                    if (barricade_pushed)
                    {
                        baseNarration += "\n\n<color=#EF4444><b>[IMPASSABLE // LOUD NOISE MADE]</b></color> The drink machine's steel legs are wedged tight under the iron rail posts. You can't clear this way.";
                    }
                    stage.SetSituationNarration(baseNarration);
                }

                if (!barricade_inspected && !barricade_pushed)
                {
                    stage.SetChoice(choiceIdx++, "[INSPECT] Look closely at how the drink machine is wedged <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        barricade_inspected = true;
                        stage.Clock?.AddTime(1);
                        stage.PlayClickSFX();
                        InspectStairwell("<color=#93C5FD><b>[BARRICADE INSPECTED]</b></color> You crouch by the gap and look at the frame. Whoever dumped this furniture from the upper floors wedged the drink machine's steel legs straight under the iron handrail posts. Shoving it by hand won't clear a path - it'll only scrape metal against concrete and make a loud racket down the shaft.");
                    }, true);
                }

                if (!barricade_pushed)
                {
                    stage.SetChoice(choiceIdx++, "Try to shove the drink machine aside to squeeze through <color=#EF4444>[RISK: NOISE]</color> <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        barricade_pushed = true;
                        stair_noise_made = true;
                        player?.SetFlag("made_noise", true);
                        stage.Clock?.AddTime(1);
                        stage.PlaySFX("Audio/SFX/metal_shelf_crash", 0.85f);
                        InspectStairwell("<color=#EF4444><b>[FAILED // LOUD NOISE MADE]</b></color> You brace your boots on the step and throw your shoulder into the side of the drink machine.\n\n<i>SCREEEEECH.</i>\n\nThe metal frame grinds hard against the concrete wall, echoing up and down the stairwell shaft like a gunshot. The machine doesn't budge an inch. Behind you on the 2nd-floor landing, something heavy thumps against the inside of the corridor wall.");
                    }, true);
                }
                else
                {
                    stage.SetChoice(choiceIdx++, "Try to shove the drink machine aside <color=#EF4444>[IMPASSABLE - WEDGED TIGHT]</color>", null, false);
                }

                if (!stair_listened_2f_door)
                {
                    stage.SetChoice(choiceIdx++, "[LISTEN] Press your ear to the 2nd Floor fire door before opening it <color=#9CA3AF>(~1 min)</color>", () =>
                    {
                        stair_listened_2f_door = true;
                        stage.Clock?.AddTime(1);
                        if (stair_noise_made)
                        {
                            stage.PlaySFX("Audio/SFX/zombie_rattle", 0.65f);
                            InspectStairwell("<color=#EF4444><b>[SOUND BEHIND DOOR // DANGER]</b></color> You press your ear against the cold steel of the 2nd-floor fire door. The noise you made in the stairwell carried inside. Right on the other side of the door, you hear wet, ragged breathing and shoes scraping against the floor. Whatever is in that hallway is already facing the door. <i>(Make sure a weapon is equipped in your Backpack before opening.)</i>");
                        }
                        else
                        {
                            stage.PlaySFX("Audio/SFX/head_thump_wet", 0.55f);
                            InspectStairwell("<color=#F59E0B><b>[SOUND BEHIND DOOR]</b></color> You press your ear to the cold steel of the 2nd-floor door. For a second there's only the buzz of a failing hallway light - then a slow, wet, rhythmic <i>thump... thump...</i> about twenty feet down the hall, like someone tapping their forehead against the plaster wall. Whoever it is hasn't heard you yet. <i>(Check your Backpack if you need to equip a weapon first.)</i>");
                        }
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "Open the fire door to the 2nd Floor <color=#10B981>[DETOUR TO 2F]</color> <color=#9CA3AF>(~2 min)</color>", () =>
                {
                    stage.StopHubBGM();
                    stage.PlaySFX("Audio/SFX/firedoor_click", 0.8f);
                    stage.Clock?.AddTime(2);
                    player?.SetFlag("made_noise", stair_noise_made);
                    stage.Objectives?.CompleteObjective("descend_stairs");
                    stage.Objectives?.AddObjective("detour_2f", "Cross the 2nd Floor to find the West Wing stairs", ObjectiveStatus.Active);
                    stair_phase = 4;
                    stage.Floor2Zone.EnterZone(stage, 1, stair_noise_made);
                }, true);
            }
            else if (stair_phase >= 4)
            {
                stage.Floor2Zone.EnterZone(stage, 1, stair_noise_made);
                return;
            }

            stage.ReindexChoices();
        }

        #endregion

        #region Modular Choice Helper

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

            ES3.Save<int>("hub_stair_phase", stair_phase, file);
            ES3.Save<string>("hub_descent_method", descentMethod, file);
            ES3.Save<bool>("hub_barricade_pushed", barricade_pushed, file);
            ES3.Save<bool>("hub_barricade_inspected", barricade_inspected, file);
            ES3.Save<bool>("hub_stair_3f_door_checked", stair_3f_door_checked, file);
            ES3.Save<bool>("hub_stair_listened_2f_door", stair_listened_2f_door, file);
            ES3.Save<bool>("hub_stair_noise_made", stair_noise_made, file);
            ES3.Save<bool>("hub_stair_hum_deducted", stair_hum_deducted, file);
        }

        public void LoadZoneState(string file)
        {
            if (string.IsNullOrEmpty(file) || !ES3.FileExists(file)) return;

            if (ES3.KeyExists("hub_stair_phase", file)) stair_phase = ES3.Load<int>("hub_stair_phase", file);
            if (ES3.KeyExists("hub_descent_method", file)) descentMethod = ES3.Load<string>("hub_descent_method", file);
            if (ES3.KeyExists("hub_barricade_pushed", file)) barricade_pushed = ES3.Load<bool>("hub_barricade_pushed", file);
            if (ES3.KeyExists("hub_barricade_inspected", file)) barricade_inspected = ES3.Load<bool>("hub_barricade_inspected", file);
            if (ES3.KeyExists("hub_stair_3f_door_checked", file)) stair_3f_door_checked = ES3.Load<bool>("hub_stair_3f_door_checked", file);
            if (ES3.KeyExists("hub_stair_listened_2f_door", file)) stair_listened_2f_door = ES3.Load<bool>("hub_stair_listened_2f_door", file);
            if (ES3.KeyExists("hub_stair_noise_made", file)) stair_noise_made = ES3.Load<bool>("hub_stair_noise_made", file);
            if (ES3.KeyExists("hub_stair_hum_deducted", file)) stair_hum_deducted = ES3.Load<bool>("hub_stair_hum_deducted", file);
        }

        #endregion
    }
}
