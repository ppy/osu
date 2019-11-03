// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            IEnumerable<SampleControlPoint> samplePoints = controlPoints.SamplePoints.Count == 0 ? new[] { controlPoints.SamplePointAt(double.MinValue) } : controlPoints.SamplePoints;

            foreach (var s in samplePoints)
            {
                var centre = s.GetSampleInfo();
                var rim = s.GetSampleInfo(HitSampleInfo.HIT_CLAP);

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

        private SkinnableSound addSound(HitSampleInfo hitSampleInfo)
        {
            var drawable = new SkinnableSound(hitSampleInfo);
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
