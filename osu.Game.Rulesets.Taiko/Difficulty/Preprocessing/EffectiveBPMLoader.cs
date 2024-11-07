using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class EffectiveBPMLoader
    {
        private IBeatmap beatmap;
        private readonly IList<TaikoDifficultyHitObject> noteObjects;
        private readonly IReadOnlyList<TimingControlPoint?> controlPoints;
        private readonly double beatmapGlobalSv;

        public EffectiveBPMLoader(IBeatmap beatmap, List<TaikoDifficultyHitObject> noteObjects)
        {
            controlPoints = beatmap.ControlPointInfo.TimingPoints;
            beatmapGlobalSv = beatmap.Difficulty.SliderMultiplier;
            this.beatmap = beatmap;
            this.noteObjects = noteObjects;
        }

        public void LoadEffectiveBPM(ControlPointInfo controlPointInfo)
        {
            using IEnumerator<TimingControlPoint?> controlPointEnumerator = controlPoints.GetEnumerator();
            controlPointEnumerator.MoveNext();
            TimingControlPoint? currentControlPoint = controlPointEnumerator.Current;
            TimingControlPoint? nextControlPoint = controlPointEnumerator.MoveNext() ? controlPointEnumerator.Current : null;
            using IEnumerator<TaikoDifficultyHitObject> noteObjectEnumerator = noteObjects.GetEnumerator();

            while (noteObjectEnumerator.MoveNext())
            {
                TaikoDifficultyHitObject currentNoteObject = noteObjectEnumerator.Current;

                if (nextControlPoint != null && currentNoteObject.StartTime > nextControlPoint.Time)
                {
                    currentControlPoint = nextControlPoint;
                    nextControlPoint = controlPointEnumerator.MoveNext() ? controlPointEnumerator.Current : null;
                }

                // Find the active EffectControlPoint at the start time of the hit object.
                EffectControlPoint activeEffectControlPoint = controlPointInfo.EffectPointAt(currentNoteObject.StartTime);

                // Use the ScrollSpeed from the activeEffectControlPoint.
                double currentSliderVelocity = activeEffectControlPoint?.ScrollSpeed ?? 1.0; // Fallback to 1.0 if null

                if (currentControlPoint != null)
                {
                    currentNoteObject.EffectiveBPM = currentControlPoint.BPM * beatmapGlobalSv * currentSliderVelocity;
                }
            }
        }
    }
}
