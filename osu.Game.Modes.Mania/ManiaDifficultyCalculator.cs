﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Beatmaps;
using osu.Game.Modes.Mania.Objects;
using System.Collections.Generic;

namespace osu.Game.Modes.Mania
{
    public class ManiaDifficultyCalculator : DifficultyCalculator<ManiaBaseHit>
    {
        public ManiaDifficultyCalculator(Beatmap beatmap)
            : base(beatmap)
        {
        }

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            return 0;
        }

        protected override IBeatmapConverter<ManiaBaseHit> CreateBeatmapConverter() => new ManiaBeatmapConverter();
    }
}