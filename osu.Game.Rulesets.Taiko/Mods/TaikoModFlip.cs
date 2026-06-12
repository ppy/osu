// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModFlip : Mod, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Name => "Flip";
        public override string Acronym => "FP";
        public override LocalisableString Description => "The playfield is flipped horizontally!";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableRuleset.Scale = new Vector2(-1, 1);
            drawableRuleset.Anchor = Framework.Graphics.Anchor.TopRight;
        }
    }
}
