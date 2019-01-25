﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseZoomableScrollContainer : ManualInputManagerTestCase
    {
        private readonly ZoomableScrollContainer scrollContainer;
        private readonly Drawable innerBox;

        public TestCaseZoomableScrollContainer()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = 250,
                    Width = 0.75f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(30)
                        },
                        scrollContainer = new ZoomableScrollContainer { RelativeSizeAxes = Axes.Both }
                    }
                },
                new MenuCursor()
            };

            scrollContainer.Add(innerBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientHorizontal(new Color4(0.8f, 0.6f, 0.4f, 1f), new Color4(0.4f, 0.6f, 0.8f, 1f))
            });
        }

        [Test]
        public void TestZoom0()
        {
            reset();
            AddAssert("Box at 0", () => Precision.AlmostEquals(boxQuad.TopLeft, scrollQuad.TopLeft));
            AddAssert("Box width = 1x", () => Precision.AlmostEquals(boxQuad.Size, scrollQuad.Size));
        }

        [Test]
        public void TestZoom10()
        {
            reset();
            AddStep("Set zoom = 10", () => scrollContainer.Zoom = 10);
            AddAssert("Box at 1/2", () => Precision.AlmostEquals(boxQuad.Centre, scrollQuad.Centre, 1));
            AddAssert("Box width = 10x", () => Precision.AlmostEquals(boxQuad.Size.X, 10 * scrollQuad.Size.X));
        }

        [Test]
        public void TestMouseZoomInOnceOutOnce()
        {
            reset();

            // Scroll in at 0.25
            AddStep("Move mouse to 0.25x", () => InputManager.MoveMouseTo(new Vector2(scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X, scrollQuad.Centre.Y)));
            AddStep("Scroll by 3", () => InputManager.ScrollBy(new Vector2(0, 3)));
            AddAssert("Box not at 0", () => !Precision.AlmostEquals(boxQuad.TopLeft, scrollQuad.TopLeft));
            AddAssert("Box 1/4 at 1/4", () => Precision.AlmostEquals(boxQuad.TopLeft.X + 0.25f * boxQuad.Size.X, scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X));

            // Scroll out at 0.25
            AddStep("Scroll by -3", () => InputManager.ScrollBy(new Vector2(0, -3)));
            AddAssert("Box at 0", () => Precision.AlmostEquals(boxQuad.TopLeft, scrollQuad.TopLeft));
            AddAssert("Box 1/4 at 1/4", () => Precision.AlmostEquals(boxQuad.TopLeft.X + 0.25f * boxQuad.Size.X, scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X));
        }

        [Test]
        public void TestMouseZoomInTwiceOutTwice()
        {
            reset();

            // Scroll in at 0.25
            AddStep("Move mouse to 0.25x", () => InputManager.MoveMouseTo(new Vector2(scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X, scrollQuad.Centre.Y)));
            AddStep("Scroll by 1", () => InputManager.ScrollBy(new Vector2(0, 1)));

            // Scroll in at 0.6
            AddStep("Move mouse to 0.75x", () => InputManager.MoveMouseTo(new Vector2(scrollQuad.TopLeft.X + 0.75f * scrollQuad.Size.X, scrollQuad.Centre.Y)));
            AddStep("Scroll by 1", () => InputManager.ScrollBy(new Vector2(0, 1)));
            AddAssert("Box not at 0", () => !Precision.AlmostEquals(boxQuad.TopLeft, scrollQuad.TopLeft));

            // Very hard to determine actual position, so approximate
            AddAssert("Box at correct position (1)", () => Precision.DefinitelyBigger(scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X, boxQuad.TopLeft.X + 0.25f * boxQuad.Size.X));
            AddAssert("Box at correct position (2)", () => Precision.DefinitelyBigger(scrollQuad.TopLeft.X + 0.6f * scrollQuad.Size.X, boxQuad.TopLeft.X + 0.3f * boxQuad.Size.X));
            AddAssert("Box at correct position (3)", () => Precision.DefinitelyBigger(boxQuad.TopLeft.X + 0.6f * boxQuad.Size.X, scrollQuad.TopLeft.X + 0.6f * scrollQuad.Size.X));

            // Scroll out at 0.6
            AddStep("Scroll by -1", () => InputManager.ScrollBy(new Vector2(0, -1)));

            // Scroll out at 0.25
            AddStep("Move mouse to 0.25x", () => InputManager.MoveMouseTo(new Vector2(scrollQuad.TopLeft.X + 0.25f * scrollQuad.Size.X, scrollQuad.Centre.Y)));
            AddStep("Scroll by -1", () => InputManager.ScrollBy(new Vector2(0, -1)));
            AddAssert("Box at 0", () => Precision.AlmostEquals(boxQuad.TopLeft, scrollQuad.TopLeft));
        }

        private void reset()
        {
            AddStep("Reset", () =>
            {
                scrollContainer.Zoom = 0;
                scrollContainer.ScrollTo(0, false);
            });
        }

        private Quad scrollQuad => scrollContainer.ScreenSpaceDrawQuad;
        private Quad boxQuad => innerBox.ScreenSpaceDrawQuad;
    }
}
