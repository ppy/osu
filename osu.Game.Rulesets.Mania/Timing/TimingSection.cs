// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    public class TimingSection
    {
        public double StartTime;
        public double Duration;
        public double BeatLength;
        public TimeSignatures TimeSignature;
    }
}