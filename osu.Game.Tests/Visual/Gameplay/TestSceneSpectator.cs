// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSpectatorStreamingClient();

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        private Spectator spectatorScreen;

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
                Remove(testSpectatorStreamingClient);
                Add(testSpectatorStreamingClient);
            });

            finish();
        }

        [Test]
        public void TestFrameStarvationAndResume()
        {
            loadSpectatingScreen();

            AddAssert("screen hasn't changed", () => Stack.CurrentScreen is Spectator);

            start();
            sendFrames();

            waitForPlayer();
            AddAssert("ensure frames arrived", () => replayHandler.HasFrames);

            AddUntilStep("wait for frame starvation", () => replayHandler.NextFrame == null);
            checkPaused(true);

            double? pausedTime = null;

            AddStep("store time", () => pausedTime = currentFrameStableTime);

            sendFrames();

            AddUntilStep("wait for frame starvation", () => replayHandler.NextFrame == null);
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

            AddAssert("screen didn't change", () => Stack.CurrentScreen is Spectator);
        }

        private OsuFramedReplayInputHandler replayHandler =>
            (OsuFramedReplayInputHandler)Stack.ChildrenOfType<OsuInputManager>().First().ReplayInputHandler;

        private Player player => Stack.CurrentScreen as Player;

        private double currentFrameStableTime
            => player.ChildrenOfType<FrameStabilityContainer>().First().FrameStableClock.CurrentTime;

        private void waitForPlayer() => AddUntilStep("wait for player", () => Stack.CurrentScreen is Player);

        private void start(int? beatmapId = null) => AddStep("start play", () => testSpectatorStreamingClient.StartPlay(beatmapId ?? importedBeatmapId));

        private void finish(int? beatmapId = null) => AddStep("end play", () => testSpectatorStreamingClient.EndPlay(beatmapId ?? importedBeatmapId));

        private void checkPaused(bool state) =>
            AddUntilStep($"game is {(state ? "paused" : "playing")}", () => player.ChildrenOfType<DrawableRuleset>().First().IsPaused.Value == state);

        private void sendFrames(int count = 10)
        {
            AddStep("send frames", () =>
            {
                testSpectatorStreamingClient.SendFrames(nextFrame, count);
                nextFrame += count;
            });
        }

        private void loadSpectatingScreen()
        {
            AddStep("load screen", () => LoadScreen(spectatorScreen = new Spectator(testSpectatorStreamingClient.StreamingUser)));
            AddUntilStep("wait for screen load", () => spectatorScreen.LoadState == LoadState.Loaded);
        }

        public class TestSpectatorStreamingClient : SpectatorStreamingClient
        {
            public readonly User StreamingUser = new User { Id = 55, Username = "Test user" };

            public new BindableList<int> PlayingUsers => (BindableList<int>)base.PlayingUsers;

            private int beatmapId;

            public TestSpectatorStreamingClient()
                : base(new DevelopmentEndpointConfiguration())
            {
            }

            protected override Task Connect()
            {
                return Task.CompletedTask;
            }

            public void StartPlay(int beatmapId)
            {
                this.beatmapId = beatmapId;
                sendState(beatmapId);
            }

            public void EndPlay(int beatmapId)
            {
                ((ISpectatorClient)this).UserFinishedPlaying(StreamingUser.Id, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });

                sentState = false;
            }

            private bool sentState;

            public void SendFrames(int index, int count)
            {
                var frames = new List<LegacyReplayFrame>();

                for (int i = index; i < index + count; i++)
                {
                    var buttonState = i == index + count - 1 ? ReplayButtonState.None : ReplayButtonState.Left1;

                    frames.Add(new LegacyReplayFrame(i * 100, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
                }

                var bundle = new FrameDataBundle(new ScoreInfo(), frames);
                ((ISpectatorClient)this).UserSentFrames(StreamingUser.Id, bundle);

                if (!sentState)
                    sendState(beatmapId);
            }

            public override void WatchUser(int userId)
            {
                if (sentState)
                {
                    // usually the server would do this.
                    sendState(beatmapId);
                }

                base.WatchUser(userId);
            }

            private void sendState(int beatmapId)
            {
                sentState = true;
                ((ISpectatorClient)this).UserBeganPlaying(StreamingUser.Id, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });
            }
        }
    }
}
