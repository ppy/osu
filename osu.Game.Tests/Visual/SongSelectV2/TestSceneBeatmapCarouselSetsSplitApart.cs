// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselSetsSplitApart : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            SortAndGroupBy(SortMode.Title, GroupMode.Length);
        }

        [Test]
        public void TestSetTraversal()
        {
            AddBeatmaps(3, splitApart: true);
            AddBeatmaps(3, splitApart: false);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(set: 0, diff: 0);

            SelectNextSet();
            WaitForSetSelection(set: 1, diff: 0);

            SelectPrevSet();
            WaitForSetSelection(set: 0, diff: 0);

            SelectPrevSet();
            WaitForSetSelection(set: 5, diff: 0);

            SelectPrevSet();
            SelectPrevSet();
            SelectPrevSet();
            WaitForSetSelection(set: 2, diff: 4);
            AddAssert("only two beatmap panels visible", () => GetVisiblePanels<PanelBeatmap>().Count(), () => Is.EqualTo(2));
        }

        [Test]
        public void TestBeatmapTraversal()
        {
            AddBeatmaps(3, splitApart: true);
            AddBeatmaps(3, splitApart: false);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(set: 0, diff: 0);

            SelectNextPanel();
            WaitForSetSelection(set: 0, diff: 1);

            SelectNextPanel(); // header of set 1 in group 0
            Select();
            WaitForSetSelection(set: 1, diff: 0);

            SelectPrevPanel(); // header of set 1 in group 0
            SelectPrevPanel(); // header of set 0 in group 0
            Select();
            WaitForSetSelection(set: 0, diff: 0);

            SelectPrevPanel(); // header of set 0 in group 0
            SelectPrevPanel(); // header of group 0
            SelectPrevPanel(); // header of group 2
            Select();
            SelectNextPanel(); // header of set 0 in group 2
            Select();
            WaitForSetSelection(set: 0, diff: 4);
        }

        [Test]
        public void TestRandomStaysInGroup()
        {
            AddBeatmaps(2, splitApart: false);
            AddBeatmaps(1, splitApart: true);
            WaitForDrawablePanels();

            SelectPrevSet();
            SelectPrevSet();
            WaitForSetSelection(set: 1);
            WaitForExpandedGroup(2);

            AddStep("select next random", () => Carousel.NextRandom());
            WaitForExpandedGroup(2);
            AddStep("select next random", () => Carousel.NextRandom());
            WaitForExpandedGroup(2);
        }

        protected void AddBeatmaps(int count, bool splitApart) => AddStep($"add {count} beatmaps ({(splitApart ? "" : "not ")}split apart)", () =>
        {
            var beatmapSets = new List<BeatmapSetInfo>();

            for (int i = 0; i < count; i++)
            {
                var beatmapSet = CreateTestBeatmapSetInfo(6, false);

                for (int j = 0; j < beatmapSet.Beatmaps.Count; j++)
                {
                    beatmapSet.Beatmaps[j].Length = splitApart ? 30_000 * (j + 1) : 180_000;
                }

                beatmapSets.Add(beatmapSet);
            }

            BeatmapSets.AddRange(beatmapSets);
        });
    }
}
