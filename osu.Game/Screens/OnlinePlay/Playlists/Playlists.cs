// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class Playlists : OnlinePlayScreen
    {
        protected override string ScreenTitle => "Playlists";

        protected override LoungeSubScreen CreateLounge() => new PlaylistsLoungeSubScreen();
    }
}
