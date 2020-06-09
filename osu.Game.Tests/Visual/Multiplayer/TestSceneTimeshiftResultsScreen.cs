// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneTimeshiftResultsScreen : ScreenTestScene
    {
        private bool roomsReceived;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            roomsReceived = false;
            bindHandler();
        });

        [Test]
        public void TestShowResultsWithScore()
        {
            createResults(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            AddWaitStep("wait for display", 5);
        }

        [Test]
        public void TestShowResultsNullScore()
        {
            createResults(null);
            AddWaitStep("wait for display", 5);
        }

        [Test]
        public void TestShowResultsNullScoreWithDelay()
        {
            AddStep("bind delayed handler", () => bindHandler(3000));
            createResults(null);
            AddUntilStep("wait for rooms to be received", () => roomsReceived);
            AddWaitStep("wait for display", 5);
        }

        private void createResults(ScoreInfo score)
        {
            AddStep("load results", () =>
            {
                LoadScreen(new TimeshiftResultsScreen(score, 1, new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                }));
            });
        }

        private void bindHandler(double delay = 0)
        {
            var roomScores = new List<RoomScore>();

            for (int i = 0; i < 10; i++)
            {
                roomScores.Add(new RoomScore
                {
                    ID = i,
                    Accuracy = 0.9 - 0.01 * i,
                    EndedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(i)),
                    Passed = true,
                    Rank = ScoreRank.B,
                    MaxCombo = 999,
                    TotalScore = 999999 - i * 1000,
                    User = new User
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    Statistics =
                    {
                        { HitResult.Miss, 1 },
                        { HitResult.Meh, 50 },
                        { HitResult.Good, 100 },
                        { HitResult.Great, 300 },
                    }
                });
            }

            ((DummyAPIAccess)API).HandleRequest = request =>
            {
                switch (request)
                {
                    case GetRoomPlaylistScoresRequest r:
                        if (delay == 0)
                            success();
                        else
                        {
                            Task.Run(async () =>
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(delay));
                                Schedule(success);
                            });
                        }

                        void success()
                        {
                            r.TriggerSuccess(new RoomPlaylistScores { Scores = roomScores });
                            roomsReceived = true;
                        }

                        break;
                }
            };
        }
    }
}
