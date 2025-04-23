// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// Shows a user's best score in a playlist item, with scores around included.
    /// </summary>
    public partial class PlaylistItemUserBestResultsScreen : PlaylistItemResultsScreen
    {
        private readonly int userId;
        private WorkingBeatmap itemBeatmap = null!;

        public PlaylistItemUserBestResultsScreen(long roomId, PlaylistItem playlistItem, int userId)
            : base(null, roomId, playlistItem)
        {
            this.userId = userId;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            var localBeatmap = beatmaps.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", PlaylistItem.Beatmap.OnlineID);
            itemBeatmap = beatmaps.GetWorkingBeatmap(localBeatmap);
        }

        protected override APIRequest<MultiplayerScore> CreateScoreRequest() => new ShowPlaylistUserScoreRequest(RoomId, PlaylistItem.ID, userId);

        protected override void OnScoresAdded(ScoreInfo[] scores)
        {
            base.OnScoresAdded(scores);

            // Prefer selecting the local user's score, or otherwise default to the first visible score.
            SelectedScore.Value ??= scores.FirstOrDefault(s => s.UserID == userId) ?? scores.FirstOrDefault();
        }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(itemBeatmap);
    }
}
