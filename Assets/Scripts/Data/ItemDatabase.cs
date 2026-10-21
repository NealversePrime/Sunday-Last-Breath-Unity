using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SundayLastBreath.Data
{
    [Serializable]
    public class ItemIconMapping
    {
        public string itemId;
        public Sprite icon;

        public ItemIconMapping(string id, Sprite icon)
        {
            this.itemId = id;
            this.icon = icon;
        }
    }

    /// <summary>
    /// Item & Trait icon lookup utility and equipment slot classifier for the Survivor PDA.
    /// </summary>
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> _traitIconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static bool _editorInitialized = false;

        private static readonly Dictionary<string, string> _itemAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Glock 19", "glock-19" },
            { "9mm Ammo", "9mm" },
            { "Sledgehammer", "sledgehammer" },
            { "Duct Tape", "duct_tape" },
            { "Combat Knife", "knife" },
            { "Kitchen Knife", "knife" },
            { "Lockpicks", "lockpicks" },
            { "Flashlight", "flashlight" },
            { "Scalpel", "scalpel" },
            { "First Aid Kit", "p3k" },
            { "Antibiotics", "antibiotics" },
            { "Golf Club", "golf_club" },
            { "Luxury Watch", "watch" },
            { "Baseball Bat", "baseball_bat" },
            { "Textbook", "encyclopedia" },
            { "Encyclopedia", "encyclopedia" },
            { "Energy Bar", "energy_bar" },
            { "Running Shoes", "running_shoes" },
            { "Multi-Tool", "multitool" },
            { "Binoculars", "binoculars" },
            { "Fuse", "fuse" },
            { "Hiking Boots", "hiking_boots" },
            { "Compound Bow", "bow" },
            { "Arrows", "arrows" },
            { "Walkie-Talkie", "speaker" },
            { "Battery", "batteries" },
            { "Cigarettes", "cigarettes" },
            { "Lighter", "lighter" },
            { "Whiskey", "whiskey" },
            { "Inhaler", "inhaler" },
            { "Glasses", "glasses" },
            { "Cane", "cane" },
            { "Bag of Chips", "chips" },
            { "Bandage", "bandage" },
            { "Tinfoil Hat", "tinfoil_hat" },
            { "Sleeping Pills", "sleeping_pills" },
            { "Hand Sanitizer", "sanitizer" },
            { "Pepper Spray", "pepper_spray" },
            { "Jerry Can", "jerrycan" },
            { "Old Map", "map" },
            { "Hard Hat", "hardhat" },
            { "Deck of Cards", "cards" },
            { "Designer Bag", "handbag" },
            { "Stale Coffee", "coffee" },
            { "T-Shirt", "tshirt" },
            { "Tier-1 Military Clearance Passkey", "fuse" }
        };

        private static readonly Dictionary<string, string> _traitAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CLEAN", "trait_germophobe" },
            { "SHOPPER", "trait_shopaholic" },
            { "BAD KNEE", "trait_badknee" },
            { "LONE WOLF", "trait_lonewolf" },
            { "RADIO OPERATOR", "trait_radio" },
            { "GUNSLINGER", "perk_gunslinger" },
            { "SHOTGUNNER", "perk_shotgunner" },
            { "BLUNT BRUISER", "perk_blunt_bruiser" },
            { "BLADESMITH", "perk_bladesmith" },
            { "IRON STOMACH", "perk_iron_stomach" },
            { "FIELD FORAGER", "perk_field_forager" },
            { "SECOND WIND", "perk_second_wind" },
            { "IRON WILL", "perk_iron_will" },
            { "CRISIS NEGOTIATOR", "perk_crisis_negotiator" },
            { "COMMANDING PRESENCE", "perk_commanding_presence" },
            { "FIELD MEDIC", "perk_iron_stomach" },
            { "FAST LEARNER", "perk_second_wind" },
            { "ADRENALINE", "perk_second_wind" }
        };

        private static void EnsureLoaded()
        {
            if (_editorInitialized && _iconCache.Count > 0) return;
            _editorInitialized = true;

#if UNITY_EDITOR
            LoadFolderEditor("Assets/Sprites/Items", _iconCache);
            LoadFolderEditor("Assets/Sprites/Traits", _traitIconCache);
            LoadFolderEditor("Assets/Sprites/Perks", _traitIconCache);
#endif
        }

