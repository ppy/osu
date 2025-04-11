// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmap : Beatmap<OsuHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int circles = HitObjects.Count(c => c is HitCircle);
            int sliders = HitObjects.Count(s => s is Slider);
            int spinners = HitObjects.Count(s => s is Spinner);
            int sum = Math.Max(1, circles + sliders + spinners);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Circles",
                    Content = circles.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    Ratio = circles / (float)sum,
                },
                new BeatmapStatistic
                {
                    Name = "Sliders",
                    Content = sliders.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                    Ratio = sliders / (float)sum,
                },
                new BeatmapStatistic
                {
                    Name = @"Spinners",
                    Content = spinners.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                    Ratio = Math.Min(spinners / 10f, 1),
                }
            };
        }
    }
}
