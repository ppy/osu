// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneTextEditIndicator : ScreenTestScene
    {
        private DependencyContainer dependencies;
        private TextEditIndicator indicator;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#777"), Color4Extensions.FromHex("#aaa")),
                    RelativeSizeAxes = Axes.Both
                },
                indicator = new TextEditIndicator()
            };

            AddStep("Toggle Indicator", indicator.ToggleVisibility);
            AddStep("Clear Indicator", () => indicator.Text = string.Empty);
            AddStep("Random Text", () =>
            {
                indicator.Text += RNG.Next().ToString();
                indicator.Show();
            });
            AddStep("Flash", indicator.Flash);
            AddStep("Test", () =>
            {
                indicator.Text = "测试";
                indicator.Text = string.Empty;
                this.Delay(3000).Schedule(() => indicator.Text = "Hey!");
            });
        }
    }
}
