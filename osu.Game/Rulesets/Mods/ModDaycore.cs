// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Timing;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDaycore : ModHalfTime
    {
        public override string Name => "Daycore";
        public override string Acronym => "DC";
        public override IconUsage Icon => FontAwesome.Solid.Question;
        public override string Description => "Whoaaaaa...";

        public override void ApplyToClock(IAdjustableClock clock, double proposedRate = 1)
        {
            const double adjust = 0.75;

            if (clock is IHasPitchAdjust pitchAdjust)
            {
                pitchAdjust.PitchAdjust = adjust;

                // todo: this can be removed once we have tempo support in AddAdjustment
                if (clock is IHasTempoAdjust tempo)
                    tempo.TempoAdjust = 1 / adjust * proposedRate * RateAdjust;
                else
                    clock.Rate = proposedRate;
            }
            else
                base.ApplyToClock(clock, proposedRate);
        }
    }
}
