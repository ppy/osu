// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            int drumrolls = HitObjects.Count(s => s is DrumRoll);
            int swells = HitObjects.Count(s => s is Swell);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Hit Count",
                    CreateIcon = () => new BeatmapStatisticSprite("circles"),
                    Content = hits.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = @"Drumroll Count",
                    CreateIcon = () => new BeatmapStatisticSprite("sliders"),
                    Content = drumrolls.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = @"Swell Count",
                    CreateIcon = () => new BeatmapStatisticSprite("spinners"),
                    Content = swells.ToString(),
                }
            };
        }
    }
}
