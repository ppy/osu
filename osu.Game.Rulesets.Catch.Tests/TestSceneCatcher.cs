// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcher : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatcherArea),
            typeof(CatcherSprite)
        };

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
