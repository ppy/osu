// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Input;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Components
{
    [TestFixture]
    public class TestCaseIdleTracker : ManualInputManagerTestCase
    {
        private readonly IdleTrackingBox box1;
        private readonly IdleTrackingBox box2;
        private readonly IdleTrackingBox box3;
        private readonly IdleTrackingBox box4;

        public TestCaseIdleTracker()
        {
            Children = new Drawable[]
            {
                box1 = new IdleTrackingBox(1000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Red,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                box2 = new IdleTrackingBox(2000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Green,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                box3 = new IdleTrackingBox(3000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Blue,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                box4 = new IdleTrackingBox(4000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Orange,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
            };
        }

        [Test]
        public void TestNudge()
        {
            AddStep("move mouse to top left", () => InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.Centre));

            AddUntilStep("Wait for all idle", () => box1.IsIdle && box2.IsIdle && box3.IsIdle && box4.IsIdle);

            AddStep("nudge mouse", () => InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.Centre + new Vector2(1)));

            AddAssert("check not idle", () => !box1.IsIdle);
            AddAssert("check idle", () => box2.IsIdle);
            AddAssert("check idle", () => box3.IsIdle);
            AddAssert("check idle", () => box4.IsIdle);
        }

        [Test]
        public void TestMovement()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(box2.ScreenSpaceDrawQuad.Centre));

            AddAssert("check not idle", () => box1.IsIdle);
            AddAssert("check not idle", () => !box2.IsIdle);
            AddAssert("check idle", () => box3.IsIdle);
            AddAssert("check idle", () => box4.IsIdle);

            AddStep("move mouse", () => InputManager.MoveMouseTo(box3.ScreenSpaceDrawQuad.Centre));
            AddStep("move mouse", () => InputManager.MoveMouseTo(box4.ScreenSpaceDrawQuad.Centre));

            AddAssert("check not idle", () => box1.IsIdle);
            AddAssert("check not idle", () => !box2.IsIdle);
            AddAssert("check idle", () => !box3.IsIdle);
            AddAssert("check idle", () => !box4.IsIdle);

            AddUntilStep("Wait for all idle", () => box1.IsIdle && box2.IsIdle && box3.IsIdle && box4.IsIdle);
        }

        [Test]
        public void TestTimings()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));

            AddAssert("check not idle", () => !box1.IsIdle && !box2.IsIdle && !box3.IsIdle && !box4.IsIdle);
            AddUntilStep("Wait for idle", () => box1.IsIdle);
            AddAssert("check not idle", () => !box2.IsIdle && !box3.IsIdle && !box4.IsIdle);
            AddUntilStep("Wait for idle", () => box2.IsIdle);
            AddAssert("check not idle", () => !box3.IsIdle && !box4.IsIdle);
            AddUntilStep("Wait for idle", () => box3.IsIdle);

            AddUntilStep("Wait for all idle", () => box1.IsIdle && box2.IsIdle && box3.IsIdle && box4.IsIdle);
        }

        private class IdleTrackingBox : CompositeDrawable
        {
            private readonly IdleTracker idleTracker;

            public bool IsIdle => idleTracker.IsIdle.Value;

            public IdleTrackingBox(double timeToIdle)
            {
                Box box;

                Alpha = 0.6f;
                Scale = new Vector2(0.6f);

                InternalChildren = new Drawable[]
                {
                    idleTracker = new IdleTracker(timeToIdle),
                    box = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                };

                idleTracker.IsIdle.BindValueChanged(idle => box.Colour = idle.NewValue ? Color4.White : Color4.Black, true);
            }
        }
    }
}
