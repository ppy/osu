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
using osu.Framework.Extensions.ObjectExtensions;
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
using osu.Game.Tests.Visual.SongSelect;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapLeaderboardWedge : SongSelectComponentsTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private TestBeatmapLeaderboardWedge leaderboard = null!;
        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;
        private OsuContextMenuContainer contentContainer = null!;
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

            Child = contentContainer = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 500,
                Children = new Drawable[]
                {
                    dialogOverlay,
                }
            };

            AddSliderStep("change relative height", 0f, 1f, 0.65f, v => Schedule(() =>
            {
                contentContainer.Height = v * DrawHeight;
            }));
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            if (leaderboard.IsNotNull())
                contentContainer.Remove(leaderboard, false);

            contentContainer.Add(leaderboard = new TestBeatmapLeaderboardWedge
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
            });
        });

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
        }

        [Test]
        public void TestPersonalBest()
        {
            AddStep(@"Show personal best", showPersonalBest);
        }

        [Test]
        public void TestGlobalScoresDisplay()
        {
            setScope(BeatmapLeaderboardScope.Global);

            AddStep(@"New Scores", () => leaderboard.SetScores(TestSceneBeatmapLeaderboard.GenerateSampleScores(new BeatmapInfo())));
            AddStep(@"New Scores with teams", () => leaderboard.SetScores(TestSceneBeatmapLeaderboard.GenerateSampleScores(new BeatmapInfo()).Select(s =>
            {
                s.User.Team = new APITeam();
                return s;
            })));
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

            AddStep(@"Retrieving", () => leaderboard.SetState(LeaderboardState.Retrieving));
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
            AddStep(@"set scores", () => leaderboard.SetScores(TestSceneBeatmapLeaderboard.GenerateSampleScores(new BeatmapInfo()), new ScoreInfo
            {
                OnlineID = 1337,
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
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableOsuMenuItem>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("received HD", () => this.ChildrenOfType<BeatmapLeaderboardScore>().Last().SelectedMods.Value.Any(m => m is OsuModHidden));
            AddAssert("did not receive SV2", () => !this.ChildrenOfType<BeatmapLeaderboardScore>().Last().SelectedMods.Value.Any(m => m is ModScoreV2));
        }

        [Test]
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
        public void TestLocalScoresDisplayWorksWhenStartingOffline()
        {
            BeatmapInfo beatmapInfo = null!;

            AddStep("Log out", () => API.Logout());
            setScope(BeatmapLeaderboardScope.Local);

            AddStep(@"Import beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);
            });

            clearScores();
            importMoreScores(() => beatmapInfo);
            checkDisplayedCount(10);
        }

        [Test]
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
            leaderboard.SetScores(TestSceneBeatmapLeaderboard.GenerateSampleScores(new BeatmapInfo()), new ScoreInfo
            {
                OnlineID = 1337,
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
                Date = DateTimeOffset.Now,
            }, 1234567);
        }

        private void showPersonalBest()
        {
            leaderboard.SetScores(TestSceneBeatmapLeaderboard.GenerateSampleScores(new BeatmapInfo()), new ScoreInfo
            {
                OnlineID = 1337,
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
                },
                Date = DateTimeOffset.Now,
            }, 1234567);
        }

        private void setScope(BeatmapLeaderboardScope scope)
        {
            AddStep(@"Set scope", () => ((Bindable<BeatmapLeaderboardScope>)leaderboard.Scope).Value = scope);
        }

        private void importMoreScores(Func<BeatmapInfo> beatmapInfo)
        {
            AddStep(@"Import new scores", () =>
            {
                foreach (var score in TestSceneBeatmapLeaderboard.GenerateSampleScores(beatmapInfo()))
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

        private partial class TestBeatmapLeaderboardWedge : BeatmapLeaderboardWedge
        {
            public new void SetState(LeaderboardState state) => base.SetState(state);
            public new void SetScores(IEnumerable<ScoreInfo> scores, ScoreInfo? userScore = null, int? totalCount = null) => base.SetScores(scores, userScore, totalCount);
        }
    }
}
