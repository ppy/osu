// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModAccuracyChallenge : ModFailCondition, IApplicableToScoreProcessor
    {
        public override string Name => "精准挑战";

        public override string Acronym => "AC";

        public override LocalisableString Description => "准确率过低时原地去世！";

        public override ModType Type => ModType.DifficultyIncrease;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModEasyWithExtraLives), typeof(ModPerfect) }).ToArray();

        public override bool RequiresConfiguration => false;

        public override string SettingDescription => base.SettingDescription.Replace(MinimumAccuracy.ToString(), MinimumAccuracy.Value.ToString("##%", NumberFormatInfo.InvariantInfo));

        [SettingSource("最低准确率", "准确率低于此数值时将触发失败", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> MinimumAccuracy { get; } = new BindableDouble
        {
            MinValue = 0.60,
            MaxValue = 0.99,
            Precision = 0.01,
            Default = 0.9,
            Value = 0.9,
        };

        private ScoreProcessor scoreProcessor = null!;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor) => this.scoreProcessor = scoreProcessor;

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!result.Type.AffectsAccuracy())
                return false;

            return getAccuracyWithImminentResultAdded(result) < MinimumAccuracy.Value;
        }

        private double getAccuracyWithImminentResultAdded(JudgementResult result)
        {
            var score = new ScoreInfo { Ruleset = scoreProcessor.Ruleset.RulesetInfo };

            // This is super ugly, but if we don't do it this way we will not have the most recent result added to the accuracy value.
            // Hopefully we can improve this in the future.
            scoreProcessor.PopulateScore(score);
            score.Statistics[result.Type]++;

            return scoreProcessor.ComputeAccuracy(score);
        }
    }
}
