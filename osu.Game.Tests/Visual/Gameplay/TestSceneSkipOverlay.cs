// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSkipOverlay : OsuManualInputManagerTestScene
    {
        private TestSkipOverlay skip;
        private int requestCount;

        private double increment;

        private GameplayClockContainer gameplayClockContainer;
        private GameplayClock gameplayClock;

        private const double skip_time = 6000;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            requestCount = 0;
            increment = skip_time;

            var working = CreateWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
            working.LoadTrack();

            Child = gameplayClockContainer = new GameplayClockContainer(working, 0)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    skip = new TestSkipOverlay(skip_time)
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
            gameplayClock = gameplayClockContainer.GameplayClock;
        });

        [Test]
        public void TestFadeOnIdle()
        {
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
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for fade", () => skip.FadingContent.Alpha == 0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
        }

        [Test]
        public void TestClickOnlyActuatesOnce()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddStep("click", () =>
            {
                increment = skip_time - gameplayClock.CurrentTime - GameplayClockContainer.MINIMUM_SKIP_TIME / 2;
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
            AddUntilStep("wait for overlay disappear", () => !skip.OverlayContent.IsPresent);
            AddAssert("ensure button didn't disappear", () => skip.FadingContent.Alpha > 0);
            AddStep("button up", () => InputManager.ReleaseButton(MouseButton.Left));
            checkRequestCount(0);
        }

        private void checkRequestCount(int expected) =>
            AddAssert($"request count is {expected}", () => requestCount == expected);

        private class TestSkipOverlay : SkipOverlay
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
