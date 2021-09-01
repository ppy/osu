// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneBeatmapLeaderboard : OsuTestScene
    {
        private readonly FailableLeaderboard leaderboard;

        [Cached]
        private readonly DialogOverlay dialogOverlay;

        private ScoreManager scoreManager;

        private RulesetStore rulesetStore;
        private BeatmapManager beatmapManager;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RulesetStore(ContextFactory));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, ContextFactory, rulesetStore, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, null, ContextFactory, Scheduler));

            return dependencies;
        }

        public TestSceneBeatmapLeaderboard()
        {
            AddRange(new Drawable[]
            {
                dialogOverlay = new DialogOverlay
                {
                    Depth = -1
                },
                leaderboard = new FailableLeaderboard
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(550f, 450f),
                    Scope = BeatmapLeaderboardScope.Global,
                }
            });
        }

        [Test]
        public void TestLocalScoresDisplay()
        {
            BeatmapInfo beatmapInfo = null;

            AddStep(@"Set scope", () => leaderboard.Scope = BeatmapLeaderboardScope.Local);

            AddStep(@"Set beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                leaderboard.Beatmap = beatmapInfo;
            });

            clearScores();
            checkCount(0);

            loadMoreScores(() => beatmapInfo);
            checkCount(10);

            loadMoreScores(() => beatmapInfo);
            checkCount(20);

            clearScores();
            checkCount(0);
        }

        [Test]
        public void TestGlobalScoresDisplay()
        {
            AddStep(@"Set scope", () => leaderboard.Scope = BeatmapLeaderboardScope.Global);
            AddStep(@"New Scores", () => leaderboard.Scores = generateSampleScores(null));
        }

        [Test]
        public void TestPersonalBest()
        {
            AddStep(@"Show personal best", showPersonalBest);
            AddStep("null personal best position", showPersonalBestWithNullPosition);
        }

        [Test]
        public void TestPlaceholderStates()
        {
            AddStep(@"Empty Scores", () => leaderboard.SetRetrievalState(PlaceholderState.NoScores));
            AddStep(@"Network failure", () => leaderboard.SetRetrievalState(PlaceholderState.NetworkFailure));
            AddStep(@"No supporter", () => leaderboard.SetRetrievalState(PlaceholderState.NotSupporter));
            AddStep(@"Not logged in", () => leaderboard.SetRetrievalState(PlaceholderState.NotLoggedIn));
            AddStep(@"Unavailable", () => leaderboard.SetRetrievalState(PlaceholderState.Unavailable));
            AddStep(@"None selected", () => leaderboard.SetRetrievalState(PlaceholderState.NoneSelected));
        }

        [Test]
        public void TestBeatmapStates()
        {
            foreach (BeatmapSetOnlineStatus status in Enum.GetValues(typeof(BeatmapSetOnlineStatus)))
                AddStep($"{status} beatmap", () => showBeatmapWithStatus(status));
        }

        private void showPersonalBestWithNullPosition()
        {
            leaderboard.TopScore = new ScoreInfo
            {
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                User = new User
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    Country = new Country
                    {
                        FullName = @"Spain",
                        FlagName = @"ES",
                    },
                },
            };
        }

        private void showPersonalBest()
        {
            leaderboard.TopScore = new ScoreInfo
            {
                Position = 999,
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                User = new User
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    Country = new Country
                    {
                        FullName = @"Spain",
                        FlagName = @"ES",
                    },
                },
            };
        }

        private void loadMoreScores(Func<BeatmapInfo> beatmapInfo)
        {
            AddStep(@"Load new scores via manager", () =>
            {
                foreach (var score in generateSampleScores(beatmapInfo()))
                    scoreManager.Import(score).Wait();
            });
        }

        private void clearScores()
        {
            AddStep("Clear all scores", () => scoreManager.Delete(scoreManager.GetAllUsableScores()));
        }

        private void checkCount(int expected) =>
            AddUntilStep("Correct count displayed", () => leaderboard.ChildrenOfType<LeaderboardScore>().Count() == expected);

        private static ScoreInfo[] generateSampleScores(BeatmapInfo beatmap)
        {
            return new[]
            {
                new ScoreInfo
                {
                    Rank = ScoreRank.XH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        Country = new Country
                        {
                            FullName = @"Spain",
                            FlagName = @"ES",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 4608074,
                        Username = @"Skycries",
                        Country = new Country
                        {
                            FullName = @"Brazil",
                            FlagName = @"BR",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.SH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 1014222,
                        Username = @"eLy",
                        Country = new Country
                        {
                            FullName = @"Japan",
                            FlagName = @"JP",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.S,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        Country = new Country
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.A,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 2243452,
                        Username = @"Satoruu",
                        Country = new Country
                        {
                            FullName = @"Venezuela",
                            FlagName = @"VE",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.B,
                    Accuracy = 0.9826,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 2705430,
                        Username = @"Mooha",
                        Country = new Country
                        {
                            FullName = @"France",
                            FlagName = @"FR",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.C,
                    Accuracy = 0.9654,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 7151382,
                        Username = @"Mayuri Hana",
                        Country = new Country
                        {
                            FullName = @"Thailand",
                            FlagName = @"TH",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.6025,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 2051389,
                        Username = @"FunOrange",
                        Country = new Country
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.5140,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 6169483,
                        Username = @"-Hebel-",
                        Country = new Country
                        {
                            FullName = @"Mexico",
                            FlagName = @"MX",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.4222,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Beatmap = beatmap,
                    User = new User
                    {
                        Id = 6702666,
                        Username = @"prhtnsm",
                        Country = new Country
                        {
                            FullName = @"Germany",
                            FlagName = @"DE",
                        },
                    },
                },
            };
        }

        private void showBeatmapWithStatus(BeatmapSetOnlineStatus status)
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = status,
            };
        }

        private class FailableLeaderboard : BeatmapLeaderboard
        {
            public void SetRetrievalState(PlaceholderState state)
            {
                PlaceholderState = state;
            }
        }
    }
}
