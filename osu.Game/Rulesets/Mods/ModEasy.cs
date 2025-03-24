// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod, IApplicableToDifficulty
    {
        public override string Name => "Easy";
        public override string Acronym => "EZ";
        public override IconUsage? Icon => OsuIcon.ModEasy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock), typeof(ModDifficultyAdjust) };
        public override bool Ranked => UsesDefaultConfiguration;
        [SettingSource("Preserve Approach Rate", "Keeps the map's original Approach Rate instead of reducing it.")]
        public BindableBool PreserveApproachRate { get; } = new BindableBool
        {
            Value = false,
            Default = false
        };
        public virtual void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }
        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 0.5f;
            
            difficulty.CircleSize *= ratio;
            if (!PreserveApproachRate.Value)
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
            }
    }
}
