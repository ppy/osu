// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual
{
    [Description("'Hold to Quit' UI element")]
    public class TestCaseHoldToQuit : OsuTestCase
    {
        private bool exitAction;

        public TestCaseHoldToQuit()
        {
            HoldToQuit holdToQuit;
            Add(holdToQuit = new HoldToQuit
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
            });
            holdToQuit.Button.ExitAction = () => exitAction = true;

            var text = holdToQuit.Children.OfType<SpriteText>().Single();

            AddStep("Trigger text fade in/out", () =>
            {
                exitAction = false;
                holdToQuit.Button.TriggerOnMouseDown();
                holdToQuit.Button.TriggerOnMouseUp();
            });

            AddUntilStep(() => text.IsPresent && !exitAction, "Text visible");
            AddUntilStep(() => !text.IsPresent && !exitAction, "Text is not visible");

            AddStep("Trigger exit action", () =>
            {
                exitAction = false;
                holdToQuit.Button.TriggerOnMouseDown();
            });

            AddUntilStep(() => exitAction, $"{nameof(holdToQuit.Button.ExitAction)} was triggered");
        }
    }
}
