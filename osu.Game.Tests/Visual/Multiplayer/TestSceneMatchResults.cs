// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Multi.Ranking.Pages;
using osu.Game.Screens.Multi.Ranking.Types;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchResults : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchResults),
            typeof(RoomLeaderboardPageInfo),
            typeof(RoomLeaderboardPage)
        };

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);

            Room.RoomID.Value = 1;
            Room.Name.Value = "an awesome room";

            LoadScreen(new TestMatchResults(new ScoreInfo
            {
                User = new User { Id = 10 },
            }));
        }

        private class TestMatchResults : MatchResults
        {
            public TestMatchResults(ScoreInfo score)
                : base(score)
            {
            }

            protected override IEnumerable<IResultPageInfo> CreateResultPages() => new[] { new TestRoomLeaderboardPageInfo(Score, Beatmap.Value) };
        }

        private class TestRoomLeaderboardPageInfo : RoomLeaderboardPageInfo
        {
            private readonly ScoreInfo score;
            private readonly WorkingBeatmap beatmap;

            public TestRoomLeaderboardPageInfo(ScoreInfo score, WorkingBeatmap beatmap)
                : base(score, beatmap)
            {
                this.score = score;
                this.beatmap = beatmap;
            }

            public override ResultsPage CreatePage() => new TestRoomLeaderboardPage(score, beatmap);
        }

        private class TestRoomLeaderboardPage : RoomLeaderboardPage
        {
            public TestRoomLeaderboardPage(ScoreInfo score, WorkingBeatmap beatmap)
                : base(score, beatmap)
            {
            }

            protected override MatchLeaderboard CreateLeaderboard() => new TestMatchLeaderboard();
        }

        private class TestMatchLeaderboard : RoomLeaderboardPage.ResultsMatchLeaderboard
        {
            protected override APIRequest FetchScores(Action<IEnumerable<APIRoomScoreInfo>> scoresCallback)
            {
                var scores = Enumerable.Range(0, 50).Select(createRoomScore).ToArray();

                scoresCallback?.Invoke(scores);
                ScoresLoaded?.Invoke(scores);

                return null;
            }

            private APIRoomScoreInfo createRoomScore(int id) => new APIRoomScoreInfo
            {
                User = new User { Id = id, Username = $"User {id}" },
                Accuracy = 0.98,
                TotalScore = 987654,
                TotalAttempts = 13,
                CompletedBeatmaps = 5
            };
        }
    }
}
