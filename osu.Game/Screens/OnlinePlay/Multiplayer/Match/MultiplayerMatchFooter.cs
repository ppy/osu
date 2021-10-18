// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerMatchFooter : CompositeDrawable
    {
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

        private readonly MultiplayerReadyButton readyButton;
        private readonly MultiplayerSpectateButton spectateButton;

        public MultiplayerMatchFooter()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new GridContainer
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
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: ready_button_width),
                    new Dimension()
                }
            };
        }
    }
}
