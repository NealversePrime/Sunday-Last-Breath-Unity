import os
import json

base_dir = r"D:\Unity\Sunday-Last-Breath-Unity\Assets\Resources\StoryPassages\Act1"
os.makedirs(base_dir, exist_ok=True)

def get_frames(folder_name):
    anim_dir = os.path.join(r"D:\Unity\Sunday-Last-Breath-Unity\Assets\Resources\Act1", folder_name)
    if not os.path.exists(anim_dir):
        return []
    frames = []
    for f in sorted(os.listdir(anim_dir)):
        if f.endswith('.png.meta'):
            meta_path = os.path.join(anim_dir, f)
            with open(meta_path, 'r', encoding='utf-8') as mf:
                for line in mf:
                    if line.startswith('guid:'):
                        frames.append(line.strip().split()[1])
                        break
    return frames

frames_h1 = get_frames("Hall406_407_Anim")
frames_h2 = get_frames("Hall404_405_Anim")
frames_h3 = get_frames("Hall402_403_Anim")
frames_h4 = get_frames("HallEnd_Anim")
frames_u408_door = get_frames("Unit408Door_Anim")
frames_u408_in = get_frames("Unit408Inside_Anim")
frames_u405_in = get_frames("Unit405Inside_Anim")
frames_u402_in = get_frames("Unit402Inside_Anim")
frames_u401_in = get_frames("Unit401Inside_Anim")
frames_util_in = get_frames("UtilityRoom_Anim")
frames_stair_in = get_frames("Stairwell4F_Anim")

GUID_HALLWAY_BGM = "16bb303f22c95804b8505e4ab0ccf0c6"
GUID_TENSION_BGM = "818303d7eaf18a5499deb9372579a6be"
GUID_CHOPIN_BGM = "7d6112f5f34341647bab3bb1e0b837cc"
GUID_PARTY_SFX = "a3219d36b7b366948bd8eaafdb51ffc8"
GUID_UTILITY_HUM = "c1917460b7c07ad46b9e0d88a1a7a8a7"
GUID_GENERATOR_HUM = "2794956e369c942429eea4d53b73aa65"
GUID_FIREDOOR_SLAM = "421b0a9a95a97d241b55c3ff4ac54d05"
GUID_DOOR_408_VIDEO = "2a97efa723e746cdbf04e4ce829d482c"

passages_meta = {
    "Passage_Prologue_WakeUp": "01100001b5696ff4eada863cecea8452",
    "Passage_Unit407_Hub": "40700001b5696ff4eada863cecea8452",
    "Passage_Hallway_1": "3d8ebf1e8e3258543b88026777e8d1d7",
    "Passage_Hallway_North": "2d8facfab5696ff4eada863cecea8452",
    "Passage_Hallway_2": "a1102002b5696ff4eada863cecea8452",
    "Passage_Hallway_3": "a1103003b5696ff4eada863cecea8452",
    "Passage_Hallway_4": "a1104004b5696ff4eada863cecea8452",
    "Passage_Unit408_Door": "05759b3026c90954b9a12ac1fa9633a0",
    "Passage_Unit408_Inside": "18f5aa56867a7e446a64c8b657e81029",
    "Passage_Unit405_Door": "b1105001b5696ff4eada863cecea8452",
    "Passage_Unit405_Inside": "b1105002b5696ff4eada863cecea8452",
    "Passage_Unit402_Door": "c1102001b5696ff4eada863cecea8452",
    "Passage_Unit402_Inside": "c1102002b5696ff4eada863cecea8452",
    "Passage_Unit401_Inside": "d1101001b5696ff4eada863cecea8452",
    "Passage_Utility_Door": "e1100001b5696ff4eada863cecea8452",
    "Passage_Utility_Inside": "e1100002b5696ff4eada863cecea8452",
    "Passage_Stairwell_4F": "f1100001b5696ff4eada863cecea8452",
}

def escape_yaml_string(s):
    if not s:
        return "''"
    # Dump using JSON formatting so all newlines are escaped as \n on a single line!
    # This prevents any YAML indentation errors or lines accidentally starting with '<' at column 0
    return json.dumps(s, ensure_ascii=False)

def write_meta(filename, guid):
    meta_path = os.path.join(base_dir, f"{filename}.asset.meta")
    content = f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(content)

