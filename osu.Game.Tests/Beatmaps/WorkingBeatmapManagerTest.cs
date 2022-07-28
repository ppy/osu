// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Beatmaps
{
    [HeadlessTest]
    public class WorkingBeatmapManagerTest : OsuTestScene
    {
        private BeatmapManager beatmaps = null!;

        private BeatmapSetInfo importedSet = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio, RulesetStore rulesets)
        {
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });
        }

        [Test]
        public void TestGetWorkingBeatmap() => AddStep("run test", () =>
        {
            Assert.That(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()), Is.Not.Null);
        });

        [Test]
        public void TestCachedRetrievalNoFiles() => AddStep("run test", () =>
        {
            var beatmap = importedSet.Beatmaps.First();

            Assert.That(beatmap.BeatmapSet?.Files, Is.Empty);

            var first = beatmaps.GetWorkingBeatmap(beatmap);
            var second = beatmaps.GetWorkingBeatmap(beatmap);

            Assert.That(first, Is.SameAs(second));
            Assert.That(first.BeatmapInfo.BeatmapSet?.Files, Has.Count.GreaterThan(0));
        });

        [Test]
        public void TestCachedRetrievalWithFiles() => AddStep("run test", () =>
        {
            var beatmap = Realm.Run(r => r.Find<BeatmapInfo>(importedSet.Beatmaps.First().ID).Detach());

            Assert.That(beatmap.BeatmapSet?.Files, Has.Count.GreaterThan(0));

            var first = beatmaps.GetWorkingBeatmap(beatmap);
            var second = beatmaps.GetWorkingBeatmap(beatmap);

            Assert.That(first, Is.SameAs(second));
            Assert.That(first.BeatmapInfo.BeatmapSet?.Files, Has.Count.GreaterThan(0));
        });

        [Test]
        public void TestForcedRefetchRetrievalNoFiles() => AddStep("run test", () =>
        {
            var beatmap = importedSet.Beatmaps.First();

            Assert.That(beatmap.BeatmapSet?.Files, Is.Empty);

            var first = beatmaps.GetWorkingBeatmap(beatmap);
            var second = beatmaps.GetWorkingBeatmap(beatmap, true);
            Assert.That(first, Is.Not.SameAs(second));
        });

        [Test]
        public void TestForcedRefetchRetrievalWithFiles() => AddStep("run test", () =>
        {
            var beatmap = Realm.Run(r => r.Find<BeatmapInfo>(importedSet.Beatmaps.First().ID).Detach());

            Assert.That(beatmap.BeatmapSet?.Files, Has.Count.GreaterThan(0));

            var first = beatmaps.GetWorkingBeatmap(beatmap);
            var second = beatmaps.GetWorkingBeatmap(beatmap, true);
            Assert.That(first, Is.Not.SameAs(second));
        });
    }
}
