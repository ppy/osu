// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimeResultsScreen : TimeshiftResultsScreen
    {
        public RealtimeResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem)
            : base(score, roomId, playlistItem, false, false)
        {
        }
    }
}
