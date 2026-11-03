// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneOfflineUsernameInput : OsuManualInputManagerTestScene
    {
        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;

        private BeatmapInfo importedBeatmap = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, Realm, API));

            Dependencies.Cache(Realm);

            return dependencies;
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("import beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedBeatmap = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();
            });
            AddStep("clear all scores", () => Realm.Write(r => r.RemoveAll<ScoreInfo>()));
            AddStep("set API offline", () => dummyAPI.SetState(APIState.Offline));
        }

        [Test]
        public void TestVisibleForOfflineGuestAndCommitOnEnter()
        {
            TestResultsScreen screen = null!;
            Guid dbScoreId = Guid.Empty;

            AddStep("create and import guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                dbScoreId = live.Value.ID;
            });
            AddStep("show results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!).Detach()));
            AddUntilStep("results loaded", () => screen.IsLoaded);

            OsuTextBox textBox = null!;
            AddUntilStep("input visible", () => this.ChildrenOfType<OfflineUsernameInput>().Single().Alpha, () => Is.EqualTo(1));
            AddStep("get username textbox", () => textBox = this.ChildrenOfType<OfflineUsernameInput>().Single().ChildrenOfType<OsuTextBox>().Single());

            AddStep("focus textbox", () => ((IFocusManager)InputManager).ChangeFocus(textBox));
            AddStep("type new name", () => textBox.Text = "NewName");
            AddStep("commit", () => InputManager.Key(Key.Enter));

            AddStep("exit results", () => this.ChildrenOfType<OsuScreenStack>().Single().CurrentScreen.Exit());

            AddUntilStep("username updated in realm (User)", () => Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!.User.Username) == "NewName");
            AddUntilStep("username updated in realm (RealmUser)", () => Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!.RealmUser.Username) == "NewName");
        }

        [Test]
        public void TestCommitOnExitWithoutExplicitEnter()
        {
            TestResultsScreen screen = null!;
            Guid dbScoreId = Guid.Empty;

            AddStep("create and import guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                dbScoreId = live.Value.ID;
            });
            AddStep("show results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!).Detach()));
            AddUntilStep("results loaded", () => screen.IsLoaded);

            OsuTextBox textBox = null!;
            AddStep("get username textbox", () => textBox = this.ChildrenOfType<OfflineUsernameInput>().Single().ChildrenOfType<OsuTextBox>().Single());

            AddStep("set text without committing", () => textBox.Text = "my cool new username");
            AddStep("exit results", () => this.ChildrenOfType<OsuScreenStack>().Single().CurrentScreen.Exit());

            AddUntilStep("username updated on dispose", () => Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!.User.Username) == "my cool new username");
            AddUntilStep("realm user updated on dispose", () => Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!.RealmUser.Username) == "my cool new username");
        }

        [Test]
        public void TestVisibilityTogglesWithAPIState()
        {
            TestResultsScreen screen = null!;
            Guid dbScoreId = Guid.Empty;

            AddStep("create and import guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                dbScoreId = live.Value.ID;
            });
            AddStep("show results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!).Detach()));
            AddUntilStep("results loaded", () => screen.IsLoaded);

            AddUntilStep("visible when offline", () => this.ChildrenOfType<OfflineUsernameInput>().Single().Alpha, () => Is.EqualTo(1));

            AddStep("go online", () => dummyAPI.SetState(APIState.Online));
            AddUntilStep("hidden when online", () => this.ChildrenOfType<OfflineUsernameInput>().Single().Alpha, () => Is.Zero);

            AddStep("go offline again", () => dummyAPI.SetState(APIState.Offline));
            AddUntilStep("visible when offline again", () => this.ChildrenOfType<OfflineUsernameInput>().Single().Alpha, () => Is.EqualTo(1));
        }

        [Test]
        public void TestLastUsernamePrefillsNextScreen()
        {
            TestResultsScreen screen = null!;
            Guid firstScoreId = Guid.Empty;
            Guid secondScoreId = Guid.Empty;

            AddStep("create and import first guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                firstScoreId = live.Value.ID;
            });
            AddStep("show first results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(firstScoreId)!).Detach()));
            AddUntilStep("first loaded", () => screen.IsLoaded);

            OsuTextBox textBox = null!;
            AddStep("get textbox", () => textBox = this.ChildrenOfType<OfflineUsernameInput>().Single().ChildrenOfType<OsuTextBox>().Single());
            AddStep("focus textbox", () => ((IFocusManager)InputManager).ChangeFocus(textBox));
            AddStep("type Alice and commit", () => textBox.Text = "Alice");
            AddStep("commit", () => InputManager.Key(Key.Enter));
            AddStep("exit first results", () => this.ChildrenOfType<OsuScreenStack>().Single().CurrentScreen.Exit());

            AddStep("create and import second guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                secondScoreId = live.Value.ID;
            });
            AddStep("show second results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(secondScoreId)!).Detach()));
            AddUntilStep("second loaded", () => screen.IsLoaded);

            OsuTextBox secondTextBox = null!;
            AddStep("get second textbox", () => secondTextBox = this.ChildrenOfType<OfflineUsernameInput>().Single().ChildrenOfType<OsuTextBox>().Single());
            AddAssert("prefilled with last username", () => secondTextBox.Text == "Alice");
        }

        [Test]
        public void TestScorePanelUsernameUpdatesImmediatelyOnCommit()
        {
            TestResultsScreen screen = null!;
            Guid dbScoreId = Guid.Empty;

            AddStep("create and import guest score", () =>
            {
                var score = TestResources.CreateTestScoreInfo(importedBeatmap);
                score.User = new GuestUser();
                var live = scoreManager.Import(score)!;
                dbScoreId = live.Value.ID;
            });
            AddStep("show results", () => loadResults(sc => screen = sc, Realm.Run(r => r.Find<ScoreInfo>(dbScoreId)!).Detach()));
            AddUntilStep("results loaded", () => screen.IsLoaded);

            AddUntilStep("panel present", () => this.ChildrenOfType<ScorePanel>().Any(p => p.Score.ID == dbScoreId));
            AddAssert("panel initially guest", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.ID == dbScoreId).Score.User.Username == "Guest");

            OsuTextBox textBox = null!;
            AddStep("get username textbox", () => textBox = this.ChildrenOfType<OfflineUsernameInput>().Single().ChildrenOfType<OsuTextBox>().Single());
            AddStep("focus textbox", () => ((IFocusManager)InputManager).ChangeFocus(textBox));
            AddStep("change to PanelName", () => textBox.Text = "PanelName");
            AddStep("commit", () => InputManager.Key(Key.Enter));

            AddUntilStep("score model username updated", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.ID == dbScoreId).Score.User.Username == "PanelName");
            AddUntilStep("panel text updated", () => this.ChildrenOfType<ScorePanel>().Single(p => p.Score.ID == dbScoreId)
                                                         .ChildrenOfType<OsuSpriteText>().Any(t => t.Text.ToString() == "PanelName"));
        }

        private void loadResults(Action<TestResultsScreen> onCreated, ScoreInfo score)
        {
            TestResultsScreen results;
            Child = new TestResultsContainer(results = new TestResultsScreen(score));
            onCreated(results);
        }

        private partial class TestResultsContainer : Container
        {
            [Cached(typeof(Player))]
            private readonly Player player = new TestPlayer();

            public TestResultsContainer(IScreen screen)
            {
                RelativeSizeAxes = Axes.Both;
                OsuScreenStack stack;

                InternalChild = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                };

                stack.Push(screen);
            }
        }

        private partial class TestResultsScreen : SoloResultsScreen
        {
            public TestResultsScreen(ScoreInfo score)
                : base(score)
            {
                AllowRetry = true;
                IsLocalPlay = true;
            }

            protected override Task<ScoreInfo[]> FetchScores()
            {
                // keep it simple; a small list ensures score panels appear and screen is interactive
                var scores = new ScoreInfo[3];
                for (int i = 0; i < scores.Length; i++)
                    scores[i] = TestResources.CreateTestScoreInfo();
                return Task.FromResult(scores);
            }
        }
    }
}
