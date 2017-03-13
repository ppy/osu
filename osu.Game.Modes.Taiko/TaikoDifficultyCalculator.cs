// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Taiko.Beatmaps;
using osu.Game.Modes.Taiko.Objects;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko
{
    public class TaikoDifficultyCalculator : DifficultyCalculator<TaikoBaseHit>
    {
        public TaikoDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            return 0;
        }

        protected override IBeatmapConverter<TaikoBaseHit> CreateBeatmapConverter() => new TaikoBeatmapConverter();
    }
}