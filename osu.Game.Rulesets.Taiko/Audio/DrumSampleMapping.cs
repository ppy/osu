// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Audio
{
    public class DrumSampleMapping : IComparable<DrumSampleMapping>
    {
        public double Time;
        public readonly SampleInfo Centre;
        public readonly SampleInfo Rim;

        public SampleChannel CentreChannel { get; private set; }
        public SampleChannel RimChannel { get; private set; }

        public DrumSampleMapping()
        {
        }

        public DrumSampleMapping(SampleControlPoint samplePoint)
        {
            Time = samplePoint.Time;
            Centre = samplePoint.GetSampleInfo();
            Rim = samplePoint.GetSampleInfo(SampleInfo.HIT_CLAP);
        }

        public void RetrieveChannels(AudioManager audio)
        {
            CentreChannel = Centre.GetChannel(audio.Sample);
            RimChannel = Rim.GetChannel(audio.Sample);
        }

        public int CompareTo(DrumSampleMapping other) => Time.CompareTo(other.Time);
    }
}
