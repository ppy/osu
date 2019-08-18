// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneHitCircleHidden : TestSceneHitCircle
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[] { typeof(OsuModHidden) }).ToList();

        public TestSceneHitCircleHidden()
        {
            Mods.Value = new[] { new OsuModHidden() };
        }
    }
}
