// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    public interface IBeatmap : IJsonSerializable
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
        ControlPointInfo ControlPointInfo { get; }

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
        IEnumerable<HitObject> HitObjects { get; }

        /// <summary>
        /// Returns statistics for the <see cref="HitObjects"/> contained in this beatmap.
        /// </summary>
        /// <returns></returns>
        IEnumerable<BeatmapStatistic> GetStatistics();

        /// <summary>
        /// Creates a shallow-clone of this beatmap and returns it.
        /// </summary>
        /// <returns>The shallow-cloned beatmap.</returns>
        IBeatmap Clone();
    }
}
