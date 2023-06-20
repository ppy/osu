// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Rulesets.Mods
{
    public class ClockWithMods
    {
        protected IApplicableToRate? Mod;

        public ClockWithMods(Mod[] mods)
        {
            Mod = mods.OfType<IApplicableToRate>().SingleOrDefault();
        }

        public double ApplyToRate(double time, double rate = 1) => Mod?.ApplyToRate(time) ?? 1;

        public double GetAverageRate() => Mod?.GetAverageRate() ?? 1;

        public double GetTimeAt(double time) => Mod?.GetTimeAt(time) ?? time;
    }
}
