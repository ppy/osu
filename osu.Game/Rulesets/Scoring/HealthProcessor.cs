// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="ScoreProcessor"/> is in a failed state.
        /// Return true if the fail was permitted.
        /// </summary>
        public event Func<bool>? Failed;

        /// <summary>
        /// Additional conditions on top of <see cref="DefaultFailCondition"/> that cause a failing state.
        /// </summary>
        public event Func<HealthProcessor, JudgementResult, bool>? FailConditions;

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Whether this ScoreProcessor has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        /// <summary>
        /// Immediately triggers a failure for this HealthProcessor.
        /// </summary>
        public void TriggerFailure()
        {
            if (Failed?.Invoke() != false)
                HasFailed = true;
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            Health.Value += GetHealthIncreaseFor(result);

            if (CanFailOn(result) && meetsAnyFailCondition(result))
                TriggerFailure();
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            Health.Value = result.HealthAtJudgement;

            // Todo: Revert HasFailed state with proper player support
        }

        /// <summary>
        /// Retrieves the health increase for a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/>.</param>
        /// <returns>The health increase.</returns>
        protected virtual double GetHealthIncreaseFor(JudgementResult result) => result.HealthIncrease;

        /// <summary>
        /// Whether a failure can occur on a given <paramref name="result"/>.
        /// If the return value of this method is <see langword="false"/>, neither <see cref="DefaultFailCondition"/> nor <see cref="FailConditions"/> will be checked
        /// after this <paramref name="result"/>.
        /// </summary>
        protected virtual bool CanFailOn(JudgementResult result) => true;

        /// <summary>
        /// The default conditions for failing.
        /// </summary>
        protected virtual bool DefaultFailCondition => Precision.AlmostBigger(Health.MinValue, Health.Value);

        /// <summary>
        /// Whether the current state of <see cref="HealthProcessor"/> or the provided <paramref name="result"/> meets any fail condition.
        /// </summary>
        /// <param name="result">The judgement result.</param>
        private bool meetsAnyFailCondition(JudgementResult result)
        {
            if (DefaultFailCondition)
                return true;

            if (FailConditions != null)
            {
                foreach (var condition in FailConditions.GetInvocationList())
                {
                    bool conditionResult = (bool)condition.Method.Invoke(condition.Target, new object[] { this, result })!;
                    if (conditionResult)
                        return true;
                }
            }

            return false;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 1;
            HasFailed = false;
        }
    }
}
