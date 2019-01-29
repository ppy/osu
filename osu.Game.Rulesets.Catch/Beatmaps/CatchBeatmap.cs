// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmap : Beatmap<CatchHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int fruits = HitObjects.Count(s => s is Fruit);
            int juiceStreams = HitObjects.Count(s => s is JuiceStream);
            int bananaShowers = HitObjects.Count(s => s is BananaShower);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Fruit Count",
                    Content = fruits.ToString(),
                    Icon = FontAwesome.fa_circle_o
                },
                new BeatmapStatistic
                {
                    Name = @"Juice Stream Count",
                    Content = juiceStreams.ToString(),
                    Icon = FontAwesome.fa_circle
                },
                new BeatmapStatistic
                {
                    Name = @"Banana Shower Count",
                    Content = bananaShowers.ToString(),
                    Icon = FontAwesome.fa_circle
                }
            };
        }
    }
}
