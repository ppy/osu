// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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
        public void TestResetCurrentPage()
        {
            AddStep("Set 10 pages", () => setMaxPages(10));
            AddStep("Select page 5", () => setCurrentPage(5));
            AddStep("Set 11 pages", () => setMaxPages(11));
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 1);
        }

        [Test]
        public void TestOutOfBoundsSelection()
        {
            AddStep("Set 10 pages", () => setMaxPages(10));
            AddStep("Select page 11", () => setCurrentPage(11));
            AddAssert("Page 10 is current", () => pageSelector.CurrentPage.Value == pageSelector.MaxPages.Value);

            AddStep("Select page -1", () => setCurrentPage(-1));
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 1);
        }

        [Test]
        public void TestNegativeMaxPages()
        {
            AddStep("Set -10 pages", () => setMaxPages(-10));
            AddAssert("Page 1 is current", () => pageSelector.CurrentPage.Value == 1);
            AddAssert("Max is 1", () => pageSelector.MaxPages.Value == 1);
        }

        private void setMaxPages(int maxPages) => pageSelector.MaxPages.Value = maxPages;

        private void setCurrentPage(int currentPage) => pageSelector.CurrentPage.Value = currentPage;
    }
}
