// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOverlayHeaderBackground : OsuTestScene
    {
        public TestSceneOverlayHeaderBackground()
        {
            Add(new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Children = new[]
                    {
                        new OverlayHeaderBackground(@"Headers/changelog"),
                        new OverlayHeaderBackground(@"Headers/news"),
                        new OverlayHeaderBackground(@"Headers/rankings"),
                        new OverlayHeaderBackground(@"Headers/search"),
                    }
                }
            });
        }
    }
}
