// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEasy : Mod, IApplicableToDifficulty, IApplicableFailOverride
    {
        public override string Name => "Easy";
        public override string Acronym => "EZ";
        public override string Description => @"Beats move slower, less accuracy required";
        public override IconUsage? Icon => OsuIcon.ModEasy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock), typeof(ModDifficultyAdjust) };

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 0.5f;
            difficulty.CircleSize *= ratio;
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
        }

        public bool PerformFail() => true;

        public bool RestartOnFail => false;

    }
}
