// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEnforceAlternate : ModEnforceAlternate, IUpdatableByPlayfield, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToHealthProcessor
    {
        public override IconUsage? Icon => FontAwesome.Solid.SignLanguage;

        private TaikoInputManager inputManager;
        private HealthProcessor healthProcessor;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            inputManager = (TaikoInputManager)drawableRuleset.KeyBindingInputManager;
        }

        public void Update(Playfield playfield)
        {
            if (!healthProcessor.IsBreakTime.Value)
            {
                if (inputManager.LastRim != null)
                {
                    inputManager.BlockedRim = inputManager.LastRim;
                }

                if (inputManager.LastCentre != null)
                {
                    inputManager.BlockedCentre = inputManager.LastCentre;
                }
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
