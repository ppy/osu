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
        /// Calculates and sets the effective BPM for each note object, considering clock rate and scroll speed.
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
                setEffectiveBPMForObject(controlPointInfo, currentNoteObject, currentControlPoint, clockRate);
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
        /// Calculates and sets the effective BPM for the given note object based on the current control point and clock rate.
        /// </summary>
        private void setEffectiveBPMForObject(ControlPointInfo controlPointInfo, TaikoDifficultyHitObject currentNoteObject, TimingControlPoint? currentControlPoint, double clockRate)
        {
            if (currentControlPoint != null)
            {
                var activeEffectControlPoint = controlPointInfo.EffectPointAt(currentNoteObject.StartTime);
                double currentSliderVelocity = (activeEffectControlPoint?.ScrollSpeed ?? 1.0) * clockRate;

                currentNoteObject.EffectiveBPM = currentControlPoint.BPM * globalSliderVelocity * currentSliderVelocity;
            }
        }
    }
}
