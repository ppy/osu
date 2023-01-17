// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomFooter : CompositeDrawable
    {
        public Action OnStart;

        public PlaylistsRoomFooter()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                new PlaylistsReadyButton
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
