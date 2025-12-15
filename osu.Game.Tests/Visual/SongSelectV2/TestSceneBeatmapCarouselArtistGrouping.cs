// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselArtistGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            SortAndGroupBy(SortMode.Artist, GroupMode.Artist);

            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionMouse()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<PanelGroup>(0);

            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<PanelGroup>(0);

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionKeyboard()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            SelectNextPanel();
            Select();

            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckNoSelection();

            Select();

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckNoSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            SelectNextSet();

            object? selection = null;

            AddStep("store drawable selection", () => selection = GetSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentGroupedBeatmap));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddBeatmaps(10);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((GroupedBeatmap)selection!).Beatmap.BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentGroupedBeatmap));
            AddUntilStep("drawable selection restored", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("carousel item not visible", GetSelectedPanel, () => Is.Null);

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
            SelectNextSet();
            WaitForBeatmapSelection(0, 1);

            SelectPrevPanel();
            SelectPrevPanel();

            ICarouselPanel? groupPanel = null;

            AddStep("get group panel", () => groupPanel = GetKeyboardSelectedPanel());

            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, () => Is.EqualTo(groupPanel));

            SelectPrevSet();

            WaitForBeatmapSelection(0, 1);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, () => Is.EqualTo(groupPanel));

            SelectPrevSet();

            WaitForBeatmapSelection(0, 1);
            // Expanding a group will move keyboard selection to the selected beatmap if contained.
            AddAssert("keyboard selected panel is expanded", () => groupPanel?.Expanded.Value, () => Is.True);
            AddAssert("keyboard selected panel is beatmap", () => GetKeyboardSelectedPanel()?.Item?.Model, Is.TypeOf<GroupedBeatmap>);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            CheckNoSelection();

            // open first group
            Select();
            CheckNoSelection();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

            SelectNextPanel();
            Select();
            WaitForBeatmapSelection(3, 1);

            SelectNextSet();
            WaitForBeatmapSelection(3, 5);

            SelectNextSet();
            WaitForBeatmapSelection(4, 1);

            SelectPrevSet();
            WaitForBeatmapSelection(3, 5);

            SelectNextSet();
            WaitForBeatmapSelection(4, 1);

            SelectNextSet();
            WaitForBeatmapSelection(4, 5);

            SelectNextSet();
            WaitForBeatmapSelection(0, 1);

            // Difficulties should get immediate selection even when using up and down traversal.
            SelectNextPanel();
            WaitForBeatmapSelection(0, 2);
            SelectNextPanel();
            WaitForBeatmapSelection(0, 3);

            SelectNextPanel();
            WaitForBeatmapSelection(0, 3);

            SelectNextSet();
            WaitForBeatmapSelection(0, 5);

            SelectNextPanel();
            SelectNextSet();
            WaitForBeatmapSelection(1, 1);
        }

        [Test]
        public void TestInputHandlingWithinGaps()
        {
            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            // Clicks just above the first group panel should not actuate any action.
            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 + 1)));

            AddAssert("no sets visible", () => !GetVisiblePanels<PanelBeatmapSet>().Any());

            // add lenience to avoid floating-point inaccuracies at edge.
            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 - 1)));

            AddUntilStep("wait for sets visible", () => GetVisiblePanels<PanelBeatmapSet>().Any());
            CheckNoSelection();

            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            ClickVisiblePanelWithOffset<PanelBeatmapSet>(0, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForBeatmapSelection(0, 1);

            AddUntilStep("wait for beatmaps visible", () => GetVisiblePanels<PanelBeatmap>().Any());

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmap>(0, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForBeatmapSelection(0, 1);

            ClickVisiblePanelWithOffset<PanelBeatmap>(1, new Vector2(0, CarouselItem.DEFAULT_HEIGHT / 2 + 1));
            WaitForBeatmapSelection(0, 2);

            ClickVisiblePanelWithOffset<PanelBeatmapSet>(1, new Vector2(0, CarouselItem.DEFAULT_HEIGHT / 2 + 1));
            WaitForBeatmapSelection(0, 5);
        }

        [Test]
        public void TestBasicFiltering()
        {
            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);

            CheckDisplayedGroupsCount(1);
            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            CheckHasSelection();

            SelectNextPanel();
            Select();
            WaitForBeatmapSelection(0, 2);

            for (int i = 0; i < 6; i++)
                SelectNextPanel();

            Select();

            WaitForBeatmapSelection(0, 3);

            ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);

            CheckDisplayedGroupsCount(5);
            CheckDisplayedBeatmapSetsCount(10);
            CheckDisplayedBeatmapsCount(30);
        }

        [Test]
        public void TestGroupDoesNotExpandAgainOnRefilterIfManuallyCollapsed()
        {
            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);

            CheckDisplayedGroupsCount(1);
            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            CheckHasSelection();

            ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);

            CheckDisplayedGroupsCount(5);
            CheckDisplayedBeatmapSetsCount(10);
            CheckDisplayedBeatmapsCount(30);

            ToggleGroupCollapse();

            ApplyToFilterAndWaitForFilter("apply no-op filter", c => c.AllowConvertedBeatmaps = !c.AllowConvertedBeatmaps);
            AddAssert("group didn't re-expand", () => Carousel.ExpandedGroup, () => Is.Null);

            ToggleGroupCollapse();
            AddAssert("beatmap set re-expanded correctly", () => Carousel.ExpandedBeatmapSet?.BeatmapSet, () => Is.EqualTo(BeatmapSets[2]));

            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[1].Metadata.Title);

            CheckDisplayedGroupsCount(1);
            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            CheckHasSelection();
        }

        [Test]
        public void TestSetDoesExpandAgainWhenGroupingTurnedOff()
        {
            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);

            CheckDisplayedGroupsCount(1);
            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            CheckHasSelection();

            ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);
            CheckDisplayedGroupsCount(5);
            CheckDisplayedBeatmapSetsCount(10);
            CheckDisplayedBeatmapsCount(30);

            ToggleGroupCollapse();

            ApplyToFilterAndWaitForFilter("apply no-op filter", c => c.AllowConvertedBeatmaps = !c.AllowConvertedBeatmaps);
            AddAssert("group didn't re-expand", () => Carousel.ExpandedGroup, () => Is.Null);
            AddAssert("beatmap set didn't re-expand", () => Carousel.GetCarouselItems()!.Count(item => item.Model is GroupedBeatmap && item.IsVisible), () => Is.Zero);

            SortAndGroupBy(SortMode.Title, GroupMode.None);
            AddAssert("beatmap set did re-expand", () => Carousel.GetCarouselItems()!.Count(item => item.Model is GroupedBeatmap && item.IsVisible), () => Is.Not.Zero);
        }

        [Test]
        public void TestManuallyCollapsingCurrentGroupAndOpeningAnother()
        {
            SelectNextSet();
            ToggleGroupCollapse();
            SelectNextGroup();
            AddUntilStep("no beatmap panels visible", () => GetVisiblePanels<PanelBeatmap>().Count(), () => Is.Zero);

            SelectNextSet();
            SelectNextSet();
            AddUntilStep("no beatmap panels visible", () => GetVisiblePanels<PanelBeatmap>().Count(), () => Is.Zero);
        }

        [Test]
        public void TestGroupChangedAfterEngagingArtistGrouping()
        {
            RemoveAllBeatmaps();
            AddStep("add test beatmaps", () =>
            {
                for (int i = 0; i < 5; ++i)
                {
                    var baseTestBeatmap = TestResources.CreateTestBeatmapSetInfo(3);

                    var metadata = new BeatmapMetadata
                    {
                        Artist = $"{(char)('A' + i)} artist",
                        Title = $"{(char)('A' + 4 - i)} title",
                    };

                    foreach (var b in baseTestBeatmap.Beatmaps)
                        b.Metadata = metadata;

                    Realm.Write(r => r.Add(baseTestBeatmap, update: true));
                    BeatmapSets.Add(baseTestBeatmap.Detach());
                }

                SortAndGroupBy(SortMode.Title, GroupMode.Title);
                SelectNextSet();
                SelectNextSet();
                WaitForExpandedGroup(1);

                SortAndGroupBy(SortMode.Artist, GroupMode.Artist);
                WaitForExpandedGroup(3);
            });
        }
    }
}
