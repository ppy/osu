// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsResultsScreen : ScreenTestScene
    {
        private const int scores_per_result = 10;

        private TestResultsScreen resultsScreen;
        private int currentScoreId;
        private bool requestComplete;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            currentScoreId = 0;
            requestComplete = false;
            bindHandler();
        });

        [Test]
        public void TestShowWithUserScore()
        {
            ScoreInfo userScore = null;

            AddStep("bind user score info handler", () =>
            {
                userScore = new TestScoreInfo(new OsuRuleset().RulesetInfo) { OnlineScoreID = currentScoreId++ };
                bindHandler(userScore: userScore);
            });

            createResults(() => userScore);
            waitForDisplay();

            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineScoreID == userScore.OnlineScoreID).State == PanelState.Expanded);
        }

        [Test]
        public void TestShowNullUserScore()
        {
            createResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestShowUserScoreWithDelay()
        {
            ScoreInfo userScore = null;

            AddStep("bind user score info handler", () =>
            {
                userScore = new TestScoreInfo(new OsuRuleset().RulesetInfo) { OnlineScoreID = currentScoreId++ };
                bindHandler(true, userScore);
            });

            createResults(() => userScore);
            waitForDisplay();

            AddAssert("more than 1 panel displayed", () => this.ChildrenOfType<ScorePanel>().Count() > 1);
            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineScoreID == userScore.OnlineScoreID).State == PanelState.Expanded);
        }

        [Test]
        public void TestShowNullUserScoreWithDelay()
        {
            AddStep("bind delayed handler", () => bindHandler(true));

            createResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestFetchWhenScrolledToTheRight()
        {
            createResults();
            waitForDisplay();

            AddStep("bind delayed handler", () => bindHandler(true));

            for (int i = 0; i < 2; i++)
            {
                int beforePanelCount = 0;

                AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
                AddStep("scroll to right", () => resultsScreen.ScorePanelList.ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));

                AddAssert("right loading spinner shown", () => resultsScreen.RightSpinner.State.Value == Visibility.Visible);
                waitForDisplay();

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount + scores_per_result);
                AddAssert("right loading spinner hidden", () => resultsScreen.RightSpinner.State.Value == Visibility.Hidden);
            }
        }

        [Test]
        public void TestFetchWhenScrolledToTheLeft()
        {
            ScoreInfo userScore = null;

            AddStep("bind user score info handler", () =>
            {
                userScore = new TestScoreInfo(new OsuRuleset().RulesetInfo) { OnlineScoreID = currentScoreId++ };
                bindHandler(userScore: userScore);
            });

            createResults(() => userScore);
            waitForDisplay();

            AddStep("bind delayed handler", () => bindHandler(true));

            for (int i = 0; i < 2; i++)
            {
                int beforePanelCount = 0;

                AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
                AddStep("scroll to left", () => resultsScreen.ScorePanelList.ChildrenOfType<OsuScrollContainer>().Single().ScrollToStart(false));

                AddAssert("left loading spinner shown", () => resultsScreen.LeftSpinner.State.Value == Visibility.Visible);
                waitForDisplay();

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount + scores_per_result);
                AddAssert("left loading spinner hidden", () => resultsScreen.LeftSpinner.State.Value == Visibility.Hidden);
            }
        }

        private void createResults(Func<ScoreInfo> getScore = null)
        {
            AddStep("load results", () =>
            {
                LoadScreen(resultsScreen = new TestResultsScreen(getScore?.Invoke(), 1, new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                }));
            });

            AddUntilStep("wait for load", () => resultsScreen.ChildrenOfType<ScorePanelList>().FirstOrDefault()?.AllPanelsVisible == true);
        }

        private void waitForDisplay()
        {
            AddUntilStep("wait for request to complete", () => requestComplete);
            AddUntilStep("wait for panels to be visible", () => resultsScreen.ChildrenOfType<ScorePanelList>().FirstOrDefault()?.AllPanelsVisible == true);
            AddWaitStep("wait for display", 5);
        }

        private void bindHandler(bool delayed = false, ScoreInfo userScore = null, bool failRequests = false) => ((DummyAPIAccess)API).HandleRequest = request =>
        {
            // pre-check for requests we should be handling (as they are scheduled below).
            switch (request)
            {
                case ShowPlaylistUserScoreRequest _:
                case IndexPlaylistScoresRequest _:
                    break;

                default:
                    return false;
            }

            requestComplete = false;

            double delay = delayed ? 3000 : 0;

            Scheduler.AddDelayed(() =>
            {
                if (failRequests)
                {
                    triggerFail(request);
                    return;
                }

                switch (request)
                {
                    case ShowPlaylistUserScoreRequest s:
                        if (userScore == null)
                            triggerFail(s);
                        else
                            triggerSuccess(s, createUserResponse(userScore));
                        break;

                    case IndexPlaylistScoresRequest i:
                        triggerSuccess(i, createIndexResponse(i));
                        break;
                }
            }, delay);

            return true;
        };

        private void triggerSuccess<T>(APIRequest<T> req, T result)
            where T : class
        {
            requestComplete = true;
            req.TriggerSuccess(result);
        }

        private void triggerFail(APIRequest req)
        {
            requestComplete = true;
            req.TriggerFailure(new WebException("Failed."));
        }

        private MultiplayerScore createUserResponse([NotNull] ScoreInfo userScore)
        {
            var multiplayerUserScore = new MultiplayerScore
            {
                ID = (int)(userScore.OnlineScoreID ?? currentScoreId++),
                Accuracy = userScore.Accuracy,
                EndedAt = userScore.Date,
                Passed = userScore.Passed,
                Rank = userScore.Rank,
                Position = 200,
                MaxCombo = userScore.MaxCombo,
                TotalScore = userScore.TotalScore,
                User = userScore.User,
                Statistics = userScore.Statistics,
                ScoresAround = new MultiplayerScoresAround
                {
                    Higher = new MultiplayerScores(),
                    Lower = new MultiplayerScores()
                }
            };

            for (int i = 1; i <= scores_per_result; i++)
            {
                multiplayerUserScore.ScoresAround.Lower.Scores.Add(new MultiplayerScore
                {
                    ID = currentScoreId++,
                    Accuracy = userScore.Accuracy,
                    EndedAt = userScore.Date,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    TotalScore = userScore.TotalScore - i,
                    User = new User
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    Statistics = userScore.Statistics
                });

                multiplayerUserScore.ScoresAround.Higher.Scores.Add(new MultiplayerScore
                {
                    ID = currentScoreId++,
                    Accuracy = userScore.Accuracy,
                    EndedAt = userScore.Date,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    TotalScore = userScore.TotalScore + i,
                    User = new User
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    Statistics = userScore.Statistics
                });
            }

            addCursor(multiplayerUserScore.ScoresAround.Lower);
            addCursor(multiplayerUserScore.ScoresAround.Higher);

            return multiplayerUserScore;
        }

        private IndexedMultiplayerScores createIndexResponse(IndexPlaylistScoresRequest req)
        {
            var result = new IndexedMultiplayerScores();

            long startTotalScore = req.Cursor?.Properties["total_score"].ToObject<long>() ?? 1000000;
            string sort = req.IndexParams?.Properties["sort"].ToObject<string>() ?? "score_desc";

            for (int i = 1; i <= scores_per_result; i++)
            {
                result.Scores.Add(new MultiplayerScore
                {
                    ID = currentScoreId++,
                    Accuracy = 1,
                    EndedAt = DateTimeOffset.Now,
                    Passed = true,
                    Rank = ScoreRank.X,
                    MaxCombo = 1000,
                    TotalScore = startTotalScore + (sort == "score_asc" ? i : -i),
                    User = new User
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    Statistics = new Dictionary<HitResult, int>
                    {
                        { HitResult.Miss, 1 },
                        { HitResult.Meh, 50 },
                        { HitResult.Good, 100 },
                        { HitResult.Great, 300 }
                    }
                });
            }

            addCursor(result);

            return result;
        }

        private void addCursor(MultiplayerScores scores)
        {
            scores.Cursor = new Cursor
            {
                Properties = new Dictionary<string, JToken>
                {
                    { "total_score", JToken.FromObject(scores.Scores[^1].TotalScore) },
                    { "score_id", JToken.FromObject(scores.Scores[^1].ID) },
                }
            };

            scores.Params = new IndexScoresParams
            {
                Properties = new Dictionary<string, JToken>
                {
                    { "sort", JToken.FromObject(scores.Scores[^1].TotalScore > scores.Scores[^2].TotalScore ? "score_asc" : "score_desc") }
                }
            };
        }

        private class TestResultsScreen : PlaylistsResultsScreen
        {
            public new LoadingSpinner LeftSpinner => base.LeftSpinner;
            public new LoadingSpinner CentreSpinner => base.CentreSpinner;
            public new LoadingSpinner RightSpinner => base.RightSpinner;
            public new ScorePanelList ScorePanelList => base.ScorePanelList;

            public TestResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem, bool allowRetry = true)
                : base(score, roomId, playlistItem, allowRetry)
            {
            }
        }
    }
}
