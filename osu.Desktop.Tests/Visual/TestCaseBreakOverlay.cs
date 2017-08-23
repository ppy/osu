// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseBreakOverlay : OsuTestCase
    {
        public override string Description => @"Tests breaks behavior";

        public TestCaseBreakOverlay()
        {
            Clock = new FramedClock();

            BreakOverlay breakOverlay = new BreakOverlay(true) { AudioClock = Clock };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                breakOverlay
            };

            AddStep("Add 5s break", () => breakOverlay.StartBreak(5000));
            AddStep("Add 10s break", () => breakOverlay.StartBreak(10000));
            AddStep("Add 15s break", () => breakOverlay.StartBreak(15000));
        }
    }
}
