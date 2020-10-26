// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSpectatorStreamingClient();

        private Spectator spectatorScreen;

        [Resolved]
        private OsuGameBase game { get; set; }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () => ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Wait());

            AddStep("add streaming client", () =>
            {
                Remove(testSpectatorStreamingClient);
                Add(testSpectatorStreamingClient);
            });
        }

        [Test]
        public void TestBasicSpectatingFlow()
        {
            beginSpectating();
            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());
        }

        [Test]
        public void TestSpectatingDuringGameplay()
        {
            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());

            // should seek immediately to available frames
            beginSpectating();
        }

        [Test]
        public void TestHostStartsPlayingWhileAlreadyWatching()
        {
            beginSpectating();

            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());

            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());
            // should restart either immediately or after running out of frames
        }

        [Test]
        public void TestHostFails()
        {
            beginSpectating();

            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());
            // todo: send fail state

            // should replay until running out of frames then fail
        }

        [Test]
        public void TestStopWatchingDuringPlay()
        {
            beginSpectating();

            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());

            AddUntilStep("wait for player", () => Stack.CurrentScreen is Player);

            // should immediately exit and unbind from streaming client
            AddStep("stop spectating", () => (Stack.CurrentScreen as Player)?.Exit());

            AddUntilStep("spectating stopped", () => spectatorScreen.GetParentScreen() == null);
        }

        [Test]
        public void TestWatchingBeatmapThatDoesntExistLocally()
        {
            beginSpectating();

            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());

            // player should never arrive.
        }

        private void beginSpectating() =>
            AddStep("load screen", () => LoadScreen(spectatorScreen = new Spectator(testSpectatorStreamingClient.StreamingUser)));

        internal class TestSpectatorStreamingClient : SpectatorStreamingClient
        {
            [Resolved]
            private BeatmapManager beatmaps { get; set; }

            public readonly User StreamingUser = new User { Id = 1234, Username = "Test user" };

            public void StartPlay()
            {
                ((ISpectatorClient)this).UserBeganPlaying((int)StreamingUser.Id, new SpectatorState
                {
                    BeatmapID = beatmaps.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.RulesetID == 0).OnlineBeatmapID,
                    RulesetID = 0,
                });
            }

            public void EndPlay()
            {
                ((ISpectatorClient)this).UserFinishedPlaying((int)StreamingUser.Id, new SpectatorState
                {
                    BeatmapID = beatmaps.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.RulesetID == 0).OnlineBeatmapID,
                    RulesetID = 0,
                });
            }

            public void SendFrames()
            {
                ((ISpectatorClient)this).UserSentFrames((int)StreamingUser.Id, new FrameDataBundle(new[]
                {
                    // todo: populate more frames
                    new LegacyReplayFrame(0, 0, 0, ReplayButtonState.Left1)
                }));
            }
        }
    }
}
