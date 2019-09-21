// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod, IApplicableToDifficulty, IApplicableFailOverride, IApplicableToScoreProcessor
    {
        public override string Name => "Easy";
        public override string Acronym => "EZ";
        public override IconUsage Icon => OsuIcon.ModEasy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };

        private int retries = 2;

        private BindableNumber<double> health;

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 0.5f;
            difficulty.CircleSize *= ratio;
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
        }

        public bool AllowFail
        {
            get
            {
                if (retries == 0) return true;

                health.Value = health.MaxValue;
                retries--;

                return false;
            }
        }

        public bool RestartOnFail => false;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            health = scoreProcessor.Health.GetBoundCopy();
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}
