// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
            // Get the initial BPM
            var initialBPM = beatmap.ControlPointInfo.TimingPoints[0].BPM;

            foreach (var group in beatmap.ControlPointInfo.Groups)
            {
                // Get the timing control point of this group, if any
                var timingControlPoints = group.ControlPoints.OfType<TimingControlPoint>().ToArray();

                // Get the difficulty control point of this group, if any
                var difficultyControlPoints = group.ControlPoints.OfType<DifficultyControlPoint>().ToArray();

                // If there is a timing control point
                if (timingControlPoints.Any())
                {
                    var timingControlPoint = timingControlPoints[0];

                    DifficultyControlPoint diffControlPoint = new DifficultyControlPoint();

                    diffControlPoint.SpeedMultiplierBindable.MinValue = double.MinValue; // Uncapped minimum value
                    diffControlPoint.SpeedMultiplierBindable.MaxValue = double.MaxValue; // Uncapped maximum value
                    diffControlPoint.SpeedMultiplierBindable.Precision = 0.000000001d; // Arbitrary value, may need changing
                    diffControlPoint.SpeedMultiplier = initialBPM / timingControlPoint.BPM; // Counteract BPM velocity

                    group.Add(diffControlPoint);
                }
                // Else, there is no timing control point, but a difficulty control point
                else if (difficultyControlPoints.Any())
                {
                    var diffControlPoint = difficultyControlPoints[0];

                    group.Remove(diffControlPoint);
                }
            }
        }
    }
}
