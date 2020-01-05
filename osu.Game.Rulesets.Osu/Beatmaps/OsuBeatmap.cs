// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
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

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"圆圈数量",
                    Content = circles.ToString(),
                    Icon = FontAwesome.Regular.Circle
                },
                new BeatmapStatistic
                {
                    Name = @"滑条数量",
                    Content = sliders.ToString(),
                    Icon = FontAwesome.Regular.Circle
                },
                new BeatmapStatistic
                {
                    Name = @"转盘数量",
                    Content = spinners.ToString(),
                    Icon = FontAwesome.Regular.Circle
                }
            };
        }
    }
}
