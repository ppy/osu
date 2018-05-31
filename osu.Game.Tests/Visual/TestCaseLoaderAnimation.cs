// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLoaderAnimation : OsuTestCase
    {
        private TestLoader loader;

        public TestCaseLoaderAnimation()
        {
            bool logoVisible = false;

            AddStep("almost instant display", () => Child = loader = new TestLoader(0.25f));
            AddUntilStep(() =>
            {
                logoVisible = loader.Logo.Alpha > 0;
                return !loader.IsCurrentScreen;
            }, "loaded");
            AddAssert("logo not visible", () => !logoVisible);

            AddStep("short load", () => Child = loader = new TestLoader(0.8f));
            AddUntilStep(() =>
            {
                logoVisible = loader.Logo.Alpha > 0;
                return !loader.IsCurrentScreen;
            }, "loaded");
            AddAssert("logo visible", () => logoVisible);
            AddUntilStep(() => loader.Logo.Alpha == 0, "logo gone");

            AddStep("longer load", () => Child = loader = new TestLoader(1.4f));
            AddUntilStep(() =>
            {
                logoVisible = loader.Logo.Alpha > 0;
                return !loader.IsCurrentScreen;
            }, "loaded");
            AddAssert("logo visible", () => logoVisible);
            AddUntilStep(() => loader.Logo.Alpha == 0, "logo gone");
        }

        private class TestLoader : Loader
        {
            private readonly float secondsDelay;

            public OsuLogo Logo;

            public TestLoader(float secondsDelay)
            {
                this.secondsDelay = secondsDelay;
            }

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                Logo = logo;
                base.LogoArriving(logo, resuming);
            }

            protected override OsuScreen CreateLoadableScreen() => new TestScreen(secondsDelay);

            private class TestScreen : OsuScreen
            {
                private readonly float secondsDelay;

                public TestScreen(float secondsDelay)
                {
                    this.secondsDelay = secondsDelay;

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DarkSlateGray,
                        Alpha = 0,
                    };
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    Thread.Sleep((int)(secondsDelay * 1000));
                }

                protected override void LogoArriving(OsuLogo logo, bool resuming)
                {
                    base.LogoArriving(logo, resuming);

                    Child.FadeInFromZero(200);
                }
            }
        }
    }
}
