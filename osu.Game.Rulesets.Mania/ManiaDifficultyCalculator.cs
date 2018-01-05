// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
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

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null) => 0;

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter(Beatmap beatmap) => new ManiaBeatmapConverter(true, beatmap);
    }
}
