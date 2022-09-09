// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModFreezeFrame : OsuModTestScene
    {
        [TestCase(0.5f)]
        [TestCase(1)]
        [TestCase(2)]
        public void TestFreezeFrequency(float beatMeasure)
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModFreezeFrame
                {
                    BeatDivisor = { Value = beatMeasure }
                },
                PassCondition = () => true,
                Autoplay = true
            });
        }
    }
}
