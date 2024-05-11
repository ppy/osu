// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface.PageSelector;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestScenePageSelector : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider provider { get; } = new OverlayColourProvider(OverlayColourScheme.Green);

        private readonly PageSelector pageSelector;

        public TestScenePageSelector()
        {
            AddRange(new Drawable[]
            {
                pageSelector = new PageSelector
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });
        }

        [Test]
        public void TestOmittedPages()
        {
            setAvailablePages(100);

            selectPageIndex(0);
            checkVisiblePageNumbers(new[] { 1, 2, 3, 100 });

            selectPageIndex(6);
            checkVisiblePageNumbers(new[] { 1, 5, 6, 7, 8, 9, 100 });

            selectPageIndex(49);
            checkVisiblePageNumbers(new[] { 1, 48, 49, 50, 51, 52, 100 });

            selectPageIndex(99);
            checkVisiblePageNumbers(new[] { 1, 98, 99, 100 });
        }

        [Test]
        public void TestResetCurrentPage()
        {
            setAvailablePages(10);
            selectPageIndex(6);
            setAvailablePages(11);
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 0);
        }

        [Test]
        public void TestOutOfBoundsSelection()
        {
            setAvailablePages(10);
            selectPageIndex(11);
            AddAssert("Page 10 is current", () => pageSelector.CurrentPage.Value == pageSelector.AvailablePages.Value - 1);

            selectPageIndex(-1);
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 0);
        }

        private void checkVisiblePageNumbers(int[] expected) => AddAssert($"Sequence is {string.Join(',', expected.Select(i => i.ToString()))}", () => pageSelector.ChildrenOfType<PageSelectorPageButton>().Select(p => p.PageNumber).SequenceEqual(expected));

        private void selectPageIndex(int pageIndex) =>
            AddStep($"Select page {pageIndex}", () => pageSelector.CurrentPage.Value = pageIndex);

        private void setAvailablePages(int availablePages) =>
            AddStep($"Set available pages to {availablePages}", () => pageSelector.AvailablePages.Value = availablePages);
    }
}
