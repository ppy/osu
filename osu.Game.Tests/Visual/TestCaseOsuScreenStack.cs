// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuScreenStack : OsuTestCase
    {
        private NoParallaxTestScreen baseScreen;
        private TestOsuScreenStack stack;

        [SetUpSteps]
        public void Setup()
        {
            AddStep("Create new screen stack", () => { Child = stack = new TestOsuScreenStack { RelativeSizeAxes = Axes.Both }; });
            AddStep("Push new base screen", () => stack.Push(baseScreen = new NoParallaxTestScreen("THIS IS SCREEN 1. THIS SCREEN SHOULD HAVE NO PARALLAX.")));
        }

        [Test]
        public void ParallaxAssignmentTest()
        {
            AddStep("Push new screen to base screen", () => baseScreen.Push(new TestScreen("THIS IS SCREEN 2. THIS SCREEN SHOULD HAVE PARALLAX.")));
            AddAssert("Parallax is correct", () => stack.IsParallaxSet);
            AddStep("Exit from new screen", () => { baseScreen.MakeCurrent(); });
            AddAssert("Parallax is correct", () => stack.IsParallaxSet);
        }

        private class TestScreen : ScreenWithBeatmapBackground
        {
            private readonly string screenText;

            public TestScreen(string screenText)
            {
                this.screenText = screenText;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new SpriteText
                {
                    Text = screenText,
                    Colour = Color4.White
                });
            }
        }

        private class NoParallaxTestScreen : TestScreen
        {
            public NoParallaxTestScreen(string screenText)
                : base(screenText)
            {
            }

            public override float BackgroundParallaxAmount => 0.0f;
        }

        private class TestOsuScreenStack : OsuScreenStack
        {
            public bool IsParallaxSet => ParallaxAmount == ((TestScreen)CurrentScreen).BackgroundParallaxAmount * ParallaxContainer.DEFAULT_PARALLAX_AMOUNT;
        }
    }
}
