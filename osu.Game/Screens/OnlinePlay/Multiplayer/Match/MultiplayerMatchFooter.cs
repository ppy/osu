// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerMatchFooter : CompositeDrawable
    {
        public const float HEIGHT = 50;
        private const float ready_button_width = 600;
        private const float spectate_button_width = 200;

        public Action OnReadyClick
        {
            set => readyButton.OnReadyClick = value;
        }

        public Action OnSpectateClick
        {
            set => spectateButton.OnSpectateClick = value;
        }

        private readonly Drawable background;
        private readonly MultiplayerReadyButton readyButton;
        private readonly MultiplayerSpectateButton spectateButton;

        public MultiplayerMatchFooter()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            null,
                            spectateButton = new MultiplayerSpectateButton
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            null,
                            readyButton = new MultiplayerReadyButton
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            null
                        }
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(maxSize: spectate_button_width),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(maxSize: ready_button_width),
                        new Dimension()
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = Color4Extensions.FromHex(@"28242d");
        }
    }
}
