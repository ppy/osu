// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEnforceAlternate : ModEnforceAlternate, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToHealthProcessor, IUpdatableByPlayfield
    {
        public override IconUsage? Icon => FontAwesome.Solid.SignLanguage;

        private TaikoInputManager inputManager;
        private HealthProcessor healthProcessor;

        private TaikoAction? blockedRim;
        private TaikoAction? blockedCentre;

        private TaikoAction? lastBlocked;
        private int blockedKeystrokes;

        private DrawableDrumRoll currentRoll;

        [SettingSource("Allow repeats on color change")]
        public Bindable<bool> RepeatOnColorChange { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Allow repeats after drumrolls")]
        public Bindable<bool> RepeatAfterDrumroll { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            inputManager = (TaikoInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.HandleBindings += handleBindings;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;

            if (CauseFail.Value)
                healthProcessor.FailConditions += FailCondition;
        }

        protected bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => blockedKeystrokes > 0 && lastBlocked != null;

        public void Update(Playfield playfield)
        {
            if (RepeatAfterDrumroll.Value)
            {
                if (currentRoll != null && currentRoll.AllJudged)
                {
                    if (currentRoll.HitObject.IsStrong)
                    {
                        blockedRim = null;
                        blockedCentre = null;
                    }
                    else
                        blockedCentre = null;

                    currentRoll = null;
                }

                currentRoll = playfield.HitObjectContainer.Objects.ToList().Find(o => o is DrawableDrumRoll && !o.AllJudged) as DrawableDrumRoll;
            }
        }

        private bool handleMultipleKeypresses(List<TaikoAction> rims, List<TaikoAction> centres)
        {
            bool bothLeft = (rims.Count == 1 && rims.First() == TaikoAction.LeftRim) && (centres.Count == 1 && centres.First() == TaikoAction.LeftCentre);
            bool bothRight = (rims.Count == 1 && rims.First() == TaikoAction.RightRim) && (centres.Count == 1 && centres.First() == TaikoAction.RightCentre);

            if (bothLeft || bothRight)
            {
                bool rimBlocked = rims.Any(a => a == blockedRim);
                bool centreBlocked = centres.Any(a => a == blockedCentre);

                if (rimBlocked && centreBlocked)
                {
                    blockedKeystrokes++;

                    return true;
                }
                else
                {
                    blockedRim = rims.First();
                    blockedCentre = centres.First();
                }
            }
            else
            {
                if (rims.Count == 2)
                    blockedRim = null;

                if (centres.Count == 2)
                    blockedCentre = null;
            }

            var blocked = rims.Concat(centres).ToList().FindAll(a => a == lastBlocked);

            lastBlocked = null;

            if (blocked.Any())
                inputManager.RetractLastBlocked();

            return false;
        }

        private bool handleBindings(TaikoAction? single, List<TaikoAction> pressedActions)
        {
            if (!healthProcessor.IsBreakTime.Value)
            {
                var rims = pressedActions.FindAll(a => a == TaikoAction.LeftRim || a == TaikoAction.RightRim);
                var centres = pressedActions.FindAll(a => a == TaikoAction.LeftCentre || a == TaikoAction.RightCentre);

                if (pressedActions.Count > 1)
                    return handleMultipleKeypresses(rims, centres);

                if (RepeatOnColorChange.Value)
                {
                    if (!rims.Any())
                        blockedRim = null;

                    if (!centres.Any())
                        blockedCentre = null;
                }

                if (pressedActions.Any(a => a == blockedRim || a == blockedCentre))
                {
                    blockedKeystrokes++;
                    lastBlocked = pressedActions.First(a => a == blockedRim || a == blockedCentre);

                    return true;
                }

                if (rims.Any())
                    blockedRim = rims.First();

                if (centres.Any())
                    blockedCentre = centres.First();
            }

            return false;
        }
    }
}
