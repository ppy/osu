// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

#nullable disable

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuInputManager osuInputManager;
        private OsuDrawableTouchInputHandler touchInputHandler;
        private Vector2 touchPosition;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("Setup touch", () =>
            {
                var drawableRuleset = (DrawableOsuRuleset)Player.DrawableRuleset;
                osuInputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
                touchInputHandler = drawableRuleset.TouchInputHandler;
                touchPosition = touchInputHandler.ScreenSpaceDrawQuad.Centre;
            });
        }

        private void touch(TouchSource source)
        {
            InputManager.BeginTouch(new Touch(source, touchPosition));
        }

        private void release(TouchSource source)
        {
            InputManager.EndTouch(new Touch(source, touchPosition));
        }

        [Test]
        public void TestTouchInput()
        {
            AddStep("Touch", () => touch(OsuDrawableTouchInputHandler.CURSOR_TOUCH));

            AddAssert("Pressed", () => osuInputManager.CurrentState.Touch.IsActive(OsuDrawableTouchInputHandler.CURSOR_TOUCH));

            AddStep("Touch with other finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));

            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3));

            AddAssert("Is dragging", () => osuInputManager.DragMode);

            AddStep("Release", () =>
            {
                foreach (var source in touchInputHandler.AllowedTouchSources)
                    release(source);
            });
        }
    }
}
