// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserRanks : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableProfileScore), typeof(RanksSection) };

        public TestSceneUserRanks()
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
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = ranks = new RanksSection(),
                    },
                }
            });

            AddStep("Show cookiezi", () => ranks.User.Value = new User { Id = 124493 });
        }
    }
}
