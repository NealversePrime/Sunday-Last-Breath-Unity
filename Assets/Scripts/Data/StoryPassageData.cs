using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    public enum PassageMusicAction
    {
        [Tooltip("Don't change the music; keep whatever BGM is currently playing.")]
        KeepPlaying,
        [Tooltip("Play the new BGM clip configured below.")]
        PlayNew,
        [Tooltip("Fade out and stop the background music completely (silence / suspense).")]
        StopMusic
    }

    public enum PassageSkillCheckType
    {
        None,
        Tactics,
        Job,
        Tech,
        Physical
    }

    public enum DialogueSide
    {
        [Tooltip("Avatar positioned on the LEFT (Player / MC / Default speaker).")]
        Left,
        [Tooltip("Avatar positioned on the RIGHT (NPC / Interlocutor like Vane, Diane, etc.).")]
        Right
    }

    [Serializable]
    public class PassageDialogueLine
    {
        [Header("1. Narration Above Dialogue (Optional)")]
        [TextArea(1, 3)]
        [Tooltip("Optional narrative text displayed above the dialogue card (e.g. ambient description or character actions).")]
        public string introNarration = "";

        [Header("2. Speaker Identity")]
        [Tooltip("Speaker name (e.g. 'YOU', 'Vane', 'Diane', 'Dr. Thorne'). If 'YOU' or blank, automatically uses player character's full name and portrait.")]
        public string speakerName = "YOU";

        [Tooltip("Visual placement of the avatar: Left (Player / MC) or Right (NPC).")]
        public DialogueSide side = DialogueSide.Left;

        [Header("3. Avatar Visuals (PNG or Animated GIF/WebP)")]
        [Tooltip("Static portrait sprite (PNG, JPG, or WebP).")]
        public Sprite avatarSprite;

        [Tooltip("Optional sequence of frames for animated avatar (GIF frames). If populated, loops at avatarFPS!")]
        public List<Sprite> animatedAvatarFrames = new List<Sprite>();

        [Tooltip("Framerate for animated avatar.")]
        public float avatarFPS = 8f;

        [Header("4. Dialogue Text (Supports Markdown & Rich Text)")]
        [TextArea(2, 6)]
        [Tooltip("Spoken line or internal monologue quote. Supports Markdown (**bold**, *italic*) and Rich Text (<color>).")]
        public string dialogueText = "";

        [Header("5. Narration Below Dialogue (Optional)")]
        [TextArea(1, 3)]
        [Tooltip("Optional narrative text displayed below the dialogue card.")]
        public string outroNarration = "";
    }

    [Serializable]
    public class PassageInitialItem
    {
        [Tooltip("Item ID from ItemDatabase, e.g. 'bandage', 'ammo_9mm', 'canned_food', 'lockpick', 'whiskey'.")]
        public string itemId = "bandage";
        [Tooltip("Quantity of items lying in this room.")]
        public int quantity = 1;
    }

    [Serializable]
    public class PassageChoiceData
    {
        [Header("Choice Button Text")]
        [Tooltip("Primary title of the button, e.g. 'Force the door with a crowbar'")]
        public string choiceLabel = "Inspect";

        [Tooltip("Optional subtle description or hint shown under or beside the label")]
        public string choiceSubtitle = "";

        [Header("Next Passage (Twine Connection)")]
        [Tooltip("Drag & drop the next Story Passage here. When clicked, the game seamlessly transitions to it.")]
        public StoryPassageData targetPassage;

        [Tooltip("Fallback passage ID string if referencing by ID instead of direct asset drag-drop.")]
        public string targetPassageId = "";

        [Header("Location & Map Update")]
        [Tooltip("If set, selecting this choice updates player location, HUD sector title, and tactical map beacon (e.g. 'unit_408', 'hallway_1').")]
        public string targetRoomId = "";

        [Header("Resources & Time Cost")]
        [Tooltip("Time in minutes to advance game clock (e.g. 1 min for quick peek, 15 min for thorough search).")]
        public int timeMinutesCost = 1;

        [Tooltip("Stamina spent when performing this action.")]
        public int staminaCost = 0;

        [Tooltip("HP damage received if action involves hazard or injury.")]
        public int healthCost = 0;

        [Header("Item Requirements & Loot")]
        [Tooltip("Item ID required in inventory to enable this choice (e.g. 'crowbar', 'room407_keys'). Leave blank if none.")]
        public string requiredItem = "";

        [Tooltip("Item ID consumed/removed from inventory upon clicking (e.g. 'lockpick', 'ammo_9mm'). Leave blank if none.")]
        public string consumesItem = "";

        [Tooltip("Single item ID awarded to the player upon clicking (e.g. 'bandage', '9mm_ammo'). Drops to room floor if inventory is full.")]
        public string givesItem = "";

        [Tooltip("Quantity of item given.")]
        public int givesItemQty = 1;

        [Tooltip("Optional multiple items scavenged/unlocked by this choice (e.g. looting a desk yields bandage x2 AND whiskey x1).")]
        public List<PassageInitialItem> givesLootList = new List<PassageInitialItem>();

        [Tooltip("If true, scavenged loot drops directly into the room's [ ROOM ITEMS ] floor container instead of automatically going into the player's backpack.")]
        public bool dropLootDirectlyToFloor = false;

        [Header("Skill Check (Optional RPG Roll)")]
        [Tooltip("Check type to perform (d20 + player skill bonus).")]
        public PassageSkillCheckType skillCheckType = PassageSkillCheckType.None;

        [Tooltip("Difficulty Class (DC) to beat.")]
        public int skillDC = 10;

        [Tooltip("Passage to transition to if the skill roll SUCCEEDS.")]
        public StoryPassageData successPassage;

        [Tooltip("Passage to transition to if the skill roll FAILS.")]
        public StoryPassageData failurePassage;

        [Header("Audio Feedback")]
        [Tooltip("Optional SFX played when clicking this choice. Leave blank to play standard interface click.")]
        public AudioClip choiceSFX;

        [Header("UI Styling")]
        [Tooltip("Highlight this choice as a return / back button with subtle gray styling.")]
        public bool isBackButton = false;

        [Header("Cinematic Video Transition (Resident Evil Style)")]
        [Tooltip("Optional VideoClip played when this choice is executed (e.g. door opening, staircase descent, elevator).")]
        public UnityEngine.Video.VideoClip transitionVideo;

        [Tooltip("Optional fallback Resources path for video if loading dynamically (e.g. 'Videos/unit408_door_transition').")]
        public string transitionVideoPath = "";

        [Header("Transition Audio & SFX Customization")]
        [Tooltip("Custom SFX when the door handle turns or button is pressed. If empty, uses default door knob.")]
        public AudioClip transitionStartSFX;

        [Tooltip("Custom SFX played as the player steps into the darkness (e.g. footsteps, creaking floorboards). If empty, uses default footsteps.")]
        public AudioClip transitionStepSFX;

        [Tooltip("Optional SFX played when the door closes behind the player (e.g. heavy slam, latch click).")]
        public AudioClip transitionEndSFX;

        [Tooltip("If true, fades out ambient BGM during the transition video to heighten suspense and dread.")]
        public bool fadeOutBgmDuringTransition = true;

        [Header("Transition Timings (Seconds)")]
        [Tooltip("Duration to fade video in from black screen (default: 0.35s).")]
        public float videoFadeInDuration = 0.35f;

        [Tooltip("Pause duration after video ends before revealing the new passage (default: 0.3s).")]
        public float postVideoPause = 0.3f;
    }

    /// <summary>
    /// Modular ScriptableObject representing an interactive story scene / Twine passage.
    /// Designers can author new scenes directly in the Unity Project view:
    /// Right-click -> Create -> Sunday's Last Breath -> Story Passage
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryPassage", menuName = "Sunday's Last Breath/Story Passage")]
    public class StoryPassageData : ScriptableObject
    {
        [Header("1. Passage Identity & Map Linking")]
        [Tooltip("Unique ID for this passage (e.g. 'unit_408_door', 'hallway_sec1').")]
        public string passageId = "new_passage";

        [Tooltip("Sector / room title displayed on top HUD and map (e.g. 'UNIT 408 // KEYPAD SCANNER').")]
        public string passageTitle = "LOCATION TITLE";

        [Tooltip("Map Room ID on the Tactical Blueprint (e.g. 'unit_407', 'unit_408', 'hallway_1'). Auto-syncs player beacon.")]
        public string associatedRoomId = "";

        [Header("2. Visual Backdrop")]
        [Tooltip("Sprite image displayed in the top 60% stage viewport. Drag your room/hallway illustration here!")]
        public Sprite backdropSprite;

        [Tooltip("If true, retains whatever backdrop image is currently on screen without changing it.")]
        public bool keepPreviousBackdrop = false;

        [Tooltip("Optional animated frame sequence or GIF frames (e.g. flickering door keypad, spinning fan, rain on window).")]
        public List<Sprite> animatedBackdropFrames = new List<Sprite>();

        [Tooltip("Framerate for the animated frame sequence.")]
        public float animationFPS = 8f;

        [Header("3. Audio & Music Atmosphere")]
        [Tooltip("What to do with background music: Keep current, play new BGM, or stop music completely.")]
        public PassageMusicAction musicAction = PassageMusicAction.KeepPlaying;

        [Tooltip("Background music or ambient drone clip to loop for this scene.")]
        public AudioClip backgroundMusicOrAmbience;

        [Range(0f, 1f)]
        [Tooltip("Volume level for background music.")]
        public float musicVolume = 0.5f;

        [Tooltip("Sound effect that plays once immediately upon entering this scene (e.g. door creek, buzzer, heartbeat, static).")]
        public AudioClip entranceSFX;

        [Range(0f, 1f)]
        [Tooltip("Volume level for entrance SFX.")]
        public float sfxVolume = 0.8f;

        [Header("4. Narrative Text Content")]
        [Tooltip("Optional speaker name (e.g. 'YOU', 'RADIO', 'PROFESSOR HARMON').")]
        public string speakerName = "";

        [TextArea(4, 12)]
        [Tooltip("The main narrative text of this passage. Supports Rich Text tags like <b>bold</b>, <i>italic</i>, and <color=#93C5FD>colors</color>.")]
        public string storyText = "Describe what the player sees, hears, and smells here...";

        [Tooltip("Optional sequential story beats (like in Prologue / Apartment Awakening). If populated, the player advances through them one-by-one with [CONTINUE] before choices appear!")]
        [TextArea(3, 8)]
        public List<string> narrativeBeats = new List<string>();

        [Header("5. Interactive Dialogue Lines (Left / Right VN Dialogue Cards)")]
        [Tooltip("Optional list of rich dialogue lines with Left/Right avatars, animated GIF portraits, and Markdown speech. If populated, plays sequentially or as conversation beats!")]
        public List<PassageDialogueLine> dialogueLines = new List<PassageDialogueLine>();

        [Header("6. Room Floor Items (Loose Stash in this Room)")]
        [Tooltip("List of loose items initially lying in this room when discovered. They appear in [ ROOM ITEMS ], stay in this room, and do not follow the player!")]
        public List<PassageInitialItem> initialFloorItems = new List<PassageInitialItem>();

        [Header("7. Choices & Branching (Twine Links)")]
        [Tooltip("The list of interactive buttons displayed in the choice dock below.")]
        public List<PassageChoiceData> choices = new List<PassageChoiceData>();
    }
}
