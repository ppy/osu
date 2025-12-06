// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.RoundResults
{
    /// <summary>
    /// Per-round results, during <see cref="MatchmakingStage.ResultsDisplaying"/>
    /// </summary>
    public partial class SubScreenRoundResults : MatchmakingSubScreen
    {
        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Hidden;
        public override Drawable? PlayersDisplayArea => null;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private PanelContainer panelContainer = null!;
        private LoadingSpinner loadingSpinner = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                panelContainer = new PanelContainer
                {
                    RelativeSizeAxes = Axes.Both
                },
                loadingSpinner = new LoadingSpinner
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadingSpinner.Show();

            queryScores().FireAndForget();
        }

        private async Task queryScores()
        {
            try
            {
                if (client.Room == null)
                    return;

                Task<APIBeatmap?> beatmapTask = beatmapLookupCache.GetBeatmapAsync(client.Room.CurrentPlaylistItem.BeatmapID);
                TaskCompletionSource<List<MultiplayerScore>> scoreTask = new TaskCompletionSource<List<MultiplayerScore>>();

                var request = new IndexPlaylistScoresRequest(client.Room.RoomID, client.Room.Settings.PlaylistItemId);
                request.Success += req => scoreTask.SetResult(req.Scores);
                request.Failure += e => scoreTask.SetException(e);
                api.Queue(request);

                await Task.WhenAll(beatmapTask, scoreTask.Task).ConfigureAwait(false);

                APIBeatmap? apiBeatmap = beatmapTask.GetResultSafely();
                List<MultiplayerScore> apiScores = scoreTask.Task.GetResultSafely();

                if (apiBeatmap == null)
                    return;

                // Reference: PlaylistItemResultsScreen
                setScores(apiScores.Select(s => s.CreateScoreInfo(scoreManager, rulesets, new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(apiBeatmap.Difficulty),
                    Metadata =
                    {
                        Artist = apiBeatmap.Metadata.Artist,
                        Title = apiBeatmap.Metadata.Title,
                        Author = new RealmUser
                        {
                            Username = apiBeatmap.Metadata.Author.Username,
                            OnlineID = apiBeatmap.Metadata.Author.OnlineID,
                        }
                    },
                    DifficultyName = apiBeatmap.DifficultyName,
                    StarRating = apiBeatmap.StarRating,
                    Length = apiBeatmap.Length,
                    BPM = apiBeatmap.BPM
                })).ToArray());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load scores for playlist item.");
                throw;
            }
            finally
            {
                Scheduler.Add(() => loadingSpinner.Hide());
            }
        }

        private void setScores(ScoreInfo[] scores) => Scheduler.Add(() =>
        {
            panelContainer.ChildrenEnumerable = scores.Select(s => new RoundResultsScorePanel(s)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        });

        private partial class RoundResultsScorePanel : CompositeDrawable
        {
            public RoundResultsScorePanel(ScoreInfo score)
            {
                Size = new Vector2(ScorePanel.CONTRACTED_WIDTH, ScorePanel.CONTRACTED_HEIGHT);

                InternalChild = new ScorePanel(score);
            }

            public override bool PropagateNonPositionalInputSubTree => false;
            public override bool PropagatePositionalInputSubTree => false;
        }

        private partial class PanelContainer : Container<RoundResultsScorePanel>
        {
            protected override Container<RoundResultsScorePanel> Content => flowContainer;

            private readonly Container centreingContainer;
            private readonly Container<RoundResultsScorePanel> flowContainer;

            public PanelContainer()
            {
                InternalChild = new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = centreingContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Child = flowContainer = new FillFlowContainer<RoundResultsScorePanel>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(5)
                        }
                    }
                };
            }

            protected override void Update()
            {
                base.Update();
                centreingContainer.Width = Math.Max(DrawWidth, flowContainer.DrawWidth);
            }
        }
    }
}
