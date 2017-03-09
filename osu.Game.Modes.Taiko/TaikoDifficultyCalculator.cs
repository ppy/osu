﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko
{
    public class TaikoDifficultyCalculator : DifficultyCalculator<TaikoHitObject>
    {
        protected override PlayMode PlayMode => PlayMode.Taiko;

        public TaikoDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override HitObjectConverter<TaikoHitObject> Converter => new TaikoConverter();

        protected override double CalculateInternal(Dictionary<string, string> categoryDifficulty)
        {
            //TODO: implement
            return 0;
        }
    }
}