// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public class TestSceneMultiplayerBeatmapTracker : OsuTestScene
    {
        private RulesetStore rulesets;
        private TestBeatmapManager beatmaps;

        private string testBeatmapFile;
        private BeatmapInfo testBeatmapInfo;
        private BeatmapSetInfo testBeatmapSet;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();
        private MultiplayerBeatmapTracker tracker;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, GameHost host)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.CacheAs<BeatmapManager>(beatmaps = new TestBeatmapManager(LocalStorage, ContextFactory, rulesets, API, audio, host, Beatmap.Default));
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            beatmaps.AllowImport = new TaskCompletionSource<bool>();

            testBeatmapFile = getTestBeatmapOsz();

            testBeatmapInfo = new TestBeatmap(Ruleset.Value).BeatmapInfo;
            testBeatmapSet = testBeatmapInfo.BeatmapSet;

            var existing = beatmaps.QueryBeatmapSet(s => s.OnlineBeatmapSetID == testBeatmapSet.OnlineBeatmapSetID);
            if (existing != null)
                beatmaps.Delete(existing);

            selectedItem.Value = new PlaylistItem
            {
                Beatmap = { Value = testBeatmapInfo },
                Ruleset = { Value = testBeatmapInfo.Ruleset },
            };

            Child = tracker = new MultiplayerBeatmapTracker
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
            AddUntilStep("wait for import", () => beatmaps.IsAvailableLocally(testBeatmapSet));
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);
        }

        [Test]
        public void TestTrackerRespectsSoftDeleting()
        {
            AddStep("allow importing", () => beatmaps.AllowImport.SetResult(true));
            AddStep("import beatmap", () => beatmaps.Import(testBeatmapSet).Wait());
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);

            AddStep("delete beatmap", () => beatmaps.Delete(testBeatmapSet));
            addAvailabilityCheckStep("state not downloaded", BeatmapAvailability.NotDownloaded);

            AddStep("undelete beatmap", () => beatmaps.Undelete(testBeatmapSet));
            addAvailabilityCheckStep("state locally available", BeatmapAvailability.LocallyAvailable);
        }

        [Test]
        public void TestTrackerRespectsChecksum()
        {
            AddStep("allow importing", () => beatmaps.AllowImport.SetResult(true));

            BeatmapInfo wrongBeatmap = null;

            AddStep("import wrong checksum beatmap", () =>
            {
                wrongBeatmap = new TestBeatmap(Ruleset.Value).BeatmapInfo;
                wrongBeatmap.MD5Hash = "1337";

                beatmaps.Import(wrongBeatmap.BeatmapSet).Wait();
            });
            AddAssert("wrong beatmap available", () => beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == wrongBeatmap.OnlineBeatmapID) != null);
            addAvailabilityCheckStep("state still not downloaded", BeatmapAvailability.NotDownloaded);

            AddStep("recreate tracker", () => Child = tracker = new MultiplayerBeatmapTracker
            {
                SelectedItem = { BindTarget = selectedItem }
            });
            addAvailabilityCheckStep("state not downloaded as well", BeatmapAvailability.NotDownloaded);
        }

        private void addAvailabilityCheckStep(string description, Func<BeatmapAvailability> expected)
        {
            AddAssert(description, () => tracker.Availability.Value.Equals(expected.Invoke()));
        }

        private string getTestBeatmapOsz()
        {
            var filename = Path.GetTempFileName() + ".osz";

            using (var stream = TestResources.OpenResource("Archives/test-beatmap.osz"))
            using (var file = File.Create(filename))
                stream.CopyTo(file);

            return filename;
        }

        private class TestBeatmapManager : BeatmapManager
        {
            public TaskCompletionSource<bool> AllowImport = new TaskCompletionSource<bool>();

            protected override ArchiveDownloadRequest<BeatmapSetInfo> CreateDownloadRequest(BeatmapSetInfo set, bool minimiseDownloadSize)
                => new TestDownloadRequest(set);

            public TestBeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, [NotNull] AudioManager audioManager, GameHost host = null, WorkingBeatmap defaultBeatmap = null, bool performOnlineLookups = false)
                : base(storage, contextFactory, rulesets, api, audioManager, host, defaultBeatmap, performOnlineLookups)
            {
            }

            public override async Task<BeatmapSetInfo> Import(BeatmapSetInfo item, ArchiveReader archive = null, CancellationToken cancellationToken = default)
            {
                await AllowImport.Task;
                return await base.Import(item, archive, cancellationToken);
            }
        }

        private class TestDownloadRequest : ArchiveDownloadRequest<BeatmapSetInfo>
        {
            public new void SetProgress(float progress) => base.SetProgress(progress);

            public TestDownloadRequest(BeatmapSetInfo model)
                : base(model)
            {
            }

            protected override string Target => null;
        }
    }
}
