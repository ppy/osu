// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
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

            dependencies.Cache(rulesetStore = new RulesetStore(Realm));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, rulesetStore, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, Realm, Scheduler));
            Dependencies.Cache(Realm);

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
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                leaderboard.BeatmapInfo = beatmapInfo;
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
            AddStep(@"New Scores", () => leaderboard.SetScores(generateSampleScores(new BeatmapInfo())));
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
            AddStep("ensure no scores displayed", () => leaderboard.SetScores(null));

            AddStep(@"Network failure", () => leaderboard.SetErrorState(LeaderboardState.NetworkFailure));
            AddStep(@"No supporter", () => leaderboard.SetErrorState(LeaderboardState.NotSupporter));
            AddStep(@"Not logged in", () => leaderboard.SetErrorState(LeaderboardState.NotLoggedIn));
            AddStep(@"Unavailable", () => leaderboard.SetErrorState(LeaderboardState.Unavailable));
            AddStep(@"None selected", () => leaderboard.SetErrorState(LeaderboardState.NoneSelected));
        }

        private void showPersonalBestWithNullPosition()
        {
            leaderboard.SetScores(leaderboard.Scores, new ScoreInfo
            {
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                Ruleset = new OsuRuleset().RulesetInfo,
                User = new APIUser
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    Country = new Country
                    {
                        FullName = @"Spain",
                        FlagName = @"ES",
                    },
                },
            });
        }

        private void showPersonalBest()
        {
            leaderboard.SetScores(leaderboard.Scores, new ScoreInfo
            {
                Position = 999,
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Ruleset = new OsuRuleset().RulesetInfo,
                Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                User = new APIUser
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    Country = new Country
                    {
                        FullName = @"Spain",
                        FlagName = @"ES",
                    },
                },
            });
        }

        private void loadMoreScores(Func<BeatmapInfo> beatmapInfo)
        {
            AddStep(@"Load new scores via manager", () =>
            {
                foreach (var score in generateSampleScores(beatmapInfo()))
                    scoreManager.Import(score);
            });
        }

        private void clearScores()
        {
            AddStep("Clear all scores", () => scoreManager.Delete());
        }

        private void checkCount(int expected) =>
            AddUntilStep("Correct count displayed", () => leaderboard.ChildrenOfType<LeaderboardScore>().Count() == expected);

        private static ScoreInfo[] generateSampleScores(BeatmapInfo beatmapInfo)
        {
            return new[]
            {
                new ScoreInfo
                {
                    Rank = ScoreRank.XH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[]
                    {
                        new OsuModHidden(),
                        new OsuModHardRock(),
                        new OsuModFlashlight
                        {
                            FollowDelay = { Value = 200 },
                            SizeMultiplier = { Value = 5 },
                        },
                        new OsuModDifficultyAdjust
                        {
                            CircleSize = { Value = 11 },
                            ApproachRate = { Value = 10 },
                            OverallDifficulty = { Value = 10 },
                            DrainRate = { Value = 10 },
                            ExtendedLimits = { Value = true }
                        }
                    },
                    Ruleset = new OsuRuleset().RulesetInfo,
                    BeatmapInfo = beatmapInfo,
                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
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

        private class FailableLeaderboard : BeatmapLeaderboard
        {
            public new void SetErrorState(LeaderboardState state) => base.SetErrorState(state);
            public new void SetScores(IEnumerable<ScoreInfo> scores, ScoreInfo userScore = default) => base.SetScores(scores, userScore);
        }
    }
}
