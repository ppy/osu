// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay
{
    public class DrawableRoomPlaylistWithResults : DrawableRoomPlaylist
    {
        public Action<PlaylistItem> RequestShowResults;

        public DrawableRoomPlaylistWithResults()
            : base(false, true)
        {
        }

        protected override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) =>
            new DrawableRoomPlaylistItemWithResults(item, false, true)
            {
                RequestShowResults = () => RequestShowResults(item),
                SelectedItem = { BindTarget = SelectedItem },
            };

        private class DrawableRoomPlaylistItemWithResults : DrawableRoomPlaylistItem
        {
            public Action RequestShowResults;

            public DrawableRoomPlaylistItemWithResults(PlaylistItem item, bool allowEdit, bool allowSelection)
                : base(item, allowEdit, allowSelection)
            {
            }

            protected override IEnumerable<Drawable> CreateButtons() =>
                base.CreateButtons().Prepend(new FilledIconButton
                {
                    Icon = FontAwesome.Solid.ChartPie,
                    Action = () => RequestShowResults?.Invoke(),
                    TooltipText = "View results"
                });

            private class FilledIconButton : IconButton
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Add(new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                        Colour = colours.Gray4,
                    });
                }
            }
        }
    }
}
