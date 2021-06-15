// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace osu.Game.Rulesets.Objects.Pooling
{
    /// <summary>
    /// Manages a mapping between <see cref="HitObject"/> and <see cref="HitObjectLifetimeEntry"/>
    /// </summary>
    internal class HitObjectEntryManager
    {
        /// <summary>
        /// All entries, including entries of the nested hit objects.
        /// </summary>
        public IEnumerable<HitObjectLifetimeEntry> AllEntries => entryMap.Values;

        public event Action<HitObjectLifetimeEntry, HitObject?>? OnEntryAdded;
        public event Action<HitObjectLifetimeEntry, HitObject?>? OnEntryRemoved;

        private readonly Func<HitObject, HitObjectLifetimeEntry> createLifetimeEntry;

        private readonly Dictionary<HitObject, HitObjectLifetimeEntry> entryMap = new Dictionary<HitObject, HitObjectLifetimeEntry>();
        private readonly Dictionary<HitObjectLifetimeEntry, HitObject?> parentMap = new Dictionary<HitObjectLifetimeEntry, HitObject?>();
        private readonly Dictionary<HitObject, List<HitObjectLifetimeEntry>> childrenMap = new Dictionary<HitObject, List<HitObjectLifetimeEntry>>();

        public HitObjectEntryManager(Func<HitObject, HitObjectLifetimeEntry> createLifetimeEntry)
        {
            this.createLifetimeEntry = createLifetimeEntry;
        }

        public HitObjectLifetimeEntry Add(HitObject hitObject, HitObject? parent)
        {
            if (entryMap.ContainsKey(hitObject))
                throw new InvalidOperationException($@"The {nameof(HitObject)} is already added to this {nameof(HitObjectEntryManager)}.");

            var entry = createLifetimeEntry(hitObject);
            entryMap[hitObject] = entry;
            parentMap[entry] = parent;

            if (parent != null && childrenMap.TryGetValue(parent, out var parentChildEntries))
                parentChildEntries.Add(entry);

            hitObject.DefaultsApplied += onDefaultsApplied;

            childrenMap[entry.HitObject] = new List<HitObjectLifetimeEntry>();

            OnEntryAdded?.Invoke(entry, parent);
            return entry;
        }

        public bool Remove(HitObject hitObject)
        {
            if (!entryMap.Remove(hitObject, out var entry))
                return false;

            parentMap.Remove(entry, out var parent);

            if (parent != null && childrenMap.TryGetValue(parent, out var parentChildEntries))
                parentChildEntries.Remove(entry);

            hitObject.DefaultsApplied -= onDefaultsApplied;

            // Remove all entries of the nested hit objects
            if (childrenMap.Remove(entry.HitObject, out var childEntries))
            {
                foreach (var childEntry in childEntries)
                    Remove(childEntry.HitObject);
            }

            OnEntryRemoved?.Invoke(entry, parent);
            return true;
        }

        public bool TryGet(HitObject hitObject, [MaybeNullWhen(false)] out HitObjectLifetimeEntry entry)
        {
            return entryMap.TryGetValue(hitObject, out entry);
        }

        /// <summary>
        /// As nested hit objects are recreated, remove entries of the old nested hit objects.
        /// </summary>
        private void onDefaultsApplied(HitObject hitObject)
        {
            if (!childrenMap.Remove(hitObject, out var childEntries))
                return;

            foreach (var entry in childEntries)
                Remove(entry.HitObject);

            childEntries.Clear();
            childrenMap[hitObject] = childEntries;
        }
    }
}
