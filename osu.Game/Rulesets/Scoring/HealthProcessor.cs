// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;

            if (result.FailedAtJudgement)
                return;

            Health.Value += GetHealthIncreaseFor(result);

            if (CheckDefaultFailCondition(result))
            {
                bool allowFail = true;

                for (int i = 0; i < Mods.Value.Count; i++)
                {
                    if (Mods.Value[i] is IBlockFail blockMod)
                    {
                        // Intentionally does not early return so that all mods have a chance to update internal states (e.g. ModEasyWithExtraLives).
                        allowFail &= blockMod.AllowFail();
                        break;
                    }
                }

                if (allowFail)
                    TriggerFailure(false);
            }
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            Health.Value = result.HealthAtJudgement;
        }

        /// <summary>
        /// Retrieves the health increase for a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/>.</param>
        /// <returns>The health increase.</returns>
        protected virtual double GetHealthIncreaseFor(JudgementResult result) => result.HealthIncrease;

        protected virtual bool CheckDefaultFailCondition(JudgementResult result) => Precision.AlmostBigger(Health.MinValue, Health.Value);

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 1;
        }
    }
}
