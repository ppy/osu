// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking
{
    public class TimeshiftResultsScreen : ResultsScreen
    {
        private readonly int roomId;
        private readonly PlaylistItem playlistItem;

        private LoadingSpinner loadingLayer;
        private MultiplayerScores higherScores;
        private MultiplayerScores lowerScores;

        [Resolved]
        private IAPIProvider api { get; set; }

        public TimeshiftResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem, bool allowRetry = true)
            : base(score, allowRetry)
        {
            this.roomId = roomId;
            this.playlistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                X = -10,
                State = { Value = Score == null ? Visibility.Visible : Visibility.Hidden },
                Padding = new MarginPadding { Bottom = TwoLayerButton.SIZE_EXTENDED.Y }
            });
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            // This performs two requests:
            // 1. A request to show the user's score (and scores around).
            // 2. If that fails, a request to index the room starting from the highest score.

            var userScoreReq = new ShowPlaylistUserScoreRequest(roomId, playlistItem.ID, api.LocalUser.Value.Id);

            userScoreReq.Success += userScore =>
            {
                var allScores = new List<MultiplayerScore> { userScore };

                if (userScore.ScoresAround?.Higher != null)
                {
                    allScores.AddRange(userScore.ScoresAround.Higher.Scores);
                    higherScores = userScore.ScoresAround.Higher;
                }

                if (userScore.ScoresAround?.Lower != null)
                {
                    allScores.AddRange(userScore.ScoresAround.Lower.Scores);
                    lowerScores = userScore.ScoresAround.Lower;
                }

                performSuccessCallback(scoresCallback, allScores);
            };

            userScoreReq.Failure += _ =>
            {
                // Fallback to a normal index.
                var indexReq = new IndexPlaylistScoresRequest(roomId, playlistItem.ID);

                indexReq.Success += r =>
                {
                    performSuccessCallback(scoresCallback, r.Scores);
                    lowerScores = r;
                };

                indexReq.Failure += __ => loadingLayer.Hide();

                api.Queue(indexReq);
            };

            return userScoreReq;
        }

        protected override APIRequest FetchNextPage(int direction, Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            Debug.Assert(direction == 1 || direction == -1);

            MultiplayerScores pivot = direction == -1 ? higherScores : lowerScores;

            if (pivot?.Cursor == null)
                return null;

            var indexReq = new IndexPlaylistScoresRequest(roomId, playlistItem.ID, pivot.Cursor, pivot.Params);

            indexReq.Success += r =>
            {
                if (direction == -1)
                    higherScores = r;
                else
                    lowerScores = r;

                performSuccessCallback(scoresCallback, r.Scores);
            };

            indexReq.Failure += _ => loadingLayer.Hide();

            return indexReq;
        }

        private void performSuccessCallback(Action<IEnumerable<ScoreInfo>> callback, List<MultiplayerScore> scores)
        {
            var scoreInfos = new List<ScoreInfo>(scores.Select(s => s.CreateScoreInfo(playlistItem)));

            // Select a score if we don't already have one selected.
            // Note: This is done before the callback so that the panel list centres on the selected score before panels are added (eliminating initial scroll).
            if (SelectedScore.Value == null)
            {
                Schedule(() =>
                {
                    // Prefer selecting the local user's score, or otherwise default to the first visible score.
                    SelectedScore.Value = scoreInfos.FirstOrDefault(s => s.User.Id == api.LocalUser.Value.Id) ?? scoreInfos.FirstOrDefault();
                });
            }

            // Invoke callback to add the scores. Exclude the user's current score which was added previously.
            callback?.Invoke(scoreInfos.Where(s => s.ID != Score?.OnlineScoreID));

            loadingLayer.Hide();
        }
    }
}
