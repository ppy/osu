// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.RoundResults
{
    public partial class RoundResultsScreen : MatchmakingSubScreen
    {
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            queryScores().FireAndForget();
        }

        private async Task queryScores()
        {
            if (client.Room == null)
                return;

            Task<APIBeatmap?> beatmapTask = beatmapLookupCache.GetBeatmapAsync(client.Room.CurrentPlaylistItem.BeatmapID);
            TaskCompletionSource<List<MultiplayerScore>> scoreTask = new TaskCompletionSource<List<MultiplayerScore>>();

            var request = new IndexPlaylistScoresRequest(client.Room.RoomID, client.Room.Settings.PlaylistItemId);
            request.Success += req => scoreTask.SetResult(req.Scores);
            request.Failure += e => scoreTask.SetException(e);
            api.Queue(request);

            await Task.WhenAll(beatmapTask, scoreTask.Task);

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

        private void setScores(ScoreInfo[] scores) => Scheduler.Add(() =>
        {
            const int panel_spacing = 5;

            Container panels;

            AddInternal(new AutoScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = panels = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = scores.Length * (ScorePanel.CONTRACTED_WIDTH + panel_spacing),
                    ChildrenEnumerable = scores.Select(s => new RoundResultsScorePanel(s)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft
                    })
                }
            });

            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].MoveToX(panels.DrawWidth * 2)
                         .Delay(i * 100)
                         .MoveToX((ScorePanel.CONTRACTED_WIDTH + panel_spacing) * i, 500, Easing.OutQuint);
            }
        });

        private partial class AutoScrollContainer : UserTrackingScrollContainer
        {
            private const float initial_offset = -0.5f;
            private const double scroll_duration = 20000;

            private double scrollStartTime;

            public AutoScrollContainer()
                : base(Direction.Horizontal)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                scrollStartTime = Time.Current;
            }

            protected override void Update()
            {
                base.Update();

                double scrollOffset = (Time.Current - scrollStartTime) / scroll_duration;

                if (!UserScrolling && scrollOffset < 1)
                    ScrollTo(DrawWidth * (initial_offset + scrollOffset), false);
            }
        }
    }
}
