// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneLoader : ScreenTestScene
    {
        private TestLoader loader;

        [Cached]
        private OsuLogo logo;

        public TestSceneLoader()
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
            AddStep("load immediately", () =>
            {
                loader = new TestLoader();
                loader.AllowLoad.Set();

                LoadScreen(loader);
            });

            spinnerNotPresentOrHidden();

            AddUntilStep("loaded", () => loader.ScreenLoaded);
            AddUntilStep("not current", () => !loader.IsCurrentScreen());

            spinnerNotPresentOrHidden();
        }

        private void spinnerNotPresentOrHidden() =>
            AddAssert("spinner did not display", () => loader.LoadingSpinner == null || loader.LoadingSpinner.Alpha == 0);

        [Test]
        public void TestDelayedLoad()
        {
            AddStep("begin loading", () => LoadScreen(loader = new TestLoader()));
            AddUntilStep("wait for spinner visible", () => loader.LoadingSpinner?.Alpha > 0);
            AddStep("finish loading", () => loader.AllowLoad.Set());
            AddUntilStep("spinner gone", () => loader.LoadingSpinner?.Alpha == 0);
            AddUntilStep("loaded", () => loader.ScreenLoaded);
            AddUntilStep("not current", () => !loader.IsCurrentScreen());
        }

        private partial class TestLoader : Loader
        {
            public readonly ManualResetEventSlim AllowLoad = new ManualResetEventSlim();

            public LoadingSpinner LoadingSpinner => this.ChildrenOfType<LoadingSpinner>().FirstOrDefault();
            private TestScreen screen;

            public bool ScreenLoaded => screen.IsCurrentScreen();

            protected override OsuScreen CreateLoadableScreen() => screen = new TestScreen();
            protected override ShaderPrecompiler CreateShaderPrecompiler() => new TestShaderPrecompiler(AllowLoad);

            private partial class TestShaderPrecompiler : ShaderPrecompiler
            {
                private readonly ManualResetEventSlim allowLoad;

                public TestShaderPrecompiler(ManualResetEventSlim allowLoad)
                {
                    this.allowLoad = allowLoad;
                }

                protected override bool AllLoaded => allowLoad.IsSet;
            }

            private partial class TestScreen : OsuScreen
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
