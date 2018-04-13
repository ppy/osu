// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Audio
{
    public class DrumSampleMapping
    {
        private readonly ControlPointInfo controlPoints;
        private readonly Dictionary<double, DrumSample> mappings = new Dictionary<double, DrumSample>();

        public readonly List<SkinnableSound> Sounds = new List<SkinnableSound>();

        public DrumSampleMapping(ControlPointInfo controlPoints)
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
                var centre = s.GetSampleInfo();
                var rim = s.GetSampleInfo(SampleInfo.HIT_CLAP);

                // todo: this is ugly
                centre.Namespace = "taiko";
                rim.Namespace = "taiko";

                mappings[s.Time] = new DrumSample
                {
                    Centre = addSound(centre),
                    Rim = addSound(rim)
                };
            }
        }

        private SkinnableSound addSound(SampleInfo sampleInfo)
        {
            var drawable = new SkinnableSound(sampleInfo);
            Sounds.Add(drawable);
            return drawable;
        }

        public DrumSample SampleAt(double time) => mappings[controlPoints.SamplePointAt(time).Time];

        public class DrumSample
        {
            public SkinnableSound Centre;
            public SkinnableSound Rim;
        }
    }
}
