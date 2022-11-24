// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePracticeRangeSliderComponent : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.001f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.001f
        };

        [Test]
        public void TestBasic()
        {
            AddStep("create Control", () => Child = new PracticeRangeSliderComponent(customStart, customEnd)
            {
                Width = 200,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3)
            });
            AddStep("Test Range", () =>
            {
                customStart.Value = 0.5f;
                customEnd.Value = 0.75f;
            });
            AddStep("Test nub pushing", () =>
            {
                customEnd.Value = 0.1f;
            });
            AddStep("Test nub pushing", () => customStart.Value = 0.7f);
        }
    }
}
