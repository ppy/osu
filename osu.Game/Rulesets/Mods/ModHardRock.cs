// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
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
        public override LocalisableString Description => "Everything just got a bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModDifficultyAdjust) };
        public override bool EligibleForPP => UsesDefaultConfiguration;

        protected const float ADJUST_RATIO = 1.4f;

        public void ReadFromDifficulty(IBeatmapDifficultyInfo difficulty)
        {
        }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = Math.Min(difficulty.DrainRate * ADJUST_RATIO, 10.0f);
            difficulty.OverallDifficulty = Math.Min(difficulty.OverallDifficulty * ADJUST_RATIO, 10.0f);
        }
    }
}
