// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    public class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="ScoreProcessor"/> is in a failed state.
        /// Return true if the fail was permitted.
        /// </summary>
        public event Func<bool> Failed;

        /// <summary>
        /// Additional conditions on top of <see cref="DefaultFailCondition"/> that cause a failing state.
        /// </summary>
        public event Func<HealthProcessor, JudgementResult, bool> FailConditions;

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        private readonly List<(double time, double health)> healthIncreases = new List<(double time, double health)>();
        private double drainRate = 1;

        public override void ApplyElapsedTime(double elapsedTime) => Health.Value -= drainRate * elapsedTime;

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            double healthIncrease = HealthAdjustmentFactorFor(result) * result.Judgement.HealthIncreaseFor(result);
            healthIncreases.Add((result.HitObject.GetEndTime() + result.TimeOffset, healthIncrease));

            if (HasFailed)
                return;

            Health.Value += healthIncrease;

            if (!DefaultFailCondition && FailConditions?.Invoke(this, result) != true)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            Health.Value = result.HealthAtJudgement;

            // Todo: Revert HasFailed state with proper player support
        }

        /// <summary>
        /// An adjustment factor which is multiplied into the health increase provided by a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> for which the adjustment should apply.</param>
        /// <returns>The adjustment factor.</returns>
        protected virtual double HealthAdjustmentFactorFor(JudgementResult result) => 1;

        /// <summary>
        /// The default conditions for failing.
        /// </summary>
        protected virtual bool DefaultFailCondition => Precision.AlmostBigger(Health.MinValue, Health.Value);

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            drainRate = 1;

            if (storeResults)
            {
                const double percentage_target = 0.5;

                int count = 1;

                while (true)
                {
                    double currentHealth = 1;
                    double lowestHealth = 1;

                    for (int i = 0; i < healthIncreases.Count; i++)
                    {
                        var lastTime = i > 0 ? healthIncreases[i - 1].time : 0;

                        currentHealth -= (healthIncreases[i].time - lastTime) * drainRate;
                        lowestHealth = Math.Min(lowestHealth, currentHealth);
                        currentHealth = Math.Min(1, currentHealth + healthIncreases[i].health);

                        // Common scenario for when the drain rate is definitely too harsh
                        if (lowestHealth < 0)
                            break;
                    }

                    if (Math.Abs(lowestHealth - percentage_target) <= 0.01)
                        break;

                    count *= 2;
                    drainRate += 1.0 / count * Math.Sign(lowestHealth - percentage_target);
                }
            }

            healthIncreases.Clear();

            Health.Value = 1;
            HasFailed = false;
        }
    }
}
