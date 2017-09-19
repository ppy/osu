// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    internal class TestCaseCatcher : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatcherArea),
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Children = new Drawable[]
            {
                new CatchInputManager(rulesets.GetRuleset(2))
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new CatcherArea
                    {
                        RelativePositionAxes = Axes.Both,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Size = new Vector2(1, 0.2f),
                    }
                },
            };
        }
    }
}
