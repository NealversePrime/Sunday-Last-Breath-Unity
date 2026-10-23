using System;
using System.Collections.Generic;
using UnityEngine;

namespace SundayLastBreath.Core
{
    public enum ObjectiveStatus
    {
        Locked,
        Active,
        Completed
    }

    [Serializable]
    public class ObjectiveItem
    {
        public string id;
        public string description;
        public ObjectiveStatus status;

        public ObjectiveItem(string id, string description, ObjectiveStatus status = ObjectiveStatus.Active)
        {
            this.id = id;
            this.description = description;
            this.status = status;
        }
    }

    /// <summary>
    /// System managing active, locked, and completed survival goals.
    /// Mirrored from the Twine objectives architecture.
    /// </summary>
    [Serializable]
    public class ObjectiveSystem
    {
        [SerializeField] private List<ObjectiveItem> objectives = new List<ObjectiveItem>();

        public event Action<ObjectiveItem> OnObjectiveUpdated;

        public IReadOnlyList<ObjectiveItem> Objectives => objectives;

        public ObjectiveSystem()
        {
            // Initial default objectives for Prologue
            objectives.Add(new ObjectiveItem("pack_gear", "Pick up your apartment keys and belongings from the coffee table", ObjectiveStatus.Active));
            objectives.Add(new ObjectiveItem("leave_apt", "Unlock the apartment door and step out into the hallway", ObjectiveStatus.Locked));
        }

        public void AddObjective(string id, string desc, ObjectiveStatus status = ObjectiveStatus.Active)
        {
            var existing = objectives.Find(o => o.id == id);
            if (existing != null)
            {
                existing.description = desc;
                existing.status = status;
                OnObjectiveUpdated?.Invoke(existing);
            }
            else
            {
                var newObj = new ObjectiveItem(id, desc, status);
                objectives.Add(newObj);
                OnObjectiveUpdated?.Invoke(newObj);
            }
        }

        public void CompleteObjective(string id)
        {
            var found = objectives.Find(o => o.id == id);
            if (found != null)
            {
                found.status = ObjectiveStatus.Completed;
                OnObjectiveUpdated?.Invoke(found);
            }
        }

        public void ActivateObjective(string id)
        {
            var found = objectives.Find(o => o.id == id);
            if (found != null)
            {
                found.status = ObjectiveStatus.Active;
                OnObjectiveUpdated?.Invoke(found);
            }
        }

        public ObjectiveItem GetActiveObjective()
        {
            return objectives.Find(o => o.status == ObjectiveStatus.Active);
        }

        public string GetActiveObjectiveText()
        {
            var active = GetActiveObjective();
            return active != null ? active.description : "None";
        }
    }
}

