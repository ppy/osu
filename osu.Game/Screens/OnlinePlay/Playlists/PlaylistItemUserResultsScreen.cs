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
    /// Shows the user's best score for a given playlist item, with scores around included.
    /// </summary>
    public partial class PlaylistItemUserResultsScreen : PlaylistItemResultsScreen
    {
        public PlaylistItemUserResultsScreen(ScoreInfo? score, long roomId, PlaylistItem playlistItem)
            : base(score, roomId, playlistItem)
        {
        }

        protected override APIRequest<MultiplayerScore> CreateScoreRequest() => new ShowPlaylistUserScoreRequest(RoomId, PlaylistItem.ID, API.LocalUser.Value.Id);

        protected override ScoreInfo[] PerformSuccessCallback(Action<IEnumerable<ScoreInfo>> callback, List<MultiplayerScore> scores, MultiplayerScores? pivot = null)
        {
            var scoreInfos = scores.Select(s => s.CreateScoreInfo(ScoreManager, Rulesets, PlaylistItem, Beatmap.Value.BeatmapInfo)).OrderByTotalScore().ToArray();

            // Select a score if we don't already have one selected.
            // Note: This is done before the callback so that the panel list centres on the selected score before panels are added (eliminating initial scroll).
            if (SelectedScore.Value == null)
            {
                Schedule(() =>
                {
                    // Prefer selecting the local user's score, or otherwise default to the first visible score.
                    SelectedScore.Value = scoreInfos.FirstOrDefault(s => s.User.OnlineID == API.LocalUser.Value.Id) ?? scoreInfos.FirstOrDefault();
                });
            }

            // Invoke callback to add the scores. Exclude the user's current score which was added previously.
            callback.Invoke(scoreInfos.Where(s => s.OnlineID != Score?.OnlineID));

            return scoreInfos;
        }
    }
}
