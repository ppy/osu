// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().First();
        private OsuTouchInputMapper touchInputMapper => osuInputManager.TouchInputMapper;

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, osuInputManager.ScreenSpaceDrawQuad.Centre));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, osuInputManager.ScreenSpaceDrawQuad.Centre));

        [Test]
        public void TestTouchInput()
        {
            // Cursor touch
            AddStep("Touch with cursor finger", () => touch(TouchSource.Touch1));

            AddAssert("The touch is a cursor touch", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Left button touch
            AddStep("Touch with other finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other finger key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));
            AddAssert("The touch is a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch2));
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 1);
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Right button touch
            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3));

            AddAssert("The other touch is also a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch3));
            AddAssert("Both keys are pressed", () => osuInputManager.PressedActions.Count() == 2);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
            AddAssert("Dragging cursor", () => touchInputMapper.DraggingCursorMode);

            // Invalid touch
            AddStep("Touch with an invalid touch", () => touch(TouchSource.Touch4));

            AddAssert("Touch is blocked", () => !touchInputMapper.AllowingOtherTouch);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);

            // Cleanup
            AddStep("Release", () =>
            {
                foreach (TouchSource source in Enum.GetValues(typeof(TouchSource)))
                    release(source);
            });
        }
    }
}
