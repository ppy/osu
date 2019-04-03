// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestCaseBreakOverlay : OsuTestCase
    {
        private readonly BreakOverlay breakOverlay;

        public TestCaseBreakOverlay()
        {
            Child = breakOverlay = new BreakOverlay(true);

            AddStep("2s break", () => startBreak(2000));
            AddStep("5s break", () => startBreak(5000));
            AddStep("10s break", () => startBreak(10000));
            AddStep("15s break", () => startBreak(15000));
            AddStep("2s, 2s", startMultipleBreaks);
            AddStep("0.5s, 0.7s, 1s, 2s", startAnotherMultipleBreaks);
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
        }

        private void startAnotherMultipleBreaks()
        {
            double currentTime = Clock.CurrentTime;

            breakOverlay.Breaks = new List<BreakPeriod>
            {
                new BreakPeriod // Duration is less than 650 - too short to appear
                {
                    StartTime = currentTime,
                    EndTime = currentTime + 500,
                },
                new BreakPeriod
                {
                    StartTime = currentTime + 1500,
                    EndTime = currentTime + 2200,
                },
                new BreakPeriod
                {
                    StartTime = currentTime + 3200,
                    EndTime = currentTime + 4200,
                },
                new BreakPeriod
                {
                    StartTime = currentTime + 5200,
                    EndTime = currentTime + 7200,
                }
            };
        }
    }
}
