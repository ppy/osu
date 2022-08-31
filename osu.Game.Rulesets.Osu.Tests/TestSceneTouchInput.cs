// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("Setup touch", () =>
            {
                var drawableRuleset = (DrawableOsuRuleset)Player.DrawableRuleset;
                osuInputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
                touchInputHandler = drawableRuleset.TouchInputHandler;
            });
        }

        [Test]
        public void TestTouchInput()
        {
            var position = new Vector2(200);

            var cursorTouch = new Touch(OsuDrawableTouchInputHandler.CURSOR_TOUCH, position);

            AddStep("Touch", () => InputManager.BeginTouch(cursorTouch));

            AddAssert("Pressed", () => osuInputManager.CurrentState.Touch.IsActive(OsuDrawableTouchInputHandler.CURSOR_TOUCH));
        }
    }
}
