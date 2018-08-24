// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        protected virtual float HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Perfect:
                    return 10.2f;
            }
        }

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        public float HealthIncreaseFor(JudgementResult result) => HealthIncreaseFor(result.Type);

        /// <summary>
        /// Whether fruit on the platter should explode or drop.
        /// Note that this is only checked if the owning object is also <see cref="IHasComboInformation.LastInCombo" />
        /// </summary>
        public virtual bool ShouldExplodeFor(JudgementResult result) => result.IsHit;
    }
}
