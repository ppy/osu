// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using M.Resources.Fonts;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Mf;

namespace osu.Game.Tests.Visual.Mvis
{
    public partial class TestSceneFontInfoTooltip : ScreenTestScene
    {
        private DependencyContainer? dependencies;
        private FontInfoTooltip? indicator;

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
                indicator = new FontInfoTooltip
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };

            AddStep("Toggle Indicator", indicator.ToggleVisibility);
            AddStep("Set Font", () => indicator.SetContent(new FakeFont()));
        }

        private class FakeFont : Font
        {
            public FakeFont()
            {
                Name = "Torus";
                Author = "Paulo Goode";
                Homepage = "https://paulogoode.com/torus/";
                FamilyName = "Torus";
            }
        }
    }
}
