// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonHoldNoteTailPiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private readonly Container container;

        public ArgonHoldNoteTailPiece()
        {
            // holds end at the middle of the tail,
            // so we do * 2 pull up the hold body to be the height of a note
            Height = ArgonNotePiece.NOTE_HEIGHT * 2;
            RelativeSizeAxes = Axes.X;

            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                container = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,

                    CornerRadius = ArgonNotePiece.CORNER_RADIUS,
                    Masking = true,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Black,
                            Alpha = 0.4f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 1 - ArgonNotePiece.NOTE_ACCENT_RATIO,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = Colour4.Black,
                            Alpha = 0.3f,
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            container.Anchor = container.Origin =
                direction.NewValue == ScrollingDirection.Down
                    ? Anchor.BottomCentre
                    : Anchor.TopCentre;
        }
    }
}
