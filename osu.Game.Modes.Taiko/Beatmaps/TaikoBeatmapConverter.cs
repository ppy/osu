// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Taiko.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : IBeatmapConverter<TaikoHitObject>
    {
        private const float legacy_velocity_scale = 1.4f;
        private const float bash_convert_factor = 1.65f;

        public Beatmap<TaikoHitObject> Convert(Beatmap original)
        {
            if (original is LegacyBeatmap)
                original.TimingInfo.ControlPoints.ForEach(c => c.VelocityAdjustment /= legacy_velocity_scale);

            return new Beatmap<TaikoHitObject>(original)
            {
                HitObjects = convertHitObjects(original.HitObjects)
            };
        }

        private List<TaikoHitObject> convertHitObjects(List<HitObject> hitObjects)
        {
            return hitObjects.Select(convertHitObject).ToList();
        }

        private TaikoHitObject convertHitObject(HitObject original)
        {
            // Check if this HitObject is already a TaikoHitObject, and return it if so
            TaikoHitObject originalTaiko = original as TaikoHitObject;
            if (originalTaiko != null)
                return originalTaiko;

            IHasDistance distanceData = original as IHasDistance;
            IHasRepeats repeatsData = original as IHasRepeats;
            IHasEndTime endTimeData = original as IHasEndTime;

            // Old osu! used hit sounding to determine various hit type information
            SampleType sample = original.Sample?.Type ?? SampleType.None;

            bool strong = (sample & SampleType.Finish) > 0;

            if (distanceData != null)
            {
                return new DrumRoll
                {
                    StartTime = original.StartTime,
                    Sample = original.Sample,
                    IsStrong = strong,

                    Distance = distanceData.Distance * (repeatsData?.RepeatCount ?? 1)
                };
            }

            if (endTimeData != null)
            {
                // We compute the end time manually to add in the Bash convert factor
                return new Swell
                {
                    StartTime = original.StartTime,
                    Sample = original.Sample,
                    IsStrong = strong,

                    EndTime = original.StartTime + endTimeData.Duration * bash_convert_factor 
                };
            }

            bool isCentre = (sample & ~(SampleType.Finish | SampleType.Normal)) == 0;

            if (isCentre)
            {
                return new CentreHit
                {
                    StartTime = original.StartTime,
                    Sample = original.Sample,
                    IsStrong = strong
                };
            }

            return new RimHit
            {
                StartTime = original.StartTime,
                Sample = original.Sample,
                IsStrong = strong,
            };
        }
    }
}
