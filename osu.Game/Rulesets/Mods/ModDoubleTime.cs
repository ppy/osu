// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDoubleTime : Mod, IApplicableToClock
    {
        public override string Name => "Double Time";
        public override string ShortenedName => "DT";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Zoooooooooom...";
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHalfTime) };

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            clock.Rate = 1.5;
        }
    }
}
