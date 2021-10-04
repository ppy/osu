// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Audio;

namespace osu.Game.Rulesets.Mods
{
    public class ClockWithMods : IApplicableToRate
    {
        protected IApplicableToRate Mod;

        public ClockWithMods(Mod[] mods)
        {
            Mod = mods.OfType<IApplicableToRate>().SingleOrDefault();
        }

        public double ApplyToRate(double time, double rate = 1) => Mod?.ApplyToRate(time) ?? 1;

        public double GetAverageRate() => Mod?.GetAverageRate() ?? 1;

        public double GetTimeAt(double time) => Mod?.GetTimeAt(time) ?? time;

        public void ApplyToSample(DrawableSample sample)
        {
            throw new NotImplementedException();
        }

        public void ApplyToTrack(ITrack track)
        {
            throw new NotImplementedException();
        }
    }
}
