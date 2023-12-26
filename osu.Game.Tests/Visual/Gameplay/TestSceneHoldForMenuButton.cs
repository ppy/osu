// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("'Hold to Quit' UI element")]
    public partial class TestSceneHoldForMenuButton : OsuManualInputManagerTestScene
    {
        private bool exitAction;

        protected override double TimePerAction => 100; // required for the early exit test, since hold-to-confirm delay is 200ms

        private HoldForMenuButton holdForMenuButton;

        private void setupSteps(bool alwaysShow)
        {
            AddStep("create button", () =>
            {
                InputManager.MoveMouseTo(Vector2.One);

                exitAction = false;

                Child = holdForMenuButton = new HoldForMenuButton
                {
                    Scale = new Vector2(2),
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Action = () => exitAction = true,
                    AlwaysShow = { Value = alwaysShow },
                };
            });
        }

        [Test]
        public void TestFullMovementAndTrigger()
        {
            setupSteps(alwaysShow: true);
            AddStep("Trigger text fade in", () => InputManager.MoveMouseTo(holdForMenuButton));
            AddUntilStep("Text visible", () => getSpriteText().IsPresent && !exitAction);
            AddStep("Trigger text fade out", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep("Text is not visible", () => !getSpriteText().IsPresent && !exitAction);

            AddStep("Trigger exit action", () =>
            {
                InputManager.MoveMouseTo(holdForMenuButton);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("Early release", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("action not triggered", () => !exitAction);

            AddStep("Trigger exit action", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep($"{nameof(holdForMenuButton.Action)} was triggered", () => exitAction);
            AddStep("Release", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestAutoHide()
        {
            setupSteps(alwaysShow: false);
            AddAssert("Container initially invisible", () => holdForMenuButton.Alpha < .01f);
            AddStep("Trigger fade in", () => InputManager.MoveMouseTo(holdForMenuButton));
            AddUntilStep("Container visible", () => holdForMenuButton.Alpha >= 1f && !exitAction);
            AddStep("Trigger fade out", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep("Container invisible", () => holdForMenuButton.Alpha < .01f && !exitAction);

            // Make sure container hasn't disappeared
            AddStep("Trigger exit action", () =>
            {
                InputManager.MoveMouseTo(holdForMenuButton);
                InputManager.PressButton(MouseButton.Left);
            });
            AddUntilStep($"{nameof(holdForMenuButton.Action)} was triggered", () => exitAction);
        }

        [Test]
        public void TestPartialFadeOnNoInput()
        {
            setupSteps(alwaysShow: true);
            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep("wait for text fade out", () => !getSpriteText().IsPresent);
            AddUntilStep("wait for button fade out", () => holdForMenuButton.Alpha < 0.1f);
        }

        private SpriteText getSpriteText() => holdForMenuButton.Children.OfType<SpriteText>().First();
    }
}
