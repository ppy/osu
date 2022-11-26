// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneRangeSlider : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.001f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(100)
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.1f
        };

        [Test]
        public void TestBasic()
        {
            AddStep("create Control", () => Child = new RangeSlider
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
                DefaultStringUpperBound = "End"
            });
            AddStep("Test Range", () =>
            {
                customStart.Value = 50;
                customEnd.Value = 75;
            });
            AddStep("Test nub pushing", () =>
            {
                customStart.Value = 90;
            });
        }
    }
}
