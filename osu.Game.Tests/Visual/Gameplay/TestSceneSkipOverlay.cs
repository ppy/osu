// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
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

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            requestCount = 0;
            Child = skip = new SkipOverlay(Clock.CurrentTime + 5000)
            {
                RequestSeek = _ => requestCount++
            };
        });

        [Test]
        public void TestFadeOnIdle()
        {
            AddStep("move mouse", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddUntilStep("wait for fade", () => skip.Children.First().Alpha == 0);
            AddStep("move mouse", () => InputManager.MoveMouseTo(skip.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("visible again", () => skip.Children.First().Alpha > 0);
            AddUntilStep("wait for fade", () => skip.Children.First().Alpha == 0);
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
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkRequestCount(1);
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
