// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Objects;
using osu.Game.Modes.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Mania
{
    public class ManiaDifficultyCalculator : DifficultyCalculator<ManiaBaseHit>
    {
        protected override PlayMode PlayMode => PlayMode.Mania;

        private int columns;

        public ManiaDifficultyCalculator(Beatmap beatmap, int columns = 5) : base(beatmap)
        {
            this.columns = columns;
        }

        protected override HitObjectConverter<ManiaBaseHit> Converter => new ManiaConverter(columns);

        protected override double ComputeDifficulty(Dictionary<String, String> categoryDifficulty)
        {
            return 0;
        }
    }
}