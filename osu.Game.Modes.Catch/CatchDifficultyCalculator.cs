// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Catch.Objects;
using osu.Game.Modes.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Catch
{
    public class CatchDifficultyCalculator : DifficultyCalculator<CatchBaseHit>
    {
        protected override PlayMode PlayMode => PlayMode.Catch;

        public CatchDifficultyCalculator(Beatmap beatmap) : base(beatmap)
        {
        }

        protected override HitObjectConverter<CatchBaseHit> Converter => new CatchConverter();

        protected override double ComputeDifficulty(Dictionary<String, String> categoryDifficulty)
        {
            return 0;
        }
    }
}