// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Screens.Play;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseBreakOverlay : OsuTestCase
    {
        public override string Description => @"Tests breaks behavior";

        public TestCaseBreakOverlay()
        {
            BreakOverlay breakOverlay = new BreakOverlay();

            AddStep("Add 5s break", () => breakOverlay.Show(5000));
            AddStep("Add 10s break", () => breakOverlay.Show(10000));
            AddStep("Add 15s break", () => breakOverlay.Show(15000));
        }
    }
}
