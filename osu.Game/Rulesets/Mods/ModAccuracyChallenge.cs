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
        public override string Name => "Accuracy Challenge";

        public override string Acronym => "AC";

        public override LocalisableString Description => "Fail if your accuracy drops too low!";

        public override ModType Type => ModType.DifficultyIncrease;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModEasyWithExtraLives), typeof(ModPerfect) }).ToArray();

        public override bool RequiresConfiguration => false;

        public override string SettingDescription => base.SettingDescription.Replace(MinimumAccuracy.ToString(), MinimumAccuracy.Value.ToString("##%", NumberFormatInfo.InvariantInfo));

        [SettingSource("Minimum accuracy", "Trigger a failure if your accuracy goes below this value.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> MinimumAccuracy { get; } = new BindableDouble
        {
            MinValue = 0.60,
            MaxValue = 0.99,
            Precision = 0.01,
            Default = 0.9,
            Value = 0.9,
        };

        private int baseScore;
        private int maxBaseScore;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.NewJudgement += j =>
            {
                if (!j.Type.AffectsAccuracy())
                    return;

                baseScore += Judgement.ToNumericResult(j.Type);
                maxBaseScore += Judgement.ToNumericResult(j.Judgement.MaxResult);
            };

            scoreProcessor.JudgementReverted += j =>
            {
                if (!j.Type.AffectsAccuracy())
                    return;

                baseScore -= Judgement.ToNumericResult(j.Type);
                maxBaseScore -= Judgement.ToNumericResult(j.Judgement.MaxResult);
            };
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!result.Type.AffectsAccuracy())
                return false;

            return getAccuracyWithImminentResultAdded(result) < MinimumAccuracy.Value;
        }

        private double getAccuracyWithImminentResultAdded(JudgementResult result)
        {
            // baseScore and maxBaseScore are always exactly one judgement behind because the health processor is processed first (see: Player).
            int imminentBaseScore = baseScore + Judgement.ToNumericResult(result.Type);
            int imminentMaxBaseScore = maxBaseScore + Judgement.ToNumericResult(result.Judgement.MaxResult);

            return imminentMaxBaseScore > 0 ? imminentBaseScore / (double)imminentMaxBaseScore : 1;
        }
    }
}
