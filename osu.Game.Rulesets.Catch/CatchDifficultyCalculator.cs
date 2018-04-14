// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Catch
{
    public class CatchDifficultyCalculator : DifficultyCalculator<CatchHitObject>
    {
        public CatchDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null) => 0;

        protected override BeatmapConverter<CatchHitObject> CreateBeatmapConverter(Beatmap beatmap) => new CatchBeatmapConverter();
    }
}
