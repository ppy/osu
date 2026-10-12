// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Playlists;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestSceneAddToPlaylistFooterButton : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private AddToPlaylistFooterButton button = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = button = new AddToPlaylistFooterButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Action = () => { }
            };
        });

        [Test]
        public void TestAppearDisappear()
        {
            AddStep("appear", () => button.Appear());
            AddWaitStep("wait for animation", 3);
            AddStep("disappear", () => button.Disappear());
            AddWaitStep("wait for animation", 3);
            AddStep("appear", () => button.Appear());
        }
    }
}
