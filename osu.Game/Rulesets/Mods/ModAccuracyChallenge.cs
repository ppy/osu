// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAccuracyChallenge : ModChallenge
    {
        public override string Name => "Accuracy Challenge";
        public override string Acronym => "AC";
        public override IconUsage? Icon => FontAwesome.Solid.Calculator;
        public override string Description => "Fail the beatmap if your accuracy goes below a specified value.";

        [SettingSource("Minimum accuracy", "Fail map if your accuracy goes under this value.")]
        public BindableNumber<double> MinimumAccuracy { get; } = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 100,
            Default = 90,
            Value = 90,
            Precision = 0.01,
        };

        private double baseScore;
        private double maxBaseScore;
        private double accuracy => maxBaseScore > 0 ? baseScore / maxBaseScore : 1;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            incrementInternalScoresFromJudgementResult(result);
            if (!AllowChallengeFailureAtHitObject(result.HitObject))
                return false;
            return accuracy < MinimumAccuracy.Value / 100;
        }

        private void incrementInternalScoresFromJudgementResult(JudgementResult result)
        {
            if (!result.Type.IsScorable() || result.Type.IsBonus())
                return;
            baseScore += result.Type.IsHit() ? result.Judgement.NumericResultFor(result) : 0;
            maxBaseScore += result.Judgement.MaxNumericResult;
        }
    }
}