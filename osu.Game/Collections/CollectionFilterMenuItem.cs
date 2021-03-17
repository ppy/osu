// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;

namespace osu.Game.Collections
{
    /// <summary>
    /// A <see cref="BeatmapCollection"/> filter.
    /// </summary>
    public class CollectionFilterMenuItem : IEquatable<CollectionFilterMenuItem>
    {
        /// <summary>
        /// The collection to filter beatmaps from.
        /// May be null to not filter by collection (include all beatmaps).
        /// </summary>
        [CanBeNull]
        public readonly BeatmapCollection Collection;

        /// <summary>
        /// The name of the collection.
        /// </summary>
        [NotNull]
        public readonly Bindable<string> CollectionName;

        /// <summary>
        /// Creates a new <see cref="CollectionFilterMenuItem"/>.
        /// </summary>
        /// <param name="collection">The collection to filter beatmaps from.</param>
        public CollectionFilterMenuItem([CanBeNull] BeatmapCollection collection)
        {
            Collection = collection;
            CollectionName = Collection?.Name.GetBoundCopy() ?? new Bindable<string>("All beatmaps");
        }

        public bool Equals(CollectionFilterMenuItem other)
        {
            if (other == null)
                return false;

            // collections may have the same name, so compare first on reference equality.
            // this relies on the assumption that only one instance of the BeatmapCollection exists game-wide, managed by CollectionManager.
            if (Collection != null)
                return Collection == other.Collection;

            // fallback to name-based comparison.
            // this is required for special dropdown items which don't have a collection (all beatmaps / manage collections items below).
            return CollectionName.Value == other.CollectionName.Value;
        }

        public override int GetHashCode() => CollectionName.Value.GetHashCode();
    }

    public class AllBeatmapsCollectionFilterMenuItem : CollectionFilterMenuItem
    {
        public AllBeatmapsCollectionFilterMenuItem()
            : base(null)
        {
        }
    }

    public class ManageCollectionsFilterMenuItem : CollectionFilterMenuItem
    {
        public ManageCollectionsFilterMenuItem()
            : base(null)
        {
            CollectionName.Value = "Manage collections...";
        }
    }
}
