// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDifficultyAdjust : TestSceneOsuModSandbox
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Append(typeof(OsuModDifficultyAdjust)).ToList();

        public TestSceneOsuModDifficultyAdjust()
            : base(new OsuModDifficultyAdjust())
        {
        }
    }
}
