// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Timing;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeAdjust : Mod
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModTimeRamp) };

        protected abstract double RateAdjust { get; }

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            if (clock is IHasTempoAdjust tempo)
                tempo.TempoAdjust *= RateAdjust;
            else
                clock.Rate *= RateAdjust;
        }
    }
}
