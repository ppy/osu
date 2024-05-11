// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class Header : Container
    {
        public const float HEIGHT = 80;

        private readonly ScreenStack stack;
        private readonly MultiHeaderTitle title;

        public Header(string mainTitle, ScreenStack stack)
        {
            this.stack = stack;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Padding = new MarginPadding { Left = WaveOverlayContainer.WIDTH_PADDING };

            Child = title = new MultiHeaderTitle(mainTitle)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            // unnecessary to unbind these as this header has the same lifetime as the screen stack we are attaching to.
            stack.ScreenPushed += (_, _) => updateSubScreenTitle();
            stack.ScreenExited += (_, _) => updateSubScreenTitle();
        }

        private void updateSubScreenTitle() => title.Screen = stack.CurrentScreen as IOnlinePlaySubScreen;

        private partial class MultiHeaderTitle : CompositeDrawable
        {
            private const float spacing = 6;

            private readonly OsuSpriteText dot;
            private readonly OsuSpriteText pageTitle;

            [CanBeNull]
            public IOnlinePlaySubScreen Screen
            {
                set => pageTitle.Text = value?.ShortTitle.Titleize() ?? string.Empty;
            }

            public MultiHeaderTitle(string mainTitle)
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(spacing, 0),
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.TorusAlternate.With(size: 24),
                                Text = mainTitle
                            },
                            dot = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.TorusAlternate.With(size: 24),
                                Text = "·"
                            },
                            pageTitle = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.TorusAlternate.With(size: 24),
                                Text = "Lounge"
                            }
                        }
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                pageTitle.Colour = dot.Colour = colours.Yellow;
            }
        }
    }
}
