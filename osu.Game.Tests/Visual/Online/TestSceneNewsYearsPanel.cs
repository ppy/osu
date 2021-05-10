// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.News.Sidebar;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsYearsPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly YearsPanel panel;

        public TestSceneNewsYearsPanel()
        {
            Add(panel = new YearsPanel()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddStep("Load years", () => panel.Years = new[] { 1000, 2000, 3000, 4000 });
            AddStep("Load different years", () => panel.Years = new[] { 1001, 2001, 3001, 4001, 5001, 6001, 7001, 8001 });
        }
    }
}
