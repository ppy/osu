// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModConstant : Mod, IApplicableToBeatmap
    {
        public override string Name => "Constant";
        public override string Acronym => "CT";
        public override double ScoreMultiplier => 1;
        public override string Description => "No more tricky note speed changes!";
        public override IconUsage? Icon => FontAwesome.Solid.BalanceScale;
        public override ModType Type => ModType.Conversion;
        public override bool Ranked => false;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            // Get all difficulty points
            var difficultyPoints = beatmap.ControlPointInfo.DifficultyPoints;
            // Get all timing points
            var timingPoints = beatmap.ControlPointInfo.TimingPoints;
            // Get the initial BPM
            var initialBPM = timingPoints[0].BPM;

            foreach (var difficultyPoint in difficultyPoints)
            {
                // Set this difficulty point's speed multiplier to 1
                difficultyPoint.SpeedMultiplier = 1;
            }

            // Correct changes in speed caused by timing points by adding an associated difficulty point
            foreach (var timingPoint in timingPoints)
            {
                // Create a new difficulty point to counteract timing point
                DifficultyControlPoint diffControlPoint = new DifficultyControlPoint
                {
                    SpeedMultiplier = initialBPM / timingPoint.BPM
                };

                // Add the new control point to the beatmap
                beatmap.ControlPointInfo.Add(timingPoint.Time, diffControlPoint);
            }
        }
    }
}
