// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFloatingFruits : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Floating Fruits";
        public override string Acronym => "FF";
        public override LocalisableString Description => "The fruits are... floating?";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Cloud;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            // todo: usually mods are not supposed to invent new code inside the ruleset implementation,
            // but it's required in this case because we need to flip the position of the catch combo counter,
            // and the only way to achieve that is by having a signal that LegacyCatchComboCounter can use to act accordingly.
            // this will be gone once ruleset-specific skinnable containers are supported.
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            drawableCatchRuleset.Flipped = true;
        }
    }
}
