// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneScoreMultiplierDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestBasic()
        {
            ScoreMultiplierDisplay multiplierDisplay = null!;

            AddStep("create content", () => Child = multiplierDisplay = new ScoreMultiplierDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            AddStep("set multiplier below 1", () => multiplierDisplay.Current.Value = 0.5);
            AddStep("set multiplier to 1", () => multiplierDisplay.Current.Value = 1);
            AddStep("set multiplier above 1", () => multiplierDisplay.Current.Value = 1.5);

            AddSliderStep("set multiplier", 0, 2, 1d, multiplier =>
            {
                if (multiplierDisplay.IsNotNull())
                    multiplierDisplay.Current.Value = multiplier;
            });
        }
    }
}
