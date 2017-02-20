// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Taiko.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko
{
    public class TaikoDifficultyCalculator : DifficultyCalculator<TaikoBaseHit>
    {
        protected override PlayMode PlayMode => PlayMode.Taiko;

        public TaikoDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override HitObjectConverter<TaikoBaseHit> Converter => new TaikoConverter();

        protected override double ComputeDifficulty(Dictionary<String, String> categoryDifficulty)
        {
            return 0;
        }
    }
}