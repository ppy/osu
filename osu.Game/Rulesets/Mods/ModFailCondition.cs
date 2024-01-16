// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFailCondition : Mod, IApplicableToHealthProcessor, IApplicableFailOverride
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModCinema) };

        [SettingSource("Restart on fail", "Automatically restarts when failed.")]
        public BindableBool Restart { get; } = new BindableBool();

        public virtual bool PerformFail() => true;

        public virtual bool RestartOnFail => Restart.Value;

        private Action? triggerFailureDelegate;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            triggerFailureDelegate = healthProcessor.TriggerFailure;
            healthProcessor.FailConditions += FailCondition;
        }

        /// <summary>
        /// Immediately triggers a failure on the loaded <see cref="HealthProcessor"/>.
        /// </summary>
        protected void TriggerFailure() => triggerFailureDelegate?.Invoke();

        /// <summary>
        /// Determines whether <paramref name="result"/> should trigger a failure. Called every time a
        /// judgement is applied to <paramref name="healthProcessor"/>.
        /// </summary>
        /// <param name="healthProcessor">The loaded <see cref="HealthProcessor"/>.</param>
        /// <param name="result">The latest <see cref="Judgement"/>.</param>
        /// <returns>Whether the fail condition has been met.</returns>
        /// <remarks>
        /// This method should only be used to trigger failures based on <paramref name="result"/>.
        /// Using outside values to evaluate failure may introduce event ordering discrepancies, use
        /// an <see cref="IApplicableMod"/> with <see cref="TriggerFailure"/> instead.
        /// </remarks>
        protected abstract bool FailCondition(HealthProcessor healthProcessor, Judgement result);
    }
}
