// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmap : Beatmap<OsuHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            IEnumerable<HitObject> circles = HitObjects.Where(c => !(c is IHasEndTime));
            IEnumerable<HitObject> sliders = HitObjects.Where(s => s is IHasCurve);
            IEnumerable<HitObject> spinners = HitObjects.Where(s => s is IHasEndTime && !(s is IHasCurve));

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Circle Count",
                    Content = circles.Count().ToString(),
                    Icon = FontAwesome.fa_circle_o
                },
                new BeatmapStatistic
                {
                    Name = @"Slider Count",
                    Content = sliders.Count().ToString(),
                    Icon = FontAwesome.fa_circle
                },
                new BeatmapStatistic
                {
                    Name = @"Spinner Count",
                    Content = spinners.Count().ToString(),
                    Icon = FontAwesome.fa_circle
                }
            };
        }
    }
}
