using System;
using UnityEngine;

namespace SundayLastBreath.Data
{
    /// <summary>
    /// Modular ScriptableObject for defining an Enemy in the Fear & Hunger Tactical Limb Combat System.
    /// Place inside Assets/Resources/Enemies/ (or assign directly) so any passage or script can spawn it by ID!
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_New", menuName = "Sunday's Last Breath/Combat/Enemy Profile", order = 20)]
    public class CombatEnemyProfile : ScriptableObject
    {
        [Header("Identity & Visuals")]
        [Tooltip("Unique ID used in passage triggers (e.g. 'zombie_commuter', 'zombie_guard').")]
        public string enemyId = "zombie_commuter";
        public string enemyName = "Infected Commuter";
        [TextArea(2, 3)]
        public string tacticalDescription = "A freshly turned commuter lunging wildly with ragged nails and snapping jaws.";

        [Tooltip("Optional single static sprite. If pre-sliced limb sprites aren't in Resources/Enemies/<enemyId>/, the system automatically slices this sprite into 6 limbs at runtime!")]
        public Sprite baseSprite;

        [Header("Offense & Defense")]
        public string weaponName = "Bloodied Teeth & Nails";
        public int baseAttackDamage = 16;
        [Range(10, 100)] public int baseAccuracy = 68;
        [Range(-30, 50)] public int baseEvasion = 12;
        [Tooltip("XP granted to the player upon defeating this enemy.")]
        public int rewardXP = 35;

        [Header("6-Limb Health & Hit Modifiers")]
        public LimbConfig head = new LimbConfig { maxHp = 25, hitBonus = -38, tacticalTag = "LETHAL" };
        public LimbConfig torso = new LimbConfig { maxHp = 75, hitBonus = 18, tacticalTag = "CORE" };
        public LimbConfig leftArm = new LimbConfig { maxHp = 30, hitBonus = -8, tacticalTag = "MAIM" };
        public LimbConfig rightArm = new LimbConfig { maxHp = 30, hitBonus = -8, tacticalTag = "DISARM" };
        public LimbConfig leftLeg = new LimbConfig { maxHp = 35, hitBonus = -4, tacticalTag = "CRIPPLE" };
        public LimbConfig rightLeg = new LimbConfig { maxHp = 35, hitBonus = -4, tacticalTag = "CRIPPLE" };

        [Serializable]
        public class LimbConfig
        {
            public int maxHp = 30;
            [Tooltip("Accuracy modifier when targeting this limb (e.g. -38 for Head, +18 for Torso).")]
            public int hitBonus = 0;
            public string tacticalTag = "MAIM";
        }
    }
}
