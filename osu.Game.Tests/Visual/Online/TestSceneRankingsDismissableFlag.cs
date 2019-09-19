// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsDismissableFlag : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DismissableFlag),
        };

        public TestSceneRankingsDismissableFlag()
        {
            DismissableFlag flag;
            SpriteText text;

            var countryA = new Country
            {
                FlagName = "BY",
                FullName = "Belarus"
            };

            var countryB = new Country
            {
                FlagName = "US",
                FullName = "United States"
            };

            AddRange(new Drawable[]
            {
                flag = new DismissableFlag
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(30, 20),
                    Country = countryA,
                },
                text = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Invoked",
                    Font = OsuFont.GetFont(size: 30),
                    Alpha = 0,
                }
            });

            flag.Action += () => text.FadeIn().Then().FadeOut(1000, Easing.OutQuint);

            AddStep("Trigger click", () => flag.Click());
            AddStep("Change to country 2", () => flag.Country = countryB);
            AddStep("Change to country 1", () => flag.Country = countryA);
        }
    }
}
