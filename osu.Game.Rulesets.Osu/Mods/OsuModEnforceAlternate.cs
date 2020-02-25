// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEnforceAlternate : ModEnforceAlternate, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToHealthProcessor
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModRelax)).ToArray();

        private OsuInputManager inputManager;
        private HealthProcessor healthProcessor;

        private OsuAction? blockedButton;

        private int blockedKeystrokes;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.HandleBindings += handleBindings;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;

            if (CauseFail.Value)
                healthProcessor.FailConditions += FailCondition;
        }

        protected bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => blockedKeystrokes > 0;

        private bool handleBindings(OsuAction? single, List<OsuAction> pressedActions)
        {
            if (!healthProcessor.IsBreakTime.Value)
            {
                if (single != null && single == blockedButton)
                {
                    blockedKeystrokes++;

                    return true;
                }
                else
                    blockedButton = single;
            }

            return false;
        }
    }
}
