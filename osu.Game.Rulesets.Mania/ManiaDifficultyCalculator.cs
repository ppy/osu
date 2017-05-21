// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaDifficultyCalculator : DifficultyCalculator<ManiaHitObject>
    {
        public ManiaDifficultyCalculator(Beatmap beatmap)
            : base(beatmap)
        {
        }

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            return 0;
        }

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter();
    }
}