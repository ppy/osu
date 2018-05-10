// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Catch
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        public CatchDifficultyCalculator(IBeatmap beatmap) : base(beatmap)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null) => 0;
    }
}
