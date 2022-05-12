// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChatOverlayDrawableChannel : DrawableChannel
    {
        public ChatOverlayDrawableChannel(Channel channel)
            : base(channel)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ChatLineFlow.Padding = new MarginPadding(0);
        }

        protected override Drawable CreateDaySeparator(DateTimeOffset time) => new ChatOverlayDaySeparator(time);

        private class ChatOverlayDaySeparator : Container
        {
            private readonly DateTimeOffset time;

            public ChatOverlayDaySeparator(DateTimeOffset time)
            {
                this.time = time;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 15, Vertical = 20 };
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 200),
                        new Dimension(GridSizeMode.Absolute, 15),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 15),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        new Circle
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Colour = colourProvider.Background5,
                                            RelativeSizeAxes = Axes.X,
                                            Height = 2,
                                        },
                                        Drawable.Empty(),
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Text = time.ToLocalTime().ToString("dd MMMM yyyy").ToUpper(),
                                            Font = OsuFont.Torus.With(size: 15, weight: FontWeight.SemiBold),
                                            Colour = colourProvider.Content1,
                                        },
                                    },
                                },
                            },
                            Drawable.Empty(),
                            new Circle
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = colourProvider.Background5,
                                RelativeSizeAxes = Axes.X,
                                Height = 2,
                            },
                        },
                    },
                };
            }
        }
    }
}
