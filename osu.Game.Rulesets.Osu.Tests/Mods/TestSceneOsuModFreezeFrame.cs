// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModFreezeFrame : OsuModTestScene
    {
        [TestCase(OsuModFreezeFrame.BeatDivisor.Quarter_Measure)]
        [TestCase(OsuModFreezeFrame.BeatDivisor.Single_Measure)]
        [TestCase(OsuModFreezeFrame.BeatDivisor.Quadruple_Measure)]
        public void TestFreezeFrequency(OsuModFreezeFrame.BeatDivisor divisor)
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModFreezeFrame { Divisor = { Value = divisor } },
                PassCondition = checkSomeHit,
                Autoplay = true
            });
        }

        [Test]
        public void TestWithHidden()
        {
            var mods = new List<Mod> { new OsuModHidden(), new OsuModFreezeFrame { Divisor = { Value = OsuModFreezeFrame.BeatDivisor.Quadruple_Measure } } };
            CreateModTest(new ModTestData
            {
                Mods = mods,
                PassCondition = checkSomeHit,
                Autoplay = true
            });
        }

        private bool checkSomeHit() => Player.ScoreProcessor.JudgedHits >= 8;
    }
}
