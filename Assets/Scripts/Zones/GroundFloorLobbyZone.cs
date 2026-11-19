using System;
using UnityEngine;
using SundayLastBreath.Core;
using SundayLastBreath.Data;
using SundayLastBreath.UI;

namespace SundayLastBreath.Zones
{
    /// <summary>
    /// Section 7: Ground Floor Grand Lobby Climax (Actor / Screenplay).
    /// Performs on the GameStageScreen Stage.
    ///
    /// Design:
    /// - Fast-paced multi-crisis triage climax inside the vaulted Ascott Grand Lobby.
    /// - Resolves Vane (Elevators) and Maya & Lily (Reception) 100% through swift, weapon-reactive
    ///   narrative checks (Firearm, Bow, Blade, Blunt, Unarmed Brawn, Desperate Bodily Sacrifice,
    ///   or Authentic Failure) with zero combat-modal popups, preserving breathless pacing between
    ///   Floor 2 (David Keller) and Act 2 (The Streets & Parking Lot).
    /// - 2-Beat Action Economy:
    ///   - Beat 1: Intervene at Elevators, Reception, Fallen Soldier's Pack, or rush the Barricade.
    ///   - Beat 2 (Center Rotunda): High-reflex builds (AGI >= 5 / RUNNER / ADRENALINE) earn a free
    ///     second intervention; slower builds can spend -15 Stamina ([BURN THE MUSCLE]) or expend a
    ///     distraction item (Bluetooth Speaker / Ceramic Mug) to save a second crisis before collapse.
    /// - Full Act 1 Continuity & Reactivity:
    ///   - Echoes discoveries from Unit 407, Floor 4 (Units 401–406), the 3rd-floor stairwell blood,
    ///     and Floor 2's outbreak realization, shaped by current Humanity bracket and Profession.
    /// </summary>
    public class GroundFloorLobbyZone : MonoBehaviour
    {
        [Header("Stage Reference")]
        private GameStageScreen stage;

        [Header("Lobby State & 2-Beat Triage")]
        public bool isInLobby = false;
        public string lobbyPhase = "entry";
        public int triageActionsTaken = 0;
        public bool extraBeatUnlocked = false;

        [Header("Crisis Outcomes")]
        public bool vane_resolved = false;
        public string vane_status = "none";     // none | joined | hidden | abandoned | dead
        public bool maya_resolved = false;
        public string maya_status = "none";     // none | rescued | dead
        public bool warned_maya_cop = false;
        public bool pack_resolved = false;

        [Header("Barricade Standoff State")]
        public bool barricade_talk_failed = false;
        public string barricade_resolution = "";

        private struct WeaponProfile
        {
            public string itemId;
            public bool isFirearm;
            public bool isBow;
            public bool isBlade;
            public bool isBlunt;
            public bool hasAmmo;
            public string ammoType;
            public int ammoCount;
        }

        /// <summary>
        /// Resets all Ground Floor Lobby states for a fresh game.
        /// </summary>
        public void ResetZoneState()
        {
            isInLobby = false;
            lobbyPhase = "entry";
            triageActionsTaken = 0;
            extraBeatUnlocked = false;
            vane_resolved = false;
            vane_status = "none";
            maya_resolved = false;
            maya_status = "none";
            warned_maya_cop = false;
            pack_resolved = false;
            barricade_talk_failed = false;
            barricade_resolution = "";
        }

        /// <summary>
        /// Enters the Ground Floor Lobby Zone, synchronizes stage state, and renders the current phase.
        /// </summary>
        public void EnterZone(GameStageScreen stageController, string startPhase = null)
        {
            stage = stageController;
            if (stage == null) return;

            isInLobby = true;
            if (!string.IsNullOrEmpty(startPhase))
            {
                lobbyPhase = startPhase;
            }
            else if (string.IsNullOrEmpty(lobbyPhase))
            {
                lobbyPhase = "entry";
            }

            stage.IsInHallwayInternal = true;
            stage.SetApartmentButtonsVisible(false);
            stage.EndAwakeningIntro();
            stage.PlayHubBGM("Audio/BGM/lobby_slaughter", 0.60f, true);
            if (string.Equals(lobbyPhase, "entry", StringComparison.OrdinalIgnoreCase))
            {
                stage.PlaySFX("Audio/SFX/heavy_door_clack", 0.75f);
            }

            InspectLobby();
        }

        private string GetCurrentLobbyRoomId()
        {
            switch (lobbyPhase)
            {
                case "vane_approach":
                case "vane_aftermath":
                    return "e_room";
                case "maya_approach":
                case "maya_aftermath":
                    return "ne_room";
                case "pack_approach":
                case "pack_grabbed":
                    return "n_room";
                case "center_triage":
                    return "c_room";
                case "barricade_approach":
                case "barricade_standoff":
                case "barricade_unlocked":
                case "act2_exit":
                    return "lobby_exit";
                case "entry":
                default:
                    return "nw_room";
            }
        }

        public void InspectLobby(string actionFeedback = null)
        {
            if (stage == null) return;

            string roomId = GetCurrentLobbyRoomId();
            stage.CurrentInspectedRoomInternal = roomId;

            stage.TacticalMap?.LoadFloorById("ascott_lobby");
            stage.TacticalMap?.DiscoverRoom(roomId);
            stage.SyncTacticalMapRoomStates();

            stage.ClearChoices();
            int choiceIdx = 0;
            var player = stage.CurrentPlayer;

            switch (lobbyPhase)
            {
                case "entry":
                    RenderEntryPhase(player, ref choiceIdx, actionFeedback);
                    break;
                case "vane_approach":
                    RenderVaneApproach(player, ref choiceIdx, actionFeedback);
                    break;
                case "vane_aftermath":
                    RenderVaneAftermath(player, ref choiceIdx, actionFeedback);
                    break;
                case "maya_approach":
                    RenderMayaApproach(player, ref choiceIdx, actionFeedback);
                    break;
                case "maya_aftermath":
                    RenderMayaAftermath(player, ref choiceIdx, actionFeedback);
                    break;
                case "pack_approach":
                    RenderPackApproach(player, ref choiceIdx, actionFeedback);
                    break;
                case "pack_grabbed":
                    RenderPackGrabbed(ref choiceIdx, actionFeedback);
                    break;
                case "center_triage":
                    RenderCenterTriage(player, ref choiceIdx, actionFeedback);
                    break;
                case "barricade_approach":
                    RenderBarricadeApproach(player, ref choiceIdx, actionFeedback);
                    break;
                case "barricade_standoff":
                    RenderBarricadeStandoff(player, ref choiceIdx, actionFeedback);
                    break;
                case "barricade_unlocked":
                case "act2_exit":
                    RenderExitUnlocked(player, ref choiceIdx, actionFeedback);
                    break;
                default:
                    lobbyPhase = "entry";
                    RenderEntryPhase(player, ref choiceIdx, actionFeedback);
                    break;
            }

            stage.TacticalMap?.SetCurrentLocation(roomId);
            stage.SyncTacticalMapRoomStates();
            stage.TacticalMap?.UpdateDoorwayVisuals();
            stage.TacticalMap?.RefreshAllNodes();
            stage.ReindexChoices();
        }

        #region Phase 1: Lobby Entry & Triage Hub

        private void RenderEntryPhase(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("nw_room", "GROUND FLOOR // THE ASCOTT GRAND LOBBY");
            stage.PlayStageAnimation("LobbyMain_Anim", "Lobby_Main", 8f);
            stage.PlaySFX("Audio/SFX/distant_scream1", 0.45f);

            string journeyEcho = GetAct1JourneyEcho(player);
            string humanityThought = GetHumanityLobbyThought(player);
            string jobThought = GetJobLobbyObservation(GetNormalizedJob(player));

            string narration = "<color=#EF4444><b>[THE ASCOTT GRAND LOBBY // THE GILDED SLAUGHTERHOUSE]</b></color>\n\n" +
                "The West Wing fire door punches open, and the last thin hope that carried you down four flights dies in your throat.\n\n" +
                "The Ascott Grand's lobby was built to make you feel small—fifty feet of vaulted ceiling, polished Italian marble the color of old bone, and tiered crystal chandeliers still burning warm amber like a luxury hotel hosting a gala inside a slaughterhouse.\n\n" +
                "The smell hits first: cordite, spilled champagne, arterial blood, and beneath them both, the same sour, unliving rot you tasted on the air when David Keller's neck snapped toward you on the 2nd Floor. " +
                "Then the sound: screaming layered over screaming, the wet skid of dress shoes on marble, glass shattering along the mezzanine.\n\n" +
                $"{journeyEcho}{jobThought}\n\n" +
                "You press your back against the stairwell steel and take one full second to read the room. One second. That is all you can buy:\n\n" +
                "• <b>By the brass elevators (Left):</b> An androgynous young man in a ruined designer coat is backed flush against the doors, swinging a hard-shell Pelican camera case with both shaking hands. He is out of room.\n" +
                "• <b>At the marble reception desk (Right):</b> An infected resident has vaulted the counter. A woman holds one flat palm out in front of a shock-silent seven-year-old girl pinned against the mail cubbies.\n" +
                "• <b>Ten feet across the open floor (North):</b> A fallen National Guard courier lies twisted beside an untouched military-grade MOLLE field pack.\n" +
                "• <b>At the front glass doors (South):</b> Two Seattle PD officers stand behind chained luggage carts—rifles raised <i>inward</i>, watching neighbors die twenty feet away without firing a rescue round.\n\n" +
                humanityThought;

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            AddUnresolvedCrisisChoices(ref choiceIdx, isSecondBeat: false);
        }

