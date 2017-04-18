// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch
{
    public class CatchDifficultyCalculator : DifficultyCalculator<CatchBaseHit>
    {
        public CatchDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            return 0;
        }

        protected override IBeatmapConverter<CatchBaseHit> CreateBeatmapConverter() => new CatchBeatmapConverter();
    }
}