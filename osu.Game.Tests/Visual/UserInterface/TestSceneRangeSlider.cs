// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneRangeSlider : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.1f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(100)
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.1f
        };

        private RangeSlider rangeSlider = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create control", () => Child = rangeSlider = new RangeSlider
            {
                Width = 200,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3),
                LowerBound = customStart,
                UpperBound = customEnd,
                TooltipSuffix = "suffix",
                NubWidth = Nub.HEIGHT * 2,
                DefaultStringLowerBound = "Start",
                DefaultStringUpperBound = "End",
                MinRange = 10
            });
        }

        [Test]
        public void TestAdjustRange()
        {
            AddAssert("Initial lower bound is correct", () => rangeSlider.LowerBound.Value, () => Is.EqualTo(0).Within(0.1f));
            AddAssert("Initial upper bound is correct", () => rangeSlider.UpperBound.Value, () => Is.EqualTo(100).Within(0.1f));

            AddStep("Adjust range", () =>
            {
                customStart.Value = 50;
                customEnd.Value = 75;
            });

            AddAssert("Adjusted lower bound is correct", () => rangeSlider.LowerBound.Value, () => Is.EqualTo(50).Within(0.1f));
            AddAssert("Adjusted upper bound is correct", () => rangeSlider.UpperBound.Value, () => Is.EqualTo(75).Within(0.1f));

            AddStep("Test nub pushing", () =>
            {
                customStart.Value = 90;
            });

            AddAssert("Pushed lower bound is correct", () => rangeSlider.LowerBound.Value, () => Is.EqualTo(90).Within(0.1f));
            AddAssert("Pushed upper bound is correct", () => rangeSlider.UpperBound.Value, () => Is.EqualTo(100).Within(0.1f));
        }
    }
}
