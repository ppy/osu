// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestSceneOsuScreenStack : OsuTestScene
    {
        private TestOsuScreenStack stack;

        [Cached]
        private MusicController musicController = new MusicController();

        [BackgroundDependencyLoader]
        private void load()
        {
            stack = new TestOsuScreenStack { RelativeSizeAxes = Axes.Both };

            Add(musicController);
            Add(stack);

            LoadComponent(stack);
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

        [Test]
        public void AllowTrackAdjustmentsTest()
        {
            AddStep("push allowing screen", () => stack.Push(loadNewScreen<AllowScreen>()));
            AddAssert("allows adjustments 1", () => musicController.AllowTrackAdjustments);

            AddStep("push inheriting screen", () => stack.Push(loadNewScreen<InheritScreen>()));
            AddAssert("allows adjustments 2", () => musicController.AllowTrackAdjustments);

            AddStep("push disallowing screen", () => stack.Push(loadNewScreen<DisallowScreen>()));
            AddAssert("disallows adjustments 3", () => !musicController.AllowTrackAdjustments);

            AddStep("push inheriting screen", () => stack.Push(loadNewScreen<InheritScreen>()));
            AddAssert("disallows adjustments 4", () => !musicController.AllowTrackAdjustments);

            AddStep("push inheriting screen", () => stack.Push(loadNewScreen<InheritScreen>()));
            AddAssert("disallows adjustments 5", () => !musicController.AllowTrackAdjustments);

            AddStep("push allowing screen", () => stack.Push(loadNewScreen<AllowScreen>()));
            AddAssert("allows adjustments 6", () => musicController.AllowTrackAdjustments);

            // Now start exiting from screens
            AddStep("exit screen", () => stack.Exit());
            AddAssert("disallows adjustments 7", () => !musicController.AllowTrackAdjustments);

            AddStep("exit screen", () => stack.Exit());
            AddAssert("disallows adjustments 8", () => !musicController.AllowTrackAdjustments);

            AddStep("exit screen", () => stack.Exit());
            AddAssert("disallows adjustments 9", () => !musicController.AllowTrackAdjustments);

            AddStep("exit screen", () => stack.Exit());
            AddAssert("allows adjustments 10", () => musicController.AllowTrackAdjustments);

            AddStep("exit screen", () => stack.Exit());
            AddAssert("allows adjustments 11", () => musicController.AllowTrackAdjustments);
        }

        public class TestScreen : ScreenWithBeatmapBackground
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

        private class AllowScreen : OsuScreen
        {
            public override bool? AllowTrackAdjustments => true;
        }

        public class DisallowScreen : OsuScreen
        {
            public override bool? AllowTrackAdjustments => false;
        }

        private class InheritScreen : OsuScreen
        {
        }

        private OsuScreen loadNewScreen<T>() where T : OsuScreen, new()
        {
            OsuScreen screen = new T();
            LoadComponent(screen);
            return screen;
        }
    }
}
