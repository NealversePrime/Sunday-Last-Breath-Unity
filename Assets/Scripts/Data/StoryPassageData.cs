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

        [Tooltip("Item ID awarded to the player upon clicking (e.g. 'bandage', '9mm_ammo'). Drops to room floor if inventory is full.")]
        public string givesItem = "";

        [Tooltip("Quantity of item given.")]
        public int givesItemQty = 1;

        [Tooltip("If true, scavenged items drop directly into the [ ROOM ITEMS ] container on the floor/table of this room instead of backpack!")]
        public bool dropDirectlyToFloor = false;

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

        [TextArea(5, 18)]
        [Tooltip("The main narrative text of this passage. Supports Rich Text tags like <b>bold</b>, <i>italic</i>, and <color=#93C5FD>colors</color>.")]
        public string storyText = "Describe what the player sees, hears, and smells here...";

        [Header("5. Loose Room Items (Floor / Table Stash)")]
        [Tooltip("List of item IDs placed openly on the table/floor of this room (e.g. 'bandage', 'ammo_9mm'). Shows in the [ ROOM ITEMS ] panel and stays in this room!")]
        public List<string> initialRoomItems = new List<string>();

        [Header("6. Choices & Branching (Twine Links)")]
        [Tooltip("The list of interactive buttons displayed in the choice dock below.")]
        public List<PassageChoiceData> choices = new List<PassageChoiceData>();
    }
}
