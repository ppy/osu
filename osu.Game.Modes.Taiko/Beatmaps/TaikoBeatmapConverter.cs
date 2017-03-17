// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Taiko.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : IBeatmapConverter<TaikoHitObject>
    {
        public Beatmap<TaikoHitObject> Convert(Beatmap original)
        {
            if (original is IIsLegacy)
                original.TimingInfo.ControlPoints.ForEach(c => c.VelocityAdjustment *= 1.4);

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
            IHasDistance distanceData = original as IHasDistance;
            IHasRepeats repeatsData = original as IHasRepeats;
            IHasEndTime endTimeData = original as IHasEndTime;

            if (distanceData != null)
            {
                return new DrumRoll
                {
                    StartTime = original.StartTime,
                    Sample = original.Sample,

                    Distance = distanceData.Distance * (repeatsData?.RepeatCount ?? 1)
                };
            }

            if (endTimeData != null)
            {
                return new Bash
                {
                    StartTime = original.StartTime,
                    Sample = original.Sample,

                    EndTime = endTimeData.EndTime
                };
            }

            return new TaikoHitObject
            {
                StartTime = original.StartTime,
                Sample = original.Sample,
            };
        }
    }
}
