// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual
{
    [Description("'Hold to Quit' UI element")]
    public class TestCaseQuitButton : OsuTestCase
    {
        private readonly QuitButton quitButton;
        private Drawable innerButton => quitButton.Children.Single(child => child is CircularContainer);
        private bool exitAction;

        public TestCaseQuitButton()
        {
            Add(quitButton = new QuitButton
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
            });
            quitButton.ExitAction = () => exitAction = true;

            var text = quitButton.Children.OfType<SpriteText>().Single();

            AddStep("Trigger text fade in/out", () =>
            {
                exitAction = false;

                innerButton.TriggerOnMouseDown();
                innerButton.TriggerOnMouseUp();
            });

            AddUntilStep(() => text.IsPresent && !exitAction, "Text visible");
            AddUntilStep(() => !text.IsPresent && !exitAction, "Text is not visible");

            AddStep("Trigger exit action", () =>
            {
                exitAction = false;
                innerButton.TriggerOnMouseDown();
            });

            AddUntilStep(() => exitAction, $"{nameof(quitButton.ExitAction)} was triggered");
        }
    }
}
