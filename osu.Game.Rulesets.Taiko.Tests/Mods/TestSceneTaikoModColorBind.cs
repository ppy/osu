// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Mods;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModColorBind : TaikoModTestScene
    {
        [Test]
        public void TestColorBind() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModColorBind(),
            Autoplay = true,
            PassCondition = () => true,
        });
    }
}
