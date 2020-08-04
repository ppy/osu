// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod, IApplicableToDifficulty, IApplicableFailOverride, IApplicableToHealthProcessor
    {
        public override string Name => "Easy";
        public override string Acronym => "EZ";
        public override IconUsage? Icon => OsuIcon.ModEasy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock), typeof(ModDifficultyAdjust) };

        [SettingSource("Extra Lives", "Number of extra lives")]
        public Bindable<int> Retries { get; } = new BindableInt(2)
        {
            MinValue = 0,
            MaxValue = 10
        };

        public override string SettingDescription => Retries.IsDefault ? string.Empty : $"{"lives".ToQuantity(Retries.Value)}";

        private int retries;

        private BindableNumber<double> health;

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

            retries = Retries.Value;
        }

        public bool PerformFail()
        {
            if (retries == 0) return true;

            health.Value = health.MaxValue;
            retries--;

            return false;
        }

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            health = healthProcessor.Health.GetBoundCopy();
        }
    }
}
