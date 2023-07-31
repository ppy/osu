// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Database
{
    [HeadlessTest]
    public partial class BackgroundBeatmapProcessorTests : OsuTestScene, ILocalUserPlayInfo
    {
        public IBindable<bool> IsPlaying => isPlaying;

        private readonly Bindable<bool> isPlaying = new Bindable<bool>();

        private BeatmapSetInfo importedSet = null!;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu)
        {
            importedSet = BeatmapImportHelper.LoadQuickOszIntoOsu(osu).GetResultSafely();
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Set not playing", () => isPlaying.Value = false);
        }

        [Test]
        public void TestDifficultyProcessing()
        {
            AddAssert("Difficulty is initially set", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });

            AddStep("Reset difficulty", () =>
            {
                Realm.Write(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    foreach (var b in beatmapSetInfo.Beatmaps)
                        b.StarRating = -1;
                });
            });

            AddStep("Run background processor", () =>
            {
                Add(new TestBackgroundBeatmapProcessor());
            });

            AddUntilStep("wait for difficulties repopulated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });
        }

        [Test]
        public void TestDifficultyProcessingWhilePlaying()
        {
            AddAssert("Difficulty is initially set", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });

            AddStep("Set playing", () => isPlaying.Value = true);

            AddStep("Reset difficulty", () =>
            {
                Realm.Write(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    foreach (var b in beatmapSetInfo.Beatmaps)
                        b.StarRating = -1;
                });
            });

            AddStep("Run background processor", () =>
            {
                Add(new TestBackgroundBeatmapProcessor());
            });

            AddWaitStep("wait some", 500);

            AddAssert("Difficulty still not populated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating == -1);
                });
            });

            AddStep("Set not playing", () => isPlaying.Value = false);

            AddUntilStep("wait for difficulties repopulated", () =>
            {
                return Realm.Run(r =>
                {
                    var beatmapSetInfo = r.Find<BeatmapSetInfo>(importedSet.ID)!;
                    return beatmapSetInfo.Beatmaps.All(b => b.StarRating > 0);
                });
            });
        }

        public partial class TestBackgroundBeatmapProcessor : BackgroundBeatmapProcessor
        {
            protected override int TimeToSleepDuringGameplay => 10;
        }
    }
}
