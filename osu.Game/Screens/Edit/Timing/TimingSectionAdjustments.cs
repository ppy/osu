// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Timing
{
    public static class TimingSectionAdjustments
    {
        public static List<HitObject> HitObjectsInTimingRange(IBeatmap beatmap, double time)
        {
            // If the first group, we grab all hitobjects prior to the next, if the last group, we grab all remaining hitobjects
            double startTime = beatmap.ControlPointInfo.TimingPoints.Any(x => x.Time < time) ? time : double.MinValue;
            double endTime = beatmap.ControlPointInfo.TimingPoints.FirstOrDefault(x => x.Time > time)?.Time ?? double.MaxValue;

            return beatmap.HitObjects.Where(x => Precision.AlmostBigger(x.StartTime, startTime) && Precision.DefinitelyBigger(endTime, x.StartTime)).ToList();
        }

        public static void AdjustHitObjectOffset(IBeatmap beatmap, TimingControlPoint timingControlPoint, double adjust)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint.Time))
            {
                hitObject.StartTime += adjust;
            }
        }

        public static void SetHitObjectBPM(IBeatmap beatmap, TimingControlPoint timingControlPoint, double oldBeatLength)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint.Time))
            {
                double beat = (hitObject.StartTime - timingControlPoint.Time) / oldBeatLength;

                hitObject.StartTime = (beat * timingControlPoint.BeatLength) + timingControlPoint.Time;

                if (hitObject is not IHasRepeats && hitObject is IHasDuration hitObjectWithDuration)
                    hitObjectWithDuration.Duration *= timingControlPoint.BeatLength / oldBeatLength;
            }
        }
    }
}
