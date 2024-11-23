// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengePlayer : PlaylistsPlayer
    {
        public DailyChallengePlayer(Room room, PlaylistItem playlistItem, PlayerConfiguration? configuration = null)
            : base(room, playlistItem, configuration)
        {
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(Room.RoomID != null);

            if (score.OnlineID >= 0)
            {
                return new PlaylistItemScoreResultsScreen(Room.RoomID.Value, PlaylistItem, score.OnlineID)
                {
                    AllowRetry = true,
                    ShowUserStatistics = true,
                };
            }

            // If the score has failed submission, fall back to displaying scores from user's highest.
            return new PlaylistItemUserResultsScreen(score, Room.RoomID.Value, PlaylistItem)
            {
                AllowRetry = true,
                ShowUserStatistics = true,
            };
        }
    }
}
