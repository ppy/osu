// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuDistanceSnapGrid : CircularDistanceSnapGrid
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        public OsuDistanceSnapGrid(OsuHitObject hitObject)
            : base(hitObject, hitObject.StackedEndPosition)
        {
        }

        protected override float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(time);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(time);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            return (float)(scoringDistance / timingPoint.BeatLength);
        }
    }
}