#if UNITY_EDITOR
        private static void LoadFolderEditor(string folder, Dictionary<string, Sprite> target)
        {
            if (!Directory.Exists(folder)) return;
            string[] files = Directory.GetFiles(folder, "*.png");
            foreach (string f in files)
            {
                string relPath = f.Replace("\\", "/");
                UnityEditor.TextureImporter ti = UnityEditor.AssetImporter.GetAtPath(relPath) as UnityEditor.TextureImporter;
                if (ti != null && (ti.textureType != UnityEditor.TextureImporterType.Sprite || !ti.alphaIsTransparency))
                {
                    ti.textureType = UnityEditor.TextureImporterType.Sprite;
                    ti.alphaIsTransparency = true;
                    ti.SaveAndReimport();
                }
                Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(relPath);
                if (s != null)
                {
                    string key = Path.GetFileNameWithoutExtension(f);
                    target[key] = s;
                }
            }
        }
#endif

        public static void RegisterIcon(string itemId, Sprite icon)
        {
            if (string.IsNullOrEmpty(itemId) || icon == null) return;
            _iconCache[itemId] = icon;
        }

        public static void RegisterIcons(IEnumerable<ItemIconMapping> mappings)
        {
            if (mappings == null) return;
            foreach (var m in mappings)
            {
                if (m != null && !string.IsNullOrEmpty(m.itemId) && m.icon != null)
                {
                    _iconCache[m.itemId] = m.icon;
                }
            }
        }

        public static Sprite GetItemIcon(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            EnsureLoaded();

            if (_iconCache.TryGetValue(itemId, out var icon)) return icon;

            if (_itemAliases.TryGetValue(itemId.Trim(), out string aliasedKey) &&
                _iconCache.TryGetValue(aliasedKey, out var aliasedIcon))
            {
                return aliasedIcon;
            }

            string normalized = itemId.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            foreach (var kvp in _iconCache)
            {
                string keyNorm = kvp.Key.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
                if (keyNorm == normalized || keyNorm.Contains(normalized) || normalized.Contains(keyNorm))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        public static Sprite GetTraitIcon(string traitName)
        {
            if (string.IsNullOrEmpty(traitName)) return null;
            EnsureLoaded();

            if (_traitIconCache.TryGetValue(traitName, out var icon)) return icon;

            if (_traitAliases.TryGetValue(traitName.Trim(), out string aliasedKey) &&
                _traitIconCache.TryGetValue(aliasedKey, out var aliasedIcon))
            {
                return aliasedIcon;
            }

            string norm = traitName.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            if (_traitIconCache.TryGetValue("trait_" + norm, out var tIcon)) return tIcon;
            if (_traitIconCache.TryGetValue("perk_" + norm, out var pIcon)) return pIcon;

            foreach (var kvp in _traitIconCache)
            {
                string keyNorm = kvp.Key.Trim().ToLowerInvariant();
                if (keyNorm.Contains(norm) || norm.Contains(keyNorm.Replace("trait_", "").Replace("perk_", "")))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns "HEAD", "BODY", "WPN", "LEGS", "FEET", or null if the item is a general backpack item.
        /// </summary>
        public static string GetEquipSlot(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            string n = itemId.Trim().ToLowerInvariant();

            if (n.Contains("hat") || n.Contains("glasses") || n.Contains("helmet")) return "HEAD";
            if (n.Contains("shirt") || n.Contains("jacket") || n.Contains("vest") || n.Contains("coat")) return "BODY";
            if (n.Contains("glock") || n.Contains("knife") || n.Contains("sledgehammer") || n.Contains("scalpel") ||
                n.Contains("club") || n.Contains("bat") || n.Contains("bow") || n.Contains("cane") ||
                n.Contains("revolver") || n.Contains("rifle") || n.Contains("shotgun")) return "WPN";
            if (n.Contains("jeans") || n.Contains("pants") || n.Contains("trousers")) return "LEGS";
            if (n.Contains("shoes") || n.Contains("boots") || n.Contains("sneakers")) return "FEET";

            return null;
        }
    }
}
