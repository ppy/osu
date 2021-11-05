// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public class TestSceneOnlinePlayBeatmapAvailabilityTracker : OsuTestScene
    {
        private RulesetStore rulesets;
        private TestBeatmapManager beatmaps;

        private string testBeatmapFile;
        private BeatmapInfo testBeatmapInfo;
        private BeatmapSetInfo testBeatmapSet;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();
        private OnlinePlayBeatmapAvailabilityTracker availabilityTracker;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, GameHost host)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.CacheAs<BeatmapManager>(beatmaps = new TestBeatmapManager(LocalStorage, ContextFactory, rulesets, API, audio, Resources, host, Beatmap.Default));
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            beatmaps.AllowImport = new TaskCompletionSource<bool>();

            testBeatmapFile = TestResources.GetQuickTestBeatmapForImport();

            testBeatmapInfo = getTestBeatmapInfo(testBeatmapFile);
            testBeatmapSet = testBeatmapInfo.BeatmapSet;

            var existing = beatmaps.QueryBeatmapSet(s => s.OnlineBeatmapSetID == testBeatmapSet.OnlineBeatmapSetID);
            if (existing != null)
                beatmaps.Delete(existing);

            selectedItem.Value = new PlaylistItem
            {
                Beatmap = { Value = testBeatmapInfo },
                Ruleset = { Value = testBeatmapInfo.Ruleset },
            };

            Child = availabilityTracker = new OnlinePlayBeatmapAvailabilityTracker
            {
                SelectedItem = { BindTarget = selectedItem, }
            };
        });

        [Test]
        public void TestBeatmapDownloadingFlow()
        {
            AddAssert("ensure beatmap unavailable", () => !beatmaps.IsAvailableLocally(testBeatmapSet));
            addAvailabilityCheckStep("state not downloaded", BeatmapAvailability.NotDownloaded);

            AddStep("start downloading", () => beatmaps.Download(testBeatmapSet));
            addAvailabilityCheckStep("state downloading 0%", () => BeatmapAvailability.Downloading(0.0f));

            AddStep("set progress 40%", () => ((TestDownloadRequest)beatmaps.GetExistingDownload(testBeatmapSet)).SetProgress(0.4f));
            addAvailabilityCheckStep("state downloading 40%", () => BeatmapAvailability.Downloading(0.4f));

            AddStep("finish download", () => ((TestDownloadRequest)beatmaps.GetExistingDownload(testBeatmapSet)).TriggerSuccess(testBeatmapFile));
            addAvailabilityCheckStep("state importing", BeatmapAvailability.Importing);

            AddStep("allow importing", () => beatmaps.AllowImport.SetResult(true));
            AddUntilStep("wait for import", () => beatmaps.CurrentImportTask?.IsCompleted == true);
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);
        }

        [Test]
        public void TestTrackerRespectsSoftDeleting()
        {
            AddStep("allow importing", () => beatmaps.AllowImport.SetResult(true));
            AddStep("import beatmap", () => beatmaps.Import(testBeatmapFile).Wait());
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);

            AddStep("delete beatmap", () => beatmaps.Delete(beatmaps.QueryBeatmapSet(b => b.OnlineBeatmapSetID == testBeatmapSet.OnlineBeatmapSetID)));
            addAvailabilityCheckStep("state not downloaded", BeatmapAvailability.NotDownloaded);

            AddStep("undelete beatmap", () => beatmaps.Undelete(beatmaps.QueryBeatmapSet(b => b.OnlineBeatmapSetID == testBeatmapSet.OnlineBeatmapSetID)));
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);
        }

        [Test]
        public void TestTrackerRespectsChecksum()
        {
            AddStep("allow importing", () => beatmaps.AllowImport.SetResult(true));

            AddStep("import altered beatmap", () =>
            {
                beatmaps.Import(TestResources.GetTestBeatmapForImport(true)).Wait();
            });
            addAvailabilityCheckStep("state still not downloaded", BeatmapAvailability.NotDownloaded);

            AddStep("recreate tracker", () => Child = availabilityTracker = new OnlinePlayBeatmapAvailabilityTracker
            {
                SelectedItem = { BindTarget = selectedItem }
            });
            addAvailabilityCheckStep("state not downloaded as well", BeatmapAvailability.NotDownloaded);
        }

        private void addAvailabilityCheckStep(string description, Func<BeatmapAvailability> expected)
        {
            AddUntilStep(description, () => availabilityTracker.Availability.Value.Equals(expected.Invoke()));
        }

        private static BeatmapInfo getTestBeatmapInfo(string archiveFile)
        {
            BeatmapInfo info;

            using (var archive = new ZipArchiveReader(File.OpenRead(archiveFile)))
            using (var stream = archive.GetStream("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var reader = new LineBufferedReader(stream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(reader);
                var beatmap = decoder.Decode(reader);

                info = beatmap.BeatmapInfo;
                info.BeatmapSet.Beatmaps = new List<BeatmapInfo> { info };
                info.BeatmapSet.Metadata = info.Metadata;
                info.MD5Hash = stream.ComputeMD5Hash();
                info.Hash = stream.ComputeSHA2Hash();
            }

            return info;
        }

        private class TestBeatmapManager : BeatmapManager
        {
            public TaskCompletionSource<bool> AllowImport = new TaskCompletionSource<bool>();

            public Task<ILive<BeatmapSetInfo>> CurrentImportTask { get; private set; }

            public TestBeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, [NotNull] AudioManager audioManager, IResourceStore<byte[]> resources, GameHost host = null, WorkingBeatmap defaultBeatmap = null)
                : base(storage, contextFactory, rulesets, api, audioManager, resources, host, defaultBeatmap)
            {
            }

            protected override BeatmapModelManager CreateBeatmapModelManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, GameHost host)
            {
                return new TestBeatmapModelManager(this, storage, contextFactory, rulesets, api, host);
            }

            protected override BeatmapModelDownloader CreateBeatmapModelDownloader(IModelImporter<BeatmapSetInfo> manager, IAPIProvider api, GameHost host)
            {
                return new TestBeatmapModelDownloader(manager, api, host);
            }

            internal class TestBeatmapModelDownloader : BeatmapModelDownloader
            {
                public TestBeatmapModelDownloader(IModelImporter<BeatmapSetInfo> importer, IAPIProvider apiProvider, GameHost gameHost)
                    : base(importer, apiProvider, gameHost)
                {
                }

                protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
                    => new TestDownloadRequest(set);
            }

            internal class TestBeatmapModelManager : BeatmapModelManager
            {
                private readonly TestBeatmapManager testBeatmapManager;

                public TestBeatmapModelManager(TestBeatmapManager testBeatmapManager, Storage storage, IDatabaseContextFactory databaseContextFactory, RulesetStore rulesetStore, IAPIProvider apiProvider, GameHost gameHost)
                    : base(storage, databaseContextFactory, rulesetStore, gameHost)
                {
                    this.testBeatmapManager = testBeatmapManager;
                }

                public override async Task<ILive<BeatmapSetInfo>> Import(BeatmapSetInfo item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
                {
                    await testBeatmapManager.AllowImport.Task.ConfigureAwait(false);
                    return await (testBeatmapManager.CurrentImportTask = base.Import(item, archive, lowPriority, cancellationToken)).ConfigureAwait(false);
                }
            }
        }

        private class TestDownloadRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
        {
            public new void SetProgress(float progress) => base.SetProgress(progress);
            public new void TriggerSuccess(string filename) => base.TriggerSuccess(filename);

            public TestDownloadRequest(IBeatmapSetInfo model)
                : base(model)
            {
            }

            protected override string Target => null;
        }
    }
}
