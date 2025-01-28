// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneFavouriteButton : OsuTestScene
    {
        private FavouriteButton favourite;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create button", () => Child = favourite = new FavouriteButton
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(50),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestLoggedOutIn()
        {
            AddStep("set valid beatmap", () => favourite.BeatmapSet.Value = new APIBeatmapSet { OnlineID = 88 });
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
        public void TestBeatmapChange()
        {
            AddStep("log in", () =>
            {
                API.Login("test", "test");
                ((DummyAPIAccess)API).AuthenticateSecondFactor("abcdefgh");
            });
            AddStep("set valid beatmap", () => favourite.BeatmapSet.Value = new APIBeatmapSet { OnlineID = 88 });
            checkEnabled(true);
            AddStep("set invalid beatmap", () => favourite.BeatmapSet.Value = new APIBeatmapSet());
            checkEnabled(false);
        }

        private void checkEnabled(bool expected)
        {
            AddAssert("is " + (expected ? "enabled" : "disabled"), () => favourite.Enabled.Value == expected);
        }
    }
}
