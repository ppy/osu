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
    /// Shows a selected arbitrary score for a playlist item, with scores around included.
    /// </summary>
    public partial class PlaylistItemScoreResultsScreen : PlaylistItemResultsScreen
    {
        private readonly long scoreId;

        public PlaylistItemScoreResultsScreen(long roomId, PlaylistItem playlistItem, long scoreId)
            : base(null, roomId, playlistItem)
        {
            this.scoreId = scoreId;
        }

        protected override APIRequest<MultiplayerScore> CreateScoreRequest() => new ShowPlaylistScoreRequest(RoomId, PlaylistItem.ID, scoreId);

        protected override ScoreInfo[] PerformSuccessCallback(Action<IEnumerable<ScoreInfo>> callback, List<MultiplayerScore> scores, MultiplayerScores? pivot = null)
        {
            var scoreInfos = base.PerformSuccessCallback(callback, scores, pivot);

            Schedule(() => SelectedScore.Value ??= scoreInfos.SingleOrDefault(score => score.OnlineID == scoreId));

            return scoreInfos;
        }
    }
}
