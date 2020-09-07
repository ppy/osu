// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Collections;

namespace osu.Game.Screens.Select
{
    public class CollectionFilter
    {
        [CanBeNull]
        public readonly BeatmapCollection Collection;

        public CollectionFilter([CanBeNull] BeatmapCollection collection)
        {
            Collection = collection;
        }

        public virtual bool ContainsBeatmap(BeatmapInfo beatmap)
            => Collection?.Beatmaps.Any(b => b.Equals(beatmap)) ?? true;
    }
}
