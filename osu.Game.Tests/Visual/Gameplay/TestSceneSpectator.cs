// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSpectatorStreamingClient();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Test]
        public void TestBasicSpectatingFlow()
        {
            AddStep("add streaming client", () => Add(testSpectatorStreamingClient));
            AddStep("load screen", () => LoadScreen(new Spectator(testSpectatorStreamingClient.StreamingUser)));
            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());
        }

        [Test]
        public void TestSpectatingDuringGameplay()
        {
            // should seek immediately to available frames
        }

        [Test]
        public void TestHostStartsPlayingWhileAlreadyWatching()
        {
            // should restart either immediately or after running out of frames
        }

        [Test]
        public void TestHostFails()
        {
            // should replay until running out of frames then fail
        }

        [Test]
        public void TestStopWatchingDuringPlay()
        {
            // should immediately exit and unbind from streaming client
        }

        internal class TestSpectatorStreamingClient : SpectatorStreamingClient
        {
            [Resolved]
            private BeatmapManager beatmaps { get; set; }

            public readonly User StreamingUser = new User { Id = 1234, Username = "Test user" };

            public void StartPlay()
            {
                ((ISpectatorClient)this).UserBeganPlaying((int)StreamingUser.Id, new SpectatorState
                {
                    BeatmapID = beatmaps.GetAllUsableBeatmapSets().First().Beatmaps.First().OnlineBeatmapID,
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
