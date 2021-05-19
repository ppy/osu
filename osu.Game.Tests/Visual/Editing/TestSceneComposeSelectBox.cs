// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneComposeSelectBox : OsuManualInputManagerTestScene
    {
        private Container selectionArea;
        private SelectionBox selectionBox;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = selectionArea = new Container
            {
                Size = new Vector2(400),
                Position = -new Vector2(150),
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    selectionBox = new SelectionBox
                    {
                        RelativeSizeAxes = Axes.Both,

                        CanRotate = true,
                        CanScaleX = true,
                        CanScaleY = true,

                        OnRotation = handleRotation,
                        OnScale = handleScale
                    }
                }
            };

            InputManager.MoveMouseTo(selectionBox);
            InputManager.ReleaseButton(MouseButton.Left);
        });

        private bool handleScale(Vector2 amount, Anchor reference)
        {
            if ((reference & Anchor.y1) == 0)
            {
                int directionY = (reference & Anchor.y0) > 0 ? -1 : 1;
                if (directionY < 0)
                    selectionArea.Y += amount.Y;
                selectionArea.Height += directionY * amount.Y;
            }

            if ((reference & Anchor.x1) == 0)
            {
                int directionX = (reference & Anchor.x0) > 0 ? -1 : 1;
                if (directionX < 0)
                    selectionArea.X += amount.X;
                selectionArea.Width += directionX * amount.X;
            }

            return true;
        }

        private bool handleRotation(float angle)
        {
            // kinda silly and wrong, but just showing that the drag handles work.
            selectionArea.Rotation += angle;
            return true;
        }

        [Test]
        public void TestRotationHandleShownOnHover()
        {
            SelectionBoxRotationHandle rotationHandle = null;

            AddStep("retrieve rotation handle", () => rotationHandle = this.ChildrenOfType<SelectionBoxRotationHandle>().First());

            AddAssert("handle hidden", () => rotationHandle.Alpha == 0);
            AddStep("hover over handle", () => InputManager.MoveMouseTo(rotationHandle));
            AddUntilStep("rotation handle shown", () => rotationHandle.Alpha == 1);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(selectionBox));
            AddUntilStep("handle hidden", () => rotationHandle.Alpha == 0);
        }

        [Test]
        public void TestRotationHandleShownOnHoveringClosestScaleHandler()
        {
            SelectionBoxRotationHandle rotationHandle = null;

            AddStep("retrieve rotation handle", () => rotationHandle = this.ChildrenOfType<SelectionBoxRotationHandle>().First());

            AddAssert("rotation handle hidden", () => rotationHandle.Alpha == 0);
            AddStep("hover over closest scale handle", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxScaleHandle>().Single(s => s.Anchor == rotationHandle.Anchor));
            });
            AddUntilStep("rotation handle shown", () => rotationHandle.Alpha == 1);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(selectionBox));
            AddUntilStep("handle hidden", () => rotationHandle.Alpha == 0);
        }

        [Test]
        public void TestHoverRotationHandleFromScaleHandle()
        {
            SelectionBoxRotationHandle rotationHandle = null;

            AddStep("retrieve rotation handle", () => rotationHandle = this.ChildrenOfType<SelectionBoxRotationHandle>().First());

            AddAssert("rotation handle hidden", () => rotationHandle.Alpha == 0);
            AddStep("hover over closest scale handle", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxScaleHandle>().Single(s => s.Anchor == rotationHandle.Anchor));
            });
            AddUntilStep("rotation handle shown", () => rotationHandle.Alpha == 1);
            AddAssert("rotation handle not hovered", () => !rotationHandle.IsHovered);

            AddStep("hover over rotation handle", () => InputManager.MoveMouseTo(rotationHandle));
            AddAssert("rotation handle still shown", () => rotationHandle.Alpha == 1);
            AddAssert("rotation handle hovered", () => rotationHandle.IsHovered);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(selectionBox));
            AddUntilStep("handle hidden", () => rotationHandle.Alpha == 0);
        }

        [Test]
        public void TestHoldingScaleHandleHidesCorrespondingRotationHandle()
        {
            SelectionBoxRotationHandle rotationHandle = null;

            AddStep("retrieve rotation handle", () => rotationHandle = this.ChildrenOfType<SelectionBoxRotationHandle>().First());

            AddAssert("rotation handle hidden", () => rotationHandle.Alpha == 0);
            AddStep("hover over closest scale handle", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxScaleHandle>().Single(s => s.Anchor == rotationHandle.Anchor));
            });
            AddUntilStep("rotation handle shown", () => rotationHandle.Alpha == 1);
            AddStep("hold scale handle", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep("rotation handle hidden", () => rotationHandle.Alpha == 0);

            int i;
            ScheduledDelegate mouseMove = null;

            AddStep("start dragging", () =>
            {
                i = 0;

                mouseMove = Scheduler.AddDelayed(() =>
                {
                    InputManager.MoveMouseTo(selectionBox.ScreenSpaceDrawQuad.TopLeft + Vector2.One * (5 * ++i));
                }, 100, true);
            });
            AddAssert("rotation handle still hidden", () => rotationHandle.Alpha == 0);

            AddStep("end dragging", () => mouseMove.Cancel());
            AddAssert("rotation handle still hidden", () => rotationHandle.Alpha == 0);
            AddStep("unhold left", () => InputManager.ReleaseButton(MouseButton.Left));
            AddUntilStep("rotation handle shown", () => rotationHandle.Alpha == 1);
            AddStep("move mouse away", () => InputManager.MoveMouseTo(selectionBox, new Vector2(20)));
            AddUntilStep("rotation handle hidden", () => rotationHandle.Alpha == 0);
        }

        /// <summary>
        /// Tests that hovering over two handles instantaneously from one to another does not crash or cause issues to the visibility state.
        /// </summary>
        [Test]
        public void TestHoverOverTwoHandlesInstantaneously()
        {
            AddStep("hover over top-left scale handle", () =>
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxScaleHandle>().Single(s => s.Anchor == Anchor.TopLeft)));
            AddStep("hover over top-right scale handle", () =>
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxScaleHandle>().Single(s => s.Anchor == Anchor.TopRight)));
            AddUntilStep("top-left rotation handle hidden", () =>
                this.ChildrenOfType<SelectionBoxRotationHandle>().Single(r => r.Anchor == Anchor.TopLeft).Alpha == 0);
            AddUntilStep("top-right rotation handle shown", () =>
                this.ChildrenOfType<SelectionBoxRotationHandle>().Single(r => r.Anchor == Anchor.TopRight).Alpha == 1);
        }
    }
}
