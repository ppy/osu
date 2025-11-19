// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneFavouriteButton : OsuTestScene
    {
        private FavouriteButton? favourite;

        private readonly BeatmapSetInfo beatmapSetInfo = new BeatmapSetInfo { OnlineID = 88 };
        private readonly BeatmapSetInfo invalidBeatmapSetInfo = new BeatmapSetInfo();

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create button", () => Child = favourite = new FavouriteButton(beatmapSetInfo)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddStep("register request handling", () => dummyAPI.HandleRequest = request =>
            {
                if (!(request is GetBeatmapSetRequest beatmapSetRequest)) return false;

                beatmapSetRequest.TriggerSuccess(new APIBeatmapSet
                {
                    OnlineID = beatmapSetRequest.ID,
                    HasFavourited = false,
                    FavouriteCount = 0,
                });

                return true;
            });
        }

        [Test]
        public void TestLoggedOutIn()
        {
            AddStep("log out", () => API.Logout());
            checkEnabled(false);
            AddStep("log in", () =>
            {
                API.Login("test", "test");
                ((DummyAPIAccess)API).AuthenticateSecondFactor("abcdefgh");
            });
            checkEnabled(true);
        }

        [Test]
        public void TestInvalidBeatmap()
        {
            AddStep("make beatmap invalid", () => Child = favourite = new FavouriteButton(invalidBeatmapSetInfo)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
            AddStep("log in", () =>
            {
                API.Login("test", "test");
                ((DummyAPIAccess)API).AuthenticateSecondFactor("abcdefgh");
            });
            checkEnabled(false);
        }

        private void checkEnabled(bool expected)
        {
            AddAssert("is " + (expected ? "enabled" : "disabled"), () => favourite!.Enabled.Value == expected);
        }
    }
}
