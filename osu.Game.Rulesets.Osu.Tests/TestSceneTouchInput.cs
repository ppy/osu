// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuInputManager osuInputManager = null!;
        private OsuTouchInputMapper touchInputMapper = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("Setup touch", () =>
            {
                var drawableRuleset = (DrawableOsuRuleset)Player.DrawableRuleset;
                osuInputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
                touchInputMapper = drawableRuleset.TouchInputMapper;
            });
        }

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, touchInputMapper.ScreenSpaceDrawQuad.Centre));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, touchInputMapper.ScreenSpaceDrawQuad.Centre));

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
                foreach (var source in touchInputMapper.AllowedTouchSources)
                    release(source);
            });
        }
    }
}
