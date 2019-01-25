// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore : ModDoubleTime
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override string Description => "Uguuuuuuuu...";

        public override void ApplyToClock(IAdjustableClock clock)
        {
            var pitchAdjust = clock as IHasPitchAdjust;
            if (pitchAdjust != null)
                pitchAdjust.PitchAdjust = 1.5;
            else
                base.ApplyToClock(clock);
        }
    }
}
