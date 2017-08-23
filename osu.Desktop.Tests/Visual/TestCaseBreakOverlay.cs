// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseBreakOverlay : OsuTestCase
    {
        public override string Description => @"Tests breaks behavior";

        public TestCaseBreakOverlay()
        {
            BreakOverlay breakOverlay = new BreakOverlay(true) { AudioClock = new FramedClock() };

            AddStep("Add 5s break", () => breakOverlay.StartBreak(5000));
            AddStep("Add 10s break", () => breakOverlay.StartBreak(10000));
            AddStep("Add 15s break", () => breakOverlay.StartBreak(15000));
        }
    }
}
