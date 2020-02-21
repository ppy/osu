// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEnforceAlternate : ModEnforceAlternate, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToHealthProcessor
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModRelax), typeof(OsuModAutopilot) }).ToArray();

        private OsuInputManager inputManager;
        private HealthProcessor healthProcessor;

        private OsuAction? blockedButton;

        private int blockedKeystrokes;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.BlockConditions += BlockCondition;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;

            if (CauseFail.Value)
                healthProcessor.FailConditions += FailCondition;
        }

        protected bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => blockedKeystrokes > 0;

        protected bool BlockCondition(UIEvent e, IEnumerable<KeyBinding> keyBindings)
        {
            if (!healthProcessor.IsBreakTime.Value)
            {
                var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                var combos = keyBindings?.ToList().FindAll(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any));

                if (combos != null)
                {
                    InputKey? key;
                    var kb = e as KeyDownEvent;
                    var mouse = e as MouseDownEvent;
                    if (kb != null)
                        key = KeyCombination.FromKey(kb.Key);
                    else if (mouse != null)
                        key = KeyCombination.FromMouseButton(mouse.Button);
                    else
                        return false;

                    var single = combos.Find(c => c.KeyCombination.Keys.Any(k => k == key))?.GetAction<OsuAction>();

                    if (single != null)
                    {
                        if (single == blockedButton)
                        {
                            if ((kb != null && !kb.Repeat) || mouse != null)
                                blockedKeystrokes++;

                            return true;
                        }
                        else
                            blockedButton = single;
                    }
                }
            }

            return false;
        }
    }
}
