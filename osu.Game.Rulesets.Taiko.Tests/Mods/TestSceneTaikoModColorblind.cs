// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Mods;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModColorBlind : TaikoModTestScene
    {
        [Test]
        public void TestColorBlind() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModColorBlind(),
            Autoplay = true,
            PassCondition = () => true,
        });
    }
}
