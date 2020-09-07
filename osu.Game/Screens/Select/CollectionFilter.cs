// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Collections;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// A <see cref="BeatmapCollection"/> filter.
    /// </summary>
    public class CollectionFilter
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
        /// Creates a new <see cref="CollectionFilter"/>.
        /// </summary>
        /// <param name="collection">The collection to filter beatmaps from.</param>
        public CollectionFilter([CanBeNull] BeatmapCollection collection)
        {
            Collection = collection;
            CollectionName = Collection?.Name.GetBoundCopy() ?? new Bindable<string>("All beatmaps");
        }

        /// <summary>
        /// Whether the collection contains a given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to check.</param>
        /// <returns>Whether <see cref="Collection"/> contains <paramref name="beatmap"/>.</returns>
        public virtual bool ContainsBeatmap(BeatmapInfo beatmap)
            => Collection?.Beatmaps.Any(b => b.Equals(beatmap)) ?? true;
    }
}
