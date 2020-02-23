// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using NUnit.Framework;
using osu.Framework.Utils;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOverlayScrollContainer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlayScrollContainer)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly OverlayScrollContainer scroll;

        public TestSceneOverlayScrollContainer()
        {
            Add(scroll = new OverlayScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Container
                {
                    Height = 3000,
                    RelativeSizeAxes = Axes.X,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray
                    }
                }
            });
        }

        [Test]
        public void TestButtonVisibility()
        {
            AddStep("Scroll to start", () => scroll.ScrollToStart(false));
            AddWaitStep("Wait for animation", 3);
            AddAssert("Button is hidden", () => scroll.Button.Alpha == 0);
            AddStep("Scroll to end", () => scroll.ScrollToEnd(false));
            AddWaitStep("Wait for animation", 3);
            AddAssert("Button is visible", () => scroll.Button.Alpha == 1);
        }

        [Test]
        public void TestButtonAction()
        {
            AddStep("Scroll to end", () => scroll.ScrollToEnd(false));
            AddStep("Click button", () => scroll.Button.Click());
            AddWaitStep("Wait for animation", 7);
            AddAssert("Scroll position is top", () => Precision.AlmostEquals(scroll.Current, 0, 0.001f));
        }
    }
}
