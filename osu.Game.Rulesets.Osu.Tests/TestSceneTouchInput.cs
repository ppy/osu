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
            AddStep("Touch with cursor finger", () => touch(TouchSource.Touch1));

            AddStep("Touch with other finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other finger key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));

            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3));

            AddAssert("Dragging cursor", () => touchInputMapper.DraggingCursorMode);

            AddStep("Release", () =>
            {
                foreach (TouchSource source in Enum.GetValues(typeof(TouchSource)))
                    release(source);
            });
        }
    }
}
