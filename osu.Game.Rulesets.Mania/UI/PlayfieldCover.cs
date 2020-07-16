// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class PlayfieldCover : CompositeDrawable
    {
        private readonly Box gradient;
        private readonly Box filled;
        private readonly IBindable<ScrollingDirection> scrollDirection = new Bindable<ScrollingDirection>();

        public PlayfieldCover()
        {
            Blending = new BlendingParameters
            {
                RGBEquation = BlendingEquation.Add,
                Source = BlendingType.Zero,
                Destination = BlendingType.One,
                AlphaEquation = BlendingEquation.Add,
                SourceAlpha = BlendingType.Zero,
                DestinationAlpha = BlendingType.OneMinusSrcAlpha
            };

            InternalChildren = new Drawable[]
            {
                gradient = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Height = 0.25f,
                    Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(0f),
                        Color4.White.Opacity(1f)
                    )
                },
                filled = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            scrollDirection.BindTo(scrollingInfo.Direction);
            scrollDirection.BindValueChanged(onScrollDirectionChanged, true);
        }

        private void onScrollDirectionChanged(ValueChangedEvent<ScrollingDirection> valueChangedEvent)
        {
            bool isUpscroll = valueChangedEvent.NewValue == ScrollingDirection.Up;
            Rotation = isUpscroll ? 180f : 0f;
        }

        public float Coverage
        {
            set
            {
                filled.Height = value;
                gradient.Y = 1 - filled.Height - gradient.Height;
            }
        }
    }
}
