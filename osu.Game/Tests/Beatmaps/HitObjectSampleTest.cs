// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osu.Game.Users;

namespace osu.Game.Tests.Beatmaps
{
    public abstract class HitObjectSampleTest : PlayerTestScene
    {
        protected abstract IResourceStore<byte[]> Resources { get; }
        protected LegacySkin Skin { get; private set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

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
        private SkinSourceDependencyContainer dependencies;
        private IBeatmap currentTestBeatmap;
        protected sealed override bool HasCustomSteps => true;

        protected sealed override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => new DependencyContainer(dependencies = new SkinSourceDependencyContainer(base.CreateChildDependencies(parent)));

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTestBeatmap;

        protected sealed override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new TestWorkingBeatmap(beatmapInfo, beatmapSkinResourceStore, beatmap, storyboard, Clock, Audio);

        protected void CreateTestWithBeatmap(string filename)
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
                    using (var reader = new LineBufferedReader(Resources.GetStream($"Resources/SampleLookups/{filename}")))
                        currentTestBeatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);

                    // populate ruleset for beatmap converters that require it to be present.
                    currentTestBeatmap.BeatmapInfo.Ruleset = rulesetStore.GetRuleset(currentTestBeatmap.BeatmapInfo.RulesetID);
                });
            });
        }

        protected void SetupSkins(string beatmapFile, string userFile)
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
                dependencies.SkinSource = new SkinProvidingContainer(Skin = new LegacySkin(userSkinInfo, userSkinResourceStore, Audio));
            });
        }

        protected void AssertBeatmapLookup(string name) => AddAssert($"\"{name}\" looked up from beatmap skin",
            () => !userSkinResourceStore.PerformedLookups.Contains(name) && beatmapSkinResourceStore.PerformedLookups.Contains(name));

        protected void AssertUserLookup(string name) => AddAssert($"\"{name}\" looked up from user skin",
            () => !beatmapSkinResourceStore.PerformedLookups.Contains(name) && userSkinResourceStore.PerformedLookups.Contains(name));

        protected void AssertNoLookup(string name) => AddAssert($"\"{name}\" not looked up",
            () => !beatmapSkinResourceStore.PerformedLookups.Contains(name) && !userSkinResourceStore.PerformedLookups.Contains(name));

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
