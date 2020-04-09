// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcher : CatchSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(CatcherArea),
            typeof(CatcherSprite)
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new Catcher
            {
                RelativePositionAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
