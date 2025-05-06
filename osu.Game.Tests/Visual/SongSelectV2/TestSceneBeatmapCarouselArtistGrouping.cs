// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

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

            SortBy(SortMode.Artist);
            GroupBy(GroupMode.Artist);

            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupMouse()
        {
            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            CheckHasSelection();

            ClickVisiblePanel<PanelGroup>(0);

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckHasSelection();
        }

        [Test]
        public void TestOpenCloseGroupKeyboard()
        {
            SelectPrevPanel();
            SelectPrevPanel();

            AddUntilStep("some sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckHasSelection();

            Select();

            AddUntilStep("no sets visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckHasSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            Guid selectedID = Guid.Empty;

            RemoveAllBeatmaps();
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextGroup();

            AddStep("record selection", () => selectedID = ((BeatmapInfo)Carousel.CurrentSelection!).ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
            }
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
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
            WaitForGroupSelection(0, 1);

            SelectPrevPanel();
            SelectPrevPanel();
            Select();

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();

            // ensure group expansion doesn't affect selection
            WaitForGroupSelection(0, 1);

            // open first group
            Select();
            WaitForGroupSelection(0, 1);
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

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
