// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEnforceAlternate : ModEnforceAlternate, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToHealthProcessor
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModRelax), typeof(OsuModAutopilot) }).ToArray();

        private OsuInputManager inputManager;
        private HealthProcessor healthProcessor;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
        }

        public void Update(Playfield playfield)
        {
            if (!healthProcessor.IsBreakTime.Value && inputManager.LastButton != null)
            {
                inputManager.BlockedButton = inputManager.LastButton;
            }
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;
            healthProcessor.FailConditions += FailCondition;
        }

        protected virtual bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => CauseFail.Value && inputManager.BlockedKeystrokes > 0;
    }
}
