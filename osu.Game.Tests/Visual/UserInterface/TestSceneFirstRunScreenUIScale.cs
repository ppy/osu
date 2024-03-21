// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFirstRunScreenUIScale : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneFirstRunScreenUIScale()
        {
            AddStep("load screen", () =>
            {
                Child = new ScreenStack(new ScreenUIScale());
            });
        }
    }
}
