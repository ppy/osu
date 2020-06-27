// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneHitObjectSamples : HitObjectSampleTest
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
        protected override IResourceStore<byte[]> Resources => TestResources.GetStore();

        /// <summary>
        /// Tests that a hitobject which provides no custom sample set retrieves samples from the user skin.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromUserSkin()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("hitobject-skin-sample.osu");

            AssertUserLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample set of 1 retrieves samples from the beatmap skin.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromBeatmap()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("hitobject-beatmap-sample.osu");

            AssertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample set of 1 retrieves samples from the user skin when the beatmap does not contain the sample.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromUserSkinFallback()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(null, expected_sample);

            CreateTestWithBeatmap("hitobject-beatmap-sample.osu");

            AssertUserLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a custom sample set of 2 retrieves the following samples from the beatmap skin:
        /// normal-hitnormal2
        /// normal-hitnormal
        /// </summary>
        [TestCase("normal-hitnormal2")]
        [TestCase("normal-hitnormal")]
        public void TestDefaultCustomSampleFromBeatmap(string expectedSample)
        {
            SetupSkins(expectedSample, expectedSample);

            CreateTestWithBeatmap("hitobject-beatmap-custom-sample.osu");

            AssertBeatmapLookup(expectedSample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a custom sample set of 2 retrieves the following samples from the user skin when the beatmap does not contain the sample:
        /// normal-hitnormal2
        /// normal-hitnormal
        /// </summary>
        [TestCase("normal-hitnormal2")]
        [TestCase("normal-hitnormal")]
        public void TestDefaultCustomSampleFromUserSkinFallback(string expectedSample)
        {
            SetupSkins(string.Empty, expectedSample);

            CreateTestWithBeatmap("hitobject-beatmap-custom-sample.osu");

            AssertUserLookup(expectedSample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample file retrieves the sample file from the beatmap skin.
        /// </summary>
        [Test]
        public void TestFileSampleFromBeatmap()
        {
            const string expected_sample = "hit_1.wav";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("file-beatmap-sample.osu");

            AssertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a default hitobject and control point causes <see cref="TestDefaultSampleFromUserSkin"/>.
        /// </summary>
        [Test]
        public void TestControlPointSampleFromSkin()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("controlpoint-skin-sample.osu");

            AssertUserLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a control point that provides a custom sample set of 1 causes <see cref="TestDefaultSampleFromBeatmap"/>.
        /// </summary>
        [Test]
        public void TestControlPointSampleFromBeatmap()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("controlpoint-beatmap-sample.osu");

            AssertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a control point that provides a custom sample of 2 causes <see cref="TestDefaultCustomSampleFromBeatmap"/>.
        /// </summary>
        [TestCase("normal-hitnormal2")]
        [TestCase("normal-hitnormal")]
        public void TestControlPointCustomSampleFromBeatmap(string sampleName)
        {
            SetupSkins(sampleName, sampleName);

            CreateTestWithBeatmap("controlpoint-beatmap-custom-sample.osu");

            AssertBeatmapLookup(sampleName);
        }

        /// <summary>
        /// Tests that a hitobject's custom sample overrides the control point's.
        /// </summary>
        [Test]
        public void TestHitObjectCustomSampleOverride()
        {
            const string expected_sample = "normal-hitnormal3";

            SetupSkins(expected_sample, expected_sample);

            CreateTestWithBeatmap("hitobject-beatmap-custom-sample-override.osu");

            AssertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that when a custom sample bank is used, both the normal and additional sounds will be looked up.
        /// </summary>
        [Test]
        public void TestHitObjectCustomSampleBank()
        {
            string[] expectedSamples =
            {
                "normal-hitnormal2",
                "normal-hitwhistle2"
            };

            SetupSkins(expectedSamples[0], expectedSamples[1]);

            CreateTestWithBeatmap("hitobject-beatmap-custom-sample-bank.osu");

            AssertBeatmapLookup(expectedSamples[0]);
            AssertUserLookup(expectedSamples[1]);
        }

        /// <summary>
        /// Tests that when a custom sample bank is used, but <see cref="GlobalSkinConfiguration.LayeredHitSounds"/> is disabled,
        /// only the additional sound will be looked up.
        /// </summary>
        [Test]
        public void TestHitObjectCustomSampleBankWithoutLayered()
        {
            const string expected_sample = "normal-hitwhistle2";
            const string unwanted_sample = "normal-hitnormal2";

            SetupSkins(expected_sample, unwanted_sample);
            disableLayeredHitSounds();

            CreateTestWithBeatmap("hitobject-beatmap-custom-sample-bank.osu");

            AssertBeatmapLookup(expected_sample);
            AssertNoLookup(unwanted_sample);
        }

        /// <summary>
        /// Tests that when a normal sample bank is used and <see cref="GlobalSkinConfiguration.LayeredHitSounds"/> is disabled,
        /// the normal sound will be looked up anyway.
        /// </summary>
        [Test]
        public void TestHitObjectNormalSampleBankWithoutLayered()
        {
            const string expected_sample = "normal-hitnormal";

            SetupSkins(expected_sample, expected_sample);
            disableLayeredHitSounds();

            CreateTestWithBeatmap("hitobject-beatmap-sample.osu");

            AssertBeatmapLookup(expected_sample);
        }

        private void disableLayeredHitSounds()
            => AddStep("set LayeredHitSounds to false", () => Skin.Configuration.ConfigDictionary[GlobalSkinConfiguration.LayeredHitSounds.ToString()] = "0");
    }
}
