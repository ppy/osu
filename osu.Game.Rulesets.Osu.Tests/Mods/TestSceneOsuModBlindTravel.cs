// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModBlindTravel : OsuModTestScene
    {
        [Test]
        public void TestBlindTravel() => CreateModTest(new ModTestData
        {
            Mod = new OsuModBlindTravel(),
            PassCondition = () => true,
            Autoplay = false,
        });
    }
}
