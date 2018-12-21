// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Multi.Ranking.Pages;
using osu.Game.Screens.Multi.Ranking.Types;
using osu.Game.Screens.Ranking.Pages;
using osu.Game.Screens.Ranking.Types;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMultiResults : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MultiResults),
            typeof(RoomRankingResultType),
            typeof(RoomRankingResultsPage)
        };

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);

            Child = new TestMultiResults(new ScoreInfo
            {
                User = new User { Id = 10 },
            });
        }

        private class TestMultiResults : MultiResults
        {
            private readonly Room room;

            public TestMultiResults(ScoreInfo score)
                : this(score, new Room())
            {
            }

            public TestMultiResults(ScoreInfo score, Room room)
                : base(score, room)
            {
                this.room = room;
            }

            protected override IEnumerable<IResultType> CreateResultTypes() => new IResultType[]
            {
                new ScoreResultType(Score, Beatmap),
                new RankingResultType(Score, Beatmap),
                new TestRoomRankingResultType(Score, Beatmap, room),
            };
        }

        private class TestRoomRankingResultType : RoomRankingResultType
        {
            private readonly ScoreInfo score;
            private readonly WorkingBeatmap beatmap;
            private readonly Room room;

            public TestRoomRankingResultType(ScoreInfo score, WorkingBeatmap beatmap, Room room)
                : base(score, beatmap, room)
            {
                this.score = score;
                this.beatmap = beatmap;
                this.room = room;
            }

            public override ResultsPage CreatePage() => new TestRoomRankingResultsPage(score, beatmap, room);
        }

        private class TestRoomRankingResultsPage : RoomRankingResultsPage
        {
            public TestRoomRankingResultsPage(ScoreInfo score, WorkingBeatmap beatmap, Room room)
                : base(score, beatmap, room)
            {
            }

            protected override MatchLeaderboard CreateLeaderboard(Room room) => new TestMatchLeaderboard(room);
        }

        private class TestMatchLeaderboard : MatchLeaderboard
        {
            public TestMatchLeaderboard(Room room)
                : base(room)
            {
            }

            protected override APIRequest FetchScores(Action<IEnumerable<RoomScore>> scoresCallback)
            {
                var scores = Enumerable.Range(0, 50).Select(createRoomScore).ToArray();

                scoresCallback?.Invoke(scores);
                ScoresLoaded?.Invoke(scores);

                return null;
            }

            private RoomScore createRoomScore(int id) => new RoomScore
            {
                User = new User { Id = id, Username = $"User {id}" },
                Accuracy = 0.98,
                TotalScore = 987654,
                TotalAttempts = 13,
                CompletedAttempts = 5
            };
        }
    }
}
