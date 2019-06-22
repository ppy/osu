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
    public class TestSceneIdleTracker : ManualInputManagerTestScene
    {
        private readonly IdleTrackingBox box1;
        private readonly IdleTrackingBox box2;
        private readonly IdleTrackingBox box3;
        private readonly IdleTrackingBox box4;

        public TestSceneIdleTracker()
        {
            Children = new Drawable[]
            {
                box1 = new IdleTrackingBox(1000)
                {
                    Name = "TopLeft",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Red,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                box2 = new IdleTrackingBox(2000)
                {
                    Name = "TopRight",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Green,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                box3 = new IdleTrackingBox(3000)
                {
                    Name = "BottomLeft",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Blue,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                box4 = new IdleTrackingBox(4000)
                {
                    Name = "BottomRight",
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
            AddStep("move to top left", () => InputManager.MoveMouseTo(box1));

            waitForAllIdle();

            AddStep("nudge mouse", () => InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.Centre + new Vector2(1)));

            checkIdleStatus(box1, false);
            checkIdleStatus(box2, true);
            checkIdleStatus(box3, true);
            checkIdleStatus(box4, true);
        }

        [Test]
        public void TestMovement()
        {
            AddStep("move to top right", () => InputManager.MoveMouseTo(box2));

            checkIdleStatus(box1, true);
            checkIdleStatus(box2, false);
            checkIdleStatus(box3, true);
            checkIdleStatus(box4, true);

            AddStep("move to bottom left", () => InputManager.MoveMouseTo(box3));
            AddStep("move to bottom right", () => InputManager.MoveMouseTo(box4));

            checkIdleStatus(box1, true);
            checkIdleStatus(box2, false);
            checkIdleStatus(box3, false);
            checkIdleStatus(box4, false);

            waitForAllIdle();
        }

        [Test]
        public void TestTimings()
        {
            AddStep("move to centre", () => InputManager.MoveMouseTo(Content));

            checkIdleStatus(box1, false);
            checkIdleStatus(box2, false);
            checkIdleStatus(box3, false);
            checkIdleStatus(box4, false);

            AddUntilStep("Wait for idle", () => box1.IsIdle);

            checkIdleStatus(box1, true);
            checkIdleStatus(box2, false);
            checkIdleStatus(box3, false);
            checkIdleStatus(box4, false);

            AddUntilStep("Wait for idle", () => box2.IsIdle);

            checkIdleStatus(box1, true);
            checkIdleStatus(box2, true);
            checkIdleStatus(box3, false);
            checkIdleStatus(box4, false);

            AddUntilStep("Wait for idle", () => box3.IsIdle);

            checkIdleStatus(box1, true);
            checkIdleStatus(box2, true);
            checkIdleStatus(box3, true);
            checkIdleStatus(box4, false);

            waitForAllIdle();
        }

        private void checkIdleStatus(IdleTrackingBox box, bool expectedIdle)
        {
            AddAssert($"{box.Name} is {(expectedIdle ? "idle" : "active")}", () => box.IsIdle == expectedIdle);
        }

        private void waitForAllIdle()
        {
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
