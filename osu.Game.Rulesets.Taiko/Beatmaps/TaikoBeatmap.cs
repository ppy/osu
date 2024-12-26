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

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Hit Count",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    Content = hits.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = @"Drumroll Count",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                    Content = drumRolls.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = @"Swell Count",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                    Content = swells.ToString(),
                }
            };
        }

        public void InvertTypes(Predicate<(int, TaikoHitObject)> need_invert)
        {
            for (int index = 0; index < HitObjects.Count; index++)
            {
                var obj = HitObjects[index];
                if (need_invert((index, obj)))
                {
                    var newValue = Hit.InvertType(obj);
                    if (newValue is not null)
                        HitObjects[index] = newValue;
                }
            }
        }
    }
}
