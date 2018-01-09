// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Audio
{
    public class DrumSampleMapping
    {
        private readonly ControlPointInfo controlPoints;
        private readonly Dictionary<double, DrumSample> mappings = new Dictionary<double, DrumSample>();

        public DrumSampleMapping(ControlPointInfo controlPoints, AudioManager audio)
        {
            this.controlPoints = controlPoints;

            IEnumerable<SampleControlPoint> samplePoints;
            if (controlPoints.SamplePoints.Count == 0)
                // Get the default sample point
                samplePoints = new[] { controlPoints.SamplePointAt(double.MinValue) };
            else
                samplePoints = controlPoints.SamplePoints;

            foreach (var s in samplePoints)
            {
                mappings[s.Time] = new DrumSample
                {
                    Centre = s.GetSampleInfo().GetChannel(audio.Sample, "Taiko"),
                    Rim = s.GetSampleInfo(SampleInfo.HIT_CLAP).GetChannel(audio.Sample, "Taiko")
                };
            }
        }

        public DrumSample SampleAt(double time) => mappings[controlPoints.SamplePointAt(time).Time];

        public class DrumSample
        {
            public SampleChannel Centre;
            public SampleChannel Rim;
        }
    }
}
