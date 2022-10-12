// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal class ArgonHoldNoteTailPiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        private readonly Box colouredBox;
        private readonly Box shadow;

        public ArgonHoldNoteTailPiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = ArgonNotePiece.NOTE_HEIGHT;

            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                shadow = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.82f,
                    Masking = true,
                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Children = new Drawable[]
                    {
                        colouredBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
                new Circle
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ArgonNotePiece.CORNER_RADIUS * 2,
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(IScrollingInfo scrollingInfo, DrawableHitObject? drawableObject)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            if (drawableObject != null)
            {
                accentColour.BindTo(drawableObject.AccentColour);
                accentColour.BindValueChanged(onAccentChanged, true);
            }
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            colouredBox.Anchor = colouredBox.Origin = direction.NewValue == ScrollingDirection.Up
                ? Anchor.TopCentre
                : Anchor.BottomCentre;
        }

        private void onAccentChanged(ValueChangedEvent<Color4> accent)
        {
            colouredBox.Colour = ColourInfo.GradientVertical(
                accent.NewValue,
                accent.NewValue.Darken(0.1f)
            );

            shadow.Colour = accent.NewValue.Darken(0.5f);
        }
    }
}
