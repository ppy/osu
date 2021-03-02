// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using static osu.Game.Rulesets.Osu.Skinning.Legacy.LegacySpinner;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySpinnerBonusCounter : CompositeDrawable
    {
        private LegacySpriteText bonusCounter;

        private DrawableSpinner drawableSpinner;

        private IBindable<double> gainedBonus;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject, ISkinSource source)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            InternalChild = new LegacyCoordinatesContainer
            {
                Child = bonusCounter = ((LegacySpriteText)source.GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ScoreText))).With(s =>
                {
                    s.Alpha = 0f;
                    s.Anchor = Anchor.TopCentre;
                    s.Origin = Anchor.Centre;
                    s.Font = s.Font.With(fixedWidth: false);
                    s.Scale = new Vector2(SPRITE_SCALE);
                    s.Y = LegacyCoordinatesContainer.SPINNER_TOP_OFFSET + 299;
                }),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gainedBonus = drawableSpinner.GainedBonus.GetBoundCopy();
            gainedBonus.BindValueChanged(bonus =>
            {
                bonusCounter.Text = $"{bonus.NewValue}";
                bonusCounter.FadeOutFromOne(800, Easing.Out);
                bonusCounter.ScaleTo(SPRITE_SCALE * 2f).Then().ScaleTo(SPRITE_SCALE * 1.28f, 800, Easing.Out);
            });
        }
    }
}
