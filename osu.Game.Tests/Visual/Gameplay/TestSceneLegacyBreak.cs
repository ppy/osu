// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneLegacyBreak : SkinnableTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private LegacyBreakOverlay? breakOverlay;

        private TestBreakTracker? breakTracker;

        private readonly IReadOnlyList<BreakPeriod> testBreaks = new List<BreakPeriod>
        {
            new BreakPeriod(1000, 5000),
            new BreakPeriod(6000, 13500),
        };

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUp]
        public void SetUp()
        {
            Child = new SkinProvidingContainer(skins.DefaultClassicSkin)
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                    breakTracker = new TestBreakTracker(),
                    breakOverlay = new LegacyBreakOverlay(new OsuHealthProcessor(0))
                    {
                        ProcessCustomClock = false,
                        BreakTracker = breakTracker,
                    },
                    new LetterboxOverlay
                    {
                        ProcessCustomClock = false,
                        BreakTracker = breakTracker,
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            if (breakOverlay != null && breakTracker != null)
                breakOverlay.Clock = breakTracker.Clock;
        }

        [Test]
        public void TestShowBreaks()
        {
            addShowBreakStep(5);
            addShowBreakStep(15);
        }

        private void addShowBreakStep(double seconds)
        {
            AddStep($"show '{seconds}s' break", () =>
            {
                breakTracker!.Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(Clock.CurrentTime, Clock.CurrentTime + seconds * 1000)
                };
            });
        }

        private partial class TestBreakTracker : BreakTracker
        {
            public readonly FramedClock FramedManualClock;

            private readonly ManualClock manualClock;
            private IFrameBasedClock originalClock;

            public double ManualClockTime
            {
                get => manualClock.CurrentTime;
                set => manualClock.CurrentTime = value;
            }

            public TestBreakTracker()
                : base(0, new ScoreProcessor(new OsuRuleset()))
            {
                FramedManualClock = new FramedClock(manualClock = new ManualClock());
                ProcessCustomClock = false;
            }

            public void ProgressTime()
            {
                FramedManualClock.ProcessFrame();
                Update();
            }

            public void SwitchClock(bool setManual) => Clock = setManual ? FramedManualClock : originalClock;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                originalClock = Clock;
            }
        }
    }
}
