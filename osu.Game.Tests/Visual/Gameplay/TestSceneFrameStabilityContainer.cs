// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneFrameStabilityContainer : OsuTestScene
    {
        private readonly ManualClock manualClock;

        private readonly Container mainContainer;

        private ClockConsumingChild consumer;

        public TestSceneFrameStabilityContainer()
        {
            Child = mainContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = new FramedClock(manualClock = new ManualClock()),
            };
        }

        [Test]
        public void TestLargeJumps()
        {
            seekManualTo(0);
            createStabilityContainer();
            seekManualTo(100000);

            confirmSeek(100000);
            checkFrameCount(6000);

            seekManualTo(0);

            confirmSeek(0);
            checkFrameCount(12000);
        }

        [Test]
        public void TestSmallJumps()
        {
            seekManualTo(0);
            createStabilityContainer();
            seekManualTo(40);

            confirmSeek(40);
            checkFrameCount(3);

            seekManualTo(0);

            confirmSeek(0);
            checkFrameCount(6);
        }

        [Test]
        public void TestSingleFrameJump()
        {
            seekManualTo(0);
            createStabilityContainer();
            seekManualTo(8);
            confirmSeek(8);
            checkFrameCount(1);

            seekManualTo(16);
            confirmSeek(16);
            checkFrameCount(2);
        }

        [Test]
        public void TestInitialSeekWithGameplayStart()
        {
            seekManualTo(1000);
            createStabilityContainer(30000);

            confirmSeek(1000);
            checkFrameCount(0);

            seekManualTo(10000);
            confirmSeek(10000);

            checkFrameCount(1);

            seekManualTo(130000);
            confirmSeek(130000);

            checkFrameCount(6002);
        }

        [Test]
        public void TestInitialSeek()
        {
            seekManualTo(100000);
            createStabilityContainer();

            confirmSeek(100000);
            checkFrameCount(0);
        }

        [Test]
        public void TestRatePreservedWhenTimeNotProgressing()
        {
            AddStep("set manual clock rate", () => manualClock.Rate = 1);
            seekManualTo(5000);
            createStabilityContainer();
            checkRate(1);

            seekManualTo(10000);
            checkRate(1);

            AddWaitStep("wait some", 3);
            checkRate(1);

            seekManualTo(5000);
            checkRate(-1);

            AddWaitStep("wait some", 3);
            checkRate(-1);

            seekManualTo(10000);
            checkRate(1);
        }

        private void createStabilityContainer(double gameplayStartTime = double.MinValue) => AddStep("create container", () =>
            mainContainer.Child = new FrameStabilityContainer(gameplayStartTime)
                {
                    AllowBackwardsSeeks = true,
                }
                .WithChild(consumer = new ClockConsumingChild()));

        private void seekManualTo(double time) => AddStep($"seek manual clock to {time}", () => manualClock.CurrentTime = time);

        private void confirmSeek(double time) => AddUntilStep($"wait for seek to {time}", () => consumer.Clock.CurrentTime, () => Is.EqualTo(time));

        private void checkFrameCount(int frames) =>
            AddAssert($"elapsed frames is {frames}", () => consumer.ElapsedFrames, () => Is.EqualTo(frames));

        private void checkRate(double rate) =>
            AddAssert($"clock rate is {rate}", () => consumer.Clock.Rate, () => Is.EqualTo(rate));

        public partial class ClockConsumingChild : CompositeDrawable
        {
            private readonly OsuSpriteText text;
            private readonly OsuSpriteText text2;
            private readonly OsuSpriteText text3;

            public ClockConsumingChild()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            text2 = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            text3 = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        }
                    },
                };
            }

            public int ElapsedFrames;

            protected override void Update()
            {
                base.Update();

                if (Clock.ElapsedFrameTime != 0)
                    ElapsedFrames++;

                text.Text = $"current time: {Clock.CurrentTime:F0}";
                if (Clock.ElapsedFrameTime != 0)
                    text2.Text = $"last elapsed frame time: {Clock.ElapsedFrameTime:F0}";
                text3.Text = $"total frames: {ElapsedFrames:F0}";
            }
        }
    }
}
