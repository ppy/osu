// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselPageNavigation : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            AddBeatmaps(20, fixedDifficultiesPerSet: 1);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestPageDownMovesSelectionByMultipleItems()
        {
            SelectNextPanel();
            WaitForScrolling();

            CarouselItem? initialSelection = null;
            int? initialIndex = null;

            AddStep("save initial selection", () =>
            {
                initialSelection = GetKeyboardSelectedPanel()?.Item;
                initialIndex = Carousel.GetCarouselItems()?.IndexOf(initialSelection!);
            });

            SelectPageDown();
            WaitForScrolling();

            AddAssert("selection moved forward", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                return currentIndex != null && initialIndex != null && currentIndex > initialIndex;
            });

            AddAssert("selection moved by more than one item", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                return currentIndex != null && initialIndex != null && currentIndex - initialIndex > 1;
            });
        }

        [Test]
        public void TestPageUpMovesSelectionByMultipleItems()
        {
            // Move to an item further down in the list first
            AddRepeatStep("move selection down", () =>
            {
                SelectNextPanel();
            }, 10);
            WaitForScrolling();

            CarouselItem? initialSelection = null;
            int? initialIndex = null;

            AddStep("save initial selection", () =>
            {
                initialSelection = GetKeyboardSelectedPanel()?.Item;
                initialIndex = Carousel.GetCarouselItems()?.IndexOf(initialSelection!);
            });

            SelectPageUp();
            WaitForScrolling();

            AddAssert("selection moved backward", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                return currentIndex != null && initialIndex != null && currentIndex < initialIndex;
            });

            AddAssert("selection moved by more than one item", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                return currentIndex != null && initialIndex != null && initialIndex - currentIndex > 1;
            });
        }

        [Test]
        public void TestPageDownDoesNotWrapAround()
        {
            // Move to the end of the list
            AddStep("scroll to end", () => Scroll.ScrollToEnd(false));

            AddStep("select last item", () =>
            {
                var items = Carousel.GetCarouselItems();
                if (items != null && items.Count > 0)
                {
                    var lastVisible = items.LastOrDefault(i => i.IsVisible);
                    if (lastVisible != null)
                    {
                        // Click the last panel to select it
                        var panel = Carousel.ChildrenOfType<ICarouselPanel>()
                            .FirstOrDefault(p => p.Item == lastVisible);
                        (panel as Panel)?.TriggerClick();
                    }
                }
            });
            WaitForScrolling();

            CarouselItem? selectionBeforePageDown = null;

            AddStep("save selection before page down", () =>
            {
                selectionBeforePageDown = GetKeyboardSelectedPanel()?.Item;
            });

            SelectPageDown();
            WaitForScrolling();

            AddAssert("selection did not wrap to beginning", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                // Selection should remain at or near the end, not wrap to beginning
                return currentIndex >= (Carousel.GetCarouselItems()?.Count ?? 0) / 2;
            });
        }

        [Test]
        public void TestPageUpDoesNotWrapAround()
        {
            SelectNextPanel();
            WaitForScrolling();

            // We're at the first item
            CarouselItem? selectionBeforePageUp = null;

            AddStep("save selection before page up", () =>
            {
                selectionBeforePageUp = GetKeyboardSelectedPanel()?.Item;
            });

            SelectPageUp();
            WaitForScrolling();

            AddAssert("selection did not wrap to end", () =>
            {
                var currentSelection = GetKeyboardSelectedPanel()?.Item;
                int? currentIndex = Carousel.GetCarouselItems()?.IndexOf(currentSelection!);

                // Selection should remain at or near the beginning, not wrap to end
                return currentIndex <= (Carousel.GetCarouselItems()?.Count ?? 0) / 2;
            });
        }
    }
}
