// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// Shows a given score in a playlist item, with scores around included.
    /// </summary>
    public partial class PlaylistItemScoreResultsScreen : PlaylistItemResultsScreen
    {
        private readonly long scoreId;

        public PlaylistItemScoreResultsScreen(ScoreInfo score, long roomId, PlaylistItem playlistItem)
            : base(score, roomId, playlistItem)
        {
            scoreId = score.OnlineID;
        }

        public PlaylistItemScoreResultsScreen(long scoreId, long roomId, PlaylistItem playlistItem)
            : base(null, roomId, playlistItem)
        {
            this.scoreId = scoreId;
        }

        protected override Task<ScoreInfo[]> FetchScores()
        {
            // Don't attempt to index scores if the given score has an invalid online ID.
            // This can happen if the score failed to submit but is otherwise in a presentable state.
            return scoreId <= 0 ? Task.FromResult<ScoreInfo[]>([]) : base.FetchScores();
        }

        protected override Task<ScoreInfo[]> FetchNextPage(int direction)
        {
            // Don't attempt to index scores if the given score has an invalid online ID.
            // This can happen if the score failed to submit but is otherwise in a presentable state.
            return scoreId <= 0 ? Task.FromResult<ScoreInfo[]>([]) : base.FetchNextPage(direction);
        }

        protected override APIRequest<MultiplayerScore> CreateScoreRequest() => new ShowPlaylistScoreRequest(RoomId, PlaylistItem.ID, scoreId);

        protected override void OnScoresAdded(ScoreInfo[] scores)
        {
            base.OnScoresAdded(scores);
            SelectedScore.Value ??= scores.SingleOrDefault(s => s.OnlineID == scoreId);
        }
    }
}
