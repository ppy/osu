// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Gameplay;
using osu.Game.Users;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneHitObjectSamples : OsuPlayerTestScene
    {
        private readonly SkinInfo userSkinInfo = new SkinInfo();

        private readonly BeatmapInfo beatmapInfo = new BeatmapInfo
        {
            BeatmapSet = new BeatmapSetInfo(),
            Metadata = new BeatmapMetadata
            {
                Author = User.SYSTEM_USER
            }
        };

        private readonly TestResourceStore userSkinResourceStore = new TestResourceStore();
        private readonly TestResourceStore beatmapSkinResourceStore = new TestResourceStore();

        protected override bool HasCustomSteps => true;

        private SkinSourceDependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => new DependencyContainer(dependencies = new SkinSourceDependencyContainer(base.CreateChildDependencies(parent)));

        /// <summary>
        /// Tests that a hitobject which provides no custom sample set retrieves samples from the user skin.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromUserSkin()
        {
            const string expected_sample = "normal-hitnormal";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("hitobject-skin-sample.osu");

            assertUserLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample set of 1 retrieves samples from the beatmap skin.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromBeatmap()
        {
            const string expected_sample = "normal-hitnormal";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("hitobject-beatmap-sample.osu");

            assertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample set of 1 retrieves samples from the user skin when the beatmap does not contain the sample.
        /// </summary>
        [Test]
        public void TestDefaultSampleFromUserSkinFallback()
        {
            const string expected_sample = "normal-hitnormal";

            setupSkins(null, expected_sample);

            createTestWithBeatmap("hitobject-beatmap-sample.osu");

            assertUserLookup(expected_sample);
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
            setupSkins(expectedSample, expectedSample);

            createTestWithBeatmap("hitobject-beatmap-custom-sample.osu");

            assertBeatmapLookup(expectedSample);
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
            setupSkins(string.Empty, expectedSample);

            createTestWithBeatmap("hitobject-beatmap-custom-sample.osu");

            assertUserLookup(expectedSample);
        }

        /// <summary>
        /// Tests that a hitobject which provides a sample file retrieves the sample file from the beatmap skin.
        /// </summary>
        [Test]
        public void TestFileSampleFromBeatmap()
        {
            const string expected_sample = "hit_1.wav";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("file-beatmap-sample.osu");

            assertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a default hitobject and control point causes <see cref="TestDefaultSampleFromUserSkin"/>.
        /// </summary>
        [Test]
        public void TestControlPointSampleFromSkin()
        {
            const string expected_sample = "normal-hitnormal";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("controlpoint-skin-sample.osu");

            assertUserLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a control point that provides a custom sample set of 1 causes <see cref="TestDefaultSampleFromBeatmap"/>.
        /// </summary>
        [Test]
        public void TestControlPointSampleFromBeatmap()
        {
            const string expected_sample = "normal-hitnormal";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("controlpoint-beatmap-sample.osu");

            assertBeatmapLookup(expected_sample);
        }

        /// <summary>
        /// Tests that a control point that provides a custom sample of 2 causes <see cref="TestDefaultCustomSampleFromBeatmap"/>.
        /// </summary>
        [TestCase("normal-hitnormal2")]
        [TestCase("normal-hitnormal")]
        public void TestControlPointCustomSampleFromBeatmap(string sampleName)
        {
            setupSkins(sampleName, sampleName);

            createTestWithBeatmap("controlpoint-beatmap-custom-sample.osu");

            assertBeatmapLookup(sampleName);
        }

        /// <summary>
        /// Tests that a hitobject's custom sample overrides the control point's.
        /// </summary>
        [Test]
        public void TestHitObjectCustomSampleOverride()
        {
            const string expected_sample = "normal-hitnormal3";

            setupSkins(expected_sample, expected_sample);

            createTestWithBeatmap("hitobject-beatmap-custom-sample-override.osu");

            assertBeatmapLookup(expected_sample);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTestBeatmap;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new TestWorkingBeatmap(beatmapInfo, beatmapSkinResourceStore, beatmap, storyboard, Clock, Audio);

        private IBeatmap currentTestBeatmap;

        private void createTestWithBeatmap(string filename)
        {
            CreateTest(() =>
            {
                AddStep("clear performed lookups", () =>
                {
                    userSkinResourceStore.PerformedLookups.Clear();
                    beatmapSkinResourceStore.PerformedLookups.Clear();
                });

                AddStep($"load {filename}", () =>
                {
                    using (var reader = new LineBufferedReader(TestResources.OpenResource($"SampleLookups/{filename}")))
                        currentTestBeatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
                });
            });
        }

        private void setupSkins(string beatmapFile, string userFile)
        {
            AddStep("setup skins", () =>
            {
                userSkinInfo.Files = new List<SkinFileInfo>
                {
                    new SkinFileInfo
                    {
                        Filename = userFile,
                        FileInfo = new IO.FileInfo { Hash = userFile }
                    }
                };

                beatmapInfo.BeatmapSet.Files = new List<BeatmapSetFileInfo>
                {
                    new BeatmapSetFileInfo
                    {
                        Filename = beatmapFile,
                        FileInfo = new IO.FileInfo { Hash = beatmapFile }
                    }
                };

                // Need to refresh the cached skin source to refresh the skin resource store.
                dependencies.SkinSource = new SkinProvidingContainer(new LegacySkin(userSkinInfo, userSkinResourceStore, Audio));
            });
        }

        private void assertBeatmapLookup(string name) => AddAssert($"\"{name}\" looked up from beatmap skin",
            () => !userSkinResourceStore.PerformedLookups.Contains(name) && beatmapSkinResourceStore.PerformedLookups.Contains(name));

        private void assertUserLookup(string name) => AddAssert($"\"{name}\" looked up from user skin",
            () => !beatmapSkinResourceStore.PerformedLookups.Contains(name) && userSkinResourceStore.PerformedLookups.Contains(name));

        private class SkinSourceDependencyContainer : IReadOnlyDependencyContainer
        {
            public ISkinSource SkinSource;

            private readonly IReadOnlyDependencyContainer fallback;

            public SkinSourceDependencyContainer(IReadOnlyDependencyContainer fallback)
            {
                this.fallback = fallback;
            }

            public object Get(Type type)
            {
                if (type == typeof(ISkinSource))
                    return SkinSource;

                return fallback.Get(type);
            }

            public object Get(Type type, CacheInfo info)
            {
                if (type == typeof(ISkinSource))
                    return SkinSource;

                return fallback.Get(type, info);
            }

            public void Inject<T>(T instance) where T : class
            {
                // Never used directly
            }
        }

        private class TestResourceStore : IResourceStore<byte[]>
        {
            public readonly List<string> PerformedLookups = new List<string>();

            public byte[] Get(string name)
            {
                markLookup(name);
                return Array.Empty<byte>();
            }

            public Task<byte[]> GetAsync(string name)
            {
                markLookup(name);
                return Task.FromResult(Array.Empty<byte>());
            }

            public Stream GetStream(string name)
            {
                markLookup(name);
                return new MemoryStream();
            }

            private void markLookup(string name) => PerformedLookups.Add(name.Substring(name.LastIndexOf(Path.DirectorySeparatorChar) + 1));

            public IEnumerable<string> GetAvailableResources() => Enumerable.Empty<string>();

            public void Dispose()
            {
            }
        }

        private class TestWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly BeatmapInfo skinBeatmapInfo;
            private readonly IResourceStore<byte[]> resourceStore;

            public TestWorkingBeatmap(BeatmapInfo skinBeatmapInfo, IResourceStore<byte[]> resourceStore, IBeatmap beatmap, Storyboard storyboard, IFrameBasedClock referenceClock, AudioManager audio,
                                      double length = 60000)
                : base(beatmap, storyboard, referenceClock, audio, length)
            {
                this.skinBeatmapInfo = skinBeatmapInfo;
                this.resourceStore = resourceStore;
            }

            protected override ISkin GetSkin() => new LegacyBeatmapSkin(skinBeatmapInfo, resourceStore, AudioManager);
        }
    }
}
