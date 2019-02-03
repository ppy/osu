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

namespace osu.Game.Tests.Visual
{
    [Description("'Hold to Quit' UI element")]
    public class TestCaseHoldForMenuButton : ManualInputManagerTestCase
    {
        private bool exitAction;

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
            AddUntilStep(() => text.IsPresent && !exitAction, "Text visible");
            AddStep("Trigger text fade out", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep(() => !text.IsPresent && !exitAction, "Text is not visible");

            AddStep("Trigger exit action", () =>
            {
                exitAction = false;
                InputManager.MoveMouseTo(holdForMenuButton);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("Early release", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("action not triggered", () => !exitAction);

            AddStep("Trigger exit action", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep(() => exitAction, $"{nameof(holdForMenuButton.Action)} was triggered");
        }
    }
}
