// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerMatchFooter : CompositeDrawable
    {
        private const float ready_button_width = 600;
        private const float spectate_button_width = 200;

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
                        new MultiplayerSpectateButton
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        null,
                        new MatchStartControl
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
