// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsResultsScreen : ScreenTestScene
    {
        private const int scores_per_result = 10;
        private const int real_user_position = 200;

        private TestResultsScreen resultsScreen;

        private int lowestScoreId; // Score ID of the lowest score in the list.
        private int highestScoreId; // Score ID of the highest score in the list.

        private bool requestComplete;
        private int totalCount;
        private ScoreInfo userScore;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // Previous test instances of the results screen may still exist at this point so wait for
            // those screens to be cleaned up by the base SetUpSteps before re-initialising test state.
            // The screen also holds a leased Beatmap bindable so reassigning it must happen after
            // the screen has been exited.
            AddStep("initialise user scores and beatmap", () =>
            {
                lowestScoreId = 1;
                highestScoreId = 1;
                requestComplete = false;
                totalCount = 0;

                userScore = TestResources.CreateTestScoreInfo();
                userScore.TotalScore = 0;
                userScore.Statistics = new Dictionary<HitResult, int>();
                userScore.MaximumStatistics = new Dictionary<HitResult, int>();

                // Beatmap is required to be an actual beatmap so the scores can get their scores correctly
                // calculated for standardised scoring, else the tests that rely on ordering will fall over.
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
            });
        }

        public void SetUpRequestHandler(bool noScores = false)
        {
            AddStep("set up request handler", () =>
            {
                bindHandler(noScores: noScores);
            });
        }

        [Test]
        public void TestShowWithUserScore()
        {
            SetUpRequestHandler();

            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

            createResults(() => userScore);
            waitForDisplay();

            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).State == PanelState.Expanded);
            AddAssert($"score panel position is {real_user_position}",
                () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).ScorePosition.Value == real_user_position);
        }

        [Test]
        public void TestShowNullUserScore()
        {
            SetUpRequestHandler();

            createResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestShowUserScoreWithDelay()
        {
            SetUpRequestHandler();

            AddStep("bind user score info handler", () => bindHandler(true, userScore));

            createResults(() => userScore);
            waitForDisplay();

            AddAssert("more than 1 panel displayed", () => this.ChildrenOfType<ScorePanel>().Count() > 1);
            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).State == PanelState.Expanded);
        }

        [Test]
        public void TestShowNullUserScoreWithDelay()
        {
            SetUpRequestHandler();

            AddStep("bind delayed handler", () => bindHandler(true));

            createResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestFetchWhenScrolledToTheRight()
        {
            SetUpRequestHandler();

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

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() >= beforePanelCount + scores_per_result);
                AddAssert("right loading spinner hidden", () => resultsScreen.RightSpinner.State.Value == Visibility.Hidden);
            }
        }

        [Test]
        public void TestFetchWhenScrolledToTheLeft()
        {
            SetUpRequestHandler();

            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

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

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() >= beforePanelCount + scores_per_result);
                AddAssert("left loading spinner hidden", () => resultsScreen.LeftSpinner.State.Value == Visibility.Hidden);
            }
        }

        [Test]
        public void TestShowWithNoScores()
        {
            SetUpRequestHandler(true);
            createResults();
            AddAssert("no scores visible", () => resultsScreen.ScorePanelList.GetScorePanels().Count() == 0);
        }

        private void createResults(Func<ScoreInfo> getScore = null)
        {
            AddStep("load results", () =>
            {
                LoadScreen(resultsScreen = new TestResultsScreen(getScore?.Invoke(), 1, new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                }));
            });

            AddUntilStep("wait for screen to load", () => resultsScreen.IsLoaded);
        }

        private void waitForDisplay()
        {
            AddUntilStep("wait for scores loaded", () =>
                requestComplete
                // request handler may need to fire more than once to get scores.
                && totalCount > 0
                && resultsScreen.ScorePanelList.GetScorePanels().Count() == totalCount
                && resultsScreen.ScorePanelList.AllPanelsVisible);
            AddWaitStep("wait for display", 5);
        }

        private void bindHandler(bool delayed = false, ScoreInfo userScore = null, bool failRequests = false, bool noScores = false) => ((DummyAPIAccess)API).HandleRequest = request =>
        {
            // pre-check for requests we should be handling (as they are scheduled below).
            switch (request)
            {
                case ShowPlaylistUserScoreRequest:
                case IndexPlaylistScoresRequest:
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
                        triggerSuccess(i, createIndexResponse(i, noScores));
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
                ID = highestScoreId,
                Accuracy = userScore.Accuracy,
                Passed = userScore.Passed,
                Rank = userScore.Rank,
                Position = real_user_position,
                MaxCombo = userScore.MaxCombo,
                User = userScore.User,
                ScoresAround = new MultiplayerScoresAround
                {
                    Higher = new MultiplayerScores(),
                    Lower = new MultiplayerScores()
                }
            };

            totalCount++;

            for (int i = 1; i <= scores_per_result; i++)
            {
                multiplayerUserScore.ScoresAround.Lower.Scores.Add(new MultiplayerScore
                {
                    ID = getNextLowestScoreId(),
                    Accuracy = userScore.Accuracy,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    User = new APIUser
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                multiplayerUserScore.ScoresAround.Higher.Scores.Add(new MultiplayerScore
                {
                    ID = getNextHighestScoreId(),
                    Accuracy = userScore.Accuracy,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    User = new APIUser
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                totalCount += 2;
            }

            addCursor(multiplayerUserScore.ScoresAround.Lower);
            addCursor(multiplayerUserScore.ScoresAround.Higher);

            return multiplayerUserScore;
        }

        private IndexedMultiplayerScores createIndexResponse(IndexPlaylistScoresRequest req, bool noScores = false)
        {
            var result = new IndexedMultiplayerScores();

            if (noScores) return result;

            string sort = req.IndexParams?.Properties["sort"].ToObject<string>() ?? "score_desc";

            for (int i = 1; i <= scores_per_result; i++)
            {
                result.Scores.Add(new MultiplayerScore
                {
                    ID = sort == "score_asc" ? getNextHighestScoreId() : getNextLowestScoreId(),
                    Accuracy = 1,
                    Passed = true,
                    Rank = ScoreRank.X,
                    MaxCombo = 1000,
                    User = new APIUser
                    {
                        Id = 2,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                totalCount++;
            }

            addCursor(result);

            return result;
        }

        /// <summary>
        /// The next highest score ID to appear at the left of the list. Monotonically decreasing.
        /// </summary>
        private int getNextHighestScoreId() => --highestScoreId;

        /// <summary>
        /// The next lowest score ID to appear at the right of the list. Monotonically increasing.
        /// </summary>
        /// <returns></returns>
        private int getNextLowestScoreId() => ++lowestScoreId;

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
                    // [ 1, 2, 3, ... ] => score_desc (will be added to the right of the list)
                    // [ 3, 2, 1, ... ] => score_asc (will be added to the left of the list)
                    { "sort", JToken.FromObject(scores.Scores[^1].ID > scores.Scores[^2].ID ? "score_desc" : "score_asc") }
                }
            };
        }

        private partial class TestResultsScreen : PlaylistsResultsScreen
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
