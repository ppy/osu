// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    public interface IWorkingBeatmapCache
    {
        /// <summary>
        /// Retrieve a <see cref="WorkingBeatmap"/> instance for the provided <see cref="BeatmapInfo"/>
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to lookup.</param>
        /// <returns>A <see cref="WorkingBeatmap"/> instance correlating to the provided <see cref="BeatmapInfo"/>.</returns>
        WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo);

        /// <summary>
        /// Invalidate a cache entry if it exists.
        /// </summary>
        /// <param name="beatmapSetInfo">The beatmap set info to invalidate any cached entries for.</param>
        void Invalidate(BeatmapSetInfo beatmapSetInfo);

        /// <summary>
        /// Invalidate a cache entry if it exists.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap info to invalidate any cached entries for.</param>
        void Invalidate(BeatmapInfo beatmapInfo);
    }
}
