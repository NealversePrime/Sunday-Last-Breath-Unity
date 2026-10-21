using System;
using System.Collections.Generic;
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
    /// Item lookup utility and icon resolver for inventory and gear stashes.
    /// </summary>
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

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

            if (_iconCache.TryGetValue(itemId, out var icon))
            {
                return icon;
            }

            // Normalization attempts: "Kitchen Knife" -> "kitchen_knife"
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
    }
}
