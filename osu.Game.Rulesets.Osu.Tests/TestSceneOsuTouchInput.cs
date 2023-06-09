// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneOsuTouchInput : OsuTouchableTestScene
    {
        [Test]
        public void TestStreamInputVisual()
        {
            AddHitCircleAtStep(TouchSource.Touch1);
            AddHitCircleAtStep(TouchSource.Touch2);

            BeginTouch(TouchSource.Touch1);
            BeginTouch(TouchSource.Touch2);

            EndTouch(TouchSource.Touch1);

            int i = 0;

            AddRepeatStep("Alternate", () =>
            {
                TouchSource down = i % 2 == 0 ? TouchSource.Touch3 : TouchSource.Touch4;
                TouchSource up = i % 2 == 0 ? TouchSource.Touch4 : TouchSource.Touch3;

                // sometimes the user will end the previous touch before touching again, sometimes not.
                if (RNG.NextBool())
                {
                    InputManager.BeginTouch(new Touch(down, GetSanePositionForSource(down)));
                    InputManager.EndTouch(new Touch(up, GetSanePositionForSource(up)));
                }
                else
                {
                    InputManager.EndTouch(new Touch(up, GetSanePositionForSource(up)));
                    InputManager.BeginTouch(new Touch(down, GetSanePositionForSource(down)));
                }

                i++;
            }, 100);
        }

        [Test]
        public void TestSimpleInput()
        {
            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            // Subsequent touches should be ignored (except position).
            BeginTouch(TouchSource.Touch3);
            checkPosition(TouchSource.Touch3);

            BeginTouch(TouchSource.Touch4);
            checkPosition(TouchSource.Touch4);

            assertKeyCounter(1, 1);

            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            assertKeyCounter(1, 1);
        }

        [Test]
        public void TestPositionalTrackingAfterLongDistanceTravelled()
        {
            // When a single touch has already travelled enough distance on screen, it should remain as the positional
            // tracking touch until released (unless a direct touch occurs).

            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // cover some distance
            BeginTouch(TouchSource.Touch1, new Vector2(0));
            BeginTouch(TouchSource.Touch1, new Vector2(9999));
            BeginTouch(TouchSource.Touch1, new Vector2(0));
            BeginTouch(TouchSource.Touch1, new Vector2(9999));
            BeginTouch(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkNotPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // in this case, touch 2 should not become the positional tracking touch.
            checkPosition(TouchSource.Touch1);

            // even if the second touch moves on the screen, the original tracking touch is retained.
            BeginTouch(TouchSource.Touch2, new Vector2(0));
            BeginTouch(TouchSource.Touch2, new Vector2(9999));
            BeginTouch(TouchSource.Touch2, new Vector2(0));
            BeginTouch(TouchSource.Touch2, new Vector2(9999));

            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestPositionalInputUpdatesOnlyFromMostRecentTouch()
        {
            BeginTouch(TouchSource.Touch1);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);
            checkPosition(TouchSource.Touch2);

            BeginTouch(TouchSource.Touch1, Vector2.One);
            checkPosition(TouchSource.Touch2);

            EndTouch(TouchSource.Touch2);
            checkPosition(TouchSource.Touch2);

            // note that touch1 was never ended, but is no longer valid for touch input due to touch 2 occurring.
            BeginTouch(TouchSource.Touch1);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestStreamInput()
        {
            // In this scenario, the user is tapping on the first object in a stream,
            // then using one or two fingers in empty space to continue the stream.

            AddHitCircleAtStep(TouchSource.Touch1);
            BeginTouch(TouchSource.Touch1);

            // The first touch is handled as normal.
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // The second touch should release the first, and also act as a right button.
            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            // Importantly, this is different from the simple case because an object was interacted with in the first touch, but not the second touch.
            // left button is automatically released.
            checkNotPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Also importantly, the positional part of the second touch is ignored.
            checkPosition(TouchSource.Touch1);

            // In this scenario, a third touch should be allowed, and handled similarly to the second.
            BeginTouch(TouchSource.Touch3);

            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            EndTouch(TouchSource.Touch2);

            checkPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            // User continues streaming
            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            // In this mode a maximum of three touches should be supported.
            // A fourth touch should result in no changes anywhere.
            BeginTouch(TouchSource.Touch4);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);
            EndTouch(TouchSource.Touch4);
        }

        [Test]
        public void TestStreamInputWithInitialTouchDownLeft()
        {
            // In this scenario, the user is wanting to use stream input but we start with one finger still on the screen.
            // That finger is mapped to a left action.

            AddHitCircleAtStep(TouchSource.Touch2);

            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // hits circle as right action
            BeginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            EndTouch(TouchSource.Touch1);
            checkNotPressed(OsuAction.LeftButton);

            // stream using other two fingers while touch2 tracks
            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            // right button is automatically released
            checkNotPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            BeginTouch(TouchSource.Touch3);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            EndTouch(TouchSource.Touch1);
            checkNotPressed(OsuAction.LeftButton);

            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(3, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestStreamInputWithInitialTouchDownRight()
        {
            // In this scenario, the user is wanting to use stream input but we start with one finger still on the screen.
            // That finger is mapped to a right action.

            BeginTouch(TouchSource.Touch1);
            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            EndTouch(TouchSource.Touch1);

            AddHitCircleAtStep(TouchSource.Touch1);

            // hits circle as left action
            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);

            EndTouch(TouchSource.Touch2);

            // stream using other two fingers while touch1 tracks
            BeginTouch(TouchSource.Touch2);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.RightButton);
            // left button is automatically released
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch3);
            assertKeyCounter(3, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);

            EndTouch(TouchSource.Touch2);
            checkNotPressed(OsuAction.RightButton);

            BeginTouch(TouchSource.Touch2);
            assertKeyCounter(3, 3);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestNonStreamOverlappingDirectTouchesWithRelease()
        {
            // In this scenario, the user is tapping on three circles directly while correctly releasing the first touch.
            // All three should be recognised.

            AddHitCircleAtStep(TouchSource.Touch1);
            AddHitCircleAtStep(TouchSource.Touch2);
            AddHitCircleAtStep(TouchSource.Touch3);

            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            EndTouch(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch3);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch3);
        }

        [Test]
        public void TestNonStreamOverlappingDirectTouchesWithoutRelease()
        {
            // In this scenario, the user is tapping on three circles directly without releasing any touches.
            // The first two should be recognised, but a third should not (as the user already has two fingers down).

            AddHitCircleAtStep(TouchSource.Touch1);
            AddHitCircleAtStep(TouchSource.Touch2);
            AddHitCircleAtStep(TouchSource.Touch3);

            BeginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            BeginTouch(TouchSource.Touch3);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch3);
        }

        [Test]
        public void TestMovementWhileDisallowed()
        {
            // aka "autopilot" mod

            AddStep("Disallow gameplay cursor movement", () => OsuInputManager.AllowUserCursorMovement = false);

            Vector2? positionBefore = null;

            AddStep("Store cursor position", () => positionBefore = OsuInputManager.CurrentState.Mouse.Position);
            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            AddAssert("Cursor position unchanged", () => OsuInputManager.CurrentState.Mouse.Position, () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestActionWhileDisallowed()
        {
            // aka "relax" mod
            AddStep("Disallow gameplay actions", () => OsuInputManager.AllowGameplayInputs = false);

            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestInputWhileMouseButtonsDisabled()
        {
            AddStep("Disable mouse buttons", () => Config.SetValue(OsuSetting.MouseDisableButtons, true));

            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestAlternatingInput()
        {
            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);

            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            for (int i = 0; i < 2; i++)
            {
                EndTouch(TouchSource.Touch1);

                checkPressed(OsuAction.RightButton);
                checkNotPressed(OsuAction.LeftButton);

                BeginTouch(TouchSource.Touch1);

                checkPressed(OsuAction.LeftButton);
                checkPressed(OsuAction.RightButton);

                EndTouch(TouchSource.Touch2);

                checkPressed(OsuAction.LeftButton);
                checkNotPressed(OsuAction.RightButton);

                BeginTouch(TouchSource.Touch2);

                checkPressed(OsuAction.LeftButton);
                checkPressed(OsuAction.RightButton);
            }
        }

        [Test]
        public void TestPressReleaseOrder()
        {
            BeginTouch(TouchSource.Touch1);
            BeginTouch(TouchSource.Touch2);
            BeginTouch(TouchSource.Touch3);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            // Touch 3 was ignored, but let's ensure that if 1 or 2 are released, 3 will be handled a second attempt.
            EndTouch(TouchSource.Touch1);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            EndTouch(TouchSource.Touch3);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            BeginTouch(TouchSource.Touch3);

            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
        }

        [Test]
        public void TestWithDisallowedUserCursor()
        {
            BeginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);

            BeginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            // Subsequent touches should be ignored.
            BeginTouch(TouchSource.Touch3);
            BeginTouch(TouchSource.Touch4);

            assertKeyCounter(1, 1);

            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            assertKeyCounter(1, 1);
        }

        private void checkPosition(TouchSource touchSource) =>
            AddAssert("Cursor position is correct", () => OsuInputManager.CurrentState.Mouse.Position, () => Is.EqualTo(GetSanePositionForSource(touchSource)));

        private void assertKeyCounter(int left, int right)
        {
            AddAssert($"The left key was pressed {left} times", () => LeftKeyCounter.CountPresses.Value, () => Is.EqualTo(left));
            AddAssert($"The right key was pressed {right} times", () => RightKeyCounter.CountPresses.Value, () => Is.EqualTo(right));
        }

        private void checkNotPressed(OsuAction action) => AddAssert($"Not pressing {action}", () => !OsuInputManager.PressedActions.Contains(action));
        private void checkPressed(OsuAction action) => AddAssert($"Is pressing {action}", () => OsuInputManager.PressedActions.Contains(action));
    }
}
