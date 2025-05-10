// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneExpandingBar : OsuTestScene
    {
        public TestSceneExpandingBar()
        {
            Container container;
            ExpandingBar expandingBar;

            Add(container = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Gray,
                        Alpha = 0.5f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    expandingBar = new ExpandingBar
                    {
                        Anchor = Anchor.Centre,
                        ExpandedSize = 10,
                        CollapsedSize = 2,
                        Colour = Color4.DeepSkyBlue,
                    }
                }
            });

            AddStep(@"Collapse", () => expandingBar.Collapse());
            AddStep(@"Expand", () => expandingBar.Expand());
            AddSliderStep(@"Resize container", 1, 300, 150, value => container.ResizeTo(value));
            AddStep(@"Horizontal", () => expandingBar.RelativeSizeAxes = Axes.X);
            AddStep(@"Anchor top", () => expandingBar.Anchor = Anchor.TopCentre);
            AddStep(@"Vertical", () => expandingBar.RelativeSizeAxes = Axes.Y);
            AddStep(@"Anchor left", () => expandingBar.Anchor = Anchor.CentreLeft);
        }
    }
}