        private void AddUnresolvedCrisisChoices(ref int choiceIdx, bool isSecondBeat)
        {
            string beatSuffix = isSecondBeat ? " <color=#10B981>[SECOND BEAT]</color>" : "";

            if (!vane_resolved)
            {
                string vaneHint = isSecondBeat
                    ? "Sprint to the elevator bank before the young man's wrists give out."
                    : "A cornered young man in designer clothes is losing his last stand with a camera case.";
                stage.SetChoice(choiceIdx++, $"<b><color=#38BDF8>[ELEVATORS — LEFT]</color> Rush to help the young man{beatSuffix}</b> <color=#9CA3AF>— {vaneHint}</color>", () =>
                {
                    stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.75f);
                    lobbyPhase = "vane_approach";
                    InspectLobby();
                }, true);
            }

            if (!maya_resolved)
            {
                string assistTag = vane_status == "joined" ? " <color=#10B981>[VANE FLANKS WITH YOU]</color>" : "";
                stage.SetChoice(choiceIdx++, $"<b><color=#F59E0B>[RECEPTION DESK — RIGHT]</color> Rush to save the woman and child{assistTag}{beatSuffix}</b> <color=#9CA3AF>— An infected has vaulted behind the marble counter.</color>", () =>
                {
                    stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.75f);
                    lobbyPhase = "maya_approach";
                    InspectLobby();
                }, true);
            }

