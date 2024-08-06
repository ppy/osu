// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="HealthProcessor"/> is in a failed state.
        /// Return true if the fail was permitted.
        /// </summary>
        public event Func<bool>? Failed;

        /// <summary>
        /// The current selected mods.
        /// </summary>
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Whether this <see cref="HealthProcessor"/> has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        /// <summary>
        /// If this <see cref="HealthProcessor"/> is in a failed state due to a mod, this returns the instance of that mod.
        /// </summary>
        /// <remarks>
        /// Used in <see cref="Player"/> to determine whether to perform a restart on failure, if the triggering mod is configured as such.
        /// </remarks>
        public Mod? ModTriggeringFailure { get; private set; }

        /// <summary>
        /// Immediately triggers a failure for this HealthProcessor.
        /// </summary>
        /// <param name="triggeringMod">An optional mod that triggered failure.</param>
        public void TriggerFailure(Mod? triggeringMod = null)
        {
            if (HasFailed)
                return;

            if (Failed?.Invoke() != false)
                HasFailed = true;

            if (triggeringMod != null)
                ModTriggeringFailure = triggeringMod;
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            Health.Value += GetHealthIncreaseFor(result);

            if (meetsAnyFailCondition(result))
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
        /// Checks whether the default conditions for failing are met.
        /// </summary>
        /// <returns><see langword="true"/> if failure should be invoked.</returns>
        protected virtual bool CheckDefaultFailCondition(JudgementResult result) => Precision.AlmostBigger(Health.MinValue, Health.Value);

        /// <summary>
        /// Whether the current state of <see cref="HealthProcessor"/> or the provided <paramref name="result"/> meets any fail condition.
        /// </summary>
        /// <param name="result">The judgement result.</param>
        private bool meetsAnyFailCondition(JudgementResult result)
        {
            if (CheckDefaultFailCondition(result))
                return true;

            foreach (var condition in Mods.Value.OfType<IApplicableFailOverride>())
            {
                if (condition.CheckFail(result) == FailState.Force)
                {
                    ModTriggeringFailure = condition as Mod;
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
