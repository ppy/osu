// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFloatingFruits : Mod, IApplicableToDrawableRuleset<CatchHitObject>, ICanBeToggledDuringReplay
    {
        public override string Name => "Floating Fruits";
        public override string Acronym => "FF";
        public override LocalisableString Description => "The fruits are... floating?";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Cloud;

        public BindableBool IsDisabled { get; } = new BindableBool();

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            IsDisabled.BindValueChanged(s =>
            {
                drawableRuleset.PlayfieldAdjustmentContainer.Scale = s.NewValue ? new Vector2(1, 1) : new Vector2(1, -1);
                drawableRuleset.PlayfieldAdjustmentContainer.Y = 1 - drawableRuleset.PlayfieldAdjustmentContainer.Y;
            }, true);
        }
    }
}
