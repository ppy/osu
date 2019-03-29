// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestCaseLoaderAnimation : ScreenTestCase
    {
        private TestLoader loader;

        [Cached]
        private OsuLogo logo;

        public TestCaseLoaderAnimation()
        {
            Add(logo = new OsuLogo { Depth = float.MinValue });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bool logoVisible = false;
            AddStep("almost instant display", () => LoadScreen(loader = new TestLoader(250)));
            AddUntilStep("loaded", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                return loader.Logo != null && loader.ScreenLoaded;
            });
            AddAssert("logo not visible", () => !logoVisible);

            AddStep("short load", () => LoadScreen(loader = new TestLoader(800)));
            AddUntilStep("loaded", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                return loader.Logo != null && loader.ScreenLoaded;
            });
            AddAssert("logo visible", () => logoVisible);
            AddUntilStep("logo gone", () => loader.Logo?.Alpha == 0);

            AddStep("longer load", () => LoadScreen(loader = new TestLoader(1400)));
            AddUntilStep("loaded", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                return loader.Logo != null && loader.ScreenLoaded;
            });
            AddAssert("logo visible", () => logoVisible);
            AddUntilStep("logo gone", () => loader.Logo?.Alpha == 0);
        }

        private class TestLoader : Loader
        {
            private readonly double delay;

            public OsuLogo Logo;
            private TestScreen screen;

            public bool ScreenLoaded => screen.IsCurrentScreen();

            public TestLoader(double delay)
            {
                this.delay = delay;
            }

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                Logo = logo;
                base.LogoArriving(logo, resuming);
            }

            protected override OsuScreen CreateLoadableScreen() => screen = new TestScreen();
            protected override ShaderPrecompiler CreateShaderPrecompiler() => new TestShaderPrecompiler(delay);

            private class TestShaderPrecompiler : ShaderPrecompiler
            {
                private readonly double delay;
                private double startTime;

                public TestShaderPrecompiler(double delay)
                {
                    this.delay = delay;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    startTime = Time.Current;
                }

                protected override bool AllLoaded => Time.Current > startTime + delay;
            }

            private class TestScreen : OsuScreen
            {
                public TestScreen()
                {
                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DarkSlateGray,
                        Alpha = 0,
                    };
                }

                protected override void LogoArriving(OsuLogo logo, bool resuming)
                {
                    base.LogoArriving(logo, resuming);
                    InternalChild.FadeInFromZero(200);
                }
            }
        }
    }
}
