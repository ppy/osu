// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Reading
{
    public class EffectiveBPMPreprocessor
    {
        private readonly IList<TaikoDifficultyHitObject> noteObjects;
        private readonly double globalSliderVelocity;

        public EffectiveBPMPreprocessor(IBeatmap beatmap, List<TaikoDifficultyHitObject> noteObjects)
        {
            this.noteObjects = noteObjects;
            globalSliderVelocity = beatmap.Difficulty.SliderMultiplier;
        }

        /// <summary>
        /// Calculates and sets the effective BPM and slider velocity for each note object, considering clock rate and scroll speed.
        /// </summary>
        public void ProcessEffectiveBPM(ControlPointInfo controlPointInfo, double clockRate)
        {
            foreach (var currentNoteObject in noteObjects)
            {
                double startTime = currentNoteObject.StartTime * clockRate;

                // Retrieve the timing point at the note's start time
                TimingControlPoint currentControlPoint = controlPointInfo.TimingPointAt(startTime);

                // Calculate the slider velocity at the note's start time.
                double currentSliderVelocity = calculateSliderVelocity(controlPointInfo, startTime, clockRate);
                currentNoteObject.CurrentSliderVelocity = currentSliderVelocity;

                currentNoteObject.EffectiveBPM = currentControlPoint.BPM * currentSliderVelocity;
            }
        }

        /// <summary>
        /// Calculates the slider velocity based on control point info and clock rate.
        /// </summary>
        private double calculateSliderVelocity(ControlPointInfo controlPointInfo, double startTime, double clockRate)
        {
            var activeEffectControlPoint = controlPointInfo.EffectPointAt(startTime);
            return globalSliderVelocity * (activeEffectControlPoint.ScrollSpeed) * clockRate;
        }
    }
}