            if (!pack_resolved)
            {
                string moralityWarning = isSecondBeat
                    ? "<color=#10B981>(No Humanity Loss — You already stepped in for the living first)</color>"
                    : "<color=#EF4444>(-10 Humanity — Stripping gear while people scream for help)</color>";
                stage.SetChoice(choiceIdx++, $"<b><color=#94A3B8>[NORTH FLOOR — NEARBY]</color> Loot the dead courier's military pack</b> {moralityWarning}", () =>
                {
                    stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.7f);
                    lobbyPhase = "pack_approach";
                    InspectLobby();
                }, true);
            }

            string exitLabel = isSecondBeat
                ? "<b><color=#EF4444>[FRONT DOORS]</color> Break for the police barricade</b> <color=#9CA3AF>— Leave whoever is still pinned behind you.</color>"
                : "<b><color=#EF4444>[FRONT DOORS]</color> Ignore the chaos and head straight for the exit</b> <color=#9CA3AF>— Let the building take whoever is left.</color>";

            stage.SetChoice(choiceIdx++, exitLabel, () =>
            {
                stage.PlayClickSFX();
                ResolveIgnoredLobbyCrises();
                lobbyPhase = "barricade_approach";
                InspectLobby();
            }, true, new Color(0.85f, 0.30f, 0.30f));
        }

        #endregion

        #region Phase 2: Crisis A — Elevators (Vane)

        private void RenderVaneApproach(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("e_room", "LOBBY // CROSSING TO THE ELEVATORS");
            stage.PlayStageAnimation("LobbyVane_Anim", "Femboy_in_need", 8f);
            stage.PlaySFX("Audio/SFX/vanebreathing", 0.75f);

            string narration = "<color=#38BDF8><b>[CROSSING TO THE ELEVATOR BANKS]</b></color>\n\n" +
                "You break left and sprint.\n\n" +
                "The Italian marble is treacherous—your first boot skids half an inch over something warm and dark, dropping your stomach into your heels before your tread bites and launches you forward. Your footsteps crack like pistol shots beneath the fifty-foot ceiling.\n\n" +
                "Fifteen feet. Ten. You hear him now—ragged, hyperventilating breaths and the heavy plastic <i>THWACK</i> of a $3,000 Pelican camera case rebounding off bone. A latch pops loose; shattered camera lenses skitter across the stone.\n\n" +
                "Five feet. His spine connects with the brushed-steel elevator doors in a hollow metallic thud. Dyed hair plastered to his forehead with sweat, clothes absurdly stylish for the end of the world—and when he catches sight of you closing distance, something shifts behind his dark eyes. Not relief yet. Just a held breath adjusting from <i>this is it</i> to <i>maybe not yet</i>.\n\n" +
                "Half the creature's jaw is torn away, and its entire universe has narrowed to the throat in front of it. It hasn't heard you coming.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            AddSwiftRescueChoices(ref choiceIdx, isVaneRescue: true, player: player);

            stage.SetChoice(choiceIdx++, "[STEP BACK] Reassess the lobby before committing", () =>
            {
                stage.PlayClickSFX();
                lobbyPhase = triageActionsTaken == 0 ? "entry" : "center_triage";
                InspectLobby();
            }, true);
        }

        private void RenderVaneAftermath(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("e_room", "LOBBY // ELEVATOR RESCUE");
            stage.PlayStageAnimation("LobbyMain_Anim", "Lobby_Main", 8f);

            string humanityPrompt = player != null && player.humanity <= 30
                ? "\n\n<i>One pair of hands carrying a heavy plastic case is either an asset or a distraction to throw between yourself and the street. Decide fast.</i>"
                : "";

            string narration = "<color=#10B981><b>[ELEVATOR RESCUE // +25 XP]</b></color>\n\n" +
                "He slides slowly down the brushed-steel elevator doors until he's sitting on the marble. For several long seconds he doesn't say a word—staring at the ruined thing beside his sneakers, then at his own hands, still trembling faintly at the wrists.\n\n" +
                "When he finally looks up at you, it's the stare of someone updating an entire model of how reality works in real time. A long, controlled breath goes out of him, followed by a tired, almost embarrassed ghost of a smile.\n\n" +
                "<b>Vane:</b> <i>\"I hauled a three-thousand-dollar Pelican case down four flights of stairs in the dark... Turns out it's actually fantastic for bludgeoning dead people. Good to know. I'm Vane. I'd tell you to follow my channel, but I'm pretty sure the internet just died. Thank you. Seriously—I was absolutely not winning that.\"</i>\n\n" +
                "He climbs back onto his feet, holding the battered case across his chest, waiting on your word." +
                humanityPrompt;

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            stage.SetChoice(choiceIdx++, "<b><color=#10B981>[TAKE HIM WITH YOU]</color> \"Stay on my right shoulder. Don't freeze when we move.\"</b>", () =>
            {
                vane_resolved = true;
                vane_status = "joined";
                player?.SetFlag("vane_joined", true);
                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:52", "LOBBY // RECRUITED VANE",
                    "Saved Vane by the Ascott elevators. He fell in off my right shoulder, gripping his Pelican case like a shield.");
                stage.PlayClickSFX();
                FinishCrisisBeat();
            }, true, new Color(0.2f, 0.85f, 0.45f));

            stage.SetChoice(choiceIdx++, "[LEAVE TO HIDE] \"Stay low in the back service alcove behind the elevators. I've got more to do here.\"", () =>
            {
                vane_resolved = true;
                vane_status = "hidden";
                player?.SetFlag("vane_hidden", true);
                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:52", "LOBBY // TOLD VANE TO HIDE",
                    "Cleared the elevator bank for Vane and told him to barricade inside the back service alcove.");
                stage.PlayClickSFX();
                FinishCrisisBeat();
            }, true);

            stage.SetChoice(choiceIdx++, "<color=#EF4444>[ABANDON HIM (-5 HUMANITY)]</color> \"We're even. Don't follow me.\"", () =>
            {
                vane_resolved = true;
                vane_status = "abandoned";
                if (player != null)
                {
                    player.humanity = Mathf.Max(0, player.humanity - 5);
                    player.SetFlag("vane_abandoned", true);
                }
                stage.RefreshAllPDAPanels();
                stage.UpdateVitalsUI();
                stage.PlayClickSFX();
                FinishCrisisBeat("<color=#EF4444><b>[-5 HUMANITY]</b></color> Whatever fragile gratitude was in Vane's eyes dies flat as you turn away from him without a second glance.");
            }, true);
        }

        #endregion

        #region Phase 3: Crisis B — Reception Desk (Maya & Lily)

        private void RenderMayaApproach(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("ne_room", "LOBBY // CROSSING TO RECEPTION");
            stage.PlayStageAnimation("LobbyMayaNeed_Anim", "Brown_woman_in_need", 8f);
            stage.PlaySFX("Audio/SFX/distant_scream2", 0.55f);

            bool vaneAssists = vane_status == "joined";
            string vaneAssistText = vaneAssists
                ? "\n\n<color=#10B981><b>[COMPANION SYNERGY — VANE FLANKS]</b></color> Without waiting to be asked, Vane sprints wide on your flank and slams the heavy Pelican case straight across the marble counter! The creature's ruined jaw snaps sideways toward the impact—unbalancing its stance and dropping its guard for your strike!"
                : "";

            string narration = "<color=#F59E0B><b>[CROSSING TO THE MARBLE RECEPTION DESK]</b></color>\n\n" +
                "You break right across the open rotunda.\n\n" +
                "The screaming is sharper here—a woman's voice and a child's voice braided tight. The woman keeps calling her niece's name, fighting to anchor the seven-year-old's eyes onto her face, while the little girl pinned behind her coat has gone somewhere far away inside shock.\n\n" +
                "The infected vaulted the reception counter like it wasn't even furniture. It's behind the desk now, cutting off their only exit, backing the woman flush against the mail cubbies. She holds one flat, trembling palm out in front of the child as if sheer determination could hold back teeth. She is completely out of room.\n\n" +
                "Then she sees you crossing the marble. You watch the entire arc cross her face—from past despair to something fragile and fierce. The creature's back is squarely toward you. That half-second is everything." +
                vaneAssistText;

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            AddSwiftRescueChoices(ref choiceIdx, isVaneRescue: false, player: player);

            stage.SetChoice(choiceIdx++, "[STEP BACK] Reassess the lobby before committing", () =>
            {
                stage.PlayClickSFX();
                lobbyPhase = triageActionsTaken == 0 ? "entry" : "center_triage";
                InspectLobby();
            }, true);
        }

        private void RenderMayaAftermath(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("ne_room", "LOBBY // RECEPTION RESCUE");

            if (warned_maya_cop)
            {
                stage.PlayStageAnimation("LobbyMayaPolice_Anim", "Brown_woman_Police", 8f);
            }
            else
            {
                stage.PlayStageAnimation("LobbyMayaRelieved_Anim", "Brown_woman_relieved", 8f);
            }

            string defaultNarration = "<color=#10B981><b>[RECEPTION RESCUE // +25 XP]</b></color>\n\n" +
                "Maya drops straight to her knees behind the counter and wraps both arms completely around the little girl. Thirty long seconds of just breathing—both of them holding on like that embrace is the only solid wall left in the building. When she finally looks up over Lily's shoulder, her eyes are dry.\n\n" +
                "<b>Maya:</b> <i>\"I'm Maya. And this is Lily—my niece. My sister didn't make it off our floor. She made me promise to get her out alive. Thank you... I didn't think anyone was coming.\"</i>\n\n" +
                "Maya's gaze tracks past your shoulder toward the two uniformed officers crouched behind the front-door barricade. Gently, deliberately, she presses a palm over Lily's ear—blocking out sounds the child has already heard too much of.\n\n" +
                "<b>Maya:</b> <i>\"Those officers have rifles and steel in front of them. Right now, walls are what Lily needs. I can't drag a seven-year-old back into open street riots with nothing but a stranger. You go—you clearly know what you're doing.\"</i>";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : defaultNarration);

            string job = GetNormalizedJob(player);
            bool canSpotBite = player != null && (player.per >= 5 || job == "doctor" || job == "police");

            if (!warned_maya_cop && canSpotBite)
            {
                string tag = job == "doctor" ? "[DOCTOR]" : (job == "police" ? "[POLICE OFFICER]" : "[PER 5]");
                stage.SetChoice(choiceIdx++, $"<color=#38BDF8>{tag} [WARN MAYA] Point out the younger officer's soaked forearm bandage (+5 Humanity)</color>", () =>
                {
                    warned_maya_cop = true;
                    if (player != null)
                    {
                        player.SetFlag("warned_maya_cop", true);
                        player.humanity = Mathf.Min(100, player.humanity + 5);
                    }
                    stage.RefreshAllPDAPanels();
                    stage.UpdateVitalsUI();
                    stage.PlayClickSFX();

                    InspectLobby("<color=#38BDF8><b>[CRUCIAL WARNING // +5 HUMANITY]</b></color>\n\n" +
                        "Before Maya rises, your eyes lock onto the younger officer across the floor. Even through the chandelier glare, you catch the blood soaking through a clumsy wrap around his left forearm—and the unnatural grey tremor racking his shoulders.\n\n" +
                        "Remembering what you just saw on David Keller's neck upstairs, you catch Maya's wrist before she steps into the open.\n\n" +
                        "<b>You:</b> <i>\"Listen to me—look at the young cop by the glass. He's bleeding through his sleeve and burning up with fever. He's been bitten. Whatever happens, do not get trapped inside a squad car or a locked room with him.\"</i>\n\n" +
                        "Maya freezes as her eyes find the rookie's trembling hands. She draws Lily tighter against her ribs, whispers a shaken <i>\"Thank you,\"</i> and moves cautiously toward the far edge of the barricade.");
                }, true, new Color(0.25f, 0.72f, 0.96f));
            }

            stage.SetChoice(choiceIdx++, "<b><color=#10B981>[WATCH THEM MOVE]</color> \"Keep Lily's head down behind the luggage carts and stay clear of the glass.\"</b>", () =>
            {
                maya_resolved = true;
                maya_status = "rescued";
                player?.SetFlag("maya_rescued", true);
                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:54", "LOBBY // SAVED MAYA & LILY",
                    warned_maya_cop
                        ? "Saved Maya and 7-year-old Lily behind Reception and warned Maya about the bitten rookie officer."
                        : "Saved Maya and 7-year-old Lily behind Reception; she moved behind the police barricade to shelter Lily.");
                stage.PlayClickSFX();
                FinishCrisisBeat(warned_maya_cop
                    ? null
                    : "Maya picks her way carefully across the blood-streaked marble toward the front barricade. Halfway there, Lily peeks back over her aunt's shoulder once—big dark eyes, fully present again—before turning forward.");
            }, true);
        }

        #endregion

        #region Phase 4: Crisis C — Dead Soldier's Pack (Humanity & Wrist-Grab Twist)

        private void RenderPackApproach(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("n_room", "LOBBY // THE DEAD MAN");
            stage.PlayStageAnimation("LobbyDeadMan_Anim", "Lobby_Dead_Man", 8f);

            string urgencyNote = triageActionsTaken == 0
                ? "<color=#EF4444><i>Twenty feet away, Vane's Pelican case cracks against bone and Maya's voice breaks behind the desk. Stripping supplies off a dead soldier while neighbors are dying right in front of you will leave a mark (-10 Humanity).</i></color>"
                : "<color=#10B981><i>You already threw yourself into the fire for the living first. Securing military-grade medical supplies and ammo now is pure survival.</i></color>";

            string narration = "<color=#94A3B8><b>[NORTH FLOOR // THE FALLEN COURIER]</b></color>\n\n" +
                "You don't have to go far—he lies twisted near the first marble pillar. Up close, the damage to his throat and face is catastrophic and recent, the frantic work of something that moved on fast.\n\n" +
                "The backpack strapped to his webbing is completely untouched: heavy MOLLE loops, compression straps properly tensioned the way someone trained loads field gear.\n\n" +
                "Behind you across the marble, something drags wetly—directionless at first, then steadily locking onto your crouch. You have ten seconds before that becomes an immediate problem.\n\n" +
                urgencyNote;

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            string lootTag = triageActionsTaken == 0
                ? "<b><color=#F59E0B>[LOOT QUICKLY & MOVE]</color> Work the zippers fast and strip the pack</b> <color=#EF4444>(-10 Humanity)</color>"
                : "<b><color=#10B981>[LOOT QUICKLY & MOVE]</color> Work the zippers fast and strip the pack</b>";

            stage.SetChoice(choiceIdx++, lootTag, () =>
            {
                pack_resolved = true;
                if (player != null)
                {
                    if (triageActionsTaken == 0)
                    {
                        player.humanity = Mathf.Max(0, player.humanity - 10);
                    }
                    player.SetFlag("looted_lobby_pack", true);
                }

                stage.GiveOrDropItem("First Aid Kit", 1);
                stage.GiveOrDropItem("9mm Ammo", 12);
                stage.GiveOrDropItem("Painkillers", 1);
                stage.RefreshAllPDAPanels();
                stage.UpdateVitalsUI();

                stage.PlaySFX("Audio/SFX/elbow_smash_glass", 0.55f);
                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:53", "LOBBY // MILITARY PACK SECURED",
                    "Recovered First Aid Kit, Painkillers, and 9mm Ammo from the fallen National Guard courier as a barricade rifle shot freed my wrist.");

                lobbyPhase = "pack_grabbed";
                InspectLobby();
            }, true);

            stage.SetChoice(choiceIdx++, "[STEP BACK] Leave the corpse untouched for now", () =>
            {
                stage.PlayClickSFX();
                lobbyPhase = triageActionsTaken == 0 ? "entry" : "center_triage";
                InspectLobby();
            }, true);
        }

        private void RenderPackGrabbed(ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("n_room", "LOBBY // LOOTING THE DEAD");
            stage.PlayStageAnimation("LobbyBag_Anim", "Bag", 8f);
            stage.PlaySFX("Audio/SFX/zombie_snapping", 0.85f);

            string humLine = triageActionsTaken == 0 ? "<color=#EF4444><b>[-10 HUMANITY]</b></color> " : "";

            string narration = "<color=#EF4444><b>[CRACK // STAY BACK FROM THE BODIES]</b></color>\n\n" +
                $"{humLine}You drop to both knees and work the zipper by touch—pulling a <b>First Aid Kit</b>, <b>Painkillers</b>, and <b>12× 9mm rounds</b> straight into your bag without looking down.\n\n" +
                "You are yanking the shoulder straps clear when you feel it.\n\n" +
                "Fingers. Around your wrist. Ice-cold, unnaturally strong—a grip like something that has completely forgotten what letting go means.\n\n" +
                "The dead soldier is staring straight up into your face. Film-white corneas locked onto yours, chest sunken and utterly still, a wet resonant gurgle climbing out of his throat as his jaw hinges wide and his grip tightens—\n\n" +
                "<b>CRACK.</b>\n\n" +
                "A rifle round punches across the rotunda from the front barricade. The grip snaps loose. You feel warm mist cross your cheekbone before your brain even catches up. A half-second later, a second crack drops the stalker that had been closing silently behind your spine.\n\n" +
                "<i>\"STAY BACK FROM THE BODIES!\"</i> an officer's voice cracks at the high end across the marble. <i>\"I SAID STAY THE HELL BACK FROM THE BODIES—WHAT IS WRONG WITH YOU!\"</i>\n\n" +
                "He's still aiming at your chest, muzzle tracking you as you rise, hands visibly unsteady. You hold his eyes for one flat second, sling the pack over your shoulder, and move.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            stage.SetChoice(choiceIdx++, "[CHECK YOUR SURROUNDINGS] Pull back into the center rotunda", () =>
            {
                stage.PlayClickSFX();
                FinishCrisisBeat();
            }, true);
        }

        #endregion

        #region Phase 5: Beat 2 Triage Hub (Center Rotunda)

        private void FinishCrisisBeat(string carryFeedback = null)
        {
            triageActionsTaken++;
            if (triageActionsTaken >= 2)
            {
                ResolveIgnoredLobbyCrises();
                lobbyPhase = "barricade_approach";
            }
            else
            {
                lobbyPhase = "center_triage";
            }
            InspectLobby(carryFeedback);
        }

        private void ResolveIgnoredLobbyCrises()
        {
            var player = stage.CurrentPlayer;
            if (!vane_resolved)
            {
                vane_resolved = true;
                vane_status = "dead";
                player?.SetFlag("vane_dead", true);
            }
            if (!maya_resolved)
            {
                maya_resolved = true;
                maya_status = "dead";
                player?.SetFlag("maya_dead", true);
            }
        }

        private void RenderCenterTriage(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("c_room", "LOBBY // CENTER ROTUNDA (BEAT 2)");
            stage.PlayStageAnimation("LobbyMain_Anim", "Lobby_Main", 8f);

            bool fastBuild = player != null && (player.agi >= 5 || player.HasTrait("RUNNER") || player.HasTrait("ADRENALINE"));
            bool canTakeSecondBeat = fastBuild || extraBeatUnlocked;

            string vaneShadow = vane_status == "joined"
                ? "\n\nVane keeps pace just off your right shoulder. He's shifted his grip on the Pelican case—holding it flat by the body now like a shield instead of a bat. He doesn't say a word. He tracks where your eyes go. He is learning."
                : "";

            if (canTakeSecondBeat)
            {
                string beatTag = extraBeatUnlocked
                    ? "<color=#F59E0B><b>[SECOND WIND / DECOY BOUGHT ONE MORE BEAT]</b></color>"
                    : "<color=#10B981><b>[REFLEX ADVANTAGE (AGI 5 / RUNNER) — STILL TIME FOR ONE MORE MOVE]</b></color>";

                string narration = "<color=#38BDF8><b>[LOBBY ROTUNDA // TWO SECONDS TO READ THE ROOM]</b></color>\n\n" +
                    "You pull back to the center of the lobby, lungs working hard, pulse loud in your temples. The marble underfoot is slick in ways you are actively trying not to think about.\n\n" +
                    "You stop for two seconds—no more—and take the whole room in: location, distance, threat, closing exits. Old reflex. Useful one.\n\n" +
                    $"{beatTag} You know what is left and where it is—one last move before the street breach swallows the room.{vaneShadow}";

                stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);
                AddUnresolvedCrisisChoices(ref choiceIdx, isSecondBeat: true);
            }
            else
            {
                string narration = "<color=#EF4444><b>[LOBBY ROTUNDA // WINDOW CLOSING]</b></color>\n\n" +
                    "You drag yourself back toward the center chandelier, chest burning from the 2nd-floor fight and the sprint across the stone. More silhouettes are pouring in from the smashed east vestibule.\n\n" +
                    "Without faster legs, crossing this floor a second time will cut off your angle to the front doors—unless you burn every ounce of stamina left in your body or throw something loud enough to turn their heads.{vaneShadow}";

                stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

                stage.SetChoice(choiceIdx++, "<b><color=#F59E0B>[BURN THE MUSCLE]</color> Push past exhaustion for ONE MORE SPRINT!</b> <color=#EF4444>(-15 Stamina)</color>", () =>
                {
                    stage.ExertStamina(15);
                    stage.PlaySFX("Audio/SFX/boots_run_concrete", 0.85f);
                    extraBeatUnlocked = true;
                    InspectLobby("<color=#F59E0B><b>[FORCED SECOND BEAT (-15 STAMINA)]</b></color> You swallow the metallic taste in your throat and drive forward again—buying one final move before the lobby collapses!");
                }, true);

                if (player != null && player.HasItem("Bluetooth Speaker"))
                {
                    stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[DISTRACTION: BLUETOOTH SPEAKER]</color> Hurl the speaker across the mezzanine!</b> <color=#10B981>(Buys Free 2nd Beat)</color>", () =>
                    {
                        player.RemoveItem("Bluetooth Speaker", 1);
                        stage.RefreshAllPDAPanels();
                        stage.PlaySFX("Audio/SFX/bestsongeversample", 0.4f);
                        extraBeatUnlocked = true;
                        InspectLobby("<color=#38BDF8><b>[AUDIO DECOY DEPLOYED]</b></color> Just like Unit 401 upstairs—you flick the max-volume switch and send the speaker skittering across the far stairs! Half a dozen snarling heads whip toward the sudden wall of bass, clearing your lane!");
                    }, true);
                }

                if (player != null && player.HasItem("Ceramic Mug"))
                {
                    stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[DISTRACTION: CERAMIC MUG]</color> Shatter the mug near the east planters!</b> <color=#10B981>(Buys Free 2nd Beat)</color>", () =>
                    {
                        player.RemoveItem("Ceramic Mug", 1);
                        stage.RefreshAllPDAPanels();
                        stage.PlaySFX("Audio/SFX/elbow_smash_glass", 0.65f);
                        extraBeatUnlocked = true;
                        InspectLobby("<color=#38BDF8><b>[ACOUSTIC DECOY]</b></color> Heavy stoneware explodes against the east marble wall like a gunshot, snapping nearby stalkers away from the center floor!");
                    }, true);
                }

                stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[CROSS TO THE FRONT DOORS]</color> Move for the police barricade while the lane is still open</b>", () =>
                {
                    stage.PlayClickSFX();
                    ResolveIgnoredLobbyCrises();
                    lobbyPhase = "barricade_approach";
                    InspectLobby();
                }, true);
            }
        }

        #endregion

        #region Phase 6: Quarantine Police Barricade Standoff & Act 2 Transition

        private void RenderBarricadeApproach(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("lobby_exit", "LOBBY // CROSSING TO THE FRONT DOORS");
            stage.PlayStageAnimation("LobbyPolice_Anim", "Police_barricade", 8f);
            stage.PlaySFX("Audio/SFX/sirens", 0.45f);

            string companionNotes = "";
            if (vane_status == "joined")
            {
                companionNotes += "\n\nVane keeps close behind your right shoulder, Pelican case held tight at his ribs, willing every muscle in his face to look like this is fine. He is not fine. He is doing a credible impression of fine.";
            }
            if (maya_status == "rescued")
            {
                companionNotes += "\n\nBehind the luggage carts, Maya is crouched low with Lily tucked beneath her coat. She looks up when she sees you coming—relief, guilt, and gratitude all occupying the same tight breath. Lily keeps her eyes lowered.";
            }

            string tragedyNote = (vane_status == "dead" || maya_status == "dead")
                ? "\n\nBehind you across the open floor, someone screams—sharp and particular—and then stops. You do not look back. Your boots know the distance and slow for half a step anyway, registering the weight of it. These two armed officers watched it happen from twenty feet away with clear sight lines. They know that you know that."
                : "";

            bool hasCarKeys = player != null && player.HasItem("Car Keys");
            string carKeysEcho = hasCarKeys
                ? "\n\nIn your coat pocket, the cold metal teeth of the Unit 401 sedan keys press against your palm—the one promise of escape left once you clear those glass doors."
                : "";

            bool spotsBite = player != null && (player.per >= 5 || GetNormalizedJob(player) == "doctor" || GetNormalizedJob(player) == "police");
            string biteHint = (!warned_maya_cop && spotsBite)
                ? "\n\n<color=#38BDF8><i>[OBSERVATION] At ten feet, you see why the younger officer is shaking so badly: a rag tied tight around his left forearm is soaked black, and cold sweat sheets his grey face.</i></color>"
                : "";

            string narration = "<color=#F59E0B><b>[TEN FEET FROM THE STREET // THE QUARANTINE BARRICADE]</b></color>\n\n" +
                "Through the towering glass front doors lies Seattle—lit, distant chaos, smoke rising somewhere upwind, open air. Exits that are not this building. The chandeliers behind you are still glowing warm amber. The lobby is technically still beautiful. Somehow that makes it worse.\n\n" +
                "Heavy chains lock the glass from the <i>inside</i>. Blocking the threshold behind upturned luggage carts stand two Seattle PD officers.\n\n" +
                "The older one is maybe forty—military posture performing authority the way an actor performs authority, technically correct and utterly hollow behind the eyes. Beside him, a rookie in his mid-twenties has given up performing anything at all; his hands haven't stopped shaking since he raised his rifle. Both barrels point roughly at your chest." +
                companionNotes + tragedyNote + carKeysEcho + biteHint;

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : narration);

            stage.SetChoice(choiceIdx++, "<b><color=#F59E0B>[WALK UP TO THE BARRICADE]</color> Confront the officers holding the locks</b>", () =>
            {
                stage.PlayClickSFX();
                lobbyPhase = "barricade_standoff";
                InspectLobby();
            }, true);
        }

        private void RenderBarricadeStandoff(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("lobby_exit", "LOBBY // THE FRONT DOORS");
            stage.PlayStageAnimation("LobbyPolice_Anim", "Police_barricade", 8f);

            string defaultNarration = "<color=#EF4444><b>[ARMED QUARANTINE // NOBODY LEAVES]</b></color>\n\n" +
                "<b>Older Officer:</b> <i>\"Hold it right there!\"</i> His voice has the flattened cadence of someone reciting protocol while his brain scrambles to catch up. <i>\"Building is on full lockdown. Nobody in, nobody out. Quarantine protocol. Step back from the doors!\"</i>\n\n" +
                "<b>You:</b> <i>\"You're sealing us in here with those things.\"</i> Not an argument—just a fact stated at normal volume.\n\n" +
                "<b>Rookie Officer:</b> <i>\"Those are our orders. Please stand down and step away from the glass.\"</i> He means the <i>please</i>. That is almost worse.\n\n" +
                "Somewhere behind you in the lobby, glass breaks, and something screams and stops. The rookie's eyes snap toward the sound and snap right back to you, fast. There is a way through this. There is always a way through.";

            stage.SetSituationNarration(!string.IsNullOrEmpty(actionFeedback) ? actionFeedback : defaultNarration);

            string job = GetNormalizedJob(player);
            bool isPolice = job == "police";
            bool hasPasskey = player != null && player.HasItem("Tier-1 Military Clearance Passkey");

            // 1. Police Officer Badge Authority
            if (isPolice)
            {
                stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[POLICE OFFICER]</color> Flip your badge open and pull rank on them!</b>", () =>
                {
                    barricade_resolution = "badge";
                    stage.PlayClickSFX();
                    lobbyPhase = "barricade_unlocked";
                    InspectLobby("<color=#38BDF8><b>[BADGE AUTHORITY // STAND DOWN]</b></color>\n\n" +
                        "You flip your Seattle PD shield open where both officers can read it without squinting. A small square of stillness settles inside the ten feet between you.\n\n" +
                        "<i>\"I'm on the job,\"</i> you say evenly, letting your eyes flick once toward the blood in the center of the lobby and back. <i>\"Civilians were dying twenty feet from your position. Twenty feet. You had weapons, you had cover, and you ran procedure while people died in front of you. You want to put that in a report, or are you unlocking this chain right now?\"</i>\n\n" +
                        "The older officer flushes red from his collar upward—not anger, shame, which is far more useful right now. Training overrides panic: <i>\"Yes, Officer. I'm sorry—opening the lock.\"</i>");
                }, true, new Color(0.25f, 0.72f, 0.96f));
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<s>[POLICE OFFICER] Pull rank with your badge</s> <color=#64748B><i>[Requires Police Officer — You are a civilian]</i></color>", null, false);
            }

            // 2. Prologue Terminal Passkey
            if (hasPasskey)
            {
                stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[TIER-1 CLEARANCE PASSKEY]</color> Flash your cached Code Amber-9 municipal passkey!</b>", () =>
                {
                    barricade_resolution = "passkey";
                    stage.PlayClickSFX();
                    lobbyPhase = "barricade_unlocked";
                    InspectLobby("<color=#38BDF8><b>[CODE AMBER-9 CLEARANCE // OVERRIDE]</b></color>\n\n" +
                        "You angle your cracked phone screen into the chandelier light and show the active Tier-1 Municipal Checkpoint Passkey pulled from the fiber node upstairs.\n\n" +
                        "<i>\"Code Amber-9,\"</i> you say flatly. <i>\"All bridges and freeway on-ramps seal hard at 01:00. Containment inside this building is already gone, and nobody is sending relief back for two patrolmen trapped in a lobby. Unlock the doors while you can still leave.\"</i>\n\n" +
                        "The older officer's eyes jump from the clearance key to the burning street beyond the glass. His grip slackens an inch. <i>\"Damn it... Go. Fast. I'm chaining it behind you.\"</i>");
                }, true, new Color(0.25f, 0.72f, 0.96f));
            }

            // 3. Charisma / Logic / Debater
            if (!barricade_talk_failed)
            {
                int reqCha = (maya_status == "rescued" || vane_status == "joined") ? 4 : 5;
                bool hasTalkBonus = player != null && (player.cha >= reqCha || player.HasTrait("DEBATER") || player.HasTrait("CRISIS NEGOTIATOR"));
                string modifierHint = (maya_status == "rescued" || vane_status == "joined") ? " (Survivors Present Bonus)" : "";

                stage.SetChoice(choiceIdx++, $"<color=#10B981>[CHA {player?.cha ?? 3}/{reqCha}{modifierHint} / DEBATER]</color> Reason with them calmly—appeal to their math", () =>
                {
                    stage.PlayClickSFX();
                    if (hasTalkBonus)
                    {
                        barricade_resolution = "charisma";
                        lobbyPhase = "barricade_unlocked";
                        InspectLobby("<color=#10B981><b>[CHARISMA — SUCCESS]</b></color>\n\n" +
                            "You raise both hands slowly, keeping your voice level and conversational—like this is still a reasonable talk between reasonable people.\n\n" +
                            "<i>\"Here's the problem with what you're doing right now: your perimeter is already breached. You're two men. Every extra civilian trapped inside this lobby is one more liability you have to watch—and you've already seen how that math turns out. I walk out that door, and your job gets smaller by exactly the amount of me.\"</i>\n\n" +
                            "A long, exhausted look passes between them. The rookie's rifle drops first before the older officer even finishes nodding. <i>\"Fine. Go. Fast—I'm locking it the second you're through.\"</i>");
                    }
                    else
                    {
                        barricade_talk_failed = true;
                        stage.PlaySFX("Audio/SFX/doorclosed1", 0.75f);
                        InspectLobby("<color=#EF4444><b>[CHARISMA — FAILED]</b></color>\n\n" +
                            "You lay out the logic cleanly—the breached perimeter, the math, every angle you can find. They are genuinely good points.\n\n" +
                            "The older officer shakes his head and talks straight over you: <i>\"I don't care. Nobody leaves. Those are the orders—I don't write them, I follow them.\"</i> He steps forward, making himself larger the way trained officers do when they want someone to shut up. <i>\"Step away from the glass. Last warning.\"</i>");
                    }
                }, true);
            }

            // 4. Intimidation (STR 6 / Melee 2 / Blunt Bruiser / Commanding Presence)
            bool canIntimidate = player != null && (player.str >= 6 || player.melee >= 2 || player.HasTrait("BLUNT BRUISER") || player.HasTrait("COMMANDING PRESENCE"));
            if (canIntimidate)
            {
                stage.SetChoice(choiceIdx++, "<b><color=#F59E0B>[STR 6 / MELEE 2 / BRUISER]</color> Walk straight into the rifle barrel and stare him down</b>", () =>
                {
                    barricade_resolution = "intimidate";
                    stage.PlayClickSFX();
                    lobbyPhase = "barricade_unlocked";
                    InspectLobby("<color=#F59E0B><b>[INTIMIDATION — STAND DOWN]</b></color>\n\n" +
                        "You don't rush it. You just walk at a steady pace until the cold muzzle presses firmly against your sternum and stops you. The officer flinches; you don't. People who are actually going to pull a trigger don't advertise it like this.\n\n" +
                        "You let two full seconds of silence breathe before speaking. <i>\"You stood there. You had a loaded weapon and watched people die twenty feet away. You aren't going to shoot me—we both know that. Open the door.\"</i>\n\n" +
                        "The rookie takes a small step backward without meaning to. Something in the veteran's posture deflates like a slow leak. <i>\"...Fine.\"</i> The rifle dips. <i>\"Get out. I'm not opening it again.\"</i>");
                }, true);
            }
            else
            {
                stage.SetChoice(choiceIdx++, "<s>[STR 6 / MELEE 2] Step into the barrel and stare him down</s> <color=#64748B><i>[Requires Might 6 or Melee 2]</i></color>", null, false);
            }

            // 5. Physical Force Fallback (Always available)
            stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[FORCE PAST BARRICADE]</color> Grip the rifle barrel, shove it clear, and shoulder the doors!</b> <color=#EF4444>(-5 Stamina)</color>", () =>
            {
                barricade_resolution = "force";
                stage.ExertStamina(5);
                stage.PlaySFX("Audio/SFX/metal_shelf_crash", 0.7f);
                lobbyPhase = "barricade_unlocked";
                InspectLobby("<color=#EF4444><b>[FORCED BREACH (-5 STAMINA)]</b></color>\n\n" +
                    "You are done talking. You just walk straight in, take hold of the rifle barrel with one steady hand—not snatching, just forcing it firmly aside.\n\n" +
                    "The older officer stumbles back half a step in shock deeper than muscle memory. For one completely frozen second his finger stays on the trigger as the barrel swings harmlessly past the frame, then completes its arc downward while he stares at you.\n\n" +
                    "You put your shoulder straight into the glass door; the old frame yields under steady pressure, popping the latch into the cold open air!");
            }, true, new Color(0.95f, 0.25f, 0.25f));
        }

        private void RenderExitUnlocked(CharacterData player, ref int choiceIdx, string actionFeedback)
        {
            stage.SetLocation("lobby_exit", "ASCOTT EXIT // INTO THE STREETS");
            stage.PlayStageAnimation("LobbyExit_Anim", "Lobby_exit", 8f);
            stage.PlaySFX("Audio/SFX/firedoor_slam", 0.65f);

            string companionExitNote = vane_status == "joined"
                ? "\n\nVane slips through the swinging glass a half-second behind you and stops beside you on the wet sidewalk, blinking in the open air. He looks back at the glowing lobby, then at you, breathing out very slowly through his nose.\n\n<b>Vane:</b> <i>\"Right. Okay. Still alive.\"</i> He shifts his grip on his Pelican camera case. <i>\"What now?\"</i>"
                : "\n\nThe heavy glass door swings shut behind you, dropping the screaming inside off like a switch being thrown. Ahead lies an avenue swallowed by sirens and panic.";

            string narration = (!string.IsNullOrEmpty(actionFeedback) ? actionFeedback + "\n\n" : "") +
                "<color=#10B981><b>[ACT 1 COMPLETE // ESCAPED THE ASCOTT RESIDENCY]</b></color>\n\n" +
                "Freezing Seattle rain hits your face the moment you cross the threshold. Behind you, Unit 407, the quiet tragedy on the 4th Floor, David Keller's corridor, and the gilded slaughterhouse of the Grand Lobby vanish behind chained glass." +
                companionExitNote;

            stage.SetSituationNarration(narration);

            stage.SetChoice(choiceIdx++, "<b><color=#38BDF8>[DON'T LOOK BACK]</color> Step into the Seattle streets — Begin Act 2</b>", () =>
            {
                stage.PlayClickSFX();
                stage.PlayHubBGM("Audio/BGM/city_riot_heavy", 0.55f, true);
                stage.Objectives?.CompleteObjective("escape_lobby");
                stage.Objectives?.AddObjective("escape_city", "Survive the Seattle street stampede and find a vehicle to escape the city", ObjectiveStatus.Active);

                stage.AppendStoryLog(stage.Clock?.GetTimeString() ?? "00:58", "ACT 1 COMPLETE // LEFT ASCOTT RESIDENCY",
                    $"Escaped the Ground Floor Lobby via {barricade_resolution} resolution. Vane: {vane_status} | Maya & Lily: {maya_status}.");

                isInLobby = false;
                lobbyPhase = "act2_exit";

                var act2Passage = Resources.Load<StoryPassageData>("StoryPassages/Passage_Act2_TheStreet");
                if (act2Passage != null)
                {
                    stage.PlayStoryPassage(act2Passage);
                }
                else
                {
                    stage.SetLocation("lobby_exit", "ACT 2 // THE STREETS OF SEATTLE");
                    stage.SetSituationNarration("<color=#38BDF8><b>[ACT 2 // THE STREETS]</b></color>\n\n" +
                        "You step onto the rain-slicked asphalt as panicking crowds surge past burning cars toward the highway. Your escape out of Seattle begins now.");
                    stage.ClearChoices();
                    stage.ReindexChoices();
                }
            }, true, new Color(0.25f, 0.72f, 0.96f));
        }

        #endregion

        #region Swift Weapon-Reactive Rescues (No Combat Modal)

        private WeaponProfile GetEquippedWeaponProfile(CharacterData player)
        {
            var profile = new WeaponProfile();
            if (player == null || string.IsNullOrEmpty(player.equippedWpn) ||
                string.Equals(player.equippedWpn, "none", StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }

            profile.itemId = player.equippedWpn;
            var def = ItemDatabase.GetDefinition(player.equippedWpn);
            if (def == null) return profile;

            if (def.category == ItemCategory.RangedWeapon)
            {
                bool isBow = string.Equals(def.id, "Compound Bow", StringComparison.OrdinalIgnoreCase);
                profile.isBow = isBow;
                profile.isFirearm = !isBow;
                profile.ammoType = def.ammoType;
                if (!string.IsNullOrEmpty(def.ammoType))
                {
                    var ammoEntry = player.inventory?.Find(i => string.Equals(i.itemId, def.ammoType, StringComparison.OrdinalIgnoreCase));
                    profile.ammoCount = ammoEntry != null ? ammoEntry.quantity : 0;
                    profile.hasAmmo = profile.ammoCount > 0;
                }
            }
            else if (def.category == ItemCategory.MeleeWeapon)
            {
                bool blade = def.id.IndexOf("Knife", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             def.id.IndexOf("Scalpel", StringComparison.OrdinalIgnoreCase) >= 0;
                profile.isBlade = blade;
                profile.isBlunt = !blade;
            }

            return profile;
        }

        private void AddSwiftRescueChoices(ref int choiceIdx, bool isVaneRescue, CharacterData player)
        {
            var wpn = GetEquippedWeaponProfile(player);
            bool vaneAssisted = !isVaneRescue && vane_status == "joined";
            int statRed = vaneAssisted ? 1 : 0;
            string surface = isVaneRescue ? "brass elevator door frame" : "edge of the marble reception desk";
            bool hasAnyCleanRescue = false;

            // Path 1: Firearm Swift Execution
            if (wpn.isFirearm && wpn.hasAmmo)
            {
                int reqPer = Mathf.Max(1, 4 - statRed);
                bool canShoot = player != null && (player.shooting >= 1 || player.per >= reqPer || player.HasTrait("GUNSLINGER"));
                if (canShoot)
                {
                    hasAnyCleanRescue = true;
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[FIREARM: {wpn.itemId}]</color> Fire warning shot into ceiling & execute at point-blank!</b> <color=#9CA3AF>(-2 {wpn.ammoType}, Instant Save)</color>", () =>
                    {
                        int spent = Mathf.Min(2, wpn.ammoCount);
                        player?.RemoveItem(wpn.ammoType, spent);

                        string prose = isVaneRescue
                            ? $"<color=#10B981><b>[FIREARM EXECUTION // +25 XP]</b></color>\n\nYou come in from the creature's left with your <b>{wpn.itemId}</b> raised. One sharp round into the marble ceiling above its head—the crack in the vaulted lobby freezes it mid-lunge. You use that half-second completely: close the final distance, press the barrel hard against its temple, and squeeze. It drops straight down."
                            : $"<color=#10B981><b>[FIREARM EXECUTION // +25 XP]</b></color>\n\nOne round from your <b>{wpn.itemId}</b> cracks into the ceiling directly above the creature's head. It stops completely. You vault the reception counter in that second, press the muzzle to the back of its skull, and pull the trigger once. It drops across the counter like cut strings.";

                        CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/distant_explosion", 0.5f);
                    }, true, new Color(0.2f, 0.85f, 0.45f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[FIREARM: {wpn.itemId}] Clean point-blank shot</s> <color=#64748B><i>[Requires Shooting 1 or Observation {reqPer} — Target is too close to the hostage]</i></color>", null, false);
                }
            }

            // Path 2: Compound Bow Swift Execution
            if (wpn.isBow && wpn.hasAmmo)
            {
                int reqAgi = Mathf.Max(1, 4 - statRed);
                bool canBow = player != null && (player.shooting >= 1 || player.agi >= reqAgi || player.HasTrait("ARCHER"));
                if (canBow)
                {
                    hasAnyCleanRescue = true;
                    bool keepArrow = player != null && (player.HasTrait("ARCHER") || player.HasTrait("SILENT HUNTER"));
                    string ammoLabel = keepArrow
                        ? "<color=#38BDF8>(Silent, Arrow Retrieved Intact)</color>"
                        : $"<color=#9CA3AF>(-1 {wpn.ammoType}, Instant Save)</color>";
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[BOW: {wpn.itemId}]</color> Loose a silent broadhead straight into the brainstem!</b> {ammoLabel}", () =>
                    {
                        if (!keepArrow)
                        {
                            player?.RemoveItem(wpn.ammoType, 1);
                        }
                        string arrowNote = keepArrow
                            ? "\n\n<color=#38BDF8><b>[ARCHER // +1 Arrow Retrieved]</b></color> You brace one boot against the stone and wrench your carbon broadhead clean back into your quiver."
                            : "";

                        string prose = (isVaneRescue
                            ? $"<color=#10B981><b>[SILENT BROADHEAD // +25 XP]</b></color>\n\nYou draw your <b>{wpn.itemId}</b> smoothly as your boots hit stillness. One clean breath—the carbon shaft punches silently through the base of its skull, pinning it motionless against the brass elevator seam."
                            : $"<color=#10B981><b>[SILENT BROADHEAD // +25 XP]</b></color>\n\nFrom the corner of the rotunda, you draw your <b>{wpn.itemId}</b> and loose cleanly over the marble partition. The broadhead punches into the creature's cervical spine before it can lunge at Maya and Lily, dropping it silently behind the desk.") + arrowNote;

                        CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/body_fall_wet", 0.70f);
                    }, true, new Color(0.2f, 0.85f, 0.45f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[BOW: {wpn.itemId}] Precision brainstem shot</s> <color=#64748B><i>[Requires Shooting 1 or Reflex {reqAgi}]</i></color>", null, false);
                }
            }

            // Path 3: Blade Swift Execution
            if (wpn.isBlade)
            {
                int reqAgi = Mathf.Max(1, 4 - statRed);
                bool canSlice = player != null && (player.melee >= 1 || player.agi >= reqAgi || player.HasTrait("BLADESMITH"));
                if (canSlice)
                {
                    hasAnyCleanRescue = true;
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[BLADE: {wpn.itemId}]</color> Close the gap without slowing & sever the cervical spine!</b> <color=#9CA3AF>(Silent, Instant Save)</color>", () =>
                    {
                        string prose = isVaneRescue
                            ? $"<color=#10B981><b>[BLADE EXECUTION // +25 XP]</b></color>\n\nYou close the gap without slowing. Your <b>{wpn.itemId}</b> finds the back of the creature's neck before it registers you are there. It seizes in a full-body spasm and buckles downward, taking the blade with it. You plant your boot flat on its shoulder, brace, and wrench the steel free."
                            : $"<color=#10B981><b>[BLADE EXECUTION // +25 XP]</b></color>\n\nYou clear the marble counter in one motion, and your <b>{wpn.itemId}</b> catches the creature's spine before it senses you behind it. It wheels blindly—buying Maya the separation she needs—before you drive the blade home at the base of its skull.";

                        CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/body_fall_wet", 0.72f);
                    }, true, new Color(0.2f, 0.85f, 0.45f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[BLADE: {wpn.itemId}] Precision cervical strike</s> <color=#64748B><i>[Requires Melee 1 or Reflex {reqAgi}]</i></color>", null, false);
                }
            }

            // Path 4: Blunt Swift Execution
            if (wpn.isBlunt)
            {
                int reqStr = Mathf.Max(1, 4 - statRed);
                bool canSmash = player != null && (player.melee >= 1 || player.str >= reqStr || player.HasTrait("BLUNT BRUISER") || player.HasTrait("BASEBALL"));
                if (canSmash)
                {
                    hasAnyCleanRescue = true;
                    stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[BLUNT: {wpn.itemId}]</color> Full-stride overhead smash into the {surface}!</b> <color=#9CA3AF>(Instant Save)</color>", () =>
                    {
                        string prose = isVaneRescue
                            ? $"<color=#10B981><b>[BLUNT TAKEDOWN // +25 XP]</b></color>\n\nYou do not break stride. Your <b>{wpn.itemId}</b> is already drawn back over your shoulder before you reach it. The impact when it connects is dense and terrible, slamming the creature sideways into the brass elevator frame. You bring it down once more, harder, and it stops moving."
                            : $"<color=#10B981><b>[BLUNT TAKEDOWN // +25 XP]</b></color>\n\nYou hook around the side of the reception counter rather than vaulting it and catch the creature mid-turn—driving your <b>{wpn.itemId}</b> squarely into the back of its skull. The crack is the loudest sound in the lobby; it folds across the marble desk and stays down.";

                        CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/skull_crush_heavy", 0.80f);
                    }, true, new Color(0.2f, 0.85f, 0.45f));
                }
                else
                {
                    stage.SetChoice(choiceIdx++, $"<s>[BLUNT: {wpn.itemId}] Crushing overhead strike</s> <color=#64748B><i>[Requires Melee 1 or Might {reqStr}]</i></color>", null, false);
                }
            }

            // Path 5: Unarmed Brawn Takedown
            int reqBrawn = Mathf.Max(1, 6 - statRed);
            bool hasUnarmedBrawn = player != null && (player.str >= reqBrawn || player.melee >= 2 || player.HasTrait("BOXER"));
            if (hasUnarmedBrawn)
            {
                hasAnyCleanRescue = true;
                stage.SetChoice(choiceIdx++, $"<b><color=#10B981>[UNARMED BRAWN]</color> Hit it at a dead sprint, pin it to the marble, and crush its skull!</b> <color=#10B981>(-5 Stamina, Instant Save)</color>", () =>
                {
                    stage.ExertStamina(5);
                    string prose = isVaneRescue
                        ? "<color=#10B981><b>[UNARMED TAKEDOWN (-5 STAMINA) // +25 XP]</b></color>\n\nYou hit it at a dead sprint—wrapping both arms around its midsection and driving it off Vane into the marble wall. You take the top, pin its throat with your knees, and drive your elbow down onto its skull until your arm goes numb and it stops moving."
                        : "<color=#10B981><b>[UNARMED TAKEDOWN (-5 STAMINA) // +25 XP]</b></color>\n\nYou vault straight over the reception desk and haul the creature backward by the collar—buying Maya separation—before slamming its skull face-first against the solid marble edge until it goes completely slack.";

                    CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/skull_crush_heavy", 0.80f);
                }, true, new Color(0.2f, 0.85f, 0.45f));
            }

            // Path 6A: Fail-Forward Personal Sacrifice (Always available)
            stage.SetChoice(choiceIdx++, "<b><color=#F59E0B>[DESPERATE BODILY INTERVENTION]</color> Throw yourself between the jaws and wrench it into the stone!</b> <color=#EF4444>(-15 HP, -10 Stamina — Guaranteed Save)</color>", () =>
            {
                stage.TakeDamage(15, isVaneRescue ? "Clawed while pulling the elevator infected off Vane" : "Clawed while shielding Maya and Lily at Reception");
                stage.ExertStamina(10);

                string targetDesc = isVaneRescue ? "Vane" : "Maya and Lily";
                string prose = $"<color=#F59E0B><b>[SAVED AT PERSONAL COST (-15 HP, -10 STAMINA) // +25 XP]</b></color>\n\n" +
                    $"You have neither the clean angle nor the leverage, so you spend your own body instead. Throwing your forearm straight across the snapping jaw to pry it away from {targetDesc}, splinters of nail and teeth rake bloody furrows across your sleeve before you slam its head repeatedly into the {surface} until it finally stops twitching.";

                CompleteSwiftRescue(isVaneRescue, prose, "Audio/SFX/blood_drip", 0.75f);
            }, true);

            // Path 6B: Authentic Failure Option when player lacks clean weapon/brawn qualifications
            if (!hasAnyCleanRescue)
            {
                stage.SetChoice(choiceIdx++, "<b><color=#EF4444>[RECKLESS LUNGE — RISK FAILURE]</color> Attack without a clean angle or leverage</b> <color=#EF4444>(-10 Stamina — Rescue Risks Failing)</color>", () =>
                {
                    ExecuteRecklessFailure(isVaneRescue, wpn);
                }, true, new Color(0.95f, 0.25f, 0.25f));
            }
        }

        private void CompleteSwiftRescue(bool isVaneRescue, string prose, string sfxPath, float sfxVol = 0.65f)
        {
            stage.CurrentPlayer?.GainXP(25);
            stage.PlaySFX(sfxPath, sfxVol);
            stage.RefreshAllPDAPanels();
            stage.UpdateVitalsUI();
            lobbyPhase = isVaneRescue ? "vane_aftermath" : "maya_aftermath";
            InspectLobby(prose);
        }

        private void ExecuteRecklessFailure(bool isVaneRescue, WeaponProfile wpn)
        {
            MarkRescueFailed(isVaneRescue);
            stage.PlaySFX("Audio/SFX/distant_scream3", 0.7f);

            string detail;
            if (wpn.isFirearm)
            {
                detail = isVaneRescue
                    ? $"You raise your <b>{wpn.itemId}</b> and the geometry is immediately wrong—Vane is pinned directly behind the creature against the elevator doors. You sidestep searching for a clear line. That half-second decides everything; by the time you find an angle, there is nothing left on the other side you need to protect."
                    : $"You raise your <b>{wpn.itemId}</b> and the geometry is immediately wrong—Maya and the little girl are directly behind the creature. Every sight line runs through the wrong bodies. By the time you accept there is no clean shot, what is happening against the wall has already happened.";
            }
            else if (wpn.isBow)
            {
                detail = isVaneRescue
                    ? $"You draw your <b>{wpn.itemId}</b>, hunting for a clean gap—Vane twists behind the creature at the exact wrong instant. Your arrow sparks uselessly off brushed steel, and the moment closes forever."
                    : $"You try to clear a shot with your <b>{wpn.itemId}</b> across the reception desk, but Maya's shoulder blocks the angle until the creature already has its teeth sunk past her palm.";
            }
            else if (wpn.isBlade)
            {
                detail = isVaneRescue
                    ? $"You lunge with your <b>{wpn.itemId}</b> and the creature twists at the exact wrong moment. Your steel finds empty air, then shrieks across the brushed elevator metal. By the time you recover your footing, the struggle against the doors has stopped."
                    : $"You slash with your <b>{wpn.itemId}</b> as the creature rounds the reception edge and catch its forearm—a real cut, meaningless to something that doesn't feel pain. It drives straight past your blade behind the counter.";
            }
            else if (wpn.isBlunt)
            {
                detail = isVaneRescue
                    ? $"You swing your <b>{wpn.itemId}</b> hard and wide as the creature lurches forward into Vane—your weapon tears through empty air and clangs off steel. When you look up, the angle of his body on the floor has already changed."
                    : $"You swing your <b>{wpn.itemId}</b> in the hardest arc you can muster, and the creature ducks low by pure animal instinct. Your weapon catches the marble corner instead, rattling bone from your wrists to your jaw.";
            }
            else
            {
                detail = isVaneRescue
                    ? "You throw yourself onto its back to haul it off him. It is stronger than anything human has a right to be—it shrugs you off across the slick marble without breaking focus. By the time you climb back up, the elevator bank has gone silent—and the silence is worse than the screaming was."
                    : "You throw yourself over the reception counter to pry it off them. It shrugs you away without even looking back; your grip peels off torn cloth and you hit the marble floor on the far side.";
            }

            string aftermath = isVaneRescue
                ? "You pull back from the elevator bank without looking down at what is on the floor. The Pelican camera case is still sitting on the marble. Just sitting there."
                : "Maya's scream cuts across the entire lobby—sharp and particular and desperate—silencing everything else for one terrible second. And then it stops. You fall back from the desk.";

            InspectLobby($"<color=#EF4444><b>[RESCUE FAILED (-10 STAMINA)]</b></color>\n\n{detail}\n\n{aftermath}");
        }

        private void MarkRescueFailed(bool isVaneRescue)
        {
            stage?.ExertStamina(10);
            var player = stage?.CurrentPlayer;
            if (isVaneRescue)
            {
                vane_resolved = true;
                vane_status = "dead";
                player?.SetFlag("vane_dead", true);
            }
            else
            {
                maya_resolved = true;
                maya_status = "dead";
                player?.SetFlag("maya_dead", true);
            }

            triageActionsTaken++;
            lobbyPhase = triageActionsTaken >= 2 ? "barricade_approach" : "center_triage";
            if (lobbyPhase == "barricade_approach")
            {
                ResolveIgnoredLobbyCrises();
            }
        }

        #endregion

        #region Act 1 Continuity, Humanity & Job Voice Helpers

        private string GetAct1JourneyEcho(CharacterData player)
        {
            var f4 = stage != null ? stage.Floor4Zone : null;
            var stairs = stage != null ? stage.EmergencyStairwellZone : null;

            string f4Memory = "";
            if (f4 != null)
            {
                if (f4.room402_opened)
                    f4Memory = "You left the Andersons holding hands on their sofa to Chopin and bleach, ";
                else if (f4.room406_opened)
                    f4Memory = "You remember Harmon's yellow note in 406—<i>living in the intermission between empires</i>—";
                else if (f4.room405_opened)
                    f4Memory = "You climbed past Marcus's shotgun tripwire in 405, ";
                else if (f4.room401_entered)
                    f4Memory = "You pulled car keys out from under a fallen party shelf upstairs, ";
            }

            string stairsMemory = (stairs != null && stairs.stair_noise_made)
                ? "tumbled down a dark blood-slicked stairwell, "
                : "crept past pooled blood in the emergency shaft, ";

            if (string.IsNullOrEmpty(f4Memory))
            {
                f4Memory = "You locked Unit 407 behind you, ";
            }

            return $"{f4Memory}{stairsMemory}and put down a dead neighbor in a rumpled business suit on the 2nd Floor—only to emerge into this. ";
        }

        private string GetHumanityLobbyThought(CharacterData player)
        {
            int hum = player != null ? player.humanity : 50;
            if (hum >= 70)
            {
                return "<color=#93C5FD><i>Your pulse hammers in your throat, but you refuse to become another coward hiding behind a rifle while neighbors get torn apart. Pick a target and move.</i></color>";
            }
            if (hum <= 30)
            {
                return "<color=#94A3B8><i>This building is already a tomb. Every second spent playing hero is a second you aren't clearing the front doors—unless someone here can actually carry their own weight outside.</i></color>";
            }
            return "<color=#93C5FD><i>You weigh distance, marble footing, and breath. You can't save the whole building, and hesitation here gets everyone killed. Choose cleanly.</i></color>";
        }

        private string GetJobLobbyObservation(string job)
        {
            switch (job)
            {
                case "police":
                    return "Your eyes snap straight to the chained glass doors: those two patrolmen aren't holding a rescue perimeter—they're panicking behind protocol.";
                case "doctor":
                    return "From the way the victims at the elevators and desk move, there's no confusion left: the pathogen isn't slowing down for pain or trauma.";
                case "thief":
                    return "Habit maps the sightlines before you finish blinking: two open kill zones across slick marble, one military pack in reach, and two nervous triggers watching the exit.";
                case "worker":
                    return "Look at the steel chains wrapped through the front door pulls—the city didn't break in here; somebody deliberately bolted everyone inside.";
                case "manager":
                    return "Two men with rifles are managing liability behind luggage carts while customers and tenants bleed out twenty feet away.";
                case "student":
                    return "An hour ago you were worried about Monday classes and rent—now crystal chandeliers shine over corpses on the floor you walked across every day.";
                default:
                    return "Nobody in uniform is coming up those stairs to save anyone.";
            }
        }

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

        #endregion

        #region ES3 Save / Load Persistence

        public void SaveZoneState(string file)
        {
            if (string.IsNullOrEmpty(file)) return;

            ES3.Save<bool>("hub_lobby_active", isInLobby, file);
            ES3.Save<string>("hub_lobby_phase", lobbyPhase ?? "entry", file);
            ES3.Save<int>("hub_lobby_actions", triageActionsTaken, file);
            ES3.Save<bool>("hub_lobby_extra_beat", extraBeatUnlocked, file);
            ES3.Save<bool>("hub_lobby_vane_resolved", vane_resolved, file);
            ES3.Save<string>("hub_lobby_vane_status", vane_status ?? "none", file);
            ES3.Save<bool>("hub_lobby_maya_resolved", maya_resolved, file);
            ES3.Save<string>("hub_lobby_maya_status", maya_status ?? "none", file);
            ES3.Save<bool>("hub_lobby_warned_maya", warned_maya_cop, file);
            ES3.Save<bool>("hub_lobby_pack_resolved", pack_resolved, file);
            ES3.Save<bool>("hub_lobby_talk_failed", barricade_talk_failed, file);
            ES3.Save<string>("hub_lobby_barricade_res", barricade_resolution ?? "", file);
        }

        public void LoadZoneState(string file)
        {
            if (string.IsNullOrEmpty(file) || !ES3.FileExists(file)) return;

            if (ES3.KeyExists("hub_lobby_active", file)) isInLobby = ES3.Load<bool>("hub_lobby_active", file);
            if (ES3.KeyExists("hub_lobby_phase", file)) lobbyPhase = ES3.Load<string>("hub_lobby_phase", file);
            if (ES3.KeyExists("hub_lobby_actions", file)) triageActionsTaken = ES3.Load<int>("hub_lobby_actions", file);
            if (ES3.KeyExists("hub_lobby_extra_beat", file)) extraBeatUnlocked = ES3.Load<bool>("hub_lobby_extra_beat", file);
            if (ES3.KeyExists("hub_lobby_vane_resolved", file)) vane_resolved = ES3.Load<bool>("hub_lobby_vane_resolved", file);
            if (ES3.KeyExists("hub_lobby_vane_status", file)) vane_status = ES3.Load<string>("hub_lobby_vane_status", file);
            if (ES3.KeyExists("hub_lobby_maya_resolved", file)) maya_resolved = ES3.Load<bool>("hub_lobby_maya_resolved", file);
            if (ES3.KeyExists("hub_lobby_maya_status", file)) maya_status = ES3.Load<string>("hub_lobby_maya_status", file);
            if (ES3.KeyExists("hub_lobby_warned_maya", file)) warned_maya_cop = ES3.Load<bool>("hub_lobby_warned_maya", file);
            if (ES3.KeyExists("hub_lobby_pack_resolved", file)) pack_resolved = ES3.Load<bool>("hub_lobby_pack_resolved", file);
            if (ES3.KeyExists("hub_lobby_talk_failed", file)) barricade_talk_failed = ES3.Load<bool>("hub_lobby_talk_failed", file);
            if (ES3.KeyExists("hub_lobby_barricade_res", file)) barricade_resolution = ES3.Load<string>("hub_lobby_barricade_res", file);
        }

        #endregion
    }
}
