// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : Mod, IApplicableToClock
    {
        public override string Name => "Half Time";
        public override string Acronym => "HT";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Less zoom...";
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime) };

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            clock.Rate = 0.75;
        }
    }
}
