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
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOverlayScrollContainer : OsuManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlayScrollContainer)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private OverlayScrollContainer scroll;

        private int invocationCount;

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

            invocationCount = 0;

            scroll.Button.Action += () => invocationCount++;
        });

        [Test]
        public void TestButtonVisibility()
        {
            AddAssert("button is hidden", () => scroll.Button.State == Visibility.Hidden);

            AddStep("scroll to end", () => scroll.ScrollToEnd(false));
            AddAssert("button is visible", () => scroll.Button.State == Visibility.Visible);

            AddStep("scroll to start", () => scroll.ScrollToStart(false));
            AddAssert("button is hidden", () => scroll.Button.State == Visibility.Hidden);
        }

        [Test]
        public void TestButtonAction()
        {
            AddStep("scroll to end", () => scroll.ScrollToEnd(false));

            AddStep("invoke action", () => scroll.Button.Action.Invoke());

            AddUntilStep("scrolled back to start", () => Precision.AlmostEquals(scroll.Current, 0, 0.1f));
        }

        [Test]
        public void TestClick()
        {
            AddStep("scroll to end", () => scroll.ScrollToEnd(false));

            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(scroll.Button);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("scrolled back to start", () => Precision.AlmostEquals(scroll.Current, 0, 0.1f));
        }

        [Test]
        public void TestMultipleClicks()
        {
            AddStep("scroll to end", () => scroll.ScrollToEnd(false));

            AddAssert("invocation count is 0", () => invocationCount == 0);

            AddStep("hover button", () => InputManager.MoveMouseTo(scroll.Button));
            AddRepeatStep("click button", () => InputManager.Click(MouseButton.Left), 3);

            AddAssert("invocation count is 1", () => invocationCount == 1);
        }
    }
}
