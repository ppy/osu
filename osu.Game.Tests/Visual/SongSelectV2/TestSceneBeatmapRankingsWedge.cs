// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapRankingsWedge : SongSelectComponentsTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private TestBeatmapLeaderboardWedge leaderboard = null!;
        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;
        private OsuContextMenuContainer? contentContainer;
        private DialogOverlay dialogOverlay = null!;

        private LeaderboardManager leaderboardManager = null!;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponent(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            LoadComponent(leaderboardManager);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            contentContainer?.Remove(dialogOverlay, false);

            Child = contentContainer = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 700,
                Children = new Drawable[]
                {
                    dialogOverlay,
                    leaderboard = new TestBeatmapLeaderboardWedge
                    {
                        RelativeSizeAxes = Axes.Both,
                        State = { Value = Visibility.Visible },
                    }
                }
            };
        });

        [Test]
        public void TestGlobalScoresDisplay()
        {
            setScope(BeatmapLeaderboardScope.Global);

            AddStep(@"New Scores", () => leaderboard.SetScores(generateSampleScores(new BeatmapInfo())));
            AddStep(@"New Scores with teams", () => leaderboard.SetScores(generateSampleScores(new BeatmapInfo()).Select(s =>
            {
                s.User.Team = new APITeam();
                return s;
            })));
        }

        [Test]
        public void TestPersonalBest()
        {
            AddStep(@"Show personal best", showPersonalBest);
        }

        [Test]
        public void TestPersonalBestWithNullPosition()
        {
            AddStep("null personal best position", showPersonalBestWithNullPosition);
        }

        [Test]
        public void TestPlaceholderStates()
        {
            AddStep("ensure no scores displayed", () => leaderboard.SetScores(Array.Empty<ScoreInfo>()));

            AddStep(@"Network failure", () => leaderboard.SetState(LeaderboardState.NetworkFailure));
            AddStep(@"No team", () => leaderboard.SetState(LeaderboardState.NoTeam));
            AddStep(@"No supporter", () => leaderboard.SetState(LeaderboardState.NotSupporter));
            AddStep(@"Not logged in", () => leaderboard.SetState(LeaderboardState.NotLoggedIn));
            AddStep(@"Ruleset unavailable", () => leaderboard.SetState(LeaderboardState.RulesetUnavailable));
            AddStep(@"Beatmap unavailable", () => leaderboard.SetState(LeaderboardState.BeatmapUnavailable));
            AddStep(@"None selected", () => leaderboard.SetState(LeaderboardState.NoneSelected));
        }

        [Test]
        public void TestUseTheseModsDoesNotCopySystemMods()
        {
            AddStep(@"set scores", () => leaderboard.SetScores(generateSampleScores(new BeatmapInfo()), new ScoreInfo
            {
                Position = 999,
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Ruleset = new OsuRuleset().RulesetInfo,
                Mods = new Mod[] { new OsuModHidden(), new ModScoreV2(), },
                User = new APIUser
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    CountryCode = CountryCode.ES,
                }
            }));
            AddUntilStep("wait for scores", () => this.ChildrenOfType<BeatmapLeaderboardScore>().Count(), () => Is.GreaterThan(0));
            AddStep("right click panel", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<BeatmapLeaderboardScore>().Last());
                InputManager.Click(MouseButton.Right);
            });
            AddStep("click use these mods", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableOsuMenuItem>().Last());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("received HD", () => this.ChildrenOfType<BeatmapLeaderboardScore>().Last().SelectedMods.Value.Any(m => m is OsuModHidden));
            AddAssert("did not receive SV2", () => !this.ChildrenOfType<BeatmapLeaderboardScore>().Last().SelectedMods.Value.Any(m => m is ModScoreV2));
        }

        [Test]
        [Ignore("Pending implementation")]
        public void TestLocalScoresDisplay()
        {
            BeatmapInfo beatmapInfo = null!;

            setScope(BeatmapLeaderboardScope.Local);

            AddStep(@"Set beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);
            });

            clearScores();
            checkDisplayedCount(0);

            importMoreScores(() => beatmapInfo);
            checkDisplayedCount(10);

            importMoreScores(() => beatmapInfo);
            checkDisplayedCount(20);

            clearScores();
            checkDisplayedCount(0);
        }

        [Test]
        [Ignore("Pending implementation")]
        public void TestLocalScoresDisplayOnBeatmapEdit()
        {
            BeatmapInfo beatmapInfo = null!;
            string originalHash = string.Empty;

            setScope(BeatmapLeaderboardScope.Local);

            AddStep(@"Import beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);
            });

            clearScores();
            checkDisplayedCount(0);

            AddStep(@"Perform initial save to guarantee stable hash", () =>
            {
                IBeatmap beatmap = beatmapManager.GetWorkingBeatmap(beatmapInfo).Beatmap;
                beatmapManager.Save(beatmapInfo, beatmap);

                originalHash = beatmapInfo.Hash;
            });

            importMoreScores(() => beatmapInfo);

            checkDisplayedCount(10);
            checkStoredCount(10);

            AddStep(@"Save with changes", () =>
            {
                IBeatmap beatmap = beatmapManager.GetWorkingBeatmap(beatmapInfo).Beatmap;
                beatmap.Difficulty.ApproachRate = 12;
                beatmapManager.Save(beatmapInfo, beatmap);
            });

            AddAssert("Hash changed", () => beatmapInfo.Hash, () => Is.Not.EqualTo(originalHash));
            checkDisplayedCount(0);
            checkStoredCount(10);

            importMoreScores(() => beatmapInfo);
            importMoreScores(() => beatmapInfo);
            checkDisplayedCount(20);
            checkStoredCount(30);

            AddStep(@"Revert changes", () =>
            {
                IBeatmap beatmap = beatmapManager.GetWorkingBeatmap(beatmapInfo).Beatmap;
                beatmap.Difficulty.ApproachRate = 8;
                beatmapManager.Save(beatmapInfo, beatmap);
            });

            AddAssert("Hash restored", () => beatmapInfo.Hash, () => Is.EqualTo(originalHash));
            checkDisplayedCount(10);
            checkStoredCount(30);

            clearScores();
            checkDisplayedCount(0);
            checkStoredCount(0);
        }

        private void showPersonalBestWithNullPosition()
        {
            leaderboard.SetScores(generateSampleScores(new BeatmapInfo()), new ScoreInfo
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
                    CountryCode = CountryCode.ES,
                },
            });
        }

        private void showPersonalBest()
        {
            leaderboard.SetScores(generateSampleScores(new BeatmapInfo()), new ScoreInfo
            {
                Position = 999,
                Rank = ScoreRank.XH,
                Accuracy = 1,
                MaxCombo = 244,
                TotalScore = 1707827,
                Ruleset = new OsuRuleset().RulesetInfo,
                Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                User = new APIUser
                {
                    Id = 6602580,
                    Username = @"waaiiru",
                    CountryCode = CountryCode.ES,
                }
            });
        }

        private void setScope(BeatmapLeaderboardScope scope)
        {
            AddStep(@"Set scope", () => ((Bindable<BeatmapLeaderboardScope>)leaderboard.Scope).Value = scope);
        }

        private void importMoreScores(Func<BeatmapInfo> beatmapInfo)
        {
            AddStep(@"Import new scores", () =>
            {
                foreach (var score in generateSampleScores(beatmapInfo()))
                    scoreManager.Import(score);
            });
        }

        private void clearScores()
        {
            AddStep("Clear all scores", () => scoreManager.Delete());
        }

        private void checkDisplayedCount(int expected) =>
            AddUntilStep($"{expected} scores displayed", () => leaderboard.ChildrenOfType<BeatmapLeaderboardScore>().Count(), () => Is.EqualTo(expected));

        private void checkStoredCount(int expected) =>
            AddUntilStep($"Total scores stored is {expected}", () => Realm.Run(r => r.All<ScoreInfo>().Count(s => !s.DeletePending)), () => Is.EqualTo(expected));

        private static int scoreID = 1001;

        private static ScoreInfo[] generateSampleScores(BeatmapInfo beatmapInfo)
        {
            return new[]
            {
                new ScoreInfo
                {
                    Rank = ScoreRank.XH,
                    OnlineID = scoreID++,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now,
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
                    BeatmapHash = beatmapInfo.Hash,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        CountryCode = CountryCode.ES,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.X,
                    OnlineID = scoreID++,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddSeconds(-30),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 4608074,
                        Username = @"Skycries",
                        CountryCode = CountryCode.BR,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.SH,
                    OnlineID = scoreID++,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddSeconds(-70),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 1014222,
                        Username = @"eLy",
                        CountryCode = CountryCode.JP,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.S,
                    OnlineID = scoreID++,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddMinutes(-40),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        CountryCode = CountryCode.CA,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.A,
                    OnlineID = scoreID++,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddHours(-2),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 2243452,
                        Username = @"Satoruu",
                        CountryCode = CountryCode.VE,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.B,
                    OnlineID = scoreID++,
                    Accuracy = 0.9826,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddHours(-25),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 2705430,
                        Username = @"Mooha",
                        CountryCode = CountryCode.FR,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.C,
                    OnlineID = scoreID++,
                    Accuracy = 0.9654,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddHours(-50),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 7151382,
                        Username = @"Mayuri Hana",
                        CountryCode = CountryCode.TH,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    OnlineID = scoreID++,
                    Accuracy = 0.6025,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddHours(-72),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 2051389,
                        Username = @"FunOrange",
                        CountryCode = CountryCode.CA,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    OnlineID = scoreID++,
                    Accuracy = 0.5140,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddMonths(-10),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 6169483,
                        Username = @"-Hebel-",
                        CountryCode = CountryCode.MX,
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    OnlineID = scoreID++,
                    Accuracy = 0.4222,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Date = DateTime.Now.AddYears(-2),
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    BeatmapInfo = beatmapInfo,
                    BeatmapHash = beatmapInfo.Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,

                    User = new APIUser
                    {
                        Id = 6702666,
                        Username = @"prhtnsm",
                        CountryCode = CountryCode.DE,
                    },
                },
            };
        }

        private partial class TestBeatmapLeaderboardWedge : BeatmapLeaderboardWedge
        {
            public new void SetState(LeaderboardState state) => base.SetState(state);
            public new void SetScores(IEnumerable<ScoreInfo> scores, ScoreInfo? userScore = null) => base.SetScores(scores, userScore);
        }
    }
}
