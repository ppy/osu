// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Tests.Visual.Spectator;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        private readonly User streamingUser = new User { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Test user" };

        [Cached(typeof(SpectatorClient))]
        private TestSpectatorClient testSpectatorClient = new TestSpectatorClient();

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        private SoloSpectator spectatorScreen;

        [Resolved]
        private OsuGameBase game { get; set; }

        private int nextFrame;

        private BeatmapSetInfo importedBeatmap;

        private int importedBeatmapId;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset sent frames", () => nextFrame = 0);

            AddStep("import beatmap", () =>
            {
                importedBeatmap = ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Result;
                importedBeatmapId = importedBeatmap.Beatmaps.First(b => b.RulesetID == 0).OnlineBeatmapID ?? -1;
            });

            AddStep("add streaming client", () =>
            {
                Remove(testSpectatorClient);
                Add(testSpectatorClient);
            });

            finish();
        }

        [Test]
        public void TestFrameStarvationAndResume()
        {
            loadSpectatingScreen();

            AddAssert("screen hasn't changed", () => Stack.CurrentScreen is SoloSpectator);

            start();
            sendFrames();

            waitForPlayer();
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

            sendFrames(1000); // send enough frames to ensure play won't be paused

            checkPaused(false);
        }

        [Test]
        public void TestSpectatingDuringGameplay()
        {
            start();

            loadSpectatingScreen();

            AddStep("advance frame count", () => nextFrame = 300);
            sendFrames();

            waitForPlayer();

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

        private OsuFramedReplayInputHandler replayHandler =>
            (OsuFramedReplayInputHandler)Stack.ChildrenOfType<OsuInputManager>().First().ReplayInputHandler;

        private Player player => Stack.CurrentScreen as Player;

        private double currentFrameStableTime
            => player.ChildrenOfType<FrameStabilityContainer>().First().FrameStableClock.CurrentTime;

        private void waitForPlayer() => AddUntilStep("wait for player", () => Stack.CurrentScreen is Player);

        private void start(int? beatmapId = null) => AddStep("start play", () => testSpectatorClient.StartPlay(streamingUser.Id, beatmapId ?? importedBeatmapId));

        private void finish() => AddStep("end play", () => testSpectatorClient.EndPlay(streamingUser.Id));

        private void checkPaused(bool state) =>
            AddUntilStep($"game is {(state ? "paused" : "playing")}", () => player.ChildrenOfType<DrawableRuleset>().First().IsPaused.Value == state);

        private void sendFrames(int count = 10)
        {
            AddStep("send frames", () =>
            {
                testSpectatorClient.SendFrames(streamingUser.Id, nextFrame, count);
                nextFrame += count;
            });
        }

        private void loadSpectatingScreen()
        {
            AddStep("load screen", () => LoadScreen(spectatorScreen = new SoloSpectator(streamingUser)));
            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded);
        }

        internal class TestUserLookupCache : UserLookupCache
        {
            protected override Task<User> ComputeValueAsync(int lookup, CancellationToken token = default) => Task.FromResult(new User
            {
                Id = lookup,
                Username = $"User {lookup}"
            });
        }
    }
}
