// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDaycore : ModHalfTime
    {
        public override string Name => "Daycore";
        public override string ShortenedName => "DC";
        public override FontAwesome Icon => FontAwesome.fa_question;
        public override string Description => "Whoaaaaa...";

        public override void ApplyToClock(IAdjustableClock clock)
        {
            var pitchAdjust = clock as IHasPitchAdjust;
            if (pitchAdjust != null)
                pitchAdjust.PitchAdjust = 0.75;
            else
                base.ApplyToClock(clock);
        }
    }
}
