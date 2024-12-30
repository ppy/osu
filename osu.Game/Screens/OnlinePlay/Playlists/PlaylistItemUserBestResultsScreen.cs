// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        protected override ScoreInfo[] PerformSuccessCallback(Action<IEnumerable<ScoreInfo>> callback, List<MultiplayerScore> scores, MultiplayerScores? pivot = null)
        {
            var scoreInfos = base.PerformSuccessCallback(callback, scores, pivot);

            Schedule(() =>
            {
                // Prefer selecting the local user's score, or otherwise default to the first visible score.
                SelectedScore.Value ??= scoreInfos.FirstOrDefault(s => s.UserID == userId) ?? scoreInfos.FirstOrDefault();
            });

            return scoreInfos;
        }
    }
}
