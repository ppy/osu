// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class SwellSampleMapping : IComparable<SwellSampleMapping>
    {
        public double Time;
        public SampleInfo Centre;
        public SampleInfo Rim;

        public SampleChannel CentreChannel { get; private set; }
        public SampleChannel RimChannel { get; private set; }

        public void RetrieveChannels(AudioManager audio)
        {
            CentreChannel = Centre.GetChannel(audio.Sample);
            RimChannel = Rim.GetChannel(audio.Sample);
        }

        public int CompareTo(SwellSampleMapping other) => Time.CompareTo(other.Time);
    }
}
