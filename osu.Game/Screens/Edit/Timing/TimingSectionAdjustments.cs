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
        /// <summary>
        /// Returns all objects from <paramref name="beatmap"/> which are affected by the supplied <paramref name="timingControlPoint"/>.
        /// </summary>
        public static List<HitObject> HitObjectsInTimingRange(IBeatmap beatmap, TimingControlPoint timingControlPoint)
        {
            // If the first group, we grab all hitobjects prior to the next, if the last group, we grab all remaining hitobjects
            double startTime = beatmap.ControlPointInfo.TimingPoints.Any(x => x.Time < timingControlPoint.Time) ? timingControlPoint.Time : double.MinValue;
            double endTime = beatmap.ControlPointInfo.TimingPoints.FirstOrDefault(x => x.Time > timingControlPoint.Time)?.Time ?? double.MaxValue;

            return beatmap.HitObjects.Where(x => Precision.AlmostBigger(x.StartTime, startTime) && Precision.DefinitelyBigger(endTime, x.StartTime)).ToList();
        }

        /// <summary>
        /// Moves all relevant objects after <paramref name="timingControlPoint"/>'s offset has been changed by <paramref name="adjustment"/>.
        /// </summary>
        public static void AdjustHitObjectOffset(IBeatmap beatmap, TimingControlPoint timingControlPoint, double adjustment)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint))
            {
                hitObject.StartTime += adjustment;
            }
        }

        /// <summary>
        /// Ensures all relevant objects are still snapped to the same beats after <paramref name="timingControlPoint"/>'s beat length / BPM has been changed.
        /// </summary>
        public static void SetHitObjectBPM(IBeatmap beatmap, TimingControlPoint timingControlPoint, double oldBeatLength)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint))
            {
                double beat = (hitObject.StartTime - timingControlPoint.Time) / oldBeatLength;

                hitObject.StartTime = (beat * timingControlPoint.BeatLength) + timingControlPoint.Time;

                if (hitObject is not IHasRepeats && hitObject is IHasDuration hitObjectWithDuration)
                    hitObjectWithDuration.Duration *= timingControlPoint.BeatLength / oldBeatLength;
            }
        }
    }
}
