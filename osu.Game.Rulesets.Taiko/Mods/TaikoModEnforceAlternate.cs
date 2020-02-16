// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEnforceAlternate : ModEnforceAlternate, IUpdatableByPlayfield, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        private TaikoInputManager inputManager;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            inputManager = (TaikoInputManager)drawableRuleset.KeyBindingInputManager;
        }

        public void Update(Playfield playfield)
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
}
