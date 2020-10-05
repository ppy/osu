// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Audio
{
    /// <summary>
    /// Stores samples for the input drum.
    /// The lifetime of the samples is adjusted so that they are only alive during the appropriate sample control point.
    /// </summary>
    public class DrumSampleContainer : LifetimeManagementContainer
    {
        private readonly ControlPointInfo controlPoints;
        private readonly Dictionary<double, DrumSample> mappings = new Dictionary<double, DrumSample>();

        public DrumSampleContainer(ControlPointInfo controlPoints)
        {
            this.controlPoints = controlPoints;

            IReadOnlyList<SampleControlPoint> samplePoints = controlPoints.SamplePoints.Count == 0 ? new[] { controlPoints.SamplePointAt(double.MinValue) } : controlPoints.SamplePoints;

            for (int i = 0; i < samplePoints.Count; i++)
            {
                var samplePoint = samplePoints[i];

                var centre = samplePoint.GetSampleInfo();
                var rim = samplePoint.GetSampleInfo(HitSampleInfo.HIT_CLAP);

                var lifetimeStart = i > 0 ? samplePoint.Time : double.MinValue;
                var lifetimeEnd = i + 1 < samplePoints.Count ? samplePoints[i + 1].Time : double.MaxValue;

                mappings[samplePoint.Time] = new DrumSample
                {
                    Centre = addSound(centre, lifetimeStart, lifetimeEnd),
                    Rim = addSound(rim, lifetimeStart, lifetimeEnd)
                };
            }
        }

        private PausableSkinnableSound addSound(HitSampleInfo hitSampleInfo, double lifetimeStart, double lifetimeEnd)
        {
            var drawable = new PausableSkinnableSound(hitSampleInfo)
            {
                LifetimeStart = lifetimeStart,
                LifetimeEnd = lifetimeEnd
            };
            AddInternal(drawable);
            return drawable;
        }

        public DrumSample SampleAt(double time) => mappings[controlPoints.SamplePointAt(time).Time];

        public class DrumSample
        {
            public PausableSkinnableSound Centre;
            public PausableSkinnableSound Rim;
        }
    }
}
