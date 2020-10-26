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
        public void TestSpectating()
        {
            AddStep("add streaming client", () => Add(testSpectatorStreamingClient));
            AddStep("load screen", () => LoadScreen(new Spectator(testSpectatorStreamingClient.StreamingUser)));
            AddStep("start play", () => testSpectatorStreamingClient.StartPlay());
            AddStep("send frames", () => testSpectatorStreamingClient.SendFrames());
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
