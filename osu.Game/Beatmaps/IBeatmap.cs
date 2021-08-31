// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    public interface IBeatmap
    {
        /// <summary>
        /// This beatmap's info.
        /// </summary>
        BeatmapInfo BeatmapInfo { get; set; }

        /// <summary>
        /// This beatmap's metadata.
        /// </summary>
        BeatmapMetadata Metadata { get; }

        /// <summary>
        /// The control points in this beatmap.
        /// </summary>
        ControlPointInfo ControlPointInfo { get; set; }

        /// <summary>
        /// The breaks in this beatmap.
        /// </summary>
        List<BreakPeriod> Breaks { get; }

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
        double TotalBreakTime { get; }

        /// <summary>
        /// The hitobjects contained by this beatmap.
        /// </summary>
        IReadOnlyList<HitObject> HitObjects { get; }

        /// <summary>
        /// Returns statistics for the <see cref="HitObjects"/> contained in this beatmap.
        /// </summary>
        IEnumerable<BeatmapStatistic> GetStatistics();

        /// <summary>
        /// Finds the most common beat length represented by the control points in this beatmap.
        /// </summary>
        double GetMostCommonBeatLength();

        /// <summary>
        /// Creates a shallow-clone of this beatmap and returns it.
        /// </summary>
        /// <returns>The shallow-cloned beatmap.</returns>
        IBeatmap Clone();
    }

    public interface IBeatmap<out T> : IBeatmap
        where T : HitObject
    {
        /// <summary>
        /// The hitobjects contained by this beatmap.
        /// </summary>
        new IReadOnlyList<T> HitObjects { get; }
    }
}
