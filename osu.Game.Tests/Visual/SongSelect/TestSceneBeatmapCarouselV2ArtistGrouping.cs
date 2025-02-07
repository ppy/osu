// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2ArtistGrouping : BeatmapCarouselV2TestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            SortBy(new FilterCriteria { Group = GroupMode.Artist, Sort = SortMode.Artist });

            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionMouse()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<GroupPanel>(0);

            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<GroupPanel>(0);

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionKeyboard()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            SelectNextPanel();
            Select();

            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckNoSelection();

            Select();

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckNoSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            SelectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = GetSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddBeatmaps(10);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));
            AddUntilStep("drawable selection restored", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("carousel item not visible", GetSelectedPanel, () => Is.Null);

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
            SelectNextGroup();
            WaitForGroupSelection(0, 1);

            SelectPrevPanel();
            SelectPrevPanel();

            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            SelectPrevGroup();

            WaitForGroupSelection(0, 1);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            SelectPrevGroup();

            WaitForGroupSelection(0, 1);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
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
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<BeatmapSetPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

            SelectNextPanel();
            Select();
            WaitForGroupSelection(3, 1);

            SelectNextGroup();
            WaitForGroupSelection(3, 5);

            SelectNextGroup();
            WaitForGroupSelection(4, 1);

            SelectPrevGroup();
            WaitForGroupSelection(3, 5);

            SelectNextGroup();
            WaitForGroupSelection(4, 1);

            SelectNextGroup();
            WaitForGroupSelection(4, 5);

            SelectNextGroup();
            WaitForGroupSelection(0, 1);

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();

            SelectNextGroup();
            WaitForGroupSelection(0, 1);

            SelectNextPanel();
            SelectNextGroup();
            WaitForGroupSelection(1, 1);
        }
    }
}
