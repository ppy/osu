// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
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

        private readonly TestBreakOverlay breakOverlay;

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
            Add(breakOverlay = new TestBreakOverlay(true));
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
            loadBreaksStep("short break", new[] { shortBreak });

            addBreakSeeks(shortBreak, false);
        }

        [Test]
        public void TestMultipleBreaks()
        {
            loadClockStep(true);
            loadBreaksStep("multiple breaks", testBreaks);

            foreach (var b in testBreaks)
                addBreakSeeks(b, false);
        }

        [Test]
        public void TestRewindBreaks()
        {
            loadClockStep(true);
            loadBreaksStep("multiple breaks", testBreaks);

            foreach (var b in testBreaks.Reverse())
                addBreakSeeks(b, true);
        }

        [Test]
        public void TestSkipBreaks()
        {
            loadClockStep(true);
            loadBreaksStep("multiple breaks", testBreaks);

            addBreakSeeks(testBreaks.Last(), false);
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
            AddStep($"load {(loadManual ? "manual" : "normal")} clock", () => breakOverlay.SwitchClock(loadManual));
        }

        private void loadBreaksStep(string breakDescription, IReadOnlyList<BreakPeriod> breaks)
        {
            AddStep($"load {breakDescription}", () => breakOverlay.Breaks = breaks);
            seekBreakStep("seek back to 0", 0, false);
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
            AddStep(seekStepDescription, () => breakOverlay.ManualClockTime = time);
            AddAssert($"is{(!onBreak ? " not " : " ")}break time", () => breakOverlay.IsBreakTime.Value == onBreak);
        }

        private class TestBreakOverlay : BreakOverlay
        {
            private readonly FramedClock framedManualClock;
            private readonly ManualClock manualClock;
            private IFrameBasedClock normalClock;

            public double ManualClockTime
            {
                get => manualClock.CurrentTime;
                set => manualClock.CurrentTime = value;
            }

            public new IBindable<bool> IsBreakTime
            {
                get
                {
                    // Manually call the update function as it might take up to 2 frames for an automatic update to happen
                    Update();

                    return base.IsBreakTime;
                }
            }

            public TestBreakOverlay(bool letterboxing)
                : base(letterboxing)
            {
                framedManualClock = new FramedClock(manualClock = new ManualClock());
            }

            public void SwitchClock(bool setManual) => Clock = setManual ? framedManualClock : normalClock;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                normalClock = Clock;
            }
        }
    }
}
