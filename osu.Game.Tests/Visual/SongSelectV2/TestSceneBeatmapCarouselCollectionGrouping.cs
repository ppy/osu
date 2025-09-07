// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Collections;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselCollectionGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            AddBeatmaps(10, 3);

            AddStep("set up collections", () =>
            {
                List<BeatmapCollection> collections =
                [
                    new BeatmapCollection("collection one", [
                        ..BeatmapSets[0].Beatmaps.Select(b => b.MD5Hash),
                        ..BeatmapSets[1].Beatmaps.Select(b => b.MD5Hash),
                        ..BeatmapSets[2].Beatmaps.Select(b => b.MD5Hash),
                        BeatmapSets[5].Beatmaps[1].MD5Hash,
                        BeatmapSets[8].Beatmaps[0].MD5Hash,
                    ]),
                    new BeatmapCollection("collection two", [
                        BeatmapSets[0].Beatmaps[0].MD5Hash,
                        ..BeatmapSets[1].Beatmaps.Select(b => b.MD5Hash),
                        ..BeatmapSets[2].Beatmaps.Select(b => b.MD5Hash),
                        BeatmapSets[6].Beatmaps[2].MD5Hash,
                        BeatmapSets[8].Beatmaps[2].MD5Hash,
                    ]),
                    new BeatmapCollection("collection one copy", [
                        ..BeatmapSets[0].Beatmaps.Select(b => b.MD5Hash),
                        ..BeatmapSets[1].Beatmaps.Select(b => b.MD5Hash),
                        ..BeatmapSets[2].Beatmaps.Select(b => b.MD5Hash),
                        BeatmapSets[5].Beatmaps[1].MD5Hash,
                        BeatmapSets[8].Beatmaps[0].MD5Hash,
                    ]),
                ];
                Carousel.AllCollections = () => collections;
            });

            SortAndGroupBy(SortMode.Title, GroupMode.Collections);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestMultipleCopiesOfBeatmapsPresent()
        {
            CheckDisplayedGroupsCount(4); // one for each collection, plus no collections
            // all three collections have beatmaps from 5 beatmap sets
            // 7 beatmap sets have beatmaps which belong to no collection
            CheckDisplayedBeatmapSetsCount(5 + 5 + 5 + 7);
        }
    }
}
