// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModHealthChallenge : ModChallenge, IApplicableToHealthProcessor
    {
        public override string Name => "Health Challenge";
        public override string Acronym => "HC";
        public override string Description => "Hold your health or you fail";

        [SettingSource("Health Threshold", "Set your own health threshold")]
        public Bindable<double> HealthThreshold { get; } = new BindableAccuracy
        {
            Precision = 0.01,
            MinValue = 0.50,
            MaxValue = 1.00,
            Default = 0.96,
            Value = 0.96,
        };

        public override string SettingDescription => $"{HealthThreshold}";

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.FailConditions += checkThresholdCondition;
        }

        private bool checkThresholdCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            // Need to manually checked here since HasCompleted is updated after ApplyResultInternal
            bool allJudged = healthProcessor.JudgedHits == healthProcessor.MaxHits;

            if (!allJudged) return false;

            return result.HealthAtJudgement < HealthThreshold.Value;
        }

        private class BindableAccuracy : BindableDouble
        {
            public override string ToString() => $"{Value:0%}";
        }
    }
}
