// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselNoGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        /// <summary>
        /// Keyboard selection via up and down arrows doesn't actually change the selection until
        /// the select key is pressed.
        /// </summary>
        [Test]
        public void TestKeyboardSelectionKeyRepeat()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();
            CheckNoSelection();

            Select();
            CheckNoSelection();

            AddStep("press down arrow", () => InputManager.PressKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("press up arrow", () => InputManager.PressKey(Key.Up));
            checkSelectionIterating(false);

            AddStep("release down arrow", () => InputManager.ReleaseKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("release up arrow", () => InputManager.ReleaseKey(Key.Up));
            checkSelectionIterating(false);

            Select();
            CheckHasSelection();
        }

        /// <summary>
        /// Keyboard selection via left and right arrows moves between groups, updating the selection
        /// immediately.
        /// </summary>
        [Test]
        public void TestGroupSelectionKeyRepeat()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();
            CheckNoSelection();

            AddStep("press right arrow", () => InputManager.PressKey(Key.Right));
            checkSelectionIterating(true);

            AddStep("press left arrow", () => InputManager.PressKey(Key.Left));
            checkSelectionIterating(true);

            AddStep("release right arrow", () => InputManager.ReleaseKey(Key.Right));
            checkSelectionIterating(true);

            AddStep("release left arrow", () => InputManager.ReleaseKey(Key.Left));
            checkSelectionIterating(false);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();

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
        }

        [Test]
        public void TestTraversalBeyondStart()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(0, 0);
            SelectPrevSet();
            WaitForSetSelection(total_set_count - 1, 0);
        }

        [Test]
        public void TestTraversalBeyondEnd()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            SelectPrevSet();
            WaitForSetSelection(total_set_count - 1, 0);
            SelectNextSet();
            WaitForSetSelection(0, 0);
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextSet();
            WaitForSetSelection(1, 0);

            SelectPrevPanel();
            SelectPrevSet();
            WaitForSetSelection(1, 0);

            SelectPrevPanel();
            SelectNextSet();
            WaitForSetSelection(1, 0);
        }

        [Test]
        public void TestMultipleKeyboardOperationsPerFrame()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(0, 0);

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();

            AddStep("Press two keys at once", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Right);
            });

            // Second key is respected, so only set selection changes.
            WaitForSetSelection(1, 0);

            AddStep("Press two keys at once", () =>
            {
                InputManager.Key(Key.Left);
                InputManager.Key(Key.Up);
            });

            // Second key is respected, so only keyboard selection changes.
            WaitForSetSelection(1, 0);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            CheckNoSelection();

            Select();
            WaitForSetSelection(3, 0);

            SelectNextPanel();
            WaitForSetSelection(3, 1);

            SelectNextPanel();
            WaitForSetSelection(3, 2);

            SelectNextPanel();
            WaitForSetSelection(3, 2);

            Select();
            WaitForSetSelection(4, 0);
        }

        [Test]
        public void TestSingleItemTraversal()
        {
            CheckNoSelection();
            AddBeatmaps(1, 3);

            WaitForSetSelection(0, 0);
            CheckActivationCount(0);

            SelectNextSet();
            WaitForSetSelection(0, 0);

            CheckActivationCount(0);
            CheckRequestPresentCount(0);

            SelectPrevSet();
            WaitForSetSelection(0, 0);

            CheckActivationCount(0);
            CheckRequestPresentCount(0);
        }

        [Test]
        public void TestSingleItemTraversal_DifficultySplit()
        {
            SortBy(SortMode.Difficulty);

            CheckNoSelection();
            AddBeatmaps(1, 1);

            WaitForSetSelection(0, 0);
            CheckActivationCount(0);

            SelectNextSet();
            WaitForSetSelection(0, 0);

            // In the case of a grouped beatmap set, the header gets activated and re-selects the recommended difficulty.
            // This is probably fine.
            CheckActivationCount(0);
            // We don't want it to request present though, which would start gameplay.
            CheckRequestPresentCount(0);

            SelectPrevSet();
            WaitForSetSelection(0, 0);

            CheckActivationCount(0);
            CheckRequestPresentCount(0);
        }

        [Test]
        public void TestEmptyTraversal()
        {
            SelectNextPanel();
            CheckNoSelection();

            SelectNextSet();
            CheckNoSelection();

            SelectPrevPanel();
            CheckNoSelection();

            SelectPrevSet();
            CheckNoSelection();
        }

        [Test]
        public void TestInputHandlingWithinGaps()
        {
            AddBeatmaps(2, 5);
            WaitForDrawablePanels();

            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            ClickVisiblePanelWithOffset<PanelBeatmapSet>(0, new Vector2(0, -(PanelBeatmapSet.HEIGHT / 2 + BeatmapCarousel.SPACING + 1)));

            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            // add lenience to avoid floating-point inaccuracies at edge.
            ClickVisiblePanelWithOffset<PanelBeatmapSet>(0, new Vector2(0, -(PanelBeatmapSet.HEIGHT / 2 - 1)));

            AddUntilStep("wait for beatmaps visible", () => GetVisiblePanels<PanelBeatmap>().Any());
            WaitForSetSelection(0, 0);

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmap>(0, new Vector2(0, -(PanelBeatmap.HEIGHT / 2 + 1)));
            WaitForSetSelection(0, 0);

            ClickVisiblePanelWithOffset<PanelBeatmap>(2, new Vector2(0, (PanelBeatmap.HEIGHT / 2 + 1)));
            WaitForSetSelection(0, 2);

            ClickVisiblePanelWithOffset<PanelBeatmap>(2, new Vector2(0, -(PanelBeatmap.HEIGHT / 2 + 1)));
            WaitForSetSelection(0, 2);

            ClickVisiblePanelWithOffset<PanelBeatmap>(3, new Vector2(0, (PanelBeatmap.HEIGHT / 2 + 1)));
            WaitForSetSelection(0, 3);
        }

        [Test]
        public void TestDifficultySortingWithNoGroups()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SortAndGroupBy(SortMode.Difficulty, GroupMode.None);

            AddUntilStep("standalone panels displayed", () => GetVisiblePanels<PanelBeatmapStandalone>().Any());

            SelectNextSet();
            // both sets have a difficulty with 0.00* star rating.
            // in the case of a tie when sorting, the first tie-breaker is `DateAdded` descending, which will pick the last set added (see `TestResources.CreateTestBeatmapSetInfo()`).
            WaitForSetSelection(1, 0);

            SelectNextSet();
            WaitForSetSelection(0, 0);

            SelectNextPanel();
            Select();
            WaitForSetSelection(1, 1);
        }

        [Test]
        public void TestPanelChangesFromStandaloneToNormal()
        {
            AddBeatmaps(1, 3);
            WaitForDrawablePanels();

            SortBy(SortMode.Difficulty);

            AddUntilStep("standalone panels displayed", () => GetVisiblePanels<PanelBeatmapStandalone>().Count(), () => Is.EqualTo(3));

            WaitForSetSelection(0, 0);

            SortBy(SortMode.Title);

            AddUntilStep("set panel displayed", () => GetVisiblePanels<PanelBeatmapSet>().Count(), () => Is.EqualTo(1));
            AddUntilStep("normal panels displayed", () => GetVisiblePanels<PanelBeatmap>().Count(), () => Is.EqualTo(3));
            AddUntilStep("standalone panels not displayed", () => GetVisiblePanels<PanelBeatmapStandalone>().Count(), () => Is.EqualTo(0));
        }

        [Test]
        public void TestRecommendedSelection()
        {
            AddBeatmaps(5, 3);
            WaitForDrawablePanels();

            AddStep("set recommendation algorithm", () => BeatmapRecommendationFunction = beatmaps => beatmaps.Last());

            SelectPrevSet();

            // check recommended was selected
            SelectNextSet();
            WaitForSetSelection(0, 2);

            // change away from recommended
            SelectPrevPanel();
            Select();
            WaitForSetSelection(0, 1);

            // next set, check recommended
            SelectNextSet();
            WaitForSetSelection(1, 2);

            // next set, check recommended
            SelectNextSet();
            WaitForSetSelection(2, 2);

            // go back to first set and ensure user selection was retained
            // todo: we don't do that yet. not sure if we will continue to have this.
            // SelectPrevSet();
            // SelectPrevSet();
            // WaitForSetSelection(0, 1);
        }

        private void checkSelectionIterating(bool isIterating)
        {
            GroupedBeatmap? selection = null;

            for (int i = 0; i < 3; i++)
            {
                AddStep("store selection", () => selection = Carousel.CurrentGroupedBeatmap);
                if (isIterating)
                    AddUntilStep("selection changed", () => Carousel.CurrentGroupedBeatmap != selection);
                else
                    AddUntilStep("selection not changed", () => Carousel.CurrentGroupedBeatmap == selection);
            }
        }
    }
}
