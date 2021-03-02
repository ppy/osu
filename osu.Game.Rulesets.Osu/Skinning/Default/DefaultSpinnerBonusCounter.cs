// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultSpinnerBonusCounter : CompositeDrawable
    {
        private OsuSpriteText bonusCounter;

        private DrawableSpinner drawableSpinner;

        private IBindable<double> gainedBonus;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            InternalChild = bonusCounter = new OsuSpriteText
            {
                Alpha = 0,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 24),
                Y = -120,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gainedBonus = drawableSpinner.GainedBonus.GetBoundCopy();
            gainedBonus.BindValueChanged(bonus =>
            {
                bonusCounter.Text = $"{bonus.NewValue}";
                bonusCounter.FadeOutFromOne(1500);
                bonusCounter.ScaleTo(1.5f).Then().ScaleTo(1f, 1000, Easing.OutQuint);
            });
        }
    }
}
