// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHardRock : Mod, IApplicableToDifficulty
    {
        public override string Name => "Hard Rock";
        public override string Acronym => "HR";
        public override IconUsage? Icon => OsuIcon.ModHardRock;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Everything just got a bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModDifficultyAdjust) };

        public void ReadFromDifficulty(IBeatmapDifficultyInfo difficulty)
        {
        }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 1.4f;
            difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 10.0f); // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ratio, 10.0f);
            difficulty.DrainRate = Math.Min(difficulty.DrainRate * ratio, 10.0f);
            difficulty.OverallDifficulty = Math.Min(difficulty.OverallDifficulty * ratio, 10.0f);
        }
    }
}
