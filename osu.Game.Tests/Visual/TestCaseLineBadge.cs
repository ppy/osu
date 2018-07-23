// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLineBadge : OsuTestCase
    {
        public TestCaseLineBadge()
        {
            Container containerHorizontal;
            LineBadge lineBadge;

            Add(containerHorizontal = new Container
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
                    lineBadge = new LineBadge
                    {
                        Anchor = Anchor.Centre,
                        UncollapsedSize = 10,
                        CollapsedSize = 2,
                        Colour = Color4.DeepSkyBlue,
                    }
                }
            });

            AddStep(@"", () => { });
            AddStep(@"Collapse", () => lineBadge.Collapse());
            AddStep(@"Uncollapse", () => lineBadge.Uncollapse());
            AddSliderStep(@"Resize container", 1, 300, 150, value => containerHorizontal.ResizeTo(value));
            AddStep(@"Horizontal", () => lineBadge.IsHorizontal = true);
            AddStep(@"Anchor top", () => lineBadge.Anchor = Anchor.TopCentre);
            AddStep(@"Vertical", () => lineBadge.IsHorizontal = false);
            AddStep(@"Anchor left", () => lineBadge.Anchor = Anchor.CentreLeft);
        }
    }
}
