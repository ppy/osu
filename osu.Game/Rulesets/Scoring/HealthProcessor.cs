// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;

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

        public HealthProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            Health.Value += HealthAdjustmentFactorFor(result) * result.Judgement.HealthIncreaseFor(result);

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

            Health.Value = 1;
            HasFailed = false;
        }

        /// <summary>
        /// Checks if the score is in a failed state and notifies subscribers.
        /// <para>
        /// This can only ever notify subscribers once.
        /// </para>
        /// </summary>
        private void updateFailed(JudgementResult result)
        {
            if (HasFailed)
                return;

            if (!DefaultFailCondition && FailConditions?.Invoke(this, result) != true)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;
        }
    }
}
