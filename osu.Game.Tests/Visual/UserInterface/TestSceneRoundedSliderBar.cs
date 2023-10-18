// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneRoundedSliderBar : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly BindableDouble current = new BindableDouble(5)
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 15
        };

        private RoundedSliderBar<double> slider = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = slider = new RoundedSliderBar<double>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = current,
                RelativeSizeAxes = Axes.X,
                Width = 0.4f
            };
        }

        [Test]
        public void TestNubDoubleClickRevertToDefault()
        {
            AddStep("set slider to 1", () => slider.Current.Value = 1);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<Nub>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("slider is default", () => slider.Current.IsDefault);
        }
    }
}
