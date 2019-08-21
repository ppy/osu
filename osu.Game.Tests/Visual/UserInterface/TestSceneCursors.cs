// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneCursors : ManualInputManagerTestScene
    {
        private readonly MenuCursorContainer menuCursorContainer;
        private readonly CustomCursorBox[] cursorBoxes = new CustomCursorBox[6];

        public TestSceneCursors()
        {
            Child = menuCursorContainer = new MenuCursorContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    // Middle user
                    cursorBoxes[0] = new CustomCursorBox(Color4.Green)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f),
                    },
                    // Top-left user
                    cursorBoxes[1] = new CustomCursorBox(Color4.Blue)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.4f)
                    },
                    // Bottom-right user
                    cursorBoxes[2] = new CustomCursorBox(Color4.Red)
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.4f)
                    },
                    // Bottom-left local
                    cursorBoxes[3] = new CustomCursorBox(Color4.Magenta, false)
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.4f)
                    },
                    // Top-right local
                    cursorBoxes[4] = new CustomCursorBox(Color4.Cyan, false)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.4f)
                    },
                    // Left-local
                    cursorBoxes[5] = new CustomCursorBox(Color4.Yellow, false)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.2f, 1),
                    },
                }
            };

            AddToggleStep("Smooth transitions", b => cursorBoxes.ForEach(box => box.SmoothTransition = b));

            testUserCursor();
            testLocalCursor();
            testUserCursorOverride();
            testMultipleLocalCursors();
        }

        /// <summary>
        /// -- Green Box --
        /// Tests whether hovering in and out of a drawable that provides the user cursor (green)
        /// results in the correct visibility state for that cursor.
        /// </summary>
        private void testUserCursor()
        {
            AddStep("Move to green area", () => InputManager.MoveMouseTo(cursorBoxes[0]));
            AddAssert("Check green cursor visible", () => checkVisible(cursorBoxes[0].Cursor));
            AddAssert("Check green cursor at mouse", () => checkAtMouse(cursorBoxes[0].Cursor));
            AddStep("Move out", moveOut);
            AddAssert("Check green cursor invisible", () => !checkVisible(cursorBoxes[0].Cursor));
            AddAssert("Check global cursor visible", () => checkVisible(menuCursorContainer.Cursor));
        }

        /// <summary>
        /// -- Purple Box --
        /// Tests whether hovering in and out of a drawable that provides a local cursor (purple)
        /// results in the correct visibility and state for that cursor.
        /// </summary>
        private void testLocalCursor()
        {
            AddStep("Move to purple area", () => InputManager.MoveMouseTo(cursorBoxes[3]));
            AddAssert("Check purple cursor visible", () => checkVisible(cursorBoxes[3].Cursor));
            AddAssert("Check purple cursor at mouse", () => checkAtMouse(cursorBoxes[3].Cursor));
            AddAssert("Check global cursor visible", () => checkVisible(menuCursorContainer.Cursor));
            AddAssert("Check global cursor at mouse", () => checkAtMouse(menuCursorContainer.Cursor));
            AddStep("Move out", moveOut);
            AddAssert("Check purple cursor visible", () => checkVisible(cursorBoxes[3].Cursor));
            AddAssert("Check global cursor visible", () => checkVisible(menuCursorContainer.Cursor));
        }

        /// <summary>
        /// -- Blue-Green Box Boundary --
        /// Tests whether overriding a user cursor (green) with another user cursor (blue)
        /// results in the correct visibility and states for the cursors.
        /// </summary>
        private void testUserCursorOverride()
        {
            AddStep("Move to blue-green boundary", () => InputManager.MoveMouseTo(cursorBoxes[1].ScreenSpaceDrawQuad.BottomRight - new Vector2(10)));
            AddAssert("Check blue cursor visible", () => checkVisible(cursorBoxes[1].Cursor));
            AddAssert("Check green cursor invisible", () => !checkVisible(cursorBoxes[0].Cursor));
            AddAssert("Check blue cursor at mouse", () => checkAtMouse(cursorBoxes[1].Cursor));
            AddStep("Move out", moveOut);
            AddAssert("Check blue cursor not visible", () => !checkVisible(cursorBoxes[1].Cursor));
            AddAssert("Check green cursor not visible", () => !checkVisible(cursorBoxes[0].Cursor));
        }

        /// <summary>
        /// -- Yellow-Purple Box Boundary --
        /// Tests whether multiple local cursors (purple + yellow) may be visible and at the mouse position at the same time.
        /// </summary>
        private void testMultipleLocalCursors()
        {
            AddStep("Move to yellow-purple boundary", () => InputManager.MoveMouseTo(cursorBoxes[5].ScreenSpaceDrawQuad.BottomRight - new Vector2(10)));
            AddAssert("Check purple cursor visible", () => checkVisible(cursorBoxes[3].Cursor));
            AddAssert("Check purple cursor at mouse", () => checkAtMouse(cursorBoxes[3].Cursor));
            AddAssert("Check yellow cursor visible", () => checkVisible(cursorBoxes[5].Cursor));
            AddAssert("Check yellow cursor at mouse", () => checkAtMouse(cursorBoxes[5].Cursor));
            AddStep("Move out", moveOut);
            AddAssert("Check purple cursor visible", () => checkVisible(cursorBoxes[3].Cursor));
            AddAssert("Check yellow cursor visible", () => checkVisible(cursorBoxes[5].Cursor));
        }

        /// <summary>
        /// -- Yellow-Blue Box Boundary --
        /// Tests whether a local cursor (yellow) may be displayed along with a user cursor override (blue).
        /// </summary>
        private void testUserOverrideWithLocal()
        {
            AddStep("Move to yellow-blue boundary", () => InputManager.MoveMouseTo(cursorBoxes[5].ScreenSpaceDrawQuad.TopRight - new Vector2(10)));
            AddAssert("Check blue cursor visible", () => checkVisible(cursorBoxes[1].Cursor));
            AddAssert("Check blue cursor at mouse", () => checkAtMouse(cursorBoxes[1].Cursor));
            AddAssert("Check yellow cursor visible", () => checkVisible(cursorBoxes[5].Cursor));
            AddAssert("Check yellow cursor at mouse", () => checkAtMouse(cursorBoxes[5].Cursor));
            AddStep("Move out", moveOut);
            AddAssert("Check blue cursor invisible", () => !checkVisible(cursorBoxes[1].Cursor));
            AddAssert("Check yellow cursor visible", () => checkVisible(cursorBoxes[5].Cursor));
        }

        /// <summary>
        /// Moves the cursor to a point not covered by any cursor containers.
        /// </summary>
        private void moveOut()
            => InputManager.MoveMouseTo(new Vector2(InputManager.ScreenSpaceDrawQuad.Centre.X, InputManager.ScreenSpaceDrawQuad.TopLeft.Y));

        /// <summary>
        /// Checks if a cursor is visible.
        /// </summary>
        /// <param name="cursorContainer">The cursor to check.</param>
        private bool checkVisible(CursorContainer cursorContainer) => cursorContainer.State.Value == Visibility.Visible;

        /// <summary>
        /// Checks if a cursor is at the current inputmanager screen position.
        /// </summary>
        /// <param name="cursorContainer">The cursor to check.</param>
        private bool checkAtMouse(CursorContainer cursorContainer)
            => Precision.AlmostEquals(InputManager.CurrentState.Mouse.Position, cursorContainer.ToScreenSpace(cursorContainer.ActiveCursor.DrawPosition));

        private class CustomCursorBox : Container, IProvideCursor
        {
            public bool SmoothTransition;

            public CursorContainer Cursor { get; }
            public bool ProvidingUserCursor { get; }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) || (SmoothTransition && !ProvidingUserCursor);

            private readonly Box background;

            public CustomCursorBox(Color4 cursorColour, bool providesUserCursor = true)
            {
                ProvidingUserCursor = providesUserCursor;

                Colour = cursorColour;
                Masking = true;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = providesUserCursor ? "User cursor" : "Local cursor"
                    },
                    Cursor = new TestCursorContainer
                    {
                        State = { Value = providesUserCursor ? Visibility.Hidden : Visibility.Visible },
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeTo(0.4f, 250, Easing.OutQuint);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeTo(0.1f, 250);
                base.OnHoverLost(e);
            }
        }

        private class TestCursorContainer : CursorContainer
        {
            protected override Drawable CreateCursor() => new TestCursor();

            private class TestCursor : CircularContainer
            {
                public TestCursor()
                {
                    Origin = Anchor.Centre;

                    Size = new Vector2(50);
                    Masking = true;

                    Blending = BlendingParameters.Additive;
                    Alpha = 0.5f;

                    Child = new Box { RelativeSizeAxes = Axes.Both };
                }
            }
        }
    }
}
