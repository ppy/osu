// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Overlays.Toolbar;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneToolbarClock : OsuManualInputManagerTestScene
    {
        private readonly Container mainContainer;

        public TestSceneToolbarClock()
        {
            Children = new Drawable[]
            {
                mainContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = Toolbar.HEIGHT,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                                new ToolbarClock(),
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                            }
                        },
                    }
                },
            };

            AddSliderStep("scale", 0.5, 4, 1, scale => mainContainer.Scale = new Vector2((float)scale));
        }

        [Test]
        public void TestRealGameTime()
        {
            AddStep("Set game time real", () => mainContainer.Clock = Clock);
        }

        [Test]
        public void TestLongGameTime()
        {
            AddStep("Set game time long", () => mainContainer.Clock = new FramedOffsetClock(Clock, false) { Offset = 3600.0 * 24 * 1000 * 98 });
        }
    }
}
