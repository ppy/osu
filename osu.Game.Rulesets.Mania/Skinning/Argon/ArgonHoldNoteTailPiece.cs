// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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

        private readonly Box shadeBackground;
        private readonly Box shadeForeground;

        public ArgonHoldNoteTailPiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = ArgonNotePiece.NOTE_HEIGHT;

            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                shadeBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = ArgonNotePiece.NOTE_ACCENT_RATIO,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        shadeForeground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
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
            Scale = new Vector2(1, direction.NewValue == ScrollingDirection.Up ? -1 : 1);
        }

        private void onAccentChanged(ValueChangedEvent<Color4> accent)
        {
            shadeBackground.Colour = accent.NewValue.Darken(1.7f);
            shadeForeground.Colour = accent.NewValue.Darken(1.1f);
        }
    }
}
