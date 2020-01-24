// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("'Hold to Quit' UI element")]
    public class TestSceneHoldForMenuButton : ManualInputManagerTestScene
    {
        private bool exitAction;

        protected override double TimePerAction => 100; // required for the early exit test, since hold-to-confirm delay is 200ms

        [BackgroundDependencyLoader]
        private void load()
        {
            HoldForMenuButton holdForMenuButton;

            Add(holdForMenuButton = new HoldForMenuButton
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                Action = () => exitAction = true
            });

            var text = holdForMenuButton.Children.OfType<SpriteText>().First();

            AddStep("Trigger text fade in", () => InputManager.MoveMouseTo(holdForMenuButton));
            AddUntilStep("Text visible", () => text.IsPresent && !exitAction);
            AddStep("Trigger text fade out", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep("Text is not visible", () => !text.IsPresent && !exitAction);

            AddStep("Trigger exit action", () =>
            {
                exitAction = false;
                InputManager.MoveMouseTo(holdForMenuButton);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("Early release", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("action not triggered", () => !exitAction);

            AddStep("Trigger exit action", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep($"{nameof(holdForMenuButton.Action)} was triggered", () => exitAction);
        }
    }
}
