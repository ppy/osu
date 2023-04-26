// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        /// <summary>
        /// Invoked when a new <see cref="HitObjectLifetimeEntry"/> is added to this <see cref="HitObjectEntryManager"/>..
        /// The second parameter of the event is the parent hit object.
        /// </summary>
        public event Action<HitObjectLifetimeEntry, HitObject?>? OnEntryAdded;

        /// <summary>
        /// Invoked when a <see cref="HitObjectLifetimeEntry"/> is removed from this <see cref="HitObjectEntryManager"/>.
        /// The second parameter of the event is the parent hit object.
        /// </summary>
        public event Action<HitObjectLifetimeEntry, HitObject?>? OnEntryRemoved;

        /// <summary>
        /// Provides the reverse mapping of <see cref="HitObjectLifetimeEntry.HitObject"/> for each entry.
        /// </summary>
        private readonly Dictionary<HitObject, HitObjectLifetimeEntry> entryMap = new Dictionary<HitObject, HitObjectLifetimeEntry>();

        /// <summary>
        /// Stores the parent hit object for entries of the nested hit objects.
        /// </summary>
        /// <remarks>
        /// The parent hit object of a pooled hit object may be non-pooled.
        /// In that case, no corresponding <see cref="HitObjectLifetimeEntry"/> is stored in this <see cref="HitObjectEntryManager"/>.
        /// </remarks>
        private readonly Dictionary<HitObjectLifetimeEntry, HitObject> parentMap = new Dictionary<HitObjectLifetimeEntry, HitObject>();

        /// <summary>
        /// Stores the list of child entries for each hit object managed by this <see cref="HitObjectEntryManager"/>.
        /// </summary>
        private readonly Dictionary<HitObject, List<HitObjectLifetimeEntry>> childrenMap = new Dictionary<HitObject, List<HitObjectLifetimeEntry>>();

        public void Add(HitObjectLifetimeEntry entry, HitObject? parent)
        {
            HitObject hitObject = entry.HitObject;

            if (entryMap.ContainsKey(hitObject))
                throw new InvalidOperationException($@"The {nameof(HitObjectLifetimeEntry)} is already added to this {nameof(HitObjectEntryManager)}.");

            // Add the entry.
            entryMap[hitObject] = entry;
            childrenMap[hitObject] = new List<HitObjectLifetimeEntry>();

            // If the entry has a parent, set it and add the entry to the parent's children.
            if (parent != null)
            {
                parentMap[entry] = parent;
                if (childrenMap.TryGetValue(parent, out var parentChildEntries))
                    parentChildEntries.Add(entry);
            }

            hitObject.DefaultsApplied += onDefaultsApplied;
            OnEntryAdded?.Invoke(entry, parent);
        }

        public void Remove(HitObjectLifetimeEntry entry)
        {
            HitObject hitObject = entry.HitObject;

            if (!entryMap.ContainsKey(hitObject))
                throw new InvalidOperationException($@"The {nameof(HitObjectLifetimeEntry)} is not contained in this {nameof(HitObjectEntryManager)}.");

            entryMap.Remove(hitObject);

            // If the entry has a parent, unset it and remove the entry from the parents' children.
            if (parentMap.Remove(entry, out var parent) && childrenMap.TryGetValue(parent, out var parentChildEntries))
                parentChildEntries.Remove(entry);

            // Remove all the entries' children.
            if (childrenMap.Remove(hitObject, out var childEntries))
            {
                foreach (var childEntry in childEntries)
                    Remove(childEntry);
            }

            hitObject.DefaultsApplied -= onDefaultsApplied;
            OnEntryRemoved?.Invoke(entry, parent);
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

            // Remove all the entries' children. At this point the parents' (this entries') children list has been removed from the map, so this does not cause upwards traversal.
            foreach (var entry in childEntries)
                Remove(entry);

            // The removed children list needs to be added back to the map for the entry to potentially receive children.
            childEntries.Clear();
            childrenMap[hitObject] = childEntries;
        }
    }
}
