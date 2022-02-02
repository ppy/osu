// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModAimAssist : OsuModTestScene
    {
        [TestCase(0.1f)]
        [TestCase(0.5f)]
        [TestCase(1)]
        public void TestAimAssist(float strength)
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModAimAssist
                {
                    AssistStrength = { Value = strength },
                },
                PassCondition = () => true,
                Autoplay = false,
            });
        }
    }
}
