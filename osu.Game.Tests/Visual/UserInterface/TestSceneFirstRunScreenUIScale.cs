// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFirstRunScreenUIScale : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Cached(typeof(BeatmapStore))]
        private BeatmapStore beatmapStore = new TestBeatmapStore();

        public TestSceneFirstRunScreenUIScale()
        {
            AddStep("load screen", () =>
            {
                Child = new ScreenStack(new ScreenUIScale());
            });
        }
    }
}
