// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Mods;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public class TestSceneTaikoModHidden : TaikoModTestScene
    {
        [Test]
        public void TestDefaultBeatmapTest() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModHidden(),
            Autoplay = true,
            PassCondition = checkSomeAutoplayHits
        });

        private bool checkSomeAutoplayHits()
            => Player.ScoreProcessor.JudgedHits >= 4
               && Player.Results.All(result => result.Type == result.Judgement.MaxResult);
    }
}
