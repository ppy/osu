// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class MainCirclePiece : CompositeDrawable
    {
        private readonly CirclePiece circle;
        private readonly RingPiece ring;
        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly NumberPiece number;
        private readonly GlowPiece glow;

        public MainCirclePiece(int index)
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                glow = new GlowPiece(),
                circle = new CirclePiece(),
                number = new NumberPiece
                {
                    Text = (index + 1).ToString(),
                },
                ring = new RingPiece(),
                flash = new FlashPiece(),
                explode = new ExplodePiece(),
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();

        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                glow.Colour = colour.NewValue;
                circle.Colour = colour.NewValue;
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            glow.FadeOut(400);

            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    explode.FadeIn(flash_in);
                    this.ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        ring.FadeOut();
                        circle.FadeOut();
                        number.FadeOut();

                        this.FadeOut(800);
                    }

                    break;
            }
        }
    }
}
