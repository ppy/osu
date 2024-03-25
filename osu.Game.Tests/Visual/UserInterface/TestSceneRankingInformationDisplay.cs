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
    public partial class TestSceneRankingInformationDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestBasic()
        {
            RankingInformationDisplay onlinePropertiesDisplay = null!;

            AddStep("create content", () => Child = onlinePropertiesDisplay = new RankingInformationDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            AddToggleStep("toggle pp eligibility", ranked => onlinePropertiesDisplay.EligibleForPP.Value = ranked);

            AddStep("set multiplier below 1", () => onlinePropertiesDisplay.ModMultiplier.Value = 0.5);
            AddStep("set multiplier to 1", () => onlinePropertiesDisplay.ModMultiplier.Value = 1);
            AddStep("set multiplier above 1", () => onlinePropertiesDisplay.ModMultiplier.Value = 1.5);

            AddSliderStep("set multiplier", 0, 2, 1d, multiplier =>
            {
                if (onlinePropertiesDisplay.IsNotNull())
                    onlinePropertiesDisplay.ModMultiplier.Value = multiplier;
            });
        }
    }
}
