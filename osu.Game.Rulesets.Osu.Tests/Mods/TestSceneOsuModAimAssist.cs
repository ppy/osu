// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModAimAssist : OsuModTestScene
    {
        [Test]
        public void TestAimAssist()
        {
            var mod = new OsuModAimAssist();

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mod = mod,
            });
        }
    }
}
