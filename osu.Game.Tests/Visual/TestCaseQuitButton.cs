// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Tests.Visual
{
    [Description("'Hold to Quit' UI element")]
    public class TestCaseQuitButton : ManualInputManagerTestCase
    {
        private bool exitAction;

        [BackgroundDependencyLoader]
        private void load()
        {
            QuitButton quitButton;

            Add(quitButton = new QuitButton
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                Action = () => exitAction = true
            });

            var text = quitButton.Children.OfType<SpriteText>().First();

            // initial display
            AddUntilStep(() => text.IsPresent && !exitAction, "Text visible");
            AddUntilStep(() => !text.IsPresent && !exitAction, "Text is not visible");

            AddStep("Trigger text fade in", () => InputManager.MoveMouseTo(quitButton));
            AddUntilStep(() => text.IsPresent && !exitAction, "Text visible");
            AddStep("Trigger text fade out", () => InputManager.MoveMouseTo(Vector2.One));
            AddUntilStep(() => !text.IsPresent && !exitAction, "Text is not visible");

            AddStep("Trigger exit action", () =>
            {
                exitAction = false;
                InputManager.MoveMouseTo(quitButton);
                InputManager.ButtonDown(MouseButton.Left);
            });

            AddStep("Early release", () => InputManager.ButtonUp(MouseButton.Left));
            AddAssert("action not triggered", () => !exitAction);

            AddStep("Trigger exit action", () => InputManager.ButtonDown(MouseButton.Left));
            AddUntilStep(() => exitAction, $"{nameof(quitButton.Action)} was triggered");
        }
    }
}
