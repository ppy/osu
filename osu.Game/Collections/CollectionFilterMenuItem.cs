// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;

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
        public readonly Live<BeatmapCollection>? Collection;

        /// <summary>
        /// The name of the collection.
        /// </summary>
        public string CollectionName { get; }

        /// <summary>
        /// Creates a new <see cref="CollectionFilterMenuItem"/>.
        /// </summary>
        /// <param name="collection">The collection to filter beatmaps from.</param>
        public CollectionFilterMenuItem(Live<BeatmapCollection> collection)
            : this(collection.PerformRead(c => c.Name))
        {
            Collection = collection;
        }

        protected CollectionFilterMenuItem(string name)
        {
            CollectionName = name;
        }

        public virtual bool Equals(CollectionFilterMenuItem? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Collection == null) return false;

            return Collection.ID == other.Collection?.ID;
        }

        public override int GetHashCode() => Collection?.ID.GetHashCode() ?? 0;
    }

    public class AllBeatmapsCollectionFilterMenuItem : CollectionFilterMenuItem
    {
        public AllBeatmapsCollectionFilterMenuItem()
            : base("All beatmaps")
        {
        }

        public override bool Equals(CollectionFilterMenuItem? other) => other is AllBeatmapsCollectionFilterMenuItem;

        public override int GetHashCode() => 1;
    }

    public class ManageCollectionsFilterMenuItem : CollectionFilterMenuItem
    {
        public ManageCollectionsFilterMenuItem()
            : base("Manage collections...")
        {
        }

        public override bool Equals(CollectionFilterMenuItem? other) => other is ManageCollectionsFilterMenuItem;

        public override int GetHashCode() => 2;
    }
}
