// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Beatmaps
{
    public class TaikoBeatmap : Beatmap<TaikoHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int hits = HitObjects.Count(s => s is Hit);
            int drumRolls = HitObjects.Count(s => s is DrumRoll);
            int swells = HitObjects.Count(s => s is Swell);
            int sum = Math.Max(1, hits + drumRolls + swells);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Hits",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    Content = hits.ToString(),
                    Ratio = hits / (float)sum,
                },
                new BeatmapStatistic
                {
                    Name = @"Drumrolls",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                    Content = drumRolls.ToString(),
                    Ratio = drumRolls / (float)sum,
                },
                new BeatmapStatistic
                {
                    Name = @"Swells",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                    Content = swells.ToString(),
                    Ratio = Math.Min(swells / 10f, 1),
                }
            };
        }
    }
}
