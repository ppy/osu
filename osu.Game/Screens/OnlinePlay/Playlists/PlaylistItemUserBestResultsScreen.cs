// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// Shows a user's best score in a playlist item, with scores around included.
    /// </summary>
    public partial class PlaylistItemUserBestResultsScreen : PlaylistItemResultsScreen
    {
        private readonly int userId;

        public PlaylistItemUserBestResultsScreen(long roomId, PlaylistItem playlistItem, int userId)
            : base(null, roomId, playlistItem)
        {
            this.userId = userId;
        }

        protected override APIRequest<MultiplayerScore> CreateScoreRequest() => new ShowPlaylistUserScoreRequest(RoomId, PlaylistItem.ID, userId);

        protected override void OnScoresAdded(ScoreInfo[] scores)
        {
            base.OnScoresAdded(scores);

            // Prefer selecting the local user's score, or otherwise default to the first visible score.
            SelectedScore.Value ??= scores.FirstOrDefault(s => s.UserID == userId) ?? scores.FirstOrDefault();
        }
    }
}
