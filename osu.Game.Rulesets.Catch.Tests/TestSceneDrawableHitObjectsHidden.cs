// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneDrawableHitObjectsHidden : TestSceneDrawableHitObjects
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[] { typeof(CatchModHidden) }).ToList();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = new[] { new CatchModHidden() };
        });
    }
}
