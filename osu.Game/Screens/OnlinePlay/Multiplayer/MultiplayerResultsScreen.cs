// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Playlists;

namespace osu.Game.Screens.Multi.Multiplayer
{
    public class MultiplayerResultsScreen : PlaylistsResultsScreen
    {
        public MultiplayerResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem)
            : base(score, roomId, playlistItem, false, false)
        {
        }
    }
}
