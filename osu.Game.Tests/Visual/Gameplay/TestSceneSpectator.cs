// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Gameplay;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Tests.Visual.Spectator;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSpectator : ScreenTestScene
    {
        private readonly APIUser streamingUser = new APIUser { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Test user" };

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        [Resolved]
        private OsuGameBase game { get; set; }

        private TestSpectatorClient spectatorClient => dependenciesScreen.SpectatorClient;
        private DependenciesScreen dependenciesScreen;
        private SoloSpectatorScreen spectatorScreen;

        private BeatmapSetInfo importedBeatmap;
        private int importedBeatmapId;

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load dependencies", () =>
            {
                LoadScreen(dependenciesScreen = new DependenciesScreen());

                // The dependencies screen gets suspended so it stops receiving updates. So its children are manually added to the test scene instead.
                Children = new Drawable[]
                {
                    dependenciesScreen.UserLookupCache,
                    dependenciesScreen.SpectatorClient,
                };
            });

            AddUntilStep("wait for dependencies to load", () => dependenciesScreen.IsLoaded);

            AddStep("import beatmap", () =>
            {
                importedBeatmap = BeatmapImportHelper.LoadOszIntoOsu(game, virtualTrack: true).GetResultSafely();
                importedBeatmapId = importedBeatmap.Beatmaps.First(b => b.Ruleset.OnlineID == 0).OnlineID;
            });
        }

        [Test]
        public void TestSeekToGameplayStartFramesArriveAfterPlayerLoad()
        {
            const double gameplay_start = 10000;

            loadSpectatingScreen();

            start();

            waitForPlayerCurrent();

            sendFrames(startTime: gameplay_start);

            AddAssert("time is greater than seek target", () => currentFrameStableTime, () => Is.GreaterThan(gameplay_start));
        }

        /// <summary>
        /// Tests the same as <see cref="TestSeekToGameplayStartFramesArriveAfterPlayerLoad"/> but with the frames arriving just as <see cref="Player"/> is transitioning into existence.
        /// </summary>
        [Test]
        public void TestSeekToGameplayStartFramesArriveAsPlayerLoaded()
        {
            const double gameplay_start = 10000;

            loadSpectatingScreen();

            start();

            AddUntilStep("wait for player loader", () => this.ChildrenOfType<PlayerLoader>().SingleOrDefault()?.IsLoaded == true);

            AddUntilStep("queue send frames on player load", () =>
            {
                var loadingPlayer = this.ChildrenOfType<PlayerLoader>().SingleOrDefault()?.CurrentPlayer;

                if (loadingPlayer == null)
                    return false;

                loadingPlayer.OnLoadComplete += _ =>
                    spectatorClient.SendFramesFromUser(streamingUser.Id, 10, gameplay_start);

                return true;
            });

            waitForPlayerCurrent();

            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
            AddAssert("time is greater than seek target", () => currentFrameStableTime, () => Is.GreaterThan(gameplay_start));
        }

        [Test]
        public void TestFrameStarvationAndResume()
        {
            loadSpectatingScreen();

            AddAssert("screen hasn't changed", () => Stack.CurrentScreen is SoloSpectatorScreen);

            start();
            waitForPlayerCurrent();

            sendFrames();
            AddAssert("ensure frames arrived", () => replayHandler.HasFrames);

            AddUntilStep("wait for frame starvation", () => replayHandler.WaitingForFrame);
            checkPaused(true);

            double? pausedTime = null;

            AddStep("store time", () => pausedTime = currentFrameStableTime);

            sendFrames();

            AddUntilStep("wait for frame starvation", () => replayHandler.WaitingForFrame);
            checkPaused(true);

            AddAssert("time advanced", () => currentFrameStableTime, () => Is.GreaterThan(pausedTime));
        }

        [Test]
        public void TestPlayStartsWithNoFrames()
        {
            loadSpectatingScreen();

            start();
            waitForPlayerCurrent();
            checkPaused(true);

            // send enough frames to ensure play won't be paused
            sendFrames(100);

            checkPaused(false);
        }

        [Test]
        public void TestSpectatingDuringGameplay()
        {
            start();
            sendFrames(300);

            loadSpectatingScreen();
            waitForPlayerCurrent();

            sendFrames(300);

            AddUntilStep("playing from correct point in time", () => player.ChildrenOfType<DrawableRuleset>().First().FrameStableClock.CurrentTime, () => Is.GreaterThan(30000));
        }

        [Test]
        public void TestHostRetriesWhileWatching()
        {
            loadSpectatingScreen();

            start();
            sendFrames();

            waitForPlayerCurrent();

            Player lastPlayer = null;
            AddStep("store first player", () => lastPlayer = player);

            start();
            sendFrames();

            waitForPlayerCurrent();
            AddAssert("player is different", () => lastPlayer != player);
        }

        [Test]
        public void TestHostFails()
        {
            loadSpectatingScreen();

            start();

            waitForPlayerCurrent();
            checkPaused(true);
            sendFrames();

            finish(SpectatedUserState.Failed);

            checkPaused(false); // Should continue playing until out of frames
            checkPaused(true); // And eventually stop after running out of frames and fail.
            // Todo: Should check for + display a failed message.

            AddStep("exit player", () => InputManager.Key(Key.Escape));
            AddAssert("player exited", () => Stack.CurrentScreen is SoloSpectatorScreen);
        }

        [Test]
        public void TestStopWatchingDuringPlay()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayerCurrent();

            AddStep("stop spectating", () => (Stack.CurrentScreen as Player)?.Exit());
            AddUntilStep("spectating stopped", () => spectatorScreen.GetChildScreen() == null);
        }

        [Test]
        public void TestStopWatchingThenHostRetries()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayerCurrent();

            AddStep("stop spectating", () => (Stack.CurrentScreen as Player)?.Exit());
            AddUntilStep("spectating stopped", () => spectatorScreen.GetChildScreen() == null);

            // host starts playing a new session
            start();
            waitForPlayerCurrent();
        }

        [Test]
        public void TestWatchingBeatmapThatDoesntExistLocally()
        {
            loadSpectatingScreen();

            start(-1234);
            sendFrames();

            AddAssert("screen didn't change", () => Stack.CurrentScreen is SoloSpectatorScreen);
        }

        [Test]
        public void TestFinalFramesPurgedBeforeEndingPlay()
        {
            AddStep("begin playing", () => spectatorClient.BeginPlaying(0, TestGameplayState.Create(new OsuRuleset()), new Score()));

            AddStep("send frames and finish play", () =>
            {
                spectatorClient.HandleFrame(new OsuReplayFrame(1000, Vector2.Zero));

                var completedGameplayState = TestGameplayState.Create(new OsuRuleset());
                completedGameplayState.HasPassed = true;
                spectatorClient.EndPlaying(completedGameplayState);
            });

            // We can't access API because we're an "online" test.
            AddAssert("last received frame has time = 1000", () => spectatorClient.LastReceivedUserFrames.First().Value.Time == 1000);
        }

        [Test]
        public void TestFinalFrameInBundleHasHeader()
        {
            FrameDataBundle lastBundle = null;

            AddStep("bind to client", () => spectatorClient.OnNewFrames += (_, bundle) => lastBundle = bundle);

            start(-1234);
            sendFrames();
            finish();

            AddUntilStep("bundle received", () => lastBundle != null);
            AddAssert("first frame does not have header", () => lastBundle.Frames[0].Header == null);
            AddAssert("last frame has header", () => lastBundle.Frames[^1].Header != null);
        }

        [Test]
        public void TestPlayingState()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayerCurrent();
            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
        }

        [Test]
        public void TestPassedState()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayerCurrent();

            AddStep("send passed", () => spectatorClient.SendEndPlay(streamingUser.Id, SpectatedUserState.Passed));
            AddUntilStep("state is passed", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Passed);

            start();
            sendFrames();
            waitForPlayerCurrent();
            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
        }

        [Test]
        public void TestQuitState()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayerCurrent();

            AddStep("send quit", () => spectatorClient.SendEndPlay(streamingUser.Id));
            AddUntilStep("state is quit", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Quit);

            AddAssert("wait for player exit", () => Stack.CurrentScreen is SoloSpectatorScreen);

            start();
            sendFrames();
            waitForPlayerCurrent();
            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
        }

        [Test]
        public void TestFailedStateDuringPlay()
        {
            loadSpectatingScreen();

            start();
            sendFrames();

            waitForPlayerCurrent();

            AddStep("send failed", () => spectatorClient.SendEndPlay(streamingUser.Id, SpectatedUserState.Failed));
            AddUntilStep("state is failed", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Failed);

            AddUntilStep("wait for player to fail", () => player.GameplayState.HasFailed);

            start();
            sendFrames();
            waitForPlayerCurrent();
            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
        }

        [Test]
        public void TestFailedStateDuringLoading()
        {
            loadSpectatingScreen();

            start();
            sendFrames();

            waitForPlayerLoader();

            AddStep("send failed", () => spectatorClient.SendEndPlay(streamingUser.Id, SpectatedUserState.Failed));
            AddUntilStep("state is failed", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Failed);

            AddAssert("wait for player exit", () => Stack.CurrentScreen is SoloSpectatorScreen);

            start();
            sendFrames();
            waitForPlayerCurrent();
            AddUntilStep("state is playing", () => spectatorClient.WatchedUserStates[streamingUser.Id].State == SpectatedUserState.Playing);
        }

        private OsuFramedReplayInputHandler replayHandler =>
            (OsuFramedReplayInputHandler)Stack.ChildrenOfType<OsuInputManager>().First().ReplayInputHandler;

        private Player player => this.ChildrenOfType<Player>().Single();

        private double currentFrameStableTime
            => player.ChildrenOfType<FrameStabilityContainer>().First().CurrentTime;

        private void waitForPlayerLoader() => AddUntilStep("wait for loading", () => this.ChildrenOfType<SpectatorPlayerLoader>().SingleOrDefault()?.IsLoaded == true);

        private void waitForPlayerCurrent() => AddUntilStep("wait for player current", () => this.ChildrenOfType<Player>().SingleOrDefault()?.IsCurrentScreen() == true);

        private void start(int? beatmapId = null) => AddStep("start play", () => spectatorClient.SendStartPlay(streamingUser.Id, beatmapId ?? importedBeatmapId));

        private void finish(SpectatedUserState state = SpectatedUserState.Quit) => AddStep("end play", () => spectatorClient.SendEndPlay(streamingUser.Id, state));

        private void checkPaused(bool state) =>
            AddUntilStep($"game is {(state ? "paused" : "playing")}", () => player.ChildrenOfType<DrawableRuleset>().First().IsPaused.Value == state);

        private void sendFrames(int count = 10, double startTime = 0)
        {
            AddStep("send frames", () => spectatorClient.SendFramesFromUser(streamingUser.Id, count, startTime));
        }

        private void loadSpectatingScreen()
        {
            AddStep("load spectator", () => LoadScreen(spectatorScreen = new SoloSpectatorScreen(streamingUser)));
            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded);
        }

        /// <summary>
        /// Used for the sole purpose of adding <see cref="TestSpectatorClient"/> as a resolvable dependency.
        /// </summary>
        private partial class DependenciesScreen : OsuScreen
        {
            [Cached(typeof(SpectatorClient))]
            public readonly TestSpectatorClient SpectatorClient = new TestSpectatorClient();

            [Cached(typeof(UserLookupCache))]
            public readonly TestUserLookupCache UserLookupCache = new TestUserLookupCache();
        }
    }
}
