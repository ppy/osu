// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
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
            Child = logo = new OsuLogo { Depth = float.MinValue };
        }

        [Test]
        public void TestInstantLoad()
        {
            bool logoVisible = false;

            AddStep("begin loading", () =>
            {
                loader = new TestLoader();
                loader.AllowLoad.Set();

                LoadScreen(loader);
            });

            AddAssert("loaded", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                return loader.Logo != null && loader.ScreenLoaded;
            });

            AddAssert("logo was not visible", () => !logoVisible);
        }

        [Test]
        public void TestShortLoad()
        {
            bool logoVisible = false;

            AddStep("begin loading", () => LoadScreen(loader = new TestLoader()));
            AddWaitStep("wait", 2);
            AddStep("finish loading", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                loader.AllowLoad.Set();
            });

            AddAssert("loaded", () => loader.Logo != null && loader.ScreenLoaded);
            AddAssert("logo was visible", () => logoVisible);
            AddUntilStep("logo gone", () => loader.Logo?.Alpha == 0);
        }

        [Test]
        public void TestLongLoad()
        {
            bool logoVisible = false;

            AddStep("begin loading", () => LoadScreen(loader = new TestLoader()));
            AddWaitStep("wait", 10);
            AddStep("finish loading", () =>
            {
                logoVisible = loader.Logo?.Alpha > 0;
                loader.AllowLoad.Set();
            });

            AddAssert("loaded", () => loader.Logo != null && loader.ScreenLoaded);
            AddAssert("logo was visible", () => logoVisible);
            AddUntilStep("logo gone", () => loader.Logo?.Alpha == 0);
        }

        private class TestLoader : Loader
        {
            public readonly ManualResetEventSlim AllowLoad = new ManualResetEventSlim();

            public OsuLogo Logo;
            private TestScreen screen;

            public bool ScreenLoaded => screen.IsCurrentScreen();

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                Logo = logo;
                base.LogoArriving(logo, resuming);
            }

            protected override OsuScreen CreateLoadableScreen() => screen = new TestScreen();
            protected override ShaderPrecompiler CreateShaderPrecompiler() => new TestShaderPrecompiler(AllowLoad);

            private class TestShaderPrecompiler : ShaderPrecompiler
            {
                private readonly ManualResetEventSlim allowLoad;

                public TestShaderPrecompiler(ManualResetEventSlim allowLoad)
                {
                    this.allowLoad = allowLoad;
                }

                protected override bool AllLoaded => allowLoad.IsSet;
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
