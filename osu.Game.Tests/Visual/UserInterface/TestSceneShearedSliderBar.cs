// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearedSliderBar : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly BindableDouble current = new BindableDouble(5)
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 15
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new ShearedSliderBar<double>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = current,
                RelativeSizeAxes = Axes.X,
                Width = 0.4f
            };
        }
    }
}
