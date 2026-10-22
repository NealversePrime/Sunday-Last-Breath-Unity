using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SundayLastBreath.Data;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Tactical HUD controller for Survivor Core Attributes (M.O.R.T.A.L.) and Combat Skills.
    /// Replaces raw ASCII text dumps with illuminated segmented gauge bars, mastery tiers,
    /// and cybernetic status cards.
    /// </summary>
    public class TacticalSkillsViewController : MonoBehaviour
    {
        [System.Serializable]
        public class StatRowUI
        {
            public string key;
            public string displayName;
            public string shortTag;
            public TextMeshProUGUI valueText;
            public TextMeshProUGUI tierText;
            public List<Image> pips = new List<Image>();
            public Color filledColor = new Color(0.18f, 0.80f, 0.44f); // Neon green / cyan
            public Color emptyColor = new Color(0.10f, 0.12f, 0.16f, 1f);
        }

        [Header("Attribute Cards (M.O.R.T.A.L.)")]
        [SerializeField] private List<StatRowUI> attributeRows = new List<StatRowUI>();

        [Header("Skill Cards (Combat & Survival)")]
        [SerializeField] private List<StatRowUI> skillRows = new List<StatRowUI>();

        public void BindAttributeRow(string key, string shortTag, string displayName,
                                     TextMeshProUGUI valTmp, List<Image> pips,
                                     Color filledCol, Color emptyCol)
        {
            var row = new StatRowUI
            {
                key = key,
                shortTag = shortTag,
                displayName = displayName,
                valueText = valTmp,
                pips = pips,
                filledColor = filledCol,
                emptyColor = emptyCol
            };
            attributeRows.Add(row);
        }

        public void BindSkillRow(string key, string displayName, TextMeshProUGUI valTmp,
                                 TextMeshProUGUI tierTmp, List<Image> pips,
                                 Color filledCol, Color emptyCol)
        {
            var row = new StatRowUI
            {
                key = key,
                displayName = displayName,
                valueText = valTmp,
                tierText = tierTmp,
                pips = pips,
                filledColor = filledCol,
                emptyColor = emptyCol
            };
            skillRows.Add(row);
        }

        public void Refresh(CharacterData player)
        {
            if (player == null) return;

            // 1. Refresh M.O.R.T.A.L. Attributes (Max 10)
            foreach (var row in attributeRows)
            {
                int val = GetAttributeValue(player, row.key);
                if (row.valueText != null)
                {
                    row.valueText.text = $"<b>{val}</b> <color=#55606e>/ 10</color>";
                }

                for (int i = 0; i < row.pips.Count; i++)
                {
                    if (row.pips[i] != null)
                    {
                        row.pips[i].color = (i < val) ? row.filledColor : row.emptyColor;
                    }
                }
            }

            // 2. Refresh Skills (Max 5)
            foreach (var row in skillRows)
            {
                int val = GetSkillValue(player, row.key);
                if (row.valueText != null)
                {
                    row.valueText.text = $"<b>{val}</b> <color=#55606e>/ 5</color>";
                }

                if (row.tierText != null)
                {
                    string tierName = GetSkillTierName(val);
                    string tierCol = val switch
                    {
                        >= 5 => "#e74c3c", // Master
                        >= 4 => "#f39c12", // Expert
                        >= 3 => "#f1c40f", // Competent
                        >= 2 => "#2ecc71", // Trained
                        >= 1 => "#3498db", // Novice
                        _ => "#7f8c8d"     // Untrained
                    };
                    row.tierText.text = $"<color={tierCol}><b>{tierName}</b></color>";
                }

                for (int i = 0; i < row.pips.Count; i++)
                {
                    if (row.pips[i] != null)
                    {
                        row.pips[i].color = (i < val) ? row.filledColor : row.emptyColor;
                    }
                }
            }
        }

        private int GetAttributeValue(CharacterData player, string key)
        {
            return key.ToUpperInvariant() switch
            {
                "STR" or "MIGHT" => player.str,
                "PER" or "OBSERVATION" => player.per,
                "AGI" or "REFLEX" => player.agi,
                "INT" or "TACTICS" => player.intl,
                "CHA" or "LEADERSHIP" => player.cha,
                "LCK" or "AUSPICE" => player.lck,
                _ => 0
            };
        }

        private int GetSkillValue(CharacterData player, string key)
        {
            return key.ToUpperInvariant() switch
            {
                "SHOOTING" => player.shooting,
                "MELEE" => player.melee,
                "STEALTH" => player.stealth,
                "TECH" => player.tech,
                "MEDICINE" => player.medicine,
                "SCAVENGE" or "SCAVENGING" => player.scavenging,
                _ => 0
            };
        }

        private string GetSkillTierName(int level)
        {
            return level switch
            {
                >= 5 => "MASTER",
                4 => "EXPERT",
                3 => "COMPETENT",
                2 => "TRAINED",
                1 => "NOVICE",
                _ => "UNTRAINED"
            };
        }
    }
}

