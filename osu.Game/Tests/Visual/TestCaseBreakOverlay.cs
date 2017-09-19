// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Screens.Play;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseBreakOverlay : OsuTestCase
    {
        public override string Description => @"Tests breaks behavior";

        private readonly BreakOverlay breakOverlay;

        public TestCaseBreakOverlay()
        {
            Clock = new FramedClock();

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                breakOverlay = new BreakOverlay(true)
            };

            AddStep("Add 2s break", () => startBreak(2000));
            AddStep("Add 5s break", () => startBreak(5000));
            AddStep("Add 2 breaks (2s each)", startMultipleBreaks);
        }

        private void startBreak(double duration)
        {
            breakOverlay.Breaks = new List<BreakPeriod>
            {
                new BreakPeriod
                {
                    StartTime = Clock.CurrentTime,
                    EndTime = Clock.CurrentTime + duration,
                }
            };

            breakOverlay.InitializeBreaks();
        }

        private void startMultipleBreaks()
        {
            double currentTime = Clock.CurrentTime;

            breakOverlay.Breaks = new List<BreakPeriod>
            {
                new BreakPeriod
                {
                    StartTime = currentTime,
                    EndTime = currentTime + 2000,
                },
                new BreakPeriod
                {
                    StartTime = currentTime + 4000,
                    EndTime = currentTime + 6000,
                }
            };

            breakOverlay.InitializeBreaks();
        }
    }
}