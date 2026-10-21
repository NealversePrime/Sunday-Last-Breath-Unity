using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Core
{
    [Serializable]
    public class LoreEntry
    {
        public string title;
        public string content;
        public string timestamp;

        public LoreEntry(string title, string content, string timestamp = "")
        {
            this.title = title;
            this.content = content;
            this.timestamp = timestamp;
        }
    }

    /// <summary>
    /// Lore & Intelligence Database.
    /// Stores intercepted radio chatter, government leaks, and digital clues.
    /// </summary>
    [Serializable]
    public class LoreSystem
    {
        [SerializeField] private List<LoreEntry> entries = new List<LoreEntry>();

        public event Action<LoreEntry> OnLoreAdded;

        public IReadOnlyList<LoreEntry> Entries => entries;

        public void AddLore(string title, string content, string timestamp = "")
        {
            if (entries.Exists(e => e.title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return;

            var newEntry = new LoreEntry(title, content, timestamp);
            entries.Add(newEntry);
            OnLoreAdded?.Invoke(newEntry);
        }

        public bool HasLore(string title)
        {
            return entries.Exists(e => e.title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }
    }
}
