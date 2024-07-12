// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFailCondition : Mod, IApplicableToHealthProcessor, IApplicableFailOverride, IHasFailCondition
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModCinema) };

        [SettingSource("Restart on fail", "Automatically restarts when failed.")]
        public BindableBool Restart { get; } = new BindableBool();

        public virtual bool PerformFail() => true;

        public virtual bool RestartOnFail => Restart.Value;

        private Action<Mod>? triggerFailureDelegate;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            triggerFailureDelegate = healthProcessor.TriggerFailure;
        }

        /// <summary>
        /// Immediately triggers a failure on the loaded <see cref="HealthProcessor"/>.
        /// </summary>
        protected void TriggerFailure() => triggerFailureDelegate?.Invoke(this);

        public abstract bool FailCondition(JudgementResult result);
    }
}
