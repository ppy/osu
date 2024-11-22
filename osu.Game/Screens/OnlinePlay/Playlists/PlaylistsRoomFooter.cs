// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomFooter : CompositeDrawable
    {
        public Action? OnStart;

        public PlaylistsRoomFooter(Room room)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                new PlaylistsReadyButton(room)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(600, 1),
                    Action = () => OnStart?.Invoke()
                }
            };
        }
    }
}
