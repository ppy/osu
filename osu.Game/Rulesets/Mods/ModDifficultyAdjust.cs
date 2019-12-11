// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDifficultyAdjust : Mod, IApplicableToDifficulty
    {
        public override string Name => @"Difficulty Adjust";

        public override string Description => @"Override a beatmap's difficulty settings";

        public override string Acronym => "DA";

        public override ModType Type => ModType.Conversion;

        public override IconUsage Icon => FontAwesome.Solid.Hammer;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModHardRock) };

        public virtual BindableNumber<float> DrainRate { get; }

        public virtual BindableNumber<float> CircleSize { get; }

        public virtual BindableNumber<float> ApproachRate { get; }

        public virtual BindableNumber<float> OverallDifficulty { get; }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = DrainRate != null ? DrainRate.Value : difficulty.DrainRate;
            difficulty.CircleSize = CircleSize != null ? CircleSize.Value : difficulty.CircleSize;
            difficulty.ApproachRate = ApproachRate != null ? ApproachRate.Value : difficulty.ApproachRate;
            difficulty.OverallDifficulty = OverallDifficulty != null ? OverallDifficulty.Value : difficulty.OverallDifficulty;
        }
    }
}
