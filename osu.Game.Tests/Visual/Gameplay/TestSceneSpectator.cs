// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Tests.Visual.Spectator;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        private readonly APIUser streamingUser = new APIUser { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Test user" };

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        [Resolved]
        private OsuGameBase game { get; set; }

        private TestSpectatorClient spectatorClient;
        private SoloSpectator spectatorScreen;

        private BeatmapSetInfo importedBeatmap;
        private int importedBeatmapId;

        [SetUpSteps]
        public void SetupSteps()
        {
            DependenciesScreen dependenciesScreen = null;

            AddStep("load dependencies", () =>
            {
                spectatorClient = new TestSpectatorClient();

                // The screen gets suspended so it stops receiving updates.
                Child = spectatorClient;

                LoadScreen(dependenciesScreen = new DependenciesScreen(spectatorClient));
            });

            AddUntilStep("wait for dependencies to load", () => dependenciesScreen.IsLoaded);

            AddStep("import beatmap", () =>
            {
                importedBeatmap = BeatmapImportHelper.LoadOszIntoOsu(game, virtualTrack: true).GetResultSafely();
                importedBeatmapId = importedBeatmap.Beatmaps.First(b => b.Ruleset.OnlineID == 0).OnlineID;
            });
        }

        [Test]
        public void TestFrameStarvationAndResume()
        {
            loadSpectatingScreen();

            AddAssert("screen hasn't changed", () => Stack.CurrentScreen is SoloSpectator);

            start();
            waitForPlayer();

            sendFrames();
            AddAssert("ensure frames arrived", () => replayHandler.HasFrames);

            AddUntilStep("wait for frame starvation", () => replayHandler.WaitingForFrame);
            checkPaused(true);

            double? pausedTime = null;

            AddStep("store time", () => pausedTime = currentFrameStableTime);

            sendFrames();

            AddUntilStep("wait for frame starvation", () => replayHandler.WaitingForFrame);
            checkPaused(true);

            AddAssert("time advanced", () => currentFrameStableTime > pausedTime);
        }

        [Test]
        public void TestPlayStartsWithNoFrames()
        {
            loadSpectatingScreen();

            start();
            waitForPlayer();
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
            waitForPlayer();

            sendFrames(300);

            AddUntilStep("playing from correct point in time", () => player.ChildrenOfType<DrawableRuleset>().First().FrameStableClock.CurrentTime > 30000);
        }

        [Test]
        public void TestHostRetriesWhileWatching()
        {
            loadSpectatingScreen();

            start();
            sendFrames();

            waitForPlayer();

            Player lastPlayer = null;
            AddStep("store first player", () => lastPlayer = player);

            start();
            sendFrames();

            waitForPlayer();
            AddAssert("player is different", () => lastPlayer != player);
        }

        [Test]
        public void TestHostFails()
        {
            loadSpectatingScreen();

            start();

            waitForPlayer();
            checkPaused(true);

            finish();

            checkPaused(false);
            // TODO: should replay until running out of frames then fail
        }

        [Test]
        public void TestStopWatchingDuringPlay()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayer();

            AddStep("stop spectating", () => (Stack.CurrentScreen as Player)?.Exit());
            AddUntilStep("spectating stopped", () => spectatorScreen.GetChildScreen() == null);
        }

        [Test]
        public void TestStopWatchingThenHostRetries()
        {
            loadSpectatingScreen();

            start();
            sendFrames();
            waitForPlayer();

            AddStep("stop spectating", () => (Stack.CurrentScreen as Player)?.Exit());
            AddUntilStep("spectating stopped", () => spectatorScreen.GetChildScreen() == null);

            // host starts playing a new session
            start();
            waitForPlayer();
        }

        [Test]
        public void TestWatchingBeatmapThatDoesntExistLocally()
        {
            loadSpectatingScreen();

            start(-1234);
            sendFrames();

            AddAssert("screen didn't change", () => Stack.CurrentScreen is SoloSpectator);
        }

        [Test]
        public void TestFinalFramesPurgedBeforeEndingPlay()
        {
            AddStep("begin playing", () => spectatorClient.BeginPlaying(new GameplayState(new TestBeatmap(new OsuRuleset().RulesetInfo), new OsuRuleset()), new Score()));

            AddStep("send frames and finish play", () =>
            {
                spectatorClient.HandleFrame(new OsuReplayFrame(1000, Vector2.Zero));
                spectatorClient.EndPlaying();
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

        private OsuFramedReplayInputHandler replayHandler =>
            (OsuFramedReplayInputHandler)Stack.ChildrenOfType<OsuInputManager>().First().ReplayInputHandler;

        private Player player => Stack.CurrentScreen as Player;

        private double currentFrameStableTime
            => player.ChildrenOfType<FrameStabilityContainer>().First().FrameStableClock.CurrentTime;

        private void waitForPlayer() => AddUntilStep("wait for player", () => (Stack.CurrentScreen as Player)?.IsLoaded == true);

        private void start(int? beatmapId = null) => AddStep("start play", () => spectatorClient.StartPlay(streamingUser.Id, beatmapId ?? importedBeatmapId));

        private void finish() => AddStep("end play", () => spectatorClient.EndPlay(streamingUser.Id));

        private void checkPaused(bool state) =>
            AddUntilStep($"game is {(state ? "paused" : "playing")}", () => player.ChildrenOfType<DrawableRuleset>().First().IsPaused.Value == state);

        private void sendFrames(int count = 10)
        {
            AddStep("send frames", () => spectatorClient.SendFrames(streamingUser.Id, count));
        }

        private void loadSpectatingScreen()
        {
            AddStep("load spectator", () => LoadScreen(spectatorScreen = new SoloSpectator(streamingUser)));
            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded);
        }

        /// <summary>
        /// Used for the sole purpose of adding <see cref="TestSpectatorClient"/> as a resolvable dependency.
        /// </summary>
        private class DependenciesScreen : OsuScreen
        {
            [Cached(typeof(SpectatorClient))]
            public readonly TestSpectatorClient Client;

            public DependenciesScreen(TestSpectatorClient client)
            {
                Client = client;
            }
        }
    }
}
