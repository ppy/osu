// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Game.Collections;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// A <see cref="BeatmapCollection"/> filter.
    /// </summary>
    public class CollectionMenuItem
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
        /// Creates a new <see cref="CollectionMenuItem"/>.
        /// </summary>
        /// <param name="collection">The collection to filter beatmaps from.</param>
        public CollectionMenuItem([CanBeNull] BeatmapCollection collection)
        {
            Collection = collection;
            CollectionName = Collection?.Name.GetBoundCopy() ?? new Bindable<string>("所有谱面");
        }
    }

    public class AllBeatmapsCollectionMenuItem : CollectionMenuItem
    {
        public AllBeatmapsCollectionMenuItem()
            : base(null)
        {
        }
    }

    public class ManageCollectionsMenuItem : CollectionMenuItem
    {
        public ManageCollectionsMenuItem()
            : base(null)
        {
            CollectionName.Value = "管理收藏夹";
        }
    }
}
