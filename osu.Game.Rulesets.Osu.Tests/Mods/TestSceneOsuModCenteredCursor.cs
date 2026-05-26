// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModCenteredCursor : OsuModTestScene
    {
        [Test]
        public void TestOsuModCenteredCursor() => CreateModTest(new ModTestData
        {
            Mod = new OsuModCenteredCursor(),
            Autoplay = true,
            PassCondition = () => true
        });
    }
}
