// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Placeholders;
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

        [Cached]
        private readonly BeatmapLookupCache beatmapLookupCache = new BeatmapLookupCache();

        private ResultsScreen resultsScreen = null!;

        private int lowestScoreId; // Score ID of the lowest score in the list.
        private int highestScoreId; // Score ID of the highest score in the list.

        private bool requestComplete;
        private int totalCount;
        private ScoreInfo userScore = null!;

        public TestScenePlaylistsResultsScreen()
        {
            Add(beatmapLookupCache);
        }

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
                userScore.OnlineID = 1;
                userScore.TotalScore = 0;
                userScore.Statistics = new Dictionary<HitResult, int>();
                userScore.MaximumStatistics = new Dictionary<HitResult, int>();
                userScore.Position = real_user_position;

                // Beatmap is required to be an actual beatmap so the scores can get their scores correctly
                // calculated for standardised scoring, else the tests that rely on ordering will fall over.
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
            });
        }

        [Test]
        public void TestShowUserScore()
        {
            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

            createResultsWithScore(() => userScore);
            waitForDisplay();

            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).State == PanelState.Expanded);
            AddAssert($"score panel position is {real_user_position}",
                () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).ScorePosition.Value == real_user_position);
        }

        [Test]
        public void TestShowUserBest()
        {
            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

            createUserBestResults();
            waitForDisplay();

            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.UserID == userScore.UserID).State == PanelState.Expanded);
            AddAssert($"score panel position is {real_user_position}",
                () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.UserID == userScore.UserID).ScorePosition.Value == real_user_position);
        }

        [Test]
        public void TestShowNonUserScores()
        {
            AddStep("bind user score info handler", () => bindHandler());

            createUserBestResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestShowUserScoreWithDelay()
        {
            AddStep("bind user score info handler", () => bindHandler(true, userScore));

            createResultsWithScore(() => userScore);
            waitForDisplay();

            AddAssert("more than 1 panel displayed", () => this.ChildrenOfType<ScorePanel>().Count() > 1);
            AddAssert("user score selected", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.OnlineID == userScore.OnlineID).State == PanelState.Expanded);
        }

        [Test]
        public void TestShowNonUserScoresWithDelay()
        {
            AddStep("bind delayed handler", () => bindHandler(true));

            createUserBestResults();
            waitForDisplay();

            AddAssert("top score selected", () => this.ChildrenOfType<ScorePanel>().OrderByDescending(p => p.Score.TotalScore).First().State == PanelState.Expanded);
        }

        [Test]
        public void TestFetchWhenScrolledToTheRight()
        {
            AddStep("bind delayed handler", () => bindHandler(true));

            createUserBestResults();
            waitForDisplay();

            for (int i = 0; i < 2; i++)
            {
                int beforePanelCount = 0;

                AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
                AddStep("scroll to right", () => resultsScreen.ChildrenOfType<ScorePanelList>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));

                AddUntilStep("right loading spinner shown", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Visible);

                waitForDisplay();

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount + scores_per_result);
                AddUntilStep("right loading spinner hidden", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Hidden);
            }
        }

        [Test]
        public void TestNoMoreScoresToTheRight()
        {
            AddStep("bind delayed handler with scores", () => bindHandler(delayed: true));

            createUserBestResults();
            waitForDisplay();

            int beforePanelCount = 0;

            AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
            AddStep("scroll to right", () => resultsScreen.ChildrenOfType<ScorePanelList>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));

            AddUntilStep("right loading spinner shown", () =>
                resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Visible);

            waitForDisplay();

            AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount + scores_per_result);
            AddUntilStep("right loading spinner hidden", () =>
                resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Hidden);

            AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
            AddStep("bind delayed handler with no scores", () => bindHandler(delayed: true, noScores: true));
            AddStep("scroll to right", () => resultsScreen.ChildrenOfType<ScorePanelList>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));

            AddUntilStep("right loading spinner shown", () =>
                resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Visible);

            waitForDisplay();

            AddAssert("count not increased", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount);
            AddUntilStep("right loading spinner hidden", () =>
                resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreRight).State.Value == Visibility.Hidden);

            AddAssert("no placeholders shown", () => this.ChildrenOfType<MessagePlaceholder>().Count(), () => Is.Zero);
        }

        [Test]
        public void TestFetchWhenScrolledToTheLeft()
        {
            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

            createResultsWithScore(() => userScore);
            waitForDisplay();

            AddStep("bind delayed handler", () => bindHandler(true));

            for (int i = 0; i < 2; i++)
            {
                int beforePanelCount = 0;

                AddStep("get panel count", () => beforePanelCount = this.ChildrenOfType<ScorePanel>().Count());
                AddStep("scroll to left", () => resultsScreen.ChildrenOfType<ScorePanelList>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToStart(false));

                AddUntilStep("left loading spinner shown", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreLeft).State.Value == Visibility.Visible);

                waitForDisplay();

                AddAssert($"count increased by {scores_per_result}", () => this.ChildrenOfType<ScorePanel>().Count() == beforePanelCount + scores_per_result);
                AddUntilStep("left loading spinner hidden", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreLeft).State.Value == Visibility.Hidden);
            }
        }

        /// <summary>
        /// Shows the <see cref="TestUserBestResultsScreen"/> with no scores provided by the API.
        /// </summary>
        [Test]
        public void TestShowUserBestWithNoScoresPresent()
        {
            AddStep("bind user score info handler", () => bindHandler(noScores: true));
            createUserBestResults();
            AddAssert("no scores visible", () => !resultsScreen.ChildrenOfType<ScorePanelList>().Single().GetScorePanels().Any());
            AddUntilStep("placeholder shown", () => this.ChildrenOfType<MessagePlaceholder>().Count(), () => Is.EqualTo(1));
        }

        [Test]
        public void TestFetchingAllTheWayToFirstNeverDisplaysNegativePosition()
        {
            AddStep("set user position", () => userScore.Position = 20);
            AddStep("bind user score info handler", () => bindHandler(userScore: userScore));

            createResultsWithScore(() => userScore);
            waitForDisplay();

            AddStep("bind delayed handler", () => bindHandler(true));

            for (int i = 0; i < 2; i++)
            {
                AddStep("simulate user falling down ranking", () => userScore.Position += 2);
                AddStep("scroll to left", () => resultsScreen.ChildrenOfType<ScorePanelList>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToStart(false));

                AddUntilStep("left loading spinner shown", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreLeft).State.Value == Visibility.Visible);

                waitForDisplay();

                AddUntilStep("left loading spinner hidden", () =>
                    resultsScreen.ChildrenOfType<LoadingSpinner>().Single(l => l.Anchor == Anchor.CentreLeft).State.Value == Visibility.Hidden);
            }

            AddAssert("total count is 34", () => this.ChildrenOfType<ScorePanel>().Count(), () => Is.EqualTo(34));
            AddUntilStep("all panels have non-negative position", () => this.ChildrenOfType<ScorePanel>().All(p => p.ScorePosition.Value > 0));
        }

        private void createResultsWithScore(Func<ScoreInfo> getScore)
        {
            AddStep("load results", () =>
            {
                LoadScreen(resultsScreen = new TestScoreResultsScreen(getScore(), 1, new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                }));
            });

            AddUntilStep("wait for screen to load", () => resultsScreen.IsLoaded);
        }

        private void createUserBestResults()
        {
            AddStep("load results", () =>
            {
                LoadScreen(resultsScreen = new TestUserBestResultsScreen(1, new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                }, 2));
            });

            AddUntilStep("wait for screen to load", () => resultsScreen.IsLoaded);
        }

        private void waitForDisplay()
        {
            AddUntilStep("wait for scores loaded", () =>
                requestComplete
                // request handler may need to fire more than once to get scores.
                && totalCount > 0
                && resultsScreen.ChildrenOfType<ScorePanelList>().Single().GetScorePanels().Count() == totalCount
                && resultsScreen.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);
            AddWaitStep("wait for display", 5);
        }

        private void bindHandler(bool delayed = false, ScoreInfo? userScore = null, bool failRequests = false, bool noScores = false) => ((DummyAPIAccess)API).HandleRequest = request =>
        {
            // pre-check for requests we should be handling (as they are scheduled below).
            switch (request)
            {
                case ShowPlaylistScoreRequest:
                case ShowPlaylistUserScoreRequest:
                case IndexPlaylistScoresRequest:
                    break;

                case GetBeatmapsRequest getBeatmaps:
                    getBeatmaps.TriggerSuccess(new GetBeatmapsResponse
                    {
                        Beatmaps = getBeatmaps.BeatmapIds.Select(id => new APIBeatmap
                        {
                            OnlineID = id,
                            StarRating = id,
                            DifficultyName = $"Beatmap {id}",
                            BeatmapSet = new APIBeatmapSet
                            {
                                Title = $"Title {id}",
                                Artist = $"Artist {id}",
                                AuthorString = $"Author {id}"
                            }
                        }).ToList()
                    });

                    return true;

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
                    case ShowPlaylistScoreRequest s:
                        if (userScore == null)
                            triggerFail(s);
                        else
                            triggerSuccess(s, () => createUserResponse(userScore));

                        break;

                    case ShowPlaylistUserScoreRequest u:
                        if (userScore == null)
                            triggerFail(u);
                        else
                            triggerSuccess(u, () => createUserResponse(userScore));

                        break;

                    case IndexPlaylistScoresRequest i:
                        triggerSuccess(i, () => createIndexResponse(i, noScores));
                        break;
                }
            }, delay);

            return true;
        };

        private void triggerSuccess<T>(APIRequest<T> req, Func<T> result)
            where T : class
        {
            requestComplete = true;
            req.TriggerSuccess(result.Invoke());
        }

        private void triggerFail(APIRequest req)
        {
            requestComplete = true;
            req.TriggerFailure(new WebException("Failed."));
        }

        private MultiplayerScore createUserResponse(ScoreInfo userScore)
        {
            var multiplayerUserScore = createMultiplayerUserScore(userScore);

            totalCount++;

            for (int i = 1; i <= scores_per_result; i++)
            {
                multiplayerUserScore.ScoresAround!.Lower!.Scores.Add(new MultiplayerScore
                {
                    ID = getNextLowestScoreId(),
                    Accuracy = userScore.Accuracy,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    BeatmapId = RNG.Next(0, 7),
                    User = new APIUser
                    {
                        Id = 2 + i,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                multiplayerUserScore.ScoresAround!.Higher!.Scores.Add(new MultiplayerScore
                {
                    ID = getNextHighestScoreId(),
                    Accuracy = userScore.Accuracy,
                    Passed = true,
                    Rank = userScore.Rank,
                    MaxCombo = userScore.MaxCombo,
                    BeatmapId = RNG.Next(0, 7),
                    User = new APIUser
                    {
                        Id = 2 + i,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                totalCount += 2;
            }

            addCursor(multiplayerUserScore.ScoresAround!.Lower!);
            addCursor(multiplayerUserScore.ScoresAround!.Higher!);

            return multiplayerUserScore;
        }

        private MultiplayerScore createMultiplayerUserScore(ScoreInfo userScore)
        {
            return new MultiplayerScore
            {
                ID = highestScoreId,
                Accuracy = userScore.Accuracy,
                Passed = userScore.Passed,
                Rank = userScore.Rank,
                Position = userScore.Position,
                MaxCombo = userScore.MaxCombo,
                User = userScore.User,
                BeatmapId = RNG.Next(0, 7),
                ScoresAround = new MultiplayerScoresAround
                {
                    Higher = new MultiplayerScores(),
                    Lower = new MultiplayerScores()
                }
            };
        }

        private IndexedMultiplayerScores createIndexResponse(IndexPlaylistScoresRequest req, bool noScores)
        {
            var result = new IndexedMultiplayerScores();

            if (noScores) return result;

            string sort = req.IndexParams?.Properties["sort"].ToObject<string>() ?? "score_desc";

            bool reachedEnd = false;

            for (int i = 1; i <= scores_per_result; i++)
            {
                int nextId = sort == "score_asc" ? getNextHighestScoreId() : getNextLowestScoreId();

                if (userScore.OnlineID - nextId >= userScore.Position)
                {
                    reachedEnd = true;
                    break;
                }

                result.Scores.Add(new MultiplayerScore
                {
                    ID = nextId,
                    Accuracy = 1,
                    Passed = true,
                    Rank = ScoreRank.X,
                    MaxCombo = 1000,
                    BeatmapId = RNG.Next(0, 7),
                    User = new APIUser
                    {
                        Id = 2 + i,
                        Username = $"peppy{i}",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                });

                totalCount++;
            }

            if (!reachedEnd)
                addCursor(result);

            result.UserScore = createMultiplayerUserScore(userScore);

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

        private partial class TestScoreResultsScreen : PlaylistItemScoreResultsScreen
        {
            public TestScoreResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem)
                : base(score, roomId, playlistItem)
            {
                AllowRetry = true;
            }
        }

        private partial class TestUserBestResultsScreen : PlaylistItemUserBestResultsScreen
        {
            public TestUserBestResultsScreen(int roomId, PlaylistItem playlistItem, int userId)
                : base(roomId, playlistItem, userId)
            {
                AllowRetry = true;
            }
        }
    }
}
