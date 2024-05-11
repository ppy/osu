// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class CreatePlaylistsRoomButton : CreateRoomButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Create playlist";
        }
    }
}
