// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModConstant : Mod, IApplicableToBeatmap
    {
        public override string Name => "Constant";
        public override string Acronym => "CT";
        public override double ScoreMultiplier => 1;
        public override string Description => "No more tricky note speed changes!";
        public override IconUsage? Icon => FontAwesome.Solid.ArrowDown;
        public override ModType Type => ModType.Conversion;
        public override bool Ranked => false;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            // Get all difficulty points
            var difficultyPoints = beatmap.ControlPointInfo.DifficultyPoints;

            foreach (var difficultyPoint in difficultyPoints)
            {
                // Set this difficulty point's speed multiplier to 1
                difficultyPoint.SpeedMultiplier = 1;
            }
        }
    }
}
