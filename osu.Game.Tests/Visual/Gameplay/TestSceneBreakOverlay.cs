// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
            setClock(false);

            addShowBreakStep(2);
            addShowBreakStep(5);
            addShowBreakStep(15);
        }

        [Test]
        public void TestNoEffectsBreak()
        {
            var shortBreak = new BreakPeriod { EndTime = 500 };

            setClock(true);
            loadBreaksStep("short break", new[] { shortBreak });

            addBreakSeeks(shortBreak, false);
        }

        [Test]
        public void TestMultipleBreaks()
        {
            setClock(true);
            loadBreaksStep("multiple breaks", testBreaks);

            foreach (var b in testBreaks)
                addBreakSeeks(b, false);
        }

        [Test]
        public void TestRewindBreaks()
        {
            setClock(true);
            loadBreaksStep("multiple breaks", testBreaks);

            foreach (var b in testBreaks.Reverse())
                addBreakSeeks(b, true);
        }

        [Test]
        public void TestSkipBreaks()
        {
            setClock(true);
            loadBreaksStep("multiple breaks", testBreaks);

            seekAndAssertBreak("seek to break start", testBreaks[1].StartTime, true);
            AddAssert("is skipped to break #2", () => breakOverlay.CurrentBreakIndex == 1);

            seekAndAssertBreak("seek to break middle", testBreaks[1].StartTime + testBreaks[1].Duration / 2, true);
            seekAndAssertBreak("seek to break end", testBreaks[1].EndTime, false);
            seekAndAssertBreak("seek to break after end", testBreaks[1].EndTime + 500, false);
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

        private void setClock(bool useManual)
        {
            AddStep($"set {(useManual ? "manual" : "realtime")} clock", () => breakOverlay.SwitchClock(useManual));
        }

        private void loadBreaksStep(string breakDescription, IReadOnlyList<BreakPeriod> breaks)
        {
            AddStep($"load {breakDescription}", () => breakOverlay.Breaks = breaks);
            seekAndAssertBreak("seek back to 0", 0, false);
        }

        private void addBreakSeeks(BreakPeriod b, bool isReversed)
        {
            if (isReversed)
            {
                seekAndAssertBreak("seek to break after end", b.EndTime + 500, false);
                seekAndAssertBreak("seek to break end", b.EndTime, false);
                seekAndAssertBreak("seek to break middle", b.StartTime + b.Duration / 2, b.HasEffect);
                seekAndAssertBreak("seek to break start", b.StartTime, b.HasEffect);
            }
            else
            {
                seekAndAssertBreak("seek to break start", b.StartTime, b.HasEffect);
                seekAndAssertBreak("seek to break middle", b.StartTime + b.Duration / 2, b.HasEffect);
                seekAndAssertBreak("seek to break end", b.EndTime, false);
                seekAndAssertBreak("seek to break after end", b.EndTime + 500, false);
            }
        }

        private void seekAndAssertBreak(string seekStepDescription, double time, bool shouldBeBreak)
        {
            AddStep(seekStepDescription, () => breakOverlay.ManualClockTime = time);
            AddAssert($"is{(!shouldBeBreak ? " not" : string.Empty)} break time", () =>
            {
                breakOverlay.ProgressTime();
                return breakOverlay.IsBreakTime.Value == shouldBeBreak;
            });
        }

        private class TestBreakOverlay : BreakOverlay
        {
            private readonly FramedClock framedManualClock;
            private readonly ManualClock manualClock;
            private IFrameBasedClock originalClock;

            public new int CurrentBreakIndex => base.CurrentBreakIndex;

            public double ManualClockTime
            {
                get => manualClock.CurrentTime;
                set => manualClock.CurrentTime = value;
            }

            public TestBreakOverlay(bool letterboxing)
                : base(letterboxing)
            {
                framedManualClock = new FramedClock(manualClock = new ManualClock());
                ProcessCustomClock = false;
            }

            public void ProgressTime()
            {
                framedManualClock.ProcessFrame();
                Update();
            }

            public void SwitchClock(bool setManual) => Clock = setManual ? framedManualClock : originalClock;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                originalClock = Clock;
            }
        }
    }
}
