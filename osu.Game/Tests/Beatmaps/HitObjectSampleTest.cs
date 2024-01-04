// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Beatmaps
{
    [HeadlessTest]
    public abstract partial class HitObjectSampleTest : PlayerTestScene, IStorageResourceProvider
    {
        protected abstract IResourceStore<byte[]> RulesetResources { get; }
        protected LegacySkin Skin { get; private set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        private readonly SkinInfo userSkinInfo = new SkinInfo();

        private readonly BeatmapInfo beatmapInfo = new BeatmapInfo
        {
            BeatmapSet = new BeatmapSetInfo(),
            Metadata = new BeatmapMetadata(),
        };

        private readonly TestResourceStore userSkinResourceStore = new TestResourceStore();
        private readonly TestResourceStore beatmapSkinResourceStore = new TestResourceStore();
        private SkinSourceDependencyContainer dependencies;
        private IBeatmap currentTestBeatmap;

        protected sealed override bool HasCustomSteps => true;
        protected override bool Autoplay => true;

        protected sealed override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => new DependencyContainer(dependencies = new SkinSourceDependencyContainer(base.CreateChildDependencies(parent)));

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTestBeatmap;

        protected sealed override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new TestWorkingBeatmap(beatmapInfo, beatmapSkinResourceStore, beatmap, storyboard, Clock, this);

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false);

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
                    using (var reader = new LineBufferedReader(RulesetResources.GetStream($"Resources/SampleLookups/{filename}")))
                        currentTestBeatmap = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);

                    // populate ruleset for beatmap converters that require it to be present.
                    var ruleset = rulesetStore.GetRuleset(currentTestBeatmap.BeatmapInfo.Ruleset.OnlineID);

                    Debug.Assert(ruleset != null);

                    currentTestBeatmap.BeatmapInfo.Ruleset = ruleset;
                });
            });

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));
            AddUntilStep("results displayed", () => Stack.CurrentScreen is ResultsScreen);
        }

        protected void SetupSkins(string beatmapFile, string userFile)
        {
            AddStep("setup skins", () =>
            {
                userSkinInfo.Files.Clear();
                if (!string.IsNullOrEmpty(userFile))
                    userSkinInfo.Files.Add(new RealmNamedFileUsage(new RealmFile { Hash = userFile }, userFile));

                Debug.Assert(beatmapInfo.BeatmapSet != null);

                beatmapInfo.BeatmapSet.Files.Clear();
                if (!string.IsNullOrEmpty(beatmapFile))
                    beatmapInfo.BeatmapSet.Files.Add(new RealmNamedFileUsage(new RealmFile { Hash = beatmapFile }, beatmapFile));

                // Need to refresh the cached skin source to refresh the skin resource store.
                dependencies.SkinSource = new SkinProvidingContainer(Skin = new LegacySkin(userSkinInfo, this));
            });
        }

        protected void AssertBeatmapLookup(string name) => AddAssert($"\"{name}\" looked up from beatmap skin",
            () => !userSkinResourceStore.PerformedLookups.Contains(name) && beatmapSkinResourceStore.PerformedLookups.Contains(name));

        protected void AssertUserLookup(string name) => AddAssert($"\"{name}\" looked up from user skin",
            () => !beatmapSkinResourceStore.PerformedLookups.Contains(name) && userSkinResourceStore.PerformedLookups.Contains(name));

        protected void AssertNoLookup(string name) => AddAssert($"\"{name}\" not looked up",
            () => !beatmapSkinResourceStore.PerformedLookups.Contains(name) && !userSkinResourceStore.PerformedLookups.Contains(name));

        #region IResourceStorageProvider

        public IRenderer Renderer => host.Renderer;
        public AudioManager AudioManager => Audio;
        public IResourceStore<byte[]> Files => userSkinResourceStore;
        public new IResourceStore<byte[]> Resources => base.Resources;
        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => throw new NotImplementedException();
        RealmAccess IStorageResourceProvider.RealmAccess => throw new NotImplementedException();

        #endregion

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

            public void Inject<T>(T instance) where T : class, IDependencyInjectionCandidate
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

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
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

        private class TestWorkingBeatmap : ClockBackedTestWorkingBeatmap, IStorageResourceProvider
        {
            private readonly BeatmapInfo skinBeatmapInfo;

            private readonly IStorageResourceProvider resources;

            public TestWorkingBeatmap(BeatmapInfo skinBeatmapInfo, IResourceStore<byte[]> accessMarkingResourceStore, IBeatmap beatmap, Storyboard storyboard, IFrameBasedClock referenceClock,
                                      IStorageResourceProvider resources)
                : base(beatmap, storyboard, referenceClock, resources.AudioManager)
            {
                this.skinBeatmapInfo = skinBeatmapInfo;
                Files = accessMarkingResourceStore;
                this.resources = resources;
            }

            protected internal override ISkin GetSkin() => new LegacyBeatmapSkin(skinBeatmapInfo, this);

            public IRenderer Renderer => resources.Renderer;
            public AudioManager AudioManager => resources.AudioManager;

            public IResourceStore<byte[]> Files { get; }

            public IResourceStore<byte[]> Resources => resources.Resources;

            public RealmAccess RealmAccess => resources.RealmAccess;

            public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => resources.CreateTextureLoaderStore(underlyingStore);
        }
    }
}
