// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : Mod, IApplicableToClock
    {
        public override string Name => "Half Time";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Less zoom";
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime) };

        public override double ScoreMultiplier => 1.12;

        public void ApplyToClock(IAdjustableClock clock)
        {
            clock.Rate = 0.75;
        }
    }
}