// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using System.Collections.Generic;

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

        protected override BeatmapConverter<CatchBaseHit> CreateBeatmapConverter() => new CatchBeatmapConverter();
    }
}