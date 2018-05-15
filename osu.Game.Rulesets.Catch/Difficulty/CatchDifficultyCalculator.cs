// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        public CatchDifficultyCalculator(IBeatmap beatmap) : base(beatmap)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null) => 0;
    }
}
