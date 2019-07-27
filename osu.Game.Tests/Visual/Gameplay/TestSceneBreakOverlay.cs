// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneBreakOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BreakOverlay),
        };

        private readonly BreakOverlay breakOverlay, manualBreakOverlay;
        private readonly ManualClock manualClock = new ManualClock();

        private readonly IReadOnlyList<BreakPeriod> testBreaks = new List<BreakPeriod>
        {
            new BreakPeriod
            {
                StartTime = 1000,
                EndTime = 5000,
            },
            new BreakPeriod
            {
                StartTime = 6000,
                EndTime = 13500,
            },
        };

        public TestSceneBreakOverlay()
        {
            Add(breakOverlay = new BreakOverlay(true));
            Add(manualBreakOverlay = new BreakOverlay(true)
            {
                Alpha = 0,
                Clock = new FramedClock(manualClock),
            });
        }

        [Test]
        public void TestShowBreaks()
        {
            loadClockStep(false);

            addShowBreakStep(2);
            addShowBreakStep(5);
            addShowBreakStep(15);
        }

        [Test]
        public void TestNoEffectsBreak()
        {
            var shortBreak = new BreakPeriod { EndTime = 500 };

            loadClockStep(true);
            AddStep("start short break", () => manualBreakOverlay.Breaks = new[] { shortBreak });

            seekBreakStep("seek back to 0", 0, false);
            addBreakSeeks(shortBreak, false);
        }

        [Test]
        public void TestMultipleBreaks()
        {
            loadClockStep(true);
            AddStep("start multiple breaks", () => manualBreakOverlay.Breaks = testBreaks);

            seekBreakStep("seek back to 0", 0, false);
            foreach (var b in testBreaks)
                addBreakSeeks(b, false);
        }

        [Test]
        public void TestRewindBreaks()
        {
            loadClockStep(true);
            AddStep("start multiple breaks in rewind", () => manualBreakOverlay.Breaks = testBreaks);

            seekBreakStep("seek back to 0", 0, false);
            foreach (var b in testBreaks.Reverse())
                addBreakSeeks(b, true);
        }

        [Test]
        public void TestSkipBreaks()
        {
            loadClockStep(true);
            AddStep("start multiple breaks with skipping", () => manualBreakOverlay.Breaks = testBreaks);

            var b = testBreaks.Last();
            seekBreakStep("seek back to 0", 0, false);
            addBreakSeeks(b, false);
        }

        private void addShowBreakStep(double seconds)
        {
            AddStep($"show '{seconds}s' break", () => breakOverlay.Breaks = new List<BreakPeriod>
            {
                new BreakPeriod
                {
                    StartTime = Clock.CurrentTime,
                    EndTime = Clock.CurrentTime + seconds * 1000,
                }
            });
        }

        private void loadClockStep(bool loadManual)
        {
            AddStep($"load {(loadManual ? "manual" : "normal")} clock", () =>
            {
                breakOverlay.FadeTo(loadManual ? 0 : 1);
                manualBreakOverlay.FadeTo(loadManual ? 1 : 0);
            });
        }

        private void addBreakSeeks(BreakPeriod b, bool isReversed)
        {
            if (isReversed)
            {
                seekBreakStep("seek to break after end", b.EndTime + 500, false);
                seekBreakStep("seek to break end", b.EndTime, false);
                seekBreakStep("seek to break middle", b.StartTime + b.Duration / 2, b.HasEffect);
                seekBreakStep("seek to break start", b.StartTime, b.HasEffect);
            }
            else
            {
                seekBreakStep("seek to break start", b.StartTime, b.HasEffect);
                seekBreakStep("seek to break middle", b.StartTime + b.Duration / 2, b.HasEffect);
                seekBreakStep("seek to break end", b.EndTime, false);
                seekBreakStep("seek to break after end", b.EndTime + 500, false);
            }
        }

        private void seekBreakStep(string seekStepDescription, double time, bool onBreak)
        {
            AddStep(seekStepDescription, () => manualClock.CurrentTime = time);
            AddAssert($"is{(!onBreak ? " not " : " ")}break time", () => manualBreakOverlay.IsBreakTime.Value == onBreak);
        }
    }
}
