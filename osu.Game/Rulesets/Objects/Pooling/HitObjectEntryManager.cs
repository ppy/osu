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
        private readonly Dictionary<HitObject, HitObject> parentMap = new Dictionary<HitObject, HitObject>();

        public HitObjectEntryManager(Func<HitObject, HitObjectLifetimeEntry> createLifetimeEntry)
        {
            this.createLifetimeEntry = createLifetimeEntry;
        }

        public HitObjectLifetimeEntry Add(HitObject hitObject, HitObject? parentHitObject)
        {
            if (parentHitObject != null && !entryMap.TryGetValue(parentHitObject, out var parentEntry))
                throw new InvalidOperationException($@"The parent {nameof(HitObject)} must be added to this {nameof(HitObjectEntryManager)} before nested {nameof(HitObject)} is added.");

            if (entryMap.ContainsKey(hitObject))
                throw new InvalidOperationException($@"The {nameof(HitObject)} is already added to this {nameof(HitObjectEntryManager)}.");

            if (parentHitObject != null)
                parentMap[hitObject] = parentHitObject;

            var entry = createLifetimeEntry(hitObject);
            entryMap[hitObject] = entry;

            OnEntryAdded?.Invoke(entry, parentHitObject);
            return entry;
        }

        public bool Remove(HitObject hitObject)
        {
            if (!entryMap.TryGetValue(hitObject, out var entry))
                return false;

            parentMap.Remove(hitObject, out var parentHitObject);

            OnEntryRemoved?.Invoke(entry, parentHitObject);
            return true;
        }

        public bool TryGet(HitObject hitObject, [MaybeNullWhen(false)] out HitObjectLifetimeEntry entry)
        {
            return entryMap.TryGetValue(hitObject, out entry);
        }
    }
}
