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
        /// The base health increase for the result achieved.
        /// </summary>
        public float HealthIncrease => HealthIncreaseFor(Result);

        /// <summary>
        /// Whether fruit on the platter should explode or drop.
        /// Note that this is only checked if the owning object is also <see cref="IHasComboInformation.LastInCombo" />
        /// </summary>
        public virtual bool ShouldExplode => IsHit;

        /// <summary>
        /// Convert a <see cref="HitResult"/> to a base health increase.
        /// </summary>
        /// <param name="result">The value to convert.</param>
        /// <returns>The base health increase.</returns>
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
    }
}
