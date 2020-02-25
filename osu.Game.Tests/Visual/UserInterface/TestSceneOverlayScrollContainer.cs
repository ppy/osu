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

        private OverlayScrollContainer scroll;

        [SetUp]
        public void SetUp() => Schedule(() =>
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
        });

        [Test]
        public void TestButtonVisibility()
        {
            AddAssert("button is hidden", () => scroll.Button.State.Value == Visibility.Hidden);

            AddStep("scroll to end", () => scroll.ScrollToEnd(false));
            AddAssert("button is visible", () => scroll.Button.State.Value == Visibility.Visible);

            AddStep("scroll to start", () => scroll.ScrollToStart(false));
            AddAssert("button is hidden", () => scroll.Button.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestButtonAction()
        {
            AddStep("scroll to end", () => scroll.ScrollToEnd(false));

            AddStep("invoke action", () => scroll.Button.Action.Invoke());

            AddUntilStep("scrolled back to start", () => Precision.AlmostEquals(scroll.Current, 0, 0.1f));
        }
    }
}
