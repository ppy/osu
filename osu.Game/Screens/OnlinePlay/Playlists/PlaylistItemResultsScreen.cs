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
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public abstract partial class PlaylistItemResultsScreen : ResultsScreen
    {
        protected readonly long RoomId;
        protected readonly PlaylistItem PlaylistItem;

        protected LoadingSpinner LeftSpinner { get; private set; } = null!;
        protected LoadingSpinner CentreSpinner { get; private set; } = null!;
        protected LoadingSpinner RightSpinner { get; private set; } = null!;

        private MultiplayerScores? higherScores;
        private MultiplayerScores? lowerScores;

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        protected ScoreManager ScoreManager { get; private set; } = null!;

        [Resolved]
        protected RulesetStore Rulesets { get; private set; } = null!;

        protected PlaylistItemResultsScreen(ScoreInfo? score, long roomId, PlaylistItem playlistItem)
            : base(score)
        {
            RoomId = roomId;
            PlaylistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Bottom = TwoLayerButton.SIZE_EXTENDED.Y },
                Children = new Drawable[]
                {
                    LeftSpinner = new PanelListLoadingSpinner(ScorePanelList)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.Centre,
                    },
                    CentreSpinner = new PanelListLoadingSpinner(ScorePanelList)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        State = { Value = Score == null ? Visibility.Visible : Visibility.Hidden },
                    },
                    RightSpinner = new PanelListLoadingSpinner(ScorePanelList)
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.Centre,
                    },
                }
            });
        }

        protected abstract APIRequest<MultiplayerScore> CreateScoreRequest();

        protected sealed override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            // This performs two requests:
            // 1. A request to show the relevant score (and scores around).
            // 2. If that fails, a request to index the room starting from the highest score.

            var userScoreReq = CreateScoreRequest();

            userScoreReq.Success += userScore =>
            {
                var allScores = new List<MultiplayerScore> { userScore };

                // Other scores could have arrived between score submission and entering the results screen. Ensure the local player score position is up to date.
                if (Score != null)
                {
                    Score.Position = userScore.Position;
                    ScorePanelList.GetPanelForScore(Score).ScorePosition.Value = userScore.Position;
                }

                if (userScore.ScoresAround?.Higher != null)
                {
                    allScores.AddRange(userScore.ScoresAround.Higher.Scores);
                    higherScores = userScore.ScoresAround.Higher;

                    Debug.Assert(userScore.Position != null);
                    setPositions(higherScores, userScore.Position.Value, -1);
                }

                if (userScore.ScoresAround?.Lower != null)
                {
                    allScores.AddRange(userScore.ScoresAround.Lower.Scores);
                    lowerScores = userScore.ScoresAround.Lower;

                    Debug.Assert(userScore.Position != null);
                    setPositions(lowerScores, userScore.Position.Value, 1);
                }

                Schedule(() =>
                {
                    PerformSuccessCallback(scoresCallback, allScores);
                    hideLoadingSpinners();
                });
            };

            // On failure, fallback to a normal index.
            userScoreReq.Failure += _ => API.Queue(createIndexRequest(scoresCallback));

            return userScoreReq;
        }

        protected override APIRequest? FetchNextPage(int direction, Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            Debug.Assert(direction == 1 || direction == -1);

            MultiplayerScores? pivot = direction == -1 ? higherScores : lowerScores;

            if (pivot?.Cursor == null)
                return null;

            if (pivot == higherScores)
                LeftSpinner.Show();
            else
                RightSpinner.Show();

            return createIndexRequest(scoresCallback, pivot);
        }

        /// <summary>
        /// Creates a <see cref="IndexPlaylistScoresRequest"/> with an optional score pivot.
        /// </summary>
        /// <remarks>Does not queue the request.</remarks>
        /// <param name="scoresCallback">The callback to perform with the resulting scores.</param>
        /// <param name="pivot">An optional score pivot to retrieve scores around. Can be null to retrieve scores from the highest score.</param>
        /// <returns>The indexing <see cref="APIRequest"/>.</returns>
        private APIRequest createIndexRequest(Action<IEnumerable<ScoreInfo>> scoresCallback, MultiplayerScores? pivot = null)
        {
            var indexReq = pivot != null
                ? new IndexPlaylistScoresRequest(RoomId, PlaylistItem.ID, pivot.Cursor, pivot.Params)
                : new IndexPlaylistScoresRequest(RoomId, PlaylistItem.ID);

            indexReq.Success += r =>
            {
                if (pivot == lowerScores)
                {
                    lowerScores = r;
                    setPositions(r, pivot, 1);
                }
                else
                {
                    higherScores = r;
                    setPositions(r, pivot, -1);
                }

                Schedule(() =>
                {
                    PerformSuccessCallback(scoresCallback, r.Scores, r);
                    hideLoadingSpinners(r);
                });
            };

            indexReq.Failure += _ => hideLoadingSpinners(pivot);

            return indexReq;
        }

        /// <summary>
        /// Transforms returned <see cref="MultiplayerScores"/> into <see cref="ScoreInfo"/>s, ensure the <see cref="ScorePanelList"/> is put into a sane state, and invokes a given success callback.
        /// </summary>
        /// <param name="callback">The callback to invoke with the final <see cref="ScoreInfo"/>s.</param>
        /// <param name="scores">The <see cref="MultiplayerScore"/>s that were retrieved from <see cref="APIRequest"/>s.</param>
        /// <param name="pivot">An optional pivot around which the scores were retrieved.</param>
        protected virtual ScoreInfo[] PerformSuccessCallback(Action<IEnumerable<ScoreInfo>> callback, List<MultiplayerScore> scores, MultiplayerScores? pivot = null)
        {
            var scoreInfos = scores.Select(s => s.CreateScoreInfo(ScoreManager, Rulesets, Beatmap.Value.BeatmapInfo)).OrderByTotalScore().ToArray();

            // Invoke callback to add the scores. Exclude the score provided to this screen since it's added already.
            callback.Invoke(scoreInfos.Where(s => s.OnlineID != Score?.OnlineID));

            return scoreInfos;
        }

        private void hideLoadingSpinners(MultiplayerScores? pivot = null)
        {
            CentreSpinner.Hide();

            if (pivot == lowerScores)
                RightSpinner.Hide();
            else if (pivot == higherScores)
                LeftSpinner.Hide();
        }

        /// <summary>
        /// Applies positions to all <see cref="MultiplayerScore"/>s referenced to a given pivot.
        /// </summary>
        /// <param name="scores">The <see cref="MultiplayerScores"/> to set positions on.</param>
        /// <param name="pivot">The pivot.</param>
        /// <param name="increment">The amount to increment the pivot position by for each <see cref="MultiplayerScore"/> in <paramref name="scores"/>.</param>
        private void setPositions(MultiplayerScores scores, MultiplayerScores? pivot, int increment)
            => setPositions(scores, pivot?.Scores[^1].Position ?? 0, increment);

        /// <summary>
        /// Applies positions to all <see cref="MultiplayerScore"/>s referenced to a given pivot.
        /// </summary>
        /// <param name="scores">The <see cref="MultiplayerScores"/> to set positions on.</param>
        /// <param name="pivotPosition">The pivot position.</param>
        /// <param name="increment">The amount to increment the pivot position by for each <see cref="MultiplayerScore"/> in <paramref name="scores"/>.</param>
        private void setPositions(MultiplayerScores scores, int pivotPosition, int increment)
        {
            foreach (var s in scores.Scores)
            {
                pivotPosition += increment;
                s.Position = pivotPosition;
            }
        }

        private partial class PanelListLoadingSpinner : LoadingSpinner
        {
            private readonly ScorePanelList list;

            /// <summary>
            /// Creates a new <see cref="PanelListLoadingSpinner"/>.
            /// </summary>
            /// <param name="list">The list to track.</param>
            /// <param name="withBox">Whether the spinner should have a surrounding black box for visibility.</param>
            public PanelListLoadingSpinner(ScorePanelList list, bool withBox = true)
                : base(withBox)
            {
                this.list = list;
            }

            protected override void Update()
            {
                base.Update();

                float panelOffset = list.DrawWidth / 2 - ScorePanel.EXPANDED_WIDTH;

                if ((Anchor & Anchor.x0) > 0)
                    X = (float)(panelOffset - list.Current);
                else if ((Anchor & Anchor.x2) > 0)
                    X = (float)(list.ScrollableExtent - list.Current - panelOffset);
            }
        }
    }
}
