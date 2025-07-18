// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Extensions;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectGrouping : SongSelectTestScene
    {
        private BeatmapCarouselFilterGrouping grouping => Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

        [Test]
        public void TestCollectionGrouping()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("add collections", () =>
            {
                beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray();

                Realm.Write(r =>
                {
                    r.RemoveAll<BeatmapCollection>();
                    r.Add(new BeatmapCollection("My Collection #1", beatmapSets[0].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #2", beatmapSets[1].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #3"));
                });
            });

            LoadSongSelect();
            GroupBy(GroupMode.Collections);
            WaitForFiltering();

            AddAssert("first collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #1");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[0]);
            });

            AddAssert("second collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #2");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[1]);
            });

            AddAssert("third collection not present", () => grouping.GroupItems.All(g => g.Key.Title != "My Collection #3"));

            AddAssert("no-collection group present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "Not in collection");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[2]);
            });
        }

        [Test]
        public void TestCollectionGroupingUpdatesOnChange()
        {
            ImportBeatmapForRuleset(0);

            BeatmapSetInfo beatmapSet = null!;

            AddStep("add collections", () =>
            {
                beatmapSet = Beatmaps.GetAllUsableBeatmapSets().Single();

                Realm.Write(r =>
                {
                    r.RemoveAll<BeatmapCollection>();
                    r.Add(new BeatmapCollection("My Collection #4"));
                });
            });

            LoadSongSelect();
            GroupBy(GroupMode.Collections);
            WaitForFiltering();

            AddAssert("collection not present", () => grouping.GroupItems.All(g => g.Key.Title != "My Collection #4"));

            AddAssert("no-collection group present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "Not in collection");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSet);
            });

            AddStep("add beatmap to collection", () =>
            {
                Realm.Write(r =>
                {
                    var collection = r.All<BeatmapCollection>().Single();
                    collection.BeatmapMD5Hashes.AddRange(beatmapSet.Beatmaps.Select(b => b.MD5Hash));
                });
            });

            WaitForFiltering();

            AddAssert("collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #4");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSet);
            });

            AddAssert("no-collection group not present", () => grouping.GroupItems.All(g => g.Key.Title != "Not in collection"));
        }
    }
}
