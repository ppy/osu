// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestCaseLogoTransitions : ScreenTestScene
    {
        [Cached]
        private OsuLogo logo;

        private TestScreen baseScreen;

        public TestCaseLogoTransitions()
        {
            Add(logo = new OsuLogo
            {
                RelativePositionAxes = Axes.Both,
                Scale = new Vector2(0.4f),
                Position = new Vector2(0.5f)
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Push base screen", () => LoadScreen(baseScreen = new TestScreen("Base Screen")));
            AddUntilStep("Base screen is current", () => baseScreen.IsCurrentScreen());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestMultiLevelExit(bool applyLogoTransforms)
        {
            TestScreen screen1 = null;
            TestScreen screen2 = null;
            AddStep("Push screen 1", () =>
            {
                baseScreen.Push(screen1 = applyLogoTransforms
                    ? new ForcedAnimationsTestScreen("Fade On Exit")
                    : new TestScreen("Don't Fade On Exit"));
            });
            AddUntilStep("Screen 1 is current", () => screen1.IsCurrentScreen());
            AddStep("Push screen 2", () => { screen1.Push(screen2 = new TestScreen("Don't Fade On Exit")); });
            AddUntilStep("Screen 2 is current", () => screen2.IsCurrentScreen());
            AddStep("Make base current", () => baseScreen.MakeCurrent());
            AddUntilStep("Base screen is current", () => baseScreen.IsCurrentScreen());
            AddAssert("Logo exiting animation was correct", () => screen1.LogoExitingReceived == applyLogoTransforms);
        }

        private class ForcedAnimationsTestScreen : TestScreen
        {
            protected override bool ForceLogoExitAnimation => true;

            public ForcedAnimationsTestScreen(string screenText)
                : base(screenText)
            {
            }

            protected override void LogoExiting(OsuLogo logo)
            {
                base.LogoExiting(logo);
                logo.FadeOut(1000, Easing.Out);
            }
        }

        private class TestScreen : OsuScreen
        {
            public bool LogoExitingReceived;

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
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                });
            }

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                base.LogoArriving(logo, resuming);
                logo.FadeIn(250, Easing.In);
            }

            protected override void LogoExiting(OsuLogo logo)
            {
                LogoExitingReceived = true;
                base.LogoExiting(logo);
            }
        }
    }
}
