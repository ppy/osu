﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
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
                    Name = @"Circle Count",
                    Content = circles.ToString(),
                    Icon = FontAwesome.fa_circle_o
                },
                new BeatmapStatistic
                {
                    Name = @"Slider Count",
                    Content = sliders.ToString(),
                    Icon = FontAwesome.fa_circle
                },
                new BeatmapStatistic
                {
                    Name = @"Spinner Count",
                    Content = spinners.ToString(),
                    Icon = FontAwesome.fa_circle
                }
            };
        }
    }
}
