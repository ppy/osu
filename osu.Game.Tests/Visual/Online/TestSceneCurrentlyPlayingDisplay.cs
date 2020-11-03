// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Overlays.Dashboard;
using osu.Game.Tests.Visual.Gameplay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCurrentlyPlayingDisplay : OsuTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSceneSpectator.TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSceneSpectator.TestSpectatorStreamingClient();

        private CurrentlyPlayingDisplay currentlyPlaying;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("register request handling", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetUserRequest cRequest:
                        cRequest.TriggerSuccess(new User { Username = "peppy", Id = 2 });
                        break;
                }
            });

            AddStep("add streaming client", () =>
            {
                Remove(testSpectatorStreamingClient);

                Children = new Drawable[]
                {
                    testSpectatorStreamingClient,
                    currentlyPlaying = new CurrentlyPlayingDisplay
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            });

            AddStep("Reset players", () => testSpectatorStreamingClient.PlayingUsers.Clear());
        }

        [Test]
        public void TestBasicDisplay()
        {
            AddStep("Add playing user", () => testSpectatorStreamingClient.PlayingUsers.Add(2));
            AddUntilStep("Panel loaded", () => currentlyPlaying.ChildrenOfType<UserGridPanel>()?.FirstOrDefault()?.User.Id == 2);
            AddStep("Remove playing user", () => testSpectatorStreamingClient.PlayingUsers.Remove(2));
            AddUntilStep("Panel no longer present", () => !currentlyPlaying.ChildrenOfType<UserGridPanel>().Any());
        }
    }
}
