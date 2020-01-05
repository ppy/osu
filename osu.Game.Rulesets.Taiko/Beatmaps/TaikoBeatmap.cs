// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
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
                    Name = @"击打数",
                    Content = hits.ToString(),
                    Icon = FontAwesome.Regular.Circle
                },
                new BeatmapStatistic
                {
                    Name = @"滑条数量",
                    Content = drumrolls.ToString(),
                    Icon = FontAwesome.Regular.Circle
                },
                new BeatmapStatistic
                {
                    Name = @"转盘数量",
                    Content = swells.ToString(),
                    Icon = FontAwesome.Regular.Circle
                }
            };
        }
    }
}
