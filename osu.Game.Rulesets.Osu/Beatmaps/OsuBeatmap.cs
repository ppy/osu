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
            int maximum = circles + sliders + spinners;

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Circles",
                    Content = circles.ToString(),
                    Count = circles,
                    Maximum = maximum,
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                },
                new BeatmapStatistic
                {
                    Name = "Sliders",
                    Content = sliders.ToString(),
                    Count = sliders,
                    Maximum = maximum,
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                },
                new BeatmapStatistic
                {
                    Name = @"Spinners",
                    Content = spinners.ToString(),
                    Count = spinners,
                    Maximum = Math.Max(spinners, 10),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                }
            };
        }
    }
}
