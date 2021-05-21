// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiMainPage : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Orange);

        public TestSceneWikiMainPage()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColour.Background5,
                    RelativeSizeAxes = Axes.Both,
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                    Child = new WikiMainPage(),
                }
            };
        }
    }
}
