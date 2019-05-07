// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Perfect:
                    return 300;
            }
        }

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return -0.02;

                case HitResult.Perfect:
                    return 0.01;
            }
        }

        /// <summary>
        /// Whether fruit on the platter should explode or drop.
        /// Note that this is only checked if the owning object is also <see cref="IHasComboInformation.LastInCombo" />
        /// </summary>
        public virtual bool ShouldExplodeFor(JudgementResult result) => result.IsHit;
    }
}
