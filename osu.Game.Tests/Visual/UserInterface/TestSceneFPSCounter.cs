// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFPSCounter : OsuTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create display", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new FPSCounter(),
                            new FPSCounter { Scale = new Vector2(2) },
                            new FPSCounter { Scale = new Vector2(4) },
                        }
                    },
                };
            });
        }

        [Test]
        public void TestBasic()
        {
        }
    }
}
