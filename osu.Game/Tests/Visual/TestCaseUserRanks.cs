﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Users;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseUserRanks : OsuTestCase
    {
        public override string Description => "showing your latest achievements";

        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableScore), typeof(RanksSection) };

        public TestCaseUserRanks()
        {
            RanksSection ranks;

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.2f)
                    },
                    new ScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = ranks = new RanksSection(),
                    },
                }
            });

            AddStep("Show cookiezi", () => ranks.User = new User { Id = 124493 });
        }
    }
}
