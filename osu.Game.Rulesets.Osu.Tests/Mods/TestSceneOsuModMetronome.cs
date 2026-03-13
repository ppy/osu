// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModMetronome : OsuModTestScene
    {
        [TestCase(0.1)]
        [TestCase(0.5)]
        [TestCase(1.0)]
        public void TestMetronome(double volume)
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModMetronome
                {
                    MetronomeVolume = { Value = volume },
                },
                PassCondition = () => true,
                Autoplay = true,
            });
        }
    }
}
