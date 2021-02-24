// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFailCondition : Mod, IApplicableToHealthProcessor, IApplicableFailOverride
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModRelax), typeof(ModAutoplay) };

        public virtual bool PerformFail() => true;

        public virtual bool RestartOnFail => true;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.FailConditions += FailCondition;
        }

        protected abstract bool FailCondition(HealthProcessor healthProcessor, JudgementResult result);
    }
}
