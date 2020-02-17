// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEnforceAlternate : ModEnforceAlternate, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToHealthProcessor
    {
        public override IconUsage? Icon => FontAwesome.Solid.SignLanguage;

        private TaikoInputManager inputManager;
        private HealthProcessor healthProcessor;

        private TaikoAction? blockedRim;
        private TaikoAction? blockedCentre;
        private TaikoAction? lastRim;
        private TaikoAction? lastCentre;

        private TaikoAction? lastBlocked;
        private int blockedKeystrokes;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            inputManager = (TaikoInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.BlockConditions += BlockCondition;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;
            if (CauseFail.Value)
                healthProcessor.FailConditions += FailCondition;
        }

        protected virtual bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => blockedKeystrokes > 0;

        protected virtual bool BlockCondition(UIEvent e, IEnumerable<KeyBinding> keyBindings)
        {
            if (e is KeyDownEvent ev && !healthProcessor.IsBreakTime.Value)
            {
                var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                var combos = keyBindings.ToList().FindAll(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any));

                var rims = combos.FindAll(c => c.GetAction<TaikoAction>() == TaikoAction.LeftRim || c.GetAction<TaikoAction>() == TaikoAction.RightRim);
                var centres = combos.FindAll(c => c.GetAction<TaikoAction>() == TaikoAction.LeftCentre || c.GetAction<TaikoAction>() == TaikoAction.RightCentre);

                bool bothLeft = (rims.Count == 1 && rims.First().GetAction<TaikoAction>() == TaikoAction.LeftRim) && (centres.Count == 1 && centres.First().GetAction<TaikoAction>() == TaikoAction.LeftCentre);
                bool bothRight = (rims.Count == 1 && rims.First().GetAction<TaikoAction>() == TaikoAction.RightRim) && (centres.Count == 1 && centres.First().GetAction<TaikoAction>() == TaikoAction.RightCentre);

                if (rims.Count == 1)
                {
                    blockedRim = lastRim;
                    lastRim = rims.First().GetAction<TaikoAction>();
                }

                if (centres.Count == 1)
                {
                    blockedCentre = lastCentre;
                    lastCentre = centres.First().GetAction<TaikoAction>();
                }

                if (combos.Count > 1)
                {
                    var blocked = combos.Find(c => c.GetAction<TaikoAction>() == lastBlocked);

                    if (bothLeft || bothRight)
                    {
                        bool rimBlocked = bothLeft ? blockedRim == TaikoAction.LeftRim : blockedRim == TaikoAction.RightRim;
                        bool centreBlocked = bothLeft ? blockedCentre == TaikoAction.LeftCentre : blockedCentre == TaikoAction.RightCentre;

                        if (rimBlocked && centreBlocked)
                        {
                            lastBlocked = null;
                            return true;
                        }
                    }
                    else
                    {
                        lastRim = null;
                        lastCentre = null;
                    }

                    lastBlocked = null;

                    if (blocked != null)
                    {
                        Key key = ev.PressedKeys.ToList().Find(k => KeyCombination.FromKey(k) == blocked.KeyCombination.Keys.First());
                        inputManager.PressKey(key, true);
                    }

                    return false;
                }

                var single = combos.Find(c => c.KeyCombination.Keys.Any(k => k == KeyCombination.FromKey(ev.Key)))?.GetAction<TaikoAction>();

                if (single != null && (single == blockedRim || single == blockedCentre))
                {
                    if (!ev.Repeat)
                        blockedKeystrokes++;

                    lastBlocked = single;

                    return true;
                }
            }

            return false;
        }
    }
}
