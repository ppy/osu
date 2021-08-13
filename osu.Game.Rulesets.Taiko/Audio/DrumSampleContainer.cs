// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        private readonly IBindableList<SampleControlPoint> samplePoints = new BindableList<SampleControlPoint>();

        public DrumSampleContainer(ControlPointInfo controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            samplePoints.BindTo(controlPoints.SamplePoints);
            samplePoints.BindCollectionChanged((_, __) => recreateMappings(), true);
        }

        private void recreateMappings()
        {
            mappings.Clear();
            ClearInternal();

            SampleControlPoint[] points = samplePoints.Count == 0
                ? new[] { controlPoints.SamplePointAt(double.MinValue) }
                : samplePoints.ToArray();

            for (int i = 0; i < points.Length; i++)
            {
                var samplePoint = points[i];

                var lifetimeStart = i > 0 ? samplePoint.Time : double.MinValue;
                var lifetimeEnd = i + 1 < points.Length ? points[i + 1].Time : double.MaxValue;

                AddInternal(mappings[samplePoint.Time] = new DrumSample(samplePoint)
                {
                    LifetimeStart = lifetimeStart,
                    LifetimeEnd = lifetimeEnd
                });
            }
        }

        public DrumSample SampleAt(double time) => mappings[controlPoints.SamplePointAt(time).Time];

        public class DrumSample : CompositeDrawable
        {
            public override bool RemoveWhenNotAlive => false;

            public PausableSkinnableSound Centre { get; private set; }
            public PausableSkinnableSound Rim { get; private set; }

            private readonly SampleControlPoint samplePoint;

            private Bindable<string> sampleBank;
            private BindableNumber<int> sampleVolume;

            public DrumSample(SampleControlPoint samplePoint)
            {
                this.samplePoint = samplePoint;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                sampleBank = samplePoint.SampleBankBindable.GetBoundCopy();
                sampleBank.BindValueChanged(_ => recreate());

                sampleVolume = samplePoint.SampleVolumeBindable.GetBoundCopy();
                sampleVolume.BindValueChanged(_ => recreate());

                recreate();
            }

            private void recreate()
            {
                InternalChildren = new Drawable[]
                {
                    Centre = new PausableSkinnableSound(samplePoint.GetSampleInfo()),
                    Rim = new PausableSkinnableSound(samplePoint.GetSampleInfo(HitSampleInfo.HIT_CLAP))
                };
            }
        }
    }
}
