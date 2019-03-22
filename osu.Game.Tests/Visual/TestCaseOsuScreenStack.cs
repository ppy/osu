// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuScreenStack : OsuTestCase
    {
        private TestScreen baseScreen;
        private TestOsuScreenStack stack;

        [SetUpSteps]
        public void Setup()
        {
            AddStep("Create new screen stack", () => { Child = stack = new TestOsuScreenStack { RelativeSizeAxes = Axes.Both }; });
            AddStep("Push new base screen", () => stack.Push(baseScreen = new TestScreen()));
        }

        [Test]
        public void ParallaxAssignmentTest()
        {
            AddStep("Push new screen to base screen", () => baseScreen.Push(new TestScreen()));
            AddAssert("Parallax is correct", () => stack.IsParallaxSet);
            AddStep("Exit from new screen", () => { baseScreen.MakeCurrent(); });
            AddAssert("Parallax is correct", () => stack.IsParallaxSet);
        }

        private class TestScreen : ScreenWithBeatmapBackground
        {
        }

        private class TestOsuScreenStack : OsuScreenStack
        {
            public bool IsParallaxSet => ParallaxAmount == ((TestScreen)CurrentScreen).BackgroundParallaxAmount * ParallaxContainer.DEFAULT_PARALLAX_AMOUNT;
        }
    }
}
