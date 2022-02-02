// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("'Hold to Quit' UI element")]
    public class TestSceneHoldForMenuButton : OsuManualInputManagerTestScene
    {
        private bool exitAction;

        protected override double TimePerAction => 100; // required for the early exit test, since hold-to-confirm delay is 200ms

        private HoldForMenuButton holdForMenuButton;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create button", () =>
            {
                exitAction = false;

                Child = holdForMenuButton = new HoldForMenuButton
                {
                    Scale = new Vector2(2),
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Action = () => exitAction = true
                };
            });
        }

        [Test]
        public void TestMovementAndTrigger()
        {
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
        public void TestFadeOnNoInput()
        {
            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep("wait for text fade out", () => !getSpriteText().IsPresent);
            AddUntilStep("wait for button fade out", () => holdForMenuButton.Alpha < 0.1f);
        }

        private SpriteText getSpriteText() => holdForMenuButton.Children.OfType<SpriteText>().First();
    }
}
