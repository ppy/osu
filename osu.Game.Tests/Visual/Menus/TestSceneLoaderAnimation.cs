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
    public class TestSceneLoaderAnimation : ScreenTestScene
    {
        private TestLoader loader;

        [Cached]
        private OsuLogo logo;

        public TestSceneLoaderAnimation()
        {
            Child = logo = new OsuLogo
            {
                Alpha = 0,
                Depth = float.MinValue
            };
        }

        [Test]
        public void TestInstantLoad()
        {
            // visual only, very impossible to test this using asserts.

            AddStep("load immediately", () =>
            {
                loader = new TestLoader();
                loader.AllowLoad.Set();

                LoadScreen(loader);
            });
        }

        [Test]
        public void TestDelayedLoad()
        {
            AddStep("begin loading", () => LoadScreen(loader = new TestLoader()));
            AddUntilStep("wait for logo visible", () => loader.Logo?.Alpha > 0);
            AddStep("finish loading", () => loader.AllowLoad.Set());
            AddUntilStep("loaded", () => loader.Logo != null && loader.ScreenLoaded);
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
