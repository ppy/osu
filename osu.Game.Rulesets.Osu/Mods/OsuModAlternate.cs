// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : ModAlternate<OsuHitObject, OsuAction>
    {
        private const double flash_duration = 1000;
        private OsuAction? lastActionPressed;
        private DrawableRuleset<OsuHitObject> ruleset;

        public override void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        protected override void Reset()
        {
            lastActionPressed = null;
        }

        protected override bool OnPressed(OsuAction key)
        {
            if (lastActionPressed == key)
            {
                ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
                return true;
            }

            lastActionPressed = key;

            return false;
        }

        protected override void OnReleased(OsuAction key)
        {
        }
    }
}
