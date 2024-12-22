// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class TrackTick : GridContainer
    {
        public TrackTick()
        {
            RelativeSizeAxes = Axes.Y;
            RowDimensions = [
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Distributed),
            ];
            ColumnDimensions = [
                new Dimension(GridSizeMode.Distributed),
            ];
            Content = new[]
            {
                new[]
                {
                    createTick(),
                },
                new[]
                {
                    createTick(),
                },
                new[]
                {
                    createTick(),
                },
            };
        }

        private Drawable createTick()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = OsuIcon.FilledCircle,
                    Enabled = { Value = true },
                }
            };
        }
    }
}
