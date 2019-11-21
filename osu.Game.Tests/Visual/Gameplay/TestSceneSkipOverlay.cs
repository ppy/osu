// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSkipOverlay : ManualInputManagerTestScene
    {
        private SkipOverlay skip;
        private int requestCount;

        private FramedOffsetClock offsetClock;
        private StopwatchClock stopwatchClock;

        private double increment;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            requestCount = 0;
            increment = 6000;

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = offsetClock = new FramedOffsetClock(stopwatchClock = new StopwatchClock(true)),
                Children = new Drawable[]
                {
                    skip = new SkipOverlay(6000)
                    {
                        RequestSkip = () =>
                        {
                            requestCount++;
                            offsetClock.Offset += increment;
                        }
                    }
                },
            };
        });

        protected override void Update()
        {
            if (stopwatchClock != null)
                stopwatchClock.Rate = Clock.Rate;
        }

        [Test]
        public void TestFadeOnIdle()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddUntilStep("fully visible", () => skip.Children.First().Alpha == 1);
            AddUntilStep("wait for fade", () => skip.Children.First().Alpha < 1);

            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("fully visible", () => skip.Children.First().Alpha == 1);
            AddUntilStep("wait for fade", () => skip.Children.First().Alpha < 1);
        }

        [Test]
        public void TestClickableAfterFade()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for fade", () => skip.Children.First().Alpha == 0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
        }

        [Test]
        public void TestClickOnlyActuatesOnce()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("click", () =>
            {
                increment = 6000 - offsetClock.CurrentTime - GameplayClockContainer.MINIMUM_SKIP_TIME / 2;
                InputManager.Click(MouseButton.Left);
            });
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
        }

        [Test]
        public void TestClickOnlyActuatesMultipleTimes()
        {
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
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("button down", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep("wait for overlay disapper", () => !skip.IsAlive);
            AddAssert("ensure button didn't disappear", () => skip.Children.First().Alpha > 0);
            AddStep("button up", () => InputManager.ReleaseButton(MouseButton.Left));
            checkRequestCount(0);
        }

        private void checkRequestCount(int expected) =>
            AddAssert($"request count is {expected}", () => requestCount == expected);
    }
}
