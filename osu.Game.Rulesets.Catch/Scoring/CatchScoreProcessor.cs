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

        private double tinyDropletScale;

        private int maximumTinyDroplets;
        private int hitTinyDroplets;
        private int maximumBasicJudgements;
        private int currentBasicJudgements;

        public CatchScoreProcessor()
            : base(new CatchRuleset())
        {
        }

        protected override double ComputeTotalScore(double comboRatio, double accuracyRatio, double bonusPortion)
        {
            double fruitHitsRatio = maximumTinyDroplets == 0 ? 0 : (double)hitTinyDroplets / maximumTinyDroplets;

            const int tiny_droplets_portion = 400000;

            return ((1000000 - tiny_droplets_portion) + tiny_droplets_portion * (1 - tinyDropletScale)) * comboRatio
                   + tiny_droplets_portion * tinyDropletScale * fruitHitsRatio
                   + bonusPortion;
        }

        protected override double GetComboScoreChange(JudgementResult result)
            => Judgement.ToNumericResult(result.Type) * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(combo_cap, combo_base));

        protected override void ApplyScoreChange(JudgementResult result)
        {
            base.ApplyScoreChange(result);

            if (result.HitObject is TinyDroplet)
                hitTinyDroplets++;

            if (result.Type.IsBasic())
                currentBasicJudgements++;
        }

        protected override void RemoveScoreChange(JudgementResult result)
        {
            base.RemoveScoreChange(result);

            if (result.HitObject is TinyDroplet)
                hitTinyDroplets--;

            if (result.Type.IsBasic())
                currentBasicJudgements--;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            if (storeResults)
            {
                maximumTinyDroplets = hitTinyDroplets;
                maximumBasicJudgements = currentBasicJudgements;

                if (maximumTinyDroplets + maximumBasicJudgements == 0)
                    tinyDropletScale = 0;
                else
                    tinyDropletScale = (double)maximumTinyDroplets / (maximumTinyDroplets + maximumBasicJudgements);
            }

            hitTinyDroplets = 0;
            currentBasicJudgements = 0;
        }
    }
}
