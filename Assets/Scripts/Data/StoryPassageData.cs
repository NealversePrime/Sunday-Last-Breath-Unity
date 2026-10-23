using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Data
{
    [Serializable]
    public class PassageChoiceData
    {
        public string choiceLabel = "Inspect the hallway";
        public string choiceSubtitle = "Peek through the peephole before unlocking";
        public string targetPassageId = "";
        [Tooltip("If set, transitioning via this choice automatically sets the player's room and updates the tactical map & top HUD.")]
        public string targetRoomId = "";
        public string conditionFlag = "";
        public string givesItem = "";
        public int staminaCost = 0;
        public int timeMinutesCost = 5;
    }

    /// <summary>
    /// Modular ScriptableObject representing a Twine-like story passage.
    /// Designers can author new story passages directly in the Unity Project view:
    /// Right-click -> Create -> Sunday's Last Breath -> Story Passage
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryPassage", menuName = "Sunday's Last Breath/Story Passage")]
    public class StoryPassageData : ScriptableObject
    {
        [Header("Passage Identity")]
        public string passageId = "unit_407_living_room";
        public string passageTitle = "UNIT 407 // LIVING ROOM";

        [Header("Location & Map Link")]
        [Tooltip("When this passage activates, the game automatically updates the HUD location and Map beacon to this Room ID.")]
        public string associatedRoomId = "unit_407";

        [Header("Narrative Presentation")]
        public string speakerName = "YOU";
        public Sprite speakerAvatar;
        [TextArea(4, 12)]
        public string passageText = "You stand in the dim glow of the television static...";

        [Header("Audio Atmosphere")]
        public AudioClip backgroundAmbience;
        public AudioClip soundEffect;

        [Header("Choices & Branching (Twine Links)")]
        public List<PassageChoiceData> choices = new List<PassageChoiceData>();
    }
}

