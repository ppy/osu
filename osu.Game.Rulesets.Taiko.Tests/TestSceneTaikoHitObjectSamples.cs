// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneTaikoHitObjectSamples : HitObjectSampleTest
    {
        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();

        protected override IResourceStore<byte[]> RulesetResources => new DllResourceStore(Assembly.GetAssembly(typeof(TestSceneTaikoHitObjectSamples)));

        [TestCase("taiko-normal-hitnormal")]
        [TestCase("hitnormal")]
        public void TestDefaultCustomSampleFromBeatmap(string expectedSample)
        {
            SetupSkins(expectedSample, expectedSample);

            CreateTestWithBeatmap("taiko-hitobject-beatmap-custom-sample-bank.osu");

            AssertBeatmapLookup(expectedSample);
        }

        [TestCase("taiko-normal-hitnormal")]
        [TestCase("hitnormal")]
        public void TestDefaultCustomSampleFromUserSkinFallback(string expectedSample)
        {
            SetupSkins(string.Empty, expectedSample);

            CreateTestWithBeatmap("taiko-hitobject-beatmap-custom-sample-bank.osu");

            AssertUserLookup(expectedSample);
        }

        [TestCase("taiko-normal-hitnormal2")]
        public void TestUserSkinLookupIgnoresSampleBank(string unwantedSample)
        {
            SetupSkins(string.Empty, unwantedSample);

            CreateTestWithBeatmap("taiko-hitobject-beatmap-custom-sample-bank.osu");

            AssertNoLookup(unwantedSample);
        }
    }
}
