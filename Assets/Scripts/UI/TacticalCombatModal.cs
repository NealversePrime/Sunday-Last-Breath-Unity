using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Fear & Hunger Inspired Tactical Limb-Targeting Combat System.
    /// Features:
    /// - Front-view RPG Maker / Fear & Hunger perspective (Player silhouette in bottom-left foreground, Enemy in center arena).
    /// - 6 Independently Targetable Body Parts per Enemy:
    ///   1. HEAD (Low hit%, Lethal instant-kill decapitation/headshot)
    ///   2. TORSO (High hit%, Core vital pool)
    ///   3. LEFT ARM (Medium hit%, Maims secondary claw/grab attack)
    ///   4. RIGHT ARM (Medium hit%, Disarms primary weapon/heavy strike)
    ///   5. LEFT LEG (Medium-high hit%, Hobbles enemy -25% evasion; severing BOTH legs collapses enemy into a Crawler!)
    ///   6. RIGHT LEG (Medium-high hit%, Hobbles enemy -25% evasion; severing BOTH legs makes Headshots 95%+)
    /// - Automatic 6-layer sprite assembly from Resources/Enemies/<id>/ or dynamic runtime texture slicing from any static sprite.
    /// - Detached limb physics animation, blood splatter particles, joint wound stumps, and procedural audio synthesis.
    /// </summary>
    public class TacticalCombatModal : MonoBehaviour
    {
        public enum LimbType
        {
            Head = 0,
            Torso = 1,
            LeftArm = 2,
            RightArm = 3,
            LeftLeg = 4,
            RightLeg = 5
        }

        [Serializable]
        public class LimbState
        {
            public LimbType type;
            public string displayName;
            public string tacticalTag;
            public int hp;
            public int maxHp;
            public int baseHitBonus; // Modifier relative to weapon accuracy (e.g. Head: -35, Torso: +15)
            public bool isSevered;
            public Vector2 reticleNormalizedOffset; // Relative to enemy box (-0.5..0.5)
            public Vector2 stumpNormalizedOffset;
            public Image layerImage;
            public Image stumpImage;
            public Button overlayReticleBtn;
            public Image overlayReticleBorder;
            public TextMeshProUGUI overlayReticleLabel;
        }

        [Serializable]
        public class CombatEnemyData
        {
            public string enemyId = "zombie_commuter";
            public string enemyName = "Infected Commuter";
            public string tacticalDescription = "A freshly turned commuter lunging wildly with ragged nails and snapping jaws.";
            public string weaponName = "Ragged Claws";
            public int baseAttackDamage = 16;
            public int baseAccuracy = 68;
            public int baseEvasion = 12;
            public int rewardXP = 35;
            public Sprite fallbackSprite;
            [NonSerialized]
            public Dictionary<LimbType, LimbState> limbs = new Dictionary<LimbType, LimbState>();
            public bool isDead;

            public bool HasOneLegSevered =>
                (IsSevered(LimbType.LeftLeg) && !IsSevered(LimbType.RightLeg)) ||
                (!IsSevered(LimbType.LeftLeg) && IsSevered(LimbType.RightLeg));

            public bool HasBothLegsSevered =>
                IsSevered(LimbType.LeftLeg) && IsSevered(LimbType.RightLeg);

            public int SeveredArmsCount =>
                (IsSevered(LimbType.LeftArm) ? 1 : 0) + (IsSevered(LimbType.RightArm) ? 1 : 0);

            public bool IsSevered(LimbType t) => limbs.TryGetValue(t, out var l) && l.isSevered;
        }

        private enum SubMenuState
        {
            MainCommands,
            TargetLimbs,
            SelectItem
        }

        // Runtime State
        private CharacterData player;
        private List<CombatEnemyData> horde = new List<CombatEnemyData>();
        private int currentEnemyIndex = 0;
        private SubMenuState menuState = SubMenuState.MainCommands;
        private LimbType? hoveredLimb = null;
        private bool isPlayerGuarding = false;
        private bool isBusyTurn = false;
        private bool combatEnded = false;
        private int turnNumber = 1;

        private Action onVictoryCallback;
        private Action onDefeatCallback;
        private Action onFleeCallback;

        // UI References
        private GameObject modalContainer;
        private TextMeshProUGUI headerTitleText;
        private TextMeshProUGUI hordeCounterText;
        private Button prevTargetBtn;
        private Button nextTargetBtn;

        // Arena Visuals
        private RectTransform arenaRect;
        private RectTransform enemyRootRect;
        private RectTransform enemyBodyPivotRect;
        private Image playerForegroundImage;
        private TextMeshProUGUI enemyTitleBanner;
        private TextMeshProUGUI enemyStatusTagsText;
        private Image screenFlashOverlay;

        // Bottom Docket
        private Button cmdAttackBtn;
        private Button cmdGuardBtn;
        private Button cmdItemBtn;
        private Button cmdShoveBtn;
        private RectTransform subMenuContentRect;
        private TextMeshProUGUI combatLogText;

        // Player Status Right Panel
        private TextMeshProUGUI playerNameJobText;
        private Image playerHpBarFill;
        private TextMeshProUGUI playerHpValueText;
        private Image playerStmBarFill;
        private TextMeshProUGUI playerStmValueText;
        private TextMeshProUGUI playerWeaponText;
        private TextMeshProUGUI playerTraitsText;

        private AudioSource audioSource;
        private readonly List<GameObject> subMenuRows = new List<GameObject>();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // ====================================================================
        // PUBLIC API: TRIGGER OR START COMBAT ENCOUNTER ANYTIME
        // ====================================================================
        /// <summary>
        /// 1-Line Modular Trigger: Call from ANY script or passage to launch Tactical Combat!
        /// Pass comma-separated enemy IDs (e.g. "zombie_commuter,zombie_guard").
        /// Automatically loads CombatEnemyProfile ScriptableObjects from Resources/Enemies/ if present!
        /// </summary>
        public static TacticalCombatModal TriggerCombat(
            string enemyIdsCsv = "zombie_commuter,zombie_guard",
            Action onVictory = null,
            Action onDefeat = null,
            Action onFlee = null)
        {
            var hub = FindAnyObjectByType<ApartmentHubScreen>(FindObjectsInactive.Include);
            var modal = FindAnyObjectByType<TacticalCombatModal>(FindObjectsInactive.Include);
            if (modal == null && hub != null)
            {
                var go = new GameObject("TacticalCombatModal", typeof(RectTransform), typeof(TacticalCombatModal));
                go.transform.SetParent(hub.transform, false);
                modal = go.GetComponent<TacticalCombatModal>();
            }
            if (modal == null) return null;

            List<CombatEnemyData> list = new List<CombatEnemyData>();
            if (!string.IsNullOrWhiteSpace(enemyIdsCsv))
            {
                var parts = enemyIdsCsv.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in parts)
                {
                    list.Add(CreateEnemyById(raw.Trim()));
                }
            }
            modal.StartEncounter(null, list, onVictory, onDefeat, onFlee);
            return modal;
        }

        public static CombatEnemyData CreateEnemyById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) id = "zombie_commuter";

            // 1. Check for a modular CombatEnemyProfile ScriptableObject in Resources/Enemies/
            var allProfiles = Resources.LoadAll<CombatEnemyProfile>("Enemies");
            if (allProfiles != null)
            {
                var match = allProfiles.FirstOrDefault(p =>
                    p != null &&
                    (string.Equals(p.enemyId, id, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.name, id, StringComparison.OrdinalIgnoreCase)));
                if (match != null)
                {
                    return FromProfile(match);
                }
            }

            // 2. Built-in presets fallback
            string lower = id.ToLowerInvariant();
            if (lower.Contains("guard"))
                return CreatePresetEnemy("zombie_guard", "Infected Security Guard", "Baton & Heavy Grasp", 20, 72, 12,
                    "Armored security guard dragging a blood-stained nightstick. High torso durability.", 50);
            if (lower.Contains("motor"))
                return CreatePresetEnemy("zombie_motorist", "Infected Motorist", "Shattered Visor & Fists", 18, 68, 10,
                    "Crashed biker in torn leather gear. Lunges aggressively at close range.", 40);
            if (lower.Contains("pass"))
                return CreatePresetEnemy("zombie_passenger", "Infected Passenger", "Frantic Claws & Bite", 14, 74, 14,
                    "Fast, erratic transit passenger. Harder to hit while both legs are intact.", 35);

            return CreatePresetEnemy("zombie_commuter", "Infected Commuter", "Bloodied Teeth & Nails", 16, 66, 10,
                "A freshly turned commuter lunging wildly with ragged nails and snapping jaws.", 30);
        }

        public static CombatEnemyData FromProfile(CombatEnemyProfile p)
        {
            var e = new CombatEnemyData
            {
                enemyId = string.IsNullOrEmpty(p.enemyId) ? "zombie_commuter" : p.enemyId,
                enemyName = string.IsNullOrEmpty(p.enemyName) ? "Infected Hostile" : p.enemyName,
                tacticalDescription = string.IsNullOrEmpty(p.tacticalDescription) ? "Hostile infected target blocking your path." : p.tacticalDescription,
                weaponName = string.IsNullOrEmpty(p.weaponName) ? "Ragged Claws" : p.weaponName,
                baseAttackDamage = p.baseAttackDamage,
                baseAccuracy = p.baseAccuracy,
                baseEvasion = p.baseEvasion,
                rewardXP = p.rewardXP,
                fallbackSprite = p.baseSprite,
                isDead = false
            };
            EnsureEnemyLimbsInitialized(e);

            if (p.head != null) { e.limbs[LimbType.Head].maxHp = e.limbs[LimbType.Head].hp = Mathf.Max(1, p.head.maxHp); e.limbs[LimbType.Head].baseHitBonus = p.head.hitBonus; e.limbs[LimbType.Head].tacticalTag = p.head.tacticalTag; }
            if (p.torso != null) { e.limbs[LimbType.Torso].maxHp = e.limbs[LimbType.Torso].hp = Mathf.Max(1, p.torso.maxHp); e.limbs[LimbType.Torso].baseHitBonus = p.torso.hitBonus; e.limbs[LimbType.Torso].tacticalTag = p.torso.tacticalTag; }
            if (p.leftArm != null) { e.limbs[LimbType.LeftArm].maxHp = e.limbs[LimbType.LeftArm].hp = Mathf.Max(1, p.leftArm.maxHp); e.limbs[LimbType.LeftArm].baseHitBonus = p.leftArm.hitBonus; e.limbs[LimbType.LeftArm].tacticalTag = p.leftArm.tacticalTag; }
            if (p.rightArm != null) { e.limbs[LimbType.RightArm].maxHp = e.limbs[LimbType.RightArm].hp = Mathf.Max(1, p.rightArm.maxHp); e.limbs[LimbType.RightArm].baseHitBonus = p.rightArm.hitBonus; e.limbs[LimbType.RightArm].tacticalTag = p.rightArm.tacticalTag; }
            if (p.leftLeg != null) { e.limbs[LimbType.LeftLeg].maxHp = e.limbs[LimbType.LeftLeg].hp = Mathf.Max(1, p.leftLeg.maxHp); e.limbs[LimbType.LeftLeg].baseHitBonus = p.leftLeg.hitBonus; e.limbs[LimbType.LeftLeg].tacticalTag = p.leftLeg.tacticalTag; }
            if (p.rightLeg != null) { e.limbs[LimbType.RightLeg].maxHp = e.limbs[LimbType.RightLeg].hp = Mathf.Max(1, p.rightLeg.maxHp); e.limbs[LimbType.RightLeg].baseHitBonus = p.rightLeg.hitBonus; e.limbs[LimbType.RightLeg].tacticalTag = p.rightLeg.tacticalTag; }

            return e;
        }

        public void StartEncounter(
            CharacterData playerData,
            List<CombatEnemyData> enemies,
            Action onVictory = null,
            Action onDefeat = null,
            Action onFlee = null)
        {
            player = playerData;
            if (player == null && Core.GameManager.Instance != null)
            {
                player = Core.GameManager.Instance.CurrentPlayer;
            }
            if (player == null)
            {
                player = new CharacterData { firstName = "Neal", lastName = "Sage", job = "SURVIVOR", hp = 100, hpMax = 100, stamina = 100, staminaMax = 100 };
            }

            onVictoryCallback = onVictory;
            onDefeatCallback = onDefeat;
            onFleeCallback = onFlee;

            horde.Clear();
            if (enemies != null && enemies.Count > 0)
            {
                foreach (var e in enemies)
                {
                    EnsureEnemyLimbsInitialized(e);
                    horde.Add(e);
                }
            }
            else
            {
                // Default Encounter using modular CreateEnemyById (loads ScriptableObjects if present)
                horde.Add(CreateEnemyById("zombie_commuter"));
                horde.Add(CreateEnemyById("zombie_guard"));
            }

            currentEnemyIndex = 0;
            turnNumber = 1;
            isPlayerGuarding = false;
            isBusyTurn = false;
            combatEnded = false;
            hoveredLimb = null;
            // Start in MainCommands state: Player must click [1] ATTACK first before limb choices appear!
            menuState = SubMenuState.MainCommands;

            // Hide global TopHUD so HP/Stamina is cleanly focused on the combat survivor card
            ApartmentHubScreen.Instance?.SetTopHUDVisible(false);

            BuildOrRefreshUI();
            transform.SetAsLastSibling();
            gameObject.SetActive(true);

            // Ensure mouse raycasts always reach the combat modal even if parent screen was force-activated!
            foreach (var cg in GetComponentsInParent<CanvasGroup>(true))
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            var modalCg = GetComponent<CanvasGroup>();
            if (modalCg == null) modalCg = gameObject.AddComponent<CanvasGroup>();
            modalCg.alpha = 1f;
            modalCg.interactable = true;
            modalCg.blocksRaycasts = true;
            modalCg.ignoreParentGroups = true;

            LoadEnemyVisuals(CurrentEnemy);
            RefreshAllUI();
            SetCombatLog($"<color=#F87171>[ENCOUNTER]</color> {horde.Count} hostiles blocking your path! Choose an action on the left ([1] ATTACK to target a limb).");
            PlayZombieGroanSFX();
        }

        public static CombatEnemyData CreatePresetEnemy(string id, string name, string weapon, int dmg, int acc, int ev, string desc = "", int xpReward = 35)
        {
            var e = new CombatEnemyData
            {
                enemyId = id,
                enemyName = name,
                tacticalDescription = string.IsNullOrEmpty(desc) ? "Hostile infected target blocking your path." : desc,
                weaponName = weapon,
                baseAttackDamage = dmg,
                baseAccuracy = acc,
                baseEvasion = ev,
                rewardXP = xpReward,
                isDead = false
            };
            EnsureEnemyLimbsInitialized(e);
            return e;
        }

        private static void EnsureEnemyLimbsInitialized(CombatEnemyData e)
        {
            if (e.limbs != null && e.limbs.Count == 6) return;
            e.limbs = new Dictionary<LimbType, LimbState>();

            e.limbs[LimbType.Head] = new LimbState
            {
                type = LimbType.Head,
                displayName = "HEAD",
                tacticalTag = "LETHAL",
                hp = 25,
                maxHp = 25,
                baseHitBonus = -32,
                reticleNormalizedOffset = new Vector2(-0.02f, 0.42f),
                stumpNormalizedOffset = new Vector2(-0.02f, 0.24f)
            };

            e.limbs[LimbType.Torso] = new LimbState
            {
                type = LimbType.Torso,
                displayName = "TORSO",
                tacticalTag = "CORE",
                hp = 75,
                maxHp = 75,
                baseHitBonus = +15,
                reticleNormalizedOffset = new Vector2(0.0f, 0.08f),
                stumpNormalizedOffset = new Vector2(0.0f, 0.08f)
            };

            e.limbs[LimbType.LeftArm] = new LimbState
            {
                type = LimbType.LeftArm,
                displayName = "LEFT ARM",
                tacticalTag = "MAIM",
                hp = 30,
                maxHp = 30,
                baseHitBonus = -8,
                reticleNormalizedOffset = new Vector2(-0.36f, 0.08f),
                stumpNormalizedOffset = new Vector2(-0.16f, 0.16f)
            };

            e.limbs[LimbType.RightArm] = new LimbState
            {
                type = LimbType.RightArm,
                displayName = "RIGHT ARM",
                tacticalTag = "DISARM",
                hp = 30,
                maxHp = 30,
                baseHitBonus = -8,
                reticleNormalizedOffset = new Vector2(0.36f, 0.04f),
                stumpNormalizedOffset = new Vector2(0.16f, 0.16f)
            };

            e.limbs[LimbType.LeftLeg] = new LimbState
            {
                type = LimbType.LeftLeg,
                displayName = "LEFT LEG",
                tacticalTag = "CRIPPLE",
                hp = 35,
                maxHp = 35,
                baseHitBonus = -4,
                reticleNormalizedOffset = new Vector2(-0.20f, -0.28f),
                stumpNormalizedOffset = new Vector2(-0.08f, -0.10f)
            };

            e.limbs[LimbType.RightLeg] = new LimbState
            {
                type = LimbType.RightLeg,
                displayName = "RIGHT LEG",
                tacticalTag = "CRIPPLE",
                hp = 35,
                maxHp = 35,
                baseHitBonus = -4,
                reticleNormalizedOffset = new Vector2(0.22f, -0.22f),
                stumpNormalizedOffset = new Vector2(0.10f, -0.10f)
            };
        }

        private CombatEnemyData CurrentEnemy
        {
            get
            {
                if (horde == null || horde.Count == 0) return null;
                currentEnemyIndex = Mathf.Clamp(currentEnemyIndex, 0, horde.Count - 1);
                return horde[currentEnemyIndex];
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || isBusyTurn || combatEnded) return;

            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.digit1Key.wasPressedThisFrame) OnCommandAttack();
                else if (kb.digit2Key.wasPressedThisFrame) OnCommandGuard();
                else if (kb.digit3Key.wasPressedThisFrame) OnCommandItem();
                else if (kb.digit4Key.wasPressedThisFrame) OnCommandShove();
                else if (kb.qKey.wasPressedThisFrame) CycleTarget(-1);
                else if (kb.eKey.wasPressedThisFrame) CycleTarget(1);
            }
        }

        // ====================================================================
        // CALCULATE HIT CHANCE & WEAPON DATA
        // ====================================================================
        private void GetEquippedWeaponStats(out string wpnName, out int baseDmg, out int baseAcc, out bool isRanged, out bool isBlunt, out bool isBlade, out string ammoType, out int staminaCost)
        {
            wpnName = player != null && !string.IsNullOrEmpty(player.equippedWpn) ? player.equippedWpn : "";

            // Fallback: check if player has a weapon in inventory if nothing equipped
            if (string.IsNullOrEmpty(wpnName) && player != null)
            {
                string[] preferred = { "Glock 19", "Remington 870", "Sawed-Off Shotgun", "Combat Knife", "Machete", "Baseball Bat", "Crowbar", "Fire Axe", "Kitchen Knife" };
                foreach (var p in preferred)
                {
                    if (player.HasItem(p))
                    {
                        wpnName = p;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(wpnName) || string.Equals(wpnName, "none", StringComparison.OrdinalIgnoreCase))
            {
                bool hasKnockout = player != null && player.HasTrait("KNOCKOUT");
                wpnName = hasKnockout ? "Heavy Boxer Fists" : "Unarmed (Fists)";
                baseDmg = hasKnockout ? 28 : 15;
                baseAcc = hasKnockout ? 82 : 70;
                isRanged = false;
                isBlunt = true;
                isBlade = false;
                ammoType = null;
                staminaCost = 10;
                return;
            }

            string lower = wpnName.ToLowerInvariant();
            isRanged = lower.Contains("glock") || lower.Contains("pistol") || lower.Contains("revolver") ||
                       lower.Contains("shotgun") || lower.Contains("remington") || lower.Contains("rifle") ||
                       lower.Contains("bow") || lower.Contains("smg");

            isBlade = lower.Contains("knife") || lower.Contains("machete") || lower.Contains("axe") || lower.Contains("cleaver") || lower.Contains("scalpel");
            isBlunt = !isRanged && !isBlade;

            ammoType = null;
            if (lower.Contains("glock") || lower.Contains("9mm") || lower.Contains("pistol") || lower.Contains("smg")) ammoType = "9mm Ammo";
            else if (lower.Contains("revolver") || lower.Contains(".38")) ammoType = ".38 Ammo";
            else if (lower.Contains("shotgun") || lower.Contains("remington")) ammoType = "12 Gauge Shells";
            else if (lower.Contains("rifle")) ammoType = "5.56 Ammo";
            else if (lower.Contains("bow")) ammoType = "Arrows";

            if (isRanged)
            {
                baseDmg = lower.Contains("shotgun") ? 45 : 35;
                baseAcc = lower.Contains("shotgun") ? 88 : 80;
                staminaCost = 5;
            }
            else if (isBlade)
            {
                baseDmg = lower.Contains("axe") || lower.Contains("machete") ? 35 : 26;
                baseAcc = 84;
                staminaCost = 12;
            }
            else
            {
                baseDmg = lower.Contains("bat") || lower.Contains("crowbar") ? 32 : 22;
                baseAcc = 82;
                staminaCost = 14;
            }
        }

        private int CalculateLimbHitChance(CombatEnemyData enemy, LimbState limb)
        {
            if (enemy == null || limb == null || limb.isSevered) return 0;

            GetEquippedWeaponStats(out _, out _, out int wpnAcc, out bool isRanged, out _, out _, out _, out _);

            int skillBonus = 0;
            if (player != null)
            {
                skillBonus = isRanged ? (player.shooting * 8 + player.Observation * 2) : (player.melee * 8 + player.Might * 2);
                if (isRanged && player.HasTrait("GUNSLINGER")) skillBonus += 15;
                if (player.HasTrait("STEADY HANDS")) skillBonus += 10;
                if (player.HasTrait("ANATOMIST") && limb.type == LimbType.Head) skillBonus += 15;
                if (player.HasTrait("POOR VISION") && isRanged) skillBonus -= 15;
            }

            // Enemy Evasion based on leg status!
            int effectiveEvasion = enemy.baseEvasion;
            if (enemy.HasBothLegsSevered)
            {
                // Crawler on the floor! Sitting duck!
                effectiveEvasion = -45;
            }
            else if (enemy.HasOneLegSevered)
            {
                // Hobbling on one leg!
                effectiveEvasion -= 20;
            }

            int rawChance = wpnAcc + limb.baseHitBonus + skillBonus - effectiveEvasion;

            // Special Fear & Hunger rule: if both legs are severed, Headshots become 95%!
            if (enemy.HasBothLegsSevered && limb.type == LimbType.Head)
            {
                rawChance = Mathf.Max(rawChance, 95);
            }

            return Mathf.Clamp(rawChance, 10, 98);
        }

        // ====================================================================
        // TURN EXECUTION: PLAYER ATTACKS A SPECIFIC LIMB
        // ====================================================================
        private void OnLimbAttackSelected(LimbType limbType)
        {
            if (isBusyTurn || combatEnded) return;
            var enemy = CurrentEnemy;
            if (enemy == null || enemy.isDead) return;

            if (!enemy.limbs.TryGetValue(limbType, out var limb) || limb.isSevered)
            {
                PlayBeep(240f, 0.1f);
                return;
            }

            StartCoroutine(ExecutePlayerAttackTurnRoutine(enemy, limb));
        }

        private IEnumerator ExecutePlayerAttackTurnRoutine(CombatEnemyData enemy, LimbState limb)
        {
            isBusyTurn = true;
            isPlayerGuarding = false;

            GetEquippedWeaponStats(out string wpnName, out int baseDmg, out _, out bool isRanged, out bool isBlunt, out bool isBlade, out string ammoType, out int stmCost);

            // Check Ammo if ranged
            if (isRanged && !string.IsNullOrEmpty(ammoType) && player != null)
            {
                if (!player.HasItem(ammoType))
                {
                    // Check alternate ammo naming
                    string altAmmo = ammoType.Split(' ')[0];
                    var match = player.inventory.FirstOrDefault(i => i.itemId.IndexOf(altAmmo, StringComparison.OrdinalIgnoreCase) >= 0 && i.quantity > 0);
                    if (match != null)
                    {
                        player.RemoveItem(match.itemId, 1);
                    }
                    else
                    {
                        PlayBeep(190f, 0.15f);
                        SetCombatLog($"<color=#F59E0B>*CLICK!* Out of {ammoType}! Switching to melee strike with {wpnName}!</color>");
                        isRanged = false;
                        baseDmg = 18;
                        stmCost = 12;
                        yield return new WaitForSecondsRealtime(0.45f);
                    }
                }
                else
                {
                    player.RemoveItem(ammoType, 1);
                }
            }

            // Deduct Stamina
            if (player != null)
            {
                player.stamina = Mathf.Max(0, player.stamina - stmCost);
            }

            int hitChance = CalculateLimbHitChance(enemy, limb);
            int roll = UnityEngine.Random.Range(1, 101);
            bool isHit = roll <= hitChance;
            bool isCrit = roll <= Mathf.Max(8, (player != null ? player.Auspice * 3 : 9));

            // Play weapon attack audio
            if (isRanged) PlayGunshotSFX();
            else if (isBlade) PlaySlashSFX();
            else PlayBluntHitSFX();

            if (isHit)
            {
                int dmg = baseDmg + UnityEngine.Random.Range(-3, 6);
                if (isCrit) dmg = Mathf.RoundToInt(dmg * 1.5f);

                // Trait bonuses
                if (isBlade && (limb.type == LimbType.LeftArm || limb.type == LimbType.RightArm || limb.type == LimbType.LeftLeg || limb.type == LimbType.RightLeg))
                {
                    dmg = Mathf.RoundToInt(dmg * 1.25f); // Blades excel at severing limbs
                }
                if (isBlunt && player != null && player.HasTrait("HOME RUN") && isCrit)
                {
                    dmg *= 2;
                }

                // Instant kill on Headshot if damage >= limb.hp or critical headshot
                if (limb.type == LimbType.Head && (dmg >= limb.hp || isCrit))
                {
                    dmg = limb.hp;
                }

                limb.hp = Mathf.Max(0, limb.hp - dmg);

                // Visual Shake & Flash on Limb
                StartCoroutine(AnimateEnemyHitShake(limb));

                if (limb.hp <= 0 && !limb.isSevered)
                {
                    // LIMB SEVERED / DESTROYED!
                    limb.isSevered = true;
                    PlayLimbSeverSFX();
                    TriggerScreenFlash(new Color(0.85f, 0.08f, 0.08f, 0.45f), 0.28f);

                    if (limb.type == LimbType.Torso)
                    {
                        enemy.isDead = true;
                        SetCombatLog($"<color=#10B981><b>[FATAL BLOW!]</b></color> You tore through {enemy.enemyName}'s <b>TORSO</b> with {wpnName} ({dmg} DMG)! Target collapses dead!");
                    }
                    else if (limb.type == LimbType.Head)
                    {
                        enemy.isDead = true;
                        AnimateLimbDetachment(limb);
                        SetCombatLog($"<color=#10B981><b>[DECAPITATION / HEADSHOT!]</b></color> {enemy.enemyName}'s <b>HEAD</b> is destroyed by {wpnName}! Instant kill!");
                    }
                    else
                    {
                        AnimateLimbDetachment(limb);

                        if (limb.type == LimbType.LeftLeg || limb.type == LimbType.RightLeg)
                        {
                            if (enemy.HasBothLegsSevered)
                            {
                                SetCombatLog($"<color=#F59E0B><b>[BOTH LEGS SEVERED!]</b></color> {enemy.enemyName} crashes to the floor as a <b>CRAWLER</b>! Evasion is 0% — <b>HEADSHOTS are now 95%!</b>");
                                StartCoroutine(AnimateCrawlerCollapse());
                            }
                            else
                            {
                                SetCombatLog($"<color=#38BDF8><b>[LIMB SEVERED!]</b></color> Cut off {enemy.enemyName}'s <b>{limb.displayName}</b>! Target is hobbling on one leg (-20% Evasion)!");
                                if (enemyBodyPivotRect != null)
                                {
                                    float tilt = limb.type == LimbType.LeftLeg ? 9f : -9f;
                                    enemyBodyPivotRect.localRotation = Quaternion.Euler(0f, 0f, tilt);
                                }
                            }
                        }
                        else if (limb.type == LimbType.RightArm || limb.type == LimbType.LeftArm)
                        {
                            SetCombatLog($"<color=#38BDF8><b>[ARM SEVERED!]</b></color> {enemy.enemyName}'s <b>{limb.displayName}</b> pops off in a spray of blood! Enemy attack power & accuracy severely crippled!");
                        }
                    }
                }
                else
                {
                    string critStr = isCrit ? "<color=#F59E0B><b>CRIT!</b></color> " : "";
                    SetCombatLog($"{critStr}Hit {enemy.enemyName}'s <b>{limb.displayName}</b> with {wpnName} for <b>{dmg} DMG</b>! ({limb.hp}/{limb.maxHp} HP left)");
                }
            }
            else
            {
                PlayBeep(260f, 0.08f);
                SetCombatLog($"<color=#94A3B8>[MISSED]</color> Aimed at {enemy.enemyName}'s <b>{limb.displayName}</b> ({hitChance}%), but it lurched aside!");
            }

            hoveredLimb = null;
            menuState = SubMenuState.MainCommands;
            RefreshAllUI();
            yield return new WaitForSecondsRealtime(0.85f);

            // Check if current enemy died
            if (enemy.isDead)
            {
                yield return StartCoroutine(HandleEnemyDeathRoutine(enemy));
                if (combatEnded) yield break;
            }

            // Enemy Counter-Attack Phase
            yield return StartCoroutine(ExecuteEnemyPhaseRoutine());
            isBusyTurn = false;
            menuState = SubMenuState.MainCommands;
            hoveredLimb = null;
            RefreshAllUI();
        }

        // ====================================================================
        // OTHER PLAYER COMMANDS (GUARD, ITEM, SHOVE/FLEE)
        // ====================================================================
        private void OnCommandAttack()
        {
            if (isBusyTurn || combatEnded) return;
            PlayBeep(750f, 0.04f);
            hoveredLimb = null;
            menuState = SubMenuState.TargetLimbs;
            SetCombatLog("<color=#F87171><b>[AIMING]</b></color> Choose a body part to strike in the center console below (hover a limb to highlight it on the target).");
            RefreshAllUI();
        }

        private void OnCommandGuard()
        {
            if (isBusyTurn || combatEnded) return;
            StartCoroutine(ExecuteGuardTurnRoutine());
        }

        private IEnumerator ExecuteGuardTurnRoutine()
        {
            isBusyTurn = true;
            isPlayerGuarding = true;
            hoveredLimb = null;
            menuState = SubMenuState.MainCommands;
            PlayBeep(580f, 0.06f);

            if (player != null)
            {
                player.stamina = Mathf.Min(player.staminaMax, player.stamina + 15);
            }

            SetCombatLog("<color=#38BDF8><b>[GUARD STANCE]</b></color> You raise your arms to protect your vitals and neck (-65% incoming damage, +15 Stamina).");
            RefreshAllUI();
            yield return new WaitForSecondsRealtime(0.7f);

            yield return StartCoroutine(ExecuteEnemyPhaseRoutine());
            isBusyTurn = false;
            RefreshAllUI();
        }

        private void OnCommandItem()
        {
            if (isBusyTurn || combatEnded) return;
            PlayBeep(700f, 0.04f);
            hoveredLimb = null;
            menuState = SubMenuState.SelectItem;
            RefreshAllUI();
        }

        private void OnUseCombatItem(string itemId, int hpHeal, int stmHeal)
        {
            if (isBusyTurn || combatEnded || player == null) return;
            StartCoroutine(ExecuteUseItemTurnRoutine(itemId, hpHeal, stmHeal));
        }

        private IEnumerator ExecuteUseItemTurnRoutine(string itemId, int hpHeal, int stmHeal)
        {
            isBusyTurn = true;
            isPlayerGuarding = false;

            player.RemoveItem(itemId, 1);
            if (player.HasTrait("FIELD MEDIC") && hpHeal > 0) hpHeal = Mathf.RoundToInt(hpHeal * 1.5f);

            if (hpHeal > 0) player.hp = Mathf.Min(player.hpMax, player.hp + hpHeal);
            if (stmHeal > 0) player.stamina = Mathf.Min(player.staminaMax, player.stamina + stmHeal);

            PlayBeep(880f, 0.08f);
            SetCombatLog($"<color=#10B981><b>[ITEM USED]</b></color> Used <b>{itemId}</b> (+{hpHeal} HP, +{stmHeal} STM).");
            hoveredLimb = null;
            menuState = SubMenuState.MainCommands;
            RefreshAllUI();

            yield return new WaitForSecondsRealtime(0.75f);
            yield return StartCoroutine(ExecuteEnemyPhaseRoutine());
            isBusyTurn = false;
            RefreshAllUI();
        }

        private void OnCommandShove()
        {
            if (isBusyTurn || combatEnded) return;
            StartCoroutine(ExecuteShoveTurnRoutine());
        }

        private IEnumerator ExecuteShoveTurnRoutine()
        {
            isBusyTurn = true;
            isPlayerGuarding = false;
            PlayBluntHitSFX();

            var enemy = CurrentEnemy;
            if (enemy != null && !enemy.isDead)
            {
                if (player != null) player.stamina = Mathf.Max(0, player.stamina - 8);

                // Shove knocks a one-legged zombie onto the ground or staggers standing enemy
                if (enemy.HasOneLegSevered)
                {
                    if (enemy.limbs.TryGetValue(LimbType.LeftLeg, out var ll)) ll.isSevered = true;
                    if (enemy.limbs.TryGetValue(LimbType.RightLeg, out var rl)) rl.isSevered = true;
                    SetCombatLog($"<color=#F59E0B><b>[TAKEDOWN SHOVE!]</b></color> You kicked out {enemy.enemyName}'s remaining leg! It crashes to the floor as a Crawler!");
                    StartCoroutine(AnimateCrawlerCollapse());
                }
                else
                {
                    enemy.baseAccuracy = Mathf.Max(20, enemy.baseAccuracy - 18);
                    SetCombatLog($"<color=#38BDF8><b>[TACTICAL SHOVE]</b></color> You shoved {enemy.enemyName} backward! Its balance breaks (-18% Enemy Accuracy)!");
                    StartCoroutine(AnimateEnemyHitShake(null));
                }
            }

            RefreshAllUI();
            yield return new WaitForSecondsRealtime(0.75f);
            yield return StartCoroutine(ExecuteEnemyPhaseRoutine());
            isBusyTurn = false;
            RefreshAllUI();
        }

        // ====================================================================
        // ENEMY COUNTER-ATTACK PHASE
        // ====================================================================
        private IEnumerator ExecuteEnemyPhaseRoutine()
        {
            var aliveEnemies = horde.Where(e => !e.isDead).ToList();
            if (aliveEnemies.Count == 0) yield break;

            foreach (var enemy in aliveEnemies)
            {
                int severedArms = enemy.SeveredArmsCount;
                bool bothLegsGone = enemy.HasBothLegsSevered;

                // Calculate enemy accuracy & damage based on remaining limbs!
                int enemyAcc = enemy.baseAccuracy;
                int enemyDmg = enemy.baseAttackDamage;
                string attackDesc = $"lunges with <b>{enemy.weaponName}</b>";

                if (severedArms == 1)
                {
                    enemyDmg = Mathf.RoundToInt(enemyDmg * 0.6f);
                    enemyAcc -= 15;
                    attackDesc = "swings clumsily with its one remaining arm";
                }
                else if (severedArms >= 2)
                {
                    enemyDmg = Mathf.RoundToInt(enemyDmg * 0.35f);
                    enemyAcc -= 30;
                    attackDesc = "Both arms severed! It tries to gnaw at your ankles";
                }

                if (bothLegsGone)
                {
                    enemyAcc -= 20;
                    enemyDmg = Mathf.RoundToInt(enemyDmg * 0.7f);
                    if (severedArms >= 2)
                    {
                        // Completely armless & legless torso!
                        SetCombatLog($"<color=#94A3B8>[HELPLESS]</color> {enemy.enemyName} writhes on the pavement with no arms or legs — completely unable to reach you!");
                        yield return new WaitForSecondsRealtime(0.65f);
                        continue;
                    }
                }
                else if (enemy.HasOneLegSevered)
                {
                    enemyAcc -= 12;
                }

                int playerDodge = player != null ? (player.Reflex * 3 + player.stealth * 4) : 10;
                int finalHitChance = Mathf.Clamp(enemyAcc - playerDodge, 10, 90);
                int roll = UnityEngine.Random.Range(1, 101);

                PlayZombieGroanSFX();

                if (roll <= finalHitChance)
                {
                    int finalDmg = Mathf.Max(2, enemyDmg + UnityEngine.Random.Range(-2, 4));
                    if (isPlayerGuarding)
                    {
                        finalDmg = Mathf.Max(1, Mathf.RoundToInt(finalDmg * 0.35f));
                    }

                    if (player != null)
                    {
                        player.hp = Mathf.Max(0, player.hp - finalDmg);
                    }

                    TriggerScreenFlash(new Color(0.9f, 0.1f, 0.1f, isPlayerGuarding ? 0.2f : 0.45f), 0.22f);
                    StartCoroutine(ShakeModalChassis(isPlayerGuarding ? 5f : 12f));

                    string guardNote = isPlayerGuarding ? " <color=#38BDF8>(Guarded -65%!)</color>" : "";
                    SetCombatLog($"<color=#EF4444><b>[ENEMY TURN]</b></color> {enemy.enemyName} {attackDesc} — hits you for <b>{finalDmg} DMG</b>!{guardNote}");

                    if (player != null && player.hp <= 0)
                    {
                        RefreshAllUI();
                        yield return new WaitForSecondsRealtime(0.9f);
                        combatEnded = true;
                        SetCombatLog("<color=#EF4444><b>[YOU WERE OVERWHELMED...]</b></color> Your vision fades to black.");
                        yield return new WaitForSecondsRealtime(1.1f);
                        CloseModal();
                        onDefeatCallback?.Invoke();
                        yield break;
                    }
                }
                else
                {
                    SetCombatLog($"<color=#94A3B8>[DODGED]</color> {enemy.enemyName} {attackDesc}, but you sidestep the attack!");
                }

                RefreshAllUI();
                yield return new WaitForSecondsRealtime(0.75f);
            }

            turnNumber++;
            isPlayerGuarding = false;
        }

        private IEnumerator HandleEnemyDeathRoutine(CombatEnemyData deadEnemy)
        {
            // 1. Collapse & Fade out the defeated enemy completely — IT STAYS GONE!
            if (enemyBodyPivotRect != null)
            {
                CanvasGroup enemyCg = enemyBodyPivotRect.GetComponent<CanvasGroup>();
                if (enemyCg == null) enemyCg = enemyBodyPivotRect.gameObject.AddComponent<CanvasGroup>();

                float elapsed = 0f;
                Vector2 startPos = enemyBodyPivotRect.anchoredPosition;
                while (elapsed < 0.40f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / 0.40f;
                    enemyBodyPivotRect.anchoredPosition = startPos + new Vector2(0f, -120f * t);
                    enemyCg.alpha = 1f - t;
                    yield return null;
                }
                enemyCg.alpha = 0f;
                // Dead enemy stays completely hidden — no popping back up!
            }

            // Check if any alive enemies remain in the horde
            int nextAlive = horde.FindIndex(e => !e.isDead);
            if (nextAlive >= 0)
            {
                currentEnemyIndex = nextAlive;
                yield return new WaitForSecondsRealtime(0.25f);

                // Load next enemy visuals and reset position for the new enemy
                LoadEnemyVisuals(CurrentEnemy);
                RefreshAllUI();

                // Smoothly fade in the new enemy
                CanvasGroup enemyCg = enemyBodyPivotRect != null ? enemyBodyPivotRect.GetComponent<CanvasGroup>() : null;
                if (enemyCg != null)
                {
                    float fadeIn = 0f;
                    while (fadeIn < 0.35f)
                    {
                        fadeIn += Time.unscaledDeltaTime;
                        enemyCg.alpha = fadeIn / 0.35f;
                        yield return null;
                    }
                    enemyCg.alpha = 1f;
                }

                SetCombatLog($"<color=#F59E0B>[NEXT TARGET]</color> {CurrentEnemy.enemyName} steps forward into the arena!");
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else
            {
                // ALL ENEMIES ELIMINATED -> VICTORY!
                // The corpse stays completely gone (alpha = 0) so the clean arena/room backdrop is visible!
                combatEnded = true;
                PlayVictoryFanfare();

                if (enemyTitleBanner != null) enemyTitleBanner.text = "<color=#10B981>[AREA SECURED]</color>";
                if (enemyStatusTagsText != null) enemyStatusTagsText.text = "[THREAT NEUTRALIZED]";

                // Calculate Total XP from defeated horde
                int totalXpGained = horde.Sum(e => Mathf.Max(10, e.rewardXP));
                int startingLevel = player != null ? player.level : 1;
                bool leveledUp = false;

                string bonusNote = "";
                if (player != null)
                {
                    player.GainXP(totalXpGained);
                    if (player.level > startingLevel)
                    {
                        leveledUp = true;
                        bonusNote += $" <color=#F59E0B><b>★ LEVEL UP! (NOW LEVEL {player.level})</b></color>";
                    }
                    else
                    {
                        bonusNote += $" <color=#10B981>[+{totalXpGained} XP ({player.xp}/{player.xpRequired})]</color>";
                    }

                    if (player.HasTrait("SILENT HUNTER"))
                    {
                        player.AddItem("Arrows", 1);
                        bonusNote += " <color=#38BDF8>[SILENT HUNTER: +1 Arrow Retrieved]</color>";
                    }
                }

                SetCombatLog($"<color=#10B981><b>[ALL TARGETS ELIMINATED]</b></color> The last infected hits the floor and stops moving!{bonusNote}");
                yield return new WaitForSecondsRealtime(leveledUp ? 2.0f : 1.4f);

                CloseModal();
                onVictoryCallback?.Invoke();
            }
        }

        private void CycleTarget(int delta)
        {
            if (horde.Count <= 1 || isBusyTurn) return;
            int start = currentEnemyIndex;
            for (int i = 1; i <= horde.Count; i++)
            {
                int idx = (start + delta * i + horde.Count * 4) % horde.Count;
                if (!horde[idx].isDead)
                {
                    if (idx != currentEnemyIndex)
                    {
                        currentEnemyIndex = idx;
                        PlayBeep(650f, 0.04f);
                        LoadEnemyVisuals(CurrentEnemy);
                        RefreshAllUI();
                    }
                    break;
                }
            }
        }

        public void CloseModal()
        {
            // Restore global TopHUD and synchronize updated vitals & XP
            ApartmentHubScreen.Instance?.SetTopHUDVisible(true);
            ApartmentHubScreen.Instance?.UpdateVitalsUI();
            ApartmentHubScreen.Instance?.RefreshPDAStatusTab();

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            // Safeguard: Ensure TopHUD is always restored whenever combat modal is hidden or disabled
            ApartmentHubScreen.Instance?.SetTopHUDVisible(true);
        }

        // ====================================================================
        // VISUAL DISMEMBERMENT & LAYER LOADER
        // ====================================================================
        private void LoadEnemyVisuals(CombatEnemyData enemy)
        {
            if (enemy == null || enemyBodyPivotRect == null) return;

            CanvasGroup enemyCg = enemyBodyPivotRect.GetComponent<CanvasGroup>();
            if (enemyCg == null) enemyCg = enemyBodyPivotRect.gameObject.AddComponent<CanvasGroup>();
            enemyCg.alpha = 1f;

            // Reset position & tilt according to leg state
            enemyBodyPivotRect.anchoredPosition = enemy.HasBothLegsSevered ? new Vector2(0f, -120f) : Vector2.zero;
            float tilt = enemy.HasOneLegSevered ? (enemy.IsSevered(LimbType.LeftLeg) ? 9f : -9f) : 0f;
            enemyBodyPivotRect.localRotation = Quaternion.Euler(0f, 0f, tilt);

            string folder = $"Enemies/{enemy.enemyId}";
            Sprite sTorso = Resources.Load<Sprite>($"{folder}/torso");
            Sprite sHead = Resources.Load<Sprite>($"{folder}/head");
            Sprite sArmL = Resources.Load<Sprite>($"{folder}/arm_left");
            Sprite sArmR = Resources.Load<Sprite>($"{folder}/arm_right");
            Sprite sLegL = Resources.Load<Sprite>($"{folder}/leg_left");
            Sprite sLegR = Resources.Load<Sprite>($"{folder}/leg_right");

            // Fallback: if pre-sliced sprites aren't in Resources, dynamically slice fallbackSprite or base sprite!
            if (sTorso == null)
            {
                Sprite baseSpr = enemy.fallbackSprite != null ? enemy.fallbackSprite : Resources.Load<Sprite>($"Enemies/{enemy.enemyId}/base");
                if (baseSpr == null) baseSpr = Resources.Load<Sprite>("Enemies/zombie_commuter/base");
                if (baseSpr != null)
                {
                    SliceSpriteRuntime(baseSpr, out sTorso, out sHead, out sArmL, out sArmR, out sLegL, out sLegR);
                }
            }

            AssignLimbLayerSprite(enemy, LimbType.LeftLeg, sLegL);
            AssignLimbLayerSprite(enemy, LimbType.RightLeg, sLegR);
            AssignLimbLayerSprite(enemy, LimbType.Torso, sTorso);
            AssignLimbLayerSprite(enemy, LimbType.LeftArm, sArmL);
            AssignLimbLayerSprite(enemy, LimbType.RightArm, sArmR);
            AssignLimbLayerSprite(enemy, LimbType.Head, sHead);
        }

        private void AssignLimbLayerSprite(CombatEnemyData enemy, LimbType type, Sprite spr)
        {
            if (!enemy.limbs.TryGetValue(type, out var limb)) return;
            if (limb.layerImage != null)
            {
                limb.layerImage.sprite = spr;
                limb.layerImage.gameObject.SetActive(!limb.isSevered && spr != null);
                limb.layerImage.color = Color.white;
            }
            if (limb.stumpImage != null)
            {
                limb.stumpImage.gameObject.SetActive(limb.isSevered && type != LimbType.Torso);
            }
            else
            {
                // Fallback direct check on transform in case stumpImage reference was shared or desynced
                Transform st = enemyBodyPivotRect != null ? enemyBodyPivotRect.Find("Stump_" + type) : null;
                if (st != null) st.gameObject.SetActive(limb.isSevered && type != LimbType.Torso);
            }
        }

        /// <summary>
        /// Automatic Runtime Slicer: Takes ANY single static enemy sprite and slices it into 6 limb layers in memory!
        /// </summary>
        private void SliceSpriteRuntime(Sprite src, out Sprite torso, out Sprite head, out Sprite armL, out Sprite armR, out Sprite legL, out Sprite legR)
        {
            torso = src;
            head = null;
            armL = null;
            armR = null;
            legL = null;
            legR = null;

            if (src == null || src.texture == null || !src.texture.isReadable) return;

            Texture2D tex = src.texture;
            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();

            Texture2D tTorso = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D tHead = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D tArmL = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D tArmR = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D tLegL = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D tLegR = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color32[] pTorso = new Color32[pixels.Length];
            Color32[] pHead = new Color32[pixels.Length];
            Color32[] pArmL = new Color32[pixels.Length];
            Color32[] pArmR = new Color32[pixels.Length];
            Color32[] pLegL = new Color32[pixels.Length];
            Color32[] pLegR = new Color32[pixels.Length];

            for (int y = 0; y < h; y++)
            {
                float ny = (float)y / h; // 0 bottom .. 1 top
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w; // 0 left .. 1 right
                    int idx = y * w + x;
                    Color32 c = pixels[idx];
                    if (c.a == 0) continue;

                    if (ny >= 0.74f && nx >= 0.28f && nx <= 0.72f) pHead[idx] = c;
                    else if (ny < 0.46f && nx < 0.50f) pLegL[idx] = c;
                    else if (ny < 0.46f && nx >= 0.50f) pLegR[idx] = c;
                    else if (ny >= 0.42f && ny < 0.78f && nx < 0.33f) pArmL[idx] = c;
                    else if (ny >= 0.38f && ny < 0.78f && nx > 0.62f) pArmR[idx] = c;
                    else pTorso[idx] = c;
                }
            }

            Rect r = new Rect(0, 0, w, h);
            Vector2 piv = new Vector2(0.5f, 0.5f);
            tTorso.SetPixels32(pTorso); tTorso.Apply(); torso = Sprite.Create(tTorso, r, piv);
            tHead.SetPixels32(pHead); tHead.Apply(); head = Sprite.Create(tHead, r, piv);
            tArmL.SetPixels32(pArmL); tArmL.Apply(); armL = Sprite.Create(tArmL, r, piv);
            tArmR.SetPixels32(pArmR); tArmR.Apply(); armR = Sprite.Create(tArmR, r, piv);
            tLegL.SetPixels32(pLegL); tLegL.Apply(); legL = Sprite.Create(tLegL, r, piv);
            tLegR.SetPixels32(pLegR); tLegR.Apply(); legR = Sprite.Create(tLegR, r, piv);
        }

        private void AnimateLimbDetachment(LimbState limb)
        {
            if (limb == null || limb.layerImage == null || enemyBodyPivotRect == null) return;

            // Hide attached limb and show bloody stump
            Sprite detachedSprite = limb.layerImage.sprite;
            limb.layerImage.gameObject.SetActive(false);
            if (limb.stumpImage != null) limb.stumpImage.gameObject.SetActive(true);

            if (detachedSprite != null)
            {
                StartCoroutine(FlyOffSeveredLimbRoutine(detachedSprite, limb.stumpNormalizedOffset));
            }
        }

        private IEnumerator FlyOffSeveredLimbRoutine(Sprite limbSprite, Vector2 startNormOffset)
        {
            GameObject flyObj = new GameObject("SeveredLimbFly", typeof(RectTransform), typeof(Image));
            flyObj.transform.SetParent(enemyBodyPivotRect, false);
            RectTransform rt = flyObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440f, 440f);
            rt.anchoredPosition = Vector2.zero;

            Image img = flyObj.GetComponent<Image>();
            img.sprite = limbSprite;
            img.color = new Color(0.95f, 0.55f, 0.55f, 1f);
            img.raycastTarget = false;

            // Spawn blood droplets
            for (int i = 0; i < 10; i++)
            {
                StartCoroutine(SpawnBloodDropletRoutine(new Vector2(startNormOffset.x * 380f, startNormOffset.y * 380f)));
            }

            float dirX = startNormOffset.x >= 0f ? 1f : -1f;
            Vector2 vel = new Vector2(dirX * UnityEngine.Random.Range(120f, 220f), UnityEngine.Random.Range(100f, 180f));
            float rotSpeed = -dirX * UnityEngine.Random.Range(180f, 360f);

            float elapsed = 0f;
            while (elapsed < 0.65f && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                vel.y -= 640f * Time.unscaledDeltaTime; // Gravity
                rt.anchoredPosition += vel * Time.unscaledDeltaTime;
                rt.localRotation = Quaternion.Euler(0f, 0f, elapsed * rotSpeed);
                img.color = new Color(0.85f, 0.3f, 0.3f, Mathf.Clamp01(1f - (elapsed / 0.65f)));
                yield return null;
            }

            if (flyObj != null) Destroy(flyObj);
        }

        private IEnumerator SpawnBloodDropletRoutine(Vector2 origin)
        {
            if (enemyBodyPivotRect == null) yield break;
            GameObject drop = new GameObject("BloodDrop", typeof(RectTransform), typeof(Image));
            drop.transform.SetParent(enemyBodyPivotRect, false);
            RectTransform rt = drop.GetComponent<RectTransform>();
            float sz = UnityEngine.Random.Range(7f, 15f);
            rt.sizeDelta = new Vector2(sz, sz);
            rt.anchoredPosition = origin;

            Image img = drop.GetComponent<Image>();
            img.color = new Color(0.75f, 0.05f, 0.05f, 0.95f);
            img.raycastTarget = false;

            Vector2 vel = new Vector2(UnityEngine.Random.Range(-180f, 180f), UnityEngine.Random.Range(50f, 200f));
            float elapsed = 0f;
            while (elapsed < 0.45f && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                vel.y -= 560f * Time.unscaledDeltaTime;
                rt.anchoredPosition += vel * Time.unscaledDeltaTime;
                yield return null;
            }
            if (drop != null) Destroy(drop);
        }

        private IEnumerator AnimateCrawlerCollapse()
        {
            if (enemyBodyPivotRect == null) yield break;
            enemyBodyPivotRect.localRotation = Quaternion.identity;
            Vector2 start = enemyBodyPivotRect.anchoredPosition;
            Vector2 target = new Vector2(0f, -120f);
            float elapsed = 0f;
            while (elapsed < 0.28f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.28f);
                enemyBodyPivotRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }
            enemyBodyPivotRect.anchoredPosition = target;
        }

        private IEnumerator AnimateEnemyHitShake(LimbState hitLimb)
        {
            if (enemyBodyPivotRect == null) yield break;
            if (hitLimb?.layerImage != null) hitLimb.layerImage.color = new Color(1f, 0.3f, 0.3f, 1f);

            Vector2 basePos = enemyBodyPivotRect.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < 0.22f)
            {
                elapsed += Time.unscaledDeltaTime;
                float ox = UnityEngine.Random.Range(-14f, 14f);
                float oy = UnityEngine.Random.Range(-8f, 8f);
                enemyBodyPivotRect.anchoredPosition = basePos + new Vector2(ox, oy);
                yield return null;
            }
            enemyBodyPivotRect.anchoredPosition = basePos;
            if (hitLimb?.layerImage != null) hitLimb.layerImage.color = Color.white;
        }

        private IEnumerator ShakeModalChassis(float magnitude)
        {
            if (modalContainer == null) yield break;
            RectTransform rt = modalContainer.GetComponent<RectTransform>();
            Vector2 orig = Vector2.zero;
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.unscaledDeltaTime;
                rt.anchoredPosition = orig + new Vector2(UnityEngine.Random.Range(-magnitude, magnitude), UnityEngine.Random.Range(-magnitude, magnitude));
                yield return null;
            }
            rt.anchoredPosition = orig;
        }

        private void TriggerScreenFlash(Color flashColor, float duration)
        {
            if (screenFlashOverlay == null) return;
            StartCoroutine(ScreenFlashRoutine(flashColor, duration));
        }

        private IEnumerator ScreenFlashRoutine(Color col, float dur)
        {
            screenFlashOverlay.gameObject.SetActive(true);
            screenFlashOverlay.color = col;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                Color c = col;
                c.a = Mathf.Lerp(col.a, 0f, elapsed / dur);
                screenFlashOverlay.color = c;
                yield return null;
            }
            screenFlashOverlay.gameObject.SetActive(false);
        }

        // ====================================================================
        // BUILD FULL DUAL-SCREEN 3DS FEAR & HUNGER TACTICAL UI
        // (Top 3DS Stage: 1360x530 over RoomBackdrop | Center Seam: Combat Log | Bottom 3DS Console: 1620x390)
        // ====================================================================
        private void BuildOrRefreshUI()
        {
            if (modalContainer != null)
            {
                BindEnemyLimbLayers(CurrentEnemy);
                return;
            }

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // Keep root raycast blocker transparent so TopHUD & TopScreen RoomBackdrop remain visible!
            Image scrim = GetComponent<Image>();
            if (scrim == null) scrim = gameObject.AddComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.01f);

            // Full Dual-Screen Container using exact 1920x1080 normalized coordinates of ApartmentHubScreen!
            modalContainer = new GameObject("CombatChassis_3DS", typeof(RectTransform));
            modalContainer.transform.SetParent(transform, false);
            RectTransform cRect = modalContainer.GetComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = Vector2.one;
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            // ----------------------------------------------------------------
            // ZONE 1: TOP 3DS SCREEN (Exact match to TopScreen_VisualStage -> FramedStage 1360x530)
            // ----------------------------------------------------------------
            GameObject topStageZone = new GameObject("TopScreen_BattleStage", typeof(RectTransform));
            topStageZone.transform.SetParent(modalContainer.transform, false);
            RectTransform tszRt = topStageZone.GetComponent<RectTransform>();
            tszRt.anchorMin = new Vector2(0f, 0.44f);
            tszRt.anchorMax = new Vector2(1f, 1f);
            tszRt.offsetMin = Vector2.zero;
            tszRt.offsetMax = Vector2.zero;

            // Framed Battle Arena (1360 x 540 — fills the upper 3DS screen cleanly without top HUD obstruction!)
            GameObject arenaObj = new GameObject("BattleArena", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(Outline));
            arenaObj.transform.SetParent(topStageZone.transform, false);
            arenaRect = arenaObj.GetComponent<RectTransform>();
            arenaRect.anchorMin = new Vector2(0.5f, 0.5f);
            arenaRect.anchorMax = new Vector2(0.5f, 0.5f);
            arenaRect.pivot = new Vector2(0.5f, 0.5f);
            arenaRect.sizeDelta = new Vector2(1360f, 540f);
            arenaRect.anchoredPosition = Vector2.zero;
            // Semi-transparent atmospheric tint so the actual RoomBackdrop scene shows right through!
            arenaObj.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.05f, 0.52f);
            Outline aOut = arenaObj.GetComponent<Outline>();
            aOut.effectColor = new Color(0.65f, 0.16f, 0.16f, 0.9f);
            aOut.effectDistance = new Vector2(2f, -2f);

            // Subtle Perspective Floor Shadow at bottom of Top 3DS Screen
            GameObject floorHorizon = new GameObject("FloorHorizon", typeof(RectTransform), typeof(Image));
            floorHorizon.transform.SetParent(arenaRect, false);
            RectTransform fhRt = floorHorizon.GetComponent<RectTransform>();
            fhRt.anchorMin = new Vector2(0f, 0f);
            fhRt.anchorMax = new Vector2(1f, 0.28f);
            fhRt.sizeDelta = Vector2.zero;
            floorHorizon.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.07f, 0.65f);

            // Top Header Strip inside Top 3DS Arena (42px tall)
            GameObject headerBar = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            headerBar.transform.SetParent(arenaRect, false);
            RectTransform hbRt = headerBar.GetComponent<RectTransform>();
            hbRt.anchorMin = new Vector2(0f, 1f);
            hbRt.anchorMax = new Vector2(1f, 1f);
            hbRt.pivot = new Vector2(0.5f, 1f);
            hbRt.sizeDelta = new Vector2(0f, 42f);
            hbRt.anchoredPosition = Vector2.zero;
            headerBar.GetComponent<Image>().color = new Color(0.10f, 0.04f, 0.05f, 0.92f);

            headerTitleText = CreateText(headerBar.transform, "HeaderTitle", "TACTICAL SURVIVAL COMBAT // LIMB DISMEMBERMENT SYSTEM", 15f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform htRt = headerTitleText.GetComponent<RectTransform>();
            htRt.anchorMin = new Vector2(0f, 0f);
            htRt.anchorMax = new Vector2(0.62f, 1f);
            htRt.offsetMin = new Vector2(20f, 0f);
            htRt.offsetMax = Vector2.zero;
            headerTitleText.color = new Color(0.96f, 0.38f, 0.38f, 1f);

            // Horde Target Switcher on Right of Top 3DS Header (plenty of breathing room!)
            prevTargetBtn = CreateSmallButton(headerBar.transform, "PrevTargetBtn", "< PREV [Q]", new Vector2(-345f, 0f), new Vector2(92f, 30f), () => CycleTarget(-1));
            hordeCounterText = CreateText(headerBar.transform, "HordeCounter", "TARGET 1 / 2 (2 ALIVE)", 13f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform hcRt = hordeCounterText.GetComponent<RectTransform>();
            hcRt.anchorMin = new Vector2(1f, 0.5f);
            hcRt.anchorMax = new Vector2(1f, 0.5f);
            hcRt.sizeDelta = new Vector2(190f, 30f);
            hcRt.anchoredPosition = new Vector2(-198f, 0f);
            nextTargetBtn = CreateSmallButton(headerBar.transform, "NextTargetBtn", "[E] NEXT >", new Vector2(-52f, 0f), new Vector2(92f, 30f), () => CycleTarget(1));

            // Enemy Composite Root in Center of Top 3DS Arena (440x440!)
            GameObject enemyRootObj = new GameObject("EnemyCompositeRoot", typeof(RectTransform));
            enemyRootObj.transform.SetParent(arenaRect, false);
            enemyRootRect = enemyRootObj.GetComponent<RectTransform>();
            enemyRootRect.anchorMin = new Vector2(0.54f, 0.46f);
            enemyRootRect.anchorMax = new Vector2(0.54f, 0.46f);
            enemyRootRect.sizeDelta = new Vector2(440f, 440f);
            enemyRootRect.anchoredPosition = Vector2.zero;

            GameObject pivotObj = new GameObject("EnemyBodyPivot", typeof(RectTransform));
            pivotObj.transform.SetParent(enemyRootRect, false);
            enemyBodyPivotRect = pivotObj.GetComponent<RectTransform>();
            enemyBodyPivotRect.anchorMin = new Vector2(0.5f, 0.5f);
            enemyBodyPivotRect.anchorMax = new Vector2(0.5f, 0.5f);
            enemyBodyPivotRect.sizeDelta = new Vector2(440f, 440f);
            enemyBodyPivotRect.anchoredPosition = Vector2.zero;

            // Create the 6 Stacked Limb Image Layers + Bloody Stump markers
            LimbType[] drawOrder = { LimbType.LeftLeg, LimbType.RightLeg, LimbType.Torso, LimbType.LeftArm, LimbType.RightArm, LimbType.Head };
            foreach (var lt in drawOrder)
            {
                GameObject layerObj = new GameObject("Layer_" + lt, typeof(RectTransform), typeof(Image));
                layerObj.transform.SetParent(enemyBodyPivotRect, false);
                RectTransform lRt = layerObj.GetComponent<RectTransform>();
                lRt.anchorMin = Vector2.zero;
                lRt.anchorMax = Vector2.one;
                lRt.sizeDelta = Vector2.zero;
                Image lImg = layerObj.GetComponent<Image>();
                lImg.preserveAspect = true;
                lImg.raycastTarget = false;

                // Bloody Stump Marker for this limb joint
                GameObject stumpObj = new GameObject("Stump_" + lt, typeof(RectTransform), typeof(Image), typeof(Outline));
                stumpObj.transform.SetParent(enemyBodyPivotRect, false);
                RectTransform sRt = stumpObj.GetComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(28f, 22f);
                Image sImg = stumpObj.GetComponent<Image>();
                sImg.color = new Color(0.75f, 0.06f, 0.06f, 0.95f);
                sImg.raycastTarget = false;
                stumpObj.GetComponent<Outline>().effectColor = new Color(0.3f, 0.02f, 0.02f, 1f);
                stumpObj.SetActive(false);
            }

            // Create Interactive On-Body Target Reticle Brackets for the 6 Limbs
            foreach (LimbType lt in Enum.GetValues(typeof(LimbType)))
            {
                LimbType captured = lt;
                GameObject retObj = new GameObject("Reticle_" + lt, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
                retObj.transform.SetParent(enemyBodyPivotRect, false);
                RectTransform rRt = retObj.GetComponent<RectTransform>();
                rRt.sizeDelta = new Vector2(108f, 30f);

                Image rBg = retObj.GetComponent<Image>();
                rBg.color = new Color(0.04f, 0.07f, 0.11f, 0.88f);

                Outline rOut = retObj.GetComponent<Outline>();
                rOut.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.85f);
                rOut.effectDistance = new Vector2(1.5f, -1.5f);

                Button rBtn = retObj.GetComponent<Button>();
                rBtn.targetGraphic = rBg;
                rBtn.onClick.AddListener(() => OnLimbAttackSelected(captured));

                TextMeshProUGUI rTxt = CreateText(retObj.transform, "Lbl", lt.ToString(), 12.5f, FontStyles.Bold, TextAlignmentOptions.Center);
                RectTransform rtTxt = rTxt.GetComponent<RectTransform>();
                rtTxt.anchorMin = Vector2.zero;
                rtTxt.anchorMax = Vector2.one;
                rtTxt.sizeDelta = Vector2.zero;
            }

            // Player Silhouette in Bottom-Left Foreground (Large 330x390 Fear & Hunger Rear Perspective)
            GameObject playerFgObj = new GameObject("PlayerForeground", typeof(RectTransform), typeof(Image));
            playerFgObj.transform.SetParent(arenaRect, false);
            RectTransform pfRt = playerFgObj.GetComponent<RectTransform>();
            pfRt.anchorMin = new Vector2(0f, 0f);
            pfRt.anchorMax = new Vector2(0f, 0f);
            pfRt.pivot = new Vector2(0f, 0f);
            pfRt.sizeDelta = new Vector2(330f, 390f);
            pfRt.anchoredPosition = new Vector2(42f, -14f);

            playerForegroundImage = playerFgObj.GetComponent<Image>();
            playerForegroundImage.preserveAspect = true;
            playerForegroundImage.raycastTarget = false;
            Sprite pBackSpr = Resources.Load<Sprite>("Enemies/player_back");
            if (pBackSpr != null) playerForegroundImage.sprite = pBackSpr;

            // Enemy Name & Status Card at Top-Right of Top 3DS Arena
            GameObject bannerObj = new GameObject("EnemyBanner", typeof(RectTransform), typeof(Image), typeof(Outline));
            bannerObj.transform.SetParent(arenaRect, false);
            RectTransform bnRt = bannerObj.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(1f, 1f);
            bnRt.anchorMax = new Vector2(1f, 1f);
            bnRt.pivot = new Vector2(1f, 1f);
            bnRt.sizeDelta = new Vector2(360f, 74f);
            bnRt.anchoredPosition = new Vector2(-18f, -54f);
            bannerObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 0.94f);
            bannerObj.GetComponent<Outline>().effectColor = new Color(0.55f, 0.18f, 0.18f, 0.85f);

            enemyTitleBanner = CreateText(bannerObj.transform, "EnemyName", "INFECTED COMMUTER", 16f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            RectTransform etbRt = enemyTitleBanner.GetComponent<RectTransform>();
            etbRt.anchorMin = Vector2.zero;
            etbRt.anchorMax = Vector2.one;
            etbRt.offsetMin = new Vector2(14f, 32f);
            etbRt.offsetMax = new Vector2(-14f, -8f);

            enemyStatusTagsText = CreateText(bannerObj.transform, "EnemyStatusTags", "[STANDING] [ARMED]", 12.5f, FontStyles.Bold, TextAlignmentOptions.BottomLeft);
            RectTransform estRt = enemyStatusTagsText.GetComponent<RectTransform>();
            estRt.anchorMin = Vector2.zero;
            estRt.anchorMax = Vector2.one;
            estRt.offsetMin = new Vector2(14f, 8f);
            estRt.offsetMax = new Vector2(-14f, -36f);

            // Screen Flash Overlay inside Top 3DS Arena
            GameObject flashObj = new GameObject("ScreenFlash", typeof(RectTransform), typeof(Image));
            flashObj.transform.SetParent(arenaRect, false);
            RectTransform flRt = flashObj.GetComponent<RectTransform>();
            flRt.anchorMin = Vector2.zero;
            flRt.anchorMax = Vector2.one;
            flRt.sizeDelta = Vector2.zero;
            screenFlashOverlay = flashObj.GetComponent<Image>();
            screenFlashOverlay.raycastTarget = false;
            flashObj.SetActive(false);

            // ----------------------------------------------------------------
            // ZONE 2: CENTER 3DS HINGE SEAM -> LIVE COMBAT LOG BANNER (Exact match to CenterSeam_ConsoleSwitcher: y = 0.40..0.44)
            // ----------------------------------------------------------------
            GameObject logBox = new GameObject("CenterSeam_CombatLog", typeof(RectTransform), typeof(Image), typeof(Outline));
            logBox.transform.SetParent(modalContainer.transform, false);
            RectTransform lbRt = logBox.GetComponent<RectTransform>();
            lbRt.anchorMin = new Vector2(0f, 0.40f);
            lbRt.anchorMax = new Vector2(1f, 0.44f);
            lbRt.offsetMin = Vector2.zero;
            lbRt.offsetMax = Vector2.zero;
            logBox.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 1f);
            Outline lbOut = logBox.GetComponent<Outline>();
            lbOut.effectColor = new Color(0.65f, 0.16f, 0.16f, 0.95f);
            lbOut.effectDistance = new Vector2(0f, -1.5f);

            combatLogText = CreateText(logBox.transform, "CombatLogText", "", 15f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform cltRt = combatLogText.GetComponent<RectTransform>();
            cltRt.anchorMin = new Vector2(0.5f, 0f);
            cltRt.anchorMax = new Vector2(0.5f, 1f);
            cltRt.sizeDelta = new Vector2(1620f, 0f);
            cltRt.anchoredPosition = Vector2.zero;

            // ----------------------------------------------------------------
            // ZONE 3: BOTTOM 3DS TOUCH CONSOLE (Exact match to BottomScreen_TouchConsole: y = 0.00..0.40)
            // ----------------------------------------------------------------
            GameObject bottomConsoleBg = new GameObject("BottomScreen_CombatConsole", typeof(RectTransform), typeof(Image));
            bottomConsoleBg.transform.SetParent(modalContainer.transform, false);
            RectTransform bcbRt = bottomConsoleBg.GetComponent<RectTransform>();
            bcbRt.anchorMin = new Vector2(0f, 0f);
            bcbRt.anchorMax = new Vector2(1f, 0.40f);
            bcbRt.offsetMin = Vector2.zero;
            bcbRt.offsetMax = Vector2.zero;
            // 100% Opaque dark console background so underlying narrative text is completely hidden during combat
            bottomConsoleBg.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 1f);

            GameObject bottomInner = new GameObject("BottomConsoleInner", typeof(RectTransform));
            bottomInner.transform.SetParent(bottomConsoleBg.transform, false);
            RectTransform biRt = bottomInner.GetComponent<RectTransform>();
            biRt.anchorMin = new Vector2(0.5f, 0.5f);
            biRt.anchorMax = new Vector2(0.5f, 0.5f);
            biRt.pivot = new Vector2(0.5f, 0.5f);
            biRt.sizeDelta = new Vector2(1620f, 384f);
            biRt.anchoredPosition = Vector2.zero;

            // Column A: Left Action Commands (290px wide x 360px tall)
            GameObject leftCmdPanel = new GameObject("LeftCommandsBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup));
            leftCmdPanel.transform.SetParent(bottomInner.transform, false);
            RectTransform lcRt = leftCmdPanel.GetComponent<RectTransform>();
            lcRt.anchorMin = new Vector2(0f, 0f);
            lcRt.anchorMax = new Vector2(0f, 1f);
            lcRt.pivot = new Vector2(0f, 0.5f);
            lcRt.sizeDelta = new Vector2(290f, 0f);
            lcRt.anchoredPosition = Vector2.zero;
            leftCmdPanel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 1f);
            leftCmdPanel.GetComponent<Outline>().effectColor = new Color(0.35f, 0.42f, 0.52f, 0.85f);

            VerticalLayoutGroup vlgCmd = leftCmdPanel.GetComponent<VerticalLayoutGroup>();
            vlgCmd.padding = new RectOffset(12, 12, 12, 12);
            vlgCmd.spacing = 10f;
            vlgCmd.childControlWidth = true;
            vlgCmd.childControlHeight = true;
            vlgCmd.childForceExpandWidth = true;
            vlgCmd.childForceExpandHeight = true;

            cmdAttackBtn = CreateCommandMenuButton(leftCmdPanel.transform, "[1] ATTACK LIMB", new Color(0.45f, 0.12f, 0.12f, 0.96f), OnCommandAttack);
            cmdGuardBtn = CreateCommandMenuButton(leftCmdPanel.transform, "[2] GUARD / BRACE", new Color(0.14f, 0.25f, 0.38f, 0.96f), OnCommandGuard);
            cmdItemBtn = CreateCommandMenuButton(leftCmdPanel.transform, "[3] USE ITEM", new Color(0.12f, 0.34f, 0.22f, 0.96f), OnCommandItem);
            cmdShoveBtn = CreateCommandMenuButton(leftCmdPanel.transform, "[4] SHOVE / KICK", new Color(0.38f, 0.27f, 0.10f, 0.96f), OnCommandShove);

            // Column B: Center 6-Limb Targeting / Item Selector (860px wide x 360px tall)
            GameObject centerSelectPanel = new GameObject("CenterLimbSelector", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(GridLayoutGroup));
            centerSelectPanel.transform.SetParent(bottomInner.transform, false);
            subMenuContentRect = centerSelectPanel.GetComponent<RectTransform>();
            subMenuContentRect.anchorMin = new Vector2(0f, 0f);
            subMenuContentRect.anchorMax = new Vector2(0f, 1f);
            subMenuContentRect.pivot = new Vector2(0f, 0.5f);
            subMenuContentRect.sizeDelta = new Vector2(860f, 0f);
            subMenuContentRect.anchoredPosition = new Vector2(306f, 0f);
            centerSelectPanel.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 1f);
            centerSelectPanel.GetComponent<Outline>().effectColor = new Color(0.35f, 0.42f, 0.52f, 0.85f);

            GridLayoutGroup glg = centerSelectPanel.GetComponent<GridLayoutGroup>();
            glg.padding = new RectOffset(14, 14, 14, 14);
            glg.cellSize = new Vector2(408f, 102f);
            glg.spacing = new Vector2(14f, 12f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            // Column C: Right Survivor Status Box (438px wide x 360px tall)
            GameObject rightStatusPanel = new GameObject("RightSurvivorStatusBox", typeof(RectTransform), typeof(Image), typeof(Outline));
            rightStatusPanel.transform.SetParent(bottomInner.transform, false);
            RectTransform rsRt = rightStatusPanel.GetComponent<RectTransform>();
            rsRt.anchorMin = new Vector2(1f, 0f);
            rsRt.anchorMax = new Vector2(1f, 1f);
            rsRt.pivot = new Vector2(1f, 0.5f);
            rsRt.sizeDelta = new Vector2(438f, 0f);
            rsRt.anchoredPosition = Vector2.zero;
            rightStatusPanel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 1f);
            rightStatusPanel.GetComponent<Outline>().effectColor = new Color(0.35f, 0.42f, 0.52f, 0.85f);

            BuildRightPlayerStatusBox(rightStatusPanel.transform);
            BindEnemyLimbLayers(CurrentEnemy);
        }

        private void BuildRightPlayerStatusBox(Transform parent)
        {
            playerNameJobText = CreateText(parent, "PlayerNameJob", "NEAL SAGE // SURVIVOR", 16f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform pnjRt = playerNameJobText.GetComponent<RectTransform>();
            pnjRt.anchorMin = new Vector2(0f, 1f);
            pnjRt.anchorMax = new Vector2(1f, 1f);
            pnjRt.pivot = new Vector2(0.5f, 1f);
            pnjRt.sizeDelta = new Vector2(-32f, 34f);
            pnjRt.anchoredPosition = new Vector2(0f, -14f);
            playerNameJobText.color = new Color(0.95f, 0.96f, 0.98f, 1f);

            // BODY (HP) Bar
            CreateStatBarRow(parent, "BODY (HP)", new Vector2(0f, -62f), new Color(0.85f, 0.18f, 0.18f, 1f), out playerHpBarFill, out playerHpValueText);

            // MIND / STAMINA Bar
            CreateStatBarRow(parent, "STAMINA", new Vector2(0f, -112f), new Color(0.92f, 0.65f, 0.12f, 1f), out playerStmBarFill, out playerStmValueText);

            // Equipped Weapon Card
            GameObject wpnCard = new GameObject("WeaponCard", typeof(RectTransform), typeof(Image), typeof(Outline));
            wpnCard.transform.SetParent(parent, false);
            RectTransform wcRt = wpnCard.GetComponent<RectTransform>();
            wcRt.anchorMin = new Vector2(0f, 1f);
            wcRt.anchorMax = new Vector2(1f, 1f);
            wcRt.pivot = new Vector2(0.5f, 1f);
            wcRt.sizeDelta = new Vector2(-32f, 48f);
            wcRt.anchoredPosition = new Vector2(0f, -166f);
            wpnCard.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 0.95f);
            wpnCard.GetComponent<Outline>().effectColor = new Color(0.28f, 0.36f, 0.48f, 0.8f);

            playerWeaponText = CreateText(wpnCard.transform, "WeaponLine", "WPN: GLOCK 19", 13.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform pwRt = playerWeaponText.GetComponent<RectTransform>();
            pwRt.anchorMin = Vector2.zero;
            pwRt.anchorMax = Vector2.one;
            pwRt.offsetMin = new Vector2(14f, 0f);
            pwRt.offsetMax = new Vector2(-14f, 0f);

            // Active Combat Perks Line
            playerTraitsText = CreateText(parent, "TraitsLine", "PERKS: NONE", 12.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform ptRt = playerTraitsText.GetComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0f, 1f);
            ptRt.anchorMax = new Vector2(1f, 1f);
            ptRt.pivot = new Vector2(0.5f, 1f);
            ptRt.sizeDelta = new Vector2(-32f, 30f);
            ptRt.anchoredPosition = new Vector2(0f, -226f);

            // Tactical Dismemberment Guide Box at bottom of Right Column
            GameObject tipBox = new GameObject("TacticalGuideBox", typeof(RectTransform), typeof(Image), typeof(Outline));
            tipBox.transform.SetParent(parent, false);
            RectTransform tbRt = tipBox.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0f);
            tbRt.anchorMax = new Vector2(1f, 0f);
            tbRt.pivot = new Vector2(0.5f, 0f);
            tbRt.sizeDelta = new Vector2(-32f, 82f);
            tbRt.anchoredPosition = new Vector2(0f, 14f);
            tipBox.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.09f, 0.95f);
            tipBox.GetComponent<Outline>().effectColor = new Color(0.45f, 0.22f, 0.25f, 0.7f);

            TextMeshProUGUI tipTxt = CreateText(
                tipBox.transform,
                "GuideTxt",
                "<color=#F87171><b>DISMEMBERMENT TACTICS:</b></color>\n" +
                "<color=#CBD5E1>• Sever <b>1 LEG</b> to hobble (-20% Evasion) | <b>BOTH LEGS</b> = Crawler (<b>95% Headshot!</b>)\n" +
                "• Sever <b>ARMS</b> to disarm claws/weapons & prevent multi-hit grabs.</color>",
                11f,
                FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft
            );
            tipTxt.textWrappingMode = TextWrappingModes.Normal;
            RectTransform ttRt = tipTxt.GetComponent<RectTransform>();
            ttRt.anchorMin = Vector2.zero;
            ttRt.anchorMax = Vector2.one;
            ttRt.offsetMin = new Vector2(12f, 6f);
            ttRt.offsetMax = new Vector2(-12f, -6f);
        }

        private void CreateStatBarRow(Transform parent, string label, Vector2 anchoredPos, Color barCol, out Image fillImg, out TextMeshProUGUI valTxt)
        {
            GameObject row = new GameObject("StatRow_" + label, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            RectTransform rRt = row.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0.5f, 1f);
            rRt.sizeDelta = new Vector2(-32f, 36f);
            rRt.anchoredPosition = anchoredPos;

            TextMeshProUGUI lbl = CreateText(row.transform, "Lbl", label, 13.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            RectTransform lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0.32f, 1f);
            lRt.sizeDelta = Vector2.zero;
            lbl.color = new Color(0.64f, 0.78f, 0.92f, 1f);

            GameObject bgObj = new GameObject("BarBg", typeof(RectTransform), typeof(Image), typeof(Outline));
            bgObj.transform.SetParent(row.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.33f, 0.16f);
            bgRt.anchorMax = new Vector2(0.76f, 0.84f);
            bgRt.sizeDelta = Vector2.zero;
            bgObj.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 1f);
            bgObj.GetComponent<Outline>().effectColor = new Color(0.35f, 0.42f, 0.52f, 0.8f);

            GameObject fObj = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            fObj.transform.SetParent(bgObj.transform, false);
            RectTransform fRt = fObj.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;
            fillImg = fObj.GetComponent<Image>();
            fillImg.color = barCol;

            valTxt = CreateText(row.transform, "Val", "100/100", 14f, FontStyles.Bold, TextAlignmentOptions.MidlineRight);
            RectTransform vRt = valTxt.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.77f, 0f);
            vRt.anchorMax = new Vector2(1f, 1f);
            vRt.sizeDelta = Vector2.zero;
        }

        private void BindEnemyLimbLayers(CombatEnemyData enemy)
        {
            if (enemy == null || enemyBodyPivotRect == null) return;

            foreach (var kvp in enemy.limbs)
            {
                LimbType lt = kvp.Key;
                LimbState ls = kvp.Value;

                Transform layerTr = enemyBodyPivotRect.Find("Layer_" + lt);
                if (layerTr != null)
                {
                    ls.layerImage = layerTr.GetComponent<Image>();
                    ls.layerImage.gameObject.SetActive(!ls.isSevered && ls.layerImage.sprite != null);
                }

                Transform stumpTr = enemyBodyPivotRect.Find("Stump_" + lt);
                if (stumpTr != null)
                {
                    ls.stumpImage = stumpTr.GetComponent<Image>();
                    RectTransform sRt = stumpTr.GetComponent<RectTransform>();
                    sRt.anchoredPosition = new Vector2(ls.stumpNormalizedOffset.x * 440f, ls.stumpNormalizedOffset.y * 440f);
                    stumpTr.gameObject.SetActive(ls.isSevered && lt != LimbType.Torso);
                }

                Transform retTr = enemyBodyPivotRect.Find("Reticle_" + lt);
                if (retTr != null)
                {
                    ls.overlayReticleBtn = retTr.GetComponent<Button>();
                    ls.overlayReticleBorder = retTr.GetComponent<Image>();
                    ls.overlayReticleLabel = retTr.GetComponentInChildren<TextMeshProUGUI>();
                    RectTransform rRt = retTr.GetComponent<RectTransform>();
                    rRt.anchoredPosition = new Vector2(ls.reticleNormalizedOffset.x * 440f, ls.reticleNormalizedOffset.y * 440f);
                }
            }
        }

        // ====================================================================
        // REFRESH ALL UI PANELS & LIMB SELECTOR
        // ====================================================================
        private void RefreshAllUI()
        {
            var enemy = CurrentEnemy;
            BindEnemyLimbLayers(enemy);

            // 1. Header & Horde Counter
            int aliveCount = horde.Count(e => !e.isDead);
            if (hordeCounterText != null)
            {
                hordeCounterText.text = $"TARGET {currentEnemyIndex + 1} / {horde.Count} ({aliveCount} ALIVE)";
            }
            if (prevTargetBtn != null) prevTargetBtn.interactable = horde.Count > 1;
            if (nextTargetBtn != null) nextTargetBtn.interactable = horde.Count > 1;

            // 2. Enemy Banner & On-Body Reticles
            if (enemy != null)
            {
                if (enemyTitleBanner != null)
                {
                    enemyTitleBanner.text = $"<color=#F87171>{enemy.enemyName.ToUpperInvariant()}</color>";
                }

                if (enemyStatusTagsText != null)
                {
                    List<string> tags = new List<string>();
                    if (enemy.HasBothLegsSevered) tags.Add("<color=#F59E0B>[CRAWLER: 0% EVASION]</color>");
                    else if (enemy.HasOneLegSevered) tags.Add("<color=#38BDF8>[HOBBLED: -20% EV]</color>");
                    else tags.Add("<color=#94A3B8>[STANDING]</color>");

                    if (enemy.SeveredArmsCount >= 2) tags.Add("<color=#10B981>[ARMLESS]</color>");
                    else if (enemy.SeveredArmsCount == 1) tags.Add("<color=#38BDF8>[MAIMED ARM]</color>");
                    else tags.Add($"<color=#CBD5E1>[{enemy.weaponName.ToUpperInvariant()}]</color>");

                    enemyStatusTagsText.text = string.Join(" ", tags);
                }

                foreach (var kvp in enemy.limbs)
                {
                    var ls = kvp.Value;
                    bool isHovered = (menuState == SubMenuState.TargetLimbs && hoveredLimb.HasValue && hoveredLimb.Value == ls.type && !ls.isSevered && !enemy.isDead);

                    if (ls.layerImage != null && !ls.isSevered)
                    {
                        ls.layerImage.color = isHovered ? new Color(1f, 0.52f, 0.52f, 1f) : Color.white;
                    }

                    if (ls.overlayReticleBtn != null)
                    {
                        // Only show the floating reticle on the zombie for the SINGLE limb being hovered in TargetLimbs mode!
                        ls.overlayReticleBtn.gameObject.SetActive(isHovered);
                        if (isHovered && ls.overlayReticleLabel != null)
                        {
                            int hitPct = CalculateLimbHitChance(enemy, ls);
                            string shortName = ls.displayName.Replace("LEFT ", "L.").Replace("RIGHT ", "R.");
                            ls.overlayReticleLabel.text = $"► {shortName} <color=#38BDF8>{hitPct}%</color>";
                        }
                    }
                }
            }

            // 3. Center Sub-Menu (Main Tactical Observation OR 6 Limb Targets OR Combat Items)
            RebuildCenterSubMenu(enemy);

            // 4. Right Player Status Panel
            if (player != null)
            {
                if (playerNameJobText != null)
                {
                    playerNameJobText.text = $"{player.FullName.ToUpperInvariant()} <color=#64748B>// {player.job.ToUpperInvariant()}</color>";
                }

                float hpFrac = player.hpMax > 0 ? Mathf.Clamp01((float)player.hp / player.hpMax) : 0f;
                if (playerHpBarFill != null)
                {
                    RectTransform rt = playerHpBarFill.GetComponent<RectTransform>();
                    rt.anchorMax = new Vector2(hpFrac, 1f);
                }
                if (playerHpValueText != null) playerHpValueText.text = $"{player.hp}/{player.hpMax}";

                float stmFrac = player.staminaMax > 0 ? Mathf.Clamp01((float)player.stamina / player.staminaMax) : 0f;
                if (playerStmBarFill != null)
                {
                    RectTransform rt = playerStmBarFill.GetComponent<RectTransform>();
                    rt.anchorMax = new Vector2(stmFrac, 1f);
                }
                if (playerStmValueText != null) playerStmValueText.text = $"{player.stamina}/{player.staminaMax}";

                GetEquippedWeaponStats(out string wpnName, out int dmg, out _, out bool isRanged, out _, out _, out string ammoType, out _);
                string ammoInfo = "";
                if (isRanged && !string.IsNullOrEmpty(ammoType))
                {
                    var entry = player.inventory.FirstOrDefault(i => string.Equals(i.itemId, ammoType, StringComparison.OrdinalIgnoreCase));
                    int qty = entry != null ? entry.quantity : 0;
                    ammoInfo = $" <color=#38BDF8>[{qty} {ammoType}]</color>";
                }
                if (playerWeaponText != null)
                {
                    playerWeaponText.text = $"<color=#CBD5E1>WPN:</color> <color=#F59E0B>{wpnName.ToUpperInvariant()}</color> ({dmg} DMG){ammoInfo}";
                }

                if (playerTraitsText != null)
                {
                    var tNames = player.traits.Take(3).Select(t => t.name.ToUpperInvariant()).ToList();
                    playerTraitsText.text = tNames.Count > 0
                        ? $"<color=#10B981>TRAITS: {string.Join(", ", tNames)}</color>"
                        : "<color=#64748B>TRAITS: STANDARD SURVIVOR</color>";
                }

                // Keep Top HUD Vitals bar (HP, Stamina, Hunger, Thirst) synchronized in real-time!
                if (ApartmentHubScreen.Instance != null)
                {
                    ApartmentHubScreen.Instance.UpdateVitalsUI();
                }
            }
        }

        private void RebuildCenterSubMenu(CombatEnemyData enemy)
        {
            if (subMenuContentRect == null) return;
            foreach (var r in subMenuRows)
            {
                if (r != null)
                {
                    r.transform.SetParent(null, false);
                    Destroy(r);
                }
            }
            subMenuRows.Clear();

            var glg = subMenuContentRect.GetComponent<GridLayoutGroup>();

            // ----------------------------------------------------------------
            // STATE A: MAIN COMMANDS (Before clicking [1] ATTACK)
            // Shows Enemy Tactical Observation & Stance Readout instead of Limb list!
            // ----------------------------------------------------------------
            if (menuState == SubMenuState.MainCommands)
            {
                if (glg != null)
                {
                    glg.constraintCount = 1;
                    glg.cellSize = new Vector2(832f, 356f);
                }

                if (enemy != null)
                {
                    int intactLimbs = enemy.limbs.Values.Count(l => !l.isSevered);
                    string legState = enemy.HasBothLegsSevered
                        ? "<color=#F59E0B><b>CRAWLER (Both Legs Severed — 0% Evasion, Headshots 95%!)</b></color>"
                        : (enemy.HasOneLegSevered
                            ? "<color=#38BDF8><b>HOBBLED (1 Leg Severed — -20% Evasion)</b></color>"
                            : "<color=#94A3B8>STANDING (Both Legs Intact)</color>");

                    string armState = enemy.SeveredArmsCount >= 2
                        ? "<color=#10B981><b>DISARMED (Both Arms Severed — -65% Damage)</b></color>"
                        : (enemy.SeveredArmsCount == 1
                            ? "<color=#38BDF8><b>MAIMED (1 Arm Severed — -40% Damage)</b></color>"
                            : $"<color=#FCA5A5>ARMED ({enemy.weaponName} — {enemy.baseAttackDamage} Base DMG)</color>");

                    string infoText =
                        $"<size=17><color=#F87171><b>TARGET OBSERVATION // {enemy.enemyName.ToUpperInvariant()}</b></color></size>\n" +
                        $"<color=#CBD5E1><i>\"{enemy.tacticalDescription}\"</i></color>\n\n" +
                        $"<color=#94A3B8><b>POSTURE & MOBILITY:</b></color>   {legState}\n" +
                        $"<color=#94A3B8><b>THREAT & LIMBS:</b></color>       {armState}   <color=#64748B>({intactLimbs}/6 Body Parts Intact)</color>\n\n" +
                        $"<color=#38BDF8><b>► AWAITING COMMAND:</b></color> Click <b><color=#FCA5A5>[1] ATTACK LIMB</color></b> on the left (or press <b>[1]</b>) to open 6-limb targeting, or select <b>[2] GUARD</b>, <b>[3] USE ITEM</b>, or <b>[4] SHOVE</b>.";

                    var readCell = CreateSubMenuCell(
                        infoText,
                        new Color(0.07f, 0.09f, 0.14f, 0.96f),
                        -1f,
                        !isBusyTurn && !combatEnded,
                        () => OnCommandAttack()
                    );
                    var tmp = readCell.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.textWrappingMode = TextWrappingModes.Normal;
                        tmp.lineSpacing = 6f;
                    }
                    subMenuRows.Add(readCell);
                }
                return;
            }

            // Restore 2-column grid for TargetLimbs and SelectItem
            if (glg != null)
            {
                glg.constraintCount = 2;
                glg.cellSize = new Vector2(408f, 108f);
            }

            // ----------------------------------------------------------------
            // STATE B: SELECT COMBAT / MEDICAL ITEM
            // ----------------------------------------------------------------
            if (menuState == SubMenuState.SelectItem)
            {
                var usable = new List<(string id, int hp, int stm)>
                {
                    ("First Aid Kit", 60, 20),
                    ("Bandage", 30, 0),
                    ("Painkillers", 25, 15),
                    ("Energy Bar", 5, 45),
                    ("Coffee", 0, 35),
                    ("Inhaler", 10, 50)
                };

                int shown = 0;
                if (player != null)
                {
                    foreach (var u in usable)
                    {
                        var inv = player.inventory.FirstOrDefault(i => string.Equals(i.itemId, u.id, StringComparison.OrdinalIgnoreCase) && i.quantity > 0);
                        if (inv != null)
                        {
                            string capturedId = inv.itemId;
                            int h = u.hp;
                            int s = u.stm;
                            var btnObj = CreateSubMenuCell(
                                $"<b>{inv.itemId.ToUpperInvariant()}</b> (x{inv.quantity})\n<color=#10B981>RESTORES +{h} HP   +{s} STAMINA</color>",
                                new Color(0.10f, 0.24f, 0.18f, 0.96f),
                                1f,
                                true,
                                () => OnUseCombatItem(capturedId, h, s)
                            );
                            subMenuRows.Add(btnObj);
                            shown++;
                        }
                    }
                }

                if (shown == 0)
                {
                    var emptyObj = CreateSubMenuCell(
                        "<color=#94A3B8><b>NO MEDICAL ITEMS IN BACKPACK</b></color>\n<color=#64748B>Click [1] ATTACK LIMB to aim at enemy</color>",
                        new Color(0.12f, 0.14f, 0.18f, 0.9f),
                        -1f,
                        false,
                        null
                    );
                    subMenuRows.Add(emptyObj);
                }
                return;
            }

            // ----------------------------------------------------------------
            // STATE C: TARGET LIMBS (Only shown AFTER clicking [1] ATTACK!)
            // ----------------------------------------------------------------
            if (enemy == null) return;
            LimbType[] order = { LimbType.Head, LimbType.Torso, LimbType.LeftArm, LimbType.RightArm, LimbType.LeftLeg, LimbType.RightLeg };

            foreach (var lt in order)
            {
                if (!enemy.limbs.TryGetValue(lt, out var limb)) continue;
                LimbType captured = lt;

                if (limb.isSevered)
                {
                    var cell = CreateSubMenuCell(
                        $"<color=#64748B><s><b>{limb.displayName}</b></s></color>   <color=#EF4444><b>[SEVERED]</b></color>\n<color=#475569>0 / {limb.maxHp} HP — LIMB DESTROYED</color>",
                        new Color(0.08f, 0.05f, 0.06f, 0.80f),
                        0f,
                        false,
                        null
                    );
                    subMenuRows.Add(cell);
                }
                else
                {
                    int hitPct = CalculateLimbHitChance(enemy, limb);
                    string hitColor = hitPct >= 80 ? "#10B981" : (hitPct >= 55 ? "#38BDF8" : "#F59E0B");
                    float hpFrac = limb.maxHp > 0 ? Mathf.Clamp01((float)limb.hp / limb.maxHp) : 1f;
                    string label = $"<b>{limb.displayName}</b>   <color={hitColor}><b>[{hitPct}% HIT]</b></color>\n<color=#CBD5E1>HP: {limb.hp}/{limb.maxHp}</color>   <color=#F87171>[{limb.tacticalTag}]</color>";

                    Color bgCol = lt == LimbType.Head
                        ? new Color(0.28f, 0.10f, 0.12f, 0.96f)
                        : new Color(0.11f, 0.15f, 0.22f, 0.96f);

                    var cell = CreateSubMenuCell(
                        label,
                        bgCol,
                        hpFrac,
                        !isBusyTurn && !combatEnded,
                        () => OnLimbAttackSelected(captured),
                        captured
                    );
                    subMenuRows.Add(cell);
                }
            }
        }

        private void SetHoveredLimb(LimbType? lt)
        {
            if (hoveredLimb == lt) return;
            hoveredLimb = lt;
            var enemy = CurrentEnemy;
            if (enemy == null) return;

            foreach (var kvp in enemy.limbs)
            {
                var ls = kvp.Value;
                bool isHovered = (menuState == SubMenuState.TargetLimbs && hoveredLimb.HasValue && hoveredLimb.Value == ls.type && !ls.isSevered && !enemy.isDead);
                if (ls.layerImage != null && !ls.isSevered)
                {
                    ls.layerImage.color = isHovered ? new Color(1f, 0.52f, 0.52f, 1f) : Color.white;
                }
                if (ls.overlayReticleBtn != null)
                {
                    ls.overlayReticleBtn.gameObject.SetActive(isHovered);
                    if (isHovered && ls.overlayReticleLabel != null)
                    {
                        int hitPct = CalculateLimbHitChance(enemy, ls);
                        string shortName = ls.displayName.Replace("LEFT ", "L.").Replace("RIGHT ", "R.");
                        ls.overlayReticleLabel.text = $"► {shortName} <color=#38BDF8>{hitPct}%</color>";
                    }
                }
            }
        }

        private GameObject CreateSubMenuCell(string richText, Color bgCol, float hpBarFrac, bool interactable, Action onClick, LimbType? hoverLimb = null)
        {
            GameObject cellObj = new GameObject("SubCell", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            cellObj.transform.SetParent(subMenuContentRect, false);

            Image img = cellObj.GetComponent<Image>();
            img.color = bgCol;

            Outline outl = cellObj.GetComponent<Outline>();
            outl.effectColor = interactable ? new Color(0.38f, 0.50f, 0.66f, 0.9f) : new Color(0.2f, 0.2f, 0.25f, 0.5f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            Button btn = cellObj.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = interactable;
            if (onClick != null) btn.onClick.AddListener(() => onClick.Invoke());

            if (hoverLimb.HasValue && interactable)
            {
                LimbType capturedLimb = hoverLimb.Value;
                EventTrigger et = cellObj.AddComponent<EventTrigger>();
                var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enterEntry.callback.AddListener(_ => SetHoveredLimb(capturedLimb));
                et.triggers.Add(enterEntry);

                var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exitEntry.callback.AddListener(_ => SetHoveredLimb(null));
                et.triggers.Add(exitEntry);
            }

            TextMeshProUGUI txt = CreateText(cellObj.transform, "Txt", richText, 14f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(14f, hpBarFrac >= 0f ? 18f : 12f);
            tRt.offsetMax = new Vector2(-12f, -10f);

            if (hpBarFrac >= 0f)
            {
                // Mini Limb HP Bar along the bottom of the cell!
                GameObject barBg = new GameObject("LimbHpBg", typeof(RectTransform), typeof(Image));
                barBg.transform.SetParent(cellObj.transform, false);
                RectTransform bbRt = barBg.GetComponent<RectTransform>();
                bbRt.anchorMin = new Vector2(0f, 0f);
                bbRt.anchorMax = new Vector2(1f, 0f);
                bbRt.pivot = new Vector2(0.5f, 0f);
                bbRt.sizeDelta = new Vector2(-28f, 7f);
                bbRt.anchoredPosition = new Vector2(0f, 8f);
                barBg.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 0.9f);

                GameObject barFill = new GameObject("LimbHpFill", typeof(RectTransform), typeof(Image));
                barFill.transform.SetParent(barBg.transform, false);
                RectTransform bfRt = barFill.GetComponent<RectTransform>();
                bfRt.anchorMin = Vector2.zero;
                bfRt.anchorMax = new Vector2(Mathf.Clamp01(hpBarFrac), 1f);
                bfRt.sizeDelta = Vector2.zero;
                Image bfImg = barFill.GetComponent<Image>();
                bfImg.color = hpBarFrac > 0.5f ? new Color(0.85f, 0.22f, 0.22f, 1f) : new Color(0.95f, 0.55f, 0.12f, 1f);
                bfImg.raycastTarget = false;
            }

            return cellObj;
        }

        private Button CreateCommandMenuButton(Transform parent, string label, Color bgCol, Action onClick)
        {
            GameObject btnObj = new GameObject("Cmd_" + label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(parent, false);
            Image img = btnObj.GetComponent<Image>();
            img.color = bgCol;

            Outline outl = btnObj.GetComponent<Outline>();
            outl.effectColor = new Color(0.45f, 0.55f, 0.68f, 0.9f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            TextMeshProUGUI txt = CreateText(btnObj.transform, "Lbl", label, 14.5f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateSmallButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Action onClick)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);
            btnObj.GetComponent<Outline>().effectColor = new Color(0.4f, 0.5f, 0.65f, 0.8f);

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            TextMeshProUGUI txt = CreateText(btnObj.transform, "Lbl", label, 12f, FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            return btn;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void SetCombatLog(string message)
        {
            if (combatLogText != null)
            {
                combatLogText.text = message;
            }
        }

        // ====================================================================
        // PROCEDURAL COMBAT AUDIO SYNTHESIS
        // ====================================================================
        private void PlayBeep(float freq, float dur)
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * dur);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sr;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - (float)i / count) * 0.25f;
            }
            AudioClip clip = AudioClip.Create("CombatBeep", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlayGunshotSFX()
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * 0.18f);
            float[] data = new float[count];
            System.Random rng = new System.Random(101);
            for (int i = 0; i < count; i++)
            {
                float env = Mathf.Pow(1f - (float)i / count, 2.2f);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float boom = Mathf.Sin(2f * Mathf.PI * 140f * ((float)i / sr));
                data[i] = (noise * 0.7f + boom * 0.3f) * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("Gunshot", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlaySlashSFX()
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * 0.14f);
            float[] data = new float[count];
            System.Random rng = new System.Random(202);
            for (int i = 0; i < count; i++)
            {
                float prog = (float)i / count;
                float env = Mathf.Sin(prog * Mathf.PI);
                float freq = Mathf.Lerp(1800f, 420f, prog);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * ((float)i / sr));
                float hiss = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = (tone * 0.4f + hiss * 0.6f) * env * 0.35f;
            }
            AudioClip clip = AudioClip.Create("Slash", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlayBluntHitSFX()
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * 0.12f);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float env = 1f - (float)i / count;
                data[i] = Mathf.Sin(2f * Mathf.PI * 115f * ((float)i / sr)) * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("BluntHit", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlayLimbSeverSFX()
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * 0.24f);
            float[] data = new float[count];
            System.Random rng = new System.Random(303);
            for (int i = 0; i < count; i++)
            {
                float prog = (float)i / count;
                float env = 1f - prog;
                float crunch = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(320f, 85f, prog) * ((float)i / sr));
                float rip = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = (crunch * 0.55f + rip * 0.45f) * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("LimbSever", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlayZombieGroanSFX()
        {
            if (audioSource == null) return;
            int sr = 44100;
            int count = (int)(sr * 0.28f);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Sin(((float)i / count) * Mathf.PI);
                float g1 = Mathf.Sin(2f * Mathf.PI * 92f * t);
                float g2 = Mathf.Sin(2f * Mathf.PI * 138f * t);
                data[i] = (g1 * 0.6f + g2 * 0.4f) * env * 0.22f;
            }
            AudioClip clip = AudioClip.Create("ZombieGroan", count, 1, sr, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private void PlayVictoryFanfare()
        {
            StartCoroutine(VictoryFanfareRoutine());
        }

        private IEnumerator VictoryFanfareRoutine()
        {
            PlayBeep(523.25f, 0.09f);
            yield return new WaitForSecondsRealtime(0.1f);
            PlayBeep(659.25f, 0.09f);
            yield return new WaitForSecondsRealtime(0.1f);
            PlayBeep(783.99f, 0.16f);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Sunday's Last Breath/⚔️ Test Tactical Limb Combat (Play Mode) _F8", false, 50)]
        [UnityEditor.MenuItem("Tools/Sunday Last Breath/Test Tactical Limb Combat (Play Mode)", false, 50)]
        public static void DebugTriggerCombatFromMenu()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Enter Play Mode First",
                    "Please enter Play Mode first, then press F8 or click this menu item to launch the Tactical Limb Combat test!",
                    "OK");
                return;
            }

            TriggerCombat("zombie_commuter,zombie_guard");
        }
#endif
    }
}
