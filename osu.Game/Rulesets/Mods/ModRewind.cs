// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModRewind : Mod, IApplicableToClock
    {
        public override string Name => "Rewind";

        public override string Acronym => "RW";

        public override double ScoreMultiplier => 1.0;

        public void ApplyToClock(IAdjustableClock clock)
        {
            switch (clock)
            {
                case IHasPitchAdjust pitch:
                    pitch.PitchAdjust = -1;
                    break;
                case IHasTempoAdjust tempo:
                    tempo.TempoAdjust = -1;
                    break;
                default:
                    clock.Rate = -1;
                    break;
            }
        }
    }
}
