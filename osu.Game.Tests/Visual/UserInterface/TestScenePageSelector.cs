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
    public class TestScenePageSelector : OsuTestScene
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

            AddAssert("Correct page buttons", () => pageSelector.ChildrenOfType<PageSelectorPageButton>().Select(p => p.PageNumber).SequenceEqual(new[] { 1, 2, 3, 100 }));
        }

        [Test]
        public void TestResetCurrentPage()
        {
            setAvailablePages(10);
            selectPage(6);
            setAvailablePages(11);
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 0);
        }

        [Test]
        public void TestOutOfBoundsSelection()
        {
            setAvailablePages(10);
            selectPage(11);
            AddAssert("Page 10 is current", () => pageSelector.CurrentPage.Value == pageSelector.AvailablePages.Value - 1);

            selectPage(-1);
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 0);
        }

        private void selectPage(int pageIndex) =>
            AddStep($"Select page {pageIndex}", () => pageSelector.CurrentPage.Value = pageIndex);

        private void setAvailablePages(int availablePages) =>
            AddStep($"Set available pages to {availablePages}", () => pageSelector.AvailablePages.Value = availablePages);
    }
}
