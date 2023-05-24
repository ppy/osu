// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSkipOverlay : OsuManualInputManagerTestScene
    {
        private TestSkipOverlay skip;
        private int requestCount;

        private double increment;

        private GameplayClockContainer gameplayClockContainer;
        private IFrameBasedClock gameplayClock;

        private const double skip_time = 6000;

        private void createTest(double skipTime = skip_time) => AddStep("create test", () =>
        {
            requestCount = 0;
            increment = skip_time;

            var working = CreateWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));

            Child = gameplayClockContainer = new MasterGameplayClockContainer(working, 0)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    skip = new TestSkipOverlay(skipTime)
                    {
                        RequestSkip = () =>
                        {
                            requestCount++;
                            gameplayClockContainer.Seek(gameplayClock.CurrentTime + increment);
                        }
                    }
                },
            };

            gameplayClockContainer.Start();
            gameplayClock = gameplayClockContainer;
        });

        [Test]
        public void TestSkipTimeZero()
        {
            createTest(0);
            AddUntilStep("wait for skip overlay expired", () => !skip.IsAlive);
        }

        [Test]
        public void TestSkipTimeEqualToSkip()
        {
            createTest(MasterGameplayClockContainer.MINIMUM_SKIP_TIME);
            AddUntilStep("wait for skip overlay expired", () => !skip.IsAlive);
        }

        [Test]
        public void TestFadeOnIdle()
        {
            createTest();

            AddStep("move mouse", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddUntilStep("fully visible", () => skip.FadingContent.Alpha == 1);
            AddUntilStep("wait for fade", () => skip.FadingContent.Alpha < 1);

            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("fully visible", () => skip.FadingContent.Alpha == 1);
            AddUntilStep("wait for fade", () => skip.FadingContent.Alpha < 1);
        }

        [Test]
        public void TestClickableAfterFade()
        {
            createTest();

            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for fade", () => skip.FadingContent.Alpha == 0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
        }

        [Test]
        public void TestAutomaticSkipActuatesOnce()
        {
            createTest();
            AddStep("start automated skip", () => skip.SkipWhenReady());
            AddUntilStep("wait for button disabled", () => !skip.IsButtonVisible);
            checkRequestCount(1);
        }

        [Test]
        public void TestClickOnlyActuatesOnce()
        {
            createTest();

            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("click", () =>
            {
                increment = skip_time - gameplayClock.CurrentTime - MasterGameplayClockContainer.MINIMUM_SKIP_TIME / 2;
                InputManager.Click(MouseButton.Left);
            });
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
        }

        [Test]
        public void TestAutomaticSkipActuatesMultipleTimes()
        {
            createTest();
            AddStep("set increment lower", () => increment = 3000);
            AddStep("start automated skip", () => skip.SkipWhenReady());
            AddUntilStep("wait for button disabled", () => !skip.IsButtonVisible);
            checkRequestCount(2);
        }

        [Test]
        public void TestClickOnlyActuatesMultipleTimes()
        {
            createTest();

            AddStep("set increment lower", () => increment = 3000);
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(2);
        }

        [Test]
        public void TestDoesntFadeOnMouseDown()
        {
            createTest();

            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("button down", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep("wait for overlay disappear", () => !skip.OverlayContent.IsPresent);
            AddAssert("ensure button didn't disappear", () => skip.FadingContent.Alpha > 0);
            AddStep("button up", () => InputManager.ReleaseButton(MouseButton.Left));
            checkRequestCount(0);
        }

        private void checkRequestCount(int expected)
        {
            AddAssert($"skip count is {expected}", () => skip.SkipCount, () => Is.EqualTo(expected));
            AddAssert($"request count is {expected}", () => requestCount, () => Is.EqualTo(expected));
        }

        private partial class TestSkipOverlay : SkipOverlay
        {
            public TestSkipOverlay(double startTime)
                : base(startTime)
            {
            }

            public Drawable OverlayContent => InternalChild;

            public Drawable FadingContent => (OverlayContent as Container)?.Child;
        }
    }
}
