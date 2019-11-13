// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneFavouriteButton : OsuTestScene
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
            AddStep("set valid beatmap", () => favourite.BeatmapSet.Value = new BeatmapSetInfo { OnlineBeatmapSetID = 88 });
            AddStep("log out", () => API.Logout());
            checkEnabled(false);
            AddStep("log in", () => API.Login("test", "test"));
            checkEnabled(true);
        }

        [Test]
        public void TestBeatmapChange()
        {
            AddStep("log in", () => API.Login("test", "test"));
            AddStep("set valid beatmap", () => favourite.BeatmapSet.Value = new BeatmapSetInfo { OnlineBeatmapSetID = 88 });
            checkEnabled(true);
            AddStep("set invalid beatmap", () => favourite.BeatmapSet.Value = new BeatmapSetInfo());
            checkEnabled(false);
        }

        private void checkEnabled(bool expected)
        {
            AddAssert("is " + (expected ? "enabled" : "disabled"), () => favourite.Enabled.Value == expected);
        }
    }
}
