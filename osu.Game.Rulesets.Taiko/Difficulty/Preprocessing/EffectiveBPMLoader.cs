// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class EffectiveBPMLoader
    {
        private readonly IBeatmap beatmap;
        private readonly IList<TaikoDifficultyHitObject> noteObjects;
        private readonly IReadOnlyList<TimingControlPoint?> timingControlPoints;
        private readonly double globalSliderVelocity;

        public EffectiveBPMLoader(IBeatmap beatmap, List<TaikoDifficultyHitObject> noteObjects)
        {
            this.beatmap = beatmap;
            this.noteObjects = noteObjects;
            timingControlPoints = beatmap.ControlPointInfo.TimingPoints;
            globalSliderVelocity = beatmap.Difficulty.SliderMultiplier;
        }

        /// <summary>
        /// Calculates and sets the effective BPM and slider velocity for each note object, considering clock rate and scroll speed.
        /// </summary>
        public void LoadEffectiveBPM(ControlPointInfo controlPointInfo, double clockRate)
        {
            using var controlPointEnumerator = timingControlPoints.GetEnumerator();
            controlPointEnumerator.MoveNext();
            var currentControlPoint = controlPointEnumerator.Current;
            var nextControlPoint = controlPointEnumerator.MoveNext() ? controlPointEnumerator.Current : null;

            foreach (var currentNoteObject in noteObjects)
            {
                currentControlPoint = getNextControlPoint(currentNoteObject, currentControlPoint, ref nextControlPoint, controlPointEnumerator);

                // Calculate and set slider velocity for the current note object.
                double currentSliderVelocity = calculateSliderVelocity(controlPointInfo, currentNoteObject.StartTime, clockRate);
                currentNoteObject.CurrentSliderVelocity = currentSliderVelocity;

                setEffectiveBPMForObject(currentNoteObject, currentControlPoint, currentSliderVelocity);
            }
        }

        /// <summary>
        /// Advances to the next timing control point if the current note's start time exceeds the current control point's time.
        /// </summary>
        private TimingControlPoint? getNextControlPoint(TaikoDifficultyHitObject currentNoteObject, TimingControlPoint? currentControlPoint, ref TimingControlPoint? nextControlPoint, IEnumerator<TimingControlPoint?> controlPointEnumerator)
        {
            if (nextControlPoint != null && currentNoteObject.StartTime > nextControlPoint.Time)
            {
                currentControlPoint = nextControlPoint;
                nextControlPoint = controlPointEnumerator.MoveNext() ? controlPointEnumerator.Current : null;
            }

            return currentControlPoint;
        }

        /// <summary>
        /// Calculates the slider velocity based on control point info and clock rate.
        /// </summary>
        private double calculateSliderVelocity(ControlPointInfo controlPointInfo, double startTime, double clockRate)
        {
            var activeEffectControlPoint = controlPointInfo.EffectPointAt(startTime);
            return globalSliderVelocity * (activeEffectControlPoint?.ScrollSpeed ?? 1.0) * clockRate;
        }

        /// <summary>
        /// Sets the effective BPM for the given note object.
        /// </summary>
        private void setEffectiveBPMForObject(TaikoDifficultyHitObject currentNoteObject, TimingControlPoint? currentControlPoint, double currentSliderVelocity)
        {
            if (currentControlPoint != null)
            {
                currentNoteObject.EffectiveBPM = currentControlPoint.BPM * currentSliderVelocity;
            }
        }
    }
}
