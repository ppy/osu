// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModLargerCircle : Mod, IApplicableToDifficulty
    {
        public override string Name => "Larger Circle";
        public override string Acronym => "LC";
        public override IconUsage Icon => FontAwesome.Solid.ChevronCircleUp;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 1.0;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const int ratio = 1;
            difficulty.CircleSize *= ratio;
        }
    }
}
