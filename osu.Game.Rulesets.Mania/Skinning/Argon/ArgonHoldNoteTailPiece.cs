// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonHoldNoteTailPiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        private readonly Container spriteContainer;
        private readonly Container shadeContainer;
        private readonly Circle hitLine;

        public ArgonHoldNoteTailPiece()
        {
            RelativeSizeAxes = Axes.X;

            // multiply by two so that the hold body extends up to the height of the note head accent
            Height = ArgonNotePiece.NOTE_HEIGHT * ArgonNotePiece.NOTE_ACCENT_RATIO * 2;

            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                shadeContainer = new Container {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[] {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.2f,
                        },
                    },
                },
                spriteContainer = new Container {
                    RelativeSizeAxes = Axes.X,
                    Height = ArgonNotePiece.NOTE_HEIGHT,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Y = 4,
                            Icon = FontAwesome.Solid.AngleDown,
                            Size = new Vector2(20),
                            Scale = new Vector2(1, 0.7f),
                            Colour = Color4.White,
                            Alpha = 0.2f,
                        },
                    },
                },
                hitLine = new Circle {
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
            hitLine.Anchor = hitLine.Origin =
            spriteContainer.Anchor = spriteContainer.Origin =
            shadeContainer.Anchor = shadeContainer.Origin =
                direction.NewValue == ScrollingDirection.Up
                    ? Anchor.TopCentre
                    : Anchor.BottomCentre;
        }

        private void onAccentChanged(ValueChangedEvent<Color4> accent)
        {
            hitLine.Colour = accent.NewValue;
        }
    }
}
