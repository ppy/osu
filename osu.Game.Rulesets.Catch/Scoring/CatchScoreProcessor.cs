// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public partial class CatchScoreProcessor : ScoreProcessor
    {
        private const double accuracy_cutoff_x = 1;
        private const double accuracy_cutoff_s = 0.98;
        private const double accuracy_cutoff_a = 0.94;
        private const double accuracy_cutoff_b = 0.9;
        private const double accuracy_cutoff_c = 0.85;
        private const double accuracy_cutoff_d = 0;

        private const int combo_cap = 200;
        private const double combo_base = 4;

        private double fruitTinyScale;

        public CatchScoreProcessor()
            : base(new CatchRuleset())
        {
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            // large ticks are *purposefully* not counted to match stable
            int fruitTinyScaleDivisor = MaximumResultCounts.GetValueOrDefault(HitResult.SmallTickHit) + MaximumResultCounts.GetValueOrDefault(HitResult.Great);
            fruitTinyScale = fruitTinyScaleDivisor == 0
                ? 0
                : (double)MaximumResultCounts.GetValueOrDefault(HitResult.SmallTickHit) / fruitTinyScaleDivisor;
        }

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            const int max_tiny_droplets_portion = 400000;

            double comboPortion = 1000000 - max_tiny_droplets_portion + max_tiny_droplets_portion * (1 - fruitTinyScale);
            double dropletsPortion = max_tiny_droplets_portion * fruitTinyScale;
            double dropletsHit = MaximumResultCounts.GetValueOrDefault(HitResult.SmallTickHit) == 0
                ? 0
                : (double)ScoreResultCounts.GetValueOrDefault(HitResult.SmallTickHit) / MaximumResultCounts.GetValueOrDefault(HitResult.SmallTickHit);

            return comboPortion * comboProgress
                   + dropletsPortion * dropletsHit
                   + bonusPortion;
        }

        public override int GetBaseScoreForResult(HitResult result)
        {
            switch (result)
            {
                // dirty hack to emulate accuracy on stable weighting every object equally in accuracy portion
                case HitResult.Great:
                case HitResult.LargeTickHit:
                case HitResult.SmallTickHit:
                    return 300;

                case HitResult.LargeBonus:
                    return 200;
            }

            return base.GetBaseScoreForResult(result);
        }

        protected override double GetComboScoreChange(JudgementResult result)
        {
            double baseIncrease = 0;

            switch (result.Type)
            {
                case HitResult.Great:
                    baseIncrease = 300;
                    break;

                case HitResult.LargeTickHit:
                    baseIncrease = 100;
                    break;
            }

            return baseIncrease * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(combo_cap, combo_base));
        }

        public override ScoreRank RankFromScore(double accuracy, IReadOnlyDictionary<HitResult, int> results)
        {
            if (accuracy == accuracy_cutoff_x)
                return ScoreRank.X;
            if (accuracy >= accuracy_cutoff_s)
                return ScoreRank.S;
            if (accuracy >= accuracy_cutoff_a)
                return ScoreRank.A;
            if (accuracy >= accuracy_cutoff_b)
                return ScoreRank.B;
            if (accuracy >= accuracy_cutoff_c)
                return ScoreRank.C;

            return ScoreRank.D;
        }

        public override double AccuracyCutoffFromRank(ScoreRank rank)
        {
            switch (rank)
            {
                case ScoreRank.X:
                case ScoreRank.XH:
                    return accuracy_cutoff_x;

                case ScoreRank.S:
                case ScoreRank.SH:
                    return accuracy_cutoff_s;

                case ScoreRank.A:
                    return accuracy_cutoff_a;

                case ScoreRank.B:
                    return accuracy_cutoff_b;

                case ScoreRank.C:
                    return accuracy_cutoff_c;

                case ScoreRank.D:
                    return accuracy_cutoff_d;

                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }
        }
    }
}
