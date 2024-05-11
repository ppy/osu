// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaHitObjectSamples : HitObjectSampleTest
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();
        protected override IResourceStore<byte[]> RulesetResources => new DllResourceStore(Assembly.GetAssembly(typeof(TestSceneManiaHitObjectSamples)));

        /// <summary>
        /// Tests that when a normal sample bank is used, the normal hitsound will be looked up.
        /// </summary>
        [Test]
        public void TestManiaHitObjectNormalSampleBank()
        {
            const string expected_sample = "normal-hitnormal2";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("mania-hitobject-beatmap-normal-sample-bank.osu");

            AssertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that when a custom sample bank is used, layered hitsounds are not played
        /// (only the sample from the custom bank is looked up).
        /// </summary>
        [Test]
        public void TestManiaHitObjectCustomSampleBank()
        {
            const string expected_sample = "normal-hitwhistle2";
            const string unwanted_sample = "normal-hitnormal2";

            SetupSkins(expected_sample, unwanted_sample);

            CreateTestWithBeatmap("mania-hitobject-beatmap-custom-sample-bank.osu");

            AssertBeatmapLookup(expected_sample);
            AssertNoLookup(unwanted_sample);
        }
    }
}
