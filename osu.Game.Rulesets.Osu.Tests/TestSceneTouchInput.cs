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
        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().Single();

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, osuInputManager.ScreenSpaceDrawQuad.Centre));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, osuInputManager.ScreenSpaceDrawQuad.Centre));

        [Test]
        public void TestTouchInput()
        {
            AddStep("Touch", () => touch(OsuTouchInputMapper.CURSOR_TOUCH));

            AddAssert("Pressed", () => osuInputManager.CurrentState.Touch.IsActive(OsuTouchInputMapper.CURSOR_TOUCH));

            AddStep("Touch with other finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));

            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3));

            AddAssert("Is dragging", () => osuInputManager.DragMode);

            AddStep("Release", () =>
            {
                foreach (var source in Enum.GetValues<TouchSource>())
                    release(source);
            });
        }
    }
}
