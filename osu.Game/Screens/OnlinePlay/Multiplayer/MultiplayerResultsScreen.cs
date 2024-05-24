// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerResultsScreen : PlaylistsResultsScreen
    {
        public MultiplayerResultsScreen(ScoreInfo score, long roomId, PlaylistItem playlistItem)
            : base(score, roomId, playlistItem)
        {
        }
    }
}