def build_passage_yaml(p):
    name = p["name"]
    pid = p.get("passageId", name)
    title = p.get("passageTitle", "")
    room_id = p.get("associatedRoomId", "")
    keep_bg = 1 if p.get("keepPreviousBackdrop", False) else 0
    music_action = p.get("musicAction", 0) # 0: KeepPlaying, 1: PlayNew, 2: StopMusic
    bgm_guid = p.get("bgm_guid", "")
    bgm_yaml = f"{{fileID: 8300000, guid: {bgm_guid}, type: 3}}" if bgm_guid else "{fileID: 0}"
    volume = p.get("musicVolume", 0.5)
    
    # Frames
    frames = p.get("frames", [])
    frames_yaml = "  animatedBackdropFrames:\n" + "\n".join([f"  - {{fileID: 21300000, guid: {g}, type: 3}}" for g in frames]) if frames else "  animatedBackdropFrames: []"
    fps = p.get("fps", 10)
    
    story_text = escape_yaml_string(p.get("storyText", ""))

    # Narrative beats
    beats = p.get("narrativeBeats", [])
    if beats:
        beats_yaml = "  narrativeBeats:\n" + "\n".join([f"  - {escape_yaml_string(b)}" for b in beats])
    else:
        beats_yaml = "  narrativeBeats: []"
    
    # Dialogue lines
    lines = p.get("dialogueLines", [])
    if lines:
        dl_yaml = "  dialogueLines:\n"
        for l in lines:
            avatar_guid = l.get("avatar_guid", "")
            avatar_yaml = f"{{fileID: 21300000, guid: {avatar_guid}, type: 3}}" if avatar_guid else "{fileID: 0}"
            req_d_flag = l.get("requiresStoryFlag", "")
            req_d_not_flag = l.get("requiresNotFlag", "")
            dl_yaml += f"""  - introNarration: {escape_yaml_string(l.get("introNarration", ""))}
    speakerName: {escape_yaml_string(l.get("speakerName", "YOU"))}
    side: {l.get("side", 0)}
    avatarSprite: {avatar_yaml}
    animatedAvatarFrames: []
    avatarFPS: 8
    dialogueText: {escape_yaml_string(l.get("dialogueText", ""))}
    outroNarration: {escape_yaml_string(l.get("outroNarration", ""))}
    requiresStoryFlag: '{req_d_flag}'
    requiresNotFlag: '{req_d_not_flag}'
"""
    else:
        dl_yaml = "  dialogueLines: []\n"
        
    # Choices
    choices = p.get("choices", [])
    if choices:
        ch_yaml = "  choices:\n"
        for c in choices:
            target_guid = passages_meta.get(c.get("targetPassage", ""), "")
            target_yaml = f"{{fileID: 11400000, guid: {target_guid}, type: 2}}" if target_guid else "{fileID: 0}"
            vid_guid = c.get("transitionVideo", "")
            vid_yaml = f"{{fileID: 32900000, guid: {vid_guid}, type: 3}}" if vid_guid else "{fileID: 0}"
            vid_path = c.get("transitionVideoPath", "")
            is_back = 1 if c.get("isBackButton", False) else 0
            time_cost = c.get("timeMinutesCost", 1)
            stamina_cost = c.get("staminaCost", 0)
            
            # Requirements & Flags
            req_flag = c.get("requiresStoryFlag", "")
            req_not_flag = c.get("requiresNotFlag", "")
            hide_unavail = 1 if c.get("hideWhenUnavailable", False) else 0
            set_flag = c.get("setsStoryFlagOnExecute", "")
            clear_flag = c.get("clearsStoryFlagOnExecute", "")
            trig_action = c.get("triggerActionId", "")
            req_mode = c.get("requirementMode", 0) # 0: AllMustBeMet, 1: AnyCanBeMet
            req_stat = c.get("requiredStat", "")
            req_stat_val = c.get("requiredStatValue", 0)
            req_skill = c.get("requiredSkill", "")
            req_skill_lvl = c.get("requiredSkillLevel", 0)
            req_trait = c.get("requiredTrait", "")

            # Loot
            loot_list = c.get("givesLootList", [])
            drop_floor = 1 if c.get("dropLootDirectlyToFloor", False) else 0
            if loot_list:
                loot_yaml = "    givesLootList:\n" + "\n".join([f"    - itemId: {escape_yaml_string(item['id'])}\n      quantity: {item['qty']}" for item in loot_list])
            else:
                loot_yaml = "    givesLootList: []"
            
            ch_yaml += f"""  - choiceLabel: {escape_yaml_string(c.get("choiceLabel", ""))}
    choiceSubtitle: {escape_yaml_string(c.get("choiceSubtitle", ""))}
    targetPassage: {target_yaml}
    targetPassageId: ''
    targetRoomId: '{c.get("targetRoomId", "")}'
    timeMinutesCost: {time_cost}
    staminaCost: {stamina_cost}
    healthCost: 0
    requiresStoryFlag: '{req_flag}'
    requiresNotFlag: '{req_not_flag}'
    hideWhenUnavailable: {hide_unavail}
    setsStoryFlagOnExecute: '{set_flag}'
    clearsStoryFlagOnExecute: '{clear_flag}'
    triggerActionId: '{trig_action}'
    requirementMode: {req_mode}
    requiredStat: '{req_stat}'
    requiredStatValue: {req_stat_val}
    requiredSkill: '{req_skill}'
    requiredSkillLevel: {req_skill_lvl}
    requiredTrait: '{req_trait}'
    requiredItem: '{c.get("requiredItem", "")}'
    consumesItem: '{c.get("consumesItem", "")}'
    givesItem: '{c.get("givesItem", "")}'
    givesItemQty: {c.get("givesItemQty", 1)}
{loot_yaml}
    dropLootDirectlyToFloor: {drop_floor}
    skillCheckType: 0
    skillDC: 10
    successPassage: {{fileID: 0}}
    failurePassage: {{fileID: 0}}
    choiceSFX: {{fileID: 0}}
    isBackButton: {is_back}
    transitionVideo: {vid_yaml}
    transitionVideoPath: {escape_yaml_string(vid_path)}
    transitionStartSFX: {{fileID: 0}}
    transitionStepSFX: {{fileID: 0}}
    transitionEndSFX: {{fileID: 0}}
    fadeOutBgmDuringTransition: 1
    videoFadeInDuration: 0.35
    postVideoPause: 0.3
"""
    else:
        ch_yaml = "  choices: []\n"

    asset_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: e5243d49411170e49a1bac6c9ca10949, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: Assembly-CSharp::SundayLastBreath.Data.StoryPassageData
  passageId: {pid}
  passageTitle: {title}
  associatedRoomId: {room_id}
  backdropSprite: {{fileID: 0}}
  keepPreviousBackdrop: {keep_bg}
{frames_yaml}
  animationFPS: {fps}
  musicAction: {music_action}
  backgroundMusicOrAmbience: {bgm_yaml}
  musicVolume: {volume}
  entranceSFX: {{fileID: 0}}
  sfxVolume: 0.8
  speakerName: ''
  storyText: {story_text}
{beats_yaml}
{dl_yaml}  initialFloorItems: []
{ch_yaml}"""

    filepath = os.path.join(base_dir, f"{name}.asset")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(asset_content)
    write_meta(name, passages_meta[name])
    print(f"Generated {name}.asset")

# =========================================================================
# PASSAGE DEFINITIONS (DIRECTLY EXTRACTED FROM TWINE SOURCE IN ENGLISH)
# =========================================================================

# 1. Hallway Section 1: Upper Hallway
h1_def = {
    "name": "Passage_Hallway_1",
    "passageId": "hallway_1",
    "passageTitle": "CORRIDOR N // UPPER HALLWAY (SEC 1)",
    "associatedRoomId": "hallway_1",
    "musicAction": 1,
    "bgm_guid": GUID_HALLWAY_BGM,
    "musicVolume": 0.5,
    "frames": frames_h1,
    "fps": 10,
    "storyText": "The door to your apartment clicks shut behind you, sealing away the only safe place you have left.\n\n<color=#93C5FD><b>UPPER HALLWAY (4TH FLOOR).</b></color> Emergency lights flicker overhead, buzzing faintly in the stagnant air. Shadows twitch across the worn carpet.\n\nTo your right is your unit (407). Directly opposite is <b>Unit 408</b>. In front of you, the corridor stretches south into darkness.",
    "dialogueLines": [
        {
            "introNarration": "The door to your apartment clicks shut behind you, sealing away the only safe place you have left.\n\n<color=#93C5FD><b>UPPER HALLWAY.</b></color> Emergency lights flicker overhead, buzzing faintly in the stagnant air.",
            "speakerName": "YOU",
            "side": 0, # Left (Player)
            "dialogueText": "Deadbolt clicked shut... no turning back now. Harmon's door is right across the hall at 408. I should check if he's still inside before heading down.",
            "outroNarration": "Every distant siren outside echoes through the ventilation grates. In front of you, the hallway stretches south toward the middle wing."
        }
    ],
    "choices": [
        {
            "choiceLabel": "INVESTIGATE UNIT 408",
            "choiceSubtitle": "The Professor's door. Electronic lock. Total silence inside.",
            "targetPassage": "Passage_Unit408_Door",
            "targetRoomId": "hallway_1",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "▼ ADVANCE DOWN THE HALL",
            "choiceSubtitle": "Walk south toward Units 404 & 405.",
            "targetPassage": "Passage_Hallway_2",
            "targetRoomId": "hallway_2",
            "timeMinutesCost": 1
        }
    ]
}

# 2. Hallway Section 2: Mid-Hallway
h2_def = {
    "name": "Passage_Hallway_2",
    "passageId": "hallway_2",
    "passageTitle": "CORRIDOR M // MID-HALLWAY (SEC 2)",
    "associatedRoomId": "hallway_2",
    "musicAction": 0,
    "frames": frames_h2,
    "fps": 10,
    "storyText": "<color=#93C5FD><b>MID-HALLWAY.</b></color> Dust motes dance in the flickering light.\n\nYou can hear a dog barking relentlessly from behind door 404, its claws scratching desperately against the wood. Across the hall, the reinforced door of <b>Unit 405</b> stands silent.\n\nThe air here smells faintly of gunpowder and cold copper.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>MID-HALLWAY (SECTION 2).</b></color> You step carefully down the corridor, avoiding loose floor tiles.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "A dog is barking frantically behind door 404... poor thing is trapped. And across the hall, Marcus reinforced 405 like a bunker.",
            "outroNarration": "The air here smells faintly of gunpowder and cold copper. Claws scrape desperately against the inside of door 404."
        }
    ],
    "choices": [
        {
            "choiceLabel": "INVESTIGATE UNIT 405",
            "choiceSubtitle": "The Hunter's door. Heavy steel frame. Faint smell of cordite.",
            "targetPassage": "Passage_Unit405_Door",
            "targetRoomId": "hallway_2",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "▲ RETREAT UP THE HALL",
            "choiceSubtitle": "Back toward Upper Hallway & Unit 407.",
            "targetPassage": "Passage_Hallway_1",
            "targetRoomId": "hallway_1",
            "isBackButton": True,
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "▼ ADVANCE DOWN THE HALL",
            "choiceSubtitle": "Proceed south toward Units 402 & 403.",
            "targetPassage": "Passage_Hallway_3",
            "targetRoomId": "hallway_3",
            "timeMinutesCost": 1
        }
    ]
}

# 3. Hallway Section 3: Lower Hallway
h3_def = {
    "name": "Passage_Hallway_3",
    "passageId": "hallway_3",
    "passageTitle": "CORRIDOR S // LOWER HALLWAY (SEC 3)",
    "associatedRoomId": "hallway_3",
    "musicAction": 1,
    "bgm_guid": GUID_CHOPIN_BGM,
    "musicVolume": 0.45,
    "frames": frames_h3,
    "fps": 10,
    "storyText": "<color=#93C5FD><b>LOWER HALLWAY.</b></color> The lights are completely dead in this section. The metallic, coppery tang of blood mixes with the smell of ozone.\n\nJarringly, soft classical music (Chopin) plays from behind the chained door of <b>Unit 402</b>. Across from it is <b>Unit 403</b>.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>LOWER HALLWAY (SECTION 3).</b></color> The ceiling bulbs have burned out completely.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Chopin? Who plays a phonograph record in the dark while the city burns outside? The Andersons live here... something isn't right.",
            "outroNarration": "The coppery tang of blood is stronger here. The classical piano melody feels chillingly out of place."
        }
    ],
    "choices": [
        {
            "choiceLabel": "INVESTIGATE UNIT 402",
            "choiceSubtitle": "The Andersons' door. Chained from the inside. Classical music playing.",
            "targetPassage": "Passage_Unit402_Door",
            "targetRoomId": "hallway_3",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "▲ RETREAT UP THE HALL",
            "choiceSubtitle": "Back toward Mid-Hallway & Unit 405.",
            "targetPassage": "Passage_Hallway_2",
            "targetRoomId": "hallway_2",
            "isBackButton": True,
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "▼ ADVANCE DOWN THE HALL",
            "choiceSubtitle": "Proceed south toward Hallway End & Stairwell.",
            "targetPassage": "Passage_Hallway_4",
            "targetRoomId": "hallway_4",
            "timeMinutesCost": 1
        }
    ]
}

# 4. Hallway Section 4: Hallway End
h4_def = {
    "name": "Passage_Hallway_4",
    "passageId": "hallway_4",
    "passageTitle": "CORRIDOR END // STAIRWELL LANDING (SEC 4)",
    "associatedRoomId": "hallway_4",
    "musicAction": 1,
    "bgm_guid": GUID_TENSION_BGM,
    "musicVolume": 0.5,
    "frames": frames_h4,
    "fps": 10,
    "storyText": "<color=#93C5FD><b>HALLWAY END.</b></color> The corridor terminates at the heavy fire doors leading to the stairwell. Bloody handprints smear the glass viewing pane.\n\nFrom <b>Unit 401</b>, loud, obnoxious pop music thumps through the wood, completely masking the sounds of the nightmare happening outside.\n\nTo the left sits the heavy steel door of the <b>Utility Room</b>.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>HALLWAY END (SECTION 4).</b></color> The corridor terminates at the heavy red fire doors.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Unit 401 was having a party before the blackout hit. That music is deafening. And those bloody streaks on the stairwell door... people were desperate to get down.",
            "outroNarration": "Next to the stairwell is the Utility Closet with an authorized personnel swipe lock."
        }
    ],
    "choices": [
        {
            "choiceLabel": "INVESTIGATE UNIT 401",
            "choiceSubtitle": "The Party door. Slightly ajar. Blaring pop music hides footsteps.",
            "targetPassage": "Passage_Unit401_Inside",
            "targetRoomId": "hallway_4",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "CHECK UTILITY CLOSET",
            "choiceSubtitle": "Maintenance room. Heavy steel door with magnetic latch.",
            "targetPassage": "Passage_Utility_Door",
            "targetRoomId": "hallway_4",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "ENTER STAIRWELL (EXIT)",
            "choiceSubtitle": "Point of no return. Push through bloody fire doors down to 2nd Floor.",
            "targetPassage": "Passage_Stairwell_4F",
            "targetRoomId": "stairwell_4f",
            "timeMinutesCost": 2
        },
        {
            "choiceLabel": "▲ RETREAT UP THE HALL",
            "choiceSubtitle": "Back toward Lower Hallway & Unit 402.",
            "targetPassage": "Passage_Hallway_3",
            "targetRoomId": "hallway_3",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 5. Unit 408 Door
u408_door_def = {
    "name": "Passage_Unit408_Door",
    "passageId": "unit_408_door",
    "passageTitle": "UNIT 408 // ENTRY DOOR",
    "associatedRoomId": "hallway_1",
    "musicAction": 0,
    "frames": frames_u408_door,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>UNIT 408 - PROFESSOR HARMON'S DOOR.</b></color>\n\nA sturdy wooden door fitted with a modern electronic keypad lock. The blue backlight on the keypad pulses weakly.\n\nA handwritten note taped beside the keypad in spidery ink reads: <i>'If you're reading this, don't look outside. Get to the basement.'</i>",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>UNIT 408 - PROFESSOR HARMON'S DOOR.</b></color> Total silence emanates from behind the heavy door.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Harmon texted me the emergency passcode earlier: 8-4-6-5. Let's see if the keypad's backup battery is still alive.",
            "outroNarration": "A handwritten note taped beside the keypad reads: 'If you're reading this, don't look outside. Get to the basement.'",
            "requiresStoryFlag": "checked_phone_harmon"
        },
        {
            "introNarration": "<color=#93C5FD><b>UNIT 408 - PROFESSOR HARMON'S DOOR.</b></color> Total silence emanates from behind the heavy door.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "A heavy electronic deadbolt seals Harmon's door. Total silence inside... I don't know his code, but maybe I can deduce it from the worn keys, bypass the latch, or force it open.",
            "outroNarration": "A handwritten note taped beside the keypad reads: 'If you're reading this, don't look outside. Get to the basement.'",
            "requiresNotFlag": "checked_phone_harmon"
        }
    ],
    "choices": [
        {
            "choiceLabel": "Enter passcode from phone (8465)",
            "choiceSubtitle": "Type Harmon's emergency passcode into the pulsing electronic keypad.",
            "requiresStoryFlag": "checked_phone_harmon",
            "hideWhenUnavailable": True,
            "setsStoryFlagOnExecute": "room408_opened",
            "targetPassage": "Passage_Unit408_Inside",
            "targetRoomId": "unit_408",
            "timeMinutesCost": 1,
            "transitionVideo": GUID_DOOR_408_VIDEO,
            "transitionVideoPath": "Videos/door_408_enter.mp4"
        },
        {
            "choiceLabel": "Enter passcode manually (Keypad Minigame)",
            "choiceSubtitle": "Inspect the worn electronic keypad. 3 attempts before security lockout.",
            "requiresNotFlag": "room408_locked_out",
            "triggerActionId": "OPEN_KEYPAD_MODAL",
            "targetPassage": "Passage_Unit408_Inside",
            "targetRoomId": "unit_408",
            "timeMinutesCost": 1,
            "transitionVideo": GUID_DOOR_408_VIDEO,
            "transitionVideoPath": "Videos/door_408_enter.mp4"
        },
        {
            "choiceLabel": "Bypass electronic latch [HACK / LOCKPICK]",
            "choiceSubtitle": "Disassemble the circuit or exploit mechanical failsafe. (INT 6, Tech 2, Locksmith, Burglar, or Multi-Tool)",
            "requirementMode": 1,
            "requiredStat": "Tactics",
            "requiredStatValue": 6,
            "requiredSkill": "tech",
            "requiredSkillLevel": 2,
            "requiredTrait": "LOCKSMITH",
            "requiredItem": "Multi-Tool",
            "staminaCost": 7,
            "timeMinutesCost": 3,
            "setsStoryFlagOnExecute": "room408_opened",
            "targetPassage": "Passage_Unit408_Inside",
            "targetRoomId": "unit_408",
            "transitionVideo": GUID_DOOR_408_VIDEO,
            "transitionVideoPath": "Videos/door_408_enter.mp4"
        },
        {
            "choiceLabel": "Force the door frame [BRUTE FORCE]",
            "choiceSubtitle": "Lever the strike plate or batter the heavy deadbolt. (STR 6, Brawn, or Crowbar)",
            "requirementMode": 1,
            "requiredStat": "Might",
            "requiredStatValue": 6,
            "requiredTrait": "BRAWN",
            "requiredItem": "crowbar",
            "staminaCost": 15,
            "timeMinutesCost": 3,
            "setsStoryFlagOnExecute": "room408_opened",
            "targetPassage": "Passage_Unit408_Inside",
            "targetRoomId": "unit_408",
            "transitionVideo": GUID_DOOR_408_VIDEO,
            "transitionVideoPath": "Videos/door_408_enter.mp4"
        },
        {
            "choiceLabel": "Return to Upper Hallway",
            "choiceSubtitle": "Step back into the corridor.",
            "targetPassage": "Passage_Hallway_1",
            "targetRoomId": "hallway_1",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 6. Unit 408 Inside
u408_in_def = {
    "name": "Passage_Unit408_Inside",
    "passageId": "unit_408_inside",
    "passageTitle": "UNIT 408 // HARMON'S STUDY",
    "associatedRoomId": "unit_408",
    "musicAction": 1,
    "bgm_guid": GUID_TENSION_BGM,
    "musicVolume": 0.45,
    "frames": frames_u408_in,
    "fps": 8,
    "storyText": "The electronic lock chirps, and the heavy door unlatches.\n\n<color=#93C5FD><b>HARMON'S STUDY.</b></color> The apartment has been turned upside down. Bookshelves are emptied onto the carpet, blueprints cover the dining table, and the faint stench of burned copper hangs in the air.\n\nHarmon fled in a panic, but his workbench drawers are still half-open.",
    "dialogueLines": [
        {
            "introNarration": "The electronic lock chirps, and the door swings open into darkness.\n\n<color=#93C5FD><b>HARMON'S STUDY.</b></color> Books and papers are scattered everywhere.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "He definitely knew something was coming. The entire desk is covered in emergency frequency notes. I should grab whatever medical supplies and rations he left behind.",
            "outroNarration": "Blueprints for the city's power grid are scattered across the drafting table."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Scavenge Harmon's Desk",
            "choiceSubtitle": "Search the drafting table drawers for supplies.",
            "targetPassage": "Passage_Unit408_Inside",
            "targetRoomId": "unit_408",
            "timeMinutesCost": 2,
            "givesLootList": [
                {"id": "bandage", "qty": 2},
                {"id": "whiskey", "qty": 1}
            ]
        },
        {
            "choiceLabel": "▲ Leave Apartment (Return to Hallway)",
            "choiceSubtitle": "Step back out into Upper Hallway.",
            "targetPassage": "Passage_Hallway_1",
            "targetRoomId": "hallway_1",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 7. Unit 405 Door
u405_door_def = {
    "name": "Passage_Unit405_Door",
    "passageId": "unit_405_door",
    "passageTitle": "UNIT 405 // MARCUS'S DOOR",
    "associatedRoomId": "hallway_2",
    "musicAction": 0,
    "frames": frames_h2,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>UNIT 405 - THE HUNTER'S DOOR.</b></color>\n\nA solid steel-plated door with heavy deadbolts. Looking closely at the gap between the door and the frame, you spot a taut monofilament tripwire wired directly to an improvised 12-gauge blank charge.\n\nMarcus doesn't like unexpected visitors.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>UNIT 405 - THE HUNTER'S DOOR.</b></color> Heavy steel plates reinforce the frame.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Tripwire at ankle height. Classic Marcus. If I kicked this door, that shotgun charge would have taken my leg off. I need to disarm it carefully.",
            "outroNarration": "A monofilament wire is stretched taut across the threshold."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Carefully Disarm the Tripwire",
            "choiceSubtitle": "Cut the retaining wire with steady hands and bypass the shotgun trap.",
            "targetPassage": "Passage_Unit405_Inside",
            "targetRoomId": "unit_405",
            "timeMinutesCost": 2,
            "staminaCost": 5
        },
        {
            "choiceLabel": "▲ Step away back to Mid-Hallway",
            "choiceSubtitle": "Return to the corridor.",
            "targetPassage": "Passage_Hallway_2",
            "targetRoomId": "hallway_2",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 8. Unit 405 Inside
u405_in_def = {
    "name": "Passage_Unit405_Inside",
    "passageId": "unit_405_inside",
    "passageTitle": "UNIT 405 // MARCUS'S WORKSHOP",
    "associatedRoomId": "unit_405",
    "musicAction": 1,
    "bgm_guid": GUID_TENSION_BGM,
    "musicVolume": 0.45,
    "frames": frames_u405_in,
    "fps": 8,
    "storyText": "You slip through the cleared doorway into <b>Unit 405</b>.\n\nThe living room looks like a forward operating base. Spent brass shell casings roll across the hardwood floor. Gun cases lie open on the sofa, emptied of rifles.\n\nHowever, a heavy tactical vest and an open box of 9mm ammunition remain on the steel workbench.",
    "dialogueLines": [
        {
            "introNarration": "You step past the disarmed tripwire into <b>Unit 405</b>.\n\n<color=#93C5FD><b>TACTICAL WORKSHOP.</b></color> Spent brass shell casings roll across the hardwood floor.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Marcus geared up for a war. He took the heavy ordnance, but there's still plenty of 9mm rounds and a combat knife here. Exactly what I need.",
            "outroNarration": "Gun cases lie open on the furniture, cleaned out in a hurry."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Raid the Gunsmith Workbench",
            "choiceSubtitle": "Loot 9mm ammunition and a tactical combat knife.",
            "targetPassage": "Passage_Unit405_Inside",
            "targetRoomId": "unit_405",
            "timeMinutesCost": 2,
            "givesLootList": [
                {"id": "ammo_9mm", "qty": 10},
                {"id": "knife", "qty": 1}
            ]
        },
        {
            "choiceLabel": "▲ Return to Mid-Hallway",
            "choiceSubtitle": "Slip back through the reinforced door.",
            "targetPassage": "Passage_Hallway_2",
            "targetRoomId": "hallway_2",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 9. Unit 402 Door
u402_door_def = {
    "name": "Passage_Unit402_Door",
    "passageId": "unit_402_door",
    "passageTitle": "UNIT 402 // THE ANDERSONS' DOOR",
    "associatedRoomId": "hallway_3",
    "musicAction": 0,
    "frames": frames_h3,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>UNIT 402 - THE ANDERSONS' APARTMENT.</b></color>\n\nThe door is cracked open two inches, held in place by a heavy brass security chain. Soft Chopin piano notes spill out into the gloomy hallway.\n\nA pale smear of dried blood runs down the doorframe.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>UNIT 402 - THE ANDERSONS' APARTMENT.</b></color> Chopin piano notes spill through the door gap.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Hello? Mr. Anderson? Are you in there? ...No answer. Just the record spinning.",
            "outroNarration": "A pale smear of dried blood runs down the doorframe."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Cut the Security Chain with Tools",
            "choiceSubtitle": "Sever the brass chain link and push the door open.",
            "targetPassage": "Passage_Unit402_Inside",
            "targetRoomId": "unit_402",
            "timeMinutesCost": 2,
            "staminaCost": 4
        },
        {
            "choiceLabel": "▲ Return to Lower Hallway",
            "choiceSubtitle": "Step back into the dark corridor.",
            "targetPassage": "Passage_Hallway_3",
            "targetRoomId": "hallway_3",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 10. Unit 402 Inside
u402_in_def = {
    "name": "Passage_Unit402_Inside",
    "passageId": "unit_402_inside",
    "passageTitle": "UNIT 402 // THE ANDERSON RESIDENCE",
    "associatedRoomId": "unit_402",
    "musicAction": 0,
    "frames": frames_u402_in,
    "fps": 8,
    "storyText": "You sever the security chain and push the door open.\n\n<color=#93C5FD><b>THE ANDERSON RESIDENCE.</b></color> The apartment smells overpowering of household bleach. Every piece of furniture is draped in white cotton drop sheets, creating ghost-like silhouettes in the dark.\n\nA vintage turntable continues spinning in the corner. On the dining table sits a pristine red first aid kit.",
    "dialogueLines": [
        {
            "introNarration": "You push through the door into <b>Unit 402</b>.\n\n<color=#93C5FD><b>THE ANDERSON RESIDENCE.</b></color> White drop sheets cover every piece of furniture.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "They scrubbed this place down with bleach before they vanished. That first aid kit on the table has sterile gauze and hospital-grade painkillers.",
            "outroNarration": "The turntable needle scratches at the end of the groove."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Take Medical Kit from Table",
            "choiceSubtitle": "Gather sterile bandages and heavy painkillers.",
            "targetPassage": "Passage_Unit402_Inside",
            "targetRoomId": "unit_402",
            "timeMinutesCost": 1,
            "givesLootList": [
                {"id": "bandage", "qty": 2},
                {"id": "painkillers", "qty": 2}
            ]
        },
        {
            "choiceLabel": "▲ Return to Lower Hallway",
            "choiceSubtitle": "Step back out into Section 3.",
            "targetPassage": "Passage_Hallway_3",
            "targetRoomId": "hallway_3",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 11. Unit 401 Inside: The Party
u401_in_def = {
    "name": "Passage_Unit401_Inside",
    "passageId": "unit_401_inside",
    "passageTitle": "UNIT 401 // THE ABANDONED PARTY",
    "associatedRoomId": "unit_401",
    "musicAction": 1,
    "bgm_guid": GUID_PARTY_SFX,
    "musicVolume": 0.6,
    "frames": frames_u401_in,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>UNIT 401 - THE COLLEGE PARTY.</b></color>\n\nThe apartment is trashed. Red plastic cups, shattered beer bottles, and discarded coats cover every surface. The blaring sound system vibrates the floorboards.\n\nIn the corner, a massive oak bookshelf has toppled over. Underneath its splintered frame, a brass keychain glints in the strobe light.",
    "dialogueLines": [
        {
            "introNarration": "You push open the half-ajar door into <b>Unit 401</b>.\n\n<color=#93C5FD><b>THE ABANDONED PARTY.</b></color> Strobe lights flicker against smashed cups.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "They panicked and stampeded for the exits. That toppled shelf is pinning a set of car keys to the floor. Might belong to a vehicle parked outside.",
            "outroNarration": "The bass keeps thumping relentlessly through the broken drywall."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Lever the Toppled Shelf and Grab Keys",
            "choiceSubtitle": "Use your body weight to lift the oak shelf and snatch the car keys and alcohol.",
            "targetPassage": "Passage_Unit401_Inside",
            "targetRoomId": "unit_401",
            "timeMinutesCost": 2,
            "staminaCost": 6,
            "givesLootList": [
                {"id": "carkeys", "qty": 1},
                {"id": "whiskey", "qty": 1}
            ]
        },
        {
            "choiceLabel": "▲ Return to Hallway End",
            "choiceSubtitle": "Step back out into Section 4.",
            "targetPassage": "Passage_Hallway_4",
            "targetRoomId": "hallway_4",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 12. Utility Closet Door
util_door_def = {
    "name": "Passage_Utility_Door",
    "passageId": "utility_door",
    "passageTitle": "UTILITY CLOSET // STEEL DOOR",
    "associatedRoomId": "hallway_4",
    "musicAction": 1,
    "bgm_guid": GUID_UTILITY_HUM,
    "musicVolume": 0.45,
    "frames": frames_h4,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>MAINTENANCE UTILITY ROOM.</b></color>\n\nA solid industrial steel door with a high-voltage magnetic locking latch. The keyhole has been welded shut, requiring a maintenance bypass card or a mechanical pry.",
    "dialogueLines": [
        {
            "introNarration": "<color=#93C5FD><b>MAINTENANCE UTILITY ROOM.</b></color> A high-voltage magnetic reader hums on the steel jamb.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "The building superintendent's workshop. If I can bypass the magnetic latch, this room has the heavy flashlights and tools needed to navigate the basement.",
            "outroNarration": "Bleach and motor oil seep faintly from beneath the sill."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Hotwire the Magnetic Reader",
            "choiceSubtitle": "Pop the casing and bridge the circuit to unlock the latch.",
            "targetPassage": "Passage_Utility_Inside",
            "targetRoomId": "utility_room",
            "timeMinutesCost": 3,
            "staminaCost": 5
        },
        {
            "choiceLabel": "▲ Return to Hallway End",
            "choiceSubtitle": "Step back into the corridor.",
            "targetPassage": "Passage_Hallway_4",
            "targetRoomId": "hallway_4",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 13. Utility Closet Inside
util_in_def = {
    "name": "Passage_Utility_Inside",
    "passageId": "utility_inside",
    "passageTitle": "UTILITY CLOSET // WORKSHOP",
    "associatedRoomId": "utility_room",
    "musicAction": 0,
    "frames": frames_util_in,
    "fps": 8,
    "storyText": "<color=#93C5FD><b>SUPERINTENDENT'S WORKSHOP.</b></color>\n\nThe smell of machine oil, WD-40, and rubber fills the room. Tall steel shelving units line both walls, loaded with industrial supplies.\n\nA heavy rubberized waterproof flashlight and rolls of industrial duct tape sit right on the work counter.",
    "dialogueLines": [
        {
            "introNarration": "The magnetic lock buzzes and pops free.\n\n<color=#93C5FD><b>SUPERINTENDENT'S WORKSHOP.</b></color> Tall industrial shelves line the bare concrete.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Jackpot. Heavy flashlight with fresh D-cells, plus reinforced duct tape. This flashlight will illuminate the stairwell all the way down.",
            "outroNarration": "A yellow mop bucket is overturned in the corner."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Take Flashlight & Duct Tape",
            "choiceSubtitle": "Equip heavy flashlight and grab duct tape.",
            "targetPassage": "Passage_Utility_Inside",
            "targetRoomId": "utility_room",
            "timeMinutesCost": 1,
            "givesLootList": [
                {"id": "flashlight", "qty": 1},
                {"id": "ducttape", "qty": 1}
            ]
        },
        {
            "choiceLabel": "▲ Return to Hallway End",
            "choiceSubtitle": "Step back out into Section 4.",
            "targetPassage": "Passage_Hallway_4",
            "targetRoomId": "hallway_4",
            "isBackButton": True,
            "timeMinutesCost": 1
        }
    ]
}

# 14. Stairwell 4F Landing
stair_def = {
    "name": "Passage_Stairwell_4F",
    "passageId": "stairwell_4f",
    "passageTitle": "STAIRWELL // 4TH FLOOR LANDING",
    "associatedRoomId": "stairwell_4f",
    "musicAction": 1,
    "bgm_guid": GUID_GENERATOR_HUM,
    "musicVolume": 0.5,
    "frames": frames_stair_in,
    "fps": 8,
    "storyText": "You throw your weight against the heavy red fire door. It slams shut behind you with a loud metallic <i>*CLANG*</i>, latching tight.\n\n<color=#EF4444><b>THE STAIRWELL (4TH FLOOR LANDING) - POINT OF NO RETURN.</b></color>\n\nThe stairwell is cast in eerie crimson emergency lighting. Cold air rushes up from below, carrying the distant metallic groan of the building's backup generator.\n\nThe 4th floor is sealed. Your only option now is to descend.",
    "dialogueLines": [
        {
            "introNarration": "The heavy fire door slams shut behind you with a deafening metallic boom.\n\n<color=#EF4444><b>STAIRWELL 4F - POINT OF NO RETURN.</b></color>",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Fire door is latched. No going back to the 4th floor now. The stairs lead all the way down to the 2nd floor and the ground lobby. Keep your eyes open.",
            "outroNarration": "Cold drafts and the low hum of a failing generator rise from the depths below."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Descend the Stairs toward 2nd Floor",
            "choiceSubtitle": "Take the concrete steps down into the red emergency glow.",
            "targetPassage": "Passage_Stairwell_4F",
            "targetRoomId": "stairwell_4f",
            "timeMinutesCost": 3
        }
    ]
}

# Also alias Passage_Hallway_North so existing scene references continue to work seamlessly!
h_north_def = dict(h1_def)
h_north_def["name"] = "Passage_Hallway_North"

# 0. Prologue WakeUp Cold Open
prologue_def = {
    "name": "Passage_Prologue_WakeUp",
    "passageId": "prologue_wakeup",
    "passageTitle": "UNIT 407 // COLD OPEN",
    "associatedRoomId": "unit_407",
    "musicAction": 0,
    "storyText": "It was supposed to be a quiet Saturday night. You remember the warmth of the couch, the low hum of the apartment heater, and drifting off to the dull murmur of a late-night talk show.",
    "narrativeBeats": [
        "It was supposed to be a quiet Saturday night. You remember the warmth of the couch, the low hum of the apartment heater, and drifting off to the dull murmur of a late-night talk show.",
        "Then, the broadcast dies. A harsh, jagged tearing sound shreds through the tail end of your sleep -- relentless, cold white noise.",
        "Your eyes snap open as you jolt upright. The living room is drowning in the chaotic strobe of an untuned television. Beside it, the modem flashes its LEDs in a frantic pulse, searching for a dead connection.",
        "You hold your breath. Beyond the thin glass of the balcony door, a sound bleeds through from the city streets below -- a rising, mournful chorus of distant emergency sirens echoing across the night.",
        "And buried deep beneath the mechanical wail of the sirens... a raw, terrifying scream that is abruptly cut short in the dark.",
        "Barefoot and disoriented, you stumble toward the glass balcony door. As you reach out to pull the curtain, your own pale, blurred reflection stares back at you from the darkened window pane."
    ],
    "dialogueLines": [],
    "choices": [
        {
            "choiceLabel": "FOCUS ON YOUR REFLECTION...",
            "choiceSubtitle": "Step up to the window and remember who you are.",
            "triggerActionId": "TRANSITION_TO_CHARACTER_CREATION"
        }
    ]
}

# 0B. Unit 407 Safehouse Hub
u407_hub_def = {
    "name": "Passage_Unit407_Hub",
    "passageId": "unit_407_hub",
    "passageTitle": "UNIT 407 // SAFEHOUSE",
    "associatedRoomId": "unit_407",
    "musicAction": 1,
    "bgm_guid": GUID_TENSION_BGM,
    "musicVolume": 0.4,
    "storyText": "The robotic emergency broadcast cuts out. Cold static returns to the television screen.\n\n<color=#93C5FD><b>UNIT 407 - YOUR APARTMENT.</b></color> Through the thin plaster of the walls, you can hear it: the distant, overlapping chorus of emergency sirens echoing across the city.\n\nThe deadbolt on your front door is still thrown, but you can't stay here forever.",
    "narrativeBeats": [],
    "dialogueLines": [
        {
            "introNarration": "The robotic emergency voice cuts out. Cold static returns to the TV.\n\nThrough the thin plaster of your apartment walls, you can hear it: the distant, overlapping chorus of emergency sirens.",
            "speakerName": "YOU",
            "side": 0,
            "dialogueText": "Lethal force authorized... The Guard doesn't deploy for a standard riot. If they establish a quarantine cordon around this block, I'll be trapped here.",
            "outroNarration": "The emergency broadcasts have fallen completely silent. The apartment is cold."
        }
    ],
    "choices": [
        {
            "choiceLabel": "Check Smartphone",
            "choiceSubtitle": "Inspect incoming alerts, cell towers, and message logs.",
            "triggerActionId": "OPEN_PHONE_MODAL",
            "timeMinutesCost": 1
        },
        {
            "choiceLabel": "Search Bathroom Medicine Cabinet",
            "choiceSubtitle": "Scavenge the mirror vanity for painkillers and bandages.",
            "requiresNotFlag": "searched_bathroom_407",
            "setsStoryFlagOnExecute": "searched_bathroom_407",
            "givesItem": "bandage",
            "givesItemQty": 2,
            "timeMinutesCost": 2
        },
        {
            "choiceLabel": "Search Front Closet",
            "choiceSubtitle": "Grab your heavy jacket and emergency supplies.",
            "requiresNotFlag": "searched_closet_407",
            "setsStoryFlagOnExecute": "searched_closet_407",
            "givesItem": "whiskey",
            "givesItemQty": 1,
            "timeMinutesCost": 2
        },
        {
            "choiceLabel": "Tune Police Radio Scanner",
            "choiceSubtitle": "Listen to emergency SPD dispatch frequencies. (Police, Tech 1, Tactics 5, or Radio Operator)",
            "triggerActionId": "OPEN_RADIO_MODAL",
            "requirementMode": 1,
            "requiredStat": "Tactics",
            "requiredStatValue": 5,
            "requiredSkill": "tech",
            "requiredSkillLevel": 1,
            "requiredTrait": "RADIO OPERATOR",
            "requiresNotFlag": "listened_radio_407",
            "setsStoryFlagOnExecute": "listened_radio_407",
            "timeMinutesCost": 3
        },
        {
            "choiceLabel": "Inspect Government Network Traffic [HACK]",
            "choiceSubtitle": "Exploit building network vulnerabilities to bypass defense protocols. (Tech 2, Tactics 6, or Gamer)",
            "triggerActionId": "OPEN_HACKING_MODAL",
            "requirementMode": 1,
            "requiredStat": "Tactics",
            "requiredStatValue": 6,
            "requiredSkill": "tech",
            "requiredSkillLevel": 2,
            "requiredTrait": "GAMER",
            "requiresNotFlag": "hacked_terminal_407",
            "setsStoryFlagOnExecute": "hacked_terminal_407",
            "timeMinutesCost": 8
        },
        {
            "choiceLabel": "Step Out into the Corridor",
            "choiceSubtitle": "Unlock the heavy deadbolt and slip into the 4th-floor hallway.",
            "targetPassage": "Passage_Hallway_North",
            "targetRoomId": "hallway_1",
            "timeMinutesCost": 1,
            "transitionVideoPath": "Videos/door_exit.mp4"
        }
    ]
}

all_passages = [
    prologue_def,
    u407_hub_def,
    h1_def,
    h_north_def,
    h2_def,
    h3_def,
    h4_def,
    u408_door_def,
    u408_in_def,
    u405_door_def,
    u405_in_def,
    u402_door_def,
    u402_in_def,
    u401_in_def,
    util_door_def,
    util_in_def,
    stair_def
]

for p in all_passages:
    build_passage_yaml(p)

print("All 14 Act 1 passages generated successfully with English text, Twine choices, and Dialogue lines!")
