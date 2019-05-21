// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestSceneOsuScreenStack : OsuTestScene
    {
        private TestOsuScreenStack stack;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create new screen stack", () => { Child = stack = new TestOsuScreenStack { RelativeSizeAxes = Axes.Both }; });
        }

        [Test]
        public void ParallaxAssignmentTest()
        {
            NoParallaxTestScreen noParallaxScreen = null;
            TestScreen parallaxScreen = null;

            AddStep("Push no parallax", () => stack.Push(noParallaxScreen = new NoParallaxTestScreen("NO PARALLAX")));
            AddUntilStep("Wait for current", () => noParallaxScreen.IsLoaded);
            AddAssert("Parallax is off", () => stack.ParallaxAmount == 0);

            AddStep("Push parallax", () => noParallaxScreen.Push(parallaxScreen = new TestScreen("PARALLAX")));
            AddUntilStep("Wait for current", () => parallaxScreen.IsLoaded);
            AddAssert("Parallax is on", () => stack.ParallaxAmount > 0);

            AddStep("Exit from new screen", () => { noParallaxScreen.MakeCurrent(); });
            AddAssert("Parallax is off", () => stack.ParallaxAmount == 0);
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
                AddInternal(new OsuSpriteText
                {
                    Text = screenText,
                    Colour = Color4.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
            public new float ParallaxAmount => base.ParallaxAmount;
        }
    }
}
