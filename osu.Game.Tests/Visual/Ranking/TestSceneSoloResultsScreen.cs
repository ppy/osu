// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneSoloResultsScreen : ScreenTestScene
    {
        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;

        private LeaderboardManager leaderboardManager = null!;
        private BeatmapInfo importedBeatmap = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, Realm, API));
            dependencies.Cache(leaderboardManager = new LeaderboardManager());

            Dependencies.Cache(Realm);

            return dependencies;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load leaderboard manager", () => LoadComponent(leaderboardManager));

            AddStep(@"set beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                Realm.Write(r =>
                {
                    foreach (var set in r.All<BeatmapSetInfo>())
                        set.Status = BeatmapOnlineStatus.Ranked;

                    foreach (var b in r.All<BeatmapInfo>())
                        b.Status = BeatmapOnlineStatus.Ranked;
                });
                importedBeatmap = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();
            });
            AddStep("clear all scores", () => Realm.Write(r => r.RemoveAll<ScoreInfo>()));
        }

        [Test]
        public void TestLocalLeaderboardWithOfflineScore()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to local", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Local, null)));
            AddStep("import some local scores", () =>
            {
                for (int i = 0; i < 30; ++i)
                {
                    var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                    score.TotalScore = 10_000 * (30 - i);
                    scoreManager.Import(score);
                }

                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                scoreManager.Import(localScore);
                localScore = localScore.Detach();
            });

            AddStep("show results", () => LoadScreen(new SoloResultsScreen(localScore)));
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #16", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(16));
        }

        [Test]
        public void TestLocalLeaderboardWithOnlineScore()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to local", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Local, null)));
            AddStep("import some local scores", () =>
            {
                for (int i = 0; i < 30; ++i)
                {
                    var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                    score.OnlineID = i;
                    score.TotalScore = 10_000 * (30 - i);
                    scoreManager.Import(score);
                }

                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.OnlineID = 30;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                scoreManager.Import(localScore);
                localScore = localScore.Detach();
            });

            AddStep("show results", () => LoadScreen(new SoloResultsScreen(localScore)));
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #16", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(16));
        }

        [Test]
        public void TestOnlineLeaderboardWithLessThan50Scores()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 30; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 10_000 * (30 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        getScoresRequest.TriggerSuccess(new APIScoresCollection { Scores = scores });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #16", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(16));
        }

        [Test]
        public void TestOnlineLeaderboardWithLessThan50Scores_UserWasInTop50()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 30; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 10_000 * (30 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        scores[^1].ID = 123456;
                        scores[^1].UserID = API.LocalUser.Value.OnlineID;

                        getScoresRequest.TriggerSuccess(new APIScoresCollection
                        {
                            Scores = scores,
                            UserScore = new APIScoreWithPosition
                            {
                                Score = scores[^1],
                                Position = 30
                            }
                        });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #16", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(16));
            AddAssert("previous user best not shown", () => this.ChildrenOfType<ScorePanel>().All(p => p.Score.OnlineID != 123456));
        }

        [Test]
        public void TestOnlineLeaderboardWithLessThan50Scores_ShowingAnotherUserScore()
        {
            var scores = new List<ScoreInfo>();
            var soloScores = new List<SoloScoreInfo>();

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () =>
            {
                for (int i = 0; i < 30; ++i)
                {
                    var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                    score.TotalScore = 10_000 * (30 - i);
                    score.Position = i + 1;
                    score.User = new APIUser { Id = i };
                    score.BeatmapInfo = new BeatmapInfo
                    {
                        OnlineID = 123123,
                        Status = BeatmapOnlineStatus.Ranked,
                    };
                    score.OnlineID = i;
                    scores.Add(score);

                    var soloScore = SoloScoreInfo.ForSubmission(score);
                    soloScore.ID = (ulong)i;
                    soloScores.Add(soloScore);
                }

                scores[^1].User = API.LocalUser.Value;
                soloScores[^1].UserID = API.LocalUser.Value.OnlineID;

                dummyAPI.HandleRequest = req =>
                {
                    switch (req)
                    {
                        case GetScoresRequest getScoresRequest:
                            getScoresRequest.TriggerSuccess(new APIScoresCollection
                            {
                                Scores = soloScores,
                                UserScore = new APIScoreWithPosition
                                {
                                    Score = soloScores[^1],
                                    Position = 30
                                }
                            });
                            return true;
                    }

                    return false;
                };
            });

            AddStep("show results", () => LoadScreen(new SoloResultsScreen(scores[0])));
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddAssert("local user best shown", () => this.ChildrenOfType<ScorePanel>().Any(p => p.Score.UserID == API.LocalUser.Value.Id));
        }

        [Test]
        public void TestOnlineLeaderboardWithLessThan50Scores_UserIsLast()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 30; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 300_000 + 10_000 * (30 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        getScoresRequest.TriggerSuccess(new APIScoresCollection { Scores = scores });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #31", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(31));
        }

        [Test]
        public void TestOnlineLeaderboardWithMoreThan50Scores_UserOutsideOfTop50_DidNotBeatOwnBest()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 50; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 500_000 + 10_000 * (50 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        var userBest = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo(importedBeatmap));
                        userBest.TotalScore = 50_000;
                        userBest.ID = 123456;

                        getScoresRequest.TriggerSuccess(new APIScoresCollection
                        {
                            Scores = scores,
                            UserScore = new APIScoreWithPosition
                            {
                                Score = userBest,
                                Position = 133_337,
                            },
                            ScoresCount = 200_000,
                        });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 31_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddAssert("local score has no position", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.Null);
            AddUntilStep("previous user best shown at same position", () => this.ChildrenOfType<ScorePanel>().Any(p => p.Score.OnlineID == 123456 && p.ScorePosition.Value == 133_337));
        }

        [Test]
        public void TestOnlineLeaderboardWithMoreThan50Scores_UserOutsideOfTop50_BeatOwnBest()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 50; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 500_000 + 10_000 * (50 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        var userBest = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo(importedBeatmap));
                        userBest.TotalScore = 50_000;
                        userBest.ID = 123456;
                        userBest.UserID = API.LocalUser.Value.OnlineID;

                        getScoresRequest.TriggerSuccess(new APIScoresCollection
                        {
                            Scores = scores,
                            UserScore = new APIScoreWithPosition
                            {
                                Score = userBest,
                                Position = 133_337,
                            },
                            ScoresCount = 200_000,
                        });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddAssert("local score has no position", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.Null);
            AddAssert("previous user best not shown", () => this.ChildrenOfType<ScorePanel>().All(p => p.Score.OnlineID != 123456));
        }

        [Test]
        public void TestOnlineLeaderboardWithMoreThan50Scores_UserInTop50()
        {
            ScoreInfo localScore = null!;

            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 50; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 500_000 + 10_000 * (50 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        var userBest = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo(importedBeatmap));
                        userBest.TotalScore = 50_000;
                        userBest.ID = 123456;
                        userBest.UserID = API.LocalUser.Value.OnlineID;

                        getScoresRequest.TriggerSuccess(new APIScoresCollection
                        {
                            Scores = scores,
                            UserScore = new APIScoreWithPosition
                            {
                                Score = userBest,
                                Position = 133_337,
                            }
                        });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 651_000;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddUntilStep("local score is #36", () => this.ChildrenOfType<ScorePanelList>().Single().GetPanelForScore(localScore).ScorePosition.Value, () => Is.EqualTo(36));
            AddAssert("previous user best not shown", () => this.ChildrenOfType<ScorePanel>().All(p => p.Score.OnlineID != 123456));
        }

        [Test]
        public void TestOnlineLeaderboardDeduplication()
        {
            AddStep("set leaderboard to global", () => leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(importedBeatmap, importedBeatmap.Ruleset, BeatmapLeaderboardScope.Global, null)));
            AddStep("set up request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetScoresRequest getScoresRequest:
                        var scores = new List<SoloScoreInfo>();

                        for (int i = 0; i < 50; ++i)
                        {
                            var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                            score.TotalScore = 500_000 + 10_000 * (50 - i);
                            score.Position = i + 1;
                            scores.Add(SoloScoreInfo.ForSubmission(score));
                        }

                        var userBest = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo(importedBeatmap));
                        userBest.TotalScore = 151_000;
                        userBest.ID = 12345;

                        getScoresRequest.TriggerSuccess(new APIScoresCollection
                        {
                            Scores = scores,
                            UserScore = new APIScoreWithPosition
                            {
                                Score = userBest,
                                Position = 133_337,
                            },
                            ScoresCount = 200_000,
                        });
                        return true;
                }

                return false;
            });

            AddStep("show results", () =>
            {
                var localScore = TestResources.CreateTestScoreInfo(importedBeatmap);
                localScore.TotalScore = 151_000;
                localScore.OnlineID = 12345;
                localScore.Position = null;
                localScore.User = API.LocalUser.Value;
                LoadScreen(new SoloResultsScreen(localScore));
            });
            AddUntilStep("wait for loaded", () => ((Drawable)Stack.CurrentScreen).IsLoaded);
            AddAssert("only one score with ID 12345", () => this.ChildrenOfType<ScorePanel>().Count(s => s.Score.OnlineID == 12345), () => Is.EqualTo(1));
            AddUntilStep("user best position preserved", () => this.ChildrenOfType<ScorePanel>().Any(p => p.ScorePosition.Value == 133_337));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesetStore.IsNotNull())
                rulesetStore.Dispose();
        }
    }
}
