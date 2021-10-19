// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapModelManager : IModelManager<BeatmapSetInfo>
    {
        /// <summary>
        /// Provide an online lookup queue component to handle populating online beatmap metadata.
        /// </summary>
        BeatmapOnlineLookupQueue OnlineLookupQueue { set; }

        /// <summary>
        /// Provide a working beatmap cache, used to invalidate entries on changes.
        /// </summary>
        IWorkingBeatmapCache WorkingBeatmapCache { set; }
    }
}
