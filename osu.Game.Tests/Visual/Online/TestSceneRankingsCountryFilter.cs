// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Framework.Allocation;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsCountryFilter : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CountryFilter),
            typeof(CountryPill)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneRankingsCountryFilter()
        {
            var countryBindable = new Bindable<Country>();

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new CountryFilter
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Current = { BindTarget = countryBindable }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Some content",
                            Margin = new MarginPadding { Vertical = 20 }
                        }
                    }
                }
            });

            var country = new Country
            {
                FlagName = "BY",
                FullName = "Belarus"
            };
            var unknownCountry = new Country
            {
                FlagName = "CK",
                FullName = "Cook Islands"
            };

            AddStep("Set country", () => countryBindable.Value = country);
            AddStep("Set null country", () => countryBindable.Value = null);
            AddStep("Set country with no flag", () => countryBindable.Value = unknownCountry);
        }
    }
}
