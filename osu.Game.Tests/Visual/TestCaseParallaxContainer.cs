// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Tests.Visual
{
    public class TestCaseParallaxContainer : OsuTestCase
    {
        public TestCaseParallaxContainer()
        {
            ParallaxContainer parallax;

            Add(parallax = new ParallaxContainer
            {
                Child = new BackgroundScreenDefault { Alpha = 0.8f }
            });

            AddStep("default parallax", () => parallax.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT);
            AddStep("high parallax", () => parallax.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * 10);
            AddStep("no parallax", () => parallax.ParallaxAmount = 0);
            AddStep("negative parallax", () => parallax.ParallaxAmount = -ParallaxContainer.DEFAULT_PARALLAX_AMOUNT);
        }
    }
}
