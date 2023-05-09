// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public partial class CatchScoreProcessor : ScoreProcessor
    {
        private const int combo_cap = 200;
        private const double combo_base = 4;

        protected override double ClassicScoreMultiplier => 28;

        private double tinyDropletScale;

        private int maximumTinyDroplets;
        private int hitTinyDroplets;

        public CatchScoreProcessor(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override double ComputeTotalScore()
        {
            double fruitHitsRatio = maximumTinyDroplets == 0 ? 0 : (double)hitTinyDroplets / maximumTinyDroplets;

            const int tiny_droplets_portion = 400000;

            return (
                ((1000000 - tiny_droplets_portion) + tiny_droplets_portion * (1 - tinyDropletScale)) * ComboPortion / MaxComboPortion +
                tiny_droplets_portion * tinyDropletScale * fruitHitsRatio +
                BonusPortion
            ) * ScoreMultiplier;
        }

        protected override void AddScoreChange(JudgementResult result)
        {
            var change = computeScoreChange(result);
            ComboPortion += change.combo;
            BonusPortion += change.bonus;
            hitTinyDroplets += change.tinyDropletHits;
        }

        protected override void RemoveScoreChange(JudgementResult result)
        {
            var change = computeScoreChange(result);
            ComboPortion -= change.combo;
            BonusPortion -= change.bonus;
            hitTinyDroplets -= change.tinyDropletHits;
        }

        private (double combo, double bonus, int tinyDropletHits) computeScoreChange(JudgementResult result)
        {
            if (result.HitObject is TinyDroplet)
                return (0, 0, 1);

            if (result.Type.IsBonus())
                return (0, Judgement.ToNumericResult(result.Type), 0);

            return (Judgement.ToNumericResult(result.Type) * Math.Min(Math.Max(0.5, Math.Log(result.ComboAtJudgement, combo_base)), Math.Log(combo_cap, combo_base)), 0, 0);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            if (storeResults)
            {
                maximumTinyDroplets = hitTinyDroplets;

                if (maximumTinyDroplets + MaxBasicJudgements == 0)
                    tinyDropletScale = 0;
                else
                    tinyDropletScale = (double)maximumTinyDroplets / (maximumTinyDroplets + MaxBasicJudgements);
            }

            hitTinyDroplets = 0;
        }
    }
}
