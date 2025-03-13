// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
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

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

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

        protected override async Task<ScoreInfo[]> FetchScores()
        {
            // This performs two requests:
            // 1. A request to show the relevant score (and scores around).
            // 2. If that fails, a request to index the room starting from the highest score.

            var requestTaskSource = new TaskCompletionSource<MultiplayerScore>();
            var userScoreReq = CreateScoreRequest();
            userScoreReq.Success += requestTaskSource.SetResult;
            userScoreReq.Failure += requestTaskSource.SetException;
            API.Queue(userScoreReq);

            try
            {
                var userScore = await requestTaskSource.Task.ConfigureAwait(false);
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

                return await transformScores(allScores).ConfigureAwait(false);
            }
            catch
            {
                return await fetchScoresAround().ConfigureAwait(false);
            }
        }

        protected override async Task<ScoreInfo[]> FetchNextPage(int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);

            MultiplayerScores? pivot = direction == -1 ? higherScores : lowerScores;
            if (pivot?.Cursor == null)
                return [];

            Schedule(() =>
            {
                if (pivot == higherScores)
                    LeftSpinner.Show();
                else
                    RightSpinner.Show();
            });

            return await fetchScoresAround(pivot).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see cref="IndexPlaylistScoresRequest"/> with an optional score pivot.
        /// </summary>
        /// <remarks>Does not queue the request.</remarks>
        /// <param name="pivot">An optional score pivot to retrieve scores around. Can be null to retrieve scores from the highest score.</param>
        private async Task<ScoreInfo[]> fetchScoresAround(MultiplayerScores? pivot = null)
        {
            var requestTaskSource = new TaskCompletionSource<IndexedMultiplayerScores>();
            var indexReq = pivot != null
                ? new IndexPlaylistScoresRequest(RoomId, PlaylistItem.ID, pivot.Cursor, pivot.Params)
                : new IndexPlaylistScoresRequest(RoomId, PlaylistItem.ID);
            indexReq.Success += requestTaskSource.SetResult;
            indexReq.Failure += requestTaskSource.SetException;
            API.Queue(indexReq);

            try
            {
                var index = await requestTaskSource.Task.ConfigureAwait(false);

                if (pivot == lowerScores)
                {
                    lowerScores = index;
                    setPositions(index, pivot, 1);
                }
                else
                {
                    higherScores = index;
                    setPositions(index, pivot, -1);

                    // when paginating the results, it's possible for the user's score to naturally fall down the rankings.
                    // unmitigated, this can cause scores at the very top of the rankings to have zero or negative positions
                    // because the positions are counted backwards from the user's score, which has increased in this case during pagination.
                    // if this happens, just give the top score the first position.
                    // note that this isn't 100% correct, but it *is* however the most reliable way to mask the problem.
                    int smallestPosition = index.Scores.Min(s => s.Position ?? 1);

                    if (smallestPosition < 1)
                    {
                        int offset = 1 - smallestPosition;

                        foreach (var scorePanel in ScorePanelList.GetScorePanels())
                            scorePanel.ScorePosition.Value += offset;

                        foreach (var score in index.Scores)
                            score.Position += offset;
                    }
                }

                return await transformScores(index.Scores).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to fetch scores (room: {RoomId}, item: {PlaylistItem.ID}): {ex}");
                return [];
            }
        }

        /// <summary>
        /// Transforms returned <see cref="MultiplayerScores"/> into <see cref="ScoreInfo"/>s.
        /// </summary>
        /// <param name="scores">The <see cref="MultiplayerScore"/>s that were retrieved from <see cref="APIRequest"/>s.</param>
        private async Task<ScoreInfo[]> transformScores(List<MultiplayerScore> scores)
        {
            int[] allBeatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToArray();
            BeatmapInfo[] localBeatmaps = allBeatmapIds.Select(id => beatmapManager.QueryBeatmap(b => b.OnlineID == id))
                                                       .Where(b => b != null)
                                                       .ToArray()!;

            int[] missingBeatmapIds = allBeatmapIds.Except(localBeatmaps.Select(b => b.OnlineID)).ToArray();
            APIBeatmap[] onlineBeatmaps = (await beatmapLookupCache.GetBeatmapsAsync(missingBeatmapIds).ConfigureAwait(false)).Where(b => b != null).ToArray()!;

            Dictionary<int, BeatmapInfo> beatmapsById = new Dictionary<int, BeatmapInfo>();

            foreach (var beatmap in localBeatmaps)
                beatmapsById[beatmap.OnlineID] = beatmap;

            foreach (var beatmap in onlineBeatmaps)
            {
                // Minimal data required to get various components in this screen to display correctly.
                beatmapsById[beatmap.OnlineID] = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(beatmap.Difficulty),
                    Metadata =
                    {
                        Artist = beatmap.Metadata.Artist,
                        Title = beatmap.Metadata.Title,
                        Author = new RealmUser
                        {
                            Username = beatmap.Metadata.Author.Username,
                            OnlineID = beatmap.Metadata.Author.OnlineID,
                        }
                    },
                    DifficultyName = beatmap.DifficultyName,
                    StarRating = beatmap.StarRating,
                    Length = beatmap.Length,
                    BPM = beatmap.BPM
                };
            }

            // Validate that we have all beatmaps we need.
            foreach (int id in allBeatmapIds)
            {
                if (!beatmapsById.ContainsKey(id))
                {
                    Logger.Log($"Failed to fetch beatmap {id} to display scores for playlist item {PlaylistItem.ID}");
                    beatmapsById[id] = Beatmap.Value.BeatmapInfo;
                }
            }

            // Exclude the score provided to this screen since it's added already.
            return scores
                   .Where(s => s.ID != Score?.OnlineID)
                   .Select(s => s.CreateScoreInfo(ScoreManager, Rulesets, beatmapsById[s.BeatmapId]))
                   .OrderByTotalScore()
                   .ToArray();
        }

        protected override void OnScoresAdded(ScoreInfo[] scores)
        {
            base.OnScoresAdded(scores);

            CentreSpinner.Hide();
            RightSpinner.Hide();
            LeftSpinner.Hide();
        }

        /// <summary>
        /// Applies positions to all <see cref="MultiplayerScore"/>s referenced to a given pivot.
        /// </summary>
        /// <param name="scores">The <see cref="MultiplayerScores"/> to set positions on.</param>
        /// <param name="pivot">The pivot.</param>
        /// <param name="increment">The amount to increment the pivot position by for each <see cref="MultiplayerScore"/> in <paramref name="scores"/>.</param>
        private static void setPositions(MultiplayerScores scores, MultiplayerScores? pivot, int increment)
            => setPositions(scores, pivot?.Scores[^1].Position ?? 0, increment);

        /// <summary>
        /// Applies positions to all <see cref="MultiplayerScore"/>s referenced to a given pivot.
        /// </summary>
        /// <param name="scores">The <see cref="MultiplayerScores"/> to set positions on.</param>
        /// <param name="pivotPosition">The pivot position.</param>
        /// <param name="increment">The amount to increment the pivot position by for each <see cref="MultiplayerScore"/> in <paramref name="scores"/>.</param>
        private static void setPositions(MultiplayerScores scores, int pivotPosition, int increment)
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
